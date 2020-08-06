﻿using AdaptiveCards;
using BuildSchoolBot.StoreModels;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.fooditem;
using static BuildSchoolBot.StoreModels.SelectMenu;


namespace BuildSchoolBot.Service
{
    public class CreateCardService
    {
        public Attachment GetStore(string texta, string menuurl)
        {
            var Guidstr = new OrderfoodServices().GetGUID();
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = texta;
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStorName);

            //actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "click", Data = new AdaptiveCardTaskFetchValue<string>() { Data = texta + "FoodData2468" + menuurl } });
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Join", Data = new AdaptiveCardTaskFetchValue<string>() { Data = texta + "FoodData2468" + menuurl + "GuidStr13579" + Guidstr } });
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Favorite", Data = new AdaptiveCardTaskFetchValue<string>() { Data = "" } });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        public Attachment GetResultClickfood(string GuidStr, string StoreName, string Orderfoodjson, string DueTime, string UserName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(GuidStr, AdaptiveTextSize.Small, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Right));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(StoreName + "訂單", AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));

            string[] itemsname = new string[] { "食物名稱", "價錢", "數量", "備註", "單品總金額" };
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(itemsname);
            var root = JsonConvert.DeserializeObject<SelectMenuDatagroup>(Orderfoodjson);
            card.Body.Add(ColumnSetitemname);
            decimal TotalMoney = 0;
            foreach (var p in root.SelectMenu)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                ColumnSetitem.Separator = true;
                if (p.Quantity != "0")
                {
                    new OrderfoodServices().GetResultClickfoodTem(ColumnSetitem, p.Dish_Name, p.Price, p.Quantity, p.Remarks);
                }
                var QuantityInt = int.Parse(p.Quantity);
                var MoneyDecimal = Convert.ToDecimal(p.Price);
                var TotalSungleMoney = QuantityInt * MoneyDecimal;
                TotalMoney = TotalMoney + TotalSungleMoney;
                card.Body.Add(ColumnSetitem);
            }
            string[] TimeAndTotalMoney = new string[] { "DueTime", DueTime, "", "總金額:", TotalMoney.ToString() };
            var ColumnSetTimeAndMoney = new OrderfoodServices().FixedtextColumn(TimeAndTotalMoney);
            card.Body.Add(ColumnSetTimeAndMoney);
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(UserName, AdaptiveTextSize.Small, AdaptiveTextColor.Good, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Left));


            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };


        }

        public Attachment CreateClickfoodModule(string Guidstr, string StorName, string modulefoodjson)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(Guidstr, AdaptiveTextSize.Small, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Right));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(Guidstr, AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));


            string[] itemsname = new string[] { "菜名", "價錢", "數量", "備註" };
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(itemsname);
            var root = JsonConvert.DeserializeObject<foodgroup>(modulefoodjson);
            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = StorName + "FoodGuid2468" + Guidstr } })
                    .ToList<AdaptiveAction>();
            card.Body.Add(ColumnSetitemname);
            foreach (var p in root.Menuproperties)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                ColumnSetitem.Separator = true;
                new OrderfoodServices().MenuModule(ColumnSetitem, p.Dish_Name, p.Price, p.Dish_Name);
                card.Body.Add(ColumnSetitem);
            }
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock("Due Time:  123", AdaptiveTextSize.Medium, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Left));
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }
}