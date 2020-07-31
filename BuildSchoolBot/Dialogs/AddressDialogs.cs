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

namespace BuildSchoolBot.Dialogs
{
    public class AddressDialogs : ComponentDialog
    {
        // private readonly IStatePropertyAccessor<Address> _addressAccessor;

        public AddressDialogs() : base()
        {
            // _addressAccessor = userState.CreateProperty<Address>("UserProfile");

            var waterfallSteps = new WaterfallStep[]
            {
                AddressStepAsync,
                GetStoreAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);

        }
        private static async Task<DialogTurnResult> AddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your address.") }, cancellationToken);
            
        }
        private static async Task<DialogTurnResult> GetStoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string add = (string)stepContext.Result;
            var LatLng = new LatLngService(add);
            string result = await new WebCrawler().GetStores(LatLng.lat, LatLng.lng);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(result));


            //范育銨
            var w = new OrderfoodServices();
            var Storedata = w.GetStoregroup(result);
            foreach (JObject item in Storedata)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(w.GetStore(item.GetValue("Store_Name").ToString(), item.GetValue("Store_Url").ToString())));
            }

            //var cards = new int[20];

            //foreach(var c in cards)
            //{
            //    await stepContext.PromptAsync(MessageFactory.Attachment(c));
            //}
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please choose your menu.") }, cancellationToken);
        }
    }
}
