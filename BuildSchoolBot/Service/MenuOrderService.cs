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
            });

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };

            return adaptiveCardAttachment;
        }
        public Attachment CreateMenuDetailAttachment(string teamsId)
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
    }
}
