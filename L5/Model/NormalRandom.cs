using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L5.Model
{
    class NormalRandom : Random
    {
        double prevSample = double.NaN;
        protected override double Sample()
        {
            if (!double.IsNaN(prevSample))
            {
                double result = prevSample;
                prevSample = double.NaN;
                return result;
            }

            double u, v, s;
            do
            {
                u = 2 * base.Sample() - 1;
                v = 2 * base.Sample() - 1; // [-1, 1)
                s = u * u + v * v;
            }
            while (u <= -1 || v <= -1 || s >= 1 || s == 0);
            double r = Math.Sqrt(-2 * Math.Log(s) / s);

            prevSample = r * v;
            return r * u;
        }
    }
}
