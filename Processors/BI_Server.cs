using InfyPOS.Common;
using InfyPOS.Controllers;
using InfyPOS.Data;
using InfyPOS.Models;
using Newtonsoft.Json;
using Npgsql;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.EnterpriseServices.Internal;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text.RegularExpressions;
using System.Web;
using static InfyPOS.Services.SyncManager;

namespace InfyPOS.Services
{
    public class BI_Server : SAASData
    {
        Models.Referencelist config { get; set; }
        public bool IsValid { get; set; }
        public long organizationid { get; set; }
        DateTime fromdate { get; set; }
        public bool Connect(string clilentid)
        {
            this.organizationid = organizationid;
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var cliientlist = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                type = "OFFLINECLIENT",
                name = clilentid,
                locationid = -1
            });
            if (cliientlist.Count == 1)
            {
                config = cliientlist[0];
                return true;
            }
            return false;
        }

        internal string ClearOfflineClient(long organizationid)
        {
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var masterrows = baseData.ExecuteQuery("Delete from sync_master where locationid=-1 and clientid=" + this.config.id, null);
            var transactionrows = baseData.ExecuteQuery("Delete from sync_transaction where locationid=01 and clientid=" + this.config.id, null);
            return string.Format("Deleted {0} rows masters, Deleted {1} rows transactions.", masterrows, transactionrows);
        }


        public bool ChangeMasterStatus(List<DBData> data)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in data)
                {
                    string json = item.GetStatusJSON();
                    string insertQuery = @"insert into sync_master(tabletype,sourceid,locationid,clientid,version,createdon,modifiedon)
                                          select @tabletype,cast(statusList->>'id' as bigint),@locationid,@clientid,cast(statusList->>'version' as int),now()::timestamp,now()::timestamp from json_array_elements ('" + json + @"'::json) statusList 
                                          left outer join sync_master on sync_master.tabletype = @tabletype and sync_master.locationid = @locationid and clientid = @clientid and sync_master.sourceid = cast(statusList->>'id' as bigint)
                                          where sync_master.id is null";

                    string updateQuery = @"update sync_master set version =  cast(statusList->>'version' as integer),modifiedon=now()::timestamp from json_array_elements ('" + json + @"'::json) statusList
                                           where sync_master.tabletype = @tabletype and sync_master.locationid = @locationid and clientid = @clientid and cast(statusList->>'id' as bigint) = sync_master.sourceid";


                    NpgsqlCommand command = new NpgsqlCommand(insertQuery, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.type;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = -1;
                    command.ExecuteNonQuery();

                    command.CommandText = updateQuery;
                    command.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
                return false;
            }
        }

        public bool ChangeTransactionStatus(List<DBData> data)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in data)
                {
                    var list = item.GetPartionedStatus(250);
                    foreach (var litem in list)
                    {
                        var json = item.GetStatusJSON(litem);
                        string insertQuery = @"insert into sync_transaction(tabletype,sourceid,code,locationid,clientid,version,createdon,modifiedon)
                                          select @tabletype,cast(statusList->>'id' as bigint),cast(statusList->>'code' as varchar),@locationid,@clientid,cast(statusList->>'version' as int),now()::timestamp,now()::timestamp from json_array_elements ('" + json + @"'::json) statusList 
                                          left outer join sync_transaction on sync_transaction.tabletype = @tabletype and sync_transaction.locationid = @locationid and clientid = @clientid and sync_transaction.sourceid = cast(statusList->>'id' as bigint)
                                          where sync_transaction.id is null";

                        string updateQuery = @"update sync_transaction set version = cast(statusList->>'version' as int),modifiedon=now()::timestamp from json_array_elements ('" + json + @"'::json) statusList
                                           where sync_transaction.tabletype = @tabletype and sync_transaction.locationid = @locationid and clientid = @clientid and cast(statusList->>'id' as bigint) = sync_transaction.sourceid";


                        NpgsqlCommand command = new NpgsqlCommand(insertQuery, conn);
                        command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.type;
                        command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = -1;
                        command.ExecuteNonQuery();

                        command.CommandText = updateQuery;
                        command.ExecuteNonQuery();
                    }
                }
                conn.Close();
                return true;
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
                return false;
            }
        }


        public class DownloadList
        {
            public bool downloadvoid { get; set; }
            public bool stock { get; set; }
            public bool sales { get; set; }
            public bool purchase { get; set; }
            public bool purchaseorder { get; set; }
            public bool lrentry { get; set; }
        }

        public enum QueryTypes
        {
            Purchase,
            Stock,
            Sales,
            PurchaseOrder,
            LrEntry
        }
        static ConcurrentDictionary<string, SyncManager.ProcessStatus> processinglist = new ConcurrentDictionary<string, SyncManager.ProcessStatus>();
        public SyncManager.ProcessStatus StartDownloadMaster(string parameter)
        {
            string paramstring = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(parameter));
            DownloadList downloadList = JsonConvert.DeserializeObject<DownloadList>(paramstring);
            SyncManager.ProcessStatus status = new SyncManager.ProcessStatus()
            {
                key = Guid.NewGuid().ToString(),
                parameter = downloadList
            };
            processinglist.TryAdd(status.key, status);
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(DownloadMasterInitiate));
            thread.Start(status);
            return status;
        }

        public SyncManager.ProcessStatus GetDownloadedMaster(string key)
        {
            SyncManager.ProcessStatus status = new SyncManager.ProcessStatus();
            if (processinglist.TryGetValue(key, out status))
            {
                return status;
            }
            return null;
        }

        public void DownloadMasterInitiate(object _status)
        {
            System.Threading.Thread.Sleep(100);
            SyncManager.ProcessStatus status = _status as SyncManager.ProcessStatus;
            DownloadMaster(status);
        }



        public class DBSchemaTable
        {
            public override string ToString()
            {
                return name;
            }
            public string CreateTableScript()
            {
                if (columns == null || columns.Count == 0) return "";
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                //stringBuilder.Append(@"CREATE TABLE public.agent(");
                foreach (var item in columns)
                {
                    stringBuilder.AppendLine((stringBuilder.Length > 0 ? "," : "") + item.toscript());
                }
                if (string.IsNullOrEmpty(name))
                    return "";
                if (name == "triggers")
                    return "";
                stringBuilder.Insert(0, "CREATE TABLE public." + name + "(");
                stringBuilder.Append(");");
                return stringBuilder.ToString();
            }

            public string CreateDeleteScript()
            {
                if (columns == null || columns.Count == 0 || string.IsNullOrEmpty(name)) return "";


                return string.Format("delete from {0};", name);
            }


            public string name { get; set; }

            public List<Column> columns { get; set; }

            public class Column
            {
                public bool isautoincrement { get; set; }
                public string column_name { get; set; }
                public string data_type { get; set; }
                public int character_maximum_length { get; set; }
                public string defaultvalue { get; set; }
                public int numeric_precision { get; set; }
                public int numeric_scale { get; set; }

                public string toscript()
                {
                    if (",methodName,killedBy,loginOn,logoutOn,".Contains("," + column_name + ","))
                        return character_maximum_length > 0 ?
                            string.Format("\"{0}\" {1} ({2}) {3}", column_name, data_type, character_maximum_length,
                            string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue)) :
                            string.Format("\"{0}\" {1}", column_name, data_type, character_maximum_length,
                            string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));

                    if (numeric_precision > 0 &&
                            numeric_scale > 0)
                        return string.Format("{0} {1} ({2},{3}) {4}", column_name, data_type, numeric_precision, numeric_scale,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));

                    return character_maximum_length > 0 ?
                        string.Format("{0} {1} ({2}) {3}", column_name, data_type, character_maximum_length,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue)) :
                        string.Format("{0} {1} {3}", column_name, data_type, character_maximum_length,
                        string.IsNullOrEmpty(defaultvalue) ? "" : (" DEFAULT " + defaultvalue));
                }
            }

        }

        public List<DBSchemaTable> DBSchema()
        {
            string fields = "";
            string joins = "";
            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                int joinrefcount = 1;
                foreach (var item in CustomFieldsManager.Instance(Context.OrganizationID).Config)
                {
                    if (item.cftype == "bigint")
                    {
                        fields += (!string.IsNullOrEmpty(fields) ? "," : "") + ("CustomFields_" + joinrefcount) + ".name as " + item.cfname;
                        joins += " left outer join referencelist " + ("CustomFields_" + joinrefcount) + " on " + ("CustomFields_" + joinrefcount) + ".id = lrinvoiceitems." + item.cfname + "\r\n";
                        joinrefcount++;
                    }
                    else
                    {
                        fields += (!string.IsNullOrEmpty(fields) ? "," : "") + item.cfname;
                    }
                }
                if (!string.IsNullOrEmpty(fields))
                    fields = "," + fields;
            }

            var stockquery = @"select salablegoods.id,stock.version+COALESCE(lrinvoiceitems.version,0)+items.version+products.version+salablegoods.version ""version"",
                                        company.code companycode,location.code locationcode,lrinvoiceitems.itbuscode,stock.barcode,stock.serialno,salablegoods.price,
                                        stock.displaymrp mrp,products.name product,prggroup.name productgroup,prggroup1.name pgh_1,prggroup2.name pgh_2,
                                        prggroup3.name pgh_3,prggroup4.name pgh_4,prggroup5.name pgh_5,brand.name brand,items.gln,size.name size,style.name style,
                                        salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) qty,
                                        location.name stocklocation
                                        {0}
                                        into {2}_stock
                                        from salablegoods
                                        inner join stock on stock.id = salablegoods.stockid and stock.isactive=true
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                        inner join items on items.id = stock.itemid
                                        inner join products on products.id = items.productid
                                        inner join company on company.id = salablegoods.companyid
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = items.sizeid
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist location on location.id = salablegoods.locationid
                                        left outer join referencelist prggroup on prggroup.id = products.productgroup
                                        left outer join referencelist prggroup1 on prggroup1.id = products.productgroup_1
                                        left outer join referencelist prggroup2 on prggroup2.id = products.productgroup_2
                                        left outer join referencelist prggroup3 on prggroup3.id = products.productgroup_3
                                        left outer join referencelist prggroup4 on prggroup4.id = products.productgroup_4
                                        left outer join referencelist prggroup5 on prggroup5.id = products.productgroup_5
                                        {1}
                                        where salablegoods.isactive=true
                                        limit 1";


            var purchasequery = @"select lrinvoiceitems.id,lrinvoiceitems.version,
                                        lrinvoice.invoiceno,lrinvoice.entrydate,lrinvoice.invoicedate,
                                        lrinvoiceitems.itbuscode,stock.barcode,stock.serialno,
                                        suppliercompany.name supplier,lrinvoiceitems.buyingqty,
                                        lrinvoiceitems.buyingprice,lrinvoiceitems.buyingprice*lrinvoiceitems.buyingqty amount,
                                        lrinvoiceitems.purchasetax,lrinvoiceitems.purchasediscount,products.name product,
                                        prggroup.name productgroup,prggroup1.name pgh_1,prggroup2.name pgh_2,
                                        prggroup3.name pgh_3,prggroup4.name pgh_4,prggroup5.name pgh_5,brand.name brand,
                                        items.gln,size.name size,style.name style,
                                        section.name section,
                                        coalesce(lrinvoiceitems.designid, stock.designid) designwithimage,
                                        coalesce(stock.imageid, items.imageid) designimage,
                                        material.name material,pattern.name pattern,sleeve.name sleeve,
                                        type.name type,colour.name color,fit.name fit,
                                        supcity.name suppliercity,agent.name supplieragent,
                                        coalesce(stock.stockdate, stock.createdon) barcodegenerationdate,
                                        coalesce(nullif(lrinvoice.pono,''), purchaseorder.pono) pono,
                                        lrentry.lrentryno
                                        {0}
                                        into {2}_purchase
                                        from lrentry
                                        inner join lrinvoice on lrinvoice.lrid = lrentry.id
                                        inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                        inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
                                        inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                                        left outer join referencelist on referencelist.id = lrentry.retaillocationid
                                        left outer join items on items.id = stock.itemid
                                        inner join products on products.id = items.productid
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = items.sizeid
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist prggroup on prggroup.id = products.productgroup
                                        left outer join referencelist prggroup1 on prggroup1.id = products.productgroup_1
                                        left outer join referencelist prggroup2 on prggroup2.id = products.productgroup_2
                                        left outer join referencelist prggroup3 on prggroup3.id = products.productgroup_3
                                        left outer join referencelist prggroup4 on prggroup4.id = products.productgroup_4
                                        left outer join referencelist prggroup5 on prggroup5.id = products.productgroup_5
                                        left outer join referencelist section on section.id = products.salescounterid
                                        left outer join referencelist material on material.id = items.materialid
                                        left outer join referencelist pattern on pattern.id = items.patternid
                                        left outer join referencelist sleeve on sleeve.id = items.sleeveid
                                        left outer join referencelist type on type.id = items.typeid
                                        left outer join referencelist colour on colour.id = items.colourid
                                        left outer join referencelist fit on fit.id = items.fitid
                                        left outer join referencelist supcity on supcity.id = suppliercompany.cityid
                                        left outer join agent on agent.id = lrentry.agentid
                                        left outer join purchaseorder on purchaseorder.id = lrinvoice.poid
                                        {1}
                                        where lrinvoiceitems.isactive=true
                                        limit 1";


            string billquery = @"select billitems.id,billitems.version, billno,billdate,lrinvoiceitems.itbuscode,stock.barcode,stock.serialno,
                                        billitems.qty,billitems.price,billitems.receivable-billitems.tax gross,billitems.tax,billitems.receivable amount,
                                        stock.buyingprice,stock.ibuyingprice,referencelist.code storecode,customer.code customercode,products.name product,
                                        billitems.additionaldiscount*billitems.qty additionaldiscount,employee.name salesman,promotions.name promotion,billitems.schemediscount,
                                        prggroup.name productgroup,prggroup1.name pgh_1,prggroup2.name pgh_2,
                                        prggroup3.name pgh_3,prggroup4.name pgh_4,prggroup5.name pgh_5,brand.name brand,items.gln,size.name size,style.name style,
                                        customer.name customername,customer.mobileno customermobileno,customercity.name customercity,
                                        billitems.salesmanid,empssection.name salesmansection,
                                        bill.createdon billtime,referencelist.name saleslocation,company.name companyname,
                                        cw.feedback
                                        {0}
                                        into {2}_sales
                                        from bill
                                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                        inner join stock on stock.id = billitems.stockid
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                        left outer join employee on employee.id = billitems.salesmanid
                                        left outer join referencelist empssection on empssection.id = employee.sectionid
                                        left outer join promotions on promotions.id = billitems.schemeid
                                        inner join referencelist on referencelist.id = bill.locationid
                                        left outer join customer on customer.id = bill.customerid
                                        left outer join referencelist customercity on customercity.id = customer.cityid
                                        left outer join company on company.id = bill.companyid
                                        inner join items on items.id = stock.itemid
                                        inner join products on products.id = items.productid
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = items.sizeid
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist prggroup on prggroup.id = products.productgroup
                                        left outer join referencelist prggroup1 on prggroup1.id = products.productgroup_1
                                        left outer join referencelist prggroup2 on prggroup2.id = products.productgroup_2
                                        left outer join referencelist prggroup3 on prggroup3.id = products.productgroup_3
                                        left outer join referencelist prggroup4 on prggroup4.id = products.productgroup_4
                                        left outer join referencelist prggroup5 on prggroup5.id = products.productgroup_5
                                        left join lateral (
                                            select customerwalkins.feedback
                                            from customerwalkins
                                            where customerwalkins.billno = bill.billno
                                              and customerwalkins.isactive = true
                                            order by customerwalkins.id desc
                                            limit 1
                                        ) cw on true
                                        {1}
                                        where bill.isactive=true and bill.billtype!=9
                                        limit 1";

            string purchaseorderquery = @"select purchaseorderitems.id,purchaseorderitems.version,
                                        purchaseorder.podate,company.name purchasename,purchaseorder.pono,
                                        supplier.code suppliercode,products.name product,
                                        purchaseorderitems.designid design,brand.name brand,
                                        coalesce(size.name, purchaseorderitems.size) size,
                                        material.name material,pattern.name pattern,sleeve.name sleeve,
                                        type.name type,colour.name color,fit.name fit,style.name style,
                                        purchaseorder.expectedate duedate,purchaseorderitems.oqty qtyordered,
                                        case purchaseorder.status
                                            when 0 then 'PENDING'
                                            when 1 then 'APPROVED'
                                            when 2 then 'REJECTED'
                                            when 3 then 'PARTIALLYCOMPLETED'
                                            when 4 then 'COMPLETED'
                                            when 11 then 'UNAPPROVED'
                                            else cast(purchaseorder.status as text)
                                        end status
                                        into {0}_purchaseorder
                                        from purchaseorder
                                        inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id and purchaseorderitems.isactive=true
                                        left outer join supplier on supplier.id = purchaseorder.supplierid
                                        left outer join company on company.id = purchaseorder.companyid
                                        left outer join items on items.id = purchaseorderitems.itemid
                                        left outer join products on products.id = coalesce(nullif(purchaseorderitems.productid,0), items.productid)
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = coalesce(nullif(purchaseorderitems.sizeid,0), items.sizeid)
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist material on material.id = items.materialid
                                        left outer join referencelist pattern on pattern.id = items.patternid
                                        left outer join referencelist sleeve on sleeve.id = items.sleeveid
                                        left outer join referencelist type on type.id = items.typeid
                                        left outer join referencelist colour on colour.id = coalesce(nullif(purchaseorderitems.colourid,0), items.colourid)
                                        left outer join referencelist fit on fit.id = items.fitid
                                        where purchaseorder.isactive=true
                                        limit 1";

            string lrentryquery = @"select coalesce(lrinvoice.id, lrentry.id) id,
                                        coalesce(lrinvoice.version, lrentry.version) version,
                                        lrentry.lrentrydate,lrentry.lrentryno,
                                        coalesce(suppliercompany.name, supplier.name) suppliername,
                                        coalesce(nullif(lrinvoice.pono,''), purchaseorder.pono) pono,
                                        lrinvoice.invoiceno,
                                        case lrentry.status
                                            when 200 then 'New_LR'
                                            when 201 then 'LRIssue'
                                            when 202 then 'LRReceived'
                                            when 203 then 'Invoice_Preparation'
                                            when 204 then 'Items_Entering'
                                            when 205 then 'Barcode_Generating'
                                            when 206 then 'Item_Pricing'
                                            when 0 then 'Open'
                                            when 1 then 'Working'
                                            when 2 then 'Completed'
                                            when 3 then 'Cancel'
                                            when 4 then 'Approved'
                                            when 5 then 'Rejected'
                                            when 6 then 'Pending'
                                            else cast(lrentry.status as text)
                                        end bundlestatus,
                                        coalesce(lrinvoice.noofbundle, lrentry.noofbundels) bundlecount,
                                        lrentry.lrno
                                        into {0}_lrentry
                                        from lrentry
                                        left outer join supplier on supplier.id = lrentry.supplierid
                                        left outer join lrinvoice on lrinvoice.lrid = lrentry.id and lrinvoice.isactive=true
                                        left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                        left outer join purchaseorder on purchaseorder.id = lrinvoice.poid
                                        where lrentry.isactive=true
                                        limit 1";

            string uniqueid = "BI_TABLE_" + DateTime.UtcNow.Ticks.ToString();

            SortedDictionary<string, DBSchemaTable> tables = new SortedDictionary<string, DBSchemaTable>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                NpgsqlCommand command = new NpgsqlCommand(string.Format(stockquery, fields, joins, uniqueid), conn, transaction);
                command.ExecuteNonQuery();

                command = new NpgsqlCommand(string.Format(purchasequery, fields, joins, uniqueid), conn, transaction);
                command.ExecuteNonQuery();

                command = new NpgsqlCommand(string.Format(billquery, fields, joins, uniqueid), conn, transaction);
                command.ExecuteNonQuery();

                command = new NpgsqlCommand(string.Format(purchaseorderquery, uniqueid), conn, transaction);
                command.ExecuteNonQuery();

                command = new NpgsqlCommand(string.Format(lrentryquery, uniqueid), conn, transaction);
                command.ExecuteNonQuery();

                string fieldsquery = string.Format(@"select column_name, data_type, character_maximum_length,
                tables.table_name,columns.column_default,numeric_precision,numeric_scale from INFORMATION_SCHEMA.TABLES tables
                inner join INFORMATION_SCHEMA.COLUMNS columns on columns.table_name = tables.table_name
                where tables.table_schema = 'public' and tables.table_type='BASE TABLE'
                and tables.table_name ilike '{0}%'
                order by tables.table_name", uniqueid);


                command = new NpgsqlCommand(fieldsquery, conn, transaction);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DBSchemaTable.Column clmn = new DBSchemaTable.Column();
                        clmn.column_name = reader.GetString(0);
                        clmn.data_type = reader.GetString(1);


                        clmn.character_maximum_length = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        if (!string.IsNullOrEmpty(reader.IsDBNull(4) ? "" : reader.GetString(4)) &&
                            reader.GetString(4).StartsWith("nextval"))
                            clmn.data_type = "bigserial";
                        string tablename = reader.GetString(3).Replace(uniqueid.ToLower() + "_", "bi_");
                        string columndefault = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        clmn.defaultvalue = columndefault.StartsWith("nextval(") ? "" : columndefault;
                        clmn.numeric_precision = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                        clmn.numeric_scale = reader.IsDBNull(6) ? 0 : reader.GetInt32(6);
                        if (!tables.ContainsKey(tablename))
                            tables.Add(tablename, new DBSchemaTable()
                            {
                                columns = new List<DBSchemaTable.Column>(),
                                name = tablename
                            });

                        tables[tablename].columns.Add(clmn);
                    }

                }
                transaction.Rollback();
                conn.Close();
            }
            catch (Exception exp)
            {
                conn.Close();
            }
            List<DBSchemaTable> response = new List<DBSchemaTable>();
            response.AddRange(tables.Values);
            return response;
        }

        public void DownloadMaster(SyncManager.ProcessStatus status)
        {
            SortedDictionary<QueryTypes, string> masterquery = new SortedDictionary<QueryTypes, string>();


            string fields = "";
            string joins = "";
            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                int joinrefcount = 1;
                foreach (var item in CustomFieldsManager.Instance(Context.OrganizationID).Config)
                {
                    if (item.cftype == "bigint")
                    {
                        fields += (!string.IsNullOrEmpty(fields) ? "," : "") + ("CustomFields_" + joinrefcount) + ".name as " + item.cfname;
                        joins += " left outer join referencelist " + ("CustomFields_" + joinrefcount) + " on " + ("CustomFields_" + joinrefcount) + ".id = lrinvoiceitems." + item.cfname + "\r\n";
                        joinrefcount++;
                    }
                    else
                    {
                        fields += (!string.IsNullOrEmpty(fields) ? "," : "") + item.cfname;
                    }
                }
                if (!string.IsNullOrEmpty(fields))
                    fields = "," + fields;
            }


            if (status.parameter.stock)
                masterquery.Add(QueryTypes.Stock, @"select salablegoods.id,stock.version+COALESCE(lrinvoiceitems.version,0)+items.version+products.version+salablegoods.version as version,
                                        company.code companycode,location.code locationcode,lrinvoiceitems.itbuscode,stock.barcode,stock.serialno,salablegoods.price,
                                        stock.displaymrp mrp,products.name product,prggroup.name productgroup,prggroup1.name pgh_1,prggroup2.name pgh_2,
                                        prggroup3.name pgh_3,prggroup4.name pgh_4,prggroup5.name pgh_5,brand.name brand,items.gln,size.name size,style.name style,
                                        salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) qty,
                                        location.name stocklocation
                                        {0}
                                        from salablegoods
                                        inner join stock on stock.id = salablegoods.stockid and stock.isactive=true
                                        inner join items on items.id = stock.itemid
                                        inner join products on products.id = items.productid
                                        inner join company on company.id = salablegoods.companyid
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                        left outer join sync_master on sync_master.tabletype = @tabletype
                                            and sync_master.clientid = @clientid and sync_master.locationid=-1 and sync_master.sourceid = salablegoods.id
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = items.sizeid
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist location on location.id = salablegoods.locationid
                                        left outer join referencelist prggroup on prggroup.id = products.productgroup
                                        left outer join referencelist prggroup1 on prggroup1.id = products.productgroup_1
                                        left outer join referencelist prggroup2 on prggroup2.id = products.productgroup_2
                                        left outer join referencelist prggroup3 on prggroup3.id = products.productgroup_3
                                        left outer join referencelist prggroup4 on prggroup4.id = products.productgroup_4
                                        left outer join referencelist prggroup5 on prggroup5.id = products.productgroup_5
                                        {1}
                                        where salablegoods.isactive=true
                                          and (sync_master.id is null or stock.version+COALESCE(lrinvoiceitems.version,0)+items.version+products.version+salablegoods.version <> sync_master.version)");

            if (status.parameter.purchase)
                masterquery.Add(QueryTypes.Purchase, @"select lrinvoiceitems.id,lrinvoiceitems.version,
                                        lrinvoice.invoiceno,lrinvoice.entrydate,lrinvoice.invoicedate,
                                        lrinvoiceitems.itbuscode,stock.barcode,stock.serialno,
                                        suppliercompany.name supplier,lrinvoiceitems.buyingqty,
                                        lrinvoiceitems.buyingprice,lrinvoiceitems.buyingprice*lrinvoiceitems.buyingqty amount,
                                        lrinvoiceitems.purchasetax,lrinvoiceitems.purchasediscount,products.name product,
                                        prggroup.name productgroup,prggroup1.name pgh_1,prggroup2.name pgh_2,
                                        prggroup3.name pgh_3,prggroup4.name pgh_4,prggroup5.name pgh_5,brand.name brand,
                                        items.gln,size.name size,style.name style,
                                        section.name section,
                                        coalesce(lrinvoiceitems.designid, stock.designid) designwithimage,
                                        coalesce(stock.imageid, items.imageid) designimage,
                                        material.name material,pattern.name pattern,sleeve.name sleeve,
                                        type.name type,colour.name color,fit.name fit,
                                        supcity.name suppliercity,agent.name supplieragent,
                                        coalesce(stock.stockdate, stock.createdon) barcodegenerationdate,
                                        coalesce(nullif(lrinvoice.pono,''), purchaseorder.pono) pono,
                                        lrentry.lrentryno
                                        {0}
                                        from lrentry
                                        inner join lrinvoice on lrinvoice.lrid = lrentry.id
                                        inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                        inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
                                        inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                                        left outer join referencelist on referencelist.id = lrentry.retaillocationid
                                        left outer join items on items.id = stock.itemid
                                        inner join products on products.id = items.productid
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = items.sizeid
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist prggroup on prggroup.id = products.productgroup
                                        left outer join referencelist prggroup1 on prggroup1.id = products.productgroup_1
                                        left outer join referencelist prggroup2 on prggroup2.id = products.productgroup_2
                                        left outer join referencelist prggroup3 on prggroup3.id = products.productgroup_3
                                        left outer join referencelist prggroup4 on prggroup4.id = products.productgroup_4
                                        left outer join referencelist prggroup5 on prggroup5.id = products.productgroup_5
                                        left outer join referencelist section on section.id = products.salescounterid
                                        left outer join referencelist material on material.id = items.materialid
                                        left outer join referencelist pattern on pattern.id = items.patternid
                                        left outer join referencelist sleeve on sleeve.id = items.sleeveid
                                        left outer join referencelist type on type.id = items.typeid
                                        left outer join referencelist colour on colour.id = items.colourid
                                        left outer join referencelist fit on fit.id = items.fitid
                                        left outer join referencelist supcity on supcity.id = suppliercompany.cityid
                                        left outer join agent on agent.id = lrentry.agentid
                                        left outer join purchaseorder on purchaseorder.id = lrinvoice.poid
                                        {1}
                                        where lrinvoiceitems.isactive=true");

            if (status.parameter.sales)
                masterquery.Add(QueryTypes.Sales, @"select billitems.id,billitems.version, billno,billdate,lrinvoiceitems.itbuscode,stock.barcode,stock.serialno,
                                        billitems.qty,billitems.price,billitems.receivable-billitems.tax gross,billitems.tax,billitems.receivable amount,
                                        stock.buyingprice,stock.ibuyingprice,referencelist.code storecode,customer.code customercode,products.name product,
                                        billitems.additionaldiscount*billitems.qty additionaldiscount,employee.name salesman,promotions.name promotion,billitems.schemediscount,
                                        prggroup.name productgroup,prggroup1.name pgh_1,prggroup2.name pgh_2,
                                        prggroup3.name pgh_3,prggroup4.name pgh_4,prggroup5.name pgh_5,brand.name brand,items.gln,size.name size,style.name style,
                                        customer.name customername,customer.mobileno customermobileno,customercity.name customercity,
                                        billitems.salesmanid,empssection.name salesmansection,
                                        bill.createdon billtime,referencelist.name saleslocation,company.name companyname,
                                        cw.feedback
                                        {0}
                                        from bill
                                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                        inner join stock on stock.id = billitems.stockid
                                        left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                                        inner join referencelist on referencelist.id = bill.locationid
                                        left outer join customer on customer.id = bill.customerid
                                        left outer join referencelist customercity on customercity.id = customer.cityid
                                        left outer join employee on employee.id = billitems.salesmanid
                                        left outer join referencelist empssection on empssection.id = employee.sectionid
                                        left outer join promotions on promotions.id = billitems.schemeid
                                        left outer join company on company.id = bill.companyid
                                        inner join items on items.id = stock.itemid
                                        inner join products on products.id = items.productid
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = items.sizeid
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist prggroup on prggroup.id = products.productgroup
                                        left outer join referencelist prggroup1 on prggroup1.id = products.productgroup_1
                                        left outer join referencelist prggroup2 on prggroup2.id = products.productgroup_2
                                        left outer join referencelist prggroup3 on prggroup3.id = products.productgroup_3
                                        left outer join referencelist prggroup4 on prggroup4.id = products.productgroup_4
                                        left outer join referencelist prggroup5 on prggroup5.id = products.productgroup_5
                                        left join lateral (
                                            select customerwalkins.feedback
                                            from customerwalkins
                                            where customerwalkins.billno = bill.billno
                                              and customerwalkins.isactive = true
                                            order by customerwalkins.id desc
                                            limit 1
                                        ) cw on true
                                        {1}
                                        where bill.isactive=true and bill.billtype!=9 " +
                                        (status.parameter.downloadvoid ? "" : " and billitems.isactive=true"));

            if (status.parameter.purchaseorder)
                masterquery.Add(QueryTypes.PurchaseOrder, @"select purchaseorderitems.id,purchaseorderitems.version,
                                        purchaseorder.podate,company.name purchasename,purchaseorder.pono,
                                        supplier.code suppliercode,products.name product,
                                        purchaseorderitems.designid design,brand.name brand,
                                        coalesce(size.name, purchaseorderitems.size) size,
                                        material.name material,pattern.name pattern,sleeve.name sleeve,
                                        type.name type,colour.name color,fit.name fit,style.name style,
                                        purchaseorder.expectedate duedate,purchaseorderitems.oqty qtyordered,
                                        case purchaseorder.status
                                            when 0 then 'PENDING'
                                            when 1 then 'APPROVED'
                                            when 2 then 'REJECTED'
                                            when 3 then 'PARTIALLYCOMPLETED'
                                            when 4 then 'COMPLETED'
                                            when 11 then 'UNAPPROVED'
                                            else cast(purchaseorder.status as text)
                                        end status
                                        from purchaseorder
                                        inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id and purchaseorderitems.isactive=true
                                        left outer join supplier on supplier.id = purchaseorder.supplierid
                                        left outer join company on company.id = purchaseorder.companyid
                                        left outer join items on items.id = purchaseorderitems.itemid
                                        left outer join products on products.id = coalesce(nullif(purchaseorderitems.productid,0), items.productid)
                                        left outer join brand on brand.id = items.brandid
                                        left outer join referencelist size on size.id = coalesce(nullif(purchaseorderitems.sizeid,0), items.sizeid)
                                        left outer join referencelist style on style.id = items.styleid
                                        left outer join referencelist material on material.id = items.materialid
                                        left outer join referencelist pattern on pattern.id = items.patternid
                                        left outer join referencelist sleeve on sleeve.id = items.sleeveid
                                        left outer join referencelist type on type.id = items.typeid
                                        left outer join referencelist colour on colour.id = coalesce(nullif(purchaseorderitems.colourid,0), items.colourid)
                                        left outer join referencelist fit on fit.id = items.fitid
                                        where purchaseorder.isactive=true");

            if (status.parameter.lrentry)
                masterquery.Add(QueryTypes.LrEntry, @"select coalesce(lrinvoice.id, lrentry.id) id,
                                        coalesce(lrinvoice.version, lrentry.version) version,
                                        lrentry.lrentrydate,lrentry.lrentryno,
                                        coalesce(suppliercompany.name, supplier.name) suppliername,
                                        coalesce(nullif(lrinvoice.pono,''), purchaseorder.pono) pono,
                                        lrinvoice.invoiceno,
                                        case lrentry.status
                                            when 200 then 'New_LR'
                                            when 201 then 'LRIssue'
                                            when 202 then 'LRReceived'
                                            when 203 then 'Invoice_Preparation'
                                            when 204 then 'Items_Entering'
                                            when 205 then 'Barcode_Generating'
                                            when 206 then 'Item_Pricing'
                                            when 0 then 'Open'
                                            when 1 then 'Working'
                                            when 2 then 'Completed'
                                            when 3 then 'Cancel'
                                            when 4 then 'Approved'
                                            when 5 then 'Rejected'
                                            when 6 then 'Pending'
                                            else cast(lrentry.status as text)
                                        end bundlestatus,
                                        coalesce(lrinvoice.noofbundle, lrentry.noofbundels) bundlecount,
                                        lrentry.lrno
                                        from lrentry
                                        left outer join supplier on supplier.id = lrentry.supplierid
                                        left outer join lrinvoice on lrinvoice.lrid = lrentry.id and lrinvoice.isactive=true
                                        left outer join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
                                        left outer join purchaseorder on purchaseorder.id = lrinvoice.poid
                                        where lrentry.isactive=true");

            string baseQuery = @"select source.* from ({0}) source
                            left outer join sync_master on sync_master.tabletype = @tabletype
                            and sync_master.clientid = @clientid and sync_master.locationid=-1 and sync_master.sourceid = source.id
                            where (sync_master.id is null or source.version != sync_master.version) ";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in masterquery)
                {
                    string table = item.Key.ToString();
                    string innerQuery = string.Format(item.Value, fields, joins);
                    string query = item.Key == QueryTypes.Stock ? innerQuery : string.Format(baseQuery, innerQuery);
                    DBData bRows = new DBData();
                    bRows.type = (int)item.Key;
                    bRows.tablename = table;
                    dBRows.Add(bRows);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    if (item.Key == QueryTypes.Stock)
                        command.CommandTimeout = 0;
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item.Key;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = -1;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                    NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(command);
                    DataSet dset = new DataSet();
                    npgsqlDataAdapter.Fill(dset);
                    dset.Tables[0].TableName = table;

                    if (!InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_hyper_uan
                        && dset.Tables[0].Columns.Contains("barcode")
                        && dset.Tables[0].Columns.Contains("serialno"))
                    {
                        foreach (DataRow drow in dset.Tables[0].Rows)
                        {
                            var barcode = drow["BARCODE"] == DBNull.Value ? "" : drow["BARCODE"].ToString();
                            var serialno = drow["SERIALNO"] == DBNull.Value ? "" : drow["SERIALNO"].ToString();
                            if (!string.IsNullOrEmpty(serialno) &&
                                !string.IsNullOrEmpty(barcode))
                            {
                                drow["BARCODE"] = serialno;
                                drow["SERIALNO"] = barcode;
                            }
                        }
                    }
                    bRows.datas = Processors.DataTableCustomFormatter.Serialize(dset.Tables[0], true);
                    //bRows.data = Newtonsoft.Json.JsonConvert.SerializeObject(dset.Tables[0],new Newtonsoft.Json.JsonSerializerSettings()
                    //{
                    //    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                    //});
                }
                conn.Close();
                status.result = dBRows;
                status.hascompleted = true;
                return;
            }
            catch (Exception exp)
            {
                status.result = null;
                status.hasfailed = true;
                status.exception = exp.Message;
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "DownloadMasters", -1);
                if (conn != null)
                    conn.Close();
            }
            status.result = null;
            status.hasfailed = true;
        }

        internal bool ClearStatus(SyncManager.ProcessStatus status)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                if (status.parameter.stock)
                {
                    NpgsqlCommand command = new NpgsqlCommand(@"delete from sync_master where sync_master.tabletype = @tabletype and sync_master.clientid = @clientid ", conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)QueryTypes.Stock;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.ExecuteNonQuery();
                }
                if (status.parameter.purchase)
                {
                    NpgsqlCommand command = new NpgsqlCommand(@"delete from sync_master where sync_master.tabletype = @tabletype and sync_master.clientid = @clientid ", conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)QueryTypes.Purchase;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.ExecuteNonQuery();
                }
                if (status.parameter.sales)
                {
                    NpgsqlCommand command = new NpgsqlCommand(@"delete from sync_master where sync_master.tabletype = @tabletype and sync_master.clientid = @clientid ", conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)QueryTypes.Sales;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.ExecuteNonQuery();
                }
                if (status.parameter.purchaseorder)
                {
                    NpgsqlCommand command = new NpgsqlCommand(@"delete from sync_master where sync_master.tabletype = @tabletype and sync_master.clientid = @clientid ", conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)QueryTypes.PurchaseOrder;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.ExecuteNonQuery();
                }
                if (status.parameter.lrentry)
                {
                    NpgsqlCommand command = new NpgsqlCommand(@"delete from sync_master where sync_master.tabletype = @tabletype and sync_master.clientid = @clientid ", conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)QueryTypes.LrEntry;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = config.id;
                    command.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "ClearStatus", -1);
                if (conn != null)
                    conn.Close();
                return false;
            }
        }
    }
}