using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class OrderDetail
    {
        public Guid OrderId { get; set; }
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public int Number { get; set; }
        public string Mark { get; set; }
        public Guid Member { get; set; }
        public virtual Order Order { get; set; }
    }
}
