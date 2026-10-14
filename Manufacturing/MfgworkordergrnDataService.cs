using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Threading.Tasks;
using System.Data.Common;
using Newtonsoft.Json;
using System.Text;


namespace InfyPOS.Data
{
    public class MfgworkordergrnDataService : SAASData
    {
        const string SQL_Search = @"select id, entryno, workorderno, locationid, entrydate, invoicedate, lrdate,lrentryid,lrentryno, invoiceno, lrno, isactive, createdby, modifiedby, version, createdon, modifiedon, tax, taxable, total, statusid, supplierid, suppliercompanyid, totalqty, receivedby, companyid, approvedby, approvedon, mode,workorderid,joborderid,product,design,sizelist,workorderdate,receivinglocationid from mfgworkordergrn where id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfgworkordergrn(entryno, workorderno, locationid, entrydate, invoicedate, lrdate,lrentryid,lrentryno, invoiceno, lrno, isactive, createdby, modifiedby, version, createdon, modifiedon, tax, taxable, total, statusid, supplierid, suppliercompanyid, totalqty, receivedby, companyid, approvedby, approvedon, mode,workorderid,joborderid,product,design,sizelist,workorderdate,receivinglocationid) 
        VALUES(@entryno, @workorderno, @locationid, @entrydate, @invoicedate, @lrdate,@lrentryid,@lrentryno, @invoiceno, @lrno, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @tax, @taxable, @total, @statusid, @supplierid, @suppliercompanyid, @totalqty, @receivedby, @companyid, @approvedby, @approvedon, @mode,@workorderid,@joborderid,@product,@design,@sizelist,@workorderdate,@receivinglocationid) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgworkordergrn SET entryno=@entryno, workorderno=@workorderno, locationid=@locationid, entrydate=@entrydate, invoicedate=@invoicedate, 
        lrdate=@lrdate, lrentryid=@lrentryid,lrentryno=@lrentryno , invoiceno=@invoiceno, lrno=@lrno, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, tax=@tax, taxable=@taxable, 
        total=@total, statusid=@statusid, supplierid=@supplierid, suppliercompanyid=@suppliercompanyid, totalqty=@totalqty, receivedby=@receivedby, companyid=@companyid, approvedby=@approvedby, approvedon=@approvedon, mode=@mode,workorderid=@workorderid,joborderid=@joborderid,
        product=@product,design=@design,sizelist=@sizelist,receivinglocationid=@receivinglocationid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkordergrn set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgworkordergrn> Search(MfgworkordergrnCreteria _mfgworkordergrn)
        {
            List<Mfgworkordergrn> lst = new List<Mfgworkordergrn>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkordergrn.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkordergrn.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_mfgworkordergrn.entryno))
                        qb.AddParameter("entryno", _mfgworkordergrn.entryno, "=");

                    if (_mfgworkordergrn.supplierid > 0)
                        qb.AddParameter("supplierid", _mfgworkordergrn.supplierid, "=");

                    if (_mfgworkordergrn.locationid > 0)
                        qb.AddParameter("locationid", _mfgworkordergrn.locationid, "=");

                    if (_mfgworkordergrn.receivinglocationid > 0)
                        qb.AddParameter("receivinglocationid", _mfgworkordergrn.receivinglocationid, "=");

                    if (_mfgworkordergrn.suppliercompanyid > 0)
                        qb.AddParameter("suppliercompanyid", _mfgworkordergrn.suppliercompanyid, "=");

                    if (!string.IsNullOrEmpty(_mfgworkordergrn.workorderno))
                        qb.AddParameter("workorderno", _mfgworkordergrn.workorderno, "=");

                    if (_mfgworkordergrn.workorderid > 0)
                        qb.AddParameter("workorderid", _mfgworkordergrn.workorderid, "=");

                    if (_mfgworkordergrn.entryfromdate.Year > 2000)
                        qb.AddParameter("entrydate", "fromdate", _mfgworkordergrn.entryfromdate, ">=");

                    if (_mfgworkordergrn.entrytodate.Year > 2000)
                        qb.AddParameter("entrydate", "todate", _mfgworkordergrn.entrytodate.AddDays(1), "<");

                    if (_mfgworkordergrn.fromdate.Year > 2000)
                        qb.AddParameter("invoicedate", "invoicedatefromdate", _mfgworkordergrn.fromdate, ">=");

                    if (_mfgworkordergrn.todate.Year > 2000)
                        qb.AddParameter("invoicedate", "invoicedatetodate", _mfgworkordergrn.todate.AddDays(1), "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkordergrn omfgworkordergrn = new Mfgworkordergrn();
                        omfgworkordergrn.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkordergrn.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        omfgworkordergrn.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        omfgworkordergrn.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgworkordergrn.workorderid = reader["workorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workorderid"]);
                        omfgworkordergrn.joborderid = reader["joborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["joborderid"]);
                        omfgworkordergrn.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        omfgworkordergrn.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        omfgworkordergrn.workorderdate = reader["workorderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["workorderdate"]);
                        omfgworkordergrn.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);

                        omfgworkordergrn.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        omfgworkordergrn.lrentryid = reader["lrentryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrentryid"]);

                        omfgworkordergrn.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        omfgworkordergrn.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        omfgworkordergrn.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkordergrn.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkordergrn.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkordergrn.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkordergrn.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkordergrn.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkordergrn.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgworkordergrn.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        omfgworkordergrn.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        omfgworkordergrn.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                        omfgworkordergrn.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgworkordergrn.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        omfgworkordergrn.totalqty = reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);
                        omfgworkordergrn.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        omfgworkordergrn.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfgworkordergrn.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        omfgworkordergrn.approvedon = reader["approvedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["approvedon"]);
                        omfgworkordergrn.mode = reader["mode"] == DBNull.Value ? "" : reader["mode"].ToString();

                        omfgworkordergrn.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        omfgworkordergrn.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        omfgworkordergrn.sizelist_json = reader["sizelist"] == DBNull.Value ? null : reader["sizelist"].ToString();
                        omfgworkordergrn.receivinglocationid = reader["receivinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivinglocationid"]);

                        lst.Add(omfgworkordergrn);
                    }
                }
                if ((_mfgworkordergrn.id > 0 || _mfgworkordergrn.workorderid > 0) && lst.Count == 1)
                {
                    MfgworkordergrnitemsDataService subMfgworkordergrnitems = new MfgworkordergrnitemsDataService() { Context = this.Context };
                    lst[0].Mfgworkordergrnitems = subMfgworkordergrnitems.SearchAll(lst.First().id);
                    MfgworkordergrnitemsizesDataService mfgworkordergrnitemsizesDataService = new MfgworkordergrnitemsizesDataService() { Context = this.Context };
                    var sizelist = mfgworkordergrnitemsizesDataService.SearchAll(lst.First().id);
                    lst[0].Mfgworkordergrnitems.ForEach(e =>
                    {
                        e.sizelist = sizelist.FindAll(x => x.mfgworkorderitemsgrnid == e.id);
                    });
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkordergrn);
            }
            return lst;
        }

        public MfgworkordergrnDetail Print(MfgworkordergrnCreteria _mfgworkordergrn)
        {
            var grn = Search(_mfgworkordergrn).First();
            var result = JsonConvert.DeserializeObject<MfgworkordergrnDetail>(JsonConvert.SerializeObject(grn));
            if (result.supplierid > 0)
            {
                SupplierDataService suppdata = new SupplierDataService() { Context = this.Context };
                var supplierlist = suppdata.Search(new SupplierCreteria()
                {
                    id = result.supplierid
                });
                if (supplierlist.Count > 0)
                {
                    result.supplier = supplierlist[0];
                }
                SuppliercompanyDataService sDataService = new SuppliercompanyDataService() { Context = this.Context };
                List<Suppliercompany> suppliercompany = sDataService.Search(new SuppliercompanyCreteria()
                { supplierid = result.supplierid });
                if (suppliercompany.Count == 1)
                    result.suppliercompany = suppliercompany[0];
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.SearchList(new ProductsCreteria()
            {
                ids = result.Mfgworkordergrnitems.ConvertAll(x => x.productid)
            });

            productlist.ForEach(x =>
            {
                var items = result.Mfgworkordergrnitems.FindAll(y => y.productid == x.id).ToList();
                items.ForEach(y =>
                {
                    y.productname = x.name;
                });
            });

            if (result.companyid > 0)
            {
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                List<Company> buyingcompany = companyDataService.Search(new CompanyCreteria()
                {
                    id = result.companyid
                }
                );
                if (buyingcompany.Count == 1)
                {
                    result.company = buyingcompany[0];
                }
            }
            return result;
        }
        public Lrinvoice LinkRetailStock(NpgsqlTransaction transaction,
        NpgsqlConnection conn, Mfgworkordergrn oMfgworkordergrn, bool validateexisting)
        {

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = oMfgworkordergrn.Mfgworkordergrnitems.ConvertAll(e => e.productid).Distinct().ToList()
            });

            long lrentryid = 0;
            if (validateexisting)
            {
                var param = new SortedList<string, object>();
                param.Add("lrentryno", string.IsNullOrEmpty(oMfgworkordergrn.lrentryno) ? "" : oMfgworkordergrn.lrentryno);
                using (var reader = ExecuteReader("select id from lrentry where isactive = true and lrentryno=@lrentryno", param))
                {
                    if (reader.Read())
                        lrentryid = reader.GetInt64(0);
                }
            }

            Lrinvoice lrinvoice = null;
            if (lrentryid != 0)
            {
                LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                var lrentry = lrentryDataService.Search(new LrentryCreteria()
                {
                    id = lrentryid
                }).First();
                LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                lrinvoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
                {
                    lrid = lrentry.id,
                    showitems = true
                }).First();
                lrinvoiceDataService.Delete(transaction, conn, lrinvoice);

                if (oMfgworkordergrn.locationid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = oMfgworkordergrn.locationid
                    });
                    if (locationinfo.Count > 0 && !locationinfo[0].additionalinfo.iswarehouse)
                        lrentry.retaillocationid = oMfgworkordergrn.locationid;
                }
                lrentry.locationid = oMfgworkordergrn.locationid;
                lrentry.lrno = oMfgworkordergrn.lrno;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;
                lrentry.lrcompanyid = oMfgworkordergrn.companyid;
                lrentry.lrdate = oMfgworkordergrn.lrdate;
                lrentry.lrentrydate = oMfgworkordergrn.entrydate;
                lrentry.invoicedate = oMfgworkordergrn.invoicedate;
                lrentry.invoiceno = oMfgworkordergrn.invoiceno;
                lrentry.modeofdelivery = 2;
                lrentry.supplierid = oMfgworkordergrn.supplierid;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;

                if (lrentry.retaillocationid == 0)
                    lrentry.retaillocationid = oMfgworkordergrn.receivinglocationid;

                lrentry.supplierid = oMfgworkordergrn.supplierid;
                lrentryDataService.Update(transaction, conn, lrentry);

                lrinvoice.id = 0;
                lrinvoice.lrid = lrentry.id;
                //lrinvoice.isimported = true;
                lrinvoice.baseamount = oMfgworkordergrn.taxable;
                lrinvoice.billvalue = oMfgworkordergrn.total;
                lrinvoice.companyid = oMfgworkordergrn.companyid;
                lrinvoice.lrsupplierid = oMfgworkordergrn.supplierid;
                lrinvoice.suppliercompanyid = oMfgworkordergrn.suppliercompanyid;
                lrinvoice.entrydate = oMfgworkordergrn.entrydate;
                lrinvoice.grossamount = oMfgworkordergrn.total;
                lrinvoice.invoicedate = oMfgworkordergrn.invoicedate;
                lrinvoice.invoiceno = oMfgworkordergrn.invoiceno;
                lrinvoice.isinclusiveall = true;
                lrinvoice.isinterstatesale = oMfgworkordergrn.isinterstatesale;
                lrinvoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                lrinvoice.Lrinvoiceitems = new List<LrinvoiceItemDetail>();

                foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems.GroupBy(e => new { e.taxpercentage, e.taxid }))
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                    {
                        taxamount = item.Sum(e => e.taxamount),
                        taxid = item.Key.taxid,
                        percentage = item.Key.taxpercentage,
                        amounton = item.Sum(e => e.amount - e.taxamount),
                        isinterstatesale = lrinvoice.isinterstatesale,
                        type = Lrinvoicetax.EntryType.Taxable,
                        isactive = true,
                        version = 1
                    });
                }

                foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems)
                {
                    foreach (var sitem in item.sizelist)
                    {
                        if (sitem.qty > 0)
                        {
                            lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                            {
                                basebuyingprice = sitem.cost,
                                buyingprice = sitem.cost,
                                amount = item.cost * sitem.qty,
                                buyingqty = sitem.qty,
                                designid = item.designname,
                                itemid = sitem.itemid,
                                mrp = sitem.salerate,
                                productid = item.productid,
                                ptaxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                hsncode = productlist.Find(e => e.id == item.productid).hsncode,
                                supplierid = oMfgworkordergrn.supplierid,
                                taxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                Details = new Items()
                                {
                                    grnitemid = item.id,
                                    grnsizeid = sitem.id,
                                    sizegroup = item.size,
                                    productid = item.productid,
                                    sleeveid = sitem.sleeveid,
                                    colourid = sitem.colourid,
                                    sizeid = sitem.sizeid,
                                    gln = sitem.uid,
                                    serialno = sitem.uid,
                                    support_gln = true,
                                    productname = productlist.Find(e => e.id == item.productid).name,
                                    sellingname = productlist.Find(e => e.id == item.productid).name
                                },
                                purchasetax = 0,
                                purchasetaxpercentage = 0,
                                sellingprice = sitem.salerate,
                                wsrate = sitem.wsrate,
                                dealerprice = sitem.dealerprice,
                                sno = lrinvoice.Lrinvoiceitems.Count + 1,
                                stocklocationid = lrentry.retaillocationid,
                                version = 1,
                                isactive = true
                            });
                        }
                        if (sitem.sqty > 0)
                        {
                            lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                            {

                                basebuyingprice = sitem.cost,
                                buyingprice = sitem.cost,
                                amount = item.cost * sitem.sqty,
                                buyingqty = sitem.sqty,
                                isseconds = true,
                                designid = item.designname,
                                itemid = sitem.itemid,
                                mrp = sitem.salerate,
                                productid = item.productid,
                                ptaxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                hsncode = productlist.Find(e => e.id == item.productid).hsncode,
                                supplierid = oMfgworkordergrn.supplierid,
                                taxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                Details = new Items()
                                {
                                    grnitemid = item.id,
                                    grnsizeid = sitem.id,
                                    sizegroup = item.size,
                                    productid = item.productid,
                                    sleeveid = sitem.sleeveid,
                                    colourid = sitem.colourid,
                                    sizeid = sitem.sizeid,
                                    gln = sitem.uid,
                                    serialno = sitem.uid,
                                    support_gln = true,
                                    productname = productlist.Find(e => e.id == item.productid).name,
                                    sellingname = productlist.Find(e => e.id == item.productid).name
                                },
                                purchasetax = 0,
                                purchasetaxpercentage = 0,
                                sellingprice = sitem.salerate,
                                wsrate = sitem.wsrate,
                                dealerprice = sitem.dealerprice,
                                stocklocationid = lrentry.retaillocationid,
                                sno = lrinvoice.Lrinvoiceitems.Count + 1,
                                version = 1,
                                isactive = true
                            });
                        }
                    }
                }
                lrinvoiceDataService.ImportInvoiceItems(lrinvoice, transaction, conn);
            }
            else
            {
                throw new Exception("Please create lr/invoice before saving grn for finished goods");
            }
            return lrinvoice;
        }

        public Lrinvoice UpdateRetailStock(NpgsqlTransaction transaction,
            NpgsqlConnection conn, Mfgworkordergrn oMfgworkordergrn, bool validateexisting)
        {

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var productlist = productsDataService.Search(new ProductsCreteria()
            {
                ids = oMfgworkordergrn.Mfgworkordergrnitems.ConvertAll(e => e.productid).Distinct().ToList()
            });

            long lrentryid = 0;
            if (validateexisting)
            {
                using (var reader = ExecuteReader("select id from lrentry where isactive = true and importreferencetype=1 and importreferenceid=" + oMfgworkordergrn.id, null))
                {
                    if (reader.Read())
                        lrentryid = reader.GetInt64(0);
                }
            }

            Lrinvoice lrinvoice = null;
            if (lrentryid != 0)
            {
                LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                var lrentry = lrentryDataService.Search(new LrentryCreteria()
                {
                    id = lrentryid
                }).First();
                LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                lrinvoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
                {
                    lrid = lrentry.id,
                    showitems = true
                }).First();
                lrinvoiceDataService.Delete(transaction, conn, lrinvoice);

                if (oMfgworkordergrn.locationid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = oMfgworkordergrn.locationid
                    });
                    if (locationinfo.Count > 0 && !locationinfo[0].additionalinfo.iswarehouse)
                        lrentry.retaillocationid = oMfgworkordergrn.locationid;
                }
                lrentry.locationid = oMfgworkordergrn.locationid;
                lrentry.lrno = oMfgworkordergrn.lrno;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;
                lrentry.lrcompanyid = oMfgworkordergrn.companyid;
                lrentry.supplierid = oMfgworkordergrn.supplierid;
                lrentry.lrdate = oMfgworkordergrn.lrdate;
                lrentry.lrentrydate = oMfgworkordergrn.entrydate;
                lrentry.invoicedate = oMfgworkordergrn.invoicedate;
                lrentry.invoiceno = oMfgworkordergrn.invoiceno;
                lrentry.modeofdelivery = 2;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;

                if (lrentry.retaillocationid == 0)
                    lrentry.retaillocationid = oMfgworkordergrn.receivinglocationid;

                lrentryDataService.Update(transaction, conn, lrentry);

                lrinvoice.id = 0;
                lrinvoice.lrid = lrentry.id;
                //lrinvoice.isimported = true;
                lrinvoice.baseamount = oMfgworkordergrn.taxable;
                lrinvoice.billvalue = oMfgworkordergrn.total;
                lrinvoice.companyid = oMfgworkordergrn.companyid;
                lrinvoice.lrsupplierid = oMfgworkordergrn.supplierid;
                lrinvoice.suppliercompanyid = oMfgworkordergrn.suppliercompanyid;
                lrinvoice.entrydate = oMfgworkordergrn.entrydate;
                lrinvoice.grossamount = oMfgworkordergrn.total;
                lrinvoice.invoicedate = oMfgworkordergrn.invoicedate;
                lrinvoice.invoiceno = oMfgworkordergrn.invoiceno;
                lrinvoice.isinclusiveall = true;
                lrinvoice.isinterstatesale = oMfgworkordergrn.isinterstatesale;
                lrinvoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                lrinvoice.Lrinvoiceitems = new List<LrinvoiceItemDetail>();

                foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems.GroupBy(e => new { e.taxpercentage, e.taxid }))
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                    {
                        taxamount = item.Sum(e => e.taxamount),
                        taxid = item.Key.taxid,
                        percentage = item.Key.taxpercentage,
                        amounton = item.Sum(e => e.amount - e.taxamount),
                        isinterstatesale = lrinvoice.isinterstatesale,
                        type = Lrinvoicetax.EntryType.Taxable,
                        isactive = true,
                        version = 1
                    });
                }

                foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems)
                {
                    foreach (var sitem in item.sizelist)
                    {
                        if (sitem.qty > 0)
                        {
                            lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                            {
                                basebuyingprice = sitem.cost,
                                buyingprice = sitem.cost,
                                amount = item.cost * sitem.qty,
                                buyingqty = sitem.qty,
                                designid = item.designname,
                                itemid = sitem.itemid,
                                mrp = sitem.salerate,
                                productid = item.productid,
                                ptaxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                hsncode = productlist.Find(e => e.id == item.productid).hsncode,
                                supplierid = oMfgworkordergrn.supplierid,
                                taxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                Details = new Items()
                                {
                                    grnitemid = item.id,
                                    grnsizeid = sitem.id,
                                    sizegroup = item.size,
                                    productid = item.productid,
                                    sleeveid = sitem.sleeveid,
                                    colourid = sitem.colourid,
                                    sizeid = sitem.sizeid,
                                    gln = sitem.uid,
                                    serialno = sitem.uid,
                                    support_gln = true,
                                    productname = productlist.Find(e => e.id == item.productid).name,
                                    sellingname = productlist.Find(e => e.id == item.productid).name
                                },
                                purchasetax = 0,
                                purchasetaxpercentage = 0,
                                sellingprice = sitem.salerate,
                                wsrate = sitem.wsrate,
                                dealerprice = sitem.dealerprice,
                                sno = lrinvoice.Lrinvoiceitems.Count + 1,
                                stocklocationid = lrentry.retaillocationid,
                                version = 1,
                                isactive = true
                            });
                        }
                        if (sitem.sqty > 0)
                        {
                            lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                            {

                                basebuyingprice = sitem.cost,
                                buyingprice = sitem.cost,
                                amount = item.cost * sitem.sqty,
                                buyingqty = sitem.sqty,
                                isseconds = true,
                                designid = item.designname,
                                itemid = sitem.itemid,
                                mrp = sitem.salerate,
                                productid = item.productid,
                                ptaxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                hsncode = productlist.Find(e => e.id == item.productid).hsncode,
                                supplierid = oMfgworkordergrn.supplierid,
                                taxid = productlist.Find(e => e.id == item.productid).salestaxid,
                                Details = new Items()
                                {
                                    grnitemid = item.id,
                                    grnsizeid = sitem.id,
                                    sizegroup = item.size,
                                    productid = item.productid,
                                    sleeveid = sitem.sleeveid,
                                    colourid = sitem.colourid,
                                    sizeid = sitem.sizeid,
                                    gln = sitem.uid,
                                    serialno = sitem.uid,
                                    support_gln = true,
                                    productname = productlist.Find(e => e.id == item.productid).name,
                                    sellingname = productlist.Find(e => e.id == item.productid).name
                                },
                                purchasetax = 0,
                                purchasetaxpercentage = 0,
                                sellingprice = sitem.salerate,
                                wsrate = sitem.wsrate,
                                dealerprice = sitem.dealerprice,
                                stocklocationid = lrentry.retaillocationid,
                                sno = lrinvoice.Lrinvoiceitems.Count + 1,
                                version = 1,
                                isactive = true
                            });
                        }
                    }
                }
                lrinvoiceDataService.ImportInvoice(lrinvoice, transaction, conn);
            }
            else
            {

                Lrentry lrentry = new Lrentry();
                lrentry.lrno = oMfgworkordergrn.lrno;
                lrentry.entrylocationid = oMfgworkordergrn.locationid;
                lrentry.lrcompanyid = oMfgworkordergrn.companyid;
                lrentry.lrdate = oMfgworkordergrn.lrdate;
                lrentry.lrentrydate = oMfgworkordergrn.entrydate;
                lrentry.modeofdelivery = 2;
                lrentry.retaillocationid = oMfgworkordergrn.receivinglocationid;
                lrentry.supplierid = oMfgworkordergrn.supplierid;
                if (oMfgworkordergrn.locationid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = oMfgworkordergrn.locationid
                    });
                    if (locationinfo.Count > 0 && (locationinfo[0].additionalinfo == null || !locationinfo[0].additionalinfo.iswarehouse))
                        lrentry.retaillocationid = oMfgworkordergrn.receivinglocationid;
                }
                lrentry.locationid = oMfgworkordergrn.locationid;

                LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                lrentryDataService.Insert(transaction, conn, lrentry);
                ExecuteQuery("update lrentry set importreferencetype=1,importreferenceid=" + oMfgworkordergrn.id + " where id=" + lrentry.id, new SortedList<string, object>(), conn, transaction);
                InboxManager.Instance(Context.OrganizationID).Update(Context, true, lrentry, transaction, conn);

                lrinvoice = new Lrinvoice();
                lrinvoice.lrid = lrentry.id;
                //lrinvoice.isimported = true;
                lrinvoice.baseamount = oMfgworkordergrn.taxable;
                lrinvoice.billvalue = oMfgworkordergrn.total;
                lrinvoice.companyid = oMfgworkordergrn.companyid;
                lrinvoice.entrydate = oMfgworkordergrn.entrydate;
                lrinvoice.grossamount = oMfgworkordergrn.total;
                lrinvoice.invoicedate = oMfgworkordergrn.invoicedate;
                lrinvoice.suppliercompanyid = oMfgworkordergrn.suppliercompanyid;
                lrinvoice.supplierid = oMfgworkordergrn.supplierid;
                lrinvoice.invoiceno = oMfgworkordergrn.invoiceno;
                lrinvoice.isinclusiveall = true;
                lrinvoice.isactive = true;
                lrinvoice.version = 1;
                lrinvoice.isinterstatesale = oMfgworkordergrn.isinterstatesale;
                lrinvoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                lrinvoice.Lrinvoiceitems = new List<LrinvoiceItemDetail>();
                foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems.GroupBy(e => new { e.taxpercentage, e.taxid }))
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                    {
                        taxamount = item.Sum(e => e.taxamount),
                        taxid = item.Key.taxid,
                        percentage = item.Key.taxpercentage,
                        amounton = item.Sum(e => e.amount - e.taxamount),
                        isinterstatesale = lrinvoice.isinterstatesale,
                        type = Lrinvoicetax.EntryType.Taxable,
                        isactive = true,
                        version = 1
                    });
                }

                foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems)
                {
                    foreach (var sitem in item.sizelist)
                    {
                        lrinvoice.Lrinvoiceitems.Add(new LrinvoiceItemDetail()
                        {
                            basebuyingprice = sitem.cost <= 0 ? 1 : sitem.cost,
                            buyingprice = sitem.cost <= 0 ? 1 : sitem.cost,
                            amount = (sitem.cost <= 0 ? 1 : sitem.cost) * sitem.qty,
                            buyingqty = sitem.qty,
                            designid = item.designname,
                            itemid = sitem.itemid,
                            mrp = sitem.salerate,
                            productid = item.productid,
                            ptaxid = productlist.Find(e => e.id == item.productid).salestaxid,
                            hsncode = productlist.Find(e => e.id == item.productid).hsncode,
                            supplierid = oMfgworkordergrn.supplierid,
                            taxid = productlist.Find(e => e.id == item.productid).salestaxid,
                            Details = new Items()
                            {
                                grnitemid = item.id,
                                grnsizeid = sitem.id,
                                sizegroup = item.size,
                                productid = item.productid,
                                sleeveid = sitem.sleeveid,
                                colourid = sitem.colourid,
                                sizeid = sitem.sizeid,
                                gln = sitem.uid,
                                serialno = sitem.uid,
                                support_gln = true,
                                productname = productlist.Find(e => e.id == item.productid).name,
                                sellingname = productlist.Find(e => e.id == item.productid).name
                            },
                            purchasetax = 0,
                            purchasetaxpercentage = 0,
                            sellingprice = sitem.salerate,
                            wsrate = sitem.wsrate,
                            dealerprice = sitem.dealerprice,
                            sno = lrinvoice.Lrinvoiceitems.Count + 1,
                            stocklocationid = lrentry.retaillocationid,
                            version = 1,
                            isactive = true
                        });
                    }
                }

                LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                lrinvoiceDataService.ImportInvoice(lrinvoice, transaction, conn);
            }
            return lrinvoice;
        }
        public Mfgworkordergrn Insert(Mfgworkordergrn oMfgworkordergrn)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgworkordergrn.entryno", 0,
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ? oMfgworkordergrn.companyid : 0);
                    oMfgworkordergrn.entryno = sequence.GetNextNumber(Context, conn, transaction);
                    oMfgworkordergrn.entrydate = DateTimeNow;
                    if (string.IsNullOrEmpty(oMfgworkordergrn.lrno))
                    {
                        oMfgworkordergrn.lrno = "Auto-" + oMfgworkordergrn.entryno;
                    }
                    if (oMfgworkordergrn.lrdate.Year < 2000)
                        oMfgworkordergrn.lrdate = DateTimeNow.Date;

                    if (string.IsNullOrEmpty(oMfgworkordergrn.invoiceno))
                        oMfgworkordergrn.invoiceno = "Auto-" + oMfgworkordergrn.entryno;
                    if (oMfgworkordergrn.invoicedate.Year < 2000)
                        oMfgworkordergrn.invoicedate = DateTimeNow.Date;

                    Insert(transaction, conn, oMfgworkordergrn);

                    var missingproductids = oMfgworkordergrn.Mfgworkordergrnitems.FindAll(x => x.productid == 0);
                    if (missingproductids.Count > 0)
                    {
                        string productquery = @"select productid,id from mfgworkorderitems where id in (" + string.Join(",", missingproductids.ConvertAll(x => x.mfgworkorderitemsid)) + ")";
                        using (var reader = ExecuteReader(productquery, new SortedList<string, object>(), conn, transaction))
                        {
                            while (reader.Read())
                            {
                                var mfgworkorderitemsid = reader.GetInt64(1);
                                var productid = reader.GetInt64(0);
                                oMfgworkordergrn.Mfgworkordergrnitems.Find(x => x.mfgworkorderitemsid == mfgworkorderitemsid).productid = productid;
                            }
                        }

                    }
                    MfgworkordergrnitemsDataService subMfgworkordergrnitems = new MfgworkordergrnitemsDataService() { Context = this.Context };
                    foreach (var item in oMfgworkordergrn.Mfgworkordergrnitems)
                    {
                        item.mfgworkordergrnid = oMfgworkordergrn.id;
                        subMfgworkordergrnitems.Insert(transaction, conn, item);
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 6)
                    {
                        string desgincodes = string.Join(",", oMfgworkordergrn.Mfgworkordergrnitems.ConvertAll(x => "'" + x.designname + "'"));
                        string validatemfgdesign = string.Format(@"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true  and mfgdesign.code in ({0})
)

select Mfgworkordergrnitems.id,mfgworkordergrnitemsizes.id,
Mfgworkordergrnitems.colourid,alluid.colourid,alluid.colourcode,
Mfgworkordergrnitems.DESIGNITEMCODE,alluid.dname,alluid.uid,
acolours.name acolour,ocolours.name ocolour
from Mfgworkordergrn 
INNER JOIN Mfgworkordergrnitems ON Mfgworkordergrnitems.mfgworkordergrnid = Mfgworkordergrn.ID
INNER JOIN mfgworkordergrnitemsizes ON mfgworkordergrnitemsizes.mfgworkorderitemsgrnid = Mfgworkordergrnitems.ID
left outer join alluid on alluid.uid = mfgworkordergrnitemsizes.uid
left outer join referencelist acolours on acolours.id = alluid.colourid
left outer join referencelist ocolours on ocolours.id = Mfgworkordergrnitems.colourid
WHERE Mfgworkordergrn.id=@id and (alluid.uid is null or Mfgworkordergrnitems.DESIGNITEMCODE != alluid.dname or 
Mfgworkordergrnitems.colourid != alluid.colourid or
mfgworkordergrnitemsizes.colourid != alluid.colourid)", desgincodes);
                        var param = new SortedList<string, object>();
                        param.Add("id", oMfgworkordergrn.id);
                        using (var reader = ExecuteReader(validatemfgdesign, param, conn, transaction))
                        {
                            if (reader.Read())
                            {
                                int failedcount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                if (failedcount > 0)
                                {
                                    string uid = reader["uid"] != DBNull.Value ? reader["uid"].ToString() : "";
                                    string uiddesignname = reader["dname"] != DBNull.Value ? reader["dname"].ToString() : "";
                                    string wodesignname = reader["DESIGNITEMCODE"] != DBNull.Value ? reader["DESIGNITEMCODE"].ToString() : "";
                                    string acolour = reader["acolour"] != DBNull.Value ? reader["acolour"].ToString() : "";
                                    string ocolour = reader["ocolour"] != DBNull.Value ? reader["ocolour"].ToString() : "";

                                    StringBuilder stringBuilder = new StringBuilder();
                                    if (string.IsNullOrEmpty(uid))
                                        stringBuilder.AppendLine("Design UID Removed after work order issue");

                                    if (uiddesignname != wodesignname)
                                        stringBuilder.AppendLine("Design code changed " + wodesignname + "->" + uiddesignname);

                                    if (acolour != ocolour)
                                        stringBuilder.AppendLine("Colour code changed " + ocolour + "->" + acolour);

                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(
                                        new Exception("WO-GRN - Validation failed- " + stringBuilder.ToString() + " conflict"), WebAPI.Common.Logger.LogType.Error, oMfgworkordergrn);
                                    reader.Close();
                                    transaction.Rollback();
                                    throw new Exception("WO Validation failed ,please contact support team - " + reader.GetString(2) + " conflict");
                                }
                            }
                        }
                    }

                    Lrinvoice lrinvoice = null;
                    if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.mfg_grnlrinvoice ||
                        InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.mfg_mode == 6)
                    {
                        lrinvoice = UpdateRetailStock(transaction, conn, oMfgworkordergrn, true);
                    }
                    else
                    {
                        lrinvoice = LinkRetailStock(transaction, conn, oMfgworkordergrn, true);
                    }
                    LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                    lrinvoiceDataService.AutoBarcodeCreation(conn, transaction, lrinvoice, true);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrn);
            }

            return oMfgworkordergrn;
        }

        public Mfgworkordergrn Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkordergrn oMfgworkordergrn)
        {
            if (!string.IsNullOrEmpty(oMfgworkordergrn.product) && oMfgworkordergrn.product.Length > 100)
                oMfgworkordergrn.product = oMfgworkordergrn.product.Substring(0, 98) + "..";
            if (!string.IsNullOrEmpty(oMfgworkordergrn.design) && oMfgworkordergrn.design.Length > 100)
                oMfgworkordergrn.design = oMfgworkordergrn.design.Substring(0, 98) + "..";

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.entryno == null ? "" : oMfgworkordergrn.entryno;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.workorderno == null ? "" : oMfgworkordergrn.workorderno;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.locationid;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.receivinglocationid;
            command.Parameters.Add("workorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.workorderid;
            command.Parameters.Add("joborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.joborderid;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.entrydate;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.invoicedate;
            command.Parameters.Add("workorderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.workorderdate;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.lrdate;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.invoiceno == null ? "" : oMfgworkordergrn.invoiceno;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.lrno == null ? "" : oMfgworkordergrn.lrno;

            command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.lrentryno == null ? "" : oMfgworkordergrn.lrentryno;
            command.Parameters.Add("lrentryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.lrentryid;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrn.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrn.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.taxable;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.total;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrn.statusid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.supplierid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.suppliercompanyid;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.totalqty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.receivedby;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.companyid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.approvedby;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.product == null ? "" : oMfgworkordergrn.product;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.design == null ? "" : oMfgworkordergrn.design;
            command.Parameters.Add("sizelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgworkordergrn.sizelist_json == null ? "null" : oMfgworkordergrn.sizelist_json;
            command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.approvedon;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.mode == null ? "" : oMfgworkordergrn.mode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkordergrn.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgworkordergrn;
        }

        public Mfgworkordergrn Update(Mfgworkordergrn oMfgworkordergrn)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkordergrn);
                    MfgworkordergrnitemsDataService subMfgworkordergrnitems = new MfgworkordergrnitemsDataService() { Context = this.Context };
                    subMfgworkordergrnitems.Update(transaction, conn, oMfgworkordergrn.Mfgworkordergrnitems, oMfgworkordergrn.id);

                    string desgincodes = string.Join(",", oMfgworkordergrn.Mfgworkordergrnitems.ConvertAll(x => "'" + x.designname + "'"));
                    string validatemfgdesign = string.Format(@"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true  and mfgdesign.code in ({0})
)

select Mfgworkordergrnitems.id,mfgworkordergrnitemsizes.id,
Mfgworkordergrnitems.colourid,alluid.colourid,alluid.colourcode,
Mfgworkordergrnitems.DESIGNITEMCODE,alluid.dname,alluid.uid,
acolours.name acolour,ocolours.name ocolour
from Mfgworkordergrn 
INNER JOIN Mfgworkordergrnitems ON Mfgworkordergrnitems.mfgworkordergrnid = Mfgworkordergrn.ID
INNER JOIN mfgworkordergrnitemsizes ON mfgworkordergrnitemsizes.mfgworkorderitemsgrnid = Mfgworkordergrnitems.ID
left outer join alluid on alluid.uid = mfgworkordergrnitemsizes.uid
left outer join referencelist acolours on acolours.id = alluid.colourid
left outer join referencelist ocolours on ocolours.id = Mfgworkordergrnitems.colourid
WHERE Mfgworkordergrn.id=@id and (alluid.uid is null or Mfgworkordergrnitems.DESIGNITEMCODE != alluid.dname or 
Mfgworkordergrnitems.colourid != alluid.colourid or
mfgworkordergrnitemsizes.colourid != alluid.colourid)", desgincodes);
                    var param = new SortedList<string, object>();
                    param.Add("id", oMfgworkordergrn.id);
                    using (var reader = ExecuteReader(validatemfgdesign, param, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            int failedcount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            if (failedcount > 0)
                            {
                                string uid = reader["uid"] != DBNull.Value ? reader["uid"].ToString() : "";
                                string uiddesignname = reader["dname"] != DBNull.Value ? reader["dname"].ToString() : "";
                                string wodesignname = reader["DESIGNITEMCODE"] != DBNull.Value ? reader["DESIGNITEMCODE"].ToString() : "";
                                string acolour = reader["acolour"] != DBNull.Value ? reader["acolour"].ToString() : "";
                                string ocolour = reader["ocolour"] != DBNull.Value ? reader["ocolour"].ToString() : "";

                                StringBuilder stringBuilder = new StringBuilder();
                                if (string.IsNullOrEmpty(uid))
                                    stringBuilder.AppendLine("Design UID Removed after work order issue");

                                if (uiddesignname != wodesignname)
                                    stringBuilder.AppendLine("Design code changed " + wodesignname + "->" + uiddesignname);

                                if (acolour != ocolour)
                                    stringBuilder.AppendLine("Colour code changed " + ocolour + "->" + acolour);

                                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(
                                    new Exception("WO-GRN - Validation failed- " + stringBuilder.ToString() + " conflict"), WebAPI.Common.Logger.LogType.Error, oMfgworkordergrn);

                                reader.Close();
                                transaction.Rollback();
                                throw new Exception("WO Validation failed ,please contact support team - " + reader.GetString(2) + " conflict");
                            }
                        }
                    }

                    var lrinvoice = UpdateRetailStock(transaction, conn, oMfgworkordergrn, true);

                    LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                    lrinvoiceDataService.AutoBarcodeCreation(conn, transaction, lrinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrn);
            }
            return oMfgworkordergrn;
        }

        public Mfgworkordergrn Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkordergrn oMfgworkordergrn)
        {
            if (!string.IsNullOrEmpty(oMfgworkordergrn.product) && oMfgworkordergrn.product.Length > 100)
                oMfgworkordergrn.product = oMfgworkordergrn.product.Substring(0, 98) + "..";
            if (!string.IsNullOrEmpty(oMfgworkordergrn.design) && oMfgworkordergrn.design.Length > 100)
                oMfgworkordergrn.design = oMfgworkordergrn.design.Substring(0, 98) + "..";
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.id;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.entryno == null ? "" : oMfgworkordergrn.entryno;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.workorderno == null ? "" : oMfgworkordergrn.workorderno;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.locationid;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.receivinglocationid;
            command.Parameters.Add("workorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.workorderid;
            command.Parameters.Add("joborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.joborderid;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.entrydate;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.invoicedate;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.lrdate;
            command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.lrentryno == null ? "" : oMfgworkordergrn.lrentryno;
            command.Parameters.Add("lrentryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.lrentryid;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.product == null ? "" : oMfgworkordergrn.product;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.design == null ? "" : oMfgworkordergrn.design;
            command.Parameters.Add("sizelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgworkordergrn.sizelist_json == null ? "null" : oMfgworkordergrn.sizelist_json;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.invoiceno == null ? "" : oMfgworkordergrn.invoiceno;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.lrno == null ? "" : oMfgworkordergrn.lrno;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrn.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrn.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.modifiedon;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.taxable;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.total;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrn.statusid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.supplierid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.suppliercompanyid;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkordergrn.totalqty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.receivedby;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.companyid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.approvedby;
            command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.approvedon;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkordergrn.mode == null ? "" : oMfgworkordergrn.mode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkordergrn.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkordergrn;
        }


        public bool Delete(Mfgworkordergrn oMfgworkordergrn)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var nmfgworkordergrn = Search(new MfgworkordergrnCreteria()
                    {
                        id = oMfgworkordergrn.id
                    }).First();
                    //if (nmfgworkordergrn.version != oMfgworkordergrn.version)
                    //    throw new Exception("Already grn got changed");

                    //long lrentryid = 0;
                    //using (var reader = ExecuteReader("select id from lrentry where isactive = true and importreferencetype=1 and importreferenceid=" + oMfgworkordergrn.id, null))
                    //{
                    //    if (reader.Read())
                    //        lrentryid = reader.GetInt64(0);
                    //}
                    //LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                    //var lrentries = lrentryDataService.Search(new LrentryCreteria()
                    //{
                    //    id = lrentryid
                    //});
                    //if (lrentries.Count == 1)
                    //{
                    //    var lrentry = lrentries.First();
                    //    LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                    //    var lrinvoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
                    //    {
                    //        lrid = lrentry.id,
                    //        showitems = true
                    //    }).First();
                    //    lrinvoiceDataService.Delete(transaction, conn, lrinvoice);
                    //    lrentryDataService.Delete(transaction, conn, lrentry);
                    //}
                    hasSuccessCount = Delete(transaction, conn, oMfgworkordergrn);
                    MfgworkordergrnitemsDataService subMfgworkordergrnitems = new MfgworkordergrnitemsDataService() { Context = this.Context };
                    subMfgworkordergrnitems.DeleteAll(oMfgworkordergrn.id, oMfgworkordergrn.modifiedby, conn, transaction);
                    MfgworkordergrnitemsizesDataService mfgworkordergrnitemsizesDataService = new MfgworkordergrnitemsizesDataService() { Context = this.Context };
                    foreach (var item in nmfgworkordergrn.Mfgworkordergrnitems)
                        foreach (var sitem in item.sizelist)
                            mfgworkordergrnitemsizesDataService.DeleteAll(sitem.id, Context.UserID, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkordergrn);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkordergrn oMfgworkordergrn)
        {
            string query = string.Format(@"select distinct bill.billno from lrentry
inner join lrinvoice on lrinvoice.lrid = lrentry.id
inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid =  lrinvoice.id
inner join stock on stock.invoiceitemid = lrinvoiceitems.id
inner join billitems on billitems.stockid = stock.id
inner join bill on bill.id = billitems.billid
where lrentry.importreferenceid = {0} and importreferencetype=1 and lrentry.isactive=true and bill.isactive=true and billitems.isactive=true and stock.isactive=true and lrinvoiceitems.isactive=true
and lrinvoice.isactive=true ", oMfgworkordergrn.id);
            List<string> billnos = new List<string>();
            using (var reader = ExecuteReader(query, null, conn, transaction))
            {
                while (reader.Read())
                {
                    billnos.Add(reader.GetString(0));
                }
            }

            if (billnos.Count > 0)
            {
                throw new Exception("Can't delete becuase some of the items sold in the bill - " + string.Join(",", billnos));
            }
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkordergrn.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkordergrn.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkordergrn.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkordergrn.id;
            int successCount = command.ExecuteNonQuery();

            query = string.Format(@"
update salablegoods set isactive=false,version=-9999 from stock
inner join lrinvoiceitems on stock.invoiceitemid = lrinvoiceitems.id
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
inner join lrentry on lrentry.id = lrinvoice.lrid and lrentry.importreferenceid = {0} and importreferencetype=1 
where  salablegoods.stockid =  stock.id and salablegoods.isactive=true;

update stock  set isactive=false,version=-9999 from lrinvoiceitems
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
inner join lrentry on lrentry.id = lrinvoice.lrid and lrentry.importreferenceid = {0} and importreferencetype=1 
where  lrinvoiceitems.id =  stock.invoiceitemid and stock.isactive=true;

update lrinvoiceitems set isactive=false,version=-9999 from lrinvoice
inner join lrentry on lrentry.id = lrinvoice.lrid and lrentry.importreferenceid = {0} and importreferencetype=1 
where  lrinvoiceitems.lrinvoiceid =  lrinvoice.id and lrinvoiceitems.isactive=true;

update lrinvoice set isactive=false,version=-9999 where lrid 
in (select id from lrentry where lrentry.importreferenceid = {0} and importreferencetype=1 )
and isactive = true;

update lrentry set isactive=false,version=-9999 where lrentry.importreferenceid = {0} and importreferencetype=1 and isactive=true;", oMfgworkordergrn.id);

            command = new NpgsqlCommand(query, conn, transaction);
            Task<int> readerTask = command.ExecuteNonQueryAsync();
            DateTime dateTime = DateTime.Now;
            while (!readerTask.IsCompleted && DateTime.Now.Subtract(dateTime).TotalMinutes < 5)
            {
                readerTask.Wait(new TimeSpan(0, 0, 15));
            }
            if (DateTime.Now.Subtract(dateTime).TotalMinutes > 5)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("GRN delete delayed"
                    , WebAPI.Common.Logger.LogType.Error, GetSQL(command));
                throw new Exception("GRN delete delayed");
            }
            return readerTask.Result > 0;
        }

        internal List<GRNPrint2Print> PrintBarcode(List<GRNPrint.GRNPrintItems> items)
        {
            var barcodeList = new List<GRNPrint2Print>();
            string grnquery = string.Format(@"select Mfgworkordergrn.id,supplier.code,supplier.name,Mfgworkordergrn.entryno,entrydate,invoicedate,invoiceno from Mfgworkordergrn
            left outer join supplier on supplier.id = Mfgworkordergrn.supplierid
            where Mfgworkordergrn.id in ({0})", string.Join(",", items.ConvertAll(x => x.grnid).Distinct()));

            List<GRNPrint2Print> gRNPrint2Prints = new List<GRNPrint2Print>();
            using (var reader = ExecuteReader(grnquery, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    GRNPrint2Print item = new GRNPrint2Print();
                    item.id = reader["id"] == DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                    item.suppliercode = reader["code"] == DBNull.Value ? Convert.ToString(reader["code"]) : "";
                    item.suppliername = reader["name"] == DBNull.Value ? Convert.ToString(reader["name"]) : "";
                    item.grnno = reader["entryno"] == DBNull.Value ? Convert.ToString(reader["entryno"]) : "";
                    item.invoiceno = reader["invoiceno"] == DBNull.Value ? Convert.ToString(reader["invoiceno"]) : "";
                    item.grndate = reader["entrydate"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : DateTime.MinValue;
                    item.invoicedate = reader["invoicedate"] == DBNull.Value ? Convert.ToDateTime(reader["invoicedate"]) : DateTime.MinValue;
                    gRNPrint2Prints.Add(item);
                }
            }

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            BrandDataService brandDataService = new BrandDataService() { Context = this.Context };
            var colournames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "COLOUR"
            });

            var sizenames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "SIZE"
            });
            var sleevenames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "SLEEVE"
            });
            var fitnames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "FIT"
            });
            var stylenames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "STYLE"
            });
            var materialnames = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "MATERIAL"
            });

            foreach (var designid in items.GroupBy(x => x.designid))
            {
                var design = mfgdesignDataService.Search(new MfgdesignCreteria()
                {
                    id = designid.Key,
                    includedetail = true
                }).First();
                foreach (var ditem in designid)
                {
                    var productnames = productsDataService.Search(new ProductsCreteria()
                    {
                        id = design.productid
                    });
                    var brandnames = brandDataService.Search(new BrandCreteria()
                    {
                        id = design.brandid
                    });
                    if (design.sizegroup.items.Exists(x => x.uid == ditem.uid))
                    {
                        var item = design.sizegroup.items.Find(x => x.uid == ditem.uid);
                        string colourcode = "";
                        if (item.name.IndexOf('/') > 0)
                            colourcode = item.code.Substring(item.code.IndexOf('/') + 1);
                        GRNPrint2Print design2Print = new GRNPrint2Print();
                        design2Print.brand = brandnames.Count > 0 ? brandnames[0].name : "";
                        design2Print.product = productnames.Count > 0 ? productnames[0].name : "";
                        design2Print.colour = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).name : "";
                        design2Print.colourcode = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).code : "";
                        design2Print.size = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).name : "";
                        design2Print.sizecode = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).code : "";

                        design2Print.sleevename = sleevenames.Exists(e => e.id == item.sleeveid) ? sleevenames.Find(e => e.id == item.sleeveid).name : "";
                        design2Print.sleevecode = sleevenames.Exists(e => e.id == item.sleeveid) ? sleevenames.Find(e => e.id == item.sleeveid).code : "";

                        design2Print.colourshade = colourcode;
                        design2Print.design = design.name;
                        design2Print.designcode = item.code;
                        design2Print.id = design.id;
                        design2Print.uid = item.uid;
                        design2Print.name = item.code;
                        design2Print.colourid = item.colourid;
                        design2Print.sizeid = item.sizeid;
                        design2Print.sizegroup = item.sizegroup;
                        design2Print.salerate = item.salerate;
                        design2Print.mrp = item.mrp;
                        design2Print.wsrate = item.wsrate;
                        design2Print.dealerprice = item.dealerprice;
                        design2Print.grnno = gRNPrint2Prints.First().grnno;
                        design2Print.grndate = gRNPrint2Prints.First().grndate;
                        design2Print.invoiceno = gRNPrint2Prints.First().invoiceno;
                        design2Print.invoicedate = gRNPrint2Prints.First().invoicedate;
                        design2Print.suppliercode = gRNPrint2Prints.First().suppliercode;
                        design2Print.suppliername = gRNPrint2Prints.First().suppliername;
                        for (int i = 0; i < ditem.pqty; i++)
                        {
                            barcodeList.Add(design2Print);
                        }
                    }
                }
            }
            return barcodeList;
        }

        internal List<Mfgworkordergrn.Attributes> ChangeStatus(InfyPOS.Models.Mfgworkordergrn.StatusChange item)
        {
            string query = "select attributes,id from mfgworkordergrn where id in (" +
                string.Join(",", item.ids)+")";
            List<Mfgworkordergrn.Attributes> response = new List<Mfgworkordergrn.Attributes>();
            using (var reader = ExecuteReader(query, null))
            {
                while(reader.Read())
                {
                    string attribute = reader["attributes"] == DBNull.Value ? "" : reader["attributes"].ToString();
                    long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    if (attribute == "")
                    {
                        response.Add(new Mfgworkordergrn.Attributes()
                        {
                            id = id
                        });
                    }
                    else
                    {
                        response.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<Mfgworkordergrn.Attributes>(attribute));
                    }
                }
            }
            using (var connection = CreateConnection())
            {
                connection.Open();
                foreach (var sitem in response)
                {
                    if(item.status == 1)
                    {
                        sitem.Opened = new InfyPOS.Models.Mfgworkordergrn.Attributes.Status()
                        {
                            date = item.actionon,
                            employeeid = item.employeeid
                        };
                    }else if (item.status == 2)
                    {
                        sitem.Barcoded = new InfyPOS.Models.Mfgworkordergrn.Attributes.Status()
                        {
                            date = item.actionon,
                            employeeid = item.employeeid
                        };
                    }
                    else if (item.status == 3)
                    {
                        sitem.Racking = new InfyPOS.Models.Mfgworkordergrn.Attributes.Status()
                        {
                            date = item.actionon,
                            employeeid = item.employeeid
                        };
                    }
                    NpgsqlCommand command = new NpgsqlCommand("update mfgworkordergrn set attributes=@attributes where id=@id", connection);
                    command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(sitem);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sitem.id;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return response;
        }
    }

}
