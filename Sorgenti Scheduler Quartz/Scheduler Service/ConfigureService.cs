using Topshelf;

namespace SchedulerService
{
    internal class ConfigureService
    {
        internal static void Configure()  
        {  
            HostFactory.Run(configure =>  
            {  
                configure.Service<ServiceWorker> (service =>  
                {  
                    service.ConstructUsing(s => new ServiceWorker());  
                    service.WhenStarted(s => s.Start().Wait());  
                    service.WhenStopped(s => s.Stop().Wait());  
                });  
                //Setup Account that window service use to run.  
                configure.RunAsLocalSystem();  
                configure.SetServiceName("SchedulerService");  
                configure.SetDisplayName("SchedulerService");  
                configure.SetDescription("PEM - Run custom jobs with quartz");  
            });  
        }  
    }
}