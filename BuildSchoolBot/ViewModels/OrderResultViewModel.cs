namespace BuildSchoolBot.ViewModels
{
    public class OrderResults
    {
        public string Dish_Name { get; set; }
        public string Price { get; set; }
        public OrderInfo[] TotalItemsGroup { get; set; }
    }

    public class OrderInfo
    {
        public string UserName { get; set; }
        public string MemberId { get; set; }
        public int Quantity { get; set; }
    }
}