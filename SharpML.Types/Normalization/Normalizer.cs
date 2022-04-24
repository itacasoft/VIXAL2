using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SharpML.Types.Normalization
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
                _instance = new FakeNormalizer();
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

        public void Initialize(double[][] data, int indexTo)
        {
            double[][] myarray = new double[indexTo][];

            for (int i=0; i<indexTo; i++)
            {
                myarray[i] = data[i];
            }

            Initialize(myarray);
        }

        public abstract void Initialize(double[][] trainMatrix);

    }
}