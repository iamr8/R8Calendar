using System;
using System.ComponentModel.DataAnnotations;

namespace R8Calendar.Converter
{
    public static class DayOfWeekConverter
    {
        public enum PersianDayOfWeeks
        {
            [Display(Name = "شنبه")]
            Shanbeh = 0,

            [Display(Name = "یکشنبه")]
            YekShanbeh = 1,

            [Display(Name = "دوشنبه")]
            DoShanbeh = 2,

            [Display(Name = "سه شنبه")]
            SeShanbeh = 3,

            [Display(Name = "چهارشنبه")]
            ChaharShanbeh = 4,

            [Display(Name = "پنجشنبه")]
            PanjShanbeh = 5,

            [Display(Name = "جمعه")]
            Jomeh = 6
        }

        public static PersianDayOfWeeks Persian(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Friday:
                    return PersianDayOfWeeks.Jomeh;

                case DayOfWeek.Monday:
                    return PersianDayOfWeeks.DoShanbeh;

                case DayOfWeek.Sunday:
                    return PersianDayOfWeeks.YekShanbeh;

                case DayOfWeek.Thursday:
                    return PersianDayOfWeeks.PanjShanbeh;

                case DayOfWeek.Tuesday:
                    return PersianDayOfWeeks.SeShanbeh;

                case DayOfWeek.Wednesday:
                    return PersianDayOfWeeks.ChaharShanbeh;

                case DayOfWeek.Saturday:
                default:
                    return PersianDayOfWeeks.Shanbeh;
            }
        }

        public static string Hijri(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Friday:
                    return "الجمعة";

                case DayOfWeek.Monday:
                    return "الإثنين";

                case DayOfWeek.Sunday:
                    return "الأحَد";

                case DayOfWeek.Thursday:
                    return "الخميس";

                case DayOfWeek.Tuesday:
                    return "الثلاثاء	";

                case DayOfWeek.Wednesday:
                    return "الأربعاء	";

                case DayOfWeek.Saturday:
                default:
                    return "السبت";
            }
        }
    }
}