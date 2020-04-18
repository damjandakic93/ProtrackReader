using Protrack;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProtrackReader
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        DevicesDatabase database = null;

        private void btnImport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {
                string serial = ProtrackDetector.CheckForProtrack(drive);
                if (serial != null)
                {
                    List<string> files = ProtrackDetector.DetectJumpFiles(drive);
                    files.ForEach(file => database.ReadJumpData(file));
                }
            }

            DatabaseUpdated();

            Cursor = Cursors.Arrow;
        }

        private void btnVisualize_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex < 0)
            {
                return;
            }

            JumpData jump = database.Devices.First(d => d.SerialNumber == (string)comboBox1.SelectedItem).Jumps.First(j => j.JumpNumber.ToString() == (string)comboBox2.SelectedItem);
            
            VisualizationForm form = new VisualizationForm(jump);
            form.Show();
        }

        private void DatabaseUpdated()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(database.Devices.Select(device => device.SerialNumber).ToArray());
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            database = DevicesDatabase.FromFile() ?? new DevicesDatabase();
            DatabaseUpdated();            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(database.Devices.First(d => d.SerialNumber == (string)comboBox1.SelectedItem).Jumps.Select(j => j.JumpNumber.ToString()).ToArray());
                if (comboBox2.Items.Count > 0)
                {
                    comboBox2.SelectedIndex = comboBox2.Items.Count - 1;
                }
            }
        }
    }
}
