using SharpML.Types;
using System;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class StocksDataset : TimeSerieDataSet
    {
        private static int _initialTestCount = -1;
        public static int InitialTestCount
        {
            get
            {
                return _initialTestCount;
            }
            set
            {
                _initialTestCount = value;
            }
        }

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

        public StocksDataset(string stockName, DateTime[] dates, double[] singleStockData) : this(Utils.StringToStrings(stockName), dates, Utils.VectorToArray(singleStockData), 0, 1)
        {
        }

        public override string ClassShortName
        {
            get
            {
                return "StockDs";
            }
        }

        public virtual DataSetType DsType
        {
            get
            {
                return DataSetType.Normal;
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

        /// <summary>
        /// Ritardo fra l'inizio della curva del dataset e i dati originali
        /// </summary>
        /// <remarks>Tipicamente, le medie mobili hanno un ritardo rispetto ai dati originali</remarks>
        public virtual int DaysGapAtStart
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

        /// <summary>
        /// Ritardo fra quando finisce la curva del dataset e quella dei dati originali
        /// </summary>
        public virtual int DaysGapAtEnd
        {
            get
            {
                return 0;
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

        protected DateTime GetFutureStockDate(DateTime date, int days)
        {
            DateTime? resultFromOriginal = null;
            int remainingDays = 0;

            for (int i = 0; i < OriginalData.Dates.Length; i++)
            {
                if (OriginalData.Dates[i] == date)
                {
                    //se il calendario dei dati originali non è abbastanza lungo
                    //calcolo quanti giorni mi mancano e per il momento prendo l'ultimo giorno
                    if (i + days > OriginalData.Dates.Length-1)
                    {
                        remainingDays = days - (OriginalData.Dates.Length - 1 - i);
                        resultFromOriginal = OriginalData.Dates[OriginalData.Dates.Length - 1];
                    }
                    else
                        resultFromOriginal = OriginalData.Dates[i + days];
                    break;
                }
            }

            //se non trovo valori nel calendario, come fallback me lo calcolo io
            if (resultFromOriginal == null)
            {
                var resultFromBusiness = SharpML.Types.Utils.AddBusinessDays(date, days);
                return resultFromBusiness;
            }

            //se rimangono dei giorni da calcolare, me li calcolo io
            if (remainingDays > 0)
            {
                resultFromOriginal = SharpML.Types.Utils.AddBusinessDays(resultFromOriginal.Value, remainingDays);
            }

            return resultFromOriginal.Value;
        }

        /// <summary>
        /// This method returns a copy of dataX for latest PredictGap days 
        /// </summary>
        public virtual TimeSerieArrayExt GetExtendedArrayX(bool normalized = false)
        {
            TimeSerieArrayExt result = new TimeSerieArrayExt(_data.Count - TrainCount - ValidCount - base.TestCount, _data[0].Length);
            result.PredictDays = PredictDays;

            for (int row = 0; row < result.Length; row++)
            {
                DateTime date = dates[row + TrainCount + ValidCount + base.TestCount];
                DateTime futureDate = GetFutureStockDate(dates[row + TrainCount + ValidCount + base.TestCount], PredictDays);

                for (int col = 0; col < result.Columns; col++)
                {
                    var currentValue = _data[row + TrainCount + ValidCount + base.TestCount][col];
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
