using System.Net.Http.Headers;
using System.Text;

namespace SecurityMonitoringApp.Services
{
    public class GraylogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GraylogService> _logger;

        public GraylogService(HttpClient httpClient, ILogger<GraylogService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Use the service name from docker-compose
            _httpClient.BaseAddress = new Uri("http://graylog:9000/api/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:admin")));

            // Add headers that Graylog expects
            _httpClient.DefaultRequestHeaders.Add("X-Requested-By", "SecurityMonitoringApp");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // Search logs in Graylog
        public async Task<string> SearchLogs(string query)
        {
            try
            {
                _logger.LogInformation("Searching Graylog with query: {Query}", query);

                // Use the correct Graylog search API endpoint
                var encodedQuery = Uri.EscapeDataString(query);
                var endpoint = $"search/universal/relative?query={encodedQuery}&range=300&fields=message,source,timestamp";

                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Graylog search failed");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while searching Graylog");
                throw;
            }
        }

        // Send custom log message
        public void LogCustomMessage(string message)
        {
            _logger.LogInformation("Custom Graylog Message: {Message}", message);
        }

        public async Task<bool> IsHealthy()
        {
            try
            {
                var response = await _httpClient.GetAsync("system");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}