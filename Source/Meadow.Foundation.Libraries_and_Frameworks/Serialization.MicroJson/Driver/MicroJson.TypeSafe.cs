using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Serialization;

public static partial class MicroJson
{
    /// <summary>
    /// Deserializes a JSON array into a list of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="array">The JSON array to deserialize.</param>
    /// <returns>A list of objects of type T.</returns>
    public static List<T> DeserializeList<T>(ArrayList array)
            where T : new()
    {
        var result = new List<T>(array.Count);
        DeserializeList(array, typeof(T), ref result);
        return result;
    }

    /// <summary>
    /// Deserializes a JSON array into a list of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects in the list.</typeparam>
    /// <param name="array">The JSON array to deserialize.</param>
    /// <param name="type">The type of objects in the list as a <see cref="Type"/>.</param>
    /// <param name="instance"></param>
    /// <returns>A list of objects of type T.</returns>
    private static void DeserializeList<T>(ArrayList array, Type type, ref List<T> instance)
    where T : new()
    {
        foreach (var item in array)
        {
            if (item is string jsonString)
            {
                var deserializedItem = Deserialize<T>(jsonString);
                instance.Add(deserializedItem);
            }
            else if (item is Hashtable hashtable)
            {
                object? listItem = Activator.CreateInstance<T>();
                Deserialize(hashtable, typeof(T), ref listItem!);
                instance.Add((T)listItem);
            }
            else
            {
                throw new ArgumentException("Unsupported type in ArrayList for deserialization.");
            }
        }
    }

    /// <summary>
    /// Deserializes a JSON array into an array of objects of type T.
    /// </summary>
    /// <typeparam name="T">The type of objects in the array.</typeparam>
    /// <param name="array">The JSON array to deserialize.</param>
    /// <returns>An array of objects of type T.</returns>
    public static T[] DeserializeArray<T>(ArrayList array)
        where T : new()
    {
        var result = new T[array.Count];
        var index = 0;

        foreach (string item in array)
        {
            result[index++] = Deserialize<T>(item);
        }

        return result;
    }

    /// <summary>
    /// Deserializes a JSON array into an array of objects of the specified type.
    /// </summary>
    /// <param name="array">The JSON array to deserialize.</param>
    /// <param name="type">The type of objects in the array as a <see cref="System.Type"/>.</param>
    /// <param name="instance">The array instance to populate.</param>
    private static void DeserializeArray(ArrayList array, Type type, ref Array instance)
    {
        var index = 0;

        foreach (Hashtable item in array)
        {
            if (type == typeof(string))
            {
                var e = item.GetEnumerator();
                e.MoveNext();
                instance.SetValue(((DictionaryEntry)e.Current).Value, index);
                index++;
            }
            else
            {
                var arrayItem = Activator.CreateInstance(type);
                Deserialize(item, type, ref arrayItem);
                instance.SetValue(arrayItem, index++);
            }
        }
    }

    /// <summary>
    /// Deserializes an object of type T from a JSON string or Hashtable.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="data">The JSON string or Hashtable to deserialize.</param>
    /// <returns>An object of type T.</returns>
    public static T Deserialize<T>(object data)
    {
        if (data is string json)
        {
            return Deserialize<T>(json);
        }
        else if (data is Hashtable hashtable)
        {
            object? instance = Activator.CreateInstance<T>();
            Deserialize(hashtable, typeof(T), ref instance!);
            return (T)instance;
        }
        else if (data is byte[] byteArray)
        {
            var jsonString = new string(System.Text.Encoding.UTF8.GetChars(byteArray));
            return Deserialize<T>(jsonString);
        }
        else
        {
            throw new ArgumentException("Unsupported data type for deserialization.");
        }
    }

    /// <summary>
    /// Deserializes an object of type T from a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An object of type T.</returns>
    public static T Deserialize<T>(string json)
    {
        var type = typeof(T);

        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var rootArray = DeserializeString(json) as ArrayList;
            var targetArray = Array.CreateInstance(elementType, rootArray?.Count ?? 0);

            var index = 0;

            if (rootArray is not null)
            {
                foreach (Hashtable item in rootArray)
                {
                    object instance = Activator.CreateInstance(elementType);
                    Deserialize(item, elementType, ref instance);
                    targetArray.SetValue(instance, index++);
                }
            }

            return (T)(object)targetArray;
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = type.GetGenericArguments()[0];
            var rootArray = DeserializeString(json) as ArrayList;
            var targetList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

            var addMethod = targetList.GetType().GetMethod("Add");

            if (rootArray is not null)
            {
                foreach (Hashtable item in rootArray)
                {
                    object instance = Activator.CreateInstance(elementType);
                    Deserialize(item, elementType, ref instance);
                    addMethod.Invoke(targetList, new[] { instance });
                }
            }

            return (T)targetList;
        }
        else
        {
            object instance = Activator.CreateInstance(type);
            Deserialize(json, typeof(T), ref instance);

            return (T)instance;
        }
    }

    /// <summary>
    /// Deserializes an object of the specified type from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="type">The type of object to deserialize as a <see cref="System.Type"/>.</param>
    /// <param name="instance">The object instance to populate.</param>
    private static void Deserialize(string json, Type type, ref object instance)
    {
        var root = DeserializeString(json) as Hashtable;

        Deserialize(root, type, ref instance);
    }

    /// <summary>
    /// Deserializes an object of the specified type from a Hashtable.
    /// </summary>
    /// <param name="root">The Hashtable representing the JSON data.</param>
    /// <param name="type">The type of object to deserialize as a <see cref="System.Type"/>.</param>
    /// <param name="instance">The object instance to populate.</param>
    private static void Deserialize(Hashtable? root, Type type, ref object instance)
    {
        var values = root ?? throw new ArgumentException();

        var props = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).ToList();

        foreach (string v in values.Keys)
        {
            var prop = props.FirstOrDefault(p => string.Compare(p.Name, v, StringComparison.OrdinalIgnoreCase) == 0);

            if (prop != null && prop.CanWrite)
            {
                switch (true)
                {
                    case bool _ when prop.PropertyType.IsEnum:
                        var enumValue = Enum.Parse(prop.PropertyType, values[v].ToString());
                        prop.SetValue(instance, enumValue);
                        break;
                    case bool _ when prop.PropertyType == typeof(ulong):
                        prop.SetValue(instance, Convert.ToUInt64(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(long):
                        prop.SetValue(instance, Convert.ToInt64(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(uint):
                        prop.SetValue(instance, Convert.ToUInt32(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(int):
                        prop.SetValue(instance, Convert.ToInt32(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(ushort):
                        prop.SetValue(instance, Convert.ToUInt16(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(short):
                        prop.SetValue(instance, Convert.ToInt16(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(byte):
                        prop.SetValue(instance, Convert.ToByte(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(sbyte):
                        prop.SetValue(instance, Convert.ToSByte(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(decimal):
                        prop.SetValue(instance, Convert.ToDecimal(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(double):
                        prop.SetValue(instance, Convert.ToDouble(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(float):
                        prop.SetValue(instance, Convert.ToSingle(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(bool):
                        prop.SetValue(instance, Convert.ToBoolean(values[v]));
                        break;
                    case bool _ when prop.PropertyType == typeof(string):
                        prop.SetValue(instance, values[v].ToString());
                        break;
                    default:
                        if (prop.PropertyType.IsArray)
                        {
                            var al = values[v] as ArrayList;
                            var elementType = prop.PropertyType.GetElementType();
                            var targetArray = Array.CreateInstance(elementType, al!.Count);
                            DeserializeArray(al, elementType, ref targetArray);
                            prop.SetValue(instance, targetArray);
                        }
                        else if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var listType = prop.PropertyType.GetGenericArguments()[0];
                            var list = Activator.CreateInstance(prop.PropertyType);
                            var addMethod = prop.PropertyType.GetMethod("Add");

                            foreach (var item in (ArrayList)values[v])
                            {
                                var listItem = Activator.CreateInstance(listType);
                                Deserialize(item as Hashtable, listType, ref listItem);
                                addMethod.Invoke(list, new[] { listItem });
                            }

                            prop.SetValue(instance, list);
                        }
                        else if (IsComplexType(prop.PropertyType))
                        {
                            if (values[v] is Hashtable hashtableValue)
                            {
                                var complexInstance = Activator.CreateInstance(prop.PropertyType);
                                Deserialize(hashtableValue, prop.PropertyType, ref complexInstance);
                                prop.SetValue(instance, complexInstance);
                            }
                        }
                        else
                        {
                            throw new NotSupportedException($"Type '{prop.PropertyType}' not supported");
                        }
                        break;
                }
            }
        }
    }


    private static bool IsComplexType(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(Guid))
        {
            return false;
        }

        if (Nullable.GetUnderlyingType(type) != null)
        {
            return IsComplexType(Nullable.GetUnderlyingType(type));
        }

        if (type.IsEnum)
        {
            return false;
        }

        // If none of the above, it's a complex type
        return true;
    }
}