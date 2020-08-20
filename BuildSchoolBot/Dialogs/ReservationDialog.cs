using AdaptiveCards;
using AngleSharp.Html.Dom;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Quartz;
using System;
using System.Collections.Generic;
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
        // private static readonly EGRepository<Schedule> _schedRepo;
        public ReservationDialog(UserState userState, AddressDialogs addressDialog) : base(nameof(ReservationDialog))
        {
            _userProfileAccessor = userState.CreateProperty<Schedule>("Schedule");
            // _schedRepo = schedRepo;
                
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
            switch (source)
            {
                case "Quick Order":
                    return await stepContext.BeginDialogAsync(nameof(AddressDialogs), "reserve");
                default: 
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Coming soon.") }, cancellationToken);
                    return await stepContext.EndDialogAsync();
 
            }
        }

        private static async Task<DialogTurnResult> StoreMenuData(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var storeData = JsonConvert.DeserializeObject<StoreOrderDuetime>(stepContext.Context.Activity.Value.ToString());
            var startTime = DateTime.Parse((string) stepContext.Values["OrderTime"]);
            var endTime = GetEndTime(startTime, storeData);
            
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
            // _schedRepo.Create(sched);
            
            
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
            endTime.AddHours(int.Parse(endHourMinute[0]));
            endTime.AddMinutes(int.Parse(endHourMinute[1]));
            return endTime;
        }
        
    }
}
