using SharpML.Types;
using System;

namespace VIXAL2.Data
{
    public class DiscontinuousDataSet : StocksDataset
    {
        public DiscontinuousDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public DiscontinuousDataSet(string stockName, DateTime[] dates, double[] singleStockData) : this(Utils.StringToStrings(stockName), dates, Utils.VectorToArray(singleStockData), 0, 1)
        {
        }

        public override string ClassShortName
        {
            get
            {
                return "DiscontinuousDS";
            }
        }

        public static double[] CreateStairVector(double[] dataX, double[] dataY)
        {
            if (dataX.Length != dataY.Length)
                throw new ArgumentException("dataX and dataY must have the same lenght");

            double [] stairVector = new double[dataX.Length];
            for (int i= 0; i < dataX.Length; i++)
            {
                double raisePerc = (dataY[i] - dataX[i]) / dataX[i];

                if (raisePerc >= 0.05)
                {
                    stairVector[i] = 0.2;
                }
                else if ((raisePerc >= 0.02) && (raisePerc < 0.05))
                {
                    stairVector[i] = 0.1;
                }
                else if ((raisePerc <= -0.02) && (raisePerc > -0.05))
                {
                    stairVector[i] = -0.1;
                }
                else if (raisePerc <= -0.05)
                {
                    stairVector[i] = -0.2;
                }
                else
                {
                    stairVector[i] = 0;
                }
            }

            return stairVector;
        }

        public override void Prepare(int validCount, int testCount)
        {
            ReloadFromOriginal();
            base.Prepare(validCount, testCount);

            for (int col = 0; col < TrainDataY[0].Length; col++)
            {
                double[] vectorX = SharpML.Types.Utils.GetVectorFromArray(TrainDataX, col);
                double[] vectorY = SharpML.Types.Utils.GetVectorFromArray(TrainDataY, col);
                double[] stairY = CreateStairVector(vectorX, vectorY);
                SharpML.Types.Utils.SetArrayFromVector(stairY, col, TrainDataY);

                vectorX = SharpML.Types.Utils.GetVectorFromArray(ValidDataX, col);
                vectorY = SharpML.Types.Utils.GetVectorFromArray(ValidDataY, col);
                stairY = CreateStairVector(vectorX, vectorY);
                SharpML.Types.Utils.SetArrayFromVector(stairY, col, ValidDataY);

                vectorX = SharpML.Types.Utils.GetVectorFromArray(TestDataX, col);
                vectorY = SharpML.Types.Utils.GetVectorFromArray(TestDataY, col);
                stairY = CreateStairVector(vectorX, vectorY);
                SharpML.Types.Utils.SetArrayFromVector(stairY, col, TestDataY);
            }
        }
    }
}
