using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class PayMentService
    {
        public TeamsBuyContext context;

        public void Update(string _memberId)
        {
            var pay = new Payment
            {
                MemberId = _memberId,
                //Url = 
            };
            context.Payment.Add(pay);
            context.SaveChanges();
        }
    }
}
