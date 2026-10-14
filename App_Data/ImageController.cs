using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using WebAPI.Data;
using InfyPOS.Common;
using System.Data;
using System.Security.Cryptography;
using InfyPOS.Data;
using InfyPOS.Processors;
using QRCoder;
using InfyPOS.Processors.Migration;
using InfyPOS.Services;
using System.Text;
using InfyPOS.Models;
using System.Reflection;
using System.Web;


namespace InfyPOS.Controllers
{
    [RoutePrefix("File")]
    public class ImageController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadOnlineCatelog/{organizationcode}/{code}")]
        public async Task<IHttpActionResult> DownloadOnlineCatelog(string organizationcode, string code)
        {
            InfyPOS.Processors.OnlineIntegration onlineIntegration = new OnlineIntegration(long.Parse(organizationcode));
            byte[] content = await Task<byte[]>.Run(() => onlineIntegration.Download(code));
            return new FileActionResult(true, "Products.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadStock/{organizationcode}/{locationid}")]
        public async Task<IHttpActionResult> DownloadStock(string organizationcode, string locationid)
        {
            ExcelImportManager onlineIntegration = new ExcelImportManager(long.Parse(organizationcode));
            byte[] content = await Task<byte[]>.Run(() => onlineIntegration.Stock(long.Parse(locationid)));
            return new FileActionResult(true, "Stock.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SyncB2BDesign/{organizationcode}")]
        public async Task<IHttpActionResult> SyncB2BDesign(string organizationcode)
        {
            LrinvoiceitemsDataService LrinvoiceitemsData = new LrinvoiceitemsDataService();
            string result = await Task<string>.Run(() => LrinvoiceitemsData.SyncB2BDesign());
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SyncMissingHSN/{organizationcode}")]
        public async Task<IHttpActionResult> SyncMissingHSN(string organizationcode)
        {
            StockDataService LrinvoiceitemsData = new StockDataService();
            int result = await Task<int>.Run(() => LrinvoiceitemsData.SyncMissingHSN());
            return this.Ok(result);
        }

        [HttpGet]
        [Route("WidgetData/{organizationcode}/{id}")]
        public async Task<IHttpActionResult> WidgetData(string organizationcode, string id)
        {
            long orgid = long.Parse(organizationcode);
            long widgetid = long.Parse(id);
            string content = await Task<string>.Run(() => WidgetDataManager.Instance(orgid).GetData(widgetid));
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(content, Encoding.UTF8, "application/json");
            return ResponseMessage(response);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadOnlineCatelog/{organizationcode}")]
        public async Task<IHttpActionResult> DownloadOnlineCatelog(string organizationcode)
        {
            InfyPOS.Processors.OnlineIntegration onlineIntegration = new OnlineIntegration(long.Parse(organizationcode));
            byte[] content = await Task<ExcelDocument>.Run(() => onlineIntegration.Download(this.Request));
            return new FileActionResult(true, "Products.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Show/{organizationid}/{imageKey}")]
        public async Task<IHttpActionResult> Show(string organizationid, string imageKey)
        {
            Common.ImageData imageData = new Common.ImageData()
            {
                Context = new UserClaim() { OrganizationID = long.Parse(organizationid) }
            };

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadImage(organizationid, imageKey));
            if (byteArray == null)
                return this.NotFound();

            // Generate response
            var response = new FileActionResult(true, false, byteArray.FileName, byteArray.Extension, byteArray.Content).response;

            // Set caching headers properly
            response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(365 * 10)
            };

            // Set content-specific header (Expires must go here)
            if (response.Content != null)
            {
                response.Content.Headers.Expires = DateTimeOffset.UtcNow.AddYears(10);
            }

            response.Headers.Pragma.Clear();

            return new System.Web.Http.Results.ResponseMessageResult(response);

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("S3Prepare/{organizationid}")]
        public async Task<IHttpActionResult> S3SyncPrepare(string organizationid)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string byteArray = await Task<string>.Run(() => imageData.S3SyncPrepare());
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(byteArray)));
            fci.FileName = "S3Prepare.txt";
            fci.Extension = "text/plain";
            return new FileActionResult(false, fci.FileName, fci.Extension, fci.Content);
        }

        [Route("Process")]
        [HttpPost]
        public async Task<IHttpActionResult> MigrateData(InfyPOS.Processors.ProcessManager.ProcessData data)
        {
            BaseData baseData = new BaseData();
            if (baseData.IsSAAS && data.organizationid > 0)
                baseData.Context.OrganizationID = data.organizationid;
            data.Context = baseData.Context;

            data.request = this.Request;
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() => StartProcess(baseData.Context.OrganizationID, data));
            return this.Ok(status);
        }

        [AllowAnonymous]
        [Route("ProcessStatus/{organizationid}/{guid}")]
        [HttpGet]
        public async Task<IHttpActionResult> ProcessStatus(string organizationid, string guid)
        {
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() => InfyPOS.Processors.ProcessManager.Instance(long.Parse(organizationid)).GetStatus(guid));
            return this.Ok(status);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ProcessDownloadXLSX/{organizationid}/{guid}")]
        public async Task<IHttpActionResult> ProcessDownloadXLSX(string organizationid, string guid)
        {
            Tuple<string, byte[]> content = await Task<Tuple<string, byte[]>>.Run(() => InfyPOS.Processors.ProcessManager.Instance(long.Parse(organizationid)).GetResultBytes(guid));
            if (content.Item1 == "ZIP")
                return new FileActionResult(true, "DataDump.zip", "application/zip", new ByteArrayContent(content.Item2));
            else
                return new FileActionResult(true, "DataDump.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new ByteArrayContent(content.Item2));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("OnlineStockSync/{organizationid}")]
        public async Task<IHttpActionResult> OnlineStockSync(string organizationid)
        {
            StockmarkerDataService stockmarkerDataService = new StockmarkerDataService();
            stockmarkerDataService.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            string byteArray = await Task<string>.Run(() => stockmarkerDataService.SendOLQTY());
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(byteArray)));
            fci.FileName = "s3Sync.txt";
            fci.Extension = "text/plain";
            return new FileActionResult(false, fci.FileName, fci.Extension, fci.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("S3Sync/{organizationid}")]
        public async Task<IHttpActionResult> S3Sync(string organizationid)
        {

            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string byteArray = await Task<string>.Run(() => imageData.S3Sync());
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(byteArray)));
            fci.FileName = "s3Sync.txt";
            fci.Extension = "text/plain";
            return new FileActionResult(false, fci.FileName, fci.Extension, fci.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PO/{licensekey}/{uniquekey}")]
        public async Task<IHttpActionResult> PO(string licensekey, string uniquekey)
        {
            if (uniquekey.IndexOf('.') > 0)
                uniquekey = uniquekey.Substring(0, uniquekey.IndexOf('.'));
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadPurchaseOrder(licensekey, uniquekey));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("SP/{licensekey}/{uniquekey}")]
        public async Task<IHttpActionResult> SP(string licensekey, string uniquekey)
        {
            if (uniquekey.IndexOf('.') > 0)
                uniquekey = uniquekey.Substring(0, uniquekey.IndexOf('.'));
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadSupplierPayment(licensekey, uniquekey));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("BP/{licensekey}/{uniquekey}")]
        public async Task<IHttpActionResult> BP(string licensekey, string uniquekey)
        {
            if (uniquekey.IndexOf('.') > 0)
                uniquekey = uniquekey.Substring(0, uniquekey.IndexOf('.'));
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadBillCopy(licensekey, uniquekey));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PDF/{organizationid}/{uniquekey}")]
        public async Task<IHttpActionResult> PDF(string organizationid, string uniquekey)
        {
            if (uniquekey.IndexOf('.') > 0)
                uniquekey = uniquekey.Substring(0, uniquekey.IndexOf('.'));
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadTypedContent(organizationid, uniquekey, true));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(true, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("CSV/{organizationid}/{uniquekey}")]
        public async Task<IHttpActionResult> CSV(string organizationid, string uniquekey)
        {
            if (uniquekey.IndexOf('.') > 0)
                uniquekey = uniquekey.Substring(0, uniquekey.IndexOf('.'));
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreateCSV(organizationid, uniquekey));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(true, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }
        public class HashDetail
        {
            public long Id0 { get; set; }
            public long Id1 { get; set; }
            public long Id2 { get; set; }
            public string Hash { get; set; }
            public bool HasMatch { get; set; }

            public HashDetail(long Id0, long id1, long id2)
            {
                byte[] orgbytes = BitConverter.GetBytes(Id0);
                byte[] patbytes = BitConverter.GetBytes(id1);
                byte[] labbytes = BitConverter.GetBytes(id2);
                byte[] data = new byte[28];
                Array.Copy(orgbytes, 0, data, 0, 8);
                Array.Copy(patbytes, 0, data, 8, 8);
                Array.Copy(labbytes, 0, data, 16, 8);
                int hash = 0;
                unchecked
                {
                    foreach (byte b in data)
                        hash = (hash * 31) ^ b;
                }
                Array.Copy(BitConverter.GetBytes(hash), 0, data, 24, 4);
                Hash = InfyPOS.Common.EncodingExtensions.Encode62(data);
            }

            public static long GetLongHash(string input)
            {
                HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
                byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
                return BitConverter.ToInt64(byteHash, 0);
            }

            public HashDetail(string source)
            {
                byte[] soure = InfyPOS.Common.EncodingExtensions.Decode62(source);
                Id0 = BitConverter.ToInt64(soure, 0);
                Id1 = BitConverter.ToInt64(soure, 8);
                Id2 = BitConverter.ToInt64(soure, 16);
                int hash = BitConverter.ToInt32(soure, 24);
                int newHash = 0;
                byte[] data = new byte[28];
                Array.Copy(soure, 0, data, 0, 24);
                unchecked
                {
                    foreach (byte b in data)
                        newHash = (newHash * 31) ^ b;
                }
                HasMatch = hash == newHash;
            }
        }

        public FileContentInfo CreateCSV(string organizationid, string uniquekey)
        {
            StockDataService stockDataService = new StockDataService()
            {
                Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                }
            };
            var barcodelist = stockDataService.SearchCuttingBarcodes();
            var fcitest = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(barcodelist)));
            fcitest.FileName = "CuttingBarcode.csv";
            fcitest.Extension = "plain/csv";
            return fcitest;
        }

        public FileContentInfo ReadTypedContent(string organizationid, string uniquekey, bool ispdf)
        {
            long orgid = 0;
            if (!long.TryParse(organizationid, out orgid))
            {
                var licensekey = LicenseManager.License.Key.Replace("-", "");
                if (licensekey != organizationid)
                    return null;

                InfyPOS.Controllers.ImageController.HashDetail hashDetail = new ImageController.HashDetail(uniquekey);
                orgid = hashDetail.Id0;
                if (!hashDetail.HasMatch || ImageController.HashDetail.GetLongHash(licensekey) != hashDetail.Id1)
                    return null;

                Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = orgid } };
                var file = imageData.GetData(hashDetail.Id2);
                if (file != null)
                {
                    var fci = new FileContentInfo(new ByteArrayContent(file.data));
                    fci.FileName = "Quanto.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
                var fcitest = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("The given file details are not found. Please contact System Administrator.")));
                fcitest.FileName = "Error.txt";
                fcitest.Extension = "plain/text";
                return fcitest;
            }
            else
            {
                Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = orgid } };
                HashDetail hashDetail = new HashDetail(uniquekey);
                if (hashDetail.HasMatch)
                {
                    var file = imageData.GetData(hashDetail.Id0);
                    if (file != null)
                    {
                        var fci = new FileContentInfo(new ByteArrayContent(file.data));
                        fci.FileName = "Quanto.pdf";
                        fci.Extension = "application/pdf";
                        return fci;
                    }
                }
                var fcitest = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("The given file details are not found. Please contact System Administrator.")));
                fcitest.FileName = "Error.txt";
                fcitest.Extension = "plain/text";
                return fcitest;
            }
        }


        public FileContentInfo DownloadBillCopy(string licensekey, string uniquekey)
        {
            var hasdetail = new InfyPOS.Controllers.ImageController.HashDetail(uniquekey);
            HashDetail hashDetail = new HashDetail(uniquekey);
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = hashDetail.Id0 } };
            if (hashDetail.HasMatch)
            {
                BillDataService BillData = new BillDataService();
                BillData.Context = new UserClaim() { OrganizationID = hashDetail.Id0 };
                var result = BillData.Search(new BillCreteria() { id = hashDetail.Id2 });

                BillServiceController billServiceController = new BillServiceController();
                return billServiceController.PrintBill(hashDetail.Id0, result[0], false, false,
                    "", 0);
            }
            var fcitest = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("Bill not found with this given details")));
            fcitest.FileName = "Error.txt";
            fcitest.Extension = "plain/text";
            return fcitest;
        }

        public FileContentInfo DownloadSupplierPayment(string licensekey, string uniquekey)
        {
            var hasdetail = new InfyPOS.Controllers.ImageController.HashDetail(uniquekey);
            HashDetail hashDetail = new HashDetail(uniquekey);
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = hashDetail.Id0 } };
            if (hashDetail.HasMatch)
            {
                //var file = imageData.GetData(hashDetail.Id2);
                //if (file != null)
                //{
                //    var fci = new FileContentInfo(new ByteArrayContent(file.data));
                //    fci.FileName = "SupplierPayment.pdf";
                //    fci.Extension = "application/pdf";
                //    return fci;
                //}
                //else
                //{
                SupplierpaymentDataService SupplierpaymentData = new SupplierpaymentDataService();
                SupplierPaymentPrint print = SupplierpaymentData.Print(new Supplierpayment() { id = hashDetail.Id2 });
                FileContentInfo fci = SupplierpaymentData.PrintChequeAdvice(SupplierpaymentData.Context.OrganizationID, print);
                fci.FileName = "SupplierPayment.pdf";
                fci.Extension = "application/pdf";
                return fci;
                //}
            }
            var fcitest = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("Supplier Payment not found with this given details")));
            fcitest.FileName = "Error.txt";
            fcitest.Extension = "plain/text";
            return fcitest;
        }

        public FileContentInfo DownloadPurchaseOrder(string organizationid, string uniquekey)
        {
            HashDetail hashDetail = new HashDetail(organizationid);
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = hashDetail.Id0 } };
            if (hashDetail.HasMatch)
            {
                //var file = imageData.GetData(hashDetail.Id2);
                //if (file != null)
                //{
                //    var fci = new FileContentInfo(new ByteArrayContent(file.data));
                //    fci.FileName = "PurchaseOrder.pdf";
                //    fci.Extension = "application/pdf";
                //    return fci;
                //}
                //else
                //{
                InfyPOS.Controllers.PurchaseorderServiceController purchaseorderServiceController = new PurchaseorderServiceController();
                PurchaseorderDataService PurchaseorderData = new PurchaseorderDataService() { Context = new UserClaim() { OrganizationID = hashDetail.Id0 } };
                var result = PurchaseorderData.Print(new Models.Purchaseorder() { id = hashDetail.Id2 });
                FileContentInfo fci = purchaseorderServiceController.PrintPO(hashDetail.Id0, result);
                fci.FileName = "PurchaseOrder.pdf";
                fci.Extension = "application/pdf";
                return fci;
                //}
            }
            var fcitest = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("Purchase order not found with this given details")));
            fcitest.FileName = "Error.txt";
            fcitest.Extension = "plain/text";
            return fcitest;
        }

        /*SELECT
  pid,
  now() - pg_stat_activity.query_start AS duration,
  query,
  state
FROM pg_stat_activity
WHERE query != 'listen dbchange' and (now() - pg_stat_activity.query_start) > interval '15 minutes';*/


        [AllowAnonymous]
        [HttpGet]
        [Route("ClearTrace/{organizationid}/{beforehour}")]
        public async Task<IHttpActionResult> ClearTrace(string organizationid, string beforehour)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string byteArray = await Task<string>.Run(() => ClearLog(imageData.Context.OrganizationID, beforehour));
            return this.Ok(byteArray);
        }

        private string ClearLog(long organizationid, string beforehour)
        {
            string query = "Delete from trace where  (now() - loggedat) > interval '" + beforehour + " hours'";
            try
            {
                Common.BaseData baseData = new Common.BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
                return "Removed Rows - " + baseData.ExecuteQuery(query, null);
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("MissingIndexes/{organizationid}")]
        public async Task<IHttpActionResult> MissingIndexes(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => GetMissingIndexes(organizationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("TableSize/{organizationid}")]
        public async Task<IHttpActionResult> TableSize(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => GetTableSize(organizationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DBSize/{organizationid}")]
        public async Task<IHttpActionResult> DBSize(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => GetDBSize(organizationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UANSyncNegativeStock/{organizationid}")]
        public async Task<IHttpActionResult> UANSyncNegativeStock(string organizationid)
        {

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => uanSyncNegativeStock(organizationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("KillLongRunningSQL/{organizationid}")]
        public async Task<IHttpActionResult> KillLongRunningSQL(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => KillrunningSQL(organizationid, true));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("KillIdleSession")]
        public async Task<IHttpActionResult> KillIIDLESession()
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => KillIdleSession());
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("KillLockSQL/{organizationid}")]
        public async Task<IHttpActionResult> KillLockSQL(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => KillTableLock(organizationid, true));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("RangeTaxSalesLossList/{organizationid}/{mode=null}/{from=null}/{to=null}")]
        public async Task<IHttpActionResult> RangeTaxSalesLossList(string organizationid, string mode, string from, string to)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => RangeTaxSalesLoss(organizationid, mode, from, to));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("StockSaleSynch/{organizationid}")]
        public async Task<IHttpActionResult> StockSaleSynch(string organizationid)
        {
            //FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => UpdateOQty(organizationid));
            //if (byteArray == null)
            return this.NotFound();
            //return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo UpdateOQty(string organizationid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>Updating Stock Sale Synchronization</H4>");
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var returnrows = imageData.ExecuteQuery(@"with allsales
as
(
	select sum(billitems.qty) sqty,salablegoodsid id from billitems
	inner join bill on bill.id = billitems.billid
	where billitems.isactive = true and bill.isactive = true and bill.billtype != 9
	group by salablegoodsid
),missmatches
as
(
select salablegoods.id,salablegoods.barcode,salablegoods.oqty,allsales.sqty from salablegoods
left outer join allsales on allsales.id = salablegoods.id
where isactive = true and salablegoods.oqty != allsales.sqty
)
update salablegoods set oqty=sqty from missmatches
where salablegoods.id = missmatches.id", new SortedList<string, object>());


            sb.AppendFormat("<H4>Affected records {0}</H4>\r\n", returnrows);

            sb.AppendLine("</BODY></HTML>");
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("BillSettlementDateMissmatches/{organizationid}/{locationid}")]
        public async Task<IHttpActionResult> BillSettlementDateMissmatches(string organizationid, string locationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => billsettlementDateMissmatches(organizationid, locationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo billsettlementDateMissmatches(string organization, string location)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>Bill Date  Settlement Date Missmatches</H4>");
            BillsettlementDataService imageData = new BillsettlementDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organization) } };
            long locationid;
            long.TryParse(location, out locationid);
            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th><th>{5}</th><th>{6}</th>><th>{7}</th></tr>",
    "locationname", "settledby", "settledon", "billno", "billdate", "settlementcode", "settlementon", "amount"));

            int count = 0;
            using (var reader = imageData.ExecuteReader(@"select referencelist.name locationname,employee.name settledby, 
billsettlementmaster.createdon settledon,
bill.billno,bill.billdate,billsettlementmaster.code settlementcode,
billsettlementmaster.settlementon::date,bill.receivable amount from billsettlementmaster
inner join billsettlement on billsettlement.billsettlementmasterid = billsettlementmaster.id
inner join bill on bill.id = billsettlement.billid
inner join referencelist on referencelist.id = bill.locationid
left outer join users on users.id = billsettlementmaster.createdby
left outer join employee on employee.id = users.employeeid
where bill.billdate::date != billsettlementmaster.settlementon::date " +
(locationid > 0 ? " and bill.locationid = " + locationid : ""), null))
            {
                while (reader.Read())
                {
                    count++;
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td></tr>",
                        reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString(),
                        reader["settledby"] == DBNull.Value ? "" : reader["settledby"].ToString(),
                            reader["settledon"] == DBNull.Value ? "" : reader["settledon"].ToString(),
                            reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString(),
                            reader["billdate"] == DBNull.Value ? "" : reader["billdate"].ToString(),
                            reader["settlementcode"] == DBNull.Value ? "" : reader["settlementcode"].ToString(),
                            reader["settlementon"] == DBNull.Value ? "" : reader["settlementon"].ToString(),
                            reader["amount"] == DBNull.Value ? "" : reader["amount"].ToString()
                        ));
                }
            }

            sb.AppendLine("</table></BODY></HTML>");

            sb.AppendFormat("<H4>Affected rows {0}</H4>\r\n", count);
            sb.AppendLine("</BODY></HTML>");
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("StockTransactionMismatches/{organizationid}/{locationid}")]
        public async Task<IHttpActionResult> StockTransactionMismatches(string organizationid, string locationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => stocktransactionmismatches(organizationid, locationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo stocktransactionmismatches(string organization, string location)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>Mismatched stock list</H4>");
            StocktransactionDataService imageData = new StocktransactionDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organization) } };
            long locationid;
            long.TryParse(location, out locationid);
            var items = imageData.StockMissmatches(locationid);


            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th><th>{5}</th><th>{6}</th></tr>",
                "locationname", "productname", "barcode", "salablegoodsid", "stockid", "systemstk", "transactionstk"));

            foreach (var itm in items)
            {
                sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td></tr>",
                    itm.locationname,
                    itm.productname,
                        itm.barcode,
                        itm.salablegoodsid,
                        itm.stockid,
                        itm.systemstk,
                        itm.transactionstk
                    ));
            }


            sb.AppendLine("</table></BODY></HTML>");

            sb.AppendFormat("<H4>Affected rows {0}</H4>\r\n", items.Count);
            sb.AppendLine("</BODY></HTML>");
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("UpdateEPrice/{organizationid}")]
        public async Task<IHttpActionResult> UpdateEPrice(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ExecuteUpdateEPrice(organizationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo ExecuteUpdateEPrice(string organizationid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>Updating EPrice on Stock</H4>");
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };

            TaxDataService taxDataService = new TaxDataService() { Context = imageData.Context };
            var taxList = taxDataService.Search(new Models.TaxCreteria());
            foreach (var taxitem in taxList)
            {
                if (taxitem.type == 2)
                {
                    bool modified = false;
                    if (taxitem.taxsplit != null && taxitem.taxsplit.items != null)
                    {
                        if (taxitem.taxsplit.items.RemoveAll(e => e.fromamount == 0 && e.toamount == 0) > 0)
                            modified = true;
                        if (taxitem.taxsplit.items.RemoveAll(e => e.taxpercent == 0) > 0)
                            modified = true;
                    }

                    if (taxitem.taxsplit != null && taxitem.taxsplit.items != null &&
                    taxitem.taxsplit.items.Count <= 1)
                    {
                        modified = true;
                        taxitem.type = 1;
                        if (taxitem.taxpercentage == 0)
                        {
                            taxitem.taxpercentage = taxitem.taxsplit.items.Count > 0 ? taxitem.taxsplit.items.First().taxpercent : 0;
                        }
                        taxitem.taxsplit = new Models.Tax.TaxSplit() { items = new List<Models.Tax.TaxSplit.Item>() };
                    }
                    if (modified)
                        taxDataService.Update(taxitem);
                }
            }

            decimal rangestart = 1045;
            if (InfyPOS.Services.ConfigManager.Instance(imageData.Context.OrganizationID).Config.system_posexclusivetax)
                rangestart = 999;
            var param = new SortedList<string, object>();
            param.Add("rangestart", rangestart);
            int returnrows = imageData.ExecuteQuery(@"update lrinvoiceitems set salestaxpercentage =  
case when tax.type != 2 then tax.taxpercentage when sellingprice-salediscount>@rangestart then 12 else 5 end 
from products 
inner join tax on tax.id = products.salestaxid
where products.id = lrinvoiceitems.productid;", param, 50000);

            sb.AppendFormat("<H4>Purchase Items updated {0}</H4>\r\n", returnrows);
            returnrows = imageData.ExecuteQuery(@"update lrinvoiceitems set salestax =  (sellingprice-salediscount)/(100+salestaxpercentage)*salestaxpercentage;", null, 50000);
            sb.AppendFormat("<H4>Purchase Items Sales Tax updated {0}</H4>\r\n", returnrows);


            if (InfyPOS.Services.ConfigManager.Instance(imageData.Context.OrganizationID).Config.system_posexclusivetax)
            {
                imageData.ExecuteQuery(@"update stock set eprice = price-discount", null, 50000);
            }
            else
            {
                returnrows = imageData.ExecuteQuery(@"with computedtax as
                (
                    select id,type,
                    case when taxsplit is null then tax.taxpercentage
					when cast((taxsplit->>'items')::json->0->'sgst' as text) is null then tax.taxpercentage
					else cast(cast((taxsplit->>'items')::json->0->'sgst' as text) as numeric)+cast(cast((taxsplit->>'items')::json->0->'cgst' as text) as numeric) end
                    taxpercentage 
                    from tax
                ),
                stocktax
                as
                (
                select stock.id,tax.type,
	                case when tax.type != 2 then tax.taxpercentage when price-discount>1045 then 12 else 5 end taxpercentage from stock
                inner join products on products.id = stock.productid
                inner join computedtax tax on tax.id = products.salestaxid
                )
                update stock set eprice = (price-discount)/(100+taxpercentage)*100 from stocktax
                where stocktax.id = stock.id;", null, 50000);
            }

            sb.AppendFormat("<H4>Sales Items Exclusive Tax Price updated {0}</H4>\r\n", returnrows);

            var stockbuyrows = imageData.ExecuteQuery(@"with computedtax as
                (
                select id,type,cast(cast((taxsplit->>'items')::json->0->'sgst' as text) as numeric)+cast(cast((taxsplit->>'items')::json->0->'cgst' as text) as numeric) taxpercentage from tax
                ),
                stocktax
                as
                (
                select stock.id,tax.type,
	                case when tax.type != 2 then tax.taxpercentage when price-discount>1045 then 12 else 5 end taxpercentage from stock
                inner join products on products.id = stock.productid
                inner join computedtax tax on tax.id = products.salestaxid
                )
                update stock set ibuyingprice = round(stock.buyingprice + (stock.buyingprice*taxpercentage/100),2) from stocktax
                where stock.ibuyingprice =0 and stock.buyingprice != 0 and stocktax.id = stock.id;", null, 50000);

            sb.AppendFormat("<H4>Stock IBuyingprice updated {0}</H4>\r\n", stockbuyrows);
            sb.AppendLine("</BODY></HTML>");
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("RunningSQL/{organizationid}")]
        public async Task<IHttpActionResult> RunningSQL(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => KillrunningSQL(organizationid, false));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("InwardAllPendingToRetail/{organizationid}")]
        public async Task<IHttpActionResult> InwardAllPendingToRetail(string organizationid)
        {

            BundlesDataService bundlesDataService = new BundlesDataService(false);
            bool byteArray = await Task<bool>.Run(() => bundlesDataService.InwardAllPendingToRetail());
            return this.Ok(byteArray);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("TriggerSchedule/{organizationid}/{scheduleid=null}/{id=null}")]
        public async Task<IHttpActionResult> TriggerSchedule(string organizationid, string scheduleid, string id)
        {
            BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            if (string.IsNullOrEmpty(scheduleid) || scheduleid == "null" || id == "null" || int.Parse(scheduleid) <= 0)
            {
                long lscheduleid;
                long.TryParse(scheduleid, out lscheduleid);
                await Task.Run(() => InfyPOS.Processors.ScheduledReports.Instance(baseData.Context.OrganizationID).Run(true, lscheduleid));
                return this.Ok<string>("Success");
            }
            else
            {
                var result = await Task<Tuple<bool, string>>.Run(() => InfyPOS.Processors.ScheduledReports.Instance(baseData.Context.OrganizationID).Run(long.Parse(scheduleid), id));

                var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(result.Item2)));
                fci.FileName = "result.html";
                fci.Extension = "text/html";
                return new FileActionResult(false, fci.FileName, fci.Extension, fci.Content);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("TriggerDayend/{organizationid}")]
        public async Task<IHttpActionResult> TriggerDayend(string organizationid)
        {
            ImageData imageData = new ImageData();
            await Task.Run(() => Processors.Schedule_Actions.Instance.RundDateChange(long.Parse(organizationid), new Models.DateConfiguration()
            {
                currentday = imageData.DateTimeNow.Date
            }));
            return this.Ok<string>("Success");
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("RefreshSetting/{organizationid}")]
        public async Task<IHttpActionResult> RefreshSetting(string organizationid)
        {
            ImageData imageData = new ImageData();
            await Task.Run(() => InfyPOS.Services.ConfigManager.Instance(long.Parse(organizationid)).Initialize());
            return this.Ok<string>("Success");
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ShowDataStatistics/{organizationid}")]
        public async Task<IHttpActionResult> ShowDataStatistics(string organizationid)
        {

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ClearDataTables(long.Parse(organizationid), true));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        //[HttpGet]
        //[Route("ClearData/{organizationid}")]
        //public async Task<IHttpActionResult> ClearData(string organizationid)
        //{

        //    FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ClearDataTables(long.Parse(organizationid), false));
        //    if (byteArray == null)
        //        return this.NotFound();
        //    return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        //}

        private FileContentInfo ClearDataTables(long organizationid, bool showdata)
        {
            BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var tables = Utility.CreateSchema(baseData.CreateConnection().ConnectionString);
            if (showdata)
            {
                string query = @"SELECT stats.relname
           AS table,
       pg_size_pretty(pg_relation_size(statsio.relid))
           AS table_size,
       pg_size_pretty(pg_total_relation_size(statsio.relid) 
           - pg_relation_size(statsio.relid))
           AS related_objects_size,
       pg_size_pretty(pg_total_relation_size(statsio.relid))
           AS total_table_size,
       stats.n_live_tup
           AS live_rows
  FROM pg_catalog.pg_statio_user_tables AS statsio
  JOIN pg_stat_user_tables AS stats
 USING (relname)
 WHERE stats.schemaname = current_schema  -- Replace with any schema name
 UNION ALL
SELECT 'TOTAL'
           AS table,
       pg_size_pretty(sum(pg_relation_size(statsio.relid)))
           AS table_size,
       pg_size_pretty(sum(pg_total_relation_size(statsio.relid) 
           - pg_relation_size(statsio.relid)))
           AS related_objects_size,
       pg_size_pretty(sum(pg_total_relation_size(statsio.relid)))
           AS total_table_size,
       sum(stats.n_live_tup)
           AS live_rows
  FROM pg_catalog.pg_statio_user_tables AS statsio
  JOIN pg_stat_user_tables AS stats
 USING (relname)
 WHERE stats.schemaname = current_schema  -- Replace with any schema name
 ORDER BY live_rows ASC;";

                using (var reader = baseData.ExecuteReader(query, new SortedList<string, object>()))
                {
                    if (reader.Read())
                    {
                        sb.AppendFormat("{0}\t{1}\t{2}\t{3}\r\n", reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetDecimal(3));
                    }
                }
            }
            //else
            //{
            //    foreach (var table in tables)
            //    {
            //        if (table.name.ToLower() == "users") continue;
            //        if (table.name.ToLower() == "usersession") continue;
            //        if (table.name.ToLower() == "trace") continue;
            //        sb.AppendFormat("{0}\t{1}", table.name, baseData.ExecuteQuery(table.CreateDeleteScript(), null));
            //    }
            //}
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "schema.txt";
            fci.Extension = "text/plain";
            return fci;
        }

        private FileContentInfo GetTableSize(string organizationid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>DB Sizes</H4>");
            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th></tr>", "Table", "Table Size", "Index Size", "Total Size"));
            System.Text.StringBuilder sbcommand = new System.Text.StringBuilder();
            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string query = @"SELECT
    table_name,
    pg_size_pretty(table_size) AS table_size,
    pg_size_pretty(indexes_size) AS indexes_size,
    pg_size_pretty(total_size) AS total_size
FROM (
    SELECT
        table_name,
        pg_table_size(table_name) AS table_size,
        pg_indexes_size(table_name) AS indexes_size,
        pg_total_relation_size(table_name) AS total_size
    FROM (
        SELECT ('""' || table_schema || '"".""' || table_name || '""') AS table_name
        FROM information_schema.tables
    ) AS all_tables
    ORDER BY total_size DESC
) AS pretty_sizes;";
            using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                        reader[0].ToString(),
                        reader[1].ToString(),
                        reader[2].ToString(),
                        reader[3].ToString()));
                }
            }

            sb.AppendLine("</table></BODY></HTML>");

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }


        private FileContentInfo GetMissingIndexes(string organizationid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>DB Sizes</H4>");
            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th></tr>", "Table", "Priority", "Read Bytes", "Seq Scan"));
            System.Text.StringBuilder sbcommand = new System.Text.StringBuilder();
            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string query = @"SELECT
  x1.table_in_trouble,
  pg_relation_size(x1.table_in_trouble) AS sz_n_byts,
  x1.seq_scan,
  x1.idx_scan,
  CASE
  WHEN pg_relation_size(x1.table_in_trouble) > 500000000
    THEN 'Exceeds 500 megs, too large to count in a view. For a count, count individually'::text
  ELSE count(x1.table_in_trouble)::text
  END                                   AS tbl_rec_count,
  x1.priority
FROM
  (
    SELECT
      (schemaname::text || '.'::text) || relname::text AS table_in_trouble,
      seq_scan,
      idx_scan,
      CASE
      WHEN (seq_scan - idx_scan) < 500
        THEN 'Minor Problem'::text
      WHEN (seq_scan - idx_scan) >= 500 AND (seq_scan - idx_scan) < 2500
        THEN 'Major Problem'::text
      WHEN (seq_scan - idx_scan) >= 2500
        THEN 'Extreme Problem'::text
      ELSE NULL::text
      END AS priority
    FROM
      pg_stat_all_tables
    WHERE
      seq_scan > idx_scan
      AND schemaname != 'pg_catalog'::name
    AND seq_scan > 100) x1
GROUP BY
  x1.table_in_trouble,
  x1.seq_scan,
  x1.idx_scan,
  x1.priority
ORDER BY
  x1.priority DESC,
  x1.seq_scan;";
            using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                        reader[0].ToString(),
                        reader[5].ToString(),
                        reader[1].ToString(),
                        reader[2].ToString()));
                }
            }

            sb.AppendLine("</table></BODY></HTML>");

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }
        private FileContentInfo GetDBSize(string organizationid)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>DB Sizes</H4>");
            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th></tr>", "DB", "Size (GB)"));
            System.Text.StringBuilder sbcommand = new System.Text.StringBuilder();
            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string query = @"SELECT pg_database.datname as ""databasename"", 
pg_database_size(pg_database.datname) / 1024 / 1024 / 1024 AS sizegb
FROM pg_database ORDER by pg_database_size(pg_database.datname) DESC;";
            using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td></tr>",
                        reader[0].ToString(),
                        reader[1].ToString()));
                }
            }

            sb.AppendLine("</table></BODY></HTML>");

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo KillTableLock(string organizationid, bool delete)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>Long running queries</H4>");
            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th></tr>", "ProcessID", "Duration", "Query", "Status"));
            List<string> sbcommand = new List<string>();
            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string query = @"SELECT
                        pid,
                        now() - pg_stat_activity.query_start AS duration,
                        query,
                        state
                        FROM pg_stat_activity
                        WHERE query != 'listen dbchange' and not application_name ilike 'pgA%' and mode ='RowExclusiveLock'";
            using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", reader[0].ToString(),
                        reader[1].ToString(), reader[2].ToString(), reader[3].ToString()));
                    sbcommand.Add(string.Format("SELECT pg_terminate_backend({0});", reader[0].ToString()));
                }
            }
            sb.AppendLine("</table></BODY></HTML>");

            sbcommand.ForEach(x =>
            {
                PurchaseorderData.ExecuteQuery(x, new SortedList<string, object>());
            });

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo RangeTaxSalesLoss(string organizationid, string mode, string from, string to)
        {
            DateTime fromdate = DateTime.Now.AddMonths(-6);
            DateTime todate = DateTime.Now.AddDays(1);
            if (!string.IsNullOrEmpty(from))
                fromdate = BaseEntity.GetDataFromString(from, "dd-MM-yyyy");

            if (!string.IsNullOrEmpty(to))
                todate = BaseEntity.GetDataFromString(to, "dd-MM-yyyy").AddDays(1);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
                                TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");
            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };

            if (string.IsNullOrEmpty(mode) || mode == "SALES")
            {

                sb.AppendLine("<H4>Sales Loss</H4>");
                sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>", "Bill Date", "Barcode", "Price", "Qty", "Loss"));
                List<string> sbcommand = new List<string>();
                string query = @" with salesloss
            as
            (
            select bill.billdate,billitems.barcode,billitems.receivable/billitems.qty rate,billitems.qty from bill
            inner join billitems on billitems.billid = bill.id and billitems.isactive=true
            inner join products on products.id = billitems.productid
            inner join tax on tax.id=products.salestaxid and tax.type=2
            where bill.isactive=true and billtype != 9
            and billitems.receivable/billitems.qty>2624 and 
            billitems.receivable/billitems.qty<2950 and billitems.qty>0
            and billitems.taxpercentage = 18
            and billitems.sellingmode!=3
            and billdate>=@fromdate and billdate<@todate 
            select salesloss.*,round(2499-(rate/118*100),2)*qty lossvalue from salesloss
			where order by billdate desc";
                decimal lossvalue = 0;
                var param = new SortedList<string, object>();
                param.Add("fromdate", fromdate);
                param.Add("todate", todate);
                using (var reader = PurchaseorderData.ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        lossvalue += (Convert.ToDecimal(reader[3]) * Convert.ToDecimal(reader[4]));
                        sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                            Convert.ToDateTime(reader[0]).ToString("dd-MM-yyyy"),
                            reader[1].ToString(),
                            Convert.ToDecimal(reader[2]).ToString("F2"),
                            Convert.ToDecimal(reader[3]).ToString("F2"),
                            Convert.ToDecimal(reader[4]).ToString("F2")));
                    }
                }
                sb.AppendLine("</table>");
                sb.AppendLine("<H4>Loss Value</H4>");
                sb.AppendLine("<H4>" + lossvalue.ToString("F2") + "</H4>");
            }
            if (string.IsNullOrEmpty(mode) || mode == "PRICE")
            {
                sb.AppendLine("<H4>Price Loss</H4>");
                sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>",
                    "Loss", "Price", "Discount", "Barcode", "Stock"));
                var query = @" select round(2499-((salablegoods.price-salablegoods.discount)/118*100),2) lasvalue,
            salablegoods.price,salablegoods.discount,salablegoods.barcode,
			iqty-(oqty+jqty+dqty) stock from stock 
            inner join salablegoods  on salablegoods.stockid = stock.id
            inner join products on products.id = stock.productid
			inner join tax on tax.id=products.salestaxid and tax.type=2
            where (salablegoods.price-salablegoods.discount)>2624 
            and (salablegoods.price-salablegoods.discount)<2950 and stock.isactive=true
            and salablegoods.hasstock=true and salablegoods.isactive=true and iqty-(oqty+jqty+dqty)>0";
                using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                            Convert.ToDecimal(reader[0]).ToString("F2"),
                            Convert.ToDecimal(reader[1]).ToString("F2"),
                            Convert.ToDecimal(reader[2]).ToString("F2"),
                            reader[3].ToString(),
                            Convert.ToDecimal(reader[4]).ToString("F2")));
                    }
                }
                sb.AppendLine("</table>");
            }

            sb.AppendLine("</BODY></HTML>");

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo uanSyncNegativeStock(string organizationid)
        {
            return uanSyncNegativeStock(organizationid, null);
        }
        public FileContentInfo uanSyncNegativeStock(string organizationid,string uan)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>UAN Negative</H4>");


            ImageData imageData = new ImageData() { Context = new UserClaim() { OrganizationID = long.Parse("0") } };
            var executingquery = @"with allnegativestock
            as
            (
            select salablegoods.barcode,salablegoods.locationid,serialno,(iqty-(oqty+dqty+jqty+aqty)) stk from salablegoods
            inner join stock on stock.id = stockid and stock.serialno != ''
            where iqty-(oqty+dqty+jqty+aqty)<0
            ),alluan as
            (
	            select distinct serialno from allnegativestock
            )
            ,totalstock as
            (
            select stock.serialno,sum(iqty-(oqty+dqty+jqty+aqty)) stk from salablegoods
            inner join stock on stock.id = stockid and stock.serialno != '' 
            inner join allnegativestock on allnegativestock.serialno = stock.serialno
	            where stock.isactive=true and salablegoods.isactive=true
            group by stock.serialno
            ),allpositive as
            (
            select * from totalstock
            where stk>=0
            )
            select salablegoods.id,salablegoods.barcode,allpositive.*,iqty,oqty,jqty,aqty,dqty,
            iqty-(oqty+jqty+aqty+dqty) itemstockqty  from salablegoods
            inner join stock on stock.id = stockid and stock.serialno != ''
            inner join allpositive on allpositive.serialno = stock.serialno
            order by stock.serialno";
            if(!string.IsNullOrEmpty(uan)) executingquery = executingquery.Replace("stock.serialno != ''", "stock.serialno != '" + uan + "'");
            var dset = imageData.ExecuteDataSet(executingquery, null);

            List<Tuple<long, string, decimal>> iqtychanges = new List<Tuple<long, string, decimal>>();
            SortedDictionary<long, decimal> adjusted = new SortedDictionary<long, decimal>();
            foreach (var items in dset.Tables[0].AsEnumerable().GroupBy(x => x["serialno"].ToString()))
            {
                var allitems = items.ToList().ConvertAll(x => new
                {
                    id = Convert.ToInt64(x["id"]),
                    barcode = Convert.ToString(x["barcode"]),
                    itemstockqty = x["itemstockqty"] == DBNull.Value ? 0 : Convert.ToInt64(x["itemstockqty"]),
                    iqty = x["iqty"] == DBNull.Value ? 0 : Convert.ToInt64(x["iqty"])
                });
                var allnegative = allitems.FindAll(x => x.itemstockqty < 0);
                foreach (var negative in allnegative)
                {
                    decimal required = negative.itemstockqty * -1;
                    var allpositive = allitems.FindAll(x => x.iqty > 0 && x.itemstockqty > 0).OrderByDescending(x => x.itemstockqty).ToList();
                    for (var i = 0; required > 0 && i < allpositive.Count; i++)
                    {
                        var adjustedqty = adjusted.ContainsKey(allpositive[i].id) ? adjusted[allpositive[i].id] : 0;
                        if ((allpositive[i].itemstockqty - adjustedqty) > required &&
                            (allpositive[i].iqty > required))
                        {
                            iqtychanges.Add(new Tuple<long, string, decimal>(negative.id, negative.barcode, required));
                            iqtychanges.Add(new Tuple<long, string, decimal>(allpositive[i].id, allpositive[i].barcode, required * -1));
                            if (adjusted.ContainsKey(allpositive[i].id))
                                adjusted[allpositive[i].id] += required;
                            else
                                adjusted.Add(allpositive[i].id, required);
                            required = 0;
                        }
                        else if ((allpositive[i].itemstockqty - adjustedqty) > 0)
                        {
                            var adjqty = (allpositive[i].itemstockqty - adjustedqty);
                            if (adjqty > allpositive[i].iqty)
                                adjqty = allpositive[i].iqty;

                            iqtychanges.Add(new Tuple<long, string, decimal>(negative.id, allpositive[i].barcode, adjqty));
                            iqtychanges.Add(new Tuple<long, string, decimal>(allpositive[i].id, allpositive[i].barcode, adjqty * -1));
                            if (adjusted.ContainsKey(allpositive[i].id))
                                adjusted[allpositive[i].id] += adjqty;
                            else
                                adjusted.Add(allpositive[i].id, adjqty);
                            required -= adjqty;
                        }
                    }
                }
            }

            if (iqtychanges.Count > 0)
            {
                using (var connection = imageData.CreateConnection())
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    foreach (var itm in iqtychanges)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("id", itm.Item1);
                        param.Add("iqty", itm.Item3);
                        imageData.ExecuteQuery("update salablegoods set iqty=iqty+@iqty where id=@id", param, connection, transaction);
                    }
                    transaction.Commit();
                }
                sb.AppendLine(String.Join("<br>", iqtychanges.ConvertAll(x => x.Item2 + " - " + x.Item1 + " - " + x.Item3)));
            }

            executingquery = @"with negative
                    as
                    (
                                select salablegoods.id,salablegoods.barcode,iqty,oqty,jqty,aqty,dqty,
                                iqty-(oqty+jqty+aqty+dqty) itemstockqty,stock.serialno  from salablegoods
                                inner join stock on stock.id = stockid and stock.serialno != ''
			                    where iqty-(oqty+jqty+aqty+dqty)<0
                                order by stock.serialno
                    )

                    select salablegoods.id,salablegoods.barcode,iqty,oqty,jqty,aqty,dqty,
                                iqty-(oqty+jqty+aqty+dqty) itemstockqty,stock.serialno  from salablegoods
                                inner join stock on stock.id = stockid and stock.serialno != ''
			                    where 
			                    --iqty-(oqty+jqty+aqty+dqty)>0 and 
			                    stock.serialno in (select serialno from negative)
                                order by stock.serialno
                    ";
            if (!string.IsNullOrEmpty(uan)) executingquery = executingquery.Replace("stock.serialno != ''", "stock.serialno != '" + uan + "'");
            dset = imageData.ExecuteDataSet(executingquery, null);

            iqtychanges = new List<Tuple<long, string, decimal>>();
            adjusted = new SortedDictionary<long, decimal>();
            foreach (var items in dset.Tables[0].AsEnumerable().GroupBy(x => x["serialno"].ToString()))
            {
                var allitems = items.ToList().ConvertAll(x => new
                {
                    id = Convert.ToInt64(x["id"]),
                    barcode = Convert.ToString(x["barcode"]),
                    itemstockqty = Convert.ToInt64(x["itemstockqty"]),
                    iqty = Convert.ToInt64(x["iqty"]),
                    jqty = Convert.ToInt64(x["jqty"]),
                });
                var allnegative = allitems.FindAll(x => x.itemstockqty < 0);
                foreach (var negative in allnegative)
                {
                    decimal required = negative.itemstockqty * -1;
                    var allpositive = allitems.FindAll(x => x.itemstockqty > 0).OrderByDescending(x => x.itemstockqty).ToList();
                    for (var i = 0; required > 0 && i < allpositive.Count; i++)
                    {
                        var adjustedqty = adjusted.ContainsKey(allpositive[i].id) ? adjusted[allpositive[i].id] : 0;
                        if ((allpositive[i].itemstockqty - adjustedqty) > required)
                        {
                            iqtychanges.Add(new Tuple<long, string, decimal>(negative.id, negative.barcode, required * -1));
                            iqtychanges.Add(new Tuple<long, string, decimal>(allpositive[i].id, allpositive[i].barcode, required));
                            if (adjusted.ContainsKey(allpositive[i].id))
                                adjusted[allpositive[i].id] += required;
                            else
                                adjusted.Add(allpositive[i].id, required);
                            required = 0;
                        }
                        else if ((allpositive[i].itemstockqty - adjustedqty) > 0)
                        {
                            var adjqty = (allpositive[i].itemstockqty - adjustedqty);

                            iqtychanges.Add(new Tuple<long, string, decimal>(negative.id, allpositive[i].barcode, adjqty * -1));
                            iqtychanges.Add(new Tuple<long, string, decimal>(allpositive[i].id, allpositive[i].barcode, adjqty));
                            if (adjusted.ContainsKey(allpositive[i].id))
                                adjusted[allpositive[i].id] += adjqty;
                            else
                                adjusted.Add(allpositive[i].id, adjqty);
                            required -= adjqty;
                        }
                    }
                    if (required > 0)
                    {
                        iqtychanges.Add(new Tuple<long, string, decimal>(negative.id, negative.barcode, required * -1));
                    }
                }
            }

            if (iqtychanges.Count > 0)
            {
                using (var connection = imageData.CreateConnection())
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    foreach (var itm in iqtychanges)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("id", itm.Item1);
                        param.Add("jqty", itm.Item3);
                        imageData.ExecuteQuery("update salablegoods set jqty=jqty+@jqty where id=@id", param, connection, transaction);
                    }

                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService();
                    executingquery = @"with tran
                    as
                    (
                    select sum(qty) stk,max(createdon) modifiedtransaction,salablegoodsid from stocktransaction
                                where salablegoodsid>0 
                                group by salablegoodsid
                    )
                    select salablegoods.createdon,salablegoods.createdby,modifiedtransaction,
                    salablegoods.id,salablegoods.jqty,salablegoods.iqty,salablegoods.oqty,
                    stk,iqty-(oqty+dqty+jqty) actualstk,salablegoods.locationid,salablegoods.stockid from salablegoods
                    inner join tran on tran.salablegoodsid = salablegoods.id
                    inner join stock on stock.id = stockid and stock.serialno != ''
                    where stk != (iqty-(oqty+dqty+jqty))";
                    if (!string.IsNullOrEmpty(uan)) executingquery = executingquery.Replace("stock.serialno != ''", "stock.serialno != '" + uan + "'");
                    dset = imageData.ExecuteDataSet(executingquery, new SortedList<string, object>(), connection, transaction);
                    foreach (var item in dset.Tables[0].AsEnumerable())
                    {
                        var createdon = Convert.ToDateTime(item["createdon"]);
                        var createdby = Convert.ToInt64(item["createdby"]);
                        var modifiedtransaction = Convert.ToDateTime(item["modifiedtransaction"]);
                        var id = Convert.ToInt64(item["id"]);
                        var stockid = Convert.ToInt64(item["stockid"]);
                        var locationid = Convert.ToInt64(item["locationid"]);
                        var jqty = Convert.ToDecimal(item["jqty"]);
                        var iqty = Convert.ToDecimal(item["iqty"]);
                        var oqty = Convert.ToDecimal(item["oqty"]);
                        var stk = Convert.ToDecimal(item["stk"]);
                        var actualstk = Convert.ToDecimal(item["actualstk"]);
                        if (actualstk - stk > 0)
                        {
                            stocktransactionDataService.Insert(new Stocktransaction()
                            {
                                createdby = createdby,
                                createdon = createdon,
                                locationid = locationid,
                                reftype = "AU",
                                salablegoodsid = id,
                                stockid = stockid,
                                type = Stocktransaction.TransactionType.Audit,
                                transactedon = modifiedtransaction,
                                qty = actualstk - stk
                            });
                        }
                        else
                        {
                            stocktransactionDataService.Insert(new Stocktransaction()
                            {
                                createdby = createdby,
                                createdon = createdon,
                                locationid = locationid,
                                reftype = "AU",
                                salablegoodsid = id,
                                stockid = stockid,
                                type = Stocktransaction.TransactionType.Audit,
                                transactedon = modifiedtransaction,
                                qty = (stk - actualstk) * -1
                            });
                        }
                    }

                    transaction.Commit();
                }
                sb.AppendLine(String.Join("<br>", iqtychanges.ConvertAll(x => x.Item2 + " - " + x.Item1 + " - " + x.Item3)));
            }

            /*with negativejournals
                as
                (
                select salablegoods.iqty,salablegoods.oqty,salablegoods.jqty,salablegoods.dqty,
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stkcurrent,
                stockid,salablegoods.id,salablegoods.locationid
                from salablegoods
                where salablegoods.iqty-salablegoods.oqty<0
                ),cnf as
                (
	                select distinct locationid,stockid from negativejournals
                ),hasnegat as
                (
                select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)),
                cnf.locationid,cnf.stockid
                from salablegoods
                inner join cnf on cnf.stockid = salablegoods.stockid
                and cnf.locationid = salablegoods.locationid
                group by cnf.locationid,cnf.stockid
                having sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty))<0
                ),fixable as
                (
	                select 
                cnf.locationid,cnf.stockid
                from salablegoods
                inner join cnf on cnf.stockid = salablegoods.stockid
                and cnf.locationid = salablegoods.locationid
                group by cnf.locationid,cnf.stockid
                having sum(salablegoods.iqty-(salablegoods.oqty))>=0
                )
                ,autofixrows 
                as
                (
	                select salablegoods.id,salablegoods.iqty,salablegoods.oqty,salablegoods.jqty,salablegoods.dqty,
	                fixable.locationid,fixable.stockid
	                from salablegoods
                inner join fixable on fixable.stockid = salablegoods.stockid
                and fixable.locationid = salablegoods.locationid
                )
                select * from fixable
                */

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo ResetSellableBarcodeQty()
        {
            BaseData imageData = new BaseData();
            var noofrows = imageData.ExecuteQuery(@"with sellstk
as
(
select invoiceitemid,sum(qty) bqty from stock
where isactive=true
group by invoiceitemid
),missingbqty as
(
select lrinvoiceitems.sellableqty,buyingqty,returnqty, bqty,id from lrinvoiceitems
left outer join sellstk on sellstk.invoiceitemid = lrinvoiceitems.id
where
lrinvoiceitems.sellableqty != coalesce( bqty,0)
)

update lrinvoiceitems set sellableqty = bqty from missingbqty
where missingbqty.id = lrinvoiceitems.id
            ", null);

            noofrows += imageData.ExecuteQuery(@"with sellstk
as
(
select invoiceitemid,sum(qty) bqty from stock
where isactive=true
group by invoiceitemid
),missingbqty as
(
select lrinvoiceitems.sellableqty,buyingqty,returnqty, bqty,id from lrinvoiceitems
left outer join sellstk on sellstk.invoiceitemid = lrinvoiceitems.id
where
lrinvoiceitems.sellableqty != coalesce( bqty,0)
)

update salablegoods set isactive = true where id in (
select salablegoods.id from missingbqty
inner join stock on stock.invoiceitemid = missingbqty.id and stock.isactive = false 
inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive=false
where bqty is null)", null);

            noofrows += imageData.ExecuteQuery(@"with sellstk
as
(
select invoiceitemid,sum(qty) bqty from stock
where isactive=true
group by invoiceitemid
),missingbqty as
(
select lrinvoiceitems.sellableqty,buyingqty,returnqty, bqty,id from lrinvoiceitems
left outer join sellstk on sellstk.invoiceitemid = lrinvoiceitems.id
where
lrinvoiceitems.sellableqty != coalesce( bqty,0)
)

update stock set isactive = true where id in (
select stock.id from missingbqty
inner join stock on stock.invoiceitemid = missingbqty.id and stock.isactive = false where bqty is null)", null);


            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("Affected rows " + noofrows)));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo ResetNegativeRetailStocks()
        {
            BaseData imageData = new BaseData();
            var noofrows = imageData.ExecuteQuery(@"with allnegativestocks 
            as
            (
            select iqty-(oqty+jqty+dqty) currentstock,
            (iqty-(oqty+jqty+dqty))+jqty newjqty,iqty,oqty,jqty,dqty,aqty,id,barcode from salablegoods 
            where 
            iqty-(oqty+jqty+dqty)<0
            )

            update salablegoods set jqty=newjqty from allnegativestocks
            where allnegativestocks.id = salablegoods.id
            ", null);

            noofrows += imageData.ExecuteQuery(@"		   
		   with allnegativestocks as 
		   (
		   select (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)),
		   (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty))+journalqty newjqty,
		   journalqty,id,barcode,locationid from stock 
		   where stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)<0
		   )
		   update stock set journalqty=newjqty from allnegativestocks
           where allnegativestocks.id = stock.id
            ", null);

            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("Affected rows " + noofrows)));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo KillIdleSession()
        {
            string query = @"SELECT
                        pg_terminate_backend(pid)
                        FROM pg_stat_activity
                        WHERE query != 'listen dbchange' 
						and not application_name ilike 'pgA%' and (now() - pg_stat_activity.query_start) > interval '30 seconds'
						and state = 'idle' and usename = 'postgres'";

            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = 0 } };
            var rows = PurchaseorderData.ExecuteQuery(query, null);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
            </style></HEAD><BODY>");
            sb.AppendLine("<H4>Killing Idle connections " + rows + "</H4>");
            sb.AppendLine("</BODY></HTML>");
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }

        public FileContentInfo KillrunningSQL(string organizationid, bool delete)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine(@"<HTML><HEAD><style>
TABLE {
                                                font-family: 'Trebuchet MS', Arial, Helvetica, sans-serif;
                                                border-collapse: collapse;
                                                width: 100%;
                                            }

                                            TABLE td, TABLE th {
                                                border: 1px solid #ddd;
                                                padding: 8px;
                                            }

                                            TABLE tr:nth-child(even){background-color: #f2f2f2;}

                                            TABLE tr:hover {background-color: #ddd;}

                                            TABLE th {
                                                padding-top: 12px;
                                                padding-bottom: 12px;
                                                text-align: left;
                                                background-color: #4CAF50;
                                                color: white;
                                            }
</style></HEAD><BODY>");

            sb.AppendLine("<H4>Long running queries</H4>");
            sb.AppendLine(string.Format("<table><tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th></tr>", "ProcessID", "Duration", "Query", "Status"));
            List<string> sbcommand = new List<string>();
            BaseData PurchaseorderData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            /*SELECT
   pg_terminate_backend(pid) 
FROM pg_stat_activity
WHERE (now() - pg_stat_activity.query_start) > interval '5 minutes';

SELECT pid, 
       usename, 
       pg_blocking_pids(pid) AS blocked_by, 
       QUERY AS blocked_query
FROM pg_stat_activity
WHERE cardinality(pg_blocking_pids(pid)) > 0;
            */

            string query = @"SELECT
                        pid,
                        now() - pg_stat_activity.query_start AS duration,
                        query,
                        state
                        FROM pg_stat_activity
                        WHERE query != 'listen dbchange'" + (delete ? " and not application_name ilike 'pgA%' and (now() - pg_stat_activity.query_start) > interval '10 minutes'" : "");
            using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>", reader[0].ToString(),
                        reader[1].ToString(), reader[2].ToString(), reader[3].ToString()));
                    sbcommand.Add(string.Format("SELECT pg_terminate_backend({0});", reader[0].ToString()));
                }
            }
            query = @"SELECT a.datname,
         cast(l.relation::regclass as varchar),
         l.transactionid,
         l.mode,
         l.GRANTED,
         a.usename,
         a.query,
         a.query_start,
         age(now(), a.query_start) AS age,
         a.pid
FROM pg_stat_activity a
JOIN pg_locks l ON l.pid = a.pid
where age(now(), a.query_start) > interval '3 minutes'
ORDER BY a.query_start;";

            sb.AppendLine("</table><h4>Long row/table lock queries</h4><table>");
            sb.AppendLine(string.Format("<tr><th>{0}</th><th>{1}</th><th>{2}</th><th>{3}</th><th>{4}</th></tr>", "ProcessID", "Type", "Mode", "Query", "Duration"));
            using (var reader = PurchaseorderData.ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>", reader[9].ToString(), reader[1].ToString(),
                        reader[3].ToString(), reader[6].ToString(), reader[8].ToString()));
                    sbcommand.Add(string.Format("SELECT pg_terminate_backend({0});", reader[9].ToString()));
                }
            }
            sb.AppendLine("</table></BODY></HTML>");
            if (delete)
            {
                sbcommand.ForEach(x =>
                {
                    PurchaseorderData.ExecuteQuery(x, new SortedList<string, object>());
                });
            }
            var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(sb.ToString())));
            fci.FileName = "result.html";
            fci.Extension = "text/html";
            return fci;
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadOfflineLicense/{organizationid}/{locationid}")]
        public async Task<IHttpActionResult> DownloadLicense(string organizationid, string locationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadOfflineLicense(organizationid, locationid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(true, "License.key", byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo DownloadOfflineLicense(string organizationid, string locationid)
        {
            long refid;
            if (long.TryParse(locationid, out refid))
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
                var reflist = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    id = refid
                });
                if (reflist.Count == 1)
                {
                    string licensekey = reflist.First().additionalinfo.licensekey;
                    var offlineLicense = LicenseManager.Instance(0).GetLicense().Copy();
                    offlineLicense.key = licensekey;
                    var encryption = new WebAPI.Data.Cryptor.AESEncryption(offlineLicense.company, licensekey.Replace("-", ""));
                    var encrypted = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(encryption.EncryptToString(offlineLicense.ToJSON())));
                    return new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(encrypted)));
                }
            }

            return new FileContentInfo(new ByteArrayContent(new byte[0]));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("DownloadOfflineData/{organizationid}/{locationid}")]
        public async Task<IHttpActionResult> DownloadOfflineData(string organizationid, string locationid)
        {
            long location = 0;
            if (long.TryParse(locationid, out location))
            {
                InfyPOS.Processors.OfflineManager offlineManager = new OfflineManager(location);
                offlineManager.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => offlineManager.CreateMasterFile());
                if (byteArray == null)
                    return this.NotFound();
                return new FileActionResult(true, byteArray.FileName, byteArray.Extension, byteArray.Content);
            }
            return this.NotFound();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Download/{key}")]
        public async Task<IHttpActionResult> DownloadFile(string key)
        {
            if (key.IndexOf('.') > 0)
                key = key.Substring(0, key.IndexOf('.'));
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => Downloadfile(key));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(true, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DownloadSchema/{organizationid}")]
        public async Task<IHttpActionResult> DownloadSchema(string organizationid)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadSchema(long.Parse(organizationid)));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(true, "Schema.data", byteArray.Extension, byteArray.Content);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("DownloadData/{organizationid}/{locationid}")]
        public async Task<IHttpActionResult> DownloadData(string organizationid, string locationid)
        {
            long location = 0;
            if (long.TryParse(locationid, out location))
            {
                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadData(long.Parse(organizationid), location));
                if (byteArray == null)
                    return this.NotFound();
                return new FileActionResult(true, byteArray.FileName, byteArray.Extension, byteArray.Content);
            }
            return this.NotFound();
        }



        [HttpGet]
        [Route("UploadData/{organizationid}/{imageKey}")]
        public async Task<IHttpActionResult> UploadData(string organizationid, string imageKey)
        {
            if (imageKey.IndexOf('.') > 0)
                imageKey = imageKey.Substring(0, imageKey.IndexOf('.'));

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => UploadDataToDB(organizationid, imageKey));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }



        //[AllowAnonymous]
        //[HttpGet]
        //[Route("DownloadCommand/{organizationid}")]
        //public async Task<IHttpActionResult> DownloadCommand(string organizationid)
        //{
        //    FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => DownloadCommandJSON(organizationid));
        //    if (byteArray == null)
        //        return this.NotFound();
        //    return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        //}


        [AllowAnonymous]
        [HttpGet]
        [Route("UpgradeSchema/{organizationid}/{imageKey}")]
        public async Task<IHttpActionResult> UpgradeSchema(string organizationid, string imageKey)
        {
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadSchema(organizationid, imageKey));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Runcommand/{organizationid}/{imageKey}")]
        public async Task<IHttpActionResult> Runcommand(string organizationid, string imageKey)
        {
            return this.Ok(Processcommand(organizationid, imageKey));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("resultcommand/{organizationid}/{guid}")]
        public async Task<IHttpActionResult> resultcommand(string organizationid, string guid)
        {
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() => InfyPOS.Processors.ProcessManager.Instance(long.Parse(organizationid)).GetStatus(guid));
            if (status.result != null)
            {
                var responsstring = status.result.ToString();
                return new FileActionResult(false, "response.html", "html", new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(responsstring)));
            }
            return new FileActionResult(false, "response.html", "html", new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes("Process failed, try again")));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Thumbnail/{organizationid}/{imageKey}")]
        public async Task<IHttpActionResult> Thumbnail(string organizationid, string imageKey)
        {
            Common.ImageData imageData = new Common.ImageData()
            {
                Context = new UserClaim() { OrganizationID = long.Parse(organizationid) }
            };

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadThumbnail(organizationid, imageKey));
            if (byteArray == null)
                return this.NotFound();

            // Generate response
            var response = new FileActionResult(true, false, byteArray.FileName, byteArray.Extension, byteArray.Content).response;

            // Set caching headers properly
            response.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(365 * 10)
            };

            // Set content-specific header (Expires must go here)
            if (response.Content != null)
            {
                response.Content.Headers.Expires = DateTimeOffset.UtcNow.AddYears(10);
            }

            response.Headers.Pragma.Clear();

            return new System.Web.Http.Results.ResponseMessageResult(response);

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PDFReports/{organizationid}/{fileid}")]
        public async Task<IHttpActionResult> PDFReports(string organizationid, string fileid)
        {
            if (fileid.IndexOf('.') > 0)
                fileid = fileid.Substring(0, fileid.IndexOf('.'));

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadPDF(organizationid, fileid));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("MobileLogin/{organizationid}/{id}")]
        public async Task<IHttpActionResult> MobileLogin(string organizationid, string id)
        {
            BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            string host = string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(baseData.Context.OrganizationID).Config.hostedserver) ?
                this.Request.RequestUri.AbsoluteUri.Substring(0, this.Request.RequestUri.AbsoluteUri.IndexOf("/File/")) : InfyPOS.Services.ConfigManager.Instance(baseData.Context.OrganizationID).Config.hostedserver;
            string qrcode = string.Format("{0}/UsersService/Connect/{2}/{1}", host, id, organizationid);
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => mobilelogin(qrcode));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
        }

        private FileContentInfo mobilelogin(string id)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(id, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(8, System.Drawing.Color.Black, System.Drawing.Color.White, true);
            FileContentInfo fileContentInfo = null;
            using (MemoryStream ms = new MemoryStream())
            {
                qrCodeImage.Save(ms, ImageFormat.Png);
                fileContentInfo = new FileContentInfo(new ByteArrayContent(ms.ToArray()));
            }
            fileContentInfo.FileName = "MobileLogin.PNG";
            fileContentInfo.Extension = "image/png";
            return fileContentInfo;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Download/{organizationid}/{imageKey}")]
        public async Task<IHttpActionResult> Download(string organizationid, string imageKey)
        {
            if (imageKey.IndexOf('.') > 0)
                imageKey = imageKey.Substring(0, imageKey.IndexOf('.'));

            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => ReadImage(organizationid, imageKey));
            if (byteArray == null)
                return this.NotFound();
            var far = new FileActionResult(true, false, byteArray.FileName, byteArray.Extension, byteArray.Content);
            return far;
        }

        [Route("Delete/{organizationid}/{imageKey}")]
        [HttpPost]
        public async Task<IHttpActionResult> Delete(string organizationid, string imageKey)
        {
            if (imageKey.IndexOf('.') > 0)
                imageKey = imageKey.Substring(0, imageKey.IndexOf('.'));
            bool deleted = await Task<List<string>>.Run(() => RemoveImage(organizationid, imageKey));
            return this.Ok(deleted);
        }

        public class ImageAsBase64
        {
            public long organizationid { get; set; }
            public string id { get; set; }
            public string base64 { get; set; }
            public string filename { get; set; }
        }

        public class BarcodeFile
        {
            public string idlist { get; set; }
            public bool log { get; set; }
            public long id { get; set; }
            public string content { get; set; }
            public string filename { get; set; }
        }


        [AllowAnonymous]
        [Route("RunScript/{organizationid}/{command}")]
        [HttpGet]
        public async Task<IHttpActionResult> RunScript(string organizationid, string command)
        {
            ScriptCollection scriptCollection = new ScriptCollection();
            scriptCollection.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            var status = await Task<bool>.Run(() => scriptCollection.RunScript(command));
            return this.Ok(status);
        }


        [AllowAnonymous]
        [Route("MigrateStatus/{organizationid}/{guid}")]
        [HttpGet]
        public async Task<IHttpActionResult> MigrateStatus(string organizationid, string guid)
        {
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() => InfyPOS.Processors.Migration.MigrationManager.Instance(long.Parse(organizationid)).GetStatus(guid));
            return this.Ok(status);
        }



        [Route("MigrateData")]
        [HttpPost]
        public async Task<IHttpActionResult> MigrateData(InfyPOS.Processors.Migration.MigrationData data)
        {
            BaseData baseData = new BaseData();
            if (baseData.IsSAAS && data.organizationid > 0)
                baseData.Context.OrganizationID = data.organizationid;
            InfyPOS.Processors.Migration.ProgressStatus status = await Task<InfyPOS.Processors.Migration.ProgressStatus>.Run(() => StartMigration(baseData.Context.OrganizationID, data));
            return this.Ok(status);
        }

        public class SMSInfo
        {
            public string template { get; set; }
            public bool ispromotional { get; set; }
            public bool excludecustomerhassale { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public string smsurl { get; set; }
            public long scheduleid { get; set; }
            public List<Item> items { get; set; }
            public class Item
            {
                public string no { get; set; }
                public string name { get; set; }
            }
        }
        [Route("SendSMS")]
        [HttpPost]
        public async Task<IHttpActionResult> SendSMS(SMSInfo data)
        {
            bool status = await Task<bool>.Run(() => sendSMS(data));
            return this.Ok(status);
        }

        [Route("SendCampaign")]
        [HttpPost]
        public async Task<IHttpActionResult> SendCampaign(SMSInfo data)
        {
            bool status = await Task<bool>.Run(() => StartCampaign(data));
            return this.Ok(status);
        }

        private bool StartCampaign(SMSInfo data)
        {
            BaseData baseData = new BaseData();
            ScheduleManager.Instance.StartCampaign(baseData.Context.OrganizationID, data.scheduleid, data.items.ConvertAll(x => new Tuple<string, string>(x.no, x.name)));
            return true;
        }
        private bool sendSMS(SMSInfo data)
        {
            List<Customer> lstcustomer = new List<Customer>();
            if (data.excludecustomerhassale && data.fromdate != DateTime.MinValue && data.todate != DateTime.MinValue)
            {
                CustomerDataService customerdata = new CustomerDataService();
                lstcustomer = customerdata.SearchMyCustomer(new CustomerCreteria()
                {
                    fromdate = data.fromdate,
                    todate = data.todate
                });
            }

            if (data.items == null || data.items.Count == 0)
                throw new Exception("Please select the customer");
            if (string.IsNullOrEmpty(data.template))
                throw new Exception("Please select/enter the message template");

            BaseData baseData = new BaseData();
            foreach (var item in data.items)
            {
                if (!data.excludecustomerhassale || !lstcustomer.Exists(e => e.mobileno == item.no))
                {
                    var message = data.template.Replace("{name}", item.name).Replace("[name]", item.name).Replace("[NAME]", item.name);
                    if (string.IsNullOrEmpty(data.smsurl))
                    {
                        if (data.ispromotional)
                            InfyPOS.Services.SMSManager.Instance.SendSMS(baseData.Context.OrganizationID, InfyPOS.Services.ConfigManager.Instance(baseData.Context.OrganizationID).Config.sms_prom_senderid, item.no, message);
                        else
                            InfyPOS.Services.SMSManager.Instance.SendSMS(baseData.Context.OrganizationID, InfyPOS.Services.ConfigManager.Instance(baseData.Context.OrganizationID).Config.sms_tran_senderid, item.no, message);
                    }
                    else
                        InfyPOS.Services.SMSManager.Instance.SendSMSURLSchedule(baseData.Context.OrganizationID, 0, data.smsurl, item.no, message);
                }
            }
            return true;
        }

        private InfyPOS.Processors.Migration.ProgressStatus StartProcess(long organizationid,
            InfyPOS.Processors.ProcessManager.ProcessData data)
        {

            return InfyPOS.Processors.ProcessManager.Instance(organizationid).Start(data);
        }

        private InfyPOS.Processors.Migration.ProgressStatus StartMigration(long organizationid, InfyPOS.Processors.Migration.MigrationData data)
        {
            return InfyPOS.Processors.Migration.MigrationManager.Instance(organizationid).StartMigration(data);
        }


        [Route("UploadBase64")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadBase64(ImageAsBase64 data)
        {
            Common.ImageData imageData = new Common.ImageData();
            string imageidentifier = await Task<List<string>>.Run(() => StoreImage(imageData.Context, data));
            return this.Ok(imageidentifier);
        }

        [Route("BarcodeFileData/{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> BarcodeFileData(string id)
        {
            Common.ImageData imageData = new Common.ImageData();
            BarcodeFile imageidentifier = await Task<bool>.Run(() => GetBarcodeFile(imageData.Context, long.Parse(id)));
            return this.Ok(imageidentifier);
        }

        [Route("SaveBarcodeFile")]
        [HttpPost]
        public async Task<IHttpActionResult> SaveBarcodeFile(BarcodeFile data)
        {
            Common.ImageData imageData = new Common.ImageData();
            long imageidentifier = await Task<long>.Run(() => StoreBarcodeFile(imageData.Context, data));
            return this.Ok(imageidentifier);
        }

        [Route("DeleteBarcodeFile/{id}")]
        [HttpPost]
        public async Task<IHttpActionResult> DeleteBarcodeFile(string id)
        {
            Common.ImageData imageData = new Common.ImageData();
            bool imageidentifier = await Task<bool>.Run(() => imageData.Remove(long.Parse(id)));
            return this.Ok(imageidentifier);
        }


        [Route("ListBarcodeFile/{organizationid}/{all}/{locationid}")]
        [HttpGet]
        public async Task<IHttpActionResult> ListBarcodeFile(string organizationid, string all, string locationid)
        {
            Common.ImageData imageData = new ImageData();
            bool allfile = !string.IsNullOrEmpty(all) && all.ToLower() == "true";
            var promolist = imageData.SearchMyDocument(allfile, locationid);
            return this.Ok(promolist);
        }

        [Route("DownloadBarcodeFile")]
        [HttpPost]
        public async Task<IHttpActionResult> DownloadBarcodeFile(BarcodeFile barcodefile)
        {
            Common.ImageData imageData = new Common.ImageData();

            DataTable imageidentifier = null;

            if (barcodefile.log)
                imageidentifier = await Task<DataTable>.Run(() => imageData.GetBarcodeLogTable(barcodefile.idlist.Split('_')));
            else
                imageidentifier = await Task<DataTable>.Run(() => imageData.GetBarcodeTable(barcodefile.idlist.Split('_')));
            using (System.IO.MemoryStream memoryStream = new MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(imageidentifier, "StockAudit-" + DateTime.Now.ToString("dd-MM-yyyy hhmm tt"));
                xLWorkbook.SaveAs(memoryStream);
                return new FileActionResult(true, "StockAudit-" + DateTime.Now.ToString("dd-MM-yyyy"),
                    "application/vnd.ms-excel", new ByteArrayContent(memoryStream.ToArray()));
            }
        }


        [Route("DownloadBarcodeFile/{organizationid}/{id}")]
        [HttpGet]
        public async Task<IHttpActionResult> DownloadBarcodeFile(string organizationid, string id)
        {
            Common.ImageData imageData = new Common.ImageData();
            DataTable imageidentifier = await Task<DataTable>.Run(() => imageData.GetBarcodeTable(id.Split('_')));
            using (System.IO.MemoryStream memoryStream = new MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(imageidentifier, "StockAudit-" + DateTime.Now.ToString("dd-MM-yyyy hhmm tt"));
                xLWorkbook.SaveAs(memoryStream);
                return new FileActionResult(true, "StockAudit-" + DateTime.Now.ToString("dd-MM-yyyy"),
                    "application/vnd.ms-excel", new ByteArrayContent(memoryStream.ToArray()));
            }
        }

        [Route("DownloadBarcodeFile/{id}")]
        [HttpPost]
        public async Task<IHttpActionResult> DownloadBarcodeFile(string id)
        {
            Common.ImageData imageData = new Common.ImageData();
            ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() => imageData.GetMyDocument(long.Parse(id)));

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            foreach (var rows in imageidentifier.rows)
            {
                dt.Rows.Add(rows.values[0]);
            }

            using (System.IO.MemoryStream memoryStream = new MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, imageidentifier.filename);
                xLWorkbook.SaveAs(memoryStream);
                return new FileActionResult(true, imageidentifier.filename, "application/vnd.ms-excel", new ByteArrayContent(memoryStream.ToArray()));
            }
        }

        [Route("UploadBarcodeFile")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadBarcodeFile(Common.ActionRequest<ExcelDocument> document)
        {
            Common.ImageData imageData = new Common.ImageData();
            string imageidentifier = await Task<string>.Run(() => StoreUserBarcodeFile(imageData.Context, document.Item));
            return this.Ok(imageidentifier);
        }

        private string StoreUserBarcodeFile(UserClaim Context, ExcelDocument document)
        {
            Common.ImageData imageData = new Common.ImageData();
            Common.ImageData.File file = new Common.ImageData.File();
            file.createdby = imageData.Context.UserID;
            file.modifiedby = imageData.Context.UserID;
            file.createdon = Context.DateTimeNow;
            file.data = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document));
            file.ext = "Barcode";
            file.filename = document.filename;
            file.isactive = true;
            file.type = "Barcode";
            imageData.Insert(file);
            return file.filename;
        }

        [AllowAnonymous]
        [Route("UploadDirect/{organizationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadDirect(string organizationid, string type)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            List<string> imageidentifier = await Task<List<string>>.Run(() => StoreMultipleImage(imageData.Context, organizationid, Request, type));
            return this.Ok(imageidentifier);
        }


        [AllowAnonymous]
        [Route("UploadPODesign/{organizationid}/{id}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadPODesign(string organizationid, string id)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            List<string> imageidentifier = await Task<List<string>>.Run(() => StoreMultipleImage(imageData.Context, organizationid, Request, "png"));
            PurchaseorderDataService purchaseorderDataService = new PurchaseorderDataService();
            purchaseorderDataService.Context = imageData.Context;
            purchaseorderDataService.UpdateDesingImage(long.Parse(id), imageidentifier.First());
            return this.Ok(imageidentifier);
        }

        [AllowAnonymous]
        [Route("UploadXLSX/{organizationid}/{type}/{hasColumn=FALSE}/{sheetname=null}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadXLSX(string organizationid, string type, string hasColumn, string sheetname)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() => Parse(organizationid, Request, hasColumn, type, sheetname));
            return this.Ok(imageidentifier);
        }

        [AllowAnonymous]
        [Route("UploadStockXSLX/{organizationid}/{type}/{hasColumn=FALSE}/{sheetname=null}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadStockXSLX(string organizationid, string type, string hasColumn, string sheetname)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() =>
            {
                var exceldocument = Parse(organizationid, Request, hasColumn, type, sheetname);
                if (exceldocument.columns.Count > 2 && exceldocument.columns.Exists(x => x.ToLower().StartsWith("design")))
                {
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = imageData.Context };
                    return mfgdesignDataService.ConvertToUID(exceldocument);
                }
                return exceldocument;
            });
            return this.Ok(imageidentifier);
        }


        [AllowAnonymous]
        [Route("UploadDesignSizeStock/{organizationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadDesignSizeStock(string organizationid, string type)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() =>
            {
                var exceldocument = Parse(organizationid, Request, "xslx", "", "");
                if (exceldocument.columns.Count > 2 && exceldocument.columns.Exists(x => x.ToLower().StartsWith("design")))
                {
                    StockDataService mfgdesignDataService = new StockDataService() { Context = imageData.Context };
                    return mfgdesignDataService.ConvertDesign2Barcode(exceldocument);
                }
                return exceldocument;
            });
            return this.Ok(imageidentifier);
        }

        [AllowAnonymous]
        [Route("UploadDesignStock/{organizationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadDesignStock(string organizationid, string type)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() =>
            {
                var exceldocument = Parse(organizationid, Request, "xslx", "", "");
                if (exceldocument.columns.Count > 2 && exceldocument.columns.Exists(x => x.ToLower().StartsWith("design")))
                {
                    StockDataService mfgdesignDataService = new StockDataService() { Context = imageData.Context };
                    return mfgdesignDataService.Convert2Barcode(exceldocument);
                }
                return exceldocument;
            });
            return this.Ok(imageidentifier);
        }

        [AllowAnonymous]
        [Route("UploadBOMXSLX/{organizationid}/{locationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadBOMXSLX(string organizationid, string locationid, string type)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            List<Models.BOMConsumptionitemsDetail> imageidentifier = await Task<ExcelDocument>.Run(() =>
            {
                var exceldocument = Parse(organizationid, Request, null, type, null);
                BOMConsumptionitemsDataService BOMConsumptionDataService = new BOMConsumptionitemsDataService() { Context = imageData.Context };
                return BOMConsumptionDataService.ConverttoItems(locationid, exceldocument);
            });
            return this.Ok(imageidentifier);
        }

        [Route("UploadBillData")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadBillData(Common.ActionRequest<BillImportManager.BillSummary> request)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.BillImportManager billImportManager = new BillImportManager(imageData.Context.OrganizationID);
            InfyPOS.Processors.BillImportManager.BillSummary imageidentifier =
                await Task<InfyPOS.Processors.BillImportManager.BillSummary>.Run(() =>
                billImportManager.UploadData(request.Item, imageData));
            return this.Ok(imageidentifier);
        }


        [Route("DocumentProcess/{documenttype}/{filetype}")]
        [HttpPost]
        public async Task<IHttpActionResult> DocumentProcess(string documenttype, string filetype)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.ExcelImportManager billImportManager = new ExcelImportManager(imageData.Context.OrganizationID);
            if (documenttype == "PO")
            {
                InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier =
                    await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => billImportManager.ValidatePO(Request, -1, imageData));
                return this.Ok(imageidentifier);
            }
            else if (documenttype == "STO")
            {
                InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier =
                    await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => billImportManager.ValidateSTO(Request, -1, imageData));
                return this.Ok(imageidentifier);
            }
            else if (documenttype == "ECOM")
            {
                InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier =
                    await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => billImportManager.ValidateECOM(Request, -1, imageData));
                return this.Ok(imageidentifier);
            }
            else if (documenttype == "SKUImages")
            {
                InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier =
                    await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => billImportManager.ValidateSKUImages(Request, -1, imageData));
                return this.Ok(imageidentifier);
            }
            else if (documenttype == "SKU")
            {
                InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier =
                    await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => billImportManager.ValidateSKU(Request, -1, imageData));
                return this.Ok(imageidentifier);
            }
            else
            {
                InfyPOS.Processors.ExcelImportManager.DocumentStatus imageidentifier =
                    await Task<InfyPOS.Processors.ExcelImportManager.DocumentStatus>.Run(() => billImportManager.ValidateSO(Request, -1, imageData));
                return this.Ok(imageidentifier);
            }
        }

        [Route("ProcessProductAgreement/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> ProcessProductAgreement(string type)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.ExcelImportManager billImportManager = new ExcelImportManager(imageData.Context.OrganizationID);
            InfyPOS.Processors.ExcelImportManager.BillSummary imageidentifier =
                await Task<InfyPOS.Processors.BillImportManager.BillSummary>.Run(() => billImportManager.Validate(Request, -1, imageData));
            return this.Ok(imageidentifier);
        }


        [Route("UpdateProductAgreement")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateProductAgreement(Common.ActionRequest<ExcelImportManager.BillSummary> request)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.ExcelImportManager billImportManager = new ExcelImportManager(imageData.Context.OrganizationID);
            InfyPOS.Processors.ExcelImportManager.BillSummary imageidentifier =
                await Task<InfyPOS.Processors.ExcelImportManager.BillSummary>.Run(() => billImportManager.UploadAgreement(request.Item.MigrationData, imageData));
            return this.Ok(imageidentifier);
        }

        [Route("UploadQuantoExcel/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadQuantoExcel(string type)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.BillImportManager billImportManager = new BillImportManager(imageData.Context.OrganizationID);
            InfyPOS.Processors.BillImportManager.BillSummary imageidentifier =
                await Task<InfyPOS.Processors.BillImportManager.BillSummary>.Run(() => billImportManager.Validate(Request, -1, imageData));
            return this.Ok(imageidentifier);
        }

        [Route("UploadBill/{importtypeid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadBill(string importtypeid, string type)
        {
            Common.ImageData imageData = new Common.ImageData();
            InfyPOS.Processors.BillImportManager billImportManager = new BillImportManager(imageData.Context.OrganizationID);
            InfyPOS.Processors.BillImportManager.BillSummary imageidentifier =
                await Task<InfyPOS.Processors.BillImportManager.BillSummary>.Run(() => billImportManager.Validate(Request, long.Parse(importtypeid), imageData));
            return this.Ok(imageidentifier);
        }

        [AllowAnonymous]
        [Route("UploadMigration/{organizationid}/{type}/{hasColumn=FALSE}/{sheetname=null}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadMigration(string organizationid, string type, string hasColumn, string sheetname)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            //ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() => ParseSchema(Request,sheetname, hasColumn, type));
            ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() => ParseSheetSchema(imageData.Context, organizationid, Request));
            return this.Ok(imageidentifier);
        }

        [AllowAnonymous]
        [Route("UploadPRN/{organizationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadPRN(string organizationid, string type)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            //ExcelDocument imageidentifier = await Task<ExcelDocument>.Run(() => ParseSchema(Request,sheetname, hasColumn, type));
            string prncontent = await Task<string>.Run(() => ParsePRN(organizationid, Request));
            return this.Ok(prncontent);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ShowBarcodeImage/{organizationid}/{mode}/{barcode}")]
        public async Task<IHttpActionResult> ShowBarcodeImage(string organizationid, string mode, string barcode)
        {
            if (barcode.IndexOf('.') > 0)
                barcode = barcode.Substring(0, barcode.IndexOf('.'));

            if (mode == "3")
            {
                string byteArray = await Task<string>.Run(() =>
                {
                    StockDataService stockDataService = new StockDataService();
                    stockDataService.Context = new UserClaim()
                    {
                        OrganizationID = long.Parse(organizationid)
                    };
                    Models.StockImage.Modes modes;
                    Models.StockImage.Modes.TryParse(mode, out modes);
                    var data = stockDataService.UpdateImage(new Models.StockImage()
                    {
                        Action = Models.StockImage.Actions.Get,
                        barcode = barcode,
                        mode = modes
                    });
                    return data.imageid;
                });
                return this.Ok(byteArray);
            }
            else
            {
                FileContentInfo byteArray = await Task<FileContentInfo>.Run(() =>
                {
                    StockDataService stockDataService = new StockDataService();
                    stockDataService.Context = new UserClaim()
                    {
                        OrganizationID = long.Parse(organizationid)
                    };
                    Models.StockImage.Modes modes;
                    Models.StockImage.Modes.TryParse(mode, out modes);
                    var data = stockDataService.UpdateImage(new Models.StockImage()
                    {
                        Action = Models.StockImage.Actions.Get,
                        barcode = barcode,
                        mode = modes
                    });
                    if (data.available)
                        return ReadImage(organizationid, data.imageid);
                    return null;
                });
                if (byteArray == null)
                    return this.NotFound();
                return new FileActionResult(false, byteArray.FileName, byteArray.Extension, byteArray.Content);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("DeleteBarcodeImage/{organizationid}/{mode}/{barcode}/{imageid}")]
        public async Task<IHttpActionResult> DeleteBarcodeImage(string organizationid,
            string mode, string barcode, string imageid)
        {
            bool deleted = await Task<string>.Run(() =>
            {
                StockDataService stockDataService = new StockDataService();
                stockDataService.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                Models.StockImage.Modes modes;
                Models.StockImage.Modes.TryParse(mode, out modes);
                var data = stockDataService.UpdateImage(new Models.StockImage()
                {
                    Action = Models.StockImage.Actions.Delete,
                    barcode = barcode,
                    mode = modes,
                    imageid = imageid
                });
                return data.deleted;
            });
            return this.Ok(deleted);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("QueueResultClear")]
        public async Task<IHttpActionResult> ClearQueueResult()
        {
            try
            {
                var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
                return this.Ok(queueManager.ClearQueues());
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("QueueResultList")]

        public async Task<IHttpActionResult> QueueResultList()
        {
            try
            {
                var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
                return this.Ok(queueManager.ActiveList);
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("QueueResultStatus/{queueId}")]
        public async Task<IHttpActionResult> GetQueueResult(string queueId)
        {
            try
            {
                Guid guid;
                if (!Guid.TryParse(queueId, out guid))
                {
                    return this.BadRequest("Invalid Queue ID format");
                }

                var queueManager = InfyPOS.Processors.WorkflowQueueManager.Instance;
                var response = queueManager.GetQueueResult(guid);

                if (response.IsCompleted)
                {
                    if (response.Status == QueueStatus.Failed)
                    {
                        throw new Exception(response.ErrorMessage);
                    }
                    else
                    {
                        return this.Ok(
                            new
                            {
                                IsCompleted = true,
                                Result = response.Result
                            }
                        );
                    }
                }
                else
                {
                    return this.Ok(new
                    {
                        QueueId = response.QueueId,
                        Status = response.Status.ToString(),
                        IsCompleted = response.IsCompleted,
                        CreatedOn = response.CreatedOn,
                        Message = "Operation is still running. Please poll again."
                    });
                }
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        [Route("UploadBarcodeImage/{mode}/{barcode}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadBarcodeImage(string mode, string barcode)
        {
            Common.ImageData imageData = new Common.ImageData();
            string imageidentifier = await Task<string>.Run(() => UploadBarcodeImage(imageData, Request, mode, barcode));
            return this.Ok(imageidentifier);
        }

        private string UploadBarcodeImage(Common.ImageData imageData,
            HttpRequestMessage Request, string mode,
            string barcode)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent() || string.IsNullOrEmpty(barcode))
                {
                    return null;
                }
                string imageid = "";
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                {
                    MultipartMemoryStreamProvider provider = task.Result;

                    foreach (HttpContent content in provider.Contents)
                    {
                        Stream stream = content.ReadAsStreamAsync().Result;
                        Byte[] buffer = new Byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);

                        if (content.Headers.ContentDisposition.FileName == null)
                            continue;
                        FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                        Common.ImageData.File file = new Common.ImageData.File();
                        file.createdby = imageData.Context.UserID;
                        file.createdon = imageData.Context.DateTimeNow;
                        file.data = buffer;
                        file.ext = fi.Extension;
                        try
                        {
                            Image img = Image.FromStream(stream);
                            if (img != null)
                            {
                                var thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 32, 32);
                                file.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg);

                                thumbimg.Dispose();
                                if (img.Height > 1500 && img.Width > 1500)
                                {
                                    if (img.Height > img.Width)
                                    {
                                        thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 850, 1200);
                                    }
                                    else
                                    {
                                        thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 1200, 850);
                                    }
                                    file.data = WebAPI.Common.ImageManager.ToByteArray(thumbimg);
                                    file.ext = ".JPG";
                                }
                                else if (buffer.Length > 150 * 1024)
                                {
                                    file.data = WebAPI.Common.ImageManager.ToByteArray(img);
                                    file.ext = ".JPG";
                                }
                            }
                        }
                        catch (Exception exp)
                        {

                        }


                        file.filename = fi.Name;
                        file.isactive = true;
                        file.modifiedby = imageData.Context.UserID;
                        file.modifiedon = imageData.Context.DateTimeNow;
                        file.version = 1;
                        file.type = "png";
                        file = imageData.Insert(file);
                        imageid = file.id.ToBase62();
                        break;
                    }

                    if (!string.IsNullOrEmpty(imageid))
                    {
                        Models.StockImage.Modes modes;
                        Models.StockImage.Modes.TryParse(mode, out modes);
                        StockDataService stockDataService = new StockDataService();
                        stockDataService.Context = imageData.Context;
                        stockDataService.UpdateImage(new Models.StockImage()
                        {
                            Action = Models.StockImage.Actions.Update,
                            imageid = imageid,
                            barcode = barcode,
                            mode = modes
                        });
                    }

                    are.Set();
                });
                are.WaitOne();
                return imageid;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(imageData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }


        [Route("Upload/{organizationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> Upload(string organizationid, string type)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            List<string> imageidentifier = await Task<List<string>>.Run(() => StoreMultipleImage(imageData.Context, "0", Request, type));
            return this.Ok(imageidentifier);
        }

        [Route("UploadSingle/{organizationid}/{type}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadSingle(string organizationid, string type)
        {
            //  imageData = new Common.ImageData();
            //SingleFile imageidentifier = await Task<SingleFile>.Run(() => ParseFile(Request));
            //return this.Ok(imageidentifier);
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            SingleFile imageidentifier = await Task<List<string>>.Run(() => ParseFile(imageData.Context, organizationid, Request));
            return this.Ok(imageidentifier);
        }

        private List<string> StoreMultipleImage(
            UserClaim Context,
            string organizationid, HttpRequestMessage Request, string type)
        {
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    lst.Add("");
                    return lst;
                }
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;
                        Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
                        foreach (HttpContent content in provider.Contents)
                        {
                            Stream stream = content.ReadAsStreamAsync().Result;
                            Byte[] buffer = new Byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);

                            if (content.Headers.ContentDisposition.FileName == null)
                                continue;
                            FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            Common.ImageData.File file = new Common.ImageData.File();
                            file.createdby = imageData.Context.UserID;
                            file.createdon = Context.DateTimeNow;
                            file.data = buffer;
                            file.ext = fi.Extension;
                            try
                            {
                                Image img = Image.FromStream(stream);
                                if (img != null)
                                {
                                    var thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 32, 32);
                                    file.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg);

                                    thumbimg.Dispose();
                                    if (img.Height > 1500 && img.Width > 1500)
                                    {
                                        if (img.Height > img.Width)
                                        {
                                            thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 850, 1150);
                                        }
                                        else
                                        {
                                            thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 1150, 850);
                                        }
                                        file.data = WebAPI.Common.ImageManager.ToByteArray(thumbimg);
                                        file.ext = ".JPG";
                                    }
                                    else if (buffer.Length > 150 * 1024)
                                    {
                                        file.data = WebAPI.Common.ImageManager.ToByteArray(img);
                                        file.ext = ".JPG";
                                    }
                                }
                            }
                            catch (Exception exp)
                            {

                            }


                            file.filename = fi.Name;
                            file.isactive = true;
                            file.modifiedby = imageData.Context.UserID;
                            file.modifiedon = Context.DateTimeNow;
                            file.version = 1;
                            file.type = type;
                            file = imageData.Insert(file);

                            lst.Add(file.id.ToBase62());
                        }
                        are.Set();
                    });
                are.WaitOne();
                return lst;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        public class ExcelSheet : BaseEntity
        {
            public string name { get; set; }
            public int id { get; set; }
            public List<string> columns { get; set; }
        }

        public class ExcelDocument : BaseEntity
        {
            public ExcelDocument()
            {
                columns = new List<string>();
                rows = new List<Row>();
            }
            public string filename { get; set; }
            public string fileid { get; set; }
            public List<string> columns { get; set; }
            public List<ExcelSheet> sheets { get; set; }
            public List<Row> rows { get; set; }
            public Exception exception { get; set; }
            public decimal qty { get; set; }
            public List<string> errors { get; set; }

            public string GetData(int index, string columnname)
            {
                return rows[index].values[columns.IndexOf(columnname)];
            }
            public string GetData(Row row, string columnname)
            {
                return row.values[columns.IndexOf(columnname)];
            }
            public class Row
            {
                public Row()
                {
                    values = new List<string>();
                }
                public List<string> values { get; set; }
            }
        }

        private DataTable CreateTable(ClosedXML.Excel.IXLWorksheet workSheet, bool hascolumn, bool onlyheader)
        {
            bool firstRow = true;
            DataTable dt = new DataTable();
            bool isrow = true;
            if (onlyheader)
            {
                var row = workSheet.Row(1);
                int i = 0;
                var cells = row.Cells();
                foreach (var cell in cells)
                {
                    while (cell.Address.ColumnNumber != i + 1)
                    {
                        dt.Columns.Add("c_" + i);
                        i++;
                    }
                    if (!dt.Columns.Contains(cell.Value.ToString()))
                        dt.Columns.Add(cell.Value.ToString());
                    i++;
                }
                return dt;
            }
            var rows = workSheet.Rows();
            foreach (var row in rows)
            {
                //Use the first row to add columns to DataTable.
                if (firstRow)
                {
                    int i = 0;
                    var cells = row.Cells();
                    foreach (var cell in cells)
                    {
                        while (cell.Address.ColumnNumber != i + 1)
                        {
                            dt.Columns.Add("c_" + i);
                            i++;
                        }
                        if (!dt.Columns.Contains(cell.Value.ToString()))
                            dt.Columns.Add(cell.Value.ToString());
                        i++;
                    }
                    firstRow = false;
                    isrow = (!hascolumn);
                    if (onlyheader) break;
                }


                if (isrow)
                {
                    //Add rows to DataTable.
                    dt.Rows.Add();
                    int i = 0;
                    foreach (var cell in row.Cells())
                    {
                        while (cell.Address.ColumnNumber != i + 1)
                        {
                            if (dt.Columns.Count < (i + 1)) break;
                            dt.Rows[dt.Rows.Count - 1][i] = "";
                            i++;
                        }
                        if (dt.Columns.Count >= cell.Address.ColumnNumber)
                        {
                            if (cell.DataType == ClosedXML.Excel.XLCellValues.Number && cell.Value is Double && cell.Value.ToString().Contains("+"))
                            {
                                Double d = (double)cell.Value;
                                if (d % 1 > 0)
                                {
                                    dt.Rows[dt.Rows.Count - 1][i] = d.ToString("F2");
                                }
                                else
                                {
                                    dt.Rows[dt.Rows.Count - 1][i] = d.ToString("F0");
                                }
                            }
                            else if (cell.DataType == ClosedXML.Excel.XLCellValues.Number && cell.Value is Int64 && cell.Value.ToString().Contains("+"))
                            {
                                Int64 d = (Int64)cell.Value;
                                dt.Rows[dt.Rows.Count - 1][i] = d.ToString("F0");
                            }
                            else if (cell.DataType == ClosedXML.Excel.XLCellValues.DateTime && cell.Value is DateTime)
                            {
                                DateTime datetime = (DateTime)cell.Value;
                                dt.Rows[dt.Rows.Count - 1][i] = datetime.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString().Trim();
                            }
                        }
                        i++;
                    }
                }
                isrow = true;
            }
            return dt;
        }

        private ExcelDocument ParseSheetSchema(UserClaim Context, string organizationid, HttpRequestMessage Request)
        {
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return new ExcelDocument();
                }
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);

                ExcelDocument exceldoc = new ExcelDocument();
                Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };

                try
                {

                    Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;

                        foreach (HttpContent content in provider.Contents)
                        {
                            Stream stream = content.ReadAsStreamAsync().Result;
                            Byte[] buffer = new Byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);

                            if (content.Headers.ContentDisposition.FileName == null)
                                continue;

                            string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();

                            exceldoc = new ExcelDocument();
                            exceldoc.sheets = new List<ExcelSheet>();
                            using (MemoryStream ms = new MemoryStream(buffer))
                            {
                                using (var workBook = new ClosedXML.Excel.XLWorkbook(ms, ClosedXML.Excel.XLEventTracking.Disabled))
                                {
                                    foreach (var worksheet in workBook.Worksheets)
                                    {
                                        ExcelSheet excelSheet = new ExcelSheet();
                                        excelSheet.name = worksheet.Name;
                                        excelSheet.id = exceldoc.sheets.Count + 1;
                                        excelSheet.columns = new List<string>();
                                        var datatable = CreateTable(worksheet, true, true);

                                        foreach (DataColumn column in datatable.Columns)
                                        {
                                            excelSheet.columns.Add(column.ColumnName);
                                        }
                                        exceldoc.sheets.Add(excelSheet);
                                    }
                                }
                            }


                            FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            Common.ImageData.File file = new Common.ImageData.File();
                            file.createdby = imageData.Context.UserID;
                            file.createdon = Context.DateTimeNow;
                            file.data = buffer;
                            file.ext = fi.Extension;
                            file.filename = fi.Name;
                            file.isactive = true;
                            file.modifiedby = imageData.Context.UserID;
                            file.modifiedon = Context.DateTimeNow;
                            file.version = 1;
                            file.type = "migration";
                            file = imageData.Insert(file);

                            exceldoc.filename = file.filename;
                            exceldoc.fileid = file.id.ToBase62();
                            break;
                        }
                        are.Set();
                    });

                    are.WaitOne();

                }
                catch (Exception exp)
                {
                    are.Set();
                }

                return exceldoc;
            }
            catch (Exception exp)
            {
                return new ExcelDocument();
            }
        }


        private SingleFile ParseFile(UserClaim Context, string organizationid, HttpRequestMessage Request)
        {
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return new SingleFile();
                }
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);

                SingleFile exceldoc = new SingleFile();
                Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };


                try
                {

                    Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;

                        foreach (HttpContent content in provider.Contents)
                        {
                            Stream stream = content.ReadAsStreamAsync().Result;
                            Byte[] buffer = new Byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);

                            if (content.Headers.ContentDisposition.FileName == null)
                                continue;

                            string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();

                            FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            Common.ImageData.File file = new Common.ImageData.File();
                            file.createdby = imageData.Context.UserID;
                            file.createdon = Context.DateTimeNow;
                            file.data = buffer;
                            file.ext = fi.Extension;
                            file.filename = fi.Name;
                            file.isactive = true;
                            file.modifiedby = imageData.Context.UserID;
                            file.modifiedon = Context.DateTimeNow;
                            file.version = 1;
                            file.type = "singlefile";
                            file = imageData.Insert(file);
                            exceldoc.size = buffer.LongLength;
                            exceldoc.file = file.filename;
                            exceldoc.fileid = file.id.ToBase62();
                            exceldoc.id = file.id;
                            break;
                        }
                        are.Set();
                    });

                    are.WaitOne();

                }
                catch (Exception exp)
                {
                    are.Set();
                }

                return exceldoc;
            }
            catch (Exception exp)
            {
                return new SingleFile();
            }
        }


        private string ParsePRN(string organizationid, HttpRequestMessage Request)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return "";
                }
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                Byte[] bytes = null;

                try
                {

                    Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;

                        foreach (HttpContent content in provider.Contents)
                        {
                            Stream stream = content.ReadAsStreamAsync().Result;
                            bytes = new Byte[stream.Length];
                            stream.Read(bytes, 0, bytes.Length);

                            if (content.Headers.ContentDisposition.FileName == null)
                                continue;

                            string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                            break;
                        }
                        are.Set();
                    });

                    are.WaitOne();

                }
                catch (Exception exp)
                {
                    are.Set();
                }

                if (bytes != null)
                {
                    List<byte> replace = new List<byte>();
                    for (var i = 0; i < bytes.Length; i++)
                    {
                        // || bytes[i]< 10
                        if (bytes[i] > 127 || bytes[i] == 0)
                        {
                            List<byte> base64 = new List<byte>();
                            for (var j = i; j < bytes.Length; j++)
                            {
                                // && bytes[j] >= 10
                                if (bytes[j] <= 127 && bytes[j] != 0)
                                {
                                    i = j - 1;
                                    break;
                                }
                                base64.Add(bytes[j]);
                            }
                            replace.AddRange(System.Text.ASCIIEncoding.ASCII.GetBytes("<Q64>" + Convert.ToBase64String(base64.ToArray()) + "</Q64>"));
                        }
                        else
                        {
                            replace.Add(bytes[i]);
                        }
                    }
                    return System.Text.ASCIIEncoding.ASCII.GetString(replace.ToArray());
                }
                return "";
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }


        private ExcelDocument ParseSchema(UserClaim Context, HttpRequestMessage Request, string sheetname, string hasColumn, string type)
        {
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return new ExcelDocument();
                }
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);

                ExcelDocument exceldoc = new ExcelDocument();

                Common.ImageData imageData = new Common.ImageData();
                try
                {

                    Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;

                        foreach (HttpContent content in provider.Contents)
                        {
                            Stream stream = content.ReadAsStreamAsync().Result;
                            Byte[] buffer = new Byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);

                            if (content.Headers.ContentDisposition.FileName == null)
                                continue;

                            string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                            if (filename.EndsWith(".xlsx"))
                                exceldoc = ExtractXlsx(buffer, hasColumn == "TRUE", true, sheetname);
                            else if (filename.EndsWith(".xls"))
                                exceldoc = ExtractXls(buffer, hasColumn == "TRUE", true);


                            FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                            Common.ImageData.File file = new Common.ImageData.File();
                            file.createdby = imageData.Context.UserID;
                            file.createdon = Context.DateTimeNow;
                            file.data = buffer;
                            file.ext = fi.Extension;
                            file.filename = fi.Name;
                            file.isactive = true;
                            file.modifiedby = imageData.Context.UserID;
                            file.modifiedon = Context.DateTimeNow;
                            file.version = 1;
                            file.type = type;
                            file = imageData.Insert(file);

                            exceldoc.filename = file.filename;
                            exceldoc.fileid = file.id.ToBase62();
                            break;
                        }
                        are.Set();
                    });

                    are.WaitOne();

                }
                catch (Exception exp)
                {
                    are.Set();
                }

                return exceldoc;
            }
            catch (Exception exp)
            {
                return new ExcelDocument();
            }
        }


        private ExcelDocument Parse(string organizationid, HttpRequestMessage Request, string hasColumn, string type, string sheetname)
        {
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return new ExcelDocument();
                }
                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);

                ExcelDocument exceldoc = new ExcelDocument();
                try
                {

                    Request.Content.ReadAsMultipartAsync<MultipartMemoryStreamProvider>(new MultipartMemoryStreamProvider()).ContinueWith((task) =>
                    {
                        MultipartMemoryStreamProvider provider = task.Result;

                        try
                        {
                            foreach (HttpContent content in provider.Contents)
                            {
                                Stream stream = content.ReadAsStreamAsync().Result;
                                Byte[] buffer = new Byte[stream.Length];
                                stream.Read(buffer, 0, buffer.Length);

                                if (content.Headers.ContentDisposition.FileName == null)
                                    continue;

                                string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                if (filename.EndsWith(".xlsx"))
                                    exceldoc = ExtractXlsx(buffer, hasColumn == "TRUE", false, sheetname);
                                else if (filename.EndsWith(".xls"))
                                    exceldoc = ExtractXls(buffer, hasColumn == "TRUE", false);
                                else
                                    exceldoc = Extractcsv(buffer, hasColumn == "TRUE");
                            }
                        }
                        catch (Exception exp)
                        {
                            exceldoc.exception = exp;
                            WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
                        }

                        are.Set();
                    });

                    are.WaitOne();

                }
                catch (Exception exp)
                {
                    are.Set();
                }

                return exceldoc;
            }
            catch (Exception exp)
            {
                return new ExcelDocument();
            }
        }



        private ExcelDocument ExtractXlsx(Byte[] buffer, bool hasColumn, bool onlyheader, string sheetname)
        {
            ExcelDocument exceldoc = new ExcelDocument();
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                {

                    var worksheet = workBook.Worksheets.First();
                    if (!string.IsNullOrEmpty(sheetname))
                    {
                        foreach (var sh in workBook.Worksheets)
                        {
                            if (sh.Name.ToLower() == sheetname.ToLower())
                                worksheet = sh;
                        }
                    }
                    var datatable = CreateTable(worksheet, hasColumn, onlyheader);

                    foreach (DataColumn column in datatable.Columns)
                    {
                        exceldoc.columns.Add(column.ColumnName);
                    }

                    if (!onlyheader)
                    {
                        foreach (DataRow dr in datatable.Rows)
                        {
                            ExcelDocument.Row row = new ExcelDocument.Row();
                            foreach (var colmvalue in dr.ItemArray)
                            {
                                row.values.Add(colmvalue == null ? "" : colmvalue.ToString());
                            }
                            if (!row.values.All(e => string.IsNullOrEmpty(e)))
                                exceldoc.rows.Add(row);
                        }
                    }
                }
            }
            return exceldoc;
        }


        private ExcelDocument ExtractXls(Byte[] buffer, bool hascolumn, bool onlyheader)
        {

            ExcelDocument exceldoc = new ExcelDocument();
            using (MemoryStream ms = new MemoryStream(buffer))
            {

                using (var reader = ExcelDataReader.ExcelReaderFactory.CreateBinaryReader(ms))
                {
                    for (int cindex = 0; cindex < reader.FieldCount; cindex++)
                    {
                        exceldoc.columns.Add(reader.GetName(cindex));
                    }

                    if (!hascolumn)
                    {
                        ExcelDocument.Row row = new ExcelDocument.Row();
                        exceldoc.rows.Add(row);
                        for (int cindex = 0; cindex < reader.FieldCount; cindex++)
                        {
                            row.values.Add(reader.GetName(cindex));
                        }
                    }
                    if (!onlyheader)
                    {
                        while (!reader.Read())
                        {
                            ExcelDocument.Row row = new ExcelDocument.Row();
                            for (int cindex = 0; cindex < reader.FieldCount; cindex++)
                            {
                                row.values.Add(reader.GetValue(cindex) == null ? "" : reader.GetValue(cindex).ToString());
                            }
                            if (!row.values.All(e => string.IsNullOrEmpty(e)))
                                exceldoc.rows.Add(row);
                        }
                    }
                }
            }
            return exceldoc;
        }



        private ExcelDocument Extractcsv(Byte[] buffer, bool hascolunm)
        {

            ExcelDocument exceldoc = new ExcelDocument();
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                System.IO.TextReader readFile = new StreamReader(ms, false);
                string[] colunmns = readFile.ReadLine().Split(',');
                for (int cindex = 0; cindex < colunmns.Length; cindex++)
                {
                    exceldoc.columns.Add(colunmns[cindex]);
                }
                if (!hascolunm)
                {
                    ExcelDocument.Row row = new ExcelDocument.Row();
                    exceldoc.rows.Add(row);
                    for (int cindex = 0; cindex < colunmns.Length; cindex++)
                    {
                        row.values.Add(colunmns[cindex]);
                    }
                }
                string line = "";
                while ((line = readFile.ReadLine()) != null)
                {
                    ExcelDocument.Row row = new ExcelDocument.Row();
                    exceldoc.rows.Add(row);
                    string[] values = line.Split(',');
                    for (int cindex = 0; cindex < colunmns.Length; cindex++)
                    {
                        row.values.Add(values[cindex]);
                    }
                }
            }
            return exceldoc;
        }

        private BarcodeFile GetBarcodeFile(UserClaim Context, long id)
        {
            try
            {
                Common.ImageData imageData = new Common.ImageData();
                imageData.Context = Context;
                var file = imageData.GetData(id);
                return new BarcodeFile()
                {
                    id = file.id,
                    content = System.Text.ASCIIEncoding.ASCII.GetString(file.data)
                };
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return new BarcodeFile();
        }

        private long StoreBarcodeFile(UserClaim Context, BarcodeFile data)
        {
            try
            {
                byte[] dataasbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(data.content);
                Common.ImageData imageData = new Common.ImageData();
                imageData.Context = Context;
                ImageData.File file = null;
                if (data.id == 0)
                {
                    file = new ImageData.File();
                    file.ext = ".bcd";
                    file.type = "Barcode";
                    file.filename = data.filename;
                    file.createdon = DateTime.Now;
                    file.modifiedon = DateTime.Now;
                    file.createdby = Context.UserID;
                    file.modifiedby = Context.UserID;
                    file.version = 1;
                    file.isactive = true;
                    file.data = dataasbytes;
                    file = imageData.Insert(file);
                }
                else
                {
                    file = imageData.GetData(data.id);
                    file.data = dataasbytes;
                    file.modifiedon = DateTime.Now;
                    file = imageData.Update(file);
                }
                return file.id;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return data.id;
        }

        private string StoreImage(UserClaim Context, ImageAsBase64 data)
        {
            try
            {
                string[] parts = data.base64.Split(',');

                byte[] dataasbytes = Convert.FromBase64String(parts[1]);
                byte[] imagebytes = null;
                Common.ImageData imageData = new Common.ImageData();
                imageData.Context = Context;
                var file = imageData.GetData(data.id.LongFromBase62());
                using (System.IO.MemoryStream ms = new MemoryStream(dataasbytes))
                {
                    Image img = Image.FromStream(ms);
                    var thumbimg = WebAPI.Common.ImageManager.ScaleBySize(img, 128);
                    file.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg);
                    using (System.IO.MemoryStream outms = new MemoryStream())
                    {
                        img.Save(outms, ImageFormat.Png);
                        imagebytes = outms.ToArray();
                    }
                }
                file.ext = ".png";
                file.type = "png";
                file.data = imagebytes;
                file = imageData.Insert(file);
                return file.id.ToBase62();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(data.organizationid).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return data.id;
        }

        public class SingleFile
        {
            public long id { get; set; }
            public string fileid { get; set; }
            public string file { get; set; }
            public long size { get; set; }
        }
        private List<string> StoreImage(UserClaim Context, string organizationid, HttpRequestMessage Request, string type)
        {
            try
            {
                if (Request.Content.IsMimeMultipartContent())
                {
                    return StoreMultipleImage(Context, organizationid, Request, type);
                }
                List<string> lst = new List<string>();
                StreamContent content = (StreamContent)Request.Content;
                Task<Stream> task = content.ReadAsStreamAsync();
                Stream readOnlyStream = task.Result;
                Byte[] buffer = new Byte[readOnlyStream.Length];
                readOnlyStream.Read(buffer, 0, buffer.Length);
                FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName);
                Common.ImageData.File file = new Common.ImageData.File();
                Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
                file.createdby = imageData.Context.UserID;
                file.createdon = Context.DateTimeNow;
                file.data = buffer;
                file.ext = fi.Extension;
                try
                {
                    Image img = Image.FromStream(readOnlyStream);
                    if (img != null)
                    {
                        var thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 32, 32);
                        file.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg);

                        thumbimg.Dispose();
                        if (img.Height > 1500 && img.Width > 1500)
                        {
                            if (img.Height > img.Width)
                            {
                                thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 850, 1150);
                            }
                            else
                            {
                                thumbimg = WebAPI.Common.ImageManager.FixedSize(img, 1150, 850);
                            }
                            file.data = WebAPI.Common.ImageManager.ToByteArray(thumbimg);
                            file.ext = ".JPG";
                        }
                        else if (buffer.Length > 150 * 1024)
                        {
                            file.data = WebAPI.Common.ImageManager.ToByteArray(img);
                            file.ext = ".JPG";
                        }
                    }
                }
                catch (Exception exp)
                {

                }
                file.filename = fi.Name;
                file.isactive = true;
                file.modifiedby = imageData.Context.UserID;
                file.modifiedon = Context.DateTimeNow;
                file.version = 1;
                file.type = type;
                file = imageData.Insert(file);
                lst.Add(file.id.ToBase62());
                return lst;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        private bool RemoveImage(string organizationid, string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            return imageData.Remove(id.LongFromBase62());
        }

        public FileContentInfo ReadThumbnail(string organizationid, string id)
        {
            if (string.IsNullOrEmpty(organizationid) || string.IsNullOrEmpty(id))
                return null;

            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var file = imageData.GetThumbnail(id.LongFromBase62());
            if (file.thumbnail != null)
            {
                var fci = new FileContentInfo(new ByteArrayContent(file.thumbnail));
                fci.FileName = file.filename;
                fci.Extension = file.thumbnail != null ? GetImageMimeType(file.thumbnail, file.ext) : "image/" + file.ext.Replace(".", "");
                return fci;
            }
            return new FileContentInfo(new ByteArrayContent(new byte[0]));
        }

        public FileContentInfo ReadPDF(string organizationid, string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var file = imageData.GetData(id.LongFromBase62(true));
            var fci = new FileContentInfo(new ByteArrayContent(file.data));
            fci.FileName = file.filename;
            fci.Extension = "application/pdf";
            return fci;
        }

        public FileContentInfo ReadImage(string organizationid, string id)
        {
            if (string.IsNullOrEmpty(organizationid) || string.IsNullOrEmpty(id))
                return null;

            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };

            var file = imageData.GetData(id.LongFromBase62());
            if (file == null)
                return null;
            var fci = new FileContentInfo(new ByteArrayContent(file.data));
            fci.FileName = file.filename;
            fci.Extension = file.data != null ? GetImageMimeType(file.data, file.ext) : "image/" + file.ext.Replace(".", "");
            return fci;
        }


        //public FileContentInfo DownloadCommandJSON(string organizationid)
        //{
        //    var request = new InfyPOS.Processors.CommandManager.Request();
        //    request.parameter = new Processors.CommandManager.Request.Parameter();
        //    var fci = new FileContentInfo(new ByteArrayContent(System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(request))));
        //    fci.FileName = "dbupgrade.txt";
        //    fci.Extension = "text/plain";
        //    return fci;
        //}

        public ProgressStatus Processcommand(string organizationid, string source)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var file = imageData.GetData(source.LongFromBase62());
            if (file == null)
            {
                return new ProgressStatus()
                {
                    hascompleted = true,
                    Exception = new Exception("Can't find the request")
                };
            }
            imageData.Remove(file.id);

            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<InfyPOS.Processors.CommandManager.Request>(System.Text.ASCIIEncoding.ASCII.GetString(file.data));
            if (request != null)
            {
                request.fileid = source.LongFromBase62();
                if (!string.IsNullOrEmpty(request.command) &&
                    !request.isview)
                {
                    imageData.Remove(file.id);
                }
            }
            return Processors.CommandManager.Run(long.Parse(organizationid), file.id, file.data);

            //var filepath = System.Configuration.ConfigurationManager.AppSettings["CommandLibPath"].ToLower();
            //Assembly asm = Assembly.LoadFrom(filepath);
            //var integration = asm.GetType(System.Configuration.ConfigurationManager.AppSettings["CommandLibClass"]);
            //if (integration != null)
            //{
            //    ICommandManager bright = Activator.CreateInstance(integration) as ICommandManager;
            //    return bright.Run(long.Parse(organizationid), file.id, file.data);
            //}
            //return new ProgressStatus()
            //{
            //    hascompleted = true,
            //    Exception = new Exception("Can't find the request")
            //};
        }

        public FileContentInfo ReadSchema(string organizationid, string source)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var file = imageData.GetData(source.LongFromBase62());
            var compareresult = Utility.CompareSchema(file.data, imageData.CreateConnection().ConnectionString);
            var fci = new FileContentInfo(new ByteArrayContent(compareresult));
            fci.FileName = "dbupgrade.txt";
            fci.Extension = "text/plain";
            return fci;
        }

        public void ClearData()
        {
            string query = @"delete from bill;
delete from billitems;
delete from billmaster;
delete from Billsettlement;
delete from Billsettlementmaster;
delete from discountqueue;
delete from stock;
delete from salablegoods;
delete from salesorder;
delete from lrentry;
delete from lrinvoiceitems;
delete from lrinvoice;
delete from voucheritems;
delete from voucher;
delete from vouchers;
delete from stocktransaction;

delete from supplierpayment;
delete from supplierpaymentinvoice;
delete from transportpaymentinvoice;
delete from transportpayment;
delete from customerpayment;
delete from customerinvoice;
delete from customerorder;

delete from salesorderitems;
delete from mfgworkorderitemsizes;
delete from mfgworkordergrnitemsizes;
delete from mfgworkordergrnitems;
delete from lrinvoicetax;
delete from mfgworkorderinbox;
delete from mfgworkorderinbox;
delete from mfgworkorder;
delete from mfgworkordergrn;
delete from items;
delete from mfgdesign;
delete from referencelist where type not in ('LOCATION','DIVISION','DESIGNATION','CITY',
											'DEPARTMENT','STATE','COUNTER','USERROLE','CUSTOMERCATEGORY')";
        }
        public FileContentInfo UploadDataToDB(string organizationid, string source)
        {
            Common.ImageData imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            var file = imageData.GetData(source.LongFromBase62());
            InfyPOS.Services.Sync_Client sync_Client = new Services.Sync_Client();
            var fci = new FileContentInfo(new ByteArrayContent(sync_Client.UploadData(file.data)));
            fci.FileName = "dbupgrade.txt";
            fci.Extension = "text/plain";
            return fci;
        }

        public class CameraImage : BaseEntity
        {
            public string image { get; set; }
            public string type { get; set; }
        }
        [HttpPost]
        [Route("StoreImage")]
        public async Task<IHttpActionResult> StoreImage(Common.ActionRequest<CameraImage> request)
        {
            ImageData StockData = new ImageData();
            string result = await Task<string>.Run(() => StockData.StoreImage(request.Item.image, request.Item.type));
            return this.Ok(result);
        }

        public FileContentInfo DownloadData(long organizationid, long locationid)
        {
            InfyPOS.Services.Sync_Server sync_Server = new Services.Sync_Server(organizationid, 0, 0);
            var fci = new FileContentInfo(new ByteArrayContent(sync_Server.DownloadData(organizationid, locationid)));
            fci.FileName = "schema.txt";
            fci.Extension = "text/plain";
            return fci;
        }

        public FileContentInfo Downloadfile(string key)
        {
            var parts = key.Split('.')[0].Split('_');
            string type = parts[0];
            long organizationid = parts[1].LongFromBase62();
            long id = parts[2].LongFromBase62();
            if (type == "BILL")
            {
                BillServiceController billServiceController = new BillServiceController();
                return billServiceController.PrintBill(organizationid, id);
            }
            BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            var fci = new FileContentInfo(new ByteArrayContent(new byte[] { }));
            fci.FileName = "missing";
            fci.Extension = "text/plain";
            return fci;
        }

        public FileContentInfo ReadSchema(long organizationid)
        {
            BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            var fci = new FileContentInfo(new ByteArrayContent(Utility.DownloadSchema(baseData.CreateConnection().ConnectionString)));
            fci.FileName = "schema.txt";
            fci.Extension = "text/plain";
            return fci;
        }


        public string GetImageMimeType(byte[] imageData, string ext)
        {
            if (string.IsNullOrEmpty(ext))
                ext = "png";

            String mimeType = "image/" + ext.Replace(".", "");

            try
            {
                Guid id;

                using (MemoryStream ms = new MemoryStream(imageData))
                {
                    using (Image img = Image.FromStream(ms))
                    {
                        id = img.RawFormat.Guid;
                    }
                }

                if (id == ImageFormat.Png.Guid)
                {
                    mimeType = "image/png";
                }
                else if (id == ImageFormat.Bmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Emf.Guid)
                {
                    mimeType = "image/x-emf";
                }
                else if (id == ImageFormat.Exif.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.Gif.Guid)
                {
                    mimeType = "image/gif";
                }
                else if (id == ImageFormat.Icon.Guid)
                {
                    mimeType = "image/ico";
                }
                else if (id == ImageFormat.Jpeg.Guid)
                {
                    mimeType = "image/jpeg";
                }
                else if (id == ImageFormat.MemoryBmp.Guid)
                {
                    mimeType = "image/bmp";
                }
                else if (id == ImageFormat.Tiff.Guid)
                {
                    mimeType = "image/tiff";
                }
                else if (id == ImageFormat.Wmf.Guid)
                {
                    mimeType = "image/wmf";
                }
            }
            catch
            {
            }

            return mimeType;
        }

        private FileContentInfo ReadImage(string fileName)
        {
            String filePath = HostingEnvironment.MapPath("~/Images/" + fileName);
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            Image image = Image.FromStream(fileStream);
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Jpeg);
            var fci = new FileContentInfo(new ByteArrayContent(memoryStream.ToArray()));
            var fi = new FileInfo(filePath);
            fci.FileName = fi.Name;
            fci.Extension = fi.Extension;
            return fci;
        }
    }

    public class FileContentInfo
    {
        public FileContentInfo(ByteArrayContent content)
        {
            this.Content = content;
        }
        public ByteArrayContent Content { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }

    }

    public class FileActionResult : IHttpActionResult
    {
        public HttpResponseMessage response { get; set; }
        public FileActionResult(
            bool isAttachment,
            string fileName,
            string imageExtn,
            ByteArrayContent content) : this(false, isAttachment, fileName, imageExtn, content) { }

        public FileActionResult(bool isstatic,
            bool isAttachment,
            string fileName,
            string imageExtn,
            ByteArrayContent content)
        {
            response = new HttpResponseMessage();
            response.Content = content;
            if (!string.IsNullOrEmpty(imageExtn) && imageExtn.Split('/').Length == 2 && imageExtn.Split('/')[1].Length > 0)
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(imageExtn);
            if (isAttachment)
            {
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileName;
            }
            if (isstatic)
            {
                response.Headers.CacheControl = new CacheControlHeaderValue()
                {
                    MaxAge = TimeSpan.FromDays(365),
                    MustRevalidate = true,
                    Private = true
                };
            }
        }


        public Task<HttpResponseMessage> ExecuteAsync(System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}
