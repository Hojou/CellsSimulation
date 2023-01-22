using JetBrains.Annotations;
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
    public NativeArray<RuleAmount> Rules;
}

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

        foreach (var property in buffer) {
            var rules = aspect.cellRules
                    .Where(r => r.Id1 == property.Id)
                    .Select(r => new RuleAmount { Id = r.Id2, Amount = r.Amount })
                    .OrderBy(t => t.Id).ToArray();
            var nrules = new NativeArray<RuleAmount>(rules, Allocator.Persistent);

            for (int i = 0; i < property.NumberOfCells; i++)
            {
                var cell = ecb.Instantiate(property.Prefab);
                ecb.SetComponent(cell, new LocalTransform
                {
                    Position = aspect.GetRandomPosition(),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                ecb.AddComponent(cell, new CellProperties {  
                    Id = property.Id,
                    Velocity = new float3(0, 0, 0),
                });

                ecb.AddSharedComponent(cell, new SharedRulesGrouping
                {
                    Group = property.Id,
                    Rules = nrules
                });

                ecb.AddBuffer<VelocityChange>(cell);
            }
        }

        //buffer.Clear();
        ecb.Playback(state.EntityManager);
    } 
}
