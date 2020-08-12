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
        protected readonly PayMentService _paymentService;
        protected readonly MenuService _menuService;
        protected readonly MenuDetailService _menuDetailService;

        public EchoBot(ConversationState conversationState, LibraryService libraryService, OrderService orderService, OrderDetailService orderDetailService, UserState userState, T dialog, OrderfoodServices orderfoodServices, ISchedulerFactory schedulerFactory, ConcurrentDictionary<string, ConversationReference> conversationReferences, CreateCardService createCardService, OrganizeStructureService organizeStructureService, PayMentService paymentService, MenuService menuService, MenuDetailService menuDetailService)
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
            _paymentService = paymentService;
            _menuService = menuService;
            _menuDetailService = menuDetailService;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //var test = turnContext.Activity.Value.ToString().Split('"') ;
            if (turnContext.Activity.Text.Contains("Library"))
            {
                var libraryCard = await GetLibraryCard(turnContext);

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
                foreach(var data in channel)
                {
                    var str = data.Name + "\r\n" + data.Id;
                    await turnContext.SendActivityAsync(MessageFactory.Text(str));
                }
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
        //當有新成員加入
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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            //家寶
            if (taskModuleRequest.Data.ToString().Split('"').FirstOrDefault(x => x.Equals("GetStore")) == "GetStore")
            {
                var StoreModule = new GetStoreList();
                return await StoreModule.OnTeamsTaskModuleFetchAsync(taskModuleRequest);
            }
            //育安
            if (JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data)).Property("SetType").Value.ToString() == "CustomizedModification")
            {
                return await _orderfoodServices.GetModifyModuleData(turnContext, taskModuleRequest, cancellationToken);
            }
            //else if (JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data)).Property("SetType").Value.ToString() == "test")
            //{
            //    var TaskInfo = new TaskModuleTaskInfo();
            //    TeamsBuyContext context = new TeamsBuyContext();
            //    var asJobject = JObject.FromObject(taskModuleRequest.Data);
            //    var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            //    //var MenuOrderData = new MenuDetailService(context).GetMenuOrder(Value).ToList();
            //    //var MenuOrderStore = new MenuService(context).GetMenuOrder(Value).Store;
            //    TaskInfo.Card = new CreateCardService().GetMenuModule("","","","");
            //   _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
            //    return await Task.FromResult(TaskInfo.ToTaskModuleResponse());               
            //}
            else
            {
                return await _orderfoodServices.GetModuleMenuData(turnContext, taskModuleRequest, cancellationToken); 
            }
    }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            //家寶
            if (taskModuleRequest.Data.ToString().Split('"').FirstOrDefault(x => x.Equals("ResultStoreCard")).Equals("ResultStoreCard"))
            {
                var result = new GetUserChosedStore().GetResultStore(taskModuleRequest.Data.ToString())[0];
                var w = new CreateCardService();
                var o = new OrderfoodServices();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(w.GetStore(result.StoreName,result.Url,result.OrderID)));
                return null;
            }
            //育安
            if (JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data)).Property("SetType").Value.ToString() == "CustomizedModification")
            {
                var TaskInfo = new TaskModuleTaskInfo();
                _orderfoodServices.ModifyMenuData(taskModuleRequest, TaskInfo);
                return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
            }
            else
            {
                return await _orderfoodServices.FinishSelectDishesSubmit(turnContext, taskModuleRequest, cancellationToken);
            }
        }
        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var memberId = turnContext.Activity.From.Id;
            var obj = JObject.FromObject(turnContext.Activity.Value).ToObject<ViewModels.MsteamsValue>();

            if (obj.Option.Equals("Create"))
            {
                var uri = obj.Url;
                var LibraryItem = await _libraryService.FindLibraryByUriAndMemberId(uri, memberId);

                if (LibraryItem.Count.Equals(0))
                    _libraryService.CreateLibraryItem(memberId, obj.Url, obj.Name);
            }
            else if (obj.Option.Equals("Delete"))
            {
                var LibraryId = obj.LibraryId;

                Guid guid;
                Guid.TryParse(LibraryId.ToString(), out guid);
                _libraryService.DeleteLibraryItem(guid);

                var libraryCard = await GetLibraryCard(turnContext);

                var activity = MessageFactory.Attachment(libraryCard);
                activity.Id = turnContext.Activity.ReplyToId;

                await turnContext.UpdateActivityAsync(activity, cancellationToken);
            }
            return await Task.FromResult(new InvokeResponse()
            {
                Status = 200
            });
        }

        private async Task<Attachment> GetLibraryCard(ITurnContext turnContext)
        {
            var memberId = turnContext.Activity.From.Id;

            var Name = turnContext.Activity.From.Name;
            var libraries = await _libraryService.FindLibraryByMemberId(memberId);
            var libraryCard = Service.LibraryService.CreateAdaptiveCardAttachment(libraries, Name);

            return libraryCard;
        }
    }
}
