using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Processors;
using WebAPI.Data;

namespace InfyPOS.Data

{
    public class EcombillDataService : BaseData
    {
        const string SQL_Search = @"select ecombill.id, ecombill.isactive, ecombill.modifiedon, ecombill.createdon, ecombill.createdby, ecombill.modifiedby,
        orderdate, unitprice, orderqty, voucheramount, lineamount, maxretailprice, ecombill.version, customername, 
        shipaddress, shipcity, shippincode, shipphone, color, brand, size, itemname, address, shipstate, shippingpackagestatus, statuscode, orderno, company, location, 
		channel, externorderno, cancellationreason, sku,ShipCode,dispatchon,InvoiceCode,InvoiceDisplayCode,
        coalesce(billecom.billno,billorder.billno) billno,
        coalesce(billecom.billdate,billorder.billdate) billdate,
		coalesce(billecom.createdon,billorder.createdon) importedon,
		coalesce(billecom.createdby,billorder.createdby) importedby
		from ecombill 
	    left outer join bill billecom on billecom.ecomorderno != '' and (billecom.ecomorderno=ecombill.orderno and billecom.billattributes->'ecom'->>'orderid' = ecombill.externorderno)
		--left outer join bill billorder on billorder.ecomorderno != '' and (billorder.ecomorderno=ecombill.externorderno and billorder.billattributes->'ecom'->>'orderid' = ecombill.externorderno)
        left outer join bill billorder on billorder.ecomorderno != '' and (billorder.billattributes->'ecom'->>'invoicecode' = InvoiceCode)
        where ecombill.id=@id";

        const string SQL_Insert = @"INSERT INTO public.ecombill(isactive, modifiedon, createdon, createdby, modifiedby, orderdate, unitprice, orderqty, voucheramount, lineamount, maxretailprice, 
    version, customername, shipaddress, shipcity, shippincode, shipphone, color, brand, size, itemname, address, shipstate, shippingpackagestatus, statuscode, orderno, company, location, channel, 
    externorderno, cancellationreason, sku,ShipCode,dispatchon,InvoiceCode,InvoiceDisplayCode) 
    VALUES(@isactive, @modifiedon, @createdon, @createdby, @modifiedby, @orderdate, @unitprice, @orderqty, @voucheramount, @lineamount, @maxretailprice, @version, @customername, @shipaddress, 
    @shipcity, @shippincode, @shipphone, @color, @brand, @size, @itemname, @address, @shipstate, @shippingpackagestatus, @statuscode, @orderno, @company, @location, @channel, @externorderno, 
    @cancellationreason, @sku,@ShipCode,@dispatchon,@InvoiceCode,@InvoiceDisplayCode) RETURNING id";

        const string SQL_Update = @"UPDATE public.ecombill SET modifiedon=@modifiedon, modifiedby=@modifiedby, orderdate=@orderdate, unitprice=@unitprice, orderqty=@orderqty, voucheramount=@voucheramount,
    lineamount=@lineamount, maxretailprice=@maxretailprice, version=@version+1, customername=@customername, shipaddress=@shipaddress, shipcity=@shipcity, shippincode=@shippincode, shipphone=@shipphone, 
    color=@color, brand=@brand, size=@size, itemname=@itemname, address=@address, shipstate=@shipstate, shippingpackagestatus=@shippingpackagestatus, statuscode=@statuscode, orderno=@orderno,
    ShipCode,dispatchon,InvoiceCode,InvoiceDisplayCode,company=@company, location=@location, channel=@channel, externorderno=@externorderno, cancellationreason=@cancellationreason, sku=@sku 
    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.ecombill set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Ecombill> Search(EcombillCreteria _ecombill)
        {
            List<Ecombill> lst = new List<Ecombill>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_ecombill.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _ecombill.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("ecombill.isactive", true, "=");

                    if (string.IsNullOrEmpty(_ecombill.datetype) || _ecombill.datetype == "ORDERDATE")
                    {
                        qb.AddParameter("orderdate", "fromdate", _ecombill.addedfrom, ">=");
                        qb.AddParameter("orderdate", "todate", _ecombill.addedto.AddDays(1), "<");
                    }
                    else
                    {
                        qb.AddParameter("dispatchon", "fromdate", _ecombill.addedfrom, ">=");
                        qb.AddParameter("dispatchon", "todate", _ecombill.addedto.AddDays(1), "<");
                    }
                    if (_ecombill.status == "UNBILLED")
                        qb.Add2Query("billecom.id is null and billorder.id is null");
                    else if (_ecombill.status == "BILLED")
                        qb.Add2Query("(billecom.id >0 or billorder.id >0)");
                    else if (_ecombill.status == "RETURNS")
                        qb.Add2Query("(ecombill.orderqty<0)");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ecombill oecombill = new Ecombill();
                        oecombill.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oecombill.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oecombill.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oecombill.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oecombill.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oecombill.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oecombill.orderdate = reader["orderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["orderdate"]);
                        oecombill.unitprice = reader["unitprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["unitprice"]);
                        oecombill.orderqty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                        oecombill.voucheramount = reader["voucheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["voucheramount"]);
                        oecombill.lineamount = reader["lineamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lineamount"]);
                        oecombill.maxretailprice = reader["maxretailprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxretailprice"]);
                        oecombill.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oecombill.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        oecombill.shipaddress = reader["shipaddress"] == DBNull.Value ? "" : reader["shipaddress"].ToString();
                        oecombill.shipcity = reader["shipcity"] == DBNull.Value ? "" : reader["shipcity"].ToString();
                        oecombill.shippincode = reader["shippincode"] == DBNull.Value ? "" : reader["shippincode"].ToString();
                        oecombill.shipphone = reader["shipphone"] == DBNull.Value ? "" : reader["shipphone"].ToString();
                        oecombill.color = reader["color"] == DBNull.Value ? "" : reader["color"].ToString();
                        oecombill.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        oecombill.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oecombill.itemname = reader["itemname"] == DBNull.Value ? "" : reader["itemname"].ToString();
                        oecombill.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        oecombill.shipstate = reader["shipstate"] == DBNull.Value ? "" : reader["shipstate"].ToString();
                        oecombill.shippingpackagestatus = reader["shippingpackagestatus"] == DBNull.Value ? "" : reader["shippingpackagestatus"].ToString();
                        oecombill.statuscode = reader["statuscode"] == DBNull.Value ? "" : reader["statuscode"].ToString();
                        oecombill.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                        oecombill.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                        oecombill.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        oecombill.channel = reader["channel"] == DBNull.Value ? "" : reader["channel"].ToString();

                        oecombill.externorderno = reader["externorderno"] == DBNull.Value ? "" : reader["externorderno"].ToString();
                        oecombill.cancellationreason = reader["cancellationreason"] == DBNull.Value ? "" : reader["cancellationreason"].ToString();
                        oecombill.sku = reader["sku"] == DBNull.Value ? "" : reader["sku"].ToString();

                        oecombill.dispatchon = reader["dispatchon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dispatchon"]);
                        oecombill.invoicedisplaycode = reader["InvoiceDisplayCode"] == DBNull.Value ? "" : reader["InvoiceDisplayCode"].ToString();
                        oecombill.invoicecode = reader["InvoiceCode"] == DBNull.Value ? "" : reader["InvoiceCode"].ToString();
                        oecombill.shipcode = reader["shipcode"] == DBNull.Value ? "" : reader["shipcode"].ToString();

                        var billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        var billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        if (!string.IsNullOrEmpty(billno) && billdate == oecombill.dispatchon)
                        {
                            oecombill.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                            oecombill.importedon = reader["importedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["importedon"]);
                            oecombill.importedby = reader["importedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["importedby"]);
                        }

                        lst.Add(oecombill);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ecombill);
            }

            EmployeeDataService employeeDataService = new EmployeeDataService();
            var namelist = employeeDataService.GetUserName(lst.ConvertAll(x => x.importedby));
            lst.ForEach(x =>
            {
                if (x.importedby > 0 & namelist.Exists(y => y.id == x.importedby))
                    x.imported = namelist.Find(y => y.id == x.importedby).name;
            });
            return lst;
        }

        public List<Ecombill> Insert(List<Ecombill> oEcombill)
        {

            List<long> duplicateids = new List<long>();
            NpgsqlTransaction transaction = null;
            try
            {

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var items in oEcombill.ChunkBy(100))
                    {

                        string billnoquery = @"select id,externorderno from Ecombill where externorderno in (" +
                string.Join(",", items.ConvertAll(x => "'" + x.externorderno + "'")) + ")";
                        using (var reader = ExecuteReader(billnoquery, null))
                        {
                            while (reader.Read())
                            {
                                var ecomorderno = reader["externorderno"] == DBNull.Value ? "" : reader["externorderno"].ToString();
                                long orderid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                                items.Find(x => x.externorderno == ecomorderno).id = orderid;
                            }
                        }

                        foreach (var itm in items)
                        {
                            itm.isactive = true;
                            itm.version = 1;
                            itm.createdby = Context.UserID;
                            itm.createdon = DateTimeNow;
                            itm.modifiedby = Context.UserID;
                            itm.modifiedon = DateTimeNow;
                            if (itm.id == 0 && itm.statuscode == "COMPLETE")
                            {
                                var param = new SortedList<string, object>();
                                param.Add("orderno", itm.orderno);
                                param.Add("externorderno", itm.externorderno);
                                param.Add("ShipCode", itm.shipcode);
                                param.Add("orderdate", itm.orderdate);
                                param.Add("DIspatchOn", itm.dispatchon);
                                param.Add("InvoiceCode", itm.invoicecode);
                                param.Add("orderqty", itm.orderqty);
                                using (var reader = ExecuteReader(@"Select id from ecombill where
                                orderno=@orderno and externorderno=@externorderno and
                                ShipCode=@ShipCode and orderdate=@orderdate and DIspatchOn=@DIspatchOn and 
                                InvoiceCode=@InvoiceCode and orderqty=@orderqty", param, conn, transaction))
                                {
                                    if (reader.Read())
                                    {
                                        itm.id = reader.GetInt64(0);
                                        duplicateids.Add(itm.id);
                                    }
                                }
                                if (itm.id == 0)
                                    Insert(transaction, conn, itm);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEcombill);
            }
            if (duplicateids.Count > 0)
            {
                var duplicates = Search(new EcombillCreteria()
                {
                    ids = duplicateids
                });
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Duplicate identified - Ecombill"), WebAPI.Common.Logger.LogType.Sync, duplicates);
            }
            return oEcombill;
        }

        public Ecombill Insert(Ecombill oEcombill)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oEcombill);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEcombill);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oEcombill;
        }

        public Ecombill Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Ecombill oEcombill)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEcombill.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.modifiedby;
            command.Parameters.Add("orderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.orderdate;
            command.Parameters.Add("unitprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.unitprice;
            command.Parameters.Add("orderqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.orderqty;
            command.Parameters.Add("voucheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.voucheramount;
            command.Parameters.Add("lineamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.lineamount;
            command.Parameters.Add("maxretailprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.maxretailprice;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEcombill.version;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.customername == null ? "" : oEcombill.customername;
            command.Parameters.Add("shipaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipaddress == null ? "" : oEcombill.shipaddress;
            command.Parameters.Add("shipcity", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipcity == null ? "" : oEcombill.shipcity;
            command.Parameters.Add("shippincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shippincode == null ? "" : oEcombill.shippincode;
            command.Parameters.Add("shipphone", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipphone == null ? "" : oEcombill.shipphone;
            command.Parameters.Add("color", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.color == null ? "" : oEcombill.color;
            command.Parameters.Add("brand", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.brand == null ? "" : oEcombill.brand;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.size == null ? "" : oEcombill.size;
            command.Parameters.Add("itemname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.itemname == null ? "" : oEcombill.itemname;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.address == null ? "" : oEcombill.address;
            command.Parameters.Add("shipstate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipstate == null ? "" : oEcombill.shipstate;
            command.Parameters.Add("shippingpackagestatus", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shippingpackagestatus == null ? "" : oEcombill.shippingpackagestatus;
            command.Parameters.Add("statuscode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.statuscode == null ? "" : oEcombill.statuscode;
            command.Parameters.Add("orderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.orderno == null ? "" : oEcombill.orderno;
            command.Parameters.Add("company", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.company == null ? "" : oEcombill.company;
            command.Parameters.Add("location", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.location == null ? "" : oEcombill.location;
            command.Parameters.Add("channel", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.channel == null ? "" : oEcombill.channel;
            command.Parameters.Add("externorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.externorderno == null ? "" : oEcombill.externorderno;
            command.Parameters.Add("cancellationreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.cancellationreason == null ? "" : oEcombill.cancellationreason;
            command.Parameters.Add("sku", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.sku == null ? "" : oEcombill.sku;

            command.Parameters.Add("shipcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipcode == null ? "" : oEcombill.shipcode;
            command.Parameters.Add("invoicecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.invoicecode == null ? "" : oEcombill.invoicecode;
            command.Parameters.Add("invoicedisplaycode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.invoicedisplaycode == null ? "" : oEcombill.invoicedisplaycode;
            command.Parameters.Add("dispatchon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.dispatchon;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEcombill.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEcombill;
        }

        public Ecombill Update(Ecombill oEcombill)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEcombill);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEcombill);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oEcombill;
        }

        public Ecombill Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Ecombill oEcombill)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEcombill.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.modifiedby;
            command.Parameters.Add("orderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.orderdate;
            command.Parameters.Add("unitprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.unitprice;
            command.Parameters.Add("orderqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.orderqty;
            command.Parameters.Add("voucheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.voucheramount;
            command.Parameters.Add("lineamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.lineamount;
            command.Parameters.Add("maxretailprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEcombill.maxretailprice;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEcombill.version;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.customername == null ? "" : oEcombill.customername;
            command.Parameters.Add("shipaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipaddress == null ? "" : oEcombill.shipaddress;
            command.Parameters.Add("shipcity", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipcity == null ? "" : oEcombill.shipcity;
            command.Parameters.Add("shippincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shippincode == null ? "" : oEcombill.shippincode;
            command.Parameters.Add("shipphone", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipphone == null ? "" : oEcombill.shipphone;
            command.Parameters.Add("color", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.color == null ? "" : oEcombill.color;
            command.Parameters.Add("brand", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.brand == null ? "" : oEcombill.brand;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.size == null ? "" : oEcombill.size;
            command.Parameters.Add("itemname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.itemname == null ? "" : oEcombill.itemname;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.address == null ? "" : oEcombill.address;
            command.Parameters.Add("shipstate", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipstate == null ? "" : oEcombill.shipstate;
            command.Parameters.Add("shippingpackagestatus", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shippingpackagestatus == null ? "" : oEcombill.shippingpackagestatus;
            command.Parameters.Add("statuscode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.statuscode == null ? "" : oEcombill.statuscode;
            command.Parameters.Add("orderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.orderno == null ? "" : oEcombill.orderno;
            command.Parameters.Add("company", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.company == null ? "" : oEcombill.company;
            command.Parameters.Add("location", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.location == null ? "" : oEcombill.location;
            command.Parameters.Add("channel", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.channel == null ? "" : oEcombill.channel;
            command.Parameters.Add("externorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.externorderno == null ? "" : oEcombill.externorderno;
            command.Parameters.Add("cancellationreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.cancellationreason == null ? "" : oEcombill.cancellationreason;
            command.Parameters.Add("sku", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.sku == null ? "" : oEcombill.sku;
            command.Parameters.Add("shipcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.shipcode == null ? "" : oEcombill.shipcode;
            command.Parameters.Add("invoicecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.invoicecode == null ? "" : oEcombill.invoicecode;
            command.Parameters.Add("invoicedisplaycode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEcombill.invoicedisplaycode == null ? "" : oEcombill.invoicedisplaycode;
            command.Parameters.Add("dispatchon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEcombill.dispatchon;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEcombill.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEcombill;
        }


        public bool Delete(Ecombill oEcombill)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEcombill);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEcombill);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Ecombill oEcombill)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEcombill.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oEcombill.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEcombill.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEcombill.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public Configuration.UnicommerceConfig GetUnicommerceConfig()
        {
            var uniconfig = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.unicommerce;
            if (uniconfig != null &&
                !uniconfig.dbdecoded &&
               uniconfig.channels != null &&
               uniconfig.channels.Count > 0 &&
               uniconfig.channels.Exists(x => string.IsNullOrEmpty(x.company)))
            {
                BaseData baseData = new BaseData();
                using (var reader = baseData.ExecuteReader("select id,name from company where id in (" + string.Join(",", uniconfig.channels.ConvertAll(x => x.companyid)) + ")", null))
                {
                    while (reader.Read())
                    {
                        var cmpid = Convert.ToInt64(reader["id"]);
                        var cmname = Convert.ToString(reader["name"]);
                        uniconfig.channels.ForEach(x =>
                        {
                            if (x.companyid == cmpid)
                            {
                                x.company = cmname;
                            }
                        });
                    }
                }
                using (var reader = baseData.ExecuteReader("select id,name from referencelist where id in (" + string.Join(",", uniconfig.channels.ConvertAll(x => x.locationid)) + ")", null))
                {
                    while (reader.Read())
                    {
                        var cmpid = Convert.ToInt64(reader["id"]);
                        var cmname = Convert.ToString(reader["name"]);
                        uniconfig.channels.ForEach(x =>
                        {
                            if (x.locationid == cmpid)
                            {
                                x.location = cmname;
                            }
                        });
                    }
                }
            }
            return uniconfig;
        }

        internal void SyncOnlineStock(List<Unicomerce.QuantoSKUStcok> quantoSKUStcoks, int minutes)
        {
            var uniconfig = GetUnicommerceConfig();
            if (uniconfig.access_validto == DateTime.MinValue ||
                DateTime.UtcNow > uniconfig.access_validto)
            {
                var token = Unicomerce.Authenticate(uniconfig.serverurl, uniconfig.username, uniconfig.password);
                if (token != null && token.access_token != null)
                {
                    uniconfig.access_token = token.access_token;
                    uniconfig.access_validto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(token.expires_in).DateTime.ToUniversalTime().AddMinutes(-5);
                }
            }

            if (minutes > 0)
            {
                List<Unicomerce.QuantoSKUStcok> changed = new List<Unicomerce.QuantoSKUStcok>();
                var response = Unicomerce.GetSnapshot(uniconfig.serverurl, uniconfig.access_token, minutes, null);
                if (response.inventorySnapshots != null)
                {
                    foreach (var item in response.inventorySnapshots)
                    {
                        if (quantoSKUStcoks.Exists(x => x.prdsku == item.itemTypeSKU))
                        {
                            var qstk = quantoSKUStcoks.Find(x => x.prdsku == item.itemTypeSKU);
                            qstk.vendorInventory = item.vendorInventory;
                            qstk.openPurchase = item.openPurchase;
                            qstk.inventory = item.inventory;
                            qstk.inventoryBlocked = item.inventoryBlocked;
                            qstk.openPurchase = item.openPurchase;
                            qstk.openSale = item.openSale;
                            qstk.pendingInventoryAssessment = item.pendingInventoryAssessment;
                            qstk.pendingStockTransfer = item.pendingStockTransfer;
                            qstk.putawayPending = item.putawayPending;
                            qstk.virtualInventory = item.virtualInventory;
                            if (qstk.inventory > qstk.stk)
                                qstk.status = "H";
                            else if (qstk.inventory < qstk.stk)
                                qstk.status = "L";
                            else
                                qstk.status = "E";
                            changed.Add(qstk);
                        }
                    }
                }
                quantoSKUStcoks.Clear();
                quantoSKUStcoks.AddRange(changed);
            }
            else
            {
                foreach (var skulist in quantoSKUStcoks.ChunkBy(2500))
                {
                    skulist.ForEach(x => x.status = "M");
                    var response = Unicomerce.GetSnapshot(uniconfig.serverurl, uniconfig.access_token, minutes, skulist.ConvertAll(x => x.prdsku).ToList());
                    if (response.inventorySnapshots != null)
                    {
                        foreach (var item in response.inventorySnapshots)
                        {
                            if (quantoSKUStcoks.Exists(x => x.prdsku == item.itemTypeSKU))
                            {
                                var qstk = quantoSKUStcoks.Find(x => x.prdsku == item.itemTypeSKU);
                                qstk.vendorInventory = item.vendorInventory;
                                qstk.openPurchase = item.openPurchase;
                                qstk.inventory = item.inventory;
                                qstk.inventoryBlocked = item.inventoryBlocked;
                                qstk.openPurchase = item.openPurchase;
                                qstk.openSale = item.openSale;
                                qstk.pendingInventoryAssessment = item.pendingInventoryAssessment;
                                qstk.pendingStockTransfer = item.pendingStockTransfer;
                                qstk.putawayPending = item.putawayPending;
                                qstk.virtualInventory = item.virtualInventory;
                                if (qstk.inventory > qstk.stk)
                                    qstk.status = "H";
                                else if (qstk.inventory < qstk.stk)
                                    qstk.status = "L";
                                else
                                    qstk.status = "E";
                            }
                        }
                    }
                }
            }
        }
        internal List<Ecombill> SearchOnline(EcombillCreteria item)
        {
            if (item == null) return null;
            var uniconfig = GetUnicommerceConfig();
            if (uniconfig.access_validto == DateTime.MinValue ||
                DateTime.UtcNow > uniconfig.access_validto)
            {
                var token = Unicomerce.Authenticate(uniconfig.serverurl, uniconfig.username, uniconfig.password);
                if (token != null && token.access_token != null)
                {
                    uniconfig.access_token = token.access_token;
                    uniconfig.access_validto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(token.expires_in).DateTime.ToUniversalTime().AddMinutes(-5);
                }
            }

            var sos = Unicomerce.SearchSalesOrder(uniconfig.serverurl, uniconfig.access_token,
                item.addedfrom, item.addedto, item.status, true, 
                string.IsNullOrEmpty(uniconfig.facility)?new string[] { }: uniconfig.facility.Split(','));

            var duplicatesso = sos.elements.ToList().GroupBy(x => x.code).ToList().FindAll(x => x.Count() > 1);

            List<ECOMSalesOrderItem> soitem = new List<ECOMSalesOrderItem>();
            foreach (var itm in sos.elements)
            {
                soitem.AddRange(Unicomerce.GetECOMSalesOrderItems(uniconfig.serverurl, uniconfig.access_token, uniconfig, itm));
            }

            if (item.status == "RETURNS")
            {
                soitem.RemoveAll(x => x.OrderQty > 0);
            }

            var duplicatessoitems = soitem.GroupBy(x => x.OrderNo).ToList().FindAll(x => x.Count() > 1);
            duplicatessoitems.ForEach(x =>
            {
                int index = 0;
                foreach (var itm in x)
                {
                    if (index > 0)
                        itm.OrderNo += "#" + index;
                    index++;
                }
            });
            //var sosnoncredit = Unicomerce.SearchSalesOrder(uniconfig.serverurl, uniconfig.access_token,
            //item.addedfrom, item.addedto, item.status, false);

            //foreach (var itm in sosnoncredit.elements)
            //{
            //    soitem.AddRange(Unicomerce.GetECOMSalesOrderItems(uniconfig.serverurl, uniconfig.access_token, uniconfig, itm));
            //}

            //if (soitem.Count > 0)
            //{
            //    string billnoquery = @"select ecomorderno,billno,billattributes->'ecom'->>'orderid' orderid from bill where ecomorderno != '' and billattributes->'ecom'->>'orderid' in (" +
            //        string.Join(",", soitem.ConvertAll(x => "'" + x.ExternOrderNo + "'")) + ")";
            //    using (var reader = ExecuteReader(billnoquery, null))
            //    {
            //        while (reader.Read())
            //        {
            //            var ecomorderno = reader["ecomorderno"] == DBNull.Value ? "" : reader["ecomorderno"].ToString();
            //            var orderid = reader["orderid"] == DBNull.Value ? "" : reader["orderid"].ToString();
            //            var billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
            //            soitem.Find(x => x.ExternOrderNo == orderid).BillNo = billno;
            //        }
            //    }

            //    billnoquery = @"select ecomorderno,billno,billattributes->'ecom'->>'orderid' orderid from bill where ecomorderno != '' and billattributes->'ecom'->>'orderid' in (" +
            //        string.Join(",", soitem.ConvertAll(x => "'" + x.OrderNo + "'")) + ")";
            //    using (var reader = ExecuteReader(billnoquery, null))
            //    {
            //        while (reader.Read())
            //        {
            //            var ecomorderno = reader["ecomorderno"] == DBNull.Value ? "" : reader["ecomorderno"].ToString();
            //            var orderid = reader["orderid"] == DBNull.Value ? "" : reader["orderid"].ToString();
            //            var billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
            //            soitem.Find(x => x.OrderNo == orderid).BillNo = billno;
            //        }
            //    }

            //    billnoquery = @"select ecomorderno,billno,billattributes->'ecom'->>'orderid' orderid from bill where ecomorderno != '' and ecomorderno in (" +
            //    string.Join(",", soitem.ConvertAll(x => "'" + x.ExternOrderNo + "'")) + ")";
            //    using (var reader = ExecuteReader(billnoquery, null))
            //    {
            //        while (reader.Read())
            //        {
            //            var ecomorderno = reader["ecomorderno"] == DBNull.Value ? "" : reader["ecomorderno"].ToString();
            //            var orderid = reader["orderid"] == DBNull.Value ? "" : reader["orderid"].ToString();
            //            var billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
            //            soitem.Find(x => x.ExternOrderNo == ecomorderno).BillNo = billno;
            //        }
            //    }

            //    billnoquery = @"select ecomorderno,billno,billattributes->'ecom'->>'orderid' orderid from bill where ecomorderno != '' and ecomorderno in (" +
            //    string.Join(",", soitem.ConvertAll(x => "'" + x.OrderNo + "'")) + ")";
            //    using (var reader = ExecuteReader(billnoquery, null))
            //    {
            //        while (reader.Read())
            //        {
            //            var ecomorderno = reader["ecomorderno"] == DBNull.Value ? "" : reader["ecomorderno"].ToString();
            //            var orderid = reader["orderid"] == DBNull.Value ? "" : reader["orderid"].ToString();
            //            var billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
            //            soitem.Find(x => x.OrderNo == ecomorderno).BillNo = billno;
            //        }
            //    }
            //}

            var ecomlist = soitem.ConvertAll(x => new Ecombill(x));


            if (item.status == "COMPLETE")
            {
                Insert(ecomlist);
            }
            return ecomlist;
        }

        internal ExcelImportManager.DocumentStatus ProcessOnline(List<Ecombill> items)
        {
            List<ExcelImportManager.ECommerceOrderItem> eCommerceOrderItems = items.ConvertAll(x => x.ToEcommerceOrderItem());

            ExcelImportManager excelImportManager = new ExcelImportManager(Context.OrganizationID);
            InfyPOS.Processors.ExcelImportManager.DocumentStatus result = new InfyPOS.Processors.ExcelImportManager.DocumentStatus() { };
            result.ecomstatus = excelImportManager.GetECOStatus(eCommerceOrderItems, new ImageData()
            {
                Context = this.Context
            });
            return result;
        }

        internal List<Unicomerce.QuantoSKUStcok> SearchOfflineStock(long locationid)
        {
            string query = string.Format(@"with allsku as
                            (
                            select schemelist->>'prdsku' prdsku,
                            schemelist->>'uid' uan
                                                            from mfgdesign
                                                            LEFT JOIN LATERAL
                                                            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                                                            left outer join referencelist on referencelist.id = cast(schemelist->>'sizeid' as bigint)
                                                            where mfgdesign.isactive = true 
                            )
                            select prdsku,uan,sum(iqty-(oqty+dqty+jqty)) stk from salablegoods 
                            inner join stock on stock.id = stockid
                            inner join allsku on allsku.uan = stock.serialno
                            where salablegoods.locationid = {0} and salablegoods.hasstock=true
                            group by prdsku,uan", locationid);
            List<Unicomerce.QuantoSKUStcok> quantoSKUStcoks = new List<Unicomerce.QuantoSKUStcok>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    Unicomerce.QuantoSKUStcok quantoSKUStcok = new Unicomerce.QuantoSKUStcok();
                    quantoSKUStcok.stk = reader["stk"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stk"]);
                    quantoSKUStcok.uan = reader["uan"] == DBNull.Value ? "" : Convert.ToString(reader["uan"]);
                    quantoSKUStcok.prdsku = reader["prdsku"] == DBNull.Value ? "" : Convert.ToString(reader["prdsku"]);
                    quantoSKUStcoks.Add(quantoSKUStcok);
                }
            }
            return quantoSKUStcoks;
        }

        internal List<Unicomerce.QuantoSKUAdjust> Stock2Unicommerce(List<Unicomerce.QuantoSKUAdjust> item)
        {
            var uniconfig = GetUnicommerceConfig();
            if (uniconfig.access_validto == DateTime.MinValue ||
                DateTime.UtcNow > uniconfig.access_validto)
            {
                var token = Unicomerce.Authenticate(uniconfig.serverurl, uniconfig.username, uniconfig.password);
                if (token != null && token.access_token != null)
                {
                    uniconfig.access_token = token.access_token;
                    uniconfig.access_validto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(token.expires_in).DateTime.ToUniversalTime().AddMinutes(-5);
                }
            }
            var response = Unicomerce.AdjustBulk(uniconfig.serverurl, uniconfig.access_token, item);

            if (response != null && response.inventoryAdjustmentResponses != null)
            {
                foreach (var itm in response.inventoryAdjustmentResponses)
                {
                    if (item.Exists(x => x.prdsku == itm.facilityInventoryAdjustment.itemSKU))
                    {
                        item.Find(x => x.prdsku == itm.facilityInventoryAdjustment.itemSKU).updated = itm.successful;
                    }
                }
            }
            return item;
        }

        internal List<Unicomerce.QuantoSKUStcok> SearchStock(EcombillCreteria item)
        {
            var offlinestock = SearchOfflineStock(item.locationid);
            if (item.Online)
            {
                SyncOnlineStock(offlinestock, item.minutes);
            }
            return offlinestock;
        }

        internal List<Unicomerce.QuantoBundleUAN> SearchBundle(EcombillCreteria item)
        {
            List<Unicomerce.QuantoBundleUAN> quantoSKUAdjusts = new List<Unicomerce.QuantoBundleUAN>();
            BundlesDataService bundlesDataService = new BundlesDataService(true) { Context = this.Context };
            var bundles = bundlesDataService.Search(new BundlesCreteria()
            {
                code = item.bundlecode
            });
            if (bundles != null && bundles.Count > 0)
            {
                foreach (var bndl in bundles)
                {
                    if (bndl.tolocationid == item.locationid || bndl.fromlocationid == item.locationid)
                    {
                        bndl.items.ForEach(x =>
                        {
                            quantoSKUAdjusts.Add(new Unicomerce.QuantoBundleUAN()
                            {
                                qty = x.aqty == 0 ? x.iqty : x.aqty,
                                uan = !string.IsNullOrEmpty(x.serialno) ? x.serialno : x.barcode,
                                add = bndl.tolocationid == item.locationid ? x.aqty : 0,
                                remove = bndl.fromlocationid == item.locationid ? x.aqty : 0,
                            });
                        });
                    }
                }
            }
            return quantoSKUAdjusts;
        }
    }

}
