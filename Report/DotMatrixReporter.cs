using InfyPOS.Common;
using QRCoder;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using WebAPI.Data;
using PdfSharp.Drawing;


namespace ReportLibrary
{
    public class DotMatrixReporter : DOSReporter
    {

        public class PrinterConfig
        {
            public string PrinterName;
            public int TopMargin = 50;
            public int LeftMargin = 50;
            public int BottomMargin = 50;
            public int RightMargin = 50;
            public int Width = 850;
            public int Height = 1100;
            public bool IsLandscape = false;
        }

        public enum DisplayTextAlign
        {
            Left,
            Rigtht,
            Center
        }

        public enum FontSize
        {
            Normal,
            Big,
            Small
        }

        public class DesignerElement
        {

            public DesignerElement(DosFormat.Content citem)
            {
                DisplayText = citem.Text;
            }

            public string DisplayText { get; set; }
            public DisplayTextAlign DisplayTextAlign { get; set; }
            public Color ForeColor { get; set; }
            public float Top { get; set; }
            public float Left { get; set; }
            public int Width { get; set; }
            public Font font { get; set; }
            internal Font GetFont()
            {
                if (font == null)
                    return new Font("verdana", 10f);
                return font;
            }
        }

        DefaultPageSettings defaultPageSettings = new DefaultPageSettings();
        Font webdings = new Font("Webdings", 12f);
        PrinterConfig lrd;
        DesignerElement currentElement;
        string configName = "CustomFinalPrint";
        public DotMatrixReporter(long organizatoinid, string configName, PrinterConfig lrdConfig)
            : this(organizatoinid, lrdConfig)
        {
            this.configName = configName;
        }

        public DefaultPageSettings DefaultPageSettings
        {
            get
            {
                return defaultPageSettings;
            }

            set
            {
                defaultPageSettings = value;
            }
        }

        public DotMatrixReporter(long organizationid) : base(organizationid)
        {

        }
        public DotMatrixReporter(long organizationid, PrinterConfig lrdConfig)
            : base(organizationid)
        {
            lrd = lrdConfig;
            DefaultPageSettings.PaperSize = new PaperSize(configName, lrd.Width, lrd.Height);
            DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(lrd.LeftMargin, lrd.RightMargin, lrd.TopMargin, lrd.BottomMargin);
            if (DefaultPageSettings.PaperSize.Width <= 550)
            {
                HeaderStyle.Font = new Font(HeaderStyle.Font.Name, 7.5f, HeaderStyle.Font.Style);
                BodyStyle.Font = new Font(BodyStyle.Font.Name, 7.5f, BodyStyle.Font.Style);
            }
            else if (DefaultPageSettings.PaperSize.Width <= 650)
            {
                HeaderStyle.Font = new Font(HeaderStyle.Font.Name, 8.5f, HeaderStyle.Font.Style);
                BodyStyle.Font = new Font(BodyStyle.Font.Name, 8.5f, BodyStyle.Font.Style);
            }
            else if (DefaultPageSettings.PaperSize.Width <= 750)
            {
                HeaderStyle.Font = new Font(HeaderStyle.Font.Name, 9f, HeaderStyle.Font.Style);
                BodyStyle.Font = new Font(BodyStyle.Font.Name, 9f, BodyStyle.Font.Style);
            }
        }

        public override void OnBeginPrint(PrintEventArgs e)
        {
            //DefaultPageSettings.PaperSize = new PaperSize(configName, lrd.Width, lrd.Height);
            //DefaultPageSettings.Margins = new System.Drawing.Printing.Margins(lrd.LeftMargin, lrd.RightMargin, lrd.TopMargin, lrd.BottomMargin);
            base.OnBeginPrint(e);
        }

        private int currentRow = 0;
        private int currentIndex = 0;

        float defaultHeight = 0;
        bool IsMultiLinePrinting = false;
        string StringToPrint = string.Empty;
        private bool DisplayMultiLine(PrintPageEventArgs e)
        {
            IsMultiLinePrinting = true;
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Near;
            int charactersOnPage = 0;
            int linesPerPage = 0;
            // Sets the value of charactersOnPage to the number of characters 0
            // of stringToPrint that will fit within the bounds of the page.
            SizeF sf = new SizeF(currentElement.Width, e.MarginBounds.Bottom - currentY);
            SizeF psf = PDFGraphic.MeasureString(StringToPrint, GetCurrentFont, sf, stringFormat,
                out charactersOnPage, out linesPerPage);

            RectangleF bodyrectangle = new RectangleF(currentX, currentY - 2, psf.Width, psf.Height + 15);

            // Draws the string within the bounds of the page
            PDFGraphic.DrawString(StringToPrint.Substring(0, charactersOnPage), GetCurrentFont, BodyStyle.Brush,
               bodyrectangle, stringFormat);

            // Remove the portion of the string that has been printed.
            StringToPrint = StringToPrint.Substring(charactersOnPage);

            currentY = currentY + bodyrectangle.Height;
            // Check to see if more pages are to be printed.
            IsMultiLinePrinting = (StringToPrint.Length > 0);
            return IsMultiLinePrinting;
        }

        private void Display(Graphics g, string text)
        {
            stringFormat.Alignment = CurrentAligntment;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            PDFGraphic.DrawString(text, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private StringAlignment CurrentAligntment
        {
            get
            {
                switch (currentElement.DisplayTextAlign)
                {
                    case DisplayTextAlign.Center:
                        return StringAlignment.Center;
                    case DisplayTextAlign.Left:
                        return StringAlignment.Near;
                }
                return StringAlignment.Far;
            }
        }

        public int GetPercentWidth(int percent)
        {
            return currentElement.Width * percent / 100;
        }


        private void Display(Graphics g, decimal value)
        {
            Display(g, value, null);
        }

        private void DisplayFullNumber(Graphics g, decimal value, String format)
        {
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            string val = string.IsNullOrEmpty(format) ? value.ToString() : string.Format(format, value);
            PDFGraphic.DrawString(val, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private void Display(Graphics g, decimal value, String format)
        {
            if (value == 0) return;
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            string val = string.IsNullOrEmpty(format) ? value.ToString() : string.Format(format, value);
            PDFGraphic.DrawString(val, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private void Display(Graphics g, bool value, string displayText)
        {
            string dispText = value ? "a" : "r";
            stringFormat.Alignment = StringAlignment.Near;
            stringFormat.LineAlignment = StringAlignment.Center;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, GetColumnWidth(webdings, g, dispText + dispText), defaultHeight);
            PDFGraphic.DrawString(dispText, webdings, BodyStyle.Brush, bodyrectangle, stringFormat);
            if (string.IsNullOrEmpty(displayText) == false)
            {
                bodyrectangle = new RectangleF(currentX + GetColumnWidth(GetCurrentFont, g, dispText + dispText), currentY, currentElement.Width, defaultHeight);
                PDFGraphic.DrawString(displayText, GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
            }
            stringFormat.LineAlignment = StringAlignment.Near;
        }

        private void Display(Graphics g, DateTime value, string format)
        {
            stringFormat.Alignment = StringAlignment.Far;
            stringFormat.Alignment = CurrentAligntment;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, currentElement.Width, defaultHeight);
            PDFGraphic.DrawString(value.ToString(format), GetCurrentFont, BodyStyle.Brush, bodyrectangle, stringFormat);
        }


        private void DrawCell(float x, float y, float width, float height)
        {
            PDFGraphic.DrawRectangle(new Pen(Color.Black), x, y, width, height);
        }

        private void DrawCell(int width, float height)
        {
            PDFGraphic.DrawRectangle(new Pen(Color.Black), currentX, currentY, width, height);
        }

        private void DrawCell(string title, int width, float height, StringAlignment sf, PrintPageEventArgs e)
        {
            PDFGraphic.FillRectangle(Brushes.LightGray, currentX, currentY, width, height);
            PDFGraphic.DrawRectangle(new Pen(Color.Black), currentX, currentY, width, height);
            stringFormat.Alignment = sf;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);
            if (!string.IsNullOrEmpty(title) && title.ToArray().Any(x => x > 200))
            {
                Style _style = (Style)TotalStyle.Clone();
                _style.Font = new Font("Times New Roman", _style.Font.Size);
                PDFGraphic.DrawString(title, _style.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
            }
            else
            {

                PDFGraphic.DrawString(title, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
            }
        }

        private void DisplayText(string title, int width, float height, StringAlignment sf, PrintPageEventArgs e)
        {
            PDFGraphic.FillRectangle(Brushes.LightGray, currentX, currentY, width, height);
            PDFGraphic.DrawRectangle(new Pen(Color.Black), currentX, currentY, width, height);
            stringFormat.Alignment = sf;
            RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);

            if (!string.IsNullOrEmpty(title) && title.ToArray().Any(x => x > 200))
            {
                Style _style = (Style)TotalStyle.Clone();
                _style.Font = new Font("Times New Roman", _style.Font.Size);
                PDFGraphic.DrawString(title, _style.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
            }
            else
            {

                PDFGraphic.DrawString(title, TotalStyle.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
            }
        }

        private void DisplayText(DosFormat.Content citem, string title, int width, float height, StringAlignment sf, PrintPageEventArgs e)
        {
            Style _style = (Style)TotalStyle.Clone();

            if (height < 30)
            {
                if (!string.IsNullOrEmpty(title) &&
                    title.ToArray().Any(x => x > 200))
                {
                    _style.Font = new Font("Times New Roman", _style.Font.Size);
                }
                if (!string.IsNullOrEmpty(citem.Style))
                {
                    _style.Font = citem.GetStyle(_style.Font);
                    float rowheight = GetRowHeight(_style.Font, e);
                    height = rowheight;
                }
                stringFormat.Alignment = sf;
                stringFormat.LineAlignment = StringAlignment.Near;
                RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);
                PDFGraphic.DrawString(title, _style.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
            }
            else
            {
                if (citem.GetStyle() != null)
                    _style.Font = citem.GetStyle();
                stringFormat.Alignment = sf;
                stringFormat.LineAlignment = StringAlignment.Near;
                RectangleF bodyrectangle = new RectangleF(currentX, currentY, width, height);
                PDFGraphic.DrawString(title, _style.Font, TotalStyle.Brush, bodyrectangle, stringFormat);
            }

        }

        private Font GetCurrentFont
        {
            get
            {
                return currentElement.GetFont();
            }
        }


        public float GetColumnWidth(Font fntReportBodyFont, Graphics g, int noofChar)
        {
            return g.MeasureString(new string('w', noofChar), fntReportBodyFont).Width + 2;
        }


        private bool isPrintRemainingDetails;

        private void PrintLine(Graphics g, PrintPageEventArgs e)
        {
            if (endX < currentElement.Width)
                currentElement.Width = (int)endX - (int)currentX;

            Pen pen = new Pen(currentElement.ForeColor.IsEmpty ? Color.Black : currentElement.ForeColor);
            pen.Width = 1;
            int v = 0;
            if (int.TryParse(currentElement.DisplayText, out v))
            {
                switch (v)
                {
                    case 0:
                        PDFGraphic.DrawLine(pen, currentX, currentY + currentElement.Top, currentX + currentElement.Width, currentY + currentElement.Top);
                        break;
                    case 1:
                        PDFGraphic.DrawLine(pen, currentX, currentY + currentElement.Top, currentX + currentElement.Width, currentY + currentElement.Top);
                        PDFGraphic.DrawLine(pen, currentX, currentY + currentElement.Top + 2, currentX + currentElement.Width, currentY + currentElement.Top + 2);
                        break;
                    case 2:
                        pen.Width = 2;
                        PDFGraphic.DrawLine(pen, currentX, currentY + currentElement.Top, currentX + currentElement.Width, currentY + currentElement.Top);
                        break;
                    case 3:
                        PDFGraphic.DrawLine(pen, 0, currentY + currentElement.Top, endX, currentY + currentElement.Top);
                        break;
                    case 4:
                        PDFGraphic.DrawLine(pen, 0, currentY + currentElement.Top, endX, currentY + currentElement.Top);
                        PDFGraphic.DrawLine(pen, 0, currentY + currentElement.Top + 2, endX, currentY + currentElement.Top + 2);
                        break;
                    case 5:
                        pen.Width = 2;
                        PDFGraphic.DrawLine(pen, 0, currentY + currentElement.Top, endX, currentY + currentElement.Top);
                        break;
                }
            }
            else
            {
                PDFGraphic.DrawLine(pen, currentX, currentY + currentElement.Top, currentX + currentElement.Width, currentY + currentElement.Top);
            }
        }

        private object dataSourceList = null;
        IEnumerator currentDataSourceList = null;
        public object DataSourceList
        {
            get
            {
                return dataSourceList;
            }
            set
            {
                dataSourceList = value;
                ie = null;
                ien = null;
            }
        }

        public string DataSourceItemMember { get; set; }

        float? defaultLeft = null;
        PDFGraphics.IGraphic eGraphics;
        IEnumerator currentBodyDataSource = null;
        public Object DataItems;
        public Object DataItem;
        public DosFormat Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
                var style = format.GetStyle(format.PageHeader.Style);
                if (style != null)
                {
                    HeaderStyle.Font = style;
                }
                style = format.GetStyle(format.PageBody.Style);
                if (style != null)
                {
                    BodyStyle.Font = style;
                }
                style = format.GetStyle(format.PageSetting.Style);
                if (style != null)
                {
                    TotalStyle.Font = style;
                }
            }
        }

        public SortedDictionary<string, object> Additional;
        int currentbodyIndex = 0;
        float endX = 0;
        float currentLeft = 0;
        public PrintingMode Mode { get; set; }
        public enum PrintingMode
        {
            Normal,
            Label,
            Single
        }

        public void RenderLabelTable(DosFormat.Line item, System.Drawing.Printing.PrintPageEventArgs e)
        {
            currentTable = new Table();
            currentTable.Thermal = item.Thermal;
            currentTable.ShowHeaderBottomLine = format.PageBody.DisableGrid && !format.PageBody.VerticleGrid;
            currentTable.AutoFitHeight = format.PageBody.WrapRow;
            currentTable.FooterRows = format.PageBody.FooterRows;

            currentTableRow = 0;
            List<DosFormat.Content> removable = new List<DosFormat.Content>();
            foreach (var citem in item.Items)
            {
                if (citem.Group)
                    currentTable.GroupColumn = currentTable.Columns.Count;

                if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                {
                    bool hide = false;
                    if (!string.IsNullOrEmpty(citem.Show))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                        if (show != null && !(bool)show)
                            hide = true;
                    }
                    if (!hide && !string.IsNullOrEmpty(citem.Hide))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                        if (show != null && (bool)show)
                            hide = true;
                    }
                    if (hide)
                    {
                        removable.Add(citem);
                        continue;
                    }
                    var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                    clmn.TitleCase = citem.TitleCase;
                    clmn.BodyFont = item.GetStyle();
                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                    {
                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                    }
                    if (clmn.BodyFont != null)
                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                    if (citem.BottomColumn)
                    {
                        currentTable.BottomColumn = currentTable.Columns.Count;
                    }
                    clmn.Multiline = citem.Multiline == "true";
                    clmn.MergeCell = citem.MergeCell;
                    clmn.MergeText = citem.MergeText;
                    clmn.Prefix = citem.Prefix;
                    clmn.Suffix = citem.Suffix;
                    currentTable.Columns.Add(clmn);
                    if (clmn.ShowTotal)
                        currentTable.Total.Add(currentTable.Columns.Count - 1, 0);
                }
                else if (citem.Type == DosFormat.CotnentType.Word)
                {
                    bool hide = false;
                    if (!string.IsNullOrEmpty(citem.Show))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                        if (show != null && !(bool)show)
                            hide = true;
                    }
                    if (!hide && !string.IsNullOrEmpty(citem.Hide))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                        if (show != null && (bool)show)
                            hide = true;
                    }
                    if (hide)
                    {
                        removable.Add(citem);
                        continue;
                    }
                    var clmn = new Column(citem.Name, citem.Name, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                    clmn.TitleCase = citem.TitleCase;
                    clmn.BodyFont = item.GetStyle();
                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                    {
                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                    }
                    if (clmn.BodyFont != null)
                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                    if (citem.BottomColumn)
                    {
                        currentTable.BottomColumn = currentTable.Columns.Count;
                    }
                    clmn.Multiline = citem.Multiline == "true";
                    clmn.MergeCell = citem.MergeCell;
                    clmn.MergeText = citem.MergeText;
                    clmn.Prefix = citem.Prefix;
                    clmn.Suffix = citem.Suffix;
                    currentTable.Columns.Add(clmn);
                }
            }
            removable.ForEach(ex => item.Items.Remove(ex));

            int sno = 0;

            var tablesource = ReportLibrary.DataSourceHelper.Eval(DataSource, item.field);
            if (tablesource != null)
            {
                ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(tablesource, null);
                if (ie != null)
                    currentBodyDataSource = ie.GetEnumerator();
            }
            while (currentBodyDataSource != null && currentBodyDataSource.MoveNext())
            {
                DataItem = currentBodyDataSource.Current;
                var columnValues = new SortedList<int, object>();
                currentTable.Rows.Add(columnValues);
                sno += 1;
                foreach (var citem in item.Items)
                {
                    if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                    {
                        bool remove = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                            remove = show != null && !(bool)show;
                        }
                        if (!remove && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                remove = true;
                        }
                        if (!remove)
                        {
                            object currentvalue = null;
                            if (citem.Format.Length > 0)
                                currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                            else
                                currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);
                            if (currentvalue == null && citem.Name == "sno")
                                currentvalue = sno;
                            if (currentvalue == null || currentvalue.ToString() == "01-01-0001" || currentvalue.ToString() == "01/01/0001") currentvalue = "";
                            if (!string.IsNullOrEmpty(citem.Prefix)) currentvalue = citem.Prefix + currentvalue.ToString();
                            if (!string.IsNullOrEmpty(citem.Suffix)) currentvalue = currentvalue.ToString() + citem.Suffix;
                            if (citem.TitleCase) currentvalue = TitleCase(currentvalue.ToString());
                            if (!string.IsNullOrEmpty(citem.HideValue))
                            {
                                object hideValue = ReportLibrary.DataSourceHelper.Eval(DataItem, citem.HideValue);
                                if (IsNumericType(hideValue.GetType()))
                                {
                                    if (Convert.ToDecimal(hideValue) == 0)
                                        currentvalue = "";
                                }
                                else if (hideValue.GetType().Name == "Boolean" && (bool)hideValue == true)
                                {
                                    currentvalue = "";
                                }
                            }
                            columnValues.Add(columnValues.Count, currentvalue);
                        }
                        //else
                        //{
                        //    columnValues.Add(columnValues.Count, null);
                        //}
                    }

                    if (citem.Type == DosFormat.CotnentType.Word)
                    {
                        bool remove = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                            remove = show != null && !(bool)show;
                        }
                        if (!remove && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                remove = true;
                        }
                        if (!remove)
                        {
                            var currentvalue = citem.Render(DataItem, Additional);
                            columnValues.Add(columnValues.Count, currentvalue);
                        }
                    }
                }
            }
            RenderTable(e,
                format.PageBody.Length == 0 ? currentTable.Rows.Count : format.PageBody.Length,
                currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight);

        }
        public void OnPrintLabelPage(System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (currentDataSourceList == null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(dataSourceList, null);
                currentDataSourceList = ie.GetEnumerator();
                currentDataSourceList.Reset();
                if (!currentDataSourceList.MoveNext())
                    return;
                DataSource = currentDataSourceList.Current;
            }

            currentY = e.MarginBounds.Top;
            currentX = e.MarginBounds.Left;
            currentLeft = e.MarginBounds.Left;
            float endY = e.MarginBounds.Bottom;
            endX = e.MarginBounds.Right;
            defaultHeight = GetRowHeight(BodyStyle.Font, e);

            if (format.PageBody == null)
                format.PageBody = new DosFormat.Body()
                {
                    Items = new List<DosFormat.Line>()
                };

            e.HasMorePages = true;
            format.PageBody.AvailableRows = 0;

            while (true)
            {
                if (format.PageHeader != null && !format.PageHeader.OnlyBOP)
                {
                    PrintHeader(e, format.PageHeader, currentLeft, e.MarginBounds.Width);
                }
                float labelheight = currentY;
                if (format.PageSetting.NoofColumns == 0)
                {
                    var headertop = currentY;
                    var left = currentX;
                    for (var currentbodyIndex = 0; currentbodyIndex < format.PageBody.Items.Count; currentbodyIndex++)
                    {
                        var item = format.PageBody.Items[currentbodyIndex];
                        currentX = currentLeft;
                        if (item.ResetTop)
                        {
                            currentY = headertop;
                        }
                        if (item.Top > 0)
                        {
                            currentY = headertop + item.Top;
                        }
                        if (item.Bottom > 0)
                        {
                            currentY = e.PageBounds.Bottom - item.Bottom;
                        }
                        if (item.Left != 0)
                            currentX = left + item.Left;

                        if (item.IsTable)
                        {
                            RenderLabelTable(item, e);
                        }
                        else
                        {
                            foreach (var citem in item.Items)
                            {
                                RenderContent(citem, DataSource, e, currentLeft, endX, e.MarginBounds.Width);
                            }
                            currentY += defaultHeight;
                        }
                    }
                    if (currentDataSourceList.MoveNext())
                    {
                        DataSource = currentDataSourceList.Current;
                    }
                    else
                    {
                        e.HasMorePages = false;
                        break;
                    }
                }
                else
                {
                    for (int columnindex = 0; columnindex < format.PageSetting.NoofColumns; columnindex++)
                    {
                        currentY = labelheight;
                        for (var currentbodyIndex = 0; currentbodyIndex < format.PageBody.Items.Count; currentbodyIndex++)
                        {
                            var item = format.PageBody.Items[currentbodyIndex];
                            currentX = currentLeft + (columnindex * format.PageSetting.ColumnWidth);
                            foreach (var citem in item.Items)
                            {
                                RenderContent(citem, DataSource, e, currentLeft, endX, e.MarginBounds.Width);
                            }
                            currentY += defaultHeight;
                        }

                        DrawCell(currentLeft + (columnindex * format.PageSetting.ColumnWidth),
                            labelheight, format.PageSetting.ColumnWidth, format.PageSetting.ColumnHeight);
                        if (currentDataSourceList.MoveNext())
                        {
                            DataSource = currentDataSourceList.Current;
                        }
                        else
                        {
                            e.HasMorePages = false;
                            break;
                        }
                    }
                    currentY = labelheight + format.PageSetting.ColumnHeight;
                }
                if (format.PageFooter != null)
                {
                    PrintFooter(e, format.PageFooter, e.MarginBounds.Left, e.MarginBounds.Width);
                }
                if (format.PageSetting.NoofColumns == 0)
                {
                    break;
                }
                else
                {
                    if (e.HasMorePages == true)
                    {
                        if (currentY + format.PageSetting.ColumnHeight > e.MarginBounds.Bottom)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public void OnPrintSingleLabelPage(System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (currentDataSourceList == null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(dataSourceList, null);
                currentDataSourceList = ie.GetEnumerator();
                currentDataSourceList.Reset();
                if (!currentDataSourceList.MoveNext())
                    return;
                DataSource = currentDataSourceList.Current;
            }
            currentY = e.MarginBounds.Top;
            currentX = e.MarginBounds.Left;
            currentLeft = e.MarginBounds.Left;
            float endY = e.MarginBounds.Bottom;
            endX = e.MarginBounds.Right;
            defaultHeight = GetRowHeight(BodyStyle.Font, e);

            PrintHeader(e, format.PageHeader, currentLeft, e.MarginBounds.Width);

            if (format.PageBody != null &&
                format.PageBody.Items != null)
            {
                for (currentbodyIndex = 0; currentbodyIndex < format.PageBody.Items.Count; currentbodyIndex++)
                {
                    var item = format.PageBody.Items[currentbodyIndex];
                    foreach (var citem in item.Items)
                    {
                        RenderContent(citem, DataSource, e, currentLeft, endX, e.MarginBounds.Width);
                    }
                }
            }
            PrintFooter(e, format.PageFooter, e.MarginBounds.Left, e.MarginBounds.Width);
            if (currentDataSourceList.MoveNext())
            {
                DataSource = currentDataSourceList.Current;
                e.HasMorePages = true;
            }
        }

        public override void OnPrintPage(System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (Mode == PrintingMode.Label)
            {
                OnPrintLabelPage(e);
                return;
            }

            if (Mode == PrintingMode.Single)
            {
                OnPrintSingleLabelPage(e);
                return;
            }
            if (currentDataSourceList == null && DataSource == null && dataSourceList != null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(dataSourceList, null);
                currentDataSourceList = ie.GetEnumerator();
                currentDataSourceList.Reset();
                currentDataSourceList.MoveNext();
                DataSource = currentDataSourceList.Current;
                DataItems = ReportLibrary.DataSourceHelper.Eval(DataSource, DataSourceItemMember);
            }

            if (currentBodyDataSource == null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(DataItems, null);
                if (ie != null)
                    currentBodyDataSource = ie.GetEnumerator();
            }

            bool HasLastVertical = false;
            float lastVerticalY = 0;
            EnablePageNumber = false;
            currentY = e.MarginBounds.Top;
            currentX = e.MarginBounds.Left;
            currentLeft = e.MarginBounds.Left;
            float endY = e.MarginBounds.Bottom;
            endX = e.MarginBounds.Right;
            defaultHeight = GetRowHeight(BodyStyle.Font, e);
            if (format.Columnes != null && format.Columnes.Count > 0)
            {
                float columnwidth = 0;
                currentLeft = e.MarginBounds.Left - 20;
                currentX = currentLeft;
                for (int i = 0; i < format.Columnes.Count; i++)
                {
                    currentY = e.MarginBounds.Top;
                    currentX = currentLeft + columnwidth + (i * 30);
                    float width = (float)e.MarginBounds.Width * (float)format.Columnes[i].width / 100;
                    endX = currentX + width;
                    PrintColumn(format.Columnes[i], currentX, width, e);
                    columnwidth += width;
                    if (i != format.Columnes.Count - 1)
                    {
                        PDFGraphic.DrawDottedLine(Pens.Black, currentX + width + 15, 0, currentX + width + 15, e.PageBounds.Bottom);
                    }
                }
                e.HasMorePages = false;
                if (currentDataSourceList != null)
                {
                    if (currentDataSourceList.MoveNext())
                    {
                        e.HasMorePages = true;
                        DataSource = currentDataSourceList.Current;
                        DataItems = ReportLibrary.DataSourceHelper.Eval(DataSource, DataSourceItemMember);
                        currentBodyDataSource = null;
                    }
                }
                return;
                //e.HasMorePages = false;
                //PageCount = 1;
                //currentpage = 1;
                //return;
            }
            else if (format.Rows != null && format.Rows.Count > 0)
            {
                float rowheight = 0;

                currentY = e.MarginBounds.Top;
                for (int i = 0; i < format.Rows.Count; i++)
                {
                    currentX = e.MarginBounds.Left;
                    endX = e.MarginBounds.Right;
                    currentY = e.MarginBounds.Top + rowheight + (i * 30);
                    float height = (float)e.MarginBounds.Height * (float)format.Rows[i].height / 100;
                    PrintColumn(format.Rows[i], currentX, e.MarginBounds.Width, e);
                    rowheight += height;
                    if (i != format.Rows.Count - 1)
                    {
                        PDFGraphic.DrawDottedLine(Pens.Black, e.MarginBounds.Left, rowheight, e.MarginBounds.Right, rowheight);
                    }
                }
                e.HasMorePages = false;
                PageCount = 1;
                currentpage = 1;
                return;
            }
            if (currentpage == 1)
            {
                currentRow = 0;
                currentIndex = 0;

                if (!defaultLeft.HasValue)
                    defaultLeft = currentLeft;

                PrintHeader(e, format.PageHeader, currentLeft, e.MarginBounds.Width);
            }
            else
            {
                if (format.PageHeader != null && !format.PageHeader.OnlyBOP)
                {
                    PrintHeader(e, format.PageHeader, currentLeft, e.MarginBounds.Width);
                }
                currentTableRow = _bodycurrentTableRow;
                currentTable = _bodycurrentTable;
                _bodycurrentTableRow = 0;
                _bodycurrentTable = null;
                if (_footerprint)
                {
                    if (currentTable != null && currentTableRow < currentTable.Rows.Count)
                    {
                        if (RenderTable(e, format.PageBody.Length, currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight))
                        {
                            currentY += defaultHeight;
                            e.HasMorePages = true;
                            currentpage++;
                            _bodycurrentTableRow = currentTableRow;
                            _bodycurrentTable = currentTable;
                            return;
                        }
                    }
                    _footerprint = false;
                    PrintFooter(e, format.PageFooter, e.MarginBounds.Left, e.MarginBounds.Width);
                    PageCount = currentpage;
                    currentpage = 1;
                    return;
                }
                else if(_continuebody)
                {
                    _continuebody = false;
                }
                else
                {
                    if (RenderTable(e, format.PageBody.Length, currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight))
                    {
                        currentY += defaultHeight;
                        e.HasMorePages = true;
                        currentpage++;
                    }
                    else
                    {
                        if (currentTable != null)
                            currentTable.FooterRowsAdjusted = false;
                        currentY += defaultHeight;
                        currentX = currentLeft;
                        currentbodyIndex++;
                    }
                    currentIndex++;
                }
            }

            bool hasbody = format.PageBody != null;
            if (format.PageBody == null)
                format.PageBody = new DosFormat.Body()
                {
                    Items = new List<DosFormat.Line>()
                };

            format.PageBody.AvailableRows = 0;
            //if (currentpage == 1)
            //{
            if (hasbody && !e.HasMorePages)
            {
                ShowGrid = !format.PageBody.DisableGrid;
                ShowVerticleLine = format.PageBody.VerticleGrid;
                ShowAlternateRowColour = format.PageBody.AlternateRowColour;
                for (; currentbodyIndex < format.PageBody.Items.Count; currentbodyIndex++)
                {
                    var item = format.PageBody.Items[currentbodyIndex];
                    if (!item.IsVerticalTable && !item.IsVerticalLine && HasLastVertical)
                    {
                        currentY = lastVerticalY;
                        HasLastVertical = false;
                        currentX = currentLeft;
                    }
                    if (item.IsTable)
                    {
                        if (!string.IsNullOrEmpty(item.field))
                        {

                            var tablesource = ReportLibrary.DataSourceHelper.Eval(DataSource, item.field);
                            if (tablesource != null)
                            {
                                ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(tablesource, null);
                                if (ie != null)
                                    currentBodyDataSource = ie.GetEnumerator();

                            }
                        }
                        if (item.Thermal)
                        {
                            currentX = currentLeft;
                            RenderThermalTable(e, currentBodyDataSource, item);
                            currentX = currentLeft;
                        }
                        else
                        {
                            currentTable = new Table();
                            currentTable.Thermal = item.Thermal;
                            currentTable.ShowHeaderBottomLine = format.PageBody.DisableGrid && !format.PageBody.VerticleGrid;
                            currentTable.AutoFitHeight = format.PageBody.WrapRow;
                            currentTable.FooterRows = format.PageBody.FooterRows;

                            currentTableRow = 0;
                            List<DosFormat.Content> removable = new List<DosFormat.Content>();
                            foreach (var citem in item.Items)
                            {
                                if (citem.Group)
                                    currentTable.GroupColumn = currentTable.Columns.Count;

                                if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                                {
                                    bool hide = false;
                                    if (!string.IsNullOrEmpty(citem.Show))
                                    {
                                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                                        if (show != null && !(bool)show)
                                            hide = true;
                                    }
                                    if (!hide && !string.IsNullOrEmpty(citem.Hide))
                                    {
                                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                                        if (show != null && (bool)show)
                                            hide = true;
                                    }
                                    if (hide)
                                    {
                                        removable.Add(citem);
                                        continue;
                                    }
                                    var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                                    clmn.TitleCase = citem.TitleCase;
                                    clmn.BodyFont = item.GetStyle();
                                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                                    {
                                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                                    }
                                    if (clmn.BodyFont != null)
                                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                                    if (citem.BottomColumn)
                                    {
                                        currentTable.BottomColumn = currentTable.Columns.Count;
                                    }
                                    clmn.Multiline = citem.Multiline == "true";
                                    clmn.MergeCell = citem.MergeCell;
                                    clmn.MergeText = citem.MergeText;
                                    clmn.Prefix = citem.Prefix;
                                    clmn.Suffix = citem.Suffix;
                                    currentTable.Columns.Add(clmn);
                                    if (clmn.ShowTotal)
                                        currentTable.Total.Add(currentTable.Columns.Count - 1, 0);
                                }
                                else if (citem.Type == DosFormat.CotnentType.Word)
                                {
                                    bool hide = false;
                                    if (!string.IsNullOrEmpty(citem.Show))
                                    {
                                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                                        if (show != null && !(bool)show)
                                            hide = true;
                                    }
                                    if (!hide && !string.IsNullOrEmpty(citem.Hide))
                                    {
                                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                                        if (show != null && (bool)show)
                                            hide = true;
                                    }
                                    if (hide)
                                    {
                                        removable.Add(citem);
                                        continue;
                                    }
                                    var clmn = new Column(citem.Name, citem.Name, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                                    clmn.TitleCase = citem.TitleCase;
                                    clmn.BodyFont = item.GetStyle();
                                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                                    {
                                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                                    }
                                    if (clmn.BodyFont != null)
                                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                                    if (citem.BottomColumn)
                                    {
                                        currentTable.BottomColumn = currentTable.Columns.Count;
                                    }
                                    clmn.Multiline = citem.Multiline == "true";
                                    clmn.MergeCell = citem.MergeCell;
                                    clmn.MergeText = citem.MergeText;
                                    clmn.Prefix = citem.Prefix;
                                    clmn.Suffix = citem.Suffix;
                                    currentTable.Columns.Add(clmn);
                                }
                            }
                            removable.ForEach(ex => item.Items.Remove(ex));

                            int sno = 0;
                            while (currentBodyDataSource != null && currentBodyDataSource.MoveNext())
                            {
                                DataItem = currentBodyDataSource.Current;
                                var columnValues = new SortedList<int, object>();
                                currentTable.Rows.Add(columnValues);
                                sno += 1;
                                foreach (var citem in item.Items)
                                {
                                    if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                                    {
                                        bool remove = false;
                                        if (!string.IsNullOrEmpty(citem.Show))
                                        {
                                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                                            remove = show != null && !(bool)show;
                                        }
                                        if (!remove && !string.IsNullOrEmpty(citem.Hide))
                                        {
                                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                                            if (show != null && (bool)show)
                                                remove = true;
                                        }
                                        if (!remove)
                                        {
                                            object currentvalue = null;
                                            if (citem.Format.Length > 0)
                                                currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                                            else
                                                currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);
                                            if (currentvalue == null && citem.Name == "sno")
                                                currentvalue = sno;
                                            if (currentvalue == null || currentvalue.ToString() == "01-01-0001" || currentvalue.ToString() == "01/01/0001") currentvalue = "";
                                            if (!string.IsNullOrEmpty(citem.Prefix)) currentvalue = citem.Prefix + currentvalue.ToString();
                                            if (!string.IsNullOrEmpty(citem.Suffix)) currentvalue = currentvalue.ToString() + citem.Suffix;
                                            if (citem.TitleCase) currentvalue = TitleCase(currentvalue.ToString());
                                            if (!string.IsNullOrEmpty(citem.HideValue))
                                            {
                                                object hideValue = ReportLibrary.DataSourceHelper.Eval(DataItem, citem.HideValue);
                                                if (IsNumericType(hideValue.GetType()))
                                                {
                                                    if (Convert.ToDecimal(hideValue) == 0)
                                                        currentvalue = "";
                                                }
                                                else if (hideValue.GetType().Name == "Boolean" && (bool)hideValue == true)
                                                {
                                                    currentvalue = "";
                                                }
                                            }
                                            columnValues.Add(columnValues.Count, currentvalue);
                                        }
                                        //else
                                        //{
                                        //    columnValues.Add(columnValues.Count, null);
                                        //}
                                    }

                                    if (citem.Type == DosFormat.CotnentType.Word)
                                    {
                                        bool remove = false;
                                        if (!string.IsNullOrEmpty(citem.Show))
                                        {
                                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                                            remove = show != null && !(bool)show;
                                        }
                                        if (!remove && !string.IsNullOrEmpty(citem.Hide))
                                        {
                                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                                            if (show != null && (bool)show)
                                                remove = true;
                                        }
                                        if (!remove)
                                        {
                                            var currentvalue = citem.Render(DataItem, Additional);
                                            columnValues.Add(columnValues.Count, currentvalue);
                                        }
                                    }
                                }
                            }
                            if (RenderTable(e,
                                format.PageBody.Length == 0 ? currentTable.Rows.Count : format.PageBody.Length,
                                currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight))
                            {
                                e.HasMorePages = true;
                                break;
                            }
                            else if (currentTable != null && currentTable.FooterRowsAdjusted)
                            {
                                e.HasMorePages = true;
                                break;
                            }
                            else
                            {

                                //currentY += defaultHeight;
                                currentX = currentLeft;
                            }
                        }
                    }
                    else if (item.IsVerticalLine)
                    {
                        DrawVerticalLine(item, e);
                        continue;
                    }
                    else if (item.IsVerticalTable)
                    {
                        var lastY = currentY;
                        var lastX = currentX;
                        RenderVerticalTable(item, DataSource, lastX, e);
                        if (HasLastVertical)
                        {
                            if (lastVerticalY < item.Height)
                                lastVerticalY = lastY + item.Height;
                        }
                        else
                        {
                            HasLastVertical = true;
                            lastVerticalY = lastY + item.Height;
                        }
                        currentY = lastY;
                        currentX = lastX + item.Width;
                        continue;
                    }
                    else
                    {
                        foreach (var citem in item.Items)
                        {
                            RenderContent(citem, DataSource, e, currentLeft, endX, e.MarginBounds.Width);
                        }
                        currentX = currentLeft;
                        if (!item.Items.TrueForAll(ex => ex.Type == DosFormat.CotnentType.Image))
                            currentY += defaultHeight;

                        if((currentY + (2 * defaultHeight)) > e.MarginBounds.Bottom)
                        {
                            _bodycurrentTableRow = -1;
                            _bodycurrentTable = null;
                            e.HasMorePages = true;
                            _continuebody = true;
                            currentpage++;
                            return;
                        }
                    }
                    format.PageBody.AvailableRows++;
                }
                //currentY += defaultHeight;
                currentX = currentLeft;
            }
            //}
            //else
            //{
            //    currentY += defaultHeight;
            //    currentX = currentLeft;
            //}

            _bodycurrentTableRow = currentTableRow;
            _bodycurrentTable = currentTable;

            if (currentTable == null || (format.PageFooter.EOD && !e.HasMorePages && !currentTable.FooterRowsAdjusted) || (!format.PageFooter.EOD && !currentTable.FooterRowsAdjusted))
            {
                if (format.PageFooter != null && format.PageFooter.Height > 0)
                {
                    if ((currentY + format.PageFooter.Height + FixedRowHeight) > e.MarginBounds.Bottom)
                    {

                        _bodycurrentTableRow = -1;
                        _bodycurrentTable = null;
                        e.HasMorePages = true;
                        _footerprint = true;
                        currentpage++;
                        return;
                    }
                }
                else
                {
                    PrintFooter(e, format.PageFooter, e.MarginBounds.Left, e.MarginBounds.Width);
                }
            }

            if (currentDataSourceList != null)
            {
                if (currentDataSourceList.MoveNext())
                {
                    e.HasMorePages = true;
                    DataSource = currentDataSourceList.Current;
                    DataItems = ReportLibrary.DataSourceHelper.Eval(DataSource, DataSourceItemMember);
                    currentBodyDataSource = null;
                }
            }

            //e.HasMorePages = !(currentIndex + 1 >= BillPrintInfo.PrintDesign.Count);
            //ShowPageNumber(!e.HasMorePages, defaultHeight, e);
            if (e.HasMorePages)
            {
                currentpage++;
            }
            else
            {
                PageCount = currentpage;
                currentpage = 1;
            }
        }

        private bool IsNumericType(Type type)
        {
            TypeCode typeCode = System.Type.GetTypeCode(type);
            //The TypeCode of numerical types are between SByte (5) and Decimal (15).
            return (int)typeCode >= 5 && (int)typeCode <= 15;
        }
        private void DrawVerticalLine(DosFormat.Line item, PrintPageEventArgs e)
        {
            if (endX < item.Width)
                item.Width = (int)endX - (int)currentX;

            Pen pen = new Pen(Color.Black);
            pen.Width = 1;

            if (item.Top != 0)
                PDFGraphic.DrawLine(pen, currentX, item.Top, currentX + item.Width,
                    item.Top + item.Height);
            if (item.Bottom != 0)
                PDFGraphic.DrawLine(pen, currentX, e.PageBounds.Bottom - item.Bottom, currentX + item.Width,
                    e.PageBounds.Bottom - item.Bottom + item.Height);
            else
                PDFGraphic.DrawLine(pen, currentX, currentY, currentX + item.Width,
                    currentY + item.Height);
        }

        private void RenderVerticalTable(DosFormat.Line item, object dataSource, float lastX, PrintPageEventArgs e)
        {
            foreach (var citem in item.Items)
            {
                RenderContent(citem, DataSource, e, lastX, lastX + item.Width, item.Width);
            }
        }

        public override int PageHeightByContent
        {
            get
            {
                return (int)Math.Ceiling(currentY);
            }
        }

        int _bodycurrentTableRow = -1;
        bool _footerprint = false;
        bool _continuebody = false;
        Table _bodycurrentTable = null;
        private void PrintColumn(DosFormat.Column column, float left, float width, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (currentBodyDataSource == null && DataItems != null)
            {
                var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(DataItems, null);
                currentBodyDataSource = ie.GetEnumerator();
            }
            if (currentBodyDataSource != null)
            {
                currentBodyDataSource.Reset();
            }
            currentbodyIndex = 0;
            currentTableRow = 0;
            currentRow = 0;
            currentIndex = 0;

            defaultLeft = left;

            if (column.border)
            {
                Rectangle bodyrectanglen = new Rectangle((int)left - 5, e.MarginBounds.Top, (int)width + 10, (int)column.height);
                PDFGraphic.DrawRectangle(Pens.Black, bodyrectanglen);
            }

            PrintHeader(e, column.PageHeader, left, width);


            if (column.PageBody == null)
            {
                column.PageBody = new DosFormat.Body();
                column.PageBody.Items = new List<DosFormat.Line>();
            }
            column.PageBody.AvailableRows = 0;
            if (currentpage == 1)
            {
                for (; currentbodyIndex < column.PageBody.Items.Count; currentbodyIndex++)
                {
                    var item = column.PageBody.Items[currentbodyIndex];
                    if (item.IsTable)
                    {
                        currentTable = new Table();
                        currentTable.AutoFitHeight = format.PageBody.WrapRow;
                        foreach (var citem in item.Items)
                        {
                            if (citem.Type == DosFormat.CotnentType.Field)
                            {
                                var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, StringAlignment.Near, citem.Total, citem.Length);

                                clmn.BodyFont = item.GetStyle();
                                if (clmn.BodyFont != null)
                                    clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);

                                if (citem.Align == "right")
                                {
                                    clmn.Alignment = StringAlignment.Far;
                                }
                                currentTable.Columns.Add(clmn);
                            }
                        }

                        while (currentBodyDataSource.MoveNext())
                        {
                            DataItem = currentBodyDataSource.Current;
                            var columnValues = new SortedList<int, object>();
                            currentTable.Rows.Add(columnValues);
                            foreach (var citem in item.Items)
                            {
                                if (citem.Type == DosFormat.CotnentType.Field)
                                {
                                    object currentvalue = null;
                                    if (citem.Format.Length > 0)
                                        currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                                    else
                                        currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);

                                    columnValues.Add(columnValues.Count, currentvalue);
                                }
                            }
                        }
                        if (RenderTable(e, column.PageBody.Length, currentX, column.PageBody.HeaderHeight, column.PageBody.LineHeight))
                        {
                            e.HasMorePages = true;
                            break;
                        }
                    }
                    else
                    {
                        while (currentBodyDataSource.MoveNext())
                        {
                            DataItem = currentBodyDataSource.Current;
                            foreach (var citem in item.Items)
                            {
                                RenderContent(citem, DataSource, e, left, endX, width);
                            }
                            currentX = left;
                            currentY += defaultHeight;
                        }
                    }
                    column.PageBody.AvailableRows++;
                }
                currentY += defaultHeight;
                currentX = left;
            }
            else
            {
                currentY += defaultHeight;
                currentX = left;
            }
            if (column.PageFooter == null)
            {
                column.PageFooter = new DosFormat.Footer();
                column.PageFooter.Items = new List<DosFormat.Line>();
            }
            PrintFooter(e, column.PageFooter, left, width);
        }

        private void RenderContent(DosFormat.Content citem, object currentDS, PrintPageEventArgs e, float leftx, float endX, float width)
        {
            if (currentX + citem.Length > endX)
                citem.Length = (int)(endX - currentX);

            if (citem.Length < 0)
                citem.Length = 0;

            switch (citem.Type)
            {
                case DosFormat.CotnentType.Field:
                    object columnvalue = null;
                    if (citem.Format.Length > 0)
                        columnvalue = DataSourceHelper.Eval(currentDS, citem.Name, "{0:" + citem.Format + "}");
                    else
                        columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                    if (columnvalue == null || columnvalue.ToString() == "01-01-0001" || columnvalue.ToString() == "01/01/0001") columnvalue = "";
                    if (columnvalue != null)
                    {
                        if (!string.IsNullOrEmpty(citem.Prefix)) columnvalue = citem.Prefix + columnvalue.ToString();
                        if (!string.IsNullOrEmpty(citem.Suffix)) columnvalue = columnvalue.ToString() + citem.Suffix;
                        DisplayText(citem, columnvalue.ToString(), citem.Length, citem.GetHeight(defaultHeight), citem.GetAlignment(), e);
                    }
                    break;
                case DosFormat.CotnentType.LineBreak:
                    currentY += defaultHeight;
                    currentX = leftx;
                    return;
                    break;
                case DosFormat.CotnentType.Line:
                    currentElement = new DesignerElement(citem);
                    currentElement.Top = (float)citem.Top;
                    currentElement.Left = currentX;
                    currentElement.Width = citem.Length > 0 ? citem.Length : (int)width;
                    PrintLine(e.Graphics, e);
                    break;
                case DosFormat.CotnentType.Table:
                    columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                    RenderTable(e, citem, columnvalue, leftx, endX, width);
                    break;
                case DosFormat.CotnentType.Barcode:
                case DosFormat.CotnentType.PlainBarcode:
                    columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                    if (columnvalue != null)
                        Draw1D(columnvalue.ToString(), currentX, currentY, citem.Length, citem.GetHeight(defaultHeight));
                    break;
                case DosFormat.CotnentType.QRBarcode:
                    columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                    if (columnvalue != null)
                        Draw2D(columnvalue.ToString(), currentX, currentY, citem.Length, citem.GetHeight(defaultHeight));
                    break;
                case DosFormat.CotnentType.Duplicate:
                    string stringText = PrintUtils.Replicate(citem.Text, citem.Length);
                    DisplayText(citem, stringText, citem.Length, defaultHeight, citem.GetAlignment(), e);
                    break;
                case DosFormat.CotnentType.Word:
                    DisplayText(citem, WordParse(citem.Text, currentDS, null), citem.Length, citem.GetHeight(defaultHeight), citem.GetAlignment(), e);
                    break;
                case DosFormat.CotnentType.Image:
                    if (string.IsNullOrEmpty(citem.Source) && !string.IsNullOrEmpty(citem.Name))
                    {
                        columnvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                        if (columnvalue != null)
                        {
                            citem.Source = columnvalue.ToString();
                            if (!string.IsNullOrEmpty(citem.Source) && !Additional.ContainsKey("IMG-" + citem.Source))
                            {
                                var imgdata = this.imageData.GetData(citem.Source.LongFromBase62());
                                var img = imageData.FromBytes(imgdata.data);
                                Additional.Add("IMG-" + citem.Source, img);
                            }
                            drawImage(citem);
                            citem.Source = "";
                        }
                    }
                    else
                    {
                        drawImage(citem);
                    }
                    break;
                case DosFormat.CotnentType.Logo:
                    if (citem.Align == "center")
                    {
                        var parts = citem.Text.Split(',');
                        if (parts.Length == 4 && Additional != null && Additional.ContainsKey("LOGO"))
                        {
                            var leftcenter = (e.PageBounds.Width / 2) - (int.Parse(parts[2]) / 2);
                            Rectangle bodyrectanglen = new Rectangle(leftcenter, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                            PDFGraphic.DrawImageUnscaled(ResizeImage(Additional["LOGO"] as Image, bodyrectanglen, citem.Grayscale), bodyrectanglen);
                            currentY += bodyrectanglen.Height;
                        }
                    }
                    else
                    {
                        drawlogo(citem);
                    }
                    break;
                case DosFormat.CotnentType.Rectangle:

                    var rparts = citem.Text.Split(',');
                    if (rparts.Length == 4)
                    {
                        var top = int.Parse(rparts[1]);
                        Rectangle bodyrectanglen = new Rectangle(int.Parse(rparts[0]), top + (int)currentY, int.Parse(rparts[2]), int.Parse(rparts[3]));
                        var height = 0.1f;
                        if (citem.Height > 0)
                            height = ((float)citem.Height) / 100;
                        Pen vbPen = new Pen(Color.Black, height);
                        PDFGraphic.DrawRectangle(vbPen, bodyrectanglen);
                    }
                    break;

                case DosFormat.CotnentType.Paragraph:
                    StringToPrint = citem.Text;
                    if (string.IsNullOrEmpty(citem.Text))
                    {
                        object cvalue = null;
                        if (citem.Format.Length > 0)
                            cvalue = DataSourceHelper.Eval(currentDS, citem.Name, "{0:" + citem.Format + "}");
                        else
                            cvalue = DataSourceHelper.Eval(currentDS, citem.Name);
                        if (cvalue != null)
                        {
                            if (!string.IsNullOrEmpty(citem.Prefix)) cvalue = citem.Prefix + cvalue.ToString();
                            if (!string.IsNullOrEmpty(citem.Suffix)) cvalue = cvalue.ToString() + citem.Suffix;
                            StringToPrint = cvalue.ToString();
                        }
                    }
                    currentElement = new DesignerElement(citem);
                    currentElement.Width = citem.Length;
                    currentElement.font = citem.GetStyle();
                    while (DisplayMultiLine(e)) ;
                    break;
            }
            if (citem.EOL)
            {
                currentY += defaultHeight;
                currentX = leftx;
            }
            else
            {
                currentX += citem.Length;
            }
        }

        private bool RenderTable(PrintPageEventArgs e, DosFormat.Content item, object columnvalue,
            float leftx, float endx, float width)
        {
            currentTable = new Table();
            currentTable.ShowHeaderBottomLine = format.PageBody.DisableGrid && !format.PageBody.VerticleGrid;
            currentTable.AutoFitHeight = format.PageBody.WrapRow;
            currentTableRow = 0;
            List<DosFormat.Content> removable = new List<DosFormat.Content>();
            foreach (var citem in item.Items)
            {
                if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                {
                    bool hide = false;
                    if (!string.IsNullOrEmpty(citem.Show))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                        if (show != null && !(bool)show)
                            hide = true;
                    }
                    if (!hide && !string.IsNullOrEmpty(citem.Hide))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                        if (show != null && (bool)show)
                            hide = true;
                    }
                    if (hide)
                    {
                        removable.Add(citem);
                        continue;
                    }
                    var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                    clmn.BodyFont = item.GetStyle();
                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                    {
                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                    }
                    if (clmn.BodyFont != null)
                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                    if (citem.BottomColumn)
                        currentTable.BottomColumn = currentTable.Columns.Count;
                    clmn.MergeCell = citem.MergeCell;
                    clmn.MergeText = citem.MergeText;
                    clmn.Prefix = citem.Prefix;
                    clmn.Suffix = citem.Suffix;
                    currentTable.Columns.Add(clmn);
                    if (clmn.ShowTotal)
                        currentTable.Total.Add(currentTable.Columns.Count - 1, 0);
                }
            }
            removable.ForEach(ex => item.Items.Remove(ex));

            int sno = 0;
            while (currentBodyDataSource != null && currentBodyDataSource.MoveNext())
            {
                DataItem = currentBodyDataSource.Current;
                var columnValues = new SortedList<int, object>();
                currentTable.Rows.Add(columnValues);
                sno += 1;
                foreach (var citem in item.Items)
                {
                    if (citem.Type == DosFormat.CotnentType.Field || citem.Type == DosFormat.CotnentType.Dynamictable)
                    {
                        bool remove = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataItem, citem.Show);
                            remove = show != null && !(bool)show;
                        }
                        if (!remove && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                remove = true;
                        }
                        if (!remove)
                        {
                            object currentvalue = null;
                            if (citem.Format.Length > 0)
                                currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                            else
                                currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);
                            if (currentvalue == null && citem.Name == "sno")
                                currentvalue = sno;
                            if (currentvalue == null || currentvalue.ToString() == "01-01-0001" || currentvalue.ToString() == "01/01/0001") currentvalue = "";
                            if (!string.IsNullOrEmpty(citem.Prefix)) currentvalue = citem.Prefix + currentvalue.ToString();
                            if (!string.IsNullOrEmpty(citem.Suffix)) currentvalue = currentvalue.ToString() + citem.Suffix;
                            columnValues.Add(columnValues.Count, currentvalue);
                        }
                        else
                        {
                            columnValues.Add(columnValues.Count, null);
                        }
                    }
                }
            }
            if (RenderTable(e,
                format.PageBody.Length == 0 ? currentTable.Rows.Count : format.PageBody.Length,
                currentX, format.PageBody.HeaderHeight, format.PageBody.LineHeight))
            {
                e.HasMorePages = true;
                return false;
            }
            return true;
        }
        public static Bitmap MakeGrayscale(Bitmap original)
        {
            Bitmap newBmp = new Bitmap(original.Width, original.Height);
            Graphics g = Graphics.FromImage(newBmp);
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                   new float[] {1.5f, 1.5f, 1.5f, 0, 0},
                   new float[] {1.5f, 1.5f, 1.5f, 0, 0},
                   new float[] {1.5f, 1.5f, 1.5f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] { -1, -1, -1, 0, 1 }
               });
            ImageAttributes img = new ImageAttributes();
            img.SetColorMatrix(colorMatrix);
            img.SetThreshold(0.9f); // Change this threshold as needed
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, img);
            g.Dispose();
            return newBmp;
        }
        private Image ResizeImage(Image img, Rectangle rect, bool Grayscale)
        {
            if (Grayscale)
                return MakeGrayscale(img as Bitmap);
            Bitmap bmPhoto = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(300, 300);
            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.DrawImage(img,
                    new Rectangle(0, 0, rect.Width, rect.Height),
                    new Rectangle(0, 0, img.Width, img.Height),
                    GraphicsUnit.Pixel);
            grPhoto.Dispose();
            return bmPhoto;
        }

        private void drawtable(DosFormat.Line line, PrintPageEventArgs e, float width)
        {
            if (line.Thermal)
            {
                drawthermaltable(line, e, width);
                return;
            }
            //foreach (var citem in line.Items)
            //{
            //    if (currentX + citem.Length > endX)
            //        citem.Length = (int)(endX - currentX);

            //    if (citem.Length < 0)
            //        citem.Length = 0;
            //    DrawCell(citem.Length, defaultHeight);
            //    RenderContent(citem, DataSource, e, width);
            //}

            currentTableRow = 0;
            currentTable = new Table();
            currentTable.Thermal = line.Thermal;
            currentTable.ShowHeaderBottomLine = !ShowGrid && !ShowVerticleLine;
            foreach (var citem in line.Items)
            {
                bool remove = false;
                if (!string.IsNullOrEmpty(citem.Show))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                    remove = show != null && !(bool)show;
                }
                if (!remove && !string.IsNullOrEmpty(citem.Hide))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                    if (show != null && (bool)show)
                        remove = true;
                }

                if (remove) continue;

                if (citem.Type == DosFormat.CotnentType.Field)
                {
                    var clmn = new Column(citem.Name, citem.Text, citem.DataType, citem.Format, citem.GetAlignment(), citem.Total, citem.Length);
                    clmn.BodyFont = line.GetStyle();
                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                    {
                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                    }
                    if (clmn.BodyFont != null)
                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                    if (citem.BottomColumn)
                        currentTable.BottomColumn = currentTable.Columns.Count;
                    clmn.MergeCell = citem.MergeCell;
                    clmn.MergeText = citem.MergeText;
                    clmn.Prefix = citem.Prefix;
                    clmn.Suffix = citem.Suffix;
                    currentTable.Columns.Add(clmn);
                }
                else
                {
                    var clmn = new Column("", citem.Text, citem.DataType, citem.Format, StringAlignment.Near, citem.Total, citem.Length);
                    clmn.BodyFont = line.GetStyle();
                    if (clmn.BodyFont == null && !string.IsNullOrEmpty(citem.Style))
                    {
                        clmn.BodyFont = citem.GetStyle(BodyStyle.Font);
                    }
                    if (clmn.BodyFont != null)
                        clmn.HeaderFont = new Font(clmn.BodyFont, FontStyle.Bold);
                    if (citem.BottomColumn)
                        currentTable.BottomColumn = currentTable.Columns.Count;
                    clmn.MergeCell = citem.MergeCell;
                    clmn.MergeText = citem.MergeText;
                    clmn.Prefix = citem.Prefix;
                    clmn.Suffix = citem.Suffix;
                    currentTable.Columns.Add(clmn);
                }
            }


            var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(ReportLibrary.DataSourceHelper.Eval(DataSource, line.field), "");
            if (ie == null) return;

            var tableDataSource = ie.GetEnumerator();
            while (tableDataSource.MoveNext())
            {
                DataItem = tableDataSource.Current;
                var columnValues = new SortedList<int, object>();
                currentTable.Rows.Add(columnValues);
                foreach (var citem in line.Items)
                {
                    bool remove = false;
                    if (!string.IsNullOrEmpty(citem.Show))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                        remove = show != null && !(bool)show;
                    }
                    if (!remove && !string.IsNullOrEmpty(citem.Hide))
                    {
                        var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                        if (show != null && (bool)show)
                            remove = true;
                    }

                    if (remove) continue;

                    if (citem.Type == DosFormat.CotnentType.Field)
                    {
                        object currentvalue = null;
                        if (citem.Format.Length > 0)
                            currentvalue = DataSourceHelper.Eval(DataItem, citem.Name, "{0:" + citem.Format + "}");
                        else
                            currentvalue = DataSourceHelper.Eval(DataItem, citem.Name);

                        if (currentvalue == null) currentvalue = "";
                        //if (citem.Total)
                        //{
                        //    decimal t;
                        //    if (decimal.TryParse(currentvalue.ToString(), out t))
                        //        currentTable.Total[columnValues.Count] = currentTable.Total[columnValues.Count] + t;
                        //}
                        if (!string.IsNullOrEmpty(citem.Prefix)) currentvalue = citem.Prefix + currentvalue.ToString();
                        if (!string.IsNullOrEmpty(citem.Suffix)) currentvalue = currentvalue.ToString() + citem.Suffix;

                        columnValues.Add(columnValues.Count, currentvalue);
                    }
                    else
                    {
                        columnValues.Add(columnValues.Count, citem.Text);
                    }
                }
            }

            currentTable.ShowHeader = !line.HideHeader;
            int minimumline = line.Length > 0 ? line.Length : currentTable.Rows.Count;
            RenderTable(e, minimumline, currentX);
        }

        private void drawImage(DosFormat.Content citem)
        {
            var parts = citem.Text.Split(',');

            if (parts.Length == 4 && Additional != null && Additional.ContainsKey("IMG-" + citem.Source))
            {

                Rectangle bodyrectanglen = new Rectangle(int.Parse(parts[0]) + (int)currentX, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));

                PDFGraphic.DrawImageUnscaled(Additional["IMG-" + citem.Source] as Image, bodyrectanglen);

            }
        }

        public void Draw2D(string value, float left, float top, float width, float height)
        {
            if (string.IsNullOrEmpty(value)) return;
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(4, Color.Black, Color.Transparent, false);
            PDFGraphic.DrawImageUnscaled(qrCodeImage, new Rectangle((int)left, (int)top, (int)width, (int)height));
        }
        public void Draw1D(string value, float left, float top, float width, float height)
        {
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, value, Color.Black, Color.Transparent, (int)width * 2, (int)(height * 1.5));
            PDFGraphic.DrawImageUnscaled(img, new Rectangle((int)left, (int)top, (int)width, (int)height));
        }

        private void drawlogo(DosFormat.Content citem)
        {
            var parts = citem.Text.Split(',');
            if (parts.Length == 4 && Additional != null && Additional.ContainsKey("LOGO"))
            {
                Rectangle bodyrectanglen = new Rectangle(int.Parse(parts[0]) + (int)currentX, int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
                PDFGraphic.DrawImageUnscaled(ResizeImage(Additional["LOGO"] as Image, bodyrectanglen, citem.Grayscale), bodyrectanglen);
            }
        }

        private void PrintHeader(PrintPageEventArgs e, DosFormat.Header header, float left, float width)
        {
            bool HasLastVertical = false;
            float lastVerticalY = 0;
            var headertop = currentY;
            foreach (var item in header.Items)
            {
                if (item.ResetTop)
                {
                    currentY = headertop;
                }
                if (item.Top > 0)
                {
                    currentY = headertop + item.Top;
                }
                if (item.Bottom > 0)
                {
                    currentY = e.PageBounds.Bottom - item.Bottom;
                }
                if (item.Left != 0)
                    currentX = left + item.Left;

                bool hide = false;
                if (!string.IsNullOrEmpty(item.Show))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Show);
                    if (show != null && !(bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.Hide))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Hide);
                    if (show != null && (bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.HasValue))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.HasValue);
                    if (show != null && !(bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.HasRange))
                {
                    decimal d;
                    var ranges = item.HasRange.Split('-').ToList().ConvertAll(x =>
                    {
                        decimal.TryParse(x, out d);
                        return d;
                    });
                    var rangevalue = ReportLibrary.DataSourceHelper.Eval(DataSource, item.field);
                    if (rangevalue != null)
                    {
                        if (decimal.TryParse(rangevalue.ToString(), out d))
                        {
                            hide = d > ranges[1] || d < ranges[0];
                        }
                    }
                }
                if (hide) continue;
                if (item.IsTable)
                {
                    drawtable(item, e, width);
                }
                else if (item.IsVerticalLine)
                {
                    DrawVerticalLine(item, e);
                }
                else if (item.IsVerticalTable)
                {
                    var lastY = currentY;
                    var lastX = currentX;
                    RenderVerticalTable(item, DataSource, lastX, e);
                    if (HasLastVertical)
                    {
                        if (lastVerticalY < item.Height)
                            lastVerticalY = lastY + item.Height;
                    }
                    else
                    {
                        HasLastVertical = true;
                        lastVerticalY = lastY + item.Height;
                    }
                    currentY = lastY;
                    currentX = lastX + item.Width;
                    continue;
                }
                else
                {
                    foreach (var citem in item.Items)
                    {
                        hide = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                            if (show != null && !(bool)show)
                                hide = true;
                        }
                        if (!hide && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                hide = true;
                        }
                        if (hide) continue;
                        RenderContent(citem, DataSource, e, left, endX, width);
                        if (citem.ResetLine)
                        {
                            currentY += defaultHeight;
                            if (item.Left != 0)
                                currentX = left + item.Left;
                            else
                                currentX = left;
                        }
                    }
                }
                currentY += defaultHeight;
                currentX = left;
            }

        }
        private void PrintFooter(PrintPageEventArgs e, DosFormat.Footer footer, float left, float width)
        {
            if (footer == null)
                return;
            currentX = left;
            var footertop = currentY;
            bool HasLastVertical = false;
            float lastVerticalY = 0;

            foreach (var item in footer.Items)
            {
                bool hide = false;
                float maxheight = 0;
                if (item.ResetTop)
                {
                    currentY = footertop;
                }
                if (item.Top > 0)
                {
                    currentY = footertop + item.Top;
                }
                if (item.Bottom > 0)
                {
                    currentY = e.PageBounds.Bottom - item.Bottom;
                }
                if (item.Left != 0)
                    currentX += item.Left;

                if (!string.IsNullOrEmpty(item.Show))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Show);
                    if (show != null && !(bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.Hide))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.Hide);
                    if (show != null && (bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.HasValue))
                {
                    var show = ReportLibrary.DataSourceHelper.Eval(DataSource, item.HasValue);
                    if (show != null && !(bool)show)
                        hide = true;
                }
                if (!hide && !string.IsNullOrEmpty(item.HasRange))
                {
                    decimal d;
                    var ranges = item.HasRange.Split('-').ToList().ConvertAll(x =>
                    {
                        decimal.TryParse(x, out d);
                        return d;
                    });
                    var rangevalue = ReportLibrary.DataSourceHelper.Eval(DataSource, item.field);
                    if (rangevalue != null)
                    {
                        if (decimal.TryParse(rangevalue.ToString(), out d))
                        {
                            hide = d > ranges[1] || d < ranges[0];
                        }
                    }
                }
                if (hide) continue;

                if (item.IsTable)
                {
                    drawtable(item, e, width);
                }
                else if (item.IsVerticalLine)
                {
                    DrawVerticalLine(item, e);
                }
                else if (item.IsVerticalTable)
                {
                    var lastY = currentY;
                    var lastX = currentX;
                    RenderVerticalTable(item, DataSource, lastX, e);
                    if (HasLastVertical)
                    {
                        if (lastVerticalY < item.Height)
                            lastVerticalY = lastY + item.Height;
                    }
                    else
                    {
                        HasLastVertical = true;
                        lastVerticalY = lastY + item.Height;
                    }
                    currentY = lastY;
                    currentX = lastX + item.Width;
                    continue;
                }
                else
                {
                    foreach (var citem in item.Items)
                    {
                        hide = false;
                        if (!string.IsNullOrEmpty(citem.Show))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Show);
                            if (show != null && !(bool)show)
                                hide = true;
                        }
                        if (!hide && !string.IsNullOrEmpty(citem.Hide))
                        {
                            var show = ReportLibrary.DataSourceHelper.Eval(DataSource, citem.Hide);
                            if (show != null && (bool)show)
                                hide = true;
                        }
                        if (hide) continue;
                        RenderContent(citem, DataSource, e, left, endX, width);
                        if (citem.GetHeight(defaultHeight) > maxheight)
                            maxheight = citem.GetHeight(defaultHeight);
                        if (citem.ResetLine)
                        {
                            currentY += defaultHeight;
                            if (item.Left != 0)
                                currentX = left + item.Left;
                            else
                                currentX = left;
                        }
                    }
                }
                currentY += maxheight > defaultHeight ? maxheight : defaultHeight;
                currentX = left;
            }

        }

        internal void ConvertSize2Thermal()
        {
            this.format.PageSetting.Height = 5000;
            ConvertSize2Thermal(this.format.PageHeader.Items);
            ConvertSize2Thermal(this.format.PageBody.Items);
            ConvertSize2Thermal(this.format.PageFooter.Items);
        }

        private void ConvertSize2Thermal(List<DosFormat.Line> items)
        {
            foreach (var line in items)
            {
                ConvertSize2Thermal(line);
            }
        }
        public override void Reset()
        {
            currentDataSourceList = null;
            currentBodyDataSource = null;
            currentbodyIndex = 0;
            currentTableRow = 0;
            currentTable = null;
            isTablePrinting = false;
            currentpage = 0;
            PageNo = 1;
            base.Reset();
        }
        private void ConvertSize2Thermal(DosFormat.Line line)
        {
            foreach (var condent in line.Items)
            {
                condent.Length = condent.Length * 10;
            }
        }
    }
}
