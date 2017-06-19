namespace L2.Model
{
    public struct Point
    {
        public Point(double x, double y, double z)
        {
            this.coords = new double[] { x, y, z };
        }

        public Point(double[] coords)
        {
            this.coords = coords;
        }

        public Point(double[] coords, double fRes)
        {
            this.coords = new double[coords.Length + 1];
            for (int i = 0; i < coords.Length; i++)
            {
                this.coords[i] = coords[i];
            }
            this.coords[coords.Length] = fRes;
        }

        public double[] coords;
    }
}
