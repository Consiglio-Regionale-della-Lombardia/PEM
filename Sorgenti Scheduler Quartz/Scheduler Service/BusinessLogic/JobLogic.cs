using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SchedulerService.Exceptions;
using SchedulerService.Models;

namespace SchedulerService.BusinessLogic
{
    /// <summary>
    ///     Class responsable to manage config of jobs
    /// </summary>
    public class JobLogic : LogicBase
    {
        public List<Job> appoggio;

        /// <summary>
        ///     Ctor
        /// </summary>
        public JobLogic()
        {
            appoggio = GetJobConfig();
        }

        #region GetJobConfig

        /// <summary>
        ///     Get Jobs json config
        /// </summary>
        /// <returns>List<Job></returns>
        public List<Job> GetJobConfig()
        {
            try
            {
                var path = ConfigurationSettings.AppSettings["PathJobsConfig"];
                if (!IsValidConfigPath(path)) throw new PathNotFoundException(path);

                var lst = new List<Job>();

                try
                {
                    lst = JsonConvert.DeserializeObject<List<Job>>(File.ReadAllText(path));
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

        #endregion GetJobConfig

        #region GetJobs

        /// <summary>
        ///     Load jobs in a DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetJobs()
        {
            try
            {
                var lst = GetJobConfig();

                var dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("Parameters");

                foreach (var job in lst)
                {
                    var row = dt.NewRow();

                    row["Name"] = job.name;
                    row["Parameters"] = string.Join(";", job.parameters.Select(x => x.Key + "=" + x.Value));

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

        #endregion GetJobs

        #region GetCustomJobs

        /// <summary>
        ///     Get files from directory CustomJobs
        /// </summary>
        /// <returns>DataTable</returns>
        public DataTable GetCustomJobs()
        {
            try
            {
                var path = ConfigurationSettings.AppSettings["PathCustomJobs"];
                var files = Directory.GetFiles(path, "*.dll").ToList();

                var dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("Path");

                foreach (var file in files)
                {
                    var row = dt.NewRow();
                    row["Name"] = Path.GetFileNameWithoutExtension(file);
                    row["Path"] = Path.GetFullPath(file);

                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion GetCustomJobs
    }
}