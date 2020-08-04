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
                HandleResponseAsync
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
            message.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson() };
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
            // Do something with step.result
            // Adaptive Card submissions are objects, so you likely need to JObject.Parse(step.result)
            await stepContext.Context.SendActivityAsync($"INPUT: {stepContext.Result}");
            return await stepContext.NextAsync();
        }

        //protected async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    // Capture input from adaptive card
        //    if (string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Value != null)
        //    {
        //        // Conditionally convert based off of input ID of Adaptive Card
        //        if ((turnContext.Activity.Value as JObject)["<adaptiveCardInputId>"] != null)
        //        {
        //            turnContext.Activity.Text = (turnContext.Activity.Value as JObject)["<adaptiveCardInputId>"].ToString();
        //        }
        //    }
        //}


        //產生卡片
        private Attachment CreateAdaptiveCardUsingJson()
        {
            var paths = new[] { ".", "Resources", "DateSelectCard.json" };
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
