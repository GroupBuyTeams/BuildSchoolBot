// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace BuildSchoolBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        public MainDialog(AddressDialogs addressDialog, HistoryDialog historyDialog) : base(nameof(MainDialog))
        {

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(addressDialog);
            AddDialog(historyDialog);
            AddDialog(new WaterfallDialog(
                nameof(WaterfallDialog),
                new WaterfallStep[] { ChooseStepAsync, MiddleStepAsync, FinalStepAscnc }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ChooseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // return await stepContext.PromptAsync(nameof(ChoicePrompt),
            //     new PromptOptions
            //     {
            //         Prompt = MessageFactory.Text("How can I serve you, darlin?."),
            //         Choices = ChoiceFactory.ToChoices(new List<string> { "I wanna buy something", "I wanna check my transaction history", "I wanna check my favorate menu" }),
            //     }, cancellationToken);
            var choices = ChoiceFactory.ToChoices(new List<string> { "Buy", "Customized", "History" });
            choices[1].Action = new Microsoft.Bot.Schema.CardAction()
            {
                Title = "Customized",
                Type = "invoke",
                Value = "{\"type\":\"task/fetch\",\"data\":\"Customized\"}"
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
            new PromptOptions
            {
                Prompt = MessageFactory.Text("How can I serve you, darlin?"),
                Choices = choices,
            }, cancellationToken);
        }
        private async Task<DialogTurnResult> MiddleStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string choise = ((FoundChoice)stepContext.Result).Value;
            // await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Your choise is {choise}"));
            switch (choise)
            {
                case "Buy":
                    return await stepContext.BeginDialogAsync(nameof(AddressDialogs));
                case "History":
                    return await stepContext.BeginDialogAsync(nameof(HistoryDialog));
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your choise is not 'Buy'."));
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