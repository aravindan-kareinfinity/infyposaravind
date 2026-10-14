
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
                                [RoutePrefix("CustomerpricelistService")]
                                public class CustomerpricelistServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Customerpricelist>();
                                            result.Item = new Customerpricelist();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<CustomerpricelistCreteria> request)
                                        {
                                            CustomerpricelistDataService CustomerpricelistData = new CustomerpricelistDataService();
                                            List<Customerpricelist> result = await Task<List<Customerpricelist>>.Run(() => CustomerpricelistData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Customerpricelist> request)
                                        {
                                            CustomerpricelistDataService CustomerpricelistData = new CustomerpricelistDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Customerpricelist result = await Task<Customerpricelist>.Run(() => CustomerpricelistData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Customerpricelist result = await Task<Customerpricelist>.Run(() => CustomerpricelistData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Customerpricelist> request)
                                        {
                                            CustomerpricelistDataService CustomerpricelistData = new CustomerpricelistDataService();
                                            Customerpricelist result = await Task<Customerpricelist>.Run(() => CustomerpricelistData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Customerpricelist> request)
                                        {
                                            CustomerpricelistDataService CustomerpricelistData = new CustomerpricelistDataService();
                                            Customerpricelist result = await Task<Customerpricelist>.Run(() => CustomerpricelistData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Customerpricelist> request)
                                        {
                                            CustomerpricelistDataService CustomerpricelistData = new CustomerpricelistDataService();
                                            bool result = await Task<bool>.Run(() => CustomerpricelistData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }