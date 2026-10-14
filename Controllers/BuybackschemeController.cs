
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
                                [RoutePrefix("BuybackschemeService")]
                                public class BuybackschemeServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Buybackscheme>();
                                            result.Item = new Buybackscheme();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<BuybackschemeCreteria> request)
                                        {
                                            BuybackschemeDataService BuybackschemeData = new BuybackschemeDataService();
                                            List<Buybackscheme> result = await Task<List<Buybackscheme>>.Run(() => BuybackschemeData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Buybackscheme> request)
                                        {
                                            BuybackschemeDataService BuybackschemeData = new BuybackschemeDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Buybackscheme result = await Task<Buybackscheme>.Run(() => BuybackschemeData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Buybackscheme result = await Task<Buybackscheme>.Run(() => BuybackschemeData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Buybackscheme> request)
                                        {
                                            BuybackschemeDataService BuybackschemeData = new BuybackschemeDataService();
                                            Buybackscheme result = await Task<Buybackscheme>.Run(() => BuybackschemeData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Buybackscheme> request)
                                        {
                                            BuybackschemeDataService BuybackschemeData = new BuybackschemeDataService();
                                            Buybackscheme result = await Task<Buybackscheme>.Run(() => BuybackschemeData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Buybackscheme> request)
                                        {
                                            BuybackschemeDataService BuybackschemeData = new BuybackschemeDataService();
                                            bool result = await Task<bool>.Run(() => BuybackschemeData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }