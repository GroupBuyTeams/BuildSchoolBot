using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildSchoolBot.Models;
using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static BuildSchoolBot.StoreModels.ModifyMenu;

namespace BuildSchoolBot.Service
{
    public class AdaptiveCardDataFactory
    {
        public TaskModuleRequest Request { get; private set; }
        public ITurnContext TurnContext { get; private set; }
        public AdaptiveCardDataFactory()
        {

        }

        public AdaptiveCardDataFactory(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest request)
        {
            Request = request;
            TurnContext = turnContext;
        }

        public CardDataModel<T> GetCardInfo<T>()
        {
            var str = (Request.Data as JObject)["data"]?.ToString();
            try
            {
                return JsonConvert.DeserializeObject<CardDataModel<T>>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public T GetCardData<T>() where T : class
        {
            return GetCardInfo<T>().Value;
        }

        public string GetCardActionType()
        {
            return GetCardInfo<object>()?.Type;
        }

        public List<SelectMenu.SelectMenuData> GetOrderedFoods()
        {
            // JObject jData = JObject.Parse(Request.Data);
            var jData = JObject.FromObject(Request.Data);

            RemoveProperty(jData);
            var dictionary = new Dictionary<string, SelectMenu.SelectMenuData>();
            foreach (var dish in jData)
            {
                var key = dish.Key.Split("&&");
                if (!key[1].Equals("mark") && !dish.Value.Equals("0"))
                {
                    var data = new SelectMenu.SelectMenuData() { Dish_Name = key[0], Price = key[1], Quantity = (string)dish.Value };
                    dictionary.Add(key[0], data);
                }
                
                
                else if (key[1].Equals("mark") && !dish.Value.Equals(string.Empty))
                {
                    var data = new SelectMenu.SelectMenuData();
                    if (dictionary.TryGetValue(key[0], out data))
                    {
                        data.Remarks = (string)dish.Value;
                    }
                }
            }
            return dictionary.Select(x => x.Value).ToList();
        }

        public StoreOrderDuetime GetGroupBuyCard()
        {
            var storesInfo = JObject.FromObject(Request.Data);
            RemoveProperty(storesInfo);
            var time = (string)storesInfo.GetValue("DueTime");
            
            foreach (var store in storesInfo)
            {
                var val = (string)store.Value;
                if (val.Equals("True"))
                {
                    var storeData = store.Key.Split("&&"); 
                    return new StoreOrderDuetime()
                    {
                        OrderID = Guid.NewGuid().ToString(),
                        DueTime = time,
                        StoreName = storeData[0],
                        Url = storeData[1]
                    };
                }
            }
            return null;
        }

        public void RemoveProperty(JObject jData)
        {
            jData.Property("msteams").Remove();
            jData.Property("data").Remove();
        }
        public void ModifyMenuData(TaskModuleRequest taskModuleRequest, TaskModuleTaskInfo TaskInfo)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            //JObject Data = JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data));
            var jData = JObject.FromObject(Request.Data);
            RemoveProperty(jData);
            var inputlist = new List<string>();
            foreach (var item in jData)
            {
                inputlist.Add(item.Value.ToString());
            }
            inputlist.Remove(inputlist[0]);
            var StoreName = inputlist[0];
            inputlist.Remove(inputlist[0]);
            var Modify = new ModifyGroup();
            var w = new List<ModifyMultiple>();
            for (int i = 0; 2 * i < inputlist.Count(); i++)
            {
                w.Add(new ModifyMultiple() { ProductName = inputlist[2 * i], Amount = inputlist[2 * i + 1], MenuId = Value });
            }
            Modify.AllModifyMultiple = w;
            Modify.StoreName = StoreName;
            for (var i = 0; i < Modify.AllModifyMultiple.Count; i++)
            {
                if (Modify.AllModifyMultiple[i].ProductName == "" || Modify.AllModifyMultiple[i].Amount.ToString() == "")
                {
                    Modify.AllModifyMultiple.Remove(Modify.AllModifyMultiple[i]);
                }
            }
            new MenuService(context).UpdateMenuOrderStoreName(Value, Modify.StoreName);
            new MenuDetailService(context).DeleteMenuDetail(Value);
            new MenuDetailService(context).CreateMenuDetail(Modify);        
        }
        public void GetModifyModuleData(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var TaskInfo = new TaskModuleTaskInfo();
            TeamsBuyContext context = new TeamsBuyContext();
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            var MenuOrderData = new MenuDetailService(context).GetMenuOrder(Value).ToList();
            var MenuOrderStore = new MenuService(context).GetMenuOrder(Value).Store;
            TaskInfo.Card = new CreateCardService2().GetCustomizedModification(MenuOrderStore, MenuOrderData, Value);
            new CreateCardService2().SetTaskInfo(TaskInfo, TaskModuleUIConstants.UpdateMenu);
        }
    }
}