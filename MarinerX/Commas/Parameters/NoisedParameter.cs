namespace MarinerX.Commas.Parameters
{
    public class NoisedParameter
    {
        public decimal Value { get; set; }
        public decimal Min { get; set; }
        public decimal Max { get; set; }

        public NoisedParameter(decimal min, decimal max, decimal value)
        {
            Min = min;
            Max = max;
            Value = value;
        }

        /// <summary>
        /// noise = 0~1 value
        /// </summary>
        /// <param name="noise"></param>
        public void MakeNoise(decimal noise)
        {

        }
    }
}
