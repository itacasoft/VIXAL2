using System;
using System.Collections.Generic;
using System.Linq;
using VIXAL2.Data.Base;

namespace NeuralNetwork.Base
{
    public static class LSTMUtils
    {
        public static void CompareSlopes(double[] dataY, double[] dataPredicted, ref List<Performance> performances)
        {
            double predicted0 = dataPredicted[0];
            double future0 = dataY[0];
            if (performances.Count == 0) performances.Add(new Performance());

            for (int row = 1; row < dataY.Length; row++)
            {
                double future1 = dataY[row];
                bool futurePositiveTrend = (future1 > future0);

                double predicted1 = dataPredicted[row];
                bool predictedPositiveTrend = (predicted1 > predicted0);

                if (performances.Count <= row) performances.Add(new Performance());

                if (predictedPositiveTrend == futurePositiveTrend)
                {
                    performances[row].Guessed++;
                }
                else
                {
                    performances[row].Failed++;
                }

                predicted0 = predicted1;
                future0 = future1;
            }
        }

        public static void CompareDifferences(double[] dataY, double[] dataPredicted, ref List<PerformanceDiff> performances)
        {
            for (int row = 0; row < dataY.Length; row++)
            {
                if (performances.Count <= row) performances.Add(new PerformanceDiff());

                performances[row].Predicted = (float)dataPredicted[row];
                performances[row].Real = (float)dataY[row];
            }
        }


        /// <summary>
        /// Compare differences between 2 series and returns the average result
        /// </summary>
        /// <param name="dataY"></param>
        /// <param name="dataPredicted"></param>
        /// <returns>0 means perfect match, values below 0.03 are good, while a number > 1 means a very poor result</returns>
        public static double CompareDifferences(IEnumerable<double> dataY, IEnumerable<double> dataPredicted)
        {
            //calcolo il delta della media delle due serie
            double delta = dataY.Average() - dataPredicted.Average();
            //questo delta va aggiunto a tutti i valori di Predicted per compensare la differenza in Y
            var lPredicted = dataPredicted.ToList<double>();
            var lDataY = dataY.ToList<double>();

            for(int i=0; i<lPredicted.Count;i++)
            {
                lPredicted[i] += delta;
            }
            
            double[] differences = new double[lPredicted.Count];    

            for (int i=0; i<lPredicted.Count; i++)
            {
                //calcolo la differenza fra ogni valore omologo in percentuale
                differences[i] = Math.Abs((lPredicted[i] - lDataY[i])/lDataY[i]);
            }

            return differences.Average();
        }
    }
}
