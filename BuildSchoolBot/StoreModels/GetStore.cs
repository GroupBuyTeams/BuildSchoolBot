using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class GetStore
    {
          public class Storenameitems
        {
            public string Store_Name { get; set; }
            public string Store_Url { get; set; }
        }

        public class Storenamegroup
        {
            public List<Storenameitems> properties { get; set; }
        }
    }
}
