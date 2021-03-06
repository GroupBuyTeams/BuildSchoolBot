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
using Microsoft.Bot.Schema.Teams;
using static BuildSchoolBot.StoreModels.AllSelectData;
using static BuildSchoolBot.StoreModels.ModifyMenu;
using static BuildSchoolBot.StoreModels.ResultTotal;
using Microsoft.Bot.Builder;

namespace BuildSchoolBot.Service
{
    public class CreateCardService2
    {
        public Attachment GetMainDialogCard()
        {
            var card = NewHeroCard();
            card.EditTitle("How can I serve you darling?")
                .NewActionSet()
                .AddAction(new CardAction() { Type = "imBack", Title = "Buy", Value = "QuickBuy" })
                .AddAction(new CardAction() { Type = "invoke", Title = "Customized", Value = "{\"type\":\"task/fetch\",\"data\":{\"Type\":\"Customized\",\"Value\":\"\"}}" })
                .AddAction(new CardAction() { Type = "imBack", Title = "History", Value = "History" })
                .AddAction(new CardAction() { Type = "imBack", Title = "Reserve", Value = "Reserve" });
            return card.ToAttachment();
        }

        /// <summary>
        /// 製作收藏庫的餐廳卡片
        /// </summary>
        /// <param name="name">餐廳名稱</param>
        /// <param name="menuUrl">餐廳網址</param>
        /// <returns>可收藏的團購卡片</returns>
        public Attachment GetStore(StoreOrderDuetime OrderInfo)
        {
            var cardData = new CardDataModel<StoreOrderDuetime>()
            {
                Type = "OpenMenuTaskModule",
                Value = OrderInfo
            };
            var objData = new Data()
            {
                msteams = new Msteams()
                {
                    type = "invoke",
                    value = new MsteamsValue()
                    {
                        Name = OrderInfo.StoreName,
                        Url = OrderInfo.Url,
                        Option = "Create"
                    }
                }
            };

            //var DeleteOrderData = new Data()
            //{
            //    msteams = new Msteams()
            //    {
            //        type = "invoke",
            //        value = new MsteamsValue()
            //        {
            //            OrderId = Guid.Parse(OrderInfo.OrderID),
            //            Option = "DeleteOrder"
            //        }
            //    }
            //};

            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = OrderInfo.StoreName,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction().SetOpenTaskModule("Join", JsonConvert.SerializeObject(cardData)))
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Favorite", Data = objData })
                //ting
                //.AddActionToSet(new AdaptiveSubmitAction() { Title = "Delete", Data = DeleteOrderData })
                );

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        public async Task<Attachment> CreateMenu(AdaptiveCardDataFactory dataFactory)
        {
            var storeData = dataFactory.GetCardData<StoreOrderDuetime>();

            var cardData = new CardDataModel<StoreOrderDuetime>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "FetchSelectedFoods", //於EchoBot判斷用
                Value = storeData //要傳出去的資料和資料結構
            };
            var itemsName = new string[] { "菜名", "價錢", "數量", "備註" };

            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = storeData.OrderID,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = storeData.StoreName,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddRow(new AdaptiveColumnSet()
                        .AddColumnsWithStrings(itemsName)
                );

            var foods = await new WebCrawler().GetOrderInfo2(storeData.Url);

            for (int i = 0; i < foods.Count; i++)
            {
                card
                    .AddRow(new AdaptiveColumnSet() { Separator = true }
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() { Text = foods[i].Dish_Name }))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(foods[i].Price).ToString() }))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveNumberInput() { Min = 0, Value = 0, Id = $"{foods[i].Dish_Name}&&{foods[i].Price}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextInput() { Placeholder = "備註", Id = $"{foods[i].Dish_Name}&&mark" }))
                    );
            }

            card.AddElement(new AdaptiveTextBlock()
            {
                Text = $"Due Time: {storeData.DueTime}",
                Size = AdaptiveTextSize.Medium,
                Weight = AdaptiveTextWeight.Bolder,
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
        public decimal GetTotalMoney(string quantity, string money)
        {
            var quantityInt = int.Parse(quantity);
            var moneyDecimal = Convert.ToDecimal(money);
            var res = quantityInt * moneyDecimal;
            return res;
        }
        public Attachment GetChosenFoodFromMenu(AdaptiveCardDataFactory dataFactory)
        {
            var orderData = dataFactory.GetOrderedFoods(); //使用者的訂購資訊
            if (orderData == null)//防呆：使用者在數量那邊輸入負值
            {
                return GetError("The numbers of products in the order cannot be negative.");
            }
            else if (orderData.Count == 0)//防呆：使用者沒有點任何東西就submit
            {
                return GetError("You order nothing.");
            }
            var itemsName = new string[] { "食物名稱", "價錢", "數量", "備註", "單品總金額" }; //顯示於TaskModule上方的欄位名稱

            var cardData = dataFactory.GetCardData<StoreOrderDuetime>();
            var ChosencardData = new GetChosenData();
            var GetAllChosenDataGroups = new List<GetChosenDataGroups>();
            ChosencardData.UserID = dataFactory.TurnContext.Activity.From.Id;
            ChosencardData.DueTime = cardData.DueTime;
            ChosencardData.StoreName = cardData.StoreName;
            ChosencardData.UserName = dataFactory.TurnContext.Activity.From.Name;
            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = cardData.OrderID,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = cardData.StoreName + "訂單",
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(itemsName) //加入欄位名稱到一列
                );

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
                                .AddElement(new AdaptiveTextBlock() { Text = p.Dish_Name }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(decimal.Parse(p.Price)).ToString() }) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = p.Quantity }) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = p.Remarks }) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(totalSingleMoney).ToString() }) //加入此餐點的總價
                        )
                    );
                    GetAllChosenDataGroups.Add(new GetChosenDataGroups() { ProductName = p.Dish_Name, Amount = decimal.Parse(p.Price), Number = int.Parse(p.Quantity), Mark = p.Remarks, TotalItemMoney= totalSingleMoney });
                }
            }
            ChosencardData.GetAllChosenDatas = GetAllChosenDataGroups;
            var GetChosencardData = new CardDataModel<GetChosenData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "GetChosenFoodFromMenuData", //於EchoBot判斷用
                Value = ChosencardData //要傳出去的資料和資料結構
            };
            //顯示於TaskModule下方的文字資訊
            var timeAndTotalMoney = new string[] { "DueTime", cardData.DueTime, "", "總金額:", decimal.Round(totalMoney).ToString() };

            //將其他資訊加入至卡片內
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(timeAndTotalMoney) //將文字資訊變為欄位並且加入至一列中
                )
                .AddElement(new AdaptiveTextBlock() //加入訂購者名稱至卡片
                {
                    Text = dataFactory.TurnContext.Activity.From.Name,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddActionsSet(
                NewActionsSet()
                    .AddActionToSet(
                        new AdaptiveSubmitAction().SetOpenTaskModule("View", JsonConvert.SerializeObject(GetChosencardData))//勿必要將傳出去的資料進行Serialize
                    )
            );
            //回傳卡片
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card, Name = "SingleOrderResult" };
        }
        //ting
        public Attachment GetCreateMenu(string activityId)
        {
            // var storeInfo = new StoreInfoData() { Name = storeName, Guid = guid };
            var guid = Guid.NewGuid().ToString();

            var cardData = new CardDataModel<StoreInfoData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "GetCustomizedMenu", //於EchoBot判斷用
                Value = new StoreInfoData() { Guid = guid, Name = activityId } //要傳出去的資料和資料結構
            };

            var itemsName = new string[] { "Name", "Price" };
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = guid,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })

                .AddElement(new AdaptiveTextBlock()
                {
                    Text = "Enter your Store",
                    Size = AdaptiveTextSize.Medium,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                })
                .AddElement(new AdaptiveTextInput()
                {
                    Placeholder = "Store",
                    Id = $"store"
                }); //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
            card
              .AddRow(new AdaptiveColumnSet() { Separator = true }
                    .AddCol(new AdaptiveColumn()
                    { Width = "80" }
                        .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "Name", Color = (AdaptiveTextColor)5 })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                    .AddCol(new AdaptiveColumn()
                    { Width = "20" }
                    .AddElement(new AdaptiveTextBlock() { Text = "Price", Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Color = (AdaptiveTextColor)5 }))
                );
            for (int i = 0; i < 20; i++)
            {
                card
                    .AddRow(new AdaptiveColumnSet() { Separator = true }
                        .AddCol(new AdaptiveColumn()
                        { Width = "65" }
                            .AddElement(new AdaptiveTextInput() { Placeholder = "Name", Id = $"name&{i}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                        { Width = "3" }
                        .AddElement(new AdaptiveTextBlock() { Text = "$", Size = (AdaptiveTextSize)3, HorizontalAlignment = (AdaptiveHorizontalAlignment)2 }))

                        .AddCol(new AdaptiveColumn()
                        { Width = "20" }
                             .AddElement(new AdaptiveNumberInput() { Placeholder = "Price", Id = $"price&{i}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                         );
            }
            card
             .AddActionsSet(
                NewActionsSet()
                    .AddActionToSet(
                        new AdaptiveSubmitAction().SetSubmitTaskModule("Create", JsonConvert.SerializeObject(cardData))//勿必要將傳出去的資料進行Serialize
                    )
            );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        //ting create menu detail
        public Attachment GetCreateMenuDetail(string storeId)
        {
            var guid = storeId;

            var cardData = new CardDataModel<StoreInfoData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "GetCustomizedMenuDetail", //於EchoBot判斷用
                Value = new StoreInfoData() { Guid = guid } //要傳出去的資料和資料結構
            };
            var itemsName = new string[] { "Name", "Price" };
            var card = NewAdaptiveCard()
              .AddRow(new AdaptiveColumnSet() { Separator = true }
                    .AddCol(new AdaptiveColumn()
                    { Width = "80" }
                        .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "Name", Color = (AdaptiveTextColor)5 })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                    .AddCol(new AdaptiveColumn()
                    { Width = "20" }
                    .AddElement(new AdaptiveTextBlock() { Text = "Price", Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Color = (AdaptiveTextColor)5 }))
                );
            for (int i = 0; i < 20; i++)
            {
                card
                    .AddRow(new AdaptiveColumnSet() { Separator = true }
                        .AddCol(new AdaptiveColumn()
                        { Width = "65" }
                            .AddElement(new AdaptiveTextInput() { Placeholder = "Name", Id = $"name&{i}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                        { Width = "3" }
                        .AddElement(new AdaptiveTextBlock() { Text = "$", Size = (AdaptiveTextSize)3, HorizontalAlignment = (AdaptiveHorizontalAlignment)2 }))

                        .AddCol(new AdaptiveColumn()
                        { Width = "20" }
                             .AddElement(new AdaptiveNumberInput() { Placeholder = "Price", Id = $"price&{i}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                         );
            }
            card
             .AddActionsSet(
                NewActionsSet()
                    .AddActionToSet(
                        new AdaptiveSubmitAction().SetSubmitTaskModule("Create", JsonConvert.SerializeObject(cardData))//勿必要將傳出去的資料進行Serialize
                    )
            );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        //ting paymenturl
        public Attachment ReplyPayment(PayMentService payment, ITurnContext turnContext)
        {
            var name = turnContext.Activity.From.Name;
            var memberId = turnContext.Activity.From.Id;
            var url = payment.GetPay(memberId).Url;

            var cardData = new CardDataModel<StoreInfoData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "ReplyPayment", //於EchoBot判斷用
                Value = new StoreInfoData() { } //要傳出去的資料和資料結構
            };
            var card = NewAdaptiveCard()
               .AddElement(new AdaptiveTextBlock()
               {
                   Text = "Payment",
                   Size = AdaptiveTextSize.Large,
                   Color = AdaptiveTextColor.Good,
                   Weight = AdaptiveTextWeight.Bolder,
                   HorizontalAlignment = AdaptiveHorizontalAlignment.Left
               })
               .AddElement(new AdaptiveTextBlock()
               {
                   Text = url,
                   Size = AdaptiveTextSize.Medium,
               })
               .AddElement(new AdaptiveTextBlock()
               {
                   Text = name,
                   Size = AdaptiveTextSize.Small,
                   Color = AdaptiveTextColor.Dark,
                   HorizontalAlignment = AdaptiveHorizontalAlignment.Left
               });
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        public void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }
        public void GetChosenFoodFromMenuCreateOrderDetail(AdaptiveCardDataFactory dataFactory, string UserId)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var cardData = dataFactory.GetCardData<StoreOrderDuetime>();
            var OrderData = dataFactory.GetOrderedFoods();
            var SelectObject = new SelectAllDataGroup();
            SelectObject.UserID = UserId;
            var ChosenFoodFromMenu = new List<SelectData>();
            foreach (var p in OrderData)
            {
                if (p.Quantity != "0")
                {
                    ChosenFoodFromMenu.Add(new SelectData() { Quantity = p.Quantity, Remarks = p.Remarks, Dish_Name = p.Dish_Name, Price = p.Price });
                }
            }
            new OrderService(context).CreateOrder(cardData.OrderID, dataFactory.TurnContext.Activity.ChannelId, cardData.StoreName);
            new OrderDetailService(context).CreateOrderDetail(SelectObject, ChosenFoodFromMenu, Guid.Parse(cardData.OrderID));
        }
        public Attachment GetResultTotal(string OrderId, string StoreName, string Orderfoodjson, string DueTime)
        {
            string[] ItemsName = new string[] { "Food Name", "Price", "Quantity", "Remarks", "Total" };

            var ResultTotalData = new ResultTotalModule();
            var ResultTotalItems = new List<ResultTotalItemsGroupModule>();
            ResultTotalData.OrderId = OrderId;
            ResultTotalData.StoreName = StoreName;
            ResultTotalData.DueTime = DueTime;
            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = OrderId,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = StoreName + "訂單",
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(ItemsName) //加入欄位名稱到一列
                );

            var root = JsonConvert.DeserializeObject<AllTotalItemsGroups>(Orderfoodjson);

            //此訂單的總花費
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
                        var TotalSungleMoney = GetTotalMoney(root.AllTotalItems[i].TotalItemsGroup[z].Quantity.ToString(), root.AllTotalItems[i].Price.ToString());
                        TotalMoneyItem = TotalMoneyItem + TotalSungleMoney;
                        var QuantityInt = root.AllTotalItems[i].TotalItemsGroup[z].Quantity;
                        TotalQuantity = TotalQuantity + QuantityInt;
                        var OrderName = root.AllTotalItems[i].TotalItemsGroup[z].UserName;
                        TotalOrderName = TotalOrderName + "," + OrderName;
                    }
                    TotalMoney = TotalMoney + TotalMoneyItem;
                    var TotalItemMoney = root.AllTotalItems[i].Price * TotalQuantity;
                    card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = root.AllTotalItems[i].Dish_Name }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(root.AllTotalItems[i].Price).ToString() }) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = TotalQuantity.ToString() }) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = TotalOrderName.TrimStart(',') }) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(TotalItemMoney).ToString() }) //加入此餐點的總價
                        )

                    );
                    ResultTotalItems.Add(new ResultTotalItemsGroupModule() {Dish_Name= root.AllTotalItems[i].Dish_Name,Price= root.AllTotalItems[i].Price, TotalQuantity = TotalQuantity, TotalOrderName= TotalOrderName.TrimStart(','), TotalItemMoney= TotalItemMoney });
                }
            }
            ResultTotalData.AllTotalItems = ResultTotalItems;
            ResultTotalData.TotalMoney = TotalMoney;
            var ResultTotalCardData = new CardDataModel<ResultTotalModule>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "GetResultTotalFromModule", //於EchoBot判斷用
                Value = ResultTotalData //要傳出去的資料和資料結構
            };



            string[] TimeAndTotalMoney = new string[] { "DueTime", DueTime, "", "Total Amount:", decimal.Round(TotalMoney).ToString() };
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                     .FixedtextColumnLeftColor(TimeAndTotalMoney) //加入欄位名稱到一列
             )
            .AddActionsSet(
                NewActionsSet()
                    .AddActionToSet(
                        new AdaptiveSubmitAction().SetOpenTaskModule("View", JsonConvert.SerializeObject(ResultTotalCardData))//勿必要將傳出去的資料進行Serialize
                    ));
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        public Attachment GetError(string errorMessage)
        {
            var card = NewAdaptiveCard()
                  .AddElement(new AdaptiveTextBlock()
                  {
                      Text = "Oops!",
                      Size = AdaptiveTextSize.Large,
                      Weight = AdaptiveTextWeight.Bolder,
                      HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                  })
                  .AddElement(new AdaptiveTextBlock()
                  {
                      Text = "Something wrong with your action:",
                      Size = AdaptiveTextSize.Medium,
                      Color = AdaptiveTextColor.Default,
                      Weight = AdaptiveTextWeight.Bolder,
                      HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                  })
                   .AddElement(new AdaptiveTextBlock()
                   {
                       Text = errorMessage,
                       Size = AdaptiveTextSize.Medium,
                       Color = AdaptiveTextColor.Warning,
                       Weight = AdaptiveTextWeight.Bolder,
                       HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                   });
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card, Name = "error" };
        }
        public Attachment GetCustomizedModification(AdaptiveCardDataFactory dataFactory)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var MenuId = dataFactory.GetCardData<ModifyData>().MenuId;
            var menuDetails = new MenuDetailService(context).GetMenuOrder(MenuId).ToList();
            var MenuOrderStore = new MenuService(context).GetMenuOrder(MenuId).Store;
            var ModifyData = new CardDataModel<ModifyData>()
            {
                Type = "CustomizedModification",
                Value = new ModifyData()
                {
                    MenuId = MenuId
                }
            };
            string[] ItemsName = new string[] { "Food Name", "Price" };
            var card =
                NewAdaptiveCard()
                    .AddRow(new AdaptiveColumnSet() { Separator = true }
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextInput() { Id = MenuOrderStore + "1", Value = MenuOrderStore }))
                      .AddCol(new AdaptiveColumn()
                                .AddElement(new AdaptiveTextBlock() { Text = "" })
                        ))
                  .AddRow(new AdaptiveColumnSet().
                        AddColumnsWithStrings(ItemsName)
                );

            for (var i = 0; i < menuDetails.Count; i++)
            {
                card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextInput() { Id = menuDetails[i].ProductName + i.ToString(), Value = menuDetails[i].ProductName }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextInput() { Id = menuDetails[i].Amount + i.ToString(), Value = decimal.Round(menuDetails[i].Amount).ToString() }) //加入餐點價格
                        )
                    );
            }
            card
            .AddActionsSet(
               NewActionsSet()
                   .AddActionToSet(
                       new AdaptiveSubmitAction().SetOpenTaskModule("Edit", JsonConvert.SerializeObject(ModifyData))//勿必要將傳出去的資料進行Serialize
                   )
           );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        public Attachment GetResultCustomizedModification(AdaptiveCardDataFactory dataFactory)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var MenuId = dataFactory.GetCardData<ModifyData>().MenuId;
            dataFactory.ModifyMenuData(MenuId);
            var menuDetails = new MenuDetailService(context).GetMenuOrder(MenuId).ToList();
            var MenuOrderStore = new MenuService(context).GetMenuOrder(MenuId).Store;
            string[] ItemsName = new string[] { "Food Name", "Price" };
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = MenuOrderStore
                })
                  .AddRow(new AdaptiveColumnSet().
                        AddColumnsWithStrings(ItemsName)
                );

            for (var i = 0; i < menuDetails.Count; i++)
            {
                card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = menuDetails[i].ProductName }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(menuDetails[i].Amount).ToString() }) //加入餐點價格
                        )
                    );
            }
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        public Attachment GetChosenFoodFromMenuModule(AdaptiveCardDataFactory dataFactory)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var ChosencardData = dataFactory.GetCardData<GetChosenData>();
            var itemsName = new string[] { "食物名稱", "價錢", "數量", "備註", "單品總金額" }; //顯示於TaskModule上方的欄位名稱
            //var cardData = dataFactory.GetCardData<StoreOrderDuetime>();

            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = ChosencardData.OrderID,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = ChosencardData.StoreName + "訂單",
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                });
            //.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
            //        .AddColumnsWithStrings(itemsName) //加入欄位名稱到一列
            //);
            card
        .AddRow(new AdaptiveColumnSet() { Separator = true }
              .AddCol(new AdaptiveColumn()
              { Width = "40" }
                  .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "食物名稱", Color = (AdaptiveTextColor)5 }))
               .AddCol(new AdaptiveColumn()
               { Width = "10" }
                  .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "價錢", Color = (AdaptiveTextColor)5 }))
                .AddCol(new AdaptiveColumn()
                { Width = "10" }
                  .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "數量", Color = (AdaptiveTextColor)5 }))
                 .AddCol(new AdaptiveColumn()
                 { Width = "20" }
                  .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "備註", Color = (AdaptiveTextColor)5 }))
                  .AddCol(new AdaptiveColumn()
                  { Width = "20" }
                  .AddElement(new AdaptiveTextBlock() { Size = (AdaptiveTextSize)2, Weight = (AdaptiveTextWeight)2, Text = "單品總金額", Color = (AdaptiveTextColor)5 }))
          );
            //此訂單的總花費
            decimal totalMoney = 0;

            //將SelectMenuDatagroup的資訊(菜色名稱、單價、數量、備註、總額)，逐一附加到卡片內
            foreach (var p in ChosencardData.GetAllChosenDatas)
            {
                //如果沒有這道菜點餐，那就不用計算、也不用顯示
                if (p.Number.ToString() != "0")
                {
                    //獲取此餐點的總價：數量x單價
                    var totalSingleMoney = GetTotalMoney(p.Number.ToString(), p.Amount.ToString());

                    //將此餐點的總額計入此訂單的總價
                    totalMoney += totalSingleMoney;

                    //在卡片內加入一列，在一列中加入五個欄位並猜入不同資訊
                    card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                        { Width = "40" }
                                .AddElement(new AdaptiveTextBlock() { Text = p.ProductName }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                        { Width = "10" }
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(p.Amount).ToString() }) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                        { Width = "10" }
                                .AddElement(new AdaptiveTextBlock() { Text = p.Number.ToString() }) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                        { Width = "20" }

                                .AddElement(new AdaptiveTextBlock() { Text = p.Mark }) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                        { Width = "20" }
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(p.TotalItemMoney).ToString() }) //加入此餐點的總價
                        )
                    );
                }
            }

            //顯示於TaskModule下方的文字資訊
            var timeAndTotalMoney = new string[] { "DueTime", ChosencardData.DueTime, "", "總金額:", decimal.Round(totalMoney).ToString() };

            //將其他資訊加入至卡片內
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(timeAndTotalMoney) //將文字資訊變為欄位並且加入至一列中
                )
                .AddElement(new AdaptiveTextBlock() //加入訂購者名稱至卡片
                {
                    Text = ChosencardData.UserName,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                });
            //回傳卡片
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card, Name = "SingleOrderResult" };
        }


        public Attachment GetResultTotalFromMenuModule(AdaptiveCardDataFactory dataFactory)
        {
            string[] ItemsName = new string[] { "Food Name", "Price", "Quantity", "Remarks", "Total" };
            var ChosencardData = dataFactory.GetCardData<ResultTotalModule>();          
            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = ChosencardData.OrderId,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = ChosencardData.StoreName + "訂單",
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddColumnsWithStrings(ItemsName) //加入欄位名稱到一列
                );

            //var root = JsonConvert.DeserializeObject<AllTotalItemsGroups>(Orderfoodjson);

            //此訂單的總花費
            decimal TotalMoney = 0;
            for (int i = 0; i < ChosencardData.AllTotalItems.Count; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    int TotalQuantity = 0;
                    decimal TotalMoneyItem = 0;
              
                    TotalMoney = TotalMoney + TotalMoneyItem;
                    var TotalItemMoney = ChosencardData.AllTotalItems[i].Price * TotalQuantity;
                    card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = ChosencardData.AllTotalItems[i].Dish_Name }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(ChosencardData.AllTotalItems[i].Price).ToString() }) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = ChosencardData.AllTotalItems[i].TotalQuantity.ToString() }) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = ChosencardData.AllTotalItems[i].TotalOrderName}) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = decimal.Round(ChosencardData.AllTotalItems[i].TotalItemMoney).ToString() }) //加入此餐點的總價
                        )
                    );
                }
            }
            string[] TimeAndTotalMoney = new string[] { "DueTime", ChosencardData.DueTime, "", "Total Amount:", decimal.Round(ChosencardData.TotalMoney).ToString() };
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                     .FixedtextColumnLeftColor(TimeAndTotalMoney) //加入欄位名稱到一列
             );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };          
        }
    }
}