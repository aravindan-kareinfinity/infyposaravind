using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using Npgsql;
using WebAPI.Data;
namespace InfyPOS.Services
{
    public class PromotionManager : IDisposable
    {
        private long organizationid;
        public PromotionManager(long organizationid)
        {
            this.organizationid = organizationid;
            dataservice = new Data.PromotionsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, PromotionManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, PromotionManager>();
        public static PromotionManager Instance(long organizationid)
        {
            PromotionManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new PromotionManager(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;

        }

        public List<Scheme> Schems { get; set; }
        public class Scheme
        {
            public long id { get; set; }
            public InfyPOS.Models.Promotions.Modes mode { get; set; }
            public long locationid { get; set; }
            public InfyPOS.Models.Promotions.ApplicableMode applicablemode { get; set; }
            public List<InfyPOS.Models.Promotions.Rules> rules { get; set; }
            public List<InfyPOS.Models.Promotions.Benefit> benefits { get; set; }
            public DateTime lastmodified { get; set; }
            public bool hasbarcode { get; set; }
            public bool hasinvoiceitem { get; set; }
            public bool hasmarker { get; set; }
            public List<long> excludedpromotionids { get; set; }
            public DateTime validto { get; set; }
            public DateTime validfrom { get; set; }
            public bool enabled { get; set; }
        }
        public class ElibleReason
        {
            public long id { get; set; }
            public bool eligible { get; set; }
            public decimal value { get; set; }
            public DateTime validity { get; set; }
            public Promotions.Rules.RuleMode mode { get; set; }
            public Promotions.Rules.RuleMode exculdemode { get; set; }
            public Promotions.Benefit.BenefitMode benefitmode { get; set; }
            public decimal unit { get; set; }
            public decimal minbillvalue { get; set; }
            public decimal maxdiscountpercentage { get; set; }
            public long locationid { get; set; }
            public bool unlimited { get; set; }
            public long filterschemeid { get; set; }
        }
        System.Threading.ManualResetEvent mre = new System.Threading.ManualResetEvent(true);
        public DateTime lastModified { get; set; }
        public bool Initialized { get; set; }

        internal List<ElibleReason> FindAvailableMatchingSchmes(Common.UserClaim Context, PromotionSearch pitem)
        {
            if (!Initialized)
            {
                LoadSchemes();
            }
            var retvalue = new List<ElibleReason>();
            if (Schems == null || Schems.Count == 0) return retvalue;

            var branchschems = Schems.FindAll(e => (e.locationid == -1 || e.locationid == pitem.locationid)
            && e.enabled && e.validfrom.Date <= Context.DateTimeNow.Date && e.validto.Date >= Context.DateTimeNow.Date);

            if (branchschems.Count == 0) return retvalue;

            Models.Bill bill = new Bill()
            {
                locationid = pitem.locationid,
                companyid = pitem.compayid,
                billdate = Context.DateTimeNow.Date
            };
            bill.Billitems = new List<Billitems>();
            bill.Billitems.Add(new Billitems()
            {
                barcode = pitem.barcode
            });

            SortedDictionary<long, List<string>> barcodeAvilable = FindBarcodeSchemes(bill, branchschems.ConvertAll(e => e.id));
            SortedDictionary<long, List<string>> invoiceitemAvilable = FindInvoiceItemSchemes(bill);
            List<ItemDetail> itemlist = FindItemDetail(bill);
            SortedDictionary<long, List<string>> markeditemlist = FindMarkedBarcodes(bill);
            SortedDictionary<long, List<string>> barcodeExcluededAvilable = FindExcludedBarcodeSchemes(bill, branchschems.ConvertAll(e => e.id));

            foreach (var item in branchschems)
            {
                bill.Billitems.ForEach(e => e.schemeeligble = false);
                Promotions.Rules.RuleMode eligiblemode = Promotions.Rules.RuleMode.none;
                Promotions.Rules.RuleMode excludeemode = Promotions.Rules.RuleMode.none;
                foreach (var rule in item.rules)
                {
                    switch (rule.mode)
                    {
                        case Models.Promotions.Rules.RuleMode.attributes:
                            var availableItems = itemlist.FindAll(e => (e.brandid == rule.item.brandid || rule.item.brandid == 0) &&
                            (e.productid == rule.item.productid || rule.item.productid == 0) &&
                            (e.colourid == rule.item.colourid || rule.item.colourid == 0) &&
                            (e.materialid == rule.item.materialid || rule.item.materialid == 0) &&
                            (e.typeid == rule.item.typeid || rule.item.typeid == 0) &&
                            (e.patternid == rule.item.patternid || rule.item.patternid == 0) &&
                            (e.designid == rule.item.designid || string.IsNullOrEmpty(rule.item.designid)) &&
                            (e.styleid == rule.item.styleid || rule.item.styleid == 0));

                            bill.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                            {
                                e.schemeeligble = true;
                                eligiblemode = Promotions.Rules.RuleMode.attributes;
                            });
                            break;
                        case Models.Promotions.Rules.RuleMode.invoiceitem:

                            if (rule.invoiceitemid != null && rule.invoiceitemid.Count > 0 &&
                                invoiceitemAvilable.Keys.ToList().Exists(e => rule.invoiceitemid.Contains(e)))
                            {
                                foreach (var itm in rule.invoiceitemid)
                                {
                                    if (invoiceitemAvilable.ContainsKey(itm))
                                    {
                                        var barcodelist = invoiceitemAvilable[itm];

                                        bill.Billitems.FindAll(e => barcodelist.Contains(e.barcode)).ForEach(e =>
                                        {
                                            e.schemeeligble = true;
                                            eligiblemode = Promotions.Rules.RuleMode.invoiceitem;
                                        });
                                    }
                                }
                            }

                            break;
                        case Models.Promotions.Rules.RuleMode.marker:
                            if (markeditemlist.ContainsKey(rule.item.markerid))
                            {
                                bill.Billitems.FindAll(e => markeditemlist[rule.item.markerid].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                                {
                                    e.schemeeligble = true;
                                    eligiblemode = Promotions.Rules.RuleMode.marker;
                                });
                            }
                            break;
                    }
                }

                if (barcodeAvilable.ContainsKey(item.id))
                {
                    bill.Billitems.FindAll(e => barcodeAvilable[item.id].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                    {
                        e.schemeeligble = true;
                        eligiblemode = Promotions.Rules.RuleMode.barcode;
                    });
                }

                if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludediscount))
                {
                    var excludediscount = item.rules.Find(e => e.mode == Promotions.Rules.RuleMode.excludediscount).excludediscount;
                    foreach (var bitem in bill.Billitems.FindAll(e => e.schemeid == 0 && e.schemeeligble))
                    {
                        if (Math.Round((bitem.price + bitem.discount) * excludediscount / 100, 2) < bitem.discount)
                        {
                            bitem.schemeeligble = false;
                            excludeemode = Promotions.Rules.RuleMode.excludediscount;
                        }
                    }
                }

                if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludemarker))
                {
                    foreach (var ritem in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludemarker))
                    {

                        if (markeditemlist.ContainsKey(ritem.item.markerid))
                        {
                            bill.Billitems.FindAll(e => markeditemlist[ritem.item.markerid].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                            {
                                e.schemeeligble = false;
                                excludeemode = Promotions.Rules.RuleMode.excludemarker;
                            });
                        }

                    }
                }

                if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
                {
                    foreach (var ritem in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
                    {

                        var availableItems = itemlist.FindAll(e => (e.brandid == ritem.item.brandid || ritem.item.brandid == 0) &&
                           (e.productid == ritem.item.productid || ritem.item.productid == 0) &&
                           (e.colourid == ritem.item.colourid || ritem.item.colourid == 0) &&
                           (e.materialid == ritem.item.materialid || ritem.item.materialid == 0) &&
                           (e.typeid == ritem.item.typeid || ritem.item.typeid == 0) &&
                           (e.patternid == ritem.item.patternid || ritem.item.patternid == 0) &&
                           (e.designid == ritem.item.designid || string.IsNullOrEmpty(ritem.item.designid)) &&
                           (e.styleid == ritem.item.styleid || ritem.item.styleid == 0));

                        bill.Billitems.FindAll(e => e.schemeid == 0 && availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                        {
                            e.schemeeligble = false;
                            excludeemode = Promotions.Rules.RuleMode.excludeattribute;
                        });

                    }
                }

                if (barcodeExcluededAvilable.ContainsKey(item.id))
                {
                    bill.Billitems.FindAll(e => e.schemeid == 0 && barcodeExcluededAvilable[item.id].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                    {
                        e.schemeeligble = false;
                        excludeemode = Promotions.Rules.RuleMode.excludebarcode;
                    });
                }

                if (eligiblemode != Promotions.Rules.RuleMode.none || excludeemode != Promotions.Rules.RuleMode.none)
                    retvalue.Add(new ElibleReason()
                    {
                        id = item.id,
                        eligible = bill.Billitems.Exists(e => e.schemeeligble),
                        mode = eligiblemode,
                        exculdemode = excludeemode
                    });

            }
            return retvalue;
        }

        internal decimal SchemedBillValue(long filterschemeid, List<Bill> list)
        {
            if (!Initialized)
            {
                LoadSchemes();
            }
            if (Schems == null || Schems.Count == 0 || !Schems.Exists(e => e.id == filterschemeid)) return list.Sum(e => e.receivable);
            return SchemedBillValue(Schems.Find(e => e.id == filterschemeid), list);
        }

        //Npgsql.NpgsqlConnection notificationConnection;
        Data.PromotionsDataService dataservice = null;
        public void LoadSchemes()
        {
            if (mre.WaitOne(100))
            {
                mre.Reset();
                if (!Initialized)
                {
                    try
                    {
                        var list = dataservice.Search(new Models.PromotionsCreteria());
                        Schems = new List<Scheme>();
                        foreach (var item in list.FindAll(e => e.enabled))
                        {
                            Schems.Add(CreateScheme(dataservice.Search(new PromotionsCreteria()
                            {
                                id = item.id
                            }).First()));
                        }
                    }
                    catch (Exception exp)
                    {
                        WebAPI.Common.Logger.Instance(organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, "Load Schemes");
                    }
                    InfyPOS.Processors.CoreDBListener.Instance(organizationid).Initialize();
                    InfyPOS.Processors.CoreDBListener.Instance(organizationid).OnPromotionChange += Instance_OnPromotionChange;

                    //string connectionstring = System.Configuration.ConfigurationManager.ConnectionStrings["InfyPOS"].ConnectionString + ";SyncNotification=true";
                    //this.notificationConnection = new Npgsql.NpgsqlConnection(connectionstring);
                    //this.notificationConnection.Open();
                    //if (this.notificationConnection.State == System.Data.ConnectionState.Open)
                    //{
                    //    using (var command = new Npgsql.NpgsqlCommand("listen promochange", this.notificationConnection))
                    //    {
                    //        command.ExecuteNonQuery();
                    //    }
                    //    this.notificationConnection.Notification += PromoChanged;
                    //}
                    Initialized = true;
                }
                mre.Set();
            }
        }

        public decimal SchemedBillValue(Scheme item, List<Bill> bills)
        {
            SortedDictionary<long, List<string>> barcodeAvilable = new SortedDictionary<long, List<string>>();
            SortedDictionary<long, List<string>> barcodeExcluededAvilable = new SortedDictionary<long, List<string>>();
            SortedDictionary<long, List<string>> invoiceitemAvilable = new SortedDictionary<long, List<string>>();
            SortedDictionary<long, List<string>> markeditemlist = new SortedDictionary<long, List<string>>();
            List<ItemDetail> itemlist = new List<ItemDetail>();
            foreach (var bill in bills)
            {
                bill.Billitems.ForEach(e => e.schemeeligble = false);
                var tbarcodeAvilable = FindBarcodeSchemes(bill, new List<long>(new long[] { item.id }));
                tbarcodeAvilable.ToList().ForEach(e => { if (!barcodeAvilable.ContainsKey(e.Key)) barcodeAvilable.Add(e.Key, e.Value); });
                var tbarcodeExcluededAvilable = FindExcludedBarcodeSchemes(bill, new List<long>(new long[] { item.id }));
                tbarcodeExcluededAvilable.ToList().ForEach(e => { if (!barcodeExcluededAvilable.ContainsKey(e.Key)) barcodeExcluededAvilable.Add(e.Key, e.Value); });
                var tinvoiceitemAvilable = FindInvoiceItemSchemes(bill);
                tinvoiceitemAvilable.ToList().ForEach(e => { if (!invoiceitemAvilable.ContainsKey(e.Key)) invoiceitemAvilable.Add(e.Key, e.Value); });
                var tmarkeditemlist = FindMarkedBarcodes(bill);
                tmarkeditemlist.ToList().ForEach(e => { if (!markeditemlist.ContainsKey(e.Key)) markeditemlist.Add(e.Key, e.Value); });
                var titemlist = FindItemDetail(bill);
                titemlist.ForEach(e =>
                {
                    //if (!itemlist.Exists(itm => itm.id == e.id))
                    itemlist.Add(e);
                });
            }

            foreach (var rule in item.rules.Where(e => e.mode != Promotions.Rules.RuleMode.excludediscount &&
                e.mode != Promotions.Rules.RuleMode.excludebarcode && e.mode != Promotions.Rules.RuleMode.excludemarker &&
                e.mode != Promotions.Rules.RuleMode.excludeattribute))
            {
                switch (rule.mode)
                {
                    case Models.Promotions.Rules.RuleMode.attributes:
                        var availableItems = itemlist.FindAll(e => (e.brandid == rule.item.brandid || rule.item.brandid == 0) &&
                        (e.productid == rule.item.productid || rule.item.productid == 0) &&
                        (e.colourid == rule.item.colourid || rule.item.colourid == 0) &&
                        (e.materialid == rule.item.materialid || rule.item.materialid == 0) &&
                        (e.typeid == rule.item.typeid || rule.item.typeid == 0) &&
                        (e.patternid == rule.item.patternid || rule.item.patternid == 0) &&
                        (e.designid == rule.item.designid || string.IsNullOrEmpty(rule.item.designid)) &&
                        (e.styleid == rule.item.styleid || rule.item.styleid == 0));

                        bills.ForEach(b => b.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                          {
                              e.schemeeligble = true;
                          }));

                        break;
                    case Models.Promotions.Rules.RuleMode.invoiceitem:
                        if (rule.invoiceitemid != null && rule.invoiceitemid.Count > 0 &&
                           invoiceitemAvilable.Keys.ToList().Exists(e => rule.invoiceitemid.Contains(e)))
                        {
                            foreach (var itm in rule.invoiceitemid)
                            {
                                if (invoiceitemAvilable.ContainsKey(itm))
                                {
                                    var barcodelist = invoiceitemAvilable[itm];

                                    bills.ForEach(b => b.Billitems.FindAll(e => barcodelist.Contains(e.barcode)).ForEach(e =>
                                    {
                                        e.schemeeligble = true;
                                    }));
                                }
                            }
                        }
                        break;
                    case Models.Promotions.Rules.RuleMode.marker:
                        if (markeditemlist.ContainsKey(rule.item.markerid))
                        {
                            bills.ForEach(b => b.Billitems.FindAll(e => markeditemlist[rule.item.markerid].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                            {
                                e.schemeeligble = true;
                            }));
                        }
                        break;
                    case Models.Promotions.Rules.RuleMode.billvalue:
                        if (item.mode == Promotions.Modes.BillItem)
                        {
                            bills.ForEach(b => b.Billitems.FindAll(e => (e.receivable >= rule.ValueFrom && e.receivable <= rule.ValueTo)).ForEach(e =>
                            {
                                e.schemeeligble = true;
                            }));
                        }
                        break;

                }
            }

            if (barcodeAvilable.ContainsKey(item.id))
            {
                bills.ForEach(b => b.Billitems.FindAll(e => barcodeAvilable[item.id].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                {
                    e.schemeeligble = true;
                }));
            }

            if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludediscount))
            {
                var excludediscount = item.rules.Find(e => e.mode == Promotions.Rules.RuleMode.excludediscount).excludediscount;
                bills.ForEach(b =>
                {
                    foreach (var bitem in b.Billitems.FindAll(e => e.schemeeligble))
                    {
                        if (Math.Round((bitem.price + (bitem.discount / bitem.qty)) * excludediscount / 100, 2) < (bitem.discount / bitem.qty))
                            bitem.schemeeligble = false;
                    }
                });
            }

            if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludemarker))
            {
                foreach (var ritem in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludemarker))
                {

                    if (markeditemlist.ContainsKey(ritem.item.markerid))
                    {
                        bills.ForEach(b => b.Billitems.FindAll(e => markeditemlist[ritem.item.markerid].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                        {
                            e.schemeeligble = false;
                        }));
                    }

                }
            }

            if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
            {
                foreach (var ritem in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
                {
                    var availableItems = itemlist.FindAll(e => (e.brandid == ritem.item.brandid || ritem.item.brandid == 0) &&
                       (e.productid == ritem.item.productid || ritem.item.productid == 0) &&
                       (e.colourid == ritem.item.colourid || ritem.item.colourid == 0) &&
                       (e.materialid == ritem.item.materialid || ritem.item.materialid == 0) &&
                       (e.typeid == ritem.item.typeid || ritem.item.typeid == 0) &&
                       (e.patternid == ritem.item.patternid || ritem.item.patternid == 0) &&
                       (e.designid == ritem.item.designid || string.IsNullOrEmpty(ritem.item.designid)) &&
                       (e.styleid == ritem.item.styleid || ritem.item.styleid == 0));

                    bills.ForEach(b => b.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                     {
                         e.schemeeligble = false;
                     }));
                }
            }
            if (item.benefits.Exists(e => e.includediscount))
            {
                return bills.Sum(b => b.Billitems.Sum(bi => bi.schemeeligble ? bi.receivable + bi.discount : 0));
            }
            else
            {
                return bills.Sum(b => b.Billitems.Sum(bi => bi.schemeeligble ? bi.receivable : 0));
            }
        }

        public Models.Bill Process(Models.Bill bill)
        {
            if (bill.billdate.Date == DateTime.MinValue)
                bill.billdate = DateTime.Now;
            bill.HasPendingPromotion = false;
            if (!Initialized)
            {
                LoadSchemes();
            }
            if (Schems == null || Schems.Count == 0) return bill;
            bill.removescheme();
            var branchschems = Schems.FindAll(e => e.mode == Promotions.Modes.BillItem && (e.locationid == -1 || e.locationid == bill.locationid)
            && e.enabled && e.validfrom.Date <= bill.billdate.Date && e.validto.Date.AddDays(1) > bill.billdate.Date);
            List<Models.Bill> afterdiscount = new List<Models.Bill>();
            if (branchschems.Count > 0)
            {
                SortedDictionary<long, List<string>> barcodeAvilable = FindBarcodeSchemes(bill, branchschems.ConvertAll(e => e.id));
                SortedDictionary<long, List<string>> barcodeExcluededAvilable = FindExcludedBarcodeSchemes(bill, branchschems.ConvertAll(e => e.id));
                SortedDictionary<long, List<string>> invoiceitemAvilable = FindInvoiceItemSchemes(bill);
                List<ItemDetail> itemlist = FindItemDetail(bill);
                SortedDictionary<long, List<string>> markeditemlist = FindMarkedBarcodes(bill);
                foreach (var item in branchschems)
                {
                    var newbill = bill.ToJSON().Json2Object<Models.Bill>();

                    if (item.rules != null && item.rules.Count > 0 &&
                        item.rules.All(e => e.mode == Promotions.Rules.RuleMode.excludeattribute
                        || e.mode == Promotions.Rules.RuleMode.excludebarcode
                        || e.mode == Promotions.Rules.RuleMode.excludemarker))
                    {
                        newbill.Billitems.ForEach(e => e.schemeeligble = true);
                    }

                    foreach (var rule in item.rules.Where(e => e.mode != Promotions.Rules.RuleMode.excludediscount &&
                    e.mode != Promotions.Rules.RuleMode.excludebarcode && e.mode != Promotions.Rules.RuleMode.excludemarker &&
                    e.mode != Promotions.Rules.RuleMode.excludeattribute))
                    {
                        switch (rule.mode)
                        {
                            case Models.Promotions.Rules.RuleMode.attributes:
                                var availableItems = itemlist.FindAll(e => (e.brandid == rule.item.brandid || rule.item.brandid == 0) &&
                                (e.productid == rule.item.productid || rule.item.productid == 0) &&
                                (e.colourid == rule.item.colourid || rule.item.colourid == 0) &&
                                (e.materialid == rule.item.materialid || rule.item.materialid == 0) &&
                                (e.typeid == rule.item.typeid || rule.item.typeid == 0) &&
                                (e.patternid == rule.item.patternid || rule.item.patternid == 0) &&
                                (e.designid == rule.item.designid || string.IsNullOrEmpty(rule.item.designid)) &&
                                (e.styleid == rule.item.styleid || rule.item.styleid == 0));

                                newbill.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                                  {
                                      e.schemeeligble = true;
                                  });
                                break;
                            case Models.Promotions.Rules.RuleMode.invoiceitem:
                                if (rule.invoiceitemid != null && rule.invoiceitemid.Count > 0 &&
                                   invoiceitemAvilable.Keys.ToList().Exists(e => rule.invoiceitemid.Contains(e)))
                                {
                                    foreach (var itm in rule.invoiceitemid)
                                    {
                                        if (invoiceitemAvilable.ContainsKey(itm))
                                        {
                                            var barcodelist = invoiceitemAvilable[itm];

                                            bill.Billitems.FindAll(e => barcodelist.Contains(e.barcode)).ForEach(e =>
                                            {
                                                e.schemeeligble = true;
                                            });
                                        }
                                    }
                                }
                                break;
                            case Models.Promotions.Rules.RuleMode.marker:
                                if (markeditemlist.ContainsKey(rule.item.markerid))
                                {
                                    newbill.Billitems.FindAll(e => markeditemlist[rule.item.markerid].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                                    {
                                        e.schemeeligble = true;
                                    });
                                }
                                break;
                            case Models.Promotions.Rules.RuleMode.billvalue:
                                newbill.Billitems.FindAll(e => (e.receivable >= rule.ValueFrom && e.receivable <= rule.ValueTo)).ForEach(e =>
                                {
                                    e.schemeeligble = true;
                                });
                                break;

                        }
                    }

                    if (barcodeAvilable.ContainsKey(item.id))
                    {
                        newbill.Billitems.FindAll(e => barcodeAvilable[item.id].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                        {
                            e.schemeeligble = true;
                        });
                    }

                    if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludediscount))
                    {
                        var excludediscount = item.rules.Find(e => e.mode == Promotions.Rules.RuleMode.excludediscount).excludediscount;
                        //if (excludediscount == 0)
                        //    excludediscount = 20;
                        foreach (var bitem in newbill.Billitems.FindAll(e => e.schemeeligble))
                        {
                            if (Math.Round((bitem.price + (bitem.discount / bitem.qty)) * excludediscount / 100, 2) < (bitem.discount / bitem.qty))
                                bitem.schemeeligble = false;
                        }
                    }

                    if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludemarker))
                    {
                        foreach (var ritem in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludemarker))
                        {

                            if (markeditemlist.ContainsKey(ritem.item.markerid))
                            {
                                bill.Billitems.FindAll(e => markeditemlist[ritem.item.markerid].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                                {
                                    e.schemeeligble = false;
                                });
                            }

                        }
                    }

                    if (item.rules.Exists(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
                    {
                        foreach (var ritem in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.excludeattribute))
                        {

                            var availableItems = itemlist.FindAll(e => (e.brandid == ritem.item.brandid || ritem.item.brandid == 0) &&
                               (e.productid == ritem.item.productid || ritem.item.productid == 0) &&
                               (e.colourid == ritem.item.colourid || ritem.item.colourid == 0) &&
                               (e.materialid == ritem.item.materialid || ritem.item.materialid == 0) &&
                               (e.typeid == ritem.item.typeid || ritem.item.typeid == 0) &&
                               (e.patternid == ritem.item.patternid || ritem.item.patternid == 0) &&
                               (e.designid == ritem.item.designid || string.IsNullOrEmpty(ritem.item.designid)) &&
                               (e.styleid == ritem.item.styleid || ritem.item.styleid == 0));

                            newbill.Billitems.FindAll(e => availableItems.Exists(x => x.barcode == e.barcode)).ForEach(e =>
                            {
                                e.schemeeligble = false;
                            });

                        }
                    }

                    if (barcodeExcluededAvilable.ContainsKey(item.id))
                    {
                        newbill.Billitems.FindAll(e => barcodeExcluededAvilable[item.id].Exists(x => x.Contains(e.barcode))).ForEach(e =>
                        {
                            e.schemeeligble = false;
                        });
                    }

                    var eligbleItems = newbill.Billitems.FindAll(e => e.schemeeligble);
                    if (eligbleItems.Count == 0) continue;

                    afterdiscount.Add(newbill);
                    foreach (var benefit in item.benefits.OrderBy(e => e.priority))
                    {
                        var pricerange = eligbleItems.FindAll(e => e.schemeid == 0).FindAll(e => (benefit.pricefrom == 0 && benefit.priceto == 0) || (e.price + e.discount >= benefit.pricefrom && e.price + e.discount <= benefit.priceto)).OrderBy(e => e.price).ToList();
                        if (pricerange.Sum(e => e.qty) == 0) break;

                        switch (benefit.mode)
                        {
                            case Models.Promotions.Benefit.BenefitMode.DiscountInQty:
                                if (benefit.similarprice)
                                {
                                    foreach (var pricegroups in pricerange.GroupBy(e => e.price))
                                    {
                                        if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricegroups.Sum(e => e.qty) &&
                                        (benefit.qtyto == 0 || benefit.qtyto >= pricegroups.Sum(e => e.qty))))
                                        {
                                            var freeqty = (benefit.qtyto == 0) ? ((pricegroups.Sum(e => e.qty) / benefit.qtyfrom) * benefit.value) : benefit.value;
                                            if (Math.Ceiling(benefit.value) == Math.Floor(benefit.value))
                                                freeqty = Math.Floor(freeqty);

                                            foreach (var bitem in pricegroups)
                                            {
                                                if (bitem.qty <= freeqty)
                                                {
                                                    bitem.schemeid = item.id;
                                                    if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                        bitem.schemediscount = (bitem.getactualreceivable());
                                                    else
                                                        bitem.schemediscountcoupen = (bitem.getactualreceivable());
                                                    freeqty -= bitem.qty;
                                                }
                                                else
                                                {
                                                    bitem.schemeid = item.id;
                                                    if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                        bitem.schemediscount = Math.Round(((bitem.getactualreceivable()) / bitem.qty) * freeqty, 2);
                                                    else
                                                        bitem.schemediscountcoupen = Math.Round(((bitem.getactualreceivable()) / bitem.qty) * freeqty, 2);
                                                    freeqty = 0;
                                                }

                                                if (freeqty == 0)
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                        (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                    {
                                        var freeqty = (benefit.qtyto == 0) ? ((pricerange.Sum(e => e.qty) / benefit.qtyfrom) * benefit.value) : benefit.value;
                                        if (Math.Ceiling(benefit.value) == Math.Floor(benefit.value))
                                            freeqty = Math.Floor(freeqty);

                                        foreach (var bitem in pricerange)
                                        {
                                            if (bitem.qty <= freeqty)
                                            {
                                                bitem.schemeid = item.id;
                                                if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                    bitem.schemediscount = (bitem.getactualreceivable());
                                                else
                                                    bitem.schemediscountcoupen = (bitem.getactualreceivable());
                                                freeqty -= bitem.qty;
                                            }
                                            else
                                            {
                                                bitem.schemeid = item.id;
                                                if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                    bitem.schemediscount = Math.Round(((bitem.getactualreceivable()) / bitem.qty) * freeqty, 2);
                                                else
                                                    bitem.schemediscountcoupen = Math.Round(((bitem.getactualreceivable()) / bitem.qty) * freeqty, 2);
                                                freeqty = 0;
                                            }

                                            if (freeqty == 0)
                                                break;
                                        }
                                    }
                                }
                                break;
                            case Models.Promotions.Benefit.BenefitMode.DiscountItem:
                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    var freeqty = benefit.unit;
                                    var totqty = pricerange.Sum(e => e.qty);
                                    if (benefit.qtyto > 1)
                                    {
                                        totqty = Math.Floor(totqty / benefit.qtyto) * freeqty;
                                    }
                                    else
                                    {
                                        totqty = Math.Floor(totqty / benefit.qtyfrom) * freeqty;
                                    }
                                    var matchedItems = itemlist.FindAll(e => (e.productid == benefit.item.productid || benefit.item.productid == 0) &&
                                    (e.brandid == benefit.item.brandid || benefit.item.brandid == 0) &&
                                    (e.colourid == benefit.item.colourid || benefit.item.colourid == 0) &&
                                    (e.materialid == benefit.item.materialid || benefit.item.materialid == 0) &&
                                    (e.patternid == benefit.item.patternid || benefit.item.patternid == 0) &&
                                    (e.styleid == benefit.item.styleid || benefit.item.styleid == 0) &&
                                    (e.typeid == benefit.item.typeid || benefit.item.typeid == 0));

                                    matchedItems = matchedItems.FindAll(e => !pricerange.Exists(p => p.barcode == e.barcode));

                                    var discountitemlist = bill.Billitems.Where(e => matchedItems.Exists(x => x.barcode == e.barcode)).ToList();
                                    //discountitemlist = discountitemlist.ToJSON().Json2Object<List<Billitems>>();

                                    //decimal negativeValue = 0;
                                    //decimal adjustedqty = 0;
                                    if (matchedItems.Count > 0)
                                    {
                                        var discountqty = discountitemlist.Sum(e => e.qty);
                                        {
                                            var acceptableqty = totqty <= discountqty ? totqty : discountqty;
                                            discountitemlist.OrderBy(e => e.price).ToList().ForEach(e =>
                                              {
                                                  if (acceptableqty > 0)
                                                  {
                                                      if (e.qty <= acceptableqty)
                                                      {
                                                          acceptableqty -= e.qty;
                                                          e.schemediscount = e.receivable;
                                                          e.schemeid = item.id;
                                                          e.tax = 0;
                                                          e.taxpercentage = 0;
                                                      }
                                                      else if (acceptableqty > 0)
                                                      {
                                                          e.tax = Math.Round(e.tax / e.qty * acceptableqty, 4);
                                                          e.schemediscount = (e.receivable / e.qty * acceptableqty) - (e.discount / e.qty * acceptableqty);
                                                          acceptableqty = 0;
                                                          e.schemeid = item.id;
                                                      }
                                                  }
                                              });


                                            //var adjustprice = ((benefit.value * totqty) - pricerange.Sum(e => e.qty * e.price)) / totqty;
                                            //foreach (var bitem in matchedItems)
                                            //{
                                            //    var billitem = newbill.Billitems.Find(e => e.barcode == bitem.barcode && e.schemeid == 0);
                                            //    if (billitem.qty <= totqty)
                                            //    {
                                            //        if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                            //            billitem.schemediscount = billitem.qty * (billitem.price - adjustprice);
                                            //        else
                                            //            billitem.schemediscountcoupen = billitem.qty * (billitem.price - adjustprice);
                                            //        totqty -= billitem.qty;
                                            //        adjustedqty += billitem.qty;
                                            //        if ((billitem.price - billitem.additionaldiscount) * billitem.qty < billitem.schemediscount)
                                            //        {
                                            //            var newschemediscount = (billitem.schemediscount - ((billitem.price - billitem.additionaldiscount) * billitem.qty)) + billitem.qty;
                                            //            negativeValue += newschemediscount;
                                            //            billitem.schemediscount = billitem.schemediscount - newschemediscount;
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                            //            billitem.schemediscount = Math.Round((billitem.price - adjustprice) * totqty, 0);
                                            //        else
                                            //            billitem.schemediscountcoupen = Math.Round((billitem.price - adjustprice) * totqty, 0);
                                            //        if ((billitem.price - billitem.additionaldiscount) * billitem.qty < billitem.schemediscount)
                                            //        {
                                            //            var newschemediscount = (billitem.schemediscount - ((billitem.price - billitem.additionaldiscount) * totqty)) + totqty;
                                            //            negativeValue += newschemediscount;
                                            //            billitem.schemediscount = billitem.schemediscount - newschemediscount;
                                            //        }
                                            //        adjustedqty += totqty;
                                            //        totqty = 0;
                                            //    }
                                            //    billitem.schemeid = item.id;
                                            //    if (totqty == 0)
                                            //        break;
                                            //}

                                            //if (negativeValue > 0)
                                            //{
                                            //    adjustprice = negativeValue / pricerange.Sum(e => e.qty * e.price);
                                            //    foreach (var bitem in pricerange)
                                            //    {
                                            //        bitem.schemediscount += Math.Round((bitem.qty * bitem.price) * adjustprice, 0);
                                            //        bitem.schemeid = item.id;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    foreach (var bitem in pricerange)
                                            //    {
                                            //        bitem.schemediscount += Math.Round((bitem.qty * bitem.price) * adjustprice, 0);
                                            //        bitem.schemeid = item.id;
                                            //    }
                                            //}
                                        }
                                    }
                                }
                                break;


                            case Models.Promotions.Benefit.BenefitMode.Free:
                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    var freeqty = benefit.unit;
                                    var totqty = pricerange.Sum(e => e.qty);
                                    if (benefit.qtyto > 1)
                                    {
                                        totqty = Math.Floor(totqty / benefit.qtyto);
                                    }

                                    Data.ItemsDataService itemsDataService = new Data.ItemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                                    var existingitems = itemsDataService.Search(new ItemsCreteria()
                                    {
                                        productmap = true,
                                        productid = benefit.item.productid,
                                        brandid = benefit.item.brandid,
                                        typeid = benefit.item.typeid,
                                        styleid = benefit.item.styleid,
                                        materialid = benefit.item.materialid,
                                        patternid = benefit.item.patternid
                                    });
                                    if (pricerange.First().attributes == null)
                                        pricerange.First().attributes = new BillItemAttributes();
                                    if (pricerange.First().attributes.freeitems == null)
                                        pricerange.First().attributes.freeitems = new List<BillItemAttributes.FreeItems>();

                                    if (existingitems.Count == 0)
                                    {
                                        Data.ProductsDataService productsDataService = new Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                                        var products = productsDataService.Search(new ProductsCreteria()
                                        {
                                            id = benefit.item.productid
                                        });

                                        existingitems.Add(itemsDataService.Insert(new Items()
                                        {
                                            printingname = products.Count > 0 ? products.First().name : "UNKNOWN",
                                            productname = products.Count > 0 ? products.First().name : "UNKNOWN",
                                            productid = benefit.item.productid,
                                            brandid = benefit.item.brandid,
                                            typeid = benefit.item.typeid,
                                            styleid = benefit.item.styleid,
                                            materialid = benefit.item.materialid,
                                            patternid = benefit.item.patternid
                                        }));
                                    }

                                    pricerange.OrderBy(e => e.receivable).Last().attributes.freeitems.Add(new BillItemAttributes.FreeItems()
                                    {
                                        itemid = existingitems[0].id,
                                        amount = benefit.value * totqty,
                                        qty = freeqty * totqty,
                                        productid = benefit.item.productid,
                                        name = existingitems[0].printingname
                                    });

                                }
                                break;
                            case Models.Promotions.Benefit.BenefitMode.Combo:

                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    var totalValue = benefit.value;
                                    if (benefit.qtyto == 0)
                                    {
                                        var maxqty = Math.Floor(pricerange.Sum(e => e.qty) / benefit.qtyfrom);
                                        totalValue = benefit.value * maxqty;
                                        List<Billitems> removelist = new List<Billitems>();
                                        if (maxqty * benefit.qtyfrom != pricerange.Sum(e => e.qty))
                                        {
                                            var remqty = pricerange.Sum(e => e.qty) - (maxqty * benefit.qtyfrom);
                                            foreach (var pitem in pricerange.OrderByDescending(e => e.price))
                                            {
                                                if (remqty > pitem.qty)
                                                {
                                                    remqty -= pitem.qty;
                                                    removelist.Add(pitem);
                                                }
                                                else
                                                {
                                                    totalValue += (pitem.receivable / pitem.qty) * remqty;
                                                    remqty = 0;
                                                }
                                                if (remqty == 0)
                                                    break;
                                            }
                                            foreach (var pitem in removelist)
                                                pricerange.Remove(pitem);
                                        }
                                    }
                                    var currentValue = pricerange.Sum(e => e.receivable - e.schemediscount);
                                    var discountpercent = (currentValue - totalValue) / currentValue * 100;
                                    if (discountpercent > 0)
                                    {
                                        foreach (var bitem in pricerange)
                                        {
                                            bitem.schemeid = item.id;
                                            var receivable = Math.Round(totalValue * ((bitem.receivable - bitem.schemediscount) / currentValue * 100) / 100, 2);
                                            if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                bitem.schemediscount = (bitem.receivable - bitem.schemediscount) - receivable;
                                            else
                                                bitem.schemediscountcoupen = (bitem.receivable - bitem.schemediscount) - receivable;
                                        }
                                    }
                                    var newtotalvalue = pricerange.Sum(e => e.schemediscount);
                                    var diffvalue = totalValue - (currentValue - newtotalvalue);
                                    if (diffvalue != 0)
                                    {
                                        if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                            pricerange.Last().schemediscount += (diffvalue * -1);
                                    }
                                }
                                break;
                            case Models.Promotions.Benefit.BenefitMode.FlatPrice:
                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    var totalValue = benefit.value;
                                    foreach (var bitem in pricerange)
                                    {
                                        bitem.schemeid = item.id;
                                        var newreceivable = bitem.qty * benefit.value;
                                        if ((bitem.receivable - bitem.schemediscount) > newreceivable)
                                        {
                                            bitem.schemediscount = (bitem.receivable - bitem.schemediscount) - newreceivable;
                                        }
                                    }
                                }
                                break;
                            case Models.Promotions.Benefit.BenefitMode.DiscountInFixedValue:
                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    var fixeddiscount = benefit.value;
                                    foreach (var bitem in pricerange)
                                    {
                                        bitem.schemeid = item.id;
                                        if (fixeddiscount > (benefit.includediscount ? bitem.discount : 0))
                                        {
                                            if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                bitem.schemediscount = Math.Round(fixeddiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                                            else
                                                bitem.schemediscountcoupen = Math.Round(fixeddiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                                        }
                                    }
                                }
                                break;
                            case Models.Promotions.Benefit.BenefitMode.DiscountInPercentage:
                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    if (benefit.applicableqty > 0)
                                    {
                                        var applicableqty = benefit.applicableqty;
                                        foreach (var bitem in pricerange)
                                        {
                                            if (applicableqty >= bitem.qty)
                                            {
                                                applicableqty -= bitem.qty;
                                                var tdiscount = Math.Round(((bitem.price + (bitem.discount / bitem.qty)) * benefit.value / 100) * bitem.qty, 2);
                                                if (benefit.discountondiscount)
                                                    tdiscount = Math.Round(((bitem.price) * benefit.value / 100) * bitem.qty, 2);

                                                bitem.schemeid = item.id;
                                                if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                    bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? (bitem.discount / bitem.qty) : 0), 2);
                                                else
                                                    bitem.schemediscountcoupen = Math.Round(tdiscount - (benefit.includediscount ? (bitem.discount / bitem.qty) : 0), 2);
                                            }
                                            else
                                            {
                                                var tdiscount = Math.Round(((bitem.price + (bitem.discount / bitem.qty)) * benefit.value / 100) * applicableqty, 2);
                                                if (benefit.discountondiscount)
                                                    tdiscount = Math.Round(((bitem.price) * benefit.value / 100) * applicableqty, 2);

                                                bitem.schemeid = item.id;
                                                if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                    bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                                                else
                                                    bitem.schemediscountcoupen = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                                                applicableqty = 0;
                                            }
                                            if (applicableqty == 0) break;
                                        }
                                        if (benefit.qtyto > 0)
                                        {
                                            applicableqty = benefit.qtyto;
                                            foreach (var bitem in pricerange)
                                            {
                                                if (applicableqty >= bitem.qty)
                                                {
                                                    applicableqty -= bitem.qty;
                                                    if (benefit.includediscount && bitem.schemediscount == 0)
                                                    {
                                                        bitem.schemediscount = bitem.discount * -1;
                                                    }
                                                }
                                                else
                                                {
                                                    if (benefit.includediscount && bitem.schemediscount == 0)
                                                    {
                                                        bitem.schemediscount = (bitem.discount / bitem.qty * applicableqty) * -1;
                                                    }
                                                    applicableqty = 0;
                                                }
                                                bitem.schemeid = item.id;

                                                if (applicableqty == 0) break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var disountpercentage = benefit.value;
                                        decimal maxqty = 0;
                                        if (benefit.eachqtyfrom)
                                        {
                                            maxqty = Math.Floor(pricerange.Sum(e => e.qty) / benefit.qtyfrom) * benefit.qtyfrom;
                                            foreach (var bitem in pricerange.OrderByDescending(x => x.qty))
                                            {
                                                decimal qty = bitem.qty <= maxqty ? bitem.qty : maxqty;
                                                if (qty > 0)
                                                {
                                                    var tdiscount = Math.Round(((bitem.price + (bitem.discount / bitem.qty)) * disountpercentage / 100) * qty, 2);
                                                    if (benefit.discountondiscount)
                                                        tdiscount = Math.Round(((bitem.price) * disountpercentage / 100) * qty, 2);

                                                    bitem.schemeid = item.id;
                                                    if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                        bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? (benefit.includediscount ? bitem.discount : 0) : 2), 2);
                                                    else
                                                        bitem.schemediscountcoupen = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                                                    maxqty -= qty;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (var bitem in pricerange)
                                            {
                                                var tdiscount = Math.Round(((bitem.price + (bitem.discount / bitem.qty)) * disountpercentage / 100) * bitem.qty, 2);
                                                if (benefit.discountondiscount)
                                                    tdiscount = Math.Round(((bitem.price) * disountpercentage / 100) * bitem.qty, 2);

                                                bitem.schemeid = item.id;
                                                if (item.applicablemode == Models.Promotions.ApplicableMode.Immediate)
                                                    bitem.schemediscount = Math.Round(tdiscount - (benefit.includediscount ? (benefit.includediscount ? bitem.discount : 0) : 0), 2);
                                                else
                                                    bitem.schemediscountcoupen = Math.Round(tdiscount - (benefit.includediscount ? bitem.discount : 0), 2);
                                            }
                                        }
                                    }
                                }
                                break;
                            case Models.Promotions.Benefit.BenefitMode.RewardPoints:
                                if ((benefit.qtyfrom == 0 || benefit.qtyfrom <= pricerange.Sum(e => e.qty) &&
                                    (benefit.qtyto == 0 || benefit.qtyto >= pricerange.Sum(e => e.qty))))
                                {
                                    foreach (var bitem in pricerange)
                                    {
                                        bitem.schemeid = item.id;
                                        bitem.schemereward = Math.Round(bitem.receivable / benefit.unit * benefit.value, 2);
                                    }
                                }
                                break;

                        }
                    }
                }
            }
            if (afterdiscount.Count > 0)
            {
                bill = PostProcess(bill, afterdiscount);
            }
            else
            {
                CheckBillValuePromotion(bill);
            }
            if (bill.Billitems.Sum(e => e.schemediscount) > 0)
            {
                bill.schemediscount = bill.Billitems.Sum(e => e.schemediscount);
                bill.schemediscountcoupen = bill.Billitems.Sum(e => e.schemediscountcoupen);
                bill.schemereward = bill.Billitems.Sum(e => e.schemereward);
                bill.HasPendingPromotion = bill.schemediscount != 0;
            }
            return bill;
        }

        private Models.Bill PostProcess(Models.Bill source, List<Models.Bill> discountedbills)
        {

            List<SchemeBenefit> schemebenefits = new List<SchemeBenefit>();
            foreach (var item in Schems.Where(e => e.locationid == -1 || e.locationid == source.locationid))
            {
                var iscombo = item.benefits.Exists(e => e.mode == Promotions.Benefit.BenefitMode.Combo);
                var ismultiply = item.benefits.Exists(e => e.mode != Promotions.Benefit.BenefitMode.Combo && e.qtyfrom > 1);
                var discount = discountedbills.Sum(e => e.Billitems.Where(x => x.schemeid == item.id).Sum(y => y.schemediscount + y.schemediscountcoupen));
                if (discount != 0)
                {
                    schemebenefits.Add(new SchemeBenefit()
                    {
                        id = item.id,
                        value = discount,
                        iscombo = iscombo,
                        ismultiple = ismultiply,
                        bill = discountedbills.Where(e => e.Billitems.Exists(ex => ex.schemeid == item.id)).OrderByDescending(e => e.Billitems.Where(x => x.schemeid == item.id && x.schemediscount != 0).Sum(y => y.schemediscount + y.schemediscountcoupen)).First()
                    });
                }
            }

            int index = 0;
            source.Billitems.ForEach(e => e.orderid = index++);
            foreach (var sb in schemebenefits.OrderByDescending(e => e.value))
            {
                index = 0;
                sb.bill.Billitems.ForEach(e => e.orderid = index++);
                foreach (var bitem in sb.bill.Billitems.Where(e => e.schemeid == sb.id))
                {
                    var actualbillitem = source.Billitems.Find(e => e.orderid == bitem.orderid);
                    if (actualbillitem.schemeid == 0)
                    {
                        actualbillitem.schemediscount = bitem.schemediscount;
                        actualbillitem.schemediscountcoupen = bitem.schemediscountcoupen;
                        actualbillitem.schemeeligble = bitem.schemeeligble;
                        actualbillitem.schemeid = bitem.schemeid;
                        actualbillitem.schemereward = bitem.schemereward;
                        source.HasPendingPromotion = true;
                    }
                    else if (!sb.iscombo && !sb.ismultiple &&
                      actualbillitem.schemediscount < bitem.schemediscount)
                    {
                        var currentscheme = schemebenefits.Find(e => e.id == actualbillitem.schemeid);
                        if (currentscheme.ismultiple || currentscheme.iscombo) continue;
                        actualbillitem.schemediscount = bitem.schemediscount;
                        actualbillitem.schemediscountcoupen = bitem.schemediscountcoupen;
                        actualbillitem.schemeeligble = bitem.schemeeligble;
                        actualbillitem.schemeid = bitem.schemeid;
                        actualbillitem.schemereward = bitem.schemereward;
                        source.HasPendingPromotion = true;
                    }
                }
            }

            CheckBillValuePromotion(source);
            return source;
        }

        public bool CheckBillValuePromotion(Bill source)
        {
            if (Schems.Exists(e => e.mode == Promotions.Modes.BillValue))
            {
                var newbill = source.ToJSON().Json2Object<Models.Bill>();
                var includediscount = BillValuePromotion(newbill);
                bool calculateBillValue = false;

                // (var bitem in newbill.Billitems)
                for (int i = 0; i < newbill.Billitems.Count; i++)
                {
                    var bitem = newbill.Billitems[i];
                    if (bitem.schemeid > 0 && bitem.schemediscount > 0)
                    {
                        var actualbillitem = source.Billitems[i];
                        if (actualbillitem.schemediscount < bitem.schemediscount)
                        {
                            actualbillitem.schemediscount = bitem.schemediscount;
                            actualbillitem.schemeid = bitem.schemeid;
                            actualbillitem.discount = bitem.discount;
                            actualbillitem.discountpercentage = bitem.discountpercentage;
                            actualbillitem.tax = bitem.tax;
                            actualbillitem.itemtax = bitem.itemtax;
                            actualbillitem.rate = bitem.rate;
                            actualbillitem.receivable = bitem.receivable;
                            source.HasPendingPromotion = true;
                            calculateBillValue = true;
                        }
                    }
                }

                if (calculateBillValue)
                {
                    source.additionaldiscount = source.Billitems.Sum(e => e.additionaldiscount * e.qty);
                    source.discount = source.Billitems.Sum(e => e.discount);
                    source.tax = source.Billitems.Sum(e => e.tax);
                    source.receivable = source.Billitems.Sum(e => e.receivable);
                    source.rounding = Math.Round(source.receivable, 0) - source.receivable;
                }
                return includediscount;
            }
            return false;
        }

        public void ClearPromotion(Models.Bill source)
        {
            Data.TaxDataService taxDataService = new Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var taxsplits = taxDataService.RangedTaxProducts(string.Join(",", source.Billitems.ConvertAll<string>(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'")));
            var billdiscount = source.Billitems.Sum(e => e.schemediscount);
            var list = source.Billitems;
            for (int i = 0; i < list.Count; i++)
            {
                var baseprice = list[i].price - list[i].additionaldiscount;
                if (taxsplits.ContainsKey(list[i].barcode))
                {
                    var newtax = taxsplits[list[i].barcode].Find(baseprice);
                    if (newtax.taxpercent != list[i].taxpercentage)
                    {
                        list[i].taxpercentage = newtax.taxpercent;
                    }
                }
                list[i].tax = Math.Round(list[i].qty * (baseprice / (100 + list[i].taxpercentage) * list[i].taxpercentage), 4);
                list[i].itemtax = Math.Round(baseprice / (100 + list[i].taxpercentage) * list[i].taxpercentage, 4);
                list[i].rate = baseprice - list[i].itemtax;
                list[i].receivable = baseprice * list[i].qty;
                list[i].discount -= list[i].schemediscount;
                list[i].schemediscount = 0;
            }
            source.schemediscount = 0;
            source.additionaldiscount = source.Billitems.Sum(e => e.additionaldiscount * e.qty);
            source.discount = source.Billitems.Sum(e => e.discount);
            source.tax = source.Billitems.Sum(e => e.tax);
            source.receivable = source.Billitems.Sum(e => e.receivable);
            source.rounding = Math.Round(source.receivable, 0) - source.receivable;
            //source.total = source.rounding + source.receivable;
        }
        public bool BillValuePromotion(Models.Bill source)
        {
            if (!Initialized)
            {
                LoadSchemes();
            }
            if (Schems == null || Schems.Count == 0) return false;
            List<SchemeBenefit> schemebenefits = new List<SchemeBenefit>();

            foreach (var item in Schems.Where(e => (e.locationid == -1 || e.locationid == source.locationid) && e.mode == Promotions.Modes.BillValue))
            {
                var bills = new List<Bill>();
                bills.Add(source.ToJSON().Json2Object<Models.Bill>());
                bool hasfilter = item.rules.Exists(e => e.mode != Promotions.Rules.RuleMode.none && e.mode != Promotions.Rules.RuleMode.billvalue);
                var billamount = hasfilter ? SchemedBillValue(item, bills) : source.total;
                if (!hasfilter)
                {
                    bills[0].Billitems.ForEach(e => e.schemeeligble = true);
                }
                if (item.rules.Exists(e => (e.mode == Promotions.Rules.RuleMode.billvalue || e.mode == Promotions.Rules.RuleMode.none) && (e.ValueFrom == 0 || e.ValueFrom <= billamount) && (e.ValueTo == 0 || e.ValueTo >= billamount)))
                {
                    foreach (var benefit in item.benefits.FindAll(e => e.mode == Promotions.Benefit.BenefitMode.DiscountInPercentage))
                    {
                        var fixedvalue = billamount * benefit.value / 100;
                        SchemeBenefit schemeBenefit = new SchemeBenefit();
                        schemeBenefit.bill = bills[0];
                        schemeBenefit.id = item.id;
                        schemeBenefit.includediscount = benefit.includediscount;
                        schemeBenefit.value = fixedvalue / billamount * 100;
                        schemebenefits.Add(schemeBenefit);
                    }
                    foreach (var benefit in item.benefits.FindAll(e => e.mode == Promotions.Benefit.BenefitMode.DiscountInFixedValue))
                    {
                        SchemeBenefit schemeBenefit = new SchemeBenefit();
                        schemeBenefit.bill = bills[0];
                        schemeBenefit.id = item.id;
                        schemeBenefit.includediscount = benefit.includediscount;
                        if (benefit.includediscount && !hasfilter)
                        {
                            billamount = source.total + source.discount;
                            schemeBenefit.value = benefit.value / billamount * 100;
                        }
                        else
                        {
                            schemeBenefit.value = benefit.value / billamount * 100;
                        }
                        schemebenefits.Add(schemeBenefit);
                    }
                }
            }
            bool isincludediscount = false;
            if (schemebenefits.Count > 0)
            {
                var scheme = schemebenefits.OrderByDescending(e => e.value).First();
                var list = source.Billitems.OrderByDescending(e => e.receivable).ToList();
                Data.TaxDataService taxDataService = new Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var taxsplits = taxDataService.RangedTaxProducts(string.Join(",", source.Billitems.ConvertAll<string>(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'")));

                var billdiscount = scheme.value;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!scheme.bill.Billitems.Exists(e => e.barcode == list[i].barcode && e.schemeeligble)) continue;

                    var price = list[i].price;
                    var currentBasePrice = price - (list[i].schemediscount / list[i].qty);
                    var newBasePrice = price - Math.Round(list[i].price * scheme.value / 100, 2);
                    if (scheme.includediscount)
                    {
                        newBasePrice = price += (list[i].discount / list[i].qty);
                        newBasePrice = newBasePrice - Math.Round(newBasePrice * scheme.value / 100, 2);
                    }
                    if (newBasePrice < currentBasePrice)
                    {
                        var schemediscount = Math.Round(price * scheme.value * list[i].qty / 100, 2);

                        if (scheme.includediscount)
                        {
                            newBasePrice = list[i].price + (list[i].discount / list[i].qty);
                            newBasePrice = newBasePrice - (schemediscount / list[i].qty);
                            if (schemediscount < list[i].discount)
                                continue;
                        }

                        list[i].schemeid = scheme.id;
                        var baseprice = list[i].price - (list[i].schemediscount / list[i].qty);
                        if (scheme.includediscount)
                        {
                            baseprice = baseprice + (list[i].discount / list[i].qty);
                            schemediscount -= list[i].discount;
                        }

                        list[i].schemediscount = schemediscount;
                        if (taxsplits.ContainsKey(list[i].barcode))
                        {
                            var newtax = taxsplits[list[i].barcode].Find(baseprice);
                            if (newtax.taxpercent != list[i].taxpercentage)
                            {
                                list[i].taxpercentage = newtax.taxpercent;
                            }
                        }
                        list[i].schemeid = scheme.id;
                        list[i].tax = Math.Round(list[i].qty * (baseprice / (100 + list[i].taxpercentage) * list[i].taxpercentage), 4);
                        list[i].itemtax = Math.Round(baseprice / (100 + list[i].taxpercentage) * list[i].taxpercentage, 4);
                        list[i].rate = baseprice - list[i].itemtax;
                        list[i].receivable = baseprice * list[i].qty;
                    }
                }
                //source.total = source.rounding + source.receivable;
            }
            return isincludediscount;
        }


        public Models.Billsettlementmaster.FreeProducts ProcessSettlementFreeProduct(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Models.Billsettlementmaster billsettlementmaster)
        {
            if (!Initialized)
            {
                LoadSchemes();
            }
            if (Schems == null || Schems.Count == 0) return null;
            var branchschems = Schems.FindAll(e => (e.locationid == -1 || e.locationid == billsettlementmaster.locationid) && e.enabled
                                                && e.applicablemode == Promotions.ApplicableMode.Settlement
                                                && e.mode == Promotions.Modes.BillValue
                                                && e.validfrom.Date <= DateTime.Now.Date && e.validto.Date >= DateTime.Now.Date);

            if (branchschems.Count == 0) return null;

            if (branchschems.Exists(e => e.benefits.Exists(x => x.mode == Promotions.Benefit.BenefitMode.Free &&
               string.IsNullOrEmpty(x.product))))
            {
                var allbenefits = new List<Promotions.Benefit>();
                branchschems.ForEach(e => allbenefits.AddRange(e.benefits));
                Data.ProductsDataService productsDataService = new Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var prodlist = productsDataService.SearchList(new ProductsCreteria()
                {
                    ids = allbenefits.GroupBy(e => e.item.productid).ToList().ConvertAll(e => e.Key)
                });
                allbenefits.ForEach(e =>
                {
                    if (prodlist.Exists(ex => ex.id == e.item.productid))
                        e.product = prodlist.Find(ex => ex.id == e.item.productid).name;
                });
            }

            var billamount = billsettlementmaster.Billsettlement.Sum(e => e.receivable);
            Models.Billsettlementmaster.FreeProducts result = null;
            var list = branchschems.FindAll(e => e.mode == Promotions.Modes.BillValue &&
                e.rules.All(ex => ex.ValueFrom <= billamount && ex.ValueTo >= billamount) &&
                e.benefits.All(ex => ex.mode == Promotions.Benefit.BenefitMode.Free));
            if (list.Count > 0)
            {
                var rlist = new List<Models.Billsettlementmaster.FreeProducts.Item>();
                foreach (var item in list)
                {
                    rlist.AddRange(item.benefits.ConvertAll(e => new Billsettlementmaster.FreeProducts.Item()
                    {
                        uancode = e.uancode,
                        from = e.pricefrom,
                        to = e.priceto,
                        productid = e.item.productid,
                        product = e.product
                    }));
                }
                rlist.RemoveAll(e => e.from > billamount);
                if (rlist.Count > 0)
                {
                    result = new Billsettlementmaster.FreeProducts();
                    result.Available = rlist;
                    result.Selected = rlist.OrderByDescending(e => e.from).First();
                }
            }
            return result;
        }

        public List<ElibleReason> ProcessSettlement(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Models.Billsettlementmaster billsettlementmaster)
        {
            if (!Initialized)
            {
                LoadSchemes();
            }
            var result = new List<ElibleReason>();
            var retvalue = new List<SchemeBenefit>();
            if (Schems == null || Schems.Count == 0) return null;
            var branchschems = Schems.FindAll(e => (e.locationid == -1 || e.locationid == billsettlementmaster.locationid) && e.enabled
                                                && e.applicablemode == Promotions.ApplicableMode.Settlement
                                                && e.validfrom.Date <= DateTime.Now.Date && e.validto.Date >= DateTime.Now.Date);

            if (branchschems.Count == 0) return null;
            var billamount = billsettlementmaster.Billsettlement.Sum(e => e.receivable);
            foreach (var item in branchschems.FindAll(e => e.mode == Promotions.Modes.BillValue))
            {
                Promotions.Rules rules = null;
                foreach (var rule in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.billvalue))
                {
                    if (billamount >= rule.ValueFrom && billamount <= rule.ValueTo)
                    {
                        rules = rule;
                        break;
                    }
                }


                if (rules != null)
                {
                    if (item.rules.Exists(e => e.mode != Promotions.Rules.RuleMode.billvalue))
                    {
                        List<Bill> bills = new List<Bill>();
                        foreach (var bitem in billsettlementmaster.Billsettlement)
                        {
                            Data.BillDataService billDataService = new Data.BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                            var bill = billDataService.Search(new BillCreteria()
                            {
                                id = bitem.billid
                            });
                            bills.AddRange(bill);
                        }
                        billamount = SchemedBillValue(item, bills);
                    }

                    decimal benefitamount = 0;
                    decimal benefitunit = 0;
                    decimal minbillvalue = 0;
                    decimal maxdiscountpercentage = 0;
                    long locationid = 0;
                    bool unlimited = false;
                    long filterschemeid = 0;
                    DateTime benefitvalidity = DateTime.MinValue;

                    foreach (var benefint in item.benefits.FindAll(e => e.mode == Promotions.Benefit.BenefitMode.Coupen))
                    {
                        //if (benefint.locationid == 0 || billsettlementmaster.locationid == benefint.locationid)
                        {
                            var newbenefitamount = (benefint.unit > 0 && benefint.value == 0) ? benefint.unit : Math.Round(benefint.value * billamount / 100, 2);
                            if (newbenefitamount > benefitamount)
                            {
                                benefitamount = newbenefitamount;
                                benefitunit = benefint.unit;
                                benefitvalidity = benefint.validity;
                                minbillvalue = benefint.pricefrom;
                                maxdiscountpercentage = benefint.priceto;
                                locationid = benefint.locationid;
                                unlimited = benefint.unlimited;
                                filterschemeid = benefint.filterschemeid;
                            }
                        }
                    }

                    if (benefitamount > 0)
                    {
                        retvalue.Add(new SchemeBenefit()
                        {
                            id = item.id,
                            value = benefitamount,
                            validity = benefitvalidity,
                            unit = benefitunit,
                            minbillvalue = minbillvalue,
                            maxdiscountpercentage = maxdiscountpercentage,
                            locationid = locationid,
                            unlimited = unlimited,
                            filterschemeid = filterschemeid
                        });
                    }

                }
            }

            if (retvalue.Count > 0)
            {
                ElibleReason elibleReason = new ElibleReason();
                var scheme = retvalue.OrderByDescending(e => e.value).First();
                elibleReason.id = scheme.id;
                elibleReason.benefitmode = Promotions.Benefit.BenefitMode.Coupen;
                elibleReason.value = scheme.value;
                elibleReason.unit = scheme.unit;
                elibleReason.validity = scheme.validity;
                elibleReason.minbillvalue = scheme.minbillvalue;
                elibleReason.unlimited = scheme.unlimited;
                elibleReason.maxdiscountpercentage = scheme.maxdiscountpercentage;
                elibleReason.locationid = scheme.locationid;
                result.Add(elibleReason);
            }

            retvalue = new List<SchemeBenefit>();
            foreach (var item in branchschems.FindAll(e => e.mode == Promotions.Modes.BillValue))
            {
                Promotions.Rules rules = null;
                foreach (var rule in item.rules.FindAll(e => e.mode == Promotions.Rules.RuleMode.billvalue))
                {
                    if (billamount >= rule.ValueFrom && billamount <= rule.ValueTo)
                    {
                        rules = rule;
                        break;
                    }
                }
                if (rules != null)
                {
                    decimal benefitamount = 0;
                    foreach (var benefint in item.benefits.FindAll(e => e.mode == Promotions.Benefit.BenefitMode.RewardPoints))
                    {
                        //if (benefint.locationid == 0 || billsettlementmaster.locationid == benefint.locationid)
                        {
                            var newbenefitamount = Math.Round(billamount / benefint.unit, 2) * benefint.value;
                            if (newbenefitamount > benefitamount)
                                benefitamount = newbenefitamount;
                        }
                    }
                    if (benefitamount > 0)
                    {
                        retvalue.Add(new SchemeBenefit()
                        {
                            id = item.id,
                            value = benefitamount
                        });
                    }
                }
            }

            if (retvalue.Count > 0)
            {
                ElibleReason elibleReason = new ElibleReason();
                var scheme = retvalue.OrderByDescending(e => e.value).First();
                elibleReason.id = scheme.id;
                elibleReason.benefitmode = Promotions.Benefit.BenefitMode.RewardPoints;
                elibleReason.value = scheme.value;
                result.Add(elibleReason);
            }

            return result;
        }

        public class SchemeBenefit
        {
            public long id { get; set; }
            public decimal value { get; set; }
            public Models.Bill bill { get; set; }
            public DateTime validity { get; set; }
            public decimal minbillvalue { get; set; }
            public bool unlimited { get; set; }
            public decimal maxdiscountpercentage { get; set; }
            public decimal unit { get; set; }
            public long locationid { get; set; }
            public bool includediscount { get; set; }
            public long filterschemeid { get; set; }
            public bool iscombo { get; internal set; }
            public bool ismultiple { get; internal set; }
        }

        private SortedDictionary<long, List<string>> FindBarcodeSchemes(Models.Bill bill, List<long> availableschems)
        {
            SortedDictionary<long, List<string>> retValue = new SortedDictionary<long, List<string>>();
            if (availableschems != null && availableschems.Count > 0)
            {
                string barcodes = string.Join(",", bill.Billitems.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'"));
                string query = "Select barcode,schemeid from schemebarcodes where (locationid=-1 or locationid=" + bill.locationid + ") and barcode in (" + barcodes + ") and schemeid in (" + String.Join(",", availableschems) + ") and COALESCE(isactive,true)=true";

                if (bill.locationid <= 0)
                    query = "Select barcode,schemeid from schemebarcodes where barcode in (" + barcodes + ") and schemeid in (" + String.Join(",", availableschems) + ") and COALESCE(isactive,true)=true";

                using (var reader = dataservice.ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        long schemeid = reader.GetInt64(1);
                        if (!retValue.ContainsKey(schemeid))
                            retValue.Add(schemeid, new List<string>());
                        retValue[schemeid].Add(reader.GetString(0));
                    }
                }
            }
            return retValue;
        }

        private SortedDictionary<long, List<string>> FindExcludedBarcodeSchemes(Models.Bill bill, List<long> availableschems)
        {
            SortedDictionary<long, List<string>> retValue = new SortedDictionary<long, List<string>>();
            if (availableschems != null && availableschems.Count > 0)
            {
                string barcodes = string.Join(",", bill.Billitems.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'"));
                string query = "Select barcode,schemeid from schemeexcludedbarcodes where locationid=" + bill.locationid + " and barcode in (" + barcodes + ") and schemeid in (" + String.Join(",", availableschems) + ") and COALESCE(isactive,true)=true";
                using (var reader = dataservice.ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        long schemeid = reader.GetInt64(1);
                        if (!retValue.ContainsKey(schemeid))
                            retValue.Add(schemeid, new List<string>());
                        retValue[schemeid].Add(reader.GetString(0));
                    }
                }
            }
            return retValue;
        }

        private SortedDictionary<long, List<string>> FindMarkedBarcodes(Models.Bill bill)
        {
            SortedDictionary<long, List<string>> retValue = new SortedDictionary<long, List<string>>();
            string barcodes = string.Join(",", bill.Billitems.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'"));
            string query = "Select markerid,barcode from stockmarker where locationid=" + bill.locationid + " and barcode in (" + barcodes + ")";
            using (var reader = dataservice.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    long markerid = reader.GetInt64(0);
                    if (!retValue.ContainsKey(markerid))
                        retValue.Add(markerid, new List<string>());
                    retValue[markerid].Add(reader.GetString(1));
                }
            }
            return retValue;
        }

        private List<ItemDetail> FindItemDetail(Models.Bill bill)
        {
            List<ItemDetail> retValue = new List<ItemDetail>();
            string barcodes = string.Join(",", bill.Billitems.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'"));
            string query = @"Select items.id,salablegoods.barcode,brandid,items.productid,colourid,patternid,sizeid,typeid,styleid,materialid,stock.designid from stock
                            inner join items on items.id = stock.itemid
                            inner join salablegoods on salablegoods.stockid = stock.id
                            where salablegoods.locationid = " + bill.locationid + " and salablegoods.barcode in (" + barcodes + ")";
            using (var reader = dataservice.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    ItemDetail oitems = new ItemDetail();
                    oitems.id = reader.GetInt64(0);
                    oitems.barcode = reader.GetString(1);
                    oitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                    oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    oitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                    oitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    oitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                    oitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                    oitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                    oitems.designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    retValue.Add(oitems);
                }
            }
            return retValue;
        }


        public class ItemDetail
        {
            public long id { get; set; }
            public long brandid { get; set; }
            public long productid { get; set; }
            public long colourid { get; set; }
            public long patternid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long typeid { get; set; }
            public long materialid { get; set; }
            public string barcode { get; set; }
            public string designid { get; set; }
        }
        private SortedDictionary<long, List<string>> FindInvoiceItemSchemes(Models.Bill bill)
        {
            SortedDictionary<long, List<string>> retValue = new SortedDictionary<long, List<string>>();
            string barcodes = string.Join(",", bill.Billitems.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'"));
            string query = @"Select barcode,invoiceitemid from stock
                            where barcode in (" + barcodes + ")";

            using (var reader = dataservice.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    long invoiceitemid = reader.GetInt64(1);
                    if (!retValue.ContainsKey(invoiceitemid))
                        retValue.Add(invoiceitemid, new List<string>());
                    retValue[invoiceitemid].Add(reader.GetString(0));
                }
            }
            return retValue;
        }

        private void Instance_OnPromotionChange(Processors.Config config)
        {
            var items = dataservice.Search(new Models.PromotionsCreteria()
            {
                id = config.id
            });
            mre.Reset();
            System.Threading.Thread.Sleep(500);
            Schems.RemoveAll(entity => entity.id == items[0].id);
            if (items[0].enabled && items[0].isactive)
            {
                Schems.Add(CreateScheme(items[0]));
            }
            mre.Set();
        }


        //private void PromoChanged(object sender, Npgsql.NpgsqlNotificationEventArgs e)
        //{
        //    string changedscheme = e.AdditionalInformation;
        //    long id = 0;
        //    if (long.TryParse(changedscheme, out id))
        //    {
        //        var items = dataservice.Search(new Models.PromotionsCreteria()
        //        {
        //            id = id
        //        });
        //        mre.Reset();
        //        System.Threading.Thread.Sleep(500);
        //        Schems.RemoveAll(entity => entity.id == items[0].id);
        //        if (items[0].enabled && items[0].isactive)
        //        {
        //            Schems.Add(CreateScheme(items[0]));
        //        }
        //        mre.Set();
        //    }
        //}

        private Scheme CreateScheme(Models.Promotions item)
        {
            Scheme scheme = new Scheme();
            scheme.id = item.id;
            scheme.rules = item.rules;
            scheme.benefits = item.benefit;
            scheme.hasbarcode = item.barcode_count > 0 || item.excludebarcode_count > 0;
            scheme.hasinvoiceitem = item.rules != null && item.rules.Exists(e => e.mode == Models.Promotions.Rules.RuleMode.invoiceitem);
            scheme.hasmarker = item.rules != null && item.rules.Exists(e => e.mode == Models.Promotions.Rules.RuleMode.marker);
            scheme.mode = (Models.Promotions.Modes)item.mode;
            scheme.applicablemode = (Models.Promotions.ApplicableMode)item.applicablemode;
            scheme.lastmodified = item.modifiedon;
            scheme.excludedpromotionids = item.excludedpromotionids;
            scheme.locationid = item.locationid;
            scheme.validto = item.validto;
            scheme.validfrom = item.validfrom;
            scheme.enabled = item.enabled;
            return scheme;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                //if (disposing)
                //{
                //    if (this.notificationConnection != null && this.notificationConnection.State != System.Data.ConnectionState.Closed)
                //    {
                //        this.notificationConnection.Notification -= this.PromoChanged;
                //        using (var command = new Npgsql.NpgsqlCommand("unlisten promochange", this.notificationConnection))
                //        {
                //            command.ExecuteNonQuery();
                //        }
                //        this.notificationConnection.Close();
                //    }
                //}
                Schems = null;
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Manager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}