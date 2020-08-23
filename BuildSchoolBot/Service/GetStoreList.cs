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
using BuildSchoolBot.ViewModels;
using static BuildSchoolBot.Service.CardAssemblyFactory;
using static BuildSchoolBot.Service.CardActionFactory;

namespace BuildSchoolBot.Service
{
    public class GetStoreList
    {
        //此方法已被CreateStoresModule取代
        // private async Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(TaskModuleRequest taskModuleRequest)
        // {
        //     var asJobject = JObject.FromObject(taskModuleRequest.Data);
        //     var value = asJobject.ToObject<CardTaskFetchValue<string>>()?.Data;
        //     var taskInfo = new TaskModuleTaskInfo();
        //     var LatLng = GetLatLng(value);
        //     string GetStoreJson = await new WebCrawler().GetStores(LatLng.lat, LatLng.lng);
        //     JArray array = JArray.Parse(GetStoreJson);
        //     JObject JStore = new JObject();
        //     JStore["Stores"] = array;
        //     string namejson = JStore.ToString();
        //     taskInfo.Card = CreateClickStoreModule(namejson);
        //     SetTaskInfo(taskInfo, TaskModuleUIConstants.AdaptiveCard);
        //     return await Task.FromResult(taskInfo.ToTaskModuleResponse());
        // }
        
        //此方法已被CreateStoresModule取代
        private void StoreModule(AdaptiveColumnSet ColumnSetitem, string StoreName, string Url)
        {
            //ColumnSetitem.Separator = true;

            //店家名稱
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(StoreName, "StoreName")));
            //店家連結
            ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(Url, "Url")));
            //勾選欄位
            var CheckBox = new AdaptiveToggleInput
            {
                Id = StoreName + "^_^" + Url,
                Title = "Confirm"
            };
            ColumnSetitem.Columns.Add(AddColumn(CheckBox));
        }
        
        //此方法已被CreateStoresModule取代
        private AdaptiveTextInput GetadaptiveTextBlock(string InputTxt, string _ID)
        {
            var TextBlock = new AdaptiveTextInput
            {
                Value = InputTxt,
                Id = _ID
            };
            return TextBlock;
        }
        
        //此方法已被CreateStoresModule取代
        private AdaptiveColumn AddColumn<T>(T adaptiveElement) where T : AdaptiveElement
        {
            var result = new AdaptiveColumn
            {
                Width = AdaptiveColumnWidth.Stretch
            };
            var Container = new AdaptiveContainer();
            Container.Items.Add(adaptiveElement);
            result.Items.Add(Container);
            return result;
        }
        private LatLngService GetLatLng(string address)
        {
            var LatLng = new LatLngService(address);
            return LatLng;
        }
        // 此方法已被GetChooseMenuCard取代
        // private Attachment GetStore(string Address, string StoreData)
        // {
        //     var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
        //     var actionSet = new AdaptiveActionSet() { Type = AdaptiveActionSet.TypeName, Separator = true };
        //     var TextBlockStorName = new AdaptiveTextBlock
        //     {
        //         Size = AdaptiveTextSize.Large,
        //         Weight = AdaptiveTextWeight.Bolder,
        //         Text = Address,
        //         HorizontalAlignment = AdaptiveHorizontalAlignment.Center
        //     };
        //     card.Body.Add(TextBlockStorName);
        //
        //     actionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Confirm", Data = new AdaptiveCardTaskFetchValue<string>() { Data = StoreData, SetType = "GetStore" } });
        //     card.Body.Add(actionSet);
        //     return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        // }

        public Attachment GetChooseMenuCard(string address, bool reserve)
        {
            var cardData = new CardDataModel<string>()
            {
                Type = reserve? "reserveStore" : "GetStore",
                Value = address
            };
            var card = NewAdaptiveCard()
                .AddElement(new AdaptiveTextBlock
                {
                    Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder, Text = address,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                })
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction().SetOpenTaskModule("Choose Menu",
                            JsonConvert.SerializeObject(cardData)))
                );
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        //此方法已被CreateStoresModule取代
        // private Attachment CreateClickStoreModule(string Jdata)
        // {
        //     var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
        //
        //     var TextBlockStorName = new AdaptiveTextBlock
        //     {
        //         Size = AdaptiveTextSize.Large,
        //         Weight = AdaptiveTextWeight.Bolder,
        //         Text = "Chose Your Order",
        //         HorizontalAlignment = AdaptiveHorizontalAlignment.Center
        //     };
        //     card.Body.Add(TextBlockStorName);
        //
        //     var root = JsonConvert.DeserializeObject<Store_List>(Jdata);
        //
        //     foreach (var s in root.Stores)
        //     {
        //         var ColumnSetitem = new AdaptiveColumnSet();
        //         StoreModule(ColumnSetitem, s.Store_Name, s.Store_Url);
        //         card.Body.Add(ColumnSetitem);
        //     }
        //
        //     //選擇時間
        //     var InputTime = new AdaptiveTimeInput();
        //     InputTime.Id = "DueTime";
        //     card.Body.Add(InputTime);
        //
        //     card.Actions = new[] { TaskModuleUIConstants.AdaptiveCard }
        //            .Select(cardType => new AdaptiveSubmitAction() { Title = "Submit", Data = new AdaptiveCardTaskFetchValue<string>() { SetType = "ResultStoreCard" } })
        //             .ToList<AdaptiveAction>();
        //     return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        // }
        
        public async Task<Attachment> CreateStoresModule(AdaptiveCardDataFactory factory, string reserve)
        {
            var address = factory.GetCardData<string>();
            var LatLng = GetLatLng(address);
            var storesInfo = await new WebCrawler().GetStores2(LatLng.lat, LatLng.lng);
            var cardData = new CardDataModel<List<Store>>();
            if (reserve == null) cardData.Type = "ResultStoreCard";
            else cardData.Type = reserve;
            
            var card =
                NewAdaptiveCard()
                    .AddElement(new AdaptiveTextBlock
                    {
                        Size = AdaptiveTextSize.Large, Weight = AdaptiveTextWeight.Bolder, Text = "Chose Your Order",
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                    });

            foreach (var store in storesInfo)
            {
                card
                    .AddRow(new AdaptiveColumnSet()
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() {Text = store.Store_Name, Id = "StoreName"}))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveTextBlock() {Text = store.Store_Url, Id = "Url"}))
                        .AddCol(new AdaptiveColumn()
                            .AddElement(new AdaptiveToggleInput() {Id = store.Store_Name + "&&" + store.Store_Url, Title = "Confirm"})));
            }

            card
                .AddElement(new AdaptiveTextInput() {Id = "DueTime"})
                .AddActionsSet(
                    NewActionsSet()
                        .AddActionToSet(new AdaptiveSubmitAction().SetSubmitTaskModule("Submit", JsonConvert.SerializeObject(cardData))));
            
            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        private static void SetTaskInfo(TaskModuleTaskInfo taskInfo, UISettings uIConstants)
        {
            taskInfo.Height = uIConstants.Height;
            taskInfo.Width = uIConstants.Width;
            taskInfo.Title = uIConstants.Title.ToString();
        }
    }
}
