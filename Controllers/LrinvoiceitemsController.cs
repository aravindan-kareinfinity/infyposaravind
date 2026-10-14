
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
    [RoutePrefix("LrinvoiceitemsService")]
    public class LrinvoiceitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrinvoiceitems>();
            result.Item = new Lrinvoiceitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrinvoiceitemsCreteria> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceitemsData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchBarcodeImages")]
        public async Task<IHttpActionResult> SearchBarcodeImages(ActionRequest<BarcodeImageRequest> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            List<BarcodeImageResponse> result = await Task<List<BarcodeImageResponse>>.Run(() => LrinvoiceitemsData.SearchBarcodeImages(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDesignList")]
        public async Task<IHttpActionResult> SearchDesignList(ActionRequest<LrinvoiceitemsCreteria> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            List<String> result = await Task<List<String>>.Run(() => LrinvoiceitemsData.SearchDesignList(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateB2BSKU")]
        public async Task<IHttpActionResult> CreateB2BSKU(ActionRequest<BarcodeImageRequest> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            string result = await Task<string>.Run(() => LrinvoiceitemsData.CreateB2BSKU(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStockLocation")]
        public async Task<IHttpActionResult> ChangeStockLocation(ActionMulitpleRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceitemsData.ChangeStockLocation(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveMultiple")]
        public async Task<IHttpActionResult> SaveMultiple(ActionRequest<LrinvoiceItems> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceitemsData.SaveMultiple(request.Item));
            ActionMulitipleResponse<LrinvoiceItemDetail> response = new ActionMulitipleResponse<LrinvoiceItemDetail>(result);
            response.Success = true;
            return this.Ok(response);
        }

        [HttpPost]
        [Route("UploadBarcode")]
        public async Task<IHttpActionResult> UploadBarcode(ActionRequest<Lrinvoiceitemsbarcode> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceitemsData.UploadBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UploadSecondaryBarcode")]
        public async Task<IHttpActionResult> UploadSecondaryBarcode(ActionRequest<Lrinvoiceitemsbarcode> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceitemsData.UploadSecondaryBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteBarcode")]
        public async Task<IHttpActionResult> DeleteBarcode(ActionRequest<PrimitiveTypes<long>> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceitemsData.DeleteBarcode(request.Item.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdatePrice")]
        public async Task<IHttpActionResult> SavePriceList(ActionMulitpleRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceitemsData.UpdatePrice(request.Items));
            ActionMulitipleResponse<LrinvoiceItemDetail> response = new ActionMulitipleResponse<LrinvoiceItemDetail>(result);
            response.Success = true;
            return this.Ok(response);
        }


        [HttpPost]
        [Route("SaveMultipleItems")]
        public async Task<IHttpActionResult> SaveMultipleItems(ActionMulitpleRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceitemsData.UpdateAll(request.Items));
            ActionMulitipleResponse<LrinvoiceItemDetail> response = new ActionMulitipleResponse<LrinvoiceItemDetail>(result);
            response.Success = true;
            return this.Ok(response);
        }

        [HttpPost]
        [Route("UpdateActualQty")]
        public async Task<IHttpActionResult> UpdateActualQty(ActionRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            var results = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.Search(new LrinvoiceitemsCreteria()
            {
                id = request.Item.id
            }));

            if (results.Count == 1 && request.Item.actualqty != results[0].actualqty)
            {
                LrinvoiceItemDetail result = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.UpdateActualQty(request.Item));
                ActionSingleResponse<LrinvoiceItemDetail> response = new ActionSingleResponse<LrinvoiceItemDetail>(result);
                response.Success = true;
                return this.Ok(response);
            }
            return this.Ok("Not able to update");
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Lrinvoiceitems result = await Task<Lrinvoiceitems>.Run(() => LrinvoiceitemsData.Insert(request.Item));
                ActionSingleResponse<Lrinvoiceitems> response = new ActionSingleResponse<Lrinvoiceitems>(result);
                response.Success = true;
                return this.Ok(response);
            }
            else if (request.Item.id > 0 && request.Item.damagedquantity > 0)
            {
                LrinvoiceItemDetail result = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.UpdateDamagedQty(request.Item));
                ActionSingleResponse<LrinvoiceItemDetail> response = new ActionSingleResponse<LrinvoiceItemDetail>(result);
                response.Success = true;
                return this.Ok(response);
            }
            else 
            {
                var results = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.Search(new LrinvoiceitemsCreteria()
                {
                    id = request.Item.id
                }));

                // Moved this option to a new Method called UpdateActualQty
                //if (results.Count == 1 && request.Item.actualqty != results[0].actualqty)
                //{
                //    LrinvoiceItemDetail result = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.UpdateActualQty(request.Item));
                //    ActionSingleResponse<LrinvoiceItemDetail> response = new ActionSingleResponse<LrinvoiceItemDetail>(result);
                //    response.Success = true;
                //    return this.Ok(response);
                //}
                //else
                if (results.Count == 1)
                {
                    request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                    bool hasCreatedNewItem = false;
                    bool updateStockWithNewID = false;
                    if (request.Item.validateitem)
                    {
                        ItemsDataService itemservice = new ItemsDataService();
                        var item = itemservice.Search(new ItemsCreteria()
                        {
                            brandid = request.Item.Details.brandid,
                            colourid = request.Item.Details.colourid,
                            materialid = request.Item.Details.materialid,
                            patternid = request.Item.Details.patternid,
                            typeid = request.Item.Details.typeid,
                            sizeid = request.Item.Details.sizeid,
                            styleid = request.Item.Details.styleid,
                            productid = request.Item.productid,
                            fitid = request.Item.Details.fitid,
                            sleeveid = request.Item.Details.sleeveid,
                            agmtcode = request.Item.itbuscode,
                            productmap = true
                        });
                        if (item.Count == 1)
                        {
                            request.Item.itemid = item[0].id;
                            updateStockWithNewID = true;
                        }
                        else
                        {
                            var newitem = request.Item.Details;
                            newitem.isactive = true;
                            newitem.salemode = 1;
                            newitem.version = 1;
                            newitem = itemservice.Insert(newitem);
                            request.Item.itemid = newitem.id;
                            hasCreatedNewItem = true;
                            updateStockWithNewID = true;
                        }
                    }
                    LrinvoiceItemDetail result = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.Update(request.Item));

                    if (updateStockWithNewID)
                    {
                        StockDataService stockdataservice = new StockDataService();
                        stockdataservice.UpdateItemID(request.Item.id, request.Item.itemid, request.Item.designid);
                    }

                    if(request.Item.newbuyingqty>0)
                    {
                        LrinvoiceitemsData.ChangeNewBuyingQty(request.Item);
                    }
                    ActionSingleResponse<Lrinvoiceitems> response = new ActionSingleResponse<Lrinvoiceitems>(result);
                    response.Success = true;
                    response.ErrorMessage = hasCreatedNewItem ? "hasCreatedNewItem" : "";
                    return this.Ok(response);
                }
            }
            return this.Ok("Something missing.");

        }
        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            LrinvoiceItemDetail result = await Task<LrinvoiceItemDetail>.Run(() => LrinvoiceitemsData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<LrinvoiceItemDetail> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            Lrinvoiceitems result = await Task<Lrinvoiceitems>.Run(() => LrinvoiceitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrinvoiceitems> request)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}