using GeneticAlgorithms.Core;
using L2.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace L2
{
    public class PopulationMinOptimum : Population<double, Point, double>
    {
        private const int D = 2;
        private const double W = 0.4;

        private readonly int _chromosomeLength;
        private readonly double _minX, _maxX;

        public PopulationMinOptimum(int chromosomeLength,
            double crossoverProbaility, double mutationProbability,
            double minX, double maxX)
            : base(chromosomeLength, crossoverProbaility, mutationProbability)
        {
            _chromosomeLength = chromosomeLength;
            _minX = minX;
            _maxX = maxX;
        }

        public PopulationMinOptimum(List<Point> solutions,
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
                var coords = Enumerable.Range(0, D).Select(n => (r.NextDouble() * (_maxX - _minX) + _minX)).ToArray();
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
            funcResults = funcResults.Distinct().OrderBy(f => f).ToList();

            var min = funcResults.Min();
            var max = funcResults.Max();

            var normalizedRes = funcResults.Select(res => new {
                Res = res,
                NRes = 1 - ((res - min) / max)
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

        protected override Point Decode(Gene<double>[] genes)
        {
            return new Point(genes[0].Value, genes[1].Value, genes[2].Value);
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
            return arg.coords[arg.coords.Length - 1];
        }

        public List<Point> StepForward(int stepsCount)
        {
            for (var i = 0; i < stepsCount; i++)
            {
                base.DoReproduction();
                base.DoCrossover();
                base.DoMutations();
            }

            return this.ToList();
        }

        protected override Tuple<Gene<double>[], Gene<double>[]> CrossoverFunction(Chromosome ch1, Chromosome ch2)
        {
            var genes1 = ch1.Genes;
            var genes2 = ch2.Genes;

            var newGenes1 = new Gene<double>[_chromosomeLength];
            var newGenes2 = new Gene<double>[_chromosomeLength];

            for (int i = 0; i < genes1.Count - 1; i++)
            {
                newGenes1[i] = new Gene<double>(W * (genes1[i].Value) + (1 - W) * genes2[i].Value);
                newGenes2[i] = new Gene<double>((1 - W) * (genes1[i].Value) + W * genes2[i].Value);
            }

            newGenes1[_chromosomeLength - 1] = new Gene<double>(
                FunctionHelper.CalculateFunctionValue(
                    newGenes1.Take(_chromosomeLength - 1).Select(g => g.Value).ToArray()
                )
            );
            newGenes2[_chromosomeLength - 1] = new Gene<double>(
                FunctionHelper.CalculateFunctionValue(
                    newGenes2.Take(_chromosomeLength - 1).Select(g => g.Value).ToArray()
                )
            );

            return new Tuple<Gene<double>[], Gene<double>[]>(newGenes1, newGenes2);
        }

        protected override Gene<double>[] MutationFunction(Gene<double>[] genes)
        {
            var r = new Random();

            var mutationPoint = r.Next(0, genes.Count() - 1);
            genes[mutationPoint] = new Gene<double>(r.NextDouble() * (_maxX - _minX) + _minX);

            var newFuncVal = FunctionHelper.CalculateFunctionValue(genes.Take(genes.Count() - 1).Select(g => g.Value).ToArray());
            genes[genes.Count() - 1] = new Gene<double>(newFuncVal);

            return genes;
        }

        public void Clear()
        {
            this.Fill(new List<Point>());
        }
    }
}
