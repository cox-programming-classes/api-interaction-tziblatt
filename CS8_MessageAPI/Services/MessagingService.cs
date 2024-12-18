using System.Collections.Immutable;
using CS8_MessageAPI.Models;

namespace CS8_MessageAPI.Services;

public class MessagingService(ApiService apiService)
{
    private readonly ApiService _api = apiService;

    /// <summary>
    /// Get the stubs for each message in your inbox.
    /// </summary>
    /// <param name="onError"></param>
    /// <param name="unreadOnly"></param>
    /// <param name="includeHidden"></param>
    /// <returns></returns>
    public async Task<ImmutableArray<ReceivedMessageStub>> GetInbox(ErrorAction onError,
        bool unreadOnly = true, bool includeHidden = false)
    {
        var result = await _api.SendAsync<ImmutableArray<ReceivedMessageStub>?>(
            HttpMethod.Get, $"api/messages/inbox?unreadOnly={unreadOnly}&hidden={includeHidden}", onError);

        return result ?? [];
    }

    /// <summary>
    /// Get the content of a message
    /// </summary>
    /// <param name="id">ID of the requested message</param>
    /// <param name="onError">Error Action if something goes wrong.</param>
    /// <returns>If the ID is unavailable, this will return null</returns>
    public async Task<ReceivedMessage?> GetMessageContent(string id, ErrorAction onError)
    {
        var result = await _api.SendAsync<ReceivedMessage?>(HttpMethod.Get, $"api/messages/{id}", onError);
        return result;
    }

    public async Task<SentMessage?> SendMessage(CreateMessage message, ErrorAction onError)
    {
        return null;
    }
}