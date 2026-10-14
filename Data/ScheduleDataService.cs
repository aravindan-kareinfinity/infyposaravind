using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ScheduleDataService : SAASData
    {
        const string SQL_Search = @"select id, name, description, config, enabled, isactionbased, isdaily, ismonthly, isweekly, version, createdon, modifiedon, modifiedby, createdby, isactive from schedule where id=@id";


        const string SQL_Insert = @"INSERT INTO public.schedule(name, description, config, enabled, isactionbased, isdaily, ismonthly, isweekly, version, createdon, modifiedon, modifiedby, createdby, isactive) VALUES(@name, @description, @config, @enabled, @isactionbased, @isdaily, @ismonthly, @isweekly, @version, @createdon, @modifiedon, @modifiedby, @createdby, @isactive) RETURNING id";


        const string SQL_Update = @"UPDATE public.schedule SET name=@name, description=@description, config=@config, enabled=@enabled, isactionbased=@isactionbased, isdaily=@isdaily, ismonthly=@ismonthly, isweekly=@isweekly, version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.schedule set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Schedule> Search(ScheduleCreteria _schedule)
        {
            List<Schedule> lst = new List<Schedule>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_schedule.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _schedule.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if(_schedule.onlyactionable)
                        qb.AddParameter("isactionbased", true, "=");

                    if (!_schedule.onlyactionable && _schedule.onlyreports)
                        qb.AddParameter("isactionbased", false, "=");

                    if (!string.IsNullOrEmpty(_schedule.name) && _schedule.name != "AllSchedule")
                        qb.AddParameter("name", _schedule.name, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Schedule oschedule = new Schedule();
                        oschedule.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oschedule.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oschedule.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oschedule.config_json = reader["config"] == DBNull.Value ? null : reader["config"].ToString();
                        oschedule.enabled = reader["enabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["enabled"]);
                        oschedule.isactionbased = reader["isactionbased"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactionbased"]);
                        oschedule.isdaily = reader["isdaily"] == DBNull.Value ? false : Convert.ToBoolean(reader["isdaily"]);
                        oschedule.ismonthly = reader["ismonthly"] == DBNull.Value ? false : Convert.ToBoolean(reader["ismonthly"]);
                        oschedule.isweekly = reader["isweekly"] == DBNull.Value ? false : Convert.ToBoolean(reader["isweekly"]);
                        oschedule.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oschedule.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oschedule.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oschedule.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oschedule.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oschedule.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        lst.Add(oschedule);
                    }
                }
                if ((_schedule.id > 0 || !string.IsNullOrEmpty(_schedule.name)) && lst.Count == 1)
                {
                    ScheduleditemsDataService subScheduleditems = new ScheduleditemsDataService() { Context = this.Context };
                    lst[0].Scheduleditems = subScheduleditems.SearchAll(_schedule.id);
                    if (lst[0].config == null)
                        lst[0].config = new Schedule.Config();
                    lst[0].config.PrepareDaily(lst[0].Scheduleditems);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _schedule);
            }
            return lst;
        }

        public Schedule Insert(Schedule oSchedule)
        {

           

            NpgsqlTransaction transaction = null;
            try
            {
                if (oSchedule.config != null)
                {
                    oSchedule.ismonthly = oSchedule.config.monthly != "AllDay";
                    if (!oSchedule.ismonthly)
                    {
                        oSchedule.isweekly = oSchedule.config.wk_0 || oSchedule.config.wk_1 || oSchedule.config.wk_2 || oSchedule.config.wk_3 ||
                            oSchedule.config.wk_4 || oSchedule.config.wk_5 || oSchedule.config.wk_6;
                    }
                    oSchedule.isdaily = !oSchedule.ismonthly && !oSchedule.isweekly;
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    if (oSchedule.config == null) oSchedule.config = new Schedule.Config();
                    oSchedule.Scheduleditems = oSchedule.config.BuildDaily(oSchedule.Scheduleditems);

                    Insert(transaction, conn, oSchedule);
                    ScheduleditemsDataService subScheduleditems = new ScheduleditemsDataService() { Context = this.Context };
                    
                    foreach (var item in oSchedule.Scheduleditems)
                    {
                        item.scheduleid = oSchedule.id;
                        subScheduleditems.Insert(transaction, conn, item);
                    }

                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oSchedule.id,
                        type = "CACHE",
                        subtype = "SCHEDULE",
                        actionon = oSchedule.modifiedon
                    }, conn);
                    transaction.Commit();
                    conn.Close();
                    Processors.Schedule_Actions.Instance.ClearSchedule(Context.OrganizationID);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSchedule);
                throw exp;
            }

            return oSchedule;
        }

        public Schedule Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Schedule oSchedule)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchedule.name == null ? "" : oSchedule.name;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchedule.description == null ? "" : oSchedule.description;
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oSchedule.config_json == null ? "null" : oSchedule.config_json;
            command.Parameters.Add("enabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.enabled;
            command.Parameters.Add("isactionbased", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isactionbased;
            command.Parameters.Add("isdaily", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isdaily;
            command.Parameters.Add("ismonthly", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.ismonthly;
            command.Parameters.Add("isweekly", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isweekly;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSchedule.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isactive;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSchedule.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSchedule;
        }

        public Schedule Update(Schedule oSchedule)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (oSchedule.config != null)
                {
                    oSchedule.ismonthly = oSchedule.config.monthly != "AllDay";
                    if (!oSchedule.ismonthly)
                    {
                        oSchedule.isweekly = oSchedule.config.wk_0 || oSchedule.config.wk_1 || oSchedule.config.wk_2 || oSchedule.config.wk_3 ||
                            oSchedule.config.wk_4 || oSchedule.config.wk_5 || oSchedule.config.wk_6;
                    }
                    oSchedule.isdaily = !oSchedule.ismonthly && !oSchedule.isweekly;
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oSchedule.config == null) oSchedule.config = new Schedule.Config();
                    oSchedule.Scheduleditems = oSchedule.config.BuildDaily(oSchedule.Scheduleditems);

                    Update(transaction, conn, oSchedule);
                    ScheduleditemsDataService subScheduleditems = new ScheduleditemsDataService() { Context = this.Context };
                    subScheduleditems.Update(transaction, conn, oSchedule.Scheduleditems, oSchedule.id);

                    InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                    {
                        id = oSchedule.id,
                        type = "CACHE",
                        subtype = "SCHEDULE",
                        actionon = oSchedule.modifiedon
                    }, conn);


                    transaction.Commit();
                    conn.Close();
                    Processors.Schedule_Actions.Instance.ClearSchedule(Context.OrganizationID);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSchedule);
                throw exp;
            }

            return oSchedule;
        }

        public Schedule Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Schedule oSchedule)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchedule.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchedule.name == null ? "" : oSchedule.name;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSchedule.description == null ? "" : oSchedule.description;
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oSchedule.config_json == null ? "null" : oSchedule.config_json;
            command.Parameters.Add("enabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.enabled;
            command.Parameters.Add("isactionbased", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isactionbased;
            command.Parameters.Add("isdaily", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isdaily;
            command.Parameters.Add("ismonthly", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.ismonthly;
            command.Parameters.Add("isweekly", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isweekly;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSchedule.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSchedule.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isactive;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSchedule.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSchedule;
        }


        public bool Delete(Schedule oSchedule)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSchedule);
                    ScheduleditemsDataService subScheduleditems = new ScheduleditemsDataService() { Context = this.Context };
                    subScheduleditems.DeleteAll(oSchedule.id, oSchedule.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSchedule);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Schedule oSchedule)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSchedule.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchedule.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSchedule.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSchedule.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSchedule.id;
            int successCount = command.ExecuteNonQuery();
            if(successCount > 0)
            {
                Processors.Schedule_Actions.Instance.ClearSchedule(Context.OrganizationID);
                return true;
            }
            return false;
        }

    }

}
