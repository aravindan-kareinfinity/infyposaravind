using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Data
{
    public class TokenManager
    {
        private static TokenManager tokenManager = null;
        static TokenManager()
        {
            tokenManager = new TokenManager();
        }
        public static TokenManager Instance
        {
            get
            {
                return tokenManager;
            }
        }
        TokenData tokenData = new TokenData();
        public Token Create(long mrnid,bool isDoctor)
        {
            WebAPI.Data.Token token = new Token(mrnid, isDoctor);
            if (tokenData.Create(token, isDoctor))
            {
                string key = "Token:" + token.CreatedOn.ToString("dd-MM-yyyy hhtt");
                System.Collections.Concurrent.ConcurrentDictionary<string, Token> tokenCache = null;
                if (HttpRuntime.Cache[key] == null)
                {
                    tokenCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Token>();
                    HttpRuntime.Cache.Add(key, tokenCache, null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                    tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, Token>;
                }
                else
                {
                    tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, Token>;
                }
                tokenCache.TryAdd(token.Value, token);
                return token;
            }
            return new Token();
        }

        public Token Find(string securityToken)
        {
            WebAPI.Data.Token token = new Token(securityToken,false);
            if (!token.IsValid) return new Token();
            System.Collections.Concurrent.ConcurrentDictionary<string, Token> tokenCache = null;
            string key = "Token:"+ token.CreatedOn.ToString("dd-MM-yyyy hh");
            if(HttpRuntime.Cache[key] == null)
            {
                tokenCache = new System.Collections.Concurrent.ConcurrentDictionary<string, Token>();
                HttpRuntime.Cache.Add(key, tokenCache, null,
                    System.Web.Caching.Cache.NoAbsoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.High, null);
                tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, Token>;

                Token newToken = tokenData.Find(securityToken);
                if(newToken != null && newToken.IsValid)
                {
                    tokenCache.TryAdd(securityToken, newToken);
                    return newToken;
                }
                return new Token();
            }
            else
            {
                tokenCache = HttpRuntime.Cache[key] as System.Collections.Concurrent.ConcurrentDictionary<string, Token>;
                if (tokenCache.ContainsKey(securityToken))
                    return tokenCache[securityToken];

                Token newToken = tokenData.Find(securityToken);
                if (newToken != null && newToken.IsValid)
                {
                    tokenCache.TryAdd(securityToken, newToken);
                    return newToken;
                }
                return new Token();
            }
        }
    }
}