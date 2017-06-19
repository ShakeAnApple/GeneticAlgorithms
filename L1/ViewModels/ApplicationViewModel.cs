using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;

namespace L1.ViewModels
{
    public class ApplicationViewModel : BaseNotifiableViewModel
    {
        public ICommand StartCommand { get; private set; }
        public ICommand ContinueCommand { get; private set; }
        public ICommand StepForwardCommand { get; private set; }
        public ICommand StepBackwardCommand { get; private set; }
        public ICommand ClearPopulationCommand { get; private set; }

        public StepsViewModel Steps { get; private set; }

        private StepItemViewModel _currentStep;
        public StepItemViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                RedrawStep(_currentStep);
            }
        }

        private PopulationMaxFunc _population;

        public ApplicationViewModel()
        {
            Steps = new StepsViewModel(this);

            Plot = new PlotModel();

            Plot.Series.Add(new FunctionSeries(FunctionHelper.CalculateFunctionValue, 3.1, 20, 0.001));
            Plot.Series.Add(new ScatterSeries {
                MarkerType = MarkerType.Circle
            });

            StartCommand = new DelegateCommand(obj => Start());
            ContinueCommand = new DelegateCommand(obj => Continue());
            StepForwardCommand = new DelegateCommand(obj => StepForward());
            StepBackwardCommand = new DelegateCommand(obj => StepBackward());
            ClearPopulationCommand = new DelegateCommand(obj => ClearPopulation());
        }

        #region dynamic properties
        PlotModel _plot;
        public PlotModel Plot
        {
            get
            {
                return _plot;
            }
            private set { _plot = value; }
        }


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

        private int _forwardStepsCount = 1;
        public int ForwardStepsCount
        {
            get { return _forwardStepsCount; }
            set
            {
                if (value <= 0) { _forwardStepsCount = 1; }
                else
                {
                    _forwardStepsCount = value;
                }
                base.OnPropertyChanged("ForwardStepsCount");
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

        private int _snapshotStepsCount;
        public int SnapshotStepsCount
        {
            get { return _snapshotStepsCount; }
            set
            {
                if (value < 0) { _snapshotStepsCount = _continueStepsCount; }
                if (_snapshotStepsCount > _continueStepsCount)
                {
                    ContinueStepsCount = _snapshotStepsCount;
                }
                else
                {
                    _snapshotStepsCount = value;
                }
                base.OnPropertyChanged("SnapshotStepsCount");
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

        private double _mutationProbability = 0.01;
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
            for (var i = 0; i < _continueStepsCount; i += _snapshotStepsCount)
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
            _population = new PopulationMaxFunc(11, _crossoverProbability, _mutationProbability, 3.1, 20);
            _population.GenerateAndFill(_populationCapacity);
        }

        private void StepBackward()
        {
            if (_currentStep.Prev != null)
            {
                CurrentStep = _currentStep.Prev;
            }
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
        }

        private StepItemViewModel NextStep(int stepsCount)
        {
            var result = _population.StepForward(stepsCount);
            var nextStep = new StepItemViewModel {
                Name = String.Format("Step {0} Population count: {1}", (Steps.Items.Count + 1), _population.Count()),
                Data = new List<ScatterPoint>(result.Select(x => new ScatterPoint(x, _population.CalculateFunctionValue(x)))),
            };

            return nextStep;
        }

        private void RedrawStep(StepItemViewModel step)
        {
            var scatterSeries = Plot.Series.FirstOrDefault(s => s is ScatterSeries);
            if (scatterSeries != null)
            {
                (scatterSeries as ScatterSeries).ItemsSource = step.Data;
            }
            
            var P = Plot;
            Plot = null;
            this.OnPropertyChanged("Plot");
            Plot = P;
            this.OnPropertyChanged("Plot");
        }
    }
}
