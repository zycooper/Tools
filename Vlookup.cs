using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools.Vlookup
{
    public class Vlookup
    {
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
        static public int NetWorkDays(DateTime From, DateTime To, bool CountWeekends)
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
    }
}