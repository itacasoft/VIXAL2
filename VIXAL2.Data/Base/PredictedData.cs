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
        public DateTime PredictionDate, Date;
        public List<DoubleDatedValue> Predicted;
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
            item.PredictionDate = predicted[0].PredictionDate;
            item.Date = predicted[0].Date;

            PredictedStack.Add(item);
        }
    }
}
