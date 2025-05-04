using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace AspireAPI.Services
{
    /// <summary>
    /// Service for parsing and validating tool parameters
    /// </summary>
    public class ParameterParserService
    {
        /// <summary>
        /// Gets a required argument from the arguments dictionary
        /// </summary>
        public T GetRequiredArgument<T>(IDictionary<string, object> arguments, string key)
        {
            if (!arguments.TryGetValue(key, out var value) || value == null)
            {
                throw new ArgumentException($"Missing required argument: {key}");
            }

            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value.ToString();
                }

                if (value is JsonElement jsonElement)
                {
                    return jsonElement.Deserialize<T>();
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid value for {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets an optional argument from the arguments dictionary or returns a default value
        /// </summary>
        public T GetOptionalArgument<T>(
            IDictionary<string, object> arguments,
            string key,
            T defaultValue)
        {
            if (!arguments.TryGetValue(key, out var value) || value == null)
            {
                return defaultValue;
            }

            try
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value.ToString();
                }

                if (value is JsonElement jsonElement)
                {
                    return jsonElement.Deserialize<T>();
                }

                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a list of strings from an argument
        /// </summary>
        public List<string> GetStringListArgument(IDictionary<string, object> arguments, string key)
        {
            var list = new List<string>();

            if (arguments.TryGetValue(key, out var listObj))
            {
                if (listObj is JsonElement jsonElement &&
                    jsonElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var element in jsonElement.EnumerateArray())
                    {
                        list.Add(element.GetString());
                    }
                }
                else if (listObj is IEnumerable<object> objectArray)
                {
                    foreach (var item in objectArray)
                    {
                        list.Add(item.ToString());
                    }
                }
            }

            return list;
        }
    }
}