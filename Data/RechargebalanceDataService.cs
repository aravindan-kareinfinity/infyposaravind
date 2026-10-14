using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class RechargebalanceDataService : SAASData
    {
        const string SQL_Search = @"select schemeid, rechargebalance.isactive, rechargebalance.createdby, rechargebalance.modifiedby, 
        rechargebalance.version, balance, rechargebalance.createdon, rechargebalance.modifiedon, rechargebalance.id, topup,discount, 
        transferin, transferout, recharged, cashback, rechargebalance.locationid , referencelist.name locationname,rechargescheme.name scheme
        from rechargebalance
        inner join referencelist on referencelist.id = locationid
        left outer join rechargescheme on rechargescheme.id = schemeid
        where id=@id";


        const string SQL_Insert = @"INSERT INTO public.rechargebalance(schemeid, isactive, createdby, modifiedby, version, balance, createdon, modifiedon, topup, transferin, transferout, recharged, cashback, locationid,discount) 
        VALUES(@schemeid, @isactive, @createdby, @modifiedby, @version, @balance, @createdon, @modifiedon, @topup, @transferin, @transferout, @recharged, @cashback, @locationid,@discount) RETURNING id";

        const string SQL_UpdateBalance = @"UPDATE public.rechargebalance SET balance=balance+@topup+@transferin-(@recharged+@transferout), 
        topup=topup+@topup, transferin=transferin+@transferin, transferout=transferout+@transferout, discount=discount+@discount,
        recharged=recharged+@recharged, cashback=cashback+@cashback where locationid=@locationid and schemeid=@schemeid RETURNING balance";

        const string SQL_Update = @"UPDATE public.rechargebalance SET schemeid=@schemeid, modifiedby=@modifiedby, version=@version+1, balance=@balance, modifiedon=@modifiedon, topup=@topup, transferin=@transferin, transferout=@transferout, 
        recharged=@recharged, cashback=@cashback, locationid=@locationid,discount=@discount where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.rechargebalance set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Rechargebalance> Search(RechargebalanceCreteria _rechargebalance)
        {
            List<Rechargebalance> lst = new List<Rechargebalance>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_rechargebalance.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _rechargebalance.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("Rechargebalance.isactive", true, "=");
                    if (_rechargebalance.locationid>0)
                    qb.AddParameter("Rechargebalance.locationid", _rechargebalance.locationid, "=");
                    if(_rechargebalance.schemeid>0)
                    qb.AddParameter("Rechargebalance.schemeid", _rechargebalance.schemeid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Rechargebalance orechargebalance = new Rechargebalance();
                        orechargebalance.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                        orechargebalance.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        orechargebalance.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        orechargebalance.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        orechargebalance.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        orechargebalance.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        orechargebalance.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        orechargebalance.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        orechargebalance.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        orechargebalance.topup = reader["topup"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["topup"]);
                        orechargebalance.transferin = reader["transferin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferin"]);
                        orechargebalance.transferout = reader["transferout"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferout"]);
                        orechargebalance.recharged = reader["recharged"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["recharged"]);
                        orechargebalance.cashback = reader["cashback"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cashback"]);
                        orechargebalance.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        orechargebalance.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        orechargebalance.scheme = reader["scheme"] == DBNull.Value ? "" : reader["scheme"].ToString();
                        orechargebalance.location = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        lst.Add(orechargebalance);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _rechargebalance);
            }
            return lst;
        }

        public Rechargebalance Insert(Rechargebalance oRechargebalance)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oRechargebalance);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRechargebalance);
                throw exp;
            }

            return oRechargebalance;
        }

        public Rechargebalance Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargebalance oRechargebalance)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.schemeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRechargebalance.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRechargebalance.version;
            command.Parameters.Add("balance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.balance;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargebalance.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargebalance.modifiedon;
            command.Parameters.Add("topup", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.topup;
            command.Parameters.Add("transferin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.transferin;
            command.Parameters.Add("transferout", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.transferout;
            command.Parameters.Add("recharged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.recharged;
            command.Parameters.Add("cashback", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.cashback;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.discount;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRechargebalance.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oRechargebalance;
        }

        public Rechargebalance Update(Rechargebalance oRechargebalance)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oRechargebalance);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRechargebalance);
                throw exp;
            }

            return oRechargebalance;
        }

        public decimal? UpdateBalance(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargebalance oRechargebalance)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateBalance, conn, transaction);
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.schemeid;
            command.Parameters.Add("topup", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.topup;
            command.Parameters.Add("transferin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.transferin;
            command.Parameters.Add("transferout", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.transferout;
            command.Parameters.Add("recharged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.recharged;
            command.Parameters.Add("cashback", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.cashback;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.locationid;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.discount;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return Convert.ToDecimal(reader[0]);
                }
            }

            return null;
        }

        public decimal UpdateBalance(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Recharge source)
        {
            decimal? balance = null;
            switch (source.mode)
            {
                case Recharge.TranModes.Topup:
                    balance = UpdateBalance(transaction, conn, new Rechargebalance()
                    {
                        locationid = source.locationid,
                        schemeid = source.schemeid,
                        topup = source.amount
                    });
                    if (balance == null)
                    {
                        Insert(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.locationid,
                            schemeid = source.schemeid,
                            isactive = true,
                            version = 1
                        });
                        balance = UpdateBalance(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.locationid,
                            schemeid = source.schemeid,
                            topup = source.amount
                        });
                    }
                    break;
                case Recharge.TranModes.Transfer:
                    balance = UpdateBalance(transaction, conn, new Rechargebalance()
                    {
                        locationid = source.tolocationid,
                        schemeid = source.schemeid,
                        transferin = source.amount
                    });
                    if (balance == null)
                    {
                        Insert(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.tolocationid,
                            schemeid = source.schemeid,
                            isactive = true,
                            version = 1
                        });
                        balance = UpdateBalance(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.tolocationid,
                            schemeid = source.schemeid,
                            transferin = source.amount
                        });

                        if (balance.Value < 0)
                        {
                            throw new Exception("Negative Balance " + source.amount.ToString("F2"));
                        }
                    }

                    balance = UpdateBalance(transaction, conn, new Rechargebalance()
                    {
                        locationid = source.fromlocationid,
                        schemeid = source.schemeid,
                        transferout = source.amount
                    });
                    if (balance == null)
                    {
                        Insert(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.fromlocationid,
                            schemeid = source.schemeid,
                            isactive = true,
                            version = 1
                        });
                        balance = UpdateBalance(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.fromlocationid,
                            schemeid = source.schemeid,
                            transferout = source.amount
                        });
                    }
                    break;
                case Recharge.TranModes.Recharge:
                    balance = UpdateBalance(transaction, conn, new Rechargebalance()
                    {
                        locationid = source.locationid,
                        schemeid = source.schemeid,
                        recharged = source.rate,
                        cashback = source.cashback,
                        discount = source.discount
                    });
                    if (balance == null)
                    {
                        Insert(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.locationid,
                            schemeid = source.schemeid,
                            isactive = true,
                            version = 1
                        });
                        balance = UpdateBalance(transaction, conn, new Rechargebalance()
                        {
                            locationid = source.locationid,
                            schemeid = source.schemeid,
                            recharged = source.rate,
                            cashback = source.cashback,
                            discount = source.discount
                        });
                    }
                    break;
            }
            return balance.Value;
        }
        public Rechargebalance Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargebalance oRechargebalance)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.schemeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRechargebalance.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRechargebalance.version;
            command.Parameters.Add("balance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.balance;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargebalance.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargebalance.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.id;
            command.Parameters.Add("topup", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.topup;
            command.Parameters.Add("transferin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.transferin;
            command.Parameters.Add("transferout", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.transferout;
            command.Parameters.Add("recharged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.recharged;
            command.Parameters.Add("cashback", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.cashback;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRechargebalance.discount;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRechargebalance.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oRechargebalance;
        }


        public bool Delete(Rechargebalance oRechargebalance)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oRechargebalance);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRechargebalance);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Rechargebalance oRechargebalance)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRechargebalance.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRechargebalance.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRechargebalance.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRechargebalance.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
