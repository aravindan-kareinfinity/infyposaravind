using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class Homepage : BaseEntity
    {
        public long id { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public string schoolname { get; set; }
        public string domain { get; set; }
        public long institutionid { get; set; }
        public string mobilenumber { get; set; }
        public string emailid { get; set; }
        public string address { get; set; }
        public bool isselected { get; set; }
        public string logo { get; set; }
        public  string ctitle { get; set; }
        public string ititle { get; set; }
        public string ftitle { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string systemimages_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(systemimages); } set { systemimages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Systemimage>>(value); } }
        public List<Systemimage> systemimages { get; set; }
        public class Systemimage
        {
            public string title { get; set; }
            public string image { get; set; }
            public string imagepath { get; set; }
           
        }


        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string facilities_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(facilities); } set { facilities = Newtonsoft.Json.JsonConvert.DeserializeObject<List<facilitie>>(value); } }
        public List<facilitie> facilities { get; set; }
        public class facilitie
        {
            public string heading { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string slides_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(slides); } set { slides = Newtonsoft.Json.JsonConvert.DeserializeObject<List<slide>>(value); } }

        public List<slide> slides { get; set; }
        public class slide
        {
            public string title { get; set; }
            public string subtitle { get; set; }
            public string image { get; set; }
        }


        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string images_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(images); } set { images = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Image>>(value); } }
        public List<Image> images { get; set; }
        public class Image
        {
            public string image { get; set; }
        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string aboutus_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(aboutus); } set { aboutus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<content>>(value); } }
        public List<content> aboutus { get; set; }

       
        public class content
        {
            public string title { get; set; }
            public string display { get; set; }

            public bool isdefault { get; set; }


        }

        
       // public string facilities_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(facilities); } set { facilities = Newtonsoft.Json.JsonConvert.DeserializeObject<facilitie>(value); } }
       

    }
    public class HomepageCreteria : Homepage
    { }


}
