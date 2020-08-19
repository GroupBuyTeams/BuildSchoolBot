using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class GetChosenData
    {
        public string OrderID { get; set; }
        public string UserID { get; set; }
        public string StoreName { get; set; }
        public string DueTime { get; set; }

    }
}
