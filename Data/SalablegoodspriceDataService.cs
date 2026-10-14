using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class SalablegoodspriceDataService : BaseData
    {
        const string SQL_Search = @"select locationid, version, createdon, modifiedon, createdby, modifiedby, isactive, approvedby, removedby, addon, removedon, price, id, mrp, batchid, barcode from salablegoodsprice where id=@id";


        const string SQL_Insert = @"INSERT INTO public.salablegoodsprice(locationid, version, createdon, modifiedon, createdby, modifiedby, isactive, approvedby, removedby, addon, removedon, price, mrp, batchid, barcode,invoiceitemid) VALUES(@locationid, @version, @createdon, @modifiedon, @createdby, @modifiedby, @isactive, @approvedby, @removedby, @addon, @removedon, @price, @mrp, @batchid, @barcode,@invoiceitemid) RETURNING id";


        const string SQL_Update = @"UPDATE public.salablegoodsprice SET locationid=@locationid, version=@version+1, modifiedon=@modifiedon, 
        modifiedby=@modifiedby, approvedby=@approvedby, removedby=@removedby, addon=@addon, removedon=@removedon, price=@price, mrp=@mrp, 
        batchid=@batchid, barcode=@barcode,isactive=@isactive where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.salablegoodsprice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Salablegoodsprice> Search(SalablegoodspriceCreteria _salablegoodsprice)
        {
            List<Salablegoodsprice> lst = new List<Salablegoodsprice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_salablegoodsprice.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _salablegoodsprice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    if(!_salablegoodsprice.includedeleted)
                        qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("barcode", _salablegoodsprice.barcode, "=");
                    if(_salablegoodsprice.locationid>0)
                        qb.AddParameter("locationid", _salablegoodsprice.locationid, "=");
                    if (_salablegoodsprice.price != 0)
                        qb.AddORParameter("price", _salablegoodsprice.price, "=","mrp","mrp",_salablegoodsprice.price,"=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Salablegoodsprice osalablegoodsprice = new Salablegoodsprice();
                        osalablegoodsprice.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        osalablegoodsprice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osalablegoodsprice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osalablegoodsprice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osalablegoodsprice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osalablegoodsprice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osalablegoodsprice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osalablegoodsprice.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        osalablegoodsprice.removedby = reader["removedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["removedby"]);
                        osalablegoodsprice.addon = reader["addon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["addon"]);
                        osalablegoodsprice.removedon = reader["removedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["removedon"]);
                        osalablegoodsprice.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        osalablegoodsprice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osalablegoodsprice.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        osalablegoodsprice.batchid = reader["batchid"] == DBNull.Value ? "" : reader["batchid"].ToString();
                        osalablegoodsprice.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        lst.Add(osalablegoodsprice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _salablegoodsprice);
            }
            return lst;
        }

        public Salablegoodsprice Insert(Salablegoodsprice oSalablegoodsprice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oSalablegoodsprice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoodsprice);
            }

            return oSalablegoodsprice;
        }

        public Salablegoodsprice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoodsprice oSalablegoodsprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.locationid;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.invoiceitemid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodsprice.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoodsprice.isactive;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.approvedby;
            command.Parameters.Add("removedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.removedby;
            command.Parameters.Add("addon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.addon;
            command.Parameters.Add("removedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.removedon;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodsprice.price;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodsprice.mrp;
            command.Parameters.Add("batchid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodsprice.batchid == null ? "" : oSalablegoodsprice.batchid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodsprice.barcode == null ? "" : oSalablegoodsprice.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalablegoodsprice.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            if(oSalablegoodsprice.sync)
            {
                var param = new SortedList<string, object>();
                param.Add("barcode", oSalablegoodsprice.barcode);
                ExecuteQuery("Update stock set ismultiprice=true where barcode=@barcode", param,
                    conn, transaction);
            }
            return oSalablegoodsprice;
        }

        public Salablegoodsprice Update(Salablegoodsprice oSalablegoodsprice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSalablegoodsprice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoodsprice);
            }

            return oSalablegoodsprice;
        }

        public Salablegoodsprice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoodsprice oSalablegoodsprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.locationid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodsprice.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.modifiedon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.modifiedby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoodsprice.isactive;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.approvedby;
            command.Parameters.Add("removedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.removedby;
            command.Parameters.Add("addon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.addon;
            command.Parameters.Add("removedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalablegoodsprice.removedon;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodsprice.price;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.id;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalablegoodsprice.mrp;
            command.Parameters.Add("batchid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodsprice.batchid == null ? "" : oSalablegoodsprice.batchid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalablegoodsprice.barcode == null ? "" : oSalablegoodsprice.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalablegoodsprice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSalablegoodsprice;
        }


        public bool Delete(Salablegoodsprice oSalablegoodsprice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oSalablegoodsprice.removedon = DateTime.Now;
                    oSalablegoodsprice.removedby = Context.UserID;
                    oSalablegoodsprice.modifiedby = Context.UserID;
                    oSalablegoodsprice.isactive = false;
                    Update(transaction, conn, oSalablegoodsprice);
                    transaction.Commit();
                    conn.Close();
                    hasSuccessCount = true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalablegoodsprice);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Salablegoodsprice oSalablegoodsprice)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalablegoodsprice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSalablegoodsprice.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalablegoodsprice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalablegoodsprice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<Salablegoodsprice> Search(long locationid, decimal price,
            string barcode)
        {

            var items = Search(new SalablegoodspriceCreteria()
            {
                locationid = locationid,
                price = price,
                barcode = barcode
            });

            if(locationid>0)
                items.AddRange(Search(new SalablegoodspriceCreteria()
                {
                    locationid = 0,
                    price = price,
                    barcode = barcode
                }));
            return items;
        }

        internal List<Salablegoodsprice> SavePriceList(SalablegoodspriceCreteria item)
        {
            List<Salablegoodsprice> list = new List<Salablegoodsprice>();
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach(var itm in item.locationids)
                    {
                        list.Add(Insert(transaction, conn,new Salablegoodsprice()
                        {
                            price = item.price,
                            mrp = item.mrp,
                            barcode = item.barcode,
                            approvedby = Context.UserID,
                            createdon = DateTime.Now,
                            modifiedon = DateTime.Now,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            isactive = true,
                            locationid = itm,
                            addon = DateTime.Now
                        }));
                    }
                    var param = new SortedList<string, object>();
                    param.Add("barcode", item.barcode);
                    ExecuteQuery("update stock set ismultiprice=true where barcode=@barcode", param, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    item);
            }
            return list;
        }

        //internal void CheckMultiplePrice(Stock oStock, NpgsqlTransaction transaction, NpgsqlConnection conn)
        //{
        //    string itmbuscode = "";
        //    string query = "select itmbuscode from lrinvoiceitem where id=" + oStock.invoiceitemid;
        //    using (var reader = ExecuteReader(query, null, conn, transaction))
        //    {
        //        if (reader.Read() && !reader.IsDBNull(0))
        //            itmbuscode = reader["itmbuscode"].ToString();
        //    }
        //    if(!string.IsNullOrEmpty(itmbuscode))
        //    {
        //        query = @"select distinct price,mrp from lrinvoiceitem 
        //                  inner join stock on stock.invoiceitemid = lrinvoiceitem.id
        //                  where itmbuscode=@itmbuscode";
        //    }
        //}
    }

}
