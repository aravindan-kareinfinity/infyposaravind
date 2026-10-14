using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using WebAPI.Data;


namespace InfyPOS.Processors
{
    public class MfgDesingImport
    {
        public class ColumnConfiguration
        {
            public string quantoreference { get; set; }
            public string configcolumn { get; set; }
            public string excelcolunm { get; set; }
            public int excelindex { get; set; }
            public long custom_id { get; set; }
            public string custom_name { get; set; }
            public string custom_key { get; set; }
            public bool direct { get; set; }
            public List<ReferenceItem> reflist { get; set; }
            public List<Brand> brandlist { get; internal set; }
            public List<Products> productlist { get; internal set; }
            public Referencelist.CustomField customfield { get; internal set; }
            public bool customref { get; internal set; }
            public bool customtext { get; internal set; }
            public List<BOMMaster> fabriclist { get; internal set; }
            public bool fixedref { get; internal set; }
        }

        public SKUUploaded ProcessImages(Common.UserClaim context, byte[] databytes)
        {

            var excel = ExtractXlsx(databytes, false, false, null);
            var excelcolumns = excel.rows[0].values;
            var missingmap = new List<KeyValuePair<string, string>>();
            excel.rows.RemoveAt(0);

            var colourindex = excelcolumns.FindIndex(x => x.ToUpper() == "COLOUR" || x.ToUpper() == "COLOR");
            var designindex = excelcolumns.FindIndex(x => x.ToUpper() == "DESIGN" || x.ToUpper() == "SKU");
            var urlindex = excelcolumns.FindIndex(x => x.ToUpper() == "URL" || x.ToUpper() == "IMAGE");

            if (colourindex == -1)
            {
                return new SKUUploaded()
                {
                    sku = new List<MfgdesignProcess>(),
                    error = "Colour Column Missing"
                };
            }
            if (designindex == -1)
            {
                return new SKUUploaded()
                {
                    sku = new List<MfgdesignProcess>(),
                    error = "Design Column Missing"
                };
            }
            if (urlindex == -1)
            {
                return new SKUUploaded()
                {
                    sku = new List<MfgdesignProcess>(),
                    error = "URL Column Missing"
                };
            }

            var colours = excel.rows.ConvertAll(x => x.values[colourindex]).ToList().Distinct().ToList();
            var designs = excel.rows.ConvertAll(x => x.values[designindex]).ToList().Distinct().ToList();

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService();
            var designlist = mfgdesignDataService.SearchList(new MfgdesignCreteria() { codes = designs });
            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var colourRef = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "COLOUR",
                names = colours
            });
            List<MfgdesignProcess> lst = new List<MfgdesignProcess>();
            var missingcolours = colours.FindAll(x => !colourRef.Exists(y => y.name.ToUpper() == x.ToUpper()));
            if (missingcolours.Count > 0)
            {
                return new SKUUploaded()
                {
                    sku = new List<MfgdesignProcess>(),
                    error = "Missing Colours "+string.Join(",", missingcolours)
                };
            }

            var missingsku = designs.FindAll(x => !designlist.Exists(y => x.ToUpper() == y.code.ToUpper()));
            if (missingsku.Count > 0)
            {
                return new SKUUploaded()
                {
                    error = "Missing SKU " + string.Join(",", missingsku),
                    sku = new List<MfgdesignProcess>()
                };
            }

            foreach (var row in excel.rows)
            {
                lst.Add(new MfgdesignProcess()
                {
                    code = row.values[designindex],
                    colourid = colourRef.Find(x => x.name.ToUpper() == row.values[colourindex].ToUpper()).id,
                    imagelist = new List<string>() { row.values[urlindex] },
                    id = designlist.Find(x=>x.code.ToUpper() == row.values[designindex].ToUpper()).id
                });
            }
            return new SKUUploaded()
            {
                sku = lst
            };
        }

        public SKUUploaded Process(Common.UserClaim context, byte[] databytes, string columnmappings)
        {
            List<MfgdesignProcess> mfgdesigns = new List<MfgdesignProcess>();
            var referencecolumns = new List<string>("SECTION,SEASON,CLASSIFICATION,PRODUCTDIVISION,STYLE,FABRIC,BRAND,PRODUCT,COLOURCODE,COLOURNAME,SIZE,PRODUCTGROUP,PATTERN,TYPE,MATERIAL,FIT,SLEEVE".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            //string columnmappings = @"EAN=UAN,Department=PRODUCTDIVISION,Report Division=RPTDIVISION,Report Class=CLASSIFICATION,Report Sub Class=RPTSUBCLASS,Class=PRODUCT,Sub Class=STYLE,Brand=BRAND,Alloc Category=TYPE,Production code=PRODUCTIOINCODE,ProductSKU=PRODUCTSKU,Product Name=PRINTINGNAME,Colour code=COLOURCODE,Colour=COLOURNAME,Size=SIZECODE,ProductGroup=DESIGNID,RevisedProductGroup=DESIGNGROUP,Business Segment Name=PRODUCTGROUP,Season=SEASON,Season Status=SEASONSTATUS,Garment Type=PATTERN,Age=SIZEMEASURE,Toon Lable=MATERIAL,Fabric content=FABRIC,Gsm=GSM,MRP/Selling Price=MRP,Purchase Price=COST,HSN No=HSN,Fit=FIT,Rise=PRODUCTRISE,Actual Season=ACTUALSEASON,Season Group=SEASONGROUP,Pocket=PRODUCTPOCKET,Neck=PRODUCTNECK,Sleeve=SLEEVE";
            var cols = columnmappings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            List<KeyValuePair<string, string>> columns = new List<KeyValuePair<string, string>>();
            cols.ForEach(x =>
            {
                var parts = x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                columns.Add(new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()));
            });

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { isMigration = true, Context = context };
            var customfields = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "CUSTOMFIELDS"
            });
            var reffields = customfields.FindAll(x => x.referenceid == 4);
            var txtfields = customfields.FindAll(x => x.referenceid == 1);
            //var refseason = customfields.FindAll(x => x.referenceid == 12);
            //var refclass = customfields.FindAll(x => x.referenceid == 13);
            //var refdivision = customfields.FindAll(x => x.referenceid == 14);

            var excel = ExtractXlsx(databytes, false, false, null);
            var excelcolumns = excel.rows[0].values;
            var missingmap = new List<KeyValuePair<string,string>>();
            excel.rows.RemoveAt(0);
            List<ColumnConfiguration> columnConfiguration = new List<ColumnConfiguration>();
            foreach (var clmn in columns)
            {
                var eid = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper());
                if (eid < 0)
                {
                    eid = 0;
                    missingmap.Add(clmn);
                }
                if (referencecolumns.IndexOf(clmn.Value.ToUpper()) >= 0)
                {
                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        quantoreference = clmn.Value,
                        configcolumn = clmn.Value,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper())
                    });
                }
                else if (customfields.Exists(x => x.referenceid >=5 && x.referenceid <=11 && x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var custom_field = customfields.Find(x => x.referenceid >= 5 && x.referenceid <= 11 && x.code.ToUpper() == clmn.Value.ToUpper());
                    var custom_id = custom_field.id;
                    Referencelist.CustomField customfield = null;
                    if (custom_id > 0)
                        customfield = custom_field.additionalinfo.customfield;

                    string quantoreference = "COLOUR";
                    switch (custom_field.referenceid)
                    {
                        case 6:
                            quantoreference = "SIZE";
                            break;
                        case 7:
                            quantoreference = "FIT";
                            break;
                        case 8:
                            quantoreference = "TYPE";
                            break;
                        case 9:
                            quantoreference = "MATERIAL";
                            break;
                        case 10:
                            quantoreference = "STYLE";
                            break;
                        case 11:
                            quantoreference = "PATTERN";
                            break;
                    }

                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        quantoreference = quantoreference,
                        configcolumn = clmn.Value,
                        custom_id = custom_id,
                        customfield = customfield,
                        fixedref = true,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper())
                    });
                }
                else if (customfields.Exists(x => x.referenceid == 14 && x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var custom_id = customfields.Find(x => x.referenceid == 14 && x.code.ToUpper() == clmn.Value.ToUpper()).id;
                    Referencelist.CustomField customfield = null;
                    if (custom_id > 0)
                        customfield = customfields.Find(x => x.referenceid == 14 && x.code.ToUpper() == clmn.Value.ToUpper()).additionalinfo.customfield;

                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        quantoreference = "PRODUCTDIVISION",
                        configcolumn = clmn.Value,
                        custom_id = custom_id,
                        customfield = customfield,
                        fixedref = true,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper())
                    });
                }
                else if (customfields.Exists(x => x.referenceid == 13 && x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var custom_id = customfields.Find(x => x.referenceid == 13 && x.code.ToUpper() == clmn.Value.ToUpper()).id;
                    Referencelist.CustomField customfield = null;
                    if (custom_id > 0)
                        customfield = customfields.Find(x => x.referenceid == 13 && x.code.ToUpper() == clmn.Value.ToUpper()).additionalinfo.customfield;

                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        quantoreference = "CLASSIFICATION",
                        configcolumn = clmn.Value,
                        custom_id = custom_id,
                        customfield = customfield,
                        fixedref = true,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper())
                    });
                }
                else if (customfields.Exists(x => x.referenceid == 12 && x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var custom_id = customfields.Find(x => x.referenceid == 12 && x.code.ToUpper() == clmn.Value.ToUpper()).id;
                    Referencelist.CustomField customfield = null;
                    if (custom_id > 0)
                        customfield = customfields.Find(x => x.referenceid == 12 && x.code.ToUpper() == clmn.Value.ToUpper()).additionalinfo.customfield;

                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        quantoreference = "SEASON",
                        configcolumn = clmn.Value,
                        excelcolunm = clmn.Key,
                        custom_id = custom_id,
                        fixedref = true,
                        customfield = customfield,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper())
                    });
                }
                else if (reffields.Exists(x => x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var cusclmn = reffields.Find(x => x.code.ToUpper() == clmn.Value.ToUpper());
                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        configcolumn = clmn.Value,
                        custom_id = cusclmn.id,
                        customfield = cusclmn.additionalinfo.customfield,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper()),
                        custom_key = cusclmn.code,
                        custom_name = clmn.Value,
                        customref = true
                    });
                }
                else if (txtfields.Exists(x => x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var cusclmn = txtfields.Find(x => x.code.ToUpper() == clmn.Value.ToUpper());
                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        configcolumn = clmn.Value,
                        custom_id = cusclmn.id,
                        customfield = cusclmn.additionalinfo.customfield,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper()),
                        custom_key = cusclmn.code,
                        custom_name = clmn.Value,
                        customtext = true
                    });
                }
                else if (customfields.Exists(x => x.code.ToUpper() == clmn.Value.ToUpper()))
                {
                    var cusclmn = customfields.Find(x => x.code.ToUpper() == clmn.Value.ToUpper());
                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        configcolumn = clmn.Value,
                        custom_id = cusclmn.id,
                        customfield = cusclmn.additionalinfo.customfield,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper()),
                        custom_key = cusclmn.code,
                        custom_name = clmn.Value,
                        customtext = false
                    });
                }
                else
                {
                    columnConfiguration.Add(new ColumnConfiguration()
                    {
                        configcolumn = clmn.Value,
                        excelcolunm = clmn.Key,
                        excelindex = excelcolumns.FindIndex(x => x.ToUpper() == clmn.Key.ToUpper()),
                        direct = true
                    });
                }
            }
            var directfields = columnConfiguration.FindAll(x => x.direct == true);

            var qrefrows = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                types = new List<string>("SEASON,CLASSIFICATION,PRODUCTDIVISION,STYLE,COLOUR,SIZE,PRODUCTGROUP,PATTERN,TYPE,MATERIAL,FIT,SLEEVE".Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            });

            var productcolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.quantoreference) &&
               (x.quantoreference == "PRODUCT"));
            if (productcolumns.Count > 0)
            {
                ProductsDataService brandDataService = new ProductsDataService() { Context = context };
                var brandlist = brandDataService.Search(new ProductsCreteria()
                {

                });
                var allreflist = excel.rows.ConvertAll(x => x.values[productcolumns[0].excelindex]).ToList().Distinct().ToList();
                var missingbrands = allreflist.FindAll(x =>
                {
                    return !brandlist.Exists(y => y.name.ToUpper() == x.ToUpper());
                }).ConvertAll(x => new Products()
                {
                    name = x,
                    isactive = true,
                    purchasetaxid = 1,
                    salescounterid = 1,
                    sellingmode = 2
                });
                brandDataService.isMigration = true;
                missingbrands.ForEach(x =>
                {
                    brandDataService.Insert(x);
                });
                productcolumns[0].productlist = brandlist;
                productcolumns[0].productlist.AddRange(missingbrands);
            }

            var fabriccolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.quantoreference) &&
               (x.quantoreference == "FABRIC"));
            if (fabriccolumns.Count > 0)
            {
                BOMMasterDataService brandDataService = new BOMMasterDataService() { Context = context };
                var brandlist = brandDataService.Search(new BOMMasterCreteria()
                {

                });
                var allreflist = excel.rows.ConvertAll(x => x.values[fabriccolumns[0].excelindex]).ToList().Distinct().ToList();
                var missingbrands = allreflist.FindAll(x =>
                {
                    return !brandlist.Exists(y => y.name.ToUpper() == x.ToUpper());
                }).ConvertAll(x => new BOMMaster()
                {
                    name = x,
                    isactive = true,
                    coretypeid = 1
                });
                missingbrands.ForEach(x =>
                {
                    brandDataService.Insert(x);
                });
                fabriccolumns[0].fabriclist = brandlist;
                fabriccolumns[0].fabriclist.AddRange(missingbrands);
            }


            var brandcolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.quantoreference) &&
                (x.quantoreference == "BRAND"));
            if (brandcolumns.Count > 0)
            {
                BrandDataService brandDataService = new BrandDataService() { Context = context };
                var brandlist = brandDataService.Search(new BrandCreteria()
                {

                });
                var allreflist = excel.rows.ConvertAll(x => x.values[brandcolumns[0].excelindex]).ToList().Distinct().ToList();
                var missingbrands = allreflist.FindAll(x =>
                {
                    return !brandlist.Exists(y => y.name.ToUpper() == x.ToUpper());
                }).ConvertAll(x => new Brand()
                {
                    name = x,
                    isactive = true
                });
                missingbrands.ForEach(x =>
                {
                    brandDataService.Insert(x);
                });
                brandcolumns[0].brandlist = brandlist;
                brandcolumns[0].brandlist.AddRange(missingbrands);
            }

            var colourcodecolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.configcolumn) &&
                (x.configcolumn == "COLOURCODE"));

            var colournamecolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.configcolumn) &&
                (x.configcolumn == "COLOURNAME"));

            var sizecodecolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.configcolumn) &&
                (x.configcolumn == "SIZECODE"));

            var sizemeasurecolumns = columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.configcolumn) &&
                (x.configcolumn == "SIZEMEASURE"));

            if (sizecodecolumns.Count > 0 && sizemeasurecolumns.Count > 0)
            {
                var allrefcodelist = excel.rows.ConvertAll(x => new KeyValuePair<string, string>(x.values[sizecodecolumns[0].excelindex], x.values[sizemeasurecolumns[0].excelindex]));
                allrefcodelist = allrefcodelist.DistinctBy(x => x.Key).ToList();
                var existing = qrefrows.FindAll(x => x.type == "SIZE");
                var newreflist = allrefcodelist.FindAll(x =>
                {
                    return !existing.Exists(y => y.name.ToUpper() == x.Key.ToUpper());
                }).ConvertAll(x => new Referencelist()
                {
                    type = "SIZE",
                    name = x.Key,
                    code = x.Key,
                    additionalinfo = new Referencelist.Attribute() { formula = x.Value }
                });
                sizecodecolumns[0].reflist = referencelistDataService.InsertAll(newreflist).ConvertAll(x => new ReferenceItem()
                {
                    id = x.id,
                    name = x.name,
                    code = x.code
                });
                sizecodecolumns[0].reflist.AddRange(existing);
                sizemeasurecolumns[0].reflist = sizecodecolumns[0].reflist;
            }

            if (colourcodecolumns.Count > 0 && colournamecolumns.Count > 0)
            {
                var allrefcodelist = excel.rows.ConvertAll(x => new KeyValuePair<string, string>(x.values[colournamecolumns[0].excelindex], x.values[colourcodecolumns[0].excelindex]));
                allrefcodelist = allrefcodelist.DistinctBy(x => x.Key).ToList();
                var existing = qrefrows.FindAll(x => x.type == "COLOUR");
                var newreflist = allrefcodelist.FindAll(x =>
                {
                    return !existing.Exists(y => y.name.ToUpper() == x.Key.ToUpper());
                }).ConvertAll(x => new Referencelist()
                {
                    type = "COLOUR",
                    name = x.Key,
                    code = x.Value
                });
                colourcodecolumns[0].reflist = referencelistDataService.InsertAll(newreflist).ConvertAll(x => new ReferenceItem()
                {
                    id = x.id,
                    name = x.name,
                    code = x.code
                });
                colourcodecolumns[0].reflist.AddRange(existing);
                colournamecolumns[0].reflist = colourcodecolumns[0].reflist;
            }


            //SIZEMEASURE
            foreach (var refclmn in columnConfiguration.FindAll(x => !string.IsNullOrEmpty(x.quantoreference)).GroupBy(x => x.quantoreference))
            {
                if (refclmn.Key == "COLOURCODE" ||
                    refclmn.Key == "COLOURNAME" ||
                    refclmn.Key == "BRAND" ||
                    refclmn.Key == "FABRIC" ||
                    refclmn.Key == "PRODUCT" ||
                    refclmn.Key == "SIZEMEASURE")
                    continue;


                var allreflist = new List<Referencelist>();
                foreach (var itm in refclmn)
                {
                    allreflist.AddRange(excel.rows.ConvertAll(x => x.values[itm.excelindex]).ToList().Distinct().ToList().ConvertAll(x => new Referencelist()
                    {
                        type = refclmn.Key,
                        name = x
                    }));
                }
                var existing = qrefrows.FindAll(x => x.type == refclmn.Key);

                var newreflist = allreflist.FindAll(x =>
                {
                    return !existing.Exists(y => y.name.ToUpper() == x.name.ToUpper());
                });
                var reflist = newreflist.GroupBy(x => new { x.name, x.code }).ToList().ConvertAll(x => new Referencelist()
                {
                    name = x.Key.name,
                    code = x.Key.code,
                    type = refclmn.Key
                });


                qrefrows.AddRange(referencelistDataService.InsertAll(newreflist).ConvertAll(x => new ReferenceItem()
                {
                    id = x.id,
                    name = x.name,
                    code = x.code,
                    type = x.type
                }));
                foreach (var itm in refclmn)
                {
                    itm.reflist = qrefrows;
                }

                //var refseason = customfields.FindAll(x => x.referenceid == 12);
                //var refclass = customfields.FindAll(x => x.referenceid == 13);
                //var refdivision = customfields.FindAll(x => x.referenceid == 14);

            }

            var QCustomrows = referencelistDataService.Search(new ReferencelistCreteria()
            {
                types = columnConfiguration.FindAll(x => x.custom_id > 0).ConvertAll(x => x.custom_name)
            });
            foreach (var refclmn in columnConfiguration.FindAll(x => x.customref && x.custom_id > 0))
            {
                var allreflist = excel.rows.ConvertAll(x => x.values[refclmn.excelindex]).ToList().Distinct().ToList().ConvertAll(x => new Referencelist()
                {
                    type = refclmn.custom_name,
                    name = x
                });
                var customfield = customfields.Find(x => x.id == refclmn.custom_id);

                var existing = QCustomrows.FindAll(x => x.type == refclmn.custom_name);
                var newreflist = allreflist.FindAll(x =>
                {
                    return !existing.Exists(y => y.name.ToUpper() == x.name.ToUpper());
                });
                refclmn.reflist = referencelistDataService.InsertAll(newreflist).ConvertAll(x => new ReferenceItem()
                {
                    id = x.id,
                    name = x.name,
                    code = x.code,
                    type = x.type
                });
                if(refclmn.reflist.Exists(x=>x.id == 0))
                {
                    throw new Exception("Cant insert the referencelist of "+ refclmn.reflist.Find(x => x.id == 0).name);
                }
                refclmn.reflist.AddRange(existing);
            }

            var designcolumn = columnConfiguration.Find(x => x.direct && x.configcolumn.ToUpper() == "DESIGNID");
            var uancolumn = columnConfiguration.Find(x => x.configcolumn == "UAN");

            var productioncodecolumn = columnConfiguration.Find(x => x.configcolumn == "PRODUCTIOINCODE");
            var productskucolumn = columnConfiguration.Find(x => x.configcolumn == "PRODUCTSKU");

            var hsncolumn = columnConfiguration.Find(x => x.configcolumn == "HSN");
            var mrpcolumn = columnConfiguration.Find(x => x.configcolumn == "MRP");
            var costcolumn = columnConfiguration.Find(x => x.configcolumn == "COST");

            var wsratecolumn = columnConfiguration.Find(x => x.configcolumn == "WSRATE");
            var dealerpricecolumn = columnConfiguration.Find(x => x.configcolumn == "DEALERRATE");
            var saleratecolumn = columnConfiguration.Find(x => x.configcolumn == "SALERATE");

            var sizecolumn = columnConfiguration.FindAll(x => x.configcolumn == "SIZECODE");
            var itemcolumn = columnConfiguration.FindAll(x => (x.configcolumn == "PRINTINGNAME"));


            var stylecolumn = columnConfiguration.FindAll(x => x.configcolumn == "STYLE");
            var patterncolumn = columnConfiguration.FindAll(x => x.configcolumn == "PATTERN");
            var materialcolumn = columnConfiguration.FindAll(x => x.configcolumn == "MATERIAL");
            var sleevecolumn = columnConfiguration.FindAll(x => x.configcolumn == "SLEEVE");
            var fitcolumn = columnConfiguration.FindAll(x => x.configcolumn == "FIT");
            var seasoncolumn = columnConfiguration.FindAll(x => x.configcolumn == "SEASON");
            var typecolumn = columnConfiguration.FindAll(x => x.configcolumn == "TYPE");
            var classcolumn = columnConfiguration.FindAll(x => x.configcolumn == "CLASSIFICATION");
            var divisioncolumn = columnConfiguration.FindAll(x => x.configcolumn == "PRODUCTDIVISION");


            foreach (var dnitem in excel.rows.GroupBy(x => new
            {
                product = x.values[productcolumns[0].excelindex],
                brand = x.values[brandcolumns[0].excelindex],
                design = x.values[designcolumn.excelindex]
            }))
            {
                var brand = brandcolumns[0].brandlist.Find(x => x.name.ToUpper() == dnitem.Key.brand.ToUpper());
                var product = productcolumns[0].productlist.Find(x => x.name.ToUpper() == dnitem.Key.product.ToUpper());

                MfgdesignProcess mfgdesign = new MfgdesignProcess();
                mfgdesign.name = dnitem.Key.design;
                mfgdesign.brandid = brand.id;
                mfgdesign.code = dnitem.Key.design;
                mfgdesign.displayname = dnitem.Key.design + " - " + product.name;
                mfgdesign.productid = product.id;
                mfgdesign.prdhsncode = dnitem.First().values[hsncolumn.excelindex];
                mfgdesign.version = 1;
                mfgdesign.isactive = true;
                mfgdesign.modifiedon = DateTime.Now;
                mfgdesign.createdon = DateTime.Now;
                mfgdesign.sizegroup = new Mfgdesign.Sizegroup() { items = new List<Mfgdesign.Sizegroup.Item>() };

                var firstrow = dnitem.First();
                mfgdesign.CustomFields = new List<Mfgdesign.CustomFieldValue>();
                foreach (var field in InfyPOS.Services.CustomFieldsManager.Instance(0).Config)
                {
                    if (columnConfiguration.Exists(x => x.custom_id == field.id))
                    {
                        var cconfig = columnConfiguration.Find(x => x.custom_id == field.id);
                        if (cconfig.customref)
                        {
                            var refitems = cconfig.reflist.FindAll(x => x.name.ToUpper() == firstrow.values[cconfig.excelindex].ToUpper());
                            if (refitems.Count > 0)
                            {
                                mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                                {
                                    key = field.cfname,
                                    value = refitems[0].id.ToString()
                                });
                            }
                            else
                            {
                                mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                                {
                                    key = field.cfname,
                                    value = null,
                                    n = "customref and referencelist missing"
                                });
                            }
                        }
                        else if (cconfig.fixedref)
                        {
                            var refitems = cconfig.reflist.FindAll(x => x.type == cconfig.quantoreference && x.name.ToUpper() == firstrow.values[cconfig.excelindex].ToUpper());
                            if (refitems.Count > 0)
                            {
                                mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                                {
                                    key = field.cfname,
                                    value = refitems[0].id.ToString()
                                });
                            }
                            else
                            {
                                mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                                {
                                    key = field.cfname,
                                    value = null,
                                    n = "fixedref and referencelist missing"
                                });
                            }
                        }
                        else
                        {
                            mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                            {
                                key = field.cfname,
                                value = firstrow.values[cconfig.excelindex]
                            });
                        }
                    }
                    else
                    {
                        if (directfields.Exists(x => "cf_" + x.configcolumn.ToLower() == field.cfname))
                        {
                            var tcconig = directfields.Find(x => "cf_" + x.configcolumn.ToLower() == field.cfname);
                            mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                            {
                                key = field.cfname,
                                value = firstrow.values[tcconig.excelindex]
                            });
                        }
                        else
                        {

                            mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                            {
                                key = field.cfname,
                                value = null,
                                n = "directfields can't find the field"
                            });
                        }
                    }
                }
                if (typecolumn.Count > 0)
                {
                    var tname = firstrow.values[typecolumn[0].excelindex];
                    mfgdesign.typeid = typecolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "TYPE").id;
                }

                if (stylecolumn.Count > 0)
                {
                    var tname = firstrow.values[stylecolumn[0].excelindex];
                    mfgdesign.styleid = stylecolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "STYLE").id;
                }
                if (patterncolumn.Count > 0)
                {
                    var tname = firstrow.values[patterncolumn[0].excelindex];
                    mfgdesign.patternid = patterncolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "PATTERN").id;
                }
                if (materialcolumn.Count > 0)
                {
                    var tname = firstrow.values[materialcolumn[0].excelindex];
                    mfgdesign.materialid = materialcolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "MATERIAL").id;
                }
                if (sleevecolumn.Count > 0)
                {
                    var tname = firstrow.values[sleevecolumn[0].excelindex];
                    mfgdesign.sleeveid = sleevecolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "SLEEVE").id;
                }
                if (fitcolumn.Count > 0)
                {
                    var tname = firstrow.values[fitcolumn[0].excelindex];
                    mfgdesign.fitid = fitcolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "FIT").id;
                }
                if (seasoncolumn.Count > 0)
                {
                    var tname = firstrow.values[seasoncolumn[0].excelindex];
                    mfgdesign.seasonid = seasoncolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "SEASON").id;
                }
                if (fabriccolumns.Count > 0)
                {
                    var tname = firstrow.values[fabriccolumns[0].excelindex];
                    mfgdesign.fabricid = fabriccolumns[0].fabriclist.Find(x => x.name.ToUpper() == tname.ToUpper()).id;
                }
                if (classcolumn.Count > 0)
                {
                    var tname = firstrow.values[classcolumn[0].excelindex];
                    mfgdesign.classid = classcolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "CLASSIFICATION").id;
                }
                if (divisioncolumn.Count > 0)
                {
                    var tname = firstrow.values[divisioncolumn[0].excelindex];
                    mfgdesign.prddivisionid = divisioncolumn[0].reflist.Find(x => x.name.ToUpper() == tname.ToUpper() && x.type == "PRODUCTDIVISION").id;
                }
                /*var stylecolumn = columnConfiguration.FindAll(x => x.configcolumn == "STYLE");
                var patterncolumn = columnConfiguration.FindAll(x => x.configcolumn == "PATTERN");
                var materialcolumn = columnConfiguration.FindAll(x => x.configcolumn == "MATERIAL");
                var sleevecolumn = columnConfiguration.FindAll(x => x.configcolumn == "SLEEVE");
                var fitcolumn = columnConfiguration.FindAll(x => x.configcolumn == "FIT");
                var seasoncolumn = columnConfiguration.FindAll(x => x.configcolumn == "SEASON");*/

                foreach (var srow in dnitem)
                {
                    var colourcode = colourcodecolumns.Count == 0 ? "" : srow.values[colourcodecolumns[0].excelindex];
                    var colourname = colournamecolumns.Count == 0 ? "" : srow.values[colournamecolumns[0].excelindex];
                    var colour = (colourcodecolumns.Count == 0 || colourcodecolumns[0].reflist == null) ? null : colourcodecolumns[0].reflist.Find(x => x.name.ToUpper() == colourname.ToUpper());
                    var sizename = sizecolumn.Count == 0 ? null : srow.values[sizecolumn[0].excelindex];
                    var size = (sizecolumn.Count == 0 || sizecolumn[0].reflist == null) ? null : sizecolumn[0].reflist.Find(x => x.name.ToUpper() == sizename.ToUpper());
                    var printingname = srow.values[itemcolumn[0].excelindex];

                    var productioncode = productioncodecolumn != null ? srow.values[productioncodecolumn.excelindex] : "";
                    var productsku = productskucolumn != null ? srow.values[productskucolumn.excelindex] : "";

                    decimal mrp = 0;
                    if (mrpcolumn != null)
                        decimal.TryParse(srow.values[mrpcolumn.excelindex], out mrp);

                    decimal cost = 0;
                    if (mrpcolumn != null)
                        decimal.TryParse(srow.values[costcolumn.excelindex], out cost);

                    decimal wsrate = 0;
                    if (wsratecolumn != null)
                        decimal.TryParse(srow.values[wsratecolumn.excelindex], out wsrate);

                    decimal dealerprice = 0;
                    if (dealerpricecolumn != null)
                        decimal.TryParse(srow.values[dealerpricecolumn.excelindex], out dealerprice);

                    decimal salerate = 0;
                    if (saleratecolumn != null)
                        decimal.TryParse(srow.values[saleratecolumn.excelindex], out salerate);

                    mfgdesign.sizegroup.items.Add(new Mfgdesign.Sizegroup.Item()
                    {
                        colourid = colour == null ? 0 : colour.id,
                        uid = srow.values[uancolumn.excelindex],
                        designitemcode = mfgdesign.code + "/" + colourcode,
                        code = productioncode,
                        cost = cost,
                        salerate = salerate,
                        prdsku = productsku,
                        mrp = mrp,
                        alloworder = true,
                        dealerprice = dealerprice,
                        wsrate = wsrate,
                        sizeid = size == null ? 0 : size.id,
                        name = printingname
                    });
                }
                mfgdesigns.Add(mfgdesign);
            }

            SKUUploaded sKUUploaded = new SKUUploaded();
            sKUUploaded.noofssku = mfgdesigns.Count;
            sKUUploaded.columnConfiguration = columnConfiguration;
            sKUUploaded.excelcolumns = excelcolumns;
            sKUUploaded.missingmap = missingmap;
            sKUUploaded.sku = mfgdesigns;
            sKUUploaded.noofuan = mfgdesigns.Sum(x => x.sizegroup.items.Count);
            string query = "select id,code from mfgdesign where isactive=true and code in (" + String.Join(",", mfgdesigns.ConvertAll(x => "'" + x.code + "'")) + ")";
            SortedDictionary<string, long> mfgdesignlist = new SortedDictionary<string, long>();
            SortedDictionary<string, List<string>> mfgdesignuanlist = new SortedDictionary<string, List<string>>();
            BaseData baseData = new BaseData() { Context = context };
            using (var reader = baseData.ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    mfgdesignlist.Add(reader.GetString(1), reader.GetInt64(0));
                }
            }

            sKUUploaded.duplicatedesigns = "";
            var duplicatedesigns = mfgdesigns.GroupBy(x => x.code).Where(x => x.Count() > 1);
            if (duplicatedesigns.Count() > 0)
            {
                var ddesigns = duplicatedesigns.ToList().ConvertAll(x => x.Key);
                sKUUploaded.duplicatedesigns = string.Join(",", ddesigns);
                mfgdesigns.RemoveAll(x => ddesigns.Contains(x.code));
            }

            mfgdesigns.ForEach(x =>
            {
                x.isexist = mfgdesignlist.ContainsKey(x.code);
                if (x.isexist)
                    x.id = mfgdesignlist[x.code];
            });
            //var existing = mfgdesigns.FindAll(x => x.isexist);

            var missingids = mfgdesigns.FindAll(x => x.isexist).ConvertAll(x => x.id);
            if (missingids.Count > 0)
            {
                query = @"select mfgdesign.code,schemelist->> 'uid' uid from mfgdesign
            LEFT JOIN LATERAL
            json_array_elements(cast(mfgdesign.sizegroup->> 'items' as json)) schemelist ON true
            where isactive=true and id in (" + String.Join(",", missingids) + ")";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        var code = reader.GetString(0);
                        var uid = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        if (!mfgdesignuanlist.ContainsKey(code))
                            mfgdesignuanlist.Add(code, new List<string>());
                        mfgdesignuanlist[code].Add(uid);
                    }
                }
            }

            foreach (var mfgitem in mfgdesignuanlist)
            {
                var mfguploaded = mfgdesigns.Find(x => x.code == mfgitem.Key);
                if (mfguploaded != null)
                {
                    sKUUploaded.rmvuan += mfguploaded.sizegroup.items.Count(x => mfgitem.Value.Contains(x.uid));
                    mfguploaded.sizegroup.items.RemoveAll(x => mfgitem.Value.Contains(x.uid));
                }
                if (mfguploaded.sizegroup.items.Count == 0)
                {
                    sKUUploaded.similarsku++;
                    mfgdesigns.Remove(mfguploaded);
                }
            }
            return sKUUploaded;
        }

        public InfyPOS.Controllers.ImageController.ExcelDocument ExtractXlsx(Byte[] buffer, bool hasColumn, bool onlyheader, string sheetname)
        {
            InfyPOS.Controllers.ImageController.ExcelDocument exceldoc = new InfyPOS.Controllers.ImageController.ExcelDocument();
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
                            InfyPOS.Controllers.ImageController.ExcelDocument.Row row = new InfyPOS.Controllers.ImageController.ExcelDocument.Row();
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
        private DataTable CreateTable(ClosedXML.Excel.IXLWorksheet workSheet, bool hascolumn, bool onlyheader)
        {
            bool firstRow = true;
            DataTable dt = new DataTable();
            bool isrow = true;
            foreach (var row in workSheet.Rows())
            {
                //Use the first row to add columns to DataTable.
                if (firstRow)
                {
                    foreach (var cell in row.Cells())
                    {
                        dt.Columns.Add(cell.Value.ToString());
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
    }
}
