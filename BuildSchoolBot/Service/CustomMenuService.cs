﻿using AdaptiveCards;
using AdaptiveCards.Templating;
using BuildSchoolBot.Models;
using Microsoft.Bot.Schema;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Dapper;
using BuildSchoolBot.ViewModels;

namespace BuildSchoolBot.Service
{
    public class CustomMenuService
    {
        public static string connString;
        public SqlConnection conn;
        private readonly TeamsBuyContext _context;

        public CustomMenuService(TeamsBuyContext context, IConfiguration config)
        {
            _context = context;

            if (string.IsNullOrEmpty(connString))
            {
                connString = config.GetConnectionString("TeamsBuyContext");
            }
            if (conn == null)
            {
                conn = new SqlConnection(connString);
            }
        }

        public List<MenuOrder> GetMenuOrders()
        {
            List<MenuOrder> menuorder;

            using (conn = new SqlConnection(connString))
            {
                string sql = @"select *
                               from MenuOrder";
                menuorder = conn.Query<MenuOrder>(sql).ToList();
            }
            return menuorder;
        }

        public Attachment CallCustomeCard()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            var ColumnSet = new AdaptiveColumnSet();
            card.Body.Add(ColumnSet);

            var Column1 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Stretch };
            ColumnSet.Columns.Add(Column1);

            var Column2 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Stretch };
            ColumnSet.Columns.Add(Column2);
            
            var ActionSet1 = new AdaptiveActionSet();
            ActionSet1.Actions.Add(new AdaptiveSubmitAction() { Title = "Create Store" });
            Column1.Items.Add(ActionSet1);


            var ActionSet2 = new AdaptiveActionSet();
            card.Actions.Add(new AdaptiveShowCardAction() { Title = "View Store", Card = StoreListAdaptiveCard() });
            Column2.Items.Add(ActionSet2);

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }
        

        private AdaptiveCard StoreListAdaptiveCard()
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            var getstore = GetMenuOrders();
            foreach (var storeitem in getstore)
            {
                card.Body.Add(StoreItems(storeitem.Store));
            }
            return card;
        }

        private AdaptiveColumnSet StoreItems(string Storename)
        {
            var MainColumnSet = new AdaptiveColumnSet();

            var Column1 = new AdaptiveColumn();
            MainColumnSet.Columns.Add(Column1);

            var StoreName = new AdaptiveTextBlock() { Text = Storename, Height = AdaptiveHeight.Stretch ,Weight = AdaptiveTextWeight.Bolder};
            Column1.Items.Add(StoreName);

            var Column2 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Auto };
            MainColumnSet.Columns.Add(Column2);

            var ChildColumnSet = new AdaptiveColumnSet();
            Column2.Items.Add(ChildColumnSet);

            var EditColumn = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Auto};
            ChildColumnSet.Columns.Add(EditColumn);

            var EditActionSet = new AdaptiveActionSet();
            EditActionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Edit"});
            EditColumn.Items.Add(EditActionSet);

            var DeleteColumn = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Auto };
            ChildColumnSet.Columns.Add(DeleteColumn);

            var DeleteActionSet = new AdaptiveActionSet();
            DeleteActionSet.Actions.Add(new AdaptiveSubmitAction() { Title = "Delete" });
            DeleteColumn.Items.Add(DeleteActionSet);

            return MainColumnSet;
        }
    }
}
