using System.Collections.Generic;

namespace R8Calendar.Models
{
    public class DayModel
    {
        public int DayOfMonth { get; set; }

        public List<string> Events { get; set; }
        public bool IsHoliday { get; set; }
    }
}