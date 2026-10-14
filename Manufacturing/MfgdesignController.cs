
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
using System.IO;
using System.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("MfgdesignService")]
    public class MfgdesignServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgdesign>();
            result.Item = new Mfgdesign();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("NextAutoNumber")]
        public async Task<IHttpActionResult> NextAutoNumber()
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            String result = await Task<String>.Run(() => MfgdesignData.NextAutoNumber());
            return this.Ok(result);
        }

        [HttpGet]
        [Route("CreateZeroStock/{id}")]
        public async Task<IHttpActionResult> CreateZeroStock(string id)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            String result = await Task<String>.Run(() => MfgdesignData.CreateZeroStock(long.Parse(id)));
            return this.Ok(result);
        }


        [Route("Export")]
        [HttpPost]
        public async Task<IHttpActionResult> Export(MfgdesignCreteria creteria)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            DataTable result = await Task<DataTable>.Run(() => MfgdesignData.Export(creteria.ids));
            using (System.IO.MemoryStream memoryStream = new MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(result, "SKU Desgins -" + DateTime.Now.ToString("dd-MM-yyyy hhmm tt"));
                xLWorkbook.SaveAs(memoryStream);
                return new FileActionResult(true, "SKUDesign-" + DateTime.Now.ToString("dd-MM-yyyy"),
                    "application/vnd.ms-excel", new ByteArrayContent(memoryStream.ToArray()));
            }
        }


        [HttpGet]
        [Route("MfgDesignCost/{id}")]
        public async Task<IHttpActionResult> MfgDesignCost(string id)
        {
            MfgdesigncostDataService MfgdesignData = new MfgdesigncostDataService();
            List<Mfgdesigncost> result = await Task<List<Mfgdesigncost>>.Run(() => MfgdesignData.SearchAll(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("MfgDesignCost")]
        public async Task<IHttpActionResult> MfgDesignCost(ActionMulitpleRequest<Mfgdesigncost> request)
        {
            MfgdesigncostDataService MfgdesignData = new MfgdesigncostDataService();
            List<Mfgdesigncost> result = await Task<List<Mfgdesigncost>>.Run(() => MfgdesignData.Save(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ImportSKU")]
        public async Task<IHttpActionResult> ImportSKU(ActionMulitpleRequest<MfgdesignProcess> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<MfgdesignProcess> result = await Task<List<MfgdesignProcess>>.Run(() => MfgdesignData.ImportSKU(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDesignList")]
        public async Task<IHttpActionResult> SearchDesignList(ActionRequest<MfgdesignCreteria> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<MfgdesignCode> result = await Task<List<MfgdesignCode>>.Run(() => MfgdesignData.SearchDesignList(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgdesignCreteria> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<Mfgdesign> result = await Task<List<Mfgdesign>>.Run(() => MfgdesignData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchUAN")]
        public async Task<IHttpActionResult> SearchUAN(ActionRequest<MfgdesignCreteria> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            if (request.Item.sizeid < 0)
            {
                List<Mfgdesign.Sizegroup.SearchItem> result = await Task<List<Mfgdesign.Sizegroup.SearchItem>>.Run(() => MfgdesignData.SearchUANList(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<Mfgdesign.Sizegroup.SearchItem> result = await Task<List<Mfgdesign.Sizegroup.SearchItem>>.Run(() => MfgdesignData.SearchUAN(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchCombo")]
        public async Task<IHttpActionResult> SearchCombo(ActionRequest<MfgdesignCreteria> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<Mfgdesign> result = await Task<List<Mfgdesign>>.Run(() => MfgdesignData.SearchCombo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeAtributes")]
        public async Task<IHttpActionResult> ChangeAtributes(ActionRequest<ChangeAttributes> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeAtributes(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("ChangeClass")]
        public async Task<IHttpActionResult> ChangeClass(ActionRequest<ChangeAttributes> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeClassid(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<ChangeAttributes> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeStatus(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("ChangePrice")]
        public async Task<IHttpActionResult> ChangePrice(ActionRequest<ChangeAttributes> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            int result = await Task<int>.Run(() => MfgdesignData.ChangePrice(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SetComboConfig")]
        public async Task<IHttpActionResult> SetComboConfig(ActionRequest<ComboConfig> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.SetComboConfig(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("GetComboConfig")]
        public async Task<IHttpActionResult> GetComboConfig(ActionRequest<ComboConfig> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            ComboConfig result = await Task<ComboConfig>.Run(() => MfgdesignData.GetComboConfig(request.Item));
            return this.Ok(result);
        }
        public class SearchCreteria : BaseEntity
        {
            public List<long> stocklocations { get; set; }
            public bool showall { get; set; }
            public string name { get; set; }
            public string designcode { get; set; }
            public string sizeset { get; set; }
            public string colourid { get; set; }
            public string sizeid { get; set; }
        }
        [HttpPost]
        [Route("SearchByName")]

        public async Task<IHttpActionResult> SearchByName(ActionRequest<SearchCreteria> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            MFGDesinInfo result = await Task<MFGDesinInfo>.Run(() =>
            {
                request.Item.name = request.Item.name.Replace("~", "/");

                var sizelist = MfgdesignData.SearchInfo(request.Item.name, request.Item.showall, request.Item.stocklocations);
                if (sizelist == null) return null;

                sizelist.auto = !string.IsNullOrEmpty(request.Item.sizeset) ||
                !string.IsNullOrEmpty(request.Item.colourid);

                if (!string.IsNullOrEmpty(request.Item.sizeset))
                {
                    sizelist.sizegroup.RemoveAll(e => e.name != request.Item.sizeset);
                    sizelist.sizegroupid = request.Item.sizeset;
                }
                else
                {
                    sizelist.sizegroupid = sizelist.sizegroup.First().id;
                }

                if (!string.IsNullOrEmpty(request.Item.colourid))
                {
                    long cid;
                    if (long.TryParse(request.Item.colourid, out cid))
                    {
                        sizelist.colourlist.RemoveAll(e => e.id != cid);
                        sizelist.colourid = cid;
                    }
                }
                else if (sizelist.sizegroup != null &&
                sizelist.sizegroup.Count > 0 &&
                sizelist.sizegroup[0].list != null &&
                sizelist.sizegroup[0].list.Count > 0)
                {
                    sizelist.colourid = sizelist.sizegroup.First().list[0].colourid;
                }

                if (!string.IsNullOrEmpty(request.Item.sizeid))
                {
                    long cid;
                    if (long.TryParse(request.Item.sizeid, out cid))
                    {
                        sizelist.sizeid = cid;
                    }
                }
                else if (sizelist.sizegroup != null &&
                        sizelist.sizegroup.Count > 0 &&
                        sizelist.sizegroup[0].list != null &&
                        sizelist.sizegroup[0].list.Count == 0)
                {
                    sizelist.sizeid = sizelist.sizegroup.First().list[0].id;
                }
                return sizelist;
            });

            if (result != null && result.sizegroup != null && result.sizegroup.Count > 1)
            {
                result.colourlist = result.colourlist.OrderBy(x =>
                {
                    int ccode = 0;
                    if (x.designcode.IndexOf('/') > 0)
                    {
                        int.TryParse(x.designcode.Substring(x.designcode.IndexOf('/') + 1), out ccode);
                    }
                    return ccode;
                }).ToList();

                result.sizegroup = result.sizegroup.OrderBy(x =>
                {
                    int ccode = 0;
                    int.TryParse(x.colourcode, out ccode);
                    return ccode;
                }).ToList();
            }
            return this.Ok(result);
            //return await SearchByName(request.Item.name, request.Item.sizeset,
            //    request.Item.colourid, request.Item.sizeid);
        }
        //[HttpPost]
        //[Route("SearchUAN")]
        //public async Task<IHttpActionResult> SearchUAN(ActionRequest<Mfgdesign.Sizegroup.SearchItem> request)
        //{
        //    MfgdesignDataService MfgdesignData = new MfgdesignDataService();
        //    List<Mfgdesign.Sizegroup.SearchItem> result = await Task<List<Mfgdesign.Sizegroup.SearchItem>>.Run(() => MfgdesignData.SearchUANItem(request.Item));
        //    return this.Ok(result);
        //}
        [HttpPost]
        [Route("SearchUANByName")]
        public async Task<IHttpActionResult> SearchUANByName(ActionRequest<SearchCreteria> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            Mfgdesign.Sizegroup.Item result = await Task<Mfgdesign.Sizegroup.Item>.Run(() => MfgdesignData.SearchUANByName(request.Item.name,
                request.Item.designcode,
                request.Item.sizeid));
            return this.Ok(result);
        }
        [HttpGet]
        [Route("SearchByName/{name}/{sizeset?}/{colourid?}/{sizeid?}")]
        public async Task<IHttpActionResult> SearchByName(string name, string sizeset = null, string colourid = null, string sizeid = null)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            MFGDesinInfo result = await Task<MFGDesinInfo>.Run(() =>
            {
                name = name.Replace("~", "/");

                var sizelist = MfgdesignData.SearchInfo(name, false, null);
                if (sizelist == null) return null;

                sizelist.auto = !string.IsNullOrEmpty(sizeset) || !string.IsNullOrEmpty(colourid);
                if (!string.IsNullOrEmpty(sizeset))
                {
                    sizelist.sizegroup.RemoveAll(e => e.name != sizeset);
                    sizelist.sizegroupid = sizeset;
                }
                else
                {
                    sizelist.sizegroupid = sizelist.sizegroup.First().id;
                }

                if (!string.IsNullOrEmpty(colourid))
                {
                    long cid;
                    if (long.TryParse(colourid, out cid))
                    {
                        sizelist.colourlist.RemoveAll(e => e.id != cid);
                        sizelist.colourid = cid;
                    }
                }
                else if (sizelist.sizegroup != null &&
                sizelist.sizegroup.Count > 0 &&
                sizelist.sizegroup[0].list != null &&
                sizelist.sizegroup[0].list.Count > 0)
                {
                    sizelist.colourid = sizelist.sizegroup.First().list[0].colourid;
                }

                if (!string.IsNullOrEmpty(sizeid))
                {
                    long cid;
                    if (long.TryParse(sizeid, out cid))
                    {
                        sizelist.sizeid = cid;
                    }
                }
                else if (sizelist.sizegroup != null &&
sizelist.sizegroup.Count > 0 &&
sizelist.sizegroup[0].list != null &&
sizelist.sizegroup[0].list.Count == 0)
                {
                    sizelist.sizeid = sizelist.sizegroup.First().list[0].id;
                }
                return sizelist;
            });

            if (result != null && result.sizegroup != null && result.sizegroup.Count > 1)
            {
                result.colourlist = result.colourlist.OrderBy(x =>
                {
                    int ccode = 0;
                    if (x.designcode.IndexOf('/') > 0)
                    {
                        int.TryParse(x.designcode.Substring(x.designcode.IndexOf('/') + 1), out ccode);
                    }
                    return ccode;
                }).ToList();

                result.sizegroup = result.sizegroup.OrderBy(x =>
                {
                    int ccode = 0;
                    int.TryParse(x.colourcode, out ccode);
                    return ccode;
                }).ToList();
            }
            return this.Ok(result);
        }


        [HttpGet]
        [Route("SearchAll/{productid?}")]
        public async Task<IHttpActionResult> SearchAll(string productid=null)
        {
            long prodid = 0;
            long.TryParse(productid, out prodid);
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<Mfgdesign> result = await Task<List<Mfgdesign>>.Run(() => MfgdesignData.Search(new MfgdesignCreteria()
            {
                includedetail = true,
                productid = prodid
            }));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Search/{id}")]
        public async Task<IHttpActionResult> Search(string id)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            Mfgdesign result = await Task<Mfgdesign>.Run(() =>
            MfgdesignData.SearchDetail(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchByFabric/{id}")]
        public async Task<IHttpActionResult> SearchByFabric(string id)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<Mfgdesign> result = await Task<List<Mfgdesign>>.Run(() => MfgdesignData.Search(new MfgdesignCreteria()
            {
                fabricid = long.Parse(id)
            }));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchByFabricDesign/{id}")]
        public async Task<IHttpActionResult> SearchByFabricDesign(string id, string minimul = null)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            List<MfgdesignCode> result = await Task<List<MfgdesignCode>>.Run(() => MfgdesignData.SearchList(new MfgdesignCreteria()
            {
                fabricid = long.Parse(id)
            }));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgdesign> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            if (request.Item.id <= 0)
            {
                Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Update(request.Item, null));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgdesign> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgdesign> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            Mfgdesign result = await Task<Mfgdesign>.Run(() => MfgdesignData.Update(request.Item, null));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgdesign> request)
        {
            MfgdesignDataService MfgdesignData = new MfgdesignDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.Delete(request.Item));
            return this.Ok(result);
        }

        public class PrintDesignBarcode : BaseEntity
        {
            public string companytitle { get; set; }
            public List<Mfgdesign.Sizegroup.PrintItem> list { get; set; }
            public long designid { get; set; }
            public long reportid { get; set; }
            public bool disable64 { get; set; }
            public Mfgdesign design { get; set; }
        }

        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<PrintDesignBarcode> request)
        {

            return this.Ok(PrintBarcodeDirect(request));

        }

        public List<string> PrintBarcodeDirect(ActionRequest<PrintDesignBarcode> request)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0)
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                    reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                ProductsDataService productsDataService = new ProductsDataService();
                BrandDataService brandDataService = new BrandDataService();

                var colournames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "COLOUR"
                });

                var sizenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SIZE"
                });

                var patternnames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "PATTERN"
                });


                var materialnames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "MATERIAL"
                });

                var typenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "TYPE"
                });

                var stylenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "STYLE"
                });

                var sleevelist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SLEEVE"
                });

                var fitlist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "FIT"
                });

                List<string> base64Response = new List<string>();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                System.Text.StringBuilder asb = new System.Text.StringBuilder();
                var barcodeList = new List<Design2Print>();
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                var design = request.Item.design;
                if (design == null && request.Item.designid > 0)
                    design = mfgdesignDataService.Search(new MfgdesignCreteria()
                    {
                        id = request.Item.designid
                    }).First();
                if (design != null)
                {
                    var productnames = productsDataService.Search(new ProductsCreteria()
                    {
                        id = design.productid
                    });
                    var brandnames = brandDataService.Search(new BrandCreteria()
                    {
                        id = design.brandid
                    });
                    foreach (var item in request.Item.list)
                    {
                        if (item.pqty == 0) item.pqty = 1;
                        string colourcode = "";
                        if (item.name.IndexOf('/') > 0)
                            colourcode = item.code.Substring(item.code.IndexOf('/') + 1);

                        for (int i = 0; i < item.pqty; i++)
                        {
                            Design2Print design2Print = new Design2Print();
                            design2Print.brand = brandnames.Count > 0 ? brandnames[0].name : "";
                            design2Print.product = productnames.Count > 0 ? productnames[0].name : "";
                            design2Print.colour = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).name : "";
                            design2Print.colourcode = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).code : "";
                            design2Print.size = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).name : "";
                            design2Print.sizecode = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).code : "";
                            design2Print.fitname = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).name : "";
                            design2Print.fitcode = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).code : "";

                            design2Print.sleevename = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).name : "";
                            design2Print.sleevecode = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).code : "";

                            design2Print.materialname = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).name : "";
                            design2Print.materialcode = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).code : "";

                            design2Print.typename = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).name : "";
                            design2Print.typecode = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).code : "";

                            design2Print.patternname = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).name : "";
                            design2Print.patterncode = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).code : "";

                            design2Print.stylename = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).name : "";
                            design2Print.stylecode = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).code : "";


                            design2Print.colourshade = colourcode;
                            design2Print.design = design.name;
                            design2Print.designcode = item.code;
                            design2Print.id = design.id;
                            design2Print.uid = item.uid;
                            design2Print.name = item.code;
                            design2Print.colourid = item.colourid;
                            design2Print.sizeid = item.sizeid;
                            design2Print.sizegroup = item.sizegroup;
                            design2Print.salerate = item.salerate;
                            design2Print.mrp = item.mrp;
                            design2Print.wsrate = item.wsrate;
                            design2Print.dealerprice = item.dealerprice;
                            barcodeList.Add(design2Print);
                        }
                    }
                }
                else
                {
                    var alluids = request.Item.list.FindAll(x => x.designid == 0).ConvertAll(x => x.uid).ToList();
                    if (alluids.Count > 0)
                    {
                        var uanlist = mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
                        {
                            uids = alluids
                        });


                        var missinguids = alluids.FindAll(x => !uanlist.Exists(y => y.uid == x));
                        if (missinguids.Count > 0)
                        {
                            SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService();
                            var alternatebarcodes = sellablebarcodesDataService.Search(new SellablebarcodesCreteria()
                            {
                                ids = missinguids
                            });
                            request.Item.list.ForEach(x =>
                            {
                                if (alternatebarcodes.Exists(y => y.barcode == x.uid))
                                {
                                    x.uid = alternatebarcodes.Find(y => y.barcode == x.uid).corebarcode;
                                }
                            });
                            uanlist.AddRange(mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
                            {
                                uids = alternatebarcodes.ConvertAll(x => x.corebarcode)
                            }));
                        }

                        missinguids = alluids.FindAll(x => !uanlist.Exists(y => y.uid == x));
                        if (missinguids.Count > 0)
                        {
                            WebAPI.Common.Logger.Instance(productsDataService.Context.OrganizationID).Log("UAN Not available for barcode print", WebAPI.Common.Logger.LogType.Info, missinguids);
                        }
                        var productnames = productsDataService.Search(new ProductsCreteria()
                        {
                            ids = uanlist.ConvertAll(x => x.productid).ToList()
                        });
                        var brandnames = brandDataService.Search(new BrandCreteria()
                        {
                            ids = uanlist.ConvertAll(x => x.brandid).ToList()
                        });

                        request.Item.list.ForEach(pitem =>
                        {


                            if (pitem.pqty == 0) return;
                            string colourcode = "";

                            var idx = uanlist.FindIndex(x => x.uid == pitem.uid);
                            if (idx < 0) return;
                            var item = uanlist[idx];

                            if (item.name.IndexOf('/') > 0)
                                colourcode = item.name.Substring(item.name.LastIndexOf('/') + 1);

                            for (int i = 0; i < pitem.pqty; i++)
                            {
                                Design2Print design2Print = new Design2Print();
                                design2Print.brand = brandnames.Exists(e => e.id == item.brandid) ? brandnames.Find(e => e.id == item.brandid).name : "";
                                design2Print.product = productnames.Exists(e => e.id == item.productid) ? productnames.Find(e => e.id == item.productid).name : "";
                                design2Print.colour = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).name : "";
                                design2Print.colourcode = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).code : "";
                                design2Print.size = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).name : "";
                                design2Print.sizecode = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).code : "";
                                design2Print.fitname = fitlist.Exists(e => e.id == item.fitid) ? fitlist.Find(e => e.id == item.fitid).name : "";
                                design2Print.fitcode = fitlist.Exists(e => e.id == item.fitid) ? fitlist.Find(e => e.id == item.fitid).code : "";

                                design2Print.sleevename = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).name : "";
                                design2Print.sleevecode = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).code : "";

                                design2Print.materialname = materialnames.Exists(e => e.id == item.materialid) ? materialnames.Find(e => e.id == item.materialid).name : "";
                                design2Print.materialcode = materialnames.Exists(e => e.id == item.materialid) ? materialnames.Find(e => e.id == item.materialid).code : "";

                                design2Print.typename = typenames.Exists(e => e.id == item.typeid) ? typenames.Find(e => e.id == item.typeid).name : "";
                                design2Print.typecode = typenames.Exists(e => e.id == item.typeid) ? typenames.Find(e => e.id == item.typeid).code : "";

                                design2Print.patternname = patternnames.Exists(e => e.id == item.patternid) ? patternnames.Find(e => e.id == item.patternid).name : "";
                                design2Print.patterncode = patternnames.Exists(e => e.id == item.patternid) ? patternnames.Find(e => e.id == item.patternid).code : "";

                                design2Print.stylename = stylenames.Exists(e => e.id == item.styleid) ? stylenames.Find(e => e.id == item.styleid).name : "";
                                design2Print.stylecode = stylenames.Exists(e => e.id == item.styleid) ? stylenames.Find(e => e.id == item.styleid).code : "";

                                design2Print.colourshade = colourcode;
                                design2Print.design = item.code;
                                design2Print.designcode = item.code;
                                design2Print.uid = item.uid;
                                design2Print.name = item.name;
                                design2Print.colourid = item.colourid;
                                design2Print.sizeid = item.sizeid;
                                design2Print.sizegroup = item.sizegroup;
                                design2Print.salerate = item.salerate;
                                design2Print.mrp = item.mrp;
                                design2Print.wsrate = item.wsrate;
                                design2Print.dealerprice = item.dealerprice;

                                if(pitem.mrp != 0)
                                    design2Print.mrp = pitem.mrp;
                                barcodeList.Add(design2Print);
                            }


                        });
                    }
                    else
                    {
                        request.Item.list.RemoveAll(x => x.designid <= 0);
                        foreach (var gitem in request.Item.list.GroupBy(x => x.designid))
                        {
                            design = mfgdesignDataService.Search(new MfgdesignCreteria()
                            {
                                id = gitem.Key,
                                includedetail = true
                            }).First();

                            var productnames = productsDataService.Search(new ProductsCreteria()
                            {
                                id = design.productid
                            });
                            var brandnames = brandDataService.Search(new BrandCreteria()
                            {
                                id = design.brandid
                            });
                            foreach (var pitem in gitem)
                            {
                                if (pitem.pqty == 0) continue;
                                string colourcode = "";

                                var idx = design.sizegroup.items.FindIndex(x => x.uid == pitem.uid);
                                if (idx < 0) continue;
                                var item = design.sizegroup.items[idx];

                                if (item.name.IndexOf('/') > 0)
                                    colourcode = item.name.Substring(item.name.LastIndexOf('/') + 1);

                                for (int i = 0; i < pitem.pqty; i++)
                                {
                                    Design2Print design2Print = new Design2Print();
                                    design2Print.brand = brandnames.Count > 0 ? brandnames[0].name : "";
                                    design2Print.product = productnames.Count > 0 ? productnames[0].name : "";
                                    design2Print.colour = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).name : "";
                                    design2Print.colourcode = colournames.Exists(e => e.id == item.colourid) ? colournames.Find(e => e.id == item.colourid).code : "";
                                    design2Print.size = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).name : "";
                                    design2Print.sizecode = sizenames.Exists(e => e.id == item.sizeid) ? sizenames.Find(e => e.id == item.sizeid).code : "";
                                    design2Print.fitname = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).name : "";
                                    design2Print.fitcode = fitlist.Exists(e => e.id == design.fitid) ? fitlist.Find(e => e.id == design.fitid).code : "";

                                    design2Print.sleevename = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).name : "";
                                    design2Print.sleevecode = sleevelist.Exists(e => e.id == item.sleeveid) ? sleevelist.Find(e => e.id == item.sleeveid).code : "";

                                    design2Print.materialname = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).name : "";
                                    design2Print.materialcode = materialnames.Exists(e => e.id == design.materialid) ? materialnames.Find(e => e.id == design.materialid).code : "";

                                    design2Print.typename = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).name : "";
                                    design2Print.typecode = typenames.Exists(e => e.id == design.typeid) ? typenames.Find(e => e.id == design.typeid).code : "";

                                    design2Print.patternname = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).name : "";
                                    design2Print.patterncode = patternnames.Exists(e => e.id == design.patternid) ? patternnames.Find(e => e.id == design.patternid).code : "";

                                    design2Print.stylename = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).name : "";
                                    design2Print.stylecode = stylenames.Exists(e => e.id == design.styleid) ? stylenames.Find(e => e.id == design.styleid).code : "";

                                    design2Print.colourshade = colourcode;
                                    design2Print.design = design.name;
                                    design2Print.designcode = item.code;
                                    design2Print.id = design.id;
                                    design2Print.uid = item.uid;
                                    design2Print.name = item.code;
                                    design2Print.colourid = item.colourid;
                                    design2Print.sizeid = item.sizeid;
                                    design2Print.sizegroup = item.sizegroup;
                                    design2Print.salerate = item.salerate;
                                    design2Print.mrp = item.mrp;
                                    design2Print.wsrate = item.wsrate;
                                    design2Print.dealerprice = item.dealerprice;
                                    barcodeList.Add(design2Print);
                                }
                            }
                        }
                    }
                }

                barcodeList = barcodeList.OrderBy(x => x.designcode).ThenBy(y => y.sizecode).ToList();
                var response = barcodeprinter.Render(barcodeList.ConvertAll(x => (Report.IBarcodeFormat)x));
                foreach (var item in response)
                {
                    asb.AppendLine(item);
                    asb.Append(";");
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                    //base64Response.Add(Convert.ToBase64String(bytes));
                }

                if (request.Item.disable64)
                    base64Response.Add(asb.ToString());
                else
                    base64Response.Add(sb.ToString());
                return base64Response;
            }

            return new List<string>();
        }
    }
}