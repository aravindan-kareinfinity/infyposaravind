using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static InfyPOS.GST.GSTR1;

namespace InfyPOS.Models.GST
{
    public class GSTR1A
    {

        [JsonProperty("gstin")]
        public string Gstin { get; set; }

        [JsonProperty("fp")]
        public string Fp { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        //[JsonProperty("gt")]
        //public long Gt { get; set; }

        //[JsonProperty("cur_gt")]
        //public long CurGt { get; set; }

        [JsonProperty("b2b")]
        public B2B[] B2B { get; set; }

        [JsonProperty("b2cs")]
        public B2C[] B2Cs { get; set; }

        [JsonProperty("b2cl")]
        public B2cl[] B2Cl { get; set; }

        [JsonProperty("exp")]
        public Exp[] Exports { get; set; }

        [JsonProperty("cdnr")]
        public Cdnr[] Cdnr { get; set; }



        [JsonProperty("hsn")]
        public Hsn Hsn { get; set; }

        public GSTR1A()
        {
            Cdnr = new Cdnr[0];
            B2B = new B2B[0];
            B2Cs = new B2C[0];
            B2Cl = new B2cl[0];
            Exports = new Exp[0];
        }
        //[JsonProperty("doc_issue")]
        //public DocIssue DocIssue { get; set; }
    }
    public partial class B2B
    {
        [JsonProperty("ctin")]
        public string Ctin { get; set; }

        [JsonProperty("inv")]
        public Invoices[] Inv { get; set; }

        public partial class Invoices
        {
            [JsonProperty("inum")]
            public string Inum { get; set; }

            [JsonProperty("idt")]
            public string Idt { get; set; }

            [JsonProperty("val")]
            public long Val { get; set; }

            [JsonProperty("pos")]
            public string Pos { get; set; }

            [JsonProperty("rchrg")]
            public string Rchrg { get; set; }
            [JsonProperty("inv_typ")]
            public string InvTyp { get; set; }

            [JsonProperty("itms")]
            public Itm[] Itms { get; set; }

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
            public decimal Txval { get; set; }

            [JsonProperty("rt")]
            public long Rt { get; set; }

            [JsonProperty("csamt", NullValueHandling = NullValueHandling.Ignore)]
            public decimal? CSamt { get; set; }

            [JsonProperty("camt", NullValueHandling = NullValueHandling.Ignore)]
            public decimal? Camt { get; set; }

            [JsonProperty("samt", NullValueHandling = NullValueHandling.Ignore)]
            public decimal? Samt { get; set; }

            [JsonProperty("iamt", NullValueHandling = NullValueHandling.Ignore)]
            public decimal? Iamt { get; set; }
        }
    }

    public partial class B2C
    {
        [JsonProperty("sply_ty")]
        public string SplyTy { get; set; }

        [JsonProperty("rt")]
        public long Rt { get; set; }


        [JsonProperty("pos")]
        public string Pos { get; set; }

        [JsonProperty("typ")]
        public string Typ { get; set; }

        [JsonProperty("txval")]
        public decimal Txval { get; set; }

        [JsonProperty("camt")]
        public decimal Camt { get; set; }

        [JsonProperty("samt")]
        public decimal Samt { get; set; }

        [JsonProperty("csamt")]
        public decimal Csamt { get; set; }
    }


    public partial class Hsn
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }
        public partial class Datum
        {
            [JsonProperty("num")]
            public long Num { get; set; }

            [JsonProperty("hsn_sc")]
            public string HsnSc { get; set; }

            [JsonProperty("uqc")]
            public string Uqc { get; set; }

            [JsonProperty("qty")]
            public long Qty { get; set; }

            [JsonProperty("rt")]
            public decimal Rt { get; set; }

            [JsonProperty("txval")]
            public decimal Txval { get; set; }

            [JsonProperty("iamt")]
            public decimal Iamt { get; set; }

            [JsonProperty("camt")]
            public decimal Camt { get; set; }

            [JsonProperty("samt")]
            public decimal Samt { get; set; }

            [JsonProperty("csamt")]
            public decimal Csamt { get; set; }
        }
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
        public string PGst { get; set; }

        [JsonProperty("inum")]
        public string Inum { get; set; }

        [JsonProperty("idt")]
        public string Idt { get; set; }

        [JsonProperty("val")]
        public long Val { get; set; }

        [JsonProperty("itms")]
        public Itm[] Itms { get; set; }
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
    }


    public class Exp
    {
        public class Invoice
        {
            public class Itm
            {
                public double txval { get; set; }

                public double rt { get; set; }
            }

            public string flag { get; set; }

            public string inum { get; set; }

            public string idt { get; set; }

            public double val { get; set; }

            public Itm[] itms { get; set; }

        }

        public string exp_typ { get; set; }

        public Invoice[] inv { get; set; }

    }

    public class B2cl
    {
        public class Invoice
        {
            public class Item
            {
                public class ItmDet
                {
                    public double rt { get; set; }

                    [JsonProperty("csamt", NullValueHandling = NullValueHandling.Ignore)]
                    public decimal? csamt { get; set; }
                    [JsonProperty("camt", NullValueHandling = NullValueHandling.Ignore)]
                    public decimal? camt { get; set; }

                    [JsonProperty("samt", NullValueHandling = NullValueHandling.Ignore)]
                    public decimal? samt { get; set; }

                    [JsonProperty("iamt", NullValueHandling = NullValueHandling.Ignore)]
                    public decimal? iamt { get; set; }
                    public double txval { get; set; }
                }

                public int num { get; set; }

                public ItmDet itm_det { get; set; }
            }

            public string flag { get; set; }

            public string chksum { get; set; }

            public string inum { get; set; }

            public string idt { get; set; }

            public double val { get; set; }

            public string etin { get; set; }

            public double diff_percent { get; set; }

            public string inv_typ { get; set; }

            public Item[] itms { get; set; }


        }


        public string pos { get; set; }

        public Invoice[] inv { get; set; }


    }


}