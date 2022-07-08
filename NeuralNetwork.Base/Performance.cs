using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork.Base
{
    public class Performance
    {
        public int Guessed = 0;
        public int Failed = 0;
        public DateTime Date; 

        public int Total
        {
            get
            { 
                return Guessed + Failed;
            }
        }
        public float SuccessPercentage
        {
            get
            {
                float result = (float)Guessed / (float)(Guessed + Failed);
                return result;
            }
        }

        public override string ToString()
        {
            return Date.ToShortDateString() + "; guessed " + Guessed + "/" + Total + " (" + SuccessPercentage.ToString("F2") + ")";
        }
    }
}
