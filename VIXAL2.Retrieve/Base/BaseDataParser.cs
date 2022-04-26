using VIXAL2.Retrieve.Base;
using SharpML.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Retrieve
{
    internal abstract class BaseDataParser
    {
        protected string dataFolder;
        public BaseDataParser(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }

        public abstract StockList Parse();
    }
}
