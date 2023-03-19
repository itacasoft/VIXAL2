using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using VIXAL2.Data;
using VIXAL2.Data.Base;
using VIXAL2.Data.Report;
using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;
using System.Reflection;

namespace VIXAL2.Report
{
    public class Manager
    {
        private static List<ReportItem> reportItems;
        private static ReportHeader reportHeader;
        public static DateTime ReportDate;
        public static int Iterations;
        public static int Hidden;
        public static int Cells;
        public static int BatchSize;

        public static void InitialConstructor(int iterations, int hiddenLayers, int cells, int batchSize)
        {
            Iterations = iterations;
            Hidden = hiddenLayers;
            Cells = cells;
            BatchSize = batchSize;

            reportItems = new List<ReportItem>();
            ReportDate = DateTime.Now;
        }

        public static void SaveTradesToXML(string pre, string dsType, List<FinTrade> trades)
        {
            var serializer = new XmlSerializer(typeof(List<FinTrade>));
            string reportFolder = ConfigurationManager.AppSettings["ReportFolder"];

            string directoryName = CreateAndGetDirectory(dsType);
            string filename = Path.Combine(directoryName, pre + "_trades_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml");

            using (var writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, trades);
            }
        }

        public static void SaveParametersToFile(string dsType, Dictionary<string,string> parames)
        {
            string directoryName = CreateAndGetDirectory(dsType);
            string filename = Path.Combine(directoryName, "1_initial_params_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".txt");

            using (var writer = new StreamWriter(filename))
            {
                foreach (var v in parames)
                {
                    writer.WriteLine(v.Key + " = " + v.Value);
                }
            }

            Console.WriteLine("Parameters saved to " + filename);
        }


        public static string CreateAndGetDirectory(string dsType)
        {
            string reportFolder = ConfigurationManager.AppSettings["ReportFolder"];

            string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            reportFolder = Path.Combine(currentDirectory, reportFolder);

            string result = Path.Combine(reportFolder, dsType + "_" + ReportDate.ToString("yyyyMMdd_HHmm"));
            if (!System.IO.Directory.Exists(result))
                System.IO.Directory.CreateDirectory(result);
            return result;
        }

        public static void PrintOverallReportAsExcel(string dsType)
        {
            List<ReportItem> sortedReportItems = reportItems.OrderBy(o => o.WeightedSlopePerformance).ToList();
            SaveToExcel(dsType, sortedReportItems);
        }

        public static void PrintOverallReportAsXML(string dsType)
        {
            List<ReportItem> sortedReportItems = reportItems.OrderBy(o => o.WeightedSlopePerformance).ToList();

            var serializer = new XmlSerializer(typeof(List<ReportItem>));

            string directoryName = CreateAndGetDirectory(dsType);
            string filename = Path.Combine(directoryName, "2_overall_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml");

            using (var writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, sortedReportItems);
            }
        }

        public static ReportItem ReportItemAdd(StocksDataset ds, double WeightedSlopePerformance, double AvgSlopePerformance, double AvgDiffPerformance)
        {
            if (reportHeader == null)
            {
                reportHeader = new ReportHeader();
                reportHeader.Title = "Report of " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                reportHeader.Text.Add("Dataset type: " + ds.DsType.ToString());
                reportHeader.Text.Add("Predict days: " + ds.PredictDays);
                reportHeader.Text.Add("Range days: " + ds.Range);
                reportHeader.Text.Add("Network hidden layers: " + Hidden);
                reportHeader.Text.Add("Network cells for layer: " + Cells);
                reportHeader.Text.Add("Iterations: " + Iterations);
                reportHeader.Text.Add("Batch size: " + BatchSize);
            }

            string stockName = ds.GetTestArrayY().GetColName(0);
            ReportItem item = new ReportItem();
            item.StockName = stockName;
            item.TimeOfSimulation = DateTime.Now;

            ImageConverter _imageConverter = new ImageConverter();

            //var img1 = zedGraphControl1.GetImage();

            //var img2 = zedGraphControl3.GetImage();

            item.WeightedSlopePerformance = WeightedSlopePerformance;
            item.AvgDiffPerformance = AvgDiffPerformance;
            item.AvgSlopePerformance = AvgSlopePerformance;

            reportItems.Add(item);
            return item;
        }


        public static void EnrichReportItemWithTradesData(ReportItem item, List<FinTrade> trades)
        {
            //calcolo la media e la somma
            double gainPerc = 0;
            double gain = 0;
            int goodTrades = 0;
            int badTrades = 0;
            for (int i = 0; i < trades.Count; i++)
            {
                gainPerc += trades[i].GainPerc;
                if (gainPerc > 0) goodTrades++;
                else badTrades++;
            }

            if (trades.Count > 0)
            {
                gainPerc = gainPerc / (double)trades.Count;
                gain = trades[trades.Count - 1].EndMoney - trades[0].StartMoney;
            }

            item.FinTrade_GainPerc = Math.Round(gainPerc, 3, MidpointRounding.AwayFromZero);
            item.FinTrade_Gain = Math.Round(gain, 2, MidpointRounding.AwayFromZero);
            item.FinTrade_BadTrades = badTrades;
            item.FinTrade_GoodTrades = goodTrades;
        }

        public static void EnrichReportItemWithTradesDataWithCommissions(ReportItem item, List<FinTrade> trades)
        {
            //calcolo la media e la somma
            double gainPerc = 0;
            double gain = 0;
            int goodTrades = 0;
            int badTrades = 0;
            for (int i = 0; i < trades.Count; i++)
            {
                gainPerc += trades[i].GainPerc;
                if (gainPerc > 0) goodTrades++;
                else badTrades++;
            }

            if (trades.Count > 0)
            {
                gainPerc = gainPerc / (double)trades.Count;
                gain = trades[trades.Count - 1].EndMoney - trades[0].StartMoney;
            }

            item.FinTradeComm_GainPerc = Math.Round(gainPerc, 3, MidpointRounding.AwayFromZero);
            item.FinTradeComm_Gain = Math.Round(gain, 2, MidpointRounding.AwayFromZero);
            item.FinTradeComm_BadTrades = badTrades;
            item.FinTradeComm_GoodTrades = goodTrades;
        }


        public static void SaveToFile(string pre, string dsType, Image im)
        {
            string directoryName = Manager.CreateAndGetDirectory(dsType);
            string filename = Path.Combine(directoryName, pre + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".png");

            im.Save(filename);
        }

        public static void SaveToExcel(string pre, string dsType, List<FinTrade> trades)
        {
            string directoryName = CreateAndGetDirectory(dsType);
            string filename = Path.Combine(directoryName, pre + "_trades_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");

            SLDocument sl = new SLDocument();
            int row = 1;

            if (trades.Count > 0)
            {
                var item = trades[0];
                sl.SetCellValue(row, 1, nameof(item.StockName));
                sl.SetCellValue(row, 2, nameof(item.StartDate));
                sl.SetCellValue(row, 3, nameof(item.StartPrice));
                sl.SetCellValue(row, 4, nameof(item.EndDate));
                sl.SetCellValue(row, 5, nameof(item.EndPrice));
                sl.SetCellValue(row, 6, nameof(item.TradingPosition));
                sl.SetCellValue(row, 7, nameof(item.Commissions));
                sl.SetCellValue(row, 8, nameof(item.Gain));
                sl.SetCellValue(row, 9, nameof(item.GainPerc));
                sl.SetCellValue(row, 10, "Success");

                SLStyle style = sl.CreateStyle();
                style.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Yellow, System.Drawing.Color.Blue);
                style.Font.Bold = true;
                sl.SetCellStyle(1, 1, style);
                sl.SetCellStyle(1, 2, style);
                sl.SetCellStyle(1, 3, style);
                sl.SetCellStyle(1, 4, style);
                sl.SetCellStyle(1, 5, style);
                sl.SetCellStyle(1, 6, style);
                sl.SetCellStyle(1, 7, style);
                sl.SetCellStyle(1, 8, style);
                sl.SetCellStyle(1, 9, style);
                sl.SetCellStyle(1, 10, style);
            }
            row++;

            foreach (var item in trades)
            {
                sl.SetCellValue(row, 1, item.StockName);
                sl.SetCellValue(row, 2, item.StartDate);
                sl.SetCellValue(row, 3, item.StartPrice);
                sl.SetCellValue(row, 4, item.EndDate);
                sl.SetCellValue(row, 5, item.EndPrice);
                sl.SetCellValue(row, 6, item.TradingPosition.ToString());
                sl.SetCellValue(row, 7, item.Commissions);
                sl.SetCellValue(row, 8, item.Gain);
                sl.SetCellValue(row, 9, item.GainPerc);

                if (item.Gain > 0)
                {
                    sl.SetCellValue(row, 10, 1);
                }
                else
                {
                    sl.SetCellValue(row, 10, 0);

                }
                row++;
            }

            sl.SetCellValue(row + 1, 7, "TOTAL");
            sl.SetCellValue(row + 1, 8, "=SUM(H2:H" + (row-1) + ")");

            sl.SetCellValue(row + 1, 9, "SUCCESS");
            //Non funzionano, perchè??
            sl.SetCellValue(row + 1, 10, "=SUM(J2:J" + (row - 1) + ")/COUNT(J2:J" + (row - 1) + ")");
            //sl.SetCellValue(row + 1, 10, "=SUM(J2:J" + (row - 1) + ")");

            SLStyle style1 = sl.CreateStyle();
            style1.FormatCode = "dd/mm/yyyy";
            sl.SetColumnStyle(2, style1);
            sl.SetColumnStyle(4, style1);

            SLStyle style2 = sl.CreateStyle();
            style2.FormatCode = "#,##0.00 €";
            sl.SetColumnStyle(7, style2);
            sl.SetColumnStyle(8, style2);

            SLStyle style3 = sl.CreateStyle();
            style3.FormatCode = "0.00 %";
            sl.SetColumnStyle(9, style3);

            sl.SaveAs(filename);
        }

        public static void SaveToExcel(string dsType, List<ReportItem> reportItems)
        {
            string directoryName = Manager.CreateAndGetDirectory(dsType);
            string filename = Path.Combine(directoryName, "2_overall_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx");

            SLDocument sl = new SLDocument();
            int row = 1;

            if (reportItems.Count > 0)
            {
                var item = reportItems[0];
                sl.SetCellValue(row, 1, nameof(item.StockName));
                sl.SetCellValue(row, 2, nameof(item.TimeOfSimulation));
                sl.SetCellValue(row, 3, nameof(item.WeightedSlopePerformance));
                sl.SetCellValue(row, 4, nameof(item.AvgSlopePerformance));
                sl.SetCellValue(row, 5, nameof(item.AvgDiffPerformance));
                sl.SetCellValue(row, 6, "Gain");
                sl.SetCellValue(row, 7, "GainPerc");
                sl.SetCellValue(row, 8, "GoodTrades");
                sl.SetCellValue(row, 9, "BadTrades");
                sl.SetCellValue(row, 10, "GoodRatio");

                SLStyle style = sl.CreateStyle();
                style.Fill.SetPattern(PatternValues.Solid, System.Drawing.Color.Yellow, System.Drawing.Color.Blue);
                style.Font.Bold = true;
                sl.SetCellStyle(1, 1, style);
                sl.SetCellStyle(1, 2, style);
                sl.SetCellStyle(1, 3, style);
                sl.SetCellStyle(1, 4, style);
                sl.SetCellStyle(1, 5, style);
                sl.SetCellStyle(1, 6, style);
                sl.SetCellStyle(1, 7, style);
                sl.SetCellStyle(1, 8, style);
                sl.SetCellStyle(1, 9, style);
                sl.SetCellStyle(1, 10, style);
            }
            row++;

            foreach (var item in reportItems)
            {
                sl.SetCellValue(row, 1, item.StockName);
                sl.SetCellValue(row, 2, item.TimeOfSimulation);
                sl.SetCellValue(row, 3, item.WeightedSlopePerformance);
                sl.SetCellValue(row, 4, item.AvgSlopePerformance);
                sl.SetCellValue(row, 5, item.AvgDiffPerformance);
                sl.SetCellValue(row, 6, item.FinTradeComm_Gain);
                sl.SetCellValue(row, 7, item.FinTradeComm_GainPerc);
                sl.SetCellValue(row, 8, item.FinTradeComm_GoodTrades);
                sl.SetCellValue(row, 9, item.FinTradeComm_BadTrades);

                double ratio = (double)item.FinTradeComm_GoodTrades / (double)(item.FinTradeComm_GoodTrades + item.FinTradeComm_BadTrades);
                sl.SetCellValue(row, 10, ratio);
                row++;
            }

            SLStyle style1 = sl.CreateStyle();
            style1.FormatCode = "dd/mm/yyyy hh:mm";
            sl.SetColumnStyle(2, style1);

            SLStyle style2 = sl.CreateStyle();
            style2.FormatCode = "#,##0.00 €";
            sl.SetColumnStyle(6, style2);

            SLStyle style3 = sl.CreateStyle();
            style3.FormatCode = "0.00 %";
            sl.SetColumnStyle(3, style3);
            sl.SetColumnStyle(4, style3);
            sl.SetColumnStyle(5, style3);
            sl.SetColumnStyle(7, style3);

            sl.SaveAs(filename);
        }

        private static void ReportClear()
        {
            reportItems.Clear();
            reportHeader = null;
        }

    }
}
