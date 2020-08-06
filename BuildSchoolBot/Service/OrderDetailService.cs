using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.AllSelectData;

namespace BuildSchoolBot.Service
{
    public class OrderDetailService
    {
        //protected 保護不能做修改
        protected readonly TeamsBuyContext context;
        //當在記憶體內新增這個類別的執行個體時,要先做這個程式(建構式)
        public OrderDetailService(TeamsBuyContext _context)
        {
            context = _context;
        }//middleware
        //create OrderDetail
        public void CreateOrderDetail(SelectAllDataGroup SelectObject, List<SelectData> SelectAllOrders,Guid orderId)
        {
            foreach(var lists in SelectAllOrders)
            {
                var detail = new OrderDetail
                {
                    OrderId = orderId,
                    OrderDetailId = Guid.NewGuid(),
                    ProductName = lists.Dish_Name,
                    Amount = decimal.Parse(lists.Price),
                    Number = int.Parse(lists.Quantity),
                    MemberId = SelectObject.UserID,
                    Mark = lists.Remarks,
                };
                context.OrderDetail.Add(detail);               
            }
            context.SaveChanges();
        }
        //delete OrderDetail
        public void DeleteOrderDetail(string orderId , string userId)
        {
            var delete_details = GetUserOrder(orderId, userId);
            foreach (var detail in delete_details)
            {
                context.OrderDetail.Remove(detail);
            }
            context.SaveChanges();
        }
        //顯示OrderDetail
        public IEnumerable<OrderDetail> GetOrderDetail(string orderId)
        {
            return context.OrderDetail.Where(x => x.OrderId.ToString().Equals(orderId));

        }
        //該用戶只能移除該用戶的order //IEnumerable多筆資料
        public IEnumerable<OrderDetail> GetUserOrder(string orderId, string userId)
        {
            return context.OrderDetail.Where(x => x.OrderId.ToString().Equals(orderId) && x.MemberId.ToString().Equals(userId));


        }
    }
}
