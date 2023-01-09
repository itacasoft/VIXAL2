using System.ComponentModel;

namespace VIXAL2.Data.Base
{
    public enum DataSetType
    {
        [Description("Normal")]
        Normal = 1,
        [Description("Moving Avg")]
        MovingAverage = 2,
        [Description("RSI")]
        RSI = 3,
        [Description("Enh Moving Avg")]
        Enh_MovingAverage = 4,
        [Description("Enh Moving Avg 2")]
        Enh2_MovingAverage = 5
    }
}
