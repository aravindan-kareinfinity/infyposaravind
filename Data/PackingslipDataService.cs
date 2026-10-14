using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Threading.Tasks;
using static ElasticEmailClient.ApiTypes;
using static InfyPOS.Data.SalesorderDataService;
using static InfyPOS.Processors.OfflineClient.Bill;
using AttributeRouting.Helpers;


namespace InfyPOS.Data

{
    public class PackingslipDataService : BaseData
    {
        const string SQL_Search = @"select barcodes, packingslip.locationid, packedby, packingslip.id, salesorderid, 
packingslip.customerid, packingslip.createdby, packingslip.isactive, 
packingslip.modifiedby, packingslip.modifiedon, packingslip.createdon, 
packingslip.version, used, billid, packingslip.companyid, packingslip.sono, 
packingno,packedon, salesorder.sobookno,billno from packingslip
left outer join salesorder on salesorder.sono = packingslip.sono
 where packingslip.id=@id";


        const string SQL_Insert = @"INSERT INTO public.packingslip(barcodes, locationid, packedby, salesorderid, customerid, createdby, isactive, modifiedby, modifiedon, createdon, version, used, billid, companyid, sono, packingno,packedon, billno) VALUES(@barcodes, @locationid, @packedby, @salesorderid, @customerid, @createdby, @isactive, @modifiedby, @modifiedon, @createdon, @version, @used, @billid, @companyid, @sono, @packingno,@packedon, @billno) RETURNING id";


        const string SQL_Update = @"UPDATE public.packingslip SET barcodes=@barcodes, locationid=@locationid, packedby=@packedby, salesorderid=@salesorderid, customerid=@customerid, modifiedby=@modifiedby, modifiedon=@modifiedon, version=@version+1, used=@used, billid=@billid, companyid=@companyid, sono=@sono, packingno=@packingno, packedon=@packedon,billno=@billno where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.packingslip set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_BillNoUpdate = @"update public.packingslip set billid=@billid,billno=@billno,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where packingno=@packingno";

        public List<Packingslip> Search(PackingslipCreteria _packingslip)
        {
            List<Packingslip> lst = new List<Packingslip>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_packingslip.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _packingslip.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("packingslip.isactive", true, "=");
                    if (_packingslip.searchmode > 0)
                    {
                        switch (_packingslip.searchmode)
                        {
                            case 1:
                                qb.AddParameter("packingslip.billid", 0, ">");
                                break;
                            case 3:
                                qb.AddParameter("packingslip.billid", 0, "=");
                                break;
                        }
                        if (_packingslip.fromdate.Year > 2000)
                        {
                            qb.AddParameter("packingslip.packedon", "fromdate", _packingslip.fromdate.Date, ">=");
                            qb.AddParameter("packingslip.packedon", "todate", _packingslip.todate.Date.AddDays(1), "<");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(_packingslip.packingno))
                            qb.AddParameter("packingslip.packingno", _packingslip.packingno, "=");

                        if (_packingslip.locationid != 0)
                            qb.AddParameter("packingslip.locationid", _packingslip.locationid, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Packingslip opackingslip = new Packingslip();
                        if (_packingslip.searchmode == 0)
                            opackingslip.barcodes_json = reader["barcodes"] == DBNull.Value ? null : reader["barcodes"].ToString();
                        opackingslip.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opackingslip.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        opackingslip.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opackingslip.salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        opackingslip.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        opackingslip.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opackingslip.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opackingslip.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opackingslip.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opackingslip.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opackingslip.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);

                        opackingslip.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opackingslip.used = reader["used"] == DBNull.Value ? false : Convert.ToBoolean(reader["used"]);
                        opackingslip.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        opackingslip.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opackingslip.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        opackingslip.packingno = reader["packingno"] == DBNull.Value ? "" : reader["packingno"].ToString();
                        opackingslip.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        opackingslip.sobookno = reader["sobookno"] == DBNull.Value ? "" : reader["sobookno"].ToString();

                        lst.Add(opackingslip);
                    }
                }

                if (lst.Count == 1 && !string.IsNullOrEmpty(_packingslip.packingno) &&
                    !string.IsNullOrEmpty(lst[0].sono))
                {
                    lst[0].othersolist = new List<string>();
                    var param = new SortedList<string, object>();
                    param.Add("sono", lst[0].sono);
                    using (var reader = ExecuteReader("select packingno from packingslip where sono=@sono and isactive=true and billno = ''", param))
                    {
                        while (reader.Read())
                        {
                            lst[0].othersolist.Add(reader.GetString(0));
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packingslip);
            }
            return lst;
        }



        public Packingslip Print(PackingslipCreteria _packingslip)
        {
            var pslip = Search(_packingslip).First();
            CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            ReferencelistDataService locationservice = new ReferencelistDataService() { Context = this.Context };
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            var allbarcodes = pslip.barcodes.ConvertAll(x => x.barcode);
            var barcodeuids = mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
            {
                uids = allbarcodes
            });

            allbarcodes.RemoveAll(x => barcodeuids.Exists(y => y.uid == x));

            if (allbarcodes.Count > 0)
            {
                SellablebarcodesDataService sellablebarcodesDataService = new SellablebarcodesDataService() { Context = this.Context };
                var uanbarcodes = sellablebarcodesDataService.Search(new SellablebarcodesCreteria()
                {
                    ids = allbarcodes
                });
                uanbarcodes.ForEach(x =>
                {
                    pslip.barcodes.FindAll(y => y.barcode == x.barcode).ForEach(z => z.uid = x.corebarcode);
                });
                barcodeuids.AddRange(mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
                {
                    uids = uanbarcodes.ConvertAll(x => x.corebarcode)
                }));
            }

            pslip.barcodes.FindAll(x => string.IsNullOrEmpty(x.uid)).ForEach(x => x.uid = x.barcode);

            var boxlist = locationservice.SearchList(new ReferencelistCreteria()
            {
                ids = pslip.barcodes.ConvertAll(x => x.soboxid).Distinct().ToList()
            });

            var colourlist = locationservice.SearchList(new ReferencelistCreteria()
            {
                ids = barcodeuids.ConvertAll(x => x.colourid).Distinct().ToList()
            });

            var sizelist = locationservice.SearchList(new ReferencelistCreteria()
            {
                ids = barcodeuids.ConvertAll(x => x.sizeid).Distinct().ToList()
            });

            var products = productsDataService.SearchList(new ProductsCreteria()
            {
                ids = barcodeuids.ConvertAll(x => x.productid).Distinct().ToList()
            });

            int index = 1;
            pslip.barcodes.ForEach(u =>
            {
                u.sno = index++;
                u.productname = products.Exists(x => x.id == u.productid) ? products.Find(x => x.id == u.productid).name : "";
                u.sizename = sizelist.Exists(x => x.id == u.sizeid) ? sizelist.Find(x => x.id == u.sizeid).name : "";
                u.colourname = colourlist.Exists(x => x.id == u.colourid) ? colourlist.Find(x => x.id == u.colourid).name : "";
                u.hsncode = products.Exists(x => x.id == u.productid) ? products.Find(x => x.id == u.productid).hsncode : "";
                u.uanitem = barcodeuids.Exists(x => x.uid == u.uid) ? barcodeuids.Find(x => x.uid == u.uid) : null;
                u.boxname = boxlist.Exists(x => x.id == u.soboxid) ? boxlist.Find(x => x.id == u.soboxid).name : "";
            });


            pslip.company = companyservice.Search(new CompanyCreteria() { id = pslip.companyid }).First();
            if (pslip.company.cityid > 0)
            {
                var cityinfo = locationservice.Search(new ReferencelistCreteria()
                {
                    id = pslip.company.cityid
                });
                if (cityinfo.Count > 0)
                {
                    pslip.company.cityname = cityinfo[0].name;
                }
            }
            if (pslip.company.stateid > 0)
            {
                var stateinfo = locationservice.Search(new ReferencelistCreteria()
                {
                    id = pslip.company.stateid
                });
                if (stateinfo.Count > 0)
                {
                    pslip.company.statename = stateinfo[0].name;
                    pslip.company.statecode = stateinfo[0].code;
                }
            }

            pslip.location = new CompanyLocation(locationservice.Search(new ReferencelistCreteria() { id = pslip.locationid }).First());
            if (pslip.location.cityid > 0)
            {
                var cityinfo = locationservice.Search(new ReferencelistCreteria()
                {
                    id = pslip.location.cityid
                });
                if (cityinfo.Count > 0)
                {
                    pslip.location.cityname = cityinfo[0].name;
                }
            }
            if (pslip.location.stateid > 0)
            {
                var stateinfo = locationservice.Search(new ReferencelistCreteria()
                {
                    id = pslip.location.stateid
                });
                if (stateinfo.Count > 0)
                {
                    pslip.location.statename = stateinfo[0].name;
                    pslip.location.statecode = stateinfo[0].code;
                }
            }
            if (pslip.customerid != 0)
            {
                pslip.customer = customerDataService.Search(new CustomerCreteria() { id = pslip.customerid }).First();
                if (pslip.customer.preferredtransportid > 0)
                {
                    TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                    pslip.transport = transportDataService.Search(new TransportCreteria() { id = pslip.customer.preferredtransportid }).First();
                }
            }
            int sno = 1;
            pslip.SizeSummary = pslip.barcodes.GroupBy(x => new { x.productname, x.sizename, x.boxname, x.price, x.fprice, x.mrp }).ToList()
                .ConvertAll(y => new Packingslip.Barcodes()
                {
                    productname = y.Key.productname,
                    sizename = y.Key.sizename,
                    boxname = y.Key.boxname,
                    price = y.Key.price,
                    fprice = y.Key.fprice,
                    uanitem = y.First().uanitem,
                    mrp = y.Key.mrp,
                    qty = y.Sum(X => X.qty)
                }).OrderBy(x => x.productname).ThenBy(y => y.sizename).ToList();

            pslip.SizeSummary.ForEach(x => x.sno = sno++);
            return pslip;
        }

        public Packingslip Insert(Packingslip oPackingslip)
        {
            if (oPackingslip.packedon.Date == DateTimeNow.Date && oPackingslip.packedon.Hour == 0)
                oPackingslip.packedon = DateTimeNow;

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("packingslip.packingno",
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisesopackingno ?
                        oPackingslip.locationid : 0);
                    oPackingslip.packingno = sequence.GetNextNumber(Context, conn, transaction);
                    Insert(transaction, conn, oPackingslip);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackingslip);
            }

            return oPackingslip;
        }

        public Packingslip Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Packingslip oPackingslip)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = oPackingslip.barcodes_json == null ? "null" : oPackingslip.barcodes_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.locationid;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.packedby;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.salesorderid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.customerid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackingslip.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.createdon;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.packedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPackingslip.version;
            command.Parameters.Add("used", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackingslip.used;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.billid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.companyid;
            command.Parameters.Add("sono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackingslip.sono == null ? "" : oPackingslip.sono;
            command.Parameters.Add("packingno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackingslip.packingno == null ? "" : oPackingslip.packingno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackingslip.billno == null ? "" : oPackingslip.billno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPackingslip.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oPackingslip;
        }

        public Packingslip Update(Packingslip oPackingslip)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPackingslip);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackingslip);
            }

            return oPackingslip;
        }

        public Packingslip Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Packingslip oPackingslip)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("barcodes", NpgsqlTypes.NpgsqlDbType.Json).Value = oPackingslip.barcodes_json == null ? "null" : oPackingslip.barcodes_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.locationid;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.packedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.id;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.salesorderid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.customerid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackingslip.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.createdon;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.packedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPackingslip.version;
            command.Parameters.Add("used", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackingslip.used;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.billid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.companyid;
            command.Parameters.Add("sono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackingslip.sono == null ? "" : oPackingslip.sono;
            command.Parameters.Add("packingno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackingslip.packingno == null ? "" : oPackingslip.packingno;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPackingslip.billno == null ? "" : oPackingslip.billno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPackingslip.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oPackingslip;
        }


        public bool Delete(Packingslip oPackingslip)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPackingslip);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPackingslip);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Packingslip oPackingslip)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPackingslip.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPackingslip.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPackingslip.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPackingslip.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<Barcodedgoods> GenerateCombo(ComboPack item)
        {

            if(item.comboitems == null ||
                item.comboitems.Count == 0 )
            {
                throw new Exception("Please select combo item barcode before generating combo set");
            }

            if (item.comboitems.Exists(x=>string.IsNullOrEmpty(x.uid)))
            {
                throw new Exception("Combo Set UAN Number is missing");
            }

            if (item.comboitems.Exists(x =>
            {
                if(x.items == null || x.items.Count == 0 )
                {
                    return true;
                }
                return (x.items.Exists(y => string.IsNullOrEmpty(y.barcode)));
            }))
            {
                throw new Exception("Combo items UAN Number is missing");
            }

            List<string> allbarcodes = new List<string>();

            item.comboitems.ForEach(x =>
            {
                allbarcodes.Add(x.uid);
                x.items.ForEach(y =>
                {
                    allbarcodes.Add(y.barcode);
                });
            });

            allbarcodes = allbarcodes.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();

            SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
            StockDataService stockDataService = new StockDataService() { Context = this.Context };
            List<Stock> availablestockbarcodes = new List<Stock>();
            var availablebarcodes = salablegoodsDataService.Search(new SalablegoodsCreteria()
            {
                barcodes = allbarcodes,
                locationid = item.locationid
            });

            allbarcodes.RemoveAll(x => availablebarcodes.Exists(y => y.barcode == x));
            if (allbarcodes.Count > 0)
            {
                availablestockbarcodes = stockDataService.Search(new StockCreteria()
                {
                    barcodes = allbarcodes.ConvertAll(x => "'" + x + "'")
                });
                allbarcodes.RemoveAll(x => availablestockbarcodes.Exists(y => y.barcode == x));
            }

            var disableNetative = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_negativestock;


            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    if (allbarcodes.Count > 0)
                    {
                        MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                        var alldesigns = mfgdesignDataService.SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
                        {
                            uids = allbarcodes
                        });

                        var productids = alldesigns.ConvertAll(x => x.productid).Distinct().ToList();
                        ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                        var products = productsDataService.SearchList(new ProductsCreteria() { ids = productids });
                        foreach (var uan in allbarcodes)
                        {
                            var design = alldesigns.Find(x => x.uid == uan);
                            Stock stock = new Stock();
                            stock.barcode = uan;
                            stock.itemid = design.itemid;
                            stock.isuan = true;
                            stock.price = design.wsrate;
                            stock.eprice = design.wsrate;
                            stock.version = 1;
                            stock.isactive = true;
                            stock.companyid = item.companyid;
                            stock.createdby = Context.UserID;
                            stock.modifiedby = Context.UserID;
                            stock.createdon = DateTimeNow;
                            stock.modifiedon = DateTimeNow;
                            stock.dealerprice = design.dealerprice;
                            stock.productid = design.productid;
                            stock.designid = design.code;
                            stock.basebuyingprice = design.cost;
                            stock.hsncode = products.Find(x => x.id == design.productid).hsncode;
                            stock.printingname = products.Find(x => x.id == design.productid).name;
                            availablestockbarcodes.Add(stockDataService.Insert(transaction, conn, stock));
                        }
                    }

                    if (availablestockbarcodes.Count > 0)
                    {
                        var missingbarcodes = salablegoodsDataService.Search(new SalablegoodsCreteria()
                        {
                            barcodes = availablestockbarcodes.ConvertAll(x => x.barcode),
                            locationid = item.locationid
                        });

                        if (missingbarcodes.Count > 0)
                        {
                            availablestockbarcodes.RemoveAll(x => missingbarcodes.Exists(y => y.barcode == x.barcode));
                            availablebarcodes.AddRange(missingbarcodes);
                        }
                    }
                    foreach (Stock stock in availablestockbarcodes)
                    {
                        Salablegoods salablegoods = new Salablegoods()
                        {
                            isactive = true,
                            version = 1,
                            modifiedby = Context.UserID,
                            createdby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            barcode = stock.barcode,
                            supplierid = stock.supplierid,
                            stockid = stock.id,
                            itemid = stock.itemid,
                            companyid = item.companyid,
                            locationid = item.locationid
                        };
                        if (salablegoodsDataService.Insert(transaction, conn, salablegoods))
                            availablebarcodes.Add(salablegoods);
                    }


                    string sql_increatestock = "update salablegoods set iqty=iqty+@qty where id=@id RETURNING iqty-(oqty+dqty+jqty),barcode";
                    string sql_decreasestock = "update salablegoods set iqty=iqty-@qty,cbqty=cbqty+@qty where id=@id RETURNING iqty-(oqty+dqty+jqty),barcode";
                    SortedList<string, object> param = new SortedList<string, object>();
                    item.comboitems.ForEach(x =>
                    {
                        x.id = availablebarcodes.Find(z => z.barcode == x.uid).id;
                        param.Clear();
                        param.Add("qty", x.qty);
                        param.Add("id", x.id);
                        decimal availablestock = 0;
                        using (var reader = ExecuteReader(sql_increatestock, param, conn, transaction))
                        {
                            if (reader.Read())
                            {
                                availablestock = reader.GetDecimal(0);
                            }
                        }
                        x.items.ForEach(y =>
                        {
                            y.id = availablebarcodes.Find(z => z.barcode == y.barcode).id;
                            param.Clear();
                            param.Add("qty", y.qty);
                            param.Add("id", y.id);

                            using (var reader = ExecuteReader(sql_decreasestock, param, conn, transaction))
                            {
                                if (reader.Read())
                                {
                                    availablestock = reader.GetInt32(0);
                                }
                            }

                            if (availablestock < 0 && disableNetative)
                            {
                                throw new Exception(y.barcode + " item don't have required stock (" + (availablestock * -1).ToString() + ")");
                            }
                        });
                    });

                    SalablegoodshistoryDataService stockhistoryDataService = new SalablegoodshistoryDataService() { Context = this.Context };

                    item.comboitems.ForEach(x =>
                    {
                        var salablegoodsid = availablebarcodes.Find(z => z.barcode == x.uid).id;
                        stockhistoryDataService.Insert(transaction, conn, new Salablegoodshistory()
                        {
                            actiontype = Salablegoodshistory.ActionType.ComboSet,
                            referenceid = item.pono,
                            barcode = x.uid,
                            mbarcode = x.uid,
                            auditqty = x.qty * -1,
                            salablegoodsid = salablegoodsid,
                            isactive = true,
                            createdby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedby = Context.UserID,
                            modifiedon = DateTimeNow
                        });

                        x.items.ForEach(y =>
                        {
                            salablegoodsid = availablebarcodes.Find(z => z.barcode == y.barcode).id;
                            stockhistoryDataService.Insert(transaction, conn, new Salablegoodshistory()
                            {
                                actiontype = Salablegoodshistory.ActionType.ComboItem,
                                referenceid = item.pono,
                                barcode = y.barcode,
                                mbarcode = x.uid,
                                auditqty = x.qty,
                                salablegoodsid = salablegoodsid,
                                isactive = true,
                                createdby = Context.UserID,
                                createdon = DateTimeNow,
                                modifiedby = Context.UserID,
                                modifiedon = DateTimeNow
                            });

                        });
                    });
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            var items = salablegoodsDataService.BarcodeSearch(item.locationid, item.comboitems.ConvertAll(x => x.uid),
            new List<SalesorderDataService.SearchBarcodeData.itemqty>(), false, true,true);

            if (items.Count > 0)
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                {
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                    var uanitems = mfgdesignDataService.SearchUANItem(items.ConvertAll(x => x.barcode));
                    foreach (var bitem in items)
                    {
                        if (uanitems.Exists(x => x.uid == bitem.barcode))
                        {
                            var uanitem = uanitems.Find(x => x.uid == bitem.barcode);
                            if (uanitem != null)
                            {

                                bitem.price = uanitem.salerate;
                                bitem.groupkey = uanitem.name;
                                bitem.priceextax = uanitem.salerate;
                                bitem.wsrateextax = uanitem.wsrate;
                                bitem.dealerpriceextax = uanitem.dealerprice;
                                bitem.wsrate = uanitem.wsrate;
                                bitem.dealerprice = uanitem.dealerprice;
                                bitem.mrp = uanitem.mrp;

                                bitem.fgboxid = uanitem.boxid;
                                bitem.soboxid = uanitem.boxid;
                            }
                        }
                    }
                }
            }
            return items;
        }

        public Packingslip UpdateBox(long packingid)
        {
            string query = @"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,cast(schemelist->>'cost' as numeric) dcost,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,cast(schemelist->>'sleeveid' as bigint) sleeveid,
mfgdesign.productid,mfgdesign.brandid,cast(schemelist->>'itemid' as bigint) itemid,mfgdesign.boxid,
mfgdesign.fitid,mfgdesign.styleid,mfgdesign.materialid ,mfgdesign.patternid,mfgdesign.typeid 
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true 
),packinglist as
(
select packingslip.id,packingslip.PACKINGNO,schemelist->>'itemdesigncode' itemdesigncode,
	split_part(schemelist->>'itemdesigncode','/',1) designcode,
	schemelist->>'barcode' barcode,schemelist->>'soboxid' soboxid,
	schemelist->>'fgboxid' fgboxid from packingslip
LEFT JOIN LATERAL
json_array_elements (cast(packingslip.barcodes as json)) schemelist ON true
)


select distinct referencelist.name,dbox.name,packinglist.barcode,PACKINGNO,packinglist.id,packinglist.fgboxid,packinglist.soboxid,alluid.boxid,alluid.wsrate from packinglist
left outer join Sellablebarcodes on Sellablebarcodes.barcode = packinglist.barcode
left outer join alluid on alluid.uid = (case when Sellablebarcodes.id is not null then Sellablebarcodes.corebarcode else packinglist.barcode end)
left outer join referencelist on referencelist.id = cast(soboxid as bigint)
left outer join referencelist dbox on dbox.id = cast(boxid as bigint)
LEFT OUTER JOIN MFGDESIGN ON MFGDESIGN.CODE =alluid.code
where packinglist.id=@packingid and cast(fgboxid as bigint) != MFGDESIGN.boxid and MFGDESIGN.boxid>0";
            var param = new SortedList<string, object>();
            param.Add("packingid", packingid);
            var tble = ExecuteDataSet(query, param).Tables[0];
            var dataList = (from System.Data.DataRow dr in tble.Rows
                            select new PackingslipBox()
                            {
                                packingcode = dr["PACKINGNO"].ToString().ToUpper(),
                                barcode = dr["BARCODE"].ToString().ToUpper(),
                                soboxid = dr["soboxid"] == DBNull.Value ? 0 : Convert.ToInt64(dr["soboxid"]),
                                fgboxid = dr["fgboxid"] == DBNull.Value ? 0 : Convert.ToInt64(dr["fgboxid"]),
                                boxid = dr["boxid"] == DBNull.Value ? 0 : Convert.ToInt64(dr["boxid"]),
                                wsrate = dr["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["wsrate"]),
                            }).ToList();

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var adcharge = referencelistDataService.Search(new ReferencelistCreteria()
            {
                type = "ADDITIONALCHARGES"
            });

            bool result = false;


            var packing = Search(new PackingslipCreteria()
            {
                id = packingid
            }).First();
            foreach (var itm in dataList)
            {
                if (string.IsNullOrEmpty(itm.barcode)) continue;
                if (itm.fgboxid == itm.soboxid)
                {
                    packing.barcodes.Find(x => x.barcode == itm.barcode).fgboxid = itm.boxid;
                    packing.barcodes.Find(x => x.barcode == itm.barcode).soboxid = itm.boxid;
                }
                else
                {
                    packing.barcodes.Find(x => x.barcode == itm.barcode).fgboxid = itm.boxid;
                }

                //1IJ590052526005
                if (packing.barcodes.Find(x => x.barcode == itm.barcode).fgboxid !=
                    packing.barcodes.Find(x => x.barcode == itm.barcode).soboxid)
                {
                    var price = itm.wsrate;
                    var adr = adcharge.Find(x => x.id == packing.barcodes.Find(y => y.barcode == itm.barcode).fgboxid);
                    if (adr != null)
                    {
                        price -= adr.additionalinfo.billdiscount;
                    }
                    adr = adcharge.Find(x => x.id == packing.barcodes.Find(y => y.barcode == itm.barcode).soboxid);
                    if (adr != null)
                    {
                        price += adr.additionalinfo.maxlimit;
                    }
                    packing.barcodes.Find(x => x.barcode == itm.barcode).fprice = price;
                    //$scope.BoxList.push({ 'id': v.id, 'name': v.name, 'add': v.additionalinfo.maxlimit, 'remove': v.additionalinfo.billdiscount });
                }
                else
                {
                    packing.barcodes.Find(x => x.barcode == itm.barcode).fprice = itm.wsrate;
                }
            }
            packing.billid = 0;
            packing.billno = "";
            return Update(packing);
        }
        internal bool UpdateBill(Bill oBill, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            //const string SQL_BillNoUpdate = @"update public.packingslip set billid=@billid,billno=@billno,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where packingno=@packingno";
            int successCount = 0;
            oBill.billattributes.packslipids = oBill.billattributes.packslipid.Split(',');
            foreach (var item in oBill.billattributes.packslipids)
            {
                NpgsqlCommand command = new NpgsqlCommand(SQL_BillNoUpdate, conn, transaction);
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                command.Parameters.Add("packingno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item;
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;
                command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.billno;
                successCount = command.ExecuteNonQuery();
            }
            return successCount > 0;
        }
        internal List<PSSummary> SummaryReport(PackingslipCreteria _packingslip)
        {

            /*
WITH ALLPACKINGSLIPS AS
(
SELECT token PCKNO,BILLNO,BILL.ID BILLID FROM BILL,unnest(string_to_array(BILLATTRIBUTES->>'packslipid', ',')) token
WHERE BILLTYPE != 9
),UPDATEPCK AS
(
SELECT ALLPACKINGSLIPS.*,packingslip.ID PCKID FROM packingslip
INNER JOIN ALLPACKINGSLIPS ON ALLPACKINGSLIPS.PCKNO = packingno
WHERE packingslip.BILLID = 0
)

UPDATE packingslip SET BILLID = UPDATEPCK.BILLID,BILLNO = UPDATEPCK.BILLNO FROM UPDATEPCK
WHERE UPDATEPCK.PCKID = packingslip.ID
*/

            List<PSSummary> lst = new List<PSSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"
with sopkqty as 
(
select sum(cast(schemelist->>'oqty' as numeric)) oqty,
sum(cast(schemelist->>'qty' as numeric)) qty,packingslip.id from packingslip
LEFT JOIN LATERAL
json_array_elements (cast(packingslip.barcodes as json)) schemelist ON true
where packingslip.packedon>=@fromdate and packingslip.packedon<@todate and isactive=true
group by packingslip.id
),sopkfullqty as 
(
select sono,
sum(cast(schemelist->>'qty' as numeric)) pqty from packingslip
LEFT JOIN LATERAL
json_array_elements (cast(packingslip.barcodes as json)) schemelist ON true
where packingslip.isactive=true and billid = 0
group by sono
),sopending as
(
select sum(oqty-(bqty+rqty+cqty)) sopendingqty,salesorder.sono from salesorder
inner join salesorderitems on salesorderitems.salesorderid = salesorder.id
inner join sopkfullqty on sopkfullqty.sono =salesorder.sono
group by salesorder.sono
)
select packingslip.id,packingno,customer.name customer,packingslip.sono,billno,packedon,
employee.name createdby,packingslip.packedby,sopkfullqty.pqty,sopending.sopendingqty,
emodify.name modifiedby,sopkqty.oqty,sopkqty.qty from packingslip

left outer join sopkqty on sopkqty.id = packingslip.id
left outer join sopending on sopending.sono = packingslip.sono
left outer join sopkfullqty on sopkfullqty.sono = packingslip.sono
left outer join customer on customer.id = packingslip.customerid
left outer join users on users.id = packingslip.createdby
left outer join employee on employee.id = users.employeeid
left outer join users modify on modify.id = packingslip.modifiedby
left outer join employee emodify on emodify.id = modify.employeeid";
                qb.AddParameter("packingslip.isactive", true, "=");
                if (_packingslip.searchmode > 0)
                {
                    switch (_packingslip.searchmode)
                    {
                        case 1:
                            qb.AddParameter("packingslip.billid", 0, ">");
                            break;
                        case 3:
                            qb.AddParameter("packingslip.billid", 0, "=");
                            break;
                    }
                }

                qb.AddParameter("packingslip.packedon", "fromdate", _packingslip.fromdate.Date, ">=");
                qb.AddParameter("packingslip.packedon", "todate", _packingslip.todate.Date.AddDays(1), "<");


                if (!string.IsNullOrEmpty(_packingslip.packingno))
                    qb.AddParameter("packingno", _packingslip.packingno, "=");

                if (_packingslip.locationid != 0)
                    qb.AddParameter("packingslip.locationid", _packingslip.locationid, "=");

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PSSummary opackingslip = new PSSummary();
                        opackingslip.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opackingslip.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        opackingslip.createdby = reader["createdby"] == DBNull.Value ? "" : Convert.ToString(reader["createdby"]);
                        opackingslip.modifiedby = reader["modifiedby"] == DBNull.Value ? "" : Convert.ToString(reader["modifiedby"]);
                        opackingslip.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                        opackingslip.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        opackingslip.packingno = reader["packingno"] == DBNull.Value ? "" : reader["packingno"].ToString();
                        opackingslip.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        opackingslip.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();

                        opackingslip.packqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        opackingslip.soqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        opackingslip.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        opackingslip.sopendingqty = reader["sopendingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sopendingqty"]);
                        lst.Add(opackingslip);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packingslip);
            }
            return lst;
        }
        internal List<PSSummaryItem> ItemReport(PackingslipCreteria _packingslip)
        {
            List<PSSummaryItem> lst = new List<PSSummaryItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select packedby,barcodelist->>'itemdesigncode' itemdesigncode,
barcodelist->>'barcode' barcode,barcodelist->>'fprice' price,barcodelist->>'qty' qty,
colour.name colour,size.name size,products.name product,
packingslip.id,packingno,customer.name customer,
sono,billno,packedon,employee.name createdby,
emodify.name modifiedby
from packingslip
LEFT JOIN LATERAL
json_array_elements (cast(packingslip.barcodes as json)) barcodelist ON true
left outer join products on products.id = cast(barcodelist->>'productid' as bigint)
left outer join referencelist colour on colour.id = cast(barcodelist->>'colourid' as bigint)
left outer join referencelist size on size.id = cast(barcodelist->>'sizeid' as bigint)
left outer join referencelist box on box.id = cast(barcodelist->>'soboxid' as bigint)
left outer join customer on customer.id = packingslip.customerid
left outer join users on users.id = packingslip.createdby
left outer join employee on employee.id = users.employeeid
left outer join users modify on modify.id = packingslip.modifiedby
left outer join employee emodify on emodify.id = modify.employeeid";
                qb.AddParameter("packingslip.isactive", true, "=");
                if (_packingslip.searchmode > 0)
                {
                    switch (_packingslip.searchmode)
                    {
                        case 1:
                            qb.AddParameter("packingslip.billid", 0, ">");
                            break;
                        case 3:
                            qb.AddParameter("packingslip.billid", 0, "=");
                            break;
                    }
                }
                if (_packingslip.fromdate.Year > 2000)
                {
                    qb.AddParameter("packingslip.packedon", "fromdate", _packingslip.fromdate.Date, ">=");
                    qb.AddParameter("packingslip.packedon", "todate", _packingslip.todate.Date.AddDays(1), "<");
                }


                if (!string.IsNullOrEmpty(_packingslip.packingno))
                    qb.AddParameter("packingno", _packingslip.packingno, "=");

                if (_packingslip.locationid != 0)
                    qb.AddParameter("packingslip.locationid", _packingslip.locationid, "=");

                if (_packingslip.productid != 0)
                    qb.AddParameter("products.id", _packingslip.productid, "=");


                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PSSummaryItem opackingslip = new PSSummaryItem();
                        opackingslip.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opackingslip.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        opackingslip.createdby = reader["createdby"] == DBNull.Value ? "" : Convert.ToString(reader["createdby"]);
                        opackingslip.modifiedby = reader["modifiedby"] == DBNull.Value ? "" : Convert.ToString(reader["modifiedby"]);
                        opackingslip.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                        opackingslip.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        opackingslip.packingno = reader["packingno"] == DBNull.Value ? "" : reader["packingno"].ToString();
                        opackingslip.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        opackingslip.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();

                        opackingslip.itemdesigncode = reader["itemdesigncode"] == DBNull.Value ? "" : reader["itemdesigncode"].ToString();
                        opackingslip.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opackingslip.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        opackingslip.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        opackingslip.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        opackingslip.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opackingslip.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        lst.Add(opackingslip);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packingslip);
            }
            return lst;
        }

        internal List<PSOrphan> ComboOrphanReport(PackingslipCreteria _packingslip)
        {
            List<PSOrphan> lst = new List<PSOrphan>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"

with allaudit
as
(
select referenceid packingno,salablegoodshistory.barcode,Packingslip.packedon,actiontype,sum(auditqty) qty from salablegoodshistory
inner join Packingslip on packingslip.packingno = salablegoodshistory.referenceid
where actiontype = 4 and Packingslip.packedon>='2023-03-01' 
and Packingslip.packedon<'2023-03-21' 
group by referenceid,salablegoodshistory.barcode,actiontype,packedon
),allps as
(
select 
barcodelist->>'barcode' barcode,
sum(cast(barcodelist->>'qty' as numeric)) qty,packingno
from packingslip
LEFT JOIN LATERAL
json_array_elements (cast(packingslip.barcodes as json)) barcodelist ON true
where packingslip.isactive =true and Packingslip.packedon>='2023-03-01' 
and Packingslip.packedon<'2023-03-21'
group by packingno,barcode
),
alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
mfgdesign.productid
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true
)

select allaudit.*,abs(allps.qty),alluid.dname designid,products.name product,colour.name colour,size.name size, 
allps.qty packqty
from allaudit
inner join alluid on alluid.uid = allaudit.barcode
left outer join products on products.id = alluid.productid
left outer join referencelist colour on colour.id = alluid.colourid
left outer join referencelist size on size.id = alluid.sizeid
left outer join allps on allps.packingno = allaudit.packingno and 
allps.barcode = allaudit.barcode
where allps.qty is null or abs(allaudit.qty) != allps.qty
" + (_packingslip.productid > 0 ? " and productid=@productid" : "");


                command = qb.GetCommand(conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _packingslip.productid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _packingslip.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _packingslip.todate.Date.AddDays(1);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PSOrphan opackingslip = new PSOrphan();
                        opackingslip.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opackingslip.packingno = reader["packingno"] == DBNull.Value ? "" : reader["packingno"].ToString();
                        opackingslip.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        opackingslip.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        opackingslip.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        opackingslip.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        opackingslip.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                        opackingslip.actiontype = reader["actiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["actiontype"]);
                        opackingslip.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        opackingslip.packqty = reader["packqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packqty"]);
                        lst.Add(opackingslip);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packingslip);
            }
            return lst;
        }
        internal List<PSComboSummaryItem> ComboReport(PackingslipCreteria _packingslip)
        {
            List<PSComboSummaryItem> lst = new List<PSComboSummaryItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select packedby,barcodelist->>'itemdesigncode' itemdesigncode,
barcodelist->>'barcode' barcode,
Salablegoodshistory.barcode mbarcode,
barcodelist->>'fprice' price,barcodelist->>'qty' qty,
colour.name colour,size.name size,products.name product,
packingslip.id,packingno,customer.name customer,
sono,billno,packedon,employee.name createdby,
emodify.name modifiedby,actiontype,Salablegoodshistory.auditqty
from packingslip
LEFT JOIN LATERAL
json_array_elements (cast(packingslip.barcodes as json)) barcodelist ON true
inner join Salablegoodshistory on Salablegoodshistory.referenceid = packingno 
and Salablegoodshistory.mbarcode = barcodelist->>'barcode'
left outer join products on products.id = cast(barcodelist->>'productid' as bigint)
left outer join referencelist colour on colour.id = cast(barcodelist->>'colourid' as bigint)
left outer join referencelist size on size.id = cast(barcodelist->>'sizeid' as bigint)
left outer join referencelist box on box.id = cast(barcodelist->>'soboxid' as bigint)
left outer join customer on customer.id = packingslip.customerid
left outer join users on users.id = packingslip.createdby
left outer join employee on employee.id = users.employeeid
left outer join users modify on modify.id = packingslip.modifiedby
left outer join employee emodify on emodify.id = modify.employeeid";
                qb.AddParameter("packingslip.isactive", true, "=");
                if (_packingslip.searchmode > 0)
                {
                    switch (_packingslip.searchmode)
                    {
                        case 1:
                            qb.AddParameter("packingslip.billid", 0, ">");
                            break;
                        case 3:
                            qb.AddParameter("packingslip.billid", 0, "=");
                            break;
                    }
                }
                if (_packingslip.fromdate.Year > 2000)
                {
                    qb.AddParameter("packingslip.packedon", "fromdate", _packingslip.fromdate.Date, ">=");
                    qb.AddParameter("packingslip.packedon", "todate", _packingslip.todate.Date.AddDays(1), "<");
                }


                if (!string.IsNullOrEmpty(_packingslip.packingno))
                    qb.AddParameter("packingno", _packingslip.packingno, "=");

                if (_packingslip.locationid != 0)
                    qb.AddParameter("packingslip.locationid", _packingslip.locationid, "=");

                if (_packingslip.productid != 0)
                    qb.AddParameter("products.id", _packingslip.productid, "=");

                if (_packingslip.auditactiontype != 0)
                    qb.AddParameter("Salablegoodshistory.actiontype", _packingslip.auditactiontype, "=");

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PSComboSummaryItem opackingslip = new PSComboSummaryItem();
                        opackingslip.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opackingslip.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        opackingslip.createdby = reader["createdby"] == DBNull.Value ? "" : Convert.ToString(reader["createdby"]);
                        opackingslip.modifiedby = reader["modifiedby"] == DBNull.Value ? "" : Convert.ToString(reader["modifiedby"]);
                        opackingslip.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);
                        opackingslip.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        opackingslip.packingno = reader["packingno"] == DBNull.Value ? "" : reader["packingno"].ToString();
                        opackingslip.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        opackingslip.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();

                        opackingslip.itemdesigncode = reader["itemdesigncode"] == DBNull.Value ? "" : reader["itemdesigncode"].ToString();
                        opackingslip.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opackingslip.mbarcode = reader["mbarcode"] == DBNull.Value ? "" : reader["mbarcode"].ToString();
                        opackingslip.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        opackingslip.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        opackingslip.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        opackingslip.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opackingslip.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        opackingslip.mqty = reader["auditqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["auditqty"]);
                        opackingslip.actiontype = reader["actiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["actiontype"]);

                        lst.Add(opackingslip);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _packingslip);
            }
            return lst;
        }
    }

}
