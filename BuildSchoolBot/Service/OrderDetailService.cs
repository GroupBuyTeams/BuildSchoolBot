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
        public void DeleteOrderDetail(string jdata)
        {
            var delete_detail = context.OrderDetail.FirstOrDefault(x => x.OrderId.ToString().Equals(jdata));
            context.OrderDetail.Remove(delete_detail);
            context.SaveChanges();

        }
        //該用戶只能移除該用戶的order
        public OrderDetail GetUserOrder(string OrderId, string MemberId)
        {
            return context.OrderDetail.FirstOrDefault(x => x.OrderId.ToString().Equals(OrderId) && x.Member.ToString().Equals(MemberId));
        }
    }
}
