using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ
{
    public abstract class TableDataLoaderXMLBase : TableDataLoaderBase
    {
        //TODO 작업해야함..
        //-----------------------------------------------------------------------
        protected sealed override void OnTableDataRead(string tableData)
        {
            base.OnTableDataRead(tableData);
        }

        protected sealed override void OnTableDataReadFinish()
        {
            base.OnTableDataReadFinish();
        }
    }
}
