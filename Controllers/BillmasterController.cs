
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
    [RoutePrefix("BillmasterService")]
    public class BillmasterServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billmaster>();
            result.Item = new Billmaster();
            return this.Ok(result);
        }


        [HttpGet]
        [Route("LastBills")]
        public async Task<IHttpActionResult> LastBills()
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            List<BillInfo> result = await Task<List<BillInfo>>.Run(() => BillmasterData.SearchLastBills());
            return this.Ok(result);
        }


        [HttpGet]
        [Route("UnsettledBillCount/{locationid}/{floorid}")]
        public async Task<IHttpActionResult> BatchList(string locationid, string floorid)
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            long lid;
            long fid;
            long.TryParse(locationid, out lid);
            long.TryParse(floorid, out fid);
            int result = await Task<int>.Run(() => BillmasterData.UnsettledBillCount(lid,fid));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BillmasterCreteria> request)
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            List<Billmaster> result = await Task<List<Billmaster>>.Run(() => BillmasterData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Billmaster> request)
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Bills != null)
                {
                    foreach (var item in request.Item.Bills)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                        if(item.Billitems != null)
                            item.Billitems.ForEach(e => e.ChangeStatus(BaseEntity.DataStatus.Create));
                    }
                }
                Billmaster result = await Task<Billmaster>.Run(() => BillmasterData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Bills != null)
                {
                    foreach (var item in request.Item.Bills)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                        if (item.Billitems != null)
                            item.Billitems.ForEach(e => e.ChangeStatus(BaseEntity.DataStatus.Update));
                        item.qty = item.Billitems.Sum(e => e.qty);
                    }
                }
                Billmaster result = await Task<Billmaster>.Run(() => BillmasterData.Update(request.Item));
                return this.Ok(result);
            }
        }



        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billmaster> request)
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            Billmaster result = await Task<Billmaster>.Run(() => BillmasterData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Billmaster> request)
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            Billmaster result = await Task<Billmaster>.Run(() => BillmasterData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billmaster> request)
        {
            BillmasterDataService BillmasterData = new BillmasterDataService();
            bool result = await Task<bool>.Run(() => BillmasterData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}