using System.Threading.Tasks;

namespace LUIZ
{
    public interface IDataLoader
    {
        public bool IsAutoLoad {  get; }
        public Task DoTaskLoadData();
    }
    public interface IDataSaver
    {
        public Task DoTaskSaveData();
    }
}
