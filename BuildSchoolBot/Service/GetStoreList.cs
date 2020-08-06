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
            string GetStoreJson = await new WebCrawler().GetStores(LatLng.lat,LatLng.lng);
            JArray array = JArray.Parse(GetStoreJson);
            JObject JStore = new JObject();
            JStore["Stores"] = array;
            string namejson = JStore.ToString();
            taskInfo.Card = CreateClickStoreModule(namejson);
            SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());
        }
        private void MenuModule(AdaptiveColumnSet ColumnSetitem, string StoreName ,string Url)
        {
            //店家名稱
            ColumnSetitem.Separator = true;
            var ColumnStoreItem = new AdaptiveColumn();
            ColumnStoreItem.Width = AdaptiveColumnWidth.Stretch;
            var containerfoodiitem = new AdaptiveContainer();
            var TextBlockfoodiitem = new AdaptiveTextBlock();
            TextBlockfoodiitem.Text = StoreName;
            containerfoodiitem.Items.Add(TextBlockfoodiitem);
            ColumnStoreItem.Items.Add(containerfoodiitem);

            //選擇時間
            var ColumnTime = new AdaptiveColumn();
            ColumnTime.Width = AdaptiveColumnWidth.Stretch;
            var containerTime = new AdaptiveContainer();
            var InputTime = new AdaptiveTimeInput();
            InputTime.Id = "DueTime";
            containerTime.Items.Add(InputTime);
            ColumnTime.Items.Add(containerTime);

            //勾選欄位
            var ColumnToggle = new AdaptiveColumn();
            ColumnToggle.Width = AdaptiveColumnWidth.Stretch;
            var containermoneyiitem = new AdaptiveContainer();
            var CheckBox = new AdaptiveToggleInput();
            CheckBox.Title = "Confirm";
            containermoneyiitem.Items.Add(CheckBox);
            ColumnToggle.Items.Add(containermoneyiitem);

            ColumnSetitem.Columns.Add(ColumnStoreItem);
            ColumnSetitem.Columns.Add(ColumnTime);
            ColumnSetitem.Columns.Add(ColumnToggle);
        }
        public LatLngService GetLatLng(string address)
        {
            var LatLng = new LatLngService(address);
            return LatLng;
        }
        public Attachment GetStore(string Address,string StoreData)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = Address;
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStorName);

            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Confirm", Data = new AdaptiveCardTaskFetchValue<string>() { Data = StoreData , SetType = "GetStore"} });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        private Attachment CreateClickStoreModule(string Jdata)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = "Chose Order";
            TextBlockStorName.Id = "GetStore";
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStorName);


            string[] itemsname = new string[] { "StoreName","Time", "Chose Your Order" };
            var ColumnSetitemname = new AdaptiveColumnSet();

            ColumnSetitemname.Separator = true;
            for (int i = 0; i < itemsname.Length; i++)
            {
                var Columnitemsname = new AdaptiveColumn();
                Columnitemsname.Width = AdaptiveColumnWidth.Stretch;
                var containeritemsname = new AdaptiveContainer();
                var TextBlockitemsname = new AdaptiveTextBlock();
                TextBlockitemsname.Text = itemsname[i];
                containeritemsname.Items.Add(TextBlockitemsname);
                Columnitemsname.Items.Add(containeritemsname);
                ColumnSetitemname.Columns.Add(Columnitemsname);
            }
            var root = JsonConvert.DeserializeObject<Store_List>(Jdata);
            foreach (var s in root.Stores)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                MenuModule(ColumnSetitem, s.Store_Name,s.Store_Url);
                card.Body.Add(ColumnSetitem);
            }
            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = Jdata } })
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
