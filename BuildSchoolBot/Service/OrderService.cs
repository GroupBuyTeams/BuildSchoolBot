﻿using BuildSchoolBot.Models;
using BuildSchoolBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class OrderService
    {
        private EGRepository<Order> _repo;
        protected readonly TeamsBuyContext context;

        public OrderService(TeamsBuyContext _context)
        {
            context = _context;
        }
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
        public Order GetOrder(string orderId)
        {
            //如搜尋結果可能為多筆資料，則需回傳IEnumerable，否則回傳單一物件即可
            return context.Order.SingleOrDefault(x => x.OrderId.ToString().Equals(orderId));
        }
        //delete Order
        public void DeleteStore(Guid orderId)
        {
            var entity = _repo.GetAll().FirstOrDefault(x => x.OrderId.Equals(orderId));

            _repo.Delete(entity);
            _repo.context.SaveChanges();
        }
    }
}
