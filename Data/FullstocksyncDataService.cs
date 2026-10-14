using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.IO;
using OfficeOpenXml;

namespace InfyPOS.Data

{
    public class FullstocksyncDataService : SAASData
    {
        const string SQL_Search = @"select fileid, companyid, locationid, synced, rollbacked, stockdetail,missingb2b,errorb2b from fullstocksync where fileid=@fileid";
        const string SQL_Insert = @"INSERT INTO public.fullstocksync(fileid, companyid, locationid, synced, rollbacked, stockdetail,missingb2b,errorb2b) 
        VALUES(@fileid, @companyid, @locationid, @synced, @rollbacked, @stockdetail,@missingb2b,@errorb2b)";

        public List<Fullstocksync> Search(FullstocksyncCreteria _fullstocksync)
        {
            List<Fullstocksync> lst = new List<Fullstocksync>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_fullstocksync.fileid != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _fullstocksync.fileid;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_fullstocksync.Code))
                    //    qb.AddParameter("code", _fullstocksync.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Fullstocksync ofullstocksync = new Fullstocksync();
                        ofullstocksync.fileid = reader["fileid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fileid"]);
                        ofullstocksync.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ofullstocksync.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ofullstocksync.synced = reader["synced"] == DBNull.Value ? false : Convert.ToBoolean(reader["synced"]);
                        ofullstocksync.rollbacked = reader["rollbacked"] == DBNull.Value ? false : Convert.ToBoolean(reader["rollbacked"]);
                        ofullstocksync.stockdetail_json = reader["stockdetail"] == DBNull.Value ? null : reader["stockdetail"].ToString();
                        ofullstocksync.missingb2b_json = reader["missingb2b"] == DBNull.Value ? null : reader["missingb2b"].ToString();
                        ofullstocksync.errorb2b_json = reader["errorb2b"] == DBNull.Value ? null : reader["errorb2b"].ToString();
                        lst.Add(ofullstocksync);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _fullstocksync);
            }
            return lst;
        }

        internal SingleStockFile FindFullUANStock(SingleStockFile singleStockFile)
        {
            List<Fullstocksync.UANStockdetail> retList = new List<Fullstocksync.UANStockdetail>();
            string whsourcequery = @"
with sourcerows as
(
select qty,transfered,stock.barcode,cast(schemelist->>'stock' as numeric) as stock,stock.serialno
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
left outer join stock on stock.serialno = schemelist->>'barcode'
where fullstocksync.fileid = @fileid and stock.companyid = @companyid and stock.isactive=true 
and stock.serialno != '' and stock.serialno is not null
)

select * from sourcerows";

            string retailsourcequery = @"
with sourcerows as
(
select stock.barcode, iqty as qty, oqty as transfered, cast(schemelist->> 'stock' as numeric) stock,stock.serialno
 from fullstocksync
LEFT JOIN LATERAL
json_array_elements(fullstocksync.stockdetail :: json) schemelist ON true
left outer join stock on stock.serialno = schemelist->>'barcode'
left outer join salablegoods on salablegoods.stockid = stock.id
where salablegoods.isactive=true and stock.isactive=true and fullstocksync.fileid = @fileid and salablegoods.locationid = @locationid 
and salablegoods.companyid = @companyid  and stock.serialno != '' and stock.serialno is not null
)

select * from sourcerows";



            Data.ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = singleStockFile.locationid
            }).First();
            var searchsourcequery = (locationinfo.additionalinfo != null && locationinfo.additionalinfo.iswarehouse) ? whsourcequery : retailsourcequery;
            if (singleStockFile.companyid == 0)
            {
                searchsourcequery = searchsourcequery.Replace("and salablegoods.companyid=@companyid", "");
                searchsourcequery = searchsourcequery.Replace("and stock.companyid = @companyid", "");
                searchsourcequery = searchsourcequery.Replace("where stock.companyid = @companyid", "");
            }

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(searchsourcequery, conn);
                command.CommandTimeout = 10000;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;

                List<Fullstocksync.UANStockdetail> sourcestocklist = new List<Fullstocksync.UANStockdetail>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sourcestocklist.Add(new Fullstocksync.UANStockdetail()
                        {
                            barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString(),
                            uan = reader["uan"] == DBNull.Value ? "" : reader["uan"].ToString(),
                            qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                            stock = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]),
                            transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"])
                        });
                    }
                }
                foreach (var uan in sourcestocklist.GroupBy(e => e.uan))
                {
                    var maxstock = uan.First().stock;
                    foreach (var item in uan)
                    {
                        decimal stock = 0;
                        if (item.qty - item.transfered < maxstock)
                        {
                            stock = item.qty - item.transfered;
                        }
                        else
                        {
                            stock = maxstock;
                        }
                        retList.Add(new Fullstocksync.UANStockdetail()
                        {
                            barcode = item.barcode,
                            uan = uan.Key,
                            stock = stock,
                        });
                        maxstock -= stock;
                        if (maxstock <= 0) break;
                    }
                    if (maxstock > 0)
                    {
                        retList.Last().stock += maxstock;
                    }
                }
                conn.Close();
                if (retList.Count > 0)
                {
                    var current = Search(new FullstocksyncCreteria()
                    {
                        fileid = singleStockFile.id
                    }).First();
                    var conflictlist = current.stockdetail.FindAll(e => retList.Exists(x => x.uan == e.barcode));
                    current.stockdetail.RemoveAll(e => retList.Exists(x => x.uan == e.barcode));
                    current.fileid = 0;
                    Insert(current);
                    singleStockFile.id = current.fileid;
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, singleStockFile);
            }
            return singleStockFile;
        }

        internal bool SyncFullUANStock(SingleStockFile singleStockFile)
        {
            string whsourcequery = @"
with sourcerows as
(
select qty,transfered,barcode,cast(schemelist->>'stock' as numeric) as stock,stock.serialno
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
left outer stock on stock.serialno = schemelist->>'barcode'
where fullstocksync.fileid = @fileid and stock.companyid = @companyid 
and stock.isactive=true and stock.serialno != '' and stock.serialno is not null
)

select * from sourcerows";

            string retailsourcequery = @"
with sourcerows as
(
select barcode, iqty as qty, oqty as transfered, cast(schemelist->> 'stock' as numeric) stock,stock.serialno
 from fullstocksync
LEFT JOIN LATERAL
json_array_elements(fullstocksync.stockdetail :: json) schemelist ON true
left outer stock on stock.serialno = schemelist->>'barcode'
left outer salablegoods on salablegoods.stockid = stock.id
where fullstocksync.fileid = @fileid and salablegoods.locationid = @locationid 
and salablegoods.isactive=true and stock.isactive=true and salablegoods.companyid = @companyid  
and stock.serialno != '' and stock.serialno is not null
)

select * from sourcerows";


            string whquery = @"
with sourcerows as
(
select modifiedon,price,barcode,cast(@sourcefileid as varchar) as referenceid,itemid,invoiceitemid,hsncode,buyingprice,designid,
cast(schemelist->>'stock' as numeric) as auditqty,journalqty,returnqty,1 as actiontype,isactive,createdby,modifiedby,version,createdon,
printingname,productid,discount,additionaldiscount,journalqty,damagedqty,qty,transfered,stock.id stockid,stock.locationid,isbit
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode'
where fullstocksync.fileid = @fileid and stock.companyid = @companyid
and stock.isactive=true 
)

insert into stockhistory(modifiedon,price,barcode,referenceid,itemid,invoiceitemid,hsncode,buyingprice,designid,auditqty,journalqty,
returnqty,actiontype,isactive,createdby,modifiedby,version,createdon,printingname,productid,discount,additionaldiscount,damagedqty,qty,
transfered,stockid,locationid,isbit)
select * from sourcerows;

update stock set journalqty=qty-(transfered+damagedqty+returnqty) where stock.companyid = @companyid;

update stock set journalqty=journalqty-cast(schemelist->>'stock' as numeric)
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
where fullstocksync.fileid=@fileid and stock.barcode = schemelist->>'barcode' and stock.companyid = @companyid;";

            //            string locationcopy = @"with sourcerows as
            //(
            //select
            //	received,supplierid,inwardon,price,discount,additionaldiscount,salereturnqty,pqty,ispurchaselocation,
            //	rqty,toqty,tiqty,costprice,icostprice,version,modifiedby,createdby,isactive,oqty,iqty,modifiedon,createdon,dqty,stockid,barcode,
            //	1237,bundleid,itemid,salablegoods.companyid
            // from fullstocksync
            //LEFT JOIN LATERAL
            //json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
            //inner join salablegoods on salablegoods.barcode = schemelist->>'barcode'
            //where fullstocksync.fileid = 3 
            //)


            //insert into salablegoods(received,supplierid,inwardon,price,discount,additionaldiscount,salereturnqty,pqty,ispurchaselocation,
            //	rqty,toqty,tiqty,costprice,icostprice,version,modifiedby,createdby,isactive,oqty,iqty,modifiedon,createdon,dqty,stockid,barcode,
            //	locationid,bundleid,itemid,companyid)
            //select * from sourcerows;";

            string retailquery = @"
with sourcerows as
(
 select createdby,version,modifiedby,isactive,discount,price,salablegoods.locationid,barcode,stockid,jqty,dqty,oqty,cast(schemelist->>'stock' as numeric) auditqty,1 as actiontype,
 salablegoods.id salablegoodsid,additionaldiscount,createdon,modifiedon,iqty,cast(@sourcefileid as varchar) as referenceid
 from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join salablegoods on salablegoods.barcode = schemelist->>'barcode'
where salablegoods.isactive=true and fullstocksync.fileid = @fileid 
and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid
)

insert into salablegoodshistory(createdby,version,modifiedby,isactive,discount,price,locationid,barcode,stockid,jqty,dqty,oqty,
								auditqty,actiontype,salablegoodsid,additionaldiscount,createdon,modifiedon,iqty,referenceid)
select * from sourcerows;

update salablegoods set dqty=iqty-oqty 
where salablegoods.locationid=@locationid and salablegoods.companyid=@companyid;


update salablegoods set dqty=dqty-cast(schemelist->>'stock' as numeric)
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
where fullstocksync.fileid=@fileid and salablegoods.barcode = schemelist->>'barcode'
and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid;";

            Data.ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = singleStockFile.locationid
            }).First();
            var searchquery = (locationinfo.additionalinfo != null && locationinfo.additionalinfo.iswarehouse) ? whquery : retailquery;
            var searchsourcequery = (locationinfo.additionalinfo != null && locationinfo.additionalinfo.iswarehouse) ? whsourcequery : retailsourcequery;
            if (singleStockFile.companyid == 0)
            {
                searchquery = searchquery.Replace("and salablegoods.companyid=@companyid", "");
                searchquery = searchquery.Replace("and stock.companyid = @companyid", "");
                searchquery = searchquery.Replace("where stock.companyid = @companyid", "");

                searchsourcequery = searchsourcequery.Replace("and salablegoods.companyid=@companyid", "");
                searchsourcequery = searchsourcequery.Replace("and stock.companyid = @companyid", "");
                searchsourcequery = searchsourcequery.Replace("where stock.companyid = @companyid", "");
            }

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                Fullstocksync fullstocksync = new Fullstocksync()
                {
                    companyid = singleStockFile.companyid,
                    locationid = singleStockFile.locationid,
                    stockdetail = new List<Fullstocksync.Stockdetail>()
                };

                NpgsqlCommand command = new NpgsqlCommand(searchsourcequery, conn);
                command.CommandTimeout = 10000;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;


                List<Fullstocksync.UANStockdetail> sourcestocklist = new List<Fullstocksync.UANStockdetail>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sourcestocklist.Add(new Fullstocksync.UANStockdetail()
                        {
                            barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString(),
                            uan = reader["uan"] == DBNull.Value ? "" : reader["uan"].ToString(),
                            qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]),
                            stock = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]),
                            transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"])
                        });
                    }
                }
                foreach (var uan in sourcestocklist.GroupBy(e => e.uan))
                {
                    var maxstock = uan.First().stock;
                    foreach (var item in uan)
                    {
                        decimal stock = 0;
                        if (item.qty - item.transfered < maxstock)
                        {
                            stock = item.qty - item.transfered;
                        }
                        else
                        {
                            stock = maxstock;
                        }
                        fullstocksync.stockdetail.Add(new Fullstocksync.Stockdetail()
                        {
                            barcode = item.barcode,
                            stock = stock
                        });
                        maxstock -= stock;
                        if (maxstock <= 0) break;
                    }
                    if (maxstock > 0)
                    {
                        fullstocksync.stockdetail.Last().stock += maxstock;
                    }
                }

                var transaction = conn.BeginTransaction();
                Insert(transaction, conn, fullstocksync);
                command = new NpgsqlCommand(searchquery, conn, transaction);
                command.CommandTimeout = 10000;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                command.Parameters.Add("sourcefileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;
                command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = fullstocksync.fileid;

                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, singleStockFile);
                return false;
            }
            return true;
        }
        internal bool SyncFullStock(SingleStockFile singleStockFile)
        {


            string whquery = @"
with sourcerows as
(
select modifiedon,price,barcode,cast(@fileid as varchar) as referenceid,itemid,invoiceitemid,hsncode,buyingprice,designid,
cast(schemelist->>'stock' as numeric) as auditqty,journalqty,returnqty,1 as actiontype,isactive,createdby,modifiedby,version,createdon,
printingname,productid,discount,additionaldiscount,damagedqty,journalqty,qty,transfered,stock.id stockid,stock.locationid,isbit
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode'
where stock.isactive=true and fullstocksync.fileid = @fileid and stock.companyid = @companyid
)

insert into stockhistory(modifiedon,price,barcode,referenceid,itemid,invoiceitemid,hsncode,buyingprice,designid,auditqty,journalqty,
returnqty,actiontype,isactive,createdby,modifiedby,version,createdon,printingname,productid,discount,additionaldiscount,damagedqty,qty,
transfered,stockid,locationid,isbit)
select * from sourcerows;

update stock set journalqty=qty-(transfered+damagedqty+returnqty) where stock.companyid = @companyid;

update stock set journalqty=journalqty-cast(schemelist->>'stock' as numeric)
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
where fullstocksync.fileid=@fileid and stock.barcode = schemelist->>'barcode' and stock.companyid = @companyid;";

            //            string locationcopy = @"with sourcerows as
            //(
            //select
            //	received,supplierid,inwardon,price,discount,additionaldiscount,salereturnqty,pqty,ispurchaselocation,
            //	rqty,toqty,tiqty,costprice,icostprice,version,modifiedby,createdby,isactive,oqty,iqty,modifiedon,createdon,dqty,stockid,barcode,
            //	1237,bundleid,itemid,salablegoods.companyid
            // from fullstocksync
            //LEFT JOIN LATERAL
            //json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
            //inner join salablegoods on salablegoods.barcode = schemelist->>'barcode'
            //where fullstocksync.fileid = 3 
            //)


            //insert into salablegoods(received,supplierid,inwardon,price,discount,additionaldiscount,salereturnqty,pqty,ispurchaselocation,
            //	rqty,toqty,tiqty,costprice,icostprice,version,modifiedby,createdby,isactive,oqty,iqty,modifiedon,createdon,dqty,stockid,barcode,
            //	locationid,bundleid,itemid,companyid)
            //select * from sourcerows;";

            string retailquery = @"
with sourcerows as
(
select createdby,version,modifiedby,isactive,discount,price,salablegoods.locationid,barcode,stockid,jqty,dqty,oqty,cast(schemelist->>'stock' as numeric) auditqty,1 as actiontype,
 salablegoods.id salablegoodsid,additionaldiscount,createdon,modifiedon,iqty,cast(@fileid as varchar) as referenceid
 from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join salablegoods on salablegoods.barcode = schemelist->>'barcode'
where salablegoods.isactive=true and fullstocksync.fileid = @fileid and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid
)

insert into salablegoodshistory(createdby,version,modifiedby,isactive,discount,price,locationid,barcode,stockid,jqty,dqty,oqty,
								auditqty,actiontype,salablegoodsid,additionaldiscount,createdon,modifiedon,iqty,referenceid)
select * from sourcerows;

update salablegoods set jqty=(iqty-(oqty+dqty))
where salablegoods.locationid=@locationid and salablegoods.companyid=@companyid;


update salablegoods set jqty=jqty-cast(schemelist->>'stock' as numeric)
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
where fullstocksync.fileid=@fileid and salablegoods.barcode = schemelist->>'barcode'
and salablegoods.isactive=true and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid;";

            Data.ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = singleStockFile.locationid
            }).First();
            var searchquery = (locationinfo.additionalinfo != null && locationinfo.additionalinfo.iswarehouse) ? whquery : retailquery;
            if (singleStockFile.companyid == 0)
            {
                searchquery = searchquery.Replace("and salablegoods.companyid=@companyid", "");
                searchquery = searchquery.Replace("and stock.companyid = @companyid", "");
                searchquery = searchquery.Replace("where stock.companyid = @companyid", "");
            }

            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    singleStockFile = FindFullUANStock(singleStockFile);
                }
                conn.Open();
                NpgsqlCommand command = null;
                command = new NpgsqlCommand(searchquery, conn);
                command.CommandTimeout = 10000;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;
                command.ExecuteNonQuery();


                if (singleStockFile.merge || singleStockFile.remove)
                {
                    var mergedata = SearchFullStockConflict(singleStockFile);
                    foreach (var prd in mergedata)
                    {
                        var jqty = (singleStockFile.merge ? prd.mergeqty : 0) + (singleStockFile.remove ? prd.removeqty : 0);
                        command = new NpgsqlCommand("Update salablegoods set jqty=jqty+@jqty where locationid=@locationid and barcode=@barcode", conn);
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                        command.Parameters.Add("jqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = jqty;
                        command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = prd.barcode;
                        command.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, singleStockFile);
                return false;
            }
            return true;
        }

        public Fullstocksync Insert(Fullstocksync oFullstocksync)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oFullstocksync);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oFullstocksync);

            }

            return oFullstocksync;
        }

        public class FullStockSummary
        {
            public List<FullStockSummaryItem> summary { get; set; }
            public List<MissingItem> missing { get; set; }
            public List<string> errorb2b { get; internal set; }
            public List<b2bdesignbased> missingb2b { get; internal set; }

            public class MissingItem
            {
                public string barcode { get; set; }
                public decimal qty { get; set; }
            }
            public class FullStockSummaryItem
            {
                public string product { get; set; }
                public decimal currentstock { get; set; }
                public decimal currentcost { get; set; }
                public decimal currentvalue { get; set; }
                public decimal newstock { get; set; }
                public decimal newcost { get; set; }
                public decimal newvalue { get; set; }
                public decimal merge { get; set; }
                public decimal remove { get; set; }
            }

            public class FullStockMismatchesItem
            {
                public string product { get; set; }
                public string barcode { get; set; }
                public decimal physicalstock { get; set; }
                public decimal mergeqty { get; set; }
                public decimal removeqty { get; set; }
                public decimal currentstock { get; set; }
            }
        }
        public class SingleStockFile : BaseEntity
        {
            public long id { get; set; }
            public string file { get; set; }
            public long size { get; set; }
            public long companyid { get; set; }
            public long locationid { get; set; }
            public DateTime mergefromdate { get; set; }
            public DateTime mergetodate { get; set; }
            public DateTime removefromdate { get; set; }
            public DateTime removetodate { get; set; }
            public bool remove { get; set; }
            public bool merge { get; set; }
        }

        public List<b2bdesignbased> Searchbrandwisestock(long companyid, long locationid,
            b2bdesignbased source, int type)
        {
            List<b2bdesignbased> list = new List<b2bdesignbased>();
            string query = @"select stock.designid,salablegoods.id,iqty,iqty-(oqty+dqty+jqty) stock,
            salablegoods.barcode,sizelist.name sizename, products.name product ,salablegoods.price,
            brand.name brandname from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            LEFT OUTER join items on items.id = stock.itemid
			left outer join brand on brand.id = items.brandid
            LEFT OUTER join referencelist sizelist on sizelist.id = items.sizeid
			LEFT OUTER join referencelist colourlist on colourlist.id = items.colourid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            inner JOIN products on products.id = stock.productid
            where stock.isactive=true and salablegoods.isactive=true and products.name=@productname
            and sizelist.name=@sizename and stock.designid=@designid and brand.name=@brandname
			and salablegoods.companyid=@companyid and salablegoods.locationid=@locationid
            order by stock.id desc limit 5";
            if (type == 2)
            {
                query = @"select stock.designid,salablegoods.id,iqty,iqty-(oqty+dqty+jqty) stock,
            salablegoods.barcode,sizelist.name sizename, products.name product ,salablegoods.price,
            brand.name brandname from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            LEFT OUTER join items on items.id = stock.itemid
			left outer join brand on brand.id = items.brandid
            LEFT OUTER join referencelist sizelist on sizelist.id = items.sizeid
			LEFT OUTER join referencelist colourlist on colourlist.id = items.colourid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            inner JOIN products on products.id = stock.productid
            where stock.isactive=true and salablegoods.isactive=true and products.name=@productname
            and stock.designid=@designid and brand.name=@brandname
			and salablegoods.companyid=@companyid and salablegoods.locationid=@locationid
            order by stock.id desc limit 5";
            }
            if (type == 1)
            {
                query = @"select stock.designid,salablegoods.id,iqty,iqty-(oqty+dqty+jqty) stock,
                salablegoods.barcode,sizelist.name sizename, products.name product ,salablegoods.price,
                brand.name brandname from salablegoods
                inner join stock on stock.id = salablegoods.stockid
                LEFT OUTER join items on items.id = stock.itemid
			    left outer join brand on brand.id = items.brandid
                LEFT OUTER join referencelist sizelist on sizelist.id = items.sizeid
			    LEFT OUTER join referencelist colourlist on colourlist.id = items.colourid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                inner JOIN products on products.id = stock.productid
                where stock.isactive=true and salablegoods.isactive=true
                and sizelist.name=@sizename and stock.designid=@designid
			    and salablegoods.companyid=@companyid and salablegoods.locationid=@locationid
                order by stock.id desc limit 5";
            }

            var param = new SortedList<string, object>();
            param.Add("locationid", locationid);
            param.Add("companyid", companyid);
            param.Add("productname", source.productid);
            param.Add("sizename", source.sizeid);
            param.Add("designid", source.design);
            param.Add("brandname", source.brandid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    b2bdesignbased b2Bdesignbased = new b2bdesignbased();
                    b2Bdesignbased.productid = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                    b2Bdesignbased.sizeid = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                    b2Bdesignbased.brandid = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                    b2Bdesignbased.design = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    b2Bdesignbased.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    b2Bdesignbased.key = string.Format("{0}-{1}-{2}-{3}", b2Bdesignbased.productid,
                         b2Bdesignbased.brandid, b2Bdesignbased.sizeid, b2Bdesignbased.design);

                    if (list.Exists(e => e.key == b2Bdesignbased.key))
                    {
                        b2Bdesignbased = list.Find(e => e.key == b2Bdesignbased.key);
                    }
                    else
                    {
                        b2Bdesignbased.stocklist = new List<b2bdesignbased.Stock>();
                        list.Add(b2Bdesignbased);
                    }
                    b2Bdesignbased.stocklist.Add(new b2bdesignbased.Stock()
                    {
                        barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString(),
                        qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]),
                        iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"])
                    });
                }
            }
            if (list.Count == 0 && type == 0)
            {
                list = Searchbrandwisestock(companyid, locationid, source, 1);
                list.ForEach(e => e.key = source.key);
            }
            if (list.Count == 0 && type == 1)
            {
                list = Searchbrandwisestock(companyid, locationid, source, 2);
                list.ForEach(e => e.key = source.key);
            }
            return list;
        }
        public List<b2bdesignbased> Searchbrandwisestock(long companyid, long locationid)
        {
            List<b2bdesignbased> list = new List<b2bdesignbased>();
            string query = @"select stock.designid,salablegoods.id,iqty,iqty-(oqty+dqty+jqty) stock,
            salablegoods.barcode,sizelist.name sizename, products.name product ,salablegoods.price,
            brand.name brandname from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            LEFT OUTER join items on items.id = stock.itemid
			left outer join brand on brand.id = items.brandid
            LEFT OUTER join referencelist sizelist on sizelist.id = items.sizeid
			LEFT OUTER join referencelist colourlist on colourlist.id = items.colourid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            inner JOIN products on products.id = stock.productid
            where stock.isactive=true and salablegoods.isactive=true and salablegoods.hasstock=true 
			and salablegoods.companyid=@companyid and salablegoods.locationid=@locationid";
            var param = new SortedList<string, object>();
            param.Add("locationid", locationid);
            param.Add("companyid", companyid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    b2bdesignbased b2Bdesignbased = new b2bdesignbased();
                    b2Bdesignbased.productid = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                    b2Bdesignbased.sizeid = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                    b2Bdesignbased.brandid = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                    b2Bdesignbased.design = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    b2Bdesignbased.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    b2Bdesignbased.key = string.Format("{0}-{1}-{2}-{3}", b2Bdesignbased.productid,
                         b2Bdesignbased.brandid, b2Bdesignbased.sizeid, b2Bdesignbased.design);

                    if (list.Exists(e => e.key == b2Bdesignbased.key))
                    {
                        b2Bdesignbased = list.Find(e => e.key == b2Bdesignbased.key);
                    }
                    else
                    {
                        b2Bdesignbased.stocklist = new List<b2bdesignbased.Stock>();
                        list.Add(b2Bdesignbased);
                    }
                    b2Bdesignbased.stocklist.Add(new b2bdesignbased.Stock()
                    {
                        barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString(),
                        qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]),
                        iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"])
                    });
                }
            }
            return list;
        }
        public class b2bdesignbased
        {
            public string brandid { get; set; }
            public string sizeid { get; set; }
            public string productid { get; set; }
            public string design { get; set; }
            public string key { get; set; }
            public decimal stock { get; set; }
            public decimal price { get; set; }
            public List<Stock> stocklist { get; set; }
            public class Stock
            {
                public string barcode { get; set; }
                public decimal qty { get; set; }
                public decimal iqty { get; set; }
                public decimal adjusted { get; set; }
                public decimal available
                {
                    get
                    {
                        return qty - adjusted;
                    }
                }
            }
        }
        public List<FullStockSummary.FullStockMismatchesItem> SearchFullStockConflict(SingleStockFile singleStockFile)
        {
            List<FullStockSummary.FullStockMismatchesItem> response = new List<FullStockSummary.FullStockMismatchesItem>();
           string query = @"with uniquebarcodes as
                            (
                            select schemelist->>'barcode' as barcode,sum(cast(schemelist->>'stock' as numeric)) physicalstock
                            from fullstocksync
                            LEFT JOIN LATERAL
                            json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
                            where fullstocksync.fileid = @fileid
                            group by schemelist->>'barcode'
                            ),mergebarcodes as
                            (
                            select sum(billitems.qty) mergeqty,billitems.barcode from billitems
                            inner join bill on bill.id =billid 
                            where billdate>=@mergefrom and billdate<@mergeto and bill.locationid=@locationid and bill.companyid=@companyid
                            group by billitems.barcode
                            ),removebarcodes as
                            (
                            select sum(billitems.qty) removeqty,billitems.barcode,billitems.salablegoodsid from billitems
                            inner join bill on bill.id =billid 
                            where billdate>=@removefrom and billdate<@removeto and bill.locationid=@locationid and bill.companyid=@companyid
                            group by billitems.barcode,billitems.salablegoodsid
                            )
                            select products.name,physicalstock,mergebarcodes.mergeqty,removebarcodes.removeqty,
                            uniquebarcodes.barcode,salablegoods.iqty -(salablegoods.oqty+salablegoods.dqty) currentstock from uniquebarcodes
                            left outer join mergebarcodes on mergebarcodes.barcode = uniquebarcodes.barcode
                            left outer join removebarcodes on removebarcodes.barcode = uniquebarcodes.barcode
                            inner join salablegoods on salablegoods.barcode = uniquebarcodes.barcode and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid
                            inner join stock on stock.id = salablegoods.stockid
                            inner join products on products.id = stock.productid
                            where (mergebarcodes.barcode is not null or  removebarcodes.barcode is not null)
                            and physicalstock != (salablegoods.iqty -(salablegoods.oqty+salablegoods.dqty))";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;
                command.Parameters.Add("removefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = singleStockFile.removefromdate;
                command.Parameters.Add("removeto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = singleStockFile.removetodate.AddDays(1);
                command.Parameters.Add("mergefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = singleStockFile.mergefromdate;
                command.Parameters.Add("mergeto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = singleStockFile.mergetodate.AddDays(1);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FullStockSummary.FullStockMismatchesItem fullStockMismatchesItem = new FullStockSummary.FullStockMismatchesItem();
                        fullStockMismatchesItem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        fullStockMismatchesItem.product = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        fullStockMismatchesItem.currentstock = reader["currentstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentstock"]);
                        fullStockMismatchesItem.removeqty = reader["removeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["removeqty"]);
                        fullStockMismatchesItem.mergeqty = reader["mergeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mergeqty"]);
                        fullStockMismatchesItem.physicalstock = reader["physicalstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["physicalstock"]);
                        response.Add(fullStockMismatchesItem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
            }
            return response;
        }

        public FullStockSummary SearchFullStock(SingleStockFile singleStockFile)
        {
            var result = Search(new FullstocksyncCreteria()
            {
                fileid = singleStockFile.id
            });

            bool isdesignbased = false;
            if (result.Count == 0)
            {

                Fullstocksync item = new Fullstocksync();
                item.fileid = singleStockFile.id;
                item.companyid = singleStockFile.companyid;
                item.locationid = singleStockFile.locationid;

                var columnsheaders = new List<string>();
                var rows = new List<List<string>>();

                var imageData = new InfyPOS.Common.ImageData();
                var file = imageData.GetData(singleStockFile.id);

                using (MemoryStream ms = new MemoryStream(file.data))
                {
                    ExcelPackage package = new ExcelPackage(ms);
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                    bool firstRow = true;

                    for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                    {
                        var excelrow = worksheet.Row(row);
                        var rowvalues = new List<string>();
                        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                        {
                            var cellvalue = worksheet.GetValue(row, col);
                            if (firstRow)
                            {
                                string scellvalue = (cellvalue != null ? cellvalue.ToString() : "").ToLower();
                                columnsheaders.Add(scellvalue.ToLower());
                            }
                            else
                            {
                                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                            }
                        }
                        if (firstRow)
                        {
                            firstRow = false;
                        }
                        else
                        {
                            rows.Add(rowvalues);
                        }
                    }
                }

                var onlybarcodes = (columnsheaders.Count == 1);
                var list = new List<Fullstocksync.Stockdetail>();
                List<b2bdesignbased> missingbarcodes = new List<b2bdesignbased>();
                var errorList = new List<string>();
                if (!onlybarcodes && columnsheaders.Count > 3)
                {
                    isdesignbased = true;
                    List<b2bdesignbased> available = Searchbrandwisestock(singleStockFile.companyid, singleStockFile.locationid);
                    int brandcolumn = columnsheaders.FindIndex(e => e.ToLower() == "brand");
                    int sizecolumn = columnsheaders.FindIndex(e => e.ToLower() == "size");
                    int designcolumn = columnsheaders.FindIndex(e => e.ToLower().IndexOf("design") >= 0);
                    int productcolumn = columnsheaders.FindIndex(e => e.ToLower() == "product");
                    int ratecolumn = columnsheaders.FindIndex(e => e.ToLower() == "rate");
                    int stockcolumn = columnsheaders.FindIndex(e => e.ToLower().IndexOf("stock") >= 0);
                    if (brandcolumn > 0)
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(rows.ConvertAll(e => new { name = e[brandcolumn].Replace("'", "") }).Distinct().ToList());
                        string query = string.Format(@"select value->>'name' from json_array_elements('{0}')
                                         left outer join brand on brand.name = value->>'name' where brand.id is null", json);
                        var param = new SortedList<string, object>();
                        using (var reader = ExecuteReader(query, param))
                        {
                            while (reader.Read())
                            {
                                errorList.Add("Missing Brand " + reader.GetString(0));
                            }
                        }
                    }

                    if (productcolumn > 0)
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(rows.ConvertAll(e => new { name = e[productcolumn].Replace("'", "") }).Distinct().ToList());
                        string query = string.Format(@"select value->>'name' from json_array_elements('{0}')
                                         left outer join products on products.name = value->>'name' where products.id is null", json);
                        var param = new SortedList<string, object>();
                        using (var reader = ExecuteReader(query, param))
                        {
                            while (reader.Read())
                            {
                                errorList.Add("Missing Products " + reader.GetString(0));
                            }
                        }
                    }

                    if (sizecolumn > 0)
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(rows.ConvertAll(e => new { name = e[sizecolumn].Replace("'", "") }).Distinct().ToList());
                        string query = string.Format(@"select value->>'name' from json_array_elements('{0}')
                                         left outer join referencelist on referencelist.name = value->>'name' where referencelist.id is null", json);
                        var param = new SortedList<string, object>();
                        using (var reader = ExecuteReader(query, param))
                        {
                            while (reader.Read())
                            {
                                errorList.Add("Missing Size " + reader.GetString(0));
                            }
                        }
                    }

                    //                 if (sizecolumn > 0)
                    //                 {
                    //                     int sno = 0;
                    //                     var datarows = rows.ConvertAll(e => new
                    //                     {
                    //                         id = sno++,
                    //                         sname = e[sizecolumn].Replace("'",""),
                    //                         pname = e[productcolumn].Replace("'",""),
                    //                         dname = e[designcolumn].Replace("'",""),
                    //                         bname = e[brandcolumn].Replace("'",""),
                    //                         stock = parsedecimal(e[stockcolumn])
                    //                     }).Distinct().ToList();
                    //                     var json = Newtonsoft.Json.JsonConvert.SerializeObject(datarows);
                    //                     string query = string.Format(@"select value->>'id',stock.id,products.id,brand.id,sizename.id 
                    //         from json_array_elements('{0}')
                    //         inner join referencelist sizename on sizename.name = value->>'sname'
                    //         inner join products on products.name = value->>'pname'
                    //         inner join brand on brand.name = value->>'bname'
                    //         inner join items on items.sizeid = sizename.id and items.productid = products.id and items.brandid = brand.id
                    //         inner join stock on stock.itemid = items.id and stock.designid = value->>'dname'
                    //         inner join salablegoods on salablegoods.stockid = stock.id
                    //         where salablegoods.isactive=true
                    //and salablegoods.companyid=@companyid and salablegoods.locationid=@locationid", json);
                    //                     var param = new SortedList<string, object>();
                    //                     param.Add("locationid", singleStockFile.locationid);
                    //                     param.Add("companyid", singleStockFile.companyid);

                    //                     using (var reader = ExecuteReader(query, param))
                    //                     {
                    //                         while (reader.Read())
                    //                         {
                    //                             long id = Convert.ToInt32(reader[0]);
                    //                             long stockid = reader[1] == DBNull.Value ? 0 : Convert.ToInt64(reader[1]);
                    //                             if(id>0 && stockid>0)
                    //                             {
                    //                                 datarows.RemoveAll(e => e.id == id);
                    //                             }
                    //                         }
                    //                     }

                    //                     foreach(var row in datarows)
                    //                     {
                    //                         errorList.Add(string.Format("Missing Row {0} {1}-{2}-{3}-{4}",row.id,row.pname,
                    //                             row.bname,row.sname,row.dname,row.stock));
                    //                     }
                    //                 }

                    foreach (var row in rows)
                    {
                        Fullstocksync.Stockdetail stockdetail = new Fullstocksync.Stockdetail();
                        b2bdesignbased b2Bdesignbased = new b2bdesignbased();
                        b2Bdesignbased.brandid = brandcolumn < 0 ? "" : row[brandcolumn];
                        b2Bdesignbased.sizeid = sizecolumn < 0 ? "" : row[sizecolumn];
                        b2Bdesignbased.design = designcolumn < 0 ? "" : row[designcolumn];
                        b2Bdesignbased.productid = productcolumn < 0 ? "" : row[productcolumn];
                        b2Bdesignbased.price = ratecolumn < 0 ? 0 : parsedecimal(row[ratecolumn]);
                        b2Bdesignbased.key = string.Format("{0}-{1}-{2}-{3}", b2Bdesignbased.productid,
                                    b2Bdesignbased.brandid, b2Bdesignbased.sizeid, b2Bdesignbased.design);
                        b2Bdesignbased.stock = stockcolumn < 0 ? 0 : parsedecimal(row[stockcolumn]);
                        if (b2Bdesignbased.stock == 0) continue;

                        var isexist = (available.Exists(e => e.key == b2Bdesignbased.key));
                        if (!isexist)
                        {
                            var exactAvailable = Searchbrandwisestock(singleStockFile.companyid,
                                singleStockFile.locationid, b2Bdesignbased, 0);
                            if (exactAvailable.Count > 0)
                            {
                                available.AddRange(exactAvailable);
                                isexist = (available.Exists(e => e.key == b2Bdesignbased.key));
                                //if (!isexist)
                                //{
                                //    errorList.Add(b2Bdesignbased.key + " Missing");
                                //}
                            }
                        }
                        if (isexist)
                        {
                            var avail = available.Find(e => e.key == b2Bdesignbased.key);
                            for (var i = 0; i < avail.stocklist.Count && b2Bdesignbased.stock > 0; i++)
                            {
                                if (avail.stocklist[i].available > 0)
                                {
                                    if (b2Bdesignbased.stock <= avail.stocklist[i].available)
                                    {
                                        list.Add(new Fullstocksync.Stockdetail()
                                        {
                                            barcode = avail.stocklist[i].barcode,
                                            stock = b2Bdesignbased.stock,
                                            price = b2Bdesignbased.price
                                        });
                                        avail.stocklist[i].adjusted += b2Bdesignbased.stock;
                                        b2Bdesignbased.stock = 0;
                                    }
                                    else
                                    {
                                        list.Add(new Fullstocksync.Stockdetail()
                                        {
                                            barcode = avail.stocklist[i].barcode,
                                            stock = avail.stocklist[i].available,
                                            price = b2Bdesignbased.price
                                        });
                                        avail.stocklist[i].adjusted += avail.stocklist[i].available;
                                        b2Bdesignbased.stock -= avail.stocklist[i].available;
                                    }
                                }
                            }

                            if (b2Bdesignbased.stock > 0)
                            {
                                if (avail.stocklist.Exists(e => e.iqty > b2Bdesignbased.stock))
                                {
                                    list.Add(new Fullstocksync.Stockdetail()
                                    {
                                        barcode = avail.stocklist.Find(e => e.iqty > b2Bdesignbased.stock).barcode,
                                        stock = b2Bdesignbased.stock,
                                        price = b2Bdesignbased.price
                                    });
                                    b2Bdesignbased.stock = 0;
                                }
                            }
                        }
                        if (b2Bdesignbased.stock > 0 && !isexist)
                        {
                            if (string.IsNullOrEmpty(b2Bdesignbased.productid)) continue;
                            if (string.IsNullOrEmpty(b2Bdesignbased.brandid)) continue;
                            if (string.IsNullOrEmpty(b2Bdesignbased.sizeid)) continue;
                            if (string.IsNullOrEmpty(b2Bdesignbased.design)) continue;
                            missingbarcodes.Add(b2Bdesignbased);
                        }
                    }

                }
                else
                {
                    foreach (var row in rows)
                    {
                        Fullstocksync.Stockdetail stockdetail = new Fullstocksync.Stockdetail();
                        stockdetail.barcode = row[0];
                        stockdetail.stock = onlybarcodes ? 1 : parsedecimal(row[1]);
                        list.Add(stockdetail);
                    }
                }
                item.stockdetail = list.GroupBy(e => e.barcode.Trim()).ToList().ConvertAll(e => new Fullstocksync.Stockdetail()
                {
                    barcode = e.Key,
                    price = e.First().price,
                    stock = e.Sum(x => x.stock)
                });
                item.missingb2b = missingbarcodes;
                item.errorb2b = errorList.Distinct().ToList();

                result.Add(Insert(item));
            }

            string whquery = @"

with currentstocklist 
as
(
select productid,sum(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) currentstock,sum((stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty))::numeric*ibuyingprice::numeric) currentcost,
sum((stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty))::numeric*stock.price::numeric) currentvalue
from stock
where stock.isactive=true and stock.companyid = @companyid
group by productid
),
newstocklist
as
(
select productid,sum(cast(schemelist->>'stock' as numeric)) newstock,
sum(cast(schemelist->>'stock' as numeric)*ibuyingprice::numeric) newcost,
sum(cast(schemelist->>'stock' as numeric)*stock.price::numeric) newvalue
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode' 
where stock.isactive=true and stock.companyid = @companyid
group by productid
)

select currentstocklist.*,newstocklist.*,products.name from currentstocklist
left outer join newstocklist on newstocklist.productid = currentstocklist.productid
left outer join products on products.id = currentstocklist.productid;

select schemelist->>'barcode' as barcode,cast(schemelist->>'stock' as numeric) as stock
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
left join stock on stock.barcode = schemelist->>'barcode'
where stock.isactive=true and fullstocksync.fileid = 8 and stock.companyid = @companyid and stock.barcode is null";


            string retaildetailquery = @"
with currentstocklist 
as
(
select productid,stock.designid,stock.itemid,sum(iqty-(oqty+dqty+jqty)) currentstock,sum((iqty-(oqty+dqty+jqty))::numeric*ibuyingprice::numeric) currentcost,
sum((iqty-(oqty+dqty+jqty))::numeric*salablegoods.price::numeric) currentvalue
from salablegoods
inner join stock on stock.id = stockid
where stock.isactive=true and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid and salablegoods.isactive=true
group by productid,stock.designid,stock.itemid
),
newstocklist
as
(
select productid,stock.designid,stock.itemid,sum(cast(schemelist->>'stock' as numeric)) newstock,
sum(cast(schemelist->>'stock' as numeric)*ibuyingprice::numeric) newcost,
sum(cast(schemelist->>'stock' as numeric)*salablegoods.price::numeric) newvalue
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode' and stock.isactive=true 
inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive=true
where fullstocksync.fileid=@fileid and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid
group by productid,stock.designid,stock.itemid
)

select currentstocklist.*,newstocklist.*,CONCAT(products.name,'-', brand.name,'-',referencelist.name,'-',currentstocklist.designid) as name from currentstocklist
left outer join newstocklist on newstocklist.productid = currentstocklist.productid
and  newstocklist.itemid = currentstocklist.itemid
and  newstocklist.designid = currentstocklist.designid
left outer join products on products.id = currentstocklist.productid
left outer join items on items.id = currentstocklist.itemid
left outer join referencelist on referencelist.id = items.sizeid
left outer join brand on brand.id = items.brandid;

select schemelist->>'barcode' as barcode,cast(schemelist->>'stock' as numeric) as stock
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode' and stock.isactive=true 
left join salablegoods on salablegoods.locationid=@locationid and salablegoods.companyid=@companyid 
and salablegoods.stockid = stock.id and salablegoods.isactive=true
where fullstocksync.fileid = @fileid and salablegoods.barcode is null";

            string retailquery = @"
with currentstocklist 
as
(
select productid,sum(iqty-(oqty+dqty+jqty)) currentstock,sum((iqty-(oqty+dqty+jqty))::numeric*ibuyingprice::numeric) currentcost,
sum((iqty-(oqty+dqty+jqty))::numeric*salablegoods.price::numeric) currentvalue
from salablegoods
inner join stock on stock.id = stockid
where stock.isactive=true and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid and salablegoods.isactive=true
group by productid
),
newstocklist
as
(
select productid,sum(cast(schemelist->>'stock' as numeric)) newstock,
sum(cast(schemelist->>'stock' as numeric)*ibuyingprice::numeric) newcost,
sum(cast(schemelist->>'stock' as numeric)*salablegoods.price::numeric) newvalue
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode'
inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive=true
where fullstocksync.fileid=@fileid and salablegoods.locationid=@locationid and salablegoods.companyid=@companyid
group by productid
)

select currentstocklist.*,newstocklist.*,products.name from currentstocklist
left outer join newstocklist on newstocklist.productid = currentstocklist.productid
left outer join products on products.id = currentstocklist.productid;

select schemelist->>'barcode' as barcode,cast(schemelist->>'stock' as numeric) as stock
from fullstocksync
LEFT JOIN LATERAL
json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
inner join stock on stock.barcode = schemelist->>'barcode'
left join salablegoods on salablegoods.locationid=@locationid and salablegoods.isactive=true
and salablegoods.companyid=@companyid and salablegoods.stockid = stock.id
where fullstocksync.fileid = @fileid and salablegoods.barcode is null";

            //            string missingstock = @"with sourcedata as
            //(
            //	select true,1,1,1,current_date,current_date,1,0,0,stock.id,
            //	stock.barcode,@locationid,1,stock.itemid,stock.companyid,true,stock.supplierid,0,
            //    current_date,stock.price,stock.discount,0,false,0,0,0,0,0,stock.buyingprice,stock.ibuyingprice 
            //    from fullstocksync
            //    LEFT JOIN LATERAL
            //    json_array_elements (fullstocksync.stockdetail :: json) schemelist ON true
            //    inner join stock on stock.barcode = schemelist->>'barcode'
            //    left join salablegoods on salablegoods.locationid=@locationid and salablegoods.companyid=@companyid and salablegoods.stockid = stock.id
            //    where salablegoods.barcode is null
            //)

            //INSERT INTO salablegoods(isactive, createdby, modifiedby, version, createdon, modifiedon, iqty, oqty, dqty, stockid, 
            //            barcode, locationid, bundleid, itemid,companyid,received,supplierid,refillboxid,inwardon,price,discount,
            //						additionaldiscount,ispurchaselocation,pqty,tiqty,toqty,rqty,salereturnqty,costprice,icostprice)";

            Data.ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = singleStockFile.locationid
            }).First();
            var searchquery = (locationinfo.additionalinfo != null && locationinfo.additionalinfo.iswarehouse) ? whquery : retailquery;

            if (isdesignbased)
                searchquery = retaildetailquery;

            if (singleStockFile.companyid == 0)
            {
                searchquery = searchquery.Replace("and salablegoods.companyid=@companyid", "");
                searchquery = searchquery.Replace("and stock.companyid = @companyid", "");
                searchquery = searchquery.Replace("where stock.companyid = @companyid", "");
            }
            var resultlist = new List<FullStockSummary.FullStockSummaryItem>();
            var missinglist = new List<FullStockSummary.MissingItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = new NpgsqlCommand(searchquery, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FullStockSummary.FullStockSummaryItem fullStockSummaryItem = new FullStockSummary.FullStockSummaryItem();
                        fullStockSummaryItem.currentcost = reader["currentcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentcost"]);
                        fullStockSummaryItem.currentstock = reader["currentstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentstock"]);
                        fullStockSummaryItem.currentvalue = reader["currentvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentvalue"]);
                        fullStockSummaryItem.newcost = reader["newcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newcost"]);
                        fullStockSummaryItem.newstock = reader["newstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newstock"]);
                        fullStockSummaryItem.newvalue = reader["newvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newvalue"]);
                        fullStockSummaryItem.product = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        resultlist.Add(fullStockSummaryItem);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            FullStockSummary.MissingItem fullStockSummaryItem = new FullStockSummary.MissingItem();
                            fullStockSummaryItem.qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                            fullStockSummaryItem.barcode = reader["barcode"] == DBNull.Value ? null : reader["barcode"].ToString();
                            missinglist.Add(fullStockSummaryItem);
                        }
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    command = new NpgsqlCommand(searchquery.Replace("inner join stock on stock.barcode = schemelist->>'barcode'",
                        @"inner join (select id,serialno,row_number() OVER (PARTITION BY serialno ORDER BY id DESC) as ranknumber from stock) uanstock on uanstock.serialno = schemelist->>'barcode'
                          inner join stock on stock.id = uanstock.id and uanstock.ranknumber=1"), conn);
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.locationid;
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.companyid;
                    command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = singleStockFile.id;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            FullStockSummary.FullStockSummaryItem fullStockSummaryItem = new FullStockSummary.FullStockSummaryItem();
                            fullStockSummaryItem.currentcost = reader["currentcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentcost"]);
                            fullStockSummaryItem.currentstock = reader["currentstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentstock"]);
                            fullStockSummaryItem.currentvalue = reader["currentvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["currentvalue"]);
                            fullStockSummaryItem.newcost = reader["newcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newcost"]);
                            fullStockSummaryItem.newstock = reader["newstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newstock"]);
                            fullStockSummaryItem.newvalue = reader["newvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newvalue"]);
                            fullStockSummaryItem.product = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                            if (resultlist.Exists(e => e.product == fullStockSummaryItem.product))
                            {
                                var oldproduct = resultlist.Find(e => e.product == fullStockSummaryItem.product);
                                //oldproduct.currentcost += fullStockSummaryItem.currentcost;
                                //oldproduct.currentstock += fullStockSummaryItem.currentstock;
                                //oldproduct.currentvalue += fullStockSummaryItem.currentvalue;
                                oldproduct.newcost += fullStockSummaryItem.newcost;
                                oldproduct.newstock += fullStockSummaryItem.newstock;
                                oldproduct.newvalue += fullStockSummaryItem.newvalue;
                            }
                            else
                            {
                                resultlist.Add(fullStockSummaryItem);
                            }
                        }
                        if (reader.NextResult())
                        {
                            var uanmissinglist = new List<FullStockSummary.MissingItem>();
                            while (reader.Read())
                            {
                                FullStockSummary.MissingItem fullStockSummaryItem = new FullStockSummary.MissingItem();
                                fullStockSummaryItem.qty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                                fullStockSummaryItem.barcode = reader["barcode"] == DBNull.Value ? null : reader["barcode"].ToString();
                                uanmissinglist.Add(fullStockSummaryItem);
                            }
                            missinglist.RemoveAll(e => !uanmissinglist.Exists(x => x.barcode == e.barcode));
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, singleStockFile);
            }


            if(singleStockFile.merge || singleStockFile.remove)
            {
                var mergedata = SearchFullStockConflict(singleStockFile);
                foreach(var prd in mergedata.GroupBy(x=>x.product))
                {
                    if(!resultlist.Exists(x=>x.product == prd.Key))
                    {
                        resultlist.Add(new FullStockSummary.FullStockSummaryItem()
                        {
                            product = prd.Key,
                        });
                    }
                    var prditem = resultlist.Find(x => x.product == prd.Key);
                    prditem.remove = prd.Sum(x => x.removeqty);
                    prditem.merge = prd.Sum(x => x.mergeqty);
                }
            }

            return new FullStockSummary()
            {
                missing = missinglist,
                summary = resultlist,
                errorb2b = result[0].errorb2b,
                missingb2b = result[0].missingb2b
            };
        }

        public decimal parsedecimal(string value)
        {
            decimal d;
            if (decimal.TryParse(value, out d))
                return d;
            return 1;
        }
        public Fullstocksync Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Fullstocksync oFullstocksync)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("fileid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFullstocksync.fileid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFullstocksync.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFullstocksync.locationid;
            command.Parameters.Add("synced", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFullstocksync.synced;
            command.Parameters.Add("rollbacked", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFullstocksync.rollbacked;
            command.Parameters.Add("stockdetail", NpgsqlTypes.NpgsqlDbType.Json).Value = oFullstocksync.stockdetail_json == null ? "null" : oFullstocksync.stockdetail_json;
            command.Parameters.Add("errorb2b", NpgsqlTypes.NpgsqlDbType.Json).Value = oFullstocksync.errorb2b_json == null ? "null" : oFullstocksync.errorb2b_json;
            command.Parameters.Add("missingb2b", NpgsqlTypes.NpgsqlDbType.Json).Value = oFullstocksync.missingb2b_json == null ? "null" : oFullstocksync.missingb2b_json;
            command.ExecuteNonQuery();
            return oFullstocksync;
        }



    }

}
