using System.Text.Json.Serialization;


namespace Jarvis_Project_Community.Models
{
    public class BeginDialogueResponse
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
    }
}
