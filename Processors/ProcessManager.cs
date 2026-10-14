using InfyPOS.Common;
using InfyPOS.Models;
using InfyPOS.Processors.Migration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using InfyPOS.Data;

namespace InfyPOS.Processors
{
    public interface ICommandManager
    {
        ProgressStatus Run(long organizationid, long id, byte[] data);
    }
    public class ProcessManager
    {
        private long organizationid;
        public ProcessManager(long organizationid)
        {
            this.organizationid = organizationid;
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, ProcessManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, ProcessManager>();

        public static ProcessManager Instance(long organizationid)
        {
            ProcessManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new ProcessManager(organizationid);
            instanceList.TryAdd(organizationid, instance);

            return instance;
        }
        public class ProcessData
        {
            public string action { get; set; }
            public ProgressStatus progress { get; set; }
            public long organizationid { get; set; }
            public UserClaim Context { get; set; }
            public OnlineIntegration.ProductCriteria criteria { get; set; }

            [JsonIgnore]
            public ReportBuilder ReportBuilderRequest { get; set; }

            [JsonIgnore]
            public HttpRequestMessage request { get; set; }
            public MigrationData migrationdata { get; internal set; }
        }
        public Tuple<string, byte[]> GetResultBytes(string id)
        {
            try
            {
                if (progressStatuses.Exists(e => e.identifier == id))
                {
                    var statusobject = progressStatuses.Find(e => e.identifier == id);
                    progressStatuses.Remove(statusobject);
                    if (statusobject.resultbytes.Length > 50000)
                    {
                        statusobject.percentage = 75;
                        using (var outStream = new MemoryStream())
                        {
                            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                            {
                                var fileInArchive = archive.CreateEntry("DataDump.xlsx", CompressionLevel.Optimal);
                                using (var entryStream = fileInArchive.Open())
                                using (var fileToCompressStream = new MemoryStream(statusobject.resultbytes))
                                {
                                    fileToCompressStream.CopyTo(entryStream);
                                }
                            }
                            var compressedBytes = outStream.ToArray();
                            return new Tuple<string, byte[]>("ZIP", compressedBytes);
                        }
                    }
                    else
                    {
                        return new Tuple<string, byte[]>("XLSX", statusobject.resultbytes);
                    }
                }
            }
            catch (Exception exp)
            {

            }
            return new Tuple<string, byte[]>("XLSX", new byte[0]);
        }

        public ProgressStatus GetEmptyStatus()
        {
            ProgressStatus progressStatus = new ProgressStatus();
            progressStatus.identifier = Guid.NewGuid().ToString();
            progressStatus.percentage = 0;
            progressStatuses.Add(progressStatus);
            return progressStatus;
        }
        public ProgressStatus GetStatus(string id)
        {
            try
            {
                if (progressStatuses.Exists(e => e.identifier == id))
                {
                    var statusobject = progressStatuses.Find(e => e.identifier == id);
                    return new ProgressStatus()
                    {
                        completedrows = statusobject.completedrows,
                        failedrows = statusobject.failedrows,
                        hascompleted = statusobject.hascompleted,
                        percentage = statusobject.percentage,
                        status = statusobject.status,
                        statuslist = statusobject.statuslist,
                        totalrows = statusobject.totalrows,
                        result = statusobject.hascompleted ?statusobject.result:null,
                        faileddata = statusobject.hascompleted ? statusobject.faileddata : null
                    };
                }

            }
            catch (Exception exp)
            {

            }
            return new ProgressStatus()
            {
                status = "Failed"
            };
        }
        public ProgressStatus Start(ProcessData data)
        {
            ProgressStatus progressStatus = new ProgressStatus();
            progressStatus.identifier = Guid.NewGuid().ToString();
            progressStatus.percentage = 0;
            data.progress = progressStatus;
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(Process));
            thread.Start(data);
            progressStatuses.Add(progressStatus);
            return progressStatus;
        }

        public List<ProgressStatus> progressStatuses = new List<ProgressStatus>();

        public void Process(object Source)
        {
            ProcessData data = Source as ProcessData;
            System.Threading.Thread.Sleep(1000);
            try
            {
                if (data.action == "PrepareUAN")
                {
                    PrepareUAN(data);
                }
                else if (data.action == "UploadQTKart")
                {
                    UploadQTKart(data);
                }
                else if (data.action == "BuildStockTransaction")
                {
                    ProcessStockTransaction(data);
                }
                else if(data.action == "DataCache")
                {
                    ProcessDataCache(data);
                }
                else if (data.action == "ITEMMIGRATION")
                {
                    MigrationItems(data);
                }
                else if (data.action == "REPORTBUILDER")
                {
                    DumpReportBuilder(data);
                }
                else
                {
                    data.progress.Exception = new Exception(data.action + " not found in the system");
                    data.progress.hascompleted = true;
                    data.progress.percentage = 100;
                }
            }
            catch (Exception exp)
            {
                data.progress.status = exp.Message;
                data.progress.hascompleted = true;
                data.progress.Exception = exp;
                data.progress.percentage = 100;
                //Logger.Instance(this.organizationid).Log(exp, Logger.LogType.Info, Source);
            }
        }


        private void ProcessDataCache(ProcessData data)
        {
            try
            {
                
                ItemsDataService itemsDataService = new ItemsDataService() { Context = data.Context };
                InfyPOS.Processors.DBViewCache dBViewCache = new DBViewCache() { Context = data.Context };
                dBViewCache.RunAll(data.progress);
                data.progress.percentage = 100;
                data.progress.hascompleted = true;
                data.progress.status = "Completed";
            }
            catch (Exception exp)
            {
                data.progress.percentage = 100;
                data.progress.hascompleted = false;
                data.progress.Exception = exp;
                data.progress.status = exp.Message;
            }
        }


        private void MigrationItems(ProcessData data)
        {
            try
            {
                ItemsDataService itemsDataService = new ItemsDataService() { Context = data.Context };
                data.migrationdata.progress = data.progress;
                itemsDataService.MigrateItemData(data.migrationdata);
                
                data.progress.percentage = 100;
                data.progress.hascompleted = true;
                data.progress.status = "Completed";
            }
            catch (Exception exp)
            {
                data.progress.percentage = 100;
                data.progress.hascompleted = false;
                data.progress.Exception = exp;
                data.progress.status = exp.Message;
            }
        }

        public void DumpReportBuilder(ProcessData data)
        {
            try
            {
                ReportBuilderDataService reportdata = new ReportBuilderDataService();
                ReportBuilder result = reportdata.BuildReport(data.ReportBuilderRequest);
                data.progress.percentage = 50;

                System.Data.DataTable dataTable = new System.Data.DataTable("Data");
                result.columnlist.ForEach(x =>
                {
                    dataTable.Columns.Add(x.name, typeof(string));
                });
                result.rangelist.ForEach(x =>
                {
                    dataTable.Columns.Add(x.name, typeof(string));
                });
                result.aggregatelist.ForEach(x =>
                {
                    dataTable.Columns.Add(x.name, typeof(decimal));
                });

                result.result.ForEach(x =>
                {
                    List<object> values = new List<object>();

                    result.columnlist.ForEach(c =>
                    {
                        values.Add(x.String[c.index]);
                    });
                    result.rangelist.ForEach(c =>
                    {
                        values.Add(x.RangeResult[c.index]);
                    });
                    result.aggregatelist.ForEach(c =>
                    {
                        values.Add(x.Amount[c.index]);
                    });
                    dataTable.Rows.Add(values.ToArray());
                });

                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                    var sheet = xLWorkbook.AddWorksheet(dataTable, "Products");
                    xLWorkbook.SaveAs(memoryStream);
                    data.progress.resultbytes = memoryStream.ToArray();
                }
                data.progress.percentage = 100;
                data.progress.hascompleted = true;
                data.progress.status = "Completed";
            }
            catch (Exception exp)
            {
                data.progress.percentage = 100;
                data.progress.hascompleted = false;
                data.progress.Exception = exp;
                data.progress.status = exp.Message;
            }
        }

        public void ProcessStockTransaction(ProcessData data)
        {
            StocktransactionDataService onlineIntegration = new StocktransactionDataService()
            {
                Context = new UserClaim()
                {
                    OrganizationID = data.organizationid
                }
            };
            onlineIntegration.Configure(data);
        }
        public void UploadQTKart(ProcessData data)
        {
            OnlineIntegration onlineIntegration = new OnlineIntegration(organizationid);
            onlineIntegration.Upload2QtKartAsync(data);
        }
        public void PrepareUAN(ProcessData data)
        {
            //for(var i=0;i<100;i++)
            //{
            //    System.Threading.Thread.Sleep(1000);
            //    data.progress.percentage = i + 1;
            //}
            //return;

            //Execute these script before run
            /*
            update stock set serialno='' where barcode=serialno;
            with uanno
            as
            (
            select distinct serialno from stock 
            ),dstock as
            (
            select stock.id, stock.barcode,stock.serialno,stock.price,
            dstock.barcode dbarcode,dstock.price dprice,uanno.serialno userialno from stock
            inner join uanno on uanno.serialno = stock.barcode
            inner join stock dstock on dstock.serialno = uanno.serialno
            ),singleduplicate
            as
            (
            select userialno,count(*) from dstock
            group by userialno
            having count(*) =1
            )
            update stock set  serialno=dstock.userialno, barcode = 'U'||stock.barcode from dstock
            where dstock.userialno = stock.barcode
            */

            InfyPOS.Data.StockDataService stockDataService = new Data.StockDataService() { Context = data.Context, IsMigration = true };
            stockDataService.PrepareUAN(data);
        }
    }
}
