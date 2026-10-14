using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfyPOS.GST
{
    public class GSTR1
    {
        public partial class RootObject
        {
            [JsonProperty("gstin")]
            public string Gstin { get; set; }

            [JsonProperty("fp")]
            public string Fp { get; set; }

            [JsonProperty("gt")]
            public long Gt { get; set; }

            [JsonProperty("cur_gt")]
            public long CurGt { get; set; }

            [JsonProperty("b2b")]
            public B2B[] B2B { get; set; }

            [JsonProperty("b2cs")]
            public B2C[] B2Cs { get; set; }

            [JsonProperty("cdnr")]
            public Cdnr[] Cdnr { get; set; }

            [JsonProperty("hsn")]
            public Hsn Hsn { get; set; }

            [JsonProperty("doc_issue")]
            public DocIssue DocIssue { get; set; }
        }

        public partial class B2B
        {
            [JsonProperty("ctin")]
            public string Ctin { get; set; }

            [JsonProperty("inv")]
            public Inv[] Inv { get; set; }
        }

        public partial class Inv
        {
            [JsonProperty("inum")]
            public string Inum { get; set; }

            [JsonProperty("idt")]
            public string Idt { get; set; }

            [JsonProperty("val")]
            public long Val { get; set; }

            [JsonProperty("pos")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Pos { get; set; }

            [JsonProperty("rchrg")]
            public Rchrg Rchrg { get; set; }

            [JsonProperty("itms")]
            public Itm[] Itms { get; set; }

            [JsonProperty("inv_typ")]
            public InvTyp InvTyp { get; set; }
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
            [JsonProperty("txval")]
            public long Txval { get; set; }

            [JsonProperty("rt")]
            public long Rt { get; set; }

            [JsonProperty("camt", NullValueHandling = NullValueHandling.Ignore)]
            public double? Camt { get; set; }

            [JsonProperty("samt", NullValueHandling = NullValueHandling.Ignore)]
            public double? Samt { get; set; }

            [JsonProperty("csamt")]
            public long Csamt { get; set; }

            [JsonProperty("iamt", NullValueHandling = NullValueHandling.Ignore)]
            public long? Iamt { get; set; }
        }

        public partial class B2C
        {
            [JsonProperty("rt")]
            public long Rt { get; set; }

            [JsonProperty("sply_ty")]
            public string SplyTy { get; set; }

            [JsonProperty("pos")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long Pos { get; set; }

            [JsonProperty("typ")]
            public string Typ { get; set; }

            [JsonProperty("txval")]
            public double Txval { get; set; }

            [JsonProperty("camt")]
            public double Camt { get; set; }

            [JsonProperty("samt")]
            public double Samt { get; set; }

            [JsonProperty("csamt")]
            public long Csamt { get; set; }
        }

        public partial class Cdnr
        {
            [JsonProperty("ctin")]
            public string Ctin { get; set; }

            [JsonProperty("nt")]
            public Nt[] Nt { get; set; }
        }

        public partial class Nt
        {
            [JsonProperty("ntty")]
            public string Ntty { get; set; }

            [JsonProperty("nt_num")]
            public string NtNum { get; set; }

            [JsonProperty("nt_dt")]
            public string NtDt { get; set; }

            [JsonProperty("p_gst")]
            public Rchrg PGst { get; set; }

            [JsonProperty("inum")]
            public string Inum { get; set; }

            [JsonProperty("idt")]
            public string Idt { get; set; }

            [JsonProperty("val")]
            public long Val { get; set; }

            [JsonProperty("itms")]
            public Itm[] Itms { get; set; }
        }

        public partial class DocIssue
        {
            [JsonProperty("doc_det")]
            public DocDet[] DocDet { get; set; }
        }

        public partial class DocDet
        {
            [JsonProperty("doc_num")]
            public long DocNum { get; set; }

            [JsonProperty("doc_typ")]
            public string DocTyp { get; set; }

            [JsonProperty("docs")]
            public Doc[] Docs { get; set; }
        }

        public partial class Doc
        {
            [JsonProperty("num")]
            public long Num { get; set; }

            [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
            public string From { get; set; }

            [JsonProperty("totnum")]
            public long Totnum { get; set; }

            [JsonProperty("cancel")]
            public long Cancel { get; set; }

            [JsonProperty("net_issue")]
            public long NetIssue { get; set; }

            [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
            public string To { get; set; }
        }

        public partial class Hsn
        {
            [JsonProperty("data")]
            public Datum[] Data { get; set; }
        }

        public partial class Datum
        {
            [JsonProperty("num")]
            public long Num { get; set; }

            [JsonProperty("hsn_sc")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long HsnSc { get; set; }

            [JsonProperty("desc")]
            public string Desc { get; set; }

            [JsonProperty("uqc")]
            public string Uqc { get; set; }

            [JsonProperty("qty")]
            public long Qty { get; set; }

            [JsonProperty("val")]
            public double Val { get; set; }

            [JsonProperty("txval")]
            public double Txval { get; set; }

            [JsonProperty("iamt")]
            public long Iamt { get; set; }

            [JsonProperty("camt")]
            public double Camt { get; set; }

            [JsonProperty("samt")]
            public double Samt { get; set; }

            [JsonProperty("csamt")]
            public long Csamt { get; set; }
        }

        public enum InvTyp { R };

        public enum Rchrg { N };

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                InvTypConverter.Singleton,
                RchrgConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class InvTypConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(InvTyp) || t == typeof(InvTyp?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "R")
                {
                    return InvTyp.R;
                }
                throw new Exception("Cannot unmarshal type InvTyp");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (InvTyp)untypedValue;
                if (value == InvTyp.R)
                {
                    serializer.Serialize(writer, "R");
                    return;
                }
                throw new Exception("Cannot marshal type InvTyp");
            }

            public static readonly InvTypConverter Singleton = new InvTypConverter();
        }

        internal class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                long l;
                if (Int64.TryParse(value, out l))
                {
                    return l;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (long)untypedValue;
                serializer.Serialize(writer, value.ToString());
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }

        internal class RchrgConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(Rchrg) || t == typeof(Rchrg?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "N")
                {
                    return Rchrg.N;
                }
                throw new Exception("Cannot unmarshal type Rchrg");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (Rchrg)untypedValue;
                if (value == Rchrg.N)
                {
                    serializer.Serialize(writer, "N");
                    return;
                }
                throw new Exception("Cannot marshal type Rchrg");
            }

            public static readonly RchrgConverter Singleton = new RchrgConverter();
        }
    }

}
