using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgbinstorage : BaseEntity
    {
        [JsonIgnore]
        public BinStorageRefrenceList.Node.StorageBin basebin { get; set; }
        public Mfgbinstorage()
        {
        }
        public Mfgbinstorage(BinStorageRefrenceList.Node.StorageBin bin, BinStorageRefrenceList.Node node)
        {
            parentid = node.id;
            isactive = true;
            version = 1;
            height = node.height;
            width = node.width;
            rowindex = bin.row;
            columnindex = bin.column;
            code = bin.code;
            this.basebin = bin;
        }

        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long locationid { get; set; }
        public string code { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int rowindex { get; set; }
        public int columnindex { get; set; }
        public decimal maxqty { get; set; }
        public decimal allocated { get; set; }
        public string parentid { get; set; }
        public bool disabled { get; set; }
        public string uniquecode { get;  set; }
        public List<Mfgbinstoragestockbase> stocklist { get; set; }
    }
    public class MfgbinstorageCreteria : Mfgbinstorage
    {
        public List<long> ids { get;  set; }
    }
}
