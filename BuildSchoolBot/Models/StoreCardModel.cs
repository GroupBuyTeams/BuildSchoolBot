using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Models
{
    public class StoreCardModel
    {
        public string ID { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public string DueTime { get; set; }
    }
}
