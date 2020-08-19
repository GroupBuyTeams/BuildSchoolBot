using BuildSchoolBot.Models;
using Microsoft.Bot.Builder;
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

        public MenuOrder CreateMenu(AdaptiveCardDataFactory dataFactory, string teamsId)
        {
            var Data = JObject.FromObject(dataFactory.Request.Data);
            var store = Data.GetValue("store").ToString();

            if (store == "")
            {
                return null;
            }
            else
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

                return menu;
            }
        }
        public void CreateMenuDetail(AdaptiveCardDataFactory dataFactory, MenuOrder menu)
        {
            var Data = JObject.FromObject(dataFactory.Request.Data);

            var name = Data.Properties().Where(x => x.Name.Contains("name")).ToList();
            var price = Data.Properties().Where(x => x.Name.Contains("price")).ToList();

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
