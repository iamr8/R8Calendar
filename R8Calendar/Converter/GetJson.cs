using Newtonsoft.Json.Linq;
using R8Calendar.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace R8Calendar.Converter
{
    public static class GetJson
    {
        public static List<MonthModel> OpenFile(DateTime targetDateTime)
        {
            string json;
            try
            {
                using (var reader = new StreamReader("eventall.json"))
                    json = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                json = "";
            }

            if (string.IsNullOrEmpty(json)) return new List<MonthModel>();
            var calendarToken = JToken.Parse(json);

            var monthResult = new List<MonthModel>();

            foreach (var calendar in calendarToken)
            {
                if (calendar["name"].ToString().Equals("jalali"))
                    monthResult = Resolver.ConvertToList(calendar);
                else if (calendar["name"].ToString().Equals("miladi"))
                    monthResult.GregorianOrHijri(calendar, true, targetDateTime);
                else if (calendar["name"].ToString().Equals("qamari"))
                    monthResult.GregorianOrHijri(calendar, false, targetDateTime);
            }

            return monthResult;
        }
    }
}