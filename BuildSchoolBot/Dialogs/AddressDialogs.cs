using System.Threading;
using System.Threading.Tasks;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;

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
                ConfirmAddressAsync
                //GetStoreAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);

        }
        private static async Task<DialogTurnResult> AddressStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your address.") }, cancellationToken);

        }
        //吳家寶
        private static async Task<DialogTurnResult> ConfirmAddressAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string add = (string)stepContext.Result;
            var LatLng = new LatLngService(add);
            var result = await new WebCrawler().GetStores(LatLng.lat, LatLng.lng);
            var get_store = new GetStoreList();
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(get_store.GetStore(add, result)));
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please Confirm Your Address.") }, cancellationToken);
        }
        private static async Task<DialogTurnResult> GetStoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string add = (string)stepContext.Result;
            var LatLng = new LatLngService(add);
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(result)); //顯示菜單字串
            //范育銨
            // var result = await new WebCrawler().GetStores(LatLng.lat, LatLng.lng);
            // var w = new CreateCardService();
            // var o = new OrderfoodServices();
            // var Storedata = o.GetStoregroup(result);
            // foreach (JObject item in Storedata)
            // {
            //     await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(w.GetStore(item.GetValue("Store_Name").ToString(), item.GetValue("Store_Url").ToString())));
            // }

            // by 阿三
            var result = await new WebCrawler().GetStores2(LatLng.lat, LatLng.lng);
            var service = new CreateCardService2();
            foreach (var store in result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(service.GetStore(store.Store_Name, store.Store_Url)));
            }
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please choose your menu.") }, cancellationToken);
        }
    }
}
