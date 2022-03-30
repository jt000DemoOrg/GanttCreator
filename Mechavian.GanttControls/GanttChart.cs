using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mechavian.GanttControls.Models;
using Mechavian.WpfHelpers.Linq;

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

            // Header
            var headers = GanttHeaderCell.ProcessRanges(descriptor.Ranges);
            var headerRowCount = headers.Values.Max(h => h.Row + h.RowSpan);
            var headerColumnCount = headers.Values.Max(h => h.Column + h.ColumnSpan);
            Enumerable.Range(0, headerRowCount).ForEach((_) => grid.RowDefinitions.Add(new RowDefinition()));
            Enumerable.Range(0, headerColumnCount).ForEach((_) => grid.ColumnDefinitions.Add(new ColumnDefinition()));

            headers.Values.ForEach((header) => grid.Children.Add(CreateColumnHeaderCell(header)));

            for (var row = 0; row < descriptor.Work.Length; row++)
            {
                Enumerable.Range(0, headerColumnCount).ForEach((col) => grid.Children.Add(CreateContentCell(col + 1, row + headerRowCount)));

                var work = descriptor.Work[row];

                // Work Item header column
                grid.RowDefinitions.Add(new RowDefinition());
                grid.Children.Add(CreateHeaderCell(0, row + headerRowCount, text: work.Name, fg: LabelForegrounds[row % LabelForegrounds.Length], bg: LabelBackgrounds[row % LabelBackgrounds.Length]));

                // Work Item range item
                if (headers.TryGetValue(work.Start.Id, out var startHeader)
                    && headers.TryGetValue(work.End.Id, out var endHeader))
                {
                    var spanCount = endHeader.Column + endHeader.ColumnSpan - startHeader.Column;
                    grid.Children.Add(CreateWorkCell(startHeader.Column + 1, spanCount, row + headerRowCount, work.Progress));
                }
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

        private static Border CreateColumnHeaderCell(GanttHeaderCell headerCell)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(HeaderBackground)
            };

            Grid.SetColumn(border, headerCell.Column + 1);
            Grid.SetColumnSpan(border, headerCell.ColumnSpan);
            Grid.SetRow(border, headerCell.Row);
            Grid.SetRowSpan(border, headerCell.RowSpan);

            border.Child = new Label()
            {
                Foreground = new SolidColorBrush(HeaderForeground),
                HorizontalAlignment = HorizontalAlignment.Center,
                FontFamily = new FontFamily("Segoe UI"),
                Content = headerCell.GanttRange.Name
            };

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
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontWeight = FontWeights.Bold,
                    Content = text
                };
            }

            return border;
        }
    }

    public class GanttHeaderCell
    {
        public GanttRange GanttRange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GanttHeaderCell" /> class.
        /// </summary>
        public GanttHeaderCell(GanttRange ganttRange)
        {
            this.GanttRange = ganttRange;
        }

        public int Column { get; set; }

        public int ColumnSpan { get; set; }

        public int Row { get; set; }

        public int RowSpan { get; set; }

        public Guid Id { get => this.GanttRange.Id; }

        public static Dictionary<Guid, GanttHeaderCell> ProcessRanges(IEnumerable<GanttRange> ranges)
        {
            var headers = new Dictionary<Guid, GanttHeaderCell>();
            ProcessRanges(headers, ranges, 1, 0, out _);

            return headers;
        }

        private static void ProcessRanges(Dictionary<Guid, GanttHeaderCell> headers, IEnumerable<GanttRange> ranges, int startCol, int startRow, out int endCol)
        {
            int column = startCol;
            foreach (var range in ranges)
            {
                var colSpan = 1;
                if (range.Children != null)
                {
                    ProcessRanges(headers, range.Children, column, startRow + 1, out int childEndCol);
                    colSpan = childEndCol - column;
                }

                var cell = new GanttHeaderCell(range)
                {
                    Column = column,
                    ColumnSpan = colSpan,
                    Row = startRow,
                    RowSpan = 1
                };

                headers.Add(cell.Id, cell);
                column += colSpan;
            }

            endCol = column;
        }

    }
}
