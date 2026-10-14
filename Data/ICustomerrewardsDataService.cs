using InfyPOS.Common;
using InfyPOS.Models;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Web.Helpers;

namespace InfyPOS.Data
{
    public interface ICustomerrewardsDataService
    {
        UserClaim Context { get; set; }
        bool CanGetRewardAtBilling();
        void ApplyManualReward(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill);
        void ApplyReward(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill);
        void ApplyReward(NpgsqlTransaction transaction, NpgsqlConnection conn, long customerid, decimal rewards, long locationid);
        CustomerRewardStatus SearchReward(CustomerrewardsCreteria item);
        CustomerBills UpdateRewards(CustomerBills oCustomerBills);
        bool ValidateOTP(Customer item);
    }

    public class CouponManager : BaseData
    {
        //{"engine":"Cipher","url":"https://demoza.ciphercode.ai/api/v1","username":"admin2","password":"Admin@3698","token":""}
        public class Config
        {
            public string engine { get; set; }
            public string url { get; set; }
            public string username { get; set; }
            public string password { get; set; }
            public string token { get; set; }
        }
        private static SortedDictionary<string, Config> internalList = new SortedDictionary<string, Config>();
        Config config = null;
        public CouponManager(string configuration)
        {
            //configuration = @"{""engine"":""Cipher"",""url"":""https://demoza.ciphercode.ai/api/v1"",""username"":""admin2"",""password"":""Admin@3698"",""token"":""""}";
            config = JsonConvert.DeserializeObject<Config>(configuration);
        }
        public bool SyncCouponCode(string coupon)
        {
            return false;
        }

        public bool HasCoupon(string code)
        {
            CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
            VouchersData.Context = this.Context;
            var existing = VouchersData.Search(new CustomerCouponsCreteria()
            {
                code = code
            });

            CipherApiClient.Initialize(config.url, config.username, config.password);
            var couponresult = CipherApiClient.VerifyCoupon(code);

            // && couponresult.coupon_owner_mapped == "no"
            if (couponresult != null &&
                couponresult.Coupon_status == "Not Redeemed Yet")
            {
                if (existing.Count == 0)
                {
                    VouchersData.Insert(new CustomerCoupons()
                    {
                        isactive=true,
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedby = Context.UserID,
                        modifiedon = DateTimeNow,
                        code = code,
                        vouchertype = CustomerCoupons.CouponType.Coupon,
                        type = 2,
                        customername = couponresult.user_data != null ? couponresult.user_data.name : "",
                        mobileno = couponresult.user_data != null ? couponresult.user_data.mobilenumber : "",
                        amount = couponresult.Value,
                        minbillvalue = couponresult.Min_bill_value,
                        issuedon = couponresult.Coupon_generated_date,
                        validfrom = couponresult.Coupon_generated_date,
                        validupto = couponresult.Coupon_expiry_date
                    });
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        internal void Redim(string couponcode,string billno)
        {
            CipherApiClient.Initialize(config.url, config.username, config.password);
            var couponresult = CipherApiClient.RedeemCoupon(couponcode, billno);
        }
    }

    public static class CipherApiClient
    {
        private static string _baseUrl;
        private static string _username;
        private static string _password;

        private static string _token;
        private static DateTime _tokenExpiry;

        private static readonly object _lock = new object();

        static CipherApiClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.Expect100Continue = false;
        }

        #region INIT

        public static void Initialize(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl.TrimEnd('/') + "/";
            _username = username;
            _password = password;
        }

        #endregion

        #region CORE SEND (SYNC STYLE)

        private static HttpResponseMessage Send(HttpRequestMessage request)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30);

                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                request.Headers.UserAgent.Clear();
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0");

                request.Headers.Connection.Clear();
                request.Headers.Connection.Add("keep-alive");

                if (!request.RequestUri.AbsolutePath.Contains("/auth/login"))
                {
                    request.Headers.Remove("Authorization");
                    request.Headers.Add("Authorization", $"Token {_token}");
                }

                var response = Task.Run(() =>
                    client.SendAsync(request, HttpCompletionOption.ResponseContentRead)
                ).Result;

                return response;
            }
        }

        #endregion

        #region AUTH (SYNC)

        private static void EnsureToken()
        {
            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
                return;

            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
                    return;

                using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "auth/login/"))
                {
                    request.Headers.Add("accept", "application/json");

                    request.Content = new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            username = _username,
                            password = _password
                        }),
                        Encoding.UTF8,
                        "application/json");

                    var response = Send(request);

                    var json = Task.Run(() =>
                        response.Content.ReadAsStringAsync()
                    ).Result;

                    if (!response.IsSuccessStatusCode)
                        throw new Exception("Login failed: " + json);

                    var result = JsonConvert.DeserializeObject<LoginResponse>(json);

                    _token = result.Token;
                    _tokenExpiry = DateTime.UtcNow.AddHours(result.ExpiresInHours - 0.1);
                }
            }
        }

        #endregion

        #region GENERIC POST (SYNC)

        private static T Post<T>(string url, object requestData)
        {
            EnsureToken();

            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url))
            {
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(requestData),
                    Encoding.UTF8,
                    "application/json");

                var response = Send(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _token = null;
                    EnsureToken();

                    using (var retryRequest = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url))
                    {
                        retryRequest.Content = new StringContent(
                            JsonConvert.SerializeObject(requestData),
                            Encoding.UTF8,
                            "application/json");

                        response = Send(retryRequest);
                    }
                }

                var json = Task.Run(() =>
                    response.Content.ReadAsStringAsync()
                ).Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"API Error ({url}): {json}");

                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        #endregion

        #region METHODS

        public static CouponVerificationResponse VerifyCoupon(string gvCode)
        {
            return Post<CouponVerificationResponse>(
                "coupon-verification/",
                new { gv_code = gvCode });
        }

        public static CouponRedeemResponse RedeemCoupon(string gvCode, string orderId = null)
        {
            return Post<CouponRedeemResponse>(
                "coupon-redeem/",
                new { gv_code = gvCode, order_id = orderId });
        }

        public static CouponUnlockResponse UnlockCoupon(string gvCode)
        {
            return Post<CouponUnlockResponse>(
                "coupon-unlock/",
                new { gv_code = gvCode });
        }

        #endregion

        #region MODELS

        public class LoginResponse
        {
            [JsonProperty("token")]
            public string Token { get; set; }

            [JsonProperty("expires_in_hours")]
            public double ExpiresInHours { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        public class CouponVerificationResponse
        {
            public string coupon_owner_mapped { get; set; }
            public string Units { get; set; }
            public decimal Value { get; set; }
            public decimal Min_bill_value { get; set; }
            public string Coupon_status { get; set; }
            public string Location { get; set; }
            public string Campaign_type { get; set; }
            public DateTime Coupon_expiry_date { get; set; }
            public DateTime Coupon_generated_date { get; set; }
            public UserData user_data { get; set; }
            public string message { get; set; }
        }

        public class CouponRedeemResponse
        {
            public string message { get; set; }
            public string coupon_owner_mapped { get; set; }
            public UserData user_data { get; set; }
        }

        public class CouponUnlockResponse
        {
            public string message { get; set; }
            public CouponUnlockData coupon { get; set; }
        }

        public class CouponUnlockData
        {
            public string coupon { get; set; }
            public bool used { get; set; }
            public bool is_unlocked { get; set; }
            public int unlock_count { get; set; }
        }

        public class UserData
        {
            public string name { get; set; }
            public string mobile { get; set; }
            public string mobilenumber { get; set; }
            public string email { get; set; }
            public string value { get; set; }
        }

        #endregion
    }
    //public static class CipherApiClient
    //{
    //    private static string _baseUrl;
    //    private static string _username;
    //    private static string _password;

    //    private static string _token;
    //    private static DateTime _tokenExpiry;

    //    private static readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

    //    private static readonly TimeSpan _httpTimeout = TimeSpan.FromSeconds(30);

    //    static CipherApiClient()
    //    {
    //        // ✅ MUST for IIS
    //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    //        ServicePointManager.DefaultConnectionLimit = 100;
    //        ServicePointManager.Expect100Continue = false;
    //    }

    //    #region INIT

    //    public static void Initialize(string baseUrl, string username, string password)
    //    {
    //        _baseUrl = baseUrl.TrimEnd('/') + "/";
    //        _username = username;
    //        _password = password;
    //    }

    //    #endregion

    //    #region HTTP CLIENT (STATIC)

    //    private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
    //    {
    //        UseCookies = false, // ✅ avoid cross-request cookie issues
    //        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    //    })
    //    {
    //        Timeout = Timeout.InfiniteTimeSpan // ✅ use only CTS timeout
    //    };

    //    #endregion

    //    #region CORE SEND

    //    private static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    //    {

    //        using (var cts = new CancellationTokenSource(_httpTimeout))

    //        {
    //            try
    //            {

    //                request.Headers.Accept.Clear();
    //                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    //                request.Headers.UserAgent.Clear();
    //                request.Headers.UserAgent.ParseAdd("Mozilla/5.0");

    //                request.Headers.Connection.Clear();
    //                request.Headers.Connection.Add("keep-alive");

    //                if (!request.RequestUri.AbsolutePath.Contains("/auth/login"))
    //                {
    //                    request.Headers.Remove("Authorization");
    //                    request.Headers.Add("Authorization", $"Token {_token}");
    //                }

    //                var response = await _httpClient.SendAsync(
    //                    request,
    //                    HttpCompletionOption.ResponseContentRead,
    //                    cts.Token
    //                );

    //                return response;
    //            }
    //            catch (TaskCanceledException)
    //            {
    //                throw new TimeoutException($"HTTP timeout after {_httpTimeout.TotalSeconds} sec");
    //            }
    //        }
    //    }

    //    #endregion

    //    #region AUTH

    //    private static async Task EnsureTokenAsync()
    //    {
    //        if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
    //            return;

    //        await _tokenLock.WaitAsync(100);

    //        try
    //        {
    //            // double-check after lock
    //            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
    //                return;

    //            using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "auth/login/"))
    //            {
    //                request.Headers.Add("accept", "application/json");

    //                request.Content = new StringContent(
    //                    JsonConvert.SerializeObject(new
    //                    {
    //                        username = _username,
    //                        password = _password
    //                    }),
    //                    Encoding.UTF8,
    //                    "application/json");

    //                var response = await SendAsync(request);

    //                var json = await response.Content.ReadAsStringAsync();

    //                if (!response.IsSuccessStatusCode)
    //                    throw new Exception("Login failed: " + json);

    //                var result = JsonConvert.DeserializeObject<LoginResponse>(json);

    //                _token = result.Token;
    //                _tokenExpiry = DateTime.UtcNow.AddHours(result.ExpiresInHours - 0.1);
    //            }
    //        }
    //        finally
    //        {
    //            _tokenLock.Release();
    //        }
    //    }

    //    #endregion

    //    #region GENERIC POST

    //    private static async Task<T> PostAsync<T>(string url, object requestData)
    //    {
    //        await EnsureTokenAsync();

    //        using (var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url))
    //        {
    //            request.Content = new StringContent(
    //                JsonConvert.SerializeObject(requestData),
    //                Encoding.UTF8,
    //                "application/json");

    //            var response = await SendAsync(request);

    //            // Retry once on 401
    //            if (response.StatusCode == HttpStatusCode.Unauthorized)
    //            {
    //                _token = null;

    //                await EnsureTokenAsync();

    //                using (var retryRequest = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url))
    //                {
    //                    retryRequest.Content = new StringContent(
    //                        JsonConvert.SerializeObject(requestData),
    //                        Encoding.UTF8,
    //                        "application/json");

    //                    response = await SendAsync(retryRequest);
    //                }
    //            }

    //            var json = await response.Content.ReadAsStringAsync();

    //            if (!response.IsSuccessStatusCode)
    //                throw new Exception($"API Error ({url}): {json}");

    //            return JsonConvert.DeserializeObject<T>(json);
    //        }
    //    }

    //    #endregion

    //    #region METHODS

    //    public static Task<CouponVerificationResponse> VerifyCouponAsync(string gvCode)
    //    {
    //        return PostAsync<CouponVerificationResponse>(
    //            "coupon-verification/",
    //            new { gv_code = gvCode });
    //    }

    //    public static Task<CouponRedeemResponse> RedeemCouponAsync(string gvCode, string orderId = null)
    //    {
    //        return PostAsync<CouponRedeemResponse>(
    //            "coupon-redeem/",
    //            new { gv_code = gvCode, order_id = orderId });
    //    }

    //    public static Task<CouponUnlockResponse> UnlockCouponAsync(string gvCode)
    //    {
    //        return PostAsync<CouponUnlockResponse>(
    //            "coupon-unlock/",
    //            new { gv_code = gvCode });
    //    }

    //    #endregion

    //    #region MODELS

    //    public class LoginResponse
    //    {
    //        [JsonProperty("token")]
    //        public string Token { get; set; }

    //        [JsonProperty("expires_in_hours")]
    //        public double ExpiresInHours { get; set; }

    //        [JsonProperty("message")]
    //        public string Message { get; set; }
    //    }

    //    public class CouponVerificationResponse
    //    {
    //        public string coupon_owner_mapped { get; set; }
    //        public string Units { get; set; }
    //        public decimal Value { get; set; }
    //        public decimal Min_bill_value { get; set; }
    //        public string Coupon_status { get; set; }
    //        public string Location { get; set; }
    //        public string Campaign_type { get; set; }
    //        public DateTime Coupon_expiry_date { get; set; }
    //        public DateTime Coupon_generated_date { get; set; }
    //        public UserData user_data { get; set; }
    //        public string message { get; set; }
    //    }

    //    public class CouponRedeemResponse
    //    {
    //        public string message { get; set; }
    //        public string coupon_owner_mapped { get; set; }
    //        public UserData user_data { get; set; }
    //    }

    //    public class CouponUnlockResponse
    //    {
    //        public string message { get; set; }
    //        public CouponUnlockData coupon { get; set; }
    //    }

    //    public class CouponUnlockData
    //    {
    //        public string coupon { get; set; }
    //        public bool used { get; set; }
    //        public bool is_unlocked { get; set; }
    //        public int unlock_count { get; set; }
    //    }

    //    public class UserData
    //    {
    //        public string name { get; set; }
    //        public string mobile { get; set; }
    //        public string mobilenumber { get; set; }
    //        public string email { get; set; }
    //        public string value { get; set; }
    //    }

    //    #endregion
    //}

    //public static class CipherApiClient
    //{

    //    private static string _csrfToken = "0DLd9Cjuo8q3nqxmFhPepguQYxWTpwDuNtvZlIkcGEF8qxFi7ETcTfNT5l4hLIYZ";
    //    private static string _baseUrl;
    //    private static string _username;
    //    private static string _password;

    //    private static string _token;
    //    private static DateTime _tokenExpiry;


    //    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);
    //    private static readonly TimeSpan _httpTimeout = TimeSpan.FromSeconds(30);

    //    static CipherApiClient()
    //    {

    //    }

    //    #region INIT

    //    public static void Initialize(string baseUrl, string username, string password)
    //    {
    //        _baseUrl = baseUrl.TrimEnd('/') + "/";
    //        _username = username;
    //        _password = password;
    //    }

    //    #endregion

    //    #region CORE HELPERS

    //    public static List<Cookie> GetAllCookies(CookieContainer cookieContainer)
    //    {
    //        var cookies = new List<Cookie>();

    //        var table = (System.Collections.Hashtable)cookieContainer
    //            .GetType()
    //            .InvokeMember("m_domainTable",
    //                System.Reflection.BindingFlags.NonPublic |
    //                System.Reflection.BindingFlags.GetField |
    //                System.Reflection.BindingFlags.Instance,
    //                null,
    //                cookieContainer,
    //                new object[] { });

    //        foreach (var key in table.Keys)
    //        {
    //            var domain = key as string;

    //            var pathList = (System.Collections.IDictionary)table[key]
    //                .GetType()
    //                .InvokeMember("m_list",
    //                    System.Reflection.BindingFlags.NonPublic |
    //                    System.Reflection.BindingFlags.GetField |
    //                    System.Reflection.BindingFlags.Instance,
    //                    null,
    //                    table[key],
    //                    new object[] { });

    //            foreach (var pathKey in pathList.Keys)
    //            {
    //                var cookieCollection = pathList[pathKey] as CookieCollection;

    //                foreach (Cookie cookie in cookieCollection)
    //                {
    //                    cookies.Add(cookie);
    //                }
    //            }
    //        }

    //        return cookies;
    //    }

    //    private static readonly HttpClientHandler _handler = new HttpClientHandler
    //    {
    //        UseCookies = true,
    //        CookieContainer = new CookieContainer(),
    //        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    //    };

    //    private static readonly HttpClient _httpClient = new HttpClient(_handler)
    //    {
    //        Timeout = TimeSpan.FromSeconds(30) // or your _httpTimeout
    //    };

    //    private static async Task<HttpResponseMessage> SendWithTimeoutAsync(HttpRequestMessage request)
    //    {
    //        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
    //        System.Net.ServicePointManager.Expect100Continue = false;
    //        using (var cts = new CancellationTokenSource(_httpTimeout))
    //        {
    //            try
    //            {
    //                if (!request.RequestUri.AbsoluteUri.Contains("login"))
    //                {
    //                    request.Headers.Remove("Authorization");
    //                    request.Headers.Add("Authorization", $"Token {_token}");
    //                }

    //                var response = await _httpClient.SendAsync(
    //                    request,
    //                    HttpCompletionOption.ResponseHeadersRead,
    //                    cts.Token
    //                );

    //                return response;
    //            }
    //            catch (TaskCanceledException)
    //            {
    //                throw new TimeoutException($"HTTP request timeout after {_httpTimeout.TotalSeconds} seconds.");
    //            }
    //        }
    //    }

    //    #endregion

    //    #region CSRF


    //    #endregion

    //    #region AUTH


    //    private static async Task<string> GetCsrfAndSessionAsync()
    //    {
    //        var url = "https://demoza.ciphercode.ai/";
    //        CookieContainer _cookies = new CookieContainer();
    //        HttpClientHandler _handler = new HttpClientHandler
    //        {
    //            UseCookies = true,
    //            CookieContainer = _cookies,
    //            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
    //        };
    //        HttpClient _httpClient = new HttpClient(_handler);
    //        _httpClient.Timeout = _httpTimeout;
    //        var request = new HttpRequestMessage(HttpMethod.Get, url);

    //        request.Headers.Add("User-Agent", "Mozilla/5.0");
    //        request.Headers.Add("Accept", "text/html");

    //        var response = await _httpClient.SendAsync(request);

    //        response.EnsureSuccessStatusCode();

    //        var cookies = _cookies.GetCookies(new Uri(url));

    //        string csrf = null;

    //        foreach (Cookie c in cookies)
    //        {
    //            Console.WriteLine($"{c.Name}={c.Value}");

    //            if (c.Name == "csrftoken")
    //                csrf = c.Value;
    //        }

    //        if (string.IsNullOrEmpty(csrf))
    //            throw new Exception("CSRF token not found");

    //        return csrf;
    //    }

    //    private static async Task EnsureTokenAsync()
    //    {
    //        if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
    //            return;

    //        //if (!await _lock.WaitAsync(_lockTimeout))
    //        //    throw new TimeoutException("Token lock acquisition timeout.");

    //        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

    //        try
    //        {
    //            if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiry)
    //                return;

    //            //_csrfToken = GetCsrfAndSessionAsync().Result;

    //            var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + "auth/login/");
    //            request.Headers.Add("accept", "application/json");
    //            //request.Headers.Add("X-CSRFToken", _csrfToken);

    //            request.Content = new StringContent(
    //                JsonConvert.SerializeObject(new
    //                {
    //                    username = _username,
    //                    password = _password
    //                }),
    //                Encoding.UTF8,
    //                "application/json");

    //            var response = await SendWithTimeoutAsync(request);

    //            var json = await response.Content.ReadAsStringAsync();

    //            if (!response.IsSuccessStatusCode)
    //                throw new Exception("Login failed: " + json);

    //            var result = JsonConvert.DeserializeObject<LoginResponse>(json);

    //            _token = result.Token;
    //            _tokenExpiry = DateTime.UtcNow.AddHours(result.ExpiresInHours - 0.1);               
    //        }
    //        finally
    //        {

    //        }
    //    }

    //    #endregion

    //    #region GENERIC POST

    //    private static async Task<T> PostAsync<T>(string url, object requestData)
    //    {
    //        await EnsureTokenAsync();

    //        var request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url);
    //        //request.Headers.Add("X-CSRFToken", _csrfToken);

    //        request.Content = new StringContent(
    //            JsonConvert.SerializeObject(requestData),
    //            Encoding.UTF8,
    //            "application/json");

    //        var response = await SendWithTimeoutAsync(request);

    //        // Retry on 401
    //        if (response.StatusCode == HttpStatusCode.Unauthorized)
    //        {
    //            _token = null;

    //            await EnsureTokenAsync();

    //            request = new HttpRequestMessage(HttpMethod.Post, _baseUrl + url);
    //            //request.Headers.Add("X-CSRFToken", _csrfToken);
    //            request.Content = new StringContent(
    //                JsonConvert.SerializeObject(requestData),
    //                Encoding.UTF8,
    //                "application/json");

    //            response = await SendWithTimeoutAsync(request);
    //        }

    //        var json = await response.Content.ReadAsStringAsync();

    //        if (!response.IsSuccessStatusCode)
    //            throw new Exception($"API Error ({url}): {json}");

    //        return JsonConvert.DeserializeObject<T>(json);
    //    }

    //    #endregion

    //    #region METHODS

    //    public static Task<CouponVerificationResponse> VerifyCouponAsync(string gvCode)
    //    {
    //        return PostAsync<CouponVerificationResponse>(
    //            "coupon-verification/",
    //            new { gv_code = gvCode });
    //    }

    //    public static Task<CouponRedeemResponse> RedeemCouponAsync(string gvCode, string orderId = null)
    //    {
    //        return PostAsync<CouponRedeemResponse>(
    //            "coupon-redeem/",
    //            new { gv_code = gvCode, order_id = orderId });
    //    }

    //    public static Task<CouponUnlockResponse> UnlockCouponAsync(string gvCode)
    //    {
    //        return PostAsync<CouponUnlockResponse>(
    //            "coupon-unlock/",
    //            new { gv_code = gvCode });
    //    }

    //    #endregion

    //    #region MODELS

    //    public class LoginResponse
    //    {
    //        [JsonProperty("token")]
    //        public string Token { get; set; }

    //        [JsonProperty("expires_in_hours")]
    //        public double ExpiresInHours { get; set; }

    //        [JsonProperty("message")]
    //        public string Message { get; set; }
    //    }

    //    public class CouponVerificationResponse
    //    {
    //        public string coupon_owner_mapped { get; set; }
    //        public string Units { get; set; }
    //        public decimal Value { get; set; }
    //        public decimal Min_bill_value { get; set; }
    //        public string Coupon_status { get; set; }
    //        public string Location { get; set; }
    //        public string Campaign_type { get; set; }
    //        public DateTime Coupon_expiry_date { get; set; }
    //        public DateTime Coupon_generated_date { get; set; }
    //        public UserData user_data { get; set; }
    //        public string message { get; set; }
    //    }

    //    public class CouponRedeemResponse
    //    {
    //        public string message { get; set; }
    //        public string coupon_owner_mapped { get; set; }
    //        public UserData user_data { get; set; }
    //    }

    //    public class CouponUnlockResponse
    //    {
    //        public string message { get; set; }
    //        public CouponUnlockData coupon { get; set; }
    //    }

    //    public class CouponUnlockData
    //    {
    //        public string coupon { get; set; }
    //        public bool used { get; set; }
    //        public bool is_unlocked { get; set; }
    //        public int unlock_count { get; set; }
    //    }

    //    public class UserData
    //    {
    //        public string name { get; set; }
    //        public string mobile { get; set; }
    //        public string mobilenumber { get; set; }
    //        public string email { get; set; }
    //        public string value { get; set; }
    //    }

    //    #endregion
    //}
    public class BrightMobile : ICustomerrewardsDataService
    {
        public bool CanGetRewardAtBilling()
        {
            return false;
        }
        public UserClaim Context { get; set; }
        public void ApplyManualReward(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyReward(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            throw new System.NotImplementedException();
        }

        public void ApplyReward(NpgsqlTransaction transaction, NpgsqlConnection conn, long customerid, decimal rewards, long locationid)
        {
            throw new System.NotImplementedException();
        }

        public CustomerRewardStatus SearchReward(CustomerrewardsCreteria item)
        {
            var scheduleList = InfyPOS.Processors.Schedule_Actions.Instance.GetSchedule(Context.OrganizationID,
                new Processors.Schedule_Actions.QueueItem()
                {
                    action = Processors.Schedule_Actions.Actions.CRM_Loyelty,
                    type = Processors.Schedule_Actions.SourceType.Customer
                });

            var schedule = scheduleList.Values.ToList().Find(x => x.config.output == "WebAPI");
            string json = @"{""data"": {""cust_num"": ""[MOBILENO]""}}";
            if (!string.IsNullOrEmpty(schedule.config.format))
            {
                json = schedule.config.format;
            }
            json = json.Replace("[MOBILENO]", item.customer.mobileno);
            json = json.Replace("[BILLVALUE]", item.billvalue.ToString("F0"));

            LoyeltyResponse loyeltyResponse = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(schedule.config.authentication))
                    {
                        if (schedule.config.authentication.StartsWith(@"Bearer "))
                        {
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", schedule.config.authentication.Substring("Bearer ".Length));
                        }
                        else
                        {
                            var byteArray = Encoding.ASCII.GetBytes(schedule.config.authentication);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        }
                    }
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = Task.Run(() => client.PostAsync(schedule.config.url, content)).Result;
                    var response = result.Content.ReadAsStringAsync().Result;
                    loyeltyResponse = JsonConvert.DeserializeObject<LoyeltyResponse>(response);
                }
            }
            catch (Exception exp)
            {
                return new CustomerRewardStatus()
                {
                    error = exp.Message
                };
            }
            return new CustomerRewardStatus()
            {
                available = loyeltyResponse == null ? 0 : loyeltyResponse.result.success.LoyaltyValue,
                convertable = loyeltyResponse == null ? 0 : loyeltyResponse.result.success.LoyaltyValue,
                rewards = loyeltyResponse == null ? 0 : loyeltyResponse.result.success.LoyaltyPoints,
            };
        }

        public bool ValidateOTP(Customer item)
        {
            var scheduleList = InfyPOS.Processors.Schedule_Actions.Instance.GetSchedule(Context.OrganizationID,
                new Processors.Schedule_Actions.QueueItem()
                {
                    action = Processors.Schedule_Actions.Actions.Otp_Loyelty_Validate,
                    type = Processors.Schedule_Actions.SourceType.OTP
                });

            var schedule = scheduleList.Values.ToList().Find(x => x.config.output == "WebAPI");

            string json = @"{
                 ""mobile"": ""[MOBILENO]"",
                 ""otp"" : ""[OTP]""}";

            if (!string.IsNullOrEmpty(schedule.config.format))
            {
                json = schedule.config.format;
            }
            json = json.Replace("[MOBILENO]", item.mobileno);
            json = json.Replace("[OTP]", item.otp);

            OtpResponse otpResponse = null;

            try
            {
                using (var client = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(schedule.config.authentication))
                    {
                        if (schedule.config.authentication.StartsWith(@"Bearer "))
                        {
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", schedule.config.authentication.Substring("Bearer ".Length));
                        }
                        else
                        {
                            var byteArray = Encoding.ASCII.GetBytes(schedule.config.authentication);
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        }
                    }
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var result = Task.Run(() => client.PostAsync(schedule.config.url, content)).Result;
                    var response = result.Content.ReadAsStringAsync().Result;
                    otpResponse = JsonConvert.DeserializeObject<OtpResponse>(response);
                }
                return otpResponse != null && otpResponse.result.Status;
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        public partial class OtpResponse
        {
            [JsonProperty("jsonrpc")]
            public string Jsonrpc { get; set; }

            [JsonProperty("id")]
            public object Id { get; set; }

            [JsonProperty("result")]
            public Result result { get; set; }

            public partial class Result
            {
                [JsonProperty("status")]
                public bool Status { get; set; }
            }
        }

        public partial class LoyeltyResponse
        {
            [JsonProperty("jsonrpc")]
            public string Jsonrpc { get; set; }

            [JsonProperty("id")]
            public object Id { get; set; }

            [JsonProperty("result")]
            public Result result { get; set; }

            public partial class Result
            {
                [JsonProperty("Success")]
                public Success success { get; set; }

                [JsonProperty("Error")]
                public string Error { get; set; }

                [JsonProperty("status")]
                public bool Status { get; set; }
                public partial class Success
                {
                    [JsonProperty("loyalty_points")]
                    public long LoyaltyPoints { get; set; }

                    [JsonProperty("loyalty_value")]
                    public long LoyaltyValue { get; set; }
                }
            }

        }



        public CustomerBills UpdateRewards(CustomerBills oCustomerBills)
        {
            throw new System.NotImplementedException();
        }
    }
}