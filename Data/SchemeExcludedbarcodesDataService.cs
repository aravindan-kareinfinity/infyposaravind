using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class SchemeExcludedbarcodesDataService : SAASData
    {
        const string SQL_Search = @"select schemeid, locationid, barcode from schemeexcludedbarcodes where schemeid=@schemeid";

        const string SQL_Insert = @"INSERT INTO public.schemeexcludedbarcodes(schemeid, locationid, barcode,version,createdon,modifiedon,isactive) VALUES(@schemeid, @locationid, @barcode,1,now()::timestamp,now()::timestamp,true) RETURNING id";

        const string SQL_Update = @"UPDATE public.schemeexcludedbarcodes SET schemeid=@schemeid, locationid=@locationid, barcode=@barcode,version=version+1,modifiedon=now()::timestamp where schemeid=@schemeid RETURNING version";

        const string SQL_Delete = @"update public.schemeexcludedbarcodes SET modifiedon=now()::timestamp,version=version+1,isactive=false where schemeid=@schemeid and barcode=@barcode";

        public List<Schemebarcodes> Search(SchemebarcodesCreteria _schemebarcodes)
        {
            List<Schemebarcodes> lst = new List<Schemebarcodes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_schemebarcodes.schemeid != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _schemebarcodes.schemeid;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_schemebarcodes.schemeid > 0)
                        qb.AddParameter("schemeid", _schemebarcodes.schemeid, "=");

                    if(_schemebarcodes.locationid>0)
                        qb.AddParameter("locationid", _schemebarcodes.locationid, "=");

                    if (_schemebarcodes.locationid <= 0 && _schemebarcodes.schemeid <= 0)
                        qb.Limit = 1000;

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Schemebarcodes oschemebarcodes = new Schemebarcodes();
                        oschemebarcodes.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                        oschemebarcodes.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oschemebarcodes.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        lst.Add(oschemebarcodes);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _schemebarcodes);
            }
            return lst;
        }

        public Schemebarcodes Insert(Schemebarcodes oSchemebarcodes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSchemebarcodes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSchemebarcodes);
                throw exp;
            }

            return oSchemebarcodes;
        }

        public Schemebarcodes Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Schemebarcodes oSchemebarcodes)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchemebarcodes.schemeid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchemebarcodes.locationid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchemebarcodes.barcode == null ? "" : oSchemebarcodes.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSchemebarcodes.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSchemebarcodes;
        }

        public Schemebarcodes Update(Schemebarcodes oSchemebarcodes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSchemebarcodes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSchemebarcodes);
                throw exp;
            }

            return oSchemebarcodes;
        }

        public Schemebarcodes Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Schemebarcodes oSchemebarcodes)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchemebarcodes.schemeid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchemebarcodes.locationid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchemebarcodes.barcode == null ? "" : oSchemebarcodes.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    //oSchemebarcodes.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSchemebarcodes;
        }


        public bool Delete(Schemebarcodes oSchemebarcodes)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSchemebarcodes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSchemebarcodes);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Schemebarcodes oSchemebarcodes)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            //command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchemebarcodes.isactive;
            //command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchemebarcodes.modifiedby;
            //command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSchemebarcodes.modifiedon;
            //command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSchemebarcodes.version;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchemebarcodes.schemeid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchemebarcodes.barcode;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
