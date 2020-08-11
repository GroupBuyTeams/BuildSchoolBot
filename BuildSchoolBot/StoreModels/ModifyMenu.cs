using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class ModifyMenu
    {
        public class ModifyMultiple
        {
            public string ProductName { get; set; }
            public string Amount { get; set; }
            public string MenuId { get; set; }
        }
        public class ModifyGroup
        {
            public List<ModifyMultiple> AllModifyMultiple { get; set; }           
            public string StoreName { get; set; }
        }

    }
}
