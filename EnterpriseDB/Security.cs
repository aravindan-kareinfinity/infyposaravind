using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;


namespace Security
{
    public class ConnectionString
    {
        public class Connection
        {
            public string userid;
            public string password;
            public bool integrated;
            public string database;
            public string server;
            public Connection(string connection)
            {
                
                string[] items = connection.Split(';');
                for (int i = 0; i < items.Length; i++)
                {
                    string[] item = items[i].Split('=');
                    switch (item[0].ToLower())
                    {
                        case "data source":
                        case "server":
                            server = item[1];
                            break;
                        case "initial catalog":
                        case "database":
                            database = item[1];
                            break;
                        case "user id":
                            userid = item[1];
                            break;
                        case "password":
                            password = item[1];
                            break;
                        case "trusted_connection":
                        case "integrated security":
                            integrated = item[1] == "true";
                            break;
                    }
                }
            }

            public override string ToString()
            {
                if (integrated) return string.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;",
                     server, database);
                return string.Format("Server={0};Database={1};User Id={2};Password={3}",server,
                    database,userid,password);
            }
        }

        public string Password;
        public string Encoder;
        public string connectionString;
        public bool IsValid;
        public Connection connection;
        public ConnectionString(string pConnectionString)
        {
            try
            {
                //pConnectionString = Decode(pConnectionString);
                if (pConnectionString.Split(';').Length == 3 && pConnectionString.Contains("ENC="))
                {
                    Encoder = pConnectionString.Split(';')[0].Substring(4);
                    Password = pConnectionString.Split(';')[1].Substring(4);
                    connectionString = pConnectionString.Split(';')[2].Substring(
                        pConnectionString.Split(';')[2].IndexOf('=') + 1);

                    Encoder = Decript("tripledes", Encoder);
                    Password = Decript(Encoder, Password);
                    connectionString = Decript(Encoder, connectionString, Password);

                    connection = new Connection(connectionString);
                    IsValid = true;
                }
                else
                {
                    connection = new Connection(pConnectionString);
                }
            }
            catch (Exception exp)
            {
            }
        }

        public static string Encript(string encripter, string value,string key)
        {
            Security.SymCryptography encEngine = new Security.SymCryptography(encripter);
            encEngine.Key = "PI"+key+"3.1415";
            encEngine.Salt = "ClinicalPlus1.0";
            return encEngine.Encrypt(value);
        }

        public static string Decript(string encripter, string value,string key)
        {
            Security.SymCryptography encEngine = new Security.SymCryptography(encripter);
            encEngine.Key = "PI"+key+"3.1415";
            encEngine.Salt = "ClinicalPlus1.0";
            return encEngine.Decrypt(value);
        }

        public static string Encript(string encripter, string value)
        {
            Security.SymCryptography encEngine = new Security.SymCryptography(encripter);
            encEngine.Key = "ultimatesoftwares.com";
            encEngine.Salt = "ClinicalPlus1.0";
            return encEngine.Encrypt(value);
        }

        public static string Decript(string encripter, string value)
        {
            Security.SymCryptography encEngine = new Security.SymCryptography(encripter);
            encEngine.Key = "ultimatesoftwares.com";
            encEngine.Salt = "ClinicalPlus1.0";
            return encEngine.Decrypt(value);
        }

        public static string Encode(string str)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(encbuff);
        }

        public static string Decode(string str)
        {
            byte[] decbuff = Convert.FromBase64String(str);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }
    }

    #region Symmetric cryptography class...

    /// <summary>Contains methods and properties for two-way encryption and decryption</summary>
    /// <author>Chidi C. Ezeukwu</author>
    /// <written>May 24, 2004</written>
    /// <Updated>Aug 07, 2004</Updated>
    public class SymCryptography
    {
        #region Private members...

        private string mKey = string.Empty;
        private string mSalt = string.Empty;
        private ServiceProviderEnum mAlgorithm;
        private SymmetricAlgorithm mCryptoService;

        private void SetLegalIV()
        {
            // Set symmetric algorithm
            switch (mAlgorithm)
            {
                case ServiceProviderEnum.Rijndael:
                    mCryptoService.IV = new byte[] { 0xf, 0x6f, 0x13, 0x2e, 0x35, 0xc2, 0xcd, 0xf9, 0x5, 0x46, 0x9c, 0xea, 0xa8, 0x4b, 0x73, 0xcc };
                    break;
                default:
                    mCryptoService.IV = new byte[] { 0xf, 0x6f, 0x13, 0x2e, 0x35, 0xc2, 0xcd, 0xf9 };
                    break;
            }
        }

        #endregion

        #region Public interfaces...

        public enum ServiceProviderEnum : int
        {
            // Supported service providers
            Rijndael,
            RC2,
            DES,
            TripleDES
        }

        public SymCryptography()
        {
            // Default symmetric algorithm
            mCryptoService = new RijndaelManaged();
            mCryptoService.Mode = CipherMode.CBC;
            mAlgorithm = ServiceProviderEnum.Rijndael;
        }

        public SymCryptography(ServiceProviderEnum serviceProvider)
        {
            // Select symmetric algorithm
            switch (serviceProvider)
            {
                case ServiceProviderEnum.Rijndael:
                    mCryptoService = new RijndaelManaged();
                    mAlgorithm = ServiceProviderEnum.Rijndael;
                    break;
                case ServiceProviderEnum.RC2:
                    mCryptoService = new RC2CryptoServiceProvider();
                    mAlgorithm = ServiceProviderEnum.RC2;
                    break;
                case ServiceProviderEnum.DES:
                    mCryptoService = new DESCryptoServiceProvider();
                    mAlgorithm = ServiceProviderEnum.DES;
                    break;
                case ServiceProviderEnum.TripleDES:
                    mCryptoService = new TripleDESCryptoServiceProvider();
                    mAlgorithm = ServiceProviderEnum.TripleDES;
                    break;
            }
            mCryptoService.Mode = CipherMode.CBC;
        }

        public SymCryptography(string serviceProviderName)
        {
            try
            {
                // Select symmetric algorithm
                switch (serviceProviderName.ToLower())
                {
                    case "rijndael":
                        serviceProviderName = "Rijndael";
                        mAlgorithm = ServiceProviderEnum.Rijndael;
                        break;
                    case "rc2":
                        serviceProviderName = "RC2";
                        mAlgorithm = ServiceProviderEnum.RC2;
                        break;
                    case "des":
                        serviceProviderName = "DES";
                        mAlgorithm = ServiceProviderEnum.DES;
                        break;
                    case "tripledes":
                        serviceProviderName = "TripleDES";
                        mAlgorithm = ServiceProviderEnum.TripleDES;
                        break;
                }

                // Set symmetric algorithm
                mCryptoService = (SymmetricAlgorithm)CryptoConfig.CreateFromName(serviceProviderName);
                mCryptoService.Mode = CipherMode.CBC;
            }
            catch
            {
                throw;
            }
        }

        public virtual byte[] GetLegalKey()
        {
            // Adjust key if necessary, and return a valid key
            if (mCryptoService.LegalKeySizes.Length > 0)
            {
                // Key sizes in bits
                int keySize = mKey.Length * 8;
                int minSize = mCryptoService.LegalKeySizes[0].MinSize;
                int maxSize = mCryptoService.LegalKeySizes[0].MaxSize;
                int skipSize = mCryptoService.LegalKeySizes[0].SkipSize;

                if (keySize > maxSize)
                {
                    // Extract maximum size allowed
                    mKey = mKey.Substring(0, maxSize / 8);
                }
                else if (keySize < maxSize)
                {
                    // Set valid size
                    int validSize = (keySize <= minSize) ? minSize : (keySize - keySize % skipSize) + skipSize;
                    if (keySize < validSize)
                    {
                        // Pad the key with asterisk to make up the size
                        mKey = mKey.PadRight(validSize / 8, '*');
                    }
                }
            }
            PasswordDeriveBytes key = new PasswordDeriveBytes(mKey, ASCIIEncoding.ASCII.GetBytes(mSalt));
            return key.GetBytes(mKey.Length);
        }

        public virtual string Encrypt(string plainText)
        {
            byte[] plainByte = ASCIIEncoding.ASCII.GetBytes(plainText);
            byte[] keyByte = GetLegalKey();

            // Set private key
            mCryptoService.Key = keyByte;
            SetLegalIV();

            // Encryptor object
            ICryptoTransform cryptoTransform = mCryptoService.CreateEncryptor();

            // Memory stream object
            MemoryStream ms = new MemoryStream();

            // Crpto stream object
            CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);

            // Write encrypted byte to memory stream
            cs.Write(plainByte, 0, plainByte.Length);
            cs.FlushFinalBlock();

            // Get the encrypted byte length
            byte[] cryptoByte = ms.ToArray();

            // Convert into base 64 to enable result to be used in Xml
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0));
        }

        public virtual string Decrypt(string cryptoText)
        {
            // Convert from base 64 string to bytes
            byte[] cryptoByte = Convert.FromBase64String(cryptoText);
            byte[] keyByte = GetLegalKey();

            // Set private key
            mCryptoService.Key = keyByte;
            SetLegalIV();

            // Decryptor object
            ICryptoTransform cryptoTransform = mCryptoService.CreateDecryptor();
            try
            {
                // Memory stream object
                MemoryStream ms = new MemoryStream(cryptoByte, 0, cryptoByte.Length);

                // Crpto stream object
                CryptoStream cs = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Read);

                // Get the result from the Crypto stream
                StreamReader sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch
            {
                return null;
            }
        }

        public string Key
        {
            get
            {
                return mKey;
            }
            set
            {
                mKey = value;
            }
        }

        public string Salt
        {
            // Salt value
            get
            {
                return mSalt;
            }
            set
            {
                mSalt = value;
            }
        }
        #endregion
    }
    #endregion

    #region Hash Class...

    /// <summary>CHashProtector is a password protection one way encryption algorithm</summary>
    /// <programmer>Chidi C. Ezeukwu</programmer>
    /// <written>May 16, 2004</written>
    /// <Updated>Aug 07, 2004</Updated>

    public class Hash
    {
        #region Private member variables...

        private string mSalt;
        private HashAlgorithm mCryptoService;

        #endregion

        #region Public interfaces...

        public enum ServiceProviderEnum : int
        {
            // Supported algorithms
            SHA1,
            SHA256,
            SHA384,
            SHA512,
            MD5
        }

        public Hash()
        {
            // Default Hash algorithm
            mCryptoService = new SHA1Managed();
        }

        public Hash(ServiceProviderEnum serviceProvider)
        {
            // Select hash algorithm
            switch (serviceProvider)
            {
                case ServiceProviderEnum.MD5:
                    mCryptoService = new MD5CryptoServiceProvider();
                    break;
                case ServiceProviderEnum.SHA1:
                    mCryptoService = new SHA1Managed();
                    break;
                case ServiceProviderEnum.SHA256:
                    mCryptoService = new SHA256Managed();
                    break;
                case ServiceProviderEnum.SHA384:
                    mCryptoService = new SHA384Managed();
                    break;
                case ServiceProviderEnum.SHA512:
                    mCryptoService = new SHA512Managed();
                    break;
            }
        }

        public Hash(string serviceProviderName)
        {
            try
            {
                // Set Hash algorithm
                mCryptoService = (HashAlgorithm)CryptoConfig.CreateFromName(serviceProviderName.ToUpper());
            }
            catch
            {
                throw;
            }
        }

        public virtual string Encrypt(string plainText)
        {
            byte[] cryptoByte = mCryptoService.ComputeHash(ASCIIEncoding.ASCII.GetBytes(plainText + mSalt));

            // Convert into base 64 to enable result to be used in Xml
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.Length);
        }

        public string Salt
        {
            // Salt value
            get
            {
                return mSalt;
            }
            set
            {
                mSalt = value;
            }
        }
        #endregion
    }
    #endregion

}

