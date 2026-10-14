
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
                                [RoutePrefix("SalablegoodspriceService")]
                                public class SalablegoodspriceServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Salablegoodsprice>();
                                            result.Item = new Salablegoodsprice();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<SalablegoodspriceCreteria> request)
                                        {
                                            SalablegoodspriceDataService SalablegoodspriceData = new SalablegoodspriceDataService();
                                            List<Salablegoodsprice> result = await Task<List<Salablegoodsprice>>.Run(() => SalablegoodspriceData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Salablegoodsprice> request)
                                        {
                                            SalablegoodspriceDataService SalablegoodspriceData = new SalablegoodspriceDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Salablegoodsprice result = await Task<Salablegoodsprice>.Run(() => SalablegoodspriceData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Salablegoodsprice result = await Task<Salablegoodsprice>.Run(() => SalablegoodspriceData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Salablegoodsprice> request)
                                        {
                                            SalablegoodspriceDataService SalablegoodspriceData = new SalablegoodspriceDataService();
                                            Salablegoodsprice result = await Task<Salablegoodsprice>.Run(() => SalablegoodspriceData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Salablegoodsprice> request)
                                        {
                                            SalablegoodspriceDataService SalablegoodspriceData = new SalablegoodspriceDataService();
                                            Salablegoodsprice result = await Task<Salablegoodsprice>.Run(() => SalablegoodspriceData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Salablegoodsprice> request)
                                        {
                                            SalablegoodspriceDataService SalablegoodspriceData = new SalablegoodspriceDataService();
                                            bool result = await Task<bool>.Run(() => SalablegoodspriceData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }