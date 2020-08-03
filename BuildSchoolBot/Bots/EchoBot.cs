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
using Microsoft.Bot.Schema.Teams;
<<<<<<< HEAD
using Quartz;
using System.Collections.Concurrent;
=======
using Microsoft.Extensions.Configuration;
using System.Linq;
using BuildSchoolBot.StoreModels;
>>>>>>> dev

namespace BuildSchoolBot.Bots
{
    public class EchoBot<T> : TeamsActivityHandler where T : Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly LibraryService _libraryService;
        protected readonly ISchedulerFactory SchedulerFactory;
        protected readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;

        public EchoBot(ConversationState conversationState, UserState userState, T dialog, LibraryService libraryService, ISchedulerFactory schedulerFactory, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            _libraryService = libraryService;
            SchedulerFactory = schedulerFactory;
            ConversationReferences = conversationReferences;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // var memberId = turnContext.Activity.From.AadObjectId;
            var memberId = "EC7A25B7-6EEB-4FB5-BE96-2FA8B166EAFA";
            Guid guid;

            if (turnContext.Activity.Text == "Library")
            {
                Guid.TryParse(memberId, out guid);
                var library = await _libraryService.FindLibraryByMemberId(guid);
                var libraryCard = Service.LibraryService.CreateAdaptiveCardAttachment(library);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(libraryCard), cancellationToken);
            }
            else if (turnContext.Activity.Text == "DeletedLibrary")
            {
                dynamic obj = turnContext.Activity.Value;
                var LibraryId = obj.LibraryId;

                Guid.TryParse(LibraryId.ToString(), out guid);
                _libraryService.DeleteLibraryItem(guid);

            }
<<<<<<< HEAD
            else if (turnContext.Activity.Text == "ccc")//Demo用
            {
                var services = await SchedulerFactory.GetAllSchedulers();
                var scheduler = new ScheduleCreator(services[0], turnContext.Activity.From.Id);
                AddConversationReference(turnContext.Activity as Activity);
                scheduler.CreateSingleGroupBuyNow(DateTime.Now.AddSeconds(15.0f));
                await turnContext.SendActivityAsync(MessageFactory.Text("schedule a group buy."));
            }
=======
            //else if (turnContext.Activity.Text == "Let's buy someting")
            //{
            //    await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            //}
>>>>>>> dev
            else
            {
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            }

        }

<<<<<<< HEAD
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            ConversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }


=======
>>>>>>> dev
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to GruopBuyBot!");
                    var paths = new[] { ".", "Resources", "IntroductionCard.json" };
                    var adaptiveCard = File.ReadAllText(Path.Combine(paths));
                    var attachment = new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = JsonConvert.DeserializeObject(adaptiveCard),
                    };
                    reply.Attachments.Add(attachment);

                    await turnContext.SendActivityAsync(reply, cancellationToken);
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
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var Menumodule = new OrderfoodServices();
            return Menumodule.OnTeamsTaskModuleFetchAsync(taskModuleRequest);
        }
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var Menumodule = new OrderfoodServices();
            return await Menumodule.OnTeamsTaskModuleSubmitAsync(turnContext, taskModuleRequest, cancellationToken);
        }
    }
}
