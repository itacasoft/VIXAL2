using SharpML.Types;
using System;

namespace VIXAL2.Data
{
    public class DiscontinuousDataSet : StocksDataset
    {
        public DiscontinuousDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int predictCount) : base(stockNames, dates, allData, predictCount)
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

            for (int col = 0; col < trainDataY[0].Length; col++)
            {
                double[] vectorX = Utils.GetVectorFromArray(trainDataX, col);
                double[] vectorY = Utils.GetVectorFromArray(trainDataY, col);
                double[] stairY = CreateStairVector(vectorX, vectorY);
                Utils.SetArrayFromVector(stairY, col, trainDataY);

                vectorX = Utils.GetVectorFromArray(validDataX, col);
                vectorY = Utils.GetVectorFromArray(validDataY, col);
                stairY = CreateStairVector(vectorX, vectorY);
                Utils.SetArrayFromVector(stairY, col, validDataY);

                vectorX = Utils.GetVectorFromArray(testDataX, col);
                vectorY = Utils.GetVectorFromArray(testDataY, col);
                stairY = CreateStairVector(vectorX, vectorY);
                Utils.SetArrayFromVector(stairY, col, testDataY);
            }
        }
    }
}
