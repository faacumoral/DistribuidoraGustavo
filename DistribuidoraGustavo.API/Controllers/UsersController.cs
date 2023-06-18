using DistribuidoraGustavo.Interfaces.Requests;
using DistribuidoraGustavo.Interfaces.Responses;
using DistribuidoraGustavo.Interfaces.Services;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistribuidoraGustavo.API.Controllers;

public class UsersController : BaseController
{
    private readonly IUserService _userService; 

    public UsersController(IUserService userService)
    {
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
