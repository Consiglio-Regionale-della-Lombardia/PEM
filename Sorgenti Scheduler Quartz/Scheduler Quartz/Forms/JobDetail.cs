using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Ionic.Zip;
using Quartz;
using Scheduler.BusinessLogic;
using Scheduler.Models;

namespace Scheduler.Forms
{
    public partial class JobDetail : Form
    {
        private readonly bool _clone;
        private readonly JobLogic _jl;
        private readonly Job _job;
        private readonly DataTable dt = new DataTable();

        public JobDetail(Job j, JobLogic jl, bool clone = false)
        {
            InitializeComponent();

            _jl = jl;
            _clone = clone;
            _job = j;
            if (_clone) btnDelete.Visible = false;

            dt.Columns.Add("Key");
            dt.Columns.Add("Value");

            if (j != null)
            {
                txtName.Text = j.name;
                txtPath.Text = j.path;
                txtEntryPoint.Text = j.entrypoint;

                btnSave.Text = clone ? "Inserisci" : "Modifica";

                foreach (var entry in j.parameters)
                {
                    var row = dt.NewRow();
                    row["Key"] = entry.Key;
                    row["Value"] = entry.Value;
                    dt.Rows.Add(row);
                }
            }
            else
            {
                txtName.Text = "";
                txtPath.Text = "";
                txtEntryPoint.Text = "";
                btnSave.Text = "Inserisci";
                btnDelete.Visible = false;
            }

            dataGridView.DataSource = dt;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtName.Text))
                    throw new Exception("Il nome è un campo obbligatorio!");

                var dictionaryParameters = new Dictionary<string, string>();

                //se in inserimento verifica che il nome sia univoco
                if (btnSave.Text.ToLower() == "inserisci")
                {
                    var duplicate = _jl.appoggio.FirstOrDefault(x => x.name == txtName.Text);
                    if (duplicate != null)
                        throw new Exception($"Il nome del lavoro {txtName.Text} esiste già!");

                    if (!_clone)
                    {
                        //verifica che sia stato selezionato uno zip
                        if (string.IsNullOrEmpty(txtPath.Text))
                            throw new Exception("Occorre selezionare un file .Zip !");

                        //verifica che nello zip ci sia una cartella plugin
                        //verifica che ci sia un solo file fuori dalla cartella plugin
                        var trovatoEntryPoint = 0;
                        using (var zip = ZipFile.Read(txtPath.Text))
                        {
                            foreach (var z in zip)
                                if (z.FileName.ToUpper().Contains("JOB"))
                                {
                                    if (!Path.GetExtension(z.FileName).Contains("dll"))
                                        continue;
                                    trovatoEntryPoint++;
                                    txtEntryPoint.Text = z.FileName;
                                }
                        }

                        if (trovatoEntryPoint != 1)
                            throw new Exception("Nel file .Zip NON ESISTE un'unica .DLL denominata 'Job.dll'!");

                        //estrai tutto
                        _jl.ExtractZipJob(txtPath.Text, Path.GetFileNameWithoutExtension(txtEntryPoint.Text));
                    }

                    //ricava il nome della classe dall'entrypoint
                    var pathAssembly =
                        $"CustomJobs/{Path.GetFileNameWithoutExtension(txtEntryPoint.Text)}/{txtEntryPoint.Text}";
                    var assembly =
                        Assembly.LoadFrom(pathAssembly);
                    var types = assembly.GetTypes();
                    var job = new Job
                    {
                        name = txtName.Text,
                        path = pathAssembly,
                        entrypoint = txtEntryPoint.Text,
                        parameters = _clone ? _job.parameters : dictionaryParameters
                    };
                    foreach (var t in types)
                    {
                        var interfaces = t.GetInterfaces();
                        if (interfaces.All(i => i != typeof(IJob))) continue;
                        job.scheduleclass = t.FullName;
                        var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var prop in properties) dictionaryParameters.Add(prop.Name, "");
                    }

                    _jl.appoggio.Add(job);
                }
                else
                {
                    dictionaryParameters =
                        dt.AsEnumerable().ToDictionary(row => row[0].ToString(), row => row[1].ToString());

                    var inmodifica = _jl.appoggio.FirstOrDefault(x => x.name == _job.name);
                    if (inmodifica != null)
                    {
                        if (inmodifica.name != txtName.Text)
                            inmodifica.name = txtName.Text;
                        inmodifica.parameters = dictionaryParameters;
                    }
                }

                _jl.SaveJobConfig();

                Close();
            }
            catch (Exception ex)
            {
                if (ex is ReflectionTypeLoadException)
                {
                    var typeLoadException = ex as ReflectionTypeLoadException;
                    var loaderExceptions = typeLoadException.LoaderExceptions;

                    MessageBox.Show(loaderExceptions[0].Message);
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Vuoi davvero cancellare questo lavoro ?", "Conferma Eliminazione",
                MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _jl.RemoveJobConfig(txtName.Text);

                Close();
            }
        }

        private void btnSearchFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Zip files | *.zip";
            //dialog.Filter = "Dinamic Link Library | *.dll";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
                txtPath.Text = dialog.FileName; // get name of file
        }

        private void btnEscape_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}