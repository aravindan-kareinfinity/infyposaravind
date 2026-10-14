using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfyPOS.Processors.Migration
{
    public interface IMigrationEntity
    {
        int sellingmode { get; set; }
        string brand { get; set; }
        long brandid { get; set; }
        string colour { get; set; }
        long colourid { get; set; }
        long itemid { get; set; }
        decimal ptaxpercentage { get; set; }
        string itemname { get; set; }
        string material { get; set; }
        long materialid { get; set; }
        string pattern { get; set; }
        long patternid { get; set; }
        string product { get; set; }
        long productid { get; set; }
        string section { get; set; }
        long sectionid { get; set; }
        string size { get; set; }
        long sizeid { get; set; }
        string style { get; set; }
        string fit { get; set; }
        string sleeve { get; set; }
        string pricetag { get; set; }
        long fitid { get; set; }
        long sleeveid { get; set; }
        long pricetagid { get; set; }
        long styleid { get; set; }
        string supplier { get; set; }
        string sourcesupplier { get; set; }
        long supplierid { get; set; }
        long sourcesupplierid { get; set; }
        string type { get; set; }
        string ptaxcode { get; set; }
        string taxcode { get; set; }
        decimal saletaxpercentage { get; set; }
        long typeid { get; set; }
        long saletaxid { get; set; }
        long ptaxid { get; set; }
        string CompanyCode { get; set; }
        long companyid { get; set; }
        string LocationCode { get; set; }
        long locationid { get; set; }
        long suppliercompanyid { get; set; }
        decimal ptaxamount { get; set; }

        string gln { get; set; }
        decimal pdiscount { get; set; }
        long invoiceitemid { get; set; }
        string hsncode { get; set; }
        string suppliercode { get; set; }
    }

    public class Purchase : IMigrationEntity
    {
        public string fit { get; set; }
        public string sleeve { get; set; }
        public long fitid { get; set; }
        public long sleeveid { get; set; }
        public long pricetagid { get; set; }
        public string hsncode { get; set; }
        public string gln { get; set; }
        public Purchase(System.Data.DataRow dr)
        {
            LocationCode = dr.IsNull("LocationCode") ? String.Empty : dr["LocationCode"].ToString();
            CompanyCode = dr.IsNull("CompanyCode") ? String.Empty : dr["CompanyCode"].ToString();

            barcode = dr.IsNull("BARCODE") ? String.Empty : dr["BARCODE"].ToString();
            product = productname = dr.IsNull("PRODUCTNAME") ? String.Empty : dr["PRODUCTNAME"].ToString();
            itemname = dr.IsNull("ITEMNAME") ? String.Empty : dr["ITEMNAME"].ToString();
            brand = dr.IsNull("Brand") ? String.Empty : dr["Brand"].ToString();
            type = dr.IsNull("Type") ? String.Empty : dr["Type"].ToString();
            style = dr.IsNull("Style") ? String.Empty : dr["Style"].ToString();
            pattern = dr.IsNull("Pattern") ? String.Empty : dr["Pattern"].ToString();
            colour = dr.IsNull("color") ? String.Empty : dr["color"].ToString();
            material = dr.IsNull("Material") ? String.Empty : dr["Material"].ToString();
            pricetag = dr.IsNull("PriceTag") ? String.Empty : dr["PriceTag"].ToString();
            section = dr.IsNull("Section") ? String.Empty : dr["Section"].ToString();
            size = dr.IsNull("Size") ? String.Empty : dr["Size"].ToString();
            designno = dr.IsNull("DESIGNNO") ? String.Empty : dr["DESIGNNO"].ToString();
            hsncode = dr.IsNull("HSNCODE") ? String.Empty : dr["HSNCODE"].ToString();

            supplier = dr.IsNull("SUPPLIER") ? String.Empty : dr["SUPPLIER"].ToString();
            sourcesupplier = dr.IsNull("SOURCESUPPLIER") ? String.Empty : dr["SOURCESUPPLIER"].ToString();
            supcity = dr.IsNull("SUPCITY") ? String.Empty : dr["SUPCITY"].ToString();
            supgroup = dr.IsNull("SUPGROUP") ? String.Empty : dr["SUPGROUP"].ToString();
            agent = dr.IsNull("AGENT") ? String.Empty : dr["AGENT"].ToString();
            invno = dr.IsNull("INVNO") ? String.Empty : dr["INVNO"].ToString();
            invdate = dr.IsNull("INVDATE") ? DateTime.MinValue : (DateTime)dr["INVDATE"];


            purentrydate = dr.IsNull("PURENTRYDATE") ? DateTime.MinValue : (DateTime)dr["PURENTRYDATE"];
            cost = dr.IsNull("COST") ? 0 : decimal.Parse(dr["COST"].ToString());
            pdiscount = dr.IsNull("PURCHASEDISCOUNT") ? 0 : decimal.Parse(dr["PURCHASEDISCOUNT"].ToString());
            salerate = dr.IsNull("SALERATE") ? 0 : decimal.Parse(dr["SALERATE"].ToString());
            salediscount = dr.IsNull("SALEDISCOUNT") ? 0 : decimal.Parse(dr["SALEDISCOUNT"].ToString());
            purqty = dr.IsNull("PURQTY") ? 0 : decimal.Parse(dr["PURQTY"].ToString());


            //taxcode = dr.IsNull("taxcode") ? String.Empty : dr["taxcode"].ToString();
            ptaxcode = dr.IsNull("taxcode") ? String.Empty : dr["taxcode"].ToString();
            ptaxpercentage = saletaxpercentage = dr.IsNull("taxpercentage") ? 0 : decimal.Parse(dr["taxpercentage"].ToString());
            ptaxamount = dr.IsNull("PurchaseTax1Amt") ? 0 : decimal.Parse(dr["PurchaseTax1Amt"].ToString());

            if (salediscount > 0)
                salediscount = salerate - salediscount;

        }
        public int sellingmode { get; set; }
        public long saletaxid { get; set; }
        public decimal pdiscount { get; set; }
        public decimal ptaxamount { get; set; }
        public string ptaxcode { get; set; }
        public long ptaxid { get; set; }
        public string taxcode { get; set; }
        public decimal saletaxpercentage { get; set; }
        public decimal ptaxpercentage { get; set; }
        public decimal cost { get; set; }
        public string material { get; set; }
        public string pricetag { get; set; }
        public string section { get; set; }
        public string size { get; set; }
        public string supplier { get; set; }
        public string sourcesupplier { get; set; }
        public string supcity { get; set; }
        public string supgroup { get; set; }
        public string agent { get; set; }
        public string invno { get; set; }
        public DateTime invdate { get; set; }
        public string designno { get; set; }
        public long invoiceitemid { get; set; }
        public decimal salerate { get; set; }
        public DateTime purentrydate { get; set; }
        public long suppliercompanyid { get; set; }
        public decimal purdays { get; set; }
        public decimal openingqty { get; set; }
        public decimal purqty { get; set; }
        public decimal salretqty { get; set; }
        public decimal salinvretqty { get; set; }
        public decimal prretqty { get; set; }
        public decimal salqty { get; set; }
        public decimal salinvqty { get; set; }
        public decimal inwqty { get; set; }
        public decimal ouwqty { get; set; }
        public decimal closingqty { get; set; }

        public string barcode { get; set; }
        public string productname { get; set; }
        public string itemname { get; set; }
        public string brand { get; set; }
        public string type { get; set; }
        public string style { get; set; }
        public string pattern { get; set; }
        public string colour { get; set; }

        public long brandid { get; set; }

        public long itemid { get; set; }
        public string product { get; set; }
        public long productid { get; set; }
        public long sectionid { get; set; }
        public long supplierid { get; set; }
        public long sourcesupplierid { get; set; }
        public long colourid { get; set; }
        public long materialid { get; set; }

        public long patternid { get; set; }

        public long sizeid { get; set; }
        public long typeid { get; set; }
        public long styleid { get; set; }

        public long locationid { get; set; }
        public string CompanyCode { get; set; }
        public long companyid { get; set; }
        public string LocationCode { get; set; }
        public decimal salediscount { get; set; }
        public long lrinvoiceid { get; set; }

        public string suppliercode { get; set; }
    }

    public class ProductTax
    {
        public string Code;
        public decimal firsttaxTaxPercentage, firsttaxRangeFrom, firsttaxRangeTo, secondtaxTaxPercentage, secondtaxRangeFrom, secondtaxRangeTo;
        internal long taxid;

        public ProductTax(System.Data.DataRow dr)
        {
            Code = dr["code"].ToString();
            firsttaxTaxPercentage = dr["TaxPercentage"] != DBNull.Value ? decimal.Parse(dr["TaxPercentage"].ToString()) : 0;
            firsttaxRangeFrom = dr["RangeFrom"] != DBNull.Value ? (decimal)dr["RangeFrom"] : 0;
            firsttaxRangeTo = dr["RangeTo"] != DBNull.Value ? (decimal)dr["RangeTo"] : 0;
            secondtaxTaxPercentage = dr["TaxPercentage1"] != DBNull.Value ? (decimal.Parse(dr["TaxPercentage1"].ToString())) : -1;
            secondtaxRangeFrom = dr["RangeFrom1"] != DBNull.Value ? (decimal)dr["RangeFrom1"] : -1;
            secondtaxRangeTo = dr["RangeTo1"] != DBNull.Value ? (decimal)dr["RangeTo1"] : -1;
        }
    }
    public class SSProduct
    {
        public static void UpdateLocationAndTax(long organizationid, List<IMigrationEntity> sales, bool issales)
        {
            InfyPOS.Data.CompanyDataService companydata = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            companydata.Context = new InfyPOS.Common.UserClaim();
            var compList = companydata.Search(new InfyPOS.Models.CompanyCreteria());

            InfyPOS.Data.ReferencelistDataService refList = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            refList.Context = new InfyPOS.Common.UserClaim();
            var loclist = refList.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "LOCATION"
            });
            InfyPOS.Data.TaxDataService taxdata = new InfyPOS.Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            taxdata.Context = new InfyPOS.Common.UserClaim();
            var taxList = taxdata.Search(new InfyPOS.Models.TaxCreteria());
            foreach (var taxitem in sales.GroupBy(e => e.taxcode))
            {
                if (!taxList.Exists(e => e.code.ToLower() == ("GST" + taxitem.First().saletaxpercentage.ToString()).ToLower()))
                {
                    var entity = new InfyPOS.Models.Tax();
                    entity.code = "GST" + taxitem.First().saletaxpercentage;
                    entity.name = "GST " + taxitem.First().saletaxpercentage.ToString("F0") + "%";
                    entity.ispurchasetax = true;
                    entity.issalestax = true;
                    entity.type = 1;
                    entity.taxpercentage = taxitem.First().saletaxpercentage;
                    entity.isactive = true;
                    taxdata.Insert(entity);
                    taxList.Add(entity);
                }
                long id = taxList.Find(e => e.code.ToLower() == ("GST" + taxitem.First().saletaxpercentage.ToString()).ToLower()).id;
                taxitem.ToList().ForEach(e => { e.ptaxid = id; e.saletaxid = id; });
            }


            foreach (var taxitem in sales.GroupBy(e => e.CompanyCode))
            {
                if (!compList.Exists(e => e.code.ToLower() == taxitem.Key.ToLower() || e.name.ToLower() == taxitem.Key.ToLower()))
                {
                    var entity = new InfyPOS.Models.Company();
                    entity.code = taxitem.Key;
                    entity.address = taxitem.Key;
                    entity.name = "QUANTO TEXTILE";
                    entity.title = taxitem.Key;
                    entity.isactive = true;
                    companydata.Insert(entity);
                    compList.Add(entity);
                }
                long id = compList.Find(e => e.code.ToLower() == taxitem.Key.ToLower() || e.name.ToLower() == taxitem.Key.ToLower()).id;
                taxitem.ToList().ForEach(e => { e.companyid = id; });
            }

            foreach (var taxitem in sales.GroupBy(e => e.LocationCode))
            {
                string code = string.IsNullOrEmpty(taxitem.Key) ? "MAIN" : taxitem.Key;
                if (!loclist.Exists(e => e.code.ToLower() == code.ToLower() || e.name.ToLower() == code.ToLower()))
                {
                    var entity = new InfyPOS.Models.Referencelist();
                    entity.code = code;
                    entity.type = "LOCATION";
                    entity.name = code;
                    entity.isactive = true;
                    refList.Insert(entity);
                    loclist.Add(entity);
                }
                long id = loclist.Find(e => e.code.ToLower() == code.ToLower() || e.name.ToLower() == code.ToLower()).id;
                sales.Where(e => e.LocationCode == code).ToList().ForEach(e => e.locationid = id);
            }
        }

        public static void UpdateLocationAndTax(long organizationid, string connectionstring, List<IMigrationEntity> sales, bool issales)
        {
            InfyPOS.Data.CompanyDataService companydata = new InfyPOS.Data.CompanyDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            companydata.Context = new InfyPOS.Common.UserClaim();
            var compList = companydata.Search(new InfyPOS.Models.CompanyCreteria());

            InfyPOS.Data.ReferencelistDataService refList = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            refList.Context = new InfyPOS.Common.UserClaim();
            var loclist = refList.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "LOCATION"
            });

            InfyPOS.Data.TaxDataService taxdata = new InfyPOS.Data.TaxDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            taxdata.Context = new InfyPOS.Common.UserClaim();
            var taxList = taxdata.Search(new InfyPOS.Models.TaxCreteria());


            foreach (var item in sales.GroupBy(e => e.ptaxpercentage))
            {
                if (!taxList.Exists(e => e.taxpercentage == item.Key))
                {
                    var entity = new InfyPOS.Models.Tax();
                    entity.code = "GST" + item.Key.ToString("F0");
                    entity.name = "GST " + item.Key.ToString("F0") + "%";
                    entity.ispurchasetax = true;
                    entity.issalestax = true;
                    entity.type = 2;
                    entity.taxpercentage = item.Key;

                    entity.taxsplit = new InfyPOS.Models.Tax.TaxSplit();
                    entity.taxsplit.items.Add(new InfyPOS.Models.Tax.TaxSplit.Item()
                    {
                        cgst = (decimal)item.Key / 2,
                        igst = (decimal)item.Key / 2,
                        sgst = (decimal)item.Key / 2
                    });
                    entity.isactive = true;
                    taxdata.Insert(entity);
                    taxList.Add(entity);
                    item.ToList().ForEach(e =>
                    {
                        e.ptaxid = entity.id;
                        e.saletaxid = entity.id;
                    });
                }
                else
                {
                    var taxid = taxList.Find(e => e.taxpercentage == item.Key).id;
                    item.ToList().ForEach(e =>
                    {
                        e.ptaxid = taxid;
                        e.saletaxid = taxid;
                    });
                }

            }



            foreach (var item in sales.GroupBy(e => e.CompanyCode))
            {
                if (!compList.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    var entity = new InfyPOS.Models.Company();
                    entity.code = item.Key;
                    entity.name = item.Key;
                    entity.title = item.Key;
                    entity.isactive = true;
                    companydata.Insert(entity);
                    compList.Add(entity);
                    item.ToList().ForEach(e =>
                    {
                        e.ptaxid = entity.id;
                        e.saletaxid = entity.id;
                    });
                }
                else
                {
                    var taxid = compList.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                    item.ToList().ForEach(e => e.companyid = taxid);
                }
            }

            foreach (var item in sales.GroupBy(e => e.LocationCode))
            {
                if (!loclist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    var entity = new InfyPOS.Models.Referencelist();
                    entity.code = item.Key;
                    entity.type = "LOCATION";
                    entity.name = item.Key;
                    entity.isactive = true;
                    refList.Insert(entity);
                    loclist.Add(entity);
                    item.ToList().ForEach(e => e.locationid = entity.id);
                }
                else
                {
                    var taxid = compList.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                    item.ToList().ForEach(e => e.locationid = taxid);
                }
            }

        }


        public static Dictionary<string, List<InfyPOS.Data.StockDataService.BarcodeID>> GetBarcodeList(long organizationid)
        {
            try
            {
                InfyPOS.Data.StockDataService stockservice = new InfyPOS.Data.StockDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                stockservice.Context = new InfyPOS.Common.UserClaim();
                var list = stockservice.SearchALLBarcode();
                return list;
            }
            catch (Exception exp)
            {
                System.IO.File.AppendAllText(@"migration.log", exp.ToString());
                System.IO.File.AppendAllText(@"migration.log", exp.StackTrace.ToString());
            }
            return new Dictionary<string, List<InfyPOS.Data.StockDataService.BarcodeID>>();
        }

        public static List<Supplier> Suppliers;

        public static List<Models.Referencelist> City { get; set; }
        public static List<Models.Referencelist> State { get; set; }

        public class Supplier
        {
            public string code;
            public string name;
            public string address;
            public string mobile;
            public string city;
            public string state;
            public string gst;
            public Supplier(System.Data.DataRow dr)
            {
                code = dr["code"].ToString();
                name = dr["name"].ToString();
                address = dr["address"].ToString();
                mobile = dr["mobile"].ToString();
                city = dr["city"].ToString();
                state = dr["state"].ToString();
                gst = dr["gstn"].ToString();
            }
        }

        public static void ProcessAll(long organizationid, List<IMigrationEntity> sales)
        {
            InfyPOS.Common.UserClaim content = new InfyPOS.Common.UserClaim();
            InfyPOS.Data.ReferencelistDataService referencedataservice = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            referencedataservice.Context = content;
            referencedataservice.isMigration = true;

            var sectionlist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "DIVISION"
            });

            foreach (var item in sales.GroupBy(e => e.section))
            {
                long sectionid = 0;
                if (!sectionlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    mproduct.type = "DIVISION";
                    referencedataservice.Insert(mproduct);
                    sectionlist.Add(mproduct);
                    sectionid = mproduct.id;
                }
                else
                {
                    sectionid = sectionlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.sectionid = sectionid);
            }


            InfyPOS.Data.ProductsDataService productdataservice = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            productdataservice.Context = content;
            productdataservice.isMigration = true;
            var productlist = productdataservice.Search(new InfyPOS.Models.ProductsCreteria()
            {
            });
            foreach (var item in sales.GroupBy(e => e.product))
            {
                if (item.Key == null) continue;
                long productid = 0;
                if (!productlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Products mproduct = new InfyPOS.Models.Products();
                    mproduct.name = item.Key;
                    mproduct.isactive = true;
                    mproduct.hsncode = item.First().hsncode;
                    mproduct.salestaxid = item.First().saletaxid;
                    mproduct.purchasetaxid = item.First().ptaxid;
                    mproduct.sellingmode = item.First().sellingmode;
                    mproduct.barcodemode = 2;
                    if (sectionlist.Exists(e => e.name.ToLower() == item.First().section.ToLower()))
                    {
                        mproduct.salescounterid = sectionlist.Find(e => e.name.ToLower() == item.First().section.ToLower()).id;
                    }
                    else
                    {
                        long sectionid = 0;
                        if (!sectionlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                        {
                            InfyPOS.Models.Referencelist mmproduct = new InfyPOS.Models.Referencelist();
                            mmproduct.name = item.First().section;
                            mmproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                            mmproduct.isactive = true;
                            mmproduct.type = "DIVISION";
                            referencedataservice.Insert(mmproduct);
                            sectionlist.Add(mmproduct);
                            sectionid = mmproduct.id;

                        }
                        else
                        {
                            sectionid = sectionlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                        }
                        mproduct.salescounterid = sectionid;
                    }

                    productdataservice.Insert(mproduct);
                    productlist.Add(mproduct);
                    productid = mproduct.id;
                }
                else
                {
                    productid = productlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.productid = productid);
            }

            InfyPOS.Data.BrandDataService branddataservice = new InfyPOS.Data.BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            branddataservice.Context = content;
            var brandlist = branddataservice.Search(new InfyPOS.Models.BrandCreteria()
            {
            });
            foreach (var item in sales.GroupBy(e => e.brand))
            {
                long brandid = 0;

                if (!brandlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Brand mproduct = new InfyPOS.Models.Brand();
                    mproduct.name = item.Key;
                    mproduct.isactive = true;
                    branddataservice.Insert(mproduct);
                    brandlist.Add(mproduct);
                    brandid = mproduct.id;
                }
                else
                {
                    brandid = brandlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.brandid = brandid);
            }


            InfyPOS.Data.SupplierDataService supplierdataservice = new InfyPOS.Data.SupplierDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            supplierdataservice.Context = content;
            supplierdataservice.isMigration = true;
            var supplierlist = supplierdataservice.Search(new InfyPOS.Models.SupplierCreteria()
            {
                allsupplier = true
            });
            foreach (var item in sales.GroupBy(e => e.supplier))
            {
                long supplierid = 0;
                long suppllercompanyid = 0;
                if (!supplierlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Supplier mproduct = new InfyPOS.Models.Supplier();
                    mproduct.name = item.Key;
                    mproduct.isactive = true;
                    mproduct.code = item.First().suppliercode;

                    if (Suppliers != null && Suppliers.Exists(e => e.code.ToLower() == item.Key.ToLower()))
                    {
                        var sup = Suppliers.Find(e => e.code == item.Key);
                        var cmp = new InfyPOS.Models.Suppliercompany();
                        cmp.address = sup.address;
                        cmp.name = sup.name;
                        cmp.cityid = City.Exists(e => e.code == sup.city) ? City.Find(e => e.code == sup.city).id : 0;
                        cmp.stateid = State.Exists(e => e.code == sup.state) ? State.Find(e => e.code == sup.state).id : 0;
                        cmp.companycontactno = sup.mobile;
                        cmp.tinanddate = sup.gst;
                        mproduct.name = sup.name;
                        mproduct.code = sup.code;
                        mproduct.companylist = new List<InfyPOS.Models.Suppliercompany>();
                        mproduct.companylist.Add(cmp);
                    }
                    else
                    {
                        mproduct.companylist = new List<InfyPOS.Models.Suppliercompany>();
                        mproduct.companylist.Add(new InfyPOS.Models.Suppliercompany()
                        {
                            name = item.Key,
                            isactive = true
                        });
                    }
                    supplierdataservice.Insert(mproduct);
                    supplierlist.Add(mproduct);
                    supplierid = mproduct.id;
                    suppllercompanyid = mproduct.companylist[0].id;
                }
                else
                {
                    supplierid = supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                    if (supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()).companylist != null)
                        suppllercompanyid = supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()).companylist[0].id;
                }
                item.ToList().ForEach(e =>
                {
                    e.supplierid = supplierid;
                    e.suppliercompanyid = suppllercompanyid;
                });
            }

            foreach (var item in sales.GroupBy(e => e.sourcesupplier))
            {
                long supplierid = 0;
                long suppllercompanyid = 0;
                if (!supplierlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Supplier mproduct = new InfyPOS.Models.Supplier();
                    mproduct.name = item.Key;
                    mproduct.isactive = true;
                    mproduct.code = item.First().suppliercode;

                    if (Suppliers != null && Suppliers.Exists(e => e.code.ToLower() == item.Key.ToLower()))
                    {
                        var sup = Suppliers.Find(e => e.code == item.Key);
                        var cmp = new InfyPOS.Models.Suppliercompany();
                        cmp.address = sup.address;
                        cmp.name = sup.name;
                        cmp.cityid = City.Exists(e => e.code == sup.city) ? City.Find(e => e.code == sup.city).id : 0;
                        cmp.stateid = State.Exists(e => e.code == sup.state) ? State.Find(e => e.code == sup.state).id : 0;
                        cmp.companycontactno = sup.mobile;
                        cmp.tinanddate = sup.gst;
                        mproduct.name = sup.name;
                        mproduct.code = sup.code;
                        mproduct.companylist = new List<InfyPOS.Models.Suppliercompany>();
                        mproduct.companylist.Add(cmp);
                    }
                    else
                    {
                        mproduct.companylist = new List<InfyPOS.Models.Suppliercompany>();
                        mproduct.companylist.Add(new InfyPOS.Models.Suppliercompany()
                        {
                            name = item.Key,
                            isactive = true
                        });
                    }
                    supplierdataservice.Insert(mproduct);
                    supplierlist.Add(mproduct);
                    supplierid = mproduct.id;
                    suppllercompanyid = mproduct.companylist[0].id;
                }
                else
                {
                    supplierid = supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                    if (supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()).companylist != null)
                        suppllercompanyid = supplierlist.Find(e => e.name.ToLower() == item.Key.ToLower()).companylist[0].id;
                }
                item.ToList().ForEach(e =>
                {
                    e.sourcesupplierid = supplierid;
                });
            }


            var sleevelist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "SLEEVE"
            });
            foreach (var item in sales.GroupBy(e => e.sleeve))
            {
                long sleeveid = 0;
                if (!sleevelist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.type = "SLEEVE";
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    referencedataservice.Insert(mproduct);
                    sleevelist.Add(mproduct);
                    sleeveid = mproduct.id;
                }
                else
                {
                    sleeveid = sleevelist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.sleeveid = sleeveid);
            }


            var fitlist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "FIT"
            });
            foreach (var item in sales.GroupBy(e => e.fit))
            {
                long fitid = 0;
                if (!fitlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.type = "FIT";
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    referencedataservice.Insert(mproduct);
                    fitlist.Add(mproduct);
                    fitid = mproduct.id;
                }
                else
                {
                    fitid = fitlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.fitid = fitid);
            }

            var stylelist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "STYLE"
            });
            foreach (var item in sales.GroupBy(e => e.style))
            {
                long styleid = 0;
                if (!stylelist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.type = "STYLE";
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    referencedataservice.Insert(mproduct);
                    stylelist.Add(mproduct);
                    styleid = mproduct.id;
                }
                else
                {
                    styleid = stylelist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.styleid = styleid);
            }

            var patternlist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "PATTERN"
            });
            foreach (var item in sales.GroupBy(e => e.pattern))
            {
                long patternid = 0;
                if (!patternlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    mproduct.type = "PATTERN";
                    referencedataservice.Insert(mproduct);
                    patternlist.Add(mproduct);
                    patternid = mproduct.id;
                }
                else
                {
                    patternid = patternlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.patternid = patternid);
            }

            var sizelist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "SIZE"
            });
            foreach (var item in sales.GroupBy(e => e.size))
            {
                long sizeid = 0;
                if (!sizelist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    mproduct.type = "SIZE";
                    referencedataservice.Insert(mproduct);
                    sizelist.Add(mproduct);
                    sizeid = mproduct.id;
                }
                else
                {
                    sizeid = sizelist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.sizeid = sizeid);
            }

            var materiallist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "MATERIAL"
            });
            foreach (var item in sales.GroupBy(e => e.material))
            {
                long materialid = 0;
                if (!materiallist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    mproduct.type = "MATERIAL";
                    referencedataservice.Insert(mproduct);
                    materiallist.Add(mproduct);
                    materialid = mproduct.id;
                }
                else
                {
                    materialid = materiallist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.materialid = materialid);
            }


            var colorlist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "COLOUR"
            });
            foreach (var item in sales.GroupBy(e => e.colour))
            {
                long colorid = 0;
                if (!colorlist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.type = "COLOUR";
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    referencedataservice.Insert(mproduct);
                    colorlist.Add(mproduct);
                    colorid = mproduct.id;
                }
                else
                {
                    colorid = colorlist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.colourid = colorid);
            }

            var pricetaglist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "PRICETAGS"
            });
            foreach (var item in sales.GroupBy(e => e.pricetag))
            {
                long pricetagid = 0;
                if (!pricetaglist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.type = "PRICETAGS";
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    referencedataservice.Insert(mproduct);
                    pricetaglist.Add(mproduct);
                    pricetagid = mproduct.id;
                }
                else
                {
                    pricetagid = pricetaglist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.pricetagid = pricetagid);
            }


            var typelist = referencedataservice.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "TYPE"
            });
            foreach (var item in sales.GroupBy(e => e.type))
            {
                long typeid = 0;
                if (!typelist.Exists(e => e.name.ToLower() == item.Key.ToLower()))
                {
                    InfyPOS.Models.Referencelist mproduct = new InfyPOS.Models.Referencelist();
                    mproduct.name = item.Key;
                    mproduct.type = "TYPE";
                    mproduct.additionalinfo = new InfyPOS.Models.Referencelist.Attribute();
                    mproduct.isactive = true;
                    referencedataservice.Insert(mproduct);
                    typelist.Add(mproduct);
                    typeid = mproduct.id;
                }
                else
                {
                    typeid = typelist.Find(e => e.name.ToLower() == item.Key.ToLower()).id;
                }
                item.ToList().ForEach(e => e.typeid = typeid);
            }

            InfyPOS.Data.ItemsDataService itemdataservice = new InfyPOS.Data.ItemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            itemdataservice.isMigration = true;
            itemdataservice.Context = content;
            var itemlist = itemdataservice.Search(new InfyPOS.Models.ItemsCreteria()
            {

            });

            foreach (var sale in sales.GroupBy(e => new
            {
                e.gln,
                e.productid,
                e.brandid,
                e.sizeid,
                e.styleid,
                e.patternid,
                e.colourid,
                e.materialid,
                e.typeid,
                e.sleeveid,
                e.fitid
            }))
            {
                long itemid = 0;

                if (!string.IsNullOrEmpty(sale.Key.gln) && itemlist.Exists(x => x.gln == sale.Key.gln))
                {
                    itemid = itemlist.Find(x => x.gln == sale.Key.gln).id;
                }
                else if (!itemlist.Exists(e => e.brandid == sale.Key.brandid &&
                e.productid == sale.Key.productid &&
                e.sizeid == sale.Key.sizeid &&
                e.styleid == sale.Key.styleid &&
                e.patternid == sale.Key.patternid &&
                e.typeid == sale.Key.typeid &&
                e.colourid == sale.Key.colourid &&
                e.materialid == sale.Key.materialid &&
                e.fitid == sale.Key.fitid &&
                e.sleeveid == sale.Key.sleeveid))
                {
                    var first = sale.First();
                    InfyPOS.Models.Items mproduct = new InfyPOS.Models.Items();
                    mproduct.printingname = first.product;
                    mproduct.sellingname = first.product;
                    mproduct.brandname = first.brand;
                    mproduct.materialname = first.material;
                    mproduct.stylename = first.style;
                    mproduct.typename = first.type;
                    mproduct.patternname = first.pattern;
                    mproduct.productname = first.product;
                    mproduct.colourname = first.colour;
                    mproduct.sizename = first.size;
                    mproduct.gln = first.gln;
                    
                    mproduct.fitid = first.fitid;
                    mproduct.sleeveid = first.sleeveid;
                    mproduct.brandid = sale.Key.brandid;
                    mproduct.productid = sale.Key.productid;
                    mproduct.sizeid = sale.Key.sizeid;
                    mproduct.styleid = sale.Key.styleid;
                    mproduct.patternid = sale.Key.patternid;
                    mproduct.colourid = sale.Key.colourid;
                    mproduct.materialid = sale.Key.materialid;
                    mproduct.typeid = sale.Key.typeid;
                    mproduct.isactive = true;
                    itemdataservice.Insert(mproduct);
                    itemlist.Add(mproduct);
                    itemid = mproduct.id;
                }
                else
                {
                    itemid = itemlist.Find(e => e.brandid == sale.Key.brandid &&
                                        e.productid == sale.Key.productid &&
                                        e.sizeid == sale.Key.sizeid &&
                                        e.styleid == sale.Key.styleid &&
                                        e.patternid == sale.Key.patternid &&
                                        e.colourid == sale.Key.colourid &&
                                        e.typeid == sale.Key.typeid &&
                                        e.materialid == sale.Key.materialid &&
                                        e.fitid == sale.Key.fitid &&
                                        e.sleeveid == sale.Key.sleeveid).id;
                }
                sale.ToList().ForEach(e => e.itemid = itemid);
            }
        }

    }
}