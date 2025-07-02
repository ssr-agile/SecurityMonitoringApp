using Microsoft.AspNetCore.Mvc;
using SecurityMonitoringApp.Services;

namespace SecurityMonitoringApp.Controllers
{
    [ApiController]
    [Route("api/monitoring")]
    public class MonitoringController : ControllerBase
    {
        private readonly WazuhService _wazuh;
        private readonly GraylogService _graylog;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(WazuhService wazuh, ILogger<MonitoringController> logger, GraylogService graylog)
        {
            _wazuh = wazuh;
            _logger = logger;
            _graylog = graylog;
        }

        [HttpGet("/graylog/health")]
        public async Task<IActionResult> GetGraylogHealth()
        {
            _logger.LogInformation("Health check requested");

            var graylogHealth = await _graylog.IsHealthy();

            return Ok(new
            {
                Status = "Running",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    Graylog = graylogHealth ? "Healthy" : "Unhealthy"
                }
            });
        }

        [HttpGet("/wazuh/health")]
        public async Task<IActionResult> GetWazuhHealth()
        {
            _logger.LogInformation("Health check requested");

            var wazuhHealth = await _wazuh.IsHealthy();

            return Ok(new
            {
                Status = "Running",
                Timestamp = DateTime.UtcNow,
                Services = new
                {
                    Wazuh = wazuhHealth ? "Healthy" : "Unhealthy"
                }
            });
        }

        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents()
        {
            try
            {
                _logger.LogInformation("Fetching Wazuh agents");
                var agents = await _wazuh.GetAgents();
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching agents");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("search-logs")]
        public async Task<IActionResult> SearchLogs([FromQuery] string query = "*")
        {
            try
            {
                _logger.LogInformation("Searching logs with query: {Query}", query);
                var results = await _graylog.SearchLogs(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching logs");
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("log")]
        public IActionResult LogMessage([FromBody] LogRequest request)
        {
            _logger.LogInformation("Custom log message: {Message}", request.Message);
            _graylog.LogCustomMessage(request.Message);
            return Ok(new { Status = "Logged", Message = request.Message });
        }
    }

    public class LogRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}