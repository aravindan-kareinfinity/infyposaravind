using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class SuppliercompanyDataService : SAASData
    {
        const string SQL_Search = @"select id, supplierid, address, cityid, stateid, countryid, pincode, contactperson, tallyledger, agentid, companycontactno, companyemailid, tinanddate, cstanddate, areacode, tan, pan, bankaccountname, bankaccountno,
        bankifci,bankid,bankbranch, isactive, createdby, modifiedby, version, createdon, modifiedon, name,isinterstatesale,preferredtransportid,msmeno, tdsgroupid, addresslist, attachment from suppliercompany where id=@id";

        const string SQL_SearchID = @"select supplier.code, suppliercompany.id, supplierid, address, cityid, stateid, countryid, pincode, suppliercompany.contactperson, 
tallyledger, agentid, companycontactno, companyemailid, tinanddate, cstanddate, areacode, supplier.contactnumber,supplier.emailid,
tan, pan, bankaccountname, bankaccountno, bankifci,bankid,bankbranch, suppliercompany.isactive, suppliercompany.createdby, suppliercompany.modifiedby, 
suppliercompany.version, suppliercompany.createdon, suppliercompany.modifiedon, suppliercompany.name,isinterstatesale,preferredtransportid,
referencelist.name cityname, state.name statename, bank.name bankname,msmeno, tdsgroupid, addresslist, attachment
from suppliercompany 
inner join supplier on supplier.id = suppliercompany.supplierid 
left outer join referencelist on referencelist.id = suppliercompany.cityid 
left outer join referencelist state on state.id = suppliercompany.stateid 
left outer join referencelist bank on bank.id = suppliercompany.bankid ";

        const string SQL_SearchAll = @"select id, supplierid, address, cityid, stateid, countryid, pincode, contactperson, tallyledger, agentid, companycontactno, companyemailid, tinanddate, cstanddate, areacode, tan, pan, bankaccountname, bankaccountno, 
        bankifci,bankid,bankbranch, isactive, createdby, modifiedby, version, createdon, modifiedon, name,isinterstatesale,preferredtransportid,msmeno, tdsgroupid, addresslist, attachment from suppliercompany where supplierid=@supplierid";


        const string SQL_Insert = @"INSERT INTO public.suppliercompany(supplierid, address, cityid, stateid, countryid, pincode, contactperson, tallyledger, agentid, companycontactno, companyemailid, tinanddate, cstanddate, areacode, tan, pan, bankaccountname, bankaccountno, bankifci,bankid,bankbranch, isactive, createdby, modifiedby, version, createdon, modifiedon, name,isinterstatesale,preferredtransportid,msmeno,tdsgroupid,addresslist,attachment) 
                                    VALUES(@supplierid, @address, @cityid, @stateid, @countryid, @pincode, @contactperson, @tallyledger, @agentid, @companycontactno, @companyemailid, @tinanddate, @cstanddate, @areacode, @tan, @pan, @bankaccountname, @bankaccountno, @bankifci,@bankid,@bankbranch, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @name,@isinterstatesale,@preferredtransportid,@msmeno,@tdsgroupid,@addresslist,@attachment) RETURNING id";


        const string SQL_Update = @"UPDATE public.suppliercompany SET supplierid=@supplierid, address=@address, cityid=@cityid, stateid=@stateid, countryid=@countryid, pincode=@pincode, contactperson=@contactperson, tallyledger=@tallyledger, agentid=@agentid, companycontactno=@companycontactno, companyemailid=@companyemailid, tinanddate=@tinanddate, cstanddate=@cstanddate, areacode=@areacode,
        tan=@tan, pan=@pan, bankaccountname=@bankaccountname, bankaccountno=@bankaccountno, bankifci=@bankifci,bankid=@bankid,bankbranch=@bankbranch, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, name=@name , isinterstatesale=@isinterstatesale, isactive=@isactive, preferredtransportid=@preferredtransportid,
        msmeno=@msmeno,tdsgroupid=@tdsgroupid,addresslist=@addresslist,attachment=@attachment
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.suppliercompany set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.suppliercompany set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where supplierid=@supplierid";
        public bool ismigration { get; set; }
        public List<Suppliercompany> Search(SuppliercompanyCreteria _suppliercompany)
        {
            List<Suppliercompany> lst = new List<Suppliercompany>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_suppliercompany.id != 0)
                {
                    if (_suppliercompany.isprint)
                    {
                        QueryBuilder qb = new QueryBuilder();
                        qb.CurrentQuery = SQL_SearchID;
                        qb.AddParameter("suppliercompany.id", _suppliercompany.id, "=");
                        command = qb.GetCommand(conn);
                    }
                    else
                        command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchID;

                    if (!ismigration &&
                        _suppliercompany.supplierid == 0 && (_suppliercompany.ids == null || _suppliercompany.ids.Count == 0))
                        throw new Exception("Supplier/Supplier company id missing in Supplier company serarch.");

                    if (_suppliercompany.supplierid > 0)
                        qb.AddParameter("supplierid", _suppliercompany.supplierid, "=");

                    if (!_suppliercompany.isprint)
                    {
                        qb.AddParameter("supplier.isactive", true, "=");
                        qb.AddParameter("suppliercompany.isactive", true, "=");
                    }

                    if (_suppliercompany.ids != null && _suppliercompany.ids.Count > 0)
                        qb.AddParameter("supplier.id", "supplierids", _suppliercompany.ids, " in ", "(" + string.Join(",", _suppliercompany.ids) + ")");

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _suppliercompany.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Suppliercompany osuppliercompany = new Suppliercompany();
                        osuppliercompany.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osuppliercompany.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        osuppliercompany.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osuppliercompany.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        osuppliercompany.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"].ToString());
                        osuppliercompany.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"].ToString());
                        osuppliercompany.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        osuppliercompany.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        osuppliercompany.tallyledger = reader["tallyledger"] == DBNull.Value ? "" : reader["tallyledger"].ToString();
                        osuppliercompany.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"].ToString());
                        osuppliercompany.tinanddate = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        osuppliercompany.cstanddate = reader["cstanddate"] == DBNull.Value ? "" : reader["cstanddate"].ToString();
                        osuppliercompany.areacode = reader["areacode"] == DBNull.Value ? "" : reader["areacode"].ToString();
                        osuppliercompany.tan = reader["tan"] == DBNull.Value ? "" : reader["tan"].ToString();
                        osuppliercompany.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        osuppliercompany.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        osuppliercompany.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        osuppliercompany.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        osuppliercompany.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        osuppliercompany.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        osuppliercompany.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osuppliercompany.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        osuppliercompany.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osuppliercompany.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osuppliercompany.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        osuppliercompany.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osuppliercompany.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osuppliercompany.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osuppliercompany.preferredtransportid = reader["preferredtransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preferredtransportid"]);

                        osuppliercompany.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                        osuppliercompany.tdsgroupid = reader["tdsgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsgroupid"]);
                        osuppliercompany.attachment_json = reader["attachment"] == DBNull.Value ? "null" : reader["attachment"].ToString();
                        osuppliercompany.addresslist_json = reader["addresslist"] == DBNull.Value ? "null" : reader["addresslist"].ToString();

                        if (_suppliercompany.isprint)
                        {
                            osuppliercompany.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                            osuppliercompany.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                            osuppliercompany.statename = reader["statename"] == DBNull.Value ? "" : reader["statename"].ToString();
                            osuppliercompany.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                            osuppliercompany.contactnumber = reader["contactnumber"] == DBNull.Value ? "" : reader["contactnumber"].ToString();
                            osuppliercompany.companycontactno = reader["contactnumber"] == DBNull.Value ? "" : reader["contactnumber"].ToString();
                            osuppliercompany.companyemailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        }
                        lst.Add(osuppliercompany);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<SupplierCompanyHeader> SearchHeader(SuppliercompanyCreteria _suppliercompany)
        {
            List<SupplierCompanyHeader> lst = new List<SupplierCompanyHeader>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(@"Select Suppliercompany.id,supplierid,Suppliercompany.name,Suppliercompany.isinterstatesale,citylist.name city,tinanddate from Suppliercompany
                                                    left outer join referencelist citylist on citylist.id =  suppliercompany.cityid
                                                    where Suppliercompany.isactive=true", conn);

                if(_suppliercompany.rowtype>=0)
                {
                    command = GetCommand(@"Select Suppliercompany.id,supplierid,Suppliercompany.name,
                                           Suppliercompany.isinterstatesale,citylist.name city,tinanddate from Suppliercompany
                                           inner join supplier on supplier.id = supplierid 
                                           left outer join referencelist citylist on citylist.id =  suppliercompany.cityid
                                           where Suppliercompany.isactive=true and supplier.isactive=true and supplier.rowtype=@rowtype", conn);
                    command.Parameters.AddWithValue("@rowtype", _suppliercompany.rowtype);
                }

                if(_suppliercompany.ids != null && _suppliercompany.ids.Count>0)
                {
                    command.CommandText += " and suppliercompany.id in (" + string.Join(",", _suppliercompany.ids) + ")";
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SupplierCompanyHeader osuppliercompany = new SupplierCompanyHeader();
                        osuppliercompany.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osuppliercompany.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        osuppliercompany.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osuppliercompany.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        osuppliercompany.gst = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        osuppliercompany.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"].ToString());
                        lst.Add(osuppliercompany);
                    }
                }

                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst.ForEach(e => e.name = "*" + e.id.ToString());
                }
                else
                {
                    var duplicatenames = lst.GroupBy(e => e.name).Where(e => e.Count() > 1).ToList();
                    List<long> ids = new List<long>();
                    duplicatenames.ForEach(e =>
                    {
                        e.ToList().ForEach(x => x.name = x.name + " " + x.city);
                    });
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }


        public List<Suppliercompany> SearchAll(long _supplierid)
        {
            List<Suppliercompany> lst = new List<Suppliercompany>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Suppliercompany osuppliercompany = new Suppliercompany();
                        osuppliercompany.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osuppliercompany.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        osuppliercompany.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osuppliercompany.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        osuppliercompany.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"].ToString());
                        osuppliercompany.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"].ToString());
                        osuppliercompany.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        osuppliercompany.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        osuppliercompany.tallyledger = reader["tallyledger"] == DBNull.Value ? "" : reader["tallyledger"].ToString();
                        osuppliercompany.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"].ToString());
                        osuppliercompany.companycontactno = reader["companycontactno"] == DBNull.Value ? "" : reader["companycontactno"].ToString();
                        osuppliercompany.companyemailid = reader["companyemailid"] == DBNull.Value ? "" : reader["companyemailid"].ToString();
                        osuppliercompany.tinanddate = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        osuppliercompany.cstanddate = reader["cstanddate"] == DBNull.Value ? "" : reader["cstanddate"].ToString();
                        osuppliercompany.areacode = reader["areacode"] == DBNull.Value ? "" : reader["areacode"].ToString();
                        osuppliercompany.tan = reader["tan"] == DBNull.Value ? "" : reader["tan"].ToString();
                        osuppliercompany.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        osuppliercompany.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        osuppliercompany.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        osuppliercompany.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        osuppliercompany.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        osuppliercompany.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        osuppliercompany.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osuppliercompany.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osuppliercompany.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osuppliercompany.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        osuppliercompany.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osuppliercompany.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osuppliercompany.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osuppliercompany.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        osuppliercompany.preferredtransportid = reader["preferredtransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preferredtransportid"]);
                        osuppliercompany.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                        osuppliercompany.tdsgroupid = reader["tdsgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsgroupid"]);
                        osuppliercompany.attachment_json = reader["attachment"] == DBNull.Value ? "null" : reader["attachment"].ToString();
                        osuppliercompany.addresslist_json = reader["addresslist"] == DBNull.Value ? "null" : reader["addresslist"].ToString();

                        lst.Add(osuppliercompany);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        internal List<SupplierProduct> SearchProductHeader()
        {
            string query = @"select distinct supplierid,productid from lrinvoiceitems
inner join lrinvoice on lrinvoice.id = lrinvoiceid
inner join suppliercompany on suppliercompany.id = suppliercompanyid";
            List<SupplierProduct> result = new List<SupplierProduct>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while(reader.Read())
                {
                    long supplierid = reader.IsDBNull(0) ? 0: reader.GetInt64(0);
                    long productid = reader.IsDBNull(0) ? 0 : reader.GetInt64(1);
                    if(!result.Exists(e=>e.id == supplierid))
                    {
                        result.Add(new SupplierProduct()
                        {
                            id = supplierid,
                            child = new List<long>()
                        });
                    }
                    result.Find(e => e.id == supplierid).child.Add(productid);
                }
            }
            return result;
        }

        public Suppliercompany Insert(Suppliercompany oSuppliercompany)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);


                    Insert(transaction, conn, oSuppliercompany);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oSuppliercompany;
        }

        public Suppliercompany Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Suppliercompany oSuppliercompany)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.supplierid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.address == null ? "" : oSuppliercompany.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.pincode == null ? "" : oSuppliercompany.pincode;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.contactperson == null ? "" : oSuppliercompany.contactperson;
            command.Parameters.Add("tallyledger", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.tallyledger == null ? "" : oSuppliercompany.tallyledger;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.agentid;
            command.Parameters.Add("companycontactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.companycontactno == null ? "" : oSuppliercompany.companycontactno;
            command.Parameters.Add("companyemailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.companyemailid == null ? "" : oSuppliercompany.companyemailid;
            command.Parameters.Add("tinanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.tinanddate == null ? "" : oSuppliercompany.tinanddate;
            command.Parameters.Add("cstanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.cstanddate == null ? "" : oSuppliercompany.cstanddate;
            command.Parameters.Add("areacode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.areacode == null ? "" : oSuppliercompany.areacode;
            command.Parameters.Add("tan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.tan == null ? "" : oSuppliercompany.tan;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.pan == null ? "" : oSuppliercompany.pan;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankaccountname == null ? "" : oSuppliercompany.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankaccountno == null ? "" : oSuppliercompany.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankifci == null ? "" : oSuppliercompany.bankifci;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankbranch == null ? "" : oSuppliercompany.bankbranch;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.bankid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliercompany.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliercompany.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.name == null ? "" : oSuppliercompany.name;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliercompany.isinterstatesale;
            command.Parameters.Add("preferredtransportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.preferredtransportid;
            command.Parameters.Add("msmeno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.msmeno == null ? "" : oSuppliercompany.msmeno;
            command.Parameters.Add("tdsgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.tdsgroupid;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliercompany.attachment == null ? "null" : oSuppliercompany.attachment_json;
            command.Parameters.Add("addresslist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliercompany.addresslist == null ? "null" : oSuppliercompany.addresslist_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSuppliercompany.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSuppliercompany;
        }

        public Suppliercompany Update(Suppliercompany oSuppliercompany)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSuppliercompany);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oSuppliercompany;
        }

        public Suppliercompany Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Suppliercompany oSuppliercompany)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.supplierid;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.address == null ? "" : oSuppliercompany.address;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.stateid;
            command.Parameters.Add("countryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.countryid;
            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.pincode == null ? "" : oSuppliercompany.pincode;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.contactperson == null ? "" : oSuppliercompany.contactperson;
            command.Parameters.Add("tallyledger", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.tallyledger == null ? "" : oSuppliercompany.tallyledger;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.agentid;
            command.Parameters.Add("companycontactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.companycontactno == null ? "" : oSuppliercompany.companycontactno;
            command.Parameters.Add("companyemailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.companyemailid == null ? "" : oSuppliercompany.companyemailid;
            command.Parameters.Add("tinanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.tinanddate == null ? "" : oSuppliercompany.tinanddate;
            command.Parameters.Add("cstanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.cstanddate == null ? "" : oSuppliercompany.cstanddate;
            command.Parameters.Add("areacode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.areacode == null ? "" : oSuppliercompany.areacode;
            command.Parameters.Add("tan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.tan == null ? "" : oSuppliercompany.tan;
            command.Parameters.Add("pan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.pan == null ? "" : oSuppliercompany.pan;
            command.Parameters.Add("bankaccountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankaccountname == null ? "" : oSuppliercompany.bankaccountname;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankaccountno == null ? "" : oSuppliercompany.bankaccountno;
            command.Parameters.Add("bankifci", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankifci == null ? "" : oSuppliercompany.bankifci;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.bankbranch == null ? "" : oSuppliercompany.bankbranch;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.bankid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliercompany.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliercompany.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSuppliercompany.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.name == null ? "" : oSuppliercompany.name;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliercompany.isinterstatesale;
            command.Parameters.Add("preferredtransportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.preferredtransportid;
            command.Parameters.Add("msmeno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSuppliercompany.msmeno == null ? "" : oSuppliercompany.msmeno;
            command.Parameters.Add("tdsgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.tdsgroupid;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliercompany.attachment == null ? "null" : oSuppliercompany.attachment_json;
            command.Parameters.Add("addresslist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSuppliercompany.addresslist == null ? "null" : oSuppliercompany.addresslist_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSuppliercompany.version = Convert.ToInt32(reader[0]);
                }
                else
                {
                    throw new Exception("version missmatch");
                }
            }

            return oSuppliercompany;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Suppliercompany> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.supplierid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Suppliercompany oSuppliercompany)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            Delete(transaction, conn, oSuppliercompany);
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Suppliercompany oSuppliercompany)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSuppliercompany.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSuppliercompany.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSuppliercompany.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long supplierid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supplierid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
