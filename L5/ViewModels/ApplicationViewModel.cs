using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using L5.ViewModels;
using L5.Model;
using System.IO;

namespace L5
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
        private const string stepsFilePath = @"c:\tmp\out_ga_la5.txt";

        private const int DIMENTIONS_COUNT = 2;

        public StepItemViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                AppendStep(_currentStep.Data);
            }
        }


        private PopulationMinOptimumEvStartegy _population;

        public ApplicationViewModel()
        {
            Steps = new StepsViewModel();

            StartCommand = new DelegateCommand(obj => Start());
            ContinueCommand = new DelegateCommand(obj => Continue());
            StepForwardCommand = new DelegateCommand(obj => StepForward());
            ClearPopulationCommand = new DelegateCommand(obj => ClearPopulation());
        }


        #region dynamic properties
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
            _population = new PopulationMinOptimumEvStartegy(DIMENTIONS_COUNT, DIMENTIONS_COUNT, -65.536, 65.536);
            _population.GenerateAndFill(1);

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            AppendStep(_population.First());
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
                BestSolution = result.ToString()
            };

            return nextStep;
        }

        private void AppendStep(Point data)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            var str = string.Format("{{{0}}}", string.Format("{{{0},{1}}}", string.Join(",", data.coords), data.result));

            File.AppendAllText(stepsFilePath, str + Environment.NewLine);
        }
    }
}
