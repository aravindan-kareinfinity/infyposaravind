using InfyPOS.Common;
using InfyPOS.Controllers;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static InfyPOS.Services.SyncManager;

namespace InfyPOS.Services
{
    public class Sync_Server : SAASData
    {
        public long clientid { get; set; }
        public long locationid { get; set; }
        DateTime fromdate { get; set; }
        public bool hasofflinelicense { get; set; }
        public long organizationid { get; set; }
        public Sync_Server(long organizationid, long clientid, long locationid) : base()
        {
            this.organizationid = organizationid;
            this.clientid = clientid;
            this.locationid = locationid;
            hasofflinelicense = LicenseManager.Instance(0).GetLicense().supportoffline;
        }
        public bool IsValid { get; set; }
        public string Create(string clientid, string locationid, string privatekey, DateTime fromdate)
        {
            if (!hasofflinelicense) return ("Offline license not available.");

            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var locations = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                name = locationid,
                type = "LOCATION"
            });

            if (locationid == "ALL LOCATION")
            {
                locations = new List<Models.Referencelist>();
                locations.Add(new Models.Referencelist()
                {
                    type = "LOCATION",
                    id = -1,
                    name = locationid
                });
            }
            if (locations.Count == 1)
            {
                var cliientlist = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "OFFLINECLIENT",
                    name = clientid,
                    locationid = locations[0].id
                });
                if (cliientlist.Count == 0)
                {
                    Models.Referencelist referencelist = new Models.Referencelist();
                    referencelist.type = "OFFLINECLIENT";
                    referencelist.locationid = locations[0].id;
                    referencelist.code = clientid;
                    referencelist.name = clientid;
                    referencelist.additionalinfo = new Models.Referencelist.Attribute()
                    {
                        fromdate = fromdate,
                        licensekey = LicenseManager.License.CreateKey(privatekey)
                    };
                    referencelistDataService.Insert(referencelist);
                    return "New Offline client added to the master server.";
                }
                else
                {
                    cliientlist[0].additionalinfo = new Models.Referencelist.Attribute()
                    {
                        fromdate = fromdate,
                        licensekey = LicenseManager.License.CreateKey(privatekey)
                    };
                    return "Existing Offline client updated in the master server.";
                }
            }
            else
            {
                return ("Given location " + locationid + " not available");
            }

            //cliientlist[0].additionalinfo.fromdate
        }

        internal string ClearOfflineClient(long organizationid)
        {
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var masterrows = baseData.ExecuteQuery("Delete from sync_master where locationid=" + this.locationid + " and clientid=" + this.clientid, null);
            var transactionrows = baseData.ExecuteQuery("Delete from sync_transaction where locationid=" + this.locationid + " and clientid=" + this.clientid, null);
            return string.Format("Deleted {0} rows masters, Deleted {1} rows transactions.", masterrows, transactionrows);
        }

        public Sync_Server(long organizationid, string clientid, string locationid, string privatekey) : base()
        {
            hasofflinelicense = LicenseManager.Instance(0).GetLicense().supportoffline;
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            if (locationid == "ALL LOCATION")
            {
                var cliientlist = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "OFFLINECLIENT",
                    name = clientid
                });
                if (cliientlist.Count == 1)
                    //&& cliientlist[0].additionalinfo.licensekey == LicenseManager.License.CreateKey(privatekey))
                {
                    this.fromdate = cliientlist[0].additionalinfo.fromdate;
                    this.clientid = cliientlist[0].id;
                    this.locationid = -1;
                    this.IsValid = true;
                }
            }
            else
            {
                var locations = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    name = locationid,
                    type = "LOCATION"
                });

                if (locations.Count == 1)
                {
                    var cliientlist = referencelistDataService.Search(new Models.ReferencelistCreteria()
                    {
                        type = "OFFLINECLIENT",
                        name = clientid,
                        locationid = locations[0].id
                    });

                    if (cliientlist.Count == 1)
                        //&& cliientlist[0].additionalinfo.licensekey == LicenseManager.License.CreateKey(privatekey))
                    {
                        this.fromdate = cliientlist[0].additionalinfo.fromdate;
                        this.clientid = cliientlist[0].id;
                        this.locationid = locations[0].id;
                        this.IsValid = true;
                    }
                }
            }
        }

        internal DBDataAvailableSummary CreateSyncPoint(long organizationid,SyncController.SyncRequest request)
        {
            DBDataAvailableSummary dBDataAvailableSummary = new DBDataAvailableSummary();
            List<DBDataAvailable> dBDataAvailables = new List<DBDataAvailable>();
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            NpgsqlConnection conn = baseData.CreateConnection();
            try
            {

                WebAPI.Common.Logger.Instance(0).Log("DownloadDataStatus from " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, "");

                Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var locations = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "LOCATION",
                    name = request.location == "ALL LOCATION" ? null : request.location
                });

                if (request.location != "ALL LOCATION" && locations.Count == 0) return new DBDataAvailableSummary();

                var client = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "OFFLINECLIENT",
                    name = request.clientid
                });

                if (client.Count == 0) return new DBDataAvailableSummary();
                this.fromdate = client[0].additionalinfo.fromdate;
                this.clientid = client[0].id;

                conn.Open();

                this.locationid = request.location == "ALL LOCATION" ? -1 : locations.OrderBy(e => e.id).First().id;

                string insertQuery = @"delete from sync_master where tabletype=@tabletype and locationid=@locationid and clientid=@clientid;
                                       insert into sync_master(tabletype,sourceid,locationid,clientid,version,createdon,modifiedon)
                                       select @tabletype,source.id,@locationid,@clientid,source.version,now()::timestamp,now()::timestamp from {0} source";

                foreach (var item in Enum.GetValues(typeof(InfyPOS.Services.SyncManager.ServerMasters)))
                {
                    string table = Enum.GetName(typeof(InfyPOS.Services.SyncManager.ServerMasters), item);
                    string query = string.Format(insertQuery, item.ToString());

                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    var available = command.ExecuteNonQuery();

                    dBDataAvailables.Add(new DBDataAvailable()
                    {
                        tablename = table,
                        type = (int)item,
                        available = available
                    });
                    dBDataAvailableSummary.masters += available;
                }

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Create Sync Point " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, dBDataAvailableSummary);

                insertQuery = @"delete from sync_transaction where tabletype=@tabletype and locationid=@locationid and clientid=@clientid;
                                insert into sync_transaction(tabletype,sourceid,code,locationid,clientid,version,createdon,modifiedon)
                                select @tabletype,source.id,{1},@locationid,@clientid,source.version,now()::timestamp,now()::timestamp from {0} source";

                foreach (var item in Enum.GetValues(typeof(InfyPOS.Services.SyncManager.AllTransaction)))
                {
                    string code = "''";
                    string table = Enum.GetName(typeof(InfyPOS.Services.SyncManager.AllTransaction), item);
                    switch ((InfyPOS.Services.SyncManager.AllTransaction)item)
                    {
                        case AllTransaction.bill:
                            code = "source.billno";
                            break;
                        case AllTransaction.billsettlementmaster:
                            code = "source.billmastercode";
                            break;
                        case AllTransaction.salablegoods:
                            code = "source.barcode";
                            break;
                        case AllTransaction.stock:
                            code = "source.barcode";
                            break;
                        case AllTransaction.billitems:
                            continue;
                        case AllTransaction.billsettlement:
                            continue;
                    }

                    string query = string.Format(insertQuery, item.ToString(), code);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    var available = command.ExecuteNonQuery();

                    dBDataAvailables.Add(new DBDataAvailable()
                    {
                        tablename = table,
                        type = (int)item,
                        available = available
                    });
                    dBDataAvailableSummary.stocks += available;

                    dBDataAvailables.Add(new DBDataAvailable()
                    {
                        tablename = table,
                        type = (int)item,
                        available = available
                    });
                }

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Create Sync Point " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, dBDataAvailableSummary);

                ExecuteQuery(string.Format("delete from sync_master where tabletype = 2 and sourceid = {0}", client[0].id), null);

                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, dBDataAvailableSummary);
                if (conn != null)
                    conn.Close();
            }
            finally
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Create Sync Completed from " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, dBDataAvailableSummary);
            }
            return dBDataAvailableSummary;
        }

        public bool Initialize()
        {
            const string baseQuery = @"insert into sync_master(tabletype,sourceid,locationid,clientid,version,createdon,modifiedon)
                                          select @tabletype,id,@locationid,@clientid,source.version,now()::timestamp,now()::timestamp from {0} as source;";

            const string transactionQuery = @"insert into sync_transaction(tabletype,sourceid,code,locationid,clientid,version,createdon,modifiedon)
                                          select @tabletype,id,{1},@locationid,@clientid,source.version,now()::timestamp,now()::timestamp from {0} as source;";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                foreach (var item in Enum.GetValues(typeof(ServerMasters)))
                {
                    string table = Enum.GetName(typeof(ServerMasters), item);
                    string query = string.Format(baseQuery, item.ToString());
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.ExecuteNonQuery();
                }


                foreach (var item in Enum.GetValues(typeof(BillTransaction)))
                {
                    string table = Enum.GetName(typeof(BillTransaction), item);

                    string codename = "";
                    if ((int)item == (int)BillTransaction.bill
                        || (int)item == (int)AllTransaction.customerinvoice)
                    {
                        codename = "billno";
                    }
                    else if ((int)item == (int)AllTransaction.billsettlementmaster)
                    {
                        codename = "code";
                    }
                    string query = string.Format(transactionQuery, item.ToString(), codename);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.ExecuteNonQuery();
                }

                foreach (var item in Enum.GetValues(typeof(ServerTransaction)))
                {
                    string table = Enum.GetName(typeof(ServerTransaction), item);
                    string query = string.Format(transactionQuery, item.ToString(), "barcode");
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
            }
            return true;
        }


        public List<DBData.RowStatus> Sync_Masters(DataTable table, string tablename)
        {
            if (table == null || table.Rows.Count == 0) return new List<DBData.RowStatus>();
            string columnlistquery = "";
            string parameterlistquery = "";
            string updatequery = "";
            bool issalablegoods = false;
            List<DBData.RowStatus> returnList = new List<DBData.RowStatus>();
            foreach (DataColumn column in table.Columns)
            {
                if (tablename == "salablegoods" && column.ColumnName.ToLower() == "oqty")
                {
                    issalablegoods = true;
                    continue;
                }

                if (column.ColumnName.ToLower() != "id")
                    updatequery += (updatequery.Length > 0 ? "," : "update public." + tablename + " SET ") + column.ColumnName + "=@" + column.ColumnName;
                columnlistquery += (columnlistquery.Length > 0 ? "," : "") + column.ColumnName;
                parameterlistquery += (parameterlistquery.Length > 0 ? ",@" : "@") + column.ColumnName;
            }
            string insertQuery = string.Format("INSERT INTO public.{0}({1}) VALUES({2}) RETURNING id", tablename, columnlistquery, parameterlistquery);
            updatequery += " where id=@id";
            List<long> ids = new List<long>();
            List<long> duplicateids = new List<long>();
            foreach (DataRow row in table.Rows)
            {
                ids.Add((long)row["id"]);
            }

            SortedDictionary<string, string> columnTypes = new SortedDictionary<string, string>();
            NpgsqlConnection conn = CreateConnection();
            NpgsqlTransaction transaction = null;
            try
            {
                conn.Open();

                string searchQuery = "Select id from " + tablename + " where id in (" + string.Join<long>(",", ids) + ");" +
                                     "Select data_type,column_name from INFORMATION_SCHEMA.COLUMNS where table_name = '" + tablename + "';";
                NpgsqlCommand command = new NpgsqlCommand(searchQuery, conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    ids.Clear();
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt64(0));
                    }
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            columnTypes.Add(reader.GetString(1), reader.GetString(0));
                        }
                    }
                }

                transaction = conn.BeginTransaction();
                foreach (DataRow row in table.Rows)
                {
                    long id = (long)row["id"];
                    if (duplicateids.Contains(id))
                        continue;

                    int version = row.IsNull("version") ? 0 : Convert.ToInt32(row["version"]);
                    returnList.Add(new DBData.RowStatus()
                    {
                        id = id,
                        version = version
                    });
                    command = new NpgsqlCommand(ids.Contains(id) ? updatequery : insertQuery, conn, transaction);
                    foreach (DataColumn column in table.Columns)
                    {
                        NpgsqlTypes.NpgsqlDbType type = NpgsqlTypes.NpgsqlDbType.Unknown;

                        switch (columnTypes[column.ColumnName])
                        {
                            case "tsvector":
                                type = NpgsqlTypes.NpgsqlDbType.TsVector;
                                command.CommandText = command.CommandText.Replace("@" + column.ColumnName, "to_tsvector(@" + column.ColumnName + ")");
                                break;
                            case "jsonb":
                                type = NpgsqlTypes.NpgsqlDbType.Jsonb;
                                break;
                            case "smallint":
                                type = NpgsqlTypes.NpgsqlDbType.Smallint;
                                break;
                            case "timestamp without time zone":
                                type = NpgsqlTypes.NpgsqlDbType.Timestamp;
                                break;
                            case "json":
                                type = NpgsqlTypes.NpgsqlDbType.Json;
                                break;
                            case "boolean":
                                type = NpgsqlTypes.NpgsqlDbType.Boolean;
                                break;
                            case "money":
                                type = NpgsqlTypes.NpgsqlDbType.Numeric;
                                break;
                            case "character varying":
                                type = NpgsqlTypes.NpgsqlDbType.Varchar;
                                break;
                            case "bytea":
                                type = NpgsqlTypes.NpgsqlDbType.Bytea;
                                break;
                            case "integer":
                                type = NpgsqlTypes.NpgsqlDbType.Integer;
                                break;
                            case "bit":
                                type = NpgsqlTypes.NpgsqlDbType.Bit;
                                break;
                            case "bigint":
                                type = NpgsqlTypes.NpgsqlDbType.Bigint;
                                break;
                        }
                        if (type == NpgsqlTypes.NpgsqlDbType.Bytea)
                            command.Parameters.Add(column.ColumnName, type).Value = DBNull.Value;
                        else if (type == NpgsqlTypes.NpgsqlDbType.TsVector)
                            command.Parameters.Add(column.ColumnName, NpgsqlTypes.NpgsqlDbType.Varchar).Value = CreatTSVector(row[column.ColumnName] as DataTable);
                        else
                            command.Parameters.Add(column.ColumnName, type).Value = row[column.ColumnName];
                    }
                    if (ids.Contains(id))
                        command.ExecuteNonQuery();
                    else
                        duplicateids.Add(id);
                }
                transaction.Commit();
                if (issalablegoods)
                {
                    ExecuteQuery("update salablegoods set oqty=0 where oqty is null", new SortedList<string, object>());
                }
                ResetSequence(tablename);
                return returnList;
            }
            catch (Exception exp)
            {
                if (transaction != null)
                    transaction.Rollback();
                if (conn != null)
                    conn.Close();
                ResetSequence(tablename);
                return null;
            }
        }

        public string CreatTSVector(DataTable dt)
        {
            if (dt == null)
                return "";
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            foreach (DataRow dr in dt.Rows)
                stringBuilder.Append((stringBuilder.Length > 0 ? " " : "") + dr[0].ToString());

            return Regex.Replace(stringBuilder.ToString(), @"[^\w\s]", "", RegexOptions.Compiled);

        }
        internal List<DBData> UploadOfflineTransaction(List<DBData> transactions)
        {
            foreach (var table in transactions)
            {
                var datatable = table.GetTable();
                try
                {
                    Sync_Masters(datatable, table.tablename);
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                }
                table.status = new List<DBData.RowStatus>();
                foreach (System.Data.DataRow dr in datatable.Rows)
                {
                    table.status.Add(new DBData.RowStatus()
                    {
                        id = dr.IsNull("id") ? -1 : Convert.ToInt64(dr["id"]),
                        version = dr.IsNull("version") ? -1 : Convert.ToInt32(dr["version"])
                    });
                }
                table.data = "";
                table.datas = null;
                table.status.RemoveAll(e => e.id == -1 || e.version == -1);
            }
            return transactions;
        }

        public bool ChangeMasterStatus(List<DBData> data)
        {
            if (!IsValid) return false;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in data)
                {
                    string json = item.GetStatusJSON();
                    string insertQuery = @"insert into sync_master(tabletype,sourceid,locationid,clientid,version,createdon,modifiedon)
                                          select @tabletype,cast(statusList->>'id' as bigint),@locationid,@clientid,cast(statusList->>'version' as int),now()::timestamp,now()::timestamp from json_array_elements ('" + json + @"'::json) statusList 
                                          left outer join sync_master on sync_master.tabletype = @tabletype and sync_master.locationid = @locationid and clientid = @clientid and sync_master.sourceid = cast(statusList->>'id' as bigint)
                                          where sync_master.id is null";

                    string updateQuery = @"update sync_master set version =  cast(statusList->>'version' as integer),modifiedon=now()::timestamp from json_array_elements ('" + json + @"'::json) statusList
                                           where sync_master.tabletype = @tabletype and sync_master.locationid = @locationid and clientid = @clientid and cast(statusList->>'id' as bigint) = sync_master.sourceid";


                    NpgsqlCommand command = new NpgsqlCommand(insertQuery, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.type;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.ExecuteNonQuery();

                    command.CommandText = updateQuery;
                    command.ExecuteNonQuery();
                }
                conn.Close();
                return true;
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
                return false;
            }
        }

        public bool ChangeTransactionStatus(List<DBData> data)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in data)
                {
                    var list = item.GetPartionedStatus(250);
                    foreach (var litem in list)
                    {
                        var json = item.GetStatusJSON(litem);
                        string insertQuery = @"insert into sync_transaction(tabletype,sourceid,code,locationid,clientid,version,createdon,modifiedon)
                                          select @tabletype,cast(statusList->>'id' as bigint),cast(statusList->>'code' as varchar),@locationid,@clientid,cast(statusList->>'version' as int),now()::timestamp,now()::timestamp from json_array_elements ('" + json + @"'::json) statusList 
                                          left outer join sync_transaction on sync_transaction.tabletype = @tabletype and sync_transaction.locationid = @locationid and clientid = @clientid and sync_transaction.sourceid = cast(statusList->>'id' as bigint)
                                          where sync_transaction.id is null";

                        string updateQuery = @"update sync_transaction set version = cast(statusList->>'version' as int),modifiedon=now()::timestamp from json_array_elements ('" + json + @"'::json) statusList
                                           where sync_transaction.tabletype = @tabletype and sync_transaction.locationid = @locationid and clientid = @clientid and cast(statusList->>'id' as bigint) = sync_transaction.sourceid";


                        NpgsqlCommand command = new NpgsqlCommand(insertQuery, conn);
                        command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.type;
                        command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                        command.ExecuteNonQuery();

                        command.CommandText = updateQuery;
                        command.ExecuteNonQuery();
                    }
                }
                conn.Close();
                return true;
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
                return false;
            }
        }

        internal DBDataAvailableSummary DownloadDataMultipleStatus(
            long organizationid,
            Controllers.SyncController.SyncRequest request, bool onlybill)
        {
            DBDataAvailableSummary dBDataAvailableSummary = new DBDataAvailableSummary();
            var locations = request.location.Split(',');
            foreach (var location in locations)
            {
                request.location = location;
                var dbsummary = DownloadDataStatus(organizationid,request, onlybill);
                dBDataAvailableSummary.masters += dbsummary.masters;
                dBDataAvailableSummary.offlinebills += dbsummary.offlinebills;
                dBDataAvailableSummary.offlinetransactions += dbsummary.offlinetransactions;
                dBDataAvailableSummary.onlinebills += dbsummary.onlinebills;
                dBDataAvailableSummary.stocks += dbsummary.stocks;
            }
            return dBDataAvailableSummary;
        }
        internal DBDataAvailableSummary DownloadDataStatus(long organiationid,Controllers.SyncController.SyncRequest request, bool onlybill)
        {
            DBDataAvailableSummary dBDataAvailableSummary = new DBDataAvailableSummary();
            List<DBDataAvailable> dBDataAvailables = new List<DBDataAvailable>();
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            NpgsqlConnection conn = baseData.CreateConnection();
            try
            {

                WebAPI.Common.Logger.Instance(0).Log("DownloadDataStatus from " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, "");

                Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                var locations = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "LOCATION",
                    name = request.location == "ALL LOCATION" ? null : request.location
                });

                if (request.location != "ALL LOCATION" && locations.Count == 0) return new DBDataAvailableSummary();

                var client = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "OFFLINECLIENT",
                    name = request.clientid
                });

                if (client.Count == 0) return new DBDataAvailableSummary();
                this.fromdate = client[0].additionalinfo.fromdate;
                this.clientid = client[0].id;

                conn.Open();

                this.locationid = request.location == "ALL LOCATION" ? -1 : locations.OrderBy(e => e.id).First().id;
                string baseQuery = @"select count(*) from {0} source
                            left outer join sync_master on sync_master.tabletype = @tabletype
                            and sync_master.clientid = @clientid and sync_master.locationid=@locationid and sync_master.sourceid = source.id
                            where source.modifiedon>=@createdon and (sync_master.id is null or source.version != sync_master.version)";


                List<InfyPOS.Services.SyncManager.DBData> dBRows = new List<InfyPOS.Services.SyncManager.DBData>();
                if (!onlybill)
                {
                    foreach (var item in Enum.GetValues(typeof(InfyPOS.Services.SyncManager.ServerMasters)))
                    {
                        string table = Enum.GetName(typeof(InfyPOS.Services.SyncManager.ServerMasters), item);
                        string query = string.Format(baseQuery, item.ToString());
                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                        command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                        command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = this.fromdate;

                        var available = Convert.ToInt32(command.ExecuteScalar());
                        dBDataAvailables.Add(new DBDataAvailable()
                        {
                            tablename = table,
                            type = (int)item,
                            available = available
                        });
                        dBDataAvailableSummary.masters += available;
                    }
                }

                baseQuery = @"select count(*) from {0} source
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.modifiedon>=@createdon and source.locationid=@locationid and (sync_transaction.id is null or source.version != sync_transaction.version)";

                foreach (var item in Enum.GetValues(typeof(InfyPOS.Services.SyncManager.AllTransaction)))
                {
                    if (!onlybill || ((InfyPOS.Services.SyncManager.AllTransaction)item != AllTransaction.salablegoods && (InfyPOS.Services.SyncManager.AllTransaction)item != AllTransaction.stock))
                    {
                        string table = Enum.GetName(typeof(InfyPOS.Services.SyncManager.AllTransaction), item);
                        string query = string.Format(baseQuery, item.ToString());
                        switch ((InfyPOS.Services.SyncManager.AllTransaction)item)
                        {
                            case InfyPOS.Services.SyncManager.AllTransaction.billsettlementmaster:

                                query = @"with offlinebills as
                            (select distinct billsettlementmaster.id from billsettlement source
                            inner join billsettlementmaster on billsettlementmaster.id = source.billsettlementmasterid
                            inner join bill on bill.id = source.billid
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.modifiedon>=@createdon and bill.offline = @offline and billsettlementmaster.locationid=@locationid and (sync_transaction.id is null or source.version != sync_transaction.version))
                            select count(*) from offlinebills";

                                break;
                            case InfyPOS.Services.SyncManager.AllTransaction.bill:

                                // or source.version != sync_transaction.version
                                query = @"select count(*) from bill source
                            inner join billsettlement on billsettlement.billid = source.id
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.offline = @offline and source.billdate >=@createdon and source.locationid=@locationid and (sync_transaction.id is null)";

                                if (request.all)
                                    query = query.Replace("source.offline = @offline and", " ");
                                break;
                            case InfyPOS.Services.SyncManager.AllTransaction.stock:
                                query = @"select count(*) from stock source
                            inner join salablegoods on salablegoods.stockid = source.id
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.modifiedon>=@createdon and salablegoods.locationid=@locationid and (sync_transaction.id is null or source.version != sync_transaction.version)";
                                break;
                            case InfyPOS.Services.SyncManager.AllTransaction.billitems:
                                continue;
                                query = @"select count(*) from billitems source
                            inner join bill on bill.id = source.billid
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.modifiedon>=@createdon and bill.offline = @offline and bill.locationid=@locationid and (sync_transaction.id is null or source.version != sync_transaction.version)";

                                break;
                            case InfyPOS.Services.SyncManager.AllTransaction.billsettlement:
                                continue;
                                query = @"select count(*) from billsettlement source
                            inner join billsettlementmaster on billsettlementmaster.id = source.billsettlementmasterid
                            inner join bill on bill.id = source.billid
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.modifiedon>=@createdon and bill.offline = @offline and billsettlementmaster.locationid=@locationid and (sync_transaction.id is null or source.version != sync_transaction.version)";
                                break;
                            case InfyPOS.Services.SyncManager.AllTransaction.customerinvoice:
                            case InfyPOS.Services.SyncManager.AllTransaction.customerrewards:
                            case InfyPOS.Services.SyncManager.AllTransaction.vouchers:
                            case InfyPOS.Services.SyncManager.AllTransaction.customer:
                                query = query.Replace("source.locationid=@locationid and", "");
                                break;
                        }

                        if (locationid <= 0)
                        {
                            query = query.Replace("and source.locationid=@locationid", "");
                            query = query.Replace("and salablegoods.locationid=@locationid", "");
                        }

                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                        command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                        command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = this.fromdate;
                        command.Parameters.Add("offline", NpgsqlTypes.NpgsqlDbType.Boolean).Value = LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline;
                        var available = Convert.ToInt32(command.ExecuteScalar());
                        if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.bill)
                            dBDataAvailableSummary.onlinebills += available;
                        else if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.salablegoods)
                            dBDataAvailableSummary.stocks += available;
                        else if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.stock)
                            dBDataAvailableSummary.stocks += available;
                        else if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.customerinvoice)
                            dBDataAvailableSummary.offlinetransactions += available;
                        else if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.customerrewards)
                            dBDataAvailableSummary.offlinetransactions += available;
                        else if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.customer)
                            dBDataAvailableSummary.offlinetransactions += available;
                        else if ((InfyPOS.Services.SyncManager.AllTransaction)item == AllTransaction.vouchers)
                            dBDataAvailableSummary.offlinetransactions += available;
                        dBDataAvailables.Add(new DBDataAvailable()
                        {
                            tablename = table,
                            type = (int)item,
                            available = available
                        });
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
            }
            finally
            {
                WebAPI.Common.Logger.Instance(0).Log("DownloadDataStatus from " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, dBDataAvailableSummary);
            }
            return dBDataAvailableSummary;
        }

        internal byte[] DownloadData(long organizationid,long locationid)
        {
            Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            NpgsqlConnection conn = baseData.CreateConnection();
            try
            {
                const string baseQuery = @"select source.* from {0} source";
                conn.Open();
                List<InfyPOS.Services.SyncManager.DBData> dBRows = new List<InfyPOS.Services.SyncManager.DBData>();
                foreach (var item in Enum.GetValues(typeof(InfyPOS.Services.SyncManager.ServerMasters)))
                {
                    string table = Enum.GetName(typeof(InfyPOS.Services.SyncManager.ServerMasters), item);
                    string query = string.Format(baseQuery, item.ToString());
                    var bRows = Download((int)item, table, query, conn);
                    dBRows.Add(bRows);
                }

                foreach (var item in Enum.GetValues(typeof(InfyPOS.Services.SyncManager.AllTransaction)))
                {
                    string table = Enum.GetName(typeof(InfyPOS.Services.SyncManager.AllTransaction), item);
                    string query = string.Format(baseQuery, item.ToString());
                    switch ((InfyPOS.Services.SyncManager.AllTransaction)item)
                    {
                        case InfyPOS.Services.SyncManager.AllTransaction.salablegoods:
                            if (locationid > 0)
                                query += " where locationid=" + locationid;
                            break;
                        case InfyPOS.Services.SyncManager.AllTransaction.billsettlementmaster:
                            if (locationid > 0)
                                query += " where settlementon>='" + DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + "'";
                            else
                                query += " where locationid=" + locationid + " and settlementon>='" + DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + "'";
                            break;
                        case InfyPOS.Services.SyncManager.AllTransaction.bill:
                            if (locationid > 0)
                                query += " where locationid=" + locationid + " and billdate>='" + DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + "'";
                            else
                                query += " where billdate>='" + DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd") + "'";
                            break;
                        case InfyPOS.Services.SyncManager.AllTransaction.stock:
                            query += " inner join salablegoods on salablegoods.stockid = source.id";
                            if (locationid > 0)
                                query += " where salablegoods.locationid=" + locationid;
                            break;
                        case InfyPOS.Services.SyncManager.AllTransaction.billitems:
                            query += " inner join bill on bill.id = source.billid";
                            if (locationid > 0)
                                query += " where bill.locationid=" + locationid;
                            break;
                        case InfyPOS.Services.SyncManager.AllTransaction.billsettlement:
                            query += " inner join billsettlementmaster on billsettlementmaster.id = source.billsettlementmasterid";
                            if (locationid > 0)
                                query += " where billsettlementmaster.locationid=" + locationid;
                            break;
                    }
                    var bRows = Download((int)item, table, query, conn);
                    dBRows.Add(bRows);
                }

                conn.Close();

                var rawData = System.Text.ASCIIEncoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(dBRows, new Newtonsoft.Json.JsonSerializerSettings()
                {
                    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                }));

                using (var output = new System.IO.MemoryStream())
                {
                    using (var dstream = new DeflateStream(output, CompressionMode.Compress))
                    {
                        dstream.Write(rawData, 0, rawData.Length);
                    }
                    return output.ToArray();
                }

            }
            catch (Exception exp)
            {
                if (conn != null)
                    conn.Close();
            }
            return new byte[] { };
        }


        public InfyPOS.Services.SyncManager.DBData Download(int type, string table, string query, NpgsqlConnection conn)
        {
            InfyPOS.Services.SyncManager.DBData bRows = new InfyPOS.Services.SyncManager.DBData();
            bRows.type = type;
            bRows.tablename = table;
            NpgsqlCommand command = new NpgsqlCommand(query, conn);
            NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(command);
            DataSet dset = new DataSet();
            npgsqlDataAdapter.Fill(dset);
            dset.Tables[0].TableName = table;
            //bRows.data = Newtonsoft.Json.JsonConvert.SerializeObject(dset.Tables[0], new Newtonsoft.Json.JsonSerializerSettings()
            //{
            //    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
            //});

            bRows.datas = Processors.DataTableCustomFormatter.Serialize(dset.Tables[0], true);

            return bRows;
        }
        

        public List<DBData> DownloadMaster()
        {
            if (!IsValid) return new List<DBData>();
            string baseQuery = @"select source.* from {0} source
                            left outer join sync_master on sync_master.tabletype = @tabletype
                            and sync_master.clientid = @clientid and sync_master.locationid=@locationid and sync_master.sourceid = source.id
                            where (sync_master.id is null or source.version != sync_master.version)";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in Enum.GetValues(typeof(ServerMasters)))
                {
                    string table = Enum.GetName(typeof(ServerMasters), item);
                    string query = string.Format(baseQuery, item.ToString());
                    DBData bRows = new DBData();
                    bRows.type = (int)item;
                    bRows.tablename = table;
                    dBRows.Add(bRows);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = this.fromdate;
                    NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(command);
                    DataSet dset = new DataSet();
                    npgsqlDataAdapter.Fill(dset);
                    dset.Tables[0].TableName = table;

                    bRows.datas = Processors.DataTableCustomFormatter.Serialize(dset.Tables[0], true);
                    //bRows.data = Newtonsoft.Json.JsonConvert.SerializeObject(dset.Tables[0],new Newtonsoft.Json.JsonSerializerSettings()
                    //{
                    //    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                    //});
                }
                conn.Close();
                return dBRows;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "DownloadMasters", locationid);
                if (conn != null)
                    conn.Close();
            }

            return null;
        }

        public List<DBData> DownloadOfflineTransaction(bool all)
        {
            const string baseQuery = @"select source.* from {0} source
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.createdon>=@createdon and (sync_transaction.id is null or source.version != sync_transaction.version)";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in Enum.GetValues(typeof(ClientTransaction)))
                {
                    string table = Enum.GetName(typeof(ClientTransaction), item);
                    string query = string.Format(baseQuery, item.ToString());
                    if (all)
                    {
                        query = query.Replace("source.createdon>=@createdon and", "");
                    }

                    DBData bRows = new DBData();
                    bRows.type = (int)item;
                    bRows.tablename = table;
                    dBRows.Add(bRows);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = this.fromdate;
                    NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(command);
                    DataSet dset = new DataSet();
                    npgsqlDataAdapter.Fill(dset);
                    dset.Tables[0].TableName = table;
                    bRows.datas = Processors.DataTableCustomFormatter.Serialize(dset.Tables[0], true);
                }
                conn.Close();
                return dBRows;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "DownloadTransaction", locationid);
                if (conn != null)
                    conn.Close();
            }

            return null;
        }

        public List<DBData> DownloadTransaction()
        {
            const string baseQuery = @"select source.* from {0} source
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.createdon>=@createdon and (sync_transaction.id is null or source.version != sync_transaction.version)";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                List<DBData> dBRows = new List<DBData>();
                foreach (var item in Enum.GetValues(typeof(ServerTransaction)))
                {
                    string table = Enum.GetName(typeof(ServerTransaction), item);
                    string query = string.Format(baseQuery, item.ToString());

                    if ((ServerTransaction)item == ServerTransaction.stock)
                    {
                        query = @"select source.* from stock source
                            inner join salablegoods on salablegoods.stockid = source.id
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where salablegoods.locationid = @locationid and (sync_transaction.id is null or source.version != sync_transaction.version)";
                    }
                    else if ((ServerTransaction)item == ServerTransaction.salablegoods)
                    {
                        query = @"select source.* from salablegoods source
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.locationid = @locationid and (sync_transaction.id is null or source.version != sync_transaction.version)";
                    }
                    if (locationid <= 0)
                    {
                        query = query.Replace("source.locationid = @locationid and ", "");
                        query = query.Replace("salablegoods.locationid = @locationid and ", "");
                        query = query.Replace("sync_transaction.locationid=@locationid and ", "");
                    }
                    DBData bRows = new DBData();
                    bRows.type = (int)item;
                    bRows.tablename = table;
                    dBRows.Add(bRows);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)item;
                    command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                    command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = this.fromdate;
                    NpgsqlDataAdapter npgsqlDataAdapter = new NpgsqlDataAdapter(command);
                    DataSet dset = new DataSet();
                    npgsqlDataAdapter.Fill(dset);
                    dset.Tables[0].TableName = table;

                    //bRows.data = Newtonsoft.Json.JsonConvert.SerializeObject(dset.Tables[0], new Newtonsoft.Json.JsonSerializerSettings()
                    //{
                    //    DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat
                    //});

                    bRows.datas = Processors.DataTableCustomFormatter.Serialize(dset.Tables[0], true);

                    //JsonSerializer serializer = new JsonSerializer();
                    //serializer.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
                    //serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()
                    //{

                    //});

                    //serializer.NullValueHandling = NullValueHandling.Ignore;
                    //using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    //{
                    //    GZipStream gZipStream = new GZipStream(ms, CompressionMode.Compress);
                    //    System.IO.TextWriter textWriter = new System.IO.StreamWriter(gZipStream);
                    //    using (JsonTextWriter reader = new JsonTextWriter(textWriter))
                    //    {
                    //        serializer.Serialize(reader, dset.Tables[0]);
                    //    }
                    //    bRows.datas = ms.ToArray();
                    //}
                }
                conn.Close();
                return dBRows;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "DownloadTransaction", locationid);
                if (conn != null)
                    conn.Close();
            }
            return null;
        }

        public List<Models.Bill> DownloadBills(bool offline, bool all)
        {
            // or source.version != sync_transaction.version
            string baseQuery = @"select source.id,billsettlement.billsettlementmasterid from bill source
                            inner join billsettlement on billsettlement.billid = source.id
                            left outer join sync_transaction on sync_transaction.tabletype = @tabletype
                            and sync_transaction.clientid = @clientid and sync_transaction.locationid=@locationid and sync_transaction.sourceid = source.id
                            where source.offline = @offline and source.billdate >=@createdon and source.locationid=@locationid and (sync_transaction.id is null)
                            order by source.id
                            limit 500";
            if (locationid == -1)
            {
                baseQuery = baseQuery.Replace("and sync_transaction.locationid = @locationid", "");
                baseQuery = baseQuery.Replace("and source.locationid=@locationid", "");
            }
            List<long> ids = new List<long>();
            SortedList<long, long> settlementids = new SortedList<long, long>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(all ? baseQuery.Replace("source.offline = @offline and", "") : baseQuery, conn);
                command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                command.Parameters.Add("tabletype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)AllTransaction.bill;
                command.Parameters.Add("offline", NpgsqlTypes.NpgsqlDbType.Boolean).Value = offline;
                command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = this.fromdate;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt64(0));
                        settlementids.Add(reader.GetInt64(0), reader.GetInt64(1));
                    }
                }
                conn.Close();
                if (ids.Count == 0)
                    return new List<Models.Bill>();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log(exp, WebAPI.Common.Logger.LogType.Error, "DownloadBills", locationid);
                if (conn != null)
                    conn.Close();
            }

            Data.BillDataService billDataService = new Data.BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var bills = billDataService.Search(new Models.BillCreteria()
            {
                ids = ids,
                isimport=true
            });
            Data.BillsettlementmasterDataService billsettlementmasterDataService = new Data.BillsettlementmasterDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var settlements = billsettlementmasterDataService.Search(new Models.BillsettlementmasterCreteria()
            {
                ids = settlementids.Values.ToList()
            });
            foreach (var bill in bills)
            {
                if (bill.billattributes == null)
                    bill.billattributes = new Models.BillAttribute();
                bill.billattributes.settlement = settlements.Find(e => e.id == settlementids[bill.id]);
            }
            return bills;
        }

        public class BarcodeInfo
        {
            public long stockid { get; set; }
            public long salablegoodsid { get; set; }
        }

        public List<Services.SyncManager.DBData> UploadBills(Controllers.SyncController.UploadBillRequest request)
        {
            NpgsqlConnection conn = null;
            try
            {
                if (request.bills == null || request.bills.Count == 0) return new List<DBData>();
                List<Models.Bill> bills = request.bills;
                bool offline = request.offline;
                WebAPI.Common.Logger.Instance(0).Log("UploadBills from " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, bills.Count);
                List<DBData.RowStatus> localChangeList = new List<DBData.RowStatus>();

                string baseQuery = @"select billno,id from bill
                                 where billno in (" + String.Join(",", bills.ConvertAll(e => "'" + e.billno + "'").ToList()) + ")";

                List<string> barcodes = new List<string>();
                foreach (var item in bills)
                {
                    item.issynchronize = true;
                    item.updatestock = (!LicenseManager.Instance(0).GetLicense().isoffline && offline) || (LicenseManager.Instance(0).GetLicense().isoffline && offline);
                    foreach (var barcode in item.Billitems)
                    {
                        barcodes.Add("'" + barcode.barcode + "'");
                    }
                }

                if (barcodes.Count == 0) return new List<DBData>();

                string barcodeidQuery = "select distinct barcode,id,stockid,locationid from salablegoods where locationid=" + bills.First().locationid + " and barcode in (" + string.Join(",", barcodes) + ")";
                if (locationid <= 0)
                {
                    WebAPI.Common.Logger.Instance(0).PerformanceLog("Location id is missing", WebAPI.Common.Logger.LogType.Sync,
                        "",request,0,locationid);
                    barcodeidQuery = "select distinct barcode,id,stockid,locationid from salablegoods where barcode in (" + string.Join(",", barcodes) + ")";
                }

                SortedDictionary<string, long> ids = new SortedDictionary<string, long>();
                SortedDictionary<string, BarcodeInfo> barcodeInfos = new SortedDictionary<string, BarcodeInfo>();
                conn = CreateConnection();

                conn.Open();
                var transaction = conn.BeginTransaction();
                NpgsqlCommand command = new NpgsqlCommand(baseQuery, conn);

                command.Parameters.Add("clientid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = clientid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = locationid;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetString(0), reader.GetInt64(1));
                    }
                }

                command.CommandText = barcodeidQuery;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        barcodeInfos.Add(reader.GetString(0) + "-" + reader.GetInt64(3), new BarcodeInfo()
                        {
                            salablegoodsid = reader.GetInt64(1),
                            stockid = reader.GetInt64(2)
                        });
                    }
                }

                var retunvalues = new List<Services.SyncManager.DBData>();
                Data.BillDataService billDataService = new Data.BillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

                List<DBData.RowStatus> changeList = new List<DBData.RowStatus>();

                Data.CustomerDataService customerDataService = new Data.CustomerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

                var settlementlist = new List<Models.Billsettlementmaster>();

                foreach (var item in bills)
                {
                    bool updated = false;
                    var billid = item.id;
                    Models.Billsettlementmaster billsettlementmaster = null;
                    if (settlementlist.Exists(e => e.Billsettlement != null && e.Billsettlement.Exists(x => x.billid == billid)))
                    {
                        billsettlementmaster = settlementlist.Find(e => e.Billsettlement != null && e.Billsettlement.Exists(x => x.billid == billid));
                    }
                    else if (item.billattributes.settlement != null)
                    {
                        billsettlementmaster = item.billattributes.settlement;
                        settlementlist.Add(billsettlementmaster);
                    }

                    item.billattributes.settlement = null;
                    if (item.customer != null)
                    {
                        var customers = customerDataService.Search(new Models.CustomerCreteria()
                        {
                            mobileno = item.customer.mobileno
                        });

                        if (customers.Count > 1)
                        {
                            customers = customerDataService.Search(new Models.CustomerCreteria()
                            {
                                mobileno = item.customer.mobileno,
                                name = item.customer.name
                            });
                        }

                        if (customers.Count == 0)
                        {
                            item.customer.id = 0;
                            customerDataService.Insert(item.customer);
                        }
                        else
                        {
                            item.customer.id = customers[0].id;
                        }
                        item.customerid = item.customer.id;
                    }

                    if (ids.ContainsKey(item.billno))
                    {
                        item.id = ids[item.billno];
                        billDataService.Update(item, false);
                        updated = true;
                    }
                    else
                    {
                        item.id = 0;
                        item.Billitems.ForEach(e =>
                        {
                            if (barcodeInfos.ContainsKey(e.barcode + "-" + item.locationid))
                            {
                                e.stockid = barcodeInfos[e.barcode + "-" + item.locationid].stockid;
                                e.salablegoodsid = barcodeInfos[e.barcode + "-" + item.locationid].salablegoodsid;
                                e.id = 0;
                            }
                            else
                            {
                                e.stockid = -3;
                                e.salablegoodsid = -3;
                                e.id = 0;
                            }
                        });
                        billDataService.Insert(transaction, conn, item, new Models.Billmaster()
                        {
                            id = item.billmasterid,
                            total = item.total
                        });

                        updated = true;
                    }


                    if (updated)
                    {
                        changeList.Add(new DBData.RowStatus()
                        {
                            code = item.billno,
                            version = item.version,
                            id = billid
                        });

                        if (billsettlementmaster != null && billsettlementmaster.Billsettlement.Exists(e => e.billno == item.billno))
                        {
                            billsettlementmaster.Billsettlement.Find(e => e.billno == item.billno).billid = item.id;
                        }

                        if (!offline && item.offline)
                        {
                            localChangeList.Add(new DBData.RowStatus()
                            {
                                code = item.billno,
                                version = item.version,
                                id = item.id
                            });
                        }
                    }
                }

                retunvalues.Add(new Services.SyncManager.DBData()
                {
                    status = changeList,
                    tablename = "bill",
                    type = (int)Services.SyncManager.BillTransaction.bill
                });

                transaction.Commit();
                baseQuery = @"select code,id,locationid from billsettlementmaster
                                 where locationid=" + bills.First().locationid + " and code in (" + String.Join(",", settlementlist.ConvertAll(e => "'" + e.code + "'").ToList()) + ")";

                if (locationid <= 0)
                    baseQuery = @"select code,id,locationid from billsettlementmaster
                                 where code in (" + String.Join(",", settlementlist.ConvertAll(e => "'" + e.code + "'").ToList()) + ")";

                command = new NpgsqlCommand(baseQuery, conn);
                ids.Clear();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetString(0) + "-" + reader.GetInt64(2), reader.GetInt64(1));
                    }
                }
                conn.Close();


                Data.BillsettlementmasterDataService billsettlementmasterDataService = new Data.BillsettlementmasterDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                List<long> setteledbillids = new List<long>();
                foreach (var settlement in settlementlist)
                {
                    settlement.IsSynchronize = true;
                    settlement.isactive = true;
                    settlement.Billsettlement.ForEach(e =>
                    {
                        e.IsSynchronize = true;
                        e.isactive = true;
                    });
                    if (ids.ContainsKey(settlement.code + "-" + settlement.locationid))
                    {
                        //settlement.id = ids[settlement.code + "-" + settlement.locationid];
                        //billsettlementmasterDataService.Update(settlement);
                    }
                    else
                    {
                        billsettlementmasterDataService.InsertDirect(settlement);
                    }
                    setteledbillids.AddRange(settlement.Billsettlement.ConvertAll(e => e.billid));
                }

                billsettlementmasterDataService.ExecuteQuery("Update bill set settled=true where id in (" + string.Join(",", setteledbillids) + ")  and isactive=true", null);

                if (localChangeList != null && localChangeList.Count > 0)
                {
                    Sync_Server sync_Server = new Sync_Server(organizationid, SyncManager.Instance.Configuration.Client, SyncManager.Instance.Configuration.Location, SyncManager.Instance.Configuration.PrivateKey);
                    var retunLocalvalues = new List<Services.SyncManager.DBData>();
                    retunLocalvalues.Add(new Services.SyncManager.DBData()
                    {
                        status = localChangeList,
                        tablename = "bill",
                        type = (int)Services.SyncManager.BillTransaction.bill
                    });
                    sync_Server.ChangeTransactionStatus(retunLocalvalues);
                }
                ResetSequence("bill");
                ResetSequence("billitems");
                ResetSequence("billmaster");
                ResetSequence("billsettlementmaster");
                ResetSequence("billsettlement");
                ResetSequence("customer");
                ResetSequence("customerpayment");
                ResetSequence("customerrewards");

                return retunvalues;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(0).Log("UploadBills failed from " + request.location + " by " + request.clientid, WebAPI.Common.Logger.LogType.Sync, exp);
                if (conn != null)
                    conn.Close();
                return new List<DBData>();
            }
        }

        public bool ResetSequence(string tablename)
        {
            string sequencename = "";
            string query = @"
with sequencelist 
as
(
WITH fq_objects AS (SELECT c.oid,n.nspname || '.' ||c.relname AS fqname , 
                           c.relkind, c.relname AS relation 
                    FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace ),

     sequences AS (SELECT oid,fqname FROM fq_objects WHERE relkind = 'S'),  
     tables    AS (SELECT oid, fqname FROM fq_objects WHERE relkind = 'r' )  
SELECT
       s.fqname AS sequence, 
       '->' as depends, 
       t.fqname AS table 
FROM 
     pg_depend d JOIN sequences s ON s.oid = d.objid  
                 JOIN tables t ON t.oid = d.refobjid  
WHERE 
     d.deptype = 'a'
)
select substring(sequence ,8) as sequencename,substring(""table"",8) tablename from sequencelist where substring(""table"" ,8)=@tablename";
            var param = new SortedList<string, object>();
            param.Add("tablename", tablename);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                    sequencename = reader.GetString(0);
            }

            long lastidentity = 0;
            if (!string.IsNullOrEmpty(sequencename))
            {
                query = "ALTER SEQUENCE {0} RESTART WITH {1}";
                using (var reader = ExecuteReader("select max(id) from " + tablename, param))
                {
                    if (reader.Read())
                        lastidentity = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                }
                if (lastidentity > 0)
                {
                    return ExecuteQuery(string.Format(query, sequencename, lastidentity + 1), param) > 0;
                }
            }
            return false;
        }
    }
}