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

namespace BuildSchoolBot.Service
{
    public class MenuOrderService
    {
        private readonly EGRepository<MenuOrder> _repo;
        public MenuOrderService(EGRepository<MenuOrder> repo)
        {
            _repo = repo;
        }
        public IQueryable<MenuOrder> FindMenuOrderByTeamsId(string teamsId)
        {
            return _repo.GetAll().Where(x => x.TeamsId.Equals(teamsId));
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
                ((dynamic)((AdaptiveSubmitAction)myCard.Actions[0]).Data).Name = item.Store;

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
    }
}
