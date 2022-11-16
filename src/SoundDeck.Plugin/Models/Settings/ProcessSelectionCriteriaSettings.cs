namespace SoundDeck.Plugin.Models.Settings
{
    using System;
    using Newtonsoft.Json;
    using SoundDeck.Core.Sessions;

    /// <summary>
    /// Provides a base class for settings that implement <see cref="IProcessSelectionCriteria"/>.
    /// </summary>
    public class ProcessSelectionCriteriaSettings : IProcessSelectionCriteria
    {
        /// <inheritdoc/>
        public string ProcessName { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public ProcessSelectionType ProcessSelectionType { get; set; }

        /// <summary>
        /// Gets or sets the label that accompanies the selected process.
        /// </summary>
        public string ProcessLabel { get; set; }

        /// <summary>
        /// Sets the <see cref="ProcessName"/> and <see cref="ProcessSelectionType"/> based on the value, providing backwards compatibility.
        /// </summary>
        [JsonProperty("processSelectionType")]
        public string DynamicProcessSelectionType
        {
            set
            {
                if (int.TryParse(value?.ToString() ?? string.Empty, out var @enum)
                    && Enum.IsDefined(typeof(ProcessSelectionType), @enum))
                {
                    this.ProcessSelectionType = (ProcessSelectionType)@enum;
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    this.ProcessSelectionType = ProcessSelectionType.Foreground;
                }
                else
                {
                    this.ProcessSelectionType = ProcessSelectionType.ByName;
                    this.ProcessName = value;
                }
            }
        }
    }
}
