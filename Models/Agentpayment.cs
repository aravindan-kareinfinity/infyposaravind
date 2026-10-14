using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Agentpayment : BaseEntity
    {
        public long locationid { get; set; }
        public decimal tdsamount { get; set; }
        public decimal rounding { get; set; }
        public long id { get; set; }
        public long companyid { get; set; }
        public int version { get; set; }
        public decimal paidamount { get; set; }
        public DateTime createdon { get; set; }
        public long modifiedby { get; set; }
        public DateTime modifiedon { get; set; }
        public bool datatransmitted { get; set; }
        public DateTime paymentdate { get; set; }
        public long bankid { get; set; }
        public long agentid { get; set; }
        public long createdby { get; set; }
        public bool isactive { get; set; }
        public int paymentmode { get; set; }
        public DateTime instrumentdate { get; set; }
        public int sourcemode { get; set; }
        public bool hastds { get; set; }
        public decimal tdspercentage { get; set; }
        public string referencedetail { get; set; }
        public string remarks { get; set; }
        public string receiptno { get; set; }
        public List<Bankchequetransaction> bankcheques { get; set; }
        public List<Agentpaymentinvoice> Agentpaymentinvoice { get; set; }
        public TransportPayment.PaymentInfo paymentinfo { get; set; }
        public string bankname { get;  set; }
    }
    public class AgentpaymentCreteria : Agentpayment
    {
        public string companygst { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
}
