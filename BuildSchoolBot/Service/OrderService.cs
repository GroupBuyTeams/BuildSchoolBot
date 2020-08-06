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
        public void CreateOrder(string _orderId , string _channelId)
        {
            var order = new Order
            {
                OrderId = Guid.Parse(_orderId),
                ChannelId = _channelId,
                Date = DateTime.Now,
                //StoreName = storeName
            };
            context.Order.Add(order);
            context.SaveChanges();
        }
        //顯示Order
        public IEnumerable<Order> GetOrder(string orderId)
        {
            return context.Order.Where(x => x.OrderId.ToString().Equals(orderId));

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
