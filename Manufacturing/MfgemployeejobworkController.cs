
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
                                [RoutePrefix("MfgemployeejobworkService")]
                                public class MfgemployeejobworkServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgemployeejobwork>();
                                            result.Item = new Mfgemployeejobwork();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgemployeejobworkCreteria> request)
                                        {
                                            MfgemployeejobworkDataService MfgemployeejobworkData = new MfgemployeejobworkDataService();
                                            List<Mfgemployeejobwork> result = await Task<List<Mfgemployeejobwork>>.Run(() => MfgemployeejobworkData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgemployeejobwork> request)
                                        {
                                            MfgemployeejobworkDataService MfgemployeejobworkData = new MfgemployeejobworkDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgemployeejobwork result = await Task<Mfgemployeejobwork>.Run(() => MfgemployeejobworkData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgemployeejobwork result = await Task<Mfgemployeejobwork>.Run(() => MfgemployeejobworkData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgemployeejobwork> request)
                                        {
                                            MfgemployeejobworkDataService MfgemployeejobworkData = new MfgemployeejobworkDataService();
                                            Mfgemployeejobwork result = await Task<Mfgemployeejobwork>.Run(() => MfgemployeejobworkData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgemployeejobwork> request)
                                        {
                                            MfgemployeejobworkDataService MfgemployeejobworkData = new MfgemployeejobworkDataService();
                                            Mfgemployeejobwork result = await Task<Mfgemployeejobwork>.Run(() => MfgemployeejobworkData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgemployeejobwork> request)
                                        {
                                            MfgemployeejobworkDataService MfgemployeejobworkData = new MfgemployeejobworkDataService();
                                            bool result = await Task<bool>.Run(() => MfgemployeejobworkData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }