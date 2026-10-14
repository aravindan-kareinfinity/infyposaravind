using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class ReferenceIdName : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
    }
    public class ReferenceIdNameParent : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public long productgroup { get; set; }
    }
    public class ReferenceItem : ReferenceIdNameParent
    {
        public string code { get; set; }
        public string type { get; set; }
        public string data { get; set; }
        public long locationid { get; set; }
        public string designid { get; set; }
    }

    public class LocationReferenceItem : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public long companyid { get; set; }
        public bool iswarehouse { get; set; }
    }
    public class CounterReferenceItem : ReferenceItem
    {
        public long floorid { get; set; }
        public bool autosettlement { get; set; }
        public bool dotmatrixprint { get; set; }
        public bool online { get; set; }
    }

    public class CustomReferenceItem : ReferenceItem
    {
        public string type { get; set; }
        public int version { get; set; }
    }

    public class ReferenceSectionConfig : Referencelist
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public decimal incentivepercentage { get; set; }
        public bool productwise { get; set; }
    }

    public class ReferenceIncentiveConfig : Referencelist
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public decimal incentivepercentage { get; set; }
        public bool productwise { get; set; }
    }

    public class ManageReference : Referencelist
    {
        public int producttype { get; set; }
        public long purchasetaxid { get; set; }
        public long salestaxid { get; set; }
        public int barcodemode { get; set; }
        public int sellingmode { get; set; }
    }
    public class Referencelist : ReferenceItem
    {
        public string ErrorMessage { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string type { get; set; }
        public long referenceid { get; set; }
        public int sortingindex { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string additionalinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(additionalinfo); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        additionalinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Attribute>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public Referencelist Clone()
        {
            var clone = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Referencelist>(clone);
        }

        public Attribute additionalinfo { get; set; }
        public class Attribute
        {
            public string paytype { get; set; }
            public string formula { get; set; }
            public string licensekey { get; set; }
            public bool deductsalary { get; set; }
            public bool includeinesi { get; set; }
            public bool includeinpf { get; set; }
            public string eligbleday { get; set; }
            public string present { get; set; }
            public int uomid { get; set; }
            public decimal uomvalue { get; set; }
            public string disbursementmode { get; set; }
            public bool disabled { get; set; }
            public string tallyledger { get; set; }
            public string tallyledgerself { get; set; }
            public string tallyledgercompany { get; set; }
            public decimal tallyledgerselfpercent { get; set; }
            public decimal tallyledgercompanypercent { get; set; }
            public InfyPOS.Data.SalaryManager.SalaryHeaderTypes GetPayType()
            {
                InfyPOS.Data.SalaryManager.SalaryHeaderTypes sht = InfyPOS.Data.SalaryManager.SalaryHeaderTypes.None;
                Enum.TryParse<InfyPOS.Data.SalaryManager.SalaryHeaderTypes>(paytype, out sht);
                return sht;
            }
            public bool issales { get; set; }
            public bool isonline { get; set; }
            public string splpaytypelistid { get; set; }
            public decimal fixedincentive { get; set; }
            public decimal dynamicincentive { get; set; }
            public ReportConfiguration reportconfig { get; set; }
            public List<StaticIP> staticip { get; set; }
            public SpecialPayType GetSPLPayType()
            {
                SpecialPayType sht;
                Enum.TryParse<SpecialPayType>(splpaytypelistid, out sht);
                return sht;
            }
            public string fromtime { get; set; }
            public string totime { get; set; }
            public decimal totaltime { get; set; }
            public Boolean iswarehouse { get; set; }
            public Boolean autosettlement { get; set; }
            public long locationid { get; set; }
            public long floorid { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public List<Permission> permissionset { get; set; }
            public List<SalesIncentive> salesincentive { get; set; }
            public List<long> sizegroups { get; set; }
            public List<long> prdgroups { get; set; }
            public List<BilldiscountConfig> billdiscountconfig { get; set; }
            public List<PriceTageConfig> pricetags { get; set; }
            public decimal rewardpoint { get; set; }
            public decimal billdiscount { get; set; }
            public decimal maxlimit { get; set; }
            public bool billdiscountinclude { get; set; }
            public bool otp { get; set; }
            public string address { get; set; }
            public long parentid { get; set; }
            public long printconfigid { get; set; }
            public bool ccmarginmode { get; set; }
            public string marginmode { get; set; }
            public int markdownmode { get; set; }
            public decimal ccmargin { get; set; }
            public decimal ccmarginmrp { get; set; }
            public decimal ccmarginprice { get; set; }
            [JsonIgnore]
            public bool ccmarginadjusttax { get; set; }
            public long supplierid { get; set; }
            public long customerid { get; set; }
            public PrdCoverConfig prdcoverconfig { get; set; }
            public MfgConfig mfgconfig { get; set; }
            public AssetConfig assetconfig { get; set; }
            public LocationConfig locationconfig { get; set; }
            public IncentiveConfig incentiveconfig { get; set; }
            public HierarchyConfig hierarchyconfig { get; set; }
            public CustomField customfield { get; set; }
            public CourierTypes courier { get; set; }
            public PaymentIntegration payintegration { get; set; }
            public GiftConfig giftconfig { get; set; }  
            public BankInfo bankinfo { get; set; }
            public VoucherConfig voucherconfig { get; set; }
            public MfgLabelConfig mfglabelconfig { get; set; }
        }

        public class MfgLabelConfig
        {
            public bool width { get; set; }
            public bool length { get; set; }
            public bool picks { get; set; }
            public bool tpicks { get; set; }
            public bool repeat { get; set; }
            public bool flength { get; set; }
            public bool designer { get; set; }
            public bool quality { get; set; }
            public bool machine { get; set; }
            public bool noofcolours { get; set; }

            public bool design_colour { get; set; }
            public bool design_thickness { get; set; }
            public bool design_shade { get; set; }
            public bool design_picks { get; set; }
            public bool design_frontcolour { get; set; }
            public bool design_backcolour { get; set; }
            public bool design_bom { get; set; }
            public bool design_yarnqty { get; set; }
            public bool requiredyarn { get; set; }
            public bool requiredbom { get; set; }
            public bool requiredhours { get; set; }
            public bool size_name { get; set; }

        }

        public class VoucherConfig
        {
            public bool gstinput { get; set; }
            public bool gstrcm { get; set; }
            public bool itemwise { get; set; }
        }
        public class BankInfo
        {
            public string accountname { get; set; }
            public string accountno { get; set; }
            public string ifsc { get; set; }
            public string branch { get; set; }
            public List<bankvalue> ob { set; get; }
            public List<bankvalue> ir { set; get; }
            
        }

        public class bankvalue
        {
            public DateTime date { get; set; }
            public decimal value { get; set; }
        }
        public class GiftConfig
        {
            //Referencelist.additionalinfo.issales
            public bool isgift { get; set; }
            //Referencelist.additionalinfo.billdiscount
            public decimal billvalue { get; set; }
            public bool isgiftcoupon { get; set; }
            public decimal giftcouponpercentage { get; set; }
            public decimal giftcouponvalue { get; set; }
            public DateTime giftcouponexpiry { get; set; }
            public DateTime giftcouponfrom { get; set; }
            public int giftcouponexpirydays { get; set; }
            public int giftcouponvalidafterdays { get; set; }
        }
        public class PaymentIntegration
        {
            public int type { get; set; }
            public List<KeyValue> list { get; set; }

            public class KeyValue
            {
                public string key { get; set; }
                public string value { get; set; }
            }
        }
        public class CustomField
        {
            public string cfref { get; set; }
            public string cftype { get; set; }
            public bool cfbuilt { get; set; }
            public string cfname { get; set; }
        }

        public class CourierTypes
        {
            public bool inward { get; set; }
            public bool outward { get; set; }
            public bool supplier { get; set; }
            public bool agent { get; set; }
            public bool customerb2b { get; set; }
            public bool customer { get; set; }
            public bool name { get; set; }
            public bool address { get; set; }
            public bool awbewb { get; set; }
            public bool detail { get; set; }
            public bool invoiceno { get; set; }
            public bool billcopy { get; set; }
            public bool collectpayment { get; set; }
            public decimal payment { get; set; }

            public bool pod { get; set; }
            public bool collectrate { get; set; }
            public bool collectweight { get; set; }


            public decimal rate { get; set; }
            public decimal weight { get; set; }

            public long couriercompanyid { get; set; }
        }

        public class HierarchyConfig
        {
            public bool islast { get; set; }
            public bool isfirst { get; set; }
            public long p1 { get; set; }
            public long p2 { get; set; }
            public long p3 { get; set; }
            public long p4 { get; set; }
            public long p5 { get; set; }
        }
        public class LocationConfig
        {
            public string gst { get; set; }
            public long cityid { get; set; }
            public long stateid { get; set; }
            public string pincode { get; set; }
            public string emailid { get; set; }
            public string contactno { get; set; }
            public string tallycompany { get; set; }
            public string tallyserver { get; set; }
            public List<retailinfo> retinv { get; set; }
            public class retailinfo
            {
                public long id { get; set; }
                public int sqft { get; set; }
                public decimal avginv { get; set; }
            }
        }

        public class AssetConfig
        {
            public bool registration { get; set; }
            public bool amc { get; set; }
            public bool insurance { get; set; }
            public bool barcode { get; set; }
            public bool multiqty { get; set; }
            public bool activeqty { get; set; }
            public bool expiry { get; set; }
            public bool periodic { get; set; }
        }
        public class PrdCoverConfig
        {
            public bool fixeddays { get; set; }
            public int days { get; set; }
        }
        public class MfgConfig
        {
            public bool autocreate { get; set; }
            public long uomid { get; set; }
            public long taxid { get; set; }
            public string measurement { get; set; }
            public decimal charge { get; set; }
            public bool supportcolour { get; set; }
            public bool supportwidth { get; set; }
            public bool supportbominvoice { get; set; }
            public long stageid { get; set; }
            public long additionalchargeid { get; set; }
            public decimal additionalchargeamount { get; set; }
            public int corestatusid { get; set; }
            public long fromuomid { get; set; }
            public string statuscolour { get; set; }
            public long touomid { get; set; }
            public int fromcomponenttype { get; set; }
            public int tocomponenttype { get; set; }
            public bool sizeratio { get; set; }
            public int sizeratiotype { get; set; }
            public decimal conratio { get; set; }
            public decimal comratio { get; set; }
            public List<SizeRatio> SizeRatioList { get; set; }
            public class SizeRatio
            {
                public long sizeid { get; set; }
                public string size { get; set; }
                public int index { get; set; }
            }
        }

        public class IncentiveConfig
        {
            public bool RemoveBarcode { get; set; }
            public bool RemovePrefix { get; set; }
            public bool RemoveProductLevel { get; set; }
            public bool RemoveSettings { get; set; }
        }

        public class PriceTageConfig
        {
            public long pgid { get; set; }
            public decimal from { get; set; }
            public decimal to { get; set; }
            public string colour { get; set; }
            public int type { get; set; }
        }
        public class BilldiscountConfig
        {
            public long id { get; set; }
            public decimal billdiscount { get; set; }
            public decimal rewardpoint { get; set; }
            public bool billdiscountinclude { get; set; }
            public bool upgrade { get; set; }
            public bool otp { get; set; }
        }

        public class SalesIncentive
        {
            public bool value { get; set; }
            public string prefix { get; set; }
            public decimal from { get; set; }
            public decimal fromprice { get; set; }
            public decimal to { get; set; }
            public decimal toprice { get; set; }
            public decimal incentive { get; set; }
            public decimal sincentive { get; set; }
            public long referenceid { get; set; }
            public string name { get; set; }
            public long targetsource { get; set; }
            public TargetSource TgtSource
            {
                get
                {
                    return (TargetSource)targetsource;
                }
            }
            public enum TargetSource
            {
                SalesValue = 1,
                ProductAge = 2,
                Prefix = 3
            }
            public TargetComputation TgtComputation
            {
                get
                {
                    return (TargetComputation)targetcomputation;
                }
            }
            public int targetcomputation { get; set; }
            public enum TargetComputation
            {
                AdditionalSaleValueSingleSlab = 1,
                AdditionalSaleValueMultipleSlab = 2,
                AdditionalPerPieceValueSingleSlab = 3,
                AdditionalPerPieceValueMultipleSlab = 4,
                Minimum = 5
            }
        }
        public class StaticIP
        {
            public string ip { get; set; }
            public bool disable { get; set; }
            public string notes { get; set; }
        }
        public class Permission
        {
            public string code { get; set; }
            public bool include { get; set; }
        }
        public enum SpecialPayType
        {
            Bonus,
            Beta,
            OverTime,
            Fine
        }
        public class ReportConfiguration
        {
            public string Beta_Time { get; set; }
            public string Beta_MaxTime { get; set; }
            public decimal Beta_Male_Before { get; set; }
            public decimal Beta_Male_After { get; set; }
            public decimal Beta_Male_Halfday { get; set; }
            public decimal Beta_Male_Minimum { get; set; }
            public decimal Beta_Female_Before { get; set; }
            public decimal Beta_Female_After { get; set; }
            public decimal Beta_Female_Halfday { get; set; }
            public decimal Beta_Female_Minimum { get; set; }
            public decimal Bonus_Percentage { get; set; }
            public string Working_Hours_From { get; set; }
            public string Working_Hours_To { get; set; }
            public int Minimum_FullWorking_Minutes { get; set; }
            public int Minimum_HalfDayWorking_Minutes { get; set; }
            public decimal OT_Before_WH { get; set; }
            public decimal OT_After_WH { get; set; }
            public int Dailygracetime { get; set; }
            public int Lunchgracetime { get; set; }
            public int Monthlygracetime { get; set; }
            public int Fine_Male_Slab { get; set; }
            public int Fine_Male_Fine { get; set; }
            public int Fine_Male_Max { get; set; }
            public int Fine_Female_Slab { get; set; }
            public int Fine_Female_Fine { get; set; }
            public int Fine_Female_Max { get; set; }
        }

        //public enum IncentiveMode
        //{
        //    Fixed,
        //    Sales
        //}

        public enum LeaveTypes
        {
            Monthly,
            Yearly,

        }
    }

    public class LedgerHierarchy : BaseEntity
    {
        public long typeid { get; set; }
        public long id { get; set; }
        public List<long> deletedlist { get; set; }
        public int version { get; set; }
        public List<Node> children { get; set; }
        public string name { get; set; }
        [JsonIgnore]
        public string children_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(children); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        children = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Node>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }
        public class Node
        {
            public string name { get; set; }
            public string id { get; set; }
            public string stylecollapsed { get; set; }
            public string styleexpanded { get; set; }
            public string stylenormal { get; set; }
            public int level { get; set; }
            public string nodetype { get; set; }
            public List<Node> children { get; set; }
            public class Basebin
            {
                public long uid { get; set; }
                public string code { get; set; }
            }
        }
    }
    public class MergeRefrenceList : BaseEntity
    {
        public long id { get; set; }
        public string type { get; set; }
        public List<Referencelist> deletelist { get; set; }
        public string ErrorMessage { get; set; }

    }
    public class BinStorageRefrenceList : BaseEntity
    {
        public long id { get; set; }

        public List<long> DeletedBinList { get; set; }
        internal List<Mfgbinstorage> GetAllNewBin(bool altered)
        {
            List<Mfgbinstorage> result = new List<Mfgbinstorage>();
            if (this.children != null && this.children.Count > 0)
            {
                foreach (var children in this.children)
                {
                    result.AddRange(GetAllNewBin(altered, children));
                }
            }
            return result;
        }
        internal List<Mfgbinstorage> GetAllNewBin(bool altered, Node node)
        {
            List<Mfgbinstorage> result = new List<Mfgbinstorage>();
            if (node.binlist != null && node.binlist.Count > 0)
            {
                if (string.IsNullOrEmpty(node.id))
                {
                    node.id = Guid.NewGuid().ToString();
                }
                foreach (var bin in node.binlist)
                {
                    if (altered)
                    {
                        if (bin.altered)
                        {
                            result.Add(new Mfgbinstorage(bin, node));
                        }
                    }
                    else if (bin.uid == 0)
                    {
                        result.Add(new Mfgbinstorage(bin, node));
                    }
                }
            }
            if (node.children != null && node.children.Count > 0)
            {
                foreach (var children in node.children)
                {
                    result.AddRange(GetAllNewBin(altered, children));
                }
            }
            return result;
        }

        public int version { get; set; }
        public long referencelocationid { get; set; }
        public List<Node> children { get; set; }

        public string name { get; set; }
        [JsonIgnore]
        public string children_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(children); }
            set
            {
                if (value != null)
                {
                    try
                    {
                        children = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Node>>(value);
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
        }

        public enum NodeType
        {
            Building,
            Floor,
            Room,
            Column,
            Row,
            Box
        }
        public class Node
        {
            public string name { get; set; }
            public string id { get; set; }
            public string stylecollapsed { get; set; }
            public string styleexpanded { get; set; }
            public string stylenormal { get; set; }
            public bool supportallocation { get; set; }
            public int level { get; set; }
            public string code { get; set; }
            public int height { get; set; }
            public int width { get; set; }
            public int rows { get; set; }
            public int columns { get; set; }
            public int rowlength { get; set; }
            public int columnlength { get; set; }
            public string rowprefix { get; set; }
            public string columnprefix { get; set; }
            public string codeprefix { get; set; }
            public string suffixprefix { get; set; }
            public List<Node> children { get; set; }
            public List<StorageBin> binlist { get; set; }
            public class Basebin
            {
                public long uid { get; set; }
                public string code { get; set; }
            }

            public class StorageBin : Basebin
            {
                public string parent { get; set; }
                public int row { get; set; }
                public int column { get; set; }
                public bool disabled { get; set; }
                public decimal maxqty { get; set; }
                public int height { get; set; }
                public int width { get; set; }
                public bool altered { get; set; }
            }
        }
    }
    public class ReferencelistCreteria : Referencelist
    {
        public long referencelocationid { get; set; }
        public List<long> ids { get; set; }
        public List<string> types { get; set; }
        public bool inactive { get; set; }
        public List<string> names { get; set; }
        public bool addmissingnames { get; set; }
        public bool searchall { get; set; }
    }

    public class ReferenceSizeGroup
    {
        public long id { get; set; }
        public string name { get; set; }
        public List<ReferenceItem> list { get; set; }
    }
}
