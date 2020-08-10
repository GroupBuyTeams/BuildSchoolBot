using Newtonsoft.Json;

namespace BuildSchoolBot.ViewModels
{
    public class CardRootData
    {
        public CardDataModel root { get; set; }
    }
    
    public class CardDataModel
    {
        public string type { get; set; }
        public object value { get; set; }
    }

    public class StoreInfoData
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        
        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Guid")]
        public string Guid { get; set; }
    }
    
}