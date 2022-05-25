using System;
using System.Collections.Generic;
using VIXAL2.Data.Base;

namespace VIXAL2.Data
{
    public class StocksDataset : TimeSerieDataSet
    {
        List<DatedValueF> _forwardPredicted;
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

            _forwardPredicted = new List<DatedValueF>();
        }

        public override string ClassShortName
        {
            get
            {
                return "StockDs";
            }
        }

        public List<DatedValueF> ForwardPredicted
        {
            get { return _forwardPredicted; }
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
    }
}
