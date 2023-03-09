using Unity.Collections;
using Unity.Entities;

public struct CellConfigurationProperties : IBufferElementData
{
    public int Id;
    public FixedString32Bytes Name;
    public int NumberOfCells;
    public Entity Prefab;
}
