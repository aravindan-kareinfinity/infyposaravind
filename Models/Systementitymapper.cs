using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Systementitymapper : BaseEntity
    {
        public long id { get; set; }
        public int type { get; set; }
        public long parentid { get; set; }
        public long childid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long companyid { get; set; }
        public long locationid { get; set; }

        public enum MappingTypes
        {
            EmployeeVsProducts = 0
        }
    }


    public class SystemMapperDetail : BaseEntity
    {
        public int type { get; set; }
        public long locationid { get; set; }
        public long companyid { get; set; }
        public long parentid { get; set; }
        public List<ChildIDs> childids { get; set; }
        public DateTime modifiedon { get; set; }
        public long modifiedby { get; set; }
        public bool isactive { get; set; }
        public int version { get; set; }
        public long id { get; set; }

        public class ChildIDs
        {
            public string code { get; set; }
            public long id { get; set; }
            public string name { get; set; }

        }
    }

    public class SystementitymapperCreteria : Systementitymapper
    {
    }
}
