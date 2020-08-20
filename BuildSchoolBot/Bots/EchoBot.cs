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
using BuildSchoolBot.ViewModels;

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
        protected readonly CustomMenuService _customMenuService;
        protected readonly MenuOrderService _menuOrderService;
        public EchoBot(ConversationState conversationState, LibraryService libraryService, OrderService orderService, OrderDetailService orderDetailService, UserState userState, T dialog, OrderfoodServices orderfoodServices, ISchedulerFactory schedulerFactory, ConcurrentDictionary<string, ConversationReference> conversationReferences, CreateCardService createCardService, OrganizeStructureService organizeStructureService, PayMentService paymentService, MenuService menuService, MenuDetailService menuDetailService, CustomMenuService customMenuService, MenuOrderService menuOrderService)
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
            _customMenuService = customMenuService;
            _menuOrderService = menuOrderService;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
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
            else if (turnContext.Activity.Text.Contains("Customized Menu"))
            {
                var CustomMenucard = _customMenuService.CallCustomeCard();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(CustomMenucard), cancellationToken);
            }
            else if (turnContext.Activity.Text.Contains("Help"))
            {
                var help = new HelpService();
                var card = help.IntroductionCard();
                var command = help.Command();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text("You can give command"), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(command), cancellationToken);
            }
            //ting 要移動到訂單完成那邊 回覆pay
            else if (turnContext.Activity.Text.Contains("aaa"))
            {
                var memberId = turnContext.Activity.From.Id;
                var card = new CreateCardService2().ReplyPayment(_paymentService, turnContext);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(card), cancellationToken);
            }
            else
            {
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
            await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to Groupbuy."));
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

            var factory = new AdaptiveCardDataFactory(turnContext, taskModuleRequest);
            var fetchType = factory.GetCardActionType();
            var service = new CreateCardService2();
            var taskInfo = new TaskModuleTaskInfo();
            //ting create Customized menu taskmodule
            if (fetchType?.Equals("createmenu") == true)
            {
                taskInfo.Card = service.GetCreateMenu();
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }
            //create menu detail
            else if (fetchType?.Equals("CreateMenuDetail") == true)
            {
                var menu = factory.GetCardData<string>();

                taskInfo.Card = service.GetCreateMenuDetail(menu);
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }
            //Group  Buy Open Menu
            if (fetchType?.Equals("OpenMenuTaskModule") == true)
            {
                taskInfo.Card = await service.CreateMenu(factory);
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }
            // Customized Card
            if (fetchType?.Equals("Customized") == true)
            {
                var TenantId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Tenant?.Id;
                TaskInfo.Card = _menuOrderService.CreateMenuOrderAttachment(TenantId);
                return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
            }
            else if (fetchType?.Equals("GetCustomizedMenu") == true)
            {
                taskInfo.Card = await _menuOrderService.CreateMenu(factory);
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }
            //家寶
            if (fetchType?.Equals("GetStore") == true)
            {
                taskInfo.Card = await new GetStoreList().CreateStoresModule(factory);
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }
            //育銨
            else if (fetchType?.Equals("GetChosenFoodFromMenuData") == true)
            {
                TaskInfo.Card = new CreateCardService2().GetChosenFoodFromMenuModule(factory);
                service.SetTaskInfo(TaskInfo, TaskModuleUIConstants.ChosenData);
                return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
            }
            else
            {
                taskInfo.Card = service.GetCustomizedModification(factory);
                service.SetTaskInfo(taskInfo, TaskModuleUIConstants.UpdateMenu);
                return await Task.FromResult(taskInfo.ToTaskModuleResponse());
            }
        }
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var factory = new AdaptiveCardDataFactory(turnContext, taskModuleRequest);
            var fetchType = factory.GetCardActionType();

            if (fetchType?.Equals("ResultStoreCard") == true)
            {
                var orderId = Guid.NewGuid().ToString();
                var data = factory.GetGroupBuyCard(orderId);
                _orderService.CreateOrder(orderId, turnContext.Activity.ChannelId, data.StoreName);
                var cardService = new CreateCardService2();
                await turnContext.SendActivityAsync(MessageFactory.Attachment(cardService.GetStore(data)));
                return null;
            }
            if (fetchType?.Equals("FetchSelectedFoods") == true)
            {
                var card = new CreateCardService2().GetChosenFoodFromMenu(factory);

                if (card.Name?.Contains("error") == true)
                {
                    var taskInfo = new TaskModuleTaskInfo();
                    taskInfo.Card = card;
                    return await Task.FromResult(taskInfo.ToTaskModuleResponse());
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(card));
                    new CreateCardService2().GetChosenFoodFromMenuCreateOrderDetail(factory, turnContext.Activity.From.Id);
                    return null;
                }
            }
            else if (fetchType?.Equals("GetCustomizedStore") == true)
            {
                var result = _menuOrderService.GetStore(factory);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(result));
                return null;
            }
            //ting 按下按鈕傳資料到data
            else if (fetchType?.Equals("GetCustomizedMenu") == true)
            {
                var teamsId = turnContext.Activity.GetChannelData<TeamsChannelData>()?.Tenant?.Id;
                var menu = _menuService.CreateMenu(factory, teamsId);
                if (menu == null)
                    await turnContext.SendActivityAsync(MessageFactory.Text("Please create your store first!"));
                else
                {
                    _menuService.CreateMenuDetail(factory, menu.MenuId);
                    await turnContext.SendActivityAsync(MessageFactory.Text("Create Successfully!"));
                }
                return null;
            }
            else if (fetchType?.Equals("GetCustomizedMenuDetail") == true)
            {
                var menu = factory.GetCardData<StoreInfoData>().Guid;
                _menuService.CreateMenuDetail(factory,Guid.Parse(menu));
                await turnContext.SendActivityAsync(MessageFactory.Text("Create Successfully!"));
                return null;
            }
            //育銨
            else
            { 
                var TaskInfo = new TaskModuleTaskInfo();
                TaskInfo.Card = new CreateCardService2().GetResultCustomizedModification(factory);
                new CreateCardService2().SetTaskInfo(TaskInfo, TaskModuleUIConstants.UpdateMenu);
                return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
            }
        }
        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var memberId = turnContext.Activity.From.Id;
            var obj = JObject.FromObject(turnContext.Activity.Value).ToObject<ViewModels.MsteamsValue>();
            if (obj?.Option?.Equals("Create") == true)
            {
                var uri = obj.Url;
                var LibraryItem = await _libraryService.FindLibraryByUriAndMemberId(uri, memberId);
                if (LibraryItem.Count.Equals(0))
                    _libraryService.CreateLibraryItem(memberId, obj.Url, obj.Name);
                await turnContext.SendActivityAsync(MessageFactory.Text("Create Successfully!"));
            }
            else if (obj?.Option?.Equals("Delete") == true)
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
            else if (obj?.Option?.Equals("DeleteMenu") == true)
            {
                var MenuId = obj.MenuId;
                Guid guid;
                Guid.TryParse(MenuId.ToString(), out guid);
                _customMenuService.DeleteOrderDetail(guid);
                await turnContext.SendActivityAsync(MessageFactory.Text("Delete Successfully!"));
            }
            //ting deleteOrder
            //else if (obj.Option?.Equals("DeleteOrder") == true)
            //{
            //    var OrderId = obj.OrderId;
            //    Guid guid;
            //    Guid.TryParse(OrderId.ToString(), out guid);
            //    _orderService.DeleteStore(guid);
            //    await turnContext.SendActivityAsync(MessageFactory.Text("Delete Successful!"));
            //}
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