using BuildSchoolBot.StoreModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Models
{
    public class OrderModuleObj
    {
        public string DishName { get; set; }
        public string Price { get; set; }
        public static UISettings Submit { get; set; } =
            new UISettings(1000, 700, "You Tube Video", TaskModuleIds.Submit, "Submit");
        public static UISettings Close { get; set; } =
            new UISettings(510, 450, "Custom Form", TaskModuleIds.Close, "Submit");
    }
}
