using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class Mfg_Swatches_FolderDataService : BaseData
    {
        const string SQL_Search = @"		select mfg_swatches_folder.swatchesid,mfg_swatches_folder.preparedby,
        mfg_swatches_folder.creationon,mfg_swatches_folder.noofcolours, mfg_swatches_folder.code, mfg_swatches_folder.qty,
        mfg_swatches_folder.status, mfg_swatches_folder.isactive, mfg_swatches_folder.version, 
        mfg_swatches_folder.modifiedon, mfg_swatches_folder.createdon, mfg_swatches_folder.createdby, 
        mfg_swatches_folder.modifiedby, mfg_swatches_folder.id,mfgdesign.code design,
		products.name productname,brand.name brandname,mfg_swatches_folder.designid from mfg_swatches_folder
		left outer join mfgdesign on mfgdesign.id = mfg_swatches_folder.designid
		left outer join products on products.id = mfgdesign.productid
		left outer join brand on brand.id = mfgdesign.brandid
        where mfg_swatches_folder.id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfg_swatches_folder(swatchesid,preparedby,qty,creationon,noofcolours, code, status, isactive, version, modifiedon, createdon, createdby, modifiedby,design,designid) 
        VALUES(@swatchesid,@preparedby,@qty,@creationon,@noofcolours, @code, @status, @isactive, @version, @modifiedon, @createdon, @createdby, @modifiedby,@design,@designid) RETURNING id";



        const string SQL_Update = @"UPDATE public.mfg_swatches_folder SET swatchesid=@swatchesid,preparedby=@preparedby,qty=@qty,creationon=@creationon,noofcolours=@noofcolours, code=@code, status=@status, 
        design=@design,designid=@designid,
        version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfg_swatches_folder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfg_Swatches_FolderDetail> Search(Mfg_Swatches_FolderCreteria _mfg_swatches_folder)
        {
            List<Mfg_Swatches_FolderDetail> lst = new List<Mfg_Swatches_FolderDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfg_swatches_folder.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfg_swatches_folder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("mfg_swatches_folder.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_mfg_swatches_folder.design))
                        qb.AddParameter("mfg_swatches_folder.design", _mfg_swatches_folder.design, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfg_Swatches_FolderDetail omfg_swatches_folder = new Mfg_Swatches_FolderDetail();
                        omfg_swatches_folder.swatchesid = reader["swatchesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["swatchesid"]);
                        omfg_swatches_folder.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        omfg_swatches_folder.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();

                        omfg_swatches_folder.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);

                        omfg_swatches_folder.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        omfg_swatches_folder.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        omfg_swatches_folder.brand = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        omfg_swatches_folder.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        omfg_swatches_folder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfg_swatches_folder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfg_swatches_folder.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_swatches_folder.noofcolours = reader["noofcolours"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofcolours"]);
                        omfg_swatches_folder.creationon = reader["creationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["creationon"]);

                        omfg_swatches_folder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfg_swatches_folder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfg_swatches_folder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfg_swatches_folder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfg_swatches_folder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(omfg_swatches_folder);
                    }
                }

                if(_mfg_swatches_folder.showdetail && lst.Count>0)
                {
                    string salesmencondtion = _mfg_swatches_folder.salesmenid>0 ?(" and referenceid=" + _mfg_swatches_folder.salesmenid):"";

                    string query = string.Format(@"select folderid,
                                    sum(Mfg_Swatches_History.qty) qty,transactiontype from Mfg_Swatches_History 
                                    where Mfg_Swatches_History.isactive=true and folderid in ({0}) {1}
                                    group by folderid,transactiontype
                                    having sum(Mfg_Swatches_History.qty) != 0;", string.Join(",",lst.ConvertAll(x=>x.id)), salesmencondtion);

                    using (System.Data.IDataReader reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            var folderid = reader["folderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["folderid"]);
                            var qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                            var transactiontype = reader["transactiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["transactiontype"]);
                            if (lst.Exists(x=>x.id == folderid))
                            {
                                switch(transactiontype)
                                {
                                    case 0:
                                        lst.Find(x => x.id == folderid).issue = Math.Abs(qty);
                                        break;
                                    case 1:
                                        lst.Find(x => x.id == folderid).received = Math.Abs(qty);
                                        break;
                                    case 2:
                                        lst.Find(x => x.id == folderid).journal = qty;
                                        break;
                                }
                               
                            }
                        }
                    }

                    foreach(var itm in lst)
                    {
                        if (itm.received > itm.issue)
                        {
                            itm.received -= itm.issue;
                            itm.issue = 0;
                        }
                        else
                        {
                            itm.issue -= itm.received;
                            itm.received = 0;
                        }
                        itm.stock = itm.qty - (itm.issue - itm.received) + itm.journal;
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfg_swatches_folder);
            }
            return lst;
        }

        public List<Mfg_Swatches_FolderSummary> Summary(Mfg_Swatches_creteria mfg_swatches_folder)
        {
            List<Mfg_Swatches_FolderSummary> omfg_swatches_folders = new List<Mfg_Swatches_FolderSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                string stockquery = @"with allsummary as
                    (select Mfg_Swatches_History.folderid,Mfg_Swatches_History.referenceid,
                    mfg_swatches_folder.designid,mfg_swatches_folder.code folder,
                    sum(Mfg_Swatches_History.qty) stock from Mfg_Swatches_History 
                    inner join mfg_swatches_folder on mfg_swatches_folder.id=Mfg_Swatches_History.folderid 
                    
                    where Mfg_Swatches_History.isactive=true {0} 
                    group by Mfg_Swatches_History.folderid,Mfg_Swatches_History.referenceid,
                    mfg_swatches_folder.code,mfg_swatches_folder.designid)
					
                    select allsummary.*,employee.name salesmen,mfgdesign.code design,
                    products.name productname,employee.name salesmen,brand.name brandname from allsummary
		            left outer join mfgdesign on mfgdesign.id = allsummary.designid
		            left outer join products on products.id = mfgdesign.productid
		            left outer join brand on brand.id = mfgdesign.brandid
                    inner join employee on employee.id = allsummary.referenceid";

                if (mfg_swatches_folder.folderid > 0 || mfg_swatches_folder.salesmenid > 0 || mfg_swatches_folder.salesmenid > 0)
                {
                    List<string> corecondition = new List<string>();
                    if (mfg_swatches_folder.folderid > 0)
                    {
                        corecondition.Add(" Mfg_Swatches_History.folderid=@folderid ");
                    }
                    if (mfg_swatches_folder.salesmenid > 0)
                    {
                        corecondition.Add(" Mfg_Swatches_History.referenceid=@salesmenid ");
                    }
                    if (mfg_swatches_folder.designid > 0)
                    {
                        corecondition.Add(" mfg_swatches_folder.designid=@designid ");
                    }
                    stockquery = string.Format(stockquery, " and " + string.Join(" and ", corecondition));
                }
                else
                {
                    stockquery = string.Format(stockquery, "");
                }

                var param = new SortedList<string, object>();
                param.Add("folderid", mfg_swatches_folder.folderid);
                param.Add("designid", mfg_swatches_folder.designid);
                param.Add("salesmenid", mfg_swatches_folder.salesmenid);
                using (var reader = ExecuteReader(stockquery, param, conn))
                {
                    while (reader.Read())
                    {
                        Mfg_Swatches_FolderSummary mfg_Swatches_FolderSummary = new Mfg_Swatches_FolderSummary();
                        mfg_Swatches_FolderSummary.folderstock = reader.IsDBNull(0) ? 0 : (Convert.ToInt64(reader["stock"])*-1);
                        mfg_Swatches_FolderSummary.design = reader.IsDBNull(0) ? "" : Convert.ToString(reader["design"]);
                        mfg_Swatches_FolderSummary.folder = reader.IsDBNull(0) ? "" : Convert.ToString(reader["folder"]);
                        mfg_Swatches_FolderSummary.product = reader.IsDBNull(0) ? "" : Convert.ToString(reader["productname"]);
                        mfg_Swatches_FolderSummary.brand = reader.IsDBNull(0) ? "" : Convert.ToString(reader["brandname"]);
                        mfg_Swatches_FolderSummary.folder = reader.IsDBNull(0) ? "" : Convert.ToString(reader["folder"]);
                        mfg_Swatches_FolderSummary.salesmen = reader.IsDBNull(0) ? "" : Convert.ToString(reader["salesmen"]);
                        omfg_swatches_folders.Add(mfg_Swatches_FolderSummary);
                    }
                }

                if (omfg_swatches_folders.Count > 0)
                {
                    List<Mgg_Stockitem> stocklist = new List<Mgg_Stockitem>();
                    string query = @"select colourid,stock.designid,
            sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stockqty from salablegoods
            inner join stock on stock.id = stockid
            inner join items on items.id = stock.itemid
            where salablegoods.hasstock=true and salablegoods.isactive=true and stock.isactive=true {0}
            group by colourid,stock.designid,stock.serialno";
                    List<string> conditions = new List<string>();
                    var designlist = omfg_swatches_folders.ConvertAll(x => x.design).Distinct().ToList();
                    if (designlist.Count > 1)
                    {
                        conditions.Add(" stock.designid in (" + String.Join(",", string.Join(",",designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"))) + ") ");
                    }
                    else if (designlist.Count == 1)
                    {
                        conditions.Add(" stock.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist.First()) + "' ");
                    }
                    query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
                    using (var reader = ExecuteReader(query, param,conn))
                    {
                        while (reader.Read())
                        {
                            Mgg_Stockitem stockItem = new Mgg_Stockitem();
                            stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            stockItem.design = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                            stocklist.Add(stockItem);
                        }
                    }

                    omfg_swatches_folders.ForEach(x =>
                    {
                        x.fgstock = stocklist.Sum(y => y.design == x.design?(long)y.stock:0);
                    });


                    stocklist = new List<Mgg_Stockitem>();
                    query = @"select sum(oqty-(bqty+cqty+rqty)) stockqty,colourid,designid 
                from salesorderitems 
				inner join salesorder on salesorder.id =salesorderid
				where salesorderitems.isactive=true and salesorder.isactive=true and oqty-(bqty+cqty+rqty)>0 {0}
                group by colourid,designid";
                    conditions = new List<string>();
                    designlist = omfg_swatches_folders.ConvertAll(x => x.design).Distinct().ToList();
                    if (designlist.Count > 1)
                    {
                        conditions.Add(" salesorderitems.designid in (" + String.Join(",", string.Join(",", designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"))) + ") ");;
                    }
                    else if(designlist.Count == 1)
                    {
                        conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist.First()) + "' ");
                    }
                    query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
                    using (var reader = ExecuteReader(query, param, conn))
                    {
                        while (reader.Read())
                        {
                            Mgg_Stockitem stockItem = new Mgg_Stockitem();
                            stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                            stockItem.design = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                            stocklist.Add(stockItem);
                        }
                    }

                    omfg_swatches_folders.ForEach(x =>
                    {
                        x.sopending = stocklist.Sum(y => y.design == x.design ? (long)y.stock : 0);
                    });

                    if(mfg_swatches_folder.fromdate.Year>2000 && mfg_swatches_folder.todate.Year>2000)
                    {



                        stocklist = new List<Mgg_Stockitem>();
                        query = @"select sum(oqty-cqty) orderqty,sum(oqty-(bqty+cqty+rqty)) stockqty,colourid,designid,salesmenid
                from salesorderitems 
				inner join salesorder on salesorder.id =salesorderid
				where salesorderitems.isactive=true and salesorder.isactive=true and sodate>=@fromdate and sodate<@todate {0}
                group by colourid,designid,salesmenid";
                        param.Add("salemenid", mfg_swatches_folder.salesmenid);
                        param.Add("fromdate", mfg_swatches_folder.fromdate.Date);
                        param.Add("todate", mfg_swatches_folder.todate.Date.AddDays(1));
                        conditions = new List<string>();
                        if (mfg_swatches_folder.salesmenid>0)
                        {
                            conditions.Add("salesorder.salesmenid = @salesmenid");
                        }
                        if (designlist.Count > 1)
                        {
                            conditions.Add(" salesorderitems.designid in (" + String.Join(",", string.Join(",",designlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'"))) + ") ");
                        }
                        else if (designlist.Count == 1)
                        {
                            conditions.Add(" salesorderitems.designid = '" + WebAPI.Data.Utility.SafeSQL(designlist.First()) + "' ");
                        }
                        query = string.Format(query, (conditions.Count > 0 ? " and " : "") + string.Join(" and ", conditions));
                        using (var reader = ExecuteReader(query, param, conn))
                        {
                            while (reader.Read())
                            {
                                Mgg_Stockitem stockItem = new Mgg_Stockitem();
                                stockItem.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                                stockItem.stock = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                                stockItem.order = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                                stockItem.design = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                                stocklist.Add(stockItem);
                            }
                        }

                        omfg_swatches_folders.ForEach(x =>
                        {
                            x.soselfpending = stocklist.Sum(y => y.design == x.design ? (long)y.stock : 0);
                            x.soselforder = stocklist.Sum(y => y.design == x.design ? (long)y.order : 0);
                        });
                    }

                   

                    //ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    //var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "COLOUR" });

                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, mfg_swatches_folder);
            }
            return omfg_swatches_folders;
        }
        public List<Mfg_Swatches_FolderDetail> Stock(Mfg_Swatches_FolderCreteria _mfg_swatches_folder)
        {
            List<Mfg_Swatches_FolderDetail> omfg_swatches_folders = new List<Mfg_Swatches_FolderDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                var modificationid = _mfg_swatches_folder.id;
                _mfg_swatches_folder.id = 0;
                _mfg_swatches_folder.showdetail = true;
                omfg_swatches_folders = Search(_mfg_swatches_folder);
                if (modificationid > 0 & !omfg_swatches_folders.Exists(x=>x.id == modificationid))
                    omfg_swatches_folders.AddRange(Search(new Mfg_Swatches_FolderCreteria() { id = modificationid }));

                foreach (var itm in omfg_swatches_folders)
                {
                    if (itm.received > itm.issue)
                    {
                        itm.received -= itm.issue;
                        itm.issue = 0;
                    }
                    else
                    {
                        itm.issue -= itm.received;
                        itm.received = 0;
                    }
                    itm.stock = (itm.issue - itm.received) + itm.journal;
                }
                //if (omfg_swatches_folders.Count > 0)
                //{
                //    omfg_swatches_folders.ForEach(x => x.stock = x.qty);
                //    string stockquery = string.Format("select folderid,sum(qty) from Mfg_Swatches_History where referenceid=@salesmenid and folderid in ({0}) and isactive=true group by folderid", string.Join(",", omfg_swatches_folders.ConvertAll(x => x.id)));
                //    var param = new SortedList<string, object>();
                //    param.Add("salesmenid", _mfg_swatches_folder.salesmenid);
                //    using (var reader = ExecuteReader(stockquery, param, conn))
                //    {
                //        while (reader.Read())
                //        {
                //            var folderid = reader.IsDBNull(0) ? 0 : Convert.ToInt64(reader[0]);
                //            omfg_swatches_folders.Find(x => x.id == folderid).qty = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader[0]);
                //        }
                //    }
                //}
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfg_swatches_folder);
            }
            return omfg_swatches_folders;
        }
        public Mfg_Swatches_Folder Insert(Mfg_Swatches_Folder oMfg_Swatches_Folder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfg_Swatches_Folder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches_Folder);
            }

            if (oMfg_Swatches_Folder.id > 0)
                return Search(new Mfg_Swatches_FolderCreteria() { id = oMfg_Swatches_Folder.id }).First();

            return oMfg_Swatches_Folder;
        }

        public Mfg_Swatches_Folder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches_Folder oMfg_Swatches_Folder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("swatchesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.swatchesid;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.preparedby;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_Folder.code == null ? "" : oMfg_Swatches_Folder.code;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_Folder.design == null ? "" : oMfg_Swatches_Folder.design;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.status;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches_Folder.isactive;
            command.Parameters.Add("noofcolours", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.noofcolours;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.version;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.qty;
            command.Parameters.Add("creationon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.creationon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.modifiedby;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Swatches_Folder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfg_Swatches_Folder;
        }

        public Mfg_Swatches_Folder Update(Mfg_Swatches_Folder oMfg_Swatches_Folder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfg_Swatches_Folder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches_Folder);
            }
            if (oMfg_Swatches_Folder.id > 0)
                return Search(new Mfg_Swatches_FolderCreteria() { id = oMfg_Swatches_Folder.id }).First();
            return oMfg_Swatches_Folder;
        }

        public Mfg_Swatches_Folder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches_Folder oMfg_Swatches_Folder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("swatchesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.swatchesid;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.preparedby;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.qty;
            command.Parameters.Add("creationon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.creationon;
            command.Parameters.Add("noofcolours", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.noofcolours;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_Folder.code == null ? "" : oMfg_Swatches_Folder.code;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_Folder.design == null ? "" : oMfg_Swatches_Folder.design;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.status;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches_Folder.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.id;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Swatches_Folder.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oMfg_Swatches_Folder;
        }


        public bool Delete(Mfg_Swatches_Folder oMfg_Swatches_Folder)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfg_Swatches_Folder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches_Folder);

            }
            return hasSuccessCount;

        }

        internal List<Mfg_Swatches_HistoryDetail> Issue(List<Mfg_Folder_Issue> items)
        {
            Mfg_Swatches_HistoryDataService mfg_Swatches_HistoryDataService = new Mfg_Swatches_HistoryDataService() { Context = this.Context };
            var Mfg_Swatches_HistoryDetails = items.ConvertAll(item =>
            {
                return new Mfg_Swatches_HistoryDetail()
                {
                    createdby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedby = Context.UserID,
                    modifiedon = DateTimeNow,
                    folderid = item.folderid,
                    isactive = true,
                    actionedby = item.issuedby,
                    referenceid = item.receivedby,
                    referencetype = 1,
                    swatchesid = item.swatchesid,
                    qty = item.qty * -1,
                    remarks = item.remark,
                    version = 1,
                    transactionon = item.transactionon,
                    transactionmode = Mfg_Swatches_History.TransactionModes.Issue
                };
            });
            return mfg_Swatches_HistoryDataService.Insert(Mfg_Swatches_HistoryDetails);
        }

        internal List<Mfg_Swatches_HistoryDetail> Journal(List<Mfg_Folder_Journal> items)
        {
            Mfg_Swatches_HistoryDataService mfg_Swatches_HistoryDataService = new Mfg_Swatches_HistoryDataService() { Context = this.Context };
            var Mfg_Swatches_HistoryDetails = items.ConvertAll(item =>
            {
                return new Mfg_Swatches_HistoryDetail()
                {
                    createdby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedby = Context.UserID,
                    modifiedon = DateTimeNow,
                    folderid = item.folderid,
                    isactive = true,
                    actionedby = item.adjustedby,
                    referenceid = item.salemenid,
                    referencetype = 1,
                    swatchesid = item.swatchesid,
                    qty = item.qty,
                    remarks = item.remark,
                    version = 1,
                    transactionon = item.transactionon,
                    transactionmode = Mfg_Swatches_History.TransactionModes.Journal
                };
            });
            return mfg_Swatches_HistoryDataService.Insert(Mfg_Swatches_HistoryDetails);
        }

        internal List<Mfg_Swatches_HistoryDetail> Receive(List<Mfg_Folder_Receive> items)
        {
            Mfg_Swatches_HistoryDataService mfg_Swatches_HistoryDataService = new Mfg_Swatches_HistoryDataService() { Context = this.Context };
            var Mfg_Swatches_HistoryDetails = items.ConvertAll(item =>
            {
                return new Mfg_Swatches_HistoryDetail()
                {
                    createdby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedby = Context.UserID,
                    modifiedon = DateTimeNow,
                    folderid = item.folderid,
                    isactive = true,
                    actionedby = item.receivedby,
                    referenceid = item.returnedby,
                    referencetype = 1,
                    swatchesid = item.swatchesid,
                    qty = item.qty,
                    remarks = item.remark,
                    version = 1,
                    transactionon = item.transactionon,
                    transactionmode = Mfg_Swatches_History.TransactionModes.Received
                };
            });
            return mfg_Swatches_HistoryDataService.Insert(Mfg_Swatches_HistoryDetails);
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches_Folder oMfg_Swatches_Folder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches_Folder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_Folder.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_Folder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_Folder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
