using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class MenuDetail
    {
        public Guid MenuDetailId { get; set; }
        public string ProductName { get; set; }
        public decimal Amount { get; set; }
        public Guid MenuId { get; set; }

        public virtual MenuOrder Menu { get; set; }
    }
}
