using InfyPOS.Common;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Services
{
    public class WidgetDataManager
    {
        private BaseData dataService;
        private long organizationid;
        public WidgetDataManager(long organizationid)
        {
            this.organizationid = organizationid;
            this.dataService = new BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, WidgetDataManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, WidgetDataManager>();

        public static WidgetDataManager Instance(long organizationid)
        {
            WidgetDataManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new WidgetDataManager(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        const string SQL_Insert = @"INSERT INTO public.DashboardData(lastupdateby,widgetid,lastupdate,datadump) 
        VALUES(@lastupdateby,@widgetid,@lastupdate,@datadump) RETURNING id";

        const string SQL_Update = @"UPDATE public.DashboardData SET lastupdateby=@lastupdateby, 
        lastupdate=@lastupdate,datadump=@datadump
        where widgetid=@widgetid";

        public string GetData(long reportid)
        {
            using (var connection = dataService.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = dataService.GetCommand("select datadump from DashboardData where widgetid=@widgetid", connection);
                command.Parameters.Add("widgetid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = reportid;
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        return reader.GetString(0);
                }
                connection.Close();
           }
            return "";
        }

        public void Create(long reportid, long userid, string data)
        {
            try
            {
                using (var connection = dataService.CreateConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = dataService.GetCommand(SQL_Insert, connection);
                    command.Parameters.Add("widgetid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = reportid;
                    command.Parameters.Add("datadump", NpgsqlTypes.NpgsqlDbType.Json).Value = data;
                    command.Parameters.Add("lastupdateby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
                    command.Parameters.Add("lastupdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception exp)
            {
                Update(reportid, userid, data);
            }
        }
        public void Update(long widgetid, long userid, string data)
        {
            using (var connection = dataService.CreateConnection())
            {
                connection.Open();
                NpgsqlCommand command = dataService.GetCommand(SQL_Update, connection);
                command.Parameters.Add("widgetid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = widgetid;
                command.Parameters.Add("datadump", NpgsqlTypes.NpgsqlDbType.Json).Value = data;
                command.Parameters.Add("lastupdateby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
                command.Parameters.Add("lastupdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                if (command.ExecuteNonQuery() == 0)
                {
                    command = dataService.GetCommand(SQL_Insert, connection);
                    command.Parameters.Add("widgetid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = widgetid;
                    command.Parameters.Add("datadump", NpgsqlTypes.NpgsqlDbType.Json).Value = data;
                    command.Parameters.Add("lastupdateby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
                    command.Parameters.Add("lastupdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

        }
    }
}