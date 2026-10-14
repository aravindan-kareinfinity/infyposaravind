
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
    [RoutePrefix("BillsettlementmasterService")]
    public class BillsettlementmasterServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Billsettlementmaster>();
            result.Item = new Billsettlementmaster();
            result.Item.paymentinfo = new Billsettlementmaster.Paymentinfo();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<BillsettlementmasterCreteria> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            List<Billsettlementmaster> result = await Task<List<Billsettlementmaster>>.Run(() => BillsettlementmasterData.Search(request.Item));
            if (request.Item.id > 0 && result.Count == 1)
            {
                result[0].directpdf = (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).Config.system_saledirectpdf ||
InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[result[0].locationid].system_saledirectpdf);
                result[0].directdos = request.Item.directdos;
                result[0].printdocument = PrintSettlement(BillsettlementmasterData.Context.OrganizationID, result[0]);
            }
            return this.Ok(result);
        }

        [HttpGet]
        [Route("HasSettlementNumber/{locationid}/{counterid}/{no}")]
        public async Task<IHttpActionResult> HasSettlementNumber(string locationid, string counterid, string no)
        {
            BillsettlementmasterDataService MfgdesignData = new BillsettlementmasterDataService();
            string result = await Task<string>.Run(() => MfgdesignData.HasSettlementNumber(locationid, counterid, no));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("NextWindowSettlementNumber/{locationid}/{counterid}")]
        public async Task<IHttpActionResult> NextWindowSettlementNumber(string locationid, string counterid)
        {
            BillsettlementmasterDataService MfgdesignData = new BillsettlementmasterDataService();
            String result = await Task<String>.Run(() => MfgdesignData.NextAutoNumber(locationid, counterid));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("WindowSettlementNumber/{locationid}/{counterid}")]
        public async Task<IHttpActionResult> WindowSettlementNumber(string locationid, string counterid)
        {
            BillsettlementmasterDataService MfgdesignData = new BillsettlementmasterDataService();
            String result = await Task<String>.Run(() => MfgdesignData.CurrentAutoNumber(locationid, counterid));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Billsettlementmaster> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            request.Item.casherid = BillsettlementmasterData.Context.EmployeeID;
            request.Item.cashiername = BillsettlementmasterData.Context.EmployeeName;
            if (request.Item.id <= 0)
            {
                if (request.Item.locationid <= 0 || request.Item.counterid <= 0)
                {
                    throw new Exception("Please select the location.");
                }
                request.Item.settlementon = BillsettlementmasterData.DateTimeNow;
                request.Item.createdon = BillsettlementmasterData.DateTimeNow;

                bool hasdaychange = false;
                if (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledatefreeze &&
                    InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledatefreezeon > DateTime.MinValue)
                {
                    DateTime newsettlementdate = InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledatefreezeon;
                    request.Item.settlementon = newsettlementdate.Date;
                }
                else if (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).Config.system_datechange &&
                    InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[request.Item.locationid].currentday == BillsettlementmasterData.DateTimeNow.Date &&
                        InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[request.Item.locationid].nextdaycount > 0)
                    {
                        DateTime currentsettlementdate = BillsettlementmasterData.DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[request.Item.locationid].nextdaycount);
                        request.Item.settlementon = currentsettlementdate;
                        hasdaychange = true;
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_validatesettlement &&
                    request.Item.Billsettlement.Any(e => e.billdate.Date != request.Item.settlementon.Date))
                {
                    var billdate = request.Item.Billsettlement.First().billdate;
                    request.Item.settlementon = billdate;
                    //request.Item.backwardserialno = hasdaychange;
                }

                if (request.Item.Billsettlement.Any(e => e.billdate.Date != request.Item.settlementon.Date))
                {
                    return this.Ok(new ActionSingleResponse<Billsettlementmaster>()
                    {
                        Item = request.Item,
                        Success = false,
                        ErrorMessage = "Can't settle other days bill.(To enable - Sync Billdate & Settlement Date)"
                    });
                }

                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Billsettlement != null)
                {
                    foreach (var item in request.Item.Billsettlement)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                        item.locationid = request.Item.locationid;
                        item.receiptdate = request.Item.settlementon;
                    }
                }
                Billsettlementmaster result = await Task<Billsettlementmaster>.Run(() => BillsettlementmasterData.InsertDirect(request.Item));
                try
                {
                    if (result.id > 0)
                    {
                        result.directdos = request.Item.directdos;
                        result.directpdf = (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).Config.system_saledirectpdf ||
InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[result.locationid].system_saledirectpdf);
                        result.printdocument = PrintSettlement(BillsettlementmasterData.Context.OrganizationID, result);
                    }
                }
                catch (Exception exp)
                {

                }
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Billsettlement != null)
                {
                    foreach (var item in request.Item.Billsettlement)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Billsettlementmaster result = await Task<Billsettlementmaster>.Run(() => BillsettlementmasterData.Update(request.Item));
                try
                {
                    if (result.id > 0)
                    {
                        result.directpdf = (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).Config.system_saledirectpdf ||
InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[result.locationid].system_saledirectpdf);
                        result.directpdf = request.Item.directdos;
                        result.printdocument = PrintSettlement(BillsettlementmasterData.Context.OrganizationID, result);
                    }
                }
                catch (Exception exp)
                {

                }
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Print")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Print)]
        public async Task<IHttpActionResult> Print(ActionRequest<SettlementPrint> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            Billsettlementmaster result = BillsettlementmasterData.Search(request.Item);
            if (result != null && result.hasCreditNote && result.creditnote != null && result.creditnote.Count == 0)
            {
                GiftCouponDataService couponData = new GiftCouponDataService();
                List<SearchCouponResult> _list = couponData.SearchCreditNote(new GiftVoucherCreteria() { billsettlementmasterid = result.id });
                if (_list != null && _list.Count > 0)
                {
                    Billsettlementmaster.voucherinfo creditnote = new Billsettlementmaster.voucherinfo()
                    {
                        code = _list[0].vouchercode,
                        amount = _list[0].vouchervalue,
                        validupto = _list[0].validto,
                        type = 7
                    };
                    if (result.paymentinfo.voucherinformation == null)
                        result.paymentinfo.voucherinformation = new List<Billsettlementmaster.voucherinfo>();
                    result.paymentinfo.voucherinformation.Add(creditnote);
                }
            }

            if (result != null)
            {
                result.directpdf = (InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).Config.system_saledirectpdf ||
    InfyPOS.Services.ConfigManager.Instance(BillsettlementmasterData.Context.OrganizationID).LocationConfig[result.locationid].system_saledirectpdf);
                result.directdos = request.Item.directdos;
                result.printdocument = PrintSettlement(BillsettlementmasterData.Context.OrganizationID, result);
                return this.Ok(result);
            }
            return this.NotFound();
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Print)]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var source = BillsettlementmasterData.Search(new BillsettlementmasterCreteria()
            {
                id = long.Parse(id)
            }).First();
            if (source != null)
            {
                if (source.paymentinfo != null && source.paymentinfo.HasDiscount)
                {
                    source.Billsettlement.ForEach(x => x.HasDiscount = true);
                    if (source.paymentinfo.discount_approvedBy > 0)
                    {
                        EmployeeDataService empdata = new EmployeeDataService();
                        var emplist = empdata.SearchHeader(new EmployeeCreteria() { id = source.paymentinfo.discount_approvedBy });
                        if (emplist != null && emplist.Count > 0)
                        {
                            source.paymentinfo.discount_approvedbyname = emplist[0].name;
                        }
                    }
                }
                if (source.createdby > 0)
                {
                    source.casherid = source.createdby;
                    UsersDataService userdata = new UsersDataService();
                    var userlist = userdata.Search(new UsersCreteria() { id = source.casherid });
                    if (userlist != null && userlist.Count > 0)
                    {
                        source.cashiername = userlist[0].title;
                    }
                }
                source.billnos = string.Join(",", source.Billsettlement.ConvertAll<string>(e => e.billno));
                source.reason = string.Join(",", source.paymentinfo.discount_info);

                InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
                var reportformats = config.Search(new ConfigurationCreteria
                {
                    key = "SETTLEMENT",
                    locationid = source.locationid,
                    organizationid = InfyPOS.Services.ConfigManager.Instance(long.Parse(organizationid)).Config.system_multicompany ? source.companyid : 0
                });

                if (reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SETTLEMENT",
                        locationid = source.locationid
                    });
                }

                if (reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SETTLEMENT"
                    });
                }
                if (reportformats.Count == 0)
                {
                    return this.InternalServerError(new Exception("Settlement PDF configuration not found"));
                }

                if (reportformats[0].reportformat.format.Contains("company"))
                {
                    CompanyDataService companyDataService = new CompanyDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
                    var companies = companyDataService.Search(new CompanyCreteria()
                    {
                        id = source.companyid
                    });
                    if (companies.Count > 0)
                    {
                        source.company = companies.First();
                        source.company.parseaddress();
                    }
                }
                var byteArray = PrintBill(long.Parse(organizationid), reportformats.First(), source, false);
                return new FileActionResult(true, "bill.pdf", byteArray.Extension, byteArray.Content);
            }
            return this.NotFound();
        }

        public byte[] PrintPDF(string id)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService() { Context = new UserClaim() { OrganizationID = 0 } };
            var source = BillsettlementmasterData.Search(new BillsettlementmasterCreteria()
            {
                id = long.Parse(id)
            }).First();
            if (source != null)
            {
                InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
                var reportformats = config.Search(new ConfigurationCreteria
                {
                    key = "SETTLEMENT",
                    locationid = source.locationid,
                    organizationid = InfyPOS.Services.ConfigManager.Instance(0).Config.system_multicompany ? source.companyid : 0
                });

                if (reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SETTLEMENT",
                        locationid = source.locationid
                    });
                }

                if (reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SETTLEMENT"
                    });
                }

                if (source.paymentinfo != null && source.paymentinfo.HasDiscount)
                {
                    source.Billsettlement.ForEach(x => x.HasDiscount = true);
                    if (source.paymentinfo.discount_approvedBy > 0)
                    {
                        EmployeeDataService empdata = new EmployeeDataService();
                        var emplist = empdata.SearchHeader(new EmployeeCreteria() { id = source.paymentinfo.discount_approvedBy });
                        if (emplist != null && emplist.Count > 0)
                        {
                            source.paymentinfo.discount_approvedbyname = emplist[0].name;
                        }
                    }
                }
                if (source.createdby > 0)
                {
                    source.casherid = source.createdby;
                    UsersDataService userdata = new UsersDataService();
                    var userlist = userdata.Search(new UsersCreteria() { id = source.casherid });
                    if (userlist != null && userlist.Count > 0)
                    {
                        source.cashiername = userlist[0].title;
                    }
                }
                source.billnos = string.Join(",", source.Billsettlement.ConvertAll<string>(e => e.billno));
                source.reason = string.Join(",", source.paymentinfo.discount_info);


                if (reportformats[0].reportformat.format.Contains("company"))
                {
                    CompanyDataService companyDataService = new CompanyDataService() { Context = new UserClaim() { OrganizationID = 0 } };
                    var companies = companyDataService.Search(new CompanyCreteria()
                    {
                        id = source.companyid
                    });
                    if (companies.Count > 0)
                    {
                        source.company = companies.First();
                        source.company.parseaddress();
                    }
                }

                return PrintBill(0, reportformats.First(), source, false).Content.ReadAsByteArrayAsync().Result;
            }
            return null;
        }

        public string PrintSettlement(long organizationid, Billsettlementmaster source)
        {
            if (source.discountedbills != null &&
                source.discountedbills.Count > 0)
            {
                int noofcopy;
                BillServiceController billServiceController = new BillServiceController();
                foreach (var item in source.discountedbills)
                {
                    item.directdos = source.directdos;
                    item.directpdf = (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_saledirectpdf ||
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].system_saledirectpdf);
                    item.printdocument = billServiceController.Print(organizationid, item.source, true, true, false, out noofcopy);
                }
            }
            if (source.paymentinfo != null && source.paymentinfo.HasDiscount)
            {
                source.Billsettlement.ForEach(x => x.HasDiscount = true);
                if (source.paymentinfo.discount_approvedBy > 0)
                {
                    EmployeeDataService empdata = new EmployeeDataService();
                    var emplist = empdata.SearchHeader(new EmployeeCreteria() { id = source.paymentinfo.discount_approvedBy });
                    if (emplist != null && emplist.Count > 0)
                    {
                        source.paymentinfo.discount_approvedbyname = emplist[0].name;
                    }
                }
            }
            if (source.createdby > 0)
            {
                source.casherid = source.createdby;
                UsersDataService userdata = new UsersDataService();
                var userlist = userdata.Search(new UsersCreteria() { id = source.casherid });
                if (userlist != null && userlist.Count > 0)
                {
                    source.cashiername = userlist[0].title;
                }
            }
            if (source.paymentinfo != null && source.paymentinfo.card_bankid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var bank = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = source.paymentinfo.card_bankid
                }).First();
                source.paymentinfo.card_bankname = bank.name;
            }
            if (source.paymentinfo != null && source.paymentinfo.upiproviderid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var upi = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = source.paymentinfo.upiproviderid
                }).First();
                source.paymentinfo.upiprovidername = upi.name;
            }

            source.billnos = string.Join(",", source.Billsettlement.ConvertAll<string>(e => e.billno));
            source.reason = string.Join(",", source.paymentinfo.discount_info);

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "SETTLEMENT",
                locationid = source.locationid,
                organizationid = InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany ? source.companyid : 0
            });

            if (reportformats.Count == 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = "SETTLEMENT",
                    locationid = source.locationid
                });
            }

            if (reportformats.Count == 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = "SETTLEMENT"
                });
            }

            if (reportformats.Count > 1)
            {
                var papersize = source.papersize;
                if (reportformats.Exists(e => e.nooflines == papersize))
                {
                    reportformats = reportformats.FindAll(e => e.nooflines == papersize);
                }
            }

            if (reportformats.Count > 0)
            {
                var additional = new SortedDictionary<string, object>();
                var imageData = new Common.ImageData();
                imageData.Context = new UserClaim()
                {
                    OrganizationID = organizationid
                };
                ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(imageData.Context.OrganizationID, reportformats[0].reportformat.format);
                if (reportformats[0].reportformat.format.Contains("settlementgift"))
                {
                    DeliveryDataService deliveryDataService = new DeliveryDataService();
                    source.settlementgift = deliveryDataService.GetSettlementGiftList(source);
                }

                if (reportformats[0].reportformat.format.Contains("deliveryno"))
                {
                    using (var reader = imageData.ExecuteReader("select cast(billattributes->>'deliveryno' as varchar),id from bill where id in (" + string.Join(",", source.Billsettlement.ConvertAll<long>(e => e.billid)) + ")", new SortedList<string, object>()))
                    {
                        while (reader.Read())
                        {
                            var billid = reader.GetInt64(1);
                            source.Billsettlement.Find(e => e.billid == billid).deliveryno = reader.IsDBNull(0) ? "" : reader.GetString(0);
                        }
                    }
                    source.deliverylist = new List<BillDeliveryMasterInfo>();
                    foreach (var item in source.Billsettlement)
                    {
                        if (source.deliverylist.Exists(e => e.deliveryno == item.deliveryno))
                        {
                            var deliveryitem = source.deliverylist.Find(e => e.deliveryno == item.deliveryno);
                            deliveryitem.receivable += item.receivable;
                        }
                        else
                        {
                            source.deliverylist.Add(new BillDeliveryMasterInfo()
                            {
                                deliveryno = item.deliveryno,
                                receivable = item.receivable
                            });
                        }
                    }
                }
                if (reportformats[0].reportformat.format.Contains("orderno"))
                {
                    for (int i = 0; i < source.Billsettlement.Count; i++)
                        source.Billsettlement[i].orderno = i + 1;

                    if (source.deliverylist != null && source.deliverylist.Count > 0)
                    {
                        for (int i = 0; i < source.deliverylist.Count; i++)
                            source.deliverylist[i].orderno = i + 1;
                    }
                }

                CompanyDataService companyDataService = new CompanyDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                var companies = companyDataService.Search(new CompanyCreteria()
                {
                    id = source.companyid
                });
                if (companies.Count > 0)
                {
                    source.company = companies.First();
                    source.company.parseaddress();
                }

                List<string> imageids = labreport.format.GetImages();
                labreport.Additional = new SortedDictionary<string, object>();
                foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                {
                    if (!string.IsNullOrEmpty(imgid))
                    {
                        var file = imageData.GetData(imgid.LongFromBase62());
                        if (file != null && file.data != null)
                        {
                            additional.Add("IMG-" + imgid, file.data);
                        }
                    }
                }

                if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("HasFreeGift"))
                {
                    BillDataService billDataService = new BillDataService();
                    var bills = billDataService.Search(new BillCreteria()
                    {
                        ids = source.Billsettlement.ConvertAll(x => x.billid)
                    });
                    source.FreeItems = new List<BillItemAttributes.FreeItems>();
                    bills.ForEach(x =>
                    {
                        x.Billitems.ForEach(y =>
                        {
                            if (y.attributes != null &&
                                y.attributes.freeitems != null)
                            {
                                source.FreeItems.AddRange(y.attributes.freeitems);
                            }
                        });
                    });
                }

                if (source.customerid > 0 &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryenable &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount > 0)
                {
                    if (reportformats[0].reportformat.format != null &&
                                            reportformats[0].reportformat.format.Contains("Lottery_") &&
                                            source.receivable >= InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount)
                    {
                        var noofcoupons = Math.Floor(source.receivable /
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount);
                        if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax > 0 &&
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax < noofcoupons)
                            noofcoupons = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax;
                        source.Lottery_Avilable = true;
                        source.Lottery_Count = Convert.ToInt32(noofcoupons);
                        if (source.customer == null)
                        {
                            CustomerDataService customerDataService = new CustomerDataService();
                            source.customer = customerDataService.Search(new CustomerCreteria() { id = source.customerid }).First();
                        }
                    }
                }


                if (source.company != null && !string.IsNullOrEmpty(source.company.logo))
                {
                    var file = imageData.GetData(source.company.logo.LongFromBase62());
                    if (file != null)
                        additional.Add("LOGO", file.data);
                }

                if (!source.directdos && (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_saledirectpdf ||
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].system_saledirectpdf))
                {
                    var billprint = PrintBill(organizationid, reportformats[0], source, true);
                    Byte[] byteArray = billprint.Content.ReadAsByteArrayAsync().Result;
                    return Convert.ToBase64String(byteArray);

                }
                else
                {
                    //labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, reportformats[0].reportformat.format);
                    var document = labreport.format.RenderBytes(new ImageData(), source, source.Billsettlement, additional, true);

                    if (reportformats[0].noofcopy > 1)
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                            bw.Write(document);
                            int c = 1;
                            while (c < reportformats[0].noofcopy)
                            {
                                bw.Write(document);
                                c++;
                            }
                            return Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    return Convert.ToBase64String(document);
                }
            }
            //WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(new Exception("SETTLEMENT format is missing for " + source.locationid), WebAPI.Common.Logger.LogType.Error, source);
            return null;
        }

        public FileContentInfo PrintBill(long organizationid, Configuration format, Billsettlementmaster source, bool json)
        {
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, format.reportformat.format);
            labreport.DataSource = source;
            labreport.DataItems = source.Billsettlement;
            var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = organizationid } };
            List<string> imageids = labreport.format.GetImages();
            labreport.Additional = new SortedDictionary<string, object>();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                if (!string.IsNullOrEmpty(imgid))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }
            }

            if (source.company != null && !string.IsNullOrEmpty(source.company.logo))
            {
                var file = imageData.GetData(source.company.logo.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }

            if (json)
            {
                ReportLibrary.JSONPrinter printer = new ReportLibrary.JSONPrinter();
                printer.Report = labreport;
                printer.IsThermalPrint = true;
                if (format.reportformat.format != null &&
                    format.reportformat.format.ToLower().IndexOf("thermal") > 0)
                {
                    printer.IsThermalPrint = true;
                }
                var document = printer.Print(labreport.format.GetDefaultSetting());
                var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document))));
                fci.FileName = "billsettlement.json";
                fci.Extension = "application/json";
                return fci;
            }
            else
            {
                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                printer.IsThermalPrint = true;
                using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                {
                    var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                    fci.FileName = "billsettlement.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
            }
        }

        [HttpPost]
        [Route("Reconilation")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Reconilation(ActionRequest<BillsettlementmasterCreteria> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            bool result = await Task<bool>.Run(() => BillsettlementmasterData.Reconilation(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Billsettlementmaster> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            Billsettlementmaster result = await Task<Billsettlementmaster>.Run(() => BillsettlementmasterData.InsertDirect(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Billsettlementmaster> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            Billsettlementmaster result = await Task<Billsettlementmaster>.Run(() => BillsettlementmasterData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Billsettlementmaster> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            bool result = await Task<bool>.Run(() => BillsettlementmasterData.Delete(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Cancel")]
        [WebAPI.InfyAuthorize(PermissionSet.settlement, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Cancel(ActionRequest<Billsettlementmaster> request)
        {
            BillsettlementmasterDataService BillsettlementmasterData = new BillsettlementmasterDataService();
            bool result = await Task<bool>.Run(() => BillsettlementmasterData.Cancel(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchLastSettlements")]
        public async Task<IHttpActionResult> SearchLastSettlements()
        {
            BillsettlementmasterDataService BillmasterData = new BillsettlementmasterDataService();
            List<BillInfo> result = await Task<List<BillInfo>>.Run(() => BillmasterData.SearchLastSettlements());
            return this.Ok(result);
        }
    }
}