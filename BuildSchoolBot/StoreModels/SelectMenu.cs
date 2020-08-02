using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class SelectMenu
    {
        public class SelectMenuData
        {
            public string Quantity { get; set; }
            public string Remarks { get; set; }
            public string Dish_Name { get; set; }
            public string Price { get; set; }
        }
        public class SelectMenuDatagroup
        {
            public List<SelectMenuData> SelectMenu { get; set; }
        }
    }
}
