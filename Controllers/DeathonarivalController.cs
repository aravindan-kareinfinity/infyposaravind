
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

namespace InfyPOS.Controllers
{
    [RoutePrefix("DeathonarrivalService")]
    public class DeathonarrivalServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Deathonarrival>();
            result.Item = new Deathonarrival();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<DeathonarrivalCreteria> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            List<Deathonarrival> result = await Task<List<Deathonarrival>>.Run(() => DeathonarrivalData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<LrinvoicereturnProductsCreteria> request)
        {
            DOA result = await Task<DOA>.Run(() => SearchItems(request.Item));
            return this.Ok(result);
        }

        [Route("Barcode")]
        public async Task<IHttpActionResult> Search(string id)
        {
            StockDataService StockData = new StockDataService();
            StockWithTax result = await Task<StockWithTax>.Run(() => StockData.Search(0,id));

            if (result != null && result.id > 0)
            {
                Deathonarrival item = new Deathonarrival();
                item.barcode = result.barcode;
                item.itemid = result.itemid;
                item.invoiceitemid = result.invoiceitemid;
                item.productid = result.productid;
                item.printingname = result.printingname;
                item.stockid = result.id;
                item.qty = result.qty - result.returnqty;
                item.rate = result.buyingprice;
                item.tax = result.purchasetax;
                item.taxid = result.taxid;
                item.discount = result.discount;
                return this.Ok(item);
            }
            return this.NotFound();
        }
        public void SearchItemsByBarcode(LrinvoicereturnProductsCreteria creteria, DOA returninvoice, List<Deathonarrival> resultItems)
        {
            ProductsDataService productdataservice = new ProductsDataService();
            TaxDataService taxdataservice = new TaxDataService();
            LrinvoiceDataService invoicedataservice = new LrinvoiceDataService();
            StockDataService StockData = new StockDataService();

            long companyid = 0;
            if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_supportcompanywisebarcode)
            {
                if (StockData.Context.CompanyID <= 0)
                    companyid = long.MaxValue;
                else
                    companyid = StockData.Context.CompanyID;
            }

            List<StockWithTax> results = new List<StockWithTax>();
            if (!string.IsNullOrEmpty(creteria.barcode))
            {
                results.Add(StockData.Search(companyid,creteria.barcode));
            }
            else if (creteria.barcodes != null)
            {
                results.AddRange(StockData.Search(companyid,creteria.barcodes));
            } else if (creteria.invoiceitemids != null)
            {
                results.AddRange(StockData.Search(companyid,creteria.invoiceitemids));
            }

            var tax = taxdataservice.RangedTaxProducts(results.ConvertAll(e => e.barcode).ToList());

            List<Salablegoods> sitems = null;
            if (creteria.locationid > 0 && (!string.IsNullOrEmpty(creteria.barcode) || (creteria.barcodes != null && creteria.barcodes.Count > 0)))
            {
                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService();
                sitems = salablegoodsDataService.Search(new SalablegoodsCreteria()
                {
                    barcode = creteria.barcode,
                    barcodes = creteria.barcodes,
                    locationid = creteria.locationid
                });
            }

            foreach (var invoice in results.GroupBy(e => e.lrinvoiceid))
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

                        Deathonarrival item = new Deathonarrival();
                        item.barcode = result.barcode;
                        item.itemid = result.itemid;
                        item.invoiceitemid = result.invoiceitemid;
                        item.productid = result.productid;
                        item.printingname = string.IsNullOrEmpty(result.itemname) ? result.printingname : result.itemname;
                        item.stockid = result.id;
                        item.qty = result.qty - (result.transfered + result.journalqty + result.damagedqty + result.returnqty);
                        item.sellingprice = result.price - result.discount;
                        item.companyid = result.companyid;
                        returninvoice.supplierid = result.supplierid;

                        if (sitems != null && sitems.Exists(e => e.barcode == result.barcode))
                        {
                            var sitem = sitems.Find(e => e.barcode == result.barcode);
                            item.salablegoodsid = sitem.id;
                            item.locationid = sitem.locationid;
                            item.companyid = sitem.companyid;
                            item.sellingprice = sitem.price - sitem.discount;
                            item.qty = sitem.iqty - (sitem.oqty + sitem.jqty + sitem.dqty);
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

                        if (invoiceList.Count == 1)
                        {
                            item.invoicedate = invoiceList[0].invoicedate;
                            item.invoiceno = invoiceList[0].invoiceno;
                            item.suppliercompanyid = invoiceList[0].suppliercompanyid;
                            item.supplierid = invoiceList[0].supplierid;
                        }
                       
                        if (tax.ContainsKey(result.barcode))
                        {
                            var taxvalue = tax[result.barcode];
                            item.taxid = taxvalue.id;
                            item.taxpercentage = taxvalue.taxsplit.Find(result.buyingprice).taxpercent;
                            item.tax = Math.Round(result.buyingprice * item.taxpercentage / 100, 2);
                        }
                        resultItems.Add(item);
                    }
                }
            }
        }

        private DOA SearchItems(LrinvoicereturnProductsCreteria creteria)
        {
            ProductsDataService productdataservice = new ProductsDataService();
            TaxDataService taxdataservice = new TaxDataService();
            List<Deathonarrival> resultItems = new List<Deathonarrival>();
            DOA returninvoice = new DOA();
            LrinvoiceDataService invoicedataservice = new LrinvoiceDataService();
            returninvoice.Items = resultItems;
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
                    LimitOne = true
                    //fromdate = creteria.returndate.AddMonths(-12),
                    //todate = creteria.returndate
                });
                if (invoiceList.Count > 0)
                {
                    var invoice = invoiceList.OrderByDescending(e => e.invoicedate).First();
                    returninvoice.supplierid = invoice.supplierid;
                    returninvoice.suppliercompanyid = invoice.suppliercompanyid;
                    LrinvoiceitemsDataService invoiceitemsdataservice = new LrinvoiceitemsDataService();
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

                        Deathonarrival item = new Deathonarrival();
                        item.suppliercompanyid = invoice.suppliercompanyid;
                        item.supplierid = invoice.supplierid;
                        item.itemid = invitem.itemid;
                        item.invoiceitemid = invitem.id;
                        item.productid = invitem.productid;
                        item.printingname = invitem.Details.productname;
                        item.stockid = 0;
                        item.qty = creteria.mode == "damage" ? invitem.damagedqty : (invitem.buyingqty - invitem.sellableqty);
                        //- invitem.returnqty;
                        //item.qty = invitem.buyingqty - invitem.sellableqty - invitem.returnqty;
                        item.rate = invitem.buyingprice + invitem.purchasediscount;
                        item.discountpercentage = invitem.purchasediscountpercentage;
                        //item.tax = Math.Round(invitem.purchasetax / item.qty, 2);
                        //item.taxid = invitem.taxid;
                        item.discount = invitem.purchasediscount;
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
                                item.taxpercentage = tax.taxsplit.Find(invitem.buyingprice).taxpercent;
                                item.tax = Math.Round(invitem.buyingprice * item.taxpercentage / 100, 2);
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
                                SearchItemsByBarcode(creteria, returninvoice, resultItems);
                            }
                        }
                    }
                }
            }
            return returninvoice;
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<DOA> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            DOA result = await Task<DOA>.Run(() => DeathonarrivalData.Update(request.Item));
            return this.Ok(result);
        }
        
        [HttpPost]
        [Route("SaveMultiple")]
        public async Task<IHttpActionResult> SaveMultiple(ActionRequest<DOA> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            DOA result = await Task<DOA>.Run(() => DeathonarrivalData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Deathonarrival> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            if (request.Item.id <= 0)
            {
                Deathonarrival result = await Task<Deathonarrival>.Run(() => DeathonarrivalData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Deathonarrival result = await Task<Deathonarrival>.Run(() => DeathonarrivalData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Deathonarrival> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            Deathonarrival result = await Task<Deathonarrival>.Run(() => DeathonarrivalData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Deathonarrival> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            Deathonarrival result = await Task<Deathonarrival>.Run(() => DeathonarrivalData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Deathonarrival> request)
        {
            DeathonarrivalDataService DeathonarrivalData = new DeathonarrivalDataService();
            bool result = await Task<bool>.Run(() => DeathonarrivalData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}