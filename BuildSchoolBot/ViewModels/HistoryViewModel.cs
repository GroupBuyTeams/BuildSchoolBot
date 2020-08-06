using AngleSharp.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.ViewModels
{
    public class HistoryViewModel
    {
        public DateTime Date { get; set; }
        public string StoreName { get; set; }
        public string ProductName { get; set; }
        public int Number { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
    }


}
