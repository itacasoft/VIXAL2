using SharpML.Types;
using SharpML.Types.Normalization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace VIXAL2.Data.Base
{
    public abstract class NormalizedDataSet
    {
        private double[][] _originalData;
        protected double[][] originalData
        {
            get 
            {
                return _originalData; 
            }
            set 
            {
                _originalData = value;
                allData = _originalData.ToList<double[]>();
            }
        }

        protected List<double[]> allData;
        protected INormalizer normalizer;

        public NormalizedDataSet(double[][] data)
        {
            originalData = data;
            string normalizerType = ConfigurationManager.AppSettings["NormalizerType"];
            normalizer = Normalizer.Constructor(normalizerType);
        }

        public abstract void Prepare();

        [ObsoleteAttribute("This method is obsolete. Call NormalizeAllData instead.", false)]
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
            normalizer.Initialize(allData);
            normalizer.NormalizeByRef(allData);
        }
    }
}
