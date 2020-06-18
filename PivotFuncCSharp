    public enum PivotFuncName { Count, Sum, Avg, Min, Max }
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
    class Pivot
    {
        public DataTable GetPivotDataTable(DataTable Source_Tbl, string columnX, List<string> columnY_List, PivotFunc func = null, bool X_axis_asc = true, bool y_axis_asc = true)
        {
            PivotFunc _pivotfunc = (func == null) ? new PivotFunc() { funcname = PivotFuncName.Count } : func;

            //DataTable Source_Tbl_populated = new DataTable();
            if (_pivotfunc.funcname != PivotFuncName.Count)
            {
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

            DataTable Result = new DataTable();

            List<KeyValuePair<string, List<string>>> Y_axis_Pairs = new List<KeyValuePair<string, List<string>>>();
            //add distinct value into two keyvaluepair
            columnY_List.ForEach(y => Y_axis_Pairs.Add(new KeyValuePair<string, List<string>>(key: y, value: Source_Tbl.AsEnumerable().Select(r => r[y].ToString()).Distinct().ToList())));
            var X_axis_values = X_axis_asc ? Source_Tbl.AsEnumerable().Select(r => r[columnX]).Distinct().OrderBy(i => i).ToList() : Source_Tbl.AsEnumerable().Select(r => r[columnX]).Distinct().OrderByDescending(i => i).ToList();

            //add columns
            columnY_List.ForEach(y => Result.Columns.Add(y.ToString()));
            Result.Columns.Add("Total");
            X_axis_values.ForEach(x => Result.Columns.Add(x.ToString()));

            List<List<string>> pivot_list = new List<List<string>>();

            Axis_Value_List(Y_axis_Pairs, 0, ref pivot_list);

            foreach (List<string> item in pivot_list)
            {
                DataRow dr = Result.NewRow();

                for (int i = 0; i < item.Count; i++)
                {
                    dr[i] = item[i];
                }

                //total cell
                List<KeyValuePair<string, string>> Dic_Total_Cell = new List<KeyValuePair<string, string>>();
                for (int j = 0; j < columnY_List.Count; j++)
                {
                    Dic_Total_Cell.Add(new KeyValuePair<string, string>(key: columnY_List[j], value: item[j]));
                }
                DataRow[] rows_total = Source_Tbl.Select(Expression_Str(Dic_Total_Cell)).ToArray();

                if (rows_total.Count() != 0)
                {
                    //each cell
                    dr[item.Count] = CellValue(rows_total, _pivotfunc);
                }
                else
                {
                    dr[item.Count] = 0;
                }

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
                        dr[i + 3] = CellValue(rows, _pivotfunc);
                    }
                }

                Result.Rows.Add(dr);
            }

            DataRow row_total = Result.NewRow();
            row_total[columnY_List.Count] = CellValue(Source_Tbl.AsEnumerable().Select(r => r).ToArray(), _pivotfunc);

            //value cells
            for (int i = 0; i < X_axis_values.Count; i++)
            {
                List<KeyValuePair<string, string>> Dic_Cell = new List<KeyValuePair<string, string>>();

                //add x-axis
                Dic_Cell.Add(new KeyValuePair<string, string>(key: columnX, value: X_axis_values[i].ToString()));

                DataRow[] rows = Source_Tbl.Select(Expression_Str(Dic_Cell)).ToArray();

                if (rows.Count() != 0)
                {
                    //each cell
                    row_total[i + 3] = CellValue(rows, _pivotfunc);
                }
            }

            Result.Rows.Add(row_total);
            return Result;
        }
        private List<List<string>> Axis_Value_List(List<KeyValuePair<string, List<string>>> Y_axis_Pairs, int i, ref List<List<string>> _result)
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
        private List<List<string>> DeepCopy(List<List<string>> source)
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
        private string Expression_Str(List<KeyValuePair<string, string>> ConditionPair)
        {
            string expression = "";
            ConditionPair.ForEach(x => expression = (!string.IsNullOrEmpty(expression)) ? (expression + " AND " + x.Key + "='" + x.Value.Replace("'", "''") + "' ") : (" " + x.Key + "='" + x.Value.Replace("'", "''") + "'"));
            return expression;
        }
        private double CellValue(DataRow[] rows, PivotFunc _func)
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

                default:
                    return 0;
            }
        }
    }
