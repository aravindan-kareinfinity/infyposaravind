using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Net.Http;
using WebAPI.Data;

namespace InfyPOS.Data
{
    public class StockdumpingDataService : BaseData
    {
        const string SQL_Search = @"select id, createdon, modifiedon, createdby, version, companyid, locationid, isactive, itemlist, dumpon,dumpgroupid,dumpcode, modifiedby, employeeid, approvedby,updatelist from stockdumping where id=@id";

        const string SQL_Insert = @"INSERT INTO public.stockdumping(createdon, modifiedon, createdby, version, companyid, locationid, isactive, itemlist, dumpon,dumpgroupid,dumpcode, modifiedby, employeeid, approvedby,updatelist) 
        VALUES(@createdon, @modifiedon, @createdby, @version, @companyid, @locationid, @isactive, @itemlist, @dumpon,@dumpgroupid,@dumpcode, @modifiedby, @employeeid, @approvedby,@updatelist) RETURNING id";

        const string SQL_Update = @"UPDATE public.stockdumping SET modifiedon=@modifiedon, version=@version+1, companyid=@companyid, locationid=@locationid, itemlist=@itemlist, 
        updatelist=@updatelist,dumpon=@dumpon,dumpgroupid=@dumpgroupid, modifiedby=@modifiedby, employeeid=@employeeid, approvedby=@approvedby where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.stockdumping set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Stockdumping> Search(StockdumpingCreteria _stockdumping)
        {
            List<Stockdumping> lst = new List<Stockdumping>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_stockdumping.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stockdumping.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if(_stockdumping.dumpgroupid>0)
                        qb.AddParameter("dumpgroupid", _stockdumping.dumpgroupid, "=");

                    if (_stockdumping.locationid > 0)
                        qb.AddParameter("locationid", _stockdumping.locationid, "=");

                    if (_stockdumping.companyid > 0)
                        qb.AddParameter("companyid", _stockdumping.companyid, "=");

                    if (_stockdumping.employeeid > 0)
                        qb.AddParameter("employeeid", _stockdumping.employeeid, "=");

                    if(_stockdumping.fromdate != DateTime.MinValue)
                    {
                        qb.AddParameter("dumpon", _stockdumping.fromdate, ">=");
                        qb.AddParameter("dumpon", _stockdumping.todate.AddDays(1), "<");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stockdumping ostockdumping = new Stockdumping();
                        ostockdumping.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostockdumping.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostockdumping.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostockdumping.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostockdumping.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ostockdumping.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ostockdumping.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ostockdumping.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        if(_stockdumping.id>0)
                            ostockdumping.itemlist_json = reader["itemlist"] == DBNull.Value ? null : reader["itemlist"].ToString();
                        ostockdumping.dumpon = reader["dumpon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dumpon"]);
                        ostockdumping.dumpgroupid = reader["dumpgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dumpgroupid"]);
                        ostockdumping.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostockdumping.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        ostockdumping.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        ostockdumping.dumpcode = reader["dumpcode"] == DBNull.Value ? "" : reader["dumpcode"].ToString();

                        lst.Add(ostockdumping);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _stockdumping);
            }
            return lst;
        }

        public Stockdumping Insert(Stockdumping oStockdumping)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oStockdumping.ChangeStatus(BaseEntity.DataStatus.Create);

                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("stockdumping.dumpcode", 0,
                  InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oStockdumping.companyid : 0);
                    oStockdumping.dumpcode = sequence.GetNextNumber(Context, conn, transaction);
                    oStockdumping.UpdateInternalList();
                    Insert(transaction, conn, oStockdumping);
                    NpgsqlCommand command = new NpgsqlCommand(@"
with alldumpstocks
as
(
select cast(schemelist ->> 'salablegoodsid' as bigint) id,
cast(schemelist ->> 'dqty' as numeric) dmpqty
from stockdumping
LEFT JOIN LATERAL
json_array_elements (stockdumping.updatelist :: json) schemelist ON true
where stockdumping.id = @id
)
update  salablegoods set dmpqty=coalesce(salablegoods.dmpqty,0)+alldumpstocks.dmpqty,iqty=iqty-alldumpstocks.dmpqty
from alldumpstocks
where salablegoods.id = alldumpstocks.id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.id;
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockdumping);
            }

            return oStockdumping;
        }

        public Stockdumping Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockdumping oStockdumping)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockdumping.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockdumping.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockdumping.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockdumping.isactive;
            command.Parameters.Add("updatelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockdumping.updatelist_json == null ? "null" : oStockdumping.updatelist_json;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockdumping.itemlist_json == null ? "null" : oStockdumping.itemlist_json;
            command.Parameters.Add("dumpcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oStockdumping.dumpcode) ? "" : oStockdumping.dumpcode;
            command.Parameters.Add("dumpon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockdumping.dumpon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.modifiedby;
            command.Parameters.Add("dumpgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.dumpgroupid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.employeeid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.approvedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockdumping.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStockdumping;
        }

        public Stockdumping Update(Stockdumping oStockdumping)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    oStockdumping.ChangeStatus(BaseEntity.DataStatus.Update);
                    NpgsqlCommand command = new NpgsqlCommand(@"
with alldumpstocks
as
(
select cast(schemelist ->> 'salablegoodsid' as bigint) id,
cast(schemelist ->> 'dqty' as numeric) dmpqty
from stockdumping
LEFT JOIN LATERAL
json_array_elements (stockdumping.updatelist :: json) schemelist ON true
where stockdumping.id = @id
)
update  salablegoods set dmpqty=coalesce(salablegoods.dmpqty,0)-alldumpstocks.dmpqty,iqty=iqty+alldumpstocks.dmpqty
from alldumpstocks
where salablegoods.id = alldumpstocks.id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.id;
                    command.ExecuteNonQuery();
                    oStockdumping.UpdateInternalList();

                    Update(transaction, conn, oStockdumping);


                    command = new NpgsqlCommand(@"
with alldumpstocks
as
(
select cast(schemelist ->> 'salablegoodsid' as bigint) id,
cast(schemelist ->> 'dqty' as numeric) dmpqty
from stockdumping
LEFT JOIN LATERAL
json_array_elements (stockdumping.updatelist :: json) schemelist ON true
where stockdumping.id = @id
)
update  salablegoods set dmpqty=coalesce(salablegoods.dmpqty,0)+alldumpstocks.dmpqty,iqty=iqty-alldumpstocks.dmpqty
from alldumpstocks
where salablegoods.id = alldumpstocks.id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.id;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockdumping);
            }

            return oStockdumping;
        }

        public Stockdumping Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockdumping oStockdumping)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockdumping.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockdumping.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockdumping.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockdumping.isactive;
            command.Parameters.Add("updatelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockdumping.updatelist_json == null ? "null" : oStockdumping.updatelist_json;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oStockdumping.itemlist_json == null ? "null" : oStockdumping.itemlist_json;
            command.Parameters.Add("dumpon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStockdumping.dumpon;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.modifiedby;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.employeeid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.approvedby;
            command.Parameters.Add("dumpgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.dumpgroupid;
            command.Parameters.Add("dumpcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oStockdumping.dumpcode) ? "" : oStockdumping.dumpcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStockdumping.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oStockdumping;
        }


        public bool Delete(Stockdumping oStockdumping)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = new NpgsqlCommand(@"
with alldumpstocks
as
(
select cast(schemelist ->> 'salablegoodsid' as bigint) id,
cast(schemelist ->> 'dqty' as numeric) dmpqty
from stockdumping
LEFT JOIN LATERAL
json_array_elements (stockdumping.updatelist :: json) schemelist ON true
where stockdumping.id = @id
)
update salablegoods set dmpqty=coalesce(salablegoods.dmpqty,0)-alldumpstocks.dmpqty,iqty=iqty+alldumpstocks.dmpqty
from alldumpstocks
where salablegoods.id = alldumpstocks.id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.id;
                    command.ExecuteNonQuery();

                    hasSuccessCount = Delete(transaction, conn, oStockdumping);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStockdumping);
            }
            return hasSuccessCount;

        }

        public Controllers.FileContentInfo PrintAdvice(long organizationid, Stockdumping stockdump)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "STOCKDUMP"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            
            ReportLibrary.LaserReporter labreport = new ReportLibrary.LaserReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = stockdump;
            labreport.DataItems = stockdump.itemlist;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var companies = companyDataService.Search(new CompanyCreteria()
            {
                id = stockdump.companyid
            });
            if (companies.Count == 1)
                stockdump.company = companies[0];

            UsersDataService usersDataService = new UsersDataService();
            if (stockdump.createdby > 0)
            {
                List<Users> user = usersDataService.Search(new UsersCreteria()
                {
                    id = stockdump.createdby
                });
                if (user.Count > 0)
                    stockdump.createdbyemp = (user != null && user.Count > 0  ? user.First().title : ""); 
            }

            EmployeeDataService empdatasource = new EmployeeDataService();
            if (stockdump.employeeid > 0)
            {
                stockdump.employeename = empdatasource.SearchHeader(new EmployeeCreteria()
                {
                    id = stockdump.employeeid
                }).First().name;
            }
            if (stockdump.approvedby > 0)
            {
                stockdump.approvedbyemp = empdatasource.SearchHeader(new EmployeeCreteria()
                {
                    id = stockdump.approvedby
                }).First().name;
            }

            if (stockdump.company != null && !string.IsNullOrEmpty(stockdump.company.logo))
            {
                var file = imageData.GetData(stockdump.company.logo.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new Controllers.FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "dumpadvice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Stockdumping oStockdumping)
        {
            oStockdumping.ChangeStatus(BaseEntity.DataStatus.Delete);
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockdumping.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oStockdumping.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockdumping.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockdumping.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal List<Stockdumping.ItemDetail> SearchItems(StockdumpingCreteria item)
        {
            var results = new List<Stockdumping.ItemDetail>();
            string query = @"select salablegoods.barcode,stock.serialno,items.gln,
                            Items.salerate,Items.mrp,salablegoods.price,salablegoods.id,items.id itemid,
                            items.printingname,
                            iqty-(oqty+dqty+aqty+jqty) stockqty,stock.productid,lrinvoiceitems.itbuscode,
                            (lrinvoiceitems.amount+lrinvoiceitems.purchasetax)/lrinvoiceitems.buyingqty costprice,
							stock.ibuyingprice
                            from salablegoods
                            inner join stock on stock.id = stockid
                            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                            inner join products on products.id = stock.productid and stdump=true
                            inner join items on items.id = stock.itemid
                            where salablegoods.hasstock=true and salablegoods.isactive=true and salablegoods.locationid=@locationid";

            if (!string.IsNullOrEmpty(item.barcode))
            {
                query += " and (stock.serialno=@barcode)";
            }
            else
            {
                if (item.companyid > 0)
                    query += " and salablegoods.companyid=@companyid";
                if (item.supplierid > 0)
                    query += " and stock.supplierid=@supplierid";
                if (item.productid > 0)
                    query += " and stock.productid=@productid";
                if (item.brandid > 0)
                    query += " and items.brandid=@brandid";
                if (item.dumpgroupid > 0)
                    query += " and products.prddumpid=@dumpgroupid";
            }

            query += " order by salablegoods.id desc";

            if(item.onlynegative)
            {
                query = query.Replace("salablegoods.hasstock=true and", "")+" limit 1";
            }

            var param = new SortedList<string, object>();
            param.Add("barcode", item.barcode);
            param.Add("companyid", item.companyid);
            param.Add("supplierid", item.supplierid);
            param.Add("productid", item.productid);
            param.Add("brandid", item.brandid);
            param.Add("dumpgroupid", item.dumpgroupid);
            param.Add("locationid", item.locationid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    string sellingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();

                    string qbarcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    string barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    decimal costprice = reader["costprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["costprice"]);

                    if(costprice == 0)
                        costprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);

                    decimal price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    decimal stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    long itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                    long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);

                    if (reader["salerate"] == DBNull.Value &&
                        Convert.ToDecimal(reader["salerate"]) > 0)
                        price = Convert.ToDecimal(reader["salerate"]);
                    else if (reader["mrp"] == DBNull.Value &&
                        Convert.ToDecimal(reader["mrp"]) > 0)
                        price = Convert.ToDecimal(reader["mrp"]);

                    if (reader["serialno"] != DBNull.Value && !string.IsNullOrEmpty(reader["serialno"].ToString()))
                        barcode = reader["serialno"].ToString();
                    else if (reader["gln"] != DBNull.Value && !string.IsNullOrEmpty(reader["gln"].ToString()))
                        barcode = reader["gln"].ToString();

                    var itemcode = reader["itbuscode"] == DBNull.Value ? "" : reader["itbuscode"].ToString();
                    if (results.Exists(x => x.barcode == barcode))
                    {
                        var exist = results.Find(x => x.barcode == barcode);
                        exist.stocklist.Add(new Stockdumping.ItemDetail()
                        {
                            barcode = qbarcode,
                            qty = stockqty,
                            salablegoodsid = id,
                        });
                        exist.productname = sellingname;
                        exist.qty += stockqty;
                    }
                    else
                    {
                        var itm = new Stockdumping.ItemDetail()
                        {
                            stocklist = new List<Stockdumping.ItemDetail>(),
                            barcode = barcode,
                            itemid = itemid,
                            productid = productid,
                            productname = sellingname,
                            price = price,
                            costprice = costprice,
                            qty = stockqty,
                            salablegoodsid = id,
                            itemcode = itemcode
                        };
                        itm.stocklist.Add(new Stockdumping.ItemDetail()
                        {
                            barcode = qbarcode,
                            qty = stockqty,
                            salablegoodsid = id,
                        });
                        results.Add(itm);
                    }
                }
            }

            if (results.Count == 0 && !string.IsNullOrEmpty(item.barcode))
            {
                if (!item.onlynegative)
                {
                    item.onlynegative = true;
                    return SearchItems(item);
                }
            }

            return results;
        }
    }

}
