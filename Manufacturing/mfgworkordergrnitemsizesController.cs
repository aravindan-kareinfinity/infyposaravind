
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
                                [RoutePrefix("MfgworkordergrnitemsizesService")]
                                public class MfgworkordergrnitemsizesServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<mfgworkordergrnitemsizes>();
                                            result.Item = new mfgworkordergrnitemsizes();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkordergrnitemsizesCreteria> request)
                                        {
                                            MfgworkordergrnitemsizesDataService MfgworkordergrnitemsizesData = new MfgworkordergrnitemsizesDataService();
                                            List<mfgworkordergrnitemsizes> result = await Task<List<mfgworkordergrnitemsizes>>.Run(() => MfgworkordergrnitemsizesData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<mfgworkordergrnitemsizes> request)
                                        {
                                            MfgworkordergrnitemsizesDataService MfgworkordergrnitemsizesData = new MfgworkordergrnitemsizesDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                mfgworkordergrnitemsizes result = await Task<mfgworkordergrnitemsizes>.Run(() => MfgworkordergrnitemsizesData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                mfgworkordergrnitemsizes result = await Task<mfgworkordergrnitemsizes>.Run(() => MfgworkordergrnitemsizesData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<mfgworkordergrnitemsizes> request)
                                        {
                                            MfgworkordergrnitemsizesDataService MfgworkordergrnitemsizesData = new MfgworkordergrnitemsizesDataService();
                                            mfgworkordergrnitemsizes result = await Task<mfgworkordergrnitemsizes>.Run(() => MfgworkordergrnitemsizesData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<mfgworkordergrnitemsizes> request)
                                        {
                                            MfgworkordergrnitemsizesDataService MfgworkordergrnitemsizesData = new MfgworkordergrnitemsizesDataService();
                                            mfgworkordergrnitemsizes result = await Task<mfgworkordergrnitemsizes>.Run(() => MfgworkordergrnitemsizesData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<mfgworkordergrnitemsizes> request)
                                        {
                                            MfgworkordergrnitemsizesDataService MfgworkordergrnitemsizesData = new MfgworkordergrnitemsizesDataService();
                                            bool result = await Task<bool>.Run(() => MfgworkordergrnitemsizesData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }