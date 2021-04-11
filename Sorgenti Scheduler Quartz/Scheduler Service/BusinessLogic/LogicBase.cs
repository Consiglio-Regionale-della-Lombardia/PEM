using System.IO;

namespace SchedulerService.BusinessLogic
{
    public abstract class LogicBase
    {
        protected bool IsValidConfigPath(string path)
        {
            return File.Exists(path);
        }
    }
}