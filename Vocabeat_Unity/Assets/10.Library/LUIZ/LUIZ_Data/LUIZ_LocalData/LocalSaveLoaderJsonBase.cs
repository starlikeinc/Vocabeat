using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using LUIZ.Security;

namespace LUIZ
{
    //persistantDataPath에 클래스를 json으로 직렬화 한 후 바이너리 포멧으로 변환하여 저장.
    //json을 암호화 하여 저장하고 싶다면 EncryptData 를 true로 할 것

    //TODO : 코드 정리
    public abstract class LocalSaveLoaderJsonBase<TData> : LocalSaveLoaderBase where TData : class, ILocalDataLoadable
    {
        [SerializeField] private bool EncryptData = false;

        //-----------------------------------------------------------------
        protected Task<TData> ProtLoadLocalData(string key)
        {
            //PrivCheckIOS();
            return PrivLoadLocalData(key);
        }

        protected Task ProtSaveLocalData(string key, TData saveData)
        {
            return PrivSaveLocalData(key,saveData);
        }

        //-----------------------------------------------------------------
        private async Task<TData> PrivLoadLocalData(string key)
        {
            TData loadData = default(TData);

            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("Load operation failed: Key is null or whitespace.");
                return null;
            }

            try
            {
                string filePath;
                string decryptedValue = string.Empty;

                if (EncryptData)
                {
                    string encryptedKey = SimpleEncrypter.DoMakeHashSimple(key);

                    filePath = PrivGetFilePath(encryptedKey);
                    if (File.Exists(filePath))
                    {
                        string encryptedValue;
                        
                        await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
                        using (var reader = new StreamReader(stream))
                        { 
                            encryptedValue = await reader.ReadToEndAsync();
                        }
                        decryptedValue = SimpleEncrypter.DoDecryptAdv(encryptedValue);
                    }
                }
                else
                {
                    filePath = PrivGetFilePath(key);

                    if (File.Exists(filePath))
                    {
                        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                        using (var reader = new StreamReader(stream))
                        { 
                            decryptedValue = await reader.ReadToEndAsync();
                        }
                    }
                }

                if (string.IsNullOrEmpty(decryptedValue))
                {
                    OnSaveLoaderJsonDataNone();
                }
                else
                {
                    loadData = JsonConvert.DeserializeObject<TData>(decryptedValue);
                }
                
                loadData?.OnDataLoadFinish();
                return loadData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error when loading data: {ex.Message}");
                return loadData;
            }
        }

        private async Task PrivSaveLocalData(string key, TData saveData)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError("Save operation failed: Key is null or whitespace.");
            }

            if (saveData == null)
            {
                Debug.LogError("Save operation failed: Data is null.");
            }

            try
            {
                string fileSavePath;
                string directoryPath;
                string jsonData = JsonConvert.SerializeObject(saveData, Formatting.Indented);

                if (EncryptData)
                {
                    string encryptedVal = SimpleEncrypter.DoMakeHashSimple(key);
                    fileSavePath = PrivGetFilePath(encryptedVal);
                }
                else
                {
                    fileSavePath = PrivGetFilePath(key);
                }

                directoryPath = Path.GetDirectoryName(fileSavePath);
                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath ?? throw new InvalidOperationException("Directory path is null."));
                }

                if (EncryptData)
                {
                    string encryptedJson = SimpleEncrypter.DoEncryptAdv(jsonData);

                    await using (var stream = new FileStream(fileSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                    await using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(encryptedJson);
                    }
                }
                else
                {
                    await using (var stream = new FileStream(fileSavePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                    await using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(jsonData);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error when saving data: {ex.Message}");
            }
        }

        private string PrivGetFilePath(string key)
        {
            return Path.Combine(Application.persistentDataPath, $"{key}");
        }

        /*private void PrivCheckIOS()
        {
            // 주의!  IOS 경우 아래 변수가 없으면 BinaryFormatter 컴파일 오류가 뜬다.
            if (m_isIOSChecked == false)
            {
                m_isIOSChecked = true;
#if UNITY_IOS
                Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
            }
        }*/

        //-----------------------------------------------------------------
        /// <summary> 로컬에 데이터가 없을 때 호출 </summary>
        protected virtual void OnSaveLoaderJsonDataNone() { }
    }
}
