using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class OrderDetailService
    {
        public TeamsBuyContext context;

        //create OrderDetail
        public void CreateOrderDetail(string OrderId)
        {
            var detail = new OrderDetail
            {
                OrderId = Guid.Parse(OrderId),
            };
            context.OrderDetail.Add(detail);
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
        //該用戶只能移除該用戶的order //IEnumerable多筆資料
        public IEnumerable<OrderDetail> GetUserOrder(string orderId, string userId)
        {
            return context.OrderDetail.Where(x => x.OrderId.ToString().Equals(orderId) && x.MemberId.ToString().Equals(userId));

        }
    }
}
