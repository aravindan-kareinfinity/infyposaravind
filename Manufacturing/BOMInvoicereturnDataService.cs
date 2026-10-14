using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BOMInvoicereturnDataService : SAASData
    {
        const string SQL_Search = @"select bominvoicereturn.id, bominvoicereturn.isactive, bominvoicereturn.createdby, bominvoicereturn.modifiedby, bominvoicereturn.version, bominvoicereturn.createdon, bominvoicereturn.modifiedon, 
                                    bominvoicereturn.returnno, bominvoicereturn.returndate , bominvoicereturn.discount, bominvoicereturn.tax, bominvoicereturn.amount, bominvoicereturn.supplierid, bominvoicereturn.bundles, bominvoicereturn.freightamt, bominvoicereturn.weight,
                                    bominvoicereturn.transportid,bominvoicereturn.lrno,bominvoicereturn.lrdate,bominvoicereturn.remarks,bominvoicereturn.suppliercompanyid,bominvoicereturn.isinterstatesale,bominvoicereturn.total,bominvoicereturn.qty,bominvoicereturn.companyid,
                                    bominvoicereturn.rounding,bominvoicereturn.shiptosupplierid,bominvoicereturn.freight,bominvoicereturn.locationid,bominvoicereturn.returntype,bominvoicereturn.packingcharge,bominvoicereturn.freighttaxamt,bominvoicereturn.freighttaxid,
                                    bominvoicereturn.agentid,suppliercompany.cityid, employee.name employeename,billimg,eway,einvoice,packedby,tolocationid
                                    from bominvoicereturn 
                                    left outer join suppliercompany on suppliercompany.id=suppliercompanyid 
                                    left outer join users on users.id = bominvoicereturn.createdby
                                    left outer join employee on employeeid = employee.id 
                                    where bominvoicereturn.id=@id";

        const string SQL_Insert = @"INSERT INTO public.bominvoicereturn(isactive, createdby, modifiedby, version, createdon, modifiedon, returnno, returndate, discount, tax, returntype,
                                    amount,supplierid,bundles,freightamt,weight,remarks,suppliercompanyid,isinterstatesale,transportid,lrno,lrdate,total,qty,companyid,rounding,
                                    shiptosupplierid,freight,locationid,packingcharge,freighttaxamt,freighttaxid,agentid) 
                                                               VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @returnno, @returndate, @discount, @tax, @returntype,
                                    @amount,@supplierid,@bundles,@freightamt,@weight,@remarks,@suppliercompanyid,@isinterstatesale,@transportid,@lrno,@lrdate,@total,@qty,@companyid,@rounding,
                                    @shiptosupplierid,@freight,@locationid,@packingcharge,@freighttaxamt,@freighttaxid,@agentid) RETURNING id";

        const string SQL_Update = @"UPDATE public.bominvoicereturn SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, returnno=@returnno, returndate=@returndate, discount=@discount, tax=@tax, 
                                    amount=@amount,supplierid=@supplierid,transportid=@transportid,bundles=@bundles,freightamt=@freightamt,weight=@weight,remarks=@remarks,suppliercompanyid=@suppliercompanyid,returntype=@returntype,
                                    isinterstatesale=@isinterstatesale,lrno=@lrno,lrdate=@lrdate,total=@total,qty=@qty,companyid=@companyid,rounding=@rounding,shiptosupplierid=@shiptosupplierid,freight=@freight,locationid=@locationid,
                                    packingcharge=@packingcharge,freighttaxamt=@freighttaxamt,freighttaxid=@freighttaxid,agentid=@agentid,billimg=@billimg,eway=@eway,einvoice=@einvoice,packedby=@packedby,tolocationid=@tolocationid
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.bominvoicereturn set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<BOMInvoicereturn> Search(BOMInvoicereturnCreteria _bominvoicereturn)
        {
            List<BOMInvoicereturn> lst = new List<BOMInvoicereturn>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bominvoicereturn.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bominvoicereturn.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("bominvoicereturn.isactive", true, "=");
                    qb.AddParameter("coalesce(returntype,0)", "returntype", _bominvoicereturn.returntype, " = ");

                    if (!string.IsNullOrEmpty(_bominvoicereturn.returnno))
                    {
                        qb.AddParameter("returnno", _bominvoicereturn.returnno, " = ");
                    }
                    if (_bominvoicereturn.supplierid > 0)
                    {
                        qb.AddParameter("bominvoicereturn.supplierid", _bominvoicereturn.supplierid, " = ");
                    }
                    if (_bominvoicereturn.companyid > 0)
                    {
                        qb.AddParameter("bominvoicereturn.companyid", _bominvoicereturn.companyid, " = ");
                    }
                    if (_bominvoicereturn.locationid > 0)
                    {
                        qb.AddParameter("bominvoicereturn.locationid", _bominvoicereturn.locationid, " = ");
                    }
                    if (_bominvoicereturn.returndate != DateTime.MinValue)
                    {
                        qb.AddParameter("returndate", "fromdate", _bominvoicereturn.returndate.Date, ">=");
                        qb.AddParameter("returndate", "todate", _bominvoicereturn.returndate.Date.AddDays(1), "<");
                    }
                    if (_bominvoicereturn.fromreturndate != DateTime.MinValue && _bominvoicereturn.toreturndate != DateTime.MinValue)
                    {
                        qb.AddParameter("returndate", "datefrom", _bominvoicereturn.fromreturndate.Date, ">=");
                        qb.AddParameter("returndate", "dateto", _bominvoicereturn.toreturndate.Date.AddDays(1), "<");
                    }

                    if (!string.IsNullOrEmpty(_bominvoicereturn.barcode))
                    {
                        qb.AddInParamer("bominvoicereturn.id", "(select bominvoicereturnid from bominvoicereturnitems where barcode='" + WebAPI.Data.Utility.SafeSQL(_bominvoicereturn.barcode) + "')");
                    }
                    if (!string.IsNullOrEmpty(_bominvoicereturn.lrentryno))
                    {
                        qb.AddInParamer("bominvoicereturn.id", @"(select bominvoicereturnid from bominvoicereturnitems 
inner join lrinvoiceitems on lrinvoiceitems.id = bominvoicereturnitems.bominvoiceitemsid
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
inner join lrentry on lrentry.id = lrinvoice.lrid
where bominvoicereturnitems.isactive = true
and lrentry.lrentryno = '" + WebAPI.Data.Utility.SafeSQL(_bominvoicereturn.lrentryno) + "')");

                    }
                    if (!string.IsNullOrEmpty(_bominvoicereturn.invoiceno))
                    {
                        qb.AddInParamer("bominvoicereturn.id", @"(select bominvoicereturnid from bominvoicereturnitems 
inner join lrinvoiceitems on lrinvoiceitems.id = bominvoicereturnitems.bominvoiceitemsid
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
where bominvoicereturnitems.isactive = true
and lrinvoice.invoiceno = '" + WebAPI.Data.Utility.SafeSQL(_bominvoicereturn.invoiceno) + "')");

                    }
                    //if (!string.IsNullOrEmpty(_bominvoicereturn.Code))
                    //    qb.AddParameter("code", _bominvoicereturn.Code, "=");

                    qb.SortField = @"bominvoicereturn.id desc";
                    //qb.Limit = 100;

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMInvoicereturn obominvoicereturn = new BOMInvoicereturn();
                        obominvoicereturn.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obominvoicereturn.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        obominvoicereturn.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obominvoicereturn.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obominvoicereturn.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        obominvoicereturn.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obominvoicereturn.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obominvoicereturn.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obominvoicereturn.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obominvoicereturn.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                        obominvoicereturn.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"].ToString());
                        //obominvoicereturn.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        //obominvoicereturn.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        obominvoicereturn.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        obominvoicereturn.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        obominvoicereturn.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        obominvoicereturn.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"].ToString());
                        obominvoicereturn.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"].ToString());
                        obominvoicereturn.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obominvoicereturn.shiptosupplierid = reader["shiptosupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shiptosupplierid"].ToString());
                        obominvoicereturn.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        obominvoicereturn.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"].ToString());
                        obominvoicereturn.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        obominvoicereturn.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"].ToString());
                        obominvoicereturn.bundles = reader["bundles"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bundles"].ToString());
                        obominvoicereturn.freightamt = reader["freightamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightamt"].ToString());
                        obominvoicereturn.packingcharge = reader["packingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packingcharge"].ToString());
                        obominvoicereturn.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"].ToString());
                        obominvoicereturn.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        obominvoicereturn.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obominvoicereturn.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obominvoicereturn.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        obominvoicereturn.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        obominvoicereturn.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        obominvoicereturn.returntype = reader["returntype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["returntype"]);
                        obominvoicereturn.freighttaxamt = reader["freighttaxamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freighttaxamt"].ToString());
                        obominvoicereturn.freighttaxid = reader["freighttaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["freighttaxid"]);
                        obominvoicereturn.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["agentid"].ToString());
                        obominvoicereturn.createdusername = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        obominvoicereturn.billimg = reader["billimg"] == DBNull.Value ? "" : reader["billimg"].ToString();
                        obominvoicereturn.eway = reader["eway"] == DBNull.Value ? "" : reader["eway"].ToString();
                        obominvoicereturn.einvoice = reader["einvoice"] == DBNull.Value ? "" : reader["einvoice"].ToString();
                        obominvoicereturn.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tolocationid"].ToString());

                        lst.Add(obominvoicereturn);
                    }
                }
                if (_bominvoicereturn.id > 0 || lst.Count == 1)
                {
                    BOMInvoicereturnitemsDataService subbominvoicereturnitems = new BOMInvoicereturnitemsDataService() { Context = this.Context };
                    subbominvoicereturnitems.Context = this.Context;
                    lst[0].BOMinvoicereturnitems = subbominvoicereturnitems.SearchAll(_bominvoicereturn.id, true, lst[0].isinterstatesale);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bominvoicereturn);
            }

            if (_bominvoicereturn.isaudit)
                return UpdateUser(lst);
            return lst;
        }
        public List<BOMInvoicereturn> UpdateUser(List<BOMInvoicereturn> lstreturn)
        {
            List<Users> lst = new List<Users>();
            string query = @"select employee.name,users.id from users
                left outer join employee on employee.id = employeeid ";
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                command = GetCommand(query, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Users oitem = new Users();
                        oitem.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oitem.username = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }

            foreach (var item in lstreturn)
            {
                if (lst.Exists(e => e.id == item.createdby))
                    item.createdusername = lst.Find(e => e.id == item.createdby).username;
                if (lst.Exists(e => e.id == item.modifiedby))
                    item.modifiedusername = lst.Find(e => e.id == item.modifiedby).username;

            }
            return lstreturn;

        }

        public BOMInvoicereturn InsertReturn(NpgsqlTransaction transaction, NpgsqlConnection conn,
            BOMInvoicereturn obominvoicereturn)
        {
            string sequencetype = "bominvoicereturn.returnno";
            if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.SupplierCreditNote)
                sequencetype = "bominvoicereturn.creditnote";
            else if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.SupplierAdvanceNote)
                sequencetype = "bominvoicereturn.advancenote";

            var sequence = SequenceManager.Instance(Context.OrganizationID).Get(sequencetype,
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisereturnno ? obominvoicereturn.locationid : 0,
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany ? obominvoicereturn.companyid : 0);

            obominvoicereturn.returnno = sequence.GetNextNumber(Context, conn, transaction);
            obominvoicereturn.isactive = true;
            obominvoicereturn.createdby = Context.UserID;
            obominvoicereturn.modifiedby = Context.UserID;
            obominvoicereturn.createdon = DateTimeNow;
            obominvoicereturn.modifiedon = DateTimeNow;
            obominvoicereturn.version = 1;
            Insert(transaction, conn, obominvoicereturn);
            BOMInvoicereturnitemsDataService subbominvoicereturnitems = new BOMInvoicereturnitemsDataService() { Context = this.Context };
            subbominvoicereturnitems.Context = this.Context;
            if (obominvoicereturn.BOMinvoicereturnitems != null)
            {
                foreach (var item in obominvoicereturn.BOMinvoicereturnitems)
                {
                    item.bominvoicereturnid = obominvoicereturn.id;
                    item.isactive = true;
                    subbominvoicereturnitems.Insert(transaction, conn, obominvoicereturn, item);
                }
            }
            if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.DebitNote)
            {
                BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                bomstocktransactionDataService.Insert(Bomstocktransaction.TransactionType.PurchaseReturn,
                    conn, transaction, obominvoicereturn, obominvoicereturn.BOMinvoicereturnitems);
            }
            return obominvoicereturn;
        }
        public BOMInvoicereturn Insert(BOMInvoicereturn obominvoicereturn)
        {
            if (obominvoicereturn.companyid == 0)
            {
                throw new Exception("Company ID missing, Please select company");
            }

            obominvoicereturn.BOMinvoicereturnitems.RemoveAll(e => e.qty == 0);

            NpgsqlTransaction transaction = null;
            try
            {

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    obominvoicereturn = InsertReturn(transaction, conn, obominvoicereturn);
                    InboxManager.Instance(Context.OrganizationID).Update(Context, true, obominvoicereturn, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, obominvoicereturn, Processors.Schedule_Actions.Actions.New);
                }
            }
            catch (Exception exp)
            {
                obominvoicereturn.id = 0;
                obominvoicereturn.returnno = "";
                obominvoicereturn.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturn);
            }

            return obominvoicereturn;
        }


        private void ValidateStock(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturnitems obominvoicereturnitems)
        {
            //if (obominvoicereturnitems.stockid <= 0)
            //{
            //    StockDataService stockDataService = new StockDataService() { Context = this.Context };
            //    Stock stock = new Stock();
            //    stock.barcode = "I" + obominvoicereturnitems.supplierid.ToString("F0") + "I" + obominvoicereturnitems.bominvoicereturnid.ToString("F0")
            //                        + "I" + obominvoicereturnitems.bominvoiceitemsid.ToString("F0");
            //    stock.qty = obominvoicereturnitems.qty;
            //    stock.buyingprice = obominvoicereturnitems.rate;
            //    stock.buyingqty = obominvoicereturnitems.qty;
            //    stock.companyid = obominvoicereturnitems.companyid;
            //    stock.designid = obominvoicereturnitems.designid;
            //    stock.hsncode = obominvoicereturnitems.hsncode;
            //    stock.invoiceitemid = obominvoicereturnitems.bominvoiceitemsid;
            //    stock.itemid = obominvoicereturnitems.itemid;
            //    stock.price = obominvoicereturnitems.rate + obominvoicereturnitems.tax;
            //    stock.productid = obominvoicereturnitems.productid;
            //    stock.purchasediscount = obominvoicereturnitems.discount;
            //    stock.purchaseqty = obominvoicereturnitems.qty;
            //    stock.purchasetax = obominvoicereturnitems.tax;
            //    stock.returnqty = obominvoicereturnitems.qty;
            //    stock.supplierid = obominvoicereturnitems.supplierid;
            //    stock.version = 1;
            //    stock.createdby = Context.UserID;
            //    stock.modifiedby = Context.UserID;
            //    stock.createdon = DateTimeNow;
            //    stock.modifiedon = DateTimeNow;
            //    stock.isactive = true;
            //    stockDataService.Insert(transaction, conn, stock);
            //    obominvoicereturnitems.id = stock.id;
            //}
        }

        public List<BOMInvoicereturn> InsertAll(BOMInvoicereturn coreReturn)
        {
            if (coreReturn.BOMinvoicereturnitems.Exists(e => e.companyid == 0))
            {
                throw new Exception("Company ID missing, Please select company");
            }

            coreReturn.BOMinvoicereturnitems.RemoveAll(e => e.qty == 0);

            List<BOMInvoicereturn> obominvoicereturns = new List<BOMInvoicereturn>();
            NpgsqlTransaction transaction = null;
            try
            {
                foreach (var item in coreReturn.BOMinvoicereturnitems.GroupBy(e => new { e.companyid, e.supplierid, e.suppliercompanyid }))
                {
                    BOMInvoicereturn obominvoicereturn = new BOMInvoicereturn();
                    obominvoicereturn.companyid = item.First().companyid;
                    obominvoicereturn.locationid = coreReturn.locationid;
                    obominvoicereturn.suppliercompanyid = item.First().suppliercompanyid;
                    obominvoicereturn.supplierid = item.First().supplierid;
                    obominvoicereturn.isinterstatesale = item.First().isinterstatesale;
                    obominvoicereturn.freight = coreReturn.freight;
                    obominvoicereturn.lrdate = coreReturn.lrdate;
                    obominvoicereturn.lrid = coreReturn.lrid;
                    obominvoicereturn.lrinvoiceid = coreReturn.lrinvoiceid;
                    obominvoicereturn.lrno = coreReturn.lrno;
                    obominvoicereturn.remarks = coreReturn.remarks;
                    obominvoicereturn.returndate = coreReturn.returndate;
                    obominvoicereturn.transportid = coreReturn.transportid;
                    obominvoicereturn.isactive = true;
                    obominvoicereturn.createdby = Context.UserID;
                    obominvoicereturn.modifiedby = Context.UserID;
                    obominvoicereturn.createdon = DateTimeNow;
                    obominvoicereturn.modifiedon = DateTimeNow;
                    obominvoicereturn.version = 1;
                    obominvoicereturn.BOMinvoicereturnitems = new List<BOMInvoicereturnitems>();
                    obominvoicereturn.BOMinvoicereturnitems.AddRange(item);
                    obominvoicereturn.amount = obominvoicereturn.BOMinvoicereturnitems.Sum(e => e.amount);
                    obominvoicereturn.discount = obominvoicereturn.BOMinvoicereturnitems.Sum(e => e.discount);
                    obominvoicereturn.qty = obominvoicereturn.BOMinvoicereturnitems.Sum(e => e.qty);
                    obominvoicereturn.tax = obominvoicereturn.BOMinvoicereturnitems.Sum(e => e.tax);

                    obominvoicereturn.total = Math.Round(obominvoicereturn.BOMinvoicereturnitems.Sum(e => e.amount - e.discount + e.tax), 0);
                    obominvoicereturn.rounding = obominvoicereturn.total - Math.Round(obominvoicereturn.BOMinvoicereturnitems.Sum(e => e.amount - e.discount + e.tax), 2);
                    obominvoicereturns.Add(obominvoicereturn);
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var obominvoicereturn in obominvoicereturns)
                    {
                        string sequencetype = "bominvoicereturn.returnno";
                        if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.SupplierCreditNote)
                            sequencetype = "bominvoicereturn.creditnote";
                        else if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.SupplierAdvanceNote)
                            sequencetype = "bominvoicereturn.advancenote";

                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get(sequencetype, 0,
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany ? obominvoicereturn.companyid : 0);
                        NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                        obominvoicereturn.returnno = sequence.GetNextNumber(Context, conn, transaction);
                        Insert(transaction, conn, obominvoicereturn);
                        BOMInvoicereturnitemsDataService subbominvoicereturnitems = new BOMInvoicereturnitemsDataService() { Context = this.Context };
                        subbominvoicereturnitems.Context = this.Context;
                        foreach (var item in obominvoicereturn.BOMinvoicereturnitems)
                        {
                            item.bominvoicereturnid = obominvoicereturn.id;
                            item.isactive = true;
                            subbominvoicereturnitems.Insert(transaction, conn, obominvoicereturn, item);
                        }
                        InboxManager.Instance(Context.OrganizationID).Update(Context, true, obominvoicereturn, transaction, conn);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                obominvoicereturns.Clear();
                obominvoicereturns.Add(new BOMInvoicereturn()
                {
                    errormessage = exp.Message
                });
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, coreReturn);
            }
            return obominvoicereturns;
        }

        internal BOMInvoicereturn ClosedSupplier(BOMInvoicereturn item)
        {
            long asupplierid = 0;
            long asuppliercompanyid = 0;
            bool closed = false;
            string newsupplier = "";
            using (var reader = ExecuteReader(@"select supplier.asupplierid,supplier.closed,asupplier.name,suppliercompany.id from supplier
                                                left outer join supplier asupplier on asupplier.id = supplier.asupplierid 
                                                left outer join suppliercompany on suppliercompany.supplierid = supplier.asupplierid
                                                where supplier.id=" + item.supplierid, null))
            {
                if (reader.Read())
                {
                    asupplierid = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                    closed = reader.IsDBNull(1) ? false : reader.GetBoolean(1);
                    newsupplier = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    asuppliercompanyid = reader.IsDBNull(3) ? 0 : reader.GetInt64(3);
                }
            }
            if (!closed)
            {
                item.errormessage = "Current Supplier not closed his business";
                return item;
            }
            if (asupplierid <= 0)
            {
                item.errormessage = "New Supplier not associated";
                return item;
            }

            string query = "update bominvoicereturn set supplierid=@supplierid,suppliercompanyid=@suppliercompanyid,dsupplierid=@dsupplierid,version=200 where id=@id and version=@version RETURNING version";
            var param = new SortedList<string, object>();
            param.Add("supplierid", asupplierid);
            param.Add("suppliercompanyid", asuppliercompanyid);
            param.Add("id", item.id);
            param.Add("dsupplierid", item.supplierid);
            param.Add("version", item.version);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    item.errormessage = "New Supplier " + newsupplier + " Updated on Debit note : " + item.returnno;
                    item.version = reader.GetInt32(0);
                }
                else
                {
                    item.errormessage = "Please reload and try again";
                }
            }
            return item;
        }

        public BOMInvoicereturn Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn obominvoicereturn)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturn.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("returnno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.returnno == null ? "" : obominvoicereturn.returnno;
            command.Parameters.Add("returndate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = obominvoicereturn.returndate;
            //command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.lrinvoiceid;
            //command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.lrid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.discount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.tax;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.total;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.rounding;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.amount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.qty;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.supplierid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.suppliercompanyid;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.transportid;
            command.Parameters.Add("bundles", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.bundles;
            command.Parameters.Add("freightamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.freightamt;
            command.Parameters.Add("packingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.packingcharge;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.weight;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.remarks == null ? "" : obominvoicereturn.remarks;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturn.isinterstatesale;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.lrno == null ? "" : obominvoicereturn.lrno;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = obominvoicereturn.lrdate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.locationid;
            command.Parameters.Add("shiptosupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.shiptosupplierid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.freight;
            command.Parameters.Add("returntype", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.returntype;
            command.Parameters.Add("freighttaxamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.freighttaxamt;
            command.Parameters.Add("freighttaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.freighttaxid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.agentid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    obominvoicereturn.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return obominvoicereturn;
        }



        internal BOMInvoicereturn UpdateLRInfo(BOMInvoicereturn item)
        {
            var existing = Search(new BOMInvoicereturnCreteria()
            {
                id = item.id
            });
            if (existing != null && existing.Count == 1)
            {
                existing[0].remarks = item.remarks;
                existing[0].transportid = item.transportid;
                existing[0].lrdate = item.lrdate;
                existing[0].lrno = item.lrno;
                return Update(item, false);
            }
            return item;
        }

        internal BOMInvoicereturn AddDirectDebitNote(BOMInvoicereturn.ReturnTypes rt, BOMInvoicereturnitemdetail lrinvoiceItemDetail, Pricedrop replacement)
        {
            return AddDirectDebitNote(rt, lrinvoiceItemDetail, replacement.qty, replacement.pricedrop, rt.ToString());
        }

        internal BOMInvoicereturn AddDirectDebitNote(BOMInvoicereturn.ReturnTypes rt, BOMInvoicereturnitemdetail lrinvoiceItemDetail, Deathonarrival replacement)
        {
            return AddDirectDebitNote(rt, lrinvoiceItemDetail, replacement.qty, replacement.qty * (replacement.rate + replacement.tax), rt.ToString());
        }
        internal BOMInvoicereturn AddDirectDebitNote(BOMInvoicereturnitemdetail item, decimal qty, string remark)
        {
            return AddDirectDebitNote(BOMInvoicereturn.ReturnTypes.DebitNote, item, qty, 0, remark);
        }
        internal BOMInvoicereturn AddDirectDebitNote(BOMInvoicereturn.ReturnTypes returnTypes, BOMInvoicereturnitemdetail item, decimal qty, decimal amount, string remark)
        {
            BOMinvoiceDataService lrinvoiceDataService = new BOMinvoiceDataService() { Context = this.Context };
            lrinvoiceDataService.Context = this.Context;
            var invoice = lrinvoiceDataService.Search(new BOMinvoiceCreteria()
            {
                id = item.bominvoicereturnid
            });

            LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
            lrentryDataService.Context = this.Context;
            var lrentry = lrentryDataService.Search(new LrentryCreteria()
            {
                id = invoice[0].lrid
            });

            string query = @"select returndate,bominvoicereturn.id from bominvoicereturnitems
                        inner join bominvoicereturn on bominvoicereturn.id = bominvoicereturnid
                        inner join lrinvoiceitems on lrinvoiceitems.id = bominvoicereturnitems.bominvoiceitemsid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        where bominvoicereturn.isactive=true and bominvoicereturnitems.isactive=true  
                        and lrinvoiceitems.lrinvoiceid=@lrinvoiceid and coalesce(returntype,0)=@returntype
                        order by returndate desc
                        limit 1";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("lrinvoiceid", item.bominvoicereturnid);
            param.Add("returntype", (int)returnTypes);
            long bominvoicereturnid = 0;
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read() && DateTimeNow.Subtract(reader.GetDateTime(0)).TotalDays < 7)
                {
                    bominvoicereturnid = reader.GetInt64(1);
                }
            }
            BOMInvoicereturnitems bominvoicereturnitems = new BOMInvoicereturnitems();
            bominvoicereturnitems.createdby = Context.UserID;
            bominvoicereturnitems.createdon = DateTimeNow;
            bominvoicereturnitems.width = item.width;
            bominvoicereturnitems.hsncode = item.hsncode;
            bominvoicereturnitems.invoicedate = invoice.First().invoicedate;
            bominvoicereturnitems.invoiceno = invoice.First().invoiceno;
            bominvoicereturnitems.isactive = true;
            bominvoicereturnitems.bomcategoryid = item.bomcategoryid;
            bominvoicereturnitems.colourid = item.colourid;
            bominvoicereturnitems.bominvoicereturnid = bominvoicereturnid;
            bominvoicereturnitems.bomid = item.bomid;
            bominvoicereturnitems.taxid = item.taxid;
            bominvoicereturnitems.taxpercentage = item.taxpercentage;
            bominvoicereturnitems.version = 1;
            bominvoicereturnitems.bominvoiceitemsid = item.id;
            bominvoicereturnitems.qty = qty;
            if (amount == 0)
            {
                bominvoicereturnitems.rate = item.rate;
                bominvoicereturnitems.tax = (item.tax / item.qty) * qty;
                bominvoicereturnitems.discount = item.discount * qty;
                bominvoicereturnitems.discountpercentage = item.discountpercentage;
                bominvoicereturnitems.amount = (bominvoicereturnitems.qty * bominvoicereturnitems.rate);
            }
            else
            {
                if (bominvoicereturnitems.taxpercentage > 0)
                {
                    bominvoicereturnitems.rate = Math.Round((amount / bominvoicereturnitems.taxpercentage + 100) * 100 / qty, 2);
                    bominvoicereturnitems.tax = Math.Round((bominvoicereturnitems.rate * qty * bominvoicereturnitems.taxpercentage) / 100, 2);
                }
                else
                {
                    bominvoicereturnitems.rate = Math.Round(amount / qty, 2);
                }
                bominvoicereturnitems.amount = amount;
            }
            var BOMInvoicereturnDataService = new BOMInvoicereturnDataService() { Context = this.Context };
            if (bominvoicereturnid > 0)
            {
                BOMInvoicereturn bominvoicereturn = BOMInvoicereturnDataService.Search(new BOMInvoicereturnCreteria()
                {
                    id = bominvoicereturnid
                }).First();

                if (bominvoicereturn.BOMinvoicereturnitems.Exists(e => e.bominvoiceitemsid == item.id))
                {
                    bominvoicereturnitems = bominvoicereturn.BOMinvoicereturnitems.First(e => e.bominvoiceitemsid == item.id);

                    bominvoicereturn.tax = bominvoicereturnitems.tax - bominvoicereturn.tax;
                    bominvoicereturn.discount = bominvoicereturnitems.discount - bominvoicereturn.discount;
                    bominvoicereturn.amount = bominvoicereturnitems.amount - bominvoicereturn.amount;
                    bominvoicereturn.qty = bominvoicereturnitems.qty - bominvoicereturn.qty;

                    bominvoicereturnitems.taxid = item.taxid;
                    bominvoicereturnitems.taxpercentage = item.taxid;
                    bominvoicereturnitems.bominvoiceitemsid = item.id;
                    bominvoicereturnitems.qty = qty;
                    bominvoicereturnitems.rate = item.rate;
                    bominvoicereturnitems.tax = (item.tax / item.qty) * qty;
                    bominvoicereturnitems.discount = item.discount * qty;
                    bominvoicereturnitems.discountpercentage = item.discountpercentage;
                    bominvoicereturnitems.amount = (bominvoicereturnitems.qty * bominvoicereturnitems.rate);
                    bominvoicereturnitems.modifiedby = Context.UserID;
                    bominvoicereturnitems.modifiedon = DateTimeNow;
                }
                else
                {
                    bominvoicereturnitems.modifiedby = Context.UserID;
                    bominvoicereturnitems.modifiedon = DateTimeNow;
                    bominvoicereturn.BOMinvoicereturnitems.Add(bominvoicereturnitems);
                    //ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                    //var sellingmodes = productsDataService.GetSellingModes(new long[] { item.productid }.ToList());
                    //bominvoicereturn.piece += sellingmodes.First().Value == 3 ? 1 : bominvoicereturnitems.qty;
                }
                bominvoicereturn.remarks = string.IsNullOrEmpty(bominvoicereturn.remarks) ? remark : bominvoicereturn.remarks;
                bominvoicereturn.tax = bominvoicereturnitems.tax + bominvoicereturn.tax;
                bominvoicereturn.discount = bominvoicereturnitems.discount + bominvoicereturn.discount;
                bominvoicereturn.amount = bominvoicereturnitems.amount + bominvoicereturn.amount;
                bominvoicereturn.qty = bominvoicereturnitems.qty + bominvoicereturn.qty;
                bominvoicereturn.total = Math.Round((bominvoicereturn.amount + bominvoicereturn.tax - bominvoicereturn.discount), 0);
                bominvoicereturn.rounding = bominvoicereturn.total - (bominvoicereturn.amount + bominvoicereturn.tax - bominvoicereturn.discount);

                if (bominvoicereturn.BOMinvoicereturnitems.Exists(e => e.bominvoiceitemsid == item.id))
                    bominvoicereturn.BOMinvoicereturnitems.RemoveAll(e => e.bominvoiceitemsid != item.id);
                else
                    bominvoicereturn.BOMinvoicereturnitems.RemoveAll(e => e.id > 0);
                BOMInvoicereturnDataService.UpdateForItemReturn(bominvoicereturn);
                return bominvoicereturn;
            }
            else
            {
                BOMInvoicereturn bominvoicereturn = new BOMInvoicereturn();
                bominvoicereturn.ReturnType = returnTypes;
                bominvoicereturn.companyid = invoice[0].companyid;
                bominvoicereturn.isinterstatesale = invoice[0].isinterstatesale;
                bominvoicereturn.createdby = Context.UserID;
                bominvoicereturn.createdon = DateTimeNow;
                bominvoicereturn.isactive = true;
                bominvoicereturn.BOMinvoicereturnitems = new List<BOMInvoicereturnitems>();
                bominvoicereturn.returndate = DateTimeNow.Date;
                bominvoicereturn.suppliercompanyid = invoice[0].suppliercompanyid;
                bominvoicereturn.supplierid = invoice[0].supplierid;
                bominvoicereturn.version = 1;
                bominvoicereturn.remarks = remark;
                bominvoicereturn.locationid = lrentry[0].entrylocationid;
                bominvoicereturn.BOMinvoicereturnitems.Add(bominvoicereturnitems);
                bominvoicereturn.tax = bominvoicereturnitems.tax + bominvoicereturn.tax;
                bominvoicereturn.discount = bominvoicereturnitems.discount + bominvoicereturn.discount;
                bominvoicereturn.amount = bominvoicereturnitems.amount + bominvoicereturn.amount;
                bominvoicereturn.qty = bominvoicereturnitems.qty + bominvoicereturn.qty;
                bominvoicereturn.total = Math.Round((bominvoicereturn.amount + bominvoicereturn.tax - bominvoicereturn.discount), 0);
                bominvoicereturn.rounding = bominvoicereturn.total - (bominvoicereturn.amount + bominvoicereturn.tax - bominvoicereturn.discount);
                BOMInvoicereturnDataService.Insert(bominvoicereturn);
                return bominvoicereturn;
            }


        }
        public BOMInvoicereturn UpdateForItemReturn(BOMInvoicereturn obominvoicereturn)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, obominvoicereturn);
                    BOMInvoicereturnitemsDataService subbominvoicereturnitems = new BOMInvoicereturnitemsDataService() { Context = this.Context };
                    subbominvoicereturnitems.Context = this.Context;
                    subbominvoicereturnitems.UpdateForItemReturn(transaction, conn, obominvoicereturn, obominvoicereturn.BOMinvoicereturnitems, obominvoicereturn.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturn);


            }

            return obominvoicereturn;
        }

        public BOMInvoicereturn Update(BOMInvoicereturn obominvoicereturn)
        {
            if (obominvoicereturn.companyid == 0)
            {
                throw new Exception("Company ID missing, Please select company");
            }

            return Update(obominvoicereturn, true);
        }

        internal BOMInvoicereturn AddAdvance(Voucher oVoucher, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            BOMInvoicereturn obominvoicereturn = new BOMInvoicereturn();
            var voucheritem = oVoucher.voucheritems.Find(e => e.attributes != null && e.attributes.supplierid > 0);
            obominvoicereturn.supplierid = voucheritem.attributes.supplierid;
            using (var reader = ExecuteReader("select id from suppliercompany where isactive = true and supplierid=" + obominvoicereturn.supplierid, new SortedList<string, object>()))
            {
                if (reader.Read())
                    obominvoicereturn.suppliercompanyid = reader.GetInt64(0);
            }
            obominvoicereturn.companyid = oVoucher.companyid;
            obominvoicereturn.amount = Math.Abs(voucheritem.amount);
            obominvoicereturn.isactive = true;
            obominvoicereturn.version = 1;
            obominvoicereturn.modifiedby = Context.UserID;
            obominvoicereturn.createdby = Context.UserID;
            obominvoicereturn.modifiedon = DateTimeNow;
            obominvoicereturn.createdon = DateTimeNow;
            obominvoicereturn.locationid = oVoucher.locationid;
            obominvoicereturn.returndate = oVoucher.voucherdate;
            obominvoicereturn.ReturnType = BOMInvoicereturn.ReturnTypes.SupplierAdvanceNote;
            obominvoicereturn.total = Math.Abs(voucheritem.amount);
            return InsertReturn(transaction, conn, obominvoicereturn);
        }

        public BOMInvoicereturn Update(BOMInvoicereturn obominvoicereturn, bool updateitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, obominvoicereturn);
                    if (updateitems)
                    {
                        BOMInvoicereturnitemsDataService subbominvoicereturnitems = new BOMInvoicereturnitemsDataService() { Context = this.Context };
                        subbominvoicereturnitems.Context = this.Context;
                        subbominvoicereturnitems.Update(transaction, conn, obominvoicereturn, obominvoicereturn.BOMinvoicereturnitems, obominvoicereturn.id);
                    }
                    if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.DebitNote)
                    {
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Update(Bomstocktransaction.TransactionType.PurchaseReturn,
                            conn, transaction, obominvoicereturn, obominvoicereturn.BOMinvoicereturnitems);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturn);


            }

            return obominvoicereturn;
        }

        public BOMInvoicereturnPrint Print(BOMInvoicereturn item)
        {
            var returninvoice = Search(new BOMInvoicereturnCreteria()
            {
                id = item.id,
                isaudit = true
            });

            BOMInvoicereturnPrint ret = new BOMInvoicereturnPrint();
            ret.id = returninvoice[0].id;
            ret.returntype = returninvoice[0].returntype;
            ret.companyid = returninvoice[0].companyid;
            ret.qty = returninvoice[0].qty;
            ret.amount = returninvoice[0].amount;
            ret.tax = returninvoice[0].tax;
            ret.taxpercentage = returninvoice[0].BOMinvoicereturnitems.Max(x=>x.taxpercentage);
            ret.discount = returninvoice[0].discount;
            ret.returndate = returninvoice[0].returndate;
            ret.returnno = returninvoice[0].returnno;
            ret.total = returninvoice[0].total;
            ret.rounding = returninvoice[0].rounding;
            ret.BOMinvoicereturnitems = returninvoice[0].BOMinvoicereturnitems;
            ret.lrid = returninvoice[0].lrid;
            ret.transportid = returninvoice[0].transportid;
            ret.bundles = returninvoice[0].bundles;
            ret.piece = returninvoice[0].qty;
            ret.freightamt = returninvoice[0].freightamt;
            ret.freighttaxamt = returninvoice[0].freighttaxamt;
            ret.freighttaxid = returninvoice[0].freighttaxid;
            ret.packingcharge = returninvoice[0].packingcharge;
            ret.freight = returninvoice[0].freight;
            ret.weight = returninvoice[0].weight;
            ret.remarks = returninvoice[0].remarks;
            ret.supplierid = returninvoice[0].suppliercompanyid;
            ret.suppliercompanyid = returninvoice[0].suppliercompanyid;
            ret.shiptosupplierid = returninvoice[0].shiptosupplierid;
            ret.agentid = returninvoice[0].agentid;
            ret.inwords = WebAPI.Data.Numberconvertor.rupees(ret.total);
            ret.isinterstatesale = returninvoice[0].isinterstatesale;
            ret.createdusername = returninvoice[0].createdusername;
            ret.modifiedusername = returninvoice[0].modifiedusername;
            ret.einvoice = returninvoice[0].einvoice;
            ret.eway = returninvoice[0].eway;

            if (ret.BOMinvoicereturnitems.Count > 0)
                ret.invoiceinfo = ret.BOMinvoicereturnitems.First().invoiceno + " - " + ret.BOMinvoicereturnitems.First().invoicedate.ToString("dd-MM-yyyy");

            if (ret.BOMinvoicereturnitems.Count > 0)
            {
                var invno = ret.BOMinvoicereturnitems.Select(o => o.invoiceno).Distinct();
                var invdate = ret.BOMinvoicereturnitems.Select(o => o.invoicedate.Date.ToString("dd/MM/yy")).Distinct();
                ret.invoicenos = string.Join(",", invno);
                ret.invoicedates = string.Join(",", invdate);
            }
            ret.lrdate = returninvoice[0].lrdate;
            ret.lrno = returninvoice[0].lrno;
            ret.transportid = returninvoice[0].transportid;
            ret.remarks = returninvoice[0].remarks;

            if (!string.IsNullOrEmpty(ret.lrno))
                ret.lrinfo = ret.lrno + " - " + ret.lrdate.ToString("dd-MM-yyyy");

            if (ret.transportid > 0)
            {
                Data.TransportDataService transdata = new TransportDataService() { Context = this.Context };
                var translist = transdata.Search(new TransportCreteria()
                {
                    id = ret.transportid
                });
                if (translist.Count > 0)
                    ret.transport = translist[0];
            }

            if (returninvoice[0].locationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = returninvoice[0].locationid
                }).First();

                ret.location = new CompanyLocation(location);
                ret.location.name = location.name;
                ret.location.phonenumber = location.additionalinfo == null || location.additionalinfo.locationconfig == null  ? "" : location.additionalinfo.locationconfig.contactno;
                ret.location.address = location.additionalinfo == null ? "" : location.additionalinfo.address;
                ret.location.addresslines = new List<CompanyLocation.Address>();
                if (!string.IsNullOrEmpty(ret.location.address))
                {
                    var addresslist = ret.location.address.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    addresslist.ToList().ForEach(e =>
                    {
                        ret.location.addresslines.Add(new CompanyLocation.Address()
                        {
                            value = e
                        });
                    });
                }
            }
            else
            {
                ret.location = new CompanyLocation();
                ret.location.name = "WAREHOUSE";
            }
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = returninvoice[0].companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            Data.SuppliercompanyDataService scompanyservice = new SuppliercompanyDataService() { Context = this.Context };
            var suppliercompany = scompanyservice.Search(new SuppliercompanyCreteria()
            {
                id = ret.suppliercompanyid,
                isprint = true
            });

            if (suppliercompany.Count > 0)
            {
                ret.supplier = suppliercompany[0];
                if (ret.supplier.cityid > 0)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var city = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = ret.supplier.cityid
                    }).First();
                    ret.supplier.cityname = city.name;
                }
            }

            if (ret.shiptosupplierid > 0)
            {
                suppliercompany = scompanyservice.Search(new SuppliercompanyCreteria()
                {
                    id = ret.shiptosupplierid,
                    isprint = true
                });
                if (suppliercompany.Count > 0)
                {
                    ret.shiptosupplier = suppliercompany[0];
                    if (ret.shiptosupplier.cityid > 0)
                    {
                        ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                        var city = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = ret.shiptosupplier.cityid
                        }).First();
                        ret.shiptosupplier.cityname = city.name;
                    }
                }
            }
            if (ret.agentid > 0)
            {
                Data.AgentDataService agentdata = new AgentDataService() { Context = this.Context };
                var agent = agentdata.Search(new AgentCreteria()
                {
                    id = ret.agentid
                });
                if (agent.Count > 0)
                    ret.agentname = agent[0].name;
            }

            if (ret.freighttaxid > 0)
            {
                Data.TaxDataService taxdata = new TaxDataService() { Context = this.Context };
                var taxlist = taxdata.Search(new TaxCreteria()
                {
                    id = ret.freighttaxid
                });
                if (taxlist.Count > 0)
                    ret.freighttaxname = "(" + taxlist[0].name + ")";
            }
            ret.totaltax = ret.tax + ret.freighttaxamt;

            ret.taxlist = new List<TaxSplit>();
            foreach (var titem in ret.BOMinvoicereturnitems.GroupBy(e => e.taxsplit.taxpercentage))
            {
                TaxSplit taxitem = new TaxSplit();
                taxitem.cgsttaxamount = titem.Sum(e => e.taxsplit.cgsttaxamount);
                taxitem.sgsttaxamount = titem.Sum(e => e.taxsplit.sgsttaxamount);
                taxitem.totalamount = titem.Sum(e => e.taxsplit.totalamount);
                taxitem.name = titem.First().taxsplit.name;
                taxitem.secondtax = titem.First().taxsplit.secondtax;
                taxitem.cgsttaxpercentage = titem.First().taxsplit.cgsttaxpercentage;
                taxitem.sgsttaxpercentage = titem.First().taxsplit.sgsttaxpercentage;
                taxitem.isinterstatesale = ret.isinterstatesale;
                if (ret.isinterstatesale)
                {
                    taxitem.igsttaxamount = taxitem.cgsttaxamount + taxitem.sgsttaxamount;
                    taxitem.cgsttaxamount = 0;
                    taxitem.sgsttaxamount = 0;
                }
                ret.taxlist.Add(taxitem);
            }

            return ret;
        }

        public BOMInvoicereturn Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn obominvoicereturn)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturn.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = obominvoicereturn.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("returnno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.returnno == null ? "" : obominvoicereturn.returnno;
            command.Parameters.Add("returndate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = obominvoicereturn.returndate;
            //command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.lrinvoiceid;
            //command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.lrid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.supplierid;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.rounding;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.suppliercompanyid;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.transportid;
            command.Parameters.Add("bundles", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.bundles;
            command.Parameters.Add("freightamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.freightamt;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.weight;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.remarks;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.discount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.tax;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.amount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.qty;
            command.Parameters.Add("shiptosupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.shiptosupplierid;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturn.isinterstatesale;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.lrno == null ? "" : obominvoicereturn.lrno;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = obominvoicereturn.lrdate;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.total;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.locationid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.freight;
            command.Parameters.Add("returntype", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.returntype;
            command.Parameters.Add("packingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.packingcharge;
            command.Parameters.Add("freighttaxamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = obominvoicereturn.freighttaxamt;
            command.Parameters.Add("freighttaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.freighttaxid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.agentid;

            command.Parameters.Add("billimg", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.billimg;
            command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.einvoice;
            command.Parameters.Add("eway", NpgsqlTypes.NpgsqlDbType.Varchar).Value = obominvoicereturn.eway;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.tolocationid;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.packedby;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    obominvoicereturn.version = Convert.ToInt32(reader[0]);
                }
            }

            return obominvoicereturn;
        }


        public bool Delete(BOMInvoicereturn obominvoicereturn)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, obominvoicereturn);
                    BOMInvoicereturnitemsDataService subbominvoicereturnitems = new BOMInvoicereturnitemsDataService() { Context = this.Context };
                    subbominvoicereturnitems.Context = this.Context;
                    subbominvoicereturnitems.DeleteAll(obominvoicereturn, obominvoicereturn.modifiedby, conn, transaction);
                    if (obominvoicereturn.ReturnType == BOMInvoicereturn.ReturnTypes.DebitNote)
                    {
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Delete(Bomstocktransaction.TransactionType.PurchaseReturn,
                        transaction, conn, obominvoicereturn.id);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, obominvoicereturn);



            }
            return hasSuccessCount;

        }

    
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMInvoicereturn obominvoicereturn)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = obominvoicereturn.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = obominvoicereturn.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obominvoicereturn.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
