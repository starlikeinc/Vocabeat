using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Reflection;

namespace LUIZ
{
    public class JsonConverterGenericCustom : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            object newObject = Activator.CreateInstance(objectType);

            foreach (JContainer jContainer in jObject.Children())
            {
                foreach (JToken jToken in jContainer.Children())
                {
                    PrivJsonConverterRecursive(jToken, newObject);
                }
            }

            return newObject;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //TODO
            /*        JToken jToken = JToken.FromObject(value);

                    if(jToken is JObject)
                    {
                        JObject obj = (JObject)jToken;
                    }
            */
        }

        //-------------------------------------------------------------------------------------
        private void PrivJsonConverterRecursive(JToken jToken, object instance)
        {
            if (jToken is JObject)
            {
                PrivJsonConverter(jToken as JObject, instance);
            }
            else if (jToken is JArray)
            {
                PrivJsonConverter(jToken as JArray, instance);
            }
        }

        private void PrivJsonConverter(JArray jArray, object instance)
        {
            Type instanceType = instance.GetType();
            JProperty parentContainer = jArray.Parent as JProperty;
            FieldInfo fieldInfo = instanceType.GetField(parentContainer.Name);
            if (fieldInfo == null) return;

            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsGenericType == false || fieldType.GetGenericTypeDefinition() != typeof(List<>)) return;

            IList listType = fieldInfo.GetValue(instance) as IList;
            Type[] aryGenericType = fieldType.GetGenericArguments();

            for (int i = 0; i < jArray.Count; i++)
            {
                object arrayItem = Activator.CreateInstance(aryGenericType[0]);
                listType.Add(arrayItem);
                PrivJsonConverterRecursive(jArray[i], arrayItem);
            }
        }

        private void PrivJsonConverter(JObject jObject, object instance)
        {
            foreach (JToken jToken in jObject.Children())
            {
                if (jToken.Type == JTokenType.Property)
                {
                    PrivJsonConverterReadValueProperty(jToken as JProperty, instance);
                }
            }
        }

        private void PrivJsonConverterReadValueProperty(JProperty property, object instance)
        {
            Type intanceType = instance.GetType();
            FieldInfo fieldInfo = intanceType.GetField(property.Name.Replace('[', '_').Replace("]", string.Empty));

            if (fieldInfo == null)
            {
                //Error! 해당 이름의 맴버 변수가 없다 (파싱 오류)
                return;
            }

            foreach (JToken jToken in property.Values())
            {
                if (jToken.Type == JTokenType.Integer)
                {
                    long value = jToken.Value<long>();

                    if (value >= int.MinValue && value <= int.MaxValue)
                    {
                        PrivJsonConverterReadValueInteger(jToken, fieldInfo, instance);
                    }
                    else
                    {
                        PrivJsonConverterReadValueLong(jToken, fieldInfo, instance);
                    }
                }
                else if (jToken.Type == JTokenType.Boolean)
                {
                    PrivJsonConverterReadValueBoolean(jToken, fieldInfo, instance);
                }
                else if (jToken.Type == JTokenType.String)
                {
                    PrivJsonConverterReadValueString(jToken, fieldInfo, instance);
                }
                else if (jToken.Type == JTokenType.Object)
                {
                    PrivJsonConverterReadValueObject(jToken as JObject, fieldInfo, instance);
                }
            }
        }

        private void PrivJsonConverterReadValueGeneric<TValue>(TValue value, FieldInfo fieldInfo, object instance)
        {
            if (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] aryGenericType = fieldInfo.FieldType.GetGenericArguments();
                if (aryGenericType.Length > 0 && aryGenericType[0] == typeof(TValue))
                {
                    IList listType = fieldInfo.GetValue(instance) as IList;
                    if (listType == null)
                    {
                        //Error 해당 변수가 존재하지 않음 (파싱 오류)
                        return;
                    }
                    listType.Add(value);
                }
            }
            else
            {
                //Error List이외의 자료구조는 허용하지 않음
            }
        }

        private void PrivJsonConverterReadValueInteger(JToken readToken, FieldInfo fieldInfo, object instance)
        {
            int value = readToken.Value<int>();

            if (fieldInfo.FieldType.IsGenericType)
            {
                PrivJsonConverterReadValueGeneric(value, fieldInfo, instance);
            }
            else if (fieldInfo.FieldType == typeof(int))
            {
                fieldInfo.SetValue(instance, value);
            }
        }

        private void PrivJsonConverterReadValueLong(JToken readToken, FieldInfo fieldInfo, object instance)
        {
            long value = readToken.Value<long>();

            if (fieldInfo.FieldType.IsGenericType)
            {
                PrivJsonConverterReadValueGeneric(value, fieldInfo, instance);
            }
            else if (fieldInfo.FieldType == typeof(long))
            {
                fieldInfo.SetValue(instance, value);
            }
        }

        private void PrivJsonConverterReadValueBoolean(JToken readToken, FieldInfo fieldInfo, object instance)
        {
            bool value = readToken.Value<bool>();

            if (fieldInfo.FieldType.IsGenericType)
            {
                PrivJsonConverterReadValueGeneric(value, fieldInfo, instance);
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                fieldInfo.SetValue(instance, value);
            }
        }

        private void PrivJsonConverterReadValueString(JToken readToken, FieldInfo fieldInfo, object instance)
        {
            string value = readToken.Value<string>();
            if (fieldInfo.FieldType.IsGenericType)
            {
                PrivJsonConverterReadValueGeneric(value, fieldInfo, instance);
            }
            else if (fieldInfo.FieldType == typeof(string))
            {
                fieldInfo.SetValue(instance, value);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                value = value.Replace(' ', '_');
                Type fieldType = fieldInfo.FieldType;
                object enumObject = Enum.Parse(fieldType, value);
                fieldInfo.SetValue(instance, enumObject);
            }
        }

        private void PrivJsonConverterReadValueObject(JObject readToken, FieldInfo fieldInfo, object instance)
        {
            if (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList listType = fieldInfo.GetValue(instance) as IList;
                Type[] aryGenericType = fieldInfo.FieldType.GetGenericArguments();
                if (aryGenericType.Length > 0)
                {
                    object newChild = Activator.CreateInstance(aryGenericType[0]);
                    listType.Add(newChild);
                    foreach (JProperty jProperty in readToken.Children())
                    {
                        PrivJsonConverterReadValueProperty(jProperty, newChild);
                    }
                }
            }
        }
    }
}
