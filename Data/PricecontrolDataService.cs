using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class PricecontrolDataService : SAASData
    {
        const string SQL_Search = @"select id, name, productid, brandid, patternid, materialid, sizeid, styleid, colourid, typeid, 
                isactive, createdby, modifiedby, version, marginmode,costmode,pricingmode, pricingvalue,dealervalue, discountmode, discountvalue, createdon, modifiedon,
                disabled,pricefrom,priceto,supplierid,locationid,sectionid,companyid from pricecontrol where id=@id";

        const string SQL_SearchAll = @"select id, name, productid, brandid, patternid, materialid, sizeid, styleid, colourid, typeid, 
                                        isactive, createdby, modifiedby, version, marginmode,costmode,pricingmode, pricingvalue,dealervalue, discountmode, discountvalue,
                                        createdon, modifiedon,disabled,pricefrom,priceto,supplierid,locationid,sectionid,companyid
                                        from pricecontrol 
                                        where disabled=false and isactive=true 
                                                and (productid=@productid or productid=0)
                                                and (sectionid=@sectionid or sectionid=0)
                                                and (brandid=@brandid or brandid=0)
                                                and (supplierid=@supplierid or supplierid=0)
                                                and (locationid=@locationid or locationid<=0)
                                                and (companyid=@companyid or companyid<=0)
                                                and (patternid=@patternid or patternid=0)
                                                and (materialid=@materialid or materialid=0)
                                                and (sizeid=@sizeid or sizeid=0)
                                                and (styleid=@styleid or styleid=0)
                                                and (colourid=@colourid or colourid=0)
                                                and  (typeid=@typeid or typeid=0)";

        const string SQL_Insert = @"INSERT INTO public.pricecontrol(name, productid, brandid, patternid, materialid, sizeid, styleid, colourid, typeid, isactive, 
                                    createdby, modifiedby, version, marginmode,costmode,pricingmode, pricingvalue,dealervalue, discountmode, discountvalue, createdon, 
                                    modifiedon,disabled,pricefrom,priceto,supplierid,locationid,sectionid,companyid) VALUES(@name, @productid, @brandid, @patternid, @materialid,
                                    @sizeid, @styleid, @colourid, @typeid, @isactive, @createdby, @modifiedby, @version,@marginmode,@costmode, @pricingmode,
                                    @pricingvalue,@dealervalue, @discountmode, @discountvalue, @createdon, @modifiedon,@disabled,@pricefrom,@priceto,@supplierid,@locationid,@sectionid,@companyid) RETURNING id";

        const string SQL_Update = @"UPDATE public.pricecontrol SET name=@name, productid=@productid, brandid=@brandid, patternid=@patternid, materialid=@materialid, 
                                    sizeid=@sizeid, styleid=@styleid, colourid=@colourid, typeid=@typeid, supplierid=@supplierid,locationid=@locationid,sectionid=@sectionid,
                                    modifiedby=@modifiedby, version=@version+1, 
                                    marginmode=@marginmode,costmode=@costmode,
                                    pricingmode=@pricingmode, pricingvalue=@pricingvalue,dealervalue=@dealervalue,
                                    pricefrom=@pricefrom,priceto=@priceto,discountmode=@discountmode, discountvalue=@discountvalue, modifiedon=@modifiedon,disabled=@disabled,companyid=@companyid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.pricecontrol set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_Activate = @"update public.pricecontrol set disabled=@disabled,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Pricecontrol> Search(PricecontrolCreteria _pricecontrol)
        {
            List<Pricecontrol> lst = new List<Pricecontrol>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_pricecontrol.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_pricecontrol.name))
                        qb.AddParameter("name", _pricecontrol.name, "=");

                    if (_pricecontrol.sectionid > 0)
                        qb.AddParameter("sectionid", _pricecontrol.sectionid, "=");

                    if (_pricecontrol.locationid > 0)
                        qb.AddParameter("locationid", _pricecontrol.locationid, "=");

                    if (_pricecontrol.companyid > 0)
                        qb.AddParameter("companyid", _pricecontrol.companyid, "=");

                    if (_pricecontrol.supplierid > 0)
                        qb.AddParameter("supplierid", _pricecontrol.supplierid, "=");

                    if (_pricecontrol.productid > 0)
                        qb.AddParameter("productid", _pricecontrol.productid, "=");

                    if (_pricecontrol.materialid > 0)
                        qb.AddParameter("materialid", _pricecontrol.materialid, "=");

                    if (_pricecontrol.brandid > 0)
                        qb.AddParameter("brandid", _pricecontrol.brandid, "=");

                    if (_pricecontrol.patternid > 0)
                        qb.AddParameter("patternid", _pricecontrol.patternid, "=");

                    if (_pricecontrol.styleid > 0)
                        qb.AddParameter("styleid", _pricecontrol.styleid, "=");

                    if (_pricecontrol.sizeid > 0)
                        qb.AddParameter("sizeid", _pricecontrol.sizeid, "=");

                    if (_pricecontrol.typeid > 0)
                        qb.AddParameter("typeid", _pricecontrol.typeid, "=");

                    if(_pricecontrol.price != 0)
                    {
                        qb.AddORParameter("pricefrom", _pricecontrol.price, "<=", " pricefrom=0 ");
                        qb.AddORParameter("priceto", _pricecontrol.price, ">=", " priceto=0 ");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricecontrol opricecontrol = new Pricecontrol();
                        opricecontrol.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opricecontrol.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opricecontrol.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opricecontrol.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        opricecontrol.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        opricecontrol.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        opricecontrol.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        opricecontrol.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        opricecontrol.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        opricecontrol.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        opricecontrol.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        opricecontrol.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        opricecontrol.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opricecontrol.disabled = reader["disabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["disabled"]);
                        opricecontrol.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opricecontrol.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opricecontrol.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opricecontrol.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);
                        opricecontrol.pricingvalue = reader["pricingvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pricingvalue"]);
                        opricecontrol.dealervalue = reader["dealervalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealervalue"]);
                        

                        opricecontrol.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        opricecontrol.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);


                        opricecontrol.costmode = reader["costmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["costmode"]);
                        opricecontrol.marginmode = reader["marginmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["marginmode"]);


                        opricecontrol.pricefrom = reader["pricefrom"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pricefrom"]);
                        opricecontrol.priceto = reader["priceto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceto"]);
                        opricecontrol.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opricecontrol.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opricecontrol.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(opricecontrol);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _pricecontrol);
            }
            return lst;
        }

        public List<Pricecontrol> SearchAll(PricecontrolCreteria _pricecontrol)
        {
            List<Pricecontrol> lst = new List<Pricecontrol>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.productid;
                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.brandid;
                command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.styleid;
                command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.patternid;
                command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.colourid;
                command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.materialid;
                command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.sizeid>0 ? _pricecontrol.sizeid : 0;
                command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.typeid;
                command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.sectionid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.supplierid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.locationid > 0 ? _pricecontrol.locationid : 0;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _pricecontrol.companyid > 0 ? _pricecontrol.companyid : 0;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Pricecontrol opricecontrol = new Pricecontrol();
                        opricecontrol.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opricecontrol.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        opricecontrol.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        opricecontrol.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        opricecontrol.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        opricecontrol.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        opricecontrol.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        opricecontrol.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        opricecontrol.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        opricecontrol.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        opricecontrol.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opricecontrol.disabled = reader["disabled"] == DBNull.Value ? false : Convert.ToBoolean(reader["disabled"]);
                        opricecontrol.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opricecontrol.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opricecontrol.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opricecontrol.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);
                        opricecontrol.pricingvalue = reader["pricingvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pricingvalue"]);
                        
                        opricecontrol.dealervalue = reader["dealervalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealervalue"]);

                        opricecontrol.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        opricecontrol.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);

                        opricecontrol.costmode = reader["costmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["costmode"]);
                        opricecontrol.marginmode = reader["marginmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["marginmode"]);

                        opricecontrol.pricefrom = reader["pricefrom"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pricefrom"]);
                        opricecontrol.priceto = reader["priceto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceto"]);

                        opricecontrol.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        opricecontrol.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opricecontrol.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opricecontrol.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);

                        opricecontrol.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opricecontrol.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opricecontrol.setWeightage();
                        lst.Add(opricecontrol);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _pricecontrol);
            }
            return lst.OrderByDescending(e => e.Weightage).ToList();
        }

        public List<Pricecontrol> Insert(PricecontrolCreteria oPricecontrol)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oPricecontrol);
                    transaction.Commit();
                    conn.Close();
                }
                var list = SearchAll(oPricecontrol);
                list.RemoveAll(e => e.id == oPricecontrol.id);
                list.Add(oPricecontrol);
                return list;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricecontrol);
            }

            return new List<Pricecontrol>(new PricecontrolCreteria[] { oPricecontrol });
        }

        public Pricecontrol Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricecontrol oPricecontrol)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricecontrol.name == null ? "" : oPricecontrol.name;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.brandid;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.patternid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.materialid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.sizeid;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.styleid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.colourid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.typeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricecontrol.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.version;
            command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricecontrol.disabled;
            command.Parameters.Add("pricingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.pricingmode;
            command.Parameters.Add("pricingvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.pricingvalue;
            command.Parameters.Add("dealervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.dealervalue;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.discountvalue;


            command.Parameters.Add("costmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.costmode;
            command.Parameters.Add("marginmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.marginmode;

            command.Parameters.Add("pricefrom", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.pricefrom;
            command.Parameters.Add("priceto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.priceto;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.companyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.supplierid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.sectionid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPricecontrol.id = Convert.ToInt64(reader[0].ToString());
                    oPricecontrol.isupdated = true;
                }
            }
            return oPricecontrol;
        }

        public List<Pricecontrol> Update(PricecontrolCreteria oPricecontrol)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPricecontrol);
                    transaction.Commit();
                    conn.Close();
                }
                var list = SearchAll(oPricecontrol);
                list.RemoveAll(e => e.id == oPricecontrol.id);
                list.Add(oPricecontrol);
                return list;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricecontrol);
                
                    
            }

            return new List<Pricecontrol>(new PricecontrolCreteria[] { oPricecontrol });
        }

        public PricecontrolCreteria Update(NpgsqlTransaction transaction, NpgsqlConnection conn, PricecontrolCreteria oPricecontrol)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPricecontrol.name == null ? "" : oPricecontrol.name;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.brandid;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.patternid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.materialid;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.sizeid;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.styleid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.colourid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.typeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricecontrol.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.version;
            command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricecontrol.disabled;
            command.Parameters.Add("pricingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.pricingmode;
            command.Parameters.Add("pricingvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.pricingvalue;
            command.Parameters.Add("dealervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.dealervalue;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.discountvalue;

            command.Parameters.Add("costmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.costmode;
            command.Parameters.Add("marginmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.marginmode;


            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPricecontrol.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

            command.Parameters.Add("pricefrom", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.pricefrom;
            command.Parameters.Add("priceto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPricecontrol.priceto;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.companyid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.supplierid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.sectionid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPricecontrol.version = Convert.ToInt32(reader[0].ToString());
                    oPricecontrol.isupdated = true;
                }
            }

            return oPricecontrol;
        }


        public bool Delete(Pricecontrol oPricecontrol)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPricecontrol);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricecontrol);

                
                    
            }
            return hasSuccessCount;
        }

        public Pricecontrol Activate(Pricecontrol oPricecontrol)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    NpgsqlCommand command = GetCommand(SQL_Activate, conn, transaction);
                    command.Parameters.Add("disabled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricecontrol.disabled;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.id;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oPricecontrol.version = Convert.ToInt32(reader[0].ToString());
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPricecontrol);

                
                    
            }
            return oPricecontrol;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Pricecontrol oPricecontrol)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPricecontrol.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPricecontrol.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPricecontrol.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
