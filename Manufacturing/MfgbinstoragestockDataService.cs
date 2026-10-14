using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;


namespace InfyPOS.Data

{
    public class MfgbinstoragestockDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdon, modifiedon, createdby, modifiedby, status, version, stockid, iqty, oqty, dqty, designid, designname, uid, sizegroupid, sizegroup, sizeid, sizename, colourid, colourname, hasstock, mfgbinstorageid,product from mfgbinstoragestock where id=@id";

        const string SQL_Insert = @"
        UPDATE Mfgbinstorage set allocated=allocated+@iqty where id=@mfgbinstorageid;
        INSERT INTO public.mfgbinstoragestock(isactive, createdon, modifiedon, createdby, modifiedby, status, version, stockid, iqty, oqty, dqty, designid, designname, uid, sizegroupid, sizegroup, sizeid, sizename, colourid, colourname, hasstock, mfgbinstorageid,product)
        VALUES(@isactive, @createdon, @modifiedon, @createdby, @modifiedby, @status, @version, @stockid, @iqty, @oqty, @dqty, @designid, @designname, @uid, @sizegroupid, @sizegroup, @sizeid, @sizename, @colourid, @colourname, @hasstock, @mfgbinstorageid,@product) RETURNING id";

        const string SQL_Update = @"
        UPDATE Mfgbinstorage set allocated=allocated-mfgbinstoragestock.iqty from mfgbinstoragestock
        where mfgbinstoragestock.id = @id and Mfgbinstorage.id = mfgbinstoragestock.mfgbinstorageid;
        UPDATE Mfgbinstorage set allocated=allocated+@iqty where id=@mfgbinstorageid;
        UPDATE public.mfgbinstoragestock SET modifiedon=@modifiedon, modifiedby=@modifiedby, status=@status, version=@version+1, stockid=@stockid, iqty=@iqty, oqty=@oqty, dqty=@dqty, designid=@designid, designname=@designname, uid=@uid, sizegroupid=@sizegroupid, sizegroup=@sizegroup, sizeid=@sizeid, sizename=@sizename, colourid=@colourid, colourname=@colourname, hasstock=@hasstock, mfgbinstorageid=@mfgbinstorageid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
        UPDATE Mfgbinstorage set allocated=allocated-mfgbinstoragestock.iqty from mfgbinstoragestock
        where mfgbinstoragestock.id = @id and Mfgbinstorage.id = mfgbinstoragestock.mfgbinstorageid;
        update public.mfgbinstoragestock set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        internal bool RemovefromBin(MfgbinstorageChange item)
        {
            decimal required = 0;
            List<Mfgbinstoragestock> list = null;
            if (item.storage.stocklist != null &&
                item.storage.stocklist.Count > 0)
            {
                foreach (var guid in item.storage.stocklist.GroupBy(x => x.uid))
                {
                    var barcode = guid.Key;
                    list = Search(new MfgbinstoragestockCreteria()
                    {
                        mfgbinstorageid = item.storage.id,
                        onlystock = true,
                        uid = barcode
                    });
                    required = guid.Sum(x => x.stock);
                    foreach (var sitem in list)
                    {
                        decimal stock = sitem.iqty - (sitem.oqty + sitem.dqty);
                        decimal removeqty = 0;
                        if (required > stock)
                        {
                            removeqty = stock;
                            required -= stock;
                        }
                        else
                        {
                            removeqty = required;
                            required = 0;
                        }
                        var param = new SortedList<string, object>();
                        param.Add("id", sitem.id);
                        param.Add("oqty", sitem.oqty);
                        param.Add("qty", removeqty);
                        using (var reader = ExecuteReader("update Mfgbinstoragestock set oqty=oqty+@qty where id=@id and oqty=@oqty RETURNING oqty", param))
                        {
                            if (reader.Read())
                                sitem.oqty = reader.GetDecimal(0);
                        }
                        if (required == 0) break;
                    }
                }
            }
            else 
            {
                list = Search(new MfgbinstoragestockCreteria()
                {
                    mfgbinstorageid = item.storage.id,
                    onlystock = true
                });
                required = item.qty;
                foreach (var sitem in list)
                {
                    decimal stock = sitem.iqty - (sitem.oqty + sitem.dqty);
                    decimal removeqty = 0;
                    if (required > stock)
                    {
                        removeqty = stock;
                        required -= stock;
                    }
                    else
                    {
                        removeqty = required;
                        required = 0;
                    }
                    var param = new SortedList<string, object>();
                    param.Add("id", sitem.id);
                    param.Add("oqty", sitem.oqty);
                    param.Add("qty", removeqty);
                    using (var reader = ExecuteReader("update Mfgbinstoragestock set oqty=oqty+@qty where id=@id and oqty=@oqty RETURNING oqty", param))
                    {
                        if (reader.Read())
                            sitem.oqty = reader.GetDecimal(0);
                    }
                    if (required == 0) break;
                }

                return required == 0;
            }
            return true;
        }

        internal List<Mfgbinstoragestock> Add2Bin(MfgbinstorageChange item)
        {
            string query = @"
select mfgdesign.id as designid,mfgdesign.name as designname,productid,cast(schemelist ->> 'sizeid' as bigint) sizeid,
schemelist ->> 'sizegroup' sizegroup,cast(schemelist ->> 'colourid' as bigint) colourid,
cast(schemelist ->> 'name' as varchar) as description,cast(schemelist ->> 'code' as varchar) as itemcode,
cast(schemelist ->> 'uid' as varchar) as uid,colour.name colourname,size.name sizename from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
left outer join referencelist colour on colour.id = cast(schemelist ->> 'colourid' as bigint)
left outer join referencelist size on size.id = cast(schemelist ->> 'sizeid' as bigint)
where mfgdesign.isactive=true and cast(schemelist ->> 'uid' as varchar)=@uid";
            List<Mfgbinstoragestock> insertlist = new List<Mfgbinstoragestock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                Mfgbinstoragestock omfgbinstoragestock = new Mfgbinstoragestock();
                omfgbinstoragestock.mfgbinstorageid = item.storage.id;
                omfgbinstoragestock.version = 1;
                omfgbinstoragestock.isactive = true;
                NpgsqlCommand command = null;

                command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.uan;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        omfgbinstoragestock.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgbinstoragestock.colourname = reader["colourname"] == DBNull.Value ? "" : Convert.ToString(reader["colourname"]);
                        omfgbinstoragestock.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgbinstoragestock.designname = reader["designname"] == DBNull.Value ? "" : Convert.ToString(reader["designname"]);
                        omfgbinstoragestock.product = reader["description"] == DBNull.Value ? "" : Convert.ToString(reader["description"]);
                        omfgbinstoragestock.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : Convert.ToString(reader["sizegroup"]);
                        omfgbinstoragestock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgbinstoragestock.sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]);
                        omfgbinstoragestock.uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    }
                }


                decimal require = item.qty;
                var newitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Mfgbinstoragestock>(Newtonsoft.Json.JsonConvert.SerializeObject(omfgbinstoragestock));
                newitem.iqty = item.qty;
                var transaction = conn.BeginTransaction();
                    Insert(transaction, conn, newitem);

                //foreach (var sitem in item.items)
                //{
                //    var available = sitem.stockqty - sitem.allocated;
                //    var newitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Mfgbinstoragestock>(Newtonsoft.Json.JsonConvert.SerializeObject(omfgbinstoragestock));
                //    newitem.stockid = sitem.stockid;
                //    insertlist.Add(newitem);
                //    if (require > available)
                //    {
                //        newitem.iqty = available;
                //        require -= available;
                //    }
                //    else
                //    {
                //        newitem.iqty = require;
                //        require = 0;
                //    }
                //    if (require <= 0) break;
                //}
                //var transaction = conn.BeginTransaction();
                //foreach (var stitem in insertlist)
                //    Insert(transaction, conn, stitem);
                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return insertlist;
        }

        public List<Mfgbinstoragestock> Search(MfgbinstoragestockCreteria _mfgbinstoragestock)
        {
            List<Mfgbinstoragestock> lst = new List<Mfgbinstoragestock>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgbinstoragestock.id != 0 && !_mfgbinstoragestock.onlystock)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgbinstoragestock.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    if (_mfgbinstoragestock.onlystock)
                    {
                        qb.CurrentQuery += " where Mfgbinstoragestock.iqty-(Mfgbinstoragestock.oqty+Mfgbinstoragestock.dqty)>0 ";
                    }
                    qb.AddParameter("isactive", true, "=");

                    if(!string.IsNullOrEmpty(_mfgbinstoragestock.uid))
                    {
                        qb.AddParameter("Mfgbinstoragestock.uid", _mfgbinstoragestock.uid, "!=");
                    }
                    if (_mfgbinstoragestock.id > 0)
                    {
                        qb.AddParameter("Mfgbinstoragestock.id", _mfgbinstoragestock.id, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgbinstoragestock omfgbinstoragestock = new Mfgbinstoragestock();
                        omfgbinstoragestock.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgbinstoragestock.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgbinstoragestock.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgbinstoragestock.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgbinstoragestock.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgbinstoragestock.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgbinstoragestock.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        omfgbinstoragestock.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgbinstoragestock.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        omfgbinstoragestock.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]);
                        omfgbinstoragestock.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        omfgbinstoragestock.dqty = reader["dqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dqty"]);
                        omfgbinstoragestock.stock = omfgbinstoragestock.iqty - (omfgbinstoragestock.oqty+ omfgbinstoragestock.dqty);
                        omfgbinstoragestock.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgbinstoragestock.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        omfgbinstoragestock.uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        omfgbinstoragestock.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        omfgbinstoragestock.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        omfgbinstoragestock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgbinstoragestock.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        omfgbinstoragestock.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgbinstoragestock.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        omfgbinstoragestock.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        omfgbinstoragestock.hasstock = reader["hasstock"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasstock"]);
                        omfgbinstoragestock.mfgbinstorageid = reader["mfgbinstorageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgbinstorageid"]);
                        lst.Add(omfgbinstoragestock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgbinstoragestock);
            }
            return lst;
        }

        public Mfgbinstoragestock Insert(Mfgbinstoragestock oMfgbinstoragestock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgbinstoragestock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgbinstoragestock);
                throw exp;
            }

            return oMfgbinstoragestock;
        }

        public Mfgbinstoragestock Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgbinstoragestock oMfgbinstoragestock)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstoragestock.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstoragestock.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstoragestock.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.modifiedby;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstoragestock.status;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstoragestock.version;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.stockid;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstoragestock.iqty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstoragestock.oqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstoragestock.dqty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.designid;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.designname == null ? "" : oMfgbinstoragestock.designname;
            command.Parameters.Add("product", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.product == null ? "" : oMfgbinstoragestock.product;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.uid == null ? "" : oMfgbinstoragestock.uid;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.sizegroupid;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.sizegroup == null ? "" : oMfgbinstoragestock.sizegroup;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.sizeid;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.sizename == null ? "" : oMfgbinstoragestock.sizename;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.colourid;
            command.Parameters.Add("colourname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.colourname == null ? "" : oMfgbinstoragestock.colourname;
            command.Parameters.Add("hasstock", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstoragestock.hasstock;
            command.Parameters.Add("mfgbinstorageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.mfgbinstorageid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgbinstoragestock.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgbinstoragestock;
        }

        public Mfgbinstoragestock Update(Mfgbinstoragestock oMfgbinstoragestock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgbinstoragestock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgbinstoragestock);
                throw exp;
            }

            return oMfgbinstoragestock;
        }


        public List<Mfgbinstoragestock.StockAvailable> SearchAvailableStock(string uan)
        {
            string query = @"with stocklist 
as
(
select stock.id as stockid,qty-(transfered+returnqty) stockqty from stock where hasstock = true and stock.serialno = @uan
),
binstock as
(
select sum(iqty-(oqty+dqty)) allocated,Mfgbinstoragestock.stockid from Mfgbinstoragestock
inner join stocklist on stocklist.stockid = Mfgbinstoragestock.stockid
group by Mfgbinstoragestock.stockid
)

select stocklist.*,binstock.allocated from stocklist
left outer join binstock on binstock.stockid = stocklist.stockid";

            List<Mfgbinstoragestock.StockAvailable> lst = new List<Mfgbinstoragestock.StockAvailable>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("uan", NpgsqlTypes.NpgsqlDbType.Varchar).Value = uan;


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgbinstoragestock.StockAvailable omfgbinstoragestock = new Mfgbinstoragestock.StockAvailable();
                        omfgbinstoragestock.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        omfgbinstoragestock.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        omfgbinstoragestock.allocated = reader["allocated"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["allocated"]);
                        lst.Add(omfgbinstoragestock);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, uan);
            }
            return lst;
        }

        public Mfgbinstoragestock Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgbinstoragestock oMfgbinstoragestock)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstoragestock.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstoragestock.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstoragestock.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.modifiedby;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstoragestock.status;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstoragestock.version;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.stockid;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstoragestock.iqty;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstoragestock.oqty;
            command.Parameters.Add("dqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgbinstoragestock.dqty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.designid;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.designname == null ? "" : oMfgbinstoragestock.designname;
            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.uid == null ? "" : oMfgbinstoragestock.uid;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.sizegroupid;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.sizegroup == null ? "" : oMfgbinstoragestock.sizegroup;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.sizeid;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.sizename == null ? "" : oMfgbinstoragestock.sizename;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.colourid;
            command.Parameters.Add("colourname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgbinstoragestock.colourname == null ? "" : oMfgbinstoragestock.colourname;
            command.Parameters.Add("hasstock", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstoragestock.hasstock;
            command.Parameters.Add("mfgbinstorageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.mfgbinstorageid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgbinstoragestock.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgbinstoragestock;
        }


        public bool Delete(Mfgbinstoragestock oMfgbinstoragestock)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgbinstoragestock);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgbinstoragestock);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgbinstoragestock oMfgbinstoragestock)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgbinstoragestock.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgbinstoragestock.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgbinstoragestock.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgbinstoragestock.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
