using Microsoft.AspNetCore.Mvc;
using TypesettingMIS.Infrastructure.Data;

namespace TypesettingMIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController(ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Health check endpoint - checks database connectivity
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connectivity
            var canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Database = "Disconnected"
                });
            }

            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Database = "Connected"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Database = "Disconnected",
                Error = ex.Message
            });
        }
    }
}
