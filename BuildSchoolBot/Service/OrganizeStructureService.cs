using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.AllSelectData;

namespace BuildSchoolBot.Service
{
    public class OrganizeStructureService
    {
        public Attachment GetTaskModuleFetchCard(string Value, string GetMenuJson, TaskModuleTaskInfo TaskInfo)
        {
            var StorName = new OrderfoodServices().GetStr(Value, "FoodData2468", true);
            var FoodAndGuidProcessUrl = new OrderfoodServices().GetStr(Value, "FoodData2468", false);
            var Guidstr = new OrderfoodServices().GetStr(FoodAndGuidProcessUrl, "GuidStr13579", false);
            var NameJson = new OrderfoodServices().ArrayPlusName(GetMenuJson, "Menuproperties");
            TaskInfo.Card = new CreateCardService().CreateClickfoodModule(Guidstr, StorName, NameJson);
            return TaskInfo.Card;
        }
        public async Task<string> GetFoodUrl(string Value)
        {
            var FoodAndGuidProcessUrl = new OrderfoodServices().GetStr(Value, "FoodData2468", false);
            var FoodUrl = new OrderfoodServices().GetStr(FoodAndGuidProcessUrl, "GuidStr13579", true);
            string GetMenu= await new WebCrawler().GetOrderInfo(FoodUrl);
            return GetMenu;
        }

        public string GetFoodUrlStr(string Value)
        {
            Task<string> FoodUrl =GetFoodUrl(Value);
            string GetMenuJson = FoodUrl.Result;
            return GetMenuJson;
        }
        public string GetStoreName(string StoreAndGuid)
        {
            return new OrderfoodServices().GetStr(StoreAndGuid, "FoodGuid2468", true);
        }
        public string GetOrderID(string StoreAndGuid)
        {
            return new OrderfoodServices().GetStr(StoreAndGuid, "FoodGuid2468", false);
        }
        public void RemoveNeedlessStructure(JObject data)
        {
            data.Property("msteams").Remove();
            data.Property("data").Remove();
        }
    }
}
