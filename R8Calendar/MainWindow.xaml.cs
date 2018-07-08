using R8Calendar.Converter;
using R8Calendar.Models;
using R8Calendar.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MonthModel = R8Calendar.Models.MonthModel;

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
        private TimeSpan EventAnimationDuration => TimeSpan.FromMilliseconds(500);
        private List<MonthModel> Months { get; set; }

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

        private Thickness EventClosedPosition { get; set; }

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

            Months = GetJson.OpenFile(NowDateTime);
            ReloadCalendar();
            EventClosedPosition = new Thickness(20, ActualHeight - 20, 20, 0);
            PageViewContainer.Margin = EventClosedPosition;
            PageViewContainer.Opacity = EventOffOpacity;
            IsDark = false;
            SetTheme();

            // http://tourismtime.ir/api/eventall
        }

        private void ReloadCalendar()
        {
            DesignCalendar(NowDateTime);
        }

        private void DesignCalendar(DateTime dt)
        {
            CalendarDays.Children.Clear();

            var persianCalendar = new PersianCalendar();

            var persianYear = persianCalendar.GetYear(dt);
            var persianMonth = persianCalendar.GetMonth(dt);
            var rowIndex =
                (int)DayOfWeekConverter.Persian(
                    persianCalendar.GetDayOfWeek(persianCalendar.ToDateTime(persianYear, persianMonth, 1, 0, 0, 0, 0)));
            var monthDaysCount = persianCalendar.GetDaysInMonth(persianYear, persianMonth, 1);

            for (var day = 0; day < monthDaysCount; day++)
            {
                var persianDay = day + 1;

                var greg = PersianConverter.PersianToGregorian(persianYear, persianMonth, persianDay);
                var gregorianDay = greg.DayOfMonth;
                var hijriDay = HijriConverter.GregorianToHijri(greg.Year, greg.Month, greg.DayOfMonth).DayOfMonth;

                DesignDayButton(persianCalendar, CalendarDays, persianYear, persianMonth, persianDay, rowIndex,
                    gregorianDay, hijriDay);
            }

            var persianTodayYear = persianCalendar.GetYear(DateTime.Now);
            var persianTodayMonth = persianCalendar.GetMonth(DateTime.Now);
            var persianTodayDay = persianCalendar.GetDayOfMonth(DateTime.Now);
            var persianTodayDayOfWeek = persianCalendar.GetDayOfWeek(DateTime.Now);

            TodayDate.Content =
                $"امروز: {DayOfWeekConverter.Persian(persianTodayDayOfWeek).GetDisplay()} {persianTodayDay} {MonthsName.Persian(persianTodayMonth)} {persianTodayYear}";

            PersianTitle.Text = $"{MonthsName.Persian(persianMonth)} {persianYear}";

            var firstDayOfMonth = persianCalendar.ToDateTime(persianYear, persianMonth, 1, 0, 0, 0, 0);
            var lastDayOfMonth = persianCalendar.ToDateTime(persianYear, persianMonth, monthDaysCount, 0, 0, 0, 0);

            GregorianTitle.Text = MonthsName.Gregorian(dt, firstDayOfMonth, lastDayOfMonth);
            HijriTitle.Text = MonthsName.Hijri(firstDayOfMonth, lastDayOfMonth);
        }

        private void DesignDayButton(PersianCalendar persianInstance, Panel parentGrid, int persianYear, int persianMonth, int persianDay, int adjustDay, int gregorianDay, int hijriDay)
        {
            var persianCalendar = persianInstance;
            var now = DateTime.Now;
            var gregorianOfThisDay = persianCalendar.ToDateTime(persianYear, persianMonth, persianDay, 0, 0, 0, 0);
            var thisDayOfWeek = DayOfWeekConverter.Persian(persianCalendar.GetDayOfWeek(gregorianOfThisDay));
            var dayIndex = (int)thisDayOfWeek;
            var eventOfDay = Months.FirstOrDefault(x => x.MonthNo == persianMonth)?.Days
                .FirstOrDefault(x => x.DayOfMonth == persianDay);

            // Condition check
            var hasEvent = eventOfDay != null;
            var holidayEvent = hasEvent && eventOfDay.IsHoliday;
            var isToday = gregorianOfThisDay.Year.Equals(now.Year) && gregorianOfThisDay.Month.Equals(now.Month) &&
                          gregorianOfThisDay.Day.Equals(now.Day);
            var isFriday = dayIndex + 1 == 7;
            var isHoliday = holidayEvent || isFriday;
            //var isHoliday = (hasEvent && eventOfDay.IsHoliday) || (dayIndex + 1 == 7);

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
            var persianDayInt = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Text = persianDay.ToString(),
                Foreground = persianDayForecolor,
                FontSize = 14,
                FontFamily = new FontFamily("IRANSansWeb(FaNum)")
            };

            var englishDayInt = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0),
                TextAlignment = TextAlignment.Center,
                Text = gregorianDay.ToString(),
                FontSize = 10,
                Foreground = gregorianDayForecolor,
                FontFamily = new FontFamily("IRANSansWeb"),
            };
            var hijriDayInt = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0),
                TextAlignment = TextAlignment.Center,
                Text = ArabicDigits.Convert(hijriDay.ToString()),
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
                    englishDayInt,
                    hijriDayInt
                },
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            Grid.SetColumn(englishDayInt, 0);
            Grid.SetColumn(hijriDayInt, 1);

            var dayGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                    new RowDefinition {Height = new GridLength(1, GridUnitType.Star)},
                },
                Children =
                {
                    persianDayInt,
                    subBlock
                },
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 40,
            };

            Grid.SetRow(persianDayInt, 0);
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

            block.Click += delegate { ToggleEvent(persianYear, persianMonth, persianDay, thisDayOfWeek, eventOfDay); };
            parentGrid.Children.Add(block);

            Grid.SetRow(block, ((persianDay - 1) + adjustDay) / 7);
            Grid.SetColumn(block, dayIndex);
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
                ReloadCalendar();

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

        private void ToggleEvent(int persianYear, int persianMonth, int persianDay, DayOfWeekConverter.PersianDayOfWeeks thisDayOfWeek, DayModel @event)
        {
            if (@event != null)
            {
                CurrentEventDay = $"{thisDayOfWeek.GetDisplay()} {persianDay} {MonthsName.Persian(persianMonth)} {persianYear}";
                CurrentEventText = string.Join(Environment.NewLine, @event.Events.ToArray());
                OpenEvent();
            }
            else
            {
                ClosePanel();
            }
        }

        private void OpenPanel<T>(T page, Func<T, double, Thickness> doAfter) where T : Page
        {
            if (!(typeof(T) != typeof(Page))) return;
            var finalHeight = doAfter.Invoke(page, ActualHeight);
            PageViewer.Content = null;
            PageViewer.Navigate(page);
            Backdrop.Visibility = Visibility.Visible;
            AnimatePanel(finalHeight, EventOnOpacity, EventBackdropOpacity);
        }

        private void OpenEvent()
        {
            OpenPanel(new Event(), (page, parentHeight) =>
            {
                page.EventDay.Text = CurrentEventDay;
                page.EventText.Text = CurrentEventText;
                var eventTitleHeight = page.EventDay.DesiredSize.Height + page.EventDay.Margin.Top +
                                       page.EventDay.Margin.Bottom;
                var eventsApproxHeight =
                    (page.EventText.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Length * 22) +
                    page.EventText.Margin.Top + page.EventText.Margin.Bottom;
                return new Thickness(20, parentHeight - eventTitleHeight - eventsApproxHeight - 40, 20, 0);
            });
        }

        private void OpenSetting()
        {
            OpenPanel(new Setting(),
                (page, parentHeight) => new Thickness(20, parentHeight - page.ParentGrid.DesiredSize.Height, 20, 0));
        }

        private void AnimatePanel(Thickness finalMove, double eventOpacity, double backdropOpacity)
        {
            var easing = new CircleEase { EasingMode = EasingMode.EaseInOut };

            PageViewContainer.BeginAnimation(MarginProperty, new ThicknessAnimation(PageViewContainer.Margin, finalMove, EventAnimationDuration) { EasingFunction = easing });
            PageViewContainer.BeginAnimation(OpacityProperty, new DoubleAnimation(PageViewContainer.Opacity, eventOpacity, EventAnimationDuration) { EasingFunction = easing });
            Backdrop.BeginAnimation(OpacityProperty, new DoubleAnimation(Backdrop.Opacity, backdropOpacity, EventAnimationDuration) { EasingFunction = easing });

            //var eventTextOpaAnim = new DoubleAnimation(EventText.Opacity, eventOpacity, EventAnimationDuration) { EasingFunction = easing };
            //EventText.BeginAnimation(OpacityProperty, eventTextOpaAnim);
            //EventDay.BeginAnimation(OpacityProperty, eventTextOpaAnim);
        }

        private void ClosePanel()
        {
            CurrentEventDay = string.Empty;
            CurrentEventText = string.Empty;
            AnimatePanel(EventClosedPosition, EventOffOpacity, 0);
            var timer = new DispatcherTimer { Interval = EventAnimationDuration };
            timer.Start();
            timer.Tick += delegate
            {
                timer.Stop();
                Backdrop.Visibility = Visibility.Hidden;
            };
        }

        private void Backdrop_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ClosePanel();
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

            var dur = TimeSpan.FromMilliseconds(300);

            ThemeNoise.CreateStoryBoard(CurrentThemeThemeNoiseColor, "(Border.Background).(SolidColorBrush.Color)", dur);
            PersianTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", dur);
            GregorianTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", dur);
            HijriTitle.CreateStoryBoard(CurrentThemeDayLeadForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", dur);
            TodayDate.CreateStoryBoard(CurrentThemeTodayForecolor, "(Button.Foreground).(SolidColorBrush.Color)", dur);
            Separator.CreateStoryBoard(CurrentThemeSeparator, "Background.Color", dur);
            BtnPrevMonth.CreateStoryBoard(CurrentThemeChangeMonthForecolor, "(Button.Foreground).(SolidColorBrush.Color)", dur);
            BtnNextMonth.CreateStoryBoard(CurrentThemeChangeMonthForecolor, "(Button.Foreground).(SolidColorBrush.Color)", dur);
            DesignCalendar(NowDateTime);

            var calendarDays = CalendarDays.Children;
            foreach (var day in calendarDays)
            {
                var dayDetails = ((day as Button)?.Content as Grid)?.Children;
                if (dayDetails == null) continue;

                foreach (var dayDetail in dayDetails)
                    switch (dayDetail)
                    {
                        case TextBlock persianDay:
                            persianDay.CreateStoryBoard(CurrentThemePersianDayForecolor, "(TextBlock.Foreground).(SolidColorBrush.Color)", dur);
                            break;

                        case Grid customDays:
                            foreach (var cusDay in customDays.Children)
                                if (cusDay is TextBlock customDay)
                                    customDay.CreateStoryBoard(CurrentThemeGregHijrForecolor,
                                        "(TextBlock.Foreground).(SolidColorBrush.Color)", dur);
                            break;
                    }
            }

            ThemeNoise.BeginAnimation(OpacityProperty, new DoubleAnimation(ThemeNoise.Opacity, CurrentThemeThemeNoiseOpacity, dur));
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