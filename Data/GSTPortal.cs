using InfyPOS.Common;
using InfyPOS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InfyGST
{
    public class GSTPortal
    {

        public class GstRequest : BaseEntity
        {
            public string accesskey { get; set; }
            public string GSTN { get; set; }
            public string portaluserid { get; set; }
            public string defaultLoginDetail { get; set; }
            public string apisetting { get; set; }
        }
        public class RequestOTP : GstRequest
        {
            public string otp { get; set; }
        }
        public class RequestGSTN : GstRequest
        {
            public string searchgstn { get; set; }
        }
        public class RequestUpdateGSTR1 : GstRequest
        {
            public string period { get; set; }
            public string json { get; set; }
        }
        public class RequestGSTR1Status : RequestUpdateGSTR1
        {
            public string referenceid { get; set; }
        }
        public class Taxpayer
        {

            public Pradr pradr { get; set; }
            public List<Adadr> adadr { get; set; }
            public string ctj { get; set; }
            public string ctjCd { get; set; }
            public string sts { get; set; }
            public string tradeNam { get; set; }
            public string ctb { get; set; }
            public string rgdt { get; set; }
            public List<string> nba { get; set; }
            public string gstin { get; set; }
            public string cxdt { get; set; }
            public string dty { get; set; }
            public string stj { get; set; }
            public string lgnm { get; set; }
            public string stjCd { get; set; }
            public string lstupdt { get; set; }

            public class Adadr
            {

                public Addr addr { get; set; }
                public string ntr { get; set; }

            }
            public class Pradr
            {


                public Addr addr { get; set; }
                public string ntr { get; set; }


            }
            public class Addr
            {

                public string bnm { get; set; }
                public string loc { get; set; }
                public string st { get; set; }
                public string bno { get; set; }
                public string stcd { get; set; }
                public string dst { get; set; }
                public string city { get; set; }
                public string flno { get; set; }
                public string lt { get; set; }
                public string lg { get; set; }
                public string pncd { get; set; }
                public string address { get; set; }
            }
        }
        public class GSTDownload
        {
            public string key { get; set; }
            public GSTDownloadResponse data { get; set; }
        }
        public class GSTDownloadResponse
        {
            public bool IsSuccess { get; set; }
            public string TxnOutcome { get; set; }
            public string data { get; set; }
            public string TokenNumber { get; set; }
            public string EstimatedTime { get; set; }
            public string ReferenceID { get; set; }
            public string TransactionID { get; set; }
        }
        public class GSTResponse
        {
            public bool IsSuccess { get; set; }
            public bool HasSessionChanged { get; set; }
            public GSTSession Session { get; set; }
            public APISetting Setting { get; set; }
            public string TxnOutcome { get; set; }
            public class APISetting
            {

                public int ID
                {
                    get;
                    set;
                }

                public string ASPName
                {
                    get;
                    set;
                }

                public bool IsActive
                {
                    get;
                    set;
                }

                public bool IsDefault
                {
                    get;
                    set;
                }

                public string AspWebsite
                {
                    get;
                    set;
                }

                public string AspUserId
                {
                    get;
                    set;
                }

                public string AspPassword
                {
                    get;
                    set;
                }

                public string AspAccountPassword
                {
                    get;
                    set;
                }

                public byte[] AspDscPfx
                {
                    get;
                    set;
                }

                public string AspDscPassword
                {
                    get;
                    set;
                }

                public string AspEK
                {
                    get;
                    set;
                }

                public string AspSessionID
                {
                    get;
                    set;
                }

                [DefaultValue(0)]
                public int AspSessionValidityMins
                {
                    get;
                    set;
                }

                public DateTime? AspSessionExp
                {
                    get;
                    set;
                }

                public string UrlAuth
                {
                    get;
                    set;
                }

                public string AuthVer
                {
                    get;
                    set;
                }

                public string UrlReturn
                {
                    get;
                    set;
                }

                public string ReturnVer
                {
                    get;
                    set;
                }

                public string UrlLedger
                {
                    get;
                    set;
                }

                public string LedgerVer
                {
                    get;
                    set;
                }

                public string UrlAspUtil
                {
                    get;
                    set;
                }

                public string UrlPayment
                {
                    get;
                    set;
                }

                public string PaymentVer
                {
                    get;
                    set;
                }

                public string GSTNClientId
                {
                    get;
                    set;
                }

                public string GSTNClientSecret
                {
                    get;
                    set;
                }
            }
            public class GSTSession
            {
                public string GstPortalUserID
                {
                    get;
                    set;
                }

                public string GSTIN
                {
                    get;
                    set;
                }

                public string AppKey
                {
                    get;
                    set;
                }

                public string AuthToken
                {
                    get;
                    set;
                }

                public DateTime TokenExp
                {
                    get;
                    set;
                }

                public string SessionEK
                {
                    get;
                    set;
                }
            }
        }
        public class RequestDownloadGSTR2B : GstRequest
        {
            public string period { get; set; }
        }
        public string GSTN;
        public string PortalUserID;
        public string URI;
        public string AccessKey;
        public long Organizationid;
        public void Initalize(string url,
            long organizationid,
            string GSTN,
            string PortalUserID,
            string AccessKey)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            this.GSTN = GSTN;
            this.Organizationid = organizationid;
            this.AccessKey = AccessKey;
            this.URI = url + "/InfyGSTService";
            this.PortalUserID = PortalUserID;
        }

        public async Task<GSTResponse> HasValidTokenAsync()
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new GstRequest() { GSTN = this.GSTN, portaluserid = this.PortalUserID, accesskey = this.AccessKey };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/HasValidToken", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<GSTResponse>(json);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/HasValidToken Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new GSTResponse() { IsSuccess = false };
        }

        public async Task<GSTResponse> GetOTPAsync()
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestGSTN() { GSTN = this.GSTN, accesskey = this.AccessKey, portaluserid = this.PortalUserID };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/GetOTP", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/GetOTP Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new GSTResponse() { IsSuccess = false };
        }

        public async Task<GSTResponse> ApplyOTPAsync(string otp)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestOTP() { GSTN = this.GSTN, accesskey = this.AccessKey, portaluserid = this.PortalUserID, otp = otp };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/ApplyOTP", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/ApplyOTP Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }

            }
            return new GSTResponse() { IsSuccess = false };
        }



        public async Task<GSTDownloadResponse> UpdateGSTR1AAsync(string period, string bodyjson)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestUpdateGSTR1()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period,
                    json = bodyjson
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/UpdateGSTR1A", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTDownloadResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTDownloadResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/UpdateGSTR1A Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new GSTDownloadResponse() { IsSuccess = false };
        }


        public async Task<GSTDownloadResponse> GetGSTR1AStatus(string period, string referenceid)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestGSTR1Status()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period,
                    referenceid = referenceid
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/GetGSTR1AStatus", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTDownloadResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTDownloadResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/GetGSTR1AStatus Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }

            }
            return new GSTDownloadResponse() { IsSuccess = false };
        }

        public async Task<GSTDownloadResponse> ResetGSTR1A(string period)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestDownloadGSTR2B()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/ResetGSTR1A", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTDownloadResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTDownloadResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/ResetGSTR1A Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new GSTDownloadResponse() { IsSuccess = false };
        }


        public async Task<GSTDownloadResponse> ValidateHSN(string period)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestDownloadGSTR2B()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");

                var result = Task.Run(() => client.PostAsync(URI + "/ValidateHSN", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTDownloadResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTDownloadResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/ValidateHSN Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new GSTDownloadResponse() { IsSuccess = false };
        }

        private bool ValidateClient()
        {
            if (string.IsNullOrEmpty(this.GSTN))
                throw new Exception("GST No Of the comapany is missing");
            if (string.IsNullOrEmpty(this.AccessKey))
                throw new Exception("GST Server Access Key is missing");
            if (string.IsNullOrEmpty(this.PortalUserID))
                throw new Exception("GST Portal User is missing");
            if (URI == "/InfyGSTService")
                throw new Exception("GST Server URL is missing on application settings");
            return true;
        }
        public async Task<Taxpayer> SearchGSTN(string gstn)
        {

            ValidateClient();

            using (var client = new HttpClient())
            {
                var creteria = new RequestGSTN()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    searchgstn = gstn
                };


                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log("Search GSTN " + URI + "/SearchGSTN", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/SearchGSTN", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log("Search GSTN Response ", WebAPI.Common.Logger.LogType.Error, json);
                    return JsonConvert.DeserializeObject<Taxpayer>(json);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log("Search GSTN Response ", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }

        }
        public async Task<GSTDownloadResponse> DownloadGSTR2BAsync(string period)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestDownloadGSTR2B()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/DownloadGSTR2B", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/DownloadGSTR2B", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<GSTDownloadResponse>(json);
                    if (response.IsSuccess)
                        return response;
                    return new GSTDownloadResponse() { IsSuccess = false, TxnOutcome = response.TxnOutcome };
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/DownloadGSTR2B Response", WebAPI.Common.Logger.LogType.Error, json);
                }
            }
            return new GSTDownloadResponse() { IsSuccess = false };
        }

        public async Task<List<GSTDownload>> DownloadGSTR2AAsync(string period)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestDownloadGSTR2B()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/DownloadGSTR2A", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/DownloadGSTR2A", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<GSTDownload>>(json);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/DownloadGSTR2A Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new List<GSTDownload>();
        }


        public async Task<List<GSTDownload>> DownloadGSTR1AAsync(string period)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var creteria = new RequestDownloadGSTR2B()
                {
                    GSTN = this.GSTN,
                    accesskey = this.AccessKey,
                    portaluserid = this.PortalUserID,
                    period = period
                };
                var serializedProduct = JsonConvert.SerializeObject(creteria);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/DownloadGSTR1", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/DownloadGSTR1", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<GSTDownload>>(json);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/DownloadGSTR1 Response", WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new List<GSTDownload>();
        }

        public string GetResourceTextFile(string filename)
        {
            string result = string.Empty;

            using (Stream stream = this.GetType().Assembly.
                       GetManifestResourceStream(filename))
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }
            }
            return result;
        }


        public class EInvoiceResponse
        {
            [JsonIgnore]
            public bool isb2b { get; set; }
            public string AckNo { get; set; }
            public string AckDt { get; set; }
            public string Irn { get; set; }
            public string SignedInvoice { get; set; }
            public string SignedQRCode { get; set; }
            public string Status { get; set; }
            public string EwbNo { get; set; }
            public string EwbDt { get; set; }
            public string EwbValidTill { get; set; }
            public EInvoiceInvoiceData ExtractedSignedInvoiceData { get; set; }
            public EInvoiceQRCode ExtractedSignedQrCode { get; set; }
            public string QrCodeImage { get; set; }
            public string JwtIssuer { get; set; }
            [JsonProperty("ErrorDetails")]
            public List<ErrorDetail> ErrorDetails { get; set; }
            public InfyPOS.Models.Billinbox BillInbox { get; set; }
            public InfyPOS.Models.Voucher Voucher { get; set; }
            public class ErrorDetail
            {
                [JsonProperty("ErrorCode")]
                public long ErrorCode { get; set; }

                [JsonProperty("ErrorMessage")]
                public string ErrorMessage { get; set; }
            }
        }

        public class EInvoiceInvoiceData
        {
            public long AckNo { get; set; }
            public string AckDt { get; set; }
            public string Version { get; set; }
            public string Irn { get; set; }
            public TranDetails TranDtls { get; set; }
            public DocSetails DocDtls { get; set; }
            public SellerDetails SellerDtls { get; set; }

            public BuyerDetails BuyerDtls { get; set; }
            public DispatchedDetails DispDtls { get; set; }
            public ShippedDetails ShipDtls { get; set; }
            public ValDetails ValDtls { get; set; }
            public PayDetails PayDtls { get; set; }
            public ExpDetails ExpDtls { get; set; }
            public RefDetails RefDtls { get; set; }
            public List<ItmList> ItemList { get; set; }
            public class ItmList
            {
                //public string PrdNm { get; set; }
                /// <summary>
                ///Required Parameter - "Serial No. of Item"
                /// </summary>
                public string SlNo { get; set; }
                /// <summary>
                /// "Product Description"
                /// </summary>
                public string PrdDesc { get; set; }
                /// <summary>
                ///Required Parameter - "Specify whether the supply is service or not. Specify Y-for Service"
                /// </summary>
                public string IsServc { get; set; }
                /// <summary>
                ///Required Parameter - "HSN Code"
                /// </summary>
                public string HsnCd { get; set; }
                public BatchDetails BchDtls { get; set; }
                public class BatchDetails
                {
                    /// <summary>
                    ///Required Parameter - "Batch name"
                    /// </summary>
                    public string Nm { get; set; }
                    /// <summary>
                    /// "Batch Expiry Date"
                    /// </summary>
                    public string ExpDt { get; set; }
                    /// <summary>
                    /// "Warranty Date"
                    /// </summary>
                    public string WrDt { get; set; }

                }
                /// <summary>
                ///  "Bar Code"
                /// </summary>
                public string Barcde { get; set; }
                /// <summary>
                /// "Quantity"
                /// </summary>
                public double Qty { get; set; }
                /// <summary>
                /// "Free Quantity"
                /// </summary>
                public double FreeQty { get; set; }
                /// <summary>
                /// "Unit"
                /// </summary>
                public string Unit { get; set; }
                /// <summary>
                ///Required Parameter - "Unit Price - Rate"
                /// </summary>
                public double UnitPrice { get; set; }
                /// <summary>
                ///Required Parameter - "Gross Amount Amount (Unit Price * Quantity)"
                /// </summary>
                public double TotAmt { get; set; }
                /// <summary>
                /// "Discount"
                /// </summary>
                public double Discount { get; set; }
                /// <summary>
                /// "Pre tax value"
                /// </summary>
                public double PreTaxVal { get; set; }
                /// <summary>
                ///Required Parameter - "Taxable Value (Total Amount -Discount)"
                /// </summary>
                public double AssAmt { get; set; }
                /// <summary>
                ///Required Parameter - "The GST rate, represented as percentage that applies to the invoiced item. It will IGST rate only."
                /// </summary>
                public double GstRt { get; set; }
                /// <summary>
                /// " Amount of IGST payable."
                /// </summary>
                public double IgstAmt { get; set; }
                /// <summary>
                /// " Amount of CGST payable."
                /// </summary>
                public double CgstAmt { get; set; }
                /// <summary>
                /// " Amount of SGST payable."
                /// </summary>
                public double SgstAmt { get; set; }
                /// <summary>
                /// "Cess Rate"
                /// </summary>
                public double CesRt { get; set; }
                /// <summary>
                /// "Cess Amount(Advalorem) on basis of rate and quantity of item"
                /// </summary>
                public double CesAmt { get; set; }
                /// <summary>
                /// "Cess Non-Advol Amount"
                /// </summary>
                public double CesNonAdvlAmt { get; set; }
                /// <summary>
                /// "State CESS Rate"
                /// </summary>
                public double StateCesRt { get; set; }
                /// <summary>
                /// "State CESS Amount"
                /// </summary>
                public double StateCesAmt { get; set; }
                /// <summary>
                ///  "State CESS Non Adval Amount"
                /// </summary>
                public double StateCesNonAdvlAmt { get; set; }
                /// <summary>
                /// "Other Charges"
                /// </summary>
                public double OthChrg { get; set; }
                /// <summary>
                ///TotItemVal "Total Item Value = Assessable Amount + CGST Amt + SGST Amt + Cess Amt + CesNonAdvlAmt + StateCesAmt + StateCesNonAdvlAmt+Otherchrg"
                /// </summary>
                public double TotItemVal { get; set; }
                /// <summary>
                /// "Order line referencee"
                /// </summary>
                public string OrdLineRef { get; set; }
                /// <summary>
                /// "Orgin Country"
                /// </summary>
                public string OrgCntry { get; set; }
                /// <summary>
                /// "Serial number in case of each item having a unique number."
                /// </summary>
                public string PrdSlNo { get; set; }
                public List<AttributeDtls> AttribDtls { get; set; }
                public class AttributeDtls
                {
                    /// <summary>
                    /// "Attribute details of the item"
                    /// </summary>
                    public string Nm { get; set; }
                    /// <summary>
                    /// "Attribute value of the item"
                    /// </summary>
                    public string Val { get; set; }
                }
            }
            public class RefDetails
            {
                /// <summary>
                /// "Remarks/Note"
                /// </summary>
                public string InvRmk { get; set; }
                /// <summary>
                ///Required Parameter - "Invoice Period Start Date"
                /// </summary>
                public string InvStDt { get; set; }
                /// <summary>
                ///Required Parameter - "Invoice Period End Date"
                /// </summary>
                public string InvEndDt { get; set; }
                public PrecDocument PrecDocDtls { get; set; }
                public class PrecDocument
                {
                    /// <summary>
                    ///Required Parameter - "Reference of original invoice, if any."
                    /// </summary>
                    public string InvNo { get; set; }
                    /// <summary>
                    ///Required Parameter - "Date of preceding invoice"
                    /// </summary>
                    public string InvDt { get; set; }
                    /// <summary>
                    /// "Other Reference"
                    /// </summary>
                    public string OthRefNo { get; set; }
                }
                public List<Contract> ContrDtls { get; set; }
                public class Contract
                {
                    /// <summary>
                    /// "Receipt Advice No."
                    /// </summary>
                    public string RecAdvDt { get; set; }
                    /// <summary>
                    /// "Date of receipt advice"
                    /// </summary>
                    public string RecAdvRefr { get; set; }
                    /// <summary>
                    /// "Lot/Batch Reference No."
                    /// </summary>
                    public string TendRefr { get; set; }
                    /// <summary>
                    /// "Contract Reference Number"
                    /// </summary>
                    public string ContrRefr { get; set; }
                    /// <summary>
                    ///  "Any other reference"
                    /// </summary>
                    public string ExtRefr { get; set; }
                    /// <summary>
                    /// "Project Reference Number"
                    /// </summary>
                    public string ProjRefr { get; set; }
                    /// <summary>
                    /// "Vendor PO Reference Number"
                    /// </summary>
                    public string PORefr { get; set; }
                    /// <summary>
                    /// "Vendor PO Reference date"
                    /// </summary>
                    public string PORefDt { get; set; }

                }
            }
            public class PayDetails
            {
                /// <summary>
                ///  "Payee Name"
                /// </summary>
                public string Nm { get; set; }
                /// <summary>
                /// "Bank account number of payee"
                /// </summary>
                public string AcctDet { get; set; }
                /// <summary>
                /// "Mode of Payment: Cash, Credit, Direct Transfer"
                /// </summary>
                public string Mode { get; set; }
                /// <summary>
                /// "Branch or IFSC code"
                /// </summary>
                public string FinInsBr { get; set; }
                /// <summary>
                /// "Credit Transfer"
                /// </summary>
                public string CrTrn { get; set; }
                /// <summary>
                /// "Payment Instruction"
                /// </summary>
                public string PayInstr { get; set; }
                /// <summary>
                /// "Terms of Payment"
                /// </summary>
                public string PayTerm { get; set; }
                /// <summary>
                /// "Direct Debit"
                /// </summary>
                public string DirDr { get; set; }
                /// <summary>
                /// "Credit Days"
                /// </summary>
                public int CrDay { get; set; }
                /// <summary>
                /// "The sum of amount which have been paid in advance."
                /// </summary>
                public double PaidAmt { get; set; }
                /// <summary>
                /// "Outstanding amount that is required to be paid."
                /// </summary>
                public double PaymtDue { get; set; }
            }

            public class ShippedDetails
            {
                /// <summary>
                ///"GSTIN of entity to whom goods are shipped"
                /// </summary>
                public string Gstin { get; set; }
                /// <summary>
                ///Required Parameter - "Legal Name"
                /// </summary>
                public string LglNm { get; set; }
                /// <summary>
                /// "Tradename"
                /// </summary>
                public string TrdNm { get; set; }
                /// <summary>
                ///Required Parameter - "Building/Flat no, Road/Street"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                //public string Bno { get; set; }
                //public string Bnm { get; set; }
                //public string Flno { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                //public string Dst { get; set; }
                /// <summary>
                ///Required Parameter - "Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///Required Parameter - "State Code to which supplies are shipped to."
                /// </summary>
                public string Stcd { get; set; }
                public string State { get; set; }
            }
            public class ValDetails
            {
                /// <summary>
                ///Required Parameter - "Total Assessable value of all items"
                /// </summary>
                public Nullable<double> AssVal { get; set; }
                /// <summary>
                /// "Total CGST value of all items"
                /// </summary>
                public Nullable<double> CgstVal { get; set; }
                /// <summary>
                /// "Total SGST value of all items"
                /// </summary>
                public Nullable<double> SgstVal { get; set; }
                /// <summary>
                /// "Total IGST value of all items"
                /// </summary>
                public Nullable<double> IgstVal { get; set; }
                /// <summary>
                /// "Total CESS value of all items"
                /// </summary>
                public Nullable<double> CesVal { get; set; }
                //public Nullable<double> CesNonAdVal { get; set; }
                /// <summary>
                /// "Total State CESS value of all items"
                /// </summary>
                public Nullable<double> StCesVal { get; set; }
                /// <summary>
                /// "Rounded off amount"
                /// </summary>
                public Nullable<double> RndOffAmt { get; set; }
                /// <summary>
                ///Required Parameter - "Final Invoice value "
                /// </summary>
                public Nullable<double> TotInvVal { get; set; }
                /// <summary>
                /// "Final Invoice value in Additional Currency"
                /// </summary>
                public Nullable<double> TotInvValFc { get; set; }
            }
            public class SellerDetails
            {
                /// <summary>
                ///Required Parameter - "GSTIN"
                /// </summary>
                public string Gstin { get; set; }
                /// <summary>
                ///Required Parameter - "Legal Name"
                /// </summary>
                public string LglNm { get; set; }
                /// <summary>
                /// "Tradename"
                /// </summary>
                public string TrdNm { get; set; }
                /// <summary>
                ///Required Parameter - "Building/Flat no, Road/Street"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                //public string Bno { get; set; }
                //public string Bnm { get; set; }
                //public string Flno { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                //public string Dst { get; set; }
                /// <summary>
                ///Required Parameter - "Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///Required Parameter - "State Name"
                /// </summary>
                public string State { get; set; }
                public string Stcd { get; set; }
                /// <summary>
                /// "Phone or Mobile No."
                /// </summary>
                public string Ph { get; set; }
                /// <summary>
                /// "Email-Id"
                /// </summary>
                public string Em { get; set; }
            }
            public class BuyerDetails
            {
                /// <summary>
                ///Required Parameter - "GSTIN"
                /// </summary>
                public string Gstin { get; set; }
                /// <summary>
                ///Required Parameter - "Legal Name"
                /// </summary>
                public string LglNm { get; set; }
                /// <summary>
                /// "Tradename"
                /// </summary>
                public string TrdNm { get; set; }

                /// <summary>
                ///Required Parameter - "State code of Place of supply. If POS lies outside the country, a the code shall be 96."
                /// </summary>
                public string Pos { get; set; }
                /// <summary>
                ///Required Parameter - "Building/Flat no, Road/Street"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                //public string Bno { get; set; }
                //public string Bnm { get; set; }
                //public string Flno { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                //public string Dst { get; set; }
                /// <summary>
                ///"Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///"State Name"
                /// </summary>
                public string State { get; set; }
                public string Stcd { get; set; }
                /// <summary>
                /// "Phone or Mobile No."
                /// </summary>
                public string Ph { get; set; }
                /// <summary>
                /// "Email-Id"
                /// </summary>
                public string Em { get; set; }
            }
            public class ExpDetails
            {
                /// <summary>
                /// "Shipping Bill No."
                /// </summary>
                public string ShipBNo { get; set; }
                /// <summary>
                ///  "Shipping Bill Date"
                /// </summary>
                public string ShipBDt { get; set; }
                /// <summary>
                /// "Country Code"
                /// </summary>
                public string CntCode { get; set; }
                /// <summary>
                /// "Additional Currency Code"
                /// </summary>
                public string ForCur { get; set; }
                public Nullable<double> InvForCur { get; set; }
                /// <summary>
                /// "Port Code"
                /// </summary>
                public string Port { get; set; }
                /// <summary>
                ///  "Options for supplier for refund. Y/N"
                /// </summary>
                public string RefClm { get; set; }
            }
            public class DocSetails
            {
                /// <summary>
                /// Required Parameter - "description": "Document Type: INVOICE, CREDIT NOTE, DEBIT NOTE"
                /// </summary>
                public string Typ { get; set; }
                /// <summary>
                ///Required Parameter - "Document Number"
                /// </summary>
                public string No { get; set; }
                /// <summary>
                ///Required Parameter - "Document Date"
                /// </summary>
                public string Dt { get; set; } //
                                               //public string OrgInvNo { get; set; }
            }
            public class TranDetails
            {
                /// <summary>
                /// Required Parameter - "GST- Goods and Services Tax Scheme"
                /// </summary>
                public string TaxSch { get; set; }
                /// <summary>
                /// Required Parameter - supply Type
                /// </summary>
                public string SupTyp { get; set; }
                //public string Catg { get; set; }
                /// <summary>
                /// "Y- whether the tax liability is payable under reverse charge"
                /// </summary>
                public string RegRev { get; set; }
                //public string Typ { get; set; }
                //public string EcmTrn { get; set; }
                /// <summary>
                /// "GSTIN of e-Commerce operator"
                /// </summary>
                public string EcmGstin { get; set; }
                //public bool EcmTrnSel { get; set; }
            }
            public class DispatchedDetails
            {
                /// <summary>
                ///Required Parameter - "Name of the company from which the goods are dispatched"
                /// </summary>
                public string Nm { get; set; }
                /// <summary>
                ///Required Parameter - "Address 1 of the entity from which goods are dispatched.(Building/Flat no.Road/Street etc.)"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the entity from which goods are dispatched. (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                /// <summary>
                ///Required Parameter - "Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///Required Parameter - "State Code"
                /// </summary>
                public string Stcd { get; set; }
                public string State { get; set; }
            }

        }
        public class EInvoiceQRCode
        {
            public string SellerGstin { get; set; }
            public string BuyerGstin { get; set; }
            public string DocNo { get; set; }
            public string DocTyp { get; set; }
            public string DocDt { get; set; }
            public Nullable<double> TotInvVal { get; set; }
            public string ItemCnt { get; set; }
            public string MainHsnCode { get; set; }
            public string Irn { get; set; }
        }

        public class EInvoiceRequest
        {
            /// <summary>
            ///Required Parameter - "Version of the schema"
            /// </summary>
            public string Version { get; set; }
            /// <summary>
            /// Required Parameter - Transaction Details
            /// </summary>
            public TranDetails TranDtls { get; set; }
            /// <summary>
            /// Required Parameter - Doccument Details
            /// </summary>
            public DocSetails DocDtls { get; set; }
            public ExpDetails ExpDtls { get; set; }
            /// <summary>
            /// Required Parameter - Seller Details
            /// </summary>
            public SellerDetails SellerDtls { get; set; }
            /// <summary>
            ///Required Parameter - Buyer Details
            /// </summary>
            public BuyerDetails BuyerDtls { get; set; }
            public DispatchedDetails DispDtls { get; set; }
            public ShippedDetails ShipDtls { get; set; }
            /// <summary>
            /// Required Parameter
            /// </summary>
            public ValDetails ValDtls { get; set; }
            public PayDetails PayDtls { get; set; }
            public RefDetails RefDtls { get; set; }
            public EwbDetails EwbDtls { get; set; }
            public class EwbDetails
            {
                /// <summary>
                /// "Transin/GSTIN"
                /// </summary>
                public string TransId { get; set; }
                /// <summary>
                /// "Name of the transporter"
                /// </summary>
                public string TransName { get; set; }
                /// <summary>
                ///Required Parameter - "Mode of transport (Road-1, Rail-2, Air-3, Ship-4)"
                /// </summary>
                public string TransMode { get; set; }
                /// <summary>
                ///Required Parameter - "Distance between source and destination PIN codes"
                /// </summary>
                public int Distance { get; set; }
                /// <summary>
                /// "Tranport Document Number"
                /// </summary>
                public string TransDocNo { get; set; }
                /// <summary>
                /// "Transport Document Date"
                /// </summary>
                public string TransDocDt { get; set; }
                /// <summary>
                /// "Vehicle Number"
                /// </summary>
                public string VehNo { get; set; }
                /// <summary>
                /// "Whether O-ODC or R-Regular "
                /// </summary>
                public string VehType { get; set; }
            }
            public List<AddlDocument> AddlDocDtls { get; set; }
            public class AddlDocument
            {
                /// <summary>
                /// "Supporting document URL"
                /// </summary>
                public string Url { get; set; }
                /// <summary>
                /// "Supporting document in Base64 Format"
                /// </summary>
                public string Docs { get; set; }
                /// <summary>
                /// "Any additional information"
                /// </summary>
                public string Info { get; set; }
            }
            /// <summary>
            /// Required Parameter - Item List Details
            /// </summary>
            public List<ItmList> ItemList { get; set; }
            public class ItmList
            {
                //public string PrdNm { get; set; }
                /// <summary>
                ///Required Parameter - "Serial No. of Item"
                /// </summary>
                public string SlNo { get; set; }
                /// <summary>
                /// "Product Description"
                /// </summary>
                public string PrdDesc { get; set; }
                /// <summary>
                ///Required Parameter - "Specify whether the supply is service or not. Specify Y-for Service"
                /// </summary>
                public string IsServc { get; set; }
                /// <summary>
                ///Required Parameter - "HSN Code"
                /// </summary>
                public string HsnCd { get; set; }
                public BatchDetails BchDtls { get; set; }
                public class BatchDetails
                {
                    /// <summary>
                    ///Required Parameter - "Batch name"
                    /// </summary>
                    public string Nm { get; set; }
                    /// <summary>
                    /// "Batch Expiry Date"
                    /// </summary>
                    public string ExpDt { get; set; }
                    /// <summary>
                    /// "Warranty Date"
                    /// </summary>
                    public string WrDt { get; set; }

                }
                /// <summary>
                ///  "Bar Code"
                /// </summary>
                public string Barcde { get; set; }
                /// <summary>
                /// "Quantity"
                /// </summary>
                public double Qty { get; set; }
                /// <summary>
                /// "Free Quantity"
                /// </summary>
                public double FreeQty { get; set; }
                /// <summary>
                /// "Unit"
                /// </summary>
                public string Unit { get; set; }
                /// <summary>
                ///Required Parameter - "Unit Price - Rate"
                /// </summary>
                public double UnitPrice { get; set; }
                /// <summary>
                ///Required Parameter - "Gross Amount Amount (Unit Price * Quantity)"
                /// </summary>
                public double TotAmt { get; set; }
                /// <summary>
                /// "Discount"
                /// </summary>
                public double Discount { get; set; }
                /// <summary>
                /// "Pre tax value"
                /// </summary>
                public double PreTaxVal { get; set; }
                /// <summary>
                ///Required Parameter - "Taxable Value (Total Amount -Discount)"
                /// </summary>
                public double AssAmt { get; set; }
                /// <summary>
                ///Required Parameter - "The GST rate, represented as percentage that applies to the invoiced item. It will IGST rate only."
                /// </summary>
                public double GstRt { get; set; }
                /// <summary>
                /// " Amount of IGST payable."
                /// </summary>
                public double IgstAmt { get; set; }
                /// <summary>
                /// " Amount of CGST payable."
                /// </summary>
                public double CgstAmt { get; set; }
                /// <summary>
                /// " Amount of SGST payable."
                /// </summary>
                public double SgstAmt { get; set; }
                /// <summary>
                /// "Cess Rate"
                /// </summary>
                public double CesRt { get; set; }
                /// <summary>
                /// "Cess Amount(Advalorem) on basis of rate and quantity of item"
                /// </summary>
                public double CesAmt { get; set; }
                /// <summary>
                /// "Cess Non-Advol Amount"
                /// </summary>
                public double CesNonAdvlAmt { get; set; }
                /// <summary>
                /// "State CESS Rate"
                /// </summary>
                public double StateCesRt { get; set; }
                /// <summary>
                /// "State CESS Amount"
                /// </summary>
                public double StateCesAmt { get; set; }
                /// <summary>
                ///  "State CESS Non Adval Amount"
                /// </summary>
                public double StateCesNonAdvlAmt { get; set; }
                /// <summary>
                /// "Other Charges"
                /// </summary>
                public double OthChrg { get; set; }
                /// <summary>
                ///TotItemVal "Total Item Value = Assessable Amount + CGST Amt + SGST Amt + Cess Amt + CesNonAdvlAmt + StateCesAmt + StateCesNonAdvlAmt+Otherchrg"
                /// </summary>
                public double TotItemVal { get; set; }
                /// <summary>
                /// "Order line referencee"
                /// </summary>
                public string OrdLineRef { get; set; }
                /// <summary>
                /// "Orgin Country"
                /// </summary>
                public string OrgCntry { get; set; }
                /// <summary>
                /// "Serial number in case of each item having a unique number."
                /// </summary>
                public string PrdSlNo { get; set; }
                public List<AttributeDtls> AttribDtls { get; set; }
                public class AttributeDtls
                {
                    /// <summary>
                    /// "Attribute details of the item"
                    /// </summary>
                    public string Nm { get; set; }
                    /// <summary>
                    /// "Attribute value of the item"
                    /// </summary>
                    public string Val { get; set; }
                }
            }

            public class ShippedDetails
            {
                /// <summary>
                ///"GSTIN of entity to whom goods are shipped"
                /// </summary>
                public string Gstin { get; set; }
                /// <summary>
                ///Required Parameter - "Legal Name"
                /// </summary>
                public string LglNm { get; set; }
                /// <summary>
                /// "Tradename"
                /// </summary>
                public string TrdNm { get; set; }
                /// <summary>
                ///Required Parameter - "Building/Flat no, Road/Street"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                //public string Bno { get; set; }
                //public string Bnm { get; set; }
                //public string Flno { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                //public string Dst { get; set; }
                /// <summary>
                ///Required Parameter - "Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///Required Parameter - "State Code to which supplies are shipped to."
                /// </summary>
                public string Stcd { get; set; }
                public string State { get; set; }
            }
            public class RefDetails
            {
                /// <summary>
                /// "Remarks/Note"
                /// </summary>
                public string InvRmk { get; set; }
                /// <summary>
                ///Required Parameter - "Invoice Period Start Date"
                /// </summary>
                public string InvStDt { get; set; }
                /// <summary>
                ///Required Parameter - "Invoice Period End Date"
                /// </summary>
                public string InvEndDt { get; set; }
                public PrecDocument PrecDocDtls { get; set; }
                public class PrecDocument
                {
                    /// <summary>
                    ///Required Parameter - "Reference of original invoice, if any."
                    /// </summary>
                    public string InvNo { get; set; }
                    /// <summary>
                    ///Required Parameter - "Date of preceding invoice"
                    /// </summary>
                    public string InvDt { get; set; }
                    /// <summary>
                    /// "Other Reference"
                    /// </summary>
                    public string OthRefNo { get; set; }
                }
                public List<Contract> ContrDtls { get; set; }
                public class Contract
                {
                    /// <summary>
                    /// "Receipt Advice No."
                    /// </summary>
                    public string RecAdvDt { get; set; }
                    /// <summary>
                    /// "Date of receipt advice"
                    /// </summary>
                    public string RecAdvRefr { get; set; }
                    /// <summary>
                    /// "Lot/Batch Reference No."
                    /// </summary>
                    public string TendRefr { get; set; }
                    /// <summary>
                    /// "Contract Reference Number"
                    /// </summary>
                    public string ContrRefr { get; set; }
                    /// <summary>
                    ///  "Any other reference"
                    /// </summary>
                    public string ExtRefr { get; set; }
                    /// <summary>
                    /// "Project Reference Number"
                    /// </summary>
                    public string ProjRefr { get; set; }
                    /// <summary>
                    /// "Vendor PO Reference Number"
                    /// </summary>
                    public string PORefr { get; set; }
                    /// <summary>
                    /// "Vendor PO Reference date"
                    /// </summary>
                    public string PORefDt { get; set; }

                }
            }
            public class PayDetails
            {
                /// <summary>
                ///  "Payee Name"
                /// </summary>
                public string Nm { get; set; }
                /// <summary>
                /// "Bank account number of payee"
                /// </summary>
                public string AcctDet { get; set; }
                /// <summary>
                /// "Mode of Payment: Cash, Credit, Direct Transfer"
                /// </summary>
                public string Mode { get; set; }
                /// <summary>
                /// "Branch or IFSC code"
                /// </summary>
                public string FinInsBr { get; set; }
                /// <summary>
                /// "Credit Transfer"
                /// </summary>
                public string CrTrn { get; set; }
                /// <summary>
                /// "Payment Instruction"
                /// </summary>
                public string PayInstr { get; set; }
                /// <summary>
                /// "Terms of Payment"
                /// </summary>
                public string PayTerm { get; set; }
                /// <summary>
                /// "Direct Debit"
                /// </summary>
                public string DirDr { get; set; }
                /// <summary>
                /// "Credit Days"
                /// </summary>
                public int CrDay { get; set; }
                /// <summary>
                /// "The sum of amount which have been paid in advance."
                /// </summary>
                public double PaidAmt { get; set; }
                /// <summary>
                /// "Outstanding amount that is required to be paid."
                /// </summary>
                public double PaymtDue { get; set; }
            }
            public class ValDetails
            {
                /// <summary>
                ///Required Parameter - "Total Assessable value of all items"
                /// </summary>
                public Nullable<double> AssVal { get; set; }
                /// <summary>
                /// "Total CGST value of all items"
                /// </summary>
                public Nullable<double> CgstVal { get; set; }
                /// <summary>
                /// "Total SGST value of all items"
                /// </summary>
                public Nullable<double> SgstVal { get; set; }
                /// <summary>
                /// "Total IGST value of all items"
                /// </summary>
                public Nullable<double> IgstVal { get; set; }
                /// <summary>
                /// "Total CESS value of all items"
                /// </summary>
                public Nullable<double> CesVal { get; set; }
                //public Nullable<double> CesNonAdVal { get; set; }
                /// <summary>
                /// "Total State CESS value of all items"
                /// </summary>
                public Nullable<double> StCesVal { get; set; }
                /// <summary>
                /// "Rounded off amount"
                /// </summary>
                public Nullable<double> RndOffAmt { get; set; }
                /// <summary>
                ///Required Parameter - "Final Invoice value "
                /// </summary>
                public Nullable<double> TotInvVal { get; set; }
                /// <summary>
                /// "Final Invoice value in Additional Currency"
                /// </summary>
                public Nullable<double> TotInvValFc { get; set; }
            }
            public class SellerDetails
            {
                /// <summary>
                ///Required Parameter - "GSTIN"
                /// </summary>
                public string Gstin { get; set; }
                /// <summary>
                ///Required Parameter - "Legal Name"
                /// </summary>
                public string LglNm { get; set; }
                /// <summary>
                /// "Tradename"
                /// </summary>
                public string TrdNm { get; set; }
                /// <summary>
                ///Required Parameter - "Building/Flat no, Road/Street"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                //public string Bno { get; set; }
                //public string Bnm { get; set; }
                //public string Flno { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                //public string Dst { get; set; }
                /// <summary>
                ///Required Parameter - "Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///Required Parameter - "State Name"
                /// </summary>
                public string State { get; set; }
                public string Stcd { get; set; }
                /// <summary>
                /// "Phone or Mobile No."
                /// </summary>
                public string Ph { get; set; }
                /// <summary>
                /// "Email-Id"
                /// </summary>
                public string Em { get; set; }
            }
            public class BuyerDetails
            {
                /// <summary>
                ///Required Parameter - "GSTIN"
                /// </summary>
                public string Gstin { get; set; }
                /// <summary>
                ///Required Parameter - "Legal Name"
                /// </summary>
                public string LglNm { get; set; }
                /// <summary>
                /// "Tradename"
                /// </summary>
                public string TrdNm { get; set; }

                /// <summary>
                ///Required Parameter - "State code of Place of supply. If POS lies outside the country, a the code shall be 96."
                /// </summary>
                public string Pos { get; set; }
                /// <summary>
                ///Required Parameter - "Building/Flat no, Road/Street"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the supplier (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                //public string Bno { get; set; }
                //public string Bnm { get; set; }
                //public string Flno { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                //public string Dst { get; set; }
                /// <summary>
                ///"Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///"State Name"
                /// </summary>
                public string State { get; set; }
                public string Stcd { get; set; }
                /// <summary>
                /// "Phone or Mobile No."
                /// </summary>
                public string Ph { get; set; }
                /// <summary>
                /// "Email-Id"
                /// </summary>
                public string Em { get; set; }
            }
            public class ExpDetails
            {
                /// <summary>
                /// "Shipping Bill No."
                /// </summary>
                public string ShipBNo { get; set; }
                /// <summary>
                ///  "Shipping Bill Date"
                /// </summary>
                public string ShipBDt { get; set; }
                /// <summary>
                /// "Country Code"
                /// </summary>
                public string CntCode { get; set; }
                /// <summary>
                /// "Additional Currency Code"
                /// </summary>
                public string ForCur { get; set; }
                public Nullable<double> InvForCur { get; set; }
                /// <summary>
                /// "Port Code"
                /// </summary>
                public string Port { get; set; }
                /// <summary>
                ///  "Options for supplier for refund. Y/N"
                /// </summary>
                public string RefClm { get; set; }
            }
            public class DocSetails
            {
                /// <summary>
                /// Required Parameter - "description": "Document Type: INVOICE, CREDIT NOTE, DEBIT NOTE"
                /// </summary>
                public string Typ { get; set; }
                /// <summary>
                ///Required Parameter - "Document Number"
                /// </summary>
                public string No { get; set; }
                /// <summary>
                ///Required Parameter - "Document Date"
                /// </summary>
                public string Dt { get; set; } //
                                               //public string OrgInvNo { get; set; }
            }
            public class TranDetails
            {
                /// <summary>
                /// Required Parameter - "GST- Goods and Services Tax Scheme"
                /// </summary>
                public string TaxSch { get; set; }
                /// <summary>
                /// Required Parameter - supply Type
                /// </summary>
                public string SupTyp { get; set; }
                //public string Catg { get; set; }
                /// <summary>
                /// "Y- whether the tax liability is payable under reverse charge"
                /// </summary>
                public string RegRev { get; set; }
                //public string Typ { get; set; }
                //public string EcmTrn { get; set; }
                /// <summary>
                /// "GSTIN of e-Commerce operator"
                /// </summary>
                public string EcmGstin { get; set; }
                //public bool EcmTrnSel { get; set; }
            }
            public class DispatchedDetails
            {
                /// <summary>
                ///Required Parameter - "Name of the company from which the goods are dispatched"
                /// </summary>
                public string Nm { get; set; }
                /// <summary>
                ///Required Parameter - "Address 1 of the entity from which goods are dispatched.(Building/Flat no.Road/Street etc.)"
                /// </summary>
                public string Addr1 { get; set; }
                /// <summary>
                /// "Address 2 of the entity from which goods are dispatched. (Floor no., Name of the premises/building)"
                /// </summary>
                public string Addr2 { get; set; }
                /// <summary>
                ///Required Parameter - "Location"
                /// </summary>
                public string Loc { get; set; }
                /// <summary>
                ///Required Parameter - "Pincode"
                /// </summary>
                public int Pin { get; set; }
                /// <summary>
                ///Required Parameter - "State Code"
                /// </summary>
                public string Stcd { get; set; }
                public string State { get; set; }
            }
        }
        public class EInvoiceBase
        {
            public string gstin = "34AACCC1596Q002";
            public string username = "TaxProEnvPON";
            public string eInvPwd = "abc34*";
            public string accesskey = "";
            public bool issandbox = false;
        }
        public class InvoiceRequest : EInvoiceBase
        {
            public EInvoiceRequest Data { get; set; }
        }



        public class EWayBase
        {
            public string gstin = "34AACCC1596Q002";
            public string username = "TaxProEnvPON";
            public string ewbpwd = "abc34*";
            public string accesskey = "";
            public bool issandbox = false;
        }

        public class EWAYBillDetail
        {
            public long ewbNo { get; set; }
            public DateTime ewayBillDate { get; set; }
            public string genMode { get; set; }
            public string userGstin { get; set; }
            public string supplyType { get; set; }
            public string subSupplyType { get; set; }
            public string docType { get; set; }
            public string docNo { get; set; }
            public string docDate { get; set; }
            public string fromGstin { get; set; }
            public string fromTrdName { get; set; }
            public string fromAddr1 { get; set; }
            public string fromAddr2 { get; set; }
            public string fromPlace { get; set; }
            public int fromPincode { get; set; }
            public int fromStateCode { get; set; }
            public string toGstin { get; set; }
            public string toTrdName { get; set; }
            public string toAddr1 { get; set; }
            public string toAddr2 { get; set; }
            public string toPlace { get; set; }
            public int toPincode { get; set; }
            public int toStateCode { get; set; }
            public decimal totalValue { get; set; }
            public decimal totInvValue { get; set; }
            public decimal cgstValue { get; set; }
            public decimal sgstValue { get; set; }
            public decimal igstValue { get; set; }
            public decimal cessValue { get; set; }
            public string transporterId { get; set; }
            public string transporterName { get; set; }
            public string status { get; set; }
            public int actualDist { get; set; }
            public int noValidDays { get; set; }
            public string validUpto { get; set; }
            public int extendedTimes { get; set; }
            public string rejectStatus { get; set; }
            public string vehicleType { get; set; }
            public int actFromStateCode { get; set; }
            public int actToStateCode { get; set; }
            public int transactionType { get; set; }
            public decimal otherValue { get; set; }
            public decimal cessNonAdvolValue { get; set; }
            public List<ItemList> itemList { get; set; }
            public List<VehiclListDetail> VehiclListDetails { get; set; }
            public class ItemList
            {
                public int itemNo { get; set; }
                public int productId { get; set; }
                public string productName { get; set; }
                public string productDesc { get; set; }
                public int hsnCode { get; set; }
                public decimal quantity { get; set; }
                public string qtyUnit { get; set; }
                public decimal cgstRate { get; set; }
                public decimal sgstRate { get; set; }
                public decimal igstRate { get; set; }
                public decimal cessRate { get; set; }
                public decimal cessNonAdvol { get; set; }
                public decimal taxableAmount { get; set; }

            }
            public class VehiclListDetail
            {
                public string updMode { get; set; }
                public string vehicleNo { get; set; }
                public string fromPlace { get; set; }
                public int fromState { get; set; }
                public int tripshtNo { get; set; }
                public string userGSTINTransin { get; set; }
                public DateTime enteredDate { get; set; }
                public string transMode { get; set; }
                public string transDocNo { get; set; }
                public string transDocDate { get; set; }
                public string groupNo { get; set; }

            }
        }

        public class EWAYBill
        {

            public class ItemList
            {
                public string productName { get; set; }
                public string productDesc { get; set; }
                public int hsnCode { get; set; }
                public decimal quantity { get; set; }
                public string qtyUnit { get; set; }
                public decimal cgstRate { get; set; }
                public decimal sgstRate { get; set; }
                public decimal igstRate { get; set; }
                public decimal cessRate { get; set; }
                public decimal cessNonAdvol { get; set; }
                public decimal taxableAmount { get; set; }

            }
            public string supplyType { get; set; }
            public string subSupplyType { get; set; }
            public string subSupplyDesc { get; set; }
            public string docType { get; set; }
            public string docNo { get; set; }
            public string docDate { get; set; }
            public string fromGstin { get; set; }
            public string fromTrdName { get; set; }
            public string fromAddr1 { get; set; }
            public string fromAddr2 { get; set; }
            public string fromPlace { get; set; }
            public int fromPincode { get; set; }
            public string actFromStateCode { get; set; }
            public string fromStateCode { get; set; }
            public string toGstin { get; set; }
            public string toTrdName { get; set; }
            public string toAddr1 { get; set; }
            public string toAddr2 { get; set; }
            public string toPlace { get; set; }
            public int toPincode { get; set; }
            public string actToStateCode { get; set; }
            public string toStateCode { get; set; }
            public int transactionType { get; set; }
            public string dispatchFromGSTIN { get; set; }
            public string dispatchFromTradeName { get; set; }
            public string shipToGSTIN { get; set; }
            public string shipToTradeName { get; set; }
            public decimal otherValue { get; set; }
            public decimal totalValue { get; set; }
            public decimal cgstValue { get; set; }
            public decimal sgstValue { get; set; }
            public decimal igstValue { get; set; }
            public decimal cessValue { get; set; }
            public decimal cessNonAdvolValue { get; set; }
            public decimal totInvValue { get; set; }
            public string transporterId { get; set; }
            public string transporterName { get; set; }
            public string transDocNo { get; set; }
            public string transMode { get; set; }
            public string transDistance { get; set; }
            public string transDocDate { get; set; }
            public string vehicleNo { get; set; }
            public string vehicleType { get; set; }
            public List<ItemList> itemList { get; set; }
        }

        public class EWAYbillResponse
        {
            public long ewayBillNo { get; set; }
            public DateTime ewayBillDate { get; set; }
            public string validUpto { get; set; }
            public string alert { get; set; }
            public string raw { get; set; }
            [JsonProperty("ErrorDetails")]
            public List<ErrorDetail> ErrorDetails { get; set; }
            public class ErrorDetail
            {
                [JsonProperty("ErrorCode")]
                public long ErrorCode { get; set; }

                [JsonProperty("ErrorMessage")]
                public string ErrorMessage { get; set; }
            }
        }


        public class EWayRequest : EWayBase
        {
            public EWAYBill Data { get; set; }
        }


        public class EWayFromIRNRequest : EInvoiceBase
        {
            public Irn2EwayRequest Data { get; set; }
        }

        public class CancellationRequest : EInvoiceBase
        {
            public string irn { get; set; }
            public long ewaybillno { get; set; }
        }

        public partial class Irn2EwayRequest
        {
            [JsonProperty("Irn")]
            public string Irn { get; set; }

            [JsonProperty("Distance")]
            public long Distance { get; set; }

            [JsonProperty("TransMode")]
            //Road-1, Rail-2, Air-3, Ship-4
            public string TransMode { get; set; }

            [JsonProperty("TransId")]
            public string TransId { get; set; }

            [JsonProperty("TransName")]
            public string TransName { get; set; }

            [JsonProperty("TransDocDt")]
            public string TransDocDt { get; set; }

            [JsonProperty("TransDocNo")]
            public string TransDocNo { get; set; }

            [JsonProperty("VehNo")]
            public string VehNo { get; set; }
            //"enum": ["O", "R"], "description": "Whether O-ODC or R-Regular "

            [JsonProperty("VehType")]
            public string VehType { get; set; }

            [JsonProperty("ExpShipDtls")]
            public PDtls ExpShipDtls { get; set; }

            [JsonProperty("DispDtls")]
            public PDtls DispDtls { get; set; }

            public partial class PDtls
            {
                [JsonProperty("Nm", NullValueHandling = NullValueHandling.Ignore)]
                public string Nm { get; set; }

                [JsonProperty("Addr1")]
                public string Addr1 { get; set; }

                [JsonProperty("Addr2")]
                public string Addr2 { get; set; }

                [JsonProperty("Loc")]
                public string Loc { get; set; }

                [JsonProperty("Pin")]
                public long Pin { get; set; }

                [JsonProperty("Stcd")]
                public long Stcd { get; set; }
            }
        }

        public class EwayFromGRNResponse
        {
            public class InfoDtl
            {
                public string InfCd { get; set; }
                public string Desc { get; set; }
            }
            public string Status { get; set; }
            public string Data { get; set; }
            [JsonIgnore]
            public EWayInfo Eway
            {
                get
                {
                    try
                    {
                        var response = JsonConvert.DeserializeObject<EWayInfo>(Data);
                        if (InfoDtls != null && InfoDtls.Count > 0 && InfoDtls.Exists(x => x.InfCd == "EWBPPD"))
                        {
                            response.Distance = InfoDtls.Find(x => x.InfCd == "EWBPPD").Desc.Split(':').Last();
                        }
                        return response;
                    }
                    catch (Exception exp)
                    {
                        return new EWayInfo();
                    }
                }
            }
            public List<InfoDtl> InfoDtls { get; set; }
            public List<ErrorDetail> ErrorDetails { get; set; }
            [JsonIgnore]
            public string raw { get; set; }

            public class ErrorDetail
            {
                [JsonProperty("ErrorCode")]
                public long ErrorCode { get; set; }

                [JsonProperty("ErrorMessage")]
                public string ErrorMessage { get; set; }
            }

            public class EWayInfo
            {
                public string EwbNo { get; set; }
                public string EwbDt { get; set; }
                public string EwbValidTill { get; set; }
                [JsonIgnore]
                public string Distance { get; set; }
                [JsonIgnore]
                public bool HasDistance
                {
                    get
                    {
                        return !string.IsNullOrEmpty(Distance);
                    }
                }
            }
        }



        public async Task<EwayFromGRNResponse> CancelEWay(CancellationRequest request)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/CancelEWay", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/CancelEWay", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<EwayFromGRNResponse>(json);
                    response.raw = json;
                    return response;
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CancelEWay Response/"+request.ewaybillno, WebAPI.Common.Logger.LogType.Error, json);
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CancelEWay Response/" + request.ewaybillno, WebAPI.Common.Logger.LogType.Error, serializedProduct);
                    throw new Exception(json);
                }
            }
            return new EwayFromGRNResponse();
        }

        public async Task<EwayFromGRNResponse> CancelEInvoice(CancellationRequest request)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/CancelEInvoice", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/CancelEInvoice", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<EwayFromGRNResponse>(json);
                    response.raw = json;
                    return response;
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CancelEInvoice Response/"+request.irn, WebAPI.Common.Logger.LogType.Error, json);
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CancelEInvoice Response/" + request.irn, WebAPI.Common.Logger.LogType.Error, serializedProduct);
                    throw new Exception(json);
                }
            }
            return new EwayFromGRNResponse();
        }

        public async Task<EWAYbillResponse> CreateEWay(InfyGST.GSTPortal.EWayRequest request)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEWay", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/CreateEWay", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<EWAYbillResponse>(json);
                    response.raw = json;
                    return response;
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEWay4IRN Response/" + request.Data.docNo, WebAPI.Common.Logger.LogType.Error, json);
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEWay4IRN Response/" + request.Data.docNo, WebAPI.Common.Logger.LogType.Error, serializedProduct);
                    throw new Exception(json);
                }
            }
            return new EWAYbillResponse();
        }
        public async Task<EwayFromGRNResponse> CreateEWay4IRN(InfyGST.GSTPortal.EWayFromIRNRequest request)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEWay4IRN", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/CreateEWay4IRN", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<EwayFromGRNResponse>(json);
                    response.raw = json;
                    return response;
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEWay4IRN Response/" + request.Data.TransDocNo, WebAPI.Common.Logger.LogType.Error, json);
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEWay4IRN Response/" + request.Data.TransDocNo, WebAPI.Common.Logger.LogType.Error, serializedProduct);
                    throw new Exception(json);
                }
            }
            return new EwayFromGRNResponse();
        }

        public async Task<EInvoiceResponse> CreateEInvoice(InvoiceRequest request)
        {
            ValidateClient();
            using (var client = new HttpClient())
            {
                var serializedProduct = JsonConvert.SerializeObject(request);
                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEInvoice", WebAPI.Common.Logger.LogType.Error, serializedProduct);
                var result = Task.Run(() => client.PostAsync(URI + "/CreateEInvoice", content)).Result;
                if (result.IsSuccessStatusCode)
                {
                    string json = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<EInvoiceResponse>(json);
                }
                else
                {
                    string json = await result.Content.ReadAsStringAsync();
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEInvoice Response/"+ request.Data.DocDtls.No, WebAPI.Common.Logger.LogType.Error, json);
                    WebAPI.Common.Logger.Instance(0).Log(URI + "/CreateEInvoice Response/" + request.Data.DocDtls.No, WebAPI.Common.Logger.LogType.Error, json);
                    throw new Exception(json);
                }
            }
            return new EInvoiceResponse();
        }
    }

}

