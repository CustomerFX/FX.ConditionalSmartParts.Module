﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace FX.ConditionalSmartParts.Utility
{
    internal static class JsonParser
    {
        static Stack<List<string>> splitArrayPool = new Stack<List<string>>();
        static StringBuilder stringBuilder = new StringBuilder();
        static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> propertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        public static T FromJson<T>(this string json)
        {
            //Remove all whitespace not within strings to make parsing simpler
            stringBuilder.Length = 0;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (c == '\"')
                {
                    i = AppendUntilStringEnd(true, i, json);
                    continue;
                }
                if (char.IsWhiteSpace(c))
                    continue;

                stringBuilder.Append(c);
            }

            //Parse the thing!
            return (T)ParseValue(typeof(T), stringBuilder.ToString());
        }

        static int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json)
        {
            stringBuilder.Append(json[startIdx]);
            for (int i = startIdx + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    if (appendEscapeCharacter)
                        stringBuilder.Append(json[i]);
                    stringBuilder.Append(json[i + 1]);
                    i++;//Skip next character as it is escaped
                }
                else if (json[i] == '\"')
                {
                    stringBuilder.Append(json[i]);
                    return i;
                }
                else
                    stringBuilder.Append(json[i]);
            }
            return json.Length - 1;
        }

        //Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings
        static List<string> Split(string json)
        {
            List<string> splitArray = splitArrayPool.Count > 0 ? splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();
            int parseDepth = 0;
            stringBuilder.Length = 0;
            for (int i = 1; i < json.Length - 1; i++)
            {
                switch (json[i])
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '\"':
                        i = AppendUntilStringEnd(true, i, json);
                        continue;
                    case ',':
                    case ':':
                        if (parseDepth == 0)
                        {
                            splitArray.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }

                stringBuilder.Append(json[i]);
            }

            splitArray.Add(stringBuilder.ToString());

            return splitArray;
        }

        internal static object ParseValue(Type type, string json)
        {
            if (type == typeof(string))
            {
                if (json.Length <= 2)
                    return string.Empty;
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\\\", "\"\"").Replace("\\", string.Empty).Replace("\"\"", "\\");
            }
            if (type == typeof(int))
            {
                int result;
                int.TryParse(json, out result);
                return result;
            }
            if (type == typeof(float))
            {
                float result;
                float.TryParse(json, out result);
                return result;
            }
            if (type == typeof(double))
            {
                double result;
                double.TryParse(json, out result);
                return result;
            }
            if (type == typeof(bool))
            {
                return json.ToLower() == "true";
            }
            if (json == "null")
            {
                return null;
            }
            if (type.IsArray)
            {
                Type arrayType = type.GetElementType();
                if (json[0] != '[' || json[json.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json);
                Array newArray = Array.CreateInstance(arrayType, elems.Count);
                for (int i = 0; i < elems.Count; i++)
                    newArray.SetValue(ParseValue(arrayType, elems[i]), i);
                splitArrayPool.Push(elems);
                return newArray;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type listType = type.GetGenericArguments()[0];
                if (json[0] != '[' || json[json.Length - 1] != ']')
                    return null;

                List<string> elems = Split(json);
                var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });
                for (int i = 0; i < elems.Count; i++)
                    list.Add(ParseValue(listType, elems[i]));
                splitArrayPool.Push(elems);
                return list;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType, valueType;
                {
                    Type[] args = type.GetGenericArguments();
                    keyType = args[0];
                    valueType = args[1];
                }

                //Refuse to parse dictionary keys that aren't of type string
                if (keyType != typeof(string))
                    return null;
                //Must be a valid dictionary element
                if (json[0] != '{' || json[json.Length - 1] != '}')
                    return null;
                //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                    return null;

                var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });
                for (int i = 0; i < elems.Count; i += 2)
                {
                    if (elems[i].Length <= 2)
                        continue;
                    string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                    object val = ParseValue(valueType, elems[i + 1]);
                    dictionary.Add(keyValue, val);
                }
                return dictionary;
            }
            if (type == typeof(object))
            {
                return ParseAnonymousValue(json);
            }
            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                return ParseObject(type, json);
            }

            return null;
        }

        static object ParseAnonymousValue(string json)
        {
            if (json.Length == 0)
                return null;
            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                List<string> elems = Split(json);
                if (elems.Count % 2 != 0)
                    return null;
                var dict = new Dictionary<string, object>(elems.Count / 2);
                for (int i = 0; i < elems.Count; i += 2)
                    dict.Add(elems[i].Substring(1, elems[i].Length - 2), ParseAnonymousValue(elems[i + 1]));
                return dict;
            }
            if (json[0] == '[' && json[json.Length - 1] == ']')
            {
                List<string> items = Split(json);
                var finalList = new List<object>(items.Count);
                for (int i = 0; i < items.Count; i++)
                    finalList.Add(ParseAnonymousValue(items[i]));
                return finalList;
            }
            if (json[0] == '\"' && json[json.Length - 1] == '\"')
            {
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\", string.Empty);
            }
            if (char.IsDigit(json[0]) || json[0] == '-')
            {
                if (json.Contains("."))
                {
                    double result;
                    double.TryParse(json, out result);
                    return result;
                }
                else
                {
                    int result;
                    int.TryParse(json, out result);
                    return result;
                }
            }
            if (json == "true")
                return true;
            if (json == "false")
                return false;
            // handles json == "null" as well as invalid JSON
            return null;
        }

        static object ParseObject(Type type, string json)
        {
            object instance = FormatterServices.GetUninitializedObject(type);

            //The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            List<string> elems = Split(json);
            if (elems.Count % 2 != 0)
                return instance;

            Dictionary<string, FieldInfo> nameToField;
            Dictionary<string, PropertyInfo> nameToProperty;
            if (!fieldInfoCache.TryGetValue(type, out nameToField))
            {
                nameToField = type.GetFields().Where(field => field.IsPublic).ToDictionary(field => field.Name);
                fieldInfoCache.Add(type, nameToField);
            }
            if (!propertyInfoCache.TryGetValue(type, out nameToProperty))
            {
                nameToProperty = type.GetProperties().ToDictionary(p => p.Name);
                propertyInfoCache.Add(type, nameToProperty);
            }

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                    continue;
                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                FieldInfo fieldInfo;
                PropertyInfo propertyInfo;
                if (nameToField.TryGetValue(key, out fieldInfo))
                    fieldInfo.SetValue(instance, ParseValue(fieldInfo.FieldType, value));
                else if (nameToProperty.TryGetValue(key, out propertyInfo))
                    propertyInfo.SetValue(instance, ParseValue(propertyInfo.PropertyType, value), null);
            }

            return instance;
        }
    }
}