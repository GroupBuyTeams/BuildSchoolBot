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
using Newtonsoft.Json;

namespace BuildSchoolBot.Service
{
    public class CreateReservationCard
    {
        //產生預約卡片
        public Attachment CreateReservationAdaptiveCard()
        {
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock() { Text = "Please select the date when you want to start group buy."})
                .AddRow(new AdaptiveColumnSet() {Separator = true}
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveDateInput() { Id = "Date", })
                        )
                        .AddCol(new AdaptiveColumn())
                        )
                .AddElement(new AdaptiveTextBlock() { Text = "Please select the time when you want to start group buy."})
                .AddRow(new AdaptiveColumnSet() {Separator = true}
                    .AddCol(new AdaptiveColumn()
                        .AddElement(new AdaptiveTimeInput() { Id = "Time"})
                    )
                    .AddCol(new AdaptiveColumn())
                )
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Confirm"})
                );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        //產生選擇訂單來源卡片
        public Attachment CreateOrderSourceAdaptiveCard()
        {
            // var card = NewAdaptiveCard()
            //     .AddActionsSet(
            //         NewActionsSet()
            //             .AddActionToSet(new AdaptiveSubmitAction() { Title = "Quick Order", Id = "Quick Order", Data = JsonConvert.SerializeObject(cardData)}
            //             .AddActionToSet(new AdaptiveSubmitAction() { Title = "Order Record", Id = "Record" })
            //             .AddActionToSet(new AdaptiveSubmitAction() { Title = "Customized", Id = "Customized" })
            //     );

            var card = NewHeroCard()
                .EditTitle("You can find a menu through:")
                .NewActionSet()
                .AddAction(new CardAction() {Type = "imBack", Title = "Quick Order", Value = "Quick Order"});
                // .AddAction(new CardAction() { Type = "imBack", Title = "Order Record", Value = "Record" })
                // .AddAction(new CardAction() { Type = "imBack", Title = "Customized", Value = "Customized" });
            return card.ToAttachment();
        }
    }
}
