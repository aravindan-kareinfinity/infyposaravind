using PDFGraphics;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using PdfSharp.Drawing;
using InfyPOS.Data;

namespace ReportLibrary
{
    public class DefaultPageSettings
    {
        public PaperSize PaperSize { get; set; }
        public System.Drawing.Printing.Margins Margins { get; set; }

        public System.Drawing.Rectangle MarginBound { get; set; }

        public bool Landscape { get; set; }
        public bool ThermalPrinter { get; set; }
        public XGraphicsUnit Unit { get; set; }
        public System.Drawing.Rectangle PageBounds { get; set; }

    }


    public class PageSetup
    {
        public Rectangle Bounds { get;  set; }
        public Margins Margins { get; set; }
    }
    public class PDFPrinter
    {
        public bool IsThermalPrint { get; set; }
        public static int Cm2Pixel(double WidthInCm)
        {
            double HeightInCm = WidthInCm;
            return Cm2Pixel(WidthInCm, HeightInCm).Width;
        } // End Function Cm2Pixel

        public static int Mm2Pixel(double WidthInCm)
        {
            double HeightInCm = WidthInCm;
            return Cm2Pixel(WidthInCm/10, HeightInCm/10).Width*10;
        } // End Function Cm2Pixel

        public static System.Drawing.Size Cm2Pixel(double WidthInCm, double HeightInCm)
        {
            float sngWidth = (float)WidthInCm; //cm
            float sngHeight = (float)HeightInCm; //cm
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(1, 1))
            {
                sngWidth *= 0.393700787f * bmp.HorizontalResolution; // x-Axis pixel
                sngHeight *= 0.393700787f * bmp.VerticalResolution; // y-Axis pixel
            }

            return new System.Drawing.Size((int)sngWidth, (int)sngHeight);
        } // End Function Cm2Pixel

        public ReportLibrary.IPDFReport Report { get; set; }
        public string[] TitleList { get;  set; }
        public bool EnablePageNumber { get; set; }
        public System.IO.MemoryStream Print(ReportLibrary.DefaultPageSettings setting)
        {
            if (IsThermalPrint)
            {

                setting.PaperSize.Height = Cm2Pixel(120);
                setting.PageBounds = new Rectangle(setting.PageBounds.Left, setting.PageBounds.Top,
                    setting.PageBounds.Width, Cm2Pixel(120));
                setting.MarginBound = new Rectangle(setting.MarginBound.Left, setting.MarginBound.Top,
    setting.MarginBound.Width, Cm2Pixel(120));
            }
            
            pdfDocument = new PdfSharp.Pdf.PdfDocument();
            pdfDocument.Info.Title = Title;
            pdfDocument.Info.Author = Author;
            pdfDocument.Info.Subject = Subject;
            pdfDocument.Info.Keywords = Keywords;

            if (Report is ReportLibrary.DotMatrixReporter)
            {
                var labreport = Report as ReportLibrary.DotMatrixReporter;
                if (labreport.format.PageHeader != null &&
        !string.IsNullOrEmpty(labreport.format.PageHeader.Style))
                {
                    labreport.BodyStyle.Font = labreport.format.GetStyle(labreport.format.PageHeader.Style);
                    labreport.HeaderStyle.Font = new System.Drawing.Font(labreport.BodyStyle.Font, System.Drawing.FontStyle.Bold);
                    labreport.TotalStyle.Font = new System.Drawing.Font(labreport.BodyStyle.Font, System.Drawing.FontStyle.Bold);
                }

            }

            //setting.PageBounds = setting.PageSettings.Bounds;
            //setting.MarginBound = new System.Drawing.Rectangle(setting.Margins.Left, setting.Margins.Top, setting.PageBounds.Width -
            //   (setting.PageBounds.Left + setting.PageBounds.Right), setting.PageBounds.Height -
            //   (setting.PageBounds.Top + setting.PageBounds.Bottom));

            PrintPageEventArgs peargs = new PrintPageEventArgs(null, setting.MarginBound, setting.PageBounds, null);
            PrintEventArgs args = new PrintEventArgs();
            Report.OnBeginPrint(args);
            Report.Unit = setting.Unit;
            if (args.Cancel) return null;
            var output = RenderPage(setting);
            if(IsThermalPrint && maxHeight>0)
            {
                pdfDocument = new PdfSharp.Pdf.PdfDocument();
                pdfDocument.Info.Title = Title;
                pdfDocument.Info.Author = Author;
                pdfDocument.Info.Subject = Subject;
                pdfDocument.Info.Keywords = Keywords;

                setting.PaperSize.Height = maxHeight;
                setting.PageBounds = new Rectangle(setting.PageBounds.Left, setting.PageBounds.Top,
                    setting.PageBounds.Width, maxHeight);
                setting.MarginBound = new Rectangle(setting.MarginBound.Left, setting.MarginBound.Top,
    setting.MarginBound.Width, maxHeight);
                peargs = new PrintPageEventArgs(null, setting.MarginBound, setting.PageBounds, null);
                args = new PrintEventArgs();
                Report.Reset();
                Report.OnBeginPrint(args);
                Report.Unit = setting.Unit;
                if (args.Cancel) return null;
                output = RenderPage(setting);
            }
            return output;
        }
        int maxHeight = 0;
        private System.IO.MemoryStream RenderPage(ReportLibrary.DefaultPageSettings setting)
        {
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            if (setting.PageBounds.Height == 0)
            {
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(setting.PageBounds.Width,
                    6000))
                {
                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                        page.Size = IsThermalPrint ? PdfSharp.PageSize.Statement : PdfSharp.PageSize.A4;
                        if (setting.Landscape)
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                        //page.Size = PdfSharp.PageSize.Undefined;
                        page.Width = setting.PageBounds.Width;
                        page.Height = 6000;
                        Report.PDFGraphic = new PDFGraphic(page, graphic, setting.Unit);
                        var peargs = new PrintPageEventArgs(graphic, setting.MarginBound, setting.PageBounds, new PageSettings()
                        {
                            PaperSize = new PaperSize("Custom", setting.PageBounds.Width, setting.PageBounds.Height),
                            Landscape = setting.Landscape,
                            Margins = setting.Margins
                        });
                        Report.OnPrintPage(peargs);
                        if (maxHeight < Report.PageHeightByContent)
                            maxHeight = Report.PageHeightByContent;
                        pdfDocument.Pages.RemoveAt(0);
                    }
                }

                Report.Reset();
                PrintEventArgs args = new PrintEventArgs();
                Report.OnBeginPrint(args);

                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(setting.PageBounds.Width,
                    maxHeight))
                {
                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                        page.Size = IsThermalPrint ? PdfSharp.PageSize.Statement : PdfSharp.PageSize.A4;
                        if (setting.Landscape)
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                        //page.Size = PdfSharp.PageSize.Undefined;
                        page.Width = setting.PageBounds.Width;
                        page.Height = maxHeight;
                        Report.PDFGraphic = new PDFGraphic(page, graphic, setting.Unit);
                        var peargs = new PrintPageEventArgs(graphic, setting.MarginBound, setting.PageBounds, new PageSettings()
                        {
                            PaperSize = new PaperSize("Custom", setting.PageBounds.Width, setting.PageBounds.Height),
                            Landscape = setting.Landscape,
                            Margins = setting.Margins
                        });
                        Report.OnPrintPage(peargs);
                        Report.PDFGraphic.Save();
                    }
                }
            }
            else
            {
                int noofpages = 0;
                if(EnablePageNumber)
                {
                    noofpages = PrintPage(setting,0,true);
                    Report.Reset();
                }
                if (TitleList == null) TitleList = new string[] { "" };
                foreach(var title in TitleList)
                {
                    if (!string.IsNullOrEmpty(title))
                    {
                        Report.Reset();
                        if (Report.DataSource != null && Report.DataSource is InfyPOS.Models.Bill)
                        {
                            (Report.DataSource as InfyPOS.Models.Bill).fixedbilldisplay = title;
                            (Report.DataSource as InfyPOS.Models.Bill).showfixeddisplay = true;
                        }
                    }
                    PrintPage(setting, noofpages,false);
                }
            }
            pdfDocument.Save(output, false);
            pdfDocument.Close();

            if(tpdfDocument != null)
            {
                tpdfDocument.Close();
                tpdfDocument = null;
            }
            return output;
        }

        private int PrintPage(ReportLibrary.DefaultPageSettings setting,
            int noofpages,
            bool isforpagecount)
        {
            int pagecount = 0;
            var printcontinue = true;
            if(noofpages>0)
            {
                Report.PageCount = noofpages;
            }
            Report.PageNo = 0;
            while (printcontinue)
            {
                //setting.PageBounds = new Rectangle(0, 0, setting.PaperSize.Width, setting.PaperSize.Height);
                //setting.MarginBound = new Rectangle(0, 0, setting.PaperSize.Width, setting.PaperSize.Height);
                pagecount++;
                Report.PageNo = pagecount;
                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(setting.PageBounds.Width, setting.PageBounds.Height))
                {
                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        PdfSharp.Pdf.PdfPage page = null;
                        if (!isforpagecount)
                        {
                            page = pdfDocument.AddPage();
                        }
                        else
                        {
                            tpdfDocument = new PdfSharp.Pdf.PdfDocument();
                            page = tpdfDocument.AddPage();
                        }
                        page.Size = IsThermalPrint ? PdfSharp.PageSize.Statement : PdfSharp.PageSize.A4;
                        if (setting.Landscape)
                            page.Orientation = PdfSharp.PageOrientation.Landscape;
                        //page.Size = PdfSharp.PageSize.Undefined;
                        page.Width = setting.PageBounds.Width;
                        page.Height = setting.PageBounds.Height;
                        if (setting.Unit == XGraphicsUnit.Presentation)
                        {
                            page.Width = XUnit.FromPresentation(setting.PageBounds.Width);
                            page.Height = XUnit.FromPresentation(setting.PageBounds.Height);
                            Report.PDFGraphic = new PDFGraphic(page, graphic, XGraphicsUnit.Point);
                        }
                        else
                        {
                            Report.PDFGraphic = new PDFGraphic(page, graphic, setting.Unit);
                        }
                        var peargs = new PrintPageEventArgs(graphic, setting.MarginBound, setting.PageBounds,
                            new PageSettings()
                            {
                                PaperSize = new PaperSize("Custom", 
                                (int)page.Width.Point, (int)page.Height.Point),
                                Landscape = setting.Landscape,
                                Margins = setting.Margins
                            });

                        Report.OnPrintPage(peargs);
                        if (maxHeight < Report.PageHeightByContent)
                            maxHeight = Report.PageHeightByContent;
                        if (!isforpagecount)
                        {
                            Report.PDFGraphic.Save();
                        }
                        printcontinue = peargs.HasMorePages || peargs.Cancel;
                    }
                }
            }
            return pagecount;
        }

        public System.IO.MemoryStream PrintOnMillimeter(ReportLibrary.DefaultPageSettings setting)
        {

            System.IO.MemoryStream output = new System.IO.MemoryStream();
            pdfDocument = new PdfSharp.Pdf.PdfDocument();
            pdfDocument.Info.Title = Title;
            pdfDocument.Info.Author = Author;
            pdfDocument.Info.Subject = Subject;
            pdfDocument.Info.Keywords = Keywords;

            //setting.PageBounds = setting.PageSettings.Bounds;
            //setting.MarginBound = new System.Drawing.Rectangle(setting.Margins.Left, setting.Margins.Top, setting.PageBounds.Width -
            //   (setting.PageBounds.Left + setting.PageBounds.Right), setting.PageBounds.Height -
            //   (setting.PageBounds.Top + setting.PageBounds.Bottom));

            var pointmargin = new Rectangle((int)XUnit.FromMillimeter(setting.Margins.Left).Point,
                (int)XUnit.FromMillimeter(setting.Margins.Top).Point,
                (int)XUnit.FromMillimeter(setting.PageBounds.Width-(setting.Margins.Left+ setting.Margins.Right)).Point,
                (int)XUnit.FromMillimeter(setting.PageBounds.Height-(setting.Margins.Top+ setting.Margins.Bottom)).Point);

            var pointpage = new Rectangle((int)XUnit.FromMillimeter(setting.PageBounds.Left).Point,
               (int)XUnit.FromMillimeter(setting.PageBounds.Top).Point,
               (int)XUnit.FromMillimeter(setting.PageBounds.Width).Point,
               (int)XUnit.FromMillimeter(setting.PageBounds.Height).Point);

            PrintPageEventArgs peargs = new PrintPageEventArgs(null, setting.MarginBound, setting.PageBounds, null);
            PrintEventArgs args = new PrintEventArgs();
            Report.OnBeginPrint(args);
            Report.Unit = setting.Unit;
            if (args.Cancel) return null;

            bool printcontinue = true;
            while (printcontinue)
            {

                //setting.PageBounds = new Rectangle(0, 0, setting.PaperSize.Width, setting.PaperSize.Height);
                //setting.MarginBound = new Rectangle(0, 0, setting.PaperSize.Width, setting.PaperSize.Height);
                PdfSharp.Pdf.PdfPage page = pdfDocument.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                if (setting.Landscape)
                    page.Orientation = PdfSharp.PageOrientation.Landscape;

                using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(Mm2Pixel(setting.PageBounds.Width),
                    Mm2Pixel(setting.PageBounds.Height)))
                {
                    using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        //page.Size = PdfSharp.PageSize.Undefined;
                        page.Width = XUnit.FromMillimeter(setting.PageBounds.Width).Point;
                        page.Height = XUnit.FromMillimeter(setting.PageBounds.Height).Point;
                        Report.PDFGraphic = new PDFGraphic(page, graphic, XGraphicsUnit.Point);
                        peargs = new PrintPageEventArgs(graphic, pointmargin, pointpage, null);
                        Report.OnPrintPage(peargs);
                        Report.PDFGraphic.Save();
                        printcontinue = peargs.HasMorePages || peargs.Cancel;
                    }
                }
            }

            pdfDocument.Save(output,false);
            pdfDocument.Close();
            return output;
        }

        
        public delegate void OnNewPageHandler(long pageindex, bool isFirstPage, bool isLastPage);
        public event OnNewPageHandler OnNewPage;

        
        public string FileName;
        public string Title = "";
        public string Author = "Quanto";
        public string Subject = "";
        public string Keywords = "";

        PdfSharp.Pdf.PdfDocument pdfDocument = null;
        PdfSharp.Pdf.PdfDocument tpdfDocument = null;

    }
}