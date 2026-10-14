using PDFGraphics;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using PdfSharp.Drawing;

namespace ReportLibrary
{

    public class JSONPrinter
    {
        public bool IsThermalPrint { get; set; }
        public static int Cm2Pixel(double WidthInCm)
        {
            double HeightInCm = WidthInCm;
            return Cm2Pixel(WidthInCm, HeightInCm).Width;
        } // End Function Cm2Pixel
        public bool EnablePageNumber { get; set; }
        public string[] TitleList { get; set; }
        public static int Mm2Pixel(double WidthInCm)
        {
            double HeightInCm = WidthInCm;
            return Cm2Pixel(WidthInCm / 10, HeightInCm / 10).Width * 10;
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

        public JSONDocument Print(ReportLibrary.DefaultPageSettings setting)
        {
            if (IsThermalPrint)
            {

                setting.PaperSize.Height = Cm2Pixel(120);
                setting.PageBounds = new Rectangle(setting.PageBounds.Left, setting.PageBounds.Top,
                    setting.PageBounds.Width, Cm2Pixel(120));
                setting.MarginBound = new Rectangle(setting.MarginBound.Left, setting.MarginBound.Top,
    setting.MarginBound.Width, Cm2Pixel(120));
            }

            PrintPageEventArgs peargs = new PrintPageEventArgs(null, setting.MarginBound, setting.PageBounds, null);
            PrintEventArgs args = new PrintEventArgs();
            Report.OnBeginPrint(args);
            Report.Unit = setting.Unit;
            if (args.Cancel) return null;
            pdfDocument = new JSONDocument();
            pdfDocument.MarginBound = setting.MarginBound;
            pdfDocument.PageBounds = setting.PageBounds;


            if (TitleList == null) TitleList = new string[] { "" };
            foreach (var title in TitleList)
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
                bool printcontinue = true;
                while (printcontinue)
                {
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(setting.PageBounds.Width, setting.PageBounds.Height))
                    {
                        using (System.Drawing.Graphics graphic = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            var page = pdfDocument.AddPage();
                            page.pagesize = IsThermalPrint ? JSONPage.PageSize.Statement : JSONPage.PageSize.A4;
                            if (setting.Landscape)
                                page.Orientation = JSONPage.PageOrientation.Landscape;
                            //page.Size = PdfSharp.PageSize.Undefined;
                            page.Width = setting.PageBounds.Width;
                            page.Height = setting.PageBounds.Height;
                            Report.PDFGraphic = new JSONGraphic(page, graphic, setting.Unit);
                            peargs = new PrintPageEventArgs(graphic, setting.MarginBound, setting.PageBounds, null);
                            Report.OnPrintPage(peargs);
                            if (IsThermalPrint)
                            {
                                page.Height = Report.PageHeightByContent;
                            }
                            printcontinue = peargs.HasMorePages || peargs.Cancel;
                        }
                    }
                }
            }
            return pdfDocument;
        }


        public delegate void OnNewPageHandler(long pageindex, bool isFirstPage, bool isLastPage);
        public event OnNewPageHandler OnNewPage;


        public string FileName;
        public string Title = "";
        public string Author = "Quanto";
        public string Subject = "";
        public string Keywords = "";

        JSONDocument pdfDocument = null;


    }
}