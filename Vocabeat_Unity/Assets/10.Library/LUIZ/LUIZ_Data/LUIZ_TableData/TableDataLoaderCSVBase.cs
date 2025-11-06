using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LUIZ
{
    public abstract class TableDataLoaderCSVBase<TData> : TableDataLoaderBase where TData : class, new()
    {
        private static readonly string c_SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        private static readonly string c_LINE_SPLIT_RE = @"\r\n|\n\r";
        private static readonly char[] c_TRIM_CHARS = { '\"' };

        private List<Dictionary<string, List<string>>> m_listTableData = new List<Dictionary<string, List<string>>>();

        //-----------------------------------------------------------------------
        protected sealed override void OnTableDataRead(string tableData)
        {
            base.OnTableDataRead(tableData);
            m_listTableData.AddRange(Read(tableData));
        }

        protected sealed override void OnTableDataReadFinish()
        {
            base.OnTableDataReadFinish();
            OnTableDataCSVLoadComplete();
        }

        //-----------------------------------------------------------------------
        protected List<TData> ProtTableDataLoadCSV()
        {
            List<TData> listInstance = new List<TData>();

            for (int i = 0; i < m_listTableData.Count; i++)
            {
                TData instance = new TData();

                Dictionary<string, List<string>>.Enumerator it = m_listTableData[i].GetEnumerator();
                while (it.MoveNext())
                {
                    List<string> pListValue = it.Current.Value;
                    for (int k = 0; k < pListValue.Count; k++)
                    {
                        DoTableDataReadField<TData>(instance, it.Current.Key, pListValue[k]);
                    }
                }
                listInstance.Add(instance);
            }

            return listInstance;
        }

        //-----------------------------------------------------------------------
        private List<Dictionary<string, List<string>>> Read(string textData)
        {
            List<Dictionary<string, List<string>>> list = new List<Dictionary<string, List<string>>>();

            string[] aryLines = Regex.Split(textData, c_LINE_SPLIT_RE);

            if (aryLines.Length <= 1) return list;

            string[] aryHeader = Regex.Split(aryLines[0], c_SPLIT_RE);

            for (int i = 1; i < aryLines.Length; i++)
            {
                string[] aryValues = Regex.Split(aryLines[i], c_SPLIT_RE);

                Dictionary<string, List<string>> dicEntry = new Dictionary<string, List<string>>();
                for (int j = 0; j < aryHeader.Length && j < aryValues.Length; j++)
                {
                    string strValue = aryValues[j];
                    string headerName = aryHeader[j];
                    strValue = strValue.TrimStart(c_TRIM_CHARS).TrimEnd(c_TRIM_CHARS);

                    List<string> listValue = null;
                    if (dicEntry.ContainsKey(headerName))
                    {
                        listValue = dicEntry[headerName];
                    }
                    else
                    {
                        listValue = new List<string>();
                        dicEntry[headerName] = listValue;
                    }

                    listValue.Add(strValue);
                }
                list.Add(dicEntry);
            }
            return list;
        }

        //-----------------------------------------------------------------------
        protected virtual void OnTableDataCSVLoadComplete() { }
    }
}