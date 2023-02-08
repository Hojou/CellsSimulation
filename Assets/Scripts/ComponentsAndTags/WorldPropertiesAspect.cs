using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;

public readonly partial struct WorldPropertiesAspect : IAspect
{
    public readonly Entity Entity;
    public readonly DynamicBuffer<CellConfigurationProperties> cellProperties;
    public readonly DynamicBuffer<CellRule> cellRules;

    private readonly RefRW<CellRandom> _cellRandom;
    private readonly RefRW<WorldProperties> _worldProperties;

    public float Strength => _worldProperties.ValueRO.Strength;
    public float Speed
    {
        get => _worldProperties.ValueRO.Speed;
        set => _worldProperties.ValueRW.Speed = value;
    }
        
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

    public NativeArray<CellConfigurationProperties> CellConfigurations
    {
        get => cellProperties.ToNativeArray(Allocator.TempJob);
    }

    public NativeArray<CellRule> CellRules => cellRules.ToNativeArray(Allocator.TempJob);

    public NativeArray<CellRule> GetRulesForConfigId(int Id)
    {
        var list = new NativeList<CellRule>(Allocator.Temp);
        foreach (var rule in cellRules)
        {
            if (rule.Id1 == Id)
            {
                list.Add(rule);
            }
        }
        return list.AsArray();
    }

    public void SetRules(int cellId, NativeArray<CellRule> rules)
    {
        //EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var brules = entityManager.GetBuffer<CellRule>(Entity);
        //brules.
        for (int i = 0; i < cellRules.Length; i++)
        {   
            var rule = cellRules[i];
            if (rule.Id1 == cellId)
            {
                foreach (var data in rules)
                {
                    if (data.Id2 != rule.Id2) continue;
                    rule.Amount = data.Amount;
                    UnityEngine.Debug.Log($"{cellId},{rule.Id2}:{rule.Amount}");
                }
            }
        }
    }
}
