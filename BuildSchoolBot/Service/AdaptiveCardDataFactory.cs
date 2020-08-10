using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BuildSchoolBot.Service
{
    public class AdaptiveCardDataFactory
    {
        public T GetDataWhenOpenTaskModule<T>(TaskModuleRequest request)
        {
            var asJObject = JObject.FromObject(request.Data);
            var value = asJObject.ToObject<CardTaskFetchValue<string>>()?.Data;
            return JsonConvert.DeserializeObject<T>(value);
        }
        
    }
}