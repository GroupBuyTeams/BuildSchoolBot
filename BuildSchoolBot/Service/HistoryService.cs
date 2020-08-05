using AdaptiveCards;
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

namespace BuildSchoolBot.Service
{
    public class HistoryService
    {
        public static string connString;
        public SqlConnection conn;
        private readonly TeamsBuyContext _context;

        public HistoryService(TeamsBuyContext context, IConfiguration config)
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

        public List<Order> GetOrder(DateTime Start, DateTime End)
        {
            List<Order> orders;

            using (conn = new SqlConnection(connString))
            {
                string sql = @"select o.Date
                                from [Order] o
                                inner join [OrderDetail] od on o.OrderId = od.OrderId
                                WHERE o.Date BETWEEN '2020/08/01' AND '2020-08-04'";
                orders = conn.Query<Order>(sql).ToList();
            }

            return orders;
        }


        public Attachment CreateHistoryCard(DateTime Start,DateTime End, string Id)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Default,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Spacing = AdaptiveSpacing.Default,
                Text = Id
            });

            //get OrderDetails from db ==> IEnumerable < OrderDetail > orderDetails
            var getdate = GetOrder(Start,End);
            foreach (var detail in getdate)
            {
                card.Body.Add(appendHistoryDetail(detail.Date));
            }

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        private AdaptiveColumnSet appendHistoryDetail(DateTime Date)
        {
            var ColumnSet = new AdaptiveColumnSet() { Separator = true };

            var Column1 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Stretch };
            ColumnSet.Columns.Add(Column1);
            SetColumnDate(Column1, Date);

            var Column2 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Stretch };
            ColumnSet.Columns.Add(Column2);
            for (var i = 0; i < 5; i++)
            { 
                SetColumnContent(Column2, "text + pirce", AdaptiveTextSize.Medium);
            } 
            SetColumnContent(Column2, "Total: 100", AdaptiveTextSize.Medium);

            return ColumnSet;
        }

        private void SetColumnDate(AdaptiveColumn col, DateTime date)
        {
            var DateTime = new AdaptiveTextBlock()
            {
                Text = date.ToString("YYYY/MM/DD"),
                Height = AdaptiveHeight.Stretch,
                Size = AdaptiveTextSize.Large,
                Color = AdaptiveTextColor.Attention
            };
            col.Items.Add(DateTime);
        }

        private void SetColumnContent(AdaptiveColumn col, string text, AdaptiveTextSize size)
        {
            var Container = new AdaptiveContainer();
            var Total = new AdaptiveTextBlock() { Text = text, Size = size };
            Container.Items.Add(Total);
            col.Items.Add(Container);
        }
    }
}
