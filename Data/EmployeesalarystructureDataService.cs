using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class EmployeesalarystructureDataService : SAASData
    {
        const string SQL_Search = @"select id, name, fromdate, todate, mode, configuration, fixedbasic, fixedda, deducttextsalaryid,splallowanceid, isactive, createdby, modifiedby, version, createdon, modifiedon from employeesalarystructure where id=@id";

        const string SQL_Insert = @"INSERT INTO public.employeesalarystructure(name, fromdate, todate, mode, configuration, fixedbasic, fixedda, deducttextsalaryid,splallowanceid, isactive, createdby, modifiedby, version, createdon, modifiedon) VALUES(@name, @fromdate, @todate, @mode, @configuration, @fixedbasic, @fixedda, @deducttextsalaryid,@splallowanceid , @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon) RETURNING id";

        const string SQL_Update = @"UPDATE public.employeesalarystructure SET name=@name, fromdate=@fromdate, todate=@todate, mode=@mode, configuration=@configuration, fixedbasic=@fixedbasic, fixedda=@fixedda, deducttextsalaryid=@deducttextsalaryid, splallowanceid=@splallowanceid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.employeesalarystructure set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Employeesalarystructure> Search(EmployeesalarystructureCreteria _employeesalarystructure)
        {
            List<Employeesalarystructure> lst = new List<Employeesalarystructure>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employeesalarystructure.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeesalarystructure.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_employeesalarystructure.Code))
                    //    qb.AddParameter("code", _employeesalarystructure.Code, "=");


                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalarystructure oemployeesalarystructure = new Employeesalarystructure();
                        oemployeesalarystructure.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalarystructure.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalarystructure.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        oemployeesalarystructure.todate = reader["todate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["todate"]);
                        oemployeesalarystructure.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        oemployeesalarystructure.fixedbasic = reader["fixedbasic"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fixedbasic"]);
                        oemployeesalarystructure.fixedda = reader["fixedda"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fixedda"]);
                        oemployeesalarystructure.deducttextsalaryid = reader["deducttextsalaryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deducttextsalaryid"]);
                        oemployeesalarystructure.splallowanceid = reader["splallowanceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["splallowanceid"]);
                        oemployeesalarystructure.configuration_json = reader["configuration"] == DBNull.Value ? "null" : reader["configuration"].ToString();
                        oemployeesalarystructure.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeesalarystructure.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeesalarystructure.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeesalarystructure.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeesalarystructure.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeesalarystructure.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oemployeesalarystructure);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalarystructure);
            }
            return lst;
        }

        internal EmployeesalarystructureReplace Replace(EmployeesalarystructureReplace item)
        {
            string query = @"select employeesalaryheader.id,configuration from employeesalaryheader
                            inner join employee on employee.id = employeesalaryheader.employeeid
                            where (employee.hasleft = false or employee.relievingdate>current_date) and salarystructureid=@salarystructureid";
            SortedList<long, List<Employeesalarystructure.Configuration>> existigValues = new SortedList<long, List<Employeesalarystructure.Configuration>>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(query, conn);
                command.Parameters.Add("salarystructureid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        List<Employeesalarystructure.Configuration> value = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employeesalarystructure.Configuration>>(reader["configuration"] == DBNull.Value ? "null" : reader["configuration"].ToString());
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        if (value != null && value.Exists(e => e.id == item.headerid))
                        {
                            if (value.Find(e => e.id == item.headerid).formula == item.source)
                            {
                                value.Find(e => e.id == item.headerid).formula = item.replace;
                                existigValues.Add(id, value);
                            }
                        }
                    }
                }

                query = "Update employeesalaryheader set configuration=@configuration where id=@id";
                command = GetCommand(query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                command.Parameters.Add("configuration", NpgsqlTypes.NpgsqlDbType.Json).Value = "null";
                foreach(var keyval in existigValues)
                {
                    command.Parameters["configuration"].Value = Newtonsoft.Json.JsonConvert.SerializeObject(keyval.Value);
                    command.Parameters["id"].Value = keyval.Key;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                return null;
            }
            return item;
        }

        public Employeesalarystructure Insert(Employeesalarystructure oEmployeesalarystructure)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oEmployeesalarystructure);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalarystructure);


            }

            return oEmployeesalarystructure;
        }

        public Employeesalarystructure Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalarystructure oEmployeesalarystructure)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeesalarystructure.name == null ? "" : oEmployeesalarystructure.name;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalarystructure.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalarystructure.todate;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalarystructure.mode;
            command.Parameters.Add("fixedbasic", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalarystructure.fixedbasic;
            command.Parameters.Add("fixedda", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalarystructure.fixedda;
            command.Parameters.Add("deducttextsalaryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.deducttextsalaryid;
            command.Parameters.Add("splallowanceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.splallowanceid;
            command.Parameters.Add("configuration", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalarystructure.configuration_json == null ? "null" : oEmployeesalarystructure.configuration_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalarystructure.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalarystructure.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalarystructure.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployeesalarystructure;
        }

        public Employeesalarystructure Update(Employeesalarystructure oEmployeesalarystructure)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeesalarystructure);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalarystructure);


            }

            return oEmployeesalarystructure;
        }

        public Employeesalarystructure Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalarystructure oEmployeesalarystructure)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeesalarystructure.name == null ? "" : oEmployeesalarystructure.name;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalarystructure.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalarystructure.todate;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalarystructure.mode;
            command.Parameters.Add("fixedbasic", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalarystructure.fixedbasic;
            command.Parameters.Add("fixedda", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalarystructure.fixedda;
            command.Parameters.Add("deducttextsalaryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.deducttextsalaryid;
            command.Parameters.Add("splallowanceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.splallowanceid;
            command.Parameters.Add("configuration", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalarystructure.configuration_json == null ? "null" : oEmployeesalarystructure.configuration_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalarystructure.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalarystructure.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalarystructure.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalarystructure.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeesalarystructure;
        }


        public bool Delete(Employeesalarystructure oEmployeesalarystructure)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeesalarystructure);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalarystructure);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalarystructure oEmployeesalarystructure)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalarystructure.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalarystructure.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalarystructure.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
