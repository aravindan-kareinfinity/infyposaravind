
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
    [RoutePrefix("SalablegoodsService")]
    public class SalablegoodsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Salablegoods>();
            result.Item = new Salablegoods();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SalablegoodsCreteria> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            List<Salablegoods> result = await Task<List<Salablegoods>>.Run(() => SalablegoodsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search4Refill")]
        public async Task<IHttpActionResult> Search4Refill(ActionRequest<RefilCreteria> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            List<RefillResult> result = await Task<List<Salablegoods>>.Run(() => SalablegoodsData.Search4Refill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeRefillID")]
        public async Task<IHttpActionResult> ChangeRefillID(ActionRequest<RefillChange> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            bool result = await Task<bool>.Run(() => SalablegoodsData.ChangeRefillID(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchGoods")]
        public async Task<IHttpActionResult> SearchGoods(ActionRequest<SalablegoodsCreteria> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            List<SalableResult> result = await Task<List<SalableResult>>.Run(() => SalablegoodsData.SearchGoods(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("BatchList/{locationid}/{productid}")]
        public async Task<IHttpActionResult> ProductList(string locationid, string productid)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();

            var result = await Task<List<BatchBarcodes>>.Run(() => StockData.ProductList(locationid, productid));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchBrandPrice/{customerid}/{locationid}")]
        public async Task<IHttpActionResult> SearchBrandPrice(string customerid, string locationid)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();
            var result = await Task<List<SellableGoodsBrandPrice>>.Run(() => StockData.SearchBrandPrice(customerid, locationid));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchCustomerHistory/{customerid}/{productid}/{days}")]
        public async Task<IHttpActionResult> SearchCustomerHistory(string customerid, string productid,string days)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();
            var result = await Task<List<SellableGoodsBrandPrice>>.Run(() => StockData.SearchCustomerHistory(customerid, productid, days));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("BatchListByBrand/{locationid}/{brandid}/{customerid?}/{salereturn?}")]
        public async Task<IHttpActionResult> BatchListByBrand(string locationid, string brandid, string customerid, string salereturn)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();

            var result = await Task<List<BatchBarcodes>>.Run(() => StockData.BatchListByBrand(locationid, brandid, customerid, salereturn));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("BatchListBySO")]
        public async Task<IHttpActionResult> BatchListBySO(ActionRequest<BatchPO> request)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();
            var result = await Task<List<BatchBarcodes>>.Run(() => StockData.BatchListBySO(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("BatchList/{locationid}/{supplierid}/{productid}")]
        public async Task<IHttpActionResult> BatchList(string locationid, string supplierid, string productid)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();

            var result = await Task<List<BatchBarcodes>>.Run(() => StockData.BatchSearch(locationid, supplierid, productid));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Barcode/{locationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> Barcode(string locationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;

            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            if (id.IndexOf("~str~") >= 0) id = id.Replace("~str~", "*");
            var result = await Task<Barcodedgoods>.Run(() => StockData.BarcodeSearch(locationid, id, false));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("ReturnBarcode/{locationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> ReturnBarcode(string locationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            if (id.IndexOf("~str~") >= 0) id = id.Replace("~str~", "*");
            var result = await Task<Barcodedgoods>.Run(() => StockData.BarcodeSearch(locationid, id, true));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SavePriceList")]
        public async Task<IHttpActionResult> SavePriceList(ActionRequest<SalablegoodspriceCreteria> request)
        {
            SalablegoodspriceDataService SalablegoodsData = new SalablegoodspriceDataService();
            List<Salablegoodsprice> result = await Task<List<Salablegoodsprice>>.Run(() => SalablegoodsData.SavePriceList(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("RemovePriceList")]
        public async Task<IHttpActionResult> RemovePriceList(ActionRequest<SalablegoodspriceCreteria> request)
        {
            SalablegoodspriceDataService SalablegoodsData = new SalablegoodspriceDataService();
            bool result = await Task<bool>.Run(() => SalablegoodsData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("BatchPriceList/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> BatchPriceList(
            string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            SalablegoodspriceDataService StockData = new SalablegoodspriceDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            if (id.IndexOf("~str~") >= 0) id = id.Replace("~str~", "*");
            var result = await Task<List<Salablegoodsprice>>.Run(() => 
                StockData.Search(new SalablegoodspriceCreteria()
                {
                    barcode = id,
                    includedeleted =false
                })) ;
            return this.Ok(result);
        }

        [HttpGet]
        [Route("BatchPrice/{locationid}/{price}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> BatchPrice(string locationid,
            string price,
            string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            SalablegoodspriceDataService StockData = new SalablegoodspriceDataService();
            if (string.IsNullOrEmpty(price) || price == "-")
                price = "0";
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            if (id.IndexOf("~str~") >= 0) id = id.Replace("~str~", "*");
            var result = await Task<List<Salablegoodsprice>>.Run(() => StockData.Search(long.Parse(locationid),
               decimal.Parse(price), id)); ;
            return this.Ok(result);
        }

        [HttpGet]
        [Route("UAN/{locationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> UAN(string locationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            SalablegoodsDataService StockData = new SalablegoodsDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            if (id.IndexOf("~str~") >= 0) id = id.Replace("~str~", "*");
            var result = await Task<Barcodedgoods>.Run(() => StockData.UANSearch(locationid, id, true));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("FindStock/{barcode}")]
        public async Task<IHttpActionResult> FindStock(string barcode)
        {
            StockDataService StockData = new StockDataService();
            var result = await Task<Barcodedgoods>.Run(() => StockData.FindStock(barcode));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchBarcode")]
        public async Task<IHttpActionResult> SearchBarcode(ActionRequest<SalesorderDataService.SearchBarcodeData> request)
        {
            if (request.Item.includesold &&
                !string.IsNullOrEmpty(request.Item.code))
            {
                Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                Barcodedgoods result = await Task<Barcodedgoods>.Run(() =>
                salesorderDataService.SearchBarcode(request.Item, true));
                return this.Ok(result);
            }
            if (request.Item.qty <= 0) request.Item.qty = 1;
            if (request.Item.source == "SOPLAN")
            {
                Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                List<Barcodedgoods> result = await Task<List<Barcodedgoods>>.Run(() =>
                salesorderDataService.SearchPlannedOrder(request.Item));
                return this.Ok(result);
            }
            else if (request.Item.source == "UAN")
            {
                Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                List<Barcodedgoods> result = await Task<List<Barcodedgoods>>.Run(() =>
                {
                    if (string.IsNullOrEmpty(request.Item.code))
                    {
                        if (request.Item.barcodes != null && request.Item.barcodes.Count > 0)
                        {
                            List<Barcodedgoods> result1 = new List<Barcodedgoods>();
                            request.Item.barcodes.GroupBy(x=>x).ToList().ForEach(x =>
                            {
                                var salablegoods = salesorderDataService.SearchPlannedOrder(request.Item, x.Key,
                           request.Item.locationid,
                           request.Item.sono, false);
                                if (salablegoods.Count > 0)
                                {
                                    salablegoods[0].qty = x.Count();
                                    result1.AddRange(salablegoods);
                                }
                            });
                            return result1;
                        }

                    }
                    else
                    {
                        return salesorderDataService.SearchPlannedOrder(request.Item,request.Item.code,
                            request.Item.locationid,
                            request.Item.sono, false);
                    }
                    return new List<Barcodedgoods>();
                });

                return this.Ok(result);
            }
            else if (request.Item.source == "DESIGN/SIZESET/COLOUR")
            {
                Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                List<Barcodedgoods> result = await Task<List<Barcodedgoods>>.Run(() =>
                salesorderDataService.SearchDesignSizeSetColourStock(request.Item, false));
                return this.Ok(result);
            }
            else if (request.Item.barcodes != null && request.Item.barcodes.Count > 0)
            {
                Data.SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService();
                List<Barcodedgoods> result = await Task<Barcodedgoods>.Run(() =>
                salablegoodsDataService.BarcodeSearch(request.Item.locationid, request.Item.barcodes,
                request.Item.usedstock, request.Item.returngoods, request.Item.returngoods,false));
                if (InfyPOS.Services.ConfigManager.Instance(salablegoodsDataService.Context.OrganizationID).Config.mfg_mode == 5)
                {
                    Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                    salesorderDataService.UpdateBarcodePrices(request.Item, result);
                }
                else
                {
                    if (result.Count > 0 &&
                        InfyPOS.Services.ConfigManager.Instance(salablegoodsDataService.Context.OrganizationID).Config.system_ismfgapplication)
                    {
                        Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                        result = salesorderDataService.ChangePriceFromDesign(result,request.Item, InfyPOS.Services.ConfigManager.Instance(salesorderDataService.Context.OrganizationID).Config.mfg_salepricefromsku);

                        MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                        var uanitems = mfgdesignDataService.SearchUANItem(result.ConvertAll(x => x.barcode));
                        foreach (var bitem in result)
                        {
                            if (uanitems.Exists(x => x.uid == bitem.barcode))
                            {
                                var uanitem = uanitems.Find(x => x.uid == bitem.barcode);
                                if (uanitem != null)
                                {
                                    bitem.buyingprice = uanitem.cost;
                                    bitem.price = uanitem.salerate;
                                    bitem.groupkey = uanitem.name;
                                    bitem.priceextax = uanitem.salerate;
                                    bitem.wsrateextax = uanitem.wsrate;
                                    bitem.dealerpriceextax = uanitem.dealerprice;
                                    bitem.wsrate = uanitem.wsrate;
                                    bitem.dealerprice = uanitem.dealerprice;
                                    bitem.mrp = uanitem.mrp;
                                    bitem.fgboxid = uanitem.boxid;
                                    bitem.soboxid = uanitem.boxid;
                                }
                            }
                        }
                    }
                    else
                    {
                        result.ForEach(x =>
                        {
                            if (x.wsrate == 0 && x.dealerprice > 0)
                            {
                                x.wsrate = x.dealerprice;
                            }
                            if (x.wsrate > 0 && x.dealerprice == 0)
                            {
                                x.dealerprice = x.wsrate;
                            }
                            if (x.wsrate == 0 && x.dealerprice == 0)
                            {
                                x.wsrate = x.price;
                                x.dealerprice = x.price;
                            }
                            x.priceextax = x.price;
                            x.wsrateextax = x.wsrate;
                            x.dealerpriceextax = x.dealerprice;
                            x.wsrate = x.wsrate;
                        });
                    }
                    result.FindAll(x => x.price == 0 || x.priceextax == 0).ForEach(x =>
                    {
                        if (x.wsrate > 0)
                        {
                            x.price = x.wsrate;
                            x.priceextax = x.wsrate;
                        }
                        else if (x.dealerprice > 0)
                        {
                            x.price = x.dealerprice;
                            x.priceextax = x.dealerprice;
                        }
                    });
                }
                return this.Ok(result);
            }
            else
            {
                Data.SalesorderDataService salesorderDataService = new SalesorderDataService();
                Barcodedgoods result = await Task<Barcodedgoods>.Run(() =>
                salesorderDataService.SearchBarcode(request.Item, false));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Salablegoods> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Salablegoods result = await Task<Salablegoods>.Run(() => SalablegoodsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Salablegoods result = await Task<Salablegoods>.Run(() => SalablegoodsData.Update(request.Item, request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Salablegoods> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            Salablegoods result = await Task<Salablegoods>.Run(() => SalablegoodsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Salablegoods> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            Salablegoods result = await Task<Salablegoods>.Run(() => SalablegoodsData.Update(request.Item, request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Salablegoods> request)
        {
            SalablegoodsDataService SalablegoodsData = new SalablegoodsDataService();
            bool result = await Task<bool>.Run(() => SalablegoodsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
    [RoutePrefix("Online")]
    public class OnlineController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateSaleQty/{token}/{organizationid}/{locationid}/{barcode}/{qty}")]
        public async Task<IHttpActionResult> UpdateSaleQty(string token, string organizationid, string locationid, string barcode, string qty)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(organizationid) || string.IsNullOrEmpty(locationid)
                    || string.IsNullOrEmpty(barcode) || string.IsNullOrEmpty(qty))
                    throw new Exception("Invalid input.");
                if (!ValidateToken(token))
                    throw new Exception("Invalid user token");
                if (long.Parse(organizationid) == 0 || long.Parse(locationid) == 0)
                    throw new Exception("Invalid company/location");
                if (decimal.Parse(qty) == 0)
                    throw new Exception("Invalid Qty");

                SalablegoodsDataService salablegoodsData = new SalablegoodsDataService();
                decimal outqty = salablegoodsData.UpdateStock(new Salablegoods()
                {
                    locationid = long.Parse(locationid),
                    barcode = barcode,
                    oqty = decimal.Parse(qty)
                });

                return this.Ok(new OnlineStockStatus()
                {
                    success = true,
                    barcode = barcode,
                    qty = outqty
                });
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
                return this.Ok(new OnlineStockStatus()
                {
                    success = false,
                    error = exp.Message
                });
            }
        }
        private bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            UsersDataService UsersData = new UsersDataService();
            List<Users> result = UsersData.Search(new UsersCreteria() { accesskey = token });
            if (result != null && result.Count > 0) return true;
            return false;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetStock/{token}/{organizationid}/{locationid}/{barcode}")]
        public async Task<IHttpActionResult> GetStock(string token, string organizationid, string locationid, string barcode)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(organizationid) || string.IsNullOrEmpty(locationid)
                    || string.IsNullOrEmpty(barcode))
                    throw new Exception("Invalid input.");
                if (!ValidateToken(token))
                    throw new Exception("Invalid user token");
                if (long.Parse(organizationid) == 0 || long.Parse(locationid) == 0)
                    throw new Exception("Invalid company/location");

                SalablegoodsDataService salablegoodsData = new SalablegoodsDataService();
                decimal outqty = salablegoodsData.GetStock(new Salablegoods()
                {
                    locationid = long.Parse(locationid),
                    barcode = barcode
                });

                return this.Ok(new OnlineStockStatus()
                {
                    success = true,
                    barcode = barcode,
                    qty = outqty
                });
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
                return this.Ok(new OnlineStockStatus()
                {
                    success = false,
                    error = exp.Message
                });
            }
        }
    }
    public class OnlineStockStatus
    {
        public bool success { get; set; }
        public string error { get; set; }
        public string barcode { get; set; }
        public decimal qty { get; set; }
    }

}