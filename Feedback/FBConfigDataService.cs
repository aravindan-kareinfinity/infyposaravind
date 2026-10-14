using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class FBConfigDataService : BaseData
    {

        public FBConfig Search()
        {
            ConfigurationDataService configurationDataService = new ConfigurationDataService() { Context = this.Context };
            var configlist = configurationDataService.Search(new ConfigurationCreteria()
            {
                key = "FBCONFIG"
            });
            if (configlist.Count > 0)
            {
                var config = configlist[0].fbconfig;
                return config;
            }
            return new FBConfig();
        }

        public FBConfig Save(FBConfig oFBConfig)
        {

            ConfigurationDataService configurationDataService = new ConfigurationDataService() { Context = this.Context };
            var configlist = configurationDataService.Search(new ConfigurationCreteria()
            {
                key = "FBCONFIG"
            });
            if (configlist.Count > 0)
            {
                configlist[0].fbconfig = oFBConfig;
                configurationDataService.Update(configlist[0]);
            }
            else
            {
                var config = new Configuration();
                config.key = "FBCONFIG";
                config.version = 1;
                config.createdby = Context.UserID;
                config.createdon = DateTimeNow;
                config.modifiedby = Context.UserID;
                config.modifiedon = DateTimeNow;
                config.name = "Feedback Configuration";
                config.fbconfig = oFBConfig;
                config.isactive = true;
                config.isdefault = true;
                configurationDataService.Insert(config);
            }
            return oFBConfig;
        }


    }

}
