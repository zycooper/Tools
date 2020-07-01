using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.NetWorkDays
{
    static class Tools
    {
         static public void NewVlookup(ref DataTable Main, DataTable Vlookup, string DT_Main_PrimaryCol, string DT_Vlookup_PrimaryCol = null, List<string> VlookupColList = null)
        {
            bool NoPrimaryinVlookupDT = false;
            //set the primary key in vlookup table if not the same with main table
            if (string.IsNullOrEmpty(DT_Vlookup_PrimaryCol))
            {
                DT_Vlookup_PrimaryCol = DT_Main_PrimaryCol;
                NoPrimaryinVlookupDT = true;
            }

            //if no VlookupColList pass in,vlookup all columns except primary kep
            if (VlookupColList == null)
            {
                if (NoPrimaryinVlookupDT)
                {
                    VlookupColList = Vlookup.Columns.Cast<DataColumn>().Where(r => r.ColumnName != DT_Vlookup_PrimaryCol).Select(r => r.ColumnName).ToList();
                }
                else
                {
                    VlookupColList = Vlookup.Columns.Cast<DataColumn>().Select(r => r.ColumnName).ToList();
                }
            }

            foreach (string Col in VlookupColList)
            {
                if (!Main.Columns.Contains(Col))
                {
                    Main.Columns.Add(Col);
                }
            }
            foreach (DataRow row in Main.Rows)
            {
                string t = "[" + DT_Vlookup_PrimaryCol + "] = '" + row[DT_Main_PrimaryCol].ToString() + "'";
                if (Vlookup.Select(t).Count() != 0)
                {
                    DataRow dr = Vlookup.Select(t)[0];
                    foreach (string Col in VlookupColList)
                    {
                        row[Col] = dr[Col].ToString();
                    }
                }
            }
        }
    }
}