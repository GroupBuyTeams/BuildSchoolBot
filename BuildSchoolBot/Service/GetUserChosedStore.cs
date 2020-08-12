using BuildSchoolBot.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class GetUserChosedStore
    {
        public List<StoreOrderDuetime> GetResultStore(string Data)
        {
            //產生訂單ID
            string GetGUID()
            {
                System.Guid guid = new Guid();
                guid = Guid.NewGuid();
                string str = guid.ToString();
                return str;
            }
            //抓取店家跟連結
            var Stores = Data.Split('"');
            var count = 0;
            foreach(var item in Stores)
            {
                if(item.Equals("True"))
                {
                    break;
                }
                count++;
            }
            var Store_Url = Stores[count - 2];
            var _StoreName = Store_Url.Split("^_^")[0];
            var _Url = Store_Url.Split("^_^")[1];
            //抓取時間
            var _time = Data.Split("DueTime")[1].Split('"')[2];
            //產生要回傳的List
            var result = new List<StoreOrderDuetime>();
            result.Add(new StoreOrderDuetime() 
            {
                OrderID = GetGUID(),
                StoreName = _StoreName,
                Url = _Url,
                DueTime = _time
            });
            return result;
        }
    }
}
