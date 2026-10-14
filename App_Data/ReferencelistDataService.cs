using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Text.RegularExpressions;

namespace InfyPOS.Data

{
    /*INSERT INTO referencelist(
            isactive, createdby, modifiedby, version, createdon, modifiedon,
            type, code, name, additionalinfo, sortingindex)
    VALUES( true, 1, 1, 1, current_date, current_date,
           'SYSTEMDOMAIN', 'LEAVETYPE', 'LEAVE TYPE', NULL, 1);*/
    public class ReferencelistDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, type, code, name, additionalinfo, sortingindex,productgroup,referenceid,referencelocationid from referencelist where id=@id";

        const string SQL_SearchList = @"select id, code, name, type, cast(additionalinfo->>'locationid' as bigint) as locationid,cast(additionalinfo->>'parentid' as bigint) as parentid from referencelist ";

        const string SQL_Insert = @"INSERT INTO public.referencelist(isactive, createdby, modifiedby, version, createdon, modifiedon, type, code, name, additionalinfo, sortingindex,productgroup,referenceid,referencelocationid) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @type, @code, @name, @additionalinfo, @sortingindex,@productgroup,@referenceid,@referencelocationid) RETURNING id";

        const string SQL_Update = @"UPDATE public.referencelist SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, type=@type, code=@code, name=@name, additionalinfo=@additionalinfo,referencelocationid=@referencelocationid, sortingindex=@sortingindex,productgroup=@productgroup,referenceid=@referenceid,isactive=@isactive where id=@id and version=@version  RETURNING version";

        const string SQL_UpdateStatus = @"UPDATE public.referencelist SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, isactive=@isactive where id=@id and version=@version  RETURNING version";
        internal List<ReferenceSizeGroup> SearchSizeGroups()
        {
            var list = Search(new ReferencelistCreteria()
            {
                type = "SIZEGROUP"
            });

            var sizelist = Search(new ReferencelistCreteria()
            {
                type = "SIZE"
            });
            List<ReferenceSizeGroup> retlist = new List<ReferenceSizeGroup>();
            foreach (var item in list)
            {
                if (item.additionalinfo != null &&
                    item.additionalinfo.sizegroups != null
                    && item.additionalinfo.sizegroups.Count > 0)
                {
                    List<ReferenceItem> lst = new List<ReferenceItem>();
                    item.additionalinfo.sizegroups.ForEach(e =>
                    {
                        if (sizelist.Exists(x => x.id == e))
                        {
                            lst.Add(new ReferenceItem()
                            {
                                name = sizelist.Find(x => x.id == e).name,
                                id = e
                            });
                        }
                    });
                    retlist.Add(new ReferenceSizeGroup()
                    {
                        id = item.id,
                        name = item.name,
                        list = lst
                    });
                }
            }
            return retlist;
        }

        const string SQL_Delete = @"update public.referencelist set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<ReferenceItem> SearchList(ReferencelistCreteria _referencelist)
        {
            if (_referencelist.addmissingnames && _referencelist.names.Count > 0)
            {
                _referencelist.names.RemoveAll(x => string.IsNullOrEmpty(x));
                if ((_referencelist.names == null || _referencelist.names.Count == 0))
                    return new List<ReferenceItem>();
            }

            List<ReferenceItem> lst = new List<ReferenceItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchList;

                if (!_referencelist.inactive)
                    qb.AddParameter("isactive", true, "=");

                if (_referencelist.id != 0)
                {
                    qb.AddParameter("id", _referencelist.id, "=");
                }
                if (_referencelist.ids != null && _referencelist.ids.Count > 0)
                {
                    qb.AddInParamer("referencelist.id", string.Join<long>(",", _referencelist.ids));
                }

                if (_referencelist.types != null && _referencelist.types.Count > 0)
                {
                    qb.AddInParamer("type", string.Join(",", _referencelist.types.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                }
                else if (!string.IsNullOrEmpty(_referencelist.type))
                {
                    qb.AddParameter("type", _referencelist.type, "=");
                }

                if (!string.IsNullOrEmpty(_referencelist.name))
                {
                    qb.AddParameter("name", _referencelist.name, "=");
                }

                if (_referencelist.names != null && _referencelist.names.Count>0)
                {
                    _referencelist.names.RemoveAll(x => string.IsNullOrEmpty(x));
                    qb.AddInParamer("upper(name)", string.Join(",", _referencelist.names.ConvertAll(x=>"'"+ WebAPI.Data.Utility.SafeSQL(x.ToUpper())+"'")));
                }

                qb.SortField = "sortingindex,name";
                if (_referencelist.productgroup > 0)
                    qb.AddParameter("productgroup", _referencelist.productgroup, " = ");

                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ReferenceItem oreferencelist = new ReferenceItem();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        oreferencelist.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();

                if(_referencelist.addmissingnames)
                {
                    var missingnames = _referencelist.names.FindAll(x => !lst.Exists(y => y.name.ToUpper() == x.ToUpper()));
                    if(missingnames.Count > 0)
                    {
                        var inserted = InsertAll(missingnames.ConvertAll(x => new Referencelist()
                        {
                            isactive = true,
                            type = _referencelist.type,
                            name = x
                        }));
                        lst.AddRange(inserted.ConvertAll(y => new ReferenceItem()
                        {
                             id =y.id,
                             name = y.name,
                             type = y.type,
                             code = y.code
                        }));
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }


        public List<ReferenceItem> SearchListWithParent(ReferencelistCreteria _referencelist)
        {
            List<ReferenceItem> lst = new List<ReferenceItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchList;

                if (_referencelist.ids != null)
                {
                    qb.AddInParamer("referencelist.id", string.Join<long>(",", _referencelist.ids));
                }

                if (_referencelist.types != null && _referencelist.types.Count > 0)
                {
                    qb.AddInParamer("type", string.Join(",", _referencelist.types.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                }
                else if (!string.IsNullOrEmpty(_referencelist.type))
                {
                    qb.AddParameter("type", _referencelist.type, "=");
                }

                if (!string.IsNullOrEmpty(_referencelist.name))
                    qb.AddParameter("name", _referencelist.name, "=");

                qb.SortField = "sortingindex,name";
                if (_referencelist.productgroup > 0)
                    qb.AddParameter("productgroup", _referencelist.productgroup, " = ");

                if (_referencelist.referenceid > 0)
                    qb.AddParameter("referenceid", _referencelist.referenceid, " = ");

                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ReferenceItem oreferencelist = new ReferenceItem();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oreferencelist.productgroup = reader["parentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["parentid"]);
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }
        public List<LedgerHierarchy> SearchLedgerHierarchy(string typeid)
        {
            List<LedgerHierarchy> lst = new List<LedgerHierarchy>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string SQL_Search = @"select id, name,additionalinfo, version,referencelocationid from referencelist where type='LEDGERHIERARCHY' and referencelocationid=" + typeid;
                SQL_Search += " limit 1";
                NpgsqlCommand command = new NpgsqlCommand(SQL_Search, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        LedgerHierarchy oreferencelist = new LedgerHierarchy();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreferencelist.children_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }


        public LedgerHierarchy SaveLedgerHierarchy(LedgerHierarchy item)
        {

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                if (item.id == 0)
                {


                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Ledger Hierarchy -" + item.name;
                    command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.name;
                    command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
                    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "LEDGERHIERARCHY";
                    command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = item.children_json == null ? "null" : item.children_json;
                    command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.typeid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item.id = Convert.ToInt64(reader[0].ToString());
                        }
                    }
                }
                else
                {
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "LEDGER HEIRARCHY -" + item.typeid;
                    command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "LEDGER HEIRARCHY -" + item.typeid;
                    command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
                    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "LEDGERHIERARCHY";
                    command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = item.children_json == null ? "null" : item.children_json;
                    command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.typeid;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item.version = Convert.ToInt32(reader[0]);
                        }
                    }
                }

                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return item;
        }


        public BinStorageRefrenceList SaveBinStorage(BinStorageRefrenceList item)
        {
            List<BinStorageRefrenceList> lst = new List<BinStorageRefrenceList>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                if (item.id == 0)
                {
                    var newBinList = item.GetAllNewBin(false);
                    MfgbinstorageDataService mfgbinstorageDataService = new MfgbinstorageDataService();
                    mfgbinstorageDataService.InsertAll(newBinList, conn, transaction);

                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Bin Storage -" + item.name;
                    command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Bin Storage -" + item.name;
                    command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
                    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "BINHIERARCHY";
                    command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = item.children_json == null ? "null" : item.children_json;
                    command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.referencelocationid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item.id = Convert.ToInt64(reader[0].ToString());
                        }
                    }
                }
                else
                {
                    var newBinList = item.GetAllNewBin(false);
                    var alteredList = item.GetAllNewBin(true);
                    MfgbinstorageDataService mfgbinstorageDataService = new MfgbinstorageDataService();
                    mfgbinstorageDataService.InsertAll(newBinList, conn, transaction);
                    mfgbinstorageDataService.ChangeAll(alteredList, conn, transaction);
                    if (item.DeletedBinList != null && item.DeletedBinList.Count > 0)
                        mfgbinstorageDataService.DeleteAll(item.DeletedBinList, conn, transaction);
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Bin Storage -" + item.referencelocationid;
                    command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "Bin Storage -" + item.referencelocationid;
                    command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
                    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.version;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "BINHIERARCHY";
                    command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = item.children_json == null ? "null" : item.children_json;
                    command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.referencelocationid;

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            item.version = Convert.ToInt32(reader[0]);
                        }
                    }
                }

                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return item;
        }

        public List<BinStorageRefrenceList> SearchBinStorages(string locationid)
        {
            List<BinStorageRefrenceList> lst = new List<BinStorageRefrenceList>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string SQL_Search = @"select id, name,additionalinfo, version,referencelocationid from referencelist where type='BINHIERARCHY'";
                SQL_Search += " and referencelocationid=" + locationid;
                SQL_Search += " limit 1";
                NpgsqlCommand command = new NpgsqlCommand(SQL_Search, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        BinStorageRefrenceList oreferencelist = new BinStorageRefrenceList();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreferencelist.referencelocationid = reader["referencelocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referencelocationid"]);
                        oreferencelist.children_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<Referencelist> Search(ReferencelistCreteria _referencelist)
        {
            List<Referencelist> lst = new List<Referencelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_referencelist.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    var hascondition = false;
                    if (!_referencelist.inactive)
                        qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_referencelist.type))
                    {
                        qb.AddParameter("type", _referencelist.type, "=");
                        hascondition = true;
                    }
                    if (!string.IsNullOrEmpty(_referencelist.name))
                    {
                        qb.AddParameter("name", "%" + _referencelist.name + "%", " ilike ");
                        hascondition = true;
                    }
                    if (!string.IsNullOrEmpty(_referencelist.code))
                    {
                        qb.AddParameter("code", _referencelist.code, " ilike ");
                        hascondition = true;
                    }
                    if (_referencelist.ids != null)
                    {
                        qb.AddInParamer("referencelist.id", string.Join<long>(",", _referencelist.ids));
                        hascondition = true;
                    }

                    if (_referencelist.locationid > 0)
                    {
                        qb.AddParameter("referencelocationid", _referencelist.locationid, " = ");
                        hascondition = true;
                    }
                    else if (_referencelist.referencelocationid > 0)
                    {
                        qb.AddParameter("referencelocationid", _referencelist.referencelocationid, " = ");
                        hascondition = true;
                    }
                    if (_referencelist.referenceid > 0)
                    {
                        qb.AddParameter("referenceid", _referencelist.referenceid, " = ");
                        hascondition = true;
                    }

                    if (_referencelist.productgroup > 0)
                    {
                        qb.AddParameter("productgroup", _referencelist.productgroup, " = ");
                        hascondition = true;
                    }
                    if (!hascondition && !_referencelist.searchall)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("ReferenceList - DirectHit", WebAPI.Common.Logger.LogType.Info,
                            "Referencelist.search", Environment.StackTrace, Context.UserID, 0);
                    }
                    qb.SortField = "sortingindex,name";
                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _referencelist.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Referencelist oreferencelist = new Referencelist();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreferencelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oreferencelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oreferencelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oreferencelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oreferencelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oreferencelist.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.sortingindex = reader["sortingindex"] == DBNull.Value ? 0 : (int)reader["sortingindex"];
                        oreferencelist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        oreferencelist.productgroup = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oreferencelist.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                        oreferencelist.locationid = reader["referencelocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referencelocationid"]);
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }
        public List<Referencelist> SearchMyRetailLocation(ReferencelistCreteria _referencelist)
        {
            string SQL_SearchMyLocation = @"select referencelist.* from referencelist where type='LOCATION' and referencelist.isactive = true  
                                    and cast(additionalinfo::json ->> 'iswarehouse' as boolean) = false and
( id in (
select cast(itemlist->> 'id' as bigint) from users inner join LATERAL json_array_elements (users.accessiblelocation :: json) itemlist ON true
where cast(itemlist->> 'view' as boolean)=true and users.id=@userid)
or 
 -1 in (
select cast(itemlist->> 'id' as bigint) from users inner join LATERAL json_array_elements (users.accessiblelocation :: json) itemlist ON true
where cast(itemlist->> 'view' as boolean)=true and users.id=@userid))
order by sortingindex,name";

            List<Referencelist> lst = new List<Referencelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = new NpgsqlCommand(SQL_SearchMyLocation, conn);
                command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Referencelist oreferencelist = new Referencelist();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreferencelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oreferencelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oreferencelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oreferencelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oreferencelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oreferencelist.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.sortingindex = reader["sortingindex"] == DBNull.Value ? 0 : (int)reader["sortingindex"];
                        oreferencelist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        oreferencelist.productgroup = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oreferencelist.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<Referencelist> SearchMyLocation(ReferencelistCreteria _referencelist)
        {
            string SQL_SearchMyLocation = @"select referencelist.* from referencelist where type='LOCATION' and referencelist.isactive = true and
( id in (
select cast(itemlist->> 'id' as bigint) from users inner join LATERAL json_array_elements (users.accessiblelocation :: json) itemlist ON true
where cast(itemlist->> 'view' as boolean)=true and users.id=@userid)
or 
 -1 in (
select cast(itemlist->> 'id' as bigint) from users inner join LATERAL json_array_elements (users.accessiblelocation :: json) itemlist ON true
where cast(itemlist->> 'view' as boolean)=true and users.id=@userid))
order by sortingindex,name";

            List<Referencelist> lst = new List<Referencelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = new NpgsqlCommand(SQL_SearchMyLocation, conn);
                command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Referencelist oreferencelist = new Referencelist();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreferencelist.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oreferencelist.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oreferencelist.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oreferencelist.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oreferencelist.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oreferencelist.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.sortingindex = reader["sortingindex"] == DBNull.Value ? 0 : (int)reader["sortingindex"];
                        oreferencelist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        oreferencelist.productgroup = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oreferencelist.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<Referencelist> SearchWHLocation(ReferencelistCreteria _referencelist)
        {
            string SQL_WHLocation = @"select id, code, name,additionalinfo from referencelist where type='LOCATION' and isactive=true and
                                            cast(additionalinfo::json ->> 'iswarehouse' as boolean) = true";

            List<Referencelist> lst = new List<Referencelist>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = new NpgsqlCommand(SQL_WHLocation, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Referencelist oreferencelist = new Referencelist();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }

            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<ReferenceItem> SearchRetLocation(ReferencelistCreteria _referencelist)
        {
            string SQL_SearchRetLocation = @"select id, code, name,cast(additionalinfo::json ->> 'iswarehouse' as boolean),cast(additionalinfo->>'parentid' as bigint) as parentid  from referencelist where isactive = true and type='LOCATION' and 
                                            coalesce(cast(additionalinfo::json ->> 'iswarehouse' as boolean),false) = false";

            List<ReferenceItem> lst = new List<ReferenceItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = new NpgsqlCommand(SQL_SearchRetLocation, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ReferenceItem oreferencelist = new ReferenceItem();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.productgroup = reader["parentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["parentid"]);
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }

            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public List<Referencelist> InsertAll(List<Referencelist> oReferencelist)
        {
            if (oReferencelist == null || oReferencelist.Count == 0) return oReferencelist;

            List<Referencelist> existing = Search(new ReferencelistCreteria() { type = oReferencelist[0].type });
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(this.Context.OrganizationID).Get(oReferencelist[0].type + ".code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    foreach (var reference in oReferencelist)
                    {
                        if (!existing.Exists(e => e.name == reference.name))
                        {
                            if (string.IsNullOrEmpty(reference.code))
                                reference.code = sequence.GetNextNumber(Context, conn, transaction);

                            if (reference.additionalinfo == null) reference.additionalinfo = new Referencelist.Attribute();
                            Insert(transaction, conn, reference);
                            if (reference.id > 0 && !isMigration)
                            {
                                InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                                {
                                    id = reference.id,
                                    type = "CACHE",
                                    subtype = "REFERENCELIST-" + reference.type,
                                    actionon = reference.modifiedon
                                }, conn);
                            }
                        }
                        else
                        {
                            reference.id = existing.Find(e => e.name == reference.name).id;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oReferencelist;
        }
        private int DuplicateValidation(Referencelist oReferencelist)
        {
            if (string.IsNullOrEmpty(oReferencelist.type)) return 0;
            if (oReferencelist.type != "LOCATION") return 0;

            string _Query = @"select count(id) items from referencelist where isactive=true and type='LOCATION' 
                        and cast(additionalinfo->> 'iswarehouse' as boolean) = true and
                    case when @id=0 then id <> 0 else id!=@id end";

            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count;
        }

        public int GetAgreementPOType()
        {
            string _Query = @"select id from referencelist where isactive=true and type='POTYPE' 
                        and cast(additionalinfo->> 'issales' as boolean) = true ";

            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]);
                    }
                }
                conn.Close();
            }
            return count;
        }

        private bool UniqueValidation(Referencelist oReferencelist)
        {
            if (string.IsNullOrEmpty(oReferencelist.type)) return false;

            if (oReferencelist.type.StartsWith("INCENTIVE")) return false;

            if (string.IsNullOrEmpty(oReferencelist.name)) return false;

            string _Query = @"select count(id) items from referencelist where isactive=true and type=@type 
                        and lower(name)=@name and id!=@id";

            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.id;
                command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.type;
                command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oReferencelist.name) ? "" : oReferencelist.name.ToLower();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count > 0;
        }

        public bool isMigration = false;
        public Referencelist Insert(Referencelist oReferencelist)
        {
            //if (oReferencelist.type == "LOCATION" && oReferencelist.additionalinfo != null &&
            //    oReferencelist.additionalinfo.iswarehouse == true && DuplicateValidation(oReferencelist) > 0)
            //{
            //    oReferencelist.ErrorMessage = "Multiple warehouse can't be supported.";
            //    return oReferencelist;
            //}

            if (!isMigration && UniqueValidation(oReferencelist))
            {
                oReferencelist.ErrorMessage = "Duplicate Name with the given Type is Exists.";
                return oReferencelist;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(this.Context.OrganizationID).Get(oReferencelist.type + ".code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (string.IsNullOrEmpty(oReferencelist.code))
                        oReferencelist.code = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oReferencelist);

                    transaction.Commit();
                    if (!isMigration && oReferencelist.id > 0)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oReferencelist.id, type = "CACHE", subtype = "REFERENCELIST-" + oReferencelist.type, actionon = oReferencelist.modifiedon }, conn);
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oReferencelist;
        }

        internal bool Marge(MergeRefrenceList item)
        {
            Referencelist _newitem = null;
            if (item.id == 0)
            {
                item.ErrorMessage = "There is no Item selected as Merge To";
                return false;
            }
            NpgsqlTransaction transaction = null;
            try
            {
                if (!item.deletelist.Exists(e => e.id == item.id))
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                if (item.deletelist.Exists(e => e.id == item.id))
                    _newitem = item.deletelist.Find(e => e.id == item.id);

                if (_newitem == null)
                {
                    item.ErrorMessage = "Merge To Item must be selected from master list of items.";
                    return false;
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    item.deletelist.RemoveAll(e => e.id == item.id);
                    foreach (var ditem in item.deletelist)
                    {
                        Delete(transaction, conn, ditem);
                    }

                    string query = "";
                    switch (_newitem.type)
                    {
                        case "SIZE":
                            query = "update items set sizeid=@newid, sizename=@newname where sizeid=@oldid";
                            break;
                        case "PATTERN":
                            query = "update items set patternid=@newid, patternname=@newname where patternid=@oldid";
                            break;
                        case "STYLE":
                            query = "update items set styleid=@newid, stylename=@newname where styleid=@oldid";
                            break;
                        case "COLOUR":
                            query = "update items set colourid=@newid, colourname=@newname where colourid=@oldid";
                            break;
                        case "MATERIAL":
                            query = "update items set materialid=@newid, materialname=@newname where materialid=@oldid";
                            break;
                        case "TYPE":
                            query = "update items set typeid=@newid, typename=@newname where typeid=@oldid";
                            break;
                        case "FIT":
                            query = "update items set fitid=@newid where fitid=@oldid";
                            break;
                        case "SLEEVE":
                            query = "update items set sleeveid=@newid where sleeveid=@oldid";
                            break;
                        case "PRODUCTGROUPS":
                            query = @"update products set productgroup=@newid where productgroup=@oldid;
                                      update referencelist set productgroup=@newid where productgroup=@oldid;";
                            break;
                        case "CITY":
                            query = @"update agent set cityid=@newid where cityid=@oldid;
                                      update company set cityid=@newid where cityid=@oldid;
                                      update customer set cityid=@newid where cityid=@oldid;
                                      update organisation set cityid=@newid where cityid=@oldid;
                                      update suppliercompany set cityid=@newid where cityid=@oldid;
                                      update transport set cityid=@newid where cityid=@oldid;";
                            break;
                    }
                    if (!string.IsNullOrEmpty(query))
                    {
                        NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                        command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("newname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _newitem.name;
                        command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        foreach (var ditem in item.deletelist)
                        {
                            command.Parameters["oldid"].Value = ditem.id;
                            int successCount = command.ExecuteNonQuery();
                        }
                        if (item.type == "CITY")
                        {
                            MergeCity(item, conn, transaction);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                return false;
            }
            return true;
        }

        internal Referencelist ManageAttribute(ManageReference item)
        {
            if (item.type == "PRODUCT")
            {
                ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                var product = productsDataService.Insert(new Products()
                {
                    name = item.name,
                    code = item.code,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow,
                    producttype = item.producttype,
                    purchasetaxid = item.purchasetaxid,
                    salestaxid = item.salestaxid,
                    barcodemode = item.barcodemode,
                    sellingmode = item.sellingmode
                });

                return new Referencelist()
                {
                    id = product.id,
                    name = product.name,
                    code = product.code,
                    type = "PRODUCT"
                };
            }
            if (item.type == "BRAND")
            {
                BrandDataService productsDataService = new BrandDataService() { Context = this.Context };
                var product = productsDataService.Insert(new Brand()
                {
                    name = item.name,
                    code = item.code,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow
                });
                return new Referencelist()
                {
                    id = product.id,
                    name = product.name,
                    code = product.code,
                    type = "BRAND"
                };
            }
            if (item.type == "TRANSPORT")
            {
                TransportDataService productsDataService = new TransportDataService() { Context = this.Context };
                var product = productsDataService.Insert(new Transport()
                {
                    name = item.name,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow
                });
                return new Referencelist()
                {
                    id = product.id,
                    name = product.name,
                    type = "TRANSPORT"
                };
            }
            if (item.type == "SUPPLIER")
            {
                SupplierDataService productsDataService = new SupplierDataService() { Context = this.Context };
                var company = new List<Suppliercompany>();
                company.Add(new Suppliercompany()
                {
                    name = item.name,
                    code = item.code,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow
                });

                var supplier = productsDataService.Insert(new Supplier()
                {
                    name = item.name,
                    code = item.code,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow,
                    companylist = company
                });
                return new Referencelist()
                {
                    id = supplier.id,
                    name = supplier.name,
                    code = supplier.code,
                    type = "SUPPLIER"
                };
            }
            if (item.type == "PRICEDESIGN")
            {
                CustomerpriceDataService productsDataService = new CustomerpriceDataService() { Context = this.Context };
                var supplier = productsDataService.Insert(new Customerprice()
                {
                    designid = item.name,
                    isactive = true,
                    createdby = Context.UserID,
                    modifiedby = Context.UserID,
                    createdon = DateTimeNow,
                    modifiedon = DateTimeNow
                });
                return new Referencelist()
                {
                    id = supplier.id,
                    name = supplier.designid,
                    type = "PRICEDESIGN"
                };
            }
            return Insert(item);
        }

        private void MergeCity(MergeRefrenceList item, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {

        }

        public Referencelist Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Referencelist oReferencelist)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            var isactive = (oReferencelist.type == "SIZE" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_size) ||
   (oReferencelist.type == "STYLE" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_style) ||
   (oReferencelist.type == "PATTERN" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pattern) ||
   (oReferencelist.type == "COLOUR" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_color) ||
   (oReferencelist.type == "MATERIAL" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_material) ||
   (oReferencelist.type == "TYPE" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_type);

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = !isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.type == null ? "" : oReferencelist.type;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.code == null ? "" : oReferencelist.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.name == null ? "" : oReferencelist.name;
            command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oReferencelist.additionalinfo_json == null ? "null" : oReferencelist.additionalinfo_json;
            command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.sortingindex;
            command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.productgroup;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.referenceid;
            command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oReferencelist.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            if (isactive)
            {
                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Attributes,
                    oReferencelist.id, string.Format("PRODUCT ATTRIBUTE - NEW OF TYPE {0} IS {1}", oReferencelist.type,
                        oReferencelist.name), Context.UserID, 0, transaction, Context);
            }

            return oReferencelist;
        }

        internal List<long> DeleteByID(ReferencelistCreteria request)
        {
            try
            {
                var param = new SortedList<string, object>();
                param.Add("type", request.type);
                string query = "Update referencelist set isactive=false where type=@type and id in (" + string.Join<long>(",", request.ids) + ")";
                if (ExecuteQuery(query, param) == request.ids.Count)
                    return request.ids;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return new List<long>();
        }

        public Referencelist Update(Referencelist oReferencelist)
        {
            oReferencelist.isactive = true;
            //if (oReferencelist.type == "LOCATION" && oReferencelist.additionalinfo != null &&
            //    oReferencelist.additionalinfo.iswarehouse == true && DuplicateValidation(oReferencelist) > 0)
            //{
            //    oReferencelist.ErrorMessage = "Multiple warehouse can't be supported.";
            //    return oReferencelist;
            //}

            if (!isMigration && UniqueValidation(oReferencelist))
            {
                oReferencelist.ErrorMessage = "Duplicate Name with the given Type is Exists.";
                return oReferencelist;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    int version = oReferencelist.version;
                    Update(transaction, conn, oReferencelist);

                    if(oReferencelist.type == "PRODUCTGROUPS" &&
                        oReferencelist.additionalinfo != null &&
                        oReferencelist.additionalinfo.hierarchyconfig != null)
                    {
                        string query = @"Update products set productgroup_1=@productgroup_1,
                                        productgroup_2=@productgroup_2,
                                        productgroup_3=@productgroup_3,
                                        productgroup_4=@productgroup_4,
                                        productgroup_5=@productgroup_5
                                        where productgroup=@productgroup";
                        var param = new SortedList<string, object>();
                        param.Add("productgroup", oReferencelist.id);
                        param.Add("productgroup_1", oReferencelist.additionalinfo.hierarchyconfig.p1);
                        param.Add("productgroup_2", oReferencelist.additionalinfo.hierarchyconfig.p2);
                        param.Add("productgroup_3", oReferencelist.additionalinfo.hierarchyconfig.p3);
                        param.Add("productgroup_4", oReferencelist.additionalinfo.hierarchyconfig.p4);
                        param.Add("productgroup_5", oReferencelist.additionalinfo.hierarchyconfig.p5);
                        ExecuteQuery(query, param, conn, transaction);
                    }
                    transaction.Commit();

                    if (version == oReferencelist.version)
                        throw new Exception("Updation not complete, please refresh and try again.");

                    if (oReferencelist.id > 0 && !isMigration)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oReferencelist.id, type = "CACHE", subtype = "REFERENCELIST-" + oReferencelist.type, actionon = oReferencelist.modifiedon }, conn);
                    }

                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oReferencelist;
        }

        internal Referencelist SaveIncentive(ReferenceSectionConfig item)
        {
            if (item.productwise)
            {
                ProductsDataService productsDataService = new ProductsDataService();
                var products = productsDataService.Search(new ProductsCreteria()
                {
                    salescounterid = item.id
                });
                var referencelist = Search(new ReferencelistCreteria()
                {
                    type = "INCENTIVE-PRODUCT"
                });
                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        foreach (var product in products)
                        {
                            var newitem = item.Clone();
                            newitem.type = "INCENTIVE-PRODUCT";
                            newitem.id = 0;
                            newitem.version = 1;
                            newitem.referenceid = product.id;
                            newitem.name = product.name;
                            newitem.code = product.code;
                            newitem.locationid = item.locationid;
                            newitem.createdon = DateTimeNow;
                            newitem.createdby = Context.UserID;
                            newitem.modifiedby = Context.UserID;
                            newitem.modifiedon = DateTimeNow;
                            if (referencelist.Exists(e => e.referenceid == product.id))
                            {
                                newitem.id = referencelist.Find(e => e.referenceid == product.id).id;
                                newitem.version = referencelist.Find(e => e.referenceid == product.id).version;
                                newitem.createdby = referencelist.Find(e => e.referenceid == product.id).createdby;
                                newitem.createdon = referencelist.Find(e => e.referenceid == product.id).createdon;
                                Update(transaction, conn, newitem);
                            }
                            else
                            {
                                Insert(transaction, conn, newitem);
                            }
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                }
            }
            else
            {
                var referencelist = Search(new ReferencelistCreteria()
                {
                    type = "INCENTIVE-SECTION",
                    referenceid = item.id
                });
                var newitem = item.Clone();
                newitem.type = "INCENTIVE-SECTION";
                newitem.id = 0;
                newitem.referenceid = item.id;
                newitem.createdon = DateTimeNow;
                newitem.createdby = Context.UserID;
                newitem.modifiedby = Context.UserID;
                newitem.modifiedon = DateTimeNow;
                newitem.locationid = item.locationid;
                if (referencelist.Count > 0)
                {
                    newitem.id = referencelist[0].id;
                    newitem.version = referencelist[0].version;
                    newitem.createdby = referencelist[0].createdby;
                    newitem.createdon = referencelist[0].createdon;
                    Update(newitem);
                }
                else
                {
                    Insert(newitem);
                }
            }
            return item;
        }



        public Referencelist Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Referencelist oReferencelist)
        {
            oReferencelist.modifiedon = DateTimeNow;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReferencelist.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReferencelist.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReferencelist.modifiedon;
            command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.type == null ? "" : oReferencelist.type;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.code == null ? "" : oReferencelist.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.name == null ? "" : oReferencelist.name;
            command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oReferencelist.additionalinfo_json == null ? "null" : oReferencelist.additionalinfo_json;
            command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.sortingindex;
            command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.locationid;
            command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.productgroup;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.referenceid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oReferencelist.version = Convert.ToInt32(reader[0]);
                }
            }
            return oReferencelist;
        }

        public Referencelist UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Referencelist oReferencelist)
        {
            oReferencelist.modifiedon = DateTimeNow;
            NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateStatus, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReferencelist.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oReferencelist.modifiedon;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oReferencelist.version = Convert.ToInt32(reader[0]);
                }
            }
            return oReferencelist;
        }

        internal List<CustomReferenceItem> Search(CustomReferenceItem _referencelist)
        {
            List<CustomReferenceItem> lst = new List<CustomReferenceItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select id, code, name, additionalinfo,version,type from referencelist where isactive=true";
                if (_referencelist.type.IndexOf(",") > 0)
                    qb.AddInParamer("type", string.Join(",", new List<string>(_referencelist.type.Split(',')).ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));
                else
                    qb.AddParameter("type", _referencelist.type, "=");
                if (_referencelist.productgroup > 0)
                    qb.AddParameter("productgroup", _referencelist.productgroup, " = ");
                qb.SortField = "sortingindex,name";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomReferenceItem oreferencelist = new CustomReferenceItem();
                        oreferencelist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oreferencelist.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oreferencelist.type = reader["type"] == DBNull.Value ? "" : reader["type"].ToString();
                        oreferencelist.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oreferencelist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oreferencelist.data = reader["additionalinfo"] == DBNull.Value ? "" : reader["additionalinfo"].ToString();
                        lst.Add(oreferencelist);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        internal CustomReferenceItem Update(CustomReferenceItem oReferencelist)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.id;
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
                    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.type;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.productgroup;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.code == null ? "" : oReferencelist.code;
                    command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.name == null ? "" : oReferencelist.name;
                    command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oReferencelist.data == null ? "null" : oReferencelist.data;
                    command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = 1;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.version;
                    command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.locationid;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.productgroup;
                    command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oReferencelist.version = Convert.ToInt32(reader[0]);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return oReferencelist;
        }

        internal CustomReferenceItem Insert(CustomReferenceItem oReferencelist)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    var isactive = (oReferencelist.type == "SIZE" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_size) ||
                       (oReferencelist.type == "STYLE" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_style) ||
                       (oReferencelist.type == "PATTERN" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_pattern) ||
                       (oReferencelist.type == "COLOUR" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_color) ||
                       (oReferencelist.type == "MATERIAL" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_material) ||
                       (oReferencelist.type == "TYPE" && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_type);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = !isactive;
                    command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = 1;
                    command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.productgroup;
                    command.Parameters.Add("type", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.type;
                    command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.code == null ? "" : oReferencelist.code;
                    command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oReferencelist.name == null ? "" : oReferencelist.name;
                    command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oReferencelist.data == null ? "null" : oReferencelist.data;
                    command.Parameters.Add("sortingindex", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                    command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("referencelocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.locationid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oReferencelist.id = Convert.ToInt64(reader[0].ToString());
                        }
                    }
                    if (oReferencelist.id > 0)
                    {
                        InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oReferencelist.id, type = "CACHE", subtype = "REFERENCELIST-" + oReferencelist.type, actionon = DateTimeNow }, conn);
                    }

                    if (isactive)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Attributes,
                            oReferencelist.id, string.Format("PRODUCT ATTRIBUTE - NEW OF TYPE {0} IS {1}", oReferencelist.type,
                                oReferencelist.name), Context.UserID, 0, transaction, Context);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return oReferencelist;
        }

        public bool Delete(Referencelist oReferencelist)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            hasSuccessCount = Delete(transaction, conn, oReferencelist);
                            transaction.Commit();
                            if (oReferencelist.id > 0)
                            {
                                InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config() { id = oReferencelist.id, type = "CACHE", subtype = "REFERENCELIST-" + oReferencelist.type, actionon = oReferencelist.modifiedon }, conn);
                            }
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Referencelist oReferencelist)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oReferencelist.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oReferencelist.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oReferencelist.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal bool PreapreCustomField(Referencelist item)
        {
            item = Search(new ReferencelistCreteria()
            {
                id = item.id
            }).First();
            if (item.additionalinfo.customfield != null &&
                item.additionalinfo.customfield.cfbuilt)
            {
                throw new Exception("This field already genarted");
            }
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            if (!rg.IsMatch(item.code))
            {
                throw new Exception("Code allowed only alpha numerals no special characters");
            }
            item.additionalinfo.customfield = new Referencelist.CustomField();
            item.additionalinfo.customfield.cfname = "cf_" + item.code.ToLower();
            switch (item.referenceid)
            {
                case 1:
                    item.additionalinfo.customfield.cftype = "varchar(" + item.additionalinfo.maxlimit.ToString("F0") + ")";
                    break;
                case 2:
                    var numericparts = item.additionalinfo.maxlimit.ToString("F2").Replace(".", ",");
                    item.additionalinfo.customfield.cftype = "numeric(" + numericparts + ")";
                    break;
                case 3:
                    item.additionalinfo.customfield.cftype = "timestamp without time zone";
                    break;
                case 4:
                    item.additionalinfo.customfield.cfref = item.code;
                    item.additionalinfo.customfield.cftype = "bigint";
                    break;
                default:
                    item.additionalinfo.customfield.cftype = "bigint";
                    break;
            }
            
            string addinvcolumn = "alter table lrinvoiceitems add " +
                item.additionalinfo.customfield.cfname + "  " + 
                item.additionalinfo.customfield.cftype;
            string additemcolumn = "alter table items add " +
                "itm_"+item.additionalinfo.customfield.cfname + "  " +
                item.additionalinfo.customfield.cftype;
            string addmfgcolumn = "alter table mfgdesign add " +
                "mfg_" + item.additionalinfo.customfield.cfname + "  " +
                item.additionalinfo.customfield.cftype;

            using (var connection = AdminConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                item.additionalinfo.customfield.cfbuilt = true;
                Update(transaction, connection, item);
                ExecuteQuery(addinvcolumn, null, connection, transaction);
                ExecuteQuery(additemcolumn, null, connection, transaction);
                ExecuteQuery(addmfgcolumn, null, connection, transaction);
                transaction.Commit();
                connection.Close(); 
            }
            return true;
        }

        internal Referencelist ChangeProductHierarchy(Referencelist item)
        {
            if (item.type == "Add2ProductHierarchy")
            {
                var productgroup = Search(new ReferencelistCreteria()
                {
                    id = item.id
                });
                if (productgroup.Count == 1 && productgroup[0].type == "PRODUCTGROUPS")
                {
                    productgroup[0].referenceid = item.referenceid;
                    if (productgroup[0].additionalinfo == null)
                        productgroup[0].additionalinfo = new Referencelist.Attribute();
                    productgroup[0].additionalinfo.hierarchyconfig = item.additionalinfo.hierarchyconfig;
                    productgroup[0].modifiedby = Context.UserID;
                    productgroup[0].modifiedon = DateTimeNow;
                    return Update(productgroup[0]);
                }
            }
            if (item.type == "EditProductHierarchy")
            {
                var productgroup = Search(new ReferencelistCreteria()
                {
                    id = item.id
                });
                if (productgroup.Count == 1 && productgroup[0].type == "PRODUCTHIERARCHY")
                {
                    productgroup[0].name = item.name;
                    productgroup[0].code = item.code;
                    productgroup[0].modifiedby = Context.UserID;
                    productgroup[0].modifiedon = DateTimeNow;
                    return Update(productgroup[0]);
                }
            }
            if (item.type == "DeleteProductHierarchy")
            {
                var productgroup = Search(new ReferencelistCreteria()
                {
                    id = item.id
                });
                if (productgroup.Count == 1 && productgroup[0].type == "PRODUCTHIERARCHY")
                {
                    Delete(productgroup[0]);
                    return productgroup[0];
                }
            }
            throw new Exception("Can't find the product group in the system");
        }

        internal Referencelist ConvertLocationToVendors(Referencelist item)
        {

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = item.additionalinfo.parentid
            }).FirstOrDefault();

            if (string.IsNullOrEmpty(company.tinanddate))
                throw new Exception("Please enter the correct gst number for the selected company");

            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            var possiblecustomer = customerDataService.Search(new CustomerCreteria()
            {
                name = item.name,
            });
            Customer customer = null;
            if(possiblecustomer.Count>0)
            {
                if(possiblecustomer.Exists(x => x.billingname == company.billingname && x.gstno == company.tinanddate))
                {
                    customer = possiblecustomer.Find(x => x.billingname == company.billingname && x.gstno == company.tinanddate);
                }
                else
                {
                    throw new Exception("Customer Record already avilable in the name of " + item.name +
                        @" but companyname and gst number not matching 
                           Change the existing name or change the gst number and billing name to match");
                }
            }

            SupplierDataService suppliercompanydataservive = new SupplierDataService() { Context = this.Context };
            var possiblesupplier = suppliercompanydataservive.Search(new SupplierCreteria()
            {
                name = item.name,
            });
            Supplier suppliercompany = null;
            if (possiblesupplier.Count > 0)
            {
                if (possiblesupplier.Exists(x => x.companyname == company.billingname && x.gstno == company.tinanddate))
                {
                    suppliercompany = possiblesupplier.Find(x => x.companyname == company.billingname && x.gstno == company.tinanddate);
                }
                else
                {
                    throw new Exception("Supplier Record already avilable in the name of " + item.name +
                        @" but companyname and gst number not matching 
                           Change the existing name or change the gst number and company name to match");
                }
            }
            if(customer == null)
            {
                customer = new Customer();
                customer.name = item.name;
                customer.code = "LOC-" + item.code + "-CUST";
                customer.billingname = company.billingname;
                customer.gstno = company.tinanddate;
                customer.isactive = true;
                customer.createdby = Context.UserID;
                customer.modifiedby = Context.UserID;
                customer.createdon = Context.DateTimeNow;
                customer.modifiedon = Context.DateTimeNow;
                customer.version = 1;
                customer.address = item.additionalinfo.address;
                customer.cityid = item.additionalinfo.locationconfig.cityid;
                customer.stateid = item.additionalinfo.locationconfig.stateid;
                customer.pincode = item.additionalinfo.locationconfig.pincode;
                customerDataService.Insert(customer);
            }

            if (suppliercompany == null)
            {
                suppliercompany = new Supplier();
                suppliercompany.name = item.name;
                suppliercompany.code = "LOC-" + item.code + "-SUPP";
                suppliercompany.companylist = new List<Suppliercompany>();
                suppliercompany.companylist.Add(new Suppliercompany());
                suppliercompany.companylist[0].name = company.billingname;
                suppliercompany.companylist[0].gst = company.tinanddate;
                suppliercompany.companylist[0].tinanddate = company.tinanddate;
                suppliercompany.gstno = company.tinanddate;
                suppliercompany.isactive = true;
                suppliercompany.createdby = Context.UserID;
                suppliercompany.modifiedby = Context.UserID;
                suppliercompany.createdon = Context.DateTimeNow;
                suppliercompany.modifiedon = Context.DateTimeNow;
                suppliercompany.version = 1;

                suppliercompany.address = item.additionalinfo.address;
                suppliercompany.cityid = item.additionalinfo.locationconfig.cityid;
                suppliercompany.pincode = item.additionalinfo.locationconfig.pincode;

                suppliercompany.companylist[0].address = item.additionalinfo.address;
                suppliercompany.companylist[0].cityid = item.additionalinfo.locationconfig.cityid;
                suppliercompany.companylist[0].stateid = item.additionalinfo.locationconfig.stateid;
                suppliercompany.companylist[0].pincode = item.additionalinfo.locationconfig.pincode;
                suppliercompanydataservive.Insert(suppliercompany);
            }

            item.additionalinfo.supplierid = suppliercompany.id;
            item.additionalinfo.customerid = customer.id;
            Update(item);
            return item;
        }
    }

}
