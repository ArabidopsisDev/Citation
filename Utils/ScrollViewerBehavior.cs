using System.Windows;
using System.Windows.Controls;

namespace Citation.Utils;

public static class ScrollViewerBehavior
{
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
            new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

    public static void SetVerticalOffset(FrameworkElement target, double value)
    {
        target.SetValue(VerticalOffsetProperty, value);
    }

    public static double GetVerticalOffset(FrameworkElement target)
    {
        return (double)target.GetValue(VerticalOffsetProperty);
    }

    private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}