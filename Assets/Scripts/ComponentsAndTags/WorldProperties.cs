using Unity.Collections;
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
    public FixedString32Bytes Name;
    public int NumberOfCells;
    public Entity Prefab;
}

public struct WorldProperties : IComponentData
{
    public float2 Dimension;
    public float Speed;
    public float Strength;
    public float Scale;
    public NativeArray<float> Rules; // Id1*32+Id2
}

public struct CellRandom : IComponentData
{
    public Random Value;
}

public struct CellRuleFor : IComponentData
{
    public int Id;
    public float Amount;
}
