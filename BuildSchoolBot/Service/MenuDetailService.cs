using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class MenuDetailService
    {
        public TeamsBuyContext context;
        public MenuDetailService(TeamsBuyContext _context)
        {
            context = _context;
        }
        public void DeleteMenuDetail(string MenuId)
        {
            var delete_details = GetMenuOrder(MenuId);
            foreach (var detail in delete_details)
            {
                context.MenuDetail.Remove(detail);
            }
            context.SaveChanges();
        }
        public IEnumerable<MenuDetail> GetMenuOrder(string MenuId)
        {
            return context.MenuDetail.Where(x => x.MenuId.ToString().Equals(MenuId));
        }

        //public void CreateMenuDetail(SelectAllDataGroup SelectObject, List<SelectData> SelectAllOrders, Guid orderId)
        //{
        //    foreach (var lists in SelectAllOrders)
        //    {
        //        var detail = new MenuDetail
        //        {
        //            MenuDetailId = Guid.NewGuid(),
        //            ProductName = lists.Dish_Name,
        //            Amount = decimal.Parse(lists.Price),
        //            MenuId=
        //        };
        //        context.OrderDetail.Add(detail);
        //    }
        //    context.SaveChanges();
        //}
    }
}
