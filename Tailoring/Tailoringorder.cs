using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Tailoringorder : BaseEntity
    {
        public long id { get; set; }
        public string customername { get; set; }
        public string customermobile { get; set; }
        public string customeraddress { get; set; }

        public long rackid { get; set; }
        public string cardno { get; set; }
        public string rackno { get; set; }
        public decimal additionalcharge { get; set; }

        public string entryno { get; set; }
        public string notes { get; set; }
        public string orderno { get; set; }
        public long customerid { get; set; }
        public long locationid { get; set; }
        public long stitchinglocationid { get; set; }
        public long supplierid { get; set; }
        public long companyid { get; set; }
        public DateTime receivedon { get; set; }
        public DateTime expectedon { get; set; }

        [JsonIgnore]
        public DateTime plannedon
        {
            get
            {
                if (DateTime.MinValue != expectedon.Date)
                    return expectedon.Date.AddDays(-1);

                return DateTime.MinValue;
            }
        }
        public int status { get; set; }
        public DateTime statuson { get; set; }
        public decimal gross { get; set; }
        public decimal tax { get; set; }
        public decimal total { get; set; }
        public decimal expectedqty { get; set; }
        public decimal receivedqty { get; set; }
        public decimal advancepaid { get; set; }
        public decimal issuedqty { get; set; }
        public decimal deliveredqty { get; set; }
        public decimal returnqty { get; set; }
        public DateTime createdon { get; set; }
        public int version { get; set; }
        public bool isactive { get; set; }
        public long modifiedby { get; set; }
        public long createdby { get; set; }
        public long receivedby { get; set; }
        public DateTime modifiedon { get; set; }
        public List<Tailoringitems> Tailoringitems { get; set; }
        public List<Tailoringitemsdelivered> TailoringDelivery { get; set; }
        public List<TailoringPaymentDetail> TailoringPayments { get; set; }
        public string printdocument { get; set; }
    }

    public class TailoringorderPrint : Tailoringorder
    {
        public Customer customer { get; set; }
        public CompanyLocation location { get; set; }
        public Company company { get; set; }
        public string receivedemployee { get;  set; }
    }
    public class TailoringorderCreteria : Tailoringorder
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public DateTime expfromdate { get; set; }
        public DateTime exptodate { get; set; }
        public long tailorid { get; set; }
        public bool showdetail { get; set; }
        public bool foroutward { get; set; }
        public bool forinward { get; set; }
        public long measurementid { get; set; }
        public List<long> ids { get;  set; }
    }
}
