using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class MenuService
    {
        public TeamsBuyContext context;
        public void CreateMenu(string _menuId)
        {
            var menu = new MenuOrder
            {
                MenuId = Guid.Parse(_menuId),
                //Store = ,
                //TeamsId = ,

            };
            context.MenuOrder.Add(menu);
            context.SaveChanges();
        }
    }
}
