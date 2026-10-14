using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class JobworkbillDataService : BaseData
    {
        const string SQL_Search = @"
            with jobworkstatus as (
            Select jobworkbillid,sum(rqty) receivedqty from jobworkbillitems where isactive = true
            group by jobworkbillid
            )
        select supplierid, taxamount, id, totalamount, grossamount, otheramount, materialcost, materialsalevalue, modifiedon, createdon, 
        createdby, settled, locationid, modifiedby, version, suppliercompanyid, companyid, isactive, entrydate, isinterstatesale, 
        invoiceno, invoicedate, noofpieces, billmode, paidamount,taxid,jobchargeid,qty,oqty,status,expecteddate,remarks,jobworkstatus.receivedqty 
        from jobworkbill inner join  jobworkstatus on jobworkstatus.jobworkbillid = jobworkbill.id
        where id=@id";


        const string SQL_Insert = @"INSERT INTO public.jobworkbill(supplierid, taxamount, totalamount, grossamount, otheramount, materialcost, materialsalevalue, modifiedon, createdon, createdby, settled, locationid, modifiedby, version, suppliercompanyid, companyid, isactive, entrydate, isinterstatesale, invoiceno, invoicedate, noofpieces, billmode, paidamount,taxid,jobchargeid,qty,oqty,status,remarks,expecteddate) 
        VALUES(@supplierid, @taxamount, @totalamount, @grossamount, @otheramount, @materialcost, @materialsalevalue, @modifiedon, @createdon, @createdby, @settled, @locationid, @modifiedby, @version, @suppliercompanyid, @companyid, @isactive, @entrydate, @isinterstatesale, @invoiceno, @invoicedate, @noofpieces, @billmode, @paidamount,@taxid,@jobchargeid,@qty,@oqty,@status,@remarks,@expecteddate) RETURNING id";

        const string SQL_Stock = @"select salablegoods.id, salablegoods.locationid,salablegoods.companyid,stock.invoiceitemid,inwardon,salablegoods.barcode,salablegoods.iqty,salablegoods.oqty,salablegoods.dqty,stock.serialno,salablegoods.jqty,
                                    stock.printingname,salablegoods.price,stock.buyingprice taxable,stock.buyingprice,stock.ibuyingprice,(stock.ibuyingprice-stock.buyingprice) tax,salablegoods.discount,salablegoods.stockid , stock.stockdate stockdate,
                                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) rstock, stock.serialno, supplier.name suppliername, brand.name brandname,
                                    products.productgroup, products.salescounterid,products.id productid, products.name as productname, stock.designid, stock.hsncode, products.sellingmode,
                                    items.sizeid, items.typeid, items.colourid,items.materialid,items.styleid,items.brandid,items.patternid,lrinvoiceitems.mrp,lrinvoiceitems.imgid,stock.imageid,
                                    style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,
                                    EXTRACT(day from @current_date - lrinvoice.entrydate) age, lrentry.stockholdingperiod, EXTRACT(day from @current_date - stock.stockdate) age1
                                    from stock
									inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive = true
									left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
									left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
                                    left outer join lrentry on lrentry.id = lrinvoice.lrid
                                    left outer join items on items.id = stock.itemid
                                    left outer join products on products.id = stock.productid
                                    left outer join supplier on supplier.id = lrentry.supplierid
                        left outer join brand brand on brand.id = items.brandid
                        left outer join referencelist colour on colour.id = colourid
                        left outer join referencelist size on size.id = sizeid
                        left outer join referencelist style on style.id = styleid
                        left outer join referencelist pattern on pattern.id = patternid
                        left outer join referencelist material on material.id = materialid
                        left outer join referencelist type on type.id = typeid
                                    where stock.isactive=true";

        internal decimal SearchBillValue(JobworkbillCreteria item)
        {
            DateTime fromdate = DateTime.MinValue;
            DateTime todate = DateTime.MinValue;
            if(item.entrydate.Month>3)
            {
                fromdate = new DateTime(item.entrydate.Year, 4, 1);
                todate = new DateTime(item.entrydate.Year + 1, 3, 31).AddDays(1);
            }
            else
            {
                fromdate = new DateTime(item.entrydate.Year - 1, 4, 1);
                todate = new DateTime(item.entrydate.Year, 3, 31).AddDays(1);
            }
            var param = new SortedList<string, object>();
            param.Add("suppliercompanyid", item.supplierid);
            param.Add("id", item.id);
            param.Add("fromdate", fromdate);
            param.Add("todate", todate);
            string query = @"select sum(grossamount) from Jobworkbill
                             where suppliercompanyid = @suppliercompanyid and entrydate>=@fromdate and entrydate<@todate";
            if (item.id > 0)
                query += " and id!=@id";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                    return reader.GetDecimal(0);
            }
            return 0;
        }

        internal byte[] ImagePDFGeneration(JobworkbillCreteria item)
        {
            string query = @"
            with orderitems
            as
            (
            select stock.barcode,stock.imageid,suppliercompany.name,jobworkbillitems.id,entrydate,invoiceno from jobworkbillitems
            inner join jobworkbill on jobworkbill.id = jobworkbillid
            inner join stock on stock.id = jobworkbillitems.stockid
            inner join suppliercompany on suppliercompany.id = suppliercompanyid
            where jobworkbillid in ({0})
            ),billed as
            (
	            select jobworkbillitemsid,Jobworkbillgrnitems.barcode tobarcode,Jobworkbillgrnitems.imageid toimageid,Jobworkbillgrn.entrydate toentrydate,
	            Jobworkbillgrn.invoiceno toinvoiceno from Jobworkbillgrnitems
	            inner join Jobworkbillgrn on Jobworkbillgrn.id = Jobworkbillgrnitems.Jobworkbillgrnid
	            inner join orderitems on orderitems.id = jobworkbillitemsid
            )

            select * from orderitems
            left outer join billed on jobworkbillitemsid = orderitems.id";

            var param = new SortedList<string, object>();
            List<JobWorkImage> list = new List<JobWorkImage>();
            using (var reader = ExecuteReader(string.Format(query, string.Join(",", item.ids)), param))
            {
                while (reader.Read())
                {
                    list.Add(new JobWorkImage()
                    {
                        supplier = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "",
                        barcode = reader["barcode"] != DBNull.Value ? Convert.ToString(reader["barcode"]) : "",
                        imageid = reader["imageid"] != DBNull.Value ? Convert.ToString(reader["imageid"]) : "",
                        invoiceno = reader["invoiceno"] != DBNull.Value ? Convert.ToString(reader["invoiceno"]) : "",
                        entrydate = reader["entrydate"] != DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : DateTime.MinValue,
                        tobarcode = reader["tobarcode"] != DBNull.Value ? Convert.ToString(reader["tobarcode"]) : "",
                        toimageid = reader["toimageid"] != DBNull.Value ? Convert.ToString(reader["toimageid"]) : "",
                        toinvoiceno = reader["toinvoiceno"] != DBNull.Value ? Convert.ToString(reader["toinvoiceno"]) : "",
                        toentrydate = reader["toentrydate"] != DBNull.Value ? Convert.ToDateTime(reader["toentrydate"]) : DateTime.MinValue
                    });
                }
            }

            PDFGenerator pDFGenerator = new PDFGenerator() { Context = this.Context };
            return pDFGenerator.ImagePDFGeneration(Context.OrganizationID, list);
        }

        const string SQL_Update = @"UPDATE public.jobworkbill SET supplierid=@supplierid, taxamount=@taxamount, totalamount=@totalamount, grossamount=@grossamount, otheramount=@otheramount, materialcost=@materialcost, materialsalevalue=@materialsalevalue, modifiedon=@modifiedon, settled=@settled, locationid=@locationid, modifiedby=@modifiedby, version=@version+1, suppliercompanyid=@suppliercompanyid, companyid=@companyid, entrydate=@entrydate, isinterstatesale=@isinterstatesale, invoiceno=@invoiceno, invoicedate=@invoicedate, noofpieces=@noofpieces, billmode=@billmode, 
        paidamount=@paidamount,taxid=@taxid,jobchargeid=@jobchargeid,qty=@qty,oqty=@oqty,status=@status,remarks=@remarks,expecteddate=@expecteddate where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.jobworkbill set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Jobworkbill> Search(JobworkbillCreteria _jobworkbill)
        {
            List<Jobworkbill> lst = new List<Jobworkbill>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_jobworkbill.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworkbill.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Replace("where id=@id", "");
                    qb.AddParameter("isactive", true, "=");

                    if (_jobworkbill.companyid > 0)
                        qb.AddParameter("companyid", _jobworkbill.companyid, "=");
                    if (_jobworkbill.supplierid > 0)
                        qb.AddParameter("supplierid", _jobworkbill.supplierid, "=");
                    if (_jobworkbill.locationid > 0)
                        qb.AddParameter("locationid", _jobworkbill.locationid, "=");
                    if (!string.IsNullOrEmpty(_jobworkbill.invoiceno))
                        qb.AddParameter("invoiceno", _jobworkbill.invoiceno, "=");
                    if (_jobworkbill.entryfrom != DateTime.MinValue && _jobworkbill.entryto != DateTime.MinValue)
                    {
                        qb.AddParameter("jobworkbill.entrydate", "datefrom", _jobworkbill.entryfrom.Date, ">=");
                        qb.AddParameter("jobworkbill.entrydate", "dateto", _jobworkbill.entryto.Date.AddDays(1), "<");
                    }
                    if (_jobworkbill.duefrom != DateTime.MinValue && _jobworkbill.dueto != DateTime.MinValue)
                    {
                        qb.AddParameter("jobworkbill.invoicedate", "invoicedatefrom", _jobworkbill.duefrom.Date, ">=");
                        qb.AddParameter("jobworkbill.invoicedate", "invoicedateto", _jobworkbill.dueto.Date.AddDays(1), "<");
                    }

                    qb.SortField = "jobworkbill.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkbill ojobworkbill = new Jobworkbill();
                        ojobworkbill.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ojobworkbill.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        ojobworkbill.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbill.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        ojobworkbill.grossamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"]);
                        ojobworkbill.otheramount = reader["otheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otheramount"]);
                        ojobworkbill.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbill.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbill.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbill.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbill.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbill.settled = reader["settled"] == DBNull.Value ? false : Convert.ToBoolean(reader["settled"]);
                        ojobworkbill.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ojobworkbill.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworkbill.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbill.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        ojobworkbill.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ojobworkbill.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbill.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        ojobworkbill.expecteddate = reader["expecteddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expecteddate"]);
                        ojobworkbill.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        ojobworkbill.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        ojobworkbill.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ojobworkbill.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        ojobworkbill.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"]);
                        ojobworkbill.billmode = reader["billmode"] == DBNull.Value ? "" : reader["billmode"].ToString();
                        ojobworkbill.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        ojobworkbill.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        ojobworkbill.jobchargeid = reader["jobchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobchargeid"]);
                        ojobworkbill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ojobworkbill.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["oqty"]);
                        ojobworkbill.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        ojobworkbill.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["receivedqty"]);
                        if (ojobworkbill.receivedqty == 0)
                            ojobworkbill.statustext = "Pending";
                        else if (ojobworkbill.receivedqty < ojobworkbill.oqty)
                            ojobworkbill.statustext = "Partially Received";
                        else if (ojobworkbill.receivedqty == ojobworkbill.oqty)
                            ojobworkbill.statustext = "Job Completed";

                        lst.Add(ojobworkbill);
                    }
                }
                if (_jobworkbill.id > 0 && lst.Count == 1)
                {
                    JobworkbillitemsDataService subJobworkbillitems = new JobworkbillitemsDataService() { Context = this.Context };
                    lst[0].Jobworkbillitems = subJobworkbillitems.SearchAll(_jobworkbill.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbill);
            }
            return lst;
        }

        public List<StockInfo> SearchStock(JobworkbillCreteria _reportcreteria)
        {

            List<StockInfo> lst = new List<StockInfo>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Stock;

                qb.AddParameter("stock.barcode", _reportcreteria.barcode, " = ");
                if (_reportcreteria.locationid > 0)
                    qb.AddParameter("salablegoods.locationid", _reportcreteria.locationid, "=");
                qb.Limit = 500;
                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                command.CommandTimeout = 10000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockInfo oitem = new StockInfo();
                        oitem.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oitem.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        oitem.inwardon = reader["inwardon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["inwardon"]);
                        oitem.createdon = reader["stockdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["stockdate"]);
                        oitem.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitem.glncode = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        oitem.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        oitem.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oitem.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"]);
                        oitem.itemname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitem.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        if (!Context.HasPermission(PermissionSet.showsellingvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                            oitem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                            oitem.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        }
                        if (!Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
                        {
                            oitem.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                            oitem.taxvalue = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            oitem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                            oitem.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                            if (oitem.ibuyingprice < oitem.buyingprice) oitem.ibuyingprice = oitem.buyingprice;
                            oitem.margin = oitem.price - oitem.discount - oitem.ibuyingprice;
                        }
                        oitem.qty = reader["rstock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rstock"]);
                        oitem.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitem.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitem.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        if (Context.HasPermission(PermissionSet.hidesupplier, InfyPOS.Common.Action.View))
                            oitem.suppliername = "hidden";
                        else
                            oitem.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        oitem.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitem.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oitem.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oitem.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitem.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitem.salablegoodsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitem.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oitem.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oitem.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        oitem.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        oitem.sectionid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salescounterid"]);
                        oitem.stockvalue = oitem.buyingprice * oitem.qty;
                        oitem.taxablevalue = oitem.taxable * oitem.qty;
                        oitem.salevalue = (oitem.price - oitem.discount) * oitem.qty;
                        oitem.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oitem.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitem.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oitem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oitem.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oitem.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oitem.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oitem.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oitem.stockage = reader["age"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age"]);
                        oitem.stockholdingperiod = reader["stockholdingperiod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stockholdingperiod"]);

                        if (oitem.buyingprice > 0 && (oitem.price - oitem.discount - oitem.buyingprice) > 0)
                            oitem.marginpercentage = Math.Round(((oitem.price - oitem.discount - oitem.buyingprice) / oitem.buyingprice) * 100, 2);

                        if (oitem.stockage > 10000)
                        {
                            oitem.stockage = reader["age1"] == DBNull.Value ? 0 : Convert.ToInt32(reader["age1"]);
                        }

                        oitem.imageid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                        if(string.IsNullOrEmpty(oitem.imageid))
                            oitem.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();

                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return lst;
        }

        public Jobworkbill Insert(Jobworkbill oJobworkbill)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("jobworkbill.invoiceno", oJobworkbill.locationid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oJobworkbill.invoiceno = sequence.GetNextNumber(Context,conn, transaction);
                    oJobworkbill.entrydate = DateTimeNow;
                    oJobworkbill.createdon = DateTimeNow;
                    oJobworkbill.modifiedon = DateTimeNow;
                    oJobworkbill.createdby = Context.UserID;
                    oJobworkbill.modifiedby = Context.UserID;
                    Insert(transaction, conn, oJobworkbill);
                    JobworkbillitemsDataService subJobworkbillitems = new JobworkbillitemsDataService() { Context = this.Context };
                    foreach (var item in oJobworkbill.Jobworkbillitems)
                    {
                        item.jobworkbillid = oJobworkbill.id;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        item.createdby = Context.UserID;
                        item.modifiedby = Context.UserID;
                        item.isactive = true;
                        item.version = 1;
                        subJobworkbillitems.Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbill);
            }

            return oJobworkbill;
        }

        public Jobworkbill Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbill oJobworkbill)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.supplierid;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.taxamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.totalamount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.grossamount;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.otheramount;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.materialcost;

            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.qty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.oqty;

            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.materialsalevalue;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.createdby;
            command.Parameters.Add("settled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.settled;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.locationid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.suppliercompanyid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.companyid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.isactive;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.entrydate;
            command.Parameters.Add("expecteddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.expecteddate;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.isinterstatesale;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbill.invoiceno == null ? "" : oJobworkbill.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.invoicedate;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.noofpieces;
            command.Parameters.Add("billmode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbill.billmode == null ? "" : oJobworkbill.billmode;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.paidamount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.taxid;
            command.Parameters.Add("jobchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.jobchargeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.status;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbill.remarks == null ? "" : oJobworkbill.remarks;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbill.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oJobworkbill;
        }

        public Jobworkbill Update(Jobworkbill oJobworkbill)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oJobworkbill);
                    JobworkbillitemsDataService subJobworkbillitems = new JobworkbillitemsDataService() { Context = this.Context };
                    subJobworkbillitems.Update(transaction, conn, oJobworkbill.Jobworkbillitems, oJobworkbill.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbill);
            }

            return oJobworkbill;
        }

        public Jobworkbill Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbill oJobworkbill)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.supplierid;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.taxamount;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.id;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.totalamount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.grossamount;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.otheramount;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.materialcost;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.materialsalevalue;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.qty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.oqty;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.createdby;
            command.Parameters.Add("settled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.settled;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.locationid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.suppliercompanyid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.companyid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.isactive;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.entrydate;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.isinterstatesale;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbill.invoiceno == null ? "" : oJobworkbill.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.invoicedate;
            command.Parameters.Add("expecteddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.expecteddate;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.noofpieces;
            command.Parameters.Add("billmode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbill.billmode == null ? "" : oJobworkbill.billmode;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbill.paidamount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.taxid;
            command.Parameters.Add("jobchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.jobchargeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.status;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbill.remarks == null ? "" : oJobworkbill.remarks;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbill.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oJobworkbill;
        }


        public bool Delete(Jobworkbill oJobworkbill)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oJobworkbill);
                    JobworkbillitemsDataService subJobworkbillitems = new JobworkbillitemsDataService() { Context = this.Context };
                    subJobworkbillitems.DeleteAll(oJobworkbill.id, oJobworkbill.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbill);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbill oJobworkbill)
        {
            string validatequery = "Select sum(rqty) from jobworkbillitems where jobworkbillid=@id and rqty>0 ";
            NpgsqlCommand command = new NpgsqlCommand(validatequery, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.id;
            var outqty = command.ExecuteScalar();
            int successCount = Convert.ToInt32( outqty == DBNull.Value ? 0 : outqty );
            if(successCount>0)
            {
                throw new Exception("Some ("+ successCount + ") of the items already received after job work, so can't delete this order");
            }

            command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbill.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbill.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbill.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbill.id;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public JobworkbillPrint Print(JobworkbillCreteria item)
        {
            var returnjobwork = Search(new JobworkbillCreteria()
            {
                id = item.id
            });

            JobworkbillPrint ret = new JobworkbillPrint();
            ret.id = returnjobwork[0].id;
            ret.companyid = returnjobwork[0].companyid;
            ret.supplierid = returnjobwork[0].supplierid;
            ret.suppliercompanyid = returnjobwork[0].suppliercompanyid;
            ret.qty = returnjobwork[0].qty;
            ret.invoiceno = returnjobwork[0].invoiceno;
            ret.invoicedate = returnjobwork[0].invoicedate;
            ret.entrydate = returnjobwork[0].entrydate;
            ret.expecteddate = returnjobwork[0].expecteddate;
            ret.totalamount = returnjobwork[0].totalamount;
            ret.Jobworkbillitems = returnjobwork[0].Jobworkbillitems;
            ret.materialcost = returnjobwork[0].materialcost;
            ret.materialsalevalue = returnjobwork[0].materialsalevalue;
            ret.remarks = returnjobwork[0].remarks;

            if (returnjobwork[0].locationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = returnjobwork[0].locationid
                }).First();

                ret.location = new CompanyLocation(location);
                ret.location.name = location.name;
                ret.location.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
                ret.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                ret.location.addresslines = new List<CompanyLocation.Address>();
                if (!string.IsNullOrEmpty(ret.location.address))
                {
                    var addresslist = ret.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        ret.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }
            }
            else
            {
                ret.location = new CompanyLocation();
                ret.location.name = "WAREHOUSE";
            }
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = returnjobwork[0].companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            Data.SuppliercompanyDataService scompanyservice = new SuppliercompanyDataService() { Context = this.Context };
            var suppliercompany = scompanyservice.Search(new SuppliercompanyCreteria()
            {
                id = ret.suppliercompanyid,
                isprint = true
            });

            if (suppliercompany.Count > 0)
            {
                ret.supplier = suppliercompany[0];
                if (ret.supplier.cityid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var city = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = ret.supplier.cityid
                    }).First();
                    ret.supplier.cityname = city.name;
                }
            }


            return ret;
        }


    }

}
