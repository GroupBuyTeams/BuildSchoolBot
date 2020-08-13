using Newtonsoft.Json;

namespace BuildSchoolBot.ViewModels
{
    public class CardDataModel<T>
    {
        [JsonProperty("Type")]
        public string Type { get; set; }
        
        [JsonProperty("Value")]
        public T Value { get; set; }
    }

    public class StoreInfoData
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        
        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Guid")]
        public string Guid { get; set; }
        [JsonProperty("DueTime")]
        public string DueTime { get; set; }

    }

}