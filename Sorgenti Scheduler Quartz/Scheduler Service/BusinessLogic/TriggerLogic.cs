using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using SchedulerService.BusinessLogic;
using SchedulerService.Exceptions;
using SchedulerService.Models;
using Newtonsoft.Json;

namespace GamScheduler.BusinessLogic
{
    public class TriggerLogic : LogicBase
    {
        public List<Trigger> appoggio;

        public TriggerLogic()
        {
            appoggio = GetTriggerConfig();
        }

        /// <summary>
        ///     Load triggers in a DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetTrigger(List<Job> Jobs)
        {
            try
            {
                var dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("JobName");
                dt.Columns.Add("CronExpression");

                foreach (var item in appoggio)
                {
                    //var itemJob = Jobs.FirstOrDefault(x => x.scheduleclass == item.jobname);

                    var row = dt.NewRow();
                    row["Name"] = item.name;
                    row["JobName"] = item.jobname;
                    row["CronExpression"] = item.cronexpression;

                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (PathNotFoundException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///     Get config from xml
        /// </summary>
        /// <returns>Trigger</returns>
        public List<Trigger> GetTriggerConfig()
        {
            var lst = new List<Trigger>();

            try
            {
                var path = ConfigurationSettings.AppSettings["PathTriggerConfig"];
                if (!IsValidConfigPath(path))
                    throw new PathNotFoundException(path);
                try
                {
                    lst = JsonConvert.DeserializeObject<List<Trigger>>(File.ReadAllText(path));
                }
                catch (Exception)
                {
                }

                return lst;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}