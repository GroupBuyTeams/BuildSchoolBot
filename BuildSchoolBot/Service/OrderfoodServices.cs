using AdaptiveCards;
using BuildSchoolBot.Models;
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
using static BuildSchoolBot.StoreModels.ModifyMenu;
using static BuildSchoolBot.StoreModels.SelectMenu;

namespace BuildSchoolBot.Service
{
    public class OrderfoodServices
    {
        //protected readonly OrderfoodServices _orderfoodServices;
        //protected readonly MenuService _menuService;
        //protected readonly MenuDetailService _menuDetailService;
        //protected readonly OrganizeStructureService _organizeStructureService;
        //public OrderfoodServices(OrderfoodServices orderfoodServices,MenuService menuService, MenuDetailService menuDetailService, OrganizeStructureService organizeStructureService)
        //{
        //    _orderfoodServices = orderfoodServices;
        //    _menuService = menuService;
        //    _menuDetailService = menuDetailService;
        //    _organizeStructureService = organizeStructureService;
        //}
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
                parts.Add(new SelectMenuData() { Quantity = inputlist[4 * i + 1], Remarks = inputlist[4 * i + 3], Dish_Name = GetStr(inputlist[4 * i], "Quantity1357", true), Price = GetStr(inputlist[4 * i], "Quantity1357", false) });
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
        public string GetStr(string Str, string Peername, bool LeftDirection)
        {
            //int count = Str.IndexOf(Peername);
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
            if (LeftDirection == true)
            {
                return Str.Substring(0, count - 11);
            }
            else
            {
                return Str.Substring(count + 1, Str.Length - count - 1);
            }
        }
        public void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }    
        public void MenuModule(AdaptiveColumnSet ColumnSetitem, string foodname, string money, string Dishname)
        {
            //食物名稱
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(foodname)));
            //錢
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(money)));
            //數量
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveNumber(Dishname + "Quantity1357" + money,"Enter a number")));
            //備註
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveText(Dishname + "Remarks" + money)));
        }

        public AdaptiveColumn AddColumn<T>(T adaptiveElement) where T : AdaptiveElement
        {
            var result = new AdaptiveColumn();
            result.Width = AdaptiveColumnWidth.Stretch;
            var Container = new AdaptiveContainer();
            Container.Items.Add(adaptiveElement);
            result.Items.Add(Container);
            return result;
        }

        public AdaptiveTextBlock GetadaptiveTextBlock(string InputTxt)
        {
            var TextBlock= new AdaptiveTextBlock();
            TextBlock.Text = InputTxt;
            return TextBlock;
        }
        public AdaptiveTextBlock GetadaptiveTextBlock(string InputTxt, AdaptiveTextWeight Weight, AdaptiveTextColor Color)
        {
            var TextBlock = GetadaptiveTextBlock(InputTxt);
            TextBlock.Weight = Weight;
            TextBlock.Color = Color;
            return TextBlock;
        }

        public AdaptiveTextBlock GetadaptiveTextBlock(string InputTxt, AdaptiveTextSize Size, AdaptiveTextWeight Weight, AdaptiveHorizontalAlignment adaptiveHorizontalAlignment)
        {
            var TextBlock = GetadaptiveTextBlock(InputTxt);
            TextBlock.Size = Size;
            TextBlock.Weight = Weight;
            TextBlock.HorizontalAlignment = adaptiveHorizontalAlignment;
            return TextBlock;
        }
            public AdaptiveTextBlock GetadaptiveTextBlock(string InputTxt, AdaptiveTextSize Size,AdaptiveTextColor Color, AdaptiveTextWeight Weight, AdaptiveHorizontalAlignment adaptiveHorizontalAlignment)
        {
            var TextBlock = GetadaptiveTextBlock(InputTxt, Size, Weight,adaptiveHorizontalAlignment);
            TextBlock.Color = Color;
            return TextBlock;
        }
        public AdaptiveNumberInput GetadaptiveNumber(string IdInput,string PlaceholderInput)
        {
            var NumberInput = new AdaptiveNumberInput()
            {
                Id = IdInput,
                Placeholder = PlaceholderInput,
                Min = 0,
                Value = 0
            };
            return NumberInput;
        }

        public AdaptiveTextInput GetadaptiveText(string IdInput)
        {
            var TextInput = new AdaptiveTextInput();
            TextInput.Id = IdInput;
            return TextInput;
        }
        public AdaptiveTextInput GetadaptiveText(string IdInput,string Value)
        {
            var TextInput = GetadaptiveText(IdInput);
            TextInput.Value = Value;
            return TextInput;
        }
        public AdaptiveColumnSet FixedtextColumn(string[]texts)
        {
            var result = new AdaptiveColumnSet() { Separator = true };
            for(int i=0;i< texts.Length; i++)
            {
                result.Columns.Add(AddColumn(GetadaptiveTextBlock(texts[i])));
            }
            return result;
        }
        public AdaptiveColumnSet FixedInputTextAdjustWidthColumn(string[] texts)
        {
            var result = new AdaptiveColumnSet() { Separator = true };
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == "")
                {
                    result.Columns.Add(AddColumn(GetadaptiveTextBlock(texts[i])));
                }
                else
                {
                    result.Columns.Add(AddColumn(GetadaptiveText(texts[i]+i.ToString(), texts[i])));
                }           
            }
            return result;
        }

        public AdaptiveColumnSet FixedtextColumnLeftColor(string[] texts)
        {
            var result = new AdaptiveColumnSet() { Separator = true };
            for (int i = 0; i < texts.Length; i++)
            {
                if (i == 0 || i == 1)
                {
                    result.Columns.Add(AddColumn(GetadaptiveTextBlock(texts[i], AdaptiveTextWeight.Bolder, AdaptiveTextColor.Good)));
                }
                else
                {
                    result.Columns.Add(AddColumn(GetadaptiveTextBlock(texts[i])));
                }
            }
            return result;
        }
        public void GetResultClickfoodTem(AdaptiveColumnSet ColumnSetitem, string foodname, string money, string Quantity, string Remarks)
        {  
            var TotalSingleMoney = GetTotalMoney(Quantity, money);
            GetItemTem(ColumnSetitem, foodname, decimal.Parse(money), int.Parse(Quantity), Remarks);
            //菜單品項各總價錢
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(TotalSingleMoney.ToString())));
        }

        public void GetItemTem(AdaptiveColumnSet ColumnSetitem, string foodname, decimal money, int Quantity, string Remarks)
        {
            //食物名稱
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(foodname)));
            //單價
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(money.ToString())));
            //數量
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(Quantity.ToString())));
            //備註
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(Remarks)));
        }
        public void GetTotalResultTem(AdaptiveColumnSet ColumnSetitem, string foodname, decimal money, int Quantity, string Remarks, decimal ItemTotalPrice)
        {
            GetItemTem(ColumnSetitem, foodname, money, Quantity, Remarks);
            //菜單品項各總價錢
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(ItemTotalPrice.ToString())));
        }


        public decimal GetTotalMoney(string Quantity,string money)
        {
            var QuantityInt = int.Parse(Quantity);
            var MoneyDecimal = Convert.ToDecimal(money);
            var TotalSingleMoney = QuantityInt * MoneyDecimal;
            return TotalSingleMoney;
        }    
        public JArray GetStoregroup(string json)
        {
            JArray array = JArray.Parse(json);
            return array;
        }
        public string ProcessCustomizedMenu(JObject data)
        {
            var inputlist = new List<string>();
            foreach (var item in data)
            {
                inputlist.Add(item.Value.ToString());
            }
            List<SelectMenuData> parts = new List<SelectMenuData>();

            for (int i = 0; 4 * i < inputlist.Count(); i++)
            {
                parts.Add(new SelectMenuData() { Quantity = inputlist[4 * i + 1], Remarks = inputlist[4 * i + 3], Dish_Name = GetStr(inputlist[4 * i], "Quantity1357", true), Price = GetStr(inputlist[4 * i], "Quantity1357", false) });
            }

            JsonSerializer serializer = new JsonSerializer();
            StringWriter s = new StringWriter();
            serializer.Serialize(new JsonTextWriter(s), parts);
            string SelectJson = s.GetStringBuilder().ToString();
            return SelectJson;
        }

        public async Task<TaskModuleResponse> FinishSelectDishesSubmit(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var TaskInfo = new TaskModuleTaskInfo();
            JObject Data = JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data));
            var StoreAndGuid = Data.Property("data").Value.ToString();
            new OrganizeStructureService().RemoveNeedlessStructure(Data);
            string SelectJson = ProcessAllSelect(Data);
            JObject o = new JObject();
            o["SelectMenu"] = JArray.Parse(SelectJson);
            bool DecideQuanRem = true;
            bool Number = true;
            var AllSelectDatas = JsonConvert.DeserializeObject<SelectMenuDatagroup>(o.ToString());
            foreach (var item in AllSelectDatas.SelectMenu)
            {
                if (item.Quantity == "0" && item.Remarks != "")
                {
                    DecideQuanRem = false;
                }
                if (Math.Sign(decimal.Parse(item.Quantity)) < 0 || (decimal.Parse(item.Quantity) - Math.Floor(decimal.Parse(item.Quantity))) != 0)
                {
                    Number = false;
                }
            }
            if (DecideQuanRem == true && Number == true)
            {
                //取完整資料
                var OAllOrderDatasStr = ProcessUnifyData(o);
                var SelectObject = JsonConvert.DeserializeObject<SelectAllDataGroup>(OAllOrderDatasStr);
                SelectObject.UserID = turnContext.Activity.From.Id;
                var ExistGuid = Guid.Parse("cf1ed7b9-ae4a-4832-a9f4-fdee6e492085");
                new OrderDetailService(context).CreateOrderDetail(SelectObject, SelectObject.SelectAllOrders, ExistGuid);

                TaskInfo.Card = new CreateCardService().GetResultClickfood(new OrganizeStructureService().GetOrderID(StoreAndGuid), new OrganizeStructureService().GetStoreName(StoreAndGuid), o.ToString(), "12:00", turnContext.Activity.From.Name);
                SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(new CreateCardService().GetResultClickfood(new OrganizeStructureService().GetOrderID(StoreAndGuid), new OrganizeStructureService().GetStoreName(StoreAndGuid), o.ToString(), "12:00", turnContext.Activity.From.Name)));
            }
            else
            {
                TaskInfo.Card = new CreateCardService().GetError(turnContext.Activity.From.Name);
               SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(new CreateCardService().GetError(turnContext.Activity.From.Name)));

            }
            return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        }
        public void ModifyMenuData(TaskModuleRequest taskModuleRequest, TaskModuleTaskInfo TaskInfo)
        {
            TeamsBuyContext context = new TeamsBuyContext();
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            JObject Data = JObject.Parse(JsonConvert.SerializeObject(taskModuleRequest.Data));
            new OrganizeStructureService().ModifyRemoveNeedlessStructure(Data);
            var inputlist = new List<string>();
            foreach (var item in Data)
            {
                inputlist.Add(item.Value.ToString());
            }
            inputlist.Remove(inputlist[0]);
            var StoreName = inputlist[0];
            inputlist.Remove(inputlist[0]);
            var Modify = new ModifyGroup();
            var w = new List<ModifyMultiple>();
            for (int i = 0; 2 * i < inputlist.Count(); i++)
            {
                w.Add(new ModifyMultiple() { ProductName = inputlist[2 * i], Amount = inputlist[2 * i + 1], MenuId = Value });
            }
            Modify.AllModifyMultiple = w;
            Modify.StoreName = StoreName;
            for (var i = 0; i < Modify.AllModifyMultiple.Count; i++)
            {
                if (Modify.AllModifyMultiple[i].ProductName == "" || Modify.AllModifyMultiple[i].Amount.ToString() == "")
                {
                    Modify.AllModifyMultiple.Remove(Modify.AllModifyMultiple[i]);
                }
            }
            new MenuService(context).UpdateMenuOrderStoreName(Value, Modify.StoreName);
            new MenuDetailService(context).DeleteMenuDetail(Value);
            new MenuDetailService(context).CreateMenuDetail(Modify);

            TaskInfo.Card = new CreateCardService().GetResultCustomizedModification(Modify.StoreName, Modify.AllModifyMultiple);
            SetTaskInfo(TaskInfo, TaskModuleUIConstants.UpdateMenu);
        }

        public async Task<TaskModuleResponse> GetModifyModuleData(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var TaskInfo = new TaskModuleTaskInfo();
            TeamsBuyContext context = new TeamsBuyContext();        
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            var MenuOrderData = new MenuDetailService(context).GetMenuOrder(Value).ToList();
            var MenuOrderStore = new MenuService(context).GetMenuOrder(Value).Store;
            TaskInfo.Card = new CreateCardService().GetCustomizedModification(MenuOrderStore, MenuOrderData, Value);
            SetTaskInfo(TaskInfo, TaskModuleUIConstants.UpdateMenu);
            return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        }

        public async Task<TaskModuleResponse> GetModuleMenuData(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var TaskInfo = new TaskModuleTaskInfo();
            var asJobject = JObject.FromObject(taskModuleRequest.Data);
            var Value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
            string GetMenuJson = new OrganizeStructureService().GetFoodUrlStr(Value);
            TaskInfo.Card = new OrganizeStructureService().GetTaskModuleFetchCard(Value, GetMenuJson, TaskInfo);
            new OrderfoodServices().SetTaskInfo(TaskInfo, TaskModuleUIConstants.AdaptiveCard);
            return await Task.FromResult(TaskInfo.ToTaskModuleResponse());
        }



        public void Menu(AdaptiveColumnSet ColumnSetitem, string name, string money, string price)
        {
            //name
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveText(name)));
            //$
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(money,AdaptiveTextSize.Medium,AdaptiveTextWeight.Bolder,AdaptiveHorizontalAlignment.Right)));
            //price
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveText(price)));
        }
    }
}
