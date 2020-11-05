using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.ModifyMenu;

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

        public void CreateMenuDetail(ModifyGroup SelectObject)
        {
            foreach (var lists in SelectObject.AllModifyMultiple)
            {
                var detail = new MenuDetail
                {
                    MenuDetailId = Guid.NewGuid(),
                    ProductName = lists.ProductName,
                    Amount = decimal.Parse(lists.Amount),
                    MenuId =Guid.Parse(lists.MenuId)
                };
                context.MenuDetail.Add(detail);
            }
            context.SaveChanges();
        }
    }
}
