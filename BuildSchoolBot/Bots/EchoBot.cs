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
using Microsoft.Bot.Builder.Dialogs;
using BuildSchoolBot.Models;
using AdaptiveCards;
using System.Net;
using System.Xml.Linq;
using System;
using System.IO;
using Newtonsoft.Json;
using BuildSchoolBot.Service;

namespace BuildSchoolBot.Bots
{
    public class EchoBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        public EchoBot(ConversationState conversationState, UserState userState, T dialog){
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        }


        //ting �}��
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

        }

        //�K�[�����|�]�o�Ӥ�k
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to GruopBuyBot!");//���s�����[�J,�|�}�@�ӰT���^�Ǥ�r
                    var paths = new[] { ".", "Resources", "IntroductionCard.json" };//New�@�ө�bResources�̪�json��
                    var adaptiveCard = File.ReadAllText(Path.Combine(paths));//�Npaths���r��X�֦��@�Ӹ��|,�ç�LŪ�X��,��b�ܼƸ̭�
                    var attachment = new Attachment //�s�ؤ@�Ӫ���
                    {
                        ContentType = AdaptiveCard.ContentType, //AdaptiveCard �����A
                        Content = JsonConvert.DeserializeObject(adaptiveCard),//��adaptiveCard�r���ഫ��json��
                    };
                    reply.Attachments.Add(attachment);//�^�_reply�o�ӰT��,���[�W�o�ӥd��

                    await turnContext.SendActivityAsync(reply, cancellationToken); //�����H�^�ǳo�ӰT��    
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occurred during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
