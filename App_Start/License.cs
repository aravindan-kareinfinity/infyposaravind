using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
public class LicenseManager
{
    private long organizationid;
    public LicenseManager(long organizationid)
    {
        this.organizationid = organizationid;
        BuildPermissionsets(organizationid);
        if (IsSAAS && organizationid > 0)
        {
            prepareSAASLicense();
        }
        else
        {
            prepareLicense();
        }
        if (license != null && !license.manualpos &&
            Permissions != null && Permissions.SpecialPermissions != null)
        {
            Permissions.SpecialPermissions.RemoveAll(e => e.code == "MANUALPOSBILL");
        }
    }


    private static System.Collections.Concurrent.ConcurrentDictionary<long, LicenseManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, LicenseManager>();
    public static LicenseManager Instance(long organizationid)
    {
        LicenseManager instance = null;
        if (instanceList.TryGetValue(organizationid, out instance))
            return instance;

        instance = new LicenseManager(organizationid);
        if (!instanceList.TryAdd(organizationid, instance))
            instanceList.TryGetValue(organizationid, out instance);
        return instance;
    }

    public static LicenseManager Instance(string organizationcode)
    {
        var organizationid = InfyPOS.Data.OrganisationDataService.GetOrganizationID(organizationcode);
        if (organizationid > 0)
            return Instance(organizationid);
        return null;
    }

    static bool? issaas;
    public static bool IsSAAS
    {
        get
        {
            if (issaas.HasValue) return issaas.Value;
            issaas = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SAASMODE"]) &&
                (System.Configuration.ConfigurationManager.AppSettings["SAASMODE"].ToLower() == "true" ||
                System.Configuration.ConfigurationManager.AppSettings["SAASMODE"].ToLower() == "yes");
            return issaas.Value;
        }
    }


    public PermissionConfigList BuildPermissionsets(long organizationid)
    {
        Permissions = new PermissionConfigList();
        var plist = permissionlist.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        Permissions.Permissions = new List<PermissionConfiguration>();
        foreach (var l in plist)
        {
            Permissions.Permissions.Add(new PermissionConfiguration(l));
        }

        plist = mobileapppermisisonlist.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var l in plist)
        {
            Permissions.Permissions.Add(new PermissionConfiguration(l));
        }
        int i = 0;
        while(InfyPOS.Services.ConfigManager.Instance(organizationid).Config == null && i++<5)
            System.Threading.Thread.Sleep(1000);

        if (IsSAAS || InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_ismobileapplication)
        {
            plist = mobilepermisisonlist.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in plist)
            {
                Permissions.Permissions.Add(new PermissionConfiguration(l));
                //if (l.IndexOf(':') > 0)
                //    Permissions.Permissions.Add(new PermissionConfiguration()
                //    {
                //        code = l.Split(':')[0],
                //        name = l.Split(':')[1]
                //    });
                //else
                //{
                //    Permissions.Permissions.Add(new PermissionConfiguration()
                //    {
                //        name = l,
                //        code = l
                //    });
                //}
            }
        }
        plist = specialpermisisonlist.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        Permissions.SpecialPermissions = new List<PermissionConfiguration>();
        foreach (var l in plist)
        {
            Permissions.SpecialPermissions.Add(new PermissionConfiguration(l));
            //if (l.IndexOf(':') > 0)
            //    Permissions.SpecialPermissions.Add(new PermissionConfiguration()
            //    {
            //        code = l.Split(':')[0],
            //        name = l.Split(':')[1]
            //    });
            //else
            //{
            //    Permissions.SpecialPermissions.Add(new PermissionConfiguration()
            //    {
            //        name = l,
            //        code = l
            //    });
            //}
        }
        
        plist = datapermisisonlist.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        Permissions.DataPermissions = new List<PermissionConfiguration>();
        foreach (var l in plist)
        {
            Permissions.DataPermissions.Add(new PermissionConfiguration(l));
            //if (l.IndexOf(':') > 0)
            //    Permissions.DataPermissions.Add(new PermissionConfiguration()
            //    {
            //        code = l.Split(':')[0],
            //        name = l.Split(':')[1]
            //    });
            //else
            //{
            //    Permissions.DataPermissions.Add(new PermissionConfiguration()
            //    {
            //        name = l,
            //        code = l
            //    });
            //}
        }
        return Permissions;
    }



    public PermissionConfigList Permissions { get; set; }

    public static License DirectLicense()
    {
        InfyPOS.Data.CompanyDataService CompanyData = new InfyPOS.Data.CompanyDataService();
        var result = CompanyData.Search(new InfyPOS.Models.CompanyCreteria());

        if (result.Count == 0)
            throw new Exception("No company available in the database");
        //comment here for development
        var response = License.Create(result.OrderBy(e => e.id).First().name);
        LicenseManager.Instance(0).Permissions = LicenseManager.Instance(0).BuildPermissionsets(0);
        return response;
    }

    private void prepareLicense()
    {
        try
        {
            string name = null;
            if (!IsSAAS)
            {
                InfyPOS.Data.CompanyDataService CompanyData = new InfyPOS.Data.CompanyDataService();
                CompanyData.Context = new InfyPOS.Common.UserClaim()
                {
                    OrganizationID = organizationid
                };
                using (var reader = CompanyData.ExecuteReader("Select name from company where isactive=true order by id limit 1", new SortedList<string, object>()))
                {
                    if (reader.Read())
                        name = reader.GetString(0);
                }
            }
            else
            {
                name = "QUANTO TEXTILE";
            }
            //comment here for development
            license = !string.IsNullOrEmpty(name) ? License.Create(name) : new License();
            if (!license.valid)
            {
                if (name == "QUANTO TEXTILE")
                {
                    license = new License()
                    {
                        valid = true,
                        advanceanalytical = true,
                        basicanalytical = true,
                        cashflow = true,
                        company = "QUANTO TEXTILE",
                        delivery = true,
                        finance = true,
                        mobile = true,
                        intray = true,
                        key = License.Key,
                        lrinbox = true,
                        noofemployees = 100,
                        payroll = true,
                        jobwork = true,
                        imagegallery = true,
                        promotions = true,
                        tallyintegration = true,
                        purchaseorder = true,
                        stocksync = true,
                        manufacture = true,
                        restrictedcode = new List<string>()
                    };
                }
                else
                {
                    license.key = LicenseManager.License.Key;
                }
            }

        }
        catch (Exception exp)
        {
            license = new License();
            license.key = LicenseManager.License.Key;
        }
        if (!license.valid)
        {
            Permissions.SpecialPermissions.Clear();
            Permissions.Permissions.Clear();
        }
        else
        {
            Permissions.SpecialPermissions.RemoveAll(e => license.restrictedcode.Contains(e.code));
            Permissions.Permissions.RemoveAll(e => license.restrictedcode.Contains(e.code));
        }
    }

    private void prepareSAASLicense()
    {
        string name = null;
        try
        {
            InfyPOS.Data.CompanyDataService CompanyData = new InfyPOS.Data.CompanyDataService();
            CompanyData.Context = new InfyPOS.Common.UserClaim()
            {
                OrganizationID = organizationid
            };
            using (var reader = CompanyData.ExecuteReader("Select name from company where isactive=true order by id limit 1", new SortedList<string, object>()))
            {
                if (reader.Read())
                    name = reader.GetString(0);
            }
            //comment here for development

            InfyPOS.Data.OrganisationDataService organisationDataService = new InfyPOS.Data.OrganisationDataService();
            var organization = organisationDataService.Search(new InfyPOS.Models.OrganisationCreteria()
            {
                id = organizationid
            }).First();

            if (organization.modules == null) organization.modules = new InfyPOS.Models.Organisation.Modules();

            license = new License()
            {
                valid = true,
                advanceanalytical = organization.modules.advanceanalytical,
                manualpos = organization.modules.manualpos,
                basicanalytical = organization.modules.basicanalytical,
                company = name,
                finance = organization.modules.finance,
                intray = organization.modules.intray,
                key = License.Key,
                noofemployees = organization.modules.noofemployees,
                payroll = organization.modules.payroll,
                jobwork = organization.modules.jobwork,
                promotions = organization.modules.promotions,
                tallyintegration = organization.modules.tallyintegration,
                purchaseorder = organization.modules.purchaseorder,
                stocksync = organization.modules.stocksync,
                saas = true,
                mobile = organization.modules.mobile,
                itemimage = organization.modules.itemimage,
                fmcgpos = organization.modules.fmcgpos,
                crm = organization.modules.crm,
                imagegallery = organization.modules.imagegallery,
                cancreatelocation = organization.modules.cancreatelocation
            };
        }
        catch (Exception exp)
        {
            license = new License();
            license.key = LicenseManager.License.Key;
        }

        license.company = name;
        if (Permissions == null) Permissions = new PermissionConfigList();
        if (Permissions.SpecialPermissions == null) Permissions.SpecialPermissions = new List<PermissionConfiguration>();
        if (Permissions.Permissions == null) Permissions.Permissions = new List<PermissionConfiguration>();
        if (license.restrictedcode == null) license.restrictedcode = new List<string>();

        Permissions.SpecialPermissions.RemoveAll(e => license.restrictedcode.Contains(e.code));
        Permissions.Permissions.RemoveAll(e => license.restrictedcode.Contains(e.code));

    }

    License license = null;
    public License GetLicense()
    {
        if (license != null) return license;
        if (IsSAAS && organizationid > 0)
        {
            prepareSAASLicense();
        }
        else
        {
            prepareLicense();
        }
        license.IsSAAS = IsSAAS;
        return license;
    }

    public class PermissionConfigList
    {
        public List<PermissionConfiguration> Permissions { get; set; }
        public List<PermissionConfiguration> SpecialPermissions { get; set; }
        public List<PermissionConfiguration> DataPermissions { get; set; }


        public bool Exist(string code)
        {
            //Permissions.Count == 0 ||
            return Permissions.Exists(e => e.code == code) || SpecialPermissions.Exists(e => e.code == code) || DataPermissions.Exists(e => e.code == code);
        }
    }
    public class PermissionConfiguration
    {
        static System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;

        public PermissionConfiguration()
        {

        }
        public PermissionConfiguration(string source)
        {
            string[] parts = source.Split(':');
            if (parts.Length == 3)
            {
                parent = parts[0];
                code = parts[1];
                name = textInfo.ToTitleCase(parts[2].ToLower());
            }
            else if (parts.Length == 2)
            {
                code = parts[0];
                name = parts[1];
            }
            else
            {
                code = source;
                name = source;
            }
        }
        public string parent { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public bool group
        {
            get
            {
                return string.IsNullOrEmpty(name);
            }
        }
    }

    private static string mobileapppermisisonlist = @"
MOBILE APP:IOSACCESS:IOS ACCESS
MOBILE APP:MOBILEDASHBOARD:DASHBOARD
MOBILE APP:MOBILEEXPRESSCOUNTER:EXPRESS COUNTER
MOBILE APP:MOBILESTOCKCHECK:STOCK CHECK
MOBILE APP:MOBILESTOCKOUTWARD:STOCK OUTWARD
MOBILE APP:MOBILESTOCKINWARD:STOCK INWARD
MOBILE APP:MOBILECUSTOMERFEEDBACK:CUSTOMER FEEDBACK
MOBILE APP:MOBILESTOCKSYNC:STOCK SYNC
MOBILE APP:MOBILESALESORDER:SALES ORDER
MOBILE APP:MOBILEINBOX:INBOX
MOBILE APP:MOBILECUSTOMERWALKIN:CUSTOMER WALK IN
MOBILE APP:MOBILECUSTOMERTOKEN:CUSTOMER TOKEN
MOBILE APP:MOBILESTOCKAUDIT:MOBILE STOCK AUDIT
MOBILE APP:MOBILECUSTOMERBILL:MOBILE CUSTOMER BILL
MOBILE APP:MOBILEITEMLOCATOR:MOBILE ITEM LOCATOR
MOBILE APP:MOBILEBARCODEIMAGE:BARCODE IMAGE
MOBILE APP:MOBILEMETERBARCODE:METER BARCODE
MOBILE APP:MOBILEKIOSK:KIOSK
MOBILE APP:MOBILEPRODUCTINSIGHT:MOBILE PRODUCT INSIGHT
MOBILE APP:MOBILESUPPLIERINSIGHT:MOBILE SUPPLIER INSIGHT
MOBILE APP:MOBILEPURCHASEORDER:MOBILE PURCHASE ORDER
MOBILE APP:MOBILESTORE:MOBILE STORE 360";

    private static string mobilepermisisonlist = @"
MOBILE:DEATHONARRIVAL:DEATH ON ARRIVAL
MOBILE:PRICEDROP:PRICE DROP
MOBILE:BUYBACK:BUY BACK
MOBILE:BUYBACKSCHEME:BUY BACK SCHEME
MOBILE:EMISCHEME:EMI SCHEME
MOBILE:RECYCLE:RECYCLE
MOBILE:RECHARGE:RECHARGE
MOBILE:RECHARGESCHEME:RECHARGE SCHEME
MOBILE:SERVICECENTERMASTER:SERVICECENTER MASTER
MOBILE REPORT:MOBILEDAYBOOK:MOBILE DAYBOOK
MOBILE REPORT:MOBILESWIPEMACHINEREPORT:SWIPE MACHINE REPORT
MOBILE REPORT:MOBILEEMIREPORT:MOBILE EMI REPORTS
MOBILE REPORT:MOBILERECHARGEREPORT:MOBILE RECHARGE REPORT
MOBILE REPORT:MOBILEBILLREPORT:MOBILE BILL REPORT
MOBILE REPORT:MOBILESCHEMEREPORT:MOBILE SCHEME REPORT";

    private static string datapermisisonlist = @"
DATA PERMISSION:HIDECUSTOMERCAT:HIDE CUSTOMER CATEGORY
DATA PERMISSION:RESTRICTEDCUSTOMER:RESTRICTED CUSTOMER
DATA PERMISSION:WAREHOUSELOCATION:HIDE WAREHOUSE DATA
DATA PERMISSION:SHOWVALUE:HIDE COST PRICE
DATA PERMISSION:SHOWSELLINGVALUE:HIDE SELLING PRICE
DATA PERMISSION:HIDESUPPLIER:HIDE SUPPLIER
DATA PERMISSION:HIDEDBSTOCK:HIDE DASHBOARD STOCK
DATA PERMISSION:SHOWROOMPURCHASE:SHOWROOM PURCHASE ONLY
DATA PERMISSION:HIDETOTALCOST:HIDE TOTAL COST
DATA PERMISSION:BILLLOG:BILL LOG
DATA PERMISSION:ADVFINANCE:ADVANCE FINANCE
DATA PERMISSION:APIACCESS:EXTERNAL API ACCESS
DATA PERMISSION:HIDECASHCOLLECTION:HIDE CASH COLLECTION
DATA PERMISSION:SALEBARCODEPRICE:ALWAYS BARCODE PRICE ON POS
DATA PERMISSION:SECURITY_TIME:TIME RESTRICTION
DATA PERMISSION:SECURITY_IP:IP RESTRICTION
DATA PERMISSION:SECURITY_LOCATION:LOCATION RESTRICTION
DATA PERMISSION:SECURITY_BIOMETRIC:BIOMETRIC RESTRICTION";

    //MANUALPOSBILL:ENABLE MANUAL POS BILL
    private static string specialpermisisonlist = @"SPECIAL PERMISSION:DISCOUNTONBILL:ENABLE DISCOUNT ON BILLING
SPECIAL PERMISSION:BILLEDIT:ENABLE BILL EDIT
SPECIAL PERMISSION:SUPPORTCREDITBILL:ENABLE CREDIT ON BILLING
SPECIAL PERMISSION:SUPPORTCREDITSETTLEMENT:ENABLE CREDIT ON SETTLEMENT
SPECIAL PERMISSION:SUPPORTCREDITNOTESETTLEMENT:ENABLE CREDIT NOTE ON SETTLEMENT
SPECIAL PERMISSION:DISCOUNTONSETTLEMENT:ENABLE DISCOUNT ON CASHIER
SPECIAL PERMISSION:PRICEEDITONSO:ENABLE PRICE EDIT ON SO
SPECIAL PERMISSION:STATUSCHANGESO:ENABLE SO STATUS CHANGE
SPECIAL PERMISSION:PRICEEDITONB2BPOS:ENABLE PRICE EDIT ON B2B POS
SPECIAL PERMISSION:SUPPORTPOSREFUND:ENABLE REFUND ON POS
SPECIAL PERMISSION:COREWIP:ENABLE CORE WIP BOOKING
SPECIAL PERMISSION:NONCOREWIP:ENABLE NON CORE WIP BOOKING
SPECIAL PERMISSION:OUTOFSTOCK:MANAGE FOOD OUT OF STOCK
SPECIAL PERMISSION:CANPOSTDAYCLOSE:CAN POST DAY CLOSE TO FINANCE
SPECIAL PERMISSION:CANAPPROVEDISCOUNT:CAN APPROVE DISCOUNT
SPECIAL PERMISSION:CANDISCOUNTONDISCOUNT:CAN DISCOUNT ON DISCOUNT
SPECIAL PERMISSION:CANAPPROVECREDIT:CAN APPROVE CREDIT
SPECIAL PERMISSION:CANAPPROVEPURCHASE:CAN APPROVE PURCHASE
SPECIAL PERMISSION:SPECIALSUPPLIERPAYMENT:CAN APPROVE SUPPLIER PAYMENT
SPECIAL PERMISSION:CANCHANGEPRICEONBARCODE:CAN CHANGE PRICE ON BARCODE
SPECIAL PERMISSION:CANCHANGEPRODATTRIBUTE:CAN CHANGE PRODUCT ATTRIBUTE
SPECIAL PERMISSION:CANCHANGEDATE:CAN CHANGE DATE
SPECIAL PERMISSION:DASHBOARDSALES:ENABLE DASHBOARD
SPECIAL PERMISSION:WIDGETS:ENABLE USER DASHBOARD
SPECIAL PERMISSION:CANLOCKHR:CAN LOCK DATE - HR
SPECIAL PERMISSION:BARCODEONCEPRINT:RESET ONETIME BARCODE
SPECIAL PERMISSION:SECURITY_GLOBALUSER:GLOBAL USER
SPECIAL PERMISSION:TRACE:TRACE
SPECIAL PERMISSION:SYSTEMCOMMANDS:SYSTEM COMMANDS";

    private static string permissionlist = @"WAREHOUSE:INBOX:WAREHOUSE DASHBOARD
WAREHOUSE:COURIER:COURIER
WAREHOUSE:LRENTRY:TRANSPORT ENTRY
WAREHOUSE:LRINVOICE:INVOICE
WAREHOUSE:LRPRICER:INVENTORY PRICING
WAREHOUSE:POGRN:PO BASED GRN
WAREHOUSE:LRPRODUCT:INVENTORY ENTRY
WAREHOUSE:DIRECTPURCHASE:DIRECT PURCHASE ENTRY
WAREHOUSE:BARCODEGEN:BARCODE GENERATION
WAREHOUSE:LRISSUE:TRANSPORT ISSUE ENTRY
WAREHOUSE:LRRECEIPT:TRANSPORT RECEIPT ENTRY
WAREHOUSE:OUTWARD:STOCK OUTWARD
WAREHOUSE:RECEIVEGOODS:RECEIVE GOODS
WAREHOUSE:STOCKADJUSTMENT:PHYSICAL STOCK AUDIT
WAREHOUSE:PORETURN:PURCHASE RETURN
WAREHOUSE:SEARCHITEM:STOCK ITEM LOCATOR
WAREHOUSE:JOBWORKSAMPLE:JOB WORK SAMPLE
WAREHOUSE:SALEORDER:SALE ORDER
WAREHOUSE:SOINBOX:SALE ORDER INBOX
WAREHOUSE:POINBOX:PURCHASE ORDER INBOX
WAREHOUSE:STOINBOX:STOCK ORDER INBOX
WAREHOUSE:RPTWAREHOUSE:REPORT WAREHOUSE
WAREHOUSE:INTRAY:INTRAY

MANUFACTURING:MFGDASHBOARD:DASHBOARD
MANUFACTURING:GATEPASS:GATE PASS
MANUFACTURING:SKUMASTER:SKU MASTER
MANUFACTURING:BOMMASTER:BOM MASTER
MANUFACTURING:BINSTORAGE:BIN STORAGE
MANUFACTURING:SWATCH:SWATCHES / FOLDERS
MANUFACTURING:CATALOGBOOKING:CATALOG BOOKING
MANUFACTURING:SAMPLEINBOX:SAMPLE - INBOX
MANUFACTURING:SAMPLERORDER:SAMPLE ORDER
MANUFACTURING:WOORCHESTRATION:ORCHESTRATION
MANUFACTURING:BOMINVOICE:COMPONENT PURCHASE
MANUFACTURING:BOMINVOICERETURN:COMPONENT PURCHASE RETURN
MANUFACTURING:BOMREQUEST:BOM REQUEST
MANUFACTURING:WORKORDER:WORK ORDER
MANUFACTURING:JOBORDER:JOB ORDER
MANUFACTURING:WOINBOX:WORK ORDER - INBOX
MANUFACTURING:WOGRN:WORK ORDER - GRN
MANUFACTURING:PACKSLIP:PACK SLIP
MANUFACTURING:BINMANAGER:BIN - MANAGER
MANUFACTURING:MFGRREFERENCEMASTER:CONFIGURATION
MANUFACTURING:UNIQUESEQUENCE:UNIQUE SEQUENCE
MANUFACTURING:COMPONENTINWARD:BOM INWARD
MANUFACTURING:COMPONENTOUTWARD:BOM OUTWARD
MANUFACTURING:COMPONENTINBOX:BOM INBOX
MANUFACTURING:COMPONENTPO:COMPONENT PURCHASE ORDER
MANUFACTURING:BOMCONSUMPTION:BOM CONSUMPTION
MANUFACTURING:MFGANALYSER:ANALYSER
MANUFACTURING:DESIGNGALLERY:DESIGN GALLERY
MANUFACTURING:RPTMFG:REPORTS
MANUFACTURING:TAILORING:TAILORING

CUSTOMER:CUSTOMERMASTER:CUSTOMER MASTER
CUSTOMER:CUSTOMERORDERS:CUSTOMER ORDERS
CUSTOMER:BILLPRINT:BILL PRINT
CUSTOMER:CUSTOMERLOYALTYCONFIG:CUSTOMER LOYALTY CONFIGURATION
CUSTOMER:CUSTOMERLOYALTYMGMT:CUSTOMER LOYALTY MGMT
CUSTOMER:GIFTVOUCHER:GIFT VOUCHER
CUSTOMER:COUPONS:COUPONS
CUSTOMER:CUSTOMERREPORTS:CUSTOMER RREPORTS
CUSTOMER:CUSTOMERANALYSER:CRM ANALYSER
CUSTOMER:PRICELIST:PRICE LIST
CUSTOMER:LEADMANAGEMENT:LEAD MANAGEMENT
CUSTOMER:CRMCONFIGURATION:CRM CONFIGURATION

SALES:SALES:SALES
SALES:BILLITEM:BILL ITEM
SALES:SALERETURN:SALE RETURN
SALES:POSINVOICE:POS SALES INVOICE
SALES:POSINVOICERETURN:POS SALES INVOICE RETURN
SALES:TOUCHSALES:TOUCH SALES
SALES:B2BSALES:B2B SALES
SALES:B2BSALESRETURN:B2B SALES RETURN
SALES:B2BINBOX:B2B INBOX
SALES:ECOMINBOX:ECOM INBOX
SALES:SETTLEMENT:SETTLEMENT
SALES:DELIVERY:DELIVERY
SALES:DELIVERYRACK:DELIVERY RACK
SALES:ALTERATION:ALTERATION
SALES:GIFTDELIVERY:GIFT DELIVERY
SALES:GIFTDELIVERYISSUE:GIFT DELIVERY ISSUE
SALES:GIFTCOUPONISSUE:GIFT COUPON ISSUE
SALES:BULKSALES:SALE ON APPROVAL
SALES:ESTIMATESALE:SALE ON ESTIMATE
SALES:SEARCHITEM:STOCK ITEM LOCATOR
SALES:CASHOPENING:CASH OPENING
SALES:CASHCLOSING:CASH CLOSING
SALES:EMPLOYEESTICKER:EMPLOYEE STICKER
SALES:SALEPROMOTION:PROMOTION ACTIVATOR
SALES:RPTRETAIL:REPORT RETAIL

FINANCE:SUPPLIERPAYMENT:SUPPLIER PAYMENT
FINANCE:TRANSPORTPAYMENT:TRANSPORT PAYMENT
FINANCE:AGENTPAYMENT:AGENT PAYMENT
FINANCE:CUSTOMERPAYMENT:CREDIT SALE COLLECTION
FINANCE:CUSTOMERCREDITNOTE:CUSTOMER CREDIT NOTE
FINANCE:MANAGELEDGER:MANAGE LEDGER
FINANCE:LEDGEROB:LEDGER OPENING BALANCE
FINANCE:VOUCHERENTRY:VOUCHER ENTRY
FINANCE:FINANALYSER:ANALYSER
FINANCE:FINTDS:TDS REPORT
FINANCE:FINGST:GST REPORT
FINANCE:LEDGERPAYABLE:PAYABLE
FINANCE:LEDGERRECEIVABLE:RECEIVABLE
FINANCE:DAYBOOK:DAY BOOK
FINANCE:CHEQUEBOOK:CHEQUE BOOK
FINANCE:BRS:BRS
FINANCE:PETTYCASH:PETTY CASH
FINANCE:DAILYPANDL:DAILY P&L
FINANCE:TRAILBALANCE:TRAIL BALANCE
FINANCE:TRADINGACCOUNT:TRADING ACCOUNT
FINANCE:PROFITANDLOSS:PROFIT & LOSS
FINANCE:BALANCESHEET:BALANCE SHEET
FINANCE:LOANRECONCILIATION:LOAN RECONCILIATION
FINANCE:INCENTIVEREPORT:INCENTIVE REPORT
FINANCE:TALLYCONFIG:TALLY CONFIG
FINANCE:TALLYINTEGRATION:TALLY INTEGRATION
FINANCE:GSTINBOX:GST INBOX
FINANCE:GSTOTPREQUEST:GST OTP REQUEST
FINANCE:GSTUPDATE:GST UPLOAD

STORE:INWARD:STOCK INWARD
STORE:TRANSFERGOODS:TRANSFER GOODS
STORE:SPLITBARCODE:SPLIT BARCODE
STORE:GIFTPACK:GIFT PACK CREATION
STORE:LASTBIT:LAST BIT CONVERTION
STORE:PRICECHANGER:PRICE CHANGER
STORE:FESTIVALDISCOUNT:FESTIVAL DISCOUNT
STORE:STOCKMARKER:STOCK MARKER
STORE:STOCKENTRY:DIRECT STOCK ENTRY
STORE:STOCKADJUSTMENT:PHYSICAL STOCK AUDIT
STORE:SALEPROMOTION:SALE PROMOTION SCHEME
STORE:POREQUESTFORM:PO REQUEST FORM
STORE:POREQUESTQUEUE:PO REQUEST QUEUE
STORE:PURCHASEORDER:PURCHASE ORDER
STORE:STORETRANSACTION:STORE TRANSACTION
STORE:STOREJOBWORK:JOB WORK ORDER
STORE:STOREJOBWORKGRN:JOB WORK GRN
STORE:STORETAILORINGORDER:TAILORING ORDER
STORE:STORETAILORINGINWARD:TAILORING INWARD
STORE:STORETAILORINGOUTWARD:TAILORING OUTWARD
STORE:STORETAILORINGDELIVERY:TAILORING DELIVERY
STORE:DAILYPRICE:DAILY PRICE
STORE:DUMPING:DUMP MANAGEMENT

HR:EMPLOYEE:EMPLOYEE
HR:SALARYSTRUCTURE:SALARY STRUCTURE
HR:SALARYMGMT:SALARY MANAGEMENT
HR:TARGETMGMT:TARGET MANAGEMENT
HR:INCREMENTMGMT:INCREMENT MANAGEMENT
HR:EMPLOYEEINCHARGE:EMPLOYEE INCHARGE MANAGEMENT
HR:SALARYDEDUCTION:SALARY DEDUCTION
HR:PAYROLL:PAYROLL PROCESSING
HR:ADDITIONALPAYROLL:ADDITIONAL PAYROLL PROCESSING
HR:BOUNSPAYROLL:BOUNS PROCESSING
HR:EMPADVANCE:EMPLOYEE ADVANCE
HR:HOLIDAYMASTER:HOLIDAY MASTER
HR:PAYSLIP:PAY SLIP
HR:SHIFTALLOCATION:SHIFT ALLOCATION
HR:EMPLOYEELEAVE:EMPLOYEE LEAVE
HR:EMPLOYEEPERMISSION:EMPLOYEE PERMISSION
HR:EMPLOYEEINOUT:EMPLOYEE IN OUT
HR:FORMGENERATION:FORM GENERATION
HR:RPTPAYROLL:REPORT PAYROLL
HR:RPTHR:HR REPORT
HR:RPTHRDASHBOARD:HR DASHBOARD
HR:FINEREPORT:FINE CALCULATION
HR:HRREFERENCEMASTER:CONFIGURATION

ANALYTICAL:STOCKANALYSER:360* Stock Analyzer
ANALYTICAL:PURCHASEANALYSER:360* Purchase Analyzer
ANALYTICAL:PRANALYSER:360* Purchase Return Analyzer
ANALYTICAL:SALESANALYSER:360* Sales Analyzer
ANALYTICAL:SALESCOMPARER:Sales Comparer
ANALYTICAL:SALESVSPURCHASE:Sales Vs Purchase
ANALYTICAL:SALESVSSTOCK:Sales Vs Stock
ANALYTICAL:SMANALYSER:Stock Mark Analyser
ANALYTICAL:REORDER:Reorder Status
ANALYTICAL:REPORTBUILDER:Report Builder
ANALYTICAL:PURCHASEPLANNER:Purchase Planner
ANALYTICAL:POBUILDER:PO Builder
ANALYTICAL:IMAGEGALLERY:Image Viewer
ANALYTICAL:PRODUCTDESIGN:Product Design
ANALYTICAL:PRODUCTGALLERY:Product Gallery
ANALYTICAL:SALESPLANNER:Sales Planner
ANALYTICAL:STRESSANALYSER:Stock Stress Analyser
ANALYTICAL:REVENUEGENERATION:Revenue Generation
ANALYTICAL:RPTPRODUCTMOVEMEN:Product Movement
ANALYTICAL:PURCHASEBUDGET:Purchase Budget

MASTER:REFERENCEMASTER:REFERENCE MASTER
MASTER:PRODUCTMASTER:PRODUCT MASTER
MASTER:PRODUCTGROUP:PRODUCT GROUP
MASTER:SUPPLIERPRODUCTGROUPS:SUPPLIER PRODUCT GROUPS
MASTER:ATTRIBUTEFILTER:ATTRIBUTE FILTER
MASTER:PRDREFERENCEMASTER:PRODUCT ATTRIBUTES
MASTER:BRANDMASTER:BRAND MASTER
MASTER:ITEMMASTER:ITEM MASTER
MASTER:SUPPLIERMASTER:SUPPLIER MASTER
MASTER:AGENTMASTER:AGENT MASTER
MASTER:TRANSPORTMASTER:TRANSPORT MASTER
MASTER:TAILORMASTER:TAILOR MASTER
MASTER:TAXMASTER:TAX MASTER
MASTER:REFILLBOX:REFILL BOX

SETTING:RPTCONFIGURATION:REPORT CONFIGURATION
SETTING:COMPANYMASTER:COMPANY MASTER
SETTING:ORGMASTER:ORGANISATION MASTER
SETTING:USERMASTER:USER MANAGEMENT
SETTING:INCENTIVESTATUS:INCENTIVE CONFIGURATION
SETTING:NUMBERCONFIG:NUMBER CONFIGURATION
SETTING:APPLICATIONSETTING:APPLICATION SETTINGS
SETTING:SCHEDULAR:JOB SCHEDULAR
SETTING:DATAMIGRATION:DATAMIGRATION
SETTING:PRICINGMASTER:MARGIN CONTROLLER
SETTING:REORDER:REORDER CONFIGURATION
SETTING:SYSTEMMAPPER:SYSTEMMAPPER
SETTING:LOGTRACE:LOG TRACE

FEEDBACK:FBCONFIG:FEEDBACK CONFIG
FEEDBACK:FBREPORT:FEEDBACK REPORT
FEEDBACK:FBDASHBOARD:FEEDBACK DASHBOARD
FEEDBACK:FBQUESTION:FEEDBACK QUESTION
FEEDBACK:CUSTOMERDIVERT:CUSTOMER DIVERT

WAREHOUSE REPORT:AGENTMASTER:AGENTS
WAREHOUSE REPORT:CANCELLEDPRF:CANCELLED PRF
WAREHOUSE REPORT:INWARDPENDINGREPORT:INWARD PENDING
WAREHOUSE REPORT:LRINVOICEREPORT:INVOICE REPORT
WAREHOUSE REPORT:LRREPORT:TRANSPORT REPORT
WAREHOUSE REPORT:OUTWARDPENDINGREPORT:OUTWARD PENDING REPORT
WAREHOUSE REPORT:OUTWARDREPORT:OUTWARD REPORT
WAREHOUSE REPORT:JOBOUTWARDREPORT:JOB OUTWARD REPORT
WAREHOUSE REPORT:JOBINWARDREPORT:JOB INWARD REPORT
WAREHOUSE REPORT:PENDINGPRF:PENDING PRF
WAREHOUSE REPORT:PRODUCTMASTER:PRODUCTS
WAREHOUSE REPORT:PURCHASEHSNREPORT:PURCHASE HSN REPORT
WAREHOUSE REPORT:PURCHASETAXREPORT:PURCHASE TAX REPORT
WAREHOUSE REPORT:PURCHASEORDERREPORT:PURCHASE ORDER REPORT
WAREHOUSE REPORT:PURCHASEREPORT:PURCHASE REPORT
WAREHOUSE REPORT:PURCHASERETURNAUDITREPORT:PURCHASE AUDIT
WAREHOUSE REPORT:PURCHASERETURNCANCELREPORT:PURCHASE RETURN CANCEL REPORT
WAREHOUSE REPORT:PURCHASERETURNREPORT:PURCHASE RETURN REPORT
WAREHOUSE REPORT:PURCHASERETURNSECTIONREPORT:PURCHASE RETURN BY SECTION
WAREHOUSE REPORT:PURCHASESECTIONREPORT:PURCHASE BY SECTION
WAREHOUSE REPORT:RETAILSTOCKANALYSISREPORT:RETAIL STOCK ANALYSIS REPORT
WAREHOUSE REPORT:SUPPLIERMASTER:SUPPLIER
WAREHOUSE REPORT:SUPPLIERPAYMENTREPORT:SUPPLIER PAYMENT REPORT
WAREHOUSE REPORT:SUPPLIERPENDING:SUPPLIER PENDING
WAREHOUSE REPORT:TAXMASTER:TAX
WAREHOUSE REPORT:WHSTOCKANALYSISREPORT:STOCK ANALYSIS REPORT
WAREHOUSE REPORT:WHSTOCKASONREPORT:WAREHOUSE AS ON STOCK
WAREHOUSE REPORT:WHSTOCKLOCATIONWISEREPORT:WAREHOUSE LOCATIONWISE STOCK
WAREHOUSE REPORT:WHSTOCKREPORT:WAREHOUSE STOCK REPORT
WAREHOUSE REPORT:WHSTOCKAUDITREPORT:WAREHOUSE STOCK AUDIT REPORT
WAREHOUSE REPORT:STOCKTRANSACTIONREPORT:STOCK TRANSACTION REPORT
WAREHOUSE REPORT:STOCKDEPRECIATIONREPORT:STOCK DEPRECIATION REPORT


SALES REPORT:SALESREPORT:SALES REPORT
SALES REPORT:UNSETTLEDSALESREPORT:UNSETTLED SALES REPORT
SALES REPORT:DAYSALESREPORT:SALES REPORT - DAY SUMMARY
SALES REPORT:SALESMANREPORT:SALESMAN REPORT
SALES REPORT:MARGINSALESREPORT:MARGIN SALES REPORT
SALES REPORT:SALESCANCELLEDREPORT:SALES CANCELLED REPORT
SALES REPORT:BULKBILLREPORT:APPROVAL SALES REPORT
SALES REPORT:ESTIMATEBILLREPORT:ESTIMATE SALES REPORT
SALES REPORT:DELIVERYREPORT:DELIVERY REPORT
SALES REPORT:ALTERATIONREPORT:ALTERATION REPORT
SALES REPORT:TEXTDAYSUMMARY:TEXT DAY REPORT
SALES REPORT:SNAPSHOTSALESREPORT:SNAPSHOT SALES REPORT
SALES REPORT:SETTLEMENTREPORT:SETTLEMENT REPORT
SALES REPORT:GIFTISSUEREPORT:GIFT ISSUE REPORT
SALES REPORT:SETTLEMENTOBCLREPORT:SETTLEMENT OPENING/CLOSING REPORT
SALES REPORT:SALESTAXREPORT:SALES TAX REPORT
SALES REPORT:SALESTAXSUMMARYREPORT:SALES TAX SUMMARY
SALES REPORT:SALESHSNREPORT:SALES HSN REPORT
SALES REPORT:CUSTOMERPAYMENTREPORT:CUSTOMER PAYMENT REPORT
SALES REPORT:CUSTOMERCREDITNOTEREPORT:CUSTOMER CREDIT NOTE REPORT
SALES REPORT:CUSTOMERREPORT:CUSTOMER REPORTS
SALES REPORT:SALESANALYSISREPORT:SALES ANALYSIS REPORT
SALES REPORT:SALESVSSETTLEMENTREPORT:SALES VS SETTLEMENT REPORT
SALES REPORT:SALESVSSTOCKREPORT:STOCK VS SALES VS PURCHASE REPORT
SALES REPORT:STOCKPURCHASESALEREPORT:STOCK VS SALES VS PURCHASE REPORT
SALES REPORT:RETAILSTOCKREPORT:RETAIL STOCK REPORT
SALES REPORT:RETAILDIRECTSTOCKREPORT:RETAIL DIRECT STOCK REPORT
SALES REPORT:RETAILSTOCKMARKERREPORT:RETAIL STOCK MARKER REPORT
SALES REPORT:RETAILPRICECHANGERREPORT:RETAIL PRICE CHANGER REPORT
SALES REPORT:RETAILSTOCKAUDITREPORT:RETAIL STOCK AUDIT REPORT
SALES REPORT:SALESBILLERREPORT:SALES BILLER REPORT
SALES REPORT:SALESCASHIERREPORT:SALES CASHIER REPORT
SALES REPORT:MOBILESCHEMEREPORT:PROMOTION SCHEME REPORT
SALES REPORT:SALESSECTIONINCENTIVEREPORT:SALES SECTIONIN CENTIVE REPORT
SALES REPORT:SALESEMPLOYINCENTIVEREPORT:SALES EMPLOY INCENTIVE REPORT
SALES REPORT:EMPLOYEEREPORT:EMPLOYEE REPORT
SALES REPORT:EMPLOYEEADVANCEPENDING:EMPLOYEE ADVANCE PENDING
SALES REPORT:B2BSALESREPORT:B2B SALES REPORT
SALES REPORT:B2BSALESTAXREPORT:B2B SALES TAX REPORT
SALES REPORT:B2BSALESHSNREPORT:B2B SALES HSN REPORT

ADDONS:ADDON_TARGETWITHMARGIN:Product Margin Against Target";

    public class License
    {
        public bool isoffline
        {
            get
            {
                return mode == 4 || (System.Configuration.ConfigurationManager.AppSettings["Offline"] == "true");
            }
        }
        public bool supportoffline
        {
            get
            {
                return offline || (System.Configuration.ConfigurationManager.AppSettings["SupportOffline"] == "true");
            }
        }
        public DateTime validupto { get; set; }
        public string company { get; set; }
        public int noofwarehouse { get; set; }
        public int noofshowroom { get; set; }
        public string key { get; set; }
        public int mode { get; set; }
        public string message { get; set; }
        public int noofusers { get; set; }
        public int noofemployees { get; set; }
        public bool valiate_upto { get; set; }
        public bool valiate_amc { get; set; }
        public bool valiate_locations { get; set; }
        public DateTime amcupto { get; set; }
        public int nowarehouse { get; set; }
        public bool onlyretail { get; set; }
        public int noofoffline { get; set; }
        public bool offline { get; set; }
        public bool payroll { get; set; }
        public bool jobwork { get; set; }
        public bool imagegallery { get; set; }
        public bool manufacture { get; set; }
        public bool basicanalytical { get; set; }
        public bool allanalytical { get; set; }
        public bool advanceanalytical { get; set; }
        public bool manualpos { get; set; }
        public bool promotions { get; set; }
        public bool intray { get; set; }
        public bool saas { get; set; }
        public bool lrinbox { get; set; }
        public bool stocksync { get; set; }
        public bool tallyintegration { get; set; }
        public bool finance { get; set; }
        public bool mobile { get; set; }
        public bool purchaseorder { get; set; }
        public bool itemimage { get; set; }
        public bool fmcgpos { get; set; }
        public bool crm { get; set; }
        public bool cashflow { get; set; }
        public bool delivery { get; set; }
        public bool fixedlocations { get; set; }
        public bool fixedcompany { get; set; }
        public bool allfeatures { get; set; }
        public bool developer { get; set; }
        public bool cancreatelocation { get; set; }
        public string version { get; set; }
        public DateTime createdon { get; set; }
        public bool valid { get; set; }
        public List<string> companylist { get; set; }
        public List<string> locationlist { get; set; }
        public List<string> restrictedcode { get; set; }
        public static License Create(string companyname)
        {
            try
            {
                //if (System.Diagnostics.Debugger.IsAttached)
                //{
                //    return new License()
                //    {
                //        valid = true,
                //        advanceanalytical = true,
                //        basicanalytical = true,
                //        cashflow = true,
                //        company = companyname,
                //        delivery = true,
                //        finance = true,
                //        intray = true,
                //        manufacture = true,
                //        key = License.Key,
                //        lrinbox = true,
                //        noofemployees = 100,
                //        payroll = true,
                //        promotions = true,
                //        tallyintegration = true,
                //        purchaseorder = true,
                //        stocksync = true,
                //        restrictedcode = new List<string>()
                //    };
                //}
                string licefile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "License.key");
                if (!System.IO.File.Exists(licefile))
                {
                    licefile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "License.temp.key");
                    if (!System.IO.File.Exists(licefile))
                    {
                        if (System.Configuration.ConfigurationManager.AppSettings["Offline"] == "true")
                            return CreateOfflineLicense(companyname);

                        throw new Exception("Can't find the license file @ " + licefile);
                    }
                }
                var license = CreateLicense(companyname, licefile);
                UpdatePermissions(license);

                return license;
            }
            catch (Exception exp)
            {
                var license = new License();
                license.key = License.Key;
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "Can't load the license");
                license.message = exp.Message;
                return license;
            }
        }

        private static void UpdatePermissions(License license)
        {
            license.restrictedcode = new List<string>();
            if (!license.intray)
            {

            }
            if (!license.delivery)
            {

            }
            if (!license.payroll)
            {

            }
            if (!license.basicanalytical)
            {

            }
            if (!license.advanceanalytical)
            {

            }
            if (!license.promotions)
            {

            }
            if (!license.lrinbox)
            {

            }
            if (!license.stocksync)
            {

            }
            if (!license.tallyintegration)
            {

            }
            if (!license.purchaseorder)
            {

            }
            if (!license.cashflow)
            {

            }
            if (!license.itemimage)
            {

            }
            if (!license.fmcgpos)
            {

            }
            if (!license.crm)
            {

            }
        }
        public static License CreateOfflineLicense(string companyname)
        {
            var license = new License()
            {
                mode = 4,
                valid = true,
                company = companyname,
                key = License.Key,
                restrictedcode = new List<string>()
            };
            UpdatePermissions(license);
            return license;
        }

        public static License CreateLicense(string companyname, string location)
        {
            var license = CreateLicense(companyname, Key, location);
            if (!license.valid)
                license = CreateLicense(companyname, SystemKey, location);

            return license;
        }
        public static License CreateLicense(string companyname, string key, string location)
        {
            bool validateDemo = location.Contains("License.temp.key");
            string linfo = System.IO.File.ReadAllText(location);
            string errormsg = "";
            var jsonlicense = linfo.Split(';');
            var encryption = new WebAPI.Data.Cryptor.AESEncryption(companyname, key.Replace("-", ""));
            string json = "null";
            var license = new License()
            {
                key = key,
                message = errormsg,
                valid = false
            };
            try
            {
                json = encryption.DecryptString(System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(jsonlicense[0])));
                license = Newtonsoft.Json.JsonConvert.DeserializeObject<License>(json);
            }
            catch (Exception exp)
            {
                if (jsonlicense.Length == 2)
                {
                    try
                    {
                        encryption = new WebAPI.Data.Cryptor.AESEncryption(Key, key.Replace("-", ""));
                        json = encryption.DecryptString(System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(jsonlicense[1])));
                        errormsg = "this is developer license";
                    }
                    catch (Exception exp1)
                    {
                        errormsg = "this is not the correct developer license";
                    }
                }
                else if (jsonlicense.Length == 3)
                {
                    try
                    {
                        encryption = new WebAPI.Data.Cryptor.AESEncryption(companyname, companyname.Replace("-", ""));
                        json = encryption.DecryptString(System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(jsonlicense[2])));
                        errormsg = "this is SaaS license";
                    }
                    catch (Exception exp1)
                    {
                        errormsg = "this SaaS license does't contain any company information @ " + companyname;
                        json = "null";
                    }
                }
                else
                {
                    errormsg = "this license does't contain any company information @ " + companyname;
                    json = "null";
                }
            }

            license = new License()
            {
                key = key,
                message = errormsg,
                valid = false
            };

            if (!string.IsNullOrEmpty(json) && json != "null")
            {
                license = Newtonsoft.Json.JsonConvert.DeserializeObject<License>(json);
                license.valid = true;
                if (validateDemo)
                {
                    license.valid = license.validupto >= DateTime.UtcNow.Date;
                }
            }
            license.key = Key;
            license.message = errormsg;
            return license;
        }

        public string ToJSON()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        public License Copy()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<License>(ToJSON());
        }
        public static string Key
        {
            get
            {
                return CreateKey(PrivateKey);
            }
        }

        private static string IPKey
        {
            get
            {
                ManagementClass mc = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = mc.GetInstances();

                string ipkey = "";
                foreach (ManagementObject mo in moc)
                {
                    if (ipkey == "")
                    {
                        foreach (var item in mo.Properties)
                        {
                            if (item.Name.ToLower() == "processorid" && item.Value != null)
                            {
                                ipkey = item.Value.ToString();
                                break;
                            }
                        }
                        //privatekey = mo.Properties["processorID"].Value.ToString();
                        //break;
                    }
                }
                return ipkey;
            }
        }

        private static string SystemKey
        {
            get
            {
                ManagementClass mc = new ManagementClass("win32_processor");
                ManagementObjectCollection moc = mc.GetInstances();

                string systemkey = "";
                string systemname = "";
                string processorname = "";
                StringBuilder sb = new StringBuilder();
                foreach (ManagementObject mo in moc)
                {
                    foreach (var item in mo.Properties)
                    {
                        sb.AppendFormat("{0}={1}", item.Name, item.Value);
                        if (item.Name.ToLower() == "systemname" && item.Value != null)
                        {
                            systemname = item.Value.ToString();
                            //break;
                        }
                        if (item.Name.ToLower() == "name" && item.Value != null)
                        {
                            processorname = item.Value.ToString();
                            //break;
                        }
                    }
                }
                systemkey = systemname + processorname;
                if (string.IsNullOrEmpty(systemkey))
                    throw new Exception("Can't find processor id - " + sb.ToString());
                return systemkey;
            }
        }

        static string privatekey = null;
        public static string PrivateKey
        {
            get
            {
                if (!string.IsNullOrEmpty(privatekey))
                    return privatekey;

                privatekey = IPKey;
                if (string.IsNullOrEmpty(privatekey))
                {
                    privatekey = SystemKey;
                }
                return privatekey;
            }
        }

        public bool IsSAAS { get; set; }

        private const string CHARACTERS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string Encode36(int number)
        {
            number = Math.Abs(number);
            List<char> database = new List<char>(CHARACTERS);
            List<char> value = new List<char>();
            long tmp = number;

            while (tmp != 0)
            {
                value.Add(database[Convert.ToInt32(tmp % 36)]);
                tmp /= 36;
            }
            value.Reverse();
            return new string(value.ToArray());
        }

        public static string CreateKey(string encryptionkey)
        {
            System.Security.Cryptography.MD5 md5Hasher = System.Security.Cryptography.MD5.Create();
            var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(encryptionkey));
            return InfyPOS.Common.EncodingExtensions.Encode36(BitConverter.ToInt32(hashed, 0)) + "-" + InfyPOS.Common.EncodingExtensions.Encode36(BitConverter.ToInt32(hashed, 4)) + "-" + InfyPOS.Common.EncodingExtensions.Encode36(BitConverter.ToInt32(hashed, 8)) + "-" + InfyPOS.Common.EncodingExtensions.Encode36(BitConverter.ToInt32(hashed, 12));
        }
    }
}