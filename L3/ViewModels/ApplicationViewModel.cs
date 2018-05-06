using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using L3.ViewModels;
using L3.Model;
using System.IO;
using System.Text;

namespace L3
{
    public class ApplicationViewModel : BaseNotifiableViewModel
    {
        public ICommand StartCommand { get; private set; }
        public ICommand ContinueCommand { get; private set; }
        public ICommand StepForwardCommand { get; private set; }
        public ICommand ClearPopulationCommand { get; private set; }

        public StepsViewModel Steps { get; private set; }

        private StepItemViewModel _currentStep;
        private const string srcFilePath = @"..\..\src.txt";

        public StepItemViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
            }
        }

        private PopulationTSP _population;
        private CitiesService _citiesService;

        public ApplicationViewModel()
        {
            Steps = new StepsViewModel();

            StartCommand = new DelegateCommand(obj => Start());
            ContinueCommand = new DelegateCommand(obj => Continue());
            StepForwardCommand = new DelegateCommand(obj => StepForward());
            ClearPopulationCommand = new DelegateCommand(obj => ClearPopulation());

            _citiesService = new CitiesService(srcFilePath);
            //var path = new int[] { 1, 8, 38, 31, 44, 18, 7, 28, 6, 37, 19, 27, 17, 43, 30, 36, 46, 33, 20, 47, 21, 32, 39, 48, 5, 42, 24, 10, 45, 35, 4, 26, 2, 29, 34, 41, 16, 22, 3, 23, 14, 25, 13, 11, 12, 15, 40, 9 };
            //var cities = new City[path.Length];
            //for (int i = 0; i < path.Length; i++)
            //{
            //    var city = _citiesService.Cities.First(c => c.number == (path[i] - 1));
            //    cities[i] = city;
            //}
            //var dist = CountSumDistance(cities);
            //int a = 0;
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
        #endregion

        private void Continue()
        {
            for (var i = 0; i < _continueStepsCount; i += _snapshotStepsCount)
            {
                var nextStep = NextStep(_snapshotStepsCount);
                Steps.Items.Add(nextStep);

                nextStep.Prev = _currentStep;
                _currentStep.Next = nextStep;

                CurrentStep = nextStep;
            }
        }

        private void Start()
        {
            _population = new PopulationTSP(_crossoverProbability, _mutationProbability, _citiesService, _populationCapacity);
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
        }

        private StepItemViewModel NextStep(int stepsCount)
        {
            var result = _population.StepForward(stepsCount);
            var nextStep = new StepItemViewModel {
                Name = string.Format("Step {0} Population count: {1}", (Steps.Items.Count + 1), _population.Count()),
                BestSolution = string.Join(" ", result.Select(c => c.number + 1)),
                SumDistance = CountSumDistance(result) 
            };

            return nextStep;
        }

        private double CountSumDistance(City[] cities)
        {
            double dist = 0;
            for (int i = 1; i < cities.Length; i++)
            {
                dist += _citiesService.GetDistance(cities[i], cities[i - 1]);
            }
            dist += _citiesService.GetDistance(cities[cities.Length - 1], cities[0]);
            return dist;
        }
    }
}
