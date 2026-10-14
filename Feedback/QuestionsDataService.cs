using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class QuestionsDataService : BaseData
    {
        const string SQL_Search = @"select enable, index, answercount, chartposition, question, charttype, answerlist, code, supportanalytical, supportacknoloedge, isactive, createdby, modifiedby, version, activefrom, activeto, createdon, modifiedon, id from fbquestions where id=@id";
        const string SQL_Insert = @"INSERT INTO public.fbquestions(enable, index, answercount, chartposition, question, charttype, answerlist, code, supportanalytical, supportacknoloedge, isactive, createdby, modifiedby, version, activefrom, activeto, createdon, modifiedon) VALUES(@enable, @index, @answercount, @chartposition, @question, @charttype, @answerlist, @code, @supportanalytical, @supportacknoloedge, @isactive, @createdby, @modifiedby, @version, @activefrom, @activeto, @createdon, @modifiedon) RETURNING id";

        internal List<FBQuestions> SearchAll()
        {
            return Search(new QuestionsCreteria()
            {
            }).OrderBy(e => e.index).ToList();
        }

        const string SQL_Update = @"UPDATE public.fbquestions SET enable=@enable, index=@index, answercount=@answercount, chartposition=@chartposition, question=@question, charttype=@charttype, answerlist=@answerlist, code=@code, supportanalytical=@supportanalytical, supportacknoloedge=@supportacknoloedge, modifiedby=@modifiedby, version=@version+1, activefrom=@activefrom, activeto=@activeto, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.fbquestions set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<FBQuestions> Search(QuestionsCreteria _questions)
        {
            List<FBQuestions> lst = new List<FBQuestions>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_questions.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _questions.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FBQuestions oquestions = new FBQuestions();
                        oquestions.enable = reader["enable"] == DBNull.Value ? false : Convert.ToBoolean(reader["enable"]);
                        oquestions.index = reader["index"] == DBNull.Value ? 0 : Convert.ToInt32(reader["index"]);
                        oquestions.answercount = reader["answercount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["answercount"]);
                        oquestions.chartposition = reader["chartposition"] == DBNull.Value ? 0 : Convert.ToInt32(reader["chartposition"]);
                        oquestions.question = reader["question"] == DBNull.Value ? "" : reader["question"].ToString();
                        oquestions.charttype = reader["charttype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["charttype"]);
                        oquestions.answerlist_json = reader["answerlist"] == DBNull.Value ? null : reader["answerlist"].ToString();
                        oquestions.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oquestions.supportanalytical = reader["supportanalytical"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportanalytical"]);
                        oquestions.supportacknoloedge = reader["supportacknoloedge"] == DBNull.Value ? false : Convert.ToBoolean(reader["supportacknoloedge"]);
                        oquestions.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oquestions.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oquestions.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oquestions.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oquestions.activefrom = reader["activefrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activefrom"]);
                        oquestions.activeto = reader["activeto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activeto"]);
                        oquestions.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oquestions.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oquestions.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(oquestions);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _questions);
            }
            return lst;
        }

        internal bool Update(QuestionChanges item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if(item.type == "enable")
                    {
                        var qitem = Search(new QuestionsCreteria()
                        {
                            id = item.question.id
                        }).First();
                        qitem.enable = true;
                        Update(transaction, conn, qitem);
                    }
                    if (item.type == "disable")
                    {
                        var qitem = Search(new QuestionsCreteria()
                        {
                            id = item.question.id
                        }).First();
                        qitem.enable = false;
                        Update(transaction, conn, qitem);
                    }

                    if (item.type == "movedown")
                    {
                        var selected = Search(new QuestionsCreteria()
                        {
                            id = item.selected.id
                        }).First();

                        var next = Search(new QuestionsCreteria()
                        {
                            id = item.next.id
                        }).First();

                        var selectedindex = selected.index;
                        selected.index = next.index;
                        Update(transaction, conn, selected);
                        next.index = selectedindex;
                        Update(transaction, conn, next);
                    }

                    if (item.type == "moveup")
                    {
                        var selected = Search(new QuestionsCreteria()
                        {
                            id = item.selected.id
                        }).First();

                        var previous = Search(new QuestionsCreteria()
                        {
                            id = item.previous.id
                        }).First();

                        var selectedindex = selected.index;
                        selected.index = previous.index;
                        Update(transaction, conn, selected);
                        previous.index = selectedindex;
                        Update(transaction, conn, previous);
                    }

                    transaction.Commit();

                    conn.Close();
                    return true;
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return false;
            }
        }

        public FBQuestions Insert(FBQuestions oQuestions)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oQuestions);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oQuestions);
            }

            return oQuestions;
        }

        public FBQuestions Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, FBQuestions oQuestions)
        {
            if (oQuestions.answerlist != null && oQuestions.answerlist.Count > 0)
            {
                int index = 0;
                oQuestions.answerlist.ForEach(e => e.id = index++);
            }
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("enable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.enable;
            command.Parameters.Add("index", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.index;
            command.Parameters.Add("answercount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.answercount;
            command.Parameters.Add("chartposition", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.chartposition;
            command.Parameters.Add("question", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oQuestions.question == null ? "" : oQuestions.question;
            command.Parameters.Add("charttype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.charttype;
            command.Parameters.Add("answerlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oQuestions.answerlist_json == null ? "null" : oQuestions.answerlist_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oQuestions.code == null ? "" : oQuestions.code;
            command.Parameters.Add("supportanalytical", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.supportanalytical;
            command.Parameters.Add("supportacknoloedge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.supportacknoloedge;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.version;
            command.Parameters.Add("activefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.activefrom;
            command.Parameters.Add("activeto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.activeto;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oQuestions.id = Convert.ToInt64(reader[0].ToString());
                }
            }


            return oQuestions;
        }

        public FBQuestions Update(FBQuestions oQuestions)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oQuestions);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oQuestions);
            }
            return oQuestions;
        }

        public FBQuestions Update(NpgsqlTransaction transaction, NpgsqlConnection conn, FBQuestions oQuestions)
        {
            if (oQuestions.answerlist != null && oQuestions.answerlist.Count > 0)
            {
                if (oQuestions.answerlist.Exists(e => e.id > 1000))
                {
                    long index = oQuestions.answerlist.All(e=>e.id>1000) ? 1 : oQuestions.answerlist.FindAll(e => e.id < 1000).Max(e => e.id)+1;
                    oQuestions.answerlist.ForEach(e => e.id = index++);
                }
            }

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("enable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.enable;
            command.Parameters.Add("index", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.index;
            command.Parameters.Add("answercount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.answercount;
            command.Parameters.Add("chartposition", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.chartposition;
            command.Parameters.Add("question", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oQuestions.question == null ? "" : oQuestions.question;
            command.Parameters.Add("charttype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.charttype;
            command.Parameters.Add("answerlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oQuestions.answerlist_json == null ? "null" : oQuestions.answerlist_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oQuestions.code == null ? "" : oQuestions.code;
            command.Parameters.Add("supportanalytical", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.supportanalytical;
            command.Parameters.Add("supportacknoloedge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.supportacknoloedge;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.version;
            command.Parameters.Add("activefrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.activefrom;
            command.Parameters.Add("activeto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.activeto;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oQuestions.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oQuestions;
        }


        public bool Delete(FBQuestions oQuestions)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oQuestions);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oQuestions);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, FBQuestions oQuestions)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oQuestions.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oQuestions.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oQuestions.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oQuestions.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
