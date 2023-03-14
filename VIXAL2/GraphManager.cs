using NeuralNetwork.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using VIXAL2.Report;
using ZedGraph;

namespace VIXAL2
{
    internal class GraphManager
    {
        #region StaticProperties
        public static List<DoubleDatedValue> latestPredictedList;
        #endregion

        public int Iterations;
        public int Hidden;
        public int Cells;
        public int BatchSize;
        GraphPane Pane;
        LineItem originalLine;
        StocksDataset ds;

        public GraphManager(StocksDataset ds, int iterations, int hiddenLayers, int cells, int batchSize)
        {
            Iterations = iterations;
            Hidden = hiddenLayers;
            Cells = cells;
            BatchSize = batchSize;
            this.ds = ds;
            Pane = new GraphPane(new RectangleF(0, 0, 8000, 2000), "Model Evaluation", "Samples", "Observed/Predicted");
        }

        private void DrawTestSeparationLine()
        {
            var separationline = new LineItem("");
            separationline.Symbol.Fill = new Fill(Color.Black);
            separationline.Line.Style = System.Drawing.Drawing2D.DashStyle.Dot;
            separationline.Symbol.Size = 1;
            Pane.CurveList.Add(separationline);

            int sample = 1 + ds.TrainCount + ds.ValidCount + ds.PredictDays + ds.DelayDays;
            var myDate = ds.OriginalData.SampleIndexToDate(sample - 1).Value;

            separationline.Clear();
            var p1 = new PointPair(sample, ds.MinYValue);
            p1.Tag = "( " + myDate.ToShortDateString() + " )";
            separationline.AddPoint(p1);
            p1 = new PointPair(sample, ds.MaxYValue);
            p1.Tag = "( " + myDate.ToShortDateString() + " )";
            separationline.AddPoint(p1);

            TextObj text = new TextObj("(" + myDate.ToShortDateString() + ")", sample, ds.MinYValue);
            text.Location.AlignH = AlignH.Center;
            text.Location.AlignV = AlignV.Bottom;
            text.FontSpec.Border.IsVisible = false;
            text.FontSpec.Fill.IsVisible = false;
            text.FontSpec.Size = 10;
            Pane.GraphObjList.Add(text);
        }

        private void DrawTrainingLine()
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

        private void DrawPredictedLine(List<DoubleDatedValue> predictedList)
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

        /// <summary>
        /// Prints the graph of performances to a png file
        /// </summary>
        /// <param name="slopePerformance"></param>
        /// <param name="AvgSlopePerformance"></param>
        /// <param name="diffPerformance"></param>
        /// <param name="AvgDiffPerformance"></param>
        public void PrintPerformances(Performance[] slopePerformance, double AvgSlopePerformance, PerformanceDiff[] diffPerformance, double AvgDiffPerformance)
        {
            var myPane = new GraphPane(new RectangleF(0, 0, 2000, 1000), "Performances", "Days from start", "Success Percentage");

            LineItem slopePerformanceDataLine = new LineItem("SlopePerformance", null, null, Color.DarkKhaki, ZedGraph.SymbolType.Diamond, 1);
            slopePerformanceDataLine.Symbol.Fill = new Fill(Color.DarkKhaki);
            slopePerformanceDataLine.Symbol.Size = 5;

            LineItem diffPerformanceDataLine = new LineItem("DiffPerformance", null, null, Color.DarkOliveGreen, ZedGraph.SymbolType.Diamond, 1);
            diffPerformanceDataLine.Symbol.Fill = new Fill(Color.DarkOliveGreen);
            diffPerformanceDataLine.Symbol.Size = 5;

            for (int i = 1; i < slopePerformance.Length; i++)
            {
                var p = new PointPair(i, slopePerformance[i].SuccessPercentage);
                p.Tag = slopePerformance[i].ToString();
                slopePerformanceDataLine.AddPoint(p);
            }

            diffPerformanceDataLine.Clear();
            for (int i = 0; i < diffPerformance.Length; i++)
            {
                var p = new PointPair(i, diffPerformance[i].SuccessPercentage);
                p.Tag = diffPerformance[i].ToString();
                diffPerformanceDataLine.AddPoint(p);
            }

            myPane.CurveList.Add(slopePerformanceDataLine);
            myPane.CurveList.Add(diffPerformanceDataLine);

            myPane.Title.Text = "Performance: SlopeDiff(%) = " + AvgSlopePerformance.ToString("P") + "; Diff(%) = " + AvgDiffPerformance.ToString("P");

            SaveToFile(ds.GetTestArrayY().GetColName(0) + "_perf", ds.DsType.ToString(), myPane);
        }

        private void DrawOriginalLine()
        {
            originalLine = new LineItem("Real Data", null, null, Color.Black, ZedGraph.SymbolType.None, 1);
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

        public void DrawPredicted(List<DoubleDatedValue> predictedList)
        {
            latestPredictedList = predictedList;

            DrawOriginalLine();

            DrawTrainingLine();

            DrawTestSeparationLine();

            DrawPredictedLine(predictedList);
        }

        public void DrawLatestPredicted()
        {
            DrawPredicted(latestPredictedList);
        }

        /// <summary>
        /// Prints the graph
        /// </summary>
        public void Print()
        {
            Pane.Title.Text = ds.GetTestArrayY().GetColName(0) + " - (DsType:" + ds.DsType.ToString() + ", Iterations:" + Iterations + ", Hidden:" + Hidden + ", Cells:" + Cells + ")";
            SaveToFile(ds.GetTestArrayY().GetColName(0), ds.DsType.ToString(), Pane);
        }


        public void DrawTrades(List<FinTrade> trades)
        {
            TimeSerieArray originalData;

            if (ds.NormalizeFirst)
                originalData = ds.OriginalNormalizedData;
            else
                originalData = ds.OriginalData;

            if (originalLine.Points.Count != originalData.Length)
                throw new Exception("Points.Count different from Original Data lenght");

            for (int i = 0; i < trades.Count; i++)
            {
                int? tradeStartIndex = originalData.DateToSampleIndex(trades[i].StartDate);
                int? tradeEndIndex = originalData.DateToSampleIndex(trades[i].EndDate);

                var p0 = originalLine.Points[tradeStartIndex.Value];
                p0.Tag = trades[i].StartDateAsString;
                var p1 = originalLine.Points[tradeEndIndex.Value];
                p1.Tag = trades[i].EndDateAsString;

                LineItem l;
                if (trades[i].TradingPosition == TradingPosition.Long)
                {
                    l = new LineItem("Trade" + (i + 1).ToString() + " (" + (int)(trades[i].GainPerc * 100) + "%)", null, null, Color.Green, ZedGraph.SymbolType.Diamond, 3);
                }
                else
                {
                    l = new LineItem("Trade" + (i + 1).ToString() + " (" + (int)(trades[i].GainPerc * 100) + "%)", null, null, Color.DarkMagenta, ZedGraph.SymbolType.Diamond, 3);
                }

                l.AddPoint(p0);
                l.AddPoint(p1);
                Pane.CurveList.Add(l);
            }

            Pane.XAxis.Scale.Min = -20;
        }

        public static void SaveToFile(string pre, string dsType, GraphPane pane)
        {
            Bitmap bm = new Bitmap(10, 10);
            Graphics g = Graphics.FromImage(bm);
            pane.AxisChange(g);

            Image im = pane.GetImage();

            Manager.SaveToFile(pre, dsType, im);
        }
    }
}
