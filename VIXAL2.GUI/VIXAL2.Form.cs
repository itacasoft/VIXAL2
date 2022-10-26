using NeuralNetwork.Base;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.Linq;
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
        LineItem slopePerformanceDataLine;
        LineItem diffPerformanceDataLine;
        LineItem longTradesLine, shortTradesLine;

        public VIXAL2Form()
        {
            InitializeComponent();
        }

        private void InitiGraphs()
        {
            lblPerformance1.Tag = null;
            lblPerformance1.Text = "Performance Slope (first): ";
            lblPerformance2.Tag = null;
            lblPerformance2.Text = "Performance Diff (first): ";

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

            if (slopePerformanceDataLine != null) slopePerformanceDataLine.Clear();
            else
                slopePerformanceDataLine = new LineItem("SlopePerformance", null, null, Color.DarkKhaki, ZedGraph.SymbolType.Diamond, 1);
            slopePerformanceDataLine.Symbol.Fill = new Fill(Color.DarkKhaki);
            slopePerformanceDataLine.Symbol.Size = 5;

            if (diffPerformanceDataLine != null) diffPerformanceDataLine.Clear();
            else
                diffPerformanceDataLine = new LineItem("DiffPerformance", null, null, Color.DarkOliveGreen, ZedGraph.SymbolType.Diamond, 1);
            diffPerformanceDataLine.Symbol.Fill = new Fill(Color.DarkOliveGreen);
            diffPerformanceDataLine.Symbol.Size = 5;

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
            zedGraphControl3.GraphPane.CurveList.Add(slopePerformanceDataLine);
            zedGraphControl3.GraphPane.CurveList.Add(diffPerformanceDataLine);
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

            MONEY = Convert.ToDouble(ConfigurationManager.AppSettings["MoneyForTradesSimulation"], CultureInfo.InvariantCulture);
            COMMISSION = Convert.ToDouble(ConfigurationManager.AppSettings["CommisionForTradesSimulation"], CultureInfo.InvariantCulture);

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

        private void LoadListView(StocksDataset ds)
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

        private void LoadBars(StocksDataset ds)
        {
            totalBar.Width = totalBar.Parent.Width - totalBar.Left * 2;
            int totalBarLen = totalBar.Width;
            totalBar.Text = "Total dates: " + ds.Dates.Count.ToString();

            trainDataXBar.Width = totalBarLen * ds.TrainDataX.Length / ds.Dates.Count;
            trainDataXBar.Text = "TrainDataX: " + ds.TrainDataX.Length.ToString();
            this.toolTip1.SetToolTip(this.trainDataXBar, trainDataXBar.Text);

            validDataXBar.Width = totalBarLen * ds.ValidDataX.Length / ds.Dates.Count;
            validDataXBar.Left = trainDataXBar.Right;
            validDataXBar.Text = "ValidDataX: " + ds.ValidDataX.Length.ToString();
            this.toolTip1.SetToolTip(this.validDataXBar, validDataXBar.Text);

            testDataXBar.Width = totalBarLen * ds.TestDataX.Length / ds.Dates.Count;
            testDataXBar.Left = validDataXBar.Right;
            testDataXBar.Text = "TestDataX: " + ds.TestDataX.Length.ToString();
            this.toolTip1.SetToolTip(this.testDataXBar, testDataXBar.Text);

            trainDataYBar.Width = totalBarLen * ds.TrainDataY.Length / ds.Dates.Count;
            trainDataYBar.Left = trainDataXBar.Left + ds.PredictDays * totalBarLen / ds.Dates.Count;
            trainDataYBar.Text = "TrainDataY: " + ds.TrainDataY.Length.ToString();
            this.toolTip1.SetToolTip(this.trainDataYBar, trainDataYBar.Text);

            validDataYBar.Width = totalBarLen * ds.ValidDataY.Length / ds.Dates.Count;
            validDataYBar.Left = trainDataYBar.Right;
            validDataYBar.Text = "TrainDataY: " + ds.ValidDataY.Length.ToString();
            this.toolTip1.SetToolTip(this.validDataYBar, validDataYBar.Text);

            testDataYBar.Width = totalBarLen * ds.TestDataY.Length / ds.Dates.Count;
            testDataYBar.Left = validDataYBar.Right;
            testDataYBar.Text = "TestDataY: " + ds.TestDataY.Length.ToString();
            this.toolTip1.SetToolTip(this.testDataYBar, testDataYBar.Text);

        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            StartTraining();
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadDataset(Convert.ToInt32(textBoxYIndex.Text));
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
