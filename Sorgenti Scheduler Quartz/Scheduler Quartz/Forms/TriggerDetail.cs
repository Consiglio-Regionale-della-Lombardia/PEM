using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Scheduler.BusinessLogic;
using Scheduler.Models;

namespace Scheduler.Forms
{
    public partial class TriggerDetail : Form
    {
        private List<Job> _jobs = new List<Job>();
        private TriggerLogic _tl;

        public TriggerDetail(Trigger t, List<Job> jobs, TriggerLogic tl)
        {
            InitializeComponent();

            _jobs = jobs;
            _tl = tl;

            dtpStartTime.Format = DateTimePickerFormat.Custom;
            dtpStartTime.CustomFormat = "dd/MM/yyyy HH:mm";

            _tl.LoadComboScheduleType(cmbScheduleType);
            cmbScheduleType.ValueMember = "ScheduleType";
            cmbScheduleType.DisplayMember = "Name";

            cmbJobs.DataSource = jobs;
            cmbJobs.DisplayMember = "Name";
            cmbJobs.ValueMember = "Name";

            if (t != null)
            {
                txtName.Text = t.name;
                cmbJobs.SelectedValue = t.jobname;
                switch (t.ScheduleType)
                {
                    case Enum.ScheduleTypeEnum.RegularIntervals:
                        cmbScheduleType.SelectedIndex = 0;
                        break;
                    case Enum.ScheduleTypeEnum.Daily:
                        cmbScheduleType.SelectedIndex = 1;
                        break;
                    case Enum.ScheduleTypeEnum.Weekly:
                        cmbScheduleType.SelectedIndex = 2;
                        break;
                    case Enum.ScheduleTypeEnum.Monthly:
                        cmbScheduleType.SelectedIndex = 3;
                        break;
                    case Enum.ScheduleTypeEnum.Yearly:
                        cmbScheduleType.SelectedIndex = 4;
                        break;
                    default:
                        break;
                }
                dtpStartTime.Value = t.StartTime;
                txtIntervalTime.Text = t.IntervalTime.ToString();
                chkSunday.Checked = t.Sunday;
                chkMonday.Checked = t.Monday;
                chkTuesday.Checked = t.Tuesday;
                chkWednesday.Checked = t.Wednesday;
                chkThursday.Checked = t.Thursday;
                chkFriday.Checked = t.Friday;
                chkSaturday.Checked = t.Saturday;

                btnSave.Text = "Modifica";
            }
            else
            {
                txtName.Text = "";
                dtpStartTime.Value = DateTime.Now.AddHours(1);

                btnSave.Text = "Inserisci";
                btnDelete.Visible = false;
                CancelButton = btnDelete;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtName.Text))
                    throw new Exception("Il nome è un campo obbligatorio");

                if (btnSave.Text != "Modifica")
                {
                    var duplicate = _tl.appoggio.FirstOrDefault(x => x.name == txtName.Text);
                    if (duplicate != null)
                        throw new Exception($"Il nome dell'evento {txtName.Text} esiste già!");
                }

                if (cmbJobs.SelectedValue.ToString() == "")
                {
                    throw new Exception("Il lavoro è un campo obbligatorio");
                }

                ScheduleTypeItem selectedType = (ScheduleTypeItem)cmbScheduleType.SelectedItem;
                var cronExpression = string.IsNullOrEmpty(txtCustomExpression.Text)
                    ? _tl.GetCronExpression(selectedType.ScheduleType, dtpStartTime.Value,
                        Convert.ToInt32(txtIntervalTime.Text),
                        chkMonday.Checked, chkTuesday.Checked, chkWednesday.Checked, chkThursday.Checked,
                        chkFriday.Checked, chkSaturday.Checked, chkSunday.Checked)
                    : txtCustomExpression.Text;


                Job job = (Job)cmbJobs.SelectedItem;

                int.TryParse(txtIntervalTime.Text, out int it);

                if (btnSave.Text != "Modifica")
                {
                    _tl.appoggio.Add(new Trigger
                    {
                        name = txtName.Text,
                        ScheduleType = selectedType.ScheduleType,
                        cronexpression = cronExpression,
                        jobname = job.name,
                        StartTime = dtpStartTime.Value,
                        IntervalTime = it,
                        Monday = chkMonday.Checked,
                        Tuesday = chkTuesday.Checked,
                        Wednesday = chkWednesday.Checked,
                        Thursday = chkThursday.Checked,
                        Friday = chkFriday.Checked,
                        Saturday = chkSaturday.Checked,
                        Sunday = chkSunday.Checked
                    });
                }
                else
                {
                    Trigger inmodifica = _tl.appoggio.FirstOrDefault(x => x.name == txtName.Text);
                    if (inmodifica != null)
                    {
                        inmodifica.cronexpression = cronExpression;
                        inmodifica.ScheduleType = selectedType.ScheduleType;
                        inmodifica.jobname = job.name;
                        inmodifica.StartTime = dtpStartTime.Value;
                        inmodifica.IntervalTime = it;
                        inmodifica.Monday = chkMonday.Checked;
                        inmodifica.Tuesday = chkTuesday.Checked;
                        inmodifica.Wednesday = chkWednesday.Checked;
                        inmodifica.Thursday = chkThursday.Checked;
                        inmodifica.Friday = chkFriday.Checked;
                        inmodifica.Saturday = chkSaturday.Checked;
                        inmodifica.Sunday = chkSunday.Checked;
                    }
                }

                _tl.SaveTriggerConfig();

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEscape_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Vuoi davvero cancellare questo evento ?", "Conferma Eliminazione", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _tl.RemoveTriggerConfig(txtName.Text);

                this.Close();
            }
        }
    }
}