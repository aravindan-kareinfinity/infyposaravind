
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
using WebAPI.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("BundlesService")]
    public class BundlesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Bundles>();
            result.Item = new Bundles();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Merge")]
        public async Task<IHttpActionResult> Merge(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesData.Merge(request.Item.ids));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            List<BundleDetail> result = await Task<List<BundleDetail>>.Run(() => BundlesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("FindPrice")]
        public async Task<IHttpActionResult> FindPrice(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            List<BundleItemPrife> result = await Task<List<BundleItemPrife>>.Run(() => BundlesData.FindPrice(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchGoods")]
        public async Task<IHttpActionResult> SearchGoods(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            List<BundleDetail> result = await Task<List<BundleDetail>>.Run(() => BundlesData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("UpdateUANStock")]
        public async Task<IHttpActionResult> UpdateUANStock(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            BundlesCreteria result = await Task<BundlesCreteria>.Run(() => BundlesData.UpdateUANStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveUANSTO")]
        public async Task<IHttpActionResult> SaveUANSTO(ActionRequest<STOUANUploaded> request)
        {
            BundlesDataService BundlesDataService = new BundlesDataService(false);
            List<StockOrderHistory> result = await Task<List<StockOrderHistory>>.Run(() => BundlesDataService.SaveUANSTO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CreateBill")]
        public async Task<IHttpActionResult> CreateBill(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesDataService = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesDataService.CreateBill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdatePackingSlip")]
        public async Task<IHttpActionResult> UpdatePackingSlip(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesDataService = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesDataService.UpdatePackingSlip(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("InwardUANSTOBill")]
        public async Task<IHttpActionResult> InwardUANSTOBill(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesDataService = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesDataService.InwardUANSTOBill(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchDesignStock")]
        public async Task<IHttpActionResult> SearchDesignStock(ActionRequest<BundleStockCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            List<Bundleitem> result = await Task<List<Bundleitem>>.Run(() => BundlesData.SearchDesignStock(request.Item));
            //ProductsDataService productdata = new ProductsDataService();
            //productdata.Context = BundlesData.Context;
            //var lsitofmeters = productdata.Search(new ProductsCreteria()
            //{
            //    sellingmode = 3
            //});
            //result.Where(e => lsitofmeters.Exists(x => x.id == e.productid)).ToList().ForEach(e => e.ismeter = true);
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchGoodsStock")]
        public async Task<IHttpActionResult> SearchGoodsStock(ActionRequest<BundleStockCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            List<Bundleitem> result = await Task<List<Bundleitem>>.Run(() => BundlesData.SearchGoodsStock(request.Item));
            //ProductsDataService productdata = new ProductsDataService();
            //productdata.Context = BundlesData.Context;
            //var lsitofmeters = productdata.Search(new ProductsCreteria()
            //{
            //    sellingmode = 3
            //});
            //result.Where(e => lsitofmeters.Exists(x => x.id == e.productid)).ToList().ForEach(e => e.ismeter = true);
            return this.Ok(result);
        }

        [HttpPost]
        [Route("InwardGoods")]
        public async Task<IHttpActionResult> InwardGoods(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            if (request.Item.id > 0)
            {
                if (!request.Item.inward2warehouse && (request.Item.fromlocationid <= 0 || request.Item.tolocationid <= 0))
                    return this.NotFound();

                request.Item.Salablegoods = request.Item.items.ConvertAll(e => new Salablegoods()
                {
                    id = e.id,
                    iqty = e.iqty,
                    aqty = e.aqty,
                    barcode = e.barcode,
                    supplierid = e.supplierid,
                    stockid = e.stockid,
                    dqty = e.damagedqty,
                    itemid = e.itemid,
                    lrreference = e.lrreference,
                    suppliername = e.suppliername,
                    locationid = request.Item.tolocationid
                });

                Bundles result = await Task<Bundles>.Run(() => BundlesData.InwardGoods(request.Item));
                return this.Ok(result);

            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("RejectGoods")]
        public async Task<IHttpActionResult> RejectGoods(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            if (request.Item.id > 0)
            {
                bool result = await Task<bool>.Run(() => BundlesData.RejectGoods(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("OutwardGoods")]
        public async Task<IHttpActionResult> OutwardGoods(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(true);
            if (request.Item.id <= 0)
            {
                if (request.Item.items.Any(e => e.qty < e.iqty))
                {
                    request.Item.errormessage = request.Item.items.First(e => e.qty < e.iqty).barcode + " don't have enough stock to transfer";
                    return this.Ok(request.Item);
                }
                Bundles result = await Task<Bundles>.Run(() => BundlesData.Insert(request.Item));

                if (result.id > 0 && request.Item.autotransfer)
                {
                    List<BundleDetail> sresult = await Task<List<BundleDetail>>.Run(() => BundlesData.Search(new BundlesCreteria()
                    {
                        id = result.id
                    }));


                    if (sresult.Count == 1)
                    {
                        sresult[0].items.ForEach(e => e.aqty = e.iqty);

                        sresult[0].Salablegoods = sresult[0].items.ConvertAll(e => new Salablegoods()
                        {
                            id = e.id,
                            iqty = e.iqty,
                            aqty = e.iqty,
                            barcode = e.barcode,
                            supplierid = e.supplierid,
                            stockid = e.stockid,
                            dqty = e.damagedqty,
                            itemid = e.itemid,
                            lrreference = e.lrreference,
                            suppliername = e.suppliername,
                            locationid = request.Item.tolocationid
                        });

                        Bundles igresult = await Task<Bundles>.Run(() => BundlesData.InwardGoods(new BundlesCreteria()
                        {
                            Salablegoods = sresult[0].Salablegoods,
                            items = sresult[0].items,
                            id = sresult[0].id,
                            inward2warehouse = request.Item.inward2warehouse,
                            fromlocationid = sresult[0].fromlocationid,
                            tolocationid = sresult[0].tolocationid,
                            code = sresult[0].code,
                            qty = sresult[0].qty,
                            version = sresult[0].version,
                            packedby = sresult[0].packedby,
                            packedon = sresult[0].packedon,
                            receivedby = sresult[0].packedby,
                            receivedon = sresult[0].packedon
                        }));
                    }
                }
                return this.Ok(result);
            }
            else
            {
                Bundles result = await Task<Bundles>.Run(() => BundlesData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("SearchStock")]
        public async Task<IHttpActionResult> SearchStock(ActionRequest<BundleStockCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            List<Bundleitem> result = await Task<List<Bundleitem>>.Run(() => BundlesData.SearchStock(request.Item));

            if (result != null && result.Count > 0)
            {
                ProductsDataService productdata = new ProductsDataService();
                productdata.Context = BundlesData.Context;
                var lsitofmeters = productdata.Search(new ProductsCreteria()
                {
                    sellingmode = 3
                });
                result.Where(e => lsitofmeters.Exists(x => x.id == e.productid)).ToList().ForEach(e => e.ismeter = true);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Bundles result = await Task<Bundles>.Run(() => BundlesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Bundles result = await Task<Bundles>.Run(() => BundlesData.Update(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Inward")]
        public async Task<IHttpActionResult> Inward(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            if (request.Item.id > 0)
            {
                Bundles result = await Task<Bundles>.Run(() => BundlesData.InwardFromWarehouse(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("Reject")]
        public async Task<IHttpActionResult> Reject(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            if (request.Item.id > 0)
            {
                bool result = await Task<bool>.Run(() => BundlesData.Reject(request.Item));
                return this.Ok(result);
            }
            return this.NotFound();
        }

        [HttpPost]
        [Route("Outward")]
        public async Task<IHttpActionResult> Outward(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            if (request.Item.id <= 0)
            {
                Bundles result = await Task<Bundles>.Run(() => BundlesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Bundles result = await Task<Bundles>.Run(() => BundlesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<Bundles> request)
        {
            BundlesDataService PurchaseorderData = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => PurchaseorderData.UpdateStatus(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Transport")]
        public async Task<IHttpActionResult> SetTransport(ActionRequest<BundleTransport> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            var result = await Task<BundleTransport>.Run(() => BundlesData.SetTransport(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Transport/{id}")]
        public async Task<IHttpActionResult> GetTransport(string id)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);

            var result = await Task<BundleTransport>.Run(() => BundlesData.GetTransport(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            bool result = await Task<bool>.Run(() => BundlesData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<Bundles> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            Bundles result = await Task<Bundles>.Run(() => BundlesData.Print(request.Item.id, null));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("PrintBundle")]
        public async Task<IHttpActionResult> PrintBundle(ActionRequest<BundlesCreteria> request)
        {
            BundlesDataService BundlesData = new BundlesDataService(false);
            BundlesPrint result = BundlesData.Print(request.Item.id, request.Item.packingcode);
            if (result != null)
            {
                FileContentInfo byteArray = PrintReceipt(BundlesData.Context.OrganizationID, result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            throw new Exception("Can't find the bundle to print");
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintUnPackedPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintUnPackedPDF(string organizationid, string id)
        {
            try
            {
                BundlesDataService BundlesData = new BundlesDataService(false);
                BundlesData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                BundlesPrint result = BundlesData.Print(long.Parse(id), null);
                if (result != null)
                {
                    if (result.status > 0) throw new Exception("Only requested state bundles considered as Unpacked");

                    if (result.packslip != null)
                    {
                        List<Bundles.PackSlip.PackslipItem> items = new List<Bundles.PackSlip.PackslipItem>();
                        result.packslip.ForEach(x => items.AddRange(x.items));
                        var packedqty = items.GroupBy(e => e.barcode).ToDictionary(x => x.Key, x => x.Sum(y => y.qty));
                        result.groupitems.ForEach(x =>
                        {
                            if (packedqty.ContainsKey(x.gln))
                            {
                                x.packedqty = x.qty;
                                packedqty[x.gln] -= x.packedqty;
                            }
                        });
                        result.groupitems.RemoveAll(x=>x.packedqty == x.qty);
                        result.groupitems.ForEach(x=>x.qty-=x.packedqty);
                        result.items = result.groupitems.ConvertAll(x => new Bundleitem()
                        {
                            serialno = x.gln,
                            qty = x.qty,
                            design = x.designid,
                            hsn = x.hsn,
                            sizename = x.sizename,
                            colourname = x.colourname,
                            buyingprice = x.costprice,
                            taxpercentage = x.taxpercentage,
                            mrp = x.mrp,
                            price = x.price,
                        });
                    }
                    FileContentInfo byteArray = PrintReceipt(BundlesData.Context.OrganizationID, result);
                    return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
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
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                BundlesDataService BundlesData = new BundlesDataService(false);
                BundlesData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                BundlesPrint result = BundlesData.Print(long.Parse(id), null);
                if (result != null)
                {
                    FileContentInfo byteArray = PrintReceipt(BundlesData.Context.OrganizationID, result);
                    return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
                }
                return null;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        private FileContentInfo PrintReceipt(long organizationid, BundlesPrint result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "STOCKTRANSFER"
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
            labreport.DataItems = result.groupitems;

            if (lstconfig[0].reportformat.format != null && lstconfig[0].reportformat.format.Contains("barcode"))
            {
                result.itemsum = true;
                //labreport.DataItems = result.items;
                labreport.DataItems = result.lstoutwarditems;
                if (result.lstoutwarditems.Exists(x => x.taxpercent == 0))
                {
                    var missingtaxitems = result.lstoutwarditems.FindAll(x => x.taxpercent == 0);
                    missingtaxitems.ForEach(x =>
                    {
                        if (x.taxamount > 0 && x.buyingprice > 0)
                            x.taxpercent = Math.Round(x.taxamount / x.buyingprice * 100, 2);
                    });
                }
            }
            else if (lstconfig[0].reportformat.format != null && lstconfig[0].reportformat.format.Contains("groupedproductname"))
            {
                var groupedItems = result.groupitems.GroupBy(e => new
                {
                    e.description
                });
                List<BundlesPrint.ShortPrintableitem> newitems = new List<BundlesPrint.ShortPrintableitem>();
                foreach (var gitem in groupedItems)
                {
                    var newitem = Newtonsoft.Json.JsonConvert.DeserializeObject<BundlesPrint.ShortPrintableitem>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem.First()));
                    newitem.groupedproductname = gitem.Key.description;
                    newitem.qty = gitem.Sum(e => e.qty);
                    newitem.sno = newitems.Count + 1;
                    newitems.Add(newitem);
                }
                labreport.DataItems = newitems;
            }

            if (labreport.format.PageBody.Items[0].IsTable &&
                labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                var sizeList = result.items.GroupBy(e => new { e.sizename }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => String.IsNullOrEmpty(e.Key.sizename) ? "" : e.Key.sizename.ToUpper()));
                if (Services.ConfigManager.Instance(organizationid).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    sizeExistOrder = result.items.GroupBy(e => string.IsNullOrEmpty(e.sizename) ? "" : e.sizename.ToUpper()).ToList().ConvertAll(e => e.Key);
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
                        if (!Services.ConfigManager.Instance(organizationid).Config.system_ismfgapplication)
                        {

                            result.items = new List<Bundleitem>();
                            List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                            int index = 0;
                            var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
                            if (rootcolumn.Text.IndexOf(',') > 0)
                            {
                                sizeExistOrder = new List<string>(rootcolumn.Text.Split(','));
                            }
                            sizeExistOrder.Add("Total");
                            var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;
                            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                            var colwidth = ((labreport.DefaultPageSettings.MarginBound.Width - totalwidth) / (sizeExistOrder.Count));
                            foreach (var sitem in sizeExistOrder)
                            {
                                ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                                content.Type = ReportLibrary.DosFormat.CotnentType.Field;
                                content.Text = sitem;
                                content.Length = colwidth;
                                if (sitem == "Total")
                                    content.Length = rootcolumn.Length;
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
                            foreach (var group in result.lstoutwarditems.GroupBy(x => new { x.designid, x.colourname, x.productname, x.mrp, x.price }))
                            {
                                var item = new Bundleitem();
                                item.design = group.Key.designid;
                                item.product = group.Key.productname;
                                item.colourname = group.Key.colourname;
                                item.hsn = group.First().hsncode;
                                item.brand = group.First().brandname;
                                item.buyingprice = group.First().itemibuyingprice;
                                item.mrp = group.First().itemmrp;
                                item.price = group.First().itemprice;
                                item.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                                foreach (var sitem in sizeExistOrder)
                                {
                                    decimal qty = 0;
                                    if (sitem == "Total")
                                    {
                                        qty = group.Sum(e => e.qty);
                                    }
                                    else if (group.Any(e => e.sizename == sitem))
                                    {
                                        qty = group.Sum(e => e.sizename == sitem ? e.qty : 0);
                                    }
                                    item.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                                    {
                                        name = sitem,
                                        oqty = qty
                                    });
                                }
                                item.qty = item.jumpqty.Find(x => x.name == "Total").oqty;
                                item.sno = result.items.Count + 1;
                                result.items.Add(item);
                            }
                            result.items = result.items.OrderBy(x => x.designid).ToList();
                            int sno = 1;
                            result.items.ForEach(x => x.sno = sno++);
                            labreport.DataItems = result.items;
                        }
                        else
                        {
                            List<Mfgdesign> alldesigns = new List<Mfgdesign>();
                            var wrongdesigns = result.items.FindAll(x => string.IsNullOrEmpty(x.designid) || x.designid.IndexOf('/') < 0);
                            var wronguans = wrongdesigns.ConvertAll(x => x.serialno).ToList();
                            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                            if (wronguans.Count > 0)
                            {
                                var missingdesigns = mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
                                {
                                    uids = wronguans
                                });
                                wrongdesigns.ForEach(x =>
                                {
                                    if (missingdesigns.Exists(y => y.uid == x.serialno))
                                    {
                                        x.design = missingdesigns.Find(y => y.uid == x.serialno).code;
                                        x.designid = missingdesigns.Find(y => y.uid == x.serialno).name;
                                    }
                                });
                            }

                            var groupedList = result.items.GroupBy(e => new
                            {
                                design = (e.designid == null || e.designid.IndexOf('/') < 0) ? e.designid : e.designid.Substring(0, e.designid.IndexOf('/')),
                                colourcode = (e.designid == null || e.designid.IndexOf('/') < 0) ? e.designid : e.designid.Substring(e.designid.IndexOf('/') + 1),
                                e.price
                            }).ToList();

                            List<ProductsBase> allproducts = null;
                            List<Brand> allbrands = null;
                            List<ReferenceItem> sizelist = null;
                            if (lstconfig[0].reportformat.format.Contains("rackno") ||
                                lstconfig[0].reportformat.format.Contains("brand") ||
                                lstconfig[0].reportformat.format.Contains("product"))
                            {

                                alldesigns.AddRange(mfgdesignDataService.Search(new MfgdesignCreteria()
                                {
                                    codes = groupedList.ConvertAll(x => x.Key.design).ToList(),
                                    includedetail = true
                                }));

                                ProductsDataService productsDataService = new ProductsDataService();
                                allproducts = productsDataService.SearchList(new ProductsCreteria()
                                {
                                    ids = alldesigns.ConvertAll(x => x.productid).ToList().Distinct().ToList()
                                });

                                BrandDataService brandDataService = new BrandDataService();
                                allbrands = brandDataService.Search(new BrandCreteria()
                                {
                                    ids = alldesigns.ConvertAll(x => x.brandid).ToList().Distinct().ToList()
                                });
                                ReferencelistDataService referencelistData = new ReferencelistDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                                sizelist = referencelistData.SearchList(new ReferencelistCreteria() { type = "SIZE" });
                            }
                            result.items = new List<Bundleitem>();
                            List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                            int index = 0;
                            var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
                            if (rootcolumn.Text.IndexOf(',') > 0)
                            {
                                sizeExistOrder = new List<string>(rootcolumn.Text.Split(','));
                            }
                            sizeExistOrder.Add("Total");
                            var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;

                            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                            var colwidth = ((labreport.DefaultPageSettings.MarginBound.Width - totalwidth) / (sizeExistOrder.Count));
                            foreach (var sitem in sizeExistOrder)
                            {
                                ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                                content.Type = ReportLibrary.DosFormat.CotnentType.Field;
                                content.Text = sitem;
                                content.Length = colwidth;
                                if (sitem == "Total")
                                    content.Length = rootcolumn.Length;
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


                            foreach (var group in groupedList)
                            {
                                var item = group.First();
                                if (alldesigns != null && alldesigns.Exists(x => x.code == group.Key.design))
                                {
                                    var design = alldesigns.Find(x => x.code == group.Key.design);
                                    item.rackno = design.racknumber;

                                    if (allproducts != null && allproducts.Exists(x => x.id == design.productid))
                                        item.product = allproducts.Find(x => x.id == design.productid).name;

                                    if (allbrands != null && allbrands.Exists(x => x.id == design.brandid))
                                        item.brand = allbrands.Find(x => x.id == design.brandid).name;

                                    foreach (var bundleitem in group)
                                    {
                                        var bundlebarcode = design.sizegroup.items.Find(x => x.uid == bundleitem.barcode);
                                        if (bundlebarcode == null)
                                            bundlebarcode = design.sizegroup.items.Find(x => x.uid == bundleitem.serialno);

                                        if (bundlebarcode != null && sizelist.Exists(X => X.id == bundlebarcode.sizeid))
                                            bundleitem.sizename = sizelist.Find(X => X.id == bundlebarcode.sizeid).name;
                                    }
                                }


                                item.design = group.Key.design;
                                item.colourcode = group.Key.colourcode;
                                item.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                                foreach (var sitem in sizeExistOrder)
                                {
                                    decimal qty = 0;
                                    if (sitem == "Total")
                                    {
                                        qty = group.Sum(e => e.iqty);
                                    }
                                    else if (group.Any(e => e.sizename == sitem))
                                    {
                                        qty = group.Sum(e => e.sizename == sitem ? e.iqty : 0);
                                    }
                                    item.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                                    {
                                        name = sitem,
                                        oqty = qty
                                    });
                                }
                                item.qty = item.jumpqty.Find(x => x.name == "Total").oqty;
                                item.sno = result.items.Count + 1;
                                result.items.Add(item);
                            }
                            result.items = result.items.OrderBy(x => x.designid).ToList();
                            int sno = 1;
                            result.items.ForEach(x => x.sno = sno++);
                            labreport.DataItems = result.items;
                        }
                    }
                }
            }
            else if (Services.ConfigManager.Instance(organizationid).Config.system_ismfgapplication)
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                var alldesigns = mfgdesignDataService.SearchDesignList(result.items.ConvertAll(x => x.serialno));
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                int sno = 1;
                TaxDataService taxDataService = new TaxDataService();
                var taxes = taxDataService.TaxProducts(result.items.ConvertAll(x => x.productid).Distinct().ToList());
                result.items.ForEach(x =>
                {
                    if (taxes.ContainsKey(x.productid))
                    {
                        var taxinfo = taxes[x.productid].taxsplit.Find(x.price);
                        x.taxpercentage = taxinfo.cgst + taxinfo.sgst;
                        x.tax = Math.Round((x.price * x.qty) * x.taxpercentage / 100, 2);
                    }
                    x.sno = sno++;
                    if (alldesigns.Exists(y => y.uid == x.serialno))
                    {
                        x.mfgdesign = alldesigns.Find(y => y.uid == x.serialno);
                        x.design = x.mfgdesign.code;
                        x.designid = x.mfgdesign.code;
                    }
                });
                if (!string.IsNullOrEmpty(result.fromlocation.gst) &&
                   !string.IsNullOrEmpty(result.tolocation.gst))
                {
                    result.isinterstatesale = result.fromlocation.gst.Substring(0, 2) != result.tolocation.gst.Substring(0, 2);
                }

                if (result.items.Exists(x => x.mfgdesign == null))
                {
                    ProductsDataService productsDataService = new ProductsDataService();
                    var missingdesign = result.items.FindAll(x => x.mfgdesign == null);
                    var allproducts = productsDataService.Search(new ProductsCreteria()
                    {
                        ids = missingdesign.ConvertAll(x => x.productid)
                    });
                    missingdesign.ForEach(x =>
                    {
                        if (allproducts.Exists(y => y.id == x.productid))
                        {
                            x.mfgdesign = new MfgDesignItem()
                            {
                                hsn = allproducts.Find(y => y.id == x.productid).hsncode
                            };
                        }
                    });
                }
                result.HSNList = new List<BundlesPrint.HSNItem>();


                if (lstconfig[0].reportformat.format.Contains("lstoutwarditems"))
                {
                    labreport.DataItems = result.lstoutwarditems;
                    foreach (var grp in result.lstoutwarditems.GroupBy(e => new { e.hsncode, e.taxpercent }))
                    {
                        result.HSNList.Add(new BundlesPrint.HSNItem()
                        {
                            code = grp.Key.hsncode,
                            qty = grp.Sum(x => x.qty),
                            isinterstate = result.isinterstatesale,
                            taxable = grp.Sum(e => e.amount),
                            tax = grp.Sum(e => e.taxamount),
                            total = grp.Sum(e => e.taxamount + e.amount),
                            cgst_percentage = (result.isinterstatesale || grp.Key.taxpercent == 0) ? 0 : grp.Key.taxpercent / 2,
                            igst_percentage = (!result.isinterstatesale || grp.Key.taxpercent == 0) ? 0 : grp.Key.taxpercent,
                            sgst_percentage = (result.isinterstatesale || grp.Key.taxpercent == 0) ? 0 : grp.Key.taxpercent / 2,
                            sgst_value = result.isinterstatesale ? 0 : grp.Sum(e => e.taxamount / 2),
                            cgst_value = result.isinterstatesale ? 0 : grp.Sum(e => e.taxamount / 2),
                            igst_value = !result.isinterstatesale ? 0 : grp.Sum(e => e.taxamount)
                        });
                    }
                }
                else
                {
                    labreport.DataItems = result.items;
                    foreach (var grp in result.items.GroupBy(e => new { e.mfgdesign.hsn, e.taxpercentage }))
                    {
                        result.HSNList.Add(new BundlesPrint.HSNItem()
                        {
                            code = grp.Key.hsn,
                            qty = grp.Sum(x => x.qty),
                            isinterstate = result.isinterstatesale,
                            taxable = grp.Sum(e => e.totalprice),
                            tax = grp.Sum(e => e.tax),
                            total = grp.Sum(e => e.netprice),
                            cgst_percentage = (result.isinterstatesale || grp.Key.taxpercentage == 0) ? 0 : grp.Key.taxpercentage / 2,
                            igst_percentage = (!result.isinterstatesale || grp.Key.taxpercentage == 0) ? 0 : grp.Key.taxpercentage,
                            sgst_percentage = (result.isinterstatesale || grp.Key.taxpercentage == 0) ? 0 : grp.Key.taxpercentage / 2,
                            sgst_value = result.isinterstatesale ? 0 : grp.Sum(e => e.tax / 2),
                            cgst_value = result.isinterstatesale ? 0 : grp.Sum(e => e.tax / 2),
                            igst_value = !result.isinterstatesale ? 0 : grp.Sum(e => e.tax)
                        });
                    }
                }
            }

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
                fci.FileName = "stocktransfer.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadExcel/{organizationid}/{code}")]
        public async Task<IHttpActionResult> DownloadExcel(string organizationid, string code)
        {
            try
            {
                System.Data.DataTable result = null;
                BundlesDataService BundlesData = new BundlesDataService(false);
                BundlesData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                result = await Task<System.Data.DataTable>.Run(() => BundlesData.ExportBundleDetail(long.Parse(code)));
                using (System.IO.MemoryStream memoryStream = new MemoryStream())
                {
                    ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                    var sheet = xLWorkbook.AddWorksheet(result, "Quanto");
                    xLWorkbook.SaveAs(memoryStream);
                    return new FileActionResult(true, "QuantoBundle.xlsx", "application/vnd.ms-excel", new ByteArrayContent(memoryStream.ToArray()));
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "DownloadExcel/" + code);
                return this.Ok(exp);
            }
        }

    }
}