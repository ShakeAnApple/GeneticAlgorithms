using System;
using System.Linq;

namespace L5
{
    public class FunctionHelper
    {
        public static double CalculateFunctionValue(double[] xx)
        {
            //var dimentions = xx.Length;
            //double outer = 0;
            //for (int i = 0; i < dimentions; i++)
            //{
            //    double inner = 0;
            //    for (int j = 0; j < i; j++)
            //    {
            //        inner += xx[j];
            //    }
            //    outer += Math.Pow(inner,2);
            //}
            //return outer;
            // f[xx_] := Sum[Sum[xx[[j]], { j, 1, i}]^ 2, { i, 1, Length[xx]}];

            return xx.Select((x, i) => Math.Pow(xx.Take(i + 1).Sum(), 2)).Sum();
        }
    }
}
