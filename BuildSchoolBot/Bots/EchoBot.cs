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
            //var test = turnContext.Activity.Value.ToString().Split('"') ;
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
        //�����s�����[�J
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
            var TaskInfo = new TaskModuleTaskInfo();
            var Data = JObject.FromObject(taskModuleRequest.Data);
            // Customized Card
            if (Data.GetValue("data").ToString().Equals("Customized"))
            {

                var TenantId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Tenant?.Id; ;

                TaskInfo.Card = _menuOrderService.CreateAdaptiveCardAttachment(TenantId);
                return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
            }
            if (taskModuleRequest.Data.ToString().Split('"').FirstOrDefault(x => x.Equals("GetStore")) == "GetStore")
            {
                var StoreModule = new GetStoreList();
                return await StoreModule.OnTeamsTaskModuleFetchAsync(taskModuleRequest);
            }
            //育銨
            if (JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data)).Property("SetType").Value.ToString() == "CustomizedModification")
            {
                return await _orderfoodServices.GetModifyModuleData(turnContext, taskModuleRequest, cancellationToken);
            }
            else
            {
                return await _orderfoodServices.GetModuleMenuData(turnContext, taskModuleRequest, cancellationToken);
            }
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            //�a�_
            if (taskModuleRequest.Data.ToString().Split('"').FirstOrDefault(x => x.Equals("ResultStoreCard")).Equals("ResultStoreCard"))
            {
                var result = new GetUserChosedStore().GetResultStore(taskModuleRequest.Data.ToString())[0];
                await turnContext.SendActivityAsync(MessageFactory.Attachment(_createCardService.GetStore(result.StoreName,result.Url,result.OrderID,result.DueTime)));
                return null;
            }
            //育銨
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
            _libraryService.LibraryCreateOrDelete(turnContext, cancellationToken);


            return await Task.FromResult(new InvokeResponse()
            {
                Status = 200
            });
        }

    }
}
