using System.Collections;
using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using Newtonsoft.Json;
using LUIZ.Security;
using System;
using System.Threading.Tasks;

namespace LUIZ
{
    //PlayerPref를 사용하게 되면 Key가 무분별하게 늘어날수있어서 클래스로 묶은 후 문자열로 저장하기 위해 제작됨

    //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.SetString.html
    //사실 유니티 공식 권장 2KB 이상은 playerPref 쓰지말것.
    //데이터 용량이 크면 LocalSaveLoaderJsonBase를 이용하도록 하자 
    public abstract class LocalSaveLoaderPlayerPrefBase<TData> : LocalSaveLoaderBase where TData : class, ILocalDataLoadable
    {
        private const string c_defaultValue = "NNN";

        [SerializeField] private bool EncryptData = false;

        //------------------------------------------------------------
        protected Task<TData> ProtLoadLocalData(string key)
        {
            if (EncryptData)
            {
                key = SimpleEncrypter.DoMakeHashSimple(key);
            }
            
            TData loadData = null;
            string jsonData = GetString(key, c_defaultValue);

            if (jsonData != c_defaultValue)//정보 있음
            {
                loadData = JsonConvert.DeserializeObject<TData>(jsonData);
                loadData?.OnDataLoadFinish();
            }
            else
            {
                //정보 없음
                OnSaveLoaderPlayerPrefDataNone();
            }
            
            return Task.FromResult(loadData);
        }
        
        protected Task ProtSaveLocalData(string key, TData saveData)
        {
            if (EncryptData)
            {
                key = SimpleEncrypter.DoMakeHashSimple(key);
            }
            
            string jsonData = JsonConvert.SerializeObject(saveData);
            SetString(key, jsonData);
            
            return Task.CompletedTask;
        }

        //------------------------------------------------------------
        protected void DeleteKey(string key)
        {
            if (EncryptData)
            {
                key = SimpleEncrypter.DoMakeHashSimple(key);
            }

            PlayerPrefs.DeleteKey(key);
        }

        protected void SetInt(string key, int value) => PrivSetData(key, value);
        protected void SetLong(string key, long value) => PrivSetData(key, value);
        protected void SetFloat(string key, float value) => PrivSetData(key, value);
        protected void SetString(string key, string value) => PrivSetData(key, value);

        protected int GetInt(string key, int defaultValue)
        {
            int result = defaultValue;
            string originalValue = PrivGetStringValue(key);

            if (string.IsNullOrEmpty(originalValue) == true)
                return defaultValue;

            if (int.TryParse(originalValue, out result) == false)
                return defaultValue;

            return result;
        }
        protected long GetLong(string key, long defaultValue)
        {
            long result = defaultValue;
            string originalValue = PrivGetStringValue(key);

            if (string.IsNullOrEmpty(originalValue) == true)
                return defaultValue;

            if (long.TryParse(originalValue, out result) == false)
                return defaultValue;

            return result;
        }
        protected float GetFloat(string key, float defaultValue)
        {
            float result = defaultValue;
            string originalValue = PrivGetStringValue(key);

            if (string.IsNullOrEmpty(originalValue) == true)
                return defaultValue;

            if (float.TryParse(originalValue, out result) == false)
                return defaultValue;

            return result;
        }
        protected string GetString(string key, string defaultValue)
        {
            string result = defaultValue;
            string originalValue = PrivGetStringValue(key);

            if (string.IsNullOrEmpty(originalValue) == false)
                result = originalValue;

            return result;
        }

        //------------------------------------------------------------
        private string PrivGetStringValue(string key)
        {
            string originalValue = null;

            if (EncryptData)
            {
                key = SimpleEncrypter.DoMakeHashSimple(key);

                string encryptedValue = PlayerPrefs.GetString(key);
                originalValue = SimpleEncrypter.DoDecryptAdv(encryptedValue);
            }
            else
            {
                originalValue = PlayerPrefs.GetString(key);
            }

            return originalValue;
        }

        private void PrivSetData<T>(string key, T value) where T : IConvertible
        {
            if (EncryptData)
            {
                key = SimpleEncrypter.DoMakeHashSimple(key);
                string encrytedVal = SimpleEncrypter.DoEncryptAdv(value.ToString());

                PrivSetPlayerPref(key, encrytedVal);
            }
            else
            {
                PrivSetPlayerPref(key, value.ToString());
            }
        }

        private void PrivSetPlayerPref(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        //-----------------------------------------------------------------
        protected virtual void OnSaveLoaderPlayerPrefDataNone() { }
    }
}
