using GeneticAlgorithms.Core;
using L4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4
{
    class PopulationFunctions : Population<Double, Function, Function>
    {
        string[] _terminals;
        int _maxDepth;
        Range _range;

        FunctionNodesFabric _ffabric;
        FunctionGenerator _fGen;

        Dictionary<string, List<Measure>> _measuresByDimention;


        public PopulationFunctions(int chromosomeLength, double crossoverProbability, double mutationProbability, int populationCapacity, string[] terminals, int maxDepth, Range range, Dictionary<string, List<Measure>> measuresByDimention) 
            : base(chromosomeLength, crossoverProbability, mutationProbability, populationCapacity)
        {
            _terminals = terminals;
            _maxDepth = maxDepth;
            _range = range;

            _measuresByDimention = measuresByDimention;

            _ffabric = new FunctionNodesFabric(_terminals);
            _fGen = new FunctionGenerator();

            this.GenerateAndFill(populationCapacity);
        }

        public void GenerateAndFill(int capacity)
        {
            var solutions = GenerateStartPopulation(capacity);
            base.Fill(solutions);
        }

        public List<Function> GenerateStartPopulation(int capacity)
        {
            var solutions = new List<Function>();
            for (var i = 0; i < (capacity) / 2; i++)
            {
                var solution = new Function(_fGen.GenerateFullInit(_ffabric, _maxDepth));
                while (CalculateFunctionValue(solution) == Int32.MaxValue)
                {
                    solution = new Function(_fGen.GenerateFullInit(_ffabric, _maxDepth));
                }
                solutions.Add(solution);
            }

            for (var i = 0; i < (capacity) / 2; i++)
            {
                var solution = new Function(_fGen.GenerateGrowthInit(_ffabric, _maxDepth));
                while (CalculateFunctionValue(solution) == Int32.MaxValue)
                {
                    solution = new Function(_fGen.GenerateGrowthInit(_ffabric, _maxDepth));
                }
                solutions.Add(solution);
            }

            return solutions;
        }


        public override double CalculateFunctionValue(Function func)
        {
            var deviations = new List<double>();
            foreach (var dimention in _measuresByDimention.Keys)
            {
                FunctionHelper.FillMeasures(_measuresByDimention[dimention], func);
                //double sum = 0;
                var devs = new List<double>();
                foreach (var measure in _measuresByDimention[dimention])
                {
                    var dev = Math.Abs(measure.HypothesisFuncResult - measure.TargetFuncResult);
                    if (Double.IsInfinity(dev))
                    {
                        dev = Int32.MaxValue;
                    }
                    devs.Add(dev);
                  //  sum += dev;
                }
                //deviations.Add(sum / _measuresByDimention[dimention].Count);
                deviations.Add(devs.Max());
            }

            var totalDev = deviations.Sum() / _measuresByDimention.Keys.Count;
            return totalDev > Int32.MaxValue ? Int32.MaxValue : totalDev;
        }

        protected override Dictionary<double, double> CalculateRelativeOptimality(List<double> funcResults)
        {
            funcResults = funcResults.Distinct().OrderBy(f => f).ToList();

            var min = funcResults.Min();
            var max = funcResults.Max();

            var normalizedRes = funcResults.Select(res => new {
                Res = res,
                NRes = (1 - (double)(res - min) / (max - min))
            }).ToArray();

            var probabilitiesBySolution = new Dictionary<double, double>();
            double sum = 0;
            foreach (var solution in normalizedRes)
            {
                sum += solution.NRes;
            }

            foreach (var solution in normalizedRes)
            {
                probabilitiesBySolution.Add(solution.Res, solution.NRes / sum);
            }

            return probabilitiesBySolution;
        }

        protected override Tuple<Gene<Function>[], Gene<Function>[]> CrossoverFunction(Chromosome ch1, Chromosome ch2)
        {
            var func1 = ch1.Genes[0].Value;
            var func2 = ch2.Genes[0].Value;

            var funcVal1 = CalculateFunctionValue(func1);
            var funcVal2 = CalculateFunctionValue(func2);

            var canReplace1 = false;
            var canReplace2 = false;

            INode n1 = null;
            INode n2 = null;

            Function res1 = null;
            Function res2 = null;

            var betterSolutionFound = false;
            int cc = 0;
            var resList = new List<Tuple<Function, double>>();
            while (!betterSolutionFound && cc < 1000)
            {
                while (!(canReplace1 && canReplace2))
                {
                    n1 = func1.GetRandomOp();
                    n2 = func2.GetRandomOp(n1.GetOpType());

                    while (n2 == null)
                    {
                        n1 = func1.GetRandomOp();
                        n2 = func2.GetRandomOp(n1.GetOpType());
                    }

                    int c = 0;
                    while (n2.Equals(n1) && c < 20)
                    {
                        n2 = func2.GetRandomOp(n1.GetOpType());
                        c++;
                    }

                    try
                    {
                        canReplace1 = func1.CanReplace(n1, n2, _maxDepth);
                        canReplace2 = func2.CanReplace(n2, n1, _maxDepth);
                    }
                    catch (Exception ex)
                    {
                        int a = 1;
                    }
                }

                res1 = func1.Clone();
                res1.ReplaceNode(n1, n2);

                res2 = func2.Clone();
                res2.ReplaceNode(n2, n1);


                var resFuncVal1 = CalculateFunctionValue(res1);
                var resFuncVal2 = CalculateFunctionValue(res2);

                if ((resFuncVal1 < funcVal1 || resFuncVal1 < funcVal2) && (resFuncVal2 < funcVal1 || resFuncVal2 < funcVal2))
                {
                    betterSolutionFound = true;
                }
                else
                {
                    resList.Add(new Tuple<Function, double>(res1, resFuncVal1));
                    resList.Add(new Tuple<Function, double>(res2, resFuncVal2));
                }
                cc++;
            }

            if (!betterSolutionFound)
            {
                resList = resList.OrderBy(r => r.Item2).ToList();
                res1 = resList.First().Item1;
                resList.RemoveAt(0);
                res2 = resList.First().Item1;
            }

            return new Tuple<Gene<Function>[], Gene<Function>[]>(
                new Gene<Function>[] { new Gene<Function>(res1) },
                new Gene<Function>[] { new Gene<Function>(res2) });
        }

        protected override Function Decode(Gene<Function>[] genes)
        {
            return genes[0].Value;
        }

        protected override Gene<Function>[] Encode(Function solution) 
        {
            return new Gene<Function>[] { new Gene<Function>(solution) };
        }

        protected override Gene<Function>[] MutationFunction(Gene<Function>[] genes)
        {
            var func = genes[0].Value;
            var funcVal = CalculateFunctionValue(func);

            INode randomOp = null;

            Function tempFunc = null;
            var betterSolutionFound = false;
            int c = 0;
            while (!betterSolutionFound && c<1000)
            {
                c++;
                tempFunc = func.Clone();
                randomOp = null;
                while (randomOp == null)
                {
                    randomOp = tempFunc.GetRandomOp();
                }

                _fGen.GenerateFromStartNode(_ffabric, randomOp.GetMaxDepth(0), randomOp);

                var tempFuncVal = CalculateFunctionValue(tempFunc);
                if (tempFuncVal < funcVal)
                {
                    betterSolutionFound = true;
                }
            }

            if (!betterSolutionFound)
            {
                tempFunc = func.Clone();
            }

            return new Gene<Function>[] { new Gene<Function>(tempFunc) };
        }

        public void Clear()
        {
            this.Clear();
        }

        


       
        
        public Result StepForward(int stepsCount)
        {
            for (var i = 0; i < stepsCount; i++)
            {
                base.DoReproduction();
                base.DoCrossover();
                base.DoMutations();
            }

            var bestSolution = this.OrderBy(r => CalculateFunctionValue(r)).First();

            foreach (var dimention in _measuresByDimention.Keys)
            {
                FunctionHelper.FillMeasures(_measuresByDimention[dimention], bestSolution);
            }

            return new Result {
                MeasuresByDimention = _measuresByDimention,
                Function = bestSolution,
                FitnesFunctionVal = CalculateFunctionValue(bestSolution)
            };
        }
    }
}
