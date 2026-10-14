
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
    [RoutePrefix("LrinvoiceService")]
    public class LrinvoiceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Lrinvoice>();
            result.Item = new Lrinvoice();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LrinvoiceCreteria> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            List<Lrinvoice> result = await Task<List<Lrinvoice>>.Run(() => LrinvoiceData.Search(request.Item));
            if(result.Count == 0 &&
                request.Item.importreferenceid>0 && 
                request.Item.importreferencetype!=0)
            {
                return this.Ok(new LrinvoiceCover() { lrid = -1, errormessage = "It's already cancelled,please create / import again" });
            }
            if (LrinvoiceData.Context.accessiblecompany != null && LrinvoiceData.Context.accessiblecompany.Count > 0)
            {
                if (!LrinvoiceData.Context.accessiblecompany.Exists(e => e == -1))
                {
                    result.RemoveAll(e => e.companyid > 0 && !LrinvoiceData.Context.accessiblecompany.Exists(x => x == e.companyid));
                }
            }
            if (InfyPOS.Services.ConfigManager.Instance(LrinvoiceData.Context.OrganizationID).Config.system_showroompurchase &&
                LrinvoiceData.Context.accessiblelocation != null && LrinvoiceData.Context.accessiblelocation.Count > 0)
            {
                if (!LrinvoiceData.Context.accessiblelocation.Exists(e => e == -1))
                {
                    result.RemoveAll(e => e.entrylocationid > 0 && !LrinvoiceData.Context.accessiblelocation.Exists(x => x == e.entrylocationid));
                }
            }

            if ((request.Item.id > 0 || request.Item.lrid > 0) || (request.Item.importreferenceid > 0 && request.Item.importreferencetype > 0))
            {
                LrinvoiceCover cover = new LrinvoiceCover();
                cover.Items = result;
                cover.supportpo = result.Count > 0 && result[0].supportpo;
                cover.lrid = request.Item.lrid > 0 ? request.Item.lrid : result[0].lrid;
                LrentryDataService LrentryinvoiceData = new LrentryDataService();
                var lrinfo = LrentryinvoiceData.Search(new LrentryCreteria()
                {
                    id = cover.lrid,
                    importreferencetype = (Lrentry.ImportReferenceTypes)request.Item.importreferencetype,
                    importreferenceid = request.Item.importreferenceid
                });


                if (request.Item.includetdsrule)
                {
                    if (result.Count > 0)
                    {
                        cover.tdsinfo = LrentryinvoiceData.ComputeTDS(result[0]);
                    }
                    else
                    {
                        SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService();
                        var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
                        {
                            supplierid = lrinfo[0].supplierid
                        });
                        if (suppliercompany.Count > 0)
                        {
                            cover.tdsinfo = LrentryinvoiceData.ComputeTDS(new Lrinvoice()
                            {
                                suppliercompanyid = suppliercompany[0].id,
                                entrydate = DateTime.Today
                            });
                        }
                    }
                }

                if (lrinfo != null && lrinfo.Count == 1)
                {
                    cover.suppliercompanyid = result.Count > 0 && result[0].suppliercompanyid > 0 ? result[0].suppliercompanyid : 0;
                    cover.lrentryno = lrinfo[0].lrentryno;
                    cover.lrno = lrinfo[0].lrno;
                    cover.entrydate = result.Count > 0 ? result[0].entrydate : lrinfo[0].lrentrydate;
                    cover.supplierid = lrinfo[0].supplierid;
                    cover.sourcesupplierid = lrinfo[0].sourcesupplierid;
                    cover.status = lrinfo[0].Status;
                    cover.modeofdelivery = lrinfo[0].modeofdelivery;
                    cover.invoicedate = lrinfo[0].invoicedate;
                    cover.invoiceno = lrinfo[0].invoiceno;
                    cover.retaillocationid = lrinfo[0].retaillocationid;
                    cover.noofpieces = lrinfo[0].noofpieces;
                    cover.noofbundels = lrinfo[0].noofbundels;
                    cover.noofboxes = lrinfo[0].noofboxes;
                    cover.transportid = lrinfo[0].transportid;
                    cover.premargin = lrinfo[0].premargin;
                    cover.receiveddate = lrinfo[0].receiveddate;
                    cover.lrdate = lrinfo[0].lrdate;
                    cover.companyid = lrinfo[0].lrcompanyid;
                    cover.receivinglocationid = lrinfo[0].receivinglocationid;

                    LrreceivedlistDataService lrreceivedDataService = new LrreceivedlistDataService();
                    var lrreceived = lrreceivedDataService.Search(new LrreceivedlistCreteria()
                    {
                        lrid = lrinfo[0].id,
                        onlyreceived = true
                    });
                    cover.frightcharge = lrreceived.Sum(e => e.frightcharge);
                    cover.loadingcharge = lrreceived.Sum(e => e.loadingcharge);
                    cover.insurancecharge = lrreceived.Sum(e => e.insurance);
                    cover.packingcharge = lrreceived.Sum(e => e.packing);
                    cover.forwardcharge = lrreceived.Sum(e => e.forward);
                    cover.otherscharge = lrreceived.Sum(e => e.others);

                    cover.agentpercentage = lrinfo[0].agentcommission;
                    if (lrinfo[0].agentcommission > 0)
                    {
                        cover.agentcommission = Math.Round(result.Sum(e => e.grossamount - e.taxamount) * lrinfo[0].agentcommission / 100, 0);
                    }
                    cover.actualweight = lrreceived.Sum(e => e.actualweight);
                    cover.receivedweight = lrreceived.Sum(e => e.receivedweight);
                    cover.chargedweight = lrreceived.Sum(e => e.chargedweight);
                    cover.lrcomputedexpense = cover.agentcommission + lrreceived.Sum(e => e.frightcharge + e.loadingcharge + e.forward + e.packing + e.others + e.insurance);
                    cover.lrexpense = lrinfo[0].lrexpense;
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    if (lrinfo[0].fromlocationid > 0)
                    {
                        cover.city = referencelistDataService.SearchList(new ReferencelistCreteria()
                        {
                            id = lrinfo[0].fromlocationid,
                            inactive = true
                        }).First().name;
                    }

                    if (lrinfo[0].transportid > 0)
                    {
                        TransportDataService transportDataService = new TransportDataService();
                        cover.transport = transportDataService.Search(new TransportCreteria()
                        {
                            id = lrinfo[0].transportid
                        }).First().name;
                    }

                    if (lrinfo[0].retaillocationid > 0)
                    {

                        var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = lrinfo[0].retaillocationid
                        });
                        if (locationinfo != null && locationinfo.Count == 1 && locationinfo[0].additionalinfo != null &&
                            locationinfo[0].additionalinfo.parentid > 0)
                        {
                            cover.companyid = locationinfo[0].additionalinfo.parentid;
                        }
                    }
                    if (cover.sourcesupplierid > 0 && cover.sourcesupplierid != cover.supplierid)
                    {
                        SupplierDataService suppdata = new SupplierDataService();
                        List<Supplier> supplierlist = suppdata.Search(new SupplierCreteria()
                        {
                            id = cover.sourcesupplierid
                        });
                        if (supplierlist.Count > 0)
                            cover.sourcesuppliername = supplierlist[0].name;
                    }
                    if (result.Count == 1)
                    {
                        cover.totalamount = result[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Taxable) ? e.amounton : 0);
                        cover.totaldiscount = result[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Taxable ||
                        e.type == Lrinvoicetax.EntryType.Discount ||
                        e.type == Lrinvoicetax.EntryType.DiscuntOnDiscount) ? e.discount : 0);
                        cover.totaltax = result[0].Lrinvoicetaxitems.Sum(e => e.taxamount);

                        cover.additionalamount = result[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Adjustment ||
                        e.type == Lrinvoicetax.EntryType.Courier || e.type == Lrinvoicetax.EntryType.Fright ||
                        e.type == Lrinvoicetax.EntryType.Insurance || e.type == Lrinvoicetax.EntryType.Packing ||
                        e.type == Lrinvoicetax.EntryType.ServiceCharge) ? e.amounton : 0);
                        cover.additionaltax = result[0].Lrinvoicetaxitems.Sum(e => (e.type == Lrinvoicetax.EntryType.Adjustment ||
                        e.type == Lrinvoicetax.EntryType.Courier || e.type == Lrinvoicetax.EntryType.Fright ||
                        e.type == Lrinvoicetax.EntryType.Insurance || e.type == Lrinvoicetax.EntryType.Packing ||
                        e.type == Lrinvoicetax.EntryType.ServiceCharge) ? e.taxamount : 0);
                        if (cover.totalamount - cover.totaldiscount > 0)
                            cover.addtionalcostpercentage = Math.Round((cover.additionalamount + cover.additionaltax) / (cover.totalamount - cover.totaldiscount) * 100, 4);
                    }
                }


                if (lrinfo.Count > 0 && lrinfo[0].lrcompanyid > 0 && cover.companyid != lrinfo[0].lrcompanyid)
                {
                    CompanyDataService sDataService = new CompanyDataService();
                    List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()
                    {
                        id = lrinfo[0].lrcompanyid
                    }));

                    if (buyingcompany.Count > 0 && buyingcompany.First().internalbuyer)
                    {
                        cover.companyid = lrinfo[0].lrcompanyid;
                        if (cover.premargin == 0)
                            cover.premargin = buyingcompany.First().internalmargin;
                    }
                }

                if (result.Count == 1)
                {
                    if (result[0].suppliercompanyid > 0)
                    {
                        SuppliercompanyDataService sDataService = new SuppliercompanyDataService();
                        List<Suppliercompany> suppliercompany = await Task<List<Suppliercompany>>.Run(() => sDataService.Search(new SuppliercompanyCreteria()
                        { id = result[0].suppliercompanyid }));
                        if (suppliercompany.Count == 1)
                        {
                            cover.suppliercompanyname = suppliercompany[0].name;
                            cover.suppliergstno = suppliercompany[0].tinanddate;
                        }
                    }

                    if (result[0].companyid > 0)
                    {
                        CompanyDataService sDataService = new CompanyDataService();
                        List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()
                        {
                            id = result[0].companyid
                        }
                        ));
                        if (buyingcompany.Count == 1)
                        {
                            cover.companyname = buyingcompany[0].name;
                            cover.companytitle = buyingcompany[0].title;
                            cover.companyid = buyingcompany[0].id;
                            cover.internalbuyer = buyingcompany[0].internalbuyer;
                        }
                    }
                    if (result[0].Lrinvoiceitems != null)
                    {
                        var snolist = result[0].Lrinvoiceitems.GroupBy(e => e.sno).ToList();
                        List<decimal> parents = new List<decimal>();
                        foreach (var kv in snolist)
                        {
                            if (kv.Count() > 1)
                                parents.Add(kv.First().sno);
                        }
                        result[0].multipacksno = parents;
                        result[0].Lrinvoiceitems = result[0].Lrinvoiceitems.OrderBy(e => e.sno).ThenBy(x => x.id).ToList();
                    }
                }
                return this.Ok(cover);
            }
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<LrinvoiceCreteria> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<LrinvoiceCover> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();

            if (request.Item.Items != null)
                request.Item.Items.ForEach(e => e.ChangeStatus(e.id > 0 ? BaseEntity.DataStatus.Update : BaseEntity.DataStatus.Create));

            foreach (var item in request.Item.Items)
            {
                item.ChangeStatus(item.id > 0 ? BaseEntity.DataStatus.Update : BaseEntity.DataStatus.Create);
                if (item.Lrinvoicetaxitems != null)
                    item.Lrinvoicetaxitems.ForEach(e => e.ChangeStatus(e.id > 0 ? BaseEntity.DataStatus.Update : BaseEntity.DataStatus.Create));
            }

            if (request.Item.Items.Exists(e => e.id > 0))
            {
                if (!LrinvoiceData.Context.HasPermission(PermissionSet.lrinvoice, InfyPOS.Common.Action.Modify))
                    throw new Exception("User doesn't have permission.");
            }
            else
            {
                if (!LrinvoiceData.Context.HasPermission(PermissionSet.lrinvoice, InfyPOS.Common.Action.Create))
                    throw new Exception("User doesn't have permission.");
            }

            LrinvoiceCover result = await Task<LrinvoiceCover>.Run(() => LrinvoiceData.Update(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("SupplierInvoiceBarcodes")]
        public async Task<IHttpActionResult> SupplierInvoiceBarcodes(ActionRequest<LrinvoiceCreteria> request)
        {
            try
            {
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                List<LrinvoiceItemBarcode> result = await Task<List<LrinvoiceItemBarcode>>.Run(() => LrinvoiceData.SupplierInvoiceBarcodes(request.Item));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.InternalServerError();
            }
        }


        [HttpPost]
        [Route("SearchBarcode")]
        public async Task<IHttpActionResult> SearchBarcode(ActionMulitipleResponse<LRInvoiceItemBarcode> request)
        {
            try
            {
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                List<LRInvoiceItemBarcode> result = await Task<List<LRInvoiceItemBarcode>>.Run(() => CreateBarcodeList(request.Items));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.InternalServerError();
            }
        }

        [HttpPost]
        [Route("ChangeSize")]
        public async Task<IHttpActionResult> ChangeSize(ActionSingleResponse<LRInvoiceItemSizeChange> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceData.ChangeSize(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SwapBarcode")]
        public async Task<IHttpActionResult> SwapBarcode(ActionSingleResponse<SwapBarcode> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.SwapBarcode(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateQty")]
        public async Task<IHttpActionResult> UpdateQty(ActionSingleResponse<LRInvoiceItemSizeChange> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceData.UpdateQty(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Change2Bit")]
        public async Task<IHttpActionResult> Change2Bit(ActionSingleResponse<LRInvoiceItemSizeChange> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            List<LrinvoiceItemDetail> result = await Task<List<LrinvoiceItemDetail>>.Run(() => LrinvoiceData.Change2Bit(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ConvertItemList")]
        public async Task<IHttpActionResult> ConvertItemList(ActionMulitpleRequest<SplitBarcode> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            request.Items = await Task<BarcodeReport>.Run(() => LrinvoiceData.ConvertItem(request.Items));
            ActionMulitipleResponse<SplitBarcode> response = new ActionMulitipleResponse<SplitBarcode>(request.Items);
            response.Success = true;
            return this.Ok(response);
        }

        [HttpPost]
        [Route("ConvertItem")]
        public async Task<IHttpActionResult> ConvertItem(ActionSingleResponse<LRInvoiceItemBitChange> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            List<Stock> result = await Task<List<Stock>>.Run(() => LrinvoiceData.ConvertItem(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("BulkSplit")]
        public async Task<IHttpActionResult> BulkSplit(ActionSingleResponse<Stock2BulkSplit> request)
        {
            LrinvoiceitemsDataService LrinvoiceData = new LrinvoiceitemsDataService();
            List<Stock2BulkSplit.Item> result = await Task<List<Stock2BulkSplit.Item>>.Run(() => LrinvoiceData.ConvertItem(request.Item));
            return this.Ok(result);
        }

        private List<LRInvoiceItemBarcode> CreateBarcodeList(List<LRInvoiceItemBarcode> items)
        {
            List<long> productids = new List<long>();
            ProductsDataService productsDataService = new ProductsDataService();
            List<LRInvoiceItemBarcode> retlist = new List<LRInvoiceItemBarcode>();
            StockDataService stockDataService = new StockDataService();
            foreach (var item in items)
            {
                if (item.code == "null" || item.code == "NOT GENERATED")
                    continue;

                if (!productids.Contains(item.productid))
                    productids.Add(item.productid);

                List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(item.code);

                var products = productsDataService.Search(new ProductsCreteria()
                {
                    id = item.productid
                });

                var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(productsDataService.Context.OrganizationID).Get(products[0].barcodemode, item.companyid);

                List<string> barcodelist = new List<string>();
                if (!string.IsNullOrEmpty(item.uan))
                {
                    barcodelist.Add(item.uan);

                    retlist.Add(new LRInvoiceItemBarcode()
                    {
                        code = item.uan,
                        name = item.name,
                        id = item.id,
                        productid = item.productid,
                        qty = item.qty,
                        sno = item.sno,
                        uan = item.uan
                    });

                }
                else
                {
                    barcodelist.AddRange(stockDataService.GetAllBarcodes(item.id));
                    foreach (var bitem in barcodelist)
                    {
                        retlist.Add(new LRInvoiceItemBarcode()
                        {
                            code = bitem,
                            name = item.name,
                            id = item.id,
                            productid = item.productid,
                            qty = item.qty,
                            sno = item.sno,
                            no = barcodeConfig.GetValue(bitem)
                        });
                    }
                }



                //var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance.Get(products[0].barcodemode, item.companyid);
                //foreach (var bitem in jsonbarcode)
                //{
                //    var codes = bitem.Split('-');
                //    if (codes.Length == 1)
                //    {


                //        retlist.Add(new LRInvoiceItemBarcode()
                //        {
                //            code = codes[0],
                //            name = item.name,
                //            id = item.id,
                //            productid = item.productid,
                //            qty = item.qty,
                //            sno = item.sno,
                //            no = barcodeConfig.GetValue(codes[0])
                //        });

                //    }
                //    else
                //    {
                //        var from = barcodeConfig.GetValue(codes[0]);
                //        var to = barcodeConfig.GetValue(codes[1]);
                //        for (int id = from; id <= to; id++)
                //        {
                //            retlist.Add(new LRInvoiceItemBarcode()
                //            {
                //                code = barcodeConfig.GetValue(codes[0], id),
                //                name = item.name,
                //                id = item.id,
                //                productid = item.productid,
                //                qty = item.qty,
                //                sno = item.sno,
                //                no = id
                //            });
                //        }
                //    }
                //}
            }

            if (retlist.Count > 0)
            {
                var blist = stockDataService.Search(new StockCreteria()
                {
                    barcodes = retlist.ConvertAll<string>(e => "'" + WebAPI.Data.Utility.SafeSQL(e.code) + "'").ToList()
                });
                foreach (var item in retlist)
                {
                    if (blist.Exists(e => e.barcode == item.code))
                    {
                        var bitem = blist.Find(e => e.barcode == item.code);
                        item.price = bitem.price;
                        //item.qty = (InfyPOS.Services.ConfigManager.Instance(productsDataService.Context.OrganizationID).Config.system_hyper_uan)? 1: bitem.qty;
                        item.qty = bitem.qty - bitem.returnqty;
                        item.imageid = bitem.imageid;
                    }
                }

                if (items.Exists(x => !string.IsNullOrEmpty(x.uan)))
                {
                    items.ForEach(x =>
                    {
                        if (retlist.Exists(y => y.uan == x.uan))
                        {
                            retlist.Find(y => y.uan == x.uan).qty = x.qty;
                        }
                    });
                }

                if (productids.Count > 0)
                {
                    var list = productsDataService.GetSellingModes(productids);
                    foreach (var item in list)
                        retlist.Where(e => e.productid == item.Key).ToList().ForEach(e => e.sellingmode = item.Value);
                }
            }
            return retlist;
        }

        [HttpPost]
        [Route("SavePriceItems")]
        public async Task<IHttpActionResult> SavePriceItems(ActionRequest<LrinvoiceDetail> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService() { ispricing = true };
            Lrinvoice result = null;
            if (request.Item.isdirectpurchase || request.Item.isdeliverychallan)
            {
                if (request.Item.id <= 0)
                {
                    result = LrinvoiceData.InsertDirectPurchase(request.Item);
                }
                else
                {
                    result = LrinvoiceData.UpdateDirectPurchase(request.Item);
                }
            }
            else
            {
                result = await Task<Lrinvoice>.Run(() => LrinvoiceData.UpdateItems(request.Item));
            }
            var snolist = result.Lrinvoiceitems.GroupBy(e => e.sno).ToList();
            List<decimal> parents = new List<decimal>();
            foreach (var kv in snolist)
            {
                if (kv.Count() > 1)
                    parents.Add(kv.First().sno);
            }
            result.multipacksno = parents;

            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateAutoDebitNotes")]
        public async Task<IHttpActionResult> CreateAutoDebitNotes(ActionRequest<LrinvoiceDetail> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            Lrinvoice result = null;
            result = await Task<Lrinvoice>.Run(() => LrinvoiceData.CreateAutoDebitNote(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveInvoiceItems")]
        public async Task<IHttpActionResult> SaveInvoiceItems(ActionRequest<LrinvoiceDetail> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            Lrinvoice result = null;
            if (request.Item.isdirectpurchase || request.Item.isdeliverychallan)
            {
                if (request.Item.id <= 0)
                {
                    if (!LrinvoiceData.Context.HasPermission(PermissionSet.lrproduct, InfyPOS.Common.Action.Create))
                        throw new Exception("User doesn't have permission.");
                    result = LrinvoiceData.InsertDirectPurchase(request.Item);
                }
                else
                {
                    if (!LrinvoiceData.Context.HasPermission(PermissionSet.lrproduct, InfyPOS.Common.Action.Modify))
                        throw new Exception("User doesn't have permission.");
                    result = LrinvoiceData.UpdateDirectPurchase(request.Item);
                }
            }
            else
            {
                if (!LrinvoiceData.Context.HasPermission(PermissionSet.lrproduct, InfyPOS.Common.Action.Modify))
                {
                    if (request.Item.Lrinvoiceitems.Exists(e => e.id > 0))
                        throw new Exception("User doesn't have permission.");
                }

                if (!LrinvoiceData.Context.HasPermission(PermissionSet.lrproduct, InfyPOS.Common.Action.Create))
                    throw new Exception("User doesn't have permission.");

                result = await Task<Lrinvoice>.Run(() => LrinvoiceData.UpdateItems(request.Item));
            }
            var snolist = result.Lrinvoiceitems.GroupBy(e => e.sno).ToList();
            List<decimal> parents = new List<decimal>();
            foreach (var kv in snolist)
            {
                if (kv.Count() > 1)
                    parents.Add(kv.First().sno);
            }
            result.multipacksno = parents;

            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.lrinvoice, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<LrinvoiceDetail> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            Lrinvoice result = await Task<Lrinvoice>.Run(() => LrinvoiceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.lrinvoice, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<LrinvoiceDetail> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            Lrinvoice result = await Task<Lrinvoice>.Run(() => LrinvoiceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteAll")]
        public async Task<IHttpActionResult> DeleteAll(ActionRequest<Lrinvoice> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.DeleteAll(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ChangeSettled")]
        public async Task<IHttpActionResult> ChangeSettled(ActionRequest<InvoiceSetted> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.ChangeSettled(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Lrinvoice> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.DeleteInvoice(request.Item));
            return this.Ok(result);
        }
        [HttpGet]
        [Route("LastEntry/{companyid}")]
        public async Task<IHttpActionResult> LastEntry(string companyid)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            string result = await Task<string>.Run(() => LrinvoiceData.LastEntry(companyid));
            return this.Ok(result);
        }
        [HttpGet]
        [Route("GetQCApproveval/{id}")]
        public async Task<IHttpActionResult> GetQCApproveval(string id)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            LrInvoiceQC result = await Task<LrInvoiceQC>.Run(() => LrinvoiceData.GetQCApproveval(id));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SetQCApproveval")]
        public async Task<IHttpActionResult> SetQCApproveval(ActionRequest<LrInvoiceQC> request)
        {
            LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
            bool result = await Task<bool>.Run(() => LrinvoiceData.SetQCApproveval(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintJSON/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintJSON(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                LrinvoiceData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                LrinvoicePrint result = LrinvoiceData.Print(new LrinvoiceCreteria() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintVendorInvoice(LrinvoiceData.Context.OrganizationID, result, true);
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("PrintInvoicePDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintInvoicePDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                LrinvoiceData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                LrinvoicePrint result = LrinvoiceData.Print(new LrinvoiceCreteria() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintVendorInvoice(LrinvoiceData.Context.OrganizationID, result, false);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintGRNPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintGRNPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                LrinvoiceData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                LrinvoicePrint result = LrinvoiceData.Print(new LrinvoiceCreteria() { id = long.Parse(id),IsGRN =true });
                
                FileContentInfo byteArray = PrintVendorInvoice(LrinvoiceData.Context.OrganizationID, result, true);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                LrinvoiceData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                LrinvoicePrint result = LrinvoiceData.Print(new LrinvoiceCreteria() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintVendorInvoice(LrinvoiceData.Context.OrganizationID, result, true);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintDCPNYPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintDCPNYPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                LrinvoiceDataService LrinvoiceData = new LrinvoiceDataService();
                LrinvoiceData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                LrinvoicePrint result = LrinvoiceData.Print(new LrinvoiceCreteria() { id = long.Parse(id), includedebitnote = true });
                FileContentInfo byteArray = PrintDiscrepancy(LrinvoiceData.Context.OrganizationID, result, true);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        private FileContentInfo PrintDiscrepancy(long organizationid, LrinvoicePrint result, bool isInvoiceDetails)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "PURCHASEINVOICEDCPNY"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(config.Context.OrganizationID, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            int orderid = 1;
            result.descripencyItems = result.Lrinvoiceitems.ConvertAll(x => new LrinvoicePrint.DescripencyItem()
            {
                uid = x.uan,
                orderid = orderid++,
                actualqty = x.actualqty,
                billqty = x.buyingqty,
                cost = x.buyingprice,
                product = x.productname,
                design = x.designid,
                tax = x.buyingqty  >0 ? x.purchasetax / x.buyingqty:0,
                designid = x.designid,
                detail = x.Details,
                colourname = x.Details.colourname,
                sizename = x.Details.sizename,
                poqty = x.poinfo == null ? 0 : (x.poinfo.Sum(y => y.oqty))
            });

            result.descripencyqty = result.descripencyItems.Sum(x => x.descripencyqty);
            result.descripencyvalue = result.descripencyItems.Sum(x => x.descripencyvalue);
            result.actualvalue = result.Lrinvoiceitems.Sum(x => x.actualqty * x.buyingprice);

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.descripencyItems;

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
                fci.FileName = "vendorinvoice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
        private FileContentInfo PrintVendorInvoice(long organizationid, LrinvoicePrint result,
            bool isInvoiceDetails)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = (isInvoiceDetails ? "PURCHASEINVOICEDETAILS" : "PURCHASEINVOICE")
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(config.Context.OrganizationID, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.Lrinvoiceitems;

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
            
            if(lstconfig[0].reportformat.format.Contains("racknumber"))
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                var racknumbers = mfgdesignDataService.SearchRackNumber(result.Lrinvoiceitems.ConvertAll(x => x.uan));
                result.Lrinvoiceitems.ForEach(e =>
                {
                    if (racknumbers.ContainsKey(e.uan))
                        e.racknumber = racknumbers[e.uan];
                });
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "vendorinvoice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}