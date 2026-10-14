using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Usersession : BaseEntity
    {
        public long userid { get; set; }
        public string username { get; set; }
        public string clientid { get; set; }
        public long employeeid { get; set; }
        public int utcoffset { get; set; }
        public string sessionkilledby { get; set; }
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public string token { get; set; }
        public string refreshtoken { get; set; }
        public DateTime tokenexpired { get; set; }
        public DateTime refreshtokenexpired { get; set; }
        public long killedBy { get; set; }
        public DateTime loginOn { get; set; }
        public DateTime logoutOn { get; set; }
        public DateTime refreshtokenon { get; set; }
        public string refereshtokenhash { get; set; }
        public string protectedticket { get; set; }
        public string platform { get; set; }
        public string deviceid { get; set; }
        public string reason { get; set; }
        public int removeusercount { get;  set; }
    }
    public class UsersessionCreteria : Usersession
    {
    }
}
