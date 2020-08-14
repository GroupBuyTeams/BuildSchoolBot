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

namespace BuildSchoolBot.Dialogs
{
    public class ReservationDialog : ComponentDialog
    {
        public ReservationDialog() : base()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                CreateReservationAdaptive
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }
        //產生預約卡片
        private static async Task<DialogTurnResult> CreateReservationAdaptive(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var ReservationCardAttachment = new CreateReservationCard().CreateAdaptiveCardUsingJson();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ReservationCardAttachment));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please choose your reservation time.") }, cancellationToken);
        }
    }
}
