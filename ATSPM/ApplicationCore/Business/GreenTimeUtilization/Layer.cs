namespace ATSPM.Application.Business.GreenTimeUtilization
{
    public class Layer
    {
        public double DataValue { get; set; }
        public int LowerEnd { get; set; }


        public Layer(double sumValue, int cycleCount, int binStart)
        {
            DataValue = (double)sumValue / cycleCount;
            LowerEnd = binStart;
        }
    }
}