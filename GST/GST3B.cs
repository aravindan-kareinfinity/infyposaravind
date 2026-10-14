using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.GST
{
    public class GSTR3B
    {
        public class OsupDet
        {
            public double txval { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class OsupZero
        {
            public double txval { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class OsupNilExmp
        {
            public double txval { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class IsupRev
        {
            public double txval { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class OsupNongst
        {
            public double txval { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class SupDetails
        {
            public OsupDet osup_det { get; set; }
            public OsupZero osup_zero { get; set; }
            public OsupNilExmp osup_nil_exmp { get; set; }
            public IsupRev isup_rev { get; set; }
            public OsupNongst osup_nongst { get; set; }
        }

        public class ItcAvl
        {
            public string ty { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class ItcRev
        {
            public string ty { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class ItcNet
        {
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class ItcInelg
        {
            public string ty { get; set; }
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class ItcElg
        {
            public List<ItcAvl> itc_avl { get; set; }
            public List<ItcRev> itc_rev { get; set; }
            public ItcNet itc_net { get; set; }
            public List<ItcInelg> itc_inelg { get; set; }
        }

        public class IsupDetail
        {
            public string ty { get; set; }
            public double inter { get; set; }
            public double intra { get; set; }
        }

        public class InwardSup
        {
            public List<IsupDetail> isup_details { get; set; }
        }

        public class IntrDetails
        {
            public double iamt { get; set; }
            public double camt { get; set; }
            public double samt { get; set; }
            public double csamt { get; set; }
        }

        public class LtfeeDetails
        {
        }

        public class IntrLtfee
        {
            public IntrDetails intr_details { get; set; }
            public LtfeeDetails ltfee_details { get; set; }
        }

        public class InterSup
        {
            public List<object> unreg_details { get; set; }
            public List<object> comp_details { get; set; }
            public List<object> uin_details { get; set; }
        }

        public class RootObject
        {
            public string gstin { get; set; }
            public string ret_period { get; set; }
            public SupDetails sup_details { get; set; }
            public ItcElg itc_elg { get; set; }
            public InwardSup inward_sup { get; set; }
            public IntrLtfee intr_ltfee { get; set; }
            public InterSup inter_sup { get; set; }
        }
    }
}