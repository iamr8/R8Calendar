using R8.Lib;
using R8.PanelStack;

using R8Calendar.Blur;
using R8Calendar.Converter;
using R8Calendar.Models;
using R8Calendar.Utils;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace R8Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DateTime NowDateTime { get; set; }
        private ConcurrentDictionary<int, List<DayModel>> Months { get; set; }

        // Theme
        private bool IsDark { get; set; }

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

        private PanelAdapter _panelWrapper;

        #endregion Theme Details

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.EnableBlur();
            NowDateTime = DateTime.Now;

            _panelWrapper = new PanelAdapter
            {
                Container = PanelContainer,
                Backdrop = Backdrop,
                UseBackdrop = true,
                ParentHeight = ActualHeight,
                Log = Console.WriteLine
            };

            DesignCalendar();
            IsDark = true;
            SetTheme();
        }

        private void DesignCalendar(DateTime? targetDateTime = null)
        {
            CalendarDays.Children.Clear();

            targetDateTime ??= NowDateTime;
            var persianCalendar = new PersianDateTime(targetDateTime.Value);

            var persianYear = persianCalendar.Year;
            var persianMonth = persianCalendar.Month;

            var monthDays = Database.GetMonthDays(persianMonth);

            var sync = SynchronizationContext.Current;
            Months = Database.DeserializeJson(persianYear, persianMonth);

            for (var dayIndex = 0; dayIndex < monthDays; dayIndex++)
            {
                var day = dayIndex + 1;
                var dayModel = Months.FirstOrDefault(x => x.Key == persianMonth).Value.Find(x => x.DayOfMonth == day);

                var currentDate = new PersianDateTime(new DateTime(targetDateTime.Value.Year, targetDateTime.Value.Month, day));
                var persianDayOfWeek = (DayOfWeekConverter.PersianDayOfWeek)currentDate.DayOfWeek;

                DesignDayButton(CalendarDays, persianYear, persianMonth, day, persianDayOfWeek, dayModel);
            }

            SetTodayDate();

            var firstDayOfGregorianMonth = new PersianDateTime(persianYear, persianMonth, 1).ToDateTime();
            var lastDayOfGregorianMonth = new PersianDateTime(persianYear, persianMonth, monthDays).ToDateTime();
            // var lastDayOfGregorianMonth = Calendar.ConvertToGregorian(persianYear, monthDays, 1, DateType.Persian);

            PersianTitle.Text = $"{MonthsName.Persian(persianMonth)} {persianYear}";
            GregorianTitle.Text = MonthsName.Gregorian((DateTime)targetDateTime, firstDayOfGregorianMonth, lastDayOfGregorianMonth);
            HijriTitle.Text = MonthsName.Hijri(firstDayOfGregorianMonth, lastDayOfGregorianMonth);
        }

        private void SetTodayDate()
        {
            var persianCalendar = new PersianDateTime(DateTime.Now);
            var persianTodayYear = persianCalendar.Year;
            var persianTodayMonth = persianCalendar.Month;
            var persianTodayDay = persianCalendar.DayOfMonth;
            var persianTodayDayOfWeek = persianCalendar.DayOfWeek;

            TodayDate.Content =
                $"امروز: {((DayOfWeekConverter.PersianDayOfWeek)persianTodayDayOfWeek).GetDisplayName()} {persianTodayDay} {MonthsName.Persian(persianTodayMonth)} {persianTodayYear}";
        }

        private void DesignDayButton(Panel parentGrid, int persianYear, int persianMonth, int persianDay, DayOfWeekConverter.PersianDayOfWeek dayOfWeek, DayModel dayModel)
        {
            var gregorianOfThisDay = new PersianDateTime(persianYear, persianMonth, persianDay).ToDateTime();

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
                Text = new HijriCalendar().GetDayOfMonth(gregorianOfThisDay).ToString(),
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
            timer.Tick += (sender, args) =>
            {
                ((DispatcherTimer)sender)?.Stop();
                NowDateTime = dt;
                DesignCalendar();

                CalendarDays.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(CalendarDays.Opacity, 1, animDur) { EasingFunction = easing });
                PersianTitle.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(PersianTitle.Opacity, 1, animDur) { EasingFunction = easing });
                GregorianTitle.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(GregorianTitle.Opacity, 1, animDur) { EasingFunction = easing });
                HijriTitle.BeginAnimation(OpacityProperty,
                    new DoubleAnimation(HijriTitle.Opacity, 1, animDur) { EasingFunction = easing });
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

        private void ToggleEvent(int persianYear, int persianMonth, int persianDay,
            DayOfWeekConverter.PersianDayOfWeek thisDayOfWeek, DayModel @event)
        {
            if (@event?.Events?.Count >= 1)
            {
                _panelWrapper.Show<Event>(eventPage =>
                {
                    eventPage.EventDay.Text =
                        $"{thisDayOfWeek.GetDisplayName()} {persianDay} {MonthsName.Persian(persianMonth)} {persianYear}";
                    eventPage.EventText.Text = string.Join(Environment.NewLine, @event.Events.ToArray());

                    var eventsApproxHeight =
                        (eventPage.EventText.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length *
                         22)
                        + eventPage.EventText.Margin.Top;
                    var topMargin = eventsApproxHeight + 50;

                    eventPage.MouseDown += (sender, args) =>
                    {
                        _panelWrapper.Show<Event>(eventPage2 =>
                        {
                            eventPage2.MouseDown += (sender2, args2) =>
                            {
                                _panelWrapper.Show<Event>(_ => topMargin);
                            };
                            return topMargin;
                        });
                    };
                    return topMargin;
                }, TimeSpan.FromSeconds(2));
            }
            else
            {
                _panelWrapper.CloseLastPanel();
            }
        }

        private void OpenSetting()
        {
            _panelWrapper.Show<Setting>(setting =>
            {
                setting.BtnChangeTheme.Click += async (sender, args) =>
                {
                    SetTheme();
                };
                return 100;
            });
        }

        private void Backdrop_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _panelWrapper.CloseLastPanel();
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void SetTheme()
        {
            if (IsDark)
            {
                CurrentThemeThemeNoiseColor = Theme.WhiteTheme.ThemeColor;
                CurrentThemeThemeNoiseOpacity = Theme.WhiteTheme.ThemeBlurinessOpacity;
                CurrentThemeThemeNoiseRatioInt = Theme.WhiteTheme.ThemeBlurinessRatio;
                CurrentThemeHasEventDayBackColor = Theme.WhiteTheme.HasEventDayBackColor;
                CurrentThemeDayLeadForecolor = Theme.WhiteTheme.DayLeadColor;
                CurrentThemePersianDayForecolor = Theme.WhiteTheme.PersianDayColor;
                CurrentThemeGregHijrForecolor = Theme.WhiteTheme.GregorianHijriColor;
                CurrentThemeTodayForecolor = Theme.WhiteTheme.TodayForecolor;
                CurrentThemeSeparator = Theme.WhiteTheme.Separator;
                CurrentThemeChangeMonthForecolor = Theme.WhiteTheme.ChangeMonth;
                IsDark = false;
            }
            else
            {
                CurrentThemeThemeNoiseColor = Theme.DarkTheme.ThemeColor;
                CurrentThemeThemeNoiseOpacity = Theme.DarkTheme.ThemeBlurinessOpacity;
                CurrentThemeThemeNoiseRatioInt = Theme.DarkTheme.ThemeBlurinessRatio;
                CurrentThemeHasEventDayBackColor = Theme.DarkTheme.HasEventDayBackColor;
                CurrentThemeDayLeadForecolor = Theme.DarkTheme.DayLeadColor;
                CurrentThemePersianDayForecolor = Theme.DarkTheme.PersianDayColor;
                CurrentThemeGregHijrForecolor = Theme.DarkTheme.GregorianHijriColor;
                CurrentThemeTodayForecolor = Theme.DarkTheme.TodayForecolor;
                CurrentThemeSeparator = Theme.DarkTheme.Separator;
                CurrentThemeChangeMonthForecolor = Theme.DarkTheme.ChangeMonth;
                IsDark = true;
            }

            var animationDuration = TimeSpan.FromMilliseconds(300);

            ThemeNoise.CreateStoryBoard(CurrentThemeThemeNoiseColor, "(Border.Background).(SolidColorBrush.Color)");
            PersianTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)");
            GregorianTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)");
            HijriTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)");
            TodayDate.CreateStoryBoard(CurrentThemeTodayForecolor, "(Button.Foreground).(SolidColorBrush.Color)");
            Separator.CreateStoryBoard(CurrentThemeSeparator, "Background.Color");
            BtnPrevMonth.CreateStoryBoard(CurrentThemeChangeMonthForecolor, "(Button.Foreground).(SolidColorBrush.Color)");
            BtnNextMonth.CreateStoryBoard(CurrentThemeChangeMonthForecolor, "(Button.Foreground).(SolidColorBrush.Color)");

            DesignCalendar(NowDateTime);

            foreach (var day in CalendarDays.Children)
            {
                var dayDetails = ((day as Button)?.Content as Grid)?.Children;
                if (dayDetails == null) continue;

                foreach (var dayDetail in dayDetails)
                    switch (dayDetail)
                    {
                        case TextBlock persianDay:
                            persianDay.CreateStoryBoard(CurrentThemePersianDayForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)");
                            break;

                        case Grid customDays:
                            foreach (var cusDay in customDays.Children)
                                if (cusDay is TextBlock customDay)
                                    customDay.CreateStoryBoard(CurrentThemeGregHijrForecolor,
                                        "(TextBlock.Foreground).(SolidColorBrush.Color)");
                            break;
                    }
            }

            ThemeNoise.BeginAnimation(OpacityProperty, new DoubleAnimation(ThemeNoise.Opacity, CurrentThemeThemeNoiseOpacity, animationDuration));
            ThemeNoiseRatio.Ratio = CurrentThemeThemeNoiseRatioInt;
            DesignCalendar(NowDateTime);
        }

        private void BtnChangeTheme_OnClick(object sender, RoutedEventArgs e)
        {
            //SetThemeAsync();
            OpenSetting();
        }

        private void TodayDate_OnClick(object sender, RoutedEventArgs e)
        {
            MonthHandle();
        }
    }
}