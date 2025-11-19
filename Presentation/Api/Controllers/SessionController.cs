using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly IMemoryCache _cache;
    private readonly IIdentityUserService _identityUser;
    private readonly ISessionService _session;

    public SessionController(IMemoryCache cache, ISessionService session)
    {
        _cache = cache;
        _session = session;
    }

    [HttpPost("logout/{userEmail}/clear-cache")]
    public IActionResult ClearCacheOnLogout(string userEmail)
    {
        _session.ClearAll(userEmail);
        return NoContent();
    }
}

