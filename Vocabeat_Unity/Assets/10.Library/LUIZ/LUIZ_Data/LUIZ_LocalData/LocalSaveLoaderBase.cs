using System.Threading.Tasks;
using UnityEngine;

namespace LUIZ
{
    public abstract class LocalSaveLoaderBase : MonoBase, IDataLoader, IDataSaver
    {
        [SerializeField] private bool LoadOnAwake = true;

        //-----------------------------------------------
        public bool IsAutoLoad => LoadOnAwake;

        //-----------------------------------------------
        public abstract Task DoTaskLoadData();
        public abstract Task DoTaskSaveData();
    }
}
