using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Rendering;
using BuildSchoolBot.Models;
using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using static BuildSchoolBot.Service.CardAssemblyFactory;
using static BuildSchoolBot.Service.CardActionFactory;
using BuildSchoolBot.Repository;

namespace BuildSchoolBot.Service
{
    public class CreateCardService2
    {
        /// <summary>
        /// 製作收藏庫的餐廳卡片
        /// </summary>
        /// <param name="name">餐廳名稱</param>
        /// <param name="menuUrl">餐廳網址</param>
        /// <returns>收藏的餐廳卡片</returns>
        public Attachment GetStore(string text, string menuUrl)
        {

            var cardData = new CardDataModel<StoreInfoData>()
            {
                Type = "OpenMenuTaskModule",
                Value = new StoreInfoData() {Guid = Guid.NewGuid().ToString(), Name = text, Url = menuUrl}
            };
            // var MSTeamsData = new Data(text, menuUrl);

            var objData = new Data()
            {
                msteams = new Msteams()
                {
                    type = "invoke",
                    value = new MsteamsValue()
                    {
                        Name = text,
                        Url = menuUrl,
                        Option = "Create"
                    }
                }
            };
                
            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = text, Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction().SetOpenTaskModule("Join", JsonConvert.SerializeObject(cardData)))
                        .AddActionToSet(new AdaptiveSubmitAction() {Title = "Favorite", Data = objData})
                        //ting
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Delete"})
                );

            return new Attachment() {ContentType = AdaptiveCard.ContentType, Content = card};
        }

        //To dear莞婷:
        //
        //    妳不是要做刪除嗎？
        private EGRepository<Order> _repo;
        public void DeleteStore(Guid orderId)
        {
            var entity = _repo.GetAll().FirstOrDefault(x => x.OrderId.Equals(orderId));

            _repo.Delete(entity);
            _repo.context.SaveChanges();
        }
        // Sincerely,
        // 阿三

        public async Task<Attachment> CreateMenu(AdaptiveCardDataFactory dataFactory)
        {
            var storeData = dataFactory.GetCardData<StoreInfoData>();
            
            var cardData = new CardDataModel<StoreInfoData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "FetchSelectedFoods", //於EchoBot判斷用
                Value = new StoreInfoData() {Guid = storeData.Guid, Name = storeData.Name} //要傳出去的資料和資料結構
            };
            var itemsName = new string[] { "菜名", "價錢", "數量", "備註" };

            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = storeData.Guid, Size = AdaptiveTextSize.Small, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = storeData.Name, Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddRow(new AdaptiveColumnSet() 
                        .AddColumnsWithStrings(itemsName) 
                );
            
            var foods = await new WebCrawler().GetOrderInfo2(storeData.Url);

            for (int i = 0 ; i < foods.Count; i++)
            {
                card
                    .AddRow(new AdaptiveColumnSet() {Separator = true }
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() {Text = foods[i].Dish_Name}))
                        .AddCol(new AdaptiveColumn() 
                            .AddElement(new AdaptiveTextBlock() {Text = foods[i].Price.ToString()}))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveNumberInput() {Min = 0, Value = 0, Id = $"{foods[i].Dish_Name}&{foods[i].Price}"} )) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextInput() {Placeholder = "備註", Id = $"{foods[i].Dish_Name}&mark"}))
                    );
            }

            card.AddElement(new AdaptiveTextBlock()
            {
                Text = "Due Time: 123", Size = AdaptiveTextSize.Medium, Weight = AdaptiveTextWeight.Bolder,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left
            })
            .AddActionsSet(
                NewActionsSet()
                    .AddActionToSet(
                        new AdaptiveSubmitAction().SetOpenTaskModule("Order", JsonConvert.SerializeObject(cardData))//勿必要將傳出去的資料進行Serialize
                    )
            );

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        
        /// <summary>
        /// 計算某餐點的總價
        /// </summary>
        /// <param name="quantity">餐點數量</param>
        /// <param name="money">餐點單價</param>
        /// <returns>餐點總價</returns>
        public decimal GetTotalMoney(string quantity,string money)
        {
            var quantityInt = int.Parse(quantity);
            var moneyDecimal = Convert.ToDecimal(money);
            var res = quantityInt * moneyDecimal;
            return res;
        }

        public Attachment GetChosenFoodFromMenu(AdaptiveCardDataFactory dataFactory)
        {
            //顯示於TaskModule上方的欄位名稱
            var itemsName = new string[] {"食物名稱", "價錢", "數量", "備註", "單品總金額"};
            var cardData = dataFactory.GetCardData<StoreInfoData>();

            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = cardData.Guid, Size = AdaptiveTextSize.Small, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = cardData.Name + "訂單", Size = AdaptiveTextSize.Small, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(itemsName) //加入欄位名稱到一列
                );

            var orderData = dataFactory.GetOrderedFoods();

            //此訂單的總花費
            decimal totalMoney = 0;

            //將SelectMenuDatagroup的資訊(菜色名稱、單價、數量、備註、總額)，逐一附加到卡片內
            foreach (var p in orderData)
            {
                //如果沒有這道菜點餐，那就不用計算、也不用顯示
                if (p.Quantity != "0")
                {
                    //獲取此餐點的總價：數量x單價
                    var totalSingleMoney = GetTotalMoney(p.Quantity, p.Price);

                    //將此餐點的總額計入此訂單的總價
                    totalMoney += totalSingleMoney;

                    //在卡片內加入一列，在一列中加入五個欄位並猜入不同資訊
                    card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() {Text = p.Dish_Name}) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() {Text = p.Price}) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() {Text = p.Quantity}) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() {Text = p.Remarks}) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() {Text = totalSingleMoney.ToString()}) //加入此餐點的總價
                        )
                    );
                }
            }

            //顯示於TaskModule下方的文字資訊
            var timeAndTotalMoney = new string[] {"DueTime", "12:00", "", "總金額:", totalMoney.ToString()};

            //將其他資訊加入至卡片內
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(timeAndTotalMoney) //將文字資訊變為欄位並且加入至一列中
                )
                .AddElement(new AdaptiveTextBlock() //加入訂購者名稱至卡片
                {
                    Text = dataFactory.TurnContext.Activity.From.Name, Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                });
            //回傳卡片
            return new Attachment() {ContentType = AdaptiveCard.ContentType, Content = card};
        }

        //ting
        public Attachment GetCreateMenu(string guid, string name, string price)
        {
            // var storeInfo = new StoreInfoData() { Name = storeName, Guid = guid };

            var itemsName = new string[] { "Name", "Price" };
            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = guid,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = "Input your Store",
                    Size = AdaptiveTextSize.Medium,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                })
                .AddRow(new AdaptiveColumnSet()
                    .AddColumnsWithStrings(itemsName)
                );
            for (int i = 0; i < 20; i++)
            {
                card
                    .AddRow(new AdaptiveColumnSet() { Separator = true }
                        .AddCol(new AdaptiveColumn()
                        { Width = "65" }
                            .AddElement(new AdaptiveNumberInput() { Min = 0, Value = 0, Placeholder = "Name", Id = $"{name[1]}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                        { Width = "5" }
                            .AddElement(new AdaptiveTextBlock() { Text = "" }))
                         .AddCol(new AdaptiveColumn()
                         { Width = "20" }
                             .AddElement(new AdaptiveNumberInput() { Min = 0, Value = 0, Placeholder = "Price", Id = $"{price[1]}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                    );
            }
            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = "test" } })
                .ToList<AdaptiveAction>();
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }
}