using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data.Base
{
    public interface IAverageRangeDataSet
    {
        int Range
        {
            get;
        }

        void SetRange(int value);
    }
}
