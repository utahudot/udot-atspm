
namespace ATSPM.Data.Models.SpeedManagement
{
    public class DataPoint<T> where T : struct, IComparable, IConvertible, IFormattable
    {
        public DateTime Timestamp { get; set; }
        public T Value { get; set; }

        public DataPoint(DateTime start, T value)
        {
            Timestamp = start;
            Value = value;
        }

        public override string ToString()
        {
            return $"Start: {Timestamp}, Value: {Value}";
        }
    }
}
