
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
using WebAPI.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("SupplierService")]
    public class SupplierServiceController : ApiController
    {
        [HttpGet]
        [Route("GSTNo/{gstno}")]
        public async Task<IHttpActionResult> SearchGST(string gstno)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            InfyGST.GSTPortal.Taxpayer result = await Task<InfyGST.GSTPortal.Taxpayer>.Run(() => SupplierData.SearchGST(gstno));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("FilterConfig/{id?}")]
        public async Task<IHttpActionResult> FilterConfig(string id = null)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            SupplierFilterConfig result = await Task<SupplierFilterConfig>.Run(() => SupplierData.FilterConfig(id));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("CompanyAsSupplier")]
        public async Task<IHttpActionResult> CompanyAsSupplier(ActionRequest<Company> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
            Supplier result = await Task<Supplier>.Run(() =>
            {
                return SupplierData.Insert(request.Item);
            });
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<SupplierCreteria> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            List<Supplier> result = await Task<List<Supplier>>.Run(() => SupplierData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchSupplierDetail")]
        public async Task<IHttpActionResult> SearchSupplierDetail(ActionRequest<SupplierCreteria> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            List<SupplierDetails> result = await Task<List<SupplierDetails>>.Run(() => SupplierData.SearchSupplierDetails(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("MobileSave")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> MobileSave(ActionRequest<Supplier> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
            if (request.Item.companylist != null)
            {
                foreach (var item in request.Item.companylist)
                {
                    item.ChangeStatus(BaseEntity.DataStatus.Create);
                }
            }
            Supplier result = await Task<Supplier>.Run(() =>
            {
                request.Item.companylist = new List<Suppliercompany>();
                request.Item.companylist.Add(new Suppliercompany()
                {
                    name = request.Item.name,
                    agentid = request.Item.agentid,
                    preferredtransportid = request.Item.preferredtransportid,
                    isactive = true,
                    createdby = SupplierData.Context.UserID,
                    modifiedby = SupplierData.Context.UserID,
                    createdon = SupplierData.DateTimeNow,
                    modifiedon = SupplierData.DateTimeNow
                });
                return SupplierData.Insert(request.Item);
            });
            return this.Ok(result);
        }
        [HttpPost]
        [Route("Save")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<Supplier> request)
        {
            if (request.Item.supportsourcesupplier)
                request.Item.agentid = 0;
            SupplierDataService SupplierData = new SupplierDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.companylist != null)
                {
                    foreach (var item in request.Item.companylist)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                Supplier result = await Task<Supplier>.Run(() => SupplierData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.companylist != null)
                {
                    foreach (var item in request.Item.companylist)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                Supplier result = await Task<Supplier>.Run(() => SupplierData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<Supplier> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            Supplier result = await Task<Supplier>.Run(() => SupplierData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<Supplier> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            Supplier result = await Task<Supplier>.Run(() => SupplierData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<ChangeAttributes> request)
        {
            SupplierDataService MfgdesignData = new SupplierDataService();
            bool result = await Task<bool>.Run(() => MfgdesignData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<Supplier> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            bool result = await Task<bool>.Run(() => SupplierData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Merge")]
        [WebAPI.InfyAuthorize(PermissionSet.suppliermaster, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Merge(ActionRequest<MergeSupplierList> request)
        {
            SupplierDataService SupplierData = new SupplierDataService();
            bool result = await Task<bool>.Run(() => SupplierData.Marge(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrepareList")]
        public async Task<IHttpActionResult> PrepareList(ActionRequest<SupplierCreteria> request)
        {
            var httpcontext = System.Web.HttpContext.Current;
            string key = "Supplier_" + Guid.NewGuid().ToString();
            while (true)
            {
                if (httpcontext.Cache.Get(key) == null)
                {
                    httpcontext.Cache.Add(key, request.Item, null, DateTime.UtcNow.AddMinutes(5), TimeSpan.Zero, System.Web.Caching.CacheItemPriority.Normal, null);
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                    key = "Supplier_" + Guid.NewGuid().ToString();
                }
            }
            return this.Ok(key);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{key}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string key)
        {
            try
            {
                var httpcontext = System.Web.HttpContext.Current;
                if (httpcontext.Cache.Get(key) == null)
                {
                    return this.InternalServerError();
                }
                FileContentInfo byteArray = PrintSupplierLabelPDF(long.Parse(organizationid), httpcontext.Cache[key] as SupplierCreteria);
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

        private FileContentInfo PrintSupplierLabelPDF(long organizationid, SupplierCreteria result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "SUPPLIERLABEL"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            SupplierDataService SupplierData = new SupplierDataService();
            SupplierData.Context = config.Context;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.Mode = ReportLibrary.DotMatrixReporter.PrintingMode.Label;
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            var supplierlist = SupplierData.Search(result);
            labreport.DataSourceList = supplierlist;

            foreach (var source in supplierlist)
            {
                source.address = FormatAddress(source.address);
                var addresslist = source.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (addresslist.Length == 1)
                {
                    var sourcelist = source.address.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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
                source.addresslines = new List<Supplier.AddressLine>();
                addresslist.ToList().ForEach(e =>
                {
                    source.addresslines.Add(new Supplier.AddressLine()
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
                fci.FileName = "supplierlabel.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

        [HttpPost]
        [Route("PrintSupplierLable")]
        public async Task<IHttpActionResult> PrintSupplierLabel(ActionRequest<SupplierIDList> request)
        {
            List<Supplier2Print> result = new List<Supplier2Print>();
            SupplierDataService supplierdate = new SupplierDataService();

            var supplierlist = supplierdate.Search(new SupplierCreteria()
            { ids = request.Item.ids});

            foreach (var source in supplierlist)
            {
                source.address = FormatAddress(source.address);
                var addresslist = source.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                if (addresslist.Length == 1)
                {
                    var sourcelist = source.address.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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
                source.addresslines = new List<InfyPOS.Models.Supplier.AddressLine>();
                addresslist.ToList().ForEach(e =>
                {
                    source.addresslines.Add(new InfyPOS.Models.Supplier.AddressLine()
                    {
                        value = e
                    });
                });
            }

            result = supplierlist.ConvertAll(e => new Supplier2Print()
            {
                code = e.code,
                name = e.name,
                contactperson = e.contactperson,
                contactnumber = e.contactnumber,
                address = e.address,
                addresslines = e.addresslines,
                tinanddate = e.tinanddate,
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
        public class SupplierIDList : BaseEntity
        {
            public string companytitle { get; set; }
            public long reportid { get; set; }
            public List<long> ids { get; set; }
        }
    }
}