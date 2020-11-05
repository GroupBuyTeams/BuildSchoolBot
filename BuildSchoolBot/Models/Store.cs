using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Models
{
    public class Store
    {
        public string Store_Name { get; set; }
        public string Store_Url { get; set; }
    }
    public class Store_Card
    {
        public string Store_Name { get; set; }
        public string Store_Url { get; set; }
    }
    public class Store_List
    {
        public List<Store_Card> Stores { get; set; }
    }
}
