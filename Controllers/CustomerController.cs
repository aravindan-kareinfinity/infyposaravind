
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
using WebAPI.Data;
using System.Text;

namespace InfyPOS.Controllers
{
    [RoutePrefix("CustomerService")]
    public class CustomerServiceController : ApiController
    {
        [HttpGet]
        [Route("PhoneNumber")]
        public async Task<IHttpActionResult> Search(string id)
        {
            CustomerDataService StockData = new CustomerDataService();
            var result = await Task<List<Customer>>.Run(() => StockData.Search(new CustomerCreteria()
            {
                mobileno = id
            }));
            if (result.Count >= 1)
                return this.Ok(result[0]);

            return this.Ok(new Customer());
        }

        [HttpPost]
        [Route("MobileSave")]
        public async Task<IHttpActionResult> MobileSave(ActionRequest<Customer> request)
        {
            CustomerDataService SupplierData = new CustomerDataService();
            request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
            Customer result = await Task<Customer>.Run(() =>
            {
                request.Item.customertype = 2;
                return SupplierData.Insert(request.Item);
            });
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CompanyAsCustomer")]
        public async Task<IHttpActionResult> CompanyAsCustomer(ActionRequest<Company> request)
        {
            CustomerDataService SupplierData = new CustomerDataService();
            request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
            Customer result = await Task<Customer>.Run(() =>
            {
                return SupplierData.Insert(request.Item);
            });
            return this.Ok(result);
        }

        [HttpPost]
        [Route("CRMIntegration")]
        public async Task<IHttpActionResult> CRMIntegration(ActionMulitpleRequest<Customer> request)
        {
            CustomerDataService SupplierData = new CustomerDataService();
            string result = await Task<string>.Run(() =>
            {
                return SupplierData.Send2CRM(request.Items);
            });
            return this.Ok(result);
        }


        [HttpGet]
        [Route("SearchByID")]
        public async Task<IHttpActionResult> SearchByID(string id)
        {
            CustomerDataService StockData = new CustomerDataService();
            var result = await Task<List<Customer>>.Run(() => StockData.Search(new CustomerCreteria()
            {
                id = long.Parse(id)
            }));
            if (result.Count >= 1)
                return this.Ok(result[0]);

            return this.Ok(new Customer());
        }

        [HttpGet]
        [Route("SearchByRackCard/{id}/{cid}/{lid}")]
        public async Task<IHttpActionResult> SearchByRackCard(string id, string cid, string lid)
        {
            if (id == "-") id = "";
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");

            if (cid == "-") cid = "";
            if (cid.IndexOf("~sls~") >= 0) cid = cid.Replace("~sls~", "/");
            if (cid.IndexOf("~dot~") >= 0) cid = id.Replace("~dot~", ".");


            long locationid = 0;
            long.TryParse(lid, out locationid);
            CustomerDataService StockData = new CustomerDataService();
            var result = await Task<CustomerRack>.Run(() => StockData.SearchByRackNo(new CustomerRack()
            {
                rackno = id,
                cardno = cid,
                locationid = locationid
            }));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchByRackNo")]
        public async Task<IHttpActionResult> SearchByRackNo(string id, string lid)
        {
            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");

            long locationid = 0;
            long.TryParse(lid, out locationid);
            CustomerDataService StockData = new CustomerDataService();
            var result = await Task<CustomerRack>.Run(() => StockData.SearchByRackNo(new CustomerRack()
            {
                rackno = id,
                locationid = locationid
            }));
            return this.Ok(result);
        }


        [HttpGet]
        [Route("SearchByWalkinToken")]
        public async Task<IHttpActionResult> SearchByWalkinToken(string id, string lid)
        {
            long locationid = 0;
            long.TryParse(lid, out locationid);
            CustomerDataService StockData = new CustomerDataService();
            var result = await Task<List<Customer>>.Run(() => StockData.SearchByWalkinToken(new CustomerCreteria()
            {
                walkintoken = id,
                locationid = locationid
            }));
            if (result.Count >= 1)
                return this.Ok(result[0]);
            return this.Ok(new Customer());
        }

        [HttpGet]
        [Route("SearchByPhoneNumber")]
        public async Task<IHttpActionResult> SearchByPhoneNumber(string id, string lid)
        {
            long locationid = 0;
            long.TryParse(lid, out locationid);
            CustomerDataService StockData = new CustomerDataService();
            var result = await Task<List<Customer>>.Run(() => StockData.Search(new CustomerCreteria()
            {
                mobileno = id,
                locationid = locationid
            }));
            if (result.Count >= 1)
                return this.Ok(result[0]);

            return this.Ok(new Customer());
        }

        [HttpGet]
        [Route("SearchByLoyaltyNo")]
        public async Task<IHttpActionResult> SearchByLoyaltyNo(string id, string lid)
        {
            if (string.IsNullOrEmpty(id)) return this.Ok(new Customer());
            long locationid = 0;
            long.TryParse(lid, out locationid);
            CustomerDataService customerData = new CustomerDataService();
            var result = await Task<List<Customer>>.Run(() => customerData.Search(new CustomerCreteria()
            {
                loyeltyno = id,
                locationid = locationid
            }));
            if (result.Count >= 1)
                return this.Ok(result[0]);

            return this.Ok(new Customer());
        }

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customer>();
            result.Item = new Customer();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerCreteria> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<Customer> result = await Task<List<Customer>>.Run(() => CustomerData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchNewCustomer")]
        public async Task<IHttpActionResult> SearchNewCustomer()
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<Customer> result = await Task<List<Customer>>.Run(() => CustomerData.Search(new CustomerCreteria()
            {
                version = 1
            }));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SendOTP")]
        public async Task<IHttpActionResult> SendOTP(ActionRequest<Customer> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            string result = await Task<string>.Run(() => CustomerData.SendOTP(request.Item));
            return this.Ok(new GenericResponse<string>(result));
        }

        [HttpPost]
        [Route("ConfirmOTP")]
        public async Task<IHttpActionResult> ConfirmOTP(ActionRequest<Customer> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            bool result = await Task<bool>.Run(() => CustomerData.ConfirmOTP(request.Item));
            return this.Ok(new GenericResponse<bool>(result));
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customer> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Customer result = await Task<Customer>.Run(() => CustomerData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Customer result = await Task<Customer>.Run(() => CustomerData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Activate")]
        public async Task<IHttpActionResult> Activate(ActionRequest<Customer> request)
        {
            CustomerDataService customerData = new CustomerDataService();
            bool result = await Task<bool>.Run(() => customerData.Activate(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<ChangeAttributes> request)
        {
            CustomerDataService MfgdesignData = new CustomerDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Merge")]
        public async Task<IHttpActionResult> Merge(ActionRequest<MergeCustomerList> request)
        {
            CustomerDataService customerData = new CustomerDataService();
            bool result = await Task<bool>.Run(() => customerData.Marge(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Integrate-Inbound")]
        public async Task<IHttpActionResult> IntegrateInBound(CustomerIntegration request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            bool result = await Task<bool>.Run(() => CustomerData.IntegrateInBound(request));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Integrate-Outbound")]
        public async Task<IHttpActionResult> IntegrateOutBound(CustomerIntegrationGroup request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            CustomerIntegrationGroup result = await Task<CustomerIntegrationGroup>.Run(() => CustomerData.IntegrateOutBound(request));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customer> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            Customer result = await Task<Customer>.Run(() => CustomerData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customer> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            Customer result = await Task<Customer>.Run(() => CustomerData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customer> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            bool result = await Task<bool>.Run(() => CustomerData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCustomerBills")]
        public async Task<IHttpActionResult> SearchBillView(ActionRequest<Customerbillrequest> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<CustomerBills> result = await Task<List<CustomerBills>>.Run(() => CustomerData.SearchCustomerBills(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchCustomerReward")]
        public async Task<IHttpActionResult> SearchCustomerReward(ActionRequest<Customerbillrequest> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<CustomerBills> result = await Task<List<CustomerBills>>.Run(() => CustomerData.SearchCustomerReward(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchRewardBill")]
        public async Task<IHttpActionResult> SearchRewardBill(ActionRequest<Customerbillrequest> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<CustomerBills> result = await Task<List<CustomerBills>>.Run(() => CustomerData.SearchRewardBill(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchBirthday")]
        public async Task<IHttpActionResult> SearchBirthday(ActionRequest<CustomerCreteria> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<Customer> result = await Task<List<Customer>>.Run(() => CustomerData.SearchBirthday(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchSMSLog")]
        public async Task<IHttpActionResult> SearchSMSLog(ActionRequest<SMSMessageLog> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<SMSMessageLog> result = await Task<List<SMSMessageLog>>.Run(() => CustomerData.SearchSMSLog(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchMyCustomer")]
        public async Task<IHttpActionResult> SearchMyCustomer(ActionRequest<CustomerCreteria> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();

            if (!CustomerData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            List<Customer> result = await Task<List<Customer>>.Run(() => CustomerData.SearchMyCustomer(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchTopCustomers")]
        public async Task<IHttpActionResult> SearchTopCustomers(ActionRequest<CustomerCreteria> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            if (!CustomerData.Context.accessiblelocation.Exists(e => e == -1) && request.Item.locationid == 0)
                throw new Exception("Don't have permission to access all location data");

            List<Customer> result = await Task<List<Customer>>.Run(() => CustomerData.SearchTopCustomers(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchInactiveCustomers")]
        public async Task<IHttpActionResult> SearchInactiveCustomers(ActionRequest<CustomerCreteria> request)
        {
            CustomerDataService CustomerData = new CustomerDataService();
            List<Customer> result = await Task<List<Customer>>.Run(() => CustomerData.SearchInactiveCustomers(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("PrepareList")]
        public async Task<IHttpActionResult> PrepareList(ActionRequest<CustomerCreteria> request)
        {
            var httpcontext = System.Web.HttpContext.Current;
            string key = "Customer_" + Guid.NewGuid().ToString();
            while (true)
            {
                if (httpcontext.Cache.Get(key) == null)
                {
                    httpcontext.Cache.Add(key, request.Item, null,
                        DateTime.UtcNow.AddMinutes(5), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Normal, null);
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                    key = "Customer_" + Guid.NewGuid().ToString();
                }
            }
            return this.Ok(key);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintLabel/{organizationid}/{key}")]
        public async Task<IHttpActionResult> PrintLabel(string organizationid, string key)
        {
            try
            {
                var httpcontext = System.Web.HttpContext.Current;
                if (httpcontext.Cache.Get(key) == null)
                {
                    return this.InternalServerError();
                }
                FileContentInfo byteArray = PrintCustomerLabel(long.Parse(organizationid), httpcontext.Cache[key] as CustomerCreteria, false);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }

        [HttpPost]
        [Route("PrintCustomerLabel")]
        public async Task<IHttpActionResult> PrintCustomerLabel(ActionRequest<SupplierServiceController.SupplierIDList> request)
        {
            List<Customer2Print> result = new List<Customer2Print>();
            CustomerDataService supplierdate = new CustomerDataService();

            result = supplierdate.Search(new CustomerCreteria()
            { ids = request.Item.ids }).ConvertAll(e => new Customer2Print()
            {
                code = e.code,
                name = e.name,
                contactperson = e.contactperson,
                mobileno = e.mobileno,
                address = e.address,
                gstno = e.gstno,
                cityname = e.cityname,
                pincode = e.pincode
            });

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
    reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

            List<string> base64Response = new List<string>();
            StringBuilder sb = new StringBuilder();

            var barcodeList = result.ConvertAll(e => (Report.IBarcodeFormat)e);
            var response = barcodeprinter.Render(barcodeList);
            foreach (var item in response)
            {
                var bytes = Encoding.ASCII.GetBytes(item);
                sb.Append(Convert.ToBase64String(bytes));
                sb.Append(";");
            }

            base64Response.Add(sb.ToString());
            return this.Ok(base64Response);
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizatoinid}/{key}")]
        public async Task<IHttpActionResult> PrintPDF(string organizatoinid, string key)
        {
            try
            {
                var httpcontext = System.Web.HttpContext.Current;
                if (httpcontext.Cache.Get(key) == null)
                {
                    return this.InternalServerError();
                }
                FileContentInfo byteArray = PrintCustomerLabel(long.Parse(organizatoinid), httpcontext.Cache[key] as CustomerCreteria, true);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        private string FormatAddress(string addr)
        {
            var formatedaddress = addr.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (formatedaddress.Length > 1) return addr;

            string line1 = "", line2 = "", line3 = "";
            if (!string.IsNullOrEmpty(addr))
            {
                var addresslist = addr.Split(new string[] { "," }, StringSplitOptions.None);
                if (addresslist.Length > 0)
                {
                    foreach (var str in addresslist)
                    {
                        if (line1.Length < 25) line1 += str.Trim() + ",";
                        else if (line1.Length > 25 && line2.Length < 25) line2 += str.Trim() + ",";
                        else if (line1.Length > 25 && line2.Length > 25 && line3.Length < 25) line3 += str.Trim() + ",";
                    }
                    if (string.IsNullOrEmpty(line3) && string.IsNullOrEmpty(line2))
                        return line1.ToUpper();
                    else if (string.IsNullOrEmpty(line3) && !string.IsNullOrEmpty(line2))
                        return line1.ToUpper() + "\r\n" + line2.ToUpper();
                    else
                        return line1.ToUpper() + "\r\n" + line2.ToUpper() + "\r\n" + line3.ToUpper();
                }
                return string.Empty;
            }
            return string.Empty;

        }

        private FileContentInfo PrintCustomerLabel(long organizationid, CustomerCreteria result, bool _SearchMyCustomer)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "CUSTOMERLABEL"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            CustomerDataService CustomerData = new CustomerDataService();
            CustomerData.Context = config.Context;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(config.Context.OrganizationID, lstconfig[0].reportformat.format);
            labreport.Mode = ReportLibrary.DotMatrixReporter.PrintingMode.Label;
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            printer.Report = labreport;

            List<Customer> customerslist = new List<Customer>();
            if (_SearchMyCustomer)
                customerslist = CustomerData.SearchMyCustomer(result);
            else
                customerslist = CustomerData.Search(result);
            labreport.DataSourceList = customerslist;

            foreach (var source in customerslist)
            {
                source.address = FormatAddress(source.address);
                var addresslist = source.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (addresslist.Length == 1)
                {
                    var sourcelist = source.address.Split(new string[] { "," }, StringSplitOptions.None);
                    if (sourcelist.Length <= 2)
                    {
                        addresslist = sourcelist;
                    }
                    else
                    {
                        addresslist = new string[2];
                        addresslist[0] = string.Join(",", sourcelist, 0, 2);
                        addresslist[1] = string.Join(",", sourcelist, 2, sourcelist.Length - 2);
                    }
                }
                source.addresslines = new List<Customer.AddressLine>();
                addresslist.ToList().ForEach(e =>
                {
                    source.addresslines.Add(new Customer.AddressLine()
                    {
                        value = e
                    });
                });
            }

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("IMG-" + imgid, img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}