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

namespace BuildSchoolBot.Dialogs
{
    public class ReservationDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Schedule> _userProfileAccessor;

        public ReservationDialog(UserState userState) : base(nameof(ReservationDialog))
        {
            _userProfileAccessor = userState.CreateProperty<Schedule>("Schedule");

            var waterfallSteps = new WaterfallStep[]
            {
                CreateReservationAdaptive,
                OrderSourceAdaptive
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }
        //請使用者選擇時間
        private static async Task<DialogTurnResult> CreateReservationAdaptive(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ReservationCardAttachment = new CreateReservationCard().CreateReservationAdaptiveCard();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ReservationCardAttachment));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please choose your reservation time.") }, cancellationToken);
        }
        //請使用者選擇訂單來源
        private static async Task<DialogTurnResult> OrderSourceAdaptive(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var OrderSourceCardAttachment = new CreateReservationCard().CreateOrderSourceAdaptiveCard();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(OrderSourceCardAttachment));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please pick an order source.") }, cancellationToken);
        }
    }
}
