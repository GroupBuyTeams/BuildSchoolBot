using BuildSchoolBot.Models;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace BuildSchoolBot.Dialogs
{
    public class AddressDialogs : ComponentDialog
    {

        public bool IsReserve { get; set; }
        public AddressDialogs() : base()
        {

            var waterfallSteps = new WaterfallStep[]
            {
                AddressStepAsync,
                ConfirmAddressAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }
        private static async Task<DialogTurnResult> AddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var AddressCard = new CreateCardService().CreateAddressInputAdaptiveCard();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(AddressCard));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
        }
        //吳家寶
        private static async Task<DialogTurnResult> ConfirmAddressAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string add = (string)JsonConvert.DeserializeObject<dynamic>((string)stepContext.Result).Address;
            var getStoreService = new GetStoreList();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(getStoreService.GetChooseMenuCard(add, GetReservation(stepContext))));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Choose Menu.") }, cancellationToken);
        }

        private static bool GetReservation(WaterfallStepContext stepContext)
        {
            var dictionary = stepContext.ActiveDialog.State;
            object reserve = null;
            if (dictionary.TryGetValue("options", out reserve))
            {
                if (reserve != null && reserve.ToString().Equals("reserve"))
                {
                    dictionary["options"] = null;
                    return true;
                }
            }
            return false;
        }
    }
}
