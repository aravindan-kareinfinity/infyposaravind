using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class FileDataService : BaseData
    {
        const string SQL_Search = @"select id, filename, ext, thumbnail, data, institutionid, version, isactive, modifiedon, createdon, createdby, modifiedby from file where id=@id";

        const string SQL_Insert = @"INSERT INTO public.file(filename, ext, thumbnail, data, institutionid, version, isactive, modifiedon, createdon, createdby, modifiedby) VALUES(@filename, @ext, @thumbnail, @data, @institutionid, @version, @isactive, @modifiedon, @createdon, @createdby, @modifiedby) RETURNING id";


        const string SQL_Update = @"UPDATE public.file SET filename=@filename, ext=@ext, thumbnail=@thumbnail, data=@data, institutionid=@institutionid, version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.file set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<File> Search(FileCreteria _file)
        {
            List<File> lst = new List<File>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_file.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _file.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_file.Code))
                    //    qb.AddParameter("code", _file.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        File ofile = new File();
                        ofile.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ofile.filename = reader["filename"] == DBNull.Value ? "" : reader["filename"].ToString();
                        ofile.ext = reader["ext"] == DBNull.Value ? "" : reader["ext"].ToString();
                        ofile.thumbnail = reader["thumbnail"] == DBNull.Value ? new byte[0] : (byte[])reader["thumbnail"];
                        ofile.data = reader["data"] == DBNull.Value ? new byte[0] : (byte[])reader["data"];
                        ofile.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
                        ofile.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ofile.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ofile.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ofile.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ofile.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ofile.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        lst.Add(ofile);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, _file);
            }
            return lst;
        }

        public File Insert(File oFile)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oFile);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oFile);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oFile;
        }

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
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFile.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oFile.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oFile.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.modifiedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oFile.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oFile;
        }

        public File Update(File oFile)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oFile);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oFile);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oFile;
        }

        public File Update(NpgsqlTransaction transaction, NpgsqlConnection conn, File oFile)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.id;
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
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFile.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oFile.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oFile.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.modifiedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oFile.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oFile;
        }


        public bool Delete(File oFile)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oFile);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance.Log(exp, WebAPI.Common.Logger.LogType.Error, oFile);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, File oFile)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFile.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oFile.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oFile.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFile.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
