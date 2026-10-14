
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
    [RoutePrefix("PermissionSlipService")]
    public class PermissionSlipServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<PermissionSlip>();
            result.Item = new PermissionSlip();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.employeepermission, Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<PermissionSlipCreteria> request)
        {
            PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
            List<PermissionSlipDetail> result = await Task<List<PermissionSlipDetail>>.Run(() => PermissionSlipData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.employeepermission, Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<PermissionSlip> request)
        {
            PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                PermissionSlip result = await Task<PermissionSlip>.Run(() => PermissionSlipData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                PermissionSlip result = await Task<PermissionSlip>.Run(() => PermissionSlipData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.employeepermission, Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<PermissionSlip> request)
        {
            PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
            PermissionSlip result = await Task<PermissionSlip>.Run(() => PermissionSlipData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.employeepermission, Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<PermissionSlip> request)
        {
            PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
            PermissionSlip result = await Task<PermissionSlip>.Run(() => PermissionSlipData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.employeepermission, Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<PermissionSlip> request)
        {
            PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
            bool result = await Task<bool>.Run(() => PermissionSlipData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<PermissionSlipPrint> request)
        {
            PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
            PermissionSlipPrint result = await Task<PermissionSlipPrint>.Run(() => PermissionSlipData.Print(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                PermissionSlipDataService PermissionSlipData = new PermissionSlipDataService();
                PermissionSlipData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                PermissionSlipPrint result = await Task<PermissionSlipPrint>.Run(() => PermissionSlipData.Print(
                    new PermissionSlipPrint()
                    {
                        id = long.Parse(id)
                    }));
                FileContentInfo byteArray = PrintPermissionSlip(PermissionSlipData.Context.OrganizationID, result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        private FileContentInfo PrintPermissionSlip(long organizationid, PermissionSlipPrint result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            string printslip = "";
            if (result.mode == 1)
            {
                printslip = "PERMISSIONSLIP";
            }
            else
            {
                printslip = "LEAVESLIP";
            }

            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = printslip
            });


            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "permissionslip.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}