
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
                                [RoutePrefix("BillhistoryitemsService")]
                                public class BillhistoryitemsServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Billhistoryitems>();
                                            result.Item = new Billhistoryitems();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<BillhistoryitemsCreteria> request)
                                        {
                                            BillhistoryitemsDataService BillhistoryitemsData = new BillhistoryitemsDataService();
                                            List<Billhistoryitems> result = await Task<List<Billhistoryitems>>.Run(() => BillhistoryitemsData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Billhistoryitems> request)
                                        {
                                            BillhistoryitemsDataService BillhistoryitemsData = new BillhistoryitemsDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Billhistoryitems result = await Task<Billhistoryitems>.Run(() => BillhistoryitemsData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Billhistoryitems result = await Task<Billhistoryitems>.Run(() => BillhistoryitemsData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Billhistoryitems> request)
                                        {
                                            BillhistoryitemsDataService BillhistoryitemsData = new BillhistoryitemsDataService();
                                            Billhistoryitems result = await Task<Billhistoryitems>.Run(() => BillhistoryitemsData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Billhistoryitems> request)
                                        {
                                            BillhistoryitemsDataService BillhistoryitemsData = new BillhistoryitemsDataService();
                                            Billhistoryitems result = await Task<Billhistoryitems>.Run(() => BillhistoryitemsData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Billhistoryitems> request)
                                        {
                                            BillhistoryitemsDataService BillhistoryitemsData = new BillhistoryitemsDataService();
                                            bool result = await Task<bool>.Run(() => BillhistoryitemsData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }