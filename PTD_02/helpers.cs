using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using PS = PTD_02.Properties.Settings;
using System.Configuration;
using System.Windows.Forms;
using System.IO;

namespace PTD_02
{
    public static class helpers
    {
        public static void applyUserProperties(DataTable userSettings)
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

        public static void readUserProperties(ref DataTable userSettings)
        {
            foreach (SettingsProperty property in PS.Default.Properties)
            {
                DataRow row = userSettings.NewRow();
                row[0] = property.Name;
                row[1] = PS.Default[property.Name].ToString();

                userSettings.Rows.Add(row);
            }
        }

        public static Tuple<List<string>,List<string>> parse2valueCSV(string path)
        {
            using (var reader = new StreamReader(path))
            {
                List<string> listA = new List<string>();
                List<string> listB = new List<string>();
                while (!reader.EndOfStream)
                {
                    var values = reader.ReadLine().Split(',');

                    listA.Add(values[0]);
                    listB.Add(values[1]);
                }
                return new Tuple<List<string>,List<string>>(listA, listB);
            }
            
        }
    }
}
