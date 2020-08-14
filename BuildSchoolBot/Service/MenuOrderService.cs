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
                ((AdaptiveChoiceSetInput)myCard.Body[1]).Choices.Add(new AdaptiveChoice()
                {
                    Title = item.Store,
                    Value = item.MenuId.ToString()

                });

            });

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };

            return adaptiveCardAttachment;
        }

        public Attachment GetStore(string Store, string MenuId)
        {
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

            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Join", Data = new AdaptiveCardTaskFetchValue<string>() { Data = MenuId, SetType = "GetCustomizedStore" } });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        public async Task<Attachment> CreateMenu(string menuId)
        {
            var MenuData = await FindMenuOrderByMenuId(menuId);
            var DetailData = await FindMenuOrderDetailByMenuId(menuId);
            var itemsName = new string[] { "菜名", "價錢", "數量", "備註" };


            var OrderData = new CustomizedData()
            {
                MenuId = menuId,
                SetType = "GetCustomizedOrder"
            };


            var card = NewCard()
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = MenuData.MenuId.ToString(),
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                })
                .AddElement(new AdaptiveTextBlock()
                {
                    Text = MenuData.Store,
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
                        new AdaptiveSubmitAction() { Title = "Order", Data = OrderData }//勿必要將傳出去的資料進行Serialize
                    )
            );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
    }

    public class CustomizedData
    {
        [JsonProperty("MenuId")]
        public string MenuId { get; set; }

        [JsonProperty("SetType")]
        public string SetType { get; set; }

    }
}
