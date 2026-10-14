using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ProductdesignDataService : BaseData
    {
        const string SQL_Search = @"select detail, createdby, modifiedby, isactive, version, modifiedon, createdon, productid, supplierid, id, colours, designid from productdesign where id=@id";

        const string SQL_Insert = @"INSERT INTO public.productdesign(detail, createdby, modifiedby, isactive, version, modifiedon, createdon, productid, supplierid, colours, designid) 
        VALUES(@detail, @createdby, @modifiedby, @isactive, @version, @modifiedon, @createdon, @productid, @supplierid, @colours, @designid) RETURNING id";

        const string SQL_Update = @"UPDATE public.productdesign SET detail=@detail, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, productid=@productid, supplierid=@supplierid, colours=@colours, designid=@designid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.productdesign set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<ProductdesignBase> SearchDesignOnly(ProductdesignCreteria _productdesign)
        {
            List<ProductdesignBase> lst = new List<ProductdesignBase>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_productdesign.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productdesign.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_productdesign.Code))
                    if (_productdesign.supplierid > 0)
                        qb.AddParameter("supplierid", _productdesign.supplierid, "=");
                    if (_productdesign.productid > 0)
                        qb.AddParameter("productid", _productdesign.productid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductdesignBase oproductdesign = new ProductdesignBase();
                        oproductdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oproductdesign.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oproductdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductdesign.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        lst.Add(oproductdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productdesign);
            }
            return lst;
        }

        public List<Productdesign> Search(ProductdesignCreteria _productdesign)
        {
            List<Productdesign> lst = new List<Productdesign>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_productdesign.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productdesign.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_productdesign.Code))
                    if (_productdesign.supplierid > 0)
                        qb.AddParameter("supplierid", _productdesign.supplierid, "=");
                    if (_productdesign.productid > 0)
                        qb.AddParameter("productid", _productdesign.productid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Productdesign oproductdesign = new Productdesign();
                        oproductdesign.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproductdesign.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproductdesign.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproductdesign.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproductdesign.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oproductdesign.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproductdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oproductdesign.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oproductdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductdesign.colours_json = reader["colours"] == DBNull.Value ? null : reader["colours"].ToString();
                        oproductdesign.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oproductdesign.detail_json = reader["detail"] == DBNull.Value ? "" : reader["detail"].ToString();
                        lst.Add(oproductdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productdesign);
            }
            return lst;
        }

        internal Productdesign SaveImage(Productdesign item)
        {
            var existing = SearchImage(new ProductdesignBase()
            {
                supplierid = item.supplierid,
                productid = item.productid,
                designid = item.designid
            });

            item.colours.RemoveAll(x => existing.colours.Exists(y => y.id == x.id && y.image == y.image));
            if (item.colours.Count > 0)
            {
                using (var connection = CreateConnection())
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    foreach (var imgitem in item.colours)
                    {
                        string updatestockimagequery = "Update stockimage set imageid=@imageid where productid=@productid and supplierid=@supplierid and designid=@designid and colourid=@colourid";
                        NpgsqlCommand command = new NpgsqlCommand(updatestockimagequery, connection, transaction);
                        command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.productid;
                        command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.supplierid;
                        command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = imgitem.id;
                        command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = imgitem.image == null ? "" : imgitem.image;
                        command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.designid == null ? "" : item.designid;
                        if (command.ExecuteNonQuery() == 0)
                        {
                            command.CommandText = "insert into stockimage(productid,supplierid,designid,colourid,imageid) values (@productid,@supplierid,@designid,@colourid,@imageid)";
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
            return item;
        }
        public Productdesign SearchImage(ProductdesignBase criteria)
        {
            Productdesign productdesign = new Productdesign()
            {
                supplierid = criteria.supplierid,
                productid = criteria.productid,
                designid = criteria.designid,
                colours = new List<Productdesign.Colours>()
            };

            productdesign.colours = new List<Productdesign.Colours>();
            string stockimagequery = "select id,colourid,imageid from stockimage where productid=@productid and designid=@designid and supplierid=@supplierid";
            var stockimageparam = new SortedList<string, object>();
            stockimageparam.Add("productid", criteria.productid);
            stockimageparam.Add("designid", criteria.designid);
            stockimageparam.Add("supplierid", criteria.supplierid);
            using (var reader = ExecuteReader(stockimagequery, stockimageparam))
            {
                while (reader.Read())
                {

                    long colourid = reader.GetInt64(1);
                    string imageid = reader.GetString(2);
                    productdesign.colours.Add(new Productdesign.Colours()
                    {
                        id = colourid,
                        image = imageid
                    });
                }
            }
            return productdesign;
        }


        public Productdesign Insert(Productdesign oProductdesign)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oProductdesign.autonumber)
                    {
                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrinvoice.designid", 0);
                        var newdesignid = sequence.GetNextNumber(Context, conn, transaction);
                        if (newdesignid.Contains("[DESIGNCODE]"))
                        {
                            newdesignid = newdesignid.Replace("[DESIGNCODE]", oProductdesign.designid);
                        }
                        else if (!string.IsNullOrEmpty(oProductdesign.designid))
                        {
                            newdesignid = oProductdesign.designid + (!newdesignid.StartsWith("/") ? "/" : "") + newdesignid;
                        }
                        oProductdesign.designid = newdesignid;
                    }
                    Insert(transaction, conn, oProductdesign);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductdesign);
            }

            return oProductdesign;
        }

        public Productdesign Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Productdesign oProductdesign)
        {
            oProductdesign.isactive = true;
            oProductdesign.createdon = DateTimeNow;
            oProductdesign.modifiedon = DateTimeNow;
            oProductdesign.modifiedby = Context.UserID;
            oProductdesign.createdby = Context.UserID;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oProductdesign.detail_json == null ? "null" : oProductdesign.detail_json;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductdesign.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductdesign.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductdesign.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductdesign.createdon;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.productid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.supplierid;
            command.Parameters.Add("colours", NpgsqlTypes.NpgsqlDbType.Json).Value = oProductdesign.colours_json == null ? "null" : oProductdesign.colours_json;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductdesign.designid == null ? "" : oProductdesign.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductdesign.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oProductdesign;
        }

        public Productdesign Update(Productdesign oProductdesign)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oProductdesign);
                    if (oProductdesign.colours != null &&
                        oProductdesign.colours.Count > 0)
                    {
                        foreach (var imgitem in oProductdesign.colours)
                        {
                            if (imgitem.id > 0 &&
                                !string.IsNullOrEmpty(imgitem.image))
                            {
                                string updatestockimagequery = "Update stockimage set imageid=@imageid where productid=@productid and supplierid=@supplierid and designid=@designid and colourid=@colourid";
                                NpgsqlCommand command = new NpgsqlCommand(updatestockimagequery, conn, transaction);
                                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.productid;
                                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.supplierid;
                                command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = imgitem.id;
                                command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = imgitem.image == null ? "" : imgitem.image;
                                command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductdesign.designid == null ? "" : oProductdesign.designid;
                                if (command.ExecuteNonQuery() == 0)
                                {
                                    command.CommandText = "insert into stockimage(productid,supplierid,designid,colourid,imageid) values (@productid,@supplierid,@designid,@colourid,@imageid)";
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductdesign);
            }

            return oProductdesign;
        }

        public Productdesign Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Productdesign oProductdesign)
        {

            oProductdesign.modifiedon = DateTimeNow;
            oProductdesign.modifiedby = Context.UserID;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oProductdesign.detail_json == null ? "null" : oProductdesign.detail_json;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductdesign.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductdesign.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductdesign.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductdesign.createdon;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.productid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.supplierid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.id;
            command.Parameters.Add("colours", NpgsqlTypes.NpgsqlDbType.Json).Value = oProductdesign.colours_json == null ? "null" : oProductdesign.colours_json;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductdesign.designid == null ? "" : oProductdesign.designid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductdesign.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oProductdesign;
        }


        public bool Delete(Productdesign oProductdesign)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oProductdesign);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductdesign);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Productdesign oProductdesign)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductdesign.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oProductdesign.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductdesign.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductdesign.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<Productdesign> Sync(ProductdesignCreteria item)
        {
            var existinglist = Search(new ProductdesignCreteria()
            {
                supplierid = item.supplierid,
                productid = item.productid
            });

            string query = @"
                select distinct stock.supplierid,stock.productid,upper(trim(stock.designid)) designid from stock
                where  stock.isactive=true and stock.productid = @productid and stock.supplierid = @supplierid";

            if (item.supplierid <= 0)
                query = query.Replace("and stock.supplierid = @supplierid", "");
            if (item.productid <= 0)
                query = query.Replace("and stock.productid = @productid", "");
            var param = new SortedList<string, object>();
            param.Add("supplierid", item.supplierid);
            param.Add("productid", item.productid);
            List<Productdesign> productdesigns = new List<Productdesign>();
            List<Productdesign> existingdesigns = new List<Productdesign>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    string designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                    long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    long supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    if (string.IsNullOrEmpty(designid) || productid == 0 || supplierid == 0) continue;
                    Productdesign productdesign = new Productdesign();
                    productdesign.Activestatus = BaseEntity.ActiveStatus.Updated;
                    productdesign.designid = designid;
                    productdesign.productid = productid;
                    productdesign.supplierid = supplierid;
                    productdesign.isactive = true;
                    productdesign.colours = new List<Productdesign.Colours>();
                    productdesign.detail = new Productdesign.Detail() { sizes = new List<long>() };
                    if (existinglist.Exists(x => x.designid == designid && x.productid == productid && x.supplierid == supplierid))
                    {
                        productdesigns.Add(existinglist.Find(x => x.designid == designid && x.productid == productid && x.supplierid == supplierid));
                    }
                    else
                    {
                        productdesigns.Add(productdesign);
                        existinglist.Add(productdesign);
                    }
                }
            }

            query = @"
                select colourid,supplierid,designid,imageid from stockimage
                where 1=1 and stockimage.productid = @productid and stockimage.supplierid = @supplierid";

            if (item.supplierid <= 0)
                query = query.Replace("and stockimage.supplierid = @supplierid", "");
            if (item.productid <= 0)
                query = query.Replace("and stockimage.productid = @productid", "");

            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    var supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    var designid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    var imageid = reader["imageid"] == DBNull.Value ? "" : Convert.ToString(reader["imageid"]);
                    if (productdesigns.Exists(x => x.designid == designid && x.supplierid == supplierid))
                    {
                        var pd = productdesigns.Find(x => x.designid == designid && x.supplierid == supplierid);
                        if (pd.colours == null) pd.colours = new List<Productdesign.Colours>();
                        if (!pd.colours.Exists(x => x.id == colourid))
                        {
                            pd.colours.Add(new Productdesign.Colours()
                            {
                                id = colourid,
                                image = imageid
                            });
                        }
                    }
                }
            }

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    foreach(var pditem in productdesigns)
                    {
                        if (pditem.id > 0)
                            Update(transaction, conn, pditem);
                        else
                            Insert(transaction,conn,pditem);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return existinglist;
        }

        public bool MergeDesign(MergeProductDesign mergeProductDesign)
        {
            string updatedesign = @"update stock set designid = @designid where id in (
                                    select stock.id from productdesign
                                    inner join stock on stock.designid = productdesign.designid
                                    and stock.productid = productdesign.productid
					                and stock.supplierid = productdesign.supplierid
                                    where productdesign.id in (" + string.Join(",", mergeProductDesign.ids) + "));";

            mergeProductDesign.ids.Remove(mergeProductDesign.mergeid);
            string qurey = @"update productdesign set designid=@designid where id=@id;
                             delete from productdesign where id in (" + string.Join(",", mergeProductDesign.ids) + ");";
            var param = new SortedList<string, object>();
            param.Add("id", mergeProductDesign.mergeid);
            param.Add("designid", mergeProductDesign.mergename);
            return ExecuteQuery(updatedesign + qurey, param) > 0;
        }

        public void ImportAll()
        {
            string query = @"insert into productdesign(productid, version, isactive,    modifiedby, createdby,  designid, supplierid,colours)
                             select distinct stock.productid,-1,true,-1,-1,upper(stock.designid) designid,stock.supplierid,'[]' from stock";
        }

        public List<Productdesign> Import(ProductdesignCreteria item)
        {
            var existinglist = Search(new ProductdesignCreteria()
            {
                supplierid = item.supplierid,
                productid = item.productid
            });

            existinglist.ForEach(x => x.designid = item.designid == null ? "" : item.designid.ToUpper());

            string query = @"select distinct stock.designid,items.sizeid,items.colourid,stock.imageid from stock
            inner join items on items.id =stock.itemid
            where stock.productid = @productid and stock.supplierid = @supplierid and stock.isactive=true";

            if (item.productid == 0)
            {
                query = query.Replace("stock.productid = @productid and", "");
            }

            if (item.supplierid == 0)
            {
                query = query.Replace("stock.supplierid = @supplierid and", "");
            }

            var param = new SortedList<string, object>();
            param.Add("supplierid", item.supplierid);
            param.Add("productid", item.productid);
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    string designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString().ToUpper();
                    string imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                    long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                    long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                    if (string.IsNullOrEmpty(designid)) continue;
                    if (existinglist.Exists(x => x.designid == designid))
                    {
                        var exist = existinglist.Find(x => x.designid == designid);
                        if (exist.colours == null) exist.colours = new List<Productdesign.Colours>();
                        if (exist.detail == null) exist.detail = new Productdesign.Detail();
                        if (exist.detail.sizes == null) exist.detail.sizes = new List<long>();
                        exist.Activestatus = BaseEntity.ActiveStatus.Updated;
                        if (!exist.colours.Exists(x => x.id == colourid))
                            exist.colours.Add(new Productdesign.Colours()
                            {
                                id = colourid,
                                image = imageid
                            });

                        if (!exist.detail.sizes.Contains(sizeid))
                            exist.detail.sizes.Add(sizeid);
                    }
                    else
                    {
                        Productdesign productdesign = new Productdesign();
                        productdesign.Activestatus = BaseEntity.ActiveStatus.Updated;
                        productdesign.designid = designid;
                        productdesign.productid = item.productid;
                        productdesign.supplierid = item.supplierid;
                        productdesign.isactive = true;
                        productdesign.colours = new List<Productdesign.Colours>();
                        productdesign.detail = new Productdesign.Detail() { sizes = new List<long>() };
                        productdesign.detail.sizes.Add(sizeid);
                        productdesign.colours.Add(new Productdesign.Colours()
                        {
                            id = colourid,
                            image = imageid
                        });
                        existinglist.Add(productdesign);
                        if (existinglist.Count == 10000) break;
                    }
                }
            }
            foreach (var eitem in existinglist)
            {
                if (eitem.id > 0)
                    Update(eitem);
                else
                    Insert(eitem);
            }
            return existinglist;
        }

        internal List<ProductDesignImage> SearchDesignImage(ProductdesignCreteria item)
        {
            string query = "select colours,designid,supplierid,productid from Productdesign ";
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            qb.AddParameter("isactive", true, "=");
            qb.Add2Query(" colours->0 is not null ");
            if (item.supplierids != null &&
                item.supplierids.Count > 0)
            {
                if (item.supplierids.Count == 1)
                {
                    qb.AddParameter("supplierid", item.supplierids[0], "=");
                }
                else
                {
                    qb.AddInParamer("supplierid", string.Join(",", item.supplierids));
                }
            }

            if (item.productids != null && item.productids.Count > 0)
            {
                if (item.productids.Count == 1)
                {
                    qb.AddParameter("productid", item.productids[0], "=");
                }
                else
                {
                    qb.AddInParamer("productid", string.Join(",", item.productids));
                }
            }
            var lst = new List<ProductDesignImage>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductDesignImage oproductdesign = new ProductDesignImage();
                        oproductdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oproductdesign.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oproductdesign.colours_json = reader["colours"] == DBNull.Value ? "" : reader["colours"].ToString();
                        oproductdesign.design = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                        if (oproductdesign.colours != null &&
                            oproductdesign.colours.Exists(x => !string.IsNullOrEmpty(x.image)))
                        {
                            oproductdesign.image = oproductdesign.colours.Find(x => !string.IsNullOrEmpty(x.image)).image;
                        }
                        lst.Add(oproductdesign);
                    }
                }
                conn.Close();
            }
            return lst;
        }
    }

}
