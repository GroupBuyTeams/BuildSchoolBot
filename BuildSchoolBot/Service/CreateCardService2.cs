using System;
using AdaptiveCards;
using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using static BuildSchoolBot.Service.CardAssemblyFactory;
using static BuildSchoolBot.Service.CardActionFactory;


namespace BuildSchoolBot.Service
{
    public class CreateCardService2
    {
        /// <summary>
        /// 建立新的、且可附加於AdaptiveSubmitAction的Library Data
        /// </summary>
        /// <param name="name">餐廳名稱</param>
        /// <param name="url">餐廳網址</param>
        /// <returns>餐廳資訊</returns>
        public Msteams GetMSTeamsData(string name, string url)
        {
            return new Msteams()
            {
                type = "invoke",
                value = new MsteamsValue()
                {
                    Name = name,
                    Url = url,
                    Option = "Create"
                }
            };
        }

        /// <summary>
        /// 製作收藏庫的餐廳卡片
        /// </summary>
        /// <param name="name">餐廳名稱</param>
        /// <param name="menuUrl">餐廳網址</param>
        /// <returns>收藏的餐廳卡片</returns>
        public Attachment GetStore(string text, string menuUrl)
        {
            var textData = text + "FoodData2468" + menuUrl + "GuidStr13579" + Guid.NewGuid().ToString();
            var objData = GetMSTeamsData(text, menuUrl);

            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = text, Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction().SetOpenTaskModule("Join", textData))
                        .AddActionToSet(new AdaptiveSubmitAction() {Title = "Favorite", Data = objData})
                );

            return new Attachment() {ContentType = AdaptiveCard.ContentType, Content = card};
        }
        
        //To dear莞婷:
        //
        //    妳不是要做刪除嗎？
        //
        // Sincerely,
        // 阿三

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
        
        public Attachment GetChosenFoodFromMenu(string guid, string storeName, string orderFoodJson, string dueTime, string userName)
        {
            //顯示於TaskModule上方的欄位名稱
            var itemsName = new string[] { "食物名稱", "價錢", "數量", "備註", "單品總金額" };

            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = guid, Size = AdaptiveTextSize.Small, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = storeName + "訂單", Size = AdaptiveTextSize.Small, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                    .AddColumnsWithStrings(itemsName) //加入欄位名稱到一列
                );
            
            //處理訂單資訊Json檔，將其轉變為SelectMenuDatagroup類別
            var root = JsonConvert.DeserializeObject<SelectMenu.SelectMenuDatagroup>(orderFoodJson);
            
            //此訂單的總花費
            decimal totalMoney = 0;
            
            //將SelectMenuDatagroup的資訊(菜色名稱、單價、數量、備註、總額)，逐一附加到卡片內
            foreach (var p in root.SelectMenu)
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
            var timeAndTotalMoney = new string[] { "DueTime", dueTime, "", "總金額:", totalMoney.ToString() };

            //將其他資訊加入至卡片內
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                    .AddColumnsWithStrings(timeAndTotalMoney) //將文字資訊變為欄位並且加入至一列中
                )
                .AddElement(new AdaptiveTextBlock() //加入訂購者名稱至卡片
                {
                    Text = userName, Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                });
            
            //回傳卡片
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }
}