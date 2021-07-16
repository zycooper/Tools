using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS_Dev_Tool
{
    static public class GeneralTools
    {
        //more complex one, you can pass your own holiday list to it
        static public int NetWorkDays(DateTime From, DateTime To, bool CountWeekends, bool CountHoliday, List<DateTime> HolidayList)
        {
            //CountWeekends CountHoliday : if true, it will include weekends and holiday in the networkdays
            int Span = 0;
            int Holidays = 0;

            foreach (DateTime holiday in HolidayList)
            {
                if (holiday > From && holiday < To)
                {
                    Holidays++;
                }
            }
            DateTime FromDate = From;
            DateTime ToDate = To.Date.AddDays(1).AddHours(0).AddMinutes(0).AddSeconds(0);
            while (FromDate < ToDate)
            {
                if (CountWeekends)
                {
                    Span++;
                }
                else
                {
                    if (FromDate.DayOfWeek != DayOfWeek.Saturday && FromDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        Span++;
                    }
                }
                FromDate = FromDate.AddDays(1);
            }
            //according to the networkdays in Excel
            int NetWorkDays = Span - Holidays;

            if (CountHoliday)
            {
                return Span;
            }
            else
            {
                return NetWorkDays;
            }
        }
        //simple one, no holiday include, just configure do you want to exclude weekends or not
        static public int NetWorkDays(DateTime From, DateTime To, bool CountWeekends = false)
        {
            //CountWeekends CountHoliday : if true, it will include weekends and holiday in the networkdays
            int Span = 0;
            //int Holidays = 0;

            DateTime FromDate = From;
            DateTime ToDate = To.Date.AddDays(1).AddHours(0).AddMinutes(0).AddSeconds(0);
            while (FromDate < ToDate)
            {
                if (CountWeekends)
                {
                    Span++;
                }
                else
                {
                    if (FromDate.DayOfWeek != DayOfWeek.Saturday && FromDate.DayOfWeek != DayOfWeek.Sunday)
                    {
                        Span++;
                    }
                }
                FromDate = FromDate.AddDays(1);
            }

            return Span;
        }
        static public void Vlookup(DataTable Main, DataTable Vlookup, string DT_Main_PrimaryCol, string DT_Vlookup_PrimaryCol = null, List<string> VlookupColList = null, string blank_value = "_Blank")
        {
            bool NoPrimaryinVlookupDT = false;
            //set the primary key in vlookup table if not the same with main table
            if (string.IsNullOrEmpty(DT_Vlookup_PrimaryCol) || DT_Main_PrimaryCol == DT_Vlookup_PrimaryCol)
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
                else
                {
                    VlookupColList.ForEach(x => row[x] = blank_value);
                }
            }
        }
        /// <summary>
        /// Check if source contains all the values in param value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static public bool ContainsAnother<T>(this IEnumerable<T> source, IEnumerable<T> value) { return !value.Except(source).Any(); }
    }
}