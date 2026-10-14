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
using System.Runtime.Serialization;
using InfyPOS.Processors;

namespace InfyPOS.QuantoAPI
{
    [RoutePrefix("QUANTOAPI")]
    public class QuantoApiController : ApiController
    {
        [Route("SALES/{token}/{from}/{to}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SALES(string token, string from, string to)
        {
            QuantoAPIDataService ReferencelistDatafULL = new QuantoAPIDataService();
            var resultFULL = await Task<List<SalesData>>.Run(() =>
                ReferencelistDatafULL.SalesAPI(new Criteria()
                {
                    token = token,
                    from = from,
                    to = to
                }));

            var querystring = Request.GetQueryNameValuePairs().ToList();
            if (querystring != null &&
                querystring.Exists(x => x.Key.ToUpper() == "TYPE"))
            {
                var type = querystring.Find(x => x.Key.ToUpper() == "TYPE").Value;
                if (type == "XLSX")
                {
                    string title = "Sales (" +
                        BaseEntity.GetDataFromString(from, "yyyyMMdd").ToString("dd-MM-yyyy") + " - " +
                        BaseEntity.GetDataFromString(to, "yyyyMMdd").ToString("dd-MM-yyyy") + ")";

                    byte[] excelcontent = ReferencelistDatafULL.Excel(resultFULL, title);
                    return new Controllers.FileActionResult(true, "Sales.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(excelcontent));
                }
            }
            return this.Ok(resultFULL);
        }

        [Route("BILLS/{token}/{location}/{from}/{to}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> BILLS(string token,string location, string from, string to)
        {
            QuantoAPIDataService ReferencelistDatafULL = new QuantoAPIDataService();
            var resultFULL = await Task<List<BillsData>>.Run(() =>
                ReferencelistDatafULL.BillsAPI(new Criteria()
                {
                    token = token,
                    from = from,
                    location = location,
                    to = to
                }));

            var querystring = Request.GetQueryNameValuePairs().ToList();
            if (querystring != null &&
                querystring.Exists(x => x.Key.ToUpper() == "TYPE"))
            {
                var type = querystring.Find(x => x.Key.ToUpper() == "TYPE").Value;
                if (type == "XLSX")
                {
                    string title = "Sales (" +
                        BaseEntity.GetDataFromString(from, "yyyyMMdd").ToString("dd-MM-yyyy") + " - " +
                        BaseEntity.GetDataFromString(to, "yyyyMMdd").ToString("dd-MM-yyyy") + ")";

                    byte[] excelcontent = ReferencelistDatafULL.Excel(resultFULL, title);
                    return new Controllers.FileActionResult(true, "Sales.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(excelcontent));
                }
            }
            return this.Ok(resultFULL);
        }

        [Route("PURCHASE/{token}/{from}/{to}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> PURCHASE(string token, string from, string to)
        {
            QuantoAPIDataService ReferencelistDatafULL = new QuantoAPIDataService();
            var resultFULL = await Task<List<SalesData>>.Run(() =>
                ReferencelistDatafULL.PurchaseAPI(new Criteria()
                {
                    token = token,
                    from = from,
                    to = to
                }));

            var querystring = Request.GetQueryNameValuePairs().ToList();
            if (querystring != null &&
                querystring.Exists(x => x.Key.ToUpper() == "TYPE"))
            {
                var type = querystring.Find(x => x.Key.ToUpper() == "TYPE").Value;
                if (type == "XLSX")
                {
                    string title = "Purchase(" +
                        BaseEntity.GetDataFromString(from, "yyyyMMdd").ToString("dd-MM-yyyy") + "-" +
                        BaseEntity.GetDataFromString(to, "yyyyMMdd").ToString("dd-MM-yyyy") + ")";

                    byte[] excelcontent = ReferencelistDatafULL.Excel(resultFULL, title);
                    return new Controllers.FileActionResult(true, "Purchase.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(excelcontent));
                }
            }
            return this.Ok(resultFULL);
        }

        [Route("STOCK/{token}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> STOCK(string token)
        {
            QuantoAPIDataService ReferencelistDatafULL = new QuantoAPIDataService();
            var resultFULL = await Task<List<StockData>>.Run(() =>
                ReferencelistDatafULL.StockAPI(new Criteria()
                {
                    token = token
                }));

            var querystring = Request.GetQueryNameValuePairs().ToList();
            if (querystring != null &&
                querystring.Exists(x => x.Key.ToUpper() == "TYPE"))
            {
                var type = querystring.Find(x => x.Key.ToUpper() == "TYPE").Value;
                if (type == "XLSX")
                {
                    string title = "Stock - "+DateTime.Now.ToString("dd-MM-yyyy");

                    byte[] excelcontent = ReferencelistDatafULL.Excel(resultFULL, title);
                    return new Controllers.FileActionResult(true, "Stock.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(excelcontent));
                }
            }
            return this.Ok(resultFULL);
        }

    }
}