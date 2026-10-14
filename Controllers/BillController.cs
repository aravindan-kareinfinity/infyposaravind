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
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf;
using Newtonsoft.Json;
using WebAPI.Data;
using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography;

namespace InfyPOS.Controllers
{
    [RoutePrefix("BillService")]
    public class BillServiceController : ApiController
    {

        [HttpGet]
        [Route("FBURL/{billno}")]
        [Route("Barcode/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> FeedbackURL(string billno)
        {
            BillDataService BillData = new BillDataService();
            string url = await Task<string>.Run(() => BillData.GetFBURL(billno));
            return this.Ok(url);
        }

        [HttpPost]
        [Route("SearchMasterBill")]
        public async Task<IHttpActionResult> SearchMasterBill(ActionRequest<BillCreteria> request)
        {
            BillDataService BillData = new BillDataService();
            try
            {
                request.Item.isMasterBill = true;

                List<Bill> result = await Task<List<Bill>>.Run(() => BillData.Search(request.Item));


                BillsettlementDataService settlementdata = new BillsettlementDataService();
                settlementdata.Context = BillData.Context;
                foreach (var item in result)
                {
                    var settlemtnitems = settlementdata.Search(new BillsettlementCreteria()
                    {
                        billid = item.id
                    });
                    if (settlemtnitems.Count > 0)
                    {
                        item.receivable = item.receivable - settlemtnitems.Sum(e => e.paid);
                    }
                }

                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BillData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return this.Ok(exp);
            }
        }


        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BillCreteria> request)
        {
            BillDataService BillData = new BillDataService();
            try
            {
                List<Bill> result = null;
                if (!request.Item.isbulkreturn)
                {
                    result = await Task<List<Bill>>.Run(() => BillData.Search(request.Item));
                }
                else
                {
                    BulkBillDataService BulkBillData = new BulkBillDataService();
                    result = await Task<List<Bill>>.Run(() => BulkBillData.Search(request.Item));
                }
                //if (request.Item.forsalesreturn && result.Count == 1)
                //{
                //    BillreturnDataService billreturndata = new BillreturnDataService();
                //    var returnresults = billreturndata.Search(new BillreturnCreteria()
                //    {
                //        billid = result[0].id
                //    });
                //    if (returnresults.Count > 0)
                //    {
                //        foreach (var item in returnresults)
                //        {
                //            if (item.Billreturnitems != null)
                //            {
                //                foreach (var bitem in item.Billreturnitems)
                //                {
                //                    if (result[0].Billitems.Exists(e => e.id == bitem.billitemsid))
                //                    {
                //                        result[0].Billitems.Find(e => e.id == bitem.billitemsid).qty -= bitem.qty;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                if (result.Count > 0 && result.Exists(e => e.Billitems.Exists(a => a.schemeid > 0)))
                {
                    PromotionsDataService promotiondata = new PromotionsDataService();
                    var promolist = promotiondata.Search(new PromotionsCreteria()).ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    foreach (var _item in result)
                    {
                        // Added scheme discount which is removed in Search Method. 
                        _item.discount += _item.schemediscount;
                        foreach (var _billitem in _item.Billitems)
                        {
                            if (_billitem.schemeid > 0 && promolist.Exists(e => e.id == _billitem.schemeid))
                                _billitem.scheme = promolist.Find(e => e.id == _billitem.schemeid).name;
                        }
                    }
                }

                if (request.Item.enableprintdocument && result.Count > 0)
                {
                    result.ForEach(e =>
                    {
                        e.papersize = request.Item.papersize;
                        e.printtemplateid = request.Item.printtemplateid;
                    });

                    bool ispdfprint = !request.Item.saledirectdos &&
                        !result.Any(x => x.billattributes.touchsale) &&
                        (request.Item.saledirectpdf ||
                        InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_saledirectpdf ||
                        (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(result[0].locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[result[0].locationid].system_saledirectpdf));

                    if (request.Item.isb2b &&
                        InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(result[0].locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[result[0].locationid].system_b2bsaledirectpdf)
                        ispdfprint = true;

                    if (ispdfprint)
                    {
                        try
                        {
                            var billprint = PrintBill(BillData.Context.OrganizationID, result[0], true);
                            Byte[] byteArray = billprint.Content.ReadAsByteArrayAsync().Result;
                            result[0].directpdf = true;
                            result[0].printdocument = Convert.ToBase64String(byteArray);
                            result[0].billcopy = ProcessBillCopy(BillData.Context.OrganizationID, result[0].locationid, result[0]);
                        }
                        catch (Exception exp)
                        {
                            result[0].errormessage = exp.Message;
                        }
                    }
                    else
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[result[0].locationid].sale_disableotherdaybillprint
                            && result.Exists(e => e.billdate.Date != BillData.DateTimeNow.Date))
                        {
                            return this.Ok(new Exception("Other day bill reprint option restricted. Please contact system admin"));
                        }

                        long lastid = result.Last().id;
                        long firstid = result.First().id;

                        foreach (var item in result)
                        {
                            try
                            {
                                int noofcopy = 0;
                                //item.printdocument = Print(BillData.Context.OrganizationID, item, false, true, true, out noofcopy);

                                item.printdocument = Print(BillData.Context.OrganizationID, item,
                                                            (lastid == item.id && !InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint),
                                                            firstid == item.id, (result.Count > 1 || InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint), out noofcopy);

                                item.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, item.locationid, item);

                                if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint &&
                                     item.billmasterid > 0)
                                {
                                    // Added this condition to skip Bill summary for Return bills
                                    if (item.billtype != Bill.BillTypes.ReturnBill
                                        || InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_exchangebillsummaryprintformat)
                                    {
                                        BillmasterDataService billmasterDataService = new BillmasterDataService();
                                        var mastList = billmasterDataService.Search(new BillmasterCreteria()
                                        {
                                            id = item.billmasterid
                                        });
                                        if (mastList.Count == 1)
                                        {
                                            mastList[0].Bills = new List<Bill>();
                                            mastList[0].Bills.Add(item);
                                            byte[] document = null;
                                            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                            {
                                                System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                                                bw.Write(Convert.FromBase64String(item.printdocument));
                                                mastList[0].discount = item.discount + item.additionaldiscount;
                                                mastList[0].receivable = item.receivable;
                                                mastList[0].total = item.total;
                                                mastList[0].totalqty = item.totalpiece;
                                                mastList[0].mastercode = mastList[0].code;
                                                mastList[0].billno = item.billno;
                                                mastList[0].code = item.billno;
                                                bw.Write(Convert.FromBase64String(Print(mastList[0])));
                                                document = ms.ToArray();
                                            }
                                            item.printdocument = Convert.ToBase64String(document);
                                            item.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, item.locationid, item);
                                        }
                                    }
                                }
                            }
                            catch (Exception exp)
                            {
                                WebAPI.Common.Logger.Instance(BillData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                                return this.Ok(exp);
                            }
                        }
                    }
                }

                if (result.Count > 0 && result.Exists(e => e.Billitems.Exists(a => a.schemeid > 0)))
                {
                    foreach (var _item in result)
                    {
                        // Added scheme discount which is removed in Search Method. 
                        _item.discount -= _item.schemediscount;
                    }
                }

                if (request.Item.forsettlement && result.Count == 1)
                {
                    BillsettlementDataService settlementdata = new BillsettlementDataService();
                    settlementdata.Context = BillData.Context;
                    var settlemtnitems = settlementdata.Search(new BillsettlementCreteria()
                    {
                        billid = result[0].id
                    });
                    if (settlemtnitems.Count > 0)
                    {
                        result[0].receivable = result[0].receivable - settlemtnitems.Sum(e => e.isactive ? 0 : e.paid);
                    }
                }

                if (request.Item.isreloadsalebill && result.Count == 1 &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig != null &&
    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_disableloadsalebill)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_disableloadsalebillacross
                        && result[0].billtype != Bill.BillTypes.ReturnBill)
                    {
                        DateTime noreload = InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_reloadsalebillfreezeon;
                        if (noreload == DateTime.MinValue || noreload.Year < 2020)
                            return this.Ok(new Exception("Don't have permission to load sale bills"));
                        else if (noreload.Date > result[0].billdate.Date)
                            return this.Ok(new Exception("Don't have permission to load sale bills"));
                    }
                    else if (!request.Item.isbulkreturn && result[0].billtype != Bill.BillTypes.ReturnBill)
                        return this.Ok(new Exception("Don't have permission to load sale bills"));
                }
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BillData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("SearchSalesReport")]
        public async Task<IHttpActionResult> SearchSalesReport(ActionRequest<SalesReportCreteria> request)
        {
            BillDataService BillData = new BillDataService();
            List<SalesReport> result = await Task<List<SalesReport>>.Run(() => BillData.SearchSalesReport(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ItemSearch")]
        public async Task<IHttpActionResult> ItemSearch(ActionRequest<SalesReportCreteria> request)
        {
            BillDataService BillData = new BillDataService();
            List<SalesReport> result = await Task<List<SalesReport>>.Run(() => BillData.ItemSearch(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("SaveSettlement")]
        //[WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Create)]
        //public async Task<IHttpActionResult> SaveSettlement(ActionRequest<BillInfo> request)
        //{
        //    BillsettlementmasterDataService BillData = new BillsettlementmasterDataService();
        //    bool result = await Task<bool>.Run(() => BillData.UpdateSettlement(request.Item));
        //    return this.Ok(result);
        //}


        [HttpPost]
        [Route("SaveTouchBill")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> SaveTouchBill(ActionRequest<Bill> request)
        {
            BillDataService BillData = new BillDataService();
            request.Item = BillData.PrepareTouchBill(request.Item);
            return await Save(request);
        }


        [HttpPost]
        [Route("UpdateAttributes")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> UpdateAttributes(ActionRequest<Bill> request)
        {
            BillDataService BillData = new BillDataService();
            Bill bill = await Task<Bill>.Run(() => BillData.UpdateAttributes(request.Item));
            return this.Ok(bill);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Bill> request)
        {
            /*var totalqty = request.Item.totalqty;
            var totalpiece = request.Item.totalpiece;
            var cqty = request.Item.Billitems.Sum(x => x.qty);
            var cpiece = request.Item.Billitems.Sum(x => x.piece);
            var mismatch = request.Item.Billitems.FindAll(x => x.piece != x.qty);*/

            BillDataService BillData = new BillDataService();
            if (request.Item.id <= 0)
            {
                request.Item.Billitems.ForEach(e =>
                {
                    e.id = 0;
                    e.billid = 0;
                });

                if (request.Item.locationid <= 0 || request.Item.counterid <= 0)
                {
                    throw new Exception("Please select the location/counter.");
                }

                if (request.Item.Billitems == null || request.Item.Billitems.Count == 0)
                    throw new Exception("Please enter minimum one item");

                if (request.Item.locationid > 0 &&
                request.Item.Billitems.Any(e => e.companyid == 0 || e.salablegoodsid == 0))
                {
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService();
                    var buglist = request.Item.Billitems.Where(e => e.companyid == 0 || e.salablegoodsid == 0).ToList();
                    var existinggoods = salablegoodsDataService.Search(new SalablegoodsCreteria()
                    {
                        locationid = request.Item.locationid,
                        barcodes = buglist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'")
                    });
                    foreach (var item in buglist)
                    {
                        if (existinggoods.Exists(e => e.barcode == item.barcode))
                        {
                            var bitem = existinggoods.Find(e => e.barcode == item.barcode);
                            item.companyid = bitem.companyid;
                            item.salablegoodsid = bitem.id;
                        }
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_multicompany &&
                   request.Item.Billitems.Any(e => e.companyid == 0 || e.salablegoodsid == 0))
                {
                    throw new Exception("Some of the barcode don't have company information..");
                }

                if (request.Item.Billitems.Any(e => e.totaldiscountpercentage < 0))
                    throw new Exception("Bill has negative discount.");


                if (request.Item.additionalchargeslist != null &&
                    request.Item.additionalchargeslist.Exists(x => x.chargetype == "EXPORTGST") &&
                    request.Item.isexport == false)
                {
                    throw new Exception("Export GST added,this bill should be marked as export");
                }

                bool splitCompany = InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_multicompany;
                if (request.Item.billtype == Bill.BillTypes.Bulk &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_supportdc)
                    splitCompany = false;

                Billmaster billMaster = new Billmaster(BillData.Context.OrganizationID, request.Item,
                    splitCompany, BillData.DateTimeNow);


                if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_salesrounding == 99)
                {
                    billMaster.total -= billMaster.rounding;
                    foreach (var bill in billMaster.Bills)
                    {
                        bill.rounding = 0;
                    }
                }


                if (billMaster.Bills.Any(e => e.billtype == Bill.BillTypes.Cancel))
                {
                    throw new Exception("Can't create bills with cancel mode.");
                }
                if (splitCompany && billMaster.Bills.Count > 1)
                {
                    // COMMENTED THIS CODE BECAUSE THE SPLIT AND ALLOCATION OF REWARD AMOUNT IS DONE AT BILLMASTER INSERT METHOD

                    //if (request.Item.billattributes != null && request.Item.billattributes.rewardamount > 0)
                    //{
                    //    if (billMaster.Bills.Exists(e => e.receivable > request.Item.billattributes.rewardamount))
                    //    {
                    //        var rewardamount = request.Item.billattributes.rewardamount;
                    //        billMaster.Bills.ForEach(e => e.billattributes.rewardamount = 0);

                    //        Bill _found = billMaster.Bills.Find(e => e.receivable > request.Item.billattributes.rewardamount);
                    //        BillAttribute attrib = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>
                    //                                (Newtonsoft.Json.JsonConvert.SerializeObject(_found.billattributes));
                    //        attrib.rewardamount = rewardamount;
                    //        _found.billattributes = attrib;
                    //    }
                    //}
                }

                if (billMaster.locationid == 0)
                    billMaster.locationid = billMaster.Bills.First().locationid;

                if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledatefreeze &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledatefreezeon > DateTime.MinValue)
                {
                    DateTime billdate = InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledatefreezeon;
                    billMaster.Bills.ForEach(e => e.billdate = billdate.Date);
                }
                else if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_datechange &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].currentday == BillData.DateTimeNow.Date &&
                        InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].nextdaycount > 0)
                    {
                        DateTime billdate = BillData.DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].nextdaycount);
                        billMaster.Bills.ForEach(e => e.billdate = billdate);
                    }
                }

                if (billMaster.Bills != null)
                {
                    foreach (var item in billMaster.Bills)
                    {
                        item.isb2b = request.Item.isb2b;
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                        if (item.Billitems != null)
                        {
                            if (item.billtype == Bill.BillTypes.ReturnBill && item.Billitems.Exists(e => e.receivable > 0))
                            {
                                throw new Exception("return bill can't contines positive values, please try again ");
                            }
                            item.Billitems.ForEach(e => e.ChangeStatus(BaseEntity.DataStatus.Create));
                        }
                    }
                }

                BillmasterDataService BillMasterData = new BillmasterDataService();
                if (request.Item.isbulkreturn)
                {
                    if (string.IsNullOrEmpty(request.Item.returnbillno) || request.Item.billtype == Bill.BillTypes.Bulk || request.Item.billtype == Bill.BillTypes.Estimate)
                        billMaster = await Task<Billmaster>.Run(() => BillMasterData.InsertBulkBill(billMaster, request.Item.returnbillno));
                    else
                        billMaster = await Task<Billmaster>.Run(() => BillMasterData.Insert(billMaster, request.Item.returnbillno));
                }
                else
                {
                    billMaster = await Task<Billmaster>.Run(() => BillMasterData.Insert(billMaster));
                }

                if (billMaster.Bills.Any(e => e.id <= 0))
                {
                    return this.Ok(billMaster);
                }
                billMaster.Bills.ForEach(e =>
                {
                    e.directpdf = request.Item.directpdf;
                    e.papersize = request.Item.papersize;
                    e.printtemplateid = request.Item.printtemplateid;
                });

                bool ispdfprint = !request.Item.directdos && (billMaster.Bills[0].isb2b || billMaster.Bills[0].directpdf)
                    && (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledirectpdf);

                if (request.Item.isb2b &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_b2bsaledirectpdf)
                    ispdfprint = true;

                if (ispdfprint)
                {
                    try
                    {
                        billMaster.directpdf = true;
                        if (billMaster.Bills.Count == 1)
                        {
                            var billprint = PrintBill(BillData.Context.OrganizationID, billMaster.Bills[0], true);
                            Byte[] byteArray = billprint.Content.ReadAsByteArrayAsync().Result;
                            billMaster.printdocument = Convert.ToBase64String(byteArray);

                            billMaster.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, request.Item.locationid, billMaster.Bills[0]);

                        }
                        else
                        {
                            foreach (var bill in billMaster.Bills)
                            {
                                bill.printtemplateid = request.Item.printtemplateid;
                                bill.directpdf = request.Item.directpdf;
                                bill.papersize = request.Item.papersize;
                                var billprint = PrintBill(BillData.Context.OrganizationID, bill, true);
                                Byte[] byteArray = billprint.Content.ReadAsByteArrayAsync().Result;
                                bill.directpdf = true;
                                bill.printdocument = Convert.ToBase64String(byteArray);
                            }
                        }
                    }
                    catch (Exception exp)
                    {
                        billMaster.errormessage = exp.Message;
                    }
                }
                else
                {
                    int noofcopy = 0;
                    long lastid = billMaster.Bills.Last().id;
                    long firstid = billMaster.Bills.First().id;

                    if (request.Item.billtype == Bill.BillTypes.Bulk && InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_supportdc)
                    {
                        billMaster.Bills[0].printdocument = Print(BillData.Context.OrganizationID, billMaster.Bills[0], true, true, false, out noofcopy);
                        billMaster.Bills[0].billcopy = ProcessBillCopy(BillData.Context.OrganizationID, billMaster.Bills[0].locationid, billMaster.Bills[0]);
                    }
                    else
                    {
                        if (!InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_multicompany)
                        {
                            billMaster.Bills[0].printdocument = Print(BillData.Context.OrganizationID, billMaster.Bills[0], true, true, false, out noofcopy);
                            billMaster.Bills[0].billcopy = ProcessBillCopy(BillData.Context.OrganizationID, billMaster.Bills[0].locationid, billMaster.Bills[0]);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(BillMasterData.Context.OrganizationID).LocationConfig[billMaster.Bills[0].locationid].sale_billcopytitle) &&
                        InfyPOS.Services.ConfigManager.Instance(BillMasterData.Context.OrganizationID).LocationConfig[billMaster.Bills[0].locationid].sale_billcopytitle.IndexOf(',') > 0 &&
                        billMaster.Bills.First().billtype != Bill.BillTypes.ReturnBill)
                            {
                                billMaster.Bills.ForEach(e => e.printdocument = "");
                                var fixedtitles = InfyPOS.Services.ConfigManager.Instance(BillMasterData.Context.OrganizationID).LocationConfig[billMaster.Bills[0].locationid].sale_billcopytitle.Split(',');
                                List<string> printdocumentlist = new List<string>();
                                for (int i = 0; i < fixedtitles.Length; i++)
                                {
                                    for (var j = 0; j < billMaster.Bills.Count; j++)
                                    {
                                        var item = billMaster.Bills[j];
                                        if (item.id > 0)
                                        {
                                            item.showfixeddisplay = true;
                                            item.fixedbilldisplay = fixedtitles[i];
                                            //bool islast = (lastid == item.id && !InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint;
                                            printdocumentlist.Add(Print(BillData.Context.OrganizationID, item,
                                               false,
                                                firstid == item.id, billMaster.Bills.Count > 1, out noofcopy));
                                            if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint &&
                                                j == billMaster.Bills.Count - 1)
                                            {
                                                if (i != fixedtitles.Length - 1)
                                                {
                                                    string sdocument = Print(billMaster);
                                                    if (!string.IsNullOrEmpty(sdocument))
                                                        printdocumentlist.Add(sdocument);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            item.billno = "";
                                        }
                                    }
                                }
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                {
                                    System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                                    foreach (var bitem in printdocumentlist)
                                    {
                                        if (!string.IsNullOrEmpty(bitem))
                                            bw.Write(Convert.FromBase64String(bitem));
                                    }
                                    billMaster.Bills.First().printdocument = Convert.ToBase64String(ms.ToArray());
                                    billMaster.Bills[0].billcopy = ProcessBillCopy(BillData.Context.OrganizationID, billMaster.Bills[0].locationid, billMaster.Bills[0]);
                                }

                            }
                            else
                            {
                                foreach (var item in billMaster.Bills)
                                {
                                    if (item.id > 0)
                                    {
                                        item.printdocument = Print(BillData.Context.OrganizationID, item, (lastid == item.id && !InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint),
                                            firstid == item.id,
                                            (billMaster.Bills.Count > 1 || InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint), out noofcopy);
                                        item.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, item.locationid, item);
                                    }
                                    else
                                    {
                                        item.billno = "";
                                    }
                                }
                            }
                        }


                        if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_mergebillprint)
                        {
                            //if (billMaster.Bills.First().billtype != Bill.BillTypes.ReturnBill) 
                            if (billMaster.Bills.First().billtype != Bill.BillTypes.ReturnBill
                                || InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_exchangebillsummaryprintformat)
                            {
                                billMaster.mastercode = billMaster.code;
                                bool hasduplicate = false;
                                //billMaster.totalqty = billMaster.Bills.Sum(e => e.Billitems.Sum(x => x.sellingmode == 3 ? 1 : x.qty));
                                billMaster.totalqty = billMaster.Bills.Sum(e => e.totalpiece);
                                byte[] document = null;
                                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                {
                                    System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                                    foreach (var bitem in billMaster.Bills)
                                    {
                                        if (!string.IsNullOrEmpty(bitem.printdocument))
                                            bw.Write(Convert.FromBase64String(bitem.printdocument));
                                    }
                                    if (billMaster.billattributes == null)
                                        billMaster.billattributes = billMaster.Bills.First().billattributes;
                                    string sdocument = Print(billMaster);
                                    
                                    if (!string.IsNullOrEmpty(sdocument))
                                    {
                                        var sdocarray = Convert.FromBase64String(sdocument);
                                        var encoding = Encoding.GetEncoding("ISO-8859-1");
                                        hasduplicate = encoding.GetString(sdocarray).Contains("REMOVE DUPLICATE");
                                        bw.Write(sdocarray);
                                    }
                                    document = ms.ToArray();
                                }

                                if (noofcopy > 1 && !billMaster.Bills.All(e => e.billtype == Bill.BillTypes.ReturnBill))
                                {
                                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                    {
                                        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                                        bw.Write(document);
                                        int c = 1;
                                        while (c < noofcopy)
                                        {
                                            bw.Write(document);
                                            c++;
                                        }
                                        if (ms.Length > 0)
                                        {
                                            if (hasduplicate)
                                            {
                                                var encoding = Encoding.GetEncoding("ISO-8859-1");
                                                var sdocument = encoding.GetString(ms.ToArray());

                                                if (sdocument.Contains("REMOVE DUPLICATE"))
                                                {
                                                    document = encoding.GetBytes(RemoveDuplicateBlocks(sdocument));
                                                }
                                                billMaster.printdocument = Convert.ToBase64String(document);
                                            }
                                            else
                                            {
                                                billMaster.printdocument = Convert.ToBase64String(ms.ToArray());
                                            }
                                        }
                                        billMaster.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, request.Item.locationid, billMaster.Bills[0]);
                                    }
                                }
                                else if (noofcopy > 1 && billMaster.Bills.All(e => e.billtype == Bill.BillTypes.ReturnBill))
                                {
                                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                                    {
                                        System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                                        bw.Write(document);
                                        int c = 1;
                                        while (c < noofcopy)
                                        {
                                            bw.Write(document);
                                            c++;
                                        }

                                        if (ms.Length > 0)
                                        {
                                            if (hasduplicate)
                                            {
                                                var encoding = Encoding.GetEncoding("ISO-8859-1");
                                                var sdocument = encoding.GetString(ms.ToArray());
                                                
                                                if (sdocument.Contains("REMOVE DUPLICATE"))
                                                {
                                                    document = encoding.GetBytes(RemoveDuplicateBlocks(sdocument));
                                                }
                                                billMaster.printdocument = Convert.ToBase64String(document);
                                            }
                                            else
                                            {
                                                billMaster.printdocument = Convert.ToBase64String(ms.ToArray());
                                            }
                                        }

                                        billMaster.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, request.Item.locationid, billMaster.Bills[0]);
                                    }
                                }
                                else
                                {
                                    billMaster.printdocument = document != null && document.Length > 0 ? Convert.ToBase64String(document) : "";
                                    billMaster.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, request.Item.locationid, billMaster.Bills[0]);
                                }
                            }
                        }
                    }
                }

                //Bill Based SMS
                if (!InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.intray_notification_bill)
                {
                    Processors.Schedule_Actions.Instance.BillNotificationDisabled();
                    if (billMaster.Bills.All(e => e.id > 0))
                    {
                        foreach (var bill in billMaster.Bills)
                        {
                            long mb;
                            if (bill.customer != null && !string.IsNullOrEmpty(bill.customer.mobileno) && long.TryParse(bill.customer.mobileno, out mb))
                            {
                                if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.intray_notification_crm)
                                    Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.CRM);
                            }
                        }
                    }
                }
                else if (billMaster.Bills.All(e => e.id > 0))
                {
                    foreach (var bill in billMaster.Bills)
                    {
                        long mb;
                        if (bill.customer != null && !string.IsNullOrEmpty(bill.customer.mobileno) && long.TryParse(bill.customer.mobileno, out mb))
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.intray_notification_crm)
                                Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.CRM);
                        }

                        if (bill.billtype == Bill.BillTypes.ReturnBill || (bill.billtype == Bill.BillTypes.Credit && bill.receivable < 0))
                        {
                            Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Return);
                        }
                        else if (bill.billtype == Bill.BillTypes.Cancel)
                        {
                            Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Cancelled);
                        }
                        else if (bill.billtype == Bill.BillTypes.Credit)
                        {
                            Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Credit);
                            if (bill.additionaldiscount > 0)
                            {
                                Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Discounted);
                            }
                        }
                        else if (bill.billtype == Bill.BillTypes.Cash && bill.returnbillno != null && bill.returnbillno.StartsWith("~"))
                        {
                            Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Exchange);
                            if (bill.additionaldiscount > 0)
                            {
                                Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Discounted);
                            }
                        }
                        else if (bill.billtype == Bill.BillTypes.Cash)
                        {
                            Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.New);
                            if (bill.additionaldiscount > 0)
                            {
                                Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Discounted);
                            }
                        }

                    }
                }
                /*billMaster.Bills.ForEach(e => e.billmasterid = billMaster.id);
                foreach (var bill in billMaster.Bills)
                {
                    Bill result = await Task<Bill>.Run(() => BillData.Insert(bill));
                }*/
                return this.Ok(billMaster);
            }
            else
            {

                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Billitems != null)
                    request.Item.Billitems.ForEach(e => e.ChangeStatus(BaseEntity.DataStatus.Update));

                Bill result = await Task<Bill>.Run(() => BillData.Update(request.Item));


                bool ispdfprint = !request.Item.directdos && (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.system_saledirectpdf
                    || result.isb2b
                    || result.directpdf
                    || (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_saledirectpdf));

                if (request.Item.isb2b &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).LocationConfig[request.Item.locationid].system_b2bsaledirectpdf)
                    ispdfprint = true;

                if (ispdfprint)
                {
                    try
                    {
                        var billprint = PrintBill(BillData.Context.OrganizationID, result, true);
                        Byte[] byteArray = billprint.Content.ReadAsByteArrayAsync().Result;
                        result.printdocument = Convert.ToBase64String(byteArray);
                        result.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, result.locationid, result);
                    }

                    catch (Exception exp)
                    {
                        result.errormessage = exp.Message;
                    }
                }
                else
                {
                    int noofcopy = 0;
                    result.printdocument = Print(BillData.Context.OrganizationID, result, true, true, false, out noofcopy);
                    result.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, result.locationid, result);
                }
                return this.Ok(result);
            }
        }

        public Bill.BillCopy ProcessBillCopy(long organizationid, long locationid, Bill bill)
        {
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(locationid) &&
                               InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].billcopymode != Models.Configuration.ApplicationSettings.billcopymodes.None)
            {
                var destination = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].billcopydestination;
                if (string.IsNullOrEmpty(destination))
                    destination = "";
                if (bill.location != null)
                {
                    destination = destination.Replace("[LOCATIONCODE]", bill.location.name.Replace(" ", ""));
                }
                else
                {
                    destination = destination.Replace("[LOCATIONCODE]", "");
                }
                if (bill.company != null)
                {
                    destination = destination.Replace("[COMPANYCODE]", bill.company.name.Replace(" ", ""));
                }
                else
                {
                    destination = destination.Replace("[COMPANYCODE]", "");
                }
                return bill.PrepareBillCopy(new Bill.BillCopy()
                {
                    destination = destination,
                    mode = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[locationid].billcopymode.ToString()
                });
            }
            return null;
        }

        [HttpPost]
        [Route("FindOffers")]
        public async Task<IHttpActionResult> FindOffers(ActionRequest<Bill> request)
        {
            BillDataService BillData = new BillDataService();
            var billMaster = await Task<Bill>.Run(() => BillData.FindOffers(request.Item));
            return this.Ok(billMaster);
        }


        [HttpPost]
        [Route("Cancellation")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Cancellation(ActionRequest<BillCancel> request)
        {
            if (request.Item.isbulksale || request.Item.isbulkreturn)
            {
                BulkBillDataService BillMasterData = new BulkBillDataService();
                List<Bill> bills = await Task<List<Bill>>.Run(() => BillMasterData.Cancellation(request));

                return this.Ok(bills);
            }
            else
            {
                BillDataService BillMasterData = new BillDataService();
                List<Bill> bills = await Task<List<Bill>>.Run(() => BillMasterData.Cancellation(request));
                return this.Ok(bills);
            }
        }

        [HttpPost]
        [Route("CancelEstimate")]
        [WebAPI.InfyAuthorize(PermissionSet.estimatesale, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> CancelEstimate(ActionRequest<BillCancel> request)
        {
            BulkBillDataService BillMasterData = new BulkBillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillMasterData.Cancellation(request));
            return this.Ok(bills);
        }
        [HttpPost]
        [Route("UpdateCustomer")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateCustomer(ActionRequest<CustomerChange> request)
        {
            BillDataService BillData = new BillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillData.UpdateCustomer(request.Item));

            return this.Ok(bills);
        }
        [HttpPost]
        [Route("UpdateBillDate")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateBillDate(ActionRequest<CustomerChange> request)
        {
            BillDataService BillData = new BillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillData.UpdateBillDate(request.Item));

            return this.Ok(bills);
        }
        [HttpPost]
        [Route("UpdateBillasCredit")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateBillasCredit(ActionRequest<Bill> request)
        {
            BillDataService BillData = new BillDataService();
            Bill obill = await Task<Bill>.Run(() => BillData.UpdateBillasCredit(request.Item));

            return this.Ok(obill);
        }
        [HttpPost]
        [Route("UpdateIGST")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateIGST(ActionRequest<CustomerChange> request)
        {
            BillDataService BillData = new BillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillData.UpdateIGST(request.Item));

            return this.Ok(bills);
        }
        [HttpPost]
        [Route("UpdateHSN")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateHSN(ActionRequest<HSNChange> request)
        {
            BillDataService BillData = new BillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillData.UpdateHSN(request.Item));

            return this.Ok(bills);
        }
        [HttpPost]
        [Route("UpdateBillHSN")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateBillHSN(ActionRequest<CustomerChange> request)
        {
            BillDataService BillData = new BillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillData.UpdateBillHSN(request.Item));

            return this.Ok(bills);
        }
        [HttpPost]
        [Route("UpdateSalesman")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateSalesman(ActionRequest<CustomerChange> request)
        {
            BillDataService BillData = new BillDataService();
            List<Bill> bills = await Task<List<Bill>>.Run(() => BillData.UpdateSalesman(request.Item));

            return this.Ok(bills);
        }

        [HttpPost]
        [Route("PromotionExchange")]
        public async Task<IHttpActionResult> PromotionExchange(ActionRequest<PromotionExchange> request)
        {
            BillDataService BillData = new BillDataService();
            PromotionExchange bills = await Task<PromotionExchange>.Run(() => BillData.PromotionExchange(request.Item));
            return this.Ok(bills);
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Bill> request)
        {
            if (request.Item.additionaldiscount > 0)
                request.Item.discountstatus = Discountqueue.DiscountStatus.Requested;
            BillDataService BillData = new BillDataService();
            Bill result = await Task<Bill>.Run(() => BillData.Insert(request.Item, new Billmaster()
            {
                id = request.Item.billmasterid,
                total = request.Item.total
            }));

            if (result.id > 0)
            {
                int noofcopy = 0;
                result.printdocument = Print(BillData.Context.OrganizationID, result, true, true, false, out noofcopy);
                result.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, result.locationid, result);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Bill> request)
        {
            BillDataService BillData = new BillDataService();
            Bill result = await Task<Bill>.Run(() => BillData.Update(request.Item));
            if (result.id > 0)
            {
                int noofcopy = 0;
                result.printdocument = Print(BillData.Context.OrganizationID, result, true, true, false, out noofcopy);
                result.billcopy = ProcessBillCopy(BillData.Context.OrganizationID, result.locationid, result);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.sales, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Bill> request)
        {
            if (request.Item.isbulkreturn)
            {
                BulkBillDataService BillData = new BulkBillDataService();
                bool result = await Task<bool>.Run(() => BillData.Delete(request.Item));
                return this.Ok(result);
            }
            else
            {
                BillDataService BillData = new BillDataService();
                bool result = await Task<bool>.Run(() => BillData.Delete(request.Item));
                return this.Ok(result);
            }
        }

        public string Print(Billmaster source)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = InfyPOS.Services.ConfigManager.Instance(config.Context.OrganizationID).Config.system_mergebillprint &&
                    (source.Bills == null || source.Bills.Count == 1) ? "SALESUMMARYSINGLERECEIPT" : "SALESUMMARYRECEIPT",
                locationid = source.locationid,
                organizationid = 0
            });

            if (reportformats.Count == 0)
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = InfyPOS.Services.ConfigManager.Instance(config.Context.OrganizationID).Config.system_mergebillprint &&
                    (source.Bills == null || source.Bills.Count == 1) ? "SALESUMMARYSINGLERECEIPT" : "SALESUMMARYRECEIPT",
                    organizationid = 0
                });

            if (reportformats.Count > 1)
            {
                var papersize = source.Bills.First().papersize;
                if (reportformats.Exists(e => e.nooflines == papersize))
                {
                    reportformats = reportformats.FindAll(e => e.nooflines == papersize);
                }
            }

            if (source.customerid == 0 && source.Bills[0].customerid>0)
            {
                source.customerid = source.Bills[0].customerid;
                source.customergst = source.Bills[0].customergst;
                source.customername = source.Bills[0].customername;
                source.customermobileno = source.Bills[0].phoneno;
            }

            if (reportformats.Count > 0)
            {
                try
                {
                    var additional = new SortedDictionary<string, object>();
                    var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = config.Context.OrganizationID } };
                    ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                    ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(imageData.Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                    labreport.format = ReportLibrary.DosFormatParser.ParseXML(imageData.Context.OrganizationID, reportformats[0].reportformat.format);

                    if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("HasFreeGift"))
                    {
                        source.FreeItems = new List<BillItemAttributes.FreeItems>();
                        source.Bills.ForEach(z =>
                        {
                            z.Billitems.ForEach(x =>
                            {
                                if (x.attributes != null &&
                                   x.attributes.freeitems != null &&
                                   x.attributes.freeitems.Count > 0)
                                    source.FreeItems.AddRange(x.attributes.freeitems);
                            });
                        });
                    }
                    var organizationid = config.Context.OrganizationID;
                    if (source.hassettlement &&
                        source.customerid > 0 &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryenable &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount > 0)
                    {
                        if (source.billattributes == null)
                            source.billattributes = new BillAttribute();

                        if (source.billattributes.settlement == null)
                            source.billattributes.settlement = new Billsettlementmaster();

                        if (source.billattributes.settlement.receivable == 0)
                            source.billattributes.settlement.receivable = source.Bills.Sum(x => x.receivable);

                        if (source.billattributes.settlement.settlementon == DateTime.MinValue)
                            source.billattributes.settlement.settlementon = source.billdate;

                        if (reportformats[0].reportformat.format != null &&
                                                reportformats[0].reportformat.format.Contains("Lottery_") &&
                                                source.billattributes.settlement.receivable >= InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount)
                        {
                            var noofcoupons = Math.Floor(source.billattributes.settlement.receivable /
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount);

                            if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax > 0 &&
                                InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax < noofcoupons)
                                noofcoupons = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax;
                            source.billattributes.settlement.Lottery_Avilable = true;
                            source.billattributes.settlement.Lottery_Count = Convert.ToInt32(noofcoupons);
                            source.billattributes.settlement.customer = source.Bills[0].customer;
                        }
                    }

                    List<string> imageids = labreport.format.GetImages();
                    labreport.Additional = new SortedDictionary<string, object>();
                    foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                    {
                        var file = imageData.GetData(imgid.LongFromBase62());
                        if (file != null && file.data != null)
                        {
                            additional.Add("IMG-" + imgid, file.data);
                        }
                    }

                    var document = labreport.format.RenderBytes(new ImageData(), source, source.Bills, additional, true);
                    return Convert.ToBase64String(document);
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(config.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    throw exp;
                }
            }
            return "";
        }

        public string Print(long organizationid, Bill source, bool isLast, bool isFirst, bool ismultibill, out int noofcopy)
        {
            noofcopy = 1;
            if (source.billtype == Bill.BillTypes.Cancel)
                throw new Exception("Cancelled bill can't print");

            var Context = new UserClaim() { OrganizationID = organizationid };
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].system_billprintcutter)
                isLast = true;

            source.greetingtext = !string.IsNullOrEmpty(Services.ConfigManager.Instance(organizationid).Config.sale_greetingtext) ? Services.ConfigManager.Instance(organizationid).Config.sale_greetingtext :
                (Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) ? Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_greetingtext : "");

            List<Billitems> actualBillitems = null;
            if (source.taxsplit == null)
            {
                source.taxsplit = new TaxSplit();
                source.taxsplit.totalamount = source.Billitems.Sum(e => e.rate * e.qty);
                if (source.isinterstatesale)
                {
                    source.taxsplit.cgsttaxamount = 0;
                    source.taxsplit.sgsttaxamount = source.tax;
                    source.taxsplit.secondtax = "IGST";
                    source.Billitems.ForEach(e =>
                    {
                        e.taxsplit = new TaxSplit()
                        {
                            cgsttaxamount = 0,
                            cgsttaxpercentage = 0,
                            secondtax = "IGST",
                            sgsttaxamount = e.tax,
                            sgsttaxpercentage = e.taxpercentage
                        };
                    });
                }
                else
                {
                    source.taxsplit.cgsttaxamount = Math.Round(source.tax / 2, 5);
                    source.taxsplit.sgsttaxamount = Math.Round(source.tax / 2, 5);
                    source.taxsplit.secondtax = "SGST";
                    source.Billitems.ForEach(e =>
                    {
                        e.taxsplit = new TaxSplit()
                        {
                            cgsttaxamount = Math.Round(e.tax / 2, 5),
                            cgsttaxpercentage = e.taxpercentage / 2,
                            secondtax = "SGST",
                            sgsttaxamount = Math.Round(e.tax / 2, 5),
                            sgsttaxpercentage = e.taxpercentage / 2
                        };
                    });
                }
                source.BillTax.ForEach(e =>
                {
                    e.secondtax = source.taxsplit.secondtax;
                });
            }

            List<BillTax> oldBillTax = null;
            if (source.BillTax.GroupBy(e => e.taxpercentage).Count() < source.BillTax.Count)
            {
                oldBillTax = source.BillTax;
                var billtax = new List<BillTax>();
                foreach (var tax in source.BillTax.GroupBy(e => e.taxpercentage))
                {
                    billtax.Add(new BillTax()
                    {
                        amount = tax.Sum(e => e.amount),
                        tax = tax.Sum(e => e.tax),
                        taxpercentage = tax.Key,
                        secondtax = tax.First().secondtax,
                        taxname = tax.First().taxname
                    });
                }
                source.BillTax = billtax;
            }

            InfyPOS.Data.CompanyDataService company = new CompanyDataService();
            var companyforlogo = company.Search(new CompanyCreteria
            {
                id = source.companyid
            });

            var printConfig = "SALERECEIPT";
            if (source.isb2b)
                printConfig = "SALEINVOICE";

            if ((source.billtype == Bill.BillTypes.ReturnBill || (source.billtype == Bill.BillTypes.Credit &&
                    source.receivable < 0)) &&
                InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillprintformat)
            {
                printConfig = "SALERETURN";
            }

            if (source.billtype == Bill.BillTypes.Bulk &&
                Services.ConfigManager.Instance(organizationid).Config.system_supportdc)
            {
                printConfig = "DELIVERYCHALLAN";
            }

            if (source.billtype == Bill.BillTypes.Estimate &&
                Services.ConfigManager.Instance(organizationid).Config.system_supportestimate)
            {
                printConfig = "ESTIMATEBILL";
            }

            if (source.billattributes != null &&
                source.billattributes.touchsale)
            {
                printConfig = "TOUCHRECEIPT";
            }

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = printConfig,
                locationid = source.locationid,
                organizationid = source.companyid,
                supportinstance = true
            });

            if (reportformats.Count == 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = printConfig,
                    locationid = source.locationid,
                    organizationid = source.companyid,
                    supportinstance = false
                });
            }

            if (reportformats.Count == 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = printConfig,
                    locationid = 0,
                    organizationid = source.companyid,
                    supportinstance = true
                });
            }

            if (reportformats.Count == 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = printConfig,
                    locationid = source.locationid,
                    organizationid = 0,
                    supportinstance = false
                });
            }

            if (source.printtemplateid > 0)
            {
                var printtemplates = config.Search(new ConfigurationCreteria
                {
                    id = source.printtemplateid
                });
                if (printtemplates.Count == 1)
                {
                    if (reportformats.Count == 0)
                    {
                        reportformats.AddRange(printtemplates);
                    }
                    else if (printtemplates.First().key == printConfig)
                    {
                        reportformats.Clear();
                        reportformats.AddRange(printtemplates);
                    }
                }
            }

            if (reportformats.Count > 0)
            {
                if (reportformats.Count > 1)
                {
                    if (reportformats.Exists(e => e.nooflines == source.papersize))
                    {
                        reportformats = reportformats.FindAll(e => e.nooflines == source.papersize);
                    }
                }
                CompanyDataService companyDataService = new CompanyDataService();
                var companylist = companyDataService.Search(new CompanyCreteria()
                {
                    id = source.companyid
                });
                if (companylist.Count == 0)
                {
                    companylist = companyDataService.Search(new CompanyCreteria()
                    {
                    });
                    if (companylist.Count == 1)
                    {
                        source.company = companylist[0];
                        source.company.parseaddress();
                    }
                    else
                    {
                        source.company = new Company();
                        source.company.name = "Name missing in bill";
                        source.company.address = "";
                        source.company.parseaddress();
                    }
                }
                else
                {
                    source.company = companylist[0];
                    source.company.parseaddress();
                }
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = source.locationid
                }).First();

                source.location = new CompanyLocation(location);
                source.location.name = location.name;
                source.location.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
                source.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                source.location.pincode = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.pincode;
                source.location.addresslines = new List<CompanyLocation.Address>();
                if (!string.IsNullOrEmpty(source.location.address))
                {
                    var addresslist = source.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        source.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }
                if (!string.IsNullOrEmpty(source.location.address))
                {
                    var addresslist = source.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        source.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }

                if (source.counterid > 0 && string.IsNullOrEmpty(source.countername))
                {
                    var counterinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = source.counterid
                    });
                    if (counterinfo.Count > 0)
                        source.countername = counterinfo[0].name;
                }

                if (source.billattributes != null && source.billattributes.settlement != null)
                {
                    if (source.billattributes.settlement.paymentinfo != null && source.billattributes.settlement.paymentinfo.card_bankid > 0)
                    {
                        var bank = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = source.billattributes.settlement.paymentinfo.card_bankid
                        }).First();
                        source.billattributes.settlement.paymentinfo.card_bankname = bank.name;
                    }
                    if (source.billattributes.settlement.paymentinfo != null && source.billattributes.settlement.paymentinfo.upiproviderid > 0)
                    {
                        var upi = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = source.billattributes.settlement.paymentinfo.upiproviderid
                        }).First();
                        source.billattributes.settlement.paymentinfo.upiprovidername = upi.name;
                    }
                }

                if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                    && location.additionalinfo.locationconfig.cityid > 0)
                {
                    var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = location.additionalinfo.locationconfig.cityid
                    });
                    if (cityinfo.Count > 0)
                        source.location.cityname = cityinfo[0].name;
                }
                if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                    && location.additionalinfo.locationconfig.stateid > 0)
                {
                    var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = location.additionalinfo.locationconfig.stateid
                    });
                    if (stateinfo.Count > 0)
                        source.location.statename = stateinfo[0].name;
                }
                if (source.hascustomer)
                {
                    if (source.customer == null)
                    {
                        CustomerDataService customerDataService = new CustomerDataService();
                        source.customer = customerDataService.Search(new CustomerCreteria()
                        {
                            id = source.customerid
                        }).First();

                        if (source.customer.stateid > 0)
                        {
                            source.customer.state = referencelistDataService.Search(new ReferencelistCreteria()
                            {
                                id = source.customer.stateid
                            }).First();
                        }



                        if (source.Billitems.Exists(x => x.attributes != null &&
                            x.attributes.so != null && x.attributes.so.Count > 0))
                        {
                            List<long> soitemids = new List<long>();
                            source.Billitems.ForEach(x =>
                            {
                                foreach (var soitem in x.attributes.so)
                                    soitemids.Add(soitem.id);
                            });
                            SalesorderitemsDataService salesorderitemsDataService = new SalesorderitemsDataService();
                            long agentid = salesorderitemsDataService.FindAgent(soitemids);
                            long addnchargeid = salesorderitemsDataService.FindAdditionalCharge(soitemids);
                            long salesmenid = salesorderitemsDataService.FindSalesmen(soitemids);

                            if (agentid > 0)
                            {
                                AgentDataService agentdatasource = new AgentDataService();
                                source.agent = agentdatasource.Search(new AgentCreteria()
                                {
                                    id = agentid
                                }).First();
                            }
                            if (salesmenid > 0)
                            {
                                EmployeeDataService agentdatasource = new EmployeeDataService();
                                source.salesmen = agentdatasource.Search(new EmployeeCreteria()
                                {
                                    id = salesmenid,
                                    hidesalaryinfo = true
                                }).First();
                            }
                            if (addnchargeid > 0)
                            {
                                source.sochargetype = referencelistDataService.Search(new ReferencelistCreteria()
                                {
                                    id = addnchargeid
                                }).First();
                            }
                        }
                        else if (source.customer.agentid > 0)
                        {
                            AgentDataService agentdatasource = new AgentDataService();
                            source.agent = agentdatasource.Search(new AgentCreteria()
                            {
                                id = source.customer.agentid
                            }).First();
                        }

                        if (source.billattributes.shiptoid > 0)
                        {
                            source.shipto = customerDataService.Search(new CustomerCreteria()
                            {
                                id = source.billattributes.shiptoid
                            }).First();
                            var addresslist = source.shipto.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            source.shipto.addresslines = new List<Customer.AddressLine>();
                            addresslist.ToList().ForEach(e =>
                            {
                                source.shipto.addresslines.Add(new Customer.AddressLine()
                                {
                                    value = e
                                });
                            });
                        }
                        else
                        {
                            source.shipto = source.customer;
                        }
                        if (string.IsNullOrEmpty(source.shipto.deliveryaddress))
                        {
                            var addresslist = source.shipto.deliveryaddress.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            source.shipto.deliveryaddresslines = new List<Customer.AddressLine>();
                            addresslist.ToList().ForEach(e =>
                            {
                                source.shipto.deliveryaddresslines.Add(new Customer.AddressLine()
                                {
                                    value = e
                                });
                            });
                        }
                    }


                    if (!string.IsNullOrEmpty(source.customer.address))
                    {
                        var addresslist = source.customer.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        source.customer.addresslines = new List<Customer.AddressLine>();
                        addresslist.ToList().ForEach(e =>
                        {
                            source.customer.addresslines.Add(new Customer.AddressLine()
                            {
                                value = e
                            });
                        });
                    }
                }
                if (source.areaid > 0)
                {
                    var area = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = source.areaid
                    }).First();
                    source.areaname = area.name;
                }
                if (source.billtype == Bill.BillTypes.Bulk && source.billattributes != null && source.billattributes.approvedby > 0)
                {
                    EmployeeDataService empdata = new EmployeeDataService();
                    var emplist = empdata.SearchHeader(new EmployeeCreteria() { id = source.billattributes.approvedby });
                    if (emplist != null && emplist.Count > 0)
                    {
                        source.approvedby = emplist[0].name;
                    }
                }
                if (source.isb2b)
                {
                    BillinboxDataService billinboxDataService = new BillinboxDataService();
                    var deliverydetails = billinboxDataService.GetBillDeliveryDetails(new BillinboxCreteria()
                    {
                        billid = source.id
                    });
                    if (deliverydetails.Count > 0)
                        source.billdeliverydetails = deliverydetails[0];
                }

                if (source.Billitems.Count > 0 && source.Billitems.Exists(e => e.schemeid > 0)
                    && source.Billitems.Any(e => string.IsNullOrEmpty(e.scheme)))
                {
                    PromotionsDataService promotiondata = new PromotionsDataService();
                    var promolist = promotiondata.Search(new PromotionsCreteria()).ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    foreach (var _billitem in source.Billitems)
                    {
                        if (_billitem.schemeid > 0 && promolist.Exists(e => e.id == _billitem.schemeid))
                            _billitem.scheme = promolist.Find(e => e.id == _billitem.schemeid).name;
                    }
                }

                noofcopy = reportformats[0].noofcopy;
                try
                {
                    var additional = new SortedDictionary<string, object>();
                    var imageData = new Common.ImageData();

                    if ((((InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_mergebillprint && isFirst)
                        || !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_mergebillprint)
                        && companyforlogo.Count == 1 && !string.IsNullOrEmpty(companyforlogo[0].logo)))
                    {
                        var file = imageData.GetData(companyforlogo[0].logo.LongFromBase62());
                        if (file != null)
                            additional.Add("LOGO", file.data);
                    }

                    source.Billitems = source.Billitems.OrderBy(e => e.orderid).ToList();
                    ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                    ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());


                    if (!(source.billtype == Bill.BillTypes.Estimate &&
                        Services.ConfigManager.Instance(organizationid).Config.system_supportestimate &&
                        Services.ConfigManager.Instance(organizationid).Config.system_supportestimatesticker &&
                         printConfig == "ESTIMATEBILL"))
                        labreport.format = ReportLibrary.DosFormatParser.ParseXML(imageData.Context.OrganizationID, reportformats[0].reportformat.format);
                    else
                        labreport.format = new ReportLibrary.DosFormat();

                    if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains(".servicecenter"))
                    {
                        ServicecentersDataService servicecentersDataService = new ServicecentersDataService();
                        servicecentersDataService.UpdateServiceCenter(source);
                    }

                    if (reportformats[0].reportformat.format.Contains("settlementgift") && source.billattributes.settlement != null)
                    {
                        DeliveryDataService deliveryDataService = new DeliveryDataService();
                        if (source.billattributes == null)
                            source.billattributes = new BillAttribute();
                        if (source.billattributes.settlement == null)
                            source.billattributes.settlement = new Billsettlementmaster();
                        if (source.billattributes.settlement.Billsettlement == null)
                        {
                            source.billattributes.settlement.Billsettlement = new List<Billsettlement>();
                            source.billattributes.settlement.Billsettlement.Add(new Billsettlement()
                            {
                                billid = source.id
                            });
                        }
                        source.billattributes.settlement.settlementgift = deliveryDataService.GetSettlementGiftList(source.billattributes.settlement);
                    }

                    if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("salesref"))
                    {
                        if (source.billattributes != null &&
                            source.billattributes.salesrefid > 0)
                        {
                            EmployeeDataService employeeDataService = new EmployeeDataService();
                            source.salesref = employeeDataService.SearchHeader(new EmployeeCreteria()
                            {
                                id = source.billattributes.salesrefid
                            }).First().name;
                        }
                    }



                    if (source.hascustomer && reportformats[0].reportformat.format != null &&
                       reportformats[0].reportformat.format.Contains("transport"))
                    {
                        source.transportcity = source.customer.cityname;
                        if (source.customer.preferredtransportid > 0)
                        {
                            TransportDataService transportDataService = new TransportDataService();
                            source.transportname = transportDataService.Search(new TransportCreteria()
                            {
                                id = source.customer.preferredtransportid
                            }).First().name;
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("fburl") ||
                            reportformats[0].reportformat.format.Contains("fburl")))
                    {
                        var BillDataService = new BillDataService();
                        source.fburl = BillDataService.GetFBURL(source.billno);
                    }


                    if (source.hassettlement &&
                        source.customerid > 0 &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryenable &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount > 0)
                    {
                        if (reportformats[0].reportformat.format != null &&
                                                reportformats[0].reportformat.format.Contains("Lottery_") &&
                                                source.billattributes.settlement.receivable >= InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount)
                        {
                            var noofcoupons = Math.Floor(source.billattributes.settlement.receivable /
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount);
                            if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax > 0 &&
                                InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax < noofcoupons)
                                noofcoupons = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax;
                            source.billattributes.settlement.Lottery_Avilable = true;
                            source.billattributes.settlement.Lottery_Count = Convert.ToInt32(noofcoupons);
                            source.billattributes.settlement.customer = source.customer;
                        }
                    }

                    if (source.hassettlement &&
                        source.customerid > 0 &&
                        reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("settlement.coupons") &&
                        source.billattributes.settlement.coupons != null)
                    {
                        source.billattributes.settlement.coupons.ForEach(x => x.customer = source.customer);
                    }


                    if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("HasFreeGift"))
                    {
                        source.FreeItems = new List<BillItemAttributes.FreeItems>();
                        source.Billitems.ForEach(x =>
                        {
                            if (x.attributes != null &&
                               x.attributes.freeitems != null &&
                               x.attributes.freeitems.Count > 0)
                                source.FreeItems.AddRange(x.attributes.freeitems);
                        });
                    }

                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("approvedby") ||
                            reportformats[0].reportformat.format.Contains("approvedby")))
                    {
                        if (source.billattributes != null &&
                            source.billattributes.approvedby > 0)
                        {
                            EmployeeDataService employeeDataService = new EmployeeDataService();
                            source.approvedby = employeeDataService.SearchHeader(new EmployeeCreteria()
                            {
                                id = source.billattributes.approvedby
                            }).First().name;
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("biller") ||
                            reportformats[0].reportformat.format.Contains("biller")))
                    {
                        if (source.billattributes != null && string.IsNullOrEmpty(source.billattributes.biller))
                        {
                            UsersDataService usersDataService = new UsersDataService();
                            var user = usersDataService.Search(new UsersCreteria()
                            {
                                id = source.createdby
                            });
                            if (user.Count > 0)
                                source.billattributes.biller = user.First().title;
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                    (reportformats[0].reportformat.format.Contains("billercode") ||
                    reportformats[0].reportformat.format.Contains("billercode")))
                    {
                        if (source.billattributes != null && string.IsNullOrEmpty(source.billattributes.billercode))
                        {
                            UsersDataService usersDataService = new UsersDataService();
                            var employeeid = usersDataService.Search(new UsersCreteria()
                            {
                                id = source.createdby
                            }).First().employeeid;
                            if (employeeid > 0)
                            {
                                EmployeeDataService employeeDataService = new EmployeeDataService();
                                source.billattributes.billercode = employeeDataService.SearchHeader(new EmployeeCreteria()
                                {
                                    id = employeeid
                                }).First().employeecode;
                            }
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                    (reportformats[0].reportformat.format.Contains("qtytometer") ||
                    reportformats[0].reportformat.format.Contains("qtytopiece")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateSellingMode(source.Billitems);
                    }
                    else if (reportformats[0].reportformat.format != null &&
                        // !reportformats[0].reportformat.format.Contains("orderid") &&
                        (reportformats[0].reportformat.format.Contains("qtyinmeter") ||
                        reportformats[0].reportformat.format.Contains("qtyinpiece")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateSellingMode(source.Billitems);
                        actualBillitems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Billitems>>(Newtonsoft.Json.JsonConvert.SerializeObject(source.Billitems));
                        var groupedItems = source.Billitems.GroupBy(e => new
                        {
                            e.printingname,
                            e.barcode,
                            e.qty,
                            e.salesmanid,
                            e.discount,
                            e.additionaldiscount,
                            e.sellingmode
                        });
                        List<Billitems> newitems = new List<Billitems>();
                        foreach (var gitem in groupedItems)
                        {
                            var newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem.First()));

                            if (gitem.Key.sellingmode == 3)
                            {
                                newbillitem.qtyinmeter = gitem.First().qty.ToString("N2");
                                newbillitem.qtyinpiece = gitem.Count().ToString("N0");
                            }
                            else
                            {
                                newbillitem.qtyinpiece = gitem.Sum(e => e.qty).ToString("N0");
                            }
                            newbillitem.qty = gitem.Sum(e => e.qty);
                            newbillitem.receivable = gitem.Sum(e => e.receivable);
                            newbillitem.discount = gitem.Sum(e => e.discount);
                            newbillitem.meteramount = gitem.Sum(e => e.amount);
                            newbillitem.schemediscount = gitem.Sum(e => e.schemediscount);
                            newbillitem.discountpercentage = gitem.Average(e => e.discountpercentage);
                            newbillitem.additionaldiscountpercentage = gitem.Average(e => e.additionaldiscountpercentage);
                            newbillitem.additionaldiscount = gitem.Average(e => e.additionaldiscount);
                            newbillitem.saleamount = gitem.Sum(e => e.rate * e.qty);
                            newbillitem.taxsplit = new TaxSplit();
                            newbillitem.taxsplit.cgsttaxamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.cgsttaxamount : 0));
                            newbillitem.taxsplit.sgsttaxamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.sgsttaxamount : 0));
                            newbillitem.taxsplit.totalamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.totalamount : 0));
                            newbillitem.taxsplit.name = gitem.First().taxsplit.name;
                            newbillitem.taxsplit.secondtax = gitem.First().taxsplit.secondtax;
                            newbillitem.taxsplit.cgsttaxpercentage = gitem.First().taxsplit.cgsttaxpercentage;
                            newbillitem.taxsplit.sgsttaxpercentage = gitem.First().taxsplit.sgsttaxpercentage;
                            newbillitem.orderid = newitems.Count + 1;
                            newitems.Add(newbillitem);
                        }
                        source.Billitems = newitems;
                    }
                    else if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("orderid") &&
                        (reportformats[0].reportformat.format.Contains("qtyinmeter") ||
                        reportformats[0].reportformat.format.Contains("qtyinpiece")))
                    {
                        foreach (var bitem in source.Billitems)
                        {
                            if (bitem.sellingmode == 3)
                            {
                                bitem.qtyinmeter = bitem.qty.ToString("N2");
                                bitem.qtyinpiece = "1";
                            }
                            else
                                bitem.qtyinpiece = bitem.qty.ToString("N0");
                        }
                    }
                    else if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_mergebillprint)
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateSellingMode(source.Billitems);
                    }


                    if (reportformats[0].reportformat.format.Contains("taxdetail.") ||
                       reportformats[0].reportformat.format.Contains("BillHSN"))
                    {
                        foreach (var item in source.Billitems)
                        {
                            if (source.isinterstatesale)
                            {
                                item.taxdetail = new Billitems.ItemTaxDetail()
                                {
                                    totaltax = item.tax,
                                    igst_percentage = item.taxpercentage,
                                    igst_value = item.tax
                                };
                            }
                            else
                            {
                                item.taxdetail = new Billitems.ItemTaxDetail()
                                {
                                    totaltax = item.tax,
                                    cgst_percentage = item.taxpercentage / 2,
                                    cgst_value = Math.Round(item.tax / 2, 4),
                                    sgst_percentage = item.taxpercentage / 2,
                                    sgst_value = Math.Round(item.tax / 2, 4)
                                };
                            }
                        }
                    }

                    if (reportformats[0].reportformat.format.Contains("BillHSN"))
                    {
                        source.BillHSN = new List<BillHSN>();
                        foreach (var grp in source.Billitems.GroupBy(e => new { e.hsncode, e.taxpercentage }))
                        {
                            source.BillHSN.Add(new BillHSN()
                            {
                                code = grp.Key.hsncode,
                                taxable = grp.Sum(e => e.gross),
                                tax = grp.Sum(e => e.tax),
                                total = grp.Sum(e => e.receivable),
                                qty = grp.Sum(e => e.qty),
                                peice = grp.Sum(e => e.piece),
                                cgst_percentage = grp.First().taxdetail.cgst_percentage,
                                igst_percentage = grp.First().taxdetail.igst_percentage,
                                sgst_percentage = grp.First().taxdetail.sgst_percentage,
                                sgst_value = grp.Sum(e => e.taxdetail.sgst_value),
                                cgst_value = grp.Sum(e => e.taxdetail.cgst_value),
                                igst_value = grp.Sum(e => e.taxdetail.igst_value)
                            });
                        }
                    }

                    if (reportformats[0].reportformat.format.Contains("BillHSN"))
                    {
                        if (source.additionalchargeslist != null &&
                            source.additionalchargeslist.Count > 0)
                        {
                            foreach (var grp in source.additionalchargeslist.GroupBy(x => new { x.hsncode, x.taxpercentage }))
                            {
                                source.BillHSN.Add(new BillHSN()
                                {
                                    code = grp.Key.hsncode,
                                    taxable = grp.Sum(e => e.Amount - e.Tax),
                                    tax = grp.Sum(e => e.Tax),
                                    total = grp.Sum(e => e.Amount),
                                    qty = grp.Count(),
                                    peice = grp.Count(),
                                    cgst_percentage = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Key.taxpercentage / 2,
                                    igst_percentage = !source.isinterstatesale ? 0 : grp.Key.taxpercentage,
                                    sgst_percentage = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Key.taxpercentage / 2,
                                    sgst_value = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Sum(e => e.Tax / 2),
                                    cgst_value = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Sum(e => e.Tax / 2),
                                    igst_value = !source.isinterstatesale ? 0 : grp.Sum(e => e.Tax)
                                });
                            }
                        }
                    }


                    if (reportformats[0].reportformat.format.Contains("FoodCounters"))
                    {
                        source.FoodCounters = new List<BillFoodCounter>();
                        foreach (var grp in source.Billitems.GroupBy(e => new { e.attributes.foodcounter }))
                        {
                            source.FoodCounters.Add(new BillFoodCounter()
                            {
                                counter = grp.Key.foodcounter,
                                bill = source,
                                qty = grp.Sum(e => e.qty),
                                peice = grp.Sum(e => e.piece),
                                items = grp.ToList()
                            });
                        }
                    }

                    foreach (var item in source.Billitems)
                    {
                        if (!item.haslastbit && (item.attributes != null && item.attributes.islastbit))
                            item.journalqty = (item.qty < 0 ? (item.qty * -1) + item.attributes.lastbitqty : item.qty + item.attributes.lastbitqty);
                    }


                    byte[] document = null;

                    List<string> imageids = labreport.format.GetImages();
                    labreport.Additional = new SortedDictionary<string, object>();
                    foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                    {
                        var file = imageData.GetData(imgid.LongFromBase62());
                        if (file != null && file.data != null)
                        {
                            additional.Add("IMG-" + imgid, file.data);
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                       reportformats[0].reportformat.format.Contains("salesmencodes"))
                    {
                        List<long> salesmenids = new List<long>();
                        foreach(var bi in source.Billitems)
                        {
                            if (bi.salesmanid > 0)
                                salesmenids.Add(bi.salesmanid);
                            if (bi.attributes != null &&
                            bi.attributes.smcodes != null &&
                            bi.attributes.smcodes.Count > 0)
                            {
                                salesmenids.AddRange(bi.attributes.smcodes);
                            }
                        }

                        salesmenids.RemoveAll(x => x == 0);
                        EmployeeDataService agentdatasource = new EmployeeDataService();
                        var salesmens = string.Join(",", agentdatasource.Search(new EmployeeCreteria()
                        {
                            ids = salesmenids.Distinct().ToList(),
                            hidesalaryinfo = true
                        }).ConvertAll(x => x.employeecode));
                        source.salesmencodes = salesmens;
                    }


                    if (source.billtype == Bill.BillTypes.Estimate &&
                        Services.ConfigManager.Instance(organizationid).Config.system_supportestimate &&
                        Services.ConfigManager.Instance(organizationid).Config.system_supportestimatesticker &&
                         printConfig == "ESTIMATEBILL")
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(source.company.name,
                        reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);
                        var billbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill2Print>(Newtonsoft.Json.JsonConvert.SerializeObject(source));
                        var barcodeformat = new List<Report.IBarcodeFormat>(new Report.IBarcodeFormat[] { billbarcode });

                        if (source.billattributes != null && source.billattributes.deliveryrackft)
                            barcodeformat.Add(barcodeformat[0]);

                        var response = barcodeprinter.Render(barcodeformat);
                        document = System.Text.ASCIIEncoding.ASCII.GetBytes(response[0]);
                    }
                    else if (source.billattributes != null &&
                                   source.billattributes.touchsale)
                    {
                        document = labreport.format.RenderBytes(new ImageData(), source, source.Billitems, additional, true);
                    }
                    else if (
                        (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany &&
                        !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle) &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle.IndexOf(',') > 0)
                        ||
                        labreport.format.PageSetting.fixedbilltypes == null || labreport.format.PageSetting.fixedbilltypes.Length == 0)
                    {
                        document = labreport.format.RenderBytes(new ImageData(), source, source.Billitems, additional,
                            isLast || (source.billtype == Bill.BillTypes.ReturnBill
                            && InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillprintformat &&
                            !ismultibill));
                    }
                    else
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                            foreach (var btype in labreport.format.PageSetting.fixedbilltypes)
                            {
                                source.showfixeddisplay = true;
                                source.fixedbilldisplay = btype;
                                var tdocument = labreport.format.RenderBytes(new ImageData(), source, source.Billitems, additional, true);
                                bw.Write(tdocument);
                            }
                            document = ms.ToArray();
                        }
                    }
                    //if (!ismultibill && noofcopy > 1 &&
                    //   (!InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_mergebillprint ||
                    //   (source.billtype == Bill.BillTypes.ReturnBill && 
                    //    InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.system_exchangebillprintformat)))

                    if ((!ismultibill && noofcopy > 1 &&
                        (!InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_mergebillprint ||
                        (source.billtype == Bill.BillTypes.ReturnBill &&
                         InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillprintformat)))
                        ||
                        ((!ismultibill && (source.billtype == Bill.BillTypes.ReturnBill
                        && InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillprintformat))
                        && noofcopy > 1)

                        )
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                            bw.Write(document);
                            int c = 1;
                            while (c < noofcopy)
                            {
                                bw.Write(document);
                                c++;
                            }
                            document = ms.ToArray();
                        }
                    }
                    else if (noofcopy > 1 && !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_multicompany)
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                            bw.Write(document);
                            int c = 1;
                            while (c < noofcopy)
                            {
                                bw.Write(document);
                                c++;
                            }
                            document = ms.ToArray();
                        }
                    }
                    else if (!InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillsummaryprintformat
                        && source.billtype == Bill.BillTypes.ReturnBill
                        && InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillprintformat
                        && noofcopy > 1)
                    {
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                            bw.Write(document);
                            int c = 1;
                            while (c < noofcopy)
                            {
                                bw.Write(document);
                                c++;
                            }
                            document = ms.ToArray();
                        }
                    }

                    if (noofcopy > 1 &&
                    reportformats[0].reportformat.format != null &&
                                                reportformats[0].reportformat.format.Contains("REMOVE DUPLICATE"))
                    {
                        var encoding = Encoding.GetEncoding("ISO-8859-1");
                        var sdocument = encoding.GetString(document);
                        if (sdocument.Contains("REMOVE DUPLICATE"))
                        {
                            document = encoding.GetBytes(RemoveDuplicateBlocks(sdocument));
                        }
                    }


                    if (actualBillitems != null)
                        source.Billitems = actualBillitems;
                    if (oldBillTax != null)
                        source.BillTax = oldBillTax;
                    return Convert.ToBase64String(document);
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, company.Context.UserID, source.locationid);
                    if (oldBillTax != null)
                        source.BillTax = oldBillTax;
                    throw exp;
                }
            }
            else
            {
                WebAPI.Common.Logger.Instance(organizationid).Log(new Exception(printConfig + " NOT CONFIGURED FOR THE SELECTED COMPANY - "
                    + (companyforlogo.Count > 0 ? companyforlogo[0].name : source.companyid.ToString())), WebAPI.Common.Logger.LogType.Error,
                    source, company.Context.UserID, source.locationid);
                if (oldBillTax != null)
                    source.BillTax = oldBillTax;
                return null;
            }

        }

        private string RemoveDuplicateBlocks(string input)
        {
            const string pattern =
                @"--REMOVE DUPLICATE BEGIN--([\s\S]*?)--REMOVE DUPLICATE END--";

            var matches = Regex.Matches(input, pattern, RegexOptions.Multiline);

            // If zero or one block  just remove markers
            if (matches.Count <= 1)
            {
                return Regex.Replace(
                    input,
                    @"--REMOVE DUPLICATE BEGIN--|--REMOVE DUPLICATE END--",
                    string.Empty
                );
            }

            // Get LAST block match
            var lastMatch = matches[matches.Count - 1];
            string lastContent = lastMatch.Groups[1].Value;

            // Build final string
            var result = new System.Text.StringBuilder(input.Length);

            int currentIndex = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                // Append text before this block
                result.Append(input, currentIndex, match.Index - currentIndex);

                if (i == matches.Count - 1)
                {
                    // LAST block keep content (without markers)
                    result.Append(lastContent);
                }
                // else earlier blocks are skipped completely

                currentIndex = match.Index + match.Length;
            }

            // Append remaining text after last block
            result.Append(input, currentIndex, input.Length - currentIndex);

            return result.ToString();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintBulkPendingPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintBulkPendingPDF(string organizationid, string id)
        {
            try
            {
                List<Bill> result = null;

                BulkBillDataService BulkBillData = new BulkBillDataService();
                BulkBillData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                result = BulkBillData.Search(new BillCreteria() { id = long.Parse(id), isbulkreturn = true });
                if (result != null && result.Count > 0)
                {
                    result[0].billtype = Bill.BillTypes.BulkSaleRemaining;
                    FileContentInfo byteArray = PrintBill(long.Parse(organizationid), result[0], false, false, "", 0);
                    return new FileActionResult(false, "bill.pdf", byteArray.Extension, byteArray.Content);
                }
                return null;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }


        int lastOrderID = 0;
        [HttpPost]
        [Route("MergePrintPDF")]
        public async Task<IHttpActionResult> MergePrintPDF(ActionRequest<BillCancel> request)
        {
            List<Stream> memoryStreams = new List<Stream>();
            BillDataService BillData = new BillDataService();
            var result = BillData.Search(new BillCreteria() { ids = request.Item.ids, isimport = true });
            PdfDocument outputPDFDocument = new PdfDocument();
            foreach (var bill in result)
            {
                var pdf = PrintBill(BillData.Context.OrganizationID, bill, false, false, "", lastOrderID);
                PdfDocument inputPDFDocument = PdfReader.Open(pdf.Content.ReadAsStreamAsync().Result, PdfDocumentOpenMode.Import);
                outputPDFDocument.Version = inputPDFDocument.Version;
                foreach (PdfPage page in inputPDFDocument.Pages)
                {
                    outputPDFDocument.AddPage(page);
                }
            }

            FileContentInfo pdfstream = null;
            using (System.IO.MemoryStream output = new System.IO.MemoryStream())
            {
                outputPDFDocument.Save(output, false);
                outputPDFDocument.Close();
                pdfstream = new FileContentInfo(new ByteArrayContent(output.ToArray()));
                pdfstream.FileName = "billsettlement.pdf";
                pdfstream.Extension = "application/pdf";
            }
            return new FileActionResult(false, "bill.pdf", pdfstream.Extension, pdfstream.Content);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}/{isbulk}/{custom=false}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid,
            string id, string isbulk,
            string custom)
        {
            try
            {
                if (!string.IsNullOrEmpty(custom) &&
                    custom.IndexOf(".") > 0)
                {
                    custom = custom.Split('.')[0];
                }
                List<Bill> result = null;
                if (isbulk == "true" || isbulk == "yes")
                {
                    BulkBillDataService BulkBillData = new BulkBillDataService();
                    BulkBillData.Context = new UserClaim()
                    {
                        OrganizationID = long.Parse(organizationid)
                    };
                    result = BulkBillData.Search(new BillCreteria() { id = long.Parse(id), enableprintdocument = true });

                }
                else
                {
                    BillDataService BillData = new BillDataService();
                    BillData.Context = new UserClaim()
                    {
                        OrganizationID = long.Parse(organizationid)
                    };
                    result = BillData.Search(new BillCreteria() { id = long.Parse(id), enableprintdocument = true });
                }

                //bool customformat = string.IsNullOrEmpty(custom) ? false : bool.Parse(custom);
                if (result != null && result.Count > 0)
                {
                    FileContentInfo byteArray = PrintBill(long.Parse(organizationid), result[0], false, false,
                        custom, 0);
                    return new FileActionResult(false, "bill.pdf", byteArray.Extension, byteArray.Content);
                }
                return null;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintDeliverySlip/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintDeliverySlip(string organizationid, string id)
        {
            try
            {
                BillDataService BillData = new BillDataService();
                BillData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                List<Bill> result = BillData.Search(new BillCreteria() { id = long.Parse(id) });

                if (result != null && result.Count > 0)
                {
                    FileContentInfo byteArray = PrintBill(long.Parse(organizationid), result[0], false, true);
                    return new FileActionResult(false, "bill.pdf", byteArray.Extension, byteArray.Content);
                }
                return null;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("SendCreditOTP")]
        public async Task<IHttpActionResult> HashOTP(ActionRequest<Bill> request)
        {
            string hashotp = await Task<string>.Run(() => SendCreditOTP(request.Item));
            return this.Ok(hashotp);
        }

        private string SendCreditOTP(Bill bill)
        {
            var passcode = Token.RandomString(6, "0123456789");
            BillDataService billDataService = new BillDataService();
            billDataService.SendCreditOTP(bill, passcode);
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(passcode);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
            return BitConverter.ToInt64(byteHash, 0).ToString();
        }

        [HttpGet]
        [Route("ConfirmCreditOTP/{otp}/{hash}")]
        public async Task<IHttpActionResult> ConfirmCreditOTP(string otp, string hash)
        {
            bool hashotp = await Task<bool>.Run(() => ConfirmHashOTP(otp, hash));
            return this.Ok(hashotp);
        }

        private bool ConfirmHashOTP(string otp, string hash)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(otp);
            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
            return BitConverter.ToInt64(byteHash, 0).ToString() == hash;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("Sales/{token}/{organizationid}/{locationid}/{fromdate}/{todate}")]
        public async Task<IHttpActionResult> Sales(string token, string organizationid, string locationid, string fromdate, string todate)
        {
            try
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(organizationid) || string.IsNullOrEmpty(locationid)
                    || string.IsNullOrEmpty(fromdate) || string.IsNullOrEmpty(todate))
                    throw new Exception("Invalid input.");
                if (!ValidateToken(token))
                    throw new Exception("Invalid user token");
                if (long.Parse(organizationid) == 0 || long.Parse(locationid) == 0)
                    throw new Exception("Invalid company/location");
                if (DateTime.Parse(fromdate) == DateTime.MinValue || DateTime.Parse(todate) == DateTime.MinValue)
                    throw new Exception("Invalid date");

                SaleRetailCreteria _reportcreteria = new SaleRetailCreteria();
                _reportcreteria.locationid = long.Parse(locationid);
                _reportcreteria.fromdate = DateTime.Parse(fromdate);
                _reportcreteria.todate = DateTime.Parse(todate);
                _reportcreteria.mode = "4";
                _reportcreteria.billtype = 99;

                if (_reportcreteria.todate.Subtract(_reportcreteria.fromdate).Days > 32)
                    throw new Exception("Input date range is more than a month");

                RetailReportDataService RetailReportData = new RetailReportDataService();
                List<RetailReport> result = await Task<List<RetailReport>>.Run(() => RetailReportData.SearchSales(_reportcreteria));
                List<SaleRecord> output = result.ConvertAll(e => new SaleRecord()
                {
                    location = e.location,
                    counter = e.counter,
                    billno = e.billno,
                    billdate = e.billdate,
                    saleqty = e.saleqty,
                    saletax = e.saletax,
                    discount = e.discount,
                    addlndiscount = e.addlndiscount,
                    amount = e.amount,
                    taxable = e.taxable,
                    rounding = e.rounding
                });

                return this.Ok(output);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
                return this.Ok(exp.Message);
            }
        }
        private bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            UsersDataService UsersData = new UsersDataService();
            List<Users> result = UsersData.Search(new UsersCreteria() { accesskey = token });
            if (result != null && result.Count > 0) return true;
            return false;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadExcel/{organizationid}/{id}/{isbulk}")]
        public async Task<IHttpActionResult> DownloadExcel(string organizationid, string id, string isbulk)
        {
            try
            {
                System.Data.DataTable result = null;
                if (isbulk == "true" || isbulk == "yes")
                {
                    BulkBillDataService BulkBillData = new BulkBillDataService();
                    BulkBillData.Context = new UserClaim()
                    {
                        OrganizationID = long.Parse(organizationid)
                    };
                    result = await Task<System.Data.DataTable>.Run(() => BulkBillData.ExportBillDetail(long.Parse(id)));
                }
                else
                {
                    BillDataService BillData = new BillDataService();
                    BillData.Context = new UserClaim()
                    {
                        OrganizationID = long.Parse(organizationid)
                    };
                    result = await Task<System.Data.DataTable>.Run(() => BillData.ExportBillDetail(long.Parse(id)));
                }
                using (System.IO.MemoryStream memoryStream = new MemoryStream())
                {
                    ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                    var sheet = xLWorkbook.AddWorksheet(result, "Quanto Bill");
                    xLWorkbook.SaveAs(memoryStream);
                    return new FileActionResult(true, "QuantoBill.xlsx", "application/vnd.ms-excel", new ByteArrayContent(memoryStream.ToArray()));
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintBill(long organizationid, long id)
        {
            BillDataService BillData = new BillDataService();
            BillData.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var result = BillData.Search(new BillCreteria() { id = id });
            return PrintBill(organizationid, result[0], false,
                false, "", 0);
        }
        public FileContentInfo PrintBill(long organizationid, Bill source, bool json)
        {
            return PrintBill(organizationid, source, json, false);
        }
        public FileContentInfo PrintBill(long organizationid, Bill source, bool json, bool isb2bdeliveryslip)
        {
            return PrintBill(organizationid, source, json, isb2bdeliveryslip, "", 0);
        }
        public FileContentInfo PrintBill(long organizationid, Bill source, bool json, bool isb2bdeliveryslip,
            string customformat, int orderid)
        {
            if (source.billtype == Bill.BillTypes.Cancel && !InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_ismfgapplication)
                throw new Exception("Cancelled bill can't print");
            else if (source.billdeliverydetails != null &&
                source.billdeliverydetails.billinbox != null)
            {
                source.billdeliverydetails.billinbox.hascancel = true;
            }

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_creditlimit &&
                source.isb2b && !source.iscreditapproved)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "bill.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

            bool isthermalprinter = false;
            string lineitemsource = "";
            if (source.billtype != Bill.BillTypes.ReturnBill &&
                (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sale_removediscountonprint ||
                InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sale_removereturnonprint))
            {
                bool modified = false;
                source = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill>(Newtonsoft.Json.JsonConvert.SerializeObject(source));
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sale_removereturnonprint &&
                    source.Billitems.Exists(e => e.qty < 0))
                {
                    var receivable = source.Billitems.Sum(e => e.qty < 0 ? e.receivable * -1 : 0);
                    var additional = source.Billitems.Sum(e => e.qty < 0 ? (e.additionaldiscount * e.qty * -1) : 0);
                    var schemediscount = source.Billitems.Sum(e => e.qty < 0 ? (e.schemediscount * -1) : 0);
                    var tax = source.Billitems.Sum(e => e.qty < 0 ? (e.tax * -1) : 0);
                    source.receivable -= receivable;
                    source.additionaldiscount -= additional;
                    source.schemediscount -= schemediscount;
                    source.tax -= tax;
                    modified = true;
                    source.Billitems.RemoveAll(e => e.qty < 0);
                }

                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.sale_removereturnonprint &&
                    source.Billitems.Exists(e => e.additionaldiscount > 0))
                {
                    modified = true;
                    foreach (var item in source.Billitems.Where(e => e.additionaldiscount > 0))
                    {
                        item.receivable += item.additionaldiscount;

                        item.tax = Math.Round(item.receivable / (100 + item.taxpercentage) * item.taxpercentage, 4);
                        item.itemtax = item.tax / item.qty;
                        item.rate = item.price - item.tax;
                        item.additionaldiscount = 0;
                        item.additionaldiscountpercentage = 0;
                    }
                    source.tax = source.Billitems.Sum(e => e.tax);
                    source.receivable = source.Billitems.Sum(e => e.receivable);
                    source.additionaldiscount = 0;
                }
                if (modified)
                    source.taxsplit = null;
            }
            List<Billitems> actualBillitems = null;

            InfyPOS.Data.CompanyDataService company = new CompanyDataService();
            var companyforlogo = company.Search(new CompanyCreteria
            {
                id = source.companyid
            });

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            List<Configuration> reportformats = new List<Configuration>();

            if (source.isb2b)
            {
                if (source.billtype == Bill.BillTypes.ReturnBill || (source.billtype == Bill.BillTypes.Credit &&
                    source.receivable < 0))
                {
                    source.Billitems.ForEach(e =>
                    {
                        e.qty = Math.Abs(e.qty);
                        e.receivable = Math.Abs(e.receivable);
                        e.tax = Math.Abs(e.tax);
                    });
                    source.receivable = Math.Abs(source.receivable);
                    source.tax = Math.Abs(source.tax);
                    //source.rounding = source.rounding * -1;
                    source.additionalcharges = Math.Abs(source.additionalcharges);
                    if (source.additionalchargeslist != null &&
                        source.additionalchargeslist.Count > 0)
                    {
                        source.additionalchargeslist.ForEach(x =>
                        {
                            x.Charge = Math.Abs(x.Charge);
                            x.Tax = Math.Abs(x.Tax);
                            x.Amount = Math.Abs(x.Amount);
                        });
                    }
                    //                    source.rounding = Math.Round(source.receivable, MidpointRounding.AwayFromZero) - source.receivable;
                    source.rounding = Math.Round(source.receivable + source.additionalcharges, MidpointRounding.AwayFromZero) - (source.receivable + source.additionalcharges);

                    source.BillTax.ForEach(e =>
                    {
                        e.amount = Math.Abs(e.amount);
                        e.tax = Math.Abs(e.tax);
                    });
                    if (source.billattributes != null && source.billattributes.showdiscount)
                    {
                        reportformats = config.Search(new ConfigurationCreteria
                        {
                            key = "SALERETURNINVOICEDISC",
                            locationid = source.locationid,
                            organizationid = source.companyid
                        });
                    }

                    if (reportformats == null || reportformats.Count == 0)
                    {
                        reportformats = config.Search(new ConfigurationCreteria
                        {
                            key = "SALERETURNINVOICE",
                            locationid = source.locationid,
                            organizationid = source.companyid
                        });
                    }
                }
                else
                {
                    source.rounding = Math.Round(source.receivable + source.additionalcharges, MidpointRounding.AwayFromZero) - (source.receivable + source.additionalcharges);

                    if (source.billattributes != null && source.billattributes.showdiscount)
                    {
                        reportformats = config.Search(new ConfigurationCreteria
                        {
                            key = "SALEINVOICEDISC",
                            locationid = source.locationid,
                            organizationid = source.companyid
                        });
                    }

                    if (reportformats == null || reportformats.Count == 0)
                    {
                        reportformats = config.Search(new ConfigurationCreteria
                        {
                            key = isb2bdeliveryslip ? "B2BDELIVERYCHALLAN" : "SALEINVOICE",
                            locationid = source.locationid,
                            organizationid = source.companyid
                        });
                    }
                }

                if (reportformats == null || reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = isb2bdeliveryslip ? "B2BDELIVERYCHALLAN" : "SALEINVOICE",
                        organizationid = source.companyid
                    });
                }

                if (reportformats == null || reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = isb2bdeliveryslip ? "B2BDELIVERYCHALLAN" : "SALEINVOICE"
                    });
                }
            }
            else
            {
                if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication
                    && source.billtype == Bill.BillTypes.Bulk &&
                        Services.ConfigManager.Instance(organizationid).Config.system_supportdc)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "DELIVERYCHALLAN",
                        locationid = source.locationid,
                        organizationid = source.companyid
                    });

                }
                else if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication
                  && source.billtype == Bill.BillTypes.Estimate &&
                      Services.ConfigManager.Instance(organizationid).Config.system_supportestimate)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "ESTIMATEBILL",
                        locationid = source.locationid,
                        organizationid = source.companyid
                    });
                }
                else if (!json)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SALERECEIPTPDF",
                        locationid = source.locationid,
                        organizationid = source.companyid
                    });
                }

                var ispdfprint = InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_saledirectpdf ||
                    (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].system_saledirectpdf);

                if (source.isb2b &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].system_b2bsaledirectpdf)
                    ispdfprint = true;


                if ((reportformats == null || reportformats.Count == 0) && (
                    !source.directdos && (ispdfprint)))
                {
                    //isthermalprinter = true;
                    var printConfig = "SALERECEIPT";
                    if (source.isb2b)
                    {
                        //isthermalprinter = false;
                        printConfig = "SALEINVOICE";
                    }


                    if (source.billtype == Bill.BillTypes.ReturnBill || (source.billtype == Bill.BillTypes.Credit &&
                    source.receivable < 0) &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_exchangebillprintformat)
                    {
                        printConfig = "SALERETURN";
                    }

                    if (source.billtype == Bill.BillTypes.Bulk &&
                        Services.ConfigManager.Instance(organizationid).Config.system_supportdc)
                    {
                        printConfig = "DELIVERYCHALLAN";
                    }
                    if (source.billtype == Bill.BillTypes.Estimate &&
                       Services.ConfigManager.Instance(organizationid).Config.system_supportestimate)
                    {
                        printConfig = "ESTIMATEBILL";
                    }
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = printConfig,
                        locationid = source.locationid,
                        organizationid = source.companyid
                    });
                }
                if (reportformats == null || reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SALERECEIPTPDF",
                        locationid = source.locationid,
                        organizationid = source.companyid
                    });
                }
                if (reportformats == null || reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SALERECEIPTPDF",
                        organizationid = source.companyid
                    });
                }
                if (reportformats == null || reportformats.Count == 0)
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SALERECEIPTPDF",
                        locationid = source.locationid,
                    });
                }
            }

            if (!string.IsNullOrEmpty(customformat))
            {
                if (customformat.ToLower() == "true")
                {
                    reportformats = config.Search(new ConfigurationCreteria
                    {
                        key = "SALERECEIPTCUSTOM",
                        locationid = source.locationid,
                        organizationid = source.companyid
                    });
                }
                else if (customformat.ToLower() != "true")
                {
                    long templateid = 0;
                    if (long.TryParse(customformat, out templateid))
                    {
                        reportformats = config.Search(new ConfigurationCreteria
                        {
                            id = templateid
                        });
                    }
                }
            }
            else if (source.printtemplateid > 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    id = source.printtemplateid
                });
            }
            else if (source.billattributes != null &&
                source.billattributes.billmode > 0 &&
                source.billattributes.billtemplateid > 0)
            {
                reportformats = config.Search(new ConfigurationCreteria
                {
                    id = source.billattributes.billtemplateid
                });
            }

            //if (source.printtemplateid > 0)
            //{
            //    var printtemplates = config.Search(new ConfigurationCreteria
            //    {
            //        id = source.printtemplateid
            //    });
            //    if (printtemplates.Count == 1)
            //    {
            //        if (reportformats.Count == 0)
            //        {
            //            reportformats.AddRange(printtemplates);
            //        }
            //        else if (printtemplates.First().key == reportformats.First().key)
            //        {
            //            reportformats.Clear();
            //            reportformats.AddRange(printtemplates);
            //        }
            //    }
            //}

            if (reportformats.Count > 1)
            {
                if (reportformats.Exists(e => e.nooflines == source.papersize))
                {
                    reportformats = reportformats.FindAll(e => e.nooflines == source.papersize);
                }
            }
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            Bill.BillitemSorting billitemSorting = Bill.BillitemSorting.None;

            if (reportformats.Count > 0)
            {
                //reportformats[0].reportformat.format = reportformats[0].reportformat.format.Replace("test='test'", "field='groupedbymfgdesign'");
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, reportformats[0].reportformat.format);
                billitemSorting = source.GetSorting(labreport.format.PageBody != null ? labreport.format.PageBody.Sort : "");

                if (reportformats[0].reportformat.format.ToLower().IndexOf("thermal") > 0)
                {
                    isthermalprinter = true;
                }

                if (reportformats[0].reportformat.format.Contains("groupedbillitems"))
                {
                    lineitemsource = "groupedbillitems";
                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("mrp")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateMRP(source.Billitems);
                    }
                    source.groupedbillitems = new List<Bill.BillGroupingItem>();
                    foreach (var gitem in source.Billitems.GroupBy(e => e.attributes.groupname))
                    {
                        orderid++;
                        source.groupedbillitems.Add(new Bill.BillGroupingItem()
                        {
                            printingname = gitem.First().printingname,
                            attributes = gitem.First().attributes,
                            design = gitem.First().attributes.designid,
                            gross = gitem.Sum(e => e.gross),
                            hsncode = gitem.First().hsncode,
                            orderid = orderid,
                            qty = gitem.Sum(e => e.qty),
                            setqty = gitem.Average(e => e.qty),
                            rate = gitem.First().rate,
                            size = gitem.First().attributes.size,
                            sizegroup = gitem.First().attributes.sizegroup,
                            colour = gitem.First().attributes.colourname,
                            source = gitem.First(),
                            mrp = gitem.First().mrp
                        });
                    }
                    if (billitemSorting != Bill.BillitemSorting.None)
                    {
                        int neworderid = 1;
                        if (billitemSorting == Bill.BillitemSorting.Name)
                            source.groupedbillitems = source.groupedbillitems.OrderBy(x => x.printingname).ToList();
                        if (billitemSorting == Bill.BillitemSorting.Price)
                            source.groupedbillitems = source.groupedbillitems.OrderBy(x => x.rate).ToList();
                        if (billitemSorting == Bill.BillitemSorting.Amount)
                            source.groupedbillitems = source.groupedbillitems.OrderBy(x => x.gross).ToList();
                        foreach (var item in source.groupedbillitems)
                            item.orderid = neworderid++;
                    }
                }
                else if (reportformats[0].reportformat.format.Contains("groupedbymeter"))
                {
                    lineitemsource = "groupedbymeter";
                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("mrp")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateMRP(source.Billitems);
                    }
                    source.groupedbymeter = new List<Bill.BillGroupingItem>();
                    foreach (var gitem in source.Billitems.GroupBy(e => new
                    {
                        e.printingname,
                        e.rate
                    }))
                    {
                        orderid++;
                        string designids = string.Join(",", gitem.ToList().FindAll(x => x.attributes != null && !string.IsNullOrEmpty(x.attributes.designid)).ConvertAll(x => x.attributes.designid).Distinct());
                        source.groupedbymeter.Add(new Bill.BillGroupingItem()
                        {
                            printingname = gitem.First().printingname,
                            design = designids,
                            gross = gitem.Sum(e => e.gross),
                            hsncode = gitem.First().hsncode,
                            orderid = orderid,
                            attributes = gitem.First().attributes,
                            qtyinpiece = gitem.Count(),
                            qtylist = string.Join(",", gitem.ToList().ConvertAll(x => " " + x.qty.ToString("N2"))).Trim(),
                            qty = gitem.Sum(e => e.qty),
                            rate = gitem.First().rate,
                            size = gitem.First().attributes.size,
                            sizegroup = gitem.First().attributes.sizegroup,
                            colour = gitem.First().attributes.colourname,
                            source = gitem.First(),
                            mrp = gitem.First().mrp
                        });
                    }
                    if (billitemSorting != Bill.BillitemSorting.None)
                    {
                        int neworderid = 1;
                        if (billitemSorting == Bill.BillitemSorting.Name)
                            source.groupedbillitems = source.groupedbillitems.OrderBy(x => x.printingname).ToList();
                        if (billitemSorting == Bill.BillitemSorting.Price)
                            source.groupedbillitems = source.groupedbillitems.OrderBy(x => x.rate).ToList();
                        if (billitemSorting == Bill.BillitemSorting.Amount)
                            source.groupedbillitems = source.groupedbillitems.OrderBy(x => x.gross).ToList();
                        foreach (var item in source.groupedbillitems)
                            item.orderid = neworderid++;
                    }
                }
                else if (reportformats[0].reportformat.format.Contains("groupedbybrand"))
                {
                    lineitemsource = "groupedbybrand";
                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("mrp")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateMRP(source.Billitems);
                    }
                    source.groupedbybrand = new List<Bill.BillGroupingItem>();
                    if (source.Billitems.Exists(e => e.attributes == null))
                    {
                        source.Billitems.ForEach(e => e.attributes = new BillItemAttributes());
                    }
                    foreach (var gitem in source.Billitems.GroupBy(e => new
                    {
                        e.printingname,
                        e.attributes.brand,
                        e.attributes.designid,
                        e.hsncode,
                        e.attributes.size,
                        e.salerate,
                        e.totaldiscountpercentage,
                        e.mrp,
                        e.etdiscount,
                        e.etprice
                    }))
                    {
                        orderid++;
                        source.groupedbybrand.Add(new Bill.BillGroupingItem()
                        {
                            printingname = gitem.First().printingname,
                            brandname = gitem.Key.brand,
                            attributes = gitem.First().attributes,
                            design = gitem.Key.designid,
                            gross = gitem.Sum(e => e.gross),
                            hsncode = gitem.First().hsncode,
                            totaldiscountpercentage = gitem.Key.totaldiscountpercentage,
                            b2bgross = gitem.Sum(e => e.b2bgross),
                            orderid = orderid,
                            qty = gitem.Sum(e => e.qty),
                            setqty = gitem.Average(e => e.qty),
                            rate = gitem.First().rate,
                            ratewithdiscount = gitem.First().ratewithdiscount,
                            salerate = gitem.Key.salerate,
                            size = gitem.Key.size,
                            mrp = gitem.Key.mrp,
                            source = gitem.First(),
                            etdiscount = gitem.Key.etdiscount,
                            etprice = gitem.Key.etprice,
                            etgross = gitem.Sum(e => e.etprice * e.qty),
                        });
                        if (billitemSorting != Bill.BillitemSorting.None)
                        {
                            int neworderid = 1;
                            if (billitemSorting == Bill.BillitemSorting.Name)
                                source.groupedbybrand = source.groupedbybrand.OrderBy(x => x.printingname).ToList();
                            if (billitemSorting == Bill.BillitemSorting.Price)
                                source.groupedbybrand = source.groupedbybrand.OrderBy(x => x.etprice).ToList();
                            if (billitemSorting == Bill.BillitemSorting.Amount)
                                source.groupedbybrand = source.groupedbybrand.OrderBy(x => x.etgross).ToList();
                            foreach (var item in source.groupedbybrand)
                                item.orderid = neworderid++;
                        }
                    }
                }
                else if (reportformats[0].reportformat.format.Contains("groupedbysize"))
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var allsize = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
                    lineitemsource = "groupedbysize";
                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("mrp")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateMRP(source.Billitems);
                    }
                    source.groupedbysize = new List<Bill.BillGroupingItem>();
                    if (source.Billitems.Exists(e => e.attributes == null))
                    {
                        source.Billitems.ForEach(e => e.attributes = new BillItemAttributes());
                    }
                    foreach (var gitem in source.Billitems.GroupBy(e => new
                    {
                        e.printingname,
                        e.attributes.designid,
                        e.hsncode,
                        e.attributes.size,
                        e.mrp,
                        e.etdiscount,
                        e.etprice,
                        e.computed_etprice,
                        e.totaldiscountpercentage
                    }))
                    {
                        orderid++;
                        source.groupedbysize.Add(new Bill.BillGroupingItem()
                        {
                            printingname = gitem.First().printingname,
                            attributes = gitem.First().attributes,
                            design = gitem.Key.designid,
                            gross = gitem.Sum(e => e.gross),
                            hsncode = gitem.Key.hsncode,
                            totaldiscountpercentage = gitem.Key.totaldiscountpercentage,
                            b2bgross = gitem.Sum(e => e.b2bgross),
                            orderid = orderid,
                            qty = gitem.Sum(e => e.qty),
                            setqty = gitem.Average(e => e.qty),
                            rate = gitem.First().rate,
                            ratewithdiscount = gitem.First().ratewithdiscount,
                            size = gitem.Key.size,
                            mrp = gitem.Key.mrp,
                            source = gitem.First(),
                            etdiscount = gitem.Key.etdiscount,
                            etprice = gitem.Key.etprice,
                            etgross = gitem.Sum(e => e.etgross),
                            computed_etprice = gitem.Key.computed_etprice,
                            computed_etgross = gitem.Sum(e => e.computed_etgross),
                            sortingindex = allsize.FindIndex(x => x.name == gitem.Key.size)
                        });
                        if (billitemSorting != Bill.BillitemSorting.None)
                        {
                            if (billitemSorting == Bill.BillitemSorting.Name)
                                source.groupedbysize = source.groupedbysize.OrderBy(x => x.printingname).ToList();
                            if (billitemSorting == Bill.BillitemSorting.Price)
                                source.groupedbysize = source.groupedbysize.OrderBy(x => x.etprice).ToList();
                            if (billitemSorting == Bill.BillitemSorting.Amount)
                                source.groupedbysize = source.groupedbysize.OrderBy(x => x.etgross).ToList();
                        }
                    }

                    source.groupedbysize = source.groupedbysize.OrderBy(x => x.printingname).ThenBy(y => y.sortingindex).ToList();
                    int neworderid = 1;
                    foreach (var item in source.groupedbysize)
                        item.orderid = neworderid++;
                }
                else if (reportformats[0].reportformat.format.Contains("groupedbydynamic"))
                {
                    lineitemsource = "groupedbydynamic";
                    BillitemsDataService billitemsDataService = new BillitemsDataService();
                    billitemsDataService.UpdateMRP(source.Billitems);

                    var xmlformat = ReportLibrary.DosFormatParser.ParseXML(0, reportformats[0].reportformat.format);
                    var fields = xmlformat.PageBody.GetFields().ConvertAll(e => e.StartsWith("source.") ? e.Substring(7) : e);
                    source.groupedbydynamic = new List<Bill.BillGroupingItem>();

                    source.Billitems.ForEach(e =>
                    {
                        if (e.attributes == null) e.attributes = new BillItemAttributes();
                        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                        foreach (var fld in fields)
                        {
                            var value = ReportLibrary.DataSourceHelper.Eval(e, fld);
                            if (value == null)
                                stringBuilder.Append("null");
                            else
                                stringBuilder.Append(value);
                            stringBuilder.Append(",");
                        }
                        e.attributes.groupkey = stringBuilder.ToString();
                    });

                    //e.mrp,
                    foreach (var gitem in source.Billitems.GroupBy(e => new
                    {
                        e.attributes.groupkey,
                        e.salerate,
                        e.totaldiscountpercentage,
                        e.etdiscount,
                        e.etprice,
                        e.computed_etprice
                    }))
                    {
                        // mrp = gitem.Key.mrp,
                        orderid++;
                        source.groupedbydynamic.Add(new Bill.BillGroupingItem()
                        {
                            printingname = gitem.First().printingname,
                            attributes = gitem.First().attributes,
                            gross = gitem.Sum(e => e.gross),
                            hsncode = gitem.First().hsncode,
                            totaldiscountpercentage = gitem.Key.totaldiscountpercentage,
                            b2bgross = gitem.Sum(e => e.b2bgross),
                            orderid = orderid,
                            qty = gitem.Sum(e => e.qty),
                            setqty = gitem.Average(e => e.qty),
                            rate = gitem.First().rate,
                            ratewithdiscount = gitem.First().ratewithdiscount,
                            salerate = gitem.Key.salerate,
                            source = gitem.First(),
                            etdiscount = gitem.Key.etdiscount,
                            etprice = gitem.Key.etprice,
                            etgross = gitem.Sum(e => e.etprice * e.qty),
                            computed_etprice = gitem.Key.computed_etprice,
                            computed_etgross = gitem.Sum(e => e.computed_etprice * e.qty),
                        });
                        if (billitemSorting != Bill.BillitemSorting.None)
                        {
                            int neworderid = 1;
                            if (billitemSorting == Bill.BillitemSorting.Name)
                                source.groupedbydynamic = source.groupedbydynamic.OrderBy(x => x.printingname).ToList();
                            if (billitemSorting == Bill.BillitemSorting.Price)
                                source.groupedbydynamic = source.groupedbydynamic.OrderBy(x => x.etprice).ToList();
                            if (billitemSorting == Bill.BillitemSorting.Amount)
                                source.groupedbydynamic = source.groupedbydynamic.OrderBy(x => x.etgross).ToList();
                            foreach (var item in source.groupedbydynamic)
                                item.orderid = neworderid++;
                        }
                    }
                }
                else if (reportformats[0].reportformat.format.Contains("mrp"))
                {
                    if (Services.ConfigManager.Instance(organizationid).Config.system_ismfgapplication)
                    {

                        var Listofserials = source.Billitems.ConvertAll(e => e.attributes == null ? e.barcode : (string.IsNullOrEmpty(e.attributes.serialno) ? e.barcode : e.attributes.serialno));
                        Listofserials.RemoveAll(e => e == "");
                        if (Listofserials.Count > 0)
                        {
                            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                            var designlist = mfgdesignDataService.SearchDesignList(Listofserials);
                            foreach (var item in source.Billitems)
                            {
                                if (item.attributes != null && !string.IsNullOrEmpty(item.attributes.serialno))
                                {
                                    if (designlist.Exists(e => item.attributes != null && e.uid == item.attributes.serialno))
                                    {
                                        item.mfgdesign = designlist.Find(e => e.uid == item.attributes.serialno);
                                        if (!string.IsNullOrEmpty(item.mfgdesign.hsn))
                                            item.hsncode = item.mfgdesign.hsn;

                                        if (!string.IsNullOrEmpty(item.mfgdesign.size) && item.attributes != null)
                                            item.attributes.size = item.mfgdesign.size;

                                        item.mrp = item.mfgdesign.mrp;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 1)
                                            item.mrp = item.mfgdesign.wsrate;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 2)
                                            item.mrp = item.mfgdesign.dealerprice;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 3)
                                            item.mrp = item.mfgdesign.salerate;
                                    }
                                }
                                else
                                {
                                    if (designlist.Exists(e => item.attributes != null && e.uid == item.barcode))
                                    {
                                        item.mfgdesign = designlist.Find(e => e.uid == item.barcode);
                                        if (!string.IsNullOrEmpty(item.mfgdesign.hsn))
                                            item.hsncode = item.mfgdesign.hsn;
                                        if (!string.IsNullOrEmpty(item.mfgdesign.size) && item.attributes != null)
                                            item.attributes.size = item.mfgdesign.size;

                                        item.mrp = item.mfgdesign.mrp;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 1)
                                            item.mrp = item.mfgdesign.wsrate;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 2)
                                            item.mrp = item.mfgdesign.dealerprice;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 3)
                                            item.mrp = item.mfgdesign.salerate;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateMRP(source.Billitems);
                    }
                }

            }

            bool hasexchangebillitem = false;
            if (source.Billitems.Exists(e => e.isexchange))
            {
                var exchangeitems = source.Billitems.FindAll(e => e.isexchange);
                source.Billitems.RemoveAll(e => e.isexchange);
                hasexchangebillitem = true;
                source.receivable -= exchangeitems.Sum(e => e.receivable);
                source.tax -= exchangeitems.Sum(e => e.tax);
                source.discount -= exchangeitems.Sum(e => e.discount);
                source.schemediscount -= exchangeitems.Sum(e => e.schemediscount);
            }

            if (reportformats.Count > 0)
            {
                if (hasexchangebillitem || source.taxsplit == null)
                {
                    source.taxsplit = new TaxSplit();
                    source.taxsplit.totalamount = source.Billitems.Sum(e => e.rate * e.qty);
                    if (source.isinterstatesale)
                    {
                        source.taxsplit.cgsttaxamount = 0;
                        source.taxsplit.sgsttaxamount = source.tax;
                        source.taxsplit.secondtax = "IGST";
                        source.Billitems.ForEach(e =>
                        {
                            e.taxsplit = new TaxSplit()
                            {
                                cgsttaxamount = 0,
                                cgsttaxpercentage = 0,
                                secondtax = "IGST",
                                sgsttaxamount = e.tax,
                                sgsttaxpercentage = e.taxpercentage
                            };
                        });
                    }
                    else
                    {
                        source.taxsplit.cgsttaxamount = Math.Round(source.tax / 2, 5);
                        source.taxsplit.sgsttaxamount = Math.Round(source.tax / 2, 5);
                        source.taxsplit.secondtax = "SGST";
                        source.Billitems.ForEach(e =>
                        {
                            e.taxsplit = new TaxSplit()
                            {
                                cgsttaxamount = Math.Round(e.tax / 2, 4),
                                cgsttaxpercentage = e.taxpercentage / 2,
                                secondtax = "SGST",
                                sgsttaxamount = Math.Round(e.tax / 2, 4),
                                sgsttaxpercentage = e.taxpercentage / 2
                            };
                        });
                    }
                    source.BillTax.ForEach(e =>
                    {
                        e.secondtax = source.taxsplit.secondtax;
                    });
                }

                List<BillTax> oldBillTax = null;
                if (hasexchangebillitem || source.BillTax.GroupBy(e => e.taxpercentage).Count() < source.BillTax.Count)
                {
                    oldBillTax = source.BillTax;
                    var billtax = new List<BillTax>();
                    foreach (var tax in source.BillTax.GroupBy(e => e.taxpercentage))
                    {
                        billtax.Add(new BillTax()
                        {
                            amount = tax.Sum(e => e.amount),
                            tax = tax.Sum(e => e.tax),
                            taxpercentage = tax.Key,
                            secondtax = tax.First().secondtax,
                            taxname = tax.First().taxname
                        });
                    }
                    source.BillTax = billtax;
                }

                CompanyDataService companyDataService = new CompanyDataService();
                var companylist = companyDataService.Search(new CompanyCreteria()
                {
                    id = source.companyid
                });
                if (companylist.Count == 0)
                {
                    companylist = companyDataService.Search(new CompanyCreteria()
                    {
                    });
                    if (companylist.Count == 1)
                    {
                        source.company = companylist[0];
                        source.company.parseaddress();
                    }
                    else
                    {
                        source.company = new Company();
                        source.company.name = "Name missing in bill";
                        source.company.address = "";
                        source.company.parseaddress();
                    }
                }
                else
                {
                    source.company = companylist[0];
                    source.company.parseaddress();
                }
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = source.locationid
                }).First();

                source.location = new CompanyLocation(location);
                source.location.name = location.name;
                source.location.phonenumber = location.additionalinfo == null ? "" : location.additionalinfo.formula;
                source.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                source.location.pincode = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.pincode;
                source.location.addresslines = new List<CompanyLocation.Address>();
                if (!string.IsNullOrEmpty(source.location.address))
                {
                    var addresslist = source.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        source.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }
                if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                    && location.additionalinfo.locationconfig.cityid > 0)
                {
                    var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = location.additionalinfo.locationconfig.cityid
                    });
                    if (cityinfo.Count > 0)
                        source.location.cityname = cityinfo[0].name;
                }
                if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                    && location.additionalinfo.locationconfig.stateid > 0)
                {
                    var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = location.additionalinfo.locationconfig.stateid
                    });
                    if (stateinfo.Count > 0)
                        source.location.statename = stateinfo[0].name;
                }
                if (source.company.stateid > 0)
                {
                    source.company.state = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = source.company.stateid
                    }).First();
                }
                if (source.areaid > 0)
                {
                    var area = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = source.areaid
                    }).First();
                    source.areaname = area.name;
                }
                if (source.isb2b)
                {
                    BillinboxDataService billinboxDataService = new BillinboxDataService();
                    var deliveryList = billinboxDataService.GetBillDeliveryDetails(new BillinboxCreteria()
                    {
                        billid = source.id
                    });
                    if (deliveryList.Count > 0)
                        source.billdeliverydetails = deliveryList[0];
                }

                if (source.billattributes != null && source.billattributes.settlement != null)
                {
                    if (source.billattributes.settlement.paymentinfo != null && source.billattributes.settlement.paymentinfo.card_bankid > 0)
                    {
                        var bank = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = source.billattributes.settlement.paymentinfo.card_bankid
                        }).First();
                        source.billattributes.settlement.paymentinfo.card_bankname = bank.name;
                    }
                    if (source.billattributes.settlement.paymentinfo != null && source.billattributes.settlement.paymentinfo.upiproviderid > 0)
                    {
                        var upi = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = source.billattributes.settlement.paymentinfo.upiproviderid
                        }).First();
                        source.billattributes.settlement.paymentinfo.upiprovidername = upi.name;
                    }
                }

                if (source.Billitems.Count > 0 && source.Billitems.Exists(e => e.schemeid > 0)
                    && source.Billitems.Any(e => string.IsNullOrEmpty(e.scheme)))
                {
                    PromotionsDataService promotiondata = new PromotionsDataService();
                    var promolist = promotiondata.Search(new PromotionsCreteria()).ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    foreach (var _billitem in source.Billitems)
                    {
                        if (_billitem.schemeid > 0 && promolist.Exists(e => e.id == _billitem.schemeid))
                            _billitem.scheme = promolist.Find(e => e.id == _billitem.schemeid).name;
                    }
                }


                try
                {
                    if (source.customerid > 0)
                    {
                        CustomerDataService customerDataService = new CustomerDataService();
                        source.customer = customerDataService.Search(new CustomerCreteria()
                        {
                            id = source.customerid
                        }).First();

                        if (source.billdeliverydetails == null &&
                          !string.IsNullOrEmpty(source.customer.gstno))
                        {
                            BillinboxDataService billinboxDataService = new BillinboxDataService();
                            var deliveryList = billinboxDataService.GetBillDeliveryDetails(new BillinboxCreteria()
                            {
                                billid = source.id
                            });
                            if (deliveryList.Count > 0)
                                source.billdeliverydetails = deliveryList[0];
                        }

                        if (source.customer.stateid > 0)
                        {
                            source.customer.state = referencelistDataService.Search(new ReferencelistCreteria()
                            {
                                id = source.customer.stateid
                            }).First();
                        }

                        if (source.Billitems.Exists(x => x.attributes != null &&
                            x.attributes.so != null && x.attributes.so.Count > 0))
                        {
                            List<long> soitemids = new List<long>();
                            source.Billitems.ForEach(x =>
                            {
                                foreach (var soitem in x.attributes.so)
                                    soitemids.Add(soitem.id);
                            });
                            SalesorderitemsDataService salesorderitemsDataService = new SalesorderitemsDataService();
                            long agentid = salesorderitemsDataService.FindAgent(soitemids);
                            long addnchargeid = salesorderitemsDataService.FindAdditionalCharge(soitemids);
                            long salesmenid = salesorderitemsDataService.FindSalesmen(soitemids);
                            source.billattributes.orderpoid = salesorderitemsDataService.FindSOBookNo(soitemids);
                            if (agentid > 0)
                            {
                                AgentDataService agentdatasource = new AgentDataService();
                                source.agent = agentdatasource.Search(new AgentCreteria()
                                {
                                    id = agentid
                                }).First();
                            }
                            if (salesmenid > 0)
                            {
                                EmployeeDataService agentdatasource = new EmployeeDataService();
                                source.salesmen = agentdatasource.Search(new EmployeeCreteria()
                                {
                                    id = salesmenid,
                                    hidesalaryinfo = true
                                }).First();
                            }
                            if (addnchargeid > 0)
                            {
                                source.sochargetype = referencelistDataService.Search(new ReferencelistCreteria()
                                {
                                    id = addnchargeid
                                }).First();
                            }
                        }
                        else if (source.customer.agentid > 0)
                        {
                            AgentDataService agentdatasource = new AgentDataService();
                            source.agent = agentdatasource.Search(new AgentCreteria()
                            {
                                id = source.customer.agentid
                            }).First();
                        }


                        if (source.billattributes.shiptoid > 0)
                        {
                            source.shipto = customerDataService.Search(new CustomerCreteria()
                            {
                                id = source.billattributes.shiptoid
                            }).First();
                            var addresslist = source.shipto.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            source.shipto.addresslines = new List<Customer.AddressLine>();
                            addresslist.ToList().ForEach(e =>
                            {
                                source.shipto.addresslines.Add(new Customer.AddressLine()
                                {
                                    value = e
                                });
                            });
                        }
                        else
                        {
                            source.shipto = source.customer;
                        }
                    }

                    source.Billitems = source.Billitems.OrderBy(e => e.orderid).ToList();

                    if (reportformats[0].reportformat.format.Contains("groupedbymfgdesign"))
                    {
                        lineitemsource = "groupedbymfgdesign";
                        source.groupedbymfgdesign = new List<Billitems>();
                        foreach (var group in source.Billitems.GroupBy(e => new
                        {
                            e.productname,
                            e.attributes.designid,
                            e.hsncode,
                            e.taxpercentage,
                            e.etprice,
                            e.etdiscount,
                            e.attributes.serialno,
                            e.attributes.colourname,
                            e.attributes.size
                        }))
                        {
                            orderid++;
                            var item = group.First();
                            item.orderid = orderid;
                            source.groupedbymfgdesign.Add(item);
                            item.qty = group.Sum(x => x.qty);
                            item.orderid = source.Billitems.Count + 1;
                            item.receivable = group.Sum(x => x.receivable);
                            item.tax = group.Sum(x => x.tax);
                            item.salerate = item.qty > 0 ? ((item.receivable - item.tax) / item.qty) : 0;
                        }
                        source.groupedbymfgdesign = source.groupedbymfgdesign.OrderBy(x => x.printingname).ThenBy(y => y.orderid).ToList();
                        int neworderid = 1;
                        foreach (var item in source.groupedbymfgdesign)
                            item.orderid = neworderid++;
                    }

                    if (reportformats[0].reportformat.format.Contains("groupedbydesign"))
                    {
                        var allsize = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
                        lineitemsource = "groupedbydesign";
                        source.groupedbydesign = new List<Billitems>();
                        foreach (var group in source.Billitems.GroupBy(e => new
                        {
                            e.productname,
                            e.attributes.designid,
                            e.hsncode,
                            e.etprice,
                            e.etdiscount,
                            e.attributes.colourcode
                        }))
                        {
                            orderid++;
                            var item = group.First();
                            item.orderid = orderid;
                            source.groupedbydesign.Add(item);
                            item.qty = group.Sum(x => x.qty);
                            item.orderid = source.Billitems.Count + 1;
                            item.receivable = group.Sum(x => x.receivable);
                            item.tax = group.Sum(x => x.tax);
                            item.salerate = item.qty > 0 ? ((item.receivable - item.tax) / item.qty) : 0;
                        }
                        source.groupedbydesign = source.groupedbydesign.OrderBy(x => x.printingname).ThenBy(y => y.orderid).ToList();
                        int neworderid = 1;
                        foreach (var item in source.groupedbydesign)
                            item.orderid = neworderid++;
                    }


                    if (labreport.format.PageBody != null &&
                        labreport.format.PageBody.Items[0].IsTable &&
                    labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
                    {
                        source.Billitems.FindAll(x => x.attributes.size == null).ForEach(x => x.attributes.size = "U");
                        var sizeList = source.Billitems.GroupBy(e => new { e.attributes.size }).ToList();
                        var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => e.Key.size.ToUpper()));
                        if (Services.ConfigManager.Instance(organizationid).Config.po_autocolumn)
                        {
                            var sizelist = WebAPI.Data.Utility.GetSizes();
                            sizeExistOrder = source.Billitems.GroupBy(e => e.attributes.size.ToUpper()).ToList().ConvertAll(e => e.Key);
                            sizeExistOrder.Sort(delegate (string a, string b)
                            {
                                var aindex = sizelist.IndexOf(a);
                                var bindex = sizelist.IndexOf(b);
                                return aindex.CompareTo(bindex);
                            });
                        }

                        int maxcolumn = Services.ConfigManager.Instance(organizationid).Config.po_maxcolumn > 0 ? Services.ConfigManager.Instance(organizationid).Config.po_maxcolumn : 16;
                        if (sizeExistOrder != null && maxcolumn > sizeExistOrder.Count)
                        {
                            if (sizeExistOrder != null && sizeExistOrder.Count != 0)
                            {
                                var groupedList = source.Billitems.GroupBy(e => new
                                {
                                    e.productname,
                                    e.attributes.designid,
                                    e.hsncode,
                                    e.etprice,
                                    e.etdiscount,
                                    e.attributes.colourcode
                                }).ToList();
                                source.Billitems.Clear();
                                List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                                int index = 0;
                                var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);

                                if (rootcolumn.Text.IndexOf(',') > 0)
                                {
                                    sizeExistOrder = new List<string>(rootcolumn.Text.Split(','));
                                    if (source.billattributes.sizegroupid > 0)
                                    {
                                        var sizegroup = referencelistDataService.Search(new ReferencelistCreteria()
                                        {
                                            id = source.billattributes.sizegroupid
                                        });
                                        if (sizegroup != null && sizegroup.Count > 0 &&
                                            sizegroup[0].additionalinfo != null &&
                                            sizegroup[0].additionalinfo.sizegroups != null)
                                        {
                                            var sizelist = referencelistDataService.Search(new ReferencelistCreteria()
                                            {
                                                ids = sizegroup[0].additionalinfo.sizegroups
                                            });
                                            sizeExistOrder = new List<string>();
                                            sizegroup[0].additionalinfo.sizegroups.ForEach(x =>
                                            {
                                                if (sizelist.Exists(y => y.id == x))
                                                    sizeExistOrder.Add(sizelist.Find(y => y.id == x).name);
                                            });
                                        }
                                    }
                                }

                                sizeExistOrder.Add("Total");



                                var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;
                                int diff = 0;
                                int colwidth = 0;
                                bool rootwidth = false;
                                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                                if (rootcolumn.Length > totalwidth)
                                {
                                    rootwidth = true;
                                    colwidth = (rootcolumn.Length * 2 - (labreport.format.PageBody.Items[0].Items.Sum(e => e.Length))) / sizeExistOrder.Count;
                                }
                                else
                                {
                                    colwidth = ((labreport.DefaultPageSettings.MarginBound.Width - totalwidth) / sizeExistOrder.Count);
                                    diff = (labreport.DefaultPageSettings.MarginBound.Width - totalwidth) - (colwidth * sizeExistOrder.Count);
                                }
                                foreach (var sitem in sizeExistOrder)
                                {
                                    ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                                    content.Type = ReportLibrary.DosFormat.CotnentType.Field;
                                    content.Text = sitem;
                                    content.Length = colwidth;
                                    if (sitem == "Total" && !rootwidth)
                                        content.Length = (int)(rootcolumn.Length);
                                    content.Align = rootcolumn.Align;
                                    content.Format = rootcolumn.Format;
                                    content.Total = rootcolumn.Total;
                                    content.Name = string.Format(rootcolumn.Name, index);
                                    index++;
                                    dynamiccolumns.Add(content);
                                }
                                //labreport.format.PageBody.WrapRow = true;
                                labreport.format.PageBody.Items[0].Items.InsertRange(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), dynamiccolumns);
                                labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);
                                labreport.format.PageBody.Items[0].Items.Last().Length += diff;
                                foreach (var group in groupedList)
                                {
                                    var item = group.First();
                                    item.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                                    foreach (var sitem in sizeExistOrder)
                                    {
                                        decimal qty = 0;
                                        if (sitem == "Total")
                                        {
                                            qty = group.Sum(e => e.qty);
                                        }
                                        else if (group.Any(e => e.attributes.size == sitem))
                                        {
                                            qty = group.Sum(e => e.attributes.size == sitem ? e.qty : 0);
                                        }
                                        item.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                                        {
                                            name = sitem,
                                            oqty = qty
                                        });
                                    }

                                    item.qty = group.Sum(x => x.qty);
                                    item.orderid = source.Billitems.Count + 1;
                                    item.receivable = group.Sum(x => x.receivable);
                                    item.tax = group.Sum(x => x.tax);
                                    item.salerate = item.qty > 0 ? ((item.receivable - item.tax) / item.qty) : 0;
                                    source.Billitems.Add(item);
                                }


                                if (source.Billitems.Exists(x => x.attributes != null && x.attributes.colourcode == null))
                                {
                                    var Listofserials = source.Billitems.ConvertAll(e => e.attributes == null ? e.barcode : (string.IsNullOrEmpty(e.attributes.serialno) ? e.barcode : e.attributes.serialno));
                                    Listofserials.RemoveAll(e => e == "");
                                    if (Listofserials.Count > 0)
                                    {
                                        MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                                        var designlist = mfgdesignDataService.SearchDesignList(Listofserials);
                                        foreach (var item in source.Billitems)
                                        {
                                            if (item.attributes != null && !string.IsNullOrEmpty(item.attributes.serialno))
                                            {
                                                if (designlist.Exists(e => item.attributes != null && e.uid == item.attributes.serialno))
                                                {
                                                    item.mfgdesign = designlist.Find(e => e.uid == item.attributes.serialno);
                                                    item.mrp = designlist.Find(e => e.uid == item.attributes.serialno).mrp;
                                                    if (source.customer != null && source.customer.mrpsource == 1)
                                                        item.mrp = item.mfgdesign.wsrate;
                                                    if (source.customer != null &&
                                                        source.customer.mrpsource == 2)
                                                        item.mrp = item.mfgdesign.dealerprice;
                                                    if (source.customer != null &&
                                                        source.customer.mrpsource == 3)
                                                        item.mrp = item.mfgdesign.salerate;
                                                }
                                            }
                                            else
                                            {
                                                if (designlist.Exists(e => item.attributes != null && e.uid == item.barcode))
                                                {
                                                    item.mfgdesign = designlist.Find(e => e.uid == item.barcode);
                                                    item.mrp = designlist.Find(e => e.uid == item.barcode).mrp;
                                                    if (source.customer != null && source.customer.mrpsource == 1)
                                                        item.mrp = item.mfgdesign.wsrate;
                                                    if (source.customer != null &&
                                                        source.customer.mrpsource == 2)
                                                        item.mrp = item.mfgdesign.dealerprice;
                                                    if (source.customer != null &&
                                                        source.customer.mrpsource == 3)
                                                        item.mrp = item.mfgdesign.salerate;
                                                }
                                            }
                                        }
                                    }
                                }
                                if (reportformats[0].reportformat.format.Contains("etdiscount"))
                                {
                                    var total = Math.Round(Math.Round(source.etgross, 2) -
                                         Math.Round(source.etdiscount, 2) +
                                         Math.Round(source.PackingAddCharge.Charge, 2) +
                                         Math.Round(source.PackingRemoveCharge.Charge, 2) +
                                         Math.Round(source.isinterstatesale ? source.totaltax.sgsttaxamount : 0, 2) +
                                         Math.Round(!source.isinterstatesale ? source.totaltax.cgsttaxamount : 0, 2) +
                                         Math.Round(!source.isinterstatesale ? source.totaltax.sgsttaxamount : 0, 2) -
                                         source.additionalchargestaxamount, 2);
                                    source.receivable = total;
                                    source.rounding = Math.Round(total, MidpointRounding.AwayFromZero) - total;
                                }
                            }
                        }
                    }





                    if (labreport.format.PageHeader != null &&
                        !string.IsNullOrEmpty(labreport.format.PageHeader.Style))
                    {
                        labreport.BodyStyle.Font = labreport.format.GetStyle(labreport.format.PageHeader.Style);
                        labreport.HeaderStyle.Font = new System.Drawing.Font(labreport.BodyStyle.Font, System.Drawing.FontStyle.Bold);
                        labreport.TotalStyle.Font = new System.Drawing.Font(labreport.BodyStyle.Font, System.Drawing.FontStyle.Bold);
                    }
                    //if (isthermalprinter)
                    //    labreport.ConvertSize2Thermal();

                    if (reportformats[0].reportformat.format.Contains("taxdetail.") ||
                        reportformats[0].reportformat.format.Contains("BillHSN"))
                    {
                        foreach (var item in source.Billitems)
                        {
                            if (source.isinterstatesale)
                            {
                                item.taxdetail = new Billitems.ItemTaxDetail()
                                {
                                    totaltax = item.tax,
                                    igst_percentage = item.taxpercentage,
                                    igst_value = item.tax
                                };
                            }
                            else
                            {
                                item.taxdetail = new Billitems.ItemTaxDetail()
                                {
                                    totaltax = item.tax,
                                    cgst_percentage = item.taxpercentage / 2,
                                    cgst_value = Math.Round(item.tax / 2, 4),
                                    sgst_percentage = item.taxpercentage / 2,
                                    sgst_value = Math.Round(item.tax / 2, 4)
                                };
                            }
                        }
                    }

                    if (reportformats[0].reportformat.format.Contains("BillHSN"))
                    {
                        source.BillHSN = new List<BillHSN>();
                        foreach (var grp in source.Billitems.GroupBy(e => new { e.hsncode, e.taxpercentage }))
                        {
                            source.BillHSN.Add(new BillHSN()
                            {
                                code = grp.Key.hsncode,
                                taxable = grp.Sum(e => e.gross),
                                tax = grp.Sum(e => e.tax),
                                total = grp.Sum(e => e.receivable),
                                qty = grp.Sum(e => e.qty),
                                peice = grp.Sum(e => e.piece),
                                cgst_percentage = grp.First().taxdetail.cgst_percentage,
                                igst_percentage = grp.First().taxdetail.igst_percentage,
                                sgst_percentage = grp.First().taxdetail.sgst_percentage,
                                sgst_value = grp.Sum(e => e.taxdetail.sgst_value),
                                cgst_value = grp.Sum(e => e.taxdetail.cgst_value),
                                igst_value = grp.Sum(e => e.taxdetail.igst_value)
                            });
                        }
                    }

                    if (reportformats[0].reportformat.format.Contains("BillHSN"))
                    {
                        if (source.additionalchargeslist != null &&
                            source.additionalchargeslist.Count > 0)
                        {

                            if (source.additionalchargeslist.Exists(x => x.chargetype == "EXPORTGST"))
                            {
                                var exportgst = source.additionalchargeslist.Find(x => x.chargetype == "EXPORTGST");
                                source.BillHSN.ForEach(x =>
                                {
                                    x.cgst_percentage = source.isinterstatesale ? 0 : exportgst.taxpercentage / 2;
                                    x.sgst_percentage = source.isinterstatesale ? 0 : exportgst.taxpercentage / 2;
                                    x.igst_percentage = !source.isinterstatesale ? 0 : exportgst.taxpercentage;
                                    if (!source.isinterstatesale)
                                    {
                                        x.sgst_value = x.taxable * x.sgst_percentage / 100;
                                        x.cgst_value = x.taxable * x.cgst_percentage / 100;
                                        x.tax = x.cgst_value + x.sgst_value;
                                    }
                                    else
                                    {
                                        x.igst_value = x.taxable * x.igst_percentage / 100;
                                        x.tax = x.igst_value;
                                    }
                                    x.total = x.taxable + x.tax;
                                });
                            }

                            foreach (var grp in source.additionalchargeslist.Where(f => f.chargetype != "EXPORTGST").GroupBy(x => new { x.hsncode, x.taxpercentage }))
                            {
                                source.BillHSN.Add(new BillHSN()
                                {
                                    code = grp.Key.hsncode,
                                    taxable = grp.Sum(e => e.Amount - e.Tax),
                                    tax = grp.Sum(e => e.Tax),
                                    total = grp.Sum(e => e.Amount),
                                    qty = grp.Count(),
                                    peice = grp.Count(),
                                    cgst_percentage = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Key.taxpercentage / 2,
                                    igst_percentage = !source.isinterstatesale ? 0 : grp.Key.taxpercentage,
                                    sgst_percentage = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Key.taxpercentage / 2,
                                    sgst_value = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Sum(e => e.Tax / 2),
                                    cgst_value = source.isinterstatesale ? 0 : grp.Key.taxpercentage <= 0 ? 0 : grp.Sum(e => e.Tax / 2),
                                    igst_value = !source.isinterstatesale ? 0 : grp.Sum(e => e.Tax)
                                });
                            }

                        }
                    }

                    if (reportformats[0].reportformat.format.Contains("salesorder.") && source.billattributes != null &&
                        !string.IsNullOrEmpty(source.billattributes.orderrefid))
                    {
                        SalesorderDataService salesorderDataService = new SalesorderDataService();
                        var salesorder = salesorderDataService.Search(new SalesorderCreteria()
                        {
                            sono = source.billattributes.orderrefid,
                            onlyheader =true
                        });
                        if (salesorder.Count>0 && (salesorder.Count == 1))
                        {
                            source.salesorder = salesorder[0];
                        }
                    }

                        if (reportformats[0].reportformat.format.Contains("mfgdesign."))
                    {
                        var Listofserials = source.Billitems.ConvertAll(e => e.attributes == null ? e.barcode : (string.IsNullOrEmpty(e.attributes.serialno) ? e.barcode : e.attributes.serialno));
                        Listofserials.RemoveAll(e => e == "");
                        if (Listofserials.Count > 0)
                        {
                            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                            var designlist = mfgdesignDataService.SearchDesignList(Listofserials);
                            foreach (var item in source.Billitems)
                            {
                                if (item.attributes != null && !string.IsNullOrEmpty(item.attributes.serialno))
                                {
                                    if (designlist.Exists(e => item.attributes != null && e.uid == item.attributes.serialno))
                                    {
                                        item.mfgdesign = designlist.Find(e => e.uid == item.attributes.serialno);
                                        item.mrp = designlist.Find(e => e.uid == item.attributes.serialno).mrp;
                                        if (source.customer != null && source.customer.mrpsource == 1)
                                            item.mrp = item.mfgdesign.wsrate;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 2)
                                            item.mrp = item.mfgdesign.dealerprice;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 3)
                                            item.mrp = item.mfgdesign.salerate;
                                    }
                                }
                                else
                                {
                                    if (designlist.Exists(e => item.attributes != null && e.uid == item.barcode))
                                    {
                                        item.mfgdesign = designlist.Find(e => e.uid == item.barcode);
                                        item.mrp = designlist.Find(e => e.uid == item.barcode).mrp;
                                        if (source.customer != null && source.customer.mrpsource == 1)
                                            item.mrp = item.mfgdesign.wsrate;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 2)
                                            item.mrp = item.mfgdesign.dealerprice;
                                        if (source.customer != null &&
                                            source.customer.mrpsource == 3)
                                            item.mrp = item.mfgdesign.salerate;

                                    }
                                }
                            }
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("salesref"))
                    {
                        if (source.billattributes != null &&
                            source.billattributes.salesrefid > 0)
                        {
                            EmployeeDataService employeeDataService = new EmployeeDataService();
                            source.salesref = employeeDataService.SearchHeader(new EmployeeCreteria()
                            {
                                id = source.billattributes.salesrefid
                            }).First().name;
                        }
                    }

                    if (source.hascustomer && reportformats[0].reportformat.format != null &&
                       reportformats[0].reportformat.format.Contains("transport"))
                    {
                        source.transportcity = source.customer.cityname;
                        if (source.customer.preferredtransportid > 0)
                        {
                            TransportDataService transportDataService = new TransportDataService();
                            source.transportname = transportDataService.Search(new TransportCreteria()
                            {
                                id = source.customer.preferredtransportid
                            }).First().name;
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                        (reportformats[0].reportformat.format.Contains("fburl") ||
                        reportformats[0].reportformat.format.Contains("fburl")))
                    {
                        var BillDataService = new BillDataService();
                        source.fburl = BillDataService.GetFBURL(source.billno);
                    }

                    if (source.hassettlement &&
                        source.customerid > 0 &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryenable &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount > 0)
                    {
                        if (reportformats[0].reportformat.format != null &&
                                                reportformats[0].reportformat.format.Contains("Lottery_") &&
                                                source.billattributes.settlement.receivable >= InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount)
                        {
                            var noofcoupons = Math.Floor(source.billattributes.settlement.receivable /
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotteryamount);
                            if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax > 0 &&
                                InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax < noofcoupons)
                                noofcoupons = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_lotterymax;
                            source.billattributes.settlement.Lottery_Avilable = true;
                            source.billattributes.settlement.Lottery_Count = Convert.ToInt32(noofcoupons);
                            source.billattributes.settlement.customer = source.customer;
                        }
                    }


                    if (source.hassettlement &&
                        source.customerid > 0 && 
                        reportformats[0].reportformat.format != null &&
                      reportformats[0].reportformat.format.Contains("settlement.coupons") &&
                      source.billattributes.settlement.coupons != null)
                    {
                        source.billattributes.settlement.coupons.ForEach(x => x.customer = source.customer);
                    }

                    if (reportformats[0].reportformat.format != null &&
                                            reportformats[0].reportformat.format.Contains("HasFreeGift"))
                    {
                        source.FreeItems = new List<BillItemAttributes.FreeItems>();
                        source.Billitems.ForEach(x =>
                        {
                            if (x.attributes != null &&
                               x.attributes.freeitems != null &&
                               x.attributes.freeitems.Count > 0)
                                source.FreeItems.AddRange(x.attributes.freeitems);
                        });
                    }

                    if (reportformats[0].reportformat.format != null &&
                        (reportformats[0].reportformat.format.Contains("approvedby") ||
                        reportformats[0].reportformat.format.Contains("approvedby")))
                    {
                        if (source.billattributes != null &&
                            source.billattributes.approvedby > 0)
                        {
                            EmployeeDataService employeeDataService = new EmployeeDataService();
                            source.approvedby = employeeDataService.SearchHeader(new EmployeeCreteria()
                            {
                                id = source.billattributes.approvedby
                            }).First().name;
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                            (reportformats[0].reportformat.format.Contains("biller") ||
                            reportformats[0].reportformat.format.Contains("biller")))
                    {
                        if (source.billattributes != null && string.IsNullOrEmpty(source.billattributes.biller))
                        {
                            UsersDataService usersDataService = new UsersDataService();
                            source.billattributes.biller = usersDataService.Search(new UsersCreteria()
                            {
                                id = source.createdby
                            }).First().title;
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                           (reportformats[0].reportformat.format.Contains("boxname") ||
                           reportformats[0].reportformat.format.Contains("boxname")))
                    {
                        source.Billitems.FindAll(x => x.attributes.packing != 0).ForEach(x => x.attributes.boxname = x.attributes.packing > 0 ? ("Added - " + x.attributes.packing.ToString("N0")) : ("Removed " + x.attributes.packing.ToString("N0")));
                        var allboxes = source.Billitems.FindAll(x => x.attributes.boxid != 0).ConvertAll(x => x.attributes.boxid).ToList().Distinct().ToList();
                        if (allboxes.Count > 0)
                        {
                            var boxnames = referencelistDataService.SearchList(new ReferencelistCreteria()
                            {
                                ids = allboxes
                            });
                            boxnames.ForEach(x =>
                            {
                                source.Billitems.FindAll(y => y.attributes.boxid == x.id).ForEach(z =>
                                {
                                    z.attributes.boxname = x.name;
                                });
                            });
                        }
                    }



                    if (reportformats[0].reportformat.format != null &&
                    (reportformats[0].reportformat.format.Contains("billercode") ||
                    reportformats[0].reportformat.format.Contains("billercode")))
                    {
                        if (source.billattributes != null && string.IsNullOrEmpty(source.billattributes.biller))
                        {
                            UsersDataService usersDataService = new UsersDataService();
                            var employeeid = usersDataService.Search(new UsersCreteria()
                            {
                                id = source.createdby
                            }).First().employeeid;
                            if (employeeid > 0)
                            {
                                EmployeeDataService employeeDataService = new EmployeeDataService();
                                source.billattributes.billercode = employeeDataService.SearchHeader(new EmployeeCreteria()
                                {
                                    id = employeeid
                                }).First().employeecode;
                            }
                        }
                    }

                    if (reportformats[0].reportformat.format != null &&
                    (reportformats[0].reportformat.format.Contains("qtytometer") ||
                    reportformats[0].reportformat.format.Contains("qtytopiece")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateSellingMode(source.Billitems);
                    }
                    else if (reportformats[0].reportformat.format != null &&
                        // !reportformats[0].reportformat.format.Contains("orderid") &&
                        (reportformats[0].reportformat.format.Contains("qtyinmeter") ||
                        reportformats[0].reportformat.format.Contains("qtyinpiece")))
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateSellingMode(source.Billitems);
                        actualBillitems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Billitems>>(Newtonsoft.Json.JsonConvert.SerializeObject(source.Billitems));
                        var groupedItems = source.Billitems.GroupBy(e => new
                        {
                            e.printingname,
                            e.barcode,
                            e.qty,
                            e.salesmanid,
                            e.discount,
                            e.additionaldiscount,
                            e.sellingmode
                        });
                        List<Billitems> newitems = new List<Billitems>();
                        foreach (var gitem in groupedItems)
                        {
                            var newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem.First()));

                            if (gitem.Key.sellingmode == 3)
                            {
                                newbillitem.qtyinmeter = gitem.First().qty.ToString("N2");
                                newbillitem.qtyinpiece = gitem.Count().ToString("N0");
                            }
                            else
                            {
                                newbillitem.qtyinpiece = gitem.Sum(e => e.qty).ToString("N0");
                            }
                            if (string.IsNullOrEmpty(lineitemsource))
                            {
                                orderid++;
                                newbillitem.orderid = orderid;
                            }
                            newbillitem.qty = gitem.Sum(e => e.qty);
                            newbillitem.receivable = gitem.Sum(e => e.receivable);
                            newbillitem.discount = gitem.Sum(e => e.discount);
                            newbillitem.meteramount = gitem.Sum(e => e.amount);
                            newbillitem.schemediscount = gitem.Sum(e => e.schemediscount);
                            newbillitem.discountpercentage = gitem.Average(e => e.discountpercentage);
                            newbillitem.additionaldiscountpercentage = gitem.Average(e => e.additionaldiscountpercentage);
                            newbillitem.additionaldiscount = gitem.Average(e => e.additionaldiscount);
                            newbillitem.saleamount = gitem.Sum(e => e.rate * e.qty);
                            newbillitem.taxsplit = new TaxSplit();
                            newbillitem.taxsplit.cgsttaxamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.cgsttaxamount : 0));
                            newbillitem.taxsplit.sgsttaxamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.sgsttaxamount : 0));
                            newbillitem.taxsplit.totalamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.totalamount : 0));
                            newbillitem.taxsplit.name = gitem.First().taxsplit.name;
                            newbillitem.taxsplit.secondtax = gitem.First().taxsplit.secondtax;
                            newbillitem.taxsplit.cgsttaxpercentage = gitem.First().taxsplit.cgsttaxpercentage;
                            newbillitem.taxsplit.sgsttaxpercentage = gitem.First().taxsplit.sgsttaxpercentage;
                            newitems.Add(newbillitem);
                        }
                        source.Billitems = newitems;
                    }
                    else if (reportformats[0].reportformat.format != null &&
                        reportformats[0].reportformat.format.Contains("orderid") &&
                        (reportformats[0].reportformat.format.Contains("qtyinmeter") ||
                        reportformats[0].reportformat.format.Contains("qtyinpiece")))
                    {
                        foreach (var bitem in source.Billitems)
                        {
                            if (bitem.sellingmode == 3)
                            {
                                bitem.qtyinmeter = bitem.qty.ToString("N2");
                                bitem.qtyinpiece = "1";
                            }
                            else
                                bitem.qtyinpiece = bitem.qty.ToString("N0");
                        }
                    }
                    else if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_mergebillprint)
                    {
                        BillitemsDataService billitemsDataService = new BillitemsDataService();
                        billitemsDataService.UpdateSellingMode(source.Billitems);
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_ismfgapplication &&
                        InfyPOS.Services.ConfigManager.Instance(organizationid).Config.mfg_mode == 5)
                    {
                        var groupedItems = source.Billitems.GroupBy(e => new
                        {
                            e.attributes.serialno,
                            e.taxpercentage,
                            e.attributes.colourcode,
                            e.attributes.colourname,
                            e.mrpdisplay,
                            e.etprice
                        });
                        List<Billitems> newitems = new List<Billitems>();
                        foreach (var gitem in groupedItems)
                        {
                            var newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem.First()));
                            orderid++;
                            newbillitem.orderid = orderid;
                            newbillitem.qty = gitem.Sum(e => e.qty);
                            newbillitem.tax = gitem.Sum(e => e.tax);
                            newbillitem.receivable = gitem.Sum(e => e.receivable);
                            newbillitem.discount = gitem.Sum(e => e.discount);
                            newbillitem.meteramount = gitem.Sum(e => e.amount);
                            newbillitem.schemediscount = gitem.Sum(e => e.schemediscount);
                            newbillitem.discountpercentage = gitem.Average(e => e.discountpercentage);
                            newbillitem.additionaldiscountpercentage = gitem.Average(e => e.additionaldiscountpercentage);
                            newbillitem.additionaldiscount = gitem.Sum(e => e.additionaldiscount);
                            newbillitem.saleamount = gitem.Sum(e => e.rate * e.qty);
                            newbillitem.taxsplit = new TaxSplit();
                            newbillitem.taxsplit.cgsttaxamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.cgsttaxamount : 0));
                            newbillitem.taxsplit.sgsttaxamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.sgsttaxamount : 0));
                            newbillitem.taxsplit.totalamount = gitem.Sum(e => (e.taxsplit != null ? e.taxsplit.totalamount : 0));
                            newbillitem.taxsplit.name = gitem.First().taxsplit.name;
                            newbillitem.taxsplit.secondtax = gitem.First().taxsplit.secondtax;
                            newbillitem.taxsplit.cgsttaxpercentage = gitem.First().taxsplit.cgsttaxpercentage;
                            newbillitem.taxsplit.sgsttaxpercentage = gitem.First().taxsplit.sgsttaxpercentage;
                            newitems.Add(newbillitem);
                        }
                        source.Billitems = newitems;
                    }

                    if (billitemSorting != Bill.BillitemSorting.None)
                    {
                        if (billitemSorting == Bill.BillitemSorting.Name)
                            source.Billitems = source.Billitems.OrderBy(x => x.printingname).ToList();
                        if (billitemSorting == Bill.BillitemSorting.Price)
                            source.Billitems = source.Billitems.OrderBy(x => x.etprice + x.price + x.salerate).ToList();
                        if (billitemSorting == Bill.BillitemSorting.Amount)
                            source.Billitems = source.Billitems.OrderBy(x => x.etgross + x.receivable).ToList();
                    }

                    labreport.DataSource = source;
                    labreport.DataItems = source.Billitems;

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

                    if (source.company != null && !string.IsNullOrEmpty(source.company.logo))
                    {
                        var file = imageData.GetData(source.company.logo.LongFromBase62());
                        if (file != null && file.data != null && file.data.Length > 0)
                        {
                            var img = imageData.FromBytes(file.data);
                            labreport.Additional.Add("LOGO", img);
                        }
                    }

                    if (string.IsNullOrEmpty(lineitemsource))
                    {
                        orderid = source.Billitems.Max(e => e.orderid);
                    }
                    lastOrderID = orderid;
                    var haspagenumber = reportformats[0].reportformat.format.Contains("PageCount");

                    if (source.billdeliverydetails != null && source.billdeliverydetails.billinbox != null &&
                        source.billdeliverydetails.billinbox.haseinvoice &&
                       source.billdeliverydetails.billinbox.einvoicejosn != null &&
                       source.billdeliverydetails.billinbox.einvoicejosn != "null")
                    {
                        InfyGST.GSTPortal.EInvoiceResponse res = JsonConvert.DeserializeObject<InfyGST.GSTPortal.EInvoiceResponse>(source.billdeliverydetails.billinbox.einvoicejosn);
                        var img = imageData.FromBytes(Convert.FromBase64String(res.QrCodeImage));
                        source.billdeliverydetails.GSTEinvoice = res;
                        labreport.Additional.Add("IMG-" + "EINVQR", img);
                        var hsnlist = source.Billitems.ConvertAll(x => new { x.hsncode, x.printingname, x.qty });
                        source.billdeliverydetails.HSNList = "";
                        foreach (var item in hsnlist.GroupBy(x => new { x.hsncode, x.printingname }).OrderBy(x => x.Sum(y => y.qty)))
                        {
                            if (source.billdeliverydetails.HSNList != "") source.billdeliverydetails.HSNList += ",";
                            source.billdeliverydetails.HSNList += item.Key.hsncode + " - " + item.Key.printingname + "(" + item.Sum(x => x.qty) + ")";
                            if (source.billdeliverydetails.HSNList.Length > 100) break;
                        }
                    }

                    if (source.billdeliverydetails != null && source.billdeliverydetails.billinbox != null &&
                        source.billdeliverydetails.billinbox.haseway &&
                       source.billdeliverydetails.billinbox.ewayjson != null &&
                       source.billdeliverydetails.billinbox.ewayjson != "null")
                    {
                        if (source.billdeliverydetails.billinbox.ewayjson.EndsWith(@"""") ||
                            source.billdeliverydetails.billinbox.ewayjson.StartsWith(@""""))
                        {
                            var js = source.billdeliverydetails.billinbox.ewayjson.Replace(@"\""", "\"");
                            js = js.Replace(@"\\", @"\");
                            source.billdeliverydetails.billinbox.ewayjson = js.Substring(1, js.Length - 2);

                        }
                        var ewayres = JsonConvert.DeserializeObject<InfyGST.GSTPortal.EwayFromGRNResponse>(source.billdeliverydetails.billinbox.ewayjson);
                        source.billdeliverydetails.GSTEWay = ewayres;
                    }

                    if (reportformats[0].reportformat.format != null &&
                      reportformats[0].reportformat.format.Contains("salesmencodes"))
                    {
                        List<long> salesmenids = new List<long>();
                        foreach (var bi in source.Billitems)
                        {
                            if (bi.salesmanid > 0)
                                salesmenids.Add(bi.salesmanid);
                            if (bi.attributes != null &&
                            bi.attributes.smcodes != null &&
                            bi.attributes.smcodes.Count > 0)
                            {
                                salesmenids.AddRange(bi.attributes.smcodes);
                            }
                        }

                        salesmenids.RemoveAll(x => x ==0);
                        EmployeeDataService agentdatasource = new EmployeeDataService();
                        var salesmens = string.Join(",", agentdatasource.Search(new EmployeeCreteria()
                        {
                            ids = salesmenids.Distinct().ToList(),
                            hidesalaryinfo = true
                        }).ConvertAll(x => x.codes));
                        source.salesmencodes = salesmens;
                    }

                    if (source.isb2b)
                    {
                        decimal totalafterround = Math.Round(source.total, MidpointRounding.AwayFromZero);
                        if (source.total != totalafterround)
                        {
                            //source.receivable = Math.Round(source.receivable, MidpointRounding.AwayFromZero);
                            //source.rounding = totalafterround - source.receivable;
                            source.rounding = Math.Round(source.receivable + source.additionalcharges, MidpointRounding.AwayFromZero) - (source.receivable + source.additionalcharges);
                        }
                    }

                    foreach (var item in source.Billitems)
                    {
                        if (!item.haslastbit && (item.attributes != null && item.attributes.islastbit))
                            item.journalqty = (item.qty < 0 ? (item.qty * -1) + item.attributes.lastbitqty : item.qty + item.attributes.lastbitqty);
                    }


                    if (source.hassettlement &&
                        source.customerid > 0 &&
                        reportformats[0].reportformat.format != null &&
                      reportformats[0].reportformat.format.Contains("settlement.coupons") &&
                      source.billattributes.settlement.coupons !=null)
                    {
                        source.billattributes.settlement.coupons.ForEach(x => x.customer = source.customer);
                    }

                    if (json)
                    {
                        ReportLibrary.JSONPrinter printer = new ReportLibrary.JSONPrinter();
                        printer.Report = labreport;
                        printer.IsThermalPrint = isthermalprinter;
                        printer.EnablePageNumber = haspagenumber;
                        if (InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                                InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle != null &&
                                InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle != "")
                        {


                            printer.TitleList = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle.Split(','); ;
                            var document = printer.Print(labreport.format.GetDefaultSetting());
                            document.noofcopy = reportformats[0].noofcopy;
                            FileContentInfo fci = null;
                            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_saleunicodepdf)
                                fci = new FileContentInfo(new ByteArrayContent(System.Text.UnicodeEncoding.Unicode.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document))));
                            else
                                fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document))));
                            fci.FileName = "billsettlement.json";
                            fci.Extension = "application/json";
                            return fci;
                        }
                        else
                        {
                            var document = printer.Print(labreport.format.GetDefaultSetting());
                            document.noofcopy = reportformats[0].noofcopy;
                            FileContentInfo fci = null;
                            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_saleunicodepdf)
                                fci = new FileContentInfo(new ByteArrayContent(System.Text.UnicodeEncoding.Unicode.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document))));
                            else
                                fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document))));
                            fci.FileName = "billsettlement.json";
                            fci.Extension = "application/json";
                            return fci;
                        }
                    }
                    else
                    {
                        ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                        printer.Report = labreport;
                        printer.EnablePageNumber = haspagenumber;
                        printer.IsThermalPrint = isthermalprinter;
                        if (!isb2bdeliveryslip &&
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig.ContainsKey(source.locationid) &&
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle != null &&
                            InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle != "")
                        {
                            printer.TitleList = InfyPOS.Services.ConfigManager.Instance(organizationid).LocationConfig[source.locationid].sale_billcopytitle.Split(','); ;
                            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                            {
                                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                                fci.FileName = "billsettlement.pdf";
                                fci.Extension = "application/pdf";
                                return fci;
                            }
                        }
                        else
                        {
                            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                            {
                                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                                fci.FileName = "billsettlement.pdf";
                                fci.Extension = "application/pdf";
                                return fci;
                            }
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    throw exp;
                }
            }
            else
            {
                throw new Exception("SETTLEMENT NOT CONFIGURED FOR THE SELECTED COMPANY - " + (companyforlogo.Count > 0 ? companyforlogo[0].name : source.companyid.ToString()));
            }
        }
    }
}