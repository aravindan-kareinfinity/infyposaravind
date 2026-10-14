using ClosedXML.Excel;
using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billmaster : BaseEntity
    {
        public string code { get; set; }
        public long id { get; set; }
        public long locationid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime billdate { get; set; }
        public long customerid { get; set; }
        public decimal tax { get; set; }
        public decimal subtotal
        {
            get
            {
                return total + discount;
            }
        }
        public decimal subtotalwithoutrounding
        {
            get
            {
                return receivable + discount;
            }
        }

        public decimal rounding
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.rounding);
                }
                else return 0;
            }
        }
        [JsonIgnore]
        public bool hasreturn
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    if (Bills.Exists(e => e.billtype == Bill.BillTypes.Cash || e.billtype == Bill.BillTypes.Credit))
                    {
                        if (Bills.Exists(e => e.Billitems.Exists(x => x.receivable < 0)))
                        {
                            return true;
                        }
                        return false;
                    }
                    return false;
                }
                return false;
            }
        }
        [JsonIgnore]
        public decimal returnvalue
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.Billitems.Sum(x => x.receivable < 0 ? x.receivable : 0));
                }
                return 0;
            }
        }
        [JsonIgnore]
        public decimal salevalue
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.Billitems.Sum(x => x.receivable > 0 ? x.receivable : 0));
                }
                return 0;
            }
        }
        [JsonIgnore]
        public decimal additionaldiscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.additionaldiscount);
            }
        }
        [JsonIgnore]
        public bool hasadditionaldiscount
        {
            get
            {
                return additionaldiscount != 0;
            }
        }
        [JsonIgnore]
        public decimal schemediscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.totalschemediscount);
            }
        }
        [JsonIgnore]
        public bool hasschemediscount
        {
            get
            {
                return schemediscount != 0;
            }
        }

        [JsonIgnore]
        public decimal aschemediscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.aschemediscount);
            }
        }
        [JsonIgnore]
        public bool hasaschemediscount { get { return aschemediscount != 0; } }
        [JsonIgnore]
        public decimal rschemediscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.rschemediscount);
            }
        }
        [JsonIgnore]
        public bool hasrschemediscount { get { return rschemediscount != 0; } }

        [JsonIgnore]
        public bool hascoupondiscount
        {
            get
            {
                return coupondiscount != 0;
            }
        }
        [JsonIgnore]
        public decimal coupondiscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.billattributes != null ? x.billattributes.couponamount : 0);
            }
        }

        [JsonIgnore]
        public decimal onlydiscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.onlydiscount);
            }
        }
        [JsonIgnore]
        public bool hasonlydiscount
        {
            get
            {
                return onlydiscount != 0;
            }
        }
        [JsonIgnore]
        public bool hasrewarddiscount
        {
            get
            {
                return Bills == null ? false : Bills.Any(x => x.hasrewarddiscount);
            }
        }
        [JsonIgnore]
        public bool hasdeliveryno
        {
            get
            {
                return Bills == null ? false : Bills.Any(x => x.hasdeliveryno);
            }
        }
        [JsonIgnore]
        public string deliveryno
        {
            get
            {
                if (hasdeliveryno)
                    return Bills.Find(e => !string.IsNullOrEmpty(e.deliveryno)).deliveryno;
                return "";
            }
        }

        [JsonIgnore]
        public decimal onlyadditionaldiscount
        {
            get
            {
                return Bills == null ? 0 : Bills.Sum(x => x.onlyadditionaldiscount);
            }
        }
        [JsonIgnore]
        public bool hasonlyadditionaldiscount
        {
            get
            {
                return onlyadditionaldiscount != 0;
            }
        }
        [JsonIgnore]
        public bool hassettlement
        {
            get
            {
                return Bills != null && Bills.Count > 0 && Bills[0].billattributes != null && Bills[0].billattributes.settlement != null;
            }
        }

        public decimal discount { get; set; }
        public decimal total { get; set; }
        public decimal totalqty { get; set; }
        public decimal receivable { get; set; }
        public decimal other { get; set; }
        public string customername { get; set; }
        public string customercode { get; set; }
        public string customermobileno { get; set; }
        public string returnbillno { get; set; }
        public long returnreasonid { get; set; }
        public List<Bill> Bills { get; set; }
        public string customergst { get; set; }
        [JsonIgnore]
        public string mastercode { get; set; }
        [JsonIgnore]
        public string billno { get; set; }

        [JsonIgnore]
        public decimal schemefreegift
        {
            get
            {
                return Bills.Sum(z =>
                z.Billitems.Sum(x => x.schemeid > 0 && x.attributes != null && x.attributes.freeitems != null && x.attributes.freeitems.Count > 0 ? x.attributes.freeitems.Sum(y => y.qty) : 0));
            }
        }
        [JsonIgnore]
        public bool HasFreeGift
        {
            get
            {
                return schemefreegift > 0;
            }
        }
        [JsonIgnore]
        public List<BillItemAttributes.FreeItems> FreeItems { get; set; }

        public long salereasonid { get; set; }
        public long areaid { get; set; }
        public long seasonid { get; set; }
        public BillAttribute billattributes { get; set; }
        public string printdocument { get; set; }
        public Bill.BillCopy billcopy { get; set; }
        public bool HasDiscount
        {
            get
            {
                return discount != 0;
            }
        }
        public bool hascustomer
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills[0].hascustomer;
                }
                else return false;
            }
        }
        public decimal newreward
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.billattributes == null ? 0 : e.billattributes.newreward);
                }
                else return 0;
            }
        }
        // Reward points availble before this bill
        public decimal existingreward
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Convert.ToInt64(Bills.First().existingreward);
                }
                else return 0;

            }
        }
        // Reward points earned this bill
        public decimal earnedreward
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.billattributes == null ? 0 : e.billattributes.earnedreward);
                }
                else return 0;
            }
        }
        // Reward points remaining (total available) after this bill
        public decimal remainingreward
        {
            get
            {
                return existingreward + earnedreward - spentreward;
            }
        }
        public decimal spentreward
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.billattributes == null ? 0 : e.billattributes.spentreward);
                }
                else return 0;
            }
        }

        public decimal couponamount
        {
            get
            {
                if (Bills != null && Bills.Count > 0)
                {
                    return Bills.Sum(e => e.billattributes == null ? 0 : e.billattributes.couponamount);
                }
                else return 0;
            }
        }
        public string errormessage { get; set; }
        public bool directpdf { get; set; }
        public long bulkbillid { get; set; }
        public Bill.BillTypes sourcebilltype { get; set; }

        public Billmaster(long organizationid, Bill source, bool multicompany, DateTime currentDateTime)
        {
            source.billdate = currentDateTime;
            CreateBills(organizationid, source, multicompany, currentDateTime);
            if (source.additionalcharges != 0 && source.additionalchargeslist.Count > 0)
            {
                Bills.First().additionalcharges = source.additionalcharges;
                Bills.First().additionalchargeslist = source.additionalchargeslist;
            }

            if (source.voiditems != null && source.voiditems.Count > 0)
            {
                foreach (var voiditems in source.voiditems.GroupBy(x => x.companyid))
                {

                    if (Bills.Exists(x => x.companyid == voiditems.Key))
                    {
                        if (Bills.Find(x => x.companyid == voiditems.Key).voiditems == null)
                            Bills.Find(x => x.companyid == voiditems.Key).voiditems = new List<Billitems>();
                        voiditems.ForEach(y =>
                        {
                            y.isactive = false;
                            Bills.Find(x => x.companyid == voiditems.Key).voiditems.Add(y);
                        });
                    }
                    else
                    {
                        voiditems.ForEach(y =>
                        {
                            y.isactive = false;
                            if (Bills[0].voiditems == null)
                                Bills[0].voiditems = new List<Billitems>();
                            Bills[0].voiditems.Add(y);
                        });
                    }
                }
            }
        }

        public static List<Bill> CreateCancelledBills(List<Bill> listofbills, DateTime currentDateTime)
        {
            List<Bill> list = new List<Bill>();
            foreach (var bill in listofbills.Where(x => x.billtype != Bill.BillTypes.Cancel && x.settled == false))
            {
                bill.id = 0;
                bill.billmasterid = 0;
                bill.returnbillno = bill.billno;
                bill.billno = "";
                bill.counterid = bill.counterid;
                bill.billtype = Bill.BillTypes.Cancel;
                bill.Billitems.ForEach(e =>
                {
                    e.receivable = e.receivable * -1;
                    e.discount = e.discount * -1;
                    e.qty = e.qty * -1;
                    e.piece = e.piece * -1;
                    e.saleamount = e.saleamount * -1;
                    e.additionaldiscount = e.additionaldiscount * -1;
                    e.journalqty = e.journalqty * -1;
                    e.tax = e.tax * -1;
                });
                bill.billdate = currentDateTime;
                bill.isactive = true;
                bill.qty = bill.qty * -1;
                bill.totalqty = bill.Billitems.Sum(e => e.qty);
                bill.totalpiece = bill.Billitems.Sum(e => e.piece);
                bill.additionaldiscount = bill.Billitems.Sum(e => e.additionaldiscount * e.qty);
                bill.discount = bill.Billitems.Sum(e => e.discount);
                bill.tax = bill.Billitems.Sum(e => e.tax);
                bill.receivable = bill.Billitems.Sum(e => e.receivable);
                bill.rounding = bill.rounding * -1;
                //bill.total = bill.rounding + bill.receivable;
                list.Add(bill);
            }
            return list;
        }

        public Billmaster()
        {

        }

        public Bill CreateBill(Bill source, DateTime currentDateTime)
        {
            Bill bill = new Bill();
            bill.papersize = source.papersize;
            bill.printtemplateid = source.printtemplateid;
            bill.HasPendingPromotion = source.HasPendingPromotion;
            bill.locationid = source.locationid;
            bill.counterid = source.counterid;
            bill.billtype = source.billtype;
            bill.floorid = source.floorid;
            bill.countername = source.countername;
            bill.floorname = source.floorname;
            bill.areaid = source.areaid;
            bill.salereasonid = source.seasonid;
            bill.iscreditbill = source.iscreditbill;
            bill.areaid = source.salereasonid;
            bill.emidetail = source.emidetail;

            bill.isbulkreturn = source.isbulkreturn;
            bill.isbulkreturngoods = source.isbulkreturngoods;
            bill.iscreditapproved = true;
            if (source.billattributes != null)
                bill.billattributes = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(Newtonsoft.Json.JsonConvert.SerializeObject(source.billattributes));
            //bill.billattributes = source.billattributes;
            if (source.isb2b && bill.billattributes != null &&
                !string.IsNullOrEmpty(bill.billattributes.printrequiredapproval))
                bill.iscreditapproved = false;

            bill.AddItems(source.Billitems.ToList(), true);
            bill.additionaldiscount = source.Billitems.Sum(e => e.additionaldiscount * e.qty);
            if (bill.additionaldiscount > 0)
            {
                bill.discountrequestreason = source.discountrequestreason;
                bill.discountstatus = Discountqueue.DiscountStatus.Requested;
            }
            bill.isinterstatesale = source.isinterstatesale;
            bill.billdate = source != null ? source.billdate : currentDateTime;
            bill.companyid = source.Billitems.First().companyid;
            bill.locationid = source.Billitems.First().locationid;
            bill.customerid = source.customerid;
            bill.customername = source.customername;
            bill.customergst = source.customergst;
            bill.phoneno = source.phoneno;
            bill.isactive = true;

            if (source.Billitems.Any(e => e.qty < 0) && source.Billitems.Any(e => e.qty > 0))
            {
                bill.totalqty = bill.qty = source.Billitems.Sum(e => e.qty > 0 ? e.qty : 0);
                bill.totalpiece = bill.Billitems.Sum(e => e.piece > 0 ? e.piece : 0);
            }
            else
            {
                bill.totalqty = bill.qty = source.Billitems.Sum(e => e.qty);
                bill.totalpiece = bill.Billitems.Sum(e => e.piece);
            }
            bill.additionaldiscount = source.Billitems.Sum(e => e.additionaldiscount * e.qty);
            bill.discount = source.Billitems.Sum(e => e.discount);
            bill.schemediscount = source.Billitems.Sum(e => e.schemediscount);
            bill.tax = source.Billitems.Sum(e => e.tax);
            bill.receivable = source.Billitems.Sum(e => e.receivable);
            if (source.rounding == 0)
                bill.rounding = Math.Round(bill.receivable, MidpointRounding.AwayFromZero) - bill.receivable;
            else
                bill.rounding = source.rounding;

            //if(bill.rounding==0)
            //    bill.rounding =  Math.Round(bill.receivable,0) - bill.receivable;
            //if (bill.HasPendingPromotion)
            //{
            //bill.rounding = Math.Round(bill.receivable, 0) - bill.receivable;            
            //}
            //bill.total = bill.rounding + bill.receivable;
            bill.returnbillno = source.returnbillno;
            bill.returnreasonid = source.returnreasonid;
            bill.returnbillid = source.returnbillid;
            bill.taxsplit = new TaxSplit();
            bill.taxsplit.totalamount = bill.Billitems.Sum(e => e.rate * e.qty);
            if (bill.isinterstatesale)
            {
                bill.taxsplit.cgsttaxamount = 0;
                bill.taxsplit.sgsttaxamount = bill.tax;
                bill.taxsplit.secondtax = "IGST";

                bill.Billitems.ForEach(e =>
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
                bill.taxsplit.cgsttaxamount = Math.Round(bill.tax / 2, 5);
                bill.taxsplit.sgsttaxamount = Math.Round(bill.tax / 2, 5);
                bill.taxsplit.secondtax = "SGST";
                bill.Billitems.ForEach(e =>
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
            bill.BillTax.ForEach(e =>
            {
                e.secondtax = bill.taxsplit.secondtax;
            });
            return bill;
        }
        private bool CreateBills(long organizationid, Bill source, bool multicompany, DateTime currentDateTime)
        {
            if (Bills == null)
            {
                isactive = true;
                int orderid = 1;
                source.Billitems.ForEach(e => e.orderid = orderid++);
                customerid = source.customerid;
                customername = source.customername;
                customergst = source.customergst;
                customermobileno = source.phoneno;
                areaid = source.areaid;
                List<Bill> list = new List<Bill>();
                if (!multicompany)
                {
                    list.Add(CreateBill(source, currentDateTime));
                }
                else
                {
                    var groups = new List<List<Billitems>>();
                    foreach (var item in source.Billitems.GroupBy(e => new { e.companyid }))
                    {
                        groups.Add(new List<Billitems>(item));
                    }
                    var maxamount = groups.OrderBy(e => e.Sum(x => x.receivable)).Last().Sum(e => e.receivable);
                    var rounding = source.rounding;
                    source.rounding = 0;
                    BillAttribute billAttribute = null;
                    if (source.billattributes != null)
                        billAttribute = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(Newtonsoft.Json.JsonConvert.SerializeObject(source.billattributes));

                    foreach (var item in groups)
                    {
                        source.Billitems = item;


                        if (groups.Count() > 1 && billAttribute != null &&
                            billAttribute.couponamount > 0)
                        {
                            //if(source.billattributes == null)
                            source.billattributes = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(Newtonsoft.Json.JsonConvert.SerializeObject(billAttribute));
                            source.billattributes.couponamount = item.Sum(e => e.attributes == null ? 0 : e.attributes.coupondiscount * e.qty);
                        }
                        list.Add(CreateBill(source, currentDateTime));
                        orderid = 1;
                        source.Billitems.ForEach(e => e.orderid = orderid++);
                    }

                    /*$scope.RoundingMode = [{ 'id': 0, 'name': 'None' }, { 'id': 1, 'name': 'Rounding-5' } ,
                         * { 'id': 2, 'name': 'Down-5' },
            { 'id': 3, 'name': 'Up-5' }, { 'id': 4, 'name': 'Down-10' }, { 'id': 5, 'name': 'Up-10' }, 
            { 'id': 6, 'name': 'Rounding-10' },
            { 'id': 7, 'name': 'Down-1' },{ 'id': 8, 'name': 'Up-1' }, 
            { 'id': 9, 'name': 'Rounding-1' }];*/

                    list.ForEach(e =>
                    {
                        var billvalue = e.Billitems.Sum(ex => ex.receivable);
                        switch (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_salesrounding)
                        {
                            case 7:
                                e.rounding = Math.Floor(billvalue) - billvalue;
                                break;
                            case 0:
                                e.rounding = Math.Round(billvalue, MidpointRounding.AwayFromZero) - billvalue;
                                break;
                            case 8:
                                e.rounding = Math.Ceiling(billvalue) - billvalue;
                                break;
                            default:
                                e.rounding = Math.Round(billvalue) - billvalue;
                                break;
                        }
                    });
                    //list.OrderBy(e => e.receivable).First().rounding = rounding;

                }

                Bills = list;
                totalqty = Bills.Sum(e => e.totalpiece > 0 ? e.totalpiece : 0);
                tax = source.tax;
                discount = Bills.Sum(e => e.discount + e.additionaldiscount);
                receivable = Bills.Sum(e => e.receivable);
                total = Bills.Sum(e => e.receivable + e.rounding);
                return true;
            }
            return false;
        }
    }

    public class BillmasterCreteria : Billmaster
    {
        public bool includebills { get; set; }
    }

    public class BillInfo : BaseEntity
    {
        public long id { get; set; }
        public string billno { get; set; }
        public decimal total { get; set; }
        public long billsettlementmasterid { get; set; }
        public Billsettlementmaster settlement { get; set; }
    }
    public class BillDeliveryMasterInfo : BaseEntity
    {
        public int orderno { get; set; }
        public string deliveryno { get; set; }
        public decimal receivable { get; set; }

    }
}
