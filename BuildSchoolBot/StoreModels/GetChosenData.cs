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
        public List<GetChosenDataGroups> GetAllChosenDatas { get; set; }
        public string UserName { get; set; }
    }
    public class GetChosenDataGroups
    {
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public int Number { get; set; }
        public string Mark { get; set; }


    }
}
