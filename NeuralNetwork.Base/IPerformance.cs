using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Base
{
    public interface IPerformance
    {
        float SuccessPercentage
        {
            get;
        }

        float FailedPercentage
        {
            get;
        }


        DateTime Date
        {
            get; set;
        }
    }
}
