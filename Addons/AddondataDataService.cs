using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class AddondataDataService : BaseData
    {
        const string SQL_Search = @"select isactive, modifiedby, customdata, companyid, referenceid, modifiedon, id, createdby, createdon, version, customtype from addondata where id=@id";

        const string SQL_Insert = @"INSERT INTO public.addondata(isactive, modifiedby, customdata, companyid, referenceid, modifiedon, createdby, createdon, version, customtype) VALUES(@isactive, @modifiedby, @customdata, @companyid, @referenceid, @modifiedon, @createdby, @createdon, @version, @customtype) RETURNING id";

        const string SQL_Update = @"UPDATE public.addondata SET modifiedby=@modifiedby, customdata=@customdata, companyid=@companyid, referenceid=@referenceid, modifiedon=@modifiedon, version=version+1, customtype=@customtype where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.addondata set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Addondata> Search(AddondataCreteria _addondata)
        {
            List<Addondata> lst = new List<Addondata>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_addondata.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _addondata.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_addondata.customtype))
                        qb.AddParameter("customtype", _addondata.customtype, "=");

                    if (_addondata.referenceid > 0)
                        qb.AddParameter("referenceid", _addondata.referenceid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Addondata oaddondata = new Addondata();
                        oaddondata.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oaddondata.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oaddondata.customdata_json = reader["customdata"] == DBNull.Value ? null : reader["customdata"].ToString();
                        oaddondata.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oaddondata.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                        oaddondata.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oaddondata.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oaddondata.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oaddondata.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oaddondata.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oaddondata.customtype = reader["customtype"] == DBNull.Value ? "" : reader["customtype"].ToString();
                        lst.Add(oaddondata);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _addondata);
            }
            return lst;
        }

        public Addondata Insert(Addondata oAddondata)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var param = new SortedList<string, object>();
                    param.Add("companyid", oAddondata.companyid);
                    param.Add("referenceid", oAddondata.referenceid);
                    param.Add("customtype", oAddondata.customtype);

                    using (System.Data.IDataReader reader = ExecuteReader("update addondata set version=version+1 where companyid=@companyid and referenceid=@referenceid and customtype=@customtype and isactive=true RETURNING id", param, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            oAddondata.id = Convert.ToInt64(reader[0].ToString());
                        }
                    }
                    if (oAddondata.id > 0)
                    {
                        throw new Exception("This configuration already exist,Please referesh and try again!");
                    }
                    else
                    {
                        Insert(transaction, conn, oAddondata);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAddondata);
            }

            return oAddondata;
        }

        public Addondata Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Addondata oAddondata)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAddondata.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("customdata", NpgsqlTypes.NpgsqlDbType.Json).Value = oAddondata.customdata_json == null ? "null" : oAddondata.customdata_json;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.companyid;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.referenceid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAddondata.version;
            command.Parameters.Add("customtype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAddondata.customtype == null ? "" : oAddondata.customtype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAddondata.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oAddondata;
        }

        public Addondata Update(Addondata oAddondata)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oAddondata);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAddondata);
            }

            return oAddondata;
        }

        public Addondata Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Addondata oAddondata)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAddondata.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("customdata", NpgsqlTypes.NpgsqlDbType.Json).Value = oAddondata.customdata_json == null ? "null" : oAddondata.customdata_json;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.companyid;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.referenceid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.id;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAddondata.version;
            command.Parameters.Add("customtype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oAddondata.customtype == null ? "" : oAddondata.customtype;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oAddondata.version = Convert.ToInt32(reader[0].ToString());
                }
                else
                {
                    throw new Exception("Already some one changed the data, please reload and save the screen");
                }
            }

            return oAddondata;
        }


        public bool Delete(Addondata oAddondata)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oAddondata);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oAddondata);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Addondata oAddondata)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oAddondata.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oAddondata.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oAddondata.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oAddondata.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<Addon_ReportTargetMargin.ReportItem> ReportTargetMargin(Addon_ReportTargetMargin item)
        {

            List<Addon_ReportTargetMargin.ReportItem> reportItems = new List<Addon_ReportTargetMargin.ReportItem>();
            string branchStockquery = @"select stock.productid,products.name,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.buyingprice::numeric) as buyingprice,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.ibuyingprice::numeric) as ibuyingprice
                                        from salablegoods 
                                inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                                inner join products on products.id = stock.productid and products.salescounterid=@salescounterid
                                where salablegoods.isactive = true 
                                and salablegoods.hasstock = true and salablegoods.locationid=@locationid
                                and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0
                                group by stock.productid,products.name";

            var param = new SortedList<string, object>();
            param.Add("salescounterid", item.sectionid);
            param.Add("locationid", item.locationid);
            param.Add("fromdate", item.salesfromdate);
            param.Add("todate", item.salestodate.AddDays(1));

            param.Add("targetfromdate", item.targetfrom);
            param.Add("targettodate", item.targettodate.AddDays(1));
            using (var reader = ExecuteReader(branchStockquery, param))
            {
                while (reader.Read())
                {
                    Addon_ReportTargetMargin.ReportItem reportItem = new Addon_ReportTargetMargin.ReportItem();
                    reportItem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    reportItem.productname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                    reportItem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    reportItem.stockbuyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    reportItem.stockibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                    reportItems.Add(reportItem);
                }
            }

            string branchStockMarkerQuery = @"select stock.productid,products.name,
                                        referencelist.id markerid,referencelist.name markername,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.buyingprice::numeric) as buyingprice,
                                        sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) * stock.ibuyingprice::numeric) as ibuyingprice
                    from stockmarker
                    inner join referencelist on referencelist.id = markerid
                    inner join salablegoods on salablegoods.id = stockmarker.salablegoodsid and salablegoods.locationid=@locationid
                    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                    inner join products on products.id = stock.productid and products.salescounterid=@salescounterid
                    where stockmarker.isactive=true and salablegoods.isactive = true
                    group by stock.productid,products.name,referencelist.id,referencelist.name";

            using (var reader = ExecuteReader(branchStockMarkerQuery, param))
            {
                while (reader.Read())
                {
                    Addon_ReportTargetMargin.ReportItem reportItem = new Addon_ReportTargetMargin.ReportItem();
                    reportItem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    reportItem.productname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                    if (reportItems.Exists(x => x.productid == reportItem.productid))
                    {
                        reportItem = reportItems.Find(x => x.productid == reportItem.productid);
                    }
                    else
                    {
                        reportItems.Add(reportItem);
                    }
                    if (reportItem.markers == null)
                        reportItem.markers = new List<Addon_ReportTargetMargin.ReportItem.ReportMarkerItem>();
                    var markeritem = new Addon_ReportTargetMargin.ReportItem.ReportMarkerItem();
                    markeritem.id = reader["markerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["markerid"]);
                    markeritem.name = reader["markername"] == DBNull.Value ? "" : Convert.ToString(reader["markername"]);
                    markeritem.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                    markeritem.stockbuyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    markeritem.stockibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                    reportItem.markers.Add(markeritem);
                }
            }

            string branchtargetSalesQuery = @"select billitems.productid,products.name,sum(billitems.qty) saleqty, sum(billitems.receivable-billitems.tax) salevalue, 
                    sum(billitems.receivable) saleivalue,
                    sum(billitems.qty * stock.buyingprice::numeric) as buyingprice,
                    sum(billitems.qty * stock.ibuyingprice::numeric) as ibuyingprice
                    from bill
                    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                    inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                    inner join products on products.id = stock.productid and products.salescounterid=@salescounterid
                    where bill.billdate>=@targetfromdate and bill.billdate<@targettodate and bill.billtype != 9 and bill.isactive=true
                    and bill.locationid = @locationid
                    group by billitems.productid,products.name";

            using (var reader = ExecuteReader(branchtargetSalesQuery, param))
            {
                while (reader.Read())
                {
                    Addon_ReportTargetMargin.ReportItem reportItem = new Addon_ReportTargetMargin.ReportItem();
                    reportItem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    reportItem.productname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                    if (reportItems.Exists(x => x.productid == reportItem.productid))
                    {
                        reportItem = reportItems.Find(x => x.productid == reportItem.productid);
                    }
                    else
                    {
                        reportItems.Add(reportItem);
                    }
                    reportItem.targetqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                    reportItem.targetvalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                    reportItem.targetivalue = reader["saleivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleivalue"]);
                    reportItem.targetcost = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    reportItem.targeticost = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                }
            }

            string branchSalesQuery = @"select billitems.productid,products.name,sum(billitems.qty) saleqty, sum(billitems.receivable-billitems.tax) salevalue, 
                    sum(billitems.receivable) saleivalue,
                    sum(billitems.qty * stock.buyingprice::numeric) as buyingprice,
                    sum(billitems.qty * stock.ibuyingprice::numeric) as ibuyingprice
                    from bill
                    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                    inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                    inner join products on products.id = stock.productid and products.salescounterid=@salescounterid
                    where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive=true
                    and bill.locationid = @locationid
                    group by billitems.productid,products.name";

            using (var reader = ExecuteReader(branchSalesQuery, param))
            {
                while (reader.Read())
                {
                    Addon_ReportTargetMargin.ReportItem reportItem = new Addon_ReportTargetMargin.ReportItem();
                    reportItem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    reportItem.productname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                    if (reportItems.Exists(x => x.productid == reportItem.productid))
                    {
                        reportItem = reportItems.Find(x => x.productid == reportItem.productid);
                    }
                    else
                    {
                        reportItems.Add(reportItem);
                    }
                    reportItem.salesqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                    reportItem.salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                    reportItem.saleivalue = reader["saleivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleivalue"]);
                    reportItem.salescost = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    reportItem.salesicost = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                }
            }


            string branchMarkerSalesQuery = @"
                    select billitems.productid,products.name,
                    referencelist.id markerid,referencelist.name markername,
                    sum(billitems.qty) saleqty, sum(billitems.receivable-billitems.tax) salevalue, 
                    sum(billitems.receivable) saleivalue
                    from bill
                    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                    inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                    inner join stock on stock.id = salablegoods.stockid and stock.isactive = true					
                    inner join products on products.id = stock.productid and products.salescounterid=@salescounterid
                    inner join stockmarker on stockmarker.salablegoodsid = billitems.salablegoodsid
                    inner join referencelist on referencelist.id = markerid
                    where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive=true
                    and bill.locationid = @locationid
                    group by billitems.productid,products.name,referencelist.id,markername";


            using (var reader = ExecuteReader(branchMarkerSalesQuery, param))
            {
                while (reader.Read())
                {
                    Addon_ReportTargetMargin.ReportItem reportItem = new Addon_ReportTargetMargin.ReportItem();
                    reportItem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    reportItem.productname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                    if (reportItems.Exists(x => x.productid == reportItem.productid))
                    {
                        reportItem = reportItems.Find(x => x.productid == reportItem.productid);
                    }
                    else
                    {
                        reportItems.Add(reportItem);
                    }

                    if (reportItem.markers == null) reportItem.markers = new List<Addon_ReportTargetMargin.ReportItem.ReportMarkerItem>();
                    Addon_ReportTargetMargin.ReportItem.ReportMarkerItem reportMarkerItem = new Addon_ReportTargetMargin.ReportItem.ReportMarkerItem();
                    reportMarkerItem.id = reader["markerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["markerid"]);
                    reportMarkerItem.name = reader["markername"] == DBNull.Value ? "" : Convert.ToString(reader["markername"]);
                    if (reportItem.markers.Exists(x => x.id == reportMarkerItem.id))
                    {
                        reportMarkerItem = reportItem.markers.Find(x => x.id == reportMarkerItem.id);
                    }
                    else
                    {
                        reportItem.markers.Add(reportMarkerItem);
                    }
                    reportMarkerItem.salesqty = reader["saleqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleqty"]);
                    reportMarkerItem.salevalue = reader["salevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salevalue"]);
                    reportMarkerItem.saleivalue = reader["saleivalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleivalue"]);
                    reportItem.markers.Add(reportMarkerItem);
                }
            }

            if (item.productids != null &&
                item.productids.Count > 0)
                reportItems.RemoveAll(x => !item.productids.Contains(x.productid));

            return reportItems;
        }
    }

}
