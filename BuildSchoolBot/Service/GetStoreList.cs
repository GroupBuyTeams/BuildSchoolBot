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
        private void MenuModule(AdaptiveColumnSet ColumnSetitem, string StoreName, string Url)
        {
            ColumnSetitem.Separator = true;

            //店家名稱
            var ColumnStoreItem = new AdaptiveColumn();
            ColumnStoreItem.Width = AdaptiveColumnWidth.Stretch;
            var ContainerStore = new AdaptiveContainer();
            var TextBlockStoreitem = new AdaptiveTextBlock();
            TextBlockStoreitem.Text = StoreName;
            ContainerStore.Items.Add(TextBlockStoreitem);
            ColumnStoreItem.Items.Add(ContainerStore);

            //店家連結
            var ColumnUrlItem = new AdaptiveColumn();
            ColumnUrlItem.Width = AdaptiveColumnWidth.Stretch;
            var ContainerUrl = new AdaptiveContainer();
            var TextBlockUrlitem = new AdaptiveTextBlock();
            TextBlockUrlitem.Text = Url;
            ContainerUrl.Items.Add(TextBlockUrlitem);
            ColumnUrlItem.Items.Add(ContainerUrl);

            //勾選欄位
            var ColumnToggle = new AdaptiveColumn();
            ColumnToggle.Width = AdaptiveColumnWidth.Stretch;
            var containermoneyiitem = new AdaptiveContainer();
            var CheckBox = new AdaptiveToggleInput();
            CheckBox.Title = "Confirm";
            containermoneyiitem.Items.Add(CheckBox);
            ColumnToggle.Items.Add(containermoneyiitem);

            ColumnSetitem.Columns.Add(ColumnStoreItem);
            ColumnSetitem.Columns.Add(ColumnUrlItem);
            ColumnSetitem.Columns.Add(ColumnToggle);
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
            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = Address;
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
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
                Text = "Chose Order",
                Id = "GetStore",
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
            };
            card.Body.Add(TextBlockStorName);
            card.Id = "GetStore";

            var root = JsonConvert.DeserializeObject<Store_List>(Jdata);

            foreach (var s in root.Stores)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                MenuModule(ColumnSetitem, s.Store_Name, s.Store_Url);
                card.Body.Add(ColumnSetitem);
            }

            //選擇時間
            var ColumnTime = new AdaptiveColumn { Width = AdaptiveColumnWidth.Auto };
            var containerTime = new AdaptiveContainer();
            var InputTime = new AdaptiveTimeInput { Id = "DueTime" };
            containerTime.Items.Add(InputTime);
            ColumnTime.Items.Add(containerTime);
            card.Body.Add(ColumnTime);

            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = "" } })
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
