using BuildSchoolBot.Models;
using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
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
            // if (string.Equals(turnContext.Activity.Text, "Let's buy something", System.StringComparison.InvariantCultureIgnoreCase))
            // {
            //     var add = MessageFactory.Text("No problem"); //�^�Ǥ�r 

            //     string card = "{\r\n    \"type\": \"AdaptiveCard\",\r\n    \"$schema\": \"\",\r\n    \"version\": \"1.2\",\r\n    \"body\": [\r\n        {\r\n            \"type\": \"TextBlock\",\r\n            \"text\": \"Input your Address:\"\r\n        },\r\n        {\r\n            \"type\": \"Input.Text\",\r\n            \"placeholder\": \"Address\",\r\n            \"id\": \"address\"\r\n        },\r\n        {\r\n            \"type\": \"ActionSet\",\r\n            \"actions\": [\r\n                {\r\n                    \"type\": \"Action.Submit\",\r\n                    \"title\": \"Confirm\"\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}";

            //     try
            //     {
            //         var parsedResult = AdaptiveCard.FromJson(card);
            //         var attachment = new Attachment
            //         {
            //             ContentType = AdaptiveCard.ContentType,
            //             Content = parsedResult.Card,
            //         };
            //         add.Attachments.Add(attachment);
            //     }
            //     catch (AdaptiveSerializationException e)
            //     {
            //         throw;
            //     }

            //     await turnContext.SendActivityAsync(add, cancellationToken);//�����H�ǰe��r�T��

            // }
            // //��a�}
        }
        private static async Task<DialogTurnResult> GetStoreAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string add = (string)stepContext.Result;
            if (add != null)
            {
                string key = "AIzaSyAlKWP4uWjQIR3WDAWLAu6rUhBfc3_ppag";
                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(add), key);
                //string.Format�N�᭱�������ন�r�괡�J�榡�r�ꤤ�����w��m
                WebRequest request = WebRequest.Create(requestUri);//�Ыؤ@�ӷs���ШD
                WebResponse response = request.GetResponse();//�h���^���ت�request�ШD,��GET API�^�Ǫ��� 
                Stream responsestream = response.GetResponseStream(); //��oresponse �̪�stream��Ƭy
                XDocument xdoc = XDocument.Load(responsestream);//�qXDocument���w����Ƭy,��bxdoc��

                XElement result = xdoc.Element("GeocodeResponse").Element("result");
                XElement locationElement = result.Element("geometry").Element("location");

                string lat = locationElement.Element("lat").Value;
                string lng = locationElement.Element("lng").Value;
                var latlng = new WebCrawler();//�ϥΪ��Ϊ���k
                string resuilt = await latlng.GetStores(lat, lng);//���g�n��
                
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("OK."));
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(resuilt));
            }else{
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("The address is invalid."));
            }

            return await stepContext.EndDialogAsync();
        }



    }
}
