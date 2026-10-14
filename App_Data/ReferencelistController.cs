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
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace InfyPOS.Controllers
{

    public class ReferencelistController : ApiController
    {
        [Route("ReferencelistService/BINHIERARCHY/{locationid}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetBinHierarchy(string locationid)
        {
            if (string.IsNullOrEmpty(locationid))
                return this.InternalServerError(new Exception("Select the location"));

            ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
            var resultFULL = await Task<List<BinStorageRefrenceList>>.Run(() =>
                ReferencelistDatafULL.SearchBinStorages(locationid));
            return this.Ok(resultFULL);
        }

        [Route("ReferencelistService/ADDONDATA/{type}/{referenceid=null}")]
        [HttpGet]
        public async Task<IHttpActionResult> ADDONDATA(string type,string referenceid)
        {
            AddondataDataService ReferencelistDatafULL = new AddondataDataService();
            long refid = 0;
            long.TryParse(referenceid, out refid);
            var resultFULL = await Task<List<Addondata>>.Run(() =>
                ReferencelistDatafULL.Search(new AddondataCreteria()
                {
                    customtype = type,
                    referenceid = refid
                }));
            return this.Ok(resultFULL);
        }

        [Route("ReferencelistService/PRODUCTHIERARCHY/{referenceid}")]
        [HttpGet]
        public async Task<IHttpActionResult> ProductHierarchy(string referenceid)
        {
            if (string.IsNullOrEmpty(referenceid))
                return this.InternalServerError(new Exception("Select the parent"));

            ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
            var resultFULL = await Task<List<ReferenceList>>.Run(() =>
                ReferencelistDatafULL.SearchListWithParent(new ReferencelistCreteria()
                {
                    referenceid = long.Parse(referenceid),
                    type = "PRODUCTGROUPS"
                }));
            return this.Ok(resultFULL);
        }

        [Route("ReferencelistService/ACCESSKEY/{sourcetype}/{sourceid}")]
        [HttpGet]
        public async Task<IHttpActionResult> ACCESSKEY(string sourcetype,string sourceid)
        {
            AccesskeysDataService ReferencelistDatafULL = new AccesskeysDataService();
            var resultFULL = await Task<AccesskeysDataService.Accesskeys>.Run(() =>
                ReferencelistDatafULL.Search(new AccesskeysDataService.AccesskeysCreteria()
                {
                    sourceid = long.Parse(sourceid),
                    sourcetype = sourcetype
                }));
            if (resultFULL.Count == 0)
            {
                resultFULL.Add(new AccesskeysDataService.Accesskeys()
                {
                    sourcetype = sourcetype,
                    sourceid = long.Parse(sourceid)
                });
            }
            else if(resultFULL.Exists(x => x.status == -1))
            {
                var lastaccess = resultFULL.FindAll(x => x.status == -1).OrderBy(x => x.id).Last();
                resultFULL.RemoveAll(x => x.status == -1);
                if(resultFULL.Count > 0)
                {
                    resultFULL[0].lastAccess = lastaccess;
                    return this.Ok(resultFULL[0]);
                }
                else
                {
                    return this.Ok(new AccesskeysDataService.Accesskeys()
                    {
                        sourcetype = sourcetype,
                        sourceid = long.Parse(sourceid),
                        lastAccess = lastaccess
                    });
                }
            }
            return this.Ok(resultFULL[0]);
        }
        [Route("ReferencelistService/GENACCESSKEY/{sourcetype}/{sourceid}")]
        [HttpGet]
        public async Task<IHttpActionResult> GENACCESSKEY(string sourcetype, string sourceid)
        {
            AccesskeysDataService ReferencelistDatafULL = new AccesskeysDataService();
            var resultFULL = await Task<AccesskeysDataService.Accesskeys>.Run(() =>
                ReferencelistDatafULL.Create(new AccesskeysDataService.Accesskeys()
                {
                    sourceid = long.Parse(sourceid),
                    sourcetype = sourcetype
                }));
            return this.Ok(resultFULL);
        }
        [Route("ReferencelistService/REVOKEACCESSKEY")]
        [HttpPost]
        public async Task<IHttpActionResult> RevokeAccessKey(ActionRequest<AccesskeysDataService.Accesskeys> request)
        {
            AccesskeysDataService ReferencelistDatafULL = new AccesskeysDataService();
            var resultFULL = await Task<AccesskeysDataService.Accesskeys>.Run(() =>
                ReferencelistDatafULL.RevokeAccessKey(request.Item));
            return this.Ok(resultFULL);
        }
        [HttpPost]
        [Route("ReferencelistService/ChangeProductHierarchy")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ChangeProductHierarchy(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.ChangeProductHierarchy(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/SaveLedgerHierarchy")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> SaveLedgerHierarchy(ActionRequest<LedgerHierarchy> request)
        {
            if (request.Item == null || request.Item.typeid <= 0)
                return this.InternalServerError(new Exception("Select the type"));

            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            LedgerHierarchy result = await Task<LedgerHierarchy>.Run(() => ReferencelistData.SaveLedgerHierarchy(request.Item));
            return this.Ok(result);
        }

        [Route("ReferencelistService/TOUCHITEMS/{locationid}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTouchItems(string locationid)
        {
          

            ItemsDataService ReferencelistDatafULL = new ItemsDataService();
            var resultFULL = await Task<List<TouchItem>>.Run(() =>
                ReferencelistDatafULL.TouchItems(long.Parse(locationid)));
            return this.Ok(resultFULL);
        }


        [Route("ReferencelistService/LEDGERHIERARCHY/{type}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetLedgerHierarchy(string type)
        {
            if (string.IsNullOrEmpty(type))
                return this.InternalServerError(new Exception("Select the type"));

            ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
            var resultFULL = await Task<List<BinStorageRefrenceList>>.Run(() =>
                ReferencelistDatafULL.SearchLedgerHierarchy(type));
            return this.Ok(resultFULL);
        }

        [InfyPOS.Common.EnableETag(EnableETag.Mode.ReferenceList)]
        [Route("ReferencelistService/{id}/{productgroup=null}")]
        [HttpGet]
        public async Task<IHttpActionResult> Get(string id, string productgroup)
        {
            try
            {
                if (id == "PRODUCTBRANDS")
                {
                    ItemsDataService itemsDataService = new ItemsDataService();
                    return this.Ok(itemsDataService.SearchProductBrand());
                }
                if (id == "SIZEGROUPINGS")
                {
                    return this.Ok(WebAPI.Data.Utility.GetSizeGroups());
                }
                if (id == "SIZEGROUP")
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    List<ReferenceSizeGroup> retlist = referencelistDataService.SearchSizeGroups();
                    return this.Ok(retlist);
                }
                if (id == "NONUAN")
                {
                    ItemsDataService referencelistDataService = new ItemsDataService();
                    List<NonUANItem> retlist = referencelistDataService.SearchNonUANItem();
                    return this.Ok(retlist);
                }
                if (id == "SIZE")
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var sizelist = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "SIZE"
                    });
                    sizelist.RemoveAll(x => x.additionalinfo != null &&
                    x.additionalinfo.isonline);
                    return this.Ok(sizelist);
                }
                if (id == "VSIZE")
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var sizelist = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "SIZE"
                    });
                    sizelist.RemoveAll(x => x.additionalinfo == null ||
                    !x.additionalinfo.isonline);
                    return this.Ok(sizelist);
                }
                if (id == "SIZEGROUPLIST")
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var list = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "SIZEGROUP"
                    });

                    var sizelist = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "SIZE"
                    });
                    List<Tuple<string, List<string>>> retlist = new List<Tuple<string, List<string>>>();
                    foreach (var item in list)
                    {
                        if (item.additionalinfo != null && item.additionalinfo.sizegroups.Count > 0)
                        {
                            List<string> lst = new List<string>();
                            item.additionalinfo.sizegroups.ForEach(e =>
                            {
                                if (sizelist.Exists(x => x.id == e))
                                {
                                    lst.Add(sizelist.Find(x => x.id == e).name);
                                }
                            });
                            retlist.Add(new Tuple<string, List<string>>(item.name, lst));
                        }
                    }
                    return this.Ok(retlist);
                }
                if (id == "SKUDESIGN")
                {
                    SKUMasterDataService ReferencelistDatafULL = new SKUMasterDataService();
                    List<SKUMaster> resultFULL = await Task<List<SKUMaster>>.Run(() => ReferencelistDatafULL.Search(new SKUMasterCreteria()
                    {

                    }));
                    return this.Ok(resultFULL);
                }
                if (id == "MFGWOFLOW")
                {
                    MfgworkfloworchestrationDataService ReferencelistDatafULL = new MfgworkfloworchestrationDataService();
                    List<Mfgworkfloworchestration> resultFULL = await Task<List<Mfgworkfloworchestration>>.Run(() =>
                    ReferencelistDatafULL.Search(new MfgworkfloworchestrationCreteria()
                    {

                    }));
                    return this.Ok(resultFULL);
                }
                if (id == "MFGSAMPLES")
                {
                    MfgsampleorderDataService ReferencelistDatafULL = new MfgsampleorderDataService();
                    List<MfgSampleList> resultFULL = await Task<List<MfgSampleList>>.Run(() =>
                    {
                        var customerlist = ReferencelistDatafULL.CustomerSamples(long.Parse(productgroup));
                        var commonlist = ReferencelistDatafULL.CustomerSamples(-1);
                        customerlist.AddRange(commonlist);
                        return customerlist;
                    });
                    return this.Ok(resultFULL);
                }
                if (id == "MFGPENDINGSO")
                {
                    SalesorderDataService ReferencelistDatafULL = new SalesorderDataService();
                    List<SampleOrderSO> resultFULL = await Task<List<SampleOrderSO>>.Run(() =>
                    ReferencelistDatafULL.PendingSampleOrderSO(long.Parse(productgroup.Split('-')[0]), long.Parse(productgroup.Split('-')[1])));
                    return this.Ok(resultFULL);
                }
                if (id == "FABRIC")
                {
                    BOMMasterDataService ReferencelistDatafULL = new BOMMasterDataService();
                    List<BOMBase> resultFULL = await Task<List<BOMBase>>.Run(() => ReferencelistDatafULL.SearchList(new BOMMasterCreteria()
                    {
                        coretypeid = 1
                    }));
                    return this.Ok(resultFULL.OrderBy(x => x.bommastercode));
                }
                if (id == "BOM")
                {
                    BOMMasterDataService ReferencelistDatafULL = new BOMMasterDataService();
                    List<BOMMaster> resultFULL = await Task<List<BOMMaster>>.Run(() => ReferencelistDatafULL.Search(new BOMMasterCreteria()
                    {

                    }));
                    return this.Ok(resultFULL.OrderBy(x => x.bommastercode));
                }
                if (id == "BOMLIST")
                {
                    BOMMasterDataService ReferencelistDatafULL = new BOMMasterDataService();
                    List<BOMBase> resultFULL = await Task<List<BOMBase>>.Run(() => ReferencelistDatafULL.SearchList(new BOMMasterCreteria()
                    {

                    }));
                    return this.Ok(resultFULL.OrderBy(x => x.bommastercode));
                }
                if (id == "PERMISSION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    return this.Ok(LicenseManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Permissions.Permissions);
                }
                if (id == "SPECIALPERMISSION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    return this.Ok(LicenseManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Permissions.SpecialPermissions);
                }
                if (id == "DATAPERMISSION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    return this.Ok(LicenseManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Permissions.DataPermissions);
                }
                long lnproductgroup = 0;
                if (!string.IsNullOrEmpty(productgroup))
                    long.TryParse(productgroup, out lnproductgroup);
                if (id == "AUTONUMBER")
                {
                    AutonumberDataService autodata = new AutonumberDataService();
                    return this.Ok(autodata.Search(new AutonumberCreteria()
                    {
                    }));
                    //select locationid, type, prefix, filler, nolength from autonumber
                }
                if (id == "BILLKEY")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<CustomReferenceItem> resultFULL = await Task<List<CustomReferenceItem>>.Run(() => ReferencelistDatafULL.Search(new CustomReferenceItem()
                    {
                        type = "BULKSALEREASON,SEASONS,AREA"
                    }));
                    resultFULL.ForEach(e => e.data = null);
                    return this.Ok(resultFULL);
                }
                if (id == "COUPONS")
                {
                    GiftCouponDataService CouponData = new GiftCouponDataService();
                    var results = CouponData.SearchCouponList().ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    return this.Ok(results);
                }
                /*if (id == "BARCODEID")
                {
                    AutonumberDataService promotiondata = new AutonumberDataService();
                    var promolist = promotiondata.Search(new AutonumberCreteria()
                    {
                        barcodenos = true
                    }).ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.description
                    });
                    return this.Ok(promolist);
                }*/
                if (id == "PROMOTIONS")
                {
                    PromotionsDataService promotiondata = new PromotionsDataService();
                    var promolist = promotiondata.Search(new PromotionsCreteria()).ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    return this.Ok(promolist);
                }
                if (id == "BARCODEFILE")
                {
                    Common.ImageData imageData = new ImageData();
                    bool all = !string.IsNullOrEmpty(productgroup) && productgroup.ToLower() == "true";
                    var promolist = imageData.SearchMyDocument(all); 
                    return this.Ok(promolist);
                }
                if (id == "SCHEMES")
                {
                    BuybackschemeDataService promotiondata = new BuybackschemeDataService();
                    var schemes = promotiondata.Search(new BuybackschemeCreteria()
                    {
                        hideitems = true
                    });
                    return this.Ok(schemes);
                }
                if (id == "EMISCHEMES")
                {
                    EmischemesDataService promotiondata = new EmischemesDataService();
                    var schemes = promotiondata.Search(new EmischemesCreteria());

                    var currentdate = promotiondata.DateTimeNow.Date;
                    schemes.RemoveAll(x => !(x.validfrom <= currentdate && x.validto >= currentdate));
                    return this.Ok(schemes);
                }
                if (id == "RECHARGESCHEMES")
                {
                    RechargeschemeDataService promotiondata = new RechargeschemeDataService();
                    var schemes = promotiondata.Search(new RechargeschemeCreteria()
                    {
                        hideitems = false
                    });

                    var currentdate = promotiondata.DateTimeNow.Date;
                    //schemes.Where(e=>e.items !=null).ToList().ForEach(e => e.items.RemoveAll(x => !(x.from.Date <= currentdate && x.to.Date >= currentdate)));
                    schemes.Where(e => e.items != null).ToList().ForEach(e => e.items.RemoveAll(x => !x.isactive));
                    return this.Ok(schemes);
                }
                if (id == "TDSRULES")
                {
                    LedgerDataService promotiondata = new LedgerDataService();
                    return this.Ok(promotiondata.TDSLedgerRules());
                }
                if (id == "LEDGERHEADERS")
                {
                    LedgerDataService promotiondata = new LedgerDataService();
                    return this.Ok(promotiondata.LedgerHeaders());
                }
                if (id == "LEDGERS")
                {
                    if (productgroup != null && productgroup.IndexOf('_') > 0)
                    {
                        List<int> types = new List<int>();
                        foreach (var item in productgroup.Split('_'))
                        {
                            int type = 0;
                            if (int.TryParse(item, out type))
                            {
                                types.Add(type);
                            }
                        }
                        LedgerDataService promotiondata = new LedgerDataService();
                        var promolist = promotiondata.Search(new LedgerCreteria()
                        {
                            typeids = types
                        });
                        List<Ledger> returnList = new List<Ledger>();
                        var grouplist = promolist.GroupBy(e => e.typeid);
                        foreach (var item in productgroup.Split('_'))
                        {
                            int type = 0;
                            if (int.TryParse(item, out type))
                            {
                                if (grouplist.Any(e => e.Key == type))
                                {
                                    returnList.AddRange(grouplist.First(e => e.Key == type).ToList());
                                }
                            }
                        }
                        return this.Ok(returnList);
                    }
                    else
                    {
                        int type = 0;
                        int.TryParse(productgroup, out type);
                        LedgerDataService promotiondata = new LedgerDataService();
                        var promolist = promotiondata.Search(new LedgerCreteria()
                        {
                            typeid = type
                        });
                        return this.Ok(promolist);
                    }
                }
                if (id == "ALLTAX")
                {
                    TaxDataService sDataService = new TaxDataService();
                    var taxcreteria = new TaxCreteria();
                    taxcreteria.isactive = true;
                    List<Tax> taxList = await Task<List<Tax>>.Run(() => sDataService.Search(taxcreteria));
                    return this.Ok(taxList);
                }
                if (id == "PURCHASETAX")
                {
                    TaxDataService sDataService = new TaxDataService();
                    var taxcreteria = new TaxCreteria();
                    taxcreteria.ispurchasetax = true;
                    taxcreteria.isactive = true;
                    List<Tax> taxList = await Task<List<Tax>>.Run(() => sDataService.Search(taxcreteria));
                    return this.Ok(taxList);
                }
                if (id == "CESSTAX")
                {
                    TaxDataService sDataService = new TaxDataService();
                    var taxcreteria = new TaxCreteria();
                    taxcreteria.type = 6;
                    taxcreteria.isactive = true;
                    List<Tax> taxList = await Task<List<Tax>>.Run(() => sDataService.Search(taxcreteria));
                    return this.Ok(taxList);
                }
                
                else if (id == "SALESTAX")
                {
                    TaxDataService sDataService = new TaxDataService();
                    var taxcreteria = new TaxCreteria();
                    taxcreteria.issalestax = true;
                    taxcreteria.isactive = true;
                    List<Tax> taxList = await Task<List<Tax>>.Run(() => sDataService.Search(taxcreteria));

                    return this.Ok(taxList);
                }
                else if (id == "ITEMS")
                {
                    ItemsDataService sDataService = new ItemsDataService();
                    List<Items> ItemsList = await Task<List<Items>>.Run(() => sDataService.Search(new ItemsCreteria()
                    {
                        onlysearchable = true
                    }));
                    return this.Ok(ItemsList);
                }
                if (id == "USERLIST")
                {
                    UsersDataService sDataService = new UsersDataService();
                    List<UserBasic> suppliers = await Task<List<UserBasic>>.Run(() => sDataService.SearchAll(new UsersCreteria()));
                    return this.Ok(suppliers.OrderBy(x => x.title));
                }
                if (id == "PRODUCTGROUP")
                {
                    ProductgroupDataService sDataService = new ProductgroupDataService();
                    List<Productgroup> suppliers = await Task<List<Productgroup>>.Run(() => sDataService.Search(new Models.ProductgroupCreteria()));
                    //var list = suppliers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    //{
                    //    id = e.id,
                    //    name = e.name,
                    //    locationid = e.locationid
                    //});
                    return this.Ok(suppliers.OrderBy(x => x.name));
                }
                if (id == "SUPPLIER")
                {
                    var request = new SupplierCreteria() { rowtype = -1 };
                    if (!string.IsNullOrEmpty(productgroup))
                    {
                        int rowtype = 0;
                        if (int.TryParse(productgroup, out rowtype))
                            request.rowtype = rowtype;
                    }
                    SupplierDataService sDataService = new SupplierDataService();
                    List<SupplierHeader> suppliers = await Task<List<SupplierHeader>>.Run(() => sDataService.SearchHeaders(request));
                    //var list = suppliers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    //{
                    //    id = e.id,
                    //    name = e.name,
                    //    locationid = e.locationid
                    //});
                    return this.Ok(suppliers.OrderBy(x => x.name));
                }
                if (id == "SUPPLIERWITHCITY")
                {
                    var request = new SupplierCreteria() { rowtype = -1 };
                    if (!string.IsNullOrEmpty(productgroup))
                    {
                        int rowtype = 0;
                        if (int.TryParse(productgroup, out rowtype))
                            request.rowtype = rowtype;
                    }
                    request.supplierwithcity = true;
                    SupplierDataService sDataService = new SupplierDataService();
                    List<SupplierHeader> suppliers = await Task<List<SupplierHeader>>.Run(() => sDataService.SearchHeaders(request));
                    suppliers.ForEach(e => e.name = e.name + " - " + e.city);
                    return this.Ok(suppliers.OrderBy(x => x.name));
                }
                if (id == "SUPPLIERLEDGER")
                {
                    SupplierDataService sDataService = new SupplierDataService();
                    List<SupplierLedger> suppliers = await Task<List<SupplierLedger>>.Run(() => sDataService.SupplierLedger(new SupplierCreteria()));
                    //var list = suppliers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    //{
                    //    id = e.id,
                    //    name = e.name,
                    //    locationid = e.locationid
                    //});
                    return this.Ok(suppliers.OrderBy(x => x.name));
                }
                if (id == "TRANSPORT")
                {
                    int transportertype = -1;
                    if (string.IsNullOrEmpty(productgroup) || !int.TryParse(productgroup, out transportertype))
                    {
                        transportertype = 0;
                    }
                    TransportDataService sDataService = new TransportDataService();
                    List<Transport> transports = await Task<List<Transport>>.Run(() => sDataService.Search(new TransportCreteria()
                    {
                        transportertype = transportertype
                    }));
                    var list = transports.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "TRANSPORTONLY")
                {
                    TransportDataService sDataService = new TransportDataService();
                    List<Transport> transports = await Task<List<Transport>>.Run(() => sDataService.Search(new TransportCreteria()
                    {
                        transportertype = 2
                    }));
                    var list = transports.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name,
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "COURIERONLY")
                {
                    TransportDataService sDataService = new TransportDataService();
                    List<Transport> transports = await Task<List<Transport>>.Run(() => sDataService.Search(new TransportCreteria()
                    {
                        transportertype = 1
                    }));
                    var list = transports.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "TRANSPORTWITHCHARGE")
                {
                    TransportDataService sDataService = new TransportDataService();
                    List<Transport> transports = await Task<List<Transport>>.Run(() => sDataService.Search(new TransportCreteria()));
                    return this.Ok(transports);
                }
                if (id == "AGREEMENTPO")
                {
                    PurchaseorderDataService sDataService = new PurchaseorderDataService();
                    List<Purchaseorder> transports = await Task<List<Purchaseorder>>.Run(() => sDataService.Search(new PurchaseorderCreteria()
                    {
                        isagreement = true,
                        statusid = 1
                    }));
                    var list = transports.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = string.Concat(e.pono, " - ", e.suppliername),
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "SUPPLIERCOMPANY")
                {
                    var request = new SuppliercompanyCreteria() { rowtype = -1 };
                    if (!string.IsNullOrEmpty(productgroup))
                    {
                        int rowtype = 0;
                        if (int.TryParse(productgroup, out rowtype))
                            request.rowtype = rowtype;
                    }
                    SuppliercompanyDataService sDataService = new SuppliercompanyDataService();
                    List<SupplierCompanyHeader> suppliercompany = await Task<List<SupplierCompanyHeader>>.Run(() => sDataService.SearchHeader(request));
                    return this.Ok(suppliercompany.OrderBy(x => x.name));
                }
                if (id == "SUPPLIERPRODUCT")
                {
                    SuppliercompanyDataService sDataService = new SuppliercompanyDataService();
                    List<SupplierProduct> suppliercompany = await Task<List<SupplierProduct>>.Run(() => sDataService.SearchProductHeader());
                    return this.Ok(suppliercompany);
                }
                if (id == "SELLINGCOMPANY")
                {
                    CompanyDataService sDataService = new CompanyDataService();
                    List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()
                    {
                        onlyseller = false
                    }));

                    var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.system_supportcompanywisebarcode &&
                        sDataService.Context.CompanyID > 0)
                    {
                        list.RemoveAll(e => e.id != sDataService.Context.CompanyID);
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.analyser_access_company &&
                        sDataService.Context.accessiblecompany != null &&
                        sDataService.Context.accessiblecompany.Count > 0)
                    {
                        if (!sDataService.Context.accessiblecompany.Exists(e => e == -1))
                        {
                            list.RemoveAll(e => !sDataService.Context.accessiblecompany.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(list);
                }

                if (id == "COMPANY")
                {
                    CompanyDataService sDataService = new CompanyDataService();
                    List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()
                    {
                        onlyseller = false
                    }));

                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.system_supportcompanywisebarcode &&
                        sDataService.Context.CompanyID > 0)
                    {
                        buyingcompany.RemoveAll(e => e.id != sDataService.Context.CompanyID);
                    }
                    var list = buyingcompany.ConvertAll(e => new BaseCompany()
                    {
                        id = e.id,
                        name = e.name,
                        internalbuyer = e.internalbuyer,
                        internalmargin = e.internalmargin,
                        stateid = e.stateid,
                        company_as_supplierid = e.company_as_supplierid,
                        gstno = e.gstno
                    });
                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.analyser_access_company &&
                        sDataService.Context.accessiblecompany != null &&
                       sDataService.Context.accessiblecompany.Count > 0)
                    {
                        if (!sDataService.Context.accessiblecompany.Exists(e => e == -1))
                        {
                            list.RemoveAll(e => !sDataService.Context.accessiblecompany.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(list);
                }
                if (id == "FULLCOMPANY")
                {
                    CompanyDataService sDataService = new CompanyDataService() { Context = new UserClaim() { OrganizationID = 0 } };
                    List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()));
                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.system_supportcompanywisebarcode &&
                        sDataService.Context.CompanyID > 0)
                    {
                        buyingcompany.RemoveAll(e => e.id != sDataService.Context.CompanyID);
                    }
                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.analyser_access_company &&
                        sDataService.Context.accessiblecompany != null &&
                       sDataService.Context.accessiblecompany.Count > 0)
                    {
                        if (!sDataService.Context.accessiblecompany.Exists(e => e == -1))
                        {
                            buyingcompany.RemoveAll(e => !sDataService.Context.accessiblecompany.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(buyingcompany);
                }
                if (id == "BUYINGCOMPANY")
                {
                    CompanyDataService sDataService = new CompanyDataService();
                    List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()));
                    var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.system_supportcompanywisebarcode &&
                        sDataService.Context.CompanyID > 0)
                    {
                        list.RemoveAll(e => e.id != sDataService.Context.CompanyID);
                    }
                    if (InfyPOS.Services.ConfigManager.Instance(sDataService.Context.OrganizationID).Config.analyser_access_company &&
                        sDataService.Context.accessiblecompany != null &&
                       sDataService.Context.accessiblecompany.Count > 0)
                    {
                        if (!sDataService.Context.accessiblecompany.Exists(e => e == -1))
                        {
                            list.RemoveAll(e => !sDataService.Context.accessiblecompany.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(list);
                }

                if (id == "PRICETAGS" || id == "ALLPRICETAGS")
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var priceresult = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "PRICETAGS"
                    });
                    return this.Ok(priceresult);
                }

                if (id == "SETPRODUCTS")
                {
                    var includeconsumable = false;
                    if (lnproductgroup == 9999)
                    {
                        lnproductgroup = 0;
                        includeconsumable = true;
                    }
                    ProductsDataService sDataService = new ProductsDataService();
                    List<ProductsBase> buyingcompany = await Task<List<ProductsBase>>.Run(() => sDataService.SearchList(new ProductsCreteria()
                    {
                        productgroup = lnproductgroup,
                        allproducts = id == "ALLPRODUCTS",
                        includeconsumables = includeconsumable
                    }));
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
                    var designs = mfgdesignDataService.Search(new MfgdesignCreteria()
                    {
                        onlycombo = true,
                        onlydesign = true
                    });
                    buyingcompany.RemoveAll(x => !designs.Exists(y => y.productid == x.id));
                    return this.Ok(buyingcompany.OrderBy(x => x.name));
                }

                if (id == "PRODUCTS" || id == "ALLPRODUCTS")
                {
                    var includeconsumable = false;
                    if (lnproductgroup == 9999)
                    {
                        lnproductgroup = 0;
                        includeconsumable = true;
                    }
                    ProductsDataService sDataService = new ProductsDataService();
                    List<ProductsBase> buyingcompany = await Task<List<ProductsBase>>.Run(() => sDataService.SearchList(new ProductsCreteria()
                    {
                        productgroup = lnproductgroup,
                        allproducts = id == "ALLPRODUCTS",
                        includeconsumables = includeconsumable
                    }));
                    //var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    //{
                    //    id = e.id,
                    //    name = e.name
                    //});
                    return this.Ok(buyingcompany.OrderBy(x => x.name));
                }

                if (id == "EXPENSELEDGER")
                {
                    LedgerDataService sDataService = new LedgerDataService();
                    List<LedgerDetail> ledgers = await Task<List<LedgerDetail>>.Run(() => sDataService.Search(new LedgerCreteria()
                    {
                        typeid = 10
                    }));
                    return this.Ok(ledgers.OrderBy(x => x.displayname));
                }

                if (id == "TAXLEDGER")
                {
                    LedgerDataService sDataService = new LedgerDataService();
                    List<LedgerDetail> ledgers = await Task<List<LedgerDetail>>.Run(() => sDataService.Search(new LedgerCreteria()
                    {
                        typeid = 16
                    }));
                    var list = ledgers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.billingname,
                        name = e.displayname
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "FULL-TDSLEDGERS")
                {
                    LedgerDataService sDataService = new LedgerDataService();
                    List<LedgerDetail> ledgers = await Task<List<LedgerDetail>>.Run(() => sDataService.Search(new LedgerCreteria()
                    {
                        suppliertds = true
                    }));
                    return this.Ok(ledgers.OrderBy(x => x.displayname));
                }

                if (id == "TDSLEDGERS")
                {
                    LedgerDataService sDataService = new LedgerDataService();
                    List<LedgerDetail> ledgers = await Task<List<LedgerDetail>>.Run(() => sDataService.Search(new LedgerCreteria()
                    {
                        suppliertds = true
                    }));
                    var list = ledgers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.billingname,
                        name = e.displayname
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }

                if (id == "TCSLEDGERS")
                {
                    LedgerDataService sDataService = new LedgerDataService();
                    List<LedgerDetail> ledgers = await Task<List<LedgerDetail>>.Run(() => sDataService.Search(new LedgerCreteria()
                    {
                        filterid = LedgerCreteria.Filter.TCSLedgers
                    }));
                    var list = ledgers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.billingname,
                        name = e.displayname
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "GSTLEDGERS")
                {
                    LedgerDataService sDataService = new LedgerDataService();
                    List<LedgerDetail> ledgers = await Task<List<LedgerDetail>>.Run(() => sDataService.Search(new LedgerCreteria()
                    {
                        filterid = LedgerCreteria.Filter.GSTLedgers
                    }));
                    List<long> emptylong = new List<long>();
                    var rows = ledgers.Select(x=>new { x.id,code = x.billingname,name = x.displayname,
                        invstaxids = (x.attributes != null ? x.attributes.invstaxids : emptylong),
                        invptaxids = (x.attributes != null ? x.attributes.invptaxids : emptylong)
                    }).OrderBy(x=>x.name).ToList();
                    return this.Ok(rows);
                    //var list = ledgers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    //{
                    //    id = e.id,
                    //    code = e.billingname,
                    //    name = e.displayname
                    //});
                    //return this.Ok(list.OrderBy(x => x.name));
                }

                if (id == "AGENT")
                {
                    int agenttype = 0;
                    if (!string.IsNullOrEmpty(productgroup))
                    {
                        int.TryParse(productgroup, out agenttype);
                    }
                    AgentDataService sDataService = new AgentDataService();
                    List<Agent> agents = await Task<List<Agent>>.Run(() => sDataService.Search(new AgentCreteria()
                    {
                        agenttypeid = agenttype
                    }));
                    var list = agents.ConvertAll<AgentBase>(e => new AgentBase
                    {
                        id = e.id,
                        name = e.name,
                        agenttypeid =e.agenttypeid,
                        commissionamount = e.commissionamount,
                        commissionpercentage = e.commissionpercentage
                    });
                    return this.Ok(list.OrderBy(x => x.name));
                }
                if (id == "STRUCTURE")
                {
                    EmployeesalarystructureDataService sDataService = new EmployeesalarystructureDataService();
                    List<Employeesalarystructure> structure = await Task<List<Employeesalarystructure>>.Run(() => sDataService.Search(new EmployeesalarystructureCreteria()));
                    var list = structure.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name
                    });
                    return this.Ok(list);
                }
                if (id == "FULL-STRUCTURE")
                {
                    EmployeesalarystructureDataService sDataService = new EmployeesalarystructureDataService();
                    List<Employeesalarystructure> structure = await Task<List<Employeesalarystructure>>.Run(() => sDataService.Search(new EmployeesalarystructureCreteria()));
                    return this.Ok(structure);
                }
                if (id == "EMPLOY")
                {
                    EmployeeDataService sDataService = new EmployeeDataService();
                    List<EmployeeHeader> buyingcompany = await Task<List<EmployeeHeader>>.Run(() => sDataService.SearchHeader(new EmployeeCreteria()
                    {
                        currentEmployees = true
                    }));
                    var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.employeecode + " - " + e.name,
                        name = e.name,
                        productgroup = e.sectionid
                    });
                    list.Add(new ReferenceItem()
                    {
                        id = -9,
                        code = "SHOW LEFT EMPLYEE",
                        name = "SHOW LEFT EMPLYEE"
                    });
                    return this.Ok(list);
                }
                if (id == "EMPLOYEELEDGER")
                {
                    EmployeeDataService sDataService = new EmployeeDataService();
                    List<EmployeeHeader> list = await Task<List<EmployeeHeader>>.Run(() => sDataService.SearchHeader(new EmployeeCreteria()));
                    return this.Ok(list);
                }
                if (id == "TAILORS")
                {
                    TailorDataService sDataService = new TailorDataService();
                    List<Tailor> list = await Task<List<Tailor>>.Run(() => sDataService.Search(new TailorCreteria()));
                    return this.Ok(list);
                }
                if (id == "SALESMEN")
                {
                    EmployeeDataService sDataService = new EmployeeDataService();
                    List<EmployeeHeader> list = await Task<List<EmployeeHeader>>.Run(() => sDataService.SearchHeader(new EmployeeCreteria()
                    {
                        onlysalesmen = true
                    }
                    ));
                    return this.Ok(list);
                }
                if (id == "IEMPLOY")
                {
                    EmployeeDataService sDataService = new EmployeeDataService();
                    List<EmployeeHeader> buyingcompany = await Task<List<EmployeeHeader>>.Run(() => sDataService.SearchHeader(new EmployeeCreteria()
                    {
                        includeleft = true
                    }
                    ));
                    var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.employeecode + " - " + e.name,
                        name = e.name
                    });
                    return this.Ok(list);
                }
                if (id == "PURCHASEMANAGER")
                {
                    EmployeeDataService sDataService = new EmployeeDataService();
                    List<EmployeeHeader> buyingcompany = await Task<List<EmployeeHeader>>.Run(() => sDataService.SearchHeader(new EmployeeCreteria()
                    {
                        standardroleid = 2
                    }));
                    var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.employeecode + " - " + e.name,
                        name = e.name
                    });
                    return this.Ok(list);
                }
                if (id == "EMPLOYONLY")
                {
                    EmployeeDataService sDataService = new EmployeeDataService();
                    List<EmployeeHeader> buyingcompany = await Task<List<EmployeeHeader>>.Run(() => sDataService.SearchHeader(new EmployeeCreteria()
                    {
                        currentEmployees = true
                    }));
                    var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.employeecode + " - " + e.name,
                        name = e.name
                    });
                    return this.Ok(list);
                }
                if (id == "BRAND")
                {
                    BrandDataService sDataService = new BrandDataService();
                    List<Brand> brands = await Task<List<Brand>>.Run(() => sDataService.Search(new BrandCreteria()
                    {

                    }));
                    var list = brands.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        name = e.name,
                        data =e.logoid
                    });
                    return this.Ok(list);
                }
                if (id == "REFILLBOX")
                {
                    RefillboxDataService sDataService = new RefillboxDataService();
                    List<RefillBoxInfo> list = await Task<List<RefillBoxInfo>>.Run(() => sDataService.SearchAll());
                    return this.Ok(list);
                }
                if (id == "PRICEDESIGN")
                {
                    CustomerpriceDataService sDataService = new CustomerpriceDataService();
                    List<Customerprice> list = await Task<List<Customerprice>>.Run(() => sDataService.Search(new CustomerpriceCreteria() { includemfg = true }));
                    return this.Ok(list);
                }
                if (id == "FRANCHISECUSTOMER")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<CustomerLedger> list = await Task<List<CustomerLedger>>.Run(() => cDataService.SearchFranchise());
                    return this.Ok(list.OrderBy(x => x.name).ToList());
                }
                if (id == "RETAILCUSTOMER")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<Customer> list = await Task<List<Customer>>.Run(() => cDataService.Search(new CustomerCreteria()
                    {
                        customertype = 2
                    }));
                    List<Customer> list1 = await Task<List<Customer>>.Run(() => cDataService.Search(new CustomerCreteria()
                    {
                        customertype = 6
                    }));
                    List<Customer> list2 = await Task<List<Customer>>.Run(() => cDataService.Search(new CustomerCreteria()
                    {
                        customertype = 7
                    }));
                    list.AddRange(list1);
                    list.AddRange(list2);
                    return this.Ok(list.OrderBy(x => x.name).ToList());
                }
                if (id == "RETAILCUSTOMERLEDGER")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<CustomerLedger> list = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria()));
                    //List<CustomerLedger> list1 = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria()
                    //{
                    //    customertype = 6
                    //}));
                    //List<CustomerLedger> list2 = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria()
                    //{
                    //    customertype = 7
                    //}));
                    //list.AddRange(list1);
                    //list.AddRange(list2);
                    return this.Ok(list.OrderBy(x => x.name).ToList());
                }
                //if (id == "PERSONS")
                //{
                //    CustomerDataService cDataService = new CustomerDataService();
                //    List<CustomerLedger> list = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria() { customertype = 100 }));
                //    return this.Ok(list);
                //}
                if (id == "CUSTOMERLEDGER")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<CustomerLedger> list = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria()));
                    return this.Ok(list);
                }
                if (id == "CUSTOMERLEDGERUNAPPROVED")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<CustomerLedger> list = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria() { includeunapproved = true }));
                    return this.Ok(list);
                }
                if (id == "MYCUSTOMER")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<CustomerLedger> list = await Task<List<CustomerLedger>>.Run(() => cDataService.CustomerLedger(new CustomerCreteria() { restricted = true }));
                    return this.Ok(list);
                }
                if (id == "CUSTOMER")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<Customer> list = await Task<List<Customer>>.Run(() => cDataService.Search(new CustomerCreteria()));
                    return this.Ok(list.OrderBy(x => x.name).ToList());
                }
                if (id == "CUSTOMERONLY")
                {
                    CustomerDataService cDataService = new CustomerDataService();
                    List<Customer> list = await Task<List<Customer>>.Run(() => cDataService.Search(new CustomerCreteria()));
                    return this.Ok(list.OrderBy(x => x.name).ToList());
                }
                if (id == "MYLOCATION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.SearchMyLocation(new ReferencelistCreteria()));
                    return this.Ok(resultFULL);
                }
                if (id == "MYRETAILLOCATION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.SearchMyRetailLocation(new ReferencelistCreteria()));
                    return this.Ok(resultFULL);
                }

                if (id == "ADDITIONALCHARGES")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.Search(new ReferencelistCreteria()
                    {
                        type = id
                    }));
                    return this.Ok(resultFULL);
                }

                if (id == "PRINTTEMPLATE")
                {
                    ConfigurationDataService configurationDataService = new ConfigurationDataService();
                    var list = configurationDataService.Search(new ConfigurationCreteria()
                    {
                        key = productgroup
                    });
                    var listresponse = list.ConvertAll<ReferenceItem>(e => new ReferenceItem
                    {
                        id = e.id,
                        code = e.name
                    });
                    return this.Ok(listresponse);
                }

                if (id == "FLOOR")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<ReferenceItem> resultFULL = await Task<List<ReferenceItem>>.Run(() => ReferencelistDatafULL.SearchList(new ReferencelistCreteria()
                    {
                        type = id
                    }));

                    return this.Ok(resultFULL);
                }

                if (id == "COUNTER")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.Search(new ReferencelistCreteria()
                    {
                        type = id
                    }));

                    return this.Ok(resultFULL.ConvertAll(e => new CounterReferenceItem()
                    {
                        id = e.id,
                        name = e.name,
                        locationid = e.additionalinfo == null ? 0 : e.additionalinfo.locationid,
                        floorid = e.additionalinfo == null ? 0 : e.additionalinfo.floorid,
                        autosettlement = e.additionalinfo != null && e.additionalinfo.autosettlement,
                        dotmatrixprint = e.additionalinfo != null && e.additionalinfo.disabled,
                        productgroup = e.additionalinfo != null ? e.additionalinfo.printconfigid : 0,
                        online = e.additionalinfo != null ? e.additionalinfo.isonline : false
                    }));
                }

                if (id == "WHLOCATION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.SearchWHLocation(new ReferencelistCreteria()));
                    if (Services.ConfigManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Config.analyser_access_location &&
                        ReferencelistDatafULL.Context.accessiblelocation != null &&
                        ReferencelistDatafULL.Context.accessiblelocation.Count > 0)
                    {
                        if (!ReferencelistDatafULL.Context.accessiblelocation.Exists(e => e == -1))
                        {
                            resultFULL.RemoveAll(e => !ReferencelistDatafULL.Context.accessiblelocation.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(resultFULL);
                }

                if (id == "CUSTOMERCAMPAIGN")
                {
                    ScheduleDataService ReferencelistDatafULL = new ScheduleDataService();
                    List<Schedule> resultFULL = await Task<List<Schedule>>.Run(() => ReferencelistDatafULL.Search(new ScheduleCreteria() { name = "CustomerCampaign" }));
                    return this.Ok(resultFULL);
                }

                if (id == "RETAILLOCATION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<ReferenceItem> resultFULL = await Task<List<ReferenceItem>>.Run(() => ReferencelistDatafULL.SearchRetLocation(new ReferencelistCreteria()));
                    if (Services.ConfigManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Config.analyser_access_location &&
                        ReferencelistDatafULL.Context.accessiblelocation != null && ReferencelistDatafULL.Context.accessiblelocation.Count > 0)
                    {
                        if (!ReferencelistDatafULL.Context.accessiblelocation.Exists(e => e == -1))
                        {
                            resultFULL.RemoveAll(e => !ReferencelistDatafULL.Context.accessiblelocation.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(resultFULL);
                }
                if (id == "ALLLOCATION")
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.Search(new ReferencelistCreteria()
                    {
                        type = "LOCATION"
                    }));

                    return this.Ok(resultFULL);
                }


                if (id.StartsWith("DIRECT-"))
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<CustomReferenceItem> resultFULL = await Task<List<CustomReferenceItem>>.Run(() => ReferencelistDatafULL.Search(new CustomReferenceItem()
                    {
                        type = id.Substring(7),
                        productgroup = lnproductgroup
                    }));
                    if (id.EndsWith("-LOCATION") && ReferencelistDatafULL.Context.accessiblelocation != null &&
                        ReferencelistDatafULL.Context.accessiblelocation.Count > 0)
                    {
                        if (Services.ConfigManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Config.analyser_access_location &&
                            !ReferencelistDatafULL.Context.accessiblelocation.Exists(e => e == -1))
                        {
                            resultFULL.RemoveAll(e => !ReferencelistDatafULL.Context.accessiblelocation.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(resultFULL);
                }
                if (id.StartsWith("LIST-"))
                {
                    if (id == "LIST-CUSTOMER")
                    {
                        CustomerDataService customerDataService = new CustomerDataService();
                        List<CustomerLedger> resultFULL = await Task<List<CustomerLedger>>.Run(() => customerDataService.CustomerLedger(new CustomerCreteria()));
                        return this.Ok(resultFULL.ConvertAll(e => new ReferenceIdName() { id = e.id, name = e.name }));
                    }
                    else if (id == "LIST-SUPPLIER")
                    {
                        SupplierDataService customerDataService = new SupplierDataService();
                        List<SupplierLedger> resultFULL = await Task<List<SupplierLedger>>.Run(() => customerDataService.SupplierLedger(new SupplierCreteria()));
                        return this.Ok(resultFULL.ConvertAll(e => new ReferenceIdName() { id = e.id, name = e.name }));
                    }
                    else if (id == "LIST-PRODUCT")
                    {
                        ProductsDataService customerDataService = new ProductsDataService();
                        long productgroupid = 0;
                        if(!string.IsNullOrEmpty(productgroup))
                        {
                            long.TryParse(productgroup, out productgroupid);
                        }
                        List<ProductsBase> resultFULL = await Task<List<ProductsBase>>.Run(() => customerDataService.SearchList(new ProductsCreteria() { productgroup = productgroupid }));
                        return this.Ok(resultFULL.ConvertAll(e => new ReferenceIdNameParent() { id = e.id, name = e.name , productgroup = e.productgroup  }));
                    }
                    else if (id == "LIST-SUPPLIERCOMPANY")
                    {
                        SuppliercompanyDataService customerDataService = new SuppliercompanyDataService();
                        List<SupplierCompanyHeader> resultFULL = await Task<List<SupplierCompanyHeader>>.Run(() => customerDataService.SearchHeader(new SuppliercompanyCreteria()));
                        return this.Ok(resultFULL.ConvertAll(e => new ReferenceIdName() { id = e.id, name = e.name }));
                    }
                    else
                    {
                        ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                        List<ReferenceItem> resultFULL = await Task<List<ReferenceItem>>.Run(() => ReferencelistDatafULL.SearchList(new ReferencelistCreteria()
                        {
                            type = id.Substring(5),
                            productgroup = lnproductgroup
                        }));
                        if (id.EndsWith("-LOCATION") && ReferencelistDatafULL.Context.accessiblelocation != null && ReferencelistDatafULL.Context.accessiblelocation.Count > 0)
                        {
                            if (Services.ConfigManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Config.analyser_access_location &&
                                !ReferencelistDatafULL.Context.accessiblelocation.Exists(e => e == -1))
                            {
                                resultFULL.RemoveAll(e => !ReferencelistDatafULL.Context.accessiblelocation.Exists(x => x == e.id));
                            }
                        }
                        return this.Ok(resultFULL.ConvertAll(e => new ReferenceIdName() { id = e.id, name = e.name }));
                    }
                }
                if (id.StartsWith("FULL-"))
                {
                    ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService();
                    List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.Search(new ReferencelistCreteria()
                    {
                        type = id.Substring(5),
                        productgroup = lnproductgroup
                    }));
                    if (id.EndsWith("-LOCATION") && ReferencelistDatafULL.Context.accessiblelocation != null && ReferencelistDatafULL.Context.accessiblelocation.Count > 0)
                    {
                        if (Services.ConfigManager.Instance(ReferencelistDatafULL.Context.OrganizationID).Config.analyser_access_location &&
                            !ReferencelistDatafULL.Context.accessiblelocation.Exists(e => e == -1))
                        {
                            resultFULL.RemoveAll(e => !ReferencelistDatafULL.Context.accessiblelocation.Exists(x => x == e.id));
                        }
                    }
                    return this.Ok(resultFULL);
                }

                ReferencelistDataService ReferencelistData = new ReferencelistDataService();
                ReferencelistCreteria creteria = new ReferencelistCreteria();
                creteria.type = id;
                creteria.productgroup = lnproductgroup;
                List<ReferenceItem> result = await Task<List<ReferenceItem>>.Run(() => ReferencelistData.SearchList(creteria));
                if (id == "LOCATION" && ReferencelistData.Context.accessiblelocation != null
                    && ReferencelistData.Context.accessiblelocation.Count > 0)
                {
                    if (Services.ConfigManager.Instance(ReferencelistData.Context.OrganizationID).Config.analyser_access_location &&
                        !ReferencelistData.Context.accessiblelocation.Exists(e => e == -1))
                    {
                        result.RemoveAll(e => !ReferencelistData.Context.accessiblelocation.Exists(x => x == e.id));
                    }
                }
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, id + "/" + productgroup);
                return this.InternalServerError(exp);
            }
        }


        [HttpGet]
        [Route("ReferencelistService/SAAS/{organizationid}/{id}")]
        public async Task<IHttpActionResult> GetSAAS(string organizationid, string id)
        {
            ReferencelistDataService ReferencelistDatafULL = new ReferencelistDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            List<Referencelist> resultFULL = await Task<List<Referencelist>>.Run(() => ReferencelistDatafULL.Search(new ReferencelistCreteria()
            {
                type = id
            }));
            return this.Ok(resultFULL);
        }

        [HttpPost]
        [Route("ReferencelistService/Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ReferencelistCreteria> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            List<Referencelist> result = await Task<List<Referencelist>>.Run(() => ReferencelistData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/DeleteByID")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> DeleteByID(ActionRequest<ReferencelistCreteria> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            List<long> result = await Task<List<long>>.Run(() => ReferencelistData.DeleteByID(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/SearchDirect")]
        public async Task<IHttpActionResult> SearchDirect(ActionRequest<CustomReferenceItem> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            List<CustomReferenceItem> result = await Task<List<CustomReferenceItem>>.Run(() => ReferencelistData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/SaveDirect")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> SaveDirect(ActionRequest<CustomReferenceItem> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            if (request.Item.id <= 0)
            {
                CustomReferenceItem result = await Task<CustomReferenceItem>.Run(() => ReferencelistData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                CustomReferenceItem result = await Task<CustomReferenceItem>.Run(() => ReferencelistData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("ReferencelistService/SaveIncentive")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> SaveIncentive(ActionRequest<ReferenceSectionConfig> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.SaveIncentive(request.Item));
            return this.Ok(result);

        }



        [HttpPost]
        [Route("ReferencelistService/Merge")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Merge(ActionRequest<MergeRefrenceList> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            bool result = await Task<bool>.Run(() => ReferencelistData.Marge(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("ReferencelistService/ConvertLocationToVendors")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ConvertLocationToVendors(ActionRequest<Referencelist> request)
        {
            if (request.Item == null || request.Item.id<=0)
                return this.InternalServerError(new Exception("Please save before proceed"));

            if (request.Item == null || request.Item.additionalinfo == null || request.Item.additionalinfo.parentid<=0)
                return this.InternalServerError(new Exception("Please select the company before proceed"));

            if (string.IsNullOrEmpty(request.Item.additionalinfo.address))
                return this.InternalServerError(new Exception("Please enter the address"));

            if (request.Item.additionalinfo.locationconfig == null||
                request.Item.additionalinfo.locationconfig.cityid <= 0)
                return this.InternalServerError(new Exception("Please select the city"));

            if (request.Item.additionalinfo.locationconfig == null ||
                request.Item.additionalinfo.locationconfig.stateid <= 0)
                return this.InternalServerError(new Exception("Please select the state"));


            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.ConvertLocationToVendors(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/SaveBinStorage")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<BinStorageRefrenceList> request)
        {
            if (request.Item == null || request.Item.referencelocationid <= 0)
                return this.InternalServerError(new Exception("Select the location"));

            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            BinStorageRefrenceList result = await Task<BinStorageRefrenceList>.Run(() => ReferencelistData.SaveBinStorage(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/Save")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            if (request.Item.id <= 0)
            {
                Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("ReferencelistService/InsertAll")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionMulitpleRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            List<Referencelist> result = await Task<List<Referencelist>>.Run(() => ReferencelistData.InsertAll(request.Items));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/Update")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            bool result = await Task<bool>.Run(() => ReferencelistData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ReferencelistService/PreapreCustomField")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> PreapreCustomField(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            bool result = await Task<bool>.Run(() => ReferencelistData.PreapreCustomField(request.Item));
            return this.Ok(result);
        }



        [HttpPost]
        [Route("ReferencelistService/ManageAttribute")]
        [WebAPI.InfyAuthorize(PermissionSet.referencemaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> ManageAttribute(ActionRequest<ManageReference> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.ManageAttribute(request.Item));
            return this.Ok(result);
        }

    }
}