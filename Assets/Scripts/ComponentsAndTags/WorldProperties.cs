using Unity.Entities;
using Unity.Mathematics;

public struct CellRule: IBufferElementData
{
    public int Id1;
    public int Id2;
    public float Amount;
}

public struct CellConfigurationProperties: IBufferElementData
{
    public int Id;
    public int NumberOfCells;
    public Entity Prefab;
}

public struct WorldProperties : IComponentData
{
    public float2 Dimension;
    public float Speed;
}


public struct CellRandom : IComponentData
{
    public Random Value;
}
