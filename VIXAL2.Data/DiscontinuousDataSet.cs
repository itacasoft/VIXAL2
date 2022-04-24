using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data
{
    public class DiscontinuousDataSet : StocksDataset
    {
        public DiscontinuousDataSet(string[] stockNames, DateTime[] dates, double[][] allData, int predictCount) : base(stockNames, dates, allData, predictCount)
        {
        }

        public override string ClassShortName
        {
            get
            {
                return "DiscontinuousDS";
            }
        }

        public override void Prepare()
        {
            base.Prepare();

            for (int row = 0; row < trainDataY.Length; row++)
            {
            }
        }
    }
}
