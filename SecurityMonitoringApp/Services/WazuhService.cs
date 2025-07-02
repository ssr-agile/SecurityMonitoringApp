using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SecurityMonitoringApp.Services
{
    public class WazuhService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<WazuhService> _logger;

        public WazuhService(HttpClient httpClient, IConfiguration config, ILogger<WazuhService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            var baseUrl = _config["Wazuh:Host"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("Wazuh:Host configuration is missing");
            }

            _httpClient.BaseAddress = new Uri(baseUrl);

            var username = _config["Wazuh:User"];
            var password = _config["Wazuh:Password"];

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var authToken = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{username}:{password}"));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
            }

            // Add timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GetAgents()
        {
            try
            {
                _logger.LogInformation("Fetching agents from Wazuh API");
                var response = await _httpClient.GetAsync("/agents");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to fetch agents from Wazuh API");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while fetching agents from Wazuh API");
                throw;
            }
        }

        public async Task<bool> IsHealthy()
        {
            try
            {
                var response = await _httpClient.GetAsync("/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}