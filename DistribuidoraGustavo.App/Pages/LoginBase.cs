using Microsoft.AspNetCore.Components;

namespace DistribuidoraGustavo.App.Pages;

public class LoginBase : ComponentBase
{
    protected bool requestSent = false;

    protected Credentials credentials = new();

    protected async Task Login()
    { 
        requestSent = true;
    }
}

public class Credentials
{ 
    public string? Username { get; set; }
    public string? Password { get; set; }
}
