
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
                                [RoutePrefix("RechargeschemeService")]
                                public class RechargeschemeServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Rechargescheme>();
                                            result.Item = new Rechargescheme();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<RechargeschemeCreteria> request)
                                        {
                                            RechargeschemeDataService RechargeschemeData = new RechargeschemeDataService();
                                            List<Rechargescheme> result = await Task<List<Rechargescheme>>.Run(() => RechargeschemeData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Rechargescheme> request)
                                        {
                                            RechargeschemeDataService RechargeschemeData = new RechargeschemeDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Rechargescheme result = await Task<Rechargescheme>.Run(() => RechargeschemeData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Rechargescheme result = await Task<Rechargescheme>.Run(() => RechargeschemeData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Rechargescheme> request)
                                        {
                                            RechargeschemeDataService RechargeschemeData = new RechargeschemeDataService();
                                            Rechargescheme result = await Task<Rechargescheme>.Run(() => RechargeschemeData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Rechargescheme> request)
                                        {
                                            RechargeschemeDataService RechargeschemeData = new RechargeschemeDataService();
                                            Rechargescheme result = await Task<Rechargescheme>.Run(() => RechargeschemeData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Rechargescheme> request)
                                        {
                                            RechargeschemeDataService RechargeschemeData = new RechargeschemeDataService();
                                            bool result = await Task<bool>.Run(() => RechargeschemeData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }