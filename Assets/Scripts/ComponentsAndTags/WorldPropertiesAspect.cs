using TMPro;
using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct WorldPropertiesAspect : IAspect
{
    public readonly Entity Entity;
    public readonly DynamicBuffer<CellConfigurationProperties> cellProperties;
    public readonly DynamicBuffer<CellRule> cellRules;

    private readonly RefRW<CellRandom> _cellRandom;
    private readonly RefRO<WorldProperties> _worldProperties;

    public float Speed => _worldProperties.ValueRO.Speed;
    public float Scale => _worldProperties.ValueRO.Scale;
    public float2 Dimensions => _worldProperties.ValueRO.Dimension;


    public float3 GetRandomPosition()
    {
        float dimensionX = _worldProperties.ValueRO.Dimension.x / 2f;
        float dimensionY = _worldProperties.ValueRO.Dimension.y / 2f;
        float x = _cellRandom.ValueRW.Value.NextFloat(-dimensionX, dimensionX);
        float z = _cellRandom.ValueRW.Value.NextFloat(-dimensionY, dimensionY);

        return new float3(x, 0, z);
    }
}
