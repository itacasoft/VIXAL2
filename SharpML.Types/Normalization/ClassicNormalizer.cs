using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpML.Types.Normalization
{
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
            return ClassicNormalizer.NormalizeValue(value, 0, 1, mean, deviance);
        }

        public double DecodeValue(double value, double mean, double deviance)
        {
            return ClassicNormalizer.NormalizeValue(value, mean, deviance, 0, 1);
        }

        public override double[] Normalize(double[] values, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double[] result = new double[values.Length];
            Stat stat = rnnConfig.GetStat(0, 0, col);

            for (int row = 0; row < result.Length; row++)
            {
                result[row] = ClassicNormalizer.NormalizeValue(values[row], 0, 1, stat.Mean, stat.Deviance);
            }
            return result;
        }


        public override double Normalize(double value, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            Stat stat = rnnConfig.GetStat(0, 0, col);
            double result = ClassicNormalizer.NormalizeValue(value, 0, 1, stat.Mean, stat.Deviance);
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

        public override List<double> Decode(List<double> values, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            List<double> result = new List<double>();
            Stat stat = rnnConfig.GetStat(0, 0, col);

            for (int row = 0; row < values.Count; row++)
            {
                result.Add(rnnConfig.GetTransformed(0, 0, col, values[row]));
            }
            return result;
        }


        public override double Decode(double value, int col)
        {
            if (!initialized) throw new InvalidOperationException("INormalizer not initialized");

            double result = rnnConfig.GetTransformed(0, 0, col, value);
            return result;
        }


        public override void Initialize(IEnumerable<double[]> trainMatrix)
        {
            rnnConfig = new RnnConfig();
            //vecchio fn-trading: salvo le statistiche in RnnConfig
            rnnConfig.BeginStat(0, trainMatrix.Count(), trainMatrix.First<double[]>().Length, int.MaxValue);
            Stat[] stats = GetStats(trainMatrix.ToArray<double[]>(), 0, trainMatrix.Count());
            foreach (Stat st in stats)
                if (st.Deviance == 0)
                    st.Deviance = 1;
            for (int col = 0; col < trainMatrix.First<double[]>().Length; ++col)
                rnnConfig.SetStat(0, 0, col, stats[col]);

            initialized = true;
        }


        public static double NormalizeValue(double value, double newMean, double newStd, double oldMean, double oldStd)
        {
            return (value - oldMean) * (newStd / oldStd) + newMean;
        }
    }
}
