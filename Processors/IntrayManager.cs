using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Services
{
    public class IntrayManager
    {


        private long organizationid;
        public IntrayManager(long organizationid)
        {
            this.organizationid = organizationid;
            this.dataService = new Data.IntrayDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<long, IntrayManager> instanceList = new System.Collections.Concurrent.ConcurrentDictionary<long, IntrayManager>();

        public static IntrayManager Instance(long organizationid)
        {
            IntrayManager instance = null;
            if (instanceList.TryGetValue(organizationid, out instance))
                return instance;

            instance = new IntrayManager(organizationid);
            if (!instanceList.TryAdd(organizationid, instance))
                instanceList.TryGetValue(organizationid, out instance);
            return instance;
        }

        public enum EntryType
        {
            None=0,
            Supplier = 1,
            Agent = 2,
            Product = 3,
            Inward = 4,
            Task = 5,
            Discount = 6,
            Notification = 7,
            PurchaseOrder = 8,
            Brand = 9,
            Attributes = 10,
            BarcodeGeneration = 11,
            SalesOrder = 12,
            Customer = 13,
            MfgDesign = 14,
            BomMaster = 15,
            BomPO = 16,
            Transfer = 17,
            Ledger = 18,
            Purchase = 19,
            PurchaseReturn = 20
        }

        public enum ActionType
        {
            Open = 1,
            Close = 2,
            Rejected = 3,
            Approved = 4
        }

        Data.IntrayDataService dataService = null;
        public void Add(long orgnizationid, EntryType type, long id, string description, long userid,
        long locationid,  System.Data.Common.DbTransaction transaction, Common.UserClaim Cotext)
        {
            Add(orgnizationid, type, id, description, userid,
             locationid, 0, transaction, Cotext);
        }

        public void Add(long orgnizationid,EntryType type,long id,string description,long userid,
            long locationid,long companyid,System.Data.Common.DbTransaction transaction, Common.UserClaim Cotext)
        {
            switch(type)
            {
                case EntryType.Agent:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_agent)
                        return;
                    break;
                case EntryType.Inward:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_notification_inward)
                        return;
                    break;
                case EntryType.Product:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_product)
                        return;
                    break;
                case EntryType.SalesOrder:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_salesorder)
                        return;
                    break;
                case EntryType.Customer:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_customer)
                        return;
                    break;
                case EntryType.BomMaster:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_bommaster)
                        return;
                    break;
                case EntryType.PurchaseReturn:
                case EntryType.Purchase:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_pur_prt &&
                        !Services.ConfigManager.Instance(orgnizationid).Config.intray_dndraft)
                        return;
                    break;
                case EntryType.MfgDesign:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_mfgdesign)
                        return;
                    break;
                case EntryType.Supplier:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_supplier)
                        return;
                    break;
                case EntryType.Task:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_task)
                        return;
                    break;
                case EntryType.PurchaseOrder:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_notification_purchaseorder)
                        return;
                    break;
                case EntryType.Discount:
                    if (!Services.ConfigManager.Instance(orgnizationid).Config.intray_discount)
                        return;
                    if (!Services.ConfigManager.Instance(orgnizationid).LocationConfig.ContainsKey(locationid)
                        || !Services.ConfigManager.Instance(orgnizationid).LocationConfig[locationid].intray_discount)
                        return;
                    break;
            }
            Models.Intray intray = new Models.Intray();
            intray.createdby = userid;
            intray.isactive = true;
            intray.referenceid = id;
            intray.companyid = companyid;
            intray.referencetype = (int) type;
            intray.typename = type.ToString();
            intray.version = 1;
            intray.locationid = locationid;
            intray.actiontype = (int) ActionType.Open;
            intray.actionby = userid;
            intray.actionon = Cotext.DateTimeNow;
            intray.detail = new Models.Intray.Detail()
            {
                description = description
            };
            dataService.Insert(transaction, transaction.Connection, intray);
        }
       
    }
}