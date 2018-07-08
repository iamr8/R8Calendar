using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R8Calendar.Models
{
    public class DateItemModel
    {
        public string DayOfWeek { get; set; }
        public int DayOfMonth { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public int Year { get; set; }
    }
}
