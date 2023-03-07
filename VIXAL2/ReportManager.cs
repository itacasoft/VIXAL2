using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using ZedGraph;

namespace VIXAL2
{
    internal class ReportManager
    {

        GraphPane Pane;
        int iterations;
        int hidden;
        int cells;

        public ReportManager(int iterations, int hiddenLayers, int cells)
        {
            this.iterations = iterations;
            this.hidden = hiddenLayers;
            this.cells = cells;
            Pane = new GraphPane(new RectangleF(0, 0, 8000, 2000), "Model Evaluation", "Samples", "Observed/Predicted");
        }

        private void DrawTestSeparationLine(StocksDataset ds)
        {
            var separationline = new LineItem("");
            separationline.Symbol.Fill = new Fill(Color.Black);
            separationline.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
            separationline.Symbol.Size = 1;
            Pane.CurveList.Add(separationline);


            int sample = 1 + ds.TrainCount + ds.ValidCount + ds.PredictDays + ds.DelayDays;
            TimeSerieArray testDataY = ds.GetTestArrayY();

            separationline.Clear();
            var p1 = new PointPair(sample, ds.MinYValue);
            p1.Tag = "( " + testDataY.GetDate(0).ToShortDateString() + " )";
            separationline.AddPoint(p1);
            p1 = new PointPair(sample, ds.MaxYValue);
            p1.Tag = "( " + testDataY.GetDate(0).ToShortDateString() + " )";
            separationline.AddPoint(p1);
        }

        private void DrawTrainingLine(StocksDataset ds)
        {
            const int COL_TO_DRAW = 0;
            var trainDataY = ds.GetTrainArrayY();

            //disegno il trainingDataY dal giorno PredictDay così mi ritrovo allineato con i
            //prezzi reali (non sono sicuro che vada bene così però)
            int sample = 1 + ds.PredictDays + ds.DelayDays;

            var trainingDataLine = new LineItem("Training Data", null, null, Color.Blue, ZedGraph.SymbolType.None, 1);
            trainingDataLine.Symbol.Fill = new Fill(Color.Blue);
            trainingDataLine.Symbol.Size = 1;
            Pane.CurveList.Add(trainingDataLine);

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

            TimeSerieArrayExt testDataY = ds.GetTestArrayY();

            for (int i = 0; i < testDataY.Length; i++)
            {
                var p = new PointPair(sample, testDataY[i][COL_TO_DRAW]);
                p.Tag = "(TESTDATAY - " + testDataY.GetDate(i).ToShortDateString() + " (value of " + testDataY.GetFutureDate(i).ToShortDateString() + "): " + testDataY[i][0] + " )";
                trainingDataLine.AddPoint(p);
                sample++;
            }

            trainingDataLine.Label.Text = "Training (" + testDataY.ToStringExt() + ", R:" + ds.Range + "" + ")";
        }

        private void DrawPredictedLine(StocksDataset ds, List<DoubleDatedValue> predictedList)
        {
            var testDataY = ds.GetTestArrayY();

            var predictedLine = new LineItem("Prediction Data", null, null, Color.Red, ZedGraph.SymbolType.None, 1);
            predictedLine.Symbol.Fill = new Fill(Color.Red);
            predictedLine.Symbol.Size = 1;
            predictedLine.Label.Text = "Model/Prediction (" + testDataY.ToStringExt() + ", R:" + ds.Range + ")";
            Pane.CurveList.Add(predictedLine);

            int sampleIndex = 1 + ds.PredictDays + ds.DelayDays;

            foreach (var y in predictedList)
            {
                predictedLine.AddPoint(new PointPair(sampleIndex, y.Value));
                sampleIndex++;
            }
        }

        private void DrawOriginalLine(StocksDataset ds)
        {
            LineItem originalLine = new LineItem("Real Data", null, null, Color.Black, ZedGraph.SymbolType.None, 1);
            originalLine.Symbol.Fill = new Fill(Color.Black);
            originalLine.Symbol.Size = 1;
            originalLine.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            Pane.CurveList.Add(originalLine);

            TimeSerieArray realData;

            if (ds.NormalizeFirst)
                realData = ds.OriginalNormalizedData;
            else
                realData = ds.OriginalData;

            var sample1 = 1;// + ds.PredictDays;
            int sampleIndex = 0;

            for (int i = sampleIndex; i < realData.Length; i++)
            {
                var p = new PointPair(sample1, realData.Values[i][ds.FirstColumnToPredict]);
                p.Tag = "[" + sample1.ToString() + "] " + realData.GetDate(i).ToShortDateString() + ": " + realData.Values[i][ds.FirstColumnToPredict] + "";
                originalLine.AddPoint(p);
                sample1++;
            }

            originalLine.Label.Text = "Close Price (" + realData.GetColName(ds.FirstColumnToPredict) + ")";
        }

        public Image PrintGraphs(StocksDataset ds, List<DoubleDatedValue> predictedList)
        {
            DrawOriginalLine(ds);

            DrawTrainingLine(ds);

            DrawTestSeparationLine(ds);

            DrawPredictedLine(ds, predictedList);

            Pane.Title.Text = ds.GetTestArrayY().GetColName(0) + " - (DsType:" + ds.ClassShortName + ", Iterations:" + iterations + ", Hidden:" + hidden + ", Cells:" + cells + ")";


            Bitmap bm = new Bitmap(10, 10);
            Graphics g = Graphics.FromImage(bm);
            Pane.AxisChange(g);

            Image im = Pane.GetImage();

            string reportFolder = ConfigurationManager.AppSettings["ReportFolder"];
            string filename = Path.Combine(reportFolder, "graph_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".png");

            im.Save(filename);
            return im;
        }
    }
}
