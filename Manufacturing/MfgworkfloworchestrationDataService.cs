using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgworkfloworchestrationDataService : SAASData
    {
        const string SQL_Search = @"select id, name, isactive, createdby, modifiedby, version, createdon, modifiedon, stages, mode,stagesupport,fgsupport,bomsupport,inwardsupport,tailoringchargeid,tailoringmode from mfgworkfloworchestration where id=@id";


        const string SQL_Insert = @"INSERT INTO public.mfgworkfloworchestration(name, isactive, createdby, modifiedby, version, createdon, modifiedon, stages, mode,stagesupport,fgsupport,bomsupport,inwardsupport,tailoringchargeid,tailoringmode)
VALUES(@name, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @stages, @mode,@stagesupport,@fgsupport,@bomsupport,@inwardsupport,@tailoringchargeid,@tailoringmode) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgworkfloworchestration SET name=@name, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, stages=@stages, 
mode=@mode,stagesupport=@stagesupport,fgsupport=@fgsupport,bomsupport=@bomsupport,inwardsupport=@inwardsupport,tailoringchargeid=@tailoringchargeid,tailoringmode=@tailoringmode where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkfloworchestration set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgworkfloworchestration> Search(MfgworkfloworchestrationCreteria _mfgworkfloworchestration)
        {

            
            List<Mfgworkfloworchestration> lst = new List<Mfgworkfloworchestration>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkfloworchestration.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkfloworchestration.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgworkfloworchestration.Code))
                    //    qb.AddParameter("code", _mfgworkfloworchestration.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkfloworchestration omfgworkfloworchestration = new Mfgworkfloworchestration();
                        omfgworkfloworchestration.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkfloworchestration.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        omfgworkfloworchestration.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkfloworchestration.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkfloworchestration.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkfloworchestration.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkfloworchestration.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkfloworchestration.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkfloworchestration.stages_json = reader["stages"] == DBNull.Value ? null : reader["stages"].ToString();
                        omfgworkfloworchestration.mode = reader["mode"] == DBNull.Value ? "" : reader["mode"].ToString();

                        omfgworkfloworchestration.bomsupport = reader["bomsupport"] == DBNull.Value ? "No" : Convert.ToBoolean(reader["bomsupport"]) ? "Yes" :"No";
                        omfgworkfloworchestration.stagesupport = reader["stagesupport"] == DBNull.Value ? "No" : Convert.ToBoolean(reader["stagesupport"]) ? "Yes" : "No";
                        omfgworkfloworchestration.fgsupport = reader["fgsupport"] == DBNull.Value ? "No" : Convert.ToBoolean(reader["fgsupport"]) ? "Yes" : "No";
                        
                        omfgworkfloworchestration.inwardsupport = reader["inwardsupport"] == DBNull.Value ? "No" : Convert.ToBoolean(reader["inwardsupport"]) ? "Yes" : "No";
                        omfgworkfloworchestration.tailoringchargeid = reader["tailoringchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringchargeid"]);
                        omfgworkfloworchestration.tailoringmode = reader["tailoringmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tailoringmode"]);
                        lst.Add(omfgworkfloworchestration);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkfloworchestration);
            }
            return lst;
        }

        public Mfgworkfloworchestration Insert(Mfgworkfloworchestration oMfgworkfloworchestration)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgworkfloworchestration);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkfloworchestration);
                throw exp;
            }

            return oMfgworkfloworchestration;
        }

        public Mfgworkfloworchestration Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkfloworchestration oMfgworkfloworchestration)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkfloworchestration.name == null ? "" : oMfgworkfloworchestration.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkfloworchestration.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkfloworchestration.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkfloworchestration.modifiedon;
            command.Parameters.Add("stages", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgworkfloworchestration.stages_json == null ? "null" : oMfgworkfloworchestration.stages_json;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkfloworchestration.mode == null ? "" : oMfgworkfloworchestration.mode;
            command.Parameters.Add("stagesupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.stagesupport == "Yes";
            command.Parameters.Add("fgsupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.fgsupport == "Yes";
            command.Parameters.Add("bomsupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.bomsupport == "Yes";
            command.Parameters.Add("inwardsupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.inwardsupport == "Yes";
            command.Parameters.Add("tailoringmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkfloworchestration.tailoringmode;
            command.Parameters.Add("tailoringchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.tailoringchargeid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkfloworchestration.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgworkfloworchestration;
        }

        public Mfgworkfloworchestration Update(Mfgworkfloworchestration oMfgworkfloworchestration)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkfloworchestration);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkfloworchestration);
                throw exp;
            }

            return oMfgworkfloworchestration;
        }

        public Mfgworkfloworchestration Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkfloworchestration oMfgworkfloworchestration)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkfloworchestration.name == null ? "" : oMfgworkfloworchestration.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkfloworchestration.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkfloworchestration.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkfloworchestration.modifiedon;
            command.Parameters.Add("stages", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgworkfloworchestration.stages_json == null ? "null" : oMfgworkfloworchestration.stages_json;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkfloworchestration.mode == null ? "" : oMfgworkfloworchestration.mode;
            command.Parameters.Add("stagesupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.stagesupport == "Yes";
            command.Parameters.Add("fgsupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.fgsupport == "Yes";
            command.Parameters.Add("bomsupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.bomsupport == "Yes";
            command.Parameters.Add("inwardsupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.inwardsupport == "Yes";
            command.Parameters.Add("tailoringmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkfloworchestration.tailoringmode;
            command.Parameters.Add("tailoringchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.tailoringchargeid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkfloworchestration.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkfloworchestration;
        }


        public bool Delete(Mfgworkfloworchestration oMfgworkfloworchestration)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgworkfloworchestration);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkfloworchestration);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkfloworchestration oMfgworkfloworchestration)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkfloworchestration.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkfloworchestration.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkfloworchestration.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkfloworchestration.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
