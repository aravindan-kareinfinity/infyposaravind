
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
                                [RoutePrefix("MfgworkorderitemsService")]
                                public class MfgworkorderitemsServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgworkorderitems>();
                                            result.Item = new Mfgworkorderitems();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkorderitemsCreteria> request)
                                        {
                                            MfgworkorderitemsDataService MfgworkorderitemsData = new MfgworkorderitemsDataService();
                                            List<Mfgworkorderitems> result = await Task<List<Mfgworkorderitems>>.Run(() => MfgworkorderitemsData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                        

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkorderitems> request)
                                        {
                                            MfgworkorderitemsDataService MfgworkorderitemsData = new MfgworkorderitemsDataService();
                                            Mfgworkorderitems result = await Task<Mfgworkorderitems>.Run(() => MfgworkorderitemsData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkorderitems> request)
                                        {
                                            MfgworkorderitemsDataService MfgworkorderitemsData = new MfgworkorderitemsDataService();
                                            bool result = await Task<bool>.Run(() => MfgworkorderitemsData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }