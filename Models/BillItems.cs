using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Billitems : BaseEntity
    {
        public string salesmancode { get; set; }
        public int orderid { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string barcode { get; set; }
        public long stockid { get; set; }
        public long productid { get; set; }
        public string printingname { get; set; }
        [JsonIgnore]
        public string productname { get; set; }
        public string customer { get; set; }
        public string description
        {
            get
            {
                if (string.IsNullOrEmpty(printingname)) return barcode;
                else
                    return string.Concat(barcode.Trim(), "/", printingname.Trim());
            }
        }
        public string itemdetail
        {
            get
            {
                string str = "";
                if (attributes != null)
                {
                    if (!string.IsNullOrEmpty(attributes.brand))
                        str = string.Concat(str, attributes.brand.Trim());
                    if (!string.IsNullOrEmpty(attributes.designid))
                        str = string.Concat(str, "/", attributes.designid.Trim());
                    if (!string.IsNullOrEmpty(attributes.size))
                        str = string.Concat(str, "/", attributes.size.Trim());
                    if (!string.IsNullOrEmpty(attributes.colourname))
                        str = string.Concat(str, "/", attributes.colourname.Trim());
                    if (!string.IsNullOrEmpty(attributes.materialname))
                        str = string.Concat(str, "/", attributes.materialname.Trim());
                }
                return str;
            }
        }
        public decimal incentive { get; set; }
        [System.ComponentModel.Description("tax = qty * itemtax")]
        public decimal tax { get; set; }

        // One qty tax
        [System.ComponentModel.Description("price-rate")]
        public decimal itemtax { get; set; }

        // One qty final rate that is after discount and without tax
        [System.ComponentModel.Description("price/100+tax*100")]
        public decimal rate { get; set; }

        [System.ComponentModel.Description("Price*Qty, All item")]
        public decimal receivable { get; set; }

        // One qty price which is after discount but including tax
        [System.ComponentModel.Description("Price = salerate - discount")]
        public decimal price { get; set; }

        // Total Discount that is qty * actual discount (we don't hold one item discount)
        [System.ComponentModel.Description("Discount = Price - , Per item , 200 - 20%")]
        public decimal discount { get; set; }

        // One qty sale rate including tax with out discount
        [System.ComponentModel.Description("fixed at warehouse including tax , Per item - 1000")]
        public decimal salerate { get; set; }

        [System.ComponentModel.Description("qty*rate")]
        public decimal saleamount { get; set; }


        public bool isexchange { get; set; }
        public bool runtimeprice { get; set; }
        [System.ComponentModel.Description("Exclusive tax of price")]
        public decimal etprice { get; set; }
        [System.ComponentModel.Description("Exclusive tax of discount")]
        public decimal etdiscount { get; set; }
        public decimal etgross
        {
            get
            {
                return etprice * qty;
            }
        }
       
        [JsonIgnore]
        public decimal computed_etprice
        {
            get
            {
                var cetprice = Math.Round((receivable - tax) / qty, 6);
                if (qty < 0)
                    cetprice = Math.Abs(etprice);
                return cetprice;
            }
        }
        [JsonIgnore]
        public decimal computed_etgross
        {
            get
            {
                return computed_etprice*qty;
            }
        }
        [JsonIgnore]
        public decimal mrp
        {
            get;
            set;
        }
        public decimal mrpdisplay
        {
            get
            {

                if (attributes != null && attributes.mrp > 0 && attributes.mrp >= salerate)
                {
                    return attributes.mrp;
                }
                if (mrp > salerate) return mrp;
                return salerate;
            }
        }
        [JsonIgnore]
        public decimal? meteramount
        {
            get; set;
        }
        public decimal amount
        {
            get
            {
                if (meteramount.HasValue)
                    return meteramount.Value;
                return price * qty;
            }
        }

        [System.ComponentModel.Description("discount/qty+additionaldiscount+rate")]
        public decimal ratewithdiscount
        {
            get
            {
                return Math.Round(rate + discountwithouttax,2);
            }
        }

        public decimal discountwithouttax
        {
            get
            {
                var currentdiscount = discount > 0 ? discount / qty : 0;
                currentdiscount += additionaldiscount;
                if (taxpercentage > 0)
                {
                    currentdiscount = (currentdiscount / (100 + taxpercentage)) * 100;
                }
                return Math.Round(currentdiscount,2);
            }
        }

        [System.ComponentModel.Description("receivable - tax + discount")]
        public decimal totalratewithdiscount
        {
            get
            {
                return ratewithdiscount * qty;
            }
        }
        [System.ComponentModel.Description("additionaldiscount = Rate % AddtionalDiscountRate, Per piece")]
        public decimal additionaldiscount { get; set; }
        public decimal schemediscount { get; set; }
        [JsonIgnore]
        public decimal aschemediscount
        {
            get
            {
                if (attributes != null && attributes.aschemediscount > 0)
                    return attributes.aschemediscount;
                return 0;
            }
        }
        [JsonIgnore]
        public decimal rschemediscount
        {
            get
            {
                return schemediscount - aschemediscount;
            }
        }
        public decimal schemereward { get; set; }
        public decimal schemediscountcoupen { get; set; }
        public bool schemeeligble { get; set; }
        public long schemeid { get; set; }
        public string scheme { get; set; }
        public bool hasscheme
        {
            get
            {
                return schemeid > 0;
            }
        }
        [System.ComponentModel.Description("Tax Percentage")]
        public decimal taxpercentage { get; set; }
        [System.ComponentModel.Description("Discount Percentage")]
        public decimal discountpercentage { get; set; }
        [System.ComponentModel.Description("Additional Discount Percentage")]
        public decimal additionaldiscountpercentage { get; set; }
        public long salesmanid { get; set; }
        public string salesmanname { get; set; }
        public long taxid { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long billid { get; set; }
        public long returnbillitemid { get; set; }
        public long rtnsrcitemid { get; set; }
        public decimal qty { get; set; }
        [System.ComponentModel.Description("stock adjustment ")]
        public decimal journalqty { get; set; }
        public decimal billedqty { get; set; }
        [System.ComponentModel.Description("no of pieces")]
        public decimal piece { get; set; }

        [JsonIgnore]
        public string qtyinpiece
        {
            get; set;
        }
        [JsonIgnore]
        public string qtyinmeter
        {
            get; set;
        }
        [JsonIgnore]
        public string qtytometer
        {
            get
            {
                if (sellingmode == 3)
                    return qty.ToString("N2");
                return "";
            }
        }
        

        [JsonIgnore]
        public decimal gross
        {
            get
            {
                return receivable - tax;
            }
        }

        [JsonIgnore]
        public decimal b2bgross
        {
            get
            {
                return (attributes != null && attributes.gross > 0) ? attributes.gross : (salerate + additionaldiscount);
            }
        }

        [JsonIgnore]
        public string qtytopiece
        {
            get
            {
                if (sellingmode != 3)
                    return qty.ToString("N0");
                return "";
            }
        }

        [JsonIgnore]
        public decimal costprice
        {
            get
            {
                return salerate * qty;
            }
        }

        [JsonIgnore]
        public decimal itemtaxable
        {
            get
            {
                return rate - itemtax;
            }
        }

        public bool haslastbit
        {
            get
            {
                return journalqty != 0;
            }
        }
        public decimal lastbitqty
        {
            get
            {
                if (journalqty > 0 && qty > 0)
                    return journalqty - qty;
                else if (journalqty > 0 && qty < 0)
                    return journalqty - (qty * -1);
                return 0;
            }
        }
        //[{ 'id': 1, 'name': 'Piece' }, { 'id': 2, 'name': 'Pack' }, { 'id': 3, 'name': 'Cut' }];
        public int sellingmode { get; set; }
        public long salablegoodsid { get; set; }
        //[JsonIgnore]
        //[JsonProperty(Required = Required.Default)]
        public TaxSplit taxsplit { get; set; }

        public decimal schemediscountpercentage
        {
            get
            {
                if (schemediscount <= 0 || qty == 0) return 0;
                return Math.Round(schemediscount / qty / salerate * 100, 2);
            }
        }
        public decimal additionalandschemediscountpercentage
        {
            get
            {
                return additionaldiscountpercentage + schemediscountpercentage;
            }
        }
        public decimal totaldiscountpercentage
        {
            get
            {
                return discountpercentage + additionaldiscountpercentage + schemediscountpercentage;
            }
        }
        public bool hasdiscountwithmrp { get { return discountwithmrp != 0; } }
        public decimal discountwithmrppercentage
        {
            get
            {
                if (attributes != null && attributes.mrp > 0)
                {
                    if (attributes.mrp <= salerate) return totaldiscountpercentage;
                    return Math.Round((attributes.mrp - salerate) / attributes.mrp * 100, 2) + totaldiscountpercentage;
                }
                return totaldiscountpercentage;
            }
        }
        public decimal discountwithmrp
        {
            get
            {
                if (attributes != null && attributes.mrp > 0)
                {
                    if (attributes.mrp <= salerate) return totaldiscount;
                    return Math.Round(((attributes.mrp - salerate) * qty) + totaldiscount, 2);
                        
                }
                return totaldiscount;
            }
        }

        public bool hasdiscount { get { return totaldiscount != 0; } }
        public decimal totaldiscount
        {
            get
            {
                return Math.Round(discount + (additionaldiscount * qty), 2);
            }
        }
        public decimal netprice
        {
            get
            {
                if (qty > 0)
                    return Math.Round(salerate - ((discount / qty) + additionaldiscount));
                else if (qty < 0 && discount < 0)
                    return Math.Round(salerate - ((discount / qty) + additionaldiscount));
                return Math.Round(salerate - additionaldiscount);
            }
        }
        public decimal grossprice
        {
            get
            {
                if (qty > 0)
                    return Math.Round(salerate - (discount / qty));
                return Math.Round(salerate);
            }
        }
        public bool hasserial
        {
            get
            {
                return attributes != null && !string.IsNullOrEmpty(attributes.serialno);
            }
        }


        public string hsncode { get; set; }


        public BillItemAttributes attributes { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); }
            set
            {
                if (!string.IsNullOrEmpty(value) && value != "null")
                    attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<BillItemAttributes>(value);
            }
        }
        [JsonIgnore]
        public MfgDesignItem mfgdesign { get; set; }

        public decimal getactualreceivable()
        {
            return (price * qty);
        }
        [JsonIgnore]
        public ItemTaxDetail taxdetail { get; set; }
        [JsonIgnore]
        public List<SalesorderitemsDetail.SizeQty> jumpqty { get;  set; }
        public class ItemTaxDetail
        {
            public decimal totaltax { get; set; }
            public decimal cgst_percentage { get; set; }
            public decimal sgst_percentage { get; set; }
            public decimal igst_percentage { get; set; }
            public decimal cgst_value { get; set; }
            public decimal sgst_value { get; set; }
            public decimal igst_value { get; set; }
        }
    }
    public class BillitemsCreteria : Billitems
    {
        public List<long> billids { get; set; }
        public List<long> ids { get; set; }
        public List<long> salablegoodsids { get; set; }
    }

    public class BillItemUAN
    {
        public List<Billitems> childs { get; set; }
        public string uan { get; set; }
        public long stockid { get; set; }
        public decimal qty { get; set; }
        public List<long> ids { get;  set; }
        public bool stockmissmatch { get; internal set; }
    }

    public class BillItemUANStock
    {
        public string uan { get; set; }
        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public decimal stockqty { get; set; }
        public decimal consumed { get; set; }
        public string barcode { get;  set; }
        public decimal availableqty
        {
            get
            {
                return stockqty - consumed;
            }
        }
    }

    public class BillItemAttributes
    {
        public class FreeItems
        {
            public string name { get; set; }
            public long itemid { get; set; }
            public long productid { get; set; }
            public long markerid { get; set; }
            public decimal qty { get; set; }
            public decimal amount { get; set; }
        }
        public string smcode { get; set; }
        public string serialno { get; set; }
        public decimal customerdiscount { get; set; }
        public decimal margin { get; set; }
        public decimal cost { get; set; }
        public bool lossprice { get; set; }
        public decimal gross { get; set; }
        public string brand { get; set; }
        public string size { get; set; }
        public string designid { get; set; }
        public string sizegroup { get; set; }
        public string colourname { get; set; }
        public string colourcode { get; set; }
        public string typename { get; set; }
        public string materialname { get; set; }
        public string stylename { get; set; }
        public string patternname { get; set; }
        public string groupkey { get; set; }
        public string groupname { get; set; }
        public string foodcounter { get; set; }
        public List<itemqty> so { get; set; }
        public List<long> smcodes { get; set; }
        public class itemqty
        {
            public long id { get; set; }
            public decimal qty { get; set; }
        }
        public List<FreeItems> freeitems { get; set; }
        public long schemeid { get; set; }
        public decimal schemediscount { get; set; }
        public decimal coupondiscount { get; set; }
        public decimal coupondiscountpercentage { get; set; }
        public decimal rewarddiscount { get; set; }
        public decimal rewarddiscountpercentage { get; set; }
        public decimal mrp { get; set; }
        public decimal packing { get; set; }
        public long boxid { get; set; }
        public string obarcode { get; set; }
        [JsonIgnore]
        public string boxname { get; set; }
        public decimal newstock { get; set; }
        public decimal newolstock { get; set; }
        public bool isolstock { get; set; }
        public bool islastbit { get; set; }
        public decimal lastbitqty { get; set; }
        public long aschemeid { get; set; }
        public decimal aschemediscount { get; set; }
    }
    public class Billitemdetails : Billitems
    {
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public string location { get; set; }
        public string returnimageid { get; set; }
        public string stockimageid { get; set; }
    }

    public class BillitemsB2B : BaseEntity
    {
        public string billno { get; set; }
        public DateTime billdate { get; set; }
        public decimal billvalue { get; set; }
        public string location { get; set; }
        public string returnimageid { get; set; }
        public string stockimageid { get; set; }
        public long productid { get; set; }
        public int sellingmode { get; set; }
        public long id { get; set; }
        public string barcode { get; set; }
        public string printingname { get; set; }
        public string customer { get; set; }
        public string salesmanname { get; set; }
        public string salesmancode { get; set; }
        public string hsncode { get; set; }
        public decimal piece { get; set; }
        public decimal journalqty { get; set; }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal price { get; set; }
        public decimal tax { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal receivable { get; set; }
        public int orderid { get; set; }
        public string designid { get;  set; }
        public long brandid { get;  set; }
    }

    public class Billitemsimulation
    {
        public long id { get; set; }
        public long taxid { get; set; }
        public int taxtype { get; set; }
        public decimal price { get; set; }
        public decimal discount { get; set; }
    }
}
