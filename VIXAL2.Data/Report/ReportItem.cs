using System;
using System.Xml.Serialization;

namespace VIXAL2.Data.Report
{
    public class ReportItem
    {
        public string StockName { get; set; }
        public DateTime TimeOfSimulation { get; set; }
        //public List<string> Text;
        [XmlIgnoreAttribute]
        public byte[] Image1;
        [XmlIgnoreAttribute]
        public byte[] Image2;
        public double WeightedSlopePerformance { get; set; }
        public double AvgSlopePerformance { get; set; }
        public double AvgDiffPerformance { get; set; }
        public double FinTrade_GainPerc { get; set; }
        public double FinTrade_Gain { get; set; }
        public int FinTrade_GoodTrades { get; set; }
        public int FinTrade_BadTrades { get; set; }
        public double FinTradeComm_GainPerc { get; set; }
        public double FinTradeComm_Gain { get; set; }
        public int FinTradeComm_GoodTrades { get; set; }
        public int FinTradeComm_BadTrades { get; set; }
    }
}
