using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class CustomizedOrder
    {
        public Guid CustomizedMenuId { get; set; }
        public string Name { get; set; }
        public string Mark { get; set; }

        public virtual CustomizedOrderDetail CustomizedMenu { get; set; }
    }
}
