using BuildSchoolBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BuildSchoolBot.Dialogs
{
    public class AddressDialogs : ComponentDialog
    {
        private readonly IStatePropertyAccessor<Address> _addressAccessor;

        public AddressDialogs(UserState userState) : base()
        {
            _addressAccessor = userState.CreateProperty<Address>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                AddressStepAsync,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);

        }
        private static async Task<DialogTurnResult> AddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Please enter your mode of transport."),
                   Choices = ChoiceFactory.ToChoices(new List<string> { "Car", "Bus", "Bicycle" }),
               }, cancellationToken);
        }

    }
}
