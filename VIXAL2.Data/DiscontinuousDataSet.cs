using SharpML.Types;
using System;

namespace VIXAL2.Data
{
    public class DiscontinuousDataSet : StocksDataset
    {
        public DiscontinuousDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
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

        public override void Prepare()
        {
            base.Prepare();

            for (int col = 0; col < TrainDataY[0].Length; col++)
            {
                double[] vectorX = Utils.GetVectorFromArray(TrainDataX, col);
                double[] vectorY = Utils.GetVectorFromArray(TrainDataY, col);
                double[] stairY = CreateStairVector(vectorX, vectorY);
                Utils.SetArrayFromVector(stairY, col, TrainDataY);

                vectorX = Utils.GetVectorFromArray(ValidDataX, col);
                vectorY = Utils.GetVectorFromArray(ValidDataY, col);
                stairY = CreateStairVector(vectorX, vectorY);
                Utils.SetArrayFromVector(stairY, col, ValidDataY);

                vectorX = Utils.GetVectorFromArray(TestDataX, col);
                vectorY = Utils.GetVectorFromArray(TestDataY, col);
                stairY = CreateStairVector(vectorX, vectorY);
                Utils.SetArrayFromVector(stairY, col, TestDataY);
            }
        }
    }
}
