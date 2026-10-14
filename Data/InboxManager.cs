using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Common;
using InfyPOS.Models;
using Npgsql;

namespace InfyPOS.Data
{
    public class InboxManager
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<long, InboxManager> instance = new System.Collections.Concurrent.ConcurrentDictionary<long, InboxManager>();

        public static InboxManager Instance(long organizationid)
        {

            if (instance.ContainsKey(organizationid))
                return instance[organizationid];

            InboxManager sellablebarcodesData = new InboxManager(organizationid);
            instance.TryAdd(organizationid, sellablebarcodesData);
            return sellablebarcodesData;
        }

        Data.InboxDataService dataService;
        private long organizationid;

        public InboxManager(long organizationid)
        {
            this.organizationid = organizationid;
            dataService = new InboxDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
        }

        private Models.Inbox Create(UserClaim Context, Models.Inbox.Types type, long referenceid)
        {
            Models.Inbox entity = new Models.Inbox();
            entity.addedon = Context.DateTimeNow;
            entity.createdby = 0;
            entity.createdon = Context.DateTimeNow;
            entity.createdusername = "";
            entity.isactive = true;
            entity.referenceid = referenceid;
            entity.Type = type;
            entity.version = 1;
            entity.Status = Models.Inbox.StatusList.Open;
            return entity;
        }

        public bool UpdateLRStatus(long refrenceid, Inbox.StatusList status,
            Npgsql.NpgsqlTransaction transaction, 
            Npgsql.NpgsqlConnection conn,
            UserClaim Context)
        {
            Models.InboxCreteria search = new Models.InboxCreteria();
            search.referenceid = refrenceid;
            search.Type = Models.Inbox.Types.LREntry;
            if (refrenceid == 0) return false;
            var items = dataService.Search(search,conn,transaction);
            if (items.Count == 1)
            {
                if (items[0].Status < status || items[0].Status == Inbox.StatusList.Item_Pricing)
                {
                    items[0].Status = status;
                    //UpdateLRStatus(refrenceid, status, transaction, conn);
                    dataService.Update(transaction, conn, items[0]);
                    if(status == Inbox.StatusList.Barcode_Generating)
                    {
                        if(Services.ConfigManager.Instance(dataService.Context.OrganizationID).Config.intray_notification_barcode)
                        {
                            Services.IntrayManager.Instance(dataService.Context.OrganizationID).Add(dataService.Context.OrganizationID,
                                Services.IntrayManager.EntryType.BarcodeGeneration,
                            items[0].id, string.Format("Barcode Generated {0} ", items[0].lrentryno),
                            dataService.Context.UserID, items[0].locationid, transaction,Context);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public bool Update(UserClaim Context, bool isNew, Models.Lrentry source, Npgsql.NpgsqlTransaction transaction, Npgsql.NpgsqlConnection conn)
        {
            Models.Inbox entity = null;
            if (!isNew)
            {
                Models.InboxCreteria search = new Models.InboxCreteria();
                search.referenceid = source.id;
                search.Type = Models.Inbox.Types.LREntry;
                var items = dataService.Search(search,conn, transaction);
                if (items.Count >= 1)
                {
                    entity = items[0];
                    entity.supplierid = source.supplierid;
                    if (entity.Status < source.Status)
                        entity.Status = source.Status;
                }
            }
            
            if(entity == null)
            {
                entity = Create(Context,Models.Inbox.Types.LREntry, source.id);
                entity.referenceno = source.lrno;
                entity.Status = Inbox.StatusList.New_LR;
                entity.referencedate = source.lrdate;
                entity.supplierid = source.supplierid;
            }
            if (entity.supplierid != source.supplierid)
            {
                entity.supplierid = source.supplierid;
                entity.suppliername = "";
            }
            entity.referenceno = source.lrno;
            entity.referencedate = source.lrdate;
            entity.lrcompanyid = source.lrcompanyid;
            entity.locationid = source.locationid;
            entity.modifiedby = source.modifiedby;
            entity.createdby = source.createdby;
            entity.modifiedon = Context.DateTimeNow;
            entity.modeofdelivery = source.modeofdelivery;
            entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
            if (entity.hasclosed)
            {
                entity.closedby = 0;
                entity.closedon = Context.DateTimeNow;
                entity.closeduser = "";
            }
            if (entity.id == 0)
                dataService.Insert(transaction, conn, entity, Context);
            else
                dataService.Update(transaction, conn, entity);
            return true;
        }


        //public bool Update(bool isNew, Models.Purchaseorder source, Npgsql.NpgsqlTransaction transaction, Npgsql.NpgsqlConnection conn)
        //{
        //    Models.Inbox entity = null;
        //    if (!isNew)
        //    {
        //        Models.InboxCreteria search = new Models.InboxCreteria();
        //        search.referenceid = source.id;
        //        search.Type = Models.Inbox.Types.PurchaseOrder;
        //        var items = dataService.Search(search, conn, transaction);
        //        if (items.Count >= 1)
        //        {
        //            entity = items[0];
        //            entity.supplierid = source.supplierid;
        //            if (entity.Status < source.status)
        //                entity.Status = source.Status;
        //        }
        //    }
        //    else
        //    {
        //        entity = Create(Models.Inbox.Types.LREntry, source.id);
        //        entity.referenceno = source.lrno;
        //        entity.Status = Inbox.StatusList.New_LR;
        //        entity.referencedate = source.lrdate;
        //    }
        //    if (entity.supplierid != source.supplierid)
        //    {
        //        entity.supplierid = source.supplierid;
        //        entity.suppliername = "";
        //    }
        //    entity.modifiedby = source.modifiedby;
        //    entity.createdby = source.createdby;
        //    entity.modifiedon = Context.DateTimeNow;
        //    entity.modeofdelivery = source.modeofdelivery;
        //    entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
        //    if (entity.hasclosed)
        //    {
        //        entity.closedby = 0;
        //        entity.closedon = Context.DateTimeNow;
        //        entity.closeduser = "";
        //    }
        //    if (entity.id == 0)
        //        dataService.Insert(transaction, conn, entity);
        //    else
        //        dataService.Update(transaction, conn, entity);
        //    return true;
        //}

        internal void Update(UserClaim Context, Lrinvoice item, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = Create(Context,Models.Inbox.Types.LREntry, item.lrid);

            Models.InboxCreteria search = new Models.InboxCreteria();
            search.referenceid = item.lrid;
            search.Type = Models.Inbox.Types.LREntry;
            var items = dataService.Search(search,conn, transaction);
            if (items.Count == 1)
            {
                entity = items[0];
                if (entity.Status < Inbox.StatusList.Invoice_Preparation)
                {
                    entity.Status = Models.Inbox.StatusList.Invoice_Preparation;
                    entity.referencedate = item.invoicedate;
                }
            }
            else
            {
                entity.referenceid = search.referenceid;
                entity.Type = search.Type;
                entity.referenceno = item.invoiceno;
                entity.Status = Models.Inbox.StatusList.Invoice_Preparation;
                entity.createdby = item.createdby;
                entity.referencedate = item.invoicedate;
            }

            entity.modifiedby = item.modifiedby;
            entity.modifiedon = Context.DateTimeNow;
            entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
            if (entity.hasclosed)
            {
                entity.closedby = 0;
                entity.closedon = Context.DateTimeNow;
                entity.closeduser = "";
            }
            if (entity.id > 0)
                dataService.Update(transaction, conn, entity);
            else
                dataService.Insert(transaction, conn, entity,Context);
        }


        internal void Update(UserClaim Context, LrinvoiceCover oLrinvoice, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            foreach (var item in oLrinvoice.Items)
                Update(Context,item, transaction, conn);
        }

        internal void Update(UserClaim Context, bool createNew, BOMInvoicereturn oLrinvoicereturn, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = null;

            if (!createNew)
            {
                Models.InboxCreteria search = new Models.InboxCreteria();
                search.referenceid = oLrinvoicereturn.id;
                search.Type = Models.Inbox.Types.PurchaseReturn;
                var items = dataService.Search(search, conn, transaction);
                if (items.Count > 0)
                {
                    entity = items[0];
                }
            }
            
            if(entity == null)
            {
                entity = Create(Context, Models.Inbox.Types.PurchaseReturn, oLrinvoicereturn.id);
                entity.referenceid = oLrinvoicereturn.id;
                entity.Type = Models.Inbox.Types.PurchaseReturn;
                entity.referenceno = oLrinvoicereturn.returnno;
                entity.addedon = oLrinvoicereturn.createdon;
            }
            if (entity != null)
            {
                entity.Status = 0;
                entity.referencedate = oLrinvoicereturn.returndate;
                entity.supplierid = oLrinvoicereturn.supplierid;
                entity.lrcompanyid = oLrinvoicereturn.companyid;
                entity.modifiedby = oLrinvoicereturn.modifiedby;
                entity.createdby = oLrinvoicereturn.createdby;
                entity.createdusername = "";
                entity.modifiedon = Context.DateTimeNow;
                entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;

                if (createNew)
                    dataService.Insert(transaction, conn, entity, Context);
                else
                    dataService.Update(transaction, conn, entity);
            }
        }
        internal void Update(UserClaim Context, bool createNew, Lrinvoicereturn oLrinvoicereturn, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = null;

            if (!createNew)
            {
                Models.InboxCreteria search = new Models.InboxCreteria();
                search.referenceid = oLrinvoicereturn.id;
                search.Type = Models.Inbox.Types.PurchaseReturn;
                var items = dataService.Search(search, conn, transaction);
                if (items.Count > 0)
                {
                    entity = items[0];
                }
            }

            if (entity == null)
            {
                entity = Create(Context, Models.Inbox.Types.PurchaseReturn, oLrinvoicereturn.id);
                entity.referenceid = oLrinvoicereturn.id;
                entity.Type = Models.Inbox.Types.PurchaseReturn;
                entity.referenceno = oLrinvoicereturn.returnno;
                entity.addedon = oLrinvoicereturn.createdon;
            }
            if (entity != null)
            {
                entity.Status = 0;
                entity.referencedate = oLrinvoicereturn.returndate;
                entity.supplierid = oLrinvoicereturn.supplierid;
                entity.lrcompanyid = oLrinvoicereturn.companyid;
                entity.modifiedby = oLrinvoicereturn.modifiedby;
                entity.createdby = oLrinvoicereturn.createdby;
                entity.createdusername = "";
                entity.modifiedon = Context.DateTimeNow;
                entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
                
                if (createNew)
                    dataService.Insert(transaction, conn, entity,Context);
                else
                    dataService.Update(transaction, conn, entity);
            }
        }

        internal void Update(UserClaim Context, bool createNew, Courier oCourier, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = null;

            if (!createNew)
            {
                Models.InboxCreteria search = new Models.InboxCreteria();
                search.referenceid = oCourier.id;
                search.Type = Models.Inbox.Types.Courier;
                var items = dataService.Search(search,conn, transaction);
                if (items.Count > 0)
                {
                    entity = items[0];
                }
            }

            if (entity == null)
            {
                entity = Create(Context,Models.Inbox.Types.Courier, oCourier.id);
                entity.referenceid = oCourier.id;
                entity.Type = Models.Inbox.Types.Courier;
                entity.referenceno = oCourier.podnumber;
                entity.addedon = oCourier.createdon;
            }
            if (entity != null)
            {
                entity.transportid = oCourier.companyid;
                entity.Status = (Inbox.StatusList)(int)oCourier.status;
                if(entity.Status == Inbox.StatusList.Received && oCourier.ishold)
                {
                    entity.Status = Inbox.StatusList.ReceivedHold;
                }
                entity.referencedate = oCourier.receivedon;
                entity.supplierid = oCourier.supplierid;
                entity.lrcompanyid = oCourier.receivingcompanyid;
                entity.modifiedby = oCourier.modifiedby;
                entity.createdby = oCourier.createdby;
                entity.modifiedon = Context.DateTimeNow;
                entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
                if (entity.hasclosed)
                {
                    entity.closedby = oCourier.modifiedby;
                    entity.closedon = Context.DateTimeNow;
                    entity.closeduser = "";
                }
                if (createNew)
                    dataService.Insert(transaction, conn, entity,Context);
                else
                    dataService.Update(transaction, conn, entity);
            }
        }

        internal void Update(UserClaim Context, Lrreceived oLrreceived, receiptitem item, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = Create(Context, Models.Inbox.Types.LREntry, item.lrid);
            Models.InboxCreteria search = new Models.InboxCreteria();
            search.Type = Models.Inbox.Types.LREntry;
            search.referenceid = item.lrid;
            var items = dataService.Search(search,conn, transaction);
            if (items.Count == 1)
            {
                entity = items[0];
                if(item.isreversed)
                    entity.Status = Models.Inbox.StatusList.New_LR;
                else
                    entity.Status = Models.Inbox.StatusList.LRReceived;
                entity.lrcompanyid = oLrreceived.companyid;
                entity.modifiedby = oLrreceived.modifiedby;
                entity.modifiedon = Context.DateTimeNow;
                dataService.Update(transaction, conn, entity);
            }
        }

        internal void Update(UserClaim Context, Lrissue oLrissue, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Update(Context, 0, oLrissue, transaction, conn);
        }

        internal void Update(UserClaim Context, Lrreceived oLrreceived, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Update(Context, 0, oLrreceived, transaction, conn);
        }
        internal void Update(UserClaim Context, long lrentryid, Lrissue oLrissue, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = Create(Context, Models.Inbox.Types.LRIssue, oLrissue.id);
            Models.InboxCreteria search = new Models.InboxCreteria();
            search.referenceid = oLrissue.id;
            search.Type = Models.Inbox.Types.LRIssue;
            var items = dataService.Search(search,conn, transaction);
            List<long> supplierids = new List<long>();
            oLrissue.lrissuelist.ForEach(e => supplierids.Add(e.supplierid));

            if (items.Count == 1)
            {
                entity = items[0];
                entity.Type = Models.Inbox.Types.LRIssue;
                entity.referenceno = oLrissue.issueno;
                entity.Status = Models.Inbox.StatusList.LRIssue;
                entity.suppliername = dataService.GetSupplierName(conn, supplierids.ToArray());
            }
            else
            {
                foreach(var lrissueitem in oLrissue.lrissuelist)
                {
                    search.referenceid = lrissueitem.lrid;
                    search.Type = Models.Inbox.Types.LREntry;
                    items = dataService.Search(search,conn, transaction);
                    if (items.Count == 1)
                    {
                        items[0].Status = Inbox.StatusList.LRIssue;
                        items[0].referencedate = oLrissue.issuedate;
                        items[0].modifiedby = oLrissue.modifiedby;
                        items[0].modifiedon = Context.DateTimeNow;
                        dataService.Update(transaction, conn, items[0]);
                        entity.locationid = items[0].locationid;
                    }
                }
                entity.suppliername = dataService.GetSupplierName(conn, supplierids.ToArray());
                entity.referenceid = oLrissue.id;
                entity.Type = Models.Inbox.Types.LRIssue;
                entity.referenceno = oLrissue.issueno;
                entity.Status = Models.Inbox.StatusList.LRIssue;
            }
            entity.referencedate = oLrissue.issuedate;
            entity.modifiedby = oLrissue.modifiedby;
            entity.modifiedon = Context.DateTimeNow;
            entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
            entity.lrcompanyid = oLrissue.companyid;
            if (entity.hasclosed)
            {
                entity.closedby = 0;
                entity.closedon = Context.DateTimeNow;
                entity.closeduser = "";
            }
            if (entity.id == 0)
                dataService.Insert(transaction, conn, entity,Context);
            else
                dataService.Update(transaction, conn, entity);
        }


        internal void Update(UserClaim Context, long lrissueid, Lrreceived oLrreceived, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.Inbox entity = Create(Context, Models.Inbox.Types.LRReceived, oLrreceived.id);
            Models.InboxCreteria search = new Models.InboxCreteria();
            search.referenceid = oLrreceived.id;
            search.Type = Models.Inbox.Types.LRReceived;
            var items = dataService.Search(search,conn, transaction);
            if (items.Count == 1)
            {
                entity = items[0];
                entity.Type = Models.Inbox.Types.LRReceived;
                entity.referenceno = oLrreceived.receiptno;
                entity.Status = Models.Inbox.StatusList.Received;
            }
            else
            {
                search.referenceid = lrissueid;
                search.Type = Models.Inbox.Types.LRIssue;
                items = dataService.Search(search,conn, transaction);
                if (items.Count == 1)
                {
                    items[0].Status = Inbox.StatusList.Completed;
                    items[0].referencedate = oLrreceived.receivedon;
                    items[0].modifiedby = oLrreceived.modifiedby;
                    items[0].modifiedon = Context.DateTimeNow;
                    items[0].hasclosed = true;
                    entity.locationid = items[0].locationid;
                    dataService.Update(transaction, conn, items[0]);
                }
                entity.referenceid = oLrreceived.id;
                entity.Type = Models.Inbox.Types.LRReceived;
                entity.referenceno = oLrreceived.issueno;
                entity.Status = Models.Inbox.StatusList.Completed;
            }
            entity.referencedate = oLrreceived.receivedon;
            entity.modifiedby = oLrreceived.modifiedby;
            entity.modifiedon = Context.DateTimeNow;
            entity.hasclosed = entity.Status == Models.Inbox.StatusList.Completed;
            if (entity.hasclosed)
            {
                entity.closedby = 0;
                entity.closedon = Context.DateTimeNow;
                entity.closeduser = "";
            }
            if (entity.id == 0)
                dataService.Insert(transaction, conn, entity,Context);
            else
                dataService.Update(transaction, conn, entity);

            foreach(var recitem in oLrreceived.receiptitem)
            {
                search.referenceid = recitem.lrid;
                search.Type = Models.Inbox.Types.LREntry;
                items = dataService.Search(search, conn, transaction);
                if (items.Count == 1)
                {
                    items[0].Status = Inbox.StatusList.LRReceived;
                    items[0].referencedate = oLrreceived.receivedon;
                    items[0].modifiedby = oLrreceived.modifiedby;
                    items[0].modifiedon = Context.DateTimeNow;
                    dataService.Update(transaction, conn, items[0]);
                }
            }
        }

        internal void Delete(UserClaim Context, Lrreceived oLrreceived, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.InboxCreteria creteria = new InboxCreteria()
            {
                Type = Inbox.Types.LRReceived,
                referenceid = oLrreceived.id
            };
            var items = dataService.Search(creteria,conn, transaction);
            if (items.Count == 1)
                dataService.Delete(items[0]);
        }

        internal void Delete(UserClaim Context, Lrentry oLrentry, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.InboxCreteria creteria = new InboxCreteria()
            {
                Type = Inbox.Types.LREntry,
                referenceid = oLrentry.id
            };
            var items = dataService.Search(creteria,conn, transaction);
            if (items.Count == 1)
                dataService.Delete(items[0]);
        }

        internal void Delete(Lrissue oLrissue, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.InboxCreteria creteria = new InboxCreteria()
            {
                Type = Inbox.Types.LRIssue,
                referenceid = oLrissue.id
            };
            var items = dataService.Search(creteria,conn, transaction);
            if (items.Count == 1)
                dataService.Delete(items[0]);
        }

        internal void Delete(UserClaim Context, Courier oCourier, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            Models.InboxCreteria creteria = new InboxCreteria()
            {
                Type = Inbox.Types.Courier,
                referenceid = oCourier.id
            };
            var items = dataService.Search(creteria,conn, transaction);
            if (items.Count == 1)
                dataService.Delete(items[0]);
        }
        
        public List<Inbox> Search(UserClaim Context, InboxCreteria search)
        {
            return dataService.Search(search);
        }

        public List<Inbox> ChangeStatus(UserClaim Context, List<InBoxStatus> lstsource,UserClaim context)
        {
            foreach (var source in lstsource)
            {
                Models.InboxCreteria search = new Models.InboxCreteria();
                search.id = source.id;

                var items = dataService.Search(search);
                if (items.Count == 1)
                {
                    var entity = items[0];
                    if(entity.Type == Inbox.Types.Courier &&
                        entity.Status == Inbox.StatusList.ReceivedHold && source.statusid == 2)
                    {
                        CourierDataService courierDataService = new CourierDataService() { Context = context };
                        var courier = courierDataService.Search(new CourierCreteria()
                        {
                            id = entity.referenceid
                        });
                        if(courier.Count == 1)
                        {
                            courier[0].ishold = false;
                            courier[0].modifiedby = context.UserID;
                            courierDataService.Update(courier[0]);
                        }
                    }
                    entity.closedby = source.closedby;
                    entity.closedon = Context.DateTimeNow;
                    entity.closeduser = source.closeduser;
                    entity.hasclosed = source.hasclosed;
                    entity.id = source.id;
                    entity.notes = source.notes;
                    entity.statusid = source.statusid;
                    entity.statusname = source.statusname;
                    entity.modifiedon = Context.DateTimeNow;
                    entity.modifiedby = context.UserID;
                    dataService.Update(entity);
                }
            }
            return null;
        }
    }
}