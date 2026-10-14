using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class ProductConfig : BaseEntity
    {
        public long id { get; set; }
        public int sellingmode { get; set; }
        public int pricingmode { get; set; }
        public int barcodesource { get; set; }
        public long taxid { get; set; }
        public long producttype { get; set; }
    }
    public class ProductsBase : BaseEntity
    {
        public bool autopo { get; set; }
        public int barcodemode { get; set; }
        public int discountmode { get; set; }
        public decimal discountvalue { get; set; }
        public int sellingmode { get; set; }
        public long id { get; set; }
        public long prodcompanyid { get; set; }
        public long purchasetaxid { get; set; }
        public long salestaxid { get; set; }
        public long salescounterid { get; set; }
        public string hsncode { get; set; }
        public long barcodeid { get; set; }
        public decimal margin { get; set; }
        public decimal minmargin { get; set; }
        public decimal maxmargin { get; set; }
        public int producttype { get; set; }
        public int uomid { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public long productgroup { get; set; }
        public long cesstaxid { get; set; }
        public bool hascess { get;set; }
        public int pricingmode { get; set; }
        public int barcodesource { get; set; }
        public Products.ProductAttributes attributes { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Products.ProductAttributes>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public Products.ProductAttributesList attributelist { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributelist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributelist); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        attributelist = Newtonsoft.Json.JsonConvert.DeserializeObject<Products.ProductAttributesList>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public bool attributefilter { get; set; }

    }

    public class MergeProducts : BaseEntity
    {
        public long id { get; set; }
        public string type { get; set; }
        public List<Products> deletelist { get; set; }

    }
    public class Products : ProductsBase
    {
        public string ErrorMessage { get; set; }
        public bool stdump { get; set; }
        public long prddumpid { get; set; }
        public long coreprdgrpid { get; set; }
        public long basepriceid { get; set; }
        public long prdcoverageid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        
        
        public decimal discountvalue { get; set; }
        public byte[] image { get; set; }
        public string barcodeprefix { get; set; }
        public string barcodesuffix { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string brands_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(brands); } set { brands = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Resource>>(value); } }
        public List<Resource> brands { get; set; }
        public class Resource
        {
            public string name { get; set; }
            public long id { get; set; }
            public bool available { get; set; }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string salesman_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(salesman); } set { salesman = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Resource>>(value); } }
        public List<Resource> salesman { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string pricerange_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(pricerange); } set { pricerange = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PriceRange>>(value); } }
        public List<PriceRange> pricerange { get; set; }
        public class PriceRange
        {
            public double fromvalue { get; set; }
            public double tovalue { get; set; }
            public long id { get; set; }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string keygroup_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(keygroup); } set { keygroup = Newtonsoft.Json.JsonConvert.DeserializeObject<List<KeyGroup>>(value); } }
        public List<KeyGroup> keygroup { get; set; }
        public class KeyGroup
        {
            public long id { get; set; }
            public string name { get; set; }
            public bool option { get; set; }
            public long groupid { get; set; }
        }


        public bool analysing { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string analysingcofig_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(analysingcofig); } set { analysingcofig = Newtonsoft.Json.JsonConvert.DeserializeObject<AnalysCofig>(value); } }
        public AnalysCofig analysingcofig { get; set; }

        public class AnalysCofig
        {
            public List<string> attributes { get; set; }
        }
        public sellingMode SellingMode
        {
            get
            {
                return (sellingMode)sellingmode;
            }
            set
            {
                sellingmode = (int)value;
            }
        }
        public bool isdailyprice { get; set; }
        
        public BarcodeSources BarcodeSource
        {
            get
            {
                return (BarcodeSources)barcodesource;
            }
            set
            {
                barcodesource = (int)value;
            }
        }
        public enum BarcodeSources
        {
            Stock,
            Item,
            Purchase
        }
        public enum sellingMode
        {
            None = 0,
            Piece = 1,
            Pack = 2,
            Cut = 3
        }

        public int stockholdingperiod { get; set; }
        public bool isrefill { get; set; }
        public PurchasePlanMode purchaseplanmode { get; set; }
        public long gender { get; set; }
        public long sizegroupid { get; set; }
        public long unit { get; set; }
        public decimal fixedprice { get; set; }
        public string employeename { get; set; }
        
        public string margins
        {
            get
            {
                return minmargin + "-" + maxmargin;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    decimal ivalue = 0;
                    var part = value.Split('-');
                    decimal.TryParse(part[0], out ivalue);
                    minmargin = ivalue;
                    decimal.TryParse(part.Length > 1 ? part[1] : "0", out ivalue);
                    maxmargin = ivalue;
                }
            }
        }
        public DateTime activatedon { get; set; }
        public DateTime deactivatedon { get; set; }


        public enum PurchasePlanMode
        {
            None = 0,
            SupplierVsPrice = 1,
            BrandVsSize = 2,
            SizeVsPrice = 3
        }
        public class ProductAttributesList
        {
            public List<long> colour { get; set; }
            public List<long> size { get; set; }
            public List<long> style { get; set; }
            public List<long> material { get; set; }
            public List<long> pattern { get; set; }
            public List<long> type { get; set; }
            public List<long> fit { get; set; }
            public List<long> sleeve { get; set; }
            public List<long> brand { get; set; }
        }

        public class ProductAttributes
        {
            public bool brand { get; set; }
            public bool colour { get; set; }
            public bool style { get; set; }
            public bool size { get; set; }
            public bool pattern { get; set; }
            public bool type { get; set; }
            public bool material { get; set; }
            public bool fit { get; set; }
            public bool sleeve { get; set; }
            public bool designid { get; set; }
            public bool purchaseorder { get; set; }
            public bool excludereward { get; set; }
            public bool serialno { get; set; } 
            public bool batchandexpiry { get; set; }
            public bool model { get; set; }
            public bool quality { get; set; }
            public bool warrenty { get; set; }
            public bool item { get; set; }
            public bool colouroption { get; set; }
            public bool multipleprice { get; set; }
            public bool simtype { get; set; }
            public bool uploadbarcode { get; set; }
            public bool weight { get; set; }
            public bool wastage { get; set; }
            public bool workingcharge { get; set; }
            public bool mandatorybrand { get; set; }
            public bool mandatorycolour { get; set; }
            public bool mandatorystyle { get; set; }
            public bool mandatorysize { get; set; }
            public bool mandatorypattern { get; set; }
            public bool mandatorytype { get; set; }
            public bool mandatorymaterial { get; set; }
            public bool mandatoryfit { get; set; }
            public bool mandatorysleeve { get; set; }
            public bool mandatorydesignid { get; set; }

            public bool rolbrand { get; set; }
            public bool rolcolour { get; set; }
            public bool rolstyle { get; set; }
            public bool rolsize { get; set; }
            public bool rolpattern { get; set; }
            public bool roltype { get; set; }
            public bool rolmaterial { get; set; }
            public bool rolfit { get; set; }
            public bool rolsleeve { get; set; }
            public bool roldesignid { get; set; }


            public bool mandatorypurchaseorder { get; set; }
            public bool mandatoryexcludereward { get; set; }
            public bool mandatoryserialno { get; set; }
            public bool mandatorybatchandexpiry { get; set; }
            public bool mandatorymodel { get; set; }
            public bool mandatoryquality { get; set; }
            public bool mandatorywarrenty { get; set; }
            public bool mandatoryitem { get; set; }
            public bool mandatorycolouroption { get; set; }
            public bool mandatorymultipleprice { get; set; }
            public bool mandatorysimtype { get; set; }
            public bool mandatoryweight { get; set; }
            public bool mandatorywastage { get; set; }
            public bool mandatoryworkingcharge { get; set; }
            public List<customfieldattribute> customfields { get; set; }

        }
        public class customfieldattribute
        {
            public string key { get; set; }
            public bool mandatory { get; set; }
            public bool show { get; set; }
            public bool rol { get; set; }
        }
        public long productgroup_1 { get; set; }
        public long productgroup_2 { get; set; }
        public long productgroup_3 { get; set; }
        public long productgroup_4 { get; set; }
        public long productgroup_5 { get; set; }
    }


    public class ProductHSNCode : BaseEntity
    {
        public long productid { get; set; }
        public string hsncode { get; set; }
    }
    public class ProductChangeGroup : BaseEntity
    {
        public bool removeexisting { get; set; }
        public long productgroup { get; set; }
        public List<long> productids { get; set; }
    }
    public class ProductChangeTax : BaseEntity
    {
        public long salestaxid { get; set; }
        public long purchasetaxid { get; set; }
        public List<long> productids { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
    public class ProductsCreteria : Products
    {
        public List<long> ids { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public bool inactive { get;  set; }
        public bool allproducts { get; set; }
        public bool showdailyprice { get; set; }
        public bool showproducttype { get; set; }
        public bool uniqueproduct { get; set; }
        public bool includeconsumables { get; set; }
    }

    public class MergeProductsList : BaseEntity
    {
        public long id { get; set; }
        public string type { get; set; }
        public List<Products> deletelist { get; set; }
        public string ErrorMessage { get; set; }

    }
}
