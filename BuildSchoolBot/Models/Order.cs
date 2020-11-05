using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderDetail = new HashSet<OrderDetail>();
        }

        public Guid OrderId { get; set; }
        public string ChannelId { get; set; }
        public DateTime Date { get; set; }
        public string StoreName { get; set; }

        public virtual ICollection<OrderDetail> OrderDetail { get; set; }
    }
}
