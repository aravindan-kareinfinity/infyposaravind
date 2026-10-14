namespace LabelDesigner
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using static ReportLibrary.DosFormat;

    public partial class ReportDesigner
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }
        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("objects")]
        public Control[] Objects { get; set; }

        public ReportLibrary.DefaultPageSettings GetDefaultSetting(PageSettings PageSetting)
        {
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            setting.Margins = new System.Drawing.Printing.Margins(PageSetting.LeftMargin, PageSetting.RightMargin, PageSetting.TopMargin, PageSetting.BottomMargin);
            setting.PageBounds = new System.Drawing.Rectangle(0, 0, PageSetting.Width, PageSetting.Height);
            setting.MarginBound = new System.Drawing.Rectangle(PageSetting.LeftMargin, PageSetting.TopMargin,
                PageSetting.Width - (PageSetting.LeftMargin + PageSetting.RightMargin), PageSetting.Height - (PageSetting.BottomMargin + PageSetting.TopMargin));
            setting.PaperSize = new System.Drawing.Printing.PaperSize("Custom", PageSetting.Width, PageSetting.Height);
            return setting;
        }
        public List<ReportLibrary.LabelElement> Elements()
        {
            List<ReportLibrary.LabelElement> rst = new List<ReportLibrary.LabelElement>();
            foreach (var ctrl in Objects)
            {
                rst.Add(ctrl.Convert());
            }
            return rst;
        }
    }

    public partial class Control
    {
        public ReportLibrary.LabelElement Convert()
        {
            ReportLibrary.LabelElement labelElement = new ReportLibrary.LabelElement();
            labelElement.left = (int)this.Left;
            labelElement.top = (int)this.Top;
            labelElement.width = (int)this.Width;
            labelElement.height = (int)this.Height;
            labelElement.fontname = this.FontFamily;
            labelElement.text = this.Text;
            labelElement.angle = this.Angle;
            labelElement.scalex = this.ScaleX;
            labelElement.scaley = this.ScaleY;
            labelElement.fontsize = this.FontSize.HasValue ? this.FontSize.Value : 10;
            labelElement.fontstyle = this.FontWeight == "bold" ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular;
            labelElement.fontstyle = labelElement.fontstyle | (this.FontStyle == "Italic" ? System.Drawing.FontStyle.Italic : System.Drawing.FontStyle.Regular);
            if (this.Underline.HasValue && this.Underline.Value)
                labelElement.fontstyle = labelElement.fontstyle | System.Drawing.FontStyle.Underline;

            //labelElement.fontsize = this.FontSize.Value;
            switch (Controltype)
            {
                case "Barcode":
                    labelElement.Type = ReportLibrary.LabelElement.type.Barcode_128;
                    labelElement.text = this.field;
                    break;
                case "QR":
                    labelElement.Type = ReportLibrary.LabelElement.type.Barcode_QR;
                    labelElement.text = this.field;
                    break;
                case "Text":
                    labelElement.Type = ReportLibrary.LabelElement.type.Text;
                    break;
                case "Field":
                    labelElement.Type = ReportLibrary.LabelElement.type.Field;
                    break;
                case "Rectangle":
                    labelElement.Type = ReportLibrary.LabelElement.type.Rectangle;
                    break;
                case "Circle":
                    labelElement.Type = ReportLibrary.LabelElement.type.Circle;
                    break;
                case "Line":
                    labelElement.Type = ReportLibrary.LabelElement.type.Line;
                    break;
                case "Image":
                    labelElement.Type = ReportLibrary.LabelElement.type.Image;
                    break;
            }
            return labelElement;
        }
        
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("originX")]
        public string OriginX { get; set; }

        [JsonProperty("originY")]
        public string OriginY { get; set; }

        [JsonProperty("left")]
        public long Left { get; set; }

        [JsonProperty("top")]
        public long Top { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("fill")]
        public string Fill { get; set; }

        [JsonProperty("stroke")]
        public string Stroke { get; set; }

        [JsonProperty("strokeWidth")]
        public long StrokeWidth { get; set; }

        [JsonProperty("strokeDashArray")]
        public object StrokeDashArray { get; set; }

        [JsonProperty("strokeLineCap")]
        public string StrokeLineCap { get; set; }

        [JsonProperty("strokeLineJoin")]
        public string StrokeLineJoin { get; set; }

        [JsonProperty("strokeMiterLimit")]
        public long StrokeMiterLimit { get; set; }

        [JsonProperty("scaleX")]
        public double ScaleX { get; set; }

        [JsonProperty("scaleY")]
        public double ScaleY { get; set; }

        [JsonProperty("field")]
        public string field { get; set; }

        [JsonProperty("angle")]
        public long Angle { get; set; }

        [JsonProperty("flipX")]
        public bool FlipX { get; set; }

        [JsonProperty("flipY")]
        public bool FlipY { get; set; }

        [JsonProperty("opacity")]
        public long Opacity { get; set; }

        [JsonProperty("shadow")]
        public object Shadow { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }

        [JsonProperty("clipTo")]
        public object ClipTo { get; set; }

        [JsonProperty("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonProperty("fillRule")]
        public string FillRule { get; set; }

        [JsonProperty("paintFirst")]
        public string PaintFirst { get; set; }

        [JsonProperty("globalCompositeOperation")]
        public string GlobalCompositeOperation { get; set; }

        [JsonProperty("transformMatrix")]
        public object TransformMatrix { get; set; }

        [JsonProperty("skewX")]
        public long SkewX { get; set; }

        [JsonProperty("skewY")]
        public long SkewY { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("fontSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? FontSize { get; set; }

        [JsonProperty("fontWeight", NullValueHandling = NullValueHandling.Ignore)]
        public string FontWeight { get; set; }

        [JsonProperty("fontFamily", NullValueHandling = NullValueHandling.Ignore)]
        public string FontFamily { get; set; }

        [JsonProperty("fontStyle", NullValueHandling = NullValueHandling.Ignore)]
        public string FontStyle { get; set; }

        [JsonProperty("lineHeight", NullValueHandling = NullValueHandling.Ignore)]
        public double? LineHeight { get; set; }

        [JsonProperty("underline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Underline { get; set; }

        [JsonProperty("overline", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Overline { get; set; }

        [JsonProperty("linethrough", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Linethrough { get; set; }

        [JsonProperty("textAlign", NullValueHandling = NullValueHandling.Ignore)]
        public string TextAlign { get; set; }

        [JsonProperty("textBackgroundColor", NullValueHandling = NullValueHandling.Ignore)]
        public string TextBackgroundColor { get; set; }

        [JsonProperty("charSpacing", NullValueHandling = NullValueHandling.Ignore)]
        public long? CharSpacing { get; set; }

        [JsonProperty("minWidth", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinWidth { get; set; }

        [JsonProperty("styles", NullValueHandling = NullValueHandling.Ignore)]
        public Styles Styles { get; set; }

        [JsonProperty("controltype", NullValueHandling = NullValueHandling.Ignore)]
        public string Controltype { get; set; }

        [JsonProperty("controlid", NullValueHandling = NullValueHandling.Ignore)]
        public long? Controlid { get; set; }

        [JsonProperty("crossOrigin", NullValueHandling = NullValueHandling.Ignore)]
        public string CrossOrigin { get; set; }

        [JsonProperty("cropX", NullValueHandling = NullValueHandling.Ignore)]
        public long? CropX { get; set; }

        [JsonProperty("cropY", NullValueHandling = NullValueHandling.Ignore)]
        public long? CropY { get; set; }

        [JsonProperty("src", NullValueHandling = NullValueHandling.Ignore)]
        public string Src { get; set; }

        [JsonProperty("filters", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Filters { get; set; }

        [JsonProperty("rx", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rx { get; set; }

        [JsonProperty("ry", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ry { get; set; }

        [JsonProperty("radius", NullValueHandling = NullValueHandling.Ignore)]
        public long? Radius { get; set; }

        [JsonProperty("startAngle", NullValueHandling = NullValueHandling.Ignore)]
        public long? StartAngle { get; set; }

        [JsonProperty("endAngle", NullValueHandling = NullValueHandling.Ignore)]
        public double? EndAngle { get; set; }
    }

    public partial class Styles
    {
    }

    public partial class ReportDesigner
    {
        public static ReportDesigner FromJson(string json) => JsonConvert.DeserializeObject<ReportDesigner>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ReportDesigner self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}