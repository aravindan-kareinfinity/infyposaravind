using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
public class Tailoringmeasurement : BaseEntity{
public long id { get; set; } 
public long productid { get; set; } 
public string code { get; set; } 
[JsonIgnore]
[JsonProperty(Required = Required.Default)]
public string measurementconfig_json { get {return Newtonsoft.Json.JsonConvert.SerializeObject(measurementconfig);} set{ measurementconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Measurementconfig>(value);} } 
public Measurementconfig measurementconfig { get; set; } 
public int version { get; set; } 
public bool isactive { get; set; } 
public long modifiedby { get; set; } 
public long createdby { get; set; } 
public DateTime modifiedon { get; set; } 
public DateTime createdon { get; set; } 
public class Measurementconfig{
}
}
public class TailoringmeasurementCreteria : Tailoringmeasurement{
}
}
