using PTD_02.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using PS = PTD_02.Properties.Settings;
using System.Configuration;

namespace PTD_02
{
    public partial class UserSettings : Form
    {
        DataTable userSettingsTemp;
        Dictionary<string, string> descriptions;
        bool settingsChanged = false;
        bool invalidCell = false;

        public UserSettings()
        {
            InitializeComponent();

            dgv_userSettings.CellValueChanged += new DataGridViewCellEventHandler(gdv_usersettings_CellValueChanged);
            dgv_userSettings.CellEndEdit += new DataGridViewCellEventHandler(dgv_userSettings_CellEndEdit);
            dgv_userSettings.CellEnter += new DataGridViewCellEventHandler(dgv_userSettings_CellEnter);

            //create data from appliaction settings
            userSettingsTemp = new DataTable();
            userSettingsTemp.Columns.Add("field");
            userSettingsTemp.Columns.Add("value");
            readUserProperties(ref userSettingsTemp);

            //parse UserSettings desctription into Tuple 
            if (Resources.userSettingsDescriptions != null)
            {
                descriptions = splitCSVLinePairsFromString(Properties.Resources.userSettingsDescriptions, ',');
            }

            //format datagridview
            dgv_userSettings.DataSource = userSettingsTemp;
            dgv_userSettings.Columns["field"].ReadOnly = true;
            dgv_userSettings.Columns["value"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void btn_Save_Click(object sender, EventArgs e)
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
                applyUserProperties(userSettingsTemp);
                PS.Default.Save();
                lbl_status.Text = "Saved!";
            }
        }

        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void gdv_usersettings_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            settingsChanged = true;
            invalidCell = false;

            string settingName = dgv_userSettings.Rows[e.RowIndex].Cells[0].Value.ToString();
            string settingValue = dgv_userSettings.Rows[e.RowIndex].Cells[1].Value.ToString();

            switch (PS.Default[settingName])
            {
                case Double t2:
                    if (!Double.TryParse(settingValue, out t2))
                    {
                        dgv_userSettings.Rows[e.RowIndex].ErrorText = $"Value given for this value should be an Integer or decimal";
                        invalidCell = true;
                    }
                    break;

                case int t3:
                    if (!Int32.TryParse(settingValue, out t3))
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
            if (descriptions.ContainsKey(settingName))
            {
                tb_settingInfo.Text = descriptions[settingName];
            }
            else
            {
                tb_settingInfo.Text = "N/A";
            }
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

        private void applyUserProperties(DataTable userSettings)
        {
            foreach (DataRow row in userSettings.Rows)
            {
                switch (PS.Default[row[0].ToString()])
                {
                    case String t1:
                        PS.Default[row[0].ToString()] = row.Field<String>(1);
                        break;
                    case Double t2:
                        PS.Default[row[0].ToString()] = Double.Parse(row[1].ToString());
                        break;
                    case int t3:
                        PS.Default[row[0].ToString()] = (row.Field<int>(1));
                        break;
                }
            }
        }

        private void readUserProperties(ref DataTable userSettings)
        {
            foreach (SettingsProperty property in PS.Default.Properties)
            {
                DataRow row = userSettings.NewRow();
                row[0] = property.Name;
                row[1] = PS.Default[property.Name].ToString();

                userSettings.Rows.Add(row);
            }
        }

        public static Dictionary<string, string> splitCSVLinePairsFromString(string text, char delimeter)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            var lines = text.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length - 1; i++)
            {
                var values = lines[i].Split(delimeter);
                keyValues.Add(values[0], values[1]);
            }
            return keyValues;
        }

    }
}
