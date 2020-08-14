using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class HelpService
    {
        public  Attachment IntroductionCard()
        {
            var reply = MessageFactory.Text("Welcome to GruopBuyBot!");
            var paths = new[] { ".", "Resources", "IntroductionCard.json" };
            var adaptiveCard = File.ReadAllText(Path.Combine(paths));
            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
            reply.Attachments.Add(attachment);
            return attachment;

        }
        public Attachment Command()
        {
            var reply = MessageFactory.Text("Please enter your command!");
            var paths = new[] { ".", "Resources", "Command.json" };
            var adaptiveCard = File.ReadAllText(Path.Combine(paths));
            var attachment = new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JsonConvert.DeserializeObject(adaptiveCard),
            };
            reply.Attachments.Add(attachment);
            return attachment;
        }
    }
}
