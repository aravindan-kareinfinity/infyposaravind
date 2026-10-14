using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;
using System.Text;
using InfyPOS.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using WebAPI.Data;
using PdfSharp.Pdf.IO;

namespace InfyPOS
{
    public class PDFGenerator : ReportData
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, Status> pdfqueue = new System.Collections.Concurrent.ConcurrentDictionary<string, Status>();
        internal Status QueueStatus(string key)
        {
            Status status = null;
            if (pdfqueue.TryGetValue(key, out status))
                return status;
            return new Status()
            {
                key = key,
                message = "Processing"
            };
        }
        public string Initiate(System.Web.HttpContext context, MfgGalleryCreteria.ImagePDFRequest item)
        {
            Status status = new Status() { context = context, config = item };
            status.key = WebAPI.Data.Token.RandomString(16);
            while (!pdfqueue.TryAdd(status.key, status))
            {
                System.Threading.Thread.Sleep(10);
            }
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Process));
            thread.Start(status);
            return status.key;
        }

        public void Process(object soure)
        {
            Status status = soure as Status;
            System.Threading.Thread.Sleep(100);
            try
            {
                var pdfbytes = ImagePDFGeneration(status.config);
                status.context.Cache.Add("PDF-" + status.key, pdfbytes, null, DateTime.Now.AddMinutes(2), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Low, null);
                status.completed = true;
            }
            catch (Exception exp)
            {
                status.failed = true;
                status.message = exp.Message;
            }
        }
        public class Status
        {
            [JsonIgnore]
            public MfgGalleryCreteria.ImagePDFRequest config { get; set; }
            [JsonIgnore]
            public System.Web.HttpContext context { get; set; }
            public string key { get; set; }
            public bool completed { get; set; }
            public bool failed { get; set; }
            public string message { get; set; }

        }
        public byte[] ImagePDFGeneration(List<Courier> item)
        {
            return ImagePDFGeneration(this.Context.OrganizationID, item);
        }

        public byte[] ImagePDFGeneration(long organizationid, List<Courier> item)
        {
            var pdfDocument = new PdfSharp.Pdf.PdfDocument();
            //pdfDocument.Info.Title = Title;
            //pdfDocument.Info.Author = Author;
            //pdfDocument.Info.Subject = Subject;
            //pdfDocument.Info.Keywords = Keywords;

            ImageData imageData = new ImageData();
            imageData.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            titleFormat.Alignment = StringAlignment.Center;
            StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            var titlefont = new Font("verdana", 9);
            var textfont = new Font("verdana", 8);
            StringBuilder titlesb = new StringBuilder();

            foreach (var courier in item)
            {
                string title = courier.name + "/" + courier.podnumber + "(" + courier.poddate.ToString("dd-MM-yyyy") + ")";

                for (var i = 0; i < courier.attachment.Count; i++)
                {
                    PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                    page.Size = PdfSharp.PageSize.A4;

                    var availablewidth = page.Width.Point - 50;
                    var availableheight = page.Height.Point - 50;


                    var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
                    var currenty = 25.0;

                    DrawString(gfx, title,
                                titlefont,
                                Brushes.Black, new RectangleF(10,
                                10, (float)availablewidth,
                                (float)25), titleFormat);
                    currenty += 10;

                    var imagewidth = (availablewidth - 50);
                    var imageheight = (availableheight - 50);

                    var file = imageData.GetData(courier.attachment[i].image.LongFromBase62());
                    if (file != null)
                    {
                        var filestream = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                            (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, 70);
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                        {
                            var image = System.Drawing.Image.FromStream(ms);
                            gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                                new PdfSharp.Drawing.XPoint(20, currenty));
                        }
                        filestream = null;
                        file.data = null;
                        file = null;
                    }
                    gfx.Save();
                }
            }

            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                pdfDocument.Save(output, false);
                pdfDocument.Close();
                return output.ToArray();
            }
        }

        //public byte[] ImagePDFGeneration(long organizationid, List<Courier> item)
        //{
        //    var pdfDocument = new PdfSharp.Pdf.PdfDocument();
        //    //pdfDocument.Info.Title = Title;
        //    //pdfDocument.Info.Author = Author;
        //    //pdfDocument.Info.Subject = Subject;
        //    //pdfDocument.Info.Keywords = Keywords;

        //    ImageData imageData = new ImageData();
        //    imageData.Context = new UserClaim()
        //    {
        //        OrganizationID = organizationid
        //    };
        //    StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
        //    titleFormat.LineAlignment = StringAlignment.Center;
        //    titleFormat.Alignment = StringAlignment.Center;
        //    StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
        //    titleFormat.LineAlignment = StringAlignment.Center;
        //    var titlefont = new Font("verdana", 9);
        //    var textfont = new Font("verdana", 8);
        //    StringBuilder titlesb = new StringBuilder();

        //    foreach (var courier in item)
        //    {
        //        string title = courier.name + "/" + courier.podnumber + "(" + courier.poddate.ToString("dd-MM-yyyy") + ")";

        //        for (var i = 0; i < courier.attachment.Count; i++)
        //        {
        //            PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
        //            page.Size = PdfSharp.PageSize.A4;

        //            var availablewidth = page.Width.Point - 50;
        //            var availableheight = page.Height.Point - 50;


        //            var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
        //            var currenty = 25.0;

        //            DrawString(gfx, title,
        //                        titlefont,
        //                        Brushes.Black, new RectangleF(10,
        //                        10, (float)availablewidth,
        //                        (float)25), titleFormat);
        //            currenty += 10;

        //            var imagewidth = (availablewidth - 50);
        //            var imageheight = (availableheight - 50);

        //            var file = imageData.GetData(courier.attachment[i].image.LongFromBase62());
        //            if (file != null)
        //            {
        //                var filestream = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
        //                    (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, 70);
        //                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
        //                {
        //                    var image = System.Drawing.Image.FromStream(ms);
        //                    gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
        //                        new PdfSharp.Drawing.XPoint(20, currenty));
        //                }
        //                filestream = null;
        //                file.data = null;
        //                file = null;
        //            }
        //            gfx.Save();
        //        }
        //    }

        //    using (System.IO.MemoryStream output = new System.IO.MemoryStream())
        //    {
        //        pdfDocument.Save(output, false);
        //        pdfDocument.Close();
        //        return output.ToArray();
        //    }
        //}

        public byte[] ImagePDFGeneration(AnalyserDataService.ImagePDFRequest item)
        {
            return ImagePDFGeneration(this.Context.OrganizationID, item);
        }

        internal byte[] ImagePDFGeneration(long organizationid, List<JobWorkImage> list)
        {

            var pdfDocument = new PdfSharp.Pdf.PdfDocument();
            //pdfDocument.Info.Title = Title;
            //pdfDocument.Info.Author = Author;
            //pdfDocument.Info.Subject = Subject;
            //pdfDocument.Info.Keywords = Keywords;

            ImageData imageData = new ImageData();
            imageData.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            titleFormat.Alignment = StringAlignment.Center;
            StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            var titlefont = new Font("verdana", 9);
            var textfont = new Font("verdana", 8);
            StringBuilder titlesb = new StringBuilder();

            foreach (var item in list.ChunkBy(4))
            {
                PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                float availablewidth = (float)page.Width.Point - 50;
                float availableheight = (float)page.Height.Point - 50;

                float imageheight = availableheight / 4;
                float imagewidth = availablewidth / 4;
                float contentwidth = availablewidth / 2;

                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
                float currenty = 25;


                for (var i = 0; i < item.Count; i++)
                {
                    float currentx = 25;
                    currenty = 25 + (i * imageheight);
                    var ditem = item[i];
                    DrawString(gfx, i.ToString() + "-" + ditem.supplier,
                                titlefont,
                                Brushes.Black, new RectangleF(currentx,
                                currenty, (float)contentwidth,
                                (float)25), titleFormat);
                    currenty += 25;

                    DrawString(gfx, ditem.invoiceno + "(" + ditem.entrydate.ToString("dd-MM-yyyy") + ")",
                                titlefont,
                                Brushes.Black, new RectangleF(currentx,
                                currenty, (float)contentwidth,
                                (float)25), titleFormat);
                    currenty += 25;

                    if (!string.IsNullOrEmpty(ditem.toinvoiceno))
                    {
                        DrawString(gfx, ditem.barcode + " -> " + ditem.tobarcode,
                                    titlefont,
                                    Brushes.Black, new RectangleF(currentx,
                                    currenty, (float)contentwidth,
                                    (float)25), titleFormat);
                        currenty += 25;

                        DrawString(gfx, ditem.toinvoiceno + "(" + ditem.toentrydate.ToString("dd-MM-yyyy") + ")",
                                    titlefont,
                                    Brushes.Black, new RectangleF(currentx,
                                    currenty, (float)contentwidth,
                                    (float)25), titleFormat);
                        currenty += 25;
                    }
                    else
                    {
                        DrawString(gfx, ditem.barcode,
                            titlefont,
                            Brushes.Black, new RectangleF(currentx,
                            currenty, (float)contentwidth,
                            (float)25), titleFormat);
                        currenty += 25;
                    }

                    currenty = 25 + (i * imageheight);
                    currentx += contentwidth;
                    if (!string.IsNullOrEmpty(ditem.imageid))
                    {
                        var file = imageData.GetData(ditem.imageid.LongFromBase62());
                        if (file != null)
                        {
                            var filestream = Resize(file.data,
                                (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                                (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, 70);
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                            {
                                var image = System.Drawing.Image.FromStream(ms);
                                gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                                    new PdfSharp.Drawing.XPoint(currentx, currenty));
                            }
                            filestream = null;
                            file.data = null;
                            file = null;
                        }
                    }

                    currentx += imagewidth;
                    if (!string.IsNullOrEmpty(ditem.toimageid))
                    {
                        var file = imageData.GetData(ditem.toimageid.LongFromBase62());
                        if (file != null)
                        {
                            var filestream = Resize(file.data,
                                (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                                (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, 70);
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                            {
                                var image = System.Drawing.Image.FromStream(ms);
                                gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                                    new PdfSharp.Drawing.XPoint(currentx + 10, currenty));
                            }
                            filestream = null;
                            file.data = null;
                            file = null;
                        }
                    }
                }

                gfx.Save();
            }

            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                pdfDocument.Save(output, false);
                pdfDocument.Close();
                return output.ToArray();
            }

        }

        public byte[] ImagePDFGeneration(long organizationid, AnalyserDataService.ImagePDFRequest item)
        {
            var pdfDocument = new PdfSharp.Pdf.PdfDocument();
            //pdfDocument.Info.Title = Title;
            //pdfDocument.Info.Author = Author;
            //pdfDocument.Info.Subject = Subject;
            //pdfDocument.Info.Keywords = Keywords;

            ImageData imageData = new ImageData();
            imageData.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            titleFormat.Alignment = StringAlignment.Center;
            StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            var titlefont = new Font("verdana", 9);
            var textfont = new Font("verdana", 8);
            StringBuilder titlesb = new StringBuilder();
            item.datasource.excludeColumns = new List<int>();
            for (var column = 0; column < item.datasource.additionalcolumns.Count; column++)
            {
                if (item.datasource.items.Count > 0)
                {
                    if (item.datasource.additionalcolumns[column].headername != "BARCODE" &&
                        (item.datasource.additionalcolumns[column].headername == "PRODUCT" ||
                        item.datasource.additionalcolumns[column].headername == "BRAND" ||
                        item.datasource.additionalcolumns[column].headername == "SUPPLIER"))
                    {
                        var coldispindex = item.datasource.additionalcolumns[column].index;
                        var firstvalue = item.datasource.items.First().String[coldispindex];

                        if (item.datasource.items.All(e => e.String[coldispindex] == firstvalue))
                        {
                            item.datasource.excludeColumns.Add(coldispindex);
                            if (titlesb.Length > 0) titlesb.Append("/");
                            titlesb.AppendFormat("{0} : {1}", item.datasource.additionalcolumns[column].headername,
                                item.datasource.items.First().String[item.datasource.additionalcolumns[column].index]);
                        }
                    }
                }
            }

            for (var column = 0; column < item.datasource.columns.Count; column++)
            {

                if (item.datasource.columns[column] != "ImageID" &&
                item.datasource.columns[column].ToUpper() != "BARCODE" &&
                (item.datasource.columns[column].ToUpper() == "PRODUCT" ||
                item.datasource.columns[column].ToUpper() == "BRAND" ||
                item.datasource.columns[column].ToUpper() == "SUPPLIER"))
                {
                    if (!item.datasource.excludeColumns.Contains(column))
                    {
                        var firstvalue = item.datasource.items.First().String[column];

                        if (item.datasource.items.All(e => e.String[column] == firstvalue))
                        {
                            item.datasource.excludeColumns.Add(column);
                            if (titlesb.Length > 0) titlesb.Append("/");
                            titlesb.AppendFormat("{0} : {1}", item.datasource.columns[column],
                                item.datasource.items.First().String[column]);
                        }
                    }
                }
            }

            for (var i = 0; i < item.datasource.items.Count; i = i + (item.ColumnCount * item.RowCount))
            {
                int rindex = 0;
                PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                var availablewidth = page.Width.Point - 50;
                var availableheight = page.Height.Point - 50;
                var descriptionwidth = 0.0;
                var descriptionheight = 0.0;
                if (item.HasColumnLabel)
                    descriptionwidth = 100;
                else if (item.HasRowLabel)
                    descriptionheight = item.ColumnCount > 3 ? descriptionheight = 50 : 30;

                var imagewidth = (availablewidth - (item.HasColumnLabel ? item.ColumnCount * descriptionwidth : 0)) / item.ColumnCount;
                var imageheight = (availableheight - (item.HasRowLabel ? item.RowCount * descriptionheight : 0)) / item.RowCount;

                if (item.HasColumnLabel)
                    descriptionheight = imageheight;
                else if (item.HasRowLabel)
                    descriptionwidth = imagewidth;

                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
                var currenty = 25.0;
                if (titlesb.Length > 0)
                {
                    DrawString(gfx, titlesb.ToString(),
                                titlefont,
                                Brushes.Black, new RectangleF(10,
                                10, (float)availablewidth,
                                (float)25), titleFormat);
                    currenty += 10;
                }

                for (var r = i; r < item.datasource.items.Count && rindex < item.RowCount; r = r + item.ColumnCount)
                {
                    int cindex = 0;
                    var currentx = 25.0;
                    for (var j = r; j < item.datasource.items.Count && cindex < item.ColumnCount; j++)
                    {
                        var value = item.datasource.items[j];
                        var additionaly = 0.0;


                        if (item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Left ||
                            item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Top)
                        {
                            gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty,
                                descriptionwidth, descriptionheight));

                            var sb = new StringBuilder();
                            for (var column = 0; column < item.datasource.columns.Count; column++)
                            {
                                if (item.datasource.columns[column] != "ImageID" &&
                                    !item.datasource.excludeColumns.Contains(column))
                                    sb.AppendFormat("{0} : {1}\r\n", item.datasource.columns[column], value.String[column]);
                            }
                            for (var column = 0; column < item.datasource.additionalcolumns.Count; column++)
                            {
                                if (!item.datasource.excludeColumns.Contains(item.datasource.additionalcolumns[column].index))
                                    sb.AppendFormat("{0} : {1}\r\n", item.datasource.additionalcolumns[column].headername,
                                        value.String[item.datasource.additionalcolumns[column].index]);
                            }


                            sb.AppendFormat("{0} : {1}\r\n", "Qty", Math.Round(value.qty, 0));
                            sb.AppendFormat("{0} : {1}\r\n", "Rate", Math.Round(value.iqty));
                            if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                                sb.AppendFormat("{0} : {1}\r\n", "Cost", Math.Round(value.cvalue));
                            sb.AppendFormat("{0} : {1}\r\n", "Price", Math.Round(value.value));


                            DrawString(gfx, sb.ToString(),
                                                textfont,
                                                Brushes.Black, new RectangleF((float)currentx,
                                                (float)currenty, (float)descriptionwidth,
                                                (float)descriptionheight), stringFormat);

                            if (item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Left)
                                currentx += descriptionwidth;
                            else
                                additionaly = descriptionheight;
                            sb.Clear();
                        }

                        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx,
                            additionaly + currenty,
     imagewidth,
     imageheight));
                        if (!string.IsNullOrEmpty(value.String[0]))
                        {
                            var file = imageData.GetData(value.String[0].LongFromBase62());
                            if (file != null)
                            {
                                var filestream = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                                    (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, 70);
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                                {
                                    var image = System.Drawing.Image.FromStream(ms);
                                    var leftmargin = image.Width < image.Height ? ((image.Height - image.Width / 2)) : 0;
                                    var topmargin = image.Width > image.Height ? ((image.Width - image.Height) / 2) : 0;

                                    gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                                        new PdfSharp.Drawing.XPoint(currentx, currenty + additionaly));
                                }
                                filestream = null;
                                file.data = null;
                                file = null;
                            }
                        }

                        if (item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Right ||
                            item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Left ||
                            item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Top ||
                            item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.None)
                            currentx += imagewidth;

                        if (item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Bottom)
                            additionaly = imageheight;

                        if (item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Right ||
                            item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Bottom)
                        {
                            gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx,
                                additionaly + currenty,
                                descriptionwidth, descriptionheight));

                            var sb = new StringBuilder();
                            for (var column = 0; column < item.datasource.columns.Count; column++)
                            {
                                if (item.datasource.columns[column] != "ImageID")
                                    sb.AppendFormat("{0} : {1}\r\n", item.datasource.columns[column], value.String[column]);
                            }
                            for (var column = 0; column < item.datasource.additionalcolumns.Count; column++)
                            {
                                if (!item.datasource.excludeColumns.Contains(item.datasource.additionalcolumns[column].index))
                                    sb.AppendFormat("{0} : {1}\r\n", item.datasource.additionalcolumns[column].headername,
                                        value.String[item.datasource.additionalcolumns[column].index]);
                            }


                            sb.AppendFormat("{0} : {1}\r\n", "Qty", Math.Round(value.qty, 0));
                            sb.AppendFormat("{0} : {1}\r\n", "Rate", Math.Round(value.iqty));
                            if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                                sb.AppendFormat("{0} : {1}\r\n", "Cost", Math.Round(value.cvalue));
                            sb.AppendFormat("{0} : {1}\r\n", "Price", Math.Round(value.value));


                            DrawString(gfx, sb.ToString(),
                                textfont,
                                Brushes.Black, new RectangleF((float)currentx,
                                (float)(currenty + additionaly), (float)descriptionwidth,
                                (float)descriptionheight), stringFormat);

                            if (item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Right ||
                                item.Detail == AnalyserDataService.ImagePDFRequest.EnumDetail.Bottom)
                                currentx += descriptionwidth;
                            sb.Clear();
                        }

                        cindex++;
                    }
                    currenty += imageheight;
                    if (item.HasRowLabel)
                        currenty += descriptionheight;
                    rindex++;
                }
                gfx.Save();
            }

            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                pdfDocument.Save(output, false);
                pdfDocument.Close();
                return output.ToArray();
            }
        }



        private Image ResizeImage(Image img, Rectangle rect)
        {
            Bitmap bmPhoto = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(128, 128);
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.DrawImage(img,
                    new Rectangle(0, 0, rect.Width, rect.Height),
                    new Rectangle(0, 0, img.Width, img.Height),
                    GraphicsUnit.Pixel);
            grPhoto.Dispose();
            return bmPhoto;
        }
        public XStringFormat Convert(StringFormat format)
        {
            XStringFormat xformat = new XStringFormat();
            xformat.Alignment = (XStringAlignment)format.Alignment;
            xformat.LineAlignment = (XLineAlignment)format.LineAlignment;
            return xformat;
        }
        public void DrawString(PdfSharp.Drawing.XGraphics gfx,
            string s, Font font, Brush brush, RectangleF layoutRectangle,
            StringFormat format)
        {
            if (string.IsNullOrEmpty(s)) return;
            PdfSharp.Drawing.Layout.XTextFormatter tf;
            tf = new PdfSharp.Drawing.Layout.XTextFormatter(gfx);
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);
            if (format.Alignment == StringAlignment.Near)
                layoutRectangle.Offset(3, 2);
            else if (format.Alignment == StringAlignment.Center)
                layoutRectangle.Offset(0, 2);
            else
                layoutRectangle.Offset(-3, 2);

            if (font.Name.Contains("Times"))
            {
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                XFont xfont = new XFont(font.Name, font.Size, (XFontStyle)font.Style, options);
                gfx.DrawString(s, xfont, brush, layoutRectangle, Convert(format));
                //tf.DrawString(s, xfont, brush, layoutRectangle, XStringFormats.TopLeft);
            }
            else
            {
                var size = gfx.Graphics.MeasureString(s, font);
                if (size.Width > layoutRectangle.Width)
                {

                    int charactersOnPage = 0;
                    int linesPerPage = 0;
                    size = gfx.Graphics.MeasureString(s, font, new SizeF(layoutRectangle.Width, layoutRectangle.Height),
                        format, out charactersOnPage, out linesPerPage);

                    if (size.Height < layoutRectangle.Height && !(s.Substring(0, charactersOnPage).IndexOf(' ') < 0 &&
                        charactersOnPage == s.Length && linesPerPage > 1))
                    {
                        var fmt = Convert(format);
                        fmt.LineAlignment = XLineAlignment.Near;
                        fmt.Alignment = XStringAlignment.Near;
                        //layoutRectangle.Offset(0, 11);
                        tf.DrawString(s, font, brush, layoutRectangle, fmt);
                        return;
                    }
                    else
                    {
                        size = gfx.Graphics.MeasureString(s, font);
                        while ((size.Width + 5) > layoutRectangle.Width && size.Width > 5)
                        {
                            font = new Font(font.Name, (float)(font.Size - 0.5), font.Style, GraphicsUnit.World);
                            size = gfx.Graphics.MeasureString(s, font, new SizeF(layoutRectangle.Width, layoutRectangle.Height),
                        format, out charactersOnPage, out linesPerPage);

                            if (size.Height < layoutRectangle.Height && !(s.Substring(0, charactersOnPage).IndexOf(' ') < 0 &&
                                charactersOnPage == s.Length && linesPerPage > 1))
                            {
                                var fmt = Convert(format);
                                fmt.LineAlignment = XLineAlignment.Near;
                                fmt.Alignment = XStringAlignment.Near;
                                //layoutRectangle.Offset(0, 11);
                                tf.DrawString(s, font, brush, layoutRectangle, fmt);
                                return;
                            }
                            size = gfx.Graphics.MeasureString(s, font);
                        }
                    }
                }

                var csize = gfx.MeasureString(s, font);
                if (csize.Width <= layoutRectangle.Width && format.Alignment == StringAlignment.Near && format.LineAlignment == StringAlignment.Near)
                    tf.DrawString(s, font, brush, layoutRectangle, Convert(format));
                else
                    gfx.DrawString(s, font, brush, layoutRectangle, Convert(format));
            }

        }
        private ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().SingleOrDefault(c => c.FormatID == format.Guid);
        }
        public Tuple<int, int> Resize(byte[] bytes, int maxWidth, int maxHeight)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                var image = System.Drawing.Image.FromStream(ms);
                // Get the image's original width and height
                int originalWidth = image.Width;
                int originalHeight = image.Height;

                // To preserve the aspect ratio
                float ratioX = (float)maxWidth / (float)originalWidth;
                float ratioY = (float)maxHeight / (float)originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                float sourceRatio = (float)originalWidth / originalHeight;

                // New width and height based on aspect ratio
                int newWidth = (int)(originalWidth * ratio);
                int newHeight = (int)(originalHeight * ratio);
                return Tuple.Create(newWidth, newHeight);
            }
        }
        public byte[] Resize(byte[] bytes, int maxWidth, int maxHeight, int quality)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
            {
                var image = System.Drawing.Image.FromStream(ms);
                // Get the image's original width and height
                int originalWidth = image.Width;
                int originalHeight = image.Height;

                // To preserve the aspect ratio
                float ratioX = (float)maxWidth / (float)originalWidth;
                float ratioY = (float)maxHeight / (float)originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                float sourceRatio = (float)originalWidth / originalHeight;

                // New width and height based on aspect ratio
                int newWidth = (int)(originalWidth * ratio);
                int newHeight = (int)(originalHeight * ratio);

                // Convert other formats (including CMYK) to RGB.
                Bitmap newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);

                // Draws the image in the specified size with quality mode set to HighQuality
                using (Graphics graphics = Graphics.FromImage(newImage))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                return WebAPI.Common.ImageManager.ToByteArray(newImage, quality);

                //ImageCodecInfo imageCodecInfo = this.GetEncoderInfo(ImageFormat.Png);


                //// Create an Encoder object for the Quality parameter.
                //System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

                //// Create an EncoderParameters object. 
                //EncoderParameters encoderParameters = new EncoderParameters(1);

                //// Save the image as a JPEG file with quality level.
                //EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                //encoderParameters.Param[0] = encoderParameter;
                //using (System.IO.MemoryStream output = new System.IO.MemoryStream())
                //{
                //    newImage.Save(output, imageCodecInfo, encoderParameters);
                //    return output.ToArray();
                //}
            }
        }

        internal byte[] ImagePDFGeneration(List<RetailReport> records)
        {
            var pdfDocument = new PdfSharp.Pdf.PdfDocument();
            //pdfDocument.Info.Title = Title;
            //pdfDocument.Info.Author = Author;
            //pdfDocument.Info.Subject = Subject;
            //pdfDocument.Info.Keywords = Keywords;

            ImageData imageData = new ImageData();
            imageData.Context = new UserClaim()
            {
                OrganizationID = this.Context.OrganizationID
            };
            StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;

            StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            var titlefont = new Font("verdana", 9);
            var textfont = new Font("verdana", 8);
            StringBuilder titlesb = new StringBuilder();
            bool titlerendered = false;
            foreach (var item in records.ChunkBy(4))
            {
                PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                float availablewidth = (float)page.Width.Point - 50;
                float availableheight = (float)page.Height.Point - 50;

                float imageheight = availableheight / 4;
                float imagewidth = availablewidth / 3;
                float contentwidth = availablewidth / 2;

                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
                float currenty = 30;
                if (!titlerendered)
                {
                    titleFormat.Alignment = StringAlignment.Center;
                    DrawString(gfx, "Approve Bill Issued , Billed & Return Report",
                                        titlefont,
                                        Brushes.Black, new RectangleF(5,
                                        5, (float)contentwidth,
                                        (float)25), titleFormat);
                    titleFormat.Alignment = StringAlignment.Near;
                    titlerendered = true;
                }
                for (var i = 0; i < item.Count; i++)
                {
                    float currentx = 25;
                    currenty = 25 + (i * imageheight);
                    var ditem = item[i];

                    if (!string.IsNullOrEmpty(ditem.billno) && !string.IsNullOrEmpty(ditem.returnbillno))
                    {
                        DrawString(gfx, ditem.billno + "->" + ditem.returnbillno + "(" + ditem.billdate.ToString("dd-MM-yyyy") + ")",
                                    titlefont,
                                    Brushes.Black, new RectangleF(currentx,
                                    currenty, (float)contentwidth,
                                    (float)25), titleFormat);
                    }
                    else if (!string.IsNullOrEmpty(ditem.billno))
                    {
                        DrawString(gfx, ditem.billno + "(" + ditem.billdate.ToString("dd-MM-yyyy") + ")",
                                   titlefont,
                                   Brushes.Black, new RectangleF(currentx,
                                   currenty, (float)contentwidth,
                                   (float)25), titleFormat);
                    }
                    else
                    {
                        DrawString(gfx, ditem.returnbillno + "(" + ditem.billdate.ToString("dd-MM-yyyy") + ")",
                                   titlefont,
                                   Brushes.Black, new RectangleF(currentx,
                                   currenty, (float)contentwidth,
                                   (float)25), titleFormat);
                    }
                    currenty += 25;

                    DrawString(gfx, ditem.customer,
            titlefont,
            Brushes.Black, new RectangleF(currentx,
            currenty, (float)contentwidth,
            (float)25), titleFormat);
                    currenty += 25;


                    DrawString(gfx, ditem.suppliername,
                                titlefont,
                                Brushes.Black, new RectangleF(currentx,
                                currenty, (float)contentwidth,
                                (float)25), titleFormat);
                    currenty += 25;

                    DrawString(gfx, ditem.productname + "/" + ditem.brandname,
                                titlefont,
                                Brushes.Black, new RectangleF(currentx,
                                currenty, (float)contentwidth,
                                (float)25), titleFormat);
                    currenty += 25;

                    DrawString(gfx, ditem.barcode,
                        titlefont,
                        Brushes.Black, new RectangleF(currentx,
                        currenty, (float)contentwidth,
                        (float)25), titleFormat);

                    DrawString(gfx, ditem.rate.ToString("N2"),
    titlefont,
    Brushes.Black, new RectangleF(currentx + 120,
    currenty, (float)contentwidth,
    (float)25), titleFormat);

                    currenty += 25;

                    DrawString(gfx, string.Format("Issued {0},Billed {1},Returned {2}", ditem.saleqty, ditem.billedqty, ditem.journalqty),
    titlefont,
    Brushes.Black, new RectangleF(currentx,
    currenty, (float)contentwidth,
    (float)25), titleFormat);
                    currenty += 25;


                    currenty = 25 + (i * imageheight);
                    currentx += contentwidth;
                    if (!string.IsNullOrEmpty(ditem.employeecode))
                    {
                        var file = imageData.GetData(ditem.employeecode.LongFromBase62());
                        if (file != null)
                        {
                            var filestream = Resize(file.data,
                                (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                                (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, 70);
                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                            {
                                var image = System.Drawing.Image.FromStream(ms);
                                gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                                    new PdfSharp.Drawing.XPoint(currentx, currenty));
                            }
                            filestream = null;
                            file.data = null;
                            file = null;
                        }
                    }
                    currentx += imagewidth;
                }

                gfx.Save();
            }

            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                pdfDocument.Save(output, false);
                pdfDocument.Close();
                return output.ToArray();
            }
        }

        internal byte[] ImagePDFGeneration(MfgGalleryCreteria.ImagePDFRequest item)
        {
            int imagequality = item.ImageQuality > 0 ? item.ImageQuality : 70;
            var pdfDocument = new PdfSharp.Pdf.PdfDocument();
            //pdfDocument.Info.Title = Title;
            //pdfDocument.Info.Author = Author;
            //pdfDocument.Info.Subject = Subject;
            //pdfDocument.Info.Keywords = Keywords;

            ImageData imageData = new ImageData();
            imageData.Context = this.Context;
            StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            titleFormat.Alignment = StringAlignment.Center;
            StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Alignment = StringAlignment.Far;
            var titlefont = new Font("verdana", 10);
            var titlefontbold = new Font("verdana", 10, FontStyle.Bold);
            var textfont = new Font("verdana", 9);
            var textfontbold = new Font("verdana", 9, FontStyle.Bold);

            var bigfontbold = new Font("verdana", 12, FontStyle.Bold);

            StringBuilder titlesb = new StringBuilder();


            for (var i = 0; i < item.datasource.items.Count; i = i + (item.RowCount))
            {
                int rindex = 0;
                PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                var availablewidth = page.Width.Point - 50;
                var availableheight = page.Height.Point - 50;
                var descriptionheight = 20.0;


                var imageheight = (availableheight - 50) / item.RowCount;
                var imagewidth = item.ImageWidth > 0 ? item.ImageWidth : 150;// imageheight/item.ColumnCount;
                var designcodeitemwidth = item.DesignWidth;
                var sizewidth = (availablewidth - (imagewidth + designcodeitemwidth)) / item.datasource.sizelist.Count;
                //availablewidth- imagewidth

                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
                var currenty = 40.0;
                if (titlesb.Length > 0)
                {
                    DrawString(gfx, titlesb.ToString(),
                                titlefont,
                                Brushes.Black, new RectangleF(10,
                                10, (float)availablewidth,
                                (float)25), titleFormat);
                    currenty += 10;
                }

                var currentx = 25.0;
                gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, imagewidth, descriptionheight));
                DrawString(gfx, "Image",
                                       textfont,
                                       Brushes.Black, new RectangleF((float)currentx,
                                       (float)currenty, (float)imagewidth,
                                       (float)descriptionheight), titleFormat);
                currentx += imagewidth;
                foreach (var sitem in item.datasource.sizelist)
                {
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, sizewidth, descriptionheight));

                    DrawString(gfx, sitem.name,
                                        textfont,
                                        Brushes.Black, new RectangleF((float)currentx,
                                        (float)currenty, (float)sizewidth,
                                        (float)descriptionheight), titleFormat);
                    currentx += sizewidth;
                }
                if (designcodeitemwidth > 0)
                {
                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, designcodeitemwidth, descriptionheight));
                    DrawString(gfx, "Design",
                                           textfont,
                                           Brushes.Black, new RectangleF((float)currentx,
                                           (float)currenty, (float)designcodeitemwidth,
                                           (float)descriptionheight), titleFormat);
                }
                currentx += designcodeitemwidth;
                currenty += descriptionheight;
                rindex = 0;
                for (var r = i; r < item.datasource.items.Count && rindex < item.RowCount; r = r + 1)
                {
                    rindex++;
                    currentx = 25.0;
                    var value = item.datasource.items[r];
                    if (!string.IsNullOrEmpty(value.imgid))
                    {
                        var file = imageData.GetData(value.imgid.LongFromBase62());
                        if (file != null)
                        {
                            //if (imagequality == 100)
                            //{
                            //    var size = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                            //        (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation);

                            //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(file.data))
                            //    {
                            //        var image = System.Drawing.Image.FromStream(ms);
                            //        gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                            //            currentx, currenty, size.Item1, size.Item2);
                            //    }
                            //}
                            //else
                            if (file.data != null && file.data.Length > 0)
                            {
                                var filestream = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                                    (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, imagequality);
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                                {
                                    var image = System.Drawing.Image.FromStream(ms);
                                    gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                                        new PdfSharp.Drawing.XPoint(currentx, currenty));
                                }
                                filestream = null;
                            }
                            file.data = null;
                            file = null;
                        }
                    }

                    DrawString(gfx, value.designitemcode,
                                           bigfontbold,
                                           Brushes.Black, new RectangleF((float)currentx,
                                           (float)(currenty + imageheight - descriptionheight), (float)imagewidth,
                                           (float)descriptionheight), titleFormat);

                    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, imagewidth, imageheight));
                    DrawString(gfx, "",
                                           textfont,
                                           Brushes.Black, new RectangleF((float)currentx,
                                           (float)currenty, (float)imagewidth,
                                           (float)descriptionheight), titleFormat);

                    currentx += imagewidth;
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    foreach (var sitem in item.datasource.sizelist)
                    {
                        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, sizewidth, imageheight));
                        var stock = value.sizes.Find(x => x.id == sitem.id);
                        DrawString(gfx, (stock != null && stock.stock != 0) ? stock.stock.ToString("F0") : "",
                                            textfontbold,
                                            Brushes.Black, new RectangleF((float)currentx,
                                            (float)currenty, (float)sizewidth,
                                            (float)imageheight), stringFormat);
                        currentx += sizewidth;
                    }
                    stringFormat.Alignment = StringAlignment.Far;
                    if (designcodeitemwidth > 0)
                    {
                        titleFormat.LineAlignment = StringAlignment.Center;
                        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, designcodeitemwidth, imageheight));
                        var colour = item.datasource.colourlist.Find(x => x.id == value.colourid);

                        DrawString(gfx, value.designitemcode,
                                               textfontbold,
                                               Brushes.Black, new RectangleF((float)currentx,
                                               (float)currenty + (float)(imageheight / 2) - (float)descriptionheight,
                                               (float)designcodeitemwidth,
                                               (float)descriptionheight), titleFormat);

                        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, designcodeitemwidth, imageheight));
                        if (colour != null)
                        {
                            DrawString(gfx, colour.name,
                                                   textfontbold,
                                                   Brushes.Black, new RectangleF((float)currentx,
                                                   (float)(currenty + (float)(imageheight / 2)), (float)designcodeitemwidth,
                                                   (float)descriptionheight), titleFormat);
                        }
                    }
                    currentx += designcodeitemwidth;
                    currenty += imageheight;
                }
                gfx.Save();
            }

            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                pdfDocument.Save(output, false);
                pdfDocument.Close();
                return output.ToArray();
            }
        }


        internal byte[] ImagePDFGeneration(System.IO.MemoryStream stream,long organizationid, SalesorderPrint item)
        {

            Context = new UserClaim() { OrganizationID = organizationid };
            int imagequality = 70;
            var pdfDocument = new PdfSharp.Pdf.PdfDocument();

            stream.Seek(0, System.IO.SeekOrigin.Begin);
            PdfDocument fromDoc = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            for (int i = 0; i < fromDoc.PageCount; i++)
            {
                pdfDocument.AddPage(fromDoc.Pages[i]);
            }

            //pdfDocument.Info.Title = Title;
            //pdfDocument.Info.Author = Author;
            //pdfDocument.Info.Subject = Subject;
            //pdfDocument.Info.Keywords = Keywords;

            ImageData imageData = new ImageData();
            imageData.Context = this.Context;
            StringFormat titleFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            titleFormat.Alignment = StringAlignment.Center;
            StringFormat stringFormat = new StringFormat(StringFormatFlags.FitBlackBox);
            titleFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Alignment = StringAlignment.Far;
            var titlefont = new Font("verdana", 10);
            var titlefontbold = new Font("verdana", 10, FontStyle.Bold);
            var textfont = new Font("verdana", 9);
            var textfontbold = new Font("verdana", 9, FontStyle.Bold);

            var bigfontbold = new Font("verdana", 12, FontStyle.Bold);

            StringBuilder titlesb = new StringBuilder();

            int RowCount = 4;
            int ColCount = 3;
            int rindex = -1;
            for (var i = 0; i < item.Salesgroupitems.Count; i = i + (ColCount))
            {
                PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                var availablewidth = page.Width.Point - 50;
                var availableheight = page.Height.Point - 50;
                var descriptionheight = 20.0;


                var imageheight = (availableheight - 50) / RowCount;
                var imagewidth = (availablewidth - 50) / ColCount;// imageheight/item.ColumnCount;

                var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, PdfSharp.Drawing.XGraphicsUnit.Point);
                var currenty = 40.0;
                var currentx = 25.0;
                //for (var j = 0; i < ColCount; j++)
                //{
                //    rindex++;
                //    currentx = 25.0;
                //    var value = item.Salesgroupitems[rindex];
                //    if (!string.IsNullOrEmpty(value.oqty))
                //    {
                //        var file = imageData.GetData(value.imgid.LongFromBase62());
                //        if (file != null)
                //        {
                //            //if (imagequality == 100)
                //            //{
                //            //    var size = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                //            //        (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation);

                //            //    using (System.IO.MemoryStream ms = new System.IO.MemoryStream(file.data))
                //            //    {
                //            //        var image = System.Drawing.Image.FromStream(ms);
                //            //        gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                //            //            currentx, currenty, size.Item1, size.Item2);
                //            //    }
                //            //}
                //            //else
                //            if (file.data != null && file.data.Length > 0)
                //            {
                //                var filestream = Resize(file.data, (int)PdfSharp.Drawing.XUnit.FromPoint(imagewidth).Presentation,
                //                    (int)PdfSharp.Drawing.XUnit.FromPoint(imageheight).Presentation, imagequality);
                //                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(filestream))
                //                {
                //                    var image = System.Drawing.Image.FromStream(ms);
                //                    gfx.DrawImage(PdfSharp.Drawing.XImage.FromGdiPlusImage(image),
                //                        new PdfSharp.Drawing.XPoint(currentx, currenty));
                //                }
                //                filestream = null;
                //            }
                //            file.data = null;
                //            file = null;
                //        }
                //    }

                //    DrawString(gfx, value.designitemcode,
                //                           bigfontbold,
                //                           Brushes.Black, new RectangleF((float)currentx,
                //                           (float)(currenty + imageheight - descriptionheight), (float)imagewidth,
                //                           (float)descriptionheight), titleFormat);

                //    gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, imagewidth, imageheight));
                //    DrawString(gfx, "",
                //                           textfont,
                //                           Brushes.Black, new RectangleF((float)currentx,
                //                           (float)currenty, (float)imagewidth,
                //                           (float)descriptionheight), titleFormat);

                //    currentx += imagewidth;
                //    stringFormat.Alignment = StringAlignment.Center;
                //    stringFormat.LineAlignment = StringAlignment.Center;
                //    foreach (var sitem in item.datasource.sizelist)
                //    {
                //        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, sizewidth, imageheight));
                //        var stock = value.sizes.Find(x => x.id == sitem.id);
                //        DrawString(gfx, (stock != null && stock.stock != 0) ? stock.stock.ToString("F0") : "",
                //                            textfontbold,
                //                            Brushes.Black, new RectangleF((float)currentx,
                //                            (float)currenty, (float)sizewidth,
                //                            (float)imageheight), stringFormat);
                //        currentx += sizewidth;
                //    }
                //    stringFormat.Alignment = StringAlignment.Far;
                //    if (designcodeitemwidth > 0)
                //    {
                //        titleFormat.LineAlignment = StringAlignment.Center;
                //        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, designcodeitemwidth, imageheight));
                //        var colour = item.datasource.colourlist.Find(x => x.id == value.colourid);

                //        DrawString(gfx, value.designitemcode,
                //                               textfontbold,
                //                               Brushes.Black, new RectangleF((float)currentx,
                //                               (float)currenty + (float)(imageheight / 2) - (float)descriptionheight,
                //                               (float)designcodeitemwidth,
                //                               (float)descriptionheight), titleFormat);

                //        gfx.DrawRectangle(PdfSharp.Drawing.XPens.Black, new PdfSharp.Drawing.XRect(currentx, currenty, designcodeitemwidth, imageheight));
                //        if (colour != null)
                //        {
                //            DrawString(gfx, colour.name,
                //                                   textfontbold,
                //                                   Brushes.Black, new RectangleF((float)currentx,
                //                                   (float)(currenty + (float)(imageheight / 2)), (float)designcodeitemwidth,
                //                                   (float)descriptionheight), titleFormat);
                //        }

                //        currentx += designcodeitemwidth;
                //    }
                    
                //}
                gfx.Save();
                currenty += imageheight;
                rindex += ColCount;
            }

            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                pdfDocument.Save(output, false);
                pdfDocument.Close();
                return output.ToArray();
            }
        }
    }
}
