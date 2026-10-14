
using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkordergrn : BaseEntity
    {
        public Mfgworkordergrn()
        {
        }
        public Mfgworkordergrn(Mfgworkorder mfgworkorder)
        {

            this.isactive = true;
            this.version = 1;
            this.workorderid = mfgworkorder.id;
            this.workorderno = mfgworkorder.workorderno;
            this.companyid = mfgworkorder.companyid;
            this.locationid = mfgworkorder.locationid;
            this.mode = mfgworkorder.mode;
            this.suppliercompanyid = mfgworkorder.suppliercompanyid;
            this.supplierid = mfgworkorder.supplierid;
            this.tax = mfgworkorder.tax;
            var received = mfgworkorder.Mfgworkorderitems.Sum(e => e.received);
            this.taxable = (mfgworkorder.taxable > 0 &&
                mfgworkorder.totalqty > 0) ? (mfgworkorder.taxable / mfgworkorder.totalqty) * (mfgworkorder.totalqty - received) : 0;
            this.total = (mfgworkorder.taxable > 0 &&
                mfgworkorder.totalqty > 0) ? (mfgworkorder.total / mfgworkorder.totalqty) * (mfgworkorder.totalqty - received) : 0;
            this.totalqty = received;
            this.design = mfgworkorder.design;
            this.product = mfgworkorder.product;
            this.workorderdate = mfgworkorder.wodate;
            this.sizelist = new List<SizeInfo>();

            foreach (var item in mfgworkorder.sizelist)
            {
                this.sizelist.Add(new SizeInfo()
                {
                    name = item.name,
                    sizeid = item.sizeid,
                    sizeindex = item.sizeindex
                });
            }

            this.Mfgworkordergrnitems = new List<Mfgworkordergrnitems>();
            foreach (var item in mfgworkorder.Mfgworkorderitems)
            {
                var newitem = new Models.Mfgworkordergrnitems()
                {
                    isactive = true,
                    version = 1,
                    productid = item.productid,
                    amount = item.amount > 0 && item.outqty > 0 ? (item.amount / item.outqty) * (item.outqty - item.received) : 0,
                    colourid = item.colourid,
                    cost = item.cost,
                    designid = item.designid,
                    designname = item.designname,
                    designitemcode = item.designitemname,
                    mfgworkorderitemsid = item.id,
                    received = item.outqty - item.received,
                    size = item.size,
                    sizelist = new List<mfgworkordergrnitemsizes>(),
                    taxamount = item.amount > 0 && item.outqty > 0 ? (item.taxamount / item.outqty) * (item.outqty - item.received) : 0,
                    taxid = item.taxid,
                    taxpercentage = item.taxpercentage,
                    sleeveid = item.sleeveid
                };
                newitem.sizelist = new List<mfgworkordergrnitemsizes>();

                foreach (var sitem in item.sizelist)
                {
                    newitem.sizelist.Add(new mfgworkordergrnitemsizes()
                    {
                        isactive = true,
                        version = 1,
                        colourid = sitem.colourid,
                        designid = sitem.designid,
                        eqty = sitem.qty,
                        dqty = sitem.delivered,
                        sizeid = sitem.sizeid,
                        sizeindex = sitem.sizeindex,
                        itemid = sitem.itemid,
                        sleeveid = sitem.sleeveid,
                        uid = sitem.uid,
                        mfgworkorderitemsizesid = sitem.id,
                        dealerprice = sitem.dealerprice,
                        wsrate = sitem.wsrate,
                        salerate = sitem.salerate,
                        cost = sitem.cost,
                        mfgworkorderid = mfgworkorder.id,
                        mfgworkorderitemsid = item.id
                    });
                }
                this.Mfgworkordergrnitems.Add(newitem);
            }
        }

        public long id { get; set; }
        public string entryno { get; set; }
        public string workorderno { get; set; }
        public long locationid { get; set; }
        public long lrentryid { get; set; }
        public string lrentryno { get; set; }
        public long workorderid { get; set; }
        public long joborderid { get; set; }
        public DateTime entrydate { get; set; }
        public DateTime invoicedate { get; set; }
        public DateTime lrdate { get; set; }
        public string invoiceno { get; set; }
        public bool isinterstatesale { get; set; }
        public string lrno { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public decimal tax { get; set; }
        public decimal taxable { get; set; }
        public decimal total { get; set; }
        public int statusid { get; set; }
        public long supplierid { get; set; }
        public long suppliercompanyid { get; set; }
        public decimal totalqty { get; set; }
        public long receivedby { get; set; }
        public long companyid { get; set; }
        public long approvedby { get; set; }
        public DateTime approvedon { get; set; }
        public string mode { get; set; }
        public string design { get; set; }
        public string product { get; set; }
        public bool inward2warehouse { get; set; }
        public long receivinglocationid { get; set; }
        public DateTime workorderdate { get; set; }
        public List<Mfgworkordergrnitems> Mfgworkordergrnitems { get; set; }
        
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string sizelist_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(sizelist); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    sizelist = new List<SizeInfo>();
                else
                    sizelist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SizeInfo>>(value);
            }
        }
        public List<SizeInfo> sizelist { get; set; }
        public class SizeInfo
        {
            public long sizeid { get; set; }
            public int sizeindex { get; set; }
            public string name { get; set; }
        }



        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string attributes_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(attributes); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    attributes = new Attributes();
                else
                    attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Attributes>(value);
            }
        }
        public class StatusChange : BaseEntity
        {
            public int status { get; set; }
            public long employeeid { get; set; }
            public DateTime actionon { get; set; }
            public List<long> ids { get; set; }
        }
        public Attributes attributes { get; set; }
        public class Attributes
        {
            public long id { get; set; }
            public Status Opened { get; set; }
            public Status Barcoded { get; set; }
            public Status Racking { get; set; }
            public class Status
            {
                public DateTime date { get; set; }
                public long employeeid { get; set; }
            }
        }
    }

    public class MfgworkordergrnDetail : Mfgworkordergrn
    {
        public Supplier supplier { get; internal set; }
        public Suppliercompany suppliercompany { get; internal set; }
        public Company company { get; internal set; }
    }
    public class MfgworkordergrnCreteria : Mfgworkordergrn
    {
        public bool creategrn { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime entryfromdate { get; set; }
        public DateTime entrytodate { get; set; }
        public DateTime wofromdate { get; set; }
        public DateTime wotodate { get; set; }
    }
    public class GRNPrint : BaseEntity
    {
        public long reportid { get; set; }
        public List<GRNPrintItems> items { get; set; }
        public class GRNPrintItems
        {
            public int sno { get; set; }
            public long grnid { get; set; }
            public long id { get; set; }
            public string designitemcode { get; set; }
            public string uid { get; set; }
            public long designid { get; set; }
            public int pqty { get; set; }
        }
    }
    public class GRNPrint2Print : Design2Print
    {
        public override bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            return base.SpecialField(field, data);
        }
        public long grnid { get; set; }
        public string sno { get; set; }
        public string suppliername { get; set; }
        public string suppliercode { get; set; }
        public DateTime grndate { get; set; }
        public DateTime invoicedate { get; set; }
        public string grnno { get; set; }
        public string invoiceno { get; set; }
        public int pqty { get; set; }
        [JsonIgnore]
        public override SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("INVOICENO", invoiceno);
                ret.Add("DATE", grndate.ToString("dd-MM-yyyy"));
                ret.Add("INVOICEDATE", invoicedate.ToString("dd-MM-yyyy"));
                ret.Add("SNO", sno);
                ret.Add("SUPPLIERNAME", suppliername);
                ret.Add("SUPPLIERCODE", suppliercode);
                ret.Add("GRNNO", grnno);
                base.KeyValue.ToList().ForEach(x =>
                {
                    if (!ret.ContainsKey(x.Key))
                        ret.Add(x.Key, x.Value);
                });
                return ret;
            }
        }
        [JsonIgnore]
        public override SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("QTY", "N0");
                base.KeyFormat.ToList().ForEach(x =>
                {
                    if (!ret.ContainsKey(x.Key))
                        ret.Add(x.Key, x.Value);
                });
                return ret;
            }
        }

    }
}
