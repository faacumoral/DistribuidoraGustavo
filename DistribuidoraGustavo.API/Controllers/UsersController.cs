using DistribuidoraGustavo.API.Shared;
using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Services;
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

    [Anonymous]
    [HttpPost("login")]
    public async Task Login([FromBody] LoginRequest request)
    {
        var response = await _userService.TryLogin(request);


    }

}
