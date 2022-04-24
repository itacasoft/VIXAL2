using SharpML.Types;
using SharpML.Types.Normalization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VIXAL2.Data;
using ZedGraph;

namespace LSTMTimeSeriesDemo
{
    public partial class LSTMTimeSeries : Form
    {
        int batchSize=100;
        int daysPredict = 10;
        string featuresName = "feature";
        string labelsName = "label";

#if ORIGINAL_WORK
        Dictionary<string, (float[][] train, float[][] valid, float[][] test)> DataSet;
#else
        StocksDataset DataSet;
#endif


        LineItem modelLine;
        LineItem trainingDataLine;

        LineItem lossDataLine;
        LineItem performanceDataLine;

        LineItem predictedLine;
        LineItem predictedLineExtreme;
        LineItem testDataLine;

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

            if (testDataLine != null) testDataLine.Clear();
            else
                testDataLine = new LineItem("Actual Data (Y)", null, null, Color.Blue, ZedGraph.SymbolType.None, 1);
            testDataLine.Symbol.Fill = new Fill(Color.Blue);
            testDataLine.Symbol.Size = 1;

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
            this.zedGraphControl3.GraphPane.CurveList.Add(testDataLine);
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

        private void loadGraphs(float[][] train, float[][] test)
        {
            for(int i=0; i<train.Length; i++)
              trainingDataLine.AddPoint(new PointPair(i + 1, train[i][0]));

            for (int i = 0; i < test.Length; i++)
                testDataLine.AddPoint(new PointPair(i + 1, test[i][0]));

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
            for (int i = 0; i < ds.StockNames.Length; i++)
            {
                var col1 = new ColumnHeader();
                col1.Text = ds.StockNames[i];
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

        static void SaveFullDataSet(Dictionary<string, (float[][] train, float[][] valid, float[][] test)> ds, string path)
        {
            List<float[]> data = new List<float[]>();
            
            for (int i=0; i < ds["features"].train.Length; i++)
            {
                data.Add(ds["features"].train[i]);
            }

            for (int i = 0; i < ds["features"].valid.Length; i++)
            {
                data.Add(ds["features"].valid[i]);
            }

            for (int i = 0; i < ds["features"].test.Length; i++)
            {
                data.Add(ds["features"].test[i]);
            }

            double[][] data1 = new double[data.Count][];
            for (int i=0; i<data.Count; i++)
            {
                data1[i] = new double[data[i].Length];
                for (int j=0; j<data[i].Length; j++)
                {
                    data1[i][j] = data[i][j];
                }
            }

            Utils.WriteCsv(data1, path);
        }


        static Dictionary<string, (float[][] train, float[][] valid, float[][] test)> loadFullDataSet(string path, int gap)
        {
            void ProcessData(string[][] rawData, out List<string> names, out List<DateTime> dates, out double[][] data)
            {
                //first row is header (symbols), exluding the first field (date)
                names = new List<string>();
                for (int i = 1; i < rawData[0].Length; i++)
                {
                    names.Add(rawData[0][i]);
                }

                dates = new List<DateTime>();

                data = new double[rawData.Length - 1][];
                for (int row = 1; row < rawData.Length; row++)
                {
                    data[row - 1] = new double[rawData[row].Length - 1]; //first column is dates

                    DateTime stockDate;
                    if (!DateTime.TryParse(rawData[row][0], out stockDate))
                        throw new Exception(rawData[row][0] + " at row " + row.ToString() + " does not represent a valid date");
                    dates.Add(stockDate);

                    for (int col = 1; col < rawData[row].Length; col++)
                    {
                        if (!double.TryParse(rawData[row][col], out data[row - 1][col - 1]))
                        {
                            throw new InvalidCastException("Field at row " + (row).ToString() + ", col " + col.ToString() + " is not a valid double value");
                        }
                    }
                }
            }


            string[][] stocksData = Utils.LoadCsvAsStrings(path, gap+1);

            List<string> stockNames;
            List<DateTime> stockDates;
            double[][] stockData;
            ProcessData(stocksData, out stockNames, out stockDates, out stockData);

            StockList.FillNaNs(stockData);

            Normalizer.Instance.Initialize(stockData, stockData.Length - gap);
            float[][] fStockData = Utils.ToFloatArray(Normalizer.Instance.Normalize(stockData));
            float[][] y = new float[fStockData.Length][];

            for (int row = 0; row < fStockData.Length; row++)
            {
                y[row] = new float[1];
                if (row >= gap)
                    y[row - gap][0] = fStockData[row][0];
            }

            var xxx = splitAndRemoveBlankData(fStockData, 0.2f, 0.2f, gap);
            var yyy = splitAndRemoveBlankData(y, 0.2f, 0.2f, gap);

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
        static (float[][] train, float[][] valid, float[][] test) splitData(float[][] data, float valSize, float testSize)
        {
            //calculate
            var posTest = (int)(data.Length * (1 - testSize));
            var posVal = (int)(posTest * (1 - valSize));

            return (data.Skip(0).Take(posVal).ToArray(), data.Skip(posVal).Take(posTest - posVal).ToArray(), data.Skip(posTest).ToArray());
        }

        static (float[][] train, float[][] valid, float[][] test) splitAndRemoveBlankData(float[][] data, float valSize, float testSize, int blankSize)
        {
            //calculate
            int total = data.Length;
            var posVal = (int)(total * (1 - valSize-testSize));
            var posTest = (int)(total * (1 - testSize));
            var lenTest = data.Length - posTest - blankSize;

            return (data.Skip(0).Take(posVal).ToArray(), data.Skip(posVal).Take(posTest - posVal).ToArray(), data.Skip(posTest).Take(lenTest).ToArray());
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
            int hiddenLayersDim = Convert.ToInt32(textBoxHidden.Text);
            int cellsNumber = Convert.ToInt32(textBoxCells.Text);
            progressBar1.Maximum = iteration;
            progressBar1.Value = 1;

            int ouDim = DataSet.TrainDataY[0].Length;
            int inDim = DataSet.StockNames.Length;

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
                        //output training process
                        textBox3.Text = iteration.ToString();
                        textBox4.Text = currentLSTMTrainer.PreviousMinibatchLossAverage.ToString();
                        progressBar1.Value = iteration;

                        reportOnGraphs(iteration);

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
            var normalizedDadValues = Normalizer.Instance.Normalize(dad.Values);

            float[] bat = new float[dad.Length * dad.Columns];
            int sss = 0;
            for (int i = 0; i < dad.Length; i++)
            {
                for (int j = 0; j < dad.Columns; j++)
                    bat[sss++] = (float)normalizedDadValues[i][j];
            }
            var oDataExt = currentLSTMTrainer.CurrentModelTest(bat);
            foreach (var y in oDataExt)
            {
                predictedLineExtreme.AddPoint(new PointPair(sample++, y[0]));
            }

        }

        private void currentModelTest(int iteration, out int sample)
        {
            //get the next minibatch amount of data
            sample = 1;
            predictedLine.Clear();
            List<float> predictectList = new List<float>();
            List<float> dataYList = new List<float>();

            foreach (var miniBatchData in nextBatch(DataSet["features"].test, DataSet["label"].test, batchSize))
            {
                 var oData = currentLSTMTrainer.CurrentModelTest(miniBatchData.X);
                //show on graph
                foreach (var y in oData)
                {
                    predictedLine.AddPoint(new PointPair(sample++, y[0]));
                    predictectList.Add(y[0]);
                }
            }

            float[][] dataY = DataSet["label"].test;
            for (int i = 0; i < dataY.Length; i++)
                dataYList.Add(dataY[i][0]);

            float performance = Compare(dataYList.ToArray(), predictectList.ToArray());
            label2.Text = "Performance: " + performance.ToString();

        }

        public virtual float Compare(float[] dataY, float[] dataPredicted)
        {
            float result;

            float guessed = 0, failed = 0;
            float predicted0 = dataPredicted[0];
            float future0 = dataY[0];

            for (int row = 1; row < dataY.Length; row++)
            {
                float future1 = dataY[row];
                bool futurePositiveTrend = (future1 > future0);

                float predicted1 = dataPredicted[row];
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

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            InitiGraphs();

#if ORIGINAL_WORK
            //load data in to memory
            var xdata = LinSpace(0, 100.0, 10000).Select(x => (float)x).ToArray<float>();
            DataSet = loadWaveDataset(Math.Sin, xdata, inDim, daysPredict);
#else
            DataSet = DatasetFactory.CreateDataset("..\\..\\..\\..\\Data\\FullDataSet.csv", 2, comboBox1.SelectedIndex + 1);
            DataSet.Prepare();
#endif

            loadListView(DataSet);
            loadGraphs(DataSet["label"].train, DataSet["label"].test);

            buttonStart.Enabled = true;
        }
    }
}
