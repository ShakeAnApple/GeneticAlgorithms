namespace L5.Model
{
    public struct Point
    {
        public double[] coords;
        public double result;

        public Point(double x, double y)
        {
            this.coords = new double[] { x, y};
            result = FunctionHelper.CalculateFunctionValue(coords);
        }

        public Point(double[] coords)
        {
            this.coords = coords;
            result = FunctionHelper.CalculateFunctionValue(coords);
        }

        public Point(double[] coords, double fRes)
        {
            this.coords = new double[coords.Length];
            for (int i = 0; i < coords.Length; i++)
            {
                this.coords[i] = coords[i];
            }
            this.result = fRes;
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", string.Join(",", coords), result);
        }
    }
}
