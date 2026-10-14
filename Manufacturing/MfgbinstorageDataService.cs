using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgbinstorageDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, locationid, code, height, width, rowindex, columnindex, maxqty, allocated, parentid, disabled,uniquecode from mfgbinstorage where id=@id";


        const string SQL_Insert = @"INSERT INTO public.mfgbinstorage(isactive, createdby, modifiedby, version, createdon, modifiedon, locationid, code, height, width, rowindex, columnindex, maxqty, allocated, parentid, disabled,uniquecode) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @locationid, @code, @height, @width, @rowindex, @columnindex, @maxqty, @allocated, @parentid, @disabled,@uniquecode) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgbinstorage SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, locationid=@locationid, code=@code, height=@height, width=@width, rowindex=@rowindex, columnindex=@columnindex, maxqty=@maxqty, allocated=@allocated, parentid=@parentid, disabled=@disabled where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgbinstorage set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgbinstorage> Search(MfgbinstorageCreteria _mfgbinstorage)
        {
            List<Mfgbinstorage> lst = new List<Mfgbinstorage>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgbinstorage.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgbinstorage.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_mfgbinstorage.ids != null && _mfgbinstorage.ids.Count > 0)
                        qb.AddInParamer("id", string.Join(",", _mfgbinstorage.ids));

                    if (!string.IsNullOrEmpty(_mfgbinstorage.code))
                        qb.AddParameter("code", _mfgbinstorage.code, "=");

                    if (!string.IsNullOrEmpty(_mfgbinstorage.parentid) && _mfgbinstorage.parentid.ToUpper() != "AUTO")
                        qb.AddParameter("parentid", _mfgbinstorage.parentid, "=");

                    //if (!string.IsNullOrEmpty(_mfgbinstorage.Code))
                    //    qb.AddParameter("code", _mfgbinstorage.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgbinstorage omfgbinstorage = new Mfgbinstorage();
                        omfgbinstorage.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgbinstorage.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgbinstorage.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgbinstorage.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgbinstorage.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgbinstorage.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgbinstorage.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgbinstorage.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgbinstorage.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfgbinstorage.uniquecode = reader["uniquecode"] == DBNull.Value ? "" : reader["uniquecode"].ToString();
                        omfgbinstorage.height = reader["height"] == DBNull.Value ? 0 : Convert.ToInt32(reader["height"]);
                        omfgbinstorage.width = reader["width"] == DBNull.Value ? 0 : Convert.ToInt32(reader["width"]);
                        omfgbinstorage.rowindex = reader["rowindex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowindex"]);
                        omfgbinstorage.columnindex = reader["columnindex"] == DBNull.Value ? 0 : Convert.ToInt32(reader["columnindex"]);
                        omfgbinstorage.maxqty = reader["maxqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxqty"]);
                        omfgbinstorage.allocated = reader["allocated"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["allocated"]);
                        omfgbinstorage.parentid = reader["parentid"] == DBNull.Value ? "" : reader["parentid"].ToString();
                        omfgbinstorage.disabled = reader["disabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["disabled"]);
                        lst.Add(omfgbinstorage);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgbinstorage);
            }
            return lst;
        }

        public Mfgbinstorage Insert(Mfgbinstorage oMfgbinstorage)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgbinstorage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgbinstorage);
                throw exp;
            }

            return oMfgbinstorage;
        }

        public Mfgbinstorage Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgbinstorage oMfgbinstorage)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstorage.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstorage.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstorage.modifiedon;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.locationid;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstorage.code == null ? "" : oMfgbinstorage.code;
            command.Parameters.Add("uniquecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstorage.uniquecode == null ? "" : oMfgbinstorage.uniquecode;
            command.Parameters.Add("height", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.height;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.width;
            command.Parameters.Add("rowindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.rowindex;
            command.Parameters.Add("columnindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.columnindex;
            command.Parameters.Add("maxqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstorage.maxqty;
            command.Parameters.Add("allocated", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstorage.allocated;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstorage.parentid == null ? "" : oMfgbinstorage.parentid;
            command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstorage.disabled;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgbinstorage.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            if (oMfgbinstorage.basebin != null)
                oMfgbinstorage.basebin.uid = oMfgbinstorage.id;

            return oMfgbinstorage;
        }

        public bool DeleteAll(List<long> ids, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = new NpgsqlCommand("update Mfgbinstorage set isactive = false where id in ("+string.Join(",", ids)+")", conn, transaction);
            int successCount = command.ExecuteNonQuery();
            return successCount == ids.Count;
        }

        public void ChangeAll(List<Mfgbinstorage> alteredList, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            List< Mfgbinstorage> list = Search(new MfgbinstorageCreteria()
            {
                ids = alteredList.ConvertAll(e => e.id)
            });
            alteredList.ForEach(e =>
            {
                var changed = list.Find(x => x.id == e.id);
                changed.code = e.code;
                changed.rowindex = e.rowindex;
                changed.columnindex = e.columnindex;
                changed.maxqty = e.maxqty;
                changed.width = e.width;
                changed.height = e.height;
                changed.disabled = e.disabled;
                Update(transaction ,conn, changed);
            });
        }

        public void InsertAll(List<Mfgbinstorage> newBinList, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("Mfgbinstorage.UniqueCode", 0);
            foreach (var item in newBinList)
            {
                item.uniquecode = sequence.GetNextNumber(Context,conn, transaction);
                Insert(transaction, conn, item);
            }
        }

        public Mfgbinstorage Update(Mfgbinstorage oMfgbinstorage)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgbinstorage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgbinstorage);
                throw exp;
            }

            return oMfgbinstorage;
        }


        public Mfgbinstorage Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgbinstorage oMfgbinstorage)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstorage.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstorage.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstorage.modifiedon;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.locationid;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstorage.code == null ? "" : oMfgbinstorage.code;
            command.Parameters.Add("height", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.height;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.width;
            command.Parameters.Add("rowindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.rowindex;
            command.Parameters.Add("columnindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.columnindex;
            command.Parameters.Add("maxqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstorage.maxqty;
            command.Parameters.Add("allocated", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstorage.allocated;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstorage.parentid == null ? "" : oMfgbinstorage.parentid;
            command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstorage.disabled;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgbinstorage.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgbinstorage;
        }


        public bool Delete(Mfgbinstorage oMfgbinstorage)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgbinstorage);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgbinstorage);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgbinstorage oMfgbinstorage)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstorage.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstorage.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstorage.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstorage.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
