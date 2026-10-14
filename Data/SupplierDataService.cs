using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyGST;

namespace InfyPOS.Data
{
    public class SupplierDataService : SAASData
    {
        const string SQL_SearchHeader = @"select supplier.id,supplier.code, supplier.name, supportsourcesupplier,supportpo,creditdays,soldpercentage,
                                          supplier.minpurdisctpercentage,agentid,maxmargin,minmargin,locationid,rowtype,limit_billvaluetaxable,limit_billvalue,
                                          tinanddate gstno,suppliercompany.id suppliercompanyid,suppliercompany.isinterstatesale,citylist.name city, supplier.activestatus
                                          from supplier
                                          inner join suppliercompany on suppliercompany.supplierid = supplier.id
                                          left outer join referencelist citylist on citylist.id =  suppliercompany.cityid
                                          where supplier.isactive=true";


        const string SQL_SearchLedger = @"select supplier.id,supplier.name,tinanddate gstno,suppliercompany.address,
                                          suppliercompany.cityid,suppliercompany.stateid,citylist.name city, supplier.activestatus from supplier
                                          inner join suppliercompany on suppliercompany.supplierid = supplier.id
                                          left outer join referencelist citylist on citylist.id =  suppliercompany.cityid
                                          where supplier.isactive=true";

        const string SQL_Search = @"select supplier.id, supplier.name, supplier.contactperson, supplier.contactnumber, limit_billvaluetaxable,
                supplier.emailid,supplier.creditdays, supplier.locationid, supplier.isactive, supportsourcesupplier,rowtype,limit_billvalue,
                supplier.createdby, supplier.modifiedby, supplier.version, supplier.createdon, supplier.modifiedon,supplier.minmargin,supplier.maxmargin,
                supplier.product,supplier.brand,supplier.code,supplier.discountpercentage,supplier.isunregistered,supplier.remarks,suppliergroupid,
                supplier.activatedon,supplier.deactivatedon, suppliercompany.pincode, suppliercompany.agentid as agentid,supportpo,suppliercompany.cityid,
                suppliercompany.isinterstatesale,autopo_lead,autopo_rate, suppliercompany.address, city.name cityname,tinanddate,suppliercompany.preferredtransportid,
                closed,asupplierid,suppliercompany.id suppliercompanyid, businessgroupid, buyergroupid, deliverylocationid,  soldpercentage,
                msmeno, tdsgroupid, addresslist, attachment, supplier.activestatus,minpurdisctpercentage,intrateagrmnt,intrateagrmntdays,suppliercompany.name companyname 
                from supplier
                left outer join suppliercompany on suppliercompany.supplierid = supplier.id and suppliercompany.isactive = true
                left outer join referencelist city on city.id = suppliercompany.cityid 
                where supplier.id=@id";

        public SupplierFilterConfig FilterConfig(string id)
        {
            if (string.IsNullOrEmpty(id)) return new SupplierFilterConfig();
            var supplier = Search(new SupplierCreteria()
            {
                id = long.Parse(id)
            }).First();
            SupplierFilterConfig result = new SupplierFilterConfig();
            result.products = supplier.product == null ? null : supplier.product.ConvertAll(e => e.id);
            result.brands = supplier.brand == null ? null : supplier.brand.ConvertAll(e => e.id);
            return result;
        }

        const string SQL_SearchbyCity = @"select supplier.id, supplier.name, supplier.contactperson, supplier.contactnumber,limit_billvaluetaxable, 
                        emailid,creditdays, suppliercompany.cityid, supplier.locationid, suppliercompany.agentid,rowtype,supplier.minmargin,supplier.maxmargin, 
                        supplier.isactive, supplier.createdby, supplier.brand, supplier.supportpo, supplier.product, supplier.code,limit_billvalue,
                        supplier.discountpercentage, supplier.isunregistered, supplier.activatedon, supplier.deactivatedon,supplier.remarks,soldpercentage,
                            supplier.modifiedby, supplier.version, supplier.createdon, supplier.modifiedon, supportsourcesupplier,suppliergroupid,closed,asupplierid,
                            suppliercompany.isinterstatesale,autopo_lead,autopo_rate, suppliercompany.pincode, city.name cityname,suppliercompany.address, tinanddate ,
                            suppliercompany.preferredtransportid,suppliercompany.id suppliercompanyid, businessgroupid, buyergroupid, deliverylocationid,
                            msmeno, tdsgroupid, addresslist, attachment, supplier.activestatus,minpurdisctpercentage,intrateagrmnt,intrateagrmntdays
                            from supplier inner join suppliercompany on suppliercompany.supplierid = supplier.id
                            left outer join referencelist city on city.id = suppliercompany.cityid 
                            where supplier.isactive=true and suppliercompany.isactive=true ";

        const string SQL_SearchSupplierDetails = @"select supplier.id, supplier.name, supplier.contactperson, supplier.contactnumber, 
supplier.emailid,supplier.creditdays, supplier.locationid, supplier.isactive, supportsourcesupplier,suppliergroupid,limit_billvaluetaxable,
supplier.createdby, supplier.modifiedby, supplier.version, supplier.createdon, supplier.modifiedon,supplier.minmargin,supplier.maxmargin,
supplier.product,supplier.brand,supplier.code,supplier.discountpercentage,supplier.isunregistered,supplier.remarks,
supplier.activatedon,supplier.deactivatedon, suppliercompany.agentid as agentid,supportpo,rowtype,limit_billvalue,soldpercentage,
suppliercompany.cityid,suppliercompany.address,suppliercompany.stateid,suppliercompany.countryid,suppliercompany.pincode,
suppliercompany.agentid,suppliercompany.companycontactno,suppliercompany.companyemailid,suppliercompany.tinanddate,closed,asupplierid,
suppliercompany.tan,suppliercompany.pan,bank.name bankname,suppliercompany.bankid,suppliercompany.bankbranch,suppliercompany.bankaccountname,suppliercompany.bankaccountno,suppliercompany.bankifci, suppliercompany.preferredtransportid,
createdemployee.name createdemployeename, modifiedemployee.name modifiedemployeename,autopo_lead,autopo_rate,intrateagrmntdays,
businessgroupid, buyergroupid, deliverylocationid, msmeno, tdsgroupid, addresslist, attachment, supplier.activestatus,minpurdisctpercentage,intrateagrmnt
from supplier
left outer join suppliercompany on suppliercompany.supplierid = supplier.id and suppliercompany.isactive = true
left outer join users createdusers on createdusers.id = supplier.createdby
left outer join employee createdemployee on createdemployee.id = createdusers.employeeid
left outer join users modifiedusers on modifiedusers.id = supplier.modifiedby
left outer join employee modifiedemployee on modifiedemployee.id = modifiedusers.employeeid
left outer join referencelist bank on bank.id = suppliercompany.bankid
where supplier.isactive = true";

        const string SQL_Insert = @"INSERT INTO public.supplier(name, contactperson, creditdays,contactnumber, emailid, locationid, isactive, createdby, modifiedby, version, createdon, modifiedon,product,brand,code,discountpercentage,isunregistered,activatedon,deactivatedon,supportpo, supportsourcesupplier,suppliergroupid,remarks,minmargin,maxmargin,rowtype,limit_billvaluetaxable,limit_billvalue,closed,asupplierid,autopo_lead,autopo_rate,businessgroupid,buyergroupid,deliverylocationid,activestatus,minpurdisctpercentage,intrateagrmnt,intrateagrmntdays,internalcustomerid,soldpercentage) 
                                    VALUES(@name, @contactperson,@creditdays, @contactnumber, @emailid, @locationid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@product,@brand,@code,@discountpercentage,@isunregistered,@activatedon,@deactivatedon,@supportpo, @supportsourcesupplier,@suppliergroupid,@remarks,@minmargin,@maxmargin,@rowtype,@limit_billvaluetaxable,@limit_billvalue,@closed,@asupplierid,@autopo_lead,@autopo_rate,@businessgroupid,@buyergroupid,@deliverylocationid,@activestatus,@minpurdisctpercentage,@intrateagrmnt,@intrateagrmntdays,@internalcustomerid,@soldpercentage) RETURNING id";


        const string SQL_Update = @"UPDATE public.supplier SET name=@name,contactperson=@contactperson,creditdays=@creditdays,supportpo=@supportpo, minmargin=@minmargin,maxmargin=@maxmargin,rowtype=@rowtype,soldpercentage=@soldpercentage,
                                    contactnumber=@contactnumber, emailid=@emailid, locationid=@locationid, modifiedby=@modifiedby, supportsourcesupplier =@supportsourcesupplier,suppliergroupid=@suppliergroupid,intrateagrmntdays=@intrateagrmntdays,
                                    version=@version+1, modifiedon=@modifiedon,product=@product,brand=@brand,code=@code,discountpercentage=@discountpercentage,isunregistered=@isunregistered,activatedon=@activatedon,intrateagrmnt=@intrateagrmnt,
                                    deactivatedon=@deactivatedon,isactive=@isactive,remarks=@remarks,limit_billvalue=@limit_billvalue,limit_billvaluetaxable=@limit_billvaluetaxable,closed=@closed,minpurdisctpercentage=@minpurdisctpercentage,
                                    asupplierid=@asupplierid,autopo_lead=@autopo_lead,autopo_rate=@autopo_rate,businessgroupid=@businessgroupid,buyergroupid=@buyergroupid,deliverylocationid=@deliverylocationid,activestatus=@activestatus 
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.supplier set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Supplier> Search(SupplierCreteria _supplier)
        {
            List<Supplier> lst = new List<Supplier>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_supplier.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplier.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();

                    if (_supplier.cityid == 0 && _supplier.agentid == 0)
                    {
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    }
                    else
                    {
                        qb.CurrentQuery = SQL_SearchbyCity;
                    }
                    if (_supplier.inactive == true)
                    {
                        //qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                        qb.AddParameter("supplier.isactive", false, "=");
                    }
                    else
                    {
                        qb.AddParameter("supplier.isactive", true, "=");
                    }
                    if (_supplier.hasgst)
                    {
                        qb.AddParameter("suppliercompany.tinanddate", "", " != ");
                    }
                    if (_supplier.closed == true)
                    {
                        //qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                        qb.AddParameter("supplier.closed", true, "=");
                    }
                    else
                    {
                        qb.AddParameter("supplier.closed", false, "=");
                    }
                    if (_supplier.isinterstatesale == true)
                    {
                        //qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                        qb.AddParameter("suppliercompany.isinterstatesale", true, "=");
                    }

                    if (_supplier.rowtype >= 0)
                        qb.AddParameter("supplier.rowtype", _supplier.rowtype, "=");
                    if (_supplier.activestatus > 0)
                        qb.AddParameter("supplier.activestatus", _supplier.activestatus, "=");

                    if (!string.IsNullOrEmpty(_supplier.name))
                    {
                        qb.AddParameter("supplier.name", "%" + _supplier.name + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_supplier.contactnumber))
                    {
                        qb.AddParameter("supplier.contactnumber", "%" + _supplier.contactnumber + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_supplier.tinanddate))
                    {
                        if (_supplier.tinanddate.Length == 15)
                            qb.AddParameter("suppliercompany.tinanddate", _supplier.tinanddate, " ilike ");
                        else
                            qb.AddParameter("suppliercompany.tinanddate", "%" + _supplier.tinanddate + "%", " ilike ");
                    }
                    if (_supplier.cityid > 0)
                        qb.AddParameter("suppliercompany.cityid", _supplier.cityid, "=");
                    if (_supplier.agentid > 0)
                        qb.AddParameter("suppliercompany.agentid", _supplier.agentid, "=");
                    if (!string.IsNullOrEmpty(_supplier.code))
                        qb.AddParameter("supplier.code", _supplier.code, "=");

                    if (_supplier.ids != null && _supplier.ids.Count > 0)
                        qb.AddParameter("supplier.id", "supplierids", _supplier.ids, " in ", "(" + string.Join(",", _supplier.ids) + ")");


                    qb.SortField = "supplier.name";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Supplier osupplier = new Supplier();
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        osupplier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osupplier.companyname = reader["companyname"] == DBNull.Value ? "" : reader["companyname"].ToString();
                        osupplier.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        osupplier.contactnumber = reader["contactnumber"] == DBNull.Value ? "" : reader["contactnumber"].ToString();
                        osupplier.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        osupplier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"].ToString());
                        osupplier.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        osupplier.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        osupplier.creditdays = reader["creditdays"] == DBNull.Value ? 0 : Convert.ToInt64(reader["creditdays"].ToString());
                        osupplier.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osupplier.supportsourcesupplier = reader["supportsourcesupplier"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportsourcesupplier"]);
                        osupplier.suppliergroupid = reader["suppliergroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliergroupid"].ToString());

                        osupplier.autopo_lead = reader["autopo_lead"] == DBNull.Value ? 0 : Convert.ToInt32(reader["autopo_lead"].ToString());
                        osupplier.autopo_rate = reader["autopo_rate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["autopo_rate"].ToString());

                        osupplier.supportpo = reader["supportpo"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportpo"]);
                        osupplier.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osupplier.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osupplier.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        osupplier.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osupplier.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osupplier.brand_json = reader["brand"] == DBNull.Value ? "null" : reader["brand"].ToString();
                        osupplier.product_json = reader["product"] == DBNull.Value ? "null" : reader["product"].ToString();
                        osupplier.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        osupplier.discountpercentage = reader["discountpercentage"] == DBNull.Value ? "" : reader["discountpercentage"].ToString();
                        osupplier.isunregistered = reader["isunregistered"] == DBNull.Value ? false : Convert.ToBoolean(reader["isunregistered"]);
                        osupplier.activatedon = reader["activatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activatedon"]);
                        osupplier.deactivatedon = reader["deactivatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deactivatedon"]);
                        osupplier.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        osupplier.tinanddate = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();
                        osupplier.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osupplier.preferredtransportid = reader["preferredtransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preferredtransportid"].ToString());
                        osupplier.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        osupplier.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        osupplier.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        osupplier.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();

                        osupplier.minpurdisctpercentage = reader["minpurdisctpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minpurdisctpercentage"]);
                        osupplier.intrateagrmnt = reader["intrateagrmnt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intrateagrmnt"]);

                        osupplier.intrateagrmntdays = reader["intrateagrmntdays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["intrateagrmntdays"]);

                        osupplier.minmargin = reader["minmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minmargin"]);
                        osupplier.maxmargin = reader["maxmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxmargin"]);
                        osupplier.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);
                        osupplier.soldpercentage = reader["soldpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldpercentage"]);
                        osupplier.limit_billvalue = reader["limit_billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["limit_billvalue"]);
                        osupplier.limit_billvaluetaxable = reader["limit_billvaluetaxable"] == DBNull.Value ? false : Convert.ToBoolean(reader["limit_billvaluetaxable"]);
                        osupplier.businessgroupid = reader["businessgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["businessgroupid"]);
                        osupplier.buyergroupid = reader["buyergroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["buyergroupid"]);
                        osupplier.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        osupplier.activestatus = reader["activestatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["activestatus"].ToString());

                        lst.Add(osupplier);
                    }
                }

                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst.ForEach(e => e.name = string.IsNullOrEmpty(e.code) ? e.id.ToString() : e.code);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            SuppliercompanyDataService subDataservice = new SuppliercompanyDataService() { Context = this.Context };
            subDataservice.Context = this.Context;
            if (_supplier.id > 0 && lst.Count == 1)
            {

                lst[0].companylist = subDataservice.Search(new SuppliercompanyCreteria() { supplierid = _supplier.id, isprint = true });
                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst[0].companylist.ForEach(e => e.name = string.IsNullOrEmpty(lst[0].code) ? e.id.ToString() : lst[0].code);
                }
            }
            if (_supplier.allsupplier)
            {
                foreach (var item in lst)
                {
                    item.companylist = subDataservice.SearchAll(item.id);
                }
            }
            return lst;
        }

        public List<SupplierDetails> SearchSupplierDetails(SupplierCreteria _supplier)
        {
            List<SupplierDetails> lst = new List<SupplierDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchSupplierDetails.Substring(0, SQL_SearchSupplierDetails.IndexOf("where"));
                qb.AddParameter("supplier.isactive", "isactive", true, "=");

                if (_supplier.fromdate != DateTime.MinValue)
                    qb.AddParameter("supplier.createdon", "datefrom", _supplier.fromdate.Date, ">=");

                if (_supplier.todate != DateTime.MinValue)
                    qb.AddParameter("supplier.createdon", "dateto", _supplier.todate.Date.AddDays(1), "<");

                if (_supplier.ids != null && _supplier.ids.Count > 0)
                    qb.AddParameter("supplier.id", "supplierids", _supplier.ids, " in ", "(" + string.Join(",", _supplier.ids) + ")");

                qb.SortField = "supplier.name";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SupplierDetails osupplier = new SupplierDetails();
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osupplier.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        osupplier.contactnumber = reader["contactnumber"] == DBNull.Value ? "" : reader["contactnumber"].ToString();
                        osupplier.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        osupplier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"].ToString());
                        osupplier.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        osupplier.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        osupplier.creditdays = reader["creditdays"] == DBNull.Value ? 0 : Convert.ToInt64(reader["creditdays"].ToString());
                        osupplier.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osupplier.supportsourcesupplier = reader["supportsourcesupplier"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportsourcesupplier"]);
                        osupplier.suppliergroupid = reader["suppliergroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliergroupid"].ToString());
                        osupplier.supportpo = reader["supportpo"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportpo"]);
                        osupplier.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osupplier.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osupplier.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        osupplier.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osupplier.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osupplier.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        osupplier.discountpercentage = reader["discountpercentage"] == DBNull.Value ? "" : reader["discountpercentage"].ToString();
                        osupplier.isunregistered = reader["isunregistered"] == DBNull.Value ? false : Convert.ToBoolean(reader["isunregistered"]);
                        osupplier.activatedon = reader["activatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activatedon"]);
                        osupplier.deactivatedon = reader["deactivatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deactivatedon"]);
                        osupplier.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osupplier.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        osupplier.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"].ToString());
                        osupplier.countryid = reader["countryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["countryid"].ToString());
                        osupplier.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();
                        osupplier.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        osupplier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"].ToString());
                        osupplier.companycontactno = reader["companycontactno"] == DBNull.Value ? "" : reader["companycontactno"].ToString();
                        osupplier.companyemailid = reader["companyemailid"] == DBNull.Value ? "" : reader["companyemailid"].ToString();
                        osupplier.tinanddate = reader["tinanddate"] == DBNull.Value ? "" : reader["tinanddate"].ToString();

                        osupplier.tan = reader["tan"] == DBNull.Value ? "" : reader["tan"].ToString();
                        osupplier.pan = reader["pan"] == DBNull.Value ? "" : reader["pan"].ToString();
                        osupplier.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                        osupplier.branchname = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        osupplier.bankaccountname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                        osupplier.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        osupplier.bankifci = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                        osupplier.preferredtransportid = reader["preferredtransportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preferredtransportid"].ToString());
                        osupplier.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        osupplier.minmargin = reader["minmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minmargin"]);
                        osupplier.maxmargin = reader["maxmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxmargin"]);
                        osupplier.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);
                        osupplier.soldpercentage = reader["soldpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldpercentage"]);
                        osupplier.limit_billvalue = reader["limit_billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["limit_billvalue"]);
                        osupplier.limit_billvaluetaxable = reader["limit_billvaluetaxable"] == DBNull.Value ? false : Convert.ToBoolean(reader["limit_billvaluetaxable"]);
                        osupplier.createdusername = reader["createdemployeename"] == DBNull.Value ? "" : reader["createdemployeename"].ToString();
                        osupplier.modifiedusername = reader["modifiedemployeename"] == DBNull.Value ? "" : reader["modifiedemployeename"].ToString();
                        osupplier.autopo_lead = reader["autopo_lead"] == DBNull.Value ? 0 : Convert.ToInt32(reader["autopo_lead"].ToString());
                        osupplier.autopo_rate = reader["autopo_rate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["autopo_rate"].ToString());
                        osupplier.minpurdisctpercentage = reader["minpurdisctpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minpurdisctpercentage"]);
                        osupplier.intrateagrmnt = reader["intrateagrmnt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["intrateagrmnt"]);
                        osupplier.intrateagrmntdays = reader["intrateagrmntdays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["intrateagrmntdays"]);
                        osupplier.businessgroupid = reader["businessgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["businessgroupid"]);
                        osupplier.buyergroupid = reader["buyergroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["buyergroupid"]);
                        osupplier.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        osupplier.tdsgroupid = reader["tdsgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsgroupid"]);
                        osupplier.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                        osupplier.activestatus = reader["activestatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["activestatus"].ToString());

                        lst.Add(osupplier);
                    }
                }
                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst.ForEach(e => e.name = string.IsNullOrEmpty(e.code) ? e.id.ToString() : e.code);
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
        public List<SupplierLedger> SupplierLedger(SupplierCreteria _supplier)
        {
            List<SupplierLedger> lst = new List<SupplierLedger>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(SQL_SearchLedger, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SupplierLedger osupplier = new SupplierLedger();
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osupplier.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        osupplier.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        osupplier.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        osupplier.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        osupplier.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        lst.Add(osupplier);
                    }
                }

                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst.ForEach(e => e.name = "*" + e.id.ToString());
                }
                else
                {
                    var duplicatenames = lst.GroupBy(e => e.name).Where(e => e.Count() > 1).ToList();
                    duplicatenames.ForEach(e =>
                    {
                        var duplicatecity = e.ToList().GroupBy(x => x.city).Where(y => y.Count() > 1).ToList();
                        if (duplicatecity.Count > 0)
                        {
                            e.ToList().ForEach(x => x.name = x.name + " - " + x.city + "(" + x.address + ")");
                        }
                        else
                        {
                            e.ToList().ForEach(x => x.name = x.name + " - " + x.city);
                        }
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
        public List<SupplierHeader> SearchHeaders(SupplierCreteria _supplier)
        {
            List<SupplierHeader> lst = new List<SupplierHeader>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchHeader.Substring(0, SQL_SearchHeader.IndexOf("where"));
                qb.AddParameter("supplier.isactive", "isactive", true, "=");
                qb.AddParameter("supplier.activestatus", "activestatus", 2, "=");
                if (_supplier.id > 0)
                    qb.AddParameter("supplier.id", "id", _supplier.id, "=");

                if (_supplier.rowtype >= 0)
                    qb.AddParameter("supplier.rowtype", "rowtype", _supplier.rowtype, "=");

                NpgsqlCommand command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SupplierHeader osupplier = new SupplierHeader();
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.maxmargin = reader["maxmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxmargin"]);
                        osupplier.minmargin = reader["minmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minmargin"]);
                        osupplier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplier.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        osupplier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        osupplier.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        osupplier.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        osupplier.creditdays = reader["creditdays"] == DBNull.Value ? 0 : Convert.ToInt64(reader["creditdays"].ToString());
                        osupplier.supportsourcesupplier = reader["supportsourcesupplier"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportsourcesupplier"]);
                        osupplier.supportpo = reader["supportpo"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportpo"]);
                        osupplier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        osupplier.companyid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        osupplier.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);
                        osupplier.soldpercentage = reader["soldpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soldpercentage"]);
                        osupplier.limit_billvalue = reader["limit_billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["limit_billvalue"]);
                        osupplier.minpurdisctpercentage = reader["minpurdisctpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minpurdisctpercentage"]);
                        osupplier.limit_billvaluetaxable = reader["limit_billvaluetaxable"] == DBNull.Value ? false : Convert.ToBoolean(reader["limit_billvaluetaxable"]);
                        osupplier.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        osupplier.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        lst.Add(osupplier);
                    }
                }
                if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                {
                    lst.ForEach(e => e.name = "*" + e.id.ToString());
                }
                else if (!_supplier.supplierwithcity)
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

        private bool UniqueValidation(Supplier oSupplier)
        {
            if (isMigration || string.IsNullOrEmpty(oSupplier.name)) return false;

            string _Query = @"select suppliercompany.id from suppliercompany
                              inner join supplier on supplier.id = suppliercompany.supplierid and supplier.isactive=true
                              where suppliercompany.isactive=true and suppliercompany.cityid=@cityid and 
                              lower(suppliercompany.name)=@suppliercompanyname and lower(supplier.name)=@suppliername and 
                              suppliercompany.id!=@id ";

            if (string.IsNullOrEmpty(oSupplier.companylist[0].tinanddate))
                oSupplier.companylist[0].tinanddate = "";

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportsuppliergstvalidation)
            {
                if (!string.IsNullOrEmpty(oSupplier.companylist[0].tinanddate))
                {
                    _Query = @"select suppliercompany.id from suppliercompany
                              inner join supplier on supplier.id = suppliercompany.supplierid and supplier.isactive=true
                              where suppliercompany.isactive=true 
                                and lower(suppliercompany.tinanddate)=@tinanddate and suppliercompany.id!=@id ";
                }
            }

            long suppliercompanyid = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.companylist[0].id;
                command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.companylist[0].cityid;
                command.Parameters.Add("suppliercompanyname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.companylist[0].name.ToLower();
                command.Parameters.Add("suppliername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.name.ToLower();
                command.Parameters.Add("tinanddate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.companylist[0].tinanddate.ToLower();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliercompanyid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]);
                    }
                }
                conn.Close();
            }

            return suppliercompanyid > 0;
        }

        public bool isMigration = false;
        public Supplier Insert(Supplier oSupplier)
        {
            if (UniqueValidation(oSupplier))
            {
                oSupplier.ErrorMessage = "Duplicate Supplier Name/GST Exists. Please verify active and inactive suppliers";
                return oSupplier;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("supplier.code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (string.IsNullOrEmpty(oSupplier.code))
                        oSupplier.code = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oSupplier);
                    SuppliercompanyDataService companydataservice = new SuppliercompanyDataService() { Context = this.Context };
                    companydataservice.Context = this.Context;
                    if (oSupplier.companylist != null)
                    {
                        foreach (var item in oSupplier.companylist)
                        {
                            item.supplierid = oSupplier.id;
                            companydataservice.Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return oSupplier;
        }

        public Supplier Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplier oSupplier)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.name == null ? "" : oSupplier.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.contactperson == null ? "" : oSupplier.contactperson;
            command.Parameters.Add("contactnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.contactnumber == null ? "" : oSupplier.contactnumber;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.emailid == null ? "" : oSupplier.emailid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.locationid;
            command.Parameters.Add("creditdays", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.creditdays;
            command.Parameters.Add("autopo_lead", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.autopo_lead;
            command.Parameters.Add("autopo_rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.autopo_rate;
            command.Parameters.Add("suppliergroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.suppliergroupid;
            command.Parameters.Add("internalcustomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.internalcustomerid;

            command.Parameters.Add("minpurdisctpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.minpurdisctpercentage;
            command.Parameters.Add("intrateagrmnt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.intrateagrmnt;
            command.Parameters.Add("intrateagrmntdays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.intrateagrmntdays;

            command.Parameters.Add("minmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.minmargin;
            command.Parameters.Add("maxmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.maxmargin;
            command.Parameters.Add("rowtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.rowtype;
            command.Parameters.Add("closed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.closed;
            command.Parameters.Add("asupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.asupplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config == null || Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_supplier == false) ? oSupplier.isactive : false;
            command.Parameters.Add("supportpo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.supportpo;
            command.Parameters.Add("supportsourcesupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.supportsourcesupplier;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("brand", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplier.brand_json == null ? "null" : oSupplier.brand_json;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplier.product_json == null ? "null" : oSupplier.product_json;
            command.Parameters.Add("isunregistered", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.isunregistered;
            command.Parameters.Add("activatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplier.activatedon;
            command.Parameters.Add("deactivatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplier.deactivatedon;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.code == null ? "" : oSupplier.code;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.discountpercentage == null ? "" : oSupplier.discountpercentage;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.remarks == null ? "" : oSupplier.remarks;
            command.Parameters.Add("limit_billvaluetaxable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.limit_billvaluetaxable;
            command.Parameters.Add("limit_billvalue", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.limit_billvalue;
            command.Parameters.Add("soldpercentage", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.soldpercentage;
            command.Parameters.Add("businessgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.businessgroupid;
            command.Parameters.Add("buyergroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.buyergroupid;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.deliverylocationid;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config == null || Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_supplier == false) ? 2 : 1;
            // activestatus = 1 - Unapproved, 2 - Approved, 3 - Deactivated

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplier.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_supplier)
            {
                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Supplier,
                        oSupplier.id, string.Format("SUPPLIER CREATION REQUEST - {0} ", oSupplier.name),
                        Context.UserID, 0, transaction, Context);
            }
            return oSupplier;
        }

        public Supplier Update(Supplier oSupplier)
        {
            if (UniqueValidation(oSupplier))
            {
                oSupplier.ErrorMessage = "Duplicate Supplier Name/GST Exists. Please verify active and inactive suppliers";
                return oSupplier;
            }

            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {

                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSupplier);
                    oSupplier.companylist.ForEach(e => e.isactive = oSupplier.isactive);

                    SuppliercompanyDataService sundata = new SuppliercompanyDataService() { Context = this.Context };
                    sundata.Context = this.Context;
                    sundata.Update(transaction, conn, oSupplier.companylist, oSupplier.id);
                    transaction.Commit();
                    conn.Close();
                }
                catch (Exception exp)
                {

                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                }
            }

            return oSupplier;
        }

        public Supplier Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplier oSupplier)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.name == null ? "" : oSupplier.name;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.contactperson == null ? "" : oSupplier.contactperson;
            command.Parameters.Add("contactnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.contactnumber == null ? "" : oSupplier.contactnumber;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.emailid == null ? "" : oSupplier.emailid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.locationid;
            command.Parameters.Add("minmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.minmargin;
            command.Parameters.Add("maxmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.maxmargin;
            command.Parameters.Add("rowtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.rowtype;
            command.Parameters.Add("creditdays", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.creditdays;

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config == null
                || Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_supplier == false)
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.isactive;
            else if (oSupplier.activestatus == 1)
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            else
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.isactive;

            command.Parameters.Add("closed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.closed;
            command.Parameters.Add("asupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.asupplierid;
            command.Parameters.Add("autopo_lead", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.autopo_lead;
            command.Parameters.Add("autopo_rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.autopo_rate;
            command.Parameters.Add("minpurdisctpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.minpurdisctpercentage;
            command.Parameters.Add("intrateagrmnt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSupplier.intrateagrmnt;
            command.Parameters.Add("intrateagrmntdays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.intrateagrmntdays;

            command.Parameters.Add("supportpo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.supportpo;
            command.Parameters.Add("supportsourcesupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.supportsourcesupplier;
            command.Parameters.Add("suppliergroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.suppliergroupid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplier.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("brand", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplier.brand_json == null ? "null" : oSupplier.brand_json;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplier.product_json == null ? "null" : oSupplier.product_json;
            command.Parameters.Add("isunregistered", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.isunregistered;
            command.Parameters.Add("activatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplier.activatedon;
            command.Parameters.Add("deactivatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplier.deactivatedon;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.code == null ? "" : oSupplier.code;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.discountpercentage == null ? "" : oSupplier.discountpercentage;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplier.remarks == null ? "" : oSupplier.remarks;
            command.Parameters.Add("limit_billvaluetaxable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.limit_billvaluetaxable;
            command.Parameters.Add("limit_billvalue", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.limit_billvalue;
            command.Parameters.Add("soldpercentage", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.soldpercentage;
            command.Parameters.Add("businessgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.businessgroupid;
            command.Parameters.Add("buyergroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.buyergroupid;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.deliverylocationid;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.activestatus;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplier.version = Convert.ToInt32(reader[0]);
                }
                else
                {
                    throw new Exception("version missmatch");
                }
            }
            return oSupplier;
        }

        internal GSTPortal.Taxpayer SearchGST(string gstno)
        {
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());
            if (companylist.Exists(e => !string.IsNullOrEmpty(e.gstaccesskey) && !string.IsNullOrEmpty(e.gstno)))
            {
                var company = companylist.First(e => !string.IsNullOrEmpty(e.gstaccesskey) && !string.IsNullOrEmpty(e.gstno));
                GSTPortal gSTPortal = new GSTPortal();
                gSTPortal.Initalize(
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.gstserver,
                company.id,
                company.gstno,
                company.gstusername,
                company.gstaccesskey);
                return gSTPortal.SearchGSTN(gstno).Result;
            }
            else
            {
                throw new Exception("You don't have permission to access Quanto GST");
            }
        }

        internal bool Marge(MergeSupplierList item)
        {
            Supplier _newitem = null;
            if (item.id == 0)
            {
                item.ErrorMessage = "There is no Item selected as Merge To";
                return false;
            }
            if (item.deletelist == null || item.deletelist.Count == 0)
            {
                item.ErrorMessage = "Please select the items";
                return false;
            }
            if (item.deletelist.Count > 2)
            {
                item.ErrorMessage = "Please merge one supplier at a time. its a long running process.";
                return false;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                if (!item.deletelist.Exists(e => e.id == item.id))
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                if (item.deletelist.Exists(e => e.id == item.id))
                    _newitem = item.deletelist.Find(e => e.id == item.id);

                if (_newitem == null)
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                SuppliercompanyDataService companydata = new SuppliercompanyDataService() { Context = this.Context };
                List<Suppliercompany> lstsuppliercomp = companydata.SearchAll(item.id);
                if (lstsuppliercomp == null || lstsuppliercomp.Count == 0 || lstsuppliercomp.Count > 1)
                {
                    item.ErrorMessage = "Supplier company missing/mismatch.";
                    return false;
                }
                long newsuppliercompanyid = lstsuppliercomp[0].id;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    item.deletelist.RemoveAll(e => e.id == item.id);
                    foreach (var ditem in item.deletelist)
                    {
                        if (!Delete(transaction, conn, ditem))
                            throw new Exception("Error occured.");

                        SuppliercompanyDataService sundata = new SuppliercompanyDataService() { Context = this.Context };
                        sundata.Context = this.Context;
                        sundata.DeleteAll(ditem.id, Context.UserID, conn, transaction);
                    }

                    string query = @"
update inbox set supplierid=@newid, suppliername=@newname where supplierid=@oldid;
update lrentry set supplierid=@newid where supplierid=@oldid;
update lrinvoice set lrsupplierid=@newid, suppliercompanyid=@newcompanyid where suppliercompanyid=@oldsuppliercompanyid;
update lrinvoicereturn set supplierid=@newid, suppliercompanyid=@newcompanyid where supplierid=@oldid;
update purchaseorder set supplierid=@newid where supplierid=@oldid;
update stock set supplierid=@newid where supplierid=@oldid;
update salablegoods set supplierid=@newid where supplierid=@oldid;
update supplierpayment set supplierid=@newid, suppliercompanyid=@newcompanyid where supplierid=@oldid;
update supplierpaymentinvoice set supplierid=@newid where supplierid=@oldid;";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("newcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = newsuppliercompanyid;
                    command.Parameters.Add("newname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _newitem.name;
                    command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("oldsuppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var ditem in item.deletelist)
                    {
                        command.Parameters["oldid"].Value = ditem.id;
                        command.Parameters["oldsuppliercompanyid"].Value = ditem.suppliercompanyid;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
            return true;
        }


        public bool Delete(Supplier oSupplier)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            hasSuccessCount = Delete(transaction, conn, oSupplier);

                            SuppliercompanyDataService sundata = new SuppliercompanyDataService() { Context = this.Context };
                            sundata.Context = this.Context;
                            sundata.DeleteAll(oSupplier.id, Context.UserID, conn, transaction);

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
            return hasSuccessCount;

        }

        internal bool ChangeStatus(ChangeAttributes item)
        {

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand("update supplier set activestatus=@activestatus,isactive=@isactive,statusby=@statusby,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                    command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.statusid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("statusby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.statusid == 2;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                    return successCount > 0;
                }
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplier oSupplier)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplier.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplier.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplier.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Supplier Insert(Company item)
        {
            Supplier customer = new Supplier();
            customer.name = item.name;
            customer.code = item.code;
            customer.isactive = true;
            customer.companylist = new List<Suppliercompany>();
            customer.contactnumber = item.contactno;
            customer.contactperson = item.contactperson;
            customer.companylist.Add(new Suppliercompany()
            {
                createdby = Context.UserID,
                modifiedby = Context.UserID,
                createdon = DateTime.Now,
                modifiedon = DateTime.Now,
                address = item.address,
                stateid = item.stateid,
                cityid = item.cityid,
                pincode = item.pincode,
                countryid = item.countryid,
                gst = item.gstno,
                code = item.code,
                name = item.name
            });
            customer.companyid = item.id;
            customer.createdby = Context.UserID;
            customer.modifiedby = Context.UserID;
            customer.createdon = DateTime.Now;
            customer.modifiedon = DateTime.Now;
            customer.address = item.address;
            customer.gstno = item.gstno;
            customer.cityid = item.cityid;
            return Insert(customer);
        }
    }

}
