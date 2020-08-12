using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BuildSchoolBot.ViewModels;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;

namespace BuildSchoolBot.Service
{
    public class PayMentService
    {
        private readonly PayMentService _paymentService;
        public TeamsBuyContext context;
        public PayMentService(TeamsBuyContext _context)
        {
            context = _context;
        }//middleware
        public void Create(string memberId, string url)
        {
            var con = new TeamsBuyContext();
            var pay = new Payment
            {
                MemberId = memberId,
                Url = url
            };
            con.Payment.Add(pay);
            con.SaveChanges();
        }
        //edit
        public void UpdatePayment(string memberId, string url)
        {
            var payment = context.Payment.FirstOrDefault(x => x.MemberId.Equals(memberId));
            if (payment?.Url == null)
            {
                Create(memberId, url);
            }
            else
            {
                payment.Url = url;
                context.Update(payment);
            }
            context.SaveChanges();
        }
        //取得資料
        public Payment GetPay(string memberId)
        {
            return context.Payment.FirstOrDefault(x => x.MemberId.Equals(memberId));
        }
        public Attachment CreatePayAdaptiveAttachment(string memberId)
        {
            var paths = new[] { ".", "Resources", "PayCard.json" };
            var payCardJson = File.ReadAllText(Path.Combine(paths));

            var myCard = JsonConvert.DeserializeObject<AdaptiveCard>(payCardJson);
            ((AdaptiveTextInput)myCard.Body[1]).Value = GetPay(memberId)?.Url;

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };
            return adaptiveCardAttachment;
        }
    }
}
