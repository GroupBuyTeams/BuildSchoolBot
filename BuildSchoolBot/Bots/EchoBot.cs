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

        public EchoBot(ConversationState conversationState, LibraryService libraryService, OrderService orderService, OrderDetailService orderDetailService, UserState userState, T dialog, OrderfoodServices orderfoodServices, ISchedulerFactory schedulerFactory, ConcurrentDictionary<string, ConversationReference> conversationReferences)
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
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            if (turnContext.Activity.Text.Contains("DeletedLibrary"))
            {
                dynamic obj = turnContext.Activity.Value;
                var LibraryId = obj.LibraryId;

                Guid guid;
                Guid.TryParse(LibraryId.ToString(), out guid);
                _libraryService.DeleteLibraryItem(guid);

                var libraryCard = await GetLibraryCard(turnContext);
                
                var activity = MessageFactory.Attachment(libraryCard);
                activity.Id = turnContext.Activity.ReplyToId;

                await turnContext.UpdateActivityAsync(activity, cancellationToken);
            }
            else if (turnContext.Activity.Text.Contains("Library"))
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
                if (string.IsNullOrEmpty(activity.Text) && activity.Value != null)
                {
                    activity.Text = JsonConvert.SerializeObject(activity.Value);
                }
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            }
        }

        private async Task<Attachment> GetLibraryCard(ITurnContext<IMessageActivity> turnContext)
        {
            var memberId = turnContext.Activity.From.Id;

            var Name = turnContext.Activity.From.Name;
            var libraries = await _libraryService.FindLibraryByMemberId(memberId);
            var libraryCard = Service.LibraryService.CreateAdaptiveCardAttachment(libraries, Name);

            return libraryCard;
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
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
        protected async override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            var Storname = _orderfoodServices.GetLeftStr(value, "FoodData2468");
            var FoodAndGuidProcessUrl = _orderfoodServices.GetRightStr(value, "FoodData2468");
            var FoodUrl = _orderfoodServices.GetLeftStr(FoodAndGuidProcessUrl, "GuidStr13579");
            var Guidstr = _orderfoodServices.GetRightStr(FoodAndGuidProcessUrl, "GuidStr13579");
            var taskInfo = new TaskModuleTaskInfo();
            string Getmenujson = await new WebCrawler().GetOrderInfo(FoodUrl);
            var namejson = _orderfoodServices.ArrayPlusName(Getmenujson, "Menuproperties");
            taskInfo.Card = _orderfoodServices.CreateClickfoodModule(Guidstr, Storname, namejson);
            _orderfoodServices.SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var UserId = turnContext.Activity.From.Id;
            var UserName = turnContext.Activity.From.Name;
            var taskInfo = new TaskModuleTaskInfo();
            var taskModuleRequestjson = JsonConvert.SerializeObject(taskModuleRequest.Data);
            JObject data = JObject.Parse(taskModuleRequestjson);
            var StoreAndGuid = data.Property("data").Value.ToString();
            var StoreName = _orderfoodServices.GetLeftStr(StoreAndGuid, "FoodGuid2468");
            var guid = _orderfoodServices.GetRightStr(StoreAndGuid, "FoodGuid2468");
            data.Property("msteams").Remove();
            data.Property("data").Remove();
            var MenudataGroups = data.ToString();
            string SelectJson = _orderfoodServices.ProcessAllSelect(data);
            JArray array = JArray.Parse(SelectJson);
            JObject o = new JObject();
            o["SelectMenu"] = array;
            string json = o.ToString();
            //��Ƨ���
            var OAllOrderDatasStr = _orderfoodServices.ProcessUnifyData(o);
            var SelectObject = JsonConvert.DeserializeObject<SelectAllDataGroup>(OAllOrderDatasStr);
            SelectObject.UserID = UserId;
            SelectObject.StoreName = StoreName;
            var SelectAllDataJson = JsonConvert.SerializeObject(SelectObject);
            var ExistGuid = Guid.Parse("cf1ed7b9-ae4a-4832-a9f4-fdee6e492085");
            _orderDetailService.CreateOrderDetail(SelectObject, SelectObject.SelectAllOrders, ExistGuid);
            //_orderService.CreateOrder()
            taskInfo.Card = _orderfoodServices.GetResultClickfood(guid, StoreName, json, "12:00", UserName);
            _orderfoodServices.SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(_orderfoodServices.GetResultClickfood(guid, StoreName, json, "12:00", UserName)));
            return await Task.FromResult(taskInfo.ToTaskModuleResponse());
        }

    }
}
