using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mechavian.GanttControls.Models;

namespace Mechavian.GanttControls
{
    public class GanttChart : Control
    {
        static GanttChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GanttChart), new FrameworkPropertyMetadata(typeof(GanttChart)));
        }

        public static readonly DependencyProperty GanttDescriptorProperty = DependencyProperty.Register("GanttDescriptor", typeof(GanttDescriptor), typeof(GanttChart), new PropertyMetadata(default(GanttDescriptor), OnGanttDescriptorChanged));

        public GanttDescriptor GanttDescriptor
        {
            get { return (GanttDescriptor)GetValue(GanttDescriptorProperty); }
            set { SetValue(GanttDescriptorProperty, value); }
        }

        private static void OnGanttDescriptorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ganttChart = (GanttChart)d;
            var descriptor = (GanttDescriptor)e.NewValue;

            var grid = ganttChart.GetTemplateChild("Grid") as Grid;
            if (grid == null) return;

            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Children.Clear();

            if (descriptor == null) return;

            // Item descriptor column
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            grid.Children.Add(CreateHeaderCell(null, 0, 0));

            for (var col = 0; col < descriptor.Ranges.Length; col++)
            {
                var range = descriptor.Ranges[col];
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.Children.Add(CreateHeaderCell(range.Name, col+1, 0));
            }

            for (var row = 0; row < descriptor.Work.Length; row++)
            {
                var work = descriptor.Work[row];
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(CreateHeaderCell(work.Name, 0, row+1));
            }
        }

        private static Border CreateHeaderCell(string text, int column, int row)
        {
            var border = new Border { Background = new SolidColorBrush(Colors.Blue) };
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);

            if (text != null)
            {
                border.Child = new Label()
                {
                    Foreground = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Content = text
                };
            }

            return border;
        }
    }
}
