using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mechavian.GanttControls.Models;

namespace Mechavian.GanttControls
{
    public class GanttChart : Control
    {
        public static readonly DependencyProperty GanttDescriptorProperty = DependencyProperty.Register("GanttDescriptor", typeof(GanttDescriptor), typeof(GanttChart), new PropertyMetadata(default(GanttDescriptor), OnGanttDescriptorChanged));

        private static readonly Color HeaderBackground = Color.FromRgb(114, 114, 114);
        private static readonly Color HeaderForeground = Color.FromRgb(255, 255, 255);
        private static readonly Color[] LabelBackgrounds = new[]
        {
            Color.FromRgb(186, 203, 229),
            Color.FromRgb(229, 194, 194),
            Color.FromRgb(217, 229, 192),
            Color.FromRgb(203, 191, 216)
        };
        private static readonly Color[] LabelForegrounds = new[]
        {
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0),
            Color.FromRgb(0, 0, 0)
        };
        private static readonly Color IncompleteProgress = Colors.Purple;
        private static readonly Color CompleteProgress = Colors.DarkGray;

        static GanttChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GanttChart), new FrameworkPropertyMetadata(typeof(GanttChart)));
        }

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

            grid.Children.Add(CreateHeaderCell(0, 0, HeaderBackground));

            for (var col = 0; col < descriptor.Ranges.Length; col++)
            {
                var range = descriptor.Ranges[col];
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.Children.Add(CreateHeaderCell(col+1, 0, text: range.Name, fg: HeaderForeground, bg: HeaderBackground));
            }

            for (var row = 0; row < descriptor.Work.Length; row++)
            {
                var work = descriptor.Work[row];
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(CreateHeaderCell(0, row+1, text: work.Name, fg: LabelForegrounds[row % LabelForegrounds.Length], bg: LabelBackgrounds[row % LabelBackgrounds.Length]));
            }

            for (var row = 0; row < descriptor.Work.Length; row++)
            {
                for (var col = 0; col < descriptor.Ranges.Length; col++)
                {
                    grid.Children.Add(CreateContentCell(col + 1, row + 1));
                }

                var work = descriptor.Work[row];
                grid.Children.Add(CreateWorkCell(work.Start + 1, work.End - work.Start + 1, row + 1, work.Progress));
            }
        }

        private static UIElement CreateWorkCell(int startCol, int span, int row, double pctComplete)
        {
            Brush brush;

            if (pctComplete == 0)
            {
                brush = new SolidColorBrush(IncompleteProgress);
            }
            else if (Math.Abs(pctComplete - 1) < .001)
            {
                brush = new SolidColorBrush(CompleteProgress);
            }
            else
            {
                var stops = new GradientStopCollection()
                {
                    new GradientStop(CompleteProgress, 0),
                    new GradientStop(CompleteProgress, pctComplete),
                    new GradientStop(IncompleteProgress, pctComplete),
                    new GradientStop(IncompleteProgress, 1),
                };
                brush = new LinearGradientBrush(stops, 0);
            }

            var border = new Border
            {
                Background= brush,
                Margin = new Thickness(5)
            };
            Grid.SetColumn(border, startCol);
            Grid.SetColumnSpan(border, span);
            Grid.SetRow(border, row);

            return border;
        }

        private static UIElement CreateContentCell(int column, int row)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.LightGray),
                BorderThickness = new Thickness(column == 0 ? 1 : 0, row == 0 ? 1 : 0, 1, 1)
            };
            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);

            return border;
        }

        private static Border CreateHeaderCell(int column, int row, Color? bg = null, string text = null, Color? fg = null)
        {
            var border = new Border();

            if (bg != null)
            {
                border.Background = new SolidColorBrush(bg.Value);
            }

            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);

            if (text != null)
            {
                border.Child = new Label()
                {
                    Foreground = new SolidColorBrush(fg.GetValueOrDefault(Colors.Black)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontFamily = new FontFamily("Segoe UI"),
                    Content = text
                };
            }

            return border;
        }
    }
}
