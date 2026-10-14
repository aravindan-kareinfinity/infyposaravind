using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Services;

namespace InfyPOS.Data

{
    public class ConfigurationDataService : SAASData
    {
        const string SQL_Search = @"select configuration.id, userid, configuration.key, reportformat, configuration.organizationid, configuration.version, 
        configuration.isactive, configuration.modifiedon, configuration.createdon, configuration.createdby, configuration.freezedate,
        configuration.modifiedby,configuration.name,configuration.nooflines,configuration.isdefault,configuration.locationid,
        configuration.noofcopy,configuration.nextdaycount,configuration.currentday,configuration.instancename 
        from configuration
        left outer join referencelist on referencelist.id = locationid 
        where configuration.id=@id";

        const string SQL_Insert = @"INSERT INTO public.configuration(userid, key, reportformat, organizationid, version, isactive, modifiedon, 
        createdon, createdby, modifiedby,name,nooflines,isdefault,locationid,noofcopy,instancename) 
        VALUES(@userid, @key, @reportformat, @organizationid, @version, @isactive, @modifiedon, @createdon, @createdby, @modifiedby,@name,
        @nooflines,@isdefault,@locationid,@noofcopy,@instancename) RETURNING id";

        const string SQL_Update = @"UPDATE public.configuration SET userid=@userid, key=@key, instancename=@instancename, reportformat=@reportformat, 
        organizationid=@organizationid, version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby,name=@name,nooflines=@nooflines,
        isdefault=@isdefault,locationid=@locationid,noofcopy=@noofcopy where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.configuration set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Configuration> Search(ConfigurationCreteria _configuration)
        {

            List<Configuration> lst = new List<Configuration>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                try
                {
                    conn.Open();

                    NpgsqlCommand command = null;
                    if (_configuration.id != 0)
                    {
                        command = GetCommand(SQL_Search, conn);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _configuration.id;
                    }
                    else
                    {
                        QueryBuilder qb = new QueryBuilder();
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                        if (_configuration.history)
                            qb.AddParameter("configuration.isactive", false, "=");
                        else
                            qb.AddParameter("configuration.isactive", true, "=");

                        if (!string.IsNullOrEmpty(_configuration.name))
                            qb.AddParameter("configuration.name", _configuration.name, "=");

                        if (_configuration.keys != null && _configuration.keys.Count > 0)
                        {
                            qb.AddInParamer("configuration.key", string.Join(",", _configuration.keys.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_configuration.key))
                                qb.AddParameter("configuration.key", _configuration.key, "=");
                            else
                                qb.AddParameter("configuration.key", "APPLICATIONSETTINGS", "!=");
                        }

                        if (_configuration.excludekeys != null && _configuration.excludekeys.Count > 0)
                        {
                            qb.AddNotInParamer("configuration.key", string.Join(",", _configuration.excludekeys.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                        }

                        if (_configuration.createdby > 0)
                            qb.AddParameter("configuration.createdby", _configuration.createdby, "=");


                        if (_configuration.organizationid > 0)
                            qb.AddParameter("configuration.organizationid", _configuration.organizationid, "=");

                        // || _configuration.key == "APPLICATIONSETTINGS"
                        if (_configuration.locationid > 0)
                            qb.AddParameter("configuration.locationid", _configuration.locationid, "=");
                        else if (_configuration.key == "APPLICATIONSETTINGS" && _configuration.locationid == -9)
                            qb.AddParameter("configuration.locationid", 0, "=");

                        if (_configuration.supportinstance)
                        {
                            qb.Add2Query(" (COALESCE(configuration.instancename,'" +
                                WebAPI.Data.Utility.SafeSQL(Services.ConfigManager.Instance(Context.OrganizationID).Config.instancename) + "') = '" +
                                WebAPI.Data.Utility.SafeSQL(Services.ConfigManager.Instance(Context.OrganizationID).Config.instancename) + "' or configuration.instancename = '')");
                        }
                        qb.SortField = "referencelist.name";
                        command = qb.GetCommand(conn);
                    }

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Configuration oconfiguration = new Configuration();
                            oconfiguration.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oconfiguration.userid = reader["userid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["userid"]);
                            oconfiguration.key = reader["key"] == DBNull.Value ? "" : reader["key"].ToString();
                            oconfiguration.reportformat_json = reader["reportformat"] == DBNull.Value ? null : reader["reportformat"].ToString();
                            oconfiguration.instancename = reader["instancename"] == DBNull.Value ? null : reader["instancename"].ToString();
                            oconfiguration.organizationid = reader["organizationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["organizationid"]);
                            oconfiguration.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                            oconfiguration.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                            oconfiguration.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                            oconfiguration.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                            oconfiguration.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                            oconfiguration.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                            oconfiguration.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            oconfiguration.nooflines = reader["nooflines"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nooflines"]);
                            oconfiguration.isdefault = reader["isdefault"] == DBNull.Value ? false : Convert.ToBoolean(reader["isdefault"]);
                            oconfiguration.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                            oconfiguration.noofcopy = reader["noofcopy"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofcopy"]);

                            if (oconfiguration.key == "APPLICATIONSETTINGS")
                            {
                                oconfiguration.appsettings_json = oconfiguration.reportformat.format;
                                oconfiguration.nextdaycount = reader["nextdaycount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nextdaycount"]);
                                oconfiguration.currentday = reader["currentday"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["currentday"]);
                                oconfiguration.freezedate = reader["freezedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["freezedate"]);
                            }
                            else if (oconfiguration.key == "FBCONFIG")
                            {
                                oconfiguration.fbconfig_json = oconfiguration.reportformat.format;
                            }
                            else if (oconfiguration.key == "CRMCONFIG")
                            {
                                oconfiguration.crmconfig_json = oconfiguration.reportformat.format;
                            }
                            else if (oconfiguration.key == "WGCONFIG")
                            {
                                oconfiguration.wgconfig_json = oconfiguration.reportformat.format;
                            }
                            else
                            {
                                oconfiguration.ispdf = oconfiguration.reportformat != null && oconfiguration.reportformat.mode == "pdf";
                            }
                            if (oconfiguration.appsettings != null)
                            {
                                oconfiguration.appsettings.CommandLibClass = System.Configuration.ConfigurationManager.AppSettings["CommandLibClass"];
                                oconfiguration.appsettings.CommandLibPath = System.Configuration.ConfigurationManager.AppSettings["CommandLibPath"];
                            }
                            lst.Add(oconfiguration);
                        }
                    }
                    conn.Close();
                }
                catch (Exception exp)
                {
                    if (conn != null) conn.Close();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _configuration);
                }
                if (lst.Count > 1)
                {
                    if (_configuration.key == "WGCONFIG")
                    {
                        lst = lst.FindAll(x => x.userid == 0 || x.createdby == Context.UserID ||
                            (x.wgconfig != null &&
                            x.wgconfig.userids != null &&
                            x.wgconfig.userids.Contains(Context.UserID)));

                        lst.ForEach(x => x.canalter = x.createdby == Context.UserID);
                    }
                    else
                    {
                        var existing = lst.FindAll(e => e.locationid <= 0);
                        lst.RemoveAll(e => e.locationid <= 0);
                        lst.InsertRange(0, existing);
                    }
                }
            }
            return lst;
        }

        internal Configuration SetValue(Configuration item)
        {
            if (item.id <= 0) return item;
            var config = Search(new ConfigurationCreteria()
            {
                id = item.id
            });
            if (item.key == "payroll_locakdate")
            {
                config[0].appsettings.payroll_locakdate = item.appsettings.payroll_locakdate;
                Update(config[0]);
                return config[0];
            }

            return item;
        }

        public Configuration Insert(Configuration oConfiguration)
        {
            if (oConfiguration.key == "APPLICATIONSETTINGS")
            {
                oConfiguration.reportformat = new Configuration.ReportFormat()
                {
                    format = oConfiguration.appsettings_json
                };
            }
            else if (oConfiguration.key == "FBCONFIG")
            {
                oConfiguration.reportformat = new Configuration.ReportFormat()
                {
                    format = oConfiguration.fbconfig_json
                };
            }
            else if (oConfiguration.key == "CRMCONFIG")
            {
                oConfiguration.reportformat = new Configuration.ReportFormat()
                {
                    format = oConfiguration.crmconfig_json
                };
            }
            else if (!IsvalidFormat(oConfiguration.reportformat.format))
            {
                throw new Exception("Invalid XML format");
            }

            if (oConfiguration.key == "WGCONFIG")
            {
                oConfiguration.createdby = Context.UserID;
                oConfiguration.createdon = DateTimeNow;
            }
            NpgsqlTransaction transaction = null;
            try
            {

                if (oConfiguration.key == "APPLICATIONSETTINGS")
                {
                    if (oConfiguration.locationid == 0)
                    {
                        var global = Search(new ConfigurationCreteria()
                        {
                            locationid = -9,
                            key = oConfiguration.key
                        });
                        if (global.Count > 0)
                            return global[0];
                    }
                    else
                    {
                        var global = Search(new ConfigurationCreteria()
                        {
                            locationid = oConfiguration.locationid,
                            key = oConfiguration.key
                        });
                        if (global.Count > 0)
                            return global[0];
                    }
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (oConfiguration.locationid == -9)
                        oConfiguration.locationid = 0;
                    Insert(transaction, conn, oConfiguration);
                    transaction.Commit();

                    if (oConfiguration.key == "APPLICATIONSETTINGS")
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oConfiguration.id, type = "CONFIG" }, conn);
                    }

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oConfiguration);


            }

            return oConfiguration;
        }
        private bool IsvalidFormat(string str)
        {
            try
            {
                //System.Xml.XmlDocument document = new System.Xml.XmlDocument();
                //document.LoadXml(str);
            }
            catch (Exception exp)
            {
                return false;
            }
            return true;
        }
        public Configuration Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Configuration oConfiguration)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.userid;
            command.Parameters.Add("key", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oConfiguration.key == null ? "" : oConfiguration.key;
            command.Parameters.Add("instancename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oConfiguration.instancename == null ? "" : oConfiguration.instancename;

            command.Parameters.Add("reportformat", NpgsqlTypes.NpgsqlDbType.Json).Value = oConfiguration.reportformat_json == null ? "null" : oConfiguration.reportformat_json;

            command.Parameters.Add("organizationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.organizationid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oConfiguration.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oConfiguration.name == null ? "" : oConfiguration.name;
            command.Parameters.Add("nooflines", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.nooflines;
            command.Parameters.Add("isdefault", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oConfiguration.isdefault;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.locationid;
            command.Parameters.Add("noofcopy", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.noofcopy;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oConfiguration.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oConfiguration;
        }

        public int UnsettledBillCount(long locationid)
        {
            var param = new SortedList<string, object>();
            param.Add("fromdate", DateTimeNow.Date);
            param.Add("todate", DateTimeNow.Date.AddDays(1));
            param.Add("locationid", locationid);
            int count = 0;
            using (var connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    using (var reader = ExecuteReader("Select count(*) from bill where bill.isactive = true " +
                        "and bill.settled = false and isb2b=false and billtype!=9 and billdate>=@fromdate and billdate<@todate and locationid=@locationid", param, connection))
                    {
                        if (reader.Read())
                        {
                            count = Convert.ToInt32(reader[0]);
                        }
                    }
                    connection.Close();
                }
                catch (Exception exp)
                {
                    if (connection.State != System.Data.ConnectionState.Closed)
                        connection.Close();
                }
            }
            return count;
        }

        DateTime? LastBillDate(DateConfiguration oConfiguration)
        {
            var param = new SortedList<string, object>();
            param.Add("fromdate", DateTimeNow.Date);
            param.Add("todate", DateTimeNow.Date.AddDays(1));
            param.Add("locationid", oConfiguration.locationid);
            using (var reader = ExecuteReader("Select createdon from bill where bill.isactive = true " +
                (oConfiguration.locationid > 0 ? " and locationid=@locationid " : "") +
                "and billdate>=@fromdate and billdate<@todate order " +
                "by createdon desc " +
                "limit 1", param))
            {
                if (reader.Read())
                {
                    return Convert.ToDateTime(reader[0]);
                }
            }
            return null;
        }

        public DateConfiguration FreezeDate(DateConfiguration oConfiguration)
        {
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_saledatefreeze) return oConfiguration;
            string SQL_UpdateStatus = @"UPDATE public.configuration SET freezedate=@freezedate where Configuration.id=@id RETURNING version";

            var items = Search(new ConfigurationCreteria()
            {
                key = "APPLICATIONSETTINGS",
                locationid = oConfiguration.locationid
            });

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in items)
                    {
                        // Deactivate the existing record and create new 
                        NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("freezedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oConfiguration.removeconfig ? DateTime.MinValue : DateTimeNow.Date;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                item.version = Convert.ToInt32(reader[0].ToString());
                                oConfiguration.currentday = oConfiguration.removeconfig ? DateTime.MinValue : DateTimeNow.Date;
                            }
                        }
                    }
                    transaction.Commit();
                    foreach (var item in items)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = item.id, type = "CONFIG" }, conn);
                    }
                    conn.Close();
                    oConfiguration.Success = true;
                }
            }
            catch (Exception exp)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oConfiguration);
            }

            return oConfiguration;
        }

        public DateConfiguration ClearChangeDate(DateConfiguration oConfiguration)
        {
            if (oConfiguration.locationid == 0)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = "Please select location";
                return oConfiguration;
            }
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_datechange)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = "System not configured for Data Change.";
                return oConfiguration;
            }



            string SQL_UpdateStatus = @"UPDATE public.configuration SET nextdaycount=@nextdaycount,currentday=@currentday where Configuration.id=@id RETURNING version";

            var items = Search(new ConfigurationCreteria()
            {
                key = "APPLICATIONSETTINGS",
                locationid = oConfiguration.locationid
            });

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in items)
                    {
                        // Deactivate the existing record and create new 
                        NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("nextdaycount", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                        command.Parameters.Add("currentday", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DBNull.Value;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                item.version = Convert.ToInt32(reader[0].ToString());
                                oConfiguration.currentday = DateTimeNow.Date;
                                oConfiguration.nextday = DateTimeNow.Date;
                                oConfiguration.nextdaycount = 0;
                            }
                        }
                    }

                    transaction.Commit();
                    foreach (var item in items)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = item.id, type = "CONFIG" }, conn);
                    }
                    conn.Close();
                    oConfiguration.Success = true;
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oConfiguration, Processors.Schedule_Actions.Actions.DayClose);
                }
            }
            catch (Exception exp)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oConfiguration);
            }

            return oConfiguration;
        }

        public DateConfiguration ChangeDate(DateConfiguration oConfiguration)
        {
            if (oConfiguration.locationid == 0)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = "Please select location";
                return oConfiguration;
            }
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_datechange)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = "System not configured for Data Change.";
                return oConfiguration;
            }

            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_datechangesilent)
            {
                var bc = (UnsettledBillCount(oConfiguration.locationid));
                if (bc > 0)
                {
                    oConfiguration.Success = false;
                    oConfiguration.ErrorMessage = "Unsettled bill available - " + bc.ToString();
                    return oConfiguration;
                }

                var lasttime = LastBillDate(oConfiguration);
                if (lasttime.HasValue && DateTimeNow.Subtract(lasttime.Value).TotalMinutes < 1)
                {
                    oConfiguration.Success = false;
                    oConfiguration.ErrorMessage = "Wait 1 minutes, then try again";
                    return oConfiguration;
                }
            }
            //if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_validatesettlement)
            //{

            //}

            string SQL_UpdateStatus = @"UPDATE public.configuration SET nextdaycount=@nextdaycount,currentday=@currentday where Configuration.id=@id RETURNING version";

            var items = Search(new ConfigurationCreteria()
            {
                key = "APPLICATIONSETTINGS",
                locationid = oConfiguration.locationid
            });

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in items)
                    {
                        // Deactivate the existing record and create new 
                        NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("nextdaycount", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.nextdaycount;
                        command.Parameters.Add("currentday", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.Date;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                item.version = Convert.ToInt32(reader[0].ToString());
                                oConfiguration.currentday = DateTimeNow.Date;
                                oConfiguration.nextday = oConfiguration.currentday.AddDays(oConfiguration.nextdaycount).Date;
                            }
                        }
                    }

                    transaction.Commit();
                    foreach (var item in items)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = item.id, type = "CONFIG" }, conn);
                    }
                    conn.Close();
                    oConfiguration.Success = true;
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oConfiguration, Processors.Schedule_Actions.Actions.DayClose);
                }
            }
            catch (Exception exp)
            {
                oConfiguration.Success = false;
                oConfiguration.ErrorMessage = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oConfiguration);
            }

            return oConfiguration;
        }

        public Configuration Update(Configuration oConfiguration)
        {
            if (oConfiguration.key == "APPLICATIONSETTINGS")
            {
                oConfiguration.reportformat = new Configuration.ReportFormat()
                {
                    format = oConfiguration.appsettings_json
                };

                var existing = Search(new ConfigurationCreteria()
                {
                    id = oConfiguration.id
                });

                if (existing.First().locationid != oConfiguration.locationid)
                {
                    existing = Search(new ConfigurationCreteria()
                    {
                        key = "APPLICATIONSETTINGS",
                        locationid = oConfiguration.locationid
                    });
                    if (existing.Count > 0)
                    {
                        oConfiguration.ErrorMessage = "Please check the location, already exist";
                        return oConfiguration;
                    }
                }
            }
            else if (oConfiguration.key == "FBCONFIG")
            {
                oConfiguration.reportformat = new Configuration.ReportFormat()
                {
                    format = oConfiguration.fbconfig_json
                };
            }
            else if (oConfiguration.key == "CRMCONFIG")
            {
                oConfiguration.reportformat = new Configuration.ReportFormat()
                {
                    format = oConfiguration.crmconfig_json
                };
            }
            else if (!IsvalidFormat(oConfiguration.reportformat.format))
            {
                throw new Exception("Invalid XML format");
            }

            string SQL_UpdateStatus = @"UPDATE public.configuration SET version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby, isactive=@isactive where id=@id  RETURNING version";
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oConfiguration.key != "APPLICATIONSETTINGS")
                    {
                        // Deactivate the existing record and create new 
                        NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.id;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.version;
                        command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                oConfiguration.version = Convert.ToInt32(reader[0].ToString());
                            }
                        }
                        oConfiguration.id = 0;
                        Insert(transaction, conn, oConfiguration);
                    }
                    else
                    {
                        Update(transaction, conn, oConfiguration);
                    }
                    transaction.Commit();
                    if (oConfiguration.key == "APPLICATIONSETTINGS")
                    {
                        LicenseManager.Instance(Context.OrganizationID).BuildPermissionsets(Context.OrganizationID);
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oConfiguration.id, type = "CONFIG" }, conn);
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oConfiguration);


            }

            return oConfiguration;
        }

        public Configuration Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Configuration oConfiguration)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.id;
            command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.userid;
            command.Parameters.Add("instancename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oConfiguration.instancename == null ? "" : oConfiguration.instancename;
            command.Parameters.Add("key", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oConfiguration.key == null ? "" : oConfiguration.key;
            command.Parameters.Add("reportformat", NpgsqlTypes.NpgsqlDbType.Json).Value = oConfiguration.reportformat_json == null ? "null" : oConfiguration.reportformat_json;
            command.Parameters.Add("organizationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.organizationid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oConfiguration.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oConfiguration.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oConfiguration.name == null ? "" : oConfiguration.name;
            command.Parameters.Add("nooflines", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.nooflines;
            command.Parameters.Add("isdefault", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oConfiguration.isdefault;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.locationid;
            command.Parameters.Add("noofcopy", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.noofcopy;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oConfiguration.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oConfiguration;
        }


        public bool Delete(Configuration oConfiguration)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oConfiguration);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oConfiguration);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Configuration oConfiguration)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oConfiguration.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oConfiguration.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oConfiguration.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal CheckSumResponse ComputeCheckSum(CheckSumRequest item)
        {
            if (item.type == "paytmqr")
            {
                return new CheckSumResponse()
                {
                    type = item.type,
                    data = item.data,
                    key = item.key,
                    checksum = PayTMChecksum.generateSignature(item.data, item.key)
                };
            }
            if (item.type == "paytm")
            {
                return new CheckSumResponse()
                {
                    type = item.type,
                    data = item.data,
                    key = item.key,
                    checksum = PayTMChecksum.generateJSONSignature(item.data,true, item.key)
                };
            }
            return null;
        }

        internal bool SaveExcludeReward(ConfigurationCreteria item)
        {
            var settings = Search(new ConfigurationCreteria()
            {
                key = "APPLICATIONSETTINGS",
                locationid = -9
            });
            foreach (var setting in settings)
            {
                setting.appsettings.gft_excludepromotionlist = item.promotionids;
                Update(setting);
            }
            ConfigManager.Instance(Context.OrganizationID).Initialize();
            return true;
        }

        internal ConfigurationByString GetConfigByKey(string key)
        {
            ConfigurationByString response = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var param = new SortedList<string, object>();
                    param.Add("key", key);
                    using (var reader = ExecuteReader("Select reportformat from configuration where key=@key and isactive=true",param))
                    {
                        if(reader.Read())
                        {
                            var reportformat = Newtonsoft.Json.JsonConvert.DeserializeObject<Configuration.ReportFormat>(reader.GetString(0));
                            response = new ConfigurationByString()
                            {
                                key = key,
                                value = reportformat.format
                            };
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "GetConfigByKey:"+key);
            }
            return response;
        }
        internal bool SaveConfigByKey(ConfigurationByString item)
        {
            if (string.IsNullOrEmpty(item.key))
                throw new Exception("Key can't be empty");

            if (string.IsNullOrEmpty(item.value))
                throw new Exception("Configuration can't be empty");

            int successCount = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                long id = 0;
                var param = new SortedList<string, object>();
                param.Add("key", item.key);
                using (var reader = ExecuteReader("Select id from configuration where key=@key and isactive=true", param))
                {
                    if (reader.Read())
                    {
                        id = reader.GetInt64(0);
                    }
                }

                if (id == 0)
                {
                    successCount = Insert(new Configuration()
                    {
                        key = item.key,
                        reportformat = new Configuration.ReportFormat()
                        {
                            format = item.value
                        },
                        isactive = true,
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        version = 1
                    }).version;
                }
                else
                {
                    var reportformat = new Configuration.ReportFormat()
                    {
                        format = item.value
                    };

                    NpgsqlCommand command = GetCommand("Update configuration set reportformat=@reportformat,modifiedby=@modifiedby,modifiedon=@modifiedon where key=@key and isactive=true", conn);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("key", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.key == null ? "" : item.key;
                    command.Parameters.Add("reportformat", NpgsqlTypes.NpgsqlDbType.Json).Value = Newtonsoft.Json.JsonConvert.SerializeObject(reportformat);
                    successCount = command.ExecuteNonQuery();
                }
                conn.Close();
            }
            return successCount > 0;
        }
    }

}
