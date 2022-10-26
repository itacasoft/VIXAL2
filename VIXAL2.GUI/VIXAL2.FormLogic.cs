using NeuralNetwork.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VIXAL2.Data.Base;
using ZedGraph;

namespace VIXAL2.GUI
{
    public partial class VIXAL2Form : Form
    {
        private void LoadDataset(int stockIndex)
        {
            InitiGraphs();

            orchestrator = new LSTMOrchestrator(DrawTestSeparationLine, progressReport, EndReport, FinalEndReport, Convert.ToInt32(textBoxBatchSize.Text));
            orchestrator.LoadAndPrepareDataSet("..\\..\\..\\Data\\FullDataSet.csv", stockIndex, 1, comboBox1.SelectedIndex + 1, Convert.ToInt32(textBoxPredictDays.Text), Convert.ToInt32(textBoxRange.Text));

            LoadListView(orchestrator.DataSet);
            //disegno il grafico dei prezzi reali
            LoadOriginalLine(orchestrator.DataSet);

            LoadGraphs(orchestrator.DataSet);

            LoadBars(orchestrator.DataSet);

            buttonStart.Enabled = true;
            label9.Text = "Dataset: " + orchestrator.DataSet.DataList[0].Length + " cols, " + orchestrator.DataSet.DataList.Count + " rows";
        }

        private void StartTraining()
        {
            int iterations = int.Parse(textBoxIterations.Text);
            int hiddenLayersDim = Convert.ToInt32(textBoxHidden.Text);
            int cellsNumber = Convert.ToInt32(textBoxCells.Text);
            progressBar1.Maximum = iterations;
            progressBar1.Value = 1;

            zedGraphControl3.GraphPane.Title.Text += "Performances";

            orchestrator.StartTraining(iterations, hiddenLayersDim, cellsNumber, checkBox1.Checked);
        }


        void progressReport(int iteration)
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            //output training process
                            label3.Text = "Current iteration: " + iteration.ToString();
                            label11.Text = "Loss value: " + orchestrator.GetPreviousLossAverage().ToString("F2");
                            progressBar1.Value = iteration;

                            reportOnGraphs(iteration);
                        }
                    }
                    ));
            }
            else
            {
                //output training process
                label3.Text = "Current iteration: " + iteration.ToString();
                label11.Text = "Loss value: " + orchestrator.GetPreviousLossAverage().ToString();
                progressBar1.Value = iteration;

                reportOnGraphs(iteration);
            }
        }


        void EndReport(int iteration)
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            DrawPerfomances(orchestrator.SlopePerformances, orchestrator.DiffPerformance);
                        }
                    }
                    ));
            }
            else
            {
                DrawPerfomances(orchestrator.SlopePerformances, orchestrator.DiffPerformance);
            }
        }

        void FinalEndReport()
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            CheckReload();
                        }
                    }
                    ));
            }
            else
            {
                CheckReload();
            }
        }

        private void CheckReload()
        {
            if (checkBoxIterateOnStocks.Checked)
            {
                int currentIndex = Convert.ToInt32(textBoxYIndex.Text);
                textBoxYIndex.Text = (currentIndex + 1).ToString();
                LoadDataset(currentIndex + 1);
                StartTraining();
            }
            else
            {

            }
        }

        private void currentModelTestExtreme(ref int sample)
        {
            var predictedList = orchestrator.CurrentModelTestExtreme();

            foreach (var predicted in predictedList)
            {
                var p = new PointPair(sample, predicted.Value);
                p.Tag = "(EXTTEST - prediction for " + predicted.Date.ToShortDateString() + ": " + predicted.Value + " )";
                modelLine.AddPoint(p);
                sample++;
            }
        }

        private void currentModelTest(int iteration, ref int sample)
        {
            var predictedList = orchestrator.CurrentModelTest();

            foreach (var predicted in predictedList)
            {
                var p = new PointPair(sample, predicted.Value);
                p.Tag = "(PREDICTEDY - on " + predicted.PredictionDate.ToShortDateString() + " (value of " + predicted.Date.ToShortDateString() + "): " + predicted.Value + " )";
                modelLine.AddPoint(p);
                sample++;
            }

            orchestrator.ComparePredictedAgainstDataY(predictedList, 0);
            SetDatesOnPerformances(ref orchestrator.SlopePerformances);
            SetDatesOnPerformances(ref orchestrator.DiffPerformance);

            //DrawPerfomances(orchestrator.SlopePerformances, orchestrator.DiffPerformance);

            var tradeResult = orchestrator.SimulateTrades(predictedList, MONEY, COMMISSION);
            DrawTrades(tradeResult);
        }

        private void SetDatesOnPerformances(ref List<Performance> performances)
        {
            var dad = orchestrator.DataSet.GetExtendedArrayX(false);
            for (int i = 0; (i < dad.Length && i < performances.Count); i++)
            {
                performances[i].Date = dad.GetFutureDate(i);
            }
        }

        private void SetDatesOnPerformances(ref List<PerformanceDiff> performances)
        {
            var dad = orchestrator.DataSet.GetExtendedArrayX(false);
            for (int i = 0; (i < dad.Length && i < performances.Count); i++)
            {
                performances[i].Date = dad.GetFutureDate(i);
            }
        }


        private void DrawPerfomances(List<Performance> performances, List<PerformanceDiff> diffPerformances)
        {
            slopePerformanceDataLine.Clear();
            for (int i = 1; i < performances.Count; i++)
            {
                var p = new PointPair(i, performances[i].SuccessPercentage);
                p.Tag = performances[i].ToString();
                slopePerformanceDataLine.AddPoint(p);
            }

            diffPerformanceDataLine.Clear();
            for (int i = 0; i < diffPerformances.Count; i++)
            {
                var p = new PointPair(i, diffPerformances[i].SuccessPercentage);
                p.Tag = diffPerformances[i].ToString();
                diffPerformanceDataLine.AddPoint(p);
            }

            double avgSlopePerformance = 0;
            double avgDiffPerformance = 0;
            //calcolo la media dei primi DAYS_FOR_PERFORMANCE
            for (int i = 1; i <= orchestrator.DAYS_FOR_PERFORMANCE; i++)
            {
                avgSlopePerformance += orchestrator.SlopePerformances[i].FailedPercentage;
            }
            avgSlopePerformance = avgSlopePerformance / orchestrator.DAYS_FOR_PERFORMANCE;

            for (int i = 0; i < orchestrator.DAYS_FOR_PERFORMANCE; i++)
            {
                avgDiffPerformance += orchestrator.DiffPerformance[i].FailedPercentage;
            }
            avgDiffPerformance = avgDiffPerformance / orchestrator.DAYS_FOR_PERFORMANCE;

            zedGraphControl3.GraphPane.Title.Text = "Performance: SlopeDiff(%) = " + avgSlopePerformance.ToString("P") + "; Diff(%) = " + avgDiffPerformance.ToString("P");
            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);

            lblPerformance1.Text = "SlopePerformance (first): " + orchestrator.SlopePerformances[1].ToString();
            lblPerformance2.Text = "DiffPerformance (first): " + orchestrator.DiffPerformance[0].ToString();
        }

        private void reportOnGraphs(int iteration)
        {
            if (iteration == Convert.ToInt32(textBoxIterations.Text))
            {
                //disegno il modello calcolato dallo stesso primo valore del trainingLine
                //int sample = orchestrator.DataSet.PredictDays + 1;
                int sample = orchestrator.DataSet.PredictDays + orchestrator.DataSet.Range;

                modelLine.Clear();
                lossDataLine.Clear();

                currentModelEvaluation(iteration, ref sample);
                currentModelTest(iteration, ref sample);
                currentModelTestExtreme(ref sample);

                zedGraphControl1.Refresh();
            }
            else
            {
                lossDataLine.AddPoint(new PointPair(iteration, orchestrator.GetPreviousLossAverage()));
                zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);
            }
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


        private void currentModelEvaluation(int iteration, ref int sample)
        {
            var list = orchestrator.CurrentModelEvaluation();
            foreach (var y in list)
            {
                modelLine.AddPoint(new PointPair(sample, y));
                sample++;
            }
            zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);
        }


    }
}
