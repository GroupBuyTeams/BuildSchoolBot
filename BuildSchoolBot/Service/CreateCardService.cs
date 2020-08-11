using AdaptiveCards;
using BuildSchoolBot.Models;
using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.fooditem;
using static BuildSchoolBot.StoreModels.ModifyMenu;
using static BuildSchoolBot.StoreModels.ResultTotal;
using static BuildSchoolBot.StoreModels.SelectMenu;


namespace BuildSchoolBot.Service
{
    public class CreateCardService
    {
        public Attachment GetStore(string texta, string menuurl,string MenuId)
        {
            var Guidstr = new OrderfoodServices().GetGUID();
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(texta, AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));
            //ting
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "text", Data = new AdaptiveCardTaskFetchValue<string>() { Data = "", SetType = "test" } });

            //actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "click", Data = new AdaptiveCardTaskFetchValue<string>() { Data = texta + "FoodData2468" + menuurl } });
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Join", Data = new AdaptiveCardTaskFetchValue<string>() { Data = texta + "FoodData2468" + menuurl + "GuidStr13579" + Guidstr, SetType = "JoinMenu" } });        
            actionSet.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Favorite",
                Data = new Data()
                {
                    msteams = new Msteams()
                    {
                        type = "invoke",
                        value = new MsteamsValue()
                        {
                            Name = texta,
                            Url = menuurl,
                            Option = "Create"
                        }
                    }
                }
            });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        public Attachment GetResultClickfood(string GuidStr, string StoreName, string Orderfoodjson, string DueTime, string UserName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(GuidStr, AdaptiveTextSize.Small, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Right));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(StoreName + "訂單", AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));

            string[] itemsname = new string[] { "Food Name", "Price", "Quantity", "Remarks", "Total" };
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
            string[] TimeAndTotalMoney = new string[] { "DueTime", DueTime, "", "Total Amount:", TotalMoney.ToString() };
            var ColumnSetTimeAndMoney = new OrderfoodServices().FixedtextColumn(TimeAndTotalMoney);
            card.Body.Add(ColumnSetTimeAndMoney);
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(UserName, AdaptiveTextSize.Small, AdaptiveTextColor.Good, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Left));


            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };


        }

        public Attachment CreateClickfoodModule(string Guidstr, string StorName, string modulefoodjson)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(Guidstr, AdaptiveTextSize.Small, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Right));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(StorName, AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));


            string[] itemsname = new string[] { "Food Name", "Price", "Quantity", "Remarks" };
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(itemsname);
            var root = JsonConvert.DeserializeObject<foodgroup>(modulefoodjson);
            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle,Data = new AdaptiveCardTaskFetchValue<string>() { Data = StorName + "FoodGuid2468" + Guidstr, SetType = "JoinMenu" } })
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


        public Attachment GetResultTotal(string OrderId, string StoreName, string Orderfoodjson, string DueTime)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(OrderId, AdaptiveTextSize.Small, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Right));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(StoreName + "訂單", AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));

            string[] itemsname = new string[] { "Food Name", "Price", "Quantity", "Remarks", "Total" };
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(itemsname);
            var root = JsonConvert.DeserializeObject<AllTotalItemsGroups>(Orderfoodjson);
            card.Body.Add(ColumnSetitemname);
            decimal TotalMoney = 0;
            for (int i = 0; i < root.AllTotalItems.Count; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    int TotalQuantity = 0;
                    string TotalOrderName = "";
                    decimal TotalMoneyItem = 0;

                    for (int z = 0; z < root.AllTotalItems[i].TotalItemsGroup.Count; z++)
                    {
                        var TotalSungleMoney = new OrderfoodServices().GetTotalMoney(root.AllTotalItems[i].TotalItemsGroup[z].Quantity.ToString(), root.AllTotalItems[i].Price.ToString());
                        TotalMoneyItem = TotalMoneyItem + TotalSungleMoney;
                        var QuantityInt = root.AllTotalItems[i].TotalItemsGroup[z].Quantity;
                        TotalQuantity = TotalQuantity + QuantityInt;
                        var OrderName = root.AllTotalItems[i].TotalItemsGroup[z].UserName;
                        TotalOrderName = TotalOrderName + "," + OrderName;
                    }
                    TotalMoney = TotalMoney + TotalMoneyItem;
                    var ColumnSetitem = new AdaptiveColumnSet();
                    ColumnSetitem.Separator = true;
                    new OrderfoodServices().GetTotalResultTem(ColumnSetitem, root.AllTotalItems[i].Dish_Name, root.AllTotalItems[i].Price, TotalQuantity, TotalOrderName.TrimStart(','), root.AllTotalItems[i].Price * TotalQuantity);
                    card.Body.Add(ColumnSetitem);
                }
            }

            string[] TimeAndTotalMoney = new string[] { "DueTime", DueTime, "", "Total Amount:", TotalMoney.ToString() };
            var ColumnSetTimeAndMoney = new OrderfoodServices().FixedtextColumnLeftColor(TimeAndTotalMoney);
            card.Body.Add(ColumnSetTimeAndMoney);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

        }
        public Attachment GetError(string UserName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock("Error.Please write again", AdaptiveTextSize.Large, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Center));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(UserName, AdaptiveTextSize.Small, AdaptiveTextColor.Good, AdaptiveTextWeight.Bolder, AdaptiveHorizontalAlignment.Left));
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };

        }

        public Attachment GetCustomizedModification(string Store, List<MenuDetail> menuDetails,string MenuId)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            string[] ItemsStoreName = new string[] { Store, "" };
            card.Body.Add(new OrderfoodServices().FixedInputTextAdjustWidthColumn(ItemsStoreName));
            string[] ItemsName = new string[] { "Food Name", "Price" };
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(ItemsName);
            card.Body.Add(ColumnSetitemname);
            for (var i = 0; i < menuDetails.Count; i++)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                ColumnSetitem.Columns.Add(new OrderfoodServices().AddColumn(new OrderfoodServices().GetadaptiveText(menuDetails[i].ProductName + i.ToString(), menuDetails[i].ProductName)));
                ColumnSetitem.Columns.Add(new OrderfoodServices().AddColumn(new OrderfoodServices().GetadaptiveText(menuDetails[i].Amount + i.ToString(), decimal.Round(menuDetails[i].Amount).ToString())));
                card.Body.Add(ColumnSetitem);
            }
            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = MenuId,SetType= "CustomizedModification" } })
                    .ToList<AdaptiveAction>();
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        public Attachment GetResultCustomizedModification(string Store, List<ModifyMultiple> menuDetails)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            card.Body.Add(new OrderfoodServices().GetadaptiveTextBlock(Store));
            string[] ItemsName = new string[] { "Food Name", "Price" };
            var ColumnSetitemname = new OrderfoodServices().FixedtextColumn(ItemsName);
            card.Body.Add(ColumnSetitemname);
            for (var i = 0; i < menuDetails.Count; i++)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                ColumnSetitem.Columns.Add(new OrderfoodServices().AddColumn(new OrderfoodServices().GetadaptiveTextBlock(menuDetails[i].ProductName)));
                ColumnSetitem.Columns.Add(new OrderfoodServices().AddColumn(new OrderfoodServices().GetadaptiveTextBlock(menuDetails[i].Amount.ToString())));
                card.Body.Add(ColumnSetitem);
            }
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

    }
}
