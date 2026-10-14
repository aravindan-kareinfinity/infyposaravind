
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
                                [RoutePrefix("MfgbinstorageService")]
                                public class MfgbinstorageServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgbinstorage>();
                                            result.Item = new Mfgbinstorage();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgbinstorageCreteria> request)
                                        {
                                            MfgbinstorageDataService MfgbinstorageData = new MfgbinstorageDataService();
                                            List<Mfgbinstorage> result = await Task<List<Mfgbinstorage>>.Run(() => MfgbinstorageData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgbinstorage> request)
                                        {
                                            MfgbinstorageDataService MfgbinstorageData = new MfgbinstorageDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgbinstorage result = await Task<Mfgbinstorage>.Run(() => MfgbinstorageData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgbinstorage result = await Task<Mfgbinstorage>.Run(() => MfgbinstorageData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgbinstorage> request)
                                        {
                                            MfgbinstorageDataService MfgbinstorageData = new MfgbinstorageDataService();
                                            Mfgbinstorage result = await Task<Mfgbinstorage>.Run(() => MfgbinstorageData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgbinstorage> request)
                                        {
                                            MfgbinstorageDataService MfgbinstorageData = new MfgbinstorageDataService();
                                            Mfgbinstorage result = await Task<Mfgbinstorage>.Run(() => MfgbinstorageData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgbinstorage> request)
                                        {
                                            MfgbinstorageDataService MfgbinstorageData = new MfgbinstorageDataService();
                                            bool result = await Task<bool>.Run(() => MfgbinstorageData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }