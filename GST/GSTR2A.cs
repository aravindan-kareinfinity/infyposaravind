using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfyPOS.GST
{
    public class GSTR2A
    {
        public partial class RootObject
        {
            [JsonProperty("b2b")]
            public B2B[] B2B { get; set; }
            public static RootObject Create(string data)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(data, new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            }
        }

        public partial class B2B
        {
            [JsonProperty("inv")]
            public Inv[] Inv { get; set; }

            [JsonProperty("cfs")]
            public string Cfs { get; set; }

            [JsonProperty("ctin")]
            public string Ctin { get; set; }

            [JsonProperty("cfs3b")]
            public string Cfs3B { get; set; }

            [JsonProperty("flprdr1")]
            public string Flprdr1 { get; set; }
        }

        public partial class Inv
        {
            [JsonProperty("itms")]
            public Itm[] Itms { get; set; }

            [JsonProperty("val")]
            public double Val { get; set; }

            [JsonProperty("inv_typ")]
            public string InvTyp { get; set; }

            [JsonProperty("pos")]
            public long Pos { get; set; }

            [JsonProperty("idt")]
            public string Idt { get; set; }

            [JsonProperty("rchrg")]
            public string Rchrg { get; set; }

            [JsonProperty("inum")]
            public string Inum { get; set; }

            [JsonProperty("chksum")]
            public string Chksum { get; set; }
        }

        public partial class Itm
        {
            [JsonProperty("num")]
            public long Num { get; set; }

            [JsonProperty("itm_det")]
            public ItmDet ItmDet { get; set; }
        }

        public partial class ItmDet
        {
            [JsonProperty("samt")]
            public double Samt { get; set; }

            [JsonProperty("rt")]
            public long Rt { get; set; }

            [JsonProperty("txval")]
            public long Txval { get; set; }

            [JsonProperty("camt")]
            public double Camt { get; set; }
        }

    }
}