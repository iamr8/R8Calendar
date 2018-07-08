using System.Collections.Generic;

namespace R8Calendar.Models
{
    public class MonthModel
    {
        public int MonthNo { get; set; }
        public List<DayModel> Days { get; set; }
    }
}