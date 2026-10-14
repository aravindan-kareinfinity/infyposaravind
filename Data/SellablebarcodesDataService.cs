using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class SellablebarcodesDataService : SAASData
    {
        const string SQL_Search = @"select modifiedby, createdby, isactive, companyid, itemid, productid, barcode, batchandexpiry, stockid, id, modifiedon, createdon, version,corebarcode from sellablebarcodes where id=@id";


        const string SQL_Insert = @"INSERT INTO public.sellablebarcodes(modifiedby, createdby, isactive, companyid, itemid, productid, barcode, batchandexpiry, stockid, modifiedon, createdon, version,corebarcode) VALUES(@modifiedby, @createdby, @isactive, @companyid, @itemid, @productid, @barcode, @batchandexpiry, @stockid, @modifiedon, @createdon, @version,@corebarcode) RETURNING id";


        const string SQL_Update = @"UPDATE public.sellablebarcodes SET modifiedby=@modifiedby, companyid=@companyid, itemid=@itemid, productid=@productid, barcode=@barcode, batchandexpiry=@batchandexpiry, stockid=@stockid, modifiedon=@modifiedon, version=@version+1,corebarcode=@corebarcode where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.sellablebarcodes set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        public bool ismigration { get; set; }
        public List<Sellablebarcodes> Search(SellablebarcodesCreteria _sellablebarcodes)
        {
            List<Sellablebarcodes> lst = new List<Sellablebarcodes>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_sellablebarcodes.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _sellablebarcodes.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_sellablebarcodes.barcode))
                        qb.AddParameter("barcode", _sellablebarcodes.barcode, "=");

                    if (!string.IsNullOrEmpty(_sellablebarcodes.corebarcode))
                        qb.AddParameter("corebarcode", _sellablebarcodes.corebarcode, "=");

                    if (_sellablebarcodes.ids != null && _sellablebarcodes.ids.Count > 0)
                        qb.AddInParamer("barcode", string.Join<string>(",", _sellablebarcodes.ids.ConvertAll(e=>"'"+ WebAPI.Data.Utility.SafeSQL(e) + "'")));

                    if (_sellablebarcodes.limit > 0)
                    {
                        qb.SortField = "sellablebarcodes.id desc";
                        qb.Limit = _sellablebarcodes.limit;
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Sellablebarcodes osellablebarcodes = new Sellablebarcodes();
                        osellablebarcodes.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        osellablebarcodes.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        osellablebarcodes.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        osellablebarcodes.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        osellablebarcodes.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        osellablebarcodes.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        osellablebarcodes.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        osellablebarcodes.corebarcode = reader["corebarcode"] == DBNull.Value ? "" : reader["corebarcode"].ToString();
                        osellablebarcodes.batchandexpiry = reader["batchandexpiry"] == DBNull.Value ? "" : reader["batchandexpiry"].ToString();
                        osellablebarcodes.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        osellablebarcodes.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        osellablebarcodes.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        osellablebarcodes.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        osellablebarcodes.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        lst.Add(osellablebarcodes);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _sellablebarcodes);
            }
            return lst;
        }

        public Sellablebarcodes Insert(Sellablebarcodes oSellablebarcodes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (!ismigration &&
                   !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_secondary_barcodes &&
                   !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_quanto_barcodes &&
                   !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) 
                    return oSellablebarcodes;


                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSellablebarcodes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSellablebarcodes);
                throw exp;
            }

            return oSellablebarcodes;
        }

        public Sellablebarcodes Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Sellablebarcodes oSellablebarcodes)
        {
            if (!ismigration &&
               !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_secondary_barcodes &&
               !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_quanto_barcodes &&
               !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                return oSellablebarcodes;

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSellablebarcodes.isactive;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.companyid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.productid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSellablebarcodes.barcode == null ? "" : oSellablebarcodes.barcode;
            command.Parameters.Add("corebarcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSellablebarcodes.corebarcode == null ? "" : oSellablebarcodes.corebarcode;
            command.Parameters.Add("batchandexpiry", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSellablebarcodes.batchandexpiry == null ? "" : oSellablebarcodes.batchandexpiry;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.stockid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSellablebarcodes.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSellablebarcodes.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSellablebarcodes.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSellablebarcodes.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSellablebarcodes;
        }

        public Sellablebarcodes Update(Sellablebarcodes oSellablebarcodes)
        {
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_secondary_barcodes) return oSellablebarcodes;
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oSellablebarcodes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSellablebarcodes);
                throw exp;
            }

            return oSellablebarcodes;
        }

        public Sellablebarcodes Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Sellablebarcodes oSellablebarcodes)
        {
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_secondary_barcodes) return oSellablebarcodes;
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSellablebarcodes.isactive;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.companyid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.itemid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.productid;
            command.Parameters.Add("corebarcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSellablebarcodes.corebarcode == null ? "" : oSellablebarcodes.corebarcode;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSellablebarcodes.barcode == null ? "" : oSellablebarcodes.barcode;
            command.Parameters.Add("batchandexpiry", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSellablebarcodes.batchandexpiry == null ? "" : oSellablebarcodes.batchandexpiry;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.stockid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.id;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSellablebarcodes.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSellablebarcodes.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSellablebarcodes.version;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSellablebarcodes.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSellablebarcodes;
        }


        public bool Delete(Sellablebarcodes oSellablebarcodes)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSellablebarcodes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSellablebarcodes);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Sellablebarcodes oSellablebarcodes)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSellablebarcodes.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSellablebarcodes.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSellablebarcodes.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSellablebarcodes.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        private static System.Collections.Concurrent.ConcurrentDictionary<long,SellablebarcodesDataService> instance;
        private long organizationid;

        static SellablebarcodesDataService()
        {
            instance = new System.Collections.Concurrent.ConcurrentDictionary<long, SellablebarcodesDataService>();
        }

        public SellablebarcodesDataService(long organizationid)
        {
            this.organizationid = organizationid;
            this.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
        }

        public SellablebarcodesDataService()
        {
        }

        public static SellablebarcodesDataService Instance(long organizationid)
        {

            if (instance.ContainsKey(organizationid))
                return instance[organizationid];

            SellablebarcodesDataService sellablebarcodesData = new SellablebarcodesDataService(organizationid);
            instance.TryAdd(organizationid, sellablebarcodesData);
            return sellablebarcodesData;
        }
        public string GetCoreBarcode(string barcode)
        {
            var items = Search(new SellablebarcodesCreteria()
            {
                barcode = barcode,
                limit = 1
            });
            if (items.Count > 0)
                return items[0].corebarcode;
            return barcode;
        }

        internal List<string> GetCoreBarcodes(List<string> barecodes)
        {
            throw new NotImplementedException();
        }
    }

}
