using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows.Forms;
using CronEspresso.NETCore;
using CronEspresso.NETCore.Enums;
using Newtonsoft.Json;
using Scheduler.Enum;
using Scheduler.Exceptions;
using Scheduler.Models;

namespace Scheduler.BusinessLogic
{
    public class TriggerLogic : LogicBase
    {
        public List<Trigger> appoggio;

        public TriggerLogic()
        {
            appoggio = GetTriggerConfig();
        }

        /// <summary>
        ///     Load schedule in grid view
        /// </summary>
        /// <param name="view">DataGridView</param>
        public void LoadGrid(DataGridView view, List<Job> Jobs)
        {
            try
            {
                view.DataSource = GetTrigger(Jobs);
            }
            catch (PathNotFoundException ex)
            {
                MessageBox.Show("[Trigger Config] " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                MessageBox.Show("[Trigger Config] " + ex.Message);
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

        /// <summary>
        ///     Load schedule type
        /// </summary>
        /// <param name="box">ComboBox</param>
        public void LoadComboScheduleType(ComboBox box)
        {
            box.DataSource = ScheduleTypeItems.scheduleTypeList;
        }

        #region GetCronExpression

        /// <summary>
        ///     method for calculate cronexpression
        /// </summary>
        /// <param name="typeEnum">ScheduleTypeEnum</param>
        /// <param name="time">DateTime</param>
        /// <param name="intervarl">int</param>
        /// <param name="monday">bool</param>
        /// <param name="tuesday">bool</param>
        /// <param name="wednesday">bool</param>
        /// <param name="thursday">bool</param>
        /// <param name="friday">bool</param>
        /// <param name="saturday">bool</param>
        /// <param name="sunday">bool</param>
        /// <returns>string</returns>
        public string GetCronExpression(ScheduleTypeEnum typeEnum, DateTime time, int intervarl, bool monday,
            bool tuesday, bool wednesday, bool thursday, bool friday, bool saturday, bool sunday)
        {
            var result = string.Empty;
            var runTime = new TimeSpan(time.Ticks);
            var dayofTheMonthToRunOn = time.Day;
            switch (typeEnum)
            {
                #region RegularIntervals

                case ScheduleTypeEnum.RegularIntervals:
                    return CronGenerator.GenerateMinutesCronExpression(TimeSpan.FromSeconds(intervarl).Minutes);

                #endregion RegularIntervals

                #region Daily

                case ScheduleTypeEnum.Daily:
                    var daysToRun = new List<DayOfWeek>();
                    if (monday)
                        daysToRun.Add(DayOfWeek.Monday);
                    if (tuesday)
                        daysToRun.Add(DayOfWeek.Tuesday);
                    if (wednesday)
                        daysToRun.Add(DayOfWeek.Wednesday);
                    if (thursday)
                        daysToRun.Add(DayOfWeek.Thursday);
                    if (friday)
                        daysToRun.Add(DayOfWeek.Friday);
                    if (saturday)
                        daysToRun.Add(DayOfWeek.Saturday);
                    if (sunday)
                        daysToRun.Add(DayOfWeek.Sunday);
                    return CronGenerator.GenerateMultiDayCronExpression(runTime, daysToRun);

                #endregion Daily

                #region Weekly

                case ScheduleTypeEnum.Weekly:
                    var dayOfWeek = time.DayOfWeek;
                    return CronGenerator.GenerateSetDayCronExpression(runTime, dayOfWeek);

                #endregion Weekly

                #region Monthly

                case ScheduleTypeEnum.Monthly:
                    var monthsToRunOn = time.Month;
                    return CronGenerator.GenerateMonthlyCronExpression(runTime, dayofTheMonthToRunOn, monthsToRunOn);

                #endregion Monthly

                #region Yearly

                case ScheduleTypeEnum.Yearly:
                    MonthOfYear monthToRunOn = (MonthOfYear) time.Month;
                    return CronGenerator.GenerateYearlyCronExpression(runTime, dayofTheMonthToRunOn, monthToRunOn);

                #endregion Yearly

                default:
                    break;
            }

            return result;
        }

        #endregion GetCronExpression

        #region SaveTriggerConfig

        /// <summary>
        ///     Save Trigger json config
        /// </summary>
        /// <returns></returns>
        public void SaveTriggerConfig()
        {
            try
            {
                var path = ConfigurationSettings.AppSettings["PathTriggerConfig"];
                if (!IsValidConfigPath(path)) throw new PathNotFoundException(path);

                var result = JsonConvert.SerializeObject(appoggio);
                File.WriteAllText(path, result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion SaveTriggerConfig

        #region RemoveTriggerConfig

        /// <summary>
        ///     Remove a Trigger and save in json config
        /// </summary>
        /// <param name="triggerName">string</param>
        public void RemoveTriggerConfig(string triggerName)
        {
            try
            {
                appoggio.RemoveAll(x => x.name == triggerName);
                SaveTriggerConfig();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion RemoveTriggerConfig
    }
}