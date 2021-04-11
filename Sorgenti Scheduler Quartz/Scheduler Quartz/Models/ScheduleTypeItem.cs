using System.Collections.ObjectModel;
using Scheduler.Enum;

namespace Scheduler.Models
{
    public class ScheduleTypeItem
    {
        public ScheduleTypeEnum ScheduleType { get; set; }
        public string Descr { get; set; }
    }

    public class ScheduleTypeItems
    {
        public static ObservableCollection<ScheduleTypeItem> scheduleTypeList = new ObservableCollection<ScheduleTypeItem>
        {
            new ScheduleTypeItem{Descr="Regular intervals", ScheduleType=ScheduleTypeEnum.RegularIntervals},
            new ScheduleTypeItem{Descr="Daily", ScheduleType=ScheduleTypeEnum.Daily},
            new ScheduleTypeItem{Descr="Weekly", ScheduleType=ScheduleTypeEnum.Weekly},
            new ScheduleTypeItem{Descr="Monthly", ScheduleType=ScheduleTypeEnum.Monthly},
            new ScheduleTypeItem{Descr="Yearly", ScheduleType=ScheduleTypeEnum.Yearly}
        };
    }
}