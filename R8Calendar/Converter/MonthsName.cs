using System;
using System.Globalization;

namespace R8Calendar.Converter
{
    public static class MonthsName
    {
        public static string Hijri(int hijMonth)
        {
            switch (hijMonth)
            {
                case 1:
                    return "محرم";

                case 2:
                    return "صفر";

                case 3:
                    return "ربیع الاول";

                case 4:
                    return "ربیع الثانی";

                case 5:
                    return "جمادی الاولی";

                case 6:
                    return "جمادی الآخره";

                case 7:
                    return "رجب";

                case 8:
                    return "شعبان";

                case 9:
                    return "رمضان";

                case 10:
                    return "شوال";

                case 11:
                    return "ذوالقعده";

                case 12:
                default:
                    return "ذوالحجه";
            }
        }

        public static string Hijri(DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            var hijriCal = new HijriCalendar();
            var firstDayOfMonthHijri = hijriCal.GetMonth(firstDayOfMonth);
            var lastDayOfMonthHijri = hijriCal.GetMonth(lastDayOfMonth);
            return firstDayOfMonthHijri.Equals(lastDayOfMonthHijri)
                ? $"{MonthsName.Hijri(firstDayOfMonthHijri)} {ArabicDigits.Convert(hijriCal.GetYear(firstDayOfMonth).ToString())}"
                : $"{MonthsName.Hijri(firstDayOfMonthHijri)} — {MonthsName.Hijri(lastDayOfMonthHijri)} {ArabicDigits.Convert(hijriCal.GetYear(firstDayOfMonth).ToString())}";
        }

        public static string Gregorian(DateTime specificDate, DateTime firstDayOfMonth, DateTime lastDayOfMonth)
        {
            return firstDayOfMonth.Month.Equals(lastDayOfMonth.Month)
                ? $"{firstDayOfMonth:MMMM} {specificDate.Year}"
                : $"{firstDayOfMonth:MMMM} — {lastDayOfMonth:MMMM} {specificDate.Year}";
        }

        public static string Persian(int month)
        {
            switch (month)
            {
                case 1:
                    return "فروردین";

                case 2:
                    return "اردیبهشت";

                case 3:
                    return "خرداد";

                case 4:
                    return "تیر";

                case 5:
                    return "مرداد";

                case 6:
                    return "شهریور";

                case 7:
                    return "مهر";

                case 8:
                    return "آبان";

                case 9:
                    return "آذر";

                case 10:
                    return "دی";

                case 11:
                    return "بهمن";

                case 12:
                default:
                    return "اسفند";
            }
        }

        public static string Gregorian(int engMonth)
        {
            switch (engMonth)
            {
                case 1:
                    return "ژانویه";

                case 2:
                    return "فوریه";

                case 3:
                    return "مارچ";

                case 4:
                    return "آپریل";

                case 5:
                    return "می";

                case 6:
                    return "ژوئن";

                case 7:
                    return "جولای";

                case 8:
                    return "آگوست";

                case 9:
                    return "سپتامبر";

                case 10:
                    return "اکتبر";

                case 11:
                    return "نوامبر";

                case 12:
                default:
                    return "دسامبر";
            }
        }
    }
}