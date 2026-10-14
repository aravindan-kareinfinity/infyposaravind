using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class PermissionSlip : BaseEntity
    {
        public long id { get; set; }
        public DateTime fromleavedate { get; set; }
        public DateTime toleavedate { get; set; }
        public DateTime requestedon { get; set; }
        public string fromtime { get; set; }
        public string totime { get; set; }
        public long employeeid { get; set; }
        public long approvedby { get; set; }
        public string reason { get; set; }
        public bool ishalfday { get; set; }
        public bool deductsalary { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int mode { get; set; }
        public string name { get; set; }

        public string employeecode { get; set; }


    }
    public class PermissionSlipCreteria : PermissionSlip
    {
        public long companyid { get; set; }
        public bool inactive { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class PermissionSlipDetail : PermissionSlip
    {
        public string designation { get; set; }
        public string sectionname { get; set; }
        public string department { get; set; }
        public string floor { get; set; }
        public string locationname { get; set; }
    }

    public class PermissionSlipPrint : PermissionSlipDetail
    {
        public Company company { get; set; }
        public bool isonduty
        {
            get
            {
                return mode == 3;
            }
        }
        public bool isleaverequest
        {
            get
            {
                return mode == 2;
            }
        }
        public bool ispermission
        {
            get
            {
                return mode == 1;
            }
        }

        
    }
}
