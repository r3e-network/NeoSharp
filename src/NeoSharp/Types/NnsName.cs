using System;

namespace NeoSharp.Types
{
    /// <summary>
    /// Represents a Neo Name Service (NNS) name
    /// </summary>
    public class NnsName
    {
        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the name value (alias for Name)
        /// </summary>
        public string Value => Name;

        /// <summary>
        /// Initializes a new NNS name
        /// </summary>
        /// <param name="name">The name</param>
        public NnsName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            
            Name = name;
        }

        /// <summary>
        /// Converts the NNS name to string
        /// </summary>
        /// <returns>The name string</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Implicit conversion from string
        /// </summary>
        /// <param name="name">The name</param>
        public static implicit operator NnsName(string name) => new(name);

        /// <summary>
        /// Implicit conversion to string
        /// </summary>
        /// <param name="nnsName">The NNS name</param>
        public static implicit operator string(NnsName nnsName) => nnsName.Name;
    }
}