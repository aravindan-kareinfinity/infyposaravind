using System;
using System.Collections.Generic;
using System.Linq;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BOMConsumptionDataService : BaseData
    {
        const string SQL_Search = @"select BOMConsumption.id, BOMConsumption.entrydate, BOMConsumption.expectedate, BOMConsumption.entryno, BOMConsumption.supplierid, BOMConsumption.preparedby,
        BOMConsumption.approvedby, BOMConsumption.isactive, BOMConsumption.createdby, BOMConsumption.modifiedby, BOMConsumption.version,BOMConsumption.tolerance,BOMConsumption.mfgstageid,
        BOMConsumption.createdon, BOMConsumption.modifiedon,BOMConsumption.companyid,BOMConsumption.mfgworkorderid,BOMConsumption.mfgjoborderid,BOMConsumption.locationid,
        mfgworkorder.workorderno,mfgjoborder.joborderno,BOMConsumption.isoutward,consumptiontype,BOMConsumption.mode,BOMConsumption.referenceno,BOMConsumption.isrequest,
        BOMConsumption.bomrequestid,BOMConsumption.bomrequestid
        from BOMConsumption 
        left outer join mfgworkorder on mfgworkorder.id = BOMConsumption.mfgworkorderid
        left outer join mfgjoborder on mfgjoborder.id = BOMConsumption.mfgjoborderid
        where BOMConsumption.id=@id";

        const string SQL_Insert = @"INSERT INTO public.BOMConsumption(entrydate, expectedate, entryno, supplierid, isactive, createdby, modifiedby, version, createdon, modifiedon, approvedby,
        preparedby,tolerance,mfgstageid,companyid,mfgworkorderid,mfgjoborderid,locationid,isoutward,consumptiontype,mode,referenceno,isrequest,bomrequestid) 
        VALUES(@entrydate, @expectedate, @entryno, @supplierid, @isactive, @createdby, @modifiedby, @version,  @createdon, @modifiedon, @approvedby,@preparedby,
        @tolerance,@mfgstageid,@companyid,@mfgworkorderid,@mfgjoborderid,@locationid,@isoutward,@consumptiontype,@mode,@referenceno,@isrequest,@bomrequestid) RETURNING id";

        const string SQL_SearchDetail = @"select BOMConsumption.id, entrydate, BOMConsumption.expectedate, 
        BOMConsumption.entryno, BOMConsumption.supplierid, 
        preparedby,approvedby, BOMConsumption.isrequest,
        BOMConsumption.isactive, BOMConsumption.createdby,
        mfgworkorder.workorderno,mfgworkorder.wodate,BOMConsumption.bomrequestid,
        mfgjoborder.joborderno,mfgjoborder.joborderdate,BOMConsumption.bomrequestid,
        design,product,mfgjoborder.jobcardno,BOMConsumption.mfgstageid,
        BOMConsumption.modifiedby, BOMConsumption.version,BOMConsumption.tolerance, 
        BOMConsumption.createdon, BOMConsumption.modifiedon,BOMConsumption.companyid,
        BOMConsumption.mfgworkorderid,BOMConsumption.mfgjoborderid,BOMConsumption.isoutward,
        BOMConsumption.locationid,consumptiontype,BOMConsumption.mode,BOMConsumption.referenceno
        from BOMConsumption 
        LEFT OUTER join mfgworkorder on mfgworkorder.id=BOMConsumption.mfgworkorderid 
        LEFT OUTER join mfgjoborder on mfgjoborder.id=BOMConsumption.mfgjoborderid
        where BOMConsumption.id=@BOMConsumption.id";

        const string SQL_SearchPOSummary = @"with itemsummary as
(
	select BOMConsumptionid, sum(oqty) oqty, sum(rqty) rqty, sum(cqty) cqty from BOMConsumptionitems 
	where isactive = true group by BOMConsumptionid
)
                                select BOMConsumption.id, entrydate, expectedate, entryno, supplierid, termsandcondition, BOMConsumption.isactive, 
                                BOMConsumption.createdby, BOMConsumption.modifiedby, BOMConsumption.version,tolerance, BOMConsumption.createdon, BOMConsumption.modifiedon,
                                BOMConsumption.locationid, itemsummary.oqty, itemsummary.rqty, itemsummary.cqty,BOMConsumption.mfgstageid,
                                deliverylocationid,mode,agentid,freight,employee.name as employeename from BOMConsumption 
                                inner join itemsummary on itemsummary.BOMConsumptionid = BOMConsumption.id
                                inner join users on users.id=BOMConsumption.createdby 
                                inner join employee on employee.id=users.employeeid";



        const string SQL_Update = @"UPDATE public.BOMConsumption SET entrydate=@entrydate, expectedate=@expectedate, entryno=@entryno, supplierid=@supplierid, consumptiontype=@consumptiontype,
        preparedby=@preparedby, approvedby=@approvedby,modifiedby=@modifiedby,tolerance=@tolerance,mfgjoborderid=@mfgjoborderid,locationid=@locationid,mode=@mode,mfgstageid=@mfgstageid,bomrequestid=@bomrequestid,
        version=@version+1,modifiedon=@modifiedon,companyid=@companyid,mfgworkorderid=@mfgworkorderid,referenceno=@referenceno,isrequest=@isrequest where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.BOMConsumption set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<BOMConsumption> CreateRequest(BOMConsumptionCreteria _BOMConsumption)
        {
            BOMConsumption bomconsumption = Create(_BOMConsumption);
            List<BOMConsumption> bOMConsumptions = new List<BOMConsumption>();
            bOMConsumptions.Add(bomconsumption);
            BomrequestDataService bomrequestDataService = new BomrequestDataService() { Context = this.Context };
            var exist = bomrequestDataService.Search(new BomrequestCreteria()
            {
                mfgjoborderid = _BOMConsumption.mfgjoborderid,
                mfgworkorderid = _BOMConsumption.mfgworkorderid,
                mfgstageid = _BOMConsumption.mfgstageid,
            });

            if (exist.Count > 0)
            {
                bomconsumption.BOMConsumptionitems = exist[0].items;
                bomconsumption.approvedby = exist[0].approvedby;
                bomconsumption.companyid = exist[0].companyid;
                bomconsumption.entrydate = exist[0].entrydate;
                bomconsumption.entryno = exist[0].entryno;
                bomconsumption.bomrequestid = exist[0].id;
            }
            return bOMConsumptions;
        }

        public List<BOMConsumption> Search(BOMConsumptionCreteria _BOMConsumption)
        {
            if (_BOMConsumption.isrequest &&
                _BOMConsumption.create)
            {
                return CreateRequest(_BOMConsumption);
            }
            List<BOMConsumption> lst = new List<BOMConsumption>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMConsumption.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMConsumption.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("BOMConsumption.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_BOMConsumption.entryno))
                        qb.AddParameter("BOMConsumption.entryno", _BOMConsumption.entryno, "=");

                    if (!string.IsNullOrEmpty(_BOMConsumption.workorderno))
                        qb.AddParameter("mfgworkorder.workorderno", _BOMConsumption.workorderno, "=");

                    if (!string.IsNullOrEmpty(_BOMConsumption.joborderno))
                        qb.AddParameter("mfgjobwork.joborderno", _BOMConsumption.joborderno, "=");

                    //if (_BOMConsumption.mfgstageid > 0)
                    //    qb.AddParameter("BOMConsumption.mfgstageid", _BOMConsumption.mfgstageid, "=");

                    if (_BOMConsumption.companyid > 0)
                        qb.AddParameter("BOMConsumption.companyid", _BOMConsumption.companyid, "=");

                    if (_BOMConsumption.supplierid > 0)
                        qb.AddParameter("BOMConsumption.supplierid", _BOMConsumption.supplierid, "=");

                    if (_BOMConsumption.mfgworkorderid > 0)
                    {
                        qb.AddParameter("BOMConsumption.mfgworkorderid", _BOMConsumption.mfgworkorderid, "=");
                        qb.AddParameter("BOMConsumption.mfgjoborderid", 0, "=");
                    }

                    if (_BOMConsumption.mfgjoborderid > 0)
                        qb.AddParameter("BOMConsumption.mfgjoborderid", _BOMConsumption.mfgjoborderid, "=");

                    if (_BOMConsumption.locationid > 0)
                        qb.AddParameter("BOMConsumption.locationid", _BOMConsumption.locationid, "=");

                    if (_BOMConsumption.fromdate.Date != DateTime.MinValue)
                    {
                        qb.AddParameter("BOMConsumption.entrydate", "entrydatefrom", _BOMConsumption.fromdate.Date, ">=");
                        qb.AddParameter("BOMConsumption.entrydate", "entrydateto", _BOMConsumption.todate.Date.AddDays(1), "<");
                    }


                    if (_BOMConsumption.supplierid > 0)
                    {
                        qb.SortField = "BOMConsumption.id desc";
                    }
                    qb.Limit = _BOMConsumption.limit;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMConsumption oBOMConsumption = new BOMConsumption();
                        oBOMConsumption.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMConsumption.mfgstageid = reader["mfgstageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgstageid"]);
                        oBOMConsumption.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);


                        oBOMConsumption.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oBOMConsumption.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oBOMConsumption.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        oBOMConsumption.mode = reader["mode"] == DBNull.Value ? "" : reader["mode"].ToString();
                        oBOMConsumption.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        oBOMConsumption.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        oBOMConsumption.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();

                        oBOMConsumption.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oBOMConsumption.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oBOMConsumption.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMConsumption.isoutward = reader["isoutward"] == DBNull.Value ? false : Convert.ToBoolean(reader["isoutward"]);
                        oBOMConsumption.isrequest = reader["isrequest"] == DBNull.Value ? false : Convert.ToBoolean(reader["isrequest"]);

                        oBOMConsumption.consumptiontype = reader["consumptiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["consumptiontype"]);
                        //consumtpiontype
                        oBOMConsumption.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMConsumption.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMConsumption.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        //oBOMConsumption.status = reader["status"] == DBNull.Value ? BOMConsumption.POStatus.PENDING : (BOMConsumption.POStatus)Convert.ToInt32(reader["status"]);
                        oBOMConsumption.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMConsumption.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMConsumption.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        oBOMConsumption.preparedby = reader["preparedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["preparedby"]);
                        oBOMConsumption.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        oBOMConsumption.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        oBOMConsumption.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        lst.Add(oBOMConsumption);
                    }
                }
                if (lst.Count == 1)
                {
                    BOMConsumptionitemsDataService subBOMConsumptionitems = new BOMConsumptionitemsDataService() { Context = this.Context };
                    lst[0].BOMConsumptionitems = subBOMConsumptionitems.SearchAll(lst.First().id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMConsumption);
            }
            if (_BOMConsumption.create && lst.Count == 0)
            {
                lst.Insert(0, Create(_BOMConsumption));
            }
            return lst;
        }
        public BOMConsumption Create(BOMConsumptionCreteria _bomconsumption)
        {
            BOMConsumption bOMConsumption = new BOMConsumption();
            string query = @"select mfgworkorder.id mfgworkorderid,mfgjoborder.id mfgjoborderid,
                            mfgworkorder.workorderno,joborderno,mfgworkorder.mode wmode,
                            mfgjoborder.mode jmode, mfgworkorder.supplierid wosupplierid,
                            mfgjoborder.supplierid josupplierid
                            from mfgworkorder
                            left outer join mfgjoborder on mfgjoborder.id = @mfgjoborderid
                            where mfgworkorder.id = @mfgworkorderid ";
            var param = new SortedList<string, object>();
            param.Add("mfgjoborderid", _bomconsumption.mfgjoborderid);
            param.Add("mfgworkorderid", _bomconsumption.mfgworkorderid);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    bOMConsumption.mfgjoborderid = reader["mfgjoborderid"] != DBNull.Value ? Convert.ToInt64(reader["mfgjoborderid"]) : 0;
                    bOMConsumption.mfgworkorderid = reader["mfgworkorderid"] != DBNull.Value ? Convert.ToInt64(reader["mfgworkorderid"]) : 0;
                    bOMConsumption.workorderno = reader["workorderno"] != DBNull.Value ? Convert.ToString(reader["workorderno"]) : "";
                    bOMConsumption.joborderno = reader["joborderno"] != DBNull.Value ? Convert.ToString(reader["joborderno"]) : "";
                    if (_bomconsumption.mfgjoborderid > 0)
                    {
                        bOMConsumption.supplierid = reader["josupplierid"] != DBNull.Value ? Convert.ToInt64(reader["josupplierid"]) : 0;
                    }
                    else
                    {
                        bOMConsumption.supplierid = reader["wosupplierid"] != DBNull.Value ? Convert.ToInt64(reader["wosupplierid"]) : 0;
                    }
                    bOMConsumption.mode = "Internal";
                    if ((reader["jmode"] != DBNull.Value ? Convert.ToString(reader["jmode"]) : "") == "External")
                    {
                        bOMConsumption.isoutward = true;
                        bOMConsumption.mode = "External";
                    }
                    else if (reader["jmode"] == DBNull.Value)
                    {
                        if ((reader["wmode"] != DBNull.Value ? Convert.ToString(reader["wmode"]) : "") == "External")
                        {
                            bOMConsumption.isoutward = true;
                            bOMConsumption.mode = "External";
                        }
                    }

                }
            }
            bOMConsumption.mfgstageid = _bomconsumption.mfgstageid;


            MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
            var mfgworkorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
            {
                id = _bomconsumption.mfgworkorderid
            }).First();

            MfgdesigncostDataService mfgdesigncostDataService = new MfgdesigncostDataService() { Context = this.Context };
            var costlist = mfgdesigncostDataService.Search(new MfgdesigncostCreteria()
            {
                mfgdesignids = mfgworkorder.Mfgworkorderitems.ConvertAll(x => x.designid).Distinct().ToList(),
                stageid = _bomconsumption.mfgstageid
            });

            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
            var bomlist = bOMMasterDataService.Search(new BOMMasterCreteria()
            {
                ids = costlist.Select(x => x.bomid).Distinct().ToList(),
            });
            bOMConsumption.BOMConsumptionitems = new List<BOMConsumptionitemsDetail>();

            foreach (var jobitem in mfgworkorder.Mfgworkorderitems)
            {
                var costitems = costlist.FindAll(x => x.mfgdesignid == jobitem.designid &&
                                x.colourid == jobitem.colourid);
                foreach (var costitem in costitems)
                {
                    var bom = bomlist.Find(x => x.id == costitem.bomid);
                    BOMMaster.colouritem col = bom != null ? bom.colourlist.Find(x => x.id == costitem.colourid) : null;
                    string colourcode = col != null ? col.name : "";
                    decimal qty = jobitem.sizelist.Sum(x => x.qty);
                    if (costitem.qty * qty > 0)
                    {
                        BOMConsumptionitemsDetail citem = bOMConsumption.BOMConsumptionitems.Find(X => X.colourid == costitem.colourid &&
                        X.bomid == costitem.bomid);
                        if (citem == null)
                        {
                            citem = new BOMConsumptionitemsDetail()
                            {
                                bomcategoryid = costitem.categoryid,
                                bomid = costitem.bomid,
                                colourid = costitem.colourid,
                                colourcode = colourcode,
                                isactive = true,
                                isrequest = true,
                                rate = bom != null ? bom.buyingrate : 0,
                                qty = costitem.qty * qty
                            };
                            bOMConsumption.BOMConsumptionitems.Add(citem);
                        }
                        else
                        {
                            citem.qty += (costitem.qty * qty);
                        }
                        citem.amount = citem.rate * citem.qty;
                    }
                }
            }
            return bOMConsumption;
        }


        public List<BOMConsumePrint> SearchDetail(BOMConsumptionCreteria _bomconsumption)
        {
            List<BOMConsumePrint> lst = new List<BOMConsumePrint>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bomconsumption.id != 0)
                {
                    command = new NpgsqlCommand(SQL_SearchDetail, conn);
                    command.Parameters.Add("BOMConsumption.id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bomconsumption.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_SearchDetail.IndexOf("where"));
                    qb.AddParameter("BOMConsumption.isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_mfgworkorder.Code))
                    //    qb.AddParameter("code", _mfgworkorder.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMConsumePrint oBOMConsumption = new BOMConsumePrint();

                        oBOMConsumption.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oBOMConsumption.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMConsumption.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMConsumption.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMConsumption.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oBOMConsumption.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        oBOMConsumption.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        oBOMConsumption.mode = reader["mode"] == DBNull.Value ? "" : reader["mode"].ToString();
                        oBOMConsumption.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                        oBOMConsumption.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oBOMConsumption.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oBOMConsumption.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        oBOMConsumption.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        oBOMConsumption.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        oBOMConsumption.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        oBOMConsumption.orderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        oBOMConsumption.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                        oBOMConsumption.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        oBOMConsumption.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        oBOMConsumption.joborderdate = reader["joborderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joborderdate"]);
                        oBOMConsumption.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oBOMConsumption.isoutward = reader["isoutward"] == DBNull.Value ? false : Convert.ToBoolean(reader["isoutward"]);
                        oBOMConsumption.isrequest = reader["isrequest"] == DBNull.Value ? false : Convert.ToBoolean(reader["isrequest"]);
                        oBOMConsumption.consumptiontype = reader["consumptiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["consumptiontype"]);
                        lst.Add(oBOMConsumption);
                    }
                }
                if (_bomconsumption.id > 0 && lst.Count == 1)
                {
                    BOMConsumptionitemsDataService subbomcomsumpitems = new BOMConsumptionitemsDataService() { Context = this.Context };
                    lst[0].BOMConsumptionitems = subbomcomsumpitems.SearchAll(_bomconsumption.id);

                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bomconsumption);
            }
            return lst;
        }

        public BOMConsumePrint PrintSummary(BOMConsumptionCreteria _bomconsumption)
        {
            var bomconsumeitems = SearchDetail(_bomconsumption);
            BOMConsumePrint ret = bomconsumeitems[0];

            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            var bomsummary = bomstocktransactionDataService.SummaryReport(new BomstocktransactionCreteria()
            {
                fromdate = ret.entrydate.Date,
                todate = ret.entrydate.Date,
                bomids = ret.BOMConsumptionitems.ConvertAll(x => x.bomid)
            });

            List<BOMConsumptionitemsDetail> items = new List<BOMConsumptionitemsDetail>();
            foreach(var item in  ret.BOMConsumptionitems.GroupBy(x => new { x.bomid,x.colourid,x.rate }))
            {
                item.First().qty = item.Sum(x => x.qty);
                items.Add(item.First());
            }
            ret.BOMConsumptionitems = items;

            ret.BOMConsumptionitems.ForEach(x =>
            {
                var bs = bomsummary.Find(s => s.bomid == x.bomid && s.colourid == x.colourid);
                if (bs != null)
                {
                    x.opening = bs.opening;
                    x.closing = x.opening - x.qty;
                    bs.opening = x.closing;
                }
            });

            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = ret.companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            Data.SupplierDataService _supplierdata = new SupplierDataService() { Context = this.Context };
            var supplier = _supplierdata.Search(new SupplierCreteria()
            {
                id = ret.supplierid
            });


            if (supplier != null && supplier.Count > 0)
            {
                ret.supplier = supplier[0];
                ret.suppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;
            }

            List<long> userids = new List<long>();
            userids.Add(ret.createdby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
            }

            return ret;

        }
        public BOMConsumePrint Print(BOMConsumptionCreteria _bomconsumption)
        {
            var bomconsumeitems = SearchDetail(_bomconsumption);

            BOMConsumePrint ret = bomconsumeitems[0];
            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = ret.companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            Data.SupplierDataService _supplierdata = new SupplierDataService() { Context = this.Context };
            var supplier = _supplierdata.Search(new SupplierCreteria()
            {
                id = ret.supplierid
            });


            if (supplier != null && supplier.Count > 0)
            {
                ret.supplier = supplier[0];
                ret.suppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;
            }

            List<long> userids = new List<long>();
            userids.Add(ret.createdby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
            }

            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            var bomsummary = bomstocktransactionDataService.SummaryReport(new BomstocktransactionCreteria()
            {
                fromdate = ret.entrydate.Date,
                todate = ret.entrydate.Date,
                bomids = ret.BOMConsumptionitems.ConvertAll(x => x.bomid)
            });
            ret.BOMConsumptionitems.ForEach(x =>
            {
                var bs = bomsummary.Find(s => s.bomid == x.bomid && s.colourid == x.colourid);
                if (bs != null)
                {
                    x.opening = bs.opening;
                    x.closing = x.opening - x.qty;
                    bs.opening = x.closing;
                }
            });
            return ret;
        }


        public BOMConsumption Insert(BOMConsumption oBOMConsumption)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("bomconsumption.entryno", 0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                   oBOMConsumption.companyid : 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oBOMConsumption.consumptiontype == 15)
                    {
                        BomrequestDataService bomrequestDataService = new BomrequestDataService()
                        {
                            Context = this.Context
                        };

                        var request = new BOMRequest()
                        {
                            Activestatus = BaseEntity.ActiveStatus.UnApproved,
                            companyid = oBOMConsumption.companyid,
                            entrydate = DateTime.Now,
                            fromlocationid = oBOMConsumption.locationid,
                            mfgjoborderid = oBOMConsumption.mfgjoborderid,
                            mfgworkorderid = oBOMConsumption.mfgworkorderid,
                            preparedby = oBOMConsumption.preparedby,
                            approvedby = oBOMConsumption.approvedby,
                            status = 0,
                            mfgstageid = oBOMConsumption.mfgstageid,
                            items = oBOMConsumption.BOMConsumptionitems
                        };

                        if (oBOMConsumption.bomrequestid > 0)
                        {
                            var requestlist = bomrequestDataService.Search(new BomrequestCreteria()
                            {
                                id = oBOMConsumption.bomrequestid
                            });
                            if (requestlist.Count > 0)
                            {
                                request = requestlist[0];
                                request.companyid = oBOMConsumption.companyid;
                                request.fromlocationid = oBOMConsumption.locationid;
                                request.preparedby = oBOMConsumption.preparedby;
                                request.approvedby = oBOMConsumption.approvedby;
                                request.remarks = oBOMConsumption.referenceno;
                                bomrequestDataService.Update(transaction, conn, request);
                            }
                        }
                        else
                        {
                            bomrequestDataService.Insert(transaction, conn, request);
                            oBOMConsumption.bomrequestid = request.id;
                            oBOMConsumption.entrydate = request.entrydate;
                            oBOMConsumption.entryno = request.entryno;
                        }
                    }
                    else
                    {
                        oBOMConsumption.entryno = sequence.GetNextNumber(Context, conn, transaction);
                        Insert(transaction, conn, oBOMConsumption);
                        BOMConsumptionitemsDataService subBOMConsumptionitems = new BOMConsumptionitemsDataService() { Context = this.Context };
                        foreach (var item in oBOMConsumption.BOMConsumptionitems)
                        {
                            item.bomconsumptionid = oBOMConsumption.id;
                            subBOMConsumptionitems.Insert(transaction, conn, item);
                        }
                        if (oBOMConsumption.bomrequestid > 0)
                        {
                            var param = new SortedList<string, object>();
                            param.Add("bomconsumptionno", oBOMConsumption.entryno);
                            param.Add("bomconsumptionid", oBOMConsumption.id);
                            param.Add("bomrequestid", oBOMConsumption.bomrequestid);
                            param.Add("bomconsumptionon", oBOMConsumption.entrydate);
                            ExecuteQuery(@"Update bomrequest 
                               set bomconsumptionno=@bomconsumptionno,
                               bomconsumptionid=@bomconsumptionid,
                               bomconsumptionon=@bomconsumptionon,
                               status = case when status>3 then status else 3 end
                               where id=@bomrequestid",
                               param, conn, transaction);
                        }
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Insert((Bomstocktransaction.TransactionType)oBOMConsumption.consumptiontype, conn, transaction, oBOMConsumption, oBOMConsumption.BOMConsumptionitems);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMConsumption);
            }
            return oBOMConsumption;
        }

        public BOMConsumption Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMConsumption oBOMConsumption)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMConsumption.entrydate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMConsumption.expectedate;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumption.entryno == null ? "" : oBOMConsumption.entryno;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumption.mode == null ? "" : oBOMConsumption.mode;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumption.referenceno == null ? "" : oBOMConsumption.referenceno;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.supplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.companyid;
            command.Parameters.Add("mfgstageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.mfgstageid;
            //command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumption.tolerance;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isactive;
            command.Parameters.Add("isoutward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isoutward;
            command.Parameters.Add("isrequest", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isrequest;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.version;
            command.Parameters.Add("consumptiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.consumptiontype;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.approvedby;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.preparedby;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.mfgworkorderid;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.mfgjoborderid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.locationid;
            command.Parameters.Add("bomrequestid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.bomrequestid;
            
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMConsumption.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            //InboxManager.Instance(true, oLrentry, transaction, conn);
            return oBOMConsumption;
        }

        public BOMConsumption Update(BOMConsumption oBOMConsumption)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBOMConsumption);

                    if (oBOMConsumption.bomrequestid > 0)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("bomconsumptionno", oBOMConsumption.entryno);
                        param.Add("bomconsumptionid", oBOMConsumption.id);
                        param.Add("bomrequestid", oBOMConsumption.bomrequestid);
                        param.Add("bomconsumptionon", oBOMConsumption.entrydate);
                        ExecuteQuery(@"Update bomrequest 
                               set bomconsumptionno=@bomconsumptionno,
                               bomconsumptionid=@bomconsumptionid,
                               bomconsumptionon=@bomconsumptionon,
                               status = case when status>3 then status else 3 end
                               where id=@bomrequestid",
                           param, conn, transaction);
                    }

                    BOMConsumptionitemsDataService subBOMConsumptionitems = new BOMConsumptionitemsDataService() { Context = this.Context };
                    subBOMConsumptionitems.Update(transaction, conn, oBOMConsumption, oBOMConsumption.BOMConsumptionitems,
                        oBOMConsumption.id);

                    var consumptiontype = Search(new BOMConsumptionCreteria()
                    {
                        id = oBOMConsumption.id,
                        create = true
                    }).First().consumptiontype;

                    if (oBOMConsumption.consumptiontype == 15)
                    {
                        BomrequestDataService bomrequestDataService = new BomrequestDataService() { Context = this.Context };
                        var requestlist = bomrequestDataService.Search(new BomrequestCreteria()
                        {
                            bomconsumptionid = oBOMConsumption.id
                        });
                        if (requestlist.Count == 1)
                        {
                            requestlist[0].items = oBOMConsumption.BOMConsumptionitems;
                            requestlist[0].modifiedby = Context.UserID;
                            requestlist[0].modifiedon = DateTime.Now;
                            bomrequestDataService.Update(transaction, conn, requestlist[0]);
                        }
                    }
                    else
                    {
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Delete((Bomstocktransaction.TransactionType)consumptiontype, transaction, conn,
                            oBOMConsumption.id);

                        bomstocktransactionDataService.Insert((Bomstocktransaction.TransactionType)oBOMConsumption.consumptiontype, conn, transaction,
                          oBOMConsumption, oBOMConsumption.BOMConsumptionitems);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMConsumption);
            }
            return oBOMConsumption;
        }

        public BOMConsumption Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMConsumption oBOMConsumption)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.id;
            command.Parameters.Add("entrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMConsumption.entrydate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMConsumption.expectedate;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumption.entryno == null ? "" : oBOMConsumption.entryno;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumption.mode == null ? "" : oBOMConsumption.mode;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMConsumption.referenceno == null ? "" : oBOMConsumption.referenceno;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.supplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.companyid;
            command.Parameters.Add("mfgstageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.mfgstageid;
            //command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMConsumption.tolerance;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isactive;
            command.Parameters.Add("isoutward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isoutward;
            command.Parameters.Add("isrequest", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isrequest;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.version;
            command.Parameters.Add("consumptiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.consumptiontype;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.approvedby;
            command.Parameters.Add("preparedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.preparedby;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.mfgworkorderid;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.mfgjoborderid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.locationid;
            command.Parameters.Add("bomrequestid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.bomrequestid;
            
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMConsumption.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBOMConsumption;
        }

        internal bool UpdateDesingImage(long BOMConsumptionitemid, string imageid)
        {
            var param = new SortedList<string, object>();
            param.Add("id", BOMConsumptionitemid);
            param.Add("imageid", imageid);
            string query = "update BOMConsumptionitems set imageid=@imageid where id=@id";
            return ExecuteQuery(query, param) == 1;
        }

        public bool Delete(BOMConsumption oBOMConsumption)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBOMConsumption);
                    BOMConsumptionitemsDataService subBOMConsumptionitems = new BOMConsumptionitemsDataService() { Context = this.Context };
                    subBOMConsumptionitems.DeleteAll(oBOMConsumption.id, oBOMConsumption.modifiedby, conn, transaction);

                    var consumptiontype = Search(new BOMConsumptionCreteria()
                    {
                        id = oBOMConsumption.id,
                        create = true
                    }).First().consumptiontype;

                    BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                    bomstocktransactionDataService.Delete((Bomstocktransaction.TransactionType)consumptiontype, transaction, conn,
                        oBOMConsumption.id);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMConsumption);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMConsumption oBOMConsumption)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMConsumption.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMConsumption.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMConsumption.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<BOMConsumptionitems> ImportBOM(BOMConsumption item)
        {
            var results = new List<BOMConsumptionitems>();
            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };

            List<long> designid = new List<long>();
            if (item.mfgjoborderid == 0)
            {
                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                {
                    id = item.mfgworkorderid
                }).First();
                designid.AddRange(workorder.Mfgworkorderitems.ConvertAll(x => x.designid));
                foreach (var witem in workorder.Mfgworkorderitems)
                {
                    var fabrics = bOMMasterDataService.Search(new BOMMasterCreteria()
                    {
                        id = witem.fabricid
                    });
                    if (fabrics.Count == 1)
                    {
                        results.Add(new BOMConsumptionitems()
                        {
                            bomcategoryid = fabrics[0].categoryid,
                            bomid = fabrics[0].id,
                            qty = witem.outqty,
                            colourid = witem.colourid,
                            colourcode = fabrics[0].colourlist.Exists(x => x.id == witem.colourid) ? fabrics[0].colourlist.Find(x => x.id == witem.colourid).name : ""
                        });
                    }
                }


                designid.AddRange(workorder.Mfgworkorderitems.ConvertAll(x => x.designid));

                MfgdesigncostDataService mfgdesigncostDataService = new MfgdesigncostDataService() { Context = this.Context };
                var bomcostlist = mfgdesigncostDataService.Search(new MfgdesigncostCreteria()
                {
                    mfgdesignids = designid,
                    stageid = item.mfgstageid
                });

                var fabriclist = bOMMasterDataService.Search(new BOMMasterCreteria()
                {
                    ids = bomcostlist.ConvertAll(x => x.bomid)
                });

                foreach (var witem in workorder.Mfgworkorderitems)
                {
                    foreach (var sitem in witem.sizelist)
                    {
                        var bomlist = bomcostlist.FindAll(x => x.mfgdesignid == witem.designid &&
                        x.colourid == witem.colourid &&
                        x.sizeid == sitem.sizeid);

                        foreach (var bom in bomlist)
                        {
                            var fabric = fabriclist.Find(x => x.id == bom.bomid);
                            results.Add(new BOMConsumptionitems()
                            {
                                bomcategoryid = bom.categoryid,
                                bomid = bom.bomid,
                                rate = fabric.buyingrate,
                                qty = sitem.qty * bom.qty,
                                colourid = witem.colourid,
                                colourcode = fabric.colourlist.Exists(x => x.id == witem.colourid) ? fabric.colourlist.Find(x => x.id == witem.colourid).name : ""
                            });
                        }
                    }
                }

            }
            else
            {
                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                {
                    id = item.mfgworkorderid
                }).First();

                MfgjoborderDataService mfgjoborderDataService = new MfgjoborderDataService() { Context = this.Context };
                var joborder = mfgjoborderDataService.Search(new MfgjoborderCreteria()
                {
                    id = item.mfgjoborderid
                }).First();
                foreach (var jitem in joborder.Mfgjoborderitems.Where(x => x.isbom))
                {
                    var fabrics = bOMMasterDataService.Search(new BOMMasterCreteria()
                    {
                        id = jitem.bomid
                    });
                    if (fabrics.Count == 1)
                    {
                        results.Add(new BOMConsumptionitems()
                        {
                            bomcategoryid = jitem.bomcategoryid,
                            bomid = jitem.bomid,
                            rate = fabrics[0].buyingrate,
                            qty = jitem.oqty,
                            colourid = jitem.colourid,
                            colourcode = fabrics[0].colourlist.Exists(x => x.id == jitem.colourid) ? fabrics[0].colourlist.Find(x => x.id == jitem.colourid).name : ""
                        });
                    }
                }

                var witems = workorder.Mfgworkorderitems.FindAll(x => joborder.Mfgjoborderitems.Exists(y => y.mfgworkorderitemsid == x.id));
                designid.AddRange(witems.ConvertAll(x => x.designid));

                MfgdesigncostDataService mfgdesigncostDataService = new MfgdesigncostDataService() { Context = this.Context };
                var bomcostlist = mfgdesigncostDataService.Search(new MfgdesigncostCreteria()
                {
                    mfgdesignids = designid,
                    stageid = item.mfgstageid
                });

                var fabriclist = bOMMasterDataService.Search(new BOMMasterCreteria()
                {
                    ids = bomcostlist.ConvertAll(x => x.bomid)
                });

                foreach (var jitem in joborder.Mfgjoborderitems)
                {
                    var witem = workorder.Mfgworkorderitems.Find(x => x.id == jitem.mfgworkorderitemsid);
                    var sitem = witem.sizelist.Find(x => x.id == jitem.mfgworkorderitemsizesid);
                    var bomlist = bomcostlist.FindAll(x => x.mfgdesignid == witem.designid &&
                    x.colourid == witem.colourid &&
                    x.sizeid == sitem.sizeid);

                    foreach (var bom in bomlist)
                    {
                        var fabric = fabriclist.Find(x => x.id == bom.bomid);
                        results.Add(new BOMConsumptionitems()
                        {
                            bomcategoryid = bom.categoryid,
                            bomid = bom.bomid,
                            qty = jitem.oqty * bom.qty,
                            rate = fabric.buyingrate,
                            colourid = jitem.colourid,
                            colourcode = fabric.colourlist.Exists(x => x.id == jitem.colourid) ? fabric.colourlist.Find(x => x.id == jitem.colourid).name : ""
                        });
                    }
                }
            }

            return results.GroupBy(x => new { x.bomcategoryid, x.bomid, x.rate, x.colourcode, x.colourid }).ToList().ConvertAll(x =>
            {
                return new BOMConsumptionitems()
                {
                    bomcategoryid = x.Key.bomcategoryid,
                    bomid = x.Key.bomid,
                    rate = x.Key.rate,
                    colourcode = x.Key.colourcode,
                    colourid = x.Key.colourid,
                    qty = Math.Round(x.Sum(y => y.qty), 4)
                };
            });
        }

        internal decimal FindBomStock(BomStockRequest item)
        {
            BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
            var stocksummary = bomstocktransactionDataService.SummaryReport(new BomstocktransactionCreteria()
            {
                fromdate = DateTime.Now.Date.AddDays(10),
                todate = DateTime.Now.Date.AddDays(10),
                locationid = item.locationid,
                companyid = item.companyid,
                bomid = item.bomid,
                colourid = item.colourid,
            });
            if (stocksummary.Count == 0) return 0;
            return stocksummary[0].closing;
        }
    }
}