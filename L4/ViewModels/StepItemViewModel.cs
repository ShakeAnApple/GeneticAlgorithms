using L4.Model;
using System.Collections.Generic;

namespace L4.ViewModels
{
    public class StepItemViewModel
    {
        public string Name { get; set; }
        public int PopulationCount { get; set; }
        public string BestSolution { get; set; }
        public Dictionary<string, List<Measure>> MeasuresByDimention { get; set; }

        public StepItemViewModel Prev { get; set; }
        public StepItemViewModel Next { get; set; }
    }    
}