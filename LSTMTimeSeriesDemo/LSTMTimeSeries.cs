using CNTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

namespace LSTMTimeSeriesDemo
{
    public partial class LSTMTimeSeries : Form
    {
        int inDim = 5;
        int ouDim = 1;
        int batchSize=100;
        string featuresName = "feature";
        string labelsName = "label";


        Dictionary<string, (float[][] train, float[][] valid, float[][] test)> DataSet;
        LineItem modelLine;
        LineItem trainingDataLine;

        LineItem lossDataLine;

        LineItem predictedLine;
        LineItem testDataLine;

        NeuralNetwork.Base.LSTMTrainer currentLSTMTrainer;

        public LSTMTimeSeries()
        {
            int timeStep = 5;
            InitializeComponent();
            InitiGraphs();
            //load data
            //load data in to memory
            var xdata = LinSpace(0, 100.0, 10000).Select(x => (float)x).ToArray<float>();
            DataSet = loadWaveDataset(Math.Sin, xdata, inDim, timeStep);
        }

        private void InitiGraphs()
        {
            ///Fitness simulation chart
            zedGraphControl1.GraphPane.Title.Text = "Model evaluation";
            zedGraphControl1.GraphPane.XAxis.Title.Text = "Samples";
            zedGraphControl1.GraphPane.YAxis.Title.Text = "Observer/Predicted";

            trainingDataLine = new LineItem("Data Points", null, null, Color.Red, ZedGraph.SymbolType.None, 1);
            trainingDataLine.Symbol.Fill = new Fill(Color.Red);
            trainingDataLine.Symbol.Size = 1;

            modelLine = new LineItem("Data Points", null, null, Color.Blue, ZedGraph.SymbolType.None, 1);
            modelLine.Symbol.Fill = new Fill(Color.Red);
            modelLine.Symbol.Size = 1;

            zedGraphControl2.GraphPane.XAxis.Title.Text = "Training Loss";
            zedGraphControl2.GraphPane.XAxis.Title.Text = "Iteration";
            zedGraphControl2.GraphPane.YAxis.Title.Text = "Loss value";

            lossDataLine = new LineItem("Loss values", null, null, Color.Red, ZedGraph.SymbolType.Circle, 1);
            lossDataLine.Symbol.Fill = new Fill(Color.Red);
            lossDataLine.Symbol.Size = 5;

            //Add line to graph
            this.zedGraphControl1.GraphPane.CurveList.Add(trainingDataLine);
           // this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl1.GraphPane.CurveList.Add(modelLine);
            this.zedGraphControl1.GraphPane.AxisChange(this.CreateGraphics());


            this.zedGraphControl2.GraphPane.CurveList.Add(lossDataLine);
            this.zedGraphControl2.GraphPane.AxisChange(this.CreateGraphics());


            zedGraphControl3.GraphPane.Title.Text = "Model testing";
            zedGraphControl3.GraphPane.XAxis.Title.Text = "Samples";
            zedGraphControl3.GraphPane.YAxis.Title.Text = "Observer/Predicted";

            testDataLine = new LineItem("Actual Data", null, null, Color.Red, ZedGraph.SymbolType.None, 1);
            testDataLine.Symbol.Fill = new Fill(Color.Red);
            testDataLine.Symbol.Size = 1;

            predictedLine = new LineItem("Prediction", null, null, Color.Blue, ZedGraph.SymbolType.None, 1);
            predictedLine.Symbol.Fill = new Fill(Color.Red);
            predictedLine.Symbol.Size = 1;


            this.zedGraphControl3.GraphPane.CurveList.Add(testDataLine);
            this.zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());
            this.zedGraphControl3.GraphPane.CurveList.Add(predictedLine);
            this.zedGraphControl3.GraphPane.AxisChange(this.CreateGraphics());

            // zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);

            //zedGraphControl3.GraphPane.YAxis.Title.Text = "Model Testing";
        }

        private void CNTKDemo_Load(object sender, EventArgs e)
        {
            loadListView(DataSet["features"].train, DataSet["label"].train);
            loadGraphs(DataSet["label"].train, DataSet["label"].test);
        }

        private void loadGraphs(float[][] train, float[][] test)
        {
            for(int i=0; i<train.Length; i++)
              trainingDataLine.AddPoint(new PointPair(i + 1, train[i][0]));

            for (int i = 0; i < test.Length; i++)
                testDataLine.AddPoint(new PointPair(i + 1, test[i][0]));

            zedGraphControl1.RestoreScale(zedGraphControl1.GraphPane);
            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);

        }

        private void loadListView(float[][] X, float[][] Y)
        {
            //clear the list first
            listView1.Clear();
            listView1.GridLines = true;
            listView1.HideSelection = false;
            if (X == null || Y == null )
                return;
            //add features
            listView1.Columns.Add(new ColumnHeader() {Width=20});
            for (int i=0; i < inDim ;i++)
            {
                ///
                var col1 = new ColumnHeader();
                col1.Text = $"x{i+1}";
                col1.Width = 70;
                listView1.Columns.Add(col1);
            }
            //Add label
            ///
            var col = new ColumnHeader();
            col.Text = $"y";
            col.Width = 70;
            listView1.Columns.Add(col);

            for (int i = 0; i < 100; i++)
            {
               var itm =  listView1.Items.Add($"{(i+1).ToString()}");
                for (int j = 0; j < X[i].Length; j++)
                    itm.SubItems.Add(X[i][j].ToString());
                itm.SubItems.Add(Y[i][0].ToString());
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
        /// Method of generating wave function y=sin(x)
        /// </summary>
        /// <param name="fun"></param>
        /// <param name="x0"></param>
        /// <param name="timeSteps"></param>
        /// <param name="timeShift"></param>
        /// <returns></returns>
        static Dictionary<string, (float[][] train, float[][] valid, float[][] test)> loadWaveDataset(Func<double, double> fun, float[] x0, int timeSteps, int timeShift)
        {
            ////fill data
            float[] xsin = new float[x0.Length];//all data
            for (int l = 0; l < x0.Length; l++)
                xsin[l] = (float)fun(x0[l]);


            //split data on training and testing part
            var a = new float[xsin.Length - timeShift];
            var b = new float[xsin.Length - timeShift];

            for (int l = 0; l < xsin.Length; l++)
            {
                //
                if (l < xsin.Length - timeShift)
                    a[l] = xsin[l];

                //
                if (l >= timeShift)
                    b[l - timeShift] = xsin[l];
            }

            //make arrays of data
            var a1 = new List<float[]>();
            var b1 = new List<float[]>();
            for (int i = 0; i < a.Length - timeSteps + 1; i++)
            {
                //features
                var row = new float[timeSteps];
                for (int j = 0; j < timeSteps; j++)
                    row[j] = a[i + j];
                //create features row
                a1.Add(row);
                //label row
                b1.Add(new float[] { b[i + timeSteps - 1] });
            }

            //split data into train, validation and test data set
            var xxx = splitData(a1.ToArray(), 0.1f, 0.1f);
            var yyy = splitData(b1.ToArray(), 0.1f, 0.1f);


            var retVal = new Dictionary<string, (float[][] train, float[][] valid, float[][] test)>();
            retVal.Add("features", xxx);
            retVal.Add("label", yyy);
            return retVal;
        }

        /// <summary>
        /// Split data on training validation and testing data sets
        /// </summary>
        /// <param name="data">full data </param>
        /// <param name="valSize">percentage amount of validation </param>
        /// <param name="testSize">percentage amount for testing</param>
        /// <returns></returns>
        static (float[][] train, float[][] valid, float[][] test) splitData(float[][] data, float valSize = 0.1f, float testSize = 0.1f)
        {
            //calculate
            var posTest = (int)(data.Length * (1 - testSize));
            var posVal = (int)(posTest * (1 - valSize));

            return (data.Skip(0).Take(posVal).ToArray(), data.Skip(posVal).Take(posTest - posVal).ToArray(), data.Skip(posTest).ToArray());
        }

        /// <summary>
        /// Taken from https://gist.github.com/wcharczuk/3948606
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="num"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static IEnumerable<double> LinSpace(double start, double stop, int num, bool endpoint = true)
        {
            var result = new List<double>();
            if (num <= 0)
            {
                return result;
            }

            if (endpoint)
            {
                if (num == 1)
                {
                    return new List<double>() { start };
                }

                var step = (stop - start) / ((double)num - 1.0d);
                result = Arange(0, num).Select(v => (v * step) + start).ToList();
            }
            else
            {
                var step = (stop - start) / (double)num;
                result = Arange(0, num).Select(v => (v * step) + start).ToList();
            }

            return result;
        }
        /// <summary>
        /// Taken from https://gist.github.com/wcharczuk/3948606
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<double> Arange(double start, int count)
        {
            return Enumerable.Range((int)start, count).Select(v => (double)v);
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

            progressBar1.Maximum = iteration;
            progressBar1.Value = 1;

            inDim = 5;
            ouDim = 1;
            int hiDim = 1;
            int cellDim = inDim;

            //            Task.Run(() =>
            //            train(DataSet, hiDim, cellDim, iteration, batchSize, progressReport, DeviceDescriptor.CPUDevice));

            currentLSTMTrainer = new NeuralNetwork.Base.LSTMTrainer(inDim, ouDim, featuresName, labelsName);
            Task.Run(() =>
            currentLSTMTrainer.Train(DataSet, hiDim, cellDim, iteration, batchSize, progressReport, DeviceDescriptor.CPUDevice));
        }


        void progressReport(CNTK.Trainer trainer, Function model, int iteration, CNTK.DeviceDescriptor device)
        {
            if (this.InvokeRequired)
            {
                // Execute the same method, but this time on the GUI thread
                this.Invoke(
                    new Action(() =>
                    {
                        //output training process
                        textBox3.Text = iteration.ToString();
                        textBox4.Text = trainer.PreviousMinibatchLossAverage().ToString();
                        progressBar1.Value = iteration;

                        reportOnGraphs(trainer, model, iteration, device);

                    }
                    ));
            }
            else
            {
                //output training process
                textBox3.Text = iteration.ToString();
                textBox4.Text = trainer.PreviousMinibatchLossAverage().ToString();
                progressBar1.Value = iteration;

                reportOnGraphs(trainer, model, iteration, device);
            }
        }

        private void reportOnGraphs(CNTK.Trainer trainer, Function model, int i, CNTK.DeviceDescriptor device)
        {
            currentModelEvaluation(trainer, model, i, device);
            currentModelTest(trainer, model, i, device);
        }

        private void currentModelTest(Trainer trainer, Function model, int i, DeviceDescriptor device)
        {
            //get the next minibatch amount of data
            int sample = 1;
            predictedLine.Clear();
            foreach (var miniBatchData in nextBatch(DataSet["features"].test, DataSet["label"].test, batchSize))
            {
                 var oData = currentLSTMTrainer.CurrentModelTest(miniBatchData.X, miniBatchData.Y);
                 //show on graph
                 foreach (var y in oData)
                     predictedLine.AddPoint(new PointPair(sample++, y[0]));
            }
            zedGraphControl3.RestoreScale(zedGraphControl3.GraphPane);
        }

        private void currentModelEvaluation(Trainer trainer, Function model, int i, DeviceDescriptor device)
        {
            lossDataLine.AddPoint(new PointPair(i, trainer.PreviousMinibatchLossAverage()));

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
    }
}
