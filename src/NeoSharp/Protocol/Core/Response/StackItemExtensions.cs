using System;
using System.Text;
using System.Linq;
using NeoSharp.Utils;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Extension methods for StackItem to match Swift SDK functionality
    /// </summary>
    public static class StackItemExtensions
    {
        /// <summary>
        /// Gets the string value from a StackItem
        /// </summary>
        public static string GetString(this StackItem stackItem)
        {
            if (stackItem == null)
                throw new ArgumentNullException(nameof(stackItem));
                
            switch (stackItem.Type?.ToLower())
            {
                case "bytestring":
                case "buffer":
                    if (stackItem.Value is string base64)
                    {
                        var bytes = Convert.FromBase64String(base64);
                        return Encoding.UTF8.GetString(bytes);
                    }
                    break;
            }
            
            throw new InvalidOperationException($"Cannot get string from StackItem of type {stackItem.Type}");
        }
        
        /// <summary>
        /// Gets the byte array value from a StackItem
        /// </summary>
        public static byte[] GetByteArray(this StackItem stackItem)
        {
            if (stackItem == null)
                throw new ArgumentNullException(nameof(stackItem));
                
            switch (stackItem.Type?.ToLower())
            {
                case "bytestring":
                case "buffer":
                    if (stackItem.Value is string base64)
                    {
                        return Convert.FromBase64String(base64);
                    }
                    break;
                case "integer":
                    if (stackItem.Value is string intStr && long.TryParse(intStr, out var intValue))
                    {
                        return BitConverter.GetBytes(intValue);
                    }
                    break;
            }
            
            throw new InvalidOperationException($"Cannot get byte array from StackItem of type {stackItem.Type}");
        }
        
        /// <summary>
        /// Checks if the StackItem is a buffer type
        /// </summary>
        public static bool IsBuffer(this StackItem stackItem)
        {
            return stackItem?.Type?.ToLower() == "buffer";
        }
        
        /// <summary>
        /// Checks if the StackItem is an interop interface
        /// </summary>
        public static bool IsInteropInterface(this StackItem stackItem)
        {
            return stackItem?.Type?.ToLower() == "interopinterface";
        }
        
        /// <summary>
        /// Gets the iterator ID from an interop interface StackItem
        /// </summary>
        public static string GetIteratorId(this StackItem stackItem)
        {
            if (!stackItem.IsInteropInterface())
                throw new InvalidOperationException("StackItem is not an interop interface");
                
            // The iterator ID is typically stored in a property or nested value
            // This will need to be adjusted based on the actual response format
            return stackItem.Value?.ToString() ?? throw new InvalidOperationException("No iterator ID found");
        }
        
        /// <summary>
        /// Gets the hex string value from a StackItem
        /// </summary>
        public static string GetHexString(this StackItem stackItem)
        {
            var bytes = stackItem.GetByteArray();
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
        
        /// <summary>
        /// Gets the integer value from a StackItem
        /// </summary>
        public static long GetInteger(this StackItem stackItem)
        {
            if (stackItem == null)
                throw new ArgumentNullException(nameof(stackItem));
                
            if (stackItem.Type?.ToLower() == "integer" && stackItem.Value is string intStr && long.TryParse(intStr, out var intValue))
            {
                return intValue;
            }
            
            throw new InvalidOperationException($"Cannot get integer from StackItem of type {stackItem.Type}");
        }
        
        /// <summary>
        /// Gets the boolean value from a StackItem
        /// </summary>
        public static bool GetBoolean(this StackItem stackItem)
        {
            if (stackItem == null)
                throw new ArgumentNullException(nameof(stackItem));
                
            switch (stackItem.Type?.ToLower())
            {
                case "boolean":
                    if (stackItem.Value is bool boolValue)
                        return boolValue;
                    if (stackItem.Value is string boolStr && bool.TryParse(boolStr, out var parsedBool))
                        return parsedBool;
                    break;
                case "integer":
                    return stackItem.GetInteger() != 0;
            }
            
            throw new InvalidOperationException($"Cannot get boolean from StackItem of type {stackItem.Type}");
        }
        
        /// <summary>
        /// Gets the list of StackItems from an array StackItem
        /// </summary>
        public static StackItem[] GetList(this StackItem stackItem)
        {
            if (stackItem == null)
                throw new ArgumentNullException(nameof(stackItem));
                
            if (!stackItem.IsArray && stackItem.Type?.ToLower() != "struct")
                throw new InvalidOperationException($"Cannot get list from StackItem of type {stackItem.Type}");
                
            // Assuming the value is stored in a property that can be cast to array
            if (stackItem.Value is StackItem[] array)
                return array;
                
            // Handle JSON deserialization case
            if (stackItem.Value is System.Text.Json.JsonElement jsonElement && jsonElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                var items = new System.Collections.Generic.List<StackItem>();
                foreach (var element in jsonElement.EnumerateArray())
                {
                    // This would need proper deserialization logic
                    // For now, throw to indicate implementation needed
                    throw new NotImplementedException("JSON array deserialization not yet implemented");
                }
                return items.ToArray();
            }
            
            throw new InvalidOperationException("Cannot extract array from StackItem");
        }
        
        /// <summary>
        /// Checks if the StackItem is an array type
        /// </summary>
        public static bool IsArray(this StackItem stackItem) => stackItem?.Type?.ToLower() == "array";
        
        /// <summary>
        /// Checks if the StackItem is a byte string type
        /// </summary>
        public static bool IsByteString(this StackItem stackItem) => stackItem?.Type?.ToLower() == "bytestring";
        
        /// <summary>
        /// Checks if the StackItem is an integer type
        /// </summary>
        public static bool IsInteger(this StackItem stackItem) => stackItem?.Type?.ToLower() == "integer";
        
        /// <summary>
        /// Checks if the StackItem is a boolean type
        /// </summary>
        public static bool IsBoolean(this StackItem stackItem) => stackItem?.Type?.ToLower() == "boolean";
        
        /// <summary>
        /// Checks if the StackItem is an any (null) type
        /// </summary>
        public static bool IsAny(this StackItem stackItem) => stackItem?.Type?.ToLower() == "any";
    }
}