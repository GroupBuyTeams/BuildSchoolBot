using System;
using System.Collections.Generic;
using System.Linq;
using BuildSchoolBot.Models;
using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                var key = dish.Key.Split('&');
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

        public void RemoveProperty(JObject jData)
        {
            jData.Property("msteams").Remove();
            jData.Property("data").Remove();
        }

    }
}