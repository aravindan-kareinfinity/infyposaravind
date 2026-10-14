using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BomrequestDataService : BaseData
    {
        const string SQL_Search = @"select items, freight, id, entrydate, tolocationid, 
    bundleid, status, modifiedby, preparedby, approvedby, companyid, isactive, 
    createdby, version, createdon, modifiedon, mfgjoborderid, mfgworkorderid, 
    fromlocationid, entryno, bundlecode, workorderno,joborderno,supplierid,
    mfgstageid,mode, remarks from bomrequest where id=@id";

        const string SQL_SearchInbox = @"select 
bomrequest.id, bomrequest.entrydate, 
bomrequest.status, bomrequest.mode,
bomrequest.modifiedby, bomrequest.preparedby, 
bomrequest.approvedby, bomrequest.companyid, 
bomrequest.mfgworkorderid, bomrequest.fromlocationid, 
bomrequest.entryno, bomrequest.bundlecode, 
bomrequest.mfgstageid, bomrequest.mode,bomrequest.supplierid,
bomrequest.remarks,stages.name stagename,
mfgworkorder.workorderno,mfgworkorder.wodate,
mfgjoborder.joborderno,mfgjoborder.joborderdate,
fromlocation.name fromlocation,tolocation.name tolocation,
employee.name approvedemp
from bomrequest
left outer join mfgworkorder on mfgworkorder.id = mfgworkorderid
left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
left outer join referencelist stages on stages.id = bomrequest.mfgstageid
left outer join referencelist fromlocation on fromlocation.id = bomrequest.fromlocationid
left outer join referencelist tolocation on tolocation.id = bomrequest.tolocationid
left outer join users on users.id = bomrequest.approvedby
left outer join employee on employee.id = users.employeeid";


        const string SQL_Insert = @"INSERT INTO public.bomrequest(items, freight, entrydate, tolocationid, bundleid, status, modifiedby, preparedby, approvedby, companyid, isactive, createdby, version, createdon, modifiedon, mfgjoborderid, mfgworkorderid, fromlocationid, entryno, bundlecode, 
        workorderno,joborderno,supplierid,mfgstageid,mode, remarks) VALUES(@items, @freight, @entrydate, @tolocationid, @bundleid, @status, @modifiedby, @preparedby, @approvedby, @companyid, @isactive, @createdby, @version, @createdon, @modifiedon, @mfgjoborderid, @mfgworkorderid, @fromlocationid, @entryno, @bundlecode, 
        @workorderno,@joborderno,@supplierid,@mfgstageid,@mode, @remarks) RETURNING id";


        const string SQL_Update = @"UPDATE public.bomrequest SET items=@items, freight=@freight, entrydate=@entrydate, tolocationid=@tolocationid, bundleid=@bundleid, status=@status, modifiedby=@modifiedby, preparedby=@preparedby, approvedby=@approvedby, companyid=@companyid, version=@version+1, modifiedon=@modifiedon, mfgjoborderid=@mfgjoborderid, mfgworkorderid=@mfgworkorderid, fromlocationid=@fromlocationid, entryno=@entryno, bundlecode=@bundlecode, 
        workorderno=@workorderno,joborderno=@joborderno,supplierid=@supplierid,mfgstageid=@mfgstageid,mode=@mode, remarks=@remarks where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.bomrequest set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<BOMRequest> CreateRequest(BomrequestCreteria _BOMConsumption)
        {
            var exist = Search(new BomrequestCreteria()
            {
                mfgjoborderid = _BOMConsumption.mfgjoborderid,
                mfgworkorderid = _BOMConsumption.mfgworkorderid,
                mfgstageid = _BOMConsumption.mfgstageid,
                mode = _BOMConsumption.mode,
                create = true,
                consume = _BOMConsumption.consume
            });
            if (exist.Count > 0)
                return exist;
            List<BOMRequest> response = new List<BOMRequest>();
            response.Add(Create(_BOMConsumption));
            return response;
        }
        public BOMConsumption Import4Job(BomrequestCreteria _BOMConsumption)
        {
            var exist = Search(new BomrequestCreteria()
            {
                mfgjoborderid = _BOMConsumption.mfgjoborderid,
                mfgworkorderid = _BOMConsumption.mfgworkorderid,
                mfgstageid = _BOMConsumption.mfgstageid,
                mode = _BOMConsumption.mode,
                create = _BOMConsumption.create,
                consume = _BOMConsumption.consume
            });
            if (exist.Count > 0 && exist[0].bomconsumptionid > 0)
            {
                BOMConsumptionDataService bOMConsumptionDataService = new BOMConsumptionDataService() { Context = this.Context };
                var response = bOMConsumptionDataService.Search(new BOMConsumptionCreteria() { id = exist[0].bomconsumptionid }).First();
                response.bomentryno = exist[0].entryno;
                return response;
            }
            return null;
        }
        public BOMRequest Create(BomrequestCreteria _bomconsumption)
        {
            BOMRequest bOMConsumption = new BOMRequest()
            {
                items = new List<BOMConsumptionitemsDetail>(),
                mfgjoborderid = _bomconsumption.mfgjoborderid,
                mfgworkorderid = _bomconsumption.mfgworkorderid,
                mfgstageid = _bomconsumption.mfgstageid,
                mode = _bomconsumption.mode
            };
            string query = @"select mfgworkorder.id mfgworkorderid,mfgjoborder.id mfgjoborderid,
                            mfgworkorder.workorderno,joborderno,mfgworkorder.mode wmode,
                            mfgjoborder.mode jmode, mfgworkorder.supplierid wosupplierid,
                            mfgjoborder.supplierid josupplierid
                            from mfgworkorder
                            left outer join mfgjoborder on mfgjoborder.id = @mfgjoborderid
                            where mfgworkorder.id = @mfgworkorderid ";
            var param = new SortedList<string, object>();
            param.Add("mfgjoborderid", _bomconsumption.mfgjoborderid);
            param.Add("mfgworkorderid", _bomconsumption.mfgworkorderid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    bOMConsumption.mfgjoborderid = reader["mfgjoborderid"] != DBNull.Value ? Convert.ToInt64(reader["mfgjoborderid"]) : 0;
                    bOMConsumption.mfgworkorderid = reader["mfgworkorderid"] != DBNull.Value ? Convert.ToInt64(reader["mfgworkorderid"]) : 0;
                    bOMConsumption.workorderno = reader["workorderno"] != DBNull.Value ? Convert.ToString(reader["workorderno"]) : "";
                    bOMConsumption.joborderno = reader["joborderno"] != DBNull.Value ? Convert.ToString(reader["joborderno"]) : "";
                    if (_bomconsumption.mfgjoborderid > 0)
                    {
                        bOMConsumption.supplierid = reader["josupplierid"] != DBNull.Value ? Convert.ToInt64(reader["josupplierid"]) : 0;
                    }
                    else
                    {
                        bOMConsumption.supplierid = reader["wosupplierid"] != DBNull.Value ? Convert.ToInt64(reader["wosupplierid"]) : 0;
                    }
                }
            }
            bOMConsumption.mfgstageid = _bomconsumption.mfgstageid;
            bOMConsumption.mode = _bomconsumption.mode;

            MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
            var mfgworkorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
            {
                id = _bomconsumption.mfgworkorderid
            }).First();

            MfgdesigncostDataService mfgdesigncostDataService = new MfgdesigncostDataService() { Context = this.Context };
            var costlist = mfgdesigncostDataService.Search(new MfgdesigncostCreteria()
            {
                mfgdesignids = mfgworkorder.Mfgworkorderitems.ConvertAll(x => x.designid).Distinct().ToList(),
                stageid = _bomconsumption.mfgstageid
            });

            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
            var bomlist = bOMMasterDataService.Search(new BOMMasterCreteria()
            {
                ids = costlist.Select(x => x.bomid).Distinct().ToList(),
            });

            foreach (var jobitem in mfgworkorder.Mfgworkorderitems)
            {
                var costitems = costlist.FindAll(x => x.mfgdesignid == jobitem.designid &&
                                x.colourid == jobitem.colourid);
                foreach (var costitem in costitems)
                {
                    var bom = bomlist.Find(x => x.id == costitem.bomid);
                    BOMMaster.colouritem col = bom != null ? bom.colourlist.Find(x => x.id == costitem.colourid) : null;
                    string colourcode = col != null ? col.name : "";
                    decimal qty = jobitem.sizelist.Sum(x => x.qty);
                    if (costitem.qty * qty > 0)
                    {
                        BOMConsumptionitemsDetail citem = bOMConsumption.items.Find(X => X.colourid == costitem.colourid &&
                        X.bomid == costitem.bomid);
                        if (citem == null)
                        {
                            citem = new BOMConsumptionitemsDetail()
                            {
                                bomcategoryid = costitem.categoryid,
                                bomid = costitem.bomid,
                                colourid = costitem.colourid,
                                colourcode = colourcode,
                                isactive = true,
                                isrequest = true,
                                rate = bom != null ? bom.buyingrate : 0,
                                qty = costitem.qty * qty
                            };
                            bOMConsumption.items.Add(citem);
                        }
                        else
                        {
                            citem.qty += (costitem.qty * qty);
                        }
                        citem.amount = citem.rate * citem.qty;
                    }
                }
            }
            return bOMConsumption;
        }
        public List<BOMRequest> Search(BomrequestCreteria _bomrequest)
        {
            List<BOMRequest> lst = new List<BOMRequest>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bomrequest.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bomrequest.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_bomrequest.mode > 0)
                        qb.AddParameter("mode", _bomrequest.mode, "=");
                    if (_bomrequest.bomconsumptionid > 0)
                        qb.AddParameter("bomconsumptionid", _bomrequest.bomconsumptionid, "=");
                    if (_bomrequest.consume)
                    {
                        qb.AddParameter("status", 5, "=");
                        qb.AddParameter("mfgstageid", _bomrequest.mfgstageid, "=");
                        qb.AddParameter("mode", _bomrequest.mode, "=");
                        qb.AddParameter("mfgworkorderid", _bomrequest.mfgworkorderid, "=");
                        qb.AddParameter("mfgjoborderid", _bomrequest.mfgjoborderid, "=");
                    }
                    else if (_bomrequest.create)
                    {
                        qb.AddParameter("status", 2, "<");
                        qb.AddParameter("mfgstageid", _bomrequest.mfgstageid, "=");
                        qb.AddParameter("mode", _bomrequest.mode, "=");
                        qb.AddParameter("mfgworkorderid", _bomrequest.mfgworkorderid, "=");
                        qb.AddParameter("mfgjoborderid", _bomrequest.mfgjoborderid, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMRequest obomrequest = new BOMRequest();
                        obomrequest.items_json = reader["items"] == DBNull.Value ? null : reader["items"].ToString();
                        obomrequest.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        obomrequest.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obomrequest.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        obomrequest.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"]);
                        obomrequest.bundleid = reader["bundleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundleid"]);
                        obomrequest.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        obomrequest.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obomrequest.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        obomrequest.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        obomrequest.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obomrequest.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obomrequest.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obomrequest.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obomrequest.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obomrequest.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obomrequest.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        obomrequest.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        obomrequest.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"]);
                        obomrequest.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        obomrequest.bundlecode = reader["bundlecode"] == DBNull.Value ? "" : reader["bundlecode"].ToString();
                        obomrequest.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        obomrequest.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        obomrequest.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        obomrequest.mfgstageid = reader["mfgstageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgstageid"]);
                        obomrequest.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        lst.Add(obomrequest);
                    }
                }
                if (lst.Count == 1 && lst[0].bomconsumptionid == 0)
                {
                    var param = new SortedList<string, object>();
                    param.Add("bomrequestid", lst[0].id);
                    using (var reader = ExecuteReader("select id,entryno from BOMConsumption where bomrequestid=@bomrequestid and isactive=true",
                        param))
                    {
                        if (reader.Read())
                        {
                            lst[0].bomconsumptionid = Convert.ToInt64(reader["id"]);
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bomrequest);
            }


            return lst;
        }
        public List<BomInboxItem> SearchInbox(BomrequestCreteria _bomrequest)
        {
            List<BomInboxItem> lst = new List<BomInboxItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchInbox;
                qb.AddParameter("bomrequest.isactive", true, "=");
                if (_bomrequest.id > 0)
                    qb.AddParameter("bomrequest.id", _bomrequest.id, "=");

                if(_bomrequest.statusid>0)
                {
                    qb.AddParameter("bomrequest.status", _bomrequest.statusid, "=");
                }
                if (_bomrequest.fromlocationid > 0)
                {
                    qb.AddParameter("bomrequest.fromlocationid", _bomrequest.fromlocationid, "=");
                }
                if(_bomrequest.tolocationid>0)
                {
                    qb.AddParameter("bomrequest.tolocationid", _bomrequest.tolocationid, "=");
                }

                if(_bomrequest.addedfrom.Date != DateTime.MinValue)
                    qb.AddParameter("bomrequest.entrydate","fromdate", _bomrequest.addedfrom.Date, ">=");

                if (_bomrequest.addedto!= DateTime.MinValue)
                    qb.AddParameter("bomrequest.entrydate", "todate", _bomrequest.addedfrom.AddDays(1).Date, "<");

                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BomInboxItem obomrequest = new BomInboxItem();
                        obomrequest.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obomrequest.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        obomrequest.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        obomrequest.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        obomrequest.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        obomrequest.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        obomrequest.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        obomrequest.bundlecode = reader["bundlecode"] == DBNull.Value ? "" : reader["bundlecode"].ToString();
                        obomrequest.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        obomrequest.stagename = reader["stagename"] == DBNull.Value ? "" : reader["stagename"].ToString();
                        obomrequest.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        obomrequest.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                        obomrequest.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        obomrequest.joborderdate = reader["joborderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joborderdate"]);
                        obomrequest.fromlocation = reader["fromlocation"] == DBNull.Value ? "" : reader["fromlocation"].ToString();
                        obomrequest.tolocation = reader["tolocation"] == DBNull.Value ? "" : reader["tolocation"].ToString();
                        obomrequest.approvedemp = reader["approvedemp"] == DBNull.Value ? "" : reader["approvedemp"].ToString();

                        lst.Add(obomrequest);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bomrequest);
            }
            return lst;
        }

        public BOMRequest Insert(BOMRequest oBomrequest)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    Insert(transaction, conn, oBomrequest);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBomrequest);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oBomrequest;
        }

        public BOMRequest Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMRequest oBomrequest)
        {
            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("BOMRequest.EntryNo", 0,
            oBomrequest.companyid);
            oBomrequest.entryno = sequence.GetNextNumber(Context, conn, transaction);
            oBomrequest.entrydate = DateTime.Now;

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBomrequest.items_json == null ? "null" : oBomrequest.items_json;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.freight;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomrequest.entrydate;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.tolocationid;
            command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.bundleid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.modifiedby;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.preparedby;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.approvedby;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.companyid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomrequest.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomrequest.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomrequest.modifiedon;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.mfgjoborderid;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.mfgworkorderid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.fromlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.supplierid;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.entryno == null ? "" : oBomrequest.entryno;
            command.Parameters.Add("bundlecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.bundlecode == null ? "" : oBomrequest.bundlecode;
            command.Parameters.Add("joborderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.joborderno == null ? "" : oBomrequest.joborderno;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.workorderno == null ? "" : oBomrequest.workorderno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.remarks == null ? "" : oBomrequest.remarks;
            command.Parameters.Add("mfgstageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.mfgstageid;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.mode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBomrequest.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBomrequest;
        }

        public BOMRequest Update(BOMRequest oBomrequest)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBomrequest);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBomrequest);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oBomrequest;
        }

        public BOMRequest Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMRequest oBomrequest)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("items", NpgsqlTypes.NpgsqlDbType.Json).Value = oBomrequest.items_json == null ? "null" : oBomrequest.items_json;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.freight;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.id;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomrequest.entrydate;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.tolocationid;
            command.Parameters.Add("bundleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.bundleid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.status;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.modifiedby;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.preparedby;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.approvedby;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.companyid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomrequest.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.createdby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomrequest.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBomrequest.modifiedon;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.mfgjoborderid;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.mfgworkorderid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.fromlocationid;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.entryno == null ? "" : oBomrequest.entryno;
            command.Parameters.Add("bundlecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.bundlecode == null ? "" : oBomrequest.bundlecode;
            command.Parameters.Add("joborderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.joborderno == null ? "" : oBomrequest.joborderno;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.workorderno == null ? "" : oBomrequest.workorderno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBomrequest.remarks == null ? "" : oBomrequest.remarks;
            command.Parameters.Add("mfgstageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.mfgstageid;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.mode;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.supplierid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBomrequest.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBomrequest;
        }


        public bool Delete(BOMRequest oBomrequest)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBomrequest);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBomrequest);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMRequest oBomrequest)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBomrequest.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oBomrequest.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBomrequest.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBomrequest.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal bool UpdateStatus(BOMStatus item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "Update bomrequest set status=@status where id in (" + string.Join(",", item.ids) + ") and status<@status";
                    var param = new SortedList<string, object>();
                    param.Add("employeeid", item.employeeid);
                    param.Add("statuson", item.statuson);
                    if (item.status == "approve")
                    {
                        param.Add("status", 1);
                        query = "Update bomrequest set approvedby=@employeeid,approvedon=@statuson,status=@status where id in (" + string.Join(",", item.ids) + ") and status<@status";
                    }
                    else if (item.status == "reject")
                    {
                        param.Add("status", 2);
                        query = "Update bomrequest set rejectedby=@employeeid,rejectedon=@statuson,status=@status where id in (" + string.Join(",", item.ids) + ") and status<@status";
                    }
                    else if (item.status == "issued")
                    {
                        param.Add("status", 4);
                        query = "Update bomrequest set issuedby=@employeeid,issuedon=@statuson,status=@status where id in (" + string.Join(",", item.ids) + ") and status<@status";
                    }
                    else if (item.status == "receive")
                    {
                        param.Add("status", 5);
                        query = "Update bomrequest set receivedby=@employeeid,receivedon=@statuson,status=@status where id in (" + string.Join(",", item.ids) + ") and status<@status";
                    }
                    else
                        param.Add("status", 0);

                    ExecuteQuery(query, param, conn, transaction);

                    transaction.Commit();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return false;
        }


        internal List<BOMConsumptionitemsDetail> ImportBOM(BOMRequest item)
        {
            var results = new List<BOMConsumptionitemsDetail>();
            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };

            List<long> designid = new List<long>();

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6)
            {

                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                {
                    id = item.mfgworkorderid
                }).First();

                string query = @"with allworkorderitemsizes
                    as
                    (
                        select mfgworkorderitemsizes.qty,mfgworkorderitemsizes.sizeid,
                        mfgworkorderitemsizes.uid,mfgworkorderitemsizes.colourid from mfgworkorder
                        inner join mfgworkorderitems on mfgworkorderitems.mfgworkorderid = mfgworkorder.id
                        inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
                        where mfgworkorder.id = @id			
                    ),allsampleorderitems as
                    (
                        select 
                        schemesizes->>'uid' uid,
                        cast(schemesizes->>'id' as bigint) sizeid,
                        schemeitems->>'design_shade' design_shade,
                        schemeitems->>'design_bom' design_bom,

						cast(config->>'repeat' as numeric) repeat,
						cast(schemelist->>'requiredbom' as numeric) requiredbom,

						case when cast(config->>'repeat' as numeric)>0 and 
						cast(schemelist->>'requiredbom' as numeric)>0 then 
						
                        ((cast(schemeitems->>'design_picks' as numeric)*
                        cast(schemeitems->>'design_thickness' as numeric))/
                        (cast(schemelist->>'requiredbom' as numeric)*cast(config->>'repeat' as numeric))) 
						else 
                        cast(schemeitems->>'design_picks' as numeric)*
                        cast(schemeitems->>'design_thickness' as numeric)
                        end perpiece,

                        mfgworkorder.workorderno,
                        mfgworkorder.id,
                        mfgsampleorder.entryno,
                        cast(schemeitems->>'colourid' as bigint) colourid,
                        schemeitems->>'design_colour' design_colour,
                        schemeitems->>'design_thickness' design_thickness,
                        schemeitems->>'design_shade' design_shade,
                        schemeitems->>'design_picks' design_picks,
                        schemeitems->>'design_frontcolour' design_frontcolour,
                        schemeitems->>'design_backcolour' design_backcolour,
                        schemeitems->>'design_bom' design_bom,
				        schemelist->>'design_code' design_code,
				        yarn.id yarnid,
                        yarn.name yarnname,
                        bom.name bomname,
				        bom.id bomid
                        from mfgworkorder
                        inner join mfgsampleorder on mfgsampleorder.id = mfgworkorder.mfgsampleorderid
                        LEFT JOIN LATERAL json_array_elements (cast(designs as json)) schemelist ON true
                        LEFT JOIN LATERAL json_array_elements (cast(schemelist->>'items' as json)) schemeitems ON true
				        LEFT JOIN LATERAL json_array_elements (cast(schemelist->>'sizes' as json)) schemesizes ON true
                        left outer join bommaster yarn on yarn.id = cast(schemeitems->>'design_shade' as bigint)
                        left outer join bommaster bom on bom.id = cast(schemeitems->>'design_bom' as bigint)
                        where mfgworkorder.id = @id
                    )				
                    select allsampleorderitems.uid,allsampleorderitems.sizeid,
                    allsampleorderitems.uid,perpiece,allsampleorderitems.colourid,
                    yarnid,bomid,design_code,allworkorderitemsizes.qty ,
					allworkorderitemsizes.qty*perpiece requried
                    from allsampleorderitems
                    inner join allworkorderitemsizes on
                    allworkorderitemsizes.sizeid = allsampleorderitems.sizeid
                    and allworkorderitemsizes.colourid = allsampleorderitems.colourid";

                query = @"with allworkorderitemsizes
                    as
                    (
                        select mfgworkorderitemsizes.qty,mfgworkorderitemsizes.sizeid,
                        mfgworkorderitemsizes.uid,mfgworkorderitemsizes.colourid from mfgworkorder
                        inner join mfgworkorderitems on mfgworkorderitems.mfgworkorderid = mfgworkorder.id
                        inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
                        where mfgworkorder.id =  @id		
                    ),allsampleorderitems as
                    (
                        select 
                        schemesizes->>'uid' uid,
                        cast(schemesizes->>'id' as bigint) sizeid,
                        schemeitems->>'design_shade' design_shade,
                        schemeitems->>'design_bom' design_bom,
						cast(config->>'repeat' as numeric) repeat,
						cast(schemelist->>'requiredbom' as numeric) requiredbom,
						cast(referencelist.additionalinfo->>'maxlimit' as numeric) machineconstant,
						mfgworkorder.workorderno,
                        mfgworkorder.id,
                        mfgsampleorder.entryno,
                        cast(schemeitems->>'colourid' as bigint) colourid,
                        schemeitems->>'design_colour' design_colour,
                        cast(schemeitems->>'design_thickness' as numeric) design_thickness,
                        schemeitems->>'design_shade' design_shade,
                        cast(schemeitems->>'design_picks' as numeric) design_picks,
                        schemeitems->>'design_frontcolour' design_frontcolour,
                        schemeitems->>'design_backcolour' design_backcolour,
                        schemeitems->>'design_bom' design_bom,
				        schemelist->>'design_code' design_code,
				        yarn.id yarnid,
                        yarn.name yarnname,
                        bom.name bomname,
				        bom.id bomid
                        from mfgworkorder
                        inner join mfgsampleorder on mfgsampleorder.id = mfgworkorder.mfgsampleorderid
						left outer join referencelist on referencelist.id = 
						cast(mfgsampleorder.config->>'machine' as bigint)
						
                        LEFT JOIN LATERAL json_array_elements (cast(designs as json)) schemelist ON true
                        LEFT JOIN LATERAL json_array_elements (cast(schemelist->>'items' as json)) schemeitems ON true
				        LEFT JOIN LATERAL json_array_elements (cast(schemelist->>'sizes' as json)) schemesizes ON true
                        left outer join bommaster yarn on yarn.id = cast(schemeitems->>'design_shade' as bigint)
                        left outer join bommaster bom on bom.id = cast(schemeitems->>'design_bom' as bigint)
                        where mfgworkorder.id =  @id
                    )				
                    select allsampleorderitems.uid,allsampleorderitems.sizeid,
                    allsampleorderitems.uid,allsampleorderitems.colourid,
                    yarnid,bomid,design_code,allworkorderitemsizes.qty ,
					allworkorderitemsizes.qty * design_picks * design_thickness uppervalue,
					machineconstant * repeat downvalue,
					case when (allworkorderitemsizes.qty * design_picks * design_thickness)>0 and
					(machineconstant * repeat)>0 then 
					(allworkorderitemsizes.qty * design_picks * design_thickness)/
					(machineconstant * repeat)
					when allsampleorderitems.requiredbom>0 then 
					allsampleorderitems.requiredbom/1000*allworkorderitemsizes.qty 
					else 0 end requried,
					allsampleorderitems.repeat,
					allsampleorderitems.requiredbom,
					allsampleorderitems.design_picks,
					allsampleorderitems.design_thickness,
					allsampleorderitems.machineconstant
                    from allsampleorderitems
                    inner join allworkorderitemsizes on
                    allworkorderitemsizes.sizeid = allsampleorderitems.sizeid";

                var bomlist = new List<BOMConsumptionitemsDetail>();
                var param = new SortedList<string, object>();
                param.Add("id", item.mfgworkorderid);
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        BOMConsumptionitemsDetail bOMConsumptionitemsDetail = new BOMConsumptionitemsDetail();
                        long bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        if (bomid == 0)
                            bomid = reader["yarnid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["yarnid"]);

                        decimal requried = reader["requried"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requried"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        bOMConsumptionitemsDetail.bomid = bomid;
                        bOMConsumptionitemsDetail.qty = requried;
                        bOMConsumptionitemsDetail.colourid = colourid;
                        bomlist.Add(bOMConsumptionitemsDetail);
                    }
                }

                var fabriclist = bOMMasterDataService.Search(new BOMMasterCreteria()
                {
                    ids = bomlist.ConvertAll(x => x.bomid)
                });

                foreach (var bom in bomlist)
                {
                    var fabric = fabriclist.Find(x => x.id == bom.bomid);
                    if (fabric != null)
                    {
                        results.Add(new BOMConsumptionitemsDetail()
                        {
                            bomcategoryid = fabric.categoryid,
                            bomid = bom.bomid,
                            rate = fabric.buyingrate,
                            qty = bom.qty,
                            colourid = bom.colourid,
                            colourcode = fabric.colourlist.Exists(x => x.id == bom.colourid) ? fabric.colourlist.Find(x => x.id == bom.colourid).name : ""
                        });
                    }
                }
                
            }
            else if (item.mfgjoborderid == 0)
            {
                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                {
                    id = item.mfgworkorderid
                }).First();
                designid.AddRange(workorder.Mfgworkorderitems.ConvertAll(x => x.designid));
                foreach (var witem in workorder.Mfgworkorderitems)
                {
                    var fabrics = bOMMasterDataService.Search(new BOMMasterCreteria()
                    {
                        id = witem.fabricid
                    });
                    if (fabrics.Count == 1)
                    {
                        results.Add(new BOMConsumptionitemsDetail()
                        {
                            bomcategoryid = fabrics[0].categoryid,
                            bomid = fabrics[0].id,
                            qty = witem.outqty,
                            colourid = witem.colourid,
                            colourcode = fabrics[0].colourlist.Exists(x => x.id == witem.colourid) ? fabrics[0].colourlist.Find(x => x.id == witem.colourid).name : ""
                        });
                    }
                }


                designid.AddRange(workorder.Mfgworkorderitems.ConvertAll(x => x.designid));

                MfgdesigncostDataService mfgdesigncostDataService = new MfgdesigncostDataService() { Context = this.Context };
                var bomcostlist = mfgdesigncostDataService.Search(new MfgdesigncostCreteria()
                {
                    mfgdesignids = designid,
                    stageid = item.mfgstageid
                });

                var fabriclist = bOMMasterDataService.Search(new BOMMasterCreteria()
                {
                    ids = bomcostlist.ConvertAll(x => x.bomid)
                });

                foreach (var witem in workorder.Mfgworkorderitems)
                {
                    foreach (var sitem in witem.sizelist)
                    {
                        var bomlist = bomcostlist.FindAll(x => x.mfgdesignid == witem.designid &&
                        x.colourid == witem.colourid &&
                        x.sizeid == sitem.sizeid);

                        foreach (var bom in bomlist)
                        {
                            var fabric = fabriclist.Find(x => x.id == bom.bomid);
                            results.Add(new BOMConsumptionitemsDetail()
                            {
                                bomcategoryid = bom.categoryid,
                                bomid = bom.bomid,
                                rate = fabric.buyingrate,
                                qty = sitem.qty * bom.qty,
                                colourid = witem.colourid,
                                colourcode = fabric.colourlist.Exists(x => x.id == witem.colourid) ? fabric.colourlist.Find(x => x.id == witem.colourid).name : ""
                            });
                        }
                    }
                }

            }
            else
            {
                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                {
                    id = item.mfgworkorderid
                }).First();

                MfgjoborderDataService mfgjoborderDataService = new MfgjoborderDataService() { Context = this.Context };
                var joborder = mfgjoborderDataService.Search(new MfgjoborderCreteria()
                {
                    id = item.mfgjoborderid
                }).First();
                foreach (var jitem in joborder.Mfgjoborderitems.Where(x => x.isbom))
                {
                    var fabrics = bOMMasterDataService.Search(new BOMMasterCreteria()
                    {
                        id = jitem.bomid
                    });
                    if (fabrics.Count == 1)
                    {
                        results.Add(new BOMConsumptionitemsDetail()
                        {
                            bomcategoryid = jitem.bomcategoryid,
                            bomid = jitem.bomid,
                            rate = fabrics[0].buyingrate,
                            qty = jitem.oqty,
                            colourid = jitem.colourid,
                            colourcode = fabrics[0].colourlist.Exists(x => x.id == jitem.colourid) ? fabrics[0].colourlist.Find(x => x.id == jitem.colourid).name : ""
                        });
                    }
                }

                var witems = workorder.Mfgworkorderitems.FindAll(x => joborder.Mfgjoborderitems.Exists(y => y.mfgworkorderitemsid == x.id));
                designid.AddRange(witems.ConvertAll(x => x.designid));

                MfgdesigncostDataService mfgdesigncostDataService = new MfgdesigncostDataService() { Context = this.Context };
                var bomcostlist = mfgdesigncostDataService.Search(new MfgdesigncostCreteria()
                {
                    mfgdesignids = designid,
                    stageid = item.mfgstageid
                });

                var fabriclist = bOMMasterDataService.Search(new BOMMasterCreteria()
                {
                    ids = bomcostlist.ConvertAll(x => x.bomid)
                });

                foreach (var jitem in joborder.Mfgjoborderitems)
                {
                    var witem = workorder.Mfgworkorderitems.Find(x => x.id == jitem.mfgworkorderitemsid);
                    var sitem = witem.sizelist.Find(x => x.id == jitem.mfgworkorderitemsizesid);
                    var bomlist = bomcostlist.FindAll(x => x.mfgdesignid == witem.designid &&
                    x.colourid == witem.colourid &&
                    x.sizeid == sitem.sizeid);

                    foreach (var bom in bomlist)
                    {
                        var fabric = fabriclist.Find(x => x.id == bom.bomid);
                        results.Add(new BOMConsumptionitemsDetail()
                        {
                            bomcategoryid = bom.categoryid,
                            bomid = bom.bomid,
                            qty = jitem.oqty * bom.qty,
                            rate = fabric.buyingrate,
                            colourid = jitem.colourid,
                            colourcode = fabric.colourlist.Exists(x => x.id == jitem.colourid) ? fabric.colourlist.Find(x => x.id == jitem.colourid).name : ""
                        });
                    }
                }
            }

            return results.GroupBy(x => new { x.bomcategoryid, x.bomid, x.rate, x.colourcode, x.colourid }).ToList().ConvertAll(x =>
            {
                return new BOMConsumptionitemsDetail()
                {
                    bomcategoryid = x.Key.bomcategoryid,
                    bomid = x.Key.bomid,
                    rate = x.Key.rate,
                    colourcode = x.Key.colourcode,
                    colourid = x.Key.colourid,
                    qty = Math.Round(x.Sum(y => y.qty),4)
                };
            });
        }

    }

}
