using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class EmployeeleavemasterDataService : SAASData
    {
        const string SQL_Search = @"select id, employeeid, leavetype, days, availdays, expirydate, carryforward, isactive, createdby, modifiedby, version, createdon, modifiedon, institutionid from employeeleavemaster where id=@id";


        const string SQL_Insert = @"INSERT INTO public.employeeleavemaster(employeeid, leavetype, days, availdays, expirydate, carryforward, isactive, createdby, modifiedby, version, createdon, modifiedon, institutionid) VALUES(@employeeid, @leavetype, @days, @availdays, @expirydate, @carryforward, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @institutionid) RETURNING id";


        const string SQL_Update = @"UPDATE public.employeeleavemaster SET employeeid=@employeeid, leavetype=@leavetype, days=@days, availdays=@availdays, expirydate=@expirydate, carryforward=@carryforward, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, institutionid=@institutionid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.employeeleavemaster set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Employeeleavemaster> Search(EmployeeleavemasterCreteria _employeeleavemaster)
        {
            List<Employeeleavemaster> lst = new List<Employeeleavemaster>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employeeleavemaster.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeeleavemaster.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("institutionid", _employeeleavemaster.institutionid, "=");
                    if (_employeeleavemaster.employeeid >0)
                        qb.AddParameter("employeeid", _employeeleavemaster.employeeid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeeleavemaster oemployeeleavemaster = new Employeeleavemaster();
                        oemployeeleavemaster.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeeleavemaster.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeeleavemaster.leavetype = reader["leavetype"] == DBNull.Value ? "" : reader["leavetype"].ToString();
                        oemployeeleavemaster.days = reader["days"] == DBNull.Value ? 0 : Convert.ToInt32(reader["days"]);
                        oemployeeleavemaster.availdays = reader["availdays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["availdays"]);
                        oemployeeleavemaster.expirydate = reader["expirydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expirydate"]);
                        oemployeeleavemaster.carryforward = reader["carryforward"] == DBNull.Value ? false : Convert.ToBoolean(reader["carryforward"]);
                        oemployeeleavemaster.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeeleavemaster.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeeleavemaster.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeeleavemaster.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeeleavemaster.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeeleavemaster.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployeeleavemaster.institutionid = reader["institutionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["institutionid"]);
                        lst.Add(oemployeeleavemaster);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeeleavemaster);
            }
            return lst;
        }

        public Employeeleavemaster Insert(Employeeleavemaster oEmployeeleavemaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oEmployeeleavemaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeleavemaster);
                
            }

            return oEmployeeleavemaster;
        }

        public Employeeleavemaster Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeleavemaster oEmployeeleavemaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeleavemaster.employeeid;
            command.Parameters.Add("leavetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeeleavemaster.leavetype == null ? "" : oEmployeeleavemaster.leavetype;
            command.Parameters.Add("days", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.days;
            command.Parameters.Add("availdays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.availdays;
            command.Parameters.Add("expirydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeleavemaster.expirydate;
            command.Parameters.Add("carryforward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeleavemaster.carryforward;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeleavemaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            //command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.GetInstitutionID(oEmployeeleavemaster.institutionid);
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeeleavemaster.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployeeleavemaster;
        }

        public Employeeleavemaster Update(Employeeleavemaster oEmployeeleavemaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeeleavemaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeleavemaster);
                
            }

            return oEmployeeleavemaster;
        }

        public Employeeleavemaster Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeleavemaster oEmployeeleavemaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeleavemaster.id;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeleavemaster.employeeid;
            command.Parameters.Add("leavetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeeleavemaster.leavetype == null ? "" : oEmployeeleavemaster.leavetype;
            command.Parameters.Add("days", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.days;
            command.Parameters.Add("availdays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.availdays;
            command.Parameters.Add("expirydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeleavemaster.expirydate;
            command.Parameters.Add("carryforward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeleavemaster.carryforward;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeleavemaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeleavemaster.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeleavemaster.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            //command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.GetInstitutionID(oEmployeeleavemaster.institutionid);
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeeleavemaster.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeeleavemaster;
        }


        public bool Delete(Employeeleavemaster oEmployeeleavemaster)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeeleavemaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeleavemaster);

                
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeleavemaster oEmployeeleavemaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeleavemaster.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeleavemaster.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeleavemaster.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
