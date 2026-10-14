using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class FBConfig : BaseEntity
    {
        public string desktopbanner { get; set; }
        public int desktopbannerheight { get; set; }
        public int desktopbannerwidth { get; set; }
        public string mobilebanner { get; set; }
        public int mobilebannerheight { get; set; }
        public int mobilebannerwidth { get; set; }
        public string feedbackmobileno { get; set; }
        public string feedbackname { get; set; }
        public string feedbackrating { get; set; }
        public bool feedbackserialno { get; set; }

        public bool collectsuggestion { get; set; }
        public int feedbacksuggestionheight { get; set; }
        public string feedbacksuggestion { get; set; }

        public bool collectcomplaint { get; set; }
        public int feedbackcomplaintheight { get; set; }
        public string feedbackcomplaint { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string html_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(html); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    html = new Html()
                    {
                        header="<p>Your headers add here</p>",
                        response = "<p>Your response add here</p>",
                    };
                else
                    html = Newtonsoft.Json.JsonConvert.DeserializeObject<Html>(value);
            }
        }
        public Html html { get; set; }
        public class Html
        {
            public string header { get; set; }
            public string response { get; set; }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string branches_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(branches); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    branches = new List<Branches>();
                else
                    branches = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Branches>>(value);
            }
        }
        public List<Branches> branches { get; set; }
        public class Branches
        {
            public string code { get; set; }
            public string name { get; set; }
        }
    }
    public class FBConfigCreteria : FBConfig
    {
        public bool current { get; internal set; }
    }
}

