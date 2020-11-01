using System.Collections.Generic;
using System.Linq;

namespace R8Calendar.Models
{
    public class DayModel
    {
        public int DayOfMonth { get; set; }

        public List<string> Events { get; set; }
        public bool IsHoliday { get; set; }

        public override string ToString()
        {
            if (Events == null || !Events.Any())
                return base.ToString();

            return $"{string.Join(" • ", Events.ToArray())}";
        }
    }
}