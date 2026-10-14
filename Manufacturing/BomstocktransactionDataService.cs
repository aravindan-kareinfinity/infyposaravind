using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BomstocktransactionDataService : SAASData
    {
        const string SQL_Search = @"select bomstocktransaction.id, bomstocktransaction.isactive, bomstocktransaction.createdby, bomstocktransaction.sourceid,
        bomstocktransaction.modifiedby, bomstocktransaction.version, bomstocktransaction.createdon, bomstocktransaction.modifiedon, bomstocktransaction.bomid, 
        bomstocktransaction.locationid,bomstocktransaction.companyid, bomstocktransaction.qty, bomstocktransaction.transactedon, reftype, refid,refno,colourid,
        width,otherlocationid,supplierid from bomstocktransaction
        where id=@id";

        const string SQL_Insert = @"
        UPDATE bommaster set currentstock=currentstock+@qty where id=@bomid;
        INSERT INTO public.bomstocktransaction(isactive, createdby, modifiedby, version, createdon, modifiedon, bomid, locationid, companyid, 
        qty, transactedon, reftype, refid,refno,colourid,width,sourceid,otherlocationid,supplierid) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @bomid, 
        @locationid,@companyid, @qty, @transactedon, @reftype, @refid,@refno,@colourid,@width,@sourceid,@otherlocationid,@supplierid) RETURNING id";

        const string SQL_Update = @"
        UPDATE bommaster set currentstock=currentstock-bomstocktransaction.qty from bomstocktransaction
        where bomstocktransaction.id = @id and bommaster.id = bomstocktransaction.bomid;
        UPDATE bommaster set currentstock=currentstock+@qty where id=@bomid;
        UPDATE public.bomstocktransaction SET modifiedby=@modifiedby, version=@version+1, 
        modifiedon=@modifiedon, bomid=@bomid, locationid=@locationid, qty=@qty, companyid=@companyid,
        transactedon=@transactedon, reftype=@reftype, refid=@refid,sourceid=@sourceid,otherlocationid=@otherlocationid,
        supplierid=@supplierid 
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
        UPDATE bommaster set currentstock=currentstock-bomstocktransaction.qty from bomstocktransaction
        where bomstocktransaction.id = @id and bommaster.id = bomstocktransaction.bomid;
        update public.bomstocktransaction set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"
        UPDATE bommaster set currentstock=currentstock-bomstocktransaction.qty from bomstocktransaction
        where reftype=@reftype and refid=@refid and bommaster.id = bomstocktransaction.bomid;
        update public.bomstocktransaction set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where reftype=@reftype and sourceid=@sourceid;";


        const string SQL_HardDeleteAll = @"
        UPDATE bommaster set currentstock=currentstock-bomstocktransaction.qty from bomstocktransaction
        where  bommaster.id = bomstocktransaction.bomid;
        delete from public.bomstocktransaction where reftype=@reftype and sourceid=@sourceid;";

        public List<Bomstocktransaction> Search(BomstocktransactionCreteria _bomstocktransaction)
        {
            List<Bomstocktransaction> lst = new List<Bomstocktransaction>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bomstocktransaction.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bomstocktransaction.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.AddParameter("Bomstocktransaction.transactedon", "fromdate", _bomstocktransaction.fromdate.Date, ">=");
                    qb.AddParameter("Bomstocktransaction.transactedon", "todate", _bomstocktransaction.todate.AddDays(1).Date, "<");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bomstocktransaction obomstocktransaction = new Bomstocktransaction();
                        obomstocktransaction.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obomstocktransaction.sourceid = reader["sourceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceid"]);
                        obomstocktransaction.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obomstocktransaction.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obomstocktransaction.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obomstocktransaction.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obomstocktransaction.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obomstocktransaction.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obomstocktransaction.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        obomstocktransaction.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obomstocktransaction.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obomstocktransaction.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        obomstocktransaction.otherlocationid = reader["otherlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["otherlocationid"]);
                        obomstocktransaction.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        obomstocktransaction.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        obomstocktransaction.refno = reader["refno"] == DBNull.Value ? "" : Convert.ToString(reader["refno"]);
                        obomstocktransaction.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obomstocktransaction.transactedon = reader["transactedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactedon"]);
                        obomstocktransaction.reftype = reader["reftype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["reftype"]);
                        obomstocktransaction.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        lst.Add(obomstocktransaction);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bomstocktransaction);
            }
            return lst;
        }

        public Bomstocktransaction Insert(Bomstocktransaction oBomstocktransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBomstocktransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBomstocktransaction);
                throw exp;
            }

            return oBomstocktransaction;
        }

        public Bomstocktransaction Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bomstocktransaction oBomstocktransaction)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomstocktransaction.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.modifiedon;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.bomid;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.sourceid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.companyid;
            command.Parameters.Add("otherlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.otherlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.supplierid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.colourid;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomstocktransaction.width == null ? "" : oBomstocktransaction.width;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBomstocktransaction.qty;
            command.Parameters.Add("transactedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.transactedon;
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.reftype;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.refid;
            command.Parameters.Add("refno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomstocktransaction.refno == null ? "" : oBomstocktransaction.refno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBomstocktransaction.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBomstocktransaction;
        }

        public Bomstocktransaction Update(Bomstocktransaction oBomstocktransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBomstocktransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBomstocktransaction);
                throw exp;
            }

            return oBomstocktransaction;
        }

        public Bomstocktransaction Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bomstocktransaction oBomstocktransaction)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomstocktransaction.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.modifiedon;
            command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.bomid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.locationid;
            command.Parameters.Add("otherlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.otherlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.supplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.companyid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBomstocktransaction.qty;
            command.Parameters.Add("transactedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.transactedon;
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.reftype;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.refid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.colourid;
            command.Parameters.Add("width", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomstocktransaction.width == null ? "" : oBomstocktransaction.width;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.sourceid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBomstocktransaction.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBomstocktransaction;
        }


        public bool Delete(Bomstocktransaction oBomstocktransaction)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBomstocktransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBomstocktransaction);

                throw exp;
            }
            return hasSuccessCount;

        }


        //public bool DeleteAll(Bomstocktransaction.TransactionType consumption, 
        //    NpgsqlTransaction transaction, NpgsqlConnection conn, 
        //    Bomstocktransaction oBomstocktransaction)
        //{
        //    NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
        //    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomstocktransaction.isactive;
        //    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.modifiedby;
        //    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.modifiedon;
        //    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.version;
        //    command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.refid;
        //    command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.reftype;
        //    int successCount = command.ExecuteNonQuery();

        //    return successCount > 0;
        //}

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Bomstocktransaction oBomstocktransaction)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomstocktransaction.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomstocktransaction.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomstocktransaction.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomstocktransaction.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }


        internal void Insert(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMInvoicereturn bominvoice, List<BOMInvoicereturnitems> bominvoiceitems)
        {
            var items = bominvoiceitems.ConvertAll(e => new Bomstocktransaction()
            {
                bomid = e.bomid,
                sourceid = bominvoice.id,
                colourid = e.colourid,
                qty = e.qty * -1,
                isactive = true,
                width = e.width,
                refno = bominvoice.returnno,
                transactedon = bominvoice.returndate,
                locationid = bominvoice.locationid,
                companyid = bominvoice.companyid,
                refid = e.id,
                supplierid = bominvoice.supplierid,
                RowType = rowtype,
                createdby = bominvoice.createdby,
                createdon = e.createdon,
                modifiedby = bominvoice.modifiedby,
                modifiedon = e.modifiedon,
                version = 1
            });
            foreach (var item in items)
                Insert(transaction, conn, item);
        }

        internal void Insert(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMinvoice bominvoice, List<Bominvoiceitems> bominvoiceitems)
        {
            var items = bominvoiceitems.ConvertAll(e => new Bomstocktransaction()
            {
                bomid = e.bomid,
                sourceid = bominvoice.id,
                colourid = e.colourid,
                qty = e.buyingqty,
                isactive = true,
                width = e.width,
                refno = bominvoice.invoiceentryno,
                transactedon = bominvoice.entrydate,
                locationid = bominvoice.locationid,
                companyid = bominvoice.companyid,
                refid = e.id,
                RowType = rowtype,
                createdby = bominvoice.createdby,
                createdon = e.createdon,
                modifiedby = bominvoice.modifiedby,
                modifiedon = e.modifiedon,
                version = 1
            });
            foreach (var item in items)
                Insert(transaction, conn, item);
        }

        internal void Delete(Bomstocktransaction.TransactionType purchase,
            NpgsqlTransaction transaction, NpgsqlConnection conn, long sourceid)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_HardDeleteAll, conn, transaction);
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)purchase;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sourceid;
            int successCount = command.ExecuteNonQuery();
        }

        internal void Update(Bomstocktransaction.TransactionType purchase,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMinvoice bominvoice, List<Bominvoiceitems> list)
        {
            Delete(purchase, transaction, conn, bominvoice.id);
            Insert(purchase, conn, transaction, bominvoice, list);
        }

        internal void Update(Bomstocktransaction.TransactionType purchase,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMInvoicereturn bominvoice, List<BOMInvoicereturnitems> list)
        {
            Delete(purchase, transaction, conn, bominvoice.id);
            Insert(purchase, conn, transaction, bominvoice, list);
        }
        internal void Insert(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMConsumption oBOMConsumption,
            List<BOMConsumptionitemsDetail> bominvoiceitems)
        {
            var items = bominvoiceitems.ConvertAll(e => new Bomstocktransaction()
            {
                bomid = e.bomid,
                colourid = e.colourid,
                qty = e.qty * -1,
                sourceid = oBOMConsumption.id,
                isactive = true,
                width = e.width,
                refno = oBOMConsumption.entryno,
                transactedon = oBOMConsumption.entrydate,
                locationid = oBOMConsumption.locationid,
                supplierid = oBOMConsumption.supplierid,
                companyid = oBOMConsumption.companyid,
                refid = e.id,
                RowType = rowtype,
                createdby = oBOMConsumption.createdby,
                createdon = e.createdon,
                modifiedby = oBOMConsumption.modifiedby,
                modifiedon = e.modifiedon,
                version = 1
            });
            foreach (var item in items)
                Insert(transaction, conn, item);
        }


        internal void Update(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMConsumption oBOMConsumption, List<BOMConsumptionitemsDetail> list)
        {
            Delete(rowtype, transaction, conn, oBOMConsumption.id);
            Insert(rowtype, conn, transaction, oBOMConsumption, list);
        }

        internal void Insert(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction,
            BOMBundles oBOMBundles, List<BOMBundlesitem> bundleitems)
        {
            long companyid = (rowtype == Bomstocktransaction.TransactionType.TransferOut) ? oBOMBundles.fromcompanyid : oBOMBundles.tocompanyid;
            if (companyid == 0)
                companyid = oBOMBundles.fromcompanyid;
            if (companyid == 0)
                companyid = oBOMBundles.tocompanyid;
            var items = bundleitems.ConvertAll(e => new Bomstocktransaction()
            {
                bomid = e.bomid,
                colourid = e.colourid,
                sourceid = oBOMBundles.id,
                qty = rowtype == Bomstocktransaction.TransactionType.TransferOut ? e.qty * -1 : e.aqty * 1,
                isactive = true,
                width = e.width,
                refno = oBOMBundles.code,
                transactedon = oBOMBundles.packedon,
                locationid = (rowtype == Bomstocktransaction.TransactionType.TransferOut)
                ? oBOMBundles.fromlocationid : oBOMBundles.tolocationid,
                otherlocationid = (rowtype != Bomstocktransaction.TransactionType.TransferOut)
                ? oBOMBundles.fromlocationid : oBOMBundles.tolocationid,
                companyid = companyid,
                refid = e.id,
                RowType = rowtype,
                createdby = oBOMBundles.createdby,
                createdon = oBOMBundles.createdon,
                modifiedby = oBOMBundles.modifiedby,
                modifiedon = oBOMBundles.modifiedon,
                version = 1
            });
            foreach (var item in items)
                Insert(transaction, conn, item);
        }

        internal void Insert(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction, Mfgjoborder oMfgjoborder,
            List<Mfgjoborderitems> list)
        {
            if (list == null || list.Count == 0) return;
            if (oMfgjoborder.isbomprocess)
            {
                var items = list.FindAll(x => x.oqty > 0).ConvertAll(e => new Bomstocktransaction()
                {
                    bomid = e.bomid,
                    colourid = e.colourid,
                    qty = e.oqty * -1,
                    supplierid = oMfgjoborder.supplierid,
                    sourceid = oMfgjoborder.id,
                    isactive = true,
                    width = e.width,
                    refno = oMfgjoborder.joborderno,
                    transactedon = oMfgjoborder.joborderdate,
                    locationid = oMfgjoborder.fromlocationid,
                    companyid = oMfgjoborder.companyid,
                    refid = e.id,
                    RowType = rowtype,
                    createdby = oMfgjoborder.createdby,
                    createdon = oMfgjoborder.createdon,
                    modifiedby = oMfgjoborder.modifiedby,
                    modifiedon = oMfgjoborder.modifiedon,
                    version = 1
                });
                foreach (var item in items)
                    Insert(transaction, conn, item);
                if (list.Exists(x => x.supportbominvoice))
                {

                    BOMinvoiceDataService bOMinvoiceDataService = new BOMinvoiceDataService() { Context = this.Context };
                    var exist = bOMinvoiceDataService.Search(new BOMinvoiceCreteria()
                    {
                        isjobinward = true,
                        mfgjoborderid = oMfgjoborder.id
                    });
                    BOMinvoiceDetail bOMinvoice = new BOMinvoiceDetail();
                    bOMinvoice.invoicedate = oMfgjoborder.invoicedate;
                    bOMinvoice.invoiceno = oMfgjoborder.invoiceno;
                    bOMinvoice.suppliercompanyid = oMfgjoborder.supplierid;
                    bOMinvoice.isactive = true;
                    bOMinvoice.isjobinward = true;
                    bOMinvoice.mfgjoborderid = oMfgjoborder.id;
                    bOMinvoice.jobcardno = oMfgjoborder.jobcardno;
                    bOMinvoice.locationid = oMfgjoborder.fromlocationid;
                    bOMinvoice.createdby = Context.UserID;
                    bOMinvoice.modifiedby = Context.UserID;
                    bOMinvoice.createdon = DateTimeNow;
                    bOMinvoice.modifiedon = DateTimeNow;
                    bOMinvoice.companyid = oMfgjoborder.companyid;
                    bOMinvoice.bominvoiceitems = list.FindAll(x => x.supportbominvoice && x.rqty > 0).ConvertAll(e => new Bominvoiceitems()
                    {
                        bomid = e.bomid,
                        colourid = e.colourid,
                        buyingqty = e.rqty,
                        isactive = true,
                        width = e.width,
                        sellingprice = e.cost,
                        purchasetax = e.tax,
                        purchasetaxpercentage = e.taxpercentage,
                        createdby = oMfgjoborder.createdby,
                        createdon = oMfgjoborder.createdon,
                        modifiedby = oMfgjoborder.modifiedby,
                        modifiedon = oMfgjoborder.modifiedon,
                        version = 1
                    });
                    if (exist != null && exist.Count > 0)
                    {
                        var existitems = exist[0].bominvoiceitems;
                        exist[0].bominvoiceitems = new List<Bominvoiceitems>();
                        foreach (var item in bOMinvoice.bominvoiceitems)
                        {
                            var eitem = existitems.Find(x => x.bomid == item.bomid && x.colourid == item.colourid);
                            if (eitem != null)
                            {
                                eitem.buyingqty = item.buyingprice;
                                exist[0].bominvoiceitems.Add(eitem);
                            }
                            else
                            {
                                item.isactive = true;
                                exist[0].bominvoiceitems.Add(item);
                            }
                        }

                        exist[0].invoicedate = oMfgjoborder.invoicedate;
                        exist[0].invoiceno = oMfgjoborder.invoiceno;
                        exist[0].suppliercompanyid = oMfgjoborder.supplierid;
                        exist[0].isactive = true;
                        exist[0].isjobinward = true;
                        exist[0].mfgjoborderid = oMfgjoborder.id;
                        exist[0].jobcardno = oMfgjoborder.jobcardno;
                        exist[0].locationid = oMfgjoborder.fromlocationid;

                        exist[0].taxamount = exist[0].bominvoiceitems.Sum(x => x.purchasetax);
                        exist[0].grossamount = exist[0].bominvoiceitems.Sum(x => x.amount-x.purchasetax);
                        exist[0].totalamount = exist[0].bominvoiceitems.Sum(x => x.amount);
                        bOMinvoice.id = exist[0].id;
                        bOMinvoice.invoiceentryno = exist[0].invoiceentryno;
                        bOMinvoiceDataService.UpdateInternal(bOMinvoice, conn,transaction);
                    }
                    else
                    {
                        bOMinvoice.taxamount = bOMinvoice.bominvoiceitems.Sum(x => x.purchasetax);
                        bOMinvoice.grossamount = bOMinvoice.bominvoiceitems.Sum(x => x.amount - x.purchasetax);
                        bOMinvoice.totalamount = bOMinvoice.bominvoiceitems.Sum(x => x.amount);
                        bOMinvoiceDataService.InsertInternal(bOMinvoice,conn, transaction);
                    }
                }
                else
                {
                    items = list.FindAll(x => x.rqty > 0).ConvertAll(e => new Bomstocktransaction()
                    {
                        bomid = e.bomid,
                        colourid = e.colourid,
                        qty = e.rqty,
                        sourceid = oMfgjoborder.id,
                        isactive = true,
                        width = e.width,
                        refno = oMfgjoborder.joborderno,
                        transactedon = oMfgjoborder.joborderdate,
                        locationid = oMfgjoborder.fromlocationid,
                        supplierid = oMfgjoborder.supplierid,
                        companyid = oMfgjoborder.companyid,
                        refid = e.id,
                        RowType = rowtype,
                        createdby = oMfgjoborder.createdby,
                        createdon = oMfgjoborder.createdon,
                        modifiedby = oMfgjoborder.modifiedby,
                        modifiedon = oMfgjoborder.modifiedon,
                        version = 1
                    });
                    foreach (var item in items)
                        Insert(transaction, conn, item);
                }
            }
            else
            {
                var items = list.FindAll(x => x.rqty > 0).ConvertAll(e => new Bomstocktransaction()
                {
                    bomid = e.bomid,
                    colourid = e.colourid,
                    qty = e.rqty * -1,
                    sourceid = oMfgjoborder.id,
                    isactive = true,
                    width = e.width,
                    supplierid = oMfgjoborder.supplierid,
                    refno = oMfgjoborder.joborderno,
                    transactedon = oMfgjoborder.joborderdate,
                    locationid = oMfgjoborder.fromlocationid,
                    companyid = oMfgjoborder.companyid,
                    refid = e.id,
                    RowType = rowtype,
                    createdby = oMfgjoborder.createdby,
                    createdon = oMfgjoborder.createdon,
                    modifiedby = oMfgjoborder.modifiedby,
                    modifiedon = oMfgjoborder.modifiedon,
                    version = 1
                });
                foreach (var item in items)
                    Insert(transaction, conn, item);
            }

        }
        internal List<BomStcokTransactionSummary> StockReport(BomstocktransactionCreteria item)
        {
            string querytransaction = @"select Bomstocktransaction.bomid,Bomstocktransaction.colourid,
            Bomstocktransaction.locationid,Bomstocktransaction.companyid,
            sum(case when Bomstocktransaction.qty>0 then Bomstocktransaction.qty else 0 end) transactionin,
            sum(case when Bomstocktransaction.qty<0 then Bomstocktransaction.qty else 0 end) transactionout from Bomstocktransaction
            inner join  bommaster on bommaster.id = bomid
            where Bomstocktransaction.isactive = true {0}
            group by Bomstocktransaction.locationid,Bomstocktransaction.companyid,Bomstocktransaction.bomid,
            Bomstocktransaction.colourid;";

            List<string> condition = new List<string>();
            if (item.bomid > 0)
                condition.Add("Bomstocktransaction.bomid=@bomid");
            if (item.locationid > 0)
                condition.Add("Bomstocktransaction.locationid=@locationid");
            if (item.companyid > 0)
                condition.Add("Bomstocktransaction.companyid=@companyid");
            if (item.categoryid > 0)
                condition.Add("bommaster.categoryid=@categoryid");
            if (item.colourid > 0)
                condition.Add("Bomstocktransaction.colourid=@colourid");

            if (item.bomids != null && item.bomids.Count > 0)
                condition.Add("Bomstocktransaction.bomid in (" + string.Join(",", item.bomids) + ")");

            List<BomStcokTransactionSummary> response = new List<BomStcokTransactionSummary>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(string.Format(querytransaction, ""), conn);
                if (condition.Count > 0)
                    command = new NpgsqlCommand(string.Format(querytransaction, " and " + string.Join(" and ", condition)), conn);
                command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.bomid;
                command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.colourid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.companyid;
                command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.categoryid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BomStcokTransactionSummary bomStcokTransactionSummary = new BomStcokTransactionSummary();
                        bomStcokTransactionSummary.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        bomStcokTransactionSummary.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        bomStcokTransactionSummary.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        bomStcokTransactionSummary.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        //bomStcokTransactionSummary.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        bomStcokTransactionSummary.inward = reader["transactionin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transactionin"]);
                        bomStcokTransactionSummary.outward = reader["transactionout"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transactionout"]) * -1;
                        var index = response.FindIndex(e => e.bomid == bomStcokTransactionSummary.bomid &&
                        e.colourid == bomStcokTransactionSummary.colourid &&
                        e.companyid == bomStcokTransactionSummary.companyid &&
                        e.locationid == bomStcokTransactionSummary.locationid
                        //&& e.width == bomStcokTransactionSummary.width
                        );
                        if (index < 0)
                            response.Add(bomStcokTransactionSummary);
                        else
                        {
                            response[index].inward = bomStcokTransactionSummary.inward;
                            response[index].outward = bomStcokTransactionSummary.outward;
                        }
                    }
                }
                conn.Close();
                return response;
            }
        }

        internal List<BomStcokTransactionSummary> SummaryReport(BomstocktransactionCreteria item)
        {
            string queryopening = @"select bomid,colourid,locationid,companyid,sum(qty) opening from Bomstocktransaction
            inner join bommaster on bommaster.id = bomid
            where Bomstocktransaction.isactive = true and transactedon<@fromdate {0}
            group by Bomstocktransaction.locationid,Bomstocktransaction.companyid,Bomstocktransaction.bomid,
            Bomstocktransaction.colourid;";
            string querytransaction = @"select Bomstocktransaction.bomid,Bomstocktransaction.colourid,
            Bomstocktransaction.locationid,Bomstocktransaction.companyid,
            sum(case when Bomstocktransaction.qty>0 then Bomstocktransaction.qty else 0 end) transactionin,
            sum(case when Bomstocktransaction.qty<0 then Bomstocktransaction.qty else 0 end) transactionout from Bomstocktransaction
            inner join  bommaster on bommaster.id = bomid
            where Bomstocktransaction.isactive = true and transactedon>=@fromdate and transactedon <@todate {0}
            group by Bomstocktransaction.locationid,Bomstocktransaction.companyid,Bomstocktransaction.bomid,
            Bomstocktransaction.colourid;";

            List<string> condition = new List<string>();
            if (item.bomid > 0)
                condition.Add("Bomstocktransaction.bomid=@bomid");
            if (item.locationid > 0)
                condition.Add("Bomstocktransaction.locationid=@locationid");
            if (item.companyid > 0)
                condition.Add("Bomstocktransaction.companyid=@companyid");
            if (item.categoryid > 0)
                condition.Add("bommaster.categoryid=@categoryid");
            if (item.colourid > 0)
                condition.Add("Bomstocktransaction.colourid=@colourid");

            if (item.bomids != null && item.bomids.Count > 0)
                condition.Add("Bomstocktransaction.bomid in (" + string.Join(",", item.bomids) + ")");

            List<BomStcokTransactionSummary> response = new List<BomStcokTransactionSummary>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(string.Format(queryopening + querytransaction, ""), conn);
                if (condition.Count > 0)
                    command = new NpgsqlCommand(string.Format(queryopening + querytransaction, " and " + string.Join(" and ", condition)), conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.todate.Date.AddDays(1);
                command.Parameters.Add("bomid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.bomid;
                command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.colourid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.companyid;
                command.Parameters.Add("categoryid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.categoryid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BomStcokTransactionSummary bomStcokTransactionSummary = new BomStcokTransactionSummary();
                        bomStcokTransactionSummary.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        bomStcokTransactionSummary.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        bomStcokTransactionSummary.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        bomStcokTransactionSummary.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        //bomStcokTransactionSummary.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        bomStcokTransactionSummary.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                        response.Add(bomStcokTransactionSummary);
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            BomStcokTransactionSummary bomStcokTransactionSummary = new BomStcokTransactionSummary();
                            bomStcokTransactionSummary.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                            bomStcokTransactionSummary.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            bomStcokTransactionSummary.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                            bomStcokTransactionSummary.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            //bomStcokTransactionSummary.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                            bomStcokTransactionSummary.inward = reader["transactionin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transactionin"]);
                            bomStcokTransactionSummary.outward = reader["transactionout"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transactionout"]) * -1;
                            var index = response.FindIndex(e => e.bomid == bomStcokTransactionSummary.bomid &&
                            e.colourid == bomStcokTransactionSummary.colourid &&
                            e.companyid == bomStcokTransactionSummary.companyid &&
                            e.locationid == bomStcokTransactionSummary.locationid
                            //&& e.width == bomStcokTransactionSummary.width
                            );
                            if (index < 0)
                                response.Add(bomStcokTransactionSummary);
                            else
                            {
                                response[index].inward = bomStcokTransactionSummary.inward;
                                response[index].outward = bomStcokTransactionSummary.outward;
                            }
                        }
                    }
                }
                conn.Close();

                BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
                var allboms = bOMMasterDataService.Search(new BOMMasterCreteria()
                {
                    ids = response.ConvertAll(x => x.bomid).Distinct().ToList()
                });
                response.ForEach(x =>
                {
                    var bomindex = allboms.FindIndex(y => y.id == x.bomid);
                    if (bomindex >= 0)
                    {
                        x.romin = allboms[bomindex].romin;
                        x.romax = allboms[bomindex].romax;
                        x.buyingrate = allboms[bomindex].buyingrate;
                        x.landingcost = allboms[bomindex].landingcost;
                        x.categoryid = allboms[bomindex].categoryid;
                        if (allboms[bomindex].colourlist != null && allboms[bomindex].colourlist.Count > 0)
                        {
                            var colourcodes = allboms[bomindex].colourlist.FindAll(z => z.id == x.colourid);
                            if (colourcodes.Count > 0)
                            {
                                x.colourcode = colourcodes.OrderBy(y => y.name).First().name;
                                x.romin = colourcodes[0].romin;
                                x.romax = colourcodes[0].romax;
                            }
                            else
                            {
                                x.colourcode = "Shade Missing";
                            }
                        }
                    }
                });

                if(item.reorderlevel)
                {
                    string pendingpoitems = @"select sum(qty-(rqty+cqty)) pqty,ComponentPOitems.colourid,
                    ComponentPOitems.bomid from ComponentPOitems
                    WHERE ComponentPOitems.isactive=true
	                and (qty-(rqty+cqty))>0
	                group by ComponentPOitems.colourid,
                    ComponentPOitems.bomid";
                    using (var reader = ExecuteReader(pendingpoitems, null))
                    {
                        while(reader.Read())
                        {
                            long bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                            long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            decimal pqty = Convert.ToDecimal(reader["pqty"]);
                            var index = response.FindIndex(x => x.bomid == bomid && x.colourid == colourid);
                            if(index>=0)
                            {
                                response[index].poqty = pqty;
                            }
                        }
                    }
                }
            }

            if (item.stockvalue)
            {
                var pendingstock = response.FindAll(x => x.closing > 0);
                string pendingpoitems = @"select sum(Bominvoiceitems.buyingqty) qty, sum(Bominvoiceitems.buyingqty * buyingprice) amt,bomid,colourid from Bominvoiceitems
                inner join bominvoice on bominvoice.id = bominvoiceitems.bominvoiceid
                where bominvoice.invoicedate > @fromdate
                group by bomid,colourid";
                var param = new SortedList<string, object>();
                param.Add("fromdate", DateTime.Now.AddYears(-1));

                using (var reader = ExecuteReader(pendingpoitems, param))
                {
                    while (reader.Read())
                    {
                        long bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        decimal amt = Convert.ToDecimal(reader["amt"]);
                        decimal qty = Convert.ToDecimal(reader["qty"]);
                        var index = response.FindIndex(x => x.bomid == bomid && x.colourid == colourid);
                        if (index >= 0)
                        {
                            response[index].stockvalue = response[index].closing  * (amt / qty);
                            pendingstock.Remove(response[index]);
                        }
                    }
                }

                if (pendingstock.Count > 0)
                {
                    pendingpoitems = String.Format(@"select sum(Bominvoiceitems.buyingqty) qty, sum(Bominvoiceitems.buyingqty * buyingprice) amt,bomid,colourid from Bominvoiceitems
                    inner join bominvoice on bominvoice.id = bominvoiceitems.bominvoiceid
                    where bomid in ({0})
                    group by bomid,colourid", String.Join(",", pendingstock.ConvertAll(x => x.bomid)));

                    using (var reader = ExecuteReader(pendingpoitems, param))
                    {
                        while (reader.Read())
                        {
                            long bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                            long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                            decimal amt = Convert.ToDecimal(reader["amt"]);
                            decimal qty = Convert.ToDecimal(reader["qty"]);
                            var index = pendingstock.FindIndex(x => x.bomid == bomid && x.colourid == colourid);
                            if (index >= 0)
                            {
                                pendingstock[index].stockvalue = pendingstock[index].closing * (amt / qty);
                            }
                        }
                    }
                }
            }



            return response;
        }


        internal List<BomStcokTransactionDetail> DetailReport(BomstocktransactionCreteria item)
        {

            List<BomStcokTransactionSummary> summary = new List<BomStcokTransactionSummary>();
            List<BomStcokTransactionDetail> response = new List<BomStcokTransactionDetail>();

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select bomid,colourid,companyid,locationid,sum(qty) opening from Bomstocktransaction
                where transactedon<@fromdate and isactive=true ";

                if (item.companyid > 0)
                    qb.AddParameter("companyid", item.companyid, "=");
                if (item.locationid > 0)
                    qb.AddParameter("locationid", item.locationid, "=");
                if (item.colourid > 0)
                    qb.AddParameter("colourid", item.colourid, "=");
                if (item.refid > 0)
                    qb.AddParameter("refid", item.refid, "=");
                if (!string.IsNullOrEmpty(item.width))
                    qb.AddParameter("width", item.width, "=");
                if (item.bomid > 0)
                    qb.AddParameter("bomid", item.bomid, "=");

                NpgsqlCommand command = qb.GetCommand(conn);
                command.CommandText += " group by locationid,companyid,bomid,colourid";
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.todate.Date.AddDays(1);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BomStcokTransactionSummary bomStcokTransactionSummary = new BomStcokTransactionSummary();
                        bomStcokTransactionSummary.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        bomStcokTransactionSummary.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        bomStcokTransactionSummary.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        bomStcokTransactionSummary.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        //bomStcokTransactionSummary.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        bomStcokTransactionSummary.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                        summary.Add(bomStcokTransactionSummary);
                    }
                }

                qb = new QueryBuilder();
                qb.CurrentQuery = @"select transactedon,locationid,otherlocationid,supplierid,bomid,companyid,colourid,width,reftype,refno,qty from Bomstocktransaction
                    where transactedon>=@fromdate and transactedon <@todate and isactive=true ";

                if (item.companyid > 0)
                    qb.AddParameter("companyid", item.companyid, "=");
                if (item.locationid > 0)
                    qb.AddParameter("locationid", item.locationid, "=");
                if (item.colourid > 0)
                    qb.AddParameter("colourid", item.colourid, "=");
                if (item.bomid > 0)
                    qb.AddParameter("bomid", item.bomid, "=");
                if (item.refid > 0)
                    qb.AddParameter("refid", item.refid, "=");
                if (!string.IsNullOrEmpty(item.width))
                    qb.AddParameter("width", item.width, "=");
                qb.SortField = "transactedon::date,id";

                command = qb.GetCommand(conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.fromdate.Date;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.todate.Date.AddDays(1);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BomStcokTransactionDetail bomStcokTransactionSummary = new BomStcokTransactionDetail();
                        bomStcokTransactionSummary.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        bomStcokTransactionSummary.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        bomStcokTransactionSummary.reftype = reader["reftype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["reftype"]);
                        bomStcokTransactionSummary.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        bomStcokTransactionSummary.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        bomStcokTransactionSummary.otherlocationid = reader["otherlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["otherlocationid"]);

                        bomStcokTransactionSummary.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        //bomStcokTransactionSummary.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        bomStcokTransactionSummary.refno = reader["refno"] == DBNull.Value ? "" : Convert.ToString(reader["refno"]);
                        bomStcokTransactionSummary.transactedon = reader["transactedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactedon"]);

                        var qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        if (qty > 0)
                            bomStcokTransactionSummary.inward = qty;
                        else
                            bomStcokTransactionSummary.outward = Math.Abs(qty);

                        var index = summary.FindIndex(e => e.bomid == bomStcokTransactionSummary.bomid &&
                        e.colourid == bomStcokTransactionSummary.colourid &&
                        e.companyid == bomStcokTransactionSummary.companyid &&
                        e.locationid == bomStcokTransactionSummary.locationid);
                        if (index < 0)
                        {
                            summary.Add(new BomStcokTransactionSummary()
                            {
                                bomid = bomStcokTransactionSummary.bomid,
                                colourid = bomStcokTransactionSummary.colourid,
                                companyid = bomStcokTransactionSummary.companyid,
                                locationid = bomStcokTransactionSummary.locationid
                            });
                            index = summary.FindIndex(e => e.bomid == bomStcokTransactionSummary.bomid &&
                            e.colourid == bomStcokTransactionSummary.colourid &&
                            e.locationid == bomStcokTransactionSummary.locationid &&
                            e.companyid == bomStcokTransactionSummary.companyid);
                        }
                        bomStcokTransactionSummary.opening = summary[index].closing;
                        summary[index].inward += bomStcokTransactionSummary.inward;
                        summary[index].outward += bomStcokTransactionSummary.outward;
                        bomStcokTransactionSummary.closing = summary[index].closing;
                        response.Add(bomStcokTransactionSummary);
                    }
                }
                conn.Close();
            }
            return response;
        }

        internal void Update(Bomstocktransaction.TransactionType rowtype,
            NpgsqlConnection conn, NpgsqlTransaction transaction, Mfgjoborder oMfgjoborder,
            List<Mfgjoborderitems> list)
        {
            if (list.Count == 0) return;
            string query = string.Format(SQL_HardDeleteAll, string.Join(",", list.ConvertAll(x => x.id)));
            NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)rowtype;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.id;
            int successCount = command.ExecuteNonQuery();
            Insert(rowtype, conn, transaction, oMfgjoborder, list);
        }

        internal int RemoveBomStcokTransaction(List<BomStcokTransactionSummary> items)
        {
            int count = 0;
            foreach (var item in items)
            {
                var param = new SortedList<string, object>();
                param.Add("modifiedby", Context.UserID);
                param.Add("bomid", item.bomid);
                param.Add("locationid", item.locationid);
                param.Add("colourid", item.colourid);
                string query = @"update bomstocktransaction set isactive=false,version=9999,
                modifiedon=current_date+current_time,modifiedby=@modifiedby 
                where bomid =@bomid and locationid=@locationid and colourid=@colourid";
                count += ExecuteQuery(query, param);
            }
            return count;
        }
    }

}
