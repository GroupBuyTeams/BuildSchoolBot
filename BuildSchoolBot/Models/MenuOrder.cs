using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class MenuOrder
    {
        public MenuOrder()
        {
            MenuDetail = new HashSet<MenuDetail>();
        }

        public Guid MenuId { get; set; }
        public string Store { get; set; }
        public string TeamsId { get; set; }

        public virtual ICollection<MenuDetail> MenuDetail { get; set; }
    }
}
