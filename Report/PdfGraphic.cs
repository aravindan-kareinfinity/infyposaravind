using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace PDFGraphics
{
    public class PDFGraphic : PDFGraphics.IGraphic
    {
        System.Drawing.Graphics InternalGP;
        PdfSharp.Pdf.PdfPage page;
        PdfSharp.Drawing.XGraphics gfx;
        PdfSharp.Drawing.Layout.XTextFormatter tf;
        public PDFGraphic(PdfSharp.Pdf.PdfPage page, System.Drawing.Graphics graphic)
        {
            this.page = page;
            this.InternalGP = graphic;
            gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            tf = new PdfSharp.Drawing.Layout.XTextFormatter(gfx);
        }
        public PDFGraphic(PdfSharp.Pdf.PdfPage page, System.Drawing.Graphics graphic, XGraphicsUnit unit)
        {
            this.page = page;
            this.InternalGP = graphic;
            switch(unit)
            {
                case XGraphicsUnit.Millimeter:
                    graphic.PageUnit = GraphicsUnit.Millimeter;
                    break;
                case XGraphicsUnit.Inch:
                    graphic.PageUnit = GraphicsUnit.Inch;
                    break;
                case XGraphicsUnit.Point:
                    graphic.PageUnit = GraphicsUnit.Point;
                    break;
                case XGraphicsUnit.Presentation:
                    graphic.PageUnit = GraphicsUnit.Pixel;
                    break;
            }
            
            gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page, unit);
            tf = new PdfSharp.Drawing.Layout.XTextFormatter(gfx);
        }
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            gfx.DrawLine( pen, x1, y1, x2, y2);
        }
        public void DrawCircle(System.Drawing.Pen pen, System.Drawing.Rectangle rect)
        {
            this.gfx.DrawEllipse(pen, rect);
        }
        public void DrawDottedLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            XPen xpen = new XPen(pen);
            xpen.DashStyle = XDashStyle.Dash;
            xpen.DashOffset = 8;
            gfx.DrawLine(xpen, x1, y1, x2, y2);
        }
        public void DrawRectangle(Pen pen, Rectangle rect)
        {
            gfx.DrawRectangle(pen, rect);
        }
        public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        {
            gfx.DrawRectangle(pen, x, y, width, height);
        }
        public void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            gfx.DrawRectangle(pen, x, y, width, height);
        }
        public void DrawRectangles(Pen pen, Rectangle[] rects)
        {
            gfx.DrawRectangles(pen, rects);
        }
        public void DrawRectangles(Pen pen, RectangleF[] rects)
        {
            gfx.DrawRectangles(pen, rects);
        }
        public void DrawString(string s, Font font, Brush brush, PointF point)
        {
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);

            //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            //XPdfFontOptions options = new XPdfFontOptions(PdfSharp.Pdf.PdfFontEncoding.WinAnsi, PdfFontEmbedding.Default);
            //XFont fontRegular = new XFont(font.Name, 20, XFontStyle.Regular, options);
            gfx.DrawString(s, font, brush, point);
        }
        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle)
        {
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);
            layoutRectangle.Offset(3, 2);
            gfx.DrawString(s, font, brush, layoutRectangle);
        }
        public void DrawString(string s, Font font, Brush brush, float x, float y)
        {
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);
            gfx.DrawString(s, font, brush, x, y);
        }

        public void DrawString(string s, Font font, Brush brush, float x, float y,float angle)
        {
            var state = gfx.Save();
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);
            gfx.RotateTransform(angle);
            gfx.DrawString(s, font, brush, x, y);
            gfx.Restore(state);
        }


        public XStringFormat Convert(StringFormat format)
        {
            XStringFormat xformat = new XStringFormat();
            xformat.Alignment = (XStringAlignment)format.Alignment;
            xformat.LineAlignment = (XLineAlignment)format.LineAlignment;
            return xformat;
        }
        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
        {
            if (string.IsNullOrEmpty(s)) return;
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);
            gfx.DrawString(s, font, brush, point, Convert(format));
        }
        public void DrawString(string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
        {
            if (string.IsNullOrEmpty(s)) return;
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
                if(size.Width>layoutRectangle.Width)
                {
                    var rowheight = size.Height;
                    int charactersOnPage = 0;
                    int linesPerPage = 0;
                    size = gfx.Graphics.MeasureString(s, font,new SizeF(layoutRectangle.Width,layoutRectangle.Height),
                        format, out charactersOnPage, out linesPerPage);
                    
                    if (size.Height < layoutRectangle.Height && !(s.Substring(0,charactersOnPage).IndexOf(' ')<0 &&
                        charactersOnPage == s.Length && linesPerPage>1))
                    {
                        var fmt = Convert(format);
                        //var availablestring = s;
                        //var nooflines = linesPerPage;
                        //while (nooflines > 0)
                        //{
                        //    size = gfx.Graphics.MeasureString(availablestring, font, 
                        //        new SizeF(layoutRectangle.Width, rowheight),format, 
                        //        out charactersOnPage, out linesPerPage);
                        //    s = availablestring.Substring(0, charactersOnPage);
                        //    gfx.DrawString(s, font, brush, layoutRectangle, Convert(format));
                        //    availablestring = availablestring.Substring(s.Length);
                        //    layoutRectangle.Offset(0, rowheight);
                        //    nooflines--;
                        //}

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
                if(csize.Width<=layoutRectangle.Width && format.Alignment == StringAlignment.Near && format.LineAlignment == StringAlignment.Near)
                    tf.DrawString(s, font, brush, layoutRectangle, Convert(format));
                else
                    gfx.DrawString(s, font, brush, layoutRectangle, Convert(format));
            }
            
        }

        XGraphicsState currentstate = null;
        public void RotateTransform(float angle)
        {
            currentstate = gfx.Save();
            gfx.RotateTransform(angle);
        }
        public void RotateTransform(float angle,Point point)
        {
            currentstate = gfx.Save();
            gfx.RotateAtTransform(angle,new XPoint(point));
        }
        public void ResetTransform()
        {
            if (currentstate != null)
                gfx.Restore(currentstate);
            currentstate = null;
        }
        public void DrawString(string s, Font font, Brush brush, float x, float y, StringFormat format)
        {
            if (string.IsNullOrEmpty(s)) return;
            font = new Font(font.Name, font.Size, font.Style, GraphicsUnit.World);
            gfx.DrawString(s, font, brush, x, y, Convert(format));
        }
        public void FillRectangle(Brush brush, Rectangle rect)
        {
            gfx.DrawRectangle(brush, rect);

        }
        public void FillRectangle(Brush brush, RectangleF rect)
        {
            gfx.DrawRectangle(brush, rect);
        }
        public void FillRectangle(Brush brush, float x, float y, float width, float height)
        {
            gfx.DrawRectangle(brush, x, y, width, height);
        }
        public void FillRectangle(Brush brush, int x, int y, int width, int height)
        {
            gfx.DrawRectangle(brush, x, y, width, height);
        }
        public void FillRectangles(Brush brush, Rectangle[] rects)
        {
            foreach (Rectangle rect in rects)
                gfx.DrawRectangle(brush, rect);
        }
        public void FillRectangles(Brush brush, RectangleF[] rects)
        {
            foreach (RectangleF rect in rects)
                gfx.DrawRectangle(brush, rect);
        }
        public SizeF MeasureString(string text, Font font)
        {
            return this.InternalGP.MeasureString(text, font);
        }
        public SizeF MeasureString(string text, Font font, int width)
        {
            return this.InternalGP.MeasureString(text, font, width);
        }
        public SizeF MeasureString(string text, Font font, SizeF layoutArea)
        {
            return this.InternalGP.MeasureString(text, font, layoutArea);
        }
        public SizeF MeasureString(string text, Font font, int width, StringFormat format)
        {
            return this.InternalGP.MeasureString(text, font, width, format);
        }
        public SizeF MeasureString(string text, Font font, PointF origin, StringFormat stringFormat)
        {
            return this.InternalGP.MeasureString(text, font, origin, stringFormat);
        }
        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
        {
            return this.InternalGP.MeasureString(text, font, layoutArea, stringFormat);
        }
        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled)
        {
            return this.InternalGP.MeasureString(text, font, layoutArea, stringFormat, out charactersFitted, out linesFilled);
        }
        public void DrawImage(Image img, RectangleF rect)
        {
            if(img != null)
            this.gfx.DrawImage(img, rect);
            //Bitmap bmPhoto = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format24bppRgb);
            //bmPhoto.SetResolution(300, 300);
            //Graphics grPhoto = Graphics.FromImage(bmPhoto);

            //grPhoto.DrawImage(img,
            //        new Rectangle(0, 0, (int)rect.Width, (int)rect.Height),
            //        new Rectangle(0, 0, img.Width, img.Height),
            //        GraphicsUnit.Pixel);
            //grPhoto.Dispose();
            //this.gfx.DrawImage(bmPhoto, rect);
        }
        public void DrawImage(Image img, Point point)
        {
            if (img != null)
                this.gfx.DrawImage(img, point);
        }
        public float DpiX
        {
            get
            {
                return this.InternalGP.DpiX;
            }
        }
        public float DpiY
        {
            get
            {
                return this.InternalGP.DpiY;
            }
        }
        public void DrawImageUnscaled(Image img, Rectangle rect)
        {
            if(img != null)
            this.gfx.DrawImage(img, rect);
        }
        public void Save()
        {
            gfx.Save();
        }
    }
}