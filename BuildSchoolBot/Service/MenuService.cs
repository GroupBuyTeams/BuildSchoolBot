﻿using BuildSchoolBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public void CreateMenu(AdaptiveCardDataFactory dataFactory, string teamsId)
        {
            var Data = JObject.FromObject(dataFactory.Request.Data);
            var store = Data.GetValue("store").ToString();

            var name = Data.Properties().Where(x => x.Name.Contains("name")).ToList();
            var price = Data.Properties().Where(x => x.Name.Contains("price")).ToList();

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

            for (int i = 0; i < name.Count(); i++)
            {
                if (name[i].Value.ToString().Equals("") || price[i].Value.ToString().Equals("0"))
                    break;
                else
                {
                    var menuDetail = new MenuDetail()
                    {
                        MenuDetailId = Guid.NewGuid(),
                        ProductName = name[i].Value.ToString(),
                        Amount = decimal.Parse(price[i].Value.ToString()),
                        MenuId = menu.MenuId
                    };

                    context.MenuDetail.Add(menuDetail);
                    context.SaveChanges();
                }

            };
        }

        public MenuOrder GetMenuOrder(string MenuId)
        {
            return context.MenuOrder.SingleOrDefault(x => x.MenuId.ToString().Equals(MenuId));
        }

        public void UpdateMenuOrderStoreName(string MenuId, string StoreName)
        {
            var MenuOrderData = context.MenuOrder.SingleOrDefault(x => x.MenuId.ToString().Equals(MenuId));
            MenuOrderData.Store = StoreName;
            context.Update(MenuOrderData);
            context.SaveChanges();
        }




    }
}
