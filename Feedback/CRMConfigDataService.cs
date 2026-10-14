using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class CRMConfigDataService : BaseData
    {

        public CRMConfig Search()
        {
            ConfigurationDataService configurationDataService = new ConfigurationDataService() { Context = this.Context };
            var configlist = configurationDataService.Search(new ConfigurationCreteria()
            {
                key = "CRMCONFIG"
            });
            if (configlist.Count > 0)
            {
                var config = configlist[0].crmconfig;
                return config;
            }
            return null;
        }

        public CRMConfig Save(CRMConfig oCRMConfig)
        {

            ConfigurationDataService configurationDataService = new ConfigurationDataService() { Context = this.Context };
            var configlist = configurationDataService.Search(new ConfigurationCreteria()
            {
                key = "CRMCONFIG"
            });
            if (configlist.Count > 0)
            {
                configlist[0].crmconfig = oCRMConfig;
                configurationDataService.Update(configlist[0]);
            }
            else
            {
                var config = new Configuration();
                config.key = "CRMCONFIG";
                config.version = 1;
                config.createdby = Context.UserID;
                config.createdon = DateTimeNow;
                config.modifiedby = Context.UserID;
                config.modifiedon = DateTimeNow;
                config.name = "CRM Configuration";
                config.crmconfig = oCRMConfig;
                config.isactive = true;
                config.isdefault = true;
                configurationDataService.Insert(config);
            }
            return oCRMConfig;
        }


    }

}
