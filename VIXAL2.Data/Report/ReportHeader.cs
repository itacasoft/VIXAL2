using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Data.Report
{
    public class ReportHeader
    {
        public string Title;
        public List<string> Text = new List<string>();

        public override string ToString()
        {
            string res = Title + Environment.NewLine;
            for (int i = 0; i < Text.Count; i++)
            {
                res += Text[i];
                res += ", ";
            }
            return res;
        }
    }

}
