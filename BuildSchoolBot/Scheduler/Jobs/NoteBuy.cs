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

namespace BuildSchoolBot.Scheduler.Jobs
{
    //[DisallowConcurrentExecution]
    public class NoteBuy : IJob
    {
        private readonly ILogger<NoteBuy> _logger;
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;
        private readonly string AppId;
        private string Message;
        public NoteBuy(ILogger<NoteBuy> logger, IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _logger = logger;
            Adapter = adapter;
            ConversationReferences = conversationReferences;

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
        }
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync(Message);
        }
    }
}