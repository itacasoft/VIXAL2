using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeuralNetwork.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.UnitTest
{
    [TestClass]
    public class TestPerformanceAvg
    {
        [TestMethod]
        public void CalculateWeightedSlopePerformance_perfect()
        {
            var orchestrator = new LSTMOrchestrator(null, null, null, null, 10);
            Assert.AreEqual(orchestrator.SlopePerformances.Length, 15);

            orchestrator.SlopePerformances[0].Date = DateTime.Today;
            orchestrator.SlopePerformances[1].Date = DateTime.Today.AddDays(1);
            orchestrator.SlopePerformances[2].Date = DateTime.Today.AddDays(2);
            orchestrator.SlopePerformances[3].Date = DateTime.Today.AddDays(3);
            orchestrator.SlopePerformances[4].Date = DateTime.Today.AddDays(4);
            orchestrator.SlopePerformances[5].Date = DateTime.Today.AddDays(5);
            orchestrator.SlopePerformances[6].Date = DateTime.Today.AddDays(6);
            orchestrator.SlopePerformances[7].Date = DateTime.Today.AddDays(7);
            orchestrator.SlopePerformances[8].Date = DateTime.Today.AddDays(8);
            orchestrator.SlopePerformances[9].Date = DateTime.Today.AddDays(9);
            orchestrator.SlopePerformances[10].Date = DateTime.Today.AddDays(10);
            orchestrator.SlopePerformances[11].Date = DateTime.Today.AddDays(11);
            orchestrator.SlopePerformances[12].Date = DateTime.Today.AddDays(12);
            orchestrator.SlopePerformances[13].Date = DateTime.Today.AddDays(13);
            orchestrator.SlopePerformances[14].Date = DateTime.Today.AddDays(14);

            //curva piatta orizzontale perfetta
            orchestrator.SlopePerformances[14].Failed = 0;
            orchestrator.SlopePerformances[14].Guessed = 1;
            orchestrator.SlopePerformances[13].Failed = 0;
            orchestrator.SlopePerformances[13].Guessed = 1;
            orchestrator.SlopePerformances[12].Failed = 0;
            orchestrator.SlopePerformances[12].Guessed = 1;
            orchestrator.SlopePerformances[11].Failed = 0;
            orchestrator.SlopePerformances[11].Guessed = 1;
            orchestrator.SlopePerformances[10].Failed = 0;
            orchestrator.SlopePerformances[10].Guessed = 1;
            orchestrator.SlopePerformances[9].Failed = 0;
            orchestrator.SlopePerformances[9].Guessed = 1;
            orchestrator.SlopePerformances[8].Failed = 0;
            orchestrator.SlopePerformances[8].Guessed = 1;
            orchestrator.SlopePerformances[7].Failed = 0;
            orchestrator.SlopePerformances[7].Guessed = 1;
            orchestrator.SlopePerformances[6].Failed = 0;
            orchestrator.SlopePerformances[6].Guessed = 1;
            orchestrator.SlopePerformances[5].Failed = 0;
            orchestrator.SlopePerformances[5].Guessed = 1;
            orchestrator.SlopePerformances[4].Failed = 0;
            orchestrator.SlopePerformances[4].Guessed = 1;
            orchestrator.SlopePerformances[3].Failed = 0;
            orchestrator.SlopePerformances[3].Guessed = 1;
            orchestrator.SlopePerformances[2].Failed = 0;
            orchestrator.SlopePerformances[2].Guessed = 1;
            orchestrator.SlopePerformances[1].Failed = 0;
            orchestrator.SlopePerformances[1].Guessed = 1;

            double avg1 = Math.Round(orchestrator.AvgSlopePerformance, 2);
            double w1 = Math.Round(orchestrator.WeightedSlopePerformance, 2);

            //ideal
            Assert.AreEqual(avg1, 0.0);
            Assert.AreEqual(w1, 0.0);
        }

        [TestMethod]
        public void CalculateWeightedSlopePerformance_Normal()
        {
            var orchestrator = new LSTMOrchestrator(null, null, null, null, 10);
            Assert.AreEqual(orchestrator.SlopePerformances.Length, 15);

            orchestrator.SlopePerformances[0].Date = DateTime.Today;
            orchestrator.SlopePerformances[1].Date = DateTime.Today.AddDays(1);
            orchestrator.SlopePerformances[2].Date = DateTime.Today.AddDays(2);
            orchestrator.SlopePerformances[3].Date = DateTime.Today.AddDays(3);
            orchestrator.SlopePerformances[4].Date = DateTime.Today.AddDays(4);
            orchestrator.SlopePerformances[5].Date = DateTime.Today.AddDays(5);
            orchestrator.SlopePerformances[6].Date = DateTime.Today.AddDays(6);
            orchestrator.SlopePerformances[7].Date = DateTime.Today.AddDays(7);
            orchestrator.SlopePerformances[8].Date = DateTime.Today.AddDays(8);
            orchestrator.SlopePerformances[9].Date = DateTime.Today.AddDays(9);
            orchestrator.SlopePerformances[10].Date = DateTime.Today.AddDays(10);
            orchestrator.SlopePerformances[11].Date = DateTime.Today.AddDays(11);
            orchestrator.SlopePerformances[12].Date = DateTime.Today.AddDays(12);
            orchestrator.SlopePerformances[13].Date = DateTime.Today.AddDays(13);
            orchestrator.SlopePerformances[14].Date = DateTime.Today.AddDays(14);

            //curva normale discendente
            orchestrator.SlopePerformances[1].Failed = 20;
            orchestrator.SlopePerformances[1].Guessed = 80;
            orchestrator.SlopePerformances[2].Failed = 21;
            orchestrator.SlopePerformances[2].Guessed = 79;
            orchestrator.SlopePerformances[3].Failed = 22;
            orchestrator.SlopePerformances[3].Guessed = 78;
            orchestrator.SlopePerformances[4].Failed = 23;
            orchestrator.SlopePerformances[4].Guessed = 77;
            orchestrator.SlopePerformances[5].Failed = 24;
            orchestrator.SlopePerformances[5].Guessed = 76;
            orchestrator.SlopePerformances[6].Failed = 25;
            orchestrator.SlopePerformances[6].Guessed = 75;
            orchestrator.SlopePerformances[7].Failed = 26;
            orchestrator.SlopePerformances[7].Guessed = 74;
            orchestrator.SlopePerformances[8].Failed = 27;
            orchestrator.SlopePerformances[8].Guessed = 73;
            orchestrator.SlopePerformances[9].Failed = 28;
            orchestrator.SlopePerformances[9].Guessed = 72;
            orchestrator.SlopePerformances[10].Failed = 29;
            orchestrator.SlopePerformances[10].Guessed = 71;
            orchestrator.SlopePerformances[11].Failed = 30;
            orchestrator.SlopePerformances[11].Guessed = 70;
            orchestrator.SlopePerformances[12].Failed = 31;
            orchestrator.SlopePerformances[12].Guessed = 69;
            orchestrator.SlopePerformances[13].Failed = 32;
            orchestrator.SlopePerformances[13].Guessed = 68;
            orchestrator.SlopePerformances[14].Failed = 33;
            orchestrator.SlopePerformances[14].Guessed = 67;

            double avg1 = Math.Round(orchestrator.AvgSlopePerformance, 2);
            double w1 = Math.Round(orchestrator.WeightedSlopePerformance, 2);

            Assert.AreEqual(avg1, 0.27);
            //good
            Assert.IsTrue(w1<avg1);
        }

        [TestMethod]
        public void CalculateWeightedSlopePerformance_inverse()
        {
            var orchestrator = new LSTMOrchestrator(null, null, null, null, 10);
            Assert.AreEqual(orchestrator.SlopePerformances.Length, 15);

            orchestrator.SlopePerformances[0].Date = DateTime.Today;
            orchestrator.SlopePerformances[1].Date = DateTime.Today.AddDays(1);
            orchestrator.SlopePerformances[2].Date = DateTime.Today.AddDays(2);
            orchestrator.SlopePerformances[3].Date = DateTime.Today.AddDays(3);
            orchestrator.SlopePerformances[4].Date = DateTime.Today.AddDays(4);
            orchestrator.SlopePerformances[5].Date = DateTime.Today.AddDays(5);
            orchestrator.SlopePerformances[6].Date = DateTime.Today.AddDays(6);
            orchestrator.SlopePerformances[7].Date = DateTime.Today.AddDays(7);
            orchestrator.SlopePerformances[8].Date = DateTime.Today.AddDays(8);
            orchestrator.SlopePerformances[9].Date = DateTime.Today.AddDays(9);
            orchestrator.SlopePerformances[10].Date = DateTime.Today.AddDays(10);
            orchestrator.SlopePerformances[11].Date = DateTime.Today.AddDays(11);
            orchestrator.SlopePerformances[12].Date = DateTime.Today.AddDays(12);
            orchestrator.SlopePerformances[13].Date = DateTime.Today.AddDays(13);
            orchestrator.SlopePerformances[14].Date = DateTime.Today.AddDays(14);

            //curva ascendente (bad)
            orchestrator.SlopePerformances[14].Failed = 20;
            orchestrator.SlopePerformances[14].Guessed = 80;
            orchestrator.SlopePerformances[13].Failed = 21;
            orchestrator.SlopePerformances[13].Guessed = 79;
            orchestrator.SlopePerformances[12].Failed = 22;
            orchestrator.SlopePerformances[12].Guessed = 78;
            orchestrator.SlopePerformances[11].Failed = 23;
            orchestrator.SlopePerformances[11].Guessed = 77;
            orchestrator.SlopePerformances[10].Failed = 24;
            orchestrator.SlopePerformances[10].Guessed = 76;
            orchestrator.SlopePerformances[9].Failed = 25;
            orchestrator.SlopePerformances[9].Guessed = 75;
            orchestrator.SlopePerformances[8].Failed = 26;
            orchestrator.SlopePerformances[8].Guessed = 74;
            orchestrator.SlopePerformances[7].Failed = 27;
            orchestrator.SlopePerformances[7].Guessed = 73;
            orchestrator.SlopePerformances[6].Failed = 28;
            orchestrator.SlopePerformances[6].Guessed = 72;
            orchestrator.SlopePerformances[5].Failed = 29;
            orchestrator.SlopePerformances[5].Guessed = 71;
            orchestrator.SlopePerformances[4].Failed = 30;
            orchestrator.SlopePerformances[4].Guessed = 70;
            orchestrator.SlopePerformances[3].Failed = 31;
            orchestrator.SlopePerformances[3].Guessed = 69;
            orchestrator.SlopePerformances[2].Failed = 32;
            orchestrator.SlopePerformances[2].Guessed = 68;
            orchestrator.SlopePerformances[1].Failed = 33;
            orchestrator.SlopePerformances[1].Guessed = 67;

            double avg1 = Math.Round(orchestrator.AvgSlopePerformance, 2);
            double w1 = Math.Round(orchestrator.WeightedSlopePerformance, 2);

            Assert.AreEqual(avg1, 0.27);
            //bad
            Assert.IsTrue(w1>avg1);
        }

        [TestMethod]
        public void CalculateWeightedSlopePerformance_flat()
        {
            var orchestrator = new LSTMOrchestrator(null, null, null, null, 10);
            Assert.AreEqual(orchestrator.SlopePerformances.Length, 15);

            orchestrator.SlopePerformances[0].Date = DateTime.Today;
            orchestrator.SlopePerformances[1].Date = DateTime.Today.AddDays(1);
            orchestrator.SlopePerformances[2].Date = DateTime.Today.AddDays(2);
            orchestrator.SlopePerformances[3].Date = DateTime.Today.AddDays(3);
            orchestrator.SlopePerformances[4].Date = DateTime.Today.AddDays(4);
            orchestrator.SlopePerformances[5].Date = DateTime.Today.AddDays(5);
            orchestrator.SlopePerformances[6].Date = DateTime.Today.AddDays(6);
            orchestrator.SlopePerformances[7].Date = DateTime.Today.AddDays(7);
            orchestrator.SlopePerformances[8].Date = DateTime.Today.AddDays(8);
            orchestrator.SlopePerformances[9].Date = DateTime.Today.AddDays(9);
            orchestrator.SlopePerformances[10].Date = DateTime.Today.AddDays(10);
            orchestrator.SlopePerformances[11].Date = DateTime.Today.AddDays(11);
            orchestrator.SlopePerformances[12].Date = DateTime.Today.AddDays(12);
            orchestrator.SlopePerformances[13].Date = DateTime.Today.AddDays(13);
            orchestrator.SlopePerformances[14].Date = DateTime.Today.AddDays(14);

            //curva piatta orizzontale (not so good)
            orchestrator.SlopePerformances[14].Failed = 27;
            orchestrator.SlopePerformances[14].Guessed = 73;
            orchestrator.SlopePerformances[13].Failed = 27;
            orchestrator.SlopePerformances[13].Guessed = 73;
            orchestrator.SlopePerformances[12].Failed = 27;
            orchestrator.SlopePerformances[12].Guessed = 73;
            orchestrator.SlopePerformances[11].Failed = 27;
            orchestrator.SlopePerformances[11].Guessed = 73;
            orchestrator.SlopePerformances[10].Failed = 27;
            orchestrator.SlopePerformances[10].Guessed = 73;
            orchestrator.SlopePerformances[9].Failed = 27;
            orchestrator.SlopePerformances[9].Guessed = 73;
            orchestrator.SlopePerformances[8].Failed = 27;
            orchestrator.SlopePerformances[8].Guessed = 73;
            orchestrator.SlopePerformances[7].Failed = 27;
            orchestrator.SlopePerformances[7].Guessed = 73;
            orchestrator.SlopePerformances[6].Failed = 27;
            orchestrator.SlopePerformances[6].Guessed = 73;
            orchestrator.SlopePerformances[5].Failed = 27;
            orchestrator.SlopePerformances[5].Guessed = 73;
            orchestrator.SlopePerformances[4].Failed = 27;
            orchestrator.SlopePerformances[4].Guessed = 73;
            orchestrator.SlopePerformances[3].Failed = 27;
            orchestrator.SlopePerformances[3].Guessed = 73;
            orchestrator.SlopePerformances[2].Failed = 27;
            orchestrator.SlopePerformances[2].Guessed = 73;
            orchestrator.SlopePerformances[1].Failed = 27;
            orchestrator.SlopePerformances[1].Guessed = 73;

            double avg1 = Math.Round(orchestrator.AvgSlopePerformance, 2);
            double w1 = Math.Round(orchestrator.WeightedSlopePerformance, 2);

            Assert.AreEqual(avg1, 0.27);
            //not so good
            Assert.IsTrue(w1>avg1);
        }

    }
}
