using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ionic.Zip;
using Newtonsoft.Json;
using Scheduler.Exceptions;
using Scheduler.Models;

namespace Scheduler.BusinessLogic
{
    /// <summary>
    ///     Class responsable to manage config of jobs
    /// </summary>
    public class JobLogic : LogicBase
    {
        public List<Job> appoggio = new List<Job>();

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

        #region SaveJobConfig

        /// <summary>
        ///     Save Jobs json config
        /// </summary>
        /// <returns></returns>
        public void SaveJobConfig()
        {
            try
            {
                var path = ConfigurationSettings.AppSettings["PathJobsConfig"];
                if (!IsValidConfigPath(path)) throw new PathNotFoundException(path);

                var result = JsonConvert.SerializeObject(appoggio);
                File.WriteAllText(path, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion SaveJobConfig

        #region RemoveJobConfig

        /// <summary>
        ///     Remove a Job and save in json config
        /// </summary>
        /// <param name="jobName">string</param>
        public void RemoveJobConfig(string jobName)
        {
            try
            {
                appoggio.RemoveAll(x => x.name == jobName);
                SaveJobConfig();

                var path_to_delete = $"CustomJobs/{jobName}";
                Directory.Delete(path_to_delete, true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion RemoveJobConfig

        #region LoadGrid

        /// <summary>
        ///     Load jobs in grid view
        /// </summary>
        /// <param name="view">DataGridView</param>
        public void LoadGrid(DataGridView view)
        {
            try
            {
                view.DataSource = GetJobs();
            }
            catch (PathNotFoundException ex)
            {
                MessageBox.Show("[Jobs Config] " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion LoadGrid

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
                MessageBox.Show("[Jobs Config] " + ex.Message);
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
                var path = "CustomJobs";
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

        #region ExtractZipJob

        /// <summary>
        ///     Extract the zip files for jobs
        /// </summary>
        /// <param name="path"></param>
        public void ExtractZipJob(string path, string job_name)
        {
            using (var zip = ZipFile.Read(path))
            {
                foreach (var e in zip)
                    e.Extract($"CustomJobs/{job_name}",
                        ExtractExistingFileAction.OverwriteSilently);
            }
        }

        #endregion ExtractZipJob
    }
}