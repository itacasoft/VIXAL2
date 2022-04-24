namespace SharpML.Types.Normalization
{
    public class FakeNormalizer : INormalizer
    {
        public override void Initialize(double[][] trainMatrix)
        {
            initialized = true;
        }

        public override void Initialize(double[] trainVector)
        {
            initialized = true;
        }


        public override double[] Decode(double[] values, int col)
        {
            return (double[])values.Clone();
        }

        public override double Decode(double value, int col)
        {
            return value;
        }

        public double DecodeValue(double value, double mean, double deviance)
        {
            return value;
        }

        public override double[] Normalize(double[] values, int col)
        {
            return (double[])values.Clone();
        }

        public override double Normalize(double value, int col)
        {
            return value;
        }

        public double NormalizeValue(double value, double mean, double deviance)
        {
            return value;
        }
    }
}
