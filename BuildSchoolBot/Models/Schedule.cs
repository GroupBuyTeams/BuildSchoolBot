using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Models
{
    public partial class Schedule
    {
        public Guid ScheduleId { get; set; }
        public string MenuUri { get; set; }
        public Guid GroupId { get; set; }
        public int TriggerType { get; set; }
        public int TriggerTime { get; set; }
        public int Duration { get; set; }
        public int RepeatWeekdays { get; set; }
    }
}
