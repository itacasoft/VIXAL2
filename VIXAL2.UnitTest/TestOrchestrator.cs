using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetwork.Base;
using System;
using VIXAL2.Data;
using VIXAL2.UnitTest.Data;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestOrchestrator
    {
        private MovingEnhancedAverageDataSet GetMovingEnhancedAverageDataset(int predictDays, int columnToPredict)
        {
            const int PREDICT_COUNT = 1;

            DateTime[] dates = EnergyData.Dates;
            double[][] data = EnergyData.AllData;
            string[] stockNames = EnergyData.StockNames;
            Assert.AreEqual(dates.Length, data.Length);
            Assert.AreEqual(stockNames.Length, data[0].Length);

            var ds = new MovingEnhancedAverageDataSet(stockNames, dates, data, columnToPredict, PREDICT_COUNT);
            ds.PredictDays = predictDays;

            return ds;
        }

        [TestMethod]
        public void Test_orchestrator_netxbatches_for_train()
        {
            const int PREDICT_DAYS = 20;
            const int RANGE = 10;
            const int COLUMN_TO_PREDICT = 2;

            var orchestrator = new LSTMOrchestrator(null, null, null, null, 100);
            var ds = GetMovingEnhancedAverageDataset(PREDICT_DAYS, COLUMN_TO_PREDICT);

            orchestrator.LoadAndPrepareDataSet(ds, PREDICT_DAYS, RANGE);

            var trainDataY = ds.GetTrainArrayY();
            ds.Normalize(ref trainDataY, COLUMN_TO_PREDICT);

            var batch = orchestrator.GetBatchesForTraining();

            var miniBatchData = batch.First();
            //mi aspetto lunghezza di 100 per i dati di training
            Assert.IsTrue(miniBatchData.Y.Length == 100);

            Assert.IsTrue(trainDataY.Values.Length > 100);

            //mi aspetto che i valori presi da trainDataY e dal batch siano uguali
            Assert.AreEqual(miniBatchData.Y[0], (float)trainDataY.Values[0][0]);
            Assert.AreEqual(miniBatchData.Y[3], (float)trainDataY.Values[3][0]);
            Assert.AreEqual(miniBatchData.Y[99], (float)trainDataY.Values[99][0]);
        }


        [TestMethod]
        public void Test_orchestrator_netxbatches_for_validation()
        {
            const int PREDICT_DAYS = 20;
            const int RANGE = 10;
            const int COLUMN_TO_PREDICT = 2;

            var orchestrator = new LSTMOrchestrator(null, null, null, null, 100);
            var ds = GetMovingEnhancedAverageDataset(PREDICT_DAYS, COLUMN_TO_PREDICT);

            orchestrator.LoadAndPrepareDataSet(ds, PREDICT_DAYS, RANGE);

            var validDataY = ds.GetValidArrayY();
            ds.Normalize(ref validDataY, COLUMN_TO_PREDICT);

            var miniBatchData = orchestrator.GetBatchesForValidation().First();
            //mi aspetto lunghezza di 5 per i dati di validation
            Assert.AreEqual(miniBatchData.Y.Length, 5);

            Assert.AreEqual(miniBatchData.Y.Length, validDataY.Values.Length);

            //verifico che i valori siano gli stessi fra il minibatch e il validDataY                
            Assert.AreEqual(miniBatchData.Y[0], (float)validDataY.Values[0][0]);
            Assert.AreEqual(miniBatchData.Y[3], (float)validDataY.Values[3][0]);
            Assert.AreEqual(miniBatchData.Y[4], (float)validDataY.Values[4][0]);
        }
    }
}
