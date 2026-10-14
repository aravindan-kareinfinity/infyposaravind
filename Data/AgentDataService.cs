using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class AgentDataService : SAASData
    {
        const string SQL_Search = @"select agent.id id, agent.name as name, contactperson, agent.contactno as contactno, address, cityid, stateid, countryid, 
                                    pincode, agent.isactive as isactive, agent.createdby as createdby, agent.modifiedby, agent.version, agent.createdon, agent.modifiedon, employee.name as employeename,
                                    agent.emailid,agenttypeid,commissionamount,commissionpercentage,pan,gst,agent.taxledgerid,
                                    agentbankid,agentbankbranch,agentbankaccountname,agentbankifci,agentbankaccountno from agent 
                                    left outer join users on users.id=agent.createdby 
                                    left outer join employee on employee.id=users.employeeid  where agent.id=@id";

        const string SQL_Insert = @"INSERT INTO public.agent(name, contactperson, contactno, address, cityid, stateid, countryid, pincode, isactive, createdby, modifiedby, version, createdon, 
        modifiedon, emailid,agenttypeid,commissionamount,commissionpercentage,pan,gst,taxledgerid,agentbankid,agentbankbranch,agentbankaccountname,agentbankifci,agentbankaccountno) 
        VALUES(@name, @contactperson, @contactno, @address, @cityid, @stateid, @countryid, @pincode, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,
        @emailid,@agenttypeid,@commissionamount,@commissionpercentage,@pan,@gst,@taxledgerid,@agentbankid,@agentbankbranch,@agentbankaccountname,@agentbankifci,@agentbankaccountno) RETURNING id";

        const string SQL_Update = @"UPDATE public.agent SET name=@name, contactperson=@contactperson, contactno=@contactno, address=@address, cityid=@cityid, 
        stateid=@stateid, countryid=@countryid, pincode=@pincode, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, emailid=@emailid,
        agentbankid=@agentbankid,agentbankbranch=@agentbankbranch,agentbankaccountname=@agentbankaccountname,agentbankifci=@agentbankifci,agentbankaccountno=@agentbankaccountno,
        agenttypeid=@agenttypeid,commissionamount=@commissionamount,commissionpercentage=@commissionpercentage,pan=@pan,gst=@gst,isactive=@isactive,taxledgerid=@taxledgerid  
        where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.agent set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Agent> Search(AgentCreteria _agent)
        {
            List<Agent> lst = new List<Agent>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_agent.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
               
                    if (!string.IsNullOrEmpty(_agent.name))
                    {
                        qb.AddParameter("agent.name", "%" + _agent.name + "%", " ilike ");
                    }
                    if (_agent.agenttypeid>0)
                    {
                        qb.AddParameter("agent.agenttypeid", _agent.agenttypeid, " = ");
                    }
                    if (_agent.inactive == true)
                    {
                        qb.AddParameter("agent.isactive", false, "=");
                    }
                    else
                        qb.AddParameter("agent.isactive", true, "=");
                    if (!string.IsNullOrEmpty(_agent.contactno))
                    {
                        qb.AddParameter("agent.contactno", "%" + _agent.contactno + "%", " ilike ");
                    }

                    if (_agent.fromdate.Year > 1900)
                        qb.AddParameter("agent.createdon", "fromdate", _agent.fromdate.Date, ">=");
                    if (_agent.todate.Year > 1900)
                        qb.AddParameter("agent.createdon", "todate", _agent.todate.Date.AddDays(1), "<"); command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _agent.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Agent oagent = new Agent();
                        oagent.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oagent.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oagent.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        oagent.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        oagent.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oagent.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        oagent.taxledgerid = reader["taxledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxledgerid"].ToString());
                        oagent.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"].ToString());
                        oagent.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"].ToString());
                        oagent.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        oagent.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oagent.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oagent.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oagent.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oagent.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oagent.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oagent.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        oagent.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        oagent.gst = reader["gst"] == DBNull.Value ? "" : reader["gst"].ToString();

                        oagent.bankbranch = reader["agentbankbranch"] == DBNull.Value ? "" : reader["agentbankbranch"].ToString();
                        oagent.bankaccountname = reader["agentbankaccountname"] == DBNull.Value ? "" : reader["agentbankaccountname"].ToString();
                        oagent.bankifci = reader["agentbankifci"] == DBNull.Value ? "" : reader["agentbankifci"].ToString();
                        oagent.bankaccountno = reader["agentbankaccountno"] == DBNull.Value ? "" : reader["agentbankaccountno"].ToString();
                        oagent.bankid = reader["agentbankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentbankid"]);

                        //bankid,bankbranch,bankaccountname,bankifci,bankaccountno
                        oagent.agenttypeid = reader["agenttypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["agenttypeid"]);
                        oagent.commissionamount = reader["commissionamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["commissionamount"]);
                        oagent.commissionpercentage = reader["commissionpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["commissionpercentage"]);
                        oagent.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                       
                        lst.Add(oagent);
                    }
                }
                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst.ForEach(e => e.name = e.id.ToString());
                }

                if (lst.Count == 1 && lst[0].bankid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var banks = referencelistDataService.SearchList(new ReferencelistCreteria() { id = lst[0].bankid });
                    if (banks.Count > 0)
                        lst[0].bankname = banks[0].name;
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }

        public Agent Insert(Agent oAgent)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oAgent);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oAgent;
        }

        public Agent Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Agent oAgent)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.name == null ? "" : oAgent.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.contactperson == null ? "" : oAgent.contactperson;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.contactno == null ? "" : oAgent.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.address == null ? "" : oAgent.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.cityid;
            command.Parameters.Add("taxledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.taxledgerid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.pincode == null ? "" : oAgent.pincode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config == null || Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_agent == false) ? oAgent.isactive : false;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgent.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.emailid == null ? "" : oAgent.emailid;
            command.Parameters.Add("gst", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.gst == null ? "" : oAgent.gst;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.pan == null ? "" : oAgent.pan;
            command.Parameters.Add("commissionpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgent.commissionpercentage;
            command.Parameters.Add("commissionamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgent.commissionamount;
            command.Parameters.Add("agenttypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgent.agenttypeid;
            command.Parameters.Add("agentbankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.bankid;
            command.Parameters.Add("agentbankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankbranch == null ? "" : oAgent.bankbranch;
            command.Parameters.Add("agentbankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankaccountname == null ? "" : oAgent.bankaccountname;
            command.Parameters.Add("agentbankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankifci == null ? "" : oAgent.bankifci;
            command.Parameters.Add("agentbankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankaccountno == null ? "" : oAgent.bankaccountno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAgent.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_agent)
            {
                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,Services.IntrayManager.EntryType.Agent,
                        oAgent.id, string.Format("AGENT CREATION REQUEST - {0} ", oAgent.name), Context.UserID, 0, 
                        transaction, Context);
            }
            return oAgent;
        }

        public Agent Update(Agent oAgent)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oAgent);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oAgent;
        }

        public Agent Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Agent oAgent)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.name == null ? "" : oAgent.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.contactperson == null ? "" : oAgent.contactperson;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.contactno == null ? "" : oAgent.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.address == null ? "" : oAgent.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.pincode == null ? "" : oAgent.pincode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgent.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("taxledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.taxledgerid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgent.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oAgent.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.emailid == null ? "" : oAgent.emailid;
            command.Parameters.Add("gst", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.gst == null ? "" : oAgent.gst;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.pan == null ? "" : oAgent.pan;
            command.Parameters.Add("commissionpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgent.commissionpercentage;
            command.Parameters.Add("commissionamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oAgent.commissionamount;
            command.Parameters.Add("agenttypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgent.agenttypeid;
            command.Parameters.Add("agentbankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.bankid;
            command.Parameters.Add("agentbankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankbranch == null ? "" : oAgent.bankbranch;
            command.Parameters.Add("agentbankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankaccountname == null ? "" : oAgent.bankaccountname;
            command.Parameters.Add("agentbankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankifci == null ? "" : oAgent.bankifci;
            command.Parameters.Add("agentbankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAgent.bankaccountno == null ? "" : oAgent.bankaccountno;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAgent.version = Convert.ToInt32(reader[0]);
                }
            }

            return oAgent;
        }


        public bool Delete(Agent oAgent)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oAgent);
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

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Agent oAgent)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAgent.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAgent.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAgent.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
