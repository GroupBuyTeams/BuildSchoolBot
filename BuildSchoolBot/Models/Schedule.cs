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
        public DateTime TriggerTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RepeatWeekdays { get; set; }
    }
}
