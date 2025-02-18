using System.Net.Http;
using System.Text;
using System.Text.Json;
using Jarvis_Project_Community.Models;
using Jarvis_Project_Community.Resources;

namespace Jarvis_Project_Community.Controllers
{
    public class UiPathInstance
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _authToken = string.Empty;
        private readonly string orchestratorUrl = ProjectResources.UiPathOrchestratorURL;

        // UiPath Kimlik Bilgileri
        private readonly string externalAppId = ProjectResources.UiPathExternalAppId;
        private readonly string externalAppSecret = ProjectResources.UiPathExternalAppSecret;

        public UiPathInstance()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Jarvis_Project_Community");
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Authenticate()
        {
            Task.Run(() => AuthenticateAsync()).Wait();
        }

        private async Task AuthenticateAsync()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", externalAppId),
                new KeyValuePair<string, string>("client_secret", externalAppSecret),
                new KeyValuePair<string, string>("scope", "OR.Jobs OR.Folders OR.Robots OR.Execution OR.Machines OR.Settings")
            });

            try
            {
                var authUrl = "https://cloud.uipath.com/identity_/connect/token";
                var response = await _httpClient.PostAsync(authUrl, formContent);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _authToken = authResponse.access_token;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
            }
            catch (Exception ex)
            {
                throw new Exception($"Kimlik doğrulama başarısız: {ex.Message}", ex);
            }
        }

        public List<UiPathProject> GetProcesses()
        {
            return Task.Run(() => GetProcessesAsync()).Result;
        }

        private async Task<List<UiPathProject>> GetProcessesAsync()
        {
            Authenticate();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{orchestratorUrl}/odata/Releases");
                request.Headers.Add("X-UIPATH-TenantName", "DefaultTenant");
                request.Headers.Add("X-UIPATH-OrganizationUnitId", "6298457");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var processListResponse = JsonSerializer.Deserialize<ProcessListResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return ConvertProcessesToUiPathProjects(processListResponse);
            }
            catch (Exception ex)
            {
                throw new Exception($"Süreçleri getirirken hata oluştu: {ex.Message}", ex);
            }
        }

        public string StartProcess(string releaseKey)
        {
            return Task.Run(() => StartProcessAsync(releaseKey)).Result;
        }

        private async Task<string> StartProcessAsync(string releaseKey)
        {
            Authenticate();

            var startJobPayload = new
            {
                startInfo = new
                {
                    ReleaseKey = releaseKey,
                    Strategy = "ModernJobsCount",
                    JobsCount = 1,
                    NoOfRobots = 0
                }
            };

            var requestBody = new StringContent(JsonSerializer.Serialize(startJobPayload), Encoding.UTF8, "application/json");

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{orchestratorUrl}/odata/Jobs/UiPath.Server.Configuration.OData.StartJobs")
                {
                    Content = requestBody
                };
                request.Headers.Add("X-UIPATH-TenantName", "DefaultTenant");
                request.Headers.Add("X-UIPATH-OrganizationUnitId", "6298457");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var startJobResponse = JsonSerializer.Deserialize<StartJobResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return startJobResponse?.value?[0]?.Key;
            }
            catch (Exception ex)
            {
                throw new Exception($"Süreç başlatılamadı: {ex.Message}", ex);
            }
        }


        public string GetJobStatus(string jobKey)
        {
            return Task.Run(() => GetJobStatusAsync(jobKey)).Result;
        }

        private async Task<string> GetJobStatusAsync(string jobKey)
        {
            Authenticate();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{orchestratorUrl}/odata/Jobs?$filter=Key eq {jobKey}");
                request.Headers.Add("X-UIPATH-TenantName", "DefaultTenant");
                request.Headers.Add("X-UIPATH-OrganizationUnitId", "6298457");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jobStatusResponse = JsonSerializer.Deserialize<JobStatusResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return jobStatusResponse?.value?[0]?.State;
            }
            catch (Exception ex)
            {
                throw new Exception($"İş durumu sorgulanamadı: {ex.Message}", ex);
            }
        }


        private List<UiPathProject> ConvertProcessesToUiPathProjects(ProcessListResponse processListResponse)
        {
            var projects = new List<UiPathProject>();

            foreach (var process in processListResponse.value)
            {
                projects.Add(new UiPathProject
                {
                    Key = process.Key,
                    ProcessKey = process.ProcessKey,
                    Name = process.Name,
                    Description = process.Description
                });
            }

            return projects;
        }
    }

    #region UiPath API Model Sınıfları

    public class AuthResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class ProcessListResponse
    {
        public List<ProcessItem> value { get; set; }
    }

    public class ProcessItem
    {
        public string Key { get; set; }
        public string ProcessKey { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class StartJobResponse
    {
        public List<JobItem> value { get; set; }
    }

    public class JobStatusResponse
    {
        public List<JobItem> value { get; set; }
    }

    public class JobItem
    {
        public string Key { get; set; }
        public string State { get; set; }
    }



    #endregion
}
