using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4.Model
{
    public class Result
    {
        public Dictionary<string, List<Measure>> MeasuresByDimention { get; set; }
        public Function Function { get; set; }
        public double FitnesFunctionVal { get; set; }
    }
}
