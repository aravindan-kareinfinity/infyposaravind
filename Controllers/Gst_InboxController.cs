
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
    [RoutePrefix("Gst_InboxService")]
    public class Gst_InboxServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Gst_Inbox>();
            result.Item = new Gst_Inbox();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<Gst_InboxCreteria> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            List<Gst_Inbox> result = await Task<List<Gst_Inbox>>.Run(() => Gst_InboxData.Search(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadGSTR1/{organizationcode}/{companyid}/{year}/{month}")]
        public async Task<IHttpActionResult> DownloadGSTR1(string organizationcode, string companyid, string year, string month)
        {
            Gst_InboxDataService onlineIntegration = new Gst_InboxDataService();
            byte[] content = await Task<byte[]>.Run(() => onlineIntegration.BuildGSTR1DataSet(new Gst_Inbox()
            {
                companyid = long.Parse(companyid),
                fromdate = new DateTime(int.Parse(year), int.Parse(month), 1),
                todate = new DateTime(int.Parse(year), int.Parse(month), 1).AddMonths(1)
            }));
            return new FileActionResult(true, "gstr1.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content));
        }

        [HttpPost]
        [Route("RequestOTP")]
        public async Task<IHttpActionResult> RequestOTP(ActionRequest<Gst_InboxCreteria> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            bool result = await Task<bool>.Run(() => Gst_InboxData.RequestOTP(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ApplyOTP")]
        public async Task<IHttpActionResult> ApplyOTP(ActionRequest<Gst_InboxCreteria> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            bool result = await Task<bool>.Run(() => Gst_InboxData.ApplyOTP(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("LocalSearch")]
        public async Task<IHttpActionResult> LocalSearch(ActionRequest<Gst_InboxLocal> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            List<GST_Reconciliation> result = await Task<List<GST_Reconciliation>>.Run(() => Gst_InboxData.LocalSearch(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ShowGSTR1")]
        public async Task<IHttpActionResult> ShowGSTR1(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            InfyPOS.Models.SalesTransaction result = await Task<InfyPOS.Models.SalesTransaction>.Run(() => Gst_InboxData.ShowGSTR1(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrepareGSTR1")]
        public async Task<IHttpActionResult> PrepareGSTR1(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            InfyPOS.Models.GST.GSTR1A result = await Task<InfyPOS.Models.GST.GSTR1A>.Run(() => Gst_InboxData.BuildGSTR1(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("EInvoice")]
        public async Task<IHttpActionResult> EInvoice(ActionRequest<GST_Invoice> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            InfyGST.GSTPortal.EInvoiceResponse result = await Task<InfyGST.GSTPortal.EInvoiceResponse>.Run(() => Gst_InboxData.CreateEInvoice4Bill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Download")]
        public async Task<IHttpActionResult> Download(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            if (request.Item.gsttype == "GSTR2A")
            {
                Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.DownloadGSTR2A(request.Item));
                return this.Ok(result);
            }
            if (request.Item.gsttype == "GSTR1A")
            {
                Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.DownloadGSTR1(request.Item));
                return this.Ok(result);
            }
            if (request.Item.gsttype == "GSTR2B")
            {
                Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.DownloadGSTR2B(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("GSTR1Status")]
        public async Task<IHttpActionResult> GSTR1Status(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            if (request.Item.gsttype == "GSTR1")
            {
                string result = await Task<string>.Run(() => Gst_InboxData.GSTR1Status(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("ResetGSTR1")]
        public async Task<IHttpActionResult> ResetGSTR1(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            if (request.Item.gsttype == "GSTR1")
            {
                string result = await Task<string>.Run(() => Gst_InboxData.ResetGSTR1(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }


        [HttpPost]
        [Route("Upload")]
        public async Task<IHttpActionResult> Upload(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            if (request.Item.gsttype == "GSTR1")
            {
                Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.UpdateGSTR1(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("ValidateHSN")]
        public async Task<IHttpActionResult> ValidateHSN(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.ValidateHSN(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Import")]
        public async Task<IHttpActionResult> Exclude(ActionMulitpleRequest<GST_Expense> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            List<GST_Expense> result = await Task<List<GST_Expense>>.Run(() => Gst_InboxData.Import(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Exclude")]
        public async Task<IHttpActionResult> Exclude(ActionMulitpleRequest<GST_Exclude> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();

            List<GST_Exclude> result = await Task<List<GST_Exclude>>.Run(() => Gst_InboxData.Exculde(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Reconcil")]
        public async Task<IHttpActionResult> Reconcil(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            if (request.Item.gsttype == "GSTR2B")
            {
                List<GST_Reconciliation> result = await Task<List<GST_Reconciliation>>.Run(() => Gst_InboxData.ReconciliationGSTR2B(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<GST_Reconciliation> result = await Task<List<GST_Reconciliation>>.Run(() => Gst_InboxData.ReconciliationGSTR2A(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            if (request.Item.id <= 0)
            {
                Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            Gst_Inbox result = await Task<Gst_Inbox>.Run(() => Gst_InboxData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Gst_Inbox> request)
        {
            Gst_InboxDataService Gst_InboxData = new Gst_InboxDataService();
            bool result = await Task<bool>.Run(() => Gst_InboxData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}