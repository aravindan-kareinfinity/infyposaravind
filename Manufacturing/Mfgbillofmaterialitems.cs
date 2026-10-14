using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
public class Mfgbillofmaterialitems : BaseEntity{
public long id { get; set; } 
public long mfgbillofmaterialid { get; set; } 
public long bomid { get; set; } 
public long bomtypeid { get; set; } 
public decimal qty { get; set; } 
public decimal returnqty { get; set; } 
public long taxpercentage { get; set; } 
public decimal cost { get; set; } 
public decimal tax { get; set; } 
public decimal total { get; set; } 
public bool isfabric { get; set; } 
public bool isactive { get; set; } 
public long createdby { get; set; } 
public long modifiedby { get; set; } 
public int version { get; set; } 
public DateTime createdon { get; set; } 
public DateTime modifiedon { get; set; } 
}
public class MfgbillofmaterialitemsCreteria : Mfgbillofmaterialitems{
}
}
