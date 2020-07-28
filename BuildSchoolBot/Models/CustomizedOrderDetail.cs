using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class CustomizedOrderDetail
    {
        public Guid CustomizedMenuId { get; set; }
        public string ProductName { get; set; }
        public decimal Money { get; set; }
    }
}
