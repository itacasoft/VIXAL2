using NeuralNetwork.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using VIXAL2.Data.Base;
using ZedGraph;

namespace VIXAL2.GUI
{
    public partial class VIXAL2Form : Form
    {
        private bool LoadDataset(int stockIndex)
        {
            InitiGraphs();

            orchestrator = new LSTMOrchestrator(OnReiterate, OnTrainingProgress, OnTrainingEnded, OnSimulationEnded, Convert.ToInt32(textBoxBatchSize.Text));
            try
            {
                orchestrator.LoadAndPrepareDataSet("..\\..\\..\\Data\\FullDataSet.csv", stockIndex, 1, (DataSetType)(comboBoxType.SelectedIndex + 1), Convert.ToInt32(textBoxPredictDays.Text), Convert.ToInt32(textBoxRange.Text));
            }
            catch (Exception ex)
            {
                if (ex is IndexOutOfRangeException)
                {
                    return false;
                }
            }

            LoadListView(orchestrator.DataSet);
            //disegno il grafico dei prezzi reali
            LoadOriginalLine(orchestrator.DataSet);

            LoadGraphs(orchestrator.DataSet);

            LoadBars(orchestrator.DataSet);

            buttonStart.Enabled = true;
            label9.Text = "Dataset: " + orchestrator.DataSet.DataList[0].Length + " cols, " + orchestrator.DataSet.DataList.Count + " rows";
            return true;
        }

        private void StartTraining()
        {
            int iterations = int.Parse(textBoxIterations.Text);
            int hiddenLayersDim = Convert.ToInt32(textBoxHidden.Text);
            int cellsNumber = Convert.ToInt32(textBoxCells.Text);
            progressBar1.Maximum = iterations;
            progressBar1.Value = 1;

            zedGraphControl3.GraphPane.Title.Text += "Performances";

            orchestrator.StartTraining(iterations, hiddenLayersDim, cellsNumber, checkBoxForwardIterate.Checked);
        }


        void OnTrainingProgress(int iteration)
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            DoTrainingProgress(iteration);
                        }
                    }
                    ));
            }
            else
            {
                DoTrainingProgress(iteration);
            }
        }

        /// <summary>
        /// Updates the graphs for each iteration 
        /// </summary>
        /// <param name="iteration"></param>
        private void DoTrainingProgress(int iteration)
        {
            //output training process
            label3.Text = "Current iteration: " + iteration.ToString();
            label11.Text = "Loss value: " + orchestrator.GetPreviousLossAverage().ToString();
            progressBar1.Value = iteration;

            lossDataLine.AddPoint(new PointPair(iteration, orchestrator.GetPreviousLossAverage()));
            zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);
        }

        /// <summary>
        /// Utilizza il modello dopo il training, per calcolare i vaori anche per validazione
        /// e test
        /// </summary>
        private void UseTrainedModel()
        {
            //disegno il modello calcolato dallo stesso primo valore del trainingLine
            //int sample = orchestrator.DataSet.PredictDays + 1;
            int sampleIndex = 1 + orchestrator.DataSet.PredictDays + orchestrator.DataSet.DelayDays;

            predictedLine.Clear();
            lossDataLine.Clear();

            var ListE = currentModelTrain(ref sampleIndex);
            var listV = currentModelValidation(ref sampleIndex);
            var listT = currentModelTest(ref sampleIndex);
            var listEx = currentModelTestExtreme(ref sampleIndex);

            var allLists = ListE.Concat(listV).Concat(listT).Concat(listEx).ToList();

            if (orchestrator.DataSet is Data.MovingAverageDataSet)
                LoadPredictedLine2(orchestrator.DataSet as Data.MovingAverageDataSet, allLists);

            zedGraphControl1.Refresh();
        }

        void OnTrainingEnded()
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            DoTrainingEnded();
                        }
                    }
                    ));
            }
            else
            {
                DoTrainingEnded();
            }
        }

        void OnSimulationEnded()
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            DoSimulationEnded();
                        }
                    }
                    ));
            }
            else
            {
                DoSimulationEnded();
            }
        }

        /// <summary>
        /// Updates graphs and performs actions at the end of all series of simulatios for a stock 
        /// </summary>
        private void DoSimulationEnded()
        {
            var trades = SimulateFinTrades(false);
            var tradesWithCommissions = SimulateFinTrades(true);
            LoadTrades(tradesWithCommissions);

            ReportItem item = ReportItemAdd();
            EnrichReportItemWithTradesData(item, trades);
            EnrichReportItemWithTradesDataWithCommissions(item, tradesWithCommissions);

            //se sto iterando su tutti gli stock
            if (checkBoxIterateOnStocks.Checked)
            {
                int currentIndex = Convert.ToInt32(textBoxYIndex.Text);
                textBoxYIndex.Text = (currentIndex + 1).ToString();
                if (LoadDataset(currentIndex + 1))
                {
                    StartTraining();
                }
                else
                {
                    //genera il report quando ha finito
                    PrintReport(); 
                }
            }
            else
            {
                //genera il report
                PrintReport(); 
            }
        }

        private List<FinTrade> SimulateFinTrades(bool applyCommissions)
        {
            var trades = orchestrator.SimulateFinTrades(applyCommissions);

            var serializer = new XmlSerializer(typeof(List<FinTrade>));
            string reportFolder = ConfigurationManager.AppSettings["ReportFolder"];
            string filename = Path.Combine(reportFolder, orchestrator.PredictedData.StockName + "_trades_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml");

            using (var writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, trades);
            }

            return trades;
        }

        private List<DoubleDatedValue> currentModelTestExtreme(ref int sampleIndex)
        {
            var predictedList = orchestrator.CurrentModelTestExtreme();

            foreach (var predicted in predictedList)
            {
                var p = new PointPair(sampleIndex, predicted.Value);
                p.Tag = "(EXTTEST - prediction for " + predicted.Date.ToShortDateString() + ": " + predicted.Value + " )";
                predictedLine.AddPoint(p);
                sampleIndex++;
            }

            return predictedList;
        }

        private List<DoubleDatedValue> currentModelValidation(ref int sampleIndex)
        {
            var predictedList = orchestrator.CurrentModelValidation();
            if (predictedList.Count == 0) return new List<DoubleDatedValue>();

            foreach (var predicted in predictedList)
            {
                var p = new PointPair(sampleIndex, predicted.Value);
                p.Tag = "(PREDICTEDY - on " + predicted.PredictionDate.ToShortDateString() + " (value of " + predicted.Date.ToShortDateString() + "): " + predicted.Value + " )";
                predictedLine.AddPoint(p);
                sampleIndex++;
            }

            zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);
            return predictedList;
        }


        private List<DoubleDatedValue> currentModelTest(ref int sampleIndex)
        {
            var predictedList = orchestrator.CurrentModelTest();

            foreach (var predicted in predictedList)
            {
                var p = new PointPair(sampleIndex, predicted.Value);
                p.Tag = "(PREDICTEDY - on " + predicted.PredictionDate.ToShortDateString() + " (value of " + predicted.Date.ToShortDateString() + "): " + predicted.Value + " )";
                predictedLine.AddPoint(p);
                sampleIndex++;
            }

            orchestrator.ComputePerformances(predictedList);

            //DrawPerfomances(orchestrator.SlopePerformances, orchestrator.DiffPerformance);

            var tradeResult = orchestrator.SimulateTrades(predictedList, MONEY, COMMISSION);
            DrawTrades(tradeResult);
            return predictedList;
        }

        /// <summary>
        /// Updates graphs and performs actions at the end of one serie of iteration for a stock 
        /// </summary>
        private void DoTrainingEnded()
        {
            UseTrainedModel();

            Performance[] performances = orchestrator.SlopePerformances;
            PerformanceDiff[] diffPerformances = orchestrator.DiffPerformance;

            slopePerformanceDataLine.Clear();
            for (int i = 1; i < performances.Count(); i++)
            {
                var p = new PointPair(i, performances[i].SuccessPercentage);
                p.Tag = performances[i].ToString();
                slopePerformanceDataLine.AddPoint(p);
            }

            diffPerformanceDataLine.Clear();
            for (int i = 0; i < diffPerformances.Count(); i++)
            {
                var p = new PointPair(i, diffPerformances[i].SuccessPercentage);
                p.Tag = diffPerformances[i].ToString();
                diffPerformanceDataLine.AddPoint(p);
            }

            zedGraphControl3.GraphPane.Title.Text = "Performance: SlopeDiff(%) = " + orchestrator.AvgSlopePerformance.ToString("P") + "; Diff(%) = " + orchestrator.AvgDiffPerformance.ToString("P");
            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);

            lblPerformance1.Text = "SlopePerformance (first): " + orchestrator.SlopePerformances[1].ToString();
            lblPerformance2.Text = "DiffPerformance (first): " + orchestrator.DiffPerformance[0].ToString();
        }

        private void DrawTrades(List<Trade> trades)
        {
            int longCount = 0, shortCount = 0, longSuccessCount = 0, shortSuccessCount = 0;
            double avgGainLong = 0, avgGainShort = 0;

            for (int i = 0; i < trades.Count; i++)
            {
                if (trades[i].PredictedTrend == 1)
                {
                    avgGainLong += trades[i].Gain;
                    longCount++;
                    if (trades[i].Gain > 0)
                        longSuccessCount++;
                }
            }
            avgGainLong = avgGainLong / longCount;
            double gainLongPerc = avgGainLong / MONEY;

            for (int i = 0; i < trades.Count; i++)
            {
                if (trades[i].PredictedTrend == -1)
                {
                    avgGainShort += trades[i].Gain;
                    shortCount++;
                    if (trades[i].Gain > 0)
                        shortSuccessCount++;
                }
            }
            avgGainShort = avgGainShort / shortCount;
            double gainShortPerc = avgGainShort / MONEY;

            for (int i = 0; i < trades.Count; i++)
            {
                if (trades[i].PredictedTrend == 1)
                {
                    var p1 = longTradesLine[0];
                    p1.Tag = "Long Trade START (" + trades[i].StartMoney.ToString("F") + ")";
                    var p2 = longTradesLine[1];
                    p2.Tag = "Long (success =" + longSuccessCount + "/" + longCount + "; gain % = " + gainLongPerc.ToString("F") + ")";
                    p2.Y = (avgGainLong + MONEY) / (MONEY * 2.0D);
                }
                else
                {
                    var p1 = shortTradesLine[0];
                    p1.Tag = "Short Trade START (" + trades[i].StartMoney.ToString("F") + ")";
                    var p2 = shortTradesLine[1];
                    p2.Tag = "Short (success =" + shortSuccessCount + "/" + shortCount + "; gain % = " + gainShortPerc.ToString("F") + ")";
                    p2.Y = (avgGainShort + MONEY) / (MONEY * 2.0D);
                }
            }

            //calcolo il numero delle volte in cui c'è stato un guadagno
            double totalGains = 0;
            for (int i = 0; i < trades.Count; i++)
            {
                if (trades[i].Gain > 0) totalGains++;
            }
            double totalGainPerc = totalGains / trades.Count;

            if (zedGraphControl3.GraphPane.GraphObjList.Count > 0)
                zedGraphControl3.GraphPane.GraphObjList.Clear();

            //aggiungo il testo
            var pp1 = shortTradesLine[1];
            var pp2 = longTradesLine[1];
            double spaziaturaGrafica;
            if (pp1.Y >= pp2.Y)
                spaziaturaGrafica = 0.02;
            else spaziaturaGrafica = -0.02;

            TextObj text = new TextObj((string)pp1.Tag, pp1.X, pp1.Y + spaziaturaGrafica);
            text.FontSpec.FontColor = Color.Black;
            text.Location.AlignH = AlignH.Left;
            text.Location.AlignV = AlignV.Center;
            text.FontSpec.Fill.IsVisible = false;
            text.FontSpec.Border.IsVisible = false;
            zedGraphControl3.GraphPane.GraphObjList.Add(text);

            text = new TextObj((string)pp2.Tag, pp2.X, pp2.Y - spaziaturaGrafica);
            text.FontSpec.FontColor = Color.Black;
            text.Location.AlignH = AlignH.Left;
            text.Location.AlignV = AlignV.Center;
            text.FontSpec.Fill.IsVisible = false;
            text.FontSpec.Border.IsVisible = false;
            zedGraphControl3.GraphPane.GraphObjList.Add(text);

            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);
        }


        private List<DoubleDatedValue> currentModelTrain(ref int sampleIndex)
        {
            var list = orchestrator.CurrentModelTrain();
            foreach (var y in list)
            {
                predictedLine.AddPoint(new PointPair(sampleIndex, y.Value));
                sampleIndex++;
            }
            zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);

            return list;
        }
    }
}
