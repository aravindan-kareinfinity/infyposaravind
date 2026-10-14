using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ComponentPODataService : SAASData
    {
        const string SQL_Search = @"select id, podate, expectedate, pono, supplierid,dsupplierid, preparedby,approvedby, isactive, 
        createdby, modifiedby, version,tolerance, createdon, modifiedon,companyid,deliverylocationid,formimageid,pobookno,remarks,pobookdate,freight from componentpo where id=@id";

        const string SQL_Insert = @"INSERT INTO public.ComponentPO(podate, expectedate, pono, supplierid,dsupplierid, isactive, createdby, modifiedby, version, createdon, modifiedon, approvedby,preparedby,tolerance,companyid,
        deliverylocationid,formimageid,pobookno,remarks,pobookdate,freight,hasapproved) 
        VALUES(@podate, @expectedate, @pono, @supplierid,@dsupplierid, @isactive, @createdby, @modifiedby, @version,  @createdon, @modifiedon, @approvedby,@preparedby,@tolerance,@companyid,
        @deliverylocationid,@formimageid,@pobookno,@remarks,@pobookdate,@freight,@hasapproved) RETURNING id";


        const string SQL_SearchPODetails = @"with allbomdetail
as
(
select id bomid,categoryid bomcategoryid,width,taxid,colourmode,printingname,name,cast(schemelist->>'id' as bigint) colourid,
schemelist->>'name' shade,supplierbrand,designno supplierdesign,categoryid from bommaster
                            LEFT JOIN LATERAL
                            json_array_elements (cast(bommaster.colourlist as json)) schemelist ON true
),allpoitemdetails as
(
select allbomdetail.name,allbomdetail.shade,referencelist.name colour,qty oqty,cqty,rqty,(qty-(rqty+cqty)) pqty,rate,printingname,
	amount,ComponentPOitems.width,Componentorderid,ComponentPOitems.id componentitemsid,
    ComponentPOitems.brandname supplierbrand,ComponentPOitems.designid supplierdesign,
    allbomdetail.bomcategoryid,allbomdetail.bomid from ComponentPOitems
	inner join allbomdetail on allbomdetail.bomid = ComponentPOitems.bomid and allbomdetail.colourid = ComponentPOitems.colourid
	left outer join referencelist on referencelist.id = ComponentPOitems.colourid
    WHERE ComponentPOitems.isactive=true
)

select allpoitemdetails.*,ComponentPO.pono,ComponentPO.podate,supplier.name suppliername,
referencelist.name cityname,pobookdate,pobookno,ComponentPO.remarks,ComponentPO.expectedate from allpoitemdetails
inner join ComponentPO on ComponentPO.id = Componentorderid
inner join supplier on supplier.id = ComponentPO.supplierid
inner join suppliercompany on suppliercompany.supplierid = ComponentPO.supplierid
left outer join referencelist on referencelist.id = suppliercompany.cityid";

        const string SQL_SearchPOSummary = @"with itemsummary as
(
	select Componentorderid, sum(qty) oqty, sum(cqty) cqty, sum(rqty) rqty, sum(qty-(rqty+cqty)) pqty from ComponentPOitems 
	where isactive = true group by Componentorderid
)
                                select id, podate, expectedate, pono, supplierid, preparedby,approvedby, isactive, 
                                createdby, modifiedby, version,tolerance, createdon, modifiedon,companyid,deliverylocationid,
                                formimageid,pobookno,ComponentPO.remarks,pobookdate,freight,ComponentPO.dsupplierid,itemsummary.*
                                from ComponentPO 
                                inner join itemsummary on itemsummary.Componentorderid = ComponentPO.id ";



        const string SQL_Update = @"UPDATE public.ComponentPO SET podate=@podate, expectedate=@expectedate, pono=@pono, supplierid=@supplierid,
                    dsupplierid=@dsupplierid,preparedby=@preparedby, approvedby=@approvedby,
                    modifiedby=@modifiedby,tolerance=@tolerance, version=@version+1,modifiedon=@modifiedon,companyid=@companyid,
                    deliverylocationid=@deliverylocationid,formimageid=@formimageid,freight=@freight,pobookno=@pobookno,remarks=@remarks,pobookdate=@pobookdate
                    where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.ComponentPO set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";



        public List<ComponentPOReportDetail> ReportDetail(ComponentPOCreteria _ComponentPO)
        {
            List<ComponentPOReportDetail> lst = new List<ComponentPOReportDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPODetails + " where ComponentPO.isactive=true ";

                if (_ComponentPO.bomid > 0)
                    qb.AddParameter("bomid", _ComponentPO.bomid, "=");

                if (_ComponentPO.categoryid > 0)
                    qb.AddParameter("bomcategoryid", _ComponentPO.categoryid, "=");

                if (!string.IsNullOrEmpty(_ComponentPO.pono))
                    qb.AddParameter("pono", _ComponentPO.pono, "=");

                if (_ComponentPO.companyid > 0)
                    qb.AddParameter("ComponentPO.companyid", _ComponentPO.companyid, "=");

                if (_ComponentPO.deliverylocationid > 0)
                    qb.AddParameter("ComponentPO.deliverylocationid", _ComponentPO.deliverylocationid, "=");

                if (_ComponentPO.supplierid > 0)
                    qb.AddParameter("ComponentPO.supplierid", _ComponentPO.supplierid, "=");

                if (_ComponentPO.fromdate != DateTime.MinValue)
                {
                    qb.AddParameter("ComponentPO.podate", "podatefrom", _ComponentPO.fromdate.Date, ">=");
                    qb.AddParameter("ComponentPO.podate", "podateto", _ComponentPO.todate.Date.AddDays(1), "<");
                }
                if (_ComponentPO.duefromdate != DateTime.MinValue)
                {
                    qb.AddParameter("ComponentPO.expectedate", "expectedatefrom", _ComponentPO.duefromdate.Date, ">=");
                    qb.AddParameter("ComponentPO.expectedate", "expectedateto", _ComponentPO.duetodate.Date.AddDays(1), "<");
                }

                if (_ComponentPO.statusid == 1)
                    qb.Add2Query(" pqty>0 ");

                if (_ComponentPO.statusid == 2)
                    qb.Add2Query(" pqty<=0 ");

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComponentPOReportDetail oComponentPO = new ComponentPOReportDetail();
                        oComponentPO.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        oComponentPO.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        oComponentPO.pobookdate = reader["pobookdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["pobookdate"]);
                        oComponentPO.duedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oComponentPO.id = reader["componentitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["componentitemsid"]);
                        oComponentPO.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["oqty"]);
                        oComponentPO.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        oComponentPO.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                        oComponentPO.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pqty"]);
                        oComponentPO.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rate"]);
                        oComponentPO.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["amount"]);
                        oComponentPO.design = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oComponentPO.name = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oComponentPO.shade = reader["shade"] == DBNull.Value ? "" : reader["shade"].ToString();
                        oComponentPO.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        oComponentPO.width = reader["width"] == DBNull.Value ? "" : reader["width"].ToString();
                        oComponentPO.supplierbrand = reader["supplierbrand"] == DBNull.Value ? "" : reader["supplierbrand"].ToString();
                        oComponentPO.supplierdesign = reader["supplierdesign"] == DBNull.Value ? "" : reader["supplierdesign"].ToString();
                        oComponentPO.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        oComponentPO.cityname = reader["cityname"] == DBNull.Value ? "" : reader["cityname"].ToString();
                        if (oComponentPO.pqty < 0) oComponentPO.pqty = 0;
                        lst.Add(oComponentPO);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ComponentPO);
            }
            return lst;
        }
        public List<ComponentPOReportSummary> ReportSummary(ComponentPOCreteria _ComponentPO)
        {
            List<ComponentPOReportSummary> lst = new List<ComponentPOReportSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPOSummary;
                qb.AddParameter("isactive", true, "=");

                if (!string.IsNullOrEmpty(_ComponentPO.pono))
                    qb.AddParameter("pono", _ComponentPO.pono, "=");

                if (_ComponentPO.companyid > 0)
                    qb.AddParameter("companyid", _ComponentPO.companyid, "=");

                if (_ComponentPO.deliverylocationid > 0)
                    qb.AddParameter("deliverylocationid", _ComponentPO.deliverylocationid, "=");

                if (_ComponentPO.supplierid > 0)
                    qb.AddParameter("supplierid", _ComponentPO.supplierid, "=");

                if (_ComponentPO.fromdate != DateTime.MinValue)
                {
                    qb.AddParameter("podate", "podatefrom", _ComponentPO.fromdate.Date, ">=");
                    qb.AddParameter("podate", "podateto", _ComponentPO.todate.Date.AddDays(1), "<");
                }
                if (_ComponentPO.duefromdate != DateTime.MinValue)
                {
                    qb.AddParameter("expectedate", "expectedatefrom", _ComponentPO.duefromdate.Date, ">=");
                    qb.AddParameter("expectedate", "expectedateto", _ComponentPO.duetodate.Date.AddDays(1), "<");
                }

                if (_ComponentPO.statusid == 1)
                    qb.Add2Query(" itemsummary.pqty>0 ");

                if (_ComponentPO.statusid == 2)
                    qb.Add2Query(" itemsummary.pqty<=0 ");

                if (_ComponentPO.supplierid > 0)
                {
                    qb.SortField = "ComponentPO.id desc";
                }
                qb.Limit = _ComponentPO.limit;
                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComponentPOReportSummary oComponentPO = new ComponentPOReportSummary();
                        oComponentPO.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oComponentPO.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        oComponentPO.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        oComponentPO.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oComponentPO.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        oComponentPO.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oComponentPO.dsupplierid = reader["dsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dsupplierid"]);
                        oComponentPO.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oComponentPO.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oComponentPO.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oComponentPO.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oComponentPO.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //oComponentPO.status = reader["status"] == DBNull.Value ? ComponentPO.POStatus.PENDING : (ComponentPO.POStatus)Convert.ToInt32(reader["status"]);
                        oComponentPO.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);

                        oComponentPO.formimageid = reader["formimageid"] == DBNull.Value ? "" : Convert.ToString(reader["formimageid"]);
                        oComponentPO.pobookno = reader["pobookno"] == DBNull.Value ? "" : Convert.ToString(reader["pobookno"]);
                        oComponentPO.remarks = reader["remarks"] == DBNull.Value ? "" : Convert.ToString(reader["remarks"]);
                        oComponentPO.pobookdate = reader["pobookdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["pobookdate"]);

                        oComponentPO.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oComponentPO.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        oComponentPO.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        oComponentPO.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        oComponentPO.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);

                        oComponentPO.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["oqty"]);
                        oComponentPO.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        oComponentPO.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                        oComponentPO.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pqty"]);
                        if (oComponentPO.pqty < 0) oComponentPO.pqty = 0;
                        lst.Add(oComponentPO);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ComponentPO);
            }

            return lst;
        }

        public List<ComponentPO> Search(ComponentPOCreteria _ComponentPO)
        {
            List<ComponentPO> lst = new List<ComponentPO>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_ComponentPO.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _ComponentPO.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_ComponentPO.pono))
                        qb.AddParameter("pono", _ComponentPO.pono, "=");

                    if (_ComponentPO.companyid > 0)
                        qb.AddParameter("companyid", _ComponentPO.companyid, "=");

                    if (_ComponentPO.supplierid > 0)
                        qb.AddParameter("supplierid", _ComponentPO.supplierid, "=");

                    if (_ComponentPO.deliverylocationid > 0)
                        qb.AddParameter("deliverylocationid", _ComponentPO.deliverylocationid, "=");

                    if (_ComponentPO.statusid != -9)
                        qb.AddParameter("status", _ComponentPO.statusid, "=");

                    if (_ComponentPO.fromdate != DateTime.MinValue)
                    {
                        qb.AddParameter("podate", "podatefrom", _ComponentPO.fromdate.Date, ">=");
                        qb.AddParameter("podate", "podateto", _ComponentPO.todate.Date.AddDays(1), "<");
                    }
                    if (_ComponentPO.duefromdate != DateTime.MinValue)
                    {
                        qb.AddParameter("expectedate", "expectedatefrom", _ComponentPO.duefromdate.Date, ">=");
                        qb.AddParameter("expectedate", "expectedateto", _ComponentPO.duetodate.Date.AddDays(1), "<");
                    }

                    if (_ComponentPO.bomid > 0)
                    {
                        qb.AddInParamer("ComponentPO.id", "select componentorderid from ComponentPOitems where bomid=@bomid");
                    }

                    if (!string.IsNullOrEmpty(_ComponentPO.designid))
                    {
                        qb.AddInParamer("ComponentPO.id", "select componentorderid from ComponentPOitems where designid=@designid");
                    }

                    if (_ComponentPO.supplierid > 0)
                    {
                        qb.SortField = "ComponentPO.id desc";
                    }
                    qb.Limit = _ComponentPO.limit;
                    command = qb.GetCommand(conn);
                    command.Parameters.AddWithValue("@bomid", _ComponentPO.bomid);
                    if (!string.IsNullOrEmpty(_ComponentPO.designid))
                        command.Parameters.AddWithValue("@designid", _ComponentPO.designid);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComponentPO oComponentPO = new ComponentPO();
                        oComponentPO.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oComponentPO.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        oComponentPO.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        oComponentPO.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oComponentPO.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        oComponentPO.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oComponentPO.dsupplierid = reader["dsupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["dsupplierid"]);
                        oComponentPO.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oComponentPO.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oComponentPO.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oComponentPO.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oComponentPO.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //oComponentPO.status = reader["status"] == DBNull.Value ? ComponentPO.POStatus.PENDING : (ComponentPO.POStatus)Convert.ToInt32(reader["status"]);
                        oComponentPO.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);

                        oComponentPO.formimageid = reader["formimageid"] == DBNull.Value ? "" : Convert.ToString(reader["formimageid"]);
                        oComponentPO.pobookno = reader["pobookno"] == DBNull.Value ? "" : Convert.ToString(reader["pobookno"]);
                        oComponentPO.pobookdate = reader["pobookdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["pobookdate"]);
                        oComponentPO.remarks = reader["remarks"] == DBNull.Value ? "" : Convert.ToString(reader["remarks"]);

                        oComponentPO.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oComponentPO.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        oComponentPO.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        oComponentPO.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        oComponentPO.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);

                        lst.Add(oComponentPO);
                    }
                }
                if ((_ComponentPO.id > 0 && lst.Count == 1))
                {
                    ComponentPOitemsDataService subComponentPOitems = new ComponentPOitemsDataService() { Context = this.Context };
                    subComponentPOitems.Context = this.Context;
                    lst[0].ComponentPOitems = subComponentPOitems.SearchAll(lst.First().id);
                }
                else
                {
                    if (lst.Count > 0)
                    {
                        string postatussql = string.Format(@"
                    select sum(qty) qty,sum(rqty) rqty,sum(cqty) cqty,statusid,componentorderid from ComponentPOitems 
                    inner join componentpo on componentpo.id = componentorderid
                    where ComponentPOitems.isactive=true and componentorderid in ({0})
                    group by componentorderid,statusid", string.Join(",", lst.ConvertAll(x => x.id)));
                        using (var reader = ExecuteReader(postatussql, null, conn, null))
                        {
                            while (reader.Read())
                            {
                                var componentorderid = reader["componentorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["componentorderid"]);
                                var oqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["qty"]);
                                var cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                                var rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                                string stdisp = rqty == oqty ? "Delivered" : "New";
                                if (rqty == 0 && cqty == oqty)
                                {
                                    if (Convert.ToInt32(reader["statusid"]) == 4)
                                        stdisp = "Full Completion";
                                    else
                                        stdisp = "Full Cancel";
                                }
                                else if (rqty > 0 && cqty > 0)
                                {
                                    if (Convert.ToInt32(reader["statusid"]) == 4)
                                        stdisp = "Part Completion";
                                    else
                                        stdisp = "Part Cancel";
                                }
                                else if (rqty > 0)
                                {
                                    if (Convert.ToInt32(reader["statusid"]) == 4)
                                        stdisp = "Part Completion";
                                    else
                                        stdisp = "Part Delivered";
                                }
                                lst.Find(x => x.id == componentorderid).stdisp = stdisp;
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ComponentPO);
            }
            return lst;
        }

        public ComponentPO Insert(ComponentPO oComponentPO)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("componentpo.pono", 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oComponentPO.pono = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oComponentPO);
                    ComponentPOitemsDataService subComponentPOitems = new ComponentPOitemsDataService() { Context = this.Context };
                    subComponentPOitems.Context = this.Context;

                    foreach (var item in oComponentPO.ComponentPOitems)
                    {
                        if (item.bomid <= 0 && !string.IsNullOrEmpty(item.bomcode))
                        {
                            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
                            var bommasterlist = bOMMasterDataService.Search(new BOMMasterCreteria() { categoryid = item.bomcategoryid, name = item.bomcode });
                            if (bommasterlist != null && bommasterlist.Count > 0)
                            {
                                item.bomid = bommasterlist[0].id;
                            }
                            else
                            {
                                var itemsequence = SequenceManager.Instance(Context.OrganizationID).Get("bommaster.bommastercode", 0);
                                var bommastercode = itemsequence.GetNextNumber(Context, conn, transaction);
                                item.bomid = bOMMasterDataService.Insert(transaction, conn, new BOMMaster()
                                {
                                    categoryid = item.bomcategoryid,
                                    name = item.bomcode,
                                    printingname = item.bomcode,
                                    isactive = true,
                                    version = 1,
                                    createdby = Context.UserID,
                                    modifiedby = Context.UserID,
                                    width = item.width,
                                    createdon = DateTimeNow,
                                    modifiedon = DateTimeNow,
                                    colorid = item.colourid,
                                    coretypeid = item.coretypeid,
                                    taxid = item.taxid,
                                    uomid = item.uomid,
                                    bommastercode = bommastercode
                                }).id;
                            }
                        }
                        item.componentorderid = oComponentPO.id;
                        subComponentPOitems.Insert(transaction, conn, item);
                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bompo)
                    {
                        SupplierDataService supplierdata = new SupplierDataService() { Context = this.Context };
                        var supplier = supplierdata.Search(new SupplierCreteria() { id = oComponentPO.supplierid });
                        if (supplier != null && supplier.Count > 0)
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(
                                Context.OrganizationID,
                                Services.IntrayManager.EntryType.BomPO,
                                oComponentPO.id, string.Format("COMPONENT PURCHASE ORDER - FOR THE SUPPLIER {0} WITH {1} QTY.", supplier[0].name,
                                oComponentPO.ComponentPOitems.Sum(x => x.qty)), Context.UserID, 0, transaction, Context);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentPO);
            }
            return oComponentPO;
        }

        public List<ComponentPOItemDetail> SearchPO(ComponentPOCreteria ComponentPOCreteria)
        {
            /*      string SQL_SearchPO = @"select ComponentPOitems.id,ComponentPO.podate,ComponentPO.id,ComponentPO.pono,oqty,
                          oqty-(rqty+cqty) balance,designid,price,ComponentPOitems.imageid,ComponentPOitems.description,referencelist.name sizename,
                          tolerance from ComponentPO
                          inner join ComponentPOitems on ComponentPOitems.ComponentPOid = ComponentPO.id
                          left outer join items on items.id = itemid
                          left outer join referencelist on referencelist.id = items.sizeid
                          where ComponentPO.supplierid = @supplierid and ComponentPOitems.productid = @productid 
                          and ComponentPO.isactive = true and ComponentPOitems.isactive = true  
                          and ComponentPO.podate>=@fromdate and oqty-(rqty+cqty)>0";*/

            List<Bominvoiceitems.Poinfo> allPOInfo = new List<Bominvoiceitems.Poinfo>();
            BOMinvoice invoice = null;
            if (ComponentPOCreteria.invoiceid > 0)
            {
                BOMinvoiceDataService bOMinvoiceDataService = new BOMinvoiceDataService() { Context = this.Context };
                invoice = bOMinvoiceDataService.Search(new BOMinvoiceCreteria()
                {
                    id = ComponentPOCreteria.invoiceid
                }).First();
                invoice.bominvoiceitems.FindAll(x => x.poinfo != null && x.poinfo.Count > 0).ForEach(x => allPOInfo.AddRange(x.poinfo));
            }

            string SQL_SearchPO = @"select ComponentPOitems.id componentpoitemsid,ComponentPO.podate,
                            ComponentPO.id componentorderid,ComponentPO.pono,qty,
                            rqty,cqty,ComponentPOitems.bomcategoryid,bomid,rate,
                            tolerance,colourid,width from ComponentPO
                            inner join ComponentPOitems on ComponentPOitems.ComponentOrderid = ComponentPO.id
                            where (ComponentPO.supplierid = @supplierid and ComponentPO.isactive = true and ComponentPOitems.isactive = true
							and ComponentPO.expectedate>@fromdate and qty>(rqty+cqty))";

            if (allPOInfo.Count > 0)
                SQL_SearchPO += " or ComponentPOitems.id in (" + string.Join(",", allPOInfo.ConvertAll(y => y.componentpoitemsid)) + ")";

            List<ComponentPOItemDetail> ComponentPOItemDetails = new List<ComponentPOItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = GetCommand(SQL_SearchPO, conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = ComponentPOCreteria.supplierid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.AddMonths(-12);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ComponentPOItemDetail ComponentPOItemDetail = new ComponentPOItemDetail();
                        ComponentPOItemDetail.componentorderid = reader["componentorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["componentorderid"]);
                        ComponentPOItemDetail.componentpoitemsid = reader["componentpoitemsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["componentpoitemsid"]);
                        ComponentPOItemDetail.bomcategoryid = reader["bomcategoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomcategoryid"]);
                        ComponentPOItemDetail.bomid = reader["bomid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomid"]);
                        ComponentPOItemDetail.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        ComponentPOItemDetail.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        ComponentPOItemDetail.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ComponentPOItemDetail.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        ComponentPOItemDetail.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        ComponentPOItemDetail.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        ComponentPOItemDetail.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        ComponentPOItemDetail.width = reader["width"] == DBNull.Value ? "" : Convert.ToString(reader["width"]);
                        if (invoice != null && allPOInfo.Exists(x => x.componentpoitemsid == ComponentPOItemDetail.componentpoitemsid))
                        {
                            var rqty = allPOInfo.Sum(x => x.componentpoitemsid == ComponentPOItemDetail.componentorderid ? x.aqty : 0);
                            ComponentPOItemDetail.rqty -= ComponentPOItemDetail.rqty;
                            if (ComponentPOItemDetail.rqty < 0) ComponentPOItemDetail.rqty = 0;
                        }
                        ComponentPOItemDetails.Add(ComponentPOItemDetail);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, ComponentPOCreteria);
            }



            return ComponentPOItemDetails.OrderBy(e => e.podate).ToList();
        }



        public ComponentPO Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, ComponentPO oComponentPO)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_bompo)
            {
                command.Parameters.Add("hasapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            }
            else
            {
                command.Parameters.Add("hasapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            }
            command.Parameters.Add("podate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentPO.podate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentPO.expectedate;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.pono == null ? "" : oComponentPO.pono;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.supplierid;
            command.Parameters.Add("dsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.dsupplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.companyid;
            //command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentPO.tolerance;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oComponentPO.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.approvedby;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.preparedby;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.deliverylocationid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.freight;

            command.Parameters.Add("formimageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.formimageid == null ? "" : oComponentPO.formimageid;
            command.Parameters.Add("pobookno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.pobookno == null ? "" : oComponentPO.pobookno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.remarks == null ? "" : oComponentPO.remarks;
            command.Parameters.Add("pobookdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentPO.pobookdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oComponentPO.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            //InboxManager.Instance(true, oLrentry, transaction, conn);
            return oComponentPO;
        }

        public ComponentPO Update(ComponentPO oComponentPO)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oComponentPO);

                    ComponentPOitemsDataService subComponentPOitems = new ComponentPOitemsDataService() { Context = this.Context };
                    subComponentPOitems.Context = this.Context;

                    foreach (var item in oComponentPO.ComponentPOitems)
                    {
                        if (item.bomid <= 0 && !string.IsNullOrEmpty(item.bomcode))
                        {
                            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
                            var bommasterlist = bOMMasterDataService.Search(new BOMMasterCreteria() { categoryid = item.bomcategoryid, name = item.bomcode });
                            if (bommasterlist != null && bommasterlist.Count > 0)
                            {
                                item.bomid = bommasterlist[0].id;
                            }
                            else
                            {
                                var itemsequence = SequenceManager.Instance(Context.OrganizationID).Get("bommaster.bommastercode", 0);
                                var bommastercode = itemsequence.GetNextNumber(Context, conn, transaction);

                                item.bomid = bOMMasterDataService.Insert(transaction, conn, new BOMMaster()
                                {
                                    categoryid = item.bomcategoryid,
                                    name = item.bomcode,
                                    printingname = item.bomcode,
                                    isactive = true,
                                    version = 1,
                                    createdby = Context.UserID,
                                    modifiedby = Context.UserID,
                                    width = item.width,
                                    createdon = DateTimeNow,
                                    modifiedon = DateTimeNow,
                                    colorid = item.colourid,
                                    coretypeid = item.coretypeid,
                                    taxid = item.taxid,
                                    uomid = item.uomid,
                                    bommastercode = bommastercode
                                }).id;
                            }
                        }
                    }
                    subComponentPOitems.Update(transaction, conn, oComponentPO.ComponentPOitems, oComponentPO.id);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentPO);


            }

            return oComponentPO;
        }

        public ComponentPO Update(NpgsqlTransaction transaction, NpgsqlConnection conn, ComponentPO oComponentPO)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.id;
            command.Parameters.Add("podate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentPO.podate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentPO.expectedate;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.pono == null ? "" : oComponentPO.pono;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.supplierid;
            command.Parameters.Add("dsupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.dsupplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.companyid;
            //command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oComponentPO.tolerance;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oComponentPO.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.approvedby;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.preparedby;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.deliverylocationid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.freight;

            command.Parameters.Add("formimageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.formimageid == null ? "" : oComponentPO.formimageid;
            command.Parameters.Add("pobookno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.pobookno == null ? "" : oComponentPO.pobookno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oComponentPO.remarks == null ? "" : oComponentPO.remarks;
            command.Parameters.Add("pobookdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oComponentPO.pobookdate;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oComponentPO.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oComponentPO;
        }

        internal bool UpdateDesingImage(long ComponentPOitemid, string imageid)
        {
            var param = new SortedList<string, object>();
            param.Add("id", ComponentPOitemid);
            param.Add("imageid", imageid);
            string query = "update ComponentPOitems set imageid=@imageid where id=@id";
            return ExecuteQuery(query, param) == 1;
        }

        public bool Delete(ComponentPO oComponentPO)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oComponentPO);
                    ComponentPOitemsDataService subComponentPOitems = new ComponentPOitemsDataService() { Context = this.Context };
                    subComponentPOitems.Context = this.Context;
                    subComponentPOitems.DeleteAll(oComponentPO.id, oComponentPO.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentPO);
            }
            return hasSuccessCount;

        }

        public bool Complete(ComponentPO oComponentPO)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    ComponentPOitemsDataService subComponentPOitems = new ComponentPOitemsDataService() { Context = this.Context };
                    subComponentPOitems.Context = this.Context;
                    hasSuccessCount = subComponentPOitems.Cancelall(oComponentPO.id, oComponentPO.modifiedby,
                        conn, transaction);

                    ExecuteQuery("update componentpo set statusid=4 where id=" + oComponentPO.id,
                        new SortedList<string, object>(), conn, transaction);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentPO);
            }
            return hasSuccessCount;
        }
        public bool Cancel(ComponentPO oComponentPO)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    ComponentPOitemsDataService subComponentPOitems = new ComponentPOitemsDataService() { Context = this.Context };
                    subComponentPOitems.Context = this.Context;
                    hasSuccessCount = subComponentPOitems.Cancelall(oComponentPO.id, oComponentPO.modifiedby,
                        conn, transaction);

                    ExecuteQuery("update componentpo set statusid=3 where id=" + oComponentPO.id,
                        new SortedList<string, object>(), conn, transaction);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oComponentPO);
            }
            return hasSuccessCount;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, ComponentPO oComponentPO)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oComponentPO.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oComponentPO.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oComponentPO.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }


        public ComponentPOPrint Print(ComponentPO _ComponentPO)
        {
            var ComponentPO = Search(new ComponentPOCreteria()
            {
                id = _ComponentPO.id
            });

            ComponentPOPrint ret = new ComponentPOPrint();
            ret.id = ComponentPO[0].id;
            ret.tolerance = ComponentPO[0].tolerance;
            ret.companyid = ComponentPO[0].companyid;
            ret.deliverylocationid = ComponentPO[0].deliverylocationid;
            ret.freight = ComponentPO[0].freight;
            ret.remarks = ComponentPO[0].remarks;
            ret.podate = ComponentPO[0].podate;
            ret.pono = ComponentPO[0].pono;
            ret.expectedate = ComponentPO[0].expectedate;
            ret.supplierid = ComponentPO[0].supplierid;
            ret.dsupplierid = ComponentPO[0].dsupplierid;
            ret.createdby = ComponentPO[0].createdby;
            ret.createdon = ComponentPO[0].createdon;
            ret.modifiedby = ComponentPO[0].modifiedby;
            ret.modifiedon = ComponentPO[0].modifiedon;

            if (ComponentPO[0].ComponentPOitems != null && ComponentPO[0].ComponentPOitems.Count > 0)
                ret.totalqty = ComponentPO[0].ComponentPOitems.Sum(e => e.qty);

            ret.ComponentPOitems = ComponentPO[0].ComponentPOitems;

            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = ret.companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();

            Data.ReferencelistDataService _refdata = new ReferencelistDataService() { Context = this.Context };
            var _deliverylocation = _refdata.Search(new ReferencelistCreteria()
            {
                id = ret.deliverylocationid
            });

            if (_deliverylocation != null && _deliverylocation.Count > 0 && _deliverylocation[0].additionalinfo != null)
            {
                ret.deliverylocation = new CompanyLocation(_deliverylocation[0]);
            }

            Data.SupplierDataService _supplierdata = new SupplierDataService() { Context = this.Context };

            var supplier = _supplierdata.Search(new SupplierCreteria()
            {
                id = ret.supplierid
            });


            if (supplier != null && supplier.Count > 0)
            {
                ret.isinterstatesale = supplier[0].isinterstatesale;
                ret.supplier = supplier[0];
                ret.suppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;
                if (!string.IsNullOrEmpty(ret.suppliercompany.gstno) &&
                    !string.IsNullOrEmpty(ret.company.gstno))
                {
                    ret.isinterstatesale = ret.suppliercompany.gstno.Substring(0, 2) != ret.company.gstno.Substring(0, 2);
                }
                if (ret.agentid > 0)
                {
                    AgentDataService _agentdata = new AgentDataService() { Context = this.Context };
                    var agent = _agentdata.Search(new AgentCreteria()
                    {
                        id = ret.agentid
                    });
                    ret.agent = agent[0];
                }
                else
                {
                    ret.agent = new Agent() { name = "DIRECT" };
                }

                if (ret.supplier.preferredtransportid > 0)
                {
                    if (ret.supplier.preferredtransportid > 0)
                    {
                        TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                        var transport = transportDataService.Search(new TransportCreteria()
                        {
                            id = ret.supplier.preferredtransportid
                        });
                        ret.transport = transport[0];
                    }
                    else
                    {
                        ret.transport = new Transport() { name = "DIRECT" };
                    }
                }
            }


            if (ret.dsupplierid > 0)
            {
                supplier = _supplierdata.Search(new SupplierCreteria()
                {
                    id = ret.dsupplierid
                });
                if (supplier != null && supplier.Count > 0)
                {
                    ret.dsupplier = supplier[0];
                    ret.dsuppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;

                    if (ret.dsupplier.preferredtransportid > 0)
                    {
                        if (ret.dsupplier.preferredtransportid > 0)
                        {
                            TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                            var transport = transportDataService.Search(new TransportCreteria()
                            {
                                id = ret.supplier.preferredtransportid
                            });
                            ret.dtransport = transport[0];
                        }
                        else
                        {
                            ret.dtransport = new Transport() { name = "DIRECT" };
                        }
                    }
                }
            }
            List<long> userids = new List<long>();
            userids.Add(ret.createdby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
            }

            ret.ComponentPOitems = ret.ComponentPOitems.OrderBy(x => x.id).ToList();
            ret.ComponentPOitems.ForEach(x =>
            {
                x.amount = x.rate * x.qty;
                if (x.taxpercentage > 0)
                    x.tax = x.taxpercentage * (x.amount) / 100;
            });

            ret.grossvalue = ret.ComponentPOitems.Sum(x => x.amount);
            ret.itax = ret.ComponentPOitems.Sum(x => x.tax);
            if (ret.itax > 0)
            {
                ret.stax = ret.itax / 2;
            }

            ret.totalvalue = Math.Round(ret.ComponentPOitems.Sum(x => x.amount + x.tax), 0);
            if (ret.ComponentPOitems.GroupBy(x => x.taxpercentage).Count() == 1)
            {
                ret.itaxpercentage = ret.ComponentPOitems.First().taxpercentage;
                if (ret.itaxpercentage != 0)
                    ret.staxpercentage = ret.itaxpercentage / 2;
            }
            ret.rounding = Math.Round(ret.totalvalue, 0)- ret.ComponentPOitems.Sum(x => x.amount + x.tax);
            return ret;

        }
    }

}
