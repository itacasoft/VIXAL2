using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data.Base
{
    public class PredictedCurve
    {
        public DateTime FirstPredictionDate;
        public List<DatedValue> Predicted;

        public PredictedCurve()
        {
            Predicted = new List<DatedValue>();
        }
    }

    public class PredictedData
    {
        public List<PredictedCurve> PredictedStack;
        public List<DatedValue> OriginalData;
        public string StockName;

        public PredictedData(List<DatedValue> originalData)
        {
            OriginalData = originalData;
            PredictedStack = new List<PredictedCurve>();
        }

        public void AddPredictedCurve(List<DoubleDatedValue> predicted)
        {
            //verifico che le date siano presenti nell'Original
            for (int i=0; i<predicted.Count; i++)
            {
                var c = GetOriginalValue(predicted[i].Date);
                if (c == Double.NaN)
                    throw new DataMisalignedException("Date " + predicted[i].Date.ToString() + " not present in original data");
            }

            //verifico che la prima data sia la seconda del precedente predicted
            if (PredictedStack.Count > 0)
            {
                if (predicted[0].Date.Date != PredictedStack[PredictedStack.Count - 1].Predicted[1].Date.Date)
                    throw new DataMisalignedException("Date " + predicted[0].Date.ToString() + " not present in previous predicted curve");
            }
            else if (PredictedStack.Count == 0)
            {
                if (predicted[0].Date.Date != OriginalData[0].Date.Date)
                    throw new DataMisalignedException("Date " + predicted[0].Date.ToString() + " is not the first of original data");
            }

            var item = new PredictedCurve();
            item.FirstPredictionDate = predicted[0].PredictionDate;

            for (int i = 0; i < predicted.Count; i++)
            {
                item.Predicted.Add(new DatedValue(predicted[i].Date.Date, predicted[i].Value));
            }

            PredictedStack.Add(item);
        }

        public List<DateTime> GetDates()
        {
            var result = new List<DateTime>();
            for (int i=0; i< OriginalData.Count; i++)
            {
                result.Add(OriginalData[i].Date);
            }

            return result;
        }

        public Double GetOriginalValue(DateTime date)
        {
            for (int i=0; i<OriginalData.Count; i++)
            {
                if (OriginalData[i].Date.Date == date.Date)
                    return GetOriginalValue(i);
            }

            return Double.NaN;
        }

        public Double GetOriginalValue(int index)
        {
            return OriginalData[index].Value;
        }

        public PredictedCurve GetPredictedCurve(DateTime date)
        {
            for (int i = 0; i < OriginalData.Count; i++)
            {
                if (OriginalData[i].Date.Date == date.Date)
                    return GetPredictedCurve(i);
            }

            return null;
        }

        public PredictedCurve GetPredictedCurve(int index)
        {
            return this.PredictedStack[index];
        }
    }
}
