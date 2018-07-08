using System.Windows;
using System.Windows.Media;

namespace R8Calendar.Utils
{
    public static class GetControl
    {
        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                //get parent item
                var parentObject = GetParentObject(child);

                switch (parentObject)
                {
                    //we've reached the end of the tree
                    case null:
                        return null;
                    //check if the parent matches the type we're looking for
                    case T parent:
                        return parent;

                    default:
                        child = parentObject;
                        continue;
                }
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            switch (child)
            {
                case null:
                    return null;
                //handle content elements separately
                case ContentElement contentElement:
                    {
                        var parent = ContentOperations.GetParent(contentElement);
                        if (parent != null) return parent;

                        var fce = contentElement as FrameworkContentElement;
                        return fce?.Parent;
                    }
                //also try searching for parent in framework elements (such as DockPanel, etc)
                case FrameworkElement frameworkElement:
                    {
                        var parent = frameworkElement.Parent;
                        if (parent != null) return parent;
                        break;
                    }
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }
}