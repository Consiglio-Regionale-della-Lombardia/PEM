using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GamScheduler.BusinessLogic;
using PortaleRegione.Logger;
using Quartz;
using Quartz.Impl;
using SchedulerService.BusinessLogic;
using SchedulerService.Models;

namespace SchedulerService
{
    public class ServiceWorker
    {
        private JobLogic _jobLogic;
        private TriggerLogic _triggerLogic;
        public List<Job> Jobs;
        private IScheduler scheduler;
        public List<Trigger> Triggers;

        public async Task Start()
        {
            Log.Initialize();
            _jobLogic = new JobLogic();
            _triggerLogic = new TriggerLogic();

            Jobs = _jobLogic.appoggio;
            Triggers = _triggerLogic.appoggio;

            // write code here that runs when the Windows Service starts up.  
            var props = new NameValueCollection
            {
                {"quartz.serializer.type", "binary"}
            };
            var factory = new StdSchedulerFactory(props);
            scheduler = await factory.GetScheduler();

            try
            {
                //clear all
                await scheduler.Clear();

                // and start it off
                if (!scheduler.IsStarted) await scheduler.Start();

                foreach (var itemTrigger in Triggers)
                {
                    var itemJob = Jobs.FirstOrDefault(x => x.name == itemTrigger.jobname);
                    if (itemJob == null) {
                        Log.Error($"Configurazione non trovata per il job [{itemTrigger.jobname}]");
                        continue;
                    }
                    var pathAssembly = Path.Combine(ConfigurationSettings.AppSettings["PathCustomJobs"], itemJob.path);
                    var info = new FileInfo(pathAssembly);
                    if (!info.Exists)
                    {
                        Log.Error($"Assembly non trovato [{itemJob.path}]");
                        continue;
                    }
                    var assembly =
                        Assembly.LoadFrom(pathAssembly);
                    var types = assembly.GetTypes();
                    Type type = null;
                    foreach (var t in types)
                    {
                        var interfaces = t.GetInterfaces();
                        if (interfaces.All(i => i != typeof(IJob))) continue;

                        type = t;
                    }

                    //carica i parametri del job
                    var dictionaryDataMap = new Dictionary<string, string>();
                    foreach (var entry in itemJob.parameters) dictionaryDataMap.Add(entry.Key, entry.Value);
                    var jobDataMap = new JobDataMap(dictionaryDataMap);

                    var job = JobBuilder.Create(type)
                        .WithIdentity(itemTrigger.jobname, "group1")
                        .SetJobData(jobDataMap)
                        .Build();

                    var trigger = TriggerBuilder.Create()
                        .WithIdentity(itemTrigger.name, "group1")
                        .StartNow()
                        .WithCronSchedule(itemTrigger.cronexpression)
                        .Build();
                    //Tell quartz to schedule the job using our trigger
                    await scheduler.ScheduleJob(job, trigger);
                }

                // some sleep to show what's happening
                await Task.Delay(TimeSpan.FromSeconds(60)); //60
            }
            catch (SchedulerException se)
            {
                Log.Error("SchedulerService", se);
            }
            catch (ReflectionTypeLoadException exRef)
            {
                Log.Error("SchedulerService", exRef);
            }
        }

        public async Task Stop()
        {
            // write code here that runs when the Windows Service stops.
            await scheduler.Shutdown();
        }
    }
}