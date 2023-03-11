using System;
using System.Xml.Serialization;

namespace VIXAL2.Data.Report
{
    public class ReportItem
    {
        public string StockName;
        public DateTime TimeOfSimulation;
        //public List<string> Text;
        [XmlIgnoreAttribute]
        public byte[] Image1;
        [XmlIgnoreAttribute]
        public byte[] Image2;
        public double WeightedSlopePerformance;
        public double AvgSlopePerformance;
        public double AvgDiffPerformance;
        public double FinTrade_GainPerc;
        public double FinTrade_Gain;
        public int FinTrade_GoodTrades;
        public int FinTrade_BadTrades;
        public double FinTradeComm_GainPerc;
        public double FinTradeComm_Gain;
        public int FinTradeComm_GoodTrades;
        public int FinTradeComm_BadTrades;
    }
}
