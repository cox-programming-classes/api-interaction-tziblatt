using System.Collections.Immutable;

namespace CS8_MessageAPI.Models;

public readonly record struct SentMessage(
    string id, 
    string sender, 
    byte[] messageContent, 
    string contentType,
    DateTime sent,
    ImmutableArray<string> recipients);
    
public readonly record struct SentMessageStub(
    string id,
    string sender,
    string contentType,
    int contentLength,
    DateTime sent,
    ImmutableArray<string> recipients);
    
public readonly record struct ReceivedMessage(
    string id,
    string sender,
    byte[] messageContent,
    string contentType,
    DateTime sent,
    string recipient,
    DateTime read,
    bool hidden);
    
public readonly record struct ReceivedMessageStub(
    string id,
    string sender,
    string contentType,
    int contentLength,
    DateTime sent,
    bool unread,
    bool hidden);
    
public readonly record struct CreateMessage(
    byte[] messageContent, 
    string contentType,
    ImmutableArray<string> recipients,
    DateTime? selfDestruct = null);