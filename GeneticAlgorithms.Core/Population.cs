using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace GeneticAlgorithms.Core
{
    public abstract class Population<TFuncResult, TSolution, TGene> : IEnumerable<TSolution>
    {
        public class Chromosome
        {
            public ReadOnlyCollection<Gene<TGene>> Genes { get; private set; }

            public Chromosome(Gene<TGene>[] genes)
            {
                Genes = new ReadOnlyCollection<Gene<TGene>>(genes);
            }

            public void Crossover(Chromosome other, Func<Chromosome, Chromosome, Tuple<Gene<TGene>[], Gene<TGene>[]>> crossover)
            {
                var result = crossover(this, other);

                this.Genes = new ReadOnlyCollection<Gene<TGene>>(result.Item1);
                other.Genes = new ReadOnlyCollection<Gene<TGene>>(result.Item2);
            }

            public void Mutate(Func<Gene<TGene>[], Gene<TGene>[]> mutate)
            {
                var newGenes = mutate(this.Genes.ToArray());
                this.Genes = new ReadOnlyCollection<Gene<TGene>>(newGenes);
            }

            public IEnumerator GetEnumerator()
            {
                return Genes.GetEnumerator();
            }

            public Gene<TGene> this[int index]
            {
                get
                {
                    return Genes[index];
                }
            }

            internal Chromosome clone()
            {
                return new Chromosome(Genes.ToArray());
            }

            public override string ToString()
            {
                return string.Join(", ", Genes.Select(g => g.ToString()));
            }
        }

        protected abstract Gene<TGene>[] Encode(TSolution solution);
        protected abstract TSolution Decode(Gene<TGene>[] genes);

        public abstract TFuncResult CalculateFunctionValue(TSolution arg);

        protected abstract Dictionary<TFuncResult, double> CalculateRelativeOptimality(List<TFuncResult> solutions);
        protected abstract Gene<TGene>[] MutationFunction(Gene<TGene>[] genes);
        protected abstract Tuple<Gene<TGene>[], Gene<TGene>[]> CrossoverFunction(Chromosome ch1, Chromosome ch2);

        private readonly int _chromosomeLength;

        private readonly double _crossoverProbability;
        private readonly double _mutationProbability;

        private List<Chromosome> _chromosomes;
        private List<Chromosome> _reproductionResult;

        private List<Chromosome> _bestSolutions;
        private const double BestSolutionsPercent = 0.5;

        private int _populationCapacity;

        private Random _r;

        public Population(int chromosomeLength, double crossoverProbability, double mutationProbability)
        {
            _chromosomeLength = chromosomeLength;
            _crossoverProbability = crossoverProbability;
            _mutationProbability = mutationProbability;

            _chromosomes = new List<Chromosome>();
            _bestSolutions = new List<Chromosome>();

            _r = new Random();
        }

        public Population(int chromosomeLength, double crossoverProbability, double mutationProbability, int capacity) 
            : this(chromosomeLength, crossoverProbability, mutationProbability)
        {
            _populationCapacity = capacity;
        }

        protected void Fill(List<TSolution> solutions)
        {
            _chromosomes.AddRange(
                solutions.Select(s => new Chromosome(Encode(s)))
            );
            if (_populationCapacity == 0)
            {
                _populationCapacity = _chromosomes.Count;
            }
        }

        protected void DoReproduction()
        {
            if (_chromosomes.Count == 0)
            {
                throw new ApplicationException("Population is empty!");
            }

            var solutionsByFuncResult = new Dictionary<TFuncResult, Chromosome>();

            var funcResults = new List<TFuncResult>(
                _chromosomes.Select(ch => {
                    var funcValue = CalculateFunctionValue(Decode(ch.Genes.ToArray()));
                    if (!solutionsByFuncResult.ContainsKey(funcValue))
                    {
                        solutionsByFuncResult.Add(funcValue, ch);
                    }
                    return funcValue;
                })
            );

            var funcResultsProbabilities = CalculateRelativeOptimality(funcResults);
            _reproductionResult = new List<Chromosome>();

            var orderedResults = funcResultsProbabilities.Keys
                                                .OrderByDescending(k => funcResultsProbabilities[k])
                                                .ToList();

            var badSolutionsCount = 0;
            var funcResultsForReproduction = new List<TFuncResult>();

            foreach (var funcResult in orderedResults)
            {
                var elementsCount = Math.Round(funcResultsProbabilities[funcResult] * _populationCapacity);
                if (elementsCount == 0)
                {
                    badSolutionsCount++;

                    badSolutionsCount %= funcResultsForReproduction.Count;
                    funcResultsForReproduction.Add(funcResultsForReproduction[badSolutionsCount]);
                    //badSolutionsCount %= _reproductionResult.Count;
                    //_reproductionResult.Add(_reproductionResult[badSolutionsCount]);
                }
                for (int i = 0; i < elementsCount; i++)
                {
                    funcResultsForReproduction.Add(funcResult);
                    //        _reproductionResult.Add(solutionsByFuncResult[funcResult]);
                }
            }
            funcResultsForReproduction = funcResultsForReproduction
                                                .OrderByDescending(res => funcResultsProbabilities[res])
                                                //.Take(funcResultsForReproduction.Count - (int)(_populationCapacity*BestSolutionsPercent))
                                                .Take(funcResultsForReproduction.Count)
                                                .ToList();
            _reproductionResult.AddRange(
                funcResultsForReproduction.Select(res => solutionsByFuncResult[res])
            );

            //_reproductionResult.AddRange(_bestSolutions);
            _bestSolutions = new List<Chromosome>();
            for (int i = 0; i < (int)(_populationCapacity * BestSolutionsPercent); i++)
            {
                _bestSolutions.Add(_reproductionResult[i].clone());
            }
            //_bestSolutions.Add(_reproductionResult.First());

            ///////////
            //var temp = Decode(_reproductionResult.First().Genes.ToArray());
            // Debug.WriteLine(temp);

            //var debug = _bestSolutions.Select(s => Decode(s.Genes.ToArray())).OrderByDescending(s => s);
            //var fres = debug.Select(s => CalculateFunctionValue(s)).OrderByDescending(s => s).ToList();
            ///////////

            //while (_bestSolutions.Count >= _populationCapacity)
            //{
            //    _bestSolutions.RemoveAt(0);
            //}
            /////////////////////
            //Debug.Print("\nAfter reproduction:");
            //Debug.Print("Func res:");
            //Debug.Print(String.Join("\n", _reproductionResult.Select(ch => CalculateFunctionValue(Decode(ch.Genes.ToArray()))).OrderBy(r => r)));

            //Debug.Print("\nBest:");
            //Debug.Print("Func res:");
            //Debug.Print(String.Join("\n", _bestSolutions.Select(ch => CalculateFunctionValue(Decode(ch.Genes.ToArray()))).OrderBy(r => r)));
            ////////////////////
            //Debug.Print("Solutions:\n");
            //Debug.Print(String.Join("\n", _reproductionResult.Select(ch => Decode(ch.Genes.ToArray())).OrderByDescending(r => CalculateFunctionValue(r))));
        }

        protected void DoCrossover()
        {
            if (_chromosomes.Count == 0)
            {
                throw new ApplicationException("Population is empty!");
            }


            _chromosomes = new List<Chromosome>();

            if (_populationCapacity == 1)
            {
                _chromosomes.AddRange(_reproductionResult);
            }

            
            //var solutionsForCrossoverCount = Math.Round(_crossoverProbability * (_reproductionResult.Count + _bestSolutions.Count) / 2);
            var solutionsForCrossoverCount = Math.Round(_crossoverProbability * _reproductionResult.Count / 2);

            //if (solutionsForCrossoverCount % 2 != 0)
            //{
            //    solutionsForCrossoverCount++;
            //}
            //while (solutionsForCrossoverCount > _reproductionResult.Count)
            //{
            //    var bestSolutionToReproduce = _bestSolutions.First();
            //    _reproductionResult.Add(bestSolutionToReproduce);
            //    _bestSolutions.Remove(bestSolutionToReproduce);
            //}
            for (int i = 0; i < solutionsForCrossoverCount; i++)
            {
                int i1 = 0, i2 = 0;
                while (i1 == i2)
                {
                    i1 = _r.Next(0, _reproductionResult.Count - 1);
                    i2 = _r.Next(0, _reproductionResult.Count - 1);
                }

                var s1 = _reproductionResult[i1];
                var s2 = _reproductionResult[i2];

                s1.Crossover(s2, CrossoverFunction);
                _chromosomes.Add(s1);
                _chromosomes.Add(s2);

                _reproductionResult.RemoveAt(i1);
                _reproductionResult.RemoveAt(i2);
            }
            //_chromosomes.AddRange(_reproductionResult);
            _chromosomes.AddRange(_bestSolutions.Take(_populationCapacity - _chromosomes.Count));
            //_chromosomes.AddRange(_bestSolutions);

            //var debug = _chromosomes.Select(s => Decode(s.Genes.ToArray())).OrderByDescending(s => s);
            //var fres = debug.Select(s => CalculateFunctionValue(s)).OrderByDescending(s=>s).ToList();

            //////////////////////////////////
            //Debug.Print("\nAfter crossover:");
            //Debug.Print("Func res:\n");
            //Debug.Print(String.Join("\n", _chromosomes.Select(ch => CalculateFunctionValue(Decode(ch.Genes.ToArray()))).OrderBy(res => res)));
            //Debug.Print("Best:");
            //Debug.Print("Func res:");
            //Debug.Print(String.Join("\n", _bestSolutions.Take(_populationCapacity - _chromosomes.Count).Select(ch => CalculateFunctionValue(Decode(ch.Genes.ToArray()))).OrderBy(res => res)));
            //////////////////////////
            
            //Debug.Print("\nSolutions:\n");
            //Debug.Print(String.Join("\n", _chromosomes.Select(ch => Decode(ch.Genes.ToArray())).OrderByDescending(res => CalculateFunctionValue(res))));
        }

        protected void DoMutations()
        {
            if (_chromosomes.Count == 0)
            {
                throw new ApplicationException("Population is empty!");
            }

            var r = new Random();
            var solutionsForMutationCount = _mutationProbability * _chromosomes.Count;
            for (int i = 0; i < solutionsForMutationCount; i++)
            {
                var s = _reproductionResult[r.Next(0, _reproductionResult.Count - 1)];
                s.Mutate(MutationFunction);
            }

        }

        #region enumerableImpl

        public IEnumerator<TSolution> GetEnumerator()
        {
            var decodedChromosomes = new List<TSolution>();
            foreach (var ch in _chromosomes)
            {
                decodedChromosomes.Add(Decode(ch.Genes.ToArray()));
            }

            return decodedChromosomes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var decodedChromosomes = new List<TSolution>();
            foreach (var ch in _chromosomes)
            {
                decodedChromosomes.Add(Decode(ch.Genes.ToArray()));
            }

            return decodedChromosomes.GetEnumerator();
        }
        #endregion
    }

    // 
}
