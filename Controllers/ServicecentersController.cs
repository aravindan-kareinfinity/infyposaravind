
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
                                [RoutePrefix("ServicecentersService")]
                                public class ServicecentersServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Servicecenters>();
                                            result.Item = new Servicecenters();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<ServicecentersCreteria> request)
                                        {
                                            ServicecentersDataService ServicecentersData = new ServicecentersDataService();
                                            List<Servicecenters> result = await Task<List<Servicecenters>>.Run(() => ServicecentersData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Servicecenters> request)
                                        {
                                            ServicecentersDataService ServicecentersData = new ServicecentersDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Servicecenters result = await Task<Servicecenters>.Run(() => ServicecentersData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Servicecenters result = await Task<Servicecenters>.Run(() => ServicecentersData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Servicecenters> request)
                                        {
                                            ServicecentersDataService ServicecentersData = new ServicecentersDataService();
                                            Servicecenters result = await Task<Servicecenters>.Run(() => ServicecentersData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Servicecenters> request)
                                        {
                                            ServicecentersDataService ServicecentersData = new ServicecentersDataService();
                                            Servicecenters result = await Task<Servicecenters>.Run(() => ServicecentersData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Servicecenters> request)
                                        {
                                            ServicecentersDataService ServicecentersData = new ServicecentersDataService();
                                            bool result = await Task<bool>.Run(() => ServicecentersData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }