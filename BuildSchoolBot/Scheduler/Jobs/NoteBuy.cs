using BuildSchoolBot.Service;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using System.Security.Claims;
using System.Threading;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Builder.Teams;
using System.Xml;
using System.Linq;

namespace BuildSchoolBot.Scheduler.Jobs
{
    //[DisallowConcurrentExecution]
    public class NoteBuy : IJob
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;
        private readonly string AppId;
        private string Message;
        public NoteBuy(IConfiguration configuration, IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            Adapter = adapter;
            ConversationReferences = conversationReferences;
            AppId = configuration["MicrosoftAppId"];
            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(AppId))
            {
                AppId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // IDialog d = new IDialog();
            string UserId = context.MergedJobDataMap.GetString("UserId");
            Message = context.MergedJobDataMap.GetString("Information");
            var conversationReference = ConversationReferences.GetValueOrDefault(UserId);
            await ((BotAdapter)Adapter).ContinueConversationAsync(AppId, conversationReference, BotCallback, default(CancellationToken));
            // return Task.CompletedTask;
        }
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {

            // await turnContext.SendActivityAsync(Message);
            var members = new List<TeamsChannelAccount>();
            try
            {
                var data = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);
                members = data.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }

            string str = string.Empty;
            str += (Message + "\r\n");

            var mentions = new List<Entity>();
            foreach (var member in members)
            {
                var mention = new Mention
                {
                    Mentioned = member,
                    Text = $"<at>{XmlConvert.EncodeName(member.Name)}</at>",
                };
                str += (mention.Text+"\r\n");
                mentions.Add(mention);
            }

            var replyActivity = MessageFactory.Text(str);
            replyActivity.Entities = mentions;
            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }
    }
}