using DistribuidoraGustavo.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class BaseController : ControllerBase
    {
        protected int UserId { get => int.Parse(User.Claims.First(c => c.Type == JwtManager.USER_ID_CLAIM).Value); }

    }
}
