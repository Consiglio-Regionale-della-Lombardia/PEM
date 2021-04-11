using System.Collections.Generic;

namespace Scheduler.Models
{
    public class Job
    {
        public string name { get; set; }
        public string path { get; set; }
        public string entrypoint { get; set; }
        public string scheduleclass { get; set; }
        public Dictionary<string, string> parameters { get; set; }
    }
}