using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
    /// Deserializes an object of type T from a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="encodedData">A UTF8-encoded JSON string to deserialize.</param>
    /// <returns>An object of type T.</returns>
    public static T Deserialize<T>(byte[] encodedData)
    {
        return Deserialize<T>(Encoding.UTF8.GetString(encodedData));
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

        return (T)Deserialize(json, type);
    }

    /// <summary>
    /// Deserializes an object of type T from a JSON string.
    /// </summary>
    /// <param name="type">The type of object to deserialize.</param>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An object of the specified type</returns>
    public static object Deserialize(string json, Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var rootArray = DeserializeString(json) as ArrayList;
            var targetArray = Array.CreateInstance(elementType, rootArray?.Count ?? 0);

            var index = 0;

            if (rootArray is not null)
            {
                foreach (var item in rootArray)
                {
                    if (item is Hashtable h)
                    {
                        object instance = Activator.CreateInstance(elementType);
                        Deserialize(h, elementType, ref instance);
                        targetArray.SetValue(instance, index++);
                    }
                    else
                    {
                        var instance = Convert.ChangeType(item, elementType);
                        targetArray.SetValue(instance, index++);
                    }
                }
            }

            return targetArray;
        }
        else if (typeof(IDictionary).IsAssignableFrom(type))
        {
            if (type.IsGenericType)
            {
                if (DeserializeString(json) is not Hashtable table)
                {
                    throw new ArgumentException("Invalid JSON data or format not supported");
                }

                return DeserializeHashtableToDictionary(table, type)
                    ?? throw new NotSupportedException($"Type '{type.Name}' not supported");
            }
            throw new NotSupportedException($"Type '{type.Name}' not supported");
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

            return targetList;
        }
        else
        {
            object instance = Activator.CreateInstance(type);
            Deserialize(json, type, ref instance);

            return instance;
        }
    }

    private static IDictionary? DeserializeHashtableToDictionary(Hashtable hashtable, Type dictionaryType)
    {
        var genericArguments = dictionaryType.GetGenericArguments();
        var keyType = genericArguments[0];
        var valueType = genericArguments[1];

        var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);

        foreach (DictionaryEntry entry in hashtable)
        {
            object key = Convert.ChangeType(entry.Key, keyType);
            object value = Convert.ChangeType(entry.Value, valueType);
            dictionary.Add(key, value);
        }

        return dictionary;
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

        var props = type.GetProperties(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length == 0)
            .ToList();

        (PropertyInfo Property, string MappedTo)[] nameMap =
            props.Select((propertyInfo, index) => (
                propertyInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(JsonPropertyName)),
                props[index]))
            .Where(p => p.Item1 != null)
            .Select(t => (t.Item2, t.Item1.ConstructorArguments[0].Value.ToString()))
            .ToArray();

        foreach (string v in values.Keys)
        {
            var prop = props.FirstOrDefault(p => string.Compare(p.Name, v, StringComparison.OrdinalIgnoreCase) == 0);

            if (prop == null)
            {
                prop = nameMap.FirstOrDefault(p => p.MappedTo == v).Property;
            }

            if (prop != null && prop.CanWrite)
            {
                Type propType = prop.PropertyType;

                if (propType.IsEnum)
                {
                    prop.SetValue(instance, Enum.Parse(propType, values[v].ToString()));
                }
                else if (propType.IsArray)
                {
                    var al = values[v] as ArrayList;
                    var elementType = propType.GetElementType();
                    var targetArray = Array.CreateInstance(elementType, al!.Count);
                    for (int i = 0; i < al.Count; i++)
                    {
                        if (elementType == typeof(string))
                        {
                            targetArray.SetValue(al[i], i);
                        }
                        else if (elementType.IsValueType || elementType.IsEnum)
                        {
                            targetArray.SetValue(Convert.ChangeType(al[i], elementType), i);
                        }
                        else
                        {
                            object arrayItem = Activator.CreateInstance(elementType);
                            Deserialize(al[i] as Hashtable, elementType, ref arrayItem);
                            targetArray.SetValue(arrayItem, i);
                        }
                    }
                    prop.SetValue(instance, targetArray);
                }
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listType = propType.GetGenericArguments()[0];
                    var list = Activator.CreateInstance(propType);
                    var addMethod = propType.GetMethod("Add");

                    foreach (var item in (ArrayList)values[v])
                    {
                        object listItem = Activator.CreateInstance(listType);
                        Deserialize(item as Hashtable, listType, ref listItem);
                        addMethod.Invoke(list, new[] { listItem });
                    }

                    prop.SetValue(instance, list);
                }
                else if (propType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    var dictionary = DeserializeHashtableToDictionary((Hashtable)values[v], propType)
                        ?? throw new NotSupportedException($"Type '{type.Name}' not supported");
                    prop.SetValue(instance, dictionary);
                }
                else if (IsComplexType(propType))
                {
                    if (values[v] is Hashtable hashtableValue)
                    {
                        object complexInstance = Activator.CreateInstance(propType);
                        Deserialize(hashtableValue, propType, ref complexInstance);
                        prop.SetValue(instance, complexInstance);
                    }
                    else if (propType == typeof(DateTimeOffset))
                    {
                        var dto = DateTimeOffset.Parse(values[v].ToString());
                        prop.SetValue(instance, dto);
                    }
                    else if (propType == typeof(TimeSpan))
                    {
                        var dto = TimeSpan.Parse(values[v].ToString());
                        prop.SetValue(instance, dto);
                    }
                    else if (propType == typeof(object))
                    {
                        prop.SetValue(instance, DeserializeDynamic(values[v]));
                    }
                    else
                    {
                        throw new NotSupportedException($"Unable to deserialize type '{propType}'");
                    }
                }
                else
                {
                    if (values[v] != null && values[v] != DBNull.Value)
                    {
                        prop.SetValue(instance, Convert.ChangeType(values[v], propType));
                    }
                }
            }
        }
    }

    private static bool IsComplexType(Type type)
    {
        if (type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(Guid)
            )
        {
            return false;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return IsComplexType(Nullable.GetUnderlyingType(type));
        }

        return true;
    }

    private static object? DeserializeDynamic(object jsonValue)
    {
        return jsonValue switch
        {
            string stringValue => stringValue,
            double doubleValue => doubleValue,
            long longValue => longValue,
            bool boolValue => boolValue,
            _ => throw new NotSupportedException($"Unable to deserialize dynamic type '{jsonValue.GetType()}'")
        };
    }
}