﻿using AdaptiveCards;
using BuildSchoolBot.StoreModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.fooditem;
using static BuildSchoolBot.StoreModels.GetStore;

namespace BuildSchoolBot.Service
{
    public class OrderfoodServices
    {
   

            public async Task OnMessageActivityAsync(string json, WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                var root = JsonConvert.SerializeObject(json);
                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);

            //foreach (var p in root)
            //{

            //    reply.Attachments.Add(GetStore());
            //}
            await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            }




            public Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(TaskModuleRequest taskModuleRequest)
            {
            var fooditemdata = @"{
                ""properties"": [
                    {
                        ""key"": ""蜂蜜鬆餅及紅茶(綠茶)"",
                        ""value"": ""NT$70""
                    },
                    {
                         ""key"": ""珍珠奶茶"",
                        ""value"": ""NT$50""
                    },
                    {
                        ""key"": ""果汁"",
                        ""value"": ""NT$30""
                    },
                    {
                        ""key"": ""ccccccc"",
                        ""value"": ""NT$70""
                    },
                    {
                        ""key"": ""bbbbbbbb"",
                        ""value"": ""NT$70""
                    },
                    {
                        ""key"": ""aaaaa"",
                        ""value"": ""NT$70""
                    },
                    {
                        ""key"": ""sacsac"",
                        ""value"": ""NT$70""
                    },
                ]
            }";
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
                var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;

                var taskInfo = new TaskModuleTaskInfo();
                switch (value)
                {
                    case TaskModuleIds.AdaptiveCard:
                        taskInfo.Card = CreateClickfoodModule(fooditemdata);
                        SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
                        break;
                    default:
                        break;
                }

                return Task.FromResult(taskInfo.ToTaskModuleResponse());
            }

            protected async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
            {

                dynamic orderAttachitem = ((dynamic)taskModuleRequest.Data);
                string orderAttachitemtext = orderAttachitem.undefined;

                var attachments = new List<Attachment>();
                var reply = MessageFactory.Attachment(attachments);

                //reply.Attachments.Add(GetResultClickfood(orderAttachitemtext));

                await turnContext.SendActivityAsync(reply, cancellationToken);

                return TaskModuleResponseFactory.CreateResponse("感謝您的點餐");
            }


            private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
            {
                taskInfo.Height = uIConstants.Height;
                taskInfo.Width = uIConstants.Width;
                taskInfo.Title = uIConstants.Title.ToString();
            }

            public  Attachment GetStore(string texta)
            {
                // Create an Adaptive Card with an AdaptiveSubmitAction for each Task Module
                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
                {

                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock(){ Text=texta,Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
                    },
                    Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                                .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = cardType.Id } })
                                .ToList<AdaptiveAction>(),
                };

                return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
            }

        //public void Findmenudata(object sender, System.EventArgs e)
        //{
            
        //}

        public string Findmenu(string json,object sender)
        {
            var rootmoduel = GetStoregroup(json);
            JObject o = new JObject();
            o["properties"] = rootmoduel;
            string namejson = o.ToString();
            var root = JsonConvert.DeserializeObject<Storenamegroup>(namejson);


            string btn = (string)sender;

            //var click = new System.EventHandler(this.Findmenudata).Target;
            var menu = root.properties.FirstOrDefault(x => x.Store_Name.ToString().Equals((sender)));
            var w = new WebCrawler();
            var menujson = w.GetOrderInfo(menu.Store_Url);
            var jsonmenudata = JsonConvert.SerializeObject(menujson);
            return jsonmenudata;
        }

        private void MenuModule(AdaptiveColumnSet ColumnSetitem, string foodname, string money, int number)
            {
                //食物名稱
                ColumnSetitem.Separator = true;
                var Columnfooditem = new AdaptiveColumn();
                Columnfooditem.Width = AdaptiveColumnWidth.Stretch;
                var containerfoodiitem = new AdaptiveContainer();
                var TextBlockfoodiitem = new AdaptiveTextBlock();
                TextBlockfoodiitem.Text = foodname;
                containerfoodiitem.Items.Add(TextBlockfoodiitem);
                Columnfooditem.Items.Add(containerfoodiitem);


                //錢
                var Columnmoneyitem = new AdaptiveColumn();
                Columnmoneyitem.Width = AdaptiveColumnWidth.Stretch;
                var containermoneyiitem = new AdaptiveContainer();
                var TextBlockmoneyiitem = new AdaptiveTextBlock();
                TextBlockmoneyiitem.Text = money;
                containermoneyiitem.Items.Add(TextBlockmoneyiitem);
                Columnmoneyitem.Items.Add(containermoneyiitem);


                //數量
                var Columnnumberitem = new AdaptiveColumn();
                Columnnumberitem.Width = AdaptiveColumnWidth.Stretch;
                var containernumberiitem = new AdaptiveContainer();
                var Inputnumberiitem = new AdaptiveNumberInput();
                Inputnumberiitem.Id = "number" + number;
                Inputnumberiitem.Placeholder = "Enter a number";
                Inputnumberiitem.Min = 0;
                Inputnumberiitem.Value = 0;
                containernumberiitem.Items.Add(Inputnumberiitem);
                Columnnumberitem.Items.Add(containernumberiitem);



                //備註
                var ColumnRemarksitem = new AdaptiveColumn();
                ColumnRemarksitem.Width = AdaptiveColumnWidth.Stretch;
                var containerRemarksiitem = new AdaptiveContainer();
                var InputRemarksiitem = new AdaptiveTextInput();
                InputRemarksiitem.Id = "number" + number;
                containerRemarksiitem.Items.Add(InputRemarksiitem);
                ColumnRemarksitem.Items.Add(containerRemarksiitem);


                ColumnSetitem.Columns.Add(Columnfooditem);
                ColumnSetitem.Columns.Add(Columnmoneyitem);
                ColumnSetitem.Columns.Add(Columnnumberitem);
                ColumnSetitem.Columns.Add(ColumnRemarksitem);



            }


            private Attachment CreateClickfoodModule(string modulefoodjson)
            {
            //object a;
            //    var menudatajson= Findmenu(modulefoodjson, a)
                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
                string[] itemsname = new string[] { "菜名", "價錢", "數量", "備註" };
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

                var root = JsonConvert.DeserializeObject<foodgroup>(modulefoodjson);
                card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                            .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = "" } })
                            .ToList<AdaptiveAction>();
                int count = 0;
                card.Body.Add(ColumnSetitemname);
                foreach (var p in root.properties)
                {
                    var ColumnSetitem = new AdaptiveColumnSet();
                    MenuModule(ColumnSetitem, p.key, p.value, count);
                    card.Body.Add(ColumnSetitem);
                    count++;
                }
                return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
            }

            private static Attachment GetResultClickfood(string Clickfood)
            {
                // Create an Adaptive Card with an AdaptiveSubmitAction for each Task Module
                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
                {

                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock(){ Text=Clickfood,Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
                    }
                };

                return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
            }

        public JArray GetStoregroup(string json)
        {
            JArray array = JArray.Parse(json);
            return array;
        }
    }
   
}
