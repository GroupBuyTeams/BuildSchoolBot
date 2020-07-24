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

        //ting 開團
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.Equals(turnContext.Activity.Text, "Let's buy something", System.StringComparison.InvariantCultureIgnoreCase))
            {
                var add = MessageFactory.Text("No problem"); //回傳文字 

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

                await turnContext.SendActivityAsync(add, cancellationToken);//機器人傳送文字訊息
               
            }
                
            //else
            //{
            //    var replyText = $"Echo: {turnContext.Activity.Text}. Say 'wait' to watch me type.";//接收文字訊息
            //    await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            //}

            string ob;
            if(turnContext.Activity.Value != null) {
                ob = turnContext.Activity.Value.ToString();
                //var input = Console.ReadLine();//輸入地址並印出來
                string input = JsonConvert.DeserializeObject<addObj>(ob).address;
                var key = "AIzaSyAlKWP4uWjQIR3WDAWLAu6rUhBfc3_ppag";//金鑰
                string requestUri = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?key={1}&address={0}&sensor=false", Uri.EscapeDataString(input), key);
                //string.Format將後面的物件轉成字串插入格式字串中的指定位置
                WebRequest request = WebRequest.Create(requestUri);//創建一個新的請求
                WebResponse response = request.GetResponse();//去拿回剛剛建的request請求,並GET API回傳的值 
                Stream responsestream = response.GetResponseStream(); //獲得response 裡的stream資料流
                XDocument xdoc = XDocument.Load(responsestream);//從XDocument指定的資料流,放在xdoc裡

                XElement result = xdoc.Element("GeocodeResponse").Element("result");//
                XElement locationElement = result.Element("geometry").Element("location");

                string lat = locationElement.Element("lat").Value;
                string lng = locationElement.Element("lng").Value;

                var latlng = new WebCrawler();//使用爬蟲的方法

                await latlng.GetStores(lat, lng);//抓到經緯度

                var replyText = $"Echo: {turnContext.Activity.Text}. OK";//接收文字訊息
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
        }
        //添加成員會跑這個方法
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";//添加成員會說這句話
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
