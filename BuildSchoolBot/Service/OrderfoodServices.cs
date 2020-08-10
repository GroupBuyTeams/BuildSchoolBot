using AdaptiveCards;
using BuildSchoolBot.StoreModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static BuildSchoolBot.StoreModels.AllSelectData;
using static BuildSchoolBot.StoreModels.fooditem;
using static BuildSchoolBot.StoreModels.GetStore;
using static BuildSchoolBot.StoreModels.SelectMenu;

namespace BuildSchoolBot.Service
{
    public class OrderfoodServices
    {
        public string GetGUID()
        {
            System.Guid guid = new Guid();
            guid = Guid.NewGuid();
            string str = guid.ToString();
            return str;
        }

        public string ArrayPlusName(string MenuJson, string ArrayName)
        {
            JArray array = JArray.Parse(MenuJson);
            JObject o = new JObject();
            o[ArrayName] = array;
            string namejson = o.ToString();
            return namejson;
        }

        public string ProcessAllSelect(JObject data)
        {
            var inputlist = new List<string>();
            //var inputname = new List<string>();
            foreach (var item in data)
            {
                inputlist.Add(item.Key.ToString());
                inputlist.Add(item.Value.ToString());
            }
            List<SelectMenuData> parts = new List<SelectMenuData>();

            for (int i = 0; 4 * i < inputlist.Count(); i++)
            {
                parts.Add(new SelectMenuData() { Quantity = inputlist[4 * i + 1], Remarks = inputlist[4 * i + 3], Dish_Name = GetLeftStr(inputlist[4 * i], "Quantity1357"), Price = GetRightStr(inputlist[4 * i], "Quantity1357") });
            }

            JsonSerializer serializer = new JsonSerializer();
            StringWriter s = new StringWriter();
            serializer.Serialize(new JsonTextWriter(s), parts);
            string SelectJson = s.GetStringBuilder().ToString();
            return SelectJson;
        }
        public string ProcessUnifyData(JObject o)
        {
            List<SelectData> SelectOrder = new List<SelectData>();
            var ProcessAllDataJson = JsonConvert.SerializeObject(o);
            var ProcessSelectObject = JsonConvert.DeserializeObject<SelectMenuDatagroup>(ProcessAllDataJson);
            foreach (var item in ProcessSelectObject.SelectMenu)
            {
                if (item.Quantity != "0")
                {
                    SelectOrder.Add(new SelectData() { Quantity = item.Quantity, Remarks = item.Remarks, Dish_Name = item.Dish_Name, Price = item.Price });
                }
            }
            JsonSerializer serializerAllOrderDatas = new JsonSerializer();
            StringWriter SAllOrderDatas = new StringWriter();
            serializerAllOrderDatas.Serialize(new JsonTextWriter(SAllOrderDatas), SelectOrder);
            string SelectJsonAllOrderDatas = SAllOrderDatas.GetStringBuilder().ToString();
            JArray arrayAllOrderDatas = JArray.Parse(SelectJsonAllOrderDatas);
            JObject OAllOrderDatas = new JObject();
            OAllOrderDatas["SelectAllOrders"] = arrayAllOrderDatas;
            var OAllOrderDatasStr = OAllOrderDatas.ToString();
            return OAllOrderDatasStr;
        }    
        public string GetLeftStr(string Str,string Peername)
        {
            int count = 0;
            List<char> Processname = new List<char>();
            string QuantityTxt = Peername;
            string strArray;
            for (int i = 0; i < Str.Length; i++)
            {
                Processname.Add(Str[i]);
                strArray = string.Concat(Processname.ToArray());
                var Firstname = strArray[0].ToString();
                var PeerFirstTxt = Peername[0].ToString();
                if (Firstname != PeerFirstTxt)
                {
                    count++;
                    Processname.Clear();
                }
                else
                {
                    if (strArray == Peername)
                    {
                        break;
                    }
                    else if (QuantityTxt.Contains(strArray))
                    {
                        count++;
                    }
                    else
                    {
                        count++;
                        Processname.Clear();
                    }
                }
            }
            return Str.Substring(0, count - 11);
        }

        public string GetRightStr(string Str, string Peername)
        {
            int count = 0;
            List<char> Processname = new List<char>();
            string QuantityTxt = Peername;
            string strArray;       
            for (int i = 0; i < Str.Length; i++)
            {
                Processname.Add(Str[i]);
                strArray = string.Concat(Processname.ToArray());
                var Firstname = strArray[0].ToString();
                var PeerFirstTxt = Peername[0].ToString();
                if (Firstname != PeerFirstTxt)
                {
                    count++;
                    Processname.Clear();
                }
                else
                {
                    if (strArray == Peername)
                    {
                        break;
                    }
                    else if (QuantityTxt.Contains(strArray))
                    {
                        count++;
                    }
                    else
                    {
                        count++;
                        Processname.Clear();
                    }
                }
            }
            return Str.Substring(count + 1, Str.Length - count - 1);
        }     
        public void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }
        public Attachment GetStore(string texta, string menuurl)
        {
            var Guidstr = GetGUID();
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = texta;
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStorName);

            //actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "click", Data = new AdaptiveCardTaskFetchValue<string>() { Data = texta + "FoodData2468" + menuurl } });
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Join", Data = new AdaptiveCardTaskFetchValue<string>() { Data = texta + "FoodData2468" + menuurl+"GuidStr13579"+ Guidstr } });
            actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Favorite", Data = new AdaptiveCardTaskFetchValue<string>() { Data = "" } });
            card.Body.Add(actionSet);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        private void MenuModule(AdaptiveColumnSet ColumnSetitem, string foodname, string money,string Dishname)
        {
            //食物名稱
            ColumnSetitem.Separator = true;
            var Columnfooditem = new AdaptiveColumn();
            Columnfooditem.Width = AdaptiveColumnWidth.Stretch;
            var containerfoodiitem = new AdaptiveContainer();
            var TextBlockfoodiitem = new AdaptiveTextBlock();
            TextBlockfoodiitem.Text = foodname;
            containerfoodiitem.Items.Add(TextBlockfoodiitem);
            Columnfooditem.Items.Add(containerfoodiitem);

            //錢
            var Columnmoneyitem = new AdaptiveColumn();
            Columnmoneyitem.Width = AdaptiveColumnWidth.Stretch;
            var containermoneyiitem = new AdaptiveContainer();
            var TextBlockmoneyiitem = new AdaptiveTextBlock();
            TextBlockmoneyiitem.Text = money;
            containermoneyiitem.Items.Add(TextBlockmoneyiitem);
            Columnmoneyitem.Items.Add(containermoneyiitem);

            //數量
            var Columnnumberitem = new AdaptiveColumn();
            Columnnumberitem.Width = AdaptiveColumnWidth.Stretch;
            var containernumberiitem = new AdaptiveContainer();
            var Inputnumberiitem = new AdaptiveNumberInput();
            Inputnumberiitem.Id = Dishname+"Quantity1357"+money;
            Inputnumberiitem.Placeholder = "Enter a number";
            Inputnumberiitem.Min = 0;
            Inputnumberiitem.Value = 0;
            containernumberiitem.Items.Add(Inputnumberiitem);
            Columnnumberitem.Items.Add(containernumberiitem);

            //備註
            var ColumnRemarksitem = new AdaptiveColumn();
            ColumnRemarksitem.Width = AdaptiveColumnWidth.Stretch;
            var containerRemarksiitem = new AdaptiveContainer();
            var InputRemarksiitem = new AdaptiveTextInput();
            InputRemarksiitem.Id = Dishname + "Remarks"+ money;
            containerRemarksiitem.Items.Add(InputRemarksiitem);
            ColumnRemarksitem.Items.Add(containerRemarksiitem);

            ColumnSetitem.Columns.Add(Columnfooditem);
            ColumnSetitem.Columns.Add(Columnmoneyitem);
            ColumnSetitem.Columns.Add(Columnnumberitem);
            ColumnSetitem.Columns.Add(ColumnRemarksitem);
         }
        public Attachment CreateClickfoodModule(string Guidstr,string StorName,string modulefoodjson)
         {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            var TextBlockGuid = new AdaptiveTextBlock();
            TextBlockGuid.Size = AdaptiveTextSize.Small;
            TextBlockGuid.Weight = AdaptiveTextWeight.Bolder;
            TextBlockGuid.Text = Guidstr;
            TextBlockGuid.HorizontalAlignment = AdaptiveHorizontalAlignment.Right;
            card.Body.Add(TextBlockGuid);

            var TextBlockStorName = new AdaptiveTextBlock();
            TextBlockStorName.Size = AdaptiveTextSize.Large;
            TextBlockStorName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStorName.Text = StorName;
            TextBlockStorName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStorName);


            string[] itemsname = new string[] { "菜名", "價錢", "數量", "備註" };
            var ColumnSetitemname = new AdaptiveColumnSet();

            ColumnSetitemname.Separator = true;
            for (int i = 0; i < itemsname.Length; i++)
             {
                var Columnitemsname = new AdaptiveColumn();
                Columnitemsname.Width = AdaptiveColumnWidth.Stretch;
                var containeritemsname = new AdaptiveContainer();
                var TextBlockitemsname = new AdaptiveTextBlock();
                TextBlockitemsname.Text = itemsname[i];
                containeritemsname.Items.Add(TextBlockitemsname);
                Columnitemsname.Items.Add(containeritemsname);
                ColumnSetitemname.Columns.Add(Columnitemsname);
            }

            var root = JsonConvert.DeserializeObject<foodgroup>(modulefoodjson);
            card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
                   .Select(cardType => new AdaptiveSubmitAction() { Title = cardType.ButtonTitle, Data = new AdaptiveCardTaskFetchValue<string>() { Data = StorName+ "FoodGuid2468"+ Guidstr } })
                    .ToList<AdaptiveAction>();
            card.Body.Add(ColumnSetitemname);
            foreach (var p in root.Menuproperties)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                MenuModule(ColumnSetitem, p.Dish_Name, p.Price, p.Dish_Name);
                card.Body.Add(ColumnSetitem);
            }

            var TextBlockDueTime = new AdaptiveTextBlock();
            TextBlockDueTime.Size = AdaptiveTextSize.Medium;
            TextBlockDueTime.Weight = AdaptiveTextWeight.Bolder;
            TextBlockDueTime.Text = "Due Time:  123";
            TextBlockDueTime.HorizontalAlignment = AdaptiveHorizontalAlignment.Left;
            card.Body.Add(TextBlockDueTime);
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        public void GetResultClickfoodTem(AdaptiveColumnSet ColumnSetitem, string foodname, string money, string Quantity, string Remarks)
        {
            //數量
            var ColumnQuantityitem = new AdaptiveColumn();
            ColumnQuantityitem.Width = AdaptiveColumnWidth.Stretch;
            var containerQuantityiitem = new AdaptiveContainer();
            var TextBlockQuantityiitem = new AdaptiveTextBlock();
            TextBlockQuantityiitem.Text = Quantity;
            if (Quantity != "0")
            {      
            containerQuantityiitem.Items.Add(TextBlockQuantityiitem);
            ColumnQuantityitem.Items.Add(containerQuantityiitem);


            ColumnSetitem.Separator = true;
            var Columnfooditem = new AdaptiveColumn();
            Columnfooditem.Width = AdaptiveColumnWidth.Stretch;
            var containerfoodiitem = new AdaptiveContainer();
            var TextBlockfoodiitem = new AdaptiveTextBlock();
            TextBlockfoodiitem.Text = foodname;
            containerfoodiitem.Items.Add(TextBlockfoodiitem);
            Columnfooditem.Items.Add(containerfoodiitem);

            //錢
            var Columnmoneyitem = new AdaptiveColumn();
            Columnmoneyitem.Width = AdaptiveColumnWidth.Stretch;
            var containermoneyiitem = new AdaptiveContainer();
            var TextBlockmoneyiitem = new AdaptiveTextBlock();
            TextBlockmoneyiitem.Text = money;
            containermoneyiitem.Items.Add(TextBlockmoneyiitem);
            Columnmoneyitem.Items.Add(containermoneyiitem);

            

            //備註
            var ColumnRemarksitem = new AdaptiveColumn();
            ColumnRemarksitem.Width = AdaptiveColumnWidth.Stretch;
            var containerRemarksitem = new AdaptiveContainer();
            var TextBlockRemarksitem = new AdaptiveTextBlock();
            TextBlockRemarksitem.Text = Remarks;
            containerRemarksitem.Items.Add(TextBlockRemarksitem);
            ColumnRemarksitem.Items.Add(containerRemarksitem);

            //菜單品項各總價錢
            var QuantityInt = int.Parse(Quantity);
            var MoneyDecimal = Convert.ToDecimal(money);
            var TotalSungleMoney = QuantityInt * MoneyDecimal;
            var ColumnTotalSungleMoneyitem = new AdaptiveColumn();
            ColumnTotalSungleMoneyitem.Width = AdaptiveColumnWidth.Stretch;
            var containerTotalSungleMoneyitem = new AdaptiveContainer();
            var TextBlockTotalSungleMoneyitem = new AdaptiveTextBlock();
            TextBlockTotalSungleMoneyitem.Text = TotalSungleMoney.ToString();
            containerTotalSungleMoneyitem.Items.Add(TextBlockTotalSungleMoneyitem);
            ColumnTotalSungleMoneyitem.Items.Add(containerTotalSungleMoneyitem);

            ColumnSetitem.Columns.Add(Columnfooditem);
            ColumnSetitem.Columns.Add(Columnmoneyitem);
            ColumnSetitem.Columns.Add(ColumnQuantityitem);
            ColumnSetitem.Columns.Add(ColumnRemarksitem);
            ColumnSetitem.Columns.Add(ColumnTotalSungleMoneyitem);
            }
        }

        public Attachment GetResultClickfood(string GuidStr,string StoreName,string Orderfoodjson,string DueTime,string UserName)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            var TextBlockGuid = new AdaptiveTextBlock();
            TextBlockGuid.Size = AdaptiveTextSize.Small;
            TextBlockGuid.Weight = AdaptiveTextWeight.Bolder;
            TextBlockGuid.Text = GuidStr;
            TextBlockGuid.HorizontalAlignment = AdaptiveHorizontalAlignment.Right;
            card.Body.Add(TextBlockGuid);

            var TextBlockStoreName = new AdaptiveTextBlock();
            TextBlockStoreName.Size = AdaptiveTextSize.Large;
            TextBlockStoreName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockStoreName.Text = StoreName+"訂單";
            TextBlockStoreName.HorizontalAlignment = AdaptiveHorizontalAlignment.Center;
            card.Body.Add(TextBlockStoreName);

            string[] itemsname = new string[] { "食物名稱", "價錢", "數量", "備註", "單品總金額"};
            var ColumnSetitemname = new AdaptiveColumnSet();

            ColumnSetitemname.Separator = true;
            for (int i = 0; i < itemsname.Length; i++)
            {
                var Columnitemsname = new AdaptiveColumn();
                Columnitemsname.Width = AdaptiveColumnWidth.Stretch;
                var containeritemsname = new AdaptiveContainer();
                var TextBlockitemsname = new AdaptiveTextBlock();
                TextBlockitemsname.Text = itemsname[i];
                containeritemsname.Items.Add(TextBlockitemsname);
                Columnitemsname.Items.Add(containeritemsname);
                ColumnSetitemname.Columns.Add(Columnitemsname);
            }
            var root = JsonConvert.DeserializeObject<SelectMenuDatagroup>(Orderfoodjson);
            card.Body.Add(ColumnSetitemname);
            decimal TotalMoney = 0;
            foreach (var p in root.SelectMenu)
            {
                var ColumnSetitem = new AdaptiveColumnSet();
                GetResultClickfoodTem(ColumnSetitem, p.Dish_Name, p.Price, p.Quantity,p.Remarks);
                var QuantityInt = int.Parse(p.Quantity);
                var MoneyDecimal = Convert.ToDecimal(p.Price);
                var TotalSungleMoney = QuantityInt * MoneyDecimal;
                TotalMoney = TotalMoney +TotalSungleMoney;
                card.Body.Add(ColumnSetitem);
            }
            

            //var TextBlockTotalMoney = new AdaptiveTextBlock();
            //TextBlockTotalMoney.Size = AdaptiveTextSize.Medium;
            //TextBlockTotalMoney.Weight = AdaptiveTextWeight.Bolder;
            //TextBlockTotalMoney.Text= "總金額:" + TotalMoney.ToString();
            //TextBlockTotalMoney.HorizontalAlignment = AdaptiveHorizontalAlignment.Right;
            //card.Body.Add(TextBlockTotalMoney);
        
            string[] TimeAndTotalMoney = new string[] { "DueTime", DueTime, "", "總金額:",TotalMoney.ToString() };
            var ColumnSetTimeAndMoney = new AdaptiveColumnSet();

            ColumnSetTimeAndMoney.Separator = true;
            for (int i = 0; i < TimeAndTotalMoney.Length; i++)
            {
                var ColumnTimeAndMoney = new AdaptiveColumn();
                ColumnTimeAndMoney.Width = AdaptiveColumnWidth.Stretch;
                var containerTimeAndMoney = new AdaptiveContainer();
                var TextBlockTimeAndMoney = new AdaptiveTextBlock();
                TextBlockTimeAndMoney.Text = TimeAndTotalMoney[i];
                containerTimeAndMoney.Items.Add(TextBlockTimeAndMoney);
                ColumnTimeAndMoney.Items.Add(containerTimeAndMoney);
                ColumnSetTimeAndMoney.Columns.Add(ColumnTimeAndMoney);
            }
            card.Body.Add(ColumnSetTimeAndMoney);

            var TextBlockUserName = new AdaptiveTextBlock();
            TextBlockUserName.Size = AdaptiveTextSize.Small;
            TextBlockUserName.Color = AdaptiveTextColor.Good;
            TextBlockUserName.Weight = AdaptiveTextWeight.Bolder;
            TextBlockUserName.Text = UserName;
            TextBlockUserName.HorizontalAlignment = AdaptiveHorizontalAlignment.Left;
            card.Body.Add(TextBlockUserName);


            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };


        }
        public JArray GetStoregroup(string json)
        {
            JArray array = JArray.Parse(json);
            return array;
        }
    }
}
