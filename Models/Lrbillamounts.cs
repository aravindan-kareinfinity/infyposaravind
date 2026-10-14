using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
public class Lrbillamounts : BaseEntity{
public long lrid { get; set; } 
public long suppliercompanyid { get; set; } 
public long buyingcompanyid { get; set; } 
public decimal totalamount { get; set; } 
public long version { get; set; } 
public long createdby { get; set; } 
public long modifiedby { get; set; } 
public DateTime createdon { get; set; } 
public DateTime modifiedon { get; set; } 
public bool isactive { get; set; } 
public long id { get; set; } 
}
public class LrbillamountsCreteria : Lrbillamounts{
}
}
