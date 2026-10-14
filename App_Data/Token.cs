using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.Serialization;
using System.Text;

namespace WebAPI.Data
{

    public class Token
    {
        public bool Expired { get; set; }
        public bool IsValid { get; set; }
        public long UserID { get; set; }
        public int Hash { get; set; }
        public bool IsDoctor { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UniqueKey { get; set; }
        public Token()
        {
            IsValid = false;
        }
        public Token(long mrnid, bool isdoctor)
        {
            IsValid = true;
            CreatedOn = DateTime.UtcNow;
            UserID = mrnid;
            UniqueKey = RandomString(8);
            Expired = false;
            IsValid = true;
            IsDoctor = isdoctor;
        }

        public static string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }

        public static string[] ShortMonths = {"JN",
"FB",
"MR",
"AP",
"MY",
"JN",
"JY",
"AG",
"SP",
"OC",
"NV",
"DM"};

    public string Value
        {
            get
            {
                byte[] patbytes = BitConverter.GetBytes(UserID);
                byte[] time = BitConverter.GetBytes(CreatedOn.ToBinary());
                byte[] unique = System.Text.ASCIIEncoding.ASCII.GetBytes(UniqueKey);
                byte[] data = new byte[28];
                Array.Copy(patbytes, 0, data, 0, 8);
                Array.Copy(time, 0, data, 8, 8);
                Array.Copy(time, 0, data, 16, 8);
                int hash = 0;
                unchecked
                {
                    foreach (byte b in data)
                        hash = (hash * 31) ^ b;
                }
                Hash = hash;
                Array.Copy(BitConverter.GetBytes(hash), 0, data, 24, 4);
                return data.ToBase64();
            }
        }

        public Token(string token,bool isDoctor)
        {
            try
            {
                byte[] data = token.FromBase64();
                if(data.Length != 28)
                {
                    IsValid = false;
                    return;
                }
                UserID = BitConverter.ToInt64(data, 0);
                CreatedOn = DateTime.FromBinary(BitConverter.ToInt64(data, 8));
                UniqueKey = System.Text.ASCIIEncoding.ASCII.GetString(data, 16, 8);
                Hash = BitConverter.ToInt32(data, 24);
                IsDoctor = isDoctor;
                byte[] patbytes = BitConverter.GetBytes(UserID);
                byte[] time = BitConverter.GetBytes(CreatedOn.ToBinary());
                byte[] unique = System.Text.ASCIIEncoding.ASCII.GetBytes(UniqueKey);
                byte[] newData = new byte[28];
                Array.Copy(patbytes, 0, newData, 0, 8);
                Array.Copy(time, 0, newData, 8, 8);
                Array.Copy(time, 0, newData, 16, 8);
                int hash = 0;
                unchecked
                {
                    foreach (byte b in newData)
                        hash = (hash * 31) ^ b;
                }
                IsValid = Hash == hash;
            }
            catch (Exception exp)
            {
                IsValid = false;
            }
        }
    }

    public class OTP : Models.Entity
    {
        public bool Expired {
        get
            {
                return ExpiredOn < DateTime.UtcNow;
            }
        }
        public long UserID { get; set; }
        public bool IsDoctor { get; set; }
        public DateTime ExpiredOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string MobileNo { get; set; }
        public string Value { get; set; }
        public OTP()
        {
            
        }
        public OTP(long userid,string mobileno, bool isdoctor)
        {
            CreatedOn = DateTime.UtcNow;
            ExpiredOn = DateTime.UtcNow.AddMinutes(60);
            UserID = userid;
            Value = RandomString(8);
            IsDoctor = isdoctor;
            MobileNo = mobileno;
        }

        string RandomString(int length, string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }

        
    }



    public class CaptchaGenerator : IDisposable
    {

        public CaptchaGenerator(string guid)
        {
            this.guid = guid;
            this.width = 150;
            this.height = 40;
        }
        private string guid;

        public void Init()
        {
            this.text = GetCaptchaString();
            this.SetDimensions(width, height);
            this.GenerateImage();
        }

        private string GetCaptchaString()
        {
            string code = GenerateRandomCode().Replace('1','I').Replace('0','O').ToUpper();

            //HealthSana.Common.Utility.StoreCapcha(guid, code);
            return code;
        }

        public string Text
        {
            get { return this.text; }
        }
        public Bitmap Image
        {
            get { return this.image; }
        }
        public int Width
        {
            get { return this.width; }
        }
        public int Height
        {
            get { return this.height; }
        }
        //Private variable
        private string text;
        private int width;
        private int height;
        private Bitmap image;
        private Random random = new Random();
        //Methods declaration
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                this.image.Dispose();
        }
        private void SetDimensions(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width,
                    "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height,
                    "Argument out of range, must be greater than zero.");
            this.width = width;
            this.height = height;
        }

        public void GenerateImage()
        {
            Bitmap bitmap = new Bitmap
              (this.width, this.height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.width, this.height);
            
            HatchBrush hatchBrush = new HatchBrush(HatchStyle.DiagonalBrick,
                Color.LightGray, Color.White);
            g.FillRectangle(hatchBrush, rect);

            SizeF size;
            float fontSize = rect.Height + 1;
            Font font;

            do
            {
                fontSize--;
                font = new Font(FontFamily.GenericSansSerif, fontSize, FontStyle.Bold);
                size = g.MeasureString(this.text, font);
            } while (size.Width > rect.Width || size.Height>rect.Height);
            
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            GraphicsPath path = new GraphicsPath();
            //path.AddString(this.text, font.FontFamily, (int) font.Style, 
            //    font.Size, rect, format);
            path.AddString(this.text, font.FontFamily, (int)font.Style, 35, rect, format);
            float v = 4F;
            PointF[] points =
          {
                new PointF(this.random.Next(rect.Width) / v, this.random.Next(
                   rect.Height) / v),
                new PointF(rect.Width - this.random.Next(rect.Width) / v, 
                    this.random.Next(rect.Height) / v),
                new PointF(this.random.Next(rect.Width) / v, 
                    rect.Height - this.random.Next(rect.Height) / v),
                new PointF(rect.Width - this.random.Next(rect.Width) / v,
                    rect.Height - this.random.Next(rect.Height) / v)
          };
            Matrix matrix = new Matrix();
            matrix.Translate(0F, 0F);
            path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);
            hatchBrush = new HatchBrush(HatchStyle.Percent50, Color.Black, Color.SkyBlue);
            g.FillPath(hatchBrush, path);
            int m = Math.Max(rect.Width, rect.Height);
            for (int i = 0; i < (int)(rect.Width * rect.Height / 30F); i++)
            {
                int x = this.random.Next(rect.Width);
                int y = this.random.Next(rect.Height);
                int w = this.random.Next(m / 50);
                int h = this.random.Next(m / 50);
                g.FillEllipse(hatchBrush, x, y, w, h);
            }
            font.Dispose();
            hatchBrush.Dispose();
            g.Dispose();
            image = bitmap;
        }
    
        // Function to generate random string with Random class.
        private string GenerateRandomCode()
        {
            Random r = new Random();
            string s = "";
            for (int j = 0; j < 5; j++)
            {
                int i = r.Next(3);
                int ch;
                switch (i)
                {
                    case 1:
                        ch = r.Next(0, 9);
                        s = s + ch.ToString();
                        break;
                    case 2:
                        ch = r.Next(65, 90);
                        s = s + Convert.ToChar(ch).ToString();
                        break;
                    case 3:
                        ch = r.Next(97, 122);
                        s = s + Convert.ToChar(ch).ToString();
                        break;
                    default:
                        ch = r.Next(97, 122);
                        s = s + Convert.ToChar(ch).ToString();
                        break;
                }
                r.NextDouble();
                r.Next(100, 1999);
            }
            return s;
        }
    }
}