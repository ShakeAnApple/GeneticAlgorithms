using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System.Text;
using L4.Model;
using OxyPlot;
using OxyPlot.Series;

namespace L4.ViewModels
{
    public class ApplicationViewModel : BaseNotifiableViewModel
    {
        public ICommand StartCommand { get; private set; }
        public ICommand ContinueCommand { get; private set; }
        public ICommand StepForwardCommand { get; private set; }
        public ICommand ClearPopulationCommand { get; private set; }

        public StepsViewModel Steps { get; private set; }

        private StepItemViewModel _currentStep;
        private Random _r;

        private const int VARS_COUNT = 3;
        //private readonly Range _range = new Range(-5.536, 65.536);
        private readonly Range _range = new Range(-5, 65);
        private string[] _terminals;
        private Dictionary<string, List<Measure>> _measuresByDimentiones;

        public StepItemViewModel CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
            }
        }

        private PopulationFunctions _population;

        public ApplicationViewModel()
        {
            _r = new Random();
            _terminals = GenerateTerminals();
            _measuresByDimentiones = GenerateMeasures();

            var plots = new List<PlotModel>();

            for (int i = 0; i < _measuresByDimentiones.Keys.Count; i++)
            {
                var plot = new PlotModel {
                    Title = _terminals[i]
                };

                var targetFuncResults = new List<Measure>();
                foreach (var measure in _measuresByDimentiones[_terminals[i]])
                {
                    measure.TargetFuncResult = FunctionHelper.CalculateTargetFunctionValue(measure);
                    targetFuncResults.Add(measure);
                }

                //plot.Series.Add(new FunctionSeries(PlotHelper.GetFunctions()[i], _range.start, _range.end, 0.01));
                plot.Series.Add(new ScatterSeries {
                    MarkerType = MarkerType.Circle,
                    MarkerFill = OxyColors.Fuchsia,
                    MarkerSize = 3,
                    Title = "target",
                    ItemsSource = new List<ScatterPoint>(targetFuncResults.Select(r => new ScatterPoint(r.TermValues[_terminals[i]], r.TargetFuncResult)))
                });
                plot.Series.Add(new ScatterSeries {
                    MarkerType = MarkerType.Triangle,
                    MarkerFill = OxyColors.Blue,
                    MarkerSize = 3
                });

                plots.Add(plot);
            }

            Plots = plots;

            Steps = new StepsViewModel();

            StartCommand = new DelegateCommand(obj => Start());
            ContinueCommand = new DelegateCommand(obj => Continue());
            StepForwardCommand = new DelegateCommand(obj => StepForward());
            ClearPopulationCommand = new DelegateCommand(obj => ClearPopulation());

        }

        private string[] GenerateTerminals()
        {
            var terminals = new string[VARS_COUNT];

            for (int i = 0; i < VARS_COUNT; i++)
            {
                terminals[i] = string.Format("x[{0}]", i);    
            }
            return terminals;
        }

        #region dynamic properties
        List<PlotModel> _plots;
        public List<PlotModel> Plots
        {
            get
            {
                return _plots;
            }
            private set { _plots = value;
                base.OnPropertyChanged("Plots");
            }
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

        private double _crossoverProbability = 0.4;
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

        private string _bestSolution;
        public string BestSolution
        {
            get { return _bestSolution; }
            set
            {
                _bestSolution = value;
                base.OnPropertyChanged("BestSolution");
            }
        }

        private string _bestSolutionTree;
        public string BestSolutionTree
        {
            get { return _bestSolutionTree; }
            set
            {
                _bestSolutionTree = value;
                base.OnPropertyChanged("BestSolutionTree");
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


        private Dictionary<string, List<Measure>> GenerateMeasures()
        {
            var measuresByD = new Dictionary<string, List<Measure>>();
            var step = 0.5;

            for (int i = 0; i < _terminals.Length; i++)
            {
                var measures = new List<Measure>();
                for (double j = _range.start; j < _range.end; j+=step)
                {
                    var termVals = new Dictionary<string, double>();
                    foreach (var term in _terminals)
                    {
                        if (term == _terminals[i])
                        {
                            termVals.Add(term, j);
                        }
                        else
                        {
                            termVals.Add(term, (double)((_r.Next(700) - 50) / 10));
                        }
                    }
                    measures.Add(new Measure(termVals, _terminals[i]));
                }
                measuresByD.Add(_terminals[i], measures);
            }
            return measuresByD;
        }

        private void Start()
        {
            int maxDepth = 5;
            _population = new PopulationFunctions(1, _crossoverProbability, _mutationProbability, _populationCapacity, _terminals, maxDepth, _range, _measuresByDimentiones);
        }

        private void StepForward()
        {
            StepItemViewModel nextStep;

            if (_currentStep == null || _currentStep.Next == null)
            {
                nextStep = NextStep(_forwardStepsCount);
                Steps.Items.Add(nextStep);
                RedrawStep(nextStep);

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
                BestSolution = string.Format("{0}\n val: {1}",result.Function.ToString(), result.FitnesFunctionVal),
                MeasuresByDimention = result.MeasuresByDimention
            };

            BestSolution = nextStep.BestSolution;
            BestSolutionTree = result.Function.ExprRoot.CollectTree(n => n.GetChildren(), n => n.ToString());

            return nextStep;
        }

        private void RedrawStep(StepItemViewModel step)
        {
            foreach (var plot in Plots)
            {
                var scatterSeries = plot.Series.FirstOrDefault(s => (s is ScatterSeries) && s.Title != "target");

                var points = step.MeasuresByDimention[plot.Title].Select(m => new ScatterPoint(m.TermValues[plot.Title], m.HypothesisFuncResult)).ToList();
                var plotData = new List<ScatterPoint>(points);
                
                if (scatterSeries != null)
                {
                    (scatterSeries as ScatterSeries).ItemsSource = plotData;
                }
            }

            var P = Plots;
            Plots = null;
            this.OnPropertyChanged("Plots");
            Plots = P;
            this.OnPropertyChanged("Plots");
        }
    }
}
