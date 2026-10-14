using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Mfgworkfloworchestration : BaseEntity
    {
        public long id { get; set; }
        public string name { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string stages_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(stages); } set { stages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Stages>>(value); } }
        public List<Stages> stages { get; set; }
        public string mode { get; set; }
        public string stagesupport { get; set; }
        public string bomsupport { get; set; }
        public string fgsupport { get; set; }
        public string inwardsupport { get; set; }
        public long tailoringchargeid { get; set; }
        public int tailoringmode { get; set; }
        public enum IOMODE
        {
            ComponentInput_1 = 1,
            ComponentOutput_1 = 2,
            ComponentInput_2 = 3,
            ComponentOutput_2 = 4,
            ProductInput = 5,
            ProductOutput = 6
        }

        public enum CALCULATIONSOURCE
        {
            WorkOrder = 1,
            PriousOrderIncome = 2,
            PriousOrderOutcome = 3
        }

        public enum CALCULATIONMODE
        {
            Fixed = 0,
            Multiply = 1,
            Divide = 2
        }
        public class Stages
        {
            public string name { get; set; }
            public int sourcematerial { get; set; }
            public int outcomematerial { get; set; }
            public long departmentid { get; set; }
            public int corestageid { get; set; }
            public long mfgstageid { get; set; }
            public int index { get; set; }
            public long storelocation { get; set; }
            public List<BOMItem> bomitems { get; set; }
            public long joborderchargeid { get; set; }
            public bool hasfixedcharge { get; set; }
            public int duration { get; set; }
            public int jobordersourceid { get; set; }
            public decimal fixedcharge { get; set; }
            public bool supportinward { get; set; }
            public IOMODE Jobordersource
            {
                get
                {
                    return (IOMODE)jobordersourceid;
                }
            }
            public class BOMItem
            {
               
                public IOMODE eIOMODE
                {
                    get
                    {
                        return (IOMODE)iomode;
                    }
                }
                public decimal cost { get; set; }
                public long type { get; set; }
                public long mode { get; set; }
                public long stageid { get; set; }
                public int iomode { get; set; }
                public long bomcategoryid { get; set; }
                public long bomid { get; set; }
                public long chargeid { get; set; }
                public long productid { get; set; }
                public long colourid { get; set; }
                public long sizeid { get; set; }
                public CALCULATIONMODE eCALCULATIONMODE
                {
                    get
                    {
                        return (CALCULATIONMODE)calculationmode;
                    }
                }
                public int calculationmode { get; set; }
                public CALCULATIONSOURCE eCALCULATIONSOURCE
                {
                    get
                    {
                        return (CALCULATIONSOURCE)calculationsource;
                    }
                }
                public int calculationsource { get; set; }
                public IOMODE eCalculationSourceMatierial
                {
                    get
                    {
                        return (IOMODE)calculationsourcematierial;
                    }
                }

                public int calculationsourcematierial { get; set; }
                public decimal calculationvalue { get; set; }
            }
        }
    }
    public class MfgworkfloworchestrationCreteria : Mfgworkfloworchestration
    {
        public long workorderid { get; set; }
    }
}
