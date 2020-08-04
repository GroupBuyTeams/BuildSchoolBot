using AdaptiveCards;
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
            //string GetStoreJson = await new WebCrawler().GetStores();
            taskInfo.Card = CreateClickfoodModule("123");
            SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());
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
        private Attachment CreateClickfoodModule(string StorName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = StorName;
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStorName);


            string[] itemsname = new string[] { "StoreName", "Confirm" };
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
