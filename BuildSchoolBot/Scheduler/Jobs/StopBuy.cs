using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BuildSchoolBot.Scheduler.Jobs
{
    //[DisallowConcurrentExecution]
    public class StopBuy : IJob
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;
        private readonly OrderDetailService OrderDetailServices;
        private readonly string AppId;
        private string Message;
        private string OrderId;
        public StopBuy(IConfiguration configuration, IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences, OrderDetailService orderDetailServices)
        {
            Adapter = adapter;
            ConversationReferences = conversationReferences;
            AppId = configuration["MicrosoftAppId"];
            OrderDetailServices = orderDetailServices;
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
            OrderId = context.MergedJobDataMap.GetString("OrderId");
            Message = "Stop buying!";
            var conversationReference = ConversationReferences.GetValueOrDefault(UserId);
            await ((BotAdapter)Adapter).ContinueConversationAsync(AppId, conversationReference, BotCallback, default(CancellationToken));
        }
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var members = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);
            var OrdersResult = OrderDetailServices.GetOrderResults(OrderId, members);
            await turnContext.SendActivityAsync(Message);
        }
    }
}