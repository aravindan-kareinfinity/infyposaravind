using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace InfyPOS.Models
{
    public class Bill : BaseEntity
    {
        public enum BillitemSorting
        {
            None,
            Name,
            Price,
            Amount
        }
        public BillitemSorting GetSorting(string sort)
        {
            if (string.IsNullOrEmpty(sort)) return BillitemSorting.None;
            switch (sort.ToLower())
            {
                case "name":
                    return BillitemSorting.Name;
                    break;
                case "price":
                    return BillitemSorting.Price;
                    break;
                case "amount":
                    return BillitemSorting.Amount;
                    break;
            }
            return BillitemSorting.None;
        }
        public Bill()
        {
            Billitems = new List<Billitems>();
        }
        public long id { get; set; }
        public long counterid { get; set; }
        public long customerstateid { get; set; }
        public string ecomorderno { get; set; }
        public string countername { get; set; }
        public long floorid { get; set; }
        public decimal additionalcharges { get; set; }
        public bool isb2b { get; set; }
        public bool iscreditapproved { get; set; }
        public bool isreturn { get; set; }
        public bool iscancel { get; set; }
        public string floorname { get; set; }
        public long locationid { get; set; }
        public bool isactive { get; set; }
        public bool isinterstatetransfer { get; set; }
        [JsonIgnore]
        public bool issynchronize { get; set; }
        public decimal qty { get; set; }
        public long createdby { get; set; }
        public bool hasemi { get; set; }
        public decimal customerreward { get; set; }
        public bool hasreturn
        {
            get
            {
                if (billtype == Bill.BillTypes.Cash || billtype == Bill.BillTypes.Credit)
                {
                    if (Billitems.Exists(e => e.receivable < 0))
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }
        public decimal returnvalue
        {
            get
            {
                if (Billitems != null && Billitems.Count > 0)
                {
                    return Billitems.Sum(e => e.receivable < 0 ? e.receivable : 0);
                }
                return 0;
            }
        }
        public decimal salevalue
        {
            get
            {
                if (Billitems != null && Billitems.Count > 0)
                {
                    return Billitems.Sum(e => e.receivable > 0 ? e.receivable : 0);
                }
                return 0;
            }
        }
        public decimal salediscount
        {
            get
            {
                if (Billitems != null && Billitems.Count > 0)
                {
                    return Billitems.Sum(e => e.receivable > 0 ? e.totaldiscount : 0);
                }
                return 0;
            }
        }
        public decimal salevaluewithdiscount
        {
            get
            {
                return salevalue + salediscount;
            }
        }
        public decimal salevaluewithouttax
        {
            get
            {
                return salevalue - tax;
            }
        }

        [JsonIgnore]
        public bool hasloan
        {

            get
            {
                return emidetail != null && emidetail.loanproviderid > 0;
            }
        }
        [JsonIgnore]
        public string hypothecation
        {
            get
            {
                return hasloan ? emidetail.loanprovidername : "";
            }
        }
        [JsonIgnore]
        public string loanreference
        {
            get
            {
                return hasloan ? emidetail.loanreference : "";
            }
        }
        [JsonIgnore]
        public decimal emiamount
        {
            get
            {
                return hasemi ? emidetail.loanamount : 0;
            }
        }


        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string emidetail_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(emidetail); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    emidetail = Newtonsoft.Json.JsonConvert.DeserializeObject<EMI>(value);
            }
        }

        public List<BillGroupingItem> groupedbillitems { get; set; }
        public List<BillGroupingItem> groupedbybrand { get; set; }
        public List<BillGroupingItem> groupedbysize { get; set; }
        public List<Billitems> groupedbydesign { get; set; }
        public List<Billitems> groupedbymfgdesign { get; set; }
        public List<BillGroupingItem> groupedbymeter { get; set; }
        public List<BillGroupingItem> groupedbydynamic { get; set; }
        public class BillGroupingItem
        {
            public Billitems source { get; set; }
            public decimal mrp { get; set; }
            public string printingname { get; set; }
            public string colour { get; set; }
            public string design { get; set; }
            public string sizegroup { get; set; }
            public string size { get; set; }
            public int orderid { get; set; }
            public string hsncode { get; set; }
            public decimal qty { get; set; }
            public decimal setqty { get; set; }
            public decimal rate { get; set; }
            public decimal gross { get; set; }
            public string brandname { get; set; }
            public string typename { get; set; }
            public decimal salerate { get; set; }
            public decimal totaldiscountpercentage { get; set; }
            public decimal ratewithdiscount { get; set; }
            public decimal b2bgross { get; set; }
            public string qtylist { get; set; }
            public decimal etprice { get; set; }
            public decimal etdiscount { get; set; }
            public decimal computed_etprice { get; set; }
            public decimal computed_etgross { get; set; }
            public decimal etgross { get; set; }
            public int sortingindex { get; set; }
            public int qtyinpiece { get; set; }
            public BillItemAttributes attributes { get; set; }
        }

        public EMI emidetail { get; set; }
        public class EMI
        {
            public string schemename { get; set; }
            public string loanprovidername { get; set; }
            public long schemeid { get; set; }
            public long loanproviderid { get; set; }
            public decimal advanceemi { get; set; }
            public int advancemonth { get; set; }
            public decimal billvalue { get; set; }
            public decimal rateofinterest { get; set; }
            public int tenure { get; set; }
            public decimal loanamount { get; set; }
            public decimal emicharges { get; set; }
            public decimal dbdcharge { get; set; }
            public decimal documentcharge { get; set; }
            public decimal marginmoney { get; set; }
            public decimal emicardcharge { get; set; }
            public string loanreference { get; set; }
            public string loanapprovedby { get; set; }
            public string loanreferredby { get; set; }
            public string loanreferrercontactno { get; set; }
        }
        [JsonIgnore]
        public string biller
        {
            get
            {
                if (billattributes != null)
                    return billattributes.biller;
                return "";
            }
        }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long billmasterid { get; set; }
        public string billno { get; set; }

        public DateTime billdate { get; set; }
        public long companyid { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Company company { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public CompanyLocation location { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public BillDeliveryDetails billdeliverydetails { get; set; }

        public Customer customer { get; set; }
        [JsonIgnore]
        public Customer shipto { get; set; }
        [JsonIgnore]
        public Agent agent { get; set; }
        public long customerid { get; set; }
        public decimal tax { get; set; }
        public decimal rounding { get; set; }
        public decimal discount { get; set; }
        public decimal receivable { get; set; }
        public decimal additionaldiscount { get; set; }
        [JsonIgnore]
        public decimal schemefreegift
        {
            get
            {
                return Billitems.Sum(x => x.schemeid > 0 && x.attributes != null && x.attributes.freeitems != null && x.attributes.freeitems.Count > 0 ? x.attributes.freeitems.Sum(y => y.qty) : 0);
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
        public decimal schemediscount { get; set; }
        public decimal schemereward { get; set; }
        public decimal schemediscountcoupen { get; set; }
        [JsonIgnore]
        public decimal saleamount
        {
            get
            {
                return receivable - tax;
            }
        }
        [JsonIgnore]
        public decimal gross
        {
            get
            {
                return Billitems.Sum(e => e.saleamount);
            }
        }
        [JsonIgnore]
        public bool hasdeliveryno { get { return !string.IsNullOrEmpty(deliveryno); } }
        [JsonIgnore]
        public string deliveryno
        {
            get
            {
                if (billattributes != null && billattributes.deliveryno != null)
                    return billattributes.deliveryno;
                return "";
            }
        }
        [JsonIgnore]
        public bool iscredit { get { return iscreditbill || billtype == BillTypes.Credit || billtype == BillTypes.Bulk; } }
        [JsonIgnore]
        public bool isbill { get { return billtype != BillTypes.Cancel; } }
        public bool iscreditbill { get; set; }
        public bool istransfer { get; set; }
        public bool isexport { get; set; }
        [JsonIgnore]
        public bool isreturnbill { get { return Billitems.All(e => e.receivable < 0); } }
        [JsonIgnore]
        public bool hascustomer { get { return customerid != 0 && !string.IsNullOrEmpty(customername); } }
        [JsonIgnore]
        public bool hasdiscount { get { return totaldiscount != 0; } }
        [JsonIgnore]
        public bool hasadditionaldiscount { get { return additionaldiscount != 0; } }
        public bool hasschemediscount { get { return totalschemediscount != 0; } }
        public bool hasonlydiscount
        {
            get
            {
                return onlydiscount != 0;
            }
        }
        [JsonIgnore]
        public bool hasgst { get { return customerid != 0 && !string.IsNullOrEmpty(customergst); } }
        [JsonIgnore]
        public bool hasdiscountandexchange { get { return totaldiscount != 0 && hasexchange; } }
        [JsonIgnore]
        public bool hasexchange
        {
            get
            {
                return Billitems.Any(e => e.receivable < 0)
                    && Billitems.Any(e => e.receivable > 0);
            }
        }
        public bool isbulkreturn { get; set; }

        public bool isestimate { get; set; }
        public bool isbulkreturngoods { get; set; }
        [JsonIgnore]
        public bool hasrewardcoupons
        {
            get
            {
                return billattributes != null && (billattributes.couponamount != 0 || billattributes.rewardamount != 0 || billattributes.manualrewardpoint != 0 || billattributes.manualrewardamount != 0);
            }
        }

        decimal _avlrewardpoint = 0;

        public decimal availablereward
        {
            get
            {
                if (_avlrewardpoint > 0) return _avlrewardpoint;
                return billattributes != null && billattributes.settlement != null ? billattributes.settlement.paymentinfo.avlrewardpoint : 0;
            }
            set
            {
                _avlrewardpoint = value;
            }
        }

        decimal _rewardpoint = 0;
        [JsonIgnore]
        public Referencelist sochargetype { get; set; }

        public decimal rewardpoint
        {
            get
            {
                if (_rewardpoint > 0) return _rewardpoint;
                return billattributes != null && billattributes.settlement != null ? billattributes.settlement.paymentinfo.rewardpoint : 0;
            }
            set
            {
                _rewardpoint = value;
            }
        }

        [JsonIgnore]
        public decimal adjustedcouponamount
        {
            get
            {
                return billattributes != null ? billattributes.couponamount : 0;
            }
        }
        [JsonIgnore]
        public decimal adjustedrewardamount
        {
            get
            {
                return billattributes != null ? billattributes.rewardamount + billattributes.manualrewardamount : 0;
            }
        }
        [JsonIgnore]
        public decimal newreward
        {
            get
            {
                return billattributes != null ? billattributes.newreward : 0;
            }
        }
        [JsonIgnore]
        public decimal earnedreward
        {
            get
            {
                return billattributes != null ? billattributes.earnedreward : 0;
            }
        }
        [JsonIgnore]
        public decimal remainingreward
        {
            get
            {
                return billattributes != null ? billattributes.remainingreward : 0;
            }
        }
        [JsonIgnore]
        public decimal spentreward
        {
            get
            {
                return billattributes != null ? billattributes.spentreward : 0;
            }
        }
        [JsonIgnore]
        public decimal existingreward
        {
            get
            {
                return remainingreward + spentreward - earnedreward;
            }
        }
        public decimal customerdiscount { get; set; }
        public decimal rewardunit { get; set; }
        public string returnbillno { get; set; }
        public long returnbillid { get; set; }
        public long returnreasonid { get; set; }
        public bool settled { get; set; }
        [JsonIgnore]
        public decimal totaldiscount { get { return discount + additionaldiscount; } }
        [JsonIgnore]
        public decimal totaldiscountlessreturn
        {
            get
            {
                if (billtype == BillTypes.ReturnBill) return totaldiscount;

                if (Billitems != null && Billitems.Count > 0)
                {
                    return Billitems.Sum(e => e.receivable > 0 ? e.totaldiscount : 0);
                }
                return 0;
            }
        }
        public decimal totalschemediscount
        {
            get
            {
                if (Billitems != null && Billitems.Count > 0)
                {
                    return Billitems.Sum(e => e.schemediscount > 0 ? e.schemediscount : 0);
                }
                return 0;
            }
        }
        public decimal onlydiscount
        {
            get
            {
                if (Billitems != null && Billitems.Count > 0)
                {
                    var totaldiscount = Billitems.Sum(e => e.discount > 0 ? e.discount : 0);
                    return totaldiscount - totalschemediscount;
                }
                return 0;
            }
        }
        [JsonIgnore]
        public decimal aschemediscount
        {
            get
            {
                return Billitems.Sum(e => e.attributes == null ? 0 : e.attributes.aschemediscount);
            }
        }
        [JsonIgnore]
        public bool hasaschemediscount { get { return aschemediscount > 0; } }
        [JsonIgnore]
        public decimal rschemediscount
        {
            get
            {
                return Billitems.Sum(e => e.rschemediscount);
            }
        }
        [JsonIgnore]
        public bool hasrschemediscount { get { return rschemediscount > 0; } }
        public bool hasonlyadditionaldiscount
        {
            get
            {
                return onlyadditionaldiscount != 0;
            }
        }
        public decimal onlyadditionaldiscount
        {
            get
            {
                if (additionaldiscount > 0)
                {
                    return additionaldiscount - (billattributes != null ? billattributes.couponamount : 0) - spentreward;
                }
                return 0;
            }
        }
        [JsonIgnore]
        public bool hastotaldiscountmrpprice
        {
            get
            {
                return totaldiscountmrpprice != 0;
            }
        }
        [JsonIgnore]
        public decimal totaldiscountmrpprice
        {
            get
            {
                //return totalmrp - (Billitems.Sum(e => e.qty > 0 ? e.salerate * e.qty : 0));
                return totalmrp - (Billitems.Sum(e => e.salerate * e.qty));
            }
        }
        [JsonIgnore]
        public decimal totaldiscountmrp
        {
            get
            {
                return totaldiscountmrpprice + totaldiscount;
            }
        }
        [JsonIgnore]
        public decimal totalsalerate
        {
            get
            {
                var mrpvalue = Billitems.Sum(e => e.salerate * e.qty);
                return mrpvalue;
            }
        }
        [JsonIgnore]
        public decimal totalmrp
        {
            get
            {
                //var mrpvalue = Billitems.Sum(e => e.qty > 0 && e.attributes == null ? 0 : (e.mrpdisplay * e.qty));
                var mrpvalue = Billitems.Sum(e => e.mrpdisplay * e.qty);
                return mrpvalue;
            }
        }
        public long salereasonid { get; set; }
        public string areaname { get; set; }

        public long areaid { get; set; }
        public long seasonid { get; set; }
        [JsonIgnore]
        public bool showfixeddisplay { get; set; }
        [JsonIgnore]
        public string fixedbilldisplay { get; set; }
        [JsonIgnore]
        public string alternatebilldisplay
        {
            get
            {
                if (billdisplay == "DELIVERY CHALLAN") return "APPROVED BILL";
                else if (billdisplay == "(COPY) DELIVERY CHALLAN") return "(COPY) APPROVED BILL";
                else if (billdisplay == "DELIVERY CHALLAN - BALANCE") return "(COPY) APPROVED BILL - PENDING";
                else return billdisplay;
            }
        }
        public int tzoffset { get; set; }
        [JsonIgnore]
        public string billdisplay
        {
            get
            {
                if (DateTime.UtcNow.AddMinutes(tzoffset).Date == billdate.Date ||
                    DateTime.UtcNow.AddMinutes(tzoffset).Date == createdon.Date)
                {
                    if (showfixeddisplay)
                        return fixedbilldisplay;
                    if (billtype == BillTypes.Credit)
                    {
                        if (receivable < 0)
                            return "RETURN CREDIT BILL";
                        else
                            return "CREDIT BILL";
                    }
                    else if (billtype == BillTypes.Bulk)
                        return "DELIVERY CHALLAN";
                    else if (billtype == Bill.BillTypes.BulkSaleRemaining)
                        return "DELIVERY CHALLAN - BALANCE";
                    else if (billtype == BillTypes.ReturnBill)
                        return "SALE RETURN";
                    return "SALES INVOICE";
                }

                if (showfixeddisplay)
                    return "(COPY) " + fixedbilldisplay;

                if (billtype == BillTypes.Credit)
                {
                    if (receivable < 0)
                        return "(COPY) RETURN CREDIT BILL";
                    else
                        return "(COPY) CREDIT BILL";
                }
                else if (billtype == BillTypes.Bulk)
                    return "(COPY) DELIVERY CHALLAN";
                else if (billtype == Bill.BillTypes.BulkSaleRemaining)
                    return "DELIVERY CHALLAN - BALANCE";
                else if (billtype == BillTypes.ReturnBill)
                    return "(COPY) SALE RETURN";
                return "(COPY) SALES INVOICE";
            }
        }
        [JsonIgnore]
        public string numberinword
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(totalreceivable);
            }
        }
        [JsonIgnore]
        public decimal totalcostprice
        {
            get
            {
                return Billitems.Sum(e => e.salerate > 0 ? e.salerate : 0);
            }
        }
        [JsonIgnore]
        public decimal totalamount_return
        {
            get
            {
                return Billitems.Sum(e => e.receivable < 0 ? e.receivable : 0);
            }
        }
        [JsonIgnore]
        public decimal totalamount_exchange
        {
            get
            {
                return Billitems.Sum(e => e.receivable > 0 ? e.receivable : 0);
            }
        }
        [JsonIgnore]
        public decimal totalamount_returndiscount
        {
            get
            {
                return Billitems.Sum(e => e.discount < 0 ? e.discount : 0);
            }
        }
        [JsonIgnore]
        public decimal totalamount_exchangediscount
        {
            get
            {
                return Billitems.Sum(e => e.discount > 0 ? e.discount : 0);
            }
        }

        [JsonIgnore]
        public decimal totalamount
        {
            get
            {
                return receivable + totaldiscount;
            }
        }
        [JsonIgnore]
        public decimal netamount
        {
            get
            {
                return totalamount - onlydiscount;
            }
        }
        public string enetamount
        {
            get
            {
                return Report.CitizenBarcode.Encrypt(netamount.ToString("F0"),
                    Services.ConfigManager.Instance(0).Config.numberencrypt);
            }
        }
        [JsonIgnore]
        public decimal totaletdiscountpercentage
        {
            get
            {
                return etdiscount > 0 ? Math.Round(etdiscount / etgross * 100, 2) : 0;
            }
        }
        [JsonIgnore]
        public decimal totaldiscountpercentage
        {
            get
            {
                return totaldiscount > 0 ? Math.Round(totaldiscount / (receivable + totaldiscount) * 100, 2) : 0;
            }
        }
        [JsonIgnore]
        public decimal grossamount
        {
            get
            {
                return totalamount - discount;
            }
        }
        [JsonIgnore]
        public decimal totalratewithdiscount
        {
            get
            {
                return Billitems.Sum(e => e.totalratewithdiscount);
            }
        }
        [JsonIgnore]
        public decimal totaldiscountwithouttax
        {
            get
            {
                return Billitems.Sum(e => e.discountwithouttax * e.qty);
            }
        }
        public int papersize { get; set; }
        public long printtemplateid { get; set; }
        public long unlimitedcouponid
        {
            get
            {
                return billattributes != null ? billattributes.couponid : 0;
            }
        }
        public BillAttribute billattributes { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string billattributes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(billattributes); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    billattributes = Newtonsoft.Json.JsonConvert.DeserializeObject<BillAttribute>(value);
            }
        }

        [JsonIgnore]
        public TaxSplit totalbilltax
        {
            get
            {
                var taxsplit = new TaxSplit();
                if (Billitems != null && Billitems.Count > 0)
                {
                    taxsplit.totalamount = Billitems.Sum(e => e.rate * e.qty);

                    if (this.isinterstatesale)
                    {
                        taxsplit.cgsttaxamount = 0;
                        taxsplit.sgsttaxamount = tax;
                        taxsplit.secondtax = "IGST";
                        taxsplit.name = "IGST";
                    }
                    else
                    {
                        taxsplit.name = "CGST";
                        taxsplit.cgsttaxamount = Math.Round(tax / 2, 5);
                        taxsplit.sgsttaxamount = taxsplit.cgsttaxamount;
                        taxsplit.secondtax = "SGST";
                    }

                    if (Billitems.Any(x => x.taxpercentage > 0))
                    {
                        taxsplit.cgsttaxpercentage = Billitems.FindAll(x => x.taxpercentage > 0).First().taxpercentage / 2;
                        taxsplit.sgsttaxpercentage = Billitems.FindAll(x => x.taxpercentage > 0).First().taxpercentage / 2;
                    }
                    else
                    {
                        var totaltax = (taxsplit.cgsttaxamount + taxsplit.sgsttaxamount) / taxsplit.totalamount * 100;
                        taxsplit.cgsttaxpercentage = Math.Round(totaltax / 2, 2);
                        taxsplit.sgsttaxpercentage = Math.Round(totaltax / 2, 2);
                    }
                }
                return taxsplit;
            }
        }

        [JsonIgnore]
        public TaxSplit totalchargetax
        {
            get
            {
                var taxsplit = new TaxSplit();
                if (additionalchargeslist != null && additionalchargeslist.Count > 0)
                {
                    taxsplit.totalamount = (additionalchargeslist == null ? 0 : additionalchargeslist.Sum(e => e.taxonly ? 0 : e.Charge));
                    if (this.isinterstatesale)
                    {
                        taxsplit.cgsttaxamount = 0;
                        taxsplit.sgsttaxamount = (additionalchargeslist == null ? 0 : additionalchargeslist.Sum(e => e.Tax));
                        taxsplit.secondtax = "IGST";
                        taxsplit.name = "IGST";
                    }
                    else
                    {
                        taxsplit.name = "CGST";
                        taxsplit.cgsttaxamount = Math.Round(((additionalchargeslist == null ? 0 : additionalchargeslist.Sum(e => e.Tax))) / 2, 5);
                        taxsplit.sgsttaxamount = taxsplit.cgsttaxamount;
                        taxsplit.secondtax = "SGST";
                    }

                    if (additionalchargeslist != null &&
                        additionalchargeslist.Any(x => x.taxpercentage > 0))
                    {
                        taxsplit.cgsttaxpercentage = additionalchargeslist.FindAll(x => x.taxpercentage > 0).First().taxpercentage / 2;
                        taxsplit.sgsttaxpercentage = additionalchargeslist.FindAll(x => x.taxpercentage > 0).First().taxpercentage / 2;
                    }
                    else if (taxsplit.totalamount > 0)
                    {
                        var totaltax = (taxsplit.cgsttaxamount + taxsplit.sgsttaxamount) / taxsplit.totalamount * 100;
                        taxsplit.cgsttaxpercentage = Math.Round(totaltax / 2, 2);
                        taxsplit.sgsttaxpercentage = Math.Round(totaltax / 2, 2);
                    }
                }
                return taxsplit;
            }
        }

        [JsonIgnore]
        public TaxSplit totaltax
        {
            get
            {
                var taxsplit = new TaxSplit();
                if (Billitems != null && Billitems.Count > 0)
                {
                    taxsplit.totalamount = Billitems.Sum(e => e.rate * e.qty) + (additionalchargeslist == null ? 0 : additionalchargeslist.Sum(e => e.taxonly ? 0 : e.Charge));

                    if (this.isinterstatesale)
                    {
                        taxsplit.cgsttaxamount = 0;
                        taxsplit.sgsttaxamount = tax + (additionalchargeslist == null ? 0 : additionalchargeslist.Sum(e => e.Tax));
                        taxsplit.secondtax = "IGST";
                        taxsplit.name = "IGST";
                    }
                    else
                    {
                        taxsplit.name = "CGST";
                        taxsplit.cgsttaxamount = Math.Round((tax + (additionalchargeslist == null ? 0 : additionalchargeslist.Sum(e => e.Tax))) / 2, 5);
                        taxsplit.sgsttaxamount = taxsplit.cgsttaxamount;
                        taxsplit.secondtax = "SGST";
                    }

                    if (Billitems.Any(x => x.taxpercentage > 0))
                    {
                        taxsplit.cgsttaxpercentage = Billitems.FindAll(x => x.taxpercentage > 0).First().taxpercentage / 2;
                        taxsplit.sgsttaxpercentage = Billitems.FindAll(x => x.taxpercentage > 0).First().taxpercentage / 2;
                    }
                    else
                    {
                        var totaltax = (taxsplit.cgsttaxamount + taxsplit.sgsttaxamount) / taxsplit.totalamount * 100;
                        taxsplit.cgsttaxpercentage = Math.Round(totaltax / 2, 2);
                        taxsplit.sgsttaxpercentage = Math.Round(totaltax / 2, 2);
                    }
                }
                return taxsplit;
            }
        }

        [JsonIgnore]
        public Billadditionalcharges PackingAddCharge
        {
            get
            {
                Billadditionalcharges billadditionalcharges = new Billadditionalcharges();
                if (additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "PACKING" && e.Charge > 0))
                {
                    return additionalchargeslist.Find(e => e.chargetype == "PACKING" && e.Charge > 0);
                }
                return new Billadditionalcharges();
            }
        }

        [JsonIgnore]
        public bool HasPackingAddCharge
        {
            get
            {
                return additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "PACKING" && e.Charge > 0);
            }
        }

        [JsonIgnore]
        public Billadditionalcharges PackingRemoveCharge
        {
            get
            {
                Billadditionalcharges billadditionalcharges = new Billadditionalcharges();
                if (additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "PACKING" && e.Charge < 0))
                {
                    return additionalchargeslist.Find(e => e.chargetype == "PACKING" && e.Charge < 0);
                }
                return new Billadditionalcharges();
            }
        }

        [JsonIgnore]
        public bool HasPackingRemoveCharge
        {
            get
            {
                return additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "PACKING" && e.Charge < 0);
            }
        }

        [JsonIgnore]
        public Billadditionalcharges TCS
        {
            get
            {
                Billadditionalcharges billadditionalcharges = new Billadditionalcharges();
                if (additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "TCS"))
                {
                    return additionalchargeslist.Find(e => e.chargetype == "TCS");
                }
                return new Billadditionalcharges();
            }
        }

        [JsonIgnore]
        public bool HasTCS
        {
            get
            {
                return additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "TCS");
            }
        }

        [JsonIgnore]
        public Billadditionalcharges Courier
        {
            get
            {
                Billadditionalcharges billadditionalcharges = new Billadditionalcharges();
                if (additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "COURIER"))
                {
                    return additionalchargeslist.Find(e => e.chargetype == "COURIER");
                }
                return new Billadditionalcharges();
            }
        }

        [JsonIgnore]
        public bool HasCourier
        {
            get
            {
                return additionalchargeslist != null && additionalchargeslist.Exists(e => e.chargetype == "COURIER");
            }
        }

        [JsonIgnore]
        public decimal additionalchargegross
        {
            get
            {
                return additionalchargeslist != null &&
                        additionalchargeslist.Count > 0 ? additionalchargeslist.Sum(e => e.Charge) : 0;
            }
        }

        [JsonIgnore]
        public decimal additionalchargestaxamount
        {
            get
            {
                return additionalchargeslist != null &&
                        additionalchargeslist.Count > 0 ? additionalchargeslist.Sum(e => e.Tax) : 0;
            }
        }

        [JsonIgnore]
        public decimal additionalchargesnetamount
        {
            get
            {
                return additionalchargeslist != null &&
                        additionalchargeslist.Count > 0 ? additionalchargeslist.Sum(e => e.Amount) : 0;
            }
        }


        [JsonIgnore]
        public string additionalchargename
        {
            get
            {
                return additionalchargeslist != null &&
                        additionalchargeslist.Count > 0 ? titleCase(additionalchargeslist.First().Name) : "";

            }
        }

        [JsonIgnore]
        public string customerpayableinword
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(customerpayable);
            }
        }
        [JsonIgnore]
        public decimal customerpayable { get { return receivable + rounding + additionalchargesnetamount; } }
        [JsonIgnore]
        public decimal totalreceivable { get { return receivable + rounding; } }    // additionalcharges
        [JsonIgnore]
        public decimal etgross { get { return Billitems != null && Billitems.Count > 0 ? Billitems.Sum(e => e.etgross) : 0; } }
        [JsonIgnore]
        public decimal etdiscount { get { return Billitems != null && Billitems.Count > 0 ? Billitems.Sum(e => e.etdiscount * e.qty) : 0; } }
        [JsonIgnore]
        public decimal etnet { get { return etgross - etdiscount; } }

        public decimal totalqty { get; set; }
        public decimal totalpiece { get; set; }
        public List<Billitems> voiditems { get; set; }
        public List<Billadditionalcharges> additionalchargeslist { get; set; }
        public List<Billitems> Billitems { get; set; }
        public List<BillTax> BillTax { get; set; }
        public List<BillHSN> BillHSN { get; set; }
        public string customername { get; set; }
        public string customergst { get; set; }
        public string phoneno { get; set; }
        public BillTypes billtype { get; set; }

        public bool offline { get; set; }
        public bool HasPromotionApplied { get; set; }
        public bool HasPendingPromotion { get; set; }
        internal void removescheme()
        {
            receivable += schemediscount;
            schemediscount = 0;
            schemediscountcoupen = 0;
            schemereward = 0;
            Billitems.ForEach(e =>
            {
                e.receivable += e.schemediscount;
                e.schemereward = 0;
                e.schemediscountcoupen = 0;
                e.schemediscount = 0;
                e.schemeeligble = false;
                e.schemeid = 0;
                if (e.attributes != null && e.attributes.freeitems != null)
                    e.attributes.freeitems.Clear();
            });
        }

        public Discountqueue.DiscountStatus discountstatus { get; set; }
        public string printdocument { get; set; }
        public BillCopy billcopy { get; set; }
        public BillCopy PrepareBillCopy(BillCopy sourcebillcopy)
        {
            billcopy = sourcebillcopy;
            billcopy.CreateClone(this);
            return billcopy;
        }
        public class BillCopy
        {
            public class BillClone
            {
                public string billno { get; set; }
                public decimal discount { get; set; }
                public string customername { get; set; }
                public string customermobileno { get; set; }
                public decimal billvalue { get; set; }
                public DateTime billdate { get; set; }
                public List<BillItemClone> lineitems { get; set; }
                public class BillItemClone
                {
                    public string code { get; set; }
                    public string product { get; set; }
                    public decimal qty { get; set; }
                    public decimal price { get; set; }
                    public decimal amount { get; set; }
                    public decimal discount { get; set; }
                }
                public BillClone()
                {

                }
                public BillClone(Bill bill)
                {
                    this.billno = bill.billno;
                    this.billvalue = bill.total;
                    this.billdate = bill.billdate.Date;
                    this.discount = bill.totaldiscount;
                    this.customername = bill.customer == null ? "" : bill.customer.name;
                    this.customermobileno = bill.customer == null ? "" : bill.customer.mobileno;
                    this.lineitems = bill.Billitems.ConvertAll(x => new BillItemClone()
                    {
                        amount = x.receivable,
                        price = x.qty != 0 ? Math.Abs((x.receivable + x.totaldiscount) / x.qty) : 0,
                        qty = x.qty,
                        product = x.printingname,
                        code = x.barcode,
                        discount = x.totaldiscount
                    });
                }
            }
            public void CreateClone(Bill bill)
            {
                content = Newtonsoft.Json.JsonConvert.SerializeObject(new BillClone(bill));
            }
            public string mode { get; set; }
            public string destination { get; set; }
            public string content { get; set; }
            public string encoded { get; set; }
        }
        public enum BillTypes
        {
            Cash = 0,
            Credit = 1,
            Card = 2,
            Redemption = 3,
            Exchange = 6,
            InterstateTransfers = 7,
            ReturnBill = 8,
            Cancel = 9,
            Bulk = 10,
            BulkSaleCompleted = 11,
            BulkSaleReturn = 12,
            BulkSaleBilled = 13,
            BulkSaleBilledWithReturns = 14,
            BulkSaleRemaining = 15,
            BulkCancel = 16,
            Estimate = 20,
            EstimateSaleCompleted = 21
        }

        internal void AddItems(List<Billitems> list, bool computetotal)
        {
            //if (computetotal)
            //{
            //    tax = 0;
            //    discount = 0;
            //    receivable = 0;
            //    additionaldiscount = 0;
            //    total = 0;
            //}
            this.BillTax = new List<BillTax>();
            this.Billitems = new List<Billitems>();
            foreach (var item in list)
            {
                //if (item.id == 0)
                //{
                //    if (item.discount > 0)
                //        item.discountpercentage = Math.Round(item.discount / (item.discount + item.price) * 100, 0);
                //    //item.piece = item.sellingmode == 3 ? 1 : item.qty;
                //    item.saleamount = item.qty * item.rate;
                //    item.salerate = Math.Round(item.price + (item.totaldiscountpercentage/item.qty),2);
                //    item.itemtax = Math.Round(item.tax / item.qty, 4);
                //}
                //if (item.taxsplit != null)
                //    item.taxpercentage = item.taxsplit.cgsttaxpercentage + item.taxsplit.sgsttaxpercentage;
                //item.taxpercentage = item.discount / item.qty;
                this.Billitems.Add(item);
                //if (computetotal)
                //{
                //    tax += item.tax;
                //    discount += item.discount;
                //    receivable += item.receivable+item.additionaldiscount;
                //    additionaldiscount += item.additionaldiscount;
                //    total += (item.receivable);
                //}

                if (item.schemediscount != 0 && computetotal)
                {
                    var price = item.price + (item.discount / item.qty);
                    if (item.schemediscount < 0)
                    {
                        item.discount -= (item.schemediscount * -1);
                        item.schemediscount = 0;
                    }

                    item.discount += (item.schemediscount);
                    decimal baserate = price - ((item.discount / item.qty) + item.additionaldiscount);
                    if (isinterstatesale)
                    {
                        var basePrice = (baserate / (100 + (item.taxpercentage)) * 100);
                        var halfTax = Math.Round(basePrice * (item.taxpercentage / 2) / 100, 5);
                        item.itemtax = Math.Round(halfTax * 2, 5);
                    }
                    else
                    {
                        item.itemtax = (baserate / (100 + item.taxpercentage)) * item.taxpercentage;
                    }
                    item.tax = Math.Round(item.itemtax * item.qty, 5);
                    item.rate = Math.Round(baserate - item.itemtax, 5);
                    item.receivable = Math.Round(baserate * item.qty, 2);
                    item.saleamount = item.rate * item.qty;
                }

                if (this.BillTax.Exists(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage))
                {
                    var taxitem = this.BillTax.Find(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage);
                    taxitem.tax += item.tax;
                    taxitem.amount += (item.gross);
                }
                else
                {
                    BillTax bt = new Models.BillTax();
                    bt.taxid = item.taxid;
                    bt.tax = item.tax;
                    bt.amount = item.gross;
                    bt.taxpercentage = item.taxpercentage;
                    this.BillTax.Add(bt);
                }
            }
        }

        public void RecalculateTax()
        {
            this.BillTax = new List<BillTax>();
            foreach (var item in Billitems)
            {
                if (this.BillTax.Exists(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage))
                {
                    var taxitem = this.BillTax.Find(e => e.taxid == item.taxid && e.taxpercentage == item.taxpercentage);
                    taxitem.tax += item.tax;
                    taxitem.amount += item.gross;
                }
                else
                {
                    BillTax bt = new Models.BillTax();
                    bt.taxid = item.taxid;
                    bt.tax = item.tax;
                    bt.amount = item.gross;
                    bt.taxpercentage = item.taxpercentage;
                    this.BillTax.Add(bt);
                }
            }
            this.taxsplit = new TaxSplit();
            this.taxsplit.totalamount = this.Billitems.Sum(e => e.rate * e.qty);
            if (this.isinterstatesale)
            {
                this.taxsplit.cgsttaxamount = 0;
                this.taxsplit.sgsttaxamount = this.tax;
                this.taxsplit.secondtax = "IGST";
                this.Billitems.ForEach(e =>
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
                this.taxsplit.cgsttaxamount = Math.Round(this.tax / 2, 5);
                this.taxsplit.sgsttaxamount = Math.Round(this.tax / 2, 5);
                this.taxsplit.secondtax = "SGST";
                this.Billitems.ForEach(e =>
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
            this.BillTax.ForEach(e =>
            {
                e.secondtax = this.taxsplit.secondtax;
            });
        }

        public string discountrequestreason { get; set; }
        public bool isinterstatesale { get; set; }
        public TaxSplit taxsplit { get; set; }
        [JsonIgnore]
        public string billkey { get; set; }
        [JsonIgnore]
        public string printurl { get; set; }
        [JsonIgnore]
        public string fburl { get; set; }
        [JsonIgnore]
        public decimal totalincludingLR
        {
            get
            {
                if (billdeliverydetails == null)
                    return total;
                else
                    return total + billdeliverydetails.frightcharges + billdeliverydetails.loadingcharges;
            }
        }
        [JsonIgnore]
        public decimal taxrounding
        {
            get
            {
                return Math.Round(tax) - tax;
            }
        }
        public decimal total
        {
            get
            {
                return receivable + rounding;

                //if (receivable < 0 && rounding < 0)
                //    return receivable - rounding;
                //else
                //    return receivable + rounding;// additionalcharges;
            }
        }
        [JsonIgnore]
        public bool hasadditionalcharges
        {
            get
            {
                return additionalcharges > 0;
            }
        }
        [JsonIgnore]
        public bool hasrewarddiscount
        {
            get
            {
                return billattributes != null && billattributes.rewardamount > 0;
            }
        }
        [JsonIgnore]
        public bool hascoupondiscount
        {
            get
            {
                return billattributes != null && billattributes.couponamount > 0;
            }
        }

        [JsonIgnore]
        public decimal totaldiscountlessreward
        {
            get
            {
                return hasrewarddiscount ? totaldiscount - billattributes.rewardamount : totaldiscount;
            }
        }
        [JsonIgnore]
        public bool DontChangeStock { get; set; }
        [JsonIgnore]
        public bool updatestock { get; set; }
        [JsonIgnore]
        public bool unsettledcancelled { get; set; }
        [JsonIgnore]
        public string greetingtext { get; set; }
        [JsonIgnore]
        public bool hasgreetingtext
        {
            get
            {
                return !string.IsNullOrEmpty(greetingtext);
            }
        }
        [JsonIgnore]
        public bool isfirst { get; set; }
        [JsonIgnore]
        public bool islast { get; set; }
        [JsonIgnore]
        public Servicecenters servicecenter { get; set; }
        [JsonIgnore]
        public bool hasservicecenter
        {
            get
            {
                return servicecenter != null;
            }
        }
        [JsonIgnore]
        public bool hassettlement
        {
            get
            {
                return billattributes != null && billattributes.settlement != null;
            }
        }
        [JsonIgnore]
        public bool backwardserialno { get; set; }
        [JsonIgnore]
        public string transportname { get; set; }
        [JsonIgnore]
        public string transportcity { get; set; }
        [JsonIgnore]
        public string agentname { get; set; }
        public string errormessage { get; set; }
        public bool directpdf { get; set; }
        public bool directdos { get; set; }
        [JsonIgnore]
        public string approvedby { get; set; }
        [JsonIgnore]
        public bool hasapprovedby
        {
            get
            {
                return !string.IsNullOrEmpty(approvedby);
            }
        }

        [JsonIgnore]
        public string salesref { get; set; }
        [JsonIgnore]
        public bool hassalesref
        {
            get
            {
                return !string.IsNullOrEmpty(salesref);
            }
        }
        [JsonIgnore]
        public Transport transport { get; set; }
        [JsonIgnore]
        public Employee salesmen { get; set; }
        [JsonIgnore]
        public string salesmencodes { get; set; }
        [JsonIgnore]
        public List<BillFoodCounter> FoodCounters { get; set; }
        [JsonIgnore]
        public Salesorder salesorder { get; set; }
    }


    public class PromotionExchange : BaseEntity
    {
        public long locationid { get; set; }
        public List<Item> items { get; set; }
        public List<Item> exchangeitems { get; set; }
        public class Item
        {
            public int sellingmode { get; set; }
            public long returnbillitemid { get; set; }
            public long rtnsrcitemid { get; set; }
            public int promotionid { get; set; }
            public int exchangeid { get; set; }
            public decimal qty { get; set; }
            public decimal eqty { get; set; }
            public string exchangeserialno { get; set; }
            public string exchangebarcode { get; set; }
            public string promotionbarcode { get; set; }
            public decimal discount { get; set; }
            [JsonIgnore]
            public Billitems billitem { get; set; }
            public decimal discountpercentage { get;  set; }
            public decimal discountmax { get;  set; }
        }
    }
    public class BillFoodCounter
    {
        public string counter { get; set; }
        public Bill bill { get; set; }
        public decimal qty { get; set; }
        public decimal peice { get; set; }
        public decimal receivable { get; set; }
        public List<Billitems> items { get; set; }
    }
    public class BillHSN
    {
        public string code { get; set; }
        public decimal qty { get; set; }
        public decimal peice { get; set; }
        public decimal tax { get; set; }
        public decimal taxpercentage
        {
            get
            {
                if (igst_percentage > 0) return igst_percentage;
                return cgst_percentage * 2;
            }
        }
        public decimal taxable { get; set; }
        public decimal total { get; set; }
        public decimal cgst_percentage { get; set; }
        public decimal sgst_percentage { get; set; }
        public decimal igst_percentage { get; set; }
        public decimal cgst_value { get; set; }
        public decimal sgst_value { get; set; }
        public decimal igst_value { get; set; }
    }
    public class BillTax
    {

        public long taxid { get; set; }
        public string taxname { get; set; }
        public decimal tax { get; set; }
        public decimal taxpart
        {
            get
            {
                return Math.Round(tax / 2, 4);
            }
        }
        public decimal taxpartpercentage
        {
            get
            {
                return Math.Round(taxpercentage / 2, 2);
            }
        }
        public decimal totalamount
        {
            get
            {
                return Math.Round(amount + tax, 2);
            }
        }
        public decimal cgsttaxamount
        {
            get
            {
                if (secondtax == "SGST")
                    return taxpart;
                return 0;
            }
        }
        public decimal sgsttaxamount
        {
            get
            {
                if (secondtax == "SGST")
                    return taxpart;
                return tax;
            }
        }

        public decimal cgsttax
        {
            get
            {
                if (secondtax == "SGST")
                    return taxpart;
                return 0;
            }
        }
        public decimal sgsttax
        {
            get
            {
                if (secondtax == "SGST")
                    return taxpart;
                return 0;
            }
        }

        public decimal igsttax
        {
            get
            {
                if (secondtax == "SGST")
                    return 0;
                return tax;
            }
        }
        public decimal amount { get; set; }
        public decimal taxpercentage { get; set; }
        public string secondtax { get; set; }
    }
    public class BillCreteria : Bill
    {
        public bool enableprintdocument { get; set; }
        public string barcode { get; set; }
        public bool saledirectpdf { get; set; }
        public bool saledirectdos { get; set; }
        public List<long> ids { get; set; }
        public List<string> billnos { get; set; }
        public bool forsettlement { get; set; }
        public bool isreloadsalebill { get; set; }
        public bool forsalesreturn { get; set; }
        public bool isMasterBill { get; set; }
        public bool isimport { get; set; }
        public bool removecancelled { get; set; }
        public bool frompos { get; set; }
    }

    public class BillAttribute
    {
        public string ebillnos
        {
            get
            {
                if (ebillno != null && ebillno.Count > 0)
                    return string.Join(", ", ebillno.ConvertAll(e => e.billno + " - " + e.countername));
                return "";
            }
        }
        public class Coupon
        {
            public string code { get; set; }
            public long id { get; set; }
            public decimal amount { get; set; }
        }
        public class EBillDetail
        {
            public string billno { get; set; }
            public string countername { get; set; }
        }
        public List<EBillDetail> ebillno { get; set; }
        public long rewardamount { get; set; }
        public long rewardpoint { get; set; }
        public string couponcode { get; set; }
        public List<Coupon> coupons { get; set; }
        public long couponid { get; set; }
        public decimal discountamount { get; set; }
        public decimal couponamount { get; set; }
        public long manualrewardpoint { get; set; }
        public long manualrewardamount { get; set; }
        public decimal newreward { get; set; }
        public decimal earnedreward { get; set; }
        public decimal remainingreward { get; set; }
        public decimal spentreward { get; set; }
        public long sizegroupid { get; set; }
        public string mode { get; set; }
        public string deliveryno { get; set; }
        public Billsettlementmaster settlement { get; set; }
        public string biller { get; set; }
        public string billercode { get; set; }
        public string floorname { get; set; }
        public string countername { get; set; }
        public long floorid { get; set; }
        public long counterid { get; set; }
        public bool showdiscount { get; set; }
        public long approvedby { get; set; }
        public long multipriceid { get; set; }
        public string remarks { get; set; }
        public bool removed { get; set; }
        public string walkintoken { get; set; }

        [JsonIgnore]
        public bool deliveryrackft { get; set; }
        public string deliveryrack { get; set; }
        public long deliveryrackid { get; set; }
        public long tailoringorderid { get; set; }
        public string deliveryrackcard { get; set; }
        public bool hasshiptto { get; set; }
        public long shiptoid { get; set; }
        public long salesrefid { get; set; }
        public long printapprovedby { get; set; }
        public string printrequiredapproval { get; set; }
        public string orderby { get; set; }
        public string orderrefid { get; set; }
        [JsonIgnore]
        public string orderpoid { get; set; }
        public string packslipid { get; set; }
        public DateTime duedate { get; set; }
        public bool gift { get; set; }
        public bool giftcoupon { get; set; }
        public bool online { get; set; }
        public bool touchsale { get; set; }
        public int billmode { get; set; }
        public long billtemplateid { get; set; }

        public ECommerce ecom { get; set; }
        [JsonIgnore]
        public Deliveryrack.AddInfo deliveryrackinfo { get; set; }
        public string tailoringorderno { get; set; }
        [JsonIgnore]
        public long billinboxid { get; set; }
        [JsonIgnore]
        public string[] packslipids { get; set; }
        [JsonIgnore]
        public long customerinvoiceid { get; set; }
        public long bundleid { get; set; }

        public class ECommerce
        {
            public string contact { get; set; }
            public string customer { get; set; }
            public string channel { get; set; }
            public string morderid { get; set; }
            public string orderid { get; set; }
            public string invoicecode { get; set; }
            public string address { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string pin { get; set; }
        }
    }

    public class SalesReport : BaseEntity
    {

        public long companyid { get; set; }
        public long locationid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long brandid { get; set; }
        public long itemid { get; set; }
        public decimal buyingprice { get; set; }
        public decimal price { get; set; }
        public decimal iqty { get; set; }
        public decimal dqty { get; set; }
        public decimal oqty { get; set; }
        public decimal qty { get; set; }
        public long typeid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long sizeid { get; set; }
        public string barcode { get; set; }
        public string billno { get; set; }

        public DateTime billdate { get; set; }
        public decimal amount { get; set; }
        public decimal tax { get; set; }
        public decimal total { get; set; }
        public decimal receivable { get; set; }
        public decimal discount { get; set; }
    }

    public class BillCancel : BaseEntity
    {
        public List<long> ids { get; set; }

        public long locationid { get; set; }
        public bool disablestockreverse { get; set; }
        public long counterid { get; set; }
        public bool isbulkreturn { get; set; }
        public bool isbulksale { get; set; }
    }
    public class CustomerChange : BaseEntity
    {
        public List<long> ids { get; set; }
        public DateTime billdate { get; set; }
        public long customerid { get; set; }
    }
    public class HSNChange : BaseEntity
    {
        public List<string> barcodes { get; set; }
        public string hsncode { get; set; }
    }
    public class SalesReportCreteria : SalesReport
    {
        public bool barcod { get; set; }
        public bool pattern { get; set; }
        public bool size { get; set; }
        public bool product { get; set; }
        public bool color { get; set; }
        public bool brand { get; set; }
        public bool style { get; set; }
        public bool material { get; set; }
        public bool supplier { get; set; }
        public bool invoicedate { get; set; }
        public bool netamount { get; set; }
        public bool availqty { get; set; }

    }

    public class TaxSplit
    {
        public decimal taxpercentage
        {
            get
            {
                return cgsttaxpercentage + sgsttaxpercentage;
            }
        }
        public decimal taxamount
        {
            get
            {
                if (isinterstatesale) return igsttaxamount;
                return cgsttaxamount + sgsttaxamount;
            }
        }
        public string name { get; set; }
        public decimal totalamount { get; set; }
        [JsonIgnore]
        public decimal netamount
        {
            get
            {
                return totalamount + taxamount;
            }
        }
        public decimal cgsttaxpercentage { get; set; }
        public decimal cgsttaxamount { get; set; }
        public string secondtax { get; set; }
        public decimal sgsttaxpercentage { get; set; }
        public decimal sgsttaxamount { get; set; }
        public decimal igsttaxamount { get; set; }
        public decimal igsttaxpercentage
        {
            get
            {
                return taxpercentage;
            }
        }
        public bool isinterstatesale { get; set; }
    }

    public class BillOTP : BaseEntity
    {
        public string MOBILE { get; set; }
        public string EMAIL { get; set; }
        public string OTP { get; set; }
        public decimal BILLAMOUNT { get; set; }
        public decimal DISCOUNT { get; set; }
        public decimal DISCOUNTPERCENT { get; set; }
        public decimal NETAMOUNT { get; set; }
        public Decimal ALLOWEDCREDIT { get; set; }
        public string LOCATION { get; set; }
        public string COUNTER { get; set; }
        public string CUSTOMER { get; set; }
        public string CUSTOMERMOBILENO { get; set; }
        public long locationid { get; set; }
        public string BILLNO { get; set; }
        public DateTime BILLDATE { get; set; }
        public string REASON { get; set; }
        public decimal CLAIMAMOUNT { get; set; }
        public decimal CLAIMPOINT { get; set; }
    }

    public class BillIntegration
    {
        public string mode { get; set; }
        public string type { get; set; }
        public string sourceformat { get; set; }
        public BillIntegration(string sourceformat)
        {
            this.sourceformat = sourceformat;
        }
        public string accesskey { get; set; }
        public string possystem { get; set; }
        public string transactionon { get; set; }
        public string branch { get; set; }

        public Customer customer { get; set; }
        public Bill bills { get; set; }
        public List<Vouchers> vouchers { get; set; }
        public List<BillItem> billitems { get; set; }

        public class Customer
        {
            public string name { get; set; }
            public string id { get; set; }
            public string mobileno { get; set; }
        }

        public class BillItem
        {
            public string barcode { get; set; }
            public string gln { get; set; }
            public decimal soldqty { get; set; }
            public decimal stockqty { get; set; }
            public decimal onlinqty { get; set; }
            public decimal amount { get; set; }
        }
        public class Bill
        {
            public string billno { get; set; }
            public long billid { get; set; }
            public long organizationid { get; set; }
            public DateTime billdate { get; set; }
            public decimal billamount { get; set; }
            public decimal loyeltyclaimed { get; set; }
            public decimal loyeltyissued { get; set; }
            public decimal loyeltybalance { get; set; }
            public string company { get; set; }
            public string branch { get; set; }
        }

        public class Vouchers
        {
            public string no { get; set; }
            public decimal issued { get; set; }
            public decimal consumed { get; set; }
        }

        public string destinationjson()
        {
            string destination = sourceformat;
            /*{
"branch":"bangalore",
"company": "kumaran",
"mode":"sales | sync",
"products": [@BILLITEMS{"BARCODE":"X1","STOCK":10}BILLITEMS@]
}*/
            if (destination.IndexOf("@BILLITEMS") > 0 &&
                destination.IndexOf("BILLITEMS@") > 0)
            {
                int startIndex = destination.IndexOf("@BILLITEMS");
                int endIndex = destination.IndexOf("BILLITEMS@", startIndex + 1);
                string left = destination.Substring(0, destination.IndexOf("@BILLITEMS"));
                string mid = destination.Substring(startIndex + 10, endIndex - (startIndex + 10));
                string right = destination.Substring(endIndex + 10, destination.Length - (endIndex + 10));
                List<string> bitemlist = new List<string>();
                foreach (var bitem in billitems)
                {
                    var bitemrow = mid.Replace("[BARCODE]", bitem.barcode.Replace(@"""", ""));
                    bitemrow = bitemrow.Replace("[GLN]", bitem.gln.Replace(@"""", ""));
                    bitemrow = bitemrow.Replace("[STOCK]", bitem.stockqty.ToString("N2"));
                    bitemrow = bitemrow.Replace("[OLSTOCK]", bitem.onlinqty.ToString("N2"));
                    bitemrow = bitemrow.Replace("[SOLD]", bitem.soldqty.ToString("N2"));
                    bitemlist.Add(bitemrow);
                }
                destination = left + string.Join(",", bitemlist) + right;
            }

            destination = destination.Replace("[MODE]", String.IsNullOrEmpty(mode) ? "" : mode.Replace(@"""", ""));
            destination = destination.Replace("[TYPE]", String.IsNullOrEmpty(type) ? "" : type.Replace(@"""", ""));
            destination = destination.Replace("[COMPANY]", String.IsNullOrEmpty(bills.company) ? "" : bills.company.Replace(@"""", ""));
            destination = destination.Replace("[BRANCH]", string.IsNullOrEmpty(bills.branch) ? "" : bills.branch.Replace(@"""", ""));
            destination = destination.Replace("[MOBILENO]", customer != null && customer.mobileno != null ? customer.mobileno.Replace(@"""", "") : "");
            destination = destination.Replace("[BILLID]", bills.billid.ToString());
            destination = destination.Replace("[BILLKEY]", "BILL_" + bills.organizationid.ToBase62() + "_" + bills.billid.ToBase62());
            destination = destination.Replace("[NAME]", customer != null && customer.name != null ? customer.name.Replace(@"""", "") : "");
            destination = destination.Replace("[LOYELTYAVAILABLE]", bills.loyeltybalance.ToString("F0"));
            destination = destination.Replace("[LOYELTYCLAIMED]", bills.loyeltyclaimed.ToString("F0"));
            destination = destination.Replace("[LOYELTYISSUED]", bills.loyeltyissued.ToString("F0"));
            destination = destination.Replace("[BILLNO]", string.IsNullOrEmpty(bills.billno) ? "" : bills.billno.Replace(@"""", ""));
            destination = destination.Replace("[BILLAMOUNT]", bills.billamount.ToString("F0"));
            destination = destination.Replace("[BILLDATE]", bills.billdate.ToString("yyyy-MM-dd hh:mm:ss"));
            destination = destination.Replace("[BILLEDON]", bills.billdate.ToString("yyyy-MM-dd"));
            return destination;
        }
    }

    public class ECOMUANUploaded : BaseEntity
    {
        public int noofbill { get; set; }
        public int validbill { get; set; }
        public long companyid { get; set; }
        public List<string> missingqty { get; set; }
        public List<string> missinguan { get; set; }
        public List<Order> orders { get; set; }
        public List<string> missingcompany { get; set; }
        public List<string> missinglocation { get; set; }
        public bool ShowBillNo
        {
            get
            {
                return orders != null &&
                    orders.Any(x => !string.IsNullOrEmpty(x.BillNo));
            }
        }
        public List<SOUANUploaded.StockInfo> stocklist { get; set; }
        public List<string> missingcustomers { get; set; }
        public List<string> missingstates { get; internal set; }

        public class OrderItem
        {
            public long salablegoodsid { get; set; }
            public string PrdSKU { get; set; }
            public long ItemID { get; set; }
            public long ProductID { get; set; }
            public long ColourID { get; set; }
            public decimal MRP { get; set; }
            public long SizeID { get; set; }
            public string Design { get; set; }
            public string Product { get; set; }
            public String Size { get; set; }
            public decimal Qty { get; set; }

            [JsonIgnore]
            public decimal RQty { get; set; }
            public decimal Price { get; set; }
            public string UAN { get; set; }
            public string HSN { get; set; }
            public decimal Percentage { get; set; }
            public decimal Tax { get; set; }
            public long TaxID { get; set; }
            [JsonIgnore]

            public string barcode { get; set; }
            [JsonIgnore]

            public long stockid { get; set; }
            public decimal Discount { get; set; }
        }
        public class Order
        {
            public decimal Tax { get; set; }
            public long customerid { get; set; }
            public string customer { get; set; }
            public string categoryid { get; set; }
            public string Remark { get; set; }
            public string MOrderNo { get; set; }
            public DateTime BillDate { get; set; }
            public string Company { get; set; }
            public string Location { get; set; }
            public string OrderNo { get; set; }
            public List<OrderItem> items { get; set; }
            public long locationid { get; set; }
            public decimal Qty { get; set; }
            public bool stocknotavailable { get; set; }
            public List<string> MissingUAN { get; set; }
            public bool valid
            {
                get
                {
                    return locationid > 0 &&
                        companyid > 0 &&
                        customerid > 0 &&
                        stateid > 0 &&
                        string.IsNullOrEmpty(BillNo) &&
                        !stocknotavailable &&
                        BillDate != DateTime.MinValue &&
                        MissingUAN.Count == 0;
                }
            }
            public int count
            {
                get
                {
                    return items.Count;
                }
            }

            public decimal Amount { get; set; }
            public long companyid { get; set; }
            public string BillNo { get; set; }
            public DateTime dispatchon { get; set; }
            public long stateid { get; set; }
            public BillAttribute.ECommerce ECommerce { get; set; }
            public string invoicecode { get; set; }
            public string invoicedisplaycode { get; set; }
        }
    }


    public class Bill2Print : Bill, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return true;
        }
        public Bill2Print(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public Bill2Print()
        {
        }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("COUNTERNAME", countername);
                ret.Add("LOCATIONNAME", location == null ? "" : location.name);
                ret.Add("COMPANYNAME", company == null ? "" : company.name);
                ret.Add("FLOORNAME", floorname);
                ret.Add("BILLDATE", billdate);
                ret.Add("BILLTIME", billdate);

                ret.Add("MOBILENO", customer != null ? customer.mobileno : "");
                ret.Add("CUSTOMER", customer != null ? customer.name : "");
                ret.Add("RACK", billattributes != null ? billattributes.deliveryrack : "");
                ret.Add("PIECE", totalpiece);
                ret.Add("BILLNO", billno);
                ret.Add("AMOUNT", total);
                return ret;
            }
        }

        public long organizationid { get; set; }
        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("AMOUNT", "F0");
                ret.Add("PIECE", "F0");
                ret.Add("BILLDATE", "dd-MM-yyyy");
                ret.Add("BILLTIME", "hh:mm tt");
                return ret;
            }
        }
    }
}
