using System.IO;

namespace Scheduler.BusinessLogic
{
    public abstract class LogicBase
    {
        protected bool IsValidConfigPath(string path)
        {
            return File.Exists(path);
        }
    }
}