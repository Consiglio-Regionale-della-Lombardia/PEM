/*
 * Copyright (C) 2019 Consiglio Regionale della Lombardia
 * SPDX-License-Identifier: AGPL-3.0-or-later
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Threading.Tasks;
using Quartz;

namespace CleanLogRetention
{
    public class MainJob : IJob
    {
        public string connectionString { get; set; }
        public int retention { get; set; }
        public string tables { get; set; }
        public string pathReport { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            ConvertParameters(context.JobDetail.JobDataMap);

            var manager = new Manager(new ThreadWorkerModel
            {
                connectionString = connectionString,
                pathReport = pathReport,
                retention = retention,
                tables = tables
            });
            await manager.Run();
        }

        private void ConvertParameters(JobDataMap data)
        {
            if (data.ContainsKey(nameof(ThreadWorkerModel.connectionString)))
                connectionString = data.Get(nameof(ThreadWorkerModel.connectionString)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.retention)))
                retention = int.Parse(data.Get(nameof(ThreadWorkerModel.retention)).ToString());

            if (data.ContainsKey(nameof(ThreadWorkerModel.tables)))
                tables = data.Get(nameof(ThreadWorkerModel.tables)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.pathReport)))
                pathReport = data.Get(nameof(ThreadWorkerModel.pathReport)).ToString();
        }
    }
}