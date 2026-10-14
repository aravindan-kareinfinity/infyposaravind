using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class DateConfiguration : BaseEntity
    {
        public bool removeconfig { get; set; }
        public DateTime freezedate { get; set; }
        public DateTime currentday { get; set; }
        public DateTime nextday { get; set; }
        public int nextdaycount { get; set; }
        public long locationid { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class Configuration : DateConfiguration
    {
        public string instancename { get; set; }
        public long id { get; set; }
        public long userid { get; set; }
        public string key { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string reportformat_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(reportformat); }
            set
            {
                reportformat = Newtonsoft.Json.JsonConvert.DeserializeObject<ReportFormat>(value);
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string appsettings_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(appsettings); }
            set
            {
                appsettings = Newtonsoft.Json.JsonConvert.DeserializeObject<ApplicationSettings>(value);
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string fbconfig_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(fbconfig); }
            set
            {
                fbconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<FBConfig>(value);
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string crmconfig_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(crmconfig); }
            set
            {
                crmconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<CRMConfig>(value);
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string wgconfig_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(wgconfig); }
            set
            {
                wgconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<WidgetConfig>(value);
            }
        }
        public WidgetConfig wgconfig { get; set; }
        public FBConfig fbconfig { get; set; }
        public CRMConfig crmconfig { get; set; }
        public ReportFormat reportformat { get; set; }
        public long organizationid { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public String name { get; set; }
        public bool isdefault { get; set; }

        public bool canalter { get; set; }

        //[JsonIgnore]
        //[JsonProperty(Required = Required.Default)]
        //public string additionalinfo_json
        //{
        //    get { return Newtonsoft.Json.JsonConvert.SerializeObject(additionalinfo); }
        //    set
        //    {
        //        additionalinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<AdditionalInfo>(value);
        //    }
        //}
        //public AdditionalInfo additionalinfo { get; set; }

        //public class AdditionalInfo
        //{
        //    public int lines { get; set; }
        //}
        public int nooflines { get; set; }
        public ApplicationSettings appsettings { get; set; }
        public int noofcopy { get; set; }
        public bool ispdf { get; set; }
        public bool isscheduled { get; set; }
        public class ApplicationSettings
        {
            public ApplicationSettings()
            {
                analyser_access_location = true;
            }
            public void MergeGlobal(ApplicationSettings globalapplicationSettings)
            {
                this.system_allowloyeltycustomerwithoutbill = globalapplicationSettings.system_allowloyeltycustomerwithoutbill;
                this.employee_minimumcode = globalapplicationSettings.employee_minimumcode;
                this.employeeno_validation = globalapplicationSettings.employeeno_validation;
                this.system_whinbox_length = globalapplicationSettings.system_whinbox_length;
                this.incentivebyworkingdays = globalapplicationSettings.incentivebyworkingdays;
                this.payroll_incentive_addb2b = globalapplicationSettings.payroll_incentive_addb2b;
                this.payroll_incentive_addexport = globalapplicationSettings.payroll_incentive_addexport;
                this.system_datechange = globalapplicationSettings.system_datechange;
                this.system_datechangesilent = globalapplicationSettings.system_datechangesilent;
                this.system_validatesettlement = globalapplicationSettings.system_validatesettlement;
                this.system_multicompany = globalapplicationSettings.system_multicompany;
                this.system_useproductcodewithname = globalapplicationSettings.system_useproductcodewithname;
                this.system_multicompanytype = globalapplicationSettings.system_multicompanytype;
                this.system_maxbarcodelength = globalapplicationSettings.system_maxbarcodelength;
                this.system_stockbatchdays = globalapplicationSettings.system_stockbatchdays;
                this.system_b2brounding = globalapplicationSettings.system_b2brounding;
                this.system_autouangln = globalapplicationSettings.system_autouangln;
                this.system_glnbatch = globalapplicationSettings.system_glnbatch;
                this.system_webprint = globalapplicationSettings.system_webprint;
                this.system_exchangebillprintformat = globalapplicationSettings.system_exchangebillprintformat;
                this.system_exchangebillsummaryprintformat = globalapplicationSettings.system_exchangebillsummaryprintformat;
                this.system_validateso = globalapplicationSettings.system_validateso;
                this.system_supportshipto = globalapplicationSettings.system_supportshipto;
                this.system_showexclusivetaxonpos = globalapplicationSettings.system_showexclusivetaxonpos;
                this.system_validateso = globalapplicationSettings.system_validateso;
                this.system_possizelist = globalapplicationSettings.system_possizelist;
                this.system_crsettlementonbill = globalapplicationSettings.system_crsettlementonbill;
                this.system_mergebillprint = globalapplicationSettings.system_mergebillprint;
                this.system_billprintcutter = globalapplicationSettings.system_billprintcutter;
                this.system_loansettlement = globalapplicationSettings.system_loansettlement;
                this.system_offlineprefix = globalapplicationSettings.system_offlineprefix;
                this.system_onlineprefix = globalapplicationSettings.system_onlineprefix;
                this.system_disableoutwardduplicate = globalapplicationSettings.system_disableoutwardduplicate;
                this.system_swippingmachine = globalapplicationSettings.system_swippingmachine;
                this.system_alwayscaps = globalapplicationSettings.system_alwayscaps;
                this.payroll_locakdate = globalapplicationSettings.payroll_locakdate;
                this.payroll_leaveencashment = globalapplicationSettings.payroll_leaveencashment;
                this.payroll_workdaysmode = globalapplicationSettings.payroll_workdaysmode;
                this.payroll_payslipbydate = globalapplicationSettings.payroll_payslipbydate;
                this.payroll_leaveeligibledays = globalapplicationSettings.payroll_leaveeligibledays;
                this.payroll_leaveeligiblemode = globalapplicationSettings.payroll_leaveeligiblemode;
                this.employee_payslip_title = globalapplicationSettings.employee_payslip_title;
                this.employee_payslip_subtitle = globalapplicationSettings.employee_payslip_subtitle;
                this.system_comisnbeforediscount = globalapplicationSettings.system_comisnbeforediscount;
                this.system_disabledesigneditable = globalapplicationSettings.system_disabledesigneditable;
                this.system_disablereturnsupplier = globalapplicationSettings.system_disablereturnsupplier;
                this.system_validatesellingprice = globalapplicationSettings.system_validatesellingprice;
                this.system_pomode = globalapplicationSettings.system_pomode;
                this.system_vendoragreement = globalapplicationSettings.system_vendoragreement;
                this.system_disableautoledger = globalapplicationSettings.system_disableautoledger;
                this.system_discountoncost = globalapplicationSettings.system_discountoncost;
                this.system_marginoncost = globalapplicationSettings.system_marginoncost;
                this.system_supportrounding = globalapplicationSettings.system_supportrounding;
                this.system_supportroundingdiscount = globalapplicationSettings.system_supportroundingdiscount;
                this.system_disableinvoiceentrydate = globalapplicationSettings.system_disableinvoiceentrydate;
                this.system_disablelrentrydate = globalapplicationSettings.system_disablelrentrydate;
                this.system_disableautotransferpermission = globalapplicationSettings.system_disableautotransferpermission;
                this.system_supportitemwiseentry = globalapplicationSettings.system_supportitemwiseentry;
                this.system_enablesourcesupplier = globalapplicationSettings.system_enablesourcesupplier;
                this.system_showlgstonlrentry = globalapplicationSettings.system_showlgstonlrentry;
                this.system_syncshowroomprice = globalapplicationSettings.system_syncshowroomprice;
                this.system_showroompurchase = globalapplicationSettings.system_showroompurchase;
                this.system_showroomentryno = globalapplicationSettings.system_showroomentryno;
                this.system_expenselrentry = globalapplicationSettings.system_expenselrentry;
                this.system_multiplewarehouse = globalapplicationSettings.system_multiplewarehouse;
                this.system_validatesettlement = globalapplicationSettings.system_validatesettlement;
                this.system_costwithadditional = globalapplicationSettings.system_costwithadditional;
                this.system_disablepodatechange = globalapplicationSettings.system_disablepodatechange;
                this.system_saleautomobileno = globalapplicationSettings.system_saleautomobileno || this.system_saleautomobileno;
                this.system_salecustomermandatory = globalapplicationSettings.system_salecustomermandatory || this.system_salecustomermandatory;
                this.system_estimatecustmandatory = globalapplicationSettings.system_estimatecustmandatory || this.system_estimatecustmandatory;
                this.system_srcustomermandatory = globalapplicationSettings.system_srcustomermandatory || this.system_srcustomermandatory;
                this.system_salesmanmandatory = globalapplicationSettings.system_salesmanmandatory || this.system_salesmanmandatory;
                this.system_supportdc = globalapplicationSettings.system_supportdc || this.system_supportdc;
                this.system_supportestimate = globalapplicationSettings.system_supportestimate || this.system_supportestimate;
                this.system_supportcustcreditnote = globalapplicationSettings.system_supportcustcreditnote || this.system_supportcustcreditnote;
                this.system_custcreditnoteabove = globalapplicationSettings.system_custcreditnoteabove;
                this.sale_emi = globalapplicationSettings.sale_emi || this.sale_emi;
                this.sale_exchange = globalapplicationSettings.sale_exchange || this.sale_exchange;
                this.sale_b2bhsnlimit = globalapplicationSettings.sale_b2bhsnlimit;
                this.sale_hsnlimit = globalapplicationSettings.sale_hsnlimit;
                this.sale_disableoffsetbill = globalapplicationSettings.sale_disableoffsetbill || this.sale_disableoffsetbill;
                this.system_silentsettlement = globalapplicationSettings.system_silentsettlement || this.system_silentsettlement;
                this.system_giftissuereceive = globalapplicationSettings.system_giftissuereceive || this.system_giftissuereceive;
                this.system_giftsameday = globalapplicationSettings.system_giftsameday || this.system_giftsameday;
                this.system_giftsettlement = globalapplicationSettings.system_giftsettlement || this.system_giftsettlement;
                this.system_giftdeliveryhighest = globalapplicationSettings.system_giftdeliveryhighest || this.system_giftdeliveryhighest;
                this.system_disableaddlndisonschemeitems = globalapplicationSettings.system_disableaddlndisonschemeitems || this.system_disableaddlndisonschemeitems;
                this.system_disableaddlndisondiscounteditems = globalapplicationSettings.system_disableaddlndisondiscounteditems || this.system_disableaddlndisondiscounteditems;
                this.system_disablecoupondisonaddlndisitems = globalapplicationSettings.system_disablecoupondisonaddlndisitems || this.system_disablecoupondisonaddlndisitems;
                this.system_disablecoupondisoncouponbill = globalapplicationSettings.system_disablecoupondisoncouponbill || this.system_disablecoupondisoncouponbill;
                this.system_disableloadsalebill = globalapplicationSettings.system_disableloadsalebill || this.system_disableloadsalebill;
                this.system_disableloadsalebillacross = globalapplicationSettings.system_disableloadsalebillacross || this.system_disableloadsalebillacross;
                this.system_salebillnodailyreset = globalapplicationSettings.system_salebillnodailyreset || this.system_salebillnodailyreset;
                this.system_salesrounding = globalapplicationSettings.system_salesrounding;
                this.system_salepriceonsalereturn = globalapplicationSettings.system_salepriceonsalereturn;
                this.system_multiplesaleqtypopup = globalapplicationSettings.system_multiplesaleqtypopup;
                this.system_multiplesaleqty = globalapplicationSettings.system_multiplesaleqty;
                this.system_multipleinvoiceqty = globalapplicationSettings.system_multipleinvoiceqty;
                this.system_multiplepossaleqty = globalapplicationSettings.system_multiplepossaleqty;
                this.sale_duplicatecoupon = globalapplicationSettings.sale_duplicatecoupon;
                this.system_adonfinalprice = globalapplicationSettings.system_adonfinalprice;
                this.system_adonschemediscount = globalapplicationSettings.system_adonschemediscount;
                this.system_disableautonumber = globalapplicationSettings.system_disableautonumber;
                this.system_supportbillnocharge = globalapplicationSettings.system_supportbillnocharge;
                this.system_uanpurchase = globalapplicationSettings.system_uanpurchase;
                this.system_supportpapersize = globalapplicationSettings.system_supportpapersize;
                this.system_supportmanualbill = globalapplicationSettings.system_supportmanualbill;
                this.system_supportpricelist = globalapplicationSettings.system_supportpricelist;
                this.system_supportbrandprice = globalapplicationSettings.system_supportbrandprice;
                this.system_b2bexclusivetax = globalapplicationSettings.system_b2bexclusivetax;
                this.system_posexclusivetax = globalapplicationSettings.system_posexclusivetax;
                this.system_creditlimit = globalapplicationSettings.system_creditlimit;
                this.system_barcodeparser = globalapplicationSettings.system_barcodeparser;
                this.system_supportmanualprice = globalapplicationSettings.system_supportmanualprice;
                this.system_posscrtemplate = globalapplicationSettings.system_posscrtemplate;
                this.system_disablebulkreturnonsale = globalapplicationSettings.system_disablebulkreturnonsale;
                this.system_supportdeliverybarcode = globalapplicationSettings.system_supportdeliverybarcode;
                this.system_supportdeliverybillno = globalapplicationSettings.system_supportdeliverybillno;
                this.system_intermediatetax = globalapplicationSettings.system_intermediatetax;
                this.system_supportstarpricematch = globalapplicationSettings.system_supportstarpricematch;
                this.system_gstbillsequence = globalapplicationSettings.system_gstbillsequence;
                this.system_cashierobcb = globalapplicationSettings.system_cashierobcb;
                this.system_sizegrouplayout = globalapplicationSettings.system_sizegrouplayout;
                this.system_poscutstock = globalapplicationSettings.system_poscutstock;
                this.system_poscutstockauto = globalapplicationSettings.system_poscutstockauto;
                this.system_supportpostaxtype = globalapplicationSettings.system_supportpostaxtype;
                this.system_disableposcredit = globalapplicationSettings.system_disableposcredit;
                this.system_disableposgstbill = globalapplicationSettings.system_disableposgstbill;
                this.system_taxrangeautodiscount = globalapplicationSettings.system_taxrangeautodiscount;
                this.system_b2bposfields = globalapplicationSettings.system_b2bposfields;
                this.system_barcodecomloc = globalapplicationSettings.system_barcodecomloc;
                this.system_posmultiprice = globalapplicationSettings.system_posmultiprice;
                this.system_posdisablerefund = globalapplicationSettings.system_posdisablerefund;
                this.system_poshold = globalapplicationSettings.system_poshold;
                this.system_posvoid = globalapplicationSettings.system_posvoid;
                this.system_disableuniquebarcode = globalapplicationSettings.system_disableuniquebarcode;
                this.system_restrictuniquebarcode = globalapplicationSettings.system_restrictuniquebarcode;
                this.system_restrictextrareturn = globalapplicationSettings.system_restrictextrareturn;
                this.system_supportautomissingstock = globalapplicationSettings.system_supportautomissingstock;
                this.system_supportbilledit = globalapplicationSettings.system_supportbilledit;
                this.system_supportcostedit = globalapplicationSettings.system_supportcostedit;
                this.system_supportpriceedit = globalapplicationSettings.system_supportpriceedit;
                this.system_supportcustomer10digits = globalapplicationSettings.system_supportcustomer10digits;
                this.system_salefloorwisebillno = globalapplicationSettings.system_salefloorwisebillno;
                this.system_floorwisebulkbillno = globalapplicationSettings.system_floorwisebulkbillno;
                this.system_srfloorwisebillno = globalapplicationSettings.system_srfloorwisebillno;
                this.system_floorwisedeliveryno = globalapplicationSettings.system_floorwisedeliveryno;
                this.system_salesroundingOnBarcode = globalapplicationSettings.system_salesroundingOnBarcode;
                this.system_purchaserounding = globalapplicationSettings.system_purchaserounding;
                this.system_purchasecustomrounding = globalapplicationSettings.system_purchasecustomrounding;
                this.payroll_incentiveminimumtarget = globalapplicationSettings.payroll_incentiveminimumtarget;
                this.payroll_leaveencashmentadjust = globalapplicationSettings.payroll_leaveencashmentadjust;
                this.system_interstatetransfer = globalapplicationSettings.system_interstatetransfer;
                this.system_immediatetransfer = globalapplicationSettings.system_immediatetransfer;
                this.system_billmarginlock = globalapplicationSettings.system_billmarginlock;
                this.system_minimumbillmargin = globalapplicationSettings.system_minimumbillmargin;
                this.system_interstatetransfermargin = globalapplicationSettings.system_interstatetransfermargin;
                this.system_srbillno = globalapplicationSettings.system_srbillno;
                this.system_srbillnobylocation = globalapplicationSettings.system_srbillnobylocation;
                this.system_locationrestriction = globalapplicationSettings.system_locationrestriction;
                this.system_iprestriction = globalapplicationSettings.system_iprestriction;
                this.system_barcodedateaspurchasedate = globalapplicationSettings.system_barcodedateaspurchasedate;
                this.system_locationbasedbarcodes = globalapplicationSettings.system_locationbasedbarcodes;
                this.system_supportuannegative = globalapplicationSettings.system_supportuannegative;
                this.system_salecollectcarddata = globalapplicationSettings.system_salecollectcarddata;
                this.system_salecollectagent = globalapplicationSettings.system_salecollectagent;
                this.system_salecollectrefemp = globalapplicationSettings.system_salecollectrefemp;
                this.system_ctrl_select2_startsWith = globalapplicationSettings.system_ctrl_select2_startsWith;
                this.system_fixed_barcodes = globalapplicationSettings.system_fixed_barcodes;
                this.system_hyper_uan = globalapplicationSettings.system_hyper_uan;
                this.system_quanto_barcodes = globalapplicationSettings.system_quanto_barcodes;
                this.system_fixed_barcode_length = globalapplicationSettings.system_fixed_barcode_length;
                this.system_supportcompanylocationfilter = globalapplicationSettings.system_supportcompanylocationfilter;
                this.system_supportcompanywiseentryno = globalapplicationSettings.system_supportcompanywiseentryno;
                this.system_supportlocationwisebundleno = globalapplicationSettings.system_supportlocationwisebundleno;
                this.system_supportlocationwisereturnno = globalapplicationSettings.system_supportlocationwisereturnno;
                this.system_supportlocationwisesupplierpaymentno = globalapplicationSettings.system_supportlocationwisesupplierpaymentno;
                this.system_supportlocationwisesopackingno = globalapplicationSettings.system_supportlocationwisesopackingno;
                this.system_supportsuppliergstvalidation = globalapplicationSettings.system_supportsuppliergstvalidation;
                this.system_supportinvaudit = globalapplicationSettings.system_supportinvaudit;
                this.system_voucherno_company = globalapplicationSettings.system_voucherno_company;
                this.system_voucherno_location = globalapplicationSettings.system_voucherno_location;
                this.system_voucherno_type = globalapplicationSettings.system_voucherno_type;
                this.system_marginexcludetax = globalapplicationSettings.system_marginexcludetax;
                this.system_marginincludetax = globalapplicationSettings.system_marginincludetax;
                this.system_supportinvoiceattachments = globalapplicationSettings.system_supportinvoiceattachments;
                this.system_supportcompanywisebarcode = globalapplicationSettings.system_supportcompanywisebarcode;
                this.system_marginonmarkup = globalapplicationSettings.system_marginonmarkup;
                this.system_marginonmarkdown = globalapplicationSettings.system_marginonmarkdown;
                this.system_supportattributefilter = globalapplicationSettings.system_supportattributefilter;
                this.system_editfinalprice = globalapplicationSettings.system_editfinalprice;
                this.system_allowduplicateitem = globalapplicationSettings.system_allowduplicateitem;
                this.system_pomandatory = globalapplicationSettings.system_pomandatory;
                this.system_allowmultiprice = globalapplicationSettings.system_allowmultiprice;
                this.system_supportsizegroup = globalapplicationSettings.system_supportsizegroup;
                this.system_maintainbarcodeseq = globalapplicationSettings.system_maintainbarcodeseq;
                this.system_hidecostinventory = globalapplicationSettings.system_hidecostinventory;
                this.system_autoglnimport = globalapplicationSettings.system_autoglnimport;
                this.system_marginpercentagezero = globalapplicationSettings.system_marginpercentagezero;
                this.system_splitbitprice = globalapplicationSettings.system_splitbitprice;
                this.system_disableupdateonbarcode = globalapplicationSettings.system_disableupdateonbarcode;
                this.system_jobworksamebarcode = globalapplicationSettings.system_jobworksamebarcode;
                this.system_supportskudesignpurchase = globalapplicationSettings.system_supportskudesignpurchase;
                this.system_addlrexptomargion = globalapplicationSettings.system_addlrexptomargion;
                this.system_salepricemrpdifference = globalapplicationSettings.system_salepricemrpdifference;
                this.incentivesalemode = globalapplicationSettings.incentivesalemode;
                this.incentivemode = globalapplicationSettings.incentivemode;
                this.system_ismobileapplication = globalapplicationSettings.system_ismobileapplication;
                this.system_isb2bapplication = globalapplicationSettings.system_isb2bapplication;
                this.system_systemrole = globalapplicationSettings.system_systemrole;
                this.system_isfranchiseapplication = globalapplicationSettings.system_isfranchiseapplication;
                this.system_ishypermarketapplication = globalapplicationSettings.system_ishypermarketapplication;
                this.analyser_stock_removeaqty = globalapplicationSettings.analyser_stock_removeaqty;
                this.analyser_access_company = globalapplicationSettings.analyser_access_company;
                this.analyser_access_restrict = globalapplicationSettings.analyser_access_restrict;
                this.analyser_data_cache = globalapplicationSettings.analyser_data_cache;
                this.analyser_incharge = globalapplicationSettings.analyser_incharge;
                this.intray_b2bcredit = globalapplicationSettings.intray_b2bcredit;
                this.intray_salesorder = globalapplicationSettings.intray_salesorder;
                this.intray_customer = globalapplicationSettings.intray_customer;
                this.intray_mfgdesign = globalapplicationSettings.intray_mfgdesign;
                this.intray_bommaster = globalapplicationSettings.intray_bommaster;
                this.intray_bundle = globalapplicationSettings.intray_bundle;
                this.intray_bompo = globalapplicationSettings.intray_bompo;
                this.integration_crm = globalapplicationSettings.integration_crm;
                this.integration_coupon = globalapplicationSettings.integration_coupon;
                this.sale_removereturnonprint = globalapplicationSettings.sale_removereturnonprint;
                this.sale_removediscountonprint = globalapplicationSettings.sale_removediscountonprint;
                this.payroll_incentive_removebarcode = globalapplicationSettings.payroll_incentive_removebarcode;
                this.payroll_incentive_removemarker = globalapplicationSettings.payroll_incentive_removemarker;
                this.payroll_incentive_removeprefix = globalapplicationSettings.payroll_incentive_removeprefix;
                this.payroll_incentive_removeproduct = globalapplicationSettings.payroll_incentive_removeproduct;
                this.payroll_incentive_addiscount = globalapplicationSettings.payroll_incentive_addiscount;
                this.system_bundledisablerollback = globalapplicationSettings.system_bundledisablerollback;
                this.system_ismfgapplication = globalapplicationSettings.system_ismfgapplication;
                this.mfg_tailoringmode = globalapplicationSettings.mfg_tailoringmode;
                this.mfg_fabricbasedorder = globalapplicationSettings.mfg_fabricbasedorder;
                this.mfg_oldskudesign = globalapplicationSettings.mfg_oldskudesign;
                this.mfg_salepricefromsku = globalapplicationSettings.mfg_salepricefromsku;
                this.mfg_grnlrinvoice = globalapplicationSettings.mfg_grnlrinvoice;
                this.mfg_fabricwostock = globalapplicationSettings.mfg_fabricwostock;
                this.mfg_wosodesignduplicate = globalapplicationSettings.mfg_wosodesignduplicate;
                this.mfg_customerpo = globalapplicationSettings.mfg_customerpo;
                this.mfg_gatepass = globalapplicationSettings.mfg_gatepass;
                this.mfg_singlepo = globalapplicationSettings.mfg_singlepo;
                this.mfg_dc2grn = globalapplicationSettings.mfg_dc2grn;
                this.mfg_whstock = globalapplicationSettings.mfg_whstock;
                this.saleorder = globalapplicationSettings.saleorder;
                this.mfg_mode = globalapplicationSettings.mfg_mode;
                this.appmode = globalapplicationSettings.appmode;
                this.mfg_eancode = globalapplicationSettings.mfg_eancode;
                this.mfg_manualdesigncode = globalapplicationSettings.mfg_manualdesigncode;
                this.mfg_eansequence = globalapplicationSettings.mfg_eansequence;
                this.mfg_designcode = globalapplicationSettings.mfg_designcode;
                this.mfg_stylecode = globalapplicationSettings.mfg_stylecode;
                this.mfg_designsizecode = globalapplicationSettings.mfg_designsizecode;
                this.gstserver = globalapplicationSettings.gstserver;
                this.mfg_designsizedescription = globalapplicationSettings.mfg_designsizedescription;
                this.system_dealerprice = globalapplicationSettings.system_dealerprice;
                this.system_dealerpricefrommargin = globalapplicationSettings.system_dealerpricefrommargin;
                this.system_dealerpricewotax = globalapplicationSettings.system_dealerpricewotax;
                this.customerotpfmt = globalapplicationSettings.customerotpfmt;
                this.approvalotpfmt = globalapplicationSettings.approvalotpfmt;
                this.approvaltemplateurl = globalapplicationSettings.approvaltemplateurl;
                this.billcopydestination = globalapplicationSettings.billcopydestination;
                this.billcopymode = globalapplicationSettings.billcopymode;
                this.intray_pur_prt = globalapplicationSettings.intray_pur_prt;
                this.system_supportimagerecord = globalapplicationSettings.system_supportimagerecord;
                this.system_invcol_colour = globalapplicationSettings.system_invcol_colour;
                this.system_invcol_size = globalapplicationSettings.system_invcol_size;
                this.finance_pettycashvoucher = globalapplicationSettings.finance_pettycashvoucher;
                this.finance_trace = globalapplicationSettings.finance_trace;
                this.finance_stocktransaction = globalapplicationSettings.finance_stocktransaction;
                this.system_invcol_brand = globalapplicationSettings.system_invcol_brand;
                this.system_invcol_style = globalapplicationSettings.system_invcol_style;
                this.system_invcol_type = globalapplicationSettings.system_invcol_type;
                this.system_invcol_expmargin = globalapplicationSettings.system_invcol_expmargin;
                this.system_invcol_dppercent = globalapplicationSettings.system_invcol_dppercent;
                this.system_invcol_txpercent = globalapplicationSettings.system_invcol_txpercent;
                this.system_invcol_pricetag = globalapplicationSettings.system_invcol_pricetag;
                this.system_invcol_pricetype = globalapplicationSettings.system_invcol_pricetype;
                this.system_cashclosewithtaken = globalapplicationSettings.system_cashclosewithtaken;
                this.system_counterpettycash = globalapplicationSettings.system_counterpettycash;
                this.system_b2bsalesbyproduct = globalapplicationSettings.system_b2bsalesbyproduct;
                this.system_customfields = globalapplicationSettings.system_customfields;
                this.system_billlog = globalapplicationSettings.system_billlog;
                this.system_auditlog = globalapplicationSettings.system_auditlog;
                this.system_supportreturnpriceqty = globalapplicationSettings.system_supportreturnpriceqty;
                this.system_enableinwardmode = globalapplicationSettings.system_enableinwardmode;
                this.system_saleunicodepdf = globalapplicationSettings.system_saleunicodepdf;
                this.system_deliveryrackcard = globalapplicationSettings.system_deliveryrackcard;
                this.system_supportdeliverywalk = globalapplicationSettings.system_supportdeliverywalk;
                this.system_tailoringunits = globalapplicationSettings.system_tailoringunits;
                this.system_cashclosedenomination = globalapplicationSettings.system_cashclosedenomination;
                this.system_creditbillotp = globalapplicationSettings.system_creditbillotp;
                this.system_multiplesalemen = globalapplicationSettings.system_multiplesalemen;
                this.system_designidautoid = globalapplicationSettings.system_designidautoid;
                this.system_costencode = globalapplicationSettings.system_costencode;
                this.system_expiredpromoexchange = globalapplicationSettings.system_expiredpromoexchange;
                this.system_deliveryrackauto = globalapplicationSettings.system_deliveryrackauto;
                this.system_supportdeliveryrack = globalapplicationSettings.system_supportdeliveryrack;
                this.system_billnoqueue = globalapplicationSettings.system_billnoqueue;
                this.system_barcodeonceprint = globalapplicationSettings.system_barcodeonceprint;
                this.system_jobworkgstforall = globalapplicationSettings.system_jobworkgstforall;
                this.system_jobworkcostmanual = globalapplicationSettings.system_jobworkcostmanual;
                //this.mfg_sovalidate = globalapplicationSettings.mfg_sovalidate;
                //this.mfg_so_wip = globalapplicationSettings.mfg_so_wip;
                //this.mfg_so_fg = globalapplicationSettings.mfg_so_fg;
                //this.sales_so_mandatory = globalapplicationSettings.sales_so_mandatory;
                //this.sales_so_partial = globalapplicationSettings.sales_so_partial;
            }
            public bool system_auditlog { get; set; }
            public bool system_billlog { get; set; }
            public bool system_enableinwardmode { get; set; }
            public bool system_barcodedateaspurchasedate { get; set; }
            public bool system_bundledisablerollback { get; set; }
            public bool system_salepricemrpdifference { get; set; }
            public bool system_ismobileapplication { get; set; }
            public bool system_isb2bapplication { get; set; }
            public bool system_isfranchiseapplication { get; set; }
            public bool system_ishypermarketapplication { get; set; }
            public bool system_locationbasedbarcodes { get; set; }
            public bool system_ctrl_select2_startsWith { get; set; }
            public int employee_minimumcode { get; set; }
            public int system_whinbox_length { get; set; }
            public bool employeeno_validation { get; set; }
            public int system_inwardmode { get; set; }
            public bool incentivebyworkingdays { get; set; }
            public bool system_supportuannegative { get; set; }
            public bool sale_negativestock { get; set; }
            public bool sale_negativereturnstock { get; set; }
            public bool sale_disableotherdayretbill { get; set; }
            public bool sale_disableotherdaybillprint { get; set; }
            public bool sale_lotteryenable { get; set; }
            public int sale_lotteryamount { get; set; }
            public int sale_lotterymax { get; set; }
            public bool sale_mandatorywalkintoken { get; set; }
            public bool sale_allowchangeofcustomer { get; set; }
            public bool sale_showitemname { get; set; }
            public bool system_enablelocbasedsalesman { get; set; }
            public bool system_enableleadmgmt { get; set; }
            public bool sale_promotion { get; set; }
            public bool sale_promotiononlyonestimate { get; set; }
            public bool sale_disablepurchasediscountonbill { get; set; }
            public bool sale_additionaldiscount { get; set; }
            public bool sale_regeneratepasscode { get; set; }
            public bool sale_sendpasscodetosms { get; set; }
            public bool sale_warehousestock { get; set; }
            public bool sale_requestpasscodetosms { get; set; }
            public bool sale_customerOTP { get; set; }
            public bool settlement_discountbill { get; set; }
            public bool settlement_nodailyreset { get; set; }
            public int settlement_discountbillprintmode { get; set; }
            public bool settlement_additionaldiscount { get; set; }
            public string sale_greetingtext { get; set; }
            public bool system_systemrole { get; set; }
            public string sale_billcopytitle { get; set; }
            public long sale_season { get; set; }
            public long salesman_designationid { get; set; }
            public long sale_area { get; set; }
            public long sale_salereason { get; set; }
            public string employee_payslip_title { get; set; }
            public string employee_payslip_subtitle { get; set; }
            public DateTime payroll_locakdate { get; set; }
            public int payroll_leaveencashment { get; set; }
            public bool system_ismfgapplication { get; set; }
            public int payroll_workdaysmode { get; set; }
            public bool payroll_payslipbydate { get; set; }
            public int payroll_leaveencashmentmode { get; set; }
            public int payroll_leaveencashmenttype { get; set; }
            public bool payroll_leaveencashmentadjust { get; set; }
            public decimal payroll_fixedincentive { get; set; }
            public int payroll_leaveeligibledays { get; set; }
            public int payroll_leaveeligiblemode { get; set; }
            public int incentivemode { get; set; }
            public enum IncentiveModes
            {
                None,
                TargetBased,
                SectionPriceRange,
                ProdutPriceRange,
                Fixed,
                EmployeeProductAge,
                EmployeeConfig,
                ProductAge,
                SectionAge
            }

            public enum leaveencashmentmode
            {
                FixedDays,
                Sunday,
                SundayAndHoliday,
                Auto,
                None
            }

            public leaveencashmentmode LeaveEncashmentMode
            {
                get
                {
                    return (leaveencashmentmode)payroll_leaveencashmenttype;
                }
            }
            public int incentivesalemode { get; set; }
            public decimal incentiveskipmonths { get; set; }
            public bool intray_supplier { get; set; }
            public bool intray_product { get; set; }
            public bool intray_agent { get; set; }
            public bool intray_inward { get; set; }
            public bool intray_task { get; set; }

            public bool intray_discount { get; set; }
            public bool settlement_reward { get; set; }
            public bool sales_reward { get; set; }
            public int sale_rewardmode { get; set; }
            public decimal sale_rewardpoint { get; set; }
            public bool sale_rewardincludediscount { get; set; }
            public long sale_defaultcustomercategory { get; set; }
            public long sale_defaultbankid { get; set; }
            public long sale_defaultupibankid { get; set; }
            public bool sale_excludeproduct { get; set; }
            public bool sale_excludediscount { get; set; }
            public int sale_excludediscountabove { get; set; }
            public int sale_excludeadditionaldiscountabove { get; set; }
            public bool gft_excludediscount { get; set; }
            public int gft_excludediscountabove { get; set; }
            public bool gft_enable { get; set; }
            public bool gft_excludeaddiscount { get; set; }
            public int gft_excludeaddiscountabove { get; set; }
            public bool gft_excludepromotion { get; set; }
            public int gft_excludepromotionabove { get; set; }
            public string gft_excludepromotioncode { get; set; }
            public bool gft_wocusredemption { get; set; }
            public bool gft_disablebillitem { get; set; }
            public bool gft_stockreduce { get; set; }
            public List<long> gft_excludepromotionlist { get; set; }
            public bool sale_excludeadditionaldiscount { get; set; }
            public bool sale_excludecoupondiscount { get; set; }
            public bool sale_includeallrangecoupon { get; set; }
            public bool sale_excludecrbill { get; set; }
            public bool sale_excludecreditnote { get; set; }
            public bool sale_canclaimsamebillrewards { get; set; }
            public bool sale_canclaimloyaltyholderonly { get; set; }
            public bool sale_deliveryno { get; set; }
            public bool settlement_coupen { get; set; }
            public bool settlement_reward_expirybydate { get; set; }
            public DateTime settlement_reward_expirydate { get; set; }
            public int settlement_reward_expiry { get; set; }
            public decimal settlement_reward_value { get; set; }
            public bool analyser_stock_withzerostock { get; set; }
            public bool analyser_stock_purchasesales { get; set; }
            public bool analyser_stock_removeaqty { get; set; }
            public bool analyser_access_company { get; set; }
            public bool analyser_access_restrict { get; set; }
            public bool analyser_data_cache { get; set; }
            public bool analyser_productdesign { get; set; }
            public bool analyser_incharge { get; set; }
            public int analyser_timeout { get; set; }
            public bool analyser_access_location { get; set; }
            public bool analyser_access_section { get; set; }
            public bool po_autocolumn { get; set; }
            public int po_maxcolumn { get; set; }
            public string hostedserver { get; set; }
            public string sms_hostedserver { get; set; }
            public bool sms_multiple_no { get; set; }
            public string sms_url { get; set; }
            public string sms_fburl { get; set; }
            public string customerotpfmt { get; set; }
            public string approvalotpfmt { get; set; }
            public string approvaltemplateurl { get; set; }
            public string billcopydestination { get; set; }
            public billcopymodes billcopymode { get; set; }
            public enum billcopymodes
            {
                None,
                File,
                TCPIP,
                Web
            }
            public string sms_tran_senderid { get; set; }
            public string sms_prom_senderid { get; set; }
            public bool intray_brand { get; set; }
            public bool intray_size { get; set; }
            public bool intray_type { get; set; }
            public bool intray_style { get; set; }
            public bool intray_pattern { get; set; }
            public bool intray_material { get; set; }
            public bool intray_color { get; set; }
            public bool intray_purchaseorder { get; set; }
            public bool intray_barcode { get; set; }
            public bool intray_b2bcredit { get; set; }
            public bool intray_salesorder { get; set; }
            public bool intray_customer { get; set; }
            public bool intray_mfgdesign { get; set; }
            public bool intray_bommaster { get; set; }
            public bool intray_dndraft { set; get; }
            public bool intray_supplierpayment {get;set;}
            public bool intray_pur_prt { set; get; }
            public bool intray_ledger { get; set; }
            public bool intray_bundle { get; set; }
            public bool intray_bompo { get; set; }
            public bool intray_notification_inward { get; set; }
            public bool intray_notification_purchaseorder { get; set; }
            public bool intray_notification_qtymismatch { get; set; }
            public bool intray_notification_potolerance { get; set; }
            public bool intray_notification_invoiceratemismatch { get; set; }
            public bool intray_notification_lastpricemismatch { get; set; }
            public bool intray_notification_marginmismatch { get; set; }
            public bool intray_notification_barcode { get; set; }
            public bool intray_notification_bill { get; set; }
            public bool intray_notification_billsettlement { get; set; }
            public bool intray_notification_crm { get; set; }
            public bool intray_notification_ecommerce { get; set; }
            public string integration_crm { get; set; }
            public string integration_coupon { get; set; }
            public bool system_syncshowroomprice { get; set; }
            public bool system_multiplewarehouse { get; set; }
            public bool system_showroompurchase { get; set; }
            public bool system_showroomentryno { get; set; }
            public bool system_expenselrentry { get; set; }
            public bool system_discountoncost { get; set; }
            public bool system_marginoncost { get; set; }
            public bool system_costwithadditional { get; set; }
            public bool system_multicompany { get; set; }
            public bool system_monthlybarcodes { get; set; }
            public bool system_tradeagreement { get; set; }
            public bool system_comisnbeforediscount { get; set; }
            public bool system_disabledesigneditable { get; set; }
            public bool system_disablereturnsupplier { get; set; }
            public bool system_validatesellingprice { get; set; }
            public int system_pomode { get; set; }
            public bool system_vendoragreement { get; set; }
            public bool system_disableautoledger { get; set; }
            public bool system_itemcode { get; set; }
            public string system_monthencryption { get; set; }
            public int system_yearencryptionfrom { get; set; }
            public string system_yearencryption { get; set; }
            public bool system_useproductcodewithname { get; set; }
            public int system_multicompanytype { get; set; }
            public int system_maxbarcodelength { get; set; }
            public int system_stockbatchdays { get; set; }
            public bool system_b2brounding { get; set; }
            public bool system_autouangln { get; set; }
            public bool system_glnbatch { get; set; }
            public bool system_webprint { get; set; }
            public bool system_validatesettlement { get; set; }
            public bool system_supportrounding { get; set; }
            public bool system_supportroundingdiscount { get; set; }
            public bool system_disableinvoiceentrydate { get; set; }
            public bool system_disablelrentrydate { get; set; }
            public bool system_disableautotransferpermission { get; set; }
            public bool system_supportitemwiseentry { get; set; }
            public bool system_supportcompanylocationfilter { get; set; }
            public bool system_marginexcludetax { get; set; }
            public bool system_marginincludetax { get; set; }
            public bool system_supportinvoiceattachments { get; set; }
            public bool system_supportcompanywisebarcode { get; set; }
            public bool system_marginonmarkup { get; set; }
            public bool system_marginonmarkdown { get; set; }
            public bool system_supportattributefilter { get; set; }
            public bool system_editfinalprice { get; set; }
            public bool system_allowduplicateitem { get; set; }
            public bool system_pomandatory { get; set; }
            public bool system_allowmultiprice { get; set; }
            public bool system_supportsizegroup { get; set; }
            public bool system_maintainbarcodeseq { get; set; }
            public bool system_hidecostinventory { get; set; }
            public bool system_autoglnimport { get; set; }
            public bool system_marginpercentagezero { get; set; }
            public bool system_splitbitprice { get; set; }
            public bool system_disableupdateonbarcode { get; set; }
            public bool system_jobworksamebarcode { get; set; }
            public bool system_supportskudesignpurchase { get; set; }
            public bool system_addlrexptomargion { get; set; }
            public bool system_supportspecialbarcode { get; set; }
            public bool system_disablepodatechange { get; set; }
            public int system_specialbarcodemode { get; set; }
            public bool system_dealerprice { get; set; }
            public bool system_dealerpricefrommargin { get; set; }
            public bool system_dealerpricewotax { get; set; }
            public bool system_supportimagerecord { get; set; }
            public bool system_invcol_size { get; set; }
            public bool system_invcol_colour { get; set; }
            public bool system_invcol_style { get; set; }
            public bool system_invcol_type { get; set; }
            public bool system_invcol_expmargin { get; set; }
            public bool system_invcol_dppercent { get; set; }
            public bool system_invcol_txpercent { get; set; }
            public bool system_invcol_brand { get; set; }
            public bool system_invcol_pricetag { get; set; }
            public bool system_invcol_pricetype { get; set; }
            public SpeciamBARCODEMODE SpeciamBarcodeMode
            {
                get
                {
                    return (SpeciamBARCODEMODE)system_specialbarcodemode;
                }
            }
            public enum SpeciamBARCODEMODE
            {
                ProductColorPrice = 1
            }
            public bool system_supportcompanywiseentryno { get; set; }
            public bool system_supportlocationwisebundleno { get; set; }
            public bool system_supportlocationwisereturnno { get; set; }
            public bool system_supportlocationwisesupplierpaymentno { get; set; }
            public bool system_supportlocationwisesopackingno { get; set; }
            public bool system_supportsuppliergstvalidation { get; set; }
            public bool system_supportinvaudit { get; set; }
            public bool system_voucherno_company { get; set; }
            public bool system_voucherno_location { get; set; }
            public bool system_voucherno_type { get; set; }
            public bool system_enablesourcesupplier { get; set; }
            public bool system_mergebillprint { get; set; }
            public bool system_showlgstonlrentry { get; set; }
            public bool system_billprintcutter { get; set; }
            public bool system_salecustomermandatory { get; set; }
            public bool system_estimatecustmandatory { get; set; }
            public bool system_srcustomermandatory { get; set; }
            public bool system_saleautomobileno { get; set; }
            public bool system_salebillnodailyreset { get; set; }
            public decimal payroll_incentiveminimumtarget { get; set; }
            public bool system_salesmanmandatory { get; set; }
            public bool system_randombundlecode { get; set; }

            public bool system_disableoutwardduplicate { get; set; }
            public bool system_interstatetransfer { get; set; }
            public bool system_immediatetransfer { get; set; }
            public bool system_loansettlement { get; set; }
            public string system_offlineprefix { get; set; }

            public string system_onlineprefix { get; set; }
            public bool system_swippingmachine { get; set; }
            public bool system_supportdc { get; set; }
            public bool system_supportdeliveryrack { get; set; }
            public bool system_deliveryrackauto { get; set; }
            public bool system_billnoqueue { get; set; }
            public bool system_deliveryrackcard { get; set; }
            public bool system_supportdeliverywalk { get; set; }
            public bool system_tailoringunits { get; set; }
            public bool system_cashclosedenomination { get; set; }
            public bool system_creditbillotp { get; set; }
            public bool system_multiplesalemen { get; set; }
            public bool system_designidautoid { get; set; }
            public string system_costencode { get; set; }
            public bool system_barcodeonceprint { get; set; }
            public bool system_jobworkgstforall { get; set; }
            public bool system_jobworkcostmanual { get; set; }
            public bool system_expiredpromoexchange { get; set; }
            public bool system_supportestimatesticker { get; set; }
            public bool system_supportestimate { get; set; }
            public bool system_supportcustcreditnote { get; set; }
            public int system_custcreditnoteabove { get; set; }
            public bool sale_emi { get; set; }
            public bool sale_exchange { get; set; }
            public bool sale_disableoffsetbill { get; set; }
            public string sale_autoprefix { get; set; }
            public int sale_hsnlimit { get; set; }
            public int sale_b2bhsnlimit { get; set; }
            public bool sale_showlastbitqty { get; set; }
            public bool sale_removediscountonprint { get; set; }
            public bool sale_removereturnonprint { get; set; }
            public bool system_silentsettlement { get; set; }
            public bool system_giftdeliveryhighest { get; set; }
            public bool system_disableaddlndisonschemeitems { get; set; }
            public bool system_disableaddlndisondiscounteditems { get; set; }
            public bool system_disablemfgmeterwarning { get; set; }
            public bool system_disablecoupondisonaddlndisitems { get; set; }
            public bool system_disablecoupondisoncouponbill { get; set; }
            public bool system_disableloadsalebill { get; set; }
            public bool system_disableloadsalebillacross { get; set; }
            public bool system_giftsettlement { get; set; }
            public bool system_giftsameday { get; set; }
            public bool system_giftissuereceive { get; set; }
            public bool system_salepriceonsalereturn { get; set; }
            public bool system_multiplesaleqtypopup { get; set; }
            public int system_multiplesaleqty { get; set; }
            public bool system_multipleinvoiceqty { get; set; }
            public bool system_multiplepossaleqty { get; set; }
            public bool system_saledirectpdf { get; set; }
            public bool system_b2bsaledirectpdf { get; set; }
            public bool system_b2bprintdisable { get; set; }
            public bool system_saleunicodepdf { get; set; }
            public bool sale_duplicatecoupon { get; set; }
            public bool system_adonfinalprice { get; set; }
            public bool system_adonschemediscount { get; set; }
            public bool system_disableautonumber { get; set; }
            public bool system_supportbillnocharge { get; set; }
            public bool system_uanpurchase { get; set; }
            public bool system_supportpapersize { get; set; }
            public bool system_supportmanualbill { get; set; }
            public bool system_supportpricelist { get; set; }
            public bool system_supportbrandprice { get; set; }
            public bool system_b2bexclusivetax { get; set; }
            public bool system_posexclusivetax { get; set; }
            public bool system_creditlimit { get; set; }
            public bool system_supportmanualprice { get; set; }
            public int system_posscrtemplate { get; set; }
            public bool system_disablebulkreturnonsale { get; set; }
            public bool system_supportdeliverybarcode { get; set; }
            public bool system_supportdeliverybillno { get; set; }
            public bool system_intermediatetax { get; set; }
            public bool system_supportstarpricematch { get; set; }
            public int system_posmultiprice { get; set; }
            public bool system_gstbillsequence { get; set; }
            public bool system_cashierobcb { get; set; }
            public bool system_sizegrouplayout { get; set; }
            public bool system_poscutstock { get; set; }
            public bool system_poscutstockauto { get; set; }
            public bool system_supportpostaxtype { get; set; }
            public bool system_taxrangeautodiscount { get; set; }
            public bool system_disableposcredit { get; set; }
            public bool system_disableposgstbill { get; set; }
            public string system_b2bposfields { get; set; }
            public bool system_barcodecomloc { get; set; }
            public bool system_posdisablerefund { get; set; }
            public bool system_poshold { get; set; }
            public bool system_posvoid { get; set; }
            public string system_barcodeparser { get; set; }

            public bool system_disableuniquebarcode { get; set; }
            public bool system_restrictuniquebarcode { get; set; }
            public bool system_restrictextrareturn { get; set; }
            public bool system_supportautomissingstock { get; set; }
            public bool system_supportbilledit { get; set; }
            public bool system_supportcostedit { get; set; }
            public bool system_supportpriceedit { get; set; }
            public bool system_supportcustomer10digits { get; set; }
            public bool system_salefloorwisebillno { get; set; }
            public bool system_floorwisebulkbillno { get; set; }
            public bool system_srfloorwisebillno { get; set; }
            public bool system_floorwisedeliveryno { get; set; }
            public bool system_srbillno { get; set; }
            public bool system_srbillnobylocation { get; set; }
            public bool system_exchangebillprintformat { get; set; }
            public bool system_exchangebillsummaryprintformat { get; set; }
            public bool system_supportshipto { get; set; }
            public bool system_showexclusivetaxonpos { get; set; }
            public bool system_validateso { get; set; }
            public string system_possizelist { get; set; }
            public bool system_crsettlementonbill { get; set; }
            public bool system_alwayscaps { get; set; }
            public bool system_saledatefreeze { get; set; }
            public bool mfg_sovalidate { get; set; }
            public bool mfg_so_retail { get; set; }
            public bool mfg_so_fs { get; set; }
            public bool mfg_so_wip { get; set; }
            public bool mfg_sosizeratio { get; set; }
            public string mfg_sosizeratiorule { get; set; }
            //public bool mfg_so_fg { get; set; }
            public bool mfg_so_po { get; set; }
            public bool sales_so_mandatory { get; set; }
            public bool sales_so_partial { get; set; }
            public DateTime system_saledatefreezeon { get; set; }
            public DateTime system_reloadsalebillfreezeon { get; set; }
            public int sale_exchangerefund { get; set; }
            public bool system_autonumberwithexpiry { get; set; }
            public bool system_warehousenotrequired { get; set; }
            public bool system_custadvnvoucher { get; set; }
            public bool system_custadvnpaymenttracking { get; set; }
            public bool system_supportreturnpriceqty { get; set; }
            public bool system_supportpricingflow { get; set; }
            public bool system_supportpricingflowproducts { get; set; }
            public bool system_cashclosewithtaken { get; set; }
            public bool system_cashclosevalidation { get; set; }
            public bool system_counterpettycash { get; set; }
            public bool system_b2bsalesbyproduct { get; set; }
            public bool system_customfields { get; set; }
            public int system_salesrounding { get; set; }
            public bool system_salesroundingOnBarcode { get; set; }
            public int system_purchaserounding { get; set; }
            public string system_purchasecustomrounding { get; set; }
            public bool system_salecollectcarddata { get; set; }
            public bool system_salecollectagent { get; set; }
            public bool system_salecollectrefemp { get; set; }
            public bool system_datechange { get; set; }
            public bool system_datechangesilent { get; set; }
            public bool settlement_disallowmulticompany { get; set; }
            public bool settlement_disableadjustment { get; set; }
            public bool settlement_disablereturnbill { get; set; }
            public int settlement_maxadjustment { get; set; }
            public bool system_billmarginlock { get; set; }
            public bool security_timebased { get; set; }
            public bool secuirty_integratebiometric { get; set; }
            public decimal system_minimumbillmargin { get; set; }
            public decimal system_interstatetransfermargin { get; set; }
            public bool system_iprestriction { get; set; }
            public bool system_fixed_barcodes { get; set; }
            public bool system_hyper_uan { get; set; }
            public bool system_quanto_barcodes { get; set; }
            public bool system_secondary_barcodes { get; set; }
            public int system_fixed_barcode_length { get; set; }
            public bool system_locationrestriction { get; set; }
            public bool payroll_incentive_removebarcode { get; set; }
            public bool payroll_incentive_removemarker { get; set; }
            public bool payroll_incentive_removeprefix { get; set; }
            public bool payroll_incentive_removeproduct { get; set; }

            public bool payroll_incentive_addb2b { get; set; }
            public bool payroll_incentive_addexport { get; set; }

            public bool payroll_incentive_addiscount { get; set; }
            public decimal settlement_reward_maxbillvaluepercentage { get; set; }


            public int settlement_reward_birthday_mode { get; set; }
            public decimal settlement_birthday_multiple { get; set; }
            public int settlement_birthday_before { get; set; }
            public int settlement_birthday_after { get; set; }
            public bool settlement_reward_first_enable { get; set; }
            public int settlement_reward_first { get; set; }
            public decimal sale_reward_minbillvalue { get; set; }
            public string sale_customercardprefix { get; set; }
            public string sale_deliverycardprefix { get; set; }
            public string prd_hierarchy { get; set; }
            public Ecommerce ecommerce { get; set; }
            public class Ecommerce
            {
                public string type { get; set; }
                public string path { get; set; }
                public string authkey { get; set; }
            }
            [JsonIgnore]
            public POSPARSER POSParser
            {
                get
                {
                    if (_POSParser == null)
                    {
                        if (!string.IsNullOrEmpty(system_barcodeparser))
                        {
                            try
                            {
                                _POSParser = JsonConvert.DeserializeObject<POSPARSER>(system_barcodeparser);
                            }
                            catch (Exception exp)
                            {
                                _POSParser = new POSPARSER() { IsEmpty = true };
                            }
                        }
                        else
                            _POSParser = new POSPARSER() { IsEmpty = true };
                    }
                    return _POSParser;
                }
            }
            POSPARSER _POSParser = null;

            public class POSPARSER
            {
                public bool IsEmpty { get; set; }
                public int start { get; set; }
                public int length { get; set; }
                public int iflength { get; set; }
                public int removeend { get; set; }
                public int removebegin { get; set; }
                public string exclude { get; set; }
                public string removeafter { get; set; }
                public Cutting cutting { get; set; }
                public class Cutting
                {
                    public bool QApp { get; set; }
                    public int barcodelength { get; set; }
                    public int meterlength { get; set; }
                    public int meterdecimal { get; set; }
                    public string separator { get; set; }
                    public string prefix { get; set; }
                }
                public string UpdateBarcode(string barcode)
                {
                    if (IsEmpty) return barcode;
                    if (!string.IsNullOrEmpty(exclude) && barcode.StartsWith(exclude)) return barcode;
                    if (!string.IsNullOrEmpty(exclude) && exclude.IndexOf(',') > 0)
                    {
                        string[] excludelist = exclude.Split(',');
                        if (excludelist != null && excludelist.Length > 0)
                        {
                            foreach (var str in excludelist)
                                if (barcode.StartsWith(str)) return barcode;
                        }
                    }
                    if (iflength > 0 && iflength > barcode.Length) return barcode;
                    if (length > 0) return barcode.Substring(start, length);
                    if (removeend > 0 && barcode.Length > removeend) return barcode.Substring(0, barcode.Length - removeend);
                    if (removebegin > 0) return barcode.Substring(removebegin);
                    if (!string.IsNullOrEmpty(removeafter) && barcode.IndexOf(removeafter) > 0)
                    {
                        return barcode.Substring(0, barcode.Length - (barcode.Length - barcode.IndexOf(removeafter)));
                    }
                    return barcode;
                }
            }

            public List<string> system_iplist { get; set; }
            public List<Reward_Slabs> settlement_reward_slabs { get; set; }
            public List<Reward_Slabs> settlement_rewardclaim_slabs { get; set; }
            public bool settlement_reward_slabed { get; set; }
            public class Reward_Slabs
            {
                public int from { get; set; }
                public int upto { get; set; }
                public decimal percentage { get; set; }
            }
            [JsonIgnore]
            public DateTime freezedate { get; set; }
            [JsonIgnore]
            public DateTime currentday { get; set; }
            [JsonIgnore]
            public int nextdaycount { get; set; }
            public bool system_allowloyeltycustomerchange { get; set; }
            public bool system_allowloyeltycustomerwithoutbill { get; set; }
            public string instancename { get; set; }
            public string numberencrypt { get; set; }
            public bool db_hidereturnbillvalue { get; set; }
            public bool db_salegraph { get; set; }
            public bool db_businesstrend { get; set; }
            public bool db_warehousestatus { get; set; }
            public bool db_employeesummary { get; set; }
            public bool int_unicommerce { get; set; }
            public bool int_payment { get; set; }
            public bool int_finance { get; set; }
            public bool finance_pettycashvoucher { get; set; }
            public bool finance_trace { get; set; }
            public bool finance_tbexclinexp { get; set; }
            public bool finance_obautopost { get; set; }
            public bool analyser_uniquemarker { get; set; }
            public bool finance_stocktransaction { get; set; }
            public bool fin_nonbarcodedvalue { get; set; }
            public int fin_lockdays { get; set; }
            public long fin_purtdsledgerid { get; set; }
            public long fin_purtcsledgerid { get; set; }
            public long fin_paytdsledgerid { get; set; }
            public int int_paymode { get; set; }
            public string int_payconfig { get; set; }

            public string int_financepostconfig { get; set; }
            public int int_financepostmode { get; set; }
            public bool int_financepost { get; set; }
            public int qr_paymode { get; set; }
            public string qr_payconfig { get; set; }
            public int mfg_mode { get; set; }
            public int appmode { get; set; }
            public string mfg_eancode { get; set; }
            public bool mfg_manualdesigncode { get; set; }
            public bool mfg_eansequence { get; set; }
            public string mfg_designcode { get; set; }
            public string mfg_stylecode { get; set; }
            public string mfg_designsizecode { get; set; }
            public string mfg_designsizedescription { get; set; }
            public string mfg_oldskudesign { get; set; }
            public string mfg_skufields { get; set; }
            public bool mfg_fabricbasedorder { get; set; }
            public int mfg_tailoringmode { get; set; }
            public bool mfg_salepricefromsku { get; set; }
            public bool mfg_gatepass { get; set; }
            public bool mfg_singlepo { get; set; }
            public bool mfg_dc2grn { get; set; }
            public bool mfg_grnlrinvoice { get; set; }
            public bool mfg_fabricwostock { get; set; }
            public bool mfg_wosodesignduplicate { get; set; }
            public bool mfg_customerpo { get; set; }
            public bool mfg_whstock { get; set; }
            public bool autouanstock { get; set; }
            public string gstserver { get; set; }
            public SaleOrderConfig saleorder { get; set; }
            public UnicommerceConfig unicommerce { get; set; }
            public string CommandLibClass { get; set; }
            public string CommandLibPath { get; set; }
            public bool HasCommandLib
            {
                get
                {
                    return !string.IsNullOrEmpty(CommandLibClass) &&
                        !string.IsNullOrEmpty(CommandLibPath);
                }
            }
        }

        public class UnicommerceConfig
        {
            public string username { get; set; }
            public string password { get; set; }
            public string serverurl { get; set; }
            public Guid access_token { get; set; }
            public DateTime access_validto { get; set; }
            [JsonIgnore]
            public bool dbdecoded { get; set; }
            public string facility { get; set; }
            public string shelf { get; set; }
            public int buffer { get; set; }
            public class Channel
            {
                public long companyid { get; set; }
                public string name { get; set; }
                public long locationid { get; set; }
                public string customercode { get; set; }
                public long customerid { get; set; }
                [JsonIgnore]
                public string company { get; set; }
                [JsonIgnore]
                public string location { get; set; }
            }
            public List<Channel> channels { set; get; }
        }

        public class SaleOrderConfig
        {
            public bool design { get; set; }
            public bool sizegroup { get; set; }
            public bool size { get; set; }
            public bool colour { get; set; }
            public bool style { get; set; }
            public bool pattern { get; set; }
            public bool brand { get; set; }
            public bool sleeve { get; set; }
            public bool fit { get; set; }
            public bool etprice { get; set; }
            public bool price { get; set; }
            public bool booking { get; set; }
        }
        public class ReportFormat
        {
            public string format { get; set; }
            public string mode { get; set; }
            public int leftmargin { get; set; }
            public int topmargin { get; set; }
            public int labelgap { get; set; }
            public int labelcolumns { get; set; }
            public int widthinmm { get; set; }
            public int heightinmm { get; set; }
        }

        public class printformat : BaseEntity
        {
            public string displayvalue { get; set; }
            public string barcodevalue { get; set; }
            public int nooflines { get; set; }
            public string mode { get; set; }
            public int noofcopy { get; set; }
            public string format { get; set; }

            public List<string> printdocument { get; set; }
        }
    }

    public class ConfigurationByString:BaseEntity
    {
        public string value { get; set; }
        public string key { get; set; }
        public long organizationid { get; set; }
    }

    public class ConfigurationCreteria : Configuration
    {
        public List<string> keys { get; set; }
        public bool history { get; set; }
        public List<string> excludekeys { get; set; }
        public bool supportinstance { get; set; }
        public List<long> promotionids { get; set; }
    }

    public class WidgetConfig : BaseEntity
    {
        public string source { get; set; }
        public string datastorage { get; set; }
        public string firstvalue { get; set; }
        public string secondvalue { get; set; }
        public string displaymode { get; set; }
        public string periodic { get; set; }
        public int topcount { get; set; }
        public string topfield { get; set; }
        public string title { get; set; }
        public string data { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public ReportBuilder rptrequest { get; set; }
        public string pivotconfig { get; set; }
        public bool accessall { get; set; }
        public List<long> userids { get; set; }
    }
}

public class CheckSumRequest : BaseEntity
{
    public string type { get; set; }
    public string key { get; set; }
    public string data { get; set; }
}


public class CheckSumResponse : CheckSumRequest
{
    public string checksum { get; set; }
}



