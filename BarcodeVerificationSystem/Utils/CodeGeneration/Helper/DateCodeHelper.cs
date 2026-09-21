using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenCode.Utils
{
    public static class DateCodeHelper
    {
        public static string GetCurrentYearTwoDigits()
        {
            return DateTime.Now.ToString("yy");
        }

        public static string GetCurrentMonthTwoDigits()
        {
            return DateTime.Now.ToString("MM");
        }

        public static string GetCurrentDayHourMinuteSecond()
        {
            return DateTime.Now.ToString("ddHHmmss");
        }

        public static string GetCurrentDayTwoDigits()
        {
            return DateTime.Now.ToString("dd");
        }

        public static string GetCurrentHourTwoDigits()
        {
            return DateTime.Now.ToString("HH");
        }
        public static string GetCurrentMinuteTwoDigits()
        {
            return DateTime.Now.ToString("mm");
        }
        public static string GetCurrentSecondTwoDigits()
        {
            return DateTime.Now.ToString("ss");
        }
    }
    }
