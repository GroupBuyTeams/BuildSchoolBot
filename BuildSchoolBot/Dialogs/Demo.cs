using BuildSchoolBot.Models;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildSchoolBot.Dialogs
{
    public class Demo : ComponentDialog
    {
        // private readonly IStatePropertyAccessor<Address> _addressAccessor;

        public Demo() : base()
        {
            // _addressAccessor = userState.CreateProperty<Address>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                sendCardAsync,
                receiveDateAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> sendCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var card = CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "DateSelectCard.json"));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Attachment(card) as Activity }, cancellationToken);
        }

        private static async Task<DialogTurnResult> receiveDateAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int i = 0; 
            return await stepContext.EndDialogAsync();
        }

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }
    }
}