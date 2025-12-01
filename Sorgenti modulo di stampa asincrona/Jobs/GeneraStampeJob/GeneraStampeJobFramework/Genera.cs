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

namespace GeneraStampeJobFramework
{
    public class Genera : IJob
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UrlApi { get; set; }
        public string UrlApi_Internal { get; set; }
        public string UrlClient { get; set; }
        public string ConnectionString { get; set; }
        public string NumMaxTentativi { get; set; }
        public string CartellaLavoroTemporanea { get; set; }
        public string CartellaLavoroStampe { get; set; }
        public string EmailFrom { get; set; }
        public string RootRepository { get; set; }
        public string PercorsoCompatibilitaDocumenti { get; set; }
        public string masterKey { get; set; }
        
        public async Task Execute(IJobExecutionContext context)
        {
            ConvertParameters(context.JobDetail.JobDataMap);

            //GET STAMPE
            var manager = new Manager(new ThreadWorkerModel
            {
                UrlAPI = UrlApi,
                UrlAPI_Internal = UrlApi_Internal,
                UrlCLIENT = UrlClient,
                Username = Username,
                Password = Password,
                EmailFrom = EmailFrom,
                NumMaxTentativi = NumMaxTentativi,
                RootRepository = RootRepository,
                CartellaLavoroStampe = CartellaLavoroStampe,
                CartellaLavoroTemporanea = CartellaLavoroTemporanea,
                PercorsoCompatibilitaDocumenti = PercorsoCompatibilitaDocumenti,
                ConnectionString = ConnectionString,
                masterKey = masterKey
            });
            await manager.Run();
        }

        private void ConvertParameters(JobDataMap data)
        {
            if (data.ContainsKey(nameof(ThreadWorkerModel.Username)))
                Username = data.Get(nameof(ThreadWorkerModel.Username)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.Password)))
                Password = data.Get(nameof(ThreadWorkerModel.Password)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.UrlAPI)))
                UrlApi = data.Get(nameof(ThreadWorkerModel.UrlAPI)).ToString();
            
            if (data.ContainsKey(nameof(ThreadWorkerModel.UrlAPI_Internal)))
                UrlApi = data.Get(nameof(ThreadWorkerModel.UrlAPI_Internal)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.UrlCLIENT)))
                UrlClient = data.Get(nameof(ThreadWorkerModel.UrlCLIENT)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.NumMaxTentativi)))
                NumMaxTentativi = data.Get(nameof(ThreadWorkerModel.NumMaxTentativi)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.CartellaLavoroTemporanea)))
                CartellaLavoroTemporanea = data.Get(nameof(ThreadWorkerModel.CartellaLavoroTemporanea)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.CartellaLavoroStampe)))
                CartellaLavoroStampe = data.Get(nameof(ThreadWorkerModel.CartellaLavoroStampe)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.EmailFrom)))
                EmailFrom = data.Get(nameof(ThreadWorkerModel.EmailFrom)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.RootRepository)))
                RootRepository = data.Get(nameof(ThreadWorkerModel.RootRepository)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.PercorsoCompatibilitaDocumenti)))
                PercorsoCompatibilitaDocumenti =
                    data.Get(nameof(ThreadWorkerModel.PercorsoCompatibilitaDocumenti)).ToString();

            if (data.ContainsKey(nameof(ThreadWorkerModel.ConnectionString)))
                ConnectionString = data.Get(nameof(ThreadWorkerModel.ConnectionString)).ToString();
            
            if (data.ContainsKey(nameof(ThreadWorkerModel.masterKey)))
                masterKey = data.Get(nameof(ThreadWorkerModel.masterKey)).ToString();
        }
    }
}