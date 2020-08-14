using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class OrderService
    {
        //protected readonly EGRepository<Order> _repo;
        protected readonly TeamsBuyContext context;

        public OrderService(TeamsBuyContext _context)
        {
            context = _context;
        }
        //create Order
        public void CreateOrder(string _orderId , string _channelId, string _storeName)
        {
            var order = new Order
            {
                OrderId = Guid.Parse(_orderId),
                ChannelId = _channelId,
                Date = DateTime.Now,
                StoreName = _storeName
            };
            context.Order.Add(order);
            context.SaveChanges();
        }
        //顯示Order
        public Order GetOrder(string orderId)
        {
            //如搜尋結果可能為多筆資料，則需回傳IEnumerable，否則回傳單一物件即可
            return context.Order.SingleOrDefault(x => x.OrderId.ToString().Equals(orderId));
        }
        //delete Order
        public void DeleteStore(Guid orderId)
        {
            var entity = context.Order.FirstOrDefault(x => x.OrderId.Equals(orderId));
            context.Order.Remove(entity);
            context.SaveChanges();
        }
    }
}
