using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class SplitbarcodehistoryDataService : BaseData
    {
        const string SQL_Search = @"select splitbarcodehistory.id, splitbarcodehistory.locationid, splitbarcodehistory.companyid, 
                splitbarcodehistory.modifiedon, splitbarcodehistory.createdon, splitbarcodehistory.version, 
                splitbarcodehistory.modifiedby, splitbarcodehistory.createdby, splitbarcodehistory.isactive, 
                sourceproductid, convertedproductid, barcode, stockid, salablegoodsid, stockqty, splitqty, noofqtyeachbarcode, 
                noofnewbarcode, isbit, newprice, newdiscount, price, discount, barcodelist, newbarcode, splitbarcodehistory.code,
                employee.name createdbyemployee 
                from splitbarcodehistory left outer join users on users.id = splitbarcodehistory.createdby
                left outer join employee on employee.id = users.employeeid
                where splitbarcodehistory.id=@id";


        const string SQL_Insert = @"INSERT INTO public.splitbarcodehistory(locationid, companyid, modifiedon, createdon, version, modifiedby, createdby, isactive, sourceproductid, convertedproductid, barcode, stockid, salablegoodsid, stockqty, splitqty, noofqtyeachbarcode, noofnewbarcode, isbit, newprice, newdiscount, price, discount, barcodelist, newbarcode, code) VALUES(@locationid, @companyid, @modifiedon, @createdon, @version, @modifiedby, @createdby, @isactive, @sourceproductid, @convertedproductid, @barcode, @stockid, @salablegoodsid, @stockqty, @splitqty, @noofqtyeachbarcode, @noofnewbarcode, @isbit, @newprice, @newdiscount, @price, @discount, @barcodelist, @newbarcode, @code) RETURNING id";


        const string SQL_Update = @"UPDATE public.splitbarcodehistory SET locationid=@locationid, companyid=@companyid, modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby, sourceproductid=@sourceproductid, convertedproductid=@convertedproductid, barcode=@barcode, stockid=@stockid, salablegoodsid=@salablegoodsid, stockqty=@stockqty, splitqty=@splitqty, noofqtyeachbarcode=@noofqtyeachbarcode, noofnewbarcode=@noofnewbarcode, isbit=@isbit, newprice=@newprice, newdiscount=@newdiscount, price=@price, discount=@discount, barcodelist=@barcodelist, newbarcode=@newbarcode, code=@code where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.splitbarcodehistory set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Splitbarcodehistory> Search(SplitbarcodehistoryCreteria _splitbarcodehistory)
        {
            List<Splitbarcodehistory> lst = new List<Splitbarcodehistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_splitbarcodehistory.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _splitbarcodehistory.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("splitbarcodehistory.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_splitbarcodehistory.barcode))
                        qb.AddParameter("barcode", _splitbarcodehistory.barcode, "=");
                    if(_splitbarcodehistory.companyid>0)
                        qb.AddParameter("splitbarcodehistory.companyid", _splitbarcodehistory.companyid, "=");
                    if (_splitbarcodehistory.locationid > 0)
                        qb.AddParameter("splitbarcodehistory.locationid", _splitbarcodehistory.locationid, "=");
                    if (_splitbarcodehistory.fromdate.Year > 1900)
                        qb.AddParameter("splitbarcodehistory.createdon", "fromdate", _splitbarcodehistory.fromdate.Date, ">=");
                    if (_splitbarcodehistory.todate.Year > 1900)
                        qb.AddParameter("splitbarcodehistory.createdon", "todate", _splitbarcodehistory.todate.Date.AddDays(1), "<");
                    qb.SortField = "Splitbarcodehistory.id";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Splitbarcodehistory osplitbarcodehistory = new Splitbarcodehistory();
                        osplitbarcodehistory.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osplitbarcodehistory.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        osplitbarcodehistory.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        osplitbarcodehistory.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osplitbarcodehistory.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osplitbarcodehistory.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osplitbarcodehistory.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osplitbarcodehistory.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osplitbarcodehistory.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osplitbarcodehistory.sourceproductid = reader["sourceproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceproductid"]);
                        osplitbarcodehistory.convertedproductid = reader["convertedproductid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["convertedproductid"]);
                        osplitbarcodehistory.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        osplitbarcodehistory.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        osplitbarcodehistory.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        osplitbarcodehistory.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        osplitbarcodehistory.splitqty = reader["splitqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["splitqty"]);
                        osplitbarcodehistory.noofqtyeachbarcode = reader["noofqtyeachbarcode"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofqtyeachbarcode"]);
                        osplitbarcodehistory.noofnewbarcode = reader["noofnewbarcode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofnewbarcode"]);
                        osplitbarcodehistory.isbit = reader["isbit"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbit"]);
                        osplitbarcodehistory.newprice = reader["newprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newprice"]);
                        osplitbarcodehistory.newdiscount = reader["newdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newdiscount"]);
                        osplitbarcodehistory.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        osplitbarcodehistory.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        osplitbarcodehistory.barcodelist_json = reader["barcodelist"] == DBNull.Value ? null : reader["barcodelist"].ToString();
                        osplitbarcodehistory.newbarcode = reader["newbarcode"] == DBNull.Value ? "" : reader["newbarcode"].ToString();
                        osplitbarcodehistory.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        osplitbarcodehistory.createdemployee = reader["createdbyemployee"] == DBNull.Value ? "" : reader["createdbyemployee"].ToString();
                        osplitbarcodehistory.newbarcodecount = osplitbarcodehistory.barcodelist.Count - 1;
                        lst.Add(osplitbarcodehistory);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _splitbarcodehistory);
            }
            return lst;
        }

        public Splitbarcodehistory Insert(Splitbarcodehistory oSplitbarcodehistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSplitbarcodehistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSplitbarcodehistory);
            }

            return oSplitbarcodehistory;
        }

        public Splitbarcodehistory Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Splitbarcodehistory oSplitbarcodehistory)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.companyid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSplitbarcodehistory.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSplitbarcodehistory.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSplitbarcodehistory.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSplitbarcodehistory.isactive;
            command.Parameters.Add("sourceproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.sourceproductid;
            command.Parameters.Add("convertedproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.convertedproductid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSplitbarcodehistory.barcode == null ? "" : oSplitbarcodehistory.barcode;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.stockid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.salablegoodsid;
            command.Parameters.Add("stockqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.stockqty;
            command.Parameters.Add("splitqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.splitqty;
            command.Parameters.Add("noofqtyeachbarcode", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.noofqtyeachbarcode;
            command.Parameters.Add("noofnewbarcode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSplitbarcodehistory.noofnewbarcode;
            command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSplitbarcodehistory.isbit;
            command.Parameters.Add("newprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.newprice;
            command.Parameters.Add("newdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.newdiscount;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.discount;
            command.Parameters.Add("barcodelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSplitbarcodehistory.barcodelist_json == null ? "null" : oSplitbarcodehistory.barcodelist_json;
            command.Parameters.Add("newbarcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSplitbarcodehistory.newbarcode == null ? "" : oSplitbarcodehistory.newbarcode;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSplitbarcodehistory.code == null ? "" : oSplitbarcodehistory.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSplitbarcodehistory.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSplitbarcodehistory;
        }

        public Splitbarcodehistory Update(Splitbarcodehistory oSplitbarcodehistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSplitbarcodehistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSplitbarcodehistory);
            }

            return oSplitbarcodehistory;
        }

        public Splitbarcodehistory Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Splitbarcodehistory oSplitbarcodehistory)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.id;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.companyid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSplitbarcodehistory.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSplitbarcodehistory.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSplitbarcodehistory.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSplitbarcodehistory.isactive;
            command.Parameters.Add("sourceproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.sourceproductid;
            command.Parameters.Add("convertedproductid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.convertedproductid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSplitbarcodehistory.barcode == null ? "" : oSplitbarcodehistory.barcode;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.stockid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.salablegoodsid;
            command.Parameters.Add("stockqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.stockqty;
            command.Parameters.Add("splitqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.splitqty;
            command.Parameters.Add("noofqtyeachbarcode", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.noofqtyeachbarcode;
            command.Parameters.Add("noofnewbarcode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSplitbarcodehistory.noofnewbarcode;
            command.Parameters.Add("isbit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSplitbarcodehistory.isbit;
            command.Parameters.Add("newprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.newprice;
            command.Parameters.Add("newdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.newdiscount;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSplitbarcodehistory.discount;
            command.Parameters.Add("barcodelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSplitbarcodehistory.barcodelist_json == null ? "null" : oSplitbarcodehistory.barcodelist_json;
            command.Parameters.Add("newbarcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSplitbarcodehistory.newbarcode == null ? "" : oSplitbarcodehistory.newbarcode;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSplitbarcodehistory.code == null ? "" : oSplitbarcodehistory.code;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSplitbarcodehistory.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSplitbarcodehistory;
        }


        public bool Delete(Splitbarcodehistory oSplitbarcodehistory)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSplitbarcodehistory);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSplitbarcodehistory);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Splitbarcodehistory oSplitbarcodehistory)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSplitbarcodehistory.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSplitbarcodehistory.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSplitbarcodehistory.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSplitbarcodehistory.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal void Save(List<SplitBarcode> obarcodereport)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("splitbarcode.code", 0, 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var code = sequence.GetNextNumber(Context, conn, transaction);

                    foreach (var item in obarcodereport)
                    {
                        Insert(transaction, conn, new Splitbarcodehistory()
                        {
                             barcode = item.barcode,
                             newbarcode = item.newbarcode,
                             code = code,
                             barcodelist = new List<string>( item.newbarcodelist.Split(',')),
                             companyid = item.companyid,
                             convertedproductid = item.changeproductid,
                             createdby = Context.UserID,
                             createdon = DateTimeNow,
                             discount = item.discount,
                             isactive = true,
                             isbit = item.isbit,
                             locationid = item.locationid,
                             modifiedby = Context.UserID,
                             modifiedon = DateTimeNow,
                             newdiscount = item.ndiscount,
                             newprice = item.nprice,
                             price = item.price,
                             stockid = item.stockid,
                             sourceproductid = item.productid,
                             stockqty = item.locationid>0 ? item.rstock : item.wstock,
                             splitqty = item.splitpiece * item.splitqty,
                             noofnewbarcode = item.splitpiece,
                             noofqtyeachbarcode = item.splitqty,
                             salablegoodsid = item.salablegoodsid,
                             version =1
                        });
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obarcodereport);

            }
        }
    }

}
