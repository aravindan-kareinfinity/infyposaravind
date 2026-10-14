using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class ComponentPO : BaseEntity
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }
        public long preparedby { get; set; }
        public long approvedby { get; set; }
        public long companyid { get; set; }
        public DateTime podate { get; set; }
        public DateTime expectedate { get; set; }
        public long deliverylocationid { get; set; }
        public int freight { get; set; }
        public long agentid { get; set; }
        public string pono { get; set; }
        public long supplierid { get; set; }
        public long dsupplierid { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public List<ComponentPOitemsDetail> ComponentPOitems { get; set; }
        public POStatus status { get; set; }
        public string pobookno { get; set; }
        public string remarks { get; set; }
        public string stdisp { get; set; }
        public string formimageid { get; set; }
        public DateTime pobookdate { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2,
            CANCELLED = 3,
            COMPLETED = 4
        }
    }


    public class ComponentPOCreteria : ComponentPO
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime duefromdate { get; set; }
        public DateTime duetodate { get; set; }
        public int mode { get; set; }
        public long productid { get; set; }
        public int statusid { get; set; }
        public string designid { get; set; }
        public long bomid { get; set; }
        public long categoryid { get; set; }
        public string pono { get; set; }
        public int limit { get; set; }
        public long invoiceid { get; set; }
    }

    public class ComponentPOReportSummary : ComponentPO
    {
        public decimal oqty { get; set; }
        public decimal cqty { get; set; }
        public decimal rqty { get; set; }
        public decimal pqty { get; set; }
    }

    public class ComponentPOReportDetail
    {
        public string name { get; set; }
        public string shade { get; set; }
        public string colour { get; set; }
        public string width { get; set; }
        public string design { get; set; }
        public string supplierbrand { get; set; }
        public string supplierdesign { get; set; }
        public string suppliername { get; set; }
        public string cityname { get; set; }
        public string pono { get; set; }
        public DateTime podate { get; set; }
        public DateTime pobookdate { get; set; }
        public DateTime duedate { get; set; }
        public decimal oqty { get; set; }
        public long id { get; set; }
        public decimal cqty { get; set; }
        public decimal rqty { get; set; }
        public decimal pqty { get; set; }
        public decimal rate { get; set; }
        public decimal amount { get; set; }
    }

    public class ComponentPOItemDetail
    {
        public long componentpoitemsid { get; set; }
        public DateTime podate { get; set; }
        public long componentorderid { get; set; }
        public long colourid { get; set; }
        public string width { get; set; }
        public string pono { get; set; }
        public decimal rate { get; set; }
        public decimal qty { get; set; }
        public decimal cqty { get; set; }
        public decimal rqty { get; set; }
        public decimal tolerance { get; set; }
        public long bomcategoryid { get; set; }
        public long bomid { get; set; }

    }
    public class ComponentPODetails
    {
        public long id { get; set; }
        public decimal tolerance { get; set; }

        public long companyid { get; set; }
        public DateTime podate { get; set; }
        public DateTime expectedate { get; set; }
        public string pono { get; set; }
        public long supplierid { get; set; }
        public POStatus status { get; set; }
        public enum POStatus
        {
            PENDING = 0,
            APPROVED = 1,
            REJECTED = 2
        }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal amount { get; set; }

    }
    public class ComponentPOPrint : ComponentPO
    {
        public decimal totalqty { get; set; }
        public decimal totalvalue { get; set; }

        public double duedays
        {
            get
            {
                if (expectedate > DateTime.MinValue && expectedate > podate)
                    return (expectedate.Date - podate.Date).TotalDays;
                return 0;
            }
        }

        public bool IsSupplierDirectDelivery
        {
            get
            {
                return dsupplierid > 0;
            }
        }

        public Company company { get; set; }
        public Supplier supplier { get; set; }
        public Suppliercompany suppliercompany { get; set; }
        public Supplier dsupplier { get; set; }
        public Suppliercompany dsuppliercompany { get; set; }
        public Transport dtransport { get; set; }
        public Transport transport { get; set; }
        public Agent agent { get; set; }
        public CompanyLocation deliverylocation { get; set; }
        public string whocreated { get; set; }
        public string whoapproved { get; set; }
        public List<ComponentPOitemsDetail> ColourBased { get; set; }
        public bool isinterstatesale { get;  set; }
        public decimal itaxpercentage { get; set; }
        public decimal grossvalue { get; set; }
        public decimal itax { get;  set; }
        public decimal stax { get; internal set; }
        public decimal staxpercentage { get; internal set; }
        public decimal rounding { get; internal set; }

        public string amountinwords
        {
            get
            {
                return WebAPI.Data.Numberconvertor.rupees(totalvalue);
            }
        }

        //public class DeliveryLocation
        //{
        //    public string name { get; set; }
        //    public string address { get; set; }
        //    public string contactnumber { get; set; }
        //    public string emailid { get; set; }
        //    public string contactperson { get; set; }
        //    public string gstno { get; set; }

        //}
    }

    public class ComponentPOItemSimple : BaseEntity
    {
        public int version { get; set; }
        public long cloneid { get; set; }
        public long id { get; set; }
        public long companyid { get; set; }
        public long supplierid { get; set; }
        public long productid { get; set; }
        public long brandid { get; set; }
        public long poid { get; set; }
        public long sizeid { get; set; }
        public decimal qty { get; set; }
        public decimal price { get; set; }
        public string product { get; set; }
        public string brand { get; set; }
        public string pono { get; set; }
        public string size { get; set; }
        public DateTime podate { get; set; }

    }
}
