using R8Calendar.Models;
using System;
using System.Globalization;

namespace R8Calendar.Converter
{
    public static class PersianConverter
    {
        public static DateItemModel PersianToGregorian(int year, int month, int day)
        {
            var obj = Convert(year, month, day);

            return new DateItemModel
            {
                Month = obj.Month,
                DayOfMonth = obj.Day,
                DayOfWeek = DayOfWeekConverter.Persian(obj.DayOfWeek).GetDisplay(),
                Year = obj.Year,
                MonthName = MonthsName.Gregorian(obj.Month)
            };
        }

        public static DateItemModel GregorianToPersian(int year, int month, int day)
        {
            var pcal = new PersianCalendar();
            var dt = new DateTime(year, month, day, 0, 0, 0, 0);
            var _year = pcal.GetYear(dt);
            var _month = pcal.GetMonth(dt);
            var _day = pcal.GetDayOfMonth(dt);
            var dayOfWeek = pcal.GetDayOfWeek(dt);

            return new DateItemModel
            {
                Month = _month,
                DayOfMonth = _day,
                DayOfWeek = DayOfWeekConverter.Persian(dayOfWeek).GetDisplay(),
                Year = _year,
                MonthName = MonthsName.Persian(_month)
            };
        }

        private static DateTime Convert(int year, int month, int day)
        {
            var pCal = new PersianCalendar();
            var _year = !year.ToString().StartsWith("13") ? 1300 + year : year;
            return new DateTime(_year, month, day, pCal);
        }
    }
}