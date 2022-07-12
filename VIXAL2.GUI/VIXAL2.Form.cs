using NeuralNetwork.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Windows.Forms;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using ZedGraph;

namespace VIXAL2.GUI
{
    public partial class VIXAL2Form : Form
    {
        double MONEY = 10000.00;
        double COMMISSION = 0.0019;

        LSTMOrchestrator orchestrator;

        LineItem modelLine;
        LineItem trainingDataLine;
        LineItem separationline;
        LineItem realLine;
        LineItem lossDataLine;
        LineItem performanceDataLine;
        LineItem longTradesLine, shortTradesLine;

        public VIXAL2Form()
        {
            InitializeComponent();
        }

        private void InitiGraphs()
        {
            label2.Tag = null;
            label2.Text = "Performance (first): ";

            ///Fitness simulation chart
            zedGraphControl1.GraphPane.Title.Text = "Model evaluation";
            zedGraphControl1.GraphPane.XAxis.Title.Text = "Samples";
            zedGraphControl1.GraphPane.YAxis.Title.Text = "Observer/Predicted";

            if (trainingDataLine != null) trainingDataLine.Clear();
            else
                trainingDataLine = new LineItem("Training Data", null, null, Color.Blue, ZedGraph.SymbolType.None, 1);
            trainingDataLine.Symbol.Fill = new Fill(Color.Blue);
            trainingDataLine.Symbol.Size = 1;

            if (modelLine != null) modelLine.Clear();
            else
                modelLine = new LineItem("Prediction Data", null, null, Color.Red, ZedGraph.SymbolType.None, 1);
            modelLine.Symbol.Fill = new Fill(Color.Red);
            modelLine.Symbol.Size = 1;

            if (realLine != null) realLine.Clear();
            else
                realLine = new LineItem("Real Data", null, null, Color.Black, ZedGraph.SymbolType.None, 1);
            realLine.Symbol.Fill = new Fill(Color.Black);
            realLine.Symbol.Size = 1;
            realLine.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;

            zedGraphControl2.GraphPane.XAxis.Title.Text = "Training Loss";
            zedGraphControl2.GraphPane.XAxis.Title.Text = "Iteration";
            zedGraphControl2.GraphPane.YAxis.Title.Text = "Loss value";

            if (lossDataLine != null) lossDataLine.Clear();
            else
                lossDataLine = new LineItem("Loss values", null, null, Color.Red, ZedGraph.SymbolType.Circle, 1);
            lossDataLine.Symbol.Fill = new Fill(Color.Red);
            lossDataLine.Symbol.Size = 5;

            if (performanceDataLine != null) performanceDataLine.Clear();
            else
                performanceDataLine = new LineItem("Performance", null, null, Color.DarkKhaki, ZedGraph.SymbolType.Diamond, 1);
            performanceDataLine.Symbol.Fill = new Fill(Color.DarkKhaki);
            performanceDataLine.Symbol.Size = 5;

            if (longTradesLine != null) longTradesLine.Clear();
            else
                longTradesLine = new LineItem("Long Trades", null, null, Color.ForestGreen, ZedGraph.SymbolType.Circle, 1);
            longTradesLine.Symbol.Fill = new Fill(Color.CadetBlue);
            longTradesLine.Symbol.Size = 5;
            var p1 = new PointPair(1, 0.5);
            var p2 = new PointPair(11, 0.5);
            longTradesLine.AddPoint(p1);
            longTradesLine.AddPoint(p2);

            if (shortTradesLine != null) shortTradesLine.Clear();
            else
                shortTradesLine = new LineItem("Short Trades", null, null, Color.PaleVioletRed, ZedGraph.SymbolType.Circle, 1);
            shortTradesLine.Symbol.Fill = new Fill(Color.RosyBrown);
            shortTradesLine.Symbol.Size = 5;
            var p3 = new PointPair(1, 0.5);
            var p4 = new PointPair(11, 0.5);
            shortTradesLine.AddPoint(p3);
            shortTradesLine.AddPoint(p4);

            //Add line to graph
            this.zedGraphControl1.GraphPane.CurveList.Clear();
            this.zedGraphControl1.GraphPane.CurveList.Add(trainingDataLine);
            this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl1.GraphPane.CurveList.Add(modelLine);
            this.zedGraphControl1.GraphPane.CurveList.Add(realLine);
            this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());

            this.zedGraphControl2.GraphPane.CurveList.Clear();
            this.zedGraphControl2.GraphPane.CurveList.Add(lossDataLine);
            this.zedGraphControl2.GraphPane.AxisChange(this.CreateGraphics());

            zedGraphControl3.GraphPane.CurveList.Clear();
            zedGraphControl3.GraphPane.Title.Text = "Performances";
            zedGraphControl3.GraphPane.XAxis.Title.Text = "Days from start";
            zedGraphControl3.GraphPane.YAxis.Title.Text = "Success Percentage";
            zedGraphControl3.GraphPane.CurveList.Add(performanceDataLine);
            zedGraphControl3.GraphPane.CurveList.Add(shortTradesLine);
            zedGraphControl3.GraphPane.CurveList.Add(longTradesLine);
            zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());
            if (zedGraphControl3.GraphPane.GraphObjList.Count > 0)
                zedGraphControl3.GraphPane.GraphObjList.Clear();

            if (separationline != null) separationline.Clear();
            else separationline = new LineItem("");
            separationline.Symbol.Fill = new Fill(Color.Black);
            separationline.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
            separationline.Symbol.Size = 1;
            zedGraphControl1.GraphPane.CurveList.Add(separationline);

            zedGraphControl1.IsShowPointValues = true;
            zedGraphControl1.PointValueFormat = "0.0000";
            zedGraphControl1.PointDateFormat = "d";

            zedGraphControl3.IsShowPointValues = true;
            zedGraphControl3.PointValueFormat = "0.0000";
            zedGraphControl3.PointDateFormat = "d";
        }

        private void VIXAL2Form_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            buttonStart.Enabled = false;

            MONEY = Convert.ToDouble(ConfigurationManager.AppSettings["MoneyForTradesSimulation"]);
            COMMISSION = Convert.ToDouble(ConfigurationManager.AppSettings["CommisionForTradesSimulation"]);

            InitiGraphs();
        }

        private void LoadOriginalLine(StocksDataset ds)
        {
            //disegno il grafico dei prezzi reali normalizzato
            var sample1 = 1;// + ds.PredictDays;

            TimeSerieArray realData;
            
            if (ds.NormalizeFirst)
                realData = ds.OriginalNormalizedData;
            else
                realData = ds.OriginalData;

            int sampleIndex = 0;

            for (int i = sampleIndex; i < realData.Length; i++)
            {
                var p = new PointPair(sample1, realData.Values[i][Convert.ToInt32(textBoxYIndex.Text)]);
                p.Tag = "[" + sample1.ToString() + "] " + realData.GetDate(i).ToShortDateString() + ": " + realData.Values[i][Convert.ToInt32(textBoxYIndex.Text)] + "";
                realLine.AddPoint(p);
                sample1++;
            }

            realLine.Label.Text = "Close Price (" + realData.GetColName(Convert.ToInt32(textBoxYIndex.Text)) + ")";

            zedGraphControl1.GraphPane.Title.Text = realData.GetColName(Convert.ToInt32(textBoxYIndex.Text)) + " - Model evaluation";
            zedGraphControl1.GraphPane.XAxis.Scale.Min = -20;
            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
        }

        private void LoadGraphs(StocksDataset ds)
        {
            const int COL_TO_DRAW = 0;
            var trainDataY = ds.GetTrainArrayY();

            //disegno il trainingDataY dal giorno PredictDay così mi ritrovo allineato con i
            //prezzi reali (non sono sicuro che vada bene così però)
            int sample = ds.PredictDays + ds.Range;

            for (int i = 0; i < trainDataY.Length; i++)
            {
                var p = new PointPair(sample, trainDataY[i][COL_TO_DRAW]);
                p.Tag = "(TRAINDATAY - " + trainDataY.GetDate(i).ToShortDateString() + " (value of " + trainDataY.GetFutureDate(i).ToShortDateString() + "): " + trainDataY[i][0] + " )";
                trainingDataLine.AddPoint(p);
                sample++;
            }

            var validDataY = ds.GetValidArrayY();

            for (int i = 0; i < validDataY.Length; i++)
            {
                var p = new PointPair(sample, validDataY[i][COL_TO_DRAW]);
                p.Tag = "(VALIDATAY - " + validDataY.GetDate(i).ToShortDateString() + " (value of " + validDataY.GetFutureDate(i).ToShortDateString() + "): " + validDataY[i][0] + " )";
                trainingDataLine.AddPoint(p);
                sample++;
            }

            DrawTestSeparationLine(ds);

            TimeSerieArrayExt testDataY = ds.GetTestArrayY();

            for (int i = 0; i < testDataY.Length; i++)
            {
                var p = new PointPair(sample, testDataY[i][COL_TO_DRAW]);
                p.Tag = "(TESTDATAY - " + testDataY.GetDate(i).ToShortDateString() + " (value of " + testDataY.GetFutureDate(i).ToShortDateString() + "): " + testDataY[i][0] + " )";
                trainingDataLine.AddPoint(p);
                sample++;
            }

            trainingDataLine.Label.Text = "Training (" + testDataY.ToStringExt() + ", R:" + textBoxRange.Text + "" +  ")";
            modelLine.Label.Text = "Model/Prediction (" + testDataY.ToStringExt() + ", R:" + textBoxRange.Text + ")";

            zedGraphControl1.GraphPane.Title.Text = testDataY.GetColName(0) + " - (I:" + textBoxIterations.Text + ", Hidden:" + textBoxHidden.Text + ", Cells:" + textBoxCells.Text + ")";
            zedGraphControl1.GraphPane.XAxis.Scale.Min = -2;
            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
        }

        private void DrawTestSeparationLine(StocksDataset ds)
        {
            int sample = ds.TrainCount + ds.ValidCount + ds.PredictDays + ds.Range;
            TimeSerieArray testDataY = ds.GetTestArrayY();

            separationline.Clear();
            var p1 = new PointPair(sample, ds.MinYValue);
            p1.Tag = "( " + testDataY.GetDate(0).ToShortDateString() + " )";
            separationline.AddPoint(p1);
            p1 = new PointPair(sample, ds.MaxYValue);
            p1.Tag = "( " + testDataY.GetDate(0).ToShortDateString() + " )";
            separationline.AddPoint(p1);
        }

        private void loadListView(StocksDataset ds)
        {
            //clear the list first
            listView1.Clear();
            listView1.GridLines = true;
            listView1.HideSelection = false;
            if (ds.TrainDataX == null || ds.TrainDataY == null)
                return;

            //add features
            listView1.Columns.Add(new ColumnHeader() { Width = 20, Text = "Sample" }); 
            for (int i = 0; i < ds.ColNames.Length; i++)
            {
                var col1 = new ColumnHeader();
                col1.Text = ds.ColNames[i];
                col1.Width = 70;
                listView1.Columns.Add(col1);
            }

            //Add labels
            for (int i = 0; i < ds.TrainDataY[0].Length; i++)
            {
                var col = new ColumnHeader();
                col.Text = $"Y" + (i + 1);
                col.Width = 70;
                listView1.Columns.Add(col);
            }

            int mydateIndex = 0;

            for (int i = 0; i < ds.TrainDataX.Length; i++, mydateIndex++)
            {
                var itm = listView1.Items.Add((mydateIndex+1) + " [" + ds.Dates[mydateIndex].ToShortDateString() + "]");
                for (int j = 0; j < ds.TrainDataX[i].Length; j++)
                    itm.SubItems.Add(ds.TrainDataX[i][j].ToString());

                for (int j = 0; j < ds.TrainDataY[i].Length; j++)
                    itm.SubItems.Add(ds.TrainDataY[i][j].ToString());
            }

            for (int i = 0; i < ds.ValidDataX.Length; i++, mydateIndex++)
            {
                var itm = listView1.Items.Add((mydateIndex + 1) + " [" + ds.Dates[mydateIndex].ToShortDateString() + "]");
                itm.BackColor = Color.Yellow;
                for (int j = 0; j < ds.ValidDataX[i].Length; j++)
                    itm.SubItems.Add(ds.ValidDataX[i][j].ToString());

                for (int j = 0; j < ds.ValidDataY[i].Length; j++)
                    itm.SubItems.Add(ds.ValidDataY[i][j].ToString());
            }

            for (int i = 0; i < ds.TestDataX.Length; i++, mydateIndex++)
            {
                var itm = listView1.Items.Add((mydateIndex + 1) + " [" + ds.Dates[mydateIndex].ToShortDateString() + "]");
                itm.BackColor = Color.LightCoral;
                for (int j = 0; j < ds.TestDataX[i].Length; j++)
                    itm.SubItems.Add(ds.TestDataX[i][j].ToString());

                for (int j = 0; j < ds.TestDataY[i].Length; j++)
                    itm.SubItems.Add(ds.TestDataY[i][j].ToString());
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
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


        void endReport(int iteration)
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        if (!this.IsDisposed)
                        {
                            DrawPerfomances(orchestrator.Performances);
                        }
                    }
                    ));
            }
            else
            {
                DrawPerfomances(orchestrator.Performances);
            }
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

            var performances = orchestrator.ComparePredictedAgainstDataY(DoubleDatedValue.ToDoubleArray(predictedList), 0);
            SetDatesOnPerformances(ref performances);
            label2.Text = "Performance (first): " + performances[1].ToString();
            DrawPerfomances(performances);

            var tradeResult = orchestrator.SimulateTrades(predictedList, MONEY, COMMISSION);
            DrawTrades(tradeResult);
        }


        private void DrawTrades(List<Trade> trades)
        {
            int longCount = 0, shortCount = 0, longSuccessCount = 0, shortSuccessCount = 0;
            double avgGainLong = 0, avgGainShort = 0;

            for (int i=0; i<trades.Count; i++)
            {
                if (trades[i].PredictedTrend == 1)
                {
                    avgGainLong += trades[i].Gain;
                    longCount++;
                    if (trades[i].Gain > 0)
                        longSuccessCount++;
                }
            }
            avgGainLong = avgGainLong/longCount;
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
            for (int i=0; i<trades.Count; i++)
            {
                if (trades[i].Gain > 0) totalGains++;
            }
            double totalGainPerc = totalGains/trades.Count;
            
            zedGraphControl3.GraphPane.Title.Text = "Performance - Trade Success % = " + totalGainPerc.ToString("F");

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


        private void SetDatesOnPerformances(ref List<Performance> performances)
        {
            var dad = orchestrator.DataSet.GetExtendedArrayX(false);
            for (int i = 0; (i < dad.Length && i < performances.Count); i++)
            {
                performances[i].Date = dad.GetFutureDate(i);
            }
        }

        private void DrawPerfomances(List<Performance> performances)
        {
            performanceDataLine.Clear();
            for (int i=1; i< performances.Count; i++)
            {
                var p = new PointPair(i, performances[i].SuccessPercentage);
                p.Tag = performances[i].ToString();
                performanceDataLine.AddPoint(p);
            }
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


        private void btnLoad_Click(object sender, EventArgs e)
        {
            InitiGraphs();

            orchestrator = new LSTMOrchestrator(DrawTestSeparationLine, progressReport, endReport, Convert.ToInt32(textBoxBatchSize.Text));
            orchestrator.LoadAndPrepareDataSet("..\\..\\..\\Data\\FullDataSet.csv", Convert.ToInt32(textBoxYIndex.Text), 1, comboBox1.SelectedIndex + 1, Convert.ToInt32(textBoxPredictDays.Text), Convert.ToInt32(textBoxRange.Text));

            loadListView(orchestrator.DataSet);
            //disegno il grafico dei prezzi reali
            LoadOriginalLine(orchestrator.DataSet);

            LoadGraphs(orchestrator.DataSet);

            buttonStart.Enabled = true;
            label9.Text = "Dataset: " + orchestrator.DataSet.DataList[0].Length + " cols, " + orchestrator.DataSet.DataList.Count + " rows";
            //            textBoxCells.Text = DataSet.AllData[0].Length.ToString();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            orchestrator.StopTrainingNow();
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            textBoxYIndex.Text = (e.Column - 1).ToString();
        }
    }
}
