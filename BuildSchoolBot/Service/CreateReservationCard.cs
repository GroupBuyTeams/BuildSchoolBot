using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.Service.CardAssemblyFactory;
using static BuildSchoolBot.Service.CardActionFactory;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace BuildSchoolBot.Service
{
    public class CreateReservationCard
    {
        //產生預約卡片
        public Attachment CreateReservationAdaptiveCard()
        {
            var card = NewCard()
                .AddElement(new AdaptiveDateInput())
                .AddElement(new AdaptiveTimeInput())
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Confirm"})
                );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        //產生選擇訂單來源卡片
        public Attachment CreateOrderSourceAdaptiveCard()
        {
            var card = NewCard()
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Quick Order" ,Id = "Quick" })
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Order Record", Id = "Record" })
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Customized", Id = "Customized" })
                );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }
}
