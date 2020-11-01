using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace R8Calendar.Utils
{
    public static class AnimateColor
    {
        public static void CreateStoryBoard(this FrameworkElement obj, SolidColorBrush toColor, string propertyPath)
        {
            var easing = new CircleEase { EasingMode = EasingMode.EaseInOut };
            var dayLeadColorAnim = new ColorAnimation
            {
                AutoReverse = false,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = easing,
                To = toColor.Color
            };

            var dayLeadStoryBoard = new Storyboard();
            dayLeadStoryBoard.Children.Add(dayLeadColorAnim);

            Storyboard.SetTarget(dayLeadColorAnim, obj);
            Storyboard.SetTargetProperty(dayLeadColorAnim, new PropertyPath(propertyPath));
            dayLeadStoryBoard.Begin(obj);
        }
    }
}