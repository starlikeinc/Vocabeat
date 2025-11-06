using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace LUIZ
{
    public abstract class TableDataLoaderJsonBase<TData> : TableDataLoaderBase where TData : class
    {
        //-----------------------------------------------------------------------
        protected sealed override void OnTableDataRead(string tableData)
        {
            base.OnTableDataRead(tableData);
            
            TData tableBuffData = PrivTableDataLoadJson(tableData);
            OnTableDataJsonLoad(tableBuffData);
        }
        
        protected sealed override void OnTableDataReadFinish()
        {
            base.OnTableDataReadFinish();
            OnTableDataJsonLoadComplete();
        }
        
        //-----------------------------------------------------------------------
        private TData PrivTableDataLoadJson(string strTextData)
        {
            return JsonConvert.DeserializeObject<TData>(strTextData, new JsonConverterGenericCustom());
        }
        
        //-----------------------------------------------------------------------
        protected virtual void OnTableDataJsonLoad(TData loadedData){ }
        protected virtual void OnTableDataJsonLoadComplete(){ }
    }
}