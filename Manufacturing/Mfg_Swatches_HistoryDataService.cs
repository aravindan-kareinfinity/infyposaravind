using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class Mfg_Swatches_HistoryDataService : BaseData
    {
        const string SQL_Search = @"select mfg_swatches_history.swatchesid, mfg_swatches_history.folderid, mfg_swatches_history.transactiontype,
        mfg_swatches_history.qty, mfg_swatches_history.remarks, mfg_swatches_history.transactionon, mfg_swatches_history.referencetype, 
        mfg_swatches_history.referenceid, mfg_swatches_history.isactive, mfg_swatches_history.version, mfg_swatches_history.modifiedon,
        mfg_swatches_history.createdon, mfg_swatches_history.createdby, mfg_swatches_history.modifiedby, mfg_swatches_history.id,
        mfg_swatches_history.actionedby,mfgdesign.code design,products.name productname,receiptno,
        brand.name brandname,empaction.name actionemploy,empsalesmen.name salesmen
        from mfg_swatches_history 
        inner join mfg_swatches_folder on mfg_swatches_folder.id = mfg_swatches_history.folderid
		left outer join mfgdesign on mfgdesign.id = mfg_swatches_folder.designid
		left outer join products on products.id = mfgdesign.productid
		left outer join brand on brand.id = mfgdesign.brandid
        left outer join employee empaction on empaction.id = mfg_swatches_history.actionedby
        left outer join employee empsalesmen on empsalesmen.id = mfg_swatches_history.referenceid
        where mfg_swatches_history.id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfg_swatches_history(swatchesid, folderid, transactiontype, qty, remarks, transactionon, referencetype, referenceid, isactive, 
        version, modifiedon, createdon, createdby, modifiedby,actionedby,receiptno) VALUES(@swatchesid, @folderid, @transactiontype, @qty, @remarks, @transactionon, @referencetype, @referenceid, 
        @isactive, @version, @modifiedon, @createdon, @createdby, @modifiedby,@actionedby,@receiptno) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfg_swatches_history SET swatchesid=@swatchesid, folderid=@folderid, 
        transactiontype=@transactiontype, qty=@qty, remarks=@remarks, transactionon=@transactionon, referencetype=@referencetype,
        referenceid=@referenceid, version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby,receiptno=@receiptno,
        actionedby=@actionedby where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfg_swatches_history set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfg_Swatches_HistoryDetail> Search(Mfg_Swatches_HistoryCreteria _mfg_swatches_history)
        {
            List<Mfg_Swatches_HistoryDetail> lst = new List<Mfg_Swatches_HistoryDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfg_swatches_history.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfg_swatches_history.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (_mfg_swatches_history.fromdate.Year > 2000)
                        qb.AddParameter("mfg_swatches_history.transactionon", "from", _mfg_swatches_history.fromdate, ">=");

                    if (_mfg_swatches_history.todate.Year > 2000)
                        qb.AddParameter("mfg_swatches_history.transactionon", "to", _mfg_swatches_history.todate.AddDays(1), "<");

                    qb.AddParameter("mfg_swatches_history.isactive", true, "=");
                    if (_mfg_swatches_history.ids != null)
                        qb.AddInParamer("mfg_swatches_history.id", string.Join(",", _mfg_swatches_history.ids.ConvertAll(x => x.ToString())));

                    if (_mfg_swatches_history.actionedby > 0)
                        qb.AddParameter("mfg_swatches_history.actionedby", "actionedby", _mfg_swatches_history.actionedby, "=");

                    if (!string.IsNullOrEmpty(_mfg_swatches_history.receiptno))
                    {
                        qb.AddParameter("mfg_swatches_history.receiptno", "receiptno", _mfg_swatches_history.receiptno, "=");
                    }else
                    {
                        if (_mfg_swatches_history.transactiontype >= 0)
                            qb.AddParameter("mfg_swatches_history.transactiontype", "transactiontype", _mfg_swatches_history.transactiontype, "=");
                    }

                    if (_mfg_swatches_history.referenceid > 0)
                        qb.AddParameter("mfg_swatches_history.referenceid", "referenceid", _mfg_swatches_history.referenceid, "=");


                    if (!string.IsNullOrEmpty(_mfg_swatches_history.design))
                    {
                        qb.AddInParamer("mfg_swatches_folder.code", string.Join(",", _mfg_swatches_history.design.Split(',').ToList().ConvertAll(x => "'" + x + "'")));
                    }
                    qb.SortField = "mfg_swatches_folder.id";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfg_Swatches_HistoryDetail omfg_swatches_history = new Mfg_Swatches_HistoryDetail();
                        omfg_swatches_history.swatchesid = reader["swatchesid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["swatchesid"]);
                        omfg_swatches_history.folderid = reader["folderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["folderid"]);
                        omfg_swatches_history.transactiontype = reader["transactiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["transactiontype"]);
                        omfg_swatches_history.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                        omfg_swatches_history.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        omfg_swatches_history.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();

                        omfg_swatches_history.transactionon = reader["transactionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactionon"]);
                        omfg_swatches_history.referencetype = reader["referencetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["referencetype"]);
                        omfg_swatches_history.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                        omfg_swatches_history.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfg_swatches_history.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfg_swatches_history.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfg_swatches_history.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfg_swatches_history.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfg_swatches_history.actionedby = reader["actionedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionedby"]);
                        omfg_swatches_history.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfg_swatches_history.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfg_swatches_history.action = reader["actionemploy"] == DBNull.Value ? "" : Convert.ToString(reader["actionemploy"]);
                        omfg_swatches_history.salesmen = reader["salesmen"] == DBNull.Value ? "" : Convert.ToString(reader["salesmen"]);
                        omfg_swatches_history.design = reader["design"] == DBNull.Value ? "" : Convert.ToString(reader["design"]);
                        omfg_swatches_history.product = reader["productname"] == DBNull.Value ? "" : Convert.ToString(reader["productname"]);
                        omfg_swatches_history.brand = reader["brandname"] == DBNull.Value ? "" : Convert.ToString(reader["brandname"]);
                        lst.Add(omfg_swatches_history);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfg_swatches_history);
            }

            if (_mfg_swatches_history.includeitems && lst.Count == 1 && !string.IsNullOrEmpty(lst[0].receiptno))
            {
                lst[0].items = Search(new Mfg_Swatches_HistoryCreteria()
                {
                    receiptno = lst[0].receiptno
                });
                var orderid = 1;
                lst[0].items.ForEach(x => x.orderid = orderid++);
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                lst[0].company = companyDataService.Search(new CompanyCreteria() { }).First();
                var employees = employeeDataService.Search(new EmployeeCreteria()
                {
                    id = lst[0].actionedby,
                    hidesalaryinfo = true
                });
                if (employees.Count > 0)
                    lst[0].employee = employees.First();

                employees = employeeDataService.Search(new EmployeeCreteria()
                {
                    id = lst[0].referenceid,
                    hidesalaryinfo = true
                });
                if (employees.Count > 0)
                    lst[0].employeesalesmen = employees.First();
            }
            return lst;
        }

        public List<Mfg_Swatches_HistoryDetail> Insert(List<Mfg_Swatches_HistoryDetail> oMfg_Swatches_History)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("folder.receiptno", 0);
                    var receiptno = sequence.GetNextNumber(Context, conn, transaction);
                    foreach (var history in oMfg_Swatches_History)
                    {
                        history.receiptno = receiptno;
                        Insert(transaction, conn, history);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches_History);

            }
            return Search(new Mfg_Swatches_HistoryCreteria() { ids = oMfg_Swatches_History.ConvertAll(x => x.id) });
        }

        public Mfg_Swatches_History Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches_History oMfg_Swatches_History)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("swatchesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.swatchesid;
            command.Parameters.Add("folderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.folderid;
            command.Parameters.Add("transactiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.transactiontype;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.qty;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_History.remarks == null ? "" : oMfg_Swatches_History.remarks;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_History.receiptno == null ? "" : oMfg_Swatches_History.receiptno;

            command.Parameters.Add("transactionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.transactionon;
            command.Parameters.Add("referencetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.referencetype;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.referenceid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches_History.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.createdby;
            command.Parameters.Add("actionedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.actionedby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.modifiedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Swatches_History.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfg_Swatches_History;
        }

        public Mfg_Swatches_HistoryDetail Update(Mfg_Swatches_HistoryDetail oMfg_Swatches_History)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfg_Swatches_History);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches_History);

            }
            return Search(new Mfg_Swatches_HistoryCreteria() { id = oMfg_Swatches_History.id }).First();
        }

        public Mfg_Swatches_HistoryDetail Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches_HistoryDetail oMfg_Swatches_History)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("swatchesid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.swatchesid;
            command.Parameters.Add("folderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.folderid;
            command.Parameters.Add("transactiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.transactiontype;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.qty;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_History.remarks == null ? "" : oMfg_Swatches_History.remarks;
            command.Parameters.Add("transactionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.transactionon;
            command.Parameters.Add("referencetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.referencetype;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.referenceid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches_History.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.id;
            command.Parameters.Add("actionedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.actionedby;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches_History.receiptno == null ? "" : oMfg_Swatches_History.receiptno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Swatches_History.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfg_Swatches_History;
        }



        public bool Delete(Mfg_Swatches_History oMfg_Swatches_History)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfg_Swatches_History);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches_History);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches_History oMfg_Swatches_History)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches_History.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches_History.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches_History.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches_History.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
