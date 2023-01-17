using Unity.Entities;
using Unity.Mathematics;

public readonly partial struct WorldPropertiesAspect : IAspect
{
    public readonly Entity Entity;
    public readonly DynamicBuffer<CellConfigurationProperties> cellProperties;

    private readonly RefRW<CellRandom> _cellRandom;
    private readonly RefRO<WorldProperties> _worldProperties;

    public float3 GetRandomPosition()
    {
        float dimensionX = _worldProperties.ValueRO.Dimension.x / 2f;
        float dimensionY = _worldProperties.ValueRO.Dimension.y / 2f;
        float x = _cellRandom.ValueRW.Value.NextFloat(-dimensionX, dimensionX);
        float z = _cellRandom.ValueRW.Value.NextFloat(-dimensionY, dimensionY);

        return new float3(x, 0, z);
    }
}
