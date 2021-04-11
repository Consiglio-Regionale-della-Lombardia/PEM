using System;
using SchedulerService.Enum;

namespace SchedulerService.Models
{
    public class Trigger
    {
        public string name { get; set; }
        public ScheduleTypeEnum ScheduleType { get; set; }
        public string jobname { get; set; }
        public string cronexpression { get; set; }
        public string timezone { get; set; }
        public int IntervalTime { get; set; }
        public bool Sunday { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public DateTime StartTime { get; set; }
    }
}