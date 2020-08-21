using BuildSchoolBot.Service;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
        private readonly IBotFrameworkHttpAdapter _Adapter;
        private readonly ConcurrentDictionary<string, ConversationReference> _ConversationReferences;
        private readonly OrderService _OrderService;
        private readonly OrderDetailService _OrderDetailServices;
        private readonly CreateCardService _CreateCardService;
        private readonly string _AppId;
        private string _Message;
        private string _OrderId;
        public StopBuy(IConfiguration configuration, IBotFrameworkHttpAdapter adapter, ConcurrentDictionary<string, ConversationReference> conversationReferences, OrderService orderService, OrderDetailService orderDetailServices, CreateCardService createCardService)
        {
            _Adapter = adapter;
            _ConversationReferences = conversationReferences;
            _AppId = configuration["MicrosoftAppId"];
            _OrderService = orderService;
            _OrderDetailServices = orderDetailServices;
            _CreateCardService = createCardService;
            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_AppId))
            {
                _AppId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string UserId = context.MergedJobDataMap.GetString("UserId");
            _OrderId = context.MergedJobDataMap.GetString("OrderId");
            _Message = "Stop buying!";
            var conversationReference = _ConversationReferences.GetValueOrDefault(UserId);
            await ((BotAdapter)_Adapter).ContinueConversationAsync(_AppId, conversationReference, BotCallback, default(CancellationToken));
        }
        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var members = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);
            var ordersResultJson = JsonConvert.SerializeObject(_OrderDetailServices.GetOrderResults(_OrderId, members));
            var Order = _OrderService.GetOrder(_OrderId);
            var attachment = new CreateCardService2().GetResultTotal(_OrderId, Order.StoreName, ordersResultJson, DateTime.Now.ToString("HH:mm"));
            await turnContext.SendActivityAsync(MessageFactory.Attachment(attachment));
            // await turnContext.SendActivityAsync(_Message);
        }
    }
}