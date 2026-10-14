using ClosedXML.Excel;
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using InfyPOS.Processors.Migration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace InfyPOS.Processors
{
    public class ExcelImportManager
    {
        long organizationid = 0;
        public ExcelImportManager(long organizationid)
        {
            this.organizationid = organizationid;
        }

        public byte[] Stock(long locationid)
        {
            BaseData baseData = new BaseData();
            string query = @"select agent.name agent,
            stock.barcode,
            brand.name brand,
            colour.name colour,
            stock.designid,
            salablegoods.discount,
            stock.hsncode,
            lrinvoice.invoicedate,
            lrinvoice.invoiceno,
            lrentry.lrentryno,
            material.name material,
            pattern.name pattern,
            products.name product,
            lrentry.lrentrydate,
            lrinvoiceitems.basebuyingprice,
            lrinvoiceitems.purchasediscount,
            lrinvoiceitems.buyingqty,
            lrinvoiceitems.buyingprice,
            salablegoods.price salerate,
            salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) retailstock,
            sections.name section,
            size.name size,
            style.name style,
            supplier.name supplier,
            suppliercity.name suppliercity,
            lrinvoiceitems.purchasetaxpercentage,
            lrinvoiceitems.salestaxpercentage
            from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            inner join items on items.id = stock.itemid
            left outer join products on products.id = items.productid
            left outer join referencelist sections on sections.id = products.salescounterid
            left outer join brand on brand.id = items.brandid
            left outer join referencelist colour on colour.id = items.colourid
            left outer join referencelist size on size.id = items.sizeid
            left outer join referencelist material on material.id = items.materialid
            left outer join referencelist pattern on pattern.id = items.patternid
            left outer join referencelist style on style.id = items.styleid
            left outer join referencelist type on type.id = items.typeid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
            left outer join lrentry on lrentry.id = lrinvoice.lrid
            left outer join agent on agent.id = lrentry.agentid
            left outer join supplier on supplier.id = stock.supplierid
            left outer join suppliercompany on suppliercompany.supplierid = supplier.id
            left outer join referencelist suppliercity on suppliercity.id = suppliercompany.cityid
            where salablegoods.locationid=@locationid";


            if(locationid==0)
            {
                query = @"select agent.name agent,
                stock.barcode,
                brand.name brand,
                colour.name colour,
                stock.designid,
                salablegoods.discount,
                stock.hsncode,
                lrinvoice.invoicedate,
                lrinvoice.invoiceno,
                lrentry.lrentryno,
                material.name material,
                pattern.name pattern,
                products.name product,
                lrentry.lrentrydate,
                lrinvoiceitems.basebuyingprice,
                lrinvoiceitems.purchasediscount,
                lrinvoiceitems.buyingqty,
                lrinvoiceitems.buyingprice,
                stock.price salerate,
                stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty) warehousestock,
                sections.name section,
                size.name size,
                style.name style,
                supplier.name supplier,
                suppliercity.name suppliercity,
                lrinvoiceitems.purchasetaxpercentage,
                lrinvoiceitems.salestaxpercentage
                from stock
                inner join items on items.id = stock.itemid
                left outer join products on products.id = items.productid
                left outer join referencelist sections on sections.id = products.salescounterid
                left outer join brand on brand.id = items.brandid
                left outer join referencelist colour on colour.id = items.colourid
                left outer join referencelist size on size.id = items.sizeid
                left outer join referencelist material on material.id = items.materialid
                left outer join referencelist pattern on pattern.id = items.patternid
                left outer join referencelist style on style.id = items.styleid
                left outer join referencelist type on type.id = items.typeid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                left outer join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
                left outer join lrentry on lrentry.id = lrinvoice.lrid
                left outer join agent on agent.id = lrentry.agentid
                left outer join supplier on supplier.id = stock.supplierid
                left outer join suppliercompany on suppliercompany.supplierid = supplier.id
                left outer join referencelist suppliercity on suppliercity.id = suppliercompany.cityid";
            }

            var param = new SortedList<string, object>();
            param.Add("locationid", locationid);
            var dt = baseData.ExecuteDataSet(query, param,100000).Tables[0];

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                ClosedXML.Excel.XLWorkbook xLWorkbook = new ClosedXML.Excel.XLWorkbook();
                if (dt.Rows.Count > 1000000)
                {
                    var tables = SplitTable(dt, 1000000);
                    int i = 1;
                    foreach(var table in tables)
                        xLWorkbook.AddWorksheet(dt, "Stock_"+(i++));
                    xLWorkbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }
                else
                {
                    var sheet = xLWorkbook.AddWorksheet(dt, "Stock");
                    xLWorkbook.SaveAs(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        private static List<DataTable> SplitTable(DataTable originalTable, int batchSize)
        {
            List<DataTable> tables = new List<DataTable>();
            int i = 0;
            int j = 1;
            DataTable newDt = originalTable.Clone();
            newDt.TableName = "Table_" + j;
            newDt.Clear();
            foreach (DataRow row in originalTable.Rows)
            {
                DataRow newRow = newDt.NewRow();
                newRow.ItemArray = row.ItemArray;
                newDt.Rows.Add(newRow);
                i++;
                if (i == batchSize)
                {
                    tables.Add(newDt);
                    j++;
                    newDt = originalTable.Clone();
                    newDt.TableName = "Table_" + j;
                    newDt.Clear();
                    i = 0;
                }
            }
            return tables;
        }
        public class AgreementStatus : Models.Lrinvoice
        {
            public List<string> missing_design { get; set; }
            public List<string> missing_uan { get; set; }
            public List<string> missing_product { get; set; }
            public List<string> missing_brand { get; set; }
            public List<string> missing_size { get; set; }
            public List<string> missing_pattern { get; set; }
            public List<string> missing_material { get; set; }
            public List<string> missing_style { get; set; }
            public List<string> missing_colour { get; set; }
            public List<string> missing_fit { get; set; }
            public List<string> missing_sleeve { get; set; }
            public List<string> missing_type { get; set; }
            public List<string> missing_uanlist { get; set; }
            public List<string> brandlist { get; set; }

            public string error { get; set; }
            public bool success { get; set; }
            public int noofrows { get; set; }

            public class MissingAttribute
            {
                public string name { get; set; }
                public int count { get; set; }
            }
        }

        public class DocumentStatus : InfyPOS.Common.BaseEntity
        {
            public string filename { get; set; }
            public long fileid { get; set; }
            public POUANUploaded postatus { get; set; }
            public SOUANUploaded sostatus { get; set; }
            public SKUUploaded skustatus { get; set; }
            public STOUANUploaded stostatus { get; internal set; }
            public ECOMUANUploaded ecomstatus { get; internal set; }
        }
        public class BillSummary : InfyPOS.Common.BaseEntity
        {
            public MigrationData MigrationData { get; set; }
            public AgreementStatus Status { get; set; }
            public string filename { get; set; }
            public long fileid { get; set; }
            public string[] columns { get; set; }
            public bool haserror { get; set; }
            public string errormessages { get; set; }
            public int importcount { get; set; }
            [Newtonsoft.Json.JsonIgnore]
            public List<AgreementProductItem> items { get; set; }
        }

        public class PurchaseOrderItem : BaseImportItem
        {
            public class Item
            {
                public long colourid { get; set; }
                public decimal mrp { get; set; }
                public long sizeid { get; set; }
                public string size { get; set; }
                public string hsn { get; set; }
                public long taxid { get; set; }
                public long productid { get; set; }
                public long id { get; set; }
                public string printingname { get; set; }
                public string glndesignid { get; set; }
                public string gln { get; set; }
            }

            public PurchaseOrderItem(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                //SUPPLIER,BOOKINGNO,SUPPLIERGST,PODATE,DUEDATE,EXPIRYDATE,UAN,TOLERANCE,HSN,COMPANY
                Company = GetString(source, columnMap, "Company");
                Location = GetString(source, columnMap, "Location");
                EntryLocation = GetString(source, columnMap, "EntryLocation");
                Supplier = GetString(source, columnMap, "Supplier");
                PODate = GetDate(source, columnMap, "PODate");
                DueDate = GetDate(source, columnMap, "DueDate");
                ExpiryDate = GetDate(source, columnMap, "ExpiryDate");
                SupplierGST = GetString(source, columnMap, "SupplierGST");
                HSN = GetString(source, columnMap, "HSN");
                Cost = GetDecimal(source, columnMap, "Cost");
                Tolerance = GetDecimal(source, columnMap, "Tolerance");
                Qty = GetDecimal(source, columnMap, "Qty");
                UAN = GetString(source, columnMap, "UAN");
                OrderSplit = GetString(source, columnMap, "OrderSplit");
                Remark = GetString(source, columnMap, "Remark");
                OrderType = GetString(source, columnMap, "OrderType");
            }
            public string OrderType { get; set; }
            public string Remark { get; set; }
            public string OrderSplit { get; set; }
            public long supplierid { get; set; }
            public string Supplier { get; set; }
            public string Company { get; set; }
            public string SupplierGST { get; set; }
            public string Location { get; set; }
            public string EntryLocation { get; set; }
            public DateTime PODate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public decimal Tolerance { get; set; }
            public decimal Qty { get; set; }
            public decimal Cost { get; set; }
            public string UAN { get; set; }
            public string HSN { get; set; }
            public Item info { get; set; }
        }

        public class ECommerceOrderItem : BaseImportItem
        {
            public class Item
            {
                public string prdsku { get; set; }
                public long colourid { get; set; }
                public decimal mrp { get; set; }
                public long sizeid { get; set; }
                public string size { get; set; }
                public string hsn { get; set; }
                public long taxid { get; set; }
                public long productid { get; set; }
                public long id { get; set; }
                public string printingname { get; set; }
                public string glndesignid { get; set; }
                public string gln { get; set; }
                public decimal stk { get; internal set; }
                public decimal stv { get; internal set; }
                public decimal sto { get; internal set; }
                public decimal so { get; internal set; }
            }
            public ECommerceOrderItem()
            {

            }
            public ECommerceOrderItem(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                //Order No=MORDERNO,	Channel=Channel,	ExternOrderNo=ORDERNO,	OrderDate=BILLDATE,	SKU=SKU,Cutomer Name=CUSTOMER,	
                //Ship Address=ADDRESS,	Ship City=CITY,	Ship PinCode=PINCODE,	Ship Phone=CONTACTNO
                //Unit Price=PRICE,	Shipped Qty=QTY,	Voucher Amount=DISCOUNT,	Ship State=STATE,	LineAmount=AMOUNT,
                Company = GetString(source, columnMap, "Company");
                Location = GetString(source, columnMap, "Location");
                BillDate = GetDate(source, columnMap, "BILLDATE");
                Qty = GetDecimal(source, columnMap, "Qty");
                UAN = GetString(source, columnMap, "UAN");
                SKU = GetString(source, columnMap, "SKU");

                CONTACTNO = GetString(source, columnMap, "CONTACTNO");
                CITY = GetString(source, columnMap, "CITY");
                PRICE = GetDecimal(source, columnMap, "PRICE");
                DISCOUNT = GetDecimal(source, columnMap, "DISCOUNT");
                AMOUNT = GetDecimal(source, columnMap, "AMOUNT");
                if (PRICE - AMOUNT > 0)
                    DISCOUNT = Math.Round(PRICE - AMOUNT / Qty, 0);
                else
                    DISCOUNT = 0;
                CUSTOMER = GetString(source, columnMap, "CUSTOMER");
                ADDRESS = GetString(source, columnMap, "ADDRESS");
                PINCODE = GetString(source, columnMap, "PINCODE");
                Channel = GetString(source, columnMap, "Channel");
                STATE = GetString(source, columnMap, "STATE");
                MOrderNo = GetString(source, columnMap, "MORDERNO");
                OrderNo = GetString(source, columnMap, "ORDERNO");
            }
            public string MOrderNo { get; set; }
            public string OrderNo { get; set; }
            public string Company { get; set; }
            public string Location { get; set; }
            public string Channel { get; set; }
            public DateTime BillDate { get; set; }
            public decimal Qty { get; set; }
            public string UAN { get; set; }
            public Item info { get; set; }
            public long locationid { get; internal set; }
            public long companyid { get; internal set; }
            public string SKU { get; set; }
            public string CUSTOMER { get; set; }
            public string CONTACTNO { get; set; }
            public string CITY { get; set; }
            public string ADDRESS { get; set; }
            public string PINCODE { get; set; }
            public string STATE { get; set; }
            public decimal PRICE { get; set; }
            public decimal DISCOUNT { get; set; }
            public decimal AMOUNT { get; set; }
            public long customerid { get;  set; }
            public long categoryid { get;  set; }
            public string invoicecode { get;  set; }
            public string invoicedisplaycode { get;  set; }
            public DateTime dispatchon { get; set; }
        }

        public class StockOrderItem : BaseImportItem
        {
            public class LocationCompany
            {
                public long locationid { get; set; }
                public string locationname { get; set; }
                public long companyid { get; set; }
                public string companyname { get; set; }
                public long customerid { get;  set; }
                public long categoryid { get; set; }
            }
            public class Item
            {
                public long colourid { get; set; }
                public decimal mrp { get; set; }
                public long sizeid { get; set; }
                public string size { get; set; }
                public string hsn { get; set; }
                public long taxid { get; set; }
                public long productid { get; set; }
                public long id { get; set; }
                public string printingname { get; set; }
                public string glndesignid { get; set; }
                public string gln { get; set; }
                public decimal stk { get; set; }
                public decimal stv { get; set; }
                public decimal sto { get; set; }
                public decimal so { get; set; }
            }

            public StockOrderItem(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                //SUPPLIER,BOOKINGNO,SUPPLIERGST,PODATE,DUEDATE,EXPIRYDATE,UAN,TOLERANCE,HSN,COMPANY
                Company = GetString(source, columnMap, "Company");
                FromLocation = GetString(source, columnMap, "FromLocation");
                ToLocation = GetString(source, columnMap, "ToLocation");
                STODate = GetDate(source, columnMap, "STODate");
                Qty = GetDecimal(source, columnMap, "Qty");
                Cost = GetDecimal(source, columnMap, "Cost");
                UAN = GetString(source, columnMap, "UAN");
                OrderSplit = GetString(source, columnMap, "OrderSplit");
            }
            public string Remark { get; set; }
            public string OrderSplit { get; set; }
            public string Company { get; set; }
            public string FromLocation { get; set; }
            public string ToLocation { get; set; }
            public DateTime STODate { get; set; }
            public decimal Qty { get; set; }
            public decimal Cost { get; set; }
            public string UAN { get; set; }
            public Item info { get; set; }
            public long fromlocationid { get; internal set; }
            public long tolocationid { get; internal set; }
            public long fromcompanyid { get; internal set; }
            public long tocompanyid { get; internal set; }
            public long categoryid { get; internal set; }
        }

        public class SalesOrderItem : BaseImportItem
        {
            public class Item
            {
                public long colourid { get; set; }
                public decimal mrp { get; set; }
                public string hsn { get; set; }
                public long sizeid { get; set; }
                public string size { get; set; }
                public long taxid { get; set; }
                public long productid { get; set; }
                public long id { get; set; }
                public string printingname { get; set; }
                public string glndesignid { get; set; }
                public string gln { get; set; }
            }

            public SalesOrderItem(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                //CUSTOMER,BOOKINGNO,CUSTOMERGST,SODATE,DUEDATE,EXPIRYDATE,UAN,TOLERANCE,PRICE
                Customer = GetString(source, columnMap, "Customer");
                Company = GetString(source, columnMap, "Company");
                BookingNo = GetString(source, columnMap, "BookingNo");
                Location = GetString(source, columnMap, "Location");
                SODate = GetDate(source, columnMap, "SODate");
                DueDate = GetDate(source, columnMap, "DueDate");
                ExpiryDate = GetDate(source, columnMap, "ExpiryDate");
                if (SODate == DateTime.MinValue)
                    SODate = DateTime.Now;
                if (DueDate == DateTime.MinValue)
                    DueDate = DateTime.Now;
                if (ExpiryDate == DateTime.MinValue)
                    ExpiryDate = DateTime.Now;
                CustomerGST = GetString(source, columnMap, "CustomerGST");
                Tolerance = GetDecimal(source, columnMap, "Tolerance");
                Qty = GetDecimal(source, columnMap, "Qty");
                Price = GetDecimal(source, columnMap, "Price");
                UAN = GetString(source, columnMap, "UAN");
                OrderSplit = GetString(source, columnMap, "OrderSplit");
                Remark = GetString(source, columnMap, "Remark");
                OrderType = GetString(source, columnMap, "OrderType");
            }
            public string OrderType { get; set; }
            public string Remark { get; set; }
            public string BookingNo { get; set; }
            public string OrderSplit { get; set; }
            public long customerid { get; set; }
            public long categoryid { get; set; }
            public string Company { get; set; }
            public string Customer { get; set; }
            public string Location { get; set; }
            public string CustomerGST { get; set; }
            public DateTime SODate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime ExpiryDate { get; set; }
            public decimal Tolerance { get; set; }
            public decimal Qty { get; set; }
            public decimal Price { get; set; }
            public string UAN { get; set; }
            public string HSN { get; set; }
            public Item info { get; set; }
        }



        public ECOMUANUploaded GetECOStatus(List<ECommerceOrderItem> list, ImageData data)
        {
            STOUANUploaded pOStatus = new STOUANUploaded();
            var locations = list.ConvertAll(x => x.Location).ToList().Distinct().ToList();
            var uanlist = list.ConvertAll(x => x.UAN).ToList().Distinct().ToList();
            var skulist = list.ConvertAll(x => x.SKU).ToList().Distinct().ToList();
            var customers = list.ConvertAll(x => x.Channel).ToList().Distinct().ToList();
            locations.Distinct().ToList().RemoveAll(x => string.IsNullOrEmpty(x));
            BaseData baseData = new BaseData();
            var status = new ECOMUANUploaded();
            status.missinglocation = new List<string>();
            status.missinguan = new List<string>();
            status.missingcustomers = new List<string>();
            if (customers.Count > 0)
            {
                SortedDictionary<string, Tuple<long, long>> suppliers = new SortedDictionary<string, Tuple<long, long>>();
                string query = "select id,name,categoryid from customer where upper(name) in (" + string.Join(",", customers.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x.ToUpper()) + "'")) + ") and isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]).ToUpper();
                        long categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        suppliers.Add(name, new Tuple<long, long>(id, categoryid));
                    }
                }

                var missingchannels = list.FindAll(x => x.Channel == null);
                var missinglocations = list.FindAll(x => x.Location == null);
                if(missinglocations.Count>0)
                {
                    throw new Exception("Pleaes configure Unicommerce channel with stock location for " + string.Join(",", missinglocations.ConvertAll(x => x.Channel).Distinct().ToList()));
                }

                foreach (var grp in list.GroupBy(x => x.Channel))
                {
                    if (grp.Key != null && suppliers.ContainsKey(grp.Key.ToUpper()))
                    {
                        long id = suppliers[grp.Key.ToUpper()].Item1;
                        long catid = suppliers[grp.Key.ToUpper()].Item2;
                        grp.ForEach(x =>
                        {
                            x.customerid = id;
                            x.categoryid = catid;
                        });
                    }
                    else
                    {
                        status.missingcustomers.Add(grp.Key);
                    }
                }
            }

            if (locations.Count > 0)
            {
                SortedDictionary<string, StockOrderItem.LocationCompany> suppliers = new SortedDictionary<string, StockOrderItem.LocationCompany>();
                string query = @"select referencelist.id,referencelist.name,
                company.id companyid,company.name companyname
                from referencelist
                left outer join company on company.id = cast(additionalinfo->>'parentid' as bigint)
                where type='LOCATION' and referencelist.name in (" + string.Join(",", locations.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and referencelist.isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        StockOrderItem.LocationCompany locationCompany = new StockOrderItem.LocationCompany();
                        locationCompany.locationid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        locationCompany.locationname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]).ToUpper();
                        locationCompany.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        locationCompany.companyname = reader["companyname"] == DBNull.Value ? "" : Convert.ToString(reader["companyname"]).ToUpper();
                        suppliers.Add(locationCompany.locationname, locationCompany);
                    }
                }


                foreach (var grp in list.GroupBy(x => x.Location))
                {
                    if (grp.Key != null && suppliers.ContainsKey(grp.Key.ToUpper()))
                    {
                        long id = suppliers[grp.Key.ToUpper()].locationid;
                        long cid = suppliers[grp.Key.ToUpper()].companyid;
                        grp.ForEach(x =>
                        {
                            x.locationid = id;
                            x.companyid = cid;
                        });
                    }
                    else
                    {
                        status.missinglocation.Add(grp.Key);
                    }
                }
            }
            uanlist.RemoveAll(x => string.IsNullOrEmpty(x));
            skulist.RemoveAll(x => string.IsNullOrEmpty(x));
            bool isuanbased = false;
            if (uanlist.Count > 0 ||
                skulist.Count > 0)
            {
                SortedDictionary<string, ECommerceOrderItem.Item> suppliers = new SortedDictionary<string, ECommerceOrderItem.Item>();
                string query = "";

                if (uanlist.Count > 0)
                {
                    query = @"select schemelist->>'prdsku' prdsku,mfgdesign.code glndesignid,schemelist->>'name' printingname,
                                cast(schemelist->>'itemid' as bigint) id,cast(schemelist->>'sizeid' as bigint) sizeid,
                                cast(schemelist->>'colourid' as bigint) colourid,schemelist->>'uid' gln,schemelist->>'prdhsn' prdhsn,
                                mfgdesign.productid,referencelist.name sizename,cast(schemelist->>'mrp' as numeric) mrp,prdhsncode
                                from mfgdesign
                                LEFT JOIN LATERAL
                                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                                left outer join referencelist on referencelist.id = cast(schemelist->>'sizeid' as bigint)
                                where mfgdesign.isactive = true and 
                                schemelist->>'uid' in (" + string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ")";
                    isuanbased = true;
                }
                else
                {
                    query = @"select schemelist->>'prdsku' prdsku,mfgdesign.code glndesignid,schemelist->>'name' printingname,
                                cast(schemelist->>'itemid' as bigint) id,cast(schemelist->>'sizeid' as bigint) sizeid,
                                cast(schemelist->>'colourid' as bigint) colourid,schemelist->>'uid' gln,schemelist->>'prdhsn' prdhsn,
                                mfgdesign.productid,referencelist.name sizename,cast(schemelist->>'mrp' as numeric) mrp,prdhsncode
                                from mfgdesign
                                LEFT JOIN LATERAL
                                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                                left outer join referencelist on referencelist.id = cast(schemelist->>'sizeid' as bigint)
                                where mfgdesign.isactive = true and 
                                upper(schemelist->>'prdsku') in (" + string.Join(",", skulist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x.ToUpper()) + "'")) + ") and mfgdesign.isactive=true";
                }

                List<string> avilableskus = new List<string>();
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        decimal mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        string prdsku = reader["prdsku"] == DBNull.Value ? "" : Convert.ToString(reader["prdsku"]).ToUpper();
                        string printingname = reader["printingname"] == DBNull.Value ? "" : Convert.ToString(reader["printingname"]).ToUpper();
                        string sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]).ToUpper();
                        string glndesignid = reader["glndesignid"] == DBNull.Value ? "" : Convert.ToString(reader["glndesignid"]).ToUpper();
                        string gln = reader["gln"] == DBNull.Value ? "" : Convert.ToString(reader["gln"]).ToUpper();
                        string prdhsncode = reader["prdhsncode"] == DBNull.Value ? "" : Convert.ToString(reader["prdhsncode"]).ToUpper();

                        avilableskus.Add(prdsku.ToUpper());
                        if (reader["prdhsn"] != DBNull.Value &&
                            reader["prdhsn"].ToString() != "")
                            prdhsncode = reader["prdhsn"].ToString();

                        if (!suppliers.ContainsKey(gln))
                        {
                            suppliers.Add(gln, new ECommerceOrderItem.Item()
                            {
                                id = id,
                                productid = productid,
                                printingname = printingname,
                                glndesignid = glndesignid,
                                size = sizename,
                                colourid = colourid,
                                sizeid = sizeid,
                                mrp = mrp,
                                prdsku = prdsku,
                                gln = gln,
                                hsn = prdhsncode
                            });
                        }
                    }

                    var missingskulist = skulist.FindAll(x => !avilableskus.Contains(x.ToUpper()));
                    if(missingskulist.Count>0)
                    {
                        throw new Exception("Missing sku - " + string.Join(",", missingskulist));
                    }
                    if (uanlist == null || uanlist.Count == 0)
                    {
                        uanlist = suppliers.Keys.ToList();
                        var allinfos = new SortedDictionary<string, ECommerceOrderItem.Item>();

                        foreach (var kv in suppliers.Values.ToList())
                        {
                            if (!allinfos.ContainsKey(kv.prdsku))
                                allinfos.Add(kv.prdsku, kv);
                        }
                        list.ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x.SKU) && allinfos.ContainsKey(x.SKU.ToUpper()))
                            {
                                x.info = allinfos[x.SKU.ToUpper()];
                                x.UAN = x.info.gln;
                            }
                        });
                        var missinguans = list.FindAll(x => string.IsNullOrEmpty(x.UAN));
                    }
                }

                status.stocklist = list.GroupBy(x => x.UAN).ToList().ConvertAll(x => new SOUANUploaded.StockInfo() { uan = x.Key, required = x.Sum(y => y.Qty) });
                query = @"with allstock as 
                          (
                              select stock.serialno,salablegoods.locationid,sum(iqty-(jqty+dqty+oqty)) stk,sum(stock.buyingprice*(iqty-(jqty+dqty+oqty))) stvalue from salablegoods 
                              inner join stock on stock.id = stockid
                              where salablegoods.isactive=true 
                              and salablegoods.locationid = @locationid
                              and stock.serialno in (" +
                              string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") " +
                              @"group by stock.serialno,salablegoods.locationid
                            ),currentpo as
                            (
                                select sum(oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)) pendingqty,
                                Salesorder.locationid,Salesorderitems.uid
                                from Salesorder 
                                inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
                                inner join allstock on allstock.serialno = Salesorderitems.uid and Salesorder.locationid=allstock.locationid
                                where oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)>0 and 
                                salesorder.isactive=true and Salesorderitems.isactive=true  and Salesorder.sosourcetype=0
                                group by Salesorderitems.uid,Salesorder.locationid
                            ),
                            currentsto as
                            (
                                select 
                                schemelist ->> 'serialno' uid,fromlocationid locationid,
                                sum(cast(schemelist ->> 'iqty' as numeric)) qty from bundles 
                                LEFT JOIN LATERAL 
                                json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                where  bundles.isactive=true and bundles.status<=0 and bundles.fromlocationid = @locationid
                                group by schemelist ->> 'serialno',fromlocationid
                            )
                            select allstock.*,currentpo.pendingqty,currentsto.qty stoqty from allstock
                            left outer join currentpo on currentpo.uid =  allstock.serialno and currentpo.locationid=allstock.locationid
                            left outer join currentsto on currentsto.uid = allstock.serialno and currentsto.locationid=allstock.locationid";

                var param = new SortedList<string, object>();
                param.Add("locationid", list.First().locationid);
                using (var reader = data.ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        string uid = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                        decimal stk = reader["stk"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stk"]);
                        decimal stv = reader["stvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stvalue"]);
                        decimal so = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        decimal sto = reader["stoqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stoqty"]);

                        if (suppliers.ContainsKey(uid))
                        {
                            suppliers[uid].stk = stk;
                            suppliers[uid].stv = stv;
                            suppliers[uid].sto = sto;
                            suppliers[uid].so = so;
                        }
                    }
                }
                foreach (var grp in list.GroupBy(x => x.UAN))
                {
                    var required = grp.Sum(x => x.Qty);
                    if (suppliers.ContainsKey(grp.Key))
                    {
                        status.stocklist.Find(x => x.uan == grp.Key).stock = suppliers[grp.Key].stk;
                        status.stocklist.Find(x => x.uan == grp.Key).so = suppliers[grp.Key].so;
                        status.stocklist.Find(x => x.uan == grp.Key).sto = suppliers[grp.Key].sto;
                        grp.ForEach(x => x.info = suppliers[grp.Key]);
                    }
                    else
                    {
                        status.missinguan.Add(grp.Key);
                    }
                }
                status.stocklist.RemoveAll(x => x.required <= (x.stock - (x.sto + x.so)));
            }

            TaxDataService taxDataService = new TaxDataService() { Context = data.Context };
            var taxlist = taxDataService.TaxProducts(list.FindAll(x => x.info != null).ConvertAll(x => x.info.productid).ToList().Distinct().ToList());

            CompanyDataService companyDataService = new CompanyDataService() { Context = data.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());
            ReferencelistDataService referenceList = new ReferencelistDataService() { Context = data.Context };
            var locationlist = referenceList.SearchList(new ReferencelistCreteria { type = "LOCATION" });
            var statelist = referenceList.SearchList(new ReferencelistCreteria { type = "STATE" });
            status.orders = new List<ECOMUANUploaded.Order>();
            
            status.missingstates = new List<string>();
            foreach (var ordergroup in list.GroupBy(x => new { x.Company, x.Location, x.MOrderNo, x.OrderNo , x.invoicecode,x.invoicedisplaycode  }))
            {
                ECOMUANUploaded.Order order = new ECOMUANUploaded.Order();
                order.Location = ordergroup.Key.Location;
                order.Company = ordergroup.Key.Company;
                order.MOrderNo = ordergroup.Key.MOrderNo;
                order.OrderNo = ordergroup.Key.OrderNo;
                order.invoicecode = ordergroup.Key.invoicecode;
                order.dispatchon = ordergroup.First().dispatchon;
                order.invoicedisplaycode = ordergroup.Key.invoicedisplaycode;
                order.customerid = ordergroup.First().customerid;
                order.locationid = ordergroup.First().locationid;
                order.companyid = ordergroup.First().companyid;
                order.BillDate = ordergroup.First().BillDate;
                order.ECommerce = new BillAttribute.ECommerce();
                order.ECommerce.customer = ordergroup.First().CUSTOMER;
                order.ECommerce.address = ordergroup.First().ADDRESS;
                order.ECommerce.pin = ordergroup.First().PINCODE;
                order.ECommerce.channel = ordergroup.First().Channel;
                order.ECommerce.state = ordergroup.First().STATE;
                order.ECommerce.morderid = ordergroup.First().MOrderNo;
                order.ECommerce.invoicecode = ordergroup.First().invoicecode;
                order.ECommerce.orderid = ordergroup.First().OrderNo;
                order.ECommerce.contact = ordergroup.First().CONTACTNO;
                order.ECommerce.city = ordergroup.First().CITY;
                var state = ordergroup.First().STATE;
                var stateparts = ordergroup.First().STATE.Split('-');
                if (statelist.Exists(x => x.name.ToLower() == ordergroup.First().STATE.ToLower()))
                {
                    order.stateid = statelist.Find(x => x.name.ToLower() == ordergroup.First().STATE.ToLower()).id;
                }
                else
                {
                    if (statelist.Exists(x => x.code == stateparts[0] || x.code == ("0" + stateparts[0].Trim())))
                    {
                        order.stateid = statelist.Find(x => x.code == stateparts[0] || x.code == ("0" + stateparts[0].Trim())).id;
                    }
                    else
                    {
                        if (!status.missingstates.Contains(ordergroup.First().STATE.ToLower()))
                            status.missingstates.Add(ordergroup.First().STATE.ToLower());
                    }
                }
                order.items = new List<ECOMUANUploaded.OrderItem>();
                foreach (var orderitemgroup in ordergroup.GroupBy(y => y.UAN))
                {
                    decimal taxpercentage = 0;
                    decimal tax = 0;
                    long taxid = 0;
                    var orderitem = orderitemgroup.First();
                    if (orderitem.info == null) continue;
                    decimal price = orderitem.PRICE - orderitem.DISCOUNT;
                    decimal mrp = orderitem.info == null ? 0 : orderitem.info.mrp;

                    if (mrp != 0 && mrp != orderitem.PRICE && orderitem.DISCOUNT != 0)
                    {
                        orderitem.DISCOUNT = 0;
                        if (Math.Abs(orderitem.PRICE * orderitemgroup.Sum(z => z.Qty)) != Math.Abs(orderitem.AMOUNT))
                        {
                            price = Math.Abs(orderitem.AMOUNT / orderitemgroup.Sum(z => z.Qty));
                        }
                    }

                    if (orderitem.info != null &&
                        taxlist.ContainsKey(orderitem.info.productid))
                    {
                        taxid = taxlist[orderitem.info.productid].id;
                        taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(price).taxpercent;
                        tax = Math.Round((price * orderitemgroup.Sum(z => z.Qty)) / (taxpercentage + 100) * taxpercentage, 2);
                    }
                    var ecomorderitem = new ECOMUANUploaded.OrderItem()
                    {
                        Price = orderitem.PRICE,
                        Discount = orderitem.DISCOUNT,
                        HSN = orderitem.info.hsn,
                        Qty = orderitemgroup.Sum(z => z.Qty),
                        PrdSKU = orderitem.SKU,
                        UAN = orderitem.UAN,
                        Percentage = taxpercentage,
                        Tax = tax,
                        TaxID = taxid,
                        ItemID = orderitem.info == null ? 0 : orderitem.info.id,
                        Design = orderitem.info == null ? "" : orderitem.info.glndesignid,
                        ColourID = orderitem.info == null ? 0 : orderitem.info.colourid,
                        SizeID = orderitem.info == null ? 0 : orderitem.info.sizeid,
                        MRP = orderitem.info == null ? 0 : orderitem.info.mrp,
                        Product = orderitem.info == null ? "" : orderitem.info.printingname,
                        ProductID = orderitem.info == null ? 0 : orderitem.info.productid,
                        Size = orderitem.info == null ? "" : orderitem.info.size
                    };
                    
                    order.items.Add(ecomorderitem);
                }
                if (isuanbased)
                    order.MissingUAN = order.items.FindAll(x => x.ItemID == 0).ConvertAll(x => x.UAN);
                else
                    order.MissingUAN = order.items.FindAll(x => x.ItemID == 0).ConvertAll(x => x.PrdSKU);
                order.Amount = order.items.Sum(x => x.Qty * x.Price);
                order.Qty = order.items.Sum(x => x.Qty);
                order.Tax = order.items.Sum(x => x.Tax);
                if (companylist.Exists(x => x.name.ToLower() == order.Company.ToLower()))
                    order.companyid = companylist.Find(x => x.name.ToLower() == order.Company.ToLower()).id;

                if (order.items.Exists(x => status.stocklist.Exists(y => y.uan == x.UAN)))
                {
                    order.stocknotavailable = true;
                }
                status.orders.Add(order);
            }

            var existquery = string.Format("Select ecomorderno,billdate,billno,billattributes->'ecom'->>'orderid' orderid,billattributes->'ecom'->>'morderid' morderid,billattributes->'ecom'->>'invoicecode' invoicecode from bill where isactive=true and billtype!=9 and ecomorderno in ({0})",
                string.Join(",", status.orders.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x.MOrderNo) + "'")));

            using (var reader = taxDataService.ExecuteReader(existquery, null))
            {
                while (reader.Read())
                {
                    string billno = reader["billno"].ToString();
                    string orderno = reader["ecomorderno"].ToString();
                    string orderid = reader["orderid"] == DBNull.Value ? "" : reader["orderid"].ToString();
                    string morderid = reader["morderid"] == DBNull.Value ? "" : reader["morderid"].ToString();
                    string invoicecode = reader["invoicecode"] == DBNull.Value ? "": reader["invoicecode"].ToString();
                    DateTime billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                    if (!string.IsNullOrEmpty(invoicecode))
                    {
                        if(status.orders.Exists(x => x.invoicecode == invoicecode && x.dispatchon.Date == billdate.Date))
                            status.orders.Find(x => x.invoicecode == invoicecode && x.dispatchon.Date == billdate.Date).BillNo = billno;
                    }
                    else
                    {
                        if(status.orders.Exists(x => x.OrderNo == orderno && x.MOrderNo == morderid))
                            status.orders.Find(x => x.OrderNo == orderno && x.MOrderNo == morderid).BillNo = billno;
                    }
                }
            }

            status.noofbill = status.orders.Count;
            status.missinguan = new List<string>();
            status.orders.ForEach(x => status.missinguan.AddRange(x.MissingUAN));
            status.missingqty = new List<string>();
            if (isuanbased)
                status.orders.ForEach(x => status.missingqty.AddRange(x.items.FindAll(y => y.Qty == 0).ConvertAll(z => z.UAN)));
            else
                status.orders.ForEach(x => status.missingqty.AddRange(x.items.FindAll(y => y.Qty == 0).ConvertAll(z => z.PrdSKU)));
            status.validbill = status.orders.Count(x => x.valid);
            status.missinglocation = status.orders.FindAll(x => x.locationid == 0).ConvertAll(x => x.Location).Distinct().ToList();
            status.missingcompany = status.orders.FindAll(x => x.companyid == 0).ConvertAll(x => x.Company).Distinct().ToList();

            //using (var reader = data.ExecuteReader("select billno,ecomorderno from bill where isactive=true and billtype != 9 and ecomorderno in (" +
            //String.Join(",", status.orders.ConvertAll(X => "'" + X.OrderNo + "'")) + ")", null))
            //{
            //    while (reader.Read())
            //    {
            //        var po = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
            //        var pouniquenumber = reader["ecomorderno"] == DBNull.Value ? "" : reader["ecomorderno"].ToString();
            //        var order = status.orders.Find(x => x.OrderNo == pouniquenumber);
            //        if (order.items.Exists(x => x.Qty > 0))
            //            order.BillNo = po;
            //    }
            //}
            return status;
        }
        private STOUANUploaded GetSTOStatus(List<StockOrderItem> list, ImageData data)
        {
            STOUANUploaded pOStatus = new STOUANUploaded();
            var locations = list.ConvertAll(x => x.FromLocation).ToList().Distinct().ToList();
            locations.AddRange(list.ConvertAll(x => x.ToLocation).ToList().Distinct().ToList());
            var uanlist = list.ConvertAll(x => x.UAN).ToList().Distinct().ToList();
            BaseData baseData = new BaseData();

            var uanquery = @"select schemelist->>'uid' uid,cast(schemelist->>'mrp' as numeric) mrp,
                                cast(schemelist->>'cost' as numeric) cost
                                from mfgdesign
                                LEFT JOIN LATERAL
                                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                                left outer join referencelist on referencelist.id = cast(schemelist->>'sizeid' as bigint)
                                where mfgdesign.isactive = true and 
                                schemelist->>'uid' in (" + string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ")";

            var uancostlist = baseData.ExecuteDataSet(uanquery, null).Tables[0].AsEnumerable().Select(x => new
            {
                uid = x["uid"].ToString(),
                cost = Convert.ToDecimal(x["cost"])
            }).ToList();


            locations.Distinct().ToList().RemoveAll(x => string.IsNullOrEmpty(x));
            var status = new STOUANUploaded();
            status.missinglocation = new List<string>();
            status.missinguan = new List<string>();
            if (locations.Count > 0)
            {
                SortedDictionary<string, StockOrderItem.LocationCompany> suppliers = new SortedDictionary<string, StockOrderItem.LocationCompany>();
                string query = @"select referencelist.id,referencelist.name,
                company.id companyid,company.name companyname,
                cast(additionalinfo->>'customerid' as bigint) customerid,
                customer.categoryid
                from referencelist
                left outer join company on company.id = cast(additionalinfo->>'parentid' as bigint)
                left outer join customer on customer.id = cast(additionalinfo->>'customerid' as bigint)
                where type='LOCATION' and referencelist.name in (" + string.Join(",", locations.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and referencelist.isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        StockOrderItem.LocationCompany locationCompany = new StockOrderItem.LocationCompany();
                        locationCompany.locationid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        locationCompany.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        locationCompany.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        locationCompany.locationname = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]).ToUpper();
                        locationCompany.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        locationCompany.companyname = reader["companyname"] == DBNull.Value ? "" : Convert.ToString(reader["companyname"]).ToUpper();
                        suppliers.Add(locationCompany.locationname, locationCompany);
                    }
                }
                foreach (var grp in list.GroupBy(x => x.FromLocation))
                {
                    if (suppliers.ContainsKey(grp.Key.ToUpper()))
                    {
                        long id = suppliers[grp.Key.ToUpper()].locationid;
                        long cid = suppliers[grp.Key.ToUpper()].companyid;
                        grp.ForEach(x =>
                        {
                            x.fromlocationid = id;
                            x.fromcompanyid = cid;
                        });
                    }
                    else
                    {
                        status.missinglocation.Add(grp.Key);
                    }
                }
                foreach (var grp in list.GroupBy(x => x.ToLocation))
                {
                    if (suppliers.ContainsKey(grp.Key.ToUpper()))
                    {
                        long id = suppliers[grp.Key.ToUpper()].locationid;
                        long cid = suppliers[grp.Key.ToUpper()].companyid;
                        long categoryid = suppliers[grp.Key.ToUpper()].categoryid;
                        grp.ForEach(x =>
                        {
                            x.tolocationid = id;
                            x.tocompanyid = cid;
                            x.categoryid = categoryid;
                        });
                    }
                    else
                    {
                        status.missinglocation.Add(grp.Key);
                    }
                }
            }
            if (uanlist.Count > 0)
            {
                SortedDictionary<string, StockOrderItem.Item> suppliers = new SortedDictionary<string, StockOrderItem.Item>();
                string query = @"select items.glndesignid,items.printingname,items.id,items.colourid,items.sizeid,
                                items.gln,items.productid,referencelist.name sizename,items.mrp,prdhsncode from items 
                                left outer join referencelist on referencelist.id = items.sizeid
                                left outer join mfgdesign on mfgdesign.code = glndesignid
                                where items.gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and items.isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        decimal mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        string printingname = reader["printingname"] == DBNull.Value ? "" : Convert.ToString(reader["printingname"]).ToUpper();
                        string sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]).ToUpper();
                        string glndesignid = reader["glndesignid"] == DBNull.Value ? "" : Convert.ToString(reader["glndesignid"]).ToUpper();
                        string gln = reader["gln"] == DBNull.Value ? "" : Convert.ToString(reader["gln"]).ToUpper();
                        string prdhsncode = reader["prdhsncode"] == DBNull.Value ? "" : Convert.ToString(reader["prdhsncode"]).ToUpper();

                        if (!suppliers.ContainsKey(gln))
                        {
                            suppliers.Add(gln, new StockOrderItem.Item()
                            {
                                id = id,
                                productid = productid,
                                printingname = printingname,
                                glndesignid = glndesignid,
                                size = sizename,
                                colourid = colourid,
                                sizeid = sizeid,
                                mrp = mrp,
                                hsn = prdhsncode
                            });
                        }
                    }
                }

                status.stocklist = list.GroupBy(x => x.UAN).ToList().ConvertAll(x => new SOUANUploaded.StockInfo() { uan = x.Key, required = x.Sum(y => y.Qty) });
                query = @"with allstock as 
                          (
                              select stock.serialno,salablegoods.locationid,sum(iqty-(jqty+dqty+oqty)) stk,sum(stock.buyingprice*(iqty-(jqty+dqty+oqty))) stvalue from salablegoods 
                              inner join stock on stock.id = stockid
                              where salablegoods.isactive=true 
                              and salablegoods.locationid = @locationid
                              and stock.serialno in (" +
                              string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") " +
                              @"group by stock.serialno,salablegoods.locationid
                            ),currentpo as
                            (
                                select sum(oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)) pendingqty,
                                Salesorder.locationid,Salesorderitems.uid
                                from Salesorder 
                                inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
                                inner join allstock on allstock.serialno = Salesorderitems.uid  and Salesorder.locationid=allstock.locationid
                                where oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)>0 and 
                                salesorder.isactive=true and Salesorderitems.isactive=true and Salesorder.sosourcetype=0
                                group by Salesorderitems.uid,Salesorder.locationid
                            ),
                            currentsto as
                            (
                                select 
                                schemelist ->> 'serialno' uid,fromlocationid locationid,
                                sum(cast(schemelist ->> 'iqty' as numeric)) qty from bundles 
                                LEFT JOIN LATERAL 
                                json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                where  bundles.isactive=true and bundles.status<=0 and bundles.fromlocationid = @locationid
                                group by schemelist ->> 'serialno',fromlocationid
                            )
                            select allstock.*,currentpo.pendingqty,currentsto.qty stoqty from allstock
                            left outer join currentpo on currentpo.uid =  allstock.serialno and currentpo.locationid=allstock.locationid
                            left outer join currentsto on currentsto.uid = allstock.serialno and currentsto.locationid=allstock.locationid";

                var param = new SortedList<string, object>();
                param.Add("locationid", list.First().fromlocationid);
                using (var reader = data.ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        string uid = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                        decimal stk = reader["stk"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stk"]);
                        decimal stv = reader["stvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stvalue"]);
                        decimal so = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                        decimal sto = reader["stoqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stoqty"]);

                        if (suppliers.ContainsKey(uid))
                        {
                            suppliers[uid].stk = stk;
                            suppliers[uid].stv = stv;
                            suppliers[uid].sto = sto;
                            suppliers[uid].so = so;
                        }
                    }
                }
                foreach (var grp in list.GroupBy(x => x.UAN))
                {
                    var required = grp.Sum(x => x.Qty);
                    if (suppliers.ContainsKey(grp.Key))
                    {
                        status.stocklist.Find(x => x.uan == grp.Key).stock = suppliers[grp.Key].stk;
                        status.stocklist.Find(x => x.uan == grp.Key).so = suppliers[grp.Key].so;
                        status.stocklist.Find(x => x.uan == grp.Key).sto = suppliers[grp.Key].sto;
                        grp.ForEach(x => x.info = suppliers[grp.Key]);
                    }
                    else
                    {
                        status.missinguan.Add(grp.Key);
                    }
                }
                status.stocklist.RemoveAll(x => x.required <= (x.stock - (x.sto + x.so)));
            }

            TaxDataService taxDataService = new TaxDataService() { Context = data.Context };
            var taxlist = taxDataService.TaxProducts(list.FindAll(x => x.info != null).ConvertAll(x => x.info.productid).ToList().Distinct().ToList());

            CompanyDataService companyDataService = new CompanyDataService() { Context = data.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());
            ReferencelistDataService referenceList = new ReferencelistDataService() { Context = data.Context };
            var locationlist = referenceList.SearchList(new ReferencelistCreteria { type = "LOCATION" });
            status.orders = new List<STOUANUploaded.Order>();
            var customercategory = referenceList.Search(new ReferencelistCreteria()
            {
                type = "CUSTOMERCATEGORY"
            });

           


            foreach (var ordergroup in list.GroupBy(x => new { x.Company, x.FromLocation, x.ToLocation, x.OrderSplit }))
            {
                STOUANUploaded.Order order = new STOUANUploaded.Order();
                order.FromLocation = ordergroup.Key.FromLocation;
                order.ToLocation = ordergroup.Key.ToLocation;
                order.Company = ordergroup.Key.Company;
                order.OrderSplit = ordergroup.Key.OrderSplit;

                order.fromlocationid = ordergroup.First().fromlocationid;
                order.tolocationid = ordergroup.First().tolocationid;
                order.fromcompanyid = ordergroup.First().fromcompanyid;
                order.tocompanyid = ordergroup.First().tocompanyid;
                order.STODate = ordergroup.First().STODate;
                order.Remark = ordergroup.First().Remark;


                decimal ccmarginmrp = 0;
                MarginRules marginRules = MarginRules.Markdown;
                if (customercategory.Exists(x => x.id == ordergroup.First().categoryid))
                {
                    var category = customercategory.Find(x => x.id == list.First().categoryid);
                    order.category = category.name;
                    if (category.additionalinfo != null &&
                        category.additionalinfo.marginmode == "mrp")
                    {
                        ccmarginmrp = category.additionalinfo.ccmarginmrp;
                        if (category.additionalinfo.markdownmode >= 0)
                        {
                            marginRules = (MarginRules)category.additionalinfo.markdownmode;
                            order.rule = marginRules.ToString();
                            order.margin = ccmarginmrp;
                        }
                    }
                }
                if (order.margin == 0)
                {
                    order.costprice = true;
                }


                order.items = new List<STOUANUploaded.OrderItem>();
                foreach (var orderitemgroup in ordergroup.GroupBy(y => y.UAN))
                {
                    decimal taxpercentage = 0;
                    decimal tax = 0;
                    long taxid = 0;
                    var orderitem = orderitemgroup.First();
                    decimal cost = 0;

                    
                    if (uancostlist.Exists(x => x.uid == orderitemgroup.Key))
                    {
                        cost = uancostlist.Find(x => x.uid == orderitemgroup.Key).cost;
                    }
                    if (cost <=0 && orderitem.info.stk > 0 && orderitem.info.stv > 0)
                    {
                        cost = Math.Round(orderitem.info.stv / orderitem.info.stk,0);
                    }
                    if (orderitem.info != null &&
                        taxlist.ContainsKey(orderitem.info.productid))
                    {
                        taxid = taxlist[orderitem.info.productid].id;
                        taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(cost).taxpercent;
                        tax = Math.Round((cost * orderitem.Qty) * taxpercentage / 100, 2);
                    }


                    if(!order.costprice)
                    {
                        taxpercentage = 0;
                        tax = 0;
                        taxid = 0;
                        if (orderitem.info != null &&
                            taxlist.ContainsKey(orderitem.info.productid))
                        {
                            taxid = taxlist[orderitem.info.productid].id;
                            taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(false, orderitem.info.mrp).taxpercent;
                            tax = Math.Round((orderitem.info.mrp * orderitem.Qty) * taxpercentage / 100, 2);
                        }
                        cost = orderitem.info == null ? 0 : orderitem.info.mrp;

                        if (orderitem.Cost > 1)
                        {
                            cost = orderitem.Cost;
                            if (orderitem.info != null &&
                            taxlist.ContainsKey(orderitem.info.productid))
                            {
                                taxid = taxlist[orderitem.info.productid].id;
                                taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(true, orderitem.Cost).taxpercent;
                                tax = Math.Round((cost * orderitem.Qty) * taxpercentage / 100, 2);
                            }
                        }
                        else
                        {
                            if (cost > 0 &&
                               ccmarginmrp > 0)
                            {
                                if (!taxlist.ContainsKey(orderitem.info.productid))
                                    throw new Exception(orderitem.info.printingname + " tax information missing");

                                var mintaxrange = taxlist[orderitem.info.productid].taxsplit.items.OrderBy(x => x.sgst + x.cgst).First();
                                cost = Math.Round(ComputeMargin(marginRules, taxpercentage, mintaxrange.cgst + mintaxrange.sgst, ccmarginmrp, cost), 2);
                                taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(true, cost).taxpercent;
                                tax = Math.Round((cost * orderitem.Qty) * taxpercentage / 100, 2);
                            }
                        }

                    }

                    order.items.Add(new STOUANUploaded.OrderItem()
                    {
                        Cost = cost,
                        HSN = orderitem.info.hsn,
                        Qty = orderitemgroup.Sum(z => z.Qty),
                        UAN = orderitem.UAN,
                        Percentage = taxpercentage,
                        Tax = tax,
                        TaxID = taxid,
                        ItemID = orderitem.info == null ? 0 : orderitem.info.id,
                        Design = orderitem.info == null ? "" : orderitem.info.glndesignid,
                        ColourID = orderitem.info == null ? 0 : orderitem.info.colourid,
                        SizeID = orderitem.info == null ? 0 : orderitem.info.sizeid,
                        MRP = orderitem.info == null ? 0 : orderitem.info.mrp,
                        Product = orderitem.info == null ? "" : orderitem.info.printingname,
                        ProductID = orderitem.info == null ? 0 : orderitem.info.productid,
                        Size = orderitem.info == null ? "" : orderitem.info.size
                    });
                }
                order.MissingUAN = order.items.FindAll(x => x.ItemID == 0).ConvertAll(x => x.UAN);
                order.Amount = order.items.Sum(x => x.Qty * x.Cost);
                order.Qty = order.items.Sum(x => x.Qty);
                order.Tax = order.items.Sum(x => x.Tax);
                if (companylist.Exists(x => x.name.ToLower() == order.Company.ToLower()))
                    order.fromcompanyid = companylist.Find(x => x.name.ToLower() == order.Company.ToLower()).id;

                if (order.items.Exists(x => status.stocklist.Exists(y => y.uan == x.UAN)))
                {
                    order.stocknotavailable = true;
                }
                status.orders.Add(order);
            }

            status.noofsto = status.orders.Count;
            status.missinguan = new List<string>();
            status.orders.ForEach(x => status.missinguan.AddRange(x.MissingUAN));
            status.missingqty = new List<string>();
            status.orders.ForEach(x => status.missingqty.AddRange(x.items.FindAll(y => y.Qty == 0).ConvertAll(z => z.UAN)));
            status.missinglocation = status.orders.FindAll(x => x.fromlocationid == 0).ConvertAll(x => x.FromLocation).Distinct().ToList();
            status.missingcompany = status.orders.FindAll(x => x.fromcompanyid == 0).ConvertAll(x => x.Company).Distinct().ToList();
            status.missingcompany.AddRange(status.orders.FindAll(x => x.tocompanyid == 0).ConvertAll(x => x.Company).Distinct().ToList());
            status.missinglocation.AddRange(status.orders.FindAll(x => x.tolocationid == 0).ConvertAll(x => x.ToLocation).Distinct().ToList());

            using (var reader = data.ExecuteReader("select code,requestcode from bundles where isactive=true and requestcode in (" +
            String.Join(",", status.orders.ConvertAll(X => "'" + X.OrderSplit + "'")) + ")", null))
            {
                while (reader.Read())
                {
                    var po = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                    var pouniquenumber = reader["requestcode"] == DBNull.Value ? "" : reader["requestcode"].ToString();
                    status.orders.FindAll(x => x.OrderSplit == pouniquenumber).ForEach(y => y.BundleCode = po);
                }
            }
            status.validsto = status.orders.Count(x => x.valid);
            return status;
        }
        /*with allprice as
            (
            select schemelist->>'uid' gln,cast(schemelist->>'mrp' as numeric) mrp,prdhsncode
                                            from mfgdesign
                                            LEFT JOIN LATERAL
                                            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                                            left outer join referencelist on referencelist.id = cast(schemelist->>'sizeid' as bigint)
                                            where mfgdesign.isactive = true 
            )

            update stock set displaymrp = allprice.mrp from allprice
            where stock.displaymrp =0  and allprice.gln = stock.serialno;

            update items set mrp = allprice.mrp from allprice
            where items.mrp =0  and allprice.gln = items.gln;
            */

        private POUANUploaded GetPOStatus(List<PurchaseOrderItem> list, ImageData data)
        {
            POUANUploaded pOStatus = new POUANUploaded();
            var supplierlist = list.ConvertAll(x => x.Supplier).ToList().Distinct().ToList();
            var uanlist = list.ConvertAll(x => x.UAN).ToList().Distinct().ToList();
            supplierlist.RemoveAll(x => string.IsNullOrEmpty(x));
            BaseData baseData = new BaseData();
            var status = new POUANUploaded();
            status.missingsuppliers = new List<string>();
            status.missinguan = new List<string>();
            if (supplierlist.Count > 0)
            {
                SortedDictionary<string, long> suppliers = new SortedDictionary<string, long>();
                string query = "select id,name from supplier where name in (" + string.Join(",", supplierlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]).ToUpper();
                        if(!suppliers.ContainsKey(name))
                            suppliers.Add(name, id);
                    }
                }
                foreach (var grp in list.GroupBy(x => x.Supplier))
                {
                    if (suppliers.ContainsKey(grp.Key.ToUpper()))
                    {
                        long id = suppliers[grp.Key.ToUpper()];
                        grp.ForEach(x => x.supplierid = id);
                    }
                    else
                    {
                        status.missingsuppliers.Add(grp.Key);
                    }
                }
            }

            if (status.missingsuppliers.Count > 0)
            {
                var missingsupplierorders = list.FindAll(x => status.missingsuppliers.Contains(x.Supplier));
                var returnorders = missingsupplierorders.FindAll(x => x.OrderType.ToLower() == "return");
                //status.missingsuppliers.RemoveAll(x => returnorders.Exists(y => y.Supplier == x));
                CustomerDataService customerDataService = new CustomerDataService() { Context = data.Context };
                SupplierDataService supplierDataService = new SupplierDataService() { Context = data.Context };
                foreach (var rorder in returnorders.ToList())
                {
                    var customer = customerDataService.Search(new CustomerCreteria()
                    {
                        name = rorder.Supplier
                    });
                    if (customer.Count > 0)
                    {
                        status.missingsuppliers.Remove(rorder.Supplier);
                        var supplier = new Supplier()
                        {
                            name = customer[0].name,
                            companylist = new List<Suppliercompany>(),
                            isactive = true,
                            createdby = data.Context.UserID,
                            modifiedby = data.Context.UserID,
                            createdon = DateTime.Now,
                            modifiedon = DateTime.Now,
                            tinanddate = customer[0].gstno,
                            gstno = customer[0].gstno,
                            internalcustomerid = customer[0].id
                        };
                        supplier.companylist.Add(new Suppliercompany()
                        {
                            name = customer[0].billingname,
                            isactive = true,
                            createdby = data.Context.UserID,
                            modifiedby = data.Context.UserID,
                            createdon = DateTime.Now,
                            modifiedon = DateTime.Now,
                            address = customer[0].address,
                            gst = customer[0].gstno,
                            stateid = customer[0].stateid,
                            cityid = customer[0].cityid,
                            countryid = customer[0].countryid
                        });
                        supplierDataService.Insert(supplier);
                        rorder.supplierid = supplier.id;
                        rorder.SupplierGST = supplier.gst;
                    }
                }
            }

            if (uanlist.Count > 0)
            {
                SortedDictionary<string, PurchaseOrderItem.Item> suppliers = new SortedDictionary<string, PurchaseOrderItem.Item>();
                string query = @"select items.glndesignid,items.printingname,items.id,items.colourid,items.sizeid,
                                items.gln,items.productid,referencelist.name sizename,items.mrp,prdhsncode from items 
                                left outer join referencelist on referencelist.id = items.sizeid
                                left outer join mfgdesign on mfgdesign.code = glndesignid
                                where items.gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and items.isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        decimal mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        string printingname = reader["printingname"] == DBNull.Value ? "" : Convert.ToString(reader["printingname"]).ToUpper();
                        string sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]).ToUpper();
                        string glndesignid = reader["glndesignid"] == DBNull.Value ? "" : Convert.ToString(reader["glndesignid"]).ToUpper();
                        string gln = reader["gln"] == DBNull.Value ? "" : Convert.ToString(reader["gln"]).ToUpper();
                        string prdhsncode = reader["prdhsncode"] == DBNull.Value ? "" : Convert.ToString(reader["prdhsncode"]).ToUpper();

                        if (!suppliers.ContainsKey(gln))
                        {
                            suppliers.Add(gln, new PurchaseOrderItem.Item()
                            {
                                id = id,
                                productid = productid,
                                printingname = printingname,
                                glndesignid = glndesignid,
                                size = sizename,
                                colourid = colourid,
                                sizeid = sizeid,
                                mrp = mrp,
                                hsn = prdhsncode
                            });
                        }
                    }
                }
                foreach (var grp in list.GroupBy(x => x.UAN))
                {
                    if (suppliers.ContainsKey(grp.Key))
                    {
                        grp.ForEach(x => x.info = suppliers[grp.Key]);
                    }
                    else
                    {
                        status.missinguan.Add(grp.Key);
                    }
                }
            }

            TaxDataService taxDataService = new TaxDataService() { Context = data.Context };
            var taxlist = taxDataService.TaxProducts(list.FindAll(x => x.info != null).ConvertAll(x => x.info.productid).ToList().Distinct().ToList());

            CompanyDataService companyDataService = new CompanyDataService() { Context = data.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());
            ReferencelistDataService referenceList = new ReferencelistDataService() { Context = data.Context };
            var locationlist = referenceList.SearchList(new ReferencelistCreteria { type = "LOCATION" });
            status.orders = new List<POUANUploaded.Order>();
            foreach (var ordergroup in list.GroupBy(x => new { x.Supplier, x.OrderSplit }))
            {
                POUANUploaded.Order order = new POUANUploaded.Order();
                order.supplierid = ordergroup.First().supplierid;
                order.SupplierGST = ordergroup.First().SupplierGST;
                order.Company = ordergroup.First().Company;
                order.Location = ordergroup.First().Location;
                order.EntryLocation = ordergroup.First().EntryLocation;
                order.Supplier = ordergroup.First().Supplier;
                order.OrderSplit = ordergroup.First().OrderSplit;
                order.DueDate = ordergroup.First().DueDate;
                order.ExpiryDate = ordergroup.First().ExpiryDate;
                order.PODate = ordergroup.First().PODate;
                order.Remark = ordergroup.First().Remark;
                order.items = new List<POUANUploaded.OrderItem>();
                if (companylist.Exists(x => x.name.ToLower() == order.Company.ToLower()))
                    order.companyid = companylist.Find(x => x.name.ToLower() == order.Company.ToLower()).id;
                if (locationlist.Exists(x => x.name.ToLower() == order.Location.ToLower()))
                    order.locationid = locationlist.Find(x => x.name.ToLower() == order.Location.ToLower()).id;
                if (locationlist.Exists(x => x.name.ToLower() == order.EntryLocation.ToLower()))
                    order.enntrylocationid = locationlist.Find(x => x.name.ToLower() == order.EntryLocation.ToLower()).id;

                if (!string.IsNullOrEmpty(ordergroup.First().OrderType) &&
                    ordergroup.First().OrderType.ToLower() == "return")
                {
                    var customercategory = referenceList.Search(new ReferencelistCreteria()
                    {
                        type = "CUSTOMERCATEGORY"
                    });

                    long categoryid = 0;
                    long customerid = 0;
                    using (var reader = data.ExecuteReader("Select internalcustomerid from supplier where id=" + order.supplierid + " and supplier.isactive=true limit 1", null))
                    {
                        if (reader.Read())
                            customerid = reader[0] == DBNull.Value ? 0 : Convert.ToInt64(reader[0]);
                    }

                    using (var reader = baseData.ExecuteReader("select id,name,categoryid from customer where customer.id = " + customerid + " and isactive=true", null))
                    {
                        if (reader.Read())
                        {
                            categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        }
                    }

                    decimal ccmarginmrp = 0;
                    MarginRules marginRules = MarginRules.Markdown;
                    if (customercategory.Exists(x => x.id == categoryid))
                    {
                        var category = customercategory.Find(x => x.id == categoryid);
                        order.category = category.name;
                        if (category.additionalinfo != null &&
                            category.additionalinfo.marginmode == "mrp")
                        {
                            ccmarginmrp = category.additionalinfo.ccmarginmrp;
                            if (category.additionalinfo.markdownmode >= 0)
                            {
                                marginRules = (MarginRules)category.additionalinfo.markdownmode;
                                order.rule = marginRules.ToString();
                                order.margin = ccmarginmrp;
                            }
                        }
                    }

                    ordergroup.ToList().FindAll(x => x.Cost < 1).ForEach(x =>
                    {
                        var price = x.info.mrp;
                        if (ccmarginmrp > 0)
                        {
                            var mintaxrange = taxlist[x.info.productid].taxsplit.items.OrderBy(y => y.sgst + y.cgst).First();
                            var taxpercentage = taxlist[x.info.productid].taxsplit.Find(price).taxpercent;
                            x.Cost = Math.Round(ComputeMargin(marginRules, taxpercentage, mintaxrange.cgst + mintaxrange.sgst, ccmarginmrp, price), 2);
                        }
                    });

                    //var missingpriceuids = ordergroup.ToList().FindAll(x => x.Cost <= 1).ConvertAll(x => "'" + x.UAN + "'");
                    //if (missingpriceuids != null &&
                    //    missingpriceuids.Count > 0)
                    //{
                    //    SortedDictionary<string, decimal> uidprices = new SortedDictionary<string, decimal>();
                    //    string query = @"select stock.serialno,sum(iqty-(oqty+dqty+jqty)) qty,sum((iqty-(oqty+dqty+jqty))*buyingprice) value from stock 
                    //    inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid = {0}
                    //    where stock.serialno in ({1}) and stock.isactive=true and salablegoods.isactive=true and iqty-(oqty+dqty+jqty)>0
                    //    group by stock.serialno";
                    //    query = string.Format(query, string.Join(",", missingpriceuids), order.locationid);
                    //    using (var reader = data.ExecuteReader(query, null))
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            var qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    //            var value = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                    //            var uid = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                    //            if (qty > 0 && value > 0)
                    //            {
                    //                uidprices.Add(uid, value / qty);
                    //            }
                    //        }
                    //    }
                    //}

                }

                foreach (var orderitemgroup in ordergroup.GroupBy(y => y.UAN))
                {
                    decimal taxpercentage = 0;
                    decimal tax = 0;
                    long taxid = 0;
                    var orderitem = orderitemgroup.First();
                    if (orderitem.info != null &&
                        taxlist.ContainsKey(orderitem.info.productid))
                    {
                        taxid = taxlist[orderitem.info.productid].id;
                        taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(orderitem.Cost).taxpercent;
                        tax = Math.Round((orderitem.Cost * orderitem.Qty) * taxpercentage / 100, 2);
                    }
                    order.items.Add(new POUANUploaded.OrderItem()
                    {
                        Cost = orderitem.Cost,
                        Tolerance = orderitem.Tolerance,
                        HSN = (string.IsNullOrEmpty(orderitem.HSN) && orderitem.info != null) ? orderitem.info.hsn : orderitem.HSN,
                        Qty = orderitemgroup.Sum(z => z.Qty),
                        UAN = orderitem.UAN,
                        Percentage = taxpercentage,
                        Tax = tax,
                        TaxID = taxid,
                        ItemID = orderitem.info == null ? 0 : orderitem.info.id,
                        Design = orderitem.info == null ? "" : orderitem.info.glndesignid,
                        ColourID = orderitem.info == null ? 0 : orderitem.info.colourid,
                        SizeID = orderitem.info == null ? 0 : orderitem.info.sizeid,
                        MRP = orderitem.info == null ? 0 : orderitem.info.mrp,
                        Product = orderitem.info == null ? "" : orderitem.info.printingname,
                        ProductID = orderitem.info == null ? 0 : orderitem.info.productid,
                        Size = orderitem.info == null ? "" : orderitem.info.size
                    });
                }

                order.MissingUAN = order.items.FindAll(x => x.ItemID == 0).ConvertAll(x => x.UAN);
                order.Amount = order.items.Sum(x => x.Qty * x.Cost);
                order.Qty = order.items.Sum(x => x.Qty);
                order.Tax = order.items.Sum(x => x.Tax);
                status.orders.Add(order);
            }


            if (status.orders.Exists(x => x.items.Exists(y => y.MRP < 1)))
            {
                uanlist.Clear();
                List<string> itemmpupdate = new List<string>();
                status.orders.ForEach(x => uanlist.AddRange(x.items.FindAll(y => y.MRP < 1).ConvertAll(z => z.UAN)));
                var query = @"select schemelist->>'uid' gln,cast(schemelist->>'mrp' as numeric) mrp,prdhsncode
                                from mfgdesign
                                LEFT JOIN LATERAL
                                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                                left outer join referencelist on referencelist.id = cast(schemelist->>'sizeid' as bigint)
                                where mfgdesign.isactive = true and 
                                schemelist->>'uid' in (" + string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ")";
                using (var reader = data.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        string uan = reader.GetString(0);
                        decimal mrp = reader.GetDecimal(1);
                        itemmpupdate.Add(string.Format("update items set mrp={0} where gln='{1}';\r\n", mrp, uan));
                        status.orders.ForEach(x =>
                            {
                                x.items.FindAll(y => y.UAN == uan).ForEach(z => z.MRP = mrp);
                            }
                        );
                    }
                }
                uanlist.Clear();
                status.orders.ForEach(x => uanlist.AddRange(x.items.FindAll(y => y.MRP < 1).ConvertAll(z => z.UAN)));
                if (uanlist.Count > 0)
                {
                    throw new Exception("Please update the MRP for these UAN " + string.Join(",", uanlist));
                }
                else
                {
                    itemmpupdate.ForEach(x =>
                    {
                        data.ExecuteQuery(x, null);
                    });
                }
            }

            status.noofpo = status.orders.Count;
            status.missinguan = new List<string>();
            status.orders.ForEach(x => status.missinguan.AddRange(x.MissingUAN));
            status.missingqty = new List<string>();
            status.orders.ForEach(x => status.missingqty.AddRange(x.items.FindAll(y => y.Qty == 0).ConvertAll(z => z.UAN)));
            status.validpo = status.orders.Count(x => x.valid);
            status.missingsuppliers = status.orders.FindAll(x => x.supplierid == 0).ConvertAll(x => x.Supplier).Distinct().ToList();
            status.missingcompany = status.orders.FindAll(x => x.companyid == 0).ConvertAll(x => x.Company).Distinct().ToList();
            status.missinglocation = status.orders.FindAll(x => x.locationid == 0).ConvertAll(x => x.Location).Distinct().ToList();


            using (var reader = data.ExecuteReader("select pono,pouniquenumber from purchaseorder where isactive=true and pouniquenumber in (" +
            String.Join(",", status.orders.ConvertAll(X => "'" + X.OrderSplit + "'")) + ")", null))
            {
                while (reader.Read())
                {
                    var po = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                    var pouniquenumber = reader["pouniquenumber"] == DBNull.Value ? "" : reader["pouniquenumber"].ToString();
                    status.orders.FindAll(x => x.OrderSplit == pouniquenumber).ForEach(y => y.PONo = po);
                }
            }
            return status;
        }

        public List<PurchaseOrderItem> ProcessPO(Stream source, string columnmappings)
        {
            var excelheader = new List<string>();
            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var columnindex = new SortedDictionary<string, int>();
            var cols = columnmappings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
            cols.ForEach(x =>
            {
                var parts = x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                columnmap.Add(parts[1].Trim().ToLower(), new MigrationManager.ColumnMap()
                {
                    columnname = parts[1].Trim(),
                    columnmap = parts[0].Trim()
                });
            });


            using (source)
            {
                ExcelPackage package = new ExcelPackage(source);
                var worksheet = package.Workbook.Worksheets[1];
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
                            columnsheaders.Add(scellvalue);
                        }
                        else
                        {
                            if (cellvalue != null &&
                               typeof(DateTime) == cellvalue.GetType())
                            {
                                rowvalues.Add(cellvalue != null ? ((DateTime)cellvalue).ToString("yyyy-MM-dd") : "");
                            }
                            else
                            {
                                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                            }
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        if (!rowvalues.All(x => string.IsNullOrEmpty(x)))
                            rows.Add(rowvalues);
                    }
                }
            }

            foreach (var item in columnmap)
            {
                item.Value.columnmap = item.Value.columnmap != null ? item.Value.columnmap.ToLower() : "";
                item.Value.index = columnsheaders.Contains(item.Value.columnmap) ? (columnsheaders.IndexOf(item.Value.columnmap)) : -1;
            }

            List<PurchaseOrderItem> lrInvoiceItems = new List<PurchaseOrderItem>();
            foreach (var item in rows)
            {
                lrInvoiceItems.Add(new PurchaseOrderItem(item, columnmap));
            }

            return lrInvoiceItems;
        }

        public List<ECommerceOrderItem> ProcessECOM(Stream source, string columnmappings)
        {
            var excelheader = new List<string>();
            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var columnindex = new SortedDictionary<string, int>();
            var cols = columnmappings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
            cols.ForEach(x =>
            {
                var parts = x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                columnmap.Add(parts[1].Trim().ToLower(), new MigrationManager.ColumnMap()
                {
                    columnname = parts[1].Trim(),
                    columnmap = parts[0].Trim()
                });
            });


            using (source)
            {
                ExcelPackage package = new ExcelPackage(source);
                var worksheet = package.Workbook.Worksheets[1];
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
                            columnsheaders.Add(scellvalue);
                        }
                        else
                        {
                            if (cellvalue != null &&
                               typeof(DateTime) == cellvalue.GetType())
                            {
                                rowvalues.Add(cellvalue != null ? ((DateTime)cellvalue).ToString("yyyy-MM-dd") : "");
                            }
                            else
                            {
                                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                            }
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        if (!rowvalues.All(x => string.IsNullOrEmpty(x)))
                            rows.Add(rowvalues);
                    }
                }
            }

            foreach (var item in columnmap)
            {
                item.Value.columnmap = item.Value.columnmap != null ? item.Value.columnmap.ToLower() : "";
                item.Value.index = columnsheaders.Contains(item.Value.columnmap) ? (columnsheaders.IndexOf(item.Value.columnmap)) : -1;
            }

            List<ECommerceOrderItem> lrInvoiceItems = new List<ECommerceOrderItem>();
            foreach (var item in rows)
            {
                lrInvoiceItems.Add(new ECommerceOrderItem(item, columnmap));
            }

            return lrInvoiceItems;
        }

        public List<StockOrderItem> ProcessSTO(Stream source, string columnmappings)
        {
            var excelheader = new List<string>();
            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var columnindex = new SortedDictionary<string, int>();
            var cols = columnmappings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
            cols.ForEach(x =>
            {
                var parts = x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                columnmap.Add(parts[1].Trim().ToLower(), new MigrationManager.ColumnMap()
                {
                    columnname = parts[1].Trim(),
                    columnmap = parts[0].Trim()
                });
            });


            using (source)
            {
                ExcelPackage package = new ExcelPackage(source);
                var worksheet = package.Workbook.Worksheets[1];
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
                            columnsheaders.Add(scellvalue);
                        }
                        else
                        {
                            if (cellvalue != null &&
                               typeof(DateTime) == cellvalue.GetType())
                            {
                                rowvalues.Add(cellvalue != null ? ((DateTime)cellvalue).ToString("yyyy-MM-dd") : "");
                            }
                            else
                            {
                                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                            }
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        if (!rowvalues.All(x => string.IsNullOrEmpty(x)))
                            rows.Add(rowvalues);
                    }
                }
            }

            foreach (var item in columnmap)
            {
                item.Value.columnmap = item.Value.columnmap != null ? item.Value.columnmap.ToLower() : "";
                item.Value.index = columnsheaders.Contains(item.Value.columnmap) ? (columnsheaders.IndexOf(item.Value.columnmap)) : -1;
            }

            List<StockOrderItem> lrInvoiceItems = new List<StockOrderItem>();
            foreach (var item in rows)
            {
                lrInvoiceItems.Add(new StockOrderItem(item, columnmap));
            }

            return lrInvoiceItems;
        }

        private SOUANUploaded GetSOStatus(List<SalesOrderItem> list, ImageData data)
        {

            var supplierlist = list.ConvertAll(x => x.Customer).ToList().Distinct().ToList();
            var uanlist = list.ConvertAll(x => x.UAN).ToList().Distinct().ToList();
            supplierlist.RemoveAll(x => string.IsNullOrEmpty(x));
            BaseData baseData = new BaseData();
            var status = new SOUANUploaded();
            status.missingcustomers = new List<string>();
            status.missinguan = new List<string>();
            if (supplierlist.Count > 0)
            {
                SortedDictionary<string, Tuple<long, long>> suppliers = new SortedDictionary<string, Tuple<long, long>>();
                string query = "select id,name,categoryid from customer where name in (" + string.Join(",", supplierlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        string name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]).ToUpper();
                        long categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        suppliers.Add(name, new Tuple<long, long>(id, categoryid));
                    }
                }
                foreach (var grp in list.GroupBy(x => x.Customer))
                {
                    if (suppliers.ContainsKey(grp.Key.ToUpper()))
                    {
                        long id = suppliers[grp.Key.ToUpper()].Item1;
                        long catid = suppliers[grp.Key.ToUpper()].Item2;
                        grp.ForEach(x =>
                        {
                            x.customerid = id;
                            x.categoryid = catid;
                        });
                    }
                    else
                    {
                        status.missingcustomers.Add(grp.Key);
                    }
                }
            }
            if (uanlist.Count > 0)
            {
                SortedDictionary<string, SalesOrderItem.Item> suppliers = new SortedDictionary<string, SalesOrderItem.Item>();
                string query = @"select items.glndesignid,items.printingname,items.id,items.colourid,items.sizeid,
                                items.gln,items.productid,referencelist.name sizename,items.mrp,prdhsncode from items 
                                left outer join referencelist on referencelist.id = items.sizeid
                                left outer join mfgdesign on mfgdesign.code = glndesignid
                                where items.gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x) + "'")) + ") and items.isactive=true";
                using (var reader = baseData.ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        decimal mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        string printingname = reader["printingname"] == DBNull.Value ? "" : Convert.ToString(reader["printingname"]).ToUpper();
                        string sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]).ToUpper();
                        string glndesignid = reader["glndesignid"] == DBNull.Value ? "" : Convert.ToString(reader["glndesignid"]).ToUpper();
                        string gln = reader["gln"] == DBNull.Value ? "" : Convert.ToString(reader["gln"]).ToUpper();
                        string hsn = reader["prdhsncode"] == DBNull.Value ? "" : Convert.ToString(reader["prdhsncode"]).ToUpper();
                        if (!suppliers.ContainsKey(gln))
                        {
                            suppliers.Add(gln, new SalesOrderItem.Item()
                            {
                                id = id,
                                productid = productid,
                                printingname = printingname,
                                glndesignid = glndesignid,
                                size = sizename,
                                colourid = colourid,
                                sizeid = sizeid,
                                mrp = mrp,
                                hsn = hsn
                            });
                        }
                    }
                }
                foreach (var grp in list.GroupBy(x => x.UAN))
                {
                    if (suppliers.ContainsKey(grp.Key))
                    {
                        grp.ForEach(x => x.info = suppliers[grp.Key]);
                    }
                    else
                    {
                        status.missinguan.Add(grp.Key);
                    }
                }
            }

            TaxDataService taxDataService = new TaxDataService() { Context = data.Context };
            var taxlist = taxDataService.TaxProducts(list.FindAll(x => x.info != null).ConvertAll(x => x.info.productid).ToList().Distinct().ToList());
            CompanyDataService companyDataService = new CompanyDataService() { Context = data.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());
            ReferencelistDataService referenceList = new ReferencelistDataService() { Context = data.Context };
            var locationlist = referenceList.SearchList(new ReferencelistCreteria { type = "LOCATION" });
            CustomerDataService customerDataService = new CustomerDataService() { Context = data.Context };
            var customercategory = referenceList.Search(new ReferencelistCreteria()
            {
                type = "CUSTOMERCATEGORY"
            });
            status.orders = new List<SOUANUploaded.Order>();
            foreach (var ordergroup in list.GroupBy(x => new { x.Customer, x.OrderSplit }))
            {
                SOUANUploaded.Order order = new SOUANUploaded.Order();
                order.customerid = ordergroup.First().customerid;
                order.CustomerGST = ordergroup.First().CustomerGST;
                order.Location = ordergroup.First().Location;
                order.Customer = ordergroup.First().Customer;
                order.Company = ordergroup.First().Company;
                order.OrderSplit = ordergroup.First().OrderSplit;
                order.BookingNo = ordergroup.First().BookingNo;
                order.Tolerance = ordergroup.First().Tolerance;
                order.DueDate = ordergroup.First().DueDate;
                order.ExpiryDate = ordergroup.First().ExpiryDate;
                order.SODate = ordergroup.First().SODate;
                order.Remark = ordergroup.First().Remark;
                order.OrderType = ordergroup.First().OrderType;

                order.items = new List<SOUANUploaded.OrderItem>();
                if (companylist.Exists(x => x.name.ToLower() == order.Company.ToLower()))
                    order.companyid = companylist.Find(x => x.name.ToLower() == order.Company.ToLower()).id;
                if (locationlist.Exists(x => x.name.ToLower() == order.Location.ToLower()))
                    order.locationid = locationlist.Find(x => x.name.ToLower() == order.Location.ToLower()).id;

                decimal ccmarginmrp = 0;
                MarginRules marginRules = MarginRules.Markdown;
                if (customercategory.Exists(x => x.id == ordergroup.First().categoryid))
                {
                    var category = customercategory.Find(x => x.id == ordergroup.First().categoryid);
                    order.category = category.name;
                    if (category.additionalinfo != null &&
                        category.additionalinfo.marginmode == "mrp")
                    {
                        ccmarginmrp = category.additionalinfo.ccmarginmrp;
                        if (category.additionalinfo.markdownmode >= 0)
                        {
                            marginRules = (MarginRules)category.additionalinfo.markdownmode;
                            order.rule = marginRules.ToString();
                            order.margin = ccmarginmrp;
                        }
                    }
                }
                if (order.margin == 0)
                {
                    order.manualprice = ordergroup.All(x => x.Price > 1);
                }
                order.MissingPrice = new List<string>();
                foreach (var orditem in ordergroup.GroupBy(x => x.UAN))
                {
                    decimal taxpercentage = 0;
                    decimal tax = 0;
                    var orderitem = orditem.First();
                    long taxid = 0;
                    if (orderitem.info != null &&
                        taxlist.ContainsKey(orderitem.info.productid))
                    {
                        taxid = taxlist[orderitem.info.productid].id;
                        taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(false, orderitem.info.mrp).taxpercent;
                        tax = Math.Round((orderitem.info.mrp * orderitem.Qty) * taxpercentage / 100, 2);
                    }
                    decimal price = orderitem.info == null ? 0 : orderitem.info.mrp;

                    if (orderitem.Price > 1)
                    {
                        price = orderitem.Price;
                        if (orderitem.info != null &&
                        taxlist.ContainsKey(orderitem.info.productid))
                        {
                            taxid = taxlist[orderitem.info.productid].id;
                            taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(true, orderitem.Price).taxpercent;
                            tax = Math.Round((price * orderitem.Qty) * taxpercentage / 100, 2);
                        }
                    }
                    else
                    {
                        if (price > 0 &&
                           ccmarginmrp > 0)
                        {
                            if (!taxlist.ContainsKey(orderitem.info.productid))
                                throw new Exception(orderitem.info.printingname + " tax information missing");

                            var mintaxrange = taxlist[orderitem.info.productid].taxsplit.items.OrderBy(x => x.sgst + x.cgst).First();
                            price = Math.Round(ComputeMargin(marginRules, taxpercentage, mintaxrange.cgst + mintaxrange.sgst, ccmarginmrp, price), 2);
                            taxpercentage = taxlist[orderitem.info.productid].taxsplit.Find(true, price).taxpercent;
                            tax = Math.Round((price * orderitem.Qty) * taxpercentage / 100, 2);
                        }
                    }

                    if (price == 0)
                        order.MissingPrice.Add(orderitem.UAN);
                    order.items.Add(new SOUANUploaded.OrderItem()
                    {
                        HSN = (string.IsNullOrEmpty(orderitem.HSN) && orderitem.info != null) ? orderitem.info.hsn : orderitem.HSN,
                        Qty = orditem.Sum(x => x.Qty),
                        UAN = orderitem.UAN,
                        Percentage = taxpercentage,
                        Price = price,
                        Tax = tax,
                        TaxID = taxid,
                        ItemID = orderitem.info == null ? 0 : orderitem.info.id,
                        Design = orderitem.info == null ? "" : orderitem.info.glndesignid,
                        ColourID = orderitem.info == null ? 0 : orderitem.info.colourid,
                        SizeID = orderitem.info == null ? 0 : orderitem.info.sizeid,
                        MRP = orderitem.info == null ? 0 : orderitem.info.mrp,
                        Product = orderitem.info == null ? "" : orderitem.info.printingname,
                        ProductID = orderitem.info == null ? 0 : orderitem.info.productid,
                        Size = orderitem.info == null ? "" : orderitem.info.size,
                    });
                }
                order.MissingUAN = order.items.FindAll(x => x.ItemID == 0).ConvertAll(x => x.UAN);
                order.MissingPrice = order.items.FindAll(x => x.Price == 0).ConvertAll(x => x.UAN);
                order.Amount = order.items.Sum(x => x.Qty * x.Cost);
                order.Qty = order.items.Sum(x => x.Qty);
                order.Tax = order.items.Sum(x => x.Tax);
                status.orders.Add(order);
            }

            using (var reader = data.ExecuteReader("select sono,sobookno from salesorder where isactive=true and sobookno in (" +
            String.Join(",", status.orders.ConvertAll(X => "'" + X.OrderSplit + "'")) + ")", null))
            {
                while (reader.Read())
                {
                    var po = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                    var pouniquenumber = reader["sobookno"] == DBNull.Value ? "" : reader["sobookno"].ToString();
                    status.orders.FindAll(x => x.OrderSplit == pouniquenumber).ForEach(y => y.SONo = po);
                }
            }

            status.noofso = status.orders.Count;
            status.missinguan = new List<string>();
            status.orders.ForEach(x => status.missinguan.AddRange(x.MissingUAN));
            status.validso = status.orders.Count(x => x.customerid > 0 && x.MissingUAN.Count == 0 && x.MissingPrice.Count == 0);
            status.missingcustomers = status.orders.FindAll(x => x.customerid == 0).ConvertAll(x => x.Customer).ToList();
            status.missingmargin = status.orders.FindAll(x => x.margin <= 0 && !x.manualprice).ConvertAll(x => x.Customer).ToList();
            UpdateStock(status, data);
            return status;
        }

        public void UpdateStock(SOUANUploaded status, ImageData imagedata)
        {
            var locationid = status.orders[0].locationid;
            List<SOUANUploaded.StockInfo> stockInfos = new List<SOUANUploaded.StockInfo>();
            status.orders.ForEach(x =>
            {
                if (!x.OrderType.ToLower().Contains("return") && string.IsNullOrEmpty(x.SONo))
                    stockInfos.AddRange(x.items.ConvertAll(y => new SOUANUploaded.StockInfo() { uan = y.UAN, required = y.Qty }));
            });

            if (stockInfos.Count == 0) return;

            stockInfos = stockInfos.GroupBy(x => x.uan).ToList().ConvertAll(x => new SOUANUploaded.StockInfo() { uan = x.Key,
                required = x.Sum(y => y.required) });
            string query = @"with allstock as 
                            (
                                 select sum(iqty-(oqty+jqty+dqty)) stk,serialno,salablegoods.locationid from salablegoods 
                                 inner join stock on stock.id = stockid and stock.isactive=true
                                 where salablegoods.isactive=true 
                                 and salablegoods.locationid = @locationid
                                 and serialno in (" + string.Join(",", stockInfos.ConvertAll(x => "'" + x.uan + "'")) + @") 
                            group by stock.serialno,salablegoods.locationid
                            ),currentpo as
                            (
                                select sum(oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)) pendingqty,
                                Salesorder.locationid,Salesorderitems.uid
                                from Salesorder 
                                inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id
                                inner join allstock on allstock.serialno = Salesorderitems.uid and Salesorder.locationid=allstock.locationid
                                where oqty::numeric-(bqty::numeric+cqty::numeric+rqty::numeric)>0 and 
                                salesorder.isactive=true and Salesorderitems.isactive=true  and Salesorder.sosourcetype=0 
                                group by Salesorderitems.uid,Salesorder.locationid
                            ),
                            currentsto as
                            (
                                select 
                                schemelist ->> 'serialno' uid,fromlocationid locationid,
                                sum(cast(schemelist ->> 'iqty' as numeric)) qty from bundles 
                                LEFT JOIN LATERAL 
                                json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                                where  bundles.isactive=true and bundles.status<=0 and bundles.fromlocationid = @locationid
                                group by schemelist ->> 'serialno',fromlocationid
                            )
                            select allstock.*,currentpo.pendingqty,currentsto.qty stoqty from allstock
                            left outer join currentpo on currentpo.uid =  allstock.serialno and currentpo.locationid=allstock.locationid
                            left outer join currentsto on currentsto.uid = allstock.serialno and currentsto.locationid=allstock.locationid";


            var param = new SortedList<string, object>();
            param.Add("locationid", locationid);
            using (var reader = imagedata.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var stk = reader["stk"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stk"]);
                    var so = reader["pendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pendingqty"]);
                    var sto = reader["stoqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stoqty"]);
                    var uid = reader["serialno"] == DBNull.Value ? "" : Convert.ToString(reader["serialno"]);
                    stockInfos.Find(x => x.uan == uid).stock = stk;
                    stockInfos.Find(x => x.uan == uid).so = so;
                    stockInfos.Find(x => x.uan == uid).sto = sto;
                }
            }

            stockInfos.RemoveAll(x => x.required <= (x.stock - (x.so + x.sto)));
            if (stockInfos.Count > 0)
            {
                status.stocklist = stockInfos;
                status.orders.ForEach(x =>
                {
                    var conflictstock = stockInfos.FindAll(z => x.items.Exists(y => y.UAN == z.uan) 
                    && string.IsNullOrEmpty(x.SONo) && !x.OrderType.ToLower().Contains("return"));
                    if (conflictstock.Count > 0)
                    {
                        x.nostock = true;
                        conflictstock.ForEach(y =>
                        {
                            if (y.orders == null)
                                y.orders = new List<string>();
                            y.orders.Add(x.OrderSplit);
                        });
                    }
                });
            }
        }

        //public decimal ComputeMargin(MarginRules rule, decimal taxpercentage, decimal margin, decimal mrp)
        //{
        //    switch (rule)
        //    {
        //        case MarginRules.Markdown:
        //            return mrp - (mrp * margin / 100);
        //        case MarginRules.Markdown_NetOff:
        //            var marginvalue = (mrp * margin / 100);
        //            var taxvalue = (mrp * (taxpercentage / 100)) / (1 + (taxpercentage / 100));
        //            return mrp - marginvalue - taxvalue;
        //        case MarginRules.Markdown_ExcludeTax:
        //            var rrp = mrp / (1 + (taxpercentage / 100));
        //            var exmarginvalue = rrp * margin / 100;
        //            return rrp - exmarginvalue;
        //    }
        //    return 0;
        //}

        public decimal ComputeMargin(MarginRules rule,
           decimal taxpercentage,
           decimal mintaxpercentage,
           decimal margin, decimal mrp)
        {
            switch (rule)
            {
                case MarginRules.Markdown:
                    return mrp - (mrp * margin / 100);				
                    /*var mrpinclusive = mrp - (mrp * margin / 100);
                    return mrpinclusive / (100 + taxpercentage) * 100;*/
                case MarginRules.Markdown_NetOff:
                    var marginvalue = (mrp * margin / 100);
                    var taxvalue = (mrp * (taxpercentage / 100)) / (1 + (taxpercentage / 100));
                    return mrp - marginvalue - taxvalue;
                case MarginRules.Markdown_ExcludeTax:
                    var rrp = mrp / (1 + (taxpercentage / 100));
                    var exmarginvalue = rrp * margin / 100;
                    return rrp - exmarginvalue;
                case MarginRules.Markdown_ExtraMargin:
                    if (taxpercentage > mintaxpercentage)
                    {
                        margin += (taxpercentage - mintaxpercentage);
                    }
                    return mrp - (mrp * margin / 100);
                //TONIQ
                case MarginRules.Markdown_IncludeTax:
                    var toniqrrp = mrp / (1 + (taxpercentage / 100));
                    var wholemargin = mrp * margin / 100;
                    var excludeprice = toniqrrp - wholemargin;
                    var includeprice = excludeprice + (excludeprice * taxpercentage / 100);
                    return includeprice;
            }
            return 0;
        }

        public enum MarginRules
        {
            Markdown,
            Markdown_NetOff,
            Markdown_ExcludeTax,
            Markdown_ExtraMargin,
            Markdown_IncludeTax
        }
        public List<SalesOrderItem> ProcessSO(Stream source, string columnmappings)
        {
            var excelheader = new List<string>();
            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var columnindex = new SortedDictionary<string, int>();
            var cols = columnmappings.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();
            cols.ForEach(x =>
            {
                var parts = x.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                columnmap.Add(parts[1].Trim().ToLower(), new MigrationManager.ColumnMap()
                {
                    columnname = parts[1].Trim(),
                    columnmap = parts[0].Trim()
                });
            });


            using (source)
            {
                ExcelPackage package = new ExcelPackage(source);
                var worksheet = package.Workbook.Worksheets[1];
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
                            columnsheaders.Add(scellvalue);
                        }
                        else
                        {
                            if (cellvalue != null &&
                                typeof(DateTime) == cellvalue.GetType())
                            {
                                rowvalues.Add(cellvalue != null ? ((DateTime)cellvalue).ToString("yyyy-MM-dd") : "");
                            }
                            else
                            {
                                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                            }
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        if (!rowvalues.All(x => string.IsNullOrEmpty(x)))
                            rows.Add(rowvalues);
                    }
                }
            }

            foreach (var item in columnmap)
            {
                item.Value.columnmap = item.Value.columnmap != null ? item.Value.columnmap.ToLower() : "";
                item.Value.index = columnsheaders.Contains(item.Value.columnmap) ? (columnsheaders.IndexOf(item.Value.columnmap)) : -1;
            }

            List<SalesOrderItem> lrInvoiceItems = new List<SalesOrderItem>();
            foreach (var item in rows)
            {
                lrInvoiceItems.Add(new SalesOrderItem(item, columnmap));
            }

            return lrInvoiceItems;
        }

        public DocumentStatus ValidateSKU(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            Exception processException = null;
            DocumentStatus result = new DocumentStatus() { sostatus = new SOUANUploaded() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var billimport = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "BILLIMPORT",
                        referenceid = 4
                    });
                    if (billimport.Count != 1)
                    {
                        throw new Exception("Please configure one SKU By UAN type Document Import");
                    }

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
                                MfgDesingImport mfgDesingImport = new MfgDesingImport();
                                result.skustatus = mfgDesingImport.Process(data.Context, buffer, billimport[0].additionalinfo.formula);
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            processException = exp;
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    processException = exp;
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                return result;
            }
            catch (Exception exp)
            {
                processException = exp;
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            if (processException != null)
                throw processException;
            return result;
        }

        public DocumentStatus ValidateSO(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            Exception processException = null;
            DocumentStatus result = new DocumentStatus() { sostatus = new SOUANUploaded() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var billimport = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "BILLIMPORT",
                        referenceid = 1
                    });
                    if (billimport.Count != 1)
                    {
                        throw new Exception("Please configure one Sales Order By UAN type Document Import");
                    }

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


                                var list = ProcessSO(stream, billimport[0].additionalinfo.formula);
                                result.sostatus = GetSOStatus(list, data);

                                //string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                //FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                                //Common.ImageData.File file = new Common.ImageData.File();
                                //file.createdby = data.Context.UserID;
                                //file.createdon = DateTime.Now;
                                //file.data = buffer;
                                //file.ext = fi.Extension;
                                //file.filename = fi.Name;
                                //file.isactive = true;
                                //file.modifiedby = data.Context.UserID;
                                //file.modifiedon = DateTime.Now;
                                //file.version = 1;
                                //file.type = "poimport";
                                //file = data.Insert(file);
                                //result.filename = file.filename;
                                //result.fileid = file.id;
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            processException = exp;
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    processException = exp;
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                return result;
            }
            catch (Exception exp)
            {
                processException = exp;
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            if (processException != null)
                throw processException;
            return result;
        }

        public DocumentStatus ValidatePO(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            DocumentStatus result = new DocumentStatus() { postatus = new POUANUploaded() };
            Exception processException = null;
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var billimport = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "BILLIMPORT",
                        referenceid = 3
                    });
                    if (billimport.Count != 1)
                    {
                        throw new Exception("Please configure one Purchase Order By UAN type Document Import");
                    }

                    
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


                                var list = ProcessPO(stream, billimport[0].additionalinfo.formula);
                                result.postatus = GetPOStatus(list, data);

                                //string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                //FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                                //Common.ImageData.File file = new Common.ImageData.File();
                                //file.createdby = data.Context.UserID;
                                //file.createdon = DateTime.Now;
                                //file.data = buffer;
                                //file.ext = fi.Extension;
                                //file.filename = fi.Name;
                                //file.isactive = true;
                                //file.modifiedby = data.Context.UserID;
                                //file.modifiedon = DateTime.Now;
                                //file.version = 1;
                                //file.type = "poimport";
                                //file = data.Insert(file);
                                //result.filename = file.filename;
                                //result.fileid = file.id;
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            processException = exp;
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    processException = exp;
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                if (processException !=null)
                    throw processException;
                return result;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return result;
        }

        public DocumentStatus ValidateECOM(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            Exception processException = null;
            DocumentStatus result = new DocumentStatus() { stostatus = new STOUANUploaded() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var billimport = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "BILLIMPORT",
                        referenceid = 6
                    });
                    if (billimport.Count != 1)
                    {
                        throw new Exception("Please configure one Stock Order By UAN type Document Import");
                    }

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


                                var list = ProcessECOM(stream, billimport[0].additionalinfo.formula);
                                result.ecomstatus = GetECOStatus(list, data);

                                //string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                //FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                                //Common.ImageData.File file = new Common.ImageData.File();
                                //file.createdby = data.Context.UserID;
                                //file.createdon = DateTime.Now;
                                //file.data = buffer;
                                //file.ext = fi.Extension;
                                //file.filename = fi.Name;
                                //file.isactive = true;
                                //file.modifiedby = data.Context.UserID;
                                //file.modifiedon = DateTime.Now;
                                //file.version = 1;
                                //file.type = "poimport";
                                //file = data.Insert(file);
                                //result.filename = file.filename;
                                //result.fileid = file.id;
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            processException = exp;
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    processException = exp;
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
				if (processException != null)
                    throw processException;
                return result;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
			if (processException != null)
            	throw processException;			
            return result;
        }

        public DocumentStatus ValidateSTO(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            Exception processException = null;
            DocumentStatus result = new DocumentStatus() { stostatus = new STOUANUploaded() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    var billimport = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "BILLIMPORT",
                        referenceid = 5
                    });
                    if (billimport.Count != 1)
                    {
                        throw new Exception("Please configure one Stock Order By UAN type Document Import");
                    }

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


                                var list = ProcessSTO(stream, billimport[0].additionalinfo.formula);
                                result.stostatus = GetSTOStatus(list, data);

                                //string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                //FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                                //Common.ImageData.File file = new Common.ImageData.File();
                                //file.createdby = data.Context.UserID;
                                //file.createdon = DateTime.Now;
                                //file.data = buffer;
                                //file.ext = fi.Extension;
                                //file.filename = fi.Name;
                                //file.isactive = true;
                                //file.modifiedby = data.Context.UserID;
                                //file.modifiedon = DateTime.Now;
                                //file.version = 1;
                                //file.type = "poimport";
                                //file = data.Insert(file);
                                //result.filename = file.filename;
                                //result.fileid = file.id;
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            processException = exp;
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    processException = exp;
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                return result;
            } 
            catch (Exception exp)
            {
                processException = exp;
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            if (processException != null)
                throw processException;
            return result;
        }

        public BillSummary Validate(HttpRequestMessage Request, long type, Common.ImageData data)
        {
            BillSummary result = new BillSummary() { Status = new AgreementStatus() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
                try
                {
                    var columnnames = @"Product,HSN,Agreementname,Agreementon,ItemCode,Cost,Price,MRP,UAN,Brand,ItemName,Size,Colour,Style,Pattern,Material,Type,Sleeve,Fit".Split(',');
                    result.columns = columnnames;

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


                                var list = Process(stream, columnnames);

                                result.Status = GetStatus(list, data);

                                string filename = content.Headers.ContentDisposition.FileName.Replace("\"", "").ToLower();
                                FileInfo fi = new FileInfo(content.Headers.ContentDisposition.FileName.Replace("\"", ""));
                                Common.ImageData.File file = new Common.ImageData.File();
                                file.createdby = data.Context.UserID;
                                file.createdon = DateTime.Now;
                                file.data = buffer;
                                file.ext = fi.Extension;
                                file.filename = fi.Name;
                                file.isactive = true;
                                file.modifiedby = data.Context.UserID;
                                file.modifiedon = DateTime.Now;
                                file.version = 1;
                                file.type = "tragrimp";
                                file = data.Insert(file);
                                result.filename = file.filename;
                                result.fileid = file.id;
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                return result;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return result;
        }

        public AgreementStatus GetStatus(List<AgreementProductItem> list, Common.ImageData data)
        {

            var uanlist = list.FindAll(x => !string.IsNullOrEmpty(x.Product)).ConvertAll(x => x.UAN);
            if (uanlist.Count > 0)
            {
                string query = @"select gln from items where gln in (" + string.Join(",", uanlist.ConvertAll(x => "'" + x + "'")) + ") and isactive=true";
                using (var reader = data.ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        uanlist.Remove(reader.GetString(0));
                    }
                }
            }
            AgreementStatus agreementStatus = new AgreementStatus()
            {
                missing_product = new List<string>(),
                missing_brand = new List<string>(),
                missing_size = new List<string>(),
                missing_pattern = new List<string>(),
                missing_material = new List<string>(),
                missing_style = new List<string>(),
                missing_colour = new List<string>(),
                missing_fit = new List<string>(),
                missing_sleeve = new List<string>(),
                missing_type = new List<string>(),
                missing_design = new List<string>(),
                missing_uan = uanlist,
                noofrows = list.Count()
            };
            InfyPOS.Data.ReferencelistDataService referencelistDataService = new InfyPOS.Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            InfyPOS.Data.ItemsDataService itemsDataService = new InfyPOS.Data.ItemsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            var brandlist = brandDataService.Search(new InfyPOS.Models.BrandCreteria());
            var types = new List<string>();
            types.AddRange("SIZE,PATTERN,STYLE,TYPE,COLOUR,MATERIAL,FIT,SLEEVE".Split(','));
            var referencelist = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                types = types
            });

            InfyPOS.Data.ProductsDataService productsDataService = new InfyPOS.Data.ProductsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } }; ;
            var existproductlist = productsDataService.Search(new InfyPOS.Models.ProductsCreteria());

            var productlist = list.GroupBy(x => x.Product).ToList().ConvertAll(x => x.Key);
            agreementStatus.missing_product = productlist.FindAll(x =>
            {
                return !existproductlist.Exists(y => y.name == x);
            });

            agreementStatus.missing_brand = list.GroupBy(x => x.Brand).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !brandlist.Exists(y => y.name == z);
            });

            agreementStatus.missing_size = list.GroupBy(x => x.Size).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "SIZE" && y.name == z);
            });
            agreementStatus.missing_material = list.GroupBy(x => x.Material).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "MATERIAL" && y.name == z);
            });
            agreementStatus.missing_fit = list.GroupBy(x => x.Fit).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "FIT" && y.name == z);
            });
            agreementStatus.missing_sleeve = list.GroupBy(x => x.Sleeve).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "SLEEVE" && y.name == z);
            });
            agreementStatus.missing_pattern = list.GroupBy(x => x.Pattern).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "PATTERN" && y.name == z);
            });
            agreementStatus.missing_style = list.GroupBy(x => x.Style).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "STYLE" && y.name == z);
            });
            agreementStatus.missing_colour = list.GroupBy(x => x.Colour).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "COLOUR" && y.name == z);
            });
            agreementStatus.missing_type = list.GroupBy(x => x.Type).ToList().ConvertAll(x => x.Key).FindAll(z =>
            {
                return !referencelist.Exists(y => y.type == "TYPE" && y.name == z);
            });
            return agreementStatus;
        }

        internal BillSummary UploadAgreement(MigrationData data, ImageData imageData)
        {
            var file = imageData.GetData(long.Parse(data.uploadfileid));
            BillSummary result = null;
            List<AgreementProductItem> list = null;
            AgreementStatus agreementStatus = new AgreementStatus();
            var columnnames = @"Product,HSN,Agreementname,Agreementon,ItemCode,Cost,Price,MRP,UAN,Brand,ItemName,Size,Colour,Style,Pattern,Material,Type,Sleeve,Fit".Split(',');
            using (System.IO.MemoryStream memoryStream = new MemoryStream(file.data))
            {
                list = Process(memoryStream, columnnames);
                agreementStatus = GetStatus(list, imageData);
            }

            InfyPOS.Data.ReferencelistDataService referencelistDataService = new InfyPOS.Data.ReferencelistDataService() { Context = imageData.Context }; ;
            InfyPOS.Data.ItemsDataService itemsDataService = new InfyPOS.Data.ItemsDataService() { Context = imageData.Context }; ;
            InfyPOS.Data.BrandDataService brandDataService = new InfyPOS.Data.BrandDataService() { Context = imageData.Context }; ;
            var brandlist = brandDataService.Search(new InfyPOS.Models.BrandCreteria());
            var types = new List<string>();
            types.AddRange("SIZE,PATTERN,STYLE,TYPE,COLOUR,MATERIAL,FIT,SLEEVE".Split(','));
            var referencelist = referencelistDataService.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                types = types
            });

            if (agreementStatus.missing_product.Count > 0)
            {
                throw new Exception("Some of the products not available,please create these products - " +
                    String.Join(",", agreementStatus.missing_product));
            }

            InfyPOS.Data.ProductsDataService productsDataService = new InfyPOS.Data.ProductsDataService() { Context = imageData.Context }; ;
            var existproductlist = productsDataService.Search(new InfyPOS.Models.ProductsCreteria());


            foreach (var item in list.GroupBy(e => e.Product))
            {
                var supplier = existproductlist.Find(e => e.name == item.Key);
                item.ToList().ForEach(e => e.productid = supplier.id);
            }


            foreach (var item in list.GroupBy(e => e.Brand))
            {
                var supplier = brandlist.Exists(e => e.name == item.Key) ? brandlist.Find(e => e.name == item.Key) : new InfyPOS.Models.Brand() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    brandlist.Add(brandDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.brandid = supplier.id);
            }


            foreach (var item in list.GroupBy(e => e.Colour))
            {
                var supplier = referencelist.Exists(e => e.type == "COLOUR" && e.name == item.Key) ? referencelist.Find(e => e.type == "COLOUR" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "COLOUR";
                    referencelistDataService.Insert(supplier);
                }
                referencelist.Add(referencelistDataService.Insert(supplier));
            }

            foreach (var item in list.GroupBy(e => e.Sleeve))
            {
                var supplier = referencelist.Exists(e => e.type == "SLEEVE" && e.name == item.Key) ? referencelist.Find(e => e.type == "SLEEVE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "SLEEVE";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.sleeveid = supplier.id);

            }

            foreach (var item in list.GroupBy(e => e.Fit))
            {
                var supplier = referencelist.Exists(e => e.type == "FIT" && e.name == item.Key) ? referencelist.Find(e => e.type == "FIT" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "FIT";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.fitid = supplier.id);
            }

            foreach (var item in list.GroupBy(e => e.Material))
            {
                var supplier = referencelist.Exists(e => e.type == "MATERIAL" && e.name == item.Key) ? referencelist.Find(e => e.type == "MATERIAL" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "MATERIAL";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.materialid = supplier.id);
            }

            foreach (var item in list.GroupBy(e => e.Pattern))
            {
                var supplier = referencelist.Exists(e => e.type == "PATTERN" && e.name == item.Key) ? referencelist.Find(e => e.type == "PATTERN" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "PATTERN";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.patternid = supplier.id);
            }

            foreach (var item in list.GroupBy(e => e.Size))
            {
                var supplier = referencelist.Exists(e => e.type == "SIZE" && e.name == item.Key) ? referencelist.Find(e => e.type == "SIZE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "SIZE";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.sizeid = supplier.id);
            }

            foreach (var item in list.GroupBy(e => e.Style))
            {
                var supplier = referencelist.Exists(e => e.type == "STYLE" && e.name == item.Key) ? referencelist.Find(e => e.type == "STYLE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "STYLE";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.styleid = supplier.id);
            }

            foreach (var item in list.GroupBy(e => e.Type))
            {
                var supplier = referencelist.Exists(e => e.type == "TYPE" && e.name == item.Key) ? referencelist.Find(e => e.type == "TYPE" && e.name == item.Key) : new InfyPOS.Models.Referencelist() { isactive = true };
                if (supplier.id == 0)
                {
                    supplier.name = item.Key;
                    supplier.type = "TYPE";
                    referencelist.Add(referencelistDataService.Insert(supplier));
                }
                item.ToList().ForEach(e => e.typeid = supplier.id);
            }

            //var itemlist = itemsDataService.Search(new InfyPOS.Models.ItemsCreteria()
            //{
            //    support_gln = true,
            //    glns = list.FindAll(x => !string.IsNullOrEmpty(x.Product)).ConvertAll(x => x.UAN)
            //});

            SupplierproductgroupsDataService supplierproductgroupsDataService = new SupplierproductgroupsDataService() { Context = imageData.Context };
            SupplierDataService supplierDataService = new SupplierDataService() { Context = imageData.Context };
            var currentsupplier = supplierDataService.Search(new SupplierCreteria() { id = data.supplierid }).First();

            SuppliertradeagreementDataService suppliertradeagreementDataService = new SuppliertradeagreementDataService() { Context = imageData.Context };




            foreach (var prd in list.GroupBy(x => x.productid))
            {
                var product = existproductlist.Find(x => x.id == prd.Key);
                Supplierproductgroups supplierproductgroups = null;

                var supplierproducts = supplierproductgroupsDataService.Search(new SupplierproductgroupsCreteria() { supplierid = currentsupplier.id, productgroupid = prd.Key });
                if (supplierproducts.Count == 1)
                {
                    supplierproductgroups = supplierproducts[0];
                }
                else
                {
                    supplierproductgroups = new Supplierproductgroups() { supplierid = currentsupplier.id, productgroupid = prd.Key };
                    supplierproductgroups.itemcode = currentsupplier.code + product.code;
                    supplierproductgroups.isactive = true;
                    supplierproductgroups.createdby = imageData.Context.UserID;
                    supplierproductgroups.modifiedby = imageData.Context.UserID;
                    supplierproductgroups.modifiedon = DateTime.Now;
                    supplierproductgroups.createdon = DateTime.Now;
                    supplierproductgroupsDataService.Insert(supplierproductgroups);
                }

                if (supplierproductgroups.attributelist == null)
                    supplierproductgroups.attributelist = new List<Supplierproductgroups.Attributelist>();

                AddAttribute(supplierproductgroups.attributelist, referencelist, "SIZE", prd.ToList().ConvertAll(x => x.Size).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "MATERIAL", prd.ToList().ConvertAll(x => x.Material).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "STYLE", prd.ToList().ConvertAll(x => x.Style).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "COLOUR", prd.ToList().ConvertAll(x => x.Colour).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "FIT", prd.ToList().ConvertAll(x => x.Fit).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "SLEEVE", prd.ToList().ConvertAll(x => x.Sleeve).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "PATTERN", prd.ToList().ConvertAll(x => x.Pattern).Distinct().ToList());
                AddAttribute(supplierproductgroups.attributelist, referencelist, "TYPE", prd.ToList().ConvertAll(x => x.Type).Distinct().ToList());

                supplierproductgroupsDataService.Update(supplierproductgroups);

                foreach (var agreement in prd.GroupBy(x => new { x.ItemCode, x.Agreementon, x.brandid, x.Agreementname, x.Cost, x.Price, x.MRP }))
                {
                    SupplierTransactionWithMatrix suppliertradeagreement = new SupplierTransactionWithMatrix()
                    {
                        supplierid = currentsupplier.id,
                        productgroupid = supplierproductgroups.productgroupid,
                        itemcode = agreement.Key.ItemCode,
                        isactive = true,
                        agreementon = agreement.Key.Agreementon,
                        agreementname = agreement.Key.Agreementname,
                        mrp = agreement.Key.MRP,
                        price = agreement.Key.Price,
                        cost = agreement.Key.Cost,
                        namingmode = data.ItemNameTemplateID,
                        barcodemode = data.barcodemode,
                        brandid = agreement.Key.brandid,
                        approvedby = data.approvedby,
                        Supplierproductgroupsid = supplierproductgroups.id
                    };

                    suppliertradeagreement.matrixlist = new List<Suppliertradeagreement.matrixconfig>();
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "SIZE", agreement.ToList().ConvertAll(x => x.Size).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "MATERIAL", agreement.ToList().ConvertAll(x => x.Material).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "STYLE", agreement.ToList().ConvertAll(x => x.Style).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "COLOUR", agreement.ToList().ConvertAll(x => x.Colour).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "FIT", agreement.ToList().ConvertAll(x => x.Fit).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "SLEEVE", agreement.ToList().ConvertAll(x => x.Sleeve).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "PATTERN", agreement.ToList().ConvertAll(x => x.Pattern).Distinct().ToList());
                    AddAttribute(suppliertradeagreement.matrixlist, supplierproductgroups.attributelist, "TYPE", agreement.ToList().ConvertAll(x => x.Type).Distinct().ToList());
                    suppliertradeagreementDataService.Insert(suppliertradeagreement);

                    if (data.ItemNameTemplateID > 0 &&
                        !string.IsNullOrEmpty(data.ItemNameTemplate))
                    {
                        foreach (var agreementitem in agreement)
                        {
                            if (string.IsNullOrEmpty(agreementitem.ItemName))
                            {
                                agreementitem.ItemName = data.ItemNameTemplate;
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[PRODUCT]", agreementitem.Product);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[BRAND]", agreementitem.Brand);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[COLOUR]", agreementitem.Colour);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[STYLE]", agreementitem.Style);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[SIZE]", agreementitem.Size);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[MATERIAL]", agreementitem.Material);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[SLEEVE]", agreementitem.Sleeve);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[FIT]", agreementitem.Fit);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[PATTERN]", agreementitem.Pattern);
                                agreementitem.ItemName = agreementitem.ItemName.Replace("[TYPE]", agreementitem.Type);
                            }
                        }
                    }

                    if (suppliertradeagreement.items == null)
                        suppliertradeagreement.items = new List<Suppliertradeagreement.MatrixItem>();

                    foreach (var agreementitem in agreement)
                    {
                        suppliertradeagreement.items.Add(new Suppliertradeagreement.MatrixItem()
                        {
                            gln = agreementitem.UAN,
                            colourid = agreementitem.colourid,
                            cost = agreementitem.Cost,
                            fitid = agreementitem.fitid,
                            materialid = agreementitem.materialid,
                            mrp = agreementitem.MRP,
                            patternid = agreementitem.patternid,
                            price = agreementitem.Price,
                            printingname = agreementitem.ItemName,
                            sizeid = agreementitem.sizeid,
                            sleeveid = agreementitem.sleeveid,
                            styleid = agreementitem.styleid,
                            typeid = agreementitem.typeid
                        });
                    }
                    suppliertradeagreementDataService.UpdateMatrixItems(suppliertradeagreement);
                }
                supplierproductgroups.isactive = true;
            }
            return result;
        }

        private void AddAttribute(List<Suppliertradeagreement.matrixconfig> attributelist,
        List<Supplierproductgroups.Attributelist> referenceLists,
        string type, List<string> attribs)
        {
            var typeindex = attributetypes.FindIndex(x => x.ToUpper() == type);
            var config = new Suppliertradeagreement.matrixconfig()
            {
                attributetype = attributetypes[typeindex],
                items = new List<Suppliertradeagreement.matrixconfig.attributeid>()
            };
            attribs.ForEach(x =>
            {
                var existitem = referenceLists.Find(y => y.attributetype == config.attributetype &&
                    y.attributename == x);
                if (existitem != null)
                {
                    config.items.Add(new Suppliertradeagreement.matrixconfig.attributeid()
                    {
                        id = existitem.attributeid
                    });
                }
            });
        }

        private void AddAttribute(List<Supplierproductgroups.Attributelist> attributelist,
            List<InfyPOS.Models.Referencelist> referenceLists,
            string type, List<string> attribs)
        {
            var typeindex = attributetypes.FindIndex(x => x.ToUpper() == type);
            attribs.RemoveAll(x => attributelist.Exists(y => y.attributetype.ToUpper() == type && y.attributename == x));
            attribs.ForEach(x =>
            {
                if (!string.IsNullOrEmpty(x))
                {
                    attributelist.Add(new Supplierproductgroups.Attributelist()
                    {
                        attributeid = referenceLists.Find(y => y.name == x).id,
                        attributename = x,
                        attributetype = attributetypes[typeindex],
                        attributetypeid = typeindex + 1
                    });
                }
            });
        }

        List<String> attributetypes = new List<string>("Size,Colour,Style,Pattern,Sleeve,Fit,Material,Type".Split(','));
        public List<AgreementProductItem> Process(Stream source, string[] excelcolumnnames)
        {
            var excelheader = new List<string>();
            var columnsheaders = new List<string>();
            var rows = new List<List<string>>();
            var columnindex = new SortedDictionary<string, int>();
            var columnnames = "Product,Agreementname,Agreementon,ItemCode,Cost,Price,MRP,UAN,Brand,ItemName,Size,Colour,Style,Pattern,Material,Type,Sleeve,Fit".Split(',');

            SortedDictionary<string, MigrationManager.ColumnMap> columnmap = new SortedDictionary<string, MigrationManager.ColumnMap>();


            var ecolumns = new List<string>(excelcolumnnames).ConvertAll(e => e.ToLower());

            for (int i = 0; i < columnnames.Length; i++)
            {
                if (ecolumns.Contains(columnnames[i].ToLower()))
                {
                    columnmap.Add(columnnames[i].ToString().ToLower(), new MigrationManager.ColumnMap()
                    {
                        columnmap = columnnames[i],
                        columnname = columnnames[i].ToString().ToLower(),
                        index = ecolumns.IndexOf(columnnames[i].ToLower())
                    });
                }
            }


            using (source)
            {
                ExcelPackage package = new ExcelPackage(source);
                var worksheet = package.Workbook.Worksheets[1];
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
                            columnsheaders.Add(scellvalue);
                        }
                        else
                        {
                            if (cellvalue != null &&
                               typeof(DateTime) == cellvalue.GetType())
                            {
                                rowvalues.Add(cellvalue != null ? ((DateTime)cellvalue).ToString("yyyy-MM-dd") : "");
                            }
                            else
                            {
                                rowvalues.Add(cellvalue != null ? cellvalue.ToString() : "");
                            }
                        }
                    }
                    if (firstRow)
                    {
                        firstRow = false;
                    }
                    else
                    {
                        if (!rowvalues.All(x => string.IsNullOrEmpty(x)))
                            rows.Add(rowvalues);
                    }
                }
            }

            foreach (var item in columnmap)
            {
                item.Value.columnmap = item.Value.columnmap != null ? item.Value.columnmap.ToLower() : "";
                item.Value.index = columnsheaders.Contains(item.Value.columnmap) ? (columnsheaders.IndexOf(item.Value.columnmap)) : -1;
            }

            List<AgreementProductItem> lrInvoiceItems = new List<AgreementProductItem>();
            foreach (var item in rows)
            {
                lrInvoiceItems.Add(new AgreementProductItem(item, columnmap));
            }

            return lrInvoiceItems;
        }

        internal DocumentStatus ValidateSKUImages(HttpRequestMessage Request, int type, ImageData data)
        {
            Exception processException = null;
            DocumentStatus result = new DocumentStatus() { sostatus = new SOUANUploaded() };
            try
            {
                List<string> lst = new List<string>();
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return null;
                }

                System.Threading.AutoResetEvent are = new System.Threading.AutoResetEvent(false);
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
                                MfgDesingImport mfgDesingImport = new MfgDesingImport();
                                result.skustatus = mfgDesingImport.ProcessImages(data.Context, buffer);
                                break;
                            }
                            are.Set();
                        }
                        catch (Exception exp)
                        {
                            processException = exp;
                            WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                            are.Set();
                        }
                    });
                    are.WaitOne();
                }
                catch (Exception exp)
                {
                    processException = exp;
                    WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    are.Set();
                }
                return result;
            }
            catch (Exception exp)
            {
                processException = exp;
                WebAPI.Common.Logger.Instance(data.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            if (processException != null)
                throw processException;
            return result;
        }

        public class BaseImportItem
        {
            private static CultureInfo enUS = new CultureInfo("en-US");
            public DateTime GetDate(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                name = name.ToLower();
                if (columnMap.ContainsKey(name) && columnMap[name].index >= 0)
                {
                    var value = source[columnMap[name].index];
                    if (!string.IsNullOrEmpty(value) && value.Length > 10)
                        value = value.Substring(0, 10);
                    DateTime dt;
                    if (DateTime.TryParseExact(value, "yyyy-MM-dd", enUS, DateTimeStyles.None, out dt))
                        return dt;
                    if (DateTime.TryParseExact(value, "dd-MM-yyyy", enUS, DateTimeStyles.None, out dt))
                        return dt;
                    if (DateTime.TryParseExact(value, "dd/MM/yyyy", enUS, DateTimeStyles.None, out dt))
                        return dt;
                    double d;
                    if (double.TryParse(value, out d))
                        return DateTime.FromOADate(d);
                }
                return DateTime.MinValue;
            }

            public decimal GetDecimal(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                name = name.ToLower();
                if (columnMap.ContainsKey(name) && columnMap[name].index >= 0)
                {
                    var value = source[columnMap[name].index];
                    decimal dvalue;
                    if (decimal.TryParse(value, out dvalue))
                        return dvalue;
                }
                return 0;
            }

            private bool isEmpty(string value)
            {
                return string.IsNullOrEmpty(value) || value == "." || value == "NULL" || value == "[NULL]" || value == "None" || value == "[None]";
            }
            public string GetString(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap, string name)
            {
                name = name.ToLower();
                if (columnMap.ContainsKey(name))
                {
                    if (columnMap[name].index >= 0 && source.Count > columnMap[name].index)
                        return isEmpty(source[columnMap[name].index]) ? "" : source[columnMap[name].index].Trim();

                    return isEmpty(columnMap[name].defaultvalue) ? "" : columnMap[name].defaultvalue;
                }
                return "";
            }
        }
        public class AgreementProductItem : BaseImportItem
        {

            public AgreementProductItem(List<string> source, SortedDictionary<string, MigrationManager.ColumnMap> columnMap)
            {
                DesignID = GetString(source, columnMap, "DesignID");
                Agreementon = GetDate(source, columnMap, "Agreementon");
                Agreementname = GetString(source, columnMap, "Agreementname");
                Product = GetString(source, columnMap, "Product");
                ItemCode = GetString(source, columnMap, "ItemCode");

                Cost = GetDecimal(source, columnMap, "Cost");
                Price = GetDecimal(source, columnMap, "Price");
                MRP = GetDecimal(source, columnMap, "MRP");

                UAN = GetString(source, columnMap, "UAN");
                Brand = GetString(source, columnMap, "Brand");
                ItemName = GetString(source, columnMap, "ItemName");
                Size = GetString(source, columnMap, "Size");
                Colour = GetString(source, columnMap, "Colour");

                Style = GetString(source, columnMap, "Style");
                Pattern = GetString(source, columnMap, "Pattern");
                Material = GetString(source, columnMap, "Material");
                Type = GetString(source, columnMap, "Type");

                Sleeve = GetString(source, columnMap, "Sleeve");
                Fit = GetString(source, columnMap, "Fit");
            }

            public string DesignID { get; set; }
            public DateTime Agreementon { get; set; }
            public string Agreementname { get; set; }
            public string ItemName { get; set; }
            public decimal MRP { get; set; }
            public decimal Cost { get; set; }
            public decimal Price { get; set; }
            public string UAN { get; set; }
            public string HSN { get; set; }
            public string ItemCode { get; set; }
            public string Product { get; set; }
            public string Brand { get; set; }
            public string Size { get; set; }
            public string Style { get; set; }
            public string Pattern { get; set; }
            public string Type { get; set; }
            public string Material { get; set; }
            public string Colour { get; set; }
            public string Fit { get; set; }
            public string Sleeve { get; set; }
            public long itemid { get; set; }
            public long productid { get; set; }
            public long brandid { get; set; }
            public long colourid { get; set; }
            public long materialid { get; set; }
            public long patternid { get; set; }
            public long sizeid { get; set; }
            public long styleid { get; set; }
            public long typeid { get; set; }
            public long sleeveid { get; set; }
            public long fitid { get; set; }
        }
    }
}