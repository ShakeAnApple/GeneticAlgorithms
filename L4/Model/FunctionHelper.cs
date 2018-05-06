using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4.Model
{
    static class FunctionHelper
    {
        public static double CalculateTargetFunctionValue(Measure measure)
        {
            double res = 0;
            var orderedVals = measure.TermValues.Keys.OrderBy(v => v).ToList();

            for (int i = measure.TermValues.Count; i >= 1; i--)
            {
                res += i * (Math.Pow(measure.TermValues[orderedVals[measure.TermValues.Count - i]], 2));
            }

            return res;
        }

        public static void FillMeasures(List<Measure> measures, Function func)
        {
            foreach (var measure in measures)
            {
                var res = func.Calculate(measure.TermValues);
                if (Double.IsNaN(res) || Double.IsInfinity(res))
                {
                    measure.HypothesisFuncResult = Int64.MaxValue;
                    measure.TargetFuncResult = Int64.MinValue;
                }
                else
                {
                    measure.HypothesisFuncResult = res;
                    measure.TargetFuncResult = FunctionHelper.CalculateTargetFunctionValue(measure);
                }
            }
        }
    }
}
