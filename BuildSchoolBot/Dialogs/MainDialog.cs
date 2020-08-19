// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildSchoolBot.Dialogs;
using BuildSchoolBot.Service;
using BuildSchoolBot.StoreModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace BuildSchoolBot.Dialogs

{
    public class MainDialog : ComponentDialog
    {
        public MainDialog(AddressDialogs addressDialog, HistoryDialog historyDialog, ReservationDialog reservationDialog) : base(nameof(MainDialog))
        {
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(addressDialog);
            AddDialog(historyDialog);
            AddDialog(reservationDialog);
            AddDialog(new WaterfallDialog(
                nameof(WaterfallDialog),
                new WaterfallStep[] { FirstStepAsync, ChooseStepAsync, MiddleStepAsync, FinalStepAscnc }));

            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var activity = stepContext.Context.Activity;
            if (!activity.Text.Contains("We are Hungry"))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please input \"we are Hungry\""), cancellationToken);

                var help = new HelpService();
                var command = help.Command();
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You can give command"), cancellationToken);
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(command), cancellationToken);
                if (string.IsNullOrWhiteSpace(activity.Text) && activity.Value != null)
                {
                    activity.Text = JsonConvert.SerializeObject(activity.Value);
                }

                return await stepContext.EndDialogAsync();
            }
            else
            {
                return await stepContext.NextAsync();
            }

        }
        private async Task<DialogTurnResult> ChooseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(new CreateCardService2().GetMainDialogCard()));
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions()
                {
                    Prompt = MessageFactory.Text("You can choose one of the actions above.")
                }, cancellationToken);

        }
        private async Task<DialogTurnResult> MiddleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // string choise = ((FoundChoice)stepContext.Result).Value;
            // await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your choise is {choise}"));
            var choice = (string)stepContext.Result;

            if (choice.Contains("Reserve"))
                return await stepContext.BeginDialogAsync(nameof(ReservationDialog));
            else if (choice.Contains("History"))
                return await stepContext.BeginDialogAsync(nameof(HistoryDialog));
            else if (choice.Contains("QuickBuy"))
                return await stepContext.BeginDialogAsync(nameof(AddressDialogs));
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your choice is not 'Buy'."));
                return await stepContext.NextAsync();
            }

        }
        private async Task<DialogTurnResult> FinalStepAscnc(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("This is only a test, darlin.\n Goodbye."));
            return await stepContext.EndDialogAsync();
        }
    }
}