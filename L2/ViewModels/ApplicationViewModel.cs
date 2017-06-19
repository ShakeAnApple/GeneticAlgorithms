using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using L2.ViewModels;
using L2.Model;
using System.IO;
using System.Text;

namespace L2
{
    public class ApplicationViewModel : BaseNotifiableViewModel
    {
        public ICommand StartCommand { get; private set; }
        public ICommand ContinueCommand { get; private set; }
        public ICommand StepForwardCommand { get; private set; }
        public ICommand ClearPopulationCommand { get; private set; }

        public StepsViewModel Steps { get; private set; }

        private StepItemViewModel _currentStep;
        private int _forwardStepsCount = 1;
        private const string stepsFilePath = @"c:\Temp\out.txt";

        public StepItemViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                AppendStep(_currentStep);
            }
        }


        private PopulationMinOptimum _population;

        public ApplicationViewModel()
        {
            Steps = new StepsViewModel();

            StartCommand = new DelegateCommand(obj => Start());
            ContinueCommand = new DelegateCommand(obj => Continue());
            StepForwardCommand = new DelegateCommand(obj => StepForward());
            ClearPopulationCommand = new DelegateCommand(obj => ClearPopulation());
        }


        #region dynamic properties
        private int _populationCapacity = 50;
        public int PopulationCapacity
        {
            get { return _populationCapacity; }
            set
            {
                _populationCapacity = value;
                base.OnPropertyChanged("PopulationCapacity");
            }
        }

        private int _continueStepsCount;
        public int ContinueStepsCount
        {
            get { return _continueStepsCount; }
            set
            {
                if (value <= 0) { _continueStepsCount = 1; }
                else
                {
                    _continueStepsCount = value;
                }
                base.OnPropertyChanged("ContinueStepsCount");
            }
        }

        private double _crossoverProbability = 0.5;
        public double CrossoverProbability
        {
            get
            {
                return _crossoverProbability;
            }
            set
            {
                if (value > 1) { _crossoverProbability = 1; }
                else if (value < 0) { _crossoverProbability = 0; }
                else
                {
                    _crossoverProbability = value;
                }
            }
        }

        private double _mutationProbability = 0;
        public double MutationProbability
        {
            get
            {
                return _mutationProbability;
            }
            set
            {
                if (value > 1) { _mutationProbability = 1; }
                else if (value < 0) { _mutationProbability = 0; }
                else
                {
                    _mutationProbability = value;
                }
            }
        }
        #endregion


        private void Continue()
        {
            for (var i = 0; i < _continueStepsCount; i ++)
            {
                var nextStep = NextStep(_forwardStepsCount);
                Steps.Items.Add(nextStep);

                nextStep.Prev = _currentStep;
                _currentStep.Next = nextStep;

                CurrentStep = nextStep;
            }
        }

        private void Start()
        {
            _population = new PopulationMinOptimum(3, _crossoverProbability, _mutationProbability, -65.536, 65.536);
            _population.GenerateAndFill(_populationCapacity);

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            var str = string.Format("{{{0}}}",
                            string.Join(", ", _population.Select(res => string.Format("{{{0}}}", string.Join(",", res.coords)))));

            File.AppendAllText(stepsFilePath, str + Environment.NewLine);

        }

        private void StepForward()
        {
            StepItemViewModel nextStep;

            if (_currentStep == null || _currentStep.Next == null)
            {
                nextStep = NextStep(_forwardStepsCount);
                Steps.Items.Add(nextStep);

                nextStep.Prev = _currentStep;
                if (_currentStep != null)
                {
                    _currentStep.Next = nextStep;
                }
            }
            else
            {
                nextStep = _currentStep.Next;
            }

            CurrentStep = nextStep;
        }

        private void ClearPopulation()
        {
            _population.Clear();
            Steps.Items.Clear();
            if (File.Exists(stepsFilePath))
            {
                File.Delete(stepsFilePath);
            }
        }

        private StepItemViewModel NextStep(int stepsCount)
        {
            var result = _population.StepForward(stepsCount);
            var nextStep = new StepItemViewModel {
                Name = String.Format("Step {0} Population count: {1}", (Steps.Items.Count + 1), _population.Count()),
                Data = result,
            };

            return nextStep;
        }

        private void AppendStep(StepItemViewModel step)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            var str = string.Format("{{{0}}}", 
                            string.Join(", ", _currentStep.Data.Select(res => string.Format("{{{0}}}", string.Join(",", res.coords)))));

            File.AppendAllText(stepsFilePath, str + Environment.NewLine);
        }
    }
}
