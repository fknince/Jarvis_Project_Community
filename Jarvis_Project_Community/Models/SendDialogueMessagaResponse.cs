using System.Text.Json.Serialization;


namespace Jarvis_Project_Community.Models
{
    public class SendDialogueMessagaResponse
    {
        [JsonPropertyName("callResponse")]
        public string CallResponse { get; set; }
    }
}
