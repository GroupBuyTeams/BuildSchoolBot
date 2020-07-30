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
            public string key { get; set; }
            public string value { get; set; }
        }

        public class foodgroup
        {
            public List<fooditems> properties { get; set; }
        }
    }
}
