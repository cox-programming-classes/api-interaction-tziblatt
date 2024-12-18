using System.Text;
using System.Text.Json;

namespace CS8_MessageAPI.Models;

public record SavedCredential(
        string SavedEmail = "",
        string SavedPassword = "",
        string Jwt = "",
        string RefreshToken = "")
{
    private static string CredFilePath => $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}{Path.DirectorySeparatorChar}.login.cred";
    
    public static void DeleteSavedCredential()
    {
        if (File.Exists(CredFilePath))
            File.Delete(CredFilePath);
    }

    public static void SaveJwt(string jwt, string refreshToken)
    {
        var saved = GetSavedCredential();
        if (saved is null)
            saved = new();

        saved = saved with { Jwt = jwt, RefreshToken = refreshToken };

        WriteFileData(saved);
    }
        

    public static void Save(string email, string password, string jwt = "", string refreshToken = "") =>
         WriteFileData(new SavedCredential(email, password, jwt, refreshToken));

    public static void WriteFileData(SavedCredential credential)
    {
        var json = JsonSerializer.Serialize(credential);

        File.WriteAllText(CredFilePath, json);
    }

    public static SavedCredential? GetSavedCredential()
    {
        if (!File.Exists(CredFilePath))
            return null;

   
        var json = File.ReadAllText(CredFilePath).Trim();

        if (!json.StartsWith('{') || !json.EndsWith('}'))
        {
            DeleteSavedCredential();
            return null;
        }

        var credential = JsonSerializer.Deserialize<SavedCredential>(json);

        return credential;
    }
}