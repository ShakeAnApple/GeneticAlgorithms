using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4.Model
{
    public class Measure
    {
        public string ChangingVarName { get; private set; }
        public Dictionary<string, double> TermValues { get; private set; }
        public double HypothesisFuncResult { get; set; }
        public double TargetFuncResult { get; set; }

        public Measure(Dictionary<string, double> termValues, string changingVarName)
        {
            TermValues = termValues;
            ChangingVarName = changingVarName;
        }
    }
}
