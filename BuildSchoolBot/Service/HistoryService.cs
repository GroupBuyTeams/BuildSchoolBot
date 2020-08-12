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
using BuildSchoolBot.ViewModels;

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

        public List<Order> GetOrderDate(DateTime Start, DateTime End,string Id)
        {
            List<Order> orders;

            using (conn = new SqlConnection(connString))
            {
                var datas = new { START = Start,END = End, MID = Id };
                string sql = @"select distinct o.date
                                from [Order] o
                                inner join OrderDetail od on od.orderid = o.orderid
                                where od.memberid = @MID
                                and o.Date BETWEEN @START AND @END";
                orders = conn.Query<Order>(sql,datas).ToList();
            }
            return orders;
        }

        public List<HistoryViewModel> GetOrder(DateTime date, string Id)
        {
            List<HistoryViewModel> orders;

            using (conn = new SqlConnection(connString))
            {
                var datas = new { DATE = date, MID = Id };
                string sql = @"select o.Date, o.StoreName,od.ProductName,od.Amount,od.Number,od.MemberId
                                from [Order] o
                                inner join OrderDetail od on od.orderid = o.orderid
                                where od.memberid = @MID
                                and o.Date = @DATE";
                orders = conn.Query<HistoryViewModel>(sql, datas).ToList();
            }
            return orders;
        }

        public Attachment CreateHistoryCard(DateTime Start, DateTime End, string Name,string Id)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Default,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Spacing = AdaptiveSpacing.Default,
                Text = Name
            });

            var getdate = GetOrderDate(Start, End,Id);
            foreach (var detail in getdate)
            {
                card.Body.Add(appendHistoryDetail(detail.Date,Id));
            }

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        private AdaptiveColumnSet appendHistoryDetail(DateTime Date,string Id)
        {
            var ColumnSet = new AdaptiveColumnSet() { Separator = true };

            var Column1 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Stretch };
            ColumnSet.Columns.Add(Column1);
            SetColumnDate(Column1, Date);

            var Column2 = new AdaptiveColumn() { Width = AdaptiveColumnWidth.Stretch };
            ColumnSet.Columns.Add(Column2);

            var getorder = GetOrder(Date, Id);
            foreach (var orderdetail in getorder)
            {
                SetColumnContent(Column2, orderdetail.StoreName, orderdetail.ProductName, orderdetail.Number, orderdetail.Amount, AdaptiveTextSize.Medium);
            }
            
            //SetColumnContent(Column2, "Total: 100", AdaptiveTextSize.Medium);

            return ColumnSet;
        }

        private void SetColumnDate(AdaptiveColumn col, DateTime date)
        {
            var DateTime = new AdaptiveTextBlock()
            {
                Text = date.ToString("yyyy/MM/dd"),
                Height = AdaptiveHeight.Stretch,
                Size = AdaptiveTextSize.Large,
                Color = AdaptiveTextColor.Attention
            };
            col.Items.Add(DateTime);
        }

        private void SetColumnContent(AdaptiveColumn col,string storename,string productname,int number,decimal amount, AdaptiveTextSize size)
        {
            var Container = new AdaptiveContainer();
            var Total = new AdaptiveTextBlock() { Text = $"{storename}-{productname} X {number}  ${decimal.Round(amount)}", Size = size };
            Container.Items.Add(Total);
            col.Items.Add(Container);
        }
    }
}
