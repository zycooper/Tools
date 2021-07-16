using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.ReOrderColumns
{
    public class ReOrderColumns
    {
        static public DataTable ReorderColumns(DataTable DT, string[] OrdCols, string TableName = "")
        {
            DataView view = new DataView(DT);

            if (string.IsNullOrEmpty(TableName))
            {
                return view.ToTable(false, OrdCols);
            }
            else
            {
                DataTable DT_Rename = view.ToTable(false, OrdCols);
                DT_Rename.TableName = TableName;
                return DT_Rename;
            }
        }
    }
}