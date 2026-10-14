using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgemployeejobworkDataService : SAASData
    {
        const string SQL_Search = @"select id, employeeid, issedon, receivedon, issedby, recievedby, issuedqty, receivedqty, returnqty, designid, designname, colourid, rate, paidamount, paidon, statusid, mfgjobworkconfigid, isactive, createdby, modifiedby, version, createdon, modifiedon from mfgemployeejobwork where id=@id";


        const string SQL_Insert = @"INSERT INTO public.mfgemployeejobwork(employeeid, issedon, receivedon, issedby, recievedby, issuedqty, receivedqty, returnqty, designid, designname, colourid, rate, paidamount, paidon, statusid, mfgjobworkconfigid, isactive, createdby, modifiedby, version, createdon, modifiedon) VALUES(@employeeid, @issedon, @receivedon, @issedby, @recievedby, @issuedqty, @receivedqty, @returnqty, @designid, @designname, @colourid, @rate, @paidamount, @paidon, @statusid, @mfgjobworkconfigid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfgemployeejobwork SET employeeid=@employeeid, issedon=@issedon, receivedon=@receivedon, issedby=@issedby, recievedby=@recievedby, issuedqty=@issuedqty, receivedqty=@receivedqty, returnqty=@returnqty, designid=@designid, designname=@designname, colourid=@colourid, rate=@rate, paidamount=@paidamount, paidon=@paidon, statusid=@statusid, mfgjobworkconfigid=@mfgjobworkconfigid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgemployeejobwork set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgemployeejobwork> Search(MfgemployeejobworkCreteria _mfgemployeejobwork)
        {
            List<Mfgemployeejobwork> lst = new List<Mfgemployeejobwork>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgemployeejobwork.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgemployeejobwork.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgemployeejobwork.Code))
                    //    qb.AddParameter("code", _mfgemployeejobwork.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgemployeejobwork omfgemployeejobwork = new Mfgemployeejobwork();
                        omfgemployeejobwork.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgemployeejobwork.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgemployeejobwork.issedon = reader["issedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issedon"]);
                        omfgemployeejobwork.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfgemployeejobwork.issedby = reader["issedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issedby"]);
                        omfgemployeejobwork.recievedby = reader["recievedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["recievedby"]);
                        omfgemployeejobwork.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        omfgemployeejobwork.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        omfgemployeejobwork.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        omfgemployeejobwork.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgemployeejobwork.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        omfgemployeejobwork.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgemployeejobwork.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        omfgemployeejobwork.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        omfgemployeejobwork.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                        omfgemployeejobwork.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                        omfgemployeejobwork.mfgjobworkconfigid = reader["mfgjobworkconfigid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjobworkconfigid"]);
                        omfgemployeejobwork.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgemployeejobwork.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgemployeejobwork.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgemployeejobwork.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgemployeejobwork.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgemployeejobwork.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(omfgemployeejobwork);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgemployeejobwork);
            }
            return lst;
        }

        public Mfgemployeejobwork Insert(Mfgemployeejobwork oMfgemployeejobwork)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgemployeejobwork);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgemployeejobwork);
                throw exp;
            }

            return oMfgemployeejobwork;
        }

        public Mfgemployeejobwork Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgemployeejobwork oMfgemployeejobwork)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.employeeid;
            command.Parameters.Add("issedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.issedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.receivedon;
            command.Parameters.Add("issedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.issedby;
            command.Parameters.Add("recievedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.recievedby;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.issuedqty;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.receivedqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.returnqty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.designid;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgemployeejobwork.designname == null ? "" : oMfgemployeejobwork.designname;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.colourid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.rate;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.paidamount;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.paidon;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgemployeejobwork.statusid;
            command.Parameters.Add("mfgjobworkconfigid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.mfgjobworkconfigid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgemployeejobwork.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgemployeejobwork.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgemployeejobwork.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgemployeejobwork;
        }

        public Mfgemployeejobwork Update(Mfgemployeejobwork oMfgemployeejobwork)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgemployeejobwork);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgemployeejobwork);
                throw exp;
            }

            return oMfgemployeejobwork;
        }

        public Mfgemployeejobwork Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgemployeejobwork oMfgemployeejobwork)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.id;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.employeeid;
            command.Parameters.Add("issedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.issedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.receivedon;
            command.Parameters.Add("issedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.issedby;
            command.Parameters.Add("recievedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.recievedby;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.issuedqty;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.receivedqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.returnqty;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.designid;
            command.Parameters.Add("designname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgemployeejobwork.designname == null ? "" : oMfgemployeejobwork.designname;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.colourid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.rate;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgemployeejobwork.paidamount;
            command.Parameters.Add("paidon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.paidon;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgemployeejobwork.statusid;
            command.Parameters.Add("mfgjobworkconfigid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.mfgjobworkconfigid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgemployeejobwork.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgemployeejobwork.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgemployeejobwork.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgemployeejobwork;
        }


        public bool Delete(Mfgemployeejobwork oMfgemployeejobwork)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgemployeejobwork);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgemployeejobwork);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgemployeejobwork oMfgemployeejobwork)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgemployeejobwork.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgemployeejobwork.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgemployeejobwork.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgemployeejobwork.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
