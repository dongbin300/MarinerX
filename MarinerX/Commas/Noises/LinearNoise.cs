namespace MarinerX.Commas.Noises
{
    public class LinearNoise : Noise
    {
        public LinearNoise(decimal evaluationMin, decimal evaluationMax) : base(evaluationMin, evaluationMax)
        {
        }

        public new decimal GetNoiseValue(decimal value)
        {
            var _value = base.GetNoiseValue(value);
            return _value / EvaluationMax;
        }
    }
}
