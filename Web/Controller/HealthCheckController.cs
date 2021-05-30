using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
