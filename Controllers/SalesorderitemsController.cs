
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
    [RoutePrefix("SalesorderitemsService")]
    public class SalesorderitemsServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Salesorderitems>();
            result.Item = new Salesorderitems();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdatePrice")]
        public async Task<IHttpActionResult> UpdatePrice(ActionRequest<SalesorderitemsCreteria> request)
        {
            SalesorderitemsDataService SalesorderitemsData = new SalesorderitemsDataService();
            List<SalesorderitemsDetail> result = await Task<List<SalesorderitemsDetail>>.Run(() =>
            {
                var list = SalesorderitemsData.UpdatePrice(request.Item);
                if (request.Item.groupbysizegroup)
                {
                    return GroupSize(list);
                }
                return list;
            });
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SalesorderitemsCreteria> request)
        {
            SalesorderitemsDataService SalesorderitemsData = new SalesorderitemsDataService();
            List<SalesorderitemsDetail> result = await Task<List<SalesorderitemsDetail>>.Run(() =>
            {
                var list = SalesorderitemsData.Search(request.Item);
                if (request.Item.groupbysizegroup)
                {
                    return GroupSize(list);
                }
                return list;
            });
            return this.Ok(result);
        }

        public List<SalesorderitemsDetail> GroupSize(List<SalesorderitemsDetail> list)
        {

            List<SalesorderitemsDetail> retList = list.GroupBy(e => new
            {
                e.designitemcode,
                e.designid,
                e.colourid,
                e.price,
                e.addnlchargeid,
                e.fgboxid,
                e.soboxid,
                e.mrp
            }).ToList().ConvertAll(e => new SalesorderitemsDetail()
            {
                id = -1,
                designid = e.Key.designid,
                oqty = e.Average(x => x.oqty),
                tqty = e.Sum(x => x.oqty),
                description = e.First().description,
                colourid = e.Key.colourid,
                designitemcode =e.Key.designitemcode,
                colourcode = e.First().colourcode,
                colourname = e.First().colourname,
                brandid = e.First().brandid,
                brandname = e.First().brandname,
                productid = e.First().productid,
                productname = e.First().productname,
                imageid = e.ToList().Exists(x => !string.IsNullOrEmpty(x.imageid)) ? e.ToList().First(x => string.IsNullOrEmpty(x.imageid)).imageid : "",
                sizegroup = !string.IsNullOrEmpty(e.First().sizegroup) ? e.First().sizegroup : GetSizeGroup(e.ToList().ConvertAll(x => x.size)),
                size = !string.IsNullOrEmpty(e.First().sizegroup) ? e.First().sizegroup : GetSizeGroup(e.ToList().ConvertAll(x => x.size)),
                price = e.First().price,
                mrp = e.First().mrp,
                addnlcharge = e.First().addnlcharge,
                addnlchargeid = e.First().addnlchargeid,
                fgboxid = e.First().fgboxid,
                soboxid = e.First().soboxid,
                pricerange = e.First().pricerange,
                jumpqty = e.ToList().ConvertAll(x => new SalesorderitemsDetail.SizeQty()
                {
                    uid = x.uid,
                    name = x.sizename,
                    id = x.id,
                    oqty = x.oqty,
                    sizeid = x.sizeid
                })
            });
            foreach (var gitem in retList.ToList())
            {
                var jumpgroup = gitem.jumpqty.GroupBy(x => x.sizeid).ToList().FindAll(y => y.Count() > 1);
                if (jumpgroup.Count > 0)
                {
                    for (var i = 0; i < jumpgroup.Count; i++)
                    {
                        var jumpqtylist = gitem.jumpqty.FindAll(x => x.sizeid == jumpgroup[0].Key);
                        gitem.jumpqty.RemoveAll(x => x.sizeid == jumpgroup[0].Key);
                        foreach (var jitem in jumpqtylist)
                        {
                            var newitem = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesorderitemsDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem));
                            newitem.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                            newitem.jumpqty.Add(jitem);
                            newitem.oqty = jitem.oqty;
                            newitem.tqty = jitem.oqty;
                            retList.Add(newitem);
                        }
                    }
                    if (gitem.jumpqty.Count == 0)
                    {
                        retList.Remove(gitem);
                    }
                    else
                    {
                        gitem.oqty = gitem.jumpqty.Average(x => x.oqty);
                        gitem.tqty = gitem.jumpqty.Sum(x => x.oqty);
                    }
                }
            }
            return retList.OrderBy(x => x.designid).ThenBy(y => y.colourcode).ToList();
        }

        private string GetSizeGroup(List<string> sizelist)
        {
            List<string> list = WebAPI.Data.Utility.GetSizeList(sizelist);
            if (list == null || list.Count == 0) return "";
            list.RemoveAll(x => x != null && !sizelist.Contains(x));
            if (list.Count == 1) return list[0];
            return list.First() + "-" + list.Last();
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<SalesorderitemsDetail> request)
        {
            SalesorderitemsDataService SalesorderitemsData = new SalesorderitemsDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Salesorderitems result = await Task<Salesorderitems>.Run(() => SalesorderitemsData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Salesorderitems result = await Task<Salesorderitems>.Run(() => SalesorderitemsData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<SalesorderitemsDetail> request)
        {
            SalesorderitemsDataService SalesorderitemsData = new SalesorderitemsDataService();
            Salesorderitems result = await Task<Salesorderitems>.Run(() => SalesorderitemsData.Insert(request.Item));

            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<SalesorderitemsDetail> request)
        {
            SalesorderitemsDataService SalesorderitemsData = new SalesorderitemsDataService();
            Salesorderitems result = await Task<Salesorderitems>.Run(() => SalesorderitemsData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Salesorderitems> request)
        {
            SalesorderitemsDataService SalesorderitemsData = new SalesorderitemsDataService();
            bool result = await Task<bool>.Run(() => SalesorderitemsData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}