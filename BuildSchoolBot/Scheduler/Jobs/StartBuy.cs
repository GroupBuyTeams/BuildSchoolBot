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
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Connector;

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
            // get data of this schedule
            string UserId = context.MergedJobDataMap.GetString("UserId");
            string scheduleId = context.MergedJobDataMap.GetString("ScheduleId");
            string orderId = context.MergedJobDataMap.GetString("OrderId");
            string teamsChannelId = context.MergedJobDataMap.GetString("Information");

            // var conversationReference = ConversationReferences.GetValueOrDefault(UserId);
            // string channelId = conversationReference.ChannelId;
            
            var schedule = Repo.GetAll().FirstOrDefault(x => x.ScheduleId.ToString().Equals(scheduleId));
            var storeInfo = dataMapping(schedule, orderId);
            orderService.CreateOrder(storeInfo.OrderID, "Teams MS", storeInfo.StoreName);
            card = new CreateCardService2().GetStore(storeInfo);
            var response = await NewConversationAsync(teamsChannelId, card);

            var conversationReference = await UpdateConversation(response, UserId);
            ConversationReferences.AddOrUpdate(UserId, conversationReference, (key, newValue) => conversationReference);
            
            // await ((BotAdapter)Adapter).ContinueConversationAsync(AppId, conversationReference, SendAttachment, default(CancellationToken));
        }

        //吳家寶
        private async Task<ConversationResourceResponse> NewConversationAsync(string teamsChannelId, Attachment card)
        {
            //teamsChannelId: Teams channel id in which to create the post.

            //The Bot Service Url needs to be dynamically fetched (and stored) from the Team. Recommendation is to capture the serviceUrl from the bot Payload and later re-use it to send proactive messages.
            string serviceUrl = "https://smba.trafficmanager.net/emea/";
            //From the Bot Channel Registration
            
            string botClientID = "0faa4e0e-1fb9-4f0b-9710-45077dab9379";
            string botClientSecret = "00R.3O~4U~4Jh.t1qweq86Udwuq2K84.XB";
            AppCredentials.TrustServiceUrl(serviceUrl);
            var connectorClient = new ConnectorClient(new Uri(serviceUrl), new MicrosoftAppCredentials(botClientID, botClientSecret));
            var topLevelMessageActivity = MessageFactory.Attachment(card);
            
            var conversationParameters = new ConversationParameters
            {
                IsGroup = true,
                ChannelData = new TeamsChannelData
                {
                    Channel = new ChannelInfo(teamsChannelId),
                },
                Activity = (Activity)topLevelMessageActivity
            };

            return await connectorClient.Conversations.CreateConversationAsync(conversationParameters);
            // await connectorClient.Conversations.CreateConversationAsync(conversationParameters);
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

        private async Task<ConversationReference> UpdateConversation(ConversationResourceResponse response, string userId)
        {
            var oldCon = ConversationReferences.GetValueOrDefault(userId);
            oldCon.Conversation.Id = response.Id;
            oldCon.ActivityId = response.ActivityId;
            return oldCon;
        }
    }
}