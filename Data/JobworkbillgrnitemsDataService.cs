using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;

namespace InfyPOS.Data
{
    public class JobworkbillgrnitemsDataService : BaseData
    {
        const string SQL_Search = @"select chargetaxid, chargetaxpercentage, chargetax, materialcost, chargerate, materialsalevalue,cost,mcost, createdby, isactive, invoiceitemid, productid, stockid, salablegoodsid, newinvoiceitemid, designid, styleid, patternid, brandid, sizeid,sleeveid, typeid, sno, id, jobworkbillgrnid, qty, generatedqty, modifiedon, createdon, version, modifiedby, jobworkbillitemsid, chargetotal, chargegross, chargeid, attributedisplay,imageid, dealerprice, finalprice,lrinvoiceid,colourid,materialid,barcode,barcodes from jobworkbillgrnitems where id=@id";
        const string SQL_SearchAll = @"select chargetaxid, chargetaxpercentage, chargetax, materialcost, chargerate, materialsalevalue,cost,mcost, createdby, isactive, invoiceitemid, productid, stockid, salablegoodsid, newinvoiceitemid, designid, styleid, patternid, brandid, sizeid,sleeveid, typeid, sno, id, jobworkbillgrnid, qty, generatedqty, modifiedon, createdon, version, modifiedby, jobworkbillitemsid, chargetotal, chargegross, chargeid, attributedisplay,imageid, finalprice,dealerprice,lrinvoiceid,colourid,materialid,barcode,barcodes from jobworkbillgrnitems where isactive=true and jobworkbillgrnid=@jobworkbillgrnid";


        const string SQL_Insert = @"
        UPDATE jobworkbillitems SET rqty=rqty+@qty where id=@jobworkbillitemsid;
        INSERT INTO public.jobworkbillgrnitems(chargetaxid, chargetaxpercentage, chargetax, materialcost, chargerate, materialsalevalue,cost,mcost, createdby, isactive, invoiceitemid, productid, stockid, salablegoodsid, newinvoiceitemid, designid, styleid, patternid, brandid, sizeid,sleeveid, typeid, sno, jobworkbillgrnid, qty, generatedqty, modifiedon, createdon, version, modifiedby, jobworkbillitemsid, chargetotal, chargegross, chargeid, attributedisplay,imageid, finalprice,dealerprice,lrinvoiceid,colourid,materialid,barcode)
        VALUES(@chargetaxid, @chargetaxpercentage, @chargetax, @materialcost, @chargerate, @materialsalevalue,@cost,@mcost, @createdby, @isactive, @invoiceitemid, @productid, @stockid, @salablegoodsid, @newinvoiceitemid, @designid, @styleid, @patternid, @brandid, @sizeid,@sleeveid, @typeid, @sno, @jobworkbillgrnid, @qty, @generatedqty, @modifiedon, @createdon, @version, @modifiedby, @jobworkbillitemsid, @chargetotal, @chargegross, @chargeid, @attributedisplay,@imageid, @finalprice,@dealerprice,@lrinvoiceid,@colourid,@materialid,@barcode) RETURNING id";

        const string SQL_Update = @"
        UPDATE jobworkbillitems SET rqty=rqty-qty+@qty from jobworkbillgrnitems
        where jobworkbillgrnitems.jobworkbillitemsid = jobworkbillitems.id and jobworkbillgrnitems.id=@id AND rqty-qty+@qty>=0;

        UPDATE public.jobworkbillgrnitems SET chargetaxid=@chargetaxid, chargetaxpercentage=@chargetaxpercentage, chargetax=@chargetax, materialcost=@materialcost, chargerate=@chargerate, 
        materialsalevalue=@materialsalevalue, invoiceitemid=@invoiceitemid, productid=@productid, stockid=@stockid, salablegoodsid=@salablegoodsid, newinvoiceitemid=@newinvoiceitemid, designid=@designid, styleid=@styleid, patternid=@patternid, brandid=@brandid, sizeid=@sizeid,sleeveid=@sleeveid, typeid=@typeid, 
        sno=@sno, jobworkbillgrnid=@jobworkbillgrnid, qty=@qty, generatedqty=@generatedqty, modifiedon=@modifiedon, dealerprice=@dealerprice,
        version=@version+1, modifiedby=@modifiedby, jobworkbillitemsid=@jobworkbillitemsid, chargetotal=@chargetotal,imageid=@imageid,cost=@cost,mcost=@mcost,
        chargegross=@chargegross, chargeid=@chargeid, attributedisplay=@attributedisplay, finalprice=@finalprice,barcode=@barcode,
        lrinvoiceid=@lrinvoiceid,colourid=@colourid,materialid=@materialid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
        UPDATE jobworkbillitems SET rqty=rqty-qty from jobworkbillgrnitems
        where jobworkbillgrnitems.jobworkbillitemsid = jobworkbillitems.id and jobworkbillgrnitems.id=@id;
        update public.jobworkbillgrnitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"
        UPDATE jobworkbillitems SET rqty=rqty-qty from jobworkbillgrnitems
        where jobworkbillgrnitems.jobworkbillitemsid = jobworkbillitems.id and jobworkbillgrnid=@jobworkbillgrnid;
        update public.jobworkbillgrnitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where jobworkbillgrnid=@jobworkbillgrnid";
        public List<Jobworkbillgrnitems> Search(JobworkbillgrnitemsCreteria _jobworkbillgrnitems)
        {
            List<Jobworkbillgrnitems> lst = new List<Jobworkbillgrnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_jobworkbillgrnitems.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworkbillgrnitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_jobworkbillgrnitems.jobworkbillgrnid > 0)
                        qb.AddParameter("jobworkbillgrnid", _jobworkbillgrnitems.jobworkbillgrnid, "=");
                    //if (!string.IsNullOrEmpty(_jobworkbillgrnitems.Code))
                    //    qb.AddParameter("code", _jobworkbillgrnitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkbillgrnitems ojobworkbillgrnitems = new Jobworkbillgrnitems();
                        ojobworkbillgrnitems.chargetaxid = reader["chargetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetaxid"]);
                        ojobworkbillgrnitems.chargetaxpercentage = reader["chargetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetaxpercentage"]);
                        ojobworkbillgrnitems.chargetax = reader["chargetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetax"]);
                        ojobworkbillgrnitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillgrnitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillgrnitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);

                        ojobworkbillgrnitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        ojobworkbillgrnitems.mcost = reader["mcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mcost"]);

                        ojobworkbillgrnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbillgrnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbillgrnitems.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        ojobworkbillgrnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ojobworkbillgrnitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ojobworkbillgrnitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        ojobworkbillgrnitems.newinvoiceitemid = reader["newinvoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["newinvoiceitemid"]);
                        ojobworkbillgrnitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ojobworkbillgrnitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ojobworkbillgrnitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        ojobworkbillgrnitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        ojobworkbillgrnitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        ojobworkbillgrnitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        ojobworkbillgrnitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        ojobworkbillgrnitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        if (reader["barcodes"] != DBNull.Value)
                        {
                            ojobworkbillgrnitems.barcodes = JsonConvert.DeserializeObject<List<string>>(reader["barcodes"].ToString());
                        }
                        ojobworkbillgrnitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        ojobworkbillgrnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        ojobworkbillgrnitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                        ojobworkbillgrnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillgrnitems.jobworkbillgrnid = reader["jobworkbillgrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillgrnid"]);
                        ojobworkbillgrnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ojobworkbillgrnitems.generatedqty = reader["generatedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["generatedqty"]);
                        ojobworkbillgrnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbillgrnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbillgrnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbillgrnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworkbillgrnitems.jobworkbillitemsid = reader["jobworkbillitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillitemsid"]);
                        ojobworkbillgrnitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                        ojobworkbillgrnitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillgrnitems.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        ojobworkbillgrnitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                        ojobworkbillgrnitems.attributedisplay = reader["attributedisplay"] == DBNull.Value ? "" : reader["attributedisplay"].ToString();
                        ojobworkbillgrnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        ojobworkbillgrnitems.finalprice = reader["finalprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["finalprice"]);
                        ojobworkbillgrnitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        lst.Add(ojobworkbillgrnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbillgrnitems);
            }
            return lst;
        }

        public List<Jobworkbillgrnitems> SearchAll(long _jobworkbillgrnid)
        {
            List<Jobworkbillgrnitems> lst = new List<Jobworkbillgrnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("jobworkbillgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _jobworkbillgrnid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkbillgrnitems ojobworkbillgrnitems = new Jobworkbillgrnitems();
                        ojobworkbillgrnitems.chargetaxid = reader["chargetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargetaxid"]);
                        ojobworkbillgrnitems.chargetaxpercentage = reader["chargetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetaxpercentage"]);
                        ojobworkbillgrnitems.chargetax = reader["chargetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetax"]);
                        ojobworkbillgrnitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillgrnitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillgrnitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillgrnitems.cost = reader["cost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cost"]);
                        ojobworkbillgrnitems.mcost = reader["mcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mcost"]);
                        ojobworkbillgrnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ojobworkbillgrnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ojobworkbillgrnitems.invoiceitemid = reader["invoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceitemid"]);
                        ojobworkbillgrnitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ojobworkbillgrnitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ojobworkbillgrnitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        ojobworkbillgrnitems.newinvoiceitemid = reader["newinvoiceitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["newinvoiceitemid"]);
                        ojobworkbillgrnitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ojobworkbillgrnitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        if (reader["barcodes"] != DBNull.Value)
                        {
                            ojobworkbillgrnitems.barcodes = JsonConvert.DeserializeObject<List<string>>(reader["barcodes"].ToString());
                        }
                        ojobworkbillgrnitems.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        ojobworkbillgrnitems.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        ojobworkbillgrnitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        ojobworkbillgrnitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        ojobworkbillgrnitems.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        ojobworkbillgrnitems.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        ojobworkbillgrnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        ojobworkbillgrnitems.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        ojobworkbillgrnitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                        ojobworkbillgrnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillgrnitems.jobworkbillgrnid = reader["jobworkbillgrnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillgrnid"]);
                        ojobworkbillgrnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ojobworkbillgrnitems.generatedqty = reader["generatedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["generatedqty"]);
                        ojobworkbillgrnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ojobworkbillgrnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ojobworkbillgrnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ojobworkbillgrnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ojobworkbillgrnitems.jobworkbillitemsid = reader["jobworkbillitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkbillitemsid"]);
                        ojobworkbillgrnitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                        ojobworkbillgrnitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillgrnitems.chargeid = reader["chargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["chargeid"]);
                        ojobworkbillgrnitems.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                        ojobworkbillgrnitems.attributedisplay = reader["attributedisplay"] == DBNull.Value ? "" : reader["attributedisplay"].ToString();
                        ojobworkbillgrnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        ojobworkbillgrnitems.finalprice = reader["finalprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["finalprice"]);
                        ojobworkbillgrnitems.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        lst.Add(ojobworkbillgrnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _jobworkbillgrnid);
            }
            return lst;
        }

        public Jobworkbillgrnitems Insert(Jobworkbillgrnitems oJobworkbillgrnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oJobworkbillgrnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillgrnitems);
            }

            return oJobworkbillgrnitems;
        }

        public Jobworkbillgrnitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrnitems oJobworkbillgrnitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("chargetaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.chargetaxid;
            command.Parameters.Add("chargetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargetaxpercentage;
            command.Parameters.Add("chargetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargetax;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.materialcost;
            command.Parameters.Add("chargerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargerate;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.materialsalevalue;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.cost;
            command.Parameters.Add("mcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.mcost;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrnitems.isactive;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.invoiceitemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.productid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.stockid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.salablegoodsid;
            command.Parameters.Add("newinvoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.newinvoiceitemid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.designid == null ? "" : oJobworkbillgrnitems.designid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.barcode == null ? "" : oJobworkbillgrnitems.barcode;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.styleid;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.patternid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.brandid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.sizeid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.sleeveid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.typeid;

            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.materialid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.colourid;

            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrnitems.sno;
            command.Parameters.Add("jobworkbillgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.jobworkbillgrnid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.qty;
            command.Parameters.Add("generatedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.generatedqty;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrnitems.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrnitems.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrnitems.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.modifiedby;
            command.Parameters.Add("jobworkbillitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.jobworkbillitemsid;
            command.Parameters.Add("chargetotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargetotal;
            command.Parameters.Add("chargegross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargegross;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.chargeid;
            command.Parameters.Add("attributedisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.attributedisplay == null ? "" : oJobworkbillgrnitems.attributedisplay;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.imageid == null ? "" : oJobworkbillgrnitems.imageid;
            command.Parameters.Add("finalprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.finalprice;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.dealerprice;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.lrinvoiceid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbillgrnitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oJobworkbillgrnitems;
        }

        public Jobworkbillgrnitems Update(Jobworkbillgrnitems oJobworkbillgrnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oJobworkbillgrnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillgrnitems);
            }

            return oJobworkbillgrnitems;
        }

        public Jobworkbillgrnitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrnitems oJobworkbillgrnitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("chargetaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.chargetaxid;
            command.Parameters.Add("chargetaxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargetaxpercentage;
            command.Parameters.Add("chargetax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargetax;
            command.Parameters.Add("materialcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.materialcost;
            command.Parameters.Add("chargerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargerate;
            command.Parameters.Add("materialsalevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.materialsalevalue;
            command.Parameters.Add("cost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.cost;
            command.Parameters.Add("mcost", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.mcost;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrnitems.isactive;
            command.Parameters.Add("invoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.invoiceitemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.productid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.stockid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.salablegoodsid;
            command.Parameters.Add("newinvoiceitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.newinvoiceitemid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.designid == null ? "" : oJobworkbillgrnitems.designid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.barcode == null ? "" : oJobworkbillgrnitems.barcode;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.styleid;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.patternid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.brandid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.sizeid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.sleeveid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.typeid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.materialid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.colourid;
            command.Parameters.Add("sno", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrnitems.sno;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.id;
            command.Parameters.Add("jobworkbillgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.jobworkbillgrnid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.qty;
            command.Parameters.Add("generatedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.generatedqty;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrnitems.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrnitems.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrnitems.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.modifiedby;
            command.Parameters.Add("jobworkbillitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.jobworkbillitemsid;
            command.Parameters.Add("chargetotal", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargetotal;
            command.Parameters.Add("chargegross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.chargegross;
            command.Parameters.Add("chargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.chargeid;
            command.Parameters.Add("attributedisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.attributedisplay == null ? "" : oJobworkbillgrnitems.attributedisplay;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oJobworkbillgrnitems.imageid == null ? "" : oJobworkbillgrnitems.imageid;
            command.Parameters.Add("finalprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.finalprice;
            command.Parameters.Add("dealerprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oJobworkbillgrnitems.dealerprice;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.lrinvoiceid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oJobworkbillgrnitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oJobworkbillgrnitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Jobworkbillgrnitems> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.jobworkbillgrnid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Jobworkbillgrnitems oJobworkbillgrnitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oJobworkbillgrnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oJobworkbillgrnitems);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Jobworkbillgrnitems oJobworkbillgrnitems)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oJobworkbillgrnitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oJobworkbillgrnitems.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oJobworkbillgrnitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oJobworkbillgrnitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        const string SQL_JobWorkSummary = @"with pendingjobworks
		as
		(
		select sum(oqty-(rqty+dqty)) pqty, sum(oqty) oqty, jobworkbillid,lrinvoiceitems.productid,outproductid,suppliercompanyid from jobworkbillitems
		inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
		inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
		where jobworkbillitems.isactive = true and Jobworkbillitems.oqty-(rqty+dqty)>0
		group by jobworkbillid,outproductid,suppliercompanyid,lrinvoiceitems.productid
		)
		select Jobworkbill.id,pendingjobworks.oqty,Jobworkbill.invoiceno,Jobworkbill.expecteddate,Jobworkbill.invoicedate,entrydate,pendingjobworks.pqty,pendingjobworks.outproductid,pendingjobworks.suppliercompanyid,
		supplier.name jobworker,suppliercompany.name sourcesupplier,inproduct.name inproduct,outproduct.name outproduct,companyid
		from Jobworkbill
		inner join pendingjobworks on pendingjobworks.jobworkbillid = Jobworkbill.id
		inner join supplier on supplier.id = Jobworkbill.supplierid
		inner join suppliercompany on suppliercompany.id =pendingjobworks.suppliercompanyid
		inner join products inproduct on inproduct.id = pendingjobworks.productid
		inner join products outproduct on outproduct.id = pendingjobworks.outproductid
        ";

        const string SQL_JobWorkItems = @" select Jobworkbillgrnitems.id,Jobworkbillgrnitems.barcode,Jobworkbillgrnitems.qty,
		Jobworkbillgrn.invoiceno,Jobworkbillgrn.invoicedate,Jobworkbillgrn.entrydate,Jobworkbillgrn.mcost,Jobworkbillgrn.cost,
		supplier.name jobworker,suppliercompany.name sourcesupplier,inproduct.name inproduct,
		Jobworkbillgrn.companyid, Jobworkbillgrnitems.materialsalevalue, Jobworkbillgrnitems.materialcost, 
		Jobworkbillgrnitems.chargerate, Jobworkbillgrnitems.chargetax, Jobworkbillgrnitems.chargegross, Jobworkbillgrnitems.chargetotal, 
        Jobworkbillgrn.invoiceno Jobworkbillgrnno,Jobworkbillgrn.entrydate Jobworkbillgrndate
		from Jobworkbillgrnitems 
		inner join Jobworkbillgrn on Jobworkbillgrn.id = Jobworkbillgrnitems.Jobworkbillgrnid
		inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
		inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
		inner join supplier on supplier.id = Jobworkbillgrn.supplierid
		inner join suppliercompany on suppliercompany.id = lrinvoice.suppliercompanyid
		inner join products inproduct on inproduct.id = lrinvoiceitems.productid 
        where Jobworkbillgrnitems.isactive = true";

        internal List<Jobworkitems> SearchJobWorkItems(JobworkbillCreteria _jobworbill)
        {
            List<Jobworkitems> lst = new List<Jobworkitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_JobWorkItems;
                qb.AddParameter("Jobworkbillgrn.isactive", true, "=");

                if (_jobworbill.companyid > 0)
                    qb.AddParameter("Jobworkbillgrn.companyid", _jobworbill.companyid, "=");
                if (_jobworbill.supplierid > 0)
                    qb.AddParameter("Jobworkbillgrn.supplierid", _jobworbill.supplierid, "=");
                if (_jobworbill.entryfrom != DateTime.MinValue && _jobworbill.entryto != DateTime.MinValue)
                {
                    qb.AddParameter("Jobworkbillgrn.entrydate", "datefrom", _jobworbill.entryfrom.Date, ">=");
                    qb.AddParameter("Jobworkbillgrn.entrydate", "dateto", _jobworbill.entryto.Date.AddDays(1), "<");
                }

                qb.SortField = "Jobworkbillgrn.id desc ";
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Jobworkitems ojobworkbillitems = new Jobworkitems();
                        ojobworkbillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ojobworkbillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ojobworkbillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ojobworkbillitems.oqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ojobworkbillitems.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        ojobworkbillitems.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        ojobworkbillitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ojobworkbillitems.jobworker = reader["jobworker"] == DBNull.Value ? "" : reader["jobworker"].ToString();
                        ojobworkbillitems.sourcesupplier = reader["sourcesupplier"] == DBNull.Value ? "" : reader["sourcesupplier"].ToString();
                        ojobworkbillitems.inproduct = reader["inproduct"] == DBNull.Value ? "" : reader["inproduct"].ToString();
                        ojobworkbillitems.materialsalevalue = reader["materialsalevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialsalevalue"]);
                        ojobworkbillitems.materialcost = reader["materialcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["materialcost"]);
                        ojobworkbillitems.chargerate = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                        ojobworkbillitems.chargegross = reader["chargegross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargegross"]);
                        ojobworkbillitems.chargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                        ojobworkbillitems.grndate = reader["Jobworkbillgrndate"] == DBNull.Value ? "" : Convert.ToDateTime(reader["Jobworkbillgrndate"]).ToString("dd-MM-yyyy");
                        ojobworkbillitems.grnno = reader["Jobworkbillgrnno"] == DBNull.Value ? "" : reader["Jobworkbillgrnno"].ToString();

                        lst.Add(ojobworkbillitems);
                    }
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "");
            }
            return lst;
        }
        public JobWorkDetails SearchInfo(string barcode, long invoiceitemid)
        {
            JobWorkDetails jobWorkDetails = null;
            string query = @"select suppliercompany.name supplier,city.name city,Jobworkbill.invoiceno,Jobworkbill.entrydate,
		chargerate,chargetotal from jobworkbillitems 
		inner join jobworkbill on Jobworkbill.id = Jobworkbillid
		inner join supplier on supplier.id = supplierid
		inner join suppliercompany on suppliercompany.id = suppliercompanyid
		left outer join referencelist city on city.id = suppliercompany.cityid
        where jobworkbillitems.isactive = true and Jobworkbillitems.oqty-(rqty+dqty)>0 
        and Jobworkbillitems.barcode=@barcode and invoiceitemid=@invoiceitemid";

            var param = new SortedList<string, object>();
            param.Add("invoiceitemid", invoiceitemid);
            param.Add("barcode", barcode);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    jobWorkDetails = new JobWorkDetails();
                    jobWorkDetails.jobworkcharge = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                    jobWorkDetails.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                    jobWorkDetails.entryno = reader["invoiceno"] == DBNull.Value ? "" : Convert.ToString(reader["invoiceno"]);
                    jobWorkDetails.jobworkername = reader["supplier"] == DBNull.Value ? "" : Convert.ToString(reader["supplier"]);
                    jobWorkDetails.jobworkercity = reader["city"] == DBNull.Value ? "" : Convert.ToString(reader["city"]);
                    jobWorkDetails.jobworkchargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                }
            }
            return jobWorkDetails;
        }

        /*WITH BILLED AS (
		SELECT SUM(QTY) GRNQTY,JOBWORKBILLITEMSID FROM jobworkbillgrnitems 
		WHERE ISACTIVE=TRUE
		GROUP BY JOBWORKBILLITEMSID)
		
		SELECT jobworkbillitems.ID,jobworkbillitems.OQTY,
		jobworkbillitems.RQTY,GRNQTY FROM jobworkbillitems
		INNER JOIN BILLED ON BILLED.JOBWORKBILLITEMSID =jobworkbillitems.ID
		WHERE jobworkbillitems.RQTY!=GRNQTY 
		AND jobworkbillitems.ISACTIVE=TRUE
		*/
				
        public JobWorkDetails SearchInfo(long jobworkbillgrnitemid,long itemid)
        {
            JobWorkDetails jobWorkDetails = new JobWorkDetails();
            string query = @"select suppliercompany.name supplier,city.name city,Jobworkbillgrn.invoiceno entryno,Jobworkbillgrn.entrydate,
		style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,
		brand.name brandname,products.name productname,chargerate,chargetotal,Jobworkbillgrnitems.designid,sleeve.name sleevename from Jobworkbillgrnitems
		inner join Jobworkbillgrn on Jobworkbillgrn.id = Jobworkbillgrnid
		inner join supplier on supplier.id = supplierid
		inner join suppliercompany on suppliercompany.id = suppliercompanyid
		left outer join referencelist city on city.id = suppliercompany.cityid
        left outer join items on items.id = @itemid
		left outer join brand brand on brand.id = items.brandid
                        left outer join referencelist colour on colour.id = items.colourid
                        left outer join referencelist size on size.id = items.sizeid
                        left outer join referencelist sleeve on sleeve.id = items.sleeveid
                        left outer join referencelist style on style.id = items.styleid
                        left outer join referencelist pattern on pattern.id = items.patternid
                        left outer join referencelist material on material.id = items.materialid
                        left outer join referencelist type on type.id = items.typeid
                        left outer join products on products.id = items.productid
        where Jobworkbillgrnitems.id=@id";
            var param = new SortedList<string, object>();
            param.Add("id", jobworkbillgrnitemid);
            param.Add("itemid", itemid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    jobWorkDetails.detail = new Items();
                    jobWorkDetails.detail.materialname = reader["materialname"] == DBNull.Value ? "" : Convert.ToString(reader["materialname"]);
                    jobWorkDetails.detail.brandname = reader["brandname"] == DBNull.Value ? "" : Convert.ToString(reader["brandname"]);
                    jobWorkDetails.detail.productname = reader["productname"] == DBNull.Value ? "" : Convert.ToString(reader["productname"]);
                    jobWorkDetails.detail.stylename = reader["stylename"] == DBNull.Value ? "" : Convert.ToString(reader["stylename"]);
                    jobWorkDetails.detail.patternname = reader["patternname"] == DBNull.Value ? "" : Convert.ToString(reader["patternname"]);
                    jobWorkDetails.detail.colourname = reader["colourname"] == DBNull.Value ? "" : Convert.ToString(reader["colourname"]);
                    jobWorkDetails.detail.typename = reader["typename"] == DBNull.Value ? "" : Convert.ToString(reader["typename"]);
                    jobWorkDetails.detail.sizename = reader["sizename"] == DBNull.Value ? "" : Convert.ToString(reader["sizename"]);
                    jobWorkDetails.detail.sleevename = reader["sleevename"] == DBNull.Value ? "" : Convert.ToString(reader["sleevename"]);
                    jobWorkDetails.detail.glndesignid = reader["designid"] == DBNull.Value ? "" : Convert.ToString(reader["designid"]);
                    jobWorkDetails.jobworkcharge = reader["chargerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargerate"]);
                    jobWorkDetails.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                    jobWorkDetails.entryno = reader["entryno"] == DBNull.Value ? "" : Convert.ToString(reader["entryno"]);
                    jobWorkDetails.jobworkername = reader["supplier"] == DBNull.Value ? "" : Convert.ToString(reader["supplier"]);
                    jobWorkDetails.jobworkercity = reader["city"] == DBNull.Value ? "" : Convert.ToString(reader["city"]);
                    jobWorkDetails.jobworkchargetotal = reader["chargetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargetotal"]);
                }
            }
            return jobWorkDetails;
        }
        public bool DeleteAll(long jobworkbillgrnid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("jobworkbillgrnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = jobworkbillgrnid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
