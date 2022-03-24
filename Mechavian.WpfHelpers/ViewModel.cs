using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Mechavian.WpfHelpers.Annotations;

namespace Mechavian.WpfHelpers
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public FrameworkElement Parent { get; set; }
        public Window ParentWindow => Parent == null ? null : Parent as Window ?? Window.GetWindow(Parent);
        public Dispatcher Dispatcher => Parent == null ? Dispatcher.CurrentDispatcher : Parent.Dispatcher;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
