using System;

namespace L1
{
    public class FunctionHelper
    {
        public static double CalculateFunctionValue(double arg)
        {
            return Math.Sin(arg) / Math.Pow(arg, 2);
        }
    }
}
