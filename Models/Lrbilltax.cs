using InfyPOS.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
public class Lrbilltax : BaseEntity{
public long lrid { get; set; } 
public long chargetypeid { get; set; } 
public float amounton { get; set; } 
public string percentage { get; set; } 
public float netamount { get; set; } 
public long createdby { get; set; } 
public long modifiedby { get; set; } 
public DateTime createdon { get; set; } 
public DateTime modifiedon { get; set; } 
public int version { get; set; } 
public bool isactive { get; set; } 
public long id { get; set; } 
}
public class LrbilltaxCreteria : Lrbilltax{
}
}
