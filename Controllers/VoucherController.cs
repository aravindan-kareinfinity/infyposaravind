
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
using Newtonsoft.Json;
using WebAPI.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("VoucherService")]
    public class VoucherServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Voucher>();
            result.Item = new Voucher();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("OpeningBalance")]
        public async Task<IHttpActionResult> OpeningBalance(ActionRequest<OpeningCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            decimal result = await Task<decimal>.Run(() => VoucherData.CashClosing(new SaleRetailCreteria()
            {
                companyid = request.Item.companyid,
                locationid = request.Item.locationid,
                fromdate = request.Item.voucherdate
            }, request.Item.ledgerid, request.Item.mode));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveDayBook")]
        public async Task<IHttpActionResult> SaveDayBook(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            Voucher result = await Task<Voucher>.Run(() => VoucherData.SaveDayBook(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("LedgerBalance")]
        public async Task<IHttpActionResult> LedgerBalance(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            LedgerBalance result = await Task<LedgerBalance>.Run(() => VoucherData.LedgerBalance(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchLedger")]
        public async Task<IHttpActionResult> SearchLedger(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            List<VoucherDisplayItem> result = await Task<List<VoucherDisplayItem>>.Run(() => VoucherData.SearchLedger(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchReceivable")]
        public async Task<IHttpActionResult> SearchReceivable(ActionRequest<LedgerListCreteria> request)
        {
            VoucheritembillwiseDataService VoucherData = new VoucheritembillwiseDataService();
            List<PayableReceivableLedger> result = await Task<List<PayableReceivableLedger>>.Run(() => VoucherData.SearchReceivable(request.Item));
            return this.Ok(result);

        }
        [HttpPost]
        [Route("SearchPayable")]
        public async Task<IHttpActionResult> SearchPayable(ActionRequest<LedgerListCreteria> request)
        {
            VoucheritembillwiseDataService VoucherData = new VoucheritembillwiseDataService();
            List<PayableReceivableLedger> result = await Task<List<PayableReceivableLedger>>.Run(() => VoucherData.SearchPayable(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Analyser")]
        public async Task<IHttpActionResult> Analyser(ActionRequest<FinanceAnalyseCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                FinanceAnalyse result = await Task<FinanceAnalyse>.Run(() => VoucherData.Analyser(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                   return await Task<FinanceAnalyse>.Run(() => VoucherData.Analyser(request.Item));
                }, "VoucherService.Search", request.Item, 3, organizationId, userId);

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

        [HttpPost]
        [Route("TDSReport")]
        public async Task<IHttpActionResult> TDSReport(ActionRequest<FinanceAnalyseCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                FinanceAnalyse result = await Task<FinanceAnalyse>.Run(() => VoucherData.TDSReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<FinanceAnalyse>.Run(() => VoucherData.TDSReport(request.Item));
                }, "VoucherService.Search", request.Item, 3, organizationId, userId);

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

        [HttpPost]
        [Route("GSTReport")]
        public async Task<IHttpActionResult> GSTReport(ActionRequest<FinanceAnalyseCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                FinanceAnalyse result = await Task<FinanceAnalyse>.Run(() => VoucherData.GSTReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<FinanceAnalyse>.Run(() => VoucherData.GSTReport(request.Item));
                }, "VoucherService.Search", request.Item, 3, organizationId, userId);

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

        [HttpPost]
        [Route("SearchDayBook")]
        public async Task<IHttpActionResult> SearchDayBook(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                List<VoucherDisplayItem> result = await Task<List<VoucherDisplayItem>>.Run(() =>
                request.Item.ledgerid > 0 ? VoucherData.SearchLedger(request.Item) : VoucherData.SearchDaybook(request.Item));
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<VoucherDisplayItem>>.Run(() => request.Item.ledgerid > 0 ? VoucherData.SearchLedger(request.Item) : VoucherData.SearchDaybook(request.Item));
                }, "VoucherService.Search", request.Item, 3, organizationId, userId);

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

        [HttpPost]
        [Route("GenerateEInvoice")]
        public async Task<IHttpActionResult> GenerateEInvoice(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService BillinboxData = new VoucherDataService();
            InfyGST.GSTPortal.EInvoiceResponse result = await Task<InfyGST.GSTPortal.EInvoiceResponse>.Run(() => BillinboxData.GenerateEInvoice(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CancelEInvoice")]
        public async Task<IHttpActionResult> CancelEInvoice(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService BillinboxData = new VoucherDataService();
            InfyGST.GSTPortal.EwayFromGRNResponse result = await Task<InfyGST.GSTPortal.EwayFromGRNResponse>.Run(() => BillinboxData.CancelEInvoice(request.Item));
            return this.Ok(result);
        }



        [HttpGet]
        [Route("DailyPnL/{locationid}/{fromdate}/{todate}")]
        public async Task<IHttpActionResult> TrialBalance(string locationid, string fromdate, string todate)
        {
            InfyPOS.Tally.FinanceService VoucherData = new InfyPOS.Tally.FinanceService();
            var from = BaseEntity.GetDataFromString(fromdate);
            var to = BaseEntity.GetDataFromString(todate);
            long lid = 0;
            long.TryParse(locationid, out lid);
            Tally.FinanceService.DailyPandL result =
                await Task<Tally.FinanceService.DailyPandL>.Run(() => VoucherData.BuildDailyPandL(from, to, lid));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("TrialBalance/{companyid}/{locationid}/{fromdate}/{todate}")]
        public async Task<IHttpActionResult> TrialBalance(string companyid, string locationid, string fromdate, string todate)
        {
            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                InfyPOS.Tally.FinanceService VoucherData = new InfyPOS.Tally.FinanceService();
                var from = BaseEntity.GetDataFromString(fromdate);
                var to = BaseEntity.GetDataFromString(todate);
                long cid = 0;
                long.TryParse(companyid, out cid);
                long lid = 0;
                long.TryParse(locationid, out lid);
                List<InfyPOS.Tally.FinanceService.TrailBalanceLedger> result =
                    await Task<List<InfyPOS.Tally.FinanceService.TrailBalanceLedger>>.Run(() => VoucherData.TrailBalance(cid, lid, from, to));
                return this.Ok(result);
            }
            else
            {
                InfyPOS.Tally.FinanceService ActualDataService = new InfyPOS.Tally.FinanceService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var from = BaseEntity.GetDataFromString(fromdate);
                var to = BaseEntity.GetDataFromString(todate);
                long cid = 0;
                long.TryParse(companyid, out cid);
                long lid = 0;
                long.TryParse(locationid, out lid);
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    return await Task<List<InfyPOS.Tally.FinanceService.TrailBalanceLedger>>.Run(() => ActualDataService.TrailBalance(cid, lid, from, to));
                }, "VoucherService.TrialBalance",
                string.Format("{0}/{1}/{2}/{3}", companyid, locationid, fromdate, todate), 3, organizationId, userId);

                if (response.IsCompleted)
                {
                    return this.Ok(response.Result);
                }
                else
                {
                    var x = this.ResponseMessage(response.TimoutResponse);
                    return x;
                }
            }
        }


        [HttpGet]
        [Route("CurrentTrialBalance/{companyid}/{yearvalue}")]
        public async Task<IHttpActionResult> CurrentTrialBalance(string companyid, string locationid, string yearvalue)
        {
            InfyPOS.Tally.FinanceService VoucherData = new InfyPOS.Tally.FinanceService();
            var year = VoucherData.DateTimeNow.Month <= 3 ? VoucherData.DateTimeNow.Year - 1 : VoucherData.DateTimeNow.Year;
            if (!string.IsNullOrEmpty(yearvalue))
            {
                int yr = 0;
                if (int.TryParse(yearvalue, out yr))
                    year = yr;
            }
            long cid = 0;
            long.TryParse(companyid, out cid);
            long lid = 0;
            long.TryParse(locationid, out lid);
            List<InfyPOS.Tally.FinanceService.TrailBalanceLedger> result = await Task<List<InfyPOS.Tally.FinanceService.TrailBalanceLedger>>.Run(() => VoucherData.TrailBalance(cid, lid, year));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("CurrentTradingAccount/{companyid}/{locationid}/{fromdate}/{todate}/{stock=null}")]
        public async Task<IHttpActionResult> CurrentTradingAccount(string companyid, string locationid, string fromdate, string todate, string stock)
        {

            InfyPOS.Tally.FinanceService VoucherData = new InfyPOS.Tally.FinanceService();
            var from = BaseEntity.GetDataFromString(fromdate);
            var to = BaseEntity.GetDataFromString(todate);
            long cid = 0;
            long.TryParse(companyid, out cid);
            long lid = 0;
            long.TryParse(locationid, out lid);
            bool stkfromob = false;
            if (!string.IsNullOrEmpty(stock))
                bool.TryParse(stock, out stkfromob);
            VoucherData.LoadStockFromOpening = stkfromob;

            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                InfyPOS.Tally.FinanceService.FinanceResult result = await Task<InfyPOS.Tally.FinanceService.FinanceResult>.Run(() => VoucherData.TradingAccount(cid, lid, from, to, true));
                if (result != null)
                    result.PrepareDisplayList();
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    InfyPOS.Tally.FinanceService.FinanceResult result = await Task<InfyPOS.Tally.FinanceService.FinanceResult>.Run(() => VoucherData.TradingAccount(cid, lid, from, to, true));
                    if (result != null)
                        result.PrepareDisplayList();
                    return result;

                }, "VoucherService.Search", string.Format("{0}/{1}/{2}/{3}", companyid, locationid, fromdate, todate), 3, organizationId, userId);

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

        [HttpGet]
        [Route("CurrentProfitAndLossAccount/{companyid}/{locationid}/{fromdate}/{todate}/{stock=null}")]
        public async Task<IHttpActionResult> CurrentProfitAndLossAccount(string companyid, string locationid, string fromdate, string todate, string stock)
        {
            InfyPOS.Tally.FinanceService VoucherData = new InfyPOS.Tally.FinanceService();
            var from = BaseEntity.GetDataFromString(fromdate);
            var to = BaseEntity.GetDataFromString(todate);

            long cid = 0;
            long.TryParse(companyid, out cid);
            long lid = 0;
            long.TryParse(locationid, out lid);
            bool stkfromob = false;
            if (!string.IsNullOrEmpty(stock))
                bool.TryParse(stock, out stkfromob);
            VoucherData.LoadStockFromOpening = stkfromob;


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                InfyPOS.Tally.FinanceService.FinanceResult result = await Task<InfyPOS.Tally.FinanceService.FinanceResult>.Run(() => VoucherData.ProfitLossAccount(cid, lid, from, to, true));
                if (result != null)
                    result.PrepareDisplayList();
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {
                    InfyPOS.Tally.FinanceService.FinanceResult result = await Task<InfyPOS.Tally.FinanceService.FinanceResult>.Run(() => VoucherData.ProfitLossAccount(cid, lid, from, to, true));
                    if (result != null)
                        result.PrepareDisplayList();
                    return result;
                }, "VoucherService.Search", string.Format("{0}/{1}/{2}/{3}", companyid, locationid, fromdate, todate), 3, organizationId, userId);

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

        [HttpGet]
        [Route("CurrentBalanceSheet/{companyid}/{locationid}/{fromdate}/{todate}/{stock=null}")]
        public async Task<IHttpActionResult> CurrentBalanceSheet(string companyid, string locationid, string fromdate, string todate,string stock)
        {
            InfyPOS.Tally.FinanceService VoucherData = new InfyPOS.Tally.FinanceService();
            var from = BaseEntity.GetDataFromString(fromdate);
            var to = BaseEntity.GetDataFromString(todate);
            long cid = 0;
            long.TryParse(companyid, out cid);
            long lid = 0;
            long.TryParse(locationid, out lid);
            bool stkfromob = false;
            if (!string.IsNullOrEmpty(stock))
                bool.TryParse(stock, out stkfromob);
            VoucherData.LoadStockFromOpening = stkfromob;


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                InfyPOS.Tally.FinanceService.FinanceResult result = await Task<InfyPOS.Tally.FinanceService.FinanceResult>.Run(() => VoucherData.BalanceSheet(cid, lid, from, to));
                if (result != null)
                    result.PrepareDisplayList();
                return this.Ok(result);
            }
            else
            {
                InboxDataService ActualDataService = new InboxDataService();
                var organizationId = ActualDataService.Context?.OrganizationID ?? 0;
                var userId = ActualDataService.Context?.UserID.ToString() ?? "";
                var response = await queueManager.ExecuteWithTimeout<object>(async () =>
                {

                    InfyPOS.Tally.FinanceService.FinanceResult result = await Task<InfyPOS.Tally.FinanceService.FinanceResult>.Run(() => VoucherData.BalanceSheet(cid, lid, from, to));
                    if (result != null)
                        result.PrepareDisplayList();
                    return result;

                }, "VoucherService.Search", string.Format("{0}/{1}/{2}/{3}", companyid, locationid, fromdate, todate), 3, organizationId, userId);

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

        [HttpPost]
        [Route("SavePettyCash")]
        public async Task<IHttpActionResult> SavePettyCash(ActionRequest<Voucher> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            Voucher result = await Task<Voucher>.Run(() => VoucherData.SavePettyCash(request.Item));
            return this.Ok(result);
        }

        [Route("DownloadPettyCash")]
        [HttpPost]
        public async Task<IHttpActionResult> DownloadPettyCash(VoucherCreteria Item)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            PettycashVoucher result = await Task<PettycashVoucher>.Run(() => VoucherData.SearchPettyCashReport(Item));

            if (result == null)
                result = new PettycashVoucher();

            decimal opening = await Task<decimal>.Run(() => VoucherData.CashClosing(new SaleRetailCreteria()
            {
                companyid = Item.companyid,
                locationid = Item.locationid,
                fromdate = Item.fromdate,
                counterid = Item.attributes == null ? 0 : Item.attributes.counterid
            }, Item.ledgerid, VoucherDataService.CashMode.PettyCash));

            result.opening = opening;
            result.cashincome = result.items.Sum(x => x.typeid != 7 ? x.credit : 0);
            result.cashexpense = result.items.Sum(x => x.typeid != 7 ? x.debit : 0);
            result.cashwithdraw = result.items.Sum(x => x.typeid == 7 ? x.credit : 0);
            result.cashdeposit = result.items.Sum(x => x.typeid == 7 ? x.debit : 0);
            result.closing = result.opening + result.cashincome - result.cashexpense + result.cashdeposit - result.cashwithdraw;
            CompanyDataService companyDataService = new CompanyDataService();
            ReferencelistDataService referencelistData = new ReferencelistDataService();
            result.company = companyDataService.Search(new CompanyCreteria() { id = Item.companyid }).First();
            result.location = new CompanyLocation(referencelistData.Search(new ReferencelistCreteria() { id = Item.locationid }).First());
            var fci = PrintLedgerPDF(VoucherData.Context.OrganizationID, Item, result);
            return new FileActionResult(false, "", fci.Extension, fci.Content);
        }

        private FileContentInfo PrintLedgerPDF(long organizationID, VoucherCreteria vouchercreteria, PettycashVoucher pettycashVoucher)
        {
            pettycashVoucher.company.parseaddress();
            PrintableLedger printableLedger = new PrintableLedger();
            printableLedger.location = pettycashVoucher.location;
            printableLedger.company = pettycashVoucher.company;
            if (printableLedger.company != null)
                printableLedger.company.parseaddress();

            printableLedger.obdebit = pettycashVoucher.opening < 0 ? (pettycashVoucher.opening * -1).ToString() : ""; 
            printableLedger.obcredit = pettycashVoucher.opening > 0 ? pettycashVoucher.opening.ToString() : "";

            printableLedger.cbdebit = pettycashVoucher.closing < 0 ? (pettycashVoucher.closing * -1).ToString() : "";
            printableLedger.cbcredit = pettycashVoucher.closing > 0 ? pettycashVoucher.closing.ToString() : "";
            if (vouchercreteria.fromdate.Date != vouchercreteria.todate.Date)
            {
                printableLedger.title = "Petty Cash Report for " + vouchercreteria.fromdate.ToString("dd-MM-yyyy") +
                    " - " + vouchercreteria.todate.ToString("dd-MM-yyyy");
            }
            else
            {
                printableLedger.title = "Petty Cash Report for " + vouchercreteria.fromdate.ToString("dd-MM-yyyy");
            }
            printableLedger.transactions = new List<PrintableLedger.Transaction>();
            printableLedger.issummary = vouchercreteria.type != "detail";
            foreach (var item in pettycashVoucher.items)
            {
                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                {
                    date = item.date,
                    description = item.ledger,
                    reference = item.narr,
                    credit = item.credit > 0 ? item.credit.ToString("F2") : "",
                    debit = item.debit>0 ? item.debit.ToString("F2") : "",
                });
            }
            return PrintLedgerPDF(organizationID, printableLedger);
        }

        public FileContentInfo PrintLedgerPDF(long organizationid,
            PrintableLedger pettycashledgers)
        {
            pettycashledgers.ProcessTotal(true);
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "PETTYCASH"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            ReportLibrary.LaserReporter labreport = new ReportLibrary.LaserReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = pettycashledgers;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    if (img != null)
                        labreport.Additional.Add("IMG-" + imgid, img);
                }
            }
            if (pettycashledgers.company != null && !string.IsNullOrEmpty(pettycashledgers.company.logo))
            {
                var file = imageData.GetData(pettycashledgers.company.logo.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    if (img != null)
                        labreport.Additional.Add("LOGO", img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "chequeadvice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

        [HttpPost]
        [Route("SearchPettyCashReport")]
        public async Task<IHttpActionResult> SearchPettyCashReport(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            PettycashVoucher result = await Task<PettycashVoucher>.Run(() => VoucherData.SearchPettyCashReport(request.Item));

            if (result == null)
                result = new PettycashVoucher();

            decimal opening = await Task<decimal>.Run(() => VoucherData.CashClosing(new SaleRetailCreteria()
            {
                companyid = request.Item.companyid,
                locationid = request.Item.locationid,
                fromdate = request.Item.fromdate,
                counterid = request.Item.attributes == null ? 0 : request.Item.attributes.counterid
            }, request.Item.ledgerid, VoucherDataService.CashMode.PettyCash));

            result.opening = opening;
            result.cashincome = result.items.Sum(x => x.typeid != 7 ? x.credit : 0);
            result.cashexpense = result.items.Sum(x => x.typeid != 7 ? x.debit : 0);
            result.cashwithdraw = result.items.Sum(x => x.typeid == 7 ? x.credit : 0);
            result.cashdeposit = result.items.Sum(x => x.typeid == 7 ? x.debit : 0);
            result.closing = result.opening + result.cashincome - result.cashexpense + result.cashdeposit - result.cashwithdraw;
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPettyCash")]
        public async Task<IHttpActionResult> SearchPettyCash(ActionRequest<VoucherCreteria> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            Voucher result = await Task<Voucher>.Run(() => VoucherData.SearchPettyCash(request.Item));

            if (result == null)
                result = new Voucher();

            decimal opening = await Task<decimal>.Run(() => VoucherData.CashClosing(new SaleRetailCreteria()
            {
                companyid = request.Item.companyid,
                locationid = request.Item.locationid,
                fromdate = request.Item.voucherdate,
                counterid = request.Item.attributes == null ? 0 : request.Item.attributes.counterid
            }, request.Item.ledgerid, VoucherDataService.CashMode.PettyCash));

            var cashinout = await Task<decimal>.Run(() => VoucherData.CashInOut(new SaleRetailCreteria()
            {
                companyid = request.Item.companyid,
                locationid = request.Item.locationid,
                fromdate = request.Item.voucherdate,
                counterid = request.Item.attributes == null ? 0 : request.Item.attributes.counterid
            }, request.Item.ledgerid));

            var cashtransaction = await Task<decimal>.Run(() => VoucherData.SearchCashTransaction(request.Item,
                request.Item.voucherdate,
                request.Item.voucherdate.AddDays(1)));

            var pcv = new PettycashVoucher()
            {
                voucher = result,
                cashexpense = cashinout.CashExpense,
                cashincome = cashinout.CashIncome,
                cashdeposit = cashinout.CashDeposit,
                cashwithdraw = cashinout.CashWithdraw,
                opening = opening
            };
            pcv.SetCashTransaction(cashtransaction);
            return this.Ok(pcv);
        }

        [HttpPost]
        [Route("Payable")]

        public async Task<IHttpActionResult> Payable(ActionRequest<VoucherCreteria> request)
        {
            VoucheritembillwiseDataService VoucherData = new VoucheritembillwiseDataService();
            List<BillwiseItem> result = await Task<List<BillwiseItem>>.Run(() => VoucherData.Payable(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Receivable")]
        public async Task<IHttpActionResult> Receivable(ActionRequest<VoucherCreteria> request)
        {
            VoucheritembillwiseDataService VoucherData = new VoucheritembillwiseDataService();
            List<BillwiseItem> result = await Task<List<BillwiseItem>>.Run(() => VoucherData.Receivable(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<VoucherCreteria> request)
        {
            if (request.Item.vouchertype == 33)
            {
                VoucherPODataService VoucherData = new VoucherPODataService();
                List<VoucherDetail> result = await Task<List<VoucherDetail>>.Run(() => VoucherData.Search(request.Item));
                return this.Ok(result);
            }
            else
            {
                VoucherDataService VoucherData = new VoucherDataService();
                List<VoucherDetail> result = await Task<List<VoucherDetail>>.Run(() => VoucherData.Search(request.Item));
                return this.Ok(result);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Print/{organizationid}/{voucherid}")]
        public async Task<IHttpActionResult> Print(string organizationid, string voucherid)
        {
            try
            {
                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateReceipt(false, long.Parse(organizationid), long.Parse(voucherid)));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPO/{organizationid}/{voucherid}")]
        public async Task<IHttpActionResult> PrintPO(string organizationid, string voucherid)
        {
            try
            {
                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateReceipt(true, long.Parse(organizationid),
                    long.Parse(voucherid)));
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<VoucherCreteria> request)
        {
            try
            {
                BaseData baseData = new BaseData();
                List<string> printdocument = await Task<List<string>>.Run(() => CreateReceipt(request.Item.vouchertype == 33, baseData.Context.OrganizationID, request.Item.ids));
                return this.Ok(printdocument);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        private List<string> CreateReceipt(bool ispo, long organizationid, List<long> ids)
        {
            List<string> receipt = new List<string>();
            try
            {
                ConfigurationDataService configdata = new ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                VoucherDataService voucherdata = new VoucherDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                VoucherPODataService voucherPOdata = new VoucherPODataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                foreach (long voucherid in ids)
                {
                    VoucherPrint _voucher = null;
                    if (ispo)
                        _voucher = voucherPOdata.PrintVoucher(voucherid);
                    else
                        _voucher = voucherdata.PrintVoucher(voucherid);

                    if (_voucher == null)
                        continue;

                    ConfigurationCreteria configcreteria = new ConfigurationCreteria();
                    configcreteria.organizationid = _voucher.companyid;
                    configcreteria.locationid = _voucher.locationid;
                    configcreteria.key = "VOUCHERRECEIPT";

                    List<Configuration> lstconfig = configdata.Search(configcreteria);
                    if (lstconfig == null || lstconfig.Count == 0)
                    {
                        configcreteria.organizationid = _voucher.companyid;
                        configcreteria.locationid = 0;
                        lstconfig = configdata.Search(configcreteria);
                    }
                    if (lstconfig == null || lstconfig.Count == 0)
                    {
                        configcreteria.organizationid = 0;
                        configcreteria.locationid = _voucher.locationid;
                        lstconfig = configdata.Search(configcreteria);
                    }
                    if (lstconfig == null || lstconfig.Count == 0)
                    {
                        configcreteria.organizationid = 0;
                        configcreteria.locationid = 0;
                        lstconfig = configdata.Search(configcreteria);
                    }
                    if (lstconfig == null || lstconfig.Count == 0)
                    {
                        continue;
                    }

                    if (lstconfig.Count > 1 && lstconfig.Exists(e => e.name == _voucher.displaytitle))
                    {
                        lstconfig.RemoveAll(e => e.name != _voucher.displaytitle);
                    }

                    ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
                    labreport.DataSource = _voucher;
                    labreport.DataItems = _voucher.voucheritem;
                    if (lstconfig[0].reportformat.format.IndexOf("childledgers") > 0)
                        labreport.DataItems = _voucher.childledgers;
                    labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
                    labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();


                    var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
                    List<string> imageids = labreport.format.GetImages();
                    labreport.Additional = new SortedDictionary<string, object>();
                    foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                    {
                        var file = imageData.GetData(imgid.LongFromBase62());
                        if (file != null && file.data != null && file.data.Length > 0)
                        {
                            var img = imageData.FromBytes(file.data);
                            labreport.Additional.Add("IMG-" + imgid, img);
                        }
                    }

                    if (_voucher.company != null && !string.IsNullOrEmpty(_voucher.company.logo))
                    {
                        var file = imageData.GetData(_voucher.company.logo.LongFromBase62());
                        if (file != null && file.data != null && file.data.Length > 0)
                        {
                            var img = imageData.FromBytes(file.data);
                            labreport.Additional.Add("LOGO", img);
                        }
                    }

                    if (_voucher.attributes != null &&
                        !string.IsNullOrEmpty(_voucher.attributes.einv))
                    {
                        InfyGST.GSTPortal.EInvoiceResponse res = JsonConvert.DeserializeObject<InfyGST.GSTPortal.EInvoiceResponse>(_voucher.attributes.einvdetail);
                        var img = imageData.FromBytes(Convert.FromBase64String(res.QrCodeImage));
                        _voucher.attributes.GSTEinvoice = res;
                        labreport.Additional.Add("IMG-" + "EINVQR", img);
                    }

                    ReportLibrary.JSONPrinter printer = new ReportLibrary.JSONPrinter();
                    printer.Report = labreport;
                    printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
                    receipt.Add(Newtonsoft.Json.JsonConvert.SerializeObject(printer.Print(labreport.format.GetDefaultSetting())));
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return receipt;
        }

        private FileContentInfo CreateReceipt(bool ispo, long organizationid, long voucherid)
        {
            try
            {
                ConfigurationDataService configdata = new ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

                VoucherPODataService voucherPOdata = new VoucherPODataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                VoucherDataService voucherdata = new VoucherDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                VoucherPrint _voucher = null;

                if (ispo)
                    _voucher = voucherPOdata.PrintVoucher(voucherid);
                else
                    _voucher = voucherdata.PrintVoucher(voucherid);


                if (_voucher == null)
                    return null;

                ConfigurationCreteria configcreteria = new ConfigurationCreteria();
                configcreteria.organizationid = _voucher.companyid;
                configcreteria.locationid = _voucher.locationid;
                configcreteria.key = "VOUCHERRECEIPT";

                List<Configuration> lstconfig = configdata.Search(configcreteria);
                if (lstconfig == null || lstconfig.Count == 0)
                {
                    configcreteria.organizationid = _voucher.companyid;
                    configcreteria.locationid = 0;
                    lstconfig = configdata.Search(configcreteria);
                }
                if (lstconfig == null || lstconfig.Count == 0)
                {
                    configcreteria.organizationid = 0;
                    configcreteria.locationid = _voucher.locationid;
                    lstconfig = configdata.Search(configcreteria);
                }
                if (lstconfig == null || lstconfig.Count == 0)
                {
                    configcreteria.organizationid = 0;
                    configcreteria.locationid = 0;
                    lstconfig = configdata.Search(configcreteria);
                }
                if (lstconfig == null || lstconfig.Count == 0)
                {
                    throw new Exception("Please configure the voucher receipt");
                }

                if (lstconfig.Count > 1 && lstconfig.Exists(e => e.name == _voucher.displaytitle))
                {
                    lstconfig.RemoveAll(e => e.name != _voucher.displaytitle);
                }

                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
                labreport.DataSource = _voucher;
                labreport.DataItems = _voucher.voucheritem;

                if (lstconfig[0].reportformat.format.IndexOf("childledgers") > 0)
                    labreport.DataItems = _voucher.childledgers;
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();


                var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
                List<string> imageids = labreport.format.GetImages();
                labreport.Additional = new SortedDictionary<string, object>();
                foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null && file.data.Length > 0)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }

                if (_voucher.company != null && !string.IsNullOrEmpty(_voucher.company.logo))
                {
                    var file = imageData.GetData(_voucher.company.logo.LongFromBase62());
                    if (file != null && file.data != null && file.data.Length > 0)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("LOGO", img);
                    }
                }


                if (_voucher.attributes != null &&
                    !string.IsNullOrEmpty(_voucher.attributes.einv))
                {
                    InfyGST.GSTPortal.EInvoiceResponse res = JsonConvert.DeserializeObject<InfyGST.GSTPortal.EInvoiceResponse>(_voucher.attributes.einvdetail);
                    var img = imageData.FromBytes(Convert.FromBase64String(res.QrCodeImage));
                    _voucher.attributes.GSTEinvoice = res;
                    labreport.Additional.Add("IMG-" + "EINVQR", img);
                }

                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
                using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                {
                    var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                    fci.FileName = "voucher-" + voucherid + ".pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Voucher> request)
        {
            if (request.Item.vouchertype == 33)
            {
                VoucherPODataService VoucherData = new VoucherPODataService();
                if (request.Item.id <= 0)
                {
                    request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                    if (request.Item.voucheritems != null)
                    {
                        foreach (var item in request.Item.voucheritems)
                        {
                            item.ChangeStatus(BaseEntity.DataStatus.Create);
                        }
                    }
                    Voucher result = await Task<Voucher>.Run(() => VoucherData.Insert(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                    if (request.Item.voucheritems != null)
                    {
                        foreach (var item in request.Item.voucheritems)
                        {
                            item.ChangeStatus(BaseEntity.DataStatus.Update);
                        }
                    }
                    Voucher result = await Task<Voucher>.Run(() => VoucherData.Update(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                VoucherDataService VoucherData = new VoucherDataService();
                if (request.Item.id <= 0)
                {
                    request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                    if (request.Item.voucheritems != null)
                    {
                        foreach (var item in request.Item.voucheritems)
                        {
                            item.ChangeStatus(BaseEntity.DataStatus.Create);
                        }
                    }
                    Voucher result = await Task<Voucher>.Run(() => VoucherData.Insert(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                    if (request.Item.voucheritems != null)
                    {
                        foreach (var item in request.Item.voucheritems)
                        {
                            item.ChangeStatus(BaseEntity.DataStatus.Update);
                        }
                    }
                    Voucher result = await Task<Voucher>.Run(() => VoucherData.Update(request.Item));
                    return this.Ok(result);
                }
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Voucher> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            Voucher result = await Task<Voucher>.Run(() => VoucherData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.voucherentry, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Voucher> request)
        {
            VoucherDataService VoucherData = new VoucherDataService();
            Voucher result = await Task<Voucher>.Run(() => VoucherData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Voucher> request)
        {
            if (request.Item.vouchertype == 33)
            {
                VoucherPODataService VoucherData = new VoucherPODataService();
                bool result = await Task<bool>.Run(() => VoucherData.Delete(request.Item));
                return this.Ok(result);
            }
            else
            {
                VoucherDataService VoucherData = new VoucherDataService();
                bool result = await Task<bool>.Run(() => VoucherData.Delete(request.Item));
                return this.Ok(result);
            }
        }
    }
}