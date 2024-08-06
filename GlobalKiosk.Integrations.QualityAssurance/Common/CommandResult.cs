namespace GlobalKiosk.Integrations.QualityAssurance.Common
{
    using Newtonsoft.Json;

    public class CommandResult
    {
        #region Constructor

        public CommandResult(bool success, string[] errors, string recordId)
        {
            Success = success;
            Errors = errors;
            RecordId = recordId;
        }

        #endregion Constructor

        #region Properties

        [JsonProperty("success")]
        public bool Success { get; }
        [JsonProperty("errors")]
        public string[] Errors { get; }
        [JsonProperty("recordId")]
        public string RecordId { get; }

        #endregion Properties
    }
}
