using System.Text.Json;


namespace Jarvis_Project_Community.Models
{
    public class SendDialogueMessageRequest
    {

        public string Message { get; set; }


        public string ModelDeploymentEndpointId { get; set; }


        public string ChatSessionId { get; set; }


        public List<ImageItem> ImageItems { get; set; } = new List<ImageItem>();


        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }


    public class ImageItem
    {

        public string FullName { get; set; }

        public string Base64Content { get; set; }
    }
}
