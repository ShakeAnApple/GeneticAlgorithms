using OxyPlot.Series;
using System.Collections.Generic;

namespace L1.ViewModels
{
    public class StepItemViewModel
    {
        public string Name { get; set; }
        public List<ScatterPoint> Data { get; set; }
        public int PopulationCount { get; set; }

        public StepItemViewModel Prev { get; set; }
        public StepItemViewModel Next { get; set; }
    }
}