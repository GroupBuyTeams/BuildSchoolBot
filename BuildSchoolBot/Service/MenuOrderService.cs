using AdaptiveCards;
using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using BuildSchoolBot.ViewModels;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class MenuOrderService
    {

        private EGRepository<MenuOrder> _repo;
        public MenuOrderService(EGRepository<MenuOrder> repo)
        {
            _repo = repo;
        }
        public static Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "MenuOrderCard.json" };

            var libraryCardJson = File.ReadAllText(Path.Combine(paths));

            var myCard = JsonConvert.DeserializeObject<AdaptiveCard>(libraryCardJson);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };

            return adaptiveCardAttachment;
        }

    }
}
