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

        public EchoBot(ConversationState conversationState, LibraryService libraryService, OrderService orderService, OrderDetailService orderDetailService, UserState userState, T dialog, OrderfoodServices orderfoodServices, ISchedulerFactory schedulerFactory, ConcurrentDictionary<string, ConversationReference> conversationReferences, CreateCardService createCardService, OrganizeStructureService organizeStructureService)
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
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Text.Contains("Library"))
            {
                var libraryCard = await GetLibraryCard(turnContext);

                await turnContext.SendActivityAsync(MessageFactory.Attachment(libraryCard), cancellationToken);
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
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            //家寶
            if (taskModuleRequest.Data.ToString().Split('"').FirstOrDefault(x => x.Equals("GetStore")) == "GetStore")
            {
                var StoreModule = new GetStoreList();
                return await StoreModule.OnTeamsTaskModuleFetchAsync(taskModuleRequest);
            }
            //育銨
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            string GetMenuJson = _organizeStructureService.GetFoodUrlStr(Value);
            var TaskInfo = new TaskModuleTaskInfo();
            TaskInfo.Card = _organizeStructureService.GetTaskModuleFetchCard(Value, GetMenuJson,TaskInfo);
            _orderfoodServices.SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var TaskInfo = new TaskModuleTaskInfo();
            JObject Data = JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data));         
            var StoreAndGuid = Data.Property("data").Value.ToString();
            _organizeStructureService.RemoveNeedlessStructure(Data);
            string SelectJson = _orderfoodServices.ProcessAllSelect(Data);
            JObject o = new JObject();
            o["SelectMenu"] = JArray.Parse(SelectJson);
            bool DecideQuanRem = true;
            bool Number = true;
            var AllSelectDatas = JsonConvert.DeserializeObject<SelectMenuDatagroup>(o.ToString());
            foreach (var item in AllSelectDatas.SelectMenu)
            {
                if (item.Quantity == "0" && item.Remarks != "")
                {
                    DecideQuanRem = false;
                }
                if (Math.Sign(decimal.Parse(item.Quantity)) < 0 || (decimal.Parse(item.Quantity) - Math.Floor(decimal.Parse(item.Quantity))) != 0)
                {
                    Number = false;
                }
            }
            if (DecideQuanRem == true && Number == true)
            {
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

            }
            return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        }
        protected override async Task<InvokeResponse> OnTeamsCardActionInvokeAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var memberId = turnContext.Activity.From.Id;
            dynamic obj = turnContext.Activity.Value;

            if (obj.Option.ToString().Contains("Create"))
            {
                _libraryService.CreateLibraryItem(memberId, obj.Url.ToString(), obj.Name.ToString());
            }
            else if (obj.Option.ToString().Contains("Delete"))
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
