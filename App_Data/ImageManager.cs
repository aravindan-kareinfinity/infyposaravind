using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace WebAPI.Common
{
    public class ImageManager
    {
        public static byte[] ToByteArray(Image imageIn)
        {
            return ToByteArray(imageIn, 70);
        }
        public static byte[] ToByteArray(Image imageIn,long quality)
        {
            //using (var ms = new MemoryStream())
            //{
            //    imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //    return ms.ToArray();
            //}

            using (System.Drawing.Imaging.EncoderParameters encoderParameters = new System.Drawing.Imaging.EncoderParameters(1))
            using (System.Drawing.Imaging.EncoderParameter encoderParameter = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality))
            {
                System.Drawing.Imaging.ImageCodecInfo codecInfo = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders().First(codec => codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                encoderParameters.Param[0] = encoderParameter;
                using (var msout = new System.IO.MemoryStream())
                {
                    imageIn.Save(msout, codecInfo, encoderParameters);
                    return msout.ToArray();
                }
            }

        }

        public static byte[] FixedSize(byte[] imgPhoto, int Width, int Height)
        {
            using (var ms = new MemoryStream(imgPhoto))
            {
                var image = Image.FromStream(ms);
                var newImage = FixedSize(image, Width, Height);

                using (System.Drawing.Imaging.EncoderParameters encoderParameters = new System.Drawing.Imaging.EncoderParameters(1))
                using (System.Drawing.Imaging.EncoderParameter encoderParameter = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L))
                {
                    System.Drawing.Imaging.ImageCodecInfo codecInfo = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders().First(codec => codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                    encoderParameters.Param[0] = encoderParameter;
                    using (var msout = new System.IO.MemoryStream())
                    {
                        newImage.Save(msout, codecInfo, encoderParameters);
                        image.Dispose();
                        newImage.Dispose();
                        return ms.ToArray();
                    }
                }
            }
        }



        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int X = 0;
            int Y = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
            }
            else
            {
                nPercent = nPercentW;
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            grPhoto.DrawImage(imgPhoto,
                    new Rectangle(X, Y, destWidth, destHeight),
                    new Rectangle(X, Y, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static Image ScaleBySize(Image imgPhoto, int size)
        {
            int logoSize = size;

            float sourceWidth = imgPhoto.Width;
            float sourceHeight = imgPhoto.Height;
            float destHeight = 0;
            float destWidth = 0;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            // Resize Image to have the height = logoSize/2 or width = logoSize.
            // Height is greater than width, set Height = logoSize and resize width accordingly
            if (sourceWidth > (2 * sourceHeight))
            {
                destWidth = logoSize;
                destHeight = (float)(sourceHeight * logoSize / sourceWidth);
            }
            else
            {
                int h = logoSize / 2;
                destHeight = h;
                destWidth = (float)(sourceWidth * h / sourceHeight);
            }
            // Width is greater than height, set Width = logoSize and resize height accordingly

            Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight,
                                        PixelFormat.Format32bppPArgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();

            return bmPhoto;
        }
    }

    public class BinaryMediaTypeFormatter : MediaTypeFormatter
    {

        private static Type _supportedType = typeof(byte[]);
        private const int BufferSize = 8192; // 8K 

        public BinaryMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }

        public override bool CanReadType(Type type)
        {
            return type == _supportedType;
        }

        public override bool CanWriteType(Type type)
        {
            return type == _supportedType;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var taskSource = new TaskCompletionSource<object>();
            try
            {
                var ms = new MemoryStream();
                stream.CopyTo(ms, BufferSize);
                taskSource.SetResult(ms.ToArray());
            }
            catch (Exception e)
            {
                taskSource.SetException(e);
            }
            return taskSource.Task;
        }


        public override Task WriteToStreamAsync(Type type, object value,
            Stream stream, HttpContent content, TransportContext transportContext)
        {
            var taskSource = new TaskCompletionSource<object>();
            try
            {
                if (value == null)
                    value = new byte[0];
                var ms = new MemoryStream((byte[])value);
                ms.CopyTo(stream);
                taskSource.SetResult(null);
            }
            catch (Exception e)
            {
                taskSource.SetException(e);
            }
            return taskSource.Task;
        }
    }
}