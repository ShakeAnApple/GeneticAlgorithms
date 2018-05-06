using L5.Model;
using System.Collections.Generic;

namespace L5.ViewModels
{
    public class StepItemViewModel
    {
        public string Name { get; set; }
        public Point Data { get; set; }
        public int PopulationCount { get; set; }
        public string BestSolution { get; set; }

        public StepItemViewModel Prev { get; set; }
        public StepItemViewModel Next { get; set; }
    }    
}