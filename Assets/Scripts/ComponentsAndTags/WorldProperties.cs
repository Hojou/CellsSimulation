using Unity.Entities;
using Unity.Mathematics;

public struct WorldProperties : IComponentData
{
    public float2 Dimension;
    public float Speed;
    public float Strength;
    public float Scale;
    public float Influence;
}
