// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using BuildSchoolBot.Models;
using AdaptiveCards;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Xml.Linq;
using System;
using System.IO;
using Newtonsoft.Json;
using BuildSchoolBot.Service;

namespace BuildSchoolBot.Bots
{
    public class EchoBot : TeamsActivityHandler
    {

        //ting �}��
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.Equals(turnContext.Activity.Text, "Let's buy something", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var add = MessageFactory.Text("No problem"); //�^�Ǥ�r 

                string card = "{\r\n    \"type\": \"AdaptiveCard\",\r\n    \"$schema\": \"\",\r\n    \"version\": \"1.2\",\r\n    \"body\": [\r\n        {\r\n            \"type\": \"TextBlock\",\r\n            \"text\": \"Input your Address:\"\r\n        },\r\n        {\r\n            \"type\": \"Input.Text\",\r\n            \"placeholder\": \"Address\",\r\n            \"id\": \"address\"\r\n        },\r\n        {\r\n            \"type\": \"ActionSet\",\r\n            \"actions\": [\r\n                {\r\n                    \"type\": \"Action.Submit\",\r\n                    \"title\": \"Confirm\"\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}";

                try
                {
                    var parsedResult = AdaptiveCard.FromJson(card);
                    var attachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = parsedResult.Card,
                        
                    };
                    add.Attachments.Add(attachment);
                    
                }
                catch (AdaptiveSerializationException e)
                {
                    throw;
                }

                await turnContext.SendActivityAsync(add, cancellationToken);//�����H�ǰe��r�T��
               
            }
                
            //else
            //{
            //    var replyText = $"Echo: {turnContext.Activity.Text}. Say 'wait' to watch me type.";//������r�T��
            //    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            //}

            string ob;
            if(turnContext.Activity.Value != null) {
                ob = turnContext.Activity.Value.ToString();
                //var input = Console.ReadLine();//��J�a�}�æL�X��
                string input = JsonConvert.DeserializeObject<addObj>(ob).address;
                var key = "AIzaSyAlKWP4uWjQIR3WDAWLAu6rUhBfc3_ppag";//���_
                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(input), key);
                //string.Format�N�᭱�������ন�r�괡�J�榡�r�ꤤ�����w��m
                WebRequest request = WebRequest.Create(requestUri);//�Ыؤ@�ӷs���ШD
                WebResponse response = request.GetResponse();//�h���^���ت�request�ШD,��GET API�^�Ǫ��� 
                Stream responsestream = response.GetResponseStream(); //��oresponse �̪�stream��Ƭy
                XDocument xdoc = XDocument.Load(responsestream);//�qXDocument���w����Ƭy,��bxdoc��

                XElement result = xdoc.Element("GeocodeResponse").Element("result");//
                XElement locationElement = result.Element("geometry").Element("location");

                string lat = locationElement.Element("lat").Value;
                string lng = locationElement.Element("lng").Value;

                var latlng = new WebCrawler();//�ϥΪ��Ϊ���k

                await latlng.GetStores(lat, lng);//���g�n��

                var replyText = $"Echo: {turnContext.Activity.Text}. OK";//������r�T��
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
        }
        //�K�[�����|�]�o�Ӥ�k
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";//�K�[�����|���o�y��
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }

    public class addObj
    {
        public string address { get; set; }
    }
}
