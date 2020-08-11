using AdaptiveCards;
using BuildSchoolBot.Models;
using BuildSchoolBot.StoreModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace BuildSchoolBot.Service
{
    public class GetStoreList
    {
        public async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(TaskModuleRequest taskModuleRequest)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            var taskInfo = new TaskModuleTaskInfo();
            var LatLng = GetLatLng(value);
            string GetStoreJson = await new WebCrawler().GetStores(LatLng.lat, LatLng.lng);
            JArray array = JArray.Parse(GetStoreJson);
            JObject JStore = new JObject();
            JStore["Stores"] = array;
            string namejson = JStore.ToString();
            taskInfo.Card = CreateClickStoreModule(namejson);
            SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());
        }
        private void StoreModule(AdaptiveColumnSet ColumnSetitem, string StoreName, string Url)
        {
            //ColumnSetitem.Separator = true;

            //店家名稱
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(StoreName, "StoreName")));
            //店家連結
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(Url, "Url")));
            //勾選欄位
            var CheckBox = new AdaptiveToggleInput
            {
                Id = StoreName + "^_^" + Url,
                Title = "Confirm"
            };
            ColumnSetitem.Columns.Add(AddColumn(CheckBox));
        }
        public AdaptiveTextInput GetadaptiveTextBlock(string InputTxt, string _ID)
        {
            var TextBlock = new AdaptiveTextInput
            {
                Value = InputTxt,
                Id = _ID
            };
            return TextBlock;
        }
        public AdaptiveColumn AddColumn<T>(T adaptiveElement) where T : AdaptiveElement
        {
            var result = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Stretch
            };
            var Container = new AdaptiveContainer();
            Container.Items.Add(adaptiveElement);
            result.Items.Add(Container);
            return result;
        }
        public LatLngService GetLatLng(string address)
        {
            var LatLng = new LatLngService(address);
            return LatLng;
        }
        public Attachment GetStore(string Address, string StoreData)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            var TextBlockStorName = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder,
                Text = Address,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
            };
            card.Body.Add(TextBlockStorName);

            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Confirm", Data = new AdaptiveCardTaskFetchValue<string>() { Data = StoreData, SetType = "GetStore" } });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        private Attachment CreateClickStoreModule(string Jdata)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            var TextBlockStorName = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder,
                Text = "Chose Your Order",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
            };
            card.Body.Add(TextBlockStorName);

            var root = JsonConvert.DeserializeObject<Store_List>(Jdata);

            foreach (var s in root.Stores)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                StoreModule(ColumnSetitem, s.Store_Name, s.Store_Url);
                card.Body.Add(ColumnSetitem);
            }

            //選擇時間
            var InputTime = new AdaptiveTimeInput();
            InputTime.Id = "DueTime";
            card.Body.Add(InputTime);

            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = "Submit", Data = new AdaptiveCardTaskFetchValue<string>() { Data = "ResultStoreCard" } })
                    .ToList<AdaptiveAction>();
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }
    }
}
