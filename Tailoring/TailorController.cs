
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
                                [RoutePrefix("TailorService")]
                                public class TailorServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Tailor>();
                                            result.Item = new Tailor();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<TailorCreteria> request)
                                        {
                                            TailorDataService TailorData = new TailorDataService();
                                            List<Tailor> result = await Task<List<Tailor>>.Run(() => TailorData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Tailor> request)
                                        {
                                            TailorDataService TailorData = new TailorDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Tailor result = await Task<Tailor>.Run(() => TailorData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Tailor result = await Task<Tailor>.Run(() => TailorData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Tailor> request)
                                        {
                                            TailorDataService TailorData = new TailorDataService();
                                            Tailor result = await Task<Tailor>.Run(() => TailorData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Tailor> request)
                                        {
                                            TailorDataService TailorData = new TailorDataService();
                                            Tailor result = await Task<Tailor>.Run(() => TailorData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Tailor> request)
                                        {
                                            TailorDataService TailorData = new TailorDataService();
                                            bool result = await Task<bool>.Run(() => TailorData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }