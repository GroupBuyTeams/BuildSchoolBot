using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildSchoolBot.Models;

namespace BuildSchoolBot.Scheduler.Jobs
{
    //[DisallowConcurrentExecution]
    public class StartBuy : IJob
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;
        private readonly OrderService orderService;
        private readonly string AppId;
        private string Message;
        public StartBuy(IConfiguration configuration, IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences, OrderService _orderService)
        {
            Adapter = adapter;
            ConversationReferences = conversationReferences;
            AppId = configuration["MicrosoftAppId"];
            orderService = _orderService;

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
            string UserId = context.MergedJobDataMap.GetString("UserId");
            var conversationReference = ConversationReferences.GetValueOrDefault(UserId);
            
            string channelId = conversationReference.ChannelId;
            string scheduleId = context.MergedJobDataMap.GetString("ScheduleId");
            // CreateOrder(scheduleId, channelId);

            Message = context.MergedJobDataMap.GetString("Information");
            await ((BotAdapter)Adapter).ContinueConversationAsync(AppId, conversationReference, BotCallback, default(CancellationToken));
        }

        private void CreateOrder(string Guid, string GroupId)
        {
            // orderService.CreateOrder(Guid, GroupId);
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(Message);
        }
    }
}