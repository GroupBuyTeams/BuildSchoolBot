﻿using BuildSchoolBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.ModifyMenu;

namespace BuildSchoolBot.Service
{
    public class MenuService
    {
        public TeamsBuyContext context;
        public MenuService(TeamsBuyContext _context)
        {
            context = _context;
        }

        public void CreateMenu(string store,string teamsId)
        {
            //var guid = Guid.NewGuid().ToString();
            var menu = new MenuOrder
            {
                //MenuId = Guid.Parse(menuId),
                MenuId = Guid.NewGuid(),
                Store = store,
                TeamsId = teamsId,

            };
            context.MenuOrder.Add(menu);
            context.SaveChanges();
        }

        public MenuOrder GetMenuOrder(string MenuId)
        {
            return context.MenuOrder.SingleOrDefault(x => x.MenuId.ToString().Equals(MenuId));
        }

        public void UpdateMenuOrderStoreName(string MenuId,string StoreName)
        {
            var MenuOrderData=context.MenuOrder.SingleOrDefault(x => x.MenuId.ToString().Equals(MenuId));
            MenuOrderData.Store =StoreName;
            context.Update(MenuOrderData);
            context.SaveChanges();
        }




    }
}
