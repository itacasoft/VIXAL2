using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class StocksDataset : TimeSerieDataSet
    {
        TimeSerieArray originalData;

        public Object Obj1;
        public Object Obj2;

        public StocksDataset(string[] stockNames, DateTime[] dates, double[][] allData, int firstColumnToPredict, int predictCount) :
            base(stockNames, dates, allData, firstColumnToPredict, predictCount)
        {
            originalData = new TimeSerieArray(stockNames, dates, allData);

            if (allData.Length != dates.Length)
                throw new ArgumentException("Date lenght is not the same of Data lenght");

            if (allData[0].Length != stockNames.Length)
                throw new ArgumentException("Names lenght is not the same of DataRow lenght");

            if (predictCount > allData[0].Length)
                throw new ArgumentException("Columns to predict is larger than input columns");
        }

        public override string ClassShortName
        {
            get
            {
                return "StockDs";
            }
        }

        public TimeSerieArray OriginalData
        {
            get
            {
                return originalData;
            }
        }


        public TimeSerieArray OriginalNormalizedData
        {
            get
            {
                double[][] data1 = normalizer.Normalize(originalData.Values);
                TimeSerieArray result = new TimeSerieArray(originalData.ColNames, originalData.Dates, data1);
                return result;
            }
        }

        public virtual int Range
        {
            get { return 1; }
        }

        public virtual int DelayDays
        {
            get 
            {
                //int result = this.Range / 2;
                int result = 0;
                for (int i = 0; i < this.OriginalData.Dates.Length; i++)
                {
                    if (this.OriginalData.Dates[i] == this.MinDate)
                    {
                        break;
                    }

                    result++;
                }
                return result;
            }
        }

        public override TimeSerieArrayExt GetTrainArrayY()
        {
            var result = base.GetTrainArrayY();
            result.Range = this.Range;
            return result;
        }

        public override TimeSerieArrayExt GetValidArrayY()
        {
            var result = base.GetValidArrayY();
            result.Range = this.Range;
            return result;
        }

        public override TimeSerieArrayExt GetTestArrayY()
        {
            var result = base.GetTestArrayY();
            result.Range = this.Range;
            return result;
        }

        private DateTime GetFutureStockDate(DateTime date, int days)
        {
            DateTime? result = null;

            for (int i = 0; i < OriginalData.Dates.Length - days; i++)
            {
                if (dates[i] == date)
                {
                    result = OriginalData.Dates[i + days];
                    break;
                }
            }

            if (result == null)
                result = SharpML.Types.Utils.AddBusinessDays(date, days);

            return result.Value;
        }

        /// <summary>
        /// This method returns a copy of dataX for latest PredictGap days 
        /// </summary>
        public virtual TimeSerieArrayExt GetExtendedArrayX(bool normalized = false)
        {
            TimeSerieArrayExt result = new TimeSerieArrayExt(dataList.Count - TrainCount - ValidCount - TestCount, dataList[0].Length);
            result.PredictDays = PredictDays;

            for (int row = 0; row < result.Length; row++)
            {
                for (int col = 0; col < result.Columns; col++)
                {
                    DateTime date = dates[row + TrainCount + ValidCount + TestCount];
                    DateTime futureDate = GetFutureStockDate(dates[row + TrainCount + ValidCount + TestCount], PredictDays + 1);
                    var currentValue = dataList[row + TrainCount + ValidCount + TestCount][col];
                    if (normalized)
                        currentValue = Normalize(currentValue, col);
                    result.SetValue(row, col, date, futureDate, currentValue);
                }
            }

            result.Range = this.Range;
            return result;
        }
    }
}
