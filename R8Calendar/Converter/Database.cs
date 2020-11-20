using Newtonsoft.Json.Linq;

using R8.Lib;

using R8Calendar.Models;

using RestSharp;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace R8Calendar.Converter
{
    public static class Database
    {
        private const string JsonFilename = "eventall.json";
        private const string JsonEndpoint = "http://tourismtime.ir/api/eventall";

        public static int GetMonthDays(int persianMonth)
        {
            return persianMonth <= 11 && persianMonth >= 7
                ? 30
                : (persianMonth >= 1 && persianMonth <= 6)
                    ? 31
                    : 29;
        }

        public static async void UpdateEvents()
        {
            var client = new RestClient(JsonEndpoint);
            var request = new RestRequest(Method.GET);

            var response = await client.ExecuteAsync(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Error in download json strings");

            var path = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + $"\\{JsonFilename}";
            var jsonText = response.Content;
            await File.WriteAllTextAsync(path, jsonText).ConfigureAwait(false);
        }

        public static ConcurrentDictionary<int, List<DayModel>> DeserializeJson(int persianYear, int persianMonth)
        {
            string json;
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), JsonFilename);
                using var reader = new StreamReader(path);
                json = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                json = "";
            }

            if (string.IsNullOrEmpty(json))
                return new ConcurrentDictionary<int, List<DayModel>>();
            var calendars = JToken.Parse(json);

            var monthResult = Init();
            foreach (var calendar in calendars)
            {
                var calendarName = calendar["name"].ToString();
                var calendarEvents = calendar["event"];

                var date = (from month in calendarEvents
                            select new MonthModel
                            {
                                MonthNo = Convert.ToInt32(((JProperty)month).Name),
                                Days = (from day in month.First
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
                if (date.Count <= 0)
                    continue;

                DateTime rangeFrom;
                DateTime rangeTo;
                CalendarTypes calendarType;

                switch (calendarName)
                {
                    case "jalali":
                        rangeFrom = new PersianDateTime(persianYear, persianMonth, 1).ToDateTime();
                        rangeTo = new PersianDateTime(persianYear, persianMonth, GetMonthDays(persianMonth)).ToDateTime();
                        calendarType = CalendarTypes.Persian;
                        break;

                    case "miladi":
                        rangeFrom = new PersianDateTime(persianYear, persianMonth, 1).ToDateTime();
                        rangeTo = new PersianDateTime(persianYear, persianMonth, GetMonthDays(persianMonth)).ToDateTime();
                        calendarType = CalendarTypes.Gregorian;
                        break;

                    case "hijri":
                    default:
                        rangeFrom = new PersianDateTime(persianYear, persianMonth, 1).ToDateTime();
                        rangeTo = new PersianDateTime(persianYear, persianMonth, GetMonthDays(persianMonth)).ToDateTime();
                        calendarType = CalendarTypes.Hijri;
                        break;
                }

                var ranges = Enumerable.Range(0, rangeTo.Subtract(rangeFrom).Days + 1)
                    .Select(d => rangeFrom.AddDays(d)).ToList();

                monthResult.UpdateLibrary(date, ranges, calendarType);
            }

            return monthResult;
        }

        private static void UpdateLibrary(this ConcurrentDictionary<int, List<DayModel>> monthResult,
            List<MonthModel> date, IEnumerable<DateTime> range, CalendarTypes calendarType)
        {
            foreach (var neededDay in range)
            {
                int currentMonth;
                int currentDay;
                int currentPersianMonth;
                int currentPersianDay;

                var persianate = PersianDateTime.GetFromDateTime(neededDay);

                switch (calendarType)
                {
                    case CalendarTypes.Persian:
                        currentMonth = persianate.Month;
                        currentDay = persianate.Day;
                        break;

                    case CalendarTypes.Hijri:
                        var hijri = new HijriCalendar();
                        // var currentHijriDate = Calendar.ConvertToIslamic(neededDay);
                        currentMonth = hijri.GetMonth(neededDay);
                        currentDay = hijri.GetDayOfMonth(neededDay);
                        break;

                    case CalendarTypes.Gregorian:
                    default:
                        currentMonth = neededDay.Month;
                        currentDay = neededDay.Day;
                        break;
                }

                if (calendarType != CalendarTypes.Persian)
                {
                    currentPersianMonth = persianate.Month;
                    currentPersianDay = persianate.Day;
                }
                else
                {
                    currentPersianMonth = currentMonth;
                    currentPersianDay = currentDay;
                }

                var gregorianMonthModel = date.Find(mnth => mnth.MonthNo == currentMonth);
                if (!(gregorianMonthModel?.Days.Count > 0)) continue;

                var day = gregorianMonthModel.Days.Find(x => x.DayOfMonth == currentDay);
                if (day == null) continue;

                var baseMonth = monthResult.FirstOrDefault(x => x.Key == currentPersianMonth);
                var baseDay = baseMonth.Value.Find(x => x.DayOfMonth == currentPersianDay);

                if (!baseDay.IsHoliday && day.IsHoliday)
                    baseDay.IsHoliday = true;

                baseDay.Events = day.Events?.Select(x => x).ToList();
            }
        }

        private static ConcurrentDictionary<int, List<DayModel>> Init()
        {
            var monthResult = new ConcurrentDictionary<int, List<DayModel>>();

            for (var monthIndex = 0; monthIndex < 12; monthIndex++)
            {
                var days = new List<DayModel>();

                var monthNo = monthIndex + 1;
                var daysCount = 0;

                if (monthNo >= 1 && monthNo <= 6)
                    daysCount = 31;
                else if (monthNo >= 7 && monthNo <= 11)
                    daysCount = 30;
                else
                    daysCount = 29;

                for (var dayIndex = 0; dayIndex < daysCount; dayIndex++)
                {
                    days.Add(new DayModel
                    {
                        DayOfMonth = dayIndex + 1
                    });
                }

                monthResult.TryAdd(monthNo, days);
            }

            return monthResult;
        }
    }
}