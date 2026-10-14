using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
public class Bompricehistory : BaseEntity{
public long id { get; set; } 
public bool isactive { get; set; } 
public long createdby { get; set; } 
public long modifiedby { get; set; } 
public int version { get; set; } 
public DateTime createdon { get; set; } 
public DateTime modifiedon { get; set; } 
public long bomid { get; set; } 
public decimal cost { get; set; } 
}
public class BompricehistoryCreteria : Bompricehistory{
}
}
