using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class IncentiveconfigDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, salablegoodsid, incentive, locationid,
        barcode,percentage from incentiveconfig where id=@id";

        const string SQL_Insert = @"
        DELETE from public.incentiveconfig where salablegoodsid=@salablegoodsid;
        INSERT INTO public.incentiveconfig(isactive, createdby, modifiedby, version, createdon, modifiedon, salablegoodsid, incentive, locationid, barcode,percentage) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @salablegoodsid, @incentive, @locationid, @barcode,@percentage) RETURNING id";

        const string SQL_Update = @"UPDATE public.incentiveconfig SET modifiedby=@modifiedby, version=@version+1, 
        modifiedon=@modifiedon, incentive=@incentive,percentage=@percentage where id=@id RETURNING version";

        const string SQL_Delete = @"update public.incentiveconfig set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";

        public List<Incentiveconfig> Search(IncentiveconfigCreteria _incentiveconfig)
        {
            List<Incentiveconfig> lst = new List<Incentiveconfig>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_incentiveconfig.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _incentiveconfig.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_incentiveconfig.salablegoodsid > 0)
                        qb.AddParameter("salablegoodsid", _incentiveconfig.salablegoodsid, "=");
                    else if (!string.IsNullOrEmpty(_incentiveconfig.barcode))
                    {
                        qb.AddParameter("barcode", _incentiveconfig.barcode, "=");
                        qb.AddParameter("locationid", _incentiveconfig.locationid, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Incentiveconfig oincentiveconfig = new Incentiveconfig();
                        oincentiveconfig.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oincentiveconfig.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oincentiveconfig.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oincentiveconfig.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oincentiveconfig.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oincentiveconfig.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oincentiveconfig.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oincentiveconfig.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        oincentiveconfig.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        oincentiveconfig.incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        oincentiveconfig.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oincentiveconfig.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        lst.Add(oincentiveconfig);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _incentiveconfig);
            }
            return lst;
        }

        public bool DeleteAll(IncentiveconfigCreteria oIncentiveconfig)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (var item in oIncentiveconfig.ids)
                    {
                        Delete(transaction, conn, new Incentiveconfig()
                        {
                            id = item
                        });
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentiveconfig);
            }
            return hasSuccessCount;

        }

        internal IncentiveUpdate SaveAll(IncentiveUpdate item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var incentiveitem in item.items)
                    {
                        Incentiveconfig iconfig = new Incentiveconfig();
                        if (incentiveitem.ispercentage)
                        {
                            iconfig.incentive = 0;
                            iconfig.percentage = incentiveitem.incentive;
                        }
                        else
                        {
                            iconfig.percentage = 0;
                            iconfig.incentive = incentiveitem.incentive;
                        }
                        iconfig.version = incentiveitem.version;
                        iconfig.id = incentiveitem.id;
                        if (incentiveitem.id > 0)
                        {
                            Update(transaction, conn, iconfig);
                        }
                        else
                        {
                            iconfig.barcode = incentiveitem.barcode;
                            iconfig.createdby = Context.UserID;
                            iconfig.createdon = DateTimeNow;
                            iconfig.isactive = true;
                            iconfig.locationid = item.locationid;
                            iconfig.modifiedby = Context.UserID;
                            iconfig.modifiedon = DateTimeNow;
                            iconfig.salablegoodsid = incentiveitem.salablegoodsid;
                            iconfig.version = 1;
                            Insert(transaction, conn, iconfig);
                        }
                        incentiveitem.id = iconfig.id;
                        incentiveitem.incentive = iconfig.incentive;
                    }
                    transaction.Commit();
                    conn.Close();
                    item.success = true;
                }
            }
            catch (Exception exp)
            {
                item.success = false;
                item.exception = exp.Message;
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }

            return item;
        }

        public List<IncentiveItem> SearchBarcode(IncentiveconfigCreteria incentiveconfig)
        {
            string baseQuery = @"with sectionconfig 
            as 
            (
	            select referencelist.id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where type = 'DIVISION' and cast(schemelist ->> 'to' as numeric) is not null 
            )";

            string indirectSection = @"with sectionconfig 
            as 
            (
	            select referencelist.referenceid as id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where type = 'INCENTIVE-SECTION' and cast(schemelist ->> 'to' as numeric) is not null 
            )";

            string indirectProduct = @"with sectionconfig 
            as 
            (
	            select referencelist.referenceid as id,code,cast(schemelist ->> 'to' as numeric) as to,cast(schemelist ->> 'from' as numeric) as from
	            ,cast(schemelist ->> 'incentive' as numeric) as incentive from referencelist
	            LEFT JOIN LATERAL
	            json_array_elements (cast(additionalinfo::json->>'salesincentive' as json)) schemelist ON true
	            where type = 'INCENTIVE-PRODUCT' and cast(schemelist ->> 'to' as numeric) is not null 
            )";

            int incentivemode = 0;
            if (string.IsNullOrEmpty(incentiveconfig.type) && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(incentiveconfig.locationid))
            {
                incentivemode = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[incentiveconfig.locationid].incentivemode;
            }
            if (incentivemode == 2 || incentiveconfig.type == "INCENTIVE-SECTION")
            {
                baseQuery = indirectSection;
                incentivemode = 2;
            }
            else if (incentivemode == 3 || incentiveconfig.type == "INCENTIVE-PRODUCT")
            {
                baseQuery = indirectProduct;
                incentivemode = 3;
            }
            else
                incentiveconfig.type = "";

            string SQL_Search = baseQuery + @"
            select salablegoods.barcode,salablegoods.id as salablegoodsid,referencelist.name as sectionname,products.name,incentiveconfig.incentive,incentiveconfig.percentage,
            sectionconfig.incentive as sectionincentive,(salablegoods.price-salablegoods.discount)::numeric as price,sectionconfig.from,sectionconfig.to,incentiveconfig.id,incentiveconfig.version from salablegoods 
            inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
            inner join products on products.id = stock.productid
            left outer join referencelist on referencelist.id = products.salescounterid
			left outer join sectionconfig on sectionconfig.id = products.salescounterid and (salablegoods.price-salablegoods.discount)::numeric between sectionconfig.from and sectionconfig.to
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id and incentiveconfig.isactive = true";
            if (incentivemode < 2 && string.IsNullOrEmpty(incentiveconfig.type))
            {
                SQL_Search = @"select salablegoods.barcode,salablegoods.id as salablegoodsid,referencelist.name as sectionname,products.name,incentiveconfig.incentive,incentiveconfig.percentage,
                (salablegoods.price-salablegoods.discount)::numeric as price,incentiveconfig.id,incentiveconfig.version from salablegoods 
                inner join stock on stock.id = salablegoods.stockid and stock.isactive = true
                inner join products on products.id = stock.productid
                left outer join referencelist on referencelist.id = products.salescounterid
                left outer join incentiveconfig on incentiveconfig.salablegoodsid = salablegoods.id and incentiveconfig.isactive = true";
            }
            SQL_Search += " where salablegoods.locationid = @locationid and " +
                         ((incentiveconfig.barcodes != null && incentiveconfig.barcodes.Count > 0) ? "salablegoods.barcode in (" + string.Join(",", incentiveconfig.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")" : "salablegoods.barcode = @barcode");


            if (string.IsNullOrEmpty(incentiveconfig.barcode) && (incentiveconfig.barcodes == null || incentiveconfig.barcodes.Count == 0))
            {
                SQL_Search = SQL_Search.Replace("and salablegoods.barcode = @barcode", "");
                SQL_Search = SQL_Search.Replace("left outer join incentiveconfig", "inner join incentiveconfig");
            }

            List<IncentiveItem> lst = new List<IncentiveItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_Search, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = incentiveconfig.locationid;
                command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(incentiveconfig.barcode) ? "" : incentiveconfig.barcode;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        IncentiveItem oincentiveconfig = new IncentiveItem();
                        oincentiveconfig.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oincentiveconfig.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        if (incentivemode >= 2)
                        {
                            oincentiveconfig.sectionincentive = reader["sectionincentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sectionincentive"]);
                            oincentiveconfig.from = reader["from"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["from"]);
                            oincentiveconfig.to = reader["to"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["to"]);
                        }
                        oincentiveconfig.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        oincentiveconfig.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oincentiveconfig.productname = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oincentiveconfig.incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        var percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                        if(percentage>0 && oincentiveconfig.incentive == 0)
                        {
                            oincentiveconfig.incentive = percentage;
                            oincentiveconfig.ispercentage = true;
                        }
                        oincentiveconfig.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oincentiveconfig.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        lst.Add(oincentiveconfig);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, incentiveconfig);
            }
            return lst;
        }

        public Incentiveconfig Insert(Incentiveconfig oIncentiveconfig)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oIncentiveconfig);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentiveconfig);
            }

            return oIncentiveconfig;
        }

        public Incentiveconfig Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Incentiveconfig oIncentiveconfig)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIncentiveconfig.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentiveconfig.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIncentiveconfig.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIncentiveconfig.modifiedon;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.salablegoodsid;
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oIncentiveconfig.incentive;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oIncentiveconfig.percentage;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.locationid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oIncentiveconfig.barcode == null ? "" : oIncentiveconfig.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oIncentiveconfig.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oIncentiveconfig;
        }

        public Incentiveconfig Update(Incentiveconfig oIncentiveconfig)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oIncentiveconfig);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentiveconfig);
            }

            return oIncentiveconfig;
        }

        public Incentiveconfig Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Incentiveconfig oIncentiveconfig)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIncentiveconfig.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentiveconfig.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIncentiveconfig.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIncentiveconfig.modifiedon;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.salablegoodsid;
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oIncentiveconfig.incentive;
            command.Parameters.Add("percentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oIncentiveconfig.percentage;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.locationid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oIncentiveconfig.barcode == null ? "" : oIncentiveconfig.barcode;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oIncentiveconfig.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oIncentiveconfig;
        }


        public bool Delete(Incentiveconfig oIncentiveconfig)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oIncentiveconfig);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentiveconfig);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Incentiveconfig oIncentiveconfig)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = 1;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentiveconfig.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }



    }

}
