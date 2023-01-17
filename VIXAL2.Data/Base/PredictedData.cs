using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data.Base
{
    public class PredictedCurve
    {
        public int Sample;
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

        public PredictedData(List<DatedValue> originalData, List<DoubleDatedValue> predictedList)
        {
            OriginalData = originalData;
            PredictedStack = new List<PredictedCurve>();
        }

        public void AddPredictedCurve(List<DoubleDatedValue> predicted, int sample)
        {
            var item = new PredictedCurve();
            item.Sample = sample;
            item.FirstPredictionDate = predicted[0].PredictionDate;

            for (int i = 0; i < predicted.Count; i++)
            {
                item.Predicted.Add(new DatedValue(predicted[i].Date, predicted[i].Value));
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
    }
}
