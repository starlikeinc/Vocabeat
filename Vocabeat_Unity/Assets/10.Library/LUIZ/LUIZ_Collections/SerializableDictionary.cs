using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace LUIZ.Collections
{
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializedDictionaryKVPs<TKey, TValue>> DictionaryList = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            foreach (KeyValuePair<TKey,TValue> keyValPair in this)
            {
                if (DictionaryList.FirstOrDefault(value => this.Comparer.Equals(value.Key, keyValPair.Key))
                    is SerializedDictionaryKVPs<TKey, TValue> serializedKVP)
                {
                    serializedKVP.Value = keyValPair.Value;
                }
                else
                {
                    DictionaryList.Add(keyValPair);
                }
            }

            DictionaryList.RemoveAll(value => ContainsKey(value.Key) == false);

            for (int i = 0; i < DictionaryList.Count; i++)
            {
                DictionaryList[i].index = i;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            DictionaryList.RemoveAll(r => r.Key == null);

            foreach (var serializedKVP in DictionaryList)
            {
                if (!(serializedKVP.isKeyDuplicated = ContainsKey(serializedKVP.Key)))
                {
                    Add(serializedKVP.Key, serializedKVP.Value);
                }
            }
        }

        public new TValue this[TKey key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    var duplicateKeysWithCount = DictionaryList.GroupBy(item => item.Key)
                                                               .Where(group => group.Count() > 1)
                                                               .Select(group => new { Key = group.Key, Count = group.Count() });

                    foreach (var duplicatedKey in duplicateKeysWithCount)
                    {
                        Debug.LogError($"Key '{duplicatedKey.Key}' is duplicated {duplicatedKey.Count} times in the dictionary.");
                    }

                    return base[key];
                }
                else
                {
                    Debug.LogError($"Key '{key}' not found in dictionary.");
                    return default(TValue);
                }
            }
        }

        [System.Serializable]
        public class SerializedDictionaryKVPs<TypeKey, TypeValue>
        {
            public TypeKey Key;
            public TypeValue Value;

            public int index;
            public bool isKeyDuplicated;

            public SerializedDictionaryKVPs(TypeKey key, TypeValue value) { this.Key = key; this.Value = value; }

            public static implicit operator SerializedDictionaryKVPs<TypeKey, TypeValue>(KeyValuePair<TypeKey, TypeValue> kvp)
                => new SerializedDictionaryKVPs<TypeKey, TypeValue>(kvp.Key, kvp.Value);
            public static implicit operator KeyValuePair<TypeKey, TypeValue>(SerializedDictionaryKVPs<TypeKey, TypeValue> kvp)
                => new KeyValuePair<TypeKey, TypeValue>(kvp.Key, kvp.Value);
        }
    }
}