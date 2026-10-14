using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Data
{
    public class OTPManager
    {
        private static OTPManager tokenManager = null;
        static OTPManager()
        {
            tokenManager = new OTPManager();
        }
        public static OTPManager Instance
        {
            get
            {
                return tokenManager;
            }
        }
        OTPData tokenData = new OTPData();
        public WebAPI.Data.OTP Create(long mrnid,
            string mobileNo,
            bool isDoctor)
        {
            WebAPI.Data.OTP token = new OTP(mrnid,mobileNo, isDoctor);
            if (tokenData.Create(token, isDoctor))
            {
                string key = "TokenList";
                System.Collections.Concurrent.ConcurrentDictionary<string, OTP> tokenCache = null;
                if (HttpRuntime.Cache[key] == null)
                {
                    tokenCache = new System.Collections.Concurrent.ConcurrentDictionary<string, OTP>();
                    HttpRuntime.Cache.Add(key, tokenCache, null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                    tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, OTP>;
                }
                else
                {
                    tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, OTP>;
                }
                OTP dOtp = null;
                string userkey = string.Format("{0}-{1}", mrnid, isDoctor);
                if (tokenCache.TryRemove(userkey, out dOtp))
                    tokenData.Remove(dOtp.UserID,dOtp.IsDoctor, dOtp.Value);

                tokenCache.TryAdd(userkey, token);
                return token;
            }
            return new OTP();
        }

        public OTP Find(long userid,bool isDoctor)
        {
            System.Collections.Concurrent.ConcurrentDictionary<string, OTP> tokenCache = null;
            string key = "TokenList";
            string userkey = string.Format("{0}-{1}", userid, isDoctor);
            if(HttpRuntime.Cache[key] == null)
            {
                tokenCache = new System.Collections.Concurrent.ConcurrentDictionary<string, OTP>();
                HttpRuntime.Cache.Add(key, tokenCache, null,
                    System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, OTP>;

                OTP newToken = tokenData.Find(userid, isDoctor);
                if(newToken != null)
                {
                    if (newToken.Expired)
                        return newToken;

                    tokenCache.TryAdd(userkey, newToken);
                    return newToken;
                }
                return new OTP();
            }
            else
            {
                tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, OTP>;
                if (tokenCache.ContainsKey(userkey))
                    return tokenCache[userkey];

                OTP newToken = tokenData.Find(userid,isDoctor);
                if (newToken != null)
                {
                    if (newToken.Expired)
                        return newToken;

                    tokenCache.TryAdd(userkey, newToken);
                    return newToken;
                }
                return new OTP();
            }
        }
    }
}