using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyGST;

namespace InfyPOS.Data

{
    public class LrinvoicereturnDataService : SAASData
    {
        const string SQL_Search = @"select lrinvoicereturn.id, lrinvoicereturn.isactive, lrinvoicereturn.createdby, lrinvoicereturn.modifiedby, lrinvoicereturn.version, lrinvoicereturn.createdon, lrinvoicereturn.modifiedon, 
                                    lrinvoicereturn.returnno, lrinvoicereturn.returndate , lrinvoicereturn.discount, lrinvoicereturn.tax, lrinvoicereturn.amount, lrinvoicereturn.supplierid, lrinvoicereturn.bundles, lrinvoicereturn.freightamt, lrinvoicereturn.weight,
                                    lrinvoicereturn.transportid,lrinvoicereturn.lrno,lrinvoicereturn.lrdate,lrinvoicereturn.remarks,lrinvoicereturn.suppliercompanyid,lrinvoicereturn.isinterstatesale,lrinvoicereturn.total,lrinvoicereturn.qty,lrinvoicereturn.companyid,
                                    lrinvoicereturn.rounding,lrinvoicereturn.shiptosupplierid,lrinvoicereturn.freight,lrinvoicereturn.locationid,lrinvoicereturn.returntype,lrinvoicereturn.packingcharge,lrinvoicereturn.freighttaxamt,lrinvoicereturn.freighttaxid,
                                    lrinvoicereturn.agentid,suppliercompany.cityid, employee.name employeename,billimg,eway,einvoice,vehicleno,packedby,tolocationid,einvoicejson,lrinvoicereturn.hasapprove
                                    from lrinvoicereturn 
                                    left outer join suppliercompany on suppliercompany.id=suppliercompanyid 
                                    left outer join users on users.id = lrinvoicereturn.createdby
                                    left outer join employee on employeeid = employee.id 
                                    where lrinvoicereturn.id=@id";

        const string SQL_Insert = @"INSERT INTO public.lrinvoicereturn(isactive, createdby, modifiedby, version, createdon, modifiedon, returnno, returndate, discount, tax, returntype,
                                    amount,supplierid,bundles,freightamt,weight,remarks,suppliercompanyid,isinterstatesale,transportid,lrno,lrdate,total,qty,companyid,rounding,
                                    shiptosupplierid,freight,locationid,packingcharge,freighttaxamt,freighttaxid,agentid,lrinvoiceid) 
                                                               VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @returnno, @returndate, @discount, @tax, @returntype,
                                    @amount,@supplierid,@bundles,@freightamt,@weight,@remarks,@suppliercompanyid,@isinterstatesale,@transportid,@lrno,@lrdate,@total,@qty,@companyid,@rounding,
                                    @shiptosupplierid,@freight,@locationid,@packingcharge,@freighttaxamt,@freighttaxid,@agentid,@lrinvoiceid) RETURNING id";

        const string SQL_Update = @"UPDATE public.lrinvoicereturn SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, returnno=@returnno, returndate=@returndate, discount=@discount, tax=@tax, 
                                    amount=@amount,supplierid=@supplierid,transportid=@transportid,bundles=@bundles,freightamt=@freightamt,weight=@weight,remarks=@remarks,suppliercompanyid=@suppliercompanyid,returntype=@returntype,
                                    isinterstatesale=@isinterstatesale,lrno=@lrno,lrdate=@lrdate,total=@total,qty=@qty,companyid=@companyid,rounding=@rounding,shiptosupplierid=@shiptosupplierid,freight=@freight,locationid=@locationid,
                                    packingcharge=@packingcharge,freighttaxamt=@freighttaxamt,freighttaxid=@freighttaxid,agentid=@agentid,billimg=@billimg,eway=@eway,vehicleno=@vehicleno,einvoice=@einvoice,packedby=@packedby,tolocationid=@tolocationid
                                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.lrinvoicereturn set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Lrinvoicereturn> Search(LrinvoicereturnCreteria _lrinvoicereturn)
        {
            List<Lrinvoicereturn> lst = new List<Lrinvoicereturn>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrinvoicereturn.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrinvoicereturn.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("lrinvoicereturn.isactive", true, "=");
                    bool hasminimumcondition = false;

                    if (!string.IsNullOrEmpty(_lrinvoicereturn.returnno))
                    {
                        qb.AddParameter("returnno", _lrinvoicereturn.returnno, " = ");
                        hasminimumcondition = true;
                    }
                    else
                    {
                        qb.AddParameter("coalesce(returntype,0)", "returntype", _lrinvoicereturn.returntype, " = ");
                    }
                    if (_lrinvoicereturn.supplierid > 0)
                    {
                        qb.AddParameter("lrinvoicereturn.supplierid", _lrinvoicereturn.supplierid, " = ");
                        hasminimumcondition = true;
                    }
                    if (_lrinvoicereturn.companyid > 0)
                    {
                        qb.AddParameter("lrinvoicereturn.companyid", _lrinvoicereturn.companyid, " = ");
                    }
                    if (_lrinvoicereturn.locationid > 0)
                    {
                        qb.AddParameter("lrinvoicereturn.locationid", _lrinvoicereturn.locationid, " = ");
                    }
                    if (_lrinvoicereturn.returndate != DateTime.MinValue)
                    {
                        qb.AddParameter("returndate", "fromdate", _lrinvoicereturn.returndate.Date, ">=");
                        qb.AddParameter("returndate", "todate", _lrinvoicereturn.returndate.Date.AddDays(1), "<");
                        hasminimumcondition = true;
                    }
                    if (_lrinvoicereturn.fromreturndate != DateTime.MinValue && _lrinvoicereturn.toreturndate != DateTime.MinValue)
                    {
                        qb.AddParameter("returndate", "datefrom", _lrinvoicereturn.fromreturndate.Date, ">=");
                        qb.AddParameter("returndate", "dateto", _lrinvoicereturn.toreturndate.Date.AddDays(1), "<");
                        hasminimumcondition = true;
                    }

                    if (!string.IsNullOrEmpty(_lrinvoicereturn.barcode))
                    {
                        qb.AddInParamer("lrinvoicereturn.id", "(select lrinvoicereturnid from Lrinvoicereturnitems where barcode='" + WebAPI.Data.Utility.SafeSQL(_lrinvoicereturn.barcode) + "')");
                        hasminimumcondition = true;
                    }
                    if (!string.IsNullOrEmpty(_lrinvoicereturn.lrentryno))
                    {
                        qb.AddInParamer("lrinvoicereturn.id", @"(select lrinvoicereturnid from Lrinvoicereturnitems 
inner join lrinvoiceitems on lrinvoiceitems.id = Lrinvoicereturnitems.lrinvoiceitemsid
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
inner join lrentry on lrentry.id = lrinvoice.lrid
where Lrinvoicereturnitems.isactive = true
and lrentry.lrentryno = '" + WebAPI.Data.Utility.SafeSQL(_lrinvoicereturn.lrentryno) + "')");
                        hasminimumcondition = true;

                    }
                    if (!string.IsNullOrEmpty(_lrinvoicereturn.invoiceno))
                    {
                        qb.AddInParamer("lrinvoicereturn.id", @"(select lrinvoicereturnid from Lrinvoicereturnitems 
inner join lrinvoiceitems on lrinvoiceitems.id = Lrinvoicereturnitems.lrinvoiceitemsid
inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid
where Lrinvoicereturnitems.isactive = true
and lrinvoice.invoiceno = '" + WebAPI.Data.Utility.SafeSQL(_lrinvoicereturn.invoiceno) + "')");
                        hasminimumcondition = true;
                    }
                    //if (!string.IsNullOrEmpty(_lrinvoicereturn.Code))
                    //    qb.AddParameter("code", _lrinvoicereturn.Code, "=");

                    qb.SortField = @"lrinvoicereturn.id desc";
                    //qb.Limit = 100;
                    if (!hasminimumcondition)
                    {
                        throw new Exception("Please provid minimum condition like return date..");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrinvoicereturn olrinvoicereturn = new Lrinvoicereturn();
                        olrinvoicereturn.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrinvoicereturn.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        olrinvoicereturn.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrinvoicereturn.hasapprove = reader["hasapprove"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasapprove"]);
                        olrinvoicereturn.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrinvoicereturn.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        olrinvoicereturn.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrinvoicereturn.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrinvoicereturn.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrinvoicereturn.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrinvoicereturn.returnno = reader["returnno"] == DBNull.Value ? "" : reader["returnno"].ToString();
                        olrinvoicereturn.returndate = reader["returndate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returndate"].ToString());
                        //olrinvoicereturn.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"].ToString());
                        //olrinvoicereturn.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"].ToString());
                        olrinvoicereturn.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        olrinvoicereturn.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        olrinvoicereturn.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        olrinvoicereturn.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"].ToString());
                        olrinvoicereturn.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"].ToString());
                        olrinvoicereturn.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        olrinvoicereturn.shiptosupplierid = reader["shiptosupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shiptosupplierid"].ToString());
                        olrinvoicereturn.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        olrinvoicereturn.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"].ToString());
                        olrinvoicereturn.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"].ToString());
                        olrinvoicereturn.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"].ToString());
                        olrinvoicereturn.bundles = reader["bundles"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bundles"].ToString());
                        olrinvoicereturn.freightamt = reader["freightamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightamt"].ToString());
                        olrinvoicereturn.packingcharge = reader["packingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packingcharge"].ToString());
                        olrinvoicereturn.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"].ToString());
                        olrinvoicereturn.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        olrinvoicereturn.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        olrinvoicereturn.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        olrinvoicereturn.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrinvoicereturn.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        olrinvoicereturn.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        olrinvoicereturn.returntype = reader["returntype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["returntype"]);
                        olrinvoicereturn.freighttaxamt = reader["freighttaxamt"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freighttaxamt"].ToString());
                        olrinvoicereturn.freighttaxid = reader["freighttaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["freighttaxid"]);
                        olrinvoicereturn.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["agentid"].ToString());
                        olrinvoicereturn.createdusername = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        olrinvoicereturn.billimg = reader["billimg"] == DBNull.Value ? "" : reader["billimg"].ToString();
                        olrinvoicereturn.eway = reader["eway"] == DBNull.Value ? "" : reader["eway"].ToString();
                        olrinvoicereturn.einvoice = reader["einvoice"] == DBNull.Value ? "" : reader["einvoice"].ToString();
                        olrinvoicereturn.vehicleno = reader["vehicleno"] == DBNull.Value ? "" : reader["vehicleno"].ToString();
                        olrinvoicereturn.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tolocationid"].ToString());

                        if (_lrinvoicereturn.id > 0)
                        {
                            olrinvoicereturn.einvoicejson = reader["einvoicejson"] == DBNull.Value ? "" : reader["einvoicejson"].ToString();
                        }

                        lst.Add(olrinvoicereturn);
                    }
                }
                if (_lrinvoicereturn.id > 0 && lst.Count == 1)
                {
                    LrinvoicereturnitemsDataService subLrinvoicereturnitems = new LrinvoicereturnitemsDataService() { Context = this.Context };
                    subLrinvoicereturnitems.Context = this.Context;
                    lst[0].Lrinvoicereturnitems = subLrinvoicereturnitems.SearchAll(_lrinvoicereturn.id, true, lst[0].isinterstatesale);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrinvoicereturn);
            }

            if (_lrinvoicereturn.isaudit)
                return UpdateUser(lst);
            return lst;
        }
        public List<Lrinvoicereturn> UpdateUser(List<Lrinvoicereturn> lstreturn)
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

        public Lrinvoicereturn InsertReturn(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Lrinvoicereturn oLrinvoicereturn)
        {
            string sequencetype = "lrinvoicereturn.returnno";
            if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.SupplierCreditNote)
                sequencetype = "lrinvoicereturn.creditnote";
            else if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.SupplierAdvanceNote)
                sequencetype = "lrinvoicereturn.advancenote";
            else if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote ||
                oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.PriceCreditNote)
                sequencetype = "lrinvoicereturn.excessnote";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_dndraft)
                sequencetype = "lrinvoicereturn.draft";


            var sequence = SequenceManager.Instance(Context.OrganizationID).Get(sequencetype,
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisereturnno ? oLrinvoicereturn.locationid : 0,
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany ? oLrinvoicereturn.companyid : 0);

            oLrinvoicereturn.returnno = sequence.GetNextNumber(Context, conn, transaction);
            oLrinvoicereturn.isactive = true;
            oLrinvoicereturn.createdby = Context.UserID;
            oLrinvoicereturn.modifiedby = Context.UserID;
            oLrinvoicereturn.createdon = DateTimeNow;
            oLrinvoicereturn.modifiedon = DateTimeNow;
            oLrinvoicereturn.version = 1;
            Insert(transaction, conn, oLrinvoicereturn);
            LrinvoicereturnitemsDataService subLrinvoicereturnitems = new LrinvoicereturnitemsDataService() { Context = this.Context };
            subLrinvoicereturnitems.Context = this.Context;
            if (oLrinvoicereturn.Lrinvoicereturnitems != null)
            {
                List<Lrinvoicereturnitems> stockFailedItems = new List<Lrinvoicereturnitems>();
                foreach (var item in oLrinvoicereturn.Lrinvoicereturnitems)
                {
                    item.lrinvoicereturnid = oLrinvoicereturn.id;
                    item.isactive = true;
                    if (!subLrinvoicereturnitems.Insert(transaction, conn, oLrinvoicereturn, item))
                    {
                        stockFailedItems.Add(item);
                    }
                }
                if (stockFailedItems.Count > 0)
                {
                    var newlrinvoiceitems = subLrinvoicereturnitems.SyncGLNStock(oLrinvoicereturn, stockFailedItems, conn, transaction);
                    stockFailedItems.ForEach(x => oLrinvoicereturn.Lrinvoicereturnitems.Remove(x));
                    oLrinvoicereturn.Lrinvoicereturnitems.AddRange(stockFailedItems);
                }
            }
            if (Processors.Schedule_Actions.Instance.HasConfig(Context.OrganizationID, Processors.Schedule_Actions.SourceType.PurchaseReturn,
                Processors.Schedule_Actions.Actions.New))
                Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oLrinvoicereturn, Processors.Schedule_Actions.Actions.New);
            return oLrinvoicereturn;
        }

        public bool IsMigration { get; set; }
        public Lrinvoicereturn Insert(Lrinvoicereturn oLrinvoicereturn)
        {
            if (oLrinvoicereturn.companyid == 0)
            {
                throw new Exception("Company ID missing, Please select company");
            }
            if (oLrinvoicereturn.supplierid == 0 || oLrinvoicereturn.suppliercompanyid == 0)
                throw new Exception("Supplier Information missing, Please select Supplier and Supplier Company.");

            if (!IsMigration && (oLrinvoicereturn.Lrinvoicereturnitems == null || oLrinvoicereturn.Lrinvoicereturnitems.Count == 0))
            {
                throw new Exception("Please enter return items.");
                oLrinvoicereturn.Lrinvoicereturnitems.RemoveAll(e => e.qty == 0);
            }

            NpgsqlTransaction transaction = null;
            try
            {

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oLrinvoicereturn = InsertReturn(transaction, conn, oLrinvoicereturn);
                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.PurchaseReturn(oLrinvoicereturn, transaction, conn);
                    InboxManager.Instance(Context.OrganizationID).Update(Context, true, oLrinvoicereturn, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                oLrinvoicereturn.id = 0;
                oLrinvoicereturn.returnno = "";
                oLrinvoicereturn.errormessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturn);
            }

            return oLrinvoicereturn;
        }


        private void ValidateStock(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturnitems oLrinvoicereturnitems)
        {
            if (oLrinvoicereturnitems.stockid <= 0)
            {
                StockDataService stockDataService = new StockDataService() { Context = this.Context };
                Stock stock = new Stock();
                stock.barcode = "I" + oLrinvoicereturnitems.supplierid.ToString("F0") + "I" + oLrinvoicereturnitems.lrinvoicereturnid.ToString("F0")
                                    + "I" + oLrinvoicereturnitems.lrinvoiceitemsid.ToString("F0");
                stock.qty = oLrinvoicereturnitems.qty;
                stock.buyingprice = oLrinvoicereturnitems.rate;
                stock.buyingqty = oLrinvoicereturnitems.qty;
                stock.companyid = oLrinvoicereturnitems.companyid;
                stock.designid = oLrinvoicereturnitems.designid;
                stock.hsncode = oLrinvoicereturnitems.hsncode;
                stock.invoiceitemid = oLrinvoicereturnitems.lrinvoiceitemsid;
                stock.itemid = oLrinvoicereturnitems.itemid;
                stock.price = oLrinvoicereturnitems.rate + oLrinvoicereturnitems.tax;
                stock.productid = oLrinvoicereturnitems.productid;
                stock.purchasediscount = oLrinvoicereturnitems.discount;
                stock.purchaseqty = oLrinvoicereturnitems.qty;
                stock.purchasetax = oLrinvoicereturnitems.tax;
                stock.returnqty = oLrinvoicereturnitems.qty;
                stock.supplierid = oLrinvoicereturnitems.supplierid;
                stock.version = 1;
                stock.createdby = Context.UserID;
                stock.modifiedby = Context.UserID;
                stock.createdon = DateTimeNow;
                stock.modifiedon = DateTimeNow;
                stock.isactive = true;
                stockDataService.Insert(transaction, conn, stock);
                oLrinvoicereturnitems.id = stock.id;
            }
        }

        public List<Lrinvoicereturn> InsertAll(Lrinvoicereturn coreReturn)
        {
            if (coreReturn.Lrinvoicereturnitems.Exists(e => e.companyid == 0))
            {
                throw new Exception("Company ID missing, Please select company");
            }

            coreReturn.Lrinvoicereturnitems.RemoveAll(e => e.qty == 0);

            List<Lrinvoicereturn> oLrinvoicereturns = new List<Lrinvoicereturn>();
            NpgsqlTransaction transaction = null;
            try
            {
                foreach (var item in coreReturn.Lrinvoicereturnitems.GroupBy(e => new { e.companyid, e.supplierid, e.suppliercompanyid }))
                {
                    Lrinvoicereturn oLrinvoicereturn = new Lrinvoicereturn();
                    oLrinvoicereturn.companyid = item.First().companyid;
                    oLrinvoicereturn.locationid = coreReturn.locationid;
                    oLrinvoicereturn.suppliercompanyid = item.First().suppliercompanyid;
                    oLrinvoicereturn.supplierid = item.First().supplierid;
                    oLrinvoicereturn.isinterstatesale = item.First().isinterstatesale;
                    oLrinvoicereturn.freight = coreReturn.freight;
                    oLrinvoicereturn.lrdate = coreReturn.lrdate;
                    oLrinvoicereturn.lrid = coreReturn.lrid;
                    oLrinvoicereturn.lrinvoiceid = coreReturn.lrinvoiceid;
                    oLrinvoicereturn.lrno = coreReturn.lrno;
                    oLrinvoicereturn.remarks = coreReturn.remarks;
                    oLrinvoicereturn.returndate = coreReturn.returndate;
                    oLrinvoicereturn.transportid = coreReturn.transportid;
                    oLrinvoicereturn.isactive = true;
                    oLrinvoicereturn.createdby = Context.UserID;
                    oLrinvoicereturn.modifiedby = Context.UserID;
                    oLrinvoicereturn.createdon = DateTimeNow;
                    oLrinvoicereturn.modifiedon = DateTimeNow;
                    oLrinvoicereturn.version = 1;
                    oLrinvoicereturn.Lrinvoicereturnitems = new List<Lrinvoicereturnitems>();
                    oLrinvoicereturn.Lrinvoicereturnitems.AddRange(item);
                    oLrinvoicereturn.amount = oLrinvoicereturn.Lrinvoicereturnitems.Sum(e => e.amount);
                    oLrinvoicereturn.discount = oLrinvoicereturn.Lrinvoicereturnitems.Sum(e => e.discount);
                    oLrinvoicereturn.qty = oLrinvoicereturn.Lrinvoicereturnitems.Sum(e => e.qty);
                    oLrinvoicereturn.tax = oLrinvoicereturn.Lrinvoicereturnitems.Sum(e => e.tax);

                    oLrinvoicereturn.total = Math.Round(oLrinvoicereturn.Lrinvoicereturnitems.Sum(e => e.amount - e.discount + e.tax), 0);
                    oLrinvoicereturn.rounding = oLrinvoicereturn.total - Math.Round(oLrinvoicereturn.Lrinvoicereturnitems.Sum(e => e.amount - e.discount + e.tax), 2);
                    oLrinvoicereturns.Add(oLrinvoicereturn);
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var oLrinvoicereturn in oLrinvoicereturns)
                    {
                        string sequencetype = "lrinvoicereturn.returnno";
                        if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.SupplierCreditNote)
                            sequencetype = "lrinvoicereturn.creditnote";
                        else if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.SupplierAdvanceNote)
                            sequencetype = "lrinvoicereturn.advancenote";
                        else if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote ||
                            oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.PriceCreditNote)
                            sequencetype = "lrinvoicereturn.excessnote";

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_dndraft)
                            sequencetype = "lrinvoicereturn.draft";


                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get(sequencetype,
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisereturnno ? oLrinvoicereturn.locationid : 0,
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany ? oLrinvoicereturn.companyid : 0);


                        NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                        oLrinvoicereturn.returnno = sequence.GetNextNumber(Context, conn, transaction);
                        Insert(transaction, conn, oLrinvoicereturn);
                        LrinvoicereturnitemsDataService subLrinvoicereturnitems = new LrinvoicereturnitemsDataService() { Context = this.Context };
                        subLrinvoicereturnitems.Context = this.Context;
                        foreach (var item in oLrinvoicereturn.Lrinvoicereturnitems)
                        {
                            item.lrinvoicereturnid = oLrinvoicereturn.id;
                            item.isactive = true;
                            subLrinvoicereturnitems.Insert(transaction, conn, oLrinvoicereturn, item);
                        }
                        StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                        stocktransactionDataService.PurchaseReturn(oLrinvoicereturn, transaction, conn);
                        InboxManager.Instance(Context.OrganizationID).Update(Context, true, oLrinvoicereturn, transaction, conn);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                oLrinvoicereturns.Clear();
                oLrinvoicereturns.Add(new Lrinvoicereturn()
                {
                    errormessage = exp.Message
                });
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, coreReturn);
            }
            return oLrinvoicereturns;
        }

        internal Lrinvoicereturn ClosedSupplier(Lrinvoicereturn item)
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

            string query = "update lrinvoicereturn set supplierid=@supplierid,suppliercompanyid=@suppliercompanyid,dsupplierid=@dsupplierid,version=200 where id=@id and version=@version RETURNING version";
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

        public Lrinvoicereturn Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn oLrinvoicereturn)
        {
            if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote &&
                (oLrinvoicereturn.supplierid == 0 || oLrinvoicereturn.suppliercompanyid == 0))
                throw new Exception("Supplier Information missing, Please select Supplier and Supplier Company.");

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturn.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("returnno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.returnno == null ? "" : oLrinvoicereturn.returnno;
            command.Parameters.Add("returndate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicereturn.returndate;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.lrinvoiceid;
            //command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.lrid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.discount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.tax;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.total;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.rounding;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.amount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.qty;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.supplierid;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.suppliercompanyid;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.transportid;
            command.Parameters.Add("bundles", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.bundles;
            command.Parameters.Add("freightamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.freightamt;
            command.Parameters.Add("packingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.packingcharge;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.weight;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.remarks == null ? "" : oLrinvoicereturn.remarks;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturn.isinterstatesale;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.lrno == null ? "" : oLrinvoicereturn.lrno;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicereturn.lrdate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.locationid;
            command.Parameters.Add("shiptosupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.shiptosupplierid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.freight;
            command.Parameters.Add("returntype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.returntype;
            command.Parameters.Add("freighttaxamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.freighttaxamt;
            command.Parameters.Add("freighttaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.freighttaxid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.agentid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoicereturn.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrinvoicereturn;
        }



        internal Lrinvoicereturn UpdateLRInfo(Lrinvoicereturn item)
        {
            var existing = Search(new LrinvoicereturnCreteria()
            {
                id = item.id
            });
            if (existing != null && existing.Count == 1)
            {
                existing[0].remarks = item.remarks;
                existing[0].transportid = item.transportid;
                existing[0].lrdate = item.lrdate;
                existing[0].lrno = item.lrno;
                existing[0].billimg = item.billimg;
                return Update(item, false);
            }
            return item;
        }

        internal Lrinvoicereturn AddDirectDebitNote(Lrinvoicereturn.ReturnTypes rt, LrinvoiceItemDetail lrinvoiceItemDetail, Pricedrop replacement)
        {
            return AddDirectDebitNote(rt, lrinvoiceItemDetail, replacement.qty, replacement.pricedrop, rt.ToString());
        }

        internal Lrinvoicereturn AddDirectDebitNote(Lrinvoicereturn.ReturnTypes rt, LrinvoiceItemDetail lrinvoiceItemDetail, Deathonarrival replacement)
        {
            return AddDirectDebitNote(rt, lrinvoiceItemDetail, replacement.qty, replacement.qty * (replacement.rate + replacement.tax), rt.ToString());
        }
        internal Lrinvoicereturn AddDirectDebitNote(LrinvoiceItemDetail item, decimal qty, string remark)
        {
            return AddDirectDebitNote(Lrinvoicereturn.ReturnTypes.DebitNote, item, qty, 0, remark);
        }
        internal Lrinvoicereturn AddDirectDebitNote(Lrinvoicereturn.ReturnTypes returnTypes, LrinvoiceItemDetail item, decimal qty, decimal amount, string remark)
        {
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            lrinvoiceDataService.Context = this.Context;
            var invoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
            {
                id = item.lrinvoiceid
            });

            LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
            lrentryDataService.Context = this.Context;
            var lrentry = lrentryDataService.Search(new LrentryCreteria()
            {
                id = invoice[0].lrid
            });

            string query = @"select returndate,lrinvoicereturn.id from lrinvoicereturnitems
                        inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid
                        inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true  
                        and lrinvoiceitems.lrinvoiceid=@lrinvoiceid and coalesce(returntype,0)=@returntype
                        order by returndate desc
                        limit 1";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("lrinvoiceid", item.lrinvoiceid);
            param.Add("returntype", (int)returnTypes);
            long lrinvoicereturnid = 0;
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read() && DateTimeNow.Subtract(reader.GetDateTime(0)).TotalDays < 7)
                {
                    lrinvoicereturnid = reader.GetInt64(1);
                }
            }
            Lrinvoicereturnitems lrinvoicereturnitems = new Lrinvoicereturnitems();
            lrinvoicereturnitems.createdby = Context.UserID;
            lrinvoicereturnitems.createdon = DateTimeNow;
            lrinvoicereturnitems.designid = item.designid;
            lrinvoicereturnitems.hsncode = item.hsncode;
            lrinvoicereturnitems.uan = item.uan;
            lrinvoicereturnitems.invoicedate = invoice.First().invoicedate;
            lrinvoicereturnitems.invoiceno = invoice.First().invoiceno;
            lrinvoicereturnitems.isactive = true;
            lrinvoicereturnitems.itemid = item.itemid;
            lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturnid;
            lrinvoicereturnitems.productid = item.productid;
            lrinvoicereturnitems.taxid = item.taxid;
            lrinvoicereturnitems.taxpercentage = item.purchasetaxpercentage;
            lrinvoicereturnitems.version = 1;
            lrinvoicereturnitems.lrinvoiceitemsid = item.id;
            lrinvoicereturnitems.barcode = item.barcodeList.Count == 1 ? item.barcodeList.First() : "";
            if (lrinvoicereturnitems.barcode.IndexOf('-') > 0)
                lrinvoicereturnitems.barcode = "";
            lrinvoicereturnitems.qty = qty;
            if (amount == 0)
            {
                lrinvoicereturnitems.rate = item.basebuyingprice;
                lrinvoicereturnitems.tax = (item.purchasetax / item.buyingqty) * qty;
                lrinvoicereturnitems.discount = item.purchasediscount * qty;
                lrinvoicereturnitems.discountpercentage = item.purchasediscountpercentage;
                lrinvoicereturnitems.amount = (lrinvoicereturnitems.qty * lrinvoicereturnitems.rate);
            }
            else
            {
                if (lrinvoicereturnitems.taxpercentage > 0)
                {
                    lrinvoicereturnitems.rate = Math.Round((amount / lrinvoicereturnitems.taxpercentage + 100) * 100 / qty, 2);
                    lrinvoicereturnitems.tax = Math.Round((lrinvoicereturnitems.rate * qty * lrinvoicereturnitems.taxpercentage) / 100, 2);
                }
                else
                {
                    lrinvoicereturnitems.rate = Math.Round(amount / qty, 2);
                }
                lrinvoicereturnitems.amount = amount;
            }
            var lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
            if (lrinvoicereturnid > 0)
            {
                Lrinvoicereturn lrinvoicereturn = lrinvoicereturnDataService.Search(new LrinvoicereturnCreteria()
                {
                    id = lrinvoicereturnid
                }).First();

                if (lrinvoicereturn.Lrinvoicereturnitems.Exists(e => e.lrinvoiceitemsid == item.id))
                {
                    lrinvoicereturnitems = lrinvoicereturn.Lrinvoicereturnitems.First(e => e.lrinvoiceitemsid == item.id);

                    lrinvoicereturn.tax = lrinvoicereturnitems.tax - lrinvoicereturn.tax;
                    lrinvoicereturn.discount = lrinvoicereturnitems.discount - lrinvoicereturn.discount;
                    lrinvoicereturn.amount = lrinvoicereturnitems.amount - lrinvoicereturn.amount;
                    lrinvoicereturn.qty = lrinvoicereturnitems.qty - lrinvoicereturn.qty;

                    lrinvoicereturnitems.taxid = item.taxid;
                    lrinvoicereturnitems.taxpercentage = item.purchasetaxpercentage;
                    lrinvoicereturnitems.lrinvoiceitemsid = item.id;
                    lrinvoicereturnitems.barcode = item.barcodeList.Count == 1 ? item.barcodeList.First() : "";
                    if (lrinvoicereturnitems.barcode.IndexOf('-') > 0)
                        lrinvoicereturnitems.barcode = "";
                    lrinvoicereturnitems.qty = qty;
                    lrinvoicereturnitems.rate = item.basebuyingprice;
                    lrinvoicereturnitems.tax = (item.purchasetax / item.buyingqty) * qty;
                    lrinvoicereturnitems.discount = item.purchasediscount * qty;
                    lrinvoicereturnitems.discountpercentage = item.purchasediscountpercentage;
                    lrinvoicereturnitems.amount = (lrinvoicereturnitems.qty * lrinvoicereturnitems.rate);
                    lrinvoicereturnitems.modifiedby = Context.UserID;
                    lrinvoicereturnitems.modifiedon = DateTimeNow;
                }
                else
                {
                    lrinvoicereturnitems.modifiedby = Context.UserID;
                    lrinvoicereturnitems.modifiedon = DateTimeNow;
                    lrinvoicereturn.Lrinvoicereturnitems.Add(lrinvoicereturnitems);
                    //ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                    //var sellingmodes = productsDataService.GetSellingModes(new long[] { item.productid }.ToList());
                    //lrinvoicereturn.piece += sellingmodes.First().Value == 3 ? 1 : lrinvoicereturnitems.qty;
                }
                lrinvoicereturn.remarks = string.IsNullOrEmpty(lrinvoicereturn.remarks) ? remark : lrinvoicereturn.remarks;

                lrinvoicereturn.tax = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.tax);
                lrinvoicereturn.discount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.discount);
                lrinvoicereturn.amount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.amount);
                lrinvoicereturn.qty = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.qty);
                lrinvoicereturn.total = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.tax + x.amount - x.discount);
                lrinvoicereturn.rounding = lrinvoicereturn.total - (lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount);

                if (lrinvoicereturn.Lrinvoicereturnitems.Exists(e => e.lrinvoiceitemsid == item.id))
                    lrinvoicereturn.Lrinvoicereturnitems.RemoveAll(e => e.lrinvoiceitemsid != item.id);
                else
                    lrinvoicereturn.Lrinvoicereturnitems.RemoveAll(e => e.id > 0);
                lrinvoicereturnDataService.UpdateForItemReturn(lrinvoicereturn);
                return lrinvoicereturn;
            }
            else
            {
                Lrinvoicereturn lrinvoicereturn = new Lrinvoicereturn();
                lrinvoicereturn.ReturnType = returnTypes;
                lrinvoicereturn.companyid = invoice[0].companyid;
                lrinvoicereturn.isinterstatesale = invoice[0].isinterstatesale;
                lrinvoicereturn.createdby = Context.UserID;
                lrinvoicereturn.createdon = DateTimeNow;
                lrinvoicereturn.isactive = true;
                lrinvoicereturn.Lrinvoicereturnitems = new List<Lrinvoicereturnitems>();
                lrinvoicereturn.returndate = DateTimeNow.Date;
                lrinvoicereturn.suppliercompanyid = invoice[0].suppliercompanyid;
                lrinvoicereturn.supplierid = invoice[0].supplierid;
                lrinvoicereturn.version = 1;
                lrinvoicereturn.remarks = remark;
                lrinvoicereturn.lrinvoiceid = invoice[0].id;
                lrinvoicereturn.locationid = lrentry[0].entrylocationid;
                lrinvoicereturn.Lrinvoicereturnitems.Add(lrinvoicereturnitems);
                lrinvoicereturn.tax = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.tax);
                lrinvoicereturn.discount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.discount);
                lrinvoicereturn.amount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.amount);
                lrinvoicereturn.qty = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.qty);
                lrinvoicereturn.total = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.tax + x.amount - x.discount);
                lrinvoicereturn.rounding = lrinvoicereturn.total - (lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount);
                lrinvoicereturnDataService.Insert(lrinvoicereturn);
                return lrinvoicereturn;
            }
        }

        internal Lrinvoicereturn AddDiscountNote(Lrinvoicereturn.ReturnTypes returnTypes, long invoiceid, decimal amount, string remark)
        {
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            lrinvoiceDataService.Context = this.Context;
            var invoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
            {
                id = invoiceid
            });

            LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
            lrentryDataService.Context = this.Context;
            var lrentry = lrentryDataService.Search(new LrentryCreteria()
            {
                id = invoice[0].lrid
            });

            string query = @"select returndate,lrinvoicereturn.id from lrinvoicereturnitems
                        inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid
                        inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true  
                        and lrinvoiceitems.lrinvoiceid=@lrinvoiceid and coalesce(returntype,0)=@returntype
                        order by returndate desc
                        limit 1";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("lrinvoiceid", invoiceid);
            param.Add("returntype", (int)returnTypes);
            long lrinvoicereturnid = 0;
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read() && DateTimeNow.Subtract(reader.GetDateTime(0)).TotalDays < 7)
                {
                    lrinvoicereturnid = reader.GetInt64(1);
                }
            }
            var lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
            if (lrinvoicereturnid > 0)
            {
                Lrinvoicereturn lrinvoicereturn = lrinvoicereturnDataService.Search(new LrinvoicereturnCreteria()
                {
                    id = lrinvoicereturnid
                }).First();

                lrinvoicereturn.remarks = string.IsNullOrEmpty(lrinvoicereturn.remarks) ? remark : lrinvoicereturn.remarks;
                lrinvoicereturn.amount = lrinvoicereturn.amount + amount;
                lrinvoicereturn.total = Math.Round((lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount), 0);
                lrinvoicereturn.rounding = lrinvoicereturn.total - (lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount);
                lrinvoicereturnDataService.Update(lrinvoicereturn);
                return lrinvoicereturn;
            }
            else
            {
                Lrinvoicereturn lrinvoicereturn = new Lrinvoicereturn();
                lrinvoicereturn.ReturnType = returnTypes;
                lrinvoicereturn.companyid = invoice[0].companyid;
                lrinvoicereturn.isinterstatesale = invoice[0].isinterstatesale;
                lrinvoicereturn.createdby = Context.UserID;
                lrinvoicereturn.createdon = DateTimeNow;
                lrinvoicereturn.isactive = true;
                lrinvoicereturn.Lrinvoicereturnitems = new List<Lrinvoicereturnitems>();
                lrinvoicereturn.returndate = DateTimeNow.Date;
                lrinvoicereturn.suppliercompanyid = invoice[0].suppliercompanyid;
                lrinvoicereturn.supplierid = invoice[0].supplierid;
                lrinvoicereturn.version = 1;
                lrinvoicereturn.remarks = remark;
                lrinvoicereturn.lrinvoiceid = invoice[0].id;
                lrinvoicereturn.locationid = lrentry[0].entrylocationid;
                lrinvoicereturn.amount = amount;
                lrinvoicereturn.total = amount;
                lrinvoicereturn.rounding = lrinvoicereturn.total - (lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount);
                lrinvoicereturnDataService.Insert(lrinvoicereturn);
                return lrinvoicereturn;
            }


        }
        public Lrinvoicereturn UpdateForItemReturn(Lrinvoicereturn oLrinvoicereturn)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrinvoicereturn);
                    LrinvoicereturnitemsDataService subLrinvoicereturnitems = new LrinvoicereturnitemsDataService() { Context = this.Context };
                    subLrinvoicereturnitems.Context = this.Context;
                    subLrinvoicereturnitems.UpdateForItemReturn(transaction, conn, oLrinvoicereturn, oLrinvoicereturn.Lrinvoicereturnitems, oLrinvoicereturn.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturn);


            }

            return oLrinvoicereturn;
        }

        public Lrinvoicereturn Update(Lrinvoicereturn oLrinvoicereturn)
        {
            if (oLrinvoicereturn.companyid == 0)
            {
                throw new Exception("Company ID missing, Please select company");
            }
            if (oLrinvoicereturn.supplierid == 0 || oLrinvoicereturn.suppliercompanyid == 0)
                throw new Exception("Supplier Information missing, Please select Supplier and Supplier Company.");

            return Update(oLrinvoicereturn, true);
        }

        internal Lrinvoicereturn AddAdvance(Voucher oVoucher, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Lrinvoicereturn oLrinvoicereturn = new Lrinvoicereturn();
            var advanceledgers = oVoucher.voucheritems.FindAll(e => e.attributes != null && e.attributes.supplierid > 0);
            if (advanceledgers.Count > 1)
            {
                foreach (var voucheritem in advanceledgers)
                {
                    oLrinvoicereturn.supplierid = voucheritem.attributes.supplierid;
                    using (var reader = ExecuteReader("select id from suppliercompany where isactive = true and supplierid=" + oLrinvoicereturn.supplierid, new SortedList<string, object>()))
                    {
                        if (reader.Read())
                            oLrinvoicereturn.suppliercompanyid = reader.GetInt64(0);
                    }
                    oLrinvoicereturn.companyid = oVoucher.companyid;
                    oLrinvoicereturn.amount = Math.Abs(voucheritem.amount);
                    oLrinvoicereturn.isactive = true;
                    oLrinvoicereturn.version = 1;
                    oLrinvoicereturn.modifiedby = Context.UserID;
                    oLrinvoicereturn.createdby = Context.UserID;
                    oLrinvoicereturn.modifiedon = DateTimeNow;
                    oLrinvoicereturn.createdon = DateTimeNow;
                    oLrinvoicereturn.locationid = oVoucher.locationid;
                    oLrinvoicereturn.returndate = oVoucher.voucherdate;
                    oLrinvoicereturn.ReturnType = Lrinvoicereturn.ReturnTypes.SupplierAdvanceNote;
                    oLrinvoicereturn.total = Math.Abs(voucheritem.amount);
                    InsertReturn(transaction, conn, oLrinvoicereturn);
                    if (voucheritem.attributes == null)
                        voucheritem.attributes = new Voucheritems.voucharattributes();
                    voucheritem.attributes.lrinvoicereturnid = oLrinvoicereturn.id;
                }
            }
            else if (advanceledgers.Count == 1)
            {
                var voucheritem = oVoucher.voucheritems.Find(e => e.attributes != null && e.attributes.supplierid > 0);
                oLrinvoicereturn.supplierid = voucheritem.attributes.supplierid;
                using (var reader = ExecuteReader("select id from suppliercompany where isactive = true and supplierid=" + oLrinvoicereturn.supplierid, new SortedList<string, object>()))
                {
                    if (reader.Read())
                        oLrinvoicereturn.suppliercompanyid = reader.GetInt64(0);
                }
                oLrinvoicereturn.companyid = oVoucher.companyid;
                oLrinvoicereturn.amount = Math.Abs(voucheritem.amount);
                oLrinvoicereturn.isactive = true;
                oLrinvoicereturn.version = 1;
                oLrinvoicereturn.modifiedby = Context.UserID;
                oLrinvoicereturn.createdby = Context.UserID;
                oLrinvoicereturn.modifiedon = DateTimeNow;
                oLrinvoicereturn.createdon = DateTimeNow;
                oLrinvoicereturn.locationid = oVoucher.locationid;
                oLrinvoicereturn.returndate = oVoucher.voucherdate;
                oLrinvoicereturn.ReturnType = Lrinvoicereturn.ReturnTypes.SupplierAdvanceNote;
                oLrinvoicereturn.total = Math.Abs(voucheritem.amount);
                InsertReturn(transaction, conn, oLrinvoicereturn);
                if (voucheritem.attributes == null)
                    voucheritem.attributes = new Voucheritems.voucharattributes();
                voucheritem.attributes.lrinvoicereturnid = oLrinvoicereturn.id;
            }
            return oLrinvoicereturn;
        }

        public Lrinvoicereturn Update(Lrinvoicereturn oLrinvoicereturn, bool updateitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrinvoicereturn);
                    if (updateitems)
                    {
                        LrinvoicereturnitemsDataService subLrinvoicereturnitems = new LrinvoicereturnitemsDataService() { Context = this.Context };
                        subLrinvoicereturnitems.Context = this.Context;
                        subLrinvoicereturnitems.Update(transaction, conn, oLrinvoicereturn, oLrinvoicereturn.Lrinvoicereturnitems, oLrinvoicereturn.id);
                    }

                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.PurchaseReturnUpdate(oLrinvoicereturn, transaction, conn);

                    if (Processors.Schedule_Actions.Instance.HasConfig(Context.OrganizationID, Processors.Schedule_Actions.SourceType.PurchaseReturn, Processors.Schedule_Actions.Actions.New))
                        Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oLrinvoicereturn, Processors.Schedule_Actions.Actions.Updated);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturn);
            }

            return oLrinvoicereturn;
        }

        public LrinvoicereturnPrint Print(Lrinvoicereturn item)
        {
            var returninvoice = Search(new LrinvoicereturnCreteria()
            {
                id = item.id,
                isaudit = true
            });

            LrinvoicereturnPrint ret = new LrinvoicereturnPrint();
            if (returninvoice.Count > 0)
            {
                ret.id = returninvoice[0].id;
                ret.returntype = returninvoice[0].returntype;
                ret.companyid = returninvoice[0].companyid;
                ret.qty = returninvoice[0].qty;
                ret.amount = returninvoice[0].amount;
                ret.tax = returninvoice[0].tax;
                ret.discount = returninvoice[0].discount;
                ret.returndate = returninvoice[0].returndate;
                ret.returnno = returninvoice[0].returnno;
                ret.total = returninvoice[0].total;
                ret.rounding = returninvoice[0].rounding;
                ret.Lrinvoicereturnitems = returninvoice[0].Lrinvoicereturnitems;
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
                ret.vehicleno = returninvoice[0].vehicleno;
                ret.eway = returninvoice[0].eway;
                ret.einvoicejson = returninvoice[0].einvoicejson;
                ret.hasapprove = returninvoice[0].hasapprove;


                if (ret.Lrinvoicereturnitems.Count > 0)
                    ret.invoiceinfo = ret.Lrinvoicereturnitems.First().invoiceno + " - " + ret.Lrinvoicereturnitems.First().invoicedate.ToString("dd-MM-yyyy");

                if (ret.Lrinvoicereturnitems.Count > 0)
                {
                    var entryno = ret.Lrinvoicereturnitems.Select(o => o.lrentryno).Distinct();
                    var invno = ret.Lrinvoicereturnitems.Select(o => o.invoiceno).Distinct();
                    var invdate = ret.Lrinvoicereturnitems.Select(o => o.invoicedate.Date.ToString("dd/MM/yy")).Distinct();
                    ret.lrentrynos = string.Join(",", entryno);
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
                    ret.location.phonenumber = location.additionalinfo == null || location.additionalinfo.locationconfig == null ? "" : location.additionalinfo.locationconfig.contactno;
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

                    if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                        && location.additionalinfo.locationconfig.cityid > 0)
                    {
                        var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = location.additionalinfo.locationconfig.cityid
                        });
                        if (cityinfo.Count > 0)
                            ret.location.cityname = cityinfo[0].name;
                    }

                    if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                        && location.additionalinfo.locationconfig.stateid > 0)
                    {
                        var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = location.additionalinfo.locationconfig.stateid
                        });
                        if (stateinfo.Count > 0)
                            ret.location.statename = stateinfo[0].name;
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
                foreach (var titem in ret.Lrinvoicereturnitems.GroupBy(e => e.taxsplit.taxpercentage))
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
            }
            else
            {
                return null;
            }
            return ret;
        }

        public Lrinvoicereturn Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn oLrinvoicereturn)
        {
            if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote &&
                (oLrinvoicereturn.supplierid == 0 || oLrinvoicereturn.suppliercompanyid == 0))
                throw new Exception("Supplier Information missing, Please select Supplier and Supplier Company.");

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturn.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicereturn.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("returnno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.returnno == null ? "" : oLrinvoicereturn.returnno;
            command.Parameters.Add("returndate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicereturn.returndate;
            //command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.lrinvoiceid;
            //command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.lrid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.supplierid;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.rounding;
            command.Parameters.Add("suppliercompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.suppliercompanyid;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.transportid;
            command.Parameters.Add("bundles", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.bundles;
            command.Parameters.Add("freightamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.freightamt;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.weight;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.remarks;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.discount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.tax;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.amount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.qty;
            command.Parameters.Add("shiptosupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.shiptosupplierid;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturn.isinterstatesale;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.lrno == null ? "" : oLrinvoicereturn.lrno;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrinvoicereturn.lrdate;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.total;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.locationid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.freight;
            command.Parameters.Add("returntype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.returntype;
            command.Parameters.Add("packingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.packingcharge;
            command.Parameters.Add("freighttaxamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrinvoicereturn.freighttaxamt;
            command.Parameters.Add("freighttaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.freighttaxid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.agentid;

            command.Parameters.Add("billimg", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.billimg == null ? "" : oLrinvoicereturn.billimg;
            command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.einvoice == null ? "" : oLrinvoicereturn.einvoice;
            command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.vehicleno == null ? "" : oLrinvoicereturn.vehicleno;
            command.Parameters.Add("eway", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrinvoicereturn.eway == null ? "" : oLrinvoicereturn.eway;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.tolocationid;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.packedby;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrinvoicereturn.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrinvoicereturn;
        }

        public bool UpdateHSN(Lrinvoicereturn oLrinvoicereturn)
        {
            NpgsqlTransaction transaction = null;
            if (oLrinvoicereturn == null || oLrinvoicereturn.id == 0)
            {
                throw new Exception("Invalid Puchase Return information. Please select Purchase Return");
            }
            int successCount = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                string SQL = @"with newhsn as (
select lrinvoiceitems.id, products.hsncode
from lrinvoicereturn inner join lrinvoicereturnitems on lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturn.id
inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid
inner join products on products.id = lrinvoicereturnitems.productid
where lrinvoicereturnid=@lrinvoicereturnid
)
update lrinvoiceitems set hsncode = newhsn.hsncode, version = version + 1, modifiedby = @modifiedby, modifiedon = @modifiedon  
from newhsn where newhsn.id = lrinvoiceitems.id; ";

                var command = GetCommand(SQL, conn, transaction);
                command.Parameters.Add("lrinvoicereturnid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.id;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                successCount = command.ExecuteNonQuery();

                transaction.Commit();
                conn.Close();
            }
            return successCount > 0;
        }
        public bool Delete(Lrinvoicereturn oLrinvoicereturn)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            NpgsqlCommand command = null;
            int _count = 0;
            string _sql = @"select count(id) countno from supplierpaymentinvoice 
                        where isactive=true and islrinvoicereturn=true and invoiceid=@id";

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    command = GetCommand(_sql, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.id;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _count = reader["countno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["countno"]);
                        }
                    }
                    if (_count > 0)
                        throw new Exception("Paid debit note cannot be deleted.");


                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oLrinvoicereturn);
                    LrinvoicereturnitemsDataService subLrinvoicereturnitems = new LrinvoicereturnitemsDataService() { Context = this.Context };
                    subLrinvoicereturnitems.Context = this.Context;
                    subLrinvoicereturnitems.DeleteAll(oLrinvoicereturn, oLrinvoicereturn.modifiedby, conn, transaction);
                    StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.PurchaseReturnDelete(oLrinvoicereturn.id, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrinvoicereturn);
            }
            return hasSuccessCount;

        }

        internal bool ChangeStatus(LrinvoicereturnCreteria item)
        {
            if (item == null || string.IsNullOrEmpty(item.ids))
                throw new Exception("Please select the debit note for status change.");

            var successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_dndraft)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("id", 0);
                        param.Add("returnno", "");
                        if (item.hasapprove)
                        {
                            //NpgsqlCommand commandupdate = GetCommand(
                            //string.Format("Update Lrinvoicereturn set approvedby=@approvedby,approvedon=@approvedon,hasapprove=@hasapprove where hasapprove=false and id in ({0})", item.ids),
                            //conn, transaction);
                            //commandupdate.Parameters.Add("hasapprove", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.hasapprove;
                            //commandupdate.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            //commandupdate.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            //successCount = commandupdate.ExecuteNonQuery();
                            var dset = ExecuteDataSet(
                                string.Format("select hasapprove,returntype,locationid,companyid,id from Lrinvoicereturn where id in ({0})", item.ids), param, conn, transaction);

                            var reader = dset.Tables[0].CreateDataReader();
                            bool rollback = false;
                            while (!rollback && reader.Read() && (reader.IsDBNull(0) || !reader.GetBoolean(0)))
                            {
                                var returntype = (Lrinvoicereturn.ReturnTypes)reader.GetInt32(1);
                                string sequencetype = "lrinvoicereturn.returnno";
                                if (returntype == Lrinvoicereturn.ReturnTypes.SupplierCreditNote)
                                    sequencetype = "lrinvoicereturn.creditnote";
                                else if (returntype == Lrinvoicereturn.ReturnTypes.SupplierAdvanceNote)
                                    sequencetype = "lrinvoicereturn.advancenote";
                                else if (returntype == Lrinvoicereturn.ReturnTypes.ExcessCreditNote ||
                                    returntype == Lrinvoicereturn.ReturnTypes.PriceCreditNote)
                                    sequencetype = "lrinvoicereturn.excessnote";

                                var locationid = reader.GetInt64(2);
                                var companyid = reader.GetInt64(3);
                                var id = reader.GetInt64(4);
                                var sequence = SequenceManager.Instance(Context.OrganizationID).Get(sequencetype,
                                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisereturnno ? locationid : 0,
                                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany ? companyid : 0);
                                var returnno = sequence.GetNextNumber(Context, conn, transaction);
                                param["id"] = id;
                                param["returnno"] = returnno;
                                param["returndate"] = DateTimeNow.Date;
                                param["modifiedby"] = Context.UserID;
                                param["modifiedon"] = DateTimeNow;
                                var rowsaffected = ExecuteQuery(@"Update Lrinvoicereturn set returnno=@returnno,returndate=@returndate,
                                version=1000,modifiedon=@modifiedon,modifiedby=@modifiedby where id = @id and version<1000", param, conn, transaction);
                                if (rowsaffected < 0)
                                    rollback = true;
                            }

                            if (rollback)
                            {
                                transaction.Rollback();
                                conn.Close();
                                throw new Exception("Once debit note number released can't change again");
                            }
                        }
                        else
                        {
                            using (var reader = ExecuteReader(
                                string.Format("select hasapprove,id from Lrinvoicereturn where id in ({0})", item.ids), param))
                            {
                                if (reader.Read() && (reader.IsDBNull(0) || !reader.GetBoolean(0)))
                                {
                                    throw new Exception("Once approved can't be rollbacked");
                                }
                            }
                        }
                    }

                    NpgsqlCommand command = GetCommand(
                        string.Format("Update Lrinvoicereturn set approvedby=@approvedby,approvedon=@approvedon,hasapprove=@hasapprove where id in ({0})", item.ids),
                        conn, transaction);
                    command.Parameters.Add("hasapprove", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.hasapprove;
                    command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return successCount > 0;
        }


        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrinvoicereturn oLrinvoicereturn)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrinvoicereturn.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrinvoicereturn.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrinvoicereturn.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }


        internal GSTPortal.EWAYbillResponse GenerateEWay(LrinvoicereturnCreteria item)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            var response = gst_InboxDataService.CreateEWay4DebitNote(new GST_Invoice()
            {
                lrinvoicereturnid = item.id,
                transportid = item.transportid,
                hsn4digit = item.hsn4digit,
                modeofdelivery = item.modeofdelivery
            });

            if (response.ErrorDetails == null || response.ErrorDetails.Count == 0)
            {
                if (response.ewayBillNo > 0)
                {
                    string query = @"update Lrinvoicereturn set ewayjson=@ewayjson,eway=@eway,haseway=true,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        Update(transaction, conn, item);
                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        command.Parameters.Add("eway", NpgsqlTypes.NpgsqlDbType.Varchar).Value = response.ewayBillNo.ToString("F0");
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            return response;
        }
        internal GSTPortal.EInvoiceResponse GenerateEInvoice(LrinvoicereturnCreteria item)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            var response = gst_InboxDataService.CreateEInvoice4DebitNote(new GST_Invoice()
            {
                billid = item.id,
                billinboxid = item.id,
                transportid = item.transportid,
                hsn4digit = item.hsn4digit
            });

            if (response.ErrorDetails == null || response.ErrorDetails.Count == 0)
            {
                if (!string.IsNullOrEmpty(response.Irn))
                {
                    string query = @"update Lrinvoicereturn set einvoicejson=@einvoicejson,einvoice=@einvoice,haseinvoice=true,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();

                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.Add("einvoicejson", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = response.Irn;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.ExecuteNonQuery();

                        if (!string.IsNullOrEmpty(response.EwbNo))
                        {
                            query = @"update Lrinvoicereturn set ewayjson=@ewayjson,eway=@eway,haseway=true,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
                            command = new NpgsqlCommand(query, conn);
                            command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            command.Parameters.Add("eway", NpgsqlTypes.NpgsqlDbType.Varchar).Value = response.EwbNo;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            return response;
        }

        internal Lrinvoicereturn CreateDB_CRNotes(LrinvoicereturnAuto item)
        {
            int returnTypes = 0;
            string remark = "";
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            lrinvoiceDataService.Context = this.Context;
            var invoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
            {
                id = item.id
            });
            if (item.crnotevalue > 0)
            {
                if (item.qtyadded > 0)
                {
                    remark = "Excess Qty Bill @ " + invoice[0].invoiceno;
                    returnTypes = 11;
                }
                else
                {
                    returnTypes = 12;
                    remark = "Lower Price Bill @ " + invoice[0].invoiceno;
                }
            }
            else
            {
                if (item.qtyremoved > 0)
                {
                    returnTypes = 0;
                }
                else
                {
                    returnTypes = 5;
                }
                if (item.valdec > 0)
                {
                    remark = "Higher Price Bill @ " + invoice[0].invoiceno;
                }
                else
                {
                    remark = "Shortage Qty Bill @ " + invoice[0].invoiceno;
                }
            }

            LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
            lrentryDataService.Context = this.Context;
            var lrentry = lrentryDataService.Search(new LrentryCreteria()
            {
                id = invoice[0].lrid
            });

            string query = @"select returndate,lrinvoicereturn.returnno,lrinvoicereturn.id from lrinvoicereturnitems
                        inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid
                        inner join lrinvoiceitems on lrinvoiceitems.id = lrinvoicereturnitems.lrinvoiceitemsid and lrinvoiceitems.isactive=true
                        inner join lrinvoice on lrinvoice.id = lrinvoiceitems.lrinvoiceid  and lrinvoice.isactive = true
                        where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true  
                        and lrinvoiceitems.lrinvoiceid=@lrinvoiceid and coalesce(returntype,0)=@returntype
                        order by returndate desc
                        limit 1";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("lrinvoiceid", item.id);
            param.Add("returntype", (int)returnTypes);
            long lrinvoicereturnid = 0;
            DateTime returndate = DateTime.MinValue;
            string returnno = "";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read() && DateTimeNow.Subtract(reader.GetDateTime(0)).TotalDays < 7)
                {
                    returndate = reader.GetDateTime(0);
                    returnno = reader.GetString(1);
                    lrinvoicereturnid = reader.GetInt64(2);
                }
            }

            if (lrinvoicereturnid > 0)
            {
                if (string.IsNullOrEmpty(item.overridemode))
                {
                    return new Lrinvoicereturn()
                    {
                        errormessage = "Already contains a Debit/Credit Note (" + returnno + ") On " + returndate.ToString("dd-MM-yyyy"),
                        id = -1
                    };
                }
                else if (item.overridemode == "Create")
                {
                    lrinvoicereturnid = 0;
                }
            }

            List<Lrinvoicereturnitems> lstlrinvoicereturnitems = new List<Lrinvoicereturnitems>();
            foreach (var itm in invoice[0].Lrinvoiceitems)
            {
                if (!item.lrinvoicereturnitems.Exists(x => x.id == itm.id)) continue;

                var ritm = item.lrinvoicereturnitems.Find(x => x.id == itm.id);
                Lrinvoicereturnitems lrinvoicereturnitems = new Lrinvoicereturnitems();
                lstlrinvoicereturnitems.Add(lrinvoicereturnitems);
                lrinvoicereturnitems.createdby = Context.UserID;
                lrinvoicereturnitems.createdon = DateTimeNow;
                lrinvoicereturnitems.designid = itm.designid;
                lrinvoicereturnitems.hsncode = itm.hsncode;
                lrinvoicereturnitems.uan = itm.uan;
                lrinvoicereturnitems.invoicedate = invoice.First().invoicedate;
                lrinvoicereturnitems.invoiceno = invoice.First().invoiceno;
                lrinvoicereturnitems.isactive = true;
                lrinvoicereturnitems.itemid = itm.itemid;
                lrinvoicereturnitems.lrinvoicereturnid = lrinvoicereturnid;
                lrinvoicereturnitems.productid = itm.productid;
                lrinvoicereturnitems.taxid = itm.taxid;
                lrinvoicereturnitems.taxpercentage = itm.purchasetaxpercentage;
                lrinvoicereturnitems.version = 1;
                lrinvoicereturnitems.lrinvoiceitemsid = ritm.id;
                lrinvoicereturnitems.barcode = itm.barcodeList.Count == 1 ? itm.barcodeList.First() : "";
                if (lrinvoicereturnitems.barcode.IndexOf('-') > 0)
                    lrinvoicereturnitems.barcode = "";

                if (ritm.nactualqty > itm.buyingqty)
                {
                    lrinvoicereturnitems.qty = ritm.nactualqty - itm.buyingqty;
                    lrinvoicereturnitems.rate = itm.buyingprice;
                    lrinvoicereturnitems.tax = (itm.purchasetax / itm.buyingqty) * lrinvoicereturnitems.qty;
                    lrinvoicereturnitems.amount = (lrinvoicereturnitems.qty * lrinvoicereturnitems.rate);
                }
                else if (ritm.nactualqty < itm.buyingqty)
                {
                    lrinvoicereturnitems.qty = itm.buyingqty - ritm.nactualqty;
                    lrinvoicereturnitems.rate = itm.buyingprice;
                    lrinvoicereturnitems.tax = (itm.purchasetax / itm.buyingqty) * lrinvoicereturnitems.qty;
                    lrinvoicereturnitems.amount = (lrinvoicereturnitems.qty * lrinvoicereturnitems.rate);
                }
                else
                {
                    lrinvoicereturnitems.qty = itm.buyingqty;
                    if (itm.buyingprice > ritm.nbuyingprice)
                    {
                        lrinvoicereturnitems.rate = ritm.nbuyingprice - itm.buyingprice;
                        lrinvoicereturnitems.tax = Math.Round((lrinvoicereturnitems.rate * lrinvoicereturnitems.qty) * lrinvoicereturnitems.taxpercentage / 100, 4);
                        lrinvoicereturnitems.amount = (lrinvoicereturnitems.qty * lrinvoicereturnitems.rate);
                    }
                    else
                    {
                        lrinvoicereturnitems.rate = itm.buyingprice - ritm.nbuyingprice;
                        lrinvoicereturnitems.tax = Math.Round((lrinvoicereturnitems.rate * lrinvoicereturnitems.qty) * lrinvoicereturnitems.taxpercentage / 100, 4);
                        lrinvoicereturnitems.amount = (lrinvoicereturnitems.qty * lrinvoicereturnitems.rate);
                    }
                }
            }
            var lrinvoicereturnDataService = new LrinvoicereturnDataService() { Context = this.Context };
            if (lrinvoicereturnid > 0)
            {
                Lrinvoicereturn lrinvoicereturn = lrinvoicereturnDataService.Search(new LrinvoicereturnCreteria()
                {
                    id = lrinvoicereturnid
                }).First();
                if (item.overridemode == "Override")
                {
                    lrinvoicereturn.Lrinvoicereturnitems.ForEach(x => x.isactive = false);
                    lrinvoicereturn.Lrinvoicereturnitems.AddRange(lstlrinvoicereturnitems);
                }
                else
                {
                    lrinvoicereturn.Lrinvoicereturnitems.AddRange(lstlrinvoicereturnitems);
                }
                lrinvoicereturn.remarks = string.IsNullOrEmpty(lrinvoicereturn.remarks) ? remark : lrinvoicereturn.remarks;
                lrinvoicereturn.tax = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.tax);
                lrinvoicereturn.discount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.discount);
                lrinvoicereturn.amount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.amount);
                lrinvoicereturn.qty = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.qty);
                lrinvoicereturn.total = Math.Round((lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount), 0);
                lrinvoicereturn.rounding = lrinvoicereturn.total - (lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount);
                lrinvoicereturnDataService.UpdateForItemReturn(lrinvoicereturn);
                return lrinvoicereturn;
            }
            else
            {
                Lrinvoicereturn lrinvoicereturn = new Lrinvoicereturn();
                lrinvoicereturn.ReturnType = (Lrinvoicereturn.ReturnTypes)returnTypes;
                lrinvoicereturn.companyid = invoice[0].companyid;
                lrinvoicereturn.isinterstatesale = invoice[0].isinterstatesale;
                lrinvoicereturn.createdby = Context.UserID;
                lrinvoicereturn.createdon = DateTimeNow;
                lrinvoicereturn.isactive = true;
                lrinvoicereturn.Lrinvoicereturnitems = lstlrinvoicereturnitems;
                lrinvoicereturn.returndate = DateTimeNow.Date;
                lrinvoicereturn.suppliercompanyid = invoice[0].suppliercompanyid;
                lrinvoicereturn.supplierid = invoice[0].supplierid;
                lrinvoicereturn.version = 1;
                lrinvoicereturn.remarks = remark;
                lrinvoicereturn.lrinvoiceid = invoice[0].id;
                lrinvoicereturn.locationid = lrentry[0].entrylocationid;
                lrinvoicereturn.remarks = string.IsNullOrEmpty(lrinvoicereturn.remarks) ? remark : lrinvoicereturn.remarks;
                lrinvoicereturn.tax = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.tax);
                lrinvoicereturn.discount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.discount);
                lrinvoicereturn.amount = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.amount);
                lrinvoicereturn.qty = lrinvoicereturn.Lrinvoicereturnitems.Sum(x => x.qty);
                lrinvoicereturn.total = Math.Round((lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount), 0);
                lrinvoicereturn.rounding = lrinvoicereturn.total - (lrinvoicereturn.amount + lrinvoicereturn.tax - lrinvoicereturn.discount);
                lrinvoicereturnDataService.Insert(lrinvoicereturn);
                return lrinvoicereturn;
            }

        }

        internal Lrinvoicereturn AddRefund(Voucher oVoucher, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Lrinvoicereturn oLrinvoicereturn = new Lrinvoicereturn();
            var advanceledgers = oVoucher.voucheritems.FindAll(e => e.attributes != null && e.attributes.supplierid > 0);
            if (advanceledgers.Count > 1)
            {
                foreach (var voucheritem in advanceledgers)
                {
                    oLrinvoicereturn.supplierid = voucheritem.attributes.supplierid;
                    using (var reader = ExecuteReader("select id from suppliercompany where isactive = true and supplierid=" + oLrinvoicereturn.supplierid, new SortedList<string, object>()))
                    {
                        if (reader.Read())
                            oLrinvoicereturn.suppliercompanyid = reader.GetInt64(0);
                    }
                    oLrinvoicereturn.companyid = oVoucher.companyid;
                    oLrinvoicereturn.amount = Math.Abs(voucheritem.amount) * -1;
                    oLrinvoicereturn.isactive = true;
                    oLrinvoicereturn.version = 1;
                    oLrinvoicereturn.modifiedby = Context.UserID;
                    oLrinvoicereturn.createdby = Context.UserID;
                    oLrinvoicereturn.modifiedon = DateTimeNow;
                    oLrinvoicereturn.createdon = DateTimeNow;
                    oLrinvoicereturn.locationid = oVoucher.locationid;
                    oLrinvoicereturn.returndate = oVoucher.voucherdate;
                    oLrinvoicereturn.ReturnType = Lrinvoicereturn.ReturnTypes.SupplierAdvanceRefund;
                    oLrinvoicereturn.total = Math.Abs(voucheritem.amount) * -1;
                    InsertReturn(transaction, conn, oLrinvoicereturn);
                    if (voucheritem.attributes == null)
                        voucheritem.attributes = new Voucheritems.voucharattributes();
                    voucheritem.attributes.lrinvoicereturnid = oLrinvoicereturn.id;
                }
            }
            else if (advanceledgers.Count == 1)
            {
                var voucheritem = oVoucher.voucheritems.Find(e => e.attributes != null && e.attributes.supplierid > 0);
                oLrinvoicereturn.supplierid = voucheritem.attributes.supplierid;
                using (var reader = ExecuteReader("select id from suppliercompany where isactive = true and supplierid=" + oLrinvoicereturn.supplierid, new SortedList<string, object>()))
                {
                    if (reader.Read())
                        oLrinvoicereturn.suppliercompanyid = reader.GetInt64(0);
                }
                oLrinvoicereturn.companyid = oVoucher.companyid;
                oLrinvoicereturn.amount = Math.Abs(voucheritem.amount) * -1;
                oLrinvoicereturn.isactive = true;
                oLrinvoicereturn.version = 1;
                oLrinvoicereturn.modifiedby = Context.UserID;
                oLrinvoicereturn.createdby = Context.UserID;
                oLrinvoicereturn.modifiedon = DateTimeNow;
                oLrinvoicereturn.createdon = DateTimeNow;
                oLrinvoicereturn.locationid = oVoucher.locationid;
                oLrinvoicereturn.returndate = oVoucher.voucherdate;
                oLrinvoicereturn.ReturnType = Lrinvoicereturn.ReturnTypes.SupplierAdvanceRefund;
                oLrinvoicereturn.total = Math.Abs(voucheritem.amount) * -1;
                InsertReturn(transaction, conn, oLrinvoicereturn);
                if (voucheritem.attributes == null)
                    voucheritem.attributes = new Voucheritems.voucharattributes();
                voucheritem.attributes.lrinvoicereturnid = oLrinvoicereturn.id;
            }
            return oLrinvoicereturn;
        }
        internal bool RemoveOpening(Voucheritembillwise item, long supplierid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete.Replace("and version=@version", ""), conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.lrinvoicereturnid;
            return command.ExecuteNonQuery() > 0;
        }
        internal Lrinvoicereturn AddOpening(Voucheritembillwise item, long companyid, long supplierid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            Lrinvoicereturn oLrinvoicereturn = new Lrinvoicereturn();
            using (var reader = ExecuteReader("select id from suppliercompany where isactive = true and supplierid=" + supplierid, new SortedList<string, object>()))
            {
                if (reader.Read())
                    oLrinvoicereturn.suppliercompanyid = reader.GetInt64(0);
            }
            oLrinvoicereturn.companyid = 0;

            if (item.receivable > 0)
                oLrinvoicereturn.amount = Math.Abs(item.receivable) * -1;
            else
                oLrinvoicereturn.amount = Math.Abs(item.payable);
            oLrinvoicereturn.isactive = true;
            oLrinvoicereturn.version = 1;
            oLrinvoicereturn.companyid = companyid;
            oLrinvoicereturn.modifiedby = Context.UserID;
            oLrinvoicereturn.createdby = Context.UserID;
            oLrinvoicereturn.modifiedon = DateTimeNow;
            oLrinvoicereturn.createdon = DateTimeNow;
            oLrinvoicereturn.returndate = item.referenceinvoicedate;
            oLrinvoicereturn.ReturnType = Lrinvoicereturn.ReturnTypes.SupplierAdvanceRefund;
            oLrinvoicereturn.total = oLrinvoicereturn.amount;
            InsertReturn(transaction, conn, oLrinvoicereturn);
            return oLrinvoicereturn;
        }
    }

}
