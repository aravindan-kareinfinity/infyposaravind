using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Data.Sql;
using WebAPI.Models;
using Npgsql;
using WebAPI.Data;
using InfyPOS.Controllers;
using System.Drawing;
using System.Data;
using InfyPOS.Models;
using Newtonsoft.Json;
using System.IO;

namespace InfyPOS.Common
{
    public class ImageData : SAASData
    {
        NpgsqlConnection ImageConnection()
        {
            string connstring = "";
            if (System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"] != null)
            {
                connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOSImage"].ConnectionString;
            }

            if (string.IsNullOrEmpty(connstring) && System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"] != null)
                connstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString;

            if (string.IsNullOrEmpty(connstring))
                connstring = "server=localhost;Port=5432;database=InfyPOS;user id=postgres;password=abc123";

            return new NpgsqlConnection(connstring);
        }


        public string StoreImage(string image, string type)
        {
            try
            {
                string[] parts = image.Split(',');
                byte[] dataasbytes = Convert.FromBase64String(parts[1]);
                var file = new ImageData.File();
                file.createdby = Context.UserID;
                file.modifiedby = Context.UserID;
                file.createdon = DateTimeNow;
                file.modifiedon = DateTimeNow;
                file.filename = "CAM" + DateTimeNow.Ticks;
                file.type = type;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(dataasbytes))
                {
                    Image img = Image.FromStream(ms);
                    var thumbimg = WebAPI.Common.ImageManager.ScaleBySize(img, 64);
                    file.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg, 90);
                    file.data = WebAPI.Common.ImageManager.ToByteArray(img, 60);
                }
                file.ext = ".jpg";
                Insert(file);
                return file.id.ToBase62();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return "";
        }
        public System.Data.DataSet ExecuteDataSet(string Query, SortedList<string, object> Paramters)
        {
            using (NpgsqlConnection conn = ImageConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(Query, conn);
                UpdateParameters(command, Paramters);
                System.Data.DataSet dset = new System.Data.DataSet();
                command.CommandTimeout = 100000;
                NpgsqlDataAdapter adaptoer = new NpgsqlDataAdapter(command);
                adaptoer.SelectCommand.CommandTimeout = 100000;
                adaptoer.Fill(dset);
                conn.Close();
                return dset;
            }
        }

        internal string S3SyncPrepare()
        {
            if (!S3Data.Support)
                return "S3 Not enabled in your application";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            List<long> ids = new List<long>();
            using (var reader = ExecuteReader(@"with allimages
                as
                (
                select replace(cast(schemelist.value as varchar),'""','') imageid from mfgdesign
                LEFT JOIN LATERAL json_array_elements (imagelist :: json) schemelist ON true
                where imagelist is not null
                and imagelist->0 is not null
                and json_typeof(imagelist)='array'
                union all
                select imageid from stock where imageid != ''
                union all
                select schemelist->>'image' from courier
                LEFT JOIN LATERAL json_array_elements (courier.attachment :: json) schemelist ON true
                where courier.attachment->0 is not null and  schemelist->>'image' is not null
                union all
                select schemelist->>'image' from lrentry
                LEFT JOIN LATERAL json_array_elements (lrentry.attachment :: json) schemelist ON true
                where lrentry.attachment->0 is not null and schemelist->>'image' is not null
                )
                select * from allimages", new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetString(0).LongFromBase62());
                }
            }
            sb.AppendLine("Available Images :" + ids.Count);
            foreach (var kv in ids.ChunkBy(1000))
            {
                sb.AppendLine("Updating Images :" + kv.Count);
                using (NpgsqlConnection conn = ImageConnection())
                {
                    conn.Open();
                    try
                    {
                        NpgsqlCommand command = new NpgsqlCommand("update file set type='IMAGE' where id in (" + string.Join(",", ids) + ")", conn);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exp)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
                    }
                    conn.Close();
                }
            }
            return sb.ToString();
        }

        internal string S3Sync()
        {
            if (!S3Data.Support)
                return "S3 Not enabled in your application";

            string query = @"select id,data from file where TYPE='IMAGE' and length(data)>0 order by id limit 250";
            SortedDictionary<long, byte[]> dataList = new SortedDictionary<long, byte[]>();

            using (NpgsqlConnection conn = ImageConnection())
            {
                conn.Open();
                try
                {
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.CommandTimeout = 1000000;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dataList.Add(reader.GetInt64(0), (byte[])reader[1]);
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
                }
                conn.Close();
            }

            var s3Data = S3Data.Instance(this.Context.OrganizationID);
            List<long> updatedList = new List<long>();
            foreach (var kv in dataList)
            {
                if (s3Data.WriteData(kv.Key.ToBase62(), kv.Value))
                {
                    updatedList.Add(kv.Key);
                }
            }
            if (updatedList.Count > 0)
            {
                using (NpgsqlConnection conn = ImageConnection())
                {
                    conn.Open();
                    try
                    {
                        query = "Update file set data=null where id in (" + string.Join(",", updatedList) + ")";
                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exp)
                    {

                    }
                    conn.Close();
                }
            }
            return "Updated Images - " + updatedList.Count;
        }


        internal string S3Sync(long id)
        {
            if (!S3Data.Support)
                return "S3 Not enabled in your application";

            string query = @"select data from file where id=@id";
            byte[] data = null;
            using (NpgsqlConnection conn = ImageConnection())
            {
                conn.Open();
                try
                {
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.AddWithValue("id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            data = (byte[])reader[0];
                        }
                    }
                }
                catch (Exception exp)
                {

                }
                conn.Close();
            }

            var s3Data = S3Data.Instance(this.Context.OrganizationID);
            if (s3Data.WriteData(id.ToBase62(), data))
            {
                using (NpgsqlConnection conn = ImageConnection())
                {
                    conn.Open();
                    try
                    {
                        query = "Update file set data=null where id=@id";
                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.AddWithValue("id", id);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception exp)
                    {

                    }
                    conn.Close();
                }
            }
            return "Updated Images - " + id.ToBase62();
        }

        public class File : BaseEntity
        {
            public long id { get; set; }
            public string filename { get; set; }
            public string type { get; set; }
            public string ext { get; set; }
            public byte[] thumbnail { get; set; }
            public byte[] data { get; set; }
            public long institutionid { get; set; }
            public int version { get; set; }
            public bool isactive { get; set; }
            public DateTime modifiedon { get; set; }
            public DateTime createdon { get; set; }
            public long createdby { get; set; }
            public long modifiedby { get; set; }
        }

        const string SQL_Insert = @"INSERT INTO public.file(filename, ext, thumbnail, data, institutionid, version, isactive, modifiedon, createdon, createdby, modifiedby,type) VALUES(@filename, @ext, @thumbnail, @data, @institutionid, @version, @isactive, @modifiedon, @createdon, @createdby, @modifiedby,@type) RETURNING id";
        const string SQL_SearchThumbnails = @"select thumbnail, id, filename, ext, institutionid, version, isactive, modifiedon, createdon, createdby, modifiedby,type from file where id=@id and isactive=true";
        const string SQL_Search = @"select data, id, filename, ext, institutionid, version, isactive, modifiedon, createdon, createdby, modifiedby,type from file where id=@id and isactive=true";
        const string SQL_SearchInfo = @"select id, institutionid from file where id=@id";
        const string SQL_Remove = @"Delete from file where id=@id and createdby=@createdby";

        public File Insert(File oFile)
        {
            byte[] s3Data = null;
            if (S3Data.Support && oFile.type == "IMAGE")
            {
                s3Data = oFile.data;
                oFile.data = null;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = ImageConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    Insert(transaction, conn, oFile);
                    transaction.Commit();
                    conn.Close();
                }
                if (S3Data.Support && s3Data != null)
                {
                    S3Data.Instance(Context.OrganizationID).WriteData(oFile.id.ToBase62(), s3Data);
                    s3Data = null;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, Environment.StackTrace, oFile);
            }
            return oFile;
        }

        internal List<InfyPOS.Models.ReferenceItem> SearchMyDocument(bool all)
        {
            return SearchMyDocument(all, "0");
        }
        internal List<InfyPOS.Models.ReferenceItem> SearchMyDocument(bool all, string locationid)
        {
            List<InfyPOS.Models.ReferenceItem> result = new List<InfyPOS.Models.ReferenceItem>();
            List<UserBasic> lstUsers = GetAllUsers();
            using (NpgsqlConnection conn = ImageConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand("select file.id,file.filename,file.modifiedon,file.createdby from file where type='Barcode' and file.createdby=@createdby and file.isactive = true order by file.id desc", conn);
                if (all)
                {
                    command = new NpgsqlCommand("select file.id,file.filename,file.modifiedon,file.createdby from file where type='Barcode' and file.isactive = true order by file.id desc", conn);
                }

                long locid;
                long.TryParse(locationid, out locid);
                if (locid > 0)
                {
                    command.CommandText = command.CommandText.Replace("type='Barcode'", "type='Barcode' and institutionid=@locationid");
                }
                command.Parameters.AddWithValue("createdby", Context.UserID);
                command.Parameters.AddWithValue("locationid", locid);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new InfyPOS.Models.ReferenceItem()
                        {
                            id = reader.GetInt64(0),
                            name = reader.GetString(1),
                            data = Convert.ToDateTime(reader[2]).ToString("dd-MM-yyyy hh:mm tt"),
                            type = lstUsers.Exists(e => e.id == reader.GetInt64(3)) ? lstUsers.Find(e => e.id == reader.GetInt64(3)).username : ""
                        });
                    }
                }
                conn.Close();
            }
            return result;
        }

        private List<UserBasic> GetAllUsers()
        {
            List<UserBasic> lst = new List<UserBasic>();
            NpgsqlConnection conn = CreateConnection();
            string SQL_Users = @"select users.id, users.username from users";
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_Users, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserBasic ousers = new UserBasic();
                        ousers.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ousers.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();

                        lst.Add(ousers);
                    }
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return lst;

        }
        public File Update(File oFile)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = ImageConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand("Update file set data=@data,thumbnail=@thumbnail,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);


                    if (oFile.thumbnail == null)
                    {
                        command.Parameters.Add("thumbnail", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
                    }
                    else
                    {
                        command.Parameters.Add("thumbnail", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oFile.thumbnail;
                    }

                    if (oFile.data == null)
                    {
                        command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
                    }
                    else
                    {
                        command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oFile.data;
                    }

                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.id;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, Environment.StackTrace, oFile);
            }
            return oFile;
        }

        //internal byte[] GetImage(long id)
        //{
        //    string query = @"select data from file where id=@id";
        //    var param = new SortedList<string, object>();
        //    param.Add("id", id);
        //    using (NpgsqlConnection conn = ImageConnection())
        //    {
        //        using (var reader = ExecuteReader(query, param))
        //        {
        //            if (reader.Read())
        //            {
        //                return reader[0] as byte[];
        //            }
        //        }
        //    }
        //    return null;
        //}

        public File Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, File oFile)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("filename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFile.filename == null ? "" : oFile.filename;
            command.Parameters.Add("ext", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFile.ext == null ? "" : oFile.ext;
            if (oFile.thumbnail == null)
            {
                command.Parameters.Add("thumbnail", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("thumbnail", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oFile.thumbnail;
            }

            if (oFile.data == null)
            {
                command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oFile.data;
            }

            command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.institutionid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oFile.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oFile.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oFile.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.modifiedby;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFile.type == null ? "" : oFile.type;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oFile.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oFile;
        }

        const string SQL_DeleteMedicalImage = @"Delete from public.profileimage where id=@id";


        public File GetData(long id)
        {
            return GetData(id, null);
        }

        public File GetData(long id, string type)
        {
            File ofile = null;
            using (NpgsqlConnection conn = ImageConnection())
            {
                ofile = GetData(id, type, conn);
            }

            if (ofile == null)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    ofile = GetData(id, type, conn);
                }

            }
            return ofile;
        }

        private File GetData(long id, string type, Npgsql.NpgsqlConnection conn)
        {
            File ofile = null;
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(SQL_Search, conn);
                if (!string.IsNullOrEmpty(type))
                {
                    command = new NpgsqlCommand(SQL_Search + " and type=@type", conn);
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = type;
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ofile = new File();
                        ofile.data = reader[0] == DBNull.Value ? null : reader[0] as byte[];
                        //using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        //{
                        //    using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
                        //    {
                        //        long startIndex = 0;
                        //        int bufferSize = byte.MaxValue * 25;
                        //        byte[] outbyte = new byte[bufferSize];
                        //        long retval = reader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                        //        while (retval == bufferSize)
                        //        {
                        //            bw.Write(outbyte);
                        //            startIndex += bufferSize;
                        //            retval = reader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                        //        }
                        //        bw.Write(outbyte);
                        //        bw.Flush();
                        //        ofile.data = ms.ToArray();
                        //    }
                        //}
                        ofile.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ofile.filename = reader["filename"] == DBNull.Value ? "" : reader["filename"].ToString();
                        ofile.ext = reader["ext"] == DBNull.Value ? "" : reader["ext"].ToString();
                        ofile.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
                        ofile.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ofile.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ofile.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ofile.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ofile.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ofile.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ofile.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
            }

            if (S3Data.Support && ofile != null && (ofile.data == null || ofile.data.Length == 0))
            {
                ofile.data = S3Data.Instance(Context.OrganizationID).GetData(ofile.id.ToBase62());
            }
            return ofile;
        }

        public File GetThumbnail(long id)
        {
            File ofile = null;
            using (NpgsqlConnection conn = ImageConnection())
            {
                ofile = GetThumbnail(id, conn);
            }

            if (ofile == null)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    ofile = GetThumbnail(id, conn);
                }

            }
            return ofile;
        }


        public File GetThumbnail(long id, NpgsqlConnection conn)
        {
            File ofile = new File();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchThumbnails, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
                            {
                                long startIndex = 0;
                                int bufferSize = byte.MaxValue * 25;
                                byte[] outbyte = new byte[bufferSize];
                                try
                                {
                                    if (reader["thumbnail"] != DBNull.Value)
                                    {
                                        long retval = reader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                                        while (retval == bufferSize)
                                        {
                                            bw.Write(outbyte);
                                            startIndex += bufferSize;
                                            retval = reader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                                        }
                                        bw.Write(outbyte);
                                        bw.Flush();
                                        ofile.thumbnail = ms.ToArray();
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        ofile.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ofile.filename = reader["filename"] == DBNull.Value ? "" : reader["filename"].ToString();
                        ofile.ext = reader["ext"] == DBNull.Value ? "" : reader["ext"].ToString();
                        ofile.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
                        ofile.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ofile.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ofile.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ofile.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ofile.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ofile.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ofile.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();

                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
            }
            return ofile;
        }


        public bool Remove(long id, NpgsqlConnection conn)
        {
            File ofile = new File();
            bool canRemove = false;

            conn.Open();
            //NpgsqlCommand command = new NpgsqlCommand(SQL_SearchInfo, conn);
            //command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            //using (System.Data.IDataReader reader = command.ExecuteReader())
            //{
            //    if (reader.Read())
            //    {
            //        ofile.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
            //        ofile.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
            //    }
            //}


            NpgsqlCommand command = new NpgsqlCommand(SQL_Remove, conn);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            canRemove = command.ExecuteNonQuery() > 0;

            conn.Close();

            return canRemove;
        }

        public bool Remove(long id)
        {
            File ofile = new File();
            bool removed = false;
            using (NpgsqlConnection conn = ImageConnection())
            {
                removed = Remove(id, conn);
                conn.Close();
            }
            if (!removed)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    removed = Remove(id, conn);
                    conn.Close();
                }
            }
            if (S3Data.Support && removed)
            {
                //S3Data.Instance(Context.OrganizationID).Delete(id.ToBase62());
            }
            return removed;
        }

        internal ImageController.ExcelDocument GetMyDocument(long v)
        {
            var file = GetData(v);
            if (file != null && file.data != null)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ImageController.ExcelDocument>(System.Text.ASCIIEncoding.ASCII.GetString(file.data));
            }
            return new ImageController.ExcelDocument();
        }

        public class BarcodeFileItem
        {
            public string barcode { get; set; }
            public string uan { get; set; }
            public decimal mqty { get; set; }
            public decimal qty { get; set; }
            [JsonIgnore]
            public string filename { get; set; }
            [JsonIgnore]
            public string location { get; set; }
            [JsonIgnore]
            public string username { get; set; }
        }
        internal DataTable GetBarcodeTable(string[] ids)
        {
            List<BarcodeFileItem> allrows = new List<BarcodeFileItem>();
            string query = @"select File.data from File 
                            where File.id in (" + String.Join(",", ids) + ") order by File.id";
            using (var connection = ImageConnection())
            {
                connection.Open();
                using (var reader = ExecuteReader(query, new SortedList<string, object>(), connection))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            var json = System.Text.ASCIIEncoding.ASCII.GetString(reader[0] as byte[]);
                            List<BarcodeFileItem> rows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BarcodeFileItem>>(json);
                            allrows.AddRange(rows);
                        }
                    }
                }
                connection.Close();
            }
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Qty", typeof(decimal)) });
            foreach (var row in allrows.GroupBy(x => new { x.barcode, x.uan }))
            {
                var barcode = string.IsNullOrEmpty(row.Key.uan) ? row.Key.barcode : row.Key.uan;
                var qty = row.Sum(x => x.qty);
                var mqty = row.Sum(x => x.mqty);
                dt.Rows.Add(new object[] { barcode, mqty > 0 ? mqty : qty });
            }
            return dt;
        }
        internal DataTable GetBarcodeLogTable(string[] ids)
        {
            List<BarcodeFileItem> allrows = new List<BarcodeFileItem>();
            string query = @"select File.data,File.filename,users.username,referencelist.name location from File 
                            inner join users on users.id = file.createdby
                            left outer join referencelist on referencelist.id = institutionid
                            where File.id in (" + String.Join(",", ids) + ") order by File.id";

            using (var connection = ImageConnection())
            {
                connection.Open();
                using (var reader = ExecuteReader(query, new SortedList<string, object>(), connection))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            var json = System.Text.ASCIIEncoding.ASCII.GetString(reader[0] as byte[]);
                            List<BarcodeFileItem> rows = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BarcodeFileItem>>(json);
                            string filename = reader[1].ToString();
                            string username = reader[2].ToString();
                            string location = reader[3].ToString();
                            rows.ForEach(x =>
                            {
                                x.filename = filename;
                                x.username = username;
                                x.location = location;
                            });
                            allrows.AddRange(rows);
                        }
                    }
                }
                connection.Close();
            }
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Qty", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Location", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("User", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("File", typeof(string)) });
            foreach (var row in allrows.GroupBy(x => new { x.barcode, x.uan, x.filename,x.location,x.username }))
            {
                var barcode = string.IsNullOrEmpty(row.Key.uan) ? row.Key.barcode : row.Key.uan;
                var qty = row.Sum(x => x.qty);
                var mqty = row.Sum(x => x.mqty);
                dt.Rows.Add(new object[] { barcode, mqty > 0 ? mqty : qty, row.Key.location, row.Key.username, row.Key.filename });
            }
            return dt;
        }
    }
}