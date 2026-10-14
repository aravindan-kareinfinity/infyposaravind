using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;
namespace InfyPOS.Data
{
    public class BOMBundlesDataService : SAASData
    {
        const string SQL_TransferHistory = @"select BOMBundles.id,BOMBundles.code,
fromlocationid,tolocationid,packedby, packedon, receivedon,receivedby, stock.id stockid,stock.barcode,
(schemelist ->> 'aqty') aqty from BOMBundles
LEFT JOIN LATERAL
json_array_elements (BOMBundles.items :: json) schemelist ON true
inner join stock ON (schemelist ->> 'stockid') :: BIGINT = stock.id
where stock.barcode = @barcode and BOMBundles.status>0";


        const string SQL_Search = @"select id, code, fromlocationid, tolocationid, packedby, receivedby, packedon, receivedon, notes, approvedby, isactive, createdon,
            modifiedon, createdby, modifiedby, status, version, companyid,items,qty,iscompanytransfer,tocompanyid,fromcompanyid,fromdepartmentid,todepartmentid,bomrequestid from BOMBundles where id=@id";

        const string SQL_Insert = @"INSERT INTO public.BOMBundles( code, fromlocationid, tolocationid, packedby, receivedby, packedon, receivedon, notes, approvedby, isactive, createdon, modifiedon, createdby, 
            modifiedby, status, version, companyid,qty,iscompanytransfer,tocompanyid,fromcompanyid,fromdepartmentid,todepartmentid,bomrequestid) 
            VALUES( @code, @fromlocationid, @tolocationid, @packedby, @receivedby, @packedon, @receivedon, @notes, @approvedby, @isactive, @createdon,
            @modifiedon, @createdby, @modifiedby, @status, @version, @companyid,@qty,@iscompanytransfer,@tocompanyid,@fromcompanyid,@fromdepartmentid,
            @todepartmentid,@bomrequestid) RETURNING id";

        const string SQL_Update = @"UPDATE public.BOMBundles SET code=@code, fromlocationid=@fromlocationid,companyid=@companyid, 
        tolocationid=@tolocationid, qty=@qty, packedby=@packedby, receivedby=@receivedby, packedon=@packedon, receivedon=@receivedon,
        notes=@notes, approvedby=@approvedby, modifiedon=@modifiedon, modifiedby=@modifiedby, status=@status, version=@version+1,
        iscompanytransfer=@iscompanytransfer,tocompanyid=@tocompanyid,fromcompanyid=@fromcompanyid,fromdepartmentid=@fromdepartmentid,
        todepartmentid=@todepartmentid,bomrequestid=@bomrequestid
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.BOMBundles set isactive=false,status=@status,version=@version+1,
                    modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version and isactive=true 
                     RETURNING version";

        const string SQL_UpdateItems = @"update public.BOMBundles set items=@items where id=@id";

        const string SQL_RejectSalableGoods = @"with outwards as
                                       (
                                        select (schemelist->> 'toid')::BIGINT id,
                                               (schemelist->> 'aqty') aqty,
                                               salablegoods.iqty,
                                               salablegoods.oqty,
                                               salablegoods.dqty,
                                               salablegoods.jqty from BOMBundles
                                        LEFT JOIN LATERAL
                                        json_array_elements(BOMBundles.items :: json) schemelist ON true
                                        inner join salablegoods ON salablegoods.id = (schemelist ->> 'toid') :: BIGINT
                                        where BOMBundles.id = @bundleid and BOMBundles.isactive = true
                                       )
                                       select id,aqty,iqty,oqty,dqty,jqty from outwards";

        const string SQL_PrintableItems = @"with outwards as
                                       (
                                        select colours.id colourid,BOMMaster.id bomid,cast((schemelist->> 'qty') as numeric) qty,(schemelist->> 'width') width,bommaster.name,
										tax.taxpercentage,cast((schemelist->> 'cost') as numeric) costvalue,BOMMaster.hsncode,
                                        cast((schemelist->> 'aqty') as numeric) aqty,referencelist.name category,BOMMaster.printingname,
										colours.name colourname,cast((schemelist->> 'bomcode') as varchar) bomcode
										from BOMBundles
                                        LEFT JOIN LATERAL
                                        json_array_elements(BOMBundles.items :: json) schemelist ON true
										left outer join BOMMaster on BOMMaster.id =  cast((schemelist->> 'bomid') as bigint)
										left outer join referencelist on referencelist.id = BOMMaster.categoryid
										left outer join referencelist colours on colours.id = cast((schemelist->> 'colourid') as bigint)
										left outer join tax on tax.id = BOMMaster.taxid
                                        where BOMBundles.id = @bundleid and BOMBundles.isactive = true
                                       )
                                       select qty ,aqty,name,category,colourname,hsncode,width,bomcode,costvalue,taxpercentage,printingname,bomid,colourid from outwards";

        const string SQL_Reject = @"with outwards as
                                       (
                                        select stock.id stockid,stock.barcode,(schemelist ->> 'aqty') aqty from BOMBundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (BOMBundles.items :: json) schemelist ON true
                                        inner join stock ON (schemelist ->> 'stockid') :: BIGINT = stock.id
                                        where BOMBundles.id=@bundleid and BOMBundles.isactive=true)

                                        update stock set transfered = (case when transfered-outwards.aqty>=0 then transfered-outwards.aqty else 0 end)
                                        from outwards
                                        where outwards.stockid = stock.id;";

        const string SQL_RejectGoods = @"with outwards as
                                       (
                                        select (schemelist ->> 'id') :: BIGINT id,(schemelist ->> 'aqty') aqty from BOMBundles
                                        LEFT JOIN LATERAL
                                        json_array_elements (BOMBundles.items :: json) schemelist ON true
                                        where BOMBundles.id=@bundleid and BOMBundles.isactive=true)

                                        update salablegoods set iqty = iqty+outwards.aqty
                                        from outwards
                                        where outwards.id = salablegoods.id;";



        internal BOMBundlesPrint Print(long id)
        {
            var BOMBundles = Search(new BOMBundlesCreteria()
            {
                id = id
            });

            if (BOMBundles == null || BOMBundles.Count == 0)
                return new BOMBundlesPrint();

            BOMBundlesPrint ret = new BOMBundlesPrint();
            ret.packedby = BOMBundles[0].packedby;
            ret.packedon = BOMBundles[0].packedon;
            ret.qty = BOMBundles[0].qty;
            ret.fromlocationid = BOMBundles[0].fromlocationid;
            ret.tolocationid = BOMBundles[0].tolocationid;
            ret.code = BOMBundles[0].code;

            ret.notes = BOMBundles[0].notes;
            ret.approvedby = BOMBundles[0].approvedby;
            ret.status = BOMBundles[0].status;
            ret.companyid = BOMBundles[0].companyid;

            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var companylist = companyservice.Search(new CompanyCreteria()
            {
                id = BOMBundles[0].fromcompanyid
            });

            if (companylist.Count == 1)
            {
                ret.company = companylist[0];
                ret.tocompany = ret.company;
            }

            if (BOMBundles[0].tocompanyid > 0)
            {
                companylist = companyservice.Search(new CompanyCreteria()
                {
                    id = BOMBundles[0].tocompanyid
                });

                if (companylist.Count == 1)
                    ret.tocompany = companylist[0];
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = ret.fromlocationid
            }).First();

            ret.fromlocation = new CompanyLocation(location);
            location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = ret.tolocationid
            }).First();

            ret.tolocation = new CompanyLocation(location);
            var param = new SortedList<string, object>();
            param.Add("@bundleid", id);
            bool isigst = (ret.company != null &&
                ret.tocompany != null &&
                ret.company.stateid != ret.tocompany.stateid);
            ret.groupitems = new List<BOMBundlesPrint.Printableitem>();
            using (var reader = ExecuteReader(SQL_PrintableItems, param))
            {
                while (reader.Read())
                {
                    BOMBundlesPrint.Printableitem printableitem = new BOMBundlesPrint.Printableitem();
                    printableitem.bom = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    printableitem.bomprintingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                    printableitem.category = reader["category"] == DBNull.Value ? "" : reader["category"].ToString();
                    printableitem.colour = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                    printableitem.bomcode = reader["bomcode"] == DBNull.Value ? "" : reader["bomcode"].ToString();
                    printableitem.hsn = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    printableitem.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                    printableitem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                    printableitem.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"].ToString());

                    printableitem.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"].ToString());
                    printableitem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());

                    printableitem.costprice = reader["costvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["costvalue"].ToString());
                    printableitem.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                    printableitem.taxable = printableitem.qty * printableitem.costprice;
                    if (isigst)
                    {
                        printableitem.igst = Math.Round(printableitem.taxable * printableitem.taxpercentage / 100, 0);
                    }
                    else
                    {
                        printableitem.cgst = printableitem.sgst = Math.Round(printableitem.taxable * (printableitem.taxpercentage / 2) / 100, 0);
                    }
                    printableitem.amount = printableitem.taxable + printableitem.igst + printableitem.cgst + printableitem.sgst;
                    printableitem.sno = ret.groupitems.Count + 1;
                    ret.groupitems.Add(printableitem);
                }
            }
            ret.inwords = WebAPI.Data.Numberconvertor.rupees(ret.netamount);

            if(BOMBundles[0].bomrequestid>0)
            {
                BomrequestDataService bomrequestDataService = new BomrequestDataService() { Context = this.Context };
                var bomrequests = bomrequestDataService.Search(new BomrequestCreteria()
                {
                    id = BOMBundles[0].bomrequestid
                });
                if(bomrequests.Count == 1)
                {
                    var requiredboms = bomrequestDataService.ImportBOM(bomrequests[0]);
                    foreach(var item in requiredboms)
                    {
                        var gitems = ret.groupitems.FindAll(x => x.bomid == item.bomid && x.colourid == item.colourid);
                        if(gitems.Count>0)
                        {
                            gitems[0].rqty = item.qty;
                        }
                    }
                }
            }
            return ret;
        }

        public List<BundleItemTransferHistory> SearchHistory(string barcode, long companyid)
        {
            List<BundleItemTransferHistory> list = new List<BundleItemTransferHistory>();
            var param = new SortedList<string, object>();
            param.Add("barcode", barcode);
            string query = SQL_TransferHistory;
            if (companyid > 0)
            {
                query = query.Replace("stock.barcode = @barcode", "stock.barcode = @barcode and stock.companyid=@companyid");
                param.Add("companyid", companyid);
            }

            using (var reader = ExecuteReader(SQL_TransferHistory, param))
            {
                while (reader.Read())
                {
                    BundleItemTransferHistory item = new BundleItemTransferHistory();
                    item.code = reader["code"] != DBNull.Value ? reader["code"].ToString() : "";
                    item.id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                    item.fromlocationid = reader["fromlocationid"] != DBNull.Value ? Convert.ToInt64(reader["fromlocationid"]) : 0;
                    item.tolocationid = reader["tolocationid"] != DBNull.Value ? Convert.ToInt64(reader["tolocationid"]) : 0;
                    item.packedon = reader["packedon"] != DBNull.Value ? Convert.ToDateTime(reader["packedon"]) : DateTime.MinValue;
                    item.receivedon = reader["receivedon"] != DBNull.Value ? Convert.ToDateTime(reader["receivedon"]) : DateTime.MinValue;
                    item.qty = reader["aqty"] != DBNull.Value ? Convert.ToInt64(reader["aqty"]) : 0;
                    list.Add(item);
                }
            }
            return list;
        }

        internal BOMBundles Inward(BOMBundles oBOMBundles)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oBOMBundles.items.All(x => x.qty == x.aqty))
                        oBOMBundles.Status = BOMBundles.statuslist.Inwarded;
                    else
                        oBOMBundles.Status = BOMBundles.statuslist.InwardedWithChanges;
                    Update(transaction, conn, oBOMBundles);
                    NpgsqlCommand command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMBundles.items_json;
                    command.ExecuteNonQuery();
                    BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                    bomstocktransactionDataService.Insert(Bomstocktransaction.TransactionType.TransferIn,
                        conn, transaction, oBOMBundles, oBOMBundles.items);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                oBOMBundles.id = 0;
                oBOMBundles.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMBundles);
            }

            return oBOMBundles;
        }


        public List<BOMBundles> Search(BOMBundlesCreteria _BOMBundles)
        {
            List<BOMBundles> lst = new List<BOMBundles>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMBundles.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMBundles.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    //qb.AddParameter("returngoods", isgoods, "=");

                    if (!string.IsNullOrEmpty(_BOMBundles.code))
                        qb.AddParameter("code", _BOMBundles.code, "=");
                    if (_BOMBundles.fromcompanyid > 0)
                        qb.AddParameter("fromcompanyid", _BOMBundles.fromcompanyid, "=");
                    if (_BOMBundles.tocompanyid > 0)
                        qb.AddParameter("tocompanyid", _BOMBundles.tocompanyid, "=");
                    if (_BOMBundles.fromlocationid > 0)
                        qb.AddParameter("fromlocationid", _BOMBundles.fromlocationid, "=");
                    if (_BOMBundles.bomrequestid > 0)
                        qb.AddParameter("bomrequestid", _BOMBundles.bomrequestid, "=");
                    if (_BOMBundles.tolocationid > 0)
                        qb.AddParameter("tolocationid", _BOMBundles.tolocationid, "=");
                    if (_BOMBundles.fromdate.Year > 2000)
                        qb.AddParameter("packedon", "fromdate", _BOMBundles.fromdate, " >= ");
                    if (_BOMBundles.todate.Year > 2000)
                        qb.AddParameter("packedon", "todate", _BOMBundles.todate.AddDays(1).Date, " < ");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMBundles oBOMBundles = new BOMBundles();
                        oBOMBundles.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        oBOMBundles.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();

                        oBOMBundles.tocompanyid = reader["tocompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tocompanyid"].ToString());
                        oBOMBundles.fromcompanyid = reader["fromcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromcompanyid"].ToString());
                        oBOMBundles.fromdepartmentid = reader["fromdepartmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromdepartmentid"].ToString());
                        oBOMBundles.todepartmentid = reader["todepartmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["todepartmentid"].ToString());
                        oBOMBundles.bomrequestid = reader["bomrequestid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomrequestid"].ToString());
                        oBOMBundles.iscompanytransfer = reader["iscompanytransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscompanytransfer"]);

                        oBOMBundles.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        oBOMBundles.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"].ToString());
                        oBOMBundles.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"].ToString());
                        oBOMBundles.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"].ToString());
                        oBOMBundles.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"].ToString());
                        oBOMBundles.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"].ToString());
                        oBOMBundles.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        oBOMBundles.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"].ToString());
                        oBOMBundles.qty = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        oBOMBundles.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMBundles.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMBundles.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMBundles.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMBundles.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMBundles.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oBOMBundles.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"].ToString());
                        oBOMBundles.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oBOMBundles.items_json = reader["items"] == DBNull.Value ? "null" : reader["items"].ToString();
                        //oBOMBundles.ToObject();
                        lst.Add(oBOMBundles);
                    }
                }
                conn.Close();
                if(lst.Count == 0 && _BOMBundles.bomrequestid>0)
                {
                    BomrequestDataService bomrequestDataService = new BomrequestDataService() { Context = this.Context };
                    var request = bomrequestDataService.Search(new BomrequestCreteria()
                    {
                        id = _BOMBundles.bomrequestid
                    }).First();
                    BOMBundles bOMBundles = new BOMBundles();
                    bOMBundles.companyid = request.companyid;
                    bOMBundles.fromlocationid = request.fromlocationid;
                    bOMBundles.tolocationid = request.tolocationid;
                    bOMBundles.bomrequestid = request.id;
                    bOMBundles.isactive = true;
                    bOMBundles.items = new List<BOMBundlesitem>();
                    lst.Add(bOMBundles);
                    foreach (var item in request.items)
                    {
                        bOMBundles.items.Add(new BOMBundlesitem()
                        {
                            bomcategoryid = item.bomcategoryid,
                            bomid = item.bomid,
                            qty = item.qty,
                            colourid = item.colourid,
                            bomcode = item.bomcode,
                            cost = item.rate,
                            imageid = item.imageid,
                            width = item.width
                        });
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMBundles);
            }

            
            return lst;
        }


        public BOMBundles Insert(BOMBundles oBOMBundles)
        {
            oBOMBundles.status = 0;
            NpgsqlTransaction transaction = null;
            try
            {
                //if (!Validate(oBOMBundles))
                //    return oBOMBundles;

                if (oBOMBundles.fromcompanyid == 0 && oBOMBundles.companyid > 0)
                {
                    oBOMBundles.fromcompanyid = oBOMBundles.companyid;
                    oBOMBundles.iscompanytransfer = (oBOMBundles.tocompanyid > 0 && oBOMBundles.fromcompanyid != oBOMBundles.tocompanyid);
                }
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("BOMBundles.code", 0);
                //oBOMBundles.ToJSON();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_randombundlecode)
                    {
                        InfyPOS.Processors.RandomManager randomManager = new Processors.RandomManager(6, Processors.RandomManager.Type.AlphaNumeric, "code", "BOMBundles");
                        oBOMBundles.code = randomManager.Generate();
                    }
                    else
                    {
                        oBOMBundles.code = sequence.GetNextNumber(Context, conn, transaction);
                    }
                    Insert(transaction, conn, oBOMBundles);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_inward)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Inward,
                                oBOMBundles.id, string.Format("BUNDLE INWARD REFERENCE NO {0} ", oBOMBundles.code), Context.UserID, oBOMBundles.tolocationid, transaction,
                                Context);
                    }

                    long itemid = oBOMBundles.id * 5000;
                    oBOMBundles.items.ForEach(x =>
                    {
                        x.id = itemid++;
                    });
                    command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMBundles.items_json;
                    command.ExecuteNonQuery();

                    BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                    bomstocktransactionDataService.Insert(Bomstocktransaction.TransactionType.TransferOut,
                        conn, transaction, oBOMBundles, oBOMBundles.items);

                    if(oBOMBundles.bomrequestid != 0)
                    {
                        var param = new SortedList<string, object>();
                        string query = "Update bomrequest set bundleid=@bundleid,bundlecode=@bundlecode where id=@bomrequestid";
                        param.Add("bundleid", oBOMBundles.id);
                        param.Add("bundlecode", oBOMBundles.code);
                        param.Add("bomrequestid", oBOMBundles.bomrequestid);
                        ExecuteQuery(query, param,
                            conn, transaction);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMBundles);


            }

            return oBOMBundles;
        }

        public BOMBundles Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMBundles oBOMBundles)
        {
            if (string.IsNullOrEmpty(oBOMBundles.code))
                throw new Exception("Empty bundle code not allowed");

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            //command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMBundles.items_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMBundles.code == null ? "" : oBOMBundles.code;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.fromlocationid;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.tolocationid;

            command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.tocompanyid;
            command.Parameters.Add("fromcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.fromcompanyid;
            command.Parameters.Add("fromdepartmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.fromdepartmentid;
            command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.tocompanyid;

            command.Parameters.Add("todepartmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.todepartmentid;
            command.Parameters.Add("iscompanytransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMBundles.iscompanytransfer;

            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.packedby;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMBundles.qty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.receivedby;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMBundles.packedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMBundles.receivedon;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMBundles.notes == null ? "" : oBOMBundles.notes;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.approvedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMBundles.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.companyid;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.status;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.version;
            command.Parameters.Add("bomrequestid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.bomrequestid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMBundles.id = Convert.ToInt64(reader[0]);
                }
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_inward)
            {
                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                    Services.IntrayManager.EntryType.Inward,
                    oBOMBundles.id, string.Format("BUNDLE INWARD REFERENCE NO {0} ", oBOMBundles.code),
                    Context.UserID, oBOMBundles.tolocationid, transaction, Context);
            }
            return oBOMBundles;
        }



        public BOMBundles Update(BOMBundles oBOMBundles)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                //oBOMBundles.ToJSON();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oBOMBundles);

                    long itemid = oBOMBundles.id * 5000;
                    var maxid = oBOMBundles.items.Max(x => x.id);
                    if (maxid > itemid)
                        itemid = maxid + 1;
                    oBOMBundles.items.ForEach(x =>
                    {
                        x.id = itemid++;
                    });
                    var command = GetCommand(SQL_UpdateItems, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.id;
                    command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMBundles.items_json;
                    command.ExecuteNonQuery();

                    if (oBOMBundles.bomrequestid != 0)
                    {
                        var param = new SortedList<string, object>();
                        string query = "Update bomrequest set bundleid=@bundleid,bundlecode=@bundlecode where id=@bomrequestid";
                        param.Add("bundleid", oBOMBundles.id);
                        param.Add("bundlecode", oBOMBundles.code);
                        param.Add("bomrequestid", oBOMBundles.bomrequestid);
                        ExecuteQuery(query, param,
                            conn, transaction);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMBundles);
            }
            return oBOMBundles;
        }

        public bool CanUpdate(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMBundles oBOMBundles, bool returnitems)
        {
            NpgsqlCommand command = GetCommand("Update BOMBundles set version=version+1 where id=@id and isactive = true and version=@version RETURNING version" + (returnitems ? ",items" : ""), conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.id;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMBundles.version = Convert.ToInt32(reader[0]);
                    if (returnitems)
                        oBOMBundles.items_json = reader.GetString(1);
                    return true;
                }
            }
            return false;
        }
        public BOMBundles Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMBundles oBOMBundles)
        {
            if (string.IsNullOrEmpty(oBOMBundles.code))
                throw new Exception("Empty bundle code not allowed");

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.id;
            //command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMBundles.items_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMBundles.code == null ? "" : oBOMBundles.code;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.fromlocationid;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.tolocationid;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.packedby;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.receivedby;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMBundles.packedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMBundles.receivedon;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMBundles.qty;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMBundles.notes == null ? "" : oBOMBundles.notes;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.approvedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMBundles.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.status;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.version;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.companyid;
            command.Parameters.Add("tocompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.tocompanyid;
            command.Parameters.Add("fromcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.fromcompanyid;
            command.Parameters.Add("fromdepartmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.fromdepartmentid;
            command.Parameters.Add("todepartmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.todepartmentid;
            command.Parameters.Add("bomrequestid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.bomrequestid;
            command.Parameters.Add("iscompanytransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMBundles.iscompanytransfer;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMBundles.version = Convert.ToInt32(reader[0]);
                }
            }

            return oBOMBundles;
        }

        public bool Reject(BOMBundles oBOMBundles)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (!CanUpdate(transaction, conn, oBOMBundles, true))
                        throw new Exception("Given package updated, please refresh and try again!");

                    BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                    bomstocktransactionDataService.Delete(Bomstocktransaction.TransactionType.TransferIn,
                        transaction, conn, oBOMBundles.id);

                    Update(transaction, conn, oBOMBundles);
                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMBundles);
            }
            return hasSuccessCount;

        }


        public bool Delete(BOMBundles oBOMBundles)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBOMBundles);

                    BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                    bomstocktransactionDataService.Delete(Bomstocktransaction.TransactionType.TransferOut,
                        transaction,conn, oBOMBundles.id);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMBundles);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMBundles oBOMBundles)
        {
            //if (!CanUpdate(transaction, conn, oBOMBundles, true) || (oBOMBundles.items != null && oBOMBundles.items.Exists(e => e.itemid > 0 && e.aqty > 0)))
            //{
            //    throw new Exception("Given package already transacted, can't delete");
            //}

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMBundles.isactive;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMBundles.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMBundles.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
