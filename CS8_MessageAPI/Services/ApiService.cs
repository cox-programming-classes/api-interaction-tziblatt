using System.Net;
using System.Text;
using System.Text.Json;
using CS8_MessageAPI.Models;

namespace CS8_MessageAPI.Services;

public class ApiService
{
    public string? AuthUserId => AuthorizedUser?.userId;
    public DateTime? AuthExpires => AuthorizedUser?.expires;

    public AuthResponse? AuthorizedUser { get; private set; }
    public UserRecord? UserInfo = default(UserRecord);

    private readonly HttpClient client = new HttpClient()
    {
        BaseAddress =
            new("http://forms-dev.winsor.edu")
    };
    
    public async Task Login(string email, string password, ErrorAction onError)
    {
        Login login = new(email, password);
        try
        {
            AuthorizedUser = await SendAsync<AuthResponse>(HttpMethod.Post, "api/auth", onError,
                JsonSerializer.Serialize(login), false);
            if (AuthorizedUser is not null)
            {

                UserInfo = await SendAsync<UserRecord>(HttpMethod.Get, "api/users/self", onError: onError);

                SavedCredential.Save(email, password, AuthorizedUser.jwt, AuthorizedUser.refreshToken);
            }
        }
        catch (Exception e)
        {
            onError.Invoke(new("Login Error", e.Message));
        }
    }

    public async Task ForgotPassword(string email, string password, Action<string> onCompleteAction, ErrorAction onError)
    {
        var login = new Login(email, password);

        try
        {
            var response = await SendAsync(HttpMethod.Post, "api/auth/forgot", onError,
                JsonSerializer.Serialize(login), false);
            onCompleteAction("Please Check your Email for your new Password.");
        }
        catch (Exception ae)
        {
            onCompleteAction($"Failed: {ae.Message}");
        }
    }

    public async Task Register(string email, string password, Action<string> onCompleteAction, ErrorAction onError)
    {
        var login = new Login(email, password);

        try
        {
            var response = await SendAsync(HttpMethod.Post, "api/auth/register", onError,
                JsonSerializer.Serialize(login), false);
            onCompleteAction(response);
        }
        catch (Exception ae)
        {
            onCompleteAction($"Failed: {ae.Message}");
        }
    }

    public void Logout()
    {
        AuthorizedUser = null;
        UserInfo = null;
        SavedCredential.DeleteSavedCredential();
    }
    
    private async Task<HttpRequestMessage> BuildRequest(HttpMethod method, string endpoint, string jsonContent = "",
        bool authorize = true)
    {
        var request = new HttpRequestMessage(method, endpoint);

        if (authorize && (AuthorizedUser is null))
        {
            throw new UnauthorizedAccessException("Unable to Authorize request.  Token is missing or expired.");
        }

        if (authorize)
        {
            var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthorizedUser!.jwt);
            request.Headers.Authorization = authHeader;
        }

        if (!string.IsNullOrEmpty(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }
        
        return request;
    }

    public async Task<string> SendAsync(HttpMethod method, string endpoint, ErrorAction onError, 
        string jsonContent = "",
        bool authorize = true,
        bool isReAuth = false)
    {


        onError ??= err => { };
        var request = await BuildRequest(method, endpoint, jsonContent, authorize);

        var response = await client.SendAsync(request);
        if (await CheckReAuth(response, () => BuildRequest(method, endpoint, jsonContent, authorize).Result))
        {
            onError(new("Unauthorized Access", "Current user is not authorized to access this endpoint."));
            return "";
        }

        if (!response.IsSuccessStatusCode)
        {
            await ProcessHttpResponse(response, onError);
            return "";
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.Accepted)
        {
            
            return "";
        }
        var message = await response.Content.ReadAsStringAsync();
        return message;
    }
    
    public async Task<TOut?> SendAsync<TIn, TOut>(HttpMethod method, string endpoint, TIn content, ErrorAction onError,
        bool authorize = true)
    {
        var json = JsonSerializer.Serialize(content);
        return await SendAsync<TOut>(method, endpoint, onError, json, authorize);
    }

    public async Task<T?> SendAsync<T>(HttpMethod method, string endpoint, ErrorAction onError, string jsonContent = "",
        bool authorize = true, bool isReAuth = false)
    {
        var request = await BuildRequest(method, endpoint, jsonContent, authorize);
        var response = await client.SendAsync(request);
        if (endpoint!="api/auth" && await CheckReAuth(response, () => BuildRequest(method, endpoint, jsonContent, authorize).Result))
        {
            onError(new("Unauthorized Access", "Current user is not authorized to access this endpoint."));
            var result = await response.Content.ReadAsStringAsync();
           
            return default;
        }
        
        return await ProcessHttpResponse<T>(response, onError);
    }

    private async Task<T?> ProcessHttpResponse<T>(HttpResponseMessage response, ErrorAction onError, T? defaultOutput = default)
    {
        var jsonContent = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = JsonSerializer.Deserialize<T>(jsonContent);
                return result;
            }
            catch (Exception e)
            {
                onError(new("Failed To Deserialize Result", e.Message));
                return defaultOutput;
            }
        }

        try
        {
            var err = JsonSerializer.Deserialize<ErrorRecord>(jsonContent);
            onError(err);
        }
        catch (Exception e)
        {
            onError(new($"{response.ReasonPhrase}", jsonContent));
        }
        
        return defaultOutput;
    }

    /// <summary>
    /// Returns TRUE if renewing the Token fails to get passed Unauthorized.
    /// </summary>
    /// <param name="response"></param>
    /// <param name="requestBuilder"></param>
    /// <returns></returns>
    public async Task<bool> CheckReAuth(HttpResponseMessage response, Func<HttpRequestMessage> requestBuilder)
    {
        for (var i = 0; i < 5; i++)
        {
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return false;
            
            await RenewTokenAsync(err => { });
            response = await client.SendAsync(requestBuilder());
        }

        return true;
    }
    
    public async Task RenewTokenAsync(ErrorAction onError, bool repeat = false)
    {
        
        try
        {
            if (AuthorizedUser is not null)
            {
                var jwt = AuthorizedUser.jwt;
                var failed = false;
                AuthorizedUser = await SendAsync<AuthResponse>(HttpMethod.Get,
                    $"api/auth/renew?refreshToken={AuthorizedUser.refreshToken}", authorize: true, onError: err =>
                    {
                        failed = true;
                    });

                if (failed)
                {
                    var savedCred = SavedCredential.GetSavedCredential();
                    if(savedCred is not null && !string.IsNullOrWhiteSpace(savedCred.SavedPassword))
                    {
                        failed = false;
                        await Login(savedCred.SavedEmail, savedCred.SavedPassword, onError: err =>
                        {
                            failed = true;
                        });
                    }
                }

                if (!failed && AuthorizedUser is not null)
                {
                    SavedCredential.SaveJwt(AuthorizedUser.jwt, AuthorizedUser.refreshToken);
                }
            }
            else
            {
                var savedCred = SavedCredential.GetSavedCredential();
                if (savedCred is null)
                    throw new InvalidOperationException("Cannot renew a token without logging in first.");

                if (!repeat && !string.IsNullOrEmpty(savedCred.Jwt) && !string.IsNullOrEmpty(savedCred.RefreshToken))
                {
                    AuthorizedUser = new(jwt: savedCred.Jwt, refreshToken: savedCred.RefreshToken);
                }
                if (!string.IsNullOrWhiteSpace(savedCred.SavedPassword))
                    await Login(savedCred.SavedEmail, savedCred.SavedPassword, onError);
                else
                {
                    //Refreshing = false;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            //ex?.LogException(_logging);
        }


        if (AuthorizedUser is not null)
        {
            if (UserInfo is null)
                UserInfo = await SendAsync<UserRecord>(HttpMethod.Get, "api/users/self", onError);
            
        }
    }
    
    private async Task<bool> ProcessHttpResponse(HttpResponseMessage response, ErrorAction onError)
    {
        if (response.IsSuccessStatusCode) 
            return true;
        var jsonContent = await response.Content.ReadAsStringAsync();
        
        try
        {
            var err = JsonSerializer.Deserialize<ErrorRecord>(jsonContent);
            onError(err);
        }
        catch (Exception e)
        {
            onError(new($"{response.ReasonPhrase}", jsonContent));
        }

        return false;
    }
}