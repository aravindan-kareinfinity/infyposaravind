using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class SystementitymapperDataService : SAASData
    {
        const string SQL_Search = @"select id, type, parentid, childid, createdon, modifiedon, createdby, modifiedby, isactive, version, companyid, locationid from systementitymapper where id=@id";


        const string SQL_Insert = @"
        DELETE from systementitymapper where type=@type and childid=@childid;
        INSERT INTO public.systementitymapper(type, parentid, childid, createdon, modifiedon, createdby, modifiedby, isactive, version, companyid, locationid) VALUES(@type, @parentid, @childid, @createdon, @modifiedon, @createdby, @modifiedby, @isactive, @version, @companyid, @locationid) RETURNING id";


        const string SQL_Update = @"UPDATE public.systementitymapper SET type=@type, parentid=@parentid, childid=@childid, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, companyid=@companyid, locationid=@locationid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.systementitymapper set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Systementitymapper> Search(SystementitymapperCreteria _systementitymapper)
        {
            List<Systementitymapper> lst = new List<Systementitymapper>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_systementitymapper.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _systementitymapper.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_systementitymapper.parentid>0)
                        qb.AddParameter("parentid", _systementitymapper.parentid, "=");
                    qb.AddParameter("type", _systementitymapper.type, "=");
                    qb.AddParameter("companyid", _systementitymapper.companyid, "=");
                    qb.AddParameter("locationid", _systementitymapper.locationid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Systementitymapper osystementitymapper = new Systementitymapper();
                        osystementitymapper.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osystementitymapper.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"]);
                        osystementitymapper.parentid = reader["parentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["parentid"]);
                        osystementitymapper.childid = reader["childid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["childid"]);
                        osystementitymapper.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osystementitymapper.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osystementitymapper.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osystementitymapper.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osystementitymapper.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osystementitymapper.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osystementitymapper.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        osystementitymapper.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        lst.Add(osystementitymapper);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _systementitymapper);
            }
            return lst;
        }

        public bool UpdateMapping(SystemMapperDetail _systemMapperDetail)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand("Delete from systementitymapper where type=@type and parentid=@parentid and locationid=@locationid and companyid=@companyid", conn, transaction);
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _systemMapperDetail.type;
                    command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _systemMapperDetail.parentid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _systemMapperDetail.locationid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _systemMapperDetail.companyid;

                    int successCount = command.ExecuteNonQuery();

                    foreach(var child in _systemMapperDetail.childids)
                    {
                        Insert(transaction, conn, new Systementitymapper()
                        {
                            childid = child.id,
                            companyid = _systemMapperDetail.companyid,
                            locationid = _systemMapperDetail.locationid,
                            type = _systemMapperDetail.type,
                            parentid = _systemMapperDetail.parentid,
                            isactive=_systemMapperDetail.isactive,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            version = 1,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        });
                    }
                    transaction.Commit();
                    conn.Close();
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _systemMapperDetail);
                return false;
            }
            
        }



        public Systementitymapper Insert(Systementitymapper oSystementitymapper)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSystementitymapper);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSystementitymapper);
                throw exp;
            }

            return oSystementitymapper;
        }

        public Systementitymapper Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Systementitymapper oSystementitymapper)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSystementitymapper.type;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.parentid;
            command.Parameters.Add("childid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.childid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSystementitymapper.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSystementitymapper.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSystementitymapper.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSystementitymapper.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSystementitymapper.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSystementitymapper;
        }

        public Systementitymapper Update(Systementitymapper oSystementitymapper)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSystementitymapper);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSystementitymapper);
                throw exp;
            }

            return oSystementitymapper;
        }

        public Systementitymapper Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Systementitymapper oSystementitymapper)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.id;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSystementitymapper.type;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.parentid;
            command.Parameters.Add("childid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.childid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSystementitymapper.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSystementitymapper.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSystementitymapper.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSystementitymapper.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSystementitymapper.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSystementitymapper;
        }


        public bool Delete(Systementitymapper oSystementitymapper)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSystementitymapper);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSystementitymapper);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Systementitymapper oSystementitymapper)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSystementitymapper.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSystementitymapper.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSystementitymapper.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSystementitymapper.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
