using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.StoreModels
{
    public class ResultTotalModule
    {
        public string OrderId { get; set; }
        public string StoreName { get; set; }
        public List<ResultTotalItemsGroupModule> AllTotalItems { get; set; }
        public string DueTime { get; set; }
        public decimal TotalMoney { get; set; }
    }
    public class ResultTotalItemModule
    {
        public int Quantity { get; set; }
        public string UserName { get; set; }
        public string MemberId { get; set; }
    }
    public class ResultTotalItemsGroupModule
    {
        public List<ResultTotalItemModule> TotalItemsGroup { get; set; }
        public string Dish_Name { get; set; }
        public decimal Price { get; set; }
        public int TotalQuantity { get; set; }
        public string TotalOrderName { get; set; }
        public decimal TotalItemMoney { get; set; }
    }
}
