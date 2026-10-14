using InfyPOS.Common;
using InfyPOS.Processors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace InfyPOS.Models
{
    public class MfgdesignCode : BaseEntity
    {
        public string code { get; set; }
        public string name { get; set; }
        public string displayname { get; set; }
        public long productid { get; set; }
        public long fabricid { get; set; }
        public long id { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string sizegroup_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(sizegroup); } set { sizegroup = Newtonsoft.Json.JsonConvert.DeserializeObject<Mfgdesign.Sizegroup>(value); } }
        public Mfgdesign.Sizegroup sizegroup { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string imagelist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(imagelist); } set { imagelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(value); } }
        public List<string> imagelist { get; set; }
    }

    public class ComboConfig : BaseEntity
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("qty")]
        public int Qty { get; set; }

        [JsonProperty("colourformat", NullValueHandling = NullValueHandling.Ignore)]
        public string ColourFormat { get; set; }

        [JsonProperty("sizeformat", NullValueHandling = NullValueHandling.Ignore)]
        public string SizeFormat { get; set; }

        [JsonProperty("configlist")]
        public List<ConfigurationItem> Configlist { get; set; }

        [JsonProperty("uanlist", NullValueHandling = NullValueHandling.Ignore)]
        public List<ComboUANConfig> UANList { get; set; }
        public class ComboUANConfig
        {
            public string uan { get; set; }
            public long productid { get; set; }
            public bool optional { get; set; }
            public decimal qty { get; set; }
            public List<string> uans { get; set; }
        }
        public class ComboConfiguration : ConfigurationItem
        {
            public long mfgdesignid { get; set; }
            public string mfgdesignname { get; set; }
        }
        public partial class ConfigurationItem
        {
            [JsonProperty("productid")]
            public long Productid { get; set; }
            [JsonProperty("qty")]
            public int Qty { get; set; }

            [JsonProperty("productqty")]
            public decimal Productqty { get; set; }

            [JsonProperty("primary_colour", NullValueHandling = NullValueHandling.Ignore)]
            public bool PrimaryColour { get; set; }

            [JsonProperty("primary_size", NullValueHandling = NullValueHandling.Ignore)]
            public bool PrimarySize { get; set; }

            [JsonProperty("primary_sleeve", NullValueHandling = NullValueHandling.Ignore)]
            public bool PrimarySleeve { get; set; }

            [JsonProperty("secondary_size", NullValueHandling = NullValueHandling.Ignore)]
            public bool SecondarySize { get; set; }
            [JsonProperty("third_size", NullValueHandling = NullValueHandling.Ignore)]
            public bool ThirdSize { get; set; }
            [JsonProperty("limit_design", NullValueHandling = NullValueHandling.Ignore)]
            public bool Limitdesign { get; set; }
            [JsonProperty("designlist", NullValueHandling = NullValueHandling.Ignore)]
            public string DesignList { get; set; }
        }
    }



    public class MfgDesignItem
    {
        public long id { get; set; }
        public long sizeid { get; set; }
        public long colourid { get; set; }
        public long sleeveid { get; set; }
        public bool alloworder { get; set; }
        public string code { get; set; }
        public string uid { get; set; }
        public string sizegroup { get; set; }
        public string size { get; set; }
        public string sleeve { get; set; }
        public string colour { get; set; }
        public string colourcode { get; set; }
        public decimal wsrate { get; set; }
        public decimal dealerprice { get; set; }
        public decimal salerate { get; set; }
        public decimal mrp { get; set; }
        public string designitemcode { get; set; }
        public string hsn { get; set; }
        public long productid { get; set; }
        public string printingname { get; set; }
    }
    public class Mfgdesign : MfgdesignCode
    {
        public int activestatus { get; set; }
        [JsonIgnore]
        public ActiveStatus Activestatus
        {
            get
            {
                return (ActiveStatus)activestatus;
            }
            set
            {
                activestatus = (int)value;
            }
        }

        public decimal cutratio { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long sleeveid { get; set; }
        public int autonumber { get; set; }
        public long colourid { get; set; }
        public long supplierid { get; set; }
        public long seasonid { get; set; }
        public long fitid { get; set; }
        public long typeid { get; set; }
        public long styleid { get; set; }
        public int pack { get; set; }
        public int mfgmonth { get; set; }
        public int mfgyear { get; set; }
        public long patternid { get; set; }
        public long materialid { get; set; }
        public long boxid { get; set; }
        public bool isactive { get; set; }
        public bool hascombo { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long prddivisionid { get; set; }
        public long taxid { get; set; }
        public long classid { get; set; }
        public bool iscoredesign { get; set; }
        public int version { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int sizemode { get; set; }
        public string colour { get; set; }
        public string sizegroupname { get; set; }
        public string racknumber { get; set; }
        public string prdhsncode { get; set; }
        public ComboConfig comboconfig { get; set; }

        public List<CustomFieldValue> CustomFields { get; set; }
        public class CustomFieldValue
        {
            public string key { get; set; }
            public string value { get; set; }
            public string n { get; set; }
        }
        public enum SizeMode
        {
            SizeRanges,
            SizeGroup,
            SingleSize
        }
        public class Sizegroup : BaseEntity
        {
            public class ColourItem
            {
                public string name { get; set; }
                public long colourid { get; set; }
                public long styleid { get; set; }
                public string imageid { get; set; }
                public long autonumber { get; set; }
            }
            public List<ColourItem> colouritems { get; set; }
            public List<Item> items { get; set; }
            public List<Item> ditems { get; set; }
            public class PrintItem : Item
            {
                public decimal mrp { get; set; }
                public int pqty { get; set; }
                public long designid { get; set; }
            }



            public SearchItem Create(Item item, Mfgdesign source)
            {
                return new SearchItem()
                {
                    productid = source.productid,
                    brandid = source.brandid,
                    fitid = source.fitid,
                    styleid = source.styleid,
                    materialid = source.materialid,
                    patternid = source.patternid,
                    typeid = source.typeid,
                    boxid = source.boxid,
                    code = source.code,
                    uid = item.uid,
                    name = item.name,
                    cost = item.cost,
                    colourid = item.colourid,
                    sizeid = item.sizeid,
                    sleeveid = item.sleeveid,
                    itemid = item.itemid,
                    salerate = item.salerate,
                    wsrate = item.wsrate,
                    dealerprice = item.dealerprice,
                    mrp = item.mrp,
                };
            }
            public class SearchItem : Item
            {
                public long productid { get; set; }
                public long brandid { get; set; }
                public long fitid { get; set; }
                public long styleid { get; set; }
                public long materialid { get; set; }
                public long patternid { get; set; }
                public long typeid { get; set; }
                public long boxid { get; set; }
                public string colourcode { get; set; }
                public string pname { get; set; }
                public List<string> uids { get; set; }
                public string img { get; set; }
            }
            public class Item : BaseEntity
            {
                public Item()
                {
                    alloworder = true;
                }
                public long id { get; set; }
                public long itemid { get; set; }
                public string prdsku { get; set; }
                public string prdhsn { get; set; }
                public string code { get; set; }
                public string name { get; set; }
                public long sleeveid { get; set; }
                public long sizeid { get; set; }
                public long colourid { get; set; }
                public long autonumber { get; set; }
                public string uid { get; set; }
                public string designitemcode { get; set; }
                public bool combo { get; set; }
                public string sizegroup { get; set; }
                public decimal qty { get; set; }
                public decimal jobcharge { get; set; }
                public decimal cost { get; set; }
                public decimal wsrate { get; set; }
                public decimal dealerprice { get; set; }
                public decimal salerate { get; set; }
                public decimal mrp { get; set; }
                public bool alloworder { get; set; }
                [JsonIgnore]
                public bool modified { get; set; }
                public List<string> barcodes { get; set; }
                [JsonIgnore]
                public decimal stock { get; set; }
                [JsonIgnore]
                public long productid { get; set; }
                [JsonIgnore]
                public string pname { get; set; }
                [JsonIgnore]
                public string hsn { get; set; }
            }
        }
    }

    public class MFGDesinInfo
    {
        public long productid { get; set; }
        public long brandid { get; set; }
        public long materialid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long typeid { get; set; }
        public long boxid { get; set; }
        public long fitid { get; set; }
        public long sleeveid { get; set; }

        public string product { get; set; }
        public List<ColourInfo> colourlist { get; set; }
        public string sizegroupid { get; set; }
        public List<SizeGroup> sizegroup { get; set; }
        public string brand { get; set; }
        public int minimumqty { get; set; }
        public long fabricid { get; set; }
        public List<Tuple<string, List<string>>> sizelist { get; set; }
        public string design { get; set; }
        public long designid { get; set; }
        public long colourid { get; set; }
        public decimal salerate { get; set; }
        public decimal rate { get; set; }
        public decimal mrp { get; set; }
        public decimal wsrate { get; set; }
        public long sizeid { get; set; }
        public bool auto { get; set; }
        public string designname { get; set; }
        public bool iscoredesign { get;  set; }

        public class ColourInfo
        {
            public string designcode { get; set; }
            public string code { get; set; }
            public string displaycode { get; set; }
            public string name { get; set; }
            public long id { get; set; }
        }
        public class SizeGroup
        {
            public string id { get; set; }
            public string name { get; set; }
            public string designcode { get; set; }
            public decimal rate { get; set; }
            public decimal wsrate { get; set; }
            public decimal salerate { get; set; }
            public decimal mrp { get; set; }
            public long colourid { get; set; }
            public bool diffprice { get; set; }
            public List<SizeItem> list { get; set; }
            public string colourcode { get; set; }
            public string colourshade { get; set; }
        }

        public class SizeItem
        {
            public long id { get; set; }
            public string name { get; set; }
            public string uid { get; set; }
            public long colourid { get; set; }
            [JsonIgnore]
            public decimal oqty { get; set; }
            //[JsonIgnore]
            //public decimal fg { get; set; }
            public decimal stock { get; set; }
            public decimal wip { get; set; }
            public decimal order { get; set; }
            public decimal rate { get; set; }
            public decimal wsrate { get; set; }
            public decimal mrp { get; set; }
            public decimal salerate { get; set; }
        }
    }
    public class MfgdesignCreteria : Mfgdesign
    {
        public int imagestatus { get; set; }
        public int coredesigntype { get; set; }
        public bool onlycombo { get; set; }
        public bool onlydesign { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool includedetail { get; set; }
        public bool includecolours { get; set; }
        public List<string> codes { get; set; }
        public List<long> ids { get; set; }
        public string uancode { get; set; }
        public string skucode { get; set; }
        public long sizeid { get; set; }
        public long sectionid { get; set; }
    }



    public class MfgGalleryCreteria : BaseEntity
    {
        public long productid { get; set; }
        public long brandid { get; set; }
        public long styleid { get; set; }
        public long patternid { get; set; }
        public long materialid { get; set; }
        public long colourid { get; set; }
        public long typeid { get; set; }
        public long sizeid { get; set; }
        public long seasonid { get; set; }
        public long fitid { get; set; }
        public long salesorderid { get; set; }
        public string designid { get; set; }
        public string possizelist { get; set; }
        public long sizegroupid { get; set; }
        public long assortedsizegroupid { get; set; }
        public long locationid { get; set; }
        public long classid { get; set; }
        public long salesmanid { get; set; }
        public bool onlyimage { get; set; }
        public bool onlystock { get; set; }
        public bool onlywip { get; set; }
        public bool onlyso { get; set; }
        public bool onlyassorted { get; set; }
        public bool corestatusid { get; set; }
        public bool assortedzero { get; set; }
        public int valuesource { get; set; }
        public class ImagePDFRequest : BaseEntity
        {
            public MfgGalleryCreteria creteria { get; set; }
            public int RowCount { get; set; }
            public int ImageWidth { get; set; }
            public int DesignWidth { get; set; }
            public int ImageQuality { get; set; }
            public int DetailPosition { get; set; }
            public bool HasRowLabel
            {
                get
                {
                    return Detail == EnumDetail.Top || Detail == EnumDetail.Bottom;
                }
            }
            public bool HasColumnLabel
            {
                get
                {
                    return Detail == EnumDetail.Left || Detail == EnumDetail.Right;
                }
            }
            public EnumDetail Detail
            {
                get
                {
                    return (EnumDetail)DetailPosition;
                }
                set
                {
                    DetailPosition = (int)value;
                }
            }
            public enum EnumDetail
            {
                None,
                Top,
                Bottom,
                Left,
                Right
            }
            public MfgGallery datasource { get; set; }
        }
    }

    public class MfgGallery
    {
        public List<MfgGalleryItem> items { get; set; }
        public List<ReferenceIdName> colourlist { get; set; }
        public List<ReferenceIdName> sizelist { get; set; }
        [JsonIgnore]
        public List<ReferenceIdName> assortedsizes { get; set; }
        public Salesorder salesorder { get; set; }
        public List<MfgGalleryColourGroup> colourgroup { get; internal set; }
    }
    public class MfgGalleryColourGroup
    {
        public string code { get; set; }
        public List<string> designs { get; set; }
    }
    public class MfgGalleryItem
    {
        public string designitemcode { get; set; }
        public long colourid { get; set; }
        public string code { get; set; }
        public string imgid { get; set; }
        public string mimgid { get; set; }
        public List<SizeStock> sizes { get; set; }
        public List<SizeStockDetail> stksizes { get; set; }
        public class SizeStock
        {
            public long id { get; set; }
            [JsonIgnore]
            public string uid { get; set; }
            [JsonIgnore]			
            public decimal so { get; set; }
            public decimal stock { get; set; }
            public decimal wip { get; set; }
        }
        public class SizeStockDetail
        {
            public string uid { get; set; }
            public long id { get; set; }
            public decimal stk { get; set; }
            public decimal so { get; set; }
            public decimal wip { get; set; }

            public decimal rate { get; set; }
            public decimal wsrate { get; set; }
            public decimal salerate { get; set; }
            public decimal mrp { get; set; }
        }
    }
    public class Design2Print : BaseEntity, Report.IBarcodeFormat
    {
        [JsonIgnore]
        public long billid { get; set; }
        [JsonIgnore]
        public string groupkey { get; set; }
        public virtual bool SpecialField(string field, SortedDictionary<string, Object> data)
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
        public string name { get; set; }
        public string colour { get; set; }
        public string size { get; set; }
        public long fitid { get; set; }
        public long sleeveid { get; set; }
        public long typeid { get; set; }
        public long patternid { get; set; }
        public long styleid { get; set; }
        public long materialid { get; set; }

        public string fitname { get; set; }
        public string sleevename { get; set; }
        public string typename { get; set; }
        public string patternname { get; set; }
        public string stylename { get; set; }
        public string materialname { get; set; }

        public string fitcode { get; set; }
        public string sleevecode { get; set; }
        public string typecode { get; set; }
        public string patterncode { get; set; }
        public string stylecode { get; set; }
        public string materialcode { get; set; }

        public string colourshade { get; set; }
        public string colourcode { get; set; }
        public string sizecode { get; set; }
        public long colourid { get; set; }
        public string uid { get; set; }
        public long sizeid { get; set; }
        public string sizegroup { get; set; }
        public string design { get; set; }
        public string designcode { get; set; }
        public long id { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public decimal wsrate { get; set; }
        public decimal dealerprice { get; set; }
        public decimal salerate { get; set; }
        public decimal mrp { get; set; }
        public int qty { get; set; }
        [JsonIgnore]
        public virtual SortedDictionary<string, object> KeyValue
        {
            get
            {
                if (sizegroup == null) sizegroup = "";
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("DESIGN", design);
                ret.Add("DESIGNCODE", designcode);
                ret.Add("COLOUR", colour);
                ret.Add("COLOURCODE", colourcode);
                ret.Add("COLOURSHADE", colourshade);

                ret.Add("FIT", fitname);
                ret.Add("FITCODE", fitcode);
                ret.Add("STYLE", stylename);
                ret.Add("STYLECODE", stylecode);
                ret.Add("PATTERN", patternname);
                ret.Add("PATTERNCODE", patterncode);
                ret.Add("TYPE", typename);
                ret.Add("TYPECODE", typecode);
                ret.Add("SLEEVE", sleevename);
                ret.Add("SLEEVECODE", sleevecode);
                ret.Add("MATERIAL", materialname);
                ret.Add("MATERIALCODE", materialcode);

                ret.Add("NAME", name);
                ret.Add("PRODUCT", product);
                ret.Add("BRAND", brand);
                ret.Add("SIZE", size);
                ret.Add("SIZECODE", sizecode);
                ret.Add("SIZEGROUP", sizegroup);
                ret.Add("UID", uid);
                ret.Add("WSRATE", wsrate);
                ret.Add("DEALERRATE", dealerprice);
                ret.Add("SALERATE", salerate);
                ret.Add("MRP", mrp);
                ret.Add("UNIQUESIZEGROUPCOLOURID", design.Replace(" ", "-X-") + "/" + sizegroup.Replace(" ", "-X-") + "/" + colourid);
                ret.Add("UNIQUESIZEGROUPCOLOURSIZEID", design.Replace(" ", "-X-") + "/" + sizegroup.Replace(" ", "-X-") + "/" + colourid + "/" + sizeid);
                ret.Add("ID", id);
                ret.Add("COLOURID", colourid);

                ret.Add("MRPDISPLAY", mrp);
                ret.Add("NPRICE", wsrate);
                return ret;
            }
        }
        [JsonIgnore]
        public virtual SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("ID", "N0");
                ret.Add("CODE", "N0");
                ret.Add("WSRATE", "N0");
                ret.Add("DEALERRATE", "N0");
                ret.Add("SALERATE", "N0");
                ret.Add("MRP", "N0");
                return ret;
            }
        }

    }

    public class MfgdesignProcess : Mfgdesign
    {
        public string status
        {
            get
            {
                return id > 0 ? "OLD" : "NEW";
            }
        }
        public bool isexist { get; set; }
        public int newuid
        {
            get
            {
                if(sizegroup != null && sizegroup.items != null)
                    return sizegroup.items.Count;
                return 0;
            }
        }
    }
    public class SKUUploaded : BaseEntity
    {
        public List<MfgdesignProcess> sku { get; set; }
        public int noofssku { get; set; }
        public int noofnewsku
        {
            get
            {
                return sku.Count(x => x.id == 0);
            }
        }
        public int noofupdatesku
        {
            get
            {
                return sku.Count(x => x.id > 0);
            }
        }
        public int noofuan { get; set; }
        public int noofnewuan
        {
            get
            {
                return noofuan - rmvuan;
            }
        }
        public int rmvuan { get; set; }
        public int similarsku { get; set; }
        public string duplicatedesigns { get; set; }
        public List<string> excelcolumns { get; internal set; }
        public List<MfgDesingImport.ColumnConfiguration> columnConfiguration { get; set; }
        public List<KeyValuePair<string, string>> missingmap { get; internal set; }
        public string error { get; internal set; }
    }
    public class ChangeAttributes : BaseEntity
    {
        public partial class Attributes
        {
            public bool productChanged { get; set; }
            public long productid { get; set; }

            public bool colourChanged { get; set; }

            public long colourid { get; set; }
            public bool patternChanged { get; set; }

            public long patternid { get; set; }

            public bool sizeChanged { get; set; }

            public long sizeid { get; set; }
            public bool sleeveChanged { get; set; }


            public long sleeveid { get; set; }
            public bool brandChanged { get; set; }

            public long brandid { get; set; }
            public bool materialChanged { get; set; }


            public long materialid { get; set; }

            public bool styleChanged { get; set; }

            public long styleid { get; set; }
            public bool typeChanged { get; set; }


            public long typeid { get; set; }

            public bool fitChanged { get; set; }


            public long fitid { get; set; }
        }

        public Attributes criteria { get; set; }
        public Mfgdesign sku { get; set; }
        public List<Mfgdesign.Sizegroup.Item> items { get; set; }
        public long classid { get; set; }
        public bool iscoredesign { get; set; }
        public string mode { get; set; }
        public long locationid { get; set; }
        public long statusid { get; set; }
        public long approvedby { get; set; }
        public DateTime statuson { get; set; }
        public List<long> ids { get; set; }
        public enum SOStatus
        {
            Pending,
            Approved,
            Rejected,
            Cancelled,
            Completed
        }
        public SOStatus Status
        {
            get
            {
                return (SOStatus)statusid;
            }
        }
    }

}
