using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct CellPropertiesAspect : IAspect
{
    private readonly RefRW<CellProperties> cellProperties;
    private readonly TransformAspect transform;
    
    public float3 LocalPosition
    {
        get => transform.LocalPosition;
        set => transform.LocalPosition = value;
    } 
    public float3 Velocity
    {
        get => cellProperties.ValueRO.Velocity;
        set => cellProperties.ValueRW.Velocity = value;
    }
}
