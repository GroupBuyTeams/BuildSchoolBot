using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Models
{
    public class AdaptiveCardTaskFetchValue<T>
    {
        [JsonProperty("msteams")]
        public object Type { get; set; } = JsonConvert.DeserializeObject("{\"type\": \"task/fetch\" }");

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
