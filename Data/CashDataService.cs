using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Configuration;

namespace InfyPOS.Data
{
    public class CashDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon,status,cashierid, paidbyemployeeid, receivedbyemployeid, paidon, returnedon, counterid, locationid, openingamount, closingamount, openingdinomination, closingdinomination, billno,expenses,notes, takendinomination,takenamount, balancedinomination, cardamount, upiamount from cash where id=@id";

        const string SQL_Insert = @"INSERT INTO public.cash(isactive, createdby, modifiedby, version, createdon, modifiedon,status,cashierid, paidbyemployeeid, receivedbyemployeid, paidon, returnedon, counterid, locationid, openingamount, closingamount, openingdinomination, closingdinomination, billno,expenses,notes,takendinomination,takenamount,balancedinomination, cardamount, upiamount) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@status,@cashierid, @paidbyemployeeid, @receivedbyemployeid, @paidon, @returnedon, @counterid, @locationid, @openingamount, @closingamount, @openingdinomination, @closingdinomination, @billno,@expenses,@notes,@takendinomination,@takenamount,@balancedinomination,@cardamount,@upiamount) RETURNING id";

        const string SQL_Update = @"UPDATE public.cash SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,status=@status,cashierid=@cashierid, paidbyemployeeid=@paidbyemployeeid, receivedbyemployeid=@receivedbyemployeid, paidon=@paidon, returnedon=@returnedon, counterid=@counterid, locationid=@locationid, openingamount=@openingamount, closingamount=@closingamount, openingdinomination=@openingdinomination, closingdinomination=@closingdinomination, billno=@billno,expenses=@expenses,notes=@notes,takendinomination=@takendinomination,takenamount=@takenamount,balancedinomination=@balancedinomination,cardamount=@cardamount,upiamount=@upiamount  where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.cash set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Cash> Search(CashCreteria _cash)
        {
            List<Cash> lst = new List<Cash>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_cash.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_cash.paidon > DateTime.MinValue)
                    {
                        qb.AddParameter("paidon", "paidfrom", _cash.paidon.Date, ">=");
                        qb.AddParameter("paidon", "paidto", _cash.paidon.Date.AddDays(1), "<");
                    }
                    if (_cash.locationid > 0)
                        qb.AddParameter("locationid", _cash.locationid, "=");
                    if (_cash.counterid > 0)
                        qb.AddParameter("counterid", _cash.counterid, "=");
                    if (_cash.cashierid > 0)
                        qb.AddParameter("cashierid", _cash.cashierid, "=");
                    if (_cash.paidbyemployeeid > 0)
                        qb.AddParameter("paidbyemployeeid", _cash.paidbyemployeeid, "=");
                    if (_cash.receivedbyemployeid > 0)
                        qb.AddParameter("receivedbyemployeid", _cash.receivedbyemployeid, "=");
                    if (_cash.status == 9)
                        qb.Add2Query(" closingamount=0 and status != 2 ");
                    else if (_cash.status > 0)
                        qb.AddParameter("status", _cash.status, "=");
                    if (_cash.ids != null && _cash.ids.Count > 0)
                        qb.AddInParamer("id", String.Join(",", _cash.ids));

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _cash.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Cash ocash = new Cash();
                        ocash.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocash.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocash.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocash.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocash.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        ocash.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"].ToString());
                        ocash.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocash.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocash.paidbyemployeeid = reader["paidbyemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidbyemployeeid"].ToString());
                        ocash.cashierid = reader["cashierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cashierid"].ToString());
                        ocash.receivedbyemployeid = reader["receivedbyemployeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedbyemployeid"].ToString());
                        ocash.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"].ToString());
                        ocash.returnedon = reader["returnedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["returnedon"].ToString());
                        ocash.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"].ToString());
                        ocash.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        ocash.expenses = reader["expenses"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expenses"].ToString());
                        ocash.openingamount = reader["openingamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["openingamount"].ToString());
                        ocash.closingamount = reader["closingamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closingamount"].ToString());
                        ocash.takenamount = reader["takenamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["takenamount"].ToString());
                        ocash.cardamount = reader["cardamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cardamount"].ToString());
                        ocash.upiamount = reader["upiamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["upiamount"].ToString());
                        ocash.openingdinomination_json = reader["openingdinomination"] == DBNull.Value ? "null" : reader["openingdinomination"].ToString();
                        ocash.closingdinomination_json = reader["closingdinomination"] == DBNull.Value ? "null" : reader["closingdinomination"].ToString();
                        ocash.takendinomination_json = reader["takendinomination"] == DBNull.Value ? "null" : reader["takendinomination"].ToString();
                        ocash.balancedinomination_json = reader["balancedinomination"] == DBNull.Value ? "null" : reader["balancedinomination"].ToString();
                        ocash.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        ocash.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        ocash.balanceamount = ocash.closingamount - ocash.takenamount;
                        lst.Add(ocash);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _cash);
            }
            return lst;
        }

        public Cash Insert(Cash oCash)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                if (oCash.locationid > 0 &&
                     oCash.counterid > 0 &&
                     Services.ConfigManager.Instance(Context.OrganizationID).Config.system_cashclosevalidation)
                {
                    List<Cash> lstcast = Search(new CashCreteria()
                    {
                        paidon = oCash.paidon,
                        locationid = oCash.locationid,
                        counterid = oCash.counterid,
                        status = 1
                    });

                    if (lstcast != null && lstcast.Count > 0)
                    {
                        oCash = new Cash();
                        oCash.ErrorMessage = "Already this counter have opening , please close before opening again";
                        return oCash;
                    }
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oCash);
                    if (oCash.status == 1 &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_cashierobcb)
                    {
                        string query = @"Select 'Already having cash opening - '||employee.name message,paidon from cash
                                         inner join employee on employee.id = cashierid
                                         where counterid=@counterid and status!=2 and cash.isactive=true and cash.id != @id
                                         union all
                                         Select 'Unclosed Counter '||referencelist.name message,paidon from cash
                                         inner join referencelist on referencelist.id = counterid
                                         where cashierid=@cashierid and status!=2 and cash.isactive=true and cash.id != @id;";

                        List<string> messages = new List<string>();
                        var param = new SortedList<string, object>();
                        param.Add("counterid", oCash.counterid);
                        param.Add("cashierid", oCash.cashierid);
                        param.Add("id", oCash.id);
                        using (var reader = ExecuteReader(query, param, conn, transaction))
                        {
                            while (reader.Read())
                            {
                                messages.Add(reader.GetString(0) + " - " + reader.GetDateTime(1).ToString("dd-MM-yyyy"));
                            }
                        }
                        if (messages.Count > 0)
                        {
                            throw new Exception(string.Join("\r\n", messages));
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCash);
            }
            return oCash;
        }

        public Cash Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Cash oCash)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCash.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCash.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCash.status;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("paidbyemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.paidbyemployeeid;
            command.Parameters.Add("receivedbyemployeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.receivedbyemployeid;
            command.Parameters.Add("cashierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.cashierid;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCash.paidon;
            command.Parameters.Add("returnedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCash.returnedon;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.counterid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.locationid;
            command.Parameters.Add("openingamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.openingamount;
            command.Parameters.Add("closingamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.closingamount;
            command.Parameters.Add("takenamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.takenamount;
            command.Parameters.Add("cardamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.cardamount;
            command.Parameters.Add("upiamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.upiamount;
            command.Parameters.Add("openingdinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.openingdinomination == null ? "null" : oCash.openingdinomination_json;
            command.Parameters.Add("closingdinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.closingdinomination == null ? "null" : oCash.closingdinomination_json;
            command.Parameters.Add("takendinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.takendinomination == null ? "null" : oCash.takendinomination_json;
            command.Parameters.Add("balancedinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.balancedinomination == null ? "null" : oCash.balancedinomination_json;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = (oCash.billno == null) ? "" : oCash.billno;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = (oCash.notes == null) ? "" : oCash.notes;
            command.Parameters.Add("expenses", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.expenses;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCash.id = Convert.ToInt64(reader[0]);
                }
            }
            return oCash;
        }

        public Cash Update(Cash oCash)
        {
            if (oCash.locationid > 0 && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_cashclosevalidation)
            {
                RetailReportDataService retailreport = new RetailReportDataService() { Context = this.Context };
                List<RetailReport> lstUnsettled = retailreport.SearchSales(new SaleRetailCreteria()
                {
                    fromdate = oCash.paidon,
                    todate = oCash.paidon,
                    locationid = oCash.locationid,
                    billtype = 0,
                    mode = "6"
                });

                if (lstUnsettled != null && lstUnsettled.Count > 0)
                {
                    List<Cash> lstcast = Search(new CashCreteria()
                    {
                        paidon = oCash.paidon,
                        locationid = oCash.locationid,
                        status = 1
                    });

                    if (lstcast != null && lstcast.Count <= 1)
                    {
                        oCash = new Cash();
                        oCash.ErrorMessage = "Please clear all the un-settled bills before closing the cash counter.";
                        return oCash;
                    }
                }
            }

            if (oCash.closingdinomination != null && oCash.takendinomination != null)
            {
                oCash.balancedinomination = new List<Dinomination>();
                foreach (var closingitem in oCash.closingdinomination)
                {
                    if (oCash.takendinomination.Exists(e => e.dinomination == closingitem.dinomination))
                    {
                        var takenitem = oCash.takendinomination.Find(e => e.dinomination == closingitem.dinomination);
                        oCash.balancedinomination.Add(new Dinomination()
                        {
                            dinomination = closingitem.dinomination,
                            qty = closingitem.qty - takenitem.qty,
                            amount = closingitem.amount - takenitem.amount
                        });
                    }
                }
                oCash.balanceamount = oCash.closingamount - oCash.takenamount;
            }

            if (oCash.status == 2)
            {
                RetailReportDataService RetailReportData = new RetailReportDataService() { Context = this.Context };
                List<SettlementReport> result = RetailReportData.SearchSettlementForReconcile(new RetailReportCreteria()
                {
                    locationid = oCash.locationid,
                    counterid = oCash.counterid,
                    cashierid = oCash.cashierid,
                    fromdate = oCash.paidon.Date,
                    todate = oCash.paidon.Date
                });
                oCash.banktransactions = new List<BankSettlement>();
                if (result.Count == 1)
                {
                    if (result[0].lstcarddetails != null)
                    {
                        result[0].lstcarddetails.ForEach(x =>
                        {
                            oCash.banktransactions.Add(new BankSettlement()
                            {
                                type = "CARD",
                                bankid = x.bankid,
                                amount = x.amount
                            });
                        });
                    }

                    if (result[0].lstupidetails != null)
                    {
                        result[0].lstupidetails.ForEach(x =>
                        {
                            oCash.banktransactions.Add(new BankSettlement()
                            {
                                type = "UPI",
                                bankid = x.bankid,
                                amount = x.amount
                            });
                        });
                    }
                }
            }

            NpgsqlTransaction transaction = null;
            try
            {
                var opening = Context.HasPermission(PermissionSet.cashopening, InfyPOS.Common.Action.Modify);
                var closing = Context.HasPermission(PermissionSet.cashclosing, InfyPOS.Common.Action.Modify);

                bool isClosed = Search(new CashCreteria()
                {
                    id = oCash.id
                }).Exists(e => e.closingdinomination != null &&
                e.closingdinomination.Count > 0);

                if (isClosed && !closing)
                {
                    oCash = new Cash();
                    oCash.ErrorMessage = "Don't have permission to modify closing balance.";
                    return oCash;
                }
                if (isClosed && !opening)
                {
                    oCash = new Cash();
                    oCash.ErrorMessage = "Don't have permission to modify opening balance.";
                    return oCash;
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oCash);

                    if (oCash.status == 1 &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_cashierobcb)
                    {
                        string query = @"Select 'Already having cash opening - '||employee.name message,paidon from cash where counterid=@counterid and status!=2 and isactive=true
                                         inner join employee on employee.id = cashierid
                                         where cash.id != @id
                                         union all
                                         Select 'Unclosed Counter '||referencelist.name message,paidon from cash where cashierid=@cashierid and status!=2 and isactive=true
                                         inner join referencelist on referencelist.id = counterid
                                         where cash.id != @id;";

                        List<string> messages = new List<string>();
                        var param = new SortedList<string, object>();
                        param.Add("counterid", oCash.counterid);
                        param.Add("cashierid", oCash.cashierid);
                        param.Add("id", oCash.id);
                        using (var reader = ExecuteReader(query, param, conn, transaction))
                        {
                            while (reader.Read())
                            {
                                messages.Add(reader.GetString(0) + " - " + reader.GetDateTime(1).ToString("dd-MM-yyyy"));
                            }
                        }
                        if (messages.Count > 0)
                        {
                            throw new Exception(string.Join("\r\n", messages));
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCash);
            }
            return oCash;
        }

        public Cash Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Cash oCash)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCash.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCash.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCash.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("paidbyemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.paidbyemployeeid;
            command.Parameters.Add("receivedbyemployeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.receivedbyemployeid;
            command.Parameters.Add("cashierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.cashierid;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCash.paidon;
            command.Parameters.Add("returnedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCash.returnedon;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.counterid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCash.status;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.locationid;
            command.Parameters.Add("openingamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.openingamount;
            command.Parameters.Add("closingamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.closingamount;
            command.Parameters.Add("takenamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.takenamount;
            command.Parameters.Add("cardamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.cardamount;
            command.Parameters.Add("upiamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.upiamount;
            command.Parameters.Add("openingdinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.openingdinomination == null ? "null" : oCash.openingdinomination_json;
            command.Parameters.Add("closingdinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.closingdinomination == null ? "null" : oCash.closingdinomination_json;
            command.Parameters.Add("takendinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.takendinomination == null ? "null" : oCash.takendinomination_json;
            command.Parameters.Add("balancedinomination", NpgsqlTypes.NpgsqlDbType.Json).Value = oCash.balancedinomination == null ? "null" : oCash.balancedinomination_json;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = (oCash.billno == null) ? "" : oCash.billno;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = (oCash.notes == null) ? "" : oCash.notes;
            command.Parameters.Add("expenses", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCash.expenses;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCash.version = Convert.ToInt32(reader[0]);
                }
            }

            return oCash;
        }


        public bool Delete(Cash oCash)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCash);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCash);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Cash oCash)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCash.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCash.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCash.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal CashPosting Post2Finance(CashPosting item)
        {
            var cashledgers = Search(new CashCreteria()
            {
                ids = item.closings
            });

            if (cashledgers.GroupBy(x => x.paidon.Date).Count() > 1)
            {
                throw new Exception("Can't post multiple days at single posting");
            }



            LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
            var ledgers = ledgerDataService.Search(new LedgerCreteria()
            {
                typeids = new List<int>(new int[] { 6, 7, 14 })
            });
            List<InfyPOS.Models.Voucher> vouchers = new List<Voucher>();
            InfyPOS.Tally.FinanceService financeService = new InfyPOS.Tally.FinanceService();
            var quantoledgers = financeService.SearchSettlementLedgers(
                cashledgers.ConvertAll(x => x.counterid),
                item.locationid,
                cashledgers[0].paidon.Date,
                cashledgers[0].paidon.Date);

            var allcashsales = quantoledgers.CashSalesList.GroupBy(x => new { x.companyid, x.locationid, x.date });
            foreach (var cashsale in allcashsales)
            {
                var cashledger = ledgers.FindAll(x => x.typeid == 6 && x.eodcashledger && x.attributes != null && x.attributes.branchids != null
                    && x.attributes.branchids.Contains(cashsale.Key.locationid));
                if (cashledger.Count == 0)
                {
                    cashledger = ledgers.FindAll(x => x.typeid == 6 && x.eodcashledger && x.attributes != null && x.attributes.branchids != null
                    && x.attributes.branchids.Contains(-999));
                    if (cashledger.Count == 0)
                    {
                        cashledger = ledgers.FindAll(x => x.typeid == 6 && x.eodcashledger);
                        if (cashledger.Count == 0)
                        {
                            throw new Exception("Please create cash account on ledger with correct retail location");
                        }
                    }
                }

                var salesledger = ledgers.FindAll(x => x.typeid == 14 && x.eodcashledger && x.attributes != null && x.attributes.branchids != null
                    && x.attributes.branchids.Contains(cashsale.Key.locationid));
                if (salesledger.Count == 0)
                {
                    salesledger = ledgers.FindAll(x => x.typeid == 14 && x.eodcashledger && x.attributes != null && x.attributes.branchids != null
                    && x.attributes.branchids.Contains(-999));
                    if (salesledger.Count == 0)
                    {
                        salesledger = ledgers.FindAll(x => x.typeid == 14 && x.eodcashledger);
                        if (salesledger.Count == 0)
                        {
                            throw new Exception("Please create sales account on ledger with correct retail location");
                        }
                        if (salesledger.Count > 1)
                        {
                            throw new Exception("Please create sales account on ledger with correct retail location");
                        }
                    }
                }


                InfyPOS.Models.Voucher voucher = new Voucher()
                {
                    companyid = cashsale.Key.companyid,
                    locationid = cashsale.Key.locationid,
                    voucherdate = cashsale.Key.date,
                    isactive = true,
                    uniqueuid = Guid.NewGuid().ToString(),
                    vouchertype = 4,
                    vouchertypeid = -1,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTime.Now,
                    modifiedon = DateTime.Now,
                    adjustmentcredit = false,
                    narration = "SETTLEMENT CASH POSTING - " + cashsale.Key.date.ToString("dd-MM-yyyy") + " - " + cashsale.First().locationname,
                    sourcetype = 3,
                    adjustmentledgerid = cashledger[0].id,
                    amount = cashsale.Sum(x => x.amount - x.cash_refund),
                    voucheritems = new List<Voucheritems>()
                };

                InfyPOS.Models.Voucheritems voucheritems = new InfyPOS.Models.Voucheritems()
                {
                    id = 0,
                    amount = cashsale.Sum(x => x.amount - x.cash_refund),
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTime.Now,
                    modifiedon = DateTime.Now,
                    iscredit = true,
                    ledgerid = salesledger[0].id
                };
                item.cash = cashsale.Sum(x => x.amount - x.cash_refund);
                if (item.cash < 0)
                {
                    voucher.amount = Math.Abs(item.cash);
                    voucher.adjustmentcredit = true;
                    voucheritems.amount = Math.Abs(item.cash);
                    voucheritems.iscredit = false;
                }
                voucher.voucheritems.Add(voucheritems);
                vouchers.Add(voucher);
            }

            var allcardsales = quantoledgers.CardSalesList.GroupBy(x => new { x.isupi, x.companyid, x.locationid, x.bankid, x.date });
            foreach (var cardsale in allcardsales)
            {
                var bankledger = ledgers.FindAll(x => x.typeid == 7 && x.bankid == cardsale.Key.bankid);
                if (bankledger.Count == 0)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(cardsale.Key.locationid))
                    {
                        if (cardsale.Key.isupi)
                        {
                            var defaultbankid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[cardsale.Key.locationid].sale_defaultupibankid;
                            bankledger = ledgers.FindAll(x => x.typeid == 7 && x.bankid == defaultbankid);
                        }
                        else
                        {
                            var defaultbankid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[cardsale.Key.locationid].sale_defaultbankid;
                            bankledger = ledgers.FindAll(x => x.typeid == 7 && x.bankid == defaultbankid);
                        }
                        if (bankledger.Count == 0)
                        {
                            throw new Exception("Please create relavent bank account on ledger");
                        }
                    }
                    else
                    {
                        throw new Exception("Please create relavent bank account on ledger");
                    }
                }

                var salesledger = ledgers.FindAll(x => x.typeid == 14 && x.attributes != null && x.attributes.branchids != null
                    && x.attributes.branchids.Contains(cardsale.Key.locationid));
                if (salesledger.Count == 0)
                {
                    salesledger = ledgers.FindAll(x => x.typeid == 14 && x.attributes != null && x.attributes.branchids != null
                    && x.attributes.branchids.Contains(-999));
                    if (salesledger.Count == 0)
                    {
                        salesledger = ledgers.FindAll(x => x.typeid == 14);
                        if (salesledger.Count == 0)
                        {
                            throw new Exception("Please create sales account on ledger with correct retail location");
                        }
                    }
                }

                InfyPOS.Models.Voucher voucher = new Voucher()
                {
                    companyid = cardsale.Key.companyid,
                    locationid = cardsale.Key.locationid,
                    voucherdate = cardsale.Key.date,
                    uniqueuid = Guid.NewGuid().ToString(),
                    isactive = true,
                    vouchertype = 4,
                    vouchertypeid = -1,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTime.Now,
                    modifiedon = DateTime.Now,
                    adjustmentcredit = false,
                    narration = "SETTLEMENT " +
                    (cardsale.Key.isupi ? ("UPI - " + cardsale.First().description) : ("BANK - " + cardsale.First().description))
                    + " POSTING - " + cardsale.Key.date.ToString("dd-MM-yyyy") + " - " + cardsale.First().locationname,
                    sourcetype = cardsale.Key.isupi ? 5 : 4,
                    adjustmentledgerid = bankledger[0].id,
                    amount = cardsale.Sum(x => x.amount),
                    voucheritems = new List<Voucheritems>()
                };

                InfyPOS.Models.Voucheritems voucheritems = new InfyPOS.Models.Voucheritems()
                {
                    id = 0,
                    amount = cardsale.Sum(x => x.amount),
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTime.Now,
                    modifiedon = DateTime.Now,
                    iscredit = true,
                    ledgerid = salesledger[0].id
                };
                if (cardsale.Key.isupi)
                {
                    item.upi = cardsale.Sum(x => x.amount);
                    item.upicount++;
                }
                else
                {
                    item.bank = cardsale.Sum(x => x.amount);
                    item.bankcount++;
                }
                voucher.voucheritems.Add(voucheritems);
                if (vouchers.Exists(x => x.sourcetype == voucher.sourcetype &&
                x.voucherdate == voucher.voucherdate &&
                x.companyid == voucher.companyid &&
                x.locationid == voucher.locationid))
                {
                    var voucherold = vouchers.Find(x => x.sourcetype == voucher.sourcetype &&
                                    x.voucherdate == voucher.voucherdate &&
                                    x.companyid == voucher.companyid &&
                                    x.locationid == voucher.locationid);
                    voucherold.amount += voucher.amount;
                    voucherold.voucheritems[0].amount += voucher.amount;
                }
                else
                {
                    vouchers.Add(voucher);
                }
            }
            VoucherDataService voucherDataService = new VoucherDataService() { Context = this.Context };
            var existingvouchers = voucherDataService.Search(new VoucherCreteria()
            {
                cashposting = true,
                locationid = item.locationid,
                voucherdate = cashledgers[0].paidon.Date
            });


            foreach (var voucher in vouchers)
            {
                var ex = existingvouchers.FindAll(x => x.sourcetype == voucher.sourcetype &&
                x.locationid == voucher.locationid && x.companyid == voucher.companyid &&
                x.adjustmentledgerid == voucher.adjustmentledgerid);
                if (ex.Count > 0)
                {
                    if (ex[0].amount == voucher.amount)
                    {
                        existingvouchers.Remove(ex[0]);
                        continue;
                    }
                    voucherDataService.Delete(ex[0]);
                    existingvouchers.Remove(ex[0]);
                }
                voucherDataService.Insert(voucher);
            }


            foreach (var voucher in existingvouchers)
            {
                voucherDataService.Delete(voucher);
            }

            //foreach (var voucher in vouchers.GroupBy(x => new { x.locationid, x.companyid }))
            //{
            //    var voucherlist = voucher.ToList();
            //    voucherlist.Count
            //}
            return item;
        }
    }

}
