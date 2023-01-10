using System;
using System.Collections.Generic;
using System.Drawing;
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
            public List<string> Text;
            public byte[] Image1;
            public byte[] Image2;
            public byte[] Image;
        }

        private List<ReportItem> reportItems = new List<ReportItem>();
        private ReportHeader reportHeader;

        private void ReportClear()
        {
            reportItems.Clear();
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
//            img.Save("c:\\temp\\pippo.jpg");
            return img;
        }

        private void ReportItemAdd()
        {
            if (reportHeader == null)
            {
                reportHeader = new ReportHeader();
                reportHeader.Title = "Report of " + DateTime.Now.ToString("dd / MM / yyyy HH: mm");
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
            item.Text = new List<string>();

            ImageConverter _imageConverter = new ImageConverter();
            byte[] xByte1 = (byte[])_imageConverter.ConvertTo(zedGraphControl1.GetImage(), typeof(byte[]));
            item.Image1 = xByte1;
            byte[] xByte2 = (byte[])_imageConverter.ConvertTo(zedGraphControl3.GetImage(), typeof(byte[]));
            item.Image2 = xByte2;

            Image image = MergeImages(zedGraphControl1.GetImage(), zedGraphControl3.GetImage(), 10);
            item.Image = (byte[])_imageConverter.ConvertTo(image, typeof(byte[]));

            reportItems.Add(item);
        }

        private void PrintReport()
        {
            string filename = "..\\..\\..\\Analysis\\report_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".pdf";

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
                        column.Item().Text(reportHeader.Title).SemiBold().FontSize(14).FontColor(Colors.Blue.Medium); ;
                        for (int i=0; i<reportHeader.Text.Count; i++)
                            column.Item().ShowOnce().Text(reportHeader.Text[i]);

                        column.Item().ShowOnce().Image(Placeholders.Image(200, 200));
                    });


                    page.Content()
                        .PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre)
                        .Column(x =>
                        {
                            for (int i = 0; i < reportItems.Count; i++)
                            {
                                x.Spacing(20);

                                x.Item().Text("Stock name: " + reportItems[i].StockName);

                                x.Spacing(10);

                                x.Item().Image(reportItems[i].Image1);

                                x.Spacing(10);
                                x.Item().Image(reportItems[i].Image2);

                                x.Item().PageBreak();
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
