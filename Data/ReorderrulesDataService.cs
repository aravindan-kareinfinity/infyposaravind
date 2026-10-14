using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ReorderrulesDataService : SAASData
    {
        const string SQL_Search = @"select reorderrules.id, reorderrules.isactive, reorderrules.createdby, reorderrules.modifiedby, 
                                    reorderrules.version, reorderrules.createdon, reorderrules.modifiedon, reorderrules.productid,
                                    reorderrules.locationid, reorderrules.supplierid, reorderrules.brandid, reorderrules.patternid, 
                                    reorderrules.styleid, reorderrules.colourid, reorderrules.materialid, reorderrules.typeid, 
                                    reorderrules.sizeid, fromrate, torate, minro, maxro,minorder,aprice, priority, description, 
                                    reorderrules.enable,reorderrules.coreid,reorderrules.designid,reorderrules.expirydays,onpurchaseprice from reorderrules
                                    left outer join products on products.id = productid 
                                    where id=@id";


        const string SQL_Insert = @"INSERT INTO public.reorderrules(isactive, createdby, modifiedby, version, createdon, modifiedon, productid, locationid, supplierid, brandid, patternid, styleid, colourid, materialid, typeid, sizeid, fromrate, torate, minro, maxro,minorder,aprice, priority, description, enable,coreid,designid,expirydays,onpurchaseprice) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @productid, @locationid, @supplierid, @brandid, @patternid, @styleid, @colourid, @materialid, @typeid, @sizeid, @fromrate, @torate, @minro, @maxro,@minorder,@aprice, @priority, @description, @enable,@coreid,@designid,@expirydays,@onpurchaseprice) RETURNING id";
        const string SQL_Update = @"UPDATE public.reorderrules SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, productid=@productid, locationid=@locationid, supplierid=@supplierid, brandid=@brandid, patternid=@patternid, styleid=@styleid, colourid=@colourid, materialid=@materialid, onpurchaseprice=@onpurchaseprice,
                                    typeid=@typeid, sizeid=@sizeid,designid=@designid, fromrate=@fromrate, torate=@torate, minro=@minro, maxro=@maxro,minorder=@minorder,aprice=@aprice, priority=@priority, description=@description, enable=@enable,coreid=@coreid,expirydays=@expirydays where id=@id and version=@version RETURNING version";

        internal List<ReorderrulesResult> ReportItems(ReorderrulesCreteria _reorderrules)
        {
            string retailquery = @"select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) as stock,salablegoods.locationid,stock.itemid from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    inner join items on items.id = stock.itemid and items.allowsearch = true and reordermin>0
                                    inner join products on products.id = stock.productid
                                    where salablegoods.isactive = true and stock.isactive = true 
                                    and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0";

            string stockquery = @"select sum(stock.qty - (stock.transfered+stock.journalqty+stock.returnqty+stock.damagedqty)) as stock,0::bigint as locationid,stock.itemid from stock
                                    inner join items on items.id = stock.itemid and items.allowsearch = true and reordermin>0
                                    inner join products on products.id = stock.productid
                                    where stock.isactive = true and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0";

            QueryBuilder retailqb = new QueryBuilder();
            QueryBuilder stockqb = new QueryBuilder();
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
            {
                stockqb.CurrentQuery = stockquery.Replace("and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", ""); ;
                retailqb.CurrentQuery = retailquery.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", ""); ;
            }
            else
            {
                stockqb.CurrentQuery = stockquery;
                retailqb.CurrentQuery = retailquery;
            }

            if (_reorderrules.productid != 0)
            {
                retailqb.AddParameter("stock.productid", _reorderrules.productid, "=");
                stockqb.AddParameter("stock.productid", _reorderrules.productid, "=");
            }
            if (_reorderrules.locationid != 0)
            {
                retailqb.AddParameter("salablegoods.locationid", _reorderrules.locationid, "=");
                stockqb.AddParameter("stock.locationid", _reorderrules.locationid, "=");
            }
            if (_reorderrules.sectionid != 0)
            {
                retailqb.AddParameter("products.salescounterid", _reorderrules.sectionid, "=");
                stockqb.AddParameter("products.salescounterid", _reorderrules.sectionid, "=");
            }
            if (_reorderrules.brandid != 0)
            {
                retailqb.AddParameter("items.brandid", _reorderrules.brandid, "=");
                stockqb.AddParameter("items.brandid", _reorderrules.brandid, "=");
            }
            if (_reorderrules.materialid != 0)
            {
                retailqb.AddParameter("items.materialid", _reorderrules.materialid, "=");
                stockqb.AddParameter("items.materialid", _reorderrules.materialid, "=");
            }
            if (_reorderrules.styleid != 0)
            {
                retailqb.AddParameter("items.styleid", _reorderrules.styleid, "=");
                stockqb.AddParameter("items.styleid", _reorderrules.styleid, "=");
            }
            if (_reorderrules.patternid != 0)
            {
                retailqb.AddParameter("items.patternid", _reorderrules.patternid, "=");
                stockqb.AddParameter("items.patternid", _reorderrules.patternid, "=");
            }
            if (_reorderrules.typeid != 0)
            {
                retailqb.AddParameter("items.typeid", _reorderrules.typeid, "=");
                stockqb.AddParameter("items.typeid", _reorderrules.typeid, "=");
            }
            if (_reorderrules.colourid != 0)
            {
                retailqb.AddParameter("items.colourid", _reorderrules.colourid, "=");
                stockqb.AddParameter("items.colourid", _reorderrules.colourid, "=");
            }
            if (_reorderrules.sizeid != 0)
            {
                retailqb.AddParameter("items.sizeid", _reorderrules.sizeid, "=");
                stockqb.AddParameter("items.sizeid", _reorderrules.sizeid, "=");
            }
            string fullQuery = string.Format(@"with fullquery as ({0} group by stock.itemid,salablegoods.locationid 
                                union all {1} group by stock.itemid),
                                 stocklist as (select sum(stock) stock,itemid from fullquery group by itemid)
                                 select items.id,items.printingname,reordermin,reordermax,stocklist.stock from stocklist
                                 inner join items on items.id = stocklist.itemid", retailqb.GetCommand(null).CommandText,
                                 stockqb.GetCommand(null).CommandText);

            var param = retailqb.Parameters;
            foreach (var item in stockqb.Parameters)
            {
                if (param.ContainsKey(item.Key)) continue;
                param.Add(item.Key, item.Value);
            }
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                fullQuery = fullQuery.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

            List<ReorderrulesResult> result = new List<ReorderrulesResult>();
            using (var reader = ExecuteReader(fullQuery, param))
            {
                while (reader.Read())
                {
                    result.Add(new ReorderrulesResult()
                    {
                        productname = reader["printingname"].ToString(),
                        minro = reader["reordermin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["reordermin"]),
                        stock = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]),
                        itemid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"])
                    });
                }
            }
            return result;
        }

        internal List<ReorderrulesResult> ReportOptimized(ReorderrulesCreteria item, List<Reorderrules> rules)
        {
            item.enable = true;
            System.Collections.Concurrent.ConcurrentQueue<ReorderrulesResult> results = new System.Collections.Concurrent.ConcurrentQueue<ReorderrulesResult>();

            ProductsDataService productData = new ProductsDataService() { Context = this.Context };
            List<Products> lstproducts = productData.Search(new ProductsCreteria() { });

            foreach (var rulegroup in rules.GroupBy(e => new { e.expirydays }))
            {
                string retailquery = @"select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)),salablegoods.locationid,{0} from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    inner join items on items.id = stock.itemid
                                    inner join products on products.id = stock.productid
                                    where salablegoods.isactive = true and stock.isactive = true 
                                    and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0 ";

                string stockquery = @"select sum(stock.qty - (stock.transfered+stock.journalqty+stock.returnqty+stock.damagedqty)),{0} from stock
                                    inner join items on items.id = stock.itemid
                                    inner join products on products.id = stock.productid
                                    where stock.isactive = true 
                                    and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0";

                string salequery = @"select sum(billitems.qty),bill.locationid,{0} from bill
                                    inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                    inner join salablegoods on salablegoods.id = billitems.salablegoodsid
                                    inner join stock on stock.id = salablegoods.stockid
                                    inner join items on items.id = stock.itemid
                                    inner join products on products.id = stock.productid
                                    where bill.isactive = true and bill.billtype != 9 ";
                QueryBuilder stockqb = new QueryBuilder();
                QueryBuilder salablegoodsqb = new QueryBuilder();
                QueryBuilder saleqb = new QueryBuilder();

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                {
                    stockqb.CurrentQuery = stockquery.Replace("and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", "");
                    salablegoodsqb.CurrentQuery = retailquery.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", "");
                }
                else
                {
                    stockqb.CurrentQuery = stockquery;
                    salablegoodsqb.CurrentQuery = retailquery;
                }
                saleqb.CurrentQuery = salequery;
                saleqb.AddParameter("bill.billdate", "fromdate", item.salefromdate, ">=");
                saleqb.AddParameter("bill.billdate", "todate", item.saletodate.Date.AddDays(1), "<");

                List<string> groupbyfields = new List<string>();
                List<string> groupbylist = new List<string>();
                if (item.companyid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                {
                    stockqb.AddParameter("stock.companyid", "companyid", item.companyid, "=");
                    salablegoodsqb.AddParameter("salablegoods.companyid", "companyid", item.companyid, "=");
                    saleqb.AddParameter("bill.companyid", "companyid", item.companyid, "=");
                }

                if (item.sectionid > 0)
                {
                    stockqb.AddParameter("products.salescounterid", "sectionid", item.sectionid, "=");
                    salablegoodsqb.AddParameter("products.salescounterid", "sectionid", item.sectionid, "=");
                    saleqb.AddParameter("products.salescounterid", "sectionid", item.sectionid, "=");
                }

                //if (item.locationid > 0)
                //{
                //    //stockqb.AddParameter("stock.locationid", "locationid", item.locationid, "=");
                //    salablegoodsqb.AddParameter("salablegoods.locationid", "locationid", item.locationid, "=");
                //    saleqb.AddParameter("bill.locationid", "locationid", item.locationid, "=");
                //}

                bool hasproductid = false;
                bool hasbrandid = false;
                bool hasmaterialid = false;
                bool hascolourid = false;
                bool haspatternid = false;
                bool hassizeid = false;
                bool hasstyleid = false;
                bool hastypeid = false;
                bool hassupplierid = false;
                bool hasdesignid = false;
                bool haspurchaseprice = false;
                bool hassalesrate = false;


                if (rulegroup.ToList().Exists(x => x.productid > 0))
                {
                    hasproductid = true;
                    groupbyfields.Add("stock.productid");
                    groupbylist.Add("stock.productid");
                    if (rulegroup.GroupBy(e => e.productid).Count() > 1)
                    {
                        if (!rulegroup.ToList().Exists(e => e.productid <= 0))
                        {
                            stockqb.AddInParamer("stock.productid", string.Join(",", rulegroup.GroupBy(e => e.productid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("stock.productid", string.Join(",", rulegroup.GroupBy(e => e.productid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("stock.productid", string.Join(",", rulegroup.GroupBy(e => e.productid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("stock.productid", "productid", rulegroup.First().productid, "=");
                        salablegoodsqb.AddParameter("stock.productid", "productid", rulegroup.First().productid, "=");
                        saleqb.AddParameter("stock.productid", "productid", rulegroup.First().productid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.brandid > 0))
                {
                    hasbrandid = true;
                    groupbyfields.Add("items.brandid");
                    groupbylist.Add("items.brandid");
                    if (rulegroup.GroupBy(e => e.brandid).Count() > 1)
                    {
                        if (!rulegroup.ToList().Exists(e => e.brandid <= 0))
                        {
                            stockqb.AddInParamer("items.brandid", string.Join(",", rulegroup.GroupBy(e => e.brandid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.brandid", string.Join(",", rulegroup.GroupBy(e => e.brandid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("items.brandid", string.Join(",", rulegroup.GroupBy(e => e.brandid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("items.brandid", "brandid", rulegroup.First().brandid, "=");
                        salablegoodsqb.AddParameter("items.brandid", "brandid", rulegroup.First().brandid, "=");
                        saleqb.AddParameter("items.brandid", "brandid", rulegroup.First().brandid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.materialid > 0))
                {
                    hasmaterialid = true;
                    groupbyfields.Add("items.materialid");
                    groupbylist.Add("items.materialid");
                    if (rulegroup.GroupBy(e => e.materialid).Count() > 1)
                    {
                        if (!rulegroup.ToList().Exists(e => e.materialid <= 0))
                        {
                            stockqb.AddInParamer("items.materialid", string.Join(",", rulegroup.GroupBy(e => e.materialid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.materialid", string.Join(",", rulegroup.GroupBy(e => e.materialid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("items.materialid", string.Join(",", rulegroup.GroupBy(e => e.materialid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("items.materialid", "materialid", rulegroup.First().materialid, "=");
                        salablegoodsqb.AddParameter("items.materialid", "materialid", rulegroup.First().materialid, "=");
                        saleqb.AddParameter("items.materialid", "materialid", rulegroup.First().materialid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.colourid > 0))
                {
                    hascolourid = true;
                    groupbyfields.Add("items.colourid");
                    groupbylist.Add("items.colourid");
                    if (rulegroup.GroupBy(e => e.colourid).Count() > 1)
                    {

                        if (!rulegroup.ToList().Exists(e => e.colourid <= 0))
                        {
                            stockqb.AddInParamer("items.colourid", string.Join(",", rulegroup.GroupBy(e => e.colourid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.colourid", string.Join(",", rulegroup.GroupBy(e => e.colourid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("items.colourid", string.Join(",", rulegroup.GroupBy(e => e.colourid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("items.colourid", "colourid", rulegroup.First().colourid, "=");
                        salablegoodsqb.AddParameter("items.colourid", "colourid", rulegroup.First().colourid, "=");
                        saleqb.AddParameter("items.colourid", "colourid", rulegroup.First().colourid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.patternid > 0))
                {
                    haspatternid = true;
                    groupbyfields.Add("items.patternid");
                    groupbylist.Add("items.patternid");
                    if (rulegroup.GroupBy(e => e.patternid).Count() > 1)
                    {

                        if (!rulegroup.ToList().Exists(e => e.patternid <= 0))
                        {
                            saleqb.AddInParamer("items.patternid", string.Join(",", rulegroup.GroupBy(e => e.patternid).ToList().ConvertAll(x => x.Key)));
                            stockqb.AddInParamer("items.patternid", string.Join(",", rulegroup.GroupBy(e => e.patternid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.patternid", string.Join(",", rulegroup.GroupBy(e => e.patternid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        saleqb.AddParameter("items.patternid", "patternid", rulegroup.First().patternid, "=");
                        stockqb.AddParameter("items.patternid", "patternid", rulegroup.First().patternid, "=");
                        salablegoodsqb.AddParameter("items.patternid", "patternid", rulegroup.First().patternid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.sizeid > 0))
                {
                    hassizeid = true;
                    groupbyfields.Add("items.sizeid");
                    groupbylist.Add("items.sizeid");
                    if (rulegroup.GroupBy(e => e.sizeid).Count() > 1)
                    {
                        if (!rulegroup.ToList().Exists(e => e.sizeid <= 0))
                        {
                            stockqb.AddInParamer("items.sizeid", string.Join(",", rulegroup.GroupBy(e => e.sizeid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.sizeid", string.Join(",", rulegroup.GroupBy(e => e.sizeid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("items.sizeid", string.Join(",", rulegroup.GroupBy(e => e.sizeid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("items.sizeid", "sizeid", rulegroup.First().sizeid, "=");
                        salablegoodsqb.AddParameter("items.sizeid", "sizeid", rulegroup.First().sizeid, "=");
                        saleqb.AddParameter("items.sizeid", "sizeid", rulegroup.First().sizeid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.styleid > 0))
                {
                    hasstyleid = true;
                    groupbyfields.Add("items.styleid");
                    groupbylist.Add("items.styleid");
                    if (rulegroup.GroupBy(e => e.styleid).Count() > 1)
                    {

                        if (!rulegroup.ToList().Exists(e => e.styleid <= 0))
                        {
                            stockqb.AddInParamer("items.styleid", string.Join(",", rulegroup.GroupBy(e => e.styleid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.styleid", string.Join(",", rulegroup.GroupBy(e => e.styleid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("items.styleid", string.Join(",", rulegroup.GroupBy(e => e.styleid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("items.styleid", "styleid", rulegroup.First().styleid, "=");
                        salablegoodsqb.AddParameter("items.styleid", "styleid", rulegroup.First().styleid, "=");
                        saleqb.AddParameter("items.styleid", "styleid", rulegroup.First().styleid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.typeid > 0))
                {
                    hastypeid = true;
                    groupbyfields.Add("items.typeid");
                    groupbylist.Add("items.typeid");
                    if (rulegroup.GroupBy(e => e.typeid).Count() > 1)
                    {

                        if (!rulegroup.ToList().Exists(e => e.typeid <= 0))
                        {
                            stockqb.AddInParamer("items.typeid", string.Join(",", rulegroup.GroupBy(e => e.typeid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("items.typeid", string.Join(",", rulegroup.GroupBy(e => e.typeid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("items.typeid", string.Join(",", rulegroup.GroupBy(e => e.typeid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("items.typeid", "typeid", rulegroup.First().typeid, "=");
                        salablegoodsqb.AddParameter("items.typeid", "typeid", rulegroup.First().typeid, "=");
                        saleqb.AddParameter("items.typeid", "typeid", rulegroup.First().typeid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => x.supplierid > 0))
                {
                    hassupplierid = true;
                    groupbyfields.Add("stock.supplierid");
                    groupbylist.Add("stock.supplierid");
                    if (rulegroup.GroupBy(e => e.supplierid).Count() > 1)
                    {
                        if (!rulegroup.ToList().Exists(e => e.supplierid <= 0))
                        {
                            stockqb.AddInParamer("stock.supplierid", string.Join(",", rulegroup.GroupBy(e => e.supplierid).ToList().ConvertAll(x => x.Key)));
                            salablegoodsqb.AddInParamer("stock.supplierid", string.Join(",", rulegroup.GroupBy(e => e.supplierid).ToList().ConvertAll(x => x.Key)));
                            saleqb.AddInParamer("stock.supplierid", string.Join(",", rulegroup.GroupBy(e => e.supplierid).ToList().ConvertAll(x => x.Key)));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("stock.supplierid", "supplierid", rulegroup.First().supplierid, "=");
                        salablegoodsqb.AddParameter("stock.supplierid", "supplierid", rulegroup.First().supplierid, "=");
                        saleqb.AddParameter("stock.supplierid", "supplierid", rulegroup.First().supplierid, "=");
                    }
                }

                if (rulegroup.ToList().Exists(x => !string.IsNullOrEmpty(x.designid)))
                {
                    hasdesignid = true;
                    groupbyfields.Add("stock.designid");
                    groupbylist.Add("stock.designid");
                    if (rulegroup.GroupBy(e => !string.IsNullOrEmpty(e.designid)).Count() > 1)
                    {
                        if (!rulegroup.ToList().Exists(e => string.IsNullOrEmpty(e.designid)))
                        {
                            stockqb.AddInParamer("stock.designid", string.Join(",", rulegroup.GroupBy(e => e.designid).ToList().ConvertAll(x => "'" + x.Key + "'")));
                            salablegoodsqb.AddInParamer("stock.designid", string.Join(",", rulegroup.GroupBy(e => e.designid).ToList().ConvertAll(x => "'" + x.Key + "'")));
                            saleqb.AddInParamer("stock.designid", string.Join(",", rulegroup.GroupBy(e => e.designid).ToList().ConvertAll(x => "'" + x.Key + "'")));
                        }
                    }
                    else
                    {
                        stockqb.AddParameter("stock.designid", "designid", rulegroup.First().designid, "=");
                        salablegoodsqb.AddParameter("stock.designid", "designin", rulegroup.First().designid, "=");
                        saleqb.AddParameter("stock.designid", "designin", rulegroup.First().designid, "=");
                    }
                }

                if (rulegroup.Key.expirydays > 0)
                {
                    stockqb.AddParameter("EXTRACT(day from @current_date - stock.stockdate)", "age", rulegroup.Key.expirydays, ">=");
                    salablegoodsqb.AddParameter("EXTRACT(day from @current_date - stock.stockdate)", "age", rulegroup.Key.expirydays, ">=");
                    saleqb.AddParameter("EXTRACT(day from @current_date - stock.stockdate)", "age", rulegroup.Key.expirydays, ">=");
                }

                if (rulegroup.ToList().Exists(x => x.fromrate > 0 || x.torate > 0))
                {
                    if (rulegroup.GroupBy(x => x.fromrate > 0 || x.torate > 0).Count() > 1)
                    {
                        if (rulegroup.ToList().Exists(e => e.onpurchaseprice))
                        {
                            haspurchaseprice = true;
                        }
                        if (rulegroup.ToList().Exists(e => !e.onpurchaseprice))
                        {
                            hassalesrate = true;
                        }
                    }
                    else
                    {
                        var rule = rulegroup.First();
                        if (rule.onpurchaseprice)
                        {
                            haspurchaseprice = true;
                        }
                        if (!rule.onpurchaseprice)
                        {
                            hassalesrate = true;
                        }
                        //if (rule.onpurchaseprice)
                        //{
                        //    stockqb.AddParameter("stock.buyingprice", "fromrate", rule.fromrate, ">=");
                        //    salablegoodsqb.AddParameter("stock.buyingprice", "fromrate", rule.fromrate, ">=");

                        //    stockqb.AddParameter("stock.buyingprice", "torate", rule.torate, "<=");
                        //    salablegoodsqb.AddParameter("stock.buyingprice", "torate", rule.torate, "<=");
                        //}
                        //else
                        //{
                        //    stockqb.AddParameter("(stock.price-stock.discount)", "fromrate", rule.fromrate, ">=");
                        //    salablegoodsqb.AddParameter("(salablegoods.price-salablegoods.discount)", "fromrate", rule.fromrate, ">=");

                        //    stockqb.AddParameter("(stock.price-stock.discount)", "torate", rule.torate, "<=");
                        //    salablegoodsqb.AddParameter("(salablegoods.price-salablegoods.discount)", "torate", rule.torate, "<=");
                        //}

                    }
                }


                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var command = stockqb.GetCommand(connection);

                    if (haspurchaseprice)
                    {
                        groupbyfields.Add("stock.buyingprice prate");
                        groupbylist.Add("stock.buyingprice");
                    }

                    if (hassalesrate)
                    {
                        groupbyfields.Add("stock.price srate");
                        groupbylist.Add("stock.price");
                    }

                    if (groupbyfields.Count > 0)
                    {
                        command.CommandText = string.Format(command.CommandText, string.Join(",", groupbyfields)) + " group by " + string.Join(",", groupbylist);
                    }
                    else
                    {
                        groupbyfields.Add("stock.productid");
                        groupbylist.Add("stock.productid");
                        command.CommandText = string.Format(command.CommandText, string.Join(",", groupbyfields)) + " group by " + string.Join(",", groupbylist);
                    }
                    command.Parameters.AddWithValue("current_date", DateTimeNow.Date);

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    List<ReorderGroupItem> allresult = new List<ReorderGroupItem>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReorderGroupItem gri = new ReorderGroupItem();
                            gri.stock = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            gri.productid = hasproductid ? (reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"])) : 0;
                            gri.brandid = hasbrandid ? (reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"])) : 0;
                            gri.colourid = hascolourid ? (reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"])) : 0;
                            gri.designid = hasdesignid ? (reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"])) : "";
                            gri.materialid = hasmaterialid ? (reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"])) : 0;
                            gri.patternid = haspatternid ? (reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"])) : 0;
                            gri.purchaseprice = haspurchaseprice ? (reader["prate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prate"])) : 0;
                            gri.salesrate = hassalesrate ? (reader["srate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["srate"])) : 0;
                            gri.sizeid = hassizeid ? (reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"])) : 0;
                            gri.styleid = hasstyleid ? (reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"])) : 0;
                            gri.supplierid = hassupplierid ? (reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"])) : 0;
                            gri.typeid = hastypeid ? (reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"])) : 0;
                            allresult.Add(gri);
                        }
                    }


                    if (hassalesrate)
                    {
                        groupbyfields.Remove("stock.price srate");
                        groupbylist.Remove("stock.price");
                        groupbyfields.Add("(salablegoods.price-salablegoods.discount) srate");
                        groupbylist.Add("(salablegoods.price-salablegoods.discount)");
                    }

                    command = salablegoodsqb.GetCommand(connection);
                    if (groupbyfields.Count > 0)
                    {
                        command.CommandText = string.Format(command.CommandText, string.Join(",", groupbyfields)) + " group by salablegoods.locationid," + string.Join(",", groupbylist);
                    }
                    else
                    {
                        command.CommandText += " group by salablegoods.locationid";
                    }
                    command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                        command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReorderGroupItem gri = new ReorderGroupItem();
                            gri.stock = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            gri.locationid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            gri.productid = hasproductid ? (reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"])) : 0;
                            gri.brandid = hasbrandid ? (reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"])) : 0;
                            gri.colourid = hascolourid ? (reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"])) : 0;
                            gri.designid = hasdesignid ? (reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"])) : "";
                            gri.materialid = hasmaterialid ? (reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"])) : 0;
                            gri.patternid = haspatternid ? (reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"])) : 0;
                            gri.purchaseprice = haspurchaseprice ? (reader["prate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prate"])) : 0;
                            gri.salesrate = hassalesrate ? (reader["srate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["srate"])) : 0;
                            gri.sizeid = hassizeid ? (reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"])) : 0;
                            gri.styleid = hasstyleid ? (reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"])) : 0;
                            gri.supplierid = hassupplierid ? (reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"])) : 0;
                            gri.typeid = hastypeid ? (reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"])) : 0;
                            allresult.Add(gri);
                        }
                    }

                    if (item.salerange)
                    {
                        command = saleqb.GetCommand(connection);
                        if (groupbyfields.Count > 0)
                        {
                            command.CommandText = string.Format(command.CommandText, string.Join(",", groupbyfields)) + " group by bill.locationid," + string.Join(",", groupbylist);
                        }
                        else
                        {
                            command.CommandText += " group by bill.locationid";
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                            command.CommandText = command.CommandText.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");
                        command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ReorderGroupItem gri = new ReorderGroupItem();
                                gri.sold = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                gri.locationid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                                gri.productid = hasproductid ? (reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"])) : 0;
                                gri.brandid = hasbrandid ? (reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"])) : 0;
                                gri.colourid = hascolourid ? (reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"])) : 0;
                                gri.designid = hasdesignid ? (reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"])) : "";
                                gri.materialid = hasmaterialid ? (reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"])) : 0;
                                gri.patternid = haspatternid ? (reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"])) : 0;
                                gri.purchaseprice = haspurchaseprice ? (reader["prate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prate"])) : 0;
                                gri.salesrate = hassalesrate ? (reader["srate"] == DBNull.Value ? 0 : Convert.ToInt64(reader["srate"])) : 0;
                                gri.sizeid = hassizeid ? (reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"])) : 0;
                                gri.styleid = hasstyleid ? (reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"])) : 0;
                                gri.supplierid = hassupplierid ? (reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"])) : 0;
                                gri.typeid = hastypeid ? (reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"])) : 0;
                                allresult.Add(gri);
                            }
                        }
                    }

                    foreach (var rule in rulegroup)
                    {
                        ReorderrulesResult reorderrulesResult = new ReorderrulesResult()
                        {
                            stock = 0,
                            retailstock = 0,
                            otherretailstock = 0,
                            title = rule.description,
                            productid = rule.productid,
                            productname = (lstproducts.Exists(e => e.id == rule.productid)) ? lstproducts.Find(e => e.id == rule.productid).name : "",
                            locationid = rule.locationid,
                            brandid = rule.brandid,
                            patternid = rule.patternid,
                            styleid = rule.styleid,
                            colourid = rule.colourid,
                            materialid = rule.materialid,
                            typeid = rule.typeid,
                            minorder = rule.minorder,
                            aprice = rule.aprice,
                            minro = rule.minro,
                            maxro = rule.maxro,
                            sizeid = rule.sizeid,
                            onpurchaseprice = rule.onpurchaseprice,
                            expect = string.Format("[{0}-{1}]", rule.minro, rule.maxro),
                            pricerange = string.Format("[{0}-{1}]", rule.fromrate, rule.torate),
                            status = ((ReorderrulesResult.Priority)rule.priority).ToString(),
                            supplierid = rule.supplierid,
                            designid = rule.designid
                        };

                        var attribfilterd = allresult.FindAll(e =>
                            (rule.productid == 0 || e.productid == rule.productid) &&
                            (rule.brandid == 0 || e.brandid == rule.brandid) &&
                             (rule.colourid == 0 || e.colourid == rule.colourid) &&
                             (string.IsNullOrEmpty(rule.designid) || e.designid == rule.designid) &&
                             (rule.materialid == 0 || e.materialid == rule.materialid) &&
                             (rule.patternid == 0 || e.patternid == rule.patternid) &&
                             (rule.styleid == 0 || e.styleid == rule.styleid) &&
                             (rule.supplierid == 0 || e.supplierid == rule.supplierid) &&
                             (rule.typeid == 0 || e.typeid == rule.typeid) &&
                             (rule.patternid == 0 || e.patternid == rule.patternid) &&
                             (rule.sizeid == 0 || e.sizeid == rule.sizeid));

                        if (rule.fromrate != 0 && rule.torate != 0)
                        {
                            if (rule.onpurchaseprice)
                            {
                                attribfilterd = attribfilterd.FindAll(x => x.purchaseprice >= rule.fromrate && x.purchaseprice <= rule.torate);
                            }
                            else
                            {
                                attribfilterd = attribfilterd.FindAll(x => x.salesrate >= rule.fromrate && x.salesrate <= rule.torate);
                            }
                        }
                        else if (rule.fromrate != 0)
                        {
                            if (rule.onpurchaseprice)
                            {
                                attribfilterd = attribfilterd.FindAll(x => x.purchaseprice >= rule.fromrate);
                            }
                            else
                            {
                                attribfilterd = attribfilterd.FindAll(x => x.salesrate >= rule.fromrate);
                            }
                        }
                        else if (rule.torate != 0)
                        {
                            if (rule.onpurchaseprice)
                            {
                                attribfilterd = attribfilterd.FindAll(x => x.purchaseprice <= rule.torate);
                            }
                            else
                            {
                                attribfilterd = attribfilterd.FindAll(x => x.salesrate <= rule.torate);
                            }
                        }

                        if (rule.locationid > 0)
                        {
                            reorderrulesResult.stock = attribfilterd.Sum(e => e.locationid == 0 ? e.stock : 0);
                            reorderrulesResult.retailstock = attribfilterd.Sum(e => e.locationid == rule.locationid ? e.stock : 0);
                            reorderrulesResult.otherretailstock = attribfilterd.Sum(e => e.locationid != 0 && e.locationid != rule.locationid ? e.stock : 0);
                            reorderrulesResult.sold = attribfilterd.Sum(e => e.locationid == rule.locationid ? e.sold : 0);
                            reorderrulesResult.othersold = attribfilterd.Sum(e => e.locationid != rule.locationid ? e.sold : 0);
                        }
                        else
                        {
                            reorderrulesResult.stock = attribfilterd.Sum(e => e.locationid == 0 ? e.stock : 0);
                            reorderrulesResult.retailstock = attribfilterd.Sum(e => e.locationid != 0 ? e.stock : 0);
                            reorderrulesResult.othersold = attribfilterd.Sum(e => e.locationid != 0 ? e.sold : 0);
                        }

                        if (rule.locationid == -1)
                        {
                            if (item.showall || rule.minro > (reorderrulesResult.stock - reorderrulesResult.tsold)
                                || rule.maxro < (reorderrulesResult.stock - reorderrulesResult.tsold))
                            {
                                if (rule.minro > (reorderrulesResult.stock - reorderrulesResult.tsold) || rule.maxro < (reorderrulesResult.stock - reorderrulesResult.tsold))
                                {
                                    reorderrulesResult.stockstatus = rule.minro > (reorderrulesResult.stock - reorderrulesResult.tsold) ? "LOW" : "HIGH";
                                    if (rule.minro - reorderrulesResult.stock > 0)
                                    {
                                        reorderrulesResult.shortage = rule.minro - (reorderrulesResult.stock - reorderrulesResult.tsold);
                                        reorderrulesResult.orderqty = rule.minorder;
                                    }
                                }
                                else
                                {
                                    reorderrulesResult.stockstatus = "NORMAL";
                                    reorderrulesResult.status = "NORMAL";
                                }
                                results.Enqueue(reorderrulesResult);
                            }
                        }
                        else if (rule.locationid == -2)
                        {
                            if (item.showall || rule.minro >
                                (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold)
                                || rule.maxro < (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold))
                            {
                                if (rule.minro > (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold)
                                    || rule.maxro < (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold))
                                {
                                    reorderrulesResult.stockstatus = rule.minro >
                                        (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold) ? "LOW" : "HIGH";
                                    if (rule.minro - (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold) > 0)
                                    {
                                        reorderrulesResult.shortage = rule.minro - (reorderrulesResult.stock + reorderrulesResult.otherretailstock - reorderrulesResult.tsold);
                                        reorderrulesResult.orderqty = rule.minorder;
                                    }
                                }
                                else
                                {
                                    reorderrulesResult.stockstatus = "NORMAL";
                                    reorderrulesResult.status = "NORMAL";
                                }
                                results.Enqueue(reorderrulesResult);
                            }
                        }
                        else if (rule.locationid == -3)
                        {
                            if (item.showall || rule.minro > (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold) || rule.maxro < (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold))
                            {
                                if (rule.minro > (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold) || rule.maxro < (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold))
                                {
                                    reorderrulesResult.stockstatus = rule.minro > (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold) ? "LOW" : "HIGH";
                                    if (rule.minro - (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold) > 0)
                                    {
                                        reorderrulesResult.shortage = rule.minro - (reorderrulesResult.stock +
                                reorderrulesResult.retailstock +
                                reorderrulesResult.otherretailstock - reorderrulesResult.tsold);
                                        reorderrulesResult.orderqty = rule.minorder;
                                    }
                                }
                                else
                                {
                                    reorderrulesResult.stockstatus = "NORMAL";
                                    reorderrulesResult.status = "NORMAL";
                                }
                                results.Enqueue(reorderrulesResult);
                            }
                        }
                        else
                        {
                            if (item.showall || rule.minro > reorderrulesResult.retailstock - reorderrulesResult.sold || rule.maxro < reorderrulesResult.retailstock - reorderrulesResult.sold)
                            {
                                if (rule.minro > reorderrulesResult.retailstock - reorderrulesResult.sold || rule.maxro < reorderrulesResult.retailstock - reorderrulesResult.sold)
                                {
                                    reorderrulesResult.stockstatus = rule.minro > reorderrulesResult.retailstock - reorderrulesResult.sold ? "LOW" : "HIGH";
                                    if (rule.minro - reorderrulesResult.retailstock - reorderrulesResult.sold > 0)
                                    {
                                        reorderrulesResult.shortage = rule.minro - reorderrulesResult.retailstock - reorderrulesResult.sold;
                                        reorderrulesResult.orderqty = rule.minorder;
                                    }
                                }
                                else
                                {
                                    reorderrulesResult.stockstatus = "NORMAL";
                                    reorderrulesResult.status = "NORMAL";
                                }
                                results.Enqueue(reorderrulesResult);
                            }
                            //else if (item.showall || rule.minro == reorderrulesResult.retailstock || rule.maxro == reorderrulesResult.retailstock)
                            //{
                            //    reorderrulesResult.stockstatus = rule.minro == reorderrulesResult.retailstock ? "LOW" : "HIGH";
                            //    if (rule.minro - reorderrulesResult.retailstock > 0)
                            //    {
                            //        reorderrulesResult.shortage = rule.minro - reorderrulesResult.retailstock;
                            //        reorderrulesResult.orderqty = rule.minorder;
                            //    }
                            //    results.Enqueue(reorderrulesResult);
                            //}
                        }
                    }
                }
            }
            if (results.ToList().Exists(e => e.stockstatus == "HIGH"))
            {
                foreach(ReorderrulesResult oitem in results.ToList())
                {
                    if (oitem.stockstatus == "HIGH")
                        oitem.excess = oitem.retailstock - (oitem.maxro > 0 ? oitem.maxro : oitem.minro);
                }
            }
            return results.ToList().OrderBy(e => e.productid).ToList();
        }
        internal List<ReorderrulesResult> Report(ReorderrulesCreteria item)
        {
            item.enable = true;
            List<ReorderrulesResult> results = new List<ReorderrulesResult>();
            var rules = Search(item);
            results.AddRange(ReportOptimized(item, rules));
            //var rulesfiltered = rules.FindAll(e => e.productid == 0);
            //var rulesbyproducts = rules.FindAll(e => e.productid != 0);
            //if (rulesfiltered.Count > 0)
            //{
            //    results.AddRange(ReportFilterd(item, rulesfiltered));
            //}
            //if (rulesbyproducts.Count > 0)
            //{
            //    results.AddRange(ReportByProducts(item, rulesbyproducts));
            //}
            return results;
        }
        internal List<ReorderrulesResult> ReportFilterd(ReorderrulesCreteria item, List<Reorderrules> rules)
        {
            System.Collections.Concurrent.ConcurrentQueue<ReorderrulesResult> results = new System.Collections.Concurrent.ConcurrentQueue<ReorderrulesResult>();


            ProductsDataService productData = new ProductsDataService() { Context = this.Context };
            List<Products> lstproducts = productData.Search(new ProductsCreteria() { });

            //System.Threading.Tasks.Parallel.ForEach(rules, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 5 }, rule =>
            //{
            foreach (var rule in rules)
            {
                string retailquery = @"select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)),salablegoods.locationid from salablegoods
                                    inner join stock on stock.id = salablegoods.stockid
                                    inner join items on items.id = stock.itemid
                                    inner join products on products.id = stock.productid
                                    where salablegoods.isactive = true and stock.isactive = true 
                                    and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0";

                string stockquery = @"select sum(stock.qty - (stock.transfered+stock.journalqty+stock.returnqty+stock.damagedqty)) from stock
                                    inner join items on items.id = stock.itemid
                                    inner join products on products.id = stock.productid
                                    where stock.isactive = true and stock.hasstock = true and (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0";

                QueryBuilder stockqb = new QueryBuilder();
                QueryBuilder salablegoodsqb = new QueryBuilder();

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_withzerostock)
                {
                    stockqb.CurrentQuery = stockquery.Replace("and stock.hasstock = true and stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0", ""); ;
                    salablegoodsqb.CurrentQuery = retailquery.Replace("and salablegoods.hasstock = true and salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)>0", ""); ;
                }
                else
                {
                    stockqb.CurrentQuery = stockquery;
                    salablegoodsqb.CurrentQuery = retailquery;
                }

                if (item.companyid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                {
                    stockqb.AddParameter("stock.companyid", "companyid", item.companyid, "=");
                    salablegoodsqb.AddParameter("salablegoods.companyid", "companyid", item.companyid, "=");
                }


                if (rule.productid == -1)
                {
                    stockqb.AddParameter("isrefill", true, "=");
                    salablegoodsqb.AddParameter("isrefill", true, "=");
                }
                else if (rule.productid == -2)
                {
                    stockqb.AddParameter("isrefill", false, "=");
                    salablegoodsqb.AddParameter("isrefill", false, "=");
                }
                else
                {
                    stockqb.AddParameter("stock.productid", "productid", rule.productid, " = ");
                    salablegoodsqb.AddParameter("stock.productid", "productid", rule.productid, " = ");
                }
                if (rule.brandid > 0)
                {
                    stockqb.AddParameter("items.brandid", "brandid", rule.brandid, "=");
                    salablegoodsqb.AddParameter("items.brandid", "brandid", rule.brandid, "=");
                }
                if (rule.materialid > 0)
                {
                    stockqb.AddParameter("items.materialid", "materialid", rule.materialid, "=");
                    salablegoodsqb.AddParameter("items.materialid", "materialid", rule.materialid, "=");
                }
                if (item.sectionid > 0)
                {
                    stockqb.AddParameter("products.salescounterid", "sectionid", item.sectionid, "=");
                    salablegoodsqb.AddParameter("products.salescounterid", "sectionid", item.sectionid, "=");
                }
                if (rule.colourid > 0)
                {
                    stockqb.AddParameter("items.colourid", "colourid", rule.colourid, "=");
                    salablegoodsqb.AddParameter("items.colourid", "colourid", rule.colourid, "=");
                }
                if (rule.patternid > 0)
                {
                    stockqb.AddParameter("items.patternid", "patternid", rule.patternid, "=");
                    salablegoodsqb.AddParameter("items.patternid", "patternid", rule.patternid, "=");
                }
                if (rule.sizeid > 0)
                {
                    stockqb.AddParameter("items.sizeid", "sizeid", rule.sizeid, "=");
                    salablegoodsqb.AddParameter("items.sizeid", "sizeid", rule.sizeid, "=");
                }
                if (rule.styleid > 0)
                {
                    stockqb.AddParameter("items.styleid", "styleid", rule.styleid, "=");
                    salablegoodsqb.AddParameter("items.styleid", "styleid", rule.styleid, "=");
                }
                if (rule.supplierid > 0)
                {
                    stockqb.AddParameter("stock.supplierid", "supplierid", rule.supplierid, "=");
                    salablegoodsqb.AddParameter("stock.supplierid", "supplierid", rule.supplierid, "=");
                }
                if (rule.typeid > 0)
                {
                    stockqb.AddParameter("items.typeid", "typeid", rule.typeid, "=");
                    salablegoodsqb.AddParameter("items.typeid", "typeid", rule.typeid, "=");
                }
                if (rule.fromrate > 0)
                {
                    if (rule.onpurchaseprice)
                    {
                        stockqb.AddParameter("stock.buyingprice", "fromrate", rule.fromrate, ">=");
                        salablegoodsqb.AddParameter("stock.buyingprice", "fromrate", rule.fromrate, ">=");
                    }
                    else
                    {
                        stockqb.AddParameter("(stock.price-stock.discount)", "fromrate", rule.fromrate, ">=");
                        salablegoodsqb.AddParameter("(salablegoods.price-salablegoods.discount)", "fromrate", rule.fromrate, ">=");
                    }
                }

                if (rule.torate > 0)
                {
                    if (rule.onpurchaseprice)
                    {
                        stockqb.AddParameter("stock.buyingprice", "torate", rule.torate, "<=");
                        salablegoodsqb.AddParameter("stock.buyingprice", "torate", rule.torate, "<=");

                    }
                    else
                    {
                        stockqb.AddParameter("(stock.price-stock.discount)", "torate", rule.torate, "<=");
                        salablegoodsqb.AddParameter("(salablegoods.price-salablegoods.discount)", "torate", rule.torate, "<=");
                    }
                }

                if (rule.expirydays > 0)
                {
                    stockqb.AddParameter("EXTRACT(day from @current_date - stock.stockdate)", "age", rule.expirydays, ">=");
                    salablegoodsqb.AddParameter("EXTRACT(day from @current_date - stock.stockdate)", "age", rule.expirydays, ">=");
                }

                if (!string.IsNullOrEmpty(rule.designid))
                {
                    stockqb.AddParameter("stock.designid", rule.designid, "=");
                    salablegoodsqb.AddParameter("stock.designid", rule.designid, "=");
                }

                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var command = stockqb.GetCommand(connection);
                    command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                    ReorderrulesResult reorderrulesResult = new ReorderrulesResult()
                    {
                        stock = 0,
                        retailstock = 0,
                        otherretailstock = 0,
                        title = rule.description,
                        productid = rule.productid,
                        productname = (lstproducts.Exists(e => e.id == rule.productid)) ? lstproducts.Find(e => e.id == rule.productid).name : "",
                        locationid = rule.locationid,
                        brandid = rule.brandid,
                        patternid = rule.patternid,
                        styleid = rule.styleid,
                        colourid = rule.colourid,
                        materialid = rule.materialid,
                        typeid = rule.typeid,
                        minorder = rule.minorder,
                        aprice = rule.aprice,
                        minro = rule.minro,
                        sizeid = rule.sizeid,
                        onpurchaseprice = rule.onpurchaseprice,
                        expect = string.Format("[{0}-{1}]", rule.minro, rule.maxro),
                        pricerange = string.Format("[{0}-{1}]", rule.fromrate, rule.torate),
                        status = ((ReorderrulesResult.Priority)rule.priority).ToString(),
                        supplierid = rule.supplierid,
                        designid = rule.designid
                    };

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            decimal stock = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);

                            reorderrulesResult.stock = stock;

                            //}
                        }
                    }
                    command = salablegoodsqb.GetCommand(connection);
                    command.CommandText += " group by salablegoods.locationid";
                    command.Parameters.AddWithValue("current_date", DateTimeNow.Date);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal stock = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            long locationid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
                            if (rule.locationid > 0)
                            {
                                if (rule.locationid == locationid)
                                    reorderrulesResult.retailstock = stock;
                                else
                                    reorderrulesResult.otherretailstock += stock;
                            }
                            else
                            {
                                reorderrulesResult.retailstock += stock;
                            }
                        }
                    }

                    /*            results.push({ 'name': 'All Warehouse Location', 'id': -1 });
            results.push({ 'name': 'All Retail Location', 'id': -2 });
            results.push({ 'name': 'All Stock', 'id': -3 });*/
                    if (rule.locationid == -1)
                    {
                        if (item.showall || rule.minro > reorderrulesResult.stock || rule.maxro < reorderrulesResult.stock)
                        {
                            reorderrulesResult.stockstatus = rule.minro > reorderrulesResult.stock ? "LOW" : "HIGH";
                            if (rule.minro - reorderrulesResult.stock > 0)
                            {
                                reorderrulesResult.shortage = rule.minro - reorderrulesResult.stock;
                                reorderrulesResult.orderqty = rule.minorder;
                            }
                            results.Enqueue(reorderrulesResult);
                        }
                    }
                    else if (rule.locationid == -2)
                    {
                        if (item.showall || rule.minro > reorderrulesResult.retailstock + reorderrulesResult.otherretailstock || rule.maxro < reorderrulesResult.retailstock + reorderrulesResult.otherretailstock)
                        {
                            reorderrulesResult.stockstatus = rule.minro > reorderrulesResult.retailstock + reorderrulesResult.otherretailstock ? "LOW" : "HIGH";
                            if (rule.minro - (reorderrulesResult.retailstock + reorderrulesResult.otherretailstock) > 0)
                            {
                                reorderrulesResult.shortage = rule.minro - (reorderrulesResult.retailstock + reorderrulesResult.otherretailstock);
                                reorderrulesResult.orderqty = rule.minorder;
                            }
                            results.Enqueue(reorderrulesResult);
                        }
                    }
                    else if (rule.locationid == -3)
                    {
                        if (item.showall || rule.minro > reorderrulesResult.stock + reorderrulesResult.retailstock + reorderrulesResult.retailstock + reorderrulesResult.otherretailstock || rule.maxro < reorderrulesResult.stock + reorderrulesResult.retailstock + reorderrulesResult.retailstock + reorderrulesResult.otherretailstock)
                        {
                            reorderrulesResult.stockstatus = rule.minro > reorderrulesResult.stock + reorderrulesResult.retailstock + reorderrulesResult.otherretailstock ? "LOW" : "HIGH";
                            if (rule.minro - (reorderrulesResult.stock + reorderrulesResult.retailstock + reorderrulesResult.otherretailstock) > 0)
                            {
                                reorderrulesResult.shortage = rule.minro - (reorderrulesResult.stock + reorderrulesResult.retailstock + reorderrulesResult.otherretailstock);
                                reorderrulesResult.orderqty = rule.minorder;
                            }
                            results.Enqueue(reorderrulesResult);
                        }
                    }
                    else
                    {
                        if (item.showall || rule.minro > reorderrulesResult.retailstock || rule.maxro < reorderrulesResult.retailstock)
                        {
                            reorderrulesResult.stockstatus = rule.minro > reorderrulesResult.retailstock ? "LOW" : "HIGH";
                            if (rule.minro - reorderrulesResult.retailstock > 0)
                            {
                                reorderrulesResult.shortage = rule.minro - reorderrulesResult.retailstock;
                                reorderrulesResult.orderqty = rule.minorder;
                            }
                            results.Enqueue(reorderrulesResult);
                        }
                        //else if (item.showall || rule.minro == reorderrulesResult.retailstock || rule.maxro == reorderrulesResult.retailstock)
                        //{
                        //    reorderrulesResult.stockstatus = rule.minro == reorderrulesResult.retailstock ? "LOW" : "HIGH";
                        //    if (rule.minro - reorderrulesResult.retailstock > 0)
                        //    {
                        //        reorderrulesResult.shortage = rule.minro - reorderrulesResult.retailstock;
                        //        reorderrulesResult.orderqty = rule.minorder;
                        //    }
                        //    results.Enqueue(reorderrulesResult);
                        //}
                    }
                }
            }
            //});
            return results.ToList().OrderBy(e => e.productid).ToList();
        }

        internal ReorderMatrixSummary SearchMatrix(ReorderrulesCreteria item)
        {
            ProductsDataService SupplierData = new ProductsDataService() { Context = this.Context };
            ProductsBase result = SupplierData.SearchList(new ProductsCreteria()
            {
                id = item.productid
            }).First();

            if (result.attributelist == null)
                result.attributelist = new Products.ProductAttributesList();

            ReorderMatrixSummary response = new ReorderMatrixSummary();
            response.DataSource = new List<ReorderMatrixSummary.Data>();
            if (item.matrixconfig.location || item.matrixconfig.column == "Location")
            {
                string query = @"select id,name from referencelist	 where type ='LOCATION' AND isactive=true
                and cast(additionalinfo->>'iswarehouse' as boolean)=false";
                response.DataSource.Add(BuildMatrix(item.productid, query, "location"));
            }
            if (item.matrixconfig.brand || item.matrixconfig.column == "Brand")
            {
                string query = @"select id,name from brand where id in (
                select distinct brandid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.brand != null && result.attributelist.brand.Count > 0)
                {
                    query = string.Format(@"select id,name from brand where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.brand));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "brand"));
            }
            if (item.matrixconfig.size || item.matrixconfig.column == "Size")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct sizeid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.size != null && result.attributelist.size.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.size));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "size"));
            }
            if (item.matrixconfig.colour || item.matrixconfig.column == "Colour")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct colourid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.colour != null && result.attributelist.colour.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.colour));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "colour"));
            }
            if (item.matrixconfig.fit || item.matrixconfig.column == "Fit")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct fitid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.fit != null && result.attributelist.fit.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.fit));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "fit"));
            }
            if (item.matrixconfig.sleeve || item.matrixconfig.column == "Sleeve")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct sleeveid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.sleeve != null && result.attributelist.sleeve.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.sleeve));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "sleeve"));
            }
            if (item.matrixconfig.material || item.matrixconfig.column == "Material")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct materialid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.material != null && result.attributelist.material.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.material));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "material"));
            }
            if (item.matrixconfig.type || item.matrixconfig.column == "Type")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct typeid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.type != null && result.attributelist.type.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.type));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "type"));
            }
            if (item.matrixconfig.pattern || item.matrixconfig.column == "Pattern")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct patternid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.pattern != null && result.attributelist.pattern.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.pattern));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "pattern"));
            }
            if (item.matrixconfig.style || item.matrixconfig.column == "Style")
            {
                string query = @"select id,name from referencelist where id in (
                select distinct styleid from items
                where productid=@productid
                and isactive=true) and isactive=true";
                if (result.attributelist.style != null && result.attributelist.style.Count > 0)
                {
                    query = string.Format(@"select id,name from referencelist where id in ({0}) and isactive=true",
                        string.Join(",", result.attributelist.style));
                }
                response.DataSource.Add(BuildMatrix(item.productid, query, "style"));
            }
            if (item.matrixconfig.pricerange || item.matrixconfig.column == "Price Range")
            {
                response.PriceRanges = new List<ReorderMatrixSummary.Range>();
                foreach (var row in item.matrixconfig.pricelist.Split(','))
                {
                    var parts = row.Split('-');
                    int from = 0;
                    int to = 0;
                    int.TryParse(parts[0], out from);
                    if (parts.Length > 1)
                    {
                        int.TryParse(parts[1], out to);
                    }
                    response.PriceRanges.Add(new ReorderMatrixSummary.Range()
                    {
                        from = from,
                        to = to
                    });
                }
            }
            if (item.matrixconfig.location || item.matrixconfig.column == "Location")
            {
                item.locationid = 0;
            }
            response.Rules = Search(item);
            return response;
        }

        public ReorderMatrixSummary.Data BuildMatrix(long productid, string query, string name)
        {
            ReorderMatrixSummary.Data response = new ReorderMatrixSummary.Data() { name = name, rows = new List<ReorderMatrixSummary.Row>() };
            var param = new SortedList<string, object>();
            param.Add("productid", productid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    response.rows.Add(new ReorderMatrixSummary.Row()
                    {
                        id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]),
                        name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"])
                    });
                }
            }
            return response;
        }
        internal Reorder SaveAll(Reorder items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (var item in items.rules)
                    {
                        item.isactive = true;
                        item.enable = true;
                        if (item.id <= 0)
                            Insert(transaction, conn, item);
                        else
                            Update(transaction, conn, item);
                    }
                    if (items.deleted != null)
                        items.deleted.ForEach(e => Delete(transaction, conn, e));
                    items.deleted = null;
                    transaction.Commit();
                    conn.Close();
                }
                return items;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                throw exp;
            }

        }

        const string SQL_Delete = @"update public.reorderrules set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Reorderrules> Search(ReorderrulesCreteria _reorderrules)
        {
            List<Reorderrules> lst = new List<Reorderrules>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_reorderrules.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reorderrules.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("reorderrules.isactive", true, "=");
                    if (_reorderrules.productid != 0)
                        qb.AddParameter("reorderrules.productid", _reorderrules.productid, "=");
                    if (_reorderrules.locationid != 0)
                        qb.AddParameter("reorderrules.locationid", _reorderrules.locationid, "=");
                    if (_reorderrules.supplierid != 0)
                        qb.AddParameter("reorderrules.supplierid", _reorderrules.supplierid, "=");
                    if (_reorderrules.enable)
                        qb.AddParameter("reorderrules.enable", _reorderrules.enable, "=");
                    if (_reorderrules.brandid != 0)
                        qb.AddParameter("reorderrules.brandid", _reorderrules.brandid, "=");
                    if (_reorderrules.materialid != 0)
                        qb.AddParameter("reorderrules.materialid", _reorderrules.materialid, "=");
                    if (_reorderrules.styleid != 0)
                        qb.AddParameter("reorderrules.styleid", _reorderrules.styleid, "=");
                    if (_reorderrules.patternid != 0)
                        qb.AddParameter("reorderrules.patternid", _reorderrules.patternid, "=");
                    if (_reorderrules.typeid != 0)
                        qb.AddParameter("reorderrules.typeid", _reorderrules.typeid, "=");
                    if (_reorderrules.colourid != 0)
                        qb.AddParameter("reorderrules.colourid", _reorderrules.colourid, "=");
                    if (_reorderrules.sizeid != 0)
                        qb.AddParameter("reorderrules.sizeid", _reorderrules.sizeid, "=");
                    if (_reorderrules.sectionid != 0)
                        qb.AddParameter("products.salescounterid", _reorderrules.sectionid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Reorderrules oreorderrules = new Reorderrules();
                        oreorderrules.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreorderrules.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oreorderrules.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oreorderrules.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oreorderrules.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreorderrules.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oreorderrules.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oreorderrules.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oreorderrules.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oreorderrules.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oreorderrules.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oreorderrules.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oreorderrules.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oreorderrules.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oreorderrules.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oreorderrules.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oreorderrules.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oreorderrules.coreid = reader["coreid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["coreid"]);
                        oreorderrules.fromrate = reader["fromrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fromrate"]);
                        oreorderrules.torate = reader["torate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["torate"]);
                        oreorderrules.minro = reader["minro"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minro"]);
                        oreorderrules.maxro = reader["maxro"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxro"]);
                        oreorderrules.minorder = reader["minorder"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minorder"]);
                        oreorderrules.aprice = reader["aprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aprice"]);
                        oreorderrules.onpurchaseprice = reader["onpurchaseprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["onpurchaseprice"]);
                        oreorderrules.priority = reader["priority"] == DBNull.Value ? 0 : Convert.ToInt32(reader["priority"]);
                        oreorderrules.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oreorderrules.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oreorderrules.enable = reader["enable"] == DBNull.Value ? false : Convert.ToBoolean(reader["enable"]);
                        oreorderrules.expirydays = reader["expirydays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["expirydays"]);
                        lst.Add(oreorderrules);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reorderrules);
            }
            return lst;
        }


        public Reorderrules Insert(Reorderrules oReorderrules)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oReorderrules);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oReorderrules);
                throw exp;
            }

            return oReorderrules;
        }

        public Reorderrules Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Reorderrules oReorderrules)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReorderrules.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReorderrules.modifiedon;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.productid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.locationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.supplierid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.brandid;
            command.Parameters.Add("coreid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.coreid;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.patternid;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.styleid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.colourid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.materialid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.typeid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.sizeid;
            command.Parameters.Add("fromrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.fromrate;
            command.Parameters.Add("torate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.torate;
            command.Parameters.Add("minro", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.minro;
            command.Parameters.Add("maxro", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.maxro;
            command.Parameters.Add("minorder", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.minorder;
            command.Parameters.Add("aprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.aprice;
            command.Parameters.Add("onpurchaseprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.onpurchaseprice;
            command.Parameters.Add("priority", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.priority;
            command.Parameters.Add("expirydays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.expirydays;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReorderrules.description == null ? "" : oReorderrules.description;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReorderrules.designid == null ? "" : oReorderrules.designid;
            command.Parameters.Add("enable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.enable;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oReorderrules.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oReorderrules;
        }

        public Reorderrules Update(Reorderrules oReorderrules)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oReorderrules);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oReorderrules);
                throw exp;
            }

            return oReorderrules;
        }

        public Reorderrules Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Reorderrules oReorderrules)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReorderrules.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReorderrules.modifiedon;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.productid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.locationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.supplierid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.brandid;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.patternid;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.styleid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.colourid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.materialid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.typeid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.sizeid;
            command.Parameters.Add("fromrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.fromrate;
            command.Parameters.Add("torate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.torate;
            command.Parameters.Add("minro", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.minro;
            command.Parameters.Add("maxro", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.maxro;
            command.Parameters.Add("minorder", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.minorder;
            command.Parameters.Add("aprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oReorderrules.aprice;
            command.Parameters.Add("onpurchaseprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.onpurchaseprice;
            command.Parameters.Add("priority", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.priority;
            command.Parameters.Add("expirydays", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.expirydays;
            command.Parameters.Add("coreid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.coreid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReorderrules.description == null ? "" : oReorderrules.description;
            command.Parameters.Add("enable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.enable;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReorderrules.designid == null ? "" : oReorderrules.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oReorderrules.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oReorderrules;
        }


        public bool Delete(Reorderrules oReorderrules)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oReorderrules);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oReorderrules);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Reorderrules oReorderrules)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReorderrules.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReorderrules.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReorderrules.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReorderrules.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
