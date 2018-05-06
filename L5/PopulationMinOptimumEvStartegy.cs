using GeneticAlgorithms.Core;
using L5.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace L5
{
    public class PopulationMinOptimumEvStartegy : Population<double, Point, double>
    {
        private int _dimentionsCount;
        private const double SIGMA = 0.3;

        private readonly int _chromosomeLength;
        private readonly double _minX, _maxX;

        private readonly NormalRandom _nr;

        public PopulationMinOptimumEvStartegy(int dimentionsCount, int chromosomeLength,
            double minX, double maxX)
            : base(chromosomeLength, 0, 1)
        {
            _dimentionsCount = dimentionsCount;
            _chromosomeLength = chromosomeLength;
            _minX = minX;
            _maxX = maxX;
            _nr = new NormalRandom();
        }

        public PopulationMinOptimumEvStartegy(List<Point> solutions,
            int chromosomeLength,
            double crossoverProbaility, double mutationProbability,
            double minX, double maxX)
            : base(chromosomeLength, crossoverProbaility, mutationProbability)
        {
            _chromosomeLength = chromosomeLength;
            _minX = minX;
            _maxX = maxX;

            base.Fill(solutions);
        }

        private List<Point> GenerateStartPopulation(int capacity)
        {
            var solutions = new List<Point>();
            var r = new Random();
            for (int i = 0; i < capacity; i++)
            {
                var coords = Enumerable.Range(0, _dimentionsCount).Select(n => (r.NextDouble() * (_maxX - _minX) + _minX)).ToArray();
                var z = FunctionHelper.CalculateFunctionValue(coords);

                solutions.Add(new Point(coords, z));
            }
            return solutions;
        }

        public void GenerateAndFill(int capacity)
        {
            var solutions = GenerateStartPopulation(capacity);
            base.Fill(solutions);
        }

        protected override Dictionary<double, double> CalculateRelativeOptimality(List<double> funcResults)
        {
            var res = new Dictionary<double, double>();
            res.Add(funcResults.First(), 1);
            return res;
            
            //funcResults = funcResults.Distinct().OrderBy(f => f).ToList();

            //var min = funcResults.Min();
            //var max = funcResults.Max();

            //var normalizedRes = funcResults.Select(res => new {
            //    Res = res,
            //    NRes = 1 - ((res - min) / max)
            //}).ToArray();

            //var probabilitiesBySolution = new Dictionary<double, double>();
            //double sum = 0;
            //foreach (var solution in normalizedRes)
            //{
            //    sum += solution.NRes;
            //}

            //foreach (var solution in normalizedRes)
            //{
            //    probabilitiesBySolution.Add(solution.Res, solution.NRes / sum);
            //}

            //return probabilitiesBySolution;
        }

        protected override Point Decode(Gene<double>[] genes)
        {
            return new Point(genes.Select(g => g.Value).ToArray());
        }

        protected override Gene<double>[] Encode(Point solution)
        {
            var res = new Gene<double>[_chromosomeLength];
            for (int i = 0; i < solution.coords.Length; i++)
            {
                res[i] = new Gene<double>(solution.coords[i]);
            }
            return res;
        }

        public override double CalculateFunctionValue(Point arg)
        {
            return FunctionHelper.CalculateFunctionValue(arg.coords);
        }

        public Point StepForward(int stepsCount)
        {
            for (var i = 0; i < stepsCount; i++)
            {
                base.DoReproduction();
                base.DoCrossover();
                base.DoMutations();
            }

            return this.First();
        }

        protected override Tuple<Gene<double>[], Gene<double>[]> CrossoverFunction(Chromosome ch1, Chromosome ch2)
        {
            var genes1 = ch1.Genes;
            var genes2 = ch2.Genes;
            return new Tuple<Gene<double>[], Gene<double>[]>(genes1.ToArray(), genes2.ToArray());
        }

        protected override Gene<double>[] MutationFunction(Gene<double>[] genes)
        {
            var newCoords = new double[genes.Length];
            for(int i = 0; i < genes.Length; i++)
            {
                var newX = genes[i].Value + _nr.NextDouble() * SIGMA;
                newCoords[i] = newX;
            }

            var newVal = FunctionHelper.CalculateFunctionValue(newCoords);
            var oldVal = FunctionHelper.CalculateFunctionValue(genes.Select(g => g.Value).ToArray());
            if (newVal < oldVal)
            {
                return newCoords.Select(c => new Gene<double>(c)).ToArray();
            }
            else
            {
                return genes;
            }
        }

        public void Clear()
        {
            this.Fill(new List<Point>());
        }
    }
}
