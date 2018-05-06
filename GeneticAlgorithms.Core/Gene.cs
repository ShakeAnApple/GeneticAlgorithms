namespace GeneticAlgorithms.Core
{
    public class Gene<T>
    {
        public Gene(T value)
        {
            _value = value;
        }

        public T Value { get { return _value; } }
        private T _value;

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
