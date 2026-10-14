
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
                                [RoutePrefix("TailoringmeasurementService")]
                                public class TailoringmeasurementServiceController : ApiController
                                    {

                                        [HttpGet]
                                        [Route("Entity")]
                                        public async Task<IHttpActionResult> Entity()
                                        {
                                            var result = new ActionRequest<Tailoringmeasurement>();
                                            result.Item = new Tailoringmeasurement();
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Search")]
                                        public async Task<IHttpActionResult> Search(ActionRequest<TailoringmeasurementCreteria> request)
                                        {
                                            TailoringmeasurementDataService TailoringmeasurementData = new TailoringmeasurementDataService();
                                            List<Tailoringmeasurement> result = await Task<List<Tailoringmeasurement>>.Run(() => TailoringmeasurementData.Search(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]  
                                        [Route("Save")]
                                        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringmeasurement> request)
                                        {
                                            TailoringmeasurementDataService TailoringmeasurementData = new TailoringmeasurementDataService();
                                            if (request.Item.id <= 0)
                                            {
                                                Tailoringmeasurement result = await Task<Tailoringmeasurement>.Run(() => TailoringmeasurementData.Insert(request.Item));
                                                return this.Ok(result);
                                            }
                                            else
                                            {
                                                Tailoringmeasurement result = await Task<Tailoringmeasurement>.Run(() => TailoringmeasurementData.Update(request.Item));
                                                return this.Ok(result);
                                            }
                                        }

                                        [HttpPost]
                                        [Route("Insert")]
                                        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringmeasurement> request)
                                        {
                                            TailoringmeasurementDataService TailoringmeasurementData = new TailoringmeasurementDataService();
                                            Tailoringmeasurement result = await Task<Tailoringmeasurement>.Run(() => TailoringmeasurementData.Insert(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Update")]
                                        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringmeasurement> request)
                                        {
                                            TailoringmeasurementDataService TailoringmeasurementData = new TailoringmeasurementDataService();
                                            Tailoringmeasurement result = await Task<Tailoringmeasurement>.Run(() => TailoringmeasurementData.Update(request.Item));
                                            return this.Ok(result);
                                        }

                                        [HttpPost]
                                        [Route("Delete")]
                                        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringmeasurement> request)
                                        {
                                            TailoringmeasurementDataService TailoringmeasurementData = new TailoringmeasurementDataService();
                                            bool result = await Task<bool>.Run(() => TailoringmeasurementData.Delete(request.Item));
                                            return this.Ok(result);
                                        }
                                    }
                                }