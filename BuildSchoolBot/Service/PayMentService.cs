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


namespace BuildSchoolBot.Service
{
    public class PayMentService
    {
        public TeamsBuyContext context;
        private EGRepository<Payment> _repo;
        public PayMentService(EGRepository<Payment> repo)
        {
            _repo = repo;
        }

        public void Update(string _memberId,string _uri)
        {
            var pay = new Payment
            {
                MemberId = _memberId,
                Url = _uri
            };
            context.Payment.Add(pay);
            context.SaveChanges();
        }

        public async Task<Payment> FindPayByMemberId(string memberId)
        {
            var result = _repo.GetAll().Where(x => x.MemberId == memberId).FirstOrDefault();
            return await Task.FromResult(result);

        }
        //public static Attachment CreatePayAdaptiveAttachment(Payment payment,string Name)
        //{
        //    var paths = new[] { ".", "Resources", "PayCard.json" };
        //    var payCardJson = File.ReadAllText(Path.Combine(paths));

        //    var obj = JsonConvert.DeserializeObject<dynamic>(payCardJson);
        //    var card = AdaptiveCards.AdaptiveCard.FromJson(payCardJson).Card;



        //}
    }
}
