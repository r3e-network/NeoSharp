using System.Collections.Generic;
using System.Text.Json.Serialization;
using NeoSharp.Types;

#nullable enable

namespace NeoSharp.Protocol.Core.Response
{
    /// <summary>
    /// Response class for getapplicationlog method.
    /// </summary>
    public class NeoApplicationLog
    {
        /// <summary>
        /// Gets or sets the transaction hash.
        /// </summary>
        [JsonPropertyName("txid")]
        public Hash256 TxId { get; set; }

        /// <summary>
        /// Gets or sets the trigger.
        /// </summary>
        [JsonPropertyName("trigger")]
        public string Trigger { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the VM state.
        /// </summary>
        [JsonPropertyName("vmstate")]
        public string VmState { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the GAS consumed.
        /// </summary>
        [JsonPropertyName("gasconsumed")]
        public string GasConsumed { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stack.
        /// </summary>
        [JsonPropertyName("stack")]
        public List<StackItem> Stack { get; set; } = new();

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        [JsonPropertyName("notifications")]
        public List<Notification> Notifications { get; set; } = new();

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        [JsonPropertyName("exception")]
        public string? Exception { get; set; }

        /// <summary>
        /// Gets or sets the executions.
        /// </summary>
        [JsonPropertyName("executions")]
        public List<Execution> Executions { get; set; } = new();
    }

    /// <summary>
    /// Represents a notification.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the contract hash.
        /// </summary>
        [JsonPropertyName("contract")]
        public Hash160 Contract { get; set; }

        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        [JsonPropertyName("eventname")]
        public string EventName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonPropertyName("state")]
        public StackItem State { get; set; } = new StackItem();
    }

    /// <summary>
    /// Represents an execution.
    /// </summary>
    public class Execution
    {
        /// <summary>
        /// Gets or sets the trigger.
        /// </summary>
        [JsonPropertyName("trigger")]
        public string Trigger { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the VM state.
        /// </summary>
        [JsonPropertyName("vmstate")]
        public string VmState { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the GAS consumed.
        /// </summary>
        [JsonPropertyName("gasconsumed")]
        public string GasConsumed { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stack.
        /// </summary>
        [JsonPropertyName("stack")]
        public List<StackItem> Stack { get; set; } = new();

        /// <summary>
        /// Gets or sets the notifications.
        /// </summary>
        [JsonPropertyName("notifications")]
        public List<Notification> Notifications { get; set; } = new();

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        [JsonPropertyName("exception")]
        public string? Exception { get; set; }
    }
}
