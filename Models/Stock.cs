using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class OutwardStock : BaseEntity
    {
        public long id { get; set; }
        public string barcode { get; set; }
        public string sellingname { get; set; }
        public decimal stock { get; set; }
    }

    public class Stock : BaseEntity
    {
        public string displayinvoiceno { get; set; }
        public DateTime displayinvoicedate { get; set; }
        public decimal displaymrp { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string printingname { get; set; }
        public decimal price { get; set; }
        public decimal eprice { get; set; }
        public decimal returnqty { get; set; }
        public string barcode { get; set; }
        public long invoiceitemid { get; set; }
        public long itemid { get; set; }
        public string itemcode { get; set; }
        public string itemname { get; set; }
        public long brandid { get; set; }
        public long sizeid { get; set; }
        public string sizename { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public long locationid { get; set; }
        public bool isbit { get; set; }
        public long productid { get; set; }
        public long retaillocationid { get; set; }
        public decimal purchasediscount { get; set; }
        public decimal discount { get; set; }
        public decimal additionaldiscount { get; set; }
        public decimal salediscountpercentage { get; set; }
        public decimal qty { get; set; }
        public string imageid { get; set; }
        public decimal damagedqty { get; set; }
        public decimal journalqty { get; set; }
        public decimal purchasetax { get; set; }
        public decimal transfered { get; set; }
        public long companyid { get; set; }
        public string companyname { get; set; }
        public long lrinvoiceid { get; set; }
        public long supplierid { get; set; }
        public long srcsupplierid { get; set; }
        public string designid { get; set; }
        public decimal basebuyingprice { get; set; }
        public decimal buyingprice { get; set; }
        public decimal dealerprice { get; set; }
        public decimal ibuyingprice { get; set; }
        public decimal buyingqty { get; set; }
        public string lrreference { get; set; }
        public DateTime stockdate { get; set; }
        public string hsncode { get; set; }
        public string newbarcode { get; set; }
        public int sellingmode { get; set; }
        public string typename { get; set; }
        public string sleevename { get; set; }
        public string sectionname { get; set; }
        public string fitname { get; set; }
        public string materialname { get; set; }
        public string pricetagname { get; set; }
        public string colourname { get; set; }
        public string brandname { get; set; }
        public string suppliercode { get; set; }
        //public DateTime invoicedate { get; set; }
        public string batch { get; set; }
        public string serialno { get; set; }
        public long pricetagid { get; set; }
        public DateTime expiry { get; set; }
        public decimal purchaseqty { get; set; }
        public bool runtimeprice { get; set; }
        public long barcodeid { get; set; }
        public bool ismultiprice { get; set; }
        public bool isuan { get; set; }
        public string quantoexistingbarcode { get; set; }
        [JsonIgnore]
        public bool uansubstiated { get; set; }
        [JsonIgnore]
        public decimal sizeinmeter { get; set; }
        public Stockhistory GetStockHistory()
        {
            return new Stockhistory
            {
                isactive = isactive,
                createdby = modifiedby,
                createdon = modifiedon,
                modifiedby = modifiedby,
                modifiedon = modifiedon,
                version = version,
                printingname = printingname,
                price = price,
                returnqty = returnqty,
                barcode = barcode,
                invoiceitemid = invoiceitemid,
                itemid = itemid,
                isbit = isbit,
                productid = productid,
                discount = discount,
                additionaldiscount = additionaldiscount,
                qty = qty,
                damagedqty = damagedqty,
                journalqty = journalqty,
                transfered = transfered,
                designid = designid,
                buyingprice = buyingprice,
                ibuyingprice = ibuyingprice,
                hsncode = hsncode,
                stockid = id
            };
        }
        [JsonIgnore]
        public decimal stockqty
        {
            get
            {
                return qty - (transfered + journalqty + damagedqty + returnqty);
            }
        }
        public Stockuan GetStockUAN()
        {
            return new Stockuan
            {
                isactive = isactive,
                createdby = modifiedby,
                createdon = DateTime.Now,
                modifiedby = modifiedby,
                modifiedon = DateTime.Now,
                version = version,
                price = price,
                barcode = barcode,
                serialno = serialno,
                invoiceitemid = invoiceitemid,
                discount = discount,
                additionaldiscount = additionaldiscount,
                qty = qty,
                transfered = transfered,
                buyingprice = buyingprice,
                stockid = id,
                retaillocationid = retaillocationid

            };
        }
    }

    public class StockUAN : Stock
    {
        public long stockid { get; set; }
    }
    public class StockUANHistory
    {
        public List<Stock> items { get; set; }
    }

    public class StockImage : BaseEntity
    {
        public long lrinvoiceitemid { get; set; }
        public long billitemid { get; set; }
        public int action { get; set; }
        public Actions Action
        {
            get
            {
                return (Actions)action;
            }
            set
            {
                action = (int)value;
            }
        }
        public string image { get; set; }
        public string imageid { get; set; }
        public string barcode { get; set; }
        public bool deleted { get; set; }
        public bool updated { get; set; }
        public bool available { get; set; }
        public Modes mode { get; set; }
        public enum Modes
        {
            StockImage,
            EANImage,
            SKUCover,
            SKUImage
        }
        public enum Actions
        {
            Get,
            Delete,
            Update
        }
    }
    public class StockCreteria : Stock
    {
        public List<long> ids { get; set; }
        public List<string> barcodes { get; set; }
        public List<string> uanbarcodes { get; set; }
        public long bundleid { get; set; }
    }

    public class BarcodeHSN : BaseEntity
    {
        public string barcode { get; set; }
        public string hsn { get; set; }
    }
    public class StockSync : BaseEntity
    {
        public string reason { get; set; }
        public int mode { get; set; }
        public List<Item> uanitems { get; set; }
        public List<Item> items { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }

        public StockItemDetail detail { get; set; }
        public class Item
        {
            public long stockid { set; get; }
            public long salablegoodsid { get; set; }
            public decimal stock { get; set; }
            public string barcode { get; set; }
        }

        public class StockItemDetail : Models.Items
        {
            public decimal price { get; set; }
            public decimal discount { get; set; }
            public string hsncode { get; set; }
            public string designid { get; set; }
        }
    }

    public class Stock4Sync : BaseEntity
    {
        public Stock4Sync()
        {
            Matched = new List<StockMiniReport>();
            Missing = new List<StockMiniReport>();
            Additional = new List<StockMiniReport>();
        }
        public List<StockMiniReport> Matched { get; set; }
        public List<StockMiniReport> Missing { get; set; }
        public List<StockMiniReport> Additional { get; set; }
    }

    public class StockMiniReport : BaseEntity
    {
        public decimal physicalqty { get; set; }
        public decimal systemqty { get; set; }
        public string barcode { get; set; }
        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public string productname { get; set; }
    }

    public class StockFileItem : BaseEntity
    {
        public string barcode { get; set; }
        public string uan { get; set; }
        public long productid { get; set; }
        public decimal qty { get; set; }
        public decimal mqty { get; set; }
    }
    public class StockReport : BaseEntity
    {
        public bool multicompany { get; set; }
        public bool isbit { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }
        public long productgroupid { get; set; }
        public long productid { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public long brandid { get; set; }
        public long itemid { get; set; }
        public DateTime inwardon { get; set; }
        public decimal basebuyingprice { get; set; }
        public decimal pdiscoutpercent { get; set; }
        public decimal pdiscout { get; set; }
        public decimal buyingprice { get; set; }
        public decimal ibuyingprice { get; set; }
        public decimal mrp { get; set; }
        public decimal finalprice { get; set; }
        public decimal rowexpense { get; set; }
        public decimal rowexpensevalue { get; set; }
        public decimal price { get; set; }
        public decimal dealerprice { get; set; }
        public decimal discount { get; set; }
        public decimal addlndiscount { get; set; }
        public decimal schemediscount { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal iqty { get; set; }
        public decimal ipurchaseqty { get; set; }
        public decimal returnqty { get; set; }
        public decimal poqty { get; set; }
        public decimal dqty { get; set; }
        public decimal oqty { get; set; }
        public decimal jqty { get; set; }
        public decimal qty { get; set; }
        public decimal whqty { get; set; }
        public decimal retailqty { get; set; }
        public long typeid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long sectionid { get; set; }
        public long sizeid { get; set; }
        public long sleeveid { get; set; }
        public long fitid { get; set; }
        public long pricetagid { get; set; }
        public string barcode { get; set; }
        public string glncode { get; set; }
        public string itemname { get; set; }
        public string serialno { get; set; }
        public string suppliercity { get; set; }
        public string agentcity { get; set; }
        public long stockid { get; set; }
        public long invoiceitemid { get; set; }
        public long salablegoodsid { get; set; }
        public string brandname { get; set; }
        public string suppliername { get; set; }
        public string suppliergstno { get; set; }
        public string sourcesuppliername { get; set; }
        public string agentname { get; set; }
        public string lrentryno { get; set; }
        public string invoiceno { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime stockdate { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string productname { get; set; }
        public string sizename { get; set; }
        public string materialname { get; set; }
        public string typename { get; set; }
        public string sleevename { get; set; }
        public string fitname { get; set; }
        public string designid { get; set; }
        public string hsncode { get; set; }
        public string colourname { get; set; }
        public string pricetagname { get; set; }
        public string suppliercode { get; set; }
        public int sellingmode { get; set; }
        public string employeename { get; set; }
        public string location { get; set; }
        public DateTime createdon { get; set; }
        public decimal taxablevalue { get; set; }
        public decimal stockvalue { get; set; }
        public decimal istockvalue { get; set; }
        public decimal grosssalevalue { get; set; }
        public decimal salevalue { get; set; }
        public decimal netsalevalue { get; set; }
        public decimal taxvalue { get; set; }
        public decimal taxable { get; set; }
        public decimal margin { get; set; }
        public long markerid { get; set; }
        public long producttype { get; set; }
        public int stockage { get; set; }
        public long agentid { get; set; }
        public long suppliercityid { get; set; }
        public int stockholdingperiod { get; set; }
        public string itbuscode { get; set; }
        public decimal marginpercentage { get; set; }
        public decimal netmarginpercent { get; set; }
        public decimal dealermargin { get; set; }
        public decimal dealermarginpercentage { get; set; }
        public decimal stockpercentage { get; set; }
        public decimal salespercentage { get; set; }
        public decimal markdown { get; set; }
        public decimal salestaxamount { get; set; }
        public decimal salestaxpercent { get; set; }
        public decimal ptaxpercent { get; set; }
        public decimal staxpercent { get; set; }
        public string barcodeidname { get; set; }
        public string entrylocationname { get; set; }
        public bool isinterstatesale { get; set; }
        public List<UANBarcodes> uanstk { get; set; }
        public class UANBarcodes
        {
            public long stockid { get; set; }
            public string barcode { get; set; }
            public long salablegoodsid { get; set; }
            public decimal iqty { get; set; }
            public decimal ipurchaseqty { get; set; }
            public decimal returnqty { get; set; }
            public decimal poqty { get; set; }
            public decimal dqty { get; set; }
            public decimal oqty { get; set; }
            public decimal jqty { get; set; }
            public decimal qty { get; set; }
        }
    }

    
    public class PurchaseHistoryItem
    {
        public string product { get; set; }
        public string detail { get; set; }
        public DateTime entrydate { get; set; }
        public string invoiceno { get; set; }
        public string price { get; set; }

        public decimal margin { get; set; }
        public decimal cost { get; set; }
        public decimal discount { get; set; }
        public decimal qty { get; set; }
        public decimal stock { get; set; }
        public string age { get; set; }
        public decimal bqty { get; set; }
        public decimal bvalue { get; set; }
        public decimal rqty { get; set; }
    }

    public class StockReportCreteria : StockReport
    {
        public bool searchbarcodeonuan { get; set; }
        public int mode { get; set; }
        public int barcodetype { get; set; }
        public bool pattern { get; set; }
        public bool size { get; set; }
        public bool product { get; set; }
        public bool color { get; set; }
        public bool brand { get; set; }
        public bool style { get; set; }
        public bool material { get; set; }
        public bool type { get; set; }
        public bool sleeve { get; set; }
        public bool supplier { get; set; }
        public bool sourcesupplier { get; set; }
        public bool design { get; set; }
        public bool section { get; set; }
        public bool isbarcode { get; set; }
        public bool productgroup { get; set; }
        public bool invoicedate { get; set; }
        public bool netamount { get; set; }
        public bool availqty { get; set; }
        public bool agent { get; set; }
        public bool issuppliercity { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime frominvdate { get; set; }
        public DateTime toinvdate { get; set; }
        public DateTime frompricingdate { get; set; }
        public DateTime topricingdate { get; set; }
        public int purchasetypeid { get; set; }
        public string pricerange { get; set; }
        public string stockrange { get; set; }
        public string saleqtyrange { get; set; }
        public int shelflifeday { get; set; }
        public long sizegroupid { get; set; }
        public List<string> barcodes { get; set; }
        public long actiontype { get; set; }
        public bool exclusivetax { get; set; }
        public string gln { get; set; }
        public bool isgln { get; set; }
        public bool removezerostock { get; set; }
        public bool isb2b { get; set; }
        public bool removemfg { get; set; }
        public bool removebarcoded { get; set; }
        public long invoiceid { get; set; }
        public List<int> fields { get; set; }
        public bool includeproductid { get;  set; }
        public bool fromstockaudit { get; set; }
        public bool allrows { get; set; }
    }


    //public class Barcode2Print : StockReport, Report.IBarcodeFormat
    //{
    //    public string size { get; set; }
    //    public int Index { get; set; }
    //    public int buyingyear { get; set; }
    //    public string printingname
    //    {
    //        get { return itemname; }
    //        set { itemname = value;  }
    //    }
    //    public SortedDictionary<string, object> KeyValue
    //    {
    //        get
    //        {
    //            SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
    //            ret.Add("BARCODE", barcode);

    //            //"BarCode,ProductName,SalesRate,size order,style,brand,SDRate,color"
    //            if (isbit)
    //            {
    //                ret.Add("NPRICE", (price * qty) - (discount * qty));
    //                //ret.Add("SPRICE", (price * qty) - ((discount + additionaldiscount) * qty));
    //                ret.Add("APRICE", (price * qty));
    //                ret.Add("OPRICE", (price * qty));
    //            }
    //            else
    //            {
    //                ret.Add("NPRICE", price - discount);
    //                //ret.Add("SPRICE", price - (discount + additionaldiscount));
    //                ret.Add("APRICE", price);
    //                ret.Add("OPRICE", price);
    //            }

    //            ret.Add("STYLE", stylename);
    //            ret.Add("BRAND", brandname);
    //            ret.Add("COLOUR", colourname);
    //ret.Add("COLOR", colourname);
    //            ret.Add("SUPPLIER", suppliercode);
    //            ret.Add("MATERIAL", materialname);
    //            ret.Add("PATTERN", patternname);
    //            ret.Add("DISCOUNT", discount);
    //            ret.Add("DESIGN", designid);
    //            ret.Add("SIZE", size);
    //            ret.Add("QTY", qty);
    //            //ret.Add("LRNO.SNO", lrreference);
    //            //ret.Add("LRNO", lrreference.Split('.')[0]);
    //            ret.Add("NAME", printingname);
    //            ret.Add("COST", Report.CitizenBarcode.Encrypt(buyingprice.ToString("F0")));
    //            ret.Add("YEAR", Report.CitizenBarcode.Encrypt(buyingyear));
    //            return ret;
    //        }

    //    }

    //    public SortedDictionary<string, string> KeyFormat
    //    {
    //        get
    //        {
    //            SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
    //            ret.Add("OPRICE", "F0");
    //            ret.Add("NPRICE", "F0");
    //            ret.Add("DISCOUNT", "F0");
    //            ret.Add("APRICE", "F0");
    //            ret.Add("SPRICE", "F0");
    //            return ret;
    //        }
    //    }

    //    [Newtonsoft.Json.JsonIgnore]
    //    public int noofcopy { get; set; }

    //    public Barcode2Print Clone()
    //    {
    //        return this.MemberwiseClone() as Barcode2Print;
    //    }

    //}


    public class Stock2Print : Stock, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            if (data.ContainsKey(field)) return true;
            try
            {
                var fields = field.Split(':');
                //SPL:QTYDISCOUNT:NPRICE:QTY:DISCOUNT%
                //SPL:QTYDISCOUNT:NPRICE:1:6%
                //SPL:QTYDISCOUNT:NPRICE:2:12%
                //SPL:QTYDISCOUNT:NPRICE:3:18%
                if (fields[1] == "QTYDISCOUNT" && fields.Length == 5)
                {
                    int qty = 0;
                    decimal discount = 0;
                    decimal value = (decimal)data[fields[2]];
                    if (int.TryParse(fields[3], out qty))
                    {
                        if (fields[4].EndsWith("%"))
                        {
                            if (decimal.TryParse(fields[4].Substring(0, fields[4].Length - 1), out discount))
                            {
                                data.Add(field, Math.Round(value * qty - (value * qty * discount / 100), 0));
                                return true;
                            }
                        }
                        else
                        {
                            if (decimal.TryParse(fields[4], out discount))
                            {
                                data.Add(field, Math.Round(value * qty - discount, 0));
                                return true;
                            }
                        }
                    }
                }
                //SPL:WSRATE:200%
                if (fields[1] == "WSRATE" && fields.Length == 3)
                {

                    decimal multipler = 0;
                    decimal value = (decimal)data[fields[1]];


                    if (fields[2].EndsWith("%"))
                    {
                        if (decimal.TryParse(fields[2].Substring(0, fields[2].Length - 1), out multipler))
                        {
                            data.Add(field, Math.Round(value * multipler / 100));
                            return true;
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(fields[2], out multipler))
                        {
                            data.Add(field, Math.Round(value * multipler, 0));
                            return true;
                        }
                    }
                }

                //SPL:INSERTCHAR:NPRICE:2:00
                if (fields[1] == "INSERTCHAR" && fields.Length == 5)
                {
                    decimal value = (decimal)data[fields[2]];
                    int insertposition = 0;
                    string insertcharactor = fields[4];
                    if (int.TryParse(fields[3], out insertposition))
                    {
                        string newvalue = value.ToString("F0").Substring(0, insertposition - 1) + insertcharactor + value.ToString("F0").Substring(insertposition - 1);
                        data.Add(field, newvalue);
                        return true;
                    }
                }

                //SPL:MRPDISPLAY:300:2:00
                if (fields[1] == "MRPDISPLAY" && fields.Length == 5)
                {
                    decimal multipler = 0;
                    decimal value = (decimal)data[fields[1]];
                    if (fields[2].EndsWith("%"))
                    {
                        if (decimal.TryParse(fields[2].Substring(0, fields[2].Length - 1), out multipler))
                        {
                            value = Math.Round(value * multipler / 100);
                        }
                    }
                    else if (fields[2].EndsWith("+"))
                    {
                        if (decimal.TryParse(fields[2].Substring(0, fields[2].Length - 1), out multipler))
                        {
                            value = Math.Round(value + multipler);
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(fields[2], out multipler))
                        {
                            value = Math.Round(value * multipler, 0);
                        }
                    }
                    int insertposition = 0;
                    string insertcharactor = fields[4];
                    if (int.TryParse(fields[3], out insertposition))
                    {
                        string newvalue = value.ToString("F0").Substring(0, insertposition - 1) + insertcharactor + value.ToString("F0").Substring(insertposition - 1);
                        data.Add(field, newvalue);
                        return true;
                    }
                }
                //SPL:MRPDISPLAY:100%
                if (fields[1] == "MRPDISPLAY" && fields.Length == 3)
                {

                    decimal multipler = 0;
                    decimal value = (decimal)data[fields[1]];


                    if (fields[2].EndsWith("%"))
                    {
                        if (decimal.TryParse(fields[2].Substring(0, fields[2].Length - 1), out multipler))
                        {
                            data.Add(field, Math.Round(value * multipler / 100));
                            return true;
                        }
                    }
                    else if (fields[2].EndsWith("+"))
                    {
                        if (decimal.TryParse(fields[2].Substring(0, fields[2].Length - 1), out multipler))
                        {
                            data.Add(field, Math.Round(value + multipler));
                            return true;
                        }
                    }
                    else
                    {
                        if (decimal.TryParse(fields[2], out multipler))
                        {
                            data.Add(field, Math.Round(value * multipler, 0));
                            return true;
                        }
                    }
                }

                //SPL:PART:SIZE:*:3
                if (fields[1] == "PART" && fields.Length == 5)
                {
                    try
                    {
                        data.Add(field, data[fields[2]].ToString().Split(fields[3][0])[int.Parse(fields[4])]);
                    }
                    catch (Exception exp)
                    {
                        data.Add(field, "ERR");
                    }
                }

                // SPL:SPLIT:NAME:23:1
                if (fields[1] == "SPLIT" && fields.Length == 5)
                {
                    if (KeyValue.ContainsKey(fields[2]))
                    {
                        int length = 0;
                        int position = 0;
                        if (int.TryParse(fields[3], out length) &&
                            int.TryParse(fields[4], out position))
                        {
                            var fullvalue = KeyValue[fields[2]].ToString();
                            int startindex = (position - 1) * length;
                            if (fullvalue.Length < startindex)
                            {
                                data.Add(field, "");
                                return true;
                            }
                            if (startindex + length > fullvalue.Length)
                            {
                                length = fullvalue.Length - startindex;
                            }
                            data.Add(field, fullvalue.Substring(startindex, length));
                            return true;
                        }
                    }
                }

            }
            catch (Exception exp)
            {

            }
            return false;
        }
        public Stock2Print(long organizationid)
        {
            this.organizationid = organizationid;
        }
        public Stock2Print()
        {
        }
        public decimal mrp { get; set; }
        public string marker { get; set; }
        public string companyname { get; set; }
        public string companycode { get; set; }
        public string retaillocationname { get; set; }
        public string retaillocationcode { get; set; }
        public long sourcesupplierid { get; set; }
        public string sourcesuppliercode { get; set; }
        public int OIndex { get; set; }
        public int Index { get; set; }
        public int buyingyear { get; set; }
        public int buyingmonth { get; set; }

        public int entryyear { get; set; }
        public int entrymonth { get; set; }
        public int entryday { get; set; }
        public string gln { get; set; }
        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("BARCODE", barcode);
                ret.Add("GLN", gln);
                ret.Add("UANBARCODE", serialno);
                if (isbit)
                {
                    ret.Add("NPRICE", (price * qty) - (discount * qty));
                    ret.Add("SPRICE", (price * qty) - ((discount + additionaldiscount) * qty));
                    ret.Add("APRICE", (price * qty));
                    ret.Add("OPRICE", (price * qty));

                    ret.Add("NPRICEF2", (price * qty) - (discount * qty));
                    ret.Add("APRICEF2", (price * qty));
                    ret.Add("OPRICEF2", (price * qty));
                }
                else
                {
                    ret.Add("NPRICE", price - discount);
                    ret.Add("SPRICE", price - (discount + additionaldiscount));
                    ret.Add("APRICE", price);
                    ret.Add("OPRICE", price);

                    ret.Add("NPRICEF2", price - discount);
                    ret.Add("APRICEF2", price);
                    ret.Add("OPRICEF2", price);
                }

                if (discount > 0 && price > 0)
                    ret.Add("DISCOUNTPER", Math.Round((discount / price) * 100, 0) + "%");
                else
                    ret.Add("DISCOUNTPER", "0%");

                if (salediscountpercentage > 0)
                    ret.Add("ENTEREDDISCOUNTPER", salediscountpercentage + "%");
                else if (discount > 0 && price > 0)
                    ret.Add("ENTEREDDISCOUNTPER", Math.Floor((discount / price) * 100) + "%");
                else
                    ret.Add("ENTEREDDISCOUNTPER", "0%");

                ret.Add("WSRATE", dealerprice);
                ret.Add("PRODUCTCODE", code_product);
                ret.Add("BRANDCODE", code_brand);
                ret.Add("COLOURCODE", code_colour);
                ret.Add("COLORCODE", code_colour);
                ret.Add("MATERIALCODE", code_material);
                ret.Add("PATTERNCODE", code_pattern);
                ret.Add("SIZECODE", code_size);
                ret.Add("STYLECODE", code_style);
                ret.Add("TYPECODE", code_type);

                ret.Add("PRODUCTGROUPCODE", code_produgroup);
                ret.Add("BARCODEGROUPCODE", code_barcodegroup);
                ret.Add("PRODUCTGROUP", produgroupname);
                ret.Add("BARCODEGROUP", barcodegroupname);

                ret.Add("ITEMCODE", itemcode);
                ret.Add("ITEMNAME", itemname);
                ret.Add("SUPPLIER", suppliercode);
                ret.Add("SSUPPLIER", sourcesuppliercode);
                ret.Add("TYPE", typename);
                ret.Add("DISCOUNT", discount);
                ret.Add("MRP", mrp);
                ret.Add("DESIGN", designid);
                ret.Add("SIZE", sizename);
                ret.Add("STYLE", stylename);
                ret.Add("PATTERN", patternname);
                ret.Add("COLOUR", colourname);
                ret.Add("COLOR", colourname);
                ret.Add("MATERIAL", materialname);
                ret.Add("BRAND", brandname);
                ret.Add("FIT", fitname);
                ret.Add("SLEEVE", sleevename);
                ret.Add("PRICETAG", pricetagname);
                ret.Add("SECTION", sectionname);
                ret.Add("COMPANYCODE", companycode);
                ret.Add("MARKERCODE", marker);
                ret.Add("COMPANYNAME", companyname);
                ret.Add("RLOCATIONNAME", retaillocationname);
                ret.Add("RLOCATIONCODE", retaillocationcode);
                ret.Add("QTY", qty);
                ret.Add("QTYF2", qty.ToString("F2"));
                ret.Add("MRPDISPLAY", buyingprice);
                ret.Add("BASECOSTDISPLAY", basebuyingprice);
                ret.Add("INDEX", Index.ToString("F0"));
                ret.Add("FULLLRNO.SNO", lrreference);
                ret.Add("LRNO", string.IsNullOrEmpty(lrreference) ? "" : lrreference.Split('.')[0]);
                ret.Add("NAME", printingname);
                ret.Add("SIZEGROUP", sizegroup);
                ret.Add("INVOICENO", invoiceno);
                if (string.IsNullOrEmpty(Services.ConfigManager.Instance(organizationid).Config.system_costencode))
                {
                    String ENCODECOST = "";
                    if (buyingprice <= 3000)
                    {
                        ENCODECOST = "D" + ((int)(buyingprice / 5));
                    }
                    else if (buyingprice <= 10000)
                    {
                        ENCODECOST = "S" + ((int)(buyingprice / 25));
                    }
                    else
                    {
                        ENCODECOST = "Z" + ((int)(buyingprice / 50));
                    }
                    ret.Add("ENCODECOST", ENCODECOST);
                }
                else if (!string.IsNullOrEmpty(Services.ConfigManager.Instance(organizationid).Config.system_costencode) &&
                    Services.ConfigManager.Instance(organizationid).Config.system_costencode == "MONTHYEARDRATEDAY*QTY")
                {

                    if (Services.ConfigManager.Instance(organizationid).Config.system_monthencryption.Length != 12)
                        Services.ConfigManager.Instance(organizationid).Config.system_monthencryption = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                    String ENCODECOST = Services.ConfigManager.Instance(organizationid).Config.system_monthencryption[buyingmonth-1].ToString();
                    ENCODECOST += new String(buyingyear.ToString("F0").Reverse().ToArray());
                    ENCODECOST += new String((buyingprice * 2).ToString("F0").Reverse().ToArray());
                    ENCODECOST += buyingday.ToString("F0");
                    ENCODECOST += "*";
                    ENCODECOST += qty.ToString("F0");
                    ret.Add("ENCODECOST", ENCODECOST);
                }

                ret.Add("COST", Report.CitizenBarcode.Encrypt(buyingprice.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("BASECOST", Report.CitizenBarcode.Encrypt(basebuyingprice.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));

                ret.Add("EYEAR", Report.CitizenBarcode.Encrypt(buyingyear.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("EMONTH", Report.CitizenBarcode.Encrypt(buyingmonth.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("EDAY", Report.CitizenBarcode.Encrypt(buyingday.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));

                ret.Add("YEAR", Report.CitizenBarcode.Encrypt(buyingyear, Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("MONTH", Report.CitizenBarcode.Encrypt(buyingmonth, Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("DAY", Report.CitizenBarcode.Encrypt(buyingday, Services.ConfigManager.Instance(organizationid).Config.numberencrypt));


                ret.Add("PRDEXPIRTY", prdexipry);
                ret.Add("ENTRYYEAR", entryyear);
                ret.Add("ENTRYMONTH", entrymonth);
                ret.Add("ENTRYDAY", entryday);

                ret.Add("EENTRYYEAR", Report.CitizenBarcode.Encrypt(entryyear.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("EENTRYMONTH", Report.CitizenBarcode.Encrypt(entrymonth.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                ret.Add("EENTRYDAY", Report.CitizenBarcode.Encrypt(entryday.ToString("F0"), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));

                ret.Add("NYEAR", buyingyear);
                ret.Add("NMONTH", buyingmonth);
                ret.Add("NDAY", buyingday);
                ret.Add("SNO", sno);
                ret.Add("EAPRICE", Report.CitizenBarcode.Encrypt(ret["APRICE"].ToString(), Services.ConfigManager.Instance(organizationid).Config.numberencrypt));
                if (!string.IsNullOrEmpty(lrreference) && lrreference.IndexOf('/') > 0)
                    ret.Add("LRNO.SNO", lrreference.Split('/')[1]);
                else
                    ret.Add("LRNO.SNO", lrreference);
                return ret;
            }
        }

        public long organizationid { get; set; }
        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("OPRICE", "F0");
                ret.Add("NPRICE", "F0");
                ret.Add("DISCOUNT", "F0");
                ret.Add("MRP", "F0");
                ret.Add("APRICE", "F0");
                ret.Add("SPRICE", "F0");
                ret.Add("WSRATE", "F0");
                ret.Add("MRPDISPLAY", "F0");
                ret.Add("ENTRYYEAR", "F0");
                ret.Add("ENTRYMONTH", "F0");
                ret.Add("ENTRYDAY", "F0");

                ret.Add("NPRICEF2", "F2");
                ret.Add("APRICEF2", "F2");
                ret.Add("OPRICEF2", "F2");

                return ret;
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public int noofcopy { get; set; }
        public string code_product { get; set; }
        public string code_size { get; set; }
        public string code_type { get; set; }
        public string code_colour { get; set; }
        public string code_material { get; set; }
        public string code_style { get; set; }
        public string code_brand { get; set; }
        public string code_pattern { get; set; }
        public string code_produgroup { get; set; }
        public string code_barcodegroup { get; set; }
        public string produgroupname { get; set; }
        public string barcodegroupname { get; set; }
        public string prdexipry { get; set; }
        public string sno { get; set; }
        public int buyingday { get; set; }
        public string invoiceno { get; set; }
        public string sizegroup { get; set; }
        public Stock2Print Clone()
        {
            return this.MemberwiseClone() as Stock2Print;
        }
    }



    public class ItemSearchResult : BaseEntity
    {
        public LrinvoiceCover lrinvoice { get; set; }
        public StockWithTax stock { get; set; }
        public List<Salablegoods> salablegoods { get; set; }
        public List<Billitems> billitems { get; set; }
        public List<LrinvoiceItemDetail> invoiceitems { get; set; }
        public List<BundleItemTransferHistory> transferHistories { get; set; }
        public List<Salablegoodshistory> audithistory { get; set; }
        public List<Pricechangehistory> pricehistory { get; set; }
        public List<BillitemsB2B> b2bbillitems { get; set; }
        public JobWorkDetails jobWorkdetails { get; set; }
        public List<Lrinvoicereturnitemdetail> returnhistory { get; set; }
    }


    public class ItemSearchCreteria : BaseEntity
    {
        public int mode { get; set; }
        public string barcode { get; set; }
        public string designid { get; set; }
        public long productid { get; set; }
        public long styleid { get; set; }
        public long sizeid { get; set; }
        public long supplierid { get; set; }
        public long brandid { get; set; }
        public long patternid { get; set; }
        public long customerid { get; set; }
        public string mobileno { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }
        public long typeid { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public decimal billamtto { get; set; }
        public decimal billamtfrom { get; set; }
        public decimal pricefrom { get; set; }
        public decimal priceto { get; set; }

        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public List<string> barcodes { get; set; }
        public bool includeTranHistory { get; set; }
    }

    public class StockWithTax : Stock
    {
        public string taxname { get; set; }
        public long taxid { get; set; }
        public long salestaxid { get; set; }
        public decimal taxpercentage { get; set; }
        public DateTime invoicedate { get; set; }
        public decimal sno { get; set; }
        public decimal purchasediscountpercentage { get; set; }
        public string itemname { get; set; }
        public decimal dealerprice { get; set; }
        public decimal minrate { get; set; }
        public decimal mrp { get; set; }
        public decimal marginpercent { get; set; }
        public decimal netmarginpercent { get; set; }
        public decimal salestaxpercent { get; set; }
        public decimal salestaxamount { get; set; }
        public string displaycode { get; set; }
        [JsonIgnore]
        public long salablegoodsid { get; set; }
    }
    public class OutwardSearchCreteria : BaseEntity
    {
        public enum SearchMode
        {
            Barcode,
            Brand,
            Product,
            BarcodedOn,
            AllStock,
            LRNo
        }
        public SearchMode mode { get; set; }
        public long brandid { get; set; }
        public long productid { get; set; }
        public bool showallstock { get; set; }
        public DateTime barcodedfrom { get; set; }
        public DateTime barcodedto { get; set; }
        public string lrentryno { get; set; }
        public string barcode { get; set; }
    }
}
