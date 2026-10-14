using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
   
    public abstract class Entity 
    {
        public static DateTime MinDate = new DateTime(1970, 1, 1);
    }

    public class ImageID
    {
        public long id { get; set; }
        public DateTime created { get; set; }
    }

    public class ActionRequest<T> where T : Entity
    {
        public string SecurityToken { get; set; }
        public string DeviceName { get; set; }
        public string DeviceID { get; set; }
        public string ClientVersion { get; set; }
        public string Localization { get; set; }
        public T Data { get; set; }
    }

    public class ResponseDetail : Dictionary<string, object>
    {
        public ResponseDetail()
        {

        }
       
        public string securityToken
        {
            get { return ContainsKey("securityToken") ? this["securityToken"].ToString() : null; }
            set
            {
                if (ContainsKey("securityToken"))
                    this["securityToken"] = value;
                else
                    this.Add("securityToken", value);
            }
        }
        public string serviceKey
        {
            get { return ContainsKey("serviceKey") ? this["serviceKey"].ToString() : null; }
            set
            {
                if (ContainsKey("serviceKey"))
                    this["serviceKey"] = value;
                else
                    this.Add("serviceKey", value);
            }
        }
        public List<string> pageKey
        {
            get { return ContainsKey("pageKey") ? (List<string>) this["pageKey"]  : null; }
            set
            {
                if (ContainsKey("pageKey"))
                    this["pageKey"] = value;
                else
                    this.Add("pageKey", value);
            }
        }
    }

    public class Response<T>:Dictionary<string,object>
    {
        public Response()
        {

        }
        
        public Response(T data)
        {
            this.Add("data", data);
        }
        public string securityToken
        {
            get { return ContainsKey("securityToken") ? this["securityToken"].ToString() : null; }
            set
            {
                if (ContainsKey("securityToken"))
                    this["securityToken"] = value;
                else
                    this.Add("securityToken", value);
            }
        }
        public string serviceKey
        {
            get { return ContainsKey("serviceKey") ? this["serviceKey"].ToString() : null; }
            set
            {
                if (ContainsKey("serviceKey"))
                    this["serviceKey"] = value;
                else
                    this.Add("serviceKey", value);
            }
        }
        public string pageKey
        {
            get { return ContainsKey("pageKey") ? this["pageKey"].ToString() : null; }
            set
            {
                if (ContainsKey("pageKey"))
                    this["pageKey"] = value;
                else
                    this.Add("pageKey", value);
            }
        }
    }

    public class Request<T>
    {
        public Request()
        {

        }
       
        public Request(T data)
        {
            this.data = data;
        }
        public string securityToken { get; set; }
        public string serviceKey { get; set; }
        public string pageKey { get; set; }
        public T data { get; set; }
    }

    public class ActionSingleResult<T> where T : Entity
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
        public ActionSingleResult()
        {

        }
        public ActionSingleResult(T data)
        {
            this.Data = data;
        }
    }

    public class ActionMulitipleResult<T> where T : Entity
    {
        public bool Success { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public ActionMulitipleResult()
        {
            Success = true;
        }
        public ActionMulitipleResult(List<T> value)
        {
            Data = value;
            Success = true;
        }
        public List<T> Data { get; set; }
    }

    public class DataResponse
    {
        public string serviceKey { get; set; }
        public List<string> pageKey { get; set; }
        public string status { get; set; }
        public string errorCode { get; set; }
        public string shortDescription { get; set; }
        public bool HasError { get; set; }
        public ResponseFullSuccess CreateFullResponse()
        {
            ResponseFullSuccess response = new ResponseFullSuccess();
            response.pageKey = pageKey;
            response.serviceKey = serviceKey;
            response.status = status;
            return response;
        }

        public ResponseSuccess CreatePartResponse()
        {
            ResponseSuccess response = new ResponseSuccess();
            response.pageKey = pageKey[0];
            response.serviceKey = serviceKey;
            response.status = status;
            return response;
        }

        public ResponseFailed CreateFailedResponse()
        {
            ResponseFailed response = new ResponseFailed();
            response.errorCode = errorCode;
            response.shortDescription = shortDescription;
            response.Status = status;
            return response;
        }
    }

    public class ResponseFullSuccess
    {
        public string serviceKey { get; set; }
        public List<string> pageKey { get; set; }
        public string status { get; set; }
    }

    public class OTPRequest
    {
        public string securityToken { get;set; }
        public string OTP { get; set; }
    }

    public class OTPResponse
    {
        public OTPResponse(string status)
        {
            this.status = status;
        }
        public string status { get; set; }
    }

    public class ResponseSuccess
    {
        public string serviceKey { get; set; }
        public string pageKey { get; set; }
        public string status { get; set; }
    }

    public class ResponseFailed
    {
        public string Status { get; set; }
        public string errorCode { get; set; }
        public string shortDescription { get; set; }
    }

    public class DBResonse<T>
    {
        public string Status { get; set; }
        public string errorCode { get; set; }
        public string shortDescription { get; set; }
        public T Data { get; set; }
        public bool HasError { get; set; }
        public Exception InnerException { get; set; }
        public DBResonse(T request)
        {
            Data = request;
        }
        public void SetException(Exception exception)
        {
            this.InnerException = exception;
            HasError = true;
        }
        public DBResonse(Exception exception)
        {
            SetException(exception);
        }
        public ResponseFailed CreateFailedResponse()
        {
            ResponseFailed response = new ResponseFailed();
            response.errorCode = errorCode;
            response.shortDescription = InnerException.Message;
            response.Status = "error";
            return response;

        }
    }

    public class HardConfig
    {
        public static bool SupportAutoCAPS(long organizationid)
        {
            
                return InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_alwayscaps;
            
        }

        //public static bool SupportAutoStock
        //{
        //    get
        //    {
        //        return InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sale_warehousestock;
        //    }
        //}
    }

    public class TTDNOWDataRequest
    {
        public string securityToken { get; set; }
        public string serviceKey { get; set; }
        public string pageKey { get; set; }
    }
}
