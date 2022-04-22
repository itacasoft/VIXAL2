using SharpML.Types;

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
            Normalizer.Instance.NormalizeByRef(valid);
            Normalizer.Instance.NormalizeByRef(test);
        }
    }
}
