//using AdaptiveCards;
//using AdaptiveCards.Templating;
//using BuildSchoolBot.StoreModels;
//using Microsoft.Bot.Builder;
//using Microsoft.Bot.Builder.Teams;
//using Microsoft.Bot.Schema;
//using Microsoft.Bot.Schema.Teams;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BuildSchoolBot.Bots
//{
//    public class OrderfoodBot : TeamsActivityHandler
//    {
//        string StoreData = "";
//        public OrderfoodBot()
//        {

//            StoreData = @"{
//    ""properties"": [
//        {
//            ""StoreName"": ""鬆餅店"",
//            ""menuproperties"": [
//                    {
//                        ""key"": ""蜂蜜鬆餅及紅茶(綠茶)"",
//                        ""value"": ""NT$70""
//                    },
//                    {
//                         ""key"": ""珍珠奶茶"",
//                        ""value"": ""NT$50""
//                    },
//                    {
//                        ""key"": ""果汁"",
//                        ""value"": ""NT$30""
//                    },
//                    {
//                        ""key"": ""ccccccc"",
//                        ""value"": ""NT$70""
//                    },
//                    {
//                        ""key"": ""bbbbbbbb"",
//                        ""value"": ""NT$70""
//                    },
//                    {
//                        ""key"": ""aaaaa"",
//                        ""value"": ""NT$70""
//                    },
//                    {
//                        ""key"": ""sacsac"",
//                        ""value"": ""NT$70""
//                    },
//                ]
//        },
//        {
//             ""StoreName"": ""蛋糕店"",
//           ""menuproperties"": [
//                    {
//                        ""key"": ""甜甜圈"",
//                        ""value"": ""NT$30""
//                    },
//                    {
//                         ""key"": ""蛋糕"",
//                        ""value"": ""NT$50""
//                    },
//                    {
//                        ""key"": ""波蘿麵包"",
//                        ""value"": ""NT$30""
//                    },          
//                ]
//        },
//        {
//            ""StoreName"": ""小吃店"",
//             ""menuproperties"": [
//                    {
//                        ""key"": ""滷肉飯"",
//                        ""value"": ""NT$30""
//                    },
//                    {
//                         ""key"": ""乾麵"",
//                        ""value"": ""NT$50""
//                    },
//                    {
//                        ""key"": ""炒飯"",
//                        ""value"": ""NT$60""
//                    },          
//                ]
//        },
//    ]
//}";

//        }

//        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
//        {
//            var root = JsonConvert.DeserializeObject<Storenamegroup>(StoreData);
//            var attachments = new List<Attachment>();
//            var reply = MessageFactory.Attachment(attachments);

//            foreach (var p in root.properties)
//            {
//                reply.Attachments.Add(GetStore(p.StoreName));
//            }


//            //var reply = MessageFactory.Attachment(new[] {GetTaskModuleHeroCardOptions()});
//            //要改成點擊按鈕
//            await turnContext.SendActivityAsync(reply, cancellationToken);
//        }

//        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
//        {
//            var asJobject = JObject.FromObject(taskModuleRequest.Data);
//            var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;

//            var taskInfo = new TaskModuleTaskInfo();
//            switch (value)
//            {
//                //case TaskModuleIds.YouTube:
//                //    taskInfo.Url = taskInfo.FallbackUrl = _baseUrl + "/" + TaskModuleIds.YouTube;
//                //    SetTaskInfo(taskInfo, TaskModuleUIConstants.YouTube);
//                //    break;
//                case TaskModuleIds.AdaptiveCard:
//                    taskInfo.Card = CreateClickfoodModule();
//                    SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
//                    break;
//                //case TaskModuleIds.food:
//                //    taskInfo.Title = TaskModuleIds.food;
//                //    SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
//                //    break;
//                //case TaskModuleIds.Click:
//                //    taskInfo.Title = TaskModuleIds.Click;
//                //    SetTaskInfo(taskInfo, TaskModuleUIConstants.Click);
//                //break;
//                default:
//                    break;
//            }

//            return Task.FromResult(taskInfo.ToTaskModuleResponse());
//        }

//        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
//        {
//            dynamic orderAttachitem = ((dynamic)taskModuleRequest.Data);
//            string orderAttachitemtext = orderAttachitem.undefined;
//            //var reply = MessageFactory.Text("OnTeamsTaskModuleSubmitAsync Value: " + orderAttachitem);

//            var attachments = new List<Attachment>();
//            var reply = MessageFactory.Attachment(attachments);

//            reply.Attachments.Add(GetResultClickfood(orderAttachitemtext));



//            //var reply = MessageFactory.Attachment(new[] {GetTaskModuleHeroCardOptions()});
//            //要改成點擊按鈕
//            await turnContext.SendActivityAsync(reply, cancellationToken);



//            return TaskModuleResponseFactory.CreateResponse("感謝您的點餐");
//        }


//        private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
//        {
//            taskInfo.Height = uIConstants.Height;
//            taskInfo.Width = uIConstants.Width;
//            taskInfo.Title = uIConstants.Title.ToString();
//        }


//        private static Attachment GetStore(string texta)
//        {
//            // Create an Adaptive Card with an AdaptiveSubmitAction for each Task Module
//            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
//            {

//                Body = new List<AdaptiveElement>()
//                    {
//                        new AdaptiveTextBlock(){ Text=texta,Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
//                    },
//                Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
//                            .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = cardType.Id } })
//                            .ToList<AdaptiveAction>(),
//            };

//            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
//        }




//        private static Attachment GetResultClickfood(string Clickfood)
//        {
//            // Create an Adaptive Card with an AdaptiveSubmitAction for each Task Module
//            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
//            {

//                Body = new List<AdaptiveElement>()
//                    {
//                        new AdaptiveTextBlock(){ Text=Clickfood,Weight=AdaptiveTextWeight.Bolder, Size=AdaptiveTextSize.Large}
//                    }
//            };

//            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
//        }


//        private Attachment CreateAdaptiveCardAttachment()
//        {
//            var cardResourcePath = "Clickfoodmodule.json";

//            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
//            {
//                using (var reader = new StreamReader(stream))
//                {
//                    var adaptiveCard = reader.ReadToEnd();
//                    return new Attachment()
//                    {
//                        ContentType = "application/vnd.microsoft.card.adaptive",
//                        Content = JsonConvert.DeserializeObject(adaptiveCard),
//                    };
//                }
//            }
//        }


//        private Attachment CreateClickfoodModule()
//        {
//            var cardResourcePath = "CoreBot.Cards.Clickfoodmodule.json";
//            using (var stream = GetType().Assembly.GetManifestResourceStream(cardResourcePath))
//            {
//                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//                using (var reader = new StreamReader(stream, Encoding.GetEncoding(950), true))
//                {
//                    var adaptiveCard = reader.ReadToEnd();
//                    string menu = "";
//                    var root = JsonConvert.DeserializeObject<Storenamegroup>(StoreData);

//                    //        var jsonData = @"{
//                    //    ""properties"": [
//                    //        {
//                    //            ""key"": ""蜂蜜鬆餅及紅茶(綠茶)"",
//                    //            ""value"": ""NT$70""
//                    //        },
//                    //        {
//                    //             ""key"": ""珍珠奶茶"",
//                    //            ""value"": ""NT$50""
//                    //        },
//                    //        {
//                    //            ""key"": ""果汁"",
//                    //            ""value"": ""NT$30""
//                    //        },
//                    //        {
//                    //            ""key"": ""ccccccc"",
//                    //            ""value"": ""NT$70""
//                    //        },
//                    //        {
//                    //            ""key"": ""bbbbbbbb"",
//                    //            ""value"": ""NT$70""
//                    //        },
//                    //        {
//                    //            ""key"": ""aaaaa"",
//                    //            ""value"": ""NT$70""
//                    //        },
//                    //        {
//                    //            ""key"": ""sacsac"",
//                    //            ""value"": ""NT$70""
//                    //        },
//                    //    ]
//                    //}";

//                    AdaptiveCardTemplate template = new AdaptiveCardTemplate(adaptiveCard);
//                    var context = new EvaluationContext
//                    {
//                        Root = jsonData
//                    };

//                    //// You can use any serializable object as your data
//                    //var card = template.expand({
//                    //    context});

//                    // "Expand" the template - this generates the final Adaptive Card payload
//                    var cardJson = template.Expand(context);

//                    return new Attachment()
//                    {
//                        ContentType = "application/vnd.microsoft.card.adaptive",
//                        Content = JsonConvert.DeserializeObject(cardJson),
//                    };
//                }
//            }




//        }
//    }
//}
