
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
                                [RoutePrefix("BompricehistoryService")]
                                public class BompricehistoryServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Bompricehistory>();
                                            result.Item = new Bompricehistory();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<BompricehistoryCreteria> request)
                                        {
                                            BompricehistoryDataService BompricehistoryData = new BompricehistoryDataService();
                                            List<Bompricehistory> result = await Task<List<Bompricehistory>>.Run(() => BompricehistoryData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Bompricehistory> request)
                                        {
                                            BompricehistoryDataService BompricehistoryData = new BompricehistoryDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Bompricehistory result = await Task<Bompricehistory>.Run(() => BompricehistoryData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Bompricehistory result = await Task<Bompricehistory>.Run(() => BompricehistoryData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Bompricehistory> request)
                                        {
                                            BompricehistoryDataService BompricehistoryData = new BompricehistoryDataService();
                                            Bompricehistory result = await Task<Bompricehistory>.Run(() => BompricehistoryData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Bompricehistory> request)
                                        {
                                            BompricehistoryDataService BompricehistoryData = new BompricehistoryDataService();
                                            Bompricehistory result = await Task<Bompricehistory>.Run(() => BompricehistoryData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Bompricehistory> request)
                                        {
                                            BompricehistoryDataService BompricehistoryData = new BompricehistoryDataService();
                                            bool result = await Task<bool>.Run(() => BompricehistoryData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }