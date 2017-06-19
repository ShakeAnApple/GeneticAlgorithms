using GeneticAlgorithms.Core;
using L3.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace L3
{
    public class PopulationTSP : Population<double, City[], City>
    {
        private readonly CitiesService _citiesService;
        private readonly int _solutionLength;

        public PopulationTSP(double crossoverProbability, double mutationProbability, CitiesService citiesService, int populationCapacity) 
            : base(citiesService.Cities.Count(), crossoverProbability, mutationProbability)
        {
            _citiesService = citiesService;
            _solutionLength = _citiesService.Cities.Count;

            this.GenerateAndFill(_citiesService.Cities, populationCapacity);
        }

        private void GenerateAndFill(List<City> cities, int populationCapacity)
        {
            var population = new List<City[]>();

            var r = new Random();
            for (int i = 0; i < populationCapacity; i++)
            {
                var path = new City[_solutionLength];
                var tmpCities = cities.ToList();
                for (int j = 0; j < _solutionLength; j++)
                {
                    var cityNum = r.Next(tmpCities.Count);
                    path[j] = tmpCities[cityNum];
                    tmpCities.RemoveAt(cityNum);
                }
                population.Add(ConvertToNeighbours(path));
            }

            base.Fill(population);
        }

        public override double CalculateFunctionValue(City[] solution)
        {
            var path = ConvertToPath(solution);
            double dist = 0;
            for (int i = 1; i < path.Length; i++)
            {
                dist += _citiesService.GetDistance(path[i], path[i - 1]);
            }
            dist += _citiesService.GetDistance(path[path.Length - 1], path[0]);
            return dist;
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

        protected override Tuple<Gene<City>[], Gene<City>[]> CrossoverFunction(Chromosome ch1, Chromosome ch2)
        {
            var children = new City?[2][];

            var ch1Cities = ch1.Genes.Select(g => g.Value).ToArray();
            var ch2Cities = ch2.Genes.Select(g => g.Value).ToArray();

            var r = new Random();
            for (int i = 0; i < 2; i++)
            {
                children[i] = new City?[_solutionLength]; 

                var availableCities = new List<City>(_citiesService.Cities);

                var startCityNum = r.Next(_solutionLength);
                var startCity = availableCities[startCityNum];
                //var startCity = availableCities.First(c => c.number == r.Next(_solutionLength));
                var prevCity = startCity;                

                availableCities.Remove(prevCity);

                var nextCity = new City(); 
                for (int j = 0; j < _solutionLength - 1; j++)
                {
                    var distanceCh1 = _citiesService.GetDistance(prevCity, ch1Cities[prevCity.number]);
                    var distanceCh2 = _citiesService.GetDistance(prevCity, ch2Cities[prevCity.number]);
                    nextCity = distanceCh1 < distanceCh2 ? ch1Cities[prevCity.number] : ch2Cities[prevCity.number];

                    if (children[i][nextCity.number] != null)
                    {
                        var nextCityNum = r.Next(_solutionLength - j - 1);
                        //nextCity = availableCities.First(c => c.number == nextCityNum);
                        nextCity = availableCities[nextCityNum];
                    }

                    availableCities.Remove(nextCity);

                    children[i][prevCity.number] = nextCity;
                    prevCity = nextCity;
                }

                children[i][nextCity.number] = startCity; 
            }
            return new Tuple<Gene<City>[], Gene<City>[]>(
                children[0].Select(c => new Gene<City>(c.Value)).ToArray(), 
                children[1].Select(c => new Gene<City>(c.Value)).ToArray());
        }

        protected override Gene<City>[] MutationFunction(Gene<City>[] genes)
        {
            var r = new Random();
            var cityToMoveNum = r.Next(genes.Length - 1) + 1;
            int position = cityToMoveNum;
            while (cityToMoveNum == position)
            {
                position = r.Next(genes.Length);
            }

            var cityToMoveGene = genes.First(g => g.Value.number == cityToMoveNum);
            var origFromNum = Array.IndexOf(genes, cityToMoveGene);
            var nextCityGene = genes[cityToMoveGene.Value.number];
            genes[origFromNum] = nextCityGene;

            var cityToReplaceGene = genes[position];
            genes[position] = cityToMoveGene;
            genes[cityToMoveGene.Value.number] = cityToReplaceGene;

            return genes;
        }

        protected override City[] Decode(Gene<City>[] genes)
        {
            return genes.Select(g => g.Value).ToArray();
        }

        protected override Gene<City>[] Encode(City[] solution)
        {
            return solution.Select(s => new Gene<City>(s)).ToArray();
        }
        

        public void Clear()
        {
            this.Clear();
        }

        public City[] StepForward(int stepsCount)
        {
            for (var i = 0; i < stepsCount; i++)
            {
                base.DoReproduction();
                base.DoCrossover();
                base.DoMutations();
            }

            return ConvertToPath(this.First());
        }

        private City[] ConvertToPath(City[] solution)
        {
            var result = new City[solution.Length];
            result[0] = solution.First(c => c.number == 0);

            for (int i = 1; i < solution.Length - 1; i++)
            {
                result[i] = solution[result[i - 1].number];
            }
            return result;
        }

        private City[] ConvertToNeighbours(City[] path)
        {
            var result = new City[path.Length];
            for (int i = 1; i < path.Length; i++)
            {
                result[path[i - 1].number] = path[i];
            }
            result[path[path.Length - 1].number] = path[0];
            return result;
        }
    }
}
