
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
    [RoutePrefix("SupplierpaymentService")]
    public class SupplierpaymentServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Supplierpayment>();
            result.Item = new Supplierpayment();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SupplierpaymentCreteria> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();

            List<Supplierpayment> result = null;
            if (request.Item.searchmode == 1)
            {
                result = await Task<List<Supplierpayment>>.Run(() => SupplierpaymentData.SearchSummary(request.Item));
                return this.Ok(result);
            }
            if (request.Item.searchmode == 2)
            {
                var aresult = await Task<List<Supplierpayment>>.Run(() => SupplierpaymentData.SearchCommmission(request.Item));
                return this.Ok(aresult);
            }
            if (request.Item.searchmode == 3)
            {
                var aresult = await Task<List<Supplierpayment>>.Run(() => SupplierpaymentData.SearchCommissionSummary(request.Item));
                return this.Ok(aresult);
            }
            result = await Task<List<Supplierpayment>>.Run(() => SupplierpaymentData.Search(request.Item));
            return this.Ok(result);

        }
        [HttpPost]
        [Route("SearchPendingPayment")]
        public async Task<IHttpActionResult> SearchPendingPayment(ActionRequest<SupplierpaymentCreteria> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            if (request.Item.rowtype == 1)
            {
                List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => SupplierpaymentData.SearchPendingJobWorkPayment(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => SupplierpaymentData.SearchPendingPayment(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchPendingPaymentDetail")]
        public async Task<IHttpActionResult> SearchPendingPaymentDetail(ActionRequest<PendingLrInvoice> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            PendingLrInvoiceDetail result = await Task<PendingLrInvoiceDetail>.Run(() => SupplierpaymentData.SearchPendingPaymentDetail(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPlannedPayment")]
        public async Task<IHttpActionResult> SearchPlannedPayment(ActionRequest<SupplierpaymentCreteria> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            List<PlannedInvoice> result = await Task<List<PlannedInvoice>>.Run(() => SupplierpaymentData.SearchPlannedPayment(request.Item.suppliercompanyids, request.Item.locationid));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SetPlannedPayment")]
        public async Task<IHttpActionResult> SetPlannedPayment(ActionRequest<SupplierpaymentCreteria> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            List<PlannedInvoice> result = await Task<List<PlannedInvoice>>.Run(() => SupplierpaymentData.SetPlannedPayment(request.Item.pendinginvoices, request.Item.locationid));
            return this.Ok(result);
        }

        //[HttpGet]
        //[Route("SearchPlannedPayment")]
        //public async Task<IHttpActionResult> SearchPlannedPayment()
        //{
        //    SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
        //    List<PlannedInvoice> result = await Task<List<PlannedInvoice>>.Run(() => SupplierpaymentData.SearchPlannedPayment(null));
        //    return this.Ok(result);
        //}
        //[HttpPost]
        //[Route("SetPlannedPayment")]
        //public async Task<IHttpActionResult> SetPlannedPayment(ActionMulitpleRequest<PendingLrInvoice> request)
        //{
        //    SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
        //    List<PlannedInvoice> result = await Task<List<PlannedInvoice>>.Run(() => SupplierpaymentData.SetPlannedPayment(request.Items));
        //    return this.Ok(result);
        //}
        [HttpPost]
        [Route("SearchSupplierPayment")]
        public async Task<IHttpActionResult> SearchSupplierPayment(ActionRequest<SupplierpaymentCreteria> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => SupplierpaymentData.SearchSupplierPayment(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSupplierPaymentSummary")]
        public async Task<IHttpActionResult> SearchSupplierPaymentSummary(ActionRequest<SupplierpaymentCreteria> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            List<PendingLrInvoice> result = await Task<List<PendingLrInvoice>>.Run(() => SupplierpaymentData.SearchSupplierPaymentSummary(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Supplierpayment> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Supplierpaymentinvoice != null)
                {
                    foreach (var item in request.Item.Supplierpaymentinvoice)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Supplierpayment result = await Task<Supplierpayment>.Run(() => SupplierpaymentData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Supplierpaymentinvoice != null)
                {
                    foreach (var item in request.Item.Supplierpaymentinvoice)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Supplierpayment result = await Task<Supplierpayment>.Run(() => SupplierpaymentData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SaveAll")]
        public async Task<IHttpActionResult> SaveAll(ActionRequest<Supplierpayment> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            List<Supplierpayment> result = await Task<List<Supplierpayment>>.Run(() => SupplierpaymentData.SaveAll(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Supplierpayment> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            Supplierpayment result = await Task<Supplierpayment>.Run(() => SupplierpaymentData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Supplierpayment> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            Supplierpayment result = await Task<Supplierpayment>.Run(() => SupplierpaymentData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Supplierpayment> request)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            bool result = await Task<bool>.Run(() => SupplierpaymentData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Publish/{organizationid}/{id}")]
        public async Task<IHttpActionResult> Publish(string organizationid, string id)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            bool result = await Task<bool>.Run(() =>
            {
                foreach (var payid in id.Split('_'))
                {
                    SupplierPaymentPrint print = SupplierpaymentData.Print(new Supplierpayment() { id = long.Parse(payid) });

                    if (!string.IsNullOrEmpty(print.supplier.contactnumber) && print.supplier.contactnumber.Length >= 10 &&
                    !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(SupplierpaymentData.Context.OrganizationID).Config.hostedserver))
                    {
                        string licensekey = LicenseManager.License.Key.Replace("-", "");
                        InfyPOS.Controllers.ImageController.HashDetail hashDetail = new ImageController.HashDetail(SupplierpaymentData.Context.OrganizationID, ImageController.HashDetail.GetLongHash(licensekey), print.id);
                        string smscontent = "Please find the payment advice slip from {0} with Reference No {1} dated {2} \n\r";
                        smscontent = string.Format(smscontent, print.company.name, print.receiptno, print.paymentdate.ToString("dd-MM-yyyy"));
                        smscontent += InfyPOS.Services.ConfigManager.Instance(SupplierpaymentData.Context.OrganizationID).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/PDF/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);

                        //Processors.Schedule_Actions.Instance.OnChange((organizationid), print, Processors.Schedule_Actions.Actions.Publish);
                        InfyPOS.Services.SMSManager.Instance.SendSMS((SupplierpaymentData.Context.OrganizationID), print.supplier.contactnumber, smscontent);
                    }

                    if (Processors.Schedule_Actions.Instance.HasConfig(SupplierpaymentData.Context.OrganizationID, Processors.Schedule_Actions.SourceType.SupplierPayment, Processors.Schedule_Actions.Actions.New))
                    {
                        string licensekey = LicenseManager.License.Key.Replace("-", "");
                        InfyPOS.Controllers.ImageController.HashDetail hashDetail = new InfyPOS.Controllers.ImageController.HashDetail(SupplierpaymentData.Context.OrganizationID, InfyPOS.Controllers.ImageController.HashDetail.GetLongHash(licensekey), print.id);
                        var url = InfyPOS.Services.ConfigManager.Instance(SupplierpaymentData.Context.OrganizationID).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/SP/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
                        print.url = url;
                        var fileattachment = SupplierpaymentData.PrintChequeAdvice(SupplierpaymentData.Context.OrganizationID, print);
                        print.attachment = new SortedDictionary<string, byte[]>();
                        print.attachment.Add("Payment-" + print.receiptno, fileattachment.Content.ReadAsByteArrayAsync().Result);
                        Processors.Schedule_Actions.Instance.OnChange(SupplierpaymentData.Context.OrganizationID, print, Processors.Schedule_Actions.Actions.New);
                    }
                }
                return true;

            });
            return this.Ok(result);
        }

        private bool Publish(long organizationid, SupplierPaymentPrint print)
        {

            return false;
        }

        [HttpGet]
        [Route("SupplierLedger/{companyid}/{locationid}/{id}/{from}/{to}")]
        public async Task<IHttpActionResult> SupplierLedger(string companyid,string locationid,
            string id, string from, string to)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                PrintableLedger printable = await Task<FileContentInfo>.Run(() =>
                {
                    return SupplierpaymentData.PrintSummary(long.Parse(id), long.Parse(locationid), long.Parse(companyid),
                    BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to), true);
                });
                return this.Ok(printable);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<FileContentInfo>.Run(() =>
                    {
                        return SupplierpaymentData.PrintSummary(long.Parse(id), long.Parse(locationid), long.Parse(companyid),
                        BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to), true);
                    });
                }, "SupplierServices.SupplierLedger", string.Format("{0}/{1}/{2}/{3}/{4}", companyid, locationid, id, from, to), 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    return this.ResponseMessage(response.TimoutResponse);
                }
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintSummaryPDF/{organizationid}/{companyid}/{locationid}/{id}/{from}/{to}")]
        public async Task<IHttpActionResult> PrintSummaryPDF(string organizationid, string companyid, string locationid,
            string id, string from, string to)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    PrintableLedger print = SupplierpaymentData.PrintSummary(long.Parse(id), 
                        long.Parse(locationid),
                        long.Parse(companyid),
                      BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to),true);
                    LedgerServiceController ledgerServiceController = new LedgerServiceController();
                    return ledgerServiceController.PrintLedgerPDF(SupplierpaymentData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(SupplierpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + id);
                return this.Ok(exp);
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintSummaryPDF/{organizationid}/{companyid}/{locationid}/{id}")]
        public async Task<IHttpActionResult> PrintSummaryPDF(string organizationid, string companyid,string locationid, string id)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    PrintableLedger print = SupplierpaymentData.PrintSummary(long.Parse(id), 
                        long.Parse(locationid),
                        long.Parse(companyid),
                      DateTime.Now.Month > 3 ? DateTime.Now.Year : DateTime.Now.Year - 1,true);
                    LedgerServiceController ledgerServiceController = new LedgerServiceController();
                    return ledgerServiceController.PrintLedgerPDF(SupplierpaymentData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(SupplierpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + id);
                return this.Ok(exp);
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintCheuqeAdvicePDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintCheuqeAdvicePDF(string organizationid, string id)
        {
            SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SupplierPaymentPrint print = SupplierpaymentData.Print(new Supplierpayment() { id = long.Parse(id) });
                    return SupplierpaymentData.PrintChequeAdvice(SupplierpaymentData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(SupplierpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + id);
                return this.Ok(exp);
            }
        }

    }
}