using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace L3.Model
{
    public class CitiesService
    {
        private double[,] _distanceMap;

        public List<City> Cities { get; private set; }

        public CitiesService(string path)
        {
            this.Cities = ReadCities(path);
        }

        public List<City> ReadCities(string path)
        {
            var result = new List<City>();

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var props = line.Split(' ');
                var num = Int32.Parse(props[0]) - 1;
                var x = Double.Parse(props[1]);
                var y = Double.Parse(props[2]);
                var city = new City(num, x, y);

                result.Add(city);
            }

            _distanceMap = new double[lines.Length, lines.Length] ;
            for (int i = 0; i < lines.Length; i++)
            {
                for (int j = 0; j < lines.Length; j++)
                {
                    _distanceMap[i, j] = -1;
                }
            }

            ////
            //foreach(var city1 in result.OrderBy(r => r.number))
            //{
            //    foreach (var city2 in result.OrderBy(r => r.number))
            //    {
            //        _distanceMap[city1.number, city2.number] = (int)city1.CountDist(city2);
            //    }
            //}

            //var sb = new StringBuilder();
            //for (int i = 0; i < result.Count; i++)
            //{
            //    var lineBuilder = new StringBuilder();
            //    for (int j = 0; j < result.Count; j++)
            //    {
            //        lineBuilder.Append(_distanceMap[i, j] + " ");
            //    }
            //    sb.AppendLine(lineBuilder.ToString());
            //}

            //File.WriteAllText("C:\\tmp\\matrix.txt", sb.ToString());
            /////

            return result;
        }

        public double GetDistance(City c1, City c2)
        {
            if (_distanceMap == null)
            {
                throw new ApplicationException("Map hasn't been read yet");
            }

            if (_distanceMap[c1.number, c2.number] == -1)
            {
                _distanceMap[c1.number, c2.number] = c1.CountDist(c2);

            }
            return _distanceMap[c1.number, c2.number];
        }
    }
}
