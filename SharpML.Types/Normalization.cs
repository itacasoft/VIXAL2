using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SharpML.Types
{
    public static class Normalizer
    {
        private static INormalizer _instance;
        public static INormalizer Instance
        {
            get
            {
                if (_instance == null)
                {
                    string normalizerType = ConfigurationManager.AppSettings["NormalizerType"];
                    InnerConstructor(normalizerType);
                }

                return _instance;
            }
        }

        private static void InnerConstructor(string normalizerType)
        {
            if (normalizerType == null)
            {
                _instance = new ClassicNormalizer();
            }
            else if ((normalizerType == "classic") || (normalizerType == "Classic") || (normalizerType == "ClassicNormalizer"))
            {
                _instance = new ClassicNormalizer();
            }
            else if ((normalizerType == "modern") || (normalizerType == "Modern") || (normalizerType == "ModernNormalizer"))
            {
                _instance = new ModernNormalizer();
            }
            else if ((normalizerType.ToLower() == "minmax") || (normalizerType.ToLower() == "minmaxscaler"))
            {
                _instance = new MinMaxScaler();
            }
            else
            {
                _instance = new FakeNormalizer();
            }
        }

        public static void SubstituteType(string normalizationType)
        {
            InnerConstructor(normalizationType);
        }
    }

    public abstract class INormalizer
    {
        public INormalizer()
        {
            Random = new Random();
        }

        protected bool initialized = false;

        public Random Random;

        public abstract double[] Normalize(double[] values, int col);
        public abstract double Normalize(double value, int col);
        public abstract double[] Decode(double[] values, int col);
        public abstract double Decode(double value, int col);

        double[] GetVectorFromArray(double[][] input, int column)
        {
            double[] result = new double[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                result[i] = input[i][column];
            }
            return result;
        }

        void SetArrayFromVector(double[] input, int column, double[][] output)
        {
            double[] result = new double[input.Length];
            for (int row = 0; row < input.Length; row++)
            {
                output[row][column] = input[row];
            }
        }


        public void NormalizeByRef(double[][] values)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            for (int column = 0; column < values[0].Length; column++)
            {
                double[] v = GetVectorFromArray(values, column);

                //normalize train
                for (int row = 0; row < v.Length; row++)
                {
                    v[row] = Normalizer.Instance.Normalize(v[row], column);
                }
                SetArrayFromVector(v, column, values);
            }
        }
        public double[][] Normalize(double[][] values)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[][] result = new double[values.Length][];
            for (int row = 0; row < values.Length; ++row)
            {
                result[row] = new double[values[0].Length];
            }

            for (int row = 0; row < values.Length; row++)
            {
                for (int col = 0; col < values[0].Length; col++)
                {
                    result[row][col] = Normalize(values[row][col], col);
                }
            }
            return result;
        }

        public double[][] Decode(double[][] values)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[][] result = new double[values.Length][];
            for (int row = 0; row < values.Length; ++row)
            {
                result[row] = new double[values[0].Length];
            }

            for (int row = 0; row < values.Length; row++)
            {
                for (int col = 0; col < values[0].Length; col++)
                {
                    result[row][col] = Decode(values[row][col], col);
                }
            }
            return result;
        }


        protected Stat[] GetStats(double[][] data, int from, int count)
        {
            from = Math.Max(from, 0);

            Stat[] stats = new Stat[data[0].Length];

            for (int i = 0; i < data[0].Length; ++i)
            {
                double[] part = data.Skip(from).Take(count).Select(x => x[i]).ToArray();

                stats[i] = new Stat();
                stats[i].Mean = Utils.Mean(part);
                stats[i].Deviance = Utils.StandardDeviation(part);
            }

            return stats;
        }

        public Stat GetStats(double[] data)
        {
            Stat stats = new Stat();
            stats.Mean = Utils.Mean(data);
            stats.Deviance = Utils.StandardDeviation(data);
            return stats;
        }

        public abstract void Initialize(double[] trainVector);

        public abstract void Initialize(double[][] trainMatrix);

    }

    public class ClassicNormalizer : INormalizer
    {
        protected RnnConfig rnnConfig;

        public ClassicNormalizer() : base()
        {
            this.rnnConfig = new RnnConfig();
        }

        public RnnConfig RnnConfig
        {
            get
            {
                return rnnConfig;
            }
        }

        public override void Initialize(double[] trainVector)
        {
            rnnConfig = new RnnConfig();
            rnnConfig.BeginStat(0, trainVector.Length, 1, int.MaxValue);
            Stat st = GetStats(trainVector);
            if (st.Deviance == 0) st.Deviance = 1;
            rnnConfig.SetStat(0, 0, 0, st);

            initialized = true;
        }


        public double NormalizeValue(double value, double mean, double deviance)
        {
            return Utils.NormalizeValue(value, 0, 1, mean, deviance);
        }

        public double DecodeValue(double value, double mean, double deviance)
        {
            return Utils.NormalizeValue(value, mean, deviance, 0, 1);
        }

        public override double[] Normalize(double[] values, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[] result = new double[values.Length];
            Stat stat = rnnConfig.GetStat(0, 0, col);

            for (int row = 0; row < result.Length; row++)
            {
                result[row] = Utils.NormalizeValue(values[row], 0, 1, stat.Mean, stat.Deviance);
            }
            return result;
        }


        public override double Normalize(double value, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            Stat stat = rnnConfig.GetStat(0, 0, col);
            double result = Utils.NormalizeValue(value, 0, 1, stat.Mean, stat.Deviance);
            return result;
        }


        public override double[] Decode(double[] values, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[] result = new double[values.Length];
            Stat stat = rnnConfig.GetStat(0, 0, col);

            for (int row = 0; row < values.Length; row++)
            {
                result[row] = rnnConfig.GetTransformed(0, 0, col, values[row]);
            }
            return result;
        }

        public override double Decode(double value, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double result = rnnConfig.GetTransformed(0, 0, col, value);
            return result;
        }

        public override void Initialize(double[][] trainMatrix)
        {
            rnnConfig = new RnnConfig();
            //vecchio fn-trading: salvo le statistiche in RnnConfig
            rnnConfig.BeginStat(0, trainMatrix.Length, trainMatrix[0].Length, int.MaxValue);
            Stat[] stats = GetStats(trainMatrix, 0, trainMatrix.Length);
            foreach (Stat st in stats)
                if (st.Deviance == 0) 
                    st.Deviance = 1;
            for (int col = 0; col < trainMatrix[0].Length; ++col)
                rnnConfig.SetStat(0, 0, col, stats[col]);

            initialized = true;
        }

    }

    public class ModernNormalizer : INormalizer
    {
        RnnConfig rnnConfig;

        public ModernNormalizer() : base()
        {
            this.rnnConfig = new RnnConfig();
        }

        public RnnConfig RnnConfig
        {
            get
            {
                return rnnConfig;
            }
        }


        public override void Initialize(double[] trainVector)
        {
            rnnConfig = new RnnConfig();
            rnnConfig.BeginStat(0, trainVector.Length, 1, int.MaxValue);
            Stat st = GetStats(trainVector);
            if (st.Deviance == 0) st.Deviance = 1;
            rnnConfig.SetStat(0, 0, 0, st);

            initialized = true;
        }


        public override void Initialize(double[][] trainMatrix)
        {
            rnnConfig = new RnnConfig();
            rnnConfig.BeginStat(0, trainMatrix.Length, trainMatrix[0].Length, int.MaxValue);
            Stat[] stats = GetStats(trainMatrix, 0, trainMatrix.Length);
            foreach (Stat st in stats)
                if (st.Deviance == 0) st.Deviance = 1;
            for (int col = 0; col < trainMatrix[0].Length; ++col)
                rnnConfig.SetStat(0, 0, col, stats[col]);

            initialized = true;
        }

        public override double[] Normalize(double[] values, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[] result = new double[values.Length];
            Stat stat = rnnConfig.GetStat(0, 0, col);

            for (int row = 0; row < result.Length; row++)
            {
                result[row] = this.NormalizeValue(values[row], stat.Mean, stat.Deviance);
            }
            return result;
        }

        public double NormalizeValue(double input, double mean, double deviance)
        {
            return (input - mean) / deviance;
        }

        public override double Normalize(double value, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            Stat stat = rnnConfig.GetStat(0, 0, col);
            double result = NormalizeValue(value, stat.Mean, stat.Deviance);
            return result;
        }

        public override double[] Decode(double[] values, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[] result = new double[values.Length];
            Stat stat = rnnConfig.GetStat(0, 0, col);

            for (int row = 0; row < result.Length; row++)
            {
                result[row] = this.DecodeValue(values[row], stat.Mean, stat.Deviance);
            }
            return result;
        }

        public override double Decode(double value, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            Stat stat = rnnConfig.GetStat(0, 0, col);
            double result = this.DecodeValue(value, stat.Mean, stat.Deviance);
            return result;
        }

        public double DecodeValue(double value, double mean, double deviance)
        {
            double result = value * deviance + mean;
            return result;
        }
    }

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
            double result = value*(maxs[col] - mins[col]) + mins[col];
            return result;
        }

        public override double[] Normalize(double[] values, int col)
        {
            double[] result = new double[values.Length];
            for (int i=0; i<values.Length; i++)
            {
                result[i] = Normalize(values[i], col);
            }
            return result;
        }

        public override double Normalize(double value, int col)
        {
            double result = (value - mins[col])/(maxs[col] - mins[col]);
            return result;
        }

        public double[] MinOfEachColumn(double[][] values)
        {
            double[] result = new double[values[0].Length];
            for (int row=0; row<result.Length; row++)
            {
                result[row] = double.MaxValue;
            }

            for (int col=0; col<values[0].Length; col++)
            {
                for (int row=0; row<values.Length; row++)
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