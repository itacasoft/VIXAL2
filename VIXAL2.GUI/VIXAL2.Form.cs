using NeuralNetwork.Base;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using ZedGraph;

namespace VIXAL2.GUI
{
    public partial class VIXAL2Form : Form
    {
        int batchSize = 100;

        LSTMOrchestrator orchestrator;

        LineItem modelLine;
        LineItem trainingDataLine;
        LineItem separationline;
        LineItem forwardModellLine;
        LineItem realLine;

        LineItem lossDataLine;
        LineItem performanceDataLine;

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


            if (forwardModellLine != null) forwardModellLine.Clear();
            else forwardModellLine = new LineItem("Forward Prediction", null, null, Color.MediumVioletRed, ZedGraph.SymbolType.Diamond, 2);
            if (orchestrator != null)
                orchestrator.DataSet.ForwardPredicted.Clear();
            forwardModellLine.Symbol.Fill = new Fill(Color.MediumVioletRed);
            forwardModellLine.Symbol.Size = 2;

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


            //Add line to graph
            this.zedGraphControl1.GraphPane.CurveList.Clear();
            this.zedGraphControl1.GraphPane.CurveList.Add(trainingDataLine);
            this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl1.GraphPane.CurveList.Add(modelLine);
            this.zedGraphControl1.GraphPane.CurveList.Add(forwardModellLine);
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
            zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());

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
            InitiGraphs();
        }

        private void loadGraphs(StocksDataset ds)
        {
            //disegno il grafico dei prezzi reale normalizzato
            var sample1 = 1; //-ds.PredictDays
            var realData = ds.OriginalNormalizedData;
            int sampleIndex = 0;

            if (ds.GetType() == typeof(MovingAverageDataSet))
            {
                int range = ((MovingAverageDataSet)ds).Range;
                DateTime mydate = realData.GetNextDate(realData.MinDate, range).Value;
                sampleIndex = realData.DateToSampleIndex(mydate).Value - 1;
            }

            for (int i = sampleIndex; i < realData.Length; i++)
            {
                var p = new PointPair(sample1, realData.Values[i][0]);
                p.Tag = "( " + realData.GetDate(i).ToShortDateString() + ", " + realData.Values[i][0] + " )";
                realLine.AddPoint(p);
                sample1++;
            }


            double[] traindDataYList = Utils.GetVectorFromArray(ds.TrainDataY, 0);
            int sample = ds.PredictDays+1;

            for (int i = 0; i < traindDataYList.Length; i++)
            {
                var p = new PointPair(sample, traindDataYList[i]);
                trainingDataLine.AddPoint(p);
                sample++;
            }

            double[] validDataYList = Utils.GetVectorFromArray(ds.ValidDataY, 0);

            for (int i = 0; i < validDataYList.Length; i++)
            {
                var p = new PointPair(sample, validDataYList[i]);
                trainingDataLine.AddPoint(p);
                sample++;
            }

            DrawTestSeparationLine(ds);

            TimeSerieArray testDataY = ds.GetTestArrayY();

            for (int i = 0; i < testDataY.Length; i++)
            {
                var p = new PointPair(sample, testDataY.Values[i][0]);
                p.Tag = "(TESTDATAY - FORESEEN ON " + testDataY.GetDate(i).ToShortDateString() + ", " + testDataY.Values[i][0] + " )";
                trainingDataLine.AddPoint(p);
                sample++;
            }

            zedGraphControl1.GraphPane.Title.Text = testDataY.GetColName(0) + " - Model evaluation";
            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
        }

        private void DrawTestSeparationLine(StocksDataset ds)
        {
            int sample = ds.TrainCount + ds.PredictDays + 1;
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
            batchSize = int.Parse(textBox2.Text);
            int hiddenLayersDim = Convert.ToInt32(textBoxHidden.Text);
            int cellsNumber = Convert.ToInt32(textBoxCells.Text);
            progressBar1.Maximum = iterations;
            progressBar1.Value = 1;

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
                            textBox3.Text = iteration.ToString();
                            textBox4.Text = orchestrator.GetPreviousLossAverage().ToString();
                            progressBar1.Value = iteration;

                            reportOnGraphs(iteration);
                        }
                    }
                    ));
            }
            else
            {
                //output training process
                textBox3.Text = iteration.ToString();
                textBox4.Text =  orchestrator.GetPreviousLossAverage().ToString();
                progressBar1.Value = iteration;

                reportOnGraphs(iteration);
            }
        }

        private void reportOnGraphs(int iteration)
        {
            if (iteration == Convert.ToInt32(textBoxIterations.Text))
            {
                int sample = orchestrator.DataSet.PredictDays + 1;
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
            //predico anche l'estremo
            var dad = orchestrator.DataSet.GetExtendedArrayX();

            float[] batch = new float[dad.Length * dad.Columns];
            int sss = 0;
            for (int i = 0; i < dad.Length; i++)
            {
                for (int j = 0; j < dad.Columns; j++)
                    batch[sss++] = (float)dad.Values[i][j];
            }
            var oDataExt = orchestrator.CurrentModelTest(batch);

            int mydateIndex = 0;
            foreach (var y in oDataExt)
            {
                var p = new PointPair(sample, y[0]);
                p.Tag = "(EXTTEST - PREDICTED_ON " + dad.GetDate(mydateIndex).ToShortDateString() + ", " + y[0] + " )";
                modelLine.AddPoint(p);
                mydateIndex++;
                sample++;
            }

        }

        private void currentModelTest(int iteration, ref int sample)
        {
            var testDataX = orchestrator.DataSet.GetTestArrayX();
            bool forwardPointAdded = false;

            //get the next minibatch amount of data
            int mydateIndex = 0;

            List<double> predictectList = new List<double>();

            foreach (var miniBatchData in orchestrator.GetBatchesForTest())
            {
                var oData = orchestrator.CurrentModelTest(miniBatchData.X);

                //show on graph
                foreach (var y in oData)
                {
                    if (!forwardPointAdded)
                    {
                        var p1 = new PointPair(sample, y[0]);
                        p1.Tag = "(FF - PREDICTED ON " + testDataX.GetDate(mydateIndex).ToShortDateString() + ", " + y[0] + " )";
                        forwardModellLine.AddPoint(p1);

                        orchestrator.DataSet.ForwardPredicted.Add(new DatedValueF(testDataX.GetDate(mydateIndex), y[0]));
                        forwardPointAdded = true;
                    }

                    var p = new PointPair(sample, y[0]);
                    p.Tag = "(PREDICTEDY - PREDICTED ON " + testDataX.GetDate(mydateIndex).ToShortDateString() + ", " + y[0] + " )";
                    modelLine.AddPoint(p);
                    predictectList.Add(y[0]);
                    mydateIndex++;
                    sample++;
                }
            }

            var performances = orchestrator.ComparePredictedAgainstDataY(predictectList.ToArray(),0);
            label2.Text = "Performance (first): " + performances[1].ToString();
            DrawPerfomances(performances);

            if (forwardPointAdded)
            {
                Tuple<float, float, float> performance2 = orchestrator.CompareForwardWithDataY();
                label10.Text = "Performance (forward): " + performance2.ToString();
            }
        }

        private void DrawPerfomances(List<Performance> performances)
        {
            performanceDataLine.Clear();
            for (int i=1; i< performances.Count; i++)
            {
                performanceDataLine.AddPoint(new PointPair(i, performances[i].SuccessPercentage));
            }
            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);
        }

        private void currentModelEvaluation(int iteration, ref int sample)
        {
            //get the next minibatch amount of data
            foreach (var miniBatchData in orchestrator.GetBatchesForTraining())
            {
                var oData = orchestrator.CurrentModelEvaluate(miniBatchData.X, miniBatchData.Y);
                foreach (var y in oData)
                {
                    modelLine.AddPoint(new PointPair(sample, y[0]));
                    sample++;
                }
            }
            zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            InitiGraphs();

            orchestrator = new LSTMOrchestrator(DrawTestSeparationLine, progressReport, Convert.ToInt32(textBox2.Text));
            orchestrator.LoadAndPrepareDataSet("..\\..\\..\\Data\\FullDataSet.csv", Convert.ToInt32(textBoxYIndex.Text), 1, comboBox1.SelectedIndex + 1, Convert.ToInt32(textBoxPredictDays.Text));

            loadListView(orchestrator.DataSet);
            loadGraphs(orchestrator.DataSet);

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
