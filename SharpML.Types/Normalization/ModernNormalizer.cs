using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpML.Types.Normalization
{
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

        public override void Initialize(IEnumerable<double[]> trainMatrix)
        {
            rnnConfig = new RnnConfig();
            rnnConfig.BeginStat(0, trainMatrix.Count(), trainMatrix.First<double[]>().Length, int.MaxValue);
            Stat[] stats = GetStats(trainMatrix.ToArray<double[]>(), 0, trainMatrix.Count());
            foreach (Stat st in stats)
                if (st.Deviance == 0) st.Deviance = 1;
            for (int col = 0; col < trainMatrix.First<double[]>().Length; ++col)
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
}
