using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Serialization;

public static partial class MicroJson
{
    /// <summary>
    /// Provides utility methods for working with JSON strings without full deserialization.
    /// </summary>
    public static class JsonUtilities
    {
        /// <summary>
        /// Extracts all root-level property names from a JSON object string.
        /// </summary>
        /// <param name="json">The JSON string to parse</param>
        /// <returns>A list of root-level property names</returns>
        public static List<string> GetRootPropertyNames(string json)
        {
            var propertyNames = new List<string>();
            
            if (string.IsNullOrWhiteSpace(json)) return propertyNames;
            
            // Find the opening brace of the root object
            int startIndex = json.IndexOf('{');
            if (startIndex == -1) return propertyNames;
            
            int currentIndex = startIndex + 1;
            
            while (currentIndex < json.Length)
            {
                var ch = json[currentIndex];
                
                // Skip whitespace
                if (char.IsWhiteSpace(ch))
                {
                    currentIndex++;
                    continue;
                }
                
                // End of root object
                if (ch == '}')
                    break;
                    
                // Found a property name (starts with quote)
                if (ch == '"')
                {
                    var propertyNameStart = currentIndex + 1;
                    var propertyNameEnd = FindStringEnd(json, currentIndex);
                    
                    if (propertyNameEnd != -1)
                    {
                        var propertyName = json.Substring(propertyNameStart, propertyNameEnd - propertyNameStart);
                        propertyNames.Add(propertyName);
                        
                        // Skip past the property name and colon to the value
                        currentIndex = propertyNameEnd + 1;
                        
                        // Skip to colon
                        while (currentIndex < json.Length && json[currentIndex] != ':')
                            currentIndex++;
                        
                        if (currentIndex < json.Length)
                            currentIndex++; // Skip the colon
                        
                        // Skip the value
                        var valueStart = SkipWhitespace(json, currentIndex);
                        if (valueStart < json.Length)
                        {
                            var valueEnd = FindJsonValueEnd(json, valueStart);
                            if (valueEnd != -1)
                                currentIndex = valueEnd + 1;
                        }
                        
                        // Skip comma if present
                        currentIndex = SkipWhitespace(json, currentIndex);
                        if (currentIndex < json.Length && json[currentIndex] == ',')
                            currentIndex++;
                    }
                    else
                    {
                        currentIndex++;
                    }
                }
                else
                {
                    currentIndex++;
                }
            }
            
            return propertyNames;
        }

        /// <summary>
        /// Extracts a specific property value from a JSON object string without full deserialization.
        /// </summary>
        /// <param name="json">The JSON string to parse</param>
        /// <param name="propertyName">The name of the property to extract</param>
        /// <returns>The JSON value as a string, or null if not found</returns>
        public static string? GetPropertyValue(string json, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(propertyName))
                return null;

            var startPattern = $"\"{propertyName}\":";
            var startIndex = json.IndexOf(startPattern);
            
            if (startIndex == -1) return null;
            
            startIndex += startPattern.Length;
            
            while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
                startIndex++;
            
            if (startIndex >= json.Length) return null;
            
            var endIndex = FindJsonValueEnd(json, startIndex);
            if (endIndex == -1) return null;
            
            return json.Substring(startIndex, endIndex - startIndex + 1);
        }

        /// <summary>
        /// Extracts multiple property values from a JSON object string.
        /// </summary>
        /// <param name="json">The JSON string to parse</param>
        /// <param name="propertyNames">The names of the properties to extract</param>
        /// <returns>A dictionary containing the property names and their JSON values</returns>
        public static Dictionary<string, string> GetPropertyValues(string json, params string[] propertyNames)
        {
            var result = new Dictionary<string, string>();
            
            foreach (var propertyName in propertyNames)
            {
                var value = GetPropertyValue(json, propertyName);
                if (value != null)
                {
                    result[propertyName] = value;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Extracts all root-level properties from a JSON object string as key-value pairs.
        /// </summary>
        /// <param name="json">The JSON string to parse</param>
        /// <returns>A dictionary containing all root-level properties and their JSON values</returns>
        public static Dictionary<string, string> GetAllRootProperties(string json)
        {
            var propertyNames = GetRootPropertyNames(json);
            return GetPropertyValues(json, propertyNames.ToArray());
        }

        #region Helper Methods

        private static int SkipWhitespace(string json, int startIndex)
        {
            while (startIndex < json.Length && char.IsWhiteSpace(json[startIndex]))
                startIndex++;
            return startIndex;
        }

        private static int FindJsonValueEnd(string json, int startIndex)
        {
            var ch = json[startIndex];
            
            if (ch == '"')
            {
                return FindStringEnd(json, startIndex);
            }
            else if (ch == '{')
            {
                return FindObjectEnd(json, startIndex);
            }
            else if (ch == '[')
            {
                return FindArrayEnd(json, startIndex);
            }
            else if (char.IsDigit(ch) || ch == '-' || ch == 't' || ch == 'f' || ch == 'n')
            {
                return FindPrimitiveEnd(json, startIndex);
            }
            
            return -1;
        }

        private static int FindStringEnd(string json, int startIndex)
        {
            for (int i = startIndex + 1; i < json.Length; i++)
            {
                if (json[i] == '"' && (i == 0 || json[i - 1] != '\\'))
                    return i;
            }
            return -1;
        }

        private static int FindObjectEnd(string json, int startIndex)
        {
            int braceCount = 1;
            bool inString = false;
            
            for (int i = startIndex + 1; i < json.Length; i++)
            {
                var ch = json[i];
                
                if (ch == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inString = !inString;
                }
                else if (!inString)
                {
                    if (ch == '{') braceCount++;
                    else if (ch == '}') braceCount--;
                    
                    if (braceCount == 0) return i;
                }
            }
            return -1;
        }

        private static int FindArrayEnd(string json, int startIndex)
        {
            int bracketCount = 1;
            bool inString = false;
            
            for (int i = startIndex + 1; i < json.Length; i++)
            {
                var ch = json[i];
                
                if (ch == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inString = !inString;
                }
                else if (!inString)
                {
                    if (ch == '[') bracketCount++;
                    else if (ch == ']') bracketCount--;
                    
                    if (bracketCount == 0) return i;
                }
            }
            return -1;
        }

        private static int FindPrimitiveEnd(string json, int startIndex)
        {
            for (int i = startIndex; i < json.Length; i++)
            {
                var ch = json[i];
                if (ch == ',' || ch == '}' || ch == ']' || char.IsWhiteSpace(ch))
                    return i - 1;
            }
            return json.Length - 1;
        }

        #endregion
    }
}