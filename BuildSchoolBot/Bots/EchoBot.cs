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
using Microsoft.Bot.Builder.Dialogs;

namespace BuildSchoolBot.Bots
{
    public class EchoBot : TeamsActivityHandler
    {

        //ting 開團
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);

        }

        //添加成員會跑這個方法
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to GruopBuyBot!");//有新成員加入,會開一個訊息回傳文字
                    var paths = new[] { ".", "Resources", "IntroductionCard.json" };//New一個放在Resources裡的json檔
                    var adaptiveCard = File.ReadAllText(Path.Combine(paths));//將paths的字串合併成一個路徑,並把他讀出來,放在變數裡面
                    var attachment = new Attachment //新建一個附件
                    {
                        ContentType = AdaptiveCard.ContentType, //AdaptiveCard 的型態
                        Content = JsonConvert.DeserializeObject(adaptiveCard),//把adaptiveCard字串轉換成json檔
                    };
                    reply.Attachments.Add(attachment);//回復reply這個訊息,附加上這個卡片

                    await turnContext.SendActivityAsync(reply, cancellationToken); //機器人回傳這個訊息    
                }
            }
        }
    }
}
