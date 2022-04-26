using SharpML.Types;
using SharpML.Types.Normalization;

namespace VIXAL2.Data.Base
{
    public abstract class NormalizedDataSet
    {
        public NormalizedDataSet()
        {

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
    }
}
