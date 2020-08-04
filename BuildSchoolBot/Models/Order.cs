using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class Order
    {
        public Guid OrderId { get; set; }
        public string GroupId { get; set; }
        public DateTime Date { get; set; }
    }
}
