using L2.Model;
using System.Collections.Generic;

namespace L2.ViewModels
{
    public class StepItemViewModel
    {
        public string Name { get; set; }
        public List<Point> Data { get; set; }
        public int PopulationCount { get; set; }

        public StepItemViewModel Prev { get; set; }
        public StepItemViewModel Next { get; set; }
    }    
}