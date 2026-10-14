
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
    [RoutePrefix("SalesorderService")]
    public class SalesorderServiceController : ApiController
    {

        [HttpGet]
        [AllowAnonymous]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Salesorder>();
            result.Item = new Salesorder() { Salesorderitems = new List<SalesorderitemsDetail>() };
            result.Item.Salesorderitems.Add(new SalesorderitemsDetail());
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SalesorderCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<Salesorder> result = await Task<List<Salesorder>>.Run(() => SalesorderData.Search(request.Item));
            if (request.Item.customerid > 0 && request.Item.showsize)
            {
                SalesOrderSearchBySize response = await Task<List<Salesorder>>.Run(() => SalesorderData.SearchBySize(result));
                return this.Ok(response);
            }
            else
            {
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchSummary")]
        public async Task<IHttpActionResult> Search(ActionRequest<SalesOrderSummaryCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderSummary> result = await Task<List<SalesOrderSummary>>.Run(() => SalesorderData.SearchSummary(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchUIDSummary")]
        public async Task<IHttpActionResult> SearchUIDSummary(ActionRequest<SalesOrderSummaryCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderUIDSummary> result = await Task<List<SalesOrderUIDSummary>>.Run(() => SalesorderData.SearchUIDSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSizeSummary")]
        public async Task<IHttpActionResult> SearchSizeSummary(ActionRequest<SalesOrderSummaryCreteria> request)
        {
            request.Item.sizesummary = true;
            SalesorderDataService SalesorderData = new SalesorderDataService();
            SalesOrderSizeSummary result = await Task<SalesOrderSizeSummary>.Run(() => SalesorderData.SearchSizeSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSizeGroupStock")]
        public async Task<IHttpActionResult> SearchSizeGroupStock(ActionRequest<SalesorderitemsCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            MFG_SizeGroup<MFG_SizeGroup_Stock> result = await Task<MFG_SizeGroup<MFG_SizeGroup_Stock>>.Run(() => SalesorderData.SearchSizeGroupStock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSizeGroupSales")]
        public async Task<IHttpActionResult> SearchSizeGroupSales(ActionRequest<SalesorderitemsCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            MFG_SizeGroup<MFG_SizeGroup_Stock> result = await Task<MFG_SizeGroup<MFG_SizeGroup_Stock>>.Run(() =>
            request.Item.showdetail ?
            SalesorderData.SearchSizeGroupSalesDetails(request.Item) :
            SalesorderData.SearchSizeGroupSales(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SetTermsandcondition")]
        public async Task<IHttpActionResult> SetTermsandcondition(ActionRequest<SOTermsandcondition> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => SalesorderData.SetTermsandcondition(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SetSMApproveval")]
        public async Task<IHttpActionResult> SetSMApproveal(ActionRequest<SOTermsandcondition> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => SalesorderData.SetSMApproveval(request.Item));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("GetSMApproveval/{id}")]
        public async Task<IHttpActionResult> GetSMApproveval(string id)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorder.Termsandcondition result = await Task<Salesorder.Termsandcondition>.Run(() => SalesorderData.GetSMApproveval(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("GetTermsandcondition/{id}")]
        public async Task<IHttpActionResult> GetTermsandcondition(string id)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorder.Termsandcondition result = await Task<Salesorder.Termsandcondition>.Run(() => SalesorderData.GetTermsandcondition(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("GetSaleOrder/{locationid}/{sono}/{sosourcetype=null}")]
        public async Task<IHttpActionResult> GetSaleOrder(string locationid, string sono, string sosourcetype)
        {
            if (sono.IndexOf("~dot~") >= 0) sono = sono.Replace("~dot~", ".");
            if (sono.IndexOf("~sls~") >= 0) sono = sono.Replace("~sls~", "/");

            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderDetail> result = await Task<List<SalesOrderDetail>>.Run(() => SalesorderData.GetSaleOrder(locationid, sono, sosourcetype));
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
            SalesorderDataService SalesorderData = new SalesorderDataService();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            if (lnversion == 1)
            {
                List<SalesorderMobile> result = await Task<List<SalesorderItemSimple>>.Run(() =>
                {
                    List<string> sizenames = null;
                    if (InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.system_possizelist != null &&
                    InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.system_possizelist != "")
                    {
                        sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.system_possizelist.Split(','));
                    }
                    var sizelist = referencelistDataService.SearchList(new ReferencelistCreteria()
                    {
                        type = "SIZE"
                    });
                    List<SalesorderMobile> rst = new List<SalesorderMobile>();
                    foreach (var item in SalesorderData.SearchMobilePO(lnsupplierid, 0, lnlimit).GroupBy(e => e.soid))
                    {
                        rst.Add(new SalesorderMobile(item.ToList(),
                            InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.mfg_mode <= 1,
                            sizenames, sizelist));
                    }
                    return rst;
                });
                return this.Ok(result);
            }
            else
            {
                List<SalesorderItemSimple> result = await Task<List<SalesorderItemSimple>>.Run(() => SalesorderData.SearchMobilePO(0, 0, 0));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("DeleteMobilePO")]
        public async Task<IHttpActionResult> DeleteMobilePO(ActionRequest<SalesorderItemSimple> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            string result = await Task<string>.Run(() => SalesorderData.DeleteMobilePO(request.Item.id));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeMobilePO")]
        public async Task<IHttpActionResult> ChangeMobilePO(ActionRequest<SalesorderItemSimple> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorderitems result = await Task<Salesorderitems>.Run(() => SalesorderData.ChangeMobilePO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("AddMobilePO")]
        public async Task<IHttpActionResult> AddMobilePO(ActionRequest<SalesorderItemSimple> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            if (request.Item.version == 1)
            {
                List<string> sizenames = null;
                if (InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.system_possizelist != null &&
                InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.system_possizelist != "")
                {
                    sizenames = new List<string>(InfyPOS.Services.ConfigManager.Instance(SalesorderData.Context.OrganizationID).Config.system_possizelist.Split(','));
                }
                var sizelist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SIZE"
                });
                Models.SalesorderMobile result = await Task<Models.SalesorderMobile>.Run(() => new SalesorderMobile(SalesorderData.AddMobilePO(request.Item), sizenames, sizelist));
                return this.Ok(result);
            }
            else
            {
                Models.Salesorder result = await Task<Models.Salesorder>.Run(() => SalesorderData.AddMobilePO(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("PublishMobilePO")]
        public async Task<IHttpActionResult> PublishMobilePO(ActionRequest<SalesorderItemSimple> request)
        {
            var basedata = new BaseData();
            return await Publish(basedata.Context.OrganizationID.ToString(), request.Item.soid.ToString());
        }

        [HttpPost]
        [Route("UpdateMobilePO")]
        public async Task<IHttpActionResult> UpdateMobilePO(ActionRequest<SalesorderItemSimple> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => SalesorderData.UpdateMobilePO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ClearPlan")]
        public async Task<IHttpActionResult> ClearPlan(ActionMulitpleRequest<SalesOrderSummary> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderSummary> result = await Task<List<SalesOrderSummary>>.Run(() => SalesorderData.ClearPlan(request.Items));
            return this.Ok(result);

        }
        [HttpPost]
        [Route("ChangePlan")]
        public async Task<IHttpActionResult> ChangePlan(ActionMulitpleRequest<SalesOrderSummary> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderSummary> result = await Task<List<SalesOrderSummary>>.Run(() => SalesorderData.ChangePlan(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CancelSO")]
        public async Task<IHttpActionResult> CancelSO(ActionMulitpleRequest<SalesOrderSummary> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderSummary> result = await Task<List<SalesOrderSummary>>.Run(() => SalesorderData.CancelSO(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CompleteSO")]
        public async Task<IHttpActionResult> CompleteSO(ActionMulitpleRequest<SalesOrderSummary> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesOrderSummary> result = await Task<List<SalesOrderSummary>>.Run(() => SalesorderData.CompleteSO(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPO")]
        public async Task<IHttpActionResult> SearchPO(ActionRequest<SalesorderCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            List<SalesorderItemDetail> result = await Task<List<SalesorderItemDetail>>.Run(() => SalesorderData.SearchSO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SaveCatalog")]
        public async Task<IHttpActionResult> SaveCatalog(ActionRequest<SalesOrderCatalog> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorder result = await Task<Salesorder>.Run(() => SalesorderData.SaveCatalog(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Salesorder> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.Salesorderitems != null)
                {
                    foreach (var item in request.Item.Salesorderitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Salesorder result = await Task<Salesorder>.Run(() => SalesorderData.Insert(request.Item));
                result.Salesorderitems = GroupSize(SalesorderData.Context.OrganizationID, result.Salesorderitems);
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.Salesorderitems != null)
                {
                    foreach (var item in request.Item.Salesorderitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Salesorder result = await Task<Salesorder>.Run(() => SalesorderData.Update(request.Item));
                result.Salesorderitems = GroupSize(SalesorderData.Context.OrganizationID, result.Salesorderitems);
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Salesorder> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorder result = await Task<Salesorder>.Run(() => SalesorderData.Insert(request.Item));

            return this.Ok(result);
        }
        [HttpPost]
        [Route("SaveRackNumber")]
        public async Task<IHttpActionResult> SaveRackNumber(ActionRequest<Salesorder> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => SalesorderData.SaveRackNumber(request.Item));
            return this.Ok(result);
        }


        public List<SalesorderitemsDetail> GroupSize(long organizationid, List<SalesorderitemsDetail> list)
        {
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.mfg_mode == 5 ||
                InfyPOS.Services.ConfigManager.Instance(organizationid).Config.mfg_mode == 6)
                return list;

            List<SalesorderitemsDetail> retList = list.GroupBy(e => new
            {
                e.designitemcode,
                e.designid,
                e.colourid
            }).ToList().ConvertAll(e => new SalesorderitemsDetail()
            {
                id = -1,
                designid = e.Key.designid,
                oqty = e.Average(x => x.oqty),
                tqty = e.Sum(x => x.oqty),
                description = e.First().description,
                colourid = e.Key.colourid,
                designitemcode = e.Key.designitemcode,
                colourcode = e.First().colourcode,
                colourname = e.First().colourname,
                brandid = e.First().brandid,
                brandname = e.First().brandname,
                productid = e.First().productid,
                productname = e.First().productname,
                imageid = e.ToList().Exists(x => !string.IsNullOrEmpty(x.imageid)) ? e.ToList().First(x => string.IsNullOrEmpty(x.imageid)).imageid : "",
                sizegroup = !string.IsNullOrEmpty(e.First().sizegroup) ? e.First().sizegroup : GetSizeGroup(e.ToList().ConvertAll(x => x.size)),
                size = !string.IsNullOrEmpty(e.First().sizegroup) ? e.First().sizegroup : GetSizeGroup(e.ToList().ConvertAll(x => x.size)),
                price = e.First().price,
                mrp = e.First().mrp,
                addnlcharge = e.First().addnlcharge,
                addnlchargeid = e.First().addnlchargeid,
                pricerange = e.First().pricerange,
                jumpqty = e.ToList().ConvertAll(x => new SalesorderitemsDetail.SizeQty()
                {
                    uid = x.uid,
                    name = x.sizename,
                    id = x.id,
                    oqty = x.oqty,
                    sizeid = x.sizeid
                })
            });
            foreach (var gitem in retList.ToList())
            {
                var jumpgroup = gitem.jumpqty.GroupBy(x => x.sizeid).ToList().FindAll(y => y.Count() > 1);
                if (jumpgroup.Count > 0)
                {
                    for (var i = 0; i < jumpgroup.Count; i++)
                    {
                        var jumpqtylist = gitem.jumpqty.FindAll(x => x.sizeid == jumpgroup[0].Key);
                        gitem.jumpqty.RemoveAll(x => x.sizeid == jumpgroup[0].Key);
                        foreach (var jitem in jumpqtylist)
                        {
                            var newitem = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesorderitemsDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem));
                            newitem.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                            newitem.jumpqty.Add(jitem);
                            newitem.oqty = jitem.oqty;
                            newitem.tqty = jitem.oqty;
                            retList.Add(newitem);
                        }
                    }
                    if (gitem.jumpqty.Count == 0)
                    {
                        retList.Remove(gitem);
                    }
                    else
                    {
                        gitem.oqty = gitem.jumpqty.Average(x => x.oqty);
                        gitem.tqty = gitem.jumpqty.Sum(x => x.oqty);
                    }
                }
            }
            return retList.OrderBy(x => x.designid).ThenBy(y => y.colourcode).ToList();
        }
        private string GetSizeGroup(List<string> sizelist)
        {
            List<string> list = WebAPI.Data.Utility.GetSizeList(sizelist);
            if (list == null) return sizelist[0];
            list.RemoveAll(x => !sizelist.Contains(x));
            if (list.Count == 1) return list[0];
            return list.First() + "-" + list.Last();
        }
        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Salesorder> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorder result = await Task<Salesorder>.Run(() => SalesorderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<Salesorder> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            Salesorder result = await Task<Salesorder>.Run(() => SalesorderData.UpdateStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Salesorder> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => SalesorderData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("AssignPO")]
        public async Task<IHttpActionResult> AssignPO(ActionRequest<SalesorderCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => SalesorderData.AssignPO(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Publish/{organizationid}/{id}")]
        public async Task<IHttpActionResult> Publish(string organizationid, string id)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
            bool result = await Task<bool>.Run(() =>
            {
                SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria() { id = long.Parse(id) });
                return Publish(long.Parse(organizationid), print);
            });
            return this.Ok(result);
        }

        private bool Publish(long organizationid, SalesorderPrint print)
        {
            if (!string.IsNullOrEmpty(print.customer.mobileno) && print.customer.mobileno.Length >= 10 &&
                !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver))
            {
                string licensekey = LicenseManager.License.Key.Replace("-", "");
                InfyPOS.Controllers.ImageController.HashDetail hashDetail = new ImageController.HashDetail(organizationid, ImageController.HashDetail.GetLongHash(licensekey), print.id);
                string smscontent = print.company.name + " PO (Order No " + print.sono + "/" + print.sodate.ToString("dd-MM-yyyy") + ")\r\n";
                smscontent += InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/PO/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
                return InfyPOS.Services.SMSManager.Instance.SendSMS(organizationid, print.customer.mobileno, smscontent);
            }
            return false;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintNOPDF/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintNOPDF(string organizationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    if (long.TryParse(id, out poid))
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
                SalesorderDataService SalesorderData = new SalesorderDataService();
                SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria() { sono = id, id = poid });
                    return PrintPO(SalesorderData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("PrintSLIP")]
        public async Task<IHttpActionResult> PrintSLIP(ActionRequest<SalesorderCreteria> request)
        {
            SalesorderDataService SalesorderData = new SalesorderDataService();
            FileContentInfo result = await Task<FileContentInfo>.Run(() =>
            {
                SalesorderPrint print = SalesorderData.Print(request.Item);
                return PrintPO(SalesorderData.Context.OrganizationID, true, false, print);
            });
            return new FileActionResult(false, "", result.Extension, result.Content);
        }

        [HttpGet]
        [Route("PrintSLIP/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintSLIP(string organizationid, string id,
         string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
                if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    if (long.TryParse(id, out poid))
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
                SalesorderDataService SalesorderData = new SalesorderDataService();
                SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria()
                    {
                        id = poid,
                        sono = id
                    });
                    return PrintPO(SalesorderData.Context.OrganizationID, true, false, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [HttpGet]
        [Route("PrintSOAttachment/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintSOAttachment(string organizationid, string id,
 string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    if (long.TryParse(id, out poid))
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
                SalesorderDataService SalesorderData = new SalesorderDataService();
                SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria()
                    {
                        id = poid,
                        sono = id
                    });
                    return PrintPO(SalesorderData.Context.OrganizationID, true, false, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintUnpackedSO/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintUnpackedSO(string organizationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
                if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
                if (!string.IsNullOrEmpty(id1))
                    id += "/" + id1;
                if (!string.IsNullOrEmpty(id2))
                    id += "/" + id2;
                if (!string.IsNullOrEmpty(id3))
                    id += "/" + id3;
                SalesorderDataService SalesorderData = new SalesorderDataService();
                SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria()
                    {
                        sono = id,
                        unpacked = true
                    });
                    return PrintPO(SalesorderData.Context.OrganizationID, true, false, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPLAN/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintPLAN(string organizationid, string id,
           string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    if (long.TryParse(id, out poid))
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
                SalesorderDataService SalesorderData = new SalesorderDataService();
                SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria()
                    {
                        id = poid,
                        sono = id
                    });
                    return PrintPO(SalesorderData.Context.OrganizationID, true, true, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id,
           string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    if (long.TryParse(id, out poid))
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
                SalesorderDataService SalesorderData = new SalesorderDataService();
                SalesorderData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    SalesorderPrint print = SalesorderData.Print(new SalesorderCreteria()
                    {
                        id = poid,
                        sono = id
                    });
                    return PrintPO(SalesorderData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintPO(long organizationid, SalesorderPrint result)
        {
            return PrintPO(organizationid, false, false, result);
        }
        public FileContentInfo PrintPO(long organizationid, bool printslip, bool printplan, SalesorderPrint result)
        {
            return PrintPO(organizationid, printslip, printplan, false, result);
        }
        public FileContentInfo PrintPO(long organizationid, bool printslip, bool printplan, bool attachment, SalesorderPrint result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "SO-SIZE",
                organizationid = result.companyid
            });
            if (!printslip && !printplan)
            {
                if (lstconfig == null || lstconfig.Count == 0)
                {
                    hasSizeBased = false;
                    lstconfig = config.Search(new ConfigurationCreteria
                    {
                        key = "SO-ITEM",
                        organizationid = result.companyid
                    });
                }

                if (lstconfig == null || lstconfig.Count == 0)
                {
                    hasSizeBased = true;
                    lstconfig = config.Search(new ConfigurationCreteria
                    {
                        key = "SO-SIZE"
                    });
                }

                if (lstconfig == null || lstconfig.Count == 0)
                {
                    hasSizeBased = false;
                    lstconfig = config.Search(new ConfigurationCreteria
                    {
                        key = "SO-ITEM"
                    });
                }
            }
            else if (printslip || printplan)
            {
                //hasSizeBased = true;
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "SO-SLIP",
                    organizationid = result.companyid
                });
                if (lstconfig == null || lstconfig.Count == 0)
                {
                    lstconfig = config.Search(new ConfigurationCreteria
                    {
                        key = "SO-SLIP"
                    });
                }
                if (lstconfig == null || lstconfig.Count == 0)
                    throw new Exception("Please configure for SO-SLIP");
                //if (lstconfig == null || lstconfig.Count == 0)
                //{
                //    lstconfig = config.Search(new ConfigurationCreteria
                //    {
                //        key = "SO-SIZE"
                //    });
                //}
                //if (lstconfig == null || lstconfig.Count == 0)
                //{
                //    hasSizeBased = false;
                //    lstconfig = config.Search(new ConfigurationCreteria
                //    {
                //        key = "SO-ITEM",
                //        organizationid = result.companyid
                //    });
                //}
                //if (lstconfig == null || lstconfig.Count == 0)
                //{
                //    hasSizeBased = false;
                //    lstconfig = config.Search(new ConfigurationCreteria
                //    {
                //        key = "SO-ITEM"
                //    });
                //}
            }
            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);

            if (printslip || printplan)
            {
                hasSizeBased = labreport.format.PageBody.Items[0].IsTable &&
                    labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
            }
            //labreport.format.PageBody.WrapRow = false;
            result.Salesgroupitems = result.Salesorderitems.GroupBy(e => new { e.sizegroup, e.designid, e.colourid }).ToList().ConvertAll(ex => new SalesorderitemsDetail()
            {
                brandname = ex.First().brandname,
                productname = ex.First().productname,
                colourid = ex.Key.colourid,
                sizegroup = ex.Key.sizegroup,
                colourname = ex.First().colourname,
                designid = ex.Key.designid,
                price = ex.First().price,
                oqty = ex.Sum(e => printslip ? (printplan ? e.pqty : e.slipqty) : e.oqty),
                setqty = ex.Average(e => printslip ? (printplan ? e.pqty : e.slipqty) : e.oqty)
            });

            if (attachment)
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                var param = new SortedList<string, object>();
                param.Add("salesorderid", result.id);
                using (var reader = mfgdesignDataService.ExecuteReader(@"with alluid
                                as
                                (
                                select mfgdesign.code,schemelist->>'sizegroup' sizegroup,
	                                schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                                cast(schemelist->>'mrp' as numeric) mrp,
                                schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	                                mfgdesign.imagelist->>0 designimage,
                                colourlist->>'imageid' colourimage from salesorderitems
                                inner join mfgdesign on mfgdesign.code = salesorderitems.designid
                                LEFT JOIN LATERAL
                                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON 
	                                 cast(schemelist->>'sizeid' as bigint) = salesorderitems.sizeid
                                LEFT JOIN LATERAL
                                json_array_elements (cast(mfgdesign.sizegroup->>'colouritems' as json)) 
	                                colourlist ON colourlist->>'colourid' = schemelist->>'colourid'
                                where 
	                                salesorderid = @salesorderid
                                )
                                select distinct dname,mrp,colourimage,designimage from alluid
                                ", param))
                {
                    var allsalesorderitems = new List<SalesorderitemsDetail>();
                    while (reader.Read())
                    {
                        SalesorderitemsDetail salesOrderDetail = new SalesorderitemsDetail();
                        salesOrderDetail.designitemcode = reader["dname"].ToString();
                        salesOrderDetail.mrp = decimal.Parse(reader["mrp"].ToString());
                        if (reader["colourimage"] != DBNull.Value)
                            salesOrderDetail.imageid = reader["colourimage"].ToString();
                        else if (reader["designimage"] != DBNull.Value)
                            salesOrderDetail.imageid = reader["designimage"].ToString();
                        allsalesorderitems.Add(salesOrderDetail);
                    }
                    result.Salesorderitems.ForEach(x =>
                    {
                        if (allsalesorderitems.Exists(y => y.designitemcode == x.designitemcode))
                        {
                            x.imageid = allsalesorderitems.Find(y => y.designitemcode == x.designitemcode).imageid;
                        }
                    });
                }
            }

            result.totalsetqty = result.Salesgroupitems.Sum(e => e.setqty);
            result.totalqty = result.Salesgroupitems.Sum(e => printslip ? (printplan ? e.pqty : e.slipqty) : e.oqty);
            //lstconfig[0].reportformat.format

            if (hasSizeBased &&
                labreport.format.PageBody.Items[0].IsTable &&
            labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                hasSizeBased = false;
                result.Salesorderitems.ForEach(e =>
                {
                    if (e.sizename == null)
                        e.sizename = e.size;
                });
                var sizeList = result.Salesorderitems.GroupBy(e => new { e.sizeid, e.sizename }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => e.Key.sizename.ToUpper()));
                if (sizeExistOrder == null || Services.ConfigManager.Instance(organizationid).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    sizeExistOrder = new List<string>();
                    foreach (var grp in result.Salesorderitems.GroupBy(e => e.sizename.ToUpper()))
                    {
                        sizeExistOrder.Add(grp.Key);
                    }
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
                        var groupedList = result.Salesorderitems.GroupBy(e => new
                        {
                            e.designid,
                            e.colourid,
                            e.colourcode,
                            e.price,
                            e.designitemcode,
                            e.imageid,
                            e.addnlcharge,
                            e.addnlchargename
                        }).ToList();
                        result.Salesorderitems.Clear();
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

                        List<Mfgdesign> alldesigns = null;
                        if (lstconfig[0].reportformat.format.Contains("rackno"))
                        {
                            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                            alldesigns = mfgdesignDataService.Search(new MfgdesignCreteria()
                            {
                                codes = groupedList.ConvertAll(x => x.Key.designid).ToList()
                            });
                        }
                        


                        foreach (var group in groupedList)
                        {
                            var item = group.First();
                            if (alldesigns != null && alldesigns.Exists(x => x.code == group.Key.designid))
                            {
                                item.rackno = alldesigns.Find(x => x.code == group.Key.designid).racknumber;
                            }
                            item.addnlchargename = item.addnlchargename.Split('-')[0];
                            item.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                            foreach (var sitem in sizeExistOrder)
                            {
                                decimal qty = 0;
                                if (sitem == "Total")
                                {
                                    qty = group.Sum(e => printslip ? (printplan ? e.pqty : e.slipqty) : e.oqty);
                                }
                                else if (group.Any(e => e.sizename == sitem))
                                {
                                    qty = group.Sum(e => e.sizename == sitem ? (printslip ? (printplan ? e.pqty : e.slipqty) : e.oqty) : 0);
                                }
                                item.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                                {
                                    name = sitem,
                                    oqty = qty
                                });
                            }
                            item.oqty = item.jumpqty.Find(x => x.name == "Total").oqty;
                            if (item.oqty > 0)
                            {
                                item.sno = result.Salesorderitems.Count + 1;
                                result.Salesorderitems.Add(item);
                            }
                        }
                        result.Salesorderitems = result.Salesorderitems.OrderBy(x => x.designid).ThenBy(y => y.colourcode).ToList();
                        hasSizeBased = true;
                    }
                }
            }

            if (hasSizeBased &&
   labreport.format.PageBody.Items[0].IsTable &&
   labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamictable))
            {
                result.Salesorderitems.ForEach(e =>
                {
                    //if (e.sizename == null)
                    e.sizename = e.size;
                });

                var sizeList = result.Salesorderitems.GroupBy(e => new { e.sizeid, e.sizename }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => e.Key.sizename.ToUpper()));
                if (sizeExistOrder == null || Services.ConfigManager.Instance(organizationid).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    sizeExistOrder = new List<string>();
                    foreach (var grp in result.Salesorderitems.GroupBy(e => e.sizename.ToUpper()))
                    {
                        sizeExistOrder.Add(grp.Key);
                    }
                    sizeExistOrder.Sort(delegate (string a, string b)
                    {
                        var aindex = sizelist.IndexOf(a);
                        var bindex = sizelist.IndexOf(b);
                        return aindex.CompareTo(bindex);
                    });
                }
                var groupedList = result.Salesorderitems.GroupBy(e => new { e.description, e.designid, e.price }).ToList();
                result.Salesorderitems.Clear();
                foreach (var group in groupedList.OrderBy(x => x.Key.description))
                {
                    var item = group.First();
                    item.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                    foreach (var sitem in sizeExistOrder)
                    {
                        decimal qty = 0;
                        if (sitem == "Total")
                        {
                            qty = group.Sum(e => printslip ? (printplan ? e.pqty : e.slipqty) : e.oqty);
                        }
                        else if (group.Any(e => e.sizename == sitem))
                        {
                            var eitem = group.First(e => e.sizename == sitem);
                            qty = printslip ? (printplan ? eitem.pqty : eitem.slipqty) : eitem.oqty;
                        }

                        item.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                        {
                            name = sitem,
                            oqty = qty
                        });
                    }
                    item.oqty = item.jumpqty.Sum(e => e.oqty);
                    if (item.oqty > 0)
                    {
                        item.sno = result.Salesorderitems.Count + 1;
                        result.Salesorderitems.Add(item);
                    }
                }
                result.Salesorderitems = result.Salesorderitems.OrderBy(x => x.designid + x.colourcode).ToList();

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
                result.Salesorderitems.ForEach(e => { e.jumpqty.RemoveAll(x => printslip ? (printplan ? e.pqty == 0 : e.slipqty == 0) : x.oqty == 0); });
                var maximumcount = result.Salesorderitems.Max(e => e.jumpqty == null ? 1 : e.jumpqty.Count + 1);

                colwidth = colwidth / maximumcount;
                foreach (var column in result.Salesorderitems)
                {
                    ReportLibrary.DOSReporter.Table table = new ReportLibrary.DOSReporter.Table();
                    var columnValues = new SortedList<int, object>();
                    foreach (var citem in column.jumpqty)
                    {
                        var clmn = new ReportLibrary.DOSReporter.Column(citem.name, citem.name, ReportLibrary.DOSReporter.DataType.String,
                            "N0", System.Drawing.StringAlignment.Far, false, colwidth);
                        clmn.ShowEmptyForZero = true;
                        table.Columns.Add(clmn);
                        columnValues.Add(columnValues.Count, citem.oqty);
                    }
                    table.Rows.Add(columnValues);
                    column.sizetable = table;
                }
            }

            if (!hasSizeBased)
            {
                if (!printslip && !printplan)
                {
                    lstconfig = config.Search(new ConfigurationCreteria
                    {
                        key = "SO-ITEM",
                        organizationid = result.companyid
                    });

                    if (lstconfig == null || lstconfig.Count == 0)
                    {
                        hasSizeBased = false;
                        lstconfig = config.Search(new ConfigurationCreteria
                        {
                            key = "SO-ITEM"
                        });
                    }

                    if (lstconfig == null || lstconfig.Count == 0)
                        return null;
                }
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            }

            if (labreport.format.PageBody.Items[0].Items.Exists(e => e.DataType == ReportLibrary.DOSReporter.DataType.Image))
            {
                if (result.Salesorderitems.Any(e => !string.IsNullOrEmpty(e.imageid)))
                {
                    var availableheight = labreport.format.PageBody.Length * labreport.format.PageBody.LineHeight;
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

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = hasSizeBased ? result.Salesorderitems : result.Salesgroupitems;
            printer.Title = result.company.name + " Sales Order @ " + result.sono + "-" + result.sodate.ToString("dd-MM-yyyy");
            printer.Keywords = result.customer.name;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            ImageData.File file = null;
            if (!string.IsNullOrEmpty(result.company.logo))
            {
                file = imageData.GetData(result.company.logo.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                file = imageData.GetData(imgid.LongFromBase62());
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

            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            int sno = 1;
            result.Salesorderitems.ForEach(x =>
            {
                x.sno = sno++;
            });
            if (result.Salesgroupitems != null)
            {
                sno = 1;
                result.Salesgroupitems.ForEach(x =>
                {
                    x.sno = sno++;
                });
            }
            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                if (attachment)
                {

                    InfyPOS.PDFGenerator pdfgen = new InfyPOS.PDFGenerator();
                    var byteArrayContent = new ByteArrayContent(pdfgen.ImagePDFGeneration(stream, organizationid, result));
                    var fci = new FileContentInfo(byteArrayContent);
                    fci.FileName = "po.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
                else
                {
                    var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                    fci.FileName = "po.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
            }

        }

        [HttpPost]
        [Route("SearchPOSummary")]
        public async Task<IHttpActionResult> SearchPOSummary(ActionRequest<SalesorderCreteria> request)
        {
            SalesorderDataService podata = new SalesorderDataService();
            List<Salesorder> result = await Task<List<Salesorder>>.Run(() => podata.SearchSOSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPendingPO")]
        public async Task<IHttpActionResult> SearchPendingPO(ActionRequest<SalesorderCreteria> request)
        {
            SalesorderDataService podata = new SalesorderDataService();
            List<SalesorderDetails> result = await Task<List<SalesorderDetails>>.Run(() => podata.SearchPendingPO(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SaveUANSO")]
        public async Task<IHttpActionResult> SaveUANPO(ActionRequest<SOUANUploaded> request)
        {
            SalesorderDataService PurchaseorderData = new SalesorderDataService();
            List<SalesOrderHistory> result = await Task<List<SalesOrderHistory>>.Run(() => PurchaseorderData.SaveUANSO(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SearchStockByUID")]
        public async Task<IHttpActionResult> SearchStockByUID(ActionRequest<SalesorderDataService.BookingRequest> request)
        {
            SalesorderDataService podata = new SalesorderDataService();
            List<SalesorderDataService.BookingResponse> result = await Task<List<SalesorderDataService.BookingResponse>>.Run(() => podata.BookSO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<ChangeAttributes> request)
        {
            SalesorderDataService MfgdesignData = new SalesorderDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

    }
}