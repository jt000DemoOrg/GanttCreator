using System;
using System.Windows;

namespace Mechavian.WpfHelpers
{
    public class Mvvm
    {
        public static readonly DependencyProperty ViewModelTypeProperty = DependencyProperty.RegisterAttached("ViewModelType", typeof(Type), typeof(Mvvm), new PropertyMetadata(default(Type), OnViewModelTypeChanged));

        public static void SetViewModelType(FrameworkElement element, Type value)
        {
            element.SetValue(ViewModelTypeProperty, value);
        }

        public static Type GetViewModelType(FrameworkElement element)
        {
            return (Type)element.GetValue(ViewModelTypeProperty);
        }

        private static void OnViewModelTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModelType = e.NewValue as Type;
            var frameworkElement = d as FrameworkElement;

            if (frameworkElement == null)
                return;

            if (viewModelType == null)
            {
                if (frameworkElement.DataContext is ViewModel viewModel)
                {
                    viewModel.Parent = null;
                }

                frameworkElement.DataContext = null;
            }
            else
            {
                frameworkElement.DataContext = Activator.CreateInstance(viewModelType);
                if (frameworkElement.DataContext is ViewModel viewModel)
                {
                    viewModel.Parent = frameworkElement;
                }
            }
        }
    }
}