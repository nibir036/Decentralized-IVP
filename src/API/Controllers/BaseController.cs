// Copyright (c) 2026 Muktadirul Islam Nibir. All rights reserved.

using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected IActionResult Success<T>(T data, string? message = null) =>
        Ok(new { success = true, data, message, timestamp = DateTime.UtcNow });

    protected IActionResult Created<T>(string location, T data) =>
        base.Created(location, new { success = true, data, timestamp = DateTime.UtcNow });
}