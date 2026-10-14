using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class CustomerpriceDataService : BaseData
    {
        const string SQL_Search = @"select createdon, isactive, id, modifiedby, version, createdby, modifiedon, designid,mfgdesignid from customerprice where id=@id";


        const string SQL_Insert = @"INSERT INTO public.customerprice(createdon, isactive, modifiedby, version, createdby, modifiedon, designid,mfgdesignid) VALUES(@createdon, @isactive, @modifiedby, @version, @createdby, @modifiedon, @designid,@mfgdesignid) RETURNING id";


        const string SQL_Update = @"UPDATE public.customerprice SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, designid=@designid,mfgdesignid=@mfgdesignid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerprice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Customerprice> Search(CustomerpriceCreteria _customerprice)
        {
            List<Customerprice> lst = new List<Customerprice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerprice.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerprice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_customerprice.mfgdesignids != null &&
                        _customerprice.mfgdesignids.Length>0)
                    {
                        qb.AddInParamer("mfgdesignid",String.Join(",", _customerprice.mfgdesignids));
                    }
                    //if (!string.IsNullOrEmpty(_customerprice.Code))
                    //    qb.AddParameter("code", _customerprice.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerprice ocustomerprice = new Customerprice();
                        ocustomerprice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerprice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerprice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerprice.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        ocustomerprice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerprice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerprice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerprice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerprice.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        lst.Add(ocustomerprice);
                    }
                }
                if (_customerprice.id > 0 && lst.Count == 1)
                {
                    CustomerpricelistDataService subCustomerpricelist = new CustomerpricelistDataService() { Context = this.Context };
                    lst[0].Customerpricelist = subCustomerpricelist.SearchAll(_customerprice.id);
                }
                else
                {
                    if(_customerprice.includemfg)
                    {
                        using (var reader = ExecuteReader("Select id,name from mfgdesign where isactive=true",null))
                        {
                            while(reader.Read())
                            {
                                var id = reader.GetInt64(0);
                                var name = reader.IsDBNull(1) ? "": reader.GetString(1);
                                if (!lst.Exists(x => x.mfgdesignid == id))
                                {
                                    lst.Add(new Customerprice()
                                    {
                                        id = id * -1,
                                        designid = name
                                    });
                                }
                                else
                                {
                                    lst.Find(x => x.mfgdesignid == id).designid = name;
                                }
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerprice);
            }
            return lst;
        }

        internal List<DesignPriceItem> SearchDesignPrice(CustomerpriceCreteria item)
        {
            List<DesignPriceItem> lst = new List<DesignPriceItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select customerprice.designid,customerprice.mfgdesignid,
customerpricelist.customerid,customerpricelist.customercategoryid categoryid,price from customerpricelist
inner join customerprice on customerprice.id = customerpricelist.customerpriceid
 where customerpricelist.isactive = true and customerpricelist.isactive=true";

                if (item.customerids != null && item.customerids.Length > 0 &&
                    (item.categoryids != null && item.categoryids.Length > 0))
                {
                    qb.CurrentQuery += " and ((customerid in (" + string.Join(",", item.customerids) + "))";
                    qb.CurrentQuery += " or (customercategoryid in (" + string.Join(",", item.categoryids) + ")))";
                }
                else if (item.categoryids != null && item.categoryids.Length > 0)
                {
                    qb.AddInParamer("customercategoryid", string.Join(",", item.categoryids));
                }
                else if (item.customerids != null && item.customerids.Length > 0)
                {
                    qb.AddInParamer("customerid", string.Join(",", item.customerids));
                }

                if (item.allcategory)
                {
                    qb.AddParameter("customercategoryid", 0, ">");
                }
                else if (item.designids != null && item.designids.Length > 0)
                    qb.AddInParamer("customerprice.id", string.Join(",", item.designids));

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DesignPriceItem ocustomerprice = new DesignPriceItem();
                        ocustomerprice.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomerprice.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerprice.design = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                        ocustomerprice.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        ocustomerprice.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        lst.Add(ocustomerprice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return lst;
        }

        internal List<CustomerPriceItem> SaveAll(List<CustomerPriceItem> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand("Update customerpricelist set price=@price,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and isactive=true", conn,transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

                    var olditems = items.FindAll(e => e.id > 0);
                    foreach(var item in olditems)
                    {
                        command.Parameters["id"].Value = item.id;
                        command.Parameters["price"].Value = item.price;
                        command.ExecuteNonQuery();
                    }

                    var newitems = items.FindAll(e => e.id <= 0 && e.price>0);
                    if (newitems.Count > 0)
                    {
                        var mfgmissings = newitems.FindAll(x => x.designid < 0).ConvertAll(x => x.designid*-1);
                        MfgdesignDataService mfgdesign = new MfgdesignDataService() { Context = this.Context };
                        var mfglist = mfgdesign.SearchDesignList(new MfgdesignCreteria()
                        {
                            ids = mfgmissings
                        });

                        var existing = Search(new CustomerpriceCreteria()
                        {
                            mfgdesignids = mfgmissings.ToArray()
                        });

                        foreach (var newitem in newitems.FindAll(x=>x.designid<0))
                        {
                            newitem.mfgdesignid = newitem.designid * -1;
                            if (existing.Exists(x => x.mfgdesignid == newitem.mfgdesignid))
                            {
                                newitem.designid = existing.Find(x => x.mfgdesignid == newitem.mfgdesignid).id;
                            }
                            else
                            {
                                string design = mfglist.Exists(x => x.id == newitem.mfgdesignid) ?
                                    mfglist.Find(x => x.id == newitem.mfgdesignid).name : "";
                                var newpricemaster = Insert(transaction, conn, new Customerprice()
                                {
                                    isactive = true,
                                    designid = design,
                                    createdby = Context.UserID,
                                    modifiedby = Context.UserID,
                                    createdon = DateTimeNow,
                                    modifiedon = DateTimeNow,
                                    mfgdesignid = newitem.mfgdesignid,
                                    version = 1
                                });
                                newitem.designid = newpricemaster.id;
                                existing.Add(newpricemaster);
                            }
                        }

                        CustomerpricelistDataService customerpricelistDataService = new CustomerpricelistDataService()
                        {
                            Context = this.Context
                        };
                        foreach (var item in newitems)
                        {
                            item.id = customerpricelistDataService.Insert(new Customerpricelist()
                            {
                                createdby = Context.UserID,
                                createdon = DateTimeNow,
                                modifiedby = Context.UserID,
                                modifiedon = DateTimeNow,
                                price = item.price,
                                customerpriceid = item.designid,
                                customercategoryid = item.categoryid,
                                customerid = item.customerid,
                                isactive = true
                            }).id;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }

        internal List<CustomerPriceItem> SearchAll(CustomerpriceCreteria item)
        {
            List<CustomerPriceItem> lst = new List<CustomerPriceItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select customerprice.id designid,customerpricelist.customerid,
customerprice.mfgdesignid,customerpricelist.customercategoryid categoryid,price,customerpricelist.id id 
from customerpricelist
inner join customerprice on customerprice.id = customerpricelist.customerpriceid
 where customerpricelist.isactive = true and customerpricelist.isactive=true";

                if (item.customerids != null && item.customerids.Length > 0 &&
                    (item.categoryids != null && item.categoryids.Length > 0))
                {
                    qb.CurrentQuery += " and ((customerid in ("+ string.Join(",", item.customerids)+"))";
                    qb.CurrentQuery += " or (customercategoryid in ("+ string.Join(",", item.categoryids)+")))";
                }
                else if (item.categoryids != null && item.categoryids.Length > 0)
                {
                    qb.AddInParamer("customercategoryid", string.Join(",", item.categoryids));
                }else if(item.customerids != null && item.customerids.Length>0)
                {
                    qb.AddInParamer("customerid", string.Join(",", item.customerids));
                }

                if(item.allcategory)
                {
                    qb.AddParameter("categoryid", 0, ">");
                }
                else  if (item.designids != null && item.designids.Length > 0)
                    qb.AddInParamer("customerprice.id", string.Join(",", item.designids));

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerPriceItem ocustomerprice = new CustomerPriceItem();
                        ocustomerprice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerprice.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomerprice.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerprice.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        ocustomerprice.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        ocustomerprice.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        lst.Add(ocustomerprice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return lst;
        }

        public Customerprice Insert(Customerprice oCustomerprice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oCustomerprice);
                    CustomerpricelistDataService subCustomerpricelist = new CustomerpricelistDataService() { Context = this.Context };
                    if (oCustomerprice.Customerpricelist != null)
                    {
                        foreach (var item in oCustomerprice.Customerpricelist)
                        {
                            item.customerpriceid = oCustomerprice.id;
                            subCustomerpricelist.Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerprice);
            }

            return oCustomerprice;
        }

        public Customerprice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerprice oCustomerprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerprice.createdon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerprice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerprice.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerprice.modifiedon;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerprice.designid == null ? "" : oCustomerprice.designid;
            command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.mfgdesignid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerprice.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerprice;
        }

        public Customerprice Update(Customerprice oCustomerprice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCustomerprice);
                    CustomerpricelistDataService subCustomerpricelist = new CustomerpricelistDataService() { Context = this.Context };
                    subCustomerpricelist.Update(transaction, conn, oCustomerprice.Customerpricelist, oCustomerprice.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerprice);
            }

            return oCustomerprice;
        }

        public Customerprice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerprice oCustomerprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerprice.createdon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerprice.isactive;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerprice.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerprice.modifiedon;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerprice.designid == null ? "" : oCustomerprice.designid;
            command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.mfgdesignid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerprice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerprice;
        }


        public bool Delete(Customerprice oCustomerprice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerprice);
                    CustomerpricelistDataService subCustomerpricelist = new CustomerpricelistDataService() { Context = this.Context };
                    subCustomerpricelist.DeleteAll(oCustomerprice.id, oCustomerprice.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerprice);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerprice oCustomerprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerprice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerprice.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerprice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerprice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }


    }

}
