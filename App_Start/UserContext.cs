using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Claims;
using System.Web;

namespace InfyPOS.Common
{

    public sealed class UserClaim : MarshalByRefObject
    {

        private static bool? multipletimezone;
        private static int? defaultutcoffset;
        public bool HasMultipleTimeZone
        {
            get
            {
                if (multipletimezone.HasValue) return multipletimezone.Value;
                if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["MultipleTimeZone"]))
                    multipletimezone = false;
                else
                    multipletimezone = System.Configuration.ConfigurationManager.AppSettings["MultipleTimeZone"].ToLower() == "true";

                return multipletimezone.Value;
            }
        }
        public int DefaultUTCOffset
        {
            get
            {
                if (defaultutcoffset.HasValue) return defaultutcoffset.Value;
                if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["DefaultUTCOffset"]))
                    defaultutcoffset = -1;
                else
                    defaultutcoffset = int.Parse(System.Configuration.ConfigurationManager.AppSettings["DefaultUTCOffset"].ToLower());

                return defaultutcoffset.Value;
            }
        }
        public int UTCOffset
        {
            get
            {
                if (TZ != -1)
                {
                    return TZ;
                }
                else
                {
                    return DefaultUTCOffset;
                }
            }
        }
        public DateTime DateTimeNow
        {
            get
            {
                if (!HasMultipleTimeZone)
                    return DateTime.Now;

                var utcnow = DateTime.UtcNow.AddMinutes(UTCOffset);
                DateTime.SpecifyKind(utcnow, DateTimeKind.Unspecified);
                return utcnow;
            }
        }

        public UserClaim()
        {
            IsSystem = true;
            UserName = "SYSTEM";
            OrganizationId = 0;
        }
        public bool IsSystem { get;  set; }
        public long OrganizationId { get;  set; }
        public string UserName { get;  set; }
        public static UserClaim Default()
        {
            return new UserClaim();
        }

        //identity.AddClaim(new Claim(ClaimTypes.Role, user.roleName.ToString()));
        //    identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
        //    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.id.ToString()));
        //    identity.AddClaim(new Claim(ClaimTypes.Sid, session.id.ToString()));
        public UserClaim(IAuthenticationManager authenticationmanager)
        {
            if (authenticationmanager.User != null && authenticationmanager.User.Claims.Count() > 0)
            {
                Valid = true;
                var claim = authenticationmanager.User.FindFirst(ClaimTypes.Role);
                if (claim != null)
                    Role = claim.Value;
                claim = authenticationmanager.User.FindFirst(ClaimTypes.Name);
                if (claim != null)
                    Name = claim.Value;
                claim = authenticationmanager.User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim != null)
                    UserID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst("Infy.EmployeeName");
                if (claim != null)
                    EmployeeName = claim.Value;


                claim = authenticationmanager.User.FindFirst("Infy.PFM");
                if (claim != null)
                    Platform = claim.Value;

                claim = authenticationmanager.User.FindFirst("Infy.DVC");
                if (claim != null)
                    Device = claim.Value;

                claim = authenticationmanager.User.FindFirst("Infy.AD");
                if (claim != null)
                    AllowedDiscount = decimal.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst("Infy.RC");
                if (claim != null)
                    HasCustomerRestricted = bool.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst("Infy.TZ");
                if (claim != null)
                    TZ = int.Parse(claim.Value);
                else
                    TZ = -1;

                claim = authenticationmanager.User.FindFirst("Infy.TR");
                if (claim != null)
                {
                    if (!string.IsNullOrEmpty(claim.Value) && claim.Value.IndexOf(',') > 0)
                    {
                        AllowedTimeFrom = int.Parse(claim.Value.Split(',')[0]);
                        AllowedTimeTo = int.Parse(claim.Value.Split(',')[1]);
                    }
                }

                claim = authenticationmanager.User.FindFirst("Infy.Employeeid");
                if (claim != null)
                    EmployeeID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst("Infy.DSG");
                if (claim != null)
                    DsgID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst("Infy.Companyid");
                if (claim != null)
                    CompanyID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst(ClaimTypes.Sid);
                if (claim != null)
                    SessionID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst(ClaimTypes.PrimaryGroupSid);
                if (claim != null)
                    OrganizationCode = claim.Value;

                claim = authenticationmanager.User.FindFirst(ClaimTypes.PrimarySid);
                if (claim != null)
                    OrganizationID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst(ClaimTypes.System);
                if (claim != null)
                    CampusID = long.Parse(claim.Value);

                claim = authenticationmanager.User.FindFirst(ClaimTypes.GroupSid);
                if (claim != null)
                {
                    ChildOrganizationIDs = new List<long>();
                    foreach (var org in claim.Value.Split(','))
                    {
                        long orgid = 0;
                        if (long.TryParse(org, out orgid))
                            ChildOrganizationIDs.Add(orgid);
                    }
                }
                claim = authenticationmanager.User.FindFirst("Infy.Permission");
                if (claim != null)
                    Permission = claim.Value;

                accessablesections = new List<long>();
                claim = authenticationmanager.User.FindFirst("Infy.Section");
                if (claim != null)
                    accessablesections = string.IsNullOrEmpty(claim.Value) ? new List<long>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(claim.Value);

                accessiblelocation = new List<long>();
                claim = authenticationmanager.User.FindFirst("Infy.Location");
                if (claim != null)
                    accessiblelocation = string.IsNullOrEmpty(claim.Value) ? new List<long>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(claim.Value);

                accessiblecompany = new List<long>();
                claim = authenticationmanager.User.FindFirst("Infy.Company");
                if (claim != null)
                    accessiblecompany = string.IsNullOrEmpty(claim.Value) ? new List<long>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<long>>(claim.Value);

            }
        }
        public long DsgID { get; set; }
        public long EmployeeID { get; set; }
        public long CompanyID { get; set; }
        public List<long> accessiblelocation { get; set; }
        public List<long> accessiblecompany { get; set; }
        public List<long> accessablesections { get; set; }
        public string EmployeeName { get; set; }
        public string Platform { get; set; }
        public string Device { get; set; }
        public long CampusID { get; set; }
        public string OrganizationCode { get; set; }
        public long OrganizationID { get; set; }
        public decimal AllowedDiscount { get; set; }
        public bool HasCustomerRestricted { get; set; }
        public int TZ { get; set; }
        List<long> ChildOrganizationIDs { get; set; }

        public long SetInstitutionID(string givenid)
        {
            long id;
            if (long.TryParse(givenid, out id))
            {
                OrganizationID = id;
                ChildOrganizationIDs = new List<long>();
                ChildOrganizationIDs.Add(id);
            }
            return OrganizationID;
        }
        public long GetInstitutionID(string givenid)
        {
            long id;
            if (long.TryParse(givenid, out id))
                return GetInstitutionID(id);

            return OrganizationID;
        }
        public long GetInstitutionID(long givenid)
        {
            if (ChildOrganizationIDs != null && ChildOrganizationIDs.Contains(givenid))
                return givenid;

            return OrganizationID;
        }
        internal bool HasRights(long institutionid)
        {
            return ChildOrganizationIDs.Contains(institutionid);
        }

        public bool Valid { get; set; }
        public long UserID { get; set; }
        public long SessionID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Permission { get; set; }
        public int AllowedTimeFrom { get;  set; }
        public int AllowedTimeTo { get; set; }

        public bool HasPermission(PermissionSet permission, Action action)
        {
            return PermissionUtil.HasPermission(Permission, permission, action);
        }
    }

    // DONT ADD permission set in between, Add new permission at the end.
    public enum PermissionSet
    {
        productmaster,
        brandmaster,
        itemmaster,
        suppliermaster,
        agentmaster,
        transportmaster,
        companymaster,
        taxmaster,
        referencemaster,
        usermaster,
        customermaster,
        pricingmaster,
        refillbox,
        holidaymaster,
        homepage,
        dashboardsales,
        inbox,
        courier,
        lrentry,
        lrinvoice,
        lrproduct,
        barcodegen,
        lrissue,
        lrreceipt,
        poreturn,
        outward,
        pricechanger,
        festivaldiscount,
        sales,
        settlement,
        inward,
        cashopening,
        cashclosing,
        salereturn,
        refill,
        debitpayment,
        daybook,
        returnapproval,
        employee,
        salarystructure,
        salarymgmt,
        salarydeduction,
        payroll,
        empadvance,
        payslip,
        shiftallocation,
        employeeleave,
        employeeinout,
        formgeneration,
        porequestform,
        porequestque,
        purchaseorder,
        rptconfiguration,
        rptpurchase,
        rptporeturn,
        rptsales,
        rptsalesreturn,
        rpttax,
        rptsalesstatment,
        supplierpayment,
        rptstock,
        rptpayroll,
        holiday,
        salepromotion,
        rptretail,
        managestock,
        searchitem,
        rptwarehouse,
        receivegoods,
        delivery,
        trace,
        returngoods,
        itemsearch,
        missingstock,
        stockmanager,
        canapprovediscount,
        stockadjustment,
        tallyintegration,
        lrreport,
        lrinvoicereport,
        purchasereport,
        purchasehsnreport,
        purchasereturnreport,
        purchasereturncancelreport,
        outwardreport,
        outwardpendingreport,
        whstockreport,
        whstockanalysisreport,
        salesreport,
        salesreturnreport,
        settlementreport,
        cancelledbillreport,
        bulkbillreport,
        deliveryreport,
        salestaxreport,
        saleshsnreport,
        salesmanreport,
        retailstockreport,
        inwardreport,
        customerpaymentreport,
        salesanalysisreport,
        employeesticker,
        stockmanagement,
        bulksales,
        customerpayment,
        giftvoucher,
        manageledger,
        voucherentry,
        transfergoods,
        splitbarcode,
        stockmarker,
        stockentry,
        incrementmgmt,
        rpthr,
        rpthrdashboard,
        numberconfig,
        applicationsetting,
        showvalue,
        salesanalyser,
        stockanalyser,
        purchaseanalyser,
        purchaseplanner,
        stressanalyser,
        salesvspurchase,
        salesvsstock,
        salescomparer,
        salesplanner,
        rptproductmovement,
        revenuegeneration,
        intry,
        reorder,
        directpurchase,
        possales,
        productgroup,
        logtrace,
        giftpack,
        lastbit,
        incentivestatus,
        retailreport,
        canlockhr,
        showsellingvalue,
        hidesupplier,
        daysalesreport,
        hidedbstock,
        salescancelledreport,
        unsettledsalesreport,
        hidecashcollection,
        warehouselocation,
        hidecustomercat,
        employeepermission,
        estimatesale,
        textdaysummary,
        revokegiftvoucher,
        salebarcodeprice,
        advfinance
    }

    public enum Action
    {
        View,
        Create,
        Modify,
        Delete,
        Print
    }

    public enum LicenseModes
    {
        All,
        payroll,
        basicanalytical,
        advanceanalytical,
        promotions,
        intray,
        lrinbox,
        stocksync,
        tallyintegration,
        finance,
        purchaseorder,
        itemimage,
        fmcgpos,
        crm,
        cashflow,
        delivery,
        manualpos
    }

    public static class PermissionUtil
    {
        public static string GetPermissionString(long organizationid,List<Permissions> list)
        {
            if (list == null) return "";
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
            foreach (var item in list)
            {
                if (!LicenseManager.Instance(organizationid).Permissions.Exist(item.code))
                    continue;

                PermissionSet pset;
                if (Enum.TryParse<PermissionSet>(item.code.ToLower(), out pset))
                {
                    if (item.HasPermission)
                    {
                        bw.Write((byte)pset);
                        var bytes = new System.Collections.BitArray(new bool[] { item.view, item.create, item.modify, item.delete, item.print, false, false, false });
                        bw.Write(bytes.ToByte());
                    }
                }
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static bool GetBit(this byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public static bool HasPermission(string permissions, PermissionSet permission, Action action)
        {
            if (permissions == null) return false;
            byte permissionIndex = (byte)permission;
            byte[] bytes = Convert.FromBase64String(permissions);
            for (int i = 0; i < bytes.Length; i = i + 2)
            {
                if (permissionIndex == bytes[i])
                {
                    byte b = bytes[i + 1];
                    
                    switch (action)
                    {
                        case Action.View:
                            return b.GetBit(0);
                        case Action.Create:
                            return b.GetBit(1);
                        case Action.Modify:
                            return b.GetBit(2);
                        case Action.Delete:
                            return b.GetBit(3);
                        case Action.Print:
                            return b.GetBit(4);
                    }
                    return false;
                }
            }
            return false;
        }

        public static byte ToByte(this System.Collections.BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        private static byte ConvertToByte(System.Collections.BitArray bits)
        {
            byte result = 0;
            for (byte i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    result |= (byte)(1 << i);
            }
            return result;
        }
    }

    public class Permissions
    {
        public string code { get; set; }
        public bool view { get; set; }
        public bool create { get; set; }
        public bool modify { get; set; }
        public bool delete { get; set; }
        public bool print { get; set; }
        public bool HasPermission
        {
            get
            {
                return view || create || modify || delete || print;
            }
        }
    }
}