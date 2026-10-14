
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
                                [RoutePrefix("MfgjoborderitemsService")]
                                public class MfgjoborderitemsServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgjoborderitems>();
                                            result.Item = new Mfgjoborderitems();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgjoborderitemsCreteria> request)
                                        {
                                            MfgjoborderitemsDataService MfgjoborderitemsData = new MfgjoborderitemsDataService();
                                            List<Mfgjoborderitems> result = await Task<List<Mfgjoborderitems>>.Run(() => MfgjoborderitemsData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgjoborderitems> request)
                                        {
                                            MfgjoborderitemsDataService MfgjoborderitemsData = new MfgjoborderitemsDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgjoborderitems result = await Task<Mfgjoborderitems>.Run(() => MfgjoborderitemsData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgjoborderitems result = await Task<Mfgjoborderitems>.Run(() => MfgjoborderitemsData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgjoborderitems> request)
                                        {
                                            MfgjoborderitemsDataService MfgjoborderitemsData = new MfgjoborderitemsDataService();
                                            Mfgjoborderitems result = await Task<Mfgjoborderitems>.Run(() => MfgjoborderitemsData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgjoborderitems> request)
                                        {
                                            MfgjoborderitemsDataService MfgjoborderitemsData = new MfgjoborderitemsDataService();
                                            Mfgjoborderitems result = await Task<Mfgjoborderitems>.Run(() => MfgjoborderitemsData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgjoborderitems> request)
                                        {
                                            MfgjoborderitemsDataService MfgjoborderitemsData = new MfgjoborderitemsDataService();
                                            bool result = await Task<bool>.Run(() => MfgjoborderitemsData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }