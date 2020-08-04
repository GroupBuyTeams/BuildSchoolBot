using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class AllSelectData
    {
        public class SelectData
        {
            public string Quantity { get; set; }
            public string Remarks { get; set; }
            public string Dish_Name { get; set; }
            public string Price { get; set; }
        }
        public class SelectAllDataGroup
        {
            public List<SelectData> SelectAllOrders { get; set; }
            public string UserID { get; set; }
            public string StoreName { get; set; }
        }

    }
}
