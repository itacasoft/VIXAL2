using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using SharpML.Types;
using VIXAL2.UnitTest.Data;
using SharpML.Types.Normalization;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestNormalization
    {
        [TestMethod]
        public void TestNormalizeDenormalizeValue()
        {
            ClassicNormalizer normalizer1 = new ClassicNormalizer();

            double value0 = 12;
            double mean = 10;
            double deviance = 1;

            double value1 = normalizer1.NormalizeValue(value0, mean, deviance);
            double value2 = normalizer1.DecodeValue(value1, mean, deviance);

            Assert.AreEqual(value0, value2);

            ModernNormalizer normalizer2 = new ModernNormalizer();
            double value3 = normalizer2.NormalizeValue(value0, mean, deviance);
            double value4 = normalizer2.DecodeValue(value3, mean, deviance);

            Assert.AreEqual(value0, value2);
        }

        [TestMethod]
        public void TestClassicNormalizer()
        {
            double[] values = new double[7];
            values[0] = 133.23;
            values[1] = 10.65;
            values[2] = 4.23;
            values[3] = 0;
            values[4] = 0.996638;
            values[5] = 31.13;
            values[6] = 20000;

            double[] values1;

            INormalizer normalizer = new ClassicNormalizer();
            normalizer.Initialize(values);

            values1 = normalizer.Normalize(values, 0);
            values1 = normalizer.Decode(values1, 0);

            for (int i = 0; i < values1.Length; i++)
            {
                //verifico che siano molto vicini, ovvero che la loro differenza sia inferiore a una parte per milione
                Assert.IsTrue(Math.Abs(values[i] - values1[i]) < Math.Abs(values[0] / 1000000.0));
            }
        }

        [TestMethod]
        public void TestModernNormalizer()
        {
            double[] values = new double[7];
            values[0] = 133.23;
            values[1] = 10.65;
            values[2] = 4.23;
            values[3] = 0;
            values[4] = 0.996638;
            values[5] = 31.13;
            values[6] = 20000;

            double[] values1;

            INormalizer normalizer = new ModernNormalizer();
            normalizer.Initialize(values);

            values1 = normalizer.Normalize(values, 0);
            values1 = normalizer.Decode(values1, 0);

            for (int i = 0; i < values1.Length; i++)
            {
                //verifico che siano molto vicini, ovvero che la loro differenza sia inferiore a una parte per milione
                Assert.IsTrue(Math.Abs(values[i] - values1[i]) < Math.Abs(values[0] / 1000000.0));
            }
        }

        [TestMethod]
        public void TestMinMaxNormalizer()
        {
            double[] values = new double[7];
            values[0] = 133.23;
            values[1] = 10.65;
            values[2] = 4.23;
            values[3] = 0;
            values[4] = 0.996638;
            values[5] = 31.13;
            values[6] = 20000;

            double[] values1;

            INormalizer normalizer = new MinMaxScaler();
            normalizer.Initialize(values);

            values1 = normalizer.Normalize(values, 0);
            for (int i = 0; i < values1.Length; i++)
            {
                //verifico che siano fra 0 e 1
                Assert.IsTrue((values1[i] <= 1) && (values1[i]>=0));
            }

            values1 = normalizer.Decode(values1, 0);
            for (int i = 0; i < values1.Length; i++)
            {
                //verifico che siano molto vicini, ovvero che la loro differenza sia inferiore a una parte per milione
                Assert.IsTrue(Math.Abs(values[i] - values1[i]) < Math.Abs(values[0] / 1000000.0));
            }
        }


        [TestMethod]
        public void TestNormalizationSingleton()
        {
            INormalizer normalizer = Normalizer.Instance;
            Assert.AreEqual(normalizer.GetType(), typeof(MinMaxScaler));
        }

        [TestMethod]
        public void TestNormalizationMatrix()
        {
            double[][] values = new double[30][];
            for (int row = 0; row < values.Length; row++)
            {
                values[row] = new double[4];
                values[row][0] = EnergyData.Eni[row];
                values[row][1] = EnergyData.ICLN[row];
                values[row][2] = EnergyData.Brent[row];
                values[row][3] = EnergyData.EurDolX[row];
            }

            double[] eni30 = EnergyData.Eni.Take(30).ToArray();
            double maxEni = eni30.Max();
            double minEni = eni30.Min();

            double[] eurodol30 = EnergyData.EurDolX.Take(30).ToArray();
            double maxEur = eurodol30.Max();
            double minEur = eurodol30.Min();


            INormalizer normalization = new ClassicNormalizer();
            normalization.Initialize(values);

            double[][] values1 = normalization.Normalize(values);

            Assert.AreNotEqual(values1[3][3], values[3][3]);
        }

        [TestMethod]
        public void TestMinMaxScaler_MinOfEachColumn()
        {
            double[][] values = new double[3][];
            values[0] = new double[4]
            {
                0.8967576, 0.03783739, 0.75952519, 0.06682827
            };
            values[1] = new double[4]
            {
                0.8354065,   0.99196818,  0.19544769,  0.43447084
            };
            values[2] = new double[4]
            {
                0.66859307,  0.15038721,  0.37911423,  0.6687194
            };

            MinMaxScaler normalizer = new MinMaxScaler();
            double[] mins = normalizer.MinOfEachColumn(values);
            double[] maxs = normalizer.MaxOfEachColumn(values);

            Assert.AreEqual(mins.Length, 4);
            Assert.AreEqual(mins[0], 0.66859307);
            Assert.AreEqual(mins[1], 0.03783739);
            Assert.AreEqual(mins[2], 0.19544769);
            Assert.AreEqual(mins[3], 0.06682827);

            Assert.AreEqual(maxs.Length, 4);
            Assert.AreEqual(maxs[0], 0.8967576);
            Assert.AreEqual(maxs[1], 0.99196818);
            Assert.AreEqual(maxs[2], 0.75952519);
            Assert.AreEqual(maxs[3], 0.6687194);
        }


        [TestMethod]
        public void TestSetGetStats()
        {
            int order = 0;

            double[][] test = new double[EnergyData.Eni.Length][];
            for (int row = 0; row < test.Length; row++)
            {
                test[row] = new double[4];
                test[row][0] = EnergyData.Eni[row];
                test[row][1] = EnergyData.ICLN[row];
                test[row][2] = EnergyData.Brent[row];
                test[row][3] = EnergyData.EurDolX[row];
            }

            ClassicNormalizer normalizer = new ClassicNormalizer();
            normalizer.Initialize(test);
            RnnConfig rnnConfig = normalizer.RnnConfig;

            //assert mean and std deviation
            Stat s0 = rnnConfig.GetStat(order, 0, 0);
            Assert.AreEqual(s0.Mean, Utils.Mean(EnergyData.Eni));
            Assert.AreEqual(s0.Deviance, Utils.StandardDeviation(EnergyData.Eni));

            Stat s1 = rnnConfig.GetStat(order, 0, 1);
            Assert.AreEqual(s1.Mean, Utils.Mean(EnergyData.ICLN));
            Assert.AreEqual(s1.Deviance, Utils.StandardDeviation(EnergyData.ICLN));

            Stat s2 = rnnConfig.GetStat(order, 0, 2);
            Assert.AreEqual(s2.Mean, Utils.Mean(EnergyData.Brent));
            Assert.AreEqual(s2.Deviance, Utils.StandardDeviation(EnergyData.Brent));

            Stat s3 = rnnConfig.GetStat(order, 0, 3);
            Assert.AreEqual(s3.Mean, Utils.Mean(EnergyData.EurDolX));
            Assert.AreEqual(s3.Deviance, Utils.StandardDeviation(EnergyData.EurDolX));

            double input = 12;
            double tr1 = rnnConfig.GetTransformed(0, 0, 0, input);
            double tr2 = ClassicNormalizer.NormalizeValue(input, s0.Mean, s0.Deviance, 0, 1);
            Assert.AreEqual(tr1, tr2);

//            Assert.ThrowsException<IndexOutOfRangeException>(() => rnnConfig.GetStat(order, 0, 4));
        }
    }
}
