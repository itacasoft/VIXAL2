using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VIXAL2.Retrieve
{
    internal abstract class BaseDataRetriever
    {
        protected string dataFolder;

        public BaseDataRetriever(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }

        public abstract Task Download(string symbol, DateTime dateFrom, DateTime dateTo);
    }
}
