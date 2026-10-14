using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class PricedropDataService : SAASData
    {
        const string SQL_Search = @"select registerno, modifiedon, isactive, invoiceitemid, itemid, productid, taxid, suppliercompanyid, supplierid, invoiceno, discountpercentage, taxpercentage, invoicedate, createdby, locationid, companyid, receivedemployeeid, approvedemployeeid, status, pricedropdate, version, id, modifiedby, stockid, barcode, salablegoodsid, qty, rate, discount, sellingprice, dealerprice, mrp, minrate, tax, pricedrop, createdon, printingname, statusupdatedon, newdealerprice, newminrate, newsellingprice, newmrp from pricedrop where id=@id";

        const string SQL_Insert = @"INSERT INTO public.pricedrop(registerno, modifiedon, isactive, invoiceitemid, itemid, productid, taxid, suppliercompanyid, supplierid, invoiceno, discountpercentage, taxpercentage, invoicedate, createdby, locationid, companyid, receivedemployeeid, approvedemployeeid, status, pricedropdate, version, modifiedby, stockid, barcode, salablegoodsid, qty, rate, discount, sellingprice, dealerprice, mrp, minrate, tax, pricedrop, createdon, printingname, statusupdatedon, newdealerprice, newminrate, newsellingprice, newmrp) VALUES(@registerno, @modifiedon, @isactive, @invoiceitemid, @itemid, @productid, @taxid, @suppliercompanyid, @supplierid, @invoiceno, @discountpercentage, @taxpercentage, @invoicedate, @createdby, @locationid, @companyid, @receivedemployeeid, @approvedemployeeid, @status, @pricedropdate, @version, @modifiedby, @stockid, @barcode, @salablegoodsid, @qty, @rate, @discount, @sellingprice, @dealerprice, @mrp, @minrate, @tax, @pricedrop, @createdon, @printingname, @statusupdatedon, @newdealerprice, @newminrate, @newsellingprice, @newmrp) RETURNING id";

        const string SQL_Update = @"UPDATE public.pricedrop SET registerno=@registerno, modifiedon=@modifiedon, invoiceitemid=@invoiceitemid, itemid=@itemid, productid=@productid, taxid=@taxid, suppliercompanyid=@suppliercompanyid, supplierid=@supplierid, invoiceno=@invoiceno, discountpercentage=@discountpercentage, taxpercentage=@taxpercentage, invoicedate=@invoicedate, locationid=@locationid, companyid=@companyid, receivedemployeeid=@receivedemployeeid, approvedemployeeid=@approvedemployeeid, status=@status, pricedropdate=@pricedropdate, version=@version+1, modifiedby=@modifiedby, stockid=@stockid, barcode=@barcode, salablegoodsid=@salablegoodsid, qty=@qty, rate=@rate, discount=@discount, sellingprice=@sellingprice, dealerprice=@dealerprice, mrp=@mrp, minrate=@minrate, tax=@tax, pricedrop=@pricedrop, printingname=@printingname, statusupdatedon=@statusupdatedon, newdealerprice=@newdealerprice, newminrate=@newminrate, newsellingprice=@newsellingprice, newmrp=@newmrp where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.pricedrop set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Pricedrop> SearchLatest(List<string> barcodes)
        {
            List<Pricedrop> lst = new List<Pricedrop>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                string query = string.Format(@"with pricedroplast as (
			    SELECT barcode, newdealerprice,newmrp,newminrate, row_number() OVER (PARTITION BY barcode ORDER BY pricedropdate DESC,id DESC) as ranknumber  
			    FROM pricedrop
				where barcode in ({0}) and pricedrop.isactive = true and pricedrop.status != 3
				)
				
				select * from pricedroplast where ranknumber = 1", string.Join(",", barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = query;

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricedrop opricedrop = new Pricedrop();
                        opricedrop.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opricedrop.newdealerprice = reader["newdealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newdealerprice"]);
                        opricedrop.newminrate = reader["newminrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newminrate"]);
                        opricedrop.newmrp = reader["newmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newmrp"]);
                        lst.Add(opricedrop);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, barcodes);
            }
            return lst;
        }
        public List<Pricedrop> Search(PricedropCreteria _pricedrop)
        {
            List<Pricedrop> lst = new List<Pricedrop>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_pricedrop.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricedrop.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_pricedrop.Code))
                    //    qb.AddParameter("code", _pricedrop.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricedrop opricedrop = new Pricedrop();
                        opricedrop.registerno = reader["registerno"] == DBNull.Value ? "" : reader["registerno"].ToString();
                        opricedrop.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opricedrop.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opricedrop.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        opricedrop.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        opricedrop.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        opricedrop.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        opricedrop.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        opricedrop.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opricedrop.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        opricedrop.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        opricedrop.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        opricedrop.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        opricedrop.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opricedrop.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opricedrop.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opricedrop.receivedemployeeid = reader["receivedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedemployeeid"]);
                        opricedrop.approvedemployeeid = reader["approvedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedemployeeid"]);
                        opricedrop.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        opricedrop.pricedropdate = reader["pricedropdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["pricedropdate"]);
                        opricedrop.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opricedrop.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opricedrop.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opricedrop.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        opricedrop.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        opricedrop.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        opricedrop.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        opricedrop.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        opricedrop.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        opricedrop.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        opricedrop.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        opricedrop.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        opricedrop.minrate = reader["minrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minrate"]);
                        opricedrop.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        opricedrop.pricedrop = reader["pricedrop"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pricedrop"]);
                        opricedrop.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opricedrop.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        opricedrop.statusupdatedon = reader["statusupdatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["statusupdatedon"]);
                        opricedrop.newdealerprice = reader["newdealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newdealerprice"]);
                        opricedrop.newminrate = reader["newminrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newminrate"]);
                        opricedrop.newsellingprice = reader["newsellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newsellingprice"]);
                        opricedrop.newmrp = reader["newmrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["newmrp"]);
                        lst.Add(opricedrop);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _pricedrop);
            }
            return lst;
        }

        public Pricedrop Insert(Pricedrop oPricedrop)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oPricedrop);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricedrop);
                throw exp;
            }

            return oPricedrop;
        }

        public Pricedrop Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricedrop oPricedrop)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("registerno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.registerno == null ? "" : oPricedrop.registerno;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricedrop.isactive;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.productid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.taxid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.suppliercompanyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.supplierid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.invoiceno == null ? "" : oPricedrop.invoiceno;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.discountpercentage;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.taxpercentage;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.invoicedate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.createdby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.companyid;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.approvedemployeeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricedrop.status;
            command.Parameters.Add("pricedropdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.pricedropdate;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricedrop.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.modifiedby;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.barcode == null ? "" : oPricedrop.barcode;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.rate;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.discount;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.sellingprice;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.dealerprice;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.mrp;
            command.Parameters.Add("minrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.minrate;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.tax;
            command.Parameters.Add("pricedrop", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.pricedrop;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.createdon;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.printingname == null ? "" : oPricedrop.printingname;
            command.Parameters.Add("statusupdatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.statusupdatedon;
            command.Parameters.Add("newdealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newdealerprice;
            command.Parameters.Add("newminrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newminrate;
            command.Parameters.Add("newsellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newsellingprice;
            command.Parameters.Add("newmrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newmrp;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPricedrop.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (oPricedrop.stockid > 0)
            {
                command.CommandText = "Update stock set price=@newsellingprice,discount=0,version=version+1 where id=@stockid";
                command.ExecuteNonQuery();
                
            }
            if (oPricedrop.salablegoodsid > 0)
            {
                command.CommandText = "Update salablegoods set price=@newsellingprice,discount=0,version=version+1 where id=@salablegoodsid";
                command.ExecuteNonQuery();
            }
            return oPricedrop;
        }

        public Pricedrop Update(Pricedrop oPricedrop)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPricedrop);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricedrop);
                throw exp;
            }

            return oPricedrop;
        }

        public Pricedrop Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricedrop oPricedrop)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("registerno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.registerno == null ? "" : oPricedrop.registerno;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.modifiedon;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricedrop.isactive;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.invoiceitemid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.productid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.taxid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.suppliercompanyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.supplierid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.invoiceno == null ? "" : oPricedrop.invoiceno;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.discountpercentage;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.taxpercentage;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.invoicedate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.createdby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.companyid;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.approvedemployeeid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricedrop.status;
            command.Parameters.Add("pricedropdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.pricedropdate;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricedrop.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.modifiedby;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.stockid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.barcode == null ? "" : oPricedrop.barcode;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.rate;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.discount;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.sellingprice;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.dealerprice;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.mrp;
            command.Parameters.Add("minrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.minrate;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.tax;
            command.Parameters.Add("pricedrop", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.pricedrop;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.createdon;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricedrop.printingname == null ? "" : oPricedrop.printingname;
            command.Parameters.Add("statusupdatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.statusupdatedon;
            command.Parameters.Add("newdealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newdealerprice;
            command.Parameters.Add("newminrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newminrate;
            command.Parameters.Add("newsellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newsellingprice;
            command.Parameters.Add("newmrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricedrop.newmrp;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPricedrop.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            if (oPricedrop.stockid > 0)
            {
                command.CommandText = "Update stock set price=@newsellingprice,discount=0,version=version+1 where id=@stockid";
                command.ExecuteNonQuery();

            }
            if (oPricedrop.salablegoodsid > 0)
            {
                command.CommandText = "Update salablegoods set price=@newsellingprice,discount=0,version=version+1 where id=@salablegoodsid";
                command.ExecuteNonQuery();
            }
            return oPricedrop;
        }


        public bool Delete(Pricedrop oPricedrop)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPricedrop);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricedrop);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricedrop oPricedrop)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricedrop.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricedrop.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricedrop.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricedrop.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal PricedropMaster Update(PricedropMaster result)
        {

            List<Pricedrop> items = result.Items;
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("PRICEDROP", 0, items.First().companyid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string seqno = sequence.GetNextNumber(Context,conn, transaction);
                    foreach (var doaitem in items)
                    {

                        if (result.status == Pricedrop.Status.CreditNote || result.status == Pricedrop.Status.DebitNote)
                        {
                            Data.LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                            var source = lrinvoiceitemsDataService.Search(new LrinvoiceitemsCreteria()
                            {
                                id = doaitem.invoiceitemid
                            });
                            if (source.Count == 1)
                            {
                                LrinvoicereturnDataService lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
                                lrinvoicereturnDataService.Context = this.Context;
                                var returnitem = lrinvoicereturnDataService.AddDirectDebitNote(
                                    Lrinvoicereturn.ReturnTypes.PriceDropCreditNote, source[0], doaitem);

                                doaitem.status = (int)result.status;
                                doaitem.statusupdatedon = result.pricedropdate;
                                Update(transaction, conn, doaitem);
                            }
                        }
                        if (result.status == Pricedrop.Status.Cancelled)
                        {
                            doaitem.status = (int)result.status;
                            doaitem.statusupdatedon = result.pricedropdate;
                            Update(transaction, conn, doaitem);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }

                return result;
            }
            catch (Exception exp)
            {
                result.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return result;
        }

        internal PricedropMaster Insert(PricedropMaster item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("PRICEDROP", 0, item.Items.First().companyid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    string seqno = sequence.GetNextNumber(Context,conn, transaction);
                    foreach (var doaitem in item.Items)
                    {
                        doaitem.modifiedby = Context.UserID;
                        doaitem.createdby = Context.UserID;
                        doaitem.createdon = DateTimeNow;
                        doaitem.modifiedon = DateTimeNow;
                        doaitem.receivedemployeeid = item.receivedemployeeid;
                        doaitem.pricedropdate = item.pricedropdate;
                        doaitem.registerno = seqno;
                        Insert(transaction, conn, doaitem);
                    }
                    transaction.Commit();
                    conn.Close();
                }

                return item;
            }
            catch (Exception exp)
            {
                item.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

            }
            return item;
        }

    }

}
