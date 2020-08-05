using AdaptiveCards;
using AdaptiveCards.Templating;
using BuildSchoolBot.Models;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class HistoryService
    {
        //public TeamsBuyContext context;
        
        //public IEnumerable<Order> GetOrder(string Start,string End)
        //{
        //    return context.Order.Where(x => x.Date.CompareTo(Start) >= 0 && x.Date.CompareTo(End) <= 0).ToList();
        //}


        public Attachment CreateHistoryCard(string Start,string End, string Id)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Weight = AdaptiveTextWeight.Default,
                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                Spacing = AdaptiveSpacing.Default,
                Text = Id
            });

            //var getdate = GetOrder(Start, End);
            //get OrderDetails from db ==> IEnumerable<OrderDetail> orderDetails
            //foreach(var detail in getdate) 
            //{
                card.Body.Add(appendHistoryDetail(Start));
            //}

            return new Attachment() { ContentType = AdaptiveCard.ContentType, Content = card };
        }

        private AdaptiveColumnSet appendHistoryDetail(string Date)
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

        private void SetColumnDate(AdaptiveColumn col, string date)
        {
            var DateTime = new AdaptiveTextBlock()
            {
                Text = date,
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
