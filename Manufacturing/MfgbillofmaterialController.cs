
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
                                [RoutePrefix("MfgbillofmaterialService")]
                                public class MfgbillofmaterialServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgbillofmaterial>();
                                            result.Item = new Mfgbillofmaterial();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgbillofmaterialCreteria> request)
                                        {
                                            MfgbillofmaterialDataService MfgbillofmaterialData = new MfgbillofmaterialDataService();
                                            List<Mfgbillofmaterial> result = await Task<List<Mfgbillofmaterial>>.Run(() => MfgbillofmaterialData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgbillofmaterial> request)
                                        {
                                            MfgbillofmaterialDataService MfgbillofmaterialData = new MfgbillofmaterialDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgbillofmaterial result = await Task<Mfgbillofmaterial>.Run(() => MfgbillofmaterialData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgbillofmaterial result = await Task<Mfgbillofmaterial>.Run(() => MfgbillofmaterialData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgbillofmaterial> request)
                                        {
                                            MfgbillofmaterialDataService MfgbillofmaterialData = new MfgbillofmaterialDataService();
                                            Mfgbillofmaterial result = await Task<Mfgbillofmaterial>.Run(() => MfgbillofmaterialData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgbillofmaterial> request)
                                        {
                                            MfgbillofmaterialDataService MfgbillofmaterialData = new MfgbillofmaterialDataService();
                                            Mfgbillofmaterial result = await Task<Mfgbillofmaterial>.Run(() => MfgbillofmaterialData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgbillofmaterial> request)
                                        {
                                            MfgbillofmaterialDataService MfgbillofmaterialData = new MfgbillofmaterialDataService();
                                            bool result = await Task<bool>.Run(() => MfgbillofmaterialData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }