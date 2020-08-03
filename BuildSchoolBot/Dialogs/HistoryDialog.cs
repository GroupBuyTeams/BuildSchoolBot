using AdaptiveCards;
using BuildSchoolBot.Models.HistoryModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

        public HistoryDialog() : base()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                DateSelectStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> DateSelectStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Attachment(CreateAdaptiveCardUsingJson()) }, cancellationToken);

            //await stepContext.NextAsync();.Context.SendActivityAsync(MessageFactory.Attachment(CreateAdaptiveCardUsingJson()), cancellationToken);
        }

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
