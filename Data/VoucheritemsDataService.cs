using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;


namespace InfyPOS.Data
{
    public class VoucheritemsDataService : SAASData
    {
        const string SQL_Search = @"select id, voucherid,bwise, iscredit, ledgerid, amount, isactive, createdby, modifiedby, version, createdon, modifiedon,attributes,hastax,
        taxable,taxpercentage,taxtype,rate,qty,sledgerid,tds from voucheritems where id=@id";
        const string SQL_SearchAll = @"select id, voucherid,bwise, iscredit, ledgerid, amount, isactive, createdby, modifiedby, version, createdon, modifiedon,attributes,hastax,
        taxable,taxpercentage,taxtype,rate,qty,sledgerid,tds from voucheritems where isactive = true and voucherid=@voucherid";
        const string SQL_Insert = @"INSERT INTO public.voucheritems(voucherid,bwise, iscredit, ledgerid, amount, isactive, createdby, modifiedby, version, createdon, modifiedon,attributes,hastax,taxable,taxpercentage,taxtype,rate,qty,sledgerid,tds) 
        VALUES(@voucherid,@bwise,@iscredit, @ledgerid, @amount, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@attributes,@hastax,@taxable,@taxpercentage,@taxtype,@rate,@qty,@sledgerid,@tds) RETURNING id";
        const string SQL_Update = @"UPDATE public.voucheritems SET voucherid=@voucherid,bwise=@bwise, iscredit=@iscredit, ledgerid=@ledgerid, amount=@amount, attributes=@attributes, modifiedby=@modifiedby, 
        version=@version+1, taxable=@taxable,taxpercentage=@taxpercentage,taxtype=@taxtype,rate=@rate,qty=@qty,sledgerid=@sledgerid,tds=@tds,modifiedon=@modifiedon,hastax=@hastax where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.voucheritems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.voucheritems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where voucherid=@voucherid";
        public List<Voucheritems> Search(VoucheritemsCreteria _voucheritems)
        {
            List<Voucheritems> lst = new List<Voucheritems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_voucheritems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _voucheritems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_voucheritems.Code))
                    //    qb.AddParameter("code", _voucheritems.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Voucheritems ovoucheritems = new Voucheritems();
                        ovoucheritems.isinv = false;
                        ovoucheritems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ovoucheritems.voucherid = reader["voucherid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["voucherid"]);
                        ovoucheritems.iscredit = reader["iscredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscredit"]);
                        ovoucheritems.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        ovoucheritems.sledgerid = reader["sledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sledgerid"]);
                        ovoucheritems.tds = reader["tds"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tds"]);
                        ovoucheritems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ovoucheritems.bwise = reader["bwise"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bwise"]);
                        ovoucheritems.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        ovoucheritems.hastax = reader["hastax"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastax"]);
                        ovoucheritems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        ovoucheritems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ovoucheritems.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        ovoucheritems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        ovoucheritems.taxtype = reader["taxtype"] == DBNull.Value ? "" : Convert.ToString(reader["taxtype"]);

                        ovoucheritems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ovoucheritems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucheritems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucheritems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ovoucheritems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucheritems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(ovoucheritems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _voucheritems);
            }
            return lst;
        }

        public List<Voucheritems> SearchAll(long _voucherid)
        {
            List<Voucheritems> lst = new List<Voucheritems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _voucherid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Voucheritems ovoucheritems = new Voucheritems();
                        ovoucheritems.isinv = false;
                        ovoucheritems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ovoucheritems.voucherid = reader["voucherid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["voucherid"]);
                        ovoucheritems.iscredit = reader["iscredit"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscredit"]);
                        ovoucheritems.ledgerid = reader["ledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ledgerid"]);
                        ovoucheritems.sledgerid = reader["sledgerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sledgerid"]);
                        ovoucheritems.tds = reader["tds"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tds"]);
                        ovoucheritems.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ovoucheritems.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        ovoucheritems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ovoucheritems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ovoucheritems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ovoucheritems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ovoucheritems.bwise = reader["bwise"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bwise"]);
                        ovoucheritems.hastax = reader["hastax"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastax"]);
                        ovoucheritems.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        ovoucheritems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        ovoucheritems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ovoucheritems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        ovoucheritems.taxtype = reader["taxtype"] == DBNull.Value ? "" : Convert.ToString(reader["taxtype"]);
                        ovoucheritems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ovoucheritems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(ovoucheritems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _voucherid);
            }
            return lst;
        }

        public Voucheritems Insert(Models.Voucher voucher, Voucheritems oVoucheritems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, voucher, oVoucheritems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritems);

            }

            return oVoucheritems;
        }

        /*
with tdsvouchers as
(
SELECT voucherid,ledgerid,cast(attributes->>'ledgertdsid' as bigint) tdsledgerid FROM VOUCHERITEMS
where attributes->>'ledgertdsid'!='0' and isactive=true
),tdsledger
as
(
select tdsvouchers.*,voucheritems.ledgerid aledger,voucheritems.id from voucheritems 
inner join tdsvouchers on tdsvouchers.voucherid = voucheritems.voucherid
where voucheritems.isactive=true
),pathvoucheritems
as(
select voucherid,id,ledgerid as sledgerid from tdsledger where  aledger = tdsledgerid
)
update voucheritems set sledgerid = pathvoucheritems.sledgerid from pathvoucheritems
where voucheritems.id = pathvoucheritems.id */

        public Voucheritems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Models.Voucher voucher,
            Voucheritems oVoucheritems)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.voucherid;
            command.Parameters.Add("iscredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.iscredit;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.ledgerid;
            command.Parameters.Add("sledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.sledgerid;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.amount;
            command.Parameters.Add("tds", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.tds;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucheritems.attributes_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.isactive;
            command.Parameters.Add("hastax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.hastax;

            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.rate;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.qty;

            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.taxable;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.taxpercentage;
            command.Parameters.Add("taxtype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = String.IsNullOrEmpty(oVoucheritems.taxtype) ? "" : oVoucheritems.taxtype;

            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bwise", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritems.bwise;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucheritems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            SyncChequeTransaction(transaction, conn, voucher, oVoucheritems);
            return oVoucheritems;
        }

        public void SyncChequeTransaction(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Models.Voucher voucher,
            Voucheritems oVoucheritems)
        {
            if (oVoucheritems.attributes != null &&
                oVoucheritems.attributes.banktransfermode == 2)
            {
                var ledgerids = new List<long>();
                ledgerids.Add(voucher.adjustmentledgerid);
                ledgerids.Add(oVoucheritems.ledgerid);
                LedgerDataService ledgerDataService = new LedgerDataService() { Context = Context };
                var voucherledgers = ledgerDataService.Search(new LedgerCreteria()
                {
                    ids = ledgerids
                });

                long bankid = 0;
                if (voucherledgers.Exists(x => x.id == oVoucheritems.ledgerid))
                    bankid = voucherledgers.Find(x => x.id == oVoucheritems.ledgerid).bankid;

                BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                var partyledger = voucherledgers.Find(x => x.id == voucher.adjustmentledgerid);
                var bct = new Bankchequetransaction()
                {
                    amount = Math.Abs(oVoucheritems.amount),
                    chequedate = oVoucheritems.attributes.bankdate,
                    chequeno = oVoucheritems.attributes.bankreference,
                    bankid = bankid,
                    accountname = partyledger.billingname,
                    sourcetype = "VPIPMT",
                    sourceid = voucher.id,
                    sourcesubid = oVoucheritems.id,
                    referenceno = voucher.voucherno,
                    ledgername = voucherledgers.Find(x => x.id == voucher.adjustmentledgerid).displayname,
                    isactive = true,
                    createdby = Context.UserID,
                    createdon = DateTime.Now,
                    modifiedby = Context.UserID,
                    modifiedon = DateTime.Now
                };

                if(partyledger.attributes != null &&
                    partyledger.attributes.bank != null)
                {
                    bct.accountname = partyledger.attributes.bank.accountname;
                    bct.bankaccountno = partyledger.attributes.bank.accountno;
                    bct.bankbranch = partyledger.attributes.bank.branch;
                    bct.bankifsc = partyledger.attributes.bank.ifsc;
                }
                bankchequetransactionDataService.Insert(transaction, conn, bct);
            }
        }
        public Voucheritems Update(Voucheritems oVoucheritems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn,null, oVoucheritems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritems);

            }
            return oVoucheritems;
        }

        public bool isMigration { get; set; }
        public Voucheritems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Models.Voucher voucher, Voucheritems oVoucheritems)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.id;
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.voucherid;
            command.Parameters.Add("iscredit", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.iscredit;
            command.Parameters.Add("ledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.ledgerid;
            command.Parameters.Add("sledgerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.sledgerid;
            command.Parameters.Add("tds", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.tds;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.amount;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.rate;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.qty;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oVoucheritems.attributes_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oVoucheritems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bwise", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritems.bwise;
            command.Parameters.Add("hastax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.hastax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.taxable;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oVoucheritems.taxpercentage;
            command.Parameters.Add("taxtype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oVoucheritems.taxtype) ? "" : oVoucheritems.taxtype;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oVoucheritems.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            if(!isMigration)
                SyncChequeTransaction(transaction, conn, voucher, oVoucheritems);
            return oVoucheritems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Voucheritems> list, Models.Voucher voucher)
        {
            var existingItems = SearchAll(voucher.id);
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
                    Update(transaction, conn, voucher, item);
                }
                else
                {
                    item.voucherid = voucher.id;
                    Insert(transaction, conn, voucher, item);
                }
            }
            return true;
        }

        public bool Delete(Voucheritems oVoucheritems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oVoucheritems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oVoucheritems);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Voucheritems oVoucheritems)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oVoucheritems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oVoucheritems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oVoucheritems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long voucherid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = voucherid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
