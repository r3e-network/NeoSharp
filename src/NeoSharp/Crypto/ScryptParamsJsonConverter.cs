using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeoSharp.Crypto
{
    /// <summary>
    /// Custom JSON converter for ScryptParams that supports multiple property name formats.
    /// </summary>
    public class ScryptParamsJsonConverter : JsonConverter<ScryptParams>
    {
        public override ScryptParams Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            int n = 0, r = 0, p = 0;
            bool hasN = false, hasR = false, hasP = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString()?.ToLowerInvariant();
                    reader.Read(); // Move to value

                    if (reader.TokenType != JsonTokenType.Number)
                    {
                        continue; // Skip non-numeric values
                    }

                    var value = reader.GetInt32();

                    switch (propertyName)
                    {
                        case "n":
                        case "cost":
                            n = value;
                            hasN = true;
                            break;
                        case "r":
                        case "blocksize":
                        case "blockSize":
                            r = value;
                            hasR = true;
                            break;
                        case "p":
                        case "parallel":
                            p = value;
                            hasP = true;
                            break;
                    }
                }
            }

            // For incomplete JSON, only provide defaults if some properties are present
            // If completely empty or missing critical properties, let validation catch it
            bool hasAnyParameter = hasN || hasR || hasP;
            
            if (!hasAnyParameter)
            {
                // Empty object - use all defaults which will fail validation
                n = 0; r = 0; p = 0;
            }
            else
            {
                // Partial data - only fill missing fields with defaults if we have at least one parameter
                if (!hasN && (hasR || hasP)) n = ScryptParams.N_STANDARD;
                if (!hasR && (hasN || hasP)) r = ScryptParams.R_STANDARD;
                if (!hasP && (hasN || hasR)) p = ScryptParams.P_STANDARD;
                
                // If only one parameter is missing in a partially complete object, that's still invalid
                if (!hasN || !hasR || !hasP)
                {
                    // Use 0 for missing parameters to trigger validation failure
                    if (!hasN) n = 0;
                    if (!hasR) r = 0;
                    if (!hasP) p = 0;
                }
            }

            return new ScryptParams(n, r, p);
        }

        public override void Write(Utf8JsonWriter writer, ScryptParams value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("n", value.N);
            writer.WriteNumber("r", value.R);
            writer.WriteNumber("p", value.P);
            writer.WriteEndObject();
        }
    }
}