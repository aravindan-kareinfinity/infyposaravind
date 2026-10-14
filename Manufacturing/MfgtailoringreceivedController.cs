
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
                                [RoutePrefix("MfgtailoringreceivedService")]
                                public class MfgtailoringreceivedServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgtailoringreceived>();
                                            result.Item = new Mfgtailoringreceived();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgtailoringreceivedCreteria> request)
                                        {
                                            MfgtailoringreceivedDataService MfgtailoringreceivedData = new MfgtailoringreceivedDataService();
                                            List<Mfgtailoringreceived> result = await Task<List<Mfgtailoringreceived>>.Run(() => MfgtailoringreceivedData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgtailoringreceived> request)
                                        {
                                            MfgtailoringreceivedDataService MfgtailoringreceivedData = new MfgtailoringreceivedDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgtailoringreceived result = await Task<Mfgtailoringreceived>.Run(() => MfgtailoringreceivedData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgtailoringreceived result = await Task<Mfgtailoringreceived>.Run(() => MfgtailoringreceivedData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgtailoringreceived> request)
                                        {
                                            MfgtailoringreceivedDataService MfgtailoringreceivedData = new MfgtailoringreceivedDataService();
                                            Mfgtailoringreceived result = await Task<Mfgtailoringreceived>.Run(() => MfgtailoringreceivedData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgtailoringreceived> request)
                                        {
                                            MfgtailoringreceivedDataService MfgtailoringreceivedData = new MfgtailoringreceivedDataService();
                                            Mfgtailoringreceived result = await Task<Mfgtailoringreceived>.Run(() => MfgtailoringreceivedData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgtailoringreceived> request)
                                        {
                                            MfgtailoringreceivedDataService MfgtailoringreceivedData = new MfgtailoringreceivedDataService();
                                            bool result = await Task<bool>.Run(() => MfgtailoringreceivedData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }