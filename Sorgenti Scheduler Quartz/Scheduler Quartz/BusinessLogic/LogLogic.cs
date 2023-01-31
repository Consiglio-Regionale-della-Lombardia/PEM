using PortaleRegione.Gateway;
using Scheduler.Exceptions;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Scheduler.BusinessLogic
{
    public class LogLogic : LogicBase
    {
        public LogLogic()
        {
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

                var apiGateway = new ApiGateway(currentServiceUser.jwt);
                var result = await apiGateway.Stampe.GetInfo();

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