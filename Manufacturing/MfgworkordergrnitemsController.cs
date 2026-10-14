
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
                                [RoutePrefix("MfgworkordergrnitemsService")]
                                public class MfgworkordergrnitemsServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgworkordergrnitems>();
                                            result.Item = new Mfgworkordergrnitems();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkordergrnitemsCreteria> request)
                                        {
                                            MfgworkordergrnitemsDataService MfgworkordergrnitemsData = new MfgworkordergrnitemsDataService();
                                            List<Mfgworkordergrnitems> result = await Task<List<Mfgworkordergrnitems>>.Run(() => MfgworkordergrnitemsData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgworkordergrnitems> request)
                                        {
                                            MfgworkordergrnitemsDataService MfgworkordergrnitemsData = new MfgworkordergrnitemsDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgworkordergrnitems result = await Task<Mfgworkordergrnitems>.Run(() => MfgworkordergrnitemsData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgworkordergrnitems result = await Task<Mfgworkordergrnitems>.Run(() => MfgworkordergrnitemsData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgworkordergrnitems> request)
                                        {
                                            MfgworkordergrnitemsDataService MfgworkordergrnitemsData = new MfgworkordergrnitemsDataService();
                                            Mfgworkordergrnitems result = await Task<Mfgworkordergrnitems>.Run(() => MfgworkordergrnitemsData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkordergrnitems> request)
                                        {
                                            MfgworkordergrnitemsDataService MfgworkordergrnitemsData = new MfgworkordergrnitemsDataService();
                                            Mfgworkordergrnitems result = await Task<Mfgworkordergrnitems>.Run(() => MfgworkordergrnitemsData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkordergrnitems> request)
                                        {
                                            MfgworkordergrnitemsDataService MfgworkordergrnitemsData = new MfgworkordergrnitemsDataService();
                                            bool result = await Task<bool>.Run(() => MfgworkordergrnitemsData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }