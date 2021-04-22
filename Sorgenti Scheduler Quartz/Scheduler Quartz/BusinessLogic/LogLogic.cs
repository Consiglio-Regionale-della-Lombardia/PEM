using System;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using PortaleRegione.Gateway;
using Scheduler.Exceptions;

namespace Scheduler.BusinessLogic
{
    public class LogLogic :LogicBase
    {
        public LogLogic()
        {
            BaseGateway.apiUrl = ConfigurationManager.AppSettings["UrlApi"];

            Task.Run(async () =>
            {
                await Init();
            });
        }

        public async Task LoadGrid(DataGridView view)
        {
            try
            {
                view.DataSource = await Get();
            }
            catch (PathNotFoundException ex)
            {
                MessageBox.Show("[Log Config] " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task<DataTable> Get()
        {
            try
            {
                var dt = new DataTable();
                dt.Columns.Add("Id");
                dt.Columns.Add("Data");
                dt.Columns.Add("Messaggio");

                var result = await StampeGate.GetInfo();

                foreach (var item in result)
                {
                    var row = dt.NewRow();
                    row["Id"] = item.UIDStampa;
                    row["Data"] = item.Date.ToString("dd/MM/yyyy HH:mm:ss");
                    row["Messaggio"] = item.Message;

                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (PathNotFoundException ex)
            {
                MessageBox.Show("[Log Config] " + ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}