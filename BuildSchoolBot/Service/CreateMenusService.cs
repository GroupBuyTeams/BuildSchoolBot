using AdaptiveCards;
using BuildSchoolBot.StoreModels;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class CreateMenusService
    {
        public Attachment Getinput()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Create", Data = new AdaptiveCardTaskFetchValue<string>() { Data = ""} });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        public Attachment CreateMenuModule(string name, string money, string price)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock("Input your Menu",AdaptiveTextWeight.Bolder,AdaptiveTextColor.Good));
            card.Body.Add(new OrderfoodServices().GetadaptiveText(name));

            string[] ItemsName = new string[] { "Name", "Price" };      
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(ItemsName);
            card.Body.Add(ColumnSetitemname);
            for (var i = 0; i < 20; i++)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                ColumnSetitem.Separator = true;
                new OrderfoodServices().GetMenuInput(ColumnSetitem, name, money, price);
                card.Body.Add(ColumnSetitem);

            }
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

        }
    }
}
