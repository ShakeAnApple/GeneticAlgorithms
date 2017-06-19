namespace L3.ViewModels
{
    public class StepItemViewModel
    {
        public string Name { get; set; }
        public int PopulationCount { get; set; }
        public string BestSolution { get; set; }
        public double SumDistance { get; set; }

        public StepItemViewModel Prev { get; set; }
        public StepItemViewModel Next { get; set; }
    }    
}