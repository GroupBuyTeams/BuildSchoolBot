using AdaptiveCards;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
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
        protected readonly HistoryService _historyService;
        public HistoryDialog(HistoryService historyService) : base()
        {
            _historyService = historyService;

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
            message.Attachments = new List<Attachment>() { CreateAdaptiveCardUsingJson("DateSelectCard.json") };
            await stepContext.Context.SendActivityAsync(message, cancellationToken);

            var opts = new PromptOptions
            {
                Prompt = new Activity
                {
                    Type = ActivityTypes.Message,
                    Text = "waiting for user select..."
                }
            };

            return await stepContext.PromptAsync(nameof(TextPrompt), opts);
        }

        private async Task<DialogTurnResult> HandleResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var json = stepContext.Result;

            var username = stepContext.Context.Activity.From.Name;
            var start = DateTime.Parse(JObject.Parse(json.ToString())["DateFrom"].ToString());
            var end = DateTime.Parse(JObject.Parse(json.ToString())["DateTo"].ToString());

            var card = _historyService.CreateHistoryCard(start.ToString("yyyy/MM/dd"), end.ToString("yyyy/MM/dd"), username);
            // Do something with step.result
            // Adaptive Card submissions are objects, so you likely need to JObject.Parse(step.result)

            await stepContext.Context.SendActivityAsync($"INPUT: {start} {end} {username}");

            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(card),cancellationToken);
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
