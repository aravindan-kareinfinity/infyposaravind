using Amazon;
using Amazon.S3;
using Amazon.S3.IO;
using Amazon.S3.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace InfyPOS.Common
{
    public class S3Data
    {
        public class Credential
        {
            public string accesskey { get; set; }
            public string secretaccesskey { get; set; }
            public string endpoint { get; set; }
            public string path { get; set; }
            public string bucketname { get; set; }
            public RegionEndpoint regionEndpoint
            {
                get
                {
                    return RegionEndpoint.GetBySystemName(endpoint);
                }
            }
        }

        static bool? support = null;
        public static bool Support
        {
            get
            {
                if (support.HasValue) return support.Value;
                support = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SupportS3"]) &&
    (System.Configuration.ConfigurationManager.AppSettings["SupportS3"].ToLower() == "true" ||
    System.Configuration.ConfigurationManager.AppSettings["SupportS3"].ToLower() == "yes");
                return support.Value;
            }
        }

        static System.Collections.Concurrent.ConcurrentDictionary<long, S3Data> list = new System.Collections.Concurrent.ConcurrentDictionary<long, S3Data>();
        public static S3Data Instance(long organizationid)
        {
            S3Data s3Data = null;
            if(!list.TryGetValue(organizationid,out s3Data))
            {
                if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSS3"] != null)
                {
                    var connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSS3"].ConnectionString;
                    s3Data = new S3Data(connstring);
                    if(!list.TryAdd(organizationid,s3Data))
                    {
                        list.TryGetValue(organizationid, out s3Data);
                    }
                }
            }
            return s3Data;
        }

        AmazonS3Client s3Client = null;
        Credential credential = null;
        public S3Data(string connectionstring)
        {
            var parts = connectionstring.Split(';');
            credential = new Credential();
            foreach(var kv in parts)
            {
                var key = kv.Split('=')[0];
                var value = kv.Split('=')[1];
                if (key.ToLower() == "server")
                    credential.endpoint = value;
                if (key.ToLower() == "user id")
                    credential.accesskey = value;
                if (key.ToLower() == "password")
                    credential.secretaccesskey = value;
                if (key.ToLower() == "database")
                    credential.bucketname = value;
                if (key.ToLower() == "port")
                    credential.path = value;
            }
            s3Client = new AmazonS3Client(credential.accesskey, credential.secretaccesskey, credential.regionEndpoint);
        }


        public bool Delete(string name)
        {
            var req = new DeleteObjectRequest
            {
                BucketName = credential.bucketname,
                Key = name
            };

            Task<DeleteObjectResponse> res = s3Client.DeleteObjectAsync(req);
            Task.WaitAll(res);
            return res.IsCompleted;
        }
        public byte[] GetData(string name)
        {
            var req = new GetObjectRequest
            {
                BucketName = credential.bucketname,
                Key = credential.path + "/"  + name
            };

            Task<GetObjectResponse> res = s3Client.GetObjectAsync(req);
            Task.WaitAll(res);

            if (res.IsCompleted)
            {
                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    res.Result.ResponseStream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            return null;
        }

        public bool WriteData(string name, byte[] data)
        {

            S3DirectoryInfo di = new S3DirectoryInfo(s3Client, credential.bucketname, credential.path);
            if (!di.Exists)
            {
                di.Create();
            }

            using (var stream = new System.IO.MemoryStream(data))
            {
                var req = new PutObjectRequest
                {
                    BucketName = credential.bucketname,
                    Key = credential.path+"/"+name,
                    InputStream = stream
                };

                Task<PutObjectResponse> res = s3Client.PutObjectAsync(req);
                Task.WaitAll(res);
                return res.IsCompleted;
            }
        }
    }
}