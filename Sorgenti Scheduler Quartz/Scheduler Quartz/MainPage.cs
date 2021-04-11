using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Windows.Forms;
using Scheduler.BusinessLogic;
using Scheduler.Forms;
using Scheduler.Models;

namespace Scheduler
{
    public partial class MainPage : Form
    {
        private ServiceController service = new ServiceController("SchedulerService");
        Timer timerStatusService = new Timer();
        private readonly JobLogic _jobLogic;
        private readonly TriggerLogic _triggerLogic;
        private bool _refreshGrid;
        private bool _running;

        private string GridTypeNow; //Jobs, Triggers, Logs

        public List<Job> Jobs;
        public List<Trigger> Triggers;

        public MainPage()
        {
            _jobLogic = new JobLogic();
            _triggerLogic = new TriggerLogic();

            InitializeComponent();

            Jobs = _jobLogic.appoggio;
            Triggers = _triggerLogic.appoggio;

            _triggerLogic.LoadGrid(dataGridView1, Jobs);

            GridTypeNow = "T";
            try
            {
                CheckStatus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void MainPage_Activated(object sender, EventArgs e)
        {
            if (_refreshGrid)
            {
                _refreshGrid = false;

                switch (GridTypeNow)
                {
                    case "J":
                        _jobLogic.LoadGrid(dataGridView1);
                        break;

                    case "T":
                        _triggerLogic.LoadGrid(dataGridView1, Jobs);
                        break;
                }
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var selectedName = dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[0].Value.ToString();

            if (selectedName != "")
                switch (GridTypeNow)
                {
                    case "J":
                        var selectedjob = _jobLogic.appoggio.FirstOrDefault(x => x.name == selectedName);
                        if (selectedjob != null)
                        {
                            var jd = new JobDetail(selectedjob, _jobLogic);
                            _refreshGrid = true;
                            jd.ShowDialog();
                        }

                        break;

                    case "T":
                        var selectedtrigger = _triggerLogic.appoggio.FirstOrDefault(x => x.name == selectedName);
                        if (selectedtrigger != null)
                        {
                            var td = new TriggerDetail(selectedtrigger, Jobs, _triggerLogic);
                            _refreshGrid = true;
                            td.ShowDialog();
                        }

                        break;
                }
        }

        private void toolStripButtonAdd_Click(object sender, EventArgs e)
        {
            switch (GridTypeNow)
            {
                case "J":
                    var job = new JobDetail(null, _jobLogic);
                    _refreshGrid = true;
                    job.ShowDialog();
                    break;

                case "T":
                    var trig = new TriggerDetail(null, Jobs, _triggerLogic);
                    _refreshGrid = true;
                    trig.ShowDialog();
                    break;
            }
        }

        private void toolStripButtonTriggers_Click(object sender, EventArgs e)
        {
            toolStripButtonTriggers.Checked = true;
            toolStripButtonJobs.Checked = false;
            _triggerLogic.LoadGrid(dataGridView1, Jobs);
            GridTypeNow = "T";
        }

        private void toolStripButtonJobs_Click(object sender, EventArgs e)
        {
            toolStripButtonTriggers.Checked = false;
            toolStripButtonJobs.Checked = true;
            _jobLogic.LoadGrid(dataGridView1);
            GridTypeNow = "J";
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            if (!_running)
                // trigger async evaluation
                RunService();
            else
                StopService();

            CheckStatus();
        }

        private void RunService()
        {
            try
            {
                var timeout = TimeSpan.FromMinutes(2);
                this.Text = "Scheduler - Pending...";
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void StopService()
        {
            try
            {
                var timeout = TimeSpan.FromMinutes(2);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception se)
            {
                MessageBox.Show(se.Message);
            }
        }

        private void CheckStatus()
        {
            switch (service.Status)
            {
                case ServiceControllerStatus.ContinuePending:
                    _running = false;
                    break;
                case ServiceControllerStatus.Paused:
                    _running = false;
                    break;
                case ServiceControllerStatus.PausePending:
                    _running = false;
                    break;
                case ServiceControllerStatus.Running:
                    _running = true;
                    break;
                case ServiceControllerStatus.StartPending:
                    _running = true;
                    break;
                case ServiceControllerStatus.Stopped:
                    _running = false;
                    break;
                case ServiceControllerStatus.StopPending:
                    _running = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.Text = $"Scheduler - {service.Status}";
            //cambiare l'immagine
            toolStripButtonStart.Image = _running ? imageList1.Images[0] : imageList1.Images[1];
            //cambia lo stato dei pulsanti
            toolStripButtonJobs.Enabled = !_running;
            toolStripButtonTriggers.Enabled = !_running;
            toolStripButtonAdd.Enabled = !_running;
        }
    }
}