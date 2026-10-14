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
    public class MfgjoborderDataService : SAASData
    {
        const string SQL_Search = @"select id, expectedate, supplierid, termsandcondition, inwardjob,inwardgoods, isactive, createdby, modifiedby, version, createdon, modifiedon, fromlocationid, status, tolocationid, tolerance, freight, issuedby, receivedby, receivedon, 
        companyid, tax, taxable, total, workorderno, mfgworkorderid, mode, stageid,corestageid,mfgstageid, joborderdate, joborderno,jobcardno, sourcematerial, outcomematerial,workflowid,stagename,sourcejobid,sourcejobno,inwardedby,inwardeddate,isbomprocess,bominvoiceid,
        invoiceno,invoicedate,bomentryno,bomrequestid from mfgjoborder where id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfgjoborder(expectedate, supplierid, termsandcondition, inwardjob,inwardgoods, isactive, createdby, modifiedby, version, createdon, modifiedon, fromlocationid, status, tolocationid, tolerance, freight, issuedby, 
        receivedby, receivedon, companyid, tax, taxable, total, workorderno, mfgworkorderid, mode, stageid,corestageid,mfgstageid, joborderdate, joborderno,jobcardno, sourcematerial, outcomematerial,workflowid,stagename,sourcejobid,sourcejobno,inwardedby,inwardeddate,
        isbomprocess,bominvoiceid,invoiceno,invoicedate,bomentryno,bomrequestid) 
        VALUES(@expectedate, @supplierid, @termsandcondition,@inwardjob,@inwardgoods, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @fromlocationid, @status, @tolocationid, @tolerance, @freight, @issuedby, @receivedby, @receivedon, @companyid,
        @tax, @taxable, @total, @workorderno, @mfgworkorderid, @mode, @stageid,@corestageid,@mfgstageid, @joborderdate, @joborderno,@jobcardno, @sourcematerial, @outcomematerial,@workflowid,@stagename,@sourcejobid,@sourcejobno,@inwardedby,@inwardeddate,@isbomprocess,
        @bominvoiceid,@invoiceno,@invoicedate,@bomentryno,@bomrequestid) RETURNING id";
        internal Mfgjoborder CreateInward(Mfgjoborderstage jobworkinfo)
        {
            var sourcejoborder = Search(new MfgjoborderCreteria()
            {
                id = jobworkinfo.joborderid
            }).First();

            sourcejoborder.sourcejobid = sourcejoborder.id;
            sourcejoborder.sourcejobno = sourcejoborder.joborderno;
            sourcejoborder.isbomprocess = sourcejoborder.corestageid == -1;
            if (sourcejoborder.isbomprocess)
                sourcejoborder.stagename += "(BOM RETURN)";
            sourcejoborder.id = 0;
            sourcejoborder.joborderno = "";
            sourcejoborder.inwardjob = true;
            sourcejoborder.createdby = Context.UserID;
            sourcejoborder.modifiedby = Context.UserID;

            MfgjoborderitemsDataService mfgjoborderitemsDataService = new MfgjoborderitemsDataService();


            MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
            var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
            {
                id = sourcejoborder.mfgworkorderid
            }).First();

            if (sourcejoborder.corestageid == 1)
            {
                var currentorderitems = sourcejoborder.Mfgjoborderitems;
                sourcejoborder.Mfgjoborderitems = new List<Mfgjoborderitems>();
                foreach (var witem in workorder.Mfgworkorderitems.OrderBy(x => x.id))
                {
                    if (witem.sizelist != null && witem.sizelist.Count > 0)
                    {
                        foreach (var sizeitem in witem.sizelist)
                        {

                            var orderitem = new Mfgjoborderitems()
                            {
                                sno = (sourcejoborder.Mfgjoborderitems.Count + 1),
                                bomcategoryid = witem.bomcategoryid,
                                isbom = false,
                                bomid = witem.fabricid,
                                productid = witem.productid,
                                colourid = witem.colourid,
                                sizeid = sizeitem.sizeid,
                                oqty = sizeitem.qty,
                                rqty = 0,
                                cqty = 0,
                                typedisplay = "Input Product",
                                itemdisplay = witem.designname,
                                bominwardcode = witem.bominwardcode,
                                bominvoiceitemimage = witem.bominvoiceitemimage,
                                mfgworkorderid = workorder.id,
                                mfgworkorderitemsid = witem.id,
                                mfgworkorderitemsizesid = sizeitem.id,
                                isactive = true,
                                corestageid = jobworkinfo.corestageid
                            };
                            sourcejoborder.Mfgjoborderitems.Add(orderitem);
                        }
                    }
                    else
                    {
                        var orderitem = new Mfgjoborderitems()
                        {
                            sno = (sourcejoborder.Mfgjoborderitems.Count + 1),
                            bomcategoryid = witem.bomcategoryid,
                            bomid = witem.fabricid,
                            isbom = false,
                            productid = witem.productid,
                            colourid = witem.colourid,
                            oqty = witem.inqty,
                            rqty = 0,
                            cqty = 0,
                            typedisplay = "Input Product",
                            itemdisplay = witem.designname,
                            bominwardcode = witem.bominwardcode,
                            bominvoiceitemimage = witem.bominvoiceitemimage,
                            mfgworkorderid = workorder.id,
                            mfgworkorderitemsid = witem.id,
                            isactive = true,
                            corestageid = jobworkinfo.corestageid
                        };
                        sourcejoborder.Mfgjoborderitems.Add(orderitem);
                    }
                }
                var existingreceived = mfgjoborderitemsDataService.Search(new MfgjoborderitemsCreteria()
                {
                    mfgworkorderid = sourcejoborder.mfgworkorderid,
                    corestageid = sourcejoborder.corestageid,
                    inwardjob = true
                });

                foreach (var item in sourcejoborder.Mfgjoborderitems)
                {
                    if (existingreceived != null && existingreceived.Count > 0)
                    {
                        item.aqty = existingreceived.Sum(x => x.mfgworkorderitemsid == item.mfgworkorderitemsid &&
                        x.mfgworkorderitemsizesid == item.mfgworkorderitemsizesid ? x.rqty : 0);
                    }
                }

                foreach (var item in currentorderitems)
                {
                    var oqty = item.oqty;
                    //if(item.isbom &&
                    //    workorder.Mfgworkorderitems.Exists(x => x.fabricid == item.bomid && x.colourid == item.colourid))
                    //{
                    //    var workorderitem = workorder.Mfgworkorderitems.Find(x => x.fabricid == item.bomid && x.colourid == item.colourid);
                    //    oqty = Math.Round(oqty * workorderitem.cutratio, 2);
                    //    item.mfgworkorderitemsid = workorderitem.id;
                    //}
                    var orderitem = new Mfgjoborderitems()
                    {
                        sno = (sourcejoborder.Mfgjoborderitems.Count + 1),
                        bomcategoryid = item.bomcategoryid,
                        isbom = item.isbom,
                        bomconsumptionitemid = item.bomconsumptionitemid,
                        inwardjob = true,
                        isfabric = item.isfabric,
                        bomid = item.bomid,
                        materialtype = item.materialtype,
                        dynamicbom = item.dynamicbom,
                        productid = item.productid,
                        colourid = item.colourid,
                        sizeid = item.sizeid,
                        oqty = oqty,
                        rqty = 0,
                        cqty = 0,
                        typedisplay = item.typedisplay,
                        itemdisplay = item.itemdisplay,
                        bominwardcode = item.bominwardcode,
                        sourcejoborderitemid = item.id,
                        bominvoiceitemimage = item.bominvoiceitemimage,
                        bominvoiceitemid = item.bominvoiceitemid,
                        mfgworkorderid = workorder.id,
                        mfgworkorderitemsid = item.mfgworkorderitemsid,
                        isactive = true,
                        corestageid = jobworkinfo.corestageid
                    };
                    sourcejoborder.Mfgjoborderitems.Add(orderitem);
                }
            }

            if (sourcejoborder.corestageid == 2)
            {
                var existingreceived = mfgjoborderitemsDataService.Search(new MfgjoborderitemsCreteria()
                {
                    mfgworkorderid = sourcejoborder.mfgworkorderid,
                    sourcejoborderitemids = sourcejoborder.Mfgjoborderitems.ConvertAll(x => x.id)
                });
                var currentorderitems = sourcejoborder.Mfgjoborderitems;
                sourcejoborder.Mfgjoborderitems = new List<Mfgjoborderitems>();
                foreach (var witem in currentorderitems)
                {
                    if (witem.oqty <= 0) continue;
                    decimal aqty = 0;
                    if (existingreceived != null && existingreceived.Count > 0)
                    {
                        aqty = existingreceived.Sum(x => x.sourcejoborderitemid == witem.id ? x.rqty : 0);
                    }
                    var orderitem = new Mfgjoborderitems()
                    {
                        sno = (sourcejoborder.Mfgjoborderitems.Count + 1),
                        bomcategoryid = witem.bomcategoryid,
                        isbom = false,
                        bomid = witem.bomid,
                        productid = witem.productid,
                        colourid = witem.colourid,
                        sizeid = witem.sizeid,
                        oqty = witem.oqty,
                        aqty = aqty,
                        sourcejoborderitemid = witem.id,
                        rqty = 0,
                        cqty = 0,
                        typedisplay = witem.typedisplay,
                        itemdisplay = witem.itemdisplay,
                        bominwardcode = witem.bominwardcode,
                        bominvoiceitemimage = witem.bominvoiceitemimage,
                        mfgworkorderitemsid = witem.mfgworkorderitemsid,
                        mfgworkorderitemsizesid = witem.mfgworkorderitemsizesid,
                        isactive = true,
                        corestageid = sourcejoborder.corestageid
                    };
                    sourcejoborder.Mfgjoborderitems.Add(orderitem);
                }
            }

            if (sourcejoborder.corestageid == 3)
            {
                var existingreceived = mfgjoborderitemsDataService.Search(new MfgjoborderitemsCreteria()
                {
                    mfgworkorderid = sourcejoborder.mfgworkorderid,
                    sourcejoborderitemids = sourcejoborder.Mfgjoborderitems.ConvertAll(x => x.id)
                });
                var currentorderitems = sourcejoborder.Mfgjoborderitems.OrderBy(x => x.id);
                sourcejoborder.Mfgjoborderitems = new List<Mfgjoborderitems>();
                foreach (var witem in currentorderitems)
                {
                    if (witem.oqty <= 0) continue;
                    decimal aqty = 0;
                    if (existingreceived != null && existingreceived.Count > 0)
                    {
                        aqty = existingreceived.Sum(x => x.sourcejoborderitemid == witem.id ? x.rqty : 0);
                    }
                    var orderitem = new Mfgjoborderitems()
                    {
                        sno = (sourcejoborder.Mfgjoborderitems.Count + 1),
                        bomcategoryid = witem.bomcategoryid,
                        isbom = false,
                        bomid = witem.bomid,
                        productid = witem.productid,
                        colourid = witem.colourid,
                        sizeid = witem.sizeid,
                        oqty = witem.oqty,
                        aqty = aqty,
                        sourcejoborderitemid = witem.id,
                        rqty = 0,
                        cqty = 0,
                        typedisplay = witem.typedisplay,
                        itemdisplay = witem.itemdisplay,
                        bominwardcode = witem.bominwardcode,
                        bominvoiceitemimage = witem.bominvoiceitemimage,
                        mfgworkorderid = workorder.id,
                        mfgworkorderitemsid = witem.mfgworkorderitemsid,
                        mfgworkorderitemsizesid = witem.mfgworkorderitemsizesid,
                        isactive = true,
                        corestageid = sourcejoborder.corestageid
                    };
                    sourcejoborder.Mfgjoborderitems.Add(orderitem);
                }
            }
            sourcejoborder.inwardeddate = DateTimeNow.Date;
            sourcejoborder.mfgworkorder = workorder;
            return sourcejoborder;
        }
        internal Mfgjoborder CreateStage(Mfgjoborderstage item)
        {


            if (item.inwardjob && item.joborderid > 0)
            {
                return CreateInward(item);
            }

            var existingorders = Search(new MfgjoborderCreteria()
            {
                mfgworkorderid = item.workorderid,
                workflowid = item.workflowid
            });


            MfgworkfloworchestrationDataService mfgworkfloworchestrationDataService = new MfgworkfloworchestrationDataService() { Context = this.Context };
            var workflow = mfgworkfloworchestrationDataService.Search(new MfgworkfloworchestrationCreteria()
            {
                id = item.workflowid
            }).First();

            Mfgworkfloworchestration.Stages stage = null;
            if (workflow.stages.Exists(e => e.index == item.stageid))
            {
                stage = workflow.stages.Find(e => e.index == item.stageid);
            }

            MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
            var workorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
            {
                id = item.workorderid
            }).First();


            Mfgjoborder mfgjoborder = new Mfgjoborder() { mfgworkorder = workorder };


            mfgjoborder.stageid = item.stageid;
            mfgjoborder.corestageid = item.corestageid;
            mfgjoborder.mfgstageid = stage.mfgstageid;
            mfgjoborder.inwardgoods = stage.supportinward;
            mfgjoborder.mfgworkorderid = workorder.id;
            mfgjoborder.joborderdate = DateTimeNow.Date;
            mfgjoborder.expectedate = DateTimeNow.Date.AddDays(stage.duration);
            mfgjoborder.companyid = workorder.companyid;
            mfgjoborder.workflowid = workflow.id;
            mfgjoborder.workorderno = workorder.workorderno;
            mfgjoborder.mode = workorder.mode;
            mfgjoborder.isactive = true;
            mfgjoborder.createdby = Context.UserID;
            mfgjoborder.modifiedby = Context.UserID;
            mfgjoborder.Mfgjoborderitems = new List<Mfgjoborderitems>();
            if (stage != null)
            {
                mfgjoborder.fromlocationid = stage.storelocation;
                mfgjoborder.sourcematerial = stage.sourcematerial;
                mfgjoborder.stageid = stage.index;
                mfgjoborder.stagename = stage.name;
                if (stage.corestageid == -1)
                {
                    mfgjoborder.isbomprocess = stage.corestageid == -1;
                }
                mfgjoborder.supplierid = workorder.supplierid;
                mfgjoborder.mode = workflow.mode;
                mfgjoborder.outcomematerial = stage.outcomematerial;
                if (stage.corestageid == 2)
                {
                    MfgjoborderitemsDataService mfgworkorderitems = new MfgjoborderitemsDataService() { Context = this.Context };
                    var items = mfgworkorderitems.SearchAll(0, workorder.id);

                    MfgworkorderitemsizesDataService subMfgworkorderitemsizes = new MfgworkorderitemsizesDataService() { Context = this.Context };
                    var Mfgworkorderitemsizes = subMfgworkorderitemsizes.SearchAll(workorder.id);

                    var allavailable = items.FindAll(e => e.corestageid == 1 && e.isbom == false && e.inwardjob).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid });
                    var isssued = items.FindAll(e => e.corestageid == 2 && e.isbom == false && !e.inwardjob).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid }).ToList();
                    foreach (var gitem in allavailable)
                    {
                        var witem = gitem.First();
                        var workorderitem = workorder.Mfgworkorderitems.Find(e => e.id == gitem.Key.mfgworkorderitemsid);
                        //workorderitem.sizelist = Mfgworkorderitemsizes.FindAll(e => e.mfgworkorderitemsid == workorderitem.id);
                        var aqty = gitem.Sum(e => e.rqty);
                        var issueqty = isssued.Sum(e => e.Key.mfgworkorderitemsid == gitem.Key.mfgworkorderitemsid && e.Key.sizeid == gitem.Key.sizeid ? e.Sum(x => x.sourcejoborderitemid == 0 ? x.oqty : 0) : 0);

                        var orderitem = new Mfgjoborderitems()
                        {
                            sno = (mfgjoborder.Mfgjoborderitems.Count + 1),
                            bomcategoryid = witem.bomcategoryid,
                            isbom = false,
                            productid = witem.productid,
                            colourid = witem.colourid,
                            sizeid = witem.sizeid,
                            woqty = workorderitem.sizelist.Find(x => x.sizeid == gitem.Key.sizeid).qty,
                            aqty = aqty - issueqty,
                            rqty = 0,
                            cqty = 0,
                            itemdisplay = workorderitem.designname,
                            typedisplay = "Input Product",
                            bominwardcode = witem.bominwardcode,
                            bominvoiceitemimage = witem.bominvoiceitemimage,
                            mfgworkorderid = witem.mfgworkorderid,
                            mfgworkorderitemsid = witem.mfgworkorderitemsid,
                            mfgworkorderitemsizesid = witem.mfgworkorderitemsizesid,
                            corestageid = stage.corestageid,
                            stageid = mfgjoborder.stageid,
                            isactive = true
                        };
                        mfgjoborder.Mfgjoborderitems.Add(orderitem);
                    }

                }

                if (stage.corestageid == 3)
                {
                    MfgjoborderitemsDataService mfgworkorderitems = new MfgjoborderitemsDataService() { Context = this.Context };
                    var items = mfgworkorderitems.SearchAll(0, workorder.id);
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6)
                    {
                        if (items.Count == 0)
                        {
                            foreach (var mfgjobitem in workorder.Mfgworkorderitems)
                            {
                                foreach (var mfgjobsizeitem in mfgjobitem.sizelist)
                                {
                                    var orderitem = new Mfgjoborderitems()
                                    {
                                        sno = (mfgjoborder.Mfgjoborderitems.Count + 1),
                                        isbom = false,
                                        productid = mfgjobitem.productid,
                                        colourid = mfgjobitem.colourid,
                                        sizeid = mfgjobsizeitem.sizeid,
                                        woqty = mfgjobsizeitem.qty,
                                        aqty = mfgjobsizeitem.qty,
                                        rqty = 0,
                                        cqty = 0,
                                        itemdisplay = mfgjobitem.designname,
                                        typedisplay = "Input Product",
                                        bominwardcode = mfgjobitem.bominwardcode,
                                        bominvoiceitemimage = mfgjobitem.bominvoiceitemimage,
                                        mfgworkorderid = mfgjobitem.mfgworkorderid,
                                        mfgworkorderitemsid = mfgjobsizeitem.mfgworkorderitemsid,
                                        mfgworkorderitemsizesid = mfgjobsizeitem.id,
                                        corestageid = stage.corestageid,
                                        stageid = mfgjoborder.stageid,
                                        isactive = true
                                    };
                                    mfgjoborder.Mfgjoborderitems.Add(orderitem);
                                }
                            }
                        }
                        else
                        {
                            foreach (var mfgjobsizeitemgroup in items.GroupBy(x=>x.mfgworkorderitemsizesid))
                            {
                                var lst = mfgjobsizeitemgroup.OrderByDescending(x => x.id).ToList();
                                Mfgjoborderitems mfgjobsizeitem = lst[0];
                                if (mfgjobsizeitem != null && mfgjobsizeitem.rqty > 0)
                                {
                                    if (!mfgjoborder.Mfgjoborderitems.Exists(x => x.mfgworkorderitemsizesid == mfgjobsizeitem.mfgworkorderitemsizesid))
                                    {
                                        var orderitem = new Mfgjoborderitems()
                                        {
                                            sno = (mfgjoborder.Mfgjoborderitems.Count + 1),
                                            isbom = false,
                                            productid = mfgjobsizeitem.productid,
                                            colourid = mfgjobsizeitem.colourid,
                                            sizeid = mfgjobsizeitem.sizeid,
                                            woqty = mfgjobsizeitem.woqty,
                                            aqty = mfgjobsizeitem.rqty,
                                            rqty = 0,
                                            cqty = 0,
                                            itemdisplay = mfgjobsizeitem.itemdisplay,
                                            typedisplay = mfgjobsizeitem.typedisplay,
                                            bominwardcode = mfgjobsizeitem.bominwardcode,
                                            bominvoiceitemimage = mfgjobsizeitem.bominvoiceitemimage,
                                            mfgworkorderid = mfgjobsizeitem.mfgworkorderid,
                                            mfgworkorderitemsid = mfgjobsizeitem.mfgworkorderitemsid,
                                            mfgworkorderitemsizesid = mfgjobsizeitem.mfgworkorderitemsizesid,
                                            corestageid = stage.corestageid,
                                            stageid = mfgjoborder.stageid,
                                            isactive = true
                                        };
                                        mfgjoborder.Mfgjoborderitems.Add(orderitem);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var allavailable = items.FindAll(e => e.corestageid == 1 && e.isbom == false).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid });
                        var wip = items.FindAll(e => e.corestageid == 2 && e.inwardjob && e.isbom == false).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid }).ToList();

                        var hasprocess = workflow.stages.Exists(x => x.corestageid == 3 && x.index < item.stageid);
                        if (hasprocess)
                        {
                            var laststageindex = item.stageid - 1;
                            wip = items.FindAll(e => e.corestageid == 3 && e.inwardjob && e.stageid == laststageindex && e.isbom == false && e.oqty > 0 && e.rqty > 0).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid }).ToList();
                        }
                        //var available = items.FindAll(e => e.corestageid == 3 && e.isbom == false && e.oqty > 0).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid }).ToList();
                        var isssued = items.FindAll(e => e.corestageid == 3 && e.stageid == item.stageid && !e.inwardjob && e.isbom == false && e.oqty > 0).GroupBy(e => new { e.mfgworkorderitemsid, e.sizeid }).ToList();
                        foreach (var gitem in allavailable)
                        {
                            //var witem = gitem.First();
                            //var workorderitem = workorder.Mfgworkorderitems.Find(e => e.id == gitem.Key.mfgworkorderitemsid);
                            //var aqty = gitem.Sum(e => e.rqty);
                            var wqty = wip.Sum(e => e.Key.mfgworkorderitemsid == gitem.Key.mfgworkorderitemsid && e.Key.sizeid == gitem.Key.sizeid ? e.Sum(x => x.rqty) : 0);
                            //var issueqty = isssued.Sum(e => e.Key.mfgworkorderitemsid == gitem.Key.mfgworkorderitemsid && e.Key.sizeid == gitem.Key.sizeid ? e.Sum(x =>x.rqty) : 0);

                            var witem = gitem.First();
                            var workorderitem = workorder.Mfgworkorderitems.Find(e => e.id == gitem.Key.mfgworkorderitemsid);
                            var aqty = gitem.Sum(e => e.rqty);
                            var issueqty = isssued.Sum(e => e.Key.mfgworkorderitemsid == gitem.Key.mfgworkorderitemsid && e.Key.sizeid == gitem.Key.sizeid ? e.Sum(x => x.sourcejoborderitemid == 0 ? x.oqty : 0) : 0);

                            var orderitem = new Mfgjoborderitems()
                            {
                                sno = (mfgjoborder.Mfgjoborderitems.Count + 1),
                                bomcategoryid = witem.bomcategoryid,
                                isbom = false,
                                productid = witem.productid,
                                colourid = witem.colourid,
                                sizeid = witem.sizeid,
                                woqty = workorderitem.sizelist.Find(x => x.sizeid == gitem.Key.sizeid).qty,
                                aqty = wqty - issueqty,
                                rqty = 0,
                                cqty = 0,
                                itemdisplay = workorderitem.designname,
                                typedisplay = "Input Product",
                                bominwardcode = witem.bominwardcode,
                                bominvoiceitemimage = witem.bominvoiceitemimage,
                                mfgworkorderid = witem.mfgworkorderid,
                                mfgworkorderitemsid = witem.mfgworkorderitemsid,
                                mfgworkorderitemsizesid = witem.mfgworkorderitemsizesid,
                                corestageid = stage.corestageid,
                                stageid = mfgjoborder.stageid,
                                isactive = true
                            };
                            mfgjoborder.Mfgjoborderitems.Add(orderitem);
                        }
                    }
                }
                //foreach(var witem in workorder.Mfgworkorderitems)
                //{
                //    if (witem.sizelist != null && witem.sizelist.Count > 0)
                //    {
                //        foreach (var sizeitem in witem.sizelist)
                //        {
                //            var orderitem = new Mfgjoborderitems()
                //            {
                //                bomcategoryid = witem.bomcategoryid,
                //                bomid = witem.fabricid,
                //                productid = witem.productid,
                //                colourid = witem.colourid,
                //                sizeid = sizeitem.sizeid,
                //                oqty = sizeitem.qty,
                //                rqty = 0,
                //                cqty = 0,
                //                typedisplay = "Input Product",
                //                itemdisplay = witem.designname+"/"+sizeitem.name,
                //                bominwardcode = witem.bominwardcode,
                //                bominvoiceitemimage= witem.bominvoiceitemimage
                //            };
                //            mfgjoborder.Mfgjoborderitems.Add(orderitem);
                //        }
                //    }
                //}
                //if (stage.bomitems != null)
                //{
                //    var bomlist = item.inwardjob ? stage.bomitems.FindAll(e => e.eIOMODE == Mfgworkfloworchestration.IOMODE.ProductOutput ||
                //     e.eIOMODE == Mfgworkfloworchestration.IOMODE.ComponentOutput_1 ||
                //     e.eIOMODE == Mfgworkfloworchestration.IOMODE.ComponentOutput_2) :
                //     stage.bomitems.FindAll(e => e.eIOMODE == Mfgworkfloworchestration.IOMODE.ProductInput ||
                //     e.eIOMODE == Mfgworkfloworchestration.IOMODE.ComponentInput_1 ||
                //     e.eIOMODE == Mfgworkfloworchestration.IOMODE.ComponentInput_2);

                //    foreach (var stageitem in bomlist)
                //    {
                //        var orderitem = new Mfgjoborderitems()
                //        {
                //            bomcategoryid = stageitem.bomcategoryid,
                //            bomid = stageitem.bomid,
                //            productid = stageitem.productid,
                //            colourid = stageitem.colourid,
                //            oqty = 0,
                //            rqty = 0,
                //            cqty = 0,
                //            materialtype = stageitem.iomode,
                //            dynamicbom = stageitem.bomid <= 0
                //        };

                //        decimal sourcevalue = 0;
                //        if (stageitem.eCALCULATIONSOURCE == Mfgworkfloworchestration.CALCULATIONSOURCE.WorkOrder)
                //        {
                //            if (workorder.Mfgworkorderitems.Count > 0)
                //                sourcevalue = workorder.Mfgworkorderitems.Sum(e => e.inqty);
                //        }
                //        else if (stageitem.eCALCULATIONSOURCE == Mfgworkfloworchestration.CALCULATIONSOURCE.PriousOrderIncome)
                //        {
                //            if (existingorders.Exists(e => e.stageid == item.stageid - 1 && e.inwardjob))
                //            {
                //                var sourceorder = existingorders.Find(e => e.stageid == item.stageid - 1 && e.inwardjob);
                //                var sourcestage = sourceorder.Mfgjoborderitems.Find(e => e.e_MaterialType == stageitem.eIOMODE &&
                //                e.bomcategoryid == stageitem.bomcategoryid);
                //                sourcevalue = sourcestage.rqty;
                //            }
                //        }
                //        else if (stageitem.eCALCULATIONSOURCE == Mfgworkfloworchestration.CALCULATIONSOURCE.PriousOrderOutcome)
                //        {
                //            if (existingorders.Exists(e => e.stageid == item.stageid - 1 && !e.inwardjob))
                //            {
                //                var sourceorder = existingorders.Find(e => e.stageid == item.stageid - 1 && !e.inwardjob);
                //                var sourcestage = sourceorder.Mfgjoborderitems.Find(e => e.e_MaterialType == stageitem.eIOMODE &&
                //                                  e.bomcategoryid == stageitem.bomcategoryid);
                //                sourcevalue = sourcestage.oqty;
                //            }
                //        }
                //        if (stageitem.eCALCULATIONMODE == Mfgworkfloworchestration.CALCULATIONMODE.Fixed)
                //        {
                //            if (item.inwardjob)
                //                orderitem.rqty = sourcevalue;
                //            else
                //                orderitem.oqty = sourcevalue;
                //        }
                //        else if (stageitem.eCALCULATIONMODE == Mfgworkfloworchestration.CALCULATIONMODE.Divide)
                //        {
                //            if (item.inwardjob)
                //                orderitem.rqty = sourcevalue / stageitem.calculationvalue;
                //            else
                //                orderitem.oqty = sourcevalue / stageitem.calculationvalue;
                //        }
                //        else if (stageitem.eCALCULATIONMODE == Mfgworkfloworchestration.CALCULATIONMODE.Multiply)
                //        {
                //            if (item.inwardjob)
                //                orderitem.rqty = sourcevalue * stageitem.calculationvalue;
                //            else
                //                orderitem.oqty = sourcevalue * stageitem.calculationvalue;
                //        }
                //        if (item.inwardjob)
                //            orderitem.cqty = orderitem.rqty;
                //        else
                //            orderitem.cqty = orderitem.oqty;
                //        mfgjoborder.Mfgjoborderitems.Add(orderitem);
                //    }
                //}
            }

            return mfgjoborder;
        }

        internal MfgjoborderPrint Print(MfgjoborderCreteria mfgjoborderCreteria)
        {
            if (mfgjoborderCreteria.id == 0) throw new Exception("Please select the job order to print");
            var joborder = Search(new MfgjoborderCreteria()
            {
                id = mfgjoborderCreteria.id
            }).First();

            var printjoborder = JsonConvert.DeserializeObject<MfgjoborderPrint>(JsonConvert.SerializeObject(joborder));
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
            SupplierDataService supplierdatasource = new SupplierDataService() { Context = this.Context };
            printjoborder.company = companyDataService.Search(new CompanyCreteria() { id = printjoborder.companyid }).First();
            printjoborder.location = new CompanyLocation(referencelistDataService.Search(new ReferencelistCreteria() { id = printjoborder.fromlocationid }).First());


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6)
            {
                long mfgsampleorderid = 0;
                string query = @"select mfgworkorder.mfgsampleorderid from mfgjoborder
                inner join mfgworkorder on mfgworkorder.id=mfgworkorderid
                where mfgjoborder.id =" + mfgjoborderCreteria.id;
                using (var reader = ExecuteReader(query, null))
                {
                    if (reader.Read())
                    {
                        mfgsampleorderid = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                    }
                }

                if (mfgsampleorderid > 0)
                {
                    MfgsampleorderDataService mfgsampleorderDataService = new MfgsampleorderDataService() { Context = this.Context };
                    var sampleorder = mfgsampleorderDataService.Search(new MfgsampleorderCreteria()
                    {
                        id = mfgsampleorderid
                    });
                    if (sampleorder.Count == 1)
                        printjoborder.mfgsampleorder = sampleorder[0];
                }
            }


            if (printjoborder.supplierid > 0)
            {
                printjoborder.supplier = supplierdatasource.Search(new SupplierCreteria() { id = joborder.supplierid }).First();
                printjoborder.suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria() { supplierid = printjoborder.supplierid }).First();
            }
            var jobreasons = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = printjoborder.Mfgjoborderitems.ConvertAll(x => x.jobreasonid).Distinct().ToList()
            });


            var jobcharges = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = printjoborder.Mfgjoborderitems.ConvertAll(x => x.jobchargeid).Distinct().ToList()
            });

            var colours = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = printjoborder.Mfgjoborderitems.ConvertAll(x => x.colourid).Distinct().ToList()
            });

            var sizes = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = printjoborder.Mfgjoborderitems.ConvertAll(x => x.sizeid).Distinct().ToList()
            });

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            var products = productsDataService.SearchList(new ProductsCreteria()
            {
                ids = printjoborder.Mfgjoborderitems.ConvertAll(x => x.productid).Distinct().ToList()
            });

            BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
            var bommaster = bOMMasterDataService.Search(new BOMMasterCreteria()
            {
                ids = printjoborder.Mfgjoborderitems.Where(x => x.bomid > 0).ToList().ConvertAll(x => x.bomid).Distinct().ToList()
            });

            printjoborder.Mfgjoborderitems.ForEach(x =>
            {
                x.stagename = joborder.stagename;
                x.parent = printjoborder;
                if (sizes.Exists(y => y.id == x.sizeid))
                {
                    x.sizename = sizes.Find(y => y.id == x.sizeid).name;
                }

                if (colours.Exists(y => y.id == x.colourid))
                {
                    x.colourname = colours.Find(y => y.id == x.colourid).name;
                }

                if (products.Exists(y => y.id == x.productid))
                {
                    x.productname = products.Find(y => y.id == x.productid).name;
                    if (!x.isbom)
                    {
                        if (x.productname.Contains(x.itemdisplay))
                            x.itemdisplay = x.productname;
                        else
                            x.itemdisplay = x.productname + "(" + x.itemdisplay + ")";
                    }
                }
                else if (bommaster.Exists(y => y.id == x.bomid))
                {
                    x.productname = bommaster.Find(y => y.id == x.bomid).name;
                    if (!x.isbom)
                    {
                        if (x.productname.Contains(x.itemdisplay))
                            x.itemdisplay = x.productname;
                        else
                            x.itemdisplay = x.productname + "(" + x.itemdisplay + ")";
                    }
                }

                if (jobreasons.Exists(y => y.id == x.jobreasonid))
                {
                    x.jobreason = jobreasons.Find(y => y.id == x.jobreasonid).name;
                }
                if (jobcharges.Exists(y => y.id == x.jobchargeid))
                {
                    x.jobworkcharge = jobcharges.Find(y => y.id == x.jobchargeid).name;
                }
            });

            printjoborder.Mfgjoborderitems = printjoborder.Mfgjoborderitems.OrderByDescending(X => X.isfabric).ThenByDescending(y => y.rqty).ToList();

            int sno = 1;
            printjoborder.Mfgjoborderitems.ForEach(x =>
            {
                x.sno = sno++;
            });
            return printjoborder;
        }

        const string SQL_Update = @"UPDATE public.mfgjoborder SET expectedate=@expectedate, supplierid=@supplierid, termsandcondition=@termsandcondition,inwardjob=@inwardjob, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, 
        fromlocationid=@fromlocationid, status=@status, tolocationid=@tolocationid, tolerance=@tolerance, freight=@freight, issuedby=@issuedby, receivedby=@receivedby, receivedon=@receivedon, companyid=@companyid, tax=@tax, 
        taxable=@taxable, total=@total, workorderno=@workorderno, mfgworkorderid=@mfgworkorderid, mode=@mode,isbomprocess=@isbomprocess,invoiceno=@invoiceno,invoicedate=@invoicedate,
        stageid=@stageid, joborderdate=@joborderdate, joborderno=@joborderno,jobcardno=@jobcardno, sourcematerial=@sourcematerial, 
        outcomematerial=@outcomematerial,workflowid=@workflowid,inwardedby=@inwardedby,inwardeddate=@inwardeddate ,bominvoiceid=@bominvoiceid,
        bomentryno=@bomentryno,bomrequestid=@bomrequestid
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgjoborder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgjoborder> Search(MfgjoborderCreteria _mfgjoborder)
        {
            List<Mfgjoborder> lst = new List<Mfgjoborder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgjoborder.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgjoborder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_mfgjoborder.mode) && _mfgjoborder.mode != "All")
                        qb.AddParameter("mode", _mfgjoborder.mode, "=");

                    if (!string.IsNullOrEmpty(_mfgjoborder.type) && _mfgjoborder.type != "All")
                        qb.AddParameter("inwardjob", _mfgjoborder.type == "Inward", "=");

                    if (!string.IsNullOrEmpty(_mfgjoborder.workorderno))
                        qb.AddParameter("workorderno", _mfgjoborder.workorderno, "=");

                    if (!string.IsNullOrEmpty(_mfgjoborder.joborderno))
                        qb.AddParameter("joborderno", _mfgjoborder.joborderno, "=");

                    if (_mfgjoborder.fromdate.Year > 2000)
                        qb.AddParameter("joborderdate", "wofromdate", _mfgjoborder.fromdate.Date, ">=");

                    if (_mfgjoborder.todate.Year > 2000)
                        qb.AddParameter("joborderdate", "wotodate", _mfgjoborder.todate.Date.AddDays(1), "<");

                    if (_mfgjoborder.mfgworkorderid > 0)
                        qb.AddParameter("mfgworkorderid", _mfgjoborder.mfgworkorderid, "=");

                    if (_mfgjoborder.mfgworkorderid > 0)
                        qb.AddParameter("mfgworkorderid", _mfgjoborder.supplierid, "=");

                    if (_mfgjoborder.workflowid > 0)
                        qb.AddParameter("workflowid", _mfgjoborder.workflowid, "=");

                    if (_mfgjoborder.corestageid >= 0)
                        qb.AddParameter("corestageid", _mfgjoborder.corestageid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgjoborder omfgjoborder = new Mfgjoborder();
                        omfgjoborder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgjoborder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        omfgjoborder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgjoborder.termsandcondition_json = reader["termsandcondition"] == DBNull.Value ? null : reader["termsandcondition"].ToString();
                        omfgjoborder.stagename = reader["stagename"] == DBNull.Value ? "" : reader["stagename"].ToString();
                        omfgjoborder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgjoborder.inwardjob = reader["inwardjob"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardjob"]);
                        omfgjoborder.inwardgoods = reader["inwardgoods"] == DBNull.Value ? false : Convert.ToBoolean(reader["inwardgoods"]);
                        omfgjoborder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgjoborder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgjoborder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgjoborder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgjoborder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgjoborder.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"]);
                        omfgjoborder.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        omfgjoborder.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"]);
                        omfgjoborder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        omfgjoborder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        omfgjoborder.issuedby = reader["issuedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedby"]);
                        omfgjoborder.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        omfgjoborder.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfgjoborder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfgjoborder.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        omfgjoborder.taxable = reader["taxable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxable"]);
                        omfgjoborder.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        omfgjoborder.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        omfgjoborder.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgjoborder.sourcejobid = reader["sourcejobid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcejobid"]);
                        omfgjoborder.mode = reader["mode"] == DBNull.Value ? "" : reader["mode"].ToString();
                        omfgjoborder.stageid = reader["stageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stageid"]);
                        omfgjoborder.corestageid = reader["corestageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["corestageid"]);
                        omfgjoborder.mfgstageid = reader["mfgstageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mfgstageid"]);
                        omfgjoborder.workflowid = reader["workflowid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workflowid"]);
                        omfgjoborder.joborderdate = reader["joborderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joborderdate"]);
                        omfgjoborder.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        omfgjoborder.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        omfgjoborder.sourcejobno = reader["sourcejobno"] == DBNull.Value ? "" : reader["sourcejobno"].ToString();
                        omfgjoborder.inwardedby = reader["inwardedby"] == DBNull.Value ? 0 : Convert.ToInt32(reader["inwardedby"]);
                        omfgjoborder.inwardeddate = reader["inwardeddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["inwardeddate"]);
                        omfgjoborder.isbomprocess = reader["isbomprocess"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbomprocess"]);
                        omfgjoborder.bominvoiceid = reader["bominvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bominvoiceid"]);

                        omfgjoborder.bomentryno = reader["bomentryno"] == DBNull.Value ? "" : reader["bomentryno"].ToString();
                        omfgjoborder.bomrequestid = reader["bomrequestid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bomrequestid"]);

                        omfgjoborder.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        omfgjoborder.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);

                        omfgjoborder.sourcematerial = reader["sourcematerial"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcematerial"]);
                        omfgjoborder.outcomematerial = reader["outcomematerial"] == DBNull.Value ? 0 : Convert.ToInt32(reader["outcomematerial"]);
                        lst.Add(omfgjoborder);
                    }
                }
                if (_mfgjoborder.mfgworkorderid > 0 || (_mfgjoborder.id > 0 && lst.Count == 1))
                {
                    MfgjoborderitemsDataService subMfgjoborderitems = new MfgjoborderitemsDataService() { Context = this.Context };
                    foreach (var item in lst)
                    {
                        item.Mfgjoborderitems = subMfgjoborderitems.SearchAll(item.id, 0);
                        if (item.corestageid == 1)
                        {
                            if (!item.inwardjob)
                            {

                                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                                item.mfgworkorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                                {
                                    id = item.mfgworkorderid
                                }).First();
                            }
                        }
                        else if (item.corestageid == 2)
                        {
                            if (!item.inwardjob)
                            {
                                MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
                                item.mfgworkorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
                                {
                                    id = item.mfgworkorderid
                                }).First();
                                foreach (var itm in item.Mfgjoborderitems)
                                {
                                    itm.aqty = itm.oqty;
                                    itm.woqty = item.mfgworkorder.Mfgworkorderitems.Sum(x => x.sizelist.Sum(y => y.id == itm.mfgworkorderitemsizesid ? y.qty : 0));
                                }
                            }
                            else
                            {
                                foreach (var itm in item.Mfgjoborderitems)
                                {
                                    itm.aqty = itm.oqty;
                                }
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgjoborder);
            }
            return lst;
        }

        public Mfgjoborder Insert(Mfgjoborder oMfgjoborder)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get(
                        oMfgjoborder.inwardjob ? "mfgjoborder.inwardjoborderno" :
                        "mfgjoborder.joborderno",
                        0,
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ? oMfgjoborder.companyid : 0);
                    oMfgjoborder.joborderno = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oMfgjoborder);
                    MfgjoborderitemsDataService subMfgjoborderitems = new MfgjoborderitemsDataService() { Context = this.Context };

                    foreach (var item in oMfgjoborder.Mfgjoborderitems)
                    {
                        item.mfgjoborderid = oMfgjoborder.id;
                        item.mfgworkorderid = oMfgjoborder.mfgworkorderid;
                        item.corestageid = oMfgjoborder.corestageid;
                        item.inwardgoods = oMfgjoborder.inwardgoods;
                        subMfgjoborderitems.Insert(transaction, conn, item);
                    }

                    MfgworkorderinboxDataService mfgworkorderinboxDataService = new MfgworkorderinboxDataService() { Context = this.Context };
                    mfgworkorderinboxDataService.Insert(oMfgjoborder, conn, transaction);

                    if (oMfgjoborder.inwardjob && oMfgjoborder.corestageid == 3 &&
                        oMfgjoborder.inwardgoods)
                    {
                        string query = @"update mfgworkorderinbox set outqty=outward from (select sum(rqty) outward from mfgjoborderitems
                        inner join mfgjoborder on mfgjoborder.id = mfgjoborderitems.mfgjoborderid
                        where mfgjoborder.corestageid=3 and isbom=false and 
                        mfgjoborder.isactive=true and mfgjoborderitems.isactive=true and
                        mfgjoborder.stageid=@stageid and mfgjoborder.inwardjob=true and
                        mfgjoborderitems.mfgworkorderid = @mfgworkorderid) outwards
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=0 
                        and isactive=true";
                        var param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("stageid", oMfgjoborder.stageid);
                        ExecuteQuery(query, param, conn, transaction);
                    }

                    if (oMfgjoborder.inwardjob && oMfgjoborder.isbomprocess)
                    {
                        string query = @"update mfgworkorderinbox set outqty=outqty+@outqty,inqty=inqty+@inqty
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=0 
                        and isactive=true";
                        var param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("outqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.oqty));
                        param.Add("inqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.supportbominvoice ? x.rqty : 0));
                        ExecuteQuery(query, param, conn, transaction);

                        query = @"update mfgworkorderinbox set outqty=outqty+@outqty,inqty=inqty+@inqty
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=@mfgjoborderid
                        and isactive=true";
                        param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("mfgjoborderid", oMfgjoborder.id);
                        param.Add("outqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.oqty));
                        param.Add("inqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.supportbominvoice ? x.rqty : 0));
                        ExecuteQuery(query, param, conn, transaction);
                    }

                    if (oMfgjoborder.inwardjob &&
                    oMfgjoborder.Mfgjoborderitems.Exists(x => x.isbom && x.bomconsumptionitemid <= 0))
                    {
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Insert(Bomstocktransaction.TransactionType.JobOrdeStageConsumption,
                            conn, transaction, oMfgjoborder, oMfgjoborder.Mfgjoborderitems.FindAll(x => x.isbom && x.bomconsumptionitemid <= 0));
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgjoborder);
                throw exp;
            }

            return oMfgjoborder;
        }

        public Mfgjoborder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgjoborder oMfgjoborder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.expectedate;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.invoicedate;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.invoiceno == null ? "" : oMfgjoborder.invoiceno;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.supplierid;
            command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgjoborder.termsandcondition_json == null ? "null" : oMfgjoborder.termsandcondition_json;
            command.Parameters.Add("stagename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.stagename == null ? "" : oMfgjoborder.stagename;

            command.Parameters.Add("sourcejobno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.sourcejobno == null ? "" : oMfgjoborder.sourcejobno;
            command.Parameters.Add("sourcejobid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.sourcejobid;

            command.Parameters.Add("bomentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.bomentryno == null ? "" : oMfgjoborder.bomentryno;
            command.Parameters.Add("bomrequestid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.bomrequestid;

            command.Parameters.Add("isbomprocess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.isbomprocess;
            command.Parameters.Add("bominvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.bominvoiceid;
            command.Parameters.Add("inwardeddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.inwardeddate;
            command.Parameters.Add("inwardedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.inwardedby;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.isactive;
            command.Parameters.Add("inwardjob", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.inwardjob;
            command.Parameters.Add("inwardgoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.inwardgoods;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.modifiedon;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.fromlocationid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.status;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.tolocationid;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.tolerance;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.freight;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.issuedby;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.receivedon;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.companyid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.taxable;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.total;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.workorderno == null ? "" : oMfgjoborder.workorderno;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.mfgworkorderid;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.mode == null ? "" : oMfgjoborder.mode;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.stageid;
            command.Parameters.Add("corestageid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.corestageid;
            command.Parameters.Add("mfgstageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.mfgstageid;
            command.Parameters.Add("workflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.workflowid;
            command.Parameters.Add("joborderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.joborderdate;
            command.Parameters.Add("joborderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.joborderno == null ? "" : oMfgjoborder.joborderno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.jobcardno == null ? "" : oMfgjoborder.jobcardno;
            command.Parameters.Add("sourcematerial", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.sourcematerial;
            command.Parameters.Add("outcomematerial", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.outcomematerial;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgjoborder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgjoborder;
        }

        public Mfgjoborder Update(Mfgjoborder oMfgjoborder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgjoborder);
                    MfgjoborderitemsDataService subMfgjoborderitems = new MfgjoborderitemsDataService() { Context = this.Context };
                    //if (oMfgjoborder.inwardjob)
                    //{
                    //    foreach (var item in oMfgjoborder.Mfgjoborderitems)
                    //    {
                    //        item.rqty = item.oqty;
                    //        item.oqty = 0;
                    //    }
                    //}
                    subMfgjoborderitems.Update(transaction, conn, oMfgjoborder.Mfgjoborderitems, oMfgjoborder);

                    if (oMfgjoborder.inwardjob && oMfgjoborder.corestageid == 3 &&
    oMfgjoborder.inwardgoods)
                    {
                        string query = @"update mfgworkorderinbox set outqty=outward from (select sum(rqty) outward from mfgjoborderitems
                        inner join mfgjoborder on mfgjoborder.id = mfgjoborderitems.mfgjoborderid
                        where mfgjoborder.corestageid=3 and isbom=false and 
                        mfgjoborder.isactive=true and mfgjoborderitems.isactive=true and
                        mfgjoborder.stageid=@stageid and mfgjoborder.inwardjob=true and
                        mfgjoborderitems.mfgworkorderid = @mfgworkorderid) outwards
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=0 
                        and isactive=true";
                        var param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("stageid", oMfgjoborder.stageid);
                        ExecuteQuery(query, param, conn, transaction);
                    }


                    if (oMfgjoborder.inwardjob && oMfgjoborder.isbomprocess)
                    {
                        string query = @"update mfgworkorderinbox set outqty=@outqty,inqty=@inqty
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=0 
                        and isactive=true";
                        var param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("outqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.oqty));
                        param.Add("inqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.rqty));
                        ExecuteQuery(query, param, conn, transaction);

                        query = @"update mfgworkorderinbox set outqty=@outqty,inqty=@inqty
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=@mfgjoborderid
                        and isactive=true";
                        param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("mfgjoborderid", oMfgjoborder.id);
                        param.Add("outqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.oqty));
                        param.Add("inqty", oMfgjoborder.Mfgjoborderitems.Sum(x => x.rqty));
                        ExecuteQuery(query, param, conn, transaction);
                    }

                    if (oMfgjoborder.inwardjob &&
                        oMfgjoborder.Mfgjoborderitems.Exists(x => x.isbom && x.bomconsumptionitemid <= 0))
                    {
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Update(Bomstocktransaction.TransactionType.JobOrdeStageConsumption,
                            conn, transaction, oMfgjoborder, oMfgjoborder.Mfgjoborderitems.FindAll(x => x.isbom && x.bomconsumptionitemid <= 0));
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgjoborder);
                throw exp;
            }

            return oMfgjoborder;
        }

        public Mfgjoborder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgjoborder oMfgjoborder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.id;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.invoicedate;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.invoiceno == null ? "" : oMfgjoborder.invoiceno;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.expectedate;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.supplierid;
            command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgjoborder.termsandcondition_json == null ? "null" : oMfgjoborder.termsandcondition_json;
            command.Parameters.Add("inwardjob", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.inwardjob;
            command.Parameters.Add("inwardgoods", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.inwardgoods;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.modifiedon;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.fromlocationid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.status;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.tolocationid;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.tolerance;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.freight;
            command.Parameters.Add("issuedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.issuedby;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.receivedby;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.receivedon;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.companyid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.tax;
            command.Parameters.Add("taxable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.taxable;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgjoborder.total;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.workorderno == null ? "" : oMfgjoborder.workorderno;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.mfgworkorderid;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.mode == null ? "" : oMfgjoborder.mode;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.stageid;
            command.Parameters.Add("corestageid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.corestageid;
            command.Parameters.Add("mfgstageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.mfgstageid;
            command.Parameters.Add("workflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.workflowid;
            command.Parameters.Add("joborderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.joborderdate;
            command.Parameters.Add("joborderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.joborderno == null ? "" : oMfgjoborder.joborderno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.jobcardno == null ? "" : oMfgjoborder.jobcardno;
            command.Parameters.Add("sourcematerial", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.sourcematerial;
            command.Parameters.Add("outcomematerial", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.outcomematerial;
            command.Parameters.Add("inwardeddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.inwardeddate;
            command.Parameters.Add("inwardedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.inwardedby;
            command.Parameters.Add("isbomprocess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.isbomprocess;
            command.Parameters.Add("bominvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.bominvoiceid;
            command.Parameters.Add("bomentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgjoborder.bomentryno == null ? "" : oMfgjoborder.bomentryno;
            command.Parameters.Add("bomrequestid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.bomrequestid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgjoborder.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgjoborder;
        }


        public bool Delete(Mfgjoborder oMfgjoborder)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    MfgjoborderitemsDataService subMfgjoborderitems = new MfgjoborderitemsDataService() { Context = this.Context };

                    if (oMfgjoborder.inwardjob)
                    {
                        BomstocktransactionDataService bomstocktransactionDataService = new BomstocktransactionDataService() { Context = this.Context };
                        bomstocktransactionDataService.Delete(Bomstocktransaction.TransactionType.JobOrdeStageConsumption,
                            transaction, conn, oMfgjoborder.id);
                    }

                    hasSuccessCount = Delete(transaction, conn, oMfgjoborder);
                    subMfgjoborderitems.DeleteAll(oMfgjoborder.id, oMfgjoborder.modifiedby, conn, transaction);

                    if (oMfgjoborder.inwardjob && oMfgjoborder.corestageid == 3 &&
                        oMfgjoborder.inwardgoods)
                    {
                        string query = @"update mfgworkorderinbox set outqty=outward from (select sum(rqty) outward from mfgjoborderitems
                        inner join mfgjoborder on mfgjoborder.id = mfgjoborderitems.mfgjoborderid
                        where mfgjoborder.corestageid=3 and isbom=false and 
                        mfgjoborder.isactive=true and mfgjoborderitems.isactive=true and
                        mfgjoborder.stageid=@stageid and mfgjoborder.inwardjob=true and
                        mfgjoborderitems.mfgworkorderid = @mfgworkorderid) outwards
                        where mfgworkorderid = @mfgworkorderid and mfgjoborderid=0 
                        and isactive=true";
                        var param = new SortedList<string, object>();
                        param.Add("mfgworkorderid", oMfgjoborder.mfgworkorderid);
                        param.Add("stageid", oMfgjoborder.stageid);
                        ExecuteQuery(query, param, conn, transaction);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgjoborder);

                throw exp;
            }
            return hasSuccessCount;

        }

        public List<MfgjoborderSummary> Report(MfgjoborderCreteria _mfgworkorder)
        {
            List<MfgjoborderSummary> lst = new List<MfgjoborderSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                var currentQuery = @"
with alljobissued as

(
select Mfgjoborderitems.mfgworkorderid,Mfgworkorderitems.designname,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
	mfgjoborder.stagename,Mfgworkorderitems.productid,mfgjoborder.id,Mfgjoborderitems.itemdisplay,
    sum(oqty) issuedqty from Mfgjoborderitems 
    left outer join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
    left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
    where inwardjob = false and mfgjoborderitems.isactive=true and mfgjoborderitems.isbom = false 
    group by Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,mfgjoborder.stagename,
	Mfgworkorderitems.productid,mfgjoborder.id,Mfgworkorderitems.designname,Mfgjoborderitems.itemdisplay
),alljobreceived as
(
	select Mfgjoborderitems.mfgworkorderid,designitemname,Mfgworkorderitems.designname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,
	mfgjoborder.stagename,Mfgworkorderitems.productid,mfgjoborder.sourcejobid id,Mfgjoborderitems.itemdisplay,
    sum(rqty) receivedqty,sum(cqty) wasteqty from Mfgjoborderitems 
    left outer join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
    left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
    where inwardjob = true and mfgjoborderitems.isactive=true
    group by Mfgjoborderitems.mfgworkorderid,designitemname,Mfgjoborderitems.colourid,mfgjoborder.corestageid,mfgjoborder.stagename,
	Mfgworkorderitems.productid,mfgjoborder.sourcejobid,Mfgworkorderitems.designname,Mfgjoborderitems.itemdisplay
)


select alljobissued.*,colourlist.name colourname,wodate,mfgjoborder.id mfgjoborderid,mfgjoborder.supplierid,
mfgworkorder.workorderno,alljobreceived.receivedqty,alljobreceived.wasteqty,mfgjoborder.joborderno,mfgjoborder.jobcardno,
joborderdate,products.name productname,mfgworkorder.locationid
from alljobissued
inner join mfgworkorder on  mfgworkorder.id =alljobissued.mfgworkorderid
inner join mfgjoborder on  mfgjoborder.id =alljobissued.id
left outer join alljobreceived on alljobreceived.mfgworkorderid = alljobissued.mfgworkorderid
and alljobreceived.itemdisplay = alljobissued.itemdisplay 
and alljobreceived.colourid = alljobissued.colourid
and alljobreceived.corestageid = alljobissued.corestageid 
and alljobreceived.id = alljobissued.id
left outer join referencelist colourlist on colourlist.id =  alljobissued.colourid
left outer join products on products.id =  alljobissued.productid
where mfgjoborder.isactive=true 
{0}";

                if (_mfgworkorder.onlybom)
                {
                    currentQuery = @"

with alljobissued as
(
    select Mfgjoborderitems.mfgworkorderid,Mfgjoborderitems.colourid,
    Mfgjoborderitems.bomid fabricid,jobchargeid,
	mfgjoborder.stagename,mfgjoborder.id,Mfgjoborderitems.itemdisplay,
    sum(oqty) issuedqty,sum(cqty) wasteqty,0::numeric receivedqty  from Mfgjoborderitems 
    left outer join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
    left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
    where inwardjob = false and mfgjoborderitems.isactive=true and mfgjoborderitems.isbom = true 
    --and mfgjoborder.isbomprocess=true 
    and oqty>0
    group by Mfgjoborderitems.mfgworkorderid,Mfgjoborderitems.colourid,
	mfgjoborder.stagename,jobchargeid,
	Mfgjoborderitems.bomid,mfgjoborder.id,
	Mfgjoborderitems.itemdisplay,Mfgworkorderitems.fabricid
union all
	select Mfgjoborderitems.mfgworkorderid,Mfgjoborderitems.colourid,
    Mfgjoborderitems.bomid fabricid,jobchargeid,
	mfgjoborder.stagename,mfgjoborder.id,Mfgjoborderitems.itemdisplay,
    0::numeric issuedqty,sum(cqty) wasteqty,sum(rqty) receivedqty  from Mfgjoborderitems 
    left outer join Mfgworkorderitems on Mfgworkorderitems.id = mfgworkorderitemsid
    left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
    where inwardjob = true and mfgjoborderitems.isactive=true and mfgjoborderitems.isbom = true 
    --and mfgjoborder.isbomprocess=true 
    and supportbominvoice = true and rqty>0
    group by Mfgjoborderitems.mfgworkorderid,Mfgjoborderitems.colourid,
	mfgjoborder.stagename,jobchargeid,
	Mfgjoborderitems.bomid,mfgjoborder.id,
	Mfgjoborderitems.itemdisplay,Mfgworkorderitems.fabricid
)


select alljobissued.*,colourlist.name colourname,wodate,mfgjoborder.id mfgjoborderid,mfgjoborder.supplierid,
mfgworkorder.workorderno,jobwork.name jobworkname,
0::numeric receivedqty,mfgjoborder.joborderno,mfgjoborder.jobcardno,joborderdate,
bommaster.name ||' - '||bommaster.printingname productname,mfgworkorder.locationid
from alljobissued
inner join mfgworkorder on  mfgworkorder.id =alljobissued.mfgworkorderid
inner join mfgjoborder on  mfgjoborder.id =alljobissued.id
left outer join referencelist colourlist on colourlist.id =  alljobissued.colourid
left outer join referencelist jobwork on jobwork.id = alljobissued.jobchargeid
left outer join bommaster on bommaster.id =  alljobissued.fabricid
where mfgjoborder.isactive=true 
{0}";
                }

                List<string> conditionlist = new List<string>();

                //if (_mfgworkorder.corestageid == 1)
                //{
                //    //currentQuery = currentQuery.Replace(" and isbom=false ", "");
                //    conditionlist.Add("alljobissued.corestageid=@corestageid");
                //}

                if (_mfgworkorder.corestageid > 0)
                    conditionlist.Add("(alljobissued.corestageid=@corestageid or alljobissued.corestageid=@corestageid)");

                if (_mfgworkorder.showonlypending)
                    conditionlist.Add("Mfgworkorder.statusid != 9");

                if (_mfgworkorder.showonlypending)
                    conditionlist.Add("mfgjoborder.status != 9");

                if (_mfgworkorder.supplierid > 0)
                    conditionlist.Add("mfgjoborder.supplierid=@supplierid");

                if (_mfgworkorder.fromdate.Year > 2000)
                    conditionlist.Add("joborderdate>=@joborderdatefromdate");

                if (_mfgworkorder.todate.Year > 2000)
                    conditionlist.Add("joborderdate<@joborderdatetodate");

                if (_mfgworkorder.mode == "Internal")
                    conditionlist.Add("mfgjoborder.mode = 'Internal'");

                if (_mfgworkorder.mode == "External")
                    conditionlist.Add("mfgjoborder.mode = 'External'");

                if (!string.IsNullOrEmpty(_mfgworkorder.design))
                {
                    string designid = string.Join(",", WebAPI.Data.Utility.SafeSQL(_mfgworkorder.design).Split(',').ToList().ConvertAll(x => "'" + x + "'"));
                    conditionlist.Add("alljobissued.designname in (" + designid + ")");
                }

                if (_mfgworkorder.locationid > 0)
                    conditionlist.Add("mfgjoborder.fromlocationid=@locationid");


                if (_mfgworkorder.productid > 0)
                    conditionlist.Add("alljobissued.productid=@productid");

                if (_mfgworkorder.showonlypending && !_mfgworkorder.onlybom)
                    conditionlist.Add("(alljobreceived.receivedqty is null or alljobreceived.receivedqty<alljobissued.issuedqty)");

                if (conditionlist.Count > 0)
                {
                    qb.CurrentQuery = string.Format(currentQuery, " and " + string.Join(" and ", conditionlist));
                }
                else
                {
                    qb.CurrentQuery = string.Format(currentQuery, "");
                }

                command = qb.GetCommand(conn);
                command.Parameters.AddWithValue("supplierid", _mfgworkorder.supplierid);
                command.Parameters.AddWithValue("productid", _mfgworkorder.productid);
                command.Parameters.AddWithValue("locationid", _mfgworkorder.locationid);
                command.Parameters.AddWithValue("joborderdatefromdate", _mfgworkorder.fromdate.Date);
                command.Parameters.AddWithValue("joborderdatetodate", _mfgworkorder.todate.Date.AddDays(1));
                command.Parameters.AddWithValue("corestageid", _mfgworkorder.corestageid);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        MfgjoborderSummary omfgworkorder = new MfgjoborderSummary();
                        omfgworkorder.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkorder.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        if (!_mfgworkorder.onlybom)
                            omfgworkorder.designitemname = reader["designitemname"] == DBNull.Value ? "" : reader["designitemname"].ToString();

                        omfgworkorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgworkorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();

                        omfgworkorder.product = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        if (string.IsNullOrEmpty(omfgworkorder.product))
                            omfgworkorder.product = reader["itemdisplay"] == DBNull.Value ? "" : reader["itemdisplay"].ToString();
                        if (!_mfgworkorder.onlybom && string.IsNullOrEmpty(omfgworkorder.designitemname))
                            omfgworkorder.designitemname = reader["itemdisplay"] == DBNull.Value ? "" : reader["itemdisplay"].ToString();

                        omfgworkorder.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        omfgworkorder.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        omfgworkorder.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        omfgworkorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgworkorder.wodate = reader["wodate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["wodate"]);
                        omfgworkorder.joborderdate = reader["joborderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joborderdate"]);
                        omfgworkorder.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        omfgworkorder.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        omfgworkorder.wasteqty = reader["wasteqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wasteqty"]);
                        omfgworkorder.stage = reader["stagename"] == DBNull.Value ? "" : reader["stagename"].ToString();


                        if (!_mfgworkorder.onlybom)
                        {
                            omfgworkorder.corestageid = reader["corestageid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["corestageid"]);
                        }
                        else
                        {
                            omfgworkorder.jobworkname = reader["jobworkname"] == DBNull.Value ? "" : reader["jobworkname"].ToString();
                            if (string.IsNullOrEmpty(omfgworkorder.jobworkname))
                            {
                                omfgworkorder.jobworkname = omfgworkorder.receivedqty > 0 ? "Received" : "Issued";
                            }
                        }
                        lst.Add(omfgworkorder);
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorder);
            }
            return lst;
        }


        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgjoborder oMfgjoborder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgjoborder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgjoborder.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgjoborder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgjoborder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<BOMPrint2Print> GenerateBarcode(List<BOMPrint2Print> newbarcodelist)
        {
            SortedDictionary<long, string> existingbarcodelist = new SortedDictionary<long, string>();
            var bominwardsequence = SequenceManager.Instance(Context.OrganizationID).Get("bominvoiceitem.bominwardcode", 0);

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    using (var reader = ExecuteReader("select id,bominwardcode from mfgjoborderitems where id in (" + String.Join(",", newbarcodelist.ConvertAll(x => x.id)) + ")", null))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(1) || string.IsNullOrEmpty(reader.GetString(1)) || reader.GetString(1) == "0") continue;
                            var id = reader.GetInt64(0);
                            newbarcodelist.Find(x => x.id == id).bominwardcode = reader.GetString(1);
                        }
                    }

                    transaction = conn.BeginTransaction();
                    foreach (var item in newbarcodelist.FindAll(x => string.IsNullOrEmpty(x.bominwardcode)).GroupBy(e => e.bomid))
                    {
                        var items = item.ToList();
                        if (items.Exists(e => !string.IsNullOrEmpty(e.bominwardcode)))
                        {
                            var maxid = items.Max(e =>
                            {
                                if (string.IsNullOrEmpty(e.bominwardcode)) return 0;
                                int intout = 0;
                                var bomcodes = e.bominwardcode.Split('-');
                                if (bomcodes.Length == 2 && int.TryParse(e.bominwardcode.Split('-')[1], out intout)) return intout;
                                return 0;
                            });
                            items = items.FindAll(e => string.IsNullOrEmpty(e.bominwardcode));
                            var uniqecode = bominwardsequence.GetNextNumber(Context, conn, transaction);
                            for (var i = maxid; i < items.Count; i++)
                            {
                                items[i].bominwardcode = uniqecode + "-" + (i + 1);
                            }
                        }
                        else
                        {
                            var uniqecode = bominwardsequence.GetNextNumber(Context, conn, transaction);
                            for (var i = 0; i < items.Count; i++)
                            {
                                items[i].bominwardcode = uniqecode + "-" + (i + 1);
                            }
                        }
                    }

                    foreach (var uitem in newbarcodelist)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("bominwardcode", uitem.bominwardcode);
                        param.Add("id", uitem.id);
                        ExecuteQuery("update mfgjoborderitems set bominwardcode=@bominwardcode where id=@id", param, conn, transaction);
                    }
                    transaction.Commit();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, newbarcodelist);
            }

            return newbarcodelist;
        }
    }

}
