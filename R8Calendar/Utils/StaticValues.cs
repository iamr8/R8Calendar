using System.Windows.Media;

namespace R8Calendar.Utils
{
    public static class StaticValues
    {
        public static class WhiteTheme
        {
            public static readonly SolidColorBrush ThemeColor = Brushes.White;
            public const double ThemeBlurinessOpacity = 0.6;
            public const double ThemeBlurinessRatio = 0.3;
            public static readonly SolidColorBrush TodayForecolor = Brushes.Black;
            public static readonly SolidColorBrush ChangeMonth = Brushes.Gray;
            public static readonly SolidColorBrush Separator = new SolidColorBrush(new Color
            {
                A = 30,
                R = 0,
                G = 0,
                B = 0
            });
            public static readonly SolidColorBrush DayLeadColor = new SolidColorBrush(new Color
            {
                A = 255,
                R = 0,
                G = 0,
                B = 0
            });

            public static readonly SolidColorBrush PersianDayColor = Brushes.Black;
            public static readonly SolidColorBrush GregorianHijriColor = Brushes.Gray;

            public static readonly SolidColorBrush HasEventDayBackColor = new SolidColorBrush(new Color
            {
                A = 10,
                R = 0,
                G = 0,
                B = 0
            });
        }

        public static class DarkTheme
        {
            public static readonly SolidColorBrush ThemeColor = Brushes.Black;
            public const double ThemeBlurinessOpacity = 0.3;
            public const double ThemeBlurinessRatio = 0.6;
            public static readonly SolidColorBrush TodayForecolor = Brushes.White;
            public static readonly SolidColorBrush ChangeMonth = Brushes.White;
            public static readonly SolidColorBrush Separator = new SolidColorBrush(new Color
            {
                A = 30,
                R = 255,
                G = 255,
                B = 255
            });

            public static readonly SolidColorBrush DayLeadColor = new SolidColorBrush(new Color
            {
                A = 255,
                R = 255,
                G = 255,
                B = 255
            });

            public static readonly SolidColorBrush PersianDayColor = Brushes.White;
            public static readonly SolidColorBrush GregorianHijriColor = Brushes.Gray;

            public static readonly SolidColorBrush HasEventDayBackColor = new SolidColorBrush(new Color
            {
                A = 10,
                R = 255,
                G = 255,
                B = 255
            });
        }
    }
}