using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using InfyPOS.Models;

namespace InfyPOS.Tally
{
    public class TallyXML
    {
        public class Voucher : Common.BaseEntity
        {
            public string addtionalAttribs { get; set; }
            public bool created { get; set; }
            public string companyName { get; set; }
            public string voucherkey { get; set; }
            public string vouchernumber { get; set; }
            public DateTime date { get; set; }
            public string type { get; set; }
            public string narration { get; set; }
            public List<InfyPOS.Tally.TallyXML.LedgerVal> ledgers { get; set; }
            public string error { get; set; }
            public List<string> require_groups { get; set; }
            public List<string> require_ledgers { get; set; }
            public string xml { get; set; }
            public bool isvalid { get; set; }
            public string dxml { get; set; }

            public Voucher()
            {
                ledgers = new List<LedgerVal>();
                require_groups = new List<string>();
                require_ledgers = new List<string>();
            }
        }
        public class Ledger
        {
            public decimal ob { get; set; }
            public bool isbillwise { get; set; }
            public bool istdsparty { get; set; }
            public string name { get; set; }
            public string parent { get; set; }
            public bool created { get; set; }
            public string error { get; set; }
            public string statename { get; set; }
            public string xml { get; set; }
            public string cityname { get; set; }
            public string additionalattribs { get; set; }
            public int creditdays { get; set; }
        }

        public class LedgerGroup
        {
            public string name { get; set; }
            public string parent { get; set; }
            public bool created { get; set; }

            public string xml { get; set; }
            public string error { get; set; }
        }

        public class LedgerVal
        {
            public int Index;
            public string Name;
            public decimal Value;
            public bool IsNeedBankAllocation;
            public string chequeno;
            public DateTime instrumentdate;
            public bool IsTDS;
            public bool AvoidMerge;
            public string gstno;
            public string address;
            public int creditdays { get; set; }
            public string GSTNOElement
            {
                get
                {
                    if (string.IsNullOrEmpty(gstno))
                        return "";

                    var addreslist = "";
                    if (!string.IsNullOrEmpty(address))
                    {
                        var addresslist = address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        addreslist = @"<ADDRESS.LIST TYPE=""String"">";
                        addresslist.ToList().ForEach(e =>
                        {
                            addreslist += string.Format(@"<ADDRESS>{0}</ADDRESS>", EncodeXML(e));
                        });
                        addreslist += @" </ADDRESS.LIST>";
                    }

                    addreslist += string.Format(@"<LEDGSTREGDETAILS.LIST>
                                   <GSTREGISTRATIONTYPE>Regular</GSTREGISTRATIONTYPE>
                                   <PLACEOFSUPPLY>{0}</PLACEOFSUPPLY>
                                   <GSTIN>{1}</GSTIN>
                                   </LEDGSTREGDETAILS.LIST>
                                   <LEDMAILINGDETAILS.LIST>
                                   {2}
                                   <MAILINGNAME>{3}</MAILINGNAME>
                                   {4}
                                   </LEDMAILINGDETAILS.LIST>", EncodeXML(statename), EncodeXML(gstno), addreslist, EncodeXML(Name),
                                   (string.IsNullOrEmpty(statename) ? "" : "<STATE>"+ EncodeXML(statename)+"</STATE>"));
                                            return string.Format(@"
	                                        <COUNTRYNAME>India</COUNTRYNAME>
	                                        <GSTREGISTRATIONTYPE>Regular</GSTREGISTRATIONTYPE>
	                                        <VATDEALERTYPE>Regular</VATDEALERTYPE>
	                                        <PARTYGSTIN>{0}</PARTYGSTIN>
	                                        <COUNTRYOFRESIDENCE>India</COUNTRYOFRESIDENCE>
	                                        <LEDSTATENAME>{1}</LEDSTATENAME>
                                            {2}", gstno, EncodeXML(statename), addreslist);
                }
            }
            public string ChildNodes { get; set; }
            public string xml { get; set; }
            public bool IsCredit
            {
                get
                {
                    return Value < 0;
                }
            }

            public string statename { get; set; }
            public String cityname { get; set; }
            public string agentname { get; set; }
            public string transactionmode { get; set; }
            public string addionalAttribs { get; set; }
            public class Allocation
            {
                public string Name;
                public decimal Amount;
                public bool IsReturn;
                public Allocation(string name, decimal amount)
                {
                    this.Name = name;
                    this.Amount = amount;
                }
            }
            //New Ref
            public List<Allocation> Allocations = new List<Allocation>();

            public Allocation AddAllocation(string name, decimal amount)
            {
                var allocation = new Allocation(name, this.Value < 0 ? (Math.Abs(amount) * -1) : Math.Abs(amount));
                Allocations.Add(allocation);
                return allocation;
            }

            public LedgerVal(string name, decimal value, bool isCr)
            {
                this.Value = value;
                if (isCr)
                    this.Value = this.Value * -1;
                Name = name;
            }


            internal void Merge(LedgerVal l)
            {
                Allocations.AddRange(l.Allocations);
                Value += l.Value;
            }
        }

        public TallyDataservice.TallyVoucher CreateXML(TallyDataservice.TallyVoucher vouchers, string reference)
        {
            foreach (var group in vouchers.groups)
            {
                group.xml = CreateGroupXML(vouchers.company, group.name, group.parent);
            }

            foreach (var ledger in vouchers.ledgers)
            {
                var ledgervoucher = vouchers.vouchers.Find(e => e.ledgers.Exists(f => f.Name == ledger.name));
                ledger.xml = CreateMasterXML(vouchers.company, ledger.name, ledger.parent, ledger.statename, 0, ledger.isbillwise, 0,
                    ledgervoucher.ledgers.Find(e=>e.Name == ledger.name));
            }

            foreach (var voucher in vouchers.vouchers)
            {
                voucher.companyName = vouchers.company;
                voucher.xml = CreateVoucherXML(voucher, reference);
                voucher.dxml = DeleteVoucherXML(voucher.companyName, voucher.voucherkey, voucher.type, voucher.date);
            }
            return vouchers;
        }

        public bool Transer(TallyDataservice.TallyVoucher vouchers)
        {
            string error = "";
            foreach (var group in vouchers.groups)
            {
                group.created = CreateGroup(vouchers.company, group.name, group.parent, out error);
                group.error = error;
            }
            foreach (var ledger in vouchers.ledgers)
            {
                ledger.created = CreateMaster(vouchers.company, ledger.name, ledger.parent, ledger.statename, 0, ledger.isbillwise, null, out error);
                ledger.error = error;
            }
            foreach (var voucher in vouchers.vouchers)
            {
                voucher.companyName = vouchers.company;
                voucher.created = CreateVoucher(voucher, out error);
                voucher.error = error;
            }
            return true;
        }

        public string DecimalFormat
        {
            get
            {
                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_b2brounding)
                    return "F2";
                else
                    return "F8";
            }
        }
        public string GetDecimalString(decimal decvalue)
        {
            string no = Convert.ToString(decvalue);
            if (no.IndexOf('.') >= 0)
            {
                if(!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_b2brounding)
                    return decvalue.ToString("N2");
                else
                    return decvalue.ToString("N8");
            }
            return no;
        }
        public string BillAllocationXML(string name, decimal amount, string type,int duedays)
        {
            string xml = @"<BILLALLOCATIONS.LIST>
                    <NAME>{0}</NAME>
                    <BILLTYPE>{1}</BILLTYPE>
                    <AMOUNT>{2}</AMOUNT>
                    {3}
                   </BILLALLOCATIONS.LIST>";
            return string.Format(xml, EncodeXML(name), type, GetDecimalString(amount),
                duedays>0? @"<BILLCREDITPERIOD JD=""44930"" P=""" + duedays.ToString("F0") + @"Days"">"+ duedays.ToString("F0") + " Days</BILLCREDITPERIOD>":""
                );
        }

        public string BillAllocationXML(List<LedgerVal.Allocation> allocations, string type,int duedays)
        {
            if (allocations == null || allocations.Count == 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < allocations.Count; i++)
            {
                if (allocations[i].IsReturn)
                {
                    if (type == "New Ref")
                        type = "Agst Ref";
                    sb.AppendLine(BillAllocationXML(allocations[i].Name, allocations[i].Amount*-1, type, duedays));
                }
                else
                {
                    sb.AppendLine(BillAllocationXML(allocations[i].Name, allocations[i].Amount, type, duedays));
                }
            }
            return sb.ToString();
        }

        public Common.UserClaim Context;
        public TallyXML(string url)
        {
            this.URL = url;
        }
        public string URL = "http://localhost:9000";
        public bool ShowErroMessage = true;
        public static string EncodeXML(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return "";
            if (xml.Contains('~'))
            {
                xml = xml.Split('~')[0];
            }
            return xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        private string YearFirstDate()
        {
            if (DateTime.Now.Month <= 3)
                return (DateTime.Now.Year - 1).ToString("N0") + "0401";
            else
                return (DateTime.Now.Year).ToString("N0") + "0401";
        }

        private string YearLastDate()
        {
            if (DateTime.Now.Month <= 3)
                return (DateTime.Now.Year).ToString("N0") + "0331";
            else
                return (DateTime.Now.Year + 1).ToString("N0") + "0331";
        }

        private string TDSMasterXML(int isTDS)
        {
            if (isTDS == 1)
                return @"
                    <TAXCLASSIFICATIONNAME/>
                    <TAXTYPE>Others</TAXTYPE>
                    <TDSDEDUCTEETYPE>Individual/HUF - Resident</TDSDEDUCTEETYPE>
                    <ISTDSAPPLICABLE>Yes</ISTDSAPPLICABLE>
                    <TDSEXEMPTIONRULES.LIST>
                        <NATUREOFPAYMENT>Fees for Professional Or Technical Services</NATUREOFPAYMENT>
                    </TDSEXEMPTIONRULES.LIST>";
            else if (isTDS == 2)
                return @"
                    <TAXCLASSIFICATIONNAME/>
                    <TAXTYPE>Others</TAXTYPE>
                    <TDSRATENAME>Fees for Professional Or Technical Services</TDSRATENAME>
                    <ISTDSAPPLICABLE>Yes</ISTDSAPPLICABLE>";

            return string.Format(@"<AUDITENTRIES.LIST>             
                    </AUDITENTRIES.LIST>
                      <TDSEXEMPTIONRULES.LIST>
                       <NATUREOFPAYMENT>Fees for Professional Or Technical Services</NATUREOFPAYMENT>
                      </TDSEXEMPTIONRULES.LIST>
                      <DEDUCTINSAMEVCHRULES.LIST>
                       <NATUREOFPAYMENT>Fees for Professional Or Technical Services</NATUREOFPAYMENT>
                      </DEDUCTINSAMEVCHRULES.LIST>
                      <LOWERDeduction.LIST>
                       <NATUREOFPAYMENT>Fees for Professional Or Technical Services</NATUREOFPAYMENT>
                       <LOWERDeductionDETAILS.LIST>
                        <FROMDATE>{0}</FROMDATE>
                        <TODATE>{1}</TODATE>
                        <SECTIONNO>197</SECTIONNO>
                        <TDSITRATE> 10</TDSITRATE>
                       </LOWERDeductionDETAILS.LIST>
                      </LOWERDeduction.LIST>", YearFirstDate(), YearLastDate());
        }

        public string TDSXML(string name, string parentName)
        {
            return string.Format(@"<LEDGER NAME='{0}' Action = 'Create'>
                <NAME>{0}</NAME>
                <CURRENCYNAME>₹</CURRENCYNAME>
                <PARENT>{1}</PARENT>
                <TAXCLASSIFICATIONNAME/>
                <TAXTYPE>TDS</TAXTYPE>
                <TDSRATENAME>Fees for Professional Or Technical Services</TDSRATENAME>
                </LEDGER>", EncodeXML(name), EncodeXML(name));
        }

        public string AdjustmentAllocation(DateTime billdate, string agsntref, decimal amount)
        {
            //<BILLCREDITPERIOD JD="43058" P="20-Nov-2017">20-Nov-2017</BILLCREDITPERIOD>
            return string.Format(@"<BILLALLOCATIONS.LIST>
                                   <BILLDATE>{0}</BILLDATE>
                                   <NAME>{1}</NAME>
                                   <ISADVANCE>No</ISADVANCE>
                                   <OPENINGBALANCE>{2}</OPENINGBALANCE>
                                  </BILLALLOCATIONS.LIST>", billdate.ToString("yyyyMMdd"), agsntref, amount.ToString(DecimalFormat));
        }

        public string PaymentXML(string bankname, string accountnumber, string ledgername, string transactionmode)
        {
            return string.Format(@" <PAYMENTDETAILS.LIST>
                                   <BANKNAME>{0}</BANKNAME>
                                   <ACCOUNTNUMBER>{1}</ACCOUNTNUMBER>
                                   <PAYMENTFAVOURING>{2}</PAYMENTFAVOURING>
                                   <TRANSACTIONNAME>{3}</TRANSACTIONNAME>
                                   <SETASDEFAULT>Yes</SETASDEFAULT>
                                   <DEFAULTTRANSACTIONTYPE>{4}</DEFAULTTRANSACTIONTYPE>
                                    </PAYMENTDETAILS.LIST>", EncodeXML(bankname), accountnumber, EncodeXML(ledgername), transactionmode,"");
        }

        public string TDSLedgerName = string.Empty;

        public string MasterXML(string ledgerName, string parent, string statename, decimal ob, bool isBillWise, int isTDS, LedgerVal ledger)
        {
            //<OPENINGBALANCE>{2}</OPENINGBALANCE>
            string xml = @"<LEDGER NAME='{0}' Action = 'Create'>
                <NAME>{0}</NAME>
                <PARENT>{1}</PARENT>
                <ISBILLWISEON>{3}</ISBILLWISEON>
                {4}
                {5}
                {6}
                {7}
             </LEDGER>";
            return string.Format(xml, EncodeXML(ledgerName), EncodeXML(parent), ob, isBillWise ? "Yes" : "No",
                string.IsNullOrEmpty(statename) ? "" : "<STATENAME>" + EncodeXML(statename)+ "</STATENAME>",
                ledger != null ? ledger.GSTNOElement : "", isTDS > 0 ? TDSMasterXML(isTDS) : "",
                (ledger == null || (string.IsNullOrEmpty(ledger.agentname) && string.IsNullOrEmpty(ledger.cityname))) ? "" : AgentXML(ledger.cityname, ledger.agentname),
                (ledger==null || ledger.creditdays<=0)?"": "<BILLCREDITPERIOD>"+ ledger.creditdays.ToString("F0") + " Days</BILLCREDITPERIOD>");
        }

        public string AddressXML(List<string> address)
        {
            return string.Format("<ADDRESS.LIST TYPE='String'>{0}</ADDRESS.LIST>", string.Join("", address.ConvertAll(e => "<ADDRESS>" + EncodeXML(e) + "</ADDRESS>")));
        }
        public string AgentXML(string cityname, string agentname)
        {
            return string.Format(@"<UDF:UDFAA.LIST DESC=""`UDFAA`"" ISLIST=""YES"" TYPE=""String"" INDEX=""1001"">
                                       <UDF:UDFAA DESC=""`UDFAA`"">{0}</UDF:UDFAA>
                                   </UDF:UDFAA.LIST>
                                   <UDF:AGENTNAME.LIST DESC=""`AgentName`"" ISLIST=""YES"" TYPE=""String"" INDEX=""1101"">
                                       <UDF:AGENTNAME DESC=""`AgentName`"">{1}</UDF:AGENTNAME>
                                   </UDF:AGENTNAME.LIST>", EncodeXML(cityname), EncodeXML(agentname));
        }
        public string VoucherXML(string ledgerName, decimal amount, bool addBankAllocation, List<LedgerVal.Allocation> allocations,
            int duedays,
            string bankAllocation,
            string defaultChilds,
            string reference)
        {
            string xml = @"<ALLLEDGERENTRIES.LIST>
						        <LEDGERNAME>{0}</LEDGERNAME>
						        <ISDEEMEDPOSITIVE>{1}</ISDEEMEDPOSITIVE>
						        <AMOUNT>{2}</AMOUNT>
                                {3}
                                {4}
                                {5}
                            </ALLLEDGERENTRIES.LIST>";
            string allocXml = BillAllocationXML(allocations, reference, duedays);
            return string.Format(xml, EncodeXML(ledgerName), (amount < 0 ? "Yes" : "No"), amount.ToString(DecimalFormat),
                defaultChilds , bankAllocation, allocXml);
        }

        private string BankAllocations(DateTime date,
            LedgerVal favouring,
            LedgerVal ledgers)
        {
            if (ledgers.IsNeedBankAllocation)
                return string.Format(@"<BANKALLOCATIONS.LIST>
                <DATE>{0}</DATE>
                <INSTRUMENTDATE>{6}</INSTRUMENTDATE>
                <TRANSACTIONTYPE>{4}</TRANSACTIONTYPE>
                <PAYMENTMODE>Transacted</PAYMENTMODE>
                <NAME>{3}</NAME>
                <PAYMENTFAVOURING>{1}</PAYMENTFAVOURING>
                <INSTRUMENTNUMBER>{5}</INSTRUMENTNUMBER>
                <BANKPARTYNAME>{1}</BANKPARTYNAME>
                <STATUS>No</STATUS>
                <AMOUNT>{2}</AMOUNT>
               </BANKALLOCATIONS.LIST>", 
               date.ToString("yyyyMMdd"), EncodeXML(favouring.Name),
               ledgers.Value.ToString(DecimalFormat).Replace(",", ""),
                                       Guid.NewGuid().ToString(),
                                       ledgers.transactionmode,
                                       ledgers.chequeno,
                                       ledgers.instrumentdate.ToString("yyyyMMdd"));
            //                
            return "";
        }

        private string TDSAllocations(DateTime date, LedgerVal ledgers)
        {
            if (ledgers.IsTDS)
                return string.Format(@"
       <TAXOBJECTALLOCATIONS.LIST>
            <CATEGORY>Fees for Professional Or Technical Services</CATEGORY>
            <TAXTYPE>TDS</TAXTYPE>
            <PARTYLEDGER>{0}</PARTYLEDGER>
            <REFTYPE>New Ref</REFTYPE>
            <ISOPTIONAL>No</ISOPTIONAL>
            <ISPANVALID>No</ISPANVALID>
            <ZERORATED>No</ZERORATED>
            <EXEMPTED>Yes</EXEMPTED>
            <ISSPECIALRATE>No</ISSPECIALRATE>
            <ISDEDUCTNOW>No</ISDEDUCTNOW>
            <ISPANNOTAVAILABLE>No</ISPANNOTAVAILABLE>
            <SUBCATEGORYALLOCATION.LIST>
                 <SUBCATEGORY>Income Tax</SUBCATEGORY>
                 <DUTYLEDGER>{3}</DUTYLEDGER>
                 <SUBCATZERORATED>No</SUBCATZERORATED>
                 <SUBCATEXEMPTED>No</SUBCATEXEMPTED>
                 <SUBCATISSPECIALRATE>No</SUBCATISSPECIALRATE>
                 <TAXRATE>{2}</TAXRATE>
                 <TAX>{1}</TAX>
            </SUBCATEGORYALLOCATION.LIST>
            <SUBCATEGORYALLOCATION.LIST>
                 <SUBCATEGORY>Surcharge</SUBCATEGORY>
                 <DUTYLEDGER>TDS</DUTYLEDGER>
                 <SUBCATZERORATED>No</SUBCATZERORATED>
                 <SUBCATEXEMPTED>No</SUBCATEXEMPTED>
                 <SUBCATISSPECIALRATE>No</SUBCATISSPECIALRATE>
            </SUBCATEGORYALLOCATION.LIST>
            <SUBCATEGORYALLOCATION.LIST>
                 <SUBCATEGORY>Education Cess</SUBCATEGORY>
                 <DUTYLEDGER>TDS</DUTYLEDGER>
                 <SUBCATZERORATED>No</SUBCATZERORATED>
                 <SUBCATEXEMPTED>No</SUBCATEXEMPTED>
                 <SUBCATISSPECIALRATE>No</SUBCATISSPECIALRATE>
            </SUBCATEGORYALLOCATION.LIST>
            <SUBCATEGORYALLOCATION.LIST>
                 <SUBCATEGORY>Secondary Education Cess</SUBCATEGORY>
                 <DUTYLEDGER>TDS</DUTYLEDGER>
                 <SUBCATZERORATED>No</SUBCATZERORATED>
                 <SUBCATEXEMPTED>No</SUBCATEXEMPTED>
                 <SUBCATISSPECIALRATE>No</SUBCATISSPECIALRATE>
            </SUBCATEGORYALLOCATION.LIST>
       </TAXOBJECTALLOCATIONS.LIST>", EncodeXML(ledgers.Name), 
       ledgers.Value.ToString(DecimalFormat).Replace(",", ""),
        TDSPercentage.ToString("N0"), EncodeXML(TDSLedgerName));
            return "";
        }

        private decimal TDSPercentage = 10;
        public string VoucherXML(List<LedgerVal> ledgers, DateTime date, string reference)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ledgers.Count; i++)
            {
                if (ledgers[i].IsNeedBankAllocation)
                {
                    
                    sb.AppendLine(VoucherXML(ledgers[i].Name, ledgers[i].Value, ledgers[i].IsNeedBankAllocation,
                        ledgers[i].Allocations,
                        ledgers.Max(x => x.creditdays),
                        BankAllocations(date, ledgers[0], ledgers[i]), ledgers[i].ChildNodes, reference));
                }
                else
                {
                    if (ledgers[i].IsTDS)
                        sb.AppendLine(VoucherXML(ledgers[i].Name, ledgers[i].Value, true, ledgers[i].Allocations,
                            ledgers.Max(x => x.creditdays),
                            TDSAllocations(date, ledgers[i]), ledgers[i].ChildNodes, reference));
                    else
                        sb.AppendLine(VoucherXML(ledgers[i].Name, ledgers[i].Value, ledgers[i].IsNeedBankAllocation, ledgers[i].Allocations,
                            ledgers.Max(x => x.creditdays),
                            ledgers[i].addionalAttribs, ledgers[i].ChildNodes, reference));
                }
            }
            return sb.ToString();
        }


        public string CreateVoucherXML(string companyName, string voucherkey, string vouchernumber, DateTime date,
            string type, string narration, List<LedgerVal> ledgers, string additionalAttributes, string reference)
        {
            /*<TALLYMESSAGE>
				        <VOUCHER VCHTYPE='{3}' ACTION='Delete' REMOTEID='TextilePOS' VCHKEY='{5}' DATE='{1}' TAGNAME = 'Voucher Number' TAGVALUE = '{6}'>

                        </VOUCHER>
			        </TALLYMESSAGE>*/
            string xml = @"<ENVELOPE>
	        <HEADER>
		        <VERSION>1</VERSION>
		        <TALLYREQUEST>Import</TALLYREQUEST>
		        <TYPE>Data</TYPE>
		        <ID>Vouchers</ID>
	        </HEADER>
	        <BODY>
		        <DESC>
                    <STATICVARIABLES>
                        <SVCURRENTCOMPANY>{0}</SVCURRENTCOMPANY>
                    </STATICVARIABLES>
		        </DESC>
		        <DATA>
			        <TALLYMESSAGE>
				        <VOUCHER VCHTYPE='{3}' ACTION='Create' REMOTEID='{5}'>
					        <DATE>{1}</DATE>
					        <NARRATION>{2}</NARRATION>
					        <VOUCHERTYPENAME>{3}</VOUCHERTYPENAME>
                            <VOUCHERKEY>{5}</VOUCHERKEY>
                            <VOUCHERNUMBER>{6}</VOUCHERNUMBER>
                            <REFERENCE>{6}</REFERENCE>
                            {7}
                            {4}
				        </VOUCHER>
			        </TALLYMESSAGE>
		        </DATA>
	        </BODY>
            </ENVELOPE>";
            return string.Format(xml, EncodeXML(companyName), date.ToString("yyyyMMdd"),
                EncodeXML(narration), type, VoucherXML(ledgers, date, reference), voucherkey, EncodeXML(vouchernumber), additionalAttributes);
            //<VOUCHERNUMBER>{6}</VOUCHERNUMBER>
            //REMOTEID='{5}' VCHKEY='{5}'
        }

        public string DeleteVoucherXML(string companyName, string remoteid, string type, DateTime date)
        {
            return string.Format(@"<ENVELOPE>
                                    <HEADER>
                                        <VERSION>1</VERSION>
                                        <TALLYREQUEST>Import</TALLYREQUEST>
                                        <TYPE>Data</TYPE>
                                        <ID>Vouchers</ID>
                                    </HEADER>
                                    <BODY>
                                        <DESC>
                                            <STATICVARIABLES>
                                                <SVCURRENTCOMPANY>{0}</SVCURRENTCOMPANY>
                                            </STATICVARIABLES>
                                        </DESC>
                                        <DATA>
                                            <TALLYMESSAGE>
                                                <VOUCHER REMOTEID='{1}' VCHTYPE = '{2}' DATE='{3}' ACTION='Delete'>
                                                </VOUCHER>
                                            </TALLYMESSAGE>
                                        </DATA>
                                    </BODY>
                                    </ENVELOPE>", EncodeXML(companyName), remoteid, type, date.ToString("yyyyMMdd"));
        }
        public List<LedgerVal> Merge(List<LedgerVal> ledgers)
        {
            int index = 0;
            foreach (var l in ledgers)
                l.Index = index++;

            var lst = ledgers.GroupBy(e => new { e.Name, e.IsCredit, e.AvoidMerge });
            List<LedgerVal> nLedgers = new List<LedgerVal>();
            foreach (var ldr in lst)
            {
                var sitems = ldr.ToList();
                if (ldr.Key.AvoidMerge)
                {
                    nLedgers.AddRange(sitems);
                }
                else
                if (sitems.Count > 1)
                {
                    LedgerVal l = sitems[0];
                    for (int i = 1; i < sitems.Count; i++)
                    {
                        l.Merge(sitems[i]);
                    }
                    nLedgers.Add(l);
                }
                else
                {
                    nLedgers.Add(sitems[0]);
                }
            }
            return nLedgers.OrderBy(e => e.Index).ToList();
        }

        public string CreateVoucherXML(Voucher voucher, string reference)
        {
            return CreateVoucherXML(voucher.companyName, voucher.voucherkey, voucher.vouchernumber, voucher.date,
                voucher.type, voucher.narration, voucher.ledgers, voucher.addtionalAttribs, reference);
        }

        public bool CreateVoucher(Voucher voucher, out string error)
        {
            string xml = CreateVoucherXML(voucher.companyName, voucher.voucherkey, voucher.vouchernumber, voucher.date, voucher.type, voucher.narration, voucher.ledgers, voucher.addtionalAttribs, "New Ref");
            return Transer(xml, out error);
        }

        public bool CreateVoucher(string companyName, string voucherkey, string vouchernumber, DateTime date, string type, string narration, List<LedgerVal> ledgers, string addtitionalattribs, out string error)
        {
            string xml = CreateVoucherXML(companyName, voucherkey, vouchernumber, date, type, narration, (ledgers), addtitionalattribs, "New Ref");
            return Transer(xml, out error);
        }

        public string CreateGroupXML(string companyName, string groupName, string parentname)
        {

            string xml = @"<ENVELOPE>
            <HEADER>
                <TALLYREQUEST>Import Data</TALLYREQUEST>
            </HEADER>
            <BODY>
                <IMPORTDATA>
                    <REQUESTDESC>
                        <REPORTNAME>All Masters</REPORTNAME>
                        <STATICVARIABLES>
                            <SVCURRENTCOMPANY>{0}</SVCURRENTCOMPANY>
                            <IMPORTDUPS>@@DupIgnoreCombine</IMPORTDUPS>
                        </STATICVARIABLES>
                    </REQUESTDESC>
                    <REQUESTDATA>
                        <TALLYMESSAGE>
                            <GROUP NAME='{1}' ACTION='Create'>
                                <NAME.LIST>
                                    <NAME>{1}</NAME>
                                </NAME.LIST>
                                <PARENT>{2}</PARENT>
                            </GROUP>
                        </TALLYMESSAGE>
                    </REQUESTDATA>
                </IMPORTDATA>
            </BODY>
            </ENVELOPE>";
            return string.Format(xml, EncodeXML(companyName), EncodeXML(groupName), EncodeXML(parentname));
        }

        public bool CreateGroup(string companyName, string groupName, string parentname, out string error)
        {
            var xml = CreateGroupXML(companyName, groupName, parentname);
            return Transer(string.Format(xml, EncodeXML(companyName), EncodeXML(groupName), EncodeXML(parentname)), out error);
        }

        public static string SerializeXML(object objectValue, Type type)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            XmlSerializer s = new XmlSerializer(type);
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            s.Serialize(stream, objectValue);
            stream.Position = 0;
            doc.Load(stream);
            return doc.InnerXml;
        }

        public static Object DeSerializeXML(string objectXML, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(objectXML)) return null;
                System.IO.StringReader read = new StringReader(objectXML);
                XmlSerializer s = new XmlSerializer(type);
                System.Xml.XmlReader reader = new System.Xml.XmlTextReader(read);
                return s.Deserialize(reader);
            }
            catch (Exception exp)
            {
                return null;
            }
        }

        public string CreateMasterXML(string companyName, string ledgerName, string parent, string statename,
            decimal ob, bool isBillwise, int isTDS, LedgerVal ledger)
        {
            //DUPCOMBINE
            string xml = @"<ENVELOPE>
                <HEADER>
                    <VERSION>1</VERSION>
                    <TALLYREQUEST>Import</TALLYREQUEST>
                    <TYPE>Data</TYPE><ID>All Masters</ID>
                </HEADER>
                <BODY>
                    <DESC>
                    <STATICVARIABLES>
                        <IMPORTDUPS>@@DupIgnoreCombine</IMPORTDUPS>
                        <SVCURRENTCOMPANY>{0}</SVCURRENTCOMPANY>
                    </STATICVARIABLES>
                    </DESC>
                    <DATA>
                        <TALLYMESSAGE>
                        {1}
                        </TALLYMESSAGE>
                    </DATA>
                    </BODY>
                </ENVELOPE>";
            return string.Format(xml, EncodeXML(companyName), MasterXML(ledgerName, parent, statename, ob, isBillwise, isTDS, ledger));
        }

        public bool Transer(string xml, out string error)
        {
            error = "";
            string filename = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".exe", ".log.xml");
            System.IO.File.AppendAllText(filename, xml);

            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] response = wc.UploadData(URL, "POST", System.Text.UTF8Encoding.UTF8.GetBytes(xml));

            string respstring = System.Text.UTF8Encoding.UTF8.GetString(response);

            filename = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".exe", ".logout.xml");
            System.IO.File.AppendAllText(filename, respstring);

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(respstring);

            try
            {

                string xmlenv = doc.OuterXml;
                if (!xmlenv.Trim().StartsWith("<ENVELOPE>"))
                    xmlenv = "<ENVELOPE>" + xmlenv + "</ENVELOPE>";
                Tally.ENVELOPE result = DeSerializeXML(xmlenv, typeof(Tally.ENVELOPE)) as Tally.ENVELOPE;

                if (result.Tables.Contains("DATA") && result.Tables["DATA"].Columns.Contains("LINEERROR"))
                {
                    string erromsg1 = result.Tables["DATA"].Rows[0]["LINEERROR"].ToString();
                    error = erromsg1;
                    return false;
                }
                if (!result.Tables[result.Tables.Count - 1].Columns.Contains("ERRORS")) return true;
                if (result.Tables[result.Tables.Count - 1].Rows[0]["ERRORS"].ToString() == "0")
                    return true;

                string erromsg = result.Tables[result.Tables.Count - 1].Rows[0]["LINEERROR"].ToString();
                error = erromsg;
                return false;

            }
            catch (Exception exp)
            {

                if (doc.DocumentElement.ChildNodes[doc.DocumentElement.ChildNodes.Count - 1].OuterXml == "<ERRORS>0</ERRORS>")
                {
                    return true;
                }
                else
                {
                    if (doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == null)
                    {
                        if (doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == "0" ||
                            doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == "1") return true;

                        error = doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value;
                        return false;
                    }
                }

                return doc.DocumentElement.ChildNodes[1].ChildNodes[0].ChildNodes[0].ChildNodes[0].Value == "1";
            }
        }

        public string CreateTDSMaster(string companyName, string ledgerName, string parent)
        {
            string xml = @"<ENVELOPE>
                <HEADER>
                    <VERSION>1</VERSION>
                    <TALLYREQUEST>Import</TALLYREQUEST>
                    <TYPE>Data</TYPE><ID>All Masters</ID>
                </HEADER>
                <BODY>
                    <DESC>
                    <STATICVARIABLES>
                        <IMPORTDUPS>@@DUPCOMBINE</IMPORTDUPS>
                        <SVCURRENTCOMPANY>{0}</SVCURRENTCOMPANY>
                    </STATICVARIABLES>
                    </DESC>
                    <DATA>
                        <TALLYMESSAGE>
                        {1}
                        </TALLYMESSAGE>
                    </DATA>
                    </BODY>
                </ENVELOPE>";
            return string.Format(xml, EncodeXML(companyName), TDSXML(EncodeXML(ledgerName), EncodeXML(parent)));
        }

        public bool CreateTDSMaster(string companyName, string ledgerName, string parent, out string error)
        {
            var xml = CreateTDSMaster(companyName, ledgerName, parent);
            return Transer(xml, out error);
        }

        public bool CreateMaster(string companyName, string ledgerName, string parent, string statename, decimal ob, bool isBillwise, LedgerVal ledger, out string error)
        {
            return CreateMaster(companyName, ledgerName, parent, statename, ob, isBillwise, 0, ledger, out error);
        }


        public bool CreateMaster(string companyName, string ledgerName, string parent, string statename, decimal ob, bool isBillwise, int isTDSParty, LedgerVal ledger, out string error)
        {
            if (ledgerName.Contains('~'))
            {
                parent = ledgerName.Split('~')[1];
                ledgerName = ledgerName.Split('~')[0];
            }
            string xml = CreateMasterXML(companyName, ledgerName, parent, statename, ob, isBillwise, isTDSParty, ledger);
            return Transer(xml, out error);
        }
    }
}

//<ISBILLWISEON>Yes</ISBILLWISEON>
