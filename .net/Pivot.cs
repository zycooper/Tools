using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SMS_Dev_Tool
{
    public enum PivotFuncName { Count, Sum, Avg, Min, Max, CountDistinct }
    public class PivotFunc
    {
        public PivotFunc(PivotFuncName _funcname, string _column)
        {
            this.funcname = _funcname;
            this.PivotFuncColumnName = _column;
        }
        public PivotFunc() { }
        public PivotFuncName funcname { get; set; }
        public string PivotFuncColumnName { get; set; }
    }
    public enum TotalColPosition { none, left, right}
    public enum TotalRowPosition { none, top, bottom }
    static public class Pivot
    {
        /// <summary>
        /// a function to return pivot data
        /// </summary>
        /// <param name="Source_Tbl">raw datasource, if null or no rows will return an empty datatable</param>
        /// <param name="columnX">The column which will show up in the axis X. could be null</param>
        /// <param name="columnY_List">The column(s) which will show up in the axis Y. could be null</param>
        /// <param name="TtlColPos">where the total column will show up. </param>
        /// <param name="TtlRowPos">where the total row will show up. </param>
        /// <param name="X_axis_asc">the way to sort x aixs value.</param>
        /// <param name="y_axis_asc">the way to sort y aixs value.</param>
        /// <param name="isPercentage">is the number a percentage of total.</param>
        /// <param name="func">pivot function.</param>
        /// <param name="valueIfNull">what value to replace null on the pivot col(s)</param>        
        /// <returns>an datatable with or without data</returns>
        static public DataTable GetPivotDataTable(
            DataTable Source_Tbl, 
            string columnX, 
            List<string> columnY_List, 
            TotalColPosition TtlColPos = TotalColPosition.none , 
            TotalRowPosition TtlRowPos = TotalRowPosition.none, 
            bool X_axis_asc = true, 
            bool y_axis_asc = true, 
            bool isPercentage = false, 
            PivotFunc func = null,
            string valueIfNull = "Null_Value")
        {
            //final result
            DataTable Result = new DataTable();

            if (Source_Tbl == null || Source_Tbl.Rows.Count ==0)
            {
                return Result;
            }
            else
            {
                //bool TotalonLeft = true,
                PivotFunc _pivotfunc = func ?? new PivotFunc() { funcname = PivotFuncName.Count };

                if (_pivotfunc.funcname != PivotFuncName.Count && _pivotfunc.funcname != PivotFuncName.CountDistinct)
                {
                    //set the column not readonly to enable next step over writing
                    Source_Tbl.Columns[_pivotfunc.PivotFuncColumnName].ReadOnly = false;

                    //populate calculate column value
                    foreach (DataRow row in Source_Tbl.Rows)
                    {
                        if (double.TryParse(row[_pivotfunc.PivotFuncColumnName].ToString(), out double result))
                        {
                            row[_pivotfunc.PivotFuncColumnName] = Double.Parse(row[_pivotfunc.PivotFuncColumnName].ToString());
                        }
                        else
                        {
                            row[_pivotfunc.PivotFuncColumnName] = 0;
                        }
                    }
                }

                //convert null value in pivot columns(both in X and Y) to the non-null value
                List<string> Pivot_col = new List<string>();

                if (!string.IsNullOrEmpty(columnX))
                {
                    Pivot_col.Add(columnX);
                }

                if (columnY_List.Count > 0)
                {
                    columnY_List.ForEach(x => Pivot_col.Add(x));
                }
                Pivot_col.ForEach(x => { Source_Tbl.Columns[x].ReadOnly = false; });

                foreach (DataRow row in Source_Tbl.Rows)
                {
                    Pivot_col.ForEach(x => {
                        if (string.IsNullOrEmpty(row[x].ToString()))
                        {
                            row[x] = valueIfNull;
                        }
                    });
                }

                List<KeyValuePair<string, List<string>>> Y_axis_Pairs = new List<KeyValuePair<string, List<string>>>();

                //add distinct value into two keyvaluepair
                if (y_axis_asc)
                {
                    columnY_List.ForEach(y => Y_axis_Pairs.Add(new KeyValuePair<string, List<string>>(key: y, value: Source_Tbl.AsEnumerable().Select(r => r[y].ToString()).Distinct().OrderBy(i => i).ToList())));
                }
                else
                {
                    columnY_List.ForEach(y => Y_axis_Pairs.Add(new KeyValuePair<string, List<string>>(key: y, value: Source_Tbl.AsEnumerable().Select(r => r[y].ToString()).Distinct().OrderByDescending(i => i).ToList())));
                }

                List<object> X_axis_values = new List<object>();

                if (!string.IsNullOrEmpty(columnX))
                {
                    X_axis_values = X_axis_asc ? Source_Tbl.AsEnumerable().Select(r => r[columnX]).Distinct().OrderBy(i => i).ToList() : Source_Tbl.AsEnumerable().Select(r => r[columnX]).Distinct().OrderByDescending(i => i).ToList();
                }

                //add columns - y axis values
                columnY_List.ForEach(y => Result.Columns.Add(y.ToString()));

                //total col position
                if (TtlColPos == TotalColPosition.none)
                {
                    //no total col
                    if (!string.IsNullOrEmpty(columnX))
                    { X_axis_values.ForEach(x => Result.Columns.Add(x.ToString())); }
                }
                else if (TtlColPos == TotalColPosition.left)
                {
                    //total col on the left add total first then add rest cols
                    Result.Columns.Add("Total");

                    if (!string.IsNullOrEmpty(columnX))
                    { X_axis_values.ForEach(x => Result.Columns.Add(x.ToString())); }
                }
                else if (TtlColPos == TotalColPosition.right)
                {
                    //total col on the right add rest cols then add total
                    if (!string.IsNullOrEmpty(columnX))
                    { X_axis_values.ForEach(x => Result.Columns.Add(x.ToString())); }

                    Result.Columns.Add("Total");
                }

                List<List<string>> pivot_list = new List<List<string>>();

                //populate y axis values
                Axis_Value_List(Y_axis_Pairs, 0, ref pivot_list);

                //total col index
                int TotalColIndex = columnY_List.Count;

                if (TtlColPos == TotalColPosition.right)
                {
                    TotalColIndex += X_axis_values.Count;
                }

                //normal rows
                foreach (List<string> item in pivot_list)
                {
                    //pivot_list only has y-axis values
                    List<KeyValuePair<string, string>> Dic_Cell_y = new List<KeyValuePair<string, string>>();
                    for (int j = 0; j < columnY_List.Count; j++)
                    {
                        Dic_Cell_y.Add(new KeyValuePair<string, string>(key: columnY_List[j], value: item[j]));
                    }
                    DataRow[] rows_exist = Source_Tbl.Select(Expression_Str(Dic_Cell_y)).ToArray();

                    //set up row
                    DataRow dr = Result.NewRow();

                    for (int i = 0; i < item.Count; i++)
                    {
                        //fill distinct y-axis pivot value on each line
                        dr[i] = item[i];
                    }

                    //total col cell
                    if (TtlColPos != TotalColPosition.none)
                    {
                        List<KeyValuePair<string, string>> Dic_Total_Cell = new List<KeyValuePair<string, string>>();

                        for (int j = 0; j < columnY_List.Count; j++)
                        {
                            Dic_Total_Cell.Add(new KeyValuePair<string, string>(key: columnY_List[j], value: item[j]));
                        }

                        DataRow[] rows_total = Source_Tbl.Select(Expression_Str(Dic_Total_Cell)).ToArray();

                        if (rows_total.Count() != 0)
                        {
                            //each cell
                            dr[TotalColIndex] = CellValue(rows_total, _pivotfunc);
                        }
                        else
                        {
                            dr[TotalColIndex] = 0;
                        }
                    }

                    //normal cell
                    if (X_axis_values.Count > 0)
                    {
                        //value cells
                        for (int i = 0; i < X_axis_values.Count; i++)
                        {
                            List<KeyValuePair<string, string>> Dic_Cell = new List<KeyValuePair<string, string>>();
                            //add y-axis 
                            for (int j = 0; j < columnY_List.Count; j++)
                            {
                                Dic_Cell.Add(new KeyValuePair<string, string>(key: columnY_List[j], value: item[j]));
                            }
                            //add x-axis
                            Dic_Cell.Add(new KeyValuePair<string, string>(key: columnX, value: X_axis_values[i].ToString()));

                            DataRow[] rows = Source_Tbl.Select(Expression_Str(Dic_Cell)).ToArray();

                            if (rows.Count() != 0)
                            {
                                //each cell
                                dr[i + columnY_List.Count + (TtlColPos == TotalColPosition.left ? 1 : 0)] = CellValue(rows, _pivotfunc);
                            }
                        }
                    }

                    //add row
                    //only have more than 0 pivot number show, used to have showEmptyItemInPivot here but no need anymore
                    if (rows_exist.Count() > 0)
                    {
                        Result.Rows.Add(dr);
                    }
                }

                //add total row
                if (TtlRowPos != TotalRowPosition.none || isPercentage)
                {
                    //add total row
                    DataRow row_total = Result.NewRow();
                    row_total[0] = "Total";
                    row_total[TotalColIndex] = CellValue(Source_Tbl.AsEnumerable().Select(r => r).ToArray(), _pivotfunc);

                    //total row value cells
                    for (int i = 0; i < X_axis_values.Count; i++)
                    {
                        List<KeyValuePair<string, string>> Dic_Cell = new List<KeyValuePair<string, string>>
                        {
                            //add x-axis
                            new KeyValuePair<string, string>(key: columnX, value: X_axis_values[i].ToString())
                        };

                        DataRow[] rows = Source_Tbl.Select(Expression_Str(Dic_Cell)).ToArray();

                        if (rows.Count() != 0)
                        {
                            //each cell
                            row_total[i + columnY_List.Count + (TtlColPos == TotalColPosition.left ? 1 : 0)] = CellValue(rows, _pivotfunc);
                        }
                    }

                    //add total row
                    if (TtlRowPos == TotalRowPosition.top)
                    {
                        Result.Rows.InsertAt(row_total, 0);
                    }
                    else if (TtlRowPos == TotalRowPosition.bottom)
                    {
                        Result.Rows.Add(row_total.ItemArray);
                    }

                    if (isPercentage)
                    {
                        DataTable Result_Per = Result.Clone();

                        //first line - col name                       
                        for (int i = 0; i < Result.Rows.Count; i++)
                        {
                            DataRow dr = Result_Per.NewRow();
                            //pivot value on y axis
                            for (int j = 0; j < columnY_List.Count; j++)
                            {
                                dr[j] = Result.Rows[i][j].ToString();
                            }
                            //values
                            for (int j = columnY_List.Count; j < Result_Per.Columns.Count; j++)
                            {
                                if (double.TryParse(Result.Rows[i][j].ToString(), out double result_1) && double.TryParse(row_total[j].ToString(), out double result_2))
                                {
                                    if (double.Parse(row_total[j].ToString()) != 0)
                                    {
                                        dr[j] = Math.Round((double.Parse(Result.Rows[i][j].ToString()) * 100 / double.Parse(row_total[j].ToString())), 2).ToString();
                                    }
                                    else
                                    {
                                        dr[j] = "NaN";
                                    }
                                }
                                else
                                {
                                    dr[j] = "NaN";
                                }
                            }
                            Result_Per.Rows.Add(dr);
                        }
                        return Result_Per;
                    }
                    else
                    {
                        return Result;
                    }
                }
                else
                {
                    return Result;
                }
            }                             
        }
        static private List<List<string>> Axis_Value_List(List<KeyValuePair<string, List<string>>> Y_axis_Pairs, int i, ref List<List<string>> _result)
        {
            if (i > Y_axis_Pairs.Count - 1)
            {
                foreach (var item in Y_axis_Pairs[Y_axis_Pairs.Count - 1].Value)
                {
                    _result.Add(new List<string>() { item });
                }
            }
            else
            {
                var sample = Axis_Value_List(Y_axis_Pairs, i + 1, ref _result);

                if (i > 0)
                {
                    _result = new List<List<string>>();
                    foreach (var item in Y_axis_Pairs[i - 1].Value)
                    {
                        var forloop = DeepCopy(sample);

                        foreach (List<string> list in forloop)
                        {
                            list.Insert(0, item);

                            _result.Add(list);
                        }
                    }
                }

            }

            return _result;
        }
        static private List<List<string>> DeepCopy(List<List<string>> source)
        {
            List<List<string>> result = new List<List<string>>();

            foreach (List<string> list in source)
            {
                List<string> _list = new List<string>();
                foreach (string item in list)
                {
                    _list.Add(item.ToString());
                }

                result.Add(_list);
            }

            return result;
        }
        static private string Expression_Str(List<KeyValuePair<string, string>> ConditionPair)
        {
            string expression = "";
            ConditionPair.ForEach(x => expression = (!string.IsNullOrEmpty(expression)) ? (expression + " AND " + x.Key + "='" + x.Value.Replace("'", "''") + "' ") : (" " + x.Key + "='" + x.Value.Replace("'", "''") + "'"));
            return expression;
        }
        static private double CellValue(DataRow[] rows, PivotFunc _func)
        {
            switch (_func.funcname)
            {
                case PivotFuncName.Count:

                    return rows.Count();

                case PivotFuncName.Sum:

                    return rows.Sum(r => Convert.ToDouble(r[_func.PivotFuncColumnName].ToString()));

                case PivotFuncName.Avg:

                    return rows.Average(r => Convert.ToDouble(r[_func.PivotFuncColumnName].ToString()));

                case PivotFuncName.Max:

                    return rows.Max(r => Convert.ToDouble(r[_func.PivotFuncColumnName].ToString()));

                case PivotFuncName.Min:

                    return rows.Min(r => Convert.ToDouble(r[_func.PivotFuncColumnName].ToString()));

                case PivotFuncName.CountDistinct:

                    return rows.Select(r => r[_func.PivotFuncColumnName]).Distinct().Count();

                default:
                    return 0;
            }
        }
        static public DataTable TransposeTable(DataTable dt)
        {
            DataTable dtNew = new DataTable();
            //adding columns    
            for (int i = 0; i <= dt.Rows.Count; i++)
            {
                dtNew.Columns.Add(i.ToString());
            }

            //Changing Column Captions: 
            dtNew.Columns[0].ColumnName = " ";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //For dateTime columns use like below
                if (DateTime.TryParse(dt.Rows[i].ItemArray[0].ToString(), out DateTime result))
                {
                    dtNew.Columns[i + 1].ColumnName = Convert.ToDateTime(dt.Rows[i].ItemArray[0].ToString()).ToString("MM/dd/yy");
                }
                else
                {
                    dtNew.Columns[i + 1].ColumnName = dt.Rows[i].ItemArray[0].ToString();
                }
                //Else just assign the ItermArry[0] to the columnName prooperty
            }

            //Adding Row Data
            for (int k = 1; k < dt.Columns.Count; k++)
            {
                DataRow r = dtNew.NewRow();
                r[0] = dt.Columns[k].ToString();
                for (int j = 1; j <= dt.Rows.Count; j++)
                    r[j] = dt.Rows[j - 1][k];
                dtNew.Rows.Add(r);
            }

            return dtNew;
        }
    }
}
