using Newtonsoft.Json.Linq;
using R8Calendar.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace R8Calendar.Converter
{
    public static class Resolver
    {
        public static List<MonthModel> ConvertToList(JToken calendarToken)
        {
            var monthToken = calendarToken["event"];
            return (from month in monthToken
                    let daysToken = month.First
                    let monthNo = Convert.ToInt32(((JProperty)month).Name)
                    select new MonthModel
                    {
                        MonthNo = monthNo,
                        Days = (from day in daysToken
                                let @event = day.First.First["event"].Value<string>()
                                let holiday = day.First.First["holiday"].Value<string>()
                                let dayNo = Convert.ToInt32(((JProperty)day).Name)
                                select new DayModel
                                {
                                    DayOfMonth = dayNo,
                                    Events = @event.Split(new[] { "\r\n", "/", "؛" }, StringSplitOptions.None)
                                        .Select(x => x.Trim()).ToList(),
                                    IsHoliday = holiday == "1"
                                }).ToList(),
                    }).ToList();
        }

        public static void GregorianOrHijri(this List<MonthModel> source, JToken calendarToken, bool isGregorianOrHijri, DateTime targetDatetime)
        {
            foreach (var sourceMonth in ConvertToList(calendarToken))
            {
                foreach (var sourceDay in sourceMonth.Days)
                {
                    int year;
                    int smonth;
                    int sday;
                    if (!isGregorianOrHijri)
                    {
                        var umAlQuraCalendar = new UmAlQuraCalendar();
                        if (umAlQuraCalendar.IsLeapYear(umAlQuraCalendar.GetYear(targetDatetime), umAlQuraCalendar.GetEra(targetDatetime)))
                            if (sourceMonth.MonthNo >= 2)
                                if (sourceDay.DayOfMonth == 30)
                                    sourceDay.DayOfMonth = 29;

                        var h2G = HijriConverter.HijriToGregorian(new HijriCalendar().GetYear(targetDatetime),
                            sourceMonth.MonthNo, sourceDay.DayOfMonth);
                        year = h2G.Year;
                        smonth = h2G.Month;
                        sday = h2G.DayOfMonth;

                        if (new DateTime(h2G.Year, h2G.Month, h2G.DayOfMonth).CompareTo(new DateTime(targetDatetime.Year, targetDatetime.Month, 1)) > 0)
                        {
                            var ff = false;
                            if (ff)
                            {
                            }
                        }
                    }
                    else
                    {
                        year = DateTime.Now.Year;
                        smonth = sourceMonth.MonthNo;
                        sday = sourceDay.DayOfMonth;
                    }

                    var conversion = PersianConverter.GregorianToPersian(year, smonth, sday);

                    var month = source.FirstOrDefault(x => x.MonthNo == conversion.Month);
                    if (month != null)
                    {
                        var day = month.Days.FirstOrDefault(x => x.DayOfMonth == conversion.DayOfMonth);
                        if (day != null)
                            foreach (var @event in sourceDay.Events)
                            {
                                day.Events.Add(@event);
                                if (!day.IsHoliday && sourceDay.IsHoliday)
                                    day.IsHoliday = true;
                            }
                        else
                            month.Days.Add(new DayModel
                            {
                                Events = sourceDay.Events,
                                DayOfMonth = conversion.DayOfMonth,
                                IsHoliday = sourceDay.IsHoliday,
                            });
                    }
                    else
                    {
                        var newMonth = new MonthModel
                        {
                            MonthNo = conversion.Month,
                        };

                        newMonth.Days.Add(new DayModel
                        {
                            Events = sourceDay.Events,
                            DayOfMonth = conversion.DayOfMonth,
                            IsHoliday = sourceDay.IsHoliday,
                        });
                        source.Add(newMonth);
                    }
                }
            }

            foreach (var month in source)
                month.Days = month.Days.OrderBy(x => x.DayOfMonth).ToList();

            source = source.OrderBy(x => x.MonthNo).ToList();
        }
    }
}