
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
                                [RoutePrefix("MfgworkorderitemsizesService")]
                                public class MfgworkorderitemsizesServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgworkorderitemsizes>();
                                            result.Item = new Mfgworkorderitemsizes();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkorderitemsizesCreteria> request)
                                        {
                                            MfgworkorderitemsizesDataService MfgworkorderitemsizesData = new MfgworkorderitemsizesDataService();
                                            List<Mfgworkorderitemsizes> result = await Task<List<Mfgworkorderitemsizes>>.Run(() => MfgworkorderitemsizesData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgworkorderitemsizes> request)
                                        {
                                            MfgworkorderitemsizesDataService MfgworkorderitemsizesData = new MfgworkorderitemsizesDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgworkorderitemsizes result = await Task<Mfgworkorderitemsizes>.Run(() => MfgworkorderitemsizesData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgworkorderitemsizes result = await Task<Mfgworkorderitemsizes>.Run(() => MfgworkorderitemsizesData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgworkorderitemsizes> request)
                                        {
                                            MfgworkorderitemsizesDataService MfgworkorderitemsizesData = new MfgworkorderitemsizesDataService();
                                            Mfgworkorderitemsizes result = await Task<Mfgworkorderitemsizes>.Run(() => MfgworkorderitemsizesData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkorderitemsizes> request)
                                        {
                                            MfgworkorderitemsizesDataService MfgworkorderitemsizesData = new MfgworkorderitemsizesDataService();
                                            Mfgworkorderitemsizes result = await Task<Mfgworkorderitemsizes>.Run(() => MfgworkorderitemsizesData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkorderitemsizes> request)
                                        {
                                            MfgworkorderitemsizesDataService MfgworkorderitemsizesData = new MfgworkorderitemsizesDataService();
                                            bool result = await Task<bool>.Run(() => MfgworkorderitemsizesData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }