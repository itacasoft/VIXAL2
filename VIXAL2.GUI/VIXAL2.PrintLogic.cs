using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using VIXAL2.Data.Base;
using VIXAL2.Data.Report;

namespace VIXAL2.GUI
{
    public partial class VIXAL2Form : Form
    {
        private List<ReportItem> reportItems = new List<ReportItem>();
        private ReportHeader reportHeader;

        private void ReportClear()
        {
            reportItems.Clear();
            reportHeader = null;
        }

        //
        // Summary:
        //     Build a System.Drawing.Bitmap object containing the graphical rendering of all
        //     the ZedGraph.GraphPane objects in this list.
        //
        // Value:
        //     A System.Drawing.Bitmap object rendered with the current graph.
        public Bitmap GetImage(ZedGraph.ZedGraphControl control)
        {
            var _rect = control.GraphPane.Rect;
            if (_rect.Width == 0) _rect.Width = 852;
            if (_rect.Height == 0) _rect.Height = 450;

            Bitmap bitmap = new Bitmap((int)_rect.Width, (int)_rect.Height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.TranslateTransform(0f - _rect.Left, 0f - _rect.Top);
                control.GraphPane.Draw(graphics);
                return bitmap;
            }
        }

        private Image MergeImages(Image image1, Image image2, int space)
        {
            Bitmap bitmap = new Bitmap(Math.Max(image1.Width, image2.Width), image1.Height + image2.Height + space);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                g.DrawImage(image1, 0, 0);
                g.DrawImage(image2, 0, image1.Height + space);
            }
            Image img = bitmap;
            return img;
        }

        private ReportItem ReportItemAdd()
        {
            if (reportHeader == null)
            {
                reportHeader = new ReportHeader();
                reportHeader.Title = "Report of " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                reportHeader.Text.Add("Dataset type: " + comboBoxType.Text);
                reportHeader.Text.Add("Predict days: " + textBoxPredictDays.Text);
                reportHeader.Text.Add("Range days: " + textBoxRange.Text);
                reportHeader.Text.Add("Network hidden layers: " + textBoxHidden.Text);
                reportHeader.Text.Add("Network cells for layer: " + textBoxCells.Text);
                reportHeader.Text.Add("Iterations: " + textBoxIterations.Text);
                reportHeader.Text.Add("Batch size: " + textBoxBatchSize.Text);
            }

            string stockName = orchestrator.DataSet.GetTestArrayY().GetColName(0);
            ReportItem item = new ReportItem();
            item.StockName = stockName;
            item.TimeOfSimulation = DateTime.Now;
            //item.Text = new List<string>();

            ImageConverter _imageConverter = new ImageConverter();

            //var img1 = zedGraphControl1.GetImage();
            var img1 = GetImage(zedGraphControl1);
            byte[] xByte1 = (byte[])_imageConverter.ConvertTo(img1, typeof(byte[]));
            item.Image1 = xByte1;

            //var img2 = zedGraphControl3.GetImage();
            var img2 = GetImage(zedGraphControl3);
            byte[] xByte2 = (byte[])_imageConverter.ConvertTo(img2, typeof(byte[]));
            item.Image2 = xByte2;

            item.WeightedSlopePerformance = orchestrator.WeightedSlopePerformance;
            item.AvgDiffPerformance = orchestrator.AvgDiffPerformance;
            item.AvgSlopePerformance = orchestrator.AvgSlopePerformance;

            reportItems.Add(item);
            return item;
        }

        private void EnrichReportItemWithTradesData(ReportItem item, List<FinTrade> trades)
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

        private void EnrichReportItemWithTradesDataWithCommissions(ReportItem item, List<FinTrade> trades)
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

        private void PrintReport()
        {
            List<ReportItem> sortedReportItems = reportItems.OrderBy(o => o.WeightedSlopePerformance).ToList();

            var serializer = new XmlSerializer(typeof(List<ReportItem>));
            string reportFolder = ConfigurationManager.AppSettings["ReportFolder"];
            string filename = Path.Combine(reportFolder, "reportItems_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xml");

            using (var writer = new StreamWriter(filename))
            {
                serializer.Serialize(writer, sortedReportItems);
            }

            filename = Path.Combine(reportFolder, "report_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(reportHeader.Title).SemiBold().FontSize(14).FontColor(Colors.Blue.Medium);
                    });


                    page.Content()
                        .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre)
                        .Column(x =>
                        {
                            for (int i = 0; i < reportHeader.Text.Count; i++)
                                x.Item().ShowOnce().Text(reportHeader.Text[i]);
                            x.Item().ShowOnce().PageBreak();


                            for (int i = 0; i < sortedReportItems.Count; i++)
                            {
                                x.Item().Text("[" + sortedReportItems[i].TimeOfSimulation.ToShortTimeString() + "]" + " Stock name: " + sortedReportItems[i].StockName);
                                x.Item().Text("Performance: WeigthedSlopeDiff(%) = " + sortedReportItems[i].WeightedSlopePerformance.ToString("P") + "; SlopeDiff(%) = " + sortedReportItems[i].AvgSlopePerformance.ToString("P") + "; AbsDiff(%) = " + sortedReportItems[i].AvgDiffPerformance.ToString("P"));
                                x.Spacing(5);
                                x.Item().Image(sortedReportItems[i].Image1);
                                x.Spacing(5);

                                x.Item().Image(sortedReportItems[i].Image2);

                                x.Item().ShowIf(i < (sortedReportItems.Count-1)).PageBreak();
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })
.GeneratePdf(filename);
        }

    }
}
