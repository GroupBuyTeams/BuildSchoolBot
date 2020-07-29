using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class CardTaskFetchValue<T>
    {      
            [JsonProperty("type")]
            public object Type { get; set; } = "task/fetch";

            [JsonProperty("data")]
            public T Data { get; set; }        
    }
}
