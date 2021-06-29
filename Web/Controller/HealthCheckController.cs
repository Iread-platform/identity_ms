using Microsoft.AspNetCore.Mvc;

namespace iread_story.Web.Controller
{
    [Route("[Controller]")]
public class HealthCheckController : ControllerBase
{
    [HttpGet("")]
    [HttpHead("")]
    public IActionResult Ping()
    {
        return Ok();
    }
}
}
