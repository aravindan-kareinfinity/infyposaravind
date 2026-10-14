using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class TransportDataService : SAASData
    {
        const string SQL_Search = @"select id, name, contactperson, contactno, address, cityid, stateid, countryid, pincode, isactive, createdby, modifiedby,transportertype,
        dailycollection,bankaccountno,bankifci,bankaccountname,bankbranch,bankid,taxid,vehicles,rcm,allowdpaymentmode,
        version, createdon, modifiedon, emailid,pan,gst,loadingchargeperbox ,loadingchargeperbundle,loadingchargedaycollection,loadingcharges,basetransportid from transport where id=@id";

        const string SQL_Insert = @"INSERT INTO public.transport(name, contactperson, contactno, address, cityid, stateid, countryid, pincode, isactive, createdby, modifiedby, version, 
        createdon, modifiedon, emailid,pan,gst,loadingchargeperbox ,loadingchargeperbundle,loadingchargedaycollection,loadingcharges,transportertype,basetransportid,
        dailycollection,bankaccountno,bankifci,bankaccountname,bankbranch,bankid,taxid,vehicles,rcm,allowdpaymentmode) 
        VALUES(@name, @contactperson, @contactno, @address, @cityid, @stateid, @countryid, @pincode, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @emailid,
        @pan,@gst,@loadingchargeperbox ,@loadingchargeperbundle,@loadingchargedaycollection,@loadingcharges,@transportertype,@basetransportid,
        @dailycollection,@bankaccountno,@bankifci,@bankaccountname,@bankbranch,@bankid,@taxid,@vehicles,@rcm,@allowdpaymentmode) RETURNING id";

        const string SQL_Update = @"UPDATE public.transport SET name=@name, contactperson=@contactperson, contactno=@contactno, address=@address, 
        cityid=@cityid, stateid=@stateid, countryid=@countryid, pincode=@pincode, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,
        emailid=@emailid,pan=@pan,gst=@gst,loadingchargeperbox=@loadingchargeperbox ,loadingchargeperbundle=@loadingchargeperbundle,transportertype=@transportertype,
        loadingchargedaycollection=@loadingchargedaycollection,loadingcharges=@loadingcharges,basetransportid=@basetransportid,
        dailycollection=@dailycollection,bankaccountno=@bankaccountno,bankifci=@bankifci,bankaccountname=@bankaccountname,allowdpaymentmode=@allowdpaymentmode,
        bankbranch=@bankbranch,bankid=@bankid,taxid=@taxid,vehicles=@vehicles,rcm=@rcm where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.transport set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Transport> Search(TransportCreteria _transport)
        {
            List<Transport> lst = new List<Transport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_transport.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (!string.IsNullOrEmpty(_transport.name))
                    {
                        qb.AddParameter("name", "%" + _transport.name + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_transport.contactno))
                    {
                        qb.AddParameter("contactno", "%" + _transport.contactno + "%", " ilike ");
                    }
                    if (_transport.transportertype == 1 || _transport.transportertype == 3)
                    {
                        qb.AddParameter("transportertype", _transport.transportertype, " = ");
                    }
                    else if (_transport.transportertype>0)
                    {
                        qb.AddInParamer("transportertype", "0," + _transport.transportertype);
                    }
                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _transport.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Transport otransport = new Transport();
                        otransport.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otransport.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        otransport.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        otransport.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        otransport.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        otransport.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        otransport.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"].ToString());
                        otransport.basetransportid = reader["basetransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["basetransportid"].ToString());
                        otransport.transportertype = reader["transportertype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["transportertype"].ToString());
                        otransport.vehicles = reader["vehicles"] == DBNull.Value ? "" : reader["vehicles"].ToString();
                        otransport.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        otransport.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"].ToString());
                        otransport.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        otransport.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otransport.rcm = reader["rcm"] == DBNull.Value ? false : Convert.ToBoolean(reader["rcm"]);
                        otransport.paymentmode = reader["allowdpaymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["allowdpaymentmode"].ToString());
                        
                        otransport.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otransport.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otransport.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        otransport.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otransport.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otransport.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        otransport.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        otransport.gst = reader["gst"] == DBNull.Value ? "" : reader["gst"].ToString();

                        otransport.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        otransport.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        otransport.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        otransport.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        otransport.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        otransport.dailycollection = reader["dailycollection"] == DBNull.Value ? false : Convert.ToBoolean(reader["dailycollection"]);

                        otransport.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otransport.loadingchargeperbox = reader["loadingchargeperbox"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingchargeperbox"].ToString());
                        otransport.loadingchargeperbundle = reader["loadingchargeperbundle"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingchargeperbundle"].ToString());
                        otransport.loadingcharges_json = reader["loadingcharges"] == DBNull.Value ? null : reader["loadingcharges"].ToString();

                        lst.Add(otransport);
                    }
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
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _transport);
            }
            return lst;
        }

        public Transport Insert(Transport oTransport)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oTransport);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oTransport;
        }

        public Transport Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Transport oTransport)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.name == null ? "" : oTransport.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.contactperson == null ? "" : oTransport.contactperson;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.contactno == null ? "" : oTransport.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.address == null ? "" : oTransport.address;

            command.Parameters.Add("dailycollection", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.dailycollection;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankaccountname == null ? "" : oTransport.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankaccountno == null ? "" : oTransport.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankifci == null ? "" : oTransport.bankifci;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankbranch == null ? "" : oTransport.bankbranch;
            command.Parameters.Add("vehicles", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.vehicles == null ? "" : oTransport.vehicles;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.bankid;

            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.cityid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.taxid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.stateid;
            command.Parameters.Add("basetransportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.basetransportid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.pincode == null ? "" : oTransport.pincode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.isactive;
            command.Parameters.Add("rcm", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.rcm;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.version;
            command.Parameters.Add("allowdpaymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.paymentmode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.emailid == null ? "" : oTransport.emailid;
            command.Parameters.Add("gst", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.gst == null ? "" : oTransport.gst;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.pan == null ? "" : oTransport.pan;

            command.Parameters.Add("loadingchargedaycollection", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.loadingchargedaycollection;
            command.Parameters.Add("loadingchargeperbox", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransport.loadingchargeperbox;
            command.Parameters.Add("loadingchargeperbundle", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransport.loadingchargeperbundle;

            command.Parameters.Add("transportertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.transportertype;

            

            command.Parameters.Add("loadingcharges", NpgsqlTypes.NpgsqlDbType.Json).Value = oTransport.loadingcharges_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTransport.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTransport;
        }

        public Transport Update(Transport oTransport)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTransport);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oTransport;
        }

        public Transport Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Transport oTransport)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.name == null ? "" : oTransport.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.contactperson == null ? "" : oTransport.contactperson;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.contactno == null ? "" : oTransport.contactno;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.address == null ? "" : oTransport.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.stateid;
            command.Parameters.Add("basetransportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.basetransportid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.pincode == null ? "" : oTransport.pincode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.version;
            command.Parameters.Add("allowdpaymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.paymentmode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransport.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("dailycollection", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.dailycollection;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankaccountname == null ? "" : oTransport.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankaccountno == null ? "" : oTransport.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankifci == null ? "" : oTransport.bankifci;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.bankbranch == null ? "" : oTransport.bankbranch;
            command.Parameters.Add("vehicles", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.vehicles == null ? "" : oTransport.vehicles;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.bankid;
            command.Parameters.Add("rcm", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.rcm;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.taxid;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.emailid == null ? "" : oTransport.emailid;
            command.Parameters.Add("gst", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.gst == null ? "" : oTransport.gst;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransport.pan == null ? "" : oTransport.pan;
            command.Parameters.Add("loadingchargedaycollection", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.loadingchargedaycollection;
            command.Parameters.Add("loadingchargeperbox", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransport.loadingchargeperbox;
            command.Parameters.Add("loadingchargeperbundle", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransport.loadingchargeperbundle;
            command.Parameters.Add("loadingcharges", NpgsqlTypes.NpgsqlDbType.Json).Value = oTransport.loadingcharges_json;
            command.Parameters.Add("transportertype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.transportertype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTransport.version = Convert.ToInt32(reader[0]);
                }
            }
            return oTransport;
        }

        public bool Delete(Transport oTransport)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTransport);
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

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Transport oTransport)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransport.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransport.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransport.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
