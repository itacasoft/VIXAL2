using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using ZedGraph;

namespace LSTMTimeSeriesDemo
{
    public partial class LSTMTimeSeries : Form
    {
        int batchSize=100;
        string featuresName = "feature";
        string labelsName = "label";

        StocksDataset DataSet;

        LineItem modelLine;
        LineItem trainingDataLine;

        LineItem lossDataLine;
        LineItem performanceDataLine;

        LineItem predictedLine;
        LineItem predictedLineExtreme;
        LineItem testDataXLine;
        LineItem testDataYLine;

        NeuralNetwork.Base.LSTMTrainer currentLSTMTrainer;

        public LSTMTimeSeries()
        {
            InitializeComponent();
        }

        private void InitiGraphs()
        {
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
           // this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl1.GraphPane.CurveList.Add(modelLine);
            this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());

            this.zedGraphControl2.GraphPane.CurveList.Clear();
            this.zedGraphControl2.GraphPane.CurveList.Add(lossDataLine);
            this.zedGraphControl2.GraphPane.CurveList.Add(performanceDataLine);
            this.zedGraphControl2.GraphPane.AxisChange(this.CreateGraphics());


            zedGraphControl3.GraphPane.Title.Text = "Model testing";
            zedGraphControl3.GraphPane.XAxis.Title.Text = "Samples";
            zedGraphControl3.GraphPane.YAxis.Title.Text = "Observer/Predicted";

            if (testDataXLine != null) testDataXLine.Clear();
            else
                testDataXLine = new LineItem("Actual Data (X)", null, null, Color.Black, ZedGraph.SymbolType.None, 1);
            testDataXLine.Symbol.Fill = new Fill(Color.Black);
            testDataXLine.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
            testDataXLine.Symbol.Size = 1;

            if (testDataYLine != null) testDataYLine.Clear();
            else
                testDataYLine = new LineItem("Expected Data (Y)", null, null, Color.Blue, ZedGraph.SymbolType.None, 1);
            testDataYLine.Symbol.Fill = new Fill(Color.Blue);
            testDataYLine.Symbol.Size = 1;

            zedGraphControl3.IsShowPointValues = true;
            zedGraphControl3.PointValueFormat = "0.0000";
            zedGraphControl3.PointDateFormat = "d";

            if (predictedLine != null) predictedLine.Clear();
            else
                predictedLine = new LineItem("Prediction", null, null, Color.Red, ZedGraph.SymbolType.None, 1);
            predictedLine.Symbol.Fill = new Fill(Color.Red);
            predictedLine.Symbol.Size = 1;

            if (predictedLineExtreme != null) predictedLineExtreme.Clear();
            else
                predictedLineExtreme = new LineItem("PredictionExtreme", null, null, Color.Violet, ZedGraph.SymbolType.Diamond, 1);
            predictedLineExtreme.Symbol.Fill = new Fill(Color.Violet);
            predictedLineExtreme.Symbol.Size = 2;
            predictedLineExtreme.Symbol.Type = SymbolType.Diamond;

            this.zedGraphControl3.GraphPane.CurveList.Clear();
            this.zedGraphControl3.GraphPane.CurveList.Add(testDataXLine);
            this.zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl3.GraphPane.CurveList.Add(testDataYLine);
            this.zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl3.GraphPane.CurveList.Add(predictedLine);
            this.zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl3.GraphPane.CurveList.Add(predictedLineExtreme);
            this.zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());

            // zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);

            //zedGraphControl3.GraphPane.YAxis.Title.Text = "Model Testing";
        }

        private void CNTKDemo_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            buttonStart.Enabled = false;
            InitiGraphs();
        }

        private void loadGraphs(StocksDataset ds)
        {
            double[] traindDataYList = Utils.GetVectorFromArray(DataSet.TrainDataY, 0);

            for (int i = 0; i < traindDataYList.Length; i++)
            {
                var p = new PointPair(i + 1, traindDataYList[i]);
                trainingDataLine.AddPoint(p);
            }

            //cache data for next iteration
            if (ds.Obj1 == null)
                ds.Obj1 = ds.GetTestArrayX();
            
            TimeSerieArray testDataX = (TimeSerieArray)ds.Obj1;

            for (int i = 0; i < testDataX.Length; i++)
            {
                var p = new PointPair(i + 1, testDataX.Values[i][0]);
                p.Tag = "( " + testDataX.GetDate(i).ToShortDateString() + ": " + testDataX.Values[i][0] + " )";
                testDataXLine.AddPoint(p);
            }

            //cache data for next iteration
            if (ds.Obj2 == null)
                ds.Obj2 = ds.GetTestArrayY();

            TimeSerieArray testDataY = (TimeSerieArray)ds.Obj2;

            for (int i = 0; i < testDataY.Length; i++)
            {
                var p = new PointPair(i + 1, testDataY.Values[i][0]);
                p.Tag = "( " + testDataY.GetDate(i).ToShortDateString() + ": " + testDataY.Values[i][0] + " )";
                testDataYLine.AddPoint(p);
            }

            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);
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
            listView1.Columns.Add(new ColumnHeader() { Width = 20 });
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

            for (int i = 0; i < ds.TrainDataX.Length; i++)
            {
                var itm = listView1.Items.Add($"{(i + 1).ToString()}");
                for (int j = 0; j < ds.TrainDataX[i].Length; j++)
                    itm.SubItems.Add(ds.TrainDataX[i][j].ToString());

                for (int j = 0; j < ds.TrainDataY[i].Length; j++)
                    itm.SubItems.Add(ds.TrainDataY[i][j].ToString());
            }

            for (int i = 0; i < ds.ValidDataX.Length; i++)
            {
                var itm = listView1.Items.Add($"{(i + 1).ToString()}");
                itm.BackColor = Color.Yellow;
                for (int j = 0; j < ds.ValidDataX[i].Length; j++)
                    itm.SubItems.Add(ds.ValidDataX[i][j].ToString());

                for (int j = 0; j < ds.ValidDataY[i].Length; j++)
                    itm.SubItems.Add(ds.ValidDataY[i][j].ToString());
            }

            for (int i = 0; i < ds.TestDataX.Length; i++)
            {
                var itm = listView1.Items.Add($"{(i + 1).ToString()}");
                itm.BackColor = Color.LightCoral;
                for (int j = 0; j < ds.TestDataX[i].Length; j++)
                    itm.SubItems.Add(ds.TestDataX[i][j].ToString());

                for (int j = 0; j < ds.TestDataY[i].Length; j++)
                    itm.SubItems.Add(ds.TestDataY[i][j].ToString());
            }
        }


        /// <summary>
        /// Iteration method for enumerating data during iteration process of training
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="mMSize"></param>
        /// <returns></returns>
        private static IEnumerable<(float[] X, float[] Y)> nextBatch(float[][] X, float[][] Y, int mMSize)
        {

            float[] asBatch(float[][] data, int start, int count)
            {
                var lst = new List<float>();
                for (int i = start; i < start + count; i++)
                {
                    if (i >= data.Length)
                        break;

                    lst.AddRange(data[i]);
                }
                return lst.ToArray();
            }

            for (int i = 0; i <= X.Length - 1; i += mMSize)
            {
                var size = X.Length - i;
                if (size > 0 && size > mMSize)
                    size = mMSize;

                var x = asBatch(X, i, size);
                var y = asBatch(Y, i, size);

                yield return (x, y);
            }

        }


        /// <summary>
        /// Starts the training process of LSTM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int iteration = int.Parse(textBox1.Text);
            batchSize = int.Parse(textBox2.Text);
            int hiddenLayersDim = Convert.ToInt32(textBoxHidden.Text);
            int cellsNumber = Convert.ToInt32(textBoxCells.Text);
            progressBar1.Maximum = iteration;
            progressBar1.Value = 1;

            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.ColNames.Length;

            //            Task.Run(() =>
            //            train(DataSet, hiDim, cellDim, iteration, batchSize, progressReport, DeviceDescriptor.CPUDevice));

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);
            Task.Run(() =>
            currentLSTMTrainer.Train(DataSet.GetFeatureLabelDataSet(), hiddenLayersDim, cellsNumber, iteration, batchSize, progressReport, NeuralNetwork.Base.DeviceType.CPUDevice));
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
                            textBox4.Text = currentLSTMTrainer.PreviousMinibatchLossAverage.ToString();
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
                textBox4.Text = currentLSTMTrainer.PreviousMinibatchLossAverage.ToString();
                progressBar1.Value = iteration;

                reportOnGraphs(iteration);
            }
        }

        private void reportOnGraphs(int iteration)
        {
            currentModelEvaluation(iteration);

            int sample;
            currentModelTest(iteration, out sample);
            currentModelTestExtreme(sample);

            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);
        }

        private void currentModelTestExtreme(int sample)
        {
            predictedLineExtreme.Clear();
            //predico anche l'estremo
            var dad = DataSet.GetExtendedArrayX();

            float[] batch = new float[dad.Length * dad.Columns];
            int sss = 0;
            for (int i = 0; i < dad.Length; i++)
            {
                for (int j = 0; j < dad.Columns; j++)
                    batch[sss++] = (float)dad.Values[i][j];
            }
            var oDataExt = currentLSTMTrainer.CurrentModelTest(batch);

            int mydateIndex = 0;
            foreach (var y in oDataExt)
            {
                var p = new PointPair(sample++, y[0]);
                p.Tag = "( " + dad.GetDate(mydateIndex).ToShortDateString() + ": " + y[0] + " )";
                predictedLineExtreme.AddPoint(p);
                mydateIndex++;
            }

        }

        private void currentModelTest(int iteration, out int sample)
        {
            var testDataX = DataSet.GetTestArrayX();

            //get the next minibatch amount of data
            sample = 1;
            int mydateIndex = 0;
            predictedLine.Clear();
            List<double> predictectList = new List<double>();

            foreach (var miniBatchData in nextBatch(DataSet["features"].test, DataSet["label"].test, batchSize))
            {
                 var oData = currentLSTMTrainer.CurrentModelTest(miniBatchData.X);
                //show on graph
                foreach (var y in oData)
                {
                    var p = new PointPair(sample++, y[0]);
                    p.Tag = "( " + testDataX.GetDate(mydateIndex).ToShortDateString() + ": " + y[0] + " )";
                    predictedLine.AddPoint(p);
                    predictectList.Add(y[0]);
                    mydateIndex++;
                }
            }

            double[] dataYList = Utils.GetVectorFromArray(DataSet.TestDataY, 0);

            float performance = Compare(dataYList, predictectList.ToArray());
            label2.Text = "Performance: " + performance.ToString();
        }

        public virtual float Compare(double[] dataY, double[] dataPredicted)
        {
            float result;

            float guessed = 0, failed = 0;
            double predicted0 = dataPredicted[0];
            double future0 = dataY[0];

            for (int row = 1; row < dataY.Length; row++)
            {
                double future1 = dataY[row];
                bool futurePositiveTrend = (future1 > future0);

                double predicted1 = dataPredicted[row];
                bool predictedPositiveTrend = (predicted1 > predicted0);

                if (predictedPositiveTrend == futurePositiveTrend)
                    guessed++;
                else
                    failed++;

                predicted0 = predicted1;
                future0 = future1;
            }

            result = guessed / (guessed + failed);

            return result;
        }


        private void currentModelEvaluation(int iteration)
        {
            lossDataLine.AddPoint(new PointPair(iteration, currentLSTMTrainer.PreviousMinibatchLossAverage));

            //get the next minibatch amount of data
            int sample = 1;
            modelLine.Clear();
            foreach (var miniBatchData in nextBatch(DataSet["features"].train, DataSet["label"].train, batchSize))
            {
                var oData = currentLSTMTrainer.CurrentModelEvaluate(miniBatchData.X, miniBatchData.Y);
                foreach (var y in oData)
                    modelLine.AddPoint(new PointPair(sample++, y[0]));
            }
            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
            zedGraphControl2.RestoreScale(zedGraphControl2.GraphPane);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            InitiGraphs();

            DataSet = DatasetFactory.CreateDataset("..\\..\\..\\..\\Data\\FullDataSet.csv", Convert.ToInt32(textBox5.Text), 1, (DataSetType)( comboBox1.SelectedIndex + 1));
            if (DataSet.GetType() == typeof(MovingAverageDataSet))
                ((MovingAverageDataSet)DataSet).PredictDays = Convert.ToInt32(textBox6.Text);

            int testCount = (int)(DataSet.Count * (1.0F - 0.8F));
            DataSet.Prepare(0, testCount);

            loadListView(DataSet);
            loadGraphs(DataSet);

            buttonStart.Enabled = true;
            label9.Text = "Dataset: " + DataSet.DataList[0].Length + " cols, " + DataSet.DataList.Count + " rows";
//            textBoxCells.Text = DataSet.AllData[0].Length.ToString();

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            currentLSTMTrainer.StopNow = true;
        }
    }
}
