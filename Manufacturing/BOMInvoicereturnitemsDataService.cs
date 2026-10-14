using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BOMInvoicereturnitemsDataService : SAASData
    {
        const string SQL_Search = @"select  id, isactive, createdby, modifiedby, version, createdon, modifiedon , qty, 
            bominvoiceitemsid, taxid,rate, tax, discount, bominvoicereturnid,bominvoicereturnitems.taxpercentage,
            bominvoicereturnitems.amount,discountpercentage,bomid,colourid,bomcategoryid,width from bominvoicereturnitems where id=@id";

        const string SQL_SearchAll = @"select  bominvoicereturnitems.id, bominvoicereturnitems.isactive, bominvoicereturnitems.createdby, bominvoicereturnitems.modifiedby, 
			bominvoicereturnitems.version,bominvoicereturnitems.createdon, bominvoicereturnitems.modifiedon , bominvoicereturnitems.qty, 
            bominvoicereturnitems.bominvoiceitemsid, bominvoicereturnitems.taxid,bominvoicereturnitems.rate, bominvoicereturnitems.tax, 
			bominvoicereturnitems.discount, bominvoicereturnitems.bominvoicereturnid,bominvoicereturnitems.taxpercentage,
            bominvoicereturnitems.amount,bominvoicereturnitems.discountpercentage,bominvoicereturnitems.bomid,
			bominvoicereturnitems.colourid,bominvoicereturnitems.bomcategoryid,bominvoicereturnitems.width,
			bominvoicereturnitems.imageid,bominvoicereturnitems.bominwardcode,bommaster.name printingname,
			Bominvoiceitems.sno invoiceserialno,BOMinvoice.invoiceno,BOMinvoice.invoiceno,BOMinvoice.invoicedate,
			tax.name taxname,bominvoicereturnitems.hsncode,referencelist.name colourname,tax.taxsplit,
            bommaster.hsncode bomhsncode
            from bominvoicereturnitems 
			inner join bommaster on bommaster.id = bominvoicereturnitems.bomid
			left outer join Bominvoiceitems on Bominvoiceitems.id = bominvoiceitemsid
			left outer join BOMinvoice on BOMinvoice.id = bominvoiceid
			left outer join tax on tax.id = bominvoicereturnitems.taxid
            left outer join referencelist on referencelist.id = bominvoicereturnitems.colourid
            where bominvoicereturnitems.bominvoicereturnid=@bominvoicereturnid and bominvoicereturnitems.isactive=true";

        const string SQL_SearchAllByType = @"
select bomid, bominvoicereturnitems.id, bominvoicereturnitems.isactive, 
bominvoicereturnitems.createdby, bominvoicereturnitems.modifiedby, bominvoicereturnitems.version, 
bominvoicereturnitems.createdon, bominvoicereturnitems.modifiedon, bominvoicereturnitems.colourid, 
bominvoicereturnitems.qty, 
bominvoicereturnitems.bominvoiceitemsid, bominvoicereturnitems.taxid,imageid, bominvoicereturnitems.rate, 
bominvoicereturnitems.tax, bominvoicereturnitems.discount, bominvoicereturnitems.bominvoicereturnid,
bominvoicereturnitems.bominwardcode,bominvoicereturnitems.taxpercentage,bominvoicereturnitems.amount,
bominvoicereturnitems.bomcategoryid,bominvoicereturn.returnno, bominvoicereturn.returndate, 
bominvoicereturnitems.discountpercentage 
from bominvoicereturnitems 
inner join bominvoicereturn on bominvoicereturn.id = bominvoicereturnid
where bominvoiceitemsid=@bominvoiceitemsid and 
bominvoicereturn.ReturnType = @returntype and
bominvoicereturnitems.isactive=true and bominvoicereturn.isactive=true";


        const string SQL_Insert = @"INSERT INTO public.bominvoicereturnitems(bomid, isactive, createdby, modifiedby, version, createdon, modifiedon, colourid, qty,bominvoiceitemsid, taxid,imageid, 
                    rate, tax, discount, bominvoicereturnid,bominwardcode,taxpercentage,amount,bomcategoryid,discountpercentage,width,hsncode) VALUES(@bomid, @isactive, @createdby, @modifiedby, @version, 
                        @createdon, @modifiedon, @colourid, @qty,@bominvoiceitemsid, @taxid,@imageid, @rate, @tax, @discount, @bominvoicereturnid,@bominwardcode,@taxpercentage,@amount,
                        @bomcategoryid,@discountpercentage,@width,@hsncode) RETURNING id";


        const string SQL_Update = @"UPDATE public.bominvoicereturnitems SET bomid=@bomid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, colourid=@colourid, qty=@qty, bominvoiceitemsid=@bominvoiceitemsid,
                                    taxid=@taxid,bominwardcode=@bominwardcode,imageid=@imageid, rate=@rate, tax=@tax, discount=@discount,bomcategoryid=@bomcategoryid, bominvoicereturnid=@bominvoicereturnid,taxpercentage=@taxpercentage,amount=@amount,discountpercentage=@discountpercentage
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.bominvoicereturnitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.bominvoicereturnitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where bominvoicereturnid=@bominvoicereturnid";
        public List<BOMInvoicereturnitems> Search(BOMInvoicereturnitemsCreteria _bominvoicereturnitems)
        {
            List<BOMInvoicereturnitems> lst = new List<BOMInvoicereturnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bominvoicereturnitems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bominvoicereturnitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_bominvoicereturnitems.Code))
                    //    qb.AddParameter("code", _bominvoicereturnitems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMInvoicereturnitems obominvoicereturnitems = new BOMInvoicereturnitems();
                        obominvoicereturnitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"].ToString());
                        obominvoicereturnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obominvoicereturnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obominvoicereturnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obominvoicereturnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obominvoicereturnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obominvoicereturnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obominvoicereturnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obominvoicereturnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());
                        obominvoicereturnitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"].ToString());
                        obominvoicereturnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obominvoicereturnitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : reader["bominwardcode"].ToString();
                        obominvoicereturnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        obominvoicereturnitems.bominvoiceitemsid = reader["bominvoiceitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceitemsid"].ToString());
                        obominvoicereturnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        obominvoicereturnitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        obominvoicereturnitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        obominvoicereturnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        obominvoicereturnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        obominvoicereturnitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        obominvoicereturnitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        obominvoicereturnitems.bominvoicereturnid = reader["bominvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoicereturnid"].ToString());
                        lst.Add(obominvoicereturnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bominvoicereturnitems);
            }
            return lst;
        }


        public List<BOMInvoicereturnitemdetail> SearchAllByReturnType(long _lrinvoicereturncolourid, int returntype)
        {
            List<BOMInvoicereturnitemdetail> lst = new List<BOMInvoicereturnitemdetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAllByType, conn);
                command.Parameters.Add("bominvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoicereturncolourid;
                command.Parameters.Add("returntype", NpgsqlTypes.NpgsqlDbType.Integer).Value = returntype;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMInvoicereturnitemdetail obominvoicereturnitems = new BOMInvoicereturnitemdetail();
                        obominvoicereturnitems.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                        obominvoicereturnitems.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"]);
                        obominvoicereturnitems.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        obominvoicereturnitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"].ToString());
                        obominvoicereturnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obominvoicereturnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obominvoicereturnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obominvoicereturnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obominvoicereturnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obominvoicereturnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obominvoicereturnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obominvoicereturnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());
                        obominvoicereturnitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : reader["bominwardcode"].ToString();
                        obominvoicereturnitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"].ToString());
                        obominvoicereturnitems.bominvoiceitemsid = reader["bominvoiceitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceitemsid"].ToString());
                        obominvoicereturnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        obominvoicereturnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        obominvoicereturnitems.bominvoicereturnid = reader["bominvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoicereturnid"].ToString());
                        obominvoicereturnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obominvoicereturnitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        obominvoicereturnitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        obominvoicereturnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        obominvoicereturnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        obominvoicereturnitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        obominvoicereturnitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        obominvoicereturnitems.taxsplit = new TaxSplit();
                        obominvoicereturnitems.taxsplit.totalamount = obominvoicereturnitems.taxableamount;

                        lst.Add(obominvoicereturnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lst);
            }
            return lst;
        }

        public List<BOMInvoicereturnitems> SearchAll(long _bominvoicereturnid,
            bool includetaxinformation,
            bool isinterstate)
        {
            List<BOMInvoicereturnitems> lst = new List<BOMInvoicereturnitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("bominvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bominvoicereturnid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMInvoicereturnitems obominvoicereturnitems = new BOMInvoicereturnitems();
                        obominvoicereturnitems.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"].ToString());
                        obominvoicereturnitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obominvoicereturnitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obominvoicereturnitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obominvoicereturnitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obominvoicereturnitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obominvoicereturnitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obominvoicereturnitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obominvoicereturnitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"].ToString());
                        obominvoicereturnitems.bominwardcode = reader["bominwardcode"] == DBNull.Value ? "" : reader["bominwardcode"].ToString();
                        obominvoicereturnitems.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"].ToString());
                        obominvoicereturnitems.bominvoiceitemsid = reader["bominvoiceitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceitemsid"].ToString());
                        obominvoicereturnitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        obominvoicereturnitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        obominvoicereturnitems.bominvoicereturnid = reader["bominvoicereturnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoicereturnid"].ToString());
                        obominvoicereturnitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obominvoicereturnitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        obominvoicereturnitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        obominvoicereturnitems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        obominvoicereturnitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"].ToString());
                        obominvoicereturnitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        obominvoicereturnitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"].ToString());
                        obominvoicereturnitems.taxsplit = new TaxSplit();
                        obominvoicereturnitems.taxsplit.totalamount = obominvoicereturnitems.taxableamount;
                        obominvoicereturnitems.isinterstatesale = isinterstate;
                        obominvoicereturnitems.invoiceserialno = reader["invoiceserialno"] == DBNull.Value ? "" : reader["invoiceserialno"].ToString();
                        obominvoicereturnitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obominvoicereturnitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        obominvoicereturnitems.bomname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obominvoicereturnitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        obominvoicereturnitems.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        obominvoicereturnitems.taxsplit.name = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                        obominvoicereturnitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        if(string.IsNullOrEmpty(obominvoicereturnitems.hsncode))
                            obominvoicereturnitems.hsncode = reader["bomhsncode"] == DBNull.Value ? "" : reader["bomhsncode"].ToString(); 
                        if (isinterstate)
                        {
                            obominvoicereturnitems.taxsplit.secondtax = "IGST";
                        }
                        else
                        {
                            obominvoicereturnitems.taxsplit.secondtax = "SGST";
                        }

                        if (reader["taxsplit"] != DBNull.Value)
                        {
                            var taxsplit = Newtonsoft.Json.JsonConvert.DeserializeObject<Tax.TaxSplit>(reader["taxsplit"].ToString());

                            if (taxsplit != null)
                            {
                                var value = ((obominvoicereturnitems.rate * obominvoicereturnitems.qty) - obominvoicereturnitems.discount);
                                if (obominvoicereturnitems.qty > 0 && value > 0)
                                    value = value / obominvoicereturnitems.qty;
                                var taxconfig = taxsplit.Find(value);
                                if (taxconfig.taxpercent != obominvoicereturnitems.taxpercentage)
                                {
                                    if (taxsplit.items.Exists(e => e.taxpercent == obominvoicereturnitems.taxpercentage))
                                        taxconfig = taxsplit.items.First(e => e.taxpercent == obominvoicereturnitems.taxpercentage);
                                    else
                                    {
                                        taxconfig = new Tax.TaxSplit.Item()
                                        {
                                            cgst = obominvoicereturnitems.taxpercentage / 2,
                                            sgst = obominvoicereturnitems.taxpercentage / 2
                                        };
                                    }
                                }
                                if (isinterstate)
                                {
                                    obominvoicereturnitems.taxsplit.sgsttaxpercentage = 0;
                                    obominvoicereturnitems.taxsplit.sgsttaxamount = 0;
                                    obominvoicereturnitems.taxsplit.cgsttaxpercentage = taxconfig.sgst + taxconfig.cgst;
                                    obominvoicereturnitems.taxsplit.cgsttaxamount = obominvoicereturnitems.tax;
                                }
                                else
                                {
                                    obominvoicereturnitems.taxsplit.sgsttaxpercentage = taxconfig.sgst;
                                    obominvoicereturnitems.taxsplit.cgsttaxpercentage = taxconfig.cgst;
                                    if (obominvoicereturnitems.tax > 0
                                        && obominvoicereturnitems.taxsplit.cgsttaxpercentage + obominvoicereturnitems.taxsplit.sgsttaxpercentage > 0)
                                    {
                                        obominvoicereturnitems.taxsplit.sgsttaxamount = Math.Round(obominvoicereturnitems.tax /
                                            (obominvoicereturnitems.taxsplit.cgsttaxpercentage + obominvoicereturnitems.taxsplit.sgsttaxpercentage)
                                            * obominvoicereturnitems.taxsplit.sgsttaxpercentage, 4);
                                    }
                                    obominvoicereturnitems.taxsplit.cgsttaxamount = obominvoicereturnitems.taxsplit.sgsttaxamount;
                                }
                            }
                            else
                            {
                                if (isinterstate)
                                {
                                    obominvoicereturnitems.taxsplit.sgsttaxpercentage = 0;
                                    obominvoicereturnitems.taxsplit.sgsttaxamount = 0;
                                    obominvoicereturnitems.taxsplit.cgsttaxpercentage = obominvoicereturnitems.taxpercentage;
                                    obominvoicereturnitems.taxsplit.cgsttaxamount = obominvoicereturnitems.tax;
                                }
                                else
                                {
                                    if (obominvoicereturnitems.tax > 0 && obominvoicereturnitems.taxpercentage > 0)
                                    {
                                        obominvoicereturnitems.taxsplit.sgsttaxpercentage = obominvoicereturnitems.taxpercentage / 2;
                                        obominvoicereturnitems.taxsplit.cgsttaxpercentage = obominvoicereturnitems.taxpercentage / 2;
                                        obominvoicereturnitems.taxsplit.sgsttaxamount = Math.Round(obominvoicereturnitems.tax / (obominvoicereturnitems.taxsplit.cgsttaxpercentage + obominvoicereturnitems.taxsplit.sgsttaxpercentage) * obominvoicereturnitems.taxsplit.sgsttaxpercentage, 4);
                                        obominvoicereturnitems.taxsplit.cgsttaxamount = obominvoicereturnitems.taxsplit.sgsttaxamount;
                                    }
                                }
                            }
                        }
                        else if (obominvoicereturnitems.tax != 0 &&
                            obominvoicereturnitems.taxpercentage != 0)
                        {
                            if (isinterstate)
                            {
                                obominvoicereturnitems.taxsplit.sgsttaxpercentage = 0;
                                obominvoicereturnitems.taxsplit.sgsttaxamount = 0;
                                obominvoicereturnitems.taxsplit.cgsttaxpercentage = obominvoicereturnitems.taxpercentage;
                                obominvoicereturnitems.taxsplit.cgsttaxamount = obominvoicereturnitems.tax;
                            }
                            else
                            {
                                obominvoicereturnitems.taxsplit.sgsttaxpercentage = obominvoicereturnitems.taxpercentage / 2;
                                obominvoicereturnitems.taxsplit.cgsttaxpercentage = obominvoicereturnitems.taxpercentage / 2;
                                obominvoicereturnitems.taxsplit.sgsttaxamount = Math.Round(obominvoicereturnitems.tax / (obominvoicereturnitems.taxsplit.cgsttaxpercentage + obominvoicereturnitems.taxsplit.sgsttaxpercentage) * obominvoicereturnitems.taxsplit.sgsttaxpercentage, 4);
                                obominvoicereturnitems.taxsplit.cgsttaxamount = obominvoicereturnitems.taxsplit.sgsttaxamount;
                            }
                        }
                        obominvoicereturnitems.sno = lst.Count + 1;
                        lst.Add(obominvoicereturnitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lst);
            }
            return lst;
        }

        public BOMInvoicereturnitems Insert(BOMInvoicereturn lrinvoicereturn, BOMInvoicereturnitems obominvoicereturnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, lrinvoicereturn, obominvoicereturnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturnitems);
            }
            return obominvoicereturnitems;
        }

        public BOMInvoicereturnitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn lrinvoicereturn, BOMInvoicereturnitems obominvoicereturnitems)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bomid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturnitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturnitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.colourid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.qty;
            command.Parameters.Add("bominwardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturnitems.bominwardcode == null ? "" : obominvoicereturnitems.bominwardcode;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturnitems.width== null ? "" : obominvoicereturnitems.width;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturnitems.hsncode == null ? "" : obominvoicereturnitems.hsncode;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturnitems.imageid == null ? "" : obominvoicereturnitems.imageid;
            command.Parameters.Add("bominvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bominvoiceitemsid;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bomcategoryid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.taxid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.rate;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.tax;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.taxpercentage;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.discount;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.amount;
            command.Parameters.Add("bominvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bominvoicereturnid;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.discountpercentage;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    obominvoicereturnitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (lrinvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.DebitNote)
            {

            }

            return obominvoicereturnitems;
        }

        public BOMInvoicereturnitems Update(BOMInvoicereturn lrinvoicereturn, BOMInvoicereturnitems obominvoicereturnitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, lrinvoicereturn, obominvoicereturnitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturnitems);
            }
            return obominvoicereturnitems;
        }

        public BOMInvoicereturnitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn lrinvoicereturn, BOMInvoicereturnitems obominvoicereturnitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bomid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturnitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturnitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = obominvoicereturnitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.colourid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.qty;
            command.Parameters.Add("bominwardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturnitems.bominwardcode == null ? "" : obominvoicereturnitems.bominwardcode;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturnitems.imageid == null ? "" : obominvoicereturnitems.imageid;
            command.Parameters.Add("bominvoiceitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bominvoiceitemsid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.taxid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.rate;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.tax;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.discount;
            command.Parameters.Add("bominvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bominvoicereturnid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.taxpercentage;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.amount;
            command.Parameters.Add("bomcategoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.bomcategoryid;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturnitems.discountpercentage;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    obominvoicereturnitems.version = Convert.ToInt32(reader[0]);
                }
            }

            if (lrinvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.DebitNote)
            {

            }
            return obominvoicereturnitems;
        }
        public bool UpdateForItemReturn(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn lrinvoicereturn, List<BOMInvoicereturnitems> list, long id)
        {
            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, lrinvoicereturn, item);
                }
                else
                {
                    item.bominvoicereturnid = id;
                    Insert(transaction, conn, lrinvoicereturn, item);
                }
            }
            return true;

        }
        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn lrinvoicereturn, List<BOMInvoicereturnitems> list, long id)
        {
            var existingItems = SearchAll(id, false, false);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                {
                    Delete(transaction, conn, lrinvoicereturn, item);
                    item.qty = 0;
                }
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, lrinvoicereturn, item);
                }
                else
                {
                    item.bominvoicereturnid = id;
                    Insert(transaction, conn, lrinvoicereturn, item);
                }
            }
            return true;
        }

        public bool Delete(BOMInvoicereturnitems obominvoicereturnitems, BOMInvoicereturn lrinvoicereturn)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, lrinvoicereturn, obominvoicereturnitems);


                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturnitems);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn lrinvoicereturn, BOMInvoicereturnitems obominvoicereturnitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturnitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturnitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturnitems.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public bool DeleteAll(BOMInvoicereturn lrinvoicereturn, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bominvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrinvoicereturn.id;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
