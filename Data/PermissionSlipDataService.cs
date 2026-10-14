using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class PermissionSlipDataService : SAASData
    {
        const string SQL_Search = @"select leaverequest.id, leaverequest.isactive as isactive, leaverequest.createdby as createdby, leaverequest.modifiedby, leaverequest.version, leaverequest.createdon, leaverequest.modifiedon,
                                    requestedon,fromleavedate,toleavedate,fromtime,totime,employeeid,approvedby,reason,ishalfday, 
deductsalary,mode, employee.name as name, employee.employeecode as employeecode,
section.name sectionname,department.name department,designation.name designation,floor.name floor,location.name locationname from leaverequest 
inner join employee on leaverequest.employeeid = employee.id 
left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
where leaverequest.id=@id";


        const string SQL_Insert = @"INSERT INTO public.leaverequest(requestedon, fromleavedate, toleavedate, fromtime, totime, employeeid, approvedby, reason,ishalfday,deductsalary, isactive, createdby, modifiedby, version, createdon, modifiedon,mode) VALUES(@requestedon, @fromleavedate, @toleavedate, @fromtime, @totime, @employeeid, @approvedby, @reason,@ishalfday,@deductsalary, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@mode) RETURNING id";


        const string SQL_Update = @"UPDATE public.leaverequest SET requestedon=@requestedon, fromleavedate=@fromleavedate, toleavedate=@toleavedate, fromtime=@fromtime, totime=@totime, employeeid=@employeeid, approvedby=@approvedby, reason=@reason,ishalfday=@ishalfday,deductsalary=@deductsalary, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,mode=@mode, isactive=@isactive where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.leaverequest set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<PermissionSlipDetail> Search(PermissionSlipCreteria _PermissionSlip)
        {
            List<PermissionSlipDetail> lst = new List<PermissionSlipDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_PermissionSlip.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_PermissionSlip.inactive == true)
                        qb.AddParameter("leaverequest.isactive", false, "=");
                    else
                        qb.AddParameter("leaverequest.isactive", true, "=");

                    if (_PermissionSlip.companyid>0)
                        qb.AddParameter("employee.companyid", _PermissionSlip.companyid, "=");

                    if (_PermissionSlip.fromdate.Year > 1900)
                        qb.AddParameter("leaverequest.createdon", "fromdate", _PermissionSlip.fromdate.Date, ">=");
                    if (_PermissionSlip.todate.Year > 1900)
                        qb.AddParameter("leaverequest.createdon", "todate", _PermissionSlip.todate.Date.AddDays(1), "<"); command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _PermissionSlip.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PermissionSlipDetail oPermissionSlip = new PermissionSlipDetail();
                        oPermissionSlip.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oPermissionSlip.requestedon = reader["requestedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["requestedon"]);
                        oPermissionSlip.fromleavedate = reader["fromleavedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromleavedate"]);
                        oPermissionSlip.toleavedate = reader["toleavedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["toleavedate"]);
                        oPermissionSlip.fromtime = reader["fromtime"] == DBNull.Value ? "" : reader["fromtime"].ToString();
                        oPermissionSlip.totime = reader["totime"] == DBNull.Value ? "" : reader["totime"].ToString();
                        oPermissionSlip.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oPermissionSlip.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        oPermissionSlip.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        oPermissionSlip.ishalfday = reader["ishalfday"] == DBNull.Value ? false : Convert.ToBoolean(reader["ishalfday"]);
                        oPermissionSlip.deductsalary = reader["deductsalary"] == DBNull.Value ? false : Convert.ToBoolean(reader["deductsalary"]);
                        oPermissionSlip.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oPermissionSlip.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oPermissionSlip.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oPermissionSlip.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oPermissionSlip.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oPermissionSlip.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oPermissionSlip.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"].ToString());
                        
                        oPermissionSlip.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["totime"].ToString();
                        oPermissionSlip.department = reader["department"] == DBNull.Value ? "" : reader["department"].ToString();
                        oPermissionSlip.designation = reader["totime"] == DBNull.Value ? "" : reader["designation"].ToString();
                        oPermissionSlip.floor = reader["floor"] == DBNull.Value ? "" : reader["floor"].ToString();
                        oPermissionSlip.locationname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();

                        oPermissionSlip.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oPermissionSlip.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        lst.Add(oPermissionSlip);
                    }
                }
               
                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }

        public PermissionSlip Insert(PermissionSlip oPermissionSlip)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    

                    Insert(transaction, conn, oPermissionSlip);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oPermissionSlip;
        }

        public PermissionSlip Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, PermissionSlip oPermissionSlip)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPermissionSlip.requestedon;
            command.Parameters.Add("fromleavedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPermissionSlip.fromleavedate;
            command.Parameters.Add("toleavedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPermissionSlip.toleavedate;

            command.Parameters.Add("fromtime", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPermissionSlip.fromtime == null ? "" : oPermissionSlip.fromtime;
            command.Parameters.Add("totime", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPermissionSlip.totime == null ? "" : oPermissionSlip.totime;
            
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPermissionSlip.employeeid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPermissionSlip.approvedby;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPermissionSlip.reason == null ? "" : oPermissionSlip.reason;
            command.Parameters.Add("ishalfday", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.ishalfday;
            command.Parameters.Add("deductsalary", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.deductsalary;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPermissionSlip.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPermissionSlip.mode;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPermissionSlip.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            
            return oPermissionSlip;
        }

        public PermissionSlip Update(PermissionSlip oPermissionSlip)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPermissionSlip);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oPermissionSlip;
        }

        public PermissionSlip Update(NpgsqlTransaction transaction, NpgsqlConnection conn, PermissionSlip oPermissionSlip)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPermissionSlip.id;
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPermissionSlip.requestedon;
            command.Parameters.Add("fromleavedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPermissionSlip.fromleavedate;
            command.Parameters.Add("toleavedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPermissionSlip.toleavedate;

            command.Parameters.Add("fromtime", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPermissionSlip.fromtime == null ? "" : oPermissionSlip.fromtime;
            command.Parameters.Add("totime", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPermissionSlip.totime == null ? "" : oPermissionSlip.totime;

            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPermissionSlip.employeeid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPermissionSlip.approvedby;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPermissionSlip.reason == null ? "" : oPermissionSlip.reason;
            command.Parameters.Add("ishalfday", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.ishalfday;
            command.Parameters.Add("deductsalary", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.deductsalary;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPermissionSlip.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPermissionSlip.mode;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPermissionSlip.version = Convert.ToInt32(reader[0]);
                }
            }

            return oPermissionSlip;
        }

        public PermissionSlipPrint Print(PermissionSlipPrint _item)
        {
            var returnprint = Search(new PermissionSlipCreteria()
            {
                id = _item.id
            });
            
            PermissionSlipPrint ret = new PermissionSlipPrint();
            ret.mode = returnprint[0].mode;
            ret.requestedon = returnprint[0].requestedon;
            ret.toleavedate = returnprint[0].toleavedate;
            ret.fromleavedate = returnprint[0].fromleavedate;
            ret.approvedby = returnprint[0].approvedby;
            ret.employeeid = returnprint[0].employeeid;
            ret.reason = returnprint[0].reason;
            ret.fromtime = returnprint[0].fromtime;
            ret.totime = returnprint[0].totime;
            ret.name = returnprint[0].name;
            ret.employeecode = returnprint[0].employeecode;

            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            return ret;
        }

        public bool Delete(PermissionSlip oPermissionSlip)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPermissionSlip);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, PermissionSlip oPermissionSlip)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPermissionSlip.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPermissionSlip.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPermissionSlip.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
