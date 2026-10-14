using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Processors.Migration;
using System.IO;
using OfficeOpenXml;
using InfyPOS.Services;
using System.Data;



namespace InfyPOS.Data
{
    public class ItemsDataService : SAASData
    {
        const string SQL_Search = @"select productname, productid, brandname, brandid, sellingname, code, version, modifiedon, 
        createdby, createdon, pricingmode, printingname, salemode, stylename, styleid, sizename, sizeid, patternname, patternid, 
        colourid, id, colourname, modifiedby, typename, isactive, typeid, materialname, materialid, searchkeys, podays, 
        stockagedays, minrate, ratetobebilled, mrp, salerate, purchaserate, dealerprice, allowsearch, reordermin, reordermax,
        autopomin,autopomax, support_pos, support_itemlevelprice, tradeagreeentid,agmtcode,support_gln, gln, support_expiry, expiryvalue, expirymode, 
        dssupport,dsmode, dsbuyingqty, dsfreeqty, dscomboprice, dsdiscountproductid, dsdiscountpercent, dsfromqty, 
        disablepo,singlesupplier,multisupplier,supplierlist,fixedsupplierid,imageid,sleeveid,fitid,seasonid,classid,prddivisionid,glndesignid {0} from items where id=@id";

        const string SQL_SearchExist = @"select productname, productid, brandname, brandid, sellingname, code, version, modifiedon, createdby, createdon, pricingmode, printingname, salemode, stylename, styleid, sizename, sizeid, patternname, patternid, colourid, id, colourname, modifiedby, typename, 
                    isactive, typeid, materialname, materialid, searchkeys,  podays, stockagedays, minrate, ratetobebilled, mrp, salerate, purchaserate, dealerprice, allowsearch, reordermin, reordermax,autopomin,autopomax, support_pos, support_itemlevelprice, support_gln, gln, support_expiry, expiryvalue, expirymode, 
                    dssupport,dsmode, dsbuyingqty, dsfreeqty, dscomboprice, dsdiscountproductid, dsdiscountpercent, dsfromqty, tradeagreeentid,agmtcode,
                    disablepo,singlesupplier,multisupplier,supplierlist,fixedsupplierid,imageid,sleeveid,fitid,seasonid,classid,prddivisionid,glndesignid from items 
                    where  isactive = true and brandid=@brandid and  productid=@productid and  colourid=@colourid and  patternid=@patternid and  sizeid=@sizeid and  styleid=@styleid and  typeid=@typeid and materialid=@materialid and sleeveid=@sleeveid and fitid=@fitid";

        const string SQL_Insert = @"INSERT INTO public.items(productname, productid, brandname, brandid, sellingname, code, version, modifiedon, createdby, createdon, pricingmode, printingname, salemode, stylename, styleid, sizename, sizeid, patternname, patternid, colourid, colourname, modifiedby, typename, isactive, typeid, materialname, materialid, searchkeys,  podays, stockagedays, minrate, ratetobebilled, mrp, salerate, purchaserate, dealerprice, allowsearch, reordermin, reordermax,autopomin,
        autopomax, support_pos, support_itemlevelprice, support_gln, gln, support_expiry, expiryvalue, expirymode, tradeagreeentid,agmtcode,
        dssupport,dsmode, dsbuyingqty, dsfreeqty, dscomboprice, dsdiscountproductid, dsdiscountpercent, dsfromqty, 
        disablepo,singlesupplier,multisupplier,supplierlist,fixedsupplierid,imageid,sleeveid,fitid,seasonid,classid,prddivisionid,glndesignid {0}) 
        VALUES(@productname, @productid, @brandname, @brandid, @sellingname, @code, @version, @modifiedon, @createdby, @createdon, @pricingmode, @printingname, @salemode, @stylename, @styleid, @sizename, @sizeid, @patternname, @patternid, @colourid, @colourname, @modifiedby, @typename, @isactive, @typeid, @materialname, @materialid, to_tsvector(@searchkeys),  @podays, @stockagedays, @minrate, @ratetobebilled, @mrp, @salerate, @purchaserate, @dealerprice, @allowsearch, @reordermin, @reordermax,@autopomin,@autopomax, 
        @support_pos, @support_itemlevelprice, @support_gln, @gln, @support_expiry, @expiryvalue, @expirymode,@tradeagreeentid,@agmtcode, 
        @dssupport,@dsmode, @dsbuyingqty, @dsfreeqty, @dscomboprice, @dsdiscountproductid, @dsdiscountpercent, @dsfromqty, 
        @disablepo,@singlesupplier,@multisupplier,@supplierlist,@fixedsupplierid,@imageid,@sleeveid,@fitid,@seasonid,@classid,@prddivisionid,@glndesignid {1}) RETURNING id";

        const string SQL_Update = @"UPDATE public.items SET productname=@productname, productid=@productid, brandname=@brandname, brandid=@brandid, sellingname=@sellingname, code=@code, version=@version+1, modifiedon=@modifiedon, pricingmode=@pricingmode, printingname=@printingname, salemode=@salemode, stylename=@stylename, styleid=@styleid, sizename=@sizename, sizeid=@sizeid, patternname=@patternname, patternid=@patternid, colourid=@colourid, colourname=@colourname, modifiedby=@modifiedby, 
        typename=@typename, typeid=@typeid, materialname=@materialname, materialid=@materialid, searchkeys=to_tsvector(@searchkeys), podays=@podays, stockagedays=@stockagedays, minrate=@minrate, ratetobebilled=@ratetobebilled, mrp=@mrp, salerate=@salerate, purchaserate=@purchaserate, dealerprice=@dealerprice, allowsearch=@allowsearch, reordermin=@reordermin,
        reordermax=@reordermax, support_pos=@support_pos, support_itemlevelprice=@support_itemlevelprice, support_gln=@support_gln, gln=@gln, support_expiry=@support_expiry, expiryvalue=@expiryvalue, expirymode=@expirymode, 
        dssupport=@dssupport,dsmode=@dsmode, dsbuyingqty=@dsbuyingqty, dsfreeqty=@dsfreeqty, dscomboprice=@dscomboprice, dsdiscountproductid=@dsdiscountproductid, dsdiscountpercent=@dsdiscountpercent, dsfromqty=@dsfromqty,autopomin=@autopomin,autopomax=@autopomax,
        disablepo=@disablepo,singlesupplier=@singlesupplier,multisupplier=@multisupplier,supplierlist=@supplierlist,tradeagreeentid=@tradeagreeentid,agmtcode=@agmtcode,
        fixedsupplierid=@fixedsupplierid,imageid=@imageid,sleeveid=@sleeveid,fitid=@fitid,seasonid=@seasonid,classid=@classid,prddivisionid=@prddivisionid,glndesignid=@glndesignid {0} where id=@id and version=@version RETURNING version";

        const string SQL_SearchInfo = @"select products.name as productname,items.productid,
                                    style.name stylename,pattern.name patternname,size.name sizename, 
                                    colour.name colourname,type.name typename,material.name materialname,
                                    items.brandid,items.colourid,items.sizeid,items.styleid,items.patternid,
                                    items.materialid,items.typeid,brand.name brandname,items.id
                                    from items
                                    left outer join products on products.id = items.productid
                                    left outer join brand brand on brand.id = items.brandid
                                    left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist type on type.id = typeid
                                    where items.id = @id";

        //        const string SQL_Insert = @"INSERT INTO public.items(code, sellingname, brandid, brandname, productid, productname, colourname, colourid, patternid, patternname, 
        //            sizeid, sizename, styleid, stylename, salemode, printingname, pricingmode, typeid, typename, createdon, modifiedon, createdby, modifiedby, isactive, version,materialid,materialname,searchkeys,podays,stockagedays,minrate,ratetobebilled,mrp,salerate,purchaserate,dealerprice) 
        //VALUES(@code, @sellingname, @brandid, @brandname, @productid, @productname, @colourname, @colourid, @patternid, @patternname, @sizeid, @sizename, @styleid, @stylename, @salemode, @printingname,
        //@pricingmode, @typeid, @typename, @createdon, @modifiedon, @createdby, @modifiedby, @isactive, @version,@materialid,@materialname,to_tsvector(@searchkeys),@podays,@stockagedays,@minrate,@ratetobebilled,@mrp,@salerate,@purchaserate,@dealerprice ) RETURNING id";


        //        const string SQL_Update = @"UPDATE public.items SET code=@code, sellingname=@sellingname, brandid=@brandid, brandname=@brandname, productid=@productid, productname=@productname, colourname=@colourname, colourid=@colourid, patternid=@patternid, patternname=@patternname, sizeid=@sizeid,
        //                    sizename=@sizename, styleid=@styleid, stylename=@stylename, salemode=@salemode, printingname=@printingname, pricingmode=@pricingmode, typeid=@typeid, typename=@typename,
        //                    modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1,materialid=@materialid,
        //                    materialname = @materialid, searchkeys = to_tsvector(@searchkeys),podays=@podays,stockagedays=@stockagedays,minrate=@minrate,ratetobebilled=@ratetobebilled,mrp=@mrp,salerate=@salerate,purchaserate=@purchaserate,dealerprice=@dealerprice  where id=@id and version=@version RETURNING version";


        //to_tsvector(@searchkeys)
        const string SQL_Delete = @"update public.items set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public bool productmap { get; set; }
        public List<Items> Search(ItemsCreteria _items)
        {
            return Search(_items, null);
        }
        public List<Items> Search(ItemsCreteria _items, List<long> itemids)
        {
            List<Items> lst = new List<Items>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(_items, itemids, conn, null);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _items);
            }
            return lst;
        }

        private bool isEmpty(string value)
        {
            return string.IsNullOrEmpty(value) || value == "." || value == "NULL" || value == "[NULL]" || value == "None" || value == "[None]";
        }
        public string GetString(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
        {
            string _name = name.ToLower();
            if (columnMap.ContainsKey(_name))
            {
                if (columnMap[_name].index >= 0 && source.Count > columnMap[_name].index)
                    return isEmpty(source[columnMap[_name].index]) ? "" : source[columnMap[_name].index].Trim();
                return isEmpty(columnMap[_name].defaultvalue) ? "" : columnMap[_name].defaultvalue;
            }
            return "";
        }

        internal ProgressStatus MigrateItemData(MigrationData data)
        {

            //var columnsheaders = new List<string>();
            //var rows = new List<List<string>>();
            //var imageData = new ImageData() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            //data.progress = new ProgressStatus()
            //{
            //    faileddata = new List<List<string>>(),
            //    ExceptionList = new List<Exception>()
            //};

            //var file = imageData.GetData(data.uploadfileid.LongFromBase62());

            //using (MemoryStream ms = new MemoryStream(file.data))
            //{
            //    ExcelPackage package = new ExcelPackage(ms);
            //    ExcelWorksheet worksheet = null;
            //    if (!string.IsNullOrEmpty(data.sheetname))
            //    {
            //        foreach (var sh in package.Workbook.Worksheets)
            //        {
            //            if (sh.Name.ToLower() == data.sheetname.ToLower())
            //                worksheet = sh;
            //        }
            //    }

            //    if (worksheet == null)
            //    {
            //        data.progress.status = "failed with exception can't find the sheet " + data.sheetname;
            //        data.progress.hascompleted = true;
            //        return null;
            //    }

            //    bool firstRow = true;

            //    for (int row = 1; row <= worksheet.Dimension.Rows; row++)
            //    {
            //        var excelrow = worksheet.Row(row);
            //        var rowvalues = new List<string>();
            //        for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            //        {
            //            var cellvalue = worksheet.GetValue(row, col);
            //            if (firstRow)
            //            {
            //                string scellvalue = (cellvalue != null ? cellvalue.ToString() : "").ToLower();
            //                columnsheaders.Add(scellvalue.ToLower());
            //            }
            //            else
            //            {
            //                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
            //            }
            //        }
            //        if (firstRow)
            //        {
            //            firstRow = false;
            //        }
            //        else
            //        {
            //            rows.Add(rowvalues);
            //        }
            //    }
            //}
            //data.progress.totalrows = rows.Count;


            //SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
            //foreach (var column in data.columns)
            //{
            //    MigrationManager.ColumnMap columnMap = new MigrationManager.ColumnMap();
            //    columnMap.columnname = column.name.ToLower();
            //    columnMap.defaultvalue = column.value;
            //    if (!string.IsNullOrEmpty(column.excelcolunmn))
            //    {
            //        columnMap.columnmap = column.excelcolunmn.ToLower();
            //        columnMap.index = columnsheaders.Contains(columnMap.columnmap) ? (columnsheaders.IndexOf(columnMap.columnmap)) : -1;
            //    }
            //    else
            //    {
            //        columnMap.columnmap = "";
            //        columnMap.index = -1;
            //    }
            //    columnmap.Add(columnMap.columnname, columnMap);
            //}

            InfyPOS.Data.SupplierDataService supplierDataService = new InfyPOS.Data.SupplierDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            var suppliers = supplierDataService.SearchHeaders(new SupplierCreteria());


            InfyPOS.Data.ItemsDataService itemDataService = new InfyPOS.Data.ItemsDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            var existing = itemDataService.Search(new InfyPOS.Models.ItemsCreteria() { onlyglnitems = true });

            InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            var brandlist = brandDataService.Search(new InfyPOS.Models.BrandCreteria() { });

            InfyPOS.Data.ProductsDataService productDataService = new InfyPOS.Data.ProductsDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            var productlist = productDataService.Search(new InfyPOS.Models.ProductsCreteria() { });

            InfyPOS.Data.ReferencelistDataService referenceDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            var reflist = referenceDataService.SearchList(new Models.ReferencelistCreteria()
            {
                types = new List<string>("STYLE,PATTERN,COLOUR,SIZE,SLEEVE,FIT,TYPE,MATERIAL".Split(','))
            });

            //List<MigrationManager.MigratingItem> migrationDatas = new List<MigrationManager.MigratingItem>();
            //foreach (var source in data.rows)
            //{
            //    try
            //    {
            //        Decimal d;
            //        MigrationManager.MigratingItem migratingItem = new MigrationManager.MigratingItem();
            //        migratingItem.source = source;
            //        migratingItem.suppliercode = GetString(source, columnmap, "suppliercode");
            //        migratingItem.supplier = GetString(source, columnmap, "supplier");
            //        migratingItem.brand = GetString(source, columnmap, "brand");
            //        migratingItem.code = GetString(source, columnmap, "code");
            //        migratingItem.colour = GetString(source, columnmap, "colour");
            //        migratingItem.fit = GetString(source, columnmap, "fit");
            //        migratingItem.gln = GetString(source, columnmap, "gln");
            //        migratingItem.material = GetString(source, columnmap, "material");
            //        migratingItem.pattern = GetString(source, columnmap, "pattern");
            //        migratingItem.printing = GetString(source, columnmap, "printing");
            //        migratingItem.product = GetString(source, columnmap, "product");
            //        migratingItem.glndesignid = GetString(source, columnmap, "glndesignid");
            //        if (decimal.TryParse(GetString(source, columnmap, "salerate"), out d))
            //        {
            //            migratingItem.salerate = d;
            //        }
            //        if (decimal.TryParse(GetString(source, columnmap, "purchaserate"), out d))
            //        {
            //            migratingItem.purchaserate = d;
            //        }
            //        if (decimal.TryParse(GetString(source, columnmap, "dealerprice"), out d))
            //        {
            //            migratingItem.dealerprice = d;
            //        }
            //        if (decimal.TryParse(GetString(source, columnmap, "mrprate"), out d))
            //        {
            //            migratingItem.mrprate = d;
            //        }
            //        migratingItem.showlist = GetString(source, columnmap, "showlist").ToUpper() != "FALSE";
            //        migratingItem.size = GetString(source, columnmap, "size");
            //        migratingItem.sleeve = GetString(source, columnmap, "sleeve");
            //        migratingItem.stylename = GetString(source, columnmap, "style");
            //        migratingItem.type = GetString(source, columnmap, "type");

            //        migrationDatas.Add(migratingItem);


            //    }
            //    catch (Exception exp)
            //    {
            //        source.Insert(0, exp.Message);
            //        data.progress.faileddata.Add(source);
            //    }
            //}

            SalablegoodspriceDataService salablegoodspriceDataService = new SalablegoodspriceDataService() { Context = this.Context };
            data.progress.totalrows = data.rows.Count;
            foreach (var source in data.rows)
            {
                try
                {
                    Models.Items items = new Models.Items();
                    bool ispricechange = false;
                    bool vendoradded = false;


                    if (!string.IsNullOrEmpty(source.gln) &&
                        existing.Exists(e => e.gln == source.gln))
                    {
                        items = existing.Find(e => e.gln == source.gln);
                        if (items.salerate != source.salerate)
                        {
                            ispricechange = true;
                            salablegoodspriceDataService.Insert(new Salablegoodsprice()
                            {
                                addon = DateTime.Now,
                                barcode = source.gln,
                                isactive = true,
                                mrp = source.mrprate,
                                price = source.salerate,
                                version = 1
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(source.supplier))
                    {
                        if (!suppliers.Exists(x => x.name == source.supplier &&
                        x.code == source.suppliercode))
                        {
                            var supplier = new Supplier()
                            {
                                name = source.supplier,
                                code = source.suppliercode
                            };
                            supplier.companylist = new List<Suppliercompany>();
                            supplier.companylist.Add(new Suppliercompany()
                            {
                                code = source.suppliercode,
                                name = source.supplier
                            });
                            suppliers.Add(supplierDataService.Insert(supplier));
                        }
                        if (items.id > 0)
                        {
                            var fixedsupplierid = suppliers.Find(e => e.name == source.supplier).id;
                            if (items.fixedsupplierid > 0 &&
                                fixedsupplierid != items.fixedsupplierid)
                            {
                                items.supplierlist = new List<InfyPOS.Models.Items.MultiSupplier>();
                                items.multisupplier = true;
                                items.singlesupplier = false;
                                items.fixedsupplierid = 0;
                                items.supplierlist.Add(new InfyPOS.Models.Items.MultiSupplier()
                                {
                                    id = items.fixedsupplierid
                                });
                            }
                            if (items.supplierlist != null &&
                                items.supplierlist.Count > 0)
                            {
                                if (!items.supplierlist.Exists(x => x.id == fixedsupplierid))
                                {
                                    vendoradded = true;
                                    items.supplierlist.Add(new InfyPOS.Models.Items.MultiSupplier()
                                    {
                                        id = fixedsupplierid
                                    });
                                }
                            }
                            else
                            {
                                items.singlesupplier = true;
                                items.fixedsupplierid = suppliers.Find(e => e.name == source.supplier).id;
                            }
                        }
                        else
                        {
                            items.singlesupplier = true;
                            items.fixedsupplierid = suppliers.Find(e => e.name == source.supplier).id;
                        }
                    }
                    if (Services.ConfigManager.Instance(data.organizationid).Config.system_ismfgapplication &&
                            items.id > 0 && (ispricechange || vendoradded))
                    {
                        itemDataService.Update(items, false);
                        continue;
                    }
                    items.gln = source.gln;
                    items.glndesignid = source.glndesignid;

                    items.salerate = source.salerate;
                    items.purchaserate = source.purchaserate;
                    items.dealerprice = source.dealerprice;
                    items.mrp = source.mrprate;

                    items.allowsearch = !string.IsNullOrEmpty(source.gln);
                    items.createdby = -2;
                    items.createdon = DateTimeNow;
                    items.isactive = true;
                    items.support_gln = !string.IsNullOrEmpty(source.gln);
                    items.productname = source.product;



                    if (!productlist.Exists(e => e.name.ToUpper() == source.product.ToUpper()))
                    {
                        productlist.Add(productDataService.Insert(new Models.Products()
                        {
                            name = source.product.ToUpper(),
                            SellingMode = Models.Products.sellingMode.Pack,
                            isactive = true,
                            attributefilter = !string.IsNullOrEmpty(source.gln),
                            attributes = new Products.ProductAttributes()
                            {
                                serialno = !string.IsNullOrEmpty(source.gln)
                            }
                        }));
                    }
                    items.productid = productlist.Find(e => e.name.ToUpper() == source.product.ToUpper()).id;

                    if (!string.IsNullOrEmpty(source.brand))
                    {
                        items.brandname = source.brand.ToUpper();
                        if (!brandlist.Exists(e => e.name.ToUpper() == source.brand.ToUpper()))
                        {
                            brandlist.Add(brandDataService.Insert(new Models.Brand()
                            {
                                name = source.brand.ToUpper(),
                                isactive = true
                            }));
                        }
                        items.brandid = brandlist.Find(e => e.name.ToUpper() == source.brand.ToUpper()).id;
                    }

                    items.printingname = string.IsNullOrEmpty(source.printing) ? items.productname : source.printing;
                    items.sellingname = string.IsNullOrEmpty(source.printing) ? items.productname : source.printing;
                    items.code = source.code;

                    if (!string.IsNullOrEmpty(source.colour))
                    {
                        items.colourname = source.colour;
                        if (!reflist.Exists(e => e.name == source.colour && e.type == "COLOUR"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.colour,
                                type = "COLOUR",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.colourid = reflist.Find(e => e.name == source.colour && e.type == "COLOUR").id;
                    }

                    if (!string.IsNullOrEmpty(source.size))
                    {
                        items.sizename = source.size;
                        if (!reflist.Exists(e => e.name == source.size && e.type == "SIZE"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.size,
                                type = "SIZE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.sizeid = reflist.Find(e => e.name == source.size && e.type == "SIZE").id;
                    }

                    if (!string.IsNullOrEmpty(source.pattern))
                    {
                        items.patternname = source.pattern;
                        if (!reflist.Exists(e => e.name == source.pattern && e.type == "PATTERN"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.pattern,
                                type = "PATTERN",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });

                        }
                        items.patternid = reflist.Find(e => e.name == source.pattern && e.type == "PATTERN").id;
                    }

                    if (!string.IsNullOrEmpty(source.material))
                    {
                        items.materialname = source.material;
                        if (!reflist.Exists(e => e.name == source.material && e.type == "MATERIAL"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.material,
                                type = "MATERIAL",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.materialid = reflist.Find(e => e.name == source.material && e.type == "MATERIAL").id;
                    }

                    if (!string.IsNullOrEmpty(source.stylename))
                    {
                        items.stylename = source.stylename;
                        if (!reflist.Exists(e => e.name == source.stylename && e.type == "STYLE"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.stylename,
                                type = "STYLE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.styleid = reflist.Find(e => e.name == source.stylename && e.type == "STYLE").id;
                    }

                    if (!string.IsNullOrEmpty(source.type))
                    {
                        items.typename = source.type;
                        if (!reflist.Exists(e => e.name == source.type && e.type == "TYPE"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.type,
                                type = "TYPE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.typeid = reflist.Find(e => e.name == source.type && e.type == "TYPE").id;
                    }

                    if (!string.IsNullOrEmpty(source.sleeve))
                    {
                        if (!reflist.Exists(e => e.name == source.sleeve && e.type == "SLEEVE"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.sleeve,
                                type = "SLEEVE",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.sleeveid = reflist.Find(e => e.name == source.sleeve && e.type == "SLEEVE").id;
                    }

                    if (!string.IsNullOrEmpty(source.fit))
                    {
                        if (!reflist.Exists(e => e.name == source.fit && e.type == "FIT"))
                        {
                            var newitem = referenceDataService.Insert(new Models.Referencelist()
                            {
                                name = source.fit,
                                type = "FIT",
                                isactive = true
                            });
                            reflist.Add(new ReferenceItem()
                            {
                                id = newitem.id,
                                code = newitem.code,
                                name = newitem.name,
                                type = newitem.type
                            });
                        }
                        items.fitid = reflist.Find(e => e.name == source.fit && e.type == "FIT").id;
                    }

                    if (items.id == 0)
                    {
                        if (!existing.Exists(e => e.productid == items.productid &&
                                                 e.styleid == items.styleid &&
                                                 e.typeid == items.typeid &&
                                                 e.patternid == items.patternid &&
                                                 e.brandid == items.brandid &&
                                                 e.materialid == items.materialid &&
                                                 e.fitid == items.fitid &&
                                                 e.sleeveid == items.sleeveid &&
                                                 e.sizeid == items.sizeid &&
                                                 e.colourid == items.colourid &&
                                                 e.gln == items.gln &&
                                                 e.glndesignid == items.glndesignid))
                        {
                            itemDataService.Insert(items);
                        }
                        else
                        {
                            var eitem = existing.Find(e => e.productid == items.productid &&
                                                 e.styleid == items.styleid &&
                                                 e.typeid == items.typeid &&
                                                 e.patternid == items.patternid &&
                                                 e.brandid == items.brandid &&
                                                 e.materialid == items.materialid &&
                                                 e.fitid == items.fitid &&
                                                 e.sleeveid == items.sleeveid &&
                                                 e.sizeid == items.sizeid &&
                                                 e.colourid == items.colourid &&
                                                 e.gln == items.gln &&
                                                 e.glndesignid == items.glndesignid);

                            if (eitem.code != items.code || eitem.gln != items.gln)
                            {
                                eitem.code = items.code;
                                eitem.gln = items.gln;
                                eitem.glndesignid = items.glndesignid;
                                eitem.allowsearch = items.allowsearch;
                                eitem.support_gln = eitem.support_gln;
                                itemDataService.Update(items, false);
                            }
                        }

                    }
                    else
                        itemDataService.Update(items, false);

                    data.progress.completedrows++;
                    data.progress.percentage = (data.progress.completedrows / data.progress.totalrows * 100);
                }
                catch (Exception exp)
                {
                    data.progress.failedrows++;
                    data.progress.faileddata.Add(source.source);
                }
            }

            return data.progress;
        }

        //internal ProgressStatus MigrateItemImport(MigrationData data)
        //{

        //    var columnsheaders = new List<string>();

        //    var imageData = new ImageData() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
        //    //data.progress = new ProgressStatus()
        //    //{
        //    //    faileddata = new List<List<string>>(),
        //    //    ExceptionList = new List<Exception>()
        //    //};


        //    var rows = data.rows;
        //    data.progress.totalrows = rows.Count;


        //    SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
        //    foreach (var column in data.columns)
        //    {
        //        MigrationManager.ColumnMap columnMap = new MigrationManager.ColumnMap();
        //        columnMap.columnname = column.name.ToLower();
        //        columnMap.defaultvalue = column.value;
        //        if (!string.IsNullOrEmpty(column.excelcolunmn))
        //        {
        //            columnMap.columnmap = column.excelcolunmn.ToLower();
        //            columnMap.index = columnsheaders.Contains(columnMap.columnmap) ? (columnsheaders.IndexOf(columnMap.columnmap)) : -1;
        //        }
        //        else
        //        {
        //            columnMap.columnmap = "";
        //            columnMap.index = -1;
        //        }
        //        columnmap.Add(columnMap.columnname, columnMap);
        //    }

        //    InfyPOS.Data.ItemsDataService itemDataService = new InfyPOS.Data.ItemsDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
        //    List<Items> existing = itemDataService.Search(new InfyPOS.Models.ItemsCreteria() { onlyglnitems = true });

        //    InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
        //    List<Brand> brandlist = brandDataService.Search(new InfyPOS.Models.BrandCreteria() { });

        //    InfyPOS.Data.ProductsDataService productDataService = new InfyPOS.Data.ProductsDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
        //    List<Products> productlist = productDataService.Search(new InfyPOS.Models.ProductsCreteria() { });

        //    InfyPOS.Data.ReferencelistDataService referenceDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
        //    List<ReferenceItem> reflist = referenceDataService.SearchList(new Models.ReferencelistCreteria()
        //    {
        //        types = new List<string>("STYLE,PATTERN,COLOUR,SIZE,SLEEVE,FIT,TYPE,MATERIAL".Split(','))
        //    });

        //    List<MigrationManager.MigratingItem> migrationDatas = data.rows;
        //    foreach (var source in migrationDatas)
        //    {
        //        try
        //        {
        //            Models.Items items = new Models.Items();
        //            if (!string.IsNullOrEmpty(source.gln) &&
        //                existing.Exists(e => e.gln == source.gln))
        //            {
        //                items = existing.Find(e => e.gln == source.gln);
        //            }
        //            items.gln = source.gln;
        //            items.glndesignid = source.glndesignid;

        //            items.salerate = source.salerate;
        //            items.purchaserate = source.purchaserate;
        //            items.dealerprice = source.dealerprice;
        //            items.mrp = source.mrprate;

        //            items.allowsearch = !string.IsNullOrEmpty(source.gln);
        //            items.createdby = -2;
        //            items.createdon = DateTimeNow;
        //            items.isactive = true;
        //            items.support_gln = !string.IsNullOrEmpty(source.gln);
        //            items.productname = source.product;
        //            if (!productlist.Exists(e => e.name == source.product))
        //            {
        //                productlist.Add(productDataService.Insert(new Models.Products()
        //                {
        //                    name = source.product,
        //                    SellingMode = Models.Products.sellingMode.Pack,
        //                    isactive = true,
        //                    attributefilter = !string.IsNullOrEmpty(source.gln),
        //                    attributes = new Products.ProductAttributes()
        //                    {
        //                        serialno = !string.IsNullOrEmpty(source.gln)
        //                    }
        //                }));
        //            }
        //            items.productid = productlist.Find(e => e.name == source.product).id;

        //            if (!string.IsNullOrEmpty(source.brand))
        //            {
        //                items.brandname = source.brand;
        //                if (!brandlist.Exists(e => e.name == source.brand))
        //                {
        //                    brandlist.Add(brandDataService.Insert(new Models.Brand()
        //                    {
        //                        name = source.brand,
        //                        isactive = true
        //                    }));
        //                }
        //                items.brandid = brandlist.Find(e => e.name == source.brand).id;
        //            }

        //            items.printingname = string.IsNullOrEmpty(source.printing) ? items.productname : source.printing;
        //            items.sellingname = string.IsNullOrEmpty(source.printing) ? items.productname : source.printing;
        //            items.code = source.code;

        //            if (!string.IsNullOrEmpty(source.colour))
        //            {
        //                items.colourname = source.colour;
        //                if (!reflist.Exists(e => e.name == source.colour && e.type == "COLOUR"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.colour,
        //                        type = "COLOUR",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });
        //                }
        //                items.colourid = reflist.Find(e => e.name == source.colour && e.type == "COLOUR").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.size))
        //            {
        //                items.sizename = source.size;
        //                if (!reflist.Exists(e => e.name == source.size && e.type == "SIZE"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.size,
        //                        type = "SIZE",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });
        //                }
        //                items.sizeid = reflist.Find(e => e.name == source.size && e.type == "SIZE").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.pattern))
        //            {
        //                items.patternname = source.pattern;
        //                if (!reflist.Exists(e => e.name == source.pattern && e.type == "PATTERN"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.pattern,
        //                        type = "PATTERN",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });

        //                }
        //                items.patternid = reflist.Find(e => e.name == source.pattern && e.type == "PATTERN").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.material))
        //            {
        //                items.materialname = source.material;
        //                if (!reflist.Exists(e => e.name == source.material && e.type == "MATERIAL"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.material,
        //                        type = "MATERIAL",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });
        //                }
        //                items.materialid = reflist.Find(e => e.name == source.material && e.type == "MATERIAL").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.stylename))
        //            {
        //                items.stylename = source.stylename;
        //                if (!reflist.Exists(e => e.name == source.stylename && e.type == "STYLE"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.stylename,
        //                        type = "STYLE",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });
        //                }
        //                items.styleid = reflist.Find(e => e.name == source.stylename && e.type == "STYLE").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.type))
        //            {
        //                items.typename = source.type;
        //                if (!reflist.Exists(e => e.name == source.type && e.type == "TYPE"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.type,
        //                        type = "TYPE",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });
        //                }
        //                items.typeid = reflist.Find(e => e.name == source.type && e.type == "TYPE").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.sleeve))
        //            {
        //                if (!reflist.Exists(e => e.name == source.sleeve && e.type == "SLEEVE"))
        //                {
        //                    var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                    {
        //                        name = source.sleeve,
        //                        type = "SLEEVE",
        //                        isactive = true
        //                    });
        //                    reflist.Add(new ReferenceItem()
        //                    {
        //                        id = newitem.id,
        //                        code = newitem.code,
        //                        name = newitem.name,
        //                        type = newitem.type
        //                    });
        //                }
        //                items.sleeveid = reflist.Find(e => e.name == source.sleeve && e.type == "SLEEVE").id;
        //            }

        //            if (!string.IsNullOrEmpty(source.fit))
        //            {
        //                var newitem = referenceDataService.Insert(new Models.Referencelist()
        //                {
        //                    name = source.fit,
        //                    type = "FIT",
        //                    isactive = true
        //                });
        //                reflist.Add(new ReferenceItem()
        //                {
        //                    id = newitem.id,
        //                    code = newitem.code,
        //                    name = newitem.name,
        //                    type = newitem.type
        //                });
        //                items.fitid = reflist.Find(e => e.name == source.fit && e.type == "FIT").id;
        //            }

        //            if (items.id == 0)
        //            {
        //                if (!existing.Exists(e => e.productid == items.productid &&
        //                                         e.styleid == items.styleid &&
        //                                         e.typeid == items.typeid &&
        //                                         e.patternid == items.patternid &&
        //                                         e.brandid == items.brandid &&
        //                                         e.materialid == items.materialid &&
        //                                         e.fitid == items.fitid &&
        //                                         e.sleeveid == items.sleeveid &&
        //                                         e.sizeid == items.sizeid &&
        //                                         e.colourid == items.colourid &&
        //                                         e.gln == items.gln &&
        //                                         e.glndesignid == items.glndesignid))
        //                {
        //                    itemDataService.Insert(items);
        //                }
        //                else
        //                {
        //                    var eitem = existing.Find(e => e.productid == items.productid &&
        //                                         e.styleid == items.styleid &&
        //                                         e.typeid == items.typeid &&
        //                                         e.patternid == items.patternid &&
        //                                         e.brandid == items.brandid &&
        //                                         e.materialid == items.materialid &&
        //                                         e.fitid == items.fitid &&
        //                                         e.sleeveid == items.sleeveid &&
        //                                         e.sizeid == items.sizeid &&
        //                                         e.colourid == items.colourid &&
        //                                         e.gln == items.gln &&
        //                                         e.glndesignid == items.glndesignid);

        //                    if (eitem.code != items.code || eitem.gln != items.gln)
        //                    {
        //                        eitem.code = items.code;
        //                        eitem.gln = items.gln;
        //                        eitem.glndesignid = items.glndesignid;
        //                        eitem.allowsearch = items.allowsearch;
        //                        eitem.support_gln = eitem.support_gln;
        //                        itemDataService.Update(items, false);
        //                    }
        //                }
        //            }
        //            else
        //                itemDataService.Update(items, false);

        //            data.progress.completedrows++;
        //        }
        //        catch (Exception exp)
        //        {
        //            data.progress.failedrows++;
        //            data.progress.faileddata.Add(source.source);
        //        }
        //    }

        //    return data.progress;
        //}
        internal List<MigrationManager.MigratingItem> MigrateItemProcess(MigrationData data)
        {

            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var imageData = new ImageData() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            data.progress = new ProgressStatus()
            {
                faileddata = new List<List<string>>(),
                ExceptionList = new List<Exception>()
            };

            var file = imageData.GetData(data.uploadfileid.LongFromBase62());

            using (MemoryStream ms = new MemoryStream(file.data))
            {
                ExcelPackage package = new ExcelPackage(ms);
                ExcelWorksheet worksheet = null;
                if (!string.IsNullOrEmpty(data.sheetname))
                {
                    foreach (var sh in package.Workbook.Worksheets)
                    {
                        if (sh.Name.ToLower() == data.sheetname.ToLower())
                            worksheet = sh;
                    }
                }

                if (worksheet == null)
                {
                    data.progress.status = "failed with exception can't find the sheet " + data.sheetname;
                    data.progress.hascompleted = true;
                    return null;
                }

                bool firstRow = true;

                for (int row = 1; row <= worksheet.Dimension.Rows; row++)
                {
                    var excelrow = worksheet.Row(row);
                    var rowvalues = new List<string>();
                    for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                    {
                        var cellvalue = worksheet.GetValue(row, col);
                        if (firstRow)
                        {
                            string scellvalue = (cellvalue != null ? cellvalue.ToString() : "").ToLower();
                            columnsheaders.Add(scellvalue.ToLower());
                        }
                        else
                        {
                            rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        rows.Add(rowvalues);
                    }
                }
            }
            data.progress.totalrows = rows.Count;


            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
            foreach (var column in data.columns)
            {
                MigrationManager.ColumnMap columnMap = new MigrationManager.ColumnMap();
                columnMap.columnname = column.name.ToLower();
                columnMap.defaultvalue = column.value;
                if (!string.IsNullOrEmpty(column.excelcolunmn))
                {
                    columnMap.columnmap = column.excelcolunmn.ToLower();
                    columnMap.index = columnsheaders.Contains(columnMap.columnmap) ? (columnsheaders.IndexOf(columnMap.columnmap)) : -1;
                }
                else
                {
                    columnMap.columnmap = "";
                    columnMap.index = -1;
                }
                columnmap.Add(columnMap.columnname, columnMap);
            }

            InfyPOS.Data.ItemsDataService itemDataService = new InfyPOS.Data.ItemsDataService() { Context = new UserClaim() { OrganizationID = this.Context.OrganizationID } };
            var existing = itemDataService.Search(new InfyPOS.Models.ItemsCreteria() { onlyglnitems = true });

            List<MigrationManager.MigratingItem> migrationDatas = new List<MigrationManager.MigratingItem>();
            foreach (var source in rows)
            {
                try
                {
                    Decimal d;
                    MigrationManager.MigratingItem migratingItem = new MigrationManager.MigratingItem();
                    migratingItem.source = source;
                    migratingItem.brand = GetString(source, columnmap, "brand");
                    migratingItem.code = GetString(source, columnmap, "code");
                    migratingItem.colour = GetString(source, columnmap, "colour");
                    migratingItem.fit = GetString(source, columnmap, "fit");
                    migratingItem.gln = GetString(source, columnmap, "gln");
                    migratingItem.material = GetString(source, columnmap, "material");
                    migratingItem.pattern = GetString(source, columnmap, "pattern");
                    migratingItem.printing = GetString(source, columnmap, "printing");
                    migratingItem.product = GetString(source, columnmap, "product");
                    migratingItem.glndesignid = GetString(source, columnmap, "glndesignid");
                    if (decimal.TryParse(GetString(source, columnmap, "salerate"), out d))
                    {
                        migratingItem.salerate = d;
                    }
                    if (decimal.TryParse(GetString(source, columnmap, "purchaserate"), out d))
                    {
                        migratingItem.purchaserate = d;
                    }
                    if (decimal.TryParse(GetString(source, columnmap, "dealerprice"), out d))
                    {
                        migratingItem.dealerprice = d;
                    }
                    if (decimal.TryParse(GetString(source, columnmap, "mrprate"), out d))
                    {
                        migratingItem.mrprate = d;
                    }
                    migratingItem.showlist = GetString(source, columnmap, "showlist").ToUpper() != "FALSE";
                    migratingItem.size = GetString(source, columnmap, "size");
                    migratingItem.sleeve = GetString(source, columnmap, "sleeve");
                    migratingItem.stylename = GetString(source, columnmap, "style");
                    migratingItem.type = GetString(source, columnmap, "type");
                    migratingItem.supplier = GetString(source, columnmap, "supplier");
                    migratingItem.suppliercode = GetString(source, columnmap, "suppliercode");
                    if (!string.IsNullOrEmpty(migratingItem.product))
                    {
                        if (!string.IsNullOrEmpty(migratingItem.gln))
                        {
                            migratingItem.exist = existing.Exists(x => x.gln == migratingItem.gln);
                        }
                        migrationDatas.Add(migratingItem);
                    }


                }
                catch (Exception exp)
                {
                    source.Insert(0, exp.Message);
                    data.progress.faileddata.Add(source);
                }
            }
            return migrationDatas;
        }

        public List<DisplayItem> SearchInfo(List<long> itemids, NpgsqlConnection conn)
        {
            List<DisplayItem> lst = new List<DisplayItem>();
            try
            {

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchInfo.Substring(0, SQL_SearchInfo.IndexOf("where"));
                qb.CurrentQuery += " where items.id in (" + string.Join<long>(",", itemids) + ")";
                command = qb.GetCommand(conn);
                command.CommandTimeout = 1500;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DisplayItem oitems = new DisplayItem();
                        oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        lst.Add(oitems);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, itemids);
            }
            return lst;
        }

        public List<Items> Search(ItemsCreteria _items, List<long> itemids, NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction)
        {
            List<Items> lst = new List<Items>();
            try
            {

                NpgsqlCommand command = null;
                if (_items.id != 0)
                {
                    if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                    {
                        command = GetCommand(string.Format(SQL_Search, "," + string.Join(",", CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "itm_" + x.cfname))), conn);
                    }
                    else
                    {
                        command = GetCommand(string.Format(SQL_Search, ""), conn);
                    }
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _items.id;
                }
                else
                {
                    if (!_items.productmap && this.productmap)
                        _items.productmap = true;
                    QueryBuilder qb = new QueryBuilder();

                    if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                    {
                        qb.CurrentQuery = string.Format(SQL_Search.Substring(0, SQL_Search.IndexOf("where")), "," + string.Join(",", CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "itm_" + x.cfname)));
                    }
                    else
                    {
                        qb.CurrentQuery = string.Format(SQL_Search.Substring(0, SQL_Search.IndexOf("where")), "");
                    }

                    if (itemids != null && itemids.Count > 0)
                    {
                        qb.CurrentQuery += " where id in (" + string.Join<long>(",", itemids) + ")";
                    }
                    else
                    {
                        qb.AddParameter("isactive", true, "=");
                        if (_items.intelligent)
                        {
                            qb.AddParameter("searchkeys", "searchkeys", _items.SearchKeywords2TS(), " @@ ", " to_tsquery('" + _items.SearchKeywords2TS() + "') ");
                        }
                        else if(_items.support_gln &&
                            _items.productmap &&
                            !string.IsNullOrEmpty(_items.gln)
                            )
                        {
                            qb.AddParameter("gln", _items.gln, "=");
                        }
                        else
                        {
                            if (_items.support_gln)
                                qb.AddParameter("support_gln", true, "=");

                            if (!string.IsNullOrEmpty(_items.gln))
                                qb.AddParameter("gln", _items.gln, "=");

                            if (_items.glns != null && _items.glns.Count > 0)
                                qb.AddInParamer("gln", String.Join(",", _items.glns.ConvertAll(x => "'" + x + "'")));

                            if (_items.ids != null && _items.ids.Count > 0)
                                qb.AddInParamer("id", String.Join(",", _items.ids.ConvertAll(x => x)));

                            if (_items.onlyglnitems)
                                qb.Add2Query(" gln != '' and length(gln)>0 ");

                            if (_items.onlynonuan)
                                qb.AddParameter("nonuan", true, "=");
                            
                            if (_items.onlytouchitems)
                                qb.AddParameter("support_pos", true, "=");

                            if (_items.onlyglnitems)
                                qb.Add2Query(" gln != '' and length(gln)>0 ");

                            if (_items.onlysearchable)
                                qb.AddParameter("allowsearch", true, "=");

                            if (!string.IsNullOrEmpty(_items.printingname))
                            {
                                qb.AddParameter("printingname", "%" + _items.printingname + "%", " ilike ");
                            }
                            if (!string.IsNullOrEmpty(_items.sellingname))
                            {
                                qb.AddParameter("sellingname", "%" + _items.sellingname + "%", " ilike ");
                            }
                            if (_items.brandid > 0 || _items.productmap)
                            {
                                qb.AddParameter("brandid", _items.brandid, "=");
                            }
                            if (_items.productid > 0 || _items.productmap)
                            {
                                qb.AddParameter("productid", _items.productid, "=");
                            }
                            if (!string.IsNullOrEmpty(_items.code))
                            {
                                qb.AddParameter("code", "%" + _items.code + "%", " ilike ");
                            }
                            if (_items.patternid > 0 || _items.productmap)
                                qb.AddParameter("patternid", _items.patternid, "=");
                            if (_items.colourid > 0 || _items.productmap)
                                qb.AddParameter("colourid", _items.colourid, "=");
                            if (_items.sizeid > 0 || _items.productmap)
                            {
                                qb.AddParameter("sizeid", _items.sizeid, "=");
                            }
                            if (_items.styleid > 0 || _items.productmap)
                            {
                                qb.AddParameter("styleid", _items.styleid, "=");
                            }
                            if (_items.materialid > 0 || _items.productmap)
                            {
                                qb.AddParameter("materialid", _items.materialid, "=");
                            }
                            if (_items.typeid > 0 || _items.productmap)
                            {
                                qb.AddParameter("typeid", _items.typeid, "=");
                            }
                            if (_items.fitid > 0 || _items.productmap)
                            {
                                qb.AddParameter("fitid", _items.fitid, "=");
                            }
                            if (_items.seasonid > 0)
                            {
                                qb.AddParameter("seasonid", _items.seasonid, "=");
                            }
                            if (!string.IsNullOrEmpty(_items.glndesignid))
                            {
                                qb.AddParameter("glndesignid", _items.glndesignid, "=");
                            }
                            if (_items.sleeveid > 0 || _items.productmap)
                            {
                                qb.AddParameter("sleeveid", _items.sleeveid, "=");
                            }
                            if (_items.typeid > 0 || _items.productmap)
                            {
                                qb.AddParameter("typeid", _items.typeid, "=");
                            }

                            if(_items.tradeagreeentid>0)
                                qb.AddParameter("tradeagreeentid", _items.tradeagreeentid, "=");

                            if (!string.IsNullOrEmpty(_items.agmtcode) ||
                                (_items.productmap && InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_itemcode))
                            {
                                qb.AddParameter("agmtcode", string.IsNullOrWhiteSpace(_items.agmtcode)? "": _items.agmtcode, "=");
                                qb.AddParameter("gln", string.IsNullOrWhiteSpace(_items.gln) ? "" : _items.gln, "=");
                            }

                            if (_items.productmap)
                                qb.Limit = 1;
                        }
                    }

                    if (isMigration)
                    {
                        qb.CurrentQuery = @"select id, code, sellingname, brandid, brandname, productid, productname, colourname, colourid, patternid, patternname, sizeid, sizename, styleid, stylename, salemode, printingname, 
                            pricingmode, typeid, typename, createdon, modifiedon, createdby, modifiedby, isactive,tradeagreeentid,agmtcode,classid,prddivisionid,
                            fixedsupplierid,singlesupplier,multisupplier,supplierlist,version,materialid,materialname,sleeveid,fitid,seasonid,glndesignid from (
                                            SELECT id as uniqueid,ROW_NUMBER() OVER( PARTITION BY colourid,
                                            patternid, sizeid,  styleid, 
                                            typeid, materialid,brandid,sleeveid,fitid,seasonid,glndesignid
                                                    ORDER BY  id ) AS row_num
                                                    FROM items
                                            ) as a
                                            inner join items on items.id = a.uniqueid
                                            where row_num=1";
                    }
                    command = qb.GetCommand(conn, transaction);
                }


                command.CommandTimeout = 1500;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Items oitems = new Items();
                        oitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oitems.sellingname = reader["sellingname"] == DBNull.Value ? "" : reader["sellingname"].ToString();
                        oitems.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oitems.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);
                        oitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitems.salemode = reader["salemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salemode"]);
                        oitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oitems.glndesignid = reader["glndesignid"] == DBNull.Value ? "" : reader["glndesignid"].ToString();
                        oitems.agmtcode = reader["agmtcode"] == DBNull.Value ? "" : reader["agmtcode"].ToString();
                        
                        oitems.tradeagreeentid = reader["tradeagreeentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tradeagreeentid"]);
                        oitems.fixedsupplierid = reader["fixedsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fixedsupplierid"]);
                        oitems.singlesupplier = reader["singlesupplier"] == DBNull.Value ? false : Convert.ToBoolean(reader["singlesupplier"]);
                        oitems.multisupplier = reader["multisupplier"] == DBNull.Value ? false : Convert.ToBoolean(reader["multisupplier"]);
                        oitems.supplierlist_json = reader["supplierlist"] == DBNull.Value ? "null" : Convert.ToString(reader["supplierlist"]);

                        oitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        oitems.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        oitems.seasonid = reader["seasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["seasonid"]);

                        oitems.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                        oitems.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);

                        if (!isMigration)
                        {
                            oitems.podays = reader["podays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["podays"]);
                            oitems.stockagedays = reader["stockagedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockagedays"]);
                            oitems.minrate = reader["minrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minrate"]);
                            oitems.ratetobebilled = reader["ratetobebilled"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ratetobebilled"]);
                            oitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                            oitems.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                            oitems.purchaserate = reader["purchaserate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaserate"]);
                            oitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                            oitems.allowsearch = reader["allowsearch"] == DBNull.Value ? false : Convert.ToBoolean(reader["allowsearch"]);
                            oitems.reordermin = reader["reordermin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["reordermin"]);
                            oitems.reordermax = reader["reordermax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["reordermax"]);

                            oitems.autopomin = reader["autopomin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["autopomin"]);
                            oitems.autopomax = reader["autopomax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["autopomax"]);

                            oitems.support_pos = reader["support_pos"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_pos"]);
                            oitems.support_itemlevelprice = reader["support_itemlevelprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_itemlevelprice"]);
                            oitems.support_gln = reader["support_gln"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_gln"]);
                            oitems.gln = reader["gln"] == DBNull.Value ? "" : reader["gln"].ToString();
                            oitems.support_expiry = reader["support_expiry"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_expiry"]);
                            oitems.expiryvalue = reader["expiryvalue"] == DBNull.Value ? 0 : Convert.ToInt32(reader["expiryvalue"]);
                            oitems.expirymode = reader["expirymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["expirymode"]);

                            oitems.dsmode = reader["dsmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["dsmode"]);


                            oitems.dssupport = reader["dssupport"] == DBNull.Value ? false : Convert.ToBoolean(reader["dssupport"]);
                            oitems.dsbuyingqty = reader["dsbuyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsbuyingqty"]);
                            oitems.dsfreeqty = reader["dsfreeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsfreeqty"]);
                            oitems.dscomboprice = reader["dscomboprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dscomboprice"]);
                            oitems.dsdiscountproductid = reader["dsdiscountproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dsdiscountproductid"]);
                            oitems.dsdiscountpercent = reader["dsdiscountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsdiscountpercent"]);
                            oitems.dsfromqty = reader["dsfromqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsfromqty"]);
                            oitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                            {
                                ReadCustomFields(oitems, reader);
                            }

                        }
                        lst.Add(oitems);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _items);
            }
            return lst;
        }

        public Items GetItem(Items oItems)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (oItems.allowsearch)
                {
                    command = GetCommand(SQL_SearchExist, conn);
                }
                else
                {
                    command = GetCommand(SQL_SearchExist + " and printingname=@printingname", conn);
                }

                if (!string.IsNullOrEmpty(oItems.gln))
                {
                    command.CommandText += " and gln=@gln";
                    command.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.gln;
                }

                if (string.IsNullOrEmpty(oItems.printingname))
                    oItems.printingname = oItems.productname;
                if (string.IsNullOrEmpty(oItems.sellingname))
                    oItems.sellingname = oItems.productname;

                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.brandid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.productid;
                command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.colourid;
                command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.patternid;
                command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.sizeid;
                command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.typeid;
                command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.styleid;
                command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.materialid;
                command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.sleeveid;
                command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.fitid;
                command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oItems.printingname) ? "" : oItems.printingname;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Items oitems = new Items();
                        oitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        oitems.sellingname = reader["sellingname"] == DBNull.Value ? "" : reader["sellingname"].ToString();
                        oitems.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oitems.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);
                        oitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oitems.salemode = reader["salemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salemode"]);
                        oitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oitems.agmtcode = reader["agmtcode"] == DBNull.Value ? "" : reader["agmtcode"].ToString();
                        oitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        oitems.tradeagreeentid = reader["tradeagreeentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tradeagreeentid"]);
                        oitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        oitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        oitems.podays = reader["podays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["podays"]);
                        oitems.stockagedays = reader["stockagedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockagedays"]);
                        oitems.minrate = reader["minrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minrate"]);
                        oitems.ratetobebilled = reader["ratetobebilled"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ratetobebilled"]);
                        oitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        oitems.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        oitems.purchaserate = reader["purchaserate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaserate"]);
                        oitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        oitems.allowsearch = reader["allowsearch"] == DBNull.Value ? false : Convert.ToBoolean(reader["allowsearch"]);
                        oitems.reordermin = reader["reordermin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["reordermin"]);
                        oitems.reordermax = reader["reordermax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["reordermax"]);
                        oitems.autopomin = reader["autopomin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["autopomin"]);
                        oitems.autopomax = reader["autopomax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["autopomax"]);
                        oitems.support_pos = reader["support_pos"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_pos"]);
                        oitems.support_itemlevelprice = reader["support_itemlevelprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_itemlevelprice"]);
                        oitems.support_gln = reader["support_gln"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_gln"]);
                        oitems.gln = reader["gln"] == DBNull.Value ? "" : reader["gln"].ToString();
                        oitems.support_expiry = reader["support_expiry"] == DBNull.Value ? false : Convert.ToBoolean(reader["support_expiry"]);
                        oitems.expiryvalue = reader["expiryvalue"] == DBNull.Value ? 0 : Convert.ToInt32(reader["expiryvalue"]);
                        oitems.expirymode = reader["expirymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["expirymode"]);
                        oitems.dsmode = reader["dsmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["dsmode"]);
                        oitems.dssupport = reader["dssupport"] == DBNull.Value ? false : Convert.ToBoolean(reader["dssupport"]);
                        oitems.dsbuyingqty = reader["dsbuyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsbuyingqty"]);
                        oitems.dsfreeqty = reader["dsfreeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsfreeqty"]);
                        oitems.dscomboprice = reader["dscomboprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dscomboprice"]);
                        oitems.dsdiscountproductid = reader["dsdiscountproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dsdiscountproductid"]);
                        oitems.dsdiscountpercent = reader["dsdiscountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsdiscountpercent"]);
                        oitems.dsfromqty = reader["dsfromqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsfromqty"]);
                        oitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();

                        oitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        oitems.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        oitems.seasonid = reader["seasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["seasonid"]);
                        oitems.glndesignid = reader["glndesignid"] == DBNull.Value ? "" : reader["glndesignid"].ToString();
                        oitems.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                        oitems.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                        return oitems;
                    }
                }
                conn.Close();
            }
            return null;
        }

        private void UpdateItem(Items oItems)
        {
            List<long> ids = new List<long>();
            ids.AddRange(new long[] { oItems.sizeid, oItems.patternid, oItems.styleid, oItems.typeid, oItems.colourid, oItems.materialid, oItems.sleeveid, oItems.fitid });
            ReferencelistDataService referecelistData = new ReferencelistDataService() { Context = this.Context };
            var idValues = referecelistData.Search(new ReferencelistCreteria()
            {
                ids = ids
            });
            if (oItems.sizeid > 0 && idValues.Exists(e => e.id == oItems.sizeid))
            {
                oItems.sizename = idValues.Find(e => e.id == oItems.sizeid).name;
            }
            if (oItems.patternid > 0 && idValues.Exists(e => e.id == oItems.patternid))
            {
                oItems.patternname = idValues.Find(e => e.id == oItems.patternid).name;
            }
            if (oItems.styleid > 0 && idValues.Exists(e => e.id == oItems.styleid))
            {
                oItems.stylename = idValues.Find(e => e.id == oItems.styleid).name;
            }
            if (oItems.typeid > 0 && idValues.Exists(e => e.id == oItems.typeid))
            {
                oItems.typename = idValues.Find(e => e.id == oItems.typeid).name;
            }
            if (oItems.colourid > 0 && idValues.Exists(e => e.id == oItems.colourid))
            {
                oItems.colourname = idValues.Find(e => e.id == oItems.colourid).name;
            }
            if (oItems.materialid > 0 && idValues.Exists(e => e.id == oItems.materialid))
            {
                oItems.materialname = idValues.Find(e => e.id == oItems.materialid).name;
            }
            if (oItems.sleeveid > 0 && idValues.Exists(e => e.id == oItems.sleeveid))
            {
                oItems.sleevename = idValues.Find(e => e.id == oItems.sleeveid).name;
            }
            if (oItems.fitid > 0 && idValues.Exists(e => e.id == oItems.fitid))
            {
                oItems.fitname = idValues.Find(e => e.id == oItems.fitid).name;
            }

            if (oItems.brandid > 0)
            {
                BrandDataService branddata = new BrandDataService() { Context = this.Context };
                var brands = branddata.Search(new BrandCreteria()
                {
                    id = oItems.brandid
                });
                if (brands.Count == 1)
                    oItems.brandname = brands[0].name;
            }

            if (oItems.productid > 0)
            {
                ProductsDataService pdataservice = new ProductsDataService() { Context = this.Context };
                var products = pdataservice.Search(new ProductsCreteria()
                {
                    id = oItems.productid
                });

                if (products.Count == 1)
                {
                    if (!oItems.allowsearch)
                    {
                        if (products[0].producttype == 6)
                        {
                            List<string> nameparts = new List<string>();
                            if (oItems.styleid > 0 && !string.IsNullOrEmpty(oItems.stylename)) nameparts.Add(oItems.stylename);
                            if (oItems.patternid > 0 && !string.IsNullOrEmpty(oItems.patternname)) nameparts.Add(oItems.patternname);
                            if (oItems.brandid > 0 && !string.IsNullOrEmpty(oItems.brandname)) nameparts.Add(oItems.brandname);
                            if (oItems.colourid > 0 && !string.IsNullOrEmpty(oItems.colourname)) nameparts.Add(oItems.colourname);
                            oItems.printingname = string.Join("-", nameparts);
                        }
                        else if (products[0].producttype == 7)
                        {
                            List<string> nameparts = new List<string>();
                            if (oItems.styleid > 0 && !string.IsNullOrEmpty(oItems.stylename)) nameparts.Add(oItems.stylename);
                            if (oItems.brandid > 0 && !string.IsNullOrEmpty(oItems.brandname)) nameparts.Add(oItems.brandname);
                            if (oItems.colourid > 0 && !string.IsNullOrEmpty(oItems.colourname)) nameparts.Add(products[0].code);
                            oItems.printingname = string.Join("-", nameparts);
                        }
                        else
                        {
                            oItems.printingname = products[0].name;
                        }
                    }
                    oItems.productname = products[0].name;
                }
            }

        }
        public bool isMigration = false;
        public Items Insert(Items oItems)
        {
            oItems.isactive = true;
            NpgsqlTransaction transaction = null;
            try
            {
                if (isMigration)
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        Insert(transaction, conn, oItems);
                        transaction.Commit();
                        conn.Close();
                    }
                }
                else
                {
                    UpdateItem(oItems);
                    Items existingitem = null;
                    if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowduplicateitem)
                        existingitem = GetItem(oItems);
                    if (existingitem != null)
                    {
                        oItems.id = existingitem.id;
                        oItems.version = existingitem.version;
                        Update(oItems, false);
                    }
                    else
                    {
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                            Insert(transaction, conn, oItems);
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oItems);
                //
                //    
            }

            return oItems;
        }

     
        public Items Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Items oItems)
        {

            if(InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_itemcode &&
                string.IsNullOrEmpty(oItems.agmtcode))
            {
                throw new Exception("Please create item along with item code");
            }

            if (!isMigration && oItems.productid <= 0) throw new Exception("Please create/select the product before the new item master creation.");

            Data.ReferencelistDataService referencelist = new ReferencelistDataService() { Context = this.Context };

            NpgsqlCommand command = null;

            if (!isMigration && ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                var flist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "itm_" + x.cfname);
                var vlist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "@" + "itm_" + x.cfname);
                var query = string.Format(SQL_Insert, "," + string.Join(",", flist), "," + string.Join(",", vlist));
                command = GetCommand(query, conn, transaction);
                AddCustomFields(oItems, command);
            }
            else
            {
                command = new NpgsqlCommand(string.Format(SQL_Insert, "", ""), conn, transaction);
            }

            if (!isMigration)
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get(oItems.allowsearch ? "items.businesscode" : "items.code", 0);
                oItems.code = sequence.GetNextNumber(this.Context, conn, transaction);
            }


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 && 
               oItems.mrp==0)
            {
                throw new Exception("Can't create items without MRP value");
            }
                command.Parameters.Add("productname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.productname == null ? "" : oItems.productname;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.productid;

            command.Parameters.Add("agmtcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.agmtcode == null ? "" : oItems.agmtcode;
            command.Parameters.Add("tradeagreeentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.tradeagreeentid;
            command.Parameters.Add("fixedsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.fixedsupplierid;
            command.Parameters.Add("disablepo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.disablepo;
            command.Parameters.Add("singlesupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.singlesupplier;
            command.Parameters.Add("multisupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.multisupplier;
            command.Parameters.Add("supplierlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oItems.supplierlist_json == null ? "null" : oItems.supplierlist_json;

            command.Parameters.Add("brandname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.brandname == null ? "" : oItems.brandname;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.brandid;
            command.Parameters.Add("sellingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.sellingname == null ? "" : oItems.sellingname;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.code == null ? "" : oItems.code;

            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("pricingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.pricingmode;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.printingname == null ? "" : oItems.printingname;
            command.Parameters.Add("salemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.salemode;
            command.Parameters.Add("stylename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.stylename == null ? "" : oItems.stylename;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.styleid;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.sizename == null ? "" : oItems.sizename;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.sizeid;
            command.Parameters.Add("patternname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.patternname == null ? "" : oItems.patternname;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.patternid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.colourid;
            command.Parameters.Add("colourname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.colourname == null ? "" : oItems.colourname;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("typename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.typename == null ? "" : oItems.typename;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.typeid;
            command.Parameters.Add("materialname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.materialname == null ? "" : oItems.materialname;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.materialid;
            command.Parameters.Add("searchkeys", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.searchkeys == null ? "" : oItems.searchkeys;
            command.Parameters.Add("podays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.podays;
            command.Parameters.Add("stockagedays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.stockagedays;
            command.Parameters.Add("minrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.minrate;
            command.Parameters.Add("ratetobebilled", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.ratetobebilled;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.mrp;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.salerate;
            command.Parameters.Add("purchaserate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.purchaserate;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dealerprice;
            command.Parameters.Add("allowsearch", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.allowsearch;
            command.Parameters.Add("reordermin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.reordermin;
            command.Parameters.Add("reordermax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.reordermax;
            command.Parameters.Add("autopomax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.autopomax;
            command.Parameters.Add("autopomin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.autopomin;

            command.Parameters.Add("support_pos", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_pos;
            command.Parameters.Add("support_itemlevelprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_itemlevelprice;
            command.Parameters.Add("support_gln", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_gln;
            command.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.gln == null ? "" : oItems.gln;
            command.Parameters.Add("support_expiry", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_expiry;
            command.Parameters.Add("expiryvalue", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.expiryvalue;
            command.Parameters.Add("expirymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.expirymode;
            command.Parameters.Add("dsmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.dsmode;
            command.Parameters.Add("dssupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.dssupport;
            command.Parameters.Add("dsbuyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsbuyingqty;
            command.Parameters.Add("dsfreeqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsfreeqty;
            command.Parameters.Add("dscomboprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dscomboprice;
            command.Parameters.Add("dsdiscountproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.dsdiscountproductid;
            command.Parameters.Add("dsdiscountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsdiscountpercent;
            command.Parameters.Add("dsfromqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsfromqty;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.imageid == null ? "" : oItems.imageid;


            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.sleeveid;
            command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.fitid;
            command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.seasonid;

            command.Parameters.Add("classid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.classid;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.prddivisionid;

            command.Parameters.Add("glndesignid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.glndesignid == null ? "" : oItems.glndesignid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oItems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oItems;
        }

        public Items Update(Items oItems, bool validate)
        {
            oItems.isactive = true;
            NpgsqlTransaction transaction = null;
            try
            {
                if (validate)
                {
                    UpdateItem(oItems);
                    Items existingitem = null;
                    if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_allowduplicateitem)
                        existingitem = GetItem(oItems);
                    if (existingitem != null)
                    {
                        if (existingitem.id != oItems.id)
                        {
                            oItems.id = existingitem.id;
                            oItems.version = existingitem.version;
                        }
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oItems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oItems);
            }

            return oItems;
        }

        public Items Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Items oItems)
        {


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
               oItems.mrp == 0)
            {
                throw new Exception("Can't create items without MRP value");
            }

            if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_itemcode &&
                string.IsNullOrEmpty(oItems.agmtcode))
            {
                throw new Exception("Please create item along with item code");
            }

            NpgsqlCommand command = null;
            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                var flist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "itm_" + x.cfname + "=@itm_" + x.cfname);
                var query = string.Format(SQL_Update, "," + string.Join(",", flist));
                command = GetCommand(query, conn, transaction);
                AddCustomFields(oItems, command);
            }
            else
            {
                command = GetCommand(string.Format(SQL_Update, ""), conn, transaction);
            }

            command.Parameters.Add("fixedsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.fixedsupplierid;
            command.Parameters.Add("disablepo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.disablepo;
            command.Parameters.Add("tradeagreeentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.tradeagreeentid;
            command.Parameters.Add("agmtcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.agmtcode == null ? "" : oItems.agmtcode;
            command.Parameters.Add("singlesupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.singlesupplier;
            command.Parameters.Add("multisupplier", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.multisupplier;
            command.Parameters.Add("supplierlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oItems.supplierlist_json == null ? "null" : oItems.supplierlist_json;

            command.Parameters.Add("productname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.productname == null ? "" : oItems.productname;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.productid;
            command.Parameters.Add("brandname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.brandname == null ? "" : oItems.brandname;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.brandid;
            command.Parameters.Add("sellingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.sellingname == null ? "" : oItems.sellingname;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.code == null ? "" : oItems.code;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oItems.createdon;
            command.Parameters.Add("pricingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.pricingmode;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.printingname == null ? "" : oItems.printingname;
            command.Parameters.Add("salemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.salemode;
            command.Parameters.Add("stylename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.stylename == null ? "" : oItems.stylename;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.styleid;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.sizename == null ? "" : oItems.sizename;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.sizeid;
            command.Parameters.Add("patternname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.patternname == null ? "" : oItems.patternname;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.patternid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.colourid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.id;
            command.Parameters.Add("colourname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.colourname == null ? "" : oItems.colourname;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("typename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.typename == null ? "" : oItems.typename;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.typeid;
            command.Parameters.Add("materialname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.materialname == null ? "" : oItems.materialname;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.materialid;
            command.Parameters.Add("searchkeys", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.searchkeys == null ? "" : oItems.searchkeys;
            command.Parameters.Add("podays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.podays;
            command.Parameters.Add("stockagedays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.stockagedays;
            command.Parameters.Add("minrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.minrate;
            command.Parameters.Add("ratetobebilled", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.ratetobebilled;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.mrp;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.salerate;
            command.Parameters.Add("purchaserate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.purchaserate;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dealerprice;
            command.Parameters.Add("allowsearch", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.allowsearch;
            command.Parameters.Add("reordermin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.reordermin;
            command.Parameters.Add("reordermax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.reordermax;
            command.Parameters.Add("autopomax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.autopomax;
            command.Parameters.Add("autopomin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.autopomin;

            command.Parameters.Add("support_pos", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_pos;
            command.Parameters.Add("support_itemlevelprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_itemlevelprice;
            command.Parameters.Add("support_gln", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_gln;
            command.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.gln == null ? "" : oItems.gln;
            command.Parameters.Add("support_expiry", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.support_expiry;
            command.Parameters.Add("expiryvalue", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.expiryvalue;
            command.Parameters.Add("expirymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.expirymode;
            command.Parameters.Add("dsmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.dsmode;
            command.Parameters.Add("dssupport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.dssupport;
            command.Parameters.Add("dsbuyingqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsbuyingqty;
            command.Parameters.Add("dsfreeqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsfreeqty;
            command.Parameters.Add("dscomboprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dscomboprice;
            command.Parameters.Add("dsdiscountproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.dsdiscountproductid;
            command.Parameters.Add("dsdiscountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsdiscountpercent;
            command.Parameters.Add("dsfromqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oItems.dsfromqty;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.imageid == null ? "" : oItems.imageid;

            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.sleeveid;
            command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.fitid;
            command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.seasonid;
            command.Parameters.Add("classid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.classid;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.prddivisionid;
            command.Parameters.Add("glndesignid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oItems.glndesignid == null ? "" : oItems.glndesignid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oItems.version = Convert.ToInt32(reader[0]);
                }
            }

            return oItems;
        }


        public bool Delete(Items oItems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oItems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oItems);



            }
            return hasSuccessCount;

        }

        public bool SyncDenormalizedNames()
        {
            string query = @"update items set printingname = products.name,productname = products.name from products
                            where products.id = items.productid and (printingname != products.name or productname != products.name);

                            update items set brandname = brand.name from brand
                            where brand.id = items.brandid and  brandname != brand.name;

                            update items set patternname = referencelist.name from referencelist
                            where referencelist.id = items.patternid and patternname != referencelist.name ;

                            update items set typename = referencelist.name from referencelist
                            where referencelist.id = items.typeid and typename != referencelist.name;

                            update items set materialname = referencelist.name from referencelist
                            where referencelist.id = items.materialid and materialname != referencelist.name;

                            update items set stylename = referencelist.name from referencelist
                            where referencelist.id = items.styleid and stylename != referencelist.name ;

                            update items set colourname = referencelist.name from referencelist
                            where referencelist.id = items.colourid and colourname != referencelist.name ;

                            update items set sizename = referencelist.name from referencelist
                            where referencelist.id = items.sizeid and sizename != referencelist.name;

                            update stock set printingname = products.name from products
                            where products.id = stock.productid and printingname != products.name ;";
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    hasSuccessCount = command.ExecuteNonQuery() > 0;
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "SyncDenormalizedNames");
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Items oItems)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oItems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oItems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oItems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public string RemoveDuplicateRecords()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            SortedDictionary<long, List<long>> changeids = new SortedDictionary<long, List<long>>();
            string query = @"				With DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM products where isactive=true)
											
											SELECT firstrow.id,dup.id,'Products '||firstrow.name,'UPDATE items set productid='||firstrow.id||' where productid='||dup.id  || ';' || E'\r\n' ||
                                            'update products set isactive=false where id='||dup.id||';' || E'\r\n' ||
											'UPDATE lrinvoiceitems set productid='||firstrow.id||' where productid='|| dup.id || ';' || E'\r\n' ||
											'UPDATE stock set productid='||firstrow.id||' where productid='||dup.id || ';' || E'\r\n' ||
											'UPDATE billitems set productid='||firstrow.id||' where productid='||dup.id || ';'  query FROM DUP
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;

											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM brand where isactive=true)
											
											SELECT firstrow.id,dup.id,'Brand '||firstrow.name,'UPDATE items set brandid='||firstrow.id||' where brandid='||dup.id||';' || E'\r\n'
                                            'update brand set isactive=false where id='||dup.id||';' FROM DUP
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;

											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM referencelist where type ='MATERIAL' and  isactive=true)
											
											SELECT firstrow.id,dup.id,'Material '||firstrow.name,'UPDATE items set materialid='||firstrow.id||' where materialid='||dup.id||';' || E'\r\n'
                                            'update referencelist set isactive=false where id='||dup.id||';' FROM DUP
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;
											
											
											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM referencelist where type ='STYLE' and  isactive=true)
											
											SELECT firstrow.id,dup.id,'Style '||firstrow.name,'UPDATE items set styleid='||firstrow.id||' where styleid='||dup.id||';'|| E'\r\n'
                                            'update referencelist set isactive=false where id='||dup.id||';' FROM DUP  
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;
											
											
											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM referencelist where type ='SIZE' and  isactive=true)
											
											SELECT firstrow.id,dup.id,'Size '||firstrow.name,'UPDATE items set styleid='||firstrow.id||' where styleid='||dup.id||';'|| E'\r\n'
                                            'update referencelist set isactive=false where id='||dup.id||';' FROM DUP
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;
											
											
											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM referencelist where type ='PATTERN' and  isactive=true)
											
											SELECT firstrow.id,dup.id,'Pattern '||firstrow.name,'UPDATE items set patternid='||firstrow.id||' where patternid='||dup.id||';'|| E'\r\n'
                                            'update referencelist set isactive=false where id='||dup.id||';' FROM DUP 
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;
											
											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM referencelist where type ='TYPE' and  isactive=true)
											
											SELECT firstrow.id,dup.id,'Type '||firstrow.name,'UPDATE items set typeid='||firstrow.id||' where typeid='||dup.id||';'|| E'\r\n'
                                            'update referencelist set isactive=false where id='||dup.id||';' FROM DUP
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;
											
											with DUP as (
                                            SELECT ID,NAME, row_number() OVER (PARTITION BY name ORDER BY name ASC) as ranknumber  FROM referencelist where type ='COLOUR' and  isactive=true)
											
											SELECT firstrow.id,dup.id,'Colour '||firstrow.name,'UPDATE items set colourid='||firstrow.id||' where colourid='||dup.id||';'|| E'\r\n'
                                            'update referencelist set isactive=false where id='||dup.id||';' FROM DUP
											inner join dup firstrow on firstrow.name = dup.name and firstrow.ranknumber = 1
											WHERE DUP.ranknumber >1;
                                            
                                            with DUP as (
                                            SELECT ID, productid, brandid, sizeid, colourid, styleid, materialid, typeid, row_number() OVER(PARTITION BY productid, brandid, sizeid, colourid, styleid, materialid, typeid order by productid ASC) as ranknumber
                                            FROM items where isactive = true),DUPLICATEITEM AS
                                            (
                                                SELECT ft.id, dup.id duplicateid,
											    'UPDATE lrinvoiceitems set itemid='||ft.id||' where itemid='|| dup.id || ';' || E'\r\n' ||
											    'UPDATE stock set itemid='||ft.id||' where itemid='||dup.id || ';' || E'\r\n' ||
											    'UPDATE salablegoods set itemid='||ft.id||' where itemid='||dup.id || ';'  query FROM DUP
                                                inner join DUP ft on
                                                ft.productid = DUP.productid and
                                                ft.brandid= DUP.brandid and
                                                ft.sizeid= DUP.sizeid and
                                                ft.colourid = DUP.colourid and
                                                ft.styleid= DUP.styleid and
                                                ft.materialid = DUP.materialid and
                                                ft.typeid = DUP.typeid and
                                                ft.ranknumber = 1
                                                WHERE DUP.ranknumber > 1
                                            )
											select * from DUPLICATEITEM DUP;";

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                List<Items> lst = new List<Items>();
                var command = GetCommand(query, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        stringBuilder.AppendLine("--" + reader.GetString(2));
                        stringBuilder.AppendLine(reader.GetString(3));
                    }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine("--" + reader.GetString(2));
                            stringBuilder.AppendLine(reader.GetString(3));
                        }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine("--" + reader.GetString(2));
                            stringBuilder.AppendLine(reader.GetString(3));
                        }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine("--" + reader.GetString(2));
                            stringBuilder.AppendLine(reader.GetString(3));
                        }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine("--" + reader.GetString(2));
                            stringBuilder.AppendLine(reader.GetString(3));
                        }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine("--" + reader.GetString(2));
                            stringBuilder.AppendLine(reader.GetString(3));
                        }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine("--" + reader.GetString(2));
                            stringBuilder.AppendLine(reader.GetString(3));
                        }
                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            stringBuilder.AppendLine(reader.GetString(3));
                        }

                    stringBuilder.AppendLine(@"
                                            with allids as
											(
											select itemid from lrinvoiceitems
											union all
											select itemid from stock
											union all 
											select itemid from salablegoods
											)
											,duplicateid as
											(
											select items.id from items
											left outer join allids on allids.itemid = items.id
											where allids.itemid is null and items.isactive = true
											)		
											update items set isactive = false from duplicateid
											where duplicateid.id = items.id;
									
                                            with DUP as (
                                            SELECT ID, productid, brandid, sizeid, colourid, styleid, materialid, typeid, row_number() OVER(PARTITION BY productid, brandid, sizeid, colourid, styleid, materialid, typeid order by productid ASC) as ranknumber

                                            FROM items where isactive = true),DUPLICATEITEM AS
                                            (
                                            SELECT ft.id, dup.idduplicateid,
											'UPDATE lrinvoiceitems set itemid='||ft.id||' where itemid='|| dup.id || ';' || E'\r\n' ||
											'UPDATE stock set itemid='||ft.id||' where itemid='||dup.id || ';' || E'\r\n' ||
											'UPDATE salablegoods set itemid='||ft.id||' where itemid='||dup.id || ';'  query FROM DUP
                                            inner join DUP ft on

                                            ft.productid = DUP.productid and
                                            ft.brandid= DUP.brandid and
                                            ft.sizeid= DUP.sizeid and
                                            ft.colourid = DUP.colourid and
                                            ft.styleid= DUP.styleid and
                                            ft.materialid = DUP.materialid and
                                            ft.typeid = DUP.typeid and
                                            ft.ranknumber = 1
                                            WHERE DUP.ranknumber > 1)
	
											select *
											from DUPLICATEITEM DUP;");


                }
                conn.Close();
            }
            return stringBuilder.ToString();
        }

        public string RemoveDuplicate()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            SortedDictionary<long, List<long>> changeids = new SortedDictionary<long, List<long>>();
            List<long> disablingItemid = new List<long>();

            string query = @"with reference as
                            (
                                select * from referencelist
                            )
                            select items.id,productid,patternid,styleid,sizeid,materialid,typeid,colourid,brandid,
                            products.name productname,
                            pattern.name as patternname,style.name as stylename,size.name as sizename,
                            material.name as materialname,
                            type.name as typename,colour.name as colourname,brand.name brandname from items
                            inner join products on products.id = productid
                            left outer join brand on brand.id = brandid
                            left outer join reference pattern on pattern.id = patternid
                            left outer join reference style on style.id = styleid
                            left outer join reference size on size.id = sizeid
                            left outer join reference material on material.id = materialid
                            left outer join reference type on type.id = typeid
                            left outer join reference colour on colour.id = colourid
                            WHERE material.name = 'COTTON'";

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    List<Items> lst = new List<Items>();
                    var command = GetCommand(query, conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Items oitems = new Items();
                            oitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());
                            oitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                            oitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                            oitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                            oitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());
                            oitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"].ToString());
                            oitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                            oitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                            oitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                            oitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                            oitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"].ToString());
                            oitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                            oitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"].ToString());
                            oitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                            oitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"].ToString());
                            oitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                            lst.Add(oitems);
                        }
                    }
                    conn.Close();
                    Dictionary<string, KeyValuePair<long, List<Items>>> colourname = lst.GroupBy(x => x.colourname)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().colourid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> patternname = lst.GroupBy(x => x.patternname)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().patternid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> sizename = lst.GroupBy(x => x.sizename)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().sizeid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> typename = lst.GroupBy(x => x.typename)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().typeid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> stylename = lst.GroupBy(x => x.stylename)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().styleid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> materialname = lst.GroupBy(x => x.materialname)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().materialid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> brandname = lst.GroupBy(x => x.brandname)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().brandid, x.ToList()));

                    Dictionary<string, KeyValuePair<long, List<Items>>> productname = lst.GroupBy(x => x.productname)
                                    .OrderBy(p => p.Key.ToString())
                                    .ToDictionary(x => x.Key, x => new KeyValuePair<long, List<Items>>(x.First().productid, x.ToList()));

                    /*lst.ForEach(
                        e =>
                        {
                            e.changed = (e.materialid != materialname[e.materialname].First().materialid ||
                        e.sizeid != sizename[e.sizename].First().sizeid ||
                        e.typeid != typename[e.typename].First().typeid ||
                        e.styleid != stylename[e.stylename].First().styleid ||
                        e.patternid != patternname[e.patternname].First().patternid ||
                        e.colourid != colourname[e.colourname].First().colourid ||
                        e.brandid != brandname[e.brandname].First().brandid ||
                        e.productid != productname[e.productname].First().productid);

                            if (e.changed)
                            {
                                e.materialid = materialname[e.materialname].First().materialid;
                                e.sizeid = sizename[e.sizename].First().sizeid;
                                e.typeid = typename[e.typename].First().typeid;
                                e.styleid = stylename[e.stylename].First().styleid;
                                e.patternid = patternname[e.patternname].First().patternid;
                                e.colourid = colourname[e.colourname].First().colourid;
                                e.brandid = brandname[e.brandname].First().brandid;
                                e.productid = productname[e.productname].First().productid;
                            }
                        }); */

                    /*lst.ForEach(e => e.patternid = patternname.First(f => f.Key == e.patternname).First().patternid);
                    lst.ForEach(e => e.sizeid = sizename.First(f => f.Key == e.sizename).First().sizeid);
                    lst.ForEach(e => e.typeid = typename.First(f => f.Key == e.typename).First().typeid);
                    lst.ForEach(e => e.styleid = stylename.First(f => f.Key == e.stylename).First().styleid);
                    lst.ForEach(e => e.colourid = colourname.First(f => f.Key == e.colourname).First().colourid);*/

                    var coregroups = lst.GroupBy(g => new { g.productname, g.brandname, g.colourname, g.patternname, g.sizename, g.typename, g.stylename, g.materialname });

                    /*
                            update items set isactive = false where id in (
                            select items.id from items
                            left outer join stock on stock.itemid = items.id
                            left outer join lrinvoiceitems on lrinvoiceitems.itemid = items.id
                            left outer join salablegoods on salablegoods.itemid = items.id
                            where stock.id is null and lrinvoiceitems.id is null and salablegoods.id is null
	                            )
	
	
	
                            update referencelist set isactive = true
                            where type = 'STYLE';	

                            update referencelist set isactive = false
                            where type = 'STYLE' and not id in (select styleid from items where ISACTIVE = true)

                            update referencelist set isactive = true
                            where type = 'PATTERN';	

                            update referencelist set isactive = false
                            where type = 'PATTERN' and not id in (select patternid from items where ISACTIVE = true)

                            update referencelist set isactive = true
                            where type = 'MATERIAL';	

                            update referencelist set isactive = false
                            where type = 'MATERIAL' and not id in (select materialid from items where ISACTIVE = true)

                            update referencelist set isactive = true
                            where type = 'SIZE';

                            update referencelist set isactive = false
                            where type = 'SIZE' and  id not in (select sizeid from items where ISACTIVE = true)

                            update referencelist set isactive = true
                            where type = 'TYPE';

                            update referencelist set isactive = false
                            where type = 'TYPE' and  id not in (select typeid from items where ISACTIVE = true)	

                            update referencelist set isactive = true
                            where type = 'COLOUR';

                            update referencelist set isactive = false
                            where type = 'COLOUR' and id not in (select colourid from items where ISACTIVE = true)	
*/
                    var updatelist = new List<Items>();
                    foreach (var core in coregroups)
                    {
                        var items = core.ToList().OrderBy(e => e.id).ToList();
                        Items item = null;
                        var firstitem = items.FindAll(e => e.materialid == materialname[e.materialname].Key &&
                        e.sizeid == sizename[e.sizename].Key &&
                        e.typeid == typename[e.typename].Key &&
                        e.styleid == stylename[e.stylename].Key &&
                        e.patternid == patternname[e.patternname].Key &&
                        e.colourid == colourname[e.colourname].Key &&
                        e.brandid == brandname[e.brandname].Key &&
                        e.productid == productname[e.productname].Key);
                        if (firstitem.Count > 0)
                        {
                            item = firstitem.First();
                        }
                        else
                        {
                            item = items.First();
                            item.changed = true;
                            item.materialid = materialname[item.materialname].Key;
                            item.sizeid = sizename[item.sizename].Key;
                            item.typeid = typename[item.typename].Key;
                            item.styleid = stylename[item.stylename].Key;
                            item.patternid = patternname[item.patternname].Key;
                            item.colourid = colourname[item.colourname].Key;
                            item.brandid = brandname[item.brandname].Key;
                            item.productid = productname[item.productname].Key;
                        }
                        items.Remove(item);
                        if (item.changed)
                            updatelist.Add(item);

                        //Update(transaction, conn, item);
                        if (items.Count > 0)
                        {
                            changeids.Add(item.id, items.ConvertAll(e => e.id));
                            //stringBuilder.AppendFormat("Update lrinvoiceitems set itemid={0} where itemid in ({1});\r\n", item.id, string.Join(",", items.ConvertAll(e => e.id)));
                            //stringBuilder.AppendFormat("Update stock set itemid={0} where itemid in ({1});\r\n", item.id, string.Join(",", items.ConvertAll(e => e.id)));
                            //stringBuilder.AppendFormat("Update salablegoods set itemid={0} where itemid in ({1});\r\n", item.id, string.Join(",", items.ConvertAll(e => e.id)));
                            //stringBuilder.AppendFormat("Update items set isactive=false where id in ({1});\r\n", item.id, string.Join(",", items.ConvertAll(e => e.id)));
                        }
                    }

                    var list = new List<long>();
                    foreach (var itm in changeids)
                    {
                        list.AddRange(itm.Value);
                        if (list.Count > 5000)
                        {
                            stringBuilder.AppendFormat("Update items set isactive=false where id in ({0});\r\n", string.Join(",", list));
                            list.Clear();
                        }
                    }
                    stringBuilder.AppendFormat("Update items set isactive=false where id in ({0});\r\n", string.Join(",", list));

                    list.Clear();
                    foreach (var oitem in colourname)
                    {
                        if (oitem.Value.Value.Count > 1)
                        {
                            list.AddRange(oitem.Value.Value.Where(x => x.colourid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.colourid).Distinct());
                        }
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update referencelist set isactive=false where id in ({0});\r\n",
                            string.Join(",", list.Distinct()));

                    list.Clear();
                    foreach (var oitem in patternname)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.patternid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.patternid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update referencelist set isactive=false where id in ({0});\r\n",
                            string.Join(",", list.Distinct()));
                    list.Clear();

                    foreach (var oitem in sizename)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.sizeid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.sizeid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update referencelist set isactive=false where id in ({0});\r\n",
                            string.Join(",", list.Distinct()));
                    list.Clear();

                    foreach (var oitem in typename)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.typeid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.typeid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update referencelist set isactive=false where id in ({0});\r\n",
                            string.Join(",", list.Distinct()));
                    list.Clear();

                    foreach (var oitem in stylename)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.styleid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.styleid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update referencelist set isactive=false where id in ({0});\r\n",
                            string.Join(",", list.Distinct()));
                    list.Clear();

                    foreach (var oitem in materialname)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.materialid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.materialid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update referencelist set isactive=false where id in ({0});\r\n",
        string.Join(",", list.Distinct()));
                    list.Clear();

                    foreach (var oitem in brandname)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.brandid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.brandid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update brand set isactive=false where id in ({0});\r\n",
                    string.Join(",", list.Distinct()));

                    list.Clear();

                    foreach (var oitem in productname)
                    {
                        if (oitem.Value.Value.Count > 1)
                            list.AddRange(oitem.Value.Value
                                .Where(x => x.productid != oitem.Value.Key)
                                .ToList().ConvertAll(e => e.productid).Distinct());
                    }

                    if (list.Count > 0)
                        stringBuilder.AppendFormat("Update products set isactive=false where id in ({0});\r\n",
                    string.Join(",", list.Distinct()));

                    foreach (var kv in changeids)
                    {
                        foreach (var kvv in kv.Value)
                        {
                            stringBuilder.AppendFormat("INSERT INTO public.duplicate(sourceid, duplicateid) VALUES({0},{1});\r\n", kv.Key, kvv);
                        }
                        //stringBuilder.AppendFormat("{0}-{1}\r\n", kv.Key, string.Join(",", kv.Value));
                    }
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    foreach (var item in updatelist)
                        Update(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }

                return stringBuilder.ToString();
            }
            catch (Exception exp)
            {
                return exp.Message;
            }
        }

        internal List<CommonList> SearchList(List<long> ids)
        {
            List<CommonList> result = new List<CommonList>();
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = "select id,printingname from items where id in (" + string.Join<long>(",", ids) + ")";
                    NpgsqlCommand command = qb.GetCommand(conn);
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CommonList oitems = new CommonList();
                            oitems.Name = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                            oitems.ID = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            result.Add(oitems);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "SearchList");
            }
            return result;

        }


        private void ReadCustomFields(Items oLrinvoiceitems,
            IDataReader reader)
        {
            oLrinvoiceitems.CustomFields = new List<Items.CustomFieldValue>();
            foreach (var field in CustomFieldsManager.Instance(Context.OrganizationID).Config)
            {
                if (reader["itm_" + field.cfname] == DBNull.Value)
                {
                    oLrinvoiceitems.CustomFields.Add(new Items.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = null
                    });
                }
                else if (field.datatype == 1)
                {
                    oLrinvoiceitems.CustomFields.Add(new Items.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = reader["itm_" + field.cfname].ToString()
                    });
                }
                else if (field.datatype == 2)
                {
                    oLrinvoiceitems.CustomFields.Add(new Items.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToDecimal(reader["itm_" + field.cfname]).ToString()
                    });
                }
                else if (field.datatype == 3)
                {
                    oLrinvoiceitems.CustomFields.Add(new Items.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToDateTime(reader["itm_" + field.cfname]).ToString()
                    });
                }
                else
                {
                    oLrinvoiceitems.CustomFields.Add(new Items.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToInt64(reader["itm_" + field.cfname]).ToString()
                    });
                }
            }
        }



        public List<TouchItem> TouchItems(long locationid)
        {
            string query = @"select prdgrp.id productgroupid,prdgrp.name productgroup,products.id productid,
                            products.name productname,items.sellingname,sizes.name sizename,
                            dssupport,dsmode, dsbuyingqty, dsfreeqty, dscomboprice,products.salestaxid,
                            dsdiscountproductid, dsdiscountpercent, dsfromqty,gln,
                            items.id itemid,items.salerate,items.mrp,items.imageid
                            from products
                            left outer join referencelist prdgrp on prdgrp.id = products.productgroup
                            left outer join items on items.productid = products.id
                            left outer join referencelist sizes on sizes.id = items.sizeid
                            where producttype=4 and gln != '' and support_gln=true and 
                            products.isactive=true and items.isactive=true;";

            List<TouchItem> touchItems = new List<TouchItem>();

            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                while (reader.Read())
                {
                    TouchItem touchItem = null;
                    var productgroupid = reader["productgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroupid"]);
                    if (!touchItems.Exists(x => x.productgroupid == productgroupid))
                    {
                        touchItem = new TouchItem() { productgroupid = productgroupid, products = new List<TouchItem.Product>() };
                        touchItem.productgroup = reader["productgroup"] == DBNull.Value ? "Non Mapped" : Convert.ToString(reader["productgroup"]);
                        touchItems.Add(touchItem);
                    }
                    else
                    {
                        touchItem = touchItems.Find(x => x.productgroupid == productgroupid);
                    }
                    TouchItem.Product product = new TouchItem.Product();
                    product.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                    product.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    product.taxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"]);

                    product.productname = reader["productname"] == DBNull.Value ? "" : Convert.ToString(reader["productname"]);
                    product.sellingname = reader["sellingname"] == DBNull.Value ? "" : Convert.ToString(reader["sellingname"]);
                    product.rate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                    product.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                    product.imageid = reader["imageid"] == DBNull.Value ? "" : Convert.ToString(reader["imageid"]);
                    product.sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]);
                    product.barcode = reader["gln"] == DBNull.Value ? "" : Convert.ToString(reader["gln"]);
                    if (reader["dssupport"] != DBNull.Value &&
                        Convert.ToBoolean(reader["dssupport"]))
                    {
                        product.discount = new TouchItem.DiscountConfig();
                        product.discount.dsmode = reader["dsmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["dsmode"]);
                        product.discount.dsbuyingqty = reader["dsbuyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsbuyingqty"]);
                        product.discount.dsfreeqty = reader["dsfreeqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsfreeqty"]);
                        product.discount.dscomboprice = reader["dscomboprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dscomboprice"]);
                        product.discount.dsdiscountpercent = reader["dsdiscountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsdiscountpercent"]);
                        product.discount.dsfromqty = reader["dsbuyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dsfromqty"]);
                        product.discount.dsdiscountproductid = reader["dsdiscountproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dsdiscountproductid"]);
                    }

                    if(!touchItem.products.Exists(x=>x.productid == product.productid))
                        touchItem.products.Add(product);
                }
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var foodcounters = referencelistDataService.Search(new ReferencelistCreteria()
            {
                type = "FOODCOUNTER",
                referencelocationid = locationid
            });

            foreach(var fc in foodcounters)
            {
                if(fc.additionalinfo != null &&
                    fc.additionalinfo.prdgroups != null &&
                    fc.additionalinfo.prdgroups.Count>0)
                {
                    var fooditems = touchItems.FindAll(x => fc.additionalinfo.prdgroups.Contains(x.productgroupid));
                    if(fooditems != null &&
                        fooditems.Count>0)
                    {
                        fooditems.ForEach(x =>
                        {
                            x.foodcounter = fc.name;
                            x.foodcounterid = fc.id;
                        });
                    }
                }
            }
            touchItems = touchItems.OrderBy(x => x.productgroup).ToList();
            touchItems.ForEach(x =>
            {
                x.products = x.products.OrderBy(y => y.productname).ToList();
            });
            return touchItems;
        }
        private void AddCustomFields(Items oLrinvoiceitems,
            NpgsqlCommand command)
        {
            foreach (var field in CustomFieldsManager.Instance(Context.OrganizationID).Config)
            {
                var hasvalue = (oLrinvoiceitems.CustomFields != null && oLrinvoiceitems.CustomFields.Exists(x => x.key == field.cfname));
                Items.CustomFieldValue cfvalue = null;
                if (hasvalue)
                {
                    cfvalue = oLrinvoiceitems.CustomFields.Find(x => x.key == field.cfname);
                }
                if (field.datatype == 1)
                {
                    command.Parameters.Add("@" + "itm_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Varchar).Value =
                        !hasvalue ? "" : (string.IsNullOrEmpty(cfvalue.value) ? "" : cfvalue.value);
                }
                else if (field.datatype == 2)
                {
                    decimal value = 0;
                    if (hasvalue)
                        decimal.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + "itm_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Numeric).Value = !hasvalue ? 0 : value;
                }
                else if (field.datatype == 3)
                {
                    DateTime value = DateTime.MinValue;
                    if (hasvalue)
                        DateTime.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + "itm_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Timestamp).Value = value;
                }
                else
                {
                    long value = 0;
                    if (hasvalue)
                        long.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + "itm_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Bigint).Value = value;
                }
            }
        }

        public List<NonUANItem> SearchNonUANItem()
        {
            string query = @"select items.id,gln,printingname,products.name,referencelist.name productgroup,products.sellingmode,
            items.imageid,items.salerate,items.mrp from items
            inner join products on products.id = items.productid
            left outer join referencelist on referencelist.id = products.productgroup
            where nonuan=true and items.isactive=true";

            var response = new List<NonUANItem>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    response.Add(new NonUANItem()
                    {
                        id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]),
                        gln = reader["gln"] == DBNull.Value ?  "" : reader["gln"].ToString(),
                        imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString(),
                        productgroup = reader["productgroup"] == DBNull.Value ? "" : reader["productgroup"].ToString(),
                        price = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]),
                        mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]),
                        printingname = reader["printingname"] == DBNull.Value ? reader["name"].ToString() : reader["printingname"].ToString(),
                    });
                }
            }
            return response;
        }

        public List<LookAheadItem> SearchProductBrand()
        {
            string query = @"with uniqueprodbrand
as
(
select distinct items.productid,items.brandid from items
)

select products.name ||' '||brand.name ""name"",
uniqueprodbrand.productid || '_'||uniqueprodbrand.brandid id from uniqueprodbrand
inner join products on products.id = uniqueprodbrand.productid
inner join brand on brand.id = uniqueprodbrand.brandid";

            var response = new List<LookAheadItem>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    response.Add(new LookAheadItem()
                    {
                        id = reader["id"] == DBNull.Value ? "0_0" : reader["id"].ToString(),
                        name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString()
                    });
                }
            }
            return response;
        }
    }

}
