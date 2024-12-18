using System.Text.Json;

namespace CS8_MessageAPI.Models;

/// <summary>
/// Login Record
/// </summary>
/// <param name="email">Your Email</param>
/// <param name="password">Your Password</param>
public readonly record struct Login(string email, string password);

/// <summary>
/// Response to a successful Login request
/// </summary>
/// <param name="userId">Your userId</param>
/// <param name="jwt">your JWT used for authentication</param>
/// <param name="refreshToken">refresh token used for renewing your JWT</param>
/// <param name="expires">When your JWT expires</param>
public record AuthResponse(string userId = "", string jwt = "", string refreshToken = "", DateTime expires = default);

/// <summary>
/// Response when an error has occurred.
/// </summary>
/// <param name="type">What kind of error</param>
/// <param name="error">Error details</param>
public readonly record struct ErrorRecord(string type, string error);