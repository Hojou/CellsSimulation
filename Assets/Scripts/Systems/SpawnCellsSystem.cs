using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct SharedRulesGrouping : ISharedComponentData
{
    public int Group;
    public NativeHashMap<int, float> Rules;
}

//public struct SharedRulesForGrouping : ISharedComponentData
//{
//    public int GroupFor;
//    public float Amount;
//}

[Serializable]
public struct RuleAmount { public int Id; public float Amount; }

[BurstCompile]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct SpawnCellsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<WorldProperties>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var properties = SystemAPI.GetSingletonEntity<WorldProperties>();
        var aspect = SystemAPI.GetAspectRW<WorldPropertiesAspect>(properties);
        var buffer = SystemAPI.GetBuffer<CellConfigurationProperties>(properties);
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var property in buffer)
        {
            SharedRulesGrouping rulesComponent = CreateRulesComponent(ref aspect, property);

            for (int i = 0; i < property.NumberOfCells; i++)
            {
                var cell = ecb.Instantiate(property.Prefab);
                ecb.SetComponent(cell, new LocalTransform
                {
                    Position = aspect.GetRandomPosition(),
                    Rotation = quaternion.identity,
                    Scale = aspect.Scale
                });

                ecb.AddComponent(cell, new CellProperties
                {
                    Id = property.Id,
                    Velocity = new float3(0, 0, 0),
                });

                ecb.AddSharedComponent(cell, rulesComponent);

                ecb.AddBuffer<VelocityChange>(cell);
            }
        }

        //var floorEntity = SystemAPI.GetSingletonEntity<FloorTag>();
        //var floor = SystemAPI.GetAspectRW<TransformAspect>(floorEntity);
        //floor.LocalTransform.

        //buffer.Clear();
        ecb.Playback(state.EntityManager);
    }

    private static SharedRulesGrouping CreateRulesComponent(ref WorldPropertiesAspect aspect, CellConfigurationProperties property)
    {
        var rules = new NativeHashMap<int, float>(aspect.cellRules.Length, Allocator.Persistent);
        foreach (var rule in aspect.cellRules)
        {
            if (rule.Id1 != property.Id) continue;
            rules.Add(rule.Id2, rule.Amount);
        }
        var rulesComponent = new SharedRulesGrouping
        {
            Group = property.Id,
            Rules = rules
        };
        return rulesComponent;
    }
}
