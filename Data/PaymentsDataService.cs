using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class PaymentsDataService : SAASData
    {
        const string SQL_Search = @"select id, curstomerid, employeeid, paymenton, paidamount, paymentinfo, paymentmode, isactive, createdby, modifiedby, version, createdon, modifiedon, billinformation, emiinformation, emiapprovedby, notes, code from payments where id=@id";


        const string SQL_Insert = @"INSERT INTO public.payments(curstomerid, employeeid, paymenton, paidamount, paymentinfo, paymentmode, isactive, createdby, modifiedby, version, createdon, modifiedon, billinformation, emiinformation, emiapprovedby, notes, code) VALUES(@curstomerid, @employeeid, @paymenton, @paidamount, @paymentinfo, @paymentmode, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @billinformation, @emiinformation, @emiapprovedby, @notes, @code) RETURNING id";


        const string SQL_Update = @"UPDATE public.payments SET curstomerid=@curstomerid, employeeid=@employeeid, paymenton=@paymenton, paidamount=@paidamount, paymentinfo=@paymentinfo, paymentmode=@paymentmode, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, billinformation=@billinformation, emiinformation=@emiinformation, emiapprovedby=@emiapprovedby, notes=@notes, code=@code where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.payments set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Payments> Search(PaymentsCreteria _payments)
        {
            List<Payments> lst = new List<Payments>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_payments.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _payments.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_payments.Code))
                    //    qb.AddParameter("code", _payments.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Payments opayments = new Payments();
                        opayments.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opayments.curstomerid = reader["curstomerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["curstomerid"]);
                        opayments.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        opayments.paymenton = reader["paymenton"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymenton"]);
                        opayments.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        opayments.paymentinfo_json = reader["paymentinfo"] == DBNull.Value ? "null" : reader["paymentinfo"].ToString();
                        opayments.paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"]);
                        opayments.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opayments.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opayments.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opayments.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opayments.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opayments.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opayments.billinformation_json = reader["billinformation"] == DBNull.Value ? "null" : reader["billinformation"].ToString();
                        opayments.emiinformation_json = reader["emiinformation"] == DBNull.Value ? "null" : reader["emiinformation"].ToString();
                        opayments.emiapprovedby = reader["emiapprovedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["emiapprovedby"]);
                        opayments.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        opayments.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        lst.Add(opayments);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _payments);
            }
            return lst;
        }

        public Payments Insert(Payments oPayments)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("payments.code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oPayments.code = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oPayments);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPayments);


            }

            return oPayments;
        }

        public Payments Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Payments oPayments)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("curstomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.curstomerid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.employeeid;
            command.Parameters.Add("paymenton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPayments.paymenton;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPayments.paidamount;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oPayments.paymentinfo_json == null ? "null" : oPayments.paymentinfo_json;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPayments.paymentmode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPayments.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPayments.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billinformation", NpgsqlTypes.NpgsqlDbType.Json).Value = oPayments.billinformation_json == null ? "null" : oPayments.billinformation_json;
            command.Parameters.Add("emiinformation", NpgsqlTypes.NpgsqlDbType.Json).Value = oPayments.emiinformation_json == null ? "null" : oPayments.emiinformation_json;
            command.Parameters.Add("emiapprovedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.emiapprovedby;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPayments.notes == null ? "" : oPayments.notes;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPayments.code == null ? "" : oPayments.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPayments.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oPayments;
        }

        public Payments Update(Payments oPayments)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPayments);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPayments);


            }

            return oPayments;
        }

        public Payments Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Payments oPayments)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.id;
            command.Parameters.Add("curstomerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.curstomerid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.employeeid;
            command.Parameters.Add("paymenton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPayments.paymenton;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPayments.paidamount;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oPayments.paymentinfo_json == null ? "null" : oPayments.paymentinfo_json;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPayments.paymentmode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPayments.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPayments.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPayments.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billinformation", NpgsqlTypes.NpgsqlDbType.Json).Value = oPayments.billinformation_json == null ? "null" : oPayments.billinformation_json;
            command.Parameters.Add("emiinformation", NpgsqlTypes.NpgsqlDbType.Json).Value = oPayments.emiinformation_json == null ? "null" : oPayments.emiinformation_json;
            command.Parameters.Add("emiapprovedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.emiapprovedby;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPayments.notes == null ? "" : oPayments.notes;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPayments.code == null ? "" : oPayments.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPayments.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oPayments;
        }


        public bool Delete(Payments oPayments)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPayments);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPayments);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Payments oPayments)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPayments.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPayments.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPayments.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
