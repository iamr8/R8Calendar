using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace R8Calendar.Utils
{
    public static class PanelUtils
    {
        public static Border AddFrame(int index, double mainWindowHeight)
        {
            var border = new Border
            {
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Margin = new Thickness(0, mainWindowHeight, 0, 0),
                Opacity = 0,
                Name = $"PageViewContainer{index}",
                Background = Brushes.White,
                Child = new Frame
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    NavigationUIVisibility = NavigationUIVisibility.Hidden,
                    Name = $"PageViewer{index}",
                    Content = null,
                }
            };

            return border;
        }

        public static void OpenPanel<T>(ref Grid gridWrapper, ref List<Border> pageViewersList, ref Border backdrop, double mainWindowHeight,
            Func<T, double> doAfter) where T : Page
        {
            if (pageViewersList == null) throw new ArgumentNullException(nameof(pageViewersList));
            if (backdrop == null) throw new ArgumentNullException(nameof(backdrop));
            if (doAfter == null) throw new ArgumentNullException(nameof(doAfter));
            if (mainWindowHeight <= 0) throw new ArgumentOutOfRangeException(nameof(mainWindowHeight));
            if (!(typeof(T) != typeof(Page)))
                throw new Exception("T is not instance of Page()");

            var index = pageViewersList?.Count + 1 ?? 0;
            var targetPage = (T)Activator.CreateInstance(typeof(T));
            var border = AddFrame(index, mainWindowHeight);
            var frame = (Frame)border.Child;

            pageViewersList.Add(border);
            frame.Navigate(targetPage);

            backdrop.Visibility = Visibility.Visible;
            gridWrapper.Children.Add(border);

            Console.WriteLine("Opened-frames count: {0}", pageViewersList.Count);
            Console.WriteLine("Mainwindow actual height: {0}", mainWindowHeight);
            AnimatePanel(ref border, ref backdrop, mainWindowHeight - doAfter.Invoke(targetPage), 1, 0.6);
        }

        public static void AnimatePanel(ref Border pageViewContainer, ref Border backdrop, double topPosition,
            double eventOpacity, double backdropOpacity)
        {
            var duration = TimeSpan.FromMilliseconds(150);

            pageViewContainer.BeginAnimation(FrameworkElement.MarginProperty,
                new ThicknessAnimation(pageViewContainer.Margin, new Thickness(0, topPosition, 0, 0), duration));

            pageViewContainer.Opacity = eventOpacity;
            backdrop.BeginAnimation(UIElement.OpacityProperty,
                new DoubleAnimation(backdrop.Opacity, backdropOpacity, duration));

            Console.WriteLine("Backdrop opacity: {0}", backdrop.Opacity);
            Console.WriteLine("panel top position: {0}", topPosition);
            Console.WriteLine("panel opacity: {0}", eventOpacity);
            Console.WriteLine("-----------------------");
        }

        public static void ClosePanel(ref Grid gridWrapper, ref List<Border> pageViewersList, ref Border backdrop, double mainWindowHeight)
        {
            if (pageViewersList.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(pageViewersList));

            var targetBorder = pageViewersList.Last();
            pageViewersList.Remove(targetBorder);
            gridWrapper.Children.Remove(targetBorder);

            AnimatePanel(ref targetBorder, ref backdrop, mainWindowHeight, 1, 0);
            backdrop.Visibility = Visibility.Hidden;
            targetBorder.Opacity = 0;
            //var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            //timer.Start();
            //timer.Tick += delegate
            //{
            //    timer.Stop();
            //    backdrop.Visibility = Visibility.Hidden;
            //    PageViewContainer.Opacity = 0;
            //};
        }
    }
}