using System.Drawing.Printing;
using PdfSharp.Drawing;

namespace ReportLibrary
{
    public interface IPDFReport
    {
        void OnBeginPrint(PrintEventArgs e);
        void OnPrintPage(PrintPageEventArgs e);
        PDFGraphics.IGraphic PDFGraphic { get; set; }
        XGraphicsUnit Unit { get; set; }
        int PageHeightByContent { get; }
        object DataSource { get; set; }
        int PageNo { get; set; }
        int PageCount { get; set; }
        void Reset();
    }
}