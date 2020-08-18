using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Models
{
    public class StoreOrderDuetime
    {
        public string OrderID { get; set; }
        public string MenuID { get; set; }
        public string StoreName { get; set; }
        public string Url { get; set; }
        public string DueTime { get; set; }
    }
}
