using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LUIZ
{
    //테이블 매니저와 같은 오브젝트에 붙여두면 매니저 Initialize 시 자동으로 로드한다. (메모리에 상주하는 데이터는 이 방식으로 처리할 것)

    //[주의!!] TextTableData는 매니저 번들과 같이 묶여 있기 때문에 에셋을 메모리에서 내릴 수가 없다. 따라서 큰 데이터는 이런식으로 올려두지 말것.
    //데이터가 큰 경우 개별 번들로 읽어와서 수동 로드해주도록 한다.
    
    public abstract class TableDataLoaderBase : MonoBase, IDataLoader
    {
        private static StringBuilder c_StringBuilder = new StringBuilder();

        private const byte c_UTFBom1 = 0xEF;
        private const byte c_UTFBom2 = 0xBB;
        private const byte c_UTFBom3 = 0xBF;

        [SerializeField] private List<TextAsset> TextTableData = new List<TextAsset>();

        [SerializeField] private bool Kor949 = false;

        [SerializeField] private bool LoadOnAwake = true;

        //-----------------------------------------------
        public bool IsAutoLoad => LoadOnAwake;

        //------------------------------------------------------------------
        /// <summary>
        /// string 데이터를 instance 내부의 변수명이 valueName인 변수에 value 값을 적용하여 넣어준다. 타입은 자동으로 확인하여 적용함.
        /// </summary>
        public static void DoTableDataReadField<T>(T instance, string valueName, string value) where T : class
        {
            PrivTableDataReadField<T>(instance, valueName, value);
        }

        public Task DoTaskLoadData()
        {
            return PrivReadTableDataInList(TextTableData);
        }

        public Task DoTaskLoadData(string data)
        {
            OnTableDataRead(data);
            OnTableDataReadFinish();
            
            return Task.CompletedTask;
        }
        
        public Task DoTaskLoadData(TextAsset data)
        {
            string tableData = PrivConvertUTF8(data.bytes);
            
            OnTableDataRead(tableData);
            OnTableDataReadFinish();
            
            return Task.CompletedTask;
        }
        
        public Task DoTaskLoadData(List<TextAsset> listTextAsset)
        {
            return PrivReadTableDataInList(listTextAsset);
        }

        //------------------------------------------------------------------
        private Task PrivReadTableDataInList(List<TextAsset> listTextAsset)
        {
            for (int i = 0; i < listTextAsset.Count; i++)
            {
                string tableData = PrivConvertUTF8(listTextAsset[i].bytes);
                OnTableDataRead(tableData);
            }

            OnTableDataReadFinish();
            //로드가 끝나면 인스펙터에서 내림
            listTextAsset.Clear();
            
            return Task.CompletedTask;
        }

        private string PrivConvertUTF8(byte[] aryTextData)
        {
            string resultText;

            if (Kor949 == true)
            {
                //멀티 바이트일 경우
                aryTextData = Encoding.Convert(Encoding.GetEncoding("euc-kr"), Encoding.UTF8, aryTextData);
                resultText = Encoding.UTF8.GetString(aryTextData);
            }
            else if (aryTextData.Length > 3 && aryTextData[0] == c_UTFBom1 && aryTextData[1] == c_UTFBom2 && aryTextData[2] == c_UTFBom3)
            {
                //유니코드 UTF-8 Bom (엑셀 익스포트 기본)
                resultText = Encoding.UTF8.GetString(aryTextData, 3, aryTextData.Length - 3);
            }
            else
            {
                //이외의 표준 UTF - 8
                resultText = Encoding.UTF8.GetString(aryTextData);
            }

            return resultText;
        }

        //------------------------------------------------------------------
        protected virtual void OnTableDataRead(string tableData) { }
        protected virtual void OnTableDataReadFinish() { }

        //------------------------------------------------------------------
        private static void PrivTableDataReadField<T>(T instance, string valueName, string value) where T : class
        {
            //런타임 type을 가져와 value 값을 적용
            Type classType = instance.GetType();
            FieldInfo fieldInfo = classType.GetField(valueName);

            if (fieldInfo == null)
                return;
            if (value == null)
                return;

            if (value == string.Empty)
            {
                if (fieldInfo.FieldType == typeof(string))
                {
                    fieldInfo.SetValue(instance, string.Empty);
                }
                return;
            }

            if (fieldInfo.FieldType == typeof(int))
            {
                fieldInfo.SetValue(instance, int.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(uint))
            {
                fieldInfo.SetValue(instance, uint.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(string))
            {
                fieldInfo.SetValue(instance, value.Trim());
            }
            else if (fieldInfo.FieldType == typeof(float))
            {
                fieldInfo.SetValue(instance, float.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                fieldInfo.SetValue(instance, bool.Parse(value));
            }
            else if (fieldInfo.FieldType == typeof(Enum))
            {
                fieldInfo.SetValue(instance, Enum.Parse(fieldInfo.FieldType, value));
            }
            else if (fieldInfo.FieldType == typeof(List<int>))
            {
                List<string> listSeperate = PrivTableDataSeperateComma(value);
                List<int> listValue = fieldInfo.GetValue(instance) as List<int>;
                for (int i = 0; i < listSeperate.Count; i++)
                {
                    listValue.Add(int.Parse(listSeperate[i]));
                }
            }
            else if (fieldInfo.FieldType == typeof(List<uint>))
            {
                List<string> listSeperate = PrivTableDataSeperateComma(value);
                List<uint> listValue = fieldInfo.GetValue(instance) as List<uint>;
                for (int i = 0; i < listSeperate.Count; i++)
                {
                    listValue.Add(uint.Parse(listSeperate[i]));
                }
            }
            else if (fieldInfo.FieldType == typeof(List<float>))
            {
                List<string> listSeperate = PrivTableDataSeperateComma(value);
                List<float> listValue = fieldInfo.GetValue(instance) as List<float>;
                for (int i = 0; i < listSeperate.Count; i++)
                {
                    listValue.Add(float.Parse(listSeperate[i]));
                }
            }
            else if (fieldInfo.FieldType == typeof(List<string>))
            {
                List<string> listSeperate = PrivTableDataSeperateComma(value);
                List<string> listValue = fieldInfo.GetValue(instance) as List<string>;
                for (int i = 0; i < listSeperate.Count; i++)
                {
                    listValue.Add(listSeperate[i].Trim());
                }
            }
        }

        private static List<string> PrivTableDataSeperateComma(string data)
        {
            c_StringBuilder.Clear();

            List<string> listResult = new List<string>();

            for (int i = 0; i < data.Length; i++)
            {
                char cha = data[i];
                if (cha == ',')
                {
                    listResult.Add(c_StringBuilder.ToString().Trim());
                }
                else
                {
                    c_StringBuilder.Append(cha);
                }
            }

            if (c_StringBuilder.Length > 0)
            {
                listResult.Add(c_StringBuilder.ToString().Trim());
            }

            return listResult;
        }
    }
}
