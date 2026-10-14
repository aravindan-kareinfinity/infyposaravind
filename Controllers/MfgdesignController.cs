
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
                                [RoutePrefix("MfgdesignService")]
                                public class MfgdesignServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgdesign>();
                                            result.Item = new Mfgdesign();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgdesignCreteria> request)
                                        {
                                            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
                                            List<Mfgdesign> result = await Task<List<Mfgdesign>>.Run(() => MfgdesignData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgdesign> request)
                                        {
                                            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgdesign> request)
                                        {
                                            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
                                            Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgdesign> request)
                                        {
                                            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
                                            Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgdesign> request)
                                        {
                                            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
                                            bool result = await Task<bool>.Run(() => MfgdesignData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }