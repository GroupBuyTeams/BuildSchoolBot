using AdaptiveCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Service
{
    public class GetPayService
    {
        private void PayModule(AdaptiveColumnSet ColumnSetitem, string Url)
        {
            //ColumnSetitem.Separator = true;
            if(Url == null)
            {
                //輸入文字
                var input = new AdaptiveTextInput();
                ColumnSetitem.Columns.Add(AddColumn(input));
            }
            else
            {
                ColumnSetitem.Columns.Add(AddColumn(GetadaptiveTextBlock(Url)));
            }        
        }
        public AdaptiveTextBlock GetadaptiveTextBlock(string InputTxt)
        {
            var TextBlock = new AdaptiveTextBlock();
            TextBlock.Text = InputTxt;
            return TextBlock;
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
    }
}
