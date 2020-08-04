using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class OrderService
    {
        public TeamsBuyContext context;
        //create Order
        public void CreateOrder(string OrderId , string GroupId)
        {
            var order = new Order
            {
                OrderId = Guid.Parse(OrderId),
                GroupId = GroupId,
                Date = DateTime.Now

            };
            context.Order.Add(order);
            context.SaveChanges();
        }
        //delete Order
        //public void DeleteOrder(string jdata)
        //{
        //    var delete_order = context.Order.FirstOrDefault(x => x.OrderId.ToString().Equals(jdata));
        //    context.Order.Remove(delete_order);
        //    context.SaveChanges();
        //}
        
    }
}
