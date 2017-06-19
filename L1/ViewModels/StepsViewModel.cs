using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace L1.ViewModels
{
    public class StepsViewModel : BaseNotifiableViewModel
    {
        public ICommand SelectStepCommand { get; private set; }

        public ObservableCollection<StepItemViewModel> Items { get; set; }

        ApplicationViewModel _owner;

        public StepsViewModel(ApplicationViewModel owner)
        {
            _owner = owner;
            Items = new ObservableCollection<StepItemViewModel>();
        }
    }
}