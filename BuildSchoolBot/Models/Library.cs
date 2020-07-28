using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class Library
    {
        public Guid LibraryId { get; set; }
        public string MenuUri { get; set; }
        public Guid MemberId { get; set; }
    }
}
