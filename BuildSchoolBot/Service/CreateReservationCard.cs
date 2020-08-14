using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.Service.CardAssemblyFactory;
using static BuildSchoolBot.Service.CardActionFactory;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using BuildSchoolBot.ViewModels;

namespace BuildSchoolBot.Service
{
    public class CreateReservationCard
    {
        //產生預約卡片
        public Attachment CreateAdaptiveCardUsingJson()
        {
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveDateInput())
                .AddElement(new AdaptiveTimeInput())
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction())
                );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }
}
