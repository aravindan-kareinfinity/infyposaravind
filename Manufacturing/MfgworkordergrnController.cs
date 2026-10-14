
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
using WebAPI.Data;
namespace InfyPOS.Controllers
{
    [RoutePrefix("MfgworkordergrnService")]
    public class MfgworkordergrnServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgworkordergrn>();
            result.Item = new Mfgworkordergrn();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkordergrnCreteria> request)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            List<Mfgworkordergrn> result = await Task<List<Mfgworkordergrn>>.Run(() =>
                {
                    
                    if (request.Item.workorderid > 0 && request.Item.creategrn)
                    {
                        List<Mfgworkordergrn> resultgrns = new List<Mfgworkordergrn>();
                        MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService();
                        var workorders = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                        {
                            id = request.Item.workorderid
                        });

                        var mfgworkordergrn = new Mfgworkordergrn(workorders[0]);
                        mfgworkordergrn.invoicedate = mfgworkorderDataService.DateTimeNow.Date;
                        mfgworkordergrn.lrdate = mfgworkorderDataService.DateTimeNow.Date;
                        resultgrns.Add(mfgworkordergrn);

                        MfgjoborderitemsDataService mfgworkorderitems = new MfgjoborderitemsDataService();
                        var items = mfgworkorderitems.SearchAll(0, request.Item.workorderid);
                        foreach(var item in items.FindAll(x=>x.inwardgoods && x.sourcejoborderitemid>0).GroupBy(x=>x.mfgworkorderitemsizesid))
                        {
                            var available = item.Sum(x => x.rqty);
                            foreach(var grnitem in mfgworkordergrn.Mfgworkordergrnitems)
                            {
                                if(grnitem.sizelist.Exists(x=>x.mfgworkorderitemsizesid == item.Key))
                                {
                                    grnitem.sizelist.Find(x => x.mfgworkorderitemsizesid == item.Key).aqty = available;
                                }
                            }

                        }

                        if (workorders[0].mode == "Internal")
                        {
                            foreach (var grnitem in mfgworkordergrn.Mfgworkordergrnitems)
                            {
                                grnitem.sizelist.RemoveAll(x => x.aqty == 0);
                            }
                        }
                        else
                        {
                            foreach (var grnitem in mfgworkordergrn.Mfgworkordergrnitems)
                            {
                                grnitem.sizelist.ForEach(x => x.aqty = x.eqty);
                            }
                        }
                        return resultgrns;
                    }
                    else
                    {
                        var resultgrns = MfgworkordergrnData.Search(request.Item);
                        return resultgrns;
                    }
                }
            );
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgworkordergrn> request)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            if (request.Item.id <= 0)
            {
                Mfgworkordergrn result = await Task<Mfgworkordergrn>.Run(() => MfgworkordergrnData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgworkordergrn result = await Task<Mfgworkordergrn>.Run(() => MfgworkordergrnData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgworkordergrn> request)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            Mfgworkordergrn result = await Task<Mfgworkordergrn>.Run(() => MfgworkordergrnData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkordergrn> request)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            Mfgworkordergrn result = await Task<Mfgworkordergrn>.Run(() => MfgworkordergrnData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkordergrn> request)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            bool result = await Task<bool>.Run(() => MfgworkordergrnData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            try
            {
                MfgworkordergrnData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                long invoiceid = long.Parse(id);

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {

                    var print = MfgworkordergrnData.Print(new MfgworkordergrnCreteria() { id = invoiceid });
                    return PrintWorkOrder(MfgworkordergrnData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(MfgworkordergrnData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        public FileContentInfo PrintWorkOrder(long organizationid, MfgworkordergrnDetail result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };

            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "WORKORDERGRN",
                organizationid = result.companyid
            });
            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "WORKORDERGRN"
                });
            }

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            List<mfgworkordergrnitemsizes> dataitems = new List<mfgworkordergrnitemsizes>();
            int sno = 1;
            result.Mfgworkordergrnitems.ForEach(x =>
            {
                if (x.sizelist != null && x.sizelist.Count > 0)
                {
                    x.sizelist.ForEach(y =>
                    {
                        y.parent = x;
                        y.sno = sno++;
                        dataitems.Add(y);
                    });
                }
            });

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = config.Context };
            
            var refids = new List<long>();
            refids.AddRange(dataitems.ConvertAll(x => x.colourid));
            refids.AddRange(dataitems.ConvertAll(x => x.sleeveid));
            refids.AddRange(dataitems.ConvertAll(x => x.sizeid));
            var referencelist = referencelistDataService.SearchList(new ReferencelistCreteria() { ids = refids.Distinct().ToList() });
            dataitems.ForEach(item =>
            {
                if (referencelist.Exists(x => x.id == item.colourid))
                    item.colourname = referencelist.Find(x => x.id == item.colourid).name;
                if (referencelist.Exists(x => x.id == item.sizeid))
                    item.sizename = referencelist.Find(x => x.id == item.sizeid).name;
                if (referencelist.Exists(x => x.id == item.sleeveid))
                    item.sleevename = referencelist.Find(x => x.id == item.sleeveid).name;
            });

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = dataitems;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    try
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                    catch (Exception exp)
                    { }
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "bominvoice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<Mfgworkordergrn.StatusChange> request)
        {
            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            List<Mfgworkordergrn.Attributes> result = await Task<List<Mfgworkordergrn.Attributes>>.Run(() => MfgworkordergrnData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<GRNPrint> request)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            MfgworkordergrnDataService MfgworkordergrnData = new MfgworkordergrnDataService();
            var barcodeDataList = MfgworkordergrnData.PrintBarcode(request.Item.items);
            List<string> base64Response = new List<string>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Text.StringBuilder asb = new System.Text.StringBuilder();

            InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode("",
                reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

            var response = barcodeprinter.Render(barcodeDataList.ConvertAll(x => x as Report.IBarcodeFormat));
            foreach (var item in response)
            {
                asb.AppendLine(item);
                asb.Append(";");
                var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                sb.Append(Convert.ToBase64String(bytes));
                sb.Append(";");
            }
            base64Response.Add(sb.ToString());
            return this.Ok(base64Response);
        }
    }
}