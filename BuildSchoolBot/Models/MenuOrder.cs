using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class MenuOrder
    {
        public Guid MenuId { get; set; }
        public string Store { get; set; }
        public string TeamsId { get; set; }

        public virtual MenuDetail Menu { get; set; }
    }
}
