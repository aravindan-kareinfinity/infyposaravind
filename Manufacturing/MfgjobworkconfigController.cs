
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
                                [RoutePrefix("MfgjobworkconfigService")]
                                public class MfgjobworkconfigServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgjobworkconfig>();
                                            result.Item = new Mfgjobworkconfig();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgjobworkconfigCreteria> request)
                                        {
                                            MfgjobworkconfigDataService MfgjobworkconfigData = new MfgjobworkconfigDataService();
                                            List<Mfgjobworkconfig> result = await Task<List<Mfgjobworkconfig>>.Run(() => MfgjobworkconfigData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgjobworkconfig> request)
                                        {
                                            MfgjobworkconfigDataService MfgjobworkconfigData = new MfgjobworkconfigDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgjobworkconfig result = await Task<Mfgjobworkconfig>.Run(() => MfgjobworkconfigData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgjobworkconfig result = await Task<Mfgjobworkconfig>.Run(() => MfgjobworkconfigData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgjobworkconfig> request)
                                        {
                                            MfgjobworkconfigDataService MfgjobworkconfigData = new MfgjobworkconfigDataService();
                                            Mfgjobworkconfig result = await Task<Mfgjobworkconfig>.Run(() => MfgjobworkconfigData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgjobworkconfig> request)
                                        {
                                            MfgjobworkconfigDataService MfgjobworkconfigData = new MfgjobworkconfigDataService();
                                            Mfgjobworkconfig result = await Task<Mfgjobworkconfig>.Run(() => MfgjobworkconfigData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgjobworkconfig> request)
                                        {
                                            MfgjobworkconfigDataService MfgjobworkconfigData = new MfgjobworkconfigDataService();
                                            bool result = await Task<bool>.Run(() => MfgjobworkconfigData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }