using GeneticAlgorithms.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace L1
{
    public class PopulationMaxFunc : Population<double, double, byte>
    {
        private readonly int _chromosomeLength;
        private readonly double _minX, _maxX;


        public PopulationMaxFunc(int chromosomeLength, 
            double crossoverProbaility, double mutationProbability, 
            double minX, double maxX) 
            : base(chromosomeLength, crossoverProbaility, mutationProbability)
        {
            _chromosomeLength = chromosomeLength;
            _minX = minX;
            _maxX = maxX;
        }

        public PopulationMaxFunc(List<double> solutions, 
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

        private List<double> GenerateStartPopulation(int capacity)
        {
            var solutions = new List<double>();
            var r = new Random();
            for (int i = 0; i < capacity; i++)
            {
                solutions.Add(r.NextDouble() * (_maxX - _minX) + _minX);
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
            funcResults = funcResults.Distinct().OrderByDescending(f => f).ToList();

            var min = funcResults.Last() >= 0 ? 0 : funcResults.Last();

            var probabilitiesBySolution = new Dictionary<double, double>();
            double sum = 0;
            foreach (var solution in funcResults)
            {
                sum += solution + Math.Abs(min);
            }

            foreach (var solution in funcResults)
            {
                probabilitiesBySolution.Add(solution, (solution + Math.Abs(min)) / sum);
            }


            return probabilitiesBySolution;
        }

        protected override double Decode(Gene<byte>[] genes)
        {
            string binaryString = "";
            for (var i = 0; i < genes.Length; i++)
            {
                binaryString = genes[i].Value + binaryString;
            }
            var intervalNum = Convert.ToInt32(binaryString, 2);
            var intervalLength = (_maxX - _minX) / 2048;
            return _minX + intervalNum * intervalLength;
        }

        protected override Gene<byte>[] Encode(double solution)
        {
            var res = new Gene<byte>[_chromosomeLength];

            var intervalLength = (_maxX - _minX) / 2048;
            var intervalNum = (solution - _minX) / intervalLength;

            var binaryString = Convert.ToString((int)intervalNum, 2);
            var fullString = new string('0', _chromosomeLength - binaryString.Length) + binaryString;
            for (int i = 0; i < _chromosomeLength; i++)
            {
                res[i] = new Gene<byte>((byte)(fullString[i] == '0' ? 0 : 1));
            }
            return res;
        }

        public override double CalculateFunctionValue(double arg)
        {
            return FunctionHelper.CalculateFunctionValue(arg);
        }

        public List<double> StepForward(int stepsCount)
        {
            for (var i = 0; i < stepsCount; i++)
            {
                base.DoReproduction();
                base.DoCrossover();
                base.DoMutations();
            }

            return this.ToList();
        }

        protected override Gene<byte>[] MutationFunction(Gene<byte>[] genes)
        {
            var r = new Random();
            var mutationPoint = r.Next(0, genes.Count());

            var newGeneValue = (byte)(genes[mutationPoint].Value == 1 ? 0 : 1);

            var newGenes = genes
                .Take(mutationPoint)
                .ToList();
            newGenes
                .Add(new Gene<byte>(newGeneValue));
            newGenes = newGenes.Concat(genes.Skip(mutationPoint + 1)).ToList();

            return newGenes.ToArray();
        }

        protected override Tuple<Gene<byte>[], Gene<byte>[]> CrossoverFunction(Chromosome ch1, Chromosome ch2)
        {
            var r = new Random();
            var crossoverPoint = r.Next(1, ch1.Genes.Count - 1);

            var ch1Tail = new List<Gene<byte>>(ch1.Genes.Skip(crossoverPoint));
            var ch2Tail = new List<Gene<byte>>(ch2.Genes.Skip(crossoverPoint));

            var ch1Genes = ch1.Genes.Take(crossoverPoint).Concat(ch2Tail).ToArray();
            var ch2Genes = ch2.Genes.Take(crossoverPoint).Concat(ch1Tail).ToArray();

            return new Tuple<Gene<byte>[], Gene<byte>[]>(ch1Genes, ch2Genes);
        }

        public void Clear()
        {
            this.Fill(new List<double>());
        }
    }
}
