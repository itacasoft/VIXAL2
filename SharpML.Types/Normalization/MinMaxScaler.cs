using System.Linq;

namespace SharpML.Types.Normalization
{
    public class MinMaxScaler : INormalizer
    {
        double[] mins;
        double[] maxs;

        public override void Initialize(double[][] X)
        {
            mins = MinOfEachColumn(X);
            maxs = MaxOfEachColumn(X);

            initialized = true;
        }

        public override void Initialize(double[] trainVector)
        {
            mins = new double[1];
            mins[0] = trainVector.Min();

            maxs = new double[1];
            maxs[0] = trainVector.Max();
            initialized = true;
        }


        public override double[] Decode(double[] values, int col)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Decode(values[i], col);
            }
            return result;
        }

        public override double Decode(double value, int col)
        {
            double result = value * (maxs[col] - mins[col]) + mins[col];
            return result;
        }

        public override double[] Normalize(double[] values, int col)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Normalize(values[i], col);
            }
            return result;
        }

        public override double Normalize(double value, int col)
        {
            double result = (value - mins[col]) / (maxs[col] - mins[col]);
            return result;
        }

        public double[] MinOfEachColumn(double[][] values)
        {
            double[] result = new double[values[0].Length];
            for (int row = 0; row < result.Length; row++)
            {
                result[row] = double.MaxValue;
            }

            for (int col = 0; col < values[0].Length; col++)
            {
                for (int row = 0; row < values.Length; row++)
                {
                    if (values[row][col] < result[col]) result[col] = values[row][col];
                }
            }

            return result;
        }

        public double[] MaxOfEachColumn(double[][] values)
        {
            double[] result = new double[values[0].Length];
            for (int row = 0; row < result.Length; row++)
            {
                result[row] = double.MinValue;
            }

            for (int col = 0; col < values[0].Length; col++)
            {
                for (int row = 0; row < values.Length; row++)
                {
                    if (values[row][col] > result[col]) result[col] = values[row][col];
                }
            }

            return result;
        }
    }

}
