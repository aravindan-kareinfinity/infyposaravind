
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
                                [RoutePrefix("BillcreditnoteitemsService")]
                                public class BillcreditnoteitemsServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Billcreditnoteitems>();
                                            result.Item = new Billcreditnoteitems();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<BillcreditnoteitemsCreteria> request)
                                        {
                                            BillcreditnoteitemsDataService BillcreditnoteitemsData = new BillcreditnoteitemsDataService();
                                            List<Billcreditnoteitems> result = await Task<List<Billcreditnoteitems>>.Run(() => BillcreditnoteitemsData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        //[HttpPost]  
                                        //[Route("Save")]
                                        //public async Task<IHttpActionResult> Save(ActionRequest<Billcreditnoteitems> request)
                                        //{
                                        //    BillcreditnoteitemsDataService BillcreditnoteitemsData = new BillcreditnoteitemsDataService();
                                        //    if (request.Item.id <= 0)
                                        //    {
                                        //        Billcreditnoteitems result = await Task<Billcreditnoteitems>.Run(() => BillcreditnoteitemsData.Insert(request.Item));
                                        //        return this.Ok(result);
                                        //    }
                                        //    else
                                        //    {
                                        //        Billcreditnoteitems result = await Task<Billcreditnoteitems>.Run(() => BillcreditnoteitemsData.Update(request.Item));
                                        //        return this.Ok(result);
                                        //    }
                                        //}

                                        //[HttpPost]
                                        //[Route("Insert")]
                                        //public async Task<IHttpActionResult> Insert(ActionRequest<Billcreditnoteitems> request)
                                        //{
                                        //    BillcreditnoteitemsDataService BillcreditnoteitemsData = new BillcreditnoteitemsDataService();
                                        //    Billcreditnoteitems result = await Task<Billcreditnoteitems>.Run(() => BillcreditnoteitemsData.Insert(request.Item));
                                        //    return this.Ok(result);
                                        //}

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Billcreditnoteitems> request)
                                        {
                                            BillcreditnoteitemsDataService BillcreditnoteitemsData = new BillcreditnoteitemsDataService();
                                            Billcreditnoteitems result = await Task<Billcreditnoteitems>.Run(() => BillcreditnoteitemsData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Billcreditnoteitems> request)
                                        {
                                            BillcreditnoteitemsDataService BillcreditnoteitemsData = new BillcreditnoteitemsDataService();
                                            bool result = await Task<bool>.Run(() => BillcreditnoteitemsData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }