using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Responses;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers;

public class UsersController : BaseController
{
    private readonly ILogger<UsersController> _logger;
    private readonly IUserService _userService; 

    public UsersController(
        ILogger<UsersController> logger,
        IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<DTOResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _userService.TryLogin(request);
        return response;
    }

}
