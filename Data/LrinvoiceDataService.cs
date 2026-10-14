using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;

namespace InfyPOS.Data
{
    public class LrinvoiceDataService : SAASData
    {
        const string SQL_Insert = @"INSERT INTO public.lrinvoice(companyid, invoiceno, invoicedate, noofpieces, noofbundle, baseamount, grossamount,taxamount, totalamount, lrid, otheramount, modifiedon, createdon, createdby, modifiedby, version, suppliercompanyid,lrsupplierid, isactive, discount,isinterstatesale,iscreditnote,billvalue,isinclusivetax,isinclusiveall,isinclusivediscount,hascommission,invoiceentryno,entrydate,isdirectpurchase,isdeliverychallan,statusid,isvendorbill,isrcm,hastds,tdsledgerid,tdsamount,tdspercentage,buyerinvoiceid,isgenerated,expensepercentage,expenseperpiece,plessdiscount,plessvalue,isintercompanytransfer,potypeid,poid,pono,locationid) 
        VALUES(@companyid, @invoiceno, @invoicedate, @noofpieces, @noofbundle, @baseamount, @grossamount,@taxamount,@totalamount, @lrid, @otheramount, @modifiedon, @createdon, @createdby, @modifiedby, @version, @suppliercompanyid,@lrsupplierid, @isactive, @discount,@isinterstatesale,@iscreditnote,@billvalue,@isinclusivetax,@isinclusiveall,@isinclusivediscount,@hascommission,@invoiceentryno,@entrydate,@isdirectpurchase,@isdeliverychallan,@statusid,@isvendorbill,@isrcm,@hastds,@tdsledgerid,@tdsamount,@tdspercentage,@buyerinvoiceid,@isgenerated,@expensepercentage,@expenseperpiece,@plessdiscount,@plessvalue,@isintercompanytransfer,@potypeid,@poid,@pono,@locationid) RETURNING id";

        //const string SQL_SearchAll = @"select  from lrinvoicereturnitems where lrinvoicereturnid=@lrinvoicereturnid";
        const string SQL_Search = @"{0} tsc as ( 
        Select invoiceid, sum(taxamount) tscamount from lrinvoicetax inner join tax on tax.id=lrinvoicetax.taxid
        where lrinvoicetax.isactive=true and tax.isactive=true and tax.type=5 and lrinvoicetax.createdby>0
        group by invoiceid
        )
        select suppliercompany.name suppliercompanyname,lrinvoice.companyid, lrentry.entrylocationid, lrentry.retaillocationid, lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoice.noofpieces, noofbundle, baseamount, grossamount,taxamount, discount, lrinvoice.totalamount, lrid, lrinvoice.id, 
        lrinvoice.isinterstatesale,iscreditnote,invoiceentryno, supplier.supportpo,isdirectpurchase,isdeliverychallan,statusid,lrinvoice.isrcm,lrinvoice.hastds,tdsledgerid,tdsamount,tdspercentage,
        otheramount, lrinvoice.modifiedon, lrinvoice.createdon, lrinvoice.createdby, lrinvoice.modifiedby, lrinvoice.version, lrinvoice.suppliercompanyid,lrsupplierid, lrinvoice.isactive,lrentry.supplierid,billvalue,isinclusivetax,isinclusiveall,isinclusivediscount,lrinvoice.hascommission, lrinvoice.entrydate,isvendorbill,
        buyerinvoiceid,isgenerated,expensepercentage,expenseperpiece,plessdiscount,plessvalue,supplier.name suppliername,lrinvoice.settled,
        createdemployee.name createduser, modifiedemployee.name modifieduser, isintercompanytransfer, city.name cityname, lrinvoice.barcodegenerationapproval,
        lrinvoice.potypeid, lrinvoice.poid, lrinvoice.pono, tsc.tscamount,purchaseorder.podate,hasapprove,lrentry.lrentryno,qcby
        {1}
		from lrinvoice
        left outer join lrentry on  lrentry.id=lrinvoice.lrid 
        left outer join purchaseorder on purchaseorder.id = poid
        left outer join supplier on supplier.id = lrentry.supplierid
		left outer join suppliercompany on suppliercompany.id = suppliercompanyid
        left outer join referencelist city on city.id = suppliercompany.cityid
        left outer join users createduser on createduser.id = lrinvoice.createdby
        left outer join employee createdemployee on createdemployee.id = createduser.employeeid
        left outer join users modifieduser on modifieduser.id = lrinvoice.modifiedby
        left outer join employee modifiedemployee on modifiedemployee.id = modifieduser.employeeid
		left outer join tsc on tsc.invoiceid=lrinvoice.id
        {2}
        {3} ";

        const string SQL_Update = @"UPDATE public.lrinvoice SET companyid=@companyid, invoiceno=@invoiceno, invoicedate=@invoicedate, noofpieces=@noofpieces,
        isinterstatesale=@isinterstatesale,billvalue=@billvalue, noofbundle=@noofbundle, baseamount=@baseamount, iscreditnote=@iscreditnote,
        grossamount=@grossamount,invoiceentryno=@invoiceentryno,isdirectpurchase=@isdirectpurchase,statusid=@statusid,isdeliverychallan=@isdeliverychallan,
        isinclusivetax=@isinclusivetax,isinclusiveall=@isinclusiveall,isinclusivediscount=@isinclusivediscount,hascommission=@hascommission,totalamount=@totalamount, lrid=@lrid, 
        isvendorbill=@isvendorbill,buyerinvoiceid=@buyerinvoiceid,isgenerated=@isgenerated,isrcm=@isrcm,hastds=@hastds,tdsledgerid=@tdsledgerid,tdsamount=@tdsamount,tdspercentage=@tdspercentage,
        otheramount=@otheramount,taxamount = @taxamount, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1,lrsupplierid=@lrsupplierid, suppliercompanyid=@suppliercompanyid, discount=@discount,entrydate=@entrydate,expensepercentage=@expensepercentage,
        expenseperpiece=@expenseperpiece,plessdiscount=@plessdiscount,plessvalue=@plessvalue,potypeid=@potypeid,poid=@poid,pono=@pono where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.lrinvoice set buyerinvoiceid = 0,isgenerated=false where buyerinvoiceid in (select id from lrinvoice where id=@id and version=@version);

                                update lrinvoiceitems set modifiedon=@modifiedon,modifiedby=@modifiedby,isactive=false where lrinvoiceid=@id;
                                delete from salablegoods using stock where salablegoods.stockid = stock.id and invoiceitemid in (select id from lrinvoiceitems where lrinvoiceid=@id);
                                delete from stock where invoiceitemid in (select id from lrinvoiceitems where lrinvoiceid=@id);
                                delete from sellablebarcodes using stock
                                where sellablebarcodes.stockid = stock.id and invoiceitemid in (select id from lrinvoiceitems where lrinvoiceid=@id);

        update public.lrinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_DeleteAll = @"update public.lrinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where lrid=@lrid";
        const string SQL_DeleteInvoice = @"update public.lrinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
        public bool ispricing { get; set; }
        public LrinvoiceCover SearchDetail(LrinvoiceCreteria _lrinvoice, bool showitems)
        {
            LrinvoiceCover cover = new LrinvoiceCover();
            List<Lrinvoice> result = Search(_lrinvoice, showitems);
            if (_lrinvoice.lrid > 0 || (_lrinvoice.id > 0 && result.Count == 1))
            {
                cover.Items = result;
                cover.lrid = _lrinvoice.lrid > 0 ? _lrinvoice.lrid : result[0].lrid;
                LrentryDataService LrentryinvoiceData = new LrentryDataService() { Context = this.Context };
                var lrinfo = LrentryinvoiceData.Search(new LrentryCreteria()
                {
                    id = cover.lrid
                });
                if (lrinfo != null && lrinfo.Count == 1)
                {
                    cover.lrentryno = lrinfo[0].lrentryno;
                    cover.lrentrydate = lrinfo[0].lrentrydate;
                    cover.lrno = lrinfo[0].lrno;
                    cover.supplierid = lrinfo[0].supplierid;
                    cover.sourcesupplierid = lrinfo[0].sourcesupplierid;
                    cover.status = lrinfo[0].Status;
                    cover.modeofdelivery = lrinfo[0].modeofdelivery;
                    cover.agentid = lrinfo[0].agentid;
                    SupplierDataService suppdata = new SupplierDataService() { Context = this.Context };
                    var supplierlist = suppdata.Search(new SupplierCreteria()
                    {
                        id = cover.supplierid
                    });
                    if (supplierlist.Count > 0)
                    {
                        cover.suppliername = supplierlist[0].name;
                        cover.sourcesuppliername = supplierlist[0].name;
                        cover.suppliercityname = supplierlist[0].cityname;
                    }
                    if (cover.sourcesupplierid > 0 && cover.sourcesupplierid != cover.supplierid)
                    {
                        supplierlist = suppdata.Search(new SupplierCreteria()
                        {
                            id = cover.sourcesupplierid
                        });
                        if (supplierlist.Count > 0)
                            cover.sourcesuppliername = supplierlist[0].name;
                    }
                    if (cover.agentid > 0)
                    {
                        AgentDataService agentdata = new AgentDataService() { Context = this.Context };
                        var agentlist = agentdata.Search(new AgentCreteria()
                        {
                            id = cover.agentid
                        });
                        if (agentlist.Count > 0)
                            cover.agentname = agentlist[0].name;
                    }
                }

                if (_lrinvoice.id > 0)
                {
                    if (result[0].suppliercompanyid > 0)
                    {
                        SuppliercompanyDataService sDataService = new SuppliercompanyDataService() { Context = this.Context };
                        List<Suppliercompany> suppliercompany = sDataService.Search(new SuppliercompanyCreteria()
                        { id = result[0].suppliercompanyid });
                        if (suppliercompany.Count == 1)
                            cover.suppliercompanyname = suppliercompany[0].name;
                    }

                    if (result[0].companyid > 0)
                    {
                        CompanyDataService sDataService = new CompanyDataService() { Context = this.Context };
                        List<Company> buyingcompany = sDataService.Search(new CompanyCreteria()
                        {
                            id = result[0].companyid
                        }
                        );
                        if (buyingcompany.Count == 1)
                        {
                            cover.companyname = buyingcompany[0].name;
                            cover.companytitle = buyingcompany[0].title;
                            cover.companyid = buyingcompany[0].id;
                        }
                    }
                    if (showitems)
                    {
                        var snolist = result[0].Lrinvoiceitems.GroupBy(e => e.sno).ToList();
                        List<decimal> parents = new List<decimal>();
                        foreach (var kv in snolist)
                        {
                            if (kv.Count() > 1)
                                parents.Add(kv.First().sno);
                        }
                        result[0].multipacksno = parents;
                        result[0].Lrinvoiceitems = result[0].Lrinvoiceitems.OrderBy(e => e.sno).ToList();
                    }
                }
            }
            return cover;
        }

        internal List<string> SearchBarcodes(LrinvoicereturnProductsCreteria creteria)
        {
            List<string> lst = new List<string>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select barcode from stock
                                    inner join items on items.id = stock.itemid
                                    inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                                    inner join lrentry on lrentry.id = lrinvoice.lrid
                                    where stock.isactive = true";

                if (creteria.locationid > 0)
                {
                    qb.CurrentQuery = @"select stock.barcode from stock
                                    inner join salablegoods on salablegoods.stockid = stock.id
                                    inner join items on items.id = stock.itemid
                                    inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                                    inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                                    inner join lrentry on lrentry.id = lrinvoice.lrid
                                    where stock.isactive = true";
                    qb.AddParameter("salablegoods.locationid", "locationid", creteria.locationid, "=");
                }
                if (creteria.supplierid > 0)
                    qb.AddParameter("lrentry.supplierid", "supplierid", creteria.supplierid, "=");
                if (!string.IsNullOrEmpty(creteria.invoiceno))
                    qb.AddParameter("lrinvoice.invoiceno", "invoiceno", creteria.invoiceno, "=");
                if (creteria.brandid > 0)
                    qb.AddParameter("items.brandid", "brandid", creteria.brandid, "=");
                if (creteria.styleid > 0)
                    qb.AddParameter("items.styleid", "styleid", creteria.styleid, "=");
                if (creteria.patternid > 0)
                    qb.AddParameter("items.patternid", "patternid", creteria.patternid, "=");

                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lst.Add(reader.GetString(0));
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return lst;
        }

        internal List<LrinvoiceItemBarcode> SupplierInvoiceBarcodes(LrinvoiceCreteria creteria)
        {
            List<LrinvoiceItemBarcode> lst = new List<LrinvoiceItemBarcode>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select lrinvoice.id,lrinvoice.invoicedate,suppliercompany.name supplier,suppliercompany.supplierid,stock.barcode,stock.serialno,
                                    lrinvoiceitems.buyingqty,salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) stockqty
                                    from lrinvoice
                                    inner join lrentry on lrentry.id = lrinvoice.lrid
                                    inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                    inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
                                    inner join  stock on stock.invoiceitemid = lrinvoiceitems.id
                                    inner join  salablegoods on salablegoods.stockid = stock.id
                                    where lrinvoice.isactive=true
                                    and lrinvoiceitems.isactive=true and salablegoods.isactive=true";

                if (creteria.searchtype == "LRENTRYNO")
                    qb.AddParameter("lrentry.lrentryno", "lrentryno", creteria.invoiceno, "=");
                else
                    qb.AddParameter("lrinvoice.invoiceno", "invoiceno", creteria.invoiceno, "=");
                qb.AddParameter("salablegoods.locationid", "locationid", creteria.locationid, "=");

                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string supplier = reader["supplier"] == DBNull.Value ? "" : Convert.ToString(reader["supplier"]);
                        DateTime invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        long supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        string barcode = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                        if (string.IsNullOrEmpty(barcode))
                            barcode = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]);
                        decimal buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                        decimal stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        if (!lst.Exists(x => x.id == x.id))
                        {
                            lst.Add(new LrinvoiceItemBarcode()
                            {
                                id = id,
                                supplier = supplier,
                                invoicedate = invoicedate,
                                stockitems = new List<LrinvoiceItemBarcode.BarcodeStock>()
                            });
                        }
                        var eitem = lst.Find(x => x.id == x.id);
                        eitem.stockitems.Add(new LrinvoiceItemBarcode.BarcodeStock()
                        {
                            barcode = barcode,
                            buyingqty = buyingqty,
                            stockqty = stockqty > buyingqty ? buyingqty : stockqty
                        });
                        eitem.buyingqty += buyingqty;
                        eitem.stockqty += (stockqty > buyingqty ? buyingqty : stockqty);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return lst;
        }
        public List<Lrinvoice> Search(LrinvoiceCreteria _lrinvoice)
        {
            return Search(_lrinvoice, true);
        }
        public List<Lrinvoice> Search(LrinvoiceCreteria _lrinvoice, bool showitems)
        {
            List<Lrinvoice> lst = new List<Lrinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                bool hasCondition = false;
                bool hasBarcoded = false;
                if (!string.IsNullOrEmpty(_lrinvoice.barcode))
                {
                    command = GetCommand(@"select lrinvoiceid  from stock 
				                    inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid and lrinvoiceitems.isactive=true
                                    where barcode=@barcode and stock.isactive=true
                                    limit 1", conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrinvoice.barcode;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            _lrinvoice.id = reader.GetInt64(0);
                    }
                }

                if (!string.IsNullOrEmpty(_lrinvoice.designid))
                {
                    command = GetCommand(@"select lrinvoiceid  from  lrinvoiceitems 
                                    where designid = @designid and  lrinvoiceitems.isactive=true
                                    limit 1", conn);
                    command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrinvoice.designid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                            _lrinvoice.id = reader.GetInt64(0);
                    }
                }
                if (_lrinvoice.id != 0)
                {
                    command = GetCommand(string.Format(SQL_Search, "with ", "", "", " where lrinvoice.id=@id ") + (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_barcode ? ";select barcodegenerationapproval from lrinvoice where id=@id;" : ";"), conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();

                    qb.CurrentQuery = string.Format(SQL_Search, "with ", "", "", " where lrinvoice.isactive=true ");

                    //qb.CurrentQuery = @" with barcodedtable as (
                    //                select lrinvoiceid,min(barcodedon) barcodedon from lrinvoiceitems
                    //                where isactive=true and barcodedon is not null
                    //                group by lrinvoiceid), " +
                    //    string.Format(SQL_Search, "", ",barcodedtable.barcodedon barcodedon ", 
                    //    " left outer join barcodedtable on barcodedtable.lrinvoiceid=lrinvoice.id", " where lrinvoice.isactive=true ");

                    if (_lrinvoice.barcodedfromdate != DateTime.MinValue && _lrinvoice.barcodedtodate != DateTime.MinValue)
                    {
                        hasBarcoded = true;
                        qb.CurrentQuery = @" with barcodedtable as (
                                    select lrinvoiceid,min(barcodedon) barcodedon from lrinvoiceitems
                                    where isactive=true and barcodedon>=@barcodedfrom 
                                    and barcodedon<@barcodedto and barcodedon is not null
                                    group by lrinvoiceid), " +
                                    string.Format(SQL_Search, "", ",barcodedtable.barcodedon barcodedon ", " inner join barcodedtable on barcodedtable.lrinvoiceid=lrinvoice.id", " where lrinvoice.isactive=true ");
                    }


                    if (!string.IsNullOrEmpty(_lrinvoice.ids) && _lrinvoice.ids.Length > 0)
                    {
                        qb.AddParameter("lrinvoice.id", "ids", _lrinvoice.ids, " in ", "(" + _lrinvoice.ids + ")");
                        hasCondition = true;
                    }

                    if (_lrinvoice.companyid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                    {
                        qb.AddParameter("lrinvoice.companyid", "companyid", _lrinvoice.companyid, "=");
                    }

                    if (_lrinvoice.paymentmode == 1)
                    {
                        qb.AddParameter("lrinvoice.settled", "settled", true, "=");
                    }

                    if (_lrinvoice.paymentmode == 2)
                    {
                        qb.AddParameter("lrinvoice.settled", "settled", false, "=");
                    }
                    if (_lrinvoice.purchaseorderid > 0)
                    {
                        qb.AddParameter("poid", _lrinvoice.purchaseorderid, "=");
                        hasCondition = true;
                    }

                    if (_lrinvoice.importreferenceid > 0)
                    {
                        qb.AddParameter("lrentry.importreferenceid", _lrinvoice.importreferenceid, "=");
                        qb.AddParameter("lrentry.importreferencetype", _lrinvoice.importreferencetype, "=");
                        hasCondition = true;
                    }

                    if (_lrinvoice.lrid > 0)
                    {
                        qb.AddParameter("lrid", _lrinvoice.lrid, "=");
                        hasCondition = true;
                    }
                    if (_lrinvoice.id > 0)
                    {
                        qb.AddParameter("id", _lrinvoice.id, "=");
                        hasCondition = true;
                    }
                    if (!string.IsNullOrEmpty(_lrinvoice.invoiceno))
                    {
                        qb.AddParameter("lrinvoice.invoiceno", "invoiceno", _lrinvoice.invoiceno, "=");
                        hasCondition = true;
                    }
                    if (!string.IsNullOrEmpty(_lrinvoice.lrno))
                    {
                        qb.AddParameter("lrentry.lrno", "lrno", _lrinvoice.lrno, "=");
                        hasCondition = true;
                    }
                    if (!string.IsNullOrEmpty(_lrinvoice.lrentryno))
                    {
                        qb.AddParameter("lrentryno", _lrinvoice.lrentryno, "=");
                        hasCondition = true;
                    }
                    if (!string.IsNullOrEmpty(_lrinvoice.invoiceentryno))
                    {
                        qb.AddParameter("invoiceentryno", _lrinvoice.invoiceentryno, "=");
                        hasCondition = true;
                    }
                    if (_lrinvoice.supplierid > 0)
                    {
                        qb.AddParameter("suppliercompany.supplierid", "supplierid", _lrinvoice.supplierid, "=");
                        hasCondition = true;
                    }

                    if (_lrinvoice.fromdate.Year > 1900)
                    {
                        qb.AddParameter("lrinvoice.invoicedate", "fromdate", _lrinvoice.fromdate.Date, ">=");
                        hasCondition = true;
                    }
                    if (_lrinvoice.todate.Year > 1900)
                    {
                        qb.AddParameter("lrinvoice.invoicedate", "todate", _lrinvoice.todate.Date.AddDays(1), "<");
                        hasCondition = true;
                    }

                    if (_lrinvoice.entryfromdate.Year > 1900)
                    {
                        qb.AddParameter("lrinvoice.entrydate", "entryfromdate", _lrinvoice.entryfromdate.Date, ">=");
                        hasCondition = true;
                    }
                    if (_lrinvoice.entrytodate.Year > 1900)
                    {
                        qb.AddParameter("lrinvoice.entrydate", "entrytodate", _lrinvoice.entrytodate.Date.AddDays(1), "<");
                        hasCondition = true;
                    }
                    if (!string.IsNullOrEmpty(_lrinvoice.lrentryno))
                    {
                        qb.AddParameter("lrentryno", _lrinvoice.lrentryno, "=");
                        hasCondition = true;
                    }
                    if (_lrinvoice.LimitOne)
                    {
                        qb.Limit = 1;
                        qb.SortField = "lrinvoice.id desc";
                    }
                    else if (!IsMigration)
                    {
                        qb.Limit = 10000;
                        qb.SortField = "lrinvoice.id desc";
                    }
                    command = qb.GetCommand(conn);
                    if (_lrinvoice.barcodedfromdate != DateTime.MinValue && _lrinvoice.barcodedtodate != DateTime.MinValue)
                    {
                        command.Parameters.Add("barcodedfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _lrinvoice.barcodedfromdate;
                        command.Parameters.Add("barcodedto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _lrinvoice.barcodedtodate.Date.AddDays(1);
                        hasCondition = true;
                    }

                    if (!hasCondition)
                        throw new Exception("Please refine search criteria.");
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoice olrinvoice = new Lrinvoice();
                        olrinvoice.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        olrinvoice.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"].ToString());
                        olrinvoice.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"].ToString());
                        olrinvoice.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        olrinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        olrinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["invoicedate"];
                        olrinvoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        if (hasBarcoded)
                            olrinvoice.barcodedon = reader["barcodedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["barcodedon"]);
                        olrinvoice.baseamount = reader["baseamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["baseamount"].ToString());
                        olrinvoice.grossamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"].ToString());
                        olrinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrinvoice.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        olrinvoice.lrsupplierid = reader["lrsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrsupplierid"]);
                        olrinvoice.supplierid = reader["lrsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrsupplierid"]);
                        olrinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        olrinvoice.suppliercompany = reader["suppliercompanyname"] == DBNull.Value ? "" : reader["suppliercompanyname"].ToString();
                        olrinvoice.suppliercityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        olrinvoice.noofbundle = reader["noofbundle"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundle"]);
                        olrinvoice.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"]);
                        olrinvoice.otheramount = reader["otheramount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["otheramount"].ToString());
                        olrinvoice.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"].ToString());
                        olrinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoice.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"].ToString());
                        olrinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoice.hasapprove = reader["hasapprove"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasapprove"]);
                        olrinvoice.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        olrinvoice.iscreditnote = reader["iscreditnote"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscreditnote"]);
                        olrinvoice.settled = reader["settled"] == DBNull.Value ? false : Convert.ToBoolean(reader["settled"]);
                        olrinvoice.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        olrinvoice.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"].ToString());
                        olrinvoice.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        olrinvoice.invoiceentryno = reader["invoiceentryno"] == DBNull.Value ? "" : reader["invoiceentryno"].ToString();
                        olrinvoice.isinclusivetax = reader["isinclusivetax"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusivetax"]);
                        olrinvoice.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                        olrinvoice.isdirectpurchase = reader["isdirectpurchase"] == DBNull.Value ? false : Convert.ToBoolean(reader["isdirectpurchase"]);
                        olrinvoice.isdeliverychallan = reader["isdeliverychallan"] == DBNull.Value ? false : Convert.ToBoolean(reader["isdeliverychallan"]);
                        olrinvoice.isinclusiveall = reader["isinclusiveall"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusiveall"]);
                        olrinvoice.isinclusivediscount = reader["isinclusivediscount"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusivediscount"]);
                        olrinvoice.hascommission = reader["hascommission"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascommission"]);
                        olrinvoice.supportpo = reader["supportpo"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportpo"]);
                        olrinvoice.isvendorbill = reader["isvendorbill"] == DBNull.Value ? false : Convert.ToBoolean(reader["isvendorbill"]);
                        olrinvoice.isgenerated = reader["isgenerated"] == DBNull.Value ? false : Convert.ToBoolean(reader["isgenerated"]);
                        olrinvoice.barcodegenerationapproval = reader["barcodegenerationapproval"] == DBNull.Value ? false : Convert.ToBoolean(reader["barcodegenerationapproval"]);

                        olrinvoice.hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);
                        olrinvoice.isrcm = reader["isrcm"] == DBNull.Value ? false : Convert.ToBoolean(reader["isrcm"]);
                        olrinvoice.tdsledgerid = reader["tdsledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsledgerid"]);
                        olrinvoice.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"].ToString());
                        olrinvoice.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"].ToString());

                        olrinvoice.tcsamount = reader["tscamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tscamount"].ToString());
                        olrinvoice.hastcs = olrinvoice.tcsamount > 0;
                        olrinvoice.qcby = reader["qcby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["qcby"]);

                        olrinvoice.buyerinvoiceid = reader["buyerinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["buyerinvoiceid"]);
                        olrinvoice.expensepercentage = reader["expensepercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expensepercentage"].ToString());
                        olrinvoice.expenseperpiece = reader["expenseperpiece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expenseperpiece"].ToString());

                        olrinvoice.plessdiscount = reader["plessdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plessdiscount"].ToString());
                        olrinvoice.plessvalue = reader["plessvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["plessvalue"].ToString());
                        olrinvoice.createduser = reader["createduser"] == DBNull.Value ? "" : reader["createduser"].ToString();
                        olrinvoice.modifieduser = reader["modifieduser"] == DBNull.Value ? "" : reader["modifieduser"].ToString();
                        olrinvoice.isintercompanytransfer = reader["isintercompanytransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["isintercompanytransfer"]);

                        olrinvoice.ptype = reader["potypeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["potypeid"].ToString());
                        olrinvoice.purchaseorderid = reader["poid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["poid"].ToString());
                        olrinvoice.pono = reader["pono"] == DBNull.Value ? "" : Convert.ToString(reader["pono"]);
                        olrinvoice.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["podate"];
                        lst.Add(olrinvoice);
                    }

                    if (lst.Count == 1 && lst[0].buyerinvoiceid > 0 && lst[0].isvendorbill)
                        lst.First().allowgenerte = false;

                    if (lst.Count == 1 && lst[0].statusid == 1)
                        lst.First().allowgenerte = false;

                    if (lst.Count > 0 && lst.First().allowgenerte &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_barcode &&
                        reader.NextResult() && reader.Read())
                        lst.First().allowgenerte = reader[0] == DBNull.Value ? false : reader.GetBoolean(0);
                }



                if (showitems && (_lrinvoice.id > 0 || _lrinvoice.showitems || _lrinvoice.importreferenceid > 0) && lst.Count == 1)
                {
                    LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                    subLrinvoiceitems.Context = this.Context;
                    lst[0].Lrinvoiceitems = subLrinvoiceitems.SearchAllDetails(lst[0].id);
                    lst[0].hasitems = (lst[0].Lrinvoiceitems != null && lst[0].Lrinvoiceitems.Count > 0);
                }
                if (lst.Count == 1 && lst[0].hasitems == false)
                {
                    LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                    subLrinvoiceitems.Context = this.Context;
                    lst[0].hasitems = subLrinvoiceitems.HasInvoiceItems(lst[0].id);
                }
                LrinvoicetaxDataService subtaxservice = new LrinvoicetaxDataService() { Context = this.Context };
                subtaxservice.Context = this.Context;
                if (!IsMigration && lst.Count <= 10)
                {
                    foreach (var item in lst)
                    {
                        item.Lrinvoicetaxitems = subtaxservice.SearchAll(item.id);
                    }
                }

                if (!IsMigration && !hasBarcoded && _lrinvoice.id == 0 && lst.Count > 0)
                {
                    string query = @"select lrinvoiceid,min(barcodedon) barcodedon from lrinvoiceitems where lrinvoiceid in (" +
                        string.Join(",", lst.ConvertAll(x => x.id)) + ") and isactive=true group by lrinvoiceid";
                    using (var reader = ExecuteReader(query, null, conn))
                    {
                        while(reader.Read())
                        {
                            long id = Convert.ToInt64(reader["lrinvoiceid"]);
                            DateTime barcoded = reader["barcodedon"] != DBNull.Value ? Convert.ToDateTime(reader["barcodedon"]) : DateTime.MinValue;
                            lst.Find(x => x.id == id).barcodedon = barcoded;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoice);
            }
            return lst;
        }

        internal Lrinvoice UpdateDirectPurchase(LrinvoiceDetail item)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_validatesellingprice &&
                item.Lrinvoiceitems.Exists(e => e.sellingprice < e.buyingprice))
            {
                throw new Exception("Selling price must be greater than Cost price Refer S.NO (" +
                        item.Lrinvoiceitems.Find(e => e.sellingprice <= e.buyingprice).sno + ")");
            }

            var result = Search(new LrinvoiceCreteria()
            {
                id = item.id
            });

            if (result.Count == 1)
            {
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                {
                    if (item.hasapprove)
                    {
                        throw new Exception("Approved Invoice can't be updated, please remove the approval");
                    }
                }
                return Update(item);
            }
            return item;
        }

        internal Lrinvoice InsertDirectPurchase(LrinvoiceDetail item)
        {
            return Insert(item);
        }
        internal LrinvoiceDetail UpdateVendorItems(LrinvoiceDetail oLrinvoice)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oLrinvoice.buyerinvoiceid > 0)
                    {
                        foreach (var item in oLrinvoice.Lrinvoiceitems)
                        {
                            item.supplierid = oLrinvoice.supplierid;
                        }
                        LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                        subLrinvoiceitems.Context = this.Context;
                        subLrinvoiceitems.Update(transaction, conn, oLrinvoice.Lrinvoiceitems, oLrinvoice.id, oLrinvoice.statusid == 4);
                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                            Inbox.StatusList.Items_Entering, transaction, conn, Context);
                        transaction.Commit();
                        conn.Close();
                    }
                    else
                    {
                        foreach (var item in oLrinvoice.Lrinvoiceitems)
                        {
                            item.supplierid = oLrinvoice.supplierid;
                        }
                        LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                        subLrinvoiceitems.Context = this.Context;
                        subLrinvoiceitems.Update(transaction, conn, oLrinvoice.Lrinvoiceitems, oLrinvoice.id, oLrinvoice.statusid == 4);
                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                            Inbox.StatusList.Items_Entering, transaction, conn, Context);


                        SuppliercompanyDataService supplierDataService = new SuppliercompanyDataService() { Context = this.Context };

                        CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                        var companylist = companyDataService.Search(new CompanyCreteria());

                        LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                        var lrentry = lrentryDataService.Search(new LrentryCreteria()
                        {
                            id = oLrinvoice.lrid
                        }).First();

                        var isinterstate = companylist.Find(e => e.id == oLrinvoice.transfercompanyid).stateid !=
                            companylist.Find(e => e.id == oLrinvoice.companyid).stateid;
                        var clone = Newtonsoft.Json.JsonConvert.SerializeObject(oLrinvoice);
                        var newinvoice = Newtonsoft.Json.JsonConvert.DeserializeObject<LrinvoiceDetail>(clone);
                        clone = Newtonsoft.Json.JsonConvert.SerializeObject(lrentry);
                        var newlrentry = Newtonsoft.Json.JsonConvert.DeserializeObject<Lrentry>(clone);

                        TaxDataService productsDataService = new TaxDataService() { Context = this.Context };
                        var taxlist = productsDataService.TaxProducts(newinvoice.Lrinvoiceitems.ConvertAll(e => e.productid));
                        foreach (var invoiceitem in newinvoice.Lrinvoiceitems)
                        {
                            invoiceitem.buyingprice += Math.Round(invoiceitem.buyingprice * newinvoice.premargin / 100, 0);
                            var taxpercentage = invoiceitem.purchasetaxpercentage;
                            if (taxlist.ContainsKey(invoiceitem.productid))
                            {
                                if (taxlist[invoiceitem.productid].taxsplit.items.Count == 1)
                                {
                                    var taxitem = taxlist[invoiceitem.productid].taxsplit.items[0];
                                    taxpercentage = taxitem.cgst + taxitem.sgst;
                                }
                                else
                                {
                                    var taxitem = taxlist[invoiceitem.productid].taxsplit.Find(invoiceitem.buyingprice);
                                    taxpercentage = taxitem.cgst + taxitem.sgst;
                                }
                                invoiceitem.taxid = taxlist[invoiceitem.productid].id;
                                invoiceitem.purchasetaxpercentage = taxpercentage;
                            }
                            invoiceitem.amount = (invoiceitem.buyingqty - invoiceitem.returnqty) * invoiceitem.buyingprice;
                            invoiceitem.buyingqty = invoiceitem.buyingqty - invoiceitem.returnqty;
                            invoiceitem.returnqty = 0;
                            invoiceitem.purchasetax = Math.Round(invoiceitem.amount * taxpercentage / 100, 4);
                            invoiceitem.sourceid = invoiceitem.id;
                            invoiceitem.id = 0;
                            invoiceitem.purchasediscount = 0;
                            invoiceitem.purchasediscountpercentage = 0;
                            invoiceitem.purchasetotaldiscount = 0;
                            invoiceitem.basebuyingprice = invoiceitem.buyingprice;
                            invoiceitem.salediscount = 0;
                            invoiceitem.salediscountpercentage = 0;
                            invoiceitem.isactive = true;
                            invoiceitem.version = 1;
                            invoiceitem.createdon = DateTimeNow;
                            invoiceitem.createdby = Context.UserID;
                            invoiceitem.modifiedon = DateTimeNow;
                            invoiceitem.modifiedby = Context.UserID;
                        }
                        newinvoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                        newinvoice.id = 0;
                        foreach (var taxitem in newinvoice.Lrinvoiceitems.GroupBy(e => new { e.taxid, e.purchasetaxpercentage }))
                        {
                            var amount = taxitem.Sum(e => e.amount);
                            var tax = taxitem.Sum(e => e.purchasetax);
                            newinvoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                            {
                                taxamount = tax,
                                taxid = taxitem.Key.taxid,
                                percentage = taxitem.Key.purchasetaxpercentage,
                                isinterstatesale = oLrinvoice.isinterstatesale,
                                amounton = amount,
                                netamount = amount + tax,
                                isactive = true
                            });
                        }
                        newinvoice.grossamount = Math.Round(newinvoice.Lrinvoicetaxitems.Sum(e => e.netamount));
                        newinvoice.baseamount = Math.Round(newinvoice.Lrinvoicetaxitems.Sum(e => e.type == 0 ? e.amounton : 0));
                        newinvoice.totalamount = Math.Round(newinvoice.Lrinvoicetaxitems.Sum(e => e.netamount));
                        newinvoice.billvalue = newinvoice.totalamount;
                        newinvoice.otheramount = 0;
                        newinvoice.discount = 0;
                        newinvoice.taxamount = newinvoice.Lrinvoicetaxitems.Sum(e => e.taxamount);
                        newlrentry.id = 0;
                        var billnosequence = SequenceManager.Instance(Context.OrganizationID).Get("bill.billno", 0, oLrinvoice.companyid, 0);
                        newinvoice.invoiceno = billnosequence.GetNextNumber(Context, conn, transaction);
                        newinvoice.invoiceentryno = oLrinvoice.invoiceno;
                        newinvoice.invoicedate = DateTimeNow.Date;
                        newinvoice.supplierid = companylist.Find(e => e.id == oLrinvoice.companyid).company_as_supplierid;
                        newinvoice.suppliercompanyid = supplierDataService.Search(new SuppliercompanyCreteria()
                        {
                            supplierid = companylist.Find(e => e.id == oLrinvoice.companyid).company_as_supplierid
                        }).First().id;
                        newlrentry.supplierid = newinvoice.supplierid;
                        newlrentry.sourcesupplierid = oLrinvoice.supplierid;
                        newlrentry.invoicedate = newinvoice.invoicedate;
                        newlrentry.invoiceno = newinvoice.invoiceno;
                        newlrentry.retaillocationid = oLrinvoice.retaillocationid;
                        newlrentry.lrcompanyid = oLrinvoice.transfercompanyid;
                        newlrentry.lrno += "(" + oLrinvoice.invoiceno + ")";
                        newinvoice.companyid = oLrinvoice.transfercompanyid;
                        newlrentry.lrentryno = SequenceManager.Instance(Context.OrganizationID).Get("lrentry.lrentryno", 0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
    newlrentry.lrcompanyid : 0).GetNextNumber(Context, conn, transaction);

                        newlrentry.isactive = true;
                        newlrentry.version = 1;
                        newlrentry.createdon = DateTimeNow;
                        newlrentry.createdby = Context.UserID;
                        newlrentry.modifiedon = DateTimeNow;
                        newlrentry.modifiedby = Context.UserID;
                        newinvoice.isactive = true;
                        newinvoice.version = 1;
                        newinvoice.createdon = DateTimeNow;
                        newinvoice.createdby = Context.UserID;
                        newinvoice.modifiedon = DateTimeNow;
                        newinvoice.modifiedby = Context.UserID;
                        newinvoice.isvendorbill = false;

                        lrentryDataService.Insert(transaction, conn, newlrentry);
                        InboxManager.Instance(Context.OrganizationID).Update(Context, true, newlrentry, transaction, conn);

                        newinvoice.lrid = newlrentry.id;
                        Insert(transaction, conn, newinvoice);
                        oLrinvoice.buyerinvoiceid = newinvoice.id;
                        oLrinvoice.isvendorbill = true;
                        oLrinvoice.isgenerated = true;
                        oLrinvoice.companyid = oLrinvoice.transfercompanyid;
                        Update(transaction, conn, oLrinvoice);
                        foreach (var item in oLrinvoice.Lrinvoiceitems)
                        {
                            item.supplierid = oLrinvoice.supplierid;
                        }

                        newinvoice.Lrinvoiceitems.RemoveAll(e => (e.buyingqty - e.returnqty) == 0);
                        subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                        subLrinvoiceitems.Context = this.Context;
                        subLrinvoiceitems.Update(transaction, conn, newinvoice.Lrinvoiceitems, newinvoice.id, oLrinvoice.statusid == 4);
                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(newlrentry.id, Inbox.StatusList.Items_Entering, transaction, conn, Context);

                        LrinvoicetaxDataService subLrinvoicetaxs = new LrinvoicetaxDataService() { Context = this.Context };
                        subLrinvoicetaxs.Context = this.Context;
                        newinvoice.Lrinvoicetaxitems.ForEach(e => e.isinterstatesale = oLrinvoice.isinterstatesale);
                        foreach (var item in newinvoice.Lrinvoicetaxitems)
                        {
                            item.invoiceid = newinvoice.id;
                            item.createdon = newinvoice.createdon;
                            item.modifiedon = DateTimeNow;
                            item.createdby = Context.UserID;
                            item.version = 1;
                            item.isactive = true;
                            subLrinvoicetaxs.Insert(transaction, conn, item);
                        }

                        //transaction.Commit();

                        //transaction = conn.BeginTransaction();
                        AutoBarcodeCreation(conn, transaction, newinvoice);
                        oLrinvoice.Lrinvoiceitems.ForEach(e =>
                        {
                            if (newinvoice.Lrinvoiceitems.Exists(ex => ex.sourceid == e.id))
                            {
                                e.barcodeList = newinvoice.Lrinvoiceitems.Find(ex => ex.sourceid == e.id).barcodeList;
                                subLrinvoiceitems.UpdateBarcode(transaction, conn, e);
                            }
                        });
                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(newlrentry.id,
                            Inbox.StatusList.Barcode_Generating, transaction, conn, Context);
                        transaction.Commit();
                        conn.Close();

                        // Auto debit note for vendor bills
                        foreach (var item in oLrinvoice.Lrinvoiceitems)
                        {
                            if (item.returnqty > 0)
                            {
                                item.damagedquantity = item.returnqty;
                                subLrinvoiceitems.UpdateDamagedQty(item);
                            }
                        }
                        oLrinvoice = newinvoice;
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
            }
            return oLrinvoice;
        }
        public LrinvoiceDetail CreateAutoDebitNote(LrinvoiceDetail oLrinvoice)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                oLrinvoice.Lrinvoiceitems.Exists(x => x.actualqty != x.buyingqty && x.buyingqty > 0))
            {
                string query = @"select sum(buyingqty*buyingprice) buyingvalue,
                                                    sum(actualqty*buyingprice) actualvalue,
                                                    sum(buyingqty-actualqty) qtydiff
                                                    from lrinvoiceitems where lrinvoiceid = @invoiceid
                                                    and isactive=true";

                var param = new SortedList<string, object>();
                param.Add("invoiceid", oLrinvoice.id);
                decimal buyingvalue = 0;
                decimal actualvalue = 0;
                decimal qtydiff = 0;
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                    {
                        buyingvalue = reader["buyingvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingvalue"]);
                        actualvalue = reader["actualvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualvalue"]);
                        qtydiff = reader["qtydiff"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qtydiff"]);
                    }
                }
                if ((buyingvalue - actualvalue) > 0)
                {
                    if (qtydiff > 0)
                    {
                        var debitqty = JsonConvert.DeserializeObject<List<LrinvoiceItems>>(JsonConvert.SerializeObject(oLrinvoice.Lrinvoiceitems.FindAll(x => x.actualqty < x.buyingqty)));
                        var creditqty = JsonConvert.DeserializeObject<List<LrinvoiceItems>>(JsonConvert.SerializeObject(oLrinvoice.Lrinvoiceitems.FindAll(x => x.actualqty > x.buyingqty)));
                        foreach (var cr in creditqty.ToList())
                        {
                            var required = cr.actualqty - cr.buyingqty;
                            if (debitqty.Exists(x => x.buyingprice == cr.buyingprice))
                            {
                                foreach (var exqty in debitqty.FindAll(x => x.buyingprice == cr.buyingprice))
                                {
                                    var extraqty = exqty.buyingqty - exqty.actualqty;
                                    if (required < extraqty)
                                    {
                                        cr.actualqty -= required;
                                        exqty.actualqty += required;
                                        required = 0;
                                    }
                                    else if (extraqty > 0)
                                    {
                                        cr.actualqty -= extraqty;
                                        exqty.actualqty += extraqty;
                                        required -= extraqty;
                                    }
                                    if (required == 0) break;
                                }
                            }
                        }
                        debitqty.RemoveAll(x => x.actualqty == x.buyingqty);
                        creditqty.RemoveAll(x => x.actualqty == x.buyingqty);
                        foreach (var item in debitqty)
                        {
                            if (item.actualqty < item.buyingqty)
                            {
                                LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                lrinvoicereturnDataService.Context = this.Context;
                                lrinvoicereturnDataService.AddDirectDebitNote(item, item.buyingqty - item.actualqty, "Received lesser qty");
                            }
                        }
                    }
                    else
                    {
                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                        lrinvoicereturnDataService.Context = this.Context;
                        lrinvoicereturnDataService.AddDiscountNote(Lrinvoicereturn.ReturnTypes.DiscountDebityNote, oLrinvoice.id,
                        buyingvalue - actualvalue, "Discount for value different");
                    }
                }
            }

            return oLrinvoice;
        }

        public LrinvoiceDetail UpdateItems(LrinvoiceDetail oLrinvoice)
        {

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
            {
                if(oLrinvoice.hasapprove)
                {
                    throw new Exception("Approved Invoice can't be updated, please remove the approval");
                }
            }


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_validatesellingprice &&
                oLrinvoice.Lrinvoiceitems.Exists(e => e.sellingprice < e.buyingprice))
            {
                throw new Exception("Selling price must be greater than Cost price Refer S.NO (" +
                        oLrinvoice.Lrinvoiceitems.Find(e => e.sellingprice <= e.buyingprice).sno + ")");
            }

            if (oLrinvoice.isvendorbill && oLrinvoice.transfercompanyid > 0 && oLrinvoice.statusid == 2)
            {
                return UpdateVendorItems(oLrinvoice);
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_dealerprice &&
                oLrinvoice.Lrinvoiceitems.Exists(e => e.dealerprice == 0))
            {
                throw new Exception("Please enter dealer price and must be greater than zero. S.NO (" +
                    oLrinvoice.Lrinvoiceitems.Find(e => e.dealerprice == 0).sno + ")");
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oLrinvoice.statusid != 4)
                    {
                        string lrinvoicetablelock = "update lrinvoice set version=@version+1 where id=@id and isactive=true RETURNING version";
                        NpgsqlCommand command = GetCommand(lrinvoicetablelock, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.id;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.version;
                        bool isinprocess = true;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                oLrinvoice.version = Convert.ToInt32(reader[0]);
                                isinprocess = false;
                            }
                        }

                        if (isinprocess)
                        {
                            transaction.Rollback();
                            conn.Close();
                            throw new Exception("Other user updated the invoice, Please reload the invoice details and save again");
                        }
                    }

                    foreach (var item in oLrinvoice.Lrinvoiceitems)
                    {
                        item.supplierid = oLrinvoice.supplierid;
                    }
                    LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                    subLrinvoiceitems.Context = this.Context;

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                    {
                        LrinvoicetaxDataService subLrtaxitems = new LrinvoicetaxDataService() { Context = this.Context };
                        subLrtaxitems.Update(transaction, conn, oLrinvoice.Lrinvoicetaxitems, oLrinvoice.id);
                    }

                    subLrinvoiceitems.Update(transaction, conn, oLrinvoice.Lrinvoiceitems, oLrinvoice.id, oLrinvoice.statusid == 4);
                    InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                        (oLrinvoice.ispricing ? Inbox.StatusList.Item_Pricing : Inbox.StatusList.Items_Entering), transaction, conn, Context);

                    if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                        oLrinvoice.statusid != 1 && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_barcode)
                    {
                        var param = new SortedList<string, object>();
                        IntrayDataService intrayDataService = new IntrayDataService() { Context = this.Context };
                        var existingIntray = intrayDataService.Search(new IntrayCreteria()
                        {
                            referencetype = (int)Services.IntrayManager.EntryType.BarcodeGeneration,
                            referenceid = oLrinvoice.id
                        });

                        if (existingIntray.Count == 0 || existingIntray.All(e => e.ActionType == Intray.IntrayActionTypes.Approved))
                        {
                            SuppliercompanyDataService supplierdata = new SuppliercompanyDataService() { Context = this.Context };
                            var supplier = supplierdata.Search(new SuppliercompanyCreteria()
                            {
                                supplierid = oLrinvoice.supplierid
                            }).First();

                            var purchasecost = oLrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty * (e.basebuyingprice - e.purchasediscount));
                            var salesprice = oLrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty * (e.sellingprice - e.salediscount));
                            var diff = Math.Round(oLrinvoice.billvalue - oLrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty * (e.basebuyingprice - e.purchasediscount)), 0);
                            var margin = salesprice > 0 ? Math.Round(purchasecost / salesprice * 100, 0) : 0;
                            var poqty = oLrinvoice.Lrinvoiceitems.Sum(e => e.poinfo == null ? 0 : e.poinfo.Sum(x => x.aqty));
                            var poextraqty = oLrinvoice.Lrinvoiceitems.Sum(e => e.poinfo == null ? 0 : e.poinfo.Sum(x => x.aqty > x.oqty ? x.aqty - x.oqty : 0));

                            List<string> fixes = new List<string>();
                            if (diff > 1)
                                fixes.Add("Diff : " + diff.ToString("F0"));
                            fixes.Add("Margin : " + margin.ToString("F0"));
                            if (poqty > 0)
                                fixes.Add("Ordered : " + poqty.ToString("F0"));
                            if (poextraqty > 0)
                                fixes.Add("Extra : " + poextraqty.ToString("F0"));

                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.BarcodeGeneration,
                                oLrinvoice.id, string.Format("{0} / {1} on {2} / {3} - {4} ({5})", supplier.name, oLrinvoice.invoiceno,
                                oLrinvoice.invoicedate.ToString("dd-MM-yyyy"),
                                oLrinvoice.totalamount.ToString("F0"),
                                oLrinvoice.lrentryno,
                                string.Join(",", fixes)),
                                Context.UserID, 0, transaction, Context);
                        }
                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                        oLrinvoice.Lrinvoiceitems.All(x => x.barcodes == null || x.barcodes == "null"))
                    {
                        if (oLrinvoice.purchaseorderid > 0)
                        {
                            var param = new SortedList<string, object>();
                            param.Add("id", oLrinvoice.purchaseorderid);
                            param.Add("internalstatus", oLrinvoice.internalstatus);
                            ExecuteQuery("update purchaseorder set internalstatus=@internalstatus where id=@id and internalstatus<@internalstatus", param, conn, transaction);
                        }

                        if (oLrinvoice.statusid == 3 ||
                            oLrinvoice.statusid == 9 ||
                            oLrinvoice.internalstatus == 9)
                        {
                            SortedDictionary<long, decimal> debitenotes = new SortedDictionary<long, decimal>();
                            foreach (var item in oLrinvoice.Lrinvoiceitems)
                            {
                                if (item.buyingqty > (item.actualqty + item.returnqty))
                                {
                                    debitenotes.Add(item.id, item.buyingqty - (item.actualqty + item.returnqty));
                                    item.returnqty = item.buyingqty - item.actualqty;
                                }
                            }
                            AutoBarcodeCreation(conn, transaction, oLrinvoice);
                            oLrinvoice.Lrinvoiceitems.ForEach(e =>
                            {
                                subLrinvoiceitems.UpdateBarcode(transaction, conn, e);
                            });
                            InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                                Inbox.StatusList.Barcode_Generating, transaction, conn, Context);


                            transaction.Commit();
                            conn.Close();

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                                oLrinvoice.Lrinvoiceitems.Exists(x => x.actualqty != x.buyingqty && x.buyingqty > 0))
                            {
                                string query = @"select sum(buyingqty*buyingprice) buyingvalue,
                                                    sum(actualqty*buyingprice) actualvalue,
                                                    sum(buyingqty-actualqty) qtydiff
                                                    from lrinvoiceitems where lrinvoiceid = @invoiceid
                                                    and isactive=true";

                                var param = new SortedList<string, object>();
                                param.Add("invoiceid", oLrinvoice.id);
                                decimal buyingvalue = 0;
                                decimal actualvalue = 0;
                                decimal qtydiff = 0;

                                conn.Open();
                                using (var reader = ExecuteReader(query, param, conn))
                                {
                                    if (reader.Read())
                                    {
                                        buyingvalue = reader["buyingvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingvalue"]);
                                        actualvalue = reader["actualvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualvalue"]);
                                        qtydiff = reader["qtydiff"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qtydiff"]);
                                    }
                                }
                                conn.Close();

                                if ((buyingvalue - actualvalue) > 0)
                                {
                                    if (qtydiff > 0)
                                    {
                                        var debitqty = JsonConvert.DeserializeObject<List<LrinvoiceItems>>(JsonConvert.SerializeObject(oLrinvoice.Lrinvoiceitems.FindAll(x => x.actualqty < x.buyingqty)));
                                        var creditqty = JsonConvert.DeserializeObject<List<LrinvoiceItems>>(JsonConvert.SerializeObject(oLrinvoice.Lrinvoiceitems.FindAll(x => x.actualqty > x.buyingqty)));
                                        foreach (var cr in creditqty.ToList())
                                        {
                                            var required = cr.actualqty - cr.buyingqty;
                                            if (debitqty.Exists(x => x.buyingprice == cr.buyingprice))
                                            {
                                                foreach (var exqty in debitqty.FindAll(x => x.buyingprice == cr.buyingprice))
                                                {
                                                    var extraqty = exqty.buyingqty - exqty.actualqty;
                                                    if (required < extraqty)
                                                    {
                                                        cr.actualqty -= required;
                                                        exqty.actualqty += required;
                                                        required = 0;
                                                    }
                                                    else if (extraqty > 0)
                                                    {
                                                        cr.actualqty -= extraqty;
                                                        exqty.actualqty += extraqty;
                                                        required -= extraqty;
                                                    }
                                                    if (required == 0) break;
                                                }
                                            }
                                        }
                                        debitqty.RemoveAll(x => x.actualqty == x.buyingqty);
                                        creditqty.RemoveAll(x => x.actualqty == x.buyingqty);
                                        foreach (var item in debitqty)
                                        {
                                            if (item.actualqty < item.buyingqty)
                                            {
                                                if (!debitenotes.ContainsKey(item.id)) continue;
                                                LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                                lrinvoicereturnDataService.Context = this.Context;
                                                lrinvoicereturnDataService.AddDirectDebitNote(item, item.buyingqty - item.actualqty, "Received lesser qty");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                        lrinvoicereturnDataService.Context = this.Context;
                                        lrinvoicereturnDataService.AddDiscountNote(Lrinvoicereturn.ReturnTypes.DiscountDebityNote, oLrinvoice.id,
                                        buyingvalue - actualvalue, "Discount for value different");
                                    }
                                }
                            }
                            else if (oLrinvoice.Lrinvoiceitems.Exists(x => x.actualqty > 0 && x.actualqty != x.buyingqty))
                            {
                                foreach (var item in oLrinvoice.Lrinvoiceitems)
                                {
                                    if (item.actualqty > 0 && item.actualqty < item.buyingqty)
                                    {
                                        LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                        lrinvoicereturnDataService.Context = this.Context;
                                        lrinvoicereturnDataService.AddDirectDebitNote(item, item.buyingqty - item.actualqty, "Received lesser qty");
                                    }
                                }
                            }

                        }
                        else
                        {
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                    else
                    {
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                oLrinvoice.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
            }
            return oLrinvoice;
        }

        internal bool ChangeSettled(InvoiceSetted item)
        {
            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = @"update lrinvoice set settled=@settled,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")";
                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("settled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.changetosettled;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

            }
            return successCount > 0;
        }

        internal bool DeleteInvoice(Lrinvoice item)
        {
            NpgsqlTransaction transaction = null;
            NpgsqlCommand command = null;
            int successCount = 0;
            int _count = 0;
            string _sql = @"select count(id) countno from supplierpaymentinvoice 
                        where isactive=true and islrinvoicereturn=false and invoiceid=@id";

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    command = GetCommand(_sql, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                           _count = reader["countno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["countno"]);
                        }
                    }
                    if (_count > 0)
                        throw new Exception("Paid invoices cannot be deleted.");


                    transaction = conn.BeginTransaction();
                    command = GetCommand(SQL_DeleteInvoice, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    successCount = command.ExecuteNonQuery();
                    if (item.id > 0)
                    {
                        LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                        lrinvoiceitemsDataService.DeleteAll(item, Context.UserID, conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

            }
            return successCount > 0;
        }

        internal bool Delete(Lrinvoice item)
        {
            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.lrid;
                    successCount = command.ExecuteNonQuery();
                    if(item.id>0)
                    {
                        LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                        lrinvoiceitemsDataService.DeleteAll(item, Context.UserID, conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

            }
            return successCount > 0;
        }
        private bool DuplicateValidation(LrinvoiceCover oLrinvoice)
        {
            string _Query = "select count(id) items from lrinvoice where suppliercompanyid=@suppliercompanyid and invoiceno=@invoiceno and id!=@id";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.Items[0].id;
                command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.Items[0].suppliercompanyid;
                command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoice.Items[0].invoiceno;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count > 0;
        }

        internal string LastEntry(string companyid)
        {
            string entryno = "";
            using (NpgsqlConnection conn = CreateConnection())
            {
                LrInformation ret = new LrInformation();

                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = "select lrinvoice.invoiceentryno from lrinvoice ";
                qb.AddParameter("lrinvoice.isactive", true, "=");
                long cid = 0;
                if (long.TryParse(companyid, out cid))
                {
                    qb.AddParameter("lrinvoice.companyid", cid, "=");
                }
                qb.Limit = 1;
                using (var reader = qb.GetCommand(conn).ExecuteReader())
                {
                    if (reader.Read())
                    {
                        entryno = reader.GetString(0);
                    }
                }
                conn.Close();
            }
            return entryno;
        }

        public Lrinvoice ImportInvoice(Lrinvoice oLrinvoice, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
            LrinvoicetaxDataService subLrinvoicetaxs = new LrinvoicetaxDataService() { Context = this.Context };
            subLrinvoiceitems.Context = this.Context;
            subLrinvoicetaxs.Context = this.Context;

            if (string.IsNullOrEmpty(oLrinvoice.invoiceentryno))
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrinvoice.invoiceentryno",
                0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                oLrinvoice.companyid : 0);
                oLrinvoice.invoiceentryno = sequence.GetNextNumber(Context, conn, transaction);
            }
            if (oLrinvoice.id == 0)
            {
                Insert(transaction, conn, oLrinvoice);
            }
            else
            {
                subLrinvoiceitems.DeleteAll(oLrinvoice, Context.UserID, conn, transaction);
                subLrinvoicetaxs.DeleteAll(oLrinvoice.id, Context.UserID, conn, transaction);
                Update(transaction, conn, oLrinvoice);
            }

            ImportInvoiceItems(oLrinvoice, transaction, conn);
            oLrinvoice.Lrinvoicetaxitems.ForEach(e => e.isinterstatesale = oLrinvoice.isinterstatesale);
            foreach (var item in oLrinvoice.Lrinvoicetaxitems)
            {
                item.invoiceid = oLrinvoice.id;
                item.createdon = oLrinvoice.createdon;
                item.modifiedon = DateTimeNow;
                item.createdby = Context.UserID;

                if (item.amounton == item.netamount && item.taxamount > 0)
                {
                    throw new Exception("Tax,Taxable and Net Amount missmatches on LRInvoiceTax");
                }

                subLrinvoicetaxs.Insert(transaction, conn, item);
            }
            return oLrinvoice;
        }

        public void ImportInvoiceItems(Lrinvoice oLrinvoice, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
            ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
            foreach (var item in oLrinvoice.Lrinvoiceitems)
            {
                item.lrinvoiceid = oLrinvoice.id;

                if (item.itemid == 0)
                {
                    var ritem = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = item.Details.brandid,
                        productid = item.productid,
                        sizeid = item.Details.sizeid,
                        typeid = item.Details.typeid,
                        styleid = item.Details.styleid,
                        materialid = item.Details.materialid,
                        patternid = item.Details.patternid,
                        colourid = item.Details.colourid,
                        fitid = item.Details.fitid,
                        sleeveid = item.Details.sleeveid,
                        gln = item.Details.serialno,
                        serialno = item.Details.serialno,
                        glndesignid = item.Details.glndesignid,
                        support_gln = !string.IsNullOrEmpty(item.Details.serialno),
                        agmtcode = item.itbuscode,
                        productmap = true
                    });
                    //item.Details.productid = item.productid;
                    if (ritem != null && ritem.Count > 0)
                    {
                        item.itemid = ritem[0].id;
                    }
                    else if (item.Details.sourceitemid > 0)
                    {
                        ritem = itemservice.Search(new ItemsCreteria()
                        {
                            id = item.Details.sourceitemid
                        });
                        if (ritem.Count == 1)
                        {
                            ritem[0].productid = item.Details.productid;
                            ritem[0].productname = item.Details.productname;
                            ritem[0].sizeid = item.Details.sizeid;
                            ritem[0].sizename = item.Details.sizename;
                            ritem[0].patternid = item.Details.patternid;
                            ritem[0].patternname = item.Details.patternname;
                            ritem[0].materialid = item.Details.materialid;
                            ritem[0].materialname = item.Details.materialname;
                            ritem[0].typeid = item.Details.typeid;
                            ritem[0].typename = item.Details.typename;
                            ritem[0].colourid = item.Details.colourid;
                            ritem[0].colourname = item.Details.colourname;
                            ritem[0].brandid = item.Details.brandid;
                            ritem[0].brandname = item.Details.brandname;
                            ritem[0].fitid = item.Details.fitid;
                            ritem[0].fitname = item.Details.fitname;
                            ritem[0].sleeveid = item.Details.sleeveid;
                            ritem[0].sleevename = item.Details.sleevename;
                            ritem[0].styleid = item.Details.styleid;
                            ritem[0].stylename = item.Details.stylename;
                            ritem[0].gln = item.Details.serialno;
                            ritem[0].serialno = item.Details.serialno;
                            ritem[0].glndesignid = item.Details.glndesignid;
                            ritem[0].support_gln = !string.IsNullOrEmpty(item.Details.serialno);
                            ritem[0].id = 0;
                            ritem[0].createdon = DateTimeNow;
                            ritem[0].modifiedon = DateTimeNow;
                            ritem[0].createdby = Context.UserID;
                            ritem[0].modifiedby = Context.UserID;
                            itemservice.Insert(ritem[0]);
                            item.itemid = ritem[0].id;
                        }
                    }
                    else
                    {
                        var items = new Items()
                        {
                            productid = item.Details.productid,
                            productname = item.Details.productname,
                            sizeid = item.Details.sizeid,
                            sizename = item.Details.sizename,
                            patternid = item.Details.patternid,
                            patternname = item.Details.patternname,
                            materialid = item.Details.materialid,
                            materialname = item.Details.materialname,
                            typeid = item.Details.typeid,
                            typename = item.Details.typename,
                            colourid = item.Details.colourid,
                            colourname = item.Details.colourname,
                            brandid = item.Details.brandid,
                            brandname = item.Details.brandname,
                            styleid = item.Details.styleid,
                            stylename = item.Details.stylename,
                            fitid = item.Details.fitid,
                            fitname = item.Details.fitname,
                            sleeveid = item.Details.sleeveid,
                            sleevename = item.Details.sleevename,
                            printingname = string.IsNullOrEmpty(item.Details.printingname) ? item.Details.productname : item.Details.printingname,
                            sellingname = item.Details.sellingname,
                            gln = item.Details.serialno,
                            serialno = item.Details.serialno,
                            glndesignid = item.Details.glndesignid,
                            support_gln = !string.IsNullOrEmpty(item.Details.serialno)
                        };
                        itemservice.Insert(transaction, conn, items);
                        item.itemid = items.id;
                    }
                }
                subLrinvoiceitems.Insert(transaction, conn, item);
            }
        }
        public Lrinvoice Insert(LrinvoiceDetail oLrinvoice)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_validatesellingprice &&
                oLrinvoice.Lrinvoiceitems.Exists(e => e.sellingprice < e.buyingprice))
            {
                throw new Exception("Selling price must be greater than Cost price Refer S.NO (" +
                        oLrinvoice.Lrinvoiceitems.Find(e => e.sellingprice <= e.buyingprice).sno + ")");
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oLrinvoice = Insert(oLrinvoice, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
            }
            return oLrinvoice;
        }
        public LrinvoiceDetail Insert(LrinvoiceDetail oLrinvoice, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            if (oLrinvoice.supplierid == 0 && oLrinvoice.lrsupplierid > 0)
                oLrinvoice.supplierid = oLrinvoice.lrsupplierid;

            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrinvoice.invoiceentryno", 0);
            oLrinvoice.invoiceentryno = sequence.GetNextNumber(Context, conn, transaction);
            if (oLrinvoice.lrid <= 0 && oLrinvoice.isdirectpurchase)
            {
                //long retaillocation = 0;
                //if (InfyPOS.Services.CacheManager.Instance.Locations != null &&
                //    InfyPOS.Services.CacheManager.Instance.Locations.Sum(e => e.iswarehouse ? 0 : 1) >0)
                //{
                //    retaillocation = InfyPOS.Services.CacheManager.Instance.Locations.FindAll(e => !e.iswarehouse).OrderBy(e=>e.id).First().id;
                //}
                LrentryDataService lrentry = new LrentryDataService() { Context = this.Context };
                lrentry.Context = this.Context;
                Lrentry newentry = lrentry.Insert(new Lrentry()
                {
                    invoiceno = oLrinvoice.invoiceno,
                    invoicedate = oLrinvoice.invoicedate,
                    supplierid = oLrinvoice.lrsupplierid,
                    createdon = DateTimeNow,
                    createdby = Context.UserID,
                    lrdate = oLrinvoice.lrdate,
                    receiveddate = DateTimeNow,
                    modeofdelivery = 2,
                    lrcompanyid = oLrinvoice.companyid,
                    retaillocationid = oLrinvoice.retaillocationid,
                    entrylocationid = oLrinvoice.entrylocationid,
                    transportid = oLrinvoice.transportid,
                    noofbundels = oLrinvoice.bundles,
                    lrno = oLrinvoice.lrno
                });

                if (newentry.id == 0 &&
                    !string.IsNullOrEmpty(newentry.ErrorMessage))
                {
                    throw new Exception(newentry.ErrorMessage);
                }
                oLrinvoice.lrid = newentry.id;


            }

            ImportInvoice(oLrinvoice, transaction, conn);

            InboxManager.Instance(Context.OrganizationID).Update(Context, oLrinvoice, transaction, conn);
            InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                Inbox.StatusList.Items_Entering, transaction, conn, Context);
            if (oLrinvoice.isdirectpurchase && oLrinvoice.statusid != 2)
                InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid, Inbox.StatusList.Barcode_Generating, transaction, conn, Context);



            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                oLrinvoice.statusid != 1 && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_barcode)
            {
                var param = new SortedList<string, object>();
                IntrayDataService intrayDataService = new IntrayDataService() { Context = this.Context };
                var existingIntray = intrayDataService.Search(new IntrayCreteria()
                {
                    referencetype = (int)Services.IntrayManager.EntryType.BarcodeGeneration,
                    referenceid = oLrinvoice.id
                });

                if (existingIntray.Count == 0 || existingIntray.All(e => e.ActionType == Intray.IntrayActionTypes.Approved))
                {
                    SuppliercompanyDataService supplierdata = new SuppliercompanyDataService() { Context = this.Context };
                    var supplier = supplierdata.Search(new SuppliercompanyCreteria()
                    {
                        supplierid = oLrinvoice.supplierid
                    }).First();

                    var purchasecost = oLrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty * (e.basebuyingprice - e.purchasediscount));
                    var salesprice = oLrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty * (e.sellingprice - e.salediscount));
                    var diff = Math.Round(oLrinvoice.billvalue - oLrinvoice.Lrinvoiceitems.Sum(e => e.buyingqty * (e.basebuyingprice - e.purchasediscount)), 0);
                    var margin = salesprice > 0 ? Math.Round(purchasecost / salesprice * 100, 0) : 0;
                    var poqty = oLrinvoice.Lrinvoiceitems.Sum(e => e.poinfo == null ? 0 : e.poinfo.Sum(x => x.aqty));
                    var poextraqty = oLrinvoice.Lrinvoiceitems.Sum(e => e.poinfo == null ? 0 : e.poinfo.Sum(x => x.aqty > x.oqty ? x.aqty - x.oqty : 0));

                    List<string> fixes = new List<string>();
                    if (diff > 1)
                        fixes.Add("Diff : " + diff.ToString("F0"));
                    fixes.Add("Margin : " + margin.ToString("F0"));
                    if (poqty > 0)
                        fixes.Add("Ordered : " + poqty.ToString("F0"));
                    if (poextraqty > 0)
                        fixes.Add("Extra : " + poextraqty.ToString("F0"));

                    Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.BarcodeGeneration,
                        oLrinvoice.id, string.Format("{0} / {1} on {2} / {3} - {4} ({5})", supplier.name, oLrinvoice.invoiceno,
                        oLrinvoice.invoicedate.ToString("dd-MM-yyyy"),
                        oLrinvoice.totalamount.ToString("F0"),
                        oLrinvoice.lrentryno,
                        string.Join(",", fixes)),
                        Context.UserID, 0, transaction, Context);
                }
            }


            if (oLrinvoice.isdirectpurchase && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication
                      && oLrinvoice.statusid != 1)
            {
                AutoBarcodeCreation(conn, transaction, oLrinvoice);
            }

            return oLrinvoice;
        }
        
        public void AutoBarcodeCreation(NpgsqlConnection connection, NpgsqlTransaction transaction, Lrinvoice oLrinvoice)
        {
            AutoBarcodeCreation(connection, transaction, oLrinvoice, false);
        }
        public void AutoBarcodeCreation(NpgsqlConnection connection, NpgsqlTransaction transaction, Lrinvoice oLrinvoice, bool isQuantoImport)
        {

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var list = productsDataService.GetSellingModes(oLrinvoice.Lrinvoiceitems.ConvertAll(e => e.productid).Distinct().ToList());
            var barcodeitemlist = new List<Lrinvoiceitemsbarcode>();

            foreach (var item in oLrinvoice.Lrinvoiceitems)
            {
                item.lrid = oLrinvoice.lrid;
                item.lrinvoiceid = oLrinvoice.id;
                //v.newbarcode = v.buyingqty - ((v.actualqty > 0 ? (v.buyingqty - v.actualqty) : 0) + v.damagedqty + v.sellableqty);
                //v.allowgenerte = v.newbarcode > 0;
                var barcodeitem = new Lrinvoiceitemsbarcode()
                {
                    actualqty = item.actualqty,
                    buyingprice = item.buyingprice,
                    barcodes = item.barcodes,
                    buyingqty = item.buyingqty,
                    freeqty = item.freeqty,
                    companyid = oLrinvoice.companyid,
                    damagedqty = item.damagedqty,
                    Details = item.Details,
                    hsncode = item.hsncode,
                    id = item.id,
                    uan = string.IsNullOrEmpty(item.Details.gln) ? item.uan : item.Details.gln,
                    gln = String.IsNullOrEmpty(item.Details.serialno) ? item.Details.gln : item.Details.serialno,
                    itemdetail = item.itemdetail,
                    itemid = item.itemid,
                    lrid = oLrinvoice.lrid,
                    printingname = item.Details.printingname,
                    productid = item.productid,
                    returnqty = item.returnqty,
                    salediscount = item.salediscount,
                    sellableqty = item.sellableqty,
                    sellingprice = item.sellingprice,
                    stocklocationid = item.stocklocationid,
                    mrp = item.mrp,
                    isQuantoImport = isQuantoImport,
                    version = item.version,
                    newbarcode = (int)(item.buyingqty + item.freeqty - ((item.actualqty > 0 ? (item.buyingqty - item.actualqty) : 0) + item.damagedqty + item.sellableqty))
                };
                if (barcodeitem.newbarcode > 0 && list.ContainsKey(item.productid) && list[item.productid] != 1)
                {
                    if (item.sellableqty == 0)
                        barcodeitem.newbarcode = 1;
                }
                if (barcodeitem.newbarcode > 0)
                    barcodeitemlist.Add(barcodeitem);
            }
            if (barcodeitemlist.Count > 0)
            {
                StockDataService stockDataService = new StockDataService() { Context = this.Context };
                stockDataService.CreateBarcode(barcodeitemlist, transaction, connection);
                foreach (var item in barcodeitemlist)
                {
                    if (oLrinvoice.Lrinvoiceitems.Exists(e => e.id == item.id))
                    {
                        oLrinvoice.Lrinvoiceitems.Find(e => e.id == item.id).barcodeList = item.barcodeList;
                        oLrinvoice.Lrinvoiceitems.Find(e => e.id == item.id).barcodes = item.barcodes;
                        oLrinvoice.Lrinvoiceitems.Find(e => e.id == item.id).sellableqty = item.sellableqty;
                    }
                }

                //NpgsqlTransaction transaction = null;

                //using (NpgsqlConnection conn = CreateConnection())
                //{
                //    conn.Open();
                //    transaction = conn.BeginTransaction();
                InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid, Inbox.StatusList.Barcode_Generating,
                    transaction, connection, Context);
                //    transaction.Commit();
                //    conn.Close();
                //}

            }
        }
        public Lrinvoice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoice oLrinvoice)
        {

            if (string.IsNullOrEmpty(oLrinvoice.invoiceentryno))
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrinvoice.invoiceentryno",
                0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                oLrinvoice.companyid : 0);
                oLrinvoice.invoiceentryno = sequence.GetNextNumber(Context, conn, transaction);
            }

            if (!string.IsNullOrEmpty(oLrinvoice.invoiceno) && oLrinvoice.invoiceno.Length > 50)
                oLrinvoice.invoiceno = oLrinvoice.invoiceno.Substring(0, 50);

            if (oLrinvoice.suppliercompanyid <= 0)
                throw new Exception("Supplier company is missing");

            if (oLrinvoice.supplierid <= 0)
                throw new Exception("Supplier is missing");

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.entrylocationid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoice.invoiceno == null ? "" : oLrinvoice.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoice.invoicedate == null ? DateTime.MinValue : oLrinvoice.invoicedate;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoice.entrydate == DateTime.MinValue ? DateTimeNow : oLrinvoice.entrydate;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.noofpieces;
            command.Parameters.Add("noofbundle", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.noofbundle;
            command.Parameters.Add("baseamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.baseamount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.grossamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.taxamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.totalamount;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.lrid;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.otheramount;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.suppliercompanyid;
            command.Parameters.Add("lrsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.supplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("isdeliverychallan", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isdeliverychallan;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinterstatesale;
            command.Parameters.Add("iscreditnote", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.iscreditnote;

            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.discount;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.billvalue;
            command.Parameters.Add("invoiceentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoice.invoiceentryno == null ? "" : oLrinvoice.invoiceentryno;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoice.entrydate != DateTime.MinValue ? oLrinvoice.entrydate : DateTimeNow;
            command.Parameters.Add("isinclusivetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinclusivetax;
            command.Parameters.Add("isintercompanytransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isintercompanytransfer;
            command.Parameters.Add("isdirectpurchase", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isdirectpurchase;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.statusid;

            command.Parameters.Add("isrcm", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isrcm;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.hastds;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.tdsamount;
            command.Parameters.Add("tdsledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.tdsledgerid;

            command.Parameters.Add("isvendorbill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isvendorbill;
            command.Parameters.Add("buyerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.buyerinvoiceid;
            command.Parameters.Add("isgenerated", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isgenerated;
            command.Parameters.Add("isinclusiveall", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinclusiveall;
            command.Parameters.Add("isinclusivediscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinclusivediscount;
            command.Parameters.Add("hascommission", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.hascommission;
            command.Parameters.Add("expensepercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.expensepercentage;
            command.Parameters.Add("expenseperpiece", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.expenseperpiece;
            command.Parameters.Add("plessdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.plessdiscount;
            command.Parameters.Add("plessvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.plessvalue;

            command.Parameters.Add("potypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.ptype;
            command.Parameters.Add("poid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.purchaseorderid;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoice.pono) ? "" : oLrinvoice.pono;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoice.id = Convert.ToInt64(reader[0]);
                }
            }

            if (oLrinvoice.lrid > 0 && !IsMigration)
                InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                    Inbox.StatusList.Invoice_Preparation, transaction, conn, Context);
            return oLrinvoice;
        }

        public LrinvoiceCover Update(LrinvoiceCover oLrinvoice)
        {
            if (oLrinvoice.lrid <= 0)
                throw new Exception("Can't update invoice,because LRID missing in the parameter.");

            //if (oLrinvoice.lrexpense > 0)
            //{
            //    var totalbillvalue = oLrinvoice.Items.Sum(e => e.billvalue);
            //    oLrinvoice.Items.ForEach(e =>
            //    {
            //        var billpercentage = totalbillvalue / e.billvalue * 100;
            //        var billexpensevalue = oLrinvoice.lrexpense * billpercentage / 100;
            //        e.expensepercentage = oLrinvoice.agentcommission + Math.Ceiling(billexpensevalue / e.billvalue * 100 / (decimal)0.5) * (decimal)0.5;
            //    });
            //}

            LrinvoiceCreteria creteria = new LrinvoiceCreteria();
            creteria.lrid = oLrinvoice.lrid;
            List<Lrinvoice> deleteItems = new List<Lrinvoice>();
            var existingItems = Search(creteria);
            if (existingItems != null && existingItems.Count > 0 && oLrinvoice.Items != null)
            {
                deleteItems = existingItems.FindAll(e => !oLrinvoice.Items.Exists(f => f.id == e.id));
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
            {
                if (existingItems != null && existingItems.Count > 0 && existingItems[0].hasapprove)
                {
                    throw new Exception("Approved Invoice can't be updated, please remove the approval");
                }
            }
            if (deleteItems.Count > 0 && !this.Context.HasPermission(PermissionSet.lrinvoice, InfyPOS.Common.Action.Delete))
                throw new Exception("User doesn't have permission.");

            if (deleteItems.Count > 1)
            {
                throw new Exception("Can't delete more than one invoice at a time");
            }
            if (deleteItems.Count == 1)
            {
                LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                subLrinvoiceitems.Context = this.Context;
                if (subLrinvoiceitems.HasInvoiceItems(deleteItems[0].id))
                {
                    throw new Exception("Invoice which are having purchase details can't be deleted.");
                }
            }
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrinvoice.invoiceentryno", 0);
                LrinvoicetaxDataService subLrtaxitems = new LrinvoicetaxDataService() { Context = this.Context };
                LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                subLrinvoiceitems.Context = this.Context;


                subLrtaxitems.Context = this.Context;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (var item in deleteItems)
                        DeleteInvoice(item);
                    foreach (var item in oLrinvoice.Items.Where(e => e.id > 0))
                    {
                        item.lrid = oLrinvoice.lrid;
                        item.entrydate = item.entrydate == DateTime.MinValue ? oLrinvoice.entrydate : item.entrydate;
                        UpdateSupplier(transaction, conn, item);
                        Update(transaction, conn, item);
                        subLrtaxitems.Update(transaction, conn, item.Lrinvoicetaxitems, item.id);
                        InboxManager.Instance(Context.OrganizationID).Update(Context, item, transaction, conn);
                    }
                    foreach (var item in oLrinvoice.Items.Where(e => e.id == 0))
                    {
                        item.lrid = oLrinvoice.lrid;
                        item.invoiceentryno = sequence.GetNextNumber(Context, conn, transaction);
                        item.entrydate = item.entrydate == DateTime.MinValue ? oLrinvoice.entrydate : item.entrydate;
                        if (item.supplierid == 0)
                            item.supplierid = oLrinvoice.supplierid;
                        if (item.lrsupplierid == 0)
                            item.lrsupplierid = oLrinvoice.supplierid;
                        Insert(transaction, conn, item);
                        subLrtaxitems.Update(transaction, conn, item.Lrinvoicetaxitems, item.id);
                        if (item.Lrinvoiceitems != null && item.Lrinvoiceitems.Exists(x => x.id == 0))
                        {
                            subLrinvoiceitems.Update(transaction, conn, item.Lrinvoiceitems, item.id, item.statusid == 4);
                        }
                    }
                    InboxManager.Instance(Context.OrganizationID).Update(Context, oLrinvoice, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                oLrinvoice.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
            }
            return oLrinvoice;
        }

        public Lrinvoice Update(Lrinvoice oLrinvoice)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrinvoice);
                    LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                    subLrinvoiceitems.Context = this.Context;
                    subLrinvoiceitems.Update(transaction, conn, oLrinvoice.Lrinvoiceitems, oLrinvoice.id, oLrinvoice.statusid == 4);

                    oLrinvoice.Lrinvoicetaxitems.ForEach(e => e.isinterstatesale = oLrinvoice.isinterstatesale);

                    LrinvoicetaxDataService subLrtaxitems = new LrinvoicetaxDataService() { Context = this.Context };
                    subLrtaxitems.Context = this.Context;
                    subLrtaxitems.Update(transaction, conn, oLrinvoice.Lrinvoicetaxitems, oLrinvoice.id);

                    InboxManager.Instance(Context.OrganizationID).Update(Context, oLrinvoice, transaction, conn);

                    if (oLrinvoice.isdirectpurchase && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication
                        && oLrinvoice.statusid != 2)
                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                            Inbox.StatusList.Barcode_Generating, transaction, conn, Context);



                    if (oLrinvoice.isdirectpurchase
                        && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication
                        && oLrinvoice.statusid != 1)
                    {
                        AutoBarcodeCreation(conn, transaction, oLrinvoice);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
                throw exp;
            }

            return oLrinvoice;
        }

        public Lrinvoice Update(LrinvoiceDetail oLrinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
                {
                    if (oLrinvoice.hasapprove)
                    {
                        throw new Exception("Approved Invoice can't be updated, please remove the approval");
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oLrinvoice.isdirectpurchase)
                    {
                        LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                        var lrentry = lrentryDataService.Search(new LrentryCreteria()
                        {
                            id = oLrinvoice.lrid
                        }).First();
                        lrentry.invoiceno = oLrinvoice.invoiceno;
                        lrentry.invoicedate = oLrinvoice.invoicedate;
                        lrentry.supplierid = oLrinvoice.lrsupplierid;
                        lrentry.modifiedon = DateTimeNow;
                        lrentry.modifiedby = Context.UserID;
                        lrentry.lrdate = oLrinvoice.lrdate;
                        lrentry.lrcompanyid = oLrinvoice.companyid;
                        lrentry.transportid = oLrinvoice.transportid;
                        lrentry.noofbundels = oLrinvoice.bundles;
                        lrentry.entrylocationid = oLrinvoice.entrylocationid;
                        lrentry.retaillocationid = oLrinvoice.retaillocationid;
                        lrentry.lrno = oLrinvoice.lrno;
                        lrentryDataService.Update(transaction, conn, lrentry);
                    }

                    Update(transaction, conn, oLrinvoice);
                    LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                    subLrinvoiceitems.Context = this.Context;
                    subLrinvoiceitems.Update(transaction, conn, oLrinvoice.Lrinvoiceitems, oLrinvoice.id, oLrinvoice.statusid == 4);

                    oLrinvoice.Lrinvoicetaxitems.ForEach(e => e.isinterstatesale = oLrinvoice.isinterstatesale);

                    LrinvoicetaxDataService subLrtaxitems = new LrinvoicetaxDataService() { Context = this.Context };
                    subLrtaxitems.Context = this.Context;
                    subLrtaxitems.Update(transaction, conn, oLrinvoice.Lrinvoicetaxitems, oLrinvoice.id);

                    InboxManager.Instance(Context.OrganizationID).Update(Context, oLrinvoice, transaction, conn);

                    if (oLrinvoice.isdirectpurchase && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication
                        && oLrinvoice.statusid != 2)
                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                            Inbox.StatusList.Barcode_Generating, transaction, conn, Context);


                    if (oLrinvoice.isdirectpurchase
                        && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication
                        && oLrinvoice.statusid != 1)
                    {
                        AutoBarcodeCreation(conn, transaction, oLrinvoice);
                    }
                    transaction.Commit();
                    conn.Close();

                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
                throw exp;
            }

            return oLrinvoice;
        }

        public Lrinvoice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoice oLrinvoice)
        {
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pur_prt)
            {
                if (oLrinvoice.hasapprove)
                {
                    throw new Exception("Approved Invoice can't be updated, please remove the approval");
                }
            }
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.companyid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoice.invoiceno == null ? "" : oLrinvoice.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoice.invoicedate;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoice.entrydate;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.noofpieces;
            command.Parameters.Add("noofbundle", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.noofbundle;
            command.Parameters.Add("baseamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.baseamount;
            command.Parameters.Add("grossamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.grossamount;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.taxamount;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.totalamount;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.lrid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.id;
            command.Parameters.Add("otheramount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.otheramount;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.version;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.suppliercompanyid;
            command.Parameters.Add("lrsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.supplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.billvalue;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.discount;
            command.Parameters.Add("isdirectpurchase", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isdirectpurchase;
            command.Parameters.Add("isdeliverychallan", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isdeliverychallan;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.statusid;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinterstatesale;
            command.Parameters.Add("iscreditnote", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.iscreditnote;
            command.Parameters.Add("isinclusivetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinclusivetax;
            command.Parameters.Add("isinclusiveall", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinclusiveall;
            command.Parameters.Add("isinclusivediscount", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isinclusivediscount;
            command.Parameters.Add("hascommission", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.hascommission;
            command.Parameters.Add("invoiceentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoice.invoiceentryno == null ? "" : oLrinvoice.invoiceentryno;
            command.Parameters.Add("isvendorbill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isvendorbill;

            command.Parameters.Add("isrcm", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isrcm;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.hastds;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.tdsamount;
            command.Parameters.Add("tdsledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.tdsledgerid;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.tdspercentage;

            command.Parameters.Add("buyerinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.buyerinvoiceid;
            command.Parameters.Add("expensepercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.expensepercentage;
            command.Parameters.Add("expenseperpiece", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.expenseperpiece;
            command.Parameters.Add("plessdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.plessdiscount;
            command.Parameters.Add("plessvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoice.plessvalue;
            command.Parameters.Add("isgenerated", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoice.isgenerated;

            command.Parameters.Add("potypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.ptype;
            command.Parameters.Add("poid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.purchaseorderid;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrinvoice.pono) ? "" : oLrinvoice.pono;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoice.version = Convert.ToInt32(reader[0]);
                }
            }
            if (!IsMigration)
                InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(oLrinvoice.lrid,
                    Inbox.StatusList.Invoice_Preparation, transaction, conn, Context);
            return oLrinvoice;
        }

        public bool IsMigration { get; set; }
        public bool DeleteAll(Lrinvoice oLrinvoice)
        {
            bool successCount = false;

            if (oLrinvoice.id > 0)
            {
                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        Delete(transaction, conn, oLrinvoice);
                        LrinvoiceitemsDataService subLrinvoiceitems = new LrinvoiceitemsDataService() { ispricing = ispricing, Context = this.Context };
                        subLrinvoiceitems.Context = this.Context;
                        successCount = subLrinvoiceitems.DeleteAll(oLrinvoice, oLrinvoice.modifiedby, conn, transaction);

                        LrinvoicetaxDataService subLrinvoicetaxitems = new LrinvoicetaxDataService() { Context = this.Context };
                        subLrinvoicetaxitems.Context = this.Context;
                        successCount = subLrinvoicetaxitems.DeleteAll(oLrinvoice.id, oLrinvoice.modifiedby, conn, transaction);



                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoice);
                }
            }
            return successCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoice oLrinvoice)
        {
            decimal soldqty = 0;
            using (var reader = ExecuteReader(@"Select sum(oqty) from salablegoods 
            inner join stock on stock.id = stockid 
            inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            where lrinvoiceid=" + oLrinvoice.id, new SortedList<string, object>()))
            {
                if (reader.Read())
                    soldqty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
            }
            if (soldqty > 0)
                throw new Exception("Some of the items already sold");

            string candeletequery = string.Format(@"select count(*) from lrinvoiceitems
            where lrinvoiceid = {0} and barcodes is not null and barcodes->0 is not null", oLrinvoice.id);

            using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
            {
                if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                {
                    throw new Exception("Some of the invoice items already barcode generated,please remove those barcode before delete.");
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public bool UpdateSupplier(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoice oLrinvoice)
        {
            const string SQL_UpdateSupplier = @"update lrentry set supplierid=@supplierid where id=@lrid and @lrid>0;
                                                update stock set supplierid=@supplierid where invoiceitemid in (select id from lrinvoiceitems where lrinvoiceid=@lrinvoiceid);
                                                update salablegoods set supplierid=@supplierid where stockid in 
                                                (select id from stock where invoiceitemid in (select id from lrinvoiceitems where lrinvoiceid=@lrinvoiceid));";
            const string SQL_CheckSupplier = @"select suppliercompanyid from lrinvoice where id=@id";

            const string SQL_CheckSupplierPayment = @"select id from supplierpaymentinvoice where islrinvoicereturn = false and isactive = true and invoiceid=@invoiceid";

            long existingsupplier = 0;
            NpgsqlCommand command = GetCommand(SQL_CheckSupplier, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    existingsupplier = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                }
            }

            if (oLrinvoice.suppliercompanyid != existingsupplier && oLrinvoice.supplierid > 0)
            {
                long paymentid = 0;
                command = GetCommand(SQL_CheckSupplierPayment, conn, transaction);
                command.Parameters.Add("invoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paymentid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    }
                }
                if (paymentid > 0)
                    throw new Exception("Can't change supplier., Supplier payment already completed.");

                NpgsqlCommand command1 = GetCommand(SQL_UpdateSupplier, conn, transaction);
                command1.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.id;
                command1.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.lrid;
                command1.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoice.supplierid;
                int successCount = command1.ExecuteNonQuery();
            }
            return true;
        }

        public LrinvoicePrint Print(LrinvoiceCreteria item)
        {
            var returninvoice = Search(new LrinvoiceCreteria()
            {
                id = item.id
            });

            if (returninvoice == null || returninvoice.Count == 0) return new LrinvoicePrint();
            LrentryDataService lrentrydate = new LrentryDataService() { Context = this.Context };
            List<Lrentry> lrentry = lrentrydate.Search(new LrentryCreteria() { id = returninvoice[0].lrid });

            LrinvoicePrint ret = new LrinvoicePrint();
            ret.supplierid = returninvoice[0].suppliercompanyid;
            ret.suppliercompanyid = returninvoice[0].suppliercompanyid;
            ret.sourcesupplierid = lrentry[0].sourcesupplierid;
            ret.inwords = WebAPI.Data.Numberconvertor.rupees(ret.totalamount);
            ret.isinterstatesale = returninvoice[0].isinterstatesale;
            ret.iscreditnote = returninvoice[0].iscreditnote;
            ret.Lrinvoicetaxitems = returninvoice.First().Lrinvoicetaxitems;
            ret.Lrinvoiceitems = returninvoice.First().Lrinvoiceitems.OrderBy(e => e.sno).ThenBy(x => x.id).ToList();

            if (item.IsGRN)
            {
                ret.Lrinvoiceitems.RemoveAll(x => x.actualqty == 0);
                ret.Lrinvoiceitems.ForEach(x =>
                {
                    if (x.actualqty != x.buyingqty)
                    {
                        x.amount = x.amount / x.buyingqty * x.actualqty;
                        x.purchasetax = x.purchasetax / x.buyingqty * x.actualqty;
                        x.purchasetotaldiscount = x.purchasetotaldiscount / x.buyingqty * x.actualqty;
                    }
                });
                ret.Lrinvoicetaxitems.RemoveAll(x => x.type == Lrinvoicetax.EntryType.Taxable);
                foreach (var taxitem in ret.Lrinvoiceitems.GroupBy(e => new { e.taxid, e.purchasetaxpercentage }))
                {
                    var amount = taxitem.Sum(e => e.amount);
                    var tax = taxitem.Sum(e => e.purchasetax);
                    ret.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                    {
                        taxamount = tax,
                        taxid = taxitem.Key.taxid,
                        percentage = taxitem.Key.purchasetaxpercentage,
                        isinterstatesale = ret.isinterstatesale,
                        amounton = amount,
                        netamount = amount + tax,
                        isactive = true,
                        type = Lrinvoicetax.EntryType.Taxable
                    });
                }
            }

            ret.totalamount = returninvoice[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Taxable) ? e.amounton : 0);
            ret.discount = returninvoice[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Taxable ||
            e.type == Lrinvoicetax.EntryType.Discount ||
            e.type == Lrinvoicetax.EntryType.DiscuntOnDiscount) ? e.discount : 0);
            ret.taxamount = returninvoice[0].Lrinvoicetaxitems.Sum(e => e.taxamount);

            ret.additionalamount = returninvoice[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Adjustment ||
            e.type == Lrinvoicetax.EntryType.Courier || e.type == Lrinvoicetax.EntryType.Fright ||
            e.type == Lrinvoicetax.EntryType.Insurance || e.type == Lrinvoicetax.EntryType.Packing ||
            e.type == Lrinvoicetax.EntryType.ServiceCharge) ? e.amounton : 0);
            ret.additionaltax = returninvoice[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Adjustment ||
            e.type == Lrinvoicetax.EntryType.Courier || e.type == Lrinvoicetax.EntryType.Fright ||
            e.type == Lrinvoicetax.EntryType.Insurance || e.type == Lrinvoicetax.EntryType.Packing ||
            e.type == Lrinvoicetax.EntryType.ServiceCharge) ? e.taxamount : 0);

            ret.baseamount = returninvoice[0].Lrinvoiceitems.Sum(x => x.amount + x.purchasediscount);
            ret.discount = returninvoice[0].Lrinvoiceitems.Sum(x => x.purchasediscount);
            ret.billvalue = returninvoice[0].Lrinvoiceitems.Sum(x => x.amount + x.purchasetax);
            ret.buyingcompany = returninvoice[0].buyingcompany;
            ret.pono = returninvoice[0].pono;
            ret.podate = returninvoice[0].podate;
            ret.entrydate = returninvoice[0].entrydate;
            ret.grossamount = returninvoice[0].Lrinvoiceitems.Sum(x => x.amount + x.purchasediscount);
            ret.invoicedate = returninvoice[0].invoicedate;
            ret.invoiceentryno = returninvoice[0].invoiceentryno;
            ret.invoiceno = returninvoice[0].invoiceno;
            ret.createdby = returninvoice[0].createdby;
            ret.createdon = returninvoice[0].createdon;
            ret.lrentrydetails = lrentry;
            ret.noofbundle = returninvoice[0].noofbundle;
            ret.companyid = returninvoice[0].companyid;
            ret.noofpieces = returninvoice[0].noofpieces;
            ret.otheramount = returninvoice[0].otheramount;
            ret.suppliercompany = returninvoice[0].suppliercompany;
            ret.taxamount = returninvoice[0].Lrinvoiceitems.Sum(x => x.purchasetax);
            ret.totalamount = returninvoice[0].Lrinvoiceitems.Sum(x => x.amount + x.purchasetax);
            ret.taxableamount = returninvoice[0].Lrinvoiceitems.Sum(x => x.amount);
            ret.lrentryinfo = lrentry[0].lrentryno + "/" + returninvoice[0].entrydate.ToString("dd/MM/yyyy");
            ret.invoiceinfo = returninvoice[0].invoiceno + "/" + returninvoice[0].invoicedate.ToString("dd/MM/yyyy");
            ret.lrinfo = lrentry[0].lrno + "/" + lrentry[0].lrdate.ToString("dd/MM/yyyy");
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = returninvoice[0].companyid
            });
            ret.company = comnanylist[0];
            ret.company.parseaddress();
            if (ret.company.cityid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var city = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = ret.company.cityid
                }).First();
                ret.company.cityname = city.name;
            }
            if (ret.company.stateid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var state = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = ret.company.stateid
                }).First();
                ret.company.statename = state.name;
            }


            if (lrentry[0].agentid > 0)
            {
                Data.AgentDataService agentdate = new AgentDataService() { Context = this.Context };
                agentdate.Context = this.Context;
                var agentlist = agentdate.Search(new AgentCreteria()
                {
                    id = lrentry[0].agentid
                });
                ret.agent = agentlist[0];
            }


            if (ret.Lrinvoiceitems != null)
            {
                ret.HSNList = new List<LrinvoicePrint.HSNItem>();
                foreach (var grp in ret.Lrinvoiceitems.GroupBy(e => new { e.hsncode, e.purchasetaxpercentage }))
                {
                    ret.HSNList.Add(new LrinvoicePrint.HSNItem()
                    {
                        code = grp.Key.hsncode,
                        isinterstate = ret.isinterstatesale,
                        taxable = grp.Sum(e => e.buyingqty * e.buyingprice),
                        tax = grp.Sum(e => e.purchasetax),
                        total = grp.Sum(e => e.amount + e.purchasetax),
                        qty = grp.Sum(e => e.buyingqty),
                        peice = grp.Sum(e => e.buyingqty),
                        cgst_percentage = (ret.isinterstatesale || grp.Key.purchasetaxpercentage == 0) ? 0 : grp.Key.purchasetaxpercentage / 2,
                        igst_percentage = (!ret.isinterstatesale || grp.Key.purchasetaxpercentage == 0) ? 0 : grp.Key.purchasetaxpercentage,
                        sgst_percentage = (ret.isinterstatesale || grp.Key.purchasetaxpercentage == 0) ? 0 : grp.Key.purchasetaxpercentage / 2,
                        sgst_value = ret.isinterstatesale ? 0 : grp.Sum(e => e.purchasetax / 2),
                        cgst_value = ret.isinterstatesale ? 0 : grp.Sum(e => e.purchasetax / 2),
                        igst_value = !ret.isinterstatesale ? 0 : grp.Sum(e => e.purchasetax)
                    });
                }
            }

            Data.SuppliercompanyDataService scompanyservice = new SuppliercompanyDataService() { Context = this.Context };
            var suppliercompany = scompanyservice.Search(new SuppliercompanyCreteria()
            {
                id = ret.suppliercompanyid
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
                if (ret.supplier.stateid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var state = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = ret.supplier.stateid
                    }).First();
                    ret.supplier.statename = state.name;
                    ret.supplier.statecode = state.code;
                }
            }

            if (ret.sourcesupplierid > 0 && ret.sourcesupplierid != ret.supplierid)
            {
                SupplierDataService suppdata = new SupplierDataService();
                List<Supplier> supplierlist = suppdata.Search(new SupplierCreteria()
                {
                    id = ret.sourcesupplierid
                });
                if (supplierlist.Count > 0)
                    ret.sourcesuppliername = supplierlist[0].name;
            }

            List<long> userids = new List<long>();
            userids.Add(ret.createdby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
            }

            string debitquery = string.Format(@"select sum(amount) amount,sum(qty) qty from lrinvoicereturnitems where lrinvoiceitemsid in ({0}); 
                            select returnno,amount from lrinvoicereturn where lrinvoiceid=@lrinvoiceid",
                            string.Join(",", ret.Lrinvoiceitems.ConvertAll(x => x.id)));

            var param = new SortedList<string, object>();
            param.Add("lrinvoiceid", item.id);
            using (var reader = ExecuteReader(debitquery, param))
            {
                if (reader.Read())
                {
                    ret.debitnoteqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    ret.debitnotevalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                }
                if (reader.NextResult())
                {
                    if (reader.Read())
                    {
                        ret.debitnoteno = reader["returnno"] == DBNull.Value ? "" : Convert.ToString(reader["returnno"]);
                        ret.debitnotevalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    }
                }
            }

            if (returninvoice[0].retaillocationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = returninvoice[0].retaillocationid
                }).First();

                ret.location = new CompanyLocation(location);
                ret.location.name = location.name;
                ret.location.phonenumber = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.contactno;
                ret.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                ret.location.pincode = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.pincode;
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

                if (ret.supplier.cityid > 0)
                {
                    referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var city = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = ret.location.cityid
                    }).First();
                    ret.location.cityname = city.name;
                }
                if (ret.supplier.stateid > 0)
                {
                    referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var state = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = ret.location.stateid
                    }).First();
                    ret.location.statename = state.name;
                    ret.location.statecode = state.code;
                }
            }
            return ret;
        }

        internal bool ChangeStatus(LrinvoiceCreteria item)
        {
            var successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(
                        string.Format("Update Lrinvoice set approvedby=@approvedby,approvedon=@approvedon,hasapprove=@hasapprove where id in ({0})", item.ids),
                        conn);
                    command.Parameters.Add("hasapprove", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.hasapprove;
                    command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return successCount > 0;
        }

        internal bool SetQCApproveval(LrInvoiceQC item)
        {
            var successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(
                        string.Format("Update Lrinvoice set qcby=@qcby,qcon=@qcon,hasqc=@hasqc where id={0}", item.id),
                        conn);
                    command.Parameters.Add("hasqc", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.qcby>0;
                    command.Parameters.Add("qcby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.qcby;
                    command.Parameters.Add("qcon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.qcon;
                    successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return successCount > 0;
        }

        internal LrInvoiceQC GetQCApproveval(string id)
        {
            LrInvoiceQC result = new LrInvoiceQC() { id = long.Parse(id),qcon =DateTimeNow.Date };
            var param = new SortedList<string, object>();
            param.Add("id", result.id);
            using (var reader = ExecuteReader("select hasqc,qcby,qcon from lrinvoice where id=@id", param))
            {
                if(reader.Read())
                {
                    if (reader["hasqc"] != DBNull.Value && Convert.ToBoolean(reader["hasqc"]))
                    {
                        result.qcby = reader["qcby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["qcby"]);
                        result.qcon = reader["qcon"] == DBNull.Value ? DateTime.Now.Date : Convert.ToDateTime(reader["qcon"]);
                    }
                }
            }
            return result;
        }
    }

}
