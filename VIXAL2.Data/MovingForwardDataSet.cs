using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class MovingForwardDataSet : StocksDataset, IAverageRangeDataSet
    {
        protected int range = 20;

        public MovingForwardDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) : base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
        }

        public int Range
        {
            get { return range; }
            set { range = value; }
        }

        public bool Forward(int steps = 1)
        {
            bool result = false;
            if (ValidCount > 0)
            {
                throw new InvalidOperationException("Forward cannot be used if ValidCount > 0");
            }

            for (int i = 0; i < steps; i++)
            {
                if (TestCount > 0)
                {
                    result = true;
                    //take first row of test if any
                    double[] dataXToMove = TestDataX[0];
                    TrainDataX.Append(dataXToMove);
                    TestDataX.RemoveAt(0);

                    double[] dataYToMove = TestDataY[0];
                    TrainDataY.Append(dataYToMove);
                    TestDataY.RemoveAt(0);
                }
            }

            return result;
        }
    }
}
