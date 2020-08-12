using AngleSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BuildSchoolBot.Models;
using System.Text.RegularExpressions;

namespace BuildSchoolBot.Service
{
    public class WebCrawler
    {
        //AngleSharp套件及httpClient前置作業
        private static IConfiguration config = Configuration.Default;
        IBrowsingContext context = BrowsingContext.New(config);
        public async Task<string> GetStores(string lat, string lng)
        {
            //AngleSharp套件及httpClient前置作業
            //var config = Configuration.Default;
            //var context = BrowsingContext.New(config);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");
            //設定店家爬蟲的連結
            string LocationUrl = $"https://www.foodpanda.com.tw/restaurants/lat/{lat}/lng/{lng}";
            var StoreInfoResponseMessage = await httpClient.GetAsync(LocationUrl);
            var StoreInfoResult = StoreInfoResponseMessage.Content.ReadAsStringAsync().Result;
            var Store_document = await context.OpenAsync(res => res.Content(StoreInfoResult));
            //設定要爬蟲的資訊
            var StoreName = Store_document.QuerySelectorAll(".vendor-list-section .name");
            var StoreUrl = Store_document.QuerySelectorAll(".vendor-list-section li a");
            //新增搜尋結果List
            List<Store> result_stores = new List<Store>();

            for (var S_count = 0; S_count < StoreName.Length; S_count++)
            {
                Store _Store = new Store();
                _Store.Store_Name = StoreName[S_count].TextContent;
                _Store.Store_Url = "https://www.foodpanda.com.tw" + StoreUrl[S_count].GetAttribute("href");
                result_stores.Add(_Store);
            }

            // return result_stores;
            var JSON_STORES = JsonConvert.SerializeObject(result_stores);

            return JSON_STORES;
        }

        //by 阿三
        public async Task<List<Store>> GetStores2(string lat, string lng)
        {
            //AngleSharp套件及httpClient前置作業
            //var config = Configuration.Default;
            //var context = BrowsingContext.New(config);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");
            //設定店家爬蟲的連結
            string LocationUrl = $"https://www.foodpanda.com.tw/restaurants/lat/{lat}/lng/{lng}";
            var StoreInfoResponseMessage = await httpClient.GetAsync(LocationUrl);
            var StoreInfoResult = StoreInfoResponseMessage.Content.ReadAsStringAsync().Result;
            var Store_document = await context.OpenAsync(res => res.Content(StoreInfoResult));
            //設定要爬蟲的資訊
            var StoreName = Store_document.QuerySelectorAll(".vendor-list-section .name");
            var StoreUrl = Store_document.QuerySelectorAll(".vendor-list-section li a");
            //新增搜尋結果List
            List<Store> result_stores = new List<Store>();

            for (var S_count = 0; S_count < StoreName.Length; S_count++)
            {
                Store _Store = new Store();
                _Store.Store_Name = StoreName[S_count].TextContent;
                _Store.Store_Url = "https://www.foodpanda.com.tw" + StoreUrl[S_count].GetAttribute("href");
                result_stores.Add(_Store);
            }

            return result_stores;
            // var JSON_STORES = JsonConvert.SerializeObject(result_stores);

            // return JSON_STORES;
        }
        public async Task<string> GetOrderInfo(string Url)
        {
            //AngleSharp套件及httpClient前置作業
            //var config = Configuration.Default;
            //var context = BrowsingContext.New(config);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");
            var OrderInfoResponseMessage = await httpClient.GetAsync(Url);
            var OrderInfoResult = OrderInfoResponseMessage.Content.ReadAsStringAsync().Result;
            var Order_document = await context.OpenAsync(res => res.Content(OrderInfoResult));
            //設定要爬蟲的資訊
            var Item_Name = Order_document.QuerySelectorAll(".dish-name span");
            var Item_Price = Order_document.QuerySelectorAll(".price");
            //新增搜尋結果List
            List<Dish> result_order = new List<Dish>();

            for (var I_count = 0; I_count < Item_Name.Length; I_count++)
            {
                Dish dish = new Dish();
                dish.Dish_Name = Item_Name[I_count].TextContent;
                var Raw_Num = Item_Price[I_count].TextContent;
                dish.Price = decimal.Parse(Regex.Replace(Raw_Num, @"[^\d.\d]", ""));
                if (dish.Price == 0)
                {
                    continue;
                }
                result_order.Add(dish);
            }

            // return result_order;

            var json_order = JsonConvert.SerializeObject(result_order);
            return json_order;
        }

        //by 阿三
        public async Task<List<Dish>> GetOrderInfo2(string Url)
        {
            //AngleSharp套件及httpClient前置作業
            //var config = Configuration.Default;
            //var context = BrowsingContext.New(config);
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");
            var OrderInfoResponseMessage = await httpClient.GetAsync(Url);
            var OrderInfoResult = OrderInfoResponseMessage.Content.ReadAsStringAsync().Result;
            var Order_document = await context.OpenAsync(res => res.Content(OrderInfoResult));
            //設定要爬蟲的資訊
            var Item_Name = Order_document.QuerySelectorAll(".dish-name span");
            var Item_Price = Order_document.QuerySelectorAll(".price");
            //新增搜尋結果List
            List<Dish> result_order = new List<Dish>();

            for (var I_count = 0; I_count < Item_Name.Length; I_count++)
            {
                Dish dish = new Dish();
                dish.Dish_Name = Item_Name[I_count].TextContent;
                var Raw_Num = Item_Price[I_count].TextContent;
                dish.Price = decimal.Parse(Regex.Replace(Raw_Num, @"[^\d.\d]", ""));
                if (dish.Price == 0)
                {
                    continue;
                }
                result_order.Add(dish);
            }

            return result_order;

            // var json_order = JsonConvert.SerializeObject(result_order);
            // return json_order;
        }
    }
}
