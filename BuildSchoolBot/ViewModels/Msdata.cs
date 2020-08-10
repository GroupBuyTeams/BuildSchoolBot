using AngleSharp.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.ViewModels
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Msteams
    {
        public string type { get; set; }
        public string text { get; set; }
        public MsteamsValue value { get; set; }
    }

    public class MsteamsValue
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Option")]
        public string Option { get; set; }

        [JsonProperty("LibraryId")]
        public Guid LibraryId { get; set; }
    }

    public class Data
    {
        public Msteams msteams { get; set; }
    }

}

