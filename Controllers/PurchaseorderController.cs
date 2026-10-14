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
using System.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("PurchaseorderService")]
    public class PurchaseorderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Purchaseorder>();
            result.Item = new Purchaseorder();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> Search(ActionRequest<PurchaseorderCreteria> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            List<Purchaseorder> result = await Task<List<Purchaseorder>>.Run(() => PurchaseorderData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchMyPendingPO")]
        public async Task<IHttpActionResult> SearchMyPendingPO(ActionRequest<PurchaseorderCreteria> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            List<PurchaseOrderHistory> result = await Task<List<PurchaseOrderHistory>>.Run(() => PurchaseorderData.SearchMyPendingPO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveUANPO")]
        public async Task<IHttpActionResult> SaveUANPO(ActionRequest<POUANUploaded> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            List<PurchaseOrderHistory> result = await Task<List<PurchaseOrderHistory>>.Run(() => PurchaseorderData.SaveUANPO(request.Item));
            return this.Ok(result);
        }

        
        [HttpGet]
        [Route("SearchMobilePO/{supplierid=null}/{version=null}/{limit=0}")]
        public async Task<IHttpActionResult> SearchMobilePO(string supplierid, string version, string limit)
        {
            long lnsupplierid = 0;
            long lnversion = 0;
            long lnlimit = 0;
            long.TryParse(supplierid, out lnsupplierid);
            long.TryParse(version, out lnversion);
            long.TryParse(limit, out lnlimit);
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            if (lnversion == 1)
            {
                List<PurchaseorderMobile> result = await Task<List<PurchaseOrderItemSimple>>.Run(() =>
                {
                    List<PurchaseorderMobile> rst = new List<PurchaseorderMobile>();
                    foreach (var item in PurchaseorderData.SearchMobilePO(lnsupplierid, 0, lnlimit).GroupBy(e => e.poid))
                    {
                        rst.Add(new PurchaseorderMobile(item.ToList()));
                    }
                    return rst;
                });
                return this.Ok(result);
            }
            else
            {
                List<PurchaseOrderItemSimple> result = await Task<List<PurchaseOrderItemSimple>>.Run(() => PurchaseorderData.SearchMobilePO(0, 0, 0));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("DeleteMobilePO")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> DeleteMobilePO(ActionRequest<PurchaseOrderItemSimple> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            string result = await Task<string>.Run(() => PurchaseorderData.DeleteMobilePO(request.Item.id));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeMobilePO")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> ChangeMobilePO(ActionRequest<PurchaseOrderItemSimple> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            Purchaseorderitems result = await Task<Purchaseorderitems>.Run(() => PurchaseorderData.ChangeMobilePO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("AddMobilePO")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> AddMobilePO(ActionRequest<PurchaseOrderItemSimple> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            if (request.Item.version == 1)
            {
                Models.PurchaseorderMobile result = await Task<Models.PurchaseorderMobile>.Run(() => new PurchaseorderMobile(PurchaseorderData.AddMobilePO(request.Item)));
                return this.Ok(result);
            }
            else
            {
                Models.Purchaseorder result = await Task<Models.Purchaseorder>.Run(() => PurchaseorderData.AddMobilePO(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("PublishMobilePO")]
        public async Task<IHttpActionResult> PublishMobilePO(ActionRequest<PurchaseOrderItemSimple> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            return await Publish(PurchaseorderData.Context.OrganizationID.ToString(), request.Item.poid.ToString());
        }

        [HttpPost]
        [Route("UpdateMobilePO")]
        public async Task<IHttpActionResult> UpdateMobilePO(ActionRequest<MobilePurchaseOrder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            List<PurchaseorderMobile> result = await Task<List<PurchaseorderMobile>>.Run(() => PurchaseorderData.UpdateMobilePO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPO")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPO(ActionRequest<PurchaseorderCreteria> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            List<PurchaseOrderItemDetail> result = await Task<List<PurchaseOrderItemDetail>>.Run(() => PurchaseorderData.SearchPO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SavePOGRN")]
        [WebAPI.InfyAuthorize(PermissionSet.lrinvoice, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SavePOGRN(ActionRequest<Lrentry> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            Purchaseorder result = await Task<Purchaseorder>.Run(() => PurchaseorderData.SavePOGRN(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("RemovePOGRN")]
        [WebAPI.InfyAuthorize(PermissionSet.lrinvoice, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> RemovePOGRN(ActionRequest<Lrentry> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            bool result = await Task<bool>.Run(() => PurchaseorderData.RemovePOGRN(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Purchaseorder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Purchaseorderitems != null)
                {
                    foreach (var item in request.Item.Purchaseorderitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Purchaseorder result = await Task<Purchaseorder>.Run(() => PurchaseorderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Purchaseorderitems != null)
                {
                    foreach (var item in request.Item.Purchaseorderitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Purchaseorder result = await Task<Purchaseorder>.Run(() => PurchaseorderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Purchaseorder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            Purchaseorder result = await Task<Purchaseorder>.Run(() => PurchaseorderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Purchaseorder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            Purchaseorder result = await Task<Purchaseorder>.Run(() => PurchaseorderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<Purchaseorder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            Purchaseorder result = await Task<Purchaseorder>.Run(() => PurchaseorderData.UpdateStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ClonePO")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> ClonePO(ActionRequest<Purchaseorder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            bool result = await Task<bool>.Run(() => PurchaseorderData.ClonePO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Purchaseorder> request)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            bool result = await Task<bool>.Run(() => PurchaseorderData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Publish/{organizationid}/{id}")]
        public async Task<IHttpActionResult> Publish(string organizationid,string id)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            PurchaseorderData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            bool result = await Task<bool>.Run(() =>
            {
                PurchaseorderPrint print = PurchaseorderData.Print(new Purchaseorder() { id = long.Parse(id) });
                return PurchaseorderData.Publish(PurchaseorderData.Context.OrganizationID, print);
            });
            return this.Ok(result);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintNOPDF/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintNOPDF(string organizationid,string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            try
            {
                PurchaseorderData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                if (!string.IsNullOrEmpty(id1))
                    id += "/" + id1;
                if (!string.IsNullOrEmpty(id2))
                    id += "/" + id2;
                if (!string.IsNullOrEmpty(id3))
                    id += "/" + id3;

               
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    PurchaseorderPrint print = PurchaseorderData.Print(new Purchaseorder() { pono = id });
                    return PrintPO(PurchaseorderData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid,string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService();
            try
            {
                PurchaseorderData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    poid = long.Parse(id);
                    id = "";
                }
                else
                {
                    if (!string.IsNullOrEmpty(id1))
                        id += "/" + id1;
                    if (!string.IsNullOrEmpty(id2))
                        id += "/" + id2;
                    if (!string.IsNullOrEmpty(id3))
                        id += "/" + id3;
                }

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    PurchaseorderPrint print = PurchaseorderData.Print(new Purchaseorder()
                    {
                        id = poid,
                        pono = id
                    });
                    return PrintPO(PurchaseorderData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(PurchaseorderData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintPO(long organizationid, PurchaseorderPrint result)
        {
            bool hasSizeBased = true;
            int i = 1;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService()
            {
                Context = new UserClaim()
                {
                    OrganizationID = organizationid
                }
            };
            List<Configuration> lstconfig = null;
            if(result.mode == 3)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "PO-DESIGN",
                    organizationid = result.companyid
                });
            }

            if (result.isnontextilepo)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "PO-CONSUMABLES",
                    organizationid = result.companyid
                });
            }

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = result.mode == 0 ? "PO-SIZE" : "PO-ITEM",
                    organizationid = result.companyid
                });
            }

            if (lstconfig == null || lstconfig.Count == 0)
            {
                hasSizeBased = false;
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "PO-ITEM",
                    organizationid = result.companyid
                });
            }

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            if (hasSizeBased &&
                labreport.format.PageBody.Items[0].IsTable &&
            labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                hasSizeBased = false;
                result.Purchaseorderitems.ForEach(e =>
                {
                    if (e.sizename == null)
                        e.sizename = e.size;
                });

                var sizeList = result.Purchaseorderitems.GroupBy(e => new { e.sizeid, e.sizename }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => e.Key.sizename.ToUpper()));
                if (Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    sizeExistOrder = result.Purchaseorderitems.GroupBy(e => e.sizename.ToUpper()).ToList().ConvertAll(e => e.Key);
                    sizeExistOrder.Sort(delegate (string a, string b)
                    {
                        var aindex = sizelist.IndexOf(a);
                        var bindex = sizelist.IndexOf(b);
                        return aindex.CompareTo(bindex);
                    });
                }

                int maxcolumn = Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.po_maxcolumn > 0 ? Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.po_maxcolumn : 16;
                if (sizeExistOrder != null && maxcolumn > sizeExistOrder.Count)
                {
                    if (sizeExistOrder != null && sizeExistOrder.Count != 0)
                    {
                        var groupedList = result.Purchaseorderitems.GroupBy(e => new { e.description, e.designid, e.price }).ToList();
                        result.Purchaseorderitems.Clear();
                        List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                        int index = 0;
                        var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
                        sizeExistOrder.Add("Total");
                        var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;

                        labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                        var colwidth = ((labreport.DefaultPageSettings.MarginBound.Width - totalwidth) / sizeExistOrder.Count);
                        foreach (var sitem in sizeExistOrder)
                        {
                            ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                            content.Type = ReportLibrary.DosFormat.CotnentType.Field;
                            content.Text = sitem;
                            content.Length = colwidth;
                            //if (sitem == "Total")
                            //    content.Length = (int)(rootcolumn.Length*1.5);
                            content.Align = rootcolumn.Align;
                            content.Format = rootcolumn.Format;
                            content.Total = rootcolumn.Total;
                            content.Style = rootcolumn.Style;
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
                            item.jumpqty = new List<PurchaseorderitemsDetail.SizeQty>();
                            foreach (var sitem in sizeExistOrder)
                            {
                                decimal qty = 0;
                                if (sitem == "Total")
                                {
                                    qty = group.Sum(e => e.oqty);
                                }
                                else if (group.Any(e => e.sizename == sitem))
                                {
                                    qty = group.First(e => e.sizename == sitem).oqty;
                                }

                                item.jumpqty.Add(new PurchaseorderitemsDetail.SizeQty()
                                {
                                    name = sitem,
                                    qty = qty
                                });
                            }
                            item.oqty = item.jumpqty.Sum(e => e.name == "Total" ? 0 : e.qty);
                            item.sno = result.Purchaseorderitems.Count + 1;
                            result.Purchaseorderitems.Add(item);
                        }
                        hasSizeBased = true;
                    }
                }
            }

            if (hasSizeBased &&
               labreport.format.PageBody.Items[0].IsTable &&
               labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamictable))
            {
                result.Purchaseorderitems.ForEach(e =>
                {
                    //if (e.sizename == null)
                        e.sizename = e.size;
                });

                var sizeList = result.Purchaseorderitems.GroupBy(e => new { e.sizeid, e.sizename }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => e.Key.sizename.ToUpper()));
                if (Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    sizeExistOrder = result.Purchaseorderitems.GroupBy(e => e.sizename.ToUpper()).ToList().ConvertAll(e => e.Key);
                    sizeExistOrder.Sort(delegate (string a, string b)
                    {
                        var aindex = sizelist.IndexOf(a);
                        var bindex = sizelist.IndexOf(b);
                        return aindex.CompareTo(bindex);
                    });
                }
                var groupedList = result.Purchaseorderitems.GroupBy(e => new { e.description, e.designid, e.price }).ToList();
                result.Purchaseorderitems.Clear();
                foreach (var group in groupedList)
                {
                    var item = group.First();
                    item.jumpqty = new List<PurchaseorderitemsDetail.SizeQty>();
                    foreach (var sitem in sizeExistOrder)
                    {
                        decimal qty = 0;
                        if (sitem == "Total")
                        {
                            qty = group.Sum(e => e.oqty);
                        }
                        else if (group.Any(e => e.sizename == sitem))
                        {
                            qty = group.First(e => e.sizename == sitem).oqty;
                        }

                        item.jumpqty.Add(new PurchaseorderitemsDetail.SizeQty()
                        {
                            name = sitem,
                            qty = qty
                        });
                    }
                    item.oqty = item.jumpqty.Sum(e => e.name == "Total" ? 0 : e.qty);
                    item.sno = result.Purchaseorderitems.Count + 1;
                    result.Purchaseorderitems.Add(item);
                }


                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamictable);
                var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;
                var colwidth = labreport.DefaultPageSettings.MarginBound.Width - totalwidth;
                ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                content.Type = ReportLibrary.DosFormat.CotnentType.Dynamictable;
                content.Text = "Size";
                content.DataType = ReportLibrary.DOSReporter.DataType.Table;
                content.Length = colwidth;
                content.Align = "center";

                content.Format = "";
                content.Total = false;
                content.Name = "sizetable";
                labreport.format.PageBody.LineHeight = labreport.format.PageBody.LineHeight * 2;
                //labreport.format.PageBody.WrapRow = true;
                labreport.format.PageBody.Items[0].Items.Insert(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), content);
                labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);
                result.Purchaseorderitems.ForEach(e => { e.jumpqty.RemoveAll(x => x.qty == 0); });
                var maximumcount = result.Purchaseorderitems.Max(e => e.jumpqty == null ? 1 : e.jumpqty.Count + 1);

                colwidth = colwidth / maximumcount;
                foreach(var column in result.Purchaseorderitems)
                {
                    ReportLibrary.DOSReporter.Table table = new ReportLibrary.DOSReporter.Table();
                    var columnValues = new SortedList<int, object>();
                    foreach (var citem in column.jumpqty)
                    {
                        var clmn = new ReportLibrary.DOSReporter.Column(citem.name, citem.name, ReportLibrary.DOSReporter.DataType.String,
                            "N0",  System.Drawing.StringAlignment.Far, false, colwidth);
                        table.Columns.Add(clmn);
                        columnValues.Add(columnValues.Count,citem.qty);
                    }
                    table.Rows.Add(columnValues);
                    column.sizetable = table;
                }

            }

            if (!hasSizeBased)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "PO-ITEM",
                    organizationid = result.companyid
                });

                if (lstconfig == null || lstconfig.Count == 0)
                    return null;

                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            }

            if (labreport.format.PageBody.Items[0].Items.Exists(e => e.DataType == ReportLibrary.DOSReporter.DataType.Image))
            {
                if (result.Purchaseorderitems.Any(e => !string.IsNullOrEmpty(e.imageid)))
                {
                    var availableheight = labreport.format.PageBody.Length * labreport.format.PageBody.LineHeight;
                    if(labreport.format.PageBody.LineHeight<50)
                        labreport.format.PageBody.LineHeight = 50;
                    labreport.format.PageBody.Length = (int)(availableheight / labreport.format.PageBody.LineHeight);
                }
                else
                {
                    labreport.format.PageBody.Items[0].Items.RemoveAll(e => e.DataType == ReportLibrary.DOSReporter.DataType.Image);
                }
            }
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            foreach (var item in result.Purchaseorderitems)
                item.sno = i++;

            BillitemsDataService billitemsDataService = new BillitemsDataService();
            billitemsDataService.UpdateProductUOM(result.Purchaseorderitems);

            if(lstconfig[0].reportformat.format.Contains("mfgfields[\""))
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                mfgdesignDataService.Context = new UserClaim() { OrganizationID = organizationid };
                var table = mfgdesignDataService.ExportAddOnFields(result.Purchaseorderitems.ConvertAll(x => x.gln).ToList());
                foreach(DataRow dr in table.Rows)
                {
                    string gln = dr["UAN"] == DBNull.Value ? "" : dr["UAN"].ToString();
                    result.Purchaseorderitems.FindAll(x => x.gln == gln).ForEach(y =>
                    {
                        y.mfgfields = dr;
                    });
                }
            }

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            labreport.DataSource = result;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataItems = result.Purchaseorderitems;
            printer.Title = result.company.name + " Purchase Order @ " + result.pono + "-" + result.podate.ToString("dd-MM-yyyy");
            printer.Keywords = result.supplier.name + "," + result.suppliercompany.name;

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
                fci.FileName = "po.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

        [HttpPost]
        [Route("SearchPOSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPOSummary(ActionRequest<PurchaseorderCreteria> request)
        {
            PurchaseorderDataService podata = new PurchaseorderDataService();
            List<Purchaseorder> result = await Task<List<Purchaseorder>>.Run(() => podata.SearchPOSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchAgreementPOSummary")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAgreementPOSummary(ActionRequest<PurchaseorderCreteria> request)
        {
            PurchaseorderDataService podata = new PurchaseorderDataService();
            List<Purchaseorder> result = await Task<List<Purchaseorder>>.Run(() => podata.SearchAgreementPOSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPendingPO")]
        [WebAPI.InfyAuthorize(PermissionSet.purchaseorder, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchPendingPO(ActionRequest<PurchaseorderCreteria> request)
        {
            PurchaseorderDataService podata = new PurchaseorderDataService();
            List<PurchaseorderDetails> result = await Task<List<PurchaseorderDetails>>.Run(() => podata.SearchPendingPO(request.Item));
            return this.Ok(result);
        }

    }
}