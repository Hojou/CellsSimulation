using Unity.Entities;
using Unity.Mathematics;

public struct CellProperties : IComponentData
{
    public float3 Velocity;
}

public struct VelocityChange : IBufferElementData
{
    public float3 Value;
}
