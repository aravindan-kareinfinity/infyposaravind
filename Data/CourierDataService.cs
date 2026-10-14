using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class CourierDataService : SAASData
    {
        const string SQL_Search = @"select description, invnos, isbillcopy, copybills, lrnos, entryno, 
            agentid, attachment, receivingcompanyid, sentby, senton, lrinfo, deliveredto, receivedby, ishold,
            courier.status, supplierid, awbnumber, podnumber, type, receivedon, courier.companyid, courier.modifiedon, courier.createdon, 
            courier.version, courier.modifiedby, courier.createdby, courier.isactive, courier.id, mode, courier.name, customerid, poddate, weight, rate, amount, taxamount,
            courier.taxid, courier.taxpercentage, bundles, ewbno, packedby, users.employeeid,paymode 
            from courier inner join users on users.id=courier.createdby 
            where courier.id=@id";

        const string SQL_Insert = @"INSERT INTO public.courier(description, invnos, isbillcopy, copybills, lrnos, 
entryno, agentid, attachment, receivingcompanyid, sentby, senton, lrinfo, deliveredto, receivedby, status, supplierid,
awbnumber, podnumber, type, receivedon, companyid, modifiedon, createdon, version, modifiedby, createdby, isactive, mode,
name, customerid, poddate, weight, rate, amount, bundles, ewbno, packedby,ishold,paymode,taxamount,taxid,taxpercentage) 
VALUES(@description, @invnos, @isbillcopy, @copybills, @lrnos, @entryno, @agentid, @attachment, @receivingcompanyid,
@sentby, @senton, @lrinfo, @deliveredto, @receivedby, @status, @supplierid, @awbnumber, @podnumber, @type, @receivedon,
@companyid, @modifiedon, @createdon, @version, @modifiedby, @createdby, @isactive, @mode, @name, @customerid, @poddate,
@weight, @rate, @amount, @bundles, @ewbno, @packedby,@ishold,@paymode,@taxamount,@taxid,@taxpercentage) RETURNING id";

        internal List<Courier.BillInfo> SearchBill(CourierCreteria item)
        {
            List<Courier.BillInfo> response = new List<Courier.BillInfo>();
            if (item.mode == 1)
            {
                var param = new SortedList<string, object>();
                param.Add("billno", item.podnumber);
                param.Add("supplierid", item.supplierid);
                param.Add("customerid", item.customerid);
                if (item.supplierid > 0)
                {
                    string query = @"Select total from lrinvoicereturn where returnno=@billno and supplierid=@supplierid;
                                     Select totalamount from jobworkbill where invoiceno=@billno and supplierid=@supplierid;";
                    using (var reader = ExecuteReader(query, param))
                    {
                        if (reader.Read())
                        {
                            response.Add(new Courier.BillInfo()
                            {
                                billno = item.podnumber,
                                billamount = reader.GetDecimal(0),
                                billtype = "Debit Note"
                            });
                        }
                        else if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                response.Add(new Courier.BillInfo()
                                {
                                    billno = item.podnumber,
                                    billamount = reader.GetDecimal(0),
                                    billtype = "Job Work"
                                });
                            }
                        }
                    }
                }
                else if (item.customerid > 0)
                {
                    string query = @"Select receivable+rounding from bill where billno=@billno and customerid=@customerid;";
                    using (var reader = ExecuteReader(query, param))
                    {
                        if (reader.Read())
                        {
                            response.Add(new Courier.BillInfo()
                            {
                                billno = item.podnumber,
                                billamount = reader.GetDecimal(0),
                                billtype = "Debit Note"
                            });
                        }
                    }
                }
                else
                {
                    string query = @"Select receivable+rounding,customerid from bill where billno=@billno order by id desc;
                                     Select total,supplierid from lrinvoicereturn where returnno=@billno order by id desc;
                                     Select totalamount,supplierid from jobworkbill where invoiceno=@billno  order by id desc";
                    using (var reader = ExecuteReader(query, param))
                    {
                        string[] billtypes = new string[] { "Bill", "Debit Note", "Job Work" };
                        int index = 0;
                        do
                        {
                            if (reader.Read())
                            {
                                long customerid = 0;
                                long supplierid = 0;
                                if(index == 0)
                                {
                                    customerid = reader["customerid"] != DBNull.Value ? Convert.ToInt64(reader["customerid"]) : 0;
                                }
                                else
                                {
                                    supplierid = reader["supplierid"] != DBNull.Value ? Convert.ToInt64(reader["supplierid"]) : 0;
                                }
                                response.Add(new Courier.BillInfo()
                                {
                                    billno = item.podnumber,
                                    billamount = reader.GetDecimal(0),
                                    billtype = billtypes[index],
                                    supplierid = supplierid,
                                    customerid= customerid
                                });
                                break;
                            }
                            index++;
                        } while (reader.NextResult());
                    }
                }
            }
            return response;
        }

        internal byte[] ImagePDFGeneration(CourierCreteria item)
        {
            var records = Search(item);
            if(!records.Exists(e=>e.attachment != null && e.attachment.Count>0))
            {
                throw new Exception("No attachment available for this given courier");
            }
            PDFGenerator pDFGenerator = new PDFGenerator() { Context = this.Context };
            return pDFGenerator.ImagePDFGeneration(records);
        }

        const string SQL_Update = @"UPDATE public.courier SET description=@description,ishold=@ishold,paymode=@paymode, invnos=@invnos, isbillcopy=@isbillcopy, copybills=@copybills, lrnos=@lrnos, entryno=@entryno, agentid=@agentid, attachment=@attachment, receivingcompanyid=@receivingcompanyid, sentby=@sentby, senton=@senton, lrinfo=@lrinfo, deliveredto=@deliveredto, receivedby=@receivedby, status=@status, supplierid=@supplierid, awbnumber=@awbnumber, podnumber=@podnumber, type=@type, receivedon=@receivedon, companyid=@companyid, modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby, mode=@mode, name=@name, customerid=@customerid, poddate=@poddate, weight=@weight, rate=@rate, 
        amount=@amount,taxamount=@taxamount, bundles=@bundles, ewbno=@ewbno, packedby=@packedby, taxid=@taxid, taxpercentage=@taxpercentage where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.courier set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Courier> Search(CourierCreteria _courier)
        {
            List<Courier> lst = new List<Courier>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_courier.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _courier.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("courier.isactive", true, "=");

                    if (_courier.ids != null && _courier.ids.Count > 0)
                        qb.AddInParamer("courier.id", string.Join(",", _courier.ids));
                    else
                    {
                        if (_courier.supplierid > 0)
                            qb.AddParameter("courier.supplierid", "supplierid", _courier.supplierid, "=");
                        if (_courier.receivingcompanyid > 0)
                            qb.AddParameter("courier.receivingcompanyid", "receivingcompanyid", _courier.receivingcompanyid, "=");

                        if (_courier.companyid > 0)
                            qb.AddParameter("courier.companyid", "companyid", _courier.companyid, "=");
                        if (_courier.agentid > 0)
                            qb.AddParameter("courier.agentid", "agentid", _courier.agentid, "=");
                        if (_courier.status > 0)
                            qb.AddParameter("courier.status", "status", (int)_courier.status, "=");
                        if (_courier.type >= 0)
                            qb.AddParameter("courier.type", "type", _courier.type, "=");
                        if (!string.IsNullOrEmpty(_courier.awbnumber))
                        {
                            qb.AddParameter("awbnumber", "%" + _courier.awbnumber + "%", " ilike ");
                        }

                        if (_courier.paymode>0)
                            qb.AddParameter("courier.paymode", "paymode", true, "=");
                       

                        if (_courier.ishold)
                            qb.AddParameter("courier.ishold", "ishold", true, "=");
                        else
                            qb.AddParameter("courier.ishold", "ishold", false, "=");

                        if (_courier.isbillcopy)
                            qb.AddParameter("courier.isbillcopy", "isbillcopy", true, "=");

                        if (_courier.fromdate.Year > 1900)
                            qb.AddParameter("receivedon", "fromdate", _courier.fromdate.Date, ">=");
                        if (_courier.todate.Year > 1900)
                            qb.AddParameter("receivedon", "todate", _courier.todate.Date.AddDays(1), "<");

                        if (!string.IsNullOrEmpty(_courier.podnumber))
                        {
                            qb.AddParameter("podnumber", "%" + _courier.podnumber + "%", " ilike ");
                        }
                    }

                    //if (!string.IsNullOrEmpty(_courier.Code))
                    //    qb.AddParameter("code", _courier.Code, "=");
                    qb.SortField = "courier.id desc";
                    qb.Limit = 1000;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Courier ocourier = new Courier();
                        ocourier.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        ocourier.invnos = reader["invnos"] == DBNull.Value ? 0 : Convert.ToInt32(reader["invnos"]);
                        ocourier.isbillcopy = reader["isbillcopy"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbillcopy"]);
                        ocourier.ishold = reader["ishold"] == DBNull.Value ? false : Convert.ToBoolean(reader["ishold"]);
                        ocourier.paymode = reader["paymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymode"]);
                        ocourier.copybills = reader["copybills"] == DBNull.Value ? 0 : Convert.ToInt32(reader["copybills"]);
                        ocourier.lrnos = reader["lrnos"] == DBNull.Value ? 0 : Convert.ToInt32(reader["lrnos"]);
                        ocourier.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        ocourier.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        ocourier.attachment_json = reader["attachment"] == DBNull.Value ? null : reader["attachment"].ToString();
                        ocourier.receivingcompanyid = reader["receivingcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivingcompanyid"]);
                        ocourier.sentby = reader["sentby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sentby"]);
                        ocourier.senton = reader["senton"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["senton"]);
                        ocourier.podlist_json = reader["lrinfo"] == DBNull.Value ? null : reader["lrinfo"].ToString();
                        ocourier.deliveredto = reader["deliveredto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliveredto"]);
                        ocourier.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        ocourier.status = (Courier.CourierStatus)(reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]));
                        ocourier.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ocourier.awbnumber = reader["awbnumber"] == DBNull.Value ? "" : reader["awbnumber"].ToString();
                        ocourier.podnumber = reader["podnumber"] == DBNull.Value ? "" : reader["podnumber"].ToString();
                        ocourier.type = reader["type"] == DBNull.Value ? 0 : Convert.ToInt64(reader["type"]);
                        ocourier.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        ocourier.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocourier.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocourier.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocourier.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocourier.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocourier.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocourier.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocourier.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocourier.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        ocourier.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocourier.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocourier.poddate = reader["poddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["poddate"]);
                        ocourier.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"]);
                        ocourier.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        ocourier.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        ocourier.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        ocourier.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        ocourier.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        ocourier.bundles = reader["bundles"] == DBNull.Value ? 0 : Convert.ToInt32(reader["bundles"]);
                        ocourier.ewbno = reader["ewbno"] == DBNull.Value ? "" : reader["ewbno"].ToString();
                        ocourier.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        ocourier.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        lst.Add(ocourier);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _courier);
            }
            return lst;
        }

        public Courier Insert(Courier oCourier)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("courier.entryno", 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oCourier.entryno = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oCourier);

                    InboxManager.Instance(Context.OrganizationID).Update(Context, true, oCourier, transaction, conn);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCourier);


            }

            return oCourier;
        }

        public Courier Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Courier oCourier)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.description == null ? "" : oCourier.description;
            command.Parameters.Add("invnos", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.invnos;
            command.Parameters.Add("isbillcopy", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.isbillcopy;
            command.Parameters.Add("copybills", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.copybills;
            command.Parameters.Add("lrnos", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.lrnos;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.entryno == null ? "" : oCourier.entryno;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.agentid;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oCourier.attachment_json == null ? "null" : oCourier.attachment_json;
            command.Parameters.Add("receivingcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.receivingcompanyid;
            command.Parameters.Add("sentby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.sentby;
            command.Parameters.Add("senton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.senton;
            command.Parameters.Add("lrinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCourier.podlist_json == null ? "null" : oCourier.podlist_json;
            command.Parameters.Add("deliveredto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.deliveredto;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.receivedby;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.status;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.supplierid;
            command.Parameters.Add("awbnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.awbnumber == null ? "" : oCourier.awbnumber;
            command.Parameters.Add("podnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.podnumber == null ? "" : oCourier.podnumber;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.type;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.receivedon;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.companyid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.isactive;
            command.Parameters.Add("ishold", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.ishold;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.mode;
            command.Parameters.Add("paymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.paymode;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.name == null ? "" : oCourier.name;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.customerid;
            command.Parameters.Add("poddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.poddate;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.weight;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.rate;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.amount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.taxid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.taxpercentage;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.taxamount;
            command.Parameters.Add("bundles", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.bundles;
            command.Parameters.Add("ewbno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.ewbno == null ? "" : oCourier.ewbno;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.packedby;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCourier.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCourier;
        }

        public Courier Update(Courier oCourier)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oCourier);
                    InboxManager.Instance(Context.OrganizationID).Update(Context, false, oCourier, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCourier);


            }

            return oCourier;
        }

        public Courier Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Courier oCourier)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.description == null ? "" : oCourier.description;
            command.Parameters.Add("invnos", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.invnos;
            command.Parameters.Add("isbillcopy", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.isbillcopy;
            command.Parameters.Add("copybills", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.copybills;
            command.Parameters.Add("lrnos", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.lrnos;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.entryno == null ? "" : oCourier.entryno;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.agentid;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oCourier.attachment_json == null ? "null" : oCourier.attachment_json;
            command.Parameters.Add("receivingcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.receivingcompanyid;
            command.Parameters.Add("sentby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.sentby;
            command.Parameters.Add("senton", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.senton;
            command.Parameters.Add("lrinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCourier.podlist_json == null ? "null" : oCourier.podlist_json;
            command.Parameters.Add("deliveredto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.deliveredto;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.receivedby;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.status;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.supplierid;
            command.Parameters.Add("awbnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.awbnumber == null ? "" : oCourier.awbnumber;
            command.Parameters.Add("podnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.podnumber == null ? "" : oCourier.podnumber;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.type;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.receivedon;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.companyid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.isactive;
            command.Parameters.Add("ishold", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.ishold;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.id;
            command.Parameters.Add("paymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.paymode;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.mode;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.name == null ? "" : oCourier.name;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.customerid;
            command.Parameters.Add("poddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCourier.poddate;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.weight;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.rate;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.amount;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.taxid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.taxpercentage;
            command.Parameters.Add("taxamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCourier.taxamount;
            command.Parameters.Add("bundles", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.bundles;
            command.Parameters.Add("ewbno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCourier.ewbno == null ? "" : oCourier.ewbno;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.packedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCourier.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCourier;
        }


        public bool Delete(Courier oCourier)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCourier);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCourier);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Courier oCourier)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCourier.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCourier.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCourier.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public Courier Print(CourierCreteria _courier)
        {
            var returncourier = Search(new CourierCreteria()
            {
                id = _courier.id
            });
            if (returncourier == null || returncourier.Count == 0)
                return null;

            // Own Company Info
            if (returncourier[0].receivingcompanyid > 0)
            {
                Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
                companyservice.Context = this.Context;
                var comnanylist = companyservice.Search(new CompanyCreteria()
                {
                    id = returncourier[0].receivingcompanyid
                });
                returncourier[0].company = comnanylist[0];
                returncourier[0].company.parseaddress();
            }

            // Supplier Info
            if (returncourier[0].supplierid > 0)
            {
                Data.SuppliercompanyDataService scompanyservice = new SuppliercompanyDataService() { Context = this.Context };
                var suppliercompany = scompanyservice.Search(new SuppliercompanyCreteria()
                {
                    supplierid = returncourier[0].supplierid,
                    isprint = true
                });
                returncourier[0].supplier = suppliercompany[0];
            }

            // Courier Company Info
            if (returncourier[0].companyid > 0)
            {
                Data.TransportDataService transdata = new TransportDataService() { Context = this.Context };
                var translist = transdata.Search(new TransportCreteria()
                {
                    id = returncourier[0].companyid
                });
                if (translist.Count > 0)
                    returncourier[0].transport = translist[0];
            }
            if (returncourier[0].createdby > 0)
            {
                Data.EmployeeDataService empData = new EmployeeDataService() { Context = this.Context };
                var emplist = empData.SearchHeader(new EmployeeCreteria() { id = returncourier[0].employeeid });
                if (emplist.Count > 0)
                    returncourier[0].enteredby = emplist[0].name;
            }
            if (returncourier[0].packedby > 0)
            {
                Data.EmployeeDataService empData = new EmployeeDataService() { Context = this.Context };
                var emplist = empData.SearchHeader(new EmployeeCreteria() { id = returncourier[0].packedby });
                if (emplist.Count > 0)
                    returncourier[0].packedbyname = emplist[0].name;
            }
            if (returncourier[0].sentby > 0)
            {
                Data.EmployeeDataService empData = new EmployeeDataService() { Context = this.Context };
                var emplist = empData.SearchHeader(new EmployeeCreteria() { id = returncourier[0].sentby });
                if (emplist.Count > 0)
                    returncourier[0].bookedby = emplist[0].name;
            }
            return returncourier[0];
        }
    }
}
