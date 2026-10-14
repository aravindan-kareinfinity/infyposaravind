using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ProductsDataService : SAASData
    {
        const string SQL_SearchList = @"select id,code,name,hsncode,purchasetaxid,salestaxid,margin,attributes,producttype,attributelist,minmargin,maxmargin,pricingmode,barcodesource,attributefilter,prodcompanyid,sellingmode,discountmode,
        barcodemode,discountvalue,productgroup,autopo,prdcoverageid,stdump,prddumpid,coreprdgrpid,basepriceid,salescounterid,uomid,barcodeid,hascess,cesstaxid from products where id=@id";

        const string SQL_Search = @"select code, name, isactive, createdby, modifiedby, version, id, createdon, modifiedon, sellingmode, pricingmode, salestaxid, discountmode, discountvalue, purchasetaxid,salesman,autopo,prdcoverageid,
                                    stdump,prddumpid,coreprdgrpid,basepriceid,salescounterid, margin, image, barcodeprefix, barcodesuffix, brands,barcodemode,analysing,analysingcofig, pricerange,keygroup,stockholdingperiod,isrefill,
                                    purchaseplanmode,gender,sizegroupid,productgroup,barcodeid,hascess,cesstaxid,
                                    unit,fixedprice,margins,hsncode,deactivatedon,activatedon,attributes,attributelist,minmargin,maxmargin,producttype,uomid,barcodesource,isdailyprice,attributefilter,prodcompanyid from products where id=@id";

        const string SQL_Insert = @"INSERT INTO public.products(code, name, isactive, createdby, modifiedby, version, createdon, modifiedon, sellingmode, pricingmode,barcodeid, salestaxid, discountmode, discountvalue,autopo,prdcoverageid,
                stdump,prddumpid,coreprdgrpid,basepriceid,purchasetaxid, salescounterid, margin, image, barcodeprefix, barcodesuffix, brands,barcodemode,analysing,analysingcofig,pricerange,keygroup,stockholdingperiod,isrefill,purchaseplanmode,gender,sizegroupid,productgroup,unit,
                fixedprice,margins,hsncode,deactivatedon,activatedon,salesman,attributes,attributelist,minmargin,maxmargin,producttype,uomid,barcodesource,isdailyprice,attributefilter,prodcompanyid,hascess,cesstaxid,
                productgroup_1,productgroup_2,productgroup_3,productgroup_4,productgroup_5) 
                VALUES(@code, @name, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @sellingmode, @pricingmode,@barcodeid, @salestaxid, @discountmode, @discountvalue,@autopo,@prdcoverageid,
                @stdump,@prddumpid,@coreprdgrpid,@basepriceid,@purchasetaxid, @salescounterid,@margin, @image, @barcodeprefix, @barcodesuffix, @brands,@barcodemode,@analysing,@analysingcofig,@pricerange,@keygroup,@stockholdingperiod,@isrefill,@purchaseplanmode,@gender,@sizegroupid,@productgroup,
                @unit,@fixedprice,@margins,@hsncode,@deactivatedon,@activatedon,@salesman,@attributes,@attributelist,@minmargin,@maxmargin,@producttype,@uomid,@barcodesource,@isdailyprice,@attributefilter,@prodcompanyid,@hascess,@cesstaxid,
                @productgroup_1,@productgroup_2,@productgroup_3,@productgroup_4,@productgroup_5) RETURNING id";

        const string SQL_UpdateAttribute = @"UPDATE public.products SET attributelist=@attributelist,version=version+1,modifiedon=@modifiedon where id=@id RETURNING version";

        const string SQL_Update = @"UPDATE public.products SET code=@code, name=@name, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, sellingmode=@sellingmode, pricingmode=@pricingmode, autopo=@autopo,prdcoverageid=@prdcoverageid,
        salestaxid=@salestaxid, discountmode=@discountmode, discountvalue=@discountvalue, purchasetaxid=@purchasetaxid, salescounterid=@salescounterid, margin=@margin, image=@image, barcodeprefix=@barcodeprefix,salesman=@salesman,
        barcodesuffix=@barcodesuffix, brands=@brands,barcodemode=@barcodemode,analysing=@analysing,analysingcofig=@analysingcofig,pricerange=@pricerange,keygroup=@keygroup,stockholdingperiod=@stockholdingperiod,isrefill=@isrefill,
        purchaseplanmode=@purchaseplanmode,gender=@gender,sizegroupid=@sizegroupid,productgroup=@productgroup,unit=@unit,fixedprice=@fixedprice,margins=@margins,hsncode=@hsncode,deactivatedon=@deactivatedon,activatedon=@activatedon,
        isactive=@isactive,attributes=@attributes,attributelist=@attributelist,minmargin=@minmargin,maxmargin=@maxmargin,producttype=@producttype,uomid=@uomid,barcodesource=@barcodesource,isdailyprice=@isdailyprice,attributefilter=@attributefilter,
        stdump=@stdump,prddumpid=@prddumpid,coreprdgrpid=@coreprdgrpid,basepriceid=@basepriceid,barcodeid=@barcodeid,hascess=@hascess,cesstaxid=@cesstaxid,
        prodcompanyid=@prodcompanyid,productgroup_1=@productgroup_1,productgroup_2=@productgroup_2,
        productgroup_3=@productgroup_3,productgroup_4=@productgroup_4,productgroup_5=@productgroup_5 
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.products set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public SortedDictionary<long, int> GetSellingModes(List<long> ids)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = "Select id,sellingmode from products";
            qb.AddParameter("isactive", true, "=");
            qb.AddInParamer("id", string.Join<long>(",", ids));
            NpgsqlConnection conn = CreateConnection();
            SortedDictionary<long, int> sd = new SortedDictionary<long, int>();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sd.Add(reader.GetInt64(0), reader.GetInt32(1));
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return sd;
        }

        public SortedDictionary<long, int> GetBarcodeModes(List<long> ids)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = "Select id,barcodemode from products";
            qb.AddParameter("isactive", true, "=");
            qb.AddInParamer("id", string.Join<long>(",", ids));
            NpgsqlConnection conn = CreateConnection();
            SortedDictionary<long, int> sd = new SortedDictionary<long, int>();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sd.Add(reader.GetInt64(0), reader.GetInt32(1));
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return sd;
        }

        internal bool UpdateAttributelist(ProductsBase oProducts)
        {
            var success = false;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(SQL_UpdateAttribute, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.id;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("attributelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.attributelist_json == null ? "null" : oProducts.attributelist_json;
                success = command.ExecuteNonQuery() > 0;
                conn.Close();
            }
            return success;
        }

        public List<ProductsBase> SearchList(ProductsCreteria _products)
        {
            List<ProductsBase> lst = new List<ProductsBase>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_products.id != 0)
                {
                    command = GetCommand(SQL_SearchList, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _products.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchList.Substring(0, SQL_SearchList.IndexOf("where"));
                    if (_products.inactive == true)
                    {
                        qb.AddParameter("isactive", !_products.inactive, "=");
                    }
                    else if (!_products.allproducts)
                    {
                        qb.AddParameter("isactive", true, "=");
                    }

                    if (_products.showdailyprice)
                        qb.AddParameter("isdailyprice", _products.isdailyprice, "=");

                    if (_products.barcodemode > 0)
                        qb.AddParameter("barcodemode", _products.barcodemode, "=");

                    if (_products.sellingmode > 0)
                        qb.AddParameter("sellingmode", _products.sellingmode, "=");

                    if (_products.salestaxid > 0)
                        qb.AddParameter("salestaxid", _products.salestaxid, "=");

                    if (_products.salescounterid > 0)
                        qb.AddParameter("salescounterid", _products.salescounterid, "=");

                    if (!string.IsNullOrEmpty(_products.code))
                    {
                        qb.AddParameter("code", "%" + _products.code + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_products.name))
                    {
                        qb.AddParameter("name", "%" + _products.name + "%", " ilike ");
                    }
                    if (_products.ids != null && _products.ids.Count > 0)
                    {
                        qb.AddInParamer("products.id", string.Join<long>(",", _products.ids));
                    }

                    if(_products.productgroup>0)
                        qb.AddParameter("productgroup", _products.productgroup, "=");

                    if (!_products.includeconsumables)
                        qb.Add2Query("COALESCE(products.producttype, 0) != 9");

                    qb.SortField = "name";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductsBase oproducts = new ProductsBase();
                        oproducts.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproducts.prodcompanyid = reader["prodcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prodcompanyid"]);
                        oproducts.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oproducts.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config != null && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_useproductcodewithname)
                            oproducts.name = oproducts.code + " - " + oproducts.name;
                        oproducts.autopo = reader["autopo"] == DBNull.Value ? false : Convert.ToBoolean(reader["autopo"]);
                        oproducts.barcodeid = reader["barcodeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["barcodeid"]);

                        oproducts.hascess = reader["hascess"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascess"]);
                        oproducts.cesstaxid = reader["cesstaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cesstaxid"]);


                        oproducts.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oproducts.productgroup = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oproducts.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        oproducts.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        oproducts.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);
                        oproducts.barcodemode = reader["barcodemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["barcodemode"]);
                        oproducts.purchasetaxid = reader["purchasetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasetaxid"]);
                        oproducts.salestaxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"]);
                        oproducts.salescounterid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        oproducts.margin = reader["margin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["margin"]);
                        oproducts.minmargin = reader["minmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minmargin"]);
                        oproducts.maxmargin = reader["maxmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["maxmargin"]);
                        oproducts.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);
                        oproducts.barcodesource = reader["barcodesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["barcodesource"]);
                        oproducts.attributefilter = reader["attributefilter"] == DBNull.Value ? false : Convert.ToBoolean(reader["attributefilter"]);
                        oproducts.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        oproducts.attributelist_json = reader["attributelist"] == DBNull.Value ? "null" : reader["attributelist"].ToString();
                        oproducts.producttype = reader["producttype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["producttype"]);
                        oproducts.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["uomid"]);
                        
                        lst.Add(oproducts);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _products);
            }
            return lst;
        }
        public List<Products> Search(ProductsCreteria _products)
        {
            List<Products> lst = new List<Products>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_products.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _products.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    if (_products.inactive == true)
                    {
                        qb.AddParameter("isactive", !_products.inactive, "=");
                    }
                    else if (!_products.allproducts)
                    {
                        qb.AddParameter("isactive", true, "=");
                    }

                    if (_products.showproducttype && _products.producttype >= 0)
                        qb.AddParameter("producttype", _products.producttype, "=");

                    if (_products.showdailyprice)
                    {
                        qb.AddParameter("isdailyprice", _products.isdailyprice, "=");
                        if(_products.basepriceid>0)
                        {
                            qb.AddParameter("basepriceid", _products.basepriceid, "=");
                        }
                    }

                    if (_products.barcodemode > 0)
                        qb.AddParameter("barcodemode", _products.barcodemode, "=");
                    else if (_products.barcodemode == -1)
                        qb.AddParameter("barcodemode", 0, "=");

                    if (_products.sellingmode > 0)
                        qb.AddParameter("sellingmode", _products.sellingmode, "=");

                    if (_products.salestaxid > 0)
                        qb.AddParameter("salestaxid", _products.salestaxid, "=");

                    if (_products.salescounterid > 0)
                        qb.AddParameter("salescounterid", _products.salescounterid, "=");

                    if (!string.IsNullOrEmpty(_products.code))
                    {
                        if (_products.uniqueproduct)
                            qb.AddParameter("code", _products.code, "=");
                        else
                            qb.AddParameter("code", "%" + _products.code + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_products.name))
                    {
                        qb.AddParameter("name", "%" + _products.name + "%", " ilike ");
                    }
                    if (_products.ids != null && _products.ids.Count > 0)
                    {
                        qb.AddInParamer("products.id", string.Join<long>(",", _products.ids));
                    }
                    qb.SortField = "name";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Products oproducts = new Products();
                        oproducts.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oproducts.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oproducts.minmargin = reader["minmargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["minmargin"]);
                        oproducts.maxmargin = reader["maxmargin"] == DBNull.Value ? 0 : Convert.ToInt32(reader["maxmargin"]);
                        oproducts.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproducts.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproducts.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproducts.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproducts.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproducts.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproducts.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oproducts.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"]);
                        oproducts.pricingmode = reader["pricingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pricingmode"]);

                        oproducts.barcodesource = reader["barcodesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["barcodesource"]);
                        oproducts.isdailyprice = reader["isdailyprice"] == DBNull.Value ? false : Convert.ToBoolean(reader["isdailyprice"]);
                        oproducts.attributefilter = reader["attributefilter"] == DBNull.Value ? false : Convert.ToBoolean(reader["attributefilter"]);
                        oproducts.prodcompanyid = reader["prodcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prodcompanyid"]);
                        oproducts.salestaxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"]);
                        oproducts.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        oproducts.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);
                        oproducts.purchasetaxid = reader["purchasetaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasetaxid"]);
                        oproducts.salescounterid = reader["salescounterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salescounterid"]);
                        oproducts.margin = reader["margin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["margin"]);
                        oproducts.image = reader["image"] == DBNull.Value ? new byte[0] : (byte[])reader["image"];
                        oproducts.barcodeprefix = reader["barcodeprefix"] == DBNull.Value ? "" : reader["barcodeprefix"].ToString();
                        oproducts.barcodesuffix = reader["barcodesuffix"] == DBNull.Value ? "" : reader["barcodesuffix"].ToString();
                        oproducts.brands_json = reader["brands"] == DBNull.Value ? "null" : reader["brands"].ToString();
                        oproducts.analysingcofig_json = reader["analysingcofig"] == DBNull.Value ? "null" : reader["analysingcofig"].ToString();
                        oproducts.analysing = reader["analysing"] == DBNull.Value ? false : Convert.ToBoolean(reader["analysing"]);
                        oproducts.barcodemode = reader["barcodemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["barcodemode"]);
                        oproducts.pricerange_json = reader["pricerange"] == DBNull.Value ? "null" : reader["pricerange"].ToString();
                        oproducts.salesman_json = reader["salesman"] == DBNull.Value ? "null" : reader["salesman"].ToString();
                        oproducts.keygroup_json = reader["keygroup"] == DBNull.Value ? "null" : reader["keygroup"].ToString();
                        oproducts.stockholdingperiod = reader["stockholdingperiod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stockholdingperiod"]);
                        oproducts.isrefill = reader["isrefill"] == DBNull.Value ? false : Convert.ToBoolean(reader["isrefill"]);
                        oproducts.purchaseplanmode = reader["purchaseplanmode"] == DBNull.Value ? Products.PurchasePlanMode.None : (Products.PurchasePlanMode)Convert.ToInt32(reader["purchaseplanmode"]);
                        oproducts.gender = reader["gender"] == DBNull.Value ? 0 : Convert.ToInt64(reader["gender"]);
                        oproducts.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        oproducts.productgroup = reader["productgroup"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productgroup"]);
                        oproducts.autopo = reader["autopo"] == DBNull.Value ? false : Convert.ToBoolean(reader["autopo"]);
                        oproducts.unit = reader["unit"] == DBNull.Value ? 0 : Convert.ToInt64(reader["unit"]);
                        oproducts.prdcoverageid = reader["prdcoverageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prdcoverageid"]);

                        oproducts.prddumpid = reader["prddumpid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddumpid"]);
                        oproducts.coreprdgrpid = reader["coreprdgrpid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["coreprdgrpid"]);
                        oproducts.basepriceid = reader["basepriceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["basepriceid"]);
                        oproducts.stdump = reader["stdump"] == DBNull.Value ? false : Convert.ToBoolean(reader["stdump"]);

                        oproducts.fixedprice = reader["fixedprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fixedprice"]);
                        oproducts.margins = reader["margins"] == DBNull.Value ? "" : reader["margins"].ToString();
                        oproducts.activatedon = reader["activatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["activatedon"]);
                        oproducts.deactivatedon = reader["deactivatedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deactivatedon"]);
                        oproducts.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oproducts.barcodeid = reader["barcodeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["barcodeid"]);
                        oproducts.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();
                        oproducts.attributelist_json = reader["attributelist"] == DBNull.Value ? "null" : reader["attributelist"].ToString();
                        oproducts.producttype = reader["producttype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["producttype"]);
                        oproducts.uomid = reader["uomid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["uomid"]);
                        oproducts.hascess = reader["hascess"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascess"]);
                        oproducts.cesstaxid = reader["cesstaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cesstaxid"]);


                        lst.Add(oproducts);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _products);
            }
            return lst;
        }
        private bool UniqueValidation(Products oProducts)
        {
            if (isMigration || string.IsNullOrEmpty(oProducts.name)) return false;

            string _Query = @"select count(id) items from products where lower(name)=@name and id!=@id";

            if (oProducts.id > 0)
                _Query = @"select count(id) items from products where isactive=true and lower(name)=@name and id!=@id";

            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.id;
                command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.name.ToLower();
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

        public Products Insert(Products oProducts)
        {
            if (UniqueValidation(oProducts))
            {
                oProducts.ErrorMessage = "Duplicate Product Name Exists. Please verify active and inactive products";
                return oProducts;
            }

            if (!isMigration && (oProducts.purchasetaxid == 0 || oProducts.salestaxid == 0))
            {
                oProducts.ErrorMessage = "Product must be mapped with Tax component.";
                return oProducts;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("products.code", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (string.IsNullOrEmpty(oProducts.code))
                        oProducts.code = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oProducts);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProducts);
            }

            return oProducts;
        }

        public Products Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Products oProducts)
        {
            

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.code == null ? "" : oProducts.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.name == null ? "" : oProducts.name;
            command.Parameters.Add("barcodeprefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.barcodeprefix == null ? "" : oProducts.barcodeprefix;
            command.Parameters.Add("barcodesuffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.barcodesuffix == null ? "" : oProducts.barcodesuffix;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (Services.ConfigManager.Instance(Context.OrganizationID).Config == null || Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_product == false) ? oProducts.isactive : false;
            command.Parameters.Add("autopo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.autopo;
            command.Parameters.Add("prdcoverageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.prdcoverageid;

            command.Parameters.Add("prddumpid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.prddumpid;
            command.Parameters.Add("coreprdgrpid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.coreprdgrpid;
            command.Parameters.Add("basepriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.basepriceid;
            command.Parameters.Add("stdump", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.stdump;

            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.version;
            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.barcodemode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("sellingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.sellingmode;
            command.Parameters.Add("pricingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.pricingmode;

            command.Parameters.Add("barcodesource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.barcodesource;
            command.Parameters.Add("isdailyprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.isdailyprice;
            command.Parameters.Add("attributefilter", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.attributefilter;
            command.Parameters.Add("prodcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.prodcompanyid;

            command.Parameters.Add("productgroup_1", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_1;
            command.Parameters.Add("productgroup_2", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_2;
            command.Parameters.Add("productgroup_3", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_3;
            command.Parameters.Add("productgroup_4", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_4;
            command.Parameters.Add("productgroup_5", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_5;

            command.Parameters.Add("salestaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.salestaxid;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.discountmode;
            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.barcodemode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.discountvalue;
            command.Parameters.Add("purchasetaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.purchasetaxid;
            command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.salescounterid;
            command.Parameters.Add("margin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.margin;
            command.Parameters.Add("producttype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.producttype;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.uomid;
            
            if (oProducts.image == null)
            {
                command.Parameters.Add("image", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("image", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oProducts.image;
            }
            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.barcodemode;
            command.Parameters.Add("barcodeprefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.barcodeprefix == null ? "" : oProducts.barcodeprefix;
            command.Parameters.Add("barcodesuffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.barcodesuffix == null ? "" : oProducts.barcodesuffix;
            command.Parameters.Add("brands", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.brands_json == null ? "null" : oProducts.brands_json;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.attributes_json == null ? "null" : oProducts.attributes_json;
            command.Parameters.Add("attributelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.attributelist_json == null ? "null" : oProducts.attributelist_json;
            command.Parameters.Add("salesman", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.salesman_json == null ? "null" : oProducts.salesman_json;

            command.Parameters.Add("minmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.minmargin;
            command.Parameters.Add("maxmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.maxmargin;

            command.Parameters.Add("analysingcofig", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.analysingcofig_json == null ? "null" : oProducts.analysingcofig_json;
            command.Parameters.Add("analysing", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.analysing;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.pricerange_json == null ? "null" : oProducts.pricerange_json;
            command.Parameters.Add("keygroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.keygroup_json == null ? "null" : oProducts.keygroup_json;

            command.Parameters.Add("stockholdingperiod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.stockholdingperiod;
            command.Parameters.Add("isrefill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.isrefill;
            command.Parameters.Add("purchaseplanmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.purchaseplanmode;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.gender;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.sizegroupid;
            command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup;
            command.Parameters.Add("unit", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.unit;
            command.Parameters.Add("fixedprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.fixedprice;
            command.Parameters.Add("activatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProducts.activatedon;
            command.Parameters.Add("deactivatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProducts.deactivatedon;
            command.Parameters.Add("margins", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.margins == null ? "" : oProducts.margins;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.hsncode == null ? "" : oProducts.hsncode;
            command.Parameters.Add("barcodeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.barcodeid;
            command.Parameters.Add("cesstaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.cesstaxid;
            command.Parameters.Add("hascess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.hascess;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProducts.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (!isMigration)
            {
                InfyPOS.Processors.CoreDBListener.SendNotification(Context.OrganizationID, new InfyPOS.Processors.Config()
                {
                    id = oProducts.id,
                    type = "CACHE",
                    subtype = "PRODUCT",
                    value = (oProducts.attributes != null && oProducts.attributes.excludereward) ? "ExcludeReward" : "IncludeReward",
                    actionon = oProducts.modifiedon
                }, conn);
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_product)
            {
                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.Product,
                    oProducts.id, string.Format("PRODUCT CREATION REQUEST - {0} ", oProducts.name),
                    Context.UserID, 0, transaction, Context);
            }
            return oProducts;
        }

        public Products Update(Products oProducts)
        {
            if (UniqueValidation(oProducts))
            {
                oProducts.ErrorMessage = "Duplicate Product Name Exists. Please verify active and inactive products";
                return oProducts;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oProducts);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProducts);
            }

            return oProducts;
        }

        public Products Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Products oProducts)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);

            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.code == null ? "" : oProducts.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.name == null ? "" : oProducts.name;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.id;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProducts.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("sellingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.sellingmode;
            command.Parameters.Add("pricingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.pricingmode;
            command.Parameters.Add("salestaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.salestaxid;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.discountvalue;
            command.Parameters.Add("purchasetaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.purchasetaxid;
            command.Parameters.Add("salescounterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.salescounterid;
            command.Parameters.Add("prodcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.prodcompanyid;
            command.Parameters.Add("margin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.margin;
            command.Parameters.Add("producttype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.producttype;
            command.Parameters.Add("uomid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.uomid;
            command.Parameters.Add("barcodesource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.barcodesource;
            command.Parameters.Add("isdailyprice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.isdailyprice;
            command.Parameters.Add("attributefilter", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.attributefilter;
            command.Parameters.Add("autopo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.autopo;
            command.Parameters.Add("productgroup_1", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_1;
            command.Parameters.Add("productgroup_2", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_2;
            command.Parameters.Add("productgroup_3", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_3;
            command.Parameters.Add("productgroup_4", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_4;
            command.Parameters.Add("productgroup_5", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup_5;
            command.Parameters.Add("prdcoverageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.prdcoverageid;

            command.Parameters.Add("prddumpid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.prddumpid;
            command.Parameters.Add("coreprdgrpid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.coreprdgrpid;
            command.Parameters.Add("basepriceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.basepriceid;
            command.Parameters.Add("stdump", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.stdump;

            if (oProducts.image == null)
            {
                command.Parameters.Add("image", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("image", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oProducts.image;
            }
            command.Parameters.Add("minmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.minmargin;
            command.Parameters.Add("maxmargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.maxmargin;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.attributes_json == null ? "null" : oProducts.attributes_json;
            command.Parameters.Add("attributelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.attributelist_json == null ? "null" : oProducts.attributelist_json;
            command.Parameters.Add("salesman", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.salesman_json == null ? "null" : oProducts.salesman_json;
            command.Parameters.Add("barcodemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.barcodemode;
            command.Parameters.Add("barcodeprefix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.barcodeprefix == null ? "" : oProducts.barcodeprefix;
            command.Parameters.Add("barcodesuffix", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.barcodesuffix == null ? "" : oProducts.barcodesuffix;
            command.Parameters.Add("brands", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.brands_json == null ? "null" : oProducts.brands_json;
            command.Parameters.Add("analysingcofig", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.analysingcofig_json == null ? "null" : oProducts.analysingcofig_json;
            command.Parameters.Add("analysing", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.analysing;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.pricerange_json == null ? "null" : oProducts.pricerange_json;
            command.Parameters.Add("keygroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oProducts.keygroup_json == null ? "null" : oProducts.keygroup_json;
            command.Parameters.Add("stockholdingperiod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.stockholdingperiod;
            command.Parameters.Add("isrefill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.isrefill;
            command.Parameters.Add("purchaseplanmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.purchaseplanmode;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.gender;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.sizegroupid;
            command.Parameters.Add("productgroup", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.productgroup;
            command.Parameters.Add("unit", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.unit;
            command.Parameters.Add("fixedprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oProducts.fixedprice;
            command.Parameters.Add("activatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProducts.activatedon;
            command.Parameters.Add("deactivatedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProducts.deactivatedon;
            command.Parameters.Add("margins", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.margins == null ? "" : oProducts.margins;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProducts.hsncode == null ? "" : oProducts.hsncode;
            command.Parameters.Add("barcodeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.barcodeid;
            command.Parameters.Add("cesstaxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.cesstaxid;
            command.Parameters.Add("hascess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProducts.hascess;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProducts.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oProducts;
        }

        internal bool Marge(MergeProductsList item)
        {
            Products _newitem = null;
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

                    string query = @"update items set productid=@newid, productname=@newname where productid=@oldid;
                                     update billitems set productid=@newid where productid=@oldid;
                                     update stock set productid=@newid where productid=@oldid;
                                     update lrinvoiceitems set productid=@newid where productid=@oldid;
                                     update lrinvoicereturnitems set productid=@newid where productid=@oldid;
                                     update mfgworkordergrnitems set productid=@newid where productid=@oldid;
                                     update mfgworkorderitems set productid=@newid where productid=@oldid;
                                     update mfgdesign set productid=@newid where productid=@oldid;
                                    update jobworkbillgrnitems set productid=@newid where productid=@oldid;
                                    update jobworkbillitems set productid=@newid where productid=@oldid;
                                    update jobworksamples set productid=@newid where productid=@oldid;
                                    update salesorderitems set productid=@newid where productid=@oldid;";

                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    command.CommandTimeout = 10000;
                    command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("newname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _newitem.name;
                    command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var ditem in item.deletelist)
                    {
                        command.Parameters["oldid"].Value = ditem.id;
                        int successCount = command.ExecuteNonQuery();
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

        public bool Delete(Products oProducts)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oProducts);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProducts);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Products oProducts)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProducts.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProducts.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal bool Marge(MergeProducts item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                if (!item.deletelist.Exists(e => e.id == item.id))
                {
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

                    string query = @"update items set productid=@newid where productid=@oldid;
                                     update stock set productid=@newid where productid=@oldid;
                                    update stock set productid=@newid where productid=@oldid;
                                    update stock set productid=@newid where productid=@oldid;";

                    switch (item.type)
                    {
                        case "SIZE":
                            query = "update items set sizeid=@newid where sizeid=@oldid";
                            break;
                        case "PATTERN":
                            query = "update items set patternid=@newid where patternid=@oldid";
                            break;
                        case "STYLE":
                            query = "update items set styleid=@newid where styleid=@oldid";
                            break;
                        case "COLOUR":
                            query = "update items set colourid=@newid where colourid=@oldid";
                            break;
                        case "MATERIAL":
                            query = "update items set materialid=@newid where materialid=@oldid";
                            break;
                    }
                    NpgsqlCommand command = new NpgsqlCommand(query, conn, transaction);
                    command.Parameters.Add("newid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("oldid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var ditem in item.deletelist)
                    {
                        command.Parameters["oldid"].Value = ditem.id;
                        int successCount = command.ExecuteNonQuery();
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

        internal bool ChangeHSN(List<ProductHSNCode> items)
        {
            string query = @"Update products set hsncode = @hsncode where id=@productid;";
            //Update stock set hsncode = @hsncode where productid = @productid;

            foreach (var item in items)
            {
                var param = new SortedList<string, object>();
                param.Add("productid", item.productid);
                param.Add("hsncode", item.hsncode);
                ExecuteQuery(query, param);
            }
            return true;
        }
        public bool ChangeProductGroup(ProductChangeGroup item)
        {
            string updatetax = @"update products set productgroup=@productgroup where id in (" +
                string.Join(",", item.productids) + ")";

            if (item.removeexisting)
                updatetax = "update products set productgroup=0 where productgroup=@productgroup;" + updatetax;

            var paramTax = new SortedList<string, object>();
            paramTax.Add("productgroup", item.productgroup);
            return ExecuteQuery(updatetax, paramTax) > 0;
        }
        public bool ChangeTax(ProductChangeTax item)
        {
            if (item.purchasetaxid == 0 && item.salestaxid == 0) return false;

            string updatetax = @"update products set {0} {1} where id in (" +
                string.Join(",", item.productids) + ")";

            var paramTax = new SortedList<string, object>();
            paramTax.Add("purchasetaxid", item.purchasetaxid);
            paramTax.Add("salestaxid", item.salestaxid);
            return ExecuteQuery(string.Format(updatetax, (item.purchasetaxid == 0 ? "purchasetaxid=purchasetaxid," : "purchasetaxid=@purchasetaxid,"), (item.salestaxid == 0 ? "" : "salestaxid=@salestaxid ")), paramTax) > 0;


            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var alltaxs = taxDataService.Search(new TaxCreteria()
            {

            });
            var purchasetax = taxDataService.Search(new TaxCreteria()
            {
                id = item.purchasetaxid
            }).First();

            var salestax = taxDataService.Search(new TaxCreteria()
            {
                id = item.salestaxid
            }).First();

            string query = @"select buyingqty,buyingprice,lrinvoiceitems.basebuyingprice,lrinvoiceitems.amount,
            lrinvoiceitems.purchasetax,lrinvoiceitems.id,lrinvoiceitems.lrinvoiceid,isinclusivetax,purchasediscount,
            lrinvoice.invoicedate,isinterstatesale,products.purchasetaxid taxid from lrinvoiceitems
            inner join lrinvoice on lrinvoice.id = lrinvoiceid
            inner join products on products.id = productid
            where invoicedate>=@fromdate and invoicedate<@todate";
            var param = new SortedList<string, object>();
            param.Add("fromdate", item.fromdate.ToLocalTime().Date);
            param.Add("todate", item.todate.ToLocalTime().Date);
            List<TaxPurchaseItem> items = new List<TaxPurchaseItem>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    TaxPurchaseItem taxChangeItem = new TaxPurchaseItem();
                    taxChangeItem.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                    taxChangeItem.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    taxChangeItem.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                    taxChangeItem.buyingqty = reader["buyingqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingqty"]);
                    taxChangeItem.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                    taxChangeItem.isinclusivetax = reader["isinclusivetax"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinclusivetax"]);
                    taxChangeItem.basebuyingprice = reader["basebuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basebuyingprice"]);
                    taxChangeItem.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    taxChangeItem.purchasetax = reader["purchasetax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetax"]);
                    taxChangeItem.purchasediscount = reader["purchasediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasediscount"]);
                    taxChangeItem.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    items.Add(taxChangeItem);
                }
            }

            foreach (var taxChangeItem in items)
            {
                purchasetax = alltaxs.Find(e => e.id == taxChangeItem.taxid);
                if (purchasetax == null)
                {
                    purchasetax = alltaxs.Find(e => e.taxsplit != null && e.taxsplit.items != null && e.taxsplit.items.Count() == 2);
                }
                if (taxChangeItem.isinclusivetax)
                {
                    taxChangeItem.buyingprice = taxChangeItem.basebuyingprice - taxChangeItem.purchasediscount;
                    var taxinfo = purchasetax.taxsplit.Find(taxChangeItem.buyingprice);
                    taxChangeItem.ptaxid = purchasetax.id;
                    taxChangeItem.taxid = salestax.id;
                    taxChangeItem.TaxPercentage = taxinfo.taxpercent;
                    var buyingprice = Math.Round(taxChangeItem.buyingprice / (100 + taxinfo.taxpercent) * 100, 2);
                    if (taxChangeItem.purchasetax != Math.Round((taxChangeItem.buyingprice / (100 + taxinfo.taxpercent) * taxinfo.taxpercent) * taxChangeItem.buyingqty, 2))
                        taxChangeItem.changed = true;
                    taxChangeItem.purchasetax = Math.Round((taxChangeItem.buyingprice / (100 + taxinfo.taxpercent) * taxinfo.taxpercent) * taxChangeItem.buyingqty, 2);
                    taxChangeItem.buyingprice = buyingprice;
                }
                else
                {
                    taxChangeItem.buyingprice = Math.Round(taxChangeItem.basebuyingprice - taxChangeItem.purchasediscount, 2);

                    var taxinfo = purchasetax.taxsplit.Find(taxChangeItem.buyingprice);
                    taxChangeItem.ptaxid = purchasetax.id;
                    taxChangeItem.taxid = salestax.id;
                    taxChangeItem.TaxPercentage = taxinfo.taxpercent;

                    if (taxChangeItem.purchasetax != Math.Round((taxChangeItem.buyingprice * taxinfo.taxpercent / 100) * taxChangeItem.buyingqty, 2))
                        taxChangeItem.changed = true;
                    taxChangeItem.purchasetax = Math.Round((taxChangeItem.buyingprice * taxinfo.taxpercent / 100) * taxChangeItem.buyingqty, 2);
                }
                taxChangeItem.amount = taxChangeItem.buyingprice * taxChangeItem.buyingqty;
            }

            //var tamount = items.Sum(e => e.basebuyingprice * e.buyingqty);
            //var taxamount = items.Sum(e => e.basebuyingprice * e.buyingqty/112*12);
            //var taxmissmatchvalue = items.Sum(e => e.purchasetax - (e.basebuyingprice * e.buyingqty / 112 * 12));
            //var taxmissmatch = items.FindAll(e => e.purchasetax != Math.Round(e.basebuyingprice * e.buyingqty / 112 * 12,4));

            query = @"select lrinvoice.id invoiceid,lrinvoice.taxamount,lrinvoice.baseamount,lrinvoice.grossamount,lrinvoice.totalamount,lrinvoice.discount,
lrinvoicetax.percentage,lrinvoicetax.amounton,lrinvoicetax.taxid,lrinvoicetax.taxamount lrtaxamount,lrinvoicetax.netamount,lrinvoicetax.id lrtaxid from lrinvoice
inner join lrinvoicetax on lrinvoicetax.invoiceid = lrinvoice.id
where invoicedate>=@fromdate and invoicedate<@todate";
            List<TaxInvoiceItem> invitems = new List<TaxInvoiceItem>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    TaxInvoiceItem taxChangeItem = new TaxInvoiceItem();
                    taxChangeItem.invoiceid = reader["invoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["invoiceid"]);
                    taxChangeItem.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                    taxChangeItem.lrtaxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrtaxid"]);
                    taxChangeItem.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                    taxChangeItem.baseamount = reader["baseamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["baseamount"]);
                    taxChangeItem.grossamount = reader["grossamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grossamount"]);
                    taxChangeItem.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                    taxChangeItem.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    taxChangeItem.percentage = reader["percentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["percentage"]);
                    taxChangeItem.amounton = reader["amounton"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amounton"]);
                    taxChangeItem.lrtaxamount = reader["lrtaxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lrtaxamount"]);
                    taxChangeItem.netamount = reader["netamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netamount"]);
                    taxChangeItem.amounton = reader["amounton"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amounton"]);
                    invitems.Add(taxChangeItem);
                }
            }

            var listLrinvoicetax = new List<Lrinvoicetax>();
            var listLrinvoiceItems = new List<LrinvoiceItems>();
            var listBillitems = new List<Billitems>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var invoiceitem in items.GroupBy(e => new { e.lrinvoiceid, e.TaxPercentage }))
            {
                var taxList = invitems.FindAll(e => e.invoiceid == invoiceitem.Key.lrinvoiceid && e.percentage == invoiceitem.Key.TaxPercentage);
                var alltaxList = invitems.FindAll(e => e.invoiceid == invoiceitem.Key.lrinvoiceid);
                if (invitems.Count(e => e.invoiceid == invoiceitem.Key.lrinvoiceid) != alltaxList.Count() || taxList.Count != 1)
                {
                    if (alltaxList.Count > 0)
                        sb.AppendFormat("update lrinvoicetax set isactive=false where id in (" + string.Join<long>(",", alltaxList.ConvertAll(e => e.lrtaxid)) + ");\r\n");

                    var lrinvoicetax = new InfyPOS.Models.Lrinvoicetax()
                    {
                        isactive = true,
                        version = 1,
                        createdby = 0,
                        modifiedby = 0,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        isinterstatesale = invoiceitem.First().isinterstatesale,
                        percentage = invoiceitem.First().TaxPercentage,
                        taxid = invoiceitem.First().taxid,
                        type = 0,
                        discount = invoiceitem.Sum(e => e.purchasediscount),
                        discountpercentage = invoiceitem.Average(e => e.purchasediscount == 0 ? 0 : Math.Round(Math.Abs(e.purchasediscount) / e.buyingqty / e.buyingprice * 100, 2)),
                        amounton = invoiceitem.Sum(e => e.amount),
                        taxamount = invoiceitem.Sum(e => e.purchasetax),
                        netamount = invoiceitem.Sum(e => e.amount + e.purchasetax)
                    };
                    listLrinvoicetax.Add(lrinvoicetax);
                }
                else
                {
                    if (invoiceitem.Sum(e => e.purchasetax) != taxList.First().lrtaxamount ||
                        invoiceitem.First().TaxPercentage != taxList.First().percentage)
                    {
                        var lrinvoicetax = new InfyPOS.Models.Lrinvoicetax()
                        {
                            isactive = true,
                            version = 1,
                            createdby = 0,
                            modifiedby = 0,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            isinterstatesale = invoiceitem.First().isinterstatesale,
                            percentage = invoiceitem.First().TaxPercentage,
                            taxid = invoiceitem.First().taxid,
                            type = 0,
                            id = taxList.First().lrtaxid,
                            invoiceid = taxList.First().invoiceid,
                            discount = invoiceitem.Sum(e => e.purchasediscount),
                            discountpercentage = invoiceitem.Average(e => e.purchasediscount == 0 ? 0 : Math.Round(Math.Abs(e.purchasediscount) / e.buyingqty / e.buyingprice * 100, 2)),
                            amounton = invoiceitem.Sum(e => e.amount),
                            taxamount = invoiceitem.Sum(e => e.purchasetax),
                            netamount = invoiceitem.Sum(e => e.amount + e.purchasetax)
                        };
                        listLrinvoicetax.Add(lrinvoicetax);
                    }
                }

                foreach (var lrinvoiceitem in invoiceitem)
                {

                    LrinvoiceItems lrinvoiceItems = new LrinvoiceItems()
                    {
                        id = lrinvoiceitem.id,
                        ptaxid = lrinvoiceitem.ptaxid,
                        taxid = lrinvoiceitem.taxid,
                        purchasetaxpercentage = lrinvoiceitem.TaxPercentage,
                        buyingprice = lrinvoiceitem.buyingprice,
                        purchasetax = lrinvoiceitem.purchasetax,
                        amount = lrinvoiceitem.amount
                    };
                    listLrinvoiceItems.Add(lrinvoiceItems);
                }
            }

            query = @"select billitems.id,billitems.billid,billitems.rate,billitems.tax,billitems.itemtax,
billitems.salerate,billitems.qty,billitems.saleamount,billitems.price,billitems.schemediscount,billitems.additionaldiscount,
billitems.receivable,products.salestaxid taxid,taxpercentage
            from billitems
            inner join bill on bill.id = billitems.billid
            inner join products on products.id = productid
            where billdate>=@fromdate and billdate<@todate";

            List<TaxSaleItem> sitems = new List<TaxSaleItem>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    TaxSaleItem taxChangeItem = new TaxSaleItem();
                    taxChangeItem.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    taxChangeItem.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                    taxChangeItem.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                    taxChangeItem.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                    taxChangeItem.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                    taxChangeItem.itemtax = reader["itemtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemtax"]);
                    taxChangeItem.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    taxChangeItem.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                    taxChangeItem.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    taxChangeItem.saleamount = reader["saleamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleamount"]);
                    taxChangeItem.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                    taxChangeItem.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                    taxChangeItem.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                    taxChangeItem.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                    sitems.Add(taxChangeItem);
                }
            }

            foreach (var taxChangeItem in sitems)
            {
                salestax = alltaxs.Find(e => e.id == taxChangeItem.taxid);
                var taxinfo = salestax.taxsplit.Find(taxChangeItem.price);
                taxChangeItem.taxid = salestax.id;
                var price = taxChangeItem.receivable / taxChangeItem.qty;

                taxChangeItem.rate = Math.Round(price / (100 + taxinfo.taxpercent) * 100, 2);
                taxChangeItem.saleamount = taxChangeItem.rate * taxChangeItem.qty;
                taxChangeItem.itemtax = Math.Round(price / (100 + taxinfo.taxpercent) * taxinfo.taxpercent, 2);
                if (taxChangeItem.tax != Math.Round(price / (100 + taxinfo.taxpercent) * taxinfo.taxpercent * taxChangeItem.qty, 2))
                {
                    taxChangeItem.tax = Math.Round(price / (100 + taxinfo.taxpercent) * taxinfo.taxpercent * taxChangeItem.qty, 2);
                    taxChangeItem.changed = true;
                }
                taxChangeItem.taxid = salestax.id;
                taxChangeItem.taxpercentage = taxinfo.taxpercent;
                var billitem = new Billitems()
                {
                    id = taxChangeItem.id,
                    rate = taxChangeItem.rate,
                    taxid = taxChangeItem.taxid,
                    taxpercentage = taxChangeItem.taxpercentage,
                    saleamount = taxChangeItem.saleamount,
                    itemtax = taxChangeItem.itemtax,
                    tax = taxChangeItem.tax
                };
                listBillitems.Add(billitem);
            }



            ExecuteQuery(sb.ToString(), null);
            int count = 0;
            sb.Clear();

            foreach (var invoiceitem in items.GroupBy(e => new { e.lrinvoiceid }))
            {
                sb.AppendFormat("update Lrinvoice set taxamount={1},baseamount={2} where id ={0};\r\n",
                    invoiceitem.Key.lrinvoiceid, invoiceitem.Sum(e => e.purchasetax), invoiceitem.Sum(e => e.amount));
                count++;
                if (count >= 4000)
                {
                    ExecuteQuery(sb.ToString(), null);
                    count = 0;
                    sb.Clear();
                }
            }
            ExecuteQuery(sb.ToString(), null);
            count = 0;
            sb.Clear();

            foreach (var lrinvoiceitem in listLrinvoiceItems)
            {
                sb.AppendFormat("update LrinvoiceItems set purchasetaxpercentage={1},ptaxid={2},taxid={3}," +
                    "buyingprice={4},purchasetax={5},amount={6} where id ={0};\r\n",
                    lrinvoiceitem.id, lrinvoiceitem.purchasetaxpercentage, lrinvoiceitem.ptaxid, lrinvoiceitem.taxid,
                    lrinvoiceitem.buyingprice, lrinvoiceitem.purchasetax,
                    lrinvoiceitem.amount);
                count++;
                if (count >= 4000)
                {
                    ExecuteQuery(sb.ToString(), null);
                    count = 0;
                    sb.Clear();
                }
            }
            ExecuteQuery(sb.ToString(), null);
            count = 0;
            sb.Clear();
            foreach (var lrinvoicetax in listLrinvoicetax.FindAll(e => e.id > 0))
            {
                sb.AppendFormat("update lrinvoicetax set percentage={1},taxid={2},amounton={3},taxamount={4},netamount={5} where id ={0};\r\n",
                    lrinvoicetax.id, lrinvoicetax.percentage, lrinvoicetax.taxid, lrinvoicetax.amounton,
                    lrinvoicetax.taxamount, lrinvoicetax.netamount);
                count++;
                if (count >= 3000)
                {
                    ExecuteQuery(sb.ToString(), null);
                    count = 0;
                    sb.Clear();
                }
            }
            ExecuteQuery(sb.ToString(), null);
            count = 0;
            sb.Clear();
            LrinvoicetaxDataService lrinvoicetaxDataService = new LrinvoicetaxDataService() { Context = this.Context };
            lrinvoicetaxDataService.InsertAll(listLrinvoicetax.FindAll(e => e.id == 0));

            foreach (var bill in listBillitems.GroupBy(e => e.billid))
            {
                sb.AppendFormat("update bill set tax=" + bill.Sum(e => e.tax) + " where id = " + bill.Key + ";\r\n");
                count++;
                if (count >= 3000)
                {
                    ExecuteQuery(sb.ToString(), null);
                    count = 0;
                    sb.Clear();
                }

            }
            ExecuteQuery(sb.ToString(), null);
            count = 0;
            sb.Clear();

            foreach (var billitem in listBillitems)
            {
                sb.AppendFormat("update billitems set rate={1},taxid={2},taxpercentage={3},saleamount={4},itemtax={5},tax={6} where id ={0};\r\n",
                    billitem.id, billitem.rate, billitem.taxid, billitem.taxpercentage, billitem.saleamount, billitem.itemtax, billitem.tax);
                count++;
                if (count >= 3000)
                {
                    ExecuteQuery(sb.ToString(), null);
                    count = 0;
                    sb.Clear();
                }

            }
            ExecuteQuery(sb.ToString(), null);
            count = 0;
            sb.Clear();

            return true;
        }

        public class TaxSaleItem
        {
            public decimal rate { get; set; }
            public decimal tax { get; set; }
            public decimal itemtax { get; set; }
            public decimal salerate { get; set; }
            public decimal qty { get; set; }
            public decimal saleamount { get; set; }
            public decimal receivable { get; set; }
            public long taxid { get; set; }
            public decimal taxpercentage { get; set; }
            public decimal price { get; set; }
            public decimal schemediscount { get; set; }
            public decimal additionaldiscount { get; set; }
            public long id { get; set; }
            public long billid { get; set; }
            public bool changed { get; set; }
        }
        public class TaxInvoiceItem
        {
            public long invoiceid { get; set; }
            public long taxid { get; set; }
            public long lrtaxid { get; set; }
            public decimal taxamount { get; set; }
            public decimal baseamount { get; set; }
            public decimal grossamount { get; set; }
            public decimal totalamount { get; set; }
            public decimal discount { get; set; }
            public decimal percentage { get; set; }
            public decimal amounton { get; set; }

            public decimal lrtaxamount { get; set; }
            public decimal netamount { get; set; }
        }
        public class TaxPurchaseItem
        {
            public long ptaxid { get; set; }
            public long taxid { get; set; }
            public decimal TaxPercentage { get; set; }
            public decimal buyingqty { get; set; }
            public decimal buyingprice { get; set; }
            public decimal basebuyingprice { get; set; }
            public decimal amount { get; set; }
            public decimal purchasetax { get; set; }
            public decimal purchasediscount { get; set; }
            public bool isinterstatesale { get; set; }
            public bool isinclusivetax { get; set; }
            public long id { get; set; }
            public long lrinvoiceid { get; set; }
            public bool changed { get; set; }
        }

    }

}

