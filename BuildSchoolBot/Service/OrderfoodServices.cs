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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.fooditem;
using static BuildSchoolBot.StoreModels.GetStore;
using static BuildSchoolBot.StoreModels.SelectMenu;

namespace BuildSchoolBot.Service
{
    public class OrderfoodServices
    {
   

            //public async Task OnMessageActivityAsync(string json, WaterfallStepContext stepContext, CancellationToken cancellationToken)
            //{
            //    var root = JsonConvert.SerializeObject(json);
            //    var attachments = new List<Attachment>();
            //    var reply = MessageFactory.Attachment(attachments);

            ////foreach (var p in root)
            ////{

            ////    reply.Attachments.Add(GetStore());
            ////}
            //await stepContext.Context.SendActivityAsync(reply, cancellationToken);
            //}




            public async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(TaskModuleRequest taskModuleRequest)
            {
                var asJobject = JObject.FromObject(taskModuleRequest.Data);
                var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
                var taskInfo = new TaskModuleTaskInfo();
                string Getmenujson = await new WebCrawler().GetOrderInfo(value);
                JArray array = JArray.Parse(Getmenujson);
                JObject o = new JObject();
                o["Menuproperties"] = array;
                string namejson = o.ToString();
                taskInfo.Card = CreateClickfoodModule(namejson);
                SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }


        public async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
            {
            var taskModuleRequestjson = JsonConvert.SerializeObject(taskModuleRequest.Data);
            JObject data = JObject.Parse(taskModuleRequestjson);
            data.Property("msteams").Remove();
            data.Property("data").Remove();
            var MenudataGroups = data.ToString();
            var inputlist = new List<string>();
            var inputname = new List<string>();
            foreach (var item in data)
            {
                inputlist.Add(item.Value.ToString());
                inputname.Add(item.Key.ToString());
            }
            //var taskModuleRequestjsonabc = JsonConvert.DeserializeObject(MenudataGroups);
            var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value:"+ MenudataGroups);
            await turnContext.SendActivityAsync(reply, cancellationToken);

            return TaskModuleResponseFactory.CreateResponse("Thanks!");


            //dynamic orderAttachitem = ((dynamic)taskModuleRequest.Data);
            //    string orderAttachitemtext = orderAttachitem.undefined;

            //    var attachments = new List<Attachment>();
            //    var reply = MessageFactory.Attachment(attachments);

            //    //reply.Attachments.Add(GetResultClickfood(orderAttachitemtext));

            //    await turnContext.SendActivityAsync(reply, cancellationToken);

            //    return TaskModuleResponseFactory.CreateResponse("感謝您的點餐");
            }


            private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
            {
                taskInfo.Height = uIConstants.Height;
                taskInfo.Width = uIConstants.Width;
                taskInfo.Title = uIConstants.Title.ToString();
            }

            public  Attachment GetStore(string texta, string menuurl)
            {
                // Create an Adaptive Card with an AdaptiveSubmitAction for each Task Module
                var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
                {

                    Body = new List<AdaptiveElement>()
                    {
                        new AdaptiveTextBlock(){ Text=texta,Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
                    },
                    Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                                .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = menuurl } })
                                .ToList<AdaptiveAction>(),
                };

                return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
            }

        //public string Findmenu(string json,object sender)
        //{
        //    var rootmoduel = GetStoregroup(json);
        //    JObject o = new JObject();
        //    o["properties"] = rootmoduel;
        //    string namejson = o.ToString();
        //    var root = JsonConvert.DeserializeObject<Storenamegroup>(namejson);


        //    string btn = (string)sender;

        //    //var click = new System.EventHandler(this.Findmenudata).Target;
        //    var menu = root.properties.FirstOrDefault(x => x.Store_Name.ToString().Equals((sender)));
        //    var w = new WebCrawler();
        //    var menujson = w.GetOrderInfo(menu.Store_Url);
        //    var jsonmenudata = JsonConvert.SerializeObject(menujson);
        //    return jsonmenudata;
        //}

        private void MenuModule(AdaptiveColumnSet ColumnSetitem, string foodname, string money,string Dishname)
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
                Inputnumberiitem.Id = Dishname+"Quantity"+money;
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
                InputRemarksiitem.Id = Dishname + "Remarks"+ money;
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
                    card.Body.Add(ColumnSetitemname);
                    foreach (var p in root.Menuproperties)
                        {
                            var ColumnSetitem = new AdaptiveColumnSet();
                            MenuModule(ColumnSetitem, p.Dish_Name, p.Price, p.Dish_Name);
                            card.Body.Add(ColumnSetitem);
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
