using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class FeedbackDataService : BaseData
    {
        const string SQL_Search = @"select mobileno, name, area, referenceid, feedbackdate, acknoledgedon, acknoledgedby, customerreason, feedbackstatus, requiredackowledge, actiontaken, qafeedback, overrallrating, id, hascomplaint, hassugession, complaintsuggesion from fbanswer where id=@id";


        const string SQL_Insert = @"INSERT INTO public.fbanswer( mobileno, name, area, referenceid, feedbackdate, acknoledgedon, acknoledgedby, customerreason, feedbackstatus, requiredackowledge, actiontaken, qafeedback, overrallrating, hascomplaint, hassugession, complaintsuggesion) VALUES(@mobileno, @name, @area, @referenceid, @feedbackdate, @acknoledgedon, @acknoledgedby, @customerreason, @feedbackstatus, @requiredackowledge, @actiontaken, @qafeedback, @overrallrating, @hascomplaint, @hassugession, @complaintsuggesion) RETURNING id";


        const string SQL_Update = @"UPDATE public.fbanswer SET mobileno=@mobileno, name=@name, area=@area, referenceid=@referenceid, feedbackdate=@feedbackdate, acknoledgedon=@acknoledgedon, acknoledgedby=@acknoledgedby, customerreason=@customerreason, feedbackstatus=@feedbackstatus, requiredackowledge=@requiredackowledge, actiontaken=@actiontaken, qafeedback=@qafeedback, overrallrating=@overrallrating, hascomplaint=@hascomplaint, hassugession=@hassugession, complaintsuggesion=@complaintsuggesion where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"delete from public.fbanswer where id=@id";

        public RatingSummary GetRatings()
        {
            var lst = new RatingSummary();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"
                select count(*) cnt,overrallrating from fbanswer where feedbackdate>=@yfrom and feedbackdate<@yto group by overrallrating;
                select count(*) cnt,overrallrating from fbanswer where feedbackdate>=@mfrom and feedbackdate<@mto group by overrallrating;
                select count(*) cnt,overrallrating from fbanswer where feedbackdate>=@wfrom and feedbackdate<@wto group by overrallrating;";

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("yfrom", DateTime.Now.Date.AddYears(-1));
                command.Parameters.AddWithValue("yto", DateTime.Now.AddDays(1));

                command.Parameters.AddWithValue("mfrom", DateTime.Now.Date.AddMonths(-1));
                command.Parameters.AddWithValue("mto", DateTime.Now.AddDays(1));

                command.Parameters.AddWithValue("wfrom", DateTime.Now.Date.AddDays(-7));
                command.Parameters.AddWithValue("wto", DateTime.Now.AddDays(1));
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    var year = new List<Rating>();
                    var month = new List<Rating>();
                    var week = new List<Rating>();
                    while (reader.Read())
                    {
                        year.Add(new Rating()
                        {
                            rate = reader.GetInt32(1),
                            value = reader.GetInt32(0)
                        });
                    }
                    lst.ydata = year.ConvertAll(e => e.value);
                    lst.ylabels = year.ConvertAll(e => e.rate.ToString("f0"));

                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            month.Add(new Rating()
                            {
                                rate = reader.GetInt32(1),
                                value = reader.GetInt32(0)
                            });
                        }

                    lst.mdata = month.ConvertAll(e => e.value);
                    lst.mlabels = month.ConvertAll(e => e.rate.ToString("f0"));


                    if (reader.NextResult())
                        while (reader.Read())
                        {
                            week.Add(new Rating()
                            {
                                rate = reader.GetInt32(1),
                                value = reader.GetInt32(0)
                            });
                        }

                    lst.wdata = week.ConvertAll(e => e.value);
                    lst.wlabels = week.ConvertAll(e => e.rate.ToString("f0"));

                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
            }
            return lst;
        }

        public FBRegistration Register(FBRegistration item)
        {
            if (item.Customer == null)
                throw new Exception("Please enter customer details");
            if (string.IsNullOrEmpty(item.Customer.mobileno) || string.IsNullOrEmpty(item.Customer.name))
                throw new Exception("Please enter customer Name and Mobile no");

            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            if (!string.IsNullOrEmpty(item.Customer.mobileno))
            {
                var customer = customerDataService.Search(new CustomerCreteria()
                {
                    mobileno = item.Customer.mobileno
                });
                if (customer.Count > 0)
                {
                    item.Customer.id = customer.First().id;
                }
                else if (!string.IsNullOrEmpty(item.Customer.name))
                {
                    item.Customer.id  = customerDataService.Insert(new Customer()
                    {
                        mobileno = item.Customer.mobileno,
                        name = item.Customer.name,
                        dateofbirth = item.Customer.dateofbirth,
                        address = item.Customer.address,
                        area = item.Customer.area,
                        registredlocationid = 0,
                        customertype = 4,
                        isactive = true,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        version = 1
                    }).id;
                }
            }

            string SQL_UpdateIssueto = "update vouchers set issueto=@issueto,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version ";
            if (item.coupen != null && !string.IsNullOrEmpty(item.coupen.code))
            {
                CustomerCouponsDataService VouchersData = new CustomerCouponsDataService();
                List<CustomerCoupons> lstVouchers = VouchersData.Search(new CustomerCouponsCreteria() { code = item.coupen.code });

                if (lstVouchers != null && lstVouchers.Count > 0)
                {
                    if (lstVouchers[0].issueto > 0)
                        throw new Exception("This coupon already issued to someother customers.");

                    NpgsqlTransaction transaction = null;
                    int successCount = 0;
                    try
                    {
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            NpgsqlCommand command = GetCommand(SQL_UpdateIssueto, conn, transaction);
                            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = lstVouchers[0].isactive;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = lstVouchers[0].version;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lstVouchers[0].id;
                            command.Parameters.Add("issueto", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.Customer.id;
                            successCount = command.ExecuteNonQuery();
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                    catch (Exception exp)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    }
                }
            }
            return item;
        }

        public FeedbackQASummary GetChartData(DateTime from, DateTime to)
        {
            var lst = new List<FeedbackQA>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"
                select cast((schemelist ->> 'question') as varchar) question,
                cast((schemelist ->> 'answer') as varchar) answer,
                count(*) count from fbanswer
                LEFT JOIN LATERAL
                json_array_elements (fbanswer.qafeedback :: json) schemelist ON true
                where feedbackdate>=@from and feedbackdate<@to
                group by cast((schemelist ->> 'question') as varchar),
                cast((schemelist ->> 'answer') as varchar)";

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("from", from.Date);
                command.Parameters.AddWithValue("to", to.Date.AddDays(1));

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        string question = reader.GetString(0);
                        string answer = reader.IsDBNull(1) ? "-" : reader.GetString(1);
                        int count = reader.GetInt32(2);
                        if (!lst.Exists(e => e.question == question))
                        {
                            lst.Add(new FeedbackQA()
                            {
                                question = question,
                                answers = new List<FeedbackQA.answercount>()
                            });
                        }
                        lst.Find(e => e.question == question).answers.Add(new FeedbackQA.answercount()
                        {
                            answer = answer,
                            count = count
                        });
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
            }
            return new FeedbackQASummary(lst);
        }
        public List<FBAnswer> Search(FeedbackCreteria _feedback)
        {
            List<FBAnswer> lst = new List<FBAnswer>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_feedback.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _feedback.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_feedback.Code))
                    //    qb.AddParameter("code", _feedback.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FBAnswer ofeedback = new FBAnswer();
                        ofeedback.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        ofeedback.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ofeedback.area = reader["area"] == DBNull.Value ? "" : reader["area"].ToString();
                        ofeedback.referenceid = reader["referenceid"] == DBNull.Value ? "" : reader["referenceid"].ToString();
                        ofeedback.feedbackdate = reader["feedbackdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["feedbackdate"]);
                        ofeedback.acknoledgedon = reader["acknoledgedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["acknoledgedon"]);
                        ofeedback.acknoledgedby = reader["acknoledgedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["acknoledgedby"]);
                        ofeedback.customerreason = reader["customerreason"] == DBNull.Value ? "" : reader["customerreason"].ToString();
                        ofeedback.feedbackstatus = reader["feedbackstatus"] == DBNull.Value ? 0 : Convert.ToInt32(reader["feedbackstatus"]);
                        ofeedback.requiredackowledge = reader["requiredackowledge"] == DBNull.Value ? false : Convert.ToBoolean(reader["requiredackowledge"]);
                        ofeedback.actiontaken = reader["actiontaken"] == DBNull.Value ? "" : reader["actiontaken"].ToString();
                        ofeedback.qafeedback_json = reader["qafeedback"] == DBNull.Value ? null : reader["qafeedback"].ToString();
                        ofeedback.complaintsuggesion_json = reader["complaintsuggesion"] == DBNull.Value ? null : reader["complaintsuggesion"].ToString();
                        ofeedback.overrallrating = reader["overrallrating"] == DBNull.Value ? 0 : Convert.ToInt32(reader["overrallrating"]);
                        ofeedback.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ofeedback.hascomplaint = reader["hascomplaint"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascomplaint"]);
                        ofeedback.hassugession = reader["hassugession"] == DBNull.Value ? false : Convert.ToBoolean(reader["hassugession"]);

                        lst.Add(ofeedback);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _feedback);
            }
            return lst;
        }

        public FBAnswer Insert(FBAnswer oFeedback)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oFeedback);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oFeedback);
            }

            return oFeedback;
        }

        public FBAnswer Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, FBAnswer oFeedback)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.mobileno == null ? "" : oFeedback.mobileno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.name == null ? "" : oFeedback.name;
            command.Parameters.Add("area", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.area == null ? "" : oFeedback.area;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.referenceid == null ? "" : oFeedback.referenceid;
            command.Parameters.Add("feedbackdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oFeedback.feedbackdate;
            command.Parameters.Add("acknoledgedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oFeedback.acknoledgedon;
            command.Parameters.Add("acknoledgedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFeedback.acknoledgedby;
            command.Parameters.Add("customerreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.customerreason == null ? "" : oFeedback.customerreason;
            command.Parameters.Add("feedbackstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oFeedback.feedbackstatus;
            command.Parameters.Add("requiredackowledge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFeedback.requiredackowledge;
            command.Parameters.Add("actiontaken", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.actiontaken == null ? "" : oFeedback.actiontaken;
            command.Parameters.Add("qafeedback", NpgsqlTypes.NpgsqlDbType.Json).Value = oFeedback.qafeedback_json == null ? "null" : oFeedback.qafeedback_json;
            command.Parameters.Add("overrallrating", NpgsqlTypes.NpgsqlDbType.Integer).Value = oFeedback.overrallrating;
            command.Parameters.Add("hascomplaint", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFeedback.hascomplaint;
            command.Parameters.Add("hassugession", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFeedback.hassugession;
            command.Parameters.Add("complaintsuggesion", NpgsqlTypes.NpgsqlDbType.Json).Value = oFeedback.complaintsuggesion_json == null ? "null" : oFeedback.complaintsuggesion_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oFeedback.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oFeedback;
        }

        public RPTSummary SearchDetail(ReportCreteria reportCreteria)
        {
            if (reportCreteria.List == null) reportCreteria.List = new List<ReportCreteria.qa>();
            RPTSummary rPTSummary = new RPTSummary();

            if (reportCreteria.fromdate == DateTime.MinValue)
                reportCreteria.fromdate = DateTime.Now.AddDays(-30).Date;
            if (reportCreteria.todate == DateTime.MinValue)
                reportCreteria.todate = DateTime.Now.Date;
            if (reportCreteria.fromrating == 0)
                reportCreteria.fromrating = 1;
            if (reportCreteria.torating == 0)
                reportCreteria.torating = 10;

            string query = @"select fbanswer.id from fbanswer
                LEFT JOIN LATERAL
                json_array_elements (fbanswer.qafeedback :: json) schemelist ON true ";

            string condition = @" where feedbackdate>=@fromdate and feedbackdate<@todate
                and overrallrating>=@fromrating and overrallrating<@torating";

            if (reportCreteria.hascomplaint)
                condition += " and hascomplaint=true ";

            if (reportCreteria.hassuggesion)
                condition += " and hassuggesion=true ";

            List<string> uniqueids = new List<string>();
            foreach (var item in reportCreteria.List.GroupBy(e => e.questionid))
            {
                uniqueids.AddRange(item.ToList().ConvertAll(e => string.Format("'{0}_{1}'", e.questionid, e.answerid)));
            }

            if (uniqueids.Count > 0)
                condition += string.Format(" and cast((schemelist ->> 'uniqueid') as varchar) in ({0})", string.Join(",", uniqueids));

            string summaryquery = string.Format(@"select cast((schemelist ->> 'question') as varchar) question,
                cast((schemelist ->> 'answer') as varchar) answer,
                count(*) count from fbanswer
                LEFT JOIN LATERAL
                json_array_elements (fbanswer.qafeedback :: json) schemelist ON true
                where fbanswer.id in ({0} {1})
                group by cast((schemelist ->> 'question') as varchar),
                cast((schemelist ->> 'answer') as varchar)", query, condition);

            string detailquery = string.Format(@"select id,mobileno,name,feedbackdate,overrallrating,qafeedback,hascomplaint,hassugession,complaintsuggesion,
                cast(complaintsuggesion ->> 'complaint' as character varying(200)) complaint,
                cast(complaintsuggesion ->> 'suggesion' as character varying(200)) suggesion from fbanswer
                where fbanswer.id in ({0})", summaryquery);

            if (reportCreteria.List.Count == 0)
            {
                detailquery = string.Format(@"select id,mobileno,name,feedbackdate,overrallrating,qafeedback,hascomplaint,hassugession,complaintsuggesion,
                cast(complaintsuggesion ->> 'complaint' as character varying(200)) complaint,
                cast(complaintsuggesion ->> 'suggesion' as character varying(200)) suggesion from fbanswer 
                {0}", condition);
            }

            if (reportCreteria.rpttype == "DETAIL")
            {
                NpgsqlConnection conn = CreateConnection();
                try
                {
                    conn.Open();
                    NpgsqlCommand command = null;
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = detailquery;

                    command = qb.GetCommand(conn);

                    if (reportCreteria.fromdate > DateTime.MinValue)
                        command.Parameters.AddWithValue("fromdate", reportCreteria.fromdate);
                    if (reportCreteria.todate > DateTime.MinValue)
                        command.Parameters.AddWithValue("todate", reportCreteria.todate.AddDays(1).Date);

                    if (reportCreteria.fromrating > 0)
                        command.Parameters.AddWithValue("fromrating", reportCreteria.fromrating);
                    if (reportCreteria.torating > 0)
                        command.Parameters.AddWithValue("torating", reportCreteria.torating + 1);

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        rPTSummary.detailitems = new List<RPTSummary.Detail>();
                        while (reader.Read())
                        {
                            RPTSummary.Detail detail = new RPTSummary.Detail();
                            detail.feedbackdate = reader["feedbackdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["feedbackdate"]);
                            detail.hascomplaint = reader["hascomplaint"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascomplaint"]);
                            detail.hassugession = reader["hassugession"] == DBNull.Value ? false : Convert.ToBoolean(reader["hassugession"]);
                            detail.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            detail.mobileno = reader["mobileno"] == DBNull.Value ? "" : Convert.ToString(reader["mobileno"]);
                            detail.name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                            detail.complaint = reader["complaint"] == DBNull.Value ? "" : Convert.ToString(reader["complaint"]);
                            detail.sugession = reader["suggesion"] == DBNull.Value ? "" : Convert.ToString(reader["suggesion"]);
                            detail.overrallrating = reader["overrallrating"] == DBNull.Value ? 0 : Convert.ToInt64(reader["overrallrating"]);
                            detail.qafeedback_json = reader["qafeedback"] == DBNull.Value ? null : reader["qafeedback"].ToString();
                            detail.complaintsuggesion_json = reader["complaintsuggesion"] == DBNull.Value ? "null" : reader["complaintsuggesion"].ToString();
                            rPTSummary.detailitems.Add(detail);
                        }
                        rPTSummary.columns = new List<string>();
                        rPTSummary.detailitems.ForEach(e =>
                        {
                            if (e.qafeedback.Any(xx => !rPTSummary.columns.Contains(xx.question)))
                            {
                                e.qafeedback.ForEach(qa =>
                                {
                                    if (!rPTSummary.columns.Exists(ex => ex == qa.question))
                                        rPTSummary.columns.Add(qa.question);
                                });
                            }
                        });
                    }
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                }

            }
            else
            {

                NpgsqlConnection conn = CreateConnection();
                try
                {
                    conn.Open();

                    NpgsqlCommand command = null;
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = summaryquery;


                    command = qb.GetCommand(conn);

                    if (reportCreteria.fromdate > DateTime.MinValue)
                        command.Parameters.AddWithValue("fromdate", reportCreteria.fromdate);
                    if (reportCreteria.todate > DateTime.MinValue)
                        command.Parameters.AddWithValue("todate", reportCreteria.todate.AddDays(1).Date);

                    if (reportCreteria.fromrating > 0)
                        command.Parameters.AddWithValue("fromrating", reportCreteria.fromrating);
                    if (reportCreteria.torating > 0)
                        command.Parameters.AddWithValue("torating", reportCreteria.torating + 1);

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        rPTSummary.summaryitems = new List<RPTSummary.Summary>();
                        while (reader.Read())
                        {
                            RPTSummary.Summary detail = new RPTSummary.Summary();
                            detail.answer = reader["answer"] == DBNull.Value ? "" : Convert.ToString(reader["answer"]);
                            detail.question = reader["question"] == DBNull.Value ? "" : Convert.ToString(reader["question"]);
                            detail.count = reader["count"] == DBNull.Value ? 0 : Convert.ToInt32(reader["count"]);
                            rPTSummary.summaryitems.Add(detail);
                        }
                    }
                    rPTSummary.summaryitems = rPTSummary.summaryitems.OrderBy(e => e.question).ToList();
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                }
            }

            return rPTSummary;
        }
        public FBAnswer Update(FBAnswer oFeedback)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oFeedback);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oFeedback);
            }

            return oFeedback;
        }

        public FBAnswer Update(NpgsqlTransaction transaction, NpgsqlConnection conn, FBAnswer oFeedback)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("mobileno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.mobileno == null ? "" : oFeedback.mobileno;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.name == null ? "" : oFeedback.name;
            command.Parameters.Add("area", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.area == null ? "" : oFeedback.area;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.referenceid == null ? "" : oFeedback.referenceid;
            command.Parameters.Add("feedbackdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oFeedback.feedbackdate;
            command.Parameters.Add("acknoledgedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oFeedback.acknoledgedon;
            command.Parameters.Add("acknoledgedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFeedback.acknoledgedby;
            command.Parameters.Add("customerreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.customerreason == null ? "" : oFeedback.customerreason;
            command.Parameters.Add("feedbackstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oFeedback.feedbackstatus;
            command.Parameters.Add("requiredackowledge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFeedback.requiredackowledge;
            command.Parameters.Add("actiontaken", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oFeedback.actiontaken == null ? "" : oFeedback.actiontaken;
            command.Parameters.Add("qafeedback", NpgsqlTypes.NpgsqlDbType.Json).Value = oFeedback.qafeedback_json == null ? "null" : oFeedback.qafeedback_json;
            command.Parameters.Add("overrallrating", NpgsqlTypes.NpgsqlDbType.Integer).Value = oFeedback.overrallrating;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFeedback.id;
            command.Parameters.Add("hascomplaint", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFeedback.hascomplaint;
            command.Parameters.Add("hassugession", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oFeedback.hassugession;
            command.Parameters.Add("complaintsuggesion", NpgsqlTypes.NpgsqlDbType.Json).Value = oFeedback.complaintsuggesion_json == null ? "null" : oFeedback.complaintsuggesion_json;

            command.ExecuteNonQuery();

            return oFeedback;
        }


        public bool Delete(FBAnswer oFeedback)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oFeedback);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oFeedback);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, FBAnswer oFeedback)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oFeedback.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
