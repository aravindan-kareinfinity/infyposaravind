using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Drawing.Printing;
using InfyPOS.Common;
using WebAPI.Data;

namespace ReportLibrary
{
    [Serializable]
    public class HtmlFormat
    {

        public Font GetStyle(string Style)
        {
            string[] str = Style.Split(',');
            Font f = null;
            if (str.Length == 3)
            {
                switch (str[2].ToLower())
                {
                    case "bold":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                    case "regular":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                    case "underline":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                    case "italic":
                        return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                }
                f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
            }
            return f;
        }


        public byte[] RenderBytes(InfyPOS.Common.ImageData imageData, object dataSource, Object Items, SortedDictionary<string, object> Additional, bool addcutter)
        {
            //ms = new MemoryStream();
            //using (var ms = new MemoryStream())
            var bw = new HtmlBinaryWriter(imageData);

            bw.Write("<html><body>");

            Render(bw, dataSource, Items, Additional);

            bw.Write("</body></html>");
            bw.Flush();
            return System.Text.ASCIIEncoding.ASCII.GetBytes(bw.ToString());

        }
        private bool Render(HtmlBinaryWriter bw, object dataSource, Object Items, SortedDictionary<string, object> Additional)
        {
            HtmlFormat format = Clone();
            var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(Items, null);
            if (ie == null) return false;
            format.PageBody.AvailableRows = 0;
            var ien = ie.GetEnumerator();
            while (ien.MoveNext())
                format.PageBody.AvailableRows++;

            ien = ie.GetEnumerator();

            if (format.PageHeader != null)
                format.PageHeader.Render(dataSource, Additional, bw);
            while (true)
            {
                if (format.PageBody.Render(ien, Additional, bw))
                {
                    if (format.PageBody.EmptyLines > 0)
                        bw.FeedLines(format.PageBody.EmptyLines);
                    break;
                }
                bw.FeedLines(format.PageFooter.Length);
            }

            if (format.PageFooter != null)
                format.PageFooter.Render(dataSource, Additional, bw);
            return true;
        }

        public static byte[] Serialize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                ms.Seek(0, 0);
                bytes = ms.ToArray();
            }
            return bytes;
        }

        public static object Deserialize(byte[] bytes)
        {

            System.IO.MemoryStream _MemoryStream = new System.IO.MemoryStream(bytes);

            // create new BinaryFormatter
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter _BinaryFormatter
                        = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            // set memory stream position to starting point
            _MemoryStream.Position = 0;

            // Deserializes a stream into an object graph and return as a object.
            return _BinaryFormatter.Deserialize(_MemoryStream);
        }


        public HtmlFormat Clone()
        {
            var bytes = Serialize(this);
            var obj = Deserialize(bytes);
            return obj as HtmlFormat;
        }

        public DefaultPageSettings GetDefaultSetting()
        {
            ReportLibrary.DefaultPageSettings setting = new DefaultPageSettings();
            if (PageSetting == null)
                PageSetting = new PageSettings();
            setting.Landscape = PageSetting.IsLandscape;

            setting.Margins = new Margins(PageSetting.LeftMargin, PageSetting.RightMargin, PageSetting.TopMargin, PageSetting.BottomMargin);
            setting.PageBounds = new Rectangle(0, 0, PageSetting.Width, PageSetting.Height);
            setting.MarginBound = new Rectangle(PageSetting.LeftMargin, PageSetting.TopMargin,
                PageSetting.Width - (PageSetting.LeftMargin + PageSetting.RightMargin), PageSetting.Height - (PageSetting.BottomMargin + PageSetting.TopMargin));
            setting.PaperSize = new PaperSize("Custom", PageSetting.Width, PageSetting.Height);
            return setting;
        }

        public List<string> GetImages()
        {
            List<string> ret = new List<string>();
            if (PageHeader != null && PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Image)))
            {
                ret.AddRange(GetImages(PageHeader.Items));
            }

            if (PageFooter != null && PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Image)))
            {
                ret.AddRange(GetImages(PageFooter.Items));
            }

            if (PageBody != null && PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Image)))
            {
                ret.AddRange(GetImages(PageBody.Items));
            }

            if (Columnes != null)
            {
                foreach (var item in Columnes)
                {
                    if (item.PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Image)))
                    {
                        ret.AddRange(GetImages(item.PageHeader.Items));
                    }
                    if (item.PageFooter != null)
                    {
                        if (item.PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Image)))
                        {
                            ret.AddRange(GetImages(item.PageFooter.Items));
                        }
                    }
                    if (item.PageBody != null)
                    {
                        if (item.PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Image)))
                        {
                            ret.AddRange(GetImages(item.PageBody.Items));
                        }
                    }
                }
            }
            return ret;
        }

        public List<string> GetImages(List<Line> source)
        {
            List<string> ret = new List<string>();

            foreach (var item in source)
            {
                foreach (var content in item.Items.FindAll(e => e.Type == CotnentType.Image))
                {
                    ret.Add(content.Source);
                }
            }
            return ret;
        }
        public bool HasLogo()
        {

            if (PageHeader != null && PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Logo)))
                return true;
            else if (PageFooter != null && PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Logo)))
                return true;
            else if (PageBody != null && PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Logo)))
                return true;
            else if (Columnes != null)
            {
                foreach (var item in Columnes)
                {
                    if (item.PageHeader != null && item.PageHeader.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Logo)))
                        return true;
                    if (item.PageFooter != null && item.PageFooter.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Logo)))
                        return true;
                    if (item.PageBody != null && item.PageBody.Items.Exists(es => es.Items.Exists(ex => ex.Type == ReportLibrary.HtmlFormat.CotnentType.Logo)))
                        return true;
                }
            }

            return false;
        }
        public PageSettings PageSetting { get; set; }
        public Header PageHeader { get; set; }
        public Body PageBody { get; set; }
        public Footer PageFooter { get; set; }
        public bool Valid { get; set; }
        public string Exceptions { get; set; }
        public List<Column> Columnes { get; set; }
        public List<Column> Rows { get; set; }
        public class Column
        {
            public float width { get; set; }
            public float height { get; set; }
            public bool border { get; set; }
            public Header PageHeader { get; set; }
            public Body PageBody { get; set; }
            public Footer PageFooter { get; set; }
            public Header PageBodyEmptyLine { get; set; }
        }
        [Serializable]
        public class LineCollection
        {
            public List<Line> Items { get; set; }
            public virtual void Parse(System.Xml.XmlNode xmlNode)
            {
                Items = new List<Line>();
                foreach (System.Xml.XmlNode item in xmlNode.ChildNodes)
                {
                    if (item.Name.ToLower() == "line")
                        Items.Add(new Line(item));
                }
            }

            public string GetAttribValue(System.Xml.XmlNode xmlNode, string key)
            {
                foreach (System.Xml.XmlAttribute aitem in xmlNode.Attributes)
                {
                    if (aitem.Name.ToLower() == key.ToLower()) return aitem.Value;
                }
                return "";
            }
        }
        [Serializable]
        public class Header : LineCollection
        {
            public Header()
            {

            }
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                Style = GetAttribValue(xmlNode, "Style");
                float height = 0;
                if (float.TryParse(GetAttribValue(xmlNode, "LineHeight"), out height))
                    LineHeight = height;

                SpecialHeader = GetAttribValue(xmlNode, "SpecialHeader");

                base.Parse(xmlNode);
            }
            public string Style { get; set; }
            public float LineHeight { get; set; }
            public string SpecialHeader { get; set; }


            internal void Render(object PageValues, SortedDictionary<string, object> Additional, HtmlBinaryWriter bw)
            {
                foreach (var item in Items)
                {
                    //if (item.Render(PageValues, Additional, bw))
                    //    bw.FeedLines(1);

                    if (item.IsTable && !string.IsNullOrEmpty(item.field))
                    {

                        var items = ReportLibrary.DataSourceHelper.Eval(PageValues, item.field);

                        var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(items, null);
                        if (ie != null)
                        {
                            Table table = new Table();
                            List<Line> lstline = new List<Line>();
                            lstline.Add(item);
                            table.Items = lstline;

                            var ien = ie.GetEnumerator();
                            while (ien.MoveNext())
                                table.AvailableRows++;
                            ien = ie.GetEnumerator();
                            table.Length = table.AvailableRows;
                            table.Render(PageValues, ien, Additional, bw);
                        }
                    }
                    else
                    {
                        if (item.Render(PageValues, Additional, bw))
                            bw.FeedLines(1);
                    }

                }

            }
        }
        [Serializable]
        public class PageSettings : LineCollection
        {
            public string PrinterName;
            public int TopMargin = 10;
            public int LeftMargin = 10;
            public int BottomMargin = 10;
            public int RightMargin = 10;
            public int Width = 800;
            public int NoofColumns = 3;
            public int ColumnWidth = 100;
            public int ColumnHeight = 100;
            public int Height = 1000;
            public bool IsLandscape = false;
            public bool IsThermal = false;
            public string Style { get; set; }
            public string[] fixedbilltypes { get; set; }
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                foreach (System.Xml.XmlNode item in xmlNode.ChildNodes)
                {
                    if (item.Name.ToLower() == "setting")
                    {
                        int.TryParse(GetAttribValue(item, "topmargin"), out TopMargin);
                        int.TryParse(GetAttribValue(item, "leftmargin"), out LeftMargin);
                        int.TryParse(GetAttribValue(item, "bottommargin"), out BottomMargin);
                        int.TryParse(GetAttribValue(item, "rightmargin"), out RightMargin);
                        int.TryParse(GetAttribValue(item, "width"), out Width);
                        int.TryParse(GetAttribValue(item, "height"), out Height);
                        int.TryParse(GetAttribValue(item, "noofcolumns"), out NoofColumns);
                        int.TryParse(GetAttribValue(item, "columnwidth"), out ColumnWidth);
                        int.TryParse(GetAttribValue(item, "columnheight"), out ColumnHeight);

                        bool.TryParse(GetAttribValue(item, "landscape"), out IsLandscape);
                        bool.TryParse(GetAttribValue(item, "thermal"), out IsThermal);
                        Style = GetAttribValue(item, "style");

                        var ftypes = GetAttribValue(item, "fixedbilltypes");
                        if (!string.IsNullOrEmpty(ftypes))
                            fixedbilltypes = ftypes.Split(',');
                    }
                }
                base.Parse(xmlNode);
            }
        }
        [Serializable]
        public class Body : LineCollection
        {
            int length = 10;
            public bool DisableGrid { get; set; }
            public bool WrapRow { get; set; }
            public int Length
            {
                get { return length; }
                set { length = value; }
            }

            public int AvailableRows { get; set; }

            public bool IsTable { get; set; }
            int printedItems = 0;
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                IsTable = GetAttribValue(xmlNode, "Type") == "Table";
                Style = GetAttribValue(xmlNode, "Style");
                float height = 0;
                if (float.TryParse(GetAttribValue(xmlNode, "HeaderHeight"), out height))
                    HeaderHeight = height;
                if (float.TryParse(GetAttribValue(xmlNode, "LineHeight"), out height))
                    LineHeight = height;
                int.TryParse(GetAttribValue(xmlNode, "length"), out length);

                DisableGrid = GetAttribValue(xmlNode, "DisableGrid") == "True";
                WrapRow = GetAttribValue(xmlNode, "WrapRow") == "True";
                base.Parse(xmlNode);
            }

            public int EmptyLines
            {
                get
                {
                    return length == 0 ? 0 : length - (AvailableRows % length);
                }
            }

            public string Style { get; set; }
            public float HeaderHeight { get; set; }

            public float LineHeight { get; set; }

            internal bool Render(System.Collections.IEnumerator ien, SortedDictionary<string, object> Additional, HtmlBinaryWriter bw)
            {
                foreach (var line in Items)
                {
                    foreach (var item in line.Items)
                    {
                        bw.Write(item.GetAlignmentText(item.Text, item.Align));
                    }
                    bw.FeedLines(1);
                }

                while (ien.MoveNext())
                {
                    printedItems++;
                    if (Additional.ContainsKey("SNo"))
                        Additional["SNo"] = printedItems;
                    else
                        Additional.Add("SNo", printedItems);
                    foreach (var item in Items)
                    {
                        if (item.Render(ien.Current, Additional, bw))
                            bw.FeedLines(1);
                    }
                    if (length != 0 && printedItems % length == 0 && printedItems != AvailableRows)
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        [Serializable]
        public class Table : LineCollection
        {
            int length = 10;

            public int Length
            {
                get { return length; }
                set { length = value; }
            }

            public int AvailableRows { get; set; }
            public string HasValue { get; set; }
            public string HideValue { get; set; }
            public bool IsTable { get; set; }
            int printedItems = 0;
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                HasValue = GetAttribValue(xmlNode, "HasValue");
                HideValue = GetAttribValue(xmlNode, "HideValue");
                IsTable = GetAttribValue(xmlNode, "Type") == "Table";
                int.TryParse(GetAttribValue(xmlNode, "length"), out length);
                base.Parse(xmlNode);
            }

            public int EmptyLines
            {
                get
                {
                    return length - (AvailableRows % length);
                }
            }


            internal bool Render(object PageValues, System.Collections.IEnumerator ien, SortedDictionary<string, object> Additional, HtmlBinaryWriter bw)
            {
                if (!String.IsNullOrEmpty(HasValue) && ien == null)
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value.GetType().Name == "Boolean" && (bool)Value == false)
                    {
                        return false;
                    }
                }
                while (ien.MoveNext())
                {
                    printedItems++;
                    if (Additional.ContainsKey("SNo"))
                        Additional["SNo"] = printedItems;
                    else
                        Additional.Add("SNo", printedItems);
                    foreach (var item in Items)
                    {
                        if (item.Render(ien.Current, Additional, bw))
                            bw.FeedLines(1);
                    }
                    if (length != 0 && printedItems % length == 0 && printedItems != AvailableRows)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        [Serializable]
        public class Footer : LineCollection
        {
            int length = 0;

            public int Length
            {
                get { return length; }
                set { length = value; }
            }
            public override void Parse(System.Xml.XmlNode xmlNode)
            {
                Style = GetAttribValue(xmlNode, "Style");
                float height = 0;
                if (float.TryParse(GetAttribValue(xmlNode, "LineHeight"), out height))
                    LineHeight = height;

                int.TryParse(GetAttribValue(xmlNode, "length"), out length);

                EOD = GetAttribValue(xmlNode, "EOD").ToLower() == "true";
                base.Parse(xmlNode);
            }

            public bool EOD { get; set; }

            public string Style { get; set; }
            public float LineHeight { get; set; }

            internal void Render(object PageValues, SortedDictionary<string, object> Additional, HtmlBinaryWriter bw)
            {
                foreach (var item in Items)
                {
                    if (item.IsTable && !string.IsNullOrEmpty(item.field))
                    {

                        var items = ReportLibrary.DataSourceHelper.Eval(PageValues, item.field);

                        var ie = ReportLibrary.DataSourceHelper.GetResolvedDataSource(items, null);
                        if (ie != null)
                        {
                            Table table = new Table();
                            List<Line> lstline = new List<Line>();
                            lstline.Add(item);
                            table.Items = lstline;

                            var ien = ie.GetEnumerator();
                            while (ien.MoveNext())
                                table.AvailableRows++;
                            ien = ie.GetEnumerator();
                            table.Length = table.AvailableRows;
                            table.Render(PageValues, ien, Additional, bw);
                        }
                    }
                    else
                    {
                        if (item.Render(PageValues, Additional, bw))
                            bw.FeedLines(1);
                    }

                }
            }
        }
        [Serializable]
        public class Line
        {
            public Font GetStyle()
            {
                if (string.IsNullOrEmpty(Style)) return null;
                string[] str = Style.Split(',');
                Font f = null;
                if (str.Length == 3)
                {
                    switch (str[2].ToLower())
                    {
                        case "bold":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                        case "regular":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                        case "underline":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                        case "italic":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                    }
                    f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                }
                return f;
            }
            public string GetAttribValue(System.Xml.XmlNode xmlNode, string key)
            {
                foreach (System.Xml.XmlAttribute aitem in xmlNode.Attributes)
                {
                    if (aitem.Name.ToLower() == key.ToLower()) return aitem.Value;
                }
                return "";
            }
            public string field { get; set; }
            public string HasValue { get; set; }
            public string HideValue { get; set; }
            public float Height { get; set; }
            public float Width { get; set; }
            public float Top { get; set; }
            public float Bottom { get; set; }

            public string Style { get; set; }
            int length = 0;
            public List<Content> Items { get; set; }
            public Line(System.Xml.XmlNode xmlNode)
            {
                Style = GetAttribValue(xmlNode, "Style");
                Hide = GetAttribValue(xmlNode, "Hide");
                Show = GetAttribValue(xmlNode, "Show");
                float height;
                if (float.TryParse(GetAttribValue(xmlNode, "Height"), out height))
                    Height = height;

                float width;
                if (float.TryParse(GetAttribValue(xmlNode, "Width"), out width))
                    Width = width;

                float bottom;
                if (float.TryParse(GetAttribValue(xmlNode, "Bottom"), out bottom))
                    Bottom = bottom;

                float top;
                if (float.TryParse(GetAttribValue(xmlNode, "Top"), out top))
                    Top = top;

                IsTable = GetAttribValue(xmlNode, "Type") == "Table";
                IsVerticalTable = GetAttribValue(xmlNode, "Type") == "VerticalTable";
                IsVerticalLine = GetAttribValue(xmlNode, "Type") == "VerticalLine";
                if (IsTable)
                {
                    field = GetAttribValue(xmlNode, "field");
                }
                Thermal = GetAttribValue(xmlNode, "Thermal") == "True";
                HideHeader = GetAttribValue(xmlNode, "HideHeader") == "True";
                IsImage = GetAttribValue(xmlNode, "Type") == "Image";
                int.TryParse(GetAttribValue(xmlNode, "length"), out length);
                HasValue = GetAttribValue(xmlNode, "HasValue");
                HideValue = GetAttribValue(xmlNode, "HideValue");
                Items = new List<Content>();
                foreach (System.Xml.XmlNode item in xmlNode.ChildNodes)
                {
                    Items.Add(new Content(item));
                }
            }
            public string Hide { get; set; }
            public string Show { get; set; }
            public bool IsTable { get; set; }
            public bool IsVerticalTable { get; set; }
            public bool IsVerticalLine { get; set; }
            public bool Thermal { get; set; }
            public bool HideHeader { get; set; }
            public bool IsImage { get; set; }

            internal bool Render(Object PageValues, SortedDictionary<string, object> Additional, HtmlBinaryWriter bw)
            {
                if (!String.IsNullOrEmpty(HasValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(Show))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, Show);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return false;
                    }
                }


                if (!String.IsNullOrEmpty(HasValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return false;
                    }
                }

                if (!String.IsNullOrEmpty(HideValue))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(PageValues, HideValue);
                    if (Value == null && Additional.ContainsKey(HideValue))
                        Value = Additional[HideValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == true))
                    {
                        return false;
                    }
                }



                foreach (var item in Items)
                {
                    string str = item.Render(PageValues, Additional, true);

                    if (item.Type == CotnentType.Barcode)
                        bw.Barcode(str);
                    else if (item.Type == CotnentType.PlainBarcode)
                        bw.Barcode(str);
                    else if (item.Type == CotnentType.Logo)
                    {
                        if (Additional.ContainsKey("LOGO"))
                            bw.WriteLogo((byte[])Additional["LOGO"], int.Parse(item.Text));
                    }
                    else if (item.Type == CotnentType.Image)
                    {

                        if (string.IsNullOrEmpty(item.Source) && !string.IsNullOrEmpty(item.Name))
                        {
                            if (str != null)
                            {
                                item.Source = str.ToString();
                                var imgdata = bw.ImageData.GetData(item.Source.LongFromBase62());
                                var img = bw.ImageData.FromBytes(imgdata.data);
                                Additional.Add("IMG-" + item.Source, img);
                            }
                        }

                        if (Additional.ContainsKey("IMG-" + item.Source))
                        {
                            int scale;
                            if (!int.TryParse(item.Text, out scale))
                                scale = 1;
                            bw.WriteLogo((byte[])Additional["IMG-" + item.Source], scale);
                        }
                    }
                    else
                        bw.Write(str, item);
                }
                if (length > 1)
                    bw.FeedLines(length - 1);
                return true;
            }
        }
        public enum CotnentType
        {
            Word,
            SpecialCharactor,
            Field,
            Reverse,
            Duplicate,
            Line,
            Paragraph,
            Logo,
            Table,
            Image,
            Barcode,
            QRBarcode,
            Cutter,
            Dynamicfield,
            PlainBarcode,
            Dynamictable,
            LineBreak
        }
        [Serializable]
        public class Content
        {
            SortedDictionary<string, string> keyvalue;
            private string GetAttributeValue(string key)
            {
                if (!keyvalue.ContainsKey(key.ToLower()))
                    return "";
                return keyvalue[key.ToLower()];
            }
            public Content()
            {

            }

            internal float GetHeight(float defaultHeight)
            {
                if (Height > defaultHeight) return Height;
                return defaultHeight;
            }

            public Content(System.Xml.XmlNode item)
            {
                keyvalue = new SortedDictionary<string, string>();
                if (item.Attributes != null)
                {
                    foreach (System.Xml.XmlAttribute aitem in item.Attributes)
                    {
                        if (!keyvalue.ContainsKey(aitem.Name.ToLower()))
                            keyvalue.Add(aitem.Name.ToLower(), aitem.Value);
                    }
                }
                switch (GetAttributeValue("Type"))
                {
                    case "Duplicate":
                        Type = CotnentType.Duplicate;
                        break;
                    case "Field":
                        Type = CotnentType.Field;
                        break;
                    case "Logo":
                        Type = CotnentType.Logo;
                        break;
                    case "Barcode":
                        Type = CotnentType.Barcode;
                        break;
                    case "PlainBarcode":
                        Type = CotnentType.PlainBarcode;
                        break;
                    case "QRBarcode":
                        Type = CotnentType.QRBarcode;
                        break;
                    case "Cutter":
                        Type = CotnentType.Cutter;
                        break;
                    case "Image":
                        Type = CotnentType.Image;
                        break;
                    case "Table":
                        Type = CotnentType.Table;
                        break;
                    case "Reverse":
                        Type = CotnentType.Reverse;
                        break;
                    case "LineBreak":
                        Type = CotnentType.LineBreak;
                        break;
                    case "SpecialCharactor":
                        Type = CotnentType.SpecialCharactor;
                        break;
                    case "Line":
                        Type = CotnentType.Line;
                        break;
                    case "Paragraph":
                        Type = CotnentType.Paragraph;
                        break;
                    case "Word":
                        Type = CotnentType.Word;
                        break;
                    case "Dynamicfield":
                        Type = CotnentType.Dynamicfield;
                        break;
                    case "Dynamictable":
                        Type = CotnentType.Dynamictable;
                        break;
                }
                int length;
                if (int.TryParse(GetAttributeValue("Length"), out length))
                    Length = length;
                if (GetAttributeValue("Text") != "")
                    Text = GetAttributeValue("Text");
                else
                    Text = item.InnerText;
                Source = GetAttributeValue("src");
                EOL = GetAttributeValue("EOL") == "true";
                Format = GetAttributeValue("Format");
                Align = GetAttributeValue("Align");
                Name = GetAttributeValue("Name");
                Style = GetAttributeValue("Style");
                Multiline = GetAttributeValue("Multiline");
                HasValue = GetAttributeValue("HasValue");
                Prefix = GetAttributeValue("Prefix");
                Suffix = GetAttributeValue("Suffix");
                BottomColumn = GetAttributeValue("BottomColumn") == "true";
                HideValue = GetAttributeValue("HideValue");
                Hide = GetAttributeValue("Hide");
                Show = GetAttributeValue("Show");
                Total = GetAttributeValue("Total") == "true";
                TrueType = GetAttributeValue("TrueType") == "true";
                MergeText = GetAttributeValue("MergeText");
                if (!string.IsNullOrEmpty(MergeText) && int.TryParse(GetAttributeValue("MergeCell"), out length))
                    MergeCell = length;

                if (!string.IsNullOrEmpty(GetAttributeValue("FontSize")) && int.TryParse(GetAttributeValue("FontSize"), out length))
                    FontSize = length;

                if (!string.IsNullOrEmpty(GetAttributeValue("Height")) && int.TryParse(GetAttributeValue("Height"), out length))
                    Height = length;

                FontName = GetAttributeValue("FontName");

                DataType = DOSReporter.DataType.String;
                switch (GetAttributeValue("DataType").ToLower())
                {
                    case "number":
                        DataType = DOSReporter.DataType.Number;
                        break;
                    case "date":
                        DataType = DOSReporter.DataType.Date;
                        break;
                    case "image":
                        DataType = DOSReporter.DataType.Image;
                        break;
                }

                if (Type == CotnentType.Table)
                {
                    Items = new List<Content>();
                    if (item.ChildNodes != null)
                    {
                        foreach (System.Xml.XmlNode citem in item.ChildNodes)
                        {
                            Items.Add(new Content(citem));
                        }
                    }
                }
            }
            public Font GetStyle()
            {
                if (string.IsNullOrEmpty(Style)) return null;
                string[] str = Style.Split(',');
                Font f = null;
                if (str.Length == 3)
                {
                    switch (str[2].ToLower())
                    {
                        case "bold":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                        case "regular":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                        case "underline":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                        case "italic":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                    }
                    f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                }
                return f;
            }

            public CotnentType Type { get; set; }
            public int Length { get; set; }
            public string Text { get; set; }
            public string Format { get; set; }
            public string Source { get; set; }
            public bool TrueType { get; set; }
            public bool EOL { get; set; }
            public string Align { get; set; }
            public string Prefix { get; set; }
            public string Suffix { get; set; }
            public bool BottomColumn { get; set; }
            public string HasValue { get; set; }
            public string HideValue { get; set; }
            public object Value { get; set; }
            public string Style { get; set; }
            public string Name { get; set; }
            public string Multiline { get; set; }
            public bool Total { get; set; }
            public int MergeCell { get; set; }
            public string Show { get; set; }
            public string Hide { get; set; }
            public string MergeText { get; set; }
            public string FontName { get; private set; }
            public DOSReporter.DataType DataType { get; set; }
            public int Height { get; set; }
            public int FontSize { get; set; }
            public List<Content> Items { get; set; }
            public StringAlignment GetAlignment()
            {
                switch (Align)
                {
                    case "right":
                        return StringAlignment.Far;
                    case "center":
                        return StringAlignment.Center;
                    default:
                        return StringAlignment.Near;
                }
            }
            public Font GetStyle(Font source)
            {
                string[] str = Style.Split(',');
                Font f = source;
                if (str.Length == 3)
                {
                    switch (str[2].ToLower())
                    {
                        case "bold":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                        case "regular":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                        case "underline":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Underline);
                        case "italic":
                            return new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Italic);
                    }
                    f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Bold);
                }
                else if (str.Length == 2)
                {
                    f = new Font(str[0], Convert.ToSingle(str[1]), FontStyle.Regular);
                }
                return f;
            }
            private string spanTag = @"<div style=""display:inline-block;{0}""><div style=""{1}"">{2}</div></div>";
            public string GetAlignmentText(string input, string style)
            {
                //if (!string.IsNullOrEmpty(input))
                //    input = input.Replace(" ", "&nbsp;");
                if (Length > 0)
                {
                    switch (Align.ToLower())
                    {
                        case "right":
                            return string.Format(spanTag, "width:" + Length + "px", "align:right", input);
                        case "center":
                            return string.Format(spanTag, "width:" + Length + "px", "align:center", input);
                        default:
                            return string.Format(spanTag, "width:" + Length + "px", "", input);
                    }
                }
                return input;
            }

            private bool IsNumericType(Type type)
            {
                TypeCode typeCode = System.Type.GetTypeCode(type);
                //The TypeCode of numerical types are between SByte (5) and Decimal (15).
                return (int)typeCode >= 5 && (int)typeCode <= 15;
            }

            public string Render(Object values, SortedDictionary<string, object> Additional)
            {
                return Render(values, Additional, false);
            }
            public string Render(Object values, SortedDictionary<string, object> Additional, bool isThermal)
            {
                if (!string.IsNullOrEmpty(HasValue))
                {
                    Value = ReportLibrary.DataSourceHelper.Eval(values, HasValue);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (IsNumericType(Value.GetType()))
                    {
                        if (Convert.ToDecimal(Value) == 0)
                            return "";
                    }
                    else if (Value.GetType().Name == "Boolean" && (bool)Value == false)
                    {
                        return "";
                    }
                }

                if (!String.IsNullOrEmpty(Show))
                {
                    object Value = ReportLibrary.DataSourceHelper.Eval(values, Show);
                    if (Value == null && Additional.ContainsKey(HasValue))
                        Value = Additional[HasValue];

                    if (Value == null || (Value.GetType().Name == "Boolean" && (bool)Value == false))
                    {
                        return "";
                    }
                }

                if (!string.IsNullOrEmpty(HideValue))
                {
                    Value = ReportLibrary.DataSourceHelper.Eval(values, HideValue);
                    if (Value == null && Additional.ContainsKey(HideValue))
                        Value = Additional[HideValue];

                    if (IsNumericType(Value.GetType()))
                    {
                        if (Convert.ToDecimal(Value) == 0)
                            return "";
                    }
                    else if (Value.GetType().Name == "Boolean" && (bool)Value == true)
                    {
                        return "";
                    }
                }


                string stringText = "";
                switch (Type)
                {
                    case CotnentType.Duplicate:
                        stringText = PrintUtils.Replicate(Text, Length);
                        break;
                    case CotnentType.Field:
                        Value = ReportLibrary.DataSourceHelper.Eval(values, Name);
                        if (Value == null && Additional.ContainsKey(HasValue))
                            Value = Additional[HasValue];
                        if (Value != null)
                        {
                            if (!string.IsNullOrEmpty(Format))
                                stringText = string.Format("{0:" + Format + "}", Value);
                            else
                                stringText = Value.ToString();
                        }
                        break;
                    case CotnentType.Reverse:
                        stringText = PrintUtils.Reverse;
                        break;
                    case CotnentType.SpecialCharactor:
                        stringText = "";
                        switch (GetAttributeValue("type").ToLower())
                        {
                            case "":
                            case "string":
                                stringText = Text;
                                break;
                            case "bytes":
                            case "byte":
                                foreach (var item in Text.Split(','))
                                {
                                    stringText += (char)int.Parse(item);
                                }
                                break;
                        }

                        break;
                    case CotnentType.Word:
                        stringText = Text;
                        break;
                    case CotnentType.Logo:
                        stringText = Text;
                        break;
                    case CotnentType.Barcode:
                    case CotnentType.PlainBarcode:
                        Value = ReportLibrary.DataSourceHelper.Eval(values, Name);
                        if (Value == null && Additional.ContainsKey(HasValue))
                            Value = Additional[HasValue];
                        if (Value != null)
                        {
                            stringText = Value.ToString();
                        }
                        break;
                }


                stringText = GetAlignmentText(stringText, Style);
                return stringText;
            }
        }

        public Header PageBodyEmptyLine { get; set; }
        public bool isValid { get; set; }
    }
}
