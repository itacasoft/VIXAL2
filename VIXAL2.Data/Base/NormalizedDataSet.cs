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

        protected void NormalizeAllData()
        {
            normalizer.Initialize(allData);
            normalizer.NormalizeByRef(allData);
        }
    }
}
