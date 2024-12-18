global using ErrorAction = System.Action<CS8_MessageAPI.Models.ErrorRecord>;
using CS8_MessageAPI.Services;

var apiService = new ApiService();

var loginSuccess = true;

await apiService.Login("jcox@winsor.edu", "not my password",
    err =>
    {
        Console.WriteLine(err);
        loginSuccess = false;
    });
    
if(!loginSuccess)
    return;
    
Console.WriteLine($"jwt: {apiService.AuthorizedUser?.jwt}");

var b64String = Convert.FromBase64String(apiService.AuthorizedUser?.jwt ?? "");

Console.WriteLine(b64String);