using InfyPOS.Common;
using InfyPOS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InfyPOS.Processors
{
    public class OnlineIntegration : Common.BaseData
    {
        public long organizationid { get; set; }
        public long locationid { get; set; }
        public OnlineIntegration(long organizationid) : base()
        {

            this.organizationid = organizationid;
        }

        public string cateloglist = @"select stock.designid design,
salablegoods.barcode,stockmarker.id stockmarkerid,
stock.imageid,salablegoods.olqty stock,salablegoods.price,
salablegoods.discount,stock.displaymrp mrp,
brand.name brand, section.name section,prdgroup.name productgroup, 
products.name as product,items.id code,stock.productid,colourid,
style.name style,pattern.name pattern,size.name size, colour.name colour,
type.name typename,material.name material,sleeve.name sleeve,fit.name fit
from stockmarker
inner join stock on stock.id = stockid
inner join items on items.id = stock.itemid
inner join salablegoods on salablegoods.id = salablegoodsid
left outer join products on products.id = stock.productid
            left outer join brand brand on brand.id = items.brandid
            left outer join referencelist colour on colour.id = colourid
            left outer join referencelist size on size.id = sizeid
            left outer join referencelist style on style.id = styleid
            left outer join referencelist pattern on pattern.id = patternid
            left outer join referencelist material on material.id = materialid
            left outer join referencelist type on type.id = typeid
			left outer join referencelist section on section.id = salescounterid
            left outer join referencelist sleeve on sleeve.id = sleeveid
            left outer join referencelist fit on fit.id = fitid
			left outer join referencelist prdgroup on prdgroup.id = products.productgroup
where stock.isactive=true and salablegoods.hasstock=true and salablegoods.isactive=true ";

        public string productlist = @"with 
uniquelist as 
(
	select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) stockqty,
    sum(salablegoods.olqty) olqty,
    salablegoods.discount,stock.displaymrp,stock.designid,colourid,sizeid,styleid,patternid,stock.imageid,
	materialid,typeid,items.brandid,stock.productid,stock.itemid itemcode,salablegoods.barcode,items.fitid,
    salablegoods.price,items.sleeveid from salablegoods
    inner join stock on stock.id = stockid
    inner join items on items.id = stock.itemid
	inner join stockmarker on stockmarker.salablegoodsid = salablegoods.id
	where salablegoods.hasstock = true {0}
	group by stock.designid,colourid,sizeid,styleid,patternid,materialid,typeid,stock.productid,brandid,stock.itemid,
    salablegoods.price,salablegoods.discount,stock.displaymrp,salablegoods.barcode,stock.imageid,items.fitid,items.sleeveid
)

select designid design,products.name product,brand.name brand,type.name typename,itemcode code,stockqty stock,
colour.name colour,size.name size,style.name style,pattern.name pattern, material.name material,olqty,
price,discount,displaymrp mrp,products.id productid,colourid,barcode,sections.name section,
productsgroups.name productsgroup,imageid,fit.name fitname,sleeve.name sleevename from uniquelist
inner join products on products.id = uniquelist.productid
left outer join brand on brand.id = brandid
left outer join referencelist sections on sections.id = salescounterid
left outer join referencelist productsgroups on productsgroups.id = products.productgroup
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist sleeve on sleeve.id = sleeveid";

        public List<Product> BuildCatelog()
        {
            List<Product> results = new List<Product>();
            var param = new SortedList<string, object>();
            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var reader = ExecuteReader(cateloglist, param, connection))
                {
                    while (reader.Read())
                    {
                        Product oitems = new Product();
                        oitems.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        oitems.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        oitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oitems.style = reader["style"] == DBNull.Value ? "" : reader["style"].ToString();
                        oitems.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        oitems.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                        oitems.productgroup = reader["productgroup"] == DBNull.Value ? "" : reader["productgroup"].ToString();
                        oitems.pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                        oitems.code = reader["code"] == DBNull.Value ? "" : Convert.ToInt64(reader["code"]).ToString();
                        oitems.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        oitems.type = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitems.material = reader["material"] == DBNull.Value ? "" : reader["material"].ToString();
                        oitems.sleeve = reader["sleeve"] == DBNull.Value ? "" : reader["sleeve"].ToString();
                        oitems.fit = reader["fit"] == DBNull.Value ? "" : reader["fit"].ToString();
                        oitems.image = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        oitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitems.otherstock = reader["stock"] == DBNull.Value ? 0 : (int)Convert.ToDecimal(reader["stock"]);
                        oitems.stock = reader["olqty"] == DBNull.Value ? 0 : (int)Convert.ToDecimal(reader["olqty"]);
                        oitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        if (oitems.mrp == 0 && oitems.price > 0)
                            oitems.mrp = oitems.price;
                        oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        results.Add(oitems);
                    }
                }

                using (var reader = ExecuteReader(@"select productid,designid,imageid,colourid,referencelist.name colour from stockimage 
                            left outer join referencelist on referencelist.id = colourid", null, connection))
                {
                    while (reader.Read())
                    {
                        long productid = reader.GetInt64(0);
                        string designid = reader.GetString(1);
                        string imageid = reader.GetString(2);
                        long colourid = reader.GetInt64(3);
                        string colour = reader.GetString(4);
                        var item = results.Find(x => x.productid == productid && x.design == designid &&
                        x.colourid == colourid);
                        if (item != null)
                        {
                            if (item.imagelist == null)
                                item.imagelist = new List<string>();
                            item.imagelist.Add(imageid);
                        }
                    }
                }

                foreach (var p in results)
                {
                    p.catelogkey = Hash(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                        p.product, p.brand, p.design, p.material, p.pattern, p.style, p.type, p.fit, p.sleeve));
                    p.uniquekey = Hash(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                        p.product, p.brand, p.code, p.colour, p.design, p.material, p.pattern,
                        p.size, p.style, p.type, p.fit, p.sleeve));
                }
                connection.Close();
                return results;
            }
        }
        public class ProductCriteria
        {
            public long productid { get; set; }
            public string designid { get; set; }
            public long markerid { get; set; }
            public bool online { get; set; }
            public List<long> colourid { get; set; }
        }
        public List<Product> BuildProducts(ProductCriteria criteria)
        {
            List<Product> results = new List<Product>();
            var param = new SortedList<string, object>();
            param.Add("locationid", locationid);
            param.Add("productid", criteria.productid);
            param.Add("designid", criteria.designid);
            param.Add("markerid", criteria.markerid);
            
            string addsql = "";
            if (criteria.productid > 0)
                addsql += " stock.productid = @productid";

            //if (criteria.markerid > 0)
            //    addsql += (addsql.Length > 0 ? " and " : "") + " stockmarker.id = @markerid";

            if(criteria.colourid != null &&
                criteria.colourid.Count>0)
            {
                addsql += (addsql.Length > 0 ? " and " : "") + " colourid in ("+String.Join(",", criteria.colourid)+")";
            }

            if (!string.IsNullOrEmpty(criteria.designid))
                addsql += (addsql.Length>0? " and ":"")+ " stock.designid = @designid";

            var sql = string.Format(productlist, (addsql.Length > 0 ? " and " : "")+ addsql);
            if (criteria.productid > 0)
                sql += " where products.id=@productid";

            using (var connection = CreateConnection())
            {
                connection.Open();
                using (var reader = ExecuteReader(sql, param))
                {
                    while (reader.Read())
                    {
                        Product oitems = new Product();
                        oitems.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        oitems.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        oitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oitems.style = reader["style"] == DBNull.Value ? "" : reader["style"].ToString();
                        oitems.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        oitems.pattern = reader["pattern"] == DBNull.Value ? "" : reader["pattern"].ToString();
                        oitems.code = reader["code"] == DBNull.Value ? "" : Convert.ToInt64(reader["code"]).ToString();
                        oitems.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        oitems.type = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitems.fit = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        oitems.sleeve = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        oitems.material = reader["material"] == DBNull.Value ? "" : reader["material"].ToString();
                        if (criteria.online)
                        {
                            oitems.stock = reader["olqty"] == DBNull.Value ? 0 : (int)Convert.ToDecimal(reader["olqty"]);
                            oitems.otherstock = reader["stock"] == DBNull.Value ? 0 : (int)Convert.ToDecimal(reader["stock"]);
                        }
                        else
                        {
                            oitems.stock = reader["stock"] == DBNull.Value ? 0 : (int)Convert.ToDecimal(reader["stock"]);
                            oitems.otherstock = reader["olqty"] == DBNull.Value ? 0 : (int)Convert.ToDecimal(reader["olqty"]);
                        }
                        oitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        oitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        if (oitems.mrp == 0 && oitems.price > 0)
                            oitems.mrp = oitems.price;

                        oitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oitems.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                        oitems.productgroup = reader["productsgroup"] == DBNull.Value ? "" : reader["productsgroup"].ToString();

                        oitems.image = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        if (!string.IsNullOrEmpty(oitems.image))
                        {
                            oitems.imagelist = new List<string>();
                            oitems.imagelist.Add(oitems.image);
                        }
                        oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        results.Add(oitems);
                    }
                }

                using (var reader = ExecuteReader(@"select productid,designid,imageid,colourid,referencelist.name colour from stockimage 
                            left outer join referencelist on referencelist.id = colourid", null))
                {
                    while (reader.Read())
                    {
                        long productid = reader.GetInt64(0);
                        string designid = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        string imageid = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        long colourid = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                        string colour = reader.IsDBNull(4) ? "": reader.GetString(4);
                        var item = results.Find(x => x.productid == productid && x.design == designid &&
                        x.colourid == colourid);
                        if (item != null)
                        {
                            if (item.imagelist == null)
                                item.imagelist = new List<string>();
                            item.imagelist.Add(imageid);
                        }
                    }
                }
                string host = InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver;
                foreach (var p in results)
                {
                    p.image = string.IsNullOrEmpty(p.image) ? "" : string.Format("{0}/File/Show/{2}/{1}", host, p.image, organizationid);
                    if (p.imagelist != null && p.imagelist.Count > 0)
                    {
                        p.imagelist = p.imagelist.ConvertAll(x =>
                        {
                            return string.IsNullOrEmpty(x) ? "" : string.Format("{0}/File/Show/{2}/{1}", host, x, organizationid);
                        });
                    }
                    p.catelogkey = Hash(string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        p.product, p.brand, p.design, p.material, p.pattern, p.style, p.type));
                    p.uniquekey = Hash(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}",
                        p.product, p.brand, p.code, p.colour, p.design, p.material, p.pattern, p.size, p.style, p.type));
                }
                connection.Close();
            }
            return results;
        }


        public byte[] Download(HttpRequestMessage request)
        {


            //this.locationid = location.First().id;
            var productlist = BuildCatelog();

            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Section", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("ProductGroup", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Product", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Brand", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Style", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Pattern", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Material", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Design", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Colour", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Size", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Sleeve", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Fit", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Code", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Barcode", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Stock", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("MRP", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Price", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Discount", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("CatelogKey", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("ItemKey", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("ImageURL", typeof(string)) });

            string host = string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver) ?
                request.RequestUri.AbsoluteUri.Substring(0, request.RequestUri.AbsoluteUri.IndexOf("/File/")) : InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver;


            foreach (var p in productlist)
            {
                var imageurl = string.IsNullOrEmpty(p.image) ? "" : string.Format("{0}/File/Show/{2}/{1}", host, p.image, organizationid);
                if (p.imagelist != null && p.imagelist.Count > 0)
                {
                    if (!string.IsNullOrEmpty(p.image))
                        p.imagelist.Add(p.image);
                    imageurl = string.Join(",", p.imagelist.ConvertAll(x => string.IsNullOrEmpty(x) ? "" : string.Format("{0}/File/Show/{2}/{1}", host, x, organizationid)));
                }
                dt.Rows.Add(new object[] { p.section,p.productgroup, p.product,p.brand,p.style,p.pattern,p.material,p.design,p.colour,p.size,
                p.sleeve,p.fit,  p.code,p.barcode,p.stock,p.mrp,p.price,p.discount,p.catelogkey,p.uniquekey,imageurl });
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, "Products");
                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public byte[] Download(string locationcode)
        {
            InfyPOS.Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
            var location = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                code = locationcode,
                type = "LOCATION"
            });
            if (location.Count != 1)
                throw new Exception("Can't find the data for the given location code");

            this.locationid = location.First().id;
            var productlist = BuildProducts(new ProductCriteria() { 
            
            });


            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Product", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Brand", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Style", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Pattern", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Material", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Design", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Colour", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Size", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Code", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Stock", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("MRP", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Price", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("Discount", typeof(decimal)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("CatelogKey", typeof(string)) });
            dt.Columns.AddRange(new DataColumn[1] { new DataColumn("ItemKey", typeof(string)) });
            ;
            foreach (var p in productlist)
            {
                dt.Rows.Add(new object[] { p.product,p.brand,p.style,p.pattern,p.material,p.design,p.colour,p.size,
                    p.code,p.stock,p.mrp,p.price,p.discount,p.catelogkey,p.uniquekey });
            }

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                var sheet = xLWorkbook.AddWorksheet(dt, "Products");
                xLWorkbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static string Hash(string value) => string.IsNullOrEmpty(value) ? value : SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(value)).Aggregate(new StringBuilder(), (sb, b) => sb.Append(b.ToString("X2"))).ToString();

        internal async Task Upload2QtKartAsync(ProcessManager.ProcessData request)
        {
            request.criteria.online = true;
            List<Product> products = BuildProducts(request.criteria);
             
            InfyPOS.Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.ecommerce != null)
            {
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var url = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.ecommerce.path;
                    using (var client = new HttpClient())
                    {
                        var content = new StringContent(JsonConvert.SerializeObject(products), Encoding.UTF8, "application/json");
                        var result = await client.PostAsync(url, content);
                        if (result.IsSuccessStatusCode)
                        {
                            request.progress.hascompleted = true;
                            request.progress.status = "Completed";
                        }
                        else
                        {
                            request.progress.status = result.Content.ReadAsStringAsync().Result;
                            request.progress.hascompleted = true;
                        }

                    }
                }catch(Exception exp)
                {
                    request.progress.status = exp.Message;
                    request.progress.hascompleted = true;
                }
            }
        }

        public class Product
        {
            public string colour { get; set; }
            public string size { get; set; }
            public string barcode { get; set; }
            public string fit { get; set; }
            public string image { get; set; }
            public List<string> imagelist { get; set; }
            public string sleeve { get; set; }
            public string style { get; set; }
            public string pattern { get; set; }
            public string material { get; set; }
            public string design { get; set; }
            public string section { get; set; }
            public string productgroup { get; set; }
            public string product { get; set; }
            public string brand { get; set; }
            public string type { get; set; }
            public string code { get; set; }
            public int stock { get; set; }
            public int otherstock { get; set; }
            public decimal price { get; set; }
            public decimal discount { get; set; }
            public decimal mrp { get; set; }
            public string uniquekey { get; set; }
            public string catelogkey { get; set; }
            [JsonIgnore]
            public long productid { get; set; }
            [JsonIgnore]
            public long colourid { get; set; }
        }

    }
}