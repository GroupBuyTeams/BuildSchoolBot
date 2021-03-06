﻿using AdaptiveCards;
using AngleSharp.Html.Dom;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BuildSchoolBot.Dialogs
{
    public class ReservationDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Schedule> _userProfileAccessor;
        private static ISchedulerFactory _schedulerFactory;
        private static EGRepository<Schedule> _schedRepo;
        private static ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        public ReservationDialog(UserState userState, AddressDialogs addressDialog, ISchedulerFactory schedulerFactory, EGRepository<Schedule> schedRepo, ConcurrentDictionary<string, ConversationReference> conversationReferences) : base(nameof(ReservationDialog))
        {
            _userProfileAccessor = userState.CreateProperty<Schedule>("Schedule");
            _schedulerFactory = schedulerFactory;
            _schedRepo = schedRepo;
            _conversationReferences = conversationReferences;
            var waterfallSteps = new WaterfallStep[]
            {
                CreateReservationAdaptive,
                OrderSourceAdaptive,
                GetMenuFromSource,
                StoreMenuData
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(addressDialog);
            AddDialog(new TextPrompt("DateTimeInput", DateTimeValidatorAsync));
            AddDialog(new TextPrompt(nameof(TextPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }
        //請使用者選擇時間
        private static async Task<DialogTurnResult> CreateReservationAdaptive(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ReservationCardAttachment = new CreateReservationCard().CreateReservationAdaptiveCard();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ReservationCardAttachment));
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please choose your reservation time."),
                RetryPrompt = MessageFactory.Text("You must provide your date and time."),
            };

            return await stepContext.PromptAsync("DateTimeInput", promptOptions, cancellationToken);
        }
        //請使用者選擇訂單來源
        private static async Task<DialogTurnResult> OrderSourceAdaptive(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            JObject timeData = stepContext.Context.Activity.Value as JObject;
            stepContext.Values["OrderTime"] = DateTime.Parse(timeData.GetValue("Date") + " " + timeData.GetValue("Time"));
            var OrderSourceCardAttachment = new CreateReservationCard().CreateOrderSourceAdaptiveCard();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(OrderSourceCardAttachment));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please pick an order source.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> GetMenuFromSource(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var source = stepContext.Context.Activity.Text;
            if(source.Contains("Quick Order")) {
                return await stepContext.BeginDialogAsync(nameof(AddressDialogs), "reserve");
            }
            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Coming soon.") }, cancellationToken);
            return await stepContext.EndDialogAsync();
        }

        private static async Task<DialogTurnResult> StoreMenuData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            StoreOrderDuetime storeData;
            try
            {
                storeData =
                    JsonConvert.DeserializeObject<StoreOrderDuetime>(stepContext.Context.Activity.Value.ToString());
            }
            catch
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("OOPS! It seems you didn't choose any menu.") }, cancellationToken);
            }

            if (storeData.DueTime == null)
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] -1;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("OOPS! I don't know when will this order END. Please order again!") }, cancellationToken);
            }
            
            var startTime = (DateTime) stepContext.Values["OrderTime"];
            var endTime = GetEndTime(startTime, storeData);

            if (startTime >= endTime)
            {
                stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] -1;
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("OOPS! The end time is earlier than start time. Please order again!") }, cancellationToken);
            }
            
            var sched = new Schedule()
            {
                ScheduleId = Guid.NewGuid(),
                GroupId = Guid.Empty,
                TriggerType = 1,
                TriggerTime = startTime,
                EndTime = endTime,
                MenuUri = storeData.Url,
                RepeatWeekdays = 0
            };

            // startTime = DateTime.Now.AddSeconds(5f);
            // endTime = DateTime.Now.AddSeconds(30f);
            var teamsChannelData =JsonConvert.DeserializeObject<dynamic>(stepContext.Context.Activity.ChannelData.ToString());
            var services = await _schedulerFactory.GetAllSchedulers();
            var scheduler = new ScheduleCreator(services[0], stepContext.Context.Activity.From.Id, storeData.OrderID, sched.ScheduleId.ToString());
            scheduler.CreateSingleGroupBuy(startTime, endTime, teamsChannelData.channel.id.ToString());
            AddConversationReference(stepContext.Context.Activity as Activity);
            
            _schedRepo.Create(sched);
            _schedRepo.context.SaveChanges();
            
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(new CreateReservationCard().CreateReserveResult(storeData.StoreName, startTime, endTime)));
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("The group buy has been reserved. I will notify you when the order start :D"));
            
            return await stepContext.EndDialogAsync();
        }

        private async Task<bool> DateTimeValidatorAsync(PromptValidatorContext<string> promptContext,
            CancellationToken cancellationToken)
        {
            JObject timeData = promptContext.Context.Activity.Value as JObject;
            JToken date;
            JToken time;

            bool b1 = timeData.TryGetValue("Date", out date);
            bool b2 = timeData.TryGetValue("Time", out time);
            if (b1 && b2)
            {
                return true;
            }
            return false;
        }
        
        private static DateTime GetEndTime(DateTime start, StoreOrderDuetime storeData)
        {
            var endHourMinute = storeData.DueTime.Split(':');
            var endTime = start.Date;
            var sp = new TimeSpan(int.Parse(endHourMinute[0]), int.Parse(endHourMinute[1]), 0);
            endTime += sp;
            return endTime;
        }
        
        private static void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }
    }
}
