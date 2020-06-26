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

namespace PTD_02
{
    public partial class Form2 : Form
    {
        DataTable userSettingsTemp;
        bool settingsSaved = false;
        bool settingsChanged = false;

        public Form2()
        {
            InitializeComponent();
            userSettingsTemp = new DataTable();
            userSettingsTemp.Columns.Add("field");
            userSettingsTemp.Columns.Add("value");
            userSettingsTemp.Columns.Add("description");
            helpers.readUserProperties(ref userSettingsTemp);


            //string csvPath = Path.GetDirectoryName(Application.ExecutablePath) + @"\userSettings.csv";
            Tuple<List<string>, List<string>> descriptions = helpers.splitCsvFromString(Properties.Resources.userSettings,',');
            //Tuple<List<string>, List<string>> descriptions = helpers.parse2valueCSV(csvPath);
            foreach (DataRow row in userSettingsTemp.Rows)
            {
                row[2] = descriptions.Item2[descriptions.Item1.IndexOf(row[0].ToString())];
            }

            dgv_userSettings.DataSource = userSettingsTemp;
        }
        
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            //check if not saved
            if (settingsSaved == false)
            {
                //check if anythig changed
                foreach (DataRow row in userSettingsTemp.Rows)
                {
                    if (PS.Default[row[0].ToString()].ToString() != row[1].ToString())
                    {
                        settingsChanged = true;
                        break;
                    }
                }
                //if changed then ask if discard
                if (settingsChanged)
                {
                    DialogResult answer = MessageBox.Show("Some user settings were modified, but not saved. " +
                        "If you close this dialog, any changes will be lost." +
                        " Do you wish to discard changes?", "Unsaved changes!",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (answer == DialogResult.No)
                    {
                        e.Cancel = true;
                        settingsChanged = false;
                    }
                }
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            helpers.applyUserProperties(userSettingsTemp);
            PS.Default.Save();
            lbl_status.Text = "Saved!";
        }
    }
}
