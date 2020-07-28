using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class AdditionalDetail
    {
        public Guid AddId { get; set; }
        public string AddItem { get; set; }

        public virtual Additional Add { get; set; }
    }
}
