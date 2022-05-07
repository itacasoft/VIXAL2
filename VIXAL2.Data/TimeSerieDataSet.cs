using SharpML.Types;
using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;
using Accord.Math;
using System.Linq;

namespace VIXAL2.Data
{
    public class TimeSerieDataSet : NormalizedDataSet
    {
        private List<DateTime> dates;
        private string[] colNames;
        private double[][] trainDataX, trainDataY;
        private double[][] validDataX, validDataY;
        private double[][] testDataX, testDataY;
        private float trainPercent = 0.60F;
        private float validPercent = 0.20F;
        private int predictDays = 20;
        private bool prepared = false;
        protected int firstColumnToPredict;
        protected int columnsToPredict;

        public TimeSerieDataSet(string[] colNames, DateTime[] dates, double[][] data, int firstColumnToPredict, int columnsToPredict) : base(data)
        {
            this.colNames = colNames;
            this.dates = new List<DateTime>();
            this.dates.AddRange(dates);
            this.columnsToPredict = columnsToPredict;
            this.firstColumnToPredict = firstColumnToPredict;
        }

        public override void Prepare()
        {
            NormalizeAllData();

            SplitData(dataList.ToArray());

            //            NormalizeData(trainDataX, validDataX, testDataX, true);
            //            NormalizeData(trainDataY, validDataY, testDataY);

            /*            Training = CreateSequencesTraining();
                        Validation = CreateSequencesValidation();
                        Testing = CreateSequencesTesting();
                        InputDimension = Training[0].Steps[0].Input.Rows;
                        OutputDimension = Training[0].Steps[0].TargetOutput.Rows;
                        LossTraining = new LossSumOfSquares();
                        LossReporting = new LossSumOfSquares();
            */
            prepared = true;
        }

        /// <summary>
        /// This method returns a copy of testDataX array, including the latest PredictGap days
        /// </summary>
        public TimeSerieArray GetTestArrayExtendedX()
        {
            TimeSerieArray result = new TimeSerieArray(dataList.Count - TrainCount - ValidCount, dataList[0].Length);
            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < result.Columns; col++)
                {
                    result.SetValue(row, col, dates[row + TrainCount + ValidCount], dataList[row + TrainCount + ValidCount][col]);
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns a copy of testDataX array, excluding the latest PredictGap days
        /// </summary>
        public TimeSerieArray GetTestArrayX()
        {
            TimeSerieArray result = new TimeSerieArray(dataList.Count - TrainCount - ValidCount - predictDays, dataList[0].Length);
            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < result.Columns; col++)
                {
                    result.SetValue(row, col, dates[row + TrainCount + ValidCount], dataList[row + TrainCount + ValidCount][col]);
                }
            }

            return result;
        }


        /// <summary>
        /// This method returns a copy of dataX for latest PredictGap days 
        /// </summary>
        public TimeSerieArray GetExtendedArrayX()
        {
            TimeSerieArray result = new TimeSerieArray(dataList.Count - TrainCount - ValidCount - TestCount, dataList[0].Length);
            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < result.Columns; col++)
                {
                    result.SetValue(row, col, dates[row + TrainCount + ValidCount + TestCount], dataList[row + TrainCount + ValidCount + TestCount][col]);
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns a copy of testDataY array
        /// </summary>
        public TimeSerieArray GetTestArrayY()
        {
            TimeSerieArray result = new TimeSerieArray(dataList.Count - TrainCount - ValidCount - predictDays, columnsToPredict);
            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < columnsToPredict; col++)
                {
                    //save the future value (+ gap), but with current date
                    result.SetValue(row, col, dates[row + TrainCount + ValidCount], dataList[row + TrainCount + ValidCount + predictDays][firstColumnToPredict+col]);
                }
            }

            for (int col = 0; col < columnsToPredict; col++)
            {
                result.SetColName(col, colNames[firstColumnToPredict + col]);
            }

            return result;
        }


        public Dictionary<string, (float[][] train, float[][] valid, float[][] test)> GetFeatureLabelDataSet()
        {
            var retVal = new Dictionary<string, (float[][] train, float[][] valid, float[][] test)>();

            float[][] extendedTestDataX = Utils.ToFloatArray(testDataX);

#if EXTENDED_TEST
            var exte = Utils.ToFloatArray(Normalizer.Instance.Normalize(this.GetExtendedArrayX().Values));
            extendedTestDataX = extendedTestDataX.Stack<float>(exte);
#endif

            var xxx = (Utils.ToFloatArray(trainDataX), Utils.ToFloatArray(validDataX), Utils.ToFloatArray(testDataX));
            var yyy = (Utils.ToFloatArray(trainDataY), Utils.ToFloatArray(validDataY), Utils.ToFloatArray(testDataY));
            retVal.Add("features", xxx);
            retVal.Add("label", yyy);

            return retVal;
        }

        public (float[][] train, float[][] valid, float[][] test) this[string key]
        {
            get
            {
                var res = GetFeatureLabelDataSet();
                return res[key];
            }
        }
        
        public int ValidCount
        {
            get
            {
                return validDataX.Length;
            }
        }

        public int TrainCount
        {
            get
            {
                return trainDataX.Length;
            }
        }

        public int TestCount
        {
            get
            {
                return testDataX.Length;
            }
        }

        public float TrainPercent
        {
            get
            {
                return trainPercent;
            }
            set
            {
                if ((value > 1) || (value < 0))
                    throw new ArgumentOutOfRangeException("TrainPercent must be between 0 and 1");
                if (prepared)
                    throw new InvalidOperationException("TrainPercent cannot be changed when DataSet is prepared");

                trainPercent = value;
            }
        }

        public float ValidPercent
        {
            get
            {
                return validPercent;
            }
            set
            {
                if ((value > 1) || (value < 0))
                    throw new ArgumentOutOfRangeException("ValidPercent must be between 0 and 1");

                if (prepared)
                    throw new InvalidOperationException("ValidPercent cannot be changed when DataSet is prepared");

                validPercent = value;
            }

        }

        public int PredictDays
        {
            get
            {
                return predictDays;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("PredictDays must be > 0");
                predictDays = value;
            }
        }


        protected void SplitData(double[][] input)
        {
            int trainTo = Convert.ToInt32(dataList.Count * trainPercent);
            int trainCount = trainTo;
            int validFrom = trainTo + 1;
            int validTo = Convert.ToInt32(dataList.Count * validPercent) + validFrom;
            int validCount = validTo - validFrom;
            int testCount = dataList.Count - validCount - trainCount - predictDays;

            trainDataX = new double[trainCount][];
            for (int row = 0; row < trainDataX.Length; row++)
            {
                trainDataX[row] = new double[input[row].Length];
                for (int col = 0; col < trainDataX[row].Length; col++)
                {
                    trainDataX[row][col] = input[row][col];
                }
            }

            trainDataY = new double[trainCount][];
            for (int row = 0; row < trainDataY.Length; row++)
            {
                trainDataY[row] = new double[columnsToPredict];
                for (int col = 0; col < trainDataY[row].Length; col++)
                {
                    trainDataY[row][col] = input[row + predictDays][col + firstColumnToPredict];
                }
            }

            validDataX = new double[validCount][];
            for (int row = 0; row < validDataX.Length; row++)
            {
                validDataX[row] = new double[input[row].Length];
                for (int col = 0; col < validDataX[row].Length; col++)
                {
                    validDataX[row][col] = input[row+TrainCount][col];
                }
            }

            validDataY = new double[validCount][];
            for (int row = 0; row < validDataY.Length; row++)
            {
                validDataY[row] = new double[columnsToPredict];
                for (int col = 0; col < validDataY[row].Length; col++)
                {
                    validDataY[row][col] = input[row + TrainCount + predictDays][col + firstColumnToPredict];
                }
            }

            testDataX = new double[testCount][];
            for (int row = 0; row < testDataX.Length; row++)
            {
                testDataX[row] = new double[input[row].Length];
                for (int col = 0; col < testDataX[row].Length; col++)
                {
                    testDataX[row][col] = input[row + TrainCount + ValidCount][col];
                }
            }

            testDataY = new double[testCount][];
            for (int row = 0; row < testDataY.Length; row++)
            {
                testDataY[row] = new double[columnsToPredict];
                for (int col = 0; col < testDataY[row].Length; col++)
                {
                    testDataY[row][col] = input[row + TrainCount + ValidCount + predictDays][col + firstColumnToPredict];
                }
            }
        }

        public TimeSerieArray GetColumnData(int col, int gap=0)
        {
            TimeSerieArray result = new TimeSerieArray(dataList.Count - gap, 1);

            for (int row=0; row< result.Length; row++)
            {
                result.SetValue(row, 0, dates[row+gap],dataList[row+gap][col]);
            }
            return result;
        }

        public List<double[]> DataList
        {
            get
            {
                return dataList;
            }
        }

        public double[][] TrainDataX
        {
            get
            {
                return trainDataX;
            }
        }

        public double[][] TrainDataY
        {
            get
            {
                return trainDataY;
            }
        }

        public double[][] ValidDataX
        {
            get
            {
                return validDataX;
            }
        }

        public double[][] ValidDataY
        {
            get
            {
                return validDataY;
            }
        }

        public double[][] TestDataX
        {
            get
            {
                return testDataX;
            }
        }

        public double[][] TestDataY
        {
            get
            {
                return testDataY;
            }
        }


        public virtual string ClassShortName
        {
            get
            {
                return "TimeSerieDs";
            }
        }

        public DateTime MinDate
        {
            get
            {
                return dates[0];
            }
        }

        public DateTime MaxDate
        {
            get
            {
                return dates[dates.Count - 1];
            }
        }

        public double MinYValue
        {
            get
            {
                double result1 = double.MaxValue;
                double result2 = double.MaxValue;
                double result3 = double.MaxValue;

                if (trainDataY.Length > 0)
                    result1 = trainDataY.Min<double>();
                
                if (validDataY.Length > 0)
                    result2 = validDataY.Min<double>();

                if (testDataY.Length > 0)
                    result3 = testDataY.Min<double>();

                if ((result1 < result2) && (result1 < result3)) return result1;
                if ((result2 < result1) && (result2 < result3)) return result2;
                return result3;
            }
        }

        public double MaxYValue
        {
            get
            {
                double result1 = double.MinValue;
                double result2 = double.MinValue;
                double result3 = double.MinValue;

                if (trainDataY.Length > 0)
                    result1 = trainDataY.Max<double>();
                if (validDataY.Length > 0)
                    result2 = validDataY.Max<double>();
                if (testDataY.Length > 0)
                    result3 = testDataY.Max<double>();

                if ((result1 > result2) && (result1 > result3)) return result1;
                if ((result2 > result1) && (result2 > result3)) return result2;
                return result3;
            }
        }


        public List<DateTime> Dates
        {
            get
            {
                return dates;
            }
        }

        public string[] ColNames
        {
            get
            {
                return colNames;
            }
        }

       
        protected void RemoveNaNs(List<double[]> mydata, List<DateTime> mydates)
        {
            for (int row = mydata.Count-1; row >= 0; row--)
            {
                bool removeThis = false;
                for (int col = 0; col < mydata[row].Length; col++)
                {
                    if (double.IsNaN(mydata[row][col])) removeThis = true;
                }

                if (removeThis)
                {
                    mydata.RemoveAt(row);
                    mydates.RemoveAt(row);
                }
            }
        }

        public bool Forward(int steps = 1)
        {
            if (ValidCount > 0)
            {
                throw new InvalidOperationException("Forward cannot be used if ValidCount > 0");
            }

            if (steps >= TestCount)
                return false;

            for (int i = 0; i < steps; i++)
            {
                //take first row of test if any
                double[] dataXToMove = TestDataX[0];
                trainDataX = trainDataX.InsertRow(dataXToMove);
                testDataX = testDataX.RemoveAt(0);

                double[] dataYToMove = TestDataY[0];
                trainDataY = trainDataY.InsertRow(dataYToMove);
                testDataY = testDataY.RemoveAt(0);
            }

            return true;
        }
    }
}
