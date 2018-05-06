using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace L4.ViewModels
{
    public class StepsViewModel : BaseNotifiableViewModel
    {
        public ICommand SelectStepCommand { get; private set; }

        public ObservableCollection<StepItemViewModel> Items { get; set; }

        public StepsViewModel()
        {
            Items = new ObservableCollection<StepItemViewModel>();
        }
    }
}