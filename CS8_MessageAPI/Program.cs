global using ErrorAction = System.Action<CS8_MessageAPI.Models.ErrorRecord>;
using CS8_MessageAPI.Models;
using CS8_MessageAPI.Services;

var apiService = new ApiService();

var loginSuccess = true;

await apiService.Login("talia.ziblatt@winsor.edu", "%*!TWI047qdu",
    err =>
    {
        Console.WriteLine(err);
        loginSuccess = false;
    });
    
if(!loginSuccess)
    return;
    
Console.WriteLine($"jwt: {apiService.AuthorizedUser?.jwt}");

var myFreeBlock = await apiService.SendAsync<FreeBlockCollection>(
    HttpMethod.Get, ""
err =>
{
    Console.WriteLine(err); 
    
});

   