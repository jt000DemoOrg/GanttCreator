using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace Mechavian.GanttControls
{
    public static class UIElementExtensions
    {
        public static XmlDocument ToSvg(this UIElement uiElement)
        {
            var doc = new XmlDocument();

            var renderSize = uiElement.RenderSize;
            var svg = CreateSvgElement(doc, renderSize.Width, renderSize.Height);
            doc.AppendChild(svg);

            var objectsToProcess = new Queue<DependencyObject>();
            objectsToProcess.Enqueue(uiElement);

            while (objectsToProcess.Count > 0)
            {
                var depObj = objectsToProcess.Dequeue();

                if (depObj is TextBlock textBlock)
                {
                    var fontFamily = textBlock.FontFamily.Source;
                    var fontSize = textBlock.FontSize;
                    var fillColor = textBlock.Foreground;
                    var bounds = textBlock.TransformToAncestor(uiElement).TransformBounds(new Rect(0, 0, textBlock.ActualWidth, textBlock.ActualHeight));
                    var val = textBlock.Text;

                    var text = CreateTextElement(doc, val, bounds.X, bounds.Y + bounds.Height/2, fontFamily, fontSize, fillColor);
                    svg.AppendChild(text);
                }

                if (depObj is Border border)
                {
                    var bounds = border.TransformToAncestor(uiElement).TransformBounds(new Rect(0, 0, border.ActualWidth, border.ActualHeight));
                    

                    var rect = CreateRectElement(doc, bounds.X, bounds.Y, bounds.Width, bounds.Height, border.Background, border.BorderBrush, border.BorderThickness, border.CornerRadius);
                    svg.AppendChild(rect);
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    objectsToProcess.Enqueue(VisualTreeHelper.GetChild(depObj, i));
                }
            }

            
            return doc;
        }

        private static XmlElement CreateRectElement(XmlDocument doc, double x, double y, double width, double height, Brush fill, Brush stroke, Thickness strokeWidth, CornerRadius cornerRadius)
        {
            var xmlElem = doc.CreateElement("rect");

            xmlElem.SetAttribute("x", x.ToString("G"));
            xmlElem.SetAttribute("y", y.ToString("G"));
            xmlElem.SetAttribute("width", width.ToString("G"));
            xmlElem.SetAttribute("height", height.ToString("G"));

            if (fill is SolidColorBrush solidFillBrush)
            {
                xmlElem.SetAttribute("fill", ToRGBA(solidFillBrush.Color));
            }
            else
            {
                //throw new InvalidOperationException($"Unhandled brush type '{fill.GetType().Name}'");
            }

            if (stroke is SolidColorBrush solidStrokeBrush)
            {
                xmlElem.SetAttribute("stroke", ToRGBA(solidStrokeBrush.Color));
            }
            else
            {
                //throw new InvalidOperationException($"Unhandled brush type '{stroke.GetType().Name}'");
            }

            xmlElem.SetAttribute("stroke-width", strokeWidth.Top.ToString("G"));

            return xmlElem;
        }

        private static XmlElement CreateTextElement(XmlDocument doc, string val, double x, double y, string fontFamily, double fontSize, Brush fill)
        {
            var xmlElem = doc.CreateElement("text");
            
            xmlElem.InnerText = val;
            xmlElem.SetAttribute("x", x.ToString("G"));
            xmlElem.SetAttribute("y", y.ToString("G"));
            xmlElem.SetAttribute("font-family", fontFamily);
            xmlElem.SetAttribute("font-size", fontSize.ToString("G"));

            if (fill is SolidColorBrush solidColorBrush)
            {
                xmlElem.SetAttribute("fill", ToRGBA(solidColorBrush.Color));
            }
            else
            {
                throw new InvalidOperationException($"Unhandled brush type '{fill.GetType().Name}'");
            }

            return xmlElem;
        }

        private static string ToRGBA(Color color)
        {
            return $"rgba({color.R:G},{color.G:G},{color.B:G},{color.ScA:#.#})";
        }

        private static XmlElement CreateSvgElement(XmlDocument doc, double width, double height)
        {
            var xmlElem = doc.CreateElement("svg");

            xmlElem.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
            xmlElem.SetAttribute("width", width.ToString("G"));
            xmlElem.SetAttribute("height", width.ToString("G"));
            xmlElem.SetAttribute("viewBox", $"0 0 {width:G} {height:G}");

            return xmlElem;
        }
    }
}
