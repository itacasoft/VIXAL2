using SharpML.Types;
using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class TimeSerieDataSet : NormalizedDataSet
    {
        protected int validCount = -1, testCount = -1, trainCount = -1;
        protected double[][] originalData;
        protected List<double[]> allData;
        protected List<DateTime> dates;
        protected string[] colNames;
        protected double[][] trainDataX, trainDataY;
        protected double[][] validDataX, validDataY;
        protected double[][] testDataX, testDataY;
        protected int columnsToPredict;
        protected float trainPercent = 0.60F;
        protected float validPercent = 0.20F;
        protected int predictDays = 10;

        public TimeSerieDataSet(string[] colNames, DateTime[] dates, double[][] data, int columnsToPredict) : base()
        {
            this.colNames = colNames;
            this.allData = new List<double[]>();
            allData.AddRange(data);

            this.dates = new List<DateTime>();
            this.dates.AddRange(dates);
            this.columnsToPredict = columnsToPredict;
            
            originalData = (double[][])data.Clone();
        }

        public override void Prepare()
        {
            int trainTo = Convert.ToInt32(allData.Count * trainPercent);
            trainCount = trainTo;
            int validFrom = trainTo + 1;
            int validTo = Convert.ToInt32(allData.Count * validPercent) + validFrom;
            validCount = validTo - validFrom;
            testCount = allData.Count - validCount - trainCount - predictDays;

            SplitData(allData.ToArray());

            NormalizeData(trainDataX, validDataX, testDataX, true);
            NormalizeData(trainDataY, validDataY, testDataY);

/*            Training = CreateSequencesTraining();
            Validation = CreateSequencesValidation();
            Testing = CreateSequencesTesting();
            InputDimension = Training[0].Steps[0].Input.Rows;
            OutputDimension = Training[0].Steps[0].TargetOutput.Rows;
            LossTraining = new LossSumOfSquares();
            LossReporting = new LossSumOfSquares();
*/
        }

        /// <summary>
        /// This method returns a copy of testDataX array, including the latest PredictGap days
        /// </summary>
        public TimeSerieArray GetTestArrayX()
        {
            TimeSerieArray result = new TimeSerieArray(allData.Count - TrainCount - validCount, allData[0].Length);
            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < result.Columns; col++)
                {
                    result.SetValue(row, col, dates[row + TrainCount + ValidCount], allData[row + TrainCount + ValidCount][col]);
                }
            }

            return result;
        }

        /// <summary>
        /// This method returns a copy of testDataY array
        /// </summary>
        public TimeSerieArray GetTestArrayY(int gap)
        {
            TimeSerieArray result = new TimeSerieArray(allData.Count - TrainCount - validCount - gap, allData[0].Length);
            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < result.Columns; col++)
                {
                    //save the future value (+ gap), but with current date
                    result.SetValue(row, col, dates[row + TrainCount + ValidCount], allData[row + TrainCount + ValidCount + gap][col]);
                }
            }

            return result;
        }


        public Dictionary<string, (float[][] train, float[][] valid, float[][] test)> GetFeatureLabelDataSet()
        {
            var retVal = new Dictionary<string, (float[][] train, float[][] valid, float[][] test)>();

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
                return validCount;
            }
        }

        public int TrainCount
        {
            get
            {
                return trainCount;
            }
        }

        public int TestCount
        {
            get
            {
                return testCount;
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
                    throw new ArgumentOutOfRangeException("PredictDays must be > 1");
                predictDays = value;
            }
        }


        protected void SplitData(double[][] input)
        {
            trainDataX = new double[TrainCount][];
            for (int row = 0; row < trainDataX.Length; row++)
            {
                trainDataX[row] = new double[input[row].Length];
                for (int col = 0; col < trainDataX[row].Length; col++)
                {
                    trainDataX[row][col] = input[row][col];
                }
            }

            trainDataY = new double[TrainCount][];
            for (int row = 0; row < trainDataY.Length; row++)
            {
                trainDataY[row] = new double[columnsToPredict];
                for (int col = 0; col < trainDataY[row].Length; col++)
                {
                    trainDataY[row][col] = input[row + predictDays][col];
                }
            }

            validDataX = new double[ValidCount][];
            for (int row = 0; row < validDataX.Length; row++)
            {
                validDataX[row] = new double[input[row].Length];
                for (int col = 0; col < validDataX[row].Length; col++)
                {
                    validDataX[row][col] = input[row+TrainCount][col];
                }
            }

            validDataY = new double[ValidCount][];
            for (int row = 0; row < validDataY.Length; row++)
            {
                validDataY[row] = new double[columnsToPredict];
                for (int col = 0; col < validDataY[row].Length; col++)
                {
                    validDataY[row][col] = input[row + TrainCount + predictDays][col];
                }
            }

            testDataX = new double[TestCount][];
            for (int row = 0; row < testDataX.Length; row++)
            {
                testDataX[row] = new double[input[row].Length];
                for (int col = 0; col < testDataX[row].Length; col++)
                {
                    testDataX[row][col] = input[row + TrainCount + ValidCount][col];
                }
            }

            testDataY = new double[TestCount][];
            for (int row = 0; row < testDataY.Length; row++)
            {
                testDataY[row] = new double[columnsToPredict];
                for (int col = 0; col < testDataY[row].Length; col++)
                {
                    testDataY[row][col] = input[row + TrainCount + ValidCount + predictDays][col];
                }
            }
        }

        public TimeSerieArray GetColumnData(int col, int gap=0)
        {
            TimeSerieArray result = new TimeSerieArray(allData.Count - gap, 1);

            for (int row=0; row< result.Length; row++)
            {
                result.SetValue(row, 0, dates[row+gap],allData[row+gap][col]);
            }
            return result;
        }

        public List<double[]> AllData
        {
            get
            {
                return allData;
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
    }
}
