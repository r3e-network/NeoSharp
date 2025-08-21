namespace NeoSharp.Serialization
{
    /// <summary>
    /// Interface for Neo serializable objects.
    /// </summary>
    public interface INeoSerializable
    {
        /// <summary>
        /// Gets the size of the serialized data.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Serializes the object to a BinaryWriter.
        /// </summary>
        /// <param name="writer">The BinaryWriter to write to.</param>
        void Serialize(BinaryWriter writer);

        /// <summary>
        /// Deserializes the object from a BinaryReader.
        /// </summary>
        /// <param name="reader">The BinaryReader to read from.</param>
        void Deserialize(BinaryReader reader);
    }

    /// <summary>
    /// Extension methods for INeoSerializable.
    /// </summary>
    public static class NeoSerializableExtensions
    {
        /// <summary>
        /// Serializes the object to a byte array.
        /// </summary>
        /// <param name="serializable">The serializable object.</param>
        /// <returns>The serialized byte array.</returns>
        public static byte[] ToArray(this INeoSerializable serializable)
        {
            using var ms = new MemoryStream();
            using var writer = new NeoSharp.Serialization.BinaryWriter(ms);
            serializable.Serialize(writer);
            return ms.ToArray();
        }

        /// <summary>
        /// Creates an instance from a byte array.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="data">The byte array data.</param>
        /// <returns>The deserialized object.</returns>
        public static T FromArray<T>(byte[] data) where T : INeoSerializable, new()
        {
            using var ms = new MemoryStream(data);
            using var reader = new NeoSharp.Serialization.BinaryReader(ms);
            var obj = new T();
            obj.Deserialize(reader);
            return obj;
        }
    }
}