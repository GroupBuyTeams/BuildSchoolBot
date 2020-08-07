using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class ResultTotal
    {
        public class ResultTotalItem
        {
            public int Quantity { get; set; }
            public string UserName { get; set; }        
            public string MemberId { get; set; }
        }
        public class ResultTotalItemsGroup
        {
            public List<ResultTotalItem> TotalItemsGroup { get; set; }
            public string Dish_Name { get; set; }
            public decimal Price { get; set; }
        }
        public class AllTotalItemsGroups
        {
            public List<ResultTotalItemsGroup> AllTotalItems { get; set; }
        }

    }
}
