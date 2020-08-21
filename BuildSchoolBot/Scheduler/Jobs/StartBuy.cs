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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using Microsoft.Bot.Schema.Teams;

namespace BuildSchoolBot.Scheduler.Jobs
{
    //[DisallowConcurrentExecution]
    public class StartBuy : IJob
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;
        private readonly EGRepository<Schedule> Repo;
        private readonly OrderService orderService;
        private readonly string AppId;
        private string Message;
        private Attachment card;
        public StartBuy(IConfiguration configuration, IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences, OrderService _orderService, EGRepository<Schedule> _repo)
        {
            Adapter = adapter;
            ConversationReferences = conversationReferences;
            AppId = configuration["MicrosoftAppId"];
            orderService = _orderService;
            Repo = _repo;

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

            var schedule = Repo.GetAll().FirstOrDefault(x => x.ScheduleId.ToString().Equals(scheduleId));
            var storeInfo = dataMapping(schedule, Guid.NewGuid().ToString());
            orderService.CreateOrder(storeInfo.OrderID, conversationReference.ChannelId, storeInfo.StoreName);
            card = new CreateCardService2().GetStore(storeInfo);
            
            await ((BotAdapter)Adapter).ContinueConversationAsync(AppId, conversationReference, SendAttachment, default(CancellationToken));
        }

        private async Task SendAttachment(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(MessageFactory.Attachment(card));
        }

        private StoreOrderDuetime dataMapping(Schedule sched, string orderId)
        {
            return new StoreOrderDuetime()
            {
                Url = sched.MenuUri,
                DueTime = sched.EndTime.ToString("HH:mm"),
                OrderID = orderId,
                StoreName = "StoreName" // 待修正
            };
        }
    }
}