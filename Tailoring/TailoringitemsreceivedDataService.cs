using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TailoringitemsreceivedDataService : BaseData
    {
        const string SQL_Search = @"select Tailoringitemsreceived.id, Tailoringitemsreceived.tailoringitemsid, Tailoringitemsreceived.receivedqty, 
Tailoringitemsreceived.receivedby,Tailoringitemsreceived.receivedfrom,Tailoringitemsreceived.inwardno, 
Tailoringitemsreceived.receivedon, 
Tailoringitemsreceived.createdon, Tailoringitemsreceived.version, Tailoringitemsreceived.isactive, Tailoringitemsreceived.modifiedby, 
Tailoringitemsreceived.createdby, Tailoringitemsreceived.modifiedon,
customername, product,orderno,entryno,locationid,Tailoringitems.total charge,sourcemeter,sourceqty,
Tailoringitems.expectedqty,Tailoringitems.tailoringorderid from Tailoringitemsreceived
inner join Tailoringitems on Tailoringitems.id = Tailoringitemsreceived.tailoringitemsid
inner join Tailoringorder on Tailoringorder.id = Tailoringitems.tailoringorderid where Tailoringitemsreceived.id=@id";

        const string SQL_SearchAll = @"select id, tailoringitemsid, receivedqty,inwardno, receivedby, receivedon, createdon, version, isactive, modifiedby, createdby, modifiedon from tailoringitemsreceived where tailoringitemsid=@tailoringitemsid";


        const string SQL_Insert = @"
UPDATE Tailoringitems SET receivedqty=receivedqty+@receivedqty,status=3 WHERE id = @tailoringitemsid;
INSERT INTO public.tailoringitemsreceived(tailoringitemsid, receivedqty,inwardno, receivedby, receivedon, createdon, version, isactive, modifiedby, createdby, modifiedon) VALUES(@tailoringitemsid, @receivedqty,@inwardno, @receivedby, @receivedon, @createdon, @version, @isactive, @modifiedby, @createdby, @modifiedon) RETURNING id";

        internal List<TailoringInwardBundle> SearchBundle(TailoringitemsreceivedCreteria item)
        {
            List<TailoringInwardBundle> lst = new List<TailoringInwardBundle>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select inwardno,Tailoringitemsreceived.receivedon::date,sum(Tailoringitemsreceived.receivedqty) receivedqty from Tailoringitemsreceived
inner join Tailoringitems on Tailoringitems.id = Tailoringitemsreceived.tailoringitemsid
inner join Tailoringorder on Tailoringorder.id = Tailoringitems.tailoringorderid ";

                qb.AddParameter("Tailoringitemsreceived.isactive", true, "=");
                if (item.companyid > 0)
                    qb.AddParameter("tailoringorder.companyid", item.companyid, "=");
                if (item.locationid > 0)
                    qb.AddParameter("tailoringorder.locationid", item.locationid, "=");
                if (item.receivedfrom > 0)
                    qb.AddParameter("Tailoringitemsreceived.receivedfrom", item.receivedfrom, "=");
                if (item.fromdate.Year > 1900)
                    qb.AddParameter("Tailoringitemsreceived.receivedon", "fromdate", item.fromdate.Date, ">=");
                if (item.todate.Year > 1900)
                    qb.AddParameter("Tailoringitemsreceived.receivedon", "todate", item.todate.Date.AddDays(1), "<");

                qb.CurrentQuery = qb.CurrentQuery + " group by inwardno,Tailoringitemsreceived.receivedon::date";
                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringInwardBundle otailoringitemsissued = new TailoringInwardBundle();
                        otailoringitemsissued.inwardno = reader["inwardno"] == DBNull.Value ? "" : Convert.ToString(reader["inwardno"]);
                        otailoringitemsissued.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringitemsissued.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        lst.Add(otailoringitemsissued);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return lst;
        }



        const string SQL_Update = @"UPDATE public.tailoringitemsreceived SET tailoringitemsid=@tailoringitemsid,inwardno=@inwardno, receivedqty=@receivedqty, receivedby=@receivedby, receivedon=@receivedon, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
UPDATE Tailoringitems SET Tailoringitems.issuedqty=Tailoringitems.issuedqty-tailoringitemsreceived.issuedqty from tailoringitemsreceived
WHERE tailoringitemsreceived.id = @id and tailoringitemsreceived.tailoringitemsid = Tailoringitems.id;
update public.tailoringitemsreceived set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"update public.tailoringitemsreceived set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where tailoringitemsid=@tailoringitemsid";
        public List<TailoringitemsreceivedDetail> Search(TailoringitemsreceivedCreteria _tailoringitemsreceived)
        {
            List<TailoringitemsreceivedDetail> lst = new List<TailoringitemsreceivedDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailoringitemsreceived.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringitemsreceived.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("tailoringitemsreceived.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_tailoringitemsreceived.inwardno))
                        qb.AddParameter("tailoringitemsreceived.inwardno", _tailoringitemsreceived.inwardno, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringitemsreceivedDetail otailoringitemsreceived = new TailoringitemsreceivedDetail();
                        otailoringitemsreceived.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitemsreceived.inwardno = reader["inwardno"] == DBNull.Value ? "" : Convert.ToString(reader["inwardno"]);
                        otailoringitemsreceived.tailoringitemsid = reader["tailoringitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringitemsid"]);
                        otailoringitemsreceived.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringitemsreceived.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        otailoringitemsreceived.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otailoringitemsreceived.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitemsreceived.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitemsreceived.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitemsreceived.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitemsreceived.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitemsreceived.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);

                        otailoringitemsreceived.customername = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]);
                        otailoringitemsreceived.product = reader["product"] == DBNull.Value ? "" : Convert.ToString(reader["product"]);
                        otailoringitemsreceived.orderno = reader["orderno"] == DBNull.Value ? "" : Convert.ToString(reader["orderno"]);
                        otailoringitemsreceived.entryno = reader["entryno"] == DBNull.Value ? "" : Convert.ToString(reader["entryno"]);
                        otailoringitemsreceived.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        otailoringitemsreceived.tailoringorderid = reader["tailoringorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringorderid"]);
                        otailoringitemsreceived.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        otailoringitemsreceived.pendingqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringitemsreceived.expectedqty = reader["expectedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expectedqty"]);
                        otailoringitemsreceived.sno = lst.Count + 1;

                        lst.Add(otailoringitemsreceived);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringitemsreceived);
            }
            return lst;
        }

        public List<Tailoringitemsreceived> SearchAll(long _tailoringitemsid)
        {
            List<Tailoringitemsreceived> lst = new List<Tailoringitemsreceived>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = new NpgsqlCommand(SQL_SearchAll, conn);
                command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringitemsid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringitemsreceived otailoringitemsreceived = new Tailoringitemsreceived();
                        otailoringitemsreceived.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringitemsreceived.inwardno = reader["inwardno"] == DBNull.Value ? "" : Convert.ToString(reader["inwardno"]);
                        otailoringitemsreceived.tailoringitemsid = reader["tailoringitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailoringitemsid"]);
                        otailoringitemsreceived.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        otailoringitemsreceived.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otailoringitemsreceived.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringitemsreceived.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringitemsreceived.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringitemsreceived.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringitemsreceived.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringitemsreceived.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(otailoringitemsreceived);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringitemsid);
            }
            return lst;
        }

        public List<TailoringitemsreceivedDetail> Insert(List<TailoringitemsreceivedDetail> oTailoringitemsreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringorder.inwardno", oTailoringitemsreceived.First().locationid);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    if (oTailoringitemsreceived.Exists(e => !string.IsNullOrEmpty(e.inwardno)))
                    {
                        var existinginward = oTailoringitemsreceived.FindAll(e => !string.IsNullOrEmpty(e.inwardno));
                        var no = existinginward.First().inwardno;
                        if (existinginward.All(e => e.inwardno == no))
                        {
                            oTailoringitemsreceived.ForEach(e => e.inwardno = no);
                        }
                        else
                        {
                            var bundleno = sequence.GetNextNumber(Context,conn, transaction);
                            oTailoringitemsreceived.FindAll(e => string.IsNullOrEmpty(e.inwardno)).ForEach(e => e.inwardno = bundleno);
                        }
                    }
                    else
                    {
                        var bundleno = sequence.GetNextNumber(Context,conn, transaction);
                        oTailoringitemsreceived.ForEach(e => e.inwardno = bundleno);
                    }

                    // To update the Order details
                    foreach (var item in oTailoringitemsreceived.GroupBy(e => new { e.tailoringorderid }))
                    {
                        UpdateOrder(transaction, conn, item.First().tailoringorderid, 3, item.Sum(e => e.receivedqty));
                    }

                    foreach (var item in oTailoringitemsreceived)
                    {
                        if (item.id <= 0)
                            Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsreceived);

            }

            return oTailoringitemsreceived;
        }

        public Tailoringitemsreceived Insert(Tailoringitemsreceived oTailoringitemsreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTailoringitemsreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsreceived);

            }

            return oTailoringitemsreceived;
        }

        public Tailoringitemsreceived Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsreceived oTailoringitemsreceived)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("inwardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsreceived.inwardno == null ? "" : oTailoringitemsreceived.inwardno;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.tailoringitemsid;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitemsreceived.receivedqty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.receivedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsreceived.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsreceived.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitemsreceived.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringitemsreceived;
        }

        public Tailoringitemsreceived Update(Tailoringitemsreceived oTailoringitemsreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTailoringitemsreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsreceived);

            }

            return oTailoringitemsreceived;
        }

        public Tailoringitemsreceived Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsreceived oTailoringitemsreceived)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.id;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.tailoringitemsid;
            command.Parameters.Add("inwardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringitemsreceived.inwardno == null ? "" : oTailoringitemsreceived.inwardno;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringitemsreceived.receivedqty;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.receivedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsreceived.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsreceived.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringitemsreceived.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTailoringitemsreceived;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Tailoringitemsreceived> list, long id)
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
                    item.tailoringitemsid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Tailoringitemsreceived oTailoringitemsreceived)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringitemsreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringitemsreceived);


            }
            return hasSuccessCount;

        }

        public bool UpdateOrder(NpgsqlTransaction transaction, NpgsqlConnection conn, long orderid, int status, decimal qty)
        {
            const string SQL_UpdateOrder = @"UPDATE public.tailoringorder SET status=@status,receivedqty=receivedqty+@qty, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id RETURNING version";

            NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateOrder, conn, transaction);
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = status;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = qty;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = orderid;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringitemsreceived oTailoringitemsreceived)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringitemsreceived.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringitemsreceived.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringitemsreceived.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringitemsreceived.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long tailoringitemsid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("tailoringitemsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = tailoringitemsid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
