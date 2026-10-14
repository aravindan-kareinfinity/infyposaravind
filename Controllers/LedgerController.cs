
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
    [RoutePrefix("LedgerService")]
    public class LedgerServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Ledger>();
            result.Item = new Ledger();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LedgerCreteria> request)
        {
            LedgerDataService LedgerData = new LedgerDataService();
            List<LedgerDetail> result = await Task<List<LedgerDetail>>.Run(() => LedgerData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Ledger> request)
        {
            LedgerDataService LedgerData = new LedgerDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Ledger result = await Task<Ledger>.Run(() => LedgerData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Ledger result = await Task<Ledger>.Run(() => LedgerData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<ChangeAttributes> request)
        {
            LedgerDataService LedgerData = new LedgerDataService();
            bool result = await Task<bool>.Run(() => LedgerData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Ledger> request)
        {
            LedgerDataService LedgerData = new LedgerDataService();
            Ledger result = await Task<Ledger>.Run(() => LedgerData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Ledger> request)
        {
            LedgerDataService LedgerData = new LedgerDataService();
            Ledger result = await Task<Ledger>.Run(() => LedgerData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Ledger> request)
        {
            LedgerDataService LedgerData = new LedgerDataService();
            bool result = await Task<bool>.Run(() => LedgerData.Delete(request.Item));
            return this.Ok(result);
        }

        public class LedgerPrint
        {
            [JsonProperty("c")]
            public long c { get; set; }
            [JsonProperty("ln")]
            public long ln { get; set; }
            [JsonProperty("ld")]
            public long ld { get; set; }

            [JsonProperty("f")]
            public DateTime f { get; set; }
            [JsonProperty("t")]
            public DateTime t { get; set; }
        }

        [HttpGet]
        [Route("LedgerSummary/{ledgerinfo}")]
        public async Task<IHttpActionResult> LedgerSummary(string ledgerinfo)
        {
            VoucherDataService SupplierpaymentData = new VoucherDataService();
            LedgerPrint ledgerCreteria = JsonConvert.DeserializeObject<LedgerPrint>(
                System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(ledgerinfo)));


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {
                PrintableLedger printable = await Task<FileContentInfo>.Run(() =>
                {
                    var print = SupplierpaymentData.SearchLedger(new VoucherCreteria()
                    {
                        companyid = ledgerCreteria.c,
                        locationid = ledgerCreteria.ln,
                        ledgerid = ledgerCreteria.ld,
                        fromdate = ledgerCreteria.f,
                        todate = ledgerCreteria.t,
                    });


                    PrintableLedger printableLedger = new PrintableLedger();

                    LedgerDataService ledgerDataService = new LedgerDataService() { Context = SupplierpaymentData.Context };
                    var ledger = ledgerDataService.Search(new LedgerCreteria()
                    {
                        id = ledgerCreteria.ld
                    });
                    printableLedger.ledgername = ledger[0].displayname;
                    printableLedger.displayname = ledger[0].displayname;
                    printableLedger.address = ledger[0].contactaddress;
                    printableLedger.title = ledgerCreteria.f.ToString("dd-MM-yyyy") + " - " + ledgerCreteria.t.ToString("dd-MM-yyyy");
                    if (ledger[0].bankid > 0)
                    {
                        printableLedger.bankid = ledger[0].bankid;
                    }
                    var row = print.Find(x => x.id == -1);
                    if (row != null)
                    {
                        printableLedger.obdebit = row.crdr == "Dr" ? row.amount.ToString() : "";
                        printableLedger.obcredit = row.crdr == "Cr" ? row.amount.ToString() : "";
                    }
                    printableLedger.transactions = new List<PrintableLedger.Transaction>();


                    foreach (var item in print.FindAll(x => x.id >= 0))
                    {
                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            type = item.vouchername,
                            date = item.date,
                            description = item.ledger,
                            credit = item.crdr == "Cr" ? item.amount.ToString("F2") : "",
                            debit = item.crdr != "Cr" ? item.amount.ToString("F2") : "",
                            detail = item
                        });
                    }

                    printableLedger.ProcessTotal();
                    return printableLedger;
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
                        var print = SupplierpaymentData.SearchLedger(new VoucherCreteria()
                        {
                            companyid = ledgerCreteria.c,
                            locationid = ledgerCreteria.ln,
                            ledgerid = ledgerCreteria.ld,
                            fromdate = ledgerCreteria.f,
                            todate = ledgerCreteria.t,
                        });


                        PrintableLedger printableLedger = new PrintableLedger();

                        LedgerDataService ledgerDataService = new LedgerDataService() { Context = SupplierpaymentData.Context };
                        var ledger = ledgerDataService.Search(new LedgerCreteria()
                        {
                            id = ledgerCreteria.ld
                        });
                        printableLedger.ledgername = ledger[0].displayname;
                        printableLedger.displayname = ledger[0].displayname;
                        printableLedger.address = ledger[0].contactaddress;
                        printableLedger.title = ledgerCreteria.f.ToString("dd-MM-yyyy") + " - " + ledgerCreteria.t.ToString("dd-MM-yyyy");
                        if (ledger[0].bankid > 0)
                        {
                            printableLedger.bankid = ledger[0].bankid;
                        }
                        var row = print.Find(x => x.id == -1);
                        if (row != null)
                        {
                            printableLedger.obdebit = row.crdr == "Dr" ? row.amount.ToString() : "";
                            printableLedger.obcredit = row.crdr == "Cr" ? row.amount.ToString() : "";
                        }
                        printableLedger.transactions = new List<PrintableLedger.Transaction>();


                        foreach (var item in print.FindAll(x => x.id >= 0))
                        {
                            printableLedger.transactions.Add(new PrintableLedger.Transaction()
                            {
                                type = item.vouchername,
                                date = item.date,
                                description = item.ledger,
                                credit = item.crdr == "Cr" ? item.amount.ToString("F2") : "",
                                debit = item.crdr != "Cr" ? item.amount.ToString("F2") : "",
                                detail = item
                            });
                        }

                        printableLedger.ProcessTotal();
                        return printableLedger;
                    });
                }, "VoucherSummary.Search", ledgerinfo, 3, organizationId, userId);

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
        [Route("VoucherSummary/{ledgerinfo}")]
        public async Task<IHttpActionResult> VoucherSummary(string ledgerinfo)
        {
            VoucherDataService SupplierpaymentData = new VoucherDataService();
            LedgerPrint ledgerCreteria = JsonConvert.DeserializeObject<LedgerPrint>(
                System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(ledgerinfo))
                );


            var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
            if (!queueManager.Enabled)
            {

                PrintableLedger printable = await Task<FileContentInfo>.Run(() =>
                {
                    var print = SupplierpaymentData.SearchLedger(new VoucherCreteria()
                    {
                        companyid = ledgerCreteria.c,
                        locationid = ledgerCreteria.ln,
                        ledgerid = ledgerCreteria.ld,
                        fromdate = ledgerCreteria.f,
                        todate = ledgerCreteria.t,
                    });


                    PrintableLedger printableLedger = new PrintableLedger();

                    LedgerDataService ledgerDataService = new LedgerDataService() { Context = SupplierpaymentData.Context };
                    var ledger = ledgerDataService.Search(new LedgerCreteria()
                    {
                        id = ledgerCreteria.ld
                    });
                    printableLedger.ledgername = ledger[0].displayname;
                    printableLedger.displayname = ledger[0].displayname;
                    printableLedger.address = ledger[0].contactaddress;
                    printableLedger.title = ledgerCreteria.f.ToString("dd-MM-yyyy") + " - " + ledgerCreteria.t.ToString("dd-MM-yyyy");
                    if (ledger[0].bankid > 0)
                    {
                        printableLedger.bankid = ledger[0].bankid;
                    }
                    var row = print.Find(x => x.id == -1);
                    if (row != null)
                    {
                        printableLedger.obdebit = row.crdr == "Dr" ? row.amount.ToString() : "";
                        printableLedger.obcredit = row.crdr == "Cr" ? row.amount.ToString() : "";
                    }
                    printableLedger.transactions = new List<PrintableLedger.Transaction>();


                    foreach (var item in print.FindAll(x => x.id > 0).GroupBy(x => new { x.id, x.vouchername, x.date }))
                    {
                        var lst = item.ToList();
                        var gst = lst.FindAll(x => x.hasgst);
                        var tds = lst.FindAll(x => x.hastds);
                        var rnd = lst.FindAll(x => x.ledgertype == 30);

                        lst.RemoveAll(x => x.hasgst);
                        lst.RemoveAll(x => x.hastds);
                        lst.RemoveAll(x => x.ledgertype == 30);

                        var trans = new List<PrintableLedger.Transaction>();
                        var flst = lst.OrderByDescending(x => x.amount).ToList();
                        for (var i = 0; i < flst.Count; i++)
                        {
                            var transaction = new PrintableLedger.Transaction()
                            {
                                type = flst[i].vouchername,
                                date = flst[i].date,
                                description = flst[i].ledger,
                                credit = flst[i].crdr == "Cr" ? flst[i].amount.ToString("F2") : "",
                                debit = flst[i].crdr != "Cr" ? flst[i].amount.ToString("F2") : "",
                                detail = flst[i],
                                tds = i == 0 ? tds.Sum(x => x.amount) : 0,
                                gst = i == 0 ? gst.Sum(x => x.amount) : 0,
                                rnd = i == 0 ? rnd.Sum(x => x.amount) : 0
                            };
                            if (trans.Exists(x => x.detail.ledgerid == transaction.detail.ledgerid))
                            {
                                var otransaction = trans.Find(x => x.detail.ledgerid == transaction.detail.ledgerid);
                                if (string.IsNullOrEmpty(otransaction.credit))
                                    otransaction.credit = transaction.credit;
                                else if (flst[i].crdr == "Cr")
                                    otransaction.credit = (decimal.Parse(transaction.credit) + flst[i].amount).ToString("F2");

                                if (string.IsNullOrEmpty(otransaction.debit))
                                    otransaction.debit = transaction.debit;
                                else if (flst[i].crdr == "Dr")
                                    otransaction.debit = (decimal.Parse(transaction.debit) + flst[i].amount).ToString("F2");
                            }
                            else
                            {
                                trans.Add(transaction);
                            }
                        }
                        printableLedger.transactions.AddRange(trans);
                    }

                    printableLedger.ProcessTotal();
                    return printableLedger;
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
                        var print = SupplierpaymentData.SearchLedger(new VoucherCreteria()
                        {
                            companyid = ledgerCreteria.c,
                            locationid = ledgerCreteria.ln,
                            ledgerid = ledgerCreteria.ld,
                            fromdate = ledgerCreteria.f,
                            todate = ledgerCreteria.t,
                        });


                        PrintableLedger printableLedger = new PrintableLedger();

                        LedgerDataService ledgerDataService = new LedgerDataService() { Context = SupplierpaymentData.Context };
                        var ledger = ledgerDataService.Search(new LedgerCreteria()
                        {
                            id = ledgerCreteria.ld
                        });
                        printableLedger.ledgername = ledger[0].displayname;
                        printableLedger.displayname = ledger[0].displayname;
                        printableLedger.address = ledger[0].contactaddress;
                        printableLedger.title = ledgerCreteria.f.ToString("dd-MM-yyyy") + " - " + ledgerCreteria.t.ToString("dd-MM-yyyy");
                        if (ledger[0].bankid > 0)
                        {
                            printableLedger.bankid = ledger[0].bankid;
                        }
                        var row = print.Find(x => x.id == -1);
                        if (row != null)
                        {
                            printableLedger.obdebit = row.crdr == "Dr" ? row.amount.ToString() : "";
                            printableLedger.obcredit = row.crdr == "Cr" ? row.amount.ToString() : "";
                        }
                        printableLedger.transactions = new List<PrintableLedger.Transaction>();


                        foreach (var item in print.FindAll(x => x.id > 0).GroupBy(x => new { x.id, x.vouchername, x.date }))
                        {
                            var lst = item.ToList();
                            var gst = lst.FindAll(x => x.hasgst);
                            var tds = lst.FindAll(x => x.hastds);
                            var rnd = lst.FindAll(x => x.ledgertype == 30);

                            lst.RemoveAll(x => x.hasgst);
                            lst.RemoveAll(x => x.hastds);
                            lst.RemoveAll(x => x.ledgertype == 30);

                            var trans = new List<PrintableLedger.Transaction>();
                            var flst = lst.OrderByDescending(x => x.amount).ToList();
                            for (var i = 0; i < flst.Count; i++)
                            {
                                var transaction = new PrintableLedger.Transaction()
                                {
                                    type = flst[i].vouchername,
                                    date = flst[i].date,
                                    description = flst[i].ledger,
                                    credit = flst[i].crdr == "Cr" ? flst[i].amount.ToString("F2") : "",
                                    debit = flst[i].crdr != "Cr" ? flst[i].amount.ToString("F2") : "",
                                    detail = flst[i],
                                    tds = i == 0 ? tds.Sum(x => x.amount) : 0,
                                    gst = i == 0 ? gst.Sum(x => x.amount) : 0,
                                    rnd = i == 0 ? rnd.Sum(x => x.amount) : 0
                                };
                                if (trans.Exists(x => x.detail.ledgerid == transaction.detail.ledgerid))
                                {
                                    var otransaction = trans.Find(x => x.detail.ledgerid == transaction.detail.ledgerid);
                                    if (string.IsNullOrEmpty(otransaction.credit))
                                        otransaction.credit = transaction.credit;
                                    else if (flst[i].crdr == "Cr")
                                        otransaction.credit = (decimal.Parse(transaction.credit) + flst[i].amount).ToString("F2");

                                    if (string.IsNullOrEmpty(otransaction.debit))
                                        otransaction.debit = transaction.debit;
                                    else if (flst[i].crdr == "Dr")
                                        otransaction.debit = (decimal.Parse(transaction.debit) + flst[i].amount).ToString("F2");
                                }
                                else
                                {
                                    trans.Add(transaction);
                                }
                            }
                            printableLedger.transactions.AddRange(trans);
                        }

                        printableLedger.ProcessTotal();
                        return printableLedger;
                    });


                }, "InboxService.Search", ledgerinfo, 3, organizationId, userId);

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
        [Route("PrintLedgerPDF/{organizationid}/{ledgerinfo}")]
        public async Task<IHttpActionResult> PrintLedgerPDF(string organizationid, string ledgerinfo)
        {
            VoucherDataService SupplierpaymentData = new VoucherDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {
                LedgerPrint ledgerCreteria = JsonConvert.DeserializeObject<LedgerPrint>(
                    System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(ledgerinfo))
                    );
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    var print = SupplierpaymentData.SearchLedger(new VoucherCreteria()
                    {
                        companyid = ledgerCreteria.c,
                        locationid = ledgerCreteria.ln,
                        ledgerid = ledgerCreteria.ld,
                        fromdate = ledgerCreteria.f,
                        todate = ledgerCreteria.t,
                    });
                    var fci = PrintLedgerPDF(SupplierpaymentData.Context.OrganizationID,
                        ledgerCreteria, print);
                    return fci;
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(SupplierpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + ledgerinfo);
                return this.Ok(exp);
            }
        }

        private FileContentInfo PrintLedgerPDF(long organizationID,
            LedgerPrint ledgerinfo,
            List<VoucherDisplayItem> print)
        {
            PrintableLedger printableLedger = new PrintableLedger();

            LedgerDataService ledgerDataService = new LedgerDataService() { Context = new UserClaim() { OrganizationID = organizationID } };
            var ledger = ledgerDataService.Search(new LedgerCreteria()
            {
                id = ledgerinfo.ld
            });
            printableLedger.ledgername = ledger[0].displayname;
            printableLedger.displayname = ledger[0].displayname;
            printableLedger.address = ledger[0].contactaddress;
            printableLedger.title = ledgerinfo.f.ToString("dd-MM-yyyy") + " - " + ledgerinfo.t.ToString("dd-MM-yyyy");

            if (ledgerinfo.c > 0)
            {
                CompanyDataService companyDataService = new CompanyDataService() { Context = new UserClaim() { OrganizationID = organizationID } };
                var companylist = companyDataService.Search(new CompanyCreteria()
                {
                    id = ledgerinfo.c
                });
                if (companylist.Count == 1)
                {
                    printableLedger.company = companylist[0];
                    printableLedger.company.parseaddress();
                }
            }

            var row = print.Find(x => x.id == -1);
            if (row != null)
            {
                printableLedger.obdebit = row.crdr == "Dr" ? row.amount.ToString() : "";
                printableLedger.obcredit = row.crdr == "Cr" ? row.amount.ToString() : "";
            }
            printableLedger.transactions = new List<PrintableLedger.Transaction>();



            foreach (var item in print.FindAll(x => x.id > 0 || x.iscustomer))
            {
                printableLedger.transactions.Add(new PrintableLedger.Transaction()
                {
                    type = item.vouchername,
                    date = item.date,
                    description = item.ledger,
                    credit = item.crdr == "Cr" ? item.amount.ToString("F2") : "",
                    debit = item.crdr != "Cr" ? item.amount.ToString("F2") : "",
                });
            }
            return PrintLedgerPDF(organizationID, printableLedger);
        }

        public FileContentInfo PrintLedgerPDF(long organizationid,
            PrintableLedger supplierPaymentPrint)
        {
            supplierPaymentPrint.ProcessTotal();
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "LEDGER"
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
            labreport.DataSource = supplierPaymentPrint;

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
            if (supplierPaymentPrint.company != null && !string.IsNullOrEmpty(supplierPaymentPrint.company.logo))
            {
                var file = imageData.GetData(supplierPaymentPrint.company.logo.LongFromBase62());
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
    }
}