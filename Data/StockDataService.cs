using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;
using System.Drawing;
using System.IO;
using InfyPOS.Processors;
using InfyPOS.Controllers;

namespace InfyPOS.Data
{

    /*
    select buyingprice,basebuyingprice,purchasetaxpercentage,buyingqty,purchasetax,((buyingprice::numeric*buyingqty::numeric*purchasetaxpercentage::numeric)/100) 
    from lrinvoiceitems where purchasetax != ((buyingprice::numeric*buyingqty::numeric*purchasetaxpercentage::numeric)/100)  and purchasetaxpercentage>0

    select lrinvoiceitems.id,stock.invoiceitemid,lrinvoiceitems.buyingprice , stock.ibuyingprice,
    ((lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty)::numeric+lrinvoiceitems.buyingprice::numeric) newibuyingprice
    from stock
    inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid 
    where  ibuyingprice != ((lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty)::numeric+lrinvoiceitems.buyingprice::numeric)
    and lrinvoiceitems.purchasetax>0 and lrinvoiceitems.buyingqty>0

    update lrinvoiceitems set purchasetax = ((buyingprice::numeric*buyingqty::numeric*purchasetaxpercentage::numeric)/100) 
    where purchasetaxpercentage>0 and purchasetax != ((buyingprice::numeric*buyingqty::numeric*purchasetaxpercentage::numeric)/100) 

    update stock set ibuyingprice = ((lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty)::numeric+lrinvoiceitems.buyingprice::numeric)
    from lrinvoiceitems 
    where lrinvoiceitems.id = stock.invoiceitemid and ibuyingprice != ((lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty)::numeric+lrinvoiceitems.buyingprice::numeric)
    and lrinvoiceitems.purchasetax>0 and lrinvoiceitems.buyingqty>0*/

    public class StockDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, printingname, price, barcode, companyid, invoiceitemid, itemid, runtimeprice,barcodeid,dealerprice,isuan,ismultiprice,serialno,pricetagid,
                                    productid,discount,additionaldiscount,qty,transfered,damagedqty,journalqty,returnqty,srcsupplierid,supplierid,buyingprice,ibuyingprice,lrreference,designid,hsncode,isbit,stockdate,imageid,locationid,retaillocationid,displaymrp from stock where id=@id";

        const string SQL_BundleStock = @"select stock.* from bundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                        inner join stock ON (schemelist ->> 'stockid') :: BIGINT = stock.id
                                        where bundles.id=@bundleid and bundles.isactive=true)";

        public const string SQL_ValidateBarcode = @"Select barcode from Salablegoods where barcode in ({0});
                                             Select barcode from Stock where barcode in ({0})";

        internal StockInfo SearchInfo(long companyid, string barcode)
        {
            var stockinfo = Search(companyid, barcode);
            string query = @"select lrinvoiceitems.lrinvoiceid, lrinvoiceitems.id itemid,
items.productid,items.brandid,lrinvoice.entrydate,
items.styleid,items.patternid,items.materialid,stock.supplierid, stock.srcsupplierid from stock
inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
inner join items on items.id = stock.itemid
where stock.barcode = @barcode";
            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("barcode", barcode);
            DateTime entrydate = DateTime.MinValue;
            using (var reader = ExecuteReader(query, parameter))
            {
                if (reader.Read())
                {
                    entrydate = reader["entrydate"] != DBNull.Value ? (DateTime)reader["entrydate"] : DateTime.MinValue;
                    parameter.Add("lrinvoiceid", reader["lrinvoiceid"]);
                    parameter.Add("itemid", reader["itemid"]);
                    parameter.Add("productid", reader["productid"]);
                    parameter.Add("brandid", reader["brandid"]);
                    parameter.Add("styleid", reader["styleid"]);
                    parameter.Add("patternid", reader["patternid"]);
                    parameter.Add("materialid", reader["materialid"]);
                    parameter.Add("supplierid", reader["supplierid"]);
                }
                else
                {
                    return new StockInfo();
                }
            }

            query = @"select location.name locationname,salablegoods.barcode,
                        iqty-(oqty+dqty+COALESCE(rqty,0)) stock,colour.name colour,products.name product,
                        size.name as size,style.name style,pattern.name pattern,brand.name brand
                        from salablegoods 
                        inner join stock on stock.id = stockid
                        inner join items on items.id = stock.itemid
                        inner join products on products.id = items.productid
                        left outer join brand on brand.id = items.brandid
                        left outer join referencelist colour on colour.id = colourid
                        left outer join referencelist size on size.id = sizeid
                        left outer join referencelist style on style.id = styleid
                        left outer join referencelist pattern on pattern.id = patternid
                        inner join referencelist location on location.id = salablegoods.locationid
                        where items.productid=@productid and COALESCE(items.brandid,0)=@brandid and COALESCE(patternid,0)=@patternid 
                        and COALESCE(styleid,0)=@styleid and iqty-(oqty+dqty+COALESCE(rqty,0))>0";

            List<StockInfoDetail> source = new List<StockInfoDetail>();
            using (var reader = ExecuteReader(query, parameter))
            {

                while (reader.Read())
                {
                    StockInfoDetail stockInfoDetail = new StockInfoDetail();
                    source.Add(stockInfoDetail);
                    stockInfoDetail.locationname = reader["locationname"].ToString();
                    stockInfoDetail.barcode = reader["barcode"].ToString();
                    stockInfoDetail.product = reader["product"].ToString();
                    stockInfoDetail.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                    stockInfoDetail.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                    stockInfoDetail.style = reader["style"] == DBNull.Value ? "" : reader["style"].ToString();
                    stockInfoDetail.pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                    stockInfoDetail.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                    stockInfoDetail.stock = Convert.ToDecimal(reader["stock"]);
                }
            }

            var r = new StockInfo(source);
            r.entrydate = entrydate;
            r.age = DateTimeNow.Subtract(entrydate).TotalDays.ToString("F0") + " days";
            return r;
        }



        public class StockInfo
        {
            public StockInfo()
            {

            }
            public StockInfo(List<StockInfoDetail> source)
            {
                this.source = source;
                if (source.Count > 0)
                {
                    Colour = source.GroupBy(e => e.colour).ToList().ConvertAll(e => e.Key);
                    Colour.RemoveAll(e => string.IsNullOrEmpty(e));
                    Size = source.GroupBy(e => e.size).ToList().ConvertAll(e => e.Key);
                    Size.RemoveAll(e => string.IsNullOrEmpty(e));
                    Style = source.OrderBy(e => e.style.Length).Last().style;
                    Pattern = source.First().pattern;
                    Brand = source.First().brand;
                    Product = source.First().product;
                    List = source.GroupBy(barcode => new { barcode.size, barcode.colour, barcode.locationname })
                        .Select(group => new StockInfoSummary()
                        {
                            locationname = group.Key.locationname,
                            size = group.Key.size,
                            colour = group.Key.colour,
                            stock = group.Sum(e => e.stock)
                        }).ToList();
                }
            }
            public List<string> Colour { get; set; }
            public List<string> Size { get; set; }
            public string Style { get; set; }
            public string Pattern { get; set; }
            public string Product { get; set; }
            public string Brand { get; set; }
            public List<StockInfoSummary> List { get; set; }

            public List<StockInfoDetail> source { get; set; }
            public string barcode { get; set; }
            public DateTime entrydate { get; set; }
            public string age { get; set; }
        }
        public class StockInfoSummary
        {
            public string locationname;
            public decimal stock;
            public string colour;
            public string size;
        }
        public class StockInfoDetail : StockInfoSummary
        {
            public string barcode;
            public string product;
            public string style;
            public string pattern;
            public string brand;
        }
        public class BarcodeID
        {
            public string barcode { get; set; }
            public long stockid { get; set; }
            public long salableid { get; set; }
            public long locationid { get; set; }
        }
        public class CuttingBarcode : BaseEntity
        {
            public string empcode { get; set; }
            public long organizatinoid { get; set; }
            public string location { get; set; }
            public long locationid { get; set; }
            public string barcode { get; set; }
            public string employeecode { get; set; }
            public string qty { get; set; }
            public decimal vqty { get; set; }
            public string employeename { get; set; }
            public string productname { get; set; }
            public decimal price { get; set; }
            public decimal amount
            {
                get
                {
                    return (price - discount) * vqty;
                }
            }
            public decimal discount { get; set; }
            public decimal final { get; set; }
            public decimal mrp { get; set; }
            public string printdocument { get; set; }
            public string ebarcode { get; set; }
        }
        public Dictionary<string, List<BarcodeID>> SearchALLBarcode()
        {
            string query = @"select id,barcode,itemid from stock where isactive=true;
                            select id,barcode,stockid,locationid from Salablegoods where isactive=true";

            List<BarcodeID> barcodelist = new List<BarcodeID>();
            List<BarcodeID> sbarcodelist = new List<BarcodeID>();

            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    BarcodeID barcode = new BarcodeID();
                    barcode.barcode = reader.GetString(1);
                    barcode.stockid = reader.GetInt64(0);
                    barcode.salableid = reader.GetInt64(2);
                    barcodelist.Add(barcode);
                    //if (list.ContainsKey(barcode.barcode))
                    //{
                    //    Console.WriteLine("Duplicate barcode" + barcode.barcode);
                    //}
                    //else
                    //{
                    //    list.Add(barcode.barcode, barcode);
                    //}
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {

                        //if (list.ContainsKey(reader.GetString(1)))
                        //{
                        //    list[reader.GetString(1)].salableid = reader.GetInt64(0);
                        //}
                        //else
                        //{
                        BarcodeID barcode = new BarcodeID();
                        barcode.barcode = reader.GetString(1);
                        barcode.salableid = reader.GetInt64(0);
                        barcode.stockid = reader.GetInt64(2);
                        barcode.locationid = reader.GetInt64(3);
                        barcodelist.Add(barcode);
                        //list.Add(barcode.barcode, barcode);
                        //}
                    }
                }

            }
            Dictionary<string, List<BarcodeID>> retValue = new Dictionary<string, List<BarcodeID>>();
            var sitem = barcodelist.GroupBy(e => e.barcode);
            foreach (var item in sitem)
            {
                retValue.Add(item.Key, new List<BarcodeID>(item));
            }
            return retValue;
        }

        internal int ChangeHSN(List<BarcodeHSN> items)
        {
            var results = items.ChunkBy(1000);
            List<string> querylist = new List<string>();
            foreach (var r in results)
            {
                var queryitems = r.ConvertAll(e =>
                {
                    return "{" + string.Format("\"barcode\":\"{0}\",\"hsn\":\"{1}\"", e.barcode, e.hsn) + "}";
                });
                querylist.Add(@"[" + string.Join(",", queryitems) + "]");
            }

            int noofaffected = 0;
            foreach (var query in querylist)
            {
                string hsnquery = string.Format(@"with hsnbarcodelist as
(
	select value->>'barcode' barcode, value->>'hsn' hsn from json_array_elements ('{0}'::json)
)
update stock set hsncode = hsn from hsnbarcodelist
where stock.barcode = hsnbarcodelist.barcode", query);
                noofaffected += ExecuteQuery(hsnquery, new SortedList<string, object>());
            }
            return noofaffected;
        }

        public string StoreImage(string image, long invoiceitemid, string barcode)
        {
            try
            {
                string[] parts = image.Split(',');
                ImageData imageData = new ImageData() { Context = this.Context };
                byte[] dataasbytes = Convert.FromBase64String(parts[1]);
                var file = new ImageData.File();
                file.createdby = Context.UserID;
                file.modifiedby = Context.UserID;
                file.createdon = DateTimeNow;
                file.modifiedon = DateTimeNow;
                file.filename = barcode;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(dataasbytes))
                {
                    Image img = Image.FromStream(ms);
                    var thumbimg = WebAPI.Common.ImageManager.ScaleBySize(img, 128);
                    file.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg, 90);
                    file.data = WebAPI.Common.ImageManager.ToByteArray(img, 60);
                }
                file.ext = ".jpg";
                file.type = "jpg";
                file = imageData.Insert(file);

                if (invoiceitemid > 0)
                {
                    string query = @"update lrinvoiceitems set imgid=@imgid where id=@id;
                                     update stock set imageid=@imgid where invoiceitemid=@id;";
                    var param = new SortedList<string, object>();
                    param.Add("imgid", file.id.ToBase62());
                    param.Add("id", invoiceitemid);
                    ExecuteQuery(query, param);
                }
                return file.id.ToBase62();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return "";
        }
        internal StockImage UpdateSKUImage(StockImage item)
        {
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            string newuid = "";
            long id = mfgdesignDataService.GetDesignID(item.barcode, out newuid);
            if (id <= 0) throw new Exception("Cant find the sku design for " + item.barcode);
            if (!string.IsNullOrEmpty(newuid))
                item.barcode = newuid;
            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("id", id);
            if (!string.IsNullOrEmpty(item.imageid))
                parameter.Add("imageid", item.imageid);
            switch (item.Action)
            {
                case StockImage.Actions.Delete:

                    MfgdesignCode mfgdesignCode = new MfgdesignCode() { id = id };
                    if (string.IsNullOrEmpty(item.imageid))
                    {
                        using (var reader = ExecuteReader("Select imagelist from Mfgdesign where id=@id limit 1", parameter))
                        {
                            if (reader.Read())
                            {
                                mfgdesignCode.imagelist_json = reader.IsDBNull(0) ? "" : reader.GetString(0);
                                item.available = mfgdesignCode.imagelist != null && mfgdesignCode.imagelist.Count > 0;
                            }
                        }
                    }
                    else
                    {
                        mfgdesignCode = new MfgdesignCode() { id = id };
                        using (var reader = ExecuteReader("Select imagelist from Mfgdesign where id=@id limit 1", parameter))
                        {
                            if (reader.Read())
                            {
                                mfgdesignCode.imagelist_json = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            }
                        }
                        if (mfgdesignCode.imagelist == null)
                            mfgdesignCode.imagelist = new List<string>();
                        mfgdesignCode.imagelist.Remove(item.imageid);
                        mfgdesignCode.imagelist.RemoveAll(x => string.IsNullOrEmpty(x));
                        mfgdesignDataService.UpdateImageList(mfgdesignCode);
                        item.deleted = true;
                    }
                    break;
                case StockImage.Actions.Update:
                    mfgdesignCode = new MfgdesignCode() { id = id };
                    using (var reader = ExecuteReader("Select imagelist from Mfgdesign where id=@id limit 1", parameter))
                    {
                        if (reader.Read())
                        {
                            mfgdesignCode.imagelist_json = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            item.available = mfgdesignCode.imagelist != null && mfgdesignCode.imagelist.Count > 0;
                        }
                    }
                    if (mfgdesignCode.imagelist == null)
                        mfgdesignCode.imagelist = new List<string>();
                    mfgdesignCode.imagelist.Add(item.imageid);
                    mfgdesignCode.imagelist.RemoveAll(x => string.IsNullOrEmpty(x));
                    mfgdesignDataService.UpdateImageList(mfgdesignCode);
                    break;
                case StockImage.Actions.Get:
                    using (var reader = ExecuteReader("Select imagelist from Mfgdesign where id=@id limit 1", parameter))
                    {
                        if (reader.Read())
                        {
                            mfgdesignCode = new MfgdesignCode();
                            mfgdesignCode.imagelist_json = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            mfgdesignCode.imagelist.RemoveAll(x => string.IsNullOrEmpty(x));
                            if (mfgdesignCode.imagelist != null && mfgdesignCode.imagelist.Count > 0)
                                item.imageid = string.Join(",", mfgdesignCode.imagelist);
                            item.available = !string.IsNullOrEmpty(item.imageid);
                        }
                    }
                    break;
            }
            return item;
        }
        internal StockImage UpdateSKUCoverImage(StockImage item)
        {
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            string newuid = "";
            long id = mfgdesignDataService.GetDesignID(item.barcode, out newuid);
            if (id <= 0) throw new Exception("Cant find the sku design for " + item.barcode);
            if (!string.IsNullOrEmpty(newuid))
                item.barcode = newuid;

            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("id", id);
            if (!string.IsNullOrEmpty(item.imageid))
                parameter.Add("imageid", item.imageid);
            switch (item.Action)
            {
                case StockImage.Actions.Delete:
                    if (ExecuteQuery("Update Mfgdesign set imageid = '' where id=@id", parameter) == 1)
                    {
                        item.deleted = true;
                    }
                    break;
                case StockImage.Actions.Update:

                    if (ExecuteQuery("Update Mfgdesign set imageid = @imageid where id=@id", parameter) == 1)
                    {
                        item.updated = true;
                    }

                    break;
                case StockImage.Actions.Get:
                    using (var reader = ExecuteReader("Select imageid from Mfgdesign where id=@id limit 1", parameter))
                    {
                        if (reader.Read())
                        {
                            item.imageid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            item.available = !string.IsNullOrEmpty(item.imageid);
                        }
                    }
                    break;
            }
            return item;
        }
        internal StockImage UpdateEANImage(StockImage item)
        {
            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("barcode", item.barcode);
            if (!string.IsNullOrEmpty(item.imageid))
                parameter.Add("imageid", item.imageid);
            switch (item.Action)
            {
                case StockImage.Actions.Delete:
                    if (ExecuteQuery("Update stock set imageid = '' where serialno=@barcode", parameter) == 1)
                    {
                        item.deleted = true;
                    }
                    break;
                case StockImage.Actions.Update:
                    if (!string.IsNullOrEmpty(item.image) && item.imageid == null)
                    {
                        string imageid = StoreImage(item.image, 0, item.barcode);
                        parameter.Add("imageid", imageid);
                    }
                    if (item.billitemid > 0)
                    {
                        parameter.Add("id", item.billitemid);
                        if (ExecuteQuery("Update billitems set imageid = @imageid where id=@id", parameter) == 1)
                        {
                            item.updated = true;
                        }
                    }
                    else
                    {
                        if (ExecuteQuery("Update stock set imageid = @imageid where serialno=@barcode", parameter) == 1)
                        {
                            item.updated = true;
                        }
                    }
                    break;
                case StockImage.Actions.Get:
                    using (var reader = ExecuteReader("Select imageid from stock where serialno=@barcode limit 1", parameter))
                    {
                        if (reader.Read())
                        {
                            item.imageid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            item.available = !string.IsNullOrEmpty(item.imageid);
                        }
                    }
                    break;
            }
            return item;
        }
        internal StockImage UpdateStockImage(StockImage item)
        {
            SortedList<string, object> parameter = new SortedList<string, object>();
            parameter.Add("barcode", item.barcode);
            if (!string.IsNullOrEmpty(item.imageid))
                parameter.Add("imageid", item.imageid);
            switch (item.Action)
            {
                case StockImage.Actions.Delete:
                    if (ExecuteQuery("Update stock set imageid = '' where barcode=@barcode", parameter) == 1)
                    {
                        item.deleted = true;
                    }
                    break;
                case StockImage.Actions.Update:
                    if (!string.IsNullOrEmpty(item.image) && item.imageid == null)
                    {
                        string imageid = StoreImage(item.image, 0, item.barcode);
                        parameter.Add("imageid", imageid);
                    }
                    if (item.billitemid > 0)
                    {
                        parameter.Add("id", item.billitemid);
                        if (ExecuteQuery("Update billitems set imageid = @imageid where id=@id", parameter) == 1)
                        {
                            item.updated = true;
                        }
                    }
                    else
                    {
                        if (ExecuteQuery("Update stock set imageid = @imageid where barcode=@barcode", parameter) == 1)
                        {
                            item.updated = true;
                        }
                    }
                    break;
                case StockImage.Actions.Get:
                    using (var reader = ExecuteReader("Select imageid from stock where barcode=@barcode limit 1", parameter))
                    {
                        if (reader.Read())
                        {
                            item.imageid = reader.IsDBNull(0) ? "" : reader.GetString(0);
                            item.available = !string.IsNullOrEmpty(item.imageid);
                        }
                    }
                    break;
            }
            return item;
        }
        internal StockImage UpdateImage(StockImage item)
        {
            if (item.mode == StockImage.Modes.StockImage)
                return UpdateStockImage(item);
            if (item.mode == StockImage.Modes.EANImage)
                return UpdateEANImage(item);
            if (item.mode == StockImage.Modes.SKUCover)
                return UpdateSKUCoverImage(item);
            if (item.mode == StockImage.Modes.SKUImage)
                return UpdateSKUImage(item);
            return item;
        }

        public byte[] PrintPDFBarcode(LrinvoicePrintCreteria Item)
        {
            if (Item.stockids == null) Item.stockids = new List<long>();
            if (Item.items == null) Item.items = new List<long>();
            List<Stock2Print> result = SearchForIds(string.Join(",", Item.items), string.Join(",", Item.stockids));

            foreach (Stock2Print item in result)
            {
                SupplierDataService supplierdate = new SupplierDataService() { Context = this.Context };
                if (item.supplierid == item.sourcesupplierid || item.sourcesupplierid == 0)
                    item.sourcesuppliercode = item.suppliercode;
                else
                    item.sourcesuppliercode = supplierdate.Search(new SupplierCreteria { id = item.sourcesupplierid }).First().code;
            }
            if (Item.filterbarcode != null && Item.filterbarcode.Count > 0)
            {
                result.RemoveAll(e => !Item.filterbarcode.Exists(ex => ex.code == e.barcode));
                result.ForEach(e => e.noofcopy = Item.filterbarcode.Find(ex => ex.code == e.barcode).copy);
            }

            int index = 1;
            foreach (var item in result)
            {
                item.OIndex = index++;
            }
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = this.Context };
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = Item.reportid
            });

            result.ForEach(e => e.Index = 1);
            List<Stock2Print> duplicate = new List<Stock2Print>();
            foreach (var item in result.Where(e => e.qty > 0))
            {
                if (item.sellingmode == 2 && item.noofcopy == 0)
                    item.noofcopy = Convert.ToInt32(item.qty);

                item.noofcopy = 1;
                for (int i = 1; i < item.noofcopy; i++)
                {
                    var citem = item.Clone();
                    citem.Index = i + 1;
                    duplicate.Add(citem);
                }
            }

            if (duplicate.Count > 0)
            {
                result.AddRange(duplicate);
                result = result.OrderBy(e => e.OIndex).ToList();
            }
            var rptDesigner = LabelDesigner.ReportDesigner.FromJson(reportformats[0].reportformat.format);
            var templates = rptDesigner.Elements();
            var label = new ReportLibrary.PDFLabel();
            label.convertopixel = Item.IsPDF;
            label.Template = templates;
            //PdfSharp.Drawing.XUnit.FromMillimeter(value).Presentation
            label.pageleftmargin = reportformats[0].reportformat.leftmargin;
            label.labelheight = (int)rptDesigner.Height;
            label.labelwidth = (int)rptDesigner.Width;
            label.labelgap = reportformats[0].reportformat.labelgap;
            label.columns = reportformats[0].nooflines;
            label.Unit = PdfSharp.Drawing.XGraphicsUnit.Presentation;

            var pageheight = (int)rptDesigner.Height;
            var pagewidth = (int)Math.Ceiling((rptDesigner.Width * label.columns) + ((label.columns - 1) * label.labelgap));
            label.AddLabel(result.ConvertAll(e => e as InfyPOS.Report.IBarcodeFormat));

            if (Item.IsPDF)
            {
                ReportLibrary.JSONPrinter printer = new ReportLibrary.JSONPrinter();
                printer.Report = label;
                var rptconfig = rptDesigner.GetDefaultSetting(new ReportLibrary.DosFormat.PageSettings()
                {
                    Height = (int)Math.Ceiling(rptDesigner.Height),
                    Width = pagewidth + label.pageleftmargin,
                    LeftMargin = label.pageleftmargin,
                });
                rptconfig.Unit = PdfSharp.Drawing.XGraphicsUnit.Presentation;
                var document = printer.Print(rptconfig);
                return System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document));
            }
            else
            {
                ReportLibrary.PDFPrinter pDFPrinter = new ReportLibrary.PDFPrinter();
                pDFPrinter.Report = label;
                var rptconfig = rptDesigner.GetDefaultSetting(new ReportLibrary.DosFormat.PageSettings()
                {
                    Height = (int)Math.Ceiling(rptDesigner.Height),
                    Width = pagewidth + label.pageleftmargin,
                    LeftMargin = label.pageleftmargin,
                });
                rptconfig.Unit = PdfSharp.Drawing.XGraphicsUnit.Presentation;
                return pDFPrinter.Print(rptconfig).ToArray();
            }

        }

        internal StockSync ChangeCompany(StockSync item)
        {
            var missingitems = item.items.Where(e => e.salablegoodsid == 0).ToList();
            if (missingitems.Count > 0) throw new Exception("Company updation can't be done for non salable stock items");
            if (item.companyid <= 0) throw new Exception("Company details missing");

            string SQL_update = "update salablegoods set companyid=@companyid,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and companyid != @companyid";
            NpgsqlTransaction transaction = null;

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var itm in item.items)
                    {
                        if (itm.salablegoodsid > 0)
                        {
                            NpgsqlCommand command = GetCommand(SQL_update, conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.salablegoodsid;
                            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.companyid;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            int successCount = command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }

        internal void UpdateStockProductExpiry(List<Stock2Print> result)
        {
            string query = string.Format(@"select stock.id,stockholdingperiod,stockdate from stock
            inner join products on products.id = stock.productid
            where stock.isactive = true and stock.id in ({0})", string.Join(",", result.ConvertAll(e => e.id)));
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var id = reader.GetInt64(0);
                    var stockholdingperiod = reader.IsDBNull(2) ? 0 : reader.GetInt32(1);
                    var stockdate = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2);
                    if (stockholdingperiod > 0 && stockdate != DateTime.MinValue)
                    {
                        var prdexpiry = stockdate.AddDays(stockholdingperiod).ToString("dd-MM-yyyy");
                        result.FindAll(e => e.id == id).ForEach(e => e.prdexipry = prdexpiry);
                    }
                }
            }
        }

        internal void UpdateStockMarker(List<Stock2Print> result)
        {
            string query = string.Format(@"select referencelist.code,stockid from stockmarker
            inner join referencelist on referencelist.id = stockmarker.markerid
            where stockmarker.isactive = true and stockid in ({0})", string.Join(",", result.ConvertAll(e => e.id)));
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var code = reader.GetString(0);
                    var id = reader.GetInt64(1);
                    result.FindAll(e => e.id == id).ForEach(e => e.marker = code);
                }
            }
        }

        internal void UpdateSizeGroup(List<Stock2Print> result)
        {
            string query = string.Format(@"select stock.id,referencelist.name,referencelist.sortingindex from stock
            inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
            inner join lrinvoiceitems allinvoiceitems on allinvoiceitems.lrinvoiceid = lrinvoiceitems.lrinvoiceid 
                    and allinvoiceitems.sno = lrinvoiceitems.sno and allinvoiceitems.isactive=true
           	inner join referencelist on referencelist.id = cast(allinvoiceitems.itemdetail->>'sizeid' as bigint)
            where stock.id in ({0})", string.Join(",", result.ConvertAll(e => e.id)));
            SortedDictionary<long, SortedDictionary<string, int>> keyValuePairs = new SortedDictionary<long, SortedDictionary<string, int>>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var size = reader.GetString(1);
                    var id = reader.GetInt64(0);
                    var index = reader.GetInt32(2);
                    if (!keyValuePairs.ContainsKey(id))
                        keyValuePairs.Add(id, new SortedDictionary<string, int>());
                    if (!keyValuePairs[id].ContainsKey(size))
                        keyValuePairs[id].Add(size, index);
                }
            }
            foreach (var item in keyValuePairs)
            {
                var sizegroup = "";
                var allkeys = item.Value.ToList().ConvertAll(x => x.Key);
                if (allkeys.Count > 1)
                {
                    int ivalue;
                    if (allkeys.All(x =>
                    {
                        return int.TryParse(x, out ivalue);
                    }))
                    {
                        var allsize = allkeys.ConvertAll(x =>
                         {
                             int.TryParse(x, out ivalue);
                             return ivalue;
                         }).OrderBy(x => x);
                        sizegroup = allsize.First() + "-" + allsize.Last();
                    }
                    else
                    {
                        var allsize = item.Value.ToList().OrderBy(x => x.Value);
                        var sizelist = WebAPI.Data.Utility.GetSizes();
                        allkeys.Sort(delegate (string a, string b)
                        {
                            var aindex = sizelist.IndexOf(a);
                            var bindex = sizelist.IndexOf(b);
                            return aindex.CompareTo(bindex);
                        });
                        sizegroup = allsize.First().Key + "-" + allsize.Last().Key;
                    }
                }
                else
                {
                    sizegroup = allkeys.First();
                }
                result.FindAll(x => x.id == item.Key).ForEach(x => x.sizegroup = sizegroup);
            }
        }
        internal StockReport UpdateBitStock(StockReport _request)
        {
            if (string.IsNullOrEmpty(_request.barcode)) return _request;

            string SQL_update = "update stock set isbit=@isbit,modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1 where barcode=@barcode";
            NpgsqlTransaction transaction = null;

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_update, conn, transaction);
                    command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = _request.isbit;
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _request.barcode;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

                    int successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _request);
            }

            return _request;
        }
        internal StockSync ChangeStock(StockSync item)
        {
            var missingitems = item.items.Where(e => e.salablegoodsid == 0 && e.stockid == 0 && item.detail.price > 0).ToList();
            item.items.RemoveAll(e => e.salablegoodsid == 0 && e.stock == 0 && item.detail.price > 0);
            ItemsDataService itemdataservice = new ItemsDataService() { Context = this.Context };


            if (missingitems.Count > 0)
            {
                List<MissingStock> missingrecords = new List<MissingStock>();
                foreach (var mitem in missingitems)
                {
                    MissingStock ms = new MissingStock();
                    ms.price = item.detail.price;
                    ms.dealerprice = item.detail.dealerprice;
                    ms.discount = item.detail.discount;
                    ms.hsncode = item.detail.hsncode;
                    ms.designid = item.detail.designid;
                    ms.iswarehouse = item.mode == 1;
                    ms.locationid = item.locationid;
                    ms.productid = item.detail.productid;
                    ms.sellingname = item.detail.sellingname;
                    ms.barcode = mitem.barcode;
                    ms.qty = mitem.stock;
                    var availableitem = itemdataservice.Search(new ItemsCreteria()
                    {
                        productid = item.detail.productid,
                        sizeid = item.detail.sizeid,
                        patternid = item.detail.patternid,
                        materialid = item.detail.materialid,
                        typeid = item.detail.typeid,
                        colourid = item.detail.colourid,
                        brandid = item.detail.brandid,
                        styleid = item.detail.styleid,
                        sleeveid = item.detail.sleeveid,
                        fitid = item.detail.fitid,
                        agmtcode = item.detail.agmtcode,
                        productmap = true
                    });
                    if (availableitem == null || availableitem.Count == 0)
                    {
                        var items = new Items()
                        {
                            productid = item.detail.productid,
                            sizeid = item.detail.sizeid,
                            patternid = item.detail.patternid,
                            materialid = item.detail.materialid,
                            typeid = item.detail.typeid,
                            colourid = item.detail.colourid,
                            brandid = item.detail.brandid,
                            styleid = item.detail.styleid,
                            sleeveid = item.detail.sleeveid,
                            fitid = item.detail.fitid,
                            printingname = string.IsNullOrEmpty(item.detail.printingname) ? item.detail.productname : item.detail.printingname,
                            sellingname = item.detail.sellingname,
                            agmtcode = item.detail.agmtcode
                        };
                        itemdataservice.Insert(items);
                        ms.itemid = items.id;
                    }
                    else
                    {
                        ms.itemid = availableitem[0].id;
                    }
                    missingrecords.Add(ms);
                    item.items.RemoveAll(e => e.barcode == ms.barcode);
                }
                SaveMissingStock(missingrecords);
            }

            List<string> barcodes = item.items.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.barcode) + "'");
            if (barcodes.Count > 0)
            {
                var stocklist = Search(new StockCreteria()
                {
                    barcodes = barcodes
                });
                var itemlist = itemdataservice.Search(new ItemsCreteria()
                {

                }, stocklist.ConvertAll(e => e.itemid));

                foreach (var itm in item.items)
                {
                    if (stocklist.Exists(e => e.barcode == itm.barcode))
                    {
                        var stockitem = stocklist.Find(e => e.barcode == itm.barcode);
                        var itemid = stockitem.itemid;
                        var itemscreteria = new ItemsCreteria();
                        Items items;
                        if (itemlist.Exists(e => e.id == itemid))
                        {
                            items = itemlist.Find(e => e.id == itemid);
                            itemscreteria = new ItemsCreteria()
                            {
                                productid = items.productid,
                                sizeid = items.sizeid,
                                patternid = items.patternid,
                                materialid = items.materialid,
                                typeid = items.typeid,
                                colourid = items.colourid,
                                brandid = items.brandid,
                                styleid = items.styleid,
                                sleeveid = items.sleeveid,
                                fitid = items.fitid
                            };
                        }
                        else
                        {
                            itemscreteria = new ItemsCreteria()
                            {
                                productid = stockitem.productid,
                                productmap = true
                            };
                        }
                        if (item.detail.productid > 0)
                            itemscreteria.productid = item.detail.productid;
                        if (item.detail.styleid > 0)
                            itemscreteria.styleid = item.detail.styleid;
                        if (item.detail.sizeid > 0)
                            itemscreteria.sizeid = item.detail.sizeid;
                        if (item.detail.patternid > 0)
                            itemscreteria.patternid = item.detail.patternid;
                        if (item.detail.colourid > 0)
                            itemscreteria.colourid = item.detail.colourid;
                        if (item.detail.typeid > 0)
                            itemscreteria.typeid = item.detail.typeid;
                        if (item.detail.materialid > 0)
                            itemscreteria.materialid = item.detail.materialid;
                        if (item.detail.brandid > 0)
                            itemscreteria.brandid = item.detail.brandid;
                        if (item.detail.sleeveid > 0)
                            itemscreteria.sleeveid = item.detail.sleeveid;
                        if (item.detail.fitid > 0)
                            itemscreteria.fitid = item.detail.fitid;

                        var availableitem = itemdataservice.Search(itemscreteria);
                        itemid = 0;
                        if (availableitem == null || availableitem.Count == 0)
                        {
                            items = new Items()
                            {
                                productid = itemscreteria.productid,
                                sizeid = itemscreteria.sizeid,
                                patternid = itemscreteria.patternid,
                                materialid = itemscreteria.materialid,
                                typeid = itemscreteria.typeid,
                                colourid = itemscreteria.colourid,
                                brandid = itemscreteria.brandid,
                                styleid = itemscreteria.styleid,
                                sleeveid = itemscreteria.sleeveid,
                                fitid = itemscreteria.fitid,
                                printingname = itemscreteria.printingname,
                                sellingname = itemscreteria.sellingname,
                            };
                            itemdataservice.Insert(items);
                        }
                        else
                        {
                            items = availableitem[0];
                        }
                        itemid = items.id;

                        // Stock History
                        StockhistoryDataService stockhistorydata = new StockhistoryDataService() { Context = this.Context };
                        Stockhistory stockhistory = stockitem.GetStockHistory();
                        stockhistory.actiontype = Stockhistory.ActionType.AttributeChange;
                        stockhistory.reason = item.reason;
                        stockhistorydata.Insert(stockhistory);

                        if (stockitem.itemid == itemid)
                        {
                            if (item.detail.productid > 0 && item.detail.productid != stockitem.productid)
                            {
                                stockitem.productid = item.detail.productid;
                                stockitem.printingname = items.printingname;
                            }
                            if (!string.IsNullOrEmpty(item.detail.hsncode))
                                stockitem.hsncode = item.detail.hsncode;
                            if (!string.IsNullOrEmpty(item.detail.designid))
                                stockitem.designid = item.detail.designid;
                            if (item.detail.pricetagid > 0)
                                stockitem.pricetagid = item.detail.pricetagid;
                            Update(stockitem);
                            stockitem.barcode = "";
                        }
                        else
                        {
                            if (item.detail.productid > 0)
                            {
                                stockitem.productid = item.detail.productid;
                                stockitem.printingname = items.printingname;
                            }
                            stockitem.itemid = itemid;
                            if (!string.IsNullOrEmpty(item.detail.hsncode))
                                stockitem.hsncode = item.detail.hsncode;
                            if (!string.IsNullOrEmpty(item.detail.designid))
                                stockitem.designid = item.detail.designid;
                            if (item.detail.pricetagid > 0)
                                stockitem.pricetagid = item.detail.pricetagid;
                            Update(stockitem);
                            stockitem.barcode = "";
                        }
                    }
                }
            }

            item.items.RemoveAll(e => e.barcode == "");
            return item;
        }

        internal List<string> GetAllBarcodes(long id)
        {
            List<string> result = new List<string>();
            var param = new SortedList<string, object>();
            param.Add("invoiceitemid", id);
            string query = "select barcode from stock where invoiceitemid=@invoiceitemid and isactive=true";
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                query = "select serialno from stockuan where invoiceitemid=@invoiceitemid and isactive=true";
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            }
            return result.Distinct().ToList();
        }

        internal MigrateStock MigrateStock(MigrateStock value)
        {
            throw new NotImplementedException();
        }

        internal List<BundleItemTransferHistory> TransferHistory(ItemSearchCreteria item)
        {
            if (string.IsNullOrEmpty(item.barcode)) return new List<BundleItemTransferHistory>();

            BundlesDataService bundlesDataService = new BundlesDataService(false);
            return bundlesDataService.SearchHistory(item.barcode, null, 0);
        }
        internal ItemSearchResult ItemHistory(ItemSearchCreteria item)
        {
            if (string.IsNullOrEmpty(item.barcode)) throw new Exception("Please enter the barcode");

            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodeparser))
            {
                item.barcode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.POSParser.UpdateBarcode(item.barcode);
            }

            if ((InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ||
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
            {
                //item.barcode = SellablebarcodesDataService.Instance.GetCoreBarcode(item.barcode);
                SalablegoodsDataService salablegoodsdata = new SalablegoodsDataService() { Context = this.Context };
                item.barcodes = salablegoodsdata.GetQuantoBarcode(item.barcode);
                if (item.barcodes.Count > 1)
                    item.barcode = "";
                else if (item.barcodes.Count == 1)
                {
                    item.barcode = item.barcodes[0];
                    item.barcodes = null;
                }
            }

            ItemSearchResult result = new ItemSearchResult();
            if (item.mode == 1)
            {
                BillitemsDataService billdata = new BillitemsDataService() { Context = this.Context };
                billdata.Context = this.Context;
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_isb2bapplication)
                    result.b2bbillitems = billdata.SearchB2BAll(item);
                else
                    result.billitems = billdata.SearchAll(item);
                return result;
            }
            if (item.mode == 4)
            {
                BulkBillItemsDataService billdata = new BulkBillItemsDataService() { Context = this.Context };
                billdata.Context = this.Context;
                result.billitems = billdata.SearchAll(item);
                return result;
            }
            if (string.IsNullOrEmpty(item.barcode) && (item.barcodes == null || item.barcodes.Count == 0)) return result;

            var stock = Search(item.companyid, string.IsNullOrEmpty(item.barcode) ? item.barcodes.First() : item.barcode);
            if (stock == null || stock.id == 0)
                return result;

            result.stock = stock;

            if (stock.salestaxid > 0)
            {
                TaxDataService taxdata = new TaxDataService() { Context = this.Context };
                var taxlist = taxdata.Search(new TaxCreteria() { id = stock.salestaxid });
                decimal taxpercentage = 0, taxamount = 0;
                if (taxlist != null && taxlist.Count > 0)
                {
                    if (taxlist[0].type == 1)
                        taxpercentage = taxlist[0].taxpercentage;
                    else if (taxlist[0].type == 2 && taxlist[0].taxsplit != null && taxlist[0].taxsplit.items.Count > 1)
                    {
                        if ((stock.price - stock.discount) > taxlist[0].taxsplit.items[0].fromamount
                            && (stock.price - stock.discount) <= taxlist[0].taxsplit.items[0].toamount)
                            taxpercentage = (taxlist[0].taxsplit.items[0].cgst * 2);
                        else
                            taxpercentage = (taxlist[0].taxsplit.items[1].cgst * 2);
                    }
                    else if (taxlist[0].type == 2 && taxlist[0].taxsplit != null && taxlist[0].taxsplit.items.Count == 1)
                    {
                        taxpercentage = (taxlist[0].taxsplit.items[0].cgst * 2);
                    }
                }
                if (taxpercentage > 0)
                    taxamount = ((stock.price - stock.discount) / (100 + taxpercentage) * taxpercentage);
                if (stock.buyingprice > 0)
                    stock.netmarginpercent = Math.Round(((stock.price - stock.discount - stock.buyingprice - taxamount) / stock.buyingprice) * 100, 2);
                stock.salestaxamount = taxamount;
                stock.salestaxpercent = taxpercentage;
            }
            if (stock.invoiceitemid > 0)
            {
                LrinvoiceitemsDataService lrinvoiceitemdata = new LrinvoiceitemsDataService() { Context = this.Context };
                lrinvoiceitemdata.Context = this.Context;
                result.invoiceitems = lrinvoiceitemdata.Search(new LrinvoiceitemsCreteria()
                {
                    id = stock.invoiceitemid
                });

                if (result.invoiceitems.Count > 0)
                {
                    LrinvoiceDataService lrinvoicedata = new LrinvoiceDataService() { Context = this.Context };
                    result.lrinvoice = lrinvoicedata.SearchDetail(new LrinvoiceCreteria()
                    {
                        id = result.invoiceitems.First().lrinvoiceid
                    }, false);

                    if (result.lrinvoice != null && result.lrinvoice.Items != null &&
                        result.lrinvoice.Items.Count > 0)
                    {
                        if (result.lrinvoice.Items[0].entrydate == DateTime.MinValue && result.lrinvoice.Items[0].invoicedate == DateTime.MinValue)
                        {
                            result.lrinvoice.Items[0].invoicedate = result.stock.stockdate;
                            result.lrinvoice.Items[0].entrydate = result.stock.stockdate;
                        }
                        result.lrinvoice.lrentrydate = result.lrinvoice.lrentrydate;
                        result.lrinvoice.invoiceno = result.lrinvoice.Items[0].invoiceno;
                        result.lrinvoice.invoicedate = result.lrinvoice.Items[0].invoicedate;
                        result.lrinvoice.invoiceid = result.lrinvoice.Items[0].id;
                        result.lrinvoice.entrydate = result.lrinvoice.Items[0].entrydate;
                        result.lrinvoice.stockdays = (Int32)DateTimeNow.Date.Subtract(result.lrinvoice.entrydate).TotalDays;
                        result.lrinvoice.stockbatch = result.lrinvoice.stockdays <= 0 ? 1 : result.lrinvoice.stockdays / (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config != null && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_stockbatchdays > 0 ? InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_stockbatchdays : 90) + 1;
                    }

                    LrinvoicereturnitemsDataService lrinvoicereturnitemsDataService = new LrinvoicereturnitemsDataService() { Context = this.Context };
                    result.returnhistory = lrinvoicereturnitemsDataService.SearchAllByReturnType(result.stock.invoiceitemid, 5);

                    if (result.invoiceitems[0].jobworkgrnitemid > 0)
                    {
                        JobworkbillgrnitemsDataService jobworkbillgrnitemsDataService = new JobworkbillgrnitemsDataService() { Context = this.Context };
                        result.jobWorkdetails = jobworkbillgrnitemsDataService.SearchInfo(result.invoiceitems[0].jobworkgrnitemid, result.invoiceitems[0].buyingitemid);
                    }
                    else
                    {
                        JobworkbillgrnitemsDataService jobworkbillgrnitemsDataService = new JobworkbillgrnitemsDataService() { Context = this.Context };
                        result.jobWorkdetails = jobworkbillgrnitemsDataService.SearchInfo(item.barcode, result.invoiceitems[0].id);
                    }
                }
            }
            SalablegoodsDataService saledatasource = new SalablegoodsDataService() { Context = this.Context };
            saledatasource.Context = this.Context;
            result.salablegoods = saledatasource.Search(new SalablegoodsCreteria()
            {
                barcode = item.barcode,
                barcodes = item.barcodes,
                companyid = item.companyid
            });

            if (item.includeTranHistory)
            {
                BundlesDataService bundlesDataService = new BundlesDataService(false);
                result.transferHistories = bundlesDataService.SearchHistory(item.barcode, item.barcodes, item.companyid);
            }

            RetailReportDataService RetailReportData = new RetailReportDataService() { Context = this.Context };
            result.audithistory = RetailReportData.SearchsalablegoodsAudit(new StockReportCreteria()
            {
                barcode = item.barcode,
                barcodes = item.barcodes,
                companyid = item.companyid,
                actiontype = 1
            });
            //result.audithistory.RemoveAll(e => e.whenqty == e.auditqty);

            var stockhistory = RetailReportData.SearchStockAudit(new StockReportCreteria()
            {
                barcode = item.barcode,
                barcodes = item.barcodes,
                companyid = item.companyid,
                actiontype = 1
            });
            //stockhistory.RemoveAll(e => e.whenqty == e.auditqty);
            result.audithistory.AddRange(stockhistory);

            result.pricehistory = RetailReportData.PriceChanger(new RetailReportCreteria()
            {
                barcode = item.barcode,
                barcodes = item.barcodes,
                companyid = item.companyid
            });
            result.pricehistory.AddRange(RetailReportData.PriceChanger(new RetailReportCreteria()
            {
                barcode = item.barcode,
                barcodes = item.barcodes,
                locationid = -9
            }));

            if (Context.HasPermission(PermissionSet.showvalue, InfyPOS.Common.Action.View))
            {
                result.stock.buyingprice = 0;
                result.stock.ibuyingprice = 0;
            }

            if (item.barcodes != null && item.barcodes.Count > 0)
            {
                result.stock.barcode = string.Join(",", item.barcodes);
                List<StockWithTax> stocklist = new List<StockWithTax>();
                foreach (var barcode in item.barcodes.Distinct())
                {
                    var stockitem = Search(item.companyid, barcode);
                    if (!stocklist.Exists(e => e.id == stockitem.id))
                        stocklist.Add(stockitem);
                }
                result.stock.qty = stocklist.Sum(e => e.qty);
                result.stock.transfered = stocklist.Sum(e => e.transfered);
                result.stock.returnqty = stocklist.Sum(e => e.returnqty);
                result.stock.damagedqty = stocklist.Sum(e => e.damagedqty);
                result.stock.journalqty = stocklist.Sum(e => e.journalqty);
            }

            if (result.stock != null && result.stock.srcsupplierid > 0 &&
                result.lrinvoice != null)
            {
                SupplierDataService suppdata = new SupplierDataService() { Context = this.Context };
                var supplierlist = suppdata.Search(new SupplierCreteria()
                {
                    id = result.stock.srcsupplierid
                });
                if (supplierlist.Count > 0)
                {
                    result.lrinvoice.sourcesupplierid = supplierlist[0].id;
                    result.lrinvoice.sourcesuppliername = supplierlist[0].name;
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5)
            {
                List<Salablegoods> salablegoods = new List<Salablegoods>();
                foreach (var retsal in result.salablegoods.GroupBy(x => x.locationid))
                {
                    var v = retsal.First();
                    v.barcode = result.stock.serialno;
                    v.iqty = retsal.Sum(x => x.iqty);
                    v.oqty = retsal.Sum(x => x.oqty);
                    v.dqty = retsal.Sum(x => x.dqty);
                    v.jqty = retsal.Sum(x => x.jqty);
                    v.aqty = retsal.Sum(x => x.aqty);
                    v.pqty = retsal.Sum(x => x.pqty);
                    v.rqty = retsal.Sum(x => x.rqty);
                    v.tiqty = retsal.Sum(x => x.tiqty);
                    v.toqty = retsal.Sum(x => x.toqty);
                    salablegoods.Add(retsal.First());
                }
                result.salablegoods = salablegoods;
            }
            return result;
        }


        internal MissingBarcode CreateBarcode(MissingBarcode item)
        {
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany && item.companyid <= 0)
                throw new Exception("Please select the company");

            ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
            var products = productdataservice.Search(new ProductsCreteria()
            {
                id = item.productid
            });
            var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, item.companyid);
            NpgsqlTransaction transaction = null;
            List<Stock> updatableStock = new List<Stock>();
            try
            {
                int barcodecount = Convert.ToInt32(products[0].sellingmode == 1 && item.qty > 0 ? item.qty : 1);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> barcodes = new List<InfyPOS.Processors.BarcodeManager.NewBarcodeID>();
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationbasedbarcodes && item.locationid > 0)
                        barcodes = CreateBarcode(barcodeConfig.Type, barcodecount, item.companyid, conn, transaction,
                            item.locationid, item, null);
                    else
                        barcodes = CreateBarcode(barcodeConfig.Type, barcodecount, item.companyid, conn, transaction, item);

                    if (barcodes.Count >= 1)
                    {
                        transaction.Commit();
                        item.barcode = barcodes[0].code;
                        item.barcodeid = barcodes[0].id;

                        if (barcodes.Count > 1)
                        {
                            item.barcodes = string.Join(",", barcodes.ConvertAll(e => e.code));
                            item.barcodeids = string.Join(",", barcodes.ConvertAll(e => e.id));
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }

        public List<BarcodeManager.NewBarcodeID> CreateBarcode(BarcodeManager.BarcodeType barcodeType, int noofcodes,
            long companyid, NpgsqlConnection conn, NpgsqlTransaction transaction, MissingBarcode item)
        {
            return CreateBarcode(barcodeType, noofcodes, companyid, conn, transaction, 0, item, null);
        }

        public List<BarcodeManager.NewBarcodeID> CreateBarcode(BarcodeManager.BarcodeType barcodeType, int noofcodes,
    long companyid, NpgsqlConnection conn, NpgsqlTransaction transaction, MissingStock item)
        {
            return CreateBarcode(barcodeType, noofcodes, companyid, conn, transaction, 0, null, item);
        }

        //private List<BarcodeManager.NewBarcodeID> CreateBarcode(BarcodeManager.BarcodeType barcodeType, int noofcodes,
        //    long companyid, NpgsqlConnection conn, NpgsqlTransaction transaction, long locationid, MissingStock item)
        //{
        //    return CreateBarcode(barcodeType, noofcodes, companyid, conn, transaction, locationid, null, item);
        //}


        //private List<BarcodeManager.NewBarcodeID> CreateBarcode(Processors.BarcodeManager.BarcodeType barcodeType, 
        //    int noofcodes, long companyid, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction, long locationid)
        //{
        //    return CreateBarcode(barcodeType, noofcodes, companyid, conn, transaction, locationid, null, null);
        //}

        const string SQL_SearchAll = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, printingname, price, barcode, companyid, invoiceitemid, itemid, productid,discount,dealerprice,
                                        additionaldiscount,qty,transfered,journalqty,damagedqty,buyingprice,ibuyingprice,lrreference, designid,hsncode,isbit,locationid,retaillocationid,displaymrp from stock where isactive=true and invoiceitemid=@invoiceitemid";

        public List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> CreateBarcode(Processors.BarcodeManager.BarcodeType barcodeType, int noofcodes, long companyid,
            Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction)
        {
            return CreateBarcode(barcodeType, noofcodes, companyid, conn, transaction, 0, null, null);
        }


        public List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> CreateBarcode(Processors.BarcodeManager.BarcodeType barcodeType, int noofcodes, long companyid,
            Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction,
            long locationid, MissingBarcode barcodeitem, MissingStock stockitem)
        {
            var newbarcode = 0;
            List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> retbarcodelist = new List<InfyPOS.Processors.BarcodeManager.NewBarcodeID>();
            List<string> barcodelist = new List<string>();
            List<string> duplicateList = new List<string>();
            var barcodeconfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(barcodeType, companyid);
            var sequence = (locationid == 0) ? barcodeconfig.GetSequenceNo() : barcodeconfig.GetSequenceNo(locationid);

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_monthlybarcodes)
            {
                if (sequence.modifiedon.ToString("MMyyyy") != DateTime.Now.ToString("MMyyyy"))
                {
                    sequence.ResetNumber(conn, transaction, DateTimeNow);
                }
            }
            int nooftries = 50;
            while (--nooftries > 0)
            {
                var nextNumber = sequence.GetNextNumber(this.Context, conn, transaction);
                var initialSlot = sequence.CurrentSlotID;
                long lastSlot = initialSlot + (noofcodes - 1);
                int tryCount = 3;
                if (barcodeType == BarcodeManager.BarcodeType.PACKSERIAL)
                {
                    lastSlot = initialSlot;
                }
                while (!sequence.SentNextNumber(initialSlot, locationid, lastSlot, conn, transaction) &&
                    tryCount > 0)
                {
                    try
                    {
                        nextNumber = sequence.GetNextNumber(this.Context, conn, transaction);
                        initialSlot = sequence.CurrentSlotID;
                        lastSlot = initialSlot + 1;
                        tryCount--;
                    }
                    catch (Exception exp)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "CreateBarcode");
                        return null;
                    }
                }
                barcodelist.Clear();
                retbarcodelist.Clear();

                if (barcodeType == BarcodeManager.BarcodeType.PACKSERIAL)
                {
                    barcodeconfig.SetValue((int)initialSlot);
                    for (int i = 1; i <= noofcodes; i++)
                    {
                        var barcode = string.Format("{0}/{1}", initialSlot, i);
                        barcodelist.Add("'" + barcode + "'");
                        retbarcodelist.Add(new InfyPOS.Processors.BarcodeManager.NewBarcodeID()
                        {
                            code = barcode,
                            id = newbarcode
                        });
                    }
                }
                else
                {
                    var tInitialSlot = initialSlot;
                    for (int i = 0; i < noofcodes; i++)
                    {
                        newbarcode = (int)tInitialSlot++;
                        barcodeconfig.SetValue(newbarcode);
                        barcodelist.Add("'" + barcodeconfig.GetSpecialValue(Context, sequence, newbarcode, barcodeitem, stockitem) + "'");
                        retbarcodelist.Add(new InfyPOS.Processors.BarcodeManager.NewBarcodeID()
                        {
                            code = barcodeconfig.GetSpecialValue(Context, sequence, newbarcode, barcodeitem, stockitem),
                            id = newbarcode
                        });
                    }
                }
                var barcodecommand = GetCommand(string.Format(SQL_ValidateBarcode, string.Join(",", barcodelist)), conn, transaction);
                duplicateList.Clear();
                using (var reader = barcodecommand.ExecuteReader())
                {
                    while (reader.Read())
                        duplicateList.Add(reader.GetString(0));

                    if (reader.NextResult())
                        while (reader.Read())
                            duplicateList.Add(reader.GetString(0));
                }
                if (duplicateList.Count == 0)
                {
                    nooftries = 50;
                    break;
                }
                else
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Duplicate barcodes"), WebAPI.Common.Logger.LogType.Info, duplicateList);
                }
            }

            if (nooftries <= 0)
            {
                throw new Exception("Please reset the current auto number for " + sequence.autonumbertype + ",Prefix " + sequence.prefix +
                    ", Current " + sequence.CurrentNo + " , Possible barcodes " + string.Join(",", retbarcodelist));
            }
            return retbarcodelist;
        }

        internal StockItem FindStock(string barcode)
        {
            string query = @"select qty, damagedqty, returnqty, transfered,journalqty from stock where isactive=true and barcode = @barcode;
                             select sum(iqty),sum(oqty),sum(dqty),sum(jqty),sum(aqty) from Salablegoods where isactive=true and barcode = @barcode";
            StockItem ostock = new StockItem();
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(query, conn);
                    command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = barcode;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ostock.qty = reader.GetDecimal(0);
                            ostock.damagedqty = reader.GetDecimal(1);
                            ostock.returnqty = reader.GetDecimal(2);
                            ostock.transfered = reader.GetDecimal(3);
                            ostock.journalqty = reader.GetDecimal(4);
                        }
                        if (reader.NextResult() && reader.Read())
                        {
                            ostock.iqty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            ostock.oqty = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            ostock.dqty = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            ostock.jqty = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                            ostock.aqty = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                        }
                        ostock.wstock = ostock.qty - (ostock.returnqty + ostock.journalqty + ostock.damagedqty + ostock.transfered);
                        ostock.rstock = ostock.iqty - (ostock.oqty + ostock.dqty + ostock.jqty);
                    }
                    conn.Close();
                }

            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "FindStock - " + barcode);
            }
            return ostock;
        }

        internal List<MissingStock> SaveMissingStock(List<MissingStock> items)
        {
            NpgsqlTransaction transaction = null;
            List<MissingStock> missingStocks = new List<MissingStock>();
            try
            {
                if (items != null && items.Exists(e => e.productid == 0))
                    throw new Exception("Product missing, please created product details with tax information.");

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    foreach (var item in items)
                    {
                        decimal taxpercentage = 0;
                        decimal salestaxpercentage = 0;

                        if (item.productid > 0)
                        {
                            ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
                            TaxDataService taxdata = new TaxDataService() { Context = this.Context };

                            var products = productdataservice.Search(new ProductsCreteria()
                            {
                                id = item.productid,
                                code = item.code
                            });

                            if (products.Count == 0 && !string.IsNullOrEmpty(item.sellingname))
                            {
                                products = productdataservice.Search(new ProductsCreteria()
                                {
                                    name = item.sellingname
                                });
                            }

                            if (products.Count == 0 && !string.IsNullOrEmpty(item.sellingname))
                            {
                                var alltaxs = taxdata.Search(new TaxCreteria());

                                productdataservice.Insert(new Products()
                                {
                                    name = item.sellingname,
                                    isactive = true,
                                    version = 1,
                                    salestaxid = alltaxs.First().id
                                });
                                products = productdataservice.Search(new ProductsCreteria()
                                {
                                    name = item.sellingname
                                });
                            }

                            if (products[0].salestaxid > 0)
                            {
                                var taxlist = taxdata.Search(new TaxCreteria() { id = products[0].salestaxid });
                                if (taxlist != null && taxlist.Count > 0)
                                {
                                    if (taxlist[0].type == 1)
                                    {
                                        taxpercentage = taxlist[0].taxpercentage;
                                        salestaxpercentage = taxpercentage;
                                    }
                                    else if (taxlist[0].type == 2 && taxlist[0].taxsplit != null && taxlist[0].taxsplit.items.Count > 1)
                                    {
                                        if (item.buyingprice > taxlist[0].taxsplit.items[0].fromamount
                                            && item.buyingprice <= taxlist[0].taxsplit.items[0].toamount)
                                        {
                                            taxpercentage = (taxlist[0].taxsplit.items[0].cgst * 2);
                                        }
                                        else
                                        {
                                            taxpercentage = (taxlist[0].taxsplit.items[1].cgst * 2);
                                        }

                                        if (item.price > taxlist[0].taxsplit.items[0].fromamount
                                            && item.price <= taxlist[0].taxsplit.items[0].toamount)
                                        {
                                            salestaxpercentage = taxpercentage;
                                        }
                                        else
                                        {
                                            salestaxpercentage = taxpercentage;
                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(item.barcode))
                            {

                                var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(products[0].barcodemode, item.companyid);
                                List<InfyPOS.Processors.BarcodeManager.NewBarcodeID> newbarcode = CreateBarcode(barcodeConfig.Type, 1, item.companyid, conn, transaction, item);
                                item.newbarcode = true;
                                item.barcode = newbarcode.First().code;
                                item.barcodeid = newbarcode.First().id;
                            }
                        }
                        if (string.IsNullOrEmpty(item.barcode))
                            throw new Exception("Barcode missing");

                        var stocks = Search(new StockCreteria()
                        {
                            barcode = item.barcode
                        });
                        if (stocks.Count > 0)
                        {
                            stocks[0].buyingqty += item.qty;
                            stocks[0].qty += item.qty;
                            if (!item.iswarehouse)
                                stocks[0].transfered += item.qty;
                            Update(transaction, conn, stocks[0]);
                            item.stockid = stocks[0].id;
                        }
                        else
                        {
                            missingStocks.Add(item);

                            if (item.sizeid == 0 && !string.IsNullOrEmpty(item.sizename))
                            {
                                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                                var sizelist = referencelistDataService.Search(new ReferencelistCreteria()
                                {
                                    type = "SIZE",
                                    name = item.sizename
                                });
                                if (sizelist.Exists(e => e.name.ToUpper() == item.sizename.ToUpper()))
                                {
                                    item.sizeid = sizelist.Find(e => e.name.ToUpper() == item.sizename.ToUpper()).id;
                                }
                                else
                                {
                                    item.sizeid = referencelistDataService.Insert(new Referencelist()
                                    {
                                        code = item.sizename.ToUpper(),
                                        name = item.sizename.ToUpper(),
                                        type = "SIZE",
                                        isactive = true
                                    }).id;
                                }
                            }

                            if (item.productid == 0 && !string.IsNullOrEmpty(item.productname))
                            {
                                ProductsDataService referencelistDataService = new ProductsDataService() { Context = this.Context };
                                var sizelist = referencelistDataService.Search(new ProductsCreteria()
                                {
                                    name = item.productname
                                });
                                if (sizelist.Exists(e => e.name.ToUpper() == item.productname.ToUpper()))
                                {
                                    item.productid = sizelist.Find(e => e.name.ToUpper() == item.productname.ToUpper()).id;
                                }
                                else
                                {
                                    item.productid = referencelistDataService.Insert(new Products()
                                    {
                                        code = item.productname.ToUpper(),
                                        name = item.productname.ToUpper(),
                                        isactive = true
                                    }).id;
                                }
                            }

                            if (item.brandid == 0 && !string.IsNullOrEmpty(item.brandname))
                            {
                                BrandDataService referencelistDataService = new BrandDataService() { Context = this.Context };
                                var sizelist = referencelistDataService.Search(new BrandCreteria()
                                {
                                    name = item.brandname
                                });
                                if (sizelist.Exists(e => e.name.ToUpper() == item.brandname.ToUpper()))
                                {
                                    item.brandid = sizelist.Find(e => e.name.ToUpper() == item.brandname.ToUpper()).id;
                                }
                                else
                                {
                                    item.brandid = referencelistDataService.Insert(new Brand()
                                    {
                                        code = item.brandname.ToUpper(),
                                        name = item.brandname.ToUpper(),
                                        isactive = true
                                    }).id;
                                }
                            }

                            ItemsDataService itemdata = new ItemsDataService() { Context = this.Context };
                            var nitem = new ItemsCreteria()
                            {
                                productid = item.productid,
                                sizeid = item.sizeid,
                                brandid = item.brandid,
                                brandname = item.brandname,
                                productname = item.productname,
                                sellingname = item.productname,
                                sizename = item.sizename,
                                colourid = item.colourid,
                                styleid = item.styleid,
                                agmtcode = item.itemcode,
                                productmap = true
                            };
                            var existingitems = itemdata.Search(nitem);
                            if (existingitems.Count == 0)
                                itemdata.Insert(nitem);
                            else
                                nitem.id = existingitems[0].id;
                            item.itemid = nitem.id;

                            Stock tstock = new Stock();
                            tstock.invoiceitemid = -1;
                            tstock.hsncode = item.hsncode;
                            tstock.isactive = true;
                            tstock.designid = item.designid;
                            tstock.barcode = item.barcode;
                            tstock.serialno = item.serialno;
                            tstock.hsncode = item.hsncode;
                            tstock.discount = item.discount;
                            tstock.designid = item.designid;
                            tstock.dealerprice = item.dealerprice;
                            tstock.itemid = item.itemid;
                            tstock.isbit = item.isbit;
                            tstock.companyid = item.companyid;
                            tstock.locationid = item.locationid;
                            tstock.retaillocationid = item.locationid;
                            tstock.displaymrp = item.displaymrp;
                            tstock.displayinvoiceno = item.displayinvoiceno;
                            tstock.displayinvoicedate = item.displayinvoicedate;
                            tstock.stockdate = item.displayinvoicedate;
                            tstock.lrreference = "...";
                            tstock.price = item.price;
                            tstock.buyingprice = item.buyingprice;
                            tstock.eprice = (tstock.price - tstock.discount) / (salestaxpercentage + 100) * 100;
                            tstock.version = item.version;
                            if (taxpercentage > 0)
                                tstock.ibuyingprice = tstock.buyingprice + ((tstock.buyingprice * taxpercentage) / 100);
                            //tstock.ibuyingprice = tstock.buyingprice / (100  + taxpercentage) * 100; 
                            else
                                tstock.ibuyingprice = item.buyingprice;
                            tstock.productid = item.productid;
                            tstock.supplierid = item.supplierid;
                            tstock.printingname = item.sellingname;
                            tstock.stockdate = DateTimeNow.Date;
                            tstock.buyingqty = item.qty;
                            tstock.qty = item.qty;
                            if (!item.iswarehouse)
                            {
                                // commented because the transfered qty getting updated when stock moved to salablegoods.
                                //tstock.transfered = item.qty;
                            }
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_autouangln)
                            {
                                var list = new List<Stock>();
                                list.Add(tstock);
                                CreateUAN(list, conn, transaction);
                            }
                            stocks.Add(tstock);
                            Insert(transaction, conn, tstock);
                            item.stockid = tstock.id;
                        }

                        if (!item.iswarehouse && item.locationid > 0)
                        {
                            SalablegoodsDataService saldata = new SalablegoodsDataService() { Context = this.Context };
                            saldata.Context = this.Context;
                            var salableitems = saldata.Search(new SalablegoodsCreteria()
                            {
                                locationid = item.locationid,
                                barcode = item.barcode
                            });

                            if (salableitems.Count > 0)
                            {
                                if (item.UpdateAll || salableitems[0].stockid == 0)
                                {
                                    salableitems[0].stockid = item.stockid;
                                    salableitems[0].iqty += item.qty;
                                    salableitems[0].price = item.price;
                                    salableitems[0].discount = item.discount;
                                    if (item.directstockupload)
                                        saldata.isMigration = true;

                                    saldata.Update(transaction, conn, salableitems[0], salableitems[0]);
                                    item.salablegoodsid = salableitems[0].id;
                                }
                                else if (salableitems[0].id > 0)
                                {
                                    salableitems[0].stockid = item.stockid;
                                    salableitems[0].iqty += item.qty;
                                    if (item.directstockupload)
                                        saldata.isMigration = true;

                                    saldata.Update(transaction, conn, salableitems[0], salableitems[0]);
                                    item.salablegoodsid = salableitems[0].id;
                                }
                            }
                            else
                            {
                                Salablegoods salitem = new Salablegoods();
                                salitem.iqty = item.qty;
                                salitem.itemid = item.itemid;
                                salitem.barcode = item.barcode;
                                salitem.discount = item.discount;
                                salitem.inwardon = DateTimeNow.Date;
                                salitem.isactive = true;
                                salitem.locationid = item.locationid;
                                salitem.lrreference = "...";
                                salitem.price = item.price;
                                salitem.received = true;
                                salitem.sellingname = item.sellingname;
                                salitem.companyid = item.companyid;
                                salitem.itemid = item.itemid;
                                salitem.stockid = item.stockid;
                                saldata.Insert(transaction, conn, salitem);
                                item.salablegoodsid = salitem.id;
                            }
                        }
                    }
                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.SaveMissingStock(items, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return missingStocks;
        }

        const string SQL_SearchStock = @"select stock.companyid,stock.productid,stock.itemid,stock.price,stock.qty,stock.barcode,price, buyingprice, qty,supplierid, stock.srcsupplierid, items.brandid,items.patternid, 
                                    items.styleid,items.stylename,items.brandname,items.productname,items.sizename,items.materialname,items.typename,company.name,supplier.name,stock.hsncode,isbit,runtimeprice
                                    from stock 
                                    left outer join items on items.id = stock.itemid
                                    left outer join company on stock.companyid = company.id
                                    inner join supplier on stock.supplierid = supplier.id";

        const string SQL_SearchRetailStock = @"select salablegoods.companyid, salablegoods.supplierid,salablegoods.itemid,salablegoods.locationid,salablegoods.iqty,salablegoods.oqty,salablegoods.jqty,salablegoods.dqty,salablegoods.barcode,
                                    items.brandid,items.patternid, 
                                    items.styleid,items.stylename,items.brandname,items.productname,items.sizeid,items.materialid,items.materialname,items.typeid,company.name,supplier.name
                                    from salablegoods 
                                    left outer join items on items.id = salablegoods.itemid
                                    inner join company on salablegoods.companyid = company.id
                                    inner join supplier on salablegoods.supplierid = supplier.id";


        const string SQL_SearchByBarcode = @"
                                    select lrinvoice.invoicedate,stock.id, stock.isactive, stock.createdby, stock.modifiedby, stock.itemid,stock.productid,stock.returnqty, sno, isbit,
                                    stock.version, stock.createdon, stock.modifiedon,stock.lrreference, stock.designid,products.name printingname, price, lrinvoiceitems.buyingqty,lrinvoiceid,
				                    barcode, stock.serialno,invoiceitemid,stock.companyid,isuan,ismultiprice,
                                    COALESCE(stock.buyingprice,lrinvoiceitems.buyingprice) as buyingprice,
                                    lrinvoiceitems.purchasediscount,
                                    lrinvoiceitems.purchasediscountpercentage,
                                    tax.id as taxid,tax.name as taxname,purchasetaxpercentage taxpercentage,stock.discount,stock.additionaldiscount,stock.qty,stock.transfered,lrinvoiceitems.purchasetax,
                                    stock.damagedqty,stock.journalqty,suppliercompany.supplierid,stock.supplierid stocksupplierid, COALESCE(stock.hsncode,products.hsncode) as hsncode,items.printingname as itemname,items.brandid,
                                    style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,sleeve.name sleevename,fit.name fitname,
                                    pricetag.name pricetagname, lrinvoice.invoicedate, products.sellingmode,stock.ibuyingprice,lrinvoice.invoiceno,stock.dealerprice wsrate,lrinvoiceitems.sno,
                                    cast(lrinvoiceitems.itemDetail->>'minrate' as numeric) minrate,
                                    cast(lrinvoiceitems.itemDetail->>'dealerprice' as numeric) dealerprice,
                                    case when coalesce(lrinvoiceitems.mrp,stock.displaymrp) = 0 then stock.displaymrp else coalesce(lrinvoiceitems.mrp,stock.displaymrp) end mrp,
                                    runtimeprice, lrinvoiceitems.basebuyingprice, stock.srcsupplierid,
                                    products.salestaxid, stock.stockdate, stock.imageid, company.name companyname
                                    from stock 
                                    left outer join products on products.id = stock.productid
				                    left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid  and lrinvoiceitems.isactive = true
				                    left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
                                    left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                    left outer join company on company.id = lrinvoice.companyid
				                    left outer join items on stock.itemid=items.id and lrinvoiceitems.isactive=true
                                    left outer join tax on tax.id = lrinvoiceitems.taxid
                                    left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist type on type.id = typeid
                                    left outer join referencelist sleeve on sleeve.id = sleeveid
                                    left outer join referencelist fit on fit.id = fitid
                                    left outer join referencelist pricetag on pricetag.id = stock.pricetagid
                                    where barcode=@barcode and stock.isactive=true
                                    order by stock.id";

        const string SQL_SearchByBarcodeInMigration = @"select stock.id, stock.isactive, stock.createdby, stock.modifiedby, stock.itemid,stock.productid,stock.returnqty, 
                                    stock.version, stock.createdon, stock.modifiedon,COALESCE(stock.buyingprice,lrinvoiceitems.buyingprice) as buyingprice,stock.lrreference, stock.designid,printingname, price, lrinvoiceitems.buyingqty,lrinvoiceid,
				                    barcode, stock.serialno,invoiceitemid,stock.companyid,lrinvoiceitems.purchasediscount,lrinvoiceitems.purchasediscountpercentage,lrinvoice.invoicedate,
                                    tax.id as taxid,tax.name as taxname,purchasetaxpercentage taxpercentage,stock.discount,stock.additionaldiscount,stock.qty,stock.transfered,lrinvoiceitems.purchasetax,
                                    stock.damagedqty,stock.journalqty,supplierid,stock.supplierid stocksupplierid,stock.hsncode,stock.ibuyingprice,stock.dealerprice wsrate,lrinvoice.invoiceno,
                                    cast(lrinvoiceitems.itemDetail->>'minrate' as numeric) minrate,cast(lrinvoiceitems.itemDetail->>'dealerprice' as numeric) dealerprice,lrinvoiceitems.sno,lrinvoiceitems.mrp,runtimeprice,lrinvoiceitems.basebuyingprice,
                                    lrinvoiceitems.taxid salestaxid, stock.stockdate, stock.imageid, company.name companyname, stock.srcsupplierid,isuan,ismultiprice
                                    from stock left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid and lrinvoiceitems.isactive=true
                                    left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid and lrinvoice.isactive = true
                                    left outer join company on company.id = lrinvoice.companyid
                                    left outer join tax on tax.id = lrinvoiceitems.taxid
                                    where barcode=@barcode and stock.isactive=true";

        const string SQL_SearchByIDs = @"select stock.id, price,discount,additionaldiscount,buyingprice,productid from stock ";

        const string SQL_SearchByInvoiceIDs = @"
                                                select stock.id, stock.isactive, stock.createdby, stock.modifiedby, stock.itemid,stock.productid,stock.returnqty, lrentry.supplierid,
                                                stock.version, stock.createdon, stock.modifiedon, products.name printingname, 
                                                stock.buyingprice buyingprice,stock.lrreference, stock.designid, price, barcode,
                                                invoiceitemid,stock.companyid,lrinvoice.invoicedate,stock.hsncode,products.sellingmode,isbit,
                                                company.code companycode,company.name companyname,
                                                stock.discount,additionaldiscount,qty,transfered,stock.damagedqty,stock.journalqty,stock.buyingprice buyingprice,lrreference,
                                                sizelist.name as size,colourlist.name as colour, typelist.name as typename, stylelist.name stylename, 
                                                materiallist.name as materialname,brand.name brandname, patternlist.name patternname,
                                                fitlist.name fitname, sleevelist.name sleevename,isuan,ismultiprice,
                                                lrinvoiceitems.sno, stock.serialno, lrinvoiceitems.basebuyingprice,
                                                coalesce(supplier.code,stocksupplier.code) suppliercode,
                                                coalesce(lrinvoiceitems.mrp,stock.displaymrp) mrp,
                                                coalesce(lrinvoice.invoiceno,stock.displayinvoiceno) invoiceno,
                                                coalesce(lrentry.lrentrydate,stock.displayinvoicedate) lrentrydate,
                                                coalesce(lrinvoice.entrydate,stock.displayinvoicedate) entrydate,
                                                lrentry.sourcesupplierid,stock.dealerprice,
                                                products.code as productcode,
                                                sizelist.code as sizecode,
                                                typelist.code as typecode,
                                                colourlist.code as colourcode,
                                                materiallist.code as materialcode,
                                                stylelist.code as stylecode,
                                                brand.code as brandcode,
                                                patternlist.code as patterncode,
                                                sectionlist.name as sectionname,
                                                stock.retaillocationid retaillocationid,
                                                referencelist.code retaillocationcode,
                                                referencelist.name retaillocationname,
                                                productgroups.name produgroupname,
                                                productgroups.code produgroupcode,
                                                barcodegroup.name barcodegroupname,
                                                barcodegroup.code barcodegroupcode,
                                                items.gln,
                                                pricetag.name pricetagname, 
                                                items.code itemcode,
                                                items.printingname itemname,
                                                lrinvoiceitems.salediscountpercentage
                                                from stock 
                                                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
                                                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                                                left outer join lrentry on lrentry.id = lrinvoice.lrid  and lrentry.isactive = true
                                                left outer join supplier on supplier.id = lrentry.supplierid 
                                                left outer join supplier stocksupplier on stocksupplier.id = stock.supplierid
                                                left outer join items on items.id = stock.itemid
                                                left outer join products on products.id = stock.productid
                                                left outer join referencelist sizelist on sizelist.id = items.sizeid
                                                left outer join referencelist colourlist on colourlist.id = items.colourid 
                                                left outer join referencelist patternlist on patternlist.id = items.patternid
                                                left outer join referencelist typelist on typelist.id = items.typeid 
                                                left outer join referencelist materiallist on materiallist.id = items.materialid
                                                left outer join referencelist stylelist on stylelist.id = items.styleid 
                                                left outer join referencelist fitlist on fitlist.id = items.fitid
                                                left outer join referencelist sleevelist on sleevelist.id = items.sleeveid
                                                left outer join referencelist sectionlist on sectionlist.id = products.salescounterid
                                                left outer join referencelist productgroups on productgroups.id = products.productgroup
                                                left outer join referencelist barcodegroup on barcodegroup.id = products.barcodeid
                                                left outer join referencelist pricetag on pricetag.id = stock.pricetagid
                                                left outer join brand on brand.id = items.brandid
                                                left outer join company on company.id = stock.companyid 
                                                left outer join referencelist on referencelist.id = stock.retaillocationid and referencelist.type = 'LOCATION' ";

        const string SQL_SearchByStockIDs = @"
                                        select stock.id, stock.isactive, stock.createdby, stock.modifiedby, stock.itemid,stock.productid,stock.returnqty, lrentry.supplierid,
                                        stock.version, stock.createdon, stock.modifiedon, products.name printingname, stock.buyingprice buyingprice,stock.lrreference, stock.designid, salablegoods.price, salablegoods.barcode,
                                        invoiceitemid,stock.companyid,lrinvoice.invoicedate,stock.hsncode,products.sellingmode,isbit,company.code companycode,company.name companyname,
                                        salablegoods.discount,salablegoods.additionaldiscount,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) qty,salablegoods.iqty transfered,
                                        salablegoods.dqty damagedqty,salablegoods.jqty journalqty,stock.buyingprice buyingprice,lrreference,
                                        sizelist.name as size,colourlist.name as colour, typelist.name as typename, stylelist.name stylename, 
                                        materiallist.name as materialname,brand.name brandname, patternlist.name patternname,
                                        fitlist.name fitname, sleevelist.name sleevename,isuan,ismultiprice,
                                        lrinvoiceitems.sno, stock.serialno, lrinvoiceitems.basebuyingprice,
                                        coalesce(supplier.code,stocksupplier.code) suppliercode,
                                        coalesce(lrinvoiceitems.mrp,stock.displaymrp) mrp,
                                        coalesce(lrinvoice.invoiceno,stock.displayinvoiceno) invoiceno,
                                        coalesce(lrentry.lrentrydate,stock.displayinvoicedate) lrentrydate,
                                        coalesce(lrinvoice.entrydate,stock.displayinvoicedate) entrydate,
                                        lrentry.sourcesupplierid,stock.dealerprice,
                                        products.code as productcode,
                                        sizelist.code as sizecode,
                                        typelist.code as typecode,
                                        colourlist.code as colourcode,
                                        materiallist.code as materialcode,
                                        stylelist.code as stylecode,
                                        brand.code as brandcode,
                                        patternlist.code as patterncode,
                                        sectionlist.name as sectionname,
                                        stock.retaillocationid retaillocationid,
                                        referencelist.code retaillocationcode,
                                        referencelist.name retaillocationname,
                                        productgroups.name produgroupname,
                                        productgroups.code produgroupcode,
                                        barcodegroup.name barcodegroupname,
                                        barcodegroup.code barcodegroupcode,
                                        pricetag.name pricetagname, 
                                        items.gln,
                                        items.code itemcode,
                                        items.printingname itemname,
                                        lrinvoiceitems.salediscountpercentage
                                        from salablegoods 
                                        inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true
                                        left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                                        left outer join lrentry on lrentry.id = lrinvoice.lrid  and lrentry.isactive = true
                                        left outer join supplier on supplier.id = lrentry.supplierid 
                                        left outer join supplier stocksupplier on stocksupplier.id = stock.supplierid
                                        left outer join items on items.id = stock.itemid
                                        left outer join products on products.id = stock.productid
                                        left outer join referencelist sizelist on sizelist.id = items.sizeid
                                        left outer join referencelist colourlist on colourlist.id = items.colourid 
                                        left outer join referencelist patternlist on patternlist.id = items.patternid
                                        left outer join referencelist typelist on typelist.id = items.typeid 
                                        left outer join referencelist materiallist on materiallist.id = items.materialid
                                        left outer join referencelist stylelist on stylelist.id = items.styleid 
                                        left outer join referencelist fitlist on fitlist.id = items.fitid
                                        left outer join referencelist sleevelist on sleevelist.id = items.sleeveid
                                        left outer join referencelist sectionlist on sectionlist.id = products.salescounterid
                                        left outer join referencelist productgroups on productgroups.id = products.productgroup
                                        left outer join referencelist barcodegroup on barcodegroup.id = products.barcodeid
                                        left outer join referencelist pricetag on pricetag.id = stock.pricetagid
                                        left outer join brand on brand.id = items.brandid
                                        left outer join company on company.id = stock.companyid 
                                        left outer join referencelist on referencelist.id = stock.retaillocationid and referencelist.type = 'LOCATION' ";

        const string SQL_SearchByInvoiceID = @"select stock.id, stock.isactive, stock.createdby, stock.modifiedby, itemid,productid,returnqty,supplierid,stock.hsncode, dealerprice,
                                    stock.version, stock.createdon, stock.modifiedon, stock.buyingprice buyingprice,stock.lrreference, stock.designid, printingname, price, barcode, invoiceitemid,companyid,
                                    tax.id as taxid,tax.name as taxname,tax.taxpercentage,discount,additionaldiscount,qty,transfered,damagedqty,journalqty,isbit from stock 
                                    inner join products on products.id = stock.productid
                                    inner join tax on tax.id = products.salestaxid
                                    where barcode=@barcode and stock.isactive=true";

        public const string SQL_SearchSupplierID = @"select lrentry.supplierid,lrentry.sourcesupplierid,lrentry.lrentryno,sno,buyingprice,
                                    (case when lrinvoiceitems.purchasetax>0 and lrinvoiceitems.buyingqty>0
	                                then (lrinvoiceitems.purchasetax/lrinvoiceitems.buyingqty)::numeric else 0::numeric end )+buyingprice::numeric ibuyingprice,
                                    designid,sellingmode,lrentry.locationid, lrinvoiceitems.dealerprice,imgid,
                                    lrentry.retaillocationid,itemdetail,mrp from lrinvoiceitems
                                    inner join products on products.id = lrinvoiceitems.productid
                                    inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                                    inner join lrentry on lrentry.id = lrinvoice.lrid
                                    where lrinvoiceitems.id = @lrinvoiceitemsid
                                    LIMIT 1";

        const string SQL_Insert = @"INSERT INTO public.stock(isactive, createdby, modifiedby, version, createdon, modifiedon, printingname, price, barcode, companyid, invoiceitemid,dealerprice, itemid, productid, discount, additionaldiscount, qty,transfered,damagedqty,journalqty,returnqty,supplierid,buyingprice,lrreference, designid,stockdate,hsncode,isbit,serialno,expiry,batch,retaillocationid,locationid,purchaseqty,purchasereturnqty,transferedin,transferedout,ibuyingprice,runtimeprice,displaymrp,displayinvoiceno,displayinvoicedate,imageid,eprice,srcsupplierid,isuan,ismultiprice,pricetagid) 
        VALUES (@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @printingname, @price, @barcode, @companyid, @invoiceitemid,@dealerprice, @itemid, @productid, @discount, @additionaldiscount, @qty, @transfered ,@damagedqty,@journalqty,@returnqty,@supplierid,@buyingprice,@lrreference, @designid,@stockdate,@hsncode,@isbit,@serialno,@expiry,@batch,@retaillocationid,@locationid,@purchaseqty,0,0,0,@ibuyingprice,@runtimeprice,@displaymrp,@displayinvoiceno,@displayinvoicedate,@imageid,@eprice,@srcsupplierid,@isuan,@ismultiprice,@pricetagid) RETURNING id";

        const string SQL_Update = @"UPDATE public.stock SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, printingname=@printingname, companyid=@companyid, price=@price, barcode=@barcode,dealerprice=@dealerprice, invoiceitemid=@invoiceitemid, itemid=@itemid, productid=@productid, discount=@discount,
        additionaldiscount=@additionaldiscount, qty=@qty,damagedqty=@damagedqty,journalqty=@journalqty,transfered=@transfered,buyingprice=@buyingprice,lrreference=@lrreference, designid=@designid,hsncode=@hsncode,isbit=@isbit,pricetagid=@pricetagid where id=@id and version=@version RETURNING version";

        const string SQL_UpdateItemID = "Update public.stock SET itemid=@itemid, designid=@designid where invoiceitemid=@invoiceitemid";
        internal void UpdateItemID(long invoiceitemid, long itemid, string designid)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_UpdateItemID, conn, transaction);
                    command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itemid;
                    command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = invoiceitemid;
                    command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(designid) ? "" : designid;
                    command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
        }

        const string SQL_Delete = @"update public.stock set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_DeleteAll = @"update public.stock set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where invoiceitemid=@invoiceitemid";
        public StockWithTax Search(long companyid, string barcode)
        {
            StockWithTax ostock = new StockWithTax();

            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();
                    ostock = Search(companyid, barcode, conn, null);
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    throw exp;
                }
            }
            return ostock;
        }
        public List<StockWithTax> Search(long companyid, List<string> barcode)
        {
            return Search(companyid, barcode, null);
        }
        public List<StockWithTax> Search(long companyid, List<long> lrinvoiceitemids)
        {
            return Search(companyid, null, lrinvoiceitemids);
        }
        public List<StockWithTax> Search(long companyid, List<string> barcode, List<long> lrinvoiceitemids)
        {
            NpgsqlConnection conn = null;
            List<StockWithTax> stockList = new List<StockWithTax>();
            try
            {
                conn = CreateConnection();
                conn.Open();

                if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_fixed_barcodes &&
                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                {
                    SortedDictionary<string, List<string>> uanquantolist = new SortedDictionary<string, List<string>>();
                    using (var reader = ExecuteReader("select barcode,serialno from stock where serialno in (" + string.Join(",", barcode.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")", new SortedList<string, object>()))
                    {
                        while (reader.Read())
                        {
                            string quantobarcode = reader.GetString(0);
                            string uanbarcode = reader.GetString(1);
                            if (!uanquantolist.ContainsKey(uanbarcode))
                            {
                                uanquantolist.Add(uanbarcode, new List<string>());
                            }
                            uanquantolist[uanbarcode].Add(quantobarcode);
                        }
                    }
                    foreach (var uanquanto in uanquantolist)
                    {
                        barcode.Remove(uanquanto.Key);
                        barcode.AddRange(uanquanto.Value);
                    }
                }

                NpgsqlCommand command = null;
                if (barcode != null)
                {
                    command = GetCommand(SQL_SearchByBarcode.Replace("barcode=@barcode", "barcode in (" + string.Join(",", barcode.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")"), conn);
                }
                else
                {
                    command = GetCommand(SQL_SearchByBarcode.Replace("barcode=@barcode", "invoiceitemid in (" + string.Join(",", lrinvoiceitemids) + ")"), conn);
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockWithTax ostock = new StockWithTax();
                        stockList.Add(ostock);
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostock.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        ostock.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        ostock.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        ostock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.itemname = reader["itemname"] == DBNull.Value ? "" : reader["itemname"].ToString();
                        ostock.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"].ToString());
                        ostock.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        ostock.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        ostock.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        ostock.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());
                        //ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        ostock.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        ostock.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        ostock.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        ostock.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();
                        ostock.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();

                        ostock.isuan = reader["isuan"] == DBNull.Value ? false : Convert.ToBoolean(reader["isuan"]);
                        ostock.ismultiprice = reader["ismultiprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["ismultiprice"]);

                        ostock.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());

                        ostock.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        ostock.taxname = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                        ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());

                        ostock.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        ostock.minrate = reader["minrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minrate"].ToString());
                        ostock.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());

                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        ostock.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                        ostock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ostock.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        ostock.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                        ostock.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        ostock.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        ostock.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());

                        ostock.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"].ToString());
                        ostock.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                        ostock.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"].ToString());
                        ostock.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        ostock.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ostock.srcsupplierid = reader["srcsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["srcsupplierid"]);
                        ostock.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());

                        ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();

                        if (ostock.invoiceitemid < 0 || ostock.supplierid == 0)
                            ostock.supplierid = reader["stocksupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocksupplierid"]);
                        if (ostock.buyingqty > 0 && ostock.purchasetax > 0)
                            ostock.purchasetax = Math.Round(ostock.purchasetax / ostock.buyingqty, 2);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }

            return stockList;
        }

        public StockWithTax Search(long companyid, string barcode, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            StockWithTax ostock = new StockWithTax();
            try
            {
                NpgsqlCommand command = transaction == null ? GetCommand(SQL_SearchByBarcode, conn) : GetCommand(SQL_SearchByBarcodeInMigration, conn, transaction);
                if (companyid > 0)
                {
                    command.CommandText = command.CommandText.Replace("barcode=@barcode", "barcode=@barcode and stock.companyid=@companyid");
                    command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = companyid;
                }
                command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = barcode;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostock.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                        if (transaction == null)
                            ostock.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        ostock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostock.stockdate = reader["stockdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["stockdate"]);
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        ostock.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        ostock.isuan = reader["isuan"] == DBNull.Value ? false : Convert.ToBoolean(reader["isuan"]);
                        ostock.ismultiprice = reader["ismultiprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["ismultiprice"]);

                        if (transaction == null)
                        {
                            ostock.itemname = reader["itemname"] == DBNull.Value ? "" : reader["itemname"].ToString();
                            ostock.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"].ToString());
                            ostock.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            ostock.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            ostock.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            ostock.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());
                            //ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                            ostock.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            ostock.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            ostock.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                            ostock.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                            ostock.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            ostock.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();
                            ostock.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        }

                        ostock.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        ostock.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.companyname = reader["companyname"] == DBNull.Value ? "" : reader["companyname"].ToString();
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.salestaxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"].ToString());
                        ostock.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        ostock.taxname = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                        ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        ostock.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        ostock.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        ostock.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                        ostock.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                        ostock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ostock.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        ostock.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"].ToString());
                        ostock.purchasediscountpercentage = reader["purchasediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscountpercentage"].ToString());
                        ostock.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        ostock.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        if (transaction == null)
                            ostock.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"].ToString());
                        ostock.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"].ToString());
                        ostock.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"].ToString());
                        ostock.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"].ToString());
                        ostock.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ostock.srcsupplierid = reader["srcsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["srcsupplierid"]);
                        ostock.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"].ToString());

                        ostock.dealerprice = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"].ToString());
                        if (ostock.dealerprice == 0)
                            ostock.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());

                        ostock.minrate = reader["minrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minrate"].ToString());
                        ostock.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();

                        if (ostock.invoiceitemid < 0 || ostock.supplierid == 0)
                            ostock.supplierid = reader["stocksupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stocksupplierid"]);

                        if (ostock.buyingqty > 0 && ostock.purchasetax > 0)
                            ostock.purchasetax = Math.Round(ostock.purchasetax / ostock.buyingqty, 2);

                        if (ostock.ibuyingprice > 0)
                            ostock.marginpercent = Math.Round(((ostock.price - ostock.discount - ostock.ibuyingprice) / ostock.ibuyingprice) * 100, 2);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            if (!string.IsNullOrEmpty(ostock.barcode) && ostock.id == 0)
            {
                if (WebAPI.Models.HardConfig.SupportAutoCAPS(Context.OrganizationID) && Char.IsLower(ostock.barcode, 0))
                {
                    ostock.barcode = ostock.barcode.ToUpper();
                    ostock = Search(companyid, barcode, conn, transaction);
                }
            }
            return ostock;
        }

        string syncStockQuery = @"Update stock set modifiedby=@modifiedby, modifiedon=@modifiedon, journalqty = (case when @stock>(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) then journalqty-(@stock - (stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)))
when @stock<(stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)) then journalqty+((stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty))-@stock)
else stock.journalqty end) where stock.id=@stockid RETURNING journalqty,qty-(transfered+journalqty+damagedqty+returnqty)";

        string syncSableGoodsQuery = @"Update salablegoods set modifiedby=@modifiedby, modifiedon=@modifiedon, jqty = (case when @stock>(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) then jqty-(@stock - ( salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)))
when @stock<(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) then jqty+((salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty))-@stock)
else salablegoods.jqty end) where salablegoods.id=@stockid RETURNING jqty,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)";

        string syncSableGoodswithAqtyQuery = @"Update salablegoods set modifiedby=@modifiedby, modifiedon=@modifiedon, jqty = (case when @stock>(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)) then jqty-(@stock - ( salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)))
when @stock<(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty)) then jqty+((salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty+salablegoods.aqty))-@stock)
else salablegoods.jqty end) where salablegoods.id=@stockid RETURNING jqty,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)";

        public StockSync SyncStock(StockSync syncstocks)
        {

            if (syncstocks.items == null || syncstocks.items.Count == 0) return syncstocks;
            if (syncstocks.mode == 1)
            {
                List<Stockhistory> stockhistories = new List<Stockhistory>();
                var stocklist = Search(new StockCreteria()
                {
                    ids = syncstocks.items.Select(e => e.stockid).ToList()
                });

                List<Stock> uanstocklist = null;
                if (syncstocks.uanitems != null &&
                    syncstocks.uanitems.Count > 0)
                {
                    uanstocklist = Search(new StockCreteria()
                    {
                        uanbarcodes = syncstocks.uanitems.ConvertAll(x => x.barcode).Distinct().ToList()
                    }).OrderByDescending(x => x.id).ToList();
                    foreach (var uanitem in syncstocks.uanitems)
                    {
                        if (uanitem.stock > 0)
                        {
                            var availstocklist = uanstocklist.FindAll(x => x.serialno == uanitem.barcode && x.qty - x.transfered > 0);
                            if (availstocklist.Count > 0)
                            {
                                syncstocks.items.Add(new StockSync.Item()
                                {
                                    barcode = availstocklist[0].barcode,
                                    stock = availstocklist[0].stockqty + uanitem.stock,
                                    stockid = availstocklist[0].id
                                });
                                stocklist.Add(availstocklist[0]);
                            }
                            else if (uanstocklist.Exists(x => x.serialno == uanitem.barcode))
                            {
                                var adjstock = uanstocklist.Find(x => x.serialno == uanitem.barcode);
                                syncstocks.items.Add(new StockSync.Item()
                                {
                                    barcode = adjstock.barcode,
                                    stock = adjstock.stockqty + uanitem.stock,
                                    stockid = adjstock.id
                                });
                                stocklist.Add(adjstock);
                            }
                        }
                        else if (uanitem.stock < 0)
                        {
                            var availstocklist = uanstocklist.FindAll(x => x.serialno == uanitem.barcode && x.stockqty > 0);
                            if (availstocklist.Count > 0)
                            {
                                syncstocks.items.Add(new StockSync.Item()
                                {
                                    barcode = availstocklist[0].barcode,
                                    stock = availstocklist[0].stockqty - uanitem.stock,
                                    stockid = availstocklist[0].id
                                });
                                stocklist.Add(availstocklist[0]);
                            }
                            else if (uanstocklist.Exists(x => x.serialno == uanitem.barcode))
                            {
                                var adjstock = uanstocklist.Find(x => x.serialno == uanitem.barcode);
                                syncstocks.items.Add(new StockSync.Item()
                                {
                                    barcode = adjstock.barcode,
                                    stock = adjstock.stockqty - uanitem.stock,
                                    stockid = adjstock.id
                                });
                                stocklist.Add(adjstock);
                            }
                        }
                    }
                }
                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        NpgsqlCommand command = GetCommand(syncStockQuery, conn, transaction);
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("stock", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        foreach (var item in syncstocks.items)
                        {
                            decimal damged = 0, stock = 0;
                            command.Parameters["stockid"].Value = item.stockid;
                            command.Parameters["stock"].Value = item.stock;
                            command.Parameters["modifiedby"].Value = Context.UserID;
                            command.Parameters["modifiedon"].Value = DateTimeNow;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    damged = reader.GetDecimal(0);
                                    stock = reader.GetDecimal(1);
                                }
                            }
                            var olditem = stocklist.Find(e => e.id == item.stockid);
                            if (olditem != null)
                            {
                                // Stock History
                                StockhistoryDataService stockhistorydata = new StockhistoryDataService() { Context = this.Context };
                                Stockhistory stockhistory = olditem.GetStockHistory();
                                stockhistory.actiontype = Stockhistory.ActionType.StockAudit;
                                stockhistory.modifiedby = Context.UserID;
                                stockhistory.modifiedon = DateTimeNow;
                                stockhistory.reason = syncstocks.reason;
                                stockhistory.auditqty = item.stock - (stockhistory.qty - (stockhistory.transfered + stockhistory.journalqty + stockhistory.damagedqty + stockhistory.returnqty));
                                stockhistories.Add(stockhistorydata.Insert(transaction, conn, stockhistory));
                            }
                        }
                        StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                        stocktransaction.SyncStock(stockhistories, transaction, conn);
                        transaction.Commit();
                        conn.Close();
                    }
                    return syncstocks;
                }
                catch (Exception exp)
                {
                    return syncstocks;
                }
            }
            else
            {
                List<Salablegoodshistory> stockhistories = new List<Salablegoodshistory>();
                NpgsqlTransaction transaction = null;
                try
                {
                    SalablegoodsDataService salblestockdata = new SalablegoodsDataService() { Context = this.Context };
                    var transferstock = syncstocks.items.Where(e => e.stockid > 0 && e.salablegoodsid == 0).ToList();
                    List<Salablegoods> newStockList = new List<Salablegoods>();
                    if (transferstock.Count > 0)
                    {
                        syncstocks.items.RemoveAll(e => e.stockid > 0 && e.salablegoodsid == 0);
                        var whstocklist = Search(new StockCreteria()
                        {
                            ids = transferstock.ConvertAll(e => e.stockid)
                        });

                        foreach (var item in whstocklist)
                        {
                            Salablegoods sitem = new Salablegoods();
                            sitem.inwardon = DateTimeNow;
                            sitem.barcode = item.barcode;
                            sitem.bundleid = -3;
                            sitem.iqty = transferstock.Find(e => e.stockid == item.id).stock;
                            sitem.isactive = true;
                            sitem.itemid = item.itemid;
                            sitem.locationid = syncstocks.locationid;
                            sitem.stockid = item.id;
                            sitem.version = 1;
                            sitem.companyid = item.companyid;
                            sitem.supplierid = item.supplierid;
                            sitem.price = item.price;
                            sitem.discount = item.discount;
                            sitem.additionaldiscount = item.additionaldiscount;
                            sitem.received = true;
                            newStockList.Add(sitem);
                        }

                        var existingStocks = salblestockdata.Search(new SalablegoodsCreteria()
                        {
                            barcodes = newStockList.ConvertAll(e => e.barcode),
                            locationid = syncstocks.locationid
                        });
                        foreach (var item in existingStocks)
                        {
                            var stockitem = transferstock.Find(e => e.stockid == item.stockid);
                            stockitem.salablegoodsid = item.id;
                            syncstocks.items.Add(stockitem);
                            newStockList.RemoveAll(e => e.stockid == item.stockid);
                        }
                    }

                    SalablegoodsDataService salablegoodsdata = new SalablegoodsDataService() { Context = this.Context };
                    var salablegoodslist = salablegoodsdata.Search(new SalablegoodsCreteria()
                    {
                        ids = syncstocks.items.Select(e => e.salablegoodsid).ToList()
                    });

                    List<Salablegoods> uansalablegoodslist = null;
                    if (syncstocks.uanitems != null &&
                        syncstocks.uanitems.Count > 0)
                    {
                        uansalablegoodslist = salablegoodsdata.Search(new SalablegoodsCreteria()
                        {
                            uanbarcodes = syncstocks.uanitems.ConvertAll(x => x.barcode).Distinct().ToList(),
                            locationid = syncstocks.locationid
                        });
                        foreach (var uanitem in syncstocks.uanitems)
                        {
                            if (uanitem.stock > 0)
                            {
                                var availstocklist = uansalablegoodslist.FindAll(x => x.serialno == uanitem.barcode && x.iqty - x.oqty > 0);
                                if (availstocklist.Count > 0)
                                {
                                    if (syncstocks.items.Exists(x => x.salablegoodsid == availstocklist[0].id))
                                    {
                                        var titem = syncstocks.items.Find(x => x.salablegoodsid == availstocklist[0].id);
                                        titem.stock += uanitem.stock;
                                    }
                                    else
                                    {
                                        syncstocks.items.Add(new StockSync.Item()
                                        {
                                            barcode = availstocklist[0].barcode,
                                            stock = availstocklist[0].stockqty + uanitem.stock,
                                            stockid = availstocklist[0].id,
                                            salablegoodsid = availstocklist[0].id
                                        });
                                        salablegoodslist.Add(availstocklist[0]);
                                    }
                                }
                                else if (uansalablegoodslist.Exists(x => x.serialno == uanitem.barcode))
                                {
                                    var adjstock = uansalablegoodslist.Find(x => x.serialno == uanitem.barcode);
                                    if (syncstocks.items.Exists(x => x.salablegoodsid == adjstock.id))
                                    {
                                        var titem = syncstocks.items.Find(x => x.salablegoodsid == adjstock.id);
                                        titem.stock += uanitem.stock;
                                    }
                                    else
                                    {
                                        syncstocks.items.Add(new StockSync.Item()
                                        {
                                            barcode = adjstock.barcode,
                                            stock = adjstock.stockqty + uanitem.stock,
                                            stockid = adjstock.id,
                                            salablegoodsid = adjstock.id
                                        });
                                        salablegoodslist.Add(adjstock);
                                    }
                                }
                            }
                            else if (uanitem.stock < 0)
                            {
                                var availstocklist = uansalablegoodslist.FindAll(x =>
                                !syncstocks.items.Exists(y=>y.salablegoodsid == x.id) &&
                                x.serialno == uanitem.barcode && x.stockqty > uanitem.stock);
                                if (availstocklist.Count > 0)
                                {
                                    syncstocks.items.Add(new StockSync.Item()
                                    {
                                        barcode = availstocklist[0].barcode,
                                        stock = availstocklist[0].stockqty - uanitem.stock,
                                        stockid = availstocklist[0].id,
                                        salablegoodsid = availstocklist[0].id
                                    });
                                    salablegoodslist.Add(availstocklist[0]);
                                }
                                else if (uansalablegoodslist.Exists(x => x.serialno == uanitem.barcode))
                                {
                                    var required = uanitem.stock;
                                    var adjstocks = uansalablegoodslist.FindAll(x => !syncstocks.items.Exists(y => y.salablegoodsid == x.id));
                                    if(adjstocks.Count>0)
                                    {
                                        var titems = adjstocks.OrderByDescending(x => x.stockqty).ToList();
                                        foreach(var titem in titems)
                                        {
                                            var adstock = titem.stockqty >= uanitem.stock? uanitem.stock: titem.stockqty;
                                            syncstocks.items.Add(new StockSync.Item()
                                            {
                                                barcode = titem.barcode,
                                                stock = adstock,
                                                stockid = titem.id,
                                                salablegoodsid = titem.id
                                            });
                                            required -= adstock;
                                            salablegoodslist.Add(titem);
                                            if (required <= 0) break;
                                        }
                                    }
                                    if(required > 0)
                                    {
                                        var titems = syncstocks.items.FindAll(y => uansalablegoodslist.Exists(x => x.id == y.salablegoodsid));
                                        foreach (var titem in titems)
                                        {
                                            var adstock = titem.stock >= uanitem.stock ? uanitem.stock : titem.stock;
                                            titem.stock -= adstock;
                                            required -= adstock;
                                            if (required <= 0) break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        NpgsqlCommand command = GetCommand(Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty ? syncSableGoodswithAqtyQuery : syncSableGoodsQuery, conn, transaction);
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("stock", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        foreach (var item in syncstocks.items)
                        {
                            decimal damged = 0, stock = 0;
                            command.Parameters["stockid"].Value = item.salablegoodsid;
                            command.Parameters["stock"].Value = item.stock;
                            command.Parameters["modifiedby"].Value = Context.UserID;
                            command.Parameters["modifiedon"].Value = DateTimeNow;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    damged = reader.GetDecimal(0);
                                    stock = reader.GetDecimal(1);
                                }
                            }
                            var olditem = salablegoodslist.Find(e => e.id == item.salablegoodsid);
                            if (olditem != null)
                            {
                                var jqty = olditem.jqty - damged;
                                // Salablegoods History
                                SalablegoodshistoryDataService salablegoodshistorydata = new SalablegoodshistoryDataService() { Context = this.Context };
                                Salablegoodshistory salablegoodshistory = olditem.GetSalablegoodsHistory();
                                salablegoodshistory.actiontype = Salablegoodshistory.ActionType.StockAudit;
                                salablegoodshistory.modifiedby = Context.UserID;
                                salablegoodshistory.modifiedon = DateTimeNow;
                                salablegoodshistory.reason = syncstocks.reason;
                                salablegoodshistory.auditqty = item.stock - (salablegoodshistory.iqty - (salablegoodshistory.oqty + salablegoodshistory.jqty + salablegoodshistory.dqty));
                                salablegoodshistory.auditqty = jqty;
                                if (jqty != 0)
                                {
                                    stockhistories.Add(salablegoodshistorydata.Insert(transaction, conn, salablegoodshistory));
                                }
                            }
                        }

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config != null
                                && !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                        {
                            if (newStockList.Count > 0)
                            {

                                salblestockdata.Context = this.Context;
                                salblestockdata.isMigration = true;

                                foreach (var item in newStockList)
                                {
                                    salblestockdata.Insert(transaction, conn, item);
                                    Salablegoodshistory salablegoodshistory = item.GetSalablegoodsHistory();
                                    salablegoodshistory.actiontype = Salablegoodshistory.ActionType.StockAudit;
                                    salablegoodshistory.modifiedby = Context.UserID;
                                    salablegoodshistory.modifiedon = DateTimeNow;
                                    salablegoodshistory.auditqty = item.iqty;
                                    stockhistories.Add(salablegoodshistory);
                                }
                            }
                        }

                        StocktransactionDataService stocktransaction = new StocktransactionDataService() { Context = this.Context };
                        stocktransaction.SyncStock(stockhistories, transaction, conn);
                        transaction.Commit();
                        conn.Close();
                    }
                    return syncstocks;
                }
                catch (Exception exp)
                {
                    return syncstocks;
                }
            }
        }
        public List<StockReport> SearchStock(StockReportCreteria _Stock)
        {
            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchStock;

                if (_Stock.companyid > 0)
                    qb.AddParameter("companyid", _Stock.companyid, "=");
                if (_Stock.patternid > 0)
                    qb.AddParameter("patternid", _Stock.patternid, "=");
                if (_Stock.brandid > 0)
                    qb.AddParameter("brandid", _Stock.brandid, "=");
                if (_Stock.productid > 0)
                    qb.AddParameter("productid", _Stock.productid, "=");
                if (_Stock.supplierid > 0)
                    qb.AddParameter("supplierid", _Stock.supplierid, "=");
                if (_Stock.materialid > 0)
                    qb.AddParameter("materialid", _Stock.materialid, "=");
                if (_Stock.styleid > 0)
                    qb.AddParameter("styleid", _Stock.styleid, "=");

                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockReport ostock = new StockReport();
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"].ToString());
                        ostock.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"].ToString());
                        //ostock.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"].ToString());
                        //ostock.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        //ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        //ostock.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"].ToString());
                        ostock.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());


                        lst.Add(ostock);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<StockReport> SearchRetailStock(StockReportCreteria _Stock)
        {
            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchRetailStock;

                if (_Stock.companyid > 0)
                    qb.AddParameter("companyid", _Stock.companyid, "=");
                if (_Stock.patternid > 0)
                    qb.AddParameter("patternid", _Stock.patternid, "=");
                if (_Stock.brandid > 0)
                    qb.AddParameter("brandid", _Stock.brandid, "=");
                if (_Stock.productid > 0)
                    qb.AddParameter("productid", _Stock.productid, "=");
                if (_Stock.supplierid > 0)
                    qb.AddParameter("supplierid", _Stock.supplierid, "=");
                if (_Stock.materialid > 0)
                    qb.AddParameter("materialid", _Stock.materialid, "=");
                if (_Stock.styleid > 0)
                    qb.AddParameter("styleid", _Stock.styleid, "=");

                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockReport ostock = new StockReport();
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                        ostock.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                        ostock.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"].ToString());
                        ostock.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"].ToString());
                        //ostock.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"].ToString());
                        ostock.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        ostock.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"].ToString());
                        ostock.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());


                        lst.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }
        public List<Stock> SearchForPrice(List<long> ids)
        {
            List<Stock> ostocks = new List<Stock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchByIDs;
                qb.AddParameter("stock.id", "ids", string.Join<long>(",", ids), " in ", "(" + string.Join<long>(",", ids) + ")");
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stock ostock = new Stock();
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        ostock.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        ostock.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        if (ostock.buyingprice == 0)
                            ostock.buyingprice = ostock.price - (ostock.discount + ostock.additionaldiscount);
                        ostocks.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return ostocks;
        }
        public List<Stock2Print> SearchForIds(string invoiceitemids, string stockids)
        {
            return SearchForIds(invoiceitemids, stockids, 0);
        }
        public List<Stock2Print> SearchForIds(string invoiceitemids, string stockids, long locationid)
        {
            List<Stock2Print> ostocks = new List<Stock2Print>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();

                if (locationid > 0)
                {
                    qb.CurrentQuery = SQL_SearchByStockIDs;
                    qb.AddParameter("salablegoods.locationid", "locationid", locationid, "=");
                }
                else
                    qb.CurrentQuery = SQL_SearchByInvoiceIDs;

                if (!string.IsNullOrEmpty(invoiceitemids))
                    qb.AddParameter("stock.invoiceitemid", "ids", invoiceitemids, " in ", "(" + invoiceitemids + ")");
                else if (!string.IsNullOrEmpty(stockids))
                    qb.AddParameter("stock.id", "stocids", stockids, " in ", "(" + stockids + ")");
                qb.AddParameter("stock.isactive", true, "=");

                //qb.SortField = "sno,sizelist.sortingindex,sizelist.name,qty";
                qb.SortField = "lrinvoiceitems.sno,stock.invoiceitemid,stock.id";
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stock2Print ostock = new Models.Stock2Print(this.Context.OrganizationID);
                        ostocks.Add(ostock);
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostock.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        ostock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);

                        ostock.isuan = reader["isuan"] == DBNull.Value ? false : Convert.ToBoolean(reader["isuan"]);
                        ostock.ismultiprice = reader["ismultiprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["ismultiprice"]);

                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                        ostock.gln = reader["gln"] == DBNull.Value ? "" : reader["gln"].ToString();
                        ostock.itemcode = reader["itemcode"] == DBNull.Value ? "" : reader["itemcode"].ToString();
                        ostock.itemname = reader["itemname"] == DBNull.Value ? "" : reader["itemname"].ToString();

                        ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ostock.buyingday = reader["invoicedate"] == DBNull.Value ? 0 : int.Parse(Convert.ToDateTime(reader["invoicedate"]).ToString("dd"));
                        ostock.buyingyear = reader["invoicedate"] == DBNull.Value ? 0 : int.Parse(Convert.ToDateTime(reader["invoicedate"]).ToString("yy"));
                        ostock.buyingmonth = reader["invoicedate"] == DBNull.Value ? 0 : int.Parse(Convert.ToDateTime(reader["invoicedate"]).ToString("MM"));

                        ostock.entryday = reader["entrydate"] == DBNull.Value ? 0 : int.Parse(Convert.ToDateTime(reader["entrydate"]).ToString("dd"));
                        ostock.entryyear = reader["entrydate"] == DBNull.Value ? 0 : int.Parse(Convert.ToDateTime(reader["entrydate"]).ToString("yy"));
                        ostock.entrymonth = reader["entrydate"] == DBNull.Value ? 0 : int.Parse(Convert.ToDateTime(reader["entrydate"]).ToString("MM"));

                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.companycode = reader["companycode"] == DBNull.Value ? "" : reader["companycode"].ToString();
                        ostock.companyname = reader["companyname"] == DBNull.Value ? "" : reader["companyname"].ToString();
                        ostock.sno = reader["sno"] == DBNull.Value ? "" : reader["sno"].ToString();
                        ostock.retaillocationcode = reader["retaillocationcode"] == DBNull.Value ? "" : reader["retaillocationcode"].ToString();
                        ostock.retaillocationname = reader["retaillocationname"] == DBNull.Value ? "" : reader["retaillocationname"].ToString();

                        ostock.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();

                        //ostock.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        //ostock.taxname = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                        ostock.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        ostock.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        ostock.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"].ToString());
                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        ostock.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                        ostock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        //ostock.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        ostock.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        ostock.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                        ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        ostock.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"].ToString());
                        ostock.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                        ostock.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"].ToString());
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.sizename = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        ostock.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        ostock.colourname = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        ostock.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        ostock.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        ostock.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        ostock.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        ostock.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        ostock.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        ostock.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        ostock.pricetagname = reader["pricetagname"] == DBNull.Value ? "" : reader["pricetagname"].ToString();

                        ostock.salediscountpercentage = reader["salediscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salediscountpercentage"].ToString());

                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ostock.sourcesupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"]);
                        ostock.suppliercode = reader["suppliercode"] == DBNull.Value ? "" : reader["suppliercode"].ToString();
                        ostock.code_product = reader["productcode"] == DBNull.Value ? "" : reader["productcode"].ToString();
                        ostock.code_size = reader["sizecode"] == DBNull.Value ? "" : reader["sizecode"].ToString();
                        ostock.code_type = reader["typecode"] == DBNull.Value ? "" : reader["typecode"].ToString();
                        ostock.code_colour = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        ostock.code_material = reader["materialcode"] == DBNull.Value ? "" : reader["materialcode"].ToString();
                        ostock.code_style = reader["stylecode"] == DBNull.Value ? "" : reader["stylecode"].ToString();
                        ostock.code_brand = reader["brandcode"] == DBNull.Value ? "" : reader["brandcode"].ToString();
                        ostock.code_pattern = reader["patterncode"] == DBNull.Value ? "" : reader["patterncode"].ToString();
                        ostock.code_produgroup = reader["produgroupcode"] == DBNull.Value ? "" : reader["produgroupcode"].ToString();
                        ostock.code_barcodegroup = reader["barcodegroupcode"] == DBNull.Value ? "" : reader["barcodegroupcode"].ToString();
                        ostock.produgroupname = reader["produgroupname"] == DBNull.Value ? "" : reader["produgroupname"].ToString();
                        ostock.barcodegroupname = reader["barcodegroupname"] == DBNull.Value ? "" : reader["barcodegroupname"].ToString();
                        ostock.sellingmode = reader["sellingmode"] != DBNull.Value ? Convert.ToInt32(reader["sellingmode"]) : 0;
                        if (ostock.sellingmode == 3)
                            ostock.sizename = ostock.qty.ToString("F2");
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return ostocks;
        }

        public List<OutwardStock> OutwardSearch(OutwardSearchCreteria _stock)
        {
            List<OutwardStock> lst = new List<OutwardStock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();

                switch (_stock.mode)
                {
                    case OutwardSearchCreteria.SearchMode.Brand:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,items.sellingname,qty-journalqty-damagedqty-transfered as stockqty from stock
                        left outer join items on items.id = stock.id
                        where qty-journalqty-damagedqty-transfered>0 and items.brandid = @brandid";
                        break;
                    case OutwardSearchCreteria.SearchMode.Product:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,items.sellingname,qty-damagedqty-journalqty-transfered as stockqty from stock
                        left outer join items on items.id = stock.id
                        where qty-damagedqty-journalqty-transfered>0 and items.productid = @productid";
                        break;
                    case OutwardSearchCreteria.SearchMode.BarcodedOn:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,items.sellingname,qty-damagedqty-journalqty-transfered as stockqty from stock
                        left outer join items on items.id = stock.id
                        where qty-damagedqty-journalqty-transfered>0 and stock.createdon >= @barcodedfrom and stock.createdon < @barcodedto";
                        break;
                    case OutwardSearchCreteria.SearchMode.AllStock:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,items.sellingname,qty-damagedqty-journalqty-transfered as stockqty from stock
                        left outer join items on items.id = stock.id
                        where qty-damagedqty-journalqty-transfered>0 ";
                        break;
                    case OutwardSearchCreteria.SearchMode.LRNo:
                        qb.CurrentQuery = @"select stock.id,stock.barcode,items.sellingname,stock.qty-stock.damagedqty-journalqty-stock.transfered as stockqty from stock
                        inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        inner join lrentry on lrentry.id = lrinvoice.lrid
                        left outer join items on items.id = stock.id
                        where stock.qty-stock.damagedqty-journalqty-stock.transfered >0
                        and lrentry.lrentryno = @lrentryno and lrinvoiceitems.isactive=true";
                        break;
                }

                command = GetCommand(qb.CurrentQuery, conn);

                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.productid;
                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.brandid;
                command.Parameters.Add("barcodedfrom", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.barcodedfrom.Date;
                command.Parameters.Add("barcodedto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.barcodedto.Date.AddDays(1);
                command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.lrentryno;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OutwardStock ostock = new OutwardStock();
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"].ToString());
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.sellingname = reader["sellingname"] == DBNull.Value ? "" : reader["sellingname"].ToString();
                        lst.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }
        public List<Stock> Search(StockCreteria _stock)
        {
            List<Stock> lst = new List<Stock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(_stock, conn, null);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<Stock> Search(StockCreteria _stock, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            List<Stock> lst = new List<Stock>();


            NpgsqlCommand command = null;
            if (_stock.id != 0)
            {
                command = GetCommand(SQL_Search, conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.id;
            }
            else if (_stock.bundleid > 0)
            {
                command = GetCommand(SQL_BundleStock, conn, transaction);
                command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stock.bundleid;
            }
            else
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                if (_stock.invoiceitemid > 0)
                    qb.AddParameter("invoiceitemid", _stock.invoiceitemid, "=");

                if (!string.IsNullOrEmpty(_stock.barcode))
                    qb.AddParameter("barcode", _stock.barcode, "=");

                if (_stock.ids != null && _stock.ids.Count > 0)
                    qb.AddInParamer("stock.id", string.Join(",", _stock.ids));

                if (_stock.companyid > 0)
                {
                    qb.AddParameter("companyid", _stock.companyid, "=");
                }
                if (_stock.barcodes != null && _stock.barcodes.Count > 0)
                {
                    var barecodes = _stock.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'").ToList();
                    qb.AddInParamer("stock.barcode", string.Join(",", barecodes));
                    if (_stock.barcodes.Count > 20)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("StockDataService.Search",
                            WebAPI.Common.Logger.LogType.Sync,
                            Environment.StackTrace, _stock);
                    }
                }
                if (_stock.uanbarcodes != null && _stock.uanbarcodes.Count > 0)
                {
                    var barecodes = _stock.uanbarcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'").ToList();
                    qb.AddInParamer("stock.barcode", string.Join(",", barecodes));
                    if (_stock.uanbarcodes.Count > 20)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("StockDataService.Search",
                            WebAPI.Common.Logger.LogType.Sync,
                            Environment.StackTrace, _stock);
                    }
                }

                command = qb.GetCommand(conn, transaction);
                //command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _stock.Code;
            }

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Stock ostock = new Stock();
                    ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    ostock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    ostock.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                    ostock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    if (_stock.bundleid == 0)
                        ostock.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                    ostock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    ostock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    ostock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                    ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                    ostock.displaymrp = reader["displaymrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["displaymrp"].ToString());
                    ostock.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                    ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                    ostock.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                    ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    if (_stock.uanbarcodes != null && _stock.uanbarcodes.Count > 0)
                        ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                    ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                    ostock.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                    ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                    ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                    ostock.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                    ostock.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                    ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                    ostock.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"].ToString());
                    ostock.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                    ostock.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"].ToString());
                    ostock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                    ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                    ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    ostock.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                    ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                    ostock.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                    ostock.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"].ToString());
                    ostock.pricetagid = reader["pricetagid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["pricetagid"].ToString());
                    
                    lst.Add(ostock);
                }
            }

            if (!string.IsNullOrEmpty(_stock.barcode) && lst.Count == 0)
            {
                if (WebAPI.Models.HardConfig.SupportAutoCAPS(Context.OrganizationID) && Char.IsLower(_stock.barcode, 0))
                {
                    _stock.barcode = _stock.barcode.ToUpper();
                    lst = Search(_stock);
                }
            }
            return lst;
        }

        public List<Stock> SearchAll(long _invoiceitemid)
        {
            List<Stock> lst = new List<Stock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _invoiceitemid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stock ostock = new Stock();
                        ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostock.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ostock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                        ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        ostock.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        ostock.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"].ToString());

                        lst.Add(ostock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }


        public Stock Insert(Stock oStock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oStock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }

            return oStock;
        }

        public void PrepareUAN(ProcessManager.ProcessData data)
        {
            List<Stock> lst = new List<Stock>();
            try
            {
                data.progress.hasstarted = true;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand("Select * from stock where serialno != '' and isactive=true", conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Stock ostock = new Stock();
                            ostock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            ostock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                            ostock.runtimeprice = reader["runtimeprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["runtimeprice"]);
                            ostock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                            ostock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                            ostock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                            ostock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                            ostock.stockdate = reader["stockdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["stockdate"]);
                            ostock.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                            ostock.serialno = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString();
                            ostock.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                            ostock.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"].ToString());
                            ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                            ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                            ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                            ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                            ostock.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                            ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                            ostock.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                            ostock.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"].ToString());
                            ostock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                            ostock.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                            ostock.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                            ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                            ostock.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"].ToString());
                            ostock.transfered = reader["transfered"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transfered"].ToString());
                            ostock.damagedqty = reader["damagedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["damagedqty"].ToString());
                            ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            ostock.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            ostock.lrreference = reader["lrreference"] == DBNull.Value ? "" : reader["lrreference"].ToString();
                            ostock.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                            lst.Add(ostock);
                        }
                    }

                    conn.Close();
                }


                StockUANDataService stockUANDataService = new StockUANDataService() { Context = this.Context };
                SalablegoodsDataService salablegoodsdata = new SalablegoodsDataService() { Context = this.Context, isMigration = true };
                var alluanlist = lst.GroupBy(x => x.serialno);

                //alluanlist = alluanlist.ToList().Where(x => x.Count() > 1).ToList();

                data.progress.status = "Available UAN " + alluanlist.Count();

                if (alluanlist.Count() == 0)
                {
                    return;
                }
                data.progress.percentage = 50;
                var completeduan = 0;
                var totalcount = alluanlist.Count();
                var MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 1.0));
                //MaxDegreeOfParallelism = 1;
                System.Threading.Tasks.Parallel.ForEach(alluanlist, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, uan =>
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        var newstock = Newtonsoft.Json.JsonConvert.DeserializeObject<Stock>(Newtonsoft.Json.JsonConvert.SerializeObject(uan.OrderByDescending(x => x.id).First()));
                        var ids = uan.ToList().ConvertAll(x => x.id);
                        ids.Remove(newstock.id);
                        if (ids.Count > 0)
                            ExecuteQuery("update stock set isactive=false where id in (" + string.Join(",", ids) + ")", null, conn, transaction);


                        newstock.qty = uan.Sum(x => x.qty);
                        newstock.barcode = newstock.serialno;
                        newstock.serialno = "";
                        newstock.isuan = true;
                        newstock.transfered = uan.Sum(x => x.transfered);
                        newstock.damagedqty = uan.Sum(x => x.damagedqty);
                        newstock.journalqty = uan.Sum(x => x.journalqty);
                        newstock.purchaseqty = uan.Sum(x => x.purchaseqty);
                        newstock.returnqty = uan.Sum(x => x.returnqty);
                        newstock.buyingqty = uan.Sum(x => x.buyingqty);

                        Update(transaction, conn, newstock);

                        foreach (var stockuan in uan)
                        {
                            var newstockuan = stockuan.GetStockUAN();
                            newstockuan.stockid = newstock.id;
                            stockUANDataService.Insert(transaction, conn, newstockuan);
                        }

                        var slst = new List<Salablegoods>();
                        var command = GetCommand("Select * from salablegoods where stockid in (" + string.Join(",", uan.ToList().ConvertAll(x => x.id)) + ")  and isactive=true", conn);
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Salablegoods osalablegoods = new Salablegoods();
                                osalablegoods.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                                osalablegoods.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                                osalablegoods.received = reader["received"] == DBNull.Value ? false : Convert.ToBoolean(reader["received"]);
                                osalablegoods.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                                osalablegoods.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                                osalablegoods.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                                osalablegoods.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                                osalablegoods.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                                osalablegoods.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"].ToString());
                                osalablegoods.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"].ToString());
                                osalablegoods.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"].ToString());
                                osalablegoods.jqty = reader["jqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jqty"].ToString());
                                osalablegoods.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"].ToString());
                                osalablegoods.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());
                                osalablegoods.tiqty = reader["tiqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tiqty"].ToString());
                                osalablegoods.toqty = reader["toqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["toqty"].ToString());
                                osalablegoods.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                                osalablegoods.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                                osalablegoods.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                                osalablegoods.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                                osalablegoods.bundleid = reader["bundleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundleid"].ToString());
                                osalablegoods.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                                osalablegoods.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                                osalablegoods.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                                osalablegoods.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"].ToString());
                                osalablegoods.ispurchaselocation = reader["ispurchaselocation"] == DBNull.Value ? false : Convert.ToBoolean(reader["ispurchaselocation"]);
                                slst.Add(osalablegoods);
                            }
                        }


                        foreach (var uansalablegoods in slst.GroupBy(x => new { x.locationid, x.companyid }))
                        {
                            if (uansalablegoods.ToList().Exists(x => x.barcode == newstock.barcode))
                            {
                                continue;
                            }

                            var existsalablegoods = salablegoodsdata.Search(new SalablegoodsCreteria()
                            {
                                locationid = uansalablegoods.Key.locationid,
                                companyid = uansalablegoods.Key.companyid,
                                barcode = newstock.barcode,
                            });

                            if (existsalablegoods.Count > 0)
                            {
                                throw new Exception(newstock.barcode + " already exist on retail, can't upgrade");
                            }


                            ids = uansalablegoods.ToList().ConvertAll(x => x.id);
                            var newsalablegoods = Newtonsoft.Json.JsonConvert.DeserializeObject<Salablegoods>(Newtonsoft.Json.JsonConvert.SerializeObject(uansalablegoods.OrderByDescending(x => x.id).First()));
                            ids.Remove(newsalablegoods.id);
                            if (ids.Count > 0)
                                ExecuteQuery("update salablegoods set isactive=false where id in (" + string.Join(",", ids) + ")", null,
                                    conn, transaction);

                            newsalablegoods.iqty = uansalablegoods.Sum(x => x.iqty);
                            newsalablegoods.barcode = newstock.barcode;
                            newsalablegoods.stockid = newstock.id;
                            newsalablegoods.oqty = uansalablegoods.Sum(x => x.oqty);
                            newsalablegoods.tiqty = uansalablegoods.Sum(x => x.tiqty);
                            newsalablegoods.toqty = uansalablegoods.Sum(x => x.toqty);
                            newsalablegoods.dqty = uansalablegoods.Sum(x => x.dqty);
                            newsalablegoods.aqty = uansalablegoods.Sum(x => x.aqty);
                            newsalablegoods.jqty = uansalablegoods.Sum(x => x.jqty);
                            newsalablegoods.rqty = uansalablegoods.Sum(x => x.rqty);

                            if (!salablegoodsdata.Update(transaction, conn, newsalablegoods, null))
                            {

                                throw new Exception(newsalablegoods.barcode + " can't update on retail stock");
                            }


                            var slist = salablegoodsdata.Search(new SalablegoodsCreteria() { id = newsalablegoods.id }, conn, transaction);
                            var param = new SortedList<string, object>();
                            param.Add("salablegoodsid", newsalablegoods.id);
                            param.Add("stockid", newstock.id);
                            param.Add("barcode", newstock.barcode);
                            ExecuteQuery("update billitems set barcode=@barcode,salablegoodsid=@salablegoodsid,stockid=@stockid where salablegoodsid in (" + string.Join(",", uansalablegoods.ToList().ConvertAll(x => x.id)) + ")",
                                param, conn, transaction);
                        }

                        var tcompleteduan = System.Threading.Interlocked.Increment(ref completeduan);
                        data.progress.percentage = 50 + (tcompleteduan / totalcount * 100 / 2);

                        transaction.Commit();
                        conn.Close();

                        data.progress.status = totalcount.ToString() + "/" + tcompleteduan;
                    }
                });

                data.progress.status = totalcount.ToString() + "/" + totalcount.ToString();
                data.progress.hascompleted = true;
                data.progress.percentage = 100;
                data.progress.status = "Completed";
            }
            catch (Exception exp)
            {
                data.progress.
                    Exception = exp;
                data.progress.status = "Exception - " + exp.Message;
                if (exp.InnerException != null)
                    data.progress.status += "(" + exp.InnerException.Message + ")";
                data.progress.hascompleted = true;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

        }
        public Stock SubstituteUAN(NpgsqlTransaction transaction, NpgsqlConnection conn, Stock oStock)
        {
            var uan = oStock.serialno;
            var existinguan = Search(new StockCreteria() { barcode = oStock.serialno }, conn, transaction);
            StockUANDataService stockUANDataService = new StockUANDataService() { Context = this.Context };

            if (existinguan.Count > 0 && existinguan[0].isactive == false)
            {
                var param = new SortedList<string, object>();
                param.Add("id", existinguan[0].id);
                ExecuteQuery("update stock set barcode = concat('~', barcode) where id=@id", param, conn, transaction);
                existinguan.Clear();
            }

            if (existinguan.Count == 0)
            {
                var stockuan = oStock.GetStockUAN();
                oStock.barcode = oStock.serialno;
                oStock.serialno = "";
                oStock.isuan = true;
                oStock.createdby = Context.UserID;
                oStock.modifiedby = Context.UserID;
                oStock = InsertInternal(transaction, conn, oStock);

                stockuan.stockid = oStock.id;
                stockuan.createdby = Context.UserID;
                stockuan.modifiedby = Context.UserID;
                stockUANDataService.Insert(transaction, conn, stockuan);

            }
            else
            {
                existinguan[0].qty += oStock.qty;
                var param = new SortedList<string, object>();
                param.Add("qty", existinguan[0].qty);
                param.Add("id", existinguan[0].id);
                param.Add("ismultiprice", (oStock.price == existinguan[0].price ? false : true));
                ExecuteQuery("update stock set qty=@qty, ismultiprice=@ismultiprice where id=@id", param, conn, transaction);
                if (oStock.price != existinguan[0].price)
                {
                    SalablegoodspriceDataService salablegoodspriceDataService = new SalablegoodspriceDataService() { Context = this.Context };
                    salablegoodspriceDataService.Insert(transaction, conn, new Salablegoodsprice()
                    {
                        createdby = Context.UserID,
                        createdon = DateTime.Now,
                        addon = DateTime.Now,
                        isactive = true,
                        invoiceitemid = oStock.invoiceitemid,
                        price = oStock.price,
                        barcode = oStock.barcode,
                        mrp = oStock.displaymrp,
                        approvedby = Context.UserID
                    });
                }

                var stockuan = oStock.GetStockUAN();
                oStock.barcode = oStock.serialno;
                oStock.serialno = "";
                stockuan.stockid = existinguan[0].id;
                stockuan.createdby = Context.UserID;
                stockuan.modifiedby = Context.UserID;
                existinguan[0].uansubstiated = true;
                stockUANDataService.Insert(transaction, conn, stockuan);
                return existinguan[0];
            }
            return oStock;
        }
        public bool IsMigration { get; set; }
        public Stock Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Stock oStock)
        {
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany && oStock.companyid <= 0)
                throw new Exception("Please select the company");



            //if (oStock.invoiceitemid <= 0)
            //    throw new Exception("Stock can't update with zero invoice item id - " + oStock.barcode);

            if (!IsMigration && oStock.productid <= 0)
            {
                throw new Exception("Stock can't create without product information - " + oStock.barcode);
            }
            if (!IsMigration && oStock.buyingprice <= 0 && oStock.invoiceitemid > 0)
                throw new Exception("Stock can't create with zero buying price - " + oStock.barcode);

            if (oStock.lrreference != null && oStock.lrreference.Length > 20)
                oStock.lrreference = oStock.lrreference.Substring(0, 20);

            if (oStock.designid != null && oStock.designid.Length > 25)
                oStock.designid = oStock.designid.Substring(0, 25);

            if (oStock.hsncode != null && oStock.hsncode.Length > 15)
                oStock.hsncode = oStock.hsncode.Substring(0, 15);

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
            {
                if (!string.IsNullOrEmpty(oStock.serialno))
                {
                    oStock = SubstituteUAN(transaction, conn, oStock);
                    if (oStock.id != 0)
                        return oStock;
                }
            }
            return InsertInternal(transaction, conn, oStock);
        }
        public Stock InsertInternal(NpgsqlTransaction transaction, NpgsqlConnection conn, Stock oStock)
        {
                        if(string.IsNullOrEmpty(oStock.serialno))
            {
                if(Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.appmode == 1 )
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("UAN/SERIALNO MISSING", WebAPI.Common.Logger.LogType.Info,
                        "StockDataservice.Insert - Statck Trace - "+Environment.StackTrace,oStock,Context.UserID,0);
                    
                    throw new Exception("UAN / GLN / SERIALNO - Missing, contact support team");
                }
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                string.IsNullOrEmpty(oStock.hsncode))
            {
                throw new Exception("HSN Can't be empty for " + oStock.serialno + "(" + oStock.barcode + ")");
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                string.IsNullOrEmpty(oStock.serialno))
            {
                throw new Exception("UAN Can't be empty for " + oStock.barcode);
            }

            if (!IsMigration && oStock.barcode == oStock.serialno)
                throw new Exception("Stock Quanto Barcode and Serialno are the same, please check - " + oStock.barcode);
            else if (IsMigration && oStock.barcode == oStock.serialno)
                oStock.serialno = "";

            if (oStock.price == 0)
                throw new Exception("Price Can't be zero value");

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isactive;
            command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isbit;
            command.Parameters.Add("runtimeprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.runtimeprice;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.retaillocationid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.locationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.supplierid;
            command.Parameters.Add("srcsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.srcsupplierid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStock.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.printingname == null ? "" : oStock.printingname;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.imageid == null ? "" : oStock.imageid;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.price;
            command.Parameters.Add("purchaseqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.purchaseqty;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.barcode == null ? "" : oStock.barcode;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.hsncode == null ? "" : oStock.hsncode;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.invoiceitemid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.companyid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.productid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.additionaldiscount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.qty;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.damagedqty;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.journalqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.returnqty;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.transfered;
            command.Parameters.Add("buyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.buyingprice;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.dealerprice;
            command.Parameters.Add("eprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.eprice;
            command.Parameters.Add("ibuyingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.ibuyingprice;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.designid == null ? "" : oStock.designid;
            command.Parameters.Add("lrreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.lrreference == null ? "" : oStock.lrreference;
            command.Parameters.Add("stockdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStock.stockdate;

            command.Parameters.Add("displayinvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStock.displayinvoicedate;
            command.Parameters.Add("displayinvoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.displayinvoiceno == null ? "" : oStock.displayinvoiceno;
            command.Parameters.Add("displaymrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.displaymrp;

            command.Parameters.Add("expiry", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStock.expiry;
            command.Parameters.Add("batch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.batch == null ? "" : oStock.batch;
            command.Parameters.Add("serialno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.serialno == null ? "" : oStock.serialno;

            command.Parameters.Add("isuan", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isuan;
            command.Parameters.Add("ismultiprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.ismultiprice;
            command.Parameters.Add("pricetagid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.pricetagid;
            //if(oStock.invoicedate == DateTime.MinValue)
            //    command.Parameters.Add("minvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DBNull.Value;
            //else
            //    command.Parameters.Add("minvoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStock.invoicedate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStock.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStock;
        }

        public Stock Update(Stock oStock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oStock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }

            return oStock;
        }

        public Stock Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Stock oStock)
        {

            if (oStock.productid <= 0)
                throw new Exception("Stock can't update without product information - " + oStock.barcode);

            if (!IsMigration && oStock.buyingprice <= 0 && oStock.invoiceitemid > 0)
                throw new Exception("Stock can't updated with zero buying price - " + oStock.barcode);

            //if (oStock.invoiceitemid <= 0)
            //    throw new Exception("Stock can't update with zero invoice item id - " + oStock.barcode);

            if (!IsMigration && oStock.barcode == oStock.serialno)
                throw new Exception("Stock Quanto Barcode and Serialno are the same, please check - " + oStock.barcode);

            else if (IsMigration && oStock.barcode == oStock.serialno)
                oStock.serialno = "";
            if (oStock.lrreference != null && oStock.lrreference.Length > 20)
                oStock.lrreference = oStock.lrreference.Substring(0, 20);

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                string.IsNullOrEmpty(oStock.serialno))
            {
                throw new Exception("UAN Can't be empty for " + oStock.barcode);
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                string.IsNullOrEmpty(oStock.hsncode))
            {
                throw new Exception("HSN Can't be empty for " + oStock.serialno + "(" + oStock.barcode + ")");
            }

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isactive;
            command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isbit;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStock.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStock.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.printingname == null ? "" : oStock.printingname;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.price;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.dealerprice;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.barcode == null ? "" : oStock.barcode;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.productid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.discount;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.companyid;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.additionaldiscount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.qty;
            command.Parameters.Add("damagedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.damagedqty;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.journalqty;
            command.Parameters.Add("transfered", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.transfered;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.hsncode == null ? "" : oStock.hsncode;
            command.Parameters.Add("buyingprice ", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStock.buyingprice;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.designid == null ? "" : oStock.designid;
            command.Parameters.Add("lrreference", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStock.lrreference == null ? "" : oStock.lrreference;
            command.Parameters.Add("pricetagid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.pricetagid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStock.version = Convert.ToInt32(reader[0]);
                }
            }

            return oStock;
        }

        public bool HasBarcodes(long[] items)
        {
            using (var reader = ExecuteReader("select sum(sellableqty) from lrinvoiceitems where id in (" + string.Join(",", items) + ") and isactive=true", null))
            {
                if (reader.Read())
                    return !reader.IsDBNull(0) && reader.GetDecimal(0) > 0;
            }
            return false;
        }
        internal List<Lrinvoiceitemsbarcode> CreateBarcodefromList(List<Lrinvoiceitemsbarcode> items)
        {

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                items = CreateBarcode(items, transaction, conn);
                transaction.Commit();
            }
            return items;
        }

        internal List<Lrinvoiceitemsbarcode> CreateBarcode(List<Lrinvoiceitemsbarcode> items, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {

            var uanmissmatches = items.FindAll(x => (!string.IsNullOrEmpty(x.uan) &&
                x.Details != null &&
                x.Details.serialno != x.uan));
            if (uanmissmatches.Count > 0)
            {
                LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                lrinvoiceitemsDataService.SyncUANSerial(conn, transaction, uanmissmatches);
            }

            bool isintercompanytransfer = false;
            if (items != null && items.Count > 0 && items[0].lrinvoiceid > 0)
            {
                using (var reader = ExecuteReader("select isintercompanytransfer from lrinvoice where id=" + items[0].lrinvoiceid + " and isactive=true", null))
                {
                    if (reader.Read())
                        isintercompanytransfer = reader["isintercompanytransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["isintercompanytransfer"]);
                }
            }

            if (isintercompanytransfer)
                throw new Exception("Use the existing Quanto Barcode for Inter company Transfer.");

            bool updatebarcodedate = false;
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_barcodedateaspurchasedate)
            {
                //updatebarcodedate = HasBarcodes(items.ConvertAll(e => e.id).ToArray());
                updatebarcodedate = true;
            }
            List<Lrinvoiceitemsbarcode> ret = new List<Lrinvoiceitemsbarcode>();

            var productids = items.ConvertAll(e => e.productid).Distinct().ToList();
            ProductsDataService dataService = new ProductsDataService() { Context = this.Context };
            dataService.Context = this.Context;
            var products = dataService.Search(new ProductsCreteria()
            {
                ids = productids
            });
            var taxid = products.ConvertAll(e => e.salestaxid).Distinct().ToList();
            TaxDataService taxdataService = new TaxDataService() { Context = this.Context };
            taxdataService.Context = this.Context;
            var taxlist = taxdataService.Search(new TaxCreteria()
            {
                ids = taxid
            });

            long _startbarcodeno = 0;
            foreach (var item in items.OrderBy(e => e.id))
            {
                if ((item.buyingqty + item.freeqty - (item.damagedqty + item.sellableqty)) < item.newbarcode || item.newbarcode == 0)
                {
                    if ((item.sellableqty != 0 && item.buyingqty >= 1) || item.newbarcode == 0)
                        continue;
                }
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportpricingflow &&
                    item.buyingprice > item.sellingprice)
                    throw new Exception("Can't Generate Barcode! Please check Selling Price.");

                var product = products.Find(e => e.id == item.productid);
                if (product == null)
                    throw new Exception("Cant find the product - "+ item.printingname);

                var tax = taxlist.Find(e => e.id == product.salestaxid);
                ret.Add(CreateBarcode(item, product, tax, conn, transaction));

                // To avoid barcode sequence missing 
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_maintainbarcodeseq)
                {
                    if (_startbarcodeno > 0 && (item.endbarcodeno - _startbarcodeno) != item.newbarcode)
                        throw new Exception("Barcode sequence Missing, try again.");
                    else
                        _startbarcodeno = item.endbarcodeno;
                }
            }
            if (updatebarcodedate && ret != null && ret.Exists(e => e != null))
            {
                var param = new SortedList<string, object>();
                param.Add("entrydate", DateTimeNow.Date);
                param.Add("invoiceitemid", items.First().id);
                using (var reader = ExecuteReader("select lrinvoiceid from lrinvoiceitems where id=@invoiceitemid", param))
                {
                    if (reader.Read())
                        param.Add("id", reader.GetInt64(0));
                }
                if (param.ContainsKey("id"))
                    ExecuteQuery("Update lrinvoice set entrydate=@entrydate where id=@id", param);
            }

            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
            stocktransactionDataService.CreateBarcode(items, transaction, conn);
            return ret;
        }


        internal Lrinvoiceitemsbarcode CreateSingleBarcode(Lrinvoiceitemsbarcode item)
        {
            bool isintercompanytransfer = false;
            if (item != null && item.lrinvoiceid > 0)
            {
                using (var reader = ExecuteReader("select isintercompanytransfer from lrinvoice where id=" + item.lrinvoiceid + " and isactive=true", null))
                {
                    if (reader.Read())
                        isintercompanytransfer = reader["isintercompanytransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["isintercompanytransfer"]);
                }
            }

            if (isintercompanytransfer)
                throw new Exception("Use the existing Quanto Barcode for Inter company Transfer.");

            List<Lrinvoiceitemsbarcode> ret = new List<Lrinvoiceitemsbarcode>();
            ret.Add(item);
            ret = CreateBarcodefromList(ret);
            return ret.First();
        }

        internal Lrinvoiceitemsbarcode CreateMobileBarcode(Lrinvoiceitemsbarcode item, ProductsBase product, Tax tax)
        {
            var barcodelist = item.barcodeList;
            var sellableqty = barcodelist == null ? 0 : barcodelist.Count;
            if (barcodelist.Count == item.sellableqty)
            {
                throw new Exception("Upload barcode before generating.");
            }

            ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
            itemservice.Context = this.Context;
            var iteminfo = itemservice.Search(new ItemsCreteria()
            {
                brandid = item.Details.brandid,
                colourid = item.Details.colourid,
                materialid = item.Details.materialid,
                patternid = item.Details.patternid,
                typeid = item.Details.typeid,
                sizeid = item.Details.sizeid,
                styleid = item.Details.styleid,
                fitid = item.Details.fitid,
                sleeveid = item.Details.sleeveid,
                productid = item.productid,
                agmtcode = item.itbuscode,
                productmap = true
            });
            if (iteminfo.Count == 1)
            {
                item.itemid = iteminfo[0].id;
            }
            else
            {
                var newitem = item.Details;
                newitem.isactive = true;
                newitem.salemode = 1;
                newitem.version = 1;
                newitem = itemservice.Insert(newitem);
                item.itemid = newitem.id;
            }

            var existingstocks = Search(item.companyid, barcodelist);
            if (existingstocks.Exists(e => e.invoiceitemid != item.id))
            {
                throw new Exception("One of barocde " + existingstocks.Find(e => e.invoiceitemid != item.id).barcode + " Already associated with different invoice");
            }
            item.stock = new List<Stock>();
            barcodelist.RemoveAll(e => existingstocks.Exists(x => x.barcode == e));
            if (barcodelist.Count > 0)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    long supplierid = 0;
                    long sourcesupplierid = 0;
                    decimal buyingprice = 0;
                    decimal ibuyingprice = 0;
                    decimal dealerprice = 0;
                    decimal mrp = 0;
                    string lrreference = "";
                    string designid = "";
                    string imgid = "";
                    long locationid = 0;
                    long retaillocationid = 0;
                    Items itemdetail = null;
                    Products.sellingMode sellingmode = Products.sellingMode.None;
                    NpgsqlCommand command = GetCommand(SQL_SearchSupplierID, conn);
                    command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                            sourcesupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"]);
                            retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                            locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                            ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                            dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                            mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                            decimal sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"].ToString());
                            string lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                            designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                            imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                            lrreference = string.Format("{0}.{1}", lrentryno, sno);
                            sellingmode = reader["sellingmode"] == DBNull.Value ? Products.sellingMode.None : (Products.sellingMode)reader["sellingmode"];
                            if (reader["itemdetail"] != DBNull.Value &&
                                !string.IsNullOrEmpty(reader["itemdetail"].ToString()) &&
                                reader["itemdetail"].ToString() != "null")
                            {
                                itemdetail = Newtonsoft.Json.JsonConvert.DeserializeObject<Items>(reader["itemdetail"].ToString());
                            }
                        }
                    }
                    foreach (var barcode in barcodelist)
                    {
                        var stck = item.CreateStock(barcode);
                        stck.printingname = item.printingname;
                        if (sellingmode != Products.sellingMode.Piece)
                            stck.qty = item.buyingqty + item.freeqty - (item.damagedqty + item.sellableqty);
                        if (retaillocationid == 0)
                            stck.purchaseqty = stck.qty;
                        item.stock.Add(stck);
                    }
                    SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                    StockUANDataService stockUANDataService = new StockUANDataService() { Context = this.Context };
                    var transaction = conn.BeginTransaction();
                    foreach (var stock in item.stock)
                    {
                        stock.supplierid = supplierid;
                        stock.buyingprice = buyingprice;
                        stock.ibuyingprice = ibuyingprice;
                        stock.lrreference = lrreference;
                        stock.designid = designid;
                        stock.stockdate = DateTimeNow;
                        stock.locationid = locationid;
                        stock.displaymrp = mrp;
                        stock.dealerprice = dealerprice;
                        stock.eprice = (stock.price - stock.discount) / (tax.taxpercentage + 100) * 100;
                        stock.imageid = imgid;
                        if (stock.retaillocationid <= 0)
                            stock.retaillocationid = retaillocationid;
                        if (itemdetail != null)
                        {
                            stock.batch = itemdetail.batch;
                            stock.expiry = itemdetail.expiry;
                            stock.serialno = itemdetail.serialno;
                        }
                        var newstock = Insert(transaction, conn, stock);
                        if (retaillocationid > 0)
                        {
                            var existingsalableitem = salablegoodsDataService.Search(new SalablegoodsCreteria()
                            {
                                barcode = newstock.barcode,
                                locationid = retaillocationid
                            });

                            if (existingsalableitem.Count == 0)
                            {
                                Salablegoods sitem = new Salablegoods();
                                sitem.inwardon = DateTimeNow;
                                sitem.barcode = newstock.barcode;
                                sitem.bundleid = -5;
                                sitem.iqty = newstock.qty;
                                sitem.isactive = true;
                                sitem.itemid = newstock.itemid;
                                sitem.locationid = retaillocationid;
                                sitem.stockid = newstock.id;
                                sitem.version = 1;
                                sitem.companyid = newstock.companyid;
                                sitem.supplierid = newstock.supplierid;
                                sitem.received = true;
                                sitem.price = newstock.price;
                                sitem.discount = newstock.discount;
                                sitem.additionaldiscount = newstock.additionaldiscount;
                                sitem.ispurchaselocation = true;
                                sitem.pqty = newstock.qty;
                                salablegoodsDataService.Insert(transaction, conn, sitem);

                                //command = GetCommand(@"update newstock set transfered=transfered+" + newstock.qty + " where id=" + newstock.id, conn, transaction);
                                //command.ExecuteNonQuery();
                            }
                            else if (newstock.uansubstiated)
                            {
                                command = GetCommand(@"update stock set transfered=transfered+" + stock.qty + " where id=" + newstock.id, conn, transaction);
                                command.ExecuteNonQuery();
                                command = GetCommand(@"update salablegoods set iqty=iqty+" + stock.qty + " where id=" + existingsalableitem[0].id, conn, transaction);
                                command.ExecuteNonQuery();

                                if (newstock.price != stock.price)
                                {
                                    command = GetCommand(@"update stock set ismultiprice=true where id=" + newstock.id, conn, transaction);
                                    command.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                if (existingsalableitem[0].ispurchaselocation)
                                    existingsalableitem[0].pqty += stock.qty;
                                existingsalableitem[0].iqty += stock.qty;
                                salablegoodsDataService.Update(transaction, conn, existingsalableitem[0], existingsalableitem[0]);
                                //command = GetCommand(@"update newstock set transfered=transfered+" + newstock.qty + " where id=" + newstock.id, conn, transaction);
                                //command.ExecuteNonQuery();
                            }

                            if (stock.isuan)
                            {
                                stockUANDataService.Transfer2Retail(transaction, conn, item, newstock, newstock.qty);
                            }
                        }
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                    {
                        List<string> externalcodes = new List<string>();

                        if (item.Details != null && !string.IsNullOrEmpty(item.Details.serialno))
                        {
                            externalcodes.AddRange(item.Details.serialno.Split(','));
                        }

                        if (item.Details != null && !string.IsNullOrEmpty(item.Details.gln))
                        {
                            externalcodes.Add(item.Details.gln);
                        }

                        SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };

                        foreach (var barcode in externalcodes)
                        {
                            if (string.IsNullOrEmpty(barcode)) continue;
                            foreach (var istock in item.stock)
                            {
                                Sellablebarcodes sellablebarcodes = new Sellablebarcodes();
                                sellablebarcodes.barcode = barcode;
                                sellablebarcodes.stockid = istock.id;
                                sellablebarcodes.itemid = istock.itemid;
                                sellablebarcodes.productid = istock.productid;
                                sellablebarcodes.isactive = true;
                                sellablebarcodes.companyid = istock.companyid;
                                sellablebarcodes.createdby = istock.createdby;
                                sellablebarcodes.createdon = istock.createdon;
                                sellablebarcodes.modifiedby = istock.modifiedby;
                                sellablebarcodes.modifiedon = istock.modifiedon;
                                sellablebarcodes.version = istock.version;
                                if (!string.IsNullOrEmpty(item.Details.batch))
                                    sellablebarcodes.batchandexpiry = string.Format("{0}/{1}", item.Details.batch, item.Details.expiry.ToString("MMM-yyyy"));
                                sellablebarcodesDataService.Insert(transaction, conn, sellablebarcodes);
                            }
                        }
                    }

                    InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(item.lrid, Inbox.StatusList.Barcode_Generating, transaction, conn, Context);
                    command = GetCommand("Update lrinvoiceitems set sellableqty=@sellableqty where id=@id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = sellableqty;
                    command.ExecuteNonQuery();
                    item.sellableqty = sellableqty;
                    transaction.Commit();
                    conn.Close();
                }
            }
            else
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var command = GetCommand("Update lrinvoiceitems set sellableqty=@sellableqty where id=@id", conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = sellableqty;
                    command.ExecuteNonQuery();
                    item.sellableqty = sellableqty;
                    conn.Close();
                }
            }
            return item;
        }

        //    internal Lrinvoiceitemsbarcode CreateBarcode(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoiceitemsbarcode item,
        //ProductsBase product, Tax tax)
        //    {
        //        NpgsqlTransaction transaction = null;
        //        Lrinvoiceitemsbarcode retvalue = null;
        //        try
        //        {
        //            if (product.producttype == 6)
        //            {
        //                return CreateMobileBarcode(item, product, tax);
        //            }
        //            using (NpgsqlConnection conn = CreateConnection())
        //            {
        //                conn.Open();
        //                transaction = conn.BeginTransaction();
        //                retvalue = CreateBarcode(item, product, tax, conn, transaction);

        //                // To avoid barcode sequence missing 
        //                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_maintainbarcodeseq)
        //                {
        //                    if ((item.endbarcodeno - item.startbarcodeno) != (item.newbarcode - 1))
        //                        throw new Exception("Barcode sequence Missing, try again.");
        //                }
        //                transaction.Commit();
        //                conn.Close();
        //            }
        //        }
        //        catch (Exception exp)
        //        {

        //            if (transaction != null)
        //            {
        //                if (transaction.Connection != null && transaction.Connection.State == System.Data.ConnectionState.Open)
        //                    transaction.Connection.Close();
        //            }
        //            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
        //            throw exp;
        //        }
        //        return retvalue;
        //    }
        internal Lrinvoiceitemsbarcode CreateBarcode(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoiceitemsbarcode item,
            ProductsBase product, Tax tax)
        {
            Lrinvoiceitemsbarcode retvalue = null;
            try
            {
                if (product.producttype == 6)
                {
                    return CreateMobileBarcode(item, product, tax);
                }

                retvalue = CreateBarcode(item, product, tax, conn, transaction);

                // To avoid barcode sequence missing 
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_maintainbarcodeseq)
                {
                    if ((item.endbarcodeno - item.startbarcodeno) != (item.newbarcode - 1))
                        throw new Exception("Barcode sequence Missing, try again.");
                }

            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return retvalue;
        }
        public Lrinvoiceitemsbarcode CreateBarcode(Lrinvoiceitemsbarcode item, ProductsBase product,
            Tax tax, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            return CreateBarcode(item, product, tax, true, conn, transaction);
        }
        public Lrinvoiceitemsbarcode CreateBarcode(Lrinvoiceitemsbarcode item, ProductsBase product,
            Tax tax, bool autotransferretail, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            long _startbarcodeno = 0;
            long _endbarcodeno = 0;
            int noofbarcodetry = 0;

            try
            {
                long _lrid = item.lrid;
                if (((item.actualqty > item.buyingqty ? item.actualqty : item.buyingqty) + item.freeqty - (item.sellableqty + item.damagedqty + ((item.actualqty > 0 && item.actualqty<item.buyingqty) ? item.buyingqty - item.actualqty : 0))) < item.newbarcode || item.newbarcode == 0)
                {
                    if ((item.sellableqty != 0 && item.buyingqty >= 1) || item.newbarcode == 0)
                        return null;
                }

                if (!item.isQuantoImport || item.itemid == 0)
                {
                    ItemsDataService itemservice = new ItemsDataService() { Context = this.Context };
                    itemservice.Context = this.Context;
                    var iteminfo = itemservice.Search(new ItemsCreteria()
                    {
                        brandid = item.Details.brandid,
                        colourid = item.Details.colourid,
                        materialid = item.Details.materialid,
                        patternid = item.Details.patternid,
                        typeid = item.Details.typeid,
                        sizeid = item.Details.sizeid,
                        styleid = item.Details.styleid,
                        sleeveid = item.Details.sleeveid,
                        fitid = item.Details.fitid,
                        productid = item.productid,
                        agmtcode = item.itbuscode,
                        mrp = item.mrp,
                        gln = string.IsNullOrWhiteSpace(item.uan) ? item.gln : item.uan,
                        productmap = true
                    }, null, conn, transaction);
                    if (iteminfo.Count > 0)
                    {
                        item.itemid = iteminfo[0].id;
                    }
                    else
                    {
                        var newitem = item.Details;
                        newitem.isactive = true;
                        newitem.salemode = 1;
                        newitem.version = 1;
                        if(newitem.mrp == 0 && item.mrp>0)
                            newitem.mrp = item.mrp;
                        newitem.agmtcode = item.itbuscode;
                        newitem.gln = string.IsNullOrWhiteSpace(item.uan) ? item.gln : item.uan;
                        newitem = itemservice.Insert(transaction, conn, newitem);
                        item.itemid = newitem.id;
                    }
                }


                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                salablegoodsDataService.Context = this.Context;

                //bool isbase36 = (products.Count == 1 && products[0].barcodemode == 1);
                var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(Context.OrganizationID).Get(product.barcodemode, item.companyid);
                SequenceNo sequence = null;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_locationbasedbarcodes && item.stocklocationid > 0)
                {
                    sequence = barcodeConfig.GetSequenceNo(item.stocklocationid);
                    if (!sequence.Validate(this.Context, "Location Based Barcode", false))
                    {
                        sequence = barcodeConfig.GetSequenceNo();
                    }
                }
                else
                {
                    sequence = barcodeConfig.GetSequenceNo();
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_monthlybarcodes)
                {
                    if (sequence.modifiedon.ToString("MMyyyy") != DateTime.Now.ToString("MMyyyy"))
                    {
                        sequence.ResetNumber(conn, transaction, DateTimeNow);
                    }
                }

                long supplierid = 0;
                long sourcesupplierid = 0;
                decimal buyingprice = 0;
                decimal ibuyingprice = 0;
                decimal dealerprice = 0;
                decimal mrp = 0;
                string lrreference = "";
                string designid = "";
                string imgid = "";
                long locationid = 0;
                long retaillocationid = 0;
                Items itemdetail = null;
                Products.sellingMode sellingmode = Products.sellingMode.None;
                NpgsqlCommand command = GetCommand(SQL_SearchSupplierID, conn, transaction);
                command.Parameters.Add("lrinvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        sourcesupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"]);
                        retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"]);
                        locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"].ToString());
                        dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"].ToString());
                        mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"].ToString());
                        decimal sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sno"].ToString());
                        string lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        imgid = reader["imgid"] == DBNull.Value ? "" : reader["imgid"].ToString();
                        lrreference = string.Format("{0}.{1}", lrentryno, sno);
                        sellingmode = reader["sellingmode"] == DBNull.Value ? Products.sellingMode.None : (Products.sellingMode)reader["sellingmode"];
                        if (reader["itemdetail"] != DBNull.Value &&
                            !string.IsNullOrEmpty(reader["itemdetail"].ToString()) &&
                            reader["itemdetail"].ToString() != "null")
                        {
                            itemdetail = Newtonsoft.Json.JsonConvert.DeserializeObject<Items>(reader["itemdetail"].ToString());
                        }
                    }
                }


                if (item.barcodes == "NOT GENERATED")
                {
                    item.barcodes = "null";
                }

                if (!string.IsNullOrEmpty(item.barcodes) && item.barcodes != "null" && sellingmode != Products.sellingMode.Piece)
                {
                    bool countmissmatch = true;
                    int retry = 0;
                    while (retry < 3 && countmissmatch)
                    {
                        string query = @"update public.lrinvoiceitems set version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,sellableqty=sellableqty+@sellableqty 
                                             where id=@id and version=@version and (lrinvoiceitems.buyingqty - (lrinvoiceitems.damagedqty+lrinvoiceitems.sellableqty))>=@sellableqty RETURNING version,sellableqty";
                        command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.newbarcode;

                        using (var dr = command.ExecuteReader(System.Data.CommandBehavior.SingleRow))
                        {
                            if (dr.Read())
                            {
                                item.version = dr.GetInt32(0);
                                item.sellableqty = dr.GetDecimal(1);
                                countmissmatch = false;
                                break;
                            }
                        }

                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception(GetSQL(command), new Exception()), WebAPI.Common.Logger.LogType.Error, item);
                        retry++;
                        restStockMissmatch(item.id, transaction, conn);
                    }
                    if (countmissmatch)
                    {
                        item = null;
                        throw new Exception("Can't create barcode,try again");
                    }
                }
                else
                {
                    long initialSlot = 0;
                    while (true)
                    {
                        var nextNumber = sequence.GetNextNumber(this.Context, conn, transaction);
                        initialSlot = sequence.CurrentSlotID;
                        long lastSlot = initialSlot + (item.newbarcode - 1);
                        int tryCount = 3;

                        if (barcodeConfig.Type == BarcodeManager.BarcodeType.PACKSERIAL)
                        {
                            lastSlot = initialSlot;
                        }
                        _startbarcodeno = initialSlot;
                        _endbarcodeno = lastSlot;
                        if (item.newbarcode > 1)
                        {
                            while (!sequence.SentNextNumber(initialSlot, 0, lastSlot, conn, transaction) &&
                                tryCount > 0)
                            {
                                try
                                {
                                    nextNumber = sequence.GetNextNumber(this.Context, conn, transaction);
                                    initialSlot = sequence.CurrentSlotID - 1;
                                    lastSlot = initialSlot + (item.newbarcode - 1);
                                }
                                catch (Exception exp)
                                {
                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                                    throw exp;
                                }
                            }
                        }
                        List<string> barcodelist = new List<string>();

                        if (barcodeConfig.Type == BarcodeManager.BarcodeType.PACKSERIAL)
                        {
                            for (int i = 1; i <= item.newbarcode; i++)
                            {
                                var barcode = string.Format("{0}/{1}", initialSlot, i);
                                barcodelist.Add("'" + barcode + "'");
                            }
                        }
                        else
                        {
                            var tInitialSlot = initialSlot;
                            for (int i = 0; i < item.newbarcode; i++)
                            {
                                int barcodeno = (int)tInitialSlot++;
                                barcodelist.Add("'" + barcodeConfig.GetSpecialValue(Context, sequence, barcodeno, item) + "'");
                            }
                        }
                        command = GetCommand(string.Format(SQL_ValidateBarcode, string.Join(",", barcodelist)), conn, transaction);
                        barcodelist.Clear();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                barcodelist.Add(reader.GetString(0));

                            if (reader.NextResult())
                                while (reader.Read())
                                    barcodelist.Add(reader.GetString(0));
                        }
                        if (barcodelist.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            noofbarcodetry++;
                            if (noofbarcodetry == 10)
                            {
                                if (barcodeConfig.Type != BarcodeManager.BarcodeType.PACKSERIAL)
                                {
                                    string lastbarcode = "";

                                    if (!string.IsNullOrEmpty(sequence.prefix) &&
                                        string.IsNullOrEmpty(sequence.Suffix))
                                    {
                                        var param = new SortedList<string, object>();
                                        param.Add("startswith", sequence.prefix);
                                        using (var reader = ExecuteReader(@"select barcode from stock where starts_with(barcode,@startswith) ORDER BY ID DESC LIMIT 1",
                                            param, conn, transaction))
                                        {
                                            if (reader.Read())
                                            {
                                                lastbarcode = reader.GetString(0);
                                                if (lastbarcode.Length > sequence.prefix.Length)
                                                    lastbarcode = lastbarcode.Substring(sequence.prefix.Length);
                                            }
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(sequence.prefix) &&
                                        !string.IsNullOrEmpty(sequence.Suffix))
                                    {
                                        var param = new SortedList<string, object>();
                                        param.Add("endswith", "%" + sequence.Suffix);
                                        using (var reader = ExecuteReader(@"select barcode from stock where barcode ilike @endswith ORDER BY ID DESC LIMIT 1",
                                            param, conn, transaction))
                                        {
                                            if (reader.Read())
                                            {
                                                lastbarcode = reader.GetString(0);
                                                if (lastbarcode.Length - sequence.Suffix.Length > 0)
                                                    lastbarcode = lastbarcode.Substring(0, lastbarcode.Length - sequence.Suffix.Length);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var param = new SortedList<string, object>();
                                        param.Add("startswith", sequence.prefix);
                                        param.Add("endswith", "%" + sequence.Suffix);
                                        using (var reader = ExecuteReader(@"select barcode from stock where starts_with(barcode,@startswith) and barcode ilike @endswith ORDER BY ID DESC LIMIT 1",
                                            param, conn, transaction))
                                        {
                                            if (reader.Read())
                                            {
                                                lastbarcode = reader.GetString(0);
                                                if (lastbarcode.Length > sequence.prefix.Length)
                                                    lastbarcode = lastbarcode.Substring(sequence.prefix.Length);
                                                if (lastbarcode.Length - sequence.Suffix.Length > 0)
                                                    lastbarcode = lastbarcode.Substring(0, lastbarcode.Length - sequence.Suffix.Length);
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(lastbarcode))
                                    {
                                        if (long.TryParse(lastbarcode, out initialSlot))
                                        {
                                            var tInitialSlot = ++initialSlot;
                                            barcodelist.Clear();
                                            for (int i = 0; i < item.newbarcode; i++)
                                            {
                                                int barcodeno = (int)tInitialSlot++;
                                                barcodelist.Add("'" + barcodeConfig.GetSpecialValue(Context, sequence, barcodeno, item) + "'");
                                            }

                                            command = GetCommand(string.Format(SQL_ValidateBarcode, string.Join(",", barcodelist)), conn, transaction);
                                            barcodelist.Clear();
                                            using (var reader = command.ExecuteReader())
                                            {
                                                while (reader.Read())
                                                    barcodelist.Add(reader.GetString(0));

                                                if (reader.NextResult())
                                                    while (reader.Read())
                                                        barcodelist.Add(reader.GetString(0));
                                            }
                                            if (barcodelist.Count > 0)
                                            {
                                                throw new Exception("Please reset the auto number configuration for " + 
                                                    sequence.Description + "("+ sequence.Type+")" + " - " + sequence.CurrentNo);
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Please reset the auto number configuration for " + sequence.Description + "(" + sequence.Type + ")" + " - " + sequence.CurrentNo);
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("Please reset the auto number configuration for " + sequence.Description + "(" + sequence.Type + ")" + " - " + sequence.CurrentNo);
                                    }
                                }
                                else
                                {
                                    throw new Exception("Please reset the auto number configuration for " + sequence.Description + "(" + sequence.Type + ")" + " - " + sequence.CurrentNo);
                                }
                            }
                            else
                            {
                                WebAPI.Common.Logger.Instance(Context.OrganizationID).
                                    Log(new Exception("Duplicate barcodes"),
                                    WebAPI.Common.Logger.LogType.Info, barcodelist);
                            }
                        }
                    }

                    item.stock = new List<Stock>();
                    for (int i = 0; i < item.newbarcode; i++)
                    {
                        Stock stck = null;
                        if (barcodeConfig.Type == BarcodeManager.BarcodeType.PACKSERIAL)
                        {
                            var barcode = string.Format("{0}/{1}", initialSlot, i + 1);
                            stck = item.CreateStock(barcode);
                        }
                        else
                        {
                            int barcodeno = (int)initialSlot++;
                            string barcode = barcodeConfig.GetSpecialValue(Context, sequence, barcodeno, item);
                            stck = item.CreateStock(barcode);
                            stck.barcodeid = barcodeno;
                        }

                        if (sellingmode != Products.sellingMode.Piece)
                        {
                            var buyingqty = item.buyingqty;
                            if (item.actualqty > buyingqty) buyingqty = item.actualqty;
                            stck.qty = buyingqty + item.freeqty - (item.damagedqty + item.sellableqty);
                            if (item.damagedqty + item.returnqty > 0)
                                stck.returnqty = item.damagedqty + item.returnqty;
                        }
                        if (retaillocationid == 0)
                            stck.purchaseqty = stck.qty;
                        if (item.srcsupplierid > 0)
                            stck.srcsupplierid = item.srcsupplierid;
                        item.stock.Add(stck);
                    }

                    List<string> jsonbarcode = item.barcodes == null ? new List<string>() :
                        Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(item.barcodes);
                    if (jsonbarcode == null)
                        jsonbarcode = new List<string>();
                    if (item.stock.Count == 1)
                        jsonbarcode.Add(item.stock.First().barcode);
                    else
                        jsonbarcode.Add(item.stock.First().barcode + "-" + item.stock.Last().barcode);
                    item.barcodes = Newtonsoft.Json.JsonConvert.SerializeObject(jsonbarcode);


                    bool countmissmatch = true;
                    int retry = 0;
                    while (countmissmatch && retry < 3)
                    {
                        string query = @"update public.lrinvoiceitems set version=@version+1,modifiedby=@modifiedby,itemid=@itemid,modifiedon=@modifiedon,barcodedon=@barcodedon,barcodes=@barcodes,sellableqty=sellableqty+@sellableqty 
                                             where id=@id and version=@version and 
                                            ((case when lrinvoiceitems.buyingqty<lrinvoiceitems.actualqty then lrinvoiceitems.actualqty else lrinvoiceitems.buyingqty end) + 
                                            coalesce(lrinvoiceitems.freeqty,0) - (lrinvoiceitems.damagedqty+lrinvoiceitems.sellableqty))>=@sellableqty RETURNING version,sellableqty";
                        command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.itemid;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("barcodedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("sellableqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = Convert.ToDecimal(item.stock.Sum(e => e.qty));
                        command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = item.barcodes;
                        using (var dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                item.version = dr.GetInt32(0);
                                item.sellableqty = dr.GetDecimal(1);
                                countmissmatch = false;
                                break;
                            }
                        }
                        command.CommandText = "Select version,buyingqty,damagedqty,sellableqty from lrinvoiceitems where id=@id";
                        using (var dr = command.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                var version = dr.GetInt32(0);
                                var buyingqty = dr.GetDecimal(1);
                                var damagedqty = dr.GetDecimal(2);
                                var sellableqty = dr.GetDecimal(3);
                            }
                        }
                        //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception(GetSQL(command), new Exception()), WebAPI.Common.Logger.LogType.Error, item);
                        retry++;
                        restStockMissmatch(item.id, transaction, conn);
                        //command.ExecuteNonQuery();
                    }



                    if (!countmissmatch)
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_autouangln &&
                            item.stock.Exists(e => string.IsNullOrEmpty(e.serialno)))
                        {
                            CreateUAN(item.stock, conn, transaction);
                        }

                        foreach (var stock in item.stock)
                        {
                            if (stock.srcsupplierid == 0 && sourcesupplierid > 0)
                                stock.srcsupplierid = sourcesupplierid;
                            stock.supplierid = supplierid;
                            stock.buyingprice = buyingprice;
                            stock.ibuyingprice = ibuyingprice;
                            stock.lrreference = lrreference;
                            stock.designid = designid;
                            stock.imageid = imgid;
                            stock.stockdate = DateTimeNow;
                            stock.locationid = locationid;
                            stock.displaymrp = mrp;
                            stock.dealerprice = dealerprice;
                            stock.pricetagid = item.Details.pricetagid;
                            if (!string.IsNullOrEmpty(item.gln))
                                stock.serialno = item.gln;
                            if (!string.IsNullOrEmpty(item.uan))
                                stock.serialno = item.uan;
                            if (stock.retaillocationid <= 0)
                                stock.retaillocationid = retaillocationid;

                            // retaillocationid = 0 - NONE          (if NONE selected, stock willn't be moved to respective sale location)
                            // retaillocationid = -1 - Multiple Locations
                            if (retaillocationid < 0 && stock.retaillocationid > 0)
                                retaillocationid = stock.retaillocationid;
                            if (itemdetail != null)
                            {
                                stock.batch = itemdetail.batch;
                                stock.expiry = itemdetail.expiry;
                                if (string.IsNullOrEmpty(stock.serialno) && !string.IsNullOrEmpty(itemdetail.serialno))
                                    stock.serialno = itemdetail.serialno;
                            }
                            stock.eprice = GetEPrice(stock.price - stock.discount, tax);

                            var newstock = Insert(transaction, conn, stock);
                            // if its existing uan based stock hence it is not created
                            if (stock.id == 0 && newstock.id > 0)
                                stock.id = newstock.id;

                            if (item.transaction == null) item.transaction = new List<Lrinvoiceitemsbarcode.Transtock>();
                            item.transaction.Add(new Lrinvoiceitemsbarcode.Transtock(stock.qty - stock.returnqty, stock));

                            if (autotransferretail && retaillocationid > 0)
                            {
                                var existingsalableitem = salablegoodsDataService.Search(new SalablegoodsCreteria()
                                {
                                    barcode = stock.barcode,
                                    locationid = retaillocationid
                                }, conn, transaction);

                                if (existingsalableitem.Count == 0)
                                {
                                    Salablegoods sitem = new Salablegoods();
                                    sitem.inwardon = DateTimeNow;
                                    sitem.barcode = stock.barcode;
                                    sitem.bundleid = -5;
                                    sitem.iqty = stock.qty - stock.returnqty;
                                    sitem.isactive = true;
                                    sitem.itemid = stock.itemid;
                                    sitem.locationid = retaillocationid;
                                    sitem.stockid = stock.id;
                                    sitem.version = 1;
                                    sitem.companyid = stock.companyid;
                                    sitem.supplierid = stock.supplierid;
                                    sitem.received = true;
                                    sitem.price = stock.price;
                                    sitem.discount = stock.discount;
                                    sitem.additionaldiscount = stock.additionaldiscount;
                                    sitem.ispurchaselocation = true;
                                    sitem.pqty = stock.qty - stock.returnqty;
                                    sitem.costprice = stock.buyingprice;
                                    sitem.icostprice = stock.ibuyingprice;
                                    salablegoodsDataService.Insert(transaction, conn, sitem);
                                    item.transaction.Add(new Lrinvoiceitemsbarcode.Transtock(stock.qty - stock.returnqty, sitem));
                                    //command = GetCommand(@"update stock set transfered=transfered+" + stock.qty + " where id=" + stock.id, conn, transaction);
                                    //command.ExecuteNonQuery();
                                }
                                else if (newstock.uansubstiated)
                                {
                                    command = GetCommand(@"update stock set transfered=transfered+" + stock.qty + " where id=" + newstock.id, conn, transaction);
                                    command.ExecuteNonQuery();
                                    command = GetCommand(@"update salablegoods set iqty=iqty+" + stock.qty + " where id=" + existingsalableitem[0].id, conn, transaction);
                                    command.ExecuteNonQuery();
                                    item.transaction.Add(new Lrinvoiceitemsbarcode.Transtock(stock.qty, existingsalableitem[0]));
                                    if (newstock.price != stock.price)
                                    {
                                        command = GetCommand(@"update stock set ismultiprice=true where id=" + newstock.id, conn, transaction);
                                        command.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    if (existingsalableitem[0].ispurchaselocation)
                                        existingsalableitem[0].pqty += (stock.qty - stock.returnqty);
                                    existingsalableitem[0].iqty += (stock.qty - stock.returnqty);
                                    salablegoodsDataService.Update(transaction, conn, existingsalableitem[0], existingsalableitem[0]);
                                    item.transaction.Add(new Lrinvoiceitemsbarcode.Transtock(stock.qty - stock.returnqty, existingsalableitem[0]));
                                }
                            }
                        }

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                            !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                            !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_autouangln)
                        {
                            List<string> externalcodes = new List<string>();

                            if (item.Details != null && !string.IsNullOrEmpty(item.Details.serialno))
                            {
                                externalcodes.AddRange(item.Details.serialno.Split(','));
                            }

                            if (item.Details != null && !string.IsNullOrEmpty(item.Details.gln) &&
                                !externalcodes.Contains(item.Details.gln))
                            {
                                externalcodes.Add(item.Details.gln);
                            }
                            SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                            foreach (var barcode in externalcodes)
                            {
                                if (string.IsNullOrEmpty(barcode)) continue;
                                foreach (var istock in item.stock)
                                {
                                    Sellablebarcodes sellablebarcodes = new Sellablebarcodes();
                                    sellablebarcodes.barcode = barcode;
                                    sellablebarcodes.stockid = istock.id;
                                    sellablebarcodes.itemid = istock.itemid;
                                    sellablebarcodes.productid = istock.productid;
                                    sellablebarcodes.isactive = true;
                                    sellablebarcodes.companyid = istock.companyid;
                                    sellablebarcodes.createdby = istock.createdby;
                                    sellablebarcodes.createdon = istock.createdon;
                                    sellablebarcodes.modifiedby = istock.modifiedby;
                                    sellablebarcodes.modifiedon = istock.modifiedon;
                                    sellablebarcodes.version = istock.version;
                                    if (!string.IsNullOrEmpty(item.Details.batch))
                                        sellablebarcodes.batchandexpiry = string.Format("{0}/{1}", item.Details.batch, item.Details.expiry.ToString("MMM-yyyy"));
                                    sellablebarcodesDataService.Insert(transaction, conn, sellablebarcodes);
                                }
                            }
                        }
                        else if (item.isQuantoImport && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_quanto_barcodes)
                        {
                            SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                            if (item.Details != null && !string.IsNullOrEmpty(item.Details.quantoexistingbarcode))
                            {
                                foreach (var istock in item.stock)
                                {
                                    Sellablebarcodes sellablebarcodes = new Sellablebarcodes();
                                    sellablebarcodes.barcode = item.Details.quantoexistingbarcode;
                                    sellablebarcodes.stockid = istock.id;
                                    sellablebarcodes.itemid = istock.itemid;
                                    sellablebarcodes.productid = istock.productid;
                                    sellablebarcodes.isactive = true;
                                    sellablebarcodes.companyid = istock.companyid;
                                    sellablebarcodes.createdby = istock.createdby;
                                    sellablebarcodes.createdon = istock.createdon;
                                    sellablebarcodes.modifiedby = istock.modifiedby;
                                    sellablebarcodes.modifiedon = istock.modifiedon;
                                    sellablebarcodes.version = istock.version;
                                    if (!string.IsNullOrEmpty(item.Details.batch))
                                        sellablebarcodes.batchandexpiry = string.Format("{0}/{1}", item.Details.batch, item.Details.expiry.ToString("MMM-yyyy"));
                                    sellablebarcodesDataService.Insert(transaction, conn, sellablebarcodes);
                                }
                            }
                        }

                        if (barcodeConfig.Type == BarcodeManager.BarcodeType.PACKSERIAL)
                        {
                            PackbarcodeDataService packcodeservice = new PackbarcodeDataService() { Context = this.Context };
                            Packbarcode pcode = new Packbarcode()
                            {
                                barcode = initialSlot.ToString("F0") + "$",
                                isactive = true,
                                version = 1,
                                barcodes = item.stock.ConvertAll(e => new Packbarcode.Barcodes()
                                {
                                    barcode = e.barcode,
                                    qty = 1,
                                    pieces = 1,
                                    stockid = e.id
                                })
                            };
                            packcodeservice.Insert(transaction, conn, pcode);
                        }

                        InboxManager.Instance(Context.OrganizationID).UpdateLRStatus(_lrid, Inbox.StatusList.Barcode_Generating, transaction, conn, Context);
                    }
                    else
                    {
                        item = null;
                    }


                }
                if (item != null)
                {
                    item.startbarcodeno = _startbarcodeno;
                    item.endbarcodeno = _endbarcodeno;
                }
                return item;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
        }

        private decimal GetEPrice(decimal price, Tax tax)
        {
            if (tax == null) return price;
            if (tax.type == 2 && tax.taxsplit != null && tax.taxsplit.items.Count > 1)
            {
                if (price > tax.taxsplit.items[0].fromamount
                    && price <= tax.taxsplit.items[0].toamount)
                {
                    var taxpercentage = (tax.taxsplit.items[0].cgst * 2);
                    return price / (100 + taxpercentage) * 100;
                }
                else
                {
                    var taxpercentage = (tax.taxsplit.items[1].cgst * 2);
                    return price / (100 + taxpercentage) * 100;
                }
            }
            return price / (100 + tax.taxpercentage) * 100;
        }

        internal void UpdateEPrice(Stock currentStockItem)
        {
            ProductsDataService productdataservice = new ProductsDataService() { Context = this.Context };
            TaxDataService taxdata = new TaxDataService() { Context = this.Context };
            decimal taxpercentage = 0;
            decimal salestaxpercentage = 0;
            var products = productdataservice.Search(new ProductsCreteria()
            {
                id = currentStockItem.productid
            });

            var taxlist = taxdata.Search(new TaxCreteria() { id = products[0].salestaxid });
            if (taxlist != null && taxlist.Count > 0)
            {
                if (taxlist[0].type == 1)
                {
                    taxpercentage = taxlist[0].taxpercentage;
                    salestaxpercentage = taxpercentage;
                }
                else if (taxlist[0].type == 2 && taxlist[0].taxsplit != null && taxlist[0].taxsplit.items.Count > 1)
                {
                    if (currentStockItem.price - currentStockItem.discount > taxlist[0].taxsplit.items[0].fromamount
                        && currentStockItem.price - currentStockItem.discount <= taxlist[0].taxsplit.items[0].toamount)
                    {
                        taxpercentage = (taxlist[0].taxsplit.items[0].cgst * 2);
                    }
                    else
                    {
                        taxpercentage = (taxlist[0].taxsplit.items[1].cgst * 2);
                    }

                    if (currentStockItem.price - currentStockItem.discount > taxlist[0].taxsplit.items[0].fromamount
                        && currentStockItem.price - currentStockItem.discount <= taxlist[0].taxsplit.items[0].toamount)
                    {
                        salestaxpercentage = taxpercentage;
                    }
                    else
                    {
                        salestaxpercentage = taxpercentage;
                    }
                }
            }
            if (salestaxpercentage > 0)
                currentStockItem.eprice = (currentStockItem.price - currentStockItem.discount) / (salestaxpercentage + 100) * 100;
            else
                currentStockItem.eprice = (currentStockItem.price - currentStockItem.discount);
        }

        private void CreateUAN(List<Stock> stock, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction trans)
        {
            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("UAN.code", 0, stock.First().companyid);
            foreach (var item in stock.Where(e => string.IsNullOrEmpty(e.serialno)))
            {
                item.serialno = sequence.GetNextNumber(this.Context, conn, trans);
            }
        }

        private void restStockMissmatch(long itemid, Npgsql.NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            NpgsqlCommand command = GetCommand(@"select sum(qty) from stock where invoiceitemid = " + itemid, conn, transaction);
            using (var dr = command.ExecuteReader())
            {
                if (dr.Read())
                {
                    decimal qty = dr.IsDBNull(0) ? 0 : Convert.ToDecimal(dr[0]);
                    command = GetCommand(@"update lrinvoiceitems set sellableqty = " + qty + " where id=" + itemid, conn, transaction);
                }
            }
            command.ExecuteNonQuery();
        }

        public bool Delete(Stock oStock)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oStock);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return successCount > 0;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Stock oStock)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStock.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(Stock oStock, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {

            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStock.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStock.version;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStock.invoiceitemid;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal bool ChangeMultiplePrice(Stock item)
        {
            var param = new SortedList<string, object>();
            param.Add("id", item.id);
            param.Add("ismultiprice", item.ismultiprice);
            return ExecuteQuery("update stock set ismultiprice =@ismultiprice where id=@id", param) > 0;
        }

        internal ImageController.ExcelDocument ConvertDesign2Barcode(ImageController.ExcelDocument source)
        {
            SortedList<string, Tuple<long, int>> sizelist = new SortedList<string, Tuple<long, int>>();
            ImageController.ExcelDocument excelDocument = new ImageController.ExcelDocument();
            excelDocument.rows = new List<ImageController.ExcelDocument.Row>();
            var designindex = source.columns.FindIndex(x => x.ToLower().StartsWith("design"));

            source.rows.RemoveAt(0);
            var designcodes = source.rows.ConvertAll(x =>
            {
                return x.values[designindex].ToUpper();
            }).Distinct().ToList();

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var items = mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
            {
                designitemcode = String.Join(",", designcodes)
            });

            var errors = new List<string>();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var allcolours = referencelistDataService.Search(new ReferencelistCreteria() { type = "COLOUR" });
            var allsizes = referencelistDataService.Search(new ReferencelistCreteria() { type = "SIZE" });

            int colindex = 0;
            source.columns.ForEach(x =>
            {
                if (allsizes.Exists(y => y.name.ToUpper() == x.ToUpper()))
                {
                    var sizeid = allsizes.Find(y => y.name.ToUpper() == x.ToUpper()).id;
                    sizelist.Add(x.ToUpper(), new Tuple<long, int>(sizeid, colindex));
                }
                colindex++;
            });

            decimal availableqty = 0;
            items.ForEach(x => x.name = x.name.ToUpper());
            source.rows.ForEach(x =>
            {
                var designitemcode = x.values[designindex].ToUpper();
                var uiditems = items.FindAll(id => id.name == designitemcode);
                if (uiditems.Count > 0)
                {
                    for (var c = 0; c < source.columns.Count; c++)
                    {
                        if (sizelist.ContainsKey(source.columns[c].ToUpper()))
                        {
                            decimal qtyvalue = 0;
                            var sizeinfo = sizelist[source.columns[c].ToUpper()];
                            if (decimal.TryParse(x.values[c], out qtyvalue))
                            {
                                availableqty += qtyvalue;
                                var uiditem = uiditems.Find(id => id.sizeid == sizeinfo.Item1);
                                var r = new ImageController.ExcelDocument.Row();
                                r.values.Add(uiditem.uid);
                                r.values.Add(qtyvalue.ToString("N2"));
                                excelDocument.rows.Add(r);
                                uiditems.Remove(uiditem);
                            }
                        }
                    }
                }
                else
                {
                    errors.Add("Design Missing - " + designitemcode);
                }
            });

            items.ForEach(x =>
            {
                var r = new ImageController.ExcelDocument.Row();
                r.values.Add(x.uid);
                r.values.Add("0");
                excelDocument.rows.Add(r);
            });

            excelDocument.qty = availableqty;
            excelDocument.errors = errors;
            return excelDocument;
        }
        internal ImageController.ExcelDocument Convert2Barcode(ImageController.ExcelDocument source)
        {
            ImageController.ExcelDocument excelDocument = new ImageController.ExcelDocument();
            excelDocument.rows = new List<ImageController.ExcelDocument.Row>();
            var designindex = source.columns.FindIndex(x => x.ToLower().StartsWith("design"));
            var brandindex = source.columns.FindIndex(x => x.ToLower().StartsWith("brand"));
            var productindex = source.columns.FindIndex(x => x.ToLower().StartsWith("product"));
            var colourindex = source.columns.FindIndex(x => x.ToLower().StartsWith("colour"));
            var sizeindex = source.columns.FindIndex(x => x.ToLower().StartsWith("size"));

            var stockqtyindex = source.columns.FindIndex(x => x.ToLower().StartsWith("stockqty"));
            var salerateindex = source.columns.FindIndex(x => x.ToLower().StartsWith("salerate"));
            source.rows.RemoveAt(0);
            var designcodes = source.rows.ConvertAll(x =>
            {
                return x.values[designindex].ToUpper();
            }).Distinct().ToList();
            var brandcodes = source.rows.ConvertAll(x =>
            {
                return x.values[brandindex];
            }).Distinct().ToList();
            var productcodes = source.rows.ConvertAll(x =>
            {
                return x.values[productindex];
            }).Distinct().ToList();
            var colourcodes = source.rows.ConvertAll(x =>
            {
                return x.values[colourindex];
            }).Distinct().ToList();
            var sizecodes = source.rows.ConvertAll(x =>
            {
                return x.values[sizeindex];
            }).Distinct().ToList();

            designcodes.RemoveAll(x => x.StartsWith("DESIGN"));
            brandcodes.RemoveAll(x => x.StartsWith("BRAND"));
            productcodes.RemoveAll(x => x.StartsWith("PRODUCT"));
            colourcodes.RemoveAll(x => x.StartsWith("COLOUR"));
            sizecodes.RemoveAll(x => x.StartsWith("SIZE"));


            BrandDataService brandDataService = new BrandDataService() { Context = this.Context };
            var allbrands = brandDataService.Search(new BrandCreteria());

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var allproducts = productsDataService.Search(new ProductsCreteria() { });

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var allcolours = referencelistDataService.Search(new ReferencelistCreteria() { type = "COLOUR" });
            var allsizes = referencelistDataService.Search(new ReferencelistCreteria() { type = "SIZE" });

            var missingbrands = brandcodes.FindAll(x => !allbrands.Exists(y => y.name.ToUpper() == x.ToUpper()));
            var missingproducts = productcodes.FindAll(x => !allproducts.Exists(y => y.name.ToUpper() == x.ToUpper()));
            var missingcolour = colourcodes.FindAll(x => !allcolours.Exists(y => y.name.ToUpper() == x.ToUpper()));
            var missingsize = sizecodes.FindAll(x => !allsizes.Exists(y => y.name.ToUpper() == x.ToUpper()));

            List<string> errors = new List<string>();
            if (missingbrands.Count > 0)
                errors.Add("Missing Brand : " + String.Join(",", missingbrands));
            if (missingbrands.Count > 0)
                errors.Add("Missing Product : " + String.Join(",", missingproducts));
            if (missingbrands.Count > 0)
                errors.Add("Missing Colour : " + String.Join(",", missingcolour));
            if (missingbrands.Count > 0)
                errors.Add("Missing Size : " + String.Join(",", missingsize));

            var alldesigns = new List<string>();
            using (var reader = ExecuteReader("Select distinct designid from stock where designid in (" + string.Join(",", designcodes.ConvertAll(x => "'" + x + "'")) + ")", null))
            {
                while (reader.Read())
                {
                    alldesigns.Add(reader.IsDBNull(0) ? "" : reader.GetString(0).ToUpper());
                }
            };
            var missingdesigns = designcodes.FindAll(x => !alldesigns.Contains(x));
            if (missingdesigns.Count > 0)
                errors.Add("Missing Designs : " + String.Join(",", missingdesigns));

            //if (errors.Count > 0)
            //    throw new Exception(string.Join("\r\n", errors));

            var datatable = ExecuteDataSet(@"SELECT stock.id,stock.barcode,items.productid,items.brandid,items.sizeid,qty,
products.name productname,brand.name brandname,refsize.name sizename,refcolour.name colourname,stock.designid designname,price FROM stock
left outer join items on items.id = stock.itemid
left outer join products on products.id = items.productid
left outer join brand on brand.id = items.brandid
left outer join referencelist refsize on refsize.id = items.sizeid
left outer join referencelist refcolour on refcolour.id = items.colourid
order by stock.id", null).Tables[0];

            decimal availableqty = 0;

            source.rows.ForEach(x =>
            {

                var brandname = x.values[brandindex];
                var designname = x.values[designindex];
                var productname = x.values[productindex];
                var sizename = x.values[sizeindex];
                var colourname = x.values[colourindex];
                decimal qty = 0;
                decimal rate = 0;
                if (decimal.TryParse(x.values[salerateindex], out rate) &&
                decimal.TryParse(x.values[stockqtyindex], out qty))
                {
                    List<string> conditions = new List<string>();
                    if (!string.IsNullOrEmpty(brandname))
                        conditions.Add("brandname='" + brandname.Replace("'", "") + "'");
                    if (!string.IsNullOrEmpty(productname))
                        conditions.Add("productname='" + productname.Replace("'", "") + "'");
                    if (!string.IsNullOrEmpty(sizename))
                        conditions.Add("sizename='" + sizename.Replace("'", "") + "'");
                    if (!string.IsNullOrEmpty(designname))
                        conditions.Add("designname='" + designname.Replace("'", "") + "'");
                    if (!string.IsNullOrEmpty(colourname))
                        conditions.Add("colourname='" + colourname.Replace("'", "") + "'");

                    var rows = datatable.Select(String.Join(" and ", conditions), "id desc");
                    var frows = rows.ToList().FindAll(r => Convert.ToDecimal(r["price"]) == rate);
                    if (frows.Count > 0)
                    {
                        var r = new ImageController.ExcelDocument.Row();
                        r.values.Add(frows[0]["barcode"].ToString());
                        r.values.Add(qty.ToString("N2"));
                        excelDocument.rows.Add(r);
                        availableqty += qty;
                    }
                    else if (rows.Length > 0)
                    {
                        var r = new ImageController.ExcelDocument.Row();
                        r.values.Add(rows[0]["barcode"].ToString());
                        r.values.Add(qty.ToString("N2"));
                        availableqty += qty;
                        excelDocument.rows.Add(r);
                    }
                    else
                    {
                        errors.Add("Missing Row - " + qty.ToString() + "-" + String.Join(" and ", conditions));
                    }
                }
            });


            excelDocument.qty = availableqty;
            excelDocument.errors = errors;
            return excelDocument;
            //int q = 0;
            //excelDocument.rows.ForEach(x =>
            //{
            //    if (int.TryParse(x.values[1], out q))
            //        x.values[1] = q.ToString();
            //});

            //excelDocument.rows = excelDocument.rows.ConvertAll(x => new { uid = x.values[0], qty = x.values[1] }).GroupBy(x => x.uid).ToList().ConvertAll(x =>
            //{
            //    var r = new ImageController.ExcelDocument.Row();
            //    r.values.Add(x.Key);
            //    r.values.Add(x.Sum(y => int.Parse(y.qty)).ToString());
            //    return r;
            //});

            //return excelDocument;
        }

        internal string SearchCuttingBarcodes()
        {
            string query = @"select stock.barcode||','||products.name||','||round(stock.price,2)||','||round(stock.price+stock.discount,2) from stock 
inner join items on items.id = stock.itemid
inner join products on products.id = items.productid
where products.sellingmode = 3 and stock.isactive=true";
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                    stringBuilder.AppendLine(reader.GetString(0));
            }
            return stringBuilder.ToString();
        }

        internal CuttingBarcode PrintCuttingBarcode(CuttingBarcode item)
        {
            if (!string.IsNullOrEmpty(item.empcode) &&
                string.IsNullOrEmpty(item.employeecode))
                item.employeecode = item.empcode;

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "CUTTINGBARCODE",
                locationid = item.locationid
            });
            if (reportformats.Count == 0)
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = "CUTTINGBARCODE"
                });
            if (reportformats.Count == 0)
                throw new Exception("Please configure cutting barcode printing format");

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(Context.OrganizationID,
                reportformats[0].reportformat.format);

            labreport.DataSource = item;

            var imageData = new ImageData() { Context = new UserClaim() { OrganizationID = Context.OrganizationID } };
            List<string> imageids = labreport.format.GetImages();
            labreport.Additional = new SortedDictionary<string, object>();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null && file.data.Length > 0)
                {
                    labreport.Additional.Add("IMG-" + imgid, file.data);
                }
            }

            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
            var barcodelocation = salablegoodsDataService.BarcodeSearch(item.locationid.ToString(),
                 item.barcode
            );

            if (!string.IsNullOrEmpty(item.employeecode))
            {
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                var employees = employeeDataService.SearchHeader(new EmployeeCreteria() { employeecode = item.employeecode });
                if (employees.Count > 0) item.employeename = employees[0].name;
            }
            item.vqty = decimal.Parse(item.qty.Split('-')[0]);
            item.productname = barcodelocation.sellingname;
            item.price = barcodelocation.price;
            item.mrp = barcodelocation.mrp;
            item.discount = barcodelocation.discount;
            item.final = barcodelocation.price - barcodelocation.discount;
            item.ebarcode = "~" + item.barcode + "~" + item.qty + "~" + item.employeecode;
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = barcodelocation.companyid
            });

            if (company.Count > 0 && !string.IsNullOrEmpty(company[0].logo))
            {
                var file = imageData.GetData(company[0].logo.LongFromBase62());
                if (file != null && file.data != null && file.data.Length > 0)
                {
                    labreport.Additional.Add("LOGO", file.data);
                }
            }

            var document = labreport.format.RenderBytes(new ImageData(), item,
                item, labreport.Additional, false);

            item.printdocument = Convert.ToBase64String(document);

            return item;
        }

        internal CuttingBarcode BarcodeInfo(StockDataService.CuttingBarcode item)
        {
            if (item.locationid == 0 &&
               !string.IsNullOrEmpty(item.location))
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService()
                {
                    Context = this.Context
                };
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    type = "LOCATION",
                    name = item.location
                });
                if (location.Count > 0)
                {
                    item.locationid = location[0].id;
                }
            }
            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
            var barcodelocation = salablegoodsDataService.BarcodeSearch(
                 item.locationid.ToString(),
                 item.barcode
            );

            if (!string.IsNullOrEmpty(item.employeecode))
            {
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                var employees = employeeDataService.SearchHeader(new EmployeeCreteria() { employeecode = item.employeecode });
                if (employees.Count > 0) item.employeename = employees[0].name;
            }
            item.vqty = decimal.Parse(item.qty.Split('-')[0]);
            item.productname = barcodelocation.sellingname;
            item.price = barcodelocation.price;
            item.mrp = barcodelocation.mrp;
            item.discount = barcodelocation.discount;
            item.final = barcodelocation.price - barcodelocation.discount;
            item.ebarcode = "~" + item.barcode + "~" + item.qty + "~" + item.employeecode;
            return item;
        }

        internal int SyncMissingHSN()
        {
            var affectedrows = ExecuteQuery(@"with alluid
                as
                (
                select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,prdhsncode,
                cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,cast(schemelist->>'cost' as numeric) dcost,
                schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,cast(schemelist->>'sleeveid' as bigint) sleeveid,
                mfgdesign.productid,mfgdesign.brandid,cast(schemelist->>'itemid' as bigint) itemid,mfgdesign.boxid,
                mfgdesign.fitid,mfgdesign.styleid,mfgdesign.materialid,mfgdesign.patternid,mfgdesign.typeid,mfgdesign.id,
                schemelist->>'designitemcode' designitemcode,schemelist->>'name' pname
                from mfgdesign
                LEFT JOIN LATERAL
                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                where mfgdesign.isactive = true
                ),missinghsn as
                (
                select stock.id,prdhsncode from stock 
                inner join alluid on alluid.uid = stock.serialno
                where stock.hsncode = ''
                )
                update stock set hsncode = prdhsncode from missinghsn
                where missinghsn.id = stock.id
                and hsncode = ''", null);

            affectedrows += ExecuteQuery(@"with alluid
                as
                (
                    select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
                    cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,prdhsncode,
                    cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,cast(schemelist->>'cost' as numeric) dcost,
                    schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,cast(schemelist->>'sleeveid' as bigint) sleeveid,
                    mfgdesign.productid,mfgdesign.brandid,cast(schemelist->>'itemid' as bigint) itemid,mfgdesign.boxid,
                    mfgdesign.fitid,mfgdesign.styleid,mfgdesign.materialid,mfgdesign.patternid,mfgdesign.typeid,mfgdesign.id,
                    schemelist->>'designitemcode' designitemcode,schemelist->>'name' pname
                    from mfgdesign
                    LEFT JOIN LATERAL
                    json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                    where mfgdesign.isactive = true
                ),missinghsn as
                (
                    select billitems.id,prdhsncode from billitems 
                    inner join alluid on alluid.uid = billitems.attributes->>'serialno'
                    where billitems.hsncode = ''
                )
				
                update billitems set hsncode = prdhsncode from missinghsn
                where missinghsn.id = billitems.id
                and hsncode = ''", null);

            return affectedrows;
        }

        internal void UpdateBarcodePrintCount(List<Stock2Print> result)
        {
            var barcodes = result.GroupBy(x => x.barcode).ToList().ConvertAll(y => new { barcode = y.Key, count = y.Count() });
            NpgsqlTransaction transaction = null;

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (var bitem in barcodes)
                {
                    var param = new SortedList<string, object>();
                    string SQL_CountUpdate = @"UPDATE public.stock SET pbarcount=COALESCE(pbarcount,0)+@count where barcode=@barcode RETURNING qty-(damagedqty+returnqty),pbarcount";
                    param.Add("barcode", bitem.barcode);
                    param.Add("count", bitem.count);
                    using (var reader = ExecuteReader(SQL_CountUpdate, param, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            var totalqty = reader.IsDBNull(0) ? 0 : Convert.ToDecimal(reader[0]);
                            var pbarcount = reader.IsDBNull(1) ? 0 : Convert.ToDecimal(reader[1]);
                            if (pbarcount > totalqty)
                            {
                                throw new Exception(bitem.barcode+" Can't take additional "+(pbarcount-totalqty)+" print out");
                            }
                        }
                    }
                }
                transaction.Commit();
                conn.Close();
            }
        }

        internal bool ResetBarcodePrintCount(List<LRInvoiceItemBarcode> items)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                foreach (var bitem in items)
                {
                    var param = new SortedList<string, object>();
                    string SQL_CountUpdate = @"UPDATE public.stock SET pbarcount=COALESCE(pbarcount,0)-@count where barcode=@barcode";
                    param.Add("barcode", bitem.code);
                    param.Add("count", bitem.qty);
                    ExecuteQuery(SQL_CountUpdate, param, conn, transaction);
                }
                transaction.Commit();
                conn.Close();
            }
            return true;
        }
    }
}
