// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using BuildSchoolBot.Models;
using AdaptiveCards;
using System.Net;
using System.Xml.Linq;
using System;
using System.IO;
using Newtonsoft.Json;
using BuildSchoolBot.Service;
using Microsoft.Bot.Schema.Teams;
using Quartz;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System.Linq;
using BuildSchoolBot.StoreModels;
using System.Net.Http;
using BuildSchoolBot.ViewModels;
using Newtonsoft.Json.Linq;
using static BuildSchoolBot.StoreModels.AllSelectData;
using static BuildSchoolBot.StoreModels.SelectMenu;
using static BuildSchoolBot.StoreModels.ModifyMenu;
using BuildSchoolBot.Dialogs;

namespace BuildSchoolBot.Bots
{
    public class EchoBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly LibraryService _libraryService;
        protected readonly ISchedulerFactory SchedulerFactory;
        protected readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;
        protected readonly OrderfoodServices _orderfoodServices;
        protected readonly OrderService _orderService;
        protected readonly OrderDetailService _orderDetailService;
        protected readonly CreateCardService _createCardService;
        protected readonly OrganizeStructureService _organizeStructureService;
        protected readonly MenuOrderService _menuOrderService;
        protected readonly PayMentService _paymentService;
        protected readonly MenuService _menuService;
        protected readonly MenuDetailService _menuDetailService;
        protected readonly CustomMenuService _customMenuService;

        public EchoBot(ConversationState conversationState, LibraryService libraryService, OrderService orderService, OrderDetailService orderDetailService, UserState userState, T dialog, OrderfoodServices orderfoodServices, ISchedulerFactory schedulerFactory, ConcurrentDictionary<string, ConversationReference> conversationReferences, CreateCardService createCardService, OrganizeStructureService organizeStructureService, PayMentService paymentService, MenuService menuService, MenuDetailService menuDetailService, MenuOrderService menuOrderService, CustomMenuService customMenuService)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            _libraryService = libraryService;
            SchedulerFactory = schedulerFactory;
            ConversationReferences = conversationReferences;
            _orderfoodServices = orderfoodServices;
            _orderService = orderService;
            _orderDetailService = orderDetailService;
            _createCardService = createCardService;
            _organizeStructureService = organizeStructureService;
            _menuOrderService = menuOrderService;
            _paymentService = paymentService;
            _menuService = menuService;
            _menuDetailService = menuDetailService;
            _customMenuService = customMenuService;

        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Text.Contains("Library"))
            {
                var libraryCard = await _libraryService.GetLibraryCard(turnContext);

                await turnContext.SendActivityAsync(MessageFactory.Attachment(libraryCard), cancellationToken);
            }
            else if (turnContext.Activity.Text.Contains("Pay"))
            {
                var memberId = turnContext.Activity.From.Id;
                var payCard = _paymentService.CreatePayAdaptiveAttachment(memberId);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(payCard), cancellationToken);
            }
            else if (turnContext.Activity.Text.Contains("payment"))
            {
                var memberId = turnContext.Activity.From.Id;
                var url = JObject.FromObject(turnContext.Activity.Value).GetValue("payment").ToString();
                _paymentService.UpdatePayment(memberId, url);
                var payCard = _paymentService.CreatePayAdaptiveAttachment(memberId);

                var activity = MessageFactory.Attachment(payCard);
                activity.Id = turnContext.Activity.ReplyToId;

                await turnContext.UpdateActivityAsync(activity, cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("You update your payment link: " + url), cancellationToken);
            }
            //Only for Demo. 
            //please don't delete it, please don't delete it, please don't delete it!!!!

            else if (turnContext.Activity.Text.Contains("ccc"))
            {
                var services = await SchedulerFactory.GetAllSchedulers();
                var scheduler = new ScheduleCreator(services[0], turnContext.Activity.From.Id, "GUID");
                AddConversationReference(turnContext.Activity as Activity);
                scheduler.CreateSingleGroupBuyNow(DateTime.Now.AddSeconds(15.0f));
                await turnContext.SendActivityAsync(MessageFactory.Text("schedule a group buy."));
            }
            else if (turnContext.Activity.Text.Contains("userid"))
            {
                var datas = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);
                foreach (var data in datas)
                {
                    var str = data.Name + "\r\n" + data.Id;
                    await turnContext.SendActivityAsync(MessageFactory.Text(str));
                }
            }
            else if (turnContext.Activity.Text.Contains("channel"))
            {
                var channel = await TeamsInfo.GetTeamChannelsAsync(turnContext);
                foreach (var data in channel)
                {
                    var str = data.Name + "\r\n" + data.Id;
                    await turnContext.SendActivityAsync(MessageFactory.Text(str));
                }
            }
            else if (turnContext.Activity.Text.Contains("Custom Menu"))
            {
                var CustomMenucard = _customMenuService.CallCustomeCard();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CustomMenucard), cancellationToken);
            }
            else
            {
                var activity = turnContext.Activity;
                if (string.IsNullOrWhiteSpace(activity.Text) && activity.Value != null)
                {
                    activity.Text = JsonConvert.SerializeObject(activity.Value);
                }
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            ConversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to GruopBuyBot!");
                    var paths = new[] { ".", "Resources", "IntroductionCard.json" };
                    var adaptiveCard = File.ReadAllText(Path.Combine(paths));
                    var attachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                    reply.Attachments.Add(attachment);

                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }
        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            var activity = turnContext.Activity;

            if (string.IsNullOrWhiteSpace(activity.Text) && activity.Value != null)
            {
                activity.Text = JsonConvert.SerializeObject(activity.Value);
            }
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
<<<<<<< HEAD
        
        //by Afan
        // protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        // {
        //     var asJobject = JObject.FromObject(taskModuleRequest.Data);
        //     var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
        //     string GetMenuJson = _organizeStructureService.GetFoodUrlStr(Value);
        //     var TaskInfo = new TaskModuleTaskInfo();
        //     TaskInfo.Card = _organizeStructureService.GetTaskModuleFetchCard(Value, GetMenuJson,TaskInfo);
        //     _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
        //     return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        // }

        //by 阿三
=======
>>>>>>> dev
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var factory = new AdaptiveCardDataFactory(turnContext, taskModuleRequest);
            var taskInfo = new TaskModuleTaskInfo();
            var actionType = factory.GetCardActionType();
            
            if (actionType.Equals("OpenMenuTaskModule"))
            {
                taskInfo.Card = await new CreateCardService2().CreateMenu(factory);
            }
            
            _orderfoodServices.SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());
        }
<<<<<<< HEAD
        //尚未完成，要調整
        // protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        // {
        //     var asJObject = JObject.FromObject(taskModuleRequest.Data);
        //     var value = asJObject.ToObject<CardTaskFetchValue<string>>()?.Data;
        //     var TaskInfo = new TaskModuleTaskInfo();
        //     JObject Data = JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data));         
        //     var StoreAndGuid = Data.Property("data").Value.ToString();
        //     _organizeStructureService.RemoveNeedlessStructure(Data);
        //     string SelectJson = _orderfoodServices.ProcessAllSelect(Data);
        //     JObject o = new JObject();
        //     o["SelectMenu"] = JArray.Parse(SelectJson);
        //     bool DecideQuanRem = true;
        //     bool Number = true;
        //     var AllSelectDatas = JsonConvert.DeserializeObject<SelectMenuDatagroup>(o.ToString());
        //     foreach (var item in AllSelectDatas.SelectMenu)
        //     {
        //         if (item.Quantity == "0" && item.Remarks != "")
        //         {
        //             DecideQuanRem = false;
        //         }
        //         if (Math.Sign(decimal.Parse(item.Quantity)) < 0 || (decimal.Parse(item.Quantity) - Math.Floor(decimal.Parse(item.Quantity))) != 0)
        //         {
        //             Number = false;
        //         }
        //     }
        //     if (DecideQuanRem == true && Number == true)
        //     {
        //         //取完整資料
        //         var OAllOrderDatasStr = _orderfoodServices.ProcessUnifyData(o);
        //         var SelectObject = JsonConvert.DeserializeObject<SelectAllDataGroup>(OAllOrderDatasStr);
        //         SelectObject.UserID = turnContext.Activity.From.Id;
        //         var ExistGuid = Guid.Parse("cf1ed7b9-ae4a-4832-a9f4-fdee6e492085");
        //         //_orderDetailService.CreateOrderDetail(SelectObject, SelectObject.SelectAllOrders, ExistGuid);
        //
        //         TaskInfo.Card = _createCardService.GetResultClickfood(_organizeStructureService.GetOrderID(StoreAndGuid),_organizeStructureService.GetStoreName(StoreAndGuid), o.ToString(), "12:00", turnContext.Activity.From.Name);
        //         _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
        //         await turnContext.SendActivityAsync(MessageFactory.Attachment(_createCardService.GetResultClickfood(_organizeStructureService.GetOrderID(StoreAndGuid), _organizeStructureService.GetStoreName(StoreAndGuid), o.ToString(), "12:00", turnContext.Activity.From.Name)));
        //     }
        //     else
        //     {
        //         TaskInfo.Card = _createCardService.GetError(turnContext.Activity.From.Name);
        //         _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
        //         await turnContext.SendActivityAsync(MessageFactory.Attachment(_createCardService.GetError(turnContext.Activity.From.Name)));
        //
        //     }
        //     return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        // }
        //
=======
        
>>>>>>> dev
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            // ?���其耨甇�銝?
            var factory = new AdaptiveCardDataFactory(turnContext, taskModuleRequest);
            var fetchType = factory.GetCardActionType();
            var taskInfo = new TaskModuleTaskInfo();

            if (fetchType.Equals("FetchSelectedFoods"))
            {
                
            }

            //
            if (taskModuleRequest.Data.ToString().Split('"').FirstOrDefault(x => x.Equals("ResultStoreCard")).Equals("ResultStoreCard"))
            {
<<<<<<< HEAD
                if (item.Quantity == "0" && item.Remarks != "")
                {
                    DecideQuanRem = false;
                }
                if (Math.Sign(decimal.Parse(item.Quantity)) < 0 || (decimal.Parse(item.Quantity) - Math.Floor(decimal.Parse(item.Quantity))) != 0)
                {
                    Number = false;
                }
=======
                var result = new GetUserChosedStore().GetResultStore(taskModuleRequest.Data.ToString())[0];
                var w = new CreateCardService();
                var o = new OrderfoodServices();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(w.GetStore(result.StoreName, result.Url)));
        
                return null;
>>>>>>> dev
            }
            //嚙罵嚙緩
            if (JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data)).Property("SetType").Value.ToString() == "CustomizedModification")
            {
<<<<<<< HEAD
                //取完整資料
                var OAllOrderDatasStr = _orderfoodServices.ProcessUnifyData(o);
                var SelectObject = JsonConvert.DeserializeObject<SelectAllDataGroup>(OAllOrderDatasStr);
                SelectObject.UserID = turnContext.Activity.From.Id;
                var ExistGuid = Guid.Parse("cf1ed7b9-ae4a-4832-a9f4-fdee6e492085");
                //_orderDetailService.CreateOrderDetail(SelectObject, SelectObject.SelectAllOrders, ExistGuid);
        
                TaskInfo.Card = _createCardService.GetResultClickfood(_organizeStructureService.GetOrderID(StoreAndGuid),_organizeStructureService.GetStoreName(StoreAndGuid), o.ToString(), "12:00", turnContext.Activity.From.Name);
                _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(_createCardService.GetResultClickfood(_organizeStructureService.GetOrderID(StoreAndGuid), _organizeStructureService.GetStoreName(StoreAndGuid), o.ToString(), "12:00", turnContext.Activity.From.Name)));
            }
            else
            {
                TaskInfo.Card = _createCardService.GetError(turnContext.Activity.From.Name);
                _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(_createCardService.GetError(turnContext.Activity.From.Name)));
        
=======
                var TaskInfo = new TaskModuleTaskInfo();
                _orderfoodServices.ModifyMenuData(taskModuleRequest, TaskInfo);
                return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
            }
            else
            {
                return await _orderfoodServices.FinishSelectDishesSubmit(turnContext, taskModuleRequest, cancellationToken);
>>>>>>> dev
            }
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());

        }

        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            _libraryService.LibraryCreateOrDelete(turnContext, cancellationToken);


            return await Task.FromResult(new InvokeResponse()
            {
                Status = 200
            });
        }

    }
}
