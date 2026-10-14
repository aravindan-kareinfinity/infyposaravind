
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("UtilityService")]
    public class UtilityServiceController : ApiController
    {
        public enum SearchType
        {
            SearchLastLR=1
        }
        public class UtilityCreteria : BaseEntity
        {
            public int searchtype { get; set; }
            public string lrentryno { get; set; }
            public SearchType Type
            {
                get
                {
                    return (SearchType)searchtype;
                }
            }

            public List<UtitlityItem> Request { get; set; }
            public List<UtitlityItem> Response { get; set; }
        }

        public class UtitlityItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
            public long GetLong()
            {
                long l;
                if (long.TryParse(Value, out l))
                    return l;
                return 0;
            }
        }


        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> SearchLastLR(ActionRequest<UtilityCreteria> request)
        {
            switch (request.Item.Type)
            {
                case SearchType.SearchLastLR:
                    request.Item.Response = SearchLastLR(request.Item.Request.Find(e => e.Key == "supplierid").GetLong());
                        break;
            }
            return this.Ok(request);
        }


        public List<UtitlityItem> SearchLastLR(long supplierid)
        {
            var dbService = new BaseData();
            var param = new SortedList<string, object>();
            param.Add("supplierid", supplierid);
            var response = new List<UtitlityItem>();
            using (var reader = dbService.ExecuteReader(@"select receivinglocationid,transportid,fromlocationid 
                                            from lrentry 
                                            where supplierid=@supplierid order by id desc LIMIT 1", param))
            {
                if (reader.Read())
                {
                    response.Add(new UtitlityItem()
                    {
                        Key = "receivinglocationid",
                        Value = reader.GetInt64(0).ToString()
                    });
                    response.Add(new UtitlityItem()
                    {
                        Key = "transportid",
                        Value = reader.GetInt64(1).ToString()
                    });
                    response.Add(new UtitlityItem()
                    {
                        Key = "fromlocationid",
                        Value = reader.GetInt64(2).ToString()
                    });
                }
            }
            return response;
        }
    }
}