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
        public Attachment GetStore(string text, string menuUrl, string OrderId, string DueTime)
        {

            var cardData = new CardDataModel<StoreInfoData>()
            {
                Type = "OpenMenuTaskModule",
                Value = new StoreInfoData() { Name = text, Url = menuUrl, Guid = OrderId, DueTime = DueTime }
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
                    Text = text,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction().SetOpenTaskModule("Join", JsonConvert.SerializeObject(cardData)))
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Favorite", Data = objData })
                        //ting
                        .AddActionToSet(new AdaptiveSubmitAction() { Title = "Delete" })
                );

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
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
                Value = new StoreInfoData() { Guid = storeData.Guid, Name = storeData.Name, DueTime = storeData.DueTime } //要傳出去的資料和資料結構
            };
            var itemsName = new string[] { "菜名", "價錢", "數量", "備註" };

            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = storeData.Guid,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = storeData.Name,
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
                            .AddElement(new AdaptiveTextBlock() { Text = foods[i].Price.ToString() }))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveNumberInput() { Min = 0, Value = 0, Id = $"{foods[i].Dish_Name}&{foods[i].Price}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextInput() { Placeholder = "備註", Id = $"{foods[i].Dish_Name}&mark" }))
                    );
            }

            card.AddElement(new AdaptiveTextBlock()
            {
                Text = "Due Time:" + storeData.DueTime,
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
            //顯示於TaskModule上方的欄位名稱
            var itemsName = new string[] { "食物名稱", "價錢", "數量", "備註", "單品總金額" };
            var cardData = dataFactory.GetCardData<StoreInfoData>();

            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewCard()
                .AddElement(new AdaptiveTextBlock() //加入訂單Guid
                {
                    Text = cardData.Guid,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock() //加入餐廳名稱
                {
                    Text = cardData.Name + "訂單",
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
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
                                .AddElement(new AdaptiveTextBlock() { Text = p.Dish_Name }) //在欄位內加入餐點名稱的文字
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = p.Price }) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = p.Quantity }) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = p.Remarks }) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = totalSingleMoney.ToString() }) //加入此餐點的總價
                        )
                    );
                }
            }

            //顯示於TaskModule下方的文字資訊
            var timeAndTotalMoney = new string[] { "DueTime", cardData.DueTime, "", "總金額:", totalMoney.ToString() };

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
                });
            //回傳卡片
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        //ting
        public Attachment GetCreateMenu()
        {
            // var storeInfo = new StoreInfoData() { Name = storeName, Guid = guid };
            var guid = Guid.NewGuid().ToString();

            var cardData = new CardDataModel<StoreInfoData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "GetCustomizedMenu", //於EchoBot判斷用
                Value = new StoreInfoData() { Guid = guid } //要傳出去的資料和資料結構
            };

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
                    Text = "Enter your Store",
                    Size = AdaptiveTextSize.Medium,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                })
                .AddElement(new AdaptiveTextInput()
                {
                    Placeholder = "Store",
                    Id = $"store"
                }) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到

                .AddRow(new AdaptiveColumnSet()
                    .AddColumnsWithStrings(itemsName)
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
                             .AddElement(new AdaptiveNumberInput() { Min = 0, Value = 0, Placeholder = "Price", Id = $"price&{i}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                    );
            }
            card
             .AddActionsSet(
                NewActionsSet()
                    .AddActionToSet(
                        new AdaptiveSubmitAction() { Title = "Create", Data = JsonConvert.SerializeObject(cardData) }//勿必要將傳出去的資料進行Serialize
                    )
            );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }


        public void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }

        public void GetChosenFoodFromMenuCreateOrderDetail(AdaptiveCardDataFactory dataFactory,string UserId)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var CardData = dataFactory.GetCardData<StoreInfoData>();
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
            new OrderDetailService(context).CreateOrderDetail(SelectObject,ChosenFoodFromMenu, Guid.Parse(CardData.Guid));
        }

        public Attachment GetResultTotal(string OrderId, string StoreName, string Orderfoodjson, string DueTime)
        {         
            string[] ItemsName = new string[] { "Food Name", "Price", "Quantity", "Remarks", "Total" };

            //新增一基本卡片，並且附加此訂單的Guid、餐廳名稱、欄位名稱等文字訊息
            var card = NewCard()
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
                                .AddElement(new AdaptiveTextBlock() { Text = root.AllTotalItems[i].Price.ToString() }) //加入餐點價格
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = TotalQuantity.ToString() }) //加入餐點數量
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = TotalOrderName.TrimStart(',') }) //加入備註
                        )
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextBlock() { Text = TotalItemMoney.ToString() }) //加入此餐點的總價
                        )
                    );                  
                }
            }     
            string[] TimeAndTotalMoney = new string[] { "DueTime", DueTime, "", "Total Amount:", TotalMoney.ToString() };
            card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                     .FixedtextColumnLeftColor(TimeAndTotalMoney) //加入欄位名稱到一列
             );          
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };    
        }
        public Attachment GetError(string UserName)
        {
            var card = NewCard()
                  .AddElement(new AdaptiveTextBlock()
                  {
                      Text = "Error.Please write again",
                      Size = AdaptiveTextSize.Large,
                      Weight = AdaptiveTextWeight.Bolder,
                      HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                  })
                   .AddElement(new AdaptiveTextBlock()
                   {
                       Text = UserName,
                       Size = AdaptiveTextSize.Small,
                       Color=AdaptiveTextColor.Good,
                       Weight = AdaptiveTextWeight.Bolder,
                       HorizontalAlignment = AdaptiveHorizontalAlignment.Left
                   });            
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }


        public Attachment GetCustomizedModification(string Store, List<MenuDetail> menuDetails, string MenuId)
        {

            var cardData = new CardDataModel<StoreInfoData>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "CustomizedModification", //於EchoBot判斷用
                Value = new StoreInfoData() { Guid = MenuId} //要傳出去的資料和資料結構
            };
            string[] ItemsStoreName = new string[] { Store, "" };
            string[] ItemsName = new string[] { "Food Name", "Price" };
            var card = NewCard()
                 .AddRow(new AdaptiveColumnSet().
                        FixedInputTextAdjustWidthColumn(ItemsStoreName)
                )
                  .AddRow(new AdaptiveColumnSet().
                        AddColumnsWithStrings(ItemsName)
                );

            for (var i = 0; i < menuDetails.Count; i++)
            {
                card.AddRow(new AdaptiveColumnSet() //加入一列到卡片裡
                        .AddCol(new AdaptiveColumn() //加入一欄位到一列
                                .AddElement(new AdaptiveTextInput() { Id= menuDetails[i].ProductName + i.ToString(),Value= menuDetails[i].ProductName }) //在欄位內加入餐點名稱的文字
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
                       new AdaptiveSubmitAction() { Title = "Modify", Data = JsonConvert.SerializeObject(cardData) }//勿必要將傳出去的資料進行Serialize
                   )
           );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }


        public Attachment GetResultCustomizedModification(string Store, List<ModifyMultiple> menuDetails)
        {
            string[] ItemsName = new string[] { "Food Name", "Price" };
            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = Store
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
                                .AddElement(new AdaptiveTextBlock() { Text = menuDetails[i].Amount.ToString() }) //加入餐點價格
                        )
                    );
            }    
           
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }
}