using System;

namespace L3.Model
{
    public struct City
    {
        public City(int number, Coords coords)
        {
            this.number = number;
            this.coords = coords;
        }

        public City(int number, double x, double y)
        {
            this.number = number;
            this.coords = new Coords(x, y);
        }

        public int number;
        public Coords coords;

        public double CountDist(City another)
        {
            var anotherCoords = another.coords;
            return Math.Sqrt(Math.Pow((anotherCoords.x - this.coords.x), 2) + Math.Pow((anotherCoords.y - this.coords.y), 2));
        }
    }

    public struct Coords
    {
        public Coords(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x;
        public double y;

        public double CountDist(Coords another)
        {
            return Math.Sqrt(Math.Pow((another.x - this.x), 2) + Math.Pow((another.y - this.y), 2));
        }
    }
}
