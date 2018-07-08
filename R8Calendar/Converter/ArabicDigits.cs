using System.Text;

namespace R8Calendar.Converter
{
    public static class ArabicDigits
    {
        public static string Convert(string digits)
        {
            var result = new StringBuilder();
            var arab = new[] { "٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩" };

            foreach (var digit in digits.ToCharArray())
                if (char.IsDigit(digit))
                    result.Append(arab[int.Parse(digit.ToString())]);

            return result.ToString();
        }
    }
}