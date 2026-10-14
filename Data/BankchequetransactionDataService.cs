using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Net.Http;
using WebAPI.Data;

namespace InfyPOS.Data

{
    public class BankchequetransactionDataService : SAASData
    {
        const string SQL_Search = @"select bankchequetransaction.id, bankchequetransaction.sourcetype, bankchequetransaction.companyid, 
        bankchequetransaction.locationid, bankchequetransaction.employeeid, bankchequetransaction.sourceid, bankchequetransaction.bankid, 
        bankchequetransaction.chequedate, bankchequetransaction.chequeno, bankchequetransaction.amount, bankchequetransaction.accountname,
        bankchequetransaction.bankaccountno,bankchequetransaction.bankifsc,bankchequetransaction.bankbranch,bankchequetransaction.entrydate, 
        bankchequetransaction.createdon, bankchequetransaction.modifiedon, bankchequetransaction.status, bankchequetransaction.isactive, 
        bankchequetransaction.createdby, bankchequetransaction.modifiedby, bankchequetransaction.version,BRSStatus.status brsstatus,
        bankchequetransaction.ledgername,bankchequetransaction.referenceno,bankchequetransaction.sourcesubid
        from bankchequetransaction
        left outer join BRSStatus on BRSStatus.refid = bankchequetransaction.sourceid and BRSStatus.transactiontype = brtrantypeid and BRSStatus.isactive=true
        where bankchequetransaction.id=@id";


        const string SQL_Insert = @"INSERT INTO public.bankchequetransaction(sourcetype, companyid, locationid, employeeid, sourceid, bankid, chequedate, chequeno, 
    amount, accountname,bankaccountno,bankifsc,bankbranch,entrydate, createdon, modifiedon, status, isactive, createdby, modifiedby, version,brtrantypeid,
    ledgername,referenceno,sourcesubid) 
    VALUES(@sourcetype, @companyid, @locationid, @employeeid, @sourceid, @bankid, @chequedate, @chequeno, @amount, @accountname,@bankaccountno,@bankifsc,
    @bankbranch,@entrydate, @createdon, @modifiedon, @status, @isactive, @createdby, @modifiedby, @version,@brtrantypeid,
    @ledgername,@referenceno,@sourcesubid) RETURNING id";

        const string SQL_Update = @"UPDATE public.bankchequetransaction SET sourcetype=@sourcetype, companyid=@companyid, locationid=@locationid, employeeid=@employeeid, sourceid=@sourceid, bankid=@bankid, chequedate=@chequedate, chequeno=@chequeno, amount=@amount, accountname=@accountname,bankaccountno=@bankaccountno,bankifsc=@bankifsc,bankbranch=@bankbranch,entrydate=@entrydate, modifiedon=@modifiedon, status=@status, modifiedby=@modifiedby, version=@version+1 where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.bankchequetransaction set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_DeleteAll = @"update public.bankchequetransaction set isactive=false,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where sourceid=@sourceid and sourcetype=@sourcetype and isactive=true";

        public List<Bankchequetransaction> Search(BankchequetransactionCreteria _bankchequetransaction)
        {
            List<Bankchequetransaction> lst = new List<Bankchequetransaction>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bankchequetransaction.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bankchequetransaction.id;
                }
                else if (_bankchequetransaction.ids != null)
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("bankchequetransaction.isactive", true, "=");
                    qb.AddInParamer("bankchequetransaction.id", string.Join(",", _bankchequetransaction.ids));
                    qb.SortField = @"case when chequeno ~ '^[0-9]+$' then cast(chequeno as numeric) else cast(0 as numeric) end";
                    command = qb.GetCommand(conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("bankchequetransaction.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_bankchequetransaction.chequeno))
                        qb.AddParameter("bankchequetransaction.chequeno", _bankchequetransaction.chequeno, "=");

                    if (_bankchequetransaction.companyid > 0)
                        qb.AddParameter("bankchequetransaction.companyid", _bankchequetransaction.companyid, "=");

                    if (_bankchequetransaction.locationid > 0)
                        qb.AddParameter("bankchequetransaction.locationid", _bankchequetransaction.locationid, "=");

                    if (!string.IsNullOrEmpty(_bankchequetransaction.sourcetype))
                        qb.AddParameter("bankchequetransaction.sourcetype", _bankchequetransaction.sourcetype, "=");

                    if (_bankchequetransaction.status > 0)
                        qb.AddParameter("bankchequetransaction.status", _bankchequetransaction.status, "=");

                    if (!string.IsNullOrEmpty(_bankchequetransaction.accountname))
                        qb.AddParameter("bankchequetransaction.accountname", "%" + _bankchequetransaction.accountname + "%", " ilike ");

                    if (_bankchequetransaction.fromdate != DateTime.MinValue && _bankchequetransaction.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("bankchequetransaction.chequedate", "fromdate", _bankchequetransaction.fromdate.Date, ">=");
                        qb.AddParameter("bankchequetransaction.chequedate", "todate", _bankchequetransaction.todate.Date.AddDays(1), "<");
                    }

                    if (_bankchequetransaction.entrytodate != DateTime.MinValue && _bankchequetransaction.entryfromdate != DateTime.MinValue)
                    {
                        qb.AddParameter("bankchequetransaction.entrydate", "entryfromdate", _bankchequetransaction.entryfromdate, ">=");
                        qb.AddParameter("bankchequetransaction.entrydate", "entrytodate", _bankchequetransaction.entrytodate.Date.AddDays(1), "<");
                    }

                    if (_bankchequetransaction.sourceid > 0)
                        qb.AddParameter("bankchequetransaction.sourceid", _bankchequetransaction.sourceid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bankchequetransaction obankchequetransaction = new Bankchequetransaction();
                        obankchequetransaction.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obankchequetransaction.sourcetype = reader["sourcetype"] == DBNull.Value ? "" : reader["sourcetype"].ToString();
                        obankchequetransaction.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obankchequetransaction.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obankchequetransaction.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        obankchequetransaction.sourceid = reader["sourceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourceid"]);
                        obankchequetransaction.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        obankchequetransaction.chequedate = reader["chequedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["chequedate"]);
                        obankchequetransaction.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        obankchequetransaction.chequeno = reader["chequeno"] == DBNull.Value ? "" : reader["chequeno"].ToString();
                        obankchequetransaction.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        obankchequetransaction.accountname = reader["accountname"] == DBNull.Value ? "" : reader["accountname"].ToString();
                        obankchequetransaction.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        obankchequetransaction.bankifsc = reader["bankifsc"] == DBNull.Value ? "" : reader["bankifsc"].ToString();
                        obankchequetransaction.bankaccountno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                        obankchequetransaction.ledgername = reader["ledgername"] == DBNull.Value ? "" : reader["ledgername"].ToString();
                        obankchequetransaction.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        obankchequetransaction.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obankchequetransaction.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obankchequetransaction.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        obankchequetransaction.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obankchequetransaction.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obankchequetransaction.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obankchequetransaction.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obankchequetransaction.brsstatus =  reader["brsstatus"] == DBNull.Value ? Bankchequetransaction.ChequeStatus.None :(Bankchequetransaction.ChequeStatus) Convert.ToInt32(reader["brsstatus"]);
                        lst.Add(obankchequetransaction);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bankchequetransaction);
            }
            return lst;
        }

        public Bankchequetransaction Insert(Bankchequetransaction oBankchequetransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oBankchequetransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBankchequetransaction);
            }

            return oBankchequetransaction;
        }

        public Bankchequetransaction Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bankchequetransaction oBankchequetransaction)
        {
            /*UPDATE Bankchequetransaction SET  brtrantypeid = 1 WHERE sourcetype ='SPMT';
                UPDATE Bankchequetransaction SET  brtrantypeid = 2 WHERE sourcetype ='TPMT';
                UPDATE Bankchequetransaction SET  brtrantypeid = 5 WHERE sourcetype ='APMT';
                UPDATE Bankchequetransaction SET  brtrantypeid = 4 WHERE sourcetype ='VPMT';
                UPDATE Bankchequetransaction SET  brtrantypeid = 6 WHERE sourcetype ='VPIPMT';
            
with nameconflict as
(
select Bankchequetransaction.chequeno,Bankchequetransaction.id,Bankchequetransaction.accountname,
(ledger.attributes->'bank')->>'accountname' ,
suppliercompany.bankaccountname,
case 
when (ledger.attributes->'bank')->>'accountname' is not null then  (ledger.attributes->'bank')->>'accountname'
when (suppliercompany.bankaccountname is not null) then suppliercompany.bankaccountname
else Bankchequetransaction.accountname end finalname
from Bankchequetransaction
inner join voucher on voucher.id = Bankchequetransaction.sourceid
inner join voucheritems on voucheritems.voucherid = voucher.id
inner join ledger on ledger.id = voucheritems.ledgerid
left outer join suppliercompany on suppliercompany.supplierid = ledger.supplierid and suppliercompany.isactive=true
where Bankchequetransaction.sourcetype='VPMT'
and Bankchequetransaction.accountname = ledger.displayname
),acnamechange as (
select id,chequeno,accountname,finalname from nameconflict where accountname !=finalname 
)

update Bankchequetransaction set accountname=finalname from acnamechange
where acnamechange.id = Bankchequetransaction.id
             
             
             
             */

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.int_finance && 
                string.IsNullOrEmpty(oBankchequetransaction.accountname)) 
                throw new Exception("Account name is empty while issuing bank cheque transaction");

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.sourcetype == null ? "" : oBankchequetransaction.sourcetype;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.locationid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.employeeid;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.sourceid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.bankid;
            command.Parameters.Add("chequedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.chequedate;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.entrydate;
            command.Parameters.Add("chequeno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.chequeno == null ? "" : oBankchequetransaction.chequeno;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBankchequetransaction.amount;
            command.Parameters.Add("accountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.accountname == null ? "" : oBankchequetransaction.accountname;
            var brtrantypeid = 0;
            switch (oBankchequetransaction.sourcetype)
            {
                case "SPMT":
                    brtrantypeid = 1;
                    break;
                case "TPMT":
                    brtrantypeid = 2;
                    break;
                case "APMT":
                    brtrantypeid = 5;
                    break;
                case "VPMT":
                    brtrantypeid = 4;
                    break;
                case "VPIPMT":
                    brtrantypeid = 6;
                    break;
            }
            command.Parameters.Add("brtrantypeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = brtrantypeid;
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.bankaccountno == null ? "" : oBankchequetransaction.bankaccountno;
            command.Parameters.Add("bankifsc", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.bankifsc == null ? "" : oBankchequetransaction.bankifsc;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.bankbranch == null ? "" : oBankchequetransaction.bankbranch;

            command.Parameters.Add("ledgername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.ledgername == null ? "" : oBankchequetransaction.ledgername;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.referenceno == null ? "" : oBankchequetransaction.referenceno;
            command.Parameters.Add("sourcesubid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBankchequetransaction.sourcesubid;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBankchequetransaction.status;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBankchequetransaction.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBankchequetransaction.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBankchequetransaction.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBankchequetransaction;
        }

        public Bankchequetransaction Update(Bankchequetransaction oBankchequetransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBankchequetransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBankchequetransaction);
            }

            return oBankchequetransaction;
        }

        public Bankchequetransaction Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bankchequetransaction oBankchequetransaction)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.id;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.sourcetype == null ? "" : oBankchequetransaction.sourcetype;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.locationid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.employeeid;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.sourceid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.bankid;
            command.Parameters.Add("chequedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.chequedate;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.entrydate;
            command.Parameters.Add("chequeno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.chequeno == null ? "" : oBankchequetransaction.chequeno;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBankchequetransaction.amount;
            command.Parameters.Add("accountname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.accountname == null ? "" : oBankchequetransaction.accountname;
            
            command.Parameters.Add("bankaccountno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.bankaccountno == null ? "" : oBankchequetransaction.bankaccountno;
            command.Parameters.Add("bankifsc", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.bankifsc == null ? "" : oBankchequetransaction.bankifsc;
            command.Parameters.Add("bankbranch", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBankchequetransaction.bankbranch == null ? "" : oBankchequetransaction.bankbranch;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.modifiedon;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBankchequetransaction.status;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBankchequetransaction.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBankchequetransaction.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBankchequetransaction.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBankchequetransaction;
        }


        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Bankchequetransaction> list, long id, string sourcetype)
        {

            var existingItems = Search(new BankchequetransactionCreteria()
            {
                sourceid = id,
                sourcetype = sourcetype
            });

            if(existingItems.Any(x=>x.brsstatus != Bankchequetransaction.ChequeStatus.None))
            {
                throw new Exception("BRS Status already got updated, Not allowed to change on source system");
            }

            if (list.All(x => x.id == 0))
            {
                list.ForEach(x =>
                {
                    if (existingItems.Exists(y => y.amount == x.amount &&
                    y.accountname == x.accountname))
                    {
                        x.id = existingItems.Find(y => y.amount == x.amount && y.accountname == x.accountname).id;
                    }
                });
            }

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
                    item.sourceid = id;
                    item.sourcetype = sourcetype;
                    item.version = 1;
                    item.isactive = true;
                    item.createdby = this.Context.UserID;
                    item.modifiedby = this.Context.UserID;
                    item.createdon = DateTimeNow;
                    item.modifiedon = DateTimeNow;
                    item.status = 0;
                    Insert(transaction, conn, item);
                }
            }



            return true;
        }

        public bool ChangeStatus(BankchequetransactionCreteria oBankchequetransaction)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var checkbooks = Search(new BankchequetransactionCreteria()
                    {
                        ids = oBankchequetransaction.ids
                    });

                    foreach (var cheque in checkbooks)
                    {
                        cheque.status = oBankchequetransaction.status;
                        Update(transaction, conn, cheque);
                    }

                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBankchequetransaction);
                return false;
            }
        }
        public bool Delete(Bankchequetransaction oBankchequetransaction)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBankchequetransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBankchequetransaction);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Bankchequetransaction oBankchequetransaction)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBankchequetransaction.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBankchequetransaction.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBankchequetransaction.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBankchequetransaction.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long sourceid, String reference, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("sourceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sourceid;
            command.Parameters.Add("sourcetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = reference;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal decimal BankOpening(BRSCreteria item)
        {
            ReferencelistDataService referencelistDataService = new ReferencelistDataService()
            {
                Context = this.Context
            };
            var banks = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = item.bankid
            });

            DateTime fromdate = DateTime.MinValue;
            decimal openining = 0;

            if (banks[0].additionalinfo !=null &&
                banks[0].additionalinfo.bankinfo != null &&
                banks[0].additionalinfo.bankinfo.ob != null)
            {
                var oldoblist = banks[0].additionalinfo.bankinfo.ob.FindAll(x => x.date < item.fromdate);
                if(oldoblist.Count>0)
                {
                    openining = oldoblist.OrderBy(x => x.date).Last().value;
                }
            }
            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", item.fromdate.Date);
            param.Add("companyid", item.companyid);
            param.Add("bankid", item.bankid);
            param.Add("statusid", item.statusid);

            string query = @"select sum(paidamount*-1) from supplierpayment 
where supplierpayment.isactive =true and paymentmode != 2  and supplierpayment.hascancel=false 
and paymentdate>=@fromdate and paymentdate<@todate and supplierpayment.companyid=@companyid 
and supplierpayment.bankid = @bankid 
union all
select sum(paidamount*-1) 
from transportpayment 
where transportpayment.isactive =true and paymentmode != 2
and paymentdate>=@fromdate and paymentdate<@todate and transportpayment.companyid=@companyid 
and transportpayment.bankid = @bankid  
union all
select sum(paidamount) from agentpayment 
where agentpayment.isactive =true and paymentmode != 2
and paymentdate>=@fromdate and paymentdate<@todate and agentpayment.companyid=@companyid 
and agentpayment.bankid = @bankid 							
union all
select sum(amount) from customerpayment 
where customerpayment.isactive =true and 
cast(paymentinfo->>'paymentmode' as int) !=2 
and paidon>=@fromdate and paidon<@todate
and customerpayment.companyid=@companyid 
and cast(paymentinfo->>'bankid' as bigint) = @bankid
union all								
select 
sum(case when VOUCHER.adjustmentcredit=true then voucheritems.amount * -1 else voucheritems.amount end) balance
from VOUCHER
inner join ledger adjustmentledger on adjustmentledger.id = voucher.adjustmentledgerid
inner join voucheritems on voucheritems.voucherid = VOUCHER.id 
and voucheritems.iscredit != VOUCHER.adjustmentcredit and voucheritems.isactive=true
where adjustmentledger.typeid=7 AND VOUCHER.ISACTIVE=TRUE
and adjustmentledger.bankid = @bankid
and voucherdate>=@fromdate and voucherdate<@todate
and VOUCHER.companyid=@companyid";

            using (var reader = ExecuteReader(query, param))
            {
                while(reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        openining += reader.GetDecimal(0);
                }
            }
            return openining;
        }

        /*with allinactive
        as
        (
        select brsstatus.refid,brsstatus.bankid,brsstatus.transactiontype,brsstatus.id,
	        brsstatus.CREATEDON,brsstatus.status,
        row_number() OVER (PARTITION BY brsstatus.refid,brsstatus.bankid,brsstatus.transactiontype 
				            ORDER BY brsstatus.id desc) rowid
        from brsstatus 
        left outer join (select * from brsstatus where isactive=true) act
        on act.refid = brsstatus.refid 
        and act.bankid = brsstatus.bankid 
        and act.transactiontype = brsstatus.transactiontype 
        where brsstatus.isactive=false 
        and act.id is  null 
        )
        update brsstatus set isactive=true,version=9999 where id in (
        select id from allinactive where rowid=1
        AND BANKID=195369 and status=2)
        */

        internal BRSSummary BRS(BRSCreteria item)
        {
            decimal opening = BankOpening(item);
            List<BRSLedger> responselist = new List<BRSLedger>();
            var param = new SortedList<string, object>();
            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.AddDays(1).Date);
            param.Add("companyid", item.companyid);
            param.Add("bankid", item.bankid);
            param.Add("statusid", item.statusid);

            string statuscondition = "";
            if (item.statusid == 0)
            {
                statuscondition = " and BRSStatus.id is null";
            }
            else if (item.statusid > 0)
            {
                statuscondition = " and BRSStatus.status = @status";
            }
            if (item.supplierpayment)
            {
                string supplierpayment = @"select BRSStatus.id,supplierpayment.id refid,paidamount,paymentdate,receiptno,paymentmode,
                                        instrumentdate,referencedetail,supplierpayment.bankid,supplier.name suppliername,BRSStatus.status,
                                        BRSStatus.realisationon,supplierpayment.createdby,supplierpayment.createdon from supplierpayment 
                                        left outer join BRSStatus on BRSStatus.refid = supplierpayment.id and transactiontype=1 and BRSStatus.isactive=true
                                        inner join supplier on supplier.id = supplierpayment.supplierid
                                        where supplierpayment.isactive =true and paymentmode != 2 and supplierpayment.hascancel=false 
                                        and paymentdate>=@fromdate and paymentdate<@todate and supplierpayment.companyid=@companyid 
                                        and supplierpayment.bankid = @bankid" + statuscondition;

                using (var reader = ExecuteReader(supplierpayment, param))
                {
                    while (reader.Read())
                    {
                        BRSLedger bRSLedger = new BRSLedger() { transactiontype = 1, bankid = item.bankid, companyid = item.companyid };
                        bRSLedger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        bRSLedger.brsstatus = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        bRSLedger.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        bRSLedger.issued = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                        bRSLedger.transactionon = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        bRSLedger.realisationon = reader["realisationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["realisationon"]);
                        bRSLedger.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        bRSLedger.transactioncode = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                        bRSLedger.description = reader["suppliername"] == DBNull.Value ? "" : Convert.ToString(reader["suppliername"]);
                        bRSLedger.remark = reader["referencedetail"] == DBNull.Value ? "" : Convert.ToString(reader["referencedetail"]);


                        //bRSLedger.createdby = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        //bRSLedger.createdon = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        responselist.Add(bRSLedger);
                    }
                }
            }
            if (item.transportpayment)
            {
                string supplierpayment = @"select BRSStatus.id,transportpayment.id refid,paidamount,paymentdate,receiptno,paymentmode,
                                        instrumentdate,referencedetail,transportpayment.bankid,Transport.name transportname,
                                        BRSStatus.status,BRSStatus.realisationon,transportpayment.createdby from transportpayment 
                                        left outer join BRSStatus on BRSStatus.refid = transportpayment.id and transactiontype=2 and BRSStatus.isactive=true
                                        inner join Transport on Transport.id = transportpayment.transportid
                                        where transportpayment.isactive =true and paymentmode != 2
                                        and paymentdate>=@fromdate and paymentdate<@todate and transportpayment.companyid=@companyid 
                                        and transportpayment.bankid = @bankid" + statuscondition;
                using (var reader = ExecuteReader(supplierpayment, param))
                {
                    while (reader.Read())
                    {
                        BRSLedger bRSLedger = new BRSLedger() { transactiontype = 2, bankid = item.bankid, companyid = item.companyid };
                        bRSLedger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        bRSLedger.brsstatus = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        bRSLedger.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        bRSLedger.issued = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                        bRSLedger.transactionon = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        bRSLedger.realisationon = reader["realisationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["realisationon"]);
                        bRSLedger.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        bRSLedger.transactioncode = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                        bRSLedger.description = reader["transportname"] == DBNull.Value ? "" : Convert.ToString(reader["transportname"]);
                        bRSLedger.remark = reader["referencedetail"] == DBNull.Value ? "" : Convert.ToString(reader["referencedetail"]);
                        responselist.Add(bRSLedger);
                    }
                }
            }

            if (item.agentpayment)
            {
                string supplierpayment = @"select BRSStatus.id,agentpayment.id refid,paidamount,paymentdate,receiptno,paymentmode,
                                        instrumentdate,referencedetail,agentpayment.bankid,Agent.name agentname,
                                        BRSStatus.status,BRSStatus.realisationon,agentpayment.createdby from agentpayment 
                                        left outer join BRSStatus on BRSStatus.refid = agentpayment.id and transactiontype=5  and BRSStatus.isactive=true
                                        inner join Agent on Agent.id = agentpayment.agentid
                                        where agentpayment.isactive =true and paymentmode != 2
                                        and paymentdate>=@fromdate and paymentdate<@todate and agentpayment.companyid=@companyid 
                                        and agentpayment.bankid = @bankid " + statuscondition;
                using (var reader = ExecuteReader(supplierpayment, param))
                {
                    while (reader.Read())
                    {
                        BRSLedger bRSLedger = new BRSLedger() { transactiontype = 5, bankid = item.bankid, companyid = item.companyid };
                        bRSLedger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        bRSLedger.brsstatus = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        bRSLedger.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        bRSLedger.issued = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                        bRSLedger.transactionon = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        bRSLedger.realisationon = reader["realisationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["realisationon"]);
                        bRSLedger.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        bRSLedger.transactioncode = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                        bRSLedger.description = reader["agentname"] == DBNull.Value ? "" : Convert.ToString(reader["agentname"]);
                        bRSLedger.remark = reader["referencedetail"] == DBNull.Value ? "" : Convert.ToString(reader["referencedetail"]);
                        responselist.Add(bRSLedger);
                    }
                }
            }

            if (item.customerpayment)
            {
                string supplierpayment = @"select BRSStatus.id,customerpayment.id refid,
                                        amount paidamount,paidon paymentdate,receiptno,
                                        cast(paymentinfo->>'paymentmode' as int) paymentmode,
                                        cast(paymentinfo->>'chequedate' as timestamp)  instrumentdate,
                                        paymentinfo->>'referencedetail' referencedetail,
                                        cast(paymentinfo->>'bankid' as bigint) bankid,
                                        customer.name customername,BRSStatus.status,BRSStatus.realisationon,customerpayment.createdby from customerpayment 
                                        left outer join BRSStatus on BRSStatus.refid = customerpayment.id and transactiontype=3 and BRSStatus.isactive=true
                                        inner join customer on customer.id = customerid
                                        where customerpayment.isactive =true and 
										cast(paymentinfo->>'paymentmode' as int) !=2 
                                        and paidon>=@fromdate and paidon<@todate and customerpayment.companyid=@companyid 
                                        and cast(paymentinfo->>'bankid' as bigint) = @bankid" + statuscondition;
                using (var reader = ExecuteReader(supplierpayment, param))
                {
                    while (reader.Read())
                    {
                        BRSLedger bRSLedger = new BRSLedger() { transactiontype = 3, bankid = item.bankid, companyid = item.companyid };
                        bRSLedger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        bRSLedger.brsstatus = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        bRSLedger.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        bRSLedger.received = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                        bRSLedger.transactionon = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        bRSLedger.realisationon = reader["realisationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["realisationon"]);
                        bRSLedger.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        bRSLedger.transactioncode = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                        bRSLedger.description = reader["customername"] == DBNull.Value ? "" : Convert.ToString(reader["customername"]);
                        bRSLedger.remark = reader["referencedetail"] == DBNull.Value ? "" : Convert.ToString(reader["referencedetail"]);
                        responselist.Add(bRSLedger);
                    }
                }
            }

            if (item.vouchers)
            {
                string supplierpayment = @"select BRSStatus.id,VOUCHER.adjustmentcredit,ledger.displayname ledgername,VOUCHER.id refid,
                                            case when voucheritems.amount<= voucher.amount then 
                                            voucheritems.amount else voucher.amount end paidamount,voucherdate paymentdate,
											voucher.voucherno receiptno,
                                            cast(VOUCHER.attributes->>'bankreference' as varchar) referencedetail,
                                            cast(VOUCHER.attributes->>'bankdate'  as timestamp) instrumentdate,
                                            BRSStatus.status,BRSStatus.realisationon,VOUCHER.narration,VOUCHER.createdby from VOUCHER
                                            inner join voucheritems on voucheritems.voucherid = VOUCHER.id
											and voucheritems.iscredit != VOUCHER.adjustmentcredit
                                            inner join ledger adjustmentledger on adjustmentledger.id = voucher.adjustmentledgerid
                                            inner join ledger on ledger.id = voucheritems.ledgerid
                                            left outer join BRSStatus on BRSStatus.refid = VOUCHER.id and transactiontype=4 and BRSStatus.isactive=true
                                            where adjustmentledger.typeid=7 AND VOUCHER.ISACTIVE=TRUE 
                                            and adjustmentledger.bankid = @bankid and voucheritems.isactive=true
                                            and voucherdate>=@fromdate and voucherdate<@todate and VOUCHER.companyid=@companyid" + statuscondition;


                supplierpayment+= " union all " + @"select BRSStatus.id,voucheritems.iscredit adjustmentcredit,adjustmentledger.displayname ledgername,VOUCHER.id refid,
                                            case when voucheritems.amount<= voucher.amount then 
                                            voucheritems.amount else voucher.amount end paidamount,voucherdate paymentdate,
											voucher.voucherno receiptno,
                                            cast(VOUCHER.attributes->>'bankreference' as varchar) referencedetail,
                                            cast(VOUCHER.attributes->>'bankdate'  as timestamp) instrumentdate,
                                            BRSStatus.status,BRSStatus.realisationon,VOUCHER.narration,VOUCHER.createdby from VOUCHER
                                            inner join voucheritems on voucheritems.voucherid = VOUCHER.id
											and voucheritems.iscredit != VOUCHER.adjustmentcredit
                                            inner join ledger adjustmentledger on adjustmentledger.id = voucher.adjustmentledgerid
                                            inner join ledger on ledger.id = voucheritems.ledgerid
                                            left outer join BRSStatus on BRSStatus.refid = VOUCHER.id and transactiontype=4 and BRSStatus.isactive=true
                                            where ledger.typeid=7 AND VOUCHER.ISACTIVE=TRUE 
                                            and ledger.bankid = @bankid and voucheritems.isactive=true
                                            and voucherdate>=@fromdate and voucherdate<@todate and VOUCHER.companyid=@companyid" + statuscondition;

                SortedDictionary<long, BRSLedger> paymentlist = new SortedDictionary<long, BRSLedger>();
                using (var reader = ExecuteReader(supplierpayment, param))
                {
                    while (reader.Read())
                    {
                        BRSLedger bRSLedger = new BRSLedger() { transactiontype = 4, bankid = item.bankid, companyid = item.companyid };
                        bRSLedger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        bRSLedger.brsstatus = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        bRSLedger.refid = reader["refid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["refid"]);
                        if (Convert.ToBoolean(reader["adjustmentcredit"]))
                        {
                            bRSLedger.issued = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                        }
                        else
                        {
                            bRSLedger.received = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                        }
                        bRSLedger.transactionon = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        bRSLedger.realisationon = reader["realisationon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["realisationon"]);
                        bRSLedger.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        bRSLedger.transactioncode = reader["receiptno"] == DBNull.Value ? "" : Convert.ToString(reader["receiptno"]);
                        bRSLedger.description = reader["ledgername"] == DBNull.Value ? "" : Convert.ToString(reader["ledgername"]);
                        bRSLedger.remark = reader["referencedetail"] == DBNull.Value ? "" : Convert.ToString(reader["referencedetail"]);
                        if(string.IsNullOrEmpty(bRSLedger.remark))
                        {
                            bRSLedger.remark = reader["narration"] == DBNull.Value ? "" : Convert.ToString(reader["narration"]);
                        }
                        if (paymentlist.ContainsKey(bRSLedger.refid))
                        {
                            paymentlist[bRSLedger.refid].description += "," + bRSLedger.description;
                            if (Convert.ToBoolean(reader["adjustmentcredit"]))
                            {
                                paymentlist[bRSLedger.refid].issued += reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                            }
                            else
                            {
                                paymentlist[bRSLedger.refid].received += reader["paidamount"] == DBNull.Value ? 0 : Convert.ToInt64(reader["paidamount"]);
                            }
                        }
                        else
                        {
                            paymentlist.Add(bRSLedger.refid, bRSLedger);
                            responselist.Add(bRSLedger);
                        }
                    }
                }
            }
            return new BRSSummary()
            {
                opening = opening,
                transactions = responselist,
                closing = opening +responselist.Sum(x=>x.received - x.issued )
            }; 
        }

        public Controllers.FileContentInfo PrintSummaryPDF(List<long> ids)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = this.Context };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "CHEQUESUMMERY"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;


            BankchequeSummary bankchequeSummary = new BankchequeSummary()
            {
                items = Search(new BankchequetransactionCreteria()
                {
                    ids = ids
                })
            };

            if(bankchequeSummary.items.Count>0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                if (bankchequeSummary.items.GroupBy(x=>x.chequeno).Count() == 1)
                {
                    bankchequeSummary.ChequeNo = bankchequeSummary.items[0].chequeno;
                    bankchequeSummary.ChequeDate = bankchequeSummary.items[0].chequedate;
                    if(bankchequeSummary.items[0].bankid >0)
                    {
                        bankchequeSummary.bankledger = referencelistDataService.Search(new ReferencelistCreteria() { id = bankchequeSummary.items[0].bankid }).First();
                    }
                }
                
                var locations = referencelistDataService.Search(new ReferencelistCreteria() { type="LOCATION", id = bankchequeSummary.items[0].locationid });
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                bankchequeSummary.company = companyDataService.Search(new CompanyCreteria() { id = bankchequeSummary.items[0].companyid }).First();
                bankchequeSummary.location = new CompanyLocation(locations[0]);

            }

            ReportLibrary.LaserReporter labreport = new ReportLibrary.LaserReporter(Context.OrganizationID);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(Context.OrganizationID, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = bankchequeSummary;
            labreport.DataItems = bankchequeSummary.items;
            List<string> imageids = labreport.format.GetImages();
            var imageData = new InfyPOS.Common.ImageData() { Context = new InfyPOS.Common.UserClaim() { OrganizationID = Context.OrganizationID } };
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    if (img != null)
                        labreport.Additional.Add("IMG-" + imgid, img);
                }
            }
            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new Controllers.FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "chequesummary.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }
        }

        internal List<BRSLedger> SaveBRS(BRSCreteria item)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    BrsstatusDataService brsstatusDataService = new BrsstatusDataService() { Context = this.Context };
                    var status = new Brsstatus();
                    foreach (var ledger in item.items)
                    {
                        if (ledger.id > 0)
                        {
                            status.id = ledger.id;
                            status.status = ledger.brsstatus;
                            status.realisationon = ledger.realisationon;
                            status.modifiedby = Context.UserID;
                            status.modifiedon = DateTime.Now;
                            brsstatusDataService.UpdateStatus(transaction, conn, status);
                        }
                        else
                        {
                            status.id = 0;
                            status.refid = ledger.refid;
                            status.isactive = true;
                            status.status = ledger.brsstatus;
                            status.bankid = ledger.bankid;
                            status.companyid = ledger.bankid;
                            status.createdby = Context.UserID;
                            status.createdon = DateTime.Now;
                            status.transactiontype = ledger.transactiontype;
                            status.realisationon = ledger.realisationon;
                            status.version = 1;
                            brsstatusDataService.Insert(transaction, conn, status);
                            ledger.id = status.id;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);

            }
            return item.items;
        }
    }

}
