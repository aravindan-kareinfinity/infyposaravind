
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
    [RoutePrefix("PricechangeService")]
    public class PricechangeServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Pricechange>();
            result.Item = new Pricechange();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PricechangeCreteria> request)
        {
            PricechangeDataService PricechangeData = new PricechangeDataService();
            List<Pricechange> result = await Task<List<Pricechange>>.Run(() => PricechangeData.Search(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("PriceSnapshot")]
        //public async Task<IHttpActionResult> PriceSnapshot(ActionRequest<PricechangeCreteria> request)
        //{
        //    PricechangeDataService PricechangeData = new PricechangeDataService();
        //    int result = await Task<int>.Run(() => PricechangeData.PriceSnapshot(request.Item));
        //    return this.Ok(result);
        //}

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<PricechangeCreteria> request)
        {
            PricechangeDataService PricechangeData = new PricechangeDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Pricechange result = await Task<Pricechange>.Run(() => PricechangeData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Pricechange result = await Task<Pricechange>.Run(() => PricechangeData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<PricechangeCreteria> request)
        {
            PricechangeDataService PricechangeData = new PricechangeDataService();
            Pricechange result = await Task<Pricechange>.Run(() => PricechangeData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Pricechange> request)
        {
            PricechangeDataService PricechangeData = new PricechangeDataService();
            Pricechange result = await Task<Pricechange>.Run(() => PricechangeData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Pricechange> request)
        {
            PricechangeDataService PricechangeData = new PricechangeDataService();
            bool result = await Task<bool>.Run(() => PricechangeData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<Discountscheme.FilterCreteria> request)
        {
            if (request.Item.id > 0)
            {
                PricechangeDataService stockdataservice = new PricechangeDataService();
                List<Pricechange.ItemDetail> result = await Task<List<Pricechange.ItemDetail>>.Run(() => stockdataservice.SearchItems(request.Item));
                return this.Ok(result);
            }
            else
            {
                PricechangeDataService stockdataservice = new PricechangeDataService();
                List<Pricechange.ItemDetail> result = await Task<List<Pricechange.ItemDetail>>.Run(() => stockdataservice.SearchItems(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<BarcodeList> request)
        {
            List<Stock2Print> result = new List<Stock2Print>();
            List<long> stockids = new List<long>();
            List<string> inputbarcodes = new List<string>();
            StockDataService LrinvoiceitemsData = new StockDataService();

            if (request.Item.barcodes == null || request.Item.barcodes.Count == 0) return this.NotFound();

            if (request.Item.barcodes != null && request.Item.barcodes.Count > 0)
            {
                request.Item.barcodes.ForEach(e => inputbarcodes.Add("'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'"));
                List<Stock> lststock = LrinvoiceitemsData.Search(new StockCreteria() { barcodes = inputbarcodes });
                if (lststock != null && lststock.Count > 0)
                {
                    request.Item.barcodes.ForEach(e => e.stockid = lststock.Find(ex => ex.barcode == e.barcode).id);
                    request.Item.barcodes.ForEach(e => stockids.Add(e.stockid));
                }
            }

            if (stockids == null || stockids.Count == 0) return this.NotFound();

            if (!request.Item.iswarehouse && request.Item.locationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = request.Item.locationid
                });
                if (locationinfo.Count > 0 && locationinfo[0].additionalinfo != null && locationinfo[0].additionalinfo.iswarehouse)
                    request.Item.iswarehouse = true;
            }

            if (request.Item.iswarehouse)
            {
                result = LrinvoiceitemsData.SearchForIds(null, string.Join(",", stockids));
            }
            else
            {
                result = LrinvoiceitemsData.SearchForIds(null, string.Join(",", stockids), request.Item.locationid);
                var missingstockids = stockids.FindAll(x => !result.Exists(y => y.id == x));
                if(missingstockids.Count > 0)
                {
                    if (result == null) result = new List<Stock2Print>();
                    var missingresult = LrinvoiceitemsData.SearchForIds(null, string.Join(",", missingstockids));
                    if (missingresult == null) missingresult = new List<Stock2Print>();
                    result.AddRange(missingresult);
                }
            }

            if (result == null || result.Count == 0) return this.NotFound();

            foreach (Stock2Print item in result)
            {
                SupplierDataService supplierdate = new SupplierDataService();
                if (item.supplierid == item.sourcesupplierid || item.sourcesupplierid == 0)
                    item.sourcesuppliercode = item.suppliercode;
                else
                    item.sourcesuppliercode = supplierdate.Search(new SupplierCreteria { id = item.sourcesupplierid }).First().code;
            }

            if (request.Item.barcodes != null && request.Item.barcodes.Count > 0)
            {
                result.RemoveAll(e => !request.Item.barcodes.Exists(ex => ex.barcode == e.barcode));
                result.ForEach(e => e.noofcopy = Convert.ToInt32(Math.Ceiling(request.Item.barcodes.Find(ex => ex.barcode == e.barcode).noofcopy)));
                result.ForEach(e => e.OIndex = request.Item.barcodes.Find(ex => ex.barcode == e.barcode).selectindex);
            }

            // COMMENTED BECAUSE ITS DONE AT THE UI SIDE
            //if (InfyPOS.Services.ConfigManager.Instance(LrinvoiceitemsData.Context.OrganizationID).Config.system_splitbitprice)
            //{
            //    result.ForEach(e => e.price = e.isbit == true ? (e.price * e.qty) : e.price);
            //    result.ForEach(e => e.discount = (e.isbit == true && e.discount > 0) ? (e.discount * e.qty) : e.discount);
            //    result.ForEach(e => e.mrp = e.isbit == true ? (e.price * e.qty) : e.price);
            //}
            
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count > 0)
            {
                result.ForEach(e => e.Index = 1);
                List<Stock2Print> duplicate = new List<Stock2Print>();
                foreach (var item in result)
                {
                    for (int i = 1; i < item.noofcopy; i++)
                    {
                        var citem = item.Clone();
                        citem.Index = i+1;
                        duplicate.Add(citem);
                    }
                }

                if (duplicate.Count > 0)
                {
                    result.AddRange(duplicate);
                }

                result = result.OrderBy(e => e.OIndex).ToList();


                if (reportformats[0].reportformat.format.Contains("MARKER"))
                {
                    LrinvoiceitemsData.UpdateStockMarker(result);
                }
                if (reportformats[0].reportformat.format.Contains("PRDEXPIRY"))
                {
                    LrinvoiceitemsData.UpdateStockProductExpiry(result);
                }
                if (reportformats[0].reportformat.format.Contains("SIZEGROUP"))
                {
                    LrinvoiceitemsData.UpdateSizeGroup(result);
                }

                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                    reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

                List<string> base64Response = new List<string>();
                StringBuilder sb = new StringBuilder();
                StringBuilder asb = new StringBuilder();

                var barcodeList = result.ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(barcodeList);
                foreach (var item in response)
                {
                    asb.AppendLine(item);
                    asb.Append(";");
                    var bytes = Encoding.ASCII.GetBytes(item);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                    //base64Response.Add(Convert.ToBase64String(bytes));
                }

                if(request.Item.disable64)
                    base64Response.Add(asb.ToString());
                else
                    base64Response.Add(sb.ToString());
                return this.Ok(base64Response);
                
            }
            return this.NotFound();

        }
        public class BarcodeList : BaseEntity
        {
            public bool disable64 { get; set; }
            public List<Items> barcodes { get; set; }
            public bool iswarehouse { get; set; }
            public long locationid { get; set; }
            public long reportid { get; set; }
            public string companytitle { get; set; }
            public class Items
            {
                public int selectindex { get; set; }
                public string barcode { get; set; }
                public decimal noofcopy { get; set; }
                public long stockid { get; set; }
                public long itemid { get; set; }
            }
        }
    }
}