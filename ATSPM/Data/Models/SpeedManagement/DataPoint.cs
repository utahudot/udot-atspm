
namespace ATSPM.Data.Models.SpeedManagement
{
    public class DataPoint<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        public DateTime Start { get; set; }
        public T Value { get; set; }

        public DataPoint(DateTime start, T value)
        {
            Start = start;
            Value = value;
        }

        public override string ToString()
        {
            return $"Start: {Start}, Value: {Value}";
        }
    }
}
