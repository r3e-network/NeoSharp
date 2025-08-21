using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Represents a stack item in Neo VM.
    /// </summary>
    public class StackItem
    {
        /// <summary>
        /// Gets or sets the type of the stack item.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the id (for iterators and other reference types).
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the interface (for interop interface types).
        /// </summary>
        [JsonPropertyName("interface")]
        public string Interface { get; set; }

        /// <summary>
        /// Gets or sets the iterator (for iterator types).
        /// </summary>
        [JsonPropertyName("iterator")]
        public List<StackItem> Iterator { get; set; }

        /// <summary>
        /// Gets or sets the truncated flag (for large items).
        /// </summary>
        [JsonPropertyName("truncated")]
        public bool? Truncated { get; set; }

        /// <summary>
        /// Gets a value indicating whether this item is a ByteString
        /// </summary>
        public bool IsByteString => Type?.Equals("ByteString", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Gets a value indicating whether this item is an Integer
        /// </summary>
        public bool IsInteger => Type?.Equals("Integer", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Gets a value indicating whether this item is a Boolean
        /// </summary>
        public bool IsBoolean => Type?.Equals("Boolean", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Gets a value indicating whether this item is an Array
        /// </summary>
        public bool IsArray => Type?.Equals("Array", StringComparison.OrdinalIgnoreCase) == true;

        /// <summary>
        /// Gets a value indicating whether this item is Any type (null or empty)
        /// </summary>
        public bool IsAny => Type?.Equals("Any", StringComparison.OrdinalIgnoreCase) == true ||
                           Type?.Equals("Null", StringComparison.OrdinalIgnoreCase) == true ||
                           string.IsNullOrEmpty(Type);

        /// <summary>
        /// Gets the value as a byte array
        /// </summary>
        /// <returns>The byte array representation</returns>
        public byte[] GetByteArray()
        {
            if (Value is string stringValue)
            {
                // Handle base64 encoded values
                return Convert.FromBase64String(stringValue);
            }
            
            if (Value is byte[] byteArray)
            {
                return byteArray;
            }
            
            throw new InvalidOperationException($"Cannot convert {Type} to byte array");
        }

        /// <summary>
        /// Gets the value as an integer
        /// </summary>
        /// <returns>The integer value</returns>
        public int GetInteger()
        {
            if (Value is int intValue)
                return intValue;
                
            if (Value is string stringValue && int.TryParse(stringValue, out var parsedInt))
                return parsedInt;
                
            throw new InvalidOperationException($"Cannot convert {Type} to integer");
        }

        /// <summary>
        /// Gets the value as a boolean
        /// </summary>
        /// <returns>The boolean value</returns>
        public bool GetBoolean()
        {
            if (Value is bool boolValue)
                return boolValue;
                
            if (Value is string stringValue && bool.TryParse(stringValue, out var parsedBool))
                return parsedBool;
                
            throw new InvalidOperationException($"Cannot convert {Type} to boolean");
        }

        /// <summary>
        /// Gets the value as a list of stack items
        /// </summary>
        /// <returns>The list of stack items</returns>
        public List<StackItem> GetList()
        {
            if (Value is List<StackItem> stackItems)
                return stackItems;
                
            if (Value is object[] objArray)
            {
                var result = new List<StackItem>();
                foreach (var item in objArray)
                {
                    if (item is StackItem stackItem)
                        result.Add(stackItem);
                    else
                        result.Add(new StackItem { Type = "Unknown", Value = item });
                }
                return result;
            }
                
            throw new InvalidOperationException($"Cannot convert {Type} to list");
        }

        /// <summary>
        /// Gets the value as a hex string
        /// </summary>
        /// <returns>The hex string value</returns>
        public string? GetHexString()
        {
            if (Value is string stringValue)
                return stringValue;
                
            if (Value is byte[] byteArray)
                return Convert.ToHexString(byteArray);
                
            return Value?.ToString();
        }
    }
}