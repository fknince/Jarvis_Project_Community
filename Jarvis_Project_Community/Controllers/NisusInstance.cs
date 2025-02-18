using System.Net.Http;
using System.Text;
using System.Text.Json;
using Jarvis_Project_Community.Models;
using Jarvis_Project_Community.Resources;
namespace Jarvis_Project_Community.Controllers
{
    public class NisusInstance
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public NisusInstance()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jarvis_Project_Community");
        }


        public SendDialogueMessagaResponse StartAndSendMessage(SendDialogueMessageRequest request)
        {
            return Task.Run(() => StartAndSendMessageAsync(request)).Result;
        }

        public async Task<SendDialogueMessagaResponse> StartAndSendMessageAsync(SendDialogueMessageRequest request)
        {
            var beginResponse = await BeginDialogueAsync();

            if (beginResponse == null || string.IsNullOrEmpty(beginResponse.SessionId))
            {
                throw new Exception("Session could not be started.");
            }

            request.ChatSessionId = beginResponse.SessionId;

            return await SendDialogueMessageAsync(request);
        }



        public BeginDialogueResponse BeginDialogue()
        {
            return Task.Run(() => BeginDialogueAsync()).Result;
        }
        private async Task<BeginDialogueResponse> BeginDialogueAsync()
        {
            var url = ProjectResources.GetNewExternalChatSessionIdEndpoint;

            try
            {
                var response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response to ChatSessionResponse
                var chatSessionResponse = JsonSerializer.Deserialize<BeginDialogueResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return chatSessionResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching chat session ID: " + ex.Message, ex);
            }
        }

        public SendDialogueMessagaResponse SendDialogueMessage(SendDialogueMessageRequest request)
        {
            return Task.Run(() => SendDialogueMessageAsync(request)).Result;
        }
        private async Task<SendDialogueMessagaResponse> SendDialogueMessageAsync(SendDialogueMessageRequest request)
        {
            var url = ProjectResources.SendExternalDeploymentCallEndpoint;
            int maxRetries = 5;
            int retryCount = 0;
            string message = request.Message; // Orijinal mesajı sakla

            while (retryCount < maxRetries)
            {
                try
                {
                    request.Message = ShortenMessage(message, retryCount);

                    var requestBody = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, requestBody);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var externalCallResponse = JsonSerializer.Deserialize<SendDialogueMessagaResponse>(
                        jsonResponse.Replace("```json", "").Replace("```", ""),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (externalCallResponse is null)
                    {
                        throw new Exception("API response can not be null.");
                    }

                    if (!IsValidJsonArray(externalCallResponse.CallResponse))
                    {
                        throw new Exception("Invalid JSON response format.");
                    }

                    if (externalCallResponse.CallResponse == "Length")
                    {
                        retryCount++;
                        continue;
                    }

                    return externalCallResponse;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= maxRetries)
                    {
                        throw new Exception($"Error while sending deployment call after {maxRetries} attempts: " + ex.Message, ex);
                    }
                    await Task.Delay(1000 * retryCount);
                }
            }

            throw new Exception("Unexpected error occurred in SendDialogueMessageAsync.");
        }
        private bool IsValidJsonArray(string json)
        {
            json = json.Trim();
            return json.StartsWith("[") && json.EndsWith("]");
        }
        private string ShortenMessage(string message, int attempt)
        {
            if (attempt == 0)
            {
                return CleanMessage(message);
            }
            else
            {
                return CleanMessage(message).Length > 100 ? CleanMessage(message).Substring(100 * attempt) : "";
            }
        }

        private string CleanMessage(string message)
        {
            return message
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("\r", " ")
                .Replace("\\", "")
                .Trim();
        }
    }
}

