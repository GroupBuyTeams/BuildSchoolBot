using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class GetStoredata
    {
        public class Storenameitems
        {
            public string StoreName { get; set; }
            public string menuproperties { get; set; }
        }
        public class Storenamegroup
        {
            public List<Storenameitems> properties { get; set; }
        }
    }
}
