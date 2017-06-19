using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace L2.ViewModels
{
    public abstract class BaseNotifiableViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName]string propName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
