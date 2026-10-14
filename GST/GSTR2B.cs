using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfyPOS.GST
{
    public class GSTR2B
    {
        public partial class RootObject
        {
            [JsonProperty("chksum")]
            public string Chksum { get; set; }

            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        public partial class Data
        {
            [JsonProperty("gstin")]
            public string Gstin { get; set; }

            [JsonProperty("rtnprd")]
            public string Rtnprd { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("gendt")]
            public string Gendt { get; set; }

            [JsonProperty("itcsumm")]
            public Itcsumm Itcsumm { get; set; }

            [JsonProperty("cpsumm")]
            public Dictionary<string, Cpsumm[]> Cpsumm { get; set; }

            [JsonProperty("docdata")]
            public Docdata Docdata { get; set; }
        }

        public partial class Cpsumm
        {
            [JsonProperty("ctin")]
            public string Ctin { get; set; }

            [JsonProperty("trdnm")]
            public string Trdnm { get; set; }

            [JsonProperty("supprd")]
            public string Supprd { get; set; }

            [JsonProperty("supfildt")]
            public string Supfildt { get; set; }

            [JsonProperty("txval")]
            public double Txval { get; set; }

            [JsonProperty("cess")]
            public long Cess { get; set; }

            [JsonProperty("cgst")]
            public double Cgst { get; set; }

            [JsonProperty("igst")]
            public double Igst { get; set; }

            [JsonProperty("sgst")]
            public double Sgst { get; set; }

            [JsonProperty("ttldocs")]
            public long Ttldocs { get; set; }

            [JsonProperty("nttyp", NullValueHandling = NullValueHandling.Ignore)]
            public NttypEnum? Nttyp { get; set; }
        }

        public partial class Docdata
        {
            [JsonProperty("b2b")]
            public B2B[] B2B { get; set; }

            [JsonProperty("b2ba")]
            public B2Ba[] B2Ba { get; set; }

            [JsonProperty("cdnr")]
            public B2B[] Cdnr { get; set; }
        }

        public partial class B2B
        {
            [JsonProperty("ctin")]
            public string CTIN { get; set; }

            [JsonProperty("trdnm")]
            public string Trdnm { get; set; }

            [JsonProperty("supfildt")]
            public string Supfildt { get; set; }

            [JsonProperty("supprd")]
            public string Supprd { get; set; }

            [JsonProperty("inv", NullValueHandling = NullValueHandling.Ignore)]
            public B2BInv[] Invoices { get; set; }

            [JsonProperty("nt", NullValueHandling = NullValueHandling.Ignore)]
            public Nt[] Nt { get; set; }
        }

        public partial class B2BInv
        {
            [JsonProperty("inum")]
            public string Inum { get; set; }

            [JsonProperty("typ")]
            public SuptypEnum Typ { get; set; }

            [JsonProperty("dt")]
            public string Dt { get; set; }

            [JsonProperty("val")]
            public double Val { get; set; }

            [JsonProperty("pos")]
            public string Pos { get; set; }

            [JsonProperty("rev")]
            public ItcavlEnum Rev { get; set; }

            [JsonProperty("itcavl")]
            public ItcavlEnum Itcavl { get; set; }

            [JsonProperty("rsn")]
            public string Rsn { get; set; }

            [JsonProperty("diffprcnt")]
            public long Diffprcnt { get; set; }

            [JsonProperty("items")]
            public Item[] Items { get; set; }

            [JsonProperty("srctyp", NullValueHandling = NullValueHandling.Ignore)]
            public string Srctyp { get; set; }

            [JsonProperty("irn", NullValueHandling = NullValueHandling.Ignore)]
            public string Irn { get; set; }

            [JsonProperty("irngendate", NullValueHandling = NullValueHandling.Ignore)]
            public string Irngendate { get; set; }
        }

        public partial class Item
        {
            [JsonProperty("num")]
            public long Num { get; set; }

            [JsonProperty("rt")]
            public long Rt { get; set; }

            [JsonProperty("txval")]
            public double Txval { get; set; }

            [JsonProperty("cgst", NullValueHandling = NullValueHandling.Ignore)]
            public double? Cgst { get; set; }

            [JsonProperty("sgst", NullValueHandling = NullValueHandling.Ignore)]
            public double? Sgst { get; set; }

            [JsonProperty("cess", NullValueHandling = NullValueHandling.Ignore)]
            public long? Cess { get; set; }

            [JsonProperty("igst", NullValueHandling = NullValueHandling.Ignore)]
            public double? Igst { get; set; }
        }

        public partial class Nt
        {
            [JsonProperty("dt")]
            public string Dt { get; set; }

            [JsonProperty("diffprcnt")]
            public long Diffprcnt { get; set; }

            [JsonProperty("ntnum")]
            public string Ntnum { get; set; }

            [JsonProperty("val")]
            public double Val { get; set; }

            [JsonProperty("rsn")]
            public string Rsn { get; set; }

            [JsonProperty("suptyp")]
            public SuptypEnum Suptyp { get; set; }

            [JsonProperty("typ")]
            public NttypEnum Typ { get; set; }

            [JsonProperty("itcavl")]
            public ItcavlEnum Itcavl { get; set; }

            [JsonProperty("pos")]
            public string Pos { get; set; }

            [JsonProperty("rev")]
            public ItcavlEnum Rev { get; set; }

            [JsonProperty("items")]
            public Item[] Items { get; set; }
        }

        public partial class B2Ba
        {
            [JsonProperty("ctin")]
            public string Ctin { get; set; }

            [JsonProperty("trdnm")]
            public string Trdnm { get; set; }

            [JsonProperty("supfildt")]
            public string Supfildt { get; set; }

            [JsonProperty("supprd")]
            public string Supprd { get; set; }

            [JsonProperty("inv")]
            public B2BaInv[] Inv { get; set; }
        }

        public partial class B2BaInv
        {
            [JsonProperty("oinum")]
            public string Oinum { get; set; }

            [JsonProperty("oidt")]
            public string Oidt { get; set; }

            [JsonProperty("inum")]
            public string Inum { get; set; }

            [JsonProperty("typ")]
            public SuptypEnum Typ { get; set; }

            [JsonProperty("dt")]
            public string Dt { get; set; }

            [JsonProperty("val")]
            public long Val { get; set; }

            [JsonProperty("pos")]
            public string Pos { get; set; }

            [JsonProperty("rev")]
            public ItcavlEnum Rev { get; set; }

            [JsonProperty("itcavl")]
            public ItcavlEnum Itcavl { get; set; }

            [JsonProperty("rsn")]
            public string Rsn { get; set; }

            [JsonProperty("diffprcnt")]
            public long Diffprcnt { get; set; }

            [JsonProperty("items")]
            public Item[] Items { get; set; }
        }

        public partial class Itcsumm
        {
            [JsonProperty("itcavl")]
            public ItcavlClass Itcavl { get; set; }
        }

        public partial class ItcavlClass
        {
            [JsonProperty("nonrevsup")]
            public Revsup Nonrevsup { get; set; }

            [JsonProperty("revsup")]
            public Revsup Revsup { get; set; }

            [JsonProperty("othersup")]
            public Othersup Othersup { get; set; }
        }

        public partial class Revsup
        {
            [JsonProperty("igst")]
            public double Igst { get; set; }

            [JsonProperty("cgst")]
            public double Cgst { get; set; }

            [JsonProperty("sgst")]
            public double Sgst { get; set; }

            [JsonProperty("cess")]
            public long Cess { get; set; }

            [JsonProperty("b2b")]
            public Othersup B2B { get; set; }

            [JsonProperty("b2ba")]
            public Othersup B2Ba { get; set; }
        }

        public partial class Othersup
        {
            [JsonProperty("igst")]
            public double Igst { get; set; }

            [JsonProperty("cgst")]
            public double Cgst { get; set; }

            [JsonProperty("sgst")]
            public double Sgst { get; set; }

            [JsonProperty("cess")]
            public long Cess { get; set; }

            [JsonProperty("cdnr", NullValueHandling = NullValueHandling.Ignore)]
            public Othersup Cdnr { get; set; }
        }

        public enum NttypEnum { C };

        public enum ItcavlEnum { N, Y };

        public enum SuptypEnum { R };

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                NttypEnumConverter.Singleton,
                ItcavlEnumConverter.Singleton,
                SuptypEnumConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class NttypEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(NttypEnum) || t == typeof(NttypEnum?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "C")
                {
                    return NttypEnum.C;
                }
                throw new Exception("Cannot unmarshal type NttypEnum");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (NttypEnum)untypedValue;
                if (value == NttypEnum.C)
                {
                    serializer.Serialize(writer, "C");
                    return;
                }
                throw new Exception("Cannot marshal type NttypEnum");
            }

            public static readonly NttypEnumConverter Singleton = new NttypEnumConverter();
        }

        internal class ItcavlEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(ItcavlEnum) || t == typeof(ItcavlEnum?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                switch (value)
                {
                    case "N":
                        return ItcavlEnum.N;
                    case "Y":
                        return ItcavlEnum.Y;
                }
                throw new Exception("Cannot unmarshal type ItcavlEnum");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (ItcavlEnum)untypedValue;
                switch (value)
                {
                    case ItcavlEnum.N:
                        serializer.Serialize(writer, "N");
                        return;
                    case ItcavlEnum.Y:
                        serializer.Serialize(writer, "Y");
                        return;
                }
                throw new Exception("Cannot marshal type ItcavlEnum");
            }

            public static readonly ItcavlEnumConverter Singleton = new ItcavlEnumConverter();
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

        internal class SuptypEnumConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(SuptypEnum) || t == typeof(SuptypEnum?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "R")
                {
                    return SuptypEnum.R;
                }
                throw new Exception("Cannot unmarshal type SuptypEnum");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (SuptypEnum)untypedValue;
                if (value == SuptypEnum.R)
                {
                    serializer.Serialize(writer, "R");
                    return;
                }
                throw new Exception("Cannot marshal type SuptypEnum");
            }

            public static readonly SuptypEnumConverter Singleton = new SuptypEnumConverter();
        }
    }

}