using R8Calendar.Models;
using System;
using System.Globalization;

namespace R8Calendar.Converter
{
    public class HijriConverter
    {
        public static DateItemModel HijriToGregorian(int year, int month, int day)
        {
            var obj = H2GAdjusted(year, month, day);
            return new DateItemModel
            {
                Month = obj.Month,
                DayOfMonth = obj.Day,
                DayOfWeek = DayOfWeekConverter.Persian(H2GAdjusted(year, month, day).DayOfWeek).GetDisplay(),
                Year = obj.Year,
                MonthName = MonthsName.Gregorian(obj.Month)
            };
        }

        public static DateItemModel GregorianToHijri(int year, int month, int day)
        {
            var dt = G2HAdjusted(year, month, day);
            var dt2 = new DateTime(year, month, day);

            var pcal = new HijriCalendar();
            var _year = pcal.GetYear(dt);
            var _month = pcal.GetMonth(dt);
            var _day = pcal.GetDayOfMonth(dt);
            var dayOfWeek = pcal.GetDayOfWeek(dt2);

            return new DateItemModel
            {
                Month = _month,
                DayOfMonth = _day,
                DayOfWeek = DayOfWeekConverter.Hijri(dayOfWeek),
                Year = _year,
                MonthName = MonthsName.Hijri(_month)
            };
        }

        private static DateTime H2G(int year, int month, int day)
        {
            var hijri = new HijriCalendar();
            return hijri.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        private static DateTime G2HAdjusted(int year, int month, int day)
        {
            return new DateTime(year, month, day).AddDays(HijriAdjust);
        }

        private static DateTime H2GAdjusted(int year, int month, int day)
        {
            var hijri = new HijriCalendar
            {
                HijriAdjustment = HijriAdjust
            };
            return hijri.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        private static int HijriAdjust => -2;
    }
}