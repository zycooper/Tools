using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Dev_Tool
{
    static public class ConvertToHtml
    {
        static public string ConvertDataTableToHTML(DataTable dt_Data, DataTable dt_CSS = null, string noDataStr = "No data for this table.")
        {
            //dt_CSS
            //1. null, 
            //2. only tablename
            //3. full table, rows count == dt.rows.count + 1
            if (dt_Data.Rows.Count > 0)
            {
                string html = "";

                foreach (DataColumn col in dt_Data.Columns)
                {
                    if (DateTime.TryParse(col.ColumnName, out DateTime result))
                    {
                        col.ColumnName = Convert.ToDateTime(col.ColumnName).ToString("MM/dd/yy");
                    }
                }
                
                string Tbl_Style = "";
                if (dt_CSS == null)
                {
                    dt_CSS = dt_Data.Clone();

                    for (int i = 0; i < dt_Data.Rows.Count+1; i++)
                    {
                        dt_CSS.Rows.Add();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(dt_CSS.TableName))
                    {
                        Tbl_Style = dt_CSS.TableName;
                    }

                    if (dt_CSS.Rows.Count == 0)
                    {
                        dt_CSS = dt_Data.Clone();

                        for (int i = 0; i < dt_Data.Rows.Count+1; i++)
                        {
                            dt_CSS.Rows.Add();
                        }
                    }
                }

                html = string.IsNullOrEmpty(Tbl_Style) ? "<table>" : @"<table style=""" + Tbl_Style + @""">";

                //1.  only 1 column has value
                //1.1 this value is for whole row ad TR
                //1.2 this value is only for first column ad TD

                //2. more than one value, fill each by each/ ad TD
                //3. 0 value, no style at all no
                for (int i = 0; i < dt_Data.Rows.Count + 1; i++)
                {
                    bool onlyTrStyle = IsTRCss(dt_CSS.Rows[i].ItemArray);
                    html += onlyTrStyle ? @"<tr style = """ + dt_CSS.Rows[i].ItemArray[0].ToString() + @""">" : "<tr>";

                    for (int j = 0; j < dt_Data.Columns.Count; j++)
                    {
                        string cell_value = (i == 0) ? dt_Data.Columns[j].ColumnName : dt_Data.Rows[i - 1][j].ToString();

                        html += ((onlyTrStyle) ? "<td>" : (string.IsNullOrEmpty(dt_CSS.Rows[i][j].ToString()) ? "<td>" : @"<td style=""" + dt_CSS.Rows[i][j].ToString() + @""">")) + cell_value + @"</td>";
                    }
                    html += "</tr>";
                }

                html += @"</table>";

                return html;
            }
            else
            {
                return noDataStr;
            }
        }
        static private bool IsTRCss(object[] itemarray)
        {
            //if itemarray[0].ToString() start with ";", then false - not for whole row
            //true this is for whole row
            //false this is for only cell
            if (itemarray.Count() == 0)
            {
                return false;
            }

            if (itemarray[0].ToString().StartsWith(";"))
            {
                return false;
            }

            bool istrcss = !string.IsNullOrEmpty(itemarray[0].ToString());

            for (int i = 1; i < itemarray.Count(); i++)
            {
                if (!string.IsNullOrEmpty(itemarray[i].ToString()) && istrcss)
                {
                    return false;
                }
            }

            return istrcss;
        }
    }
}
