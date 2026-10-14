using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using Newtonsoft.Json;

namespace InfyPOS.Data
{
    public class MfgsampleorderDataService : BaseData
    {
        const string SQL_Search = @"select id, entrydate, companyid, locationid, prddivisionid,designno, customerid,allowdirectorder, preparedby,productformid, approvedby, statusid, 
        hasapproved, config, designs, sizes, requestimageids, modifiedby, createdby, isactive, approvedon, modifiedon, createdon, hasdeactivated, deactivatedby, price,history,
        deactivateon, version, imageid, deactivatereason,bookorderno,bookingdate,samplename, entryno,agentid,productid,mfgdesignid from mfgsampleorder where id=@id order by id";

        const string SQL_Insert = @"INSERT INTO public.mfgsampleorder
        (   entrydate, companyid, locationid, prddivisionid,
            designno, customerid, allowdirectorder, productformid,
            approvedby, statusid, hasapproved, config, designs, sizes, 
            requestimageids, modifiedby, createdby, isactive, approvedon, 
            modifiedon, createdon, hasdeactivated, deactivatedby, price, 
            history, deactivateon, version, imageid, deactivatereason,
            bookorderno, bookingdate, samplename, entryno, agentid, productid )

           VALUES(@entrydate, @companyid, @locationid, @prddivisionid,
           @designno, @customerid, @allowdirectorder, @productformid,
           @approvedby, @statusid, @hasapproved, @config, @designs, @sizes,
           @requestimageids, @modifiedby, @createdby, @isactive, @approvedon,
           @modifiedon, @createdon, @hasdeactivated, @deactivatedby, @price,
           @history, @deactivateon, @version, @imageid, @deactivatereason,
           @bookorderno, @bookingdate, @samplename, @entryno, @agentid, @productid)        
            RETURNING id";

        const string SQL_Update = @"UPDATE public.mfgsampleorder SET entrydate=@entrydate, companyid=@companyid, locationid=@locationid, prddivisionid=@prddivisionid, customerid=@customerid, preparedby=@preparedby,productformid=@productformid, approvedby=@approvedby, statusid=@statusid, hasapproved=@hasapproved, config=@config, designs=@designs, sizes=@sizes, requestimageids=@requestimageids, modifiedby=@modifiedby, 
        approvedon=@approvedon, modifiedon=@modifiedon,productid=@productid,designno=@designno,allowdirectorder=@allowdirectorder,price=@price,history=@history,
        hasdeactivated=@hasdeactivated, deactivatedby=@deactivatedby, deactivateon=@deactivateon, version=@version+1, imageid=@imageid, deactivatereason=@deactivatereason, 
        bookorderno=@bookorderno,bookingdate=@bookingdate,samplename=@samplename,entryno=@entryno,agentid=@agentid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgsampleorder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<MfgSampleList> CustomerSamples(long customerid)
        {
            List<MfgSampleList> lst = new List<MfgSampleList>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(@"select mfgsampleorder.id,
                mfgsampleorder.samplename,mfgsampleorder.imageid,mfgsampleorder.mfgdesignid,
                itemlist->>'design_code' designcode,
                itemlist->>'design_name' designname,
                itemlist->>'requiredhours' requiredhours,
                itemlist->>'requiredcost' requiredcost,
                itemlist->>'requiredprice' requiredprice,
                itemlist->>'imageid' designimageid,
                itemlist->>'colourid' colourid,
                cast(sizelist->>'id' as bigint) sizeid,
                cast(sizelist->>'uid' as varchar) uid,
                cast(sizelist->>'itemid' as bigint) itemid,
                mfgsampleorder.productid,
                mfgsampleorder.prddivisionid,
                mfgsampleorder.designno
                from mfgsampleorder
                INNER JOIN LATERAL json_array_elements (case when 
                mfgsampleorder.designs is null or mfgsampleorder.designs::text ='null' then '[]'::json 
                else mfgsampleorder.designs :: json end) itemlist ON true
                INNER JOIN LATERAL json_array_elements (itemlist->'sizes') sizelist ON true
                where mfgsampleorder.isactive=true and mfgsampleorder.hasapproved = true and mfgsampleorder.customerid = @customerid", conn);

                if(customerid <=0)
                {
                    command.CommandText = command.CommandText.Replace("and mfgsampleorder.customerid = @customerid", "and mfgsampleorder.allowdirectorder=true");
                }

                command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = customerid;
                List<long> sizeids = new List<long>();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        long prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                        long colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        long productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        long mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        long itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        
                        long sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        var imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        var designno = reader["designno"] == DBNull.Value ? "" : reader["designno"].ToString();


                        decimal requiredhours = reader["requiredhours"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requiredhours"]);
                        decimal requiredcost = reader["requiredcost"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requiredcost"]);
                        decimal requiredprice = reader["requiredprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["requiredprice"]);
                        var designimageid = reader["designimageid"] == DBNull.Value ? "" : reader["designimageid"].ToString();
                        var designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        var designcode = reader["designcode"] == DBNull.Value ? "" : reader["designcode"].ToString();
                        var samplename = reader["samplename"] == DBNull.Value ? "" : reader["samplename"].ToString();
                        var uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        if (!sizeids.Contains(sizeid))
                            sizeids.Add(sizeid);

                        if (!lst.Exists(x => x.id == id))
                        {
                            lst.Add(new MfgSampleList()
                            {
                                id = id,
                                samplename = samplename,
                                name = designno+"("+samplename+")",
                                mfgid = mfgdesignid,
                                imageid = imageid,
                                prdid = productid,
                                designno = designno,
                                prddivid = prddivisionid
                            });
                        }
                        if (!lst.Find(x => x.id == id).designs.Exists(y => y.name == designname))
                        {
                            lst.Find(x => x.id == id).designs.Add(new MfgSampleList.design()
                            {
                                name = designname,
                                code = designcode,
                                colourid = colourid,
                                imageid = designimageid,
                                requiredhours= requiredhours,
                                requiredcost = requiredcost,
                                requiredprice = requiredprice,
                            });
                        }
                        if (!string.IsNullOrEmpty(uid))
                        {
                            if (!lst.Find(x => x.id == id).designs.Find(y => y.name == designname).sizes.Exists(z => z.id == sizeid))
                            {
                                lst.Find(x => x.id == id).designs.Find(y => y.name == designname).sizes.Add(new MfgSampleList.size()
                                {
                                    id = sizeid,
                                    uid = uid,
                                    itemid = itemid
                                });
                            }
                        }
                    }
                }

                ReferencelistDataService reflist = new ReferencelistDataService() { Context = this.Context };
                var sizenames = reflist.SearchList(new ReferencelistCreteria() { ids = sizeids });
                SortedDictionary<long, string> sizenamelist = new SortedDictionary<long, string>();
                sizenames.ForEach(x => sizenamelist.Add(x.id, x.name));

                lst.ForEach(x =>
                {
                    int designid = 1;
                    x.designs.ForEach(y =>
                    {
                        y.id = designid++;
                        y.sizes.ForEach(z =>
                        {
                            z.name = sizenamelist.ContainsKey(z.id) ? sizenamelist[z.id] : "";
                        });
                    });
                });
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, customerid);
            }
            return lst;
        }

        public List<Mfgsampleorder> SearchSummary(MfgsampleorderCreteria _mfgsampleorder)
        {
            List<Mfgsampleorder> lst = new List<Mfgsampleorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_mfgsampleorder.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgsampleorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if(_mfgsampleorder.addedfrom > DateTime.MinValue)
                    {
                        qb.AddParameter("entrydate", "fromdate", _mfgsampleorder.addedfrom.Date,">=");
                        qb.AddParameter("entrydate", "todate", _mfgsampleorder.addedto.Date.AddDays(1),"<");
                    }
                    if(_mfgsampleorder.statusid>0)
                    {
                        qb.AddParameter("statusid",_mfgsampleorder.statusid, "=");
                    }else if(_mfgsampleorder.statusid<0)
                    {
                        switch(_mfgsampleorder.statusid)
                        {
                            case -1:
                                qb.AddParameter("hasapproved",true, "=");
                                break;
                            case -2:
                                qb.AddParameter("hasdeactivated", true, "=");
                                break;
                            case -3:
                                qb.AddParameter("hasapproved", false, "=");
                                break;
                        }
                    }

                    qb.AddParameter("isactive", true, "=");
                    qb.SortField = "id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgsampleorder omfgsampleorder = new Mfgsampleorder();
                        omfgsampleorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgsampleorder.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        omfgsampleorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgsampleorder.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        omfgsampleorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfgsampleorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgsampleorder.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                        omfgsampleorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        omfgsampleorder.allowdirectorder = reader["allowdirectorder"] == DBNull.Value ? false : Convert.ToBoolean(reader["allowdirectorder"]);
                        omfgsampleorder.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        omfgsampleorder.productformid = reader["productformid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productformid"]);
                        omfgsampleorder.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        omfgsampleorder.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["statusid"]);
                        omfgsampleorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        omfgsampleorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        
                        omfgsampleorder.hasapproved = reader["hasapproved"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasapproved"]);
                        omfgsampleorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgsampleorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgsampleorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgsampleorder.approvedon = reader["approvedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["approvedon"]);
                        omfgsampleorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgsampleorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgsampleorder.hasdeactivated = reader["hasdeactivated"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasdeactivated"]);
                        omfgsampleorder.deactivatedby = reader["deactivatedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deactivatedby"]);
                        omfgsampleorder.deactivateon = reader["deactivateon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deactivateon"]);
                        omfgsampleorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgsampleorder.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        omfgsampleorder.deactivatereason = reader["deactivatereason"] == DBNull.Value ? "" : reader["deactivatereason"].ToString();
                        omfgsampleorder.designno = reader["designno"] == DBNull.Value ? "" : reader["designno"].ToString();
                        omfgsampleorder.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        omfgsampleorder.bookorderno = reader["bookorderno"] == DBNull.Value ? "" : reader["bookorderno"].ToString();
                        omfgsampleorder.samplename = reader["samplename"] == DBNull.Value ? "" : reader["samplename"].ToString();
                        omfgsampleorder.bookingdate = reader["bookingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["bookingdate"]);
                        lst.Add(omfgsampleorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgsampleorder);
            }
            return lst;
        }

        public MfgSampleDesign DesignInfo(long id)
        {
            var response = Search(new MfgsampleorderCreteria()
            {
                id = id
            }).First();

            if(string.IsNullOrEmpty(response.designno))
            {
                throw new Exception("Update the design number before generating UAN");
            }

            MfgSampleDesign design = JsonConvert.DeserializeObject<MfgSampleDesign>(JsonConvert.SerializeObject(response));
            Mfgdesign mfgdesign = new Mfgdesign();
            var sequencedesigncode = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.designcode", 0);
            var sequencedesignitemcode = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.designitemcode", 0);
            var sequenceuid = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.uid", 0);
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var productdivision = referencelistDataService.Search(new ReferencelistCreteria() { id = response.prddivisionid }).First();
            string message = "";
            if (response.mfgdesignid == 0)
            {

                mfgdesign.productid = productdivision.productgroup;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    mfgdesign.code = string.IsNullOrEmpty(response.designno) ? sequencedesigncode.GetNextNumber(Context, conn, transaction) : response.designno;
                    mfgdesign.sizegroup = new Mfgdesign.Sizegroup() { items = new List<Mfgdesign.Sizegroup.Item>() };
                    if (response.productid > 0)
                        mfgdesign.productid = response.productid;
                    if (mfgdesignDataService.Search(new MfgdesignCreteria() { code = mfgdesign.code}).Count()>0)
                    {
                        throw new Exception(mfgdesign.code + " already exist on the SKU design, please remove or rename the existing");
                    }

                    List<Mfgsampleorder.SampleDesign.Size> uids = new List<Mfgsampleorder.SampleDesign.Size>();
                    foreach (var designitem in response.Designs)
                    {
                        if (response.config == null) response.config = new Mfgsampleorder.SampleConfig();
                        response.config.lastdesignid++;
                        designitem.sizes = new List<Mfgsampleorder.SampleDesign.Size>();
                        designitem.design_code = mfgdesign.code + "/" + (response.config.lastdesignid.ToString("F0"));
                            //sequencedesignitemcode.GetNextNumber(Context, conn, transaction);
                        foreach (var size in response.Sizes)
                        {
                            Mfgdesign.Sizegroup.Item item = new Mfgdesign.Sizegroup.Item();
                            item.pname = designitem.design_name;
                            item.name = designitem.design_name;
                            item.code = designitem.design_code;
                            item.designitemcode = designitem.design_code;
                            if (designitem.items.Count > 0)
                                item.colourid = designitem.items.First().colourid;
                            else
                                item.colourid = designitem.colourid;
                            item.sizeid = size.id;
                            item.productid = mfgdesign.productid;
                            item.uid = mfgdesign.code + '-'+ (response.config.lastdesignid.ToString("F0")) + "-" + (designitem.sizes.Count+1).ToString("F0");
                            mfgdesign.sizegroup.items.Add(item);
                            designitem.sizes.Add(new Mfgsampleorder.SampleDesign.Size()
                            {
                                id = item.sizeid,
                                uid = item.uid
                            });
                        }
                        uids.AddRange(designitem.sizes);
                    }
                    mfgdesignDataService.Insert(transaction,conn, mfgdesign);
                    mfgdesign.sizegroup.items.ForEach(x =>
                    {
                        uids.Find(y => y.uid == x.uid).itemid = x.itemid;
                    });
                    NpgsqlCommand command = new NpgsqlCommand("update mfgsampleorder set designs=@designs,config=@config,mfgdesignid=@mfgdesignid where id=@id", conn, transaction);
                    command.Parameters.Add("designs", NpgsqlTypes.NpgsqlDbType.Json).Value = response.designs_json;
                    command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = response.config_json;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = response.id;
                    command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgdesign.id;
                    int successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                    message = "New SKU Design created - " + mfgdesign.code;
                }
            }
            else
            {
                bool updatedesign = false;
                mfgdesign = mfgdesignDataService.Search(new MfgdesignCreteria() { id = response.mfgdesignid, includedetail = true }).First();
                mfgdesign.productid = productdivision.productgroup;
                if (response.productid > 0)
                    mfgdesign.productid = response.productid;
                
                int newdesigns = 0;
                int newuid = 0;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    List<Mfgsampleorder.SampleDesign.Size> uids = new List<Mfgsampleorder.SampleDesign.Size>();
                    foreach (var designitem in response.Designs)
                    {
                        if (designitem.sizes == null)
                            designitem.sizes = new List<Mfgsampleorder.SampleDesign.Size>();

                        if (string.IsNullOrEmpty(designitem.design_code))
                        {
                            newdesigns++;
                            response.config.lastdesignid++;
                            designitem.design_code = mfgdesign.code + "/" + (response.config.lastdesignid.ToString("F0"));
                            foreach (var size in response.Sizes)
                            {
                                newuid++;
                                Mfgdesign.Sizegroup.Item item = new Mfgdesign.Sizegroup.Item();
                                item.name = designitem.design_name;
                                item.pname = designitem.design_name;
	                            if (designitem.items.Count > 0)								
                                	item.colourid = designitem.items.First().colourid;
                                item.designitemcode = designitem.design_code;
                                item.code = designitem.design_code;
                                item.sizeid = size.id;
                                item.productid = mfgdesign.productid;
                                item.uid = mfgdesign.code + '-' + (response.config.lastdesignid.ToString("F0")) + "-" + (designitem.sizes.Count + 1).ToString("F0");
                                mfgdesign.sizegroup.items.Add(item);
                                designitem.sizes.Add(new Mfgsampleorder.SampleDesign.Size()
                                {
                                    id = item.sizeid,
                                    uid = item.uid
                                });
                            }
                            updatedesign = true;
                        }
                        else
                        {
                            if(mfgdesign.sizegroup.items.Exists(x => !response.Sizes.Exists(y => y.id == x.sizeid)))
                            {
                                updatedesign = true;
                                mfgdesign.sizegroup.items.RemoveAll(x => !response.Sizes.Exists(y => y.id == x.sizeid));
                            }
                            var sizeitems = mfgdesign.sizegroup.items.FindAll(x => x.code == designitem.design_code);
                            if (response.Sizes.Exists(x => !mfgdesign.sizegroup.items.Exists(y => y.sizeid == x.id)))
                            {
                                updatedesign = true;
                            }
                            int sizeid = 1;
                            var designid = designitem.design_code.Split('/').Last();
                            foreach (var size in response.Sizes)
                            {
                                if (sizeitems.Exists(x => x.sizeid == size.id))
                                {
                                    if(!designitem.sizes.Exists(x=>x.id == size.id))
                                    {
                                        var itm = sizeitems.Find(x => x.sizeid == size.id);
                                        designitem.sizes.Add(new Mfgsampleorder.SampleDesign.Size()
                                        {
                                            id = itm.sizeid,
                                            uid = itm.uid
                                        });
                                    }
                                    updatedesign = true;
                                    continue;
                                }
                                newuid++;
                                updatedesign = true;
                                Mfgdesign.Sizegroup.Item item = new Mfgdesign.Sizegroup.Item();
                                item.pname = designitem.design_name;
                                item.designitemcode = designitem.design_code;
                                item.code = designitem.design_code;
                                item.colourid = designitem.items.First().colourid;
                                item.sizeid = size.id;
                                item.productid = mfgdesign.productid;
                                var uid = mfgdesign.code + '-' + designid + "-" + (sizeid++).ToString("F0");
                                while(designitem.sizes.Exists(x=>x.uid == uid))
                                {
                                    uid = mfgdesign.code + '-' + designid + "-" + (sizeid++).ToString("F0");
                                }
                                item.uid = uid;
                                mfgdesign.sizegroup.items.Add(item);
                                designitem.sizes.Add(new Mfgsampleorder.SampleDesign.Size()
                                {
                                    id = item.sizeid,
                                    uid = item.uid
                                });
                            }
                        }
                    }
                    if (updatedesign)
                    {
                        mfgdesignDataService.Update(transaction, conn, mfgdesign, null);
                        mfgdesign.sizegroup.items.ForEach(x =>
                        {
                            if(uids.Exists(y => y.uid == x.uid))
                                uids.Find(y => y.uid == x.uid).itemid = x.itemid;
                        });
                        NpgsqlCommand command = new NpgsqlCommand("update mfgsampleorder set designs=@designs,config=@config,mfgdesignid=@mfgdesignid where id=@id", conn, transaction);
                        command.Parameters.Add("designs", NpgsqlTypes.NpgsqlDbType.Json).Value = response.designs_json;
                        command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = response.config_json;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = response.id;
                        command.Parameters.Add("mfgdesignid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgdesign.id;
                        int successCount = command.ExecuteNonQuery();
                        message = "Exising SKU Design updated - " + mfgdesign.code+" Newly created designs - "+newdesigns+" Newly created UID - "+newuid;
                    }
                    else
                    {
                        message = "Existing design not altered, both are already sync";
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            design.message = message;
            design.mfgdesign = mfgdesign;
            return design;
        }

        public List<Mfgsampleorder> Search(MfgsampleorderCreteria _mfgsampleorder)
        {
            List<Mfgsampleorder> lst = new List<Mfgsampleorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgsampleorder.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgsampleorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.SortField = "id";

                    if (_mfgsampleorder.statusid > 0)
                        qb.AddParameter("statusid", _mfgsampleorder.statusid, "=");

                    if (_mfgsampleorder.customerid>0)
                        qb.AddParameter("customerid", _mfgsampleorder.customerid, "=");

                    if(_mfgsampleorder.fromdate.Date>DateTime.MinValue)
                        qb.AddParameter("entrydate","fromdate", _mfgsampleorder.fromdate, ">=");

                    if (_mfgsampleorder.todate.Date > DateTime.MinValue)
                        qb.AddParameter("entrydate", "todate", _mfgsampleorder.todate.AddDays(1), "<");

                    if (!string.IsNullOrEmpty(_mfgsampleorder.entryno))
                        qb.AddParameter("entryno", _mfgsampleorder.entryno, "=");

                    if (!string.IsNullOrEmpty(_mfgsampleorder.sono))
                        qb.Add2Query("mfgsampleorder.id in (select mfgsampleorderid from salesorder inner join salesorderitems on salesorderitems.salesorderid = salesorder.id where sono = @sono and salesorder.isactive=true limit 1)");

                    command = qb.GetCommand(conn);
                    if (!string.IsNullOrEmpty(_mfgsampleorder.sono))
                        command.Parameters.Add("sono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _mfgsampleorder.sono;
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgsampleorder omfgsampleorder = new Mfgsampleorder();
                        omfgsampleorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgsampleorder.mfgdesignid = reader["mfgdesignid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgdesignid"]);
                        omfgsampleorder.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgsampleorder.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        omfgsampleorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfgsampleorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgsampleorder.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                        omfgsampleorder.designno = reader["designno"] == DBNull.Value ? "" : reader["designno"].ToString();
                        omfgsampleorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        omfgsampleorder.allowdirectorder = reader["allowdirectorder"] == DBNull.Value ? false : Convert.ToBoolean(reader["allowdirectorder"]);
                        omfgsampleorder.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        omfgsampleorder.productformid = reader["productformid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productformid"]);
                        omfgsampleorder.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        omfgsampleorder.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["statusid"]);
                        omfgsampleorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        omfgsampleorder.hasapproved = reader["hasapproved"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasapproved"]);
                        omfgsampleorder.config_json = reader["config"] == DBNull.Value ? null : reader["config"].ToString();
                        omfgsampleorder.designs_json = reader["designs"] == DBNull.Value ? null : reader["designs"].ToString();
                        omfgsampleorder.sizes_json = reader["sizes"] == DBNull.Value ? null : reader["sizes"].ToString();
                        omfgsampleorder.history_json = reader["history"] == DBNull.Value ? "" : reader["history"].ToString();
                        omfgsampleorder.requestimageids_json = reader["requestimageids"] == DBNull.Value ? null : reader["requestimageids"].ToString();
                        omfgsampleorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgsampleorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgsampleorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        omfgsampleorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgsampleorder.approvedon = reader["approvedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["approvedon"]);
                        omfgsampleorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgsampleorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgsampleorder.hasdeactivated = reader["hasdeactivated"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasdeactivated"]);
                        omfgsampleorder.deactivatedby = reader["deactivatedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deactivatedby"]);
                        omfgsampleorder.deactivateon = reader["deactivateon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deactivateon"]);
                        omfgsampleorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgsampleorder.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        omfgsampleorder.deactivatereason = reader["deactivatereason"] == DBNull.Value ? "" : reader["deactivatereason"].ToString();
                        omfgsampleorder.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();

                        omfgsampleorder.bookorderno = reader["bookorderno"] == DBNull.Value ? "" : reader["bookorderno"].ToString();
                        omfgsampleorder.samplename = reader["samplename"] == DBNull.Value ? "" : reader["samplename"].ToString();
                        omfgsampleorder.bookingdate = reader["bookingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["bookingdate"]);
                        lst.Add(omfgsampleorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgsampleorder);
            }
            return lst;
        }

        public Mfgsampleorder Insert(Mfgsampleorder oMfgsampleorder)
        {

            NpgsqlTransaction transaction = null;
            try
            {

                
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgsampleorder.entryno", 0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ? oMfgsampleorder.companyid : 0);
                    oMfgsampleorder.entryno = sequence.GetNextNumber(Context, conn, transaction);
                    Insert(transaction, conn, oMfgsampleorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgsampleorder);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oMfgsampleorder;
        }

        public Mfgsampleorder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgsampleorder oMfgsampleorder)
        {

            oMfgsampleorder.Designs.ForEach(x =>
            {
                if(x.items.Exists(y=>y.colourid>0))
                {
                    x.colourid = x.items.Find(y => y.colourid > 0).colourid;
                }
            });

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.entrydate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.companyid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.productid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.locationid;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.prddivisionid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.customerid;
            command.Parameters.Add("allowdirectorder", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.allowdirectorder;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.preparedby;
            command.Parameters.Add("productformid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.productformid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.approvedby;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.statusid;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgsampleorder.price;
            command.Parameters.Add("hasapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.hasapproved;
            command.Parameters.Add("designno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oMfgsampleorder.designno) ? "" : oMfgsampleorder.designno;
            command.Parameters.Add("history", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.history_json == null ? "null" : oMfgsampleorder.history_json;
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.config_json == null ? "null" : oMfgsampleorder.config_json;
            command.Parameters.Add("designs", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.designs_json == null ? "null" : oMfgsampleorder.designs_json;
            command.Parameters.Add("sizes", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.sizes_json == null ? "null" : oMfgsampleorder.sizes_json;
            command.Parameters.Add("requestimageids", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.requestimageids_json == null ? "null" : oMfgsampleorder.requestimageids_json;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.isactive;
            command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.approvedon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.createdon;
            command.Parameters.Add("hasdeactivated", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.hasdeactivated;
            command.Parameters.Add("deactivatedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.deactivatedby;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.agentid;
            command.Parameters.Add("deactivateon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.deactivateon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgsampleorder.version;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.imageid == null ? "" : oMfgsampleorder.imageid;
            command.Parameters.Add("deactivatereason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.deactivatereason == null ? "" : oMfgsampleorder.deactivatereason;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.entryno == null ? "" : oMfgsampleorder.entryno;

            command.Parameters.Add("bookorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.bookorderno == null ? "" : oMfgsampleorder.bookorderno;
            command.Parameters.Add("samplename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.samplename == null ? "" : oMfgsampleorder.samplename;
            command.Parameters.Add("bookingdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.bookingdate;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgsampleorder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgsampleorder;
        }

        public Mfgsampleorder Update(Mfgsampleorder oMfgsampleorder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgsampleorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgsampleorder);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oMfgsampleorder;
        }

        public Mfgsampleorder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgsampleorder oMfgsampleorder)
        {
            oMfgsampleorder.Designs.ForEach(x =>
            {
                if (x.items.Exists(y => y.colourid > 0))
                {
                    x.colourid = x.items.Find(y => y.colourid > 0).colourid;
                }
            });

            oMfgsampleorder.history.FindAll(x => x.actionby == 0).ForEach(x =>
            {
                x.actionon = DateTime.Now;
                x.actionby = Context.UserID;
            });

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.id;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.entrydate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.locationid;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.prddivisionid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.customerid;
            command.Parameters.Add("allowdirectorder", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.allowdirectorder;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.preparedby;
            command.Parameters.Add("productformid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.productformid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.approvedby;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.statusid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.productid;
            command.Parameters.Add("hasapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.hasapproved;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgsampleorder.price;
            command.Parameters.Add("designno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oMfgsampleorder.designno) ? "" : oMfgsampleorder.designno;
            command.Parameters.Add("config", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.config_json == null ? "null" : oMfgsampleorder.config_json;
            command.Parameters.Add("history", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.history_json == null ? "null" : oMfgsampleorder.history_json;
            command.Parameters.Add("designs", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.designs_json == null ? "null" : oMfgsampleorder.designs_json;
            command.Parameters.Add("sizes", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.sizes_json == null ? "null" : oMfgsampleorder.sizes_json;
            command.Parameters.Add("requestimageids", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgsampleorder.requestimageids_json == null ? "null" : oMfgsampleorder.requestimageids_json;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.isactive;
            command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.approvedon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.modifiedon;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.createdon;
            command.Parameters.Add("hasdeactivated", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.hasdeactivated;
            command.Parameters.Add("deactivatedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.deactivatedby;
            command.Parameters.Add("deactivateon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.deactivateon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgsampleorder.version;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.imageid == null ? "" : oMfgsampleorder.imageid;
            command.Parameters.Add("deactivatereason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.deactivatereason == null ? "" : oMfgsampleorder.deactivatereason;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.entryno == null ? "" : oMfgsampleorder.entryno;
            command.Parameters.Add("bookorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.bookorderno == null ? "" : oMfgsampleorder.bookorderno;
            command.Parameters.Add("samplename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgsampleorder.samplename == null ? "" : oMfgsampleorder.samplename;
            command.Parameters.Add("bookingdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgsampleorder.bookingdate;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.agentid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgsampleorder.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgsampleorder;
        }


        public bool Delete(Mfgsampleorder oMfgsampleorder)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgsampleorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgsampleorder);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgsampleorder oMfgsampleorder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgsampleorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oMfgsampleorder.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgsampleorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgsampleorder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal MfgStatusChange ChangeStatus(MfgStatusChange item)
        {
            if(item.status == "status")
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(
                        String.Format("Update mfgsampleorder set statusid=@statusid,modifiedon=@modifiedon,modifiedby=@modifiedby where id in ({0})",
                        String.Join(",",item.ids)) , conn);
                    command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.statusid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
                item.status = "Selected sample orders status changed";
            }
            else if(item.status == "approve")
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(
                        String.Format("Update mfgsampleorder set approvedby=@approvedby,approvedon=@approvedon,hasapproved=true,modifiedon=@modifiedon,modifiedby=@modifiedby where id in ({0})",
                        String.Join(",", item.ids)), conn);
                    command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.statusid;
                    command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.employeeid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTimeNow;
                    command.Parameters.Add("approvedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = item.actionon;

                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
                item.status = "Selected sample orders are approved";
            }
            else if (item.status == "deactivate")
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(
                        String.Format("Update mfgsampleorder set dactivatedby=@dactivatedby,deactiveon=@deactiveon,hasdeactivated=true,modifiedon=@modifiedon,modifiedby=@modifiedby,deactivatereason=@deactivatereason where id in ({0})",
                        String.Join(",", item.ids)), conn);
                    command.Parameters.Add("dactivatedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.employeeid;
                    command.Parameters.Add("deactiveon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = item.actionon;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTimeNow;
                    command.Parameters.Add("deactivatereason", NpgsqlTypes.NpgsqlDbType.Varchar).Value =string.IsNullOrEmpty(item.reason)?"": item.reason;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
                item.status = "Selected sample orders are deacivated";
            }
            else if (item.status == "reject")
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(
                        String.Format("Update mfgsampleorder set approvedby=0,approvedon=null,hasapproved=false,modifiedon=@modifiedon,modifiedby=@modifiedby where id in ({0})",
                        String.Join(",", item.ids)), conn);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
                item.status = "Selected sample orders are rejected";
            }
            return item;
        }

        internal MfgsampleorderPrint Print(MfgsampleorderCreteria mfgsampleorderCreteria)
        {
            var results = Search(mfgsampleorderCreteria);
            if (results.Count == 0) throw new Exception("Can't find the sample order");
            MfgsampleorderPrint print = JsonConvert.DeserializeObject<MfgsampleorderPrint>(JsonConvert.SerializeObject(results[0]));

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            AgentDataService agentDataservice = new AgentDataService() { Context = this.Context };
            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            print.company = companyDataService.Search(new CompanyCreteria()
            {
                id = print.companyid
            }).First();
            print.customer = customerDataService.Search(new CustomerCreteria() { id = print.customerid}).First();
            print.location = new CompanyLocation(referencelistDataService.Search(new ReferencelistCreteria() { id = print.locationid }).First());
            print.product = productsDataService.Search(new ProductsCreteria() { id = print.productid}).First();
            print.productdivision = referencelistDataService.Search(new ReferencelistCreteria() { id = print.prddivisionid }).First();
            print.agent = agentDataservice.Search(new AgentCreteria() { id = print.agentid }).First();
            print.employ = employeeDataService.Search(new EmployeeCreteria() { id = print.preparedby }).First();
            print.designer = employeeDataService.Search(new EmployeeCreteria() { id = print.config.designer }).First();
            List<long> refids = new List<long>();
            List<long> bomids = new List<long>();
            print.Designs.ForEach(x =>
            {
                refids.Add(x.colourid);
                x.items.ForEach(y =>
                {
                    refids.Add(y.colourid);
                    refids.Add(y.design_colour);
                    refids.Add(y.design_frontcolour);
                    refids.Add(y.design_backcolour);
                    bomids.Add(y.design_bom);
                    bomids.Add(y.design_shade);
                });
            });
            refids.Add(print.productformid);
            refids.Add(print.config.quality);
            refids.Add(print.config.machine);
            refids.AddRange(print.Sizes.ConvertAll(x => x.id));

            refids.RemoveAll(x => x == 0);
            
            bomids.RemoveAll(x => x == 0);
            List<ReferenceItem> reflist = null;
            if(refids.Count > 0)
                reflist = referencelistDataService.SearchList(new ReferencelistCreteria() { ids = refids });

            print.Sizes.ForEach(x => x.sizename = reflist.Find(y => y.id == x.id).name);

            if (print.productdivision != null)
                print.labelconfig = print.productdivision.additionalinfo.mfglabelconfig;

            if (print.productformid > 0 && reflist.Exists(x => x.id == print.productformid))
                print.productformname = reflist.Find(x => x.id == print.productformid).name;

            if (print.config.quality > 0 && reflist.Exists(x => x.id == print.config.quality))
                print.config.qualityname = reflist.Find(x => x.id == print.config.quality).name;

            if (print.config.machine > 0 && reflist.Exists(x => x.id == print.config.machine))
                print.config.machinename = reflist.Find(x => x.id == print.config.machine).name;

            List<BOMBase> bomlist = null;
            if(bomids.Count > 0)
                bomlist = bOMMasterDataService.SearchList(new BOMMasterCreteria() { ids = bomids });

            
            print.Designs.ForEach(x =>
            {
                if (reflist != null && reflist.Exists(z => z.id == x.colourid))
                    x.colourname = reflist.Find(z => z.id == x.colourid).name;

                if(x.items.All(y=>y.sno==0))
                {
                    int sno = 1;
                    x.items.ForEach(y =>
                    {
                        y.sno = sno++;
                    });
                }
                x.items = x.items.OrderBy(y => y.sno).ToList();
                x.items.ForEach(y =>
                {
                    if (reflist != null && reflist.Exists(z => z.id == y.colourid))
                        y.colourname = reflist.Find(z => z.id == y.colourid).name;

                    if (reflist != null && reflist.Exists(z => z.id == y.design_colour))
                        y.design_colourname = reflist.Find(z => z.id == y.design_colour).name;

                    if (reflist != null && reflist.Exists(z => z.id == y.design_frontcolour))
                        y.design_frontcolourname = reflist.Find(z => z.id == y.design_frontcolour).name;

                    if (reflist != null && bomlist.Exists(z => z.id == y.design_shade))
                        y.design_shadename = bomlist.Find(z => z.id == y.design_shade).name;

                    if (reflist != null && reflist.Exists(z => z.id == y.design_backcolour))
                        y.design_backcolourname = reflist.Find(z => z.id == y.design_backcolour).name;

                    if (bomlist != null && bomlist.Exists(z => z.id == y.design_bom))
                        y.design_bomname = bomlist.Find(z => z.id == y.design_bom).name;
                });
            });
            return print;
        }
    }

}
