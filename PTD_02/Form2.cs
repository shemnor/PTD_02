using RenderData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PS = PTD_02.Properties.Settings;
using System.IO;
using System.Configuration;
using Tekla.Structures.InpParser;
using PTD_02.Properties;

namespace PTD_02
{
    public partial class Form2 : Form
    {
        DataTable userSettingsTemp;
        Tuple<List<string>, List<string>> descriptions;
        bool settingsChanged = false;
        bool invalidCell = false;

        public Form2()
        {
            InitializeComponent();

            dgv_userSettings.CellValueChanged += new DataGridViewCellEventHandler(gdv_usersettings_CellValueChanged);
            dgv_userSettings.CellEndEdit += new DataGridViewCellEventHandler(dgv_userSettings_CellEndEdit);
            dgv_userSettings.CellEnter += new DataGridViewCellEventHandler(dgv_userSettings_CellEnter);

            //create data from appliaction settings
            userSettingsTemp = new DataTable();
            userSettingsTemp.Columns.Add("field");
            userSettingsTemp.Columns.Add("value");
            helpers.readUserProperties(ref userSettingsTemp);

            //parse UserSettings desctription into Tuple 
            if (Resources.userSettingsDescriptions != null)
            {
                descriptions = helpers.splitCsvFromString(Properties.Resources.userSettingsDescriptions, ',');
             
            }

            //format datagridview
            dgv_userSettings.DataSource = userSettingsTemp;
            dgv_userSettings.Columns["field"].ReadOnly = true;
            dgv_userSettings.Columns["value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void gdv_usersettings_CellValueChanged (object sender, DataGridViewCellEventArgs e)
        {
            settingsChanged = true;
            invalidCell = false;

            string settingName = dgv_userSettings.Rows[e.RowIndex].Cells[0].Value.ToString();
            string settingValue = dgv_userSettings.Rows[e.RowIndex].Cells[1].Value.ToString();

            switch (PS.Default[settingName])
            {
                case Double t2:
                    if (!Double.TryParse(settingValue,out t2))
                    {
                        dgv_userSettings.Rows[e.RowIndex].ErrorText = $"Value given for this value should be an Integer or decimal";
                        invalidCell = true;
                    }
                    break;

                case int t3:
                    if(!Int32.TryParse(settingValue, out t3))
                    {
                        dgv_userSettings.Rows[e.RowIndex].ErrorText = $"Value given for this value should be an Integer";
                        invalidCell = true;
                    }
                    break;
            }
        }

        void dgv_userSettings_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            string settingName = dgv_userSettings.Rows[e.RowIndex].Cells[0].Value.ToString();
            string description = descriptions.Item2[descriptions.Item1.IndexOf(settingName)];
            tb_settingInfo.Text = description;
        }

        void dgv_userSettings_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!invalidCell)
            {
                // Clear the row error in case the user presses ESC.
                dgv_userSettings.Rows[e.RowIndex].ErrorText = String.Empty;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if changed then ask if discard
            if (settingsChanged)
            {
                DialogResult answer = MessageBox.Show(
                    "Some user settings were modified, but not saved. " +
                    "If you close this dialog, any changes will be lost." +
                    " Do you wish to discard changes?", "Unsaved changes!",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (answer == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (invalidCell)
            {
                DialogResult answer = MessageBox.Show(
                    "Settings cannot be saved untill all input errors are cleared." +
                    " Hover over the error icon to see more information.",
                    "Input Errors!",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                helpers.applyUserProperties(userSettingsTemp);
                PS.Default.Save();
                lbl_status.Text = "Saved!";
            }
        }
    }
}
