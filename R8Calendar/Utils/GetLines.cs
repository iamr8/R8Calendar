using System.Windows.Controls;
using System.Windows.Documents;

namespace R8Calendar.Utils
{
    public static class Extras
    {
        public static int CountLines(this TextBlock textBlock)
        {
            var linesCount = 0;
            foreach (var line in textBlock.Inlines)
                if (line is LineBreak)
                    linesCount++;

            return linesCount;
        }
    }
}