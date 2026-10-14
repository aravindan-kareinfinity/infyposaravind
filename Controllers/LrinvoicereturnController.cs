using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;
using WebAPI.Data;
using Newtonsoft.Json;
using System.IO;

namespace InfyPOS.Controllers
{
    [RoutePrefix("LrinvoicereturnService")]
    public class LrinvoicereturnServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrinvoicereturn>();
            result.Item = new Lrinvoicereturn();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateDB_CRNotes")]
        public async Task<IHttpActionResult> CreateDB_CRNotes(ActionRequest<LrinvoicereturnAuto> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.CreateDB_CRNotes(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<LrinvoicereturnProductsCreteria> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            Lrinvoicereturn result = await Task<Lrinvoicereturnitems>.Run(() => SearchItems(request.Item));
            return this.Ok(result);
        }

        public void SearchItemsByBarcode(LrinvoicereturnProductsCreteria creteria, Lrinvoicereturn returninvoice, List<Lrinvoicereturnitems> resultItems)
        {
            ProductsDataService productdataservice = new ProductsDataService();
            TaxDataService taxdataservice = new TaxDataService();
            LrinvoiceDataService invoicedataservice = new LrinvoiceDataService();
            StockDataService StockData = new StockDataService();
            List<StockWithTax> results = new List<StockWithTax>();

            if (!creteria.allowall &&
                InfyPOS.Services.ConfigManager.Instance(StockData.Context.OrganizationID).Config.system_disablereturnsupplier)
                creteria.allowall = true;

                if (!string.IsNullOrEmpty(creteria.barcode))
            {
                creteria.barcodes = new List<string>();
                creteria.barcodes.Add(creteria.barcode);
                results.AddRange(StockData.Search(creteria.companyid, creteria.barcodes));
            }
            else
            {
                results.AddRange(StockData.Search(creteria.companyid, creteria.barcodes));
                var astock = results.FindAll(e => e.qty > 0);
            }

            if (InfyPOS.Services.ConfigManager.Instance(StockData.Context.OrganizationID).Config.system_fixed_barcodes &&
                !InfyPOS.Services.ConfigManager.Instance(StockData.Context.OrganizationID).Config.system_hyper_uan)
            {
                if (creteria.barcodes != null)
                {
                    creteria.barcodes.Clear();
                    creteria.barcodes.AddRange(results.ConvertAll(e => e.barcode));
                    results.ForEach(e =>
                    {
                        if (creteria.barcodes.Contains(e.serialno))
                            e.displaycode = e.serialno;
                        else
                            e.displaycode = e.barcode;
                    });
                }
                else
                {
                    if (results[0].serialno == creteria.barcode)
                        results[0].displaycode = results[0].serialno;
                    else
                        results[0].displaycode = results[0].barcode;
                }
            }

            var tax = taxdataservice.RangedTaxProducts(results.ConvertAll(e => e.barcode).ToList());

            List<Salablegoods> sitems = null;
            if (creteria.locationid > 0 && (!string.IsNullOrEmpty(creteria.barcode) || (creteria.barcodes != null && creteria.barcodes.Count > 0)))
            {
                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService();
                sitems = salablegoodsDataService.Search(new SalablegoodsCreteria()
                {
                    barcode = (creteria.barcodes != null && creteria.barcodes.Count>0) ? "" : creteria.barcode,
                    barcodes = creteria.barcodes,
                    locationid = creteria.locationid
                });
            }
            else
            {
                results.ForEach(e => e.qty = e.qty - e.transfered);
            }

            foreach (var invoice in results.GroupBy(e => e.lrinvoiceid))
            {
                if (invoice.Key > 0)
                {
                    var invoiceList = invoicedataservice.Search(new LrinvoiceCreteria()
                    {
                        id = invoice.Key
                    });

                    if (invoiceList.Count == 1)
                    {
                        returninvoice.suppliercompanyid = invoiceList[0].suppliercompanyid;
                        returninvoice.supplierid = invoiceList[0].supplierid;
                    }
                    foreach (var result in invoice)
                    {
                        if (result != null && result.id > 0 && (creteria.allowall || result.supplierid == creteria.supplierid))
                        {

                            Lrinvoicereturnitems item = new Lrinvoicereturnitems();
                            item.barcode = result.barcode;
                            item.itemid = result.itemid;
                            item.lrinvoiceitemsid = result.invoiceitemid;
                            item.productid = result.productid;
                            item.printingname = result.printingname;
                            item.stockid = result.id;
                            item.qty = result.qty - (result.transfered + result.journalqty + result.damagedqty + result.returnqty);
                            item.designid = result.designid;
                            item.serialno = result.serialno;
                            item.imageid = result.imageid;
                            item.companyid = result.companyid;
                            item.iscut = result.sellingmode == 3;
                            returninvoice.supplierid = result.supplierid;

                            if (sitems != null && sitems.Exists(e => e.barcode == result.barcode))
                            {

                                var sitem = sitems.Find(e => e.barcode == result.barcode);
                                item.salablegoodsid = sitem.id;
                                item.companyid = sitem.companyid;
                                item.qty = sitem.iqty - (sitem.oqty + sitem.dqty);

                            }
                            else if (creteria.locationid > 0)
                            {
                                item.qty = 0;
                            }

                            item.rate = result.buyingprice + result.purchasediscount;
                            item.tax = result.purchasetax;
                            item.taxpercentage = result.taxpercentage;
                            item.taxid = result.taxid;
                            item.discount = result.purchasediscount;
                            item.discountpercentage = result.purchasediscountpercentage;

                            if (invoiceList.Count == 1)
                            {
                                item.invoicedate = invoiceList[0].invoicedate;
                                item.invoiceno = invoiceList[0].invoiceno;
                                item.suppliercompanyid = invoiceList[0].suppliercompanyid;
                                item.supplierid = invoiceList[0].supplierid;
                                item.isinterstatesale = invoiceList[0].isinterstatesale;
                            }
                            //var products = productdataservice.Search(new ProductsCreteria()
                            //{
                            //    id = item.productid
                            //});
                            //if (products.Count > 0)
                            //{
                            //    var tax = taxdataservice.Search(new TaxCreteria()
                            //    {
                            //        id = products[0].purchasetaxid
                            //    });
                            //    if (tax.Count > 0)
                            //    {
                            //        item.taxid = products[0].purchasetaxid;
                            //        item.taxpercentage = tax[0].taxsplit.Find(result.buyingprice).taxpercent;
                            //        item.tax = Math.Round(result.buyingprice * item.taxpercentage / 100, 2);
                            //    }
                            //}

                            if (item.taxid == 0 && tax.Count>0 && tax.ContainsKey(result.barcode))
                            {
                                var taxvalue = tax[result.barcode];
                                item.taxid = taxvalue.id;
                                item.taxpercentage = taxvalue.taxsplit.Find(result.buyingprice).taxpercent;
                                item.tax = Math.Round(result.buyingprice * item.taxpercentage / 100, 2);
                            }
                            //}
                            resultItems.Add(item);
                        }
                    }
                }
                else
                {
                    // Stock which are not having Invoice Information.. Mainly stock entered through Direct Stock Entry.
                    foreach (var result in invoice)
                    {
                        if (result.supplierid > 0)
                        {
                            SuppliercompanyDataService suppcomp = new SuppliercompanyDataService();
                            var suppliercompany = suppcomp.Search(new SuppliercompanyCreteria() { supplierid = result.supplierid });
                            if (suppliercompany != null && suppliercompany.Count > 0)
                            {
                                returninvoice.suppliercompanyid = suppliercompany[0].id;
                                returninvoice.supplierid = suppliercompany[0].supplierid;
                            }
                        }
                        Lrinvoicereturnitems item = new Lrinvoicereturnitems();
                        item.barcode = result.barcode;
                        item.imageid = result.imageid;
                        item.itemid = result.itemid;
                        item.lrinvoiceitemsid = result.invoiceitemid;
                        item.productid = result.productid;
                        item.printingname = result.printingname;
                        item.stockid = result.id;
                        item.qty = result.qty - (result.transfered + result.journalqty + result.damagedqty + result.returnqty);
                        item.designid = result.designid;
                        item.serialno = result.serialno;

                        item.companyid = result.companyid;
                        item.iscut = result.sellingmode == 3;
                        returninvoice.supplierid = result.supplierid;

                        if (sitems != null && sitems.Exists(e => e.barcode == result.barcode))
                        {

                            var sitem = sitems.Find(e => e.barcode == result.barcode);
                            item.salablegoodsid = sitem.id;
                            item.companyid = sitem.companyid;
                            item.qty = sitem.iqty - (sitem.oqty + sitem.dqty);

                        }
                        else if (creteria.locationid > 0)
                        {
                            item.qty = 0;
                        }

                        item.rate = result.buyingprice + result.purchasediscount;
                        item.tax = result.purchasetax;
                        item.taxid = result.taxid;
                        item.discount = result.purchasediscount;
                        item.discountpercentage = result.purchasediscountpercentage;

                        item.invoicedate = StockData.DateTimeNow.Date;
                        item.invoiceno = "000";
                        item.suppliercompanyid = returninvoice.suppliercompanyid;
                        item.supplierid = returninvoice.supplierid;
                        item.isinterstatesale = returninvoice.isinterstatesale;

                        if (item.taxid == 0 && tax.Count > 0 && tax.ContainsKey(result.barcode))
                        {
                            var taxvalue = tax[result.barcode];
                            item.taxid = taxvalue.id;
                            item.taxpercentage = taxvalue.taxsplit.Find(result.buyingprice).taxpercent;
                            item.tax = Math.Round(result.buyingprice * item.taxpercentage / 100, 2);
                        }
                        //}
                        resultItems.Add(item);
                    }
                }
            }
        }

        private Lrinvoicereturn SearchItems(LrinvoicereturnProductsCreteria creteria)
        {
            ProductsDataService productdataservice = new ProductsDataService();
            LrinvoiceitemsDataService invoiceitemsdataservice = new LrinvoiceitemsDataService();
            TaxDataService taxdataservice = new TaxDataService();
            List<Lrinvoicereturnitems> resultItems = new List<Lrinvoicereturnitems>();
            Lrinvoicereturn returninvoice = new Lrinvoicereturn();
            LrinvoiceDataService invoicedataservice = new LrinvoiceDataService();
            returninvoice.Lrinvoicereturnitems = resultItems;
            creteria.returndate = (creteria.returndate == DateTime.MinValue ? invoicedataservice.DateTimeNow : creteria.returndate);

            if (creteria.mode == "1")
            {
                SearchItemsByBarcode(creteria, returninvoice, resultItems);
            }
            else
            {
                var invoiceList = invoicedataservice.Search(new LrinvoiceCreteria()
                {
                    supplierid = creteria.supplierid,
                    invoiceno = creteria.invoiceno,
                    lrentryno = creteria.lrentryno,
                    LimitOne = true
                    //fromdate = creteria.returndate.AddMonths(-12),
                    //todate = creteria.returndate
                });
                if (invoiceList.Count > 0)
                {
                    var invoice = invoiceList.OrderByDescending(e => e.invoicedate).First();
                    returninvoice.supplierid = invoice.supplierid;
                    returninvoice.suppliercompanyid = invoice.suppliercompanyid;

                    var invoiceitems = invoiceitemsdataservice.SearchAllDetails(invoice.id);
                    var products = productdataservice.Search(new ProductsCreteria()
                    {
                        ids = invoiceitems.ConvertAll(e => e.productid).Distinct().ToList()
                    });

                    var taxlist = taxdataservice.Search(new TaxCreteria());


                    foreach (var invitem in invoiceitems)
                    {
                        if (creteria.mode == "damage" &&
                            invitem.damagedqty - invitem.returnqty <= 0)
                            continue;

                        Lrinvoicereturnitems item = new Lrinvoicereturnitems();
                        item.suppliercompanyid = invoice.suppliercompanyid;
                        item.supplierid = invoice.supplierid;
                        item.imageid  = invitem.imgid;
                        item.isinterstatesale = invoice.isinterstatesale;
                        item.companyid = invoice.companyid;
                        item.itemid = invitem.itemid;
                        item.lrinvoiceitemsid = invitem.id;
                        item.productid = invitem.productid;
                        item.sno = invitem.sno;
                        item.printingname = invitem.Details.productname;
                        item.sizename = invitem.Details.sizename;
                        item.designid = invitem.designid;
                        item.brandname = invitem.Details.brandname;
                        if (invoice.isvendorbill)
                        {
                            item.sizename = item.sizename + "[" + invitem.barcode + "]";
                        }
                        //item.sizeid = invitem.sizeid;
                        item.stockid = 0;
                        if (creteria.returntype == Lrinvoicereturn.ReturnTypes.DebitNote)
                        {
                            item.qty = creteria.mode == "damage" ? invitem.damagedqty : (invitem.buyingqty - (invitem.sellableqty + invitem.returnqty));
                            item.rate = invitem.buyingprice + invitem.purchasediscount;
                            item.discountpercentage = invitem.purchasediscountpercentage;
                            item.discount = invitem.purchasediscount;
                        }
                        else
                        {
                            item.qty = invitem.buyingqty - (invitem.damagedqty + invitem.returnqty);
                            item.rate = invitem.buyingprice + invitem.purchasediscount;
                            item.discountpercentage = invitem.purchasediscountpercentage;
                            item.discount = invitem.purchasediscount;
                        }
                        item.invoiceno = invoice.invoiceno;
                        item.invoicedate = invoice.invoicedate;
                        if (item.qty <= 0) continue;
                        if (products.Exists(e => e.id == invitem.productid))
                        {
                            var product = products.Find(e => e.id == invitem.productid);
                            if (taxlist.Exists(e => e.id == product.purchasetaxid))
                            {
                                var tax = taxlist.Find(e => e.id == product.purchasetaxid);
                                item.taxid = product.purchasetaxid;
                                item.taxpercentage = tax.taxsplit.Find(true, invitem.buyingprice).taxpercent;
                                item.tax = Math.Round(invitem.buyingprice * item.taxpercentage / 100, 2);
                                item.iscut = product.sellingmode == 3;
                            }
                        }
                        resultItems.Add(item);
                    }

                    if (creteria.includebarcoded)
                    {
                        foreach (var invitem in invoiceitems)
                        {
                            if (invitem.sellableqty > 0)
                            {
                                StockDataService StockData = new StockDataService();
                                var stockitems = StockData.SearchAll(invitem.id);
                                creteria.barcodes = stockitems.ConvertAll(e => e.barcode).ToList();
                                creteria.barcode = null;
                                if (creteria.barcodes.Count > 0)
                                    SearchItemsByBarcode(creteria, returninvoice, resultItems);
                            }
                        }
                    }

                    if (invoice != null && invoice.Lrinvoicetaxitems != null &&
                        invoice.Lrinvoicetaxitems.Exists(e => e.type == Lrinvoicetax.EntryType.Packing))
                    {
                        returninvoice.packingcharge = invoice.Lrinvoicetaxitems.Find(e => e.type == Lrinvoicetax.EntryType.Packing).netamount;
                    }
                    if (invoice != null && invoice.Lrinvoicetaxitems != null &&
                        invoice.Lrinvoicetaxitems.Exists(e => e.type == Lrinvoicetax.EntryType.Fright))
                    {
                        returninvoice.freightamt = invoice.Lrinvoicetaxitems.Find(e => e.type == Lrinvoicetax.EntryType.Fright).amounton;
                        returninvoice.freighttaxamt = invoice.Lrinvoicetaxitems.Find(e => e.type == Lrinvoicetax.EntryType.Fright).taxamount;
                        returninvoice.freighttaxid = invoice.Lrinvoicetaxitems.Find(e => e.type == Lrinvoicetax.EntryType.Fright).taxid;
                    }
                }
            }

            var lrinvoiceitemsids = returninvoice.Lrinvoicereturnitems.ConvertAll(e => e.lrinvoiceitemsid).Distinct();
            SortedDictionary<long, decimal> discountlist = invoiceitemsdataservice.SearchAllDiscount(lrinvoiceitemsids.ToList());
            foreach(var discout in discountlist)
            {
                var items = returninvoice.Lrinvoicereturnitems.FindAll(e => e.lrinvoiceitemsid == discout.Key);
                foreach(var item in items)
                {
                    //item.discount = discout.Value;
                    //item.discountpercentage = Math.Round(item.discount / item.rate * 100,4);
                    //item.tax = Math.Round(item.tax * item.discountpercentage / 100, 4);
                    var discount = discout.Value;
                    var discountpercentage = Math.Round(discount / item.rate * 100, 4);
                    var tax = Math.Round(item.tax * discountpercentage / 100, 4);

                    item.discount += discount;
                    item.discountpercentage += discountpercentage;
                    item.tax += tax;
                }
            }
            return returninvoice;
        }

        [HttpGet]
        [Route("Barcode")]
        public async Task<IHttpActionResult> Search(string id)
        {

            StockDataService StockData = new StockDataService();
            long companyid = 0;
            if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportcompanywisebarcode)
            {
                if (StockData.Context.CompanyID <= 0)
                    companyid = long.MaxValue;
                else
                    companyid = StockData.Context.CompanyID;
            }

            StockWithTax result = await Task<StockWithTax>.Run(() => StockData.Search(companyid, id));

            if (result != null && result.id > 0)
            {
                //Lrinvoicereturnitems item = new Lrinvoicereturnitems();
                //item.barcode = result.barcode;
                //item.itemid = result.itemid;
                //item.lrinvoiceitemsid = result.invoiceitemid;
                //item.productid = result.productid;
                //item.printingname = result.printingname;
                //LrinvoiceitemsDataService itemdata = new LrinvoiceitemsDataService();
                //var iteminfo = itemdata.Search(new LrinvoiceitemsCreteria
                //{
                //    id = item.lrinvoiceitemsid
                //});
                //if (iteminfo.Count == 1)
                //{
                //    item.rate = iteminfo[0].buyingprice;
                //    item.tax = iteminfo[0].purchasetax;
                //    item.taxid = iteminfo[0].taxid;
                //    item.discount = iteminfo[0].salediscount;
                //}

                Lrinvoicereturnitems item = new Lrinvoicereturnitems();
                item.barcode = result.barcode;
                item.itemid = result.itemid;
                item.lrinvoiceitemsid = result.invoiceitemid;
                item.productid = result.productid;
                item.printingname = result.printingname;
                item.stockid = result.id;
                item.qty = result.qty - result.returnqty;
                item.designid = result.designid;
                item.iscut = result.sellingmode == 3;
                //returninvoice.supplierid = result.supplierid;
                //LrinvoiceitemsDataService itemdata = new LrinvoiceitemsDataService();
                //var iteminfo = itemdata.Search(new LrinvoiceitemsCreteria
                //{
                //    id = item.lrinvoiceitemsid
                //});
                //if (iteminfo.Count == 1)
                //{
                item.rate = result.buyingprice;
                item.tax = result.purchasetax;
                item.taxid = result.taxid;
                item.discount = result.discount;
                //}
                return this.Ok(item);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrinvoicereturnCreteria> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            List<Lrinvoicereturn> result = await Task<List<Lrinvoicereturn>>.Run(() => LrinvoicereturnData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Lrinvoicereturnitems != null)
                {
                    foreach (var item in request.Item.Lrinvoicereturnitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                //if(InfyPOS.Services.ConfigManager.Instance(LrinvoicereturnData.Context.OrganizationID).Config.system_disablereturnsupplier)
                //{
                //    request.Item.Lrinvoicereturnitems.ForEach(x =>
                //    {
                //        x.companyid = request.Item.companyid;
                //        x.suppliercompanyid = request.Item.suppliercompanyid;
                //        x.supplierid = request.Item.supplierid;
                //        x.isinterstatesale = request.Item.isinterstatesale;
                //    });
                //    Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.Insert(request.Item));
                //    return this.Ok(result);
                //}
                //else 
                if (request.Item.Lrinvoicereturnitems.GroupBy(e => new { e.companyid, e.supplierid, e.suppliercompanyid }).Count() == 1)
                {
                    request.Item.companyid = request.Item.Lrinvoicereturnitems[0].companyid;
                    request.Item.suppliercompanyid = request.Item.Lrinvoicereturnitems[0].suppliercompanyid;
                    request.Item.supplierid = request.Item.Lrinvoicereturnitems[0].supplierid;
                    request.Item.isinterstatesale = request.Item.isinterstatesale ? true : request.Item.Lrinvoicereturnitems[0].isinterstatesale;
                    Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.Insert(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    List<Lrinvoicereturn> result = await Task<List<Lrinvoicereturn>>.Run(() => LrinvoicereturnData.InsertAll(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Lrinvoicereturnitems != null)
                {
                    foreach (var item in request.Item.Lrinvoicereturnitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("GenerateEInvoice")]
        public async Task<IHttpActionResult> GenerateEInvoice(ActionRequest<LrinvoicereturnCreteria> request)
        {
            LrinvoicereturnDataService BillinboxData = new LrinvoicereturnDataService();
            InfyGST.GSTPortal.EInvoiceResponse result = await Task<InfyGST.GSTPortal.EInvoiceResponse>.Run(() => BillinboxData.GenerateEInvoice(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("GenerateEWay")]
        public async Task<IHttpActionResult> GenerateEWay(ActionRequest<LrinvoicereturnCreteria> request)
        {
            LrinvoicereturnDataService BillinboxData = new LrinvoicereturnDataService();
            InfyGST.GSTPortal.EWAYbillResponse result = await Task<InfyGST.GSTPortal.EWAYbillResponse>.Run(() => BillinboxData.GenerateEWay(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.Update(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("UpdateLRInfo")]
        public async Task<IHttpActionResult> UpdateLRInfo(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.UpdateLRInfo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ClosedSupplier")]
        public async Task<IHttpActionResult> ClosedSupplier(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            Lrinvoicereturn result = await Task<Lrinvoicereturn>.Run(() => LrinvoicereturnData.ClosedSupplier(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            bool result = await Task<bool>.Run(() => LrinvoicereturnData.Delete(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<LrinvoicereturnCreteria> request)
        {
            LrinvoicereturnDataService LrinvoiceData = new LrinvoicereturnDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.ChangeStatus(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("UpdateHSN")]
        public async Task<IHttpActionResult> UpdateHSN(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            bool result = await Task<bool>.Run(() => LrinvoicereturnData.UpdateHSN(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<Lrinvoicereturn> request)
        {
            LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
            LrinvoicereturnPrint result = await Task<LrinvoicereturnPrint>.Run(() => LrinvoicereturnData.Print(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                LrinvoicereturnDataService LrinvoicereturnData = new LrinvoicereturnDataService();
                LrinvoicereturnData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                LrinvoicereturnPrint result = LrinvoicereturnData.Print(new Lrinvoicereturn() { id = long.Parse(id) });
                if (result == null) return null;

                FileContentInfo byteArray = PrintDebitNote(long.Parse(organizationid), result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintDebitNote(long organizationid, LrinvoicereturnPrint result)
        {
            if (result == null)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            string key = result.returntype == 0 ? "DEBITNOTE" : "CREDITNOTE";
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = key,
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = key
                });
            }
            if (lstconfig == null || lstconfig.Count == 0)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }


            if (lstconfig.Count>1 && lstconfig.Exists(x=>x.name == result.reporttype))
            {
                lstconfig = lstconfig.FindAll(x => x.name == result.reporttype);
            }

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            if (lstconfig[0].reportformat.format.Contains("HSNList"))
            {
                result.HSNList = new List<LrinvoicereturnPrint.DebitNoteHSN>();
                foreach (var grp in result.Lrinvoicereturnitems.GroupBy(e => new { e.hsncode, e.taxpercentage }))
                {
                    result.HSNList.Add(new LrinvoicereturnPrint.DebitNoteHSN()
                    {
                        code = grp.Key.hsncode,
                        taxable = grp.Sum(e => e.taxableamount),
                        tax = grp.Sum(e => e.tax),
                        total = grp.Sum(e => e.netamount),
                        qty = grp.Sum(e => e.qty),
                        cgst_percentage = (result.isinterstatesale ? 0 : grp.First().taxsplit.cgsttaxpercentage),
                        sgst_percentage = (result.isinterstatesale ? 0 : grp.First().taxsplit.sgsttaxpercentage),
                        igst_percentage = (result.isinterstatesale ? grp.First().taxsplit.cgsttaxpercentage : 0),
                        sgst_value = (result.isinterstatesale ? 0 : grp.Sum(e =>e.taxsplit.sgsttaxamount)),
                        cgst_value = (result.isinterstatesale ? 0 : grp.Sum(e => e.taxsplit.cgsttaxamount)),
                        igst_value = (result.isinterstatesale ? grp.Sum(e => e.taxsplit.cgsttaxamount) : 0)
                    });
                }
            }

            if (lstconfig[0].reportformat.format.Contains("invoiceserialno") 
                || lstconfig[0].reportformat.format.Contains("groupeddesignid"))
            {
                List<Lrinvoicereturnitems> lrinvoicereturnitems = new List<Lrinvoicereturnitems>();
                foreach (var item in result.Lrinvoicereturnitems.GroupBy(e => new
                {
                    e.hsncode,
                    e.invoiceno,
                    e.particulars,
                    e.rate
                }))
                {
                    lrinvoicereturnitems.Add(new Lrinvoicereturnitems()
                    {
                        printingname = item.First().printingname,
                        designid = item.First().designid,
                        groupeddesignid = item.First().designid,
                        hsncode = item.First().hsncode,
                        invoiceno = item.First().invoiceno,
                        invoicedate = item.First().invoicedate,
                        qty = item.Sum(e => e.qty),
                        rate = item.First().rate,
                        amount = item.Sum(e => e.amount),
                        discount = item.Sum(e => e.discount),
                        discountpercentage = Math.Round((item.Sum(e => e.discount) / item.Sum(e => e.amount)) * 100, 2),
                        tax = item.Sum(e => e.tax),
                        sno = lrinvoicereturnitems.Count + 1,
                        taxsplit = new TaxSplit()
                        {
                            cgsttaxamount = item.Sum(e => e.taxsplit.cgsttaxamount),
                            name = item.First().taxsplit.name,
                            secondtax = item.First().taxsplit.secondtax,
                            sgsttaxamount = item.Sum(e => e.taxsplit.sgsttaxamount),
                            totalamount = item.Sum(e => e.taxsplit.totalamount),
                            cgsttaxpercentage = item.First().taxsplit.cgsttaxpercentage,
                            sgsttaxpercentage = item.First().taxsplit.sgsttaxpercentage
                        },
                        invoiceserialno = item.First().invoiceserialno,
                        taxpercentage = item.First().taxpercentage,
                        isactive = false
                    });
                }
                result.Lrinvoicereturnitems = lrinvoicereturnitems;
            }

            
            labreport.DataItems = result.Lrinvoicereturnitems;
            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }

            if (!string.IsNullOrEmpty(result.einvoicejson))
            {
                InfyGST.GSTPortal.EInvoiceResponse res = JsonConvert.DeserializeObject<InfyGST.GSTPortal.EInvoiceResponse>(result.einvoicejson);
                var img = imageData.FromBytes(Convert.FromBase64String(res.QrCodeImage));
                result.GSTEinvoice = res;
                labreport.Additional.Add("IMG-" + "EINVQR", img);
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}