

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
    [RoutePrefix("PricedropService")]
    public class PricedropServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Pricedrop>();
            result.Item = new Pricedrop();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PricedropCreteria> request)
        {
            PricedropDataService PricedropData = new PricedropDataService();
            List<Pricedrop> result = await Task<List<Pricedrop>>.Run(() => PricedropData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Pricedrop> request)
        {
            PricedropDataService PricedropData = new PricedropDataService();
            if (request.Item.id <= 0)
            {
                Pricedrop result = await Task<Pricedrop>.Run(() => PricedropData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Pricedrop result = await Task<Pricedrop>.Run(() => PricedropData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Pricedrop> request)
        {
            PricedropDataService PricedropData = new PricedropDataService();
            Pricedrop result = await Task<Pricedrop>.Run(() => PricedropData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Pricedrop> request)
        {
            PricedropDataService PricedropData = new PricedropDataService();
            Pricedrop result = await Task<Pricedrop>.Run(() => PricedropData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Pricedrop> request)
        {
            PricedropDataService PricedropData = new PricedropDataService();
            bool result = await Task<bool>.Run(() => PricedropData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<LrinvoicereturnProductsCreteria> request)
        {
            PricedropMaster result = await Task<PricedropMaster>.Run(() => SearchItems(request.Item));
            return this.Ok(result);
        }

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
            StockWithTax result = await Task<StockWithTax>.Run(() => StockData.Search(companyid,id));

            if (result != null && result.id > 0)
            {
                Pricedrop item = new Pricedrop();
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
        public void SearchItemsByBarcode(LrinvoicereturnProductsCreteria creteria, PricedropMaster returninvoice, List<Pricedrop> resultItems)
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
            }
            else if (creteria.invoiceitemids != null)
            {
                results.AddRange(StockData.Search(companyid,creteria.invoiceitemids));
            }

            var tax = taxdataservice.RangedTaxProducts(results.ConvertAll(e => e.barcode).ToList());

            List<Salablegoods> sitems = null;
            if ( (!string.IsNullOrEmpty(creteria.barcode) || (creteria.barcodes != null && creteria.barcodes.Count > 0)))
            {
                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService();
                sitems = salablegoodsDataService.Search(new SalablegoodsCreteria()
                {
                    barcode = creteria.barcode,
                    barcodes = creteria.barcodes,
                    locationid = creteria.locationid
                });
                sitems.RemoveAll(e => e.iqty - (e.dqty + e.oqty) == 0);
            }

            PricedropDataService pricedropDataService = new PricedropDataService();
            var lastpricedrops = pricedropDataService.SearchLatest(results.ConvertAll(e => e.barcode));

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
                    if (result != null && result.id > 0 && (creteria.allowall || creteria.supplierid == 0 || result.supplierid == creteria.supplierid))
                    {

                        Pricedrop item = new Pricedrop();
                        item.barcode = result.barcode;
                        item.itemid = result.itemid;
                        item.invoiceitemid = result.invoiceitemid;
                        item.productid = result.productid;
                        item.printingname = string.IsNullOrEmpty(result.itemname) ? result.printingname : result.itemname;
                        item.stockid = result.id;
                        item.qty = result.qty - (result.transfered + result.journalqty + result.damagedqty + result.returnqty);
                        item.sellingprice = result.price - result.discount;

                        item.dealerprice = result.dealerprice;
                        item.minrate = result.minrate;
                        item.mrp = result.mrp;

                        if(lastpricedrops.Exists(e=>e.barcode == result.barcode))
                        {
                            var lastprice = lastpricedrops.Find(e => e.barcode == result.barcode);
                            item.dealerprice = lastprice.dealerprice;
                            item.minrate = lastprice.minrate;
                            item.mrp = lastprice.mrp;
                        }
                        item.companyid = result.companyid;
                        item.pricedrop = 0;
                        returninvoice.supplierid = result.supplierid;

                        if (sitems != null && sitems.Exists(e => e.barcode == result.barcode))
                        {
                            var sitem = sitems.Find(e => e.barcode == result.barcode);
                            item.salablegoodsid = sitem.id;
                            item.locationid = sitem.locationid;
                            item.companyid = sitem.companyid;
                            item.sellingprice = sitem.price - sitem.discount;
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

        private PricedropMaster SearchItems(LrinvoicereturnProductsCreteria creteria)
        {
            ProductsDataService productdataservice = new ProductsDataService();
            TaxDataService taxdataservice = new TaxDataService();
            List<Pricedrop> resultItems = new List<Pricedrop>();
            PricedropMaster returninvoice = new PricedropMaster();
            LrinvoiceDataService invoicedataservice = new LrinvoiceDataService();
            returninvoice.Items = resultItems;
            creteria.returndate = (creteria.returndate == DateTime.MinValue ? invoicedataservice.DateTimeNow : creteria.returndate);

            if (creteria.barcodes != null && creteria.barcodes.Count> 0)
            {
                SearchItemsByBarcode(creteria, returninvoice, resultItems);
            }
            if (!string.IsNullOrEmpty(creteria.barcode))
            {
                creteria.barcodes = new List<string>();
                creteria.barcodes.Add(creteria.barcode);
                creteria.barcode = null;
                SearchItemsByBarcode(creteria, returninvoice, resultItems);
            }
            else
            {
                creteria.barcodes = invoicedataservice.SearchBarcodes(creteria);
                creteria.barcode = null;
                SearchItemsByBarcode(creteria, returninvoice, resultItems);
            }
            return returninvoice;
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<PricedropMaster> request)
        {
            PricedropDataService DeathonarrivalData = new PricedropDataService();
            PricedropMaster result = await Task<PricedropMaster>.Run(() => DeathonarrivalData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveMultiple")]
        public async Task<IHttpActionResult> SaveMultiple(ActionRequest<PricedropMaster> request)
        {
            PricedropDataService DeathonarrivalData = new PricedropDataService();
            PricedropMaster result = await Task<PricedropMaster>.Run(() => DeathonarrivalData.Insert(request.Item));
            return this.Ok(result);
        }
    }
}