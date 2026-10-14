
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
                                [RoutePrefix("MfgbillofmaterialitemsService")]
                                public class MfgbillofmaterialitemsServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Mfgbillofmaterialitems>();
                                            result.Item = new Mfgbillofmaterialitems();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<MfgbillofmaterialitemsCreteria> request)
                                        {
                                            MfgbillofmaterialitemsDataService MfgbillofmaterialitemsData = new MfgbillofmaterialitemsDataService();
                                            List<Mfgbillofmaterialitems> result = await Task<List<Mfgbillofmaterialitems>>.Run(() => MfgbillofmaterialitemsData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Mfgbillofmaterialitems> request)
                                        {
                                            MfgbillofmaterialitemsDataService MfgbillofmaterialitemsData = new MfgbillofmaterialitemsDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Mfgbillofmaterialitems result = await Task<Mfgbillofmaterialitems>.Run(() => MfgbillofmaterialitemsData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Mfgbillofmaterialitems result = await Task<Mfgbillofmaterialitems>.Run(() => MfgbillofmaterialitemsData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgbillofmaterialitems> request)
                                        {
                                            MfgbillofmaterialitemsDataService MfgbillofmaterialitemsData = new MfgbillofmaterialitemsDataService();
                                            Mfgbillofmaterialitems result = await Task<Mfgbillofmaterialitems>.Run(() => MfgbillofmaterialitemsData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Mfgbillofmaterialitems> request)
                                        {
                                            MfgbillofmaterialitemsDataService MfgbillofmaterialitemsData = new MfgbillofmaterialitemsDataService();
                                            Mfgbillofmaterialitems result = await Task<Mfgbillofmaterialitems>.Run(() => MfgbillofmaterialitemsData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgbillofmaterialitems> request)
                                        {
                                            MfgbillofmaterialitemsDataService MfgbillofmaterialitemsData = new MfgbillofmaterialitemsDataService();
                                            bool result = await Task<bool>.Run(() => MfgbillofmaterialitemsData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }