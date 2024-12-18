namespace CS8_MessageAPI.Models;
/// <summary>
/// academic block 
/// </summary>
/// <param name="blockId"></param>
/// <param name="name"></param>
/// <param name="schoolLevel"></param>
public record Block(
    string blockId,
    string name,
    string schoolLevel);
/// <summary>
/// collection of free blocks
/// </summary>
/// <param name="block"></param>
/// <param name="start"></param>
/// <param name="end"></param>
public record FreeBlock(
    Block block,
    DateTime start,
    DateTime end);

public record FreeBlockCollection(
    FreeBlock[] freeBlocks,
    DateOnly inRange);

    