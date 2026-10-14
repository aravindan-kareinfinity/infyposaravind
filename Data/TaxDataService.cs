using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TaxDataService : SAASData
    {
        const string SQL_Search = @"select code, tax.name as name, type, issalestax, ispurchasetax, tax.isactive as isactive, tax.createdby as createdby, 
                                    tax.modifiedby, tax.version,tax.id as id ,tax.createdon, tax.modifiedon as modifiedon, taxpercentage, taxorder, 
                                    taxlist,disable,taxsplit,employee.name employeename,hascess,cesspercentage from tax
                                    left outer join users on users.id=tax.createdby 
                                    left outer join employee on employee.id=users.employeeid where tax.id=@id";

        const string SQL_Insert = @"INSERT INTO public.tax(code, name, type, issalestax, ispurchasetax, isactive, createdby, modifiedby, version, createdon, modifiedon, taxpercentage, taxorder, taxlist,disable,taxsplit,hascess,cesspercentage) 
VALUES(@code, @name, @type, @issalestax, @ispurchasetax, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @taxpercentage, @taxorder, @taxlist,@disable,@taxsplit,@hascess,@cesspercentage) RETURNING id";

        const string SQL_Update = @"UPDATE public.tax SET code=@code, name=@name, type=@type, issalestax=@issalestax, ispurchasetax=@ispurchasetax, modifiedby=@modifiedby,taxsplit=@taxsplit, version=@version+1, modifiedon=@modifiedon, taxpercentage=@taxpercentage, 
    taxorder=@taxorder, taxlist=@taxlist,disable=@disable,hascess=@hascess,cesspercentage=@cesspercentage where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.tax set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Tax> Search(TaxCreteria _tax)
        {
            List<Tax> lst = new List<Tax>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tax.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("tax.isactive", true, "=");
                    if (_tax.ids != null && _tax.ids.Count > 0)
                    {
                        qb.AddInParamer("tax.id", string.Join<long>(",", _tax.ids));
                    }
                    if (_tax.ispurchasetax)
                        qb.AddParameter("tax.ispurchasetax", _tax.ispurchasetax, "=");
                    if (_tax.issalestax)
                        qb.AddParameter("tax.issalestax", _tax.issalestax, "=");
                    if (!string.IsNullOrEmpty(_tax.name))
                    {
                        qb.AddParameter("tax.name", "%" + _tax.name + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_tax.code))
                    {
                        qb.AddParameter("tax.code", "%" + _tax.code + "%", " ilike ");
                    }

                    if(_tax.type != 0)
                        qb.AddParameter("tax.type", _tax.type, "=");

                    if (_tax.taxpercentage != 0)
                        qb.AddParameter("taxpercentage", _tax.taxpercentage, "=");

                    if (_tax.fromdate.Year > 1900)
                        qb.AddParameter("tax.createdon", "fromdate", _tax.fromdate.Date, ">=");
                    if (_tax.todate.Year > 1900)
                        qb.AddParameter("tax.createdon", "todate", _tax.todate.Date.AddDays(1), "<"); command = qb.GetCommand(conn);

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tax.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tax otax = new Tax();
                        otax.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        otax.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        otax.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"].ToString());
                        otax.issalestax = reader["issalestax"] == DBNull.Value ? false : Convert.ToBoolean(reader["issalestax"].ToString());
                        otax.ispurchasetax = reader["ispurchasetax"] == DBNull.Value ? false : Convert.ToBoolean(reader["ispurchasetax"].ToString());
                        otax.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otax.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"].ToString());
                        otax.hascess = reader["hascess"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascess"]);
                        otax.disable = reader["disable"] == DBNull.Value ? false : Convert.ToBoolean(reader["disable"]);
                        otax.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otax.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otax.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        otax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otax.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otax.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otax.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        otax.taxorder = reader["taxorder"] == DBNull.Value ? 0 : Convert.ToInt32(reader["taxorder"].ToString());
                        otax.taxlist = reader["taxlist"] == DBNull.Value ? "" : reader["taxlist"].ToString();
                        otax.taxsplit_json = reader["taxsplit"] == DBNull.Value ? "null" : reader["taxsplit"].ToString();
                        otax.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        if (otax.taxsplit == null)
                        {
                            otax.taxsplit = new Tax.TaxSplit();
                        }
                        if (otax.taxsplit.items == null)
                        {
                            otax.taxsplit.items = new List<Tax.TaxSplit.Item>();
                        }
                        if (otax.taxsplit.items.Count == 0 && otax.taxpercentage > 0)
                        {
                            otax.taxsplit.items.Add(new Tax.TaxSplit.Item()
                            {
                                cgst = otax.taxpercentage / 2,
                                sgst = otax.taxpercentage / 2,
                                igst = otax.taxpercentage / 2
                            });
                        }
                        if(otax.taxpercentage<=0 &&
                            otax.taxsplit != null &&
                            otax.taxsplit.items != null &&
                            otax.taxsplit.items.Count>0)
                        {
                            otax.taxpercentage = otax.taxsplit.items.First().cgst + otax.taxsplit.items.First().sgst;
                        }
                        lst.Add(otax);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tax);
            }
            return lst;
        }

        public Tax Insert(Tax oTax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTax);


            }

            return oTax;
        }

        public Tax Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tax oTax)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTax.code == null ? "" : oTax.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTax.name == null ? "" : oTax.name;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.type;
            command.Parameters.Add("issalestax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.issalestax;
            command.Parameters.Add("ispurchasetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.ispurchasetax;
            command.Parameters.Add("cesspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTax.cesspercentage;
            command.Parameters.Add("hascess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.hascess;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.isactive;
            command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.disable;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTax.taxpercentage;
            command.Parameters.Add("taxorder", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.taxorder;
            command.Parameters.Add("taxlist", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTax.taxlist == null ? "" : oTax.taxlist;
            command.Parameters.Add("taxsplit", NpgsqlTypes.NpgsqlDbType.Json).Value = oTax.taxsplit == null ? "null" : oTax.taxsplit_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTax.id = Convert.ToInt64(reader[0]);
                }
            }
            return oTax;
        }

        internal MergeTaxList Merge(MergeTaxList item)
        {
            if (item.id == 0)
            {
                item.ErrorMessage = "There is no Item selected as Merge To";
                return item;
            }
            if (item.deletelist == null || item.deletelist.Count == 0)
            {
                item.ErrorMessage = "Please select the items";
                return item;
            }
            if (item.deletelist.Count > 5)
            {
                item.ErrorMessage = "Please max 5 tax at a time. its a long running process.";
                return item;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                if (!item.deletelist.Exists(e => e == item.id))
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return item;
                }
                
                TaxDataService companydata = new TaxDataService() { Context = this.Context };
                List<Tax> lstsuppliercomp = companydata.Search(new TaxCreteria() { id = item.id});
                if (lstsuppliercomp == null || lstsuppliercomp.Count == 0 || lstsuppliercomp.Count > 1)
                {
                    item.ErrorMessage = "Tax is missing/mismatch.";
                    return item;
                }
                lstsuppliercomp = companydata.Search(new TaxCreteria() { ids = item.deletelist });
                
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    item.deletelist.RemoveAll(e => e == item.id);
                    foreach (var ditem in lstsuppliercomp)
                    {
                        Delete(transaction, conn, ditem);
                    }

                    string query = @"


with alltaxlist
as
(
select displayname,cast(ledger.attributes->>'invstaxids' as json),
case when staxlist is null then '0' else cast(staxlist as varchar) end stax,
case when ptaxlist is null then '0' else cast(ptaxlist as varchar) end ptax
from ledger
LEFT JOIN LATERAL json_array_elements (cast(ledger.attributes->>'invstaxids' as json)) staxlist ON true
LEFT JOIN LATERAL json_array_elements (cast(ledger.attributes->>'invptaxids' as json)) ptaxlist ON true
where typeid=16
),pandstax as
(
select distinct cast(stax as bigint) taxid from alltaxlist
union all 
select distinct cast(ptax as bigint) taxid from alltaxlist
)

select id,name from tax
left outer join pandstax on pandstax.taxid = tax.id
where isactive=true and pandstax.taxid is null

drop table mergemasters
create table mergemasters
(
id bigint,
field varchar(150),
sourceid bigint,
destinationid bigint,
changes json,
createdby bigint,
createdon timestamp without time zone
)

insert into mergemasters(field,changes,sourceid,destinationid,createdby,createdon)
select 'billadditionalcharges.taxid' field, 
cast('['||STRING_AGG(cast(id as varchar),',')||']' as json) changes,
19 as sourceid,
20 as destinationid,
10 createdby,
current_date+current_time createdon
from billadditionalcharges where taxid=19


update billadditionalcharges taxid = @newtaxid  where taxid in ({0});
update billcreditnoteitems taxid = @newtaxid  where taxid in ({0});
update billitems taxid = @newtaxid  where taxid in ({0});
update billreturnitems taxid = @newtaxid  where taxid in ({0});
update bomconsumptionitems taxid = @newtaxid  where taxid in ({0});
update bominvoiceitems taxid = @newtaxid  where taxid in ({0});
update bominvoicereturnitems taxid = @newtaxid  where taxid in ({0});
update bominvoicetax taxid = @newtaxid  where taxid in ({0});
update bommaster taxid = @newtaxid  where taxid in ({0});
update bulkbillitems taxid = @newtaxid  where taxid in ({0});
update componentpoitems taxid = @newtaxid  where taxid in ({0});
update courier taxid = @newtaxid  where taxid in ({0});
update deathonarival taxid = @newtaxid  where taxid in ({0});
update deathonarrival taxid = @newtaxid  where taxid in ({0});
update jobworkbill taxid = @newtaxid  where taxid in ({0});
update jobworkbillgrnitems chargetaxid = @newtaxid  where chargetaxid in ({0});
update jobworkbillitems chargetaxid = @newtaxid  where chargetaxid in ({0});
update lrinvoiceitems ptaxid = @newtaxid  where ptaxid in ({0});
update lrinvoiceitems taxid = @newtaxid  where taxid in ({0});
update lrinvoicereturn freighttaxid = @newtaxid  where freighttaxid in ({0});
update lrinvoicereturnitems taxid = @newtaxid  where taxid in ({0});
update lrinvoicetax taxid = @newtaxid  where taxid in ({0});
update mfgdesign taxid = @newtaxid  where taxid in ({0});
update mfgjoborderitems taxid = @newtaxid  where taxid in ({0});
update mfgjobworkconfig taxid = @newtaxid  where taxid in ({0});
update mfgworkordergrnitems taxid = @newtaxid  where taxid in ({0});
update mfgworkorderitems taxid = @newtaxid  where taxid in ({0});
update pricedrop taxid = @newtaxid  where taxid in ({0});
update products cesstaxid = @newtaxid  where cesstaxid in ({0});
update products salestaxid = @newtaxid  where salestaxid in ({0});
update purchaseorderitems taxid = @newtaxid  where taxid in ({0});
update recharge taxid = @newtaxid  where taxid in ({0});
update salesorderitems taxid = @newtaxid  where taxid in ({0});
update transport taxid = @newtaxid  where taxid in ({0});";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("oldsuppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var ditem in item.deletelist)
                    {
                        command.Parameters["oldid"].Value = ditem;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return item;
            }
            return item;
        }
        public Tax Update(Tax oTax)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTax);


            }

            return oTax;
        }

        public Tax Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tax oTax)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTax.code == null ? "" : oTax.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTax.name == null ? "" : oTax.name;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.type;
            command.Parameters.Add("issalestax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.issalestax;
            command.Parameters.Add("ispurchasetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.ispurchasetax;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTax.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTax.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTax.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTax.taxpercentage;
            command.Parameters.Add("taxorder", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.taxorder;
            command.Parameters.Add("taxlist", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTax.taxlist == null ? "0" : oTax.taxlist;
            command.Parameters.Add("disable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.disable;
            command.Parameters.Add("taxsplit", NpgsqlTypes.NpgsqlDbType.Json).Value = oTax.taxsplit == null ? "null" : oTax.taxsplit_json;
            command.Parameters.Add("cesspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTax.cesspercentage;
            command.Parameters.Add("hascess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.hascess;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTax.version = Convert.ToInt32(reader[0]);
                }
            }

            return oTax;
        }
        public SortedList<long, Tax> TaxProducts(List<long> productids)
        {
            return TaxProducts(productids, false);
        }
        public SortedList<long, Tax> TaxProducts(List<long> productids,bool forpurchase)
        {
            SortedList<long, Tax> lst = new SortedList<long, Tax>();
            if (productids == null || productids.Count == 0) return lst;

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(string.Format(@"select products.id productid,tax.id,tax.type,taxsplit,taxpercentage,products.hascess,cesspercentage from products
                inner join tax on tax.id = products.salestaxid
                where products.id in ({0})", string.Join(",", productids)), conn);
                if(productids.Count == 0)
                {
                    command = GetCommand(@"select products.id productid,tax.id,tax.type,taxsplit,taxpercentage,products.hascess,cesspercentage from products
                    inner join tax on tax.id = products.salestaxid
                    where products.isactive=true", conn);
                }

                if(forpurchase)
                {
                    command.CommandText = command.CommandText.Replace("salestaxid", "purchasetaxid");
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        Tax otax = new Tax();
                        otax.taxsplit_json = reader["taxsplit"] == DBNull.Value ? "null" : reader["taxsplit"].ToString();
                        otax.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"].ToString());
                        otax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otax.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        otax.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"].ToString());
                        otax.hascess = reader["hascess"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascess"]);


                        if (otax.taxsplit == null)
                            otax.taxsplit = new Tax.TaxSplit();
                        if (otax.taxsplit.items == null)
                            otax.taxsplit.items = new List<Tax.TaxSplit.Item>();

                        if (otax.taxsplit.items.Count == 0)
                        {
                            otax.taxsplit.items.Add(new Tax.TaxSplit.Item()
                            {
                                fromamount = 0,
                                toamount = 0,
                                cgst = otax.taxpercentage > 0 ? otax.taxpercentage / 2 : 0,
                                igst = otax.taxpercentage > 0 ? otax.taxpercentage / 2 : 0,
                                sgst = otax.taxpercentage > 0 ? otax.taxpercentage / 2 : 0
                            });
                        }
                        lst.Add(productid, otax);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Ranged Tax Products");
            }
            return lst;
        }

        public SortedList<string, Tax> RangedTaxProducts(List<string> barcodes)
        {
            SortedList<string, Tax> lst = new SortedList<string, Tax>();
            if (barcodes == null || barcodes.Count == 0) return lst;

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(string.Format(@"select barcode,tax.id,tax.type,taxsplit,taxpercentage from stock
                inner join products on products.id = stock.productid
                inner join tax on tax.id = products.salestaxid
                where barcode in ({0})", string.Join(",", barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"))), conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tax otax = new Tax();
                        otax.code = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                        otax.taxsplit_json = reader["taxsplit"] == DBNull.Value ? "null" : reader["taxsplit"].ToString();
                        otax.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt32(reader["type"].ToString());
                        otax.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otax.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        if (otax.taxsplit == null)
                            otax.taxsplit = new Tax.TaxSplit();
                        if (otax.taxsplit.items == null)
                            otax.taxsplit.items = new List<Tax.TaxSplit.Item>();

                        if (otax.taxsplit.items.Count == 0)
                        {
                            otax.taxsplit.items.Add(new Tax.TaxSplit.Item()
                            {
                                fromamount = 0,
                                toamount = 0,
                                cgst = otax.taxpercentage > 0 ? otax.taxpercentage / 2 : 0,
                                igst = otax.taxpercentage > 0 ? otax.taxpercentage / 2 : 0,
                                sgst = otax.taxpercentage > 0 ? otax.taxpercentage / 2 : 0
                            });
                        }
                        lst.Add(otax.code, otax);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Ranged Tax Products");
            }
            return lst;
        }

        public SortedList<string, Tax.TaxSplit> RangedTaxProducts(string billids)
        {

            SortedList<string, Tax.TaxSplit> lst = new SortedList<string, Tax.TaxSplit>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(string.Format(@"select barcode,taxsplit from stock
                inner join products on products.id = stock.productid
                inner join tax on tax.type = 2 and tax.id = products.salestaxid
                where barcode in (select barcode from billitems inner join bill on bill.id = billitems.billid where bill.id in ({0}))", billids), conn);

                if (billids.Contains("'"))
                {
                    command.CommandText = string.Format(@"select barcode,taxsplit from stock
                    inner join products on products.id = stock.productid
                    inner join tax on tax.type = 2 and tax.id = products.salestaxid
                    where barcode in ({0})", billids);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tax otax = new Tax();
                        otax.code = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                        otax.taxsplit_json = reader["taxsplit"] == DBNull.Value ? "null" : reader["taxsplit"].ToString();
                        lst.Add(otax.code, otax.taxsplit);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Ranged Tax Products");
            }
            return lst;
        }

        public bool Delete(Tax oTax)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTax);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTax);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tax oTax)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTax.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTax.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTax.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
