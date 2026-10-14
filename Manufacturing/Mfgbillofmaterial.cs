using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
public class Mfgbillofmaterial : BaseEntity{
public long id { get; set; } 
public long mfgworkorderid { get; set; } 
public string bomissuecode { get; set; } 
public long supplierid { get; set; } 
public long issuedby { get; set; } 
public string receivedby { get; set; } 
public bool isactive { get; set; } 
public long createdby { get; set; } 
public long modifiedby { get; set; } 
public int version { get; set; } 
public DateTime createdon { get; set; } 
public DateTime modifiedon { get; set; } 
public DateTime bomdate { get; set; } 
public List<Mfgbillofmaterialitems> Mfgbillofmaterialitems { get; set; }
}
public class MfgbillofmaterialCreteria : Mfgbillofmaterial{
}
}
