using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class Mfg_SwatchesDataService : BaseData
    {
        const string SQL_Search = @"select mfg_swatches.code, mfg_swatches.receivedon, mfg_swatches.entrydate, mfg_swatches.pono,
        mfg_swatches.receivedby, mfg_swatches.receivedmtr, mfg_swatches.invoiceno, mfg_swatches.invoicedate, mfg_swatches.receivedmtr,
        mfg_swatches.supplierid, mfg_swatches.isactive, mfg_swatches.companyid, mfg_swatches.designid,mfg_swatches.design,
        mfg_swatches.receivedpcs, mfg_swatches.version, mfg_swatches.modifiedon, mfg_swatches.createdon, mfg_swatches.createdby,
        mfg_swatches.modifiedby, mfg_swatches.id,products.name productname,brand.name brandname,mfg_swatches.requiredmtr from mfg_swatches 
		left outer join mfgdesign on mfgdesign.id = mfg_swatches.designid
		left outer join products on products.id = mfgdesign.productid
		left outer join brand on brand.id = mfgdesign.brandid
        where mfg_swatches.id=@id";


        const string SQL_Insert = @"INSERT INTO public.mfg_swatches(code, receivedon, entrydate, receivedby, receivedmtr, invoiceno, invoicedate, supplierid, isactive, companyid, designid,design,receivedpcs, version, modifiedon, 
        createdon, createdby, modifiedby,pono,requiredmtr) VALUES(@code, @receivedon, @entrydate, @receivedby, @receivedmtr, @invoiceno,
        @invoicedate, @supplierid, @isactive, @companyid, @designid,@design,@receivedpcs, @version, @modifiedon, @createdon,
        @createdby, @modifiedby,@pono,@requiredmtr) RETURNING id";


        const string SQL_Update = @"UPDATE public.mfg_swatches SET code=@code, receivedon=@receivedon, entrydate=@entrydate,pono=@pono,receivedpcs=@receivedpcs,requiredmtr=@requiredmtr,
        receivedby=@receivedby, receivedmtr=@receivedmtr, invoiceno=@invoiceno, invoicedate=@invoicedate, supplierid=@supplierid, companyid=@companyid, designid=@designid,design=@design,
        version=@version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfg_swatches set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfg_Swatches> Search(Mfg_SwatchesCreteria _mfg_swatches)
        {
            List<Mfg_Swatches> lst = new List<Mfg_Swatches>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfg_swatches.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfg_swatches.id;
                }
                else
                {

                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("mfg_swatches.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_mfg_swatches.code))
                        qb.AddParameter("mfg_swatches.code", _mfg_swatches.code, "=");

                    if (!string.IsNullOrEmpty(_mfg_swatches.design))
                        qb.AddParameter("mfg_swatches.design", _mfg_swatches.design, "=");

                    if (_mfg_swatches.productid>0)
                        qb.AddParameter("mfgdesign.productid", _mfg_swatches.productid, "=");

                    if (_mfg_swatches.supplierid > 0)
                        qb.AddParameter("mfg_swatches.supplierid", _mfg_swatches.supplierid, "=");
                    if (_mfg_swatches.entryfromdate.Date != DateTime.MinValue)
                    {
                        qb.AddParameter("mfg_swatches.entrydate", "entryfromdate", _mfg_swatches.entryfromdate.Date, ">=");
                        qb.AddParameter("mfg_swatches.entrydate", "entrytodate", _mfg_swatches.entrytodate.Date.AddDays(1), "<");
                    }
                    if (_mfg_swatches.showpending)
                    {
                        qb.Add2Query("mfg_swatches.receivedon = '0001-01-01'");
                    }
                    else
                    {
                        if (_mfg_swatches.invoicefromdate.Date != DateTime.MinValue)
                        {
                            qb.AddParameter("mfg_swatches.invoicedate", "invoicefromdate", _mfg_swatches.invoicefromdate.Date, ">=");
                            qb.AddParameter("mfg_swatches.invoicedate", "invoicetodate", _mfg_swatches.invoicetodate.Date.AddDays(1), "<");
                        }

                        if (_mfg_swatches.receivedfromdate.Date != DateTime.MinValue)
                        {
                            qb.AddParameter("mfg_swatches.receivedon", "receivedfromdate", _mfg_swatches.receivedfromdate.Date, ">=");
                            qb.AddParameter("mfg_swatches.receivedon", "receivedtodate", _mfg_swatches.receivedtodate.Date.AddDays(1), "<");
                        }
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfg_Swatches omfg_swatches = new Mfg_Swatches();
                        omfg_swatches.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfg_swatches.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        omfg_swatches.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        omfg_swatches.brand = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        omfg_swatches.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfg_swatches.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        omfg_swatches.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        omfg_swatches.receivedpcs = reader["receivedpcs"] == DBNull.Value ? 0 : Convert.ToInt32(reader["receivedpcs"]);
                        omfg_swatches.receivedmtr = reader["receivedmtr"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedmtr"]);
                        omfg_swatches.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();

                        omfg_swatches.requiredmtr = reader["requiredmtr"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requiredmtr"]);
                        omfg_swatches.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();

                        omfg_swatches.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        omfg_swatches.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfg_swatches.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfg_swatches.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfg_swatches.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfg_swatches.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfg_swatches.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfg_swatches.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfg_swatches.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfg_swatches.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfg_swatches.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(omfg_swatches);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfg_swatches);
            }
            return lst;
        }

        public Mfg_Swatches Insert(Mfg_Swatches oMfg_Swatches)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfg_Swatches);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches);
            }
            if(oMfg_Swatches.id>0)
               return Search(new Mfg_SwatchesCreteria() { id = oMfg_Swatches.id }).First();
            return oMfg_Swatches;
        }

        public Mfg_Swatches Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches oMfg_Swatches)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.code == null ? "" : oMfg_Swatches.code;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.design == null ? "" : oMfg_Swatches.design;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.receivedon;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.entrydate;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.receivedby;
            command.Parameters.Add("receivedpcs", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches.receivedpcs;
            command.Parameters.Add("receivedmtr", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfg_Swatches.receivedmtr;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.invoiceno == null ? "" : oMfg_Swatches.invoiceno;
            command.Parameters.Add("requiredmtr", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfg_Swatches.requiredmtr;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.pono == null ? "" : oMfg_Swatches.pono;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.invoicedate;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.supplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches.isactive;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.companyid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.designid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.modifiedby;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Swatches.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfg_Swatches;
        }

        public Mfg_Swatches Update(Mfg_Swatches oMfg_Swatches)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfg_Swatches);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches);
            }
            if (oMfg_Swatches.id > 0)
                return Search(new Mfg_SwatchesCreteria() { id = oMfg_Swatches.id }).First();
            return oMfg_Swatches;
        }

        public Mfg_Swatches Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches oMfg_Swatches)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.code == null ? "" : oMfg_Swatches.code;
            command.Parameters.Add("design", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.design == null ? "" : oMfg_Swatches.design;
            command.Parameters.Add("receivedpcs", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches.receivedpcs;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.receivedon;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.entrydate;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.receivedby;
            command.Parameters.Add("receivedmtr", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfg_Swatches.receivedmtr;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.invoiceno == null ? "" : oMfg_Swatches.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.invoicedate;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.supplierid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches.isactive;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.companyid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.designid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.id;
            command.Parameters.Add("requiredmtr", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfg_Swatches.requiredmtr;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfg_Swatches.pono == null ? "" : oMfg_Swatches.pono;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfg_Swatches.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfg_Swatches;
        }


        public bool Delete(Mfg_Swatches oMfg_Swatches)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfg_Swatches);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfg_Swatches);

                
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfg_Swatches oMfg_Swatches)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfg_Swatches.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfg_Swatches.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfg_Swatches.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfg_Swatches.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
