using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class fooditem
    {
        public class fooditems
        {
            public string Dish_Name { get; set; }
            public string Price { get; set; }
        }

        public class foodgroup
        {
            public List<fooditems> Menuproperties { get; set; }
        }

    }
}
