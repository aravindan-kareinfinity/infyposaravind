using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ExpensesDataService : SAASData
    {
        const string SQL_Search = @"select isactive, createdby, modifiedby, version, id, createdon, modifiedon,expensestypeid,vendortypeid, amount, modeofpaymentid, note, bankname, expensesdate from expenses where id=@id";


        const string SQL_Insert = @"INSERT INTO public.expenses(isactive, createdby, modifiedby, version, createdon, modifiedon, expensestypeid, vendortypeid, amount, modeofpaymentid, note, bankname, expensesdate) 
                VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @expensestypeid, @vendortypeid, @amount, @modeofpaymentid, @note, @bankname, @expensesdate) RETURNING id";
                                                                                                                                                                                                                                      

        const string SQL_Update = @"UPDATE public.expenses SET  modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,expensestypeid=@expensestypeid,vendortypeid=@vendortypeid,amount=@amount,modeofpaymentid=@modeofpaymentid,note=@note, bankname=@bankname, expensesdate=@expensesdate  where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.expenses set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Expenses> Search(ExpensesCreteria _Expenses)
        {
            List<Expenses> lst = new List<Expenses>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_Expenses.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Expenses.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_Expenses.expensestypeid > 0)
                    {
                        qb.AddParameter("expensestypeid", _Expenses.expensestypeid, "=");
                    }

                    if (_Expenses.vendortypeid > 0)
                        qb.AddParameter("vendortypeid", _Expenses.vendortypeid, "=");

                    if (_Expenses.expensesdate.Year > 1900)
                        qb.AddParameter("expensesdate", _Expenses.expensesdate.Date, ">=");

                    qb.SortField = "id desc";

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Expenses oExpenses = new Expenses();
                        oExpenses.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oExpenses.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oExpenses.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oExpenses.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oExpenses.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oExpenses.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oExpenses.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oExpenses.expensestypeid = reader["expensestypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["expensestypeid"]);
                        oExpenses.vendortypeid = reader["vendortypeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["vendortypeid"]);
                        oExpenses.modeofpaymentid = reader["modeofpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modeofpaymentid"]);
                        oExpenses.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oExpenses.note = reader["note"] == DBNull.Value ? "" : reader["note"].ToString();
                        oExpenses.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                        oExpenses.expensesdate = reader["expensesdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expensesdate"].ToString());
                        lst.Add(oExpenses);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Expenses);
            }
            return lst;
        }

        public Expenses Insert(Expenses oExpenses)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                   

                    Insert(transaction, conn, oExpenses);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oExpenses);
                
                    
            }

            return oExpenses;
        }

        public Expenses Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Expenses oExpenses)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
           
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oExpenses.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.modifiedon;
            command.Parameters.Add("expensestypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.expensestypeid;
            command.Parameters.Add("vendortypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.vendortypeid;
            command.Parameters.Add("modeofpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.modeofpaymentid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oExpenses.amount;
            command.Parameters.Add("note", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oExpenses.note == null ? "" : oExpenses.note;
            command.Parameters.Add("bankname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oExpenses.bankname == null ? "" : oExpenses.bankname;
            command.Parameters.Add("expensesdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.expensesdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oExpenses.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oExpenses;
        }

        public Expenses Update(Expenses oExpenses)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oExpenses);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oExpenses);
                
                    
            }

            return oExpenses;
        }

        public Expenses Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Expenses oExpenses)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oExpenses.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.modifiedon;
            command.Parameters.Add("expensestypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.expensestypeid;
            command.Parameters.Add("vendortypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.vendortypeid;
            command.Parameters.Add("modeofpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.modeofpaymentid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oExpenses.amount;
            command.Parameters.Add("note", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oExpenses.note == null ? "" : oExpenses.note;
            command.Parameters.Add("bankname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oExpenses.bankname == null ? "" : oExpenses.bankname;
            command.Parameters.Add("expensesdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.expensesdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oExpenses.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oExpenses;
        }


        public bool Delete(Expenses oExpenses)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oExpenses);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oExpenses);

                
                    
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Expenses oExpenses)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oExpenses.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oExpenses.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExpenses.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
