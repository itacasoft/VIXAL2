using System.Threading.Tasks;

namespace VIXAL2.Retrieve.Base
{
    internal abstract class BaseManager
    {
        protected string dataFolder;
        public BaseManager(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }

        public abstract Task Run();
    }
}
