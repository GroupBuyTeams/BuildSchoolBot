using AdaptiveCards;
using BuildSchoolBot.Models.HistoryModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace BuildSchoolBot.Dialogs
{
    public class HistoryDialog : ComponentDialog
    {
        protected readonly Dialog Dialog;

        public HistoryDialog() : base()
        {

            var waterfallSteps = new WaterfallStep[]
            {
                DateSelectStepAsync,
                HandleResponseAsync,
                ShowhistorytStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DateSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text("");
            message.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson("DateSelectCard.json") };
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "waiting for user input..."
                }
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> HandleResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var start = stepContext.Result;
            // Do something with step.result
            // Adaptive Card submissions are objects, so you likely need to JObject.Parse(step.result)
            await stepContext.Context.SendActivityAsync($"INPUT: {start}");
            return await stepContext.NextAsync();
        }

        private async Task<DialogTurnResult> ShowhistorytStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = MessageFactory.Text("");
            message.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson("HistoryCard.json") };
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            return await stepContext.EndDialogAsync();
        }


        //產生卡片
        private Attachment CreateAdaptiveCardUsingJson(string json)
        {
            var paths = new[] { ".", "Resources", json };
            var HistoryCardJson = File.ReadAllText(Path.Combine(paths));

            var HistoryCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(HistoryCardJson),
            };
            return HistoryCardAttachment;
        }
    }
}
