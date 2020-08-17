using AdaptiveCards;
using Microsoft.Bot.Schema;

namespace BuildSchoolBot.Service
{
    public static class CardAssemblyFactory
    {
        /// <summary>
        /// 新增一基本的AdaptiveCard
        /// </summary>
        /// <returns>基本的AdaptiveCard</returns>
        public static AdaptiveCard NewAdaptiveCard()
        {
            return new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
        }
        /// <summary>
        /// 為AdaptiveCard新增AdaptiveContainer
        /// </summary>
        /// <param name="card">要被加入元素的AdaptiveCard</param>
        /// <param name="container">即將加入的AdaptiveContainer</param>
        /// <returns>原先的AdaptiveCard</returns>
        public static AdaptiveCard AddContainer(this AdaptiveCard card, AdaptiveContainer container)
        {
            card.Body.Add(container);
            return card;
        }
        /// <summary>
        /// 為AdaptiveCard新增AdaptiveColumnSet
        /// </summary>
        /// <param name="card">要被加入元素的AdaptiveCard</param>
        /// <param name="row">即將加入的AdaptiveColumnSet</param>
        /// <returns>原先的AdaptiveCard</returns>
        public static AdaptiveCard AddRow(this AdaptiveCard card, AdaptiveColumnSet row)
        {
            card.Body.Add(row);
            return card;
        }
        /// <summary>
        /// 為AdaptiveCard新增AdaptiveElement
        /// </summary>
        /// <param name="card">要被加入元素的AdaptiveCard</param>
        /// <param name="cardElement">即將加入的AdaptiveElement</param>
        /// <typeparam name="T">AdaptiveElement: TextBlock, TextInput, NumberInput, etc</typeparam>
        /// <returns>原先的AdaptiveCard</returns>
        public static AdaptiveCard AddElement<T>(this AdaptiveCard card, T cardElement) where T : AdaptiveElement
        {
            card.Body.Add(cardElement);
            return card;
        }
        /// <summary>
        /// 為AdaptiveCard新增AdaptiveColumnSet
        /// </summary>
        /// <param name="container">要被加入元素的AdaptiveContainer</param>
        /// <param name="row">即將加入的AdaptiveColumnSet</param>
        /// <returns>原先的AdaptiveContainer</returns>
        public static AdaptiveContainer AddRow(this AdaptiveContainer container, AdaptiveColumnSet row)
        {
            container.Items.Add(row);
            return container;
        }
        /// <summary>
        /// 為AdaptiveColumnSet新增AdaptiveColumn
        /// </summary>
        /// <param name="row">要被加入元素的AdaptiveColumnSet</param>
        /// <param name="col">即將加入的AdaptiveColumn</param>
        /// <returns>原先的AdaptiveColumnSet</returns>
        public static AdaptiveColumnSet AddCol(this AdaptiveColumnSet row, AdaptiveColumn col)
        {
            row.Columns.Add(col);
            return row;
        }
        /// <summary>
        /// 為AdaptiveColumn新增AdaptiveElement
        /// </summary>
        /// <param name="col">要被加入元素的AdaptiveColumn</param>
        /// <param name="cardElement">即將加入的AdaptiveElement</param>
        /// <typeparam name="T">AdaptiveElement: TextBlock, TextInput, NumberInput, etc</typeparam>
        /// <returns>原先的AdaptiveColumn</returns>
        public static AdaptiveColumn AddElement<T>(this AdaptiveColumn col, T cardElement) where T : AdaptiveElement
        {
            col.Items.Add(cardElement);
            return col;
        }
        /// <summary>
        /// 為AdaptiveColumnSet加入多個包含文字的AdaptiveColumn
        /// </summary>
        /// <param name="row">要被加入元素的AdaptiveColumnSet</param>
        /// <param name="strings">要顯示個多個Columns中的文字</param>
        /// <returns>原先的AdaptiveColumnSet</returns>
        public static AdaptiveColumnSet AddColumnsWithStrings(this AdaptiveColumnSet row, string[] strings)
        {
            foreach (var text in strings)
                row.AddCol(new AdaptiveColumn().AddElement(new AdaptiveTextBlock(text)));
            
            return row;
        }

        //public static AdaptiveColumnSet FixedInputTextAdjustWidthColumn(this AdaptiveColumnSet row, string[] texts)
        //{
        //    for (int i = 0; i < texts.Length; i++)
        //    {
        //        if (texts[i] == "")
        //        {
        //            row.AddCol(new AdaptiveColumn().AddElement(new AdaptiveTextBlock(texts[i])));
        //        }
        //        else
        //        {
        //            row.AddCol(new AdaptiveColumn().AddElement(new AdaptiveTextInput() { Id = texts[i] + i.ToString(), Value = texts[i] }));
        //        }
        //    }
        //    return row;
        //}

        public static AdaptiveColumnSet FixedtextColumnLeftColor(this AdaptiveColumnSet row,string[] texts)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                if (i == 0 || i == 1)
                {
                    row.AddCol(new AdaptiveColumn().AddElement(new AdaptiveTextBlock() { Text= texts[i] ,Weight= AdaptiveTextWeight.Bolder ,Color= AdaptiveTextColor.Good }));
                }
                else
                {
                    row.AddCol(new AdaptiveColumn().AddElement(new AdaptiveTextBlock() { Text = texts[i] }));
                }
            }
            return row;
        }
        
        public static HeroCard NewHeroCard()
        {
            return new HeroCard();
        }
        public static HeroCard EditText(this HeroCard card, string text)
        {
            card.Text = text;
            return card;
        }
        public static HeroCard EditTitle(this HeroCard card, string title)
        {
            card.Title = title;
            return card;
        }
        
        public static HeroCard EditSubtitle(this HeroCard card, string subtitle)
        {
            card.Subtitle = subtitle;
            return card;
        }
    }
}