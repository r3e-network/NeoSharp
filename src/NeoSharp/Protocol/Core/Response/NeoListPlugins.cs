using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for listplugins method.
    /// </summary>
    public class NeoListPlugins
    {
        /// <summary>
        /// Represents a plugin.
        /// </summary>
        public class Plugin
        {
            /// <summary>
            /// Gets or sets the plugin name.
            /// </summary>
            [JsonPropertyName("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the version.
            /// </summary>
            [JsonPropertyName("version")]
            public string Version { get; set; }

            /// <summary>
            /// Gets or sets the interfaces.
            /// </summary>
            [JsonPropertyName("interfaces")]
            public List<string> Interfaces { get; set; }
        }
    }
}