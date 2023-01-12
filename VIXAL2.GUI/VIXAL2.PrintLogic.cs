using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QuestPDF.Fluent;
using QuestPDF.Helpers;


namespace VIXAL2.GUI
{
    public partial class VIXAL2Form : Form
    {
        private class ReportHeader
        {
            public string Title;
            public List<string> Text = new List<string>();

            public override string ToString()
            {
                string res = Title + Environment.NewLine;
                for (int i=0; i<Text.Count; i++)
                {
                    res += Text[i];
                    res += ", ";
                }
                return res;
            }
        }

        private class ReportItem
        {
            public string StockName;
            public DateTime TimeOfSimulation;
            public List<string> Text;
            public byte[] Image1;
            public byte[] Image2;
            public double WeightedSlopePerformance;
            public double AvgSlopePerformance;
            public double AvgDiffPerformance;
        }

        private List<ReportItem> reportItems = new List<ReportItem>();
        private ReportHeader reportHeader;

        private void ReportClear()
        {
            reportItems.Clear();
            reportHeader = null;
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

        private void ReportItemAdd()
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
            item.Text = new List<string>();

            ImageConverter _imageConverter = new ImageConverter();
            var img1 = zedGraphControl1.GetImage();
            byte[] xByte1 = (byte[])_imageConverter.ConvertTo(zedGraphControl1.GetImage(), typeof(byte[]));
            item.Image1 = xByte1;
            byte[] xByte2 = (byte[])_imageConverter.ConvertTo(zedGraphControl3.GetImage(), typeof(byte[]));
            item.Image2 = xByte2;

            item.WeightedSlopePerformance = orchestrator.WeightedSlopePerformance;
            item.AvgDiffPerformance = orchestrator.AvgDiffPerformance;
            item.AvgSlopePerformance = orchestrator.AvgSlopePerformance;

            reportItems.Add(item);
        }

        private void PrintReport()
        {
            List<ReportItem> sortedReportItems = reportItems.OrderBy(o => o.WeightedSlopePerformance).ToList();

            string reportFolder = ConfigurationManager.AppSettings["ReportFolder"];
            string filename = Path.Combine(reportFolder, "report_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf");

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
