using Persia;
using R8Calendar.Converter;
using R8Calendar.Models;
using R8Calendar.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Calendar = Persia.Calendar;

namespace R8Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime NowDateTime { get; set; }
        private double EventOffOpacity => 0;
        private double EventOnOpacity => 1;
        private const double EventBackdropOpacity = 0.6;
        private TimeSpan EventAnimationDuration => TimeSpan.FromMilliseconds(150);
        private ConcurrentDictionary<int, List<DayModel>> Months { get; set; }

        // Theme
        private bool IsDark { get; set; }

        private string CurrentEventDay { get; set; }
        private string CurrentEventText { get; set; }

        #region Theme Details

        private double CurrentThemeThemeNoiseRatioInt { get; set; }
        private SolidColorBrush CurrentThemeThemeNoiseColor { get; set; }

        private double CurrentThemeThemeNoiseOpacity { get; set; }
        private SolidColorBrush CurrentThemeHasEventDayBackColor { get; set; }
        private SolidColorBrush CurrentThemeDayLeadForecolor { get; set; }
        private SolidColorBrush CurrentThemePersianDayForecolor { get; set; }
        private SolidColorBrush CurrentThemeGregHijrForecolor { get; set; }
        private SolidColorBrush CurrentThemeTodayForecolor { get; set; }
        private SolidColorBrush CurrentThemeSeparator { get; set; }
        private SolidColorBrush CurrentThemeChangeMonthForecolor { get; set; }

        #endregion Theme Details

        private List<Border> OpenedFrames = new List<Border>();
        private Thickness EventClosedPosition => new Thickness(0, ActualHeight, 0, 0);

        public MainWindow()
        {
            InitializeComponent();
            //var wi = SystemParameters.WorkArea.Width;
            //var he = SystemParameters.WorkArea.Height;
            //Left = wi - ActualWidth - 50;
            //Top = he - ActualHeight - 50;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.EnableBlur();
            NowDateTime = DateTime.Now;

            //GetJson.UpdateEvents();
            DesignCalendar();

            IsDark = true;
            SetTheme();

            // http://tourismtime.ir/api/eventall
        }

        private void DesignCalendar(DateTime? targetDateTime = null)
        {
            CalendarDays.Children.Clear();

            if (targetDateTime == null) targetDateTime = NowDateTime;
            var persianCalendar = Calendar.ConvertToPersian((DateTime)targetDateTime);

            var persianYear = persianCalendar.ArrayType[0];
            var persianMonth = persianCalendar.ArrayType[1];

            var monthDays = Database.GetMonthDays(persianMonth);
            Months = Database.DeserializeJson(persianYear, persianMonth);

            for (var day = 0; day < monthDays; day++)
            {
                var persianDay = day + 1;
                var dayModel = Months.FirstOrDefault(x => x.Key == persianMonth).Value.Find(x => x.DayOfMonth == persianDay);

                var currentDate =
                    Calendar.ConvertToPersian(((DateTime)targetDateTime).Year, ((DateTime)targetDateTime).Month, day, DateType.Persian);
                var persianDayOfWeek = (DayOfWeekConverter.PersianDayOfWeek)currentDate.DayOfWeek;

                DesignDayButton(CalendarDays, persianYear, persianMonth, persianDay, persianDayOfWeek, dayModel);
            }

            SetTodayDate();

            var firstDayOfGregorianMonth = Calendar.ConvertToGregorian(persianYear, persianMonth, 1, DateType.Persian);
            var lastDayOfGregorianMonth = Calendar.ConvertToGregorian(persianYear, monthDays, 1, DateType.Persian);

            PersianTitle.Text = $"{MonthsName.Persian(persianMonth)} {persianYear}";
            GregorianTitle.Text = MonthsName.Gregorian((DateTime)targetDateTime, firstDayOfGregorianMonth, lastDayOfGregorianMonth);
            HijriTitle.Text = MonthsName.Hijri(firstDayOfGregorianMonth, lastDayOfGregorianMonth);
        }

        private void SetTodayDate()
        {
            var persianCalendar = Calendar.ConvertToPersian(DateTime.Now);
            var persianTodayYear = persianCalendar.ArrayType[0];
            var persianTodayMonth = persianCalendar.ArrayType[1];
            var persianTodayDay = persianCalendar.ArrayType[2];
            var persianTodayDayOfWeek = persianCalendar.DayOfWeek;

            TodayDate.Content =
                $"امروز: {((DayOfWeekConverter.PersianDayOfWeek)persianTodayDayOfWeek).GetDisplay()} {persianTodayDay} {MonthsName.Persian(persianTodayMonth)} {persianTodayYear}";
        }

        private void DesignDayButton(Panel parentGrid, int persianYear, int persianMonth, int persianDay, DayOfWeekConverter.PersianDayOfWeek dayOfWeek, DayModel dayModel)
        {
            var gregorianOfThisDay = Calendar.ConvertToGregorian(persianYear, persianMonth, persianDay, DateType.Persian);

            // Condition check
            var hasEvent = dayModel.Events?.Count > 0;
            var isToday = gregorianOfThisDay.Date == DateTime.Now.Date;
            var isHoliday = (hasEvent && dayModel.IsHoliday) || ((int)dayOfWeek + 1 == 7);

            var persianDayForecolor = CurrentThemePersianDayForecolor;
            var gregorianDayForecolor = CurrentThemeGregHijrForecolor;
            var hijriDayForecolor = CurrentThemeGregHijrForecolor;

            var dayBackground = hasEvent ? CurrentThemeHasEventDayBackColor : Brushes.Transparent;

            var dayBorderThick = hasEvent && isToday ? new Thickness(3, 3, 3, 3) : new Thickness(0, 0, 0, 0);
            var dayBorderColor = hasEvent && isToday ? Brushes.DimGray : Brushes.Transparent;
            if (isToday)
            {
                dayBackground = new SolidColorBrush(new Color
                {
                    A = 75,
                    R = 0,
                    G = 0,
                    B = 0
                });
                persianDayForecolor = Brushes.White;
                gregorianDayForecolor = Brushes.White;
                hijriDayForecolor = Brushes.White;
            }

            if (isHoliday)
            {
                persianDayForecolor = Brushes.Red;
                gregorianDayForecolor = Brushes.Red;
                hijriDayForecolor = Brushes.Red;
            }

            if (isToday && isHoliday)
            {
                dayBackground = Brushes.Red;
                persianDayForecolor = Brushes.White;
                gregorianDayForecolor = Brushes.White;
                hijriDayForecolor = Brushes.White;
            }

            // Stylize day box
            var persianDayNumber = new DayTextBlock
            {
                Text = persianDay.ToString(),
                Foreground = persianDayForecolor,
                FontSize = 14,
                FontFamily = new FontFamily("IRANSansWeb(FaNum)")
            };

            var gregorianDayNumber = new DayTextBlock
            {
                Margin = new Thickness(4, 0, 0, 0),
                Text = gregorianOfThisDay.Day.ToString(),
                FontSize = 10,
                Foreground = gregorianDayForecolor,
                FontFamily = new FontFamily("IRANSansWeb"),
            };
            var hijriDayNumber = new DayTextBlock
            {
                Margin = new Thickness(0, 0, 4, 0),
                Text = ArabicDigits.Convert(Calendar
                    .ConvertToIslamic(persianYear, persianMonth, persianDay, DateType.Persian).ArrayType[2].ToString()),
                FontFamily = new FontFamily("IRANSansWeb"),
                Foreground = hijriDayForecolor,
                FontSize = 10,
            };

            var subBlock = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)},
                    new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)}
                },
                Children =
                {
                    gregorianDayNumber,
                    hijriDayNumber
                },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            Grid.SetColumn(gregorianDayNumber, 0);
            Grid.SetColumn(hijriDayNumber, 1);

            var dayGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                },
                Children =
                {
                    persianDayNumber,
                    subBlock
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 40,
            };

            Grid.SetRow(persianDayNumber, 0);
            Grid.SetRow(subBlock, 1);

            var block = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Style = (Style)Application.Current.Resources["DayItemTheme"],
                ClipToBounds = true,
                Content = dayGrid,
                Background = dayBackground,
                BorderThickness = dayBorderThick,
                BorderBrush = dayBorderColor,
            };

            block.Click += delegate { ToggleEvent(persianYear, persianMonth, persianDay, dayOfWeek, dayModel); };
            parentGrid.Children.Add(block);

            var dayOfWeekIndex = (int)dayOfWeek;
            var weekIndex = ((persianDay - 1) + (6 - dayOfWeekIndex)) / 7;

            Grid.SetRow(block, weekIndex);
            Grid.SetColumn(block, dayOfWeekIndex);
        }

        //private void PointToDay(int persianYear, int persianMonth, int persianDay)
        //{
        //}

        //public class Day
        //{
        //    public DayOfWeekConverter.PersianDayOfWeeks DayOfWeek { get; set; }
        //    public DayModel Event { get; set; }
        //}
        //private Day ParseDay(int persianYear, int persianMonth, int persianDay)
        //{
        //}
        private void BtnNextMonth_OnClick(object sender, RoutedEventArgs e)
        {
            MonthHandle(true);
        }

        private void ChangeMonth(DateTime dt)
        {
            var easing = new CircleEase { EasingMode = EasingMode.EaseInOut };
            var animDur = TimeSpan.FromMilliseconds(300);
            CalendarDays.BeginAnimation(OpacityProperty, new DoubleAnimation(CalendarDays.Opacity, 0, animDur) { EasingFunction = easing });
            PersianTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(PersianTitle.Opacity, 0, animDur) { EasingFunction = easing });
            GregorianTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(GregorianTitle.Opacity, 0, animDur) { EasingFunction = easing });
            HijriTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(HijriTitle.Opacity, 0, animDur) { EasingFunction = easing });

            var timer = new DispatcherTimer { Interval = animDur };
            timer.Start();
            timer.Tick += delegate
            {
                timer.Stop();
                NowDateTime = dt;
                DesignCalendar();

                CalendarDays.BeginAnimation(OpacityProperty, new DoubleAnimation(CalendarDays.Opacity, 1, animDur) { EasingFunction = easing });
                PersianTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(PersianTitle.Opacity, 1, animDur) { EasingFunction = easing });
                GregorianTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(GregorianTitle.Opacity, 1, animDur) { EasingFunction = easing });
                HijriTitle.BeginAnimation(OpacityProperty, new DoubleAnimation(HijriTitle.Opacity, 1, animDur) { EasingFunction = easing });
            };
        }

        // Next or previous month
        private void MonthHandle(bool next)
        {
            var d = next ? 1 : -1;
            ChangeMonth(NowDateTime.AddMonths(d));
        }

        // Today
        private void MonthHandle()
        {
            var persianCalendar = new PersianCalendar();
            ChangeMonth(DateTime.Now);
        }

        // Specific Month
        private void MonthHandle(int persianYear, int persianMonth)
        {
            var persianCalendar = new PersianCalendar();
            var gregorianOfThisDay = persianCalendar.ToDateTime(persianYear, persianMonth, 1, 0, 0, 0, 0);
            ChangeMonth(gregorianOfThisDay);
        }

        private void BtnPrevMonth_OnClick(object sender, RoutedEventArgs e)
        {
            MonthHandle(false);
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ToggleEvent(int persianYear, int persianMonth, int persianDay, DayOfWeekConverter.PersianDayOfWeek thisDayOfWeek, DayModel @event)
        {
            if (@event?.Events?.Count >= 1)
            {
                CurrentEventDay = $"{thisDayOfWeek.GetDisplay()} {persianDay} {MonthsName.Persian(persianMonth)} {persianYear}";

                CurrentEventText = string.Join(Environment.NewLine, @event.Events.ToArray());
                PanelUtils.OpenPanel<Event>(ref GridWrapper, ref OpenedFrames, ref Backdrop, ActualHeight, eventPage =>
                  {
                      eventPage.EventDay.Text = CurrentEventDay;
                      eventPage.EventText.Text = CurrentEventText;
                      eventPage.EventText.MouseDown += (sender, eventArgs) =>
                      {
                      };
                      const int eventTitleHeight = 20;
                      var eventsApproxHeight =
                          (eventPage.EventText.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length * 22)
                          + eventPage.EventText.Margin.Top;
                      return eventTitleHeight + eventsApproxHeight + 30;
                  });
            }
            else
            {
                PanelUtils.ClosePanel(ref GridWrapper, ref OpenedFrames, ref Backdrop, ActualHeight);
            }
        }

        private void OpenSetting()
        {
            //SetTheme();
            //PanelUtils.OpenPanel<Setting>(_ => 100);
        }

        private void Backdrop_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            PanelUtils.ClosePanel(ref GridWrapper, ref OpenedFrames, ref Backdrop, ActualHeight);

            Console.WriteLine("Opened-frames count: {0}", OpenedFrames.Count);
            Console.WriteLine("Backdrop opacity: {0}", Backdrop.Opacity);
            Console.WriteLine("Mainwindow actual height: {0}", ActualHeight);
            Console.WriteLine("-----------------------");
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void SetTheme()
        {
            if (IsDark)
            {
                CurrentThemeThemeNoiseColor = StaticValues.WhiteTheme.ThemeColor;
                CurrentThemeThemeNoiseOpacity = StaticValues.WhiteTheme.ThemeBlurinessOpacity;
                CurrentThemeThemeNoiseRatioInt = StaticValues.WhiteTheme.ThemeBlurinessRatio;
                CurrentThemeHasEventDayBackColor = StaticValues.WhiteTheme.HasEventDayBackColor;
                CurrentThemeDayLeadForecolor = StaticValues.WhiteTheme.DayLeadColor;
                CurrentThemePersianDayForecolor = StaticValues.WhiteTheme.PersianDayColor;
                CurrentThemeGregHijrForecolor = StaticValues.WhiteTheme.GregorianHijriColor;
                CurrentThemeTodayForecolor = StaticValues.WhiteTheme.TodayForecolor;
                CurrentThemeSeparator = StaticValues.WhiteTheme.Separator;
                CurrentThemeChangeMonthForecolor = StaticValues.WhiteTheme.ChangeMonth;
                IsDark = false;
            }
            else
            {
                CurrentThemeThemeNoiseColor = StaticValues.DarkTheme.ThemeColor;
                CurrentThemeThemeNoiseOpacity = StaticValues.DarkTheme.ThemeBlurinessOpacity;
                CurrentThemeThemeNoiseRatioInt = StaticValues.DarkTheme.ThemeBlurinessRatio;
                CurrentThemeHasEventDayBackColor = StaticValues.DarkTheme.HasEventDayBackColor;
                CurrentThemeDayLeadForecolor = StaticValues.DarkTheme.DayLeadColor;
                CurrentThemePersianDayForecolor = StaticValues.DarkTheme.PersianDayColor;
                CurrentThemeGregHijrForecolor = StaticValues.DarkTheme.GregorianHijriColor;
                CurrentThemeTodayForecolor = StaticValues.DarkTheme.TodayForecolor;
                CurrentThemeSeparator = StaticValues.DarkTheme.Separator;
                CurrentThemeChangeMonthForecolor = StaticValues.DarkTheme.ChangeMonth;
                IsDark = true;
            }

            var animationDuration = TimeSpan.FromMilliseconds(300);

            ThemeNoise.CreateStoryBoard(CurrentThemeThemeNoiseColor, "(Border.Background).(SolidColorBrush.Color)", animationDuration);
            PersianTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", animationDuration);
            GregorianTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", animationDuration);
            HijriTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", animationDuration);
            TodayDate.CreateStoryBoard(CurrentThemeTodayForecolor, "(Button.Foreground).(SolidColorBrush.Color)", animationDuration);
            Separator.CreateStoryBoard(CurrentThemeSeparator, "Background.Color", animationDuration);
            BtnPrevMonth.CreateStoryBoard(CurrentThemeChangeMonthForecolor, "(Button.Foreground).(SolidColorBrush.Color)", animationDuration);
            BtnNextMonth.CreateStoryBoard(CurrentThemeChangeMonthForecolor, "(Button.Foreground).(SolidColorBrush.Color)", animationDuration);
            DesignCalendar(NowDateTime);

            foreach (var day in CalendarDays.Children)
            {
                var dayDetails = ((day as Button)?.Content as Grid)?.Children;
                if (dayDetails == null) continue;

                foreach (var dayDetail in dayDetails)
                    switch (dayDetail)
                    {
                        case TextBlock persianDay:
                            persianDay.CreateStoryBoard(CurrentThemePersianDayForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", animationDuration);
                            break;

                        case Grid customDays:
                            foreach (var cusDay in customDays.Children)
                                if (cusDay is TextBlock customDay)
                                    customDay.CreateStoryBoard(CurrentThemeGregHijrForecolor,
                                        "(TextBlock.Foreground).(SolidColorBrush.Color)", animationDuration);
                            break;
                    }
            }

            ThemeNoise.BeginAnimation(OpacityProperty, new DoubleAnimation(ThemeNoise.Opacity, CurrentThemeThemeNoiseOpacity, animationDuration));
            ThemeNoiseRatio.Ratio = CurrentThemeThemeNoiseRatioInt;
            DesignCalendar(NowDateTime);
        }

        private void BtnChangeTheme_OnClick(object sender, RoutedEventArgs e)
        {
            //SetTheme();
            OpenSetting();
        }

        private void TodayDate_OnClick(object sender, RoutedEventArgs e)
        {
            MonthHandle();
        }
    }
}