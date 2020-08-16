using AdaptiveCards;
using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using BuildSchoolBot.StoreModels;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.Service.CardAssemblyFactory;
using static BuildSchoolBot.Service.CardActionFactory;

namespace BuildSchoolBot.Service
{
    public class MenuOrderService
    {
        private readonly EGRepository<MenuOrder> _repo;
        private readonly EGRepository<MenuDetail> _repoDetail;
        public MenuOrderService(EGRepository<MenuOrder> repo, EGRepository<MenuDetail> repoDetail)
        {
            _repo = repo;
            _repoDetail = repoDetail;
        }
        public IQueryable<MenuOrder> FindMenuOrderByTeamsId(string teamsId)
        {
            return _repo.GetAll().Where(x => x.TeamsId.Equals(teamsId));
        }
        public async Task<MenuOrder> FindMenuOrderByMenuId(string menuId)
        {
            var result = _repo.GetAll().FirstOrDefault(x => x.MenuId.ToString().Equals(menuId));
            return await Task.FromResult(result);
        }
        public async Task<List<MenuDetail>> FindMenuOrderDetailByMenuId(string menuId)
        {
            var result = _repoDetail.GetAll().Where(x => x.MenuId.ToString().Equals(menuId)).ToList();
            return await Task.FromResult(result);
        }
        public Attachment CreateMenuOrderAttachment(string teamsId)
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "MenuOrderCard.json" };

            var libraryCardJson = File.ReadAllText(Path.Combine(paths));

            var myCard = JsonConvert.DeserializeObject<AdaptiveCard>(libraryCardJson);
            var menuOrder = FindMenuOrderByTeamsId(teamsId).ToList();

            menuOrder.ForEach(item =>
            {
                var cardData = new CardDataModel<StoreOrderDuetime>() { Type = "GetCustomizedStore", Value = new StoreOrderDuetime { MenuID = item.MenuId.ToString(), StoreName = item.Store } };//包資料到Submit Action, Type是給EchoBot判斷用的字串，Value是要傳遞資料


                ((AdaptiveChoiceSetInput)myCard.Body[1]).Choices.Add(new AdaptiveChoice()
                {
                    Title = item.Store,
                    Value = JsonConvert.SerializeObject(cardData)

                });

            });

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };

            return adaptiveCardAttachment;
        }

        public Attachment GetStore(AdaptiveCardDataFactory dataFactory)
        {
            var MenuId = dataFactory.GetCardData<StoreOrderDuetime>().MenuID;
            var Store = dataFactory.GetCardData<StoreOrderDuetime>().StoreName;

            var cardData = new CardDataModel<StoreOrderDuetime>() { Type = "GetCustomizedMenu", Value = new StoreOrderDuetime { OrderID = Guid.NewGuid().ToString(), MenuID = MenuId, StoreName = Store } };//包資料到Submit Action, Type是給EchoBot判斷用的字串，Value是要傳遞資料

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            var TextBlockStorName = new AdaptiveTextBlock
            {
                Size = AdaptiveTextSize.Large,
                Weight = AdaptiveTextWeight.Bolder,
                Text = Store,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Center
            };
            card.Body.Add(TextBlockStorName);

            actionSet.Actions.Add(new AdaptiveSubmitAction().SetOpenTaskModule("Join", JsonConvert.SerializeObject(cardData)));
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        public async Task<Attachment> CreateMenu(AdaptiveCardDataFactory dataFactory)
        {

            var storeData = dataFactory.GetCardData<StoreOrderDuetime>();
            var DetailData = await FindMenuOrderDetailByMenuId(storeData.MenuID);
            var itemsName = new string[] { "菜名", "價錢", "數量", "備註" };

            var cardData = new CardDataModel<StoreOrderDuetime>()//務必按照此格式新增需要傳出去的資料
            {
                Type = "FetchSelectedFoods", //於EchoBot判斷用
                Value = storeData //要傳出去的資料和資料結構
            };


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


            DetailData.ForEach(item =>
            {

                card
                    .AddRow(new AdaptiveColumnSet() { Separator = true }
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() { Text = item.ProductName }))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() { Text = item.Amount.ToString() }))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveNumberInput() { Min = 0, Value = 0, Id = $"{item.ProductName}&&{item.Amount}" })) //Input相關的一定要給ID，且每個ID必須不一樣，否則傳回TaskModuleSubmit的時候會抓不到
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextInput() { Placeholder = "備註", Id = $"{item.ProductName}&&mark" }))
                    );

            });

            card.AddElement(new AdaptiveTextBlock()
            {
                Text = $"Due Time:",// {storeData.DueTime}
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
    }

    public class CustomizedData
    {
        [JsonProperty("MenuId")]
        public string MenuId { get; set; }

        [JsonProperty("StoreName")]
        public string StoreName { get; set; }

    }
}
