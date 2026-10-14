
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
using System.Text;

namespace InfyPOS.Controllers
{
    [RoutePrefix("ConfigurationService")]
    public class ConfigurationServiceController : ApiController
    {


        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Configuration>();
            result.Item = new Configuration();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("ReportList")]
        public async Task<IHttpActionResult> ReportList(string id)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            List<Configuration> result = await Task<List<Configuration>>.Run(() => ConfigurationData.Search(new ConfigurationCreteria()
            {
                keys = new List<string>(id.Split(','))
            }));
            result.ForEach(e => e.reportformat = null);
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<Configuration.printformat> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            Configuration.printformat result = await Task<Configuration.printformat>.Run(() => PrintBarcode(request.Item));
            return this.Ok(result);
        }

        public class TestBarcode : Report.IBarcodeFormat
        {
            public bool SpecialField(string field, SortedDictionary<string, Object> data)
            {
                return false;
            }
            public TestBarcode(int sno, string barcodevalue, string displayvalue)
            {
                keyValue.Add("SNO", sno);
                keyFormat.Add("SNO", "F0");
                keyValue.Add("DISPLAYVALUE", displayvalue);
                keyValue.Add("BARCODEVALUE", barcodevalue);
            }
            private SortedDictionary<string, string> keyFormat = new SortedDictionary<string, string>();
            SortedDictionary<string, object> keyValue = new SortedDictionary<string, object>();
            public SortedDictionary<string, string> KeyFormat
            {
                get
                {
                    return keyFormat;
                }
            }

            public SortedDictionary<string, object> KeyValue
            {
                get
                {
                    return keyValue;
                }
            }
        }

        private Configuration.printformat PrintBarcode(Configuration.printformat item)
        {
            InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode("",
                    item.format, item.mode, item.nooflines);
            var barcodeList = new List<Report.IBarcodeFormat>();
            for (int i = 0; i < item.nooflines; i++)
            {
                barcodeList.Add(new TestBarcode(i + 1, item.barcodevalue, item.displayvalue));
            }

            var response = barcodeprinter.Render(barcodeList);
            List<string> base64Response = new List<string>();
            foreach (var pitem in response)
            {
                var bytes = Encoding.ASCII.GetBytes(pitem);
                base64Response.Add(Convert.ToBase64String(bytes));
            }
            item.printdocument = base64Response;
            return item;
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ConfigurationCreteria> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            if (ConfigurationData.IsSAAS && request.Item.organizationid > 0)
            {
                ConfigurationData.Context.OrganizationID = request.Item.organizationid;
            }
            List<Configuration> result = await Task<List<Configuration>>.Run(() => ConfigurationData.Search(request.Item));
            if (request.Item.key == "APPLICATIONSETTINGS" && request.Item.locationid > 0)
            {
                request.Item.locationid = -9;
                List<Configuration> gresult = await Task<List<Configuration>>.Run(() => ConfigurationData.Search(request.Item));
                if (result.Count > 0 && gresult.Count == 1 && result[0].appsettings != null &&
                    gresult[0].appsettings != null)
                {
                    result[0].appsettings.MergeGlobal(gresult[0].appsettings);
                }
            }
            if (result.Exists(e => e.key == "APPLICATIONSETTINGS" && e.locationid == 0))
            {
                result.Find(e => e.key == "APPLICATIONSETTINGS" && e.locationid == 0).appsettings.system_saledatefreeze = false;
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SetValue")]
        public async Task<IHttpActionResult> SetValue(ActionRequest<Configuration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            Configuration result = await Task<Configuration>.Run(() => ConfigurationData.SetValue(request.Item));
            return this.Ok(result);
        }
        [HttpGet]
        [Route("ConfigByKey/{key}")]
        public async Task<IHttpActionResult> ConfigByKey(string key)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            ConfigurationByString result = await Task<ConfigurationByString>.Run(() => ConfigurationData.GetConfigByKey(key));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("ConfigByKey")]
        public async Task<IHttpActionResult> ConfigByKey(ActionRequest<ConfigurationByString> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            if (ConfigurationData.IsSAAS && request.Item.organizationid > 0)
            {
                ConfigurationData.Context.OrganizationID = request.Item.organizationid;
            }
            bool result = await Task<bool>.Run(() => ConfigurationData.SaveConfigByKey(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Configuration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            if (ConfigurationData.IsSAAS && request.Item.organizationid > 0)
            {
                ConfigurationData.Context.OrganizationID = request.Item.organizationid;
            }
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Configuration result = await Task<Configuration>.Run(() => ConfigurationData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Configuration result = await Task<Configuration>.Run(() => ConfigurationData.Update(request.Item));
                return this.Ok(result);
            }
        }
        [HttpPost]
        [Route("SaveExcludeReward")]
        public async Task<IHttpActionResult> SaveExcludeReward(ActionRequest<ConfigurationCreteria> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            if (ConfigurationData.IsSAAS && request.Item.organizationid > 0)
            {
                ConfigurationData.Context.OrganizationID = request.Item.organizationid;
            }
            request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
            bool result = await Task<bool>.Run(() => ConfigurationData.SaveExcludeReward(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Configuration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            Configuration result = await Task<Configuration>.Run(() => ConfigurationData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ClearChangeDate")]
        public async Task<IHttpActionResult> ClearChangeDate(ActionRequest<DateConfiguration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            DateConfiguration result = await Task<DateConfiguration>.Run(() => ConfigurationData.ClearChangeDate(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeDate")]
        public async Task<IHttpActionResult> ChangeDate(ActionRequest<DateConfiguration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            DateConfiguration result = await Task<DateConfiguration>.Run(() => ConfigurationData.ChangeDate(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("FreezeDate")]
        public async Task<IHttpActionResult> FreezeDate(ActionRequest<DateConfiguration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            DateConfiguration result = await Task<DateConfiguration>.Run(() => ConfigurationData.ChangeDate(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Configuration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            Configuration result = await Task<Configuration>.Run(() => ConfigurationData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Configuration> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            bool result = await Task<bool>.Run(() => ConfigurationData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CheckSum")]
        public async Task<IHttpActionResult> CheckSum(ActionRequest<CheckSumRequest> request)
        {
            ConfigurationDataService ConfigurationData = new ConfigurationDataService();
            CheckSumResponse result = await Task<CheckSumResponse>.Run(() => ConfigurationData.ComputeCheckSum(request.Item));
            return this.Ok(result);
        }
    }
}