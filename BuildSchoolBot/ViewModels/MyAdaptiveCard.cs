using AngleSharp.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.ViewModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

    public class Action
    {
        public string type { get; set; }
        public string title { get; set; }
        public string style { get; set; }
        public Data data { get; set; }
    }

    public class Item
    {
        public string type { get; set; }
        public string size { get; set; }
        public string weight { get; set; }
        public string text { get; set; }
        public string color { get; set; }
        public string style { get; set; }
        public string url { get; set; }
        public bool? wrap { get; set; }
        public string spacing { get; set; }
        public string fontType { get; set; }
        public string title { get; set; }
        public string horizontalAlignment { get; set; }
        public List<Action> actions { get; set; }
    }

    public class Column
    {
        public string type { get; set; }
        public string width { get; set; }
        public List<Item> items { get; set; }
        public string verticalContentAlignment { get; set; }
        public string horizontalAlignment { get; set; }
        public string minHeight { get; set; }
    }

    public class Body
    {
        public string type { get; set; }
        public List<Column> columns { get; set; }
        public string style { get; set; }
    }

    public class MyAdaptiveCard
    {
        public string type { get; set; }
        public List<Body> body { get; set; }
        [JsonProperty("$schema")]
        public string schema { get; set; }
        public string version { get; set; }
    }



}

