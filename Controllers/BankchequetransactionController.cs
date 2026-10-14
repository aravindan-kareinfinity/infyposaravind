
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
using System.Drawing.Printing;
using ReportLibrary;
using System.Collections;

namespace InfyPOS.Controllers
{
    [RoutePrefix("BankchequetransactionService")]
    public class BankchequetransactionServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Bankchequetransaction>();
            result.Item = new Bankchequetransaction();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("BRS")]
        public async Task<IHttpActionResult> BRS(ActionRequest<BRSCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            BRSSummary result = await Task<BRSSummary>.Run(() => BankchequetransactionData.BRS(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SaveBRS")]
        public async Task<IHttpActionResult> SaveBRS(ActionRequest<BRSCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            List<BRSLedger> result = await Task<List<BRSLedger>>.Run(() => BankchequetransactionData.SaveBRS(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BankchequetransactionCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            List<Bankchequetransaction> result = await Task<List<Bankchequetransaction>>.Run(() => BankchequetransactionData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(ActionRequest<BankchequetransactionCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            bool result = await Task<bool>.Run(() => BankchequetransactionData.ChangeStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Bankchequetransaction> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            if (request.Item.id <= 0)
            {
                Bankchequetransaction result = await Task<Bankchequetransaction>.Run(() => BankchequetransactionData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Bankchequetransaction result = await Task<Bankchequetransaction>.Run(() => BankchequetransactionData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Bankchequetransaction> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            Bankchequetransaction result = await Task<Bankchequetransaction>.Run(() => BankchequetransactionData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Bankchequetransaction> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            Bankchequetransaction result = await Task<Bankchequetransaction>.Run(() => BankchequetransactionData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Bankchequetransaction> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            bool result = await Task<bool>.Run(() => BankchequetransactionData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ChequePDF")]
        public async Task<IHttpActionResult> ChequePDF(ActionRequest<BankchequetransactionCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => CreatePDF(BankchequetransactionData.Context.OrganizationID,
                request.Item.reportid, request.Item.ids, false));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

        [HttpPost]
        [Route("ChequeSummaryPDF")]
        public async Task<IHttpActionResult> CreateSummaryPDF(ActionRequest<BankchequetransactionCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            FileContentInfo byteArray = await Task<FileContentInfo>.Run(() => BankchequetransactionData.PrintSummaryPDF(request.Item.ids));
            if (byteArray == null)
                return this.NotFound();
            return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
        }

        

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<BankchequetransactionCreteria> request)
        {
            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            string result = await Task<byte[]>.Run(() => CreateJSON(BankchequetransactionData.Context.OrganizationID,
                request.Item.reportid, request.Item.ids));
            return this.Ok(result);
        }

        private string CreateJSON(long organizationid, long reportid, List<long> ids)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            List<Configuration> reportformats = config.Search(new ConfigurationCreteria
            {
                id = reportid
            });

            var rptDesigner = LabelDesigner.ReportDesigner.FromJson(reportformats[0].reportformat.format);
            

            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            BankchequetransactionData.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var result = BankchequetransactionData.Search(new BankchequetransactionCreteria()
            {
                ids = ids
            }).ConvertAll(e => new ChequePrint(e) as InfyPOS.Report.IBarcodeFormat);

            return Convert.ToBase64String(GenerateJSON(rptDesigner, result));
        }

        private FileContentInfo CreatePDF(long organizationid, long rptid, List<long> ids, bool isjson)
        {

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            List<Configuration> reportformats = config.Search(new ConfigurationCreteria
            {
                id = rptid
            });

            var rptDesigner = LabelDesigner.ReportDesigner.FromJson(reportformats[0].reportformat.format);

            BankchequetransactionDataService BankchequetransactionData = new BankchequetransactionDataService();
            BankchequetransactionData.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var result = BankchequetransactionData.Search(new BankchequetransactionCreteria()
            {
                ids = ids
            }).ConvertAll(e => new ChequePrint(e) as InfyPOS.Report.IBarcodeFormat);

            //ReportLibrary.DefaultPageSettings pagesetting = new ReportLibrary.DefaultPageSettings()
            //{
            //    Landscape = false,
            //    PaperSize = new PaperSize("Cheque", (int)(PdfSharp.Drawing.XUnit.FromMillimeter(210).Point), (int)(PdfSharp.Drawing.XUnit.FromMillimeter(90).Point)),
            //    Margins = new Margins(0, 0, 0, 0),
            //    Unit = PdfSharp.Drawing.XGraphicsUnit.Millimeter
            //};
            //var bytes = Generate(Newtonsoft.Json.JsonConvert.SerializeObject(CF2010()), pagesetting, result);
            //System.IO.File.WriteAllBytes(@"e:\test.pdf", bytes);
            if (isjson)
            {
                var bytes = GenerateJSON(rptDesigner, result);
                var fci = new FileContentInfo(new ByteArrayContent(bytes));
                fci.FileName = "ChequePrint.PDF";
                fci.Extension = "application/pdf";
                return fci;
            }
            else
            {
                var bytes =  Generate(rptDesigner, result);
                var fci = new FileContentInfo(new ByteArrayContent(bytes));
                fci.FileName = "ChequePrint.PDF";
                fci.Extension = "application/pdf";
                return fci;
            }
        }

        public byte[] GenerateJSON(LabelDesigner.ReportDesigner rptDesign,
           List<InfyPOS.Report.IBarcodeFormat> entityList)
        {
            var templates = rptDesign.Elements();
            var label = new PDFLabel();
            label.convertopixel = true;
            label.Template = templates;
            label.columns = 1;
            label.AddLabel(entityList);
            ReportLibrary.JSONPrinter pDFPrinter = new ReportLibrary.JSONPrinter();
            pDFPrinter.Report = label;
            var document = pDFPrinter.Print(rptDesign.GetDefaultSetting(new ReportLibrary.DosFormat.PageSettings()
            {
                Height = (int)rptDesign.Height,
                Width = (int)rptDesign.Width
            }));
            return System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document));
        }
        public byte[] Generate(LabelDesigner.ReportDesigner rptDesign,
            List<InfyPOS.Report.IBarcodeFormat> entityList)
        {
            var templates = rptDesign.Elements();
            var label = new PDFLabel();
            label.convertopixel = true;
            label.Template = templates;
            label.AddLabel(entityList);
            ReportLibrary.PDFPrinter pDFPrinter = new ReportLibrary.PDFPrinter();
            pDFPrinter.Report = label;
            return pDFPrinter.Print(rptDesign.GetDefaultSetting(new ReportLibrary.DosFormat.PageSettings()
            {
                Height = (int) rptDesign.Height,
                Width = (int)rptDesign.Width
            })).ToArray();
        }

        public byte[] Generate(string elementjson, ReportLibrary.DefaultPageSettings setting, List<InfyPOS.Report.IBarcodeFormat> entityList)
        {
            var templates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<LabelElement>>(elementjson);
            var label = new PDFLabel();
            label.convertopixel = true;
            label.Template = templates;
            label.AddLabel(entityList);
            ReportLibrary.PDFPrinter pDFPrinter = new ReportLibrary.PDFPrinter();
            pDFPrinter.Report = label;


            setting.MarginBound = new System.Drawing.Rectangle(setting.Margins.Left, setting.Margins.Top, setting.PageBounds.Width -
               (setting.PageBounds.Left + setting.PageBounds.Right), setting.PageBounds.Height -
               (setting.PageBounds.Top + setting.PageBounds.Bottom));
            return pDFPrinter.Print(setting).ToArray();
        }


        private List<LabelElement> CF2010()
        {
            List<LabelElement> labelElements = new List<LabelElement>();
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "name",
                left = 15,
                top = 20,
                width = 140
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-day1",
                left = 143,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-day2",
                left = 150,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-month1",
                left = 157,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-month2",
                left = 164,
                top = 10,
                width = 5
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year1",
                left = 171,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year2",
                left = 178,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year3",
                left = 185,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "date-year4",
                left = 192,
                top = 10,
                width = 5
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "words-1",
                left = 30,
                top = 28,
                width = 150
            });
            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "words-2",
                left = 30,
                top = 35,
                width = 110
            });

            labelElements.Add(new LabelElement()
            {
                Type = LabelElement.type.Field,
                text = "amount",
                left = 160,
                top = 35,
                width = 30,
                valign = LabelElement.Align.Right
            });

            return labelElements;
        }
        public class ChequePrint : InfyPOS.Report.IBarcodeFormat
        {
            public bool SpecialField(string field, SortedDictionary<string, Object> data)
            {
                return false;
            }
            public ChequePrint(Bankchequetransaction bankchequetransaction)
            {
                this.name = bankchequetransaction.accountname;
                this.amount = bankchequetransaction.amount;
                this.date = bankchequetransaction.chequedate;
                var words = WebAPI.Data.Numberconvertor.rupees(amount);
                this.word1 = "";
                this.word2 = "";
                foreach (var word in words.Split(' '))
                {
                    if (word1.Length > 40)
                    {
                        word2 += (word2.Length > 0 ? " " : "") + word;
                    }
                    else
                    {
                        word1 += (word1.Length > 0 ? " " : "") + word;
                    }
                }
            }
            public string name { get; set; }
            public decimal amount { get; set; }
            public DateTime date { get; set; }
            public string word1 { get; set; }
            public string word2 { get; set; }
            public SortedDictionary<string, object> KeyValue
            {
                get
                {
                    SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                    ret.Add("Account", name);
                    ret.Add("Amount", amount);
                    ret.Add("Date", date);
                    ret.Add("Date-Day01", date.ToString("dd")[0].ToString());
                    ret.Add("Date-Day02", date.ToString("dd")[1].ToString());
                    ret.Add("Date-Month01", date.ToString("MM")[0].ToString());
                    ret.Add("Date-Month02", date.ToString("MM")[1].ToString());
                    ret.Add("Date-Year01", date.ToString("yyyy")[0].ToString());
                    ret.Add("Date-Year02", date.ToString("yyyy")[1].ToString());
                    ret.Add("Date-Year03", date.ToString("yyyy")[2].ToString());
                    ret.Add("Date-Year04", date.ToString("yyyy")[3].ToString());
                    ret.Add("InWords-01", word1);
                    ret.Add("InWords-02", word2);
                    return ret;
                }
            }

            public SortedDictionary<string, string> KeyFormat
            {
                get
                {
                    SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                    ret.Add("Date", "dd-MM-yyyy");
                    ret.Add("Amount", "F0");
                    return ret;
                }
            }
        }
    }
}