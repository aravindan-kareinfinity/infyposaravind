using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class FBAnswer : BaseEntity
    {
        public string mobileno { get; set; }
        public string name { get; set; }
        public string area { get; set; }
        public string referenceid { get; set; }
        public DateTime feedbackdate { get; set; }
        public DateTime acknoledgedon { get; set; }
        public long acknoledgedby { get; set; }
        public string customerreason { get; set; }
        public int feedbackstatus { get; set; }
        public bool requiredackowledge { get; set; }
        public string actiontaken { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string qafeedback_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(qafeedback); } set { qafeedback = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Qafeedback>>(value); } }
        public List<Qafeedback> qafeedback { get; set; }
        public int overrallrating { get; set; }
        public long id { get; set; }

        public class Complaintsuggesion
        {
            public string complaint { get; set; }
            public string suggesion { get; set; }
        }

        public bool hascomplaint { get; set; }
        public bool hassugession { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string complaintsuggesion_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(complaintsuggesion); } set { complaintsuggesion = Newtonsoft.Json.JsonConvert.DeserializeObject<Complaintsuggesion>(value); } }
        public Complaintsuggesion complaintsuggesion { get; set; }

        public class Qafeedback
        {
            public string question { get; set; }
            public string answer { get; set; }
            public long questionid { get; set; }
            public long answerid { get; set; }
            public string uniqueid { get; set; }
        }
    }
    public class FeedbackCreteria : FBAnswer
    {
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }

    public class FBRegistration:BaseEntity
    {
        public Customer Customer { get; set; }
        public List<QuestionAnswer> answerlist { get; set; }
        public CustomerCoupons coupen { get; set; }
        public class QuestionAnswer
        {
            public long id { get; set; }
            public string answer { get; set; }
        }
    }


    public class RatingSummary
    {
        public List<int> wdata { get; set; }
        public List<string> wlabels { get; set; }

        public List<int> mdata { get; set; }
        public List<string> mlabels { get; set; }

        public List<int> ydata { get; set; }
        public List<string> ylabels { get; set; }

    }
    public class Rating
    {
        public int rate { get; set; }
        public int value { get; set; }
    }

    public class FeedbackQASummary
    {
        const string backgroudcolours = "#FF3784,#36A2EB,#4BC0C0,#F77825,#9966FF,#00A8C6,#379F7A,#CC2738,#8B628A,#8FBE00,#606060";
        public FeedbackQASummary(List<FeedbackQA> list)
        {
            this.list = list;
            datasets = new List<dataset>();

            if (list.Count > 0)
            {
                var colourlist = backgroudcolours.Split(',');
                int maxanswers = list.Max(e => e.answers.Count);
                labels = list.ConvertAll(e => e.question);
                for (int i = 0; i < maxanswers; i++)
                {
                    var ds = new dataset(list.Count, colourlist[i]);
                    datasets.Add(ds);
                    for (int q = 0; q < list.Count; q++)
                    {
                        ds.data[q] = list[q].answers.Count > i ? list[q].answers[i].count : 0;
                        ds.labels[q] = list[q].answers.Count > i ? list[q].answers[i].answer : "";
                    }
                }
            }
        }
        [JsonIgnore]
        public List<FeedbackQA> list { get; set; }
        public List<string> labels { get; set; }
        public List<dataset> datasets { get; set; }
        public class dataset
        {
            public dataset()
            {

            }

            public dataset(int questioncount, string colour)
            {
                labels = new List<string>();
                data = new List<int>();
                backgroundColor = new List<string>();
                for (var i = 0; i < questioncount; i++)
                {
                    labels.Add("");
                    data.Add(0);
                    backgroundColor.Add(colour);
                }
            }
            public List<string> backgroundColor { get; set; }
            public List<string> labels { get; set; }
            public List<int> data { get; set; }
        }

    }
    public class FeedbackQA
    {
        public string question { get; set; }
        public List<answercount> answers { get; set; }
        public class answercount
        {
            public string answer { get; set; }
            public int count { get; set; }
        }
    }

    public class RPTSummary
    {
        public List<string> columns { get; set; }

        public List<Detail> detailitems { get; set; }

        public List<Summary> summaryitems { get; set; }

        public class Summary
        {
            public string question { get; set; }
            public string answer { get; set; }
            public int count { get; set; }
        }
        public class Detail
        {
            public long id { get; set; }
            public string mobileno { get; set; }
            public string name { get; set; }
            public DateTime feedbackdate { get; set; }
            public long overrallrating { get; set; }
            [JsonIgnore]
            [JsonProperty(Required = Required.Default)]
            public string qafeedback_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(qafeedback); } set { qafeedback = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FBAnswer.Qafeedback>>(value); } }
            public List<FBAnswer.Qafeedback> qafeedback { get; set; }
            public bool hascomplaint { get; set; }
            public bool hassugession { get; set; }
            public string complaint { get; set; }
            public string sugession { get; set; }

            [JsonIgnore]
            [JsonProperty(Required = Required.Default)]
            public string complaintsuggesion_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(complaintsuggesion); } set { complaintsuggesion = Newtonsoft.Json.JsonConvert.DeserializeObject<FBAnswer.Complaintsuggesion>(value); } }
            public FBAnswer.Complaintsuggesion complaintsuggesion { get; set; }
        }
    }
    public class ReportCreteria : BaseEntity
    {
        public class qa
        {
            public string question { get; set; }
            public string answer { get; set; }
            public long questionid { get; set; }
            public long answerid { get; set; }
        }
        public List<qa> List { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public int fromrating { get; set; }
        public int torating { get; set; }
        public string rpttype { get; set; }
        public bool hascomplaint { get; set; }
        public bool hassuggesion { get; set; }
        public long organizationid { get; set; }
        
    }
}
