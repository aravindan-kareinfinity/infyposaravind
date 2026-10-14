using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Drawing.Printing;
using PDFGraphics;
using QRCoder;
using PdfSharp.Drawing;

namespace ReportLibrary
{
    public class PDFLabel : IPDFReport
    {
        public int PageNo { get; set; }
        public int PageCount { get; set; }
        public int PageHeightByContent
        {
            get
            {
                return labelheight;
            }
        }
        private int GetPosition(double value)
        {
            if(!convertopixel)
            return (int)XUnit.FromPresentation(value).Point;
            return (int)value;
        }
        public object DataSource { get; set; }

        PDFGraphics.IGraphic eGraphics;
        public PDFGraphics.IGraphic PDFGraphic
        {

            get
            {
                return eGraphics;
            }
            set
            {
                eGraphics = value;
            }
        }
        public PdfSharp.Drawing.XGraphicsUnit Unit { get; set; }
        public int columns { get; set; }
        public int labelwidth { get; set; }
        public int labelheight { get; set; }
        public int pageleftmargin { get; set; }
        public int pagerightmargin { get; set; }
        public int labelgap { get; set; }
        public bool convertopixel { get; set; }
        public List<LabelElement> Template
        {
            get
            {
                return template;
            }
            set
            {
                //if (convertopixel)
                //{
                //    foreach (var item in value)
                //    {
                //        item.height = CentimeterToPixel(item.height);
                //        item.height = CentimeterToPixel(item.width);
                //        item.height = CentimeterToPixel(item.top);
                //        item.height = CentimeterToPixel(item.left);
                //    }
                //}
                template = value;
            }
        }

        List<LabelElement> template { get; set; }
        public List<List<LabelElement>> Labels { get; set; }
        public List<List<List<LabelElement>>> Pages { get; set; }
        public int currentpage = 1;

        public void AddLabel(List<InfyPOS.Report.IBarcodeFormat> entity)
        {
            foreach (var item in entity)
                AddLabel(item);
        }

        public void AddLabel(InfyPOS.Report.IBarcodeFormat entity)
        {
            var newelements = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelElement>>(Newtonsoft.Json.JsonConvert.SerializeObject(Template));
            if (Labels == null)
                Labels = new List<List<LabelElement>>();
            Labels.Add(newelements);
            SortedDictionary<string, Object> data = entity.KeyValue;
            SortedDictionary<string, string> format = entity.KeyFormat;
            foreach (var line in newelements)
            {
                if(string.IsNullOrEmpty(line.text))
                {
                    line.text = "";
                    continue;
                }
                var responseline = line.text;
                
                foreach (var kv in data)
                {
                    if (responseline.Contains(kv.Key))
                    {
                        if (kv.Value == null)
                        {
                            responseline = responseline.Replace("[" + kv.Key + "]", "");
                        }
                        else if (format.ContainsKey(kv.Key))
                        {
                            if (kv.Key == line.text)
                            {
                                responseline = string.Format("{0:" + format[kv.Key] + "}", kv.Value);
                            }
                            else
                            {
                                responseline = responseline.Replace("[" + kv.Key + "]", string.Format("{0:" + format[kv.Key] + "}", kv.Value));
                            }
                        }
                        else if(kv.Key == line.text)
                        {
                            responseline = kv.Value.ToString();
                        }
                        else
                        {
                            responseline = responseline.Replace("[" + kv.Key + "]", kv.Value.ToString());
                        }
                    }
                }
                line.text = responseline;
            }
        }

        void IPDFReport.OnBeginPrint(PrintEventArgs e)
        {
            currentpage = 1;
            Pages = new List<List<List<LabelElement>>>();
            List<List<LabelElement>> currentpageelement = null;
            for (int i = 0; i < Labels.Count; i++)
            {
                if (currentpageelement == null || currentpageelement.Count == columns)
                {
                    currentpageelement = new List<List<LabelElement>>();
                    Pages.Add(currentpageelement);
                }
                currentpageelement.Add(Labels[i]);
            }
        }


        void IPDFReport.OnPrintPage(PrintPageEventArgs e)
        {
            int leftmargin = this.pageleftmargin;
            int labelindex = 0;
            foreach (var label in Pages[currentpage - 1])
            {
                int left = labelindex == 0 ? leftmargin : (leftmargin + (labelindex * labelwidth) + (labelindex*labelgap));
                labelindex++;
                foreach (var element in label)
                {
                    if (element.height == 0)
                    {
                       var size = e.Graphics.MeasureString(element.text, element.font);
                       element.height = (int) size.Height;
                    }

                    if (element.angle != 0)
                    {
                        PDFGraphic.RotateTransform(element.angle,new Point(GetPosition(element.left+left),
                            GetPosition(element.top)));
                    }

                    switch (element.Type)
                    {
                        case LabelElement.type.Barcode_128:
                            Draw1D(element, BarcodeLib.TYPE.CODE128, element.left+ left);
                            break;
                        case LabelElement.type.Barcode_39:
                            Draw1D(element, BarcodeLib.TYPE.CODE39, element.left + left);
                            break;
                        case LabelElement.type.Barcode_QR:
                            Draw2D(element, element.left + left);
                            break;
                        case LabelElement.type.Field:
                            DrawText(element, element.left + left);
                            break;
                        case LabelElement.type.Text:
                            DrawText(element, element.left + left);
                            break;
                        case LabelElement.type.Image:
                            DrawImage(element, element.left + left);
                            break;
                        case LabelElement.type.Line:
                            DrawLine(element, element.left + left);
                            break;
                        case LabelElement.type.Rectangle:
                            DrawRectangle(element, element.left + left);
                            break;
                        case LabelElement.type.Circle:
                            DrawRectangle(element, element.left + left);
                            break;
                    }
                    if (element.angle != 0)
                    {
                        PDFGraphic.ResetTransform();
                    }
                }
            }
            currentpage++;
            e.HasMorePages = currentpage <= Pages.Count;
        }

        public void DrawText(LabelElement element, int left)
        {
            var stringformat = new StringFormat();
            var rect = new RectangleF(GetPosition(left), GetPosition(element.top),
                GetPosition(element.width), GetPosition(element.height));
            switch(element.halign)
            {
                case LabelElement.Align.Center:
                    stringformat.Alignment = StringAlignment.Center;
                    break;
                case LabelElement.Align.Right:
                    stringformat.Alignment = StringAlignment.Far;
                    break;
                case LabelElement.Align.Left:
                    stringformat.Alignment = StringAlignment.Near;
                    break;
            }
            switch (element.valign)
            {
                case LabelElement.Align.Center:
                    stringformat.LineAlignment = StringAlignment.Center;
                    break;
                case LabelElement.Align.Right:
                    stringformat.LineAlignment = StringAlignment.Far;
                    break;
                case LabelElement.Align.Left:
                    stringformat.LineAlignment = StringAlignment.Near;
                    break;
            }

            var font = element.font;

            if(Unit == PdfSharp.Drawing.XGraphicsUnit.Millimeter)
                font = new Font(element.font.FontFamily, (float)PdfSharp.Drawing.XUnit.FromPoint(element.font.Size).Millimeter,
                    element.font.Style);
            else if (Unit == PdfSharp.Drawing.XGraphicsUnit.Inch)
                font = new Font(element.font.FontFamily, (float)PdfSharp.Drawing.XUnit.FromPoint(element.font.Size).Inch,
                    element.font.Style);

                PDFGraphic.DrawString(element.text, font, Brushes.Black, rect, stringformat);
            
        }

        public void Draw2D(LabelElement element, int left)
        {
            if (string.IsNullOrEmpty(element.text)) return;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(element.text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(4, Color.Black, Color.Transparent,false);
            PDFGraphic.DrawImageUnscaled(qrCodeImage, new Rectangle(GetPosition(left), GetPosition(element.top),
                GetPosition(element.width), GetPosition(element.height)));
        }

        //int CentimeterToPixel(double Centimeter)
        //{
        //    double pixel = Centimeter * 2.54d;
        //    return (int)pixel;
        //}

        public void Draw1D(LabelElement element, BarcodeLib.TYPE type, int left)
        {
            if (!string.IsNullOrEmpty(element.text))
            {
                BarcodeLib.Barcode b = new BarcodeLib.Barcode();
                Image img = b.Encode(type, element.text, Color.Black, Color.Transparent, element.width*2, (int)(element.height*1.5));
                PDFGraphic.DrawImageUnscaled(img, new Rectangle(GetPosition(left),
                    GetPosition(element.top), GetPosition(element.width), GetPosition(element.height)));
            }
        }

        public void DrawLine(LabelElement element, int left)
        {
            PDFGraphic.DrawLine(Pens.Black, GetPosition(left), GetPosition(element.top),
                GetPosition(element.width + left), GetPosition(element.top + element.height));
        }

        public void DrawRectangle(LabelElement element, int left)
        {
            PDFGraphic.DrawRectangle(Pens.Black, GetPosition(left), GetPosition(element.top),
                GetPosition(element.width ), GetPosition( element.height));
        }

        public void DrawCircle(LabelElement element,int left, System.Drawing.Pen pen, System.Drawing.Rectangle rect)
        {
            PDFGraphic.DrawCircle(Pens.Black, new System.Drawing.Rectangle(GetPosition(left), GetPosition(element.top),
                GetPosition(element.width), GetPosition(element.height)));
        }

        public void DrawImage(LabelElement element, int left)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(Convert.FromBase64String(element.text)))
            {
                PDFGraphic.DrawImage(Image.FromStream(ms), new Point(GetPosition(left), GetPosition(element.top)));
            }
        }

        public void Reset()
        {
            
        }
    }



    public class LabelElement
    {
        public type Type { get; set; }
        public string text { get; set; }
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
        public string fontname { get; set; }
        public float fontsize { get; set; }
        public System.Drawing.FontStyle fontstyle { get; set; }
        public Align valign { get; set; }
        public Align halign { get; set; }
        public Font font
        {
            get
            {
                if (string.IsNullOrEmpty(fontname))
                    fontname = "Verdana";
                if (fontsize == 0)
                    fontsize = 10;

                return new Font(fontname, fontsize, fontstyle);
            }
        }

        public double scalex { get;  set; }
        public double scaley { get;  set; }
        public float angle { get; set; }
        public enum Align
        {
            Left,
            Right,
            Center
        }
        public enum type
        {
            Barcode_39,
            Barcode_128,
            Barcode_QR,
            Text,
            Field,
            Image,
            Line,
            Rectangle,
            Circle
        }
        public enum Fontstyle
        {
            Normal,
            Bold,
            Italic,
            BoldItalic
        }
    }


    public class PDFLabelGenerator
    {

        public void TestCheque()
        {
            List<LabelElement> labelElements = new List<LabelElement>();
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "name",
                left = 15,
                top = 20,
                width = 140
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-day1",
                left = 143,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-day2",
                left = 150,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-month1",
                left = 157,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-month2",
                left = 164,
                top = 10,
                width = 5
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year1",
                left = 171,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year2",
                left = 178,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year3",
                left = 185,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year4",
                left = 192,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "words-1",
                left = 30,
                top = 28,
                width = 150
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "words-2",
                left = 30,
                top = 35,
                width = 110
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "amount",
                left = 160,
                top = 35,
                width = 30,
                valign = LabelElement.Align.Right
            });
            ReportLibrary.DefaultPageSettings pagesetting = new ReportLibrary.DefaultPageSettings()
            {
                Landscape = false,
                PaperSize = new PaperSize("Cheque", (int)(PdfSharp.Drawing.XUnit.FromMillimeter(210).Point), (int)(PdfSharp.Drawing.XUnit.FromMillimeter(90).Point)),
                Margins = new Margins(0, 0, 0, 0),
                Unit = PdfSharp.Drawing.XGraphicsUnit.Millimeter
            };
            ChequePrint chequePrint = new ChequePrint();
            List<InfyPOS.Report.IBarcodeFormat> barcodeFormats = new List<InfyPOS.Report.IBarcodeFormat>();
            barcodeFormats.Add(chequePrint);
            var bytes = Generate(Newtonsoft.Json.JsonConvert.SerializeObject(labelElements), pagesetting, barcodeFormats);
            System.IO.File.WriteAllBytes(@"e:\test.pdf", bytes);
        }


        public void TestBarcode()
        {
            List<LabelElement> labelElements = new List<LabelElement>();
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Text,
                text = "QUANTO SILKS",
                left = 0,
                top = 0,
                width = 35,
                halign = LabelElement.Align.Center,
                fontname = "tohama",
                fontsize = 8,
                fontstyle = FontStyle.Bold
            });


            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Barcode_QR,
                text = "BARCODE",
                left = 4,
                top = 8,
                height = 9,
                width = 9,
                halign = LabelElement.Align.Center,
                fontname = "tohama",
                fontsize = 8
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "NAME",
                left = 14,
                top = 8,
                width = 20,
                fontname = "tohama",
                fontsize = 8
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "STYLE",
                left = 14,
                top = 11,
                width = 20,
                fontname = "tohama",
                fontsize = 8
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "APRICE",
                left = 4,
                top = 17,
                width = 10,
                fontname = "tohama",
                fontsize = 8
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "QTY",
                left = 23,
                top = 17,
                width = 10,
                fontname = "tohama",
                fontsize = 8

            });

            var pdflength = PdfSharp.Drawing.XUnit.FromMillimeter(1);

            ReportLibrary.DefaultPageSettings pagesetting = new ReportLibrary.DefaultPageSettings()
            {
                Landscape = false,
                Unit = PdfSharp.Drawing.XGraphicsUnit.Millimeter,
                PaperSize = new PaperSize("Cheque", (int)PdfSharp.Drawing.XUnit.FromMillimeter(110).Point,
                (int)PdfSharp.Drawing.XUnit.FromMillimeter(25).Point),
                Margins = new Margins((int)PdfSharp.Drawing.XUnit.FromMillimeter(2).Point,
                                        (int)PdfSharp.Drawing.XUnit.FromMillimeter(2).Point,
                                        (int)PdfSharp.Drawing.XUnit.FromMillimeter(25).Point,
                                        (int)PdfSharp.Drawing.XUnit.FromMillimeter(25).Point)
            };
            List<InfyPOS.Report.IBarcodeFormat> barcodeFormats = new List<InfyPOS.Report.IBarcodeFormat>();
            for (var i = 0; i < 10; i++)
            {
                barcodeFormats.Add(
                    new InfyPOS.Models.Stock2Print()
                    {
                        printingname = "RUNNING MATERIALS",
                        stylename = "KOREAN SEQ EMP",
                        price = 700,
                        qty = 10,
                        barcode = "Y5S400"
                    });
            }
            var bytes = GenerateBarcode(Newtonsoft.Json.JsonConvert.SerializeObject(labelElements), pagesetting, barcodeFormats);
            System.IO.File.WriteAllBytes(@"e:\test.pdf", bytes);
        }


        public byte[] GenerateBarcode(string elementjson, ReportLibrary.DefaultPageSettings pagesetting, List<InfyPOS.Report.IBarcodeFormat> entityList)
        {
            var templates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelElement>>(elementjson);
            var label = new PDFLabel();
            label.convertopixel = true;
            label.Template = templates;
            label.pageleftmargin = 2;
            label.labelheight = 25;
            label.labelwidth = 35;
            label.labelgap = 1;
            label.columns = 3;
            label.AddLabel(entityList);
            ReportLibrary.PDFPrinter pDFPrinter = new ReportLibrary.PDFPrinter();
            pDFPrinter.Report = label;
            return pDFPrinter.Print(pagesetting).ToArray();
        }

        public byte[] Generate(string elementjson, ReportLibrary.DefaultPageSettings pagesetting, List<InfyPOS.Report.IBarcodeFormat> entityList)
        {
            var templates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelElement>>(elementjson);
            var label = new PDFLabel();
            label.convertopixel = true;
            label.Template = templates;
            label.AddLabel(entityList);
            ReportLibrary.PDFPrinter pDFPrinter = new ReportLibrary.PDFPrinter();
            pDFPrinter.Report = label;
            return pDFPrinter.Print(pagesetting).ToArray();
        }
    }

    public class ChequePrint : InfyPOS.Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return false;
        }
        public string name { get; set; }
        public decimal amount { get; set; }
        public DateTime date { get; set; }
        public string word1 { get; set; }
        public string word2 { get; set; }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("name", "Elayaraja & Co");
                ret.Add("amount", 15200);
                ret.Add("date-day1", date.ToString("dd")[0].ToString());
                ret.Add("date-day2", date.ToString("dd")[1].ToString());
                ret.Add("date-month1", date.ToString("MM")[0].ToString());
                ret.Add("date-month2", date.ToString("MM")[1].ToString());
                ret.Add("date-year1", date.ToString("yyyy")[0].ToString());
                ret.Add("date-year2", date.ToString("yyyy")[1].ToString());
                ret.Add("date-year3", date.ToString("yyyy")[2].ToString());
                ret.Add("date-year4", date.ToString("yyyy")[3].ToString());

                ret.Add("words1", word1);
                ret.Add("words2", word2);
                return ret;
            }
        }

        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("Date", "dd-MM-yyyy");
                ret.Add("Amount", "F0");
                return ret;
            }
        }
    }

    //public class LabelElement
    //{
    //    public type Type { get; set; }
    //    public string text { get; set; }
    //    public int left { get; set; }
    //    public int top { get; set; }
    //    public int width { get; set; }
    //    public int height { get; set; }
    //    public string format { get; set; }
    //    public string fontname { get; set; }
    //    public float fontsize { get; set; }
    //    public System.Drawing.FontStyle fontstyle { get; set; }
    //    public Align valign { get; set; }
    //    public Align halign { get; set; }
    //    public Font font
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(fontname))
    //                fontname = "Verdana";
    //            if (fontsize == 0)
    //                fontsize = 10;

    //            return new Font(fontname, fontsize, fontstyle);
    //        }
    //    }
    //    public enum Align
    //    {
    //        Left,
    //        Right,
    //        Center
    //    }
    //    public enum type
    //    {
    //        Barcode_39,
    //        Barcode_128,
    //        Barcode_QR,
    //        Text,
    //        Field,
    //        Image,
    //        Line,
    //        Rectangle,
    //        Circle
    //    }
    //    public enum Fontstyle
    //    {
    //        Normal,
    //        Bold,
    //        Italic,
    //        BoldItalic
    //    }
    //}



    //public class PDFLabelGenerator
    //{
    //    public byte[] Generate(string elementjson,List<InfyPOS.Report.IBarcodeFormat> entityList)
    //    {
    //        var templates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelElement>>(elementjson);
    //        var label = new PDFLabel();
    //        label.Template = templates;
    //        label.AddLabel(entityList);
    //        PDFPrinter pDFPrinter = new PDFPrinter();
    //        pDFPrinter.Report = label;
    //        return pDFPrinter.Print(new DefaultPageSettings()).ToArray();
    //    }
    //}
}
