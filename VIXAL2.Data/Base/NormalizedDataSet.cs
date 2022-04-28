using SharpML.Types;
using SharpML.Types.Normalization;
using System.Collections.Generic;

namespace VIXAL2.Data.Base
{
    public abstract class NormalizedDataSet
    {
        protected double[][] originalData;
        protected List<double[]> allData;

        public NormalizedDataSet(double[][] data)
        {
            this.allData = new List<double[]>();
            allData.AddRange(data);
            originalData = (double[][])data.Clone();
        }

        public abstract void Prepare();
        protected void NormalizeData(double[][] train, double[][] valid, double[][] test, bool initialize = false)
        {
            if (initialize)
            {
                double [][] alldata = new double[train.Length+valid.Length+test.Length][];
                train.CopyTo(alldata, 0);
                valid.CopyTo(alldata, train.Length);
                test.CopyTo(alldata, train.Length+valid.Length);

                Normalizer.Instance.Initialize(alldata);
            }

            Normalizer.Instance.NormalizeByRef(train);
            if (valid.Length > 0)
                Normalizer.Instance.NormalizeByRef(valid);
            if (test.Length > 0)
                Normalizer.Instance.NormalizeByRef(test);

        }

        protected void NormalizeAllData()
        {
            Normalizer.Instance.Initialize(allData.ToArray());
            Normalizer.Instance.Normalize(allData.ToArray());
        }
    }
}
