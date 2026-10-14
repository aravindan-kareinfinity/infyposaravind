using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;

using System.Data;

namespace InfyPOS.Data

{
    public class SupplierproductgroupsDataService : BaseData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, productgroupid, 
    supplierid, attributelist, priceagreements,itemcode,cur_seq,code from supplierproductgroups where id=@id";

        const string SQL_DailySearch = @"select supplierproductgroups.id, supplierproductgroups.isactive, 
supplierproductgroups.createdby, 
supplierproductgroups.modifiedby, supplierproductgroups.version, supplierproductgroups.createdon, 
supplierproductgroups.modifiedon, supplierproductgroups.productgroupid,supplierproductgroups.supplierid, 
supplierproductgroups.attributelist, supplierproductgroups.priceagreements,supplierproductgroups.itemcode,
supplierproductgroups.cur_seq,supplierproductgroups.code from supplierproductgroups
inner join products on products.id = supplierproductgroups.productgroupid
where products.isdailyprice = true 
and supplierproductgroups.isactive=true and products.isactive=true
and supplierproductgroups.supplierid=@supplierid and products.basepriceid=@basepriceid";

        const string SQL_Insert = @"INSERT INTO public.supplierproductgroups(isactive, createdby, modifiedby, version, createdon, modifiedon, productgroupid, supplierid, attributelist, priceagreements,itemcode,cur_seq, code) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @productgroupid, @supplierid, @attributelist, @priceagreements,@itemcode,@cur_seq, @code) RETURNING id";
        
        const string SQL_Update = @"UPDATE public.supplierproductgroups SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, productgroupid=@productgroupid, supplierid=@supplierid, 
        attributelist=@attributelist,priceagreements=@priceagreements,itemcode=@itemcode,cur_seq=@cur_seq,code=@code where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.supplierproductgroups set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Supplierproductgroups> Search(SupplierproductgroupsCreteria _supplierproductgroups)
        {
            List<Supplierproductgroups> lst = new List<Supplierproductgroups>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_supplierproductgroups.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierproductgroups.id;
                }
                else if(_supplierproductgroups.dailyproducts)
                {
                    command = new NpgsqlCommand(SQL_DailySearch, conn);
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierproductgroups.supplierid;
                    command.Parameters.Add("basepriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierproductgroups.dailypricegroupid;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_supplierproductgroups.supplierid >0)
                        qb.AddParameter("supplierid", _supplierproductgroups.supplierid,"=");
                    if (_supplierproductgroups.productgroupid > 0)
                        qb.AddParameter("productgroupid", _supplierproductgroups.productgroupid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Supplierproductgroups osupplierproductgroups = new Supplierproductgroups();
                        osupplierproductgroups.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osupplierproductgroups.cur_seq = reader["cur_seq"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cur_seq"]);
                        osupplierproductgroups.itemcode = reader["itemcode"] == DBNull.Value ? "" : reader["itemcode"].ToString();
                        osupplierproductgroups.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osupplierproductgroups.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osupplierproductgroups.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osupplierproductgroups.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        osupplierproductgroups.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osupplierproductgroups.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osupplierproductgroups.productgroupid = reader["productgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroupid"]);
                        osupplierproductgroups.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        osupplierproductgroups.attributelist_json = reader["attributelist"] == DBNull.Value ? null : reader["attributelist"].ToString();
                        osupplierproductgroups.priceagreements_json = reader["priceagreements"] == DBNull.Value ? null : reader["priceagreements"].ToString();
                        osupplierproductgroups.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        lst.Add(osupplierproductgroups);
                    }
                }

                
                if(_supplierproductgroups.dailyproducts || _supplierproductgroups.includeitems)
                {
                    SuppliertradeagreementDataService suppliertradeagreementDataService = new SuppliertradeagreementDataService() { Context = this.Context };
                    var agreements = suppliertradeagreementDataService.Search(new SuppliertradeagreementCreteria()
                    {
                        Supplierproductgroupsids = lst.ConvertAll(x=>x.id)
                    });

                    lst.ForEach(x =>
                    {
                        x.tradeagreements = agreements.FindAll(y => y.Supplierproductgroupsid == x.id);
                    });
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierproductgroups);
            }
            return lst;
        }

        public Supplierproductgroups Insert(Supplierproductgroups oSupplierproductgroups)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("supplierproductgroups.code",0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oSupplierproductgroups.code = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oSupplierproductgroups);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierproductgroups);
            }

            return oSupplierproductgroups;
        }

        public Supplierproductgroups Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierproductgroups oSupplierproductgroups)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierproductgroups.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierproductgroups.version;
            command.Parameters.Add("cur_seq", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierproductgroups.cur_seq;
            command.Parameters.Add("itemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierproductgroups.itemcode == null ? "" : oSupplierproductgroups.itemcode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierproductgroups.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierproductgroups.modifiedon;
            command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.productgroupid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.supplierid;
            command.Parameters.Add("attributelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplierproductgroups.attributelist_json == null ? "null" : oSupplierproductgroups.attributelist_json;
            command.Parameters.Add("priceagreements", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplierproductgroups.priceagreements_json == null ? "null" : oSupplierproductgroups.priceagreements_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierproductgroups.code == null ? "" : oSupplierproductgroups.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplierproductgroups.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSupplierproductgroups;
        }

        public Supplierproductgroups Update(Supplierproductgroups oSupplierproductgroups)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSupplierproductgroups);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierproductgroups);
            }

            return oSupplierproductgroups;
        }

        public Supplierproductgroups Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierproductgroups oSupplierproductgroups)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.id;
            command.Parameters.Add("cur_seq", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierproductgroups.cur_seq;
            command.Parameters.Add("itemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierproductgroups.itemcode == null ? "" : oSupplierproductgroups.itemcode;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierproductgroups.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierproductgroups.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierproductgroups.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSupplierproductgroups.modifiedon;
            command.Parameters.Add("productgroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.productgroupid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.supplierid;
            command.Parameters.Add("attributelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplierproductgroups.attributelist_json == null ? "null" : oSupplierproductgroups.attributelist_json;
            command.Parameters.Add("priceagreements", NpgsqlTypes.NpgsqlDbType.Json).Value = oSupplierproductgroups.priceagreements_json == null ? "null" : oSupplierproductgroups.priceagreements_json;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSupplierproductgroups.code == null ? "" : oSupplierproductgroups.code;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSupplierproductgroups.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSupplierproductgroups;
        }


        public bool Delete(Supplierproductgroups oSupplierproductgroups)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSupplierproductgroups);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSupplierproductgroups);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Supplierproductgroups oSupplierproductgroups)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSupplierproductgroups.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oSupplierproductgroups.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSupplierproductgroups.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSupplierproductgroups.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Tuple<int,string> GetNextItemCode(long supplierproductgroupsid, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            var command = new Npgsql.NpgsqlCommand(@"UPDATE Supplierproductgroups a SET cur_seq=cur_seq+1
                                                where id=@id
                                                RETURNING a.cur_seq,a.itemcode", conn, trans);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = supplierproductgroupsid;
            using (var dr = command.ExecuteReader(CommandBehavior.SingleRow))
            {
                if (dr.Read())
                {
                    var cur_seq = dr.GetInt32(0);
                    var itemcode = dr.GetString(1);
                    return new Tuple<int, string>(cur_seq, itemcode);
                }
            }
            return new Tuple<int, string>(0,"");
        }
    }

}
