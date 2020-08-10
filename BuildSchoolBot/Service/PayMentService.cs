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

namespace BuildSchoolBot.Service
{
    public class PayMentService
    {
        public TeamsBuyContext context;
        //public PayMentService(TeamsBuyContext _context)
        //{
        //    context = _context;
        //}//middleware

        public void Create(string _memberId,string _uri)
        {
            var con = new TeamsBuyContext();
            var pay = new Payment
            {
                MemberId = _memberId,
                Url = _uri
            };
            con.Payment.Add(pay);
            con.SaveChanges();
        }
        //edit
        public Payment GetPay(string memberId)
        {
            return context.Payment.FirstOrDefault(x => x.MemberId.ToString().Equals(memberId));

        }
        public Attachment CreatePayAdaptiveAttachment()
        {
            var paths = new[] { ".", "Resources", "PayCard.json" };
            var payCardJson = File.ReadAllText(Path.Combine(paths));

            var myCard = JsonConvert.DeserializeObject<AdaptiveCard>(payCardJson);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = myCard
            };

            return adaptiveCardAttachment;
        }

        //use memberId to find the pay
        //public async Task<Payment> FindPayByMemberId(string memberId)
        //{
        //    var result = _repo.GetAll().Where(x => x.MemberId == memberId).FirstOrDefault();
        //    return await Task.FromResult(result);

        //}
        //public async Task<Payment> FindPayByUriAndMemberId(string uri, string memberId)
        //{
        //    var result = _repo.GetAll().Where(x => x.MemberId.Equals(memberId) && x.Url.Equals(uri)).FirstOrDefault();
        //    return await Task.FromResult(result);
        //}
        //public async Task<Attachment> GetPayCard(ITurnContext turnContext)
        //{
        //    var memberId = turnContext.Activity.From.Id;

        //    var Name = turnContext.Activity.From.Name;
        //    var payMemberId = await _payMentService.FindPayByMemberId(memberId);
        //    var payCard = PayMentService.CreatePayAdaptiveAttachment(payMemberId, Name);
        //    return payCard;
        //}
    }
}
