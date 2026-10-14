
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
    [RoutePrefix("PricecontrolService")]
    public class PricecontrolServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Pricecontrol>();
            result.Item = new Pricecontrol();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PricecontrolCreteria> request)
        {
            if (request.Item.fetchprices)
            {
                request.Item.itemid = request.Item.id;
                request.Item.id = 0;
            }
            PricecontrolDataService PricecontrolData = new PricecontrolDataService();
            List<Pricecontrol> result = await Task<List<Pricecontrol>>.Run(() =>
            request.Item.fetchprices ? PricecontrolData.SearchAll(request.Item) : PricecontrolData.Search(request.Item));
            if (result.Count > 1 && request.Item.price > 0)
            {
                if (result.Exists(e => e.pricefrom <= request.Item.price && e.priceto >= request.Item.price))
                {
                    result = result.FindAll(e => e.pricefrom <= request.Item.price && e.priceto >= request.Item.price).ToList();
                }
                else
                {
                    result = result.FindAll(e => e.pricefrom == 0 || e.priceto == 0).ToList();
                }
            }
            if (request.Item.fetchprices)
            {
                if (request.Item.itemid == 0)
                {
                    ItemsDataService ids = new ItemsDataService();
                    var item = ids.Search(new ItemsCreteria()
                    {
                        brandid = request.Item.brandid,
                        colourid = request.Item.colourid,
                        materialid = request.Item.materialid,
                        patternid = request.Item.patternid,
                        productid = request.Item.productid,
                        sizeid = request.Item.sizeid,
                        styleid = request.Item.styleid,
                        typeid = request.Item.typeid,
                        agmtcode = request.Item.itemcode,
                        productmap = true
                    });
                    if (item.Count > 0)
                        request.Item.itemid = item[0].id;
                }
                Pricecontrolinfo pci = new Pricecontrolinfo();
                pci.items = result;


                LrinvoiceitemsDataService lritemdata = new LrinvoiceitemsDataService();
                var items = lritemdata.Search(new LrinvoiceitemsCreteria
                {
                    fetchprice = true,
                    suppliercompanyid = request.Item.suppliercompanyid,
                    itemid = request.Item.itemid,
                    designid = request.Item.designid,
                    productid = request.Item.productid
                });

                if (items.Count == 0 && !string.IsNullOrEmpty(request.Item.designid))
                {
                    items = lritemdata.Search(new LrinvoiceitemsCreteria
                    {
                        fetchprice = true,
                        suppliercompanyid = request.Item.suppliercompanyid,
                        itemid = request.Item.itemid,
                        productid = request.Item.productid
                    });
                }

                if (items.Count == 0 && request.Item.itemid > 0)
                {
                    items = lritemdata.Search(new LrinvoiceitemsCreteria
                    {
                        fetchprice = true,
                        suppliercompanyid = request.Item.suppliercompanyid,
                        designid = request.Item.designid,
                        productid = request.Item.productid
                    });
                }

                if (items.Count == 0 && !string.IsNullOrEmpty(request.Item.designid))
                {
                    items = lritemdata.Search(new LrinvoiceitemsCreteria
                    {
                        fetchprice = true,
                        suppliercompanyid = request.Item.suppliercompanyid,
                        productid = request.Item.productid
                    });
                }

                if (items.Count == 0 && !string.IsNullOrEmpty(request.Item.designid))
                {
                    items = lritemdata.Search(new LrinvoiceitemsCreteria
                    {
                        fetchprice = true,
                        productid = request.Item.productid
                    });
                }

                if (items.Count > 0)
                {
                    pci.lastpricelist = items;
                    pci.lastprice = items.First();
                }

                return this.Ok(pci);
            }
            else
            {
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<PricecontrolCreteria> request)
        {
            PricecontrolDataService PricecontrolData = new PricecontrolDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                List<Pricecontrol> result = await Task<List<Pricecontrol>>.Run(() => PricecontrolData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                List<Pricecontrol> result = await Task<List<Pricecontrol>>.Run(() => PricecontrolData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Activate")]
        public async Task<IHttpActionResult> Activate(ActionRequest<Pricecontrol> request)
        {
            PricecontrolDataService PricecontrolData = new PricecontrolDataService();
            Pricecontrol result = await Task<Pricecontrol>.Run(() => PricecontrolData.Activate(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<PricecontrolCreteria> request)
        {
            PricecontrolDataService PricecontrolData = new PricecontrolDataService();
            List<Pricecontrol> result = await Task<List<Pricecontrol>>.Run(() => PricecontrolData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<PricecontrolCreteria> request)
        {
            PricecontrolDataService PricecontrolData = new PricecontrolDataService();
            List<Pricecontrol> result = await Task<List<Pricecontrol>>.Run(() => PricecontrolData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Pricecontrol> request)
        {
            PricecontrolDataService PricecontrolData = new PricecontrolDataService();
            bool result = await Task<bool>.Run(() => PricecontrolData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}