using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class FBQuestions : BaseEntity
    {
        public bool enable { get; set; }
        public int index { get; set; }
        public int answercount { get; set; }
        public int chartposition { get; set; }
        public string question { get; set; }
        public int charttype { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string answerlist_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(answerlist); } set { answerlist = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Answerlist>>(value); } }
        public List<Answerlist> answerlist { get; set; }
        public string code { get; set; }
        public bool supportanalytical { get; set; }
        public bool supportacknoloedge { get; set; }
        public bool isactive { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public DateTime activefrom { get; set; }
        public DateTime activeto { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public long id { get; set; }
        public class Answerlist
        {
            public string answer { get; set; }
            public bool ack { get; set; }
            public long id { get; set; }
        }
    }
    public class QuestionsCreteria : FBQuestions
    {
    }

    public class QuestionChanges : BaseEntity
    {
        public string type { get; set; }
        public FBQuestions question { get; set; }
        public FBQuestions selected { get; set; }
        public FBQuestions next { get; set; }
        public FBQuestions previous { get; set; }
    }
}
